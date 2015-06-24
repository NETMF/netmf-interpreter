////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "USB.h"

#define USB_FLUSH_RETRY_COUNT 1000

//--//

#if defined(BUILD_RTM)
void USB_debug_printf( const char*format, ... ) {}
#else
void USB_debug_printf( const char*format, ... )
{
    char    buffer[256];
    va_list arg_ptr;

    va_start( arg_ptr, format );

    int len = hal_vsnprintf( buffer, sizeof(buffer)-1, format, arg_ptr );
                               
    // flush existing characters
    DebuggerPort_Flush( USART_DEFAULT_PORT );

    // write string
    DebuggerPort_Write( USART_DEFAULT_PORT, buffer, len, 0 );

    // flush new characters
    DebuggerPort_Flush( USART_DEFAULT_PORT );

    va_end( arg_ptr );
}
#endif

//--//

// This version of the USB code supports only one language - which
// is not specified by USB configuration records - it is defined here.
// This is the String 0 descriptor.This array includes the String descriptor
// header and exactly one language.

#define USB_LANGUAGE_DESCRIPTOR_SIZE 4

UINT8 USB_LanguageDescriptor[USB_LANGUAGE_DESCRIPTOR_SIZE] =
{
    USB_LANGUAGE_DESCRIPTOR_SIZE,
    USB_STRING_DESCRIPTOR_TYPE,
    0x09, 0x04                      // U.S. English
};

// This provides storage for the "friendly name" (String 5) if it is specified
// by the Flash configuration sector
ADS_PACKED struct GNU_PACKED 
{
    UINT8 bLength;
    UINT8 bDescriptorType;
    UINT8 sFriendlyName[USB_FRIENDLY_NAME_LENGTH * sizeof(USB_STRING_CHAR)];
    static LPCSTR GetDriverName() { return "USB_NAME_CONFIG"; }
}FriendlyNameString;

USB_SETUP_PACKET RequestPacket = { 0, 0, 0, 0, 0 };

//--//

int USB_GetControllerCount()
{
    return USB_Driver::GetControllerCount();
}

BOOL USB_Initialize( int Controller )
{
    return USB_Driver::Initialize( Controller );
}

int USB_Configure( int Controller, const USB_DYNAMIC_CONFIGURATION *config )
{
    return USB_Driver::Configure( Controller, config );
}

const USB_DYNAMIC_CONFIGURATION * USB_GetConfiguration( int Controller )
{
    return USB_Driver::GetConfiguration( Controller );
}

BOOL USB_Uninitialize( int Controller )
{
    return USB_Driver::Uninitialize( Controller );
}

BOOL USB_OpenStream( int UsbStream, int writeEP, int readEP )
{
    return USB_Driver::OpenStream( UsbStream, writeEP, readEP );
}

BOOL USB_CloseStream( int UsbStream )
{
    return USB_Driver::CloseStream( UsbStream );
}

int USB_Write( int UsbStream, const char* Data, size_t size )
{
    return USB_Driver::Write( UsbStream, Data, size );
}

int USB_Read( int UsbStream, char* Data, size_t size )
{
    return USB_Driver::Read( UsbStream, Data, size );
}

BOOL USB_Flush( int UsbStream )
{
    return USB_Driver::Flush(UsbStream);
}

UINT32 USB_GetEvent( int Controller, UINT32 Mask )
{
    return USB_Driver::GetEvent( Controller, Mask );
}

UINT32 USB_SetEvent( int Controller, UINT32 Event )
{
    return USB_Driver::SetEvent( Controller, Event );
}

UINT32 USB_ClearEvent( int Controller, UINT32 Event )
{
    return USB_Driver::ClearEvent( Controller, Event );
}

UINT8 USB_GetStatus( int Controller )
{
    return USB_Driver::GetStatus( Controller );
}

void USB_DiscardData( int UsbStream, BOOL fTx )
{
    USB_Driver::DiscardData(UsbStream, fTx);
}

//--//

int USB_Driver::GetControllerCount()
{
    return TOTAL_USB_CONTROLLER;
}

BOOL USB_Driver::Initialize( int Controller )
{
    NATIVE_PROFILE_PAL_COM();

    char szFriendlyName[USB_FRIENDLY_NAME_LENGTH];

    USB_CONTROLLER_STATE *State = CPU_USB_GetState( Controller );

    GLOBAL_LOCK(irq);

    USB_CONFIGURATION_DESCRIPTOR * Config;

    if(State == NULL) return FALSE;

#if defined(USB_ALLOW_CONFIGURATION_OVERRIDE)
    //
    // If initialized, uninitilize the current usb stack so that we can override it with a new configuration
    //
    if(State->Initialized && COM_IsUsb(HalSystemConfig.DebuggerPorts[0]) && Controller == ConvertCOM_UsbController(HalSystemConfig.DebuggerPorts[0]))
    {
        USB_Driver::Uninitialize( Controller );
    }
#endif

    if(State->Configuration == NULL)
    {
        USB_Driver::Configure( Controller, NULL );
    }

    // Check if friendly name has been changed
    if(HAL_CONFIG_BLOCK::ApplyConfig( FriendlyNameString.GetDriverName(), (void*)&szFriendlyName[0], sizeof(szFriendlyName) ))
    {
        int length = 0;

        // Find the real length of the string & expand into String 5 response record
        for(length = 0; length < USB_FRIENDLY_NAME_LENGTH; length++ )
        {
            // Expand Friendly Name to wchar type in string response buffer
            FriendlyNameString.sFriendlyName[length * sizeof(USB_STRING_CHAR)] = szFriendlyName[length];
            FriendlyNameString.sFriendlyName[length * sizeof(USB_STRING_CHAR) + 1] = 0;     // Roughly convert to wchar_t

            if( szFriendlyName[length] == 0 )
                break;
        }

        // Finish filling out the String descriptor response record
        FriendlyNameString.bLength = (length * sizeof(USB_STRING_CHAR)) + USB_STRING_DESCRIPTOR_HEADER_LENGTH;
        FriendlyNameString.bDescriptorType = USB_STRING_DESCRIPTOR_TYPE;
    }
    else
    {
        // Indicate that "String 5" (if it exists) comes from the configuration
        FriendlyNameString.bLength = 0;
    }
    

    Config = (USB_CONFIGURATION_DESCRIPTOR *)USB_FindRecord( State, USB_CONFIGURATION_DESCRIPTOR_MARKER, 0 );

    /* now we actually initialize everything */
    if(State->Initialized == FALSE && Config != NULL)
    {
        // Remember only the USB configuration information
        const USB_DYNAMIC_CONFIGURATION *Save = State->Configuration;
        
        // Wipe the Controller state clean before passing to the hardware driver
        memset( State, 0, sizeof(USB_CONTROLLER_STATE) );

        State->Configuration = Save;        // Restore the configuration information

        // Set all streams to unused
        for( int stream = 0; stream < USB_MAX_QUEUES; stream++ )
        {
            State->streams[stream].RxEP = USB_NULL_ENDPOINT;
            State->streams[stream].TxEP = USB_NULL_ENDPOINT;
        }

        State->ControllerNum = Controller;
        State->CurrentState  = USB_DEVICE_STATE_UNINITIALIZED;


        // at configuration descriptor, BUS powered, bmAttribute(bit 6 = 0 and MaxPower != 0)
        // but the Status report is reversed
        
        if(Config->bmAttributes & USB_ATTRIBUTE_SELF_POWER)
        {
            State->DeviceStatus |= USB_STATUS_DEVICE_SELF_POWERED;
        }
        else
        {
            State->DeviceStatus &= ~USB_STATUS_DEVICE_SELF_POWERED;
        }

        // The Remote Wake Up status feature is disabled by default
        // It is currently not necessary since at this time there are
        // no host-side drivers that place the device in a SUSPEND state
    #if defined(USB_REMOTE_WAKEUP)
        if((Config->bmAttributes & USB_ATTRIBUTE_REMOTE_WAKEUP)
        {
            State->DeviceStatus |= USB_STATUS_DEVICE_REMOTE_WAKEUP;
        }
        else
        {
            State->DeviceStatus &= ~USB_STATUS_DEVICE_REMOTE_WAKEUP;
        }
    #endif

        if( S_OK != CPU_USB_Initialize( Controller ) )
        {
            return FALSE;       // If Hardware initialization fails
        }

#if defined(USB_ALLOW_CONFIGURATION_OVERRIDE)
        //
        // Re-initialize the Debugger stream
        //
        if(COM_IsUsb(HalSystemConfig.DebuggerPorts[0]) && Controller == ConvertCOM_UsbController(HalSystemConfig.DebuggerPorts[0]))
        {
            USB_OpenStream( ConvertCOM_UsbStream(HalSystemConfig.DebuggerPorts[0]), USB_DEBUG_EP_WRITE, USB_DEBUG_EP_READ );    
        }
#endif

        State->Initialized   = TRUE;

        return TRUE;
    }
    
    return FALSE;
}

int USB_Driver::Configure( int Controller, const USB_DYNAMIC_CONFIGURATION* Config )
{
    NATIVE_PROFILE_PAL_COM();
    if( Controller > 9 )
        return USB_CONFIG_ERR_NO_CONTROLLER;

    int err;
    size_t Length = 0;
    USB_DESCRIPTOR_HEADER *configHeader; 
    char configName[5] = { 'U', 'S', 'B', '1' + (char)Controller, 0 }; 
    USB_CONTROLLER_STATE* State = CPU_USB_GetState( Controller );
    
    // Check if controller exists
    if( NULL == State )
        return USB_CONFIG_ERR_NO_CONTROLLER;

    // Cannot alter the configuration while the Controller is running
#if !defined(USB_ALLOW_CONFIGURATION_OVERRIDE)
    if( State->Initialized )
        return USB_CONFIG_ERR_STARTED;
#endif

    // If the default configuration is to be used
    if( Config == NULL )
    {
        Config = &UsbDefaultConfiguration;
    }
    
    // Check the configuration to be sure that there are no glaring errors
    err = UsbConfigurationCheck( Config );

    if( err != USB_CONFIG_ERR_OK )
        return err;

    // only save non default configurations
    if(Config != &UsbDefaultConfiguration)
    {
        configHeader = (USB_DESCRIPTOR_HEADER *)Config;

        // Calculate the size of the default USB configuration
        while( configHeader->marker != USB_END_DESCRIPTOR_MARKER )
        {
            Length += configHeader->size;
            configHeader = configHeader->next(configHeader);
        }
        Length += sizeof(USB_DESCRIPTOR_HEADER);    // Don't forget to include the ending header

        // Write the default USB configuration to the Flash config sector
        HAL_CONFIG_BLOCK::UpdateBlockWithName(configName, (void *)Config, Length, TRUE);


        //
        // Free the old configuration (UsbDefaultConfiguration is a global variable so do not free it)
        //
        if(State->Configuration != &UsbDefaultConfiguration)
        {
            private_free((void*)State->Configuration);
        }

        //
        // Make sure that we allocate the native configuration buffer, the one passed in will be garbage collected
        //
        State->Configuration = (USB_DYNAMIC_CONFIGURATION*)private_malloc(Length);

        //

        // Copy the configuration from the temporary config (from the caller)
        //
        memcpy((void*)State->Configuration, (void*)Config, Length);
    }
    else
    {
        // Set the configuration for this Controller
        State->Configuration = Config;
    }
    
    return err;
}

//
// The GetConfiguration method will attempt to load the configurate from the config sector in flash
// Since the USB configuration is of variable size, this method may use private_malloc, therefore,
// this method should not be called prior to the heap initialization.
//
const USB_DYNAMIC_CONFIGURATION * USB_Driver::GetConfiguration( int Controller )
{
    NATIVE_PROFILE_PAL_COM();

    USB_CONTROLLER_STATE *State = CPU_USB_GetState( Controller );
    char configName[5] = { 'U', 'S', 'B', '1' + Controller, 0 }; 

    // Check if controller exists
    if(NULL == State)
        return NULL;

    // return immediately if we already have a configuration (other than the default)
    if(State->Configuration != NULL && State->Configuration != &UsbDefaultConfiguration)
    {
        if(USB_CONFIG_ERR_OK == UsbConfigurationCheck( State->Configuration ))
        {
            return State->Configuration;
        }
    }

    // It is assumed that the Flash config sector will only change while in TinyBooter, and so if the
    // configuration is in the Flash config sector, it will always be OK since TinyBooter always
    // uses the configuration in initialized RAM.
    
    // If the requested USB configuration was not found in the Flash configuration sector
    void *pConfig = NULL;

    // this calls private_malloc
    if(HAL_CONFIG_BLOCK::ApplyConfig( configName, NULL, 0, (void**)&pConfig ) && pConfig != NULL)
    {
        State->Configuration = (const USB_DYNAMIC_CONFIGURATION *)pConfig;

        // Check the configuration to be sure that there are no glaring errors
        if(USB_CONFIG_ERR_OK != UsbConfigurationCheck( State->Configuration ))
        {
            State->Configuration = &UsbDefaultConfiguration;
        }
    }
    else
    {
        // Use the default USB configuration
        State->Configuration = &UsbDefaultConfiguration;    
    }

    return State->Configuration;
}
    
BOOL USB_Driver::Uninitialize( int Controller )
{
    NATIVE_PROFILE_PAL_COM();

    USB_CONTROLLER_STATE *State = CPU_USB_GetState( Controller );
    
    if(NULL == State) return FALSE;
    
    GLOBAL_LOCK(irq);

    if(State->Initialized == FALSE) return FALSE;

    // All streams must be closed on the Controller, or it may not be uninitialized (stopped)
    for(int stream = 0; stream < USB_MAX_QUEUES; stream++)
    {
#if defined(USB_ALLOW_CONFIGURATION_OVERRIDE)
        //
        // Ignore the debugger stream as we will only close it if we need to
        //
        if((TRUE       == COM_IsUsb               (HalSystemConfig.DebuggerPorts[0])) &&
           (Controller == ConvertCOM_UsbController(HalSystemConfig.DebuggerPorts[0])) &&
           (stream     == ConvertCOM_UsbStream    (HalSystemConfig.DebuggerPorts[0])))
        {
            continue;
        }
#endif            

        if(State->streams[stream].RxEP != USB_NULL_ENDPOINT || State->streams[stream].TxEP != USB_NULL_ENDPOINT)
        {
            return FALSE;
        }
    }

    //
    // If we have gotten this far then all other streams are closed so close the debugger streams (if applicable)
    //
#if defined(USB_ALLOW_CONFIGURATION_OVERRIDE)
    if(COM_IsUsb(HalSystemConfig.DebuggerPorts[0]) && Controller == ConvertCOM_UsbController(HalSystemConfig.DebuggerPorts[0]))
    {
        USB_CloseStream(ConvertCOM_UsbStream(HalSystemConfig.DebuggerPorts[0]));
    }
#endif

    // Stop all activity on the specified Controller and make it appear disconnected from the host
    State->Initialized = FALSE;

    CPU_USB_Uninitialize( Controller );

    // for soft reboot allow the USB to be off for at least 100ms
    HAL_Time_Sleep_MicroSeconds(100000); // 100ms

    return TRUE;
}

BOOL USB_Driver::OpenStream( int UsbStream, int writeEP, int readEP )
{
    NATIVE_PROFILE_PAL_COM();

    int Controller  = ConvertCOM_UsbController ( UsbStream );
    int StreamIndex = ConvertCOM_UsbStreamIndex( UsbStream );

    USB_CONTROLLER_STATE * State = CPU_USB_GetState( Controller );

    if( NULL == State || !State->Initialized )     // If no such controller exists (or it is not initialized)
        return FALSE;

    // Check the StreamIndex and the two endpoint numbers for validity (both endpoints cannot be zero)
    if( StreamIndex >= USB_MAX_QUEUES
        || (readEP == USB_NULL_ENDPOINT && writeEP == USB_NULL_ENDPOINT)
        || (readEP != USB_NULL_ENDPOINT && (readEP < 1 || readEP >= USB_MAX_QUEUES))
        || (writeEP != USB_NULL_ENDPOINT && (writeEP < 1 || writeEP >= USB_MAX_QUEUES)) )
        return FALSE;

    // The Stream must be currently closed
    if( State->streams[StreamIndex].RxEP != USB_NULL_ENDPOINT || State->streams[StreamIndex].TxEP != USB_NULL_ENDPOINT )
        return FALSE;

    // The requested endpoints must have been configured
    if( (readEP != USB_NULL_ENDPOINT && State->Queues[readEP] == NULL) || (writeEP != USB_NULL_ENDPOINT && State->Queues[writeEP] == NULL) )
        return FALSE;

    // The requested endpoints can only be used in the direction specified by the configuration
    if( (readEP != USB_NULL_ENDPOINT && State->IsTxQueue[readEP]) || (writeEP != USB_NULL_ENDPOINT && !State->IsTxQueue[writeEP]) )
        return FALSE;

    // The specified endpoints must not be in use by another stream
    for( int stream = 0; stream < USB_MAX_QUEUES; stream++ )
    {
        if( readEP != USB_NULL_ENDPOINT && (State->streams[stream].RxEP == readEP || State->streams[stream].TxEP == readEP) )
            return FALSE;
        if( writeEP != USB_NULL_ENDPOINT && (State->streams[stream].RxEP == writeEP || State->streams[stream].TxEP == writeEP) )
            return FALSE;
    }

    // All tests pass, assign the endpoints to the stream
    {
        GLOBAL_LOCK(irq);

        State->streams[StreamIndex].RxEP = readEP;
        State->streams[StreamIndex].TxEP = writeEP;
    }

    return TRUE;
}

BOOL USB_Driver::CloseStream ( int UsbStream )
{
    NATIVE_PROFILE_PAL_COM();

    int Controller  = ConvertCOM_UsbController ( UsbStream );
    int StreamIndex = ConvertCOM_UsbStreamIndex( UsbStream );

    USB_CONTROLLER_STATE * State = CPU_USB_GetState( Controller );

    if( NULL == State || !State->Initialized )
        return FALSE;
    
    if( StreamIndex >= USB_MAX_QUEUES )
        return FALSE;

    {
        int endpoint;
        GLOBAL_LOCK(irq);

        // Close the Rx stream
        endpoint = State->streams[StreamIndex].RxEP;
        if( endpoint != USB_NULL_ENDPOINT && State->Queues[endpoint] )
        {
            State->Queues[endpoint]->Initialize();      // Clear the queue
        }
        State->streams[StreamIndex].RxEP = USB_NULL_ENDPOINT;

        // Close the TX stream
        endpoint = State->streams[StreamIndex].TxEP;
        if( endpoint != USB_NULL_ENDPOINT && State->Queues[endpoint] )
        {
            State->Queues[endpoint]->Initialize();      // Clear the queue
        }
        State->streams[StreamIndex].TxEP = USB_NULL_ENDPOINT;
    }
    
    return TRUE;
}

int USB_Driver::Write( int UsbStream, const char* Data, size_t size )
{
    NATIVE_PROFILE_PAL_COM();

    int Controller  = ConvertCOM_UsbController ( UsbStream );
    int StreamIndex = ConvertCOM_UsbStreamIndex( UsbStream );
    int endpoint;
    int totWrite = 0;
    USB_CONTROLLER_STATE * State = CPU_USB_GetState( Controller );

    if( NULL == State || StreamIndex >= USB_MAX_QUEUES )
    {
        return -1;
    }

    if(size == 0   ) return 0;
    if(Data == NULL)
    {
        return -1;
    }

    // If the Controller is not yet initialized
    if(State->DeviceState != USB_DEVICE_STATE_CONFIGURED)
    {
        // No data can be sent until the controller is initialized
        return -1;
    }
    
    endpoint = State->streams[StreamIndex].TxEP;
    // If no Write side to stream (or if not yet open)
    if( endpoint == USB_NULL_ENDPOINT || State->Queues[endpoint] == NULL )
    {
        return -1;
    }
    else
    {
        GLOBAL_LOCK(irq);

        const char*   ptr         = Data;
        UINT32        count       = size;
        BOOL          Done        = FALSE;
        UINT32        WaitLoopCnt = 0;

        // This loop packetizes the data and sends it out.  All packets sent have
        // the maximum size for the given endpoint except for the last packet which
        // will always have less than the maximum size - even if the packet length
        // must be zero for this to occur.   This is done to comply with standard
        // USB bulk-mode transfers.
        while(!Done)
        {

            USB_PACKET64* Packet64 = State->Queues[endpoint]->Push();

            if(Packet64)
            {
                UINT32 max_move;

                if(count > State->MaxPacketSize[endpoint])
                    max_move = State->MaxPacketSize[endpoint];
                else
                    max_move = count;

                if(max_move)
                {
                    memcpy( Packet64->Buffer, ptr, max_move );
                }

                // we are done when we send a non-full length packet
                if(max_move < State->MaxPacketSize[endpoint])
                {
                    Done = TRUE;
                }

                Packet64->Size  = max_move;
                count          -= max_move;
                ptr            += max_move;

                totWrite       += max_move;

                WaitLoopCnt = 0;
            }
            else
            {
                // a 64-byte USB packet takes less than 50uSec
                // according to the timing calculations of the USB Chief
                // this is way too short to bother with a call
                // to WaitForEventsInternal, so just uSec delay the path
                // here for 50uSec.

                // if in ISR, return

                // if more than 100*50us=5ms,still no packet avaialable, PC side go wrong,stop the loop
                // otherwise it will spin here forever and stopwatch get kick in.
                WaitLoopCnt++;
                if(WaitLoopCnt > 100)
                {
                    // if we were unable to send any data then no one is listening so lets
                    if(count == size)
                    {
                        State->Queues[endpoint]->Initialize();
                    }
                    
                    return totWrite;
                }

                if(SystemState_QueryNoLock( SYSTEM_STATE_ISR ))
                {
                    return totWrite;
                }

                if(irq.WasDisabled()) // @todo - this really needs more checks to be totally valid
                {
                    return totWrite;
                }

                if(State->DeviceState != USB_DEVICE_STATE_CONFIGURED)
                {
                    return totWrite;
                }

                CPU_USB_StartOutput( State, endpoint );

                irq.Release();
//                lcd_printf("Looping in write\r\n");

                HAL_Time_Sleep_MicroSeconds_InterruptEnabled(50);

                irq.Acquire();
            }
        }

        // here we have a post-condition that IRQs are disabled for all paths through conditional block above

        if(State->DeviceState == USB_DEVICE_STATE_CONFIGURED)
        {
            CPU_USB_StartOutput( State, endpoint );
        }

        return totWrite;
    }
}

int USB_Driver::Read( int UsbStream, char* Data, size_t size )
{
    NATIVE_PROFILE_PAL_COM();

    int Controller  = ConvertCOM_UsbController ( UsbStream );
    int StreamIndex = ConvertCOM_UsbStreamIndex( UsbStream );
    int endpoint;
    USB_CONTROLLER_STATE * State = CPU_USB_GetState( Controller );

    if( NULL == State || StreamIndex >= USB_MAX_QUEUES )
    {
        return 0;
    }

    /* not configured, no data can go in or out */
    if( State->DeviceState != USB_DEVICE_STATE_CONFIGURED )
    {
        return 0;
    }

    endpoint = State->streams[StreamIndex].RxEP;
    // If no Read side to stream (or if not yet open)
    if( endpoint == USB_NULL_ENDPOINT || State->Queues[endpoint] == NULL )
    {
        return 0;
    }

    {
        GLOBAL_LOCK(irq);

        USB_PACKET64* Packet64 = NULL;
        UINT8*        ptr      = (UINT8*)Data;
        UINT32        count    = 0;
        UINT32        remain   = size;

        while(count < size)
        {
            UINT32 max_move;

            if(!Packet64) Packet64 = State->Queues[endpoint]->Peek();

            if(!Packet64)
            {
                USB_ClearEvent( Controller, 1 << endpoint );
                break;
            }

            max_move = Packet64->Size - State->CurrentPacketOffset[endpoint];
            if(remain < max_move) max_move = remain;

            memcpy( ptr, &Packet64->Buffer[ State->CurrentPacketOffset[endpoint] ], max_move );

            State->CurrentPacketOffset[endpoint] += max_move;
            ptr                                           += max_move;
            count                                         += max_move;
            remain                                        -= max_move;

            /* if we're done with this packet, move onto the next */
            if(State->CurrentPacketOffset[endpoint] == Packet64->Size)
            {
                State->CurrentPacketOffset[endpoint] = 0;
                Packet64                             = NULL;

                State->Queues[endpoint]->Pop();

                CPU_USB_RxEnable( State, endpoint );
            }
        }

        return count;
    }
}

BOOL USB_Driver::Flush( int UsbStream )
{
    NATIVE_PROFILE_PAL_COM();

    int Controller  = ConvertCOM_UsbController ( UsbStream );
    int StreamIndex = ConvertCOM_UsbStreamIndex( UsbStream );
    int endpoint;
    int retries = USB_FLUSH_RETRY_COUNT;
    int queueCnt;
    USB_CONTROLLER_STATE * State = CPU_USB_GetState( Controller );

    if( NULL == State || StreamIndex >= USB_MAX_QUEUES )
    {
        return FALSE;
    }

    /* not configured, no data can go in or out */
    if(State->DeviceState != USB_DEVICE_STATE_CONFIGURED)
    {
        return TRUE;
    }

    endpoint = State->streams[StreamIndex].TxEP;
    // If no Write side to stream (or if not yet open)
    if( endpoint == USB_NULL_ENDPOINT || State->Queues[endpoint] == NULL )
    {
        return FALSE;
    }

    queueCnt = State->Queues[endpoint]->NumberOfElements();
    
    // interrupts were disabled or USB interrupt was disabled for whatever reason, so force the flush
    while(State->Queues[endpoint]->IsEmpty() == false && retries > 0)
    {
        CPU_USB_StartOutput( State, endpoint );

        HAL_Time_Sleep_MicroSeconds_InterruptEnabled(100); // don't call Events_WaitForEventsXXX because it will turn off interrupts

        int cnt = State->Queues[endpoint]->NumberOfElements();
        retries  = (queueCnt == cnt) ? retries-1: USB_FLUSH_RETRY_COUNT;
        queueCnt = cnt;
    }

    if(retries <=0)
    {
        State->Queues[endpoint]->Initialize();
    }

    return TRUE;
}

UINT32 USB_Driver::GetEvent( int Controller, UINT32 Mask )
{
    GLOBAL_LOCK(irq);

    USB_CONTROLLER_STATE *State = CPU_USB_GetState( Controller );

    if( State )
        return (State->Event & Mask);
    else
        return 0;
}

UINT32 USB_Driver::SetEvent( int Controller, UINT32 Event )
{
    GLOBAL_LOCK(irq);

    USB_CONTROLLER_STATE *State = CPU_USB_GetState( Controller );

    if( State == NULL )
        return 0;

    UINT32 OldEvent = State->Event;

    State->Event |= Event;

    if(OldEvent != State->Event)
    {
        Events_Set( SYSTEM_EVENT_FLAG_USB_IN );
    }

//printf("SetEv %d\r\n",State->Event);
    return OldEvent;
}

UINT32 USB_Driver::ClearEvent( int Controller, UINT32 Event )
{
    GLOBAL_LOCK(irq);

    USB_CONTROLLER_STATE *State = CPU_USB_GetState( Controller );

    if( State == NULL )
        return 0;

    UINT32 OldEvent = State->Event;

    State->Event &= ~Event;

    if( State->Event == 0 )
    {
        Events_Clear( SYSTEM_EVENT_FLAG_USB_IN );
    }

//printf("ClrEV %d\r\n",State->Event);
    return OldEvent;
}

UINT8 USB_Driver::GetStatus( int Controller )
{
    USB_CONTROLLER_STATE *State = CPU_USB_GetState( Controller );

    if( State == NULL )
        return USB_DEVICE_STATE_NO_CONTROLLER;

    if( !State->Initialized || State->Configuration == NULL )
        return USB_DEVICE_STATE_UNINITIALIZED;
    
    return State->CurrentState;
}

void USB_Driver::DiscardData( int UsbStream, BOOL fTx )
{
    int Controller  = ConvertCOM_UsbController ( UsbStream );
    int StreamIndex = ConvertCOM_UsbStreamIndex( UsbStream );
    int endpoint;
    USB_CONTROLLER_STATE *State = CPU_USB_GetState( Controller );

    if( State == NULL )
        return;

    if( !State->Initialized || State->Configuration == NULL )
        return;

    if(fTx)
    {
        endpoint = State->streams[StreamIndex].TxEP;
    }
    else
    {
        endpoint = State->streams[StreamIndex].RxEP;
    }
    
    // If no Read side to stream (or if not yet open)
    if( endpoint == USB_NULL_ENDPOINT || State->Queues[endpoint] == NULL )
    {
        return;
    }

    if( State->Queues[endpoint] )
    {
        State->Queues[endpoint]->Initialize();
    }
}

//--//

void USB_ClearQueues( USB_CONTROLLER_STATE *State, BOOL ClrRxQueue, BOOL ClrTxQueue )
{
    GLOBAL_LOCK(irq);

    if(ClrRxQueue)
    {
        for(int endpoint = 0; endpoint < USB_MAX_QUEUES; endpoint++)
        {
            if( State->Queues[endpoint] == NULL || State->IsTxQueue[endpoint] )
                continue;
            State->Queues[endpoint]->Initialize();

            /* since this queue is now reset, we have room available for newly arrived packets */
            CPU_USB_RxEnable( State, endpoint );
        }
    }

    if( ClrTxQueue )
    {
        for(int endpoint = 0; endpoint < USB_MAX_QUEUES; endpoint++)
        {
            if( State->Queues[endpoint] && State->IsTxQueue[endpoint] )
                State->Queues[endpoint]->Initialize();
        }
    }
}

void USB_StateCallback( USB_CONTROLLER_STATE* State )
{
    if(State->CurrentState != State->DeviceState)
    {
        /* whenever we leave the configured state, re-initialize all of the queues */
//Not necessary, as TxBuffer may hold any data and then send them out when it is configured again.
// The RxQueue is clear when it is configured.

        if(USB_DEVICE_STATE_CONFIGURED == State->CurrentState)
        {
            USB_ClearQueues( State, TRUE, TRUE );
        }

        State->CurrentState = State->DeviceState;

        switch(State->DeviceState)
        {
        case USB_DEVICE_STATE_DETACHED:
            State->ResidualCount =0;
            State->DataCallback = NULL;
//            hal_printf("USB_DEVICE_STATE_DETACHED\r\n");
            break;

        case USB_DEVICE_STATE_ATTACHED:
//            hal_printf("USB_DEVICE_STATE_ATTACHED\r\n");
            break;

        case USB_DEVICE_STATE_POWERED:
//            hal_printf("USB_DEVICE_STATE_POWERED\r\n");
            break;

        case USB_DEVICE_STATE_DEFAULT:
//            hal_printf("USB_DEVICE_STATE_DEFAULT\r\n");
            break;

        case USB_DEVICE_STATE_ADDRESS:
//            hal_printf("USB_DEVICE_STATE_ADDRESS\r\n");
            break;

        case USB_DEVICE_STATE_CONFIGURED:
//            hal_printf("USB_DEVICE_STATE_CONFIGURED\r\n");

            /* whenever we enter the configured state, re-initialize all of the RxQueues */
            /* Txqueue has stored some data to be transmitted */
            USB_ClearQueues( State, TRUE, FALSE );
            break;

        case USB_DEVICE_STATE_SUSPENDED:
//            hal_printf("USB_DEVICE_STATE_SUSPENDED\r\n");
            break;

        default:
            ASSERT(0);
            break;
        }
    }
}

void USB_DataCallback( USB_CONTROLLER_STATE* State )
{
    UINT32 length = __min(State->PacketSize, State->ResidualCount);

    memcpy( State->Data, State->ResidualData, length );

    State->DataSize       = length;
    State->ResidualData  += length;
    State->ResidualCount -= length;

    if(length == State->PacketSize)
    {
        State->Expected -= length;
    }
    else
    {
        State->Expected = 0;
    }

    if(State->Expected)
    {
        State->DataCallback = USB_DataCallback;
    }
    else
    {
        State->DataCallback = NULL;
    }
}

UINT8 USB_HandleGetStatus( USB_CONTROLLER_STATE* State, USB_SETUP_PACKET* Setup )
{
    UINT16* status;
    UINT16  zero = 0;

    /* validate setup packet */
    if(Setup->wValue != 0 || Setup->wLength != 2)
    {
        return USB_STATE_STALL;
    }

    /* validate based on device state */
    if(State->DeviceState == USB_DEVICE_STATE_DEFAULT)
    {
        return USB_STATE_STALL;
    }

    switch(USB_SETUP_RECIPIENT(Setup->bmRequestType))
    {
    case USB_SETUP_RECIPIENT_DEVICE:
        status = &State->DeviceStatus;
        break;

    case USB_SETUP_RECIPIENT_INTERFACE:
        if(State->DeviceState != USB_DEVICE_STATE_CONFIGURED)
        {
            return USB_STATE_STALL;
        }

        status = &zero;
        break;

    case USB_SETUP_RECIPIENT_ENDPOINT:
        if(State->DeviceState == USB_DEVICE_STATE_ADDRESS && Setup->wIndex != 0)
        {
            return USB_STATE_STALL;
        }

        /* bit 0x80 designates direction, which we don't utilize in this calculation */
        Setup->wIndex &= 0x7F;

        if(Setup->wIndex >= State->EndpointCount)
        {
            return USB_STATE_STALL;
        }

        status = &State->EndpointStatus[Setup->wIndex];
        break;

    default:
        return USB_STATE_STALL;
    }

    /* send requested status to host */
    State->ResidualData  = (UINT8*)status;
    State->ResidualCount = 2;
    State->DataCallback  = USB_DataCallback;

    return USB_STATE_DATA;
}

UINT8 USB_HandleClearFeature( USB_CONTROLLER_STATE* State, USB_SETUP_PACKET* Setup )
{
    USB_CONFIGURATION_DESCRIPTOR * Config;
    UINT8       retState;

    /* validate setup packet */
    if(Setup->wLength != 0)
    {
        return USB_STATE_STALL;
    }

    /* validate based on device state */
    if(State->DeviceState != USB_DEVICE_STATE_CONFIGURED)
    {
        return USB_STATE_STALL;
    }

    switch(USB_SETUP_RECIPIENT(Setup->bmRequestType))
    {
    case USB_SETUP_RECIPIENT_DEVICE:
         // only support Remote wakeup
        if(Setup->wValue != USB_FEATURE_DEVICE_REMOTE_WAKEUP)
            return USB_STATE_STALL;

        // Locate the configuration descriptor
        Config = (USB_CONFIGURATION_DESCRIPTOR *)USB_FindRecord( State, USB_CONFIGURATION_DESCRIPTOR_MARKER, Setup );

        if(Config && (Config->bmAttributes & USB_ATTRIBUTE_REMOTE_WAKEUP))
        {
            State->DeviceStatus &= ~USB_STATUS_DEVICE_REMOTE_WAKEUP;
            retState             = USB_STATE_REMOTE_WAKEUP;
        }
        else
        {
            return USB_STATE_STALL;
        }
        break;

    case USB_SETUP_RECIPIENT_INTERFACE:
        /* there are no interface features to clear */
        return USB_STATE_STALL;

    case USB_SETUP_RECIPIENT_ENDPOINT:
        if(State->DeviceState == USB_DEVICE_STATE_ADDRESS && Setup->wIndex != 0)
            return USB_STATE_STALL;

        /* bit 0x80 designates direction, which we dont utilize in this calculation */
        Setup->wIndex &= 0x7F;

        if(Setup->wIndex == 0 || Setup->wIndex >= State->EndpointCount)
            return USB_STATE_STALL;

        if(Setup->wValue != USB_FEATURE_ENDPOINT_HALT)
            return USB_STATE_STALL;

        /* clear the halt feature */
        State->EndpointStatus[Setup->wIndex] &= ~USB_STATUS_ENDPOINT_HALT;
        State->EndpointStatusChange = Setup->wIndex;
        retState=  USB_STATE_STATUS;
        break;

    default:
        return USB_STATE_STALL;
    }

    /* send zero-length packet to tell host we're done */
    State->ResidualCount = 0;
    State->DataCallback  = USB_DataCallback;

    /* notify lower layer of status change */
    return retState;
}

UINT8 USB_HandleSetFeature( USB_CONTROLLER_STATE* State, USB_SETUP_PACKET* Setup )
{
    USB_CONFIGURATION_DESCRIPTOR * Config;
    UINT8       retState;

    /* validate setup packet */
    if(Setup->wLength != 0)
    {
        return USB_STATE_STALL;
    }

    /* validate based on device state */
    if(State->DeviceState == USB_DEVICE_STATE_DEFAULT)
    {
        return USB_STATE_STALL;
    }

    switch(USB_SETUP_RECIPIENT(Setup->bmRequestType))
    {
    case USB_SETUP_RECIPIENT_DEVICE:
         // only support Remote wakeup
        if(Setup->wValue != USB_FEATURE_DEVICE_REMOTE_WAKEUP)
        {
            return USB_STATE_STALL;
        }

        Config = (USB_CONFIGURATION_DESCRIPTOR *)USB_FindRecord( State, USB_CONFIGURATION_DESCRIPTOR_MARKER, Setup );
        if( Config == NULL )        // If the configuration record could not be found
            return USB_STATE_STALL; // Something pretty serious is wrong

        if(Config->bmAttributes & USB_ATTRIBUTE_REMOTE_WAKEUP)
        {
            State->DeviceStatus |= USB_STATUS_DEVICE_REMOTE_WAKEUP;
        }

        retState =  USB_STATE_REMOTE_WAKEUP;
        break;

    case USB_SETUP_RECIPIENT_INTERFACE:
        /* there are no interface features to set */
        return USB_STATE_STALL;

    case USB_SETUP_RECIPIENT_ENDPOINT:
        if(State->DeviceState == USB_DEVICE_STATE_ADDRESS && Setup->wIndex != 0)
        {
            return USB_STATE_STALL;
        }

        /* bit 0x80 designates direction, which we don't utilize in this calculation */
        Setup->wIndex &= 0x7F;

        if(Setup->wIndex == 0 || Setup->wIndex >= State->EndpointCount)
        {
            return USB_STATE_STALL;
        }

        if(Setup->wValue != USB_FEATURE_ENDPOINT_HALT)
        {
            return USB_STATE_STALL;
        }

        /* set the halt feature */
        State->EndpointStatus[Setup->wIndex] |= USB_STATUS_ENDPOINT_HALT;
        State->EndpointStatusChange           = Setup->wIndex;
        retState = USB_STATE_STATUS;
        break;

    default:
        return USB_STATE_STALL;
    }

    /* send zero-length packet to tell host we're done */
    State->ResidualCount = 0;
    State->DataCallback  = USB_DataCallback;

    /* notify lower layer of status change */
    return retState;
}

UINT8 USB_HandleSetAddress( USB_CONTROLLER_STATE* State, USB_SETUP_PACKET* Setup )
{
    /* validate setup packet */
    if(Setup->wValue > 127 || Setup->wIndex != 0 || Setup->wLength != 0)
    {
        return USB_STATE_STALL;
    }

    /* validate based on device state */
    if(State->DeviceState >= USB_DEVICE_STATE_CONFIGURED)
    {
        return USB_STATE_STALL;
    }

    /* set address */
    State->Address = Setup->wValue;

    /* catch state changes */
    if(State->Address == 0)
    {
        State->DeviceState = USB_DEVICE_STATE_DEFAULT;
    }
    else
    {
        State->DeviceState = USB_DEVICE_STATE_ADDRESS;
    }

    USB_StateCallback( State );

    /* send zero-length packet to tell host we're done */
    State->ResidualCount = 0;
    State->DataCallback  = USB_DataCallback;

    /* notify hardware of address change */
    return USB_STATE_ADDRESS;
}

UINT8 USB_HandleConfigurationRequests( USB_CONTROLLER_STATE* State, USB_SETUP_PACKET* Setup )
{
    const USB_DESCRIPTOR_HEADER * header;
    UINT8       type;
    UINT8       DescriptorIndex;

    /* this request is valid regardless of device state */
    type            = ((Setup->wValue & 0xFF00) >> 8);
    DescriptorIndex =  (Setup->wValue & 0x00FF);
    State->Expected =   Setup->wLength;

    if(State->Expected == 0)
    {
        // just return an empty Status packet
         State->ResidualCount = 0;
         State->DataCallback = USB_DataCallback;
         return USB_STATE_DATA;
    }

    //
    // The very first GET_DESCRIPTOR command out of reset should always return at most PacketSize bytes.
    // After that, you can return as many as the host has asked.
    //
    if(State->DeviceState <= USB_DEVICE_STATE_DEFAULT)
    {
        if(State->FirstGetDescriptor)
        {
            State->FirstGetDescriptor = FALSE;

            State->Expected = __min(State->Expected, State->PacketSize);
        }
    }

    State->ResidualData = NULL;
    State->ResidualCount = 0;

    if( Setup->bRequest == USB_GET_DESCRIPTOR )
    {
        switch(type)
        {
        case USB_DEVICE_DESCRIPTOR_TYPE:
            header = USB_FindRecord( State, USB_DEVICE_DESCRIPTOR_MARKER, Setup );
            if( header )
            {
                const USB_DEVICE_DESCRIPTOR * device = (USB_DEVICE_DESCRIPTOR *)header;
                State->ResidualData = (UINT8 *)&device->bLength;      // Start of the device descriptor
                State->ResidualCount = __min(State->Expected, device->bLength);
            }
            break;

        case USB_CONFIGURATION_DESCRIPTOR_TYPE:
            header = USB_FindRecord( State, USB_CONFIGURATION_DESCRIPTOR_MARKER, Setup );
            if( header )
            {
                const USB_CONFIGURATION_DESCRIPTOR * Config = (USB_CONFIGURATION_DESCRIPTOR *)header;
                State->ResidualData = (UINT8 *)&Config->bLength;
                State->ResidualCount = __min(State->Expected, Config->wTotalLength);
            }
            break;

        case USB_STRING_DESCRIPTOR_TYPE:
            if(DescriptorIndex == 0)        // If host is requesting the language list
            {
                State->ResidualData  = USB_LanguageDescriptor;
                State->ResidualCount = __min(State->Expected, USB_LANGUAGE_DESCRIPTOR_SIZE);
            }
            else if( DescriptorIndex == USB_FRIENDLY_STRING_NUM && FriendlyNameString.bLength != 0 )      // If "friendly name" was changed by Flash Config sector
            {
                State->ResidualData = (UINT8 *)&FriendlyNameString;
                State->ResidualCount = __min(State->Expected, FriendlyNameString.bLength);
            }
            else if( NULL != (header = USB_FindRecord( State, USB_STRING_DESCRIPTOR_MARKER, Setup )) )
            {
                const USB_STRING_DESCRIPTOR_HEADER * string = (USB_STRING_DESCRIPTOR_HEADER *)header;
                State->ResidualData = (UINT8 *)&string->bLength;
                State->ResidualCount = __min(State->Expected, string->bLength);
            }
            break;

        default:
            break;
        }
    }

    // If the request was not recognized, the generic types should be searched
    if( State->ResidualData == NULL )
    {
        if( NULL != (header = USB_FindRecord( State, USB_GENERIC_DESCRIPTOR_MARKER, Setup )) )
        {
            State->ResidualData = (UINT8 *)header;
            State->ResidualData += sizeof(USB_GENERIC_DESCRIPTOR_HEADER);       // Data is located right after the header
            State->ResidualCount = __min(State->Expected, header->size - sizeof(USB_GENERIC_DESCRIPTOR_HEADER));
        }
        else
            return USB_STATE_STALL;
    }

    State->DataCallback = USB_DataCallback;

    return USB_STATE_DATA;
}

UINT8 USB_HandleGetConfiguration( USB_CONTROLLER_STATE* State, USB_SETUP_PACKET* Setup )
{
    /* validate setup packet */
    if(Setup->wValue != 0 || Setup->wIndex != 0 || Setup->wLength != 1)
    {
        return USB_STATE_STALL;
    }

    /* validate based on device state */
    if(State->DeviceState == USB_DEVICE_STATE_DEFAULT)
    {
        return USB_STATE_STALL;
    }

    State->ResidualData  = &State->ConfigurationNum;
    State->ResidualCount = 1;
    State->Expected      = 1;
    State->DataCallback  = USB_DataCallback;

    return USB_STATE_DATA;
}

UINT8 USB_HandleSetConfiguration( USB_CONTROLLER_STATE* State, USB_SETUP_PACKET* Setup, BOOL DataPhase )
{
    /* validate setup packet */
    if(Setup->wIndex != 0 || Setup->wLength != 0)
    {
        return USB_STATE_STALL;
    }

    /* validate based on device state */
    if(State->DeviceState == USB_DEVICE_STATE_DEFAULT)
    {
        return USB_STATE_STALL;
    }

    /* we only support one configuration */
    if(Setup->wValue > 1)
    {
        return USB_STATE_STALL;
    }

    State->ConfigurationNum = Setup->wValue;

    /* catch state changes */
    if(State->ConfigurationNum == 0)
    {
        State->DeviceState = USB_DEVICE_STATE_ADDRESS;
    }
    else
    {
        State->DeviceState = USB_DEVICE_STATE_CONFIGURED;
    }

    USB_StateCallback( State );

    if (DataPhase)
    {
        /* send zero-length packet to tell host we're done */
        State->ResidualCount = 0;
        State->DataCallback  = USB_DataCallback;
    }

    return USB_STATE_CONFIGURATION;
}

//--//

// Searches through the USB Configuration records for the requested type
// Returns a pointer to the header information if found and NULL if not
const USB_DESCRIPTOR_HEADER * USB_FindRecord( USB_CONTROLLER_STATE* State, UINT8 marker, USB_SETUP_PACKET * setup )
{
    bool Done = false;
    const USB_DESCRIPTOR_HEADER * header = (const USB_DESCRIPTOR_HEADER *)State->Configuration;

    // If there is no configuration for this Controller
    if( NULL == header )
        return header;

    while( !Done )
    {
        const UINT8 * next = (const UINT8 *)header;
        next += header->size;      // Calculate address of next record
        const USB_GENERIC_DESCRIPTOR_HEADER *generic = (USB_GENERIC_DESCRIPTOR_HEADER *)header;
        
        switch( header->marker )
        {
        case USB_DEVICE_DESCRIPTOR_MARKER:
        case USB_CONFIGURATION_DESCRIPTOR_MARKER:
            if( header->marker == marker )
                Done = true;
            break;
        case USB_STRING_DESCRIPTOR_MARKER:
            // If String descriptor then the index is significant
            if( (header->marker == marker) && (header->iValue == (setup->wValue & 0x00FF)) )
                Done = true;
            break;
        case USB_GENERIC_DESCRIPTOR_MARKER:
            if( generic->bmRequestType == setup->bmRequestType &&
                generic->bRequest      == setup->bRequest &&
                generic->wValue        == setup->wValue &&
                generic->wIndex        == setup->wIndex )
            {
                Done = true;
            }
            break;
        case USB_END_DESCRIPTOR_MARKER:
        default:
            Done = true;
            header = NULL;
            break;
        }
        if( !Done )
            header = (const USB_DESCRIPTOR_HEADER *)next;    // Try next record
    }

    return header;
}

UINT8 USB_ControlCallback( USB_CONTROLLER_STATE* State )
{        
    USB_SETUP_PACKET* Setup;

    if(State->DataSize == 0)
    {
        return USB_STATE_DONE;
    }

    Setup = (USB_SETUP_PACKET*)State->Data;
    
    switch(Setup->bRequest)
    {
    case USB_GET_STATUS:
        return USB_HandleGetStatus            ( State, Setup );
    case USB_CLEAR_FEATURE:
        return USB_HandleClearFeature         ( State, Setup );
    case USB_SET_FEATURE:
        return USB_HandleSetFeature           ( State, Setup );
    case USB_SET_ADDRESS:
        return USB_HandleSetAddress           ( State, Setup );
    case USB_GET_CONFIGURATION:
        return USB_HandleGetConfiguration     ( State, Setup );
    case USB_SET_CONFIGURATION:
        return USB_HandleSetConfiguration     ( State, Setup, TRUE );
    default:
        return USB_HandleConfigurationRequests( State, Setup );
    }

    return USB_STATE_STALL;
}

USB_PACKET64* USB_RxEnqueue( USB_CONTROLLER_STATE* State, int endpoint, BOOL& DisableRx )
{
    ASSERT_IRQ_MUST_BE_OFF();
    ASSERT( State && (endpoint < USB_MAX_QUEUES) );
    ASSERT( State->Queues[endpoint] && !State->IsTxQueue[endpoint] )

    USB_PACKET64* Packet64 = State->Queues[endpoint]->Push();

    DisableRx = State->Queues[endpoint]->IsFull();

    USB_SetEvent( State->ControllerNum, 1 << endpoint );

    return Packet64;
}

USB_PACKET64* USB_TxDequeue( USB_CONTROLLER_STATE* State, int endpoint, BOOL Done )
{
    ASSERT_IRQ_MUST_BE_OFF();
    ASSERT( State && (endpoint < USB_MAX_QUEUES) );
    ASSERT( State->Queues[endpoint] && State->IsTxQueue[endpoint] )

    if(Done)
    {
        return State->Queues[endpoint]->Pop();
    }
    else
    {
        return State->Queues[endpoint]->Peek();
    }
}

// UsbConfigurationCheck()
// Checks each record of a USB descriptor list for simple mistakes.  This test should
// always be performed before allowing a USB port to be initialized with the descriptor
// list.
//
// NOTE:  TODO: Endpoints need to be checked for overlap (must not be used more than once).
int UsbConfigurationCheck( const USB_DYNAMIC_CONFIGURATION *firstRecord )
{
    const UINT8                         *next;
    const USB_DESCRIPTOR_HEADER         *record;

    const USB_DEVICE_DESCRIPTOR         *device;
    const USB_CONFIGURATION_DESCRIPTOR  *configuration;
    const USB_INTERFACE_DESCRIPTOR      *interface;
    const USB_ENDPOINT_DESCRIPTOR       *endpoint;
    const USB_STRING_DESCRIPTOR_HEADER  *string;
    const USB_GENERIC_DESCRIPTOR_HEADER *generic;

    UINT8 nInterfaces   = 0;
    UINT8 nEndpoints    = 0;
    bool  foundDevice   = false;
    bool  foundConfig   = false;
    bool  epUsed[31];
    UINT8 itfcUsed[10];
    int   i;

    int  recordError    = USB_CONFIG_ERR_OK;

    for( i = 0; i < 31; i++ )
        epUsed[i] = false;                  // Set all endpoints to unused

    for( i = 0; i < 10; i++ )
        itfcUsed[i] = 0xFF;                 // Empty interface list

    for( record = (const USB_DESCRIPTOR_HEADER *)firstRecord; USB_CONFIG_ERR_OK == recordError; record = (const USB_DESCRIPTOR_HEADER *)next )
    {
        next = (const UINT8 *)record;
        next += record->size;       // Calculate address of next record

        switch( record->marker )
        {
        case USB_END_DESCRIPTOR_MARKER:
            if( foundDevice && foundConfig )
                return recordError;
            recordError = USB_CONFIG_ERR_MISSING_RECORD;
            break;

        case USB_DEVICE_DESCRIPTOR_MARKER:
            if( foundDevice )
            {
                recordError = USB_CONFIG_ERR_DUP_DEVICE;
                break;      // Only one device descriptor allowed
            }
            device = (const USB_DEVICE_DESCRIPTOR *)record;
            if( record->size != sizeof(USB_DEVICE_DESCRIPTOR) || device->bLength != USB_DEVICE_DESCRIPTOR_LENGTH )
            {
                recordError = USB_CONFIG_ERR_DEVICE_SIZE;
                break;      // Record has wrong length
            }
            if( device->bDescriptorType != USB_DEVICE_DESCRIPTOR_TYPE )
            {
                recordError = USB_CONFIG_ERR_DEVICE_TYPE;
                break;      // Not actually a device descriptor
            }
            if( device->bMaxPacketSize0 != 8 && device->bMaxPacketSize0 != 16
                && device->bMaxPacketSize0 != 32 && device->bMaxPacketSize0 != 64 )
            {
                recordError = USB_CONFIG_ERR_EP0_SIZE;
                break;      // Endpoint 0 packet size is not legal
            }
            if( device->bNumConfigurations != 1 )
            {
                recordError = USB_CONFIG_ERR_NCONFIGS;
                break;      // Only exactly 1 configuration is allowed
            }
            foundDevice = true;
            break;
        case USB_CONFIGURATION_DESCRIPTOR_MARKER:
            if( foundConfig )
            {
                recordError = USB_CONFIG_ERR_DUP_CONFIG;
                break;      // Only one configuration descriptor allowed
            }
            configuration = (USB_CONFIGURATION_DESCRIPTOR *)record;
            if( configuration->bLength != USB_CONFIGURATION_DESCRIPTOR_LENGTH
                || record->size != (configuration->wTotalLength + sizeof(USB_DESCRIPTOR_HEADER)) )
            {
                recordError = USB_CONFIG_ERR_CONFIG_SIZE;
                break;      // Record sizes wrong or do not match
            }
            if( configuration->bDescriptorType != USB_CONFIGURATION_DESCRIPTOR_TYPE )
            {
                recordError = USB_CONFIG_ERR_CONFIG_TYPE;
                break;      // Not actually a configuration descriptor
            }
            nInterfaces = configuration->bNumInterfaces;
            if( nInterfaces == 0 )
            {
                recordError = USB_CONFIG_ERR_NO_INTERFACE;
                break;      // There must be at least one interface
            }
            if( configuration->bConfigurationValue > 1 )
            {
                recordError = USB_CONFIG_ERR_CONFIG_NUM;
                break;      // Only allow configuration numbers less than 2
            }
            if( (configuration->bmAttributes & 0x8F) != 0x80 )
            {
                recordError = USB_CONFIG_ERR_CONFIG_ATTR;
                break;      // Attribute byte has wrong format
            }
            // First interface descriptor is right after configuration descriptor
            interface = (const USB_INTERFACE_DESCRIPTOR *)&configuration[1];
            while( nInterfaces-- )
            {
                if( interface->bDescriptorType != USB_INTERFACE_DESCRIPTOR_TYPE )
                {
                    recordError = USB_CONFIG_ERR_INTERFACE_TYPE;
                    break;      // Not really an interface descriptor
                }
                if( interface->bLength != sizeof(USB_INTERFACE_DESCRIPTOR) )
                {
                    recordError = USB_CONFIG_ERR_INTERFACE_LEN;
                    break;      // Length of interface descriptor is wrong
                }
                if( interface->bAlternateSetting != 0 )
                {
                    recordError = USB_CONFIG_ERR_INTERFACE_ALT;
                    break;      // Alternate interfaces not allowed
                }
                nEndpoints = interface->bNumEndpoints;
                if( nEndpoints == 0 )
                {
                    recordError = USB_CONFIG_ERR_NO_ENDPOINT;
                    break;      // Each interface must have at least one endpoint
                }
                for( i = 0; itfcUsed[i] != 0xFF && i < 10; i++ )
                {
                    // If this interface number has already been used
                    if( itfcUsed[i] == interface->bInterfaceNumber )
                    {
                        recordError = USB_CONFIG_ERR_DUP_INTERFACE;
                        break;
                    }
                }
                if( i >= 10 )
                {
                    recordError = USB_CONFIG_ERR_TOO_MANY_ITFC;
                    break;
                }
                itfcUsed[i] = interface->bInterfaceNumber;              // Mark interface as used
                endpoint = (const USB_ENDPOINT_DESCRIPTOR *)&interface[1];    // First endpoint is right after interface
                // Records after interface descriptor may be some kind of class descriptor
                while( endpoint->bDescriptorType != USB_ENDPOINT_DESCRIPTOR_TYPE )
                {
                    // Leap over possible interface class descriptor
                    const UINT8 *classDescriptor = (const UINT8 *)endpoint;
                    classDescriptor += endpoint->bLength;       // Position of length is the same for any descriptor
                    endpoint = (const USB_ENDPOINT_DESCRIPTOR *)classDescriptor;
                }
                while( nEndpoints-- )
                {
                    if( endpoint->bDescriptorType != USB_ENDPOINT_DESCRIPTOR_TYPE )
                    {
                        recordError = USB_CONFIG_ERR_ENDPOINT_TYPE;
                        break;      // Not really an endpoint descriptor
                    }
                    if( endpoint->bLength != sizeof(USB_ENDPOINT_DESCRIPTOR) )
                    {
                        recordError = USB_CONFIG_ERR_ENDPOINT_LEN;
                        break;      // Length of endpoint descriptor is wrong
                    }
                    if( (endpoint->bEndpointAddress & 0x7F) < 1 || (endpoint->bEndpointAddress & 0x7F) > 31 )
                    {
                        recordError = USB_CONFIG_ERR_ENDPOINT_RANGE;
                        break;      // Endpoint number either zero or too large
                    }
                    if( (endpoint->bmAttributes & 0x03) == 0 || (endpoint->bmAttributes & 0xC0) != 0 )
                    {
                        recordError = USB_CONFIG_ERR_ENDPOINT_ATTR;
                        break;      // Endpoint set to control type or has reserved bits set
                    }
                    if( epUsed[(endpoint->bEndpointAddress & 0x7F)-1] )
                    {
                        recordError = USB_CONFIG_ERR_DUP_ENDPOINT;
                        break;
                    }
                    epUsed[(endpoint->bEndpointAddress & 0x7F)-1] = true;       // Show endpoint as being used
                    endpoint = &endpoint[1];        // Next endpoint immediately follows last one
                }
                if( recordError != USB_CONFIG_ERR_OK )
                    break;
                interface = (USB_INTERFACE_DESCRIPTOR *)endpoint;       // Next interface descriptor is right after last endpoint
            }
            // If all interfaces & endpoints are OK and end of all configuration descriptors coincides
            // with the start of the next record, only then is this record good.
            if( recordError != USB_CONFIG_ERR_OK )
                break;
            if( ((UINT8 *)interface == next) )
                foundConfig = true;
            else
                recordError = USB_CONFIG_ERR_CONFIG_SIZE;
            break;

        case USB_STRING_DESCRIPTOR_MARKER:
            string = (const USB_STRING_DESCRIPTOR_HEADER *)record;
            if( record->size != (string->bLength + sizeof(USB_DESCRIPTOR_HEADER)) )
            {
                recordError = USB_CONFIG_ERR_STRING_SIZE;
                break;      // Record sizes do not match
            }
            if( string->bDescriptorType != USB_STRING_DESCRIPTOR_TYPE )
            {
                recordError = USB_CONFIG_ERR_STRING_TYPE;
                break;      // Not actually a string descriptor
            }
            break;
        case USB_GENERIC_DESCRIPTOR_MARKER:
            generic = (const USB_GENERIC_DESCRIPTOR_HEADER *)record;
            if( (generic->bmRequestType & 0x80) != 0x80 )
            {
                recordError = USB_CONFIG_ERR_GENERIC_DIR;
                break;      // Must be a request for information
            }
            break;
        default:
            recordError = USB_CONFIG_ERR_UNKNOWN_RECORD;
            break;
        }
    }
    
    return recordError;
}


// USB_NextEndpoint()
// Returns a pointer to the next endpoint descriptor in the USB configuration list along with
// a pointer to its interface descriptor.  Returns FALSE if ep or itfc are bogus pointers or
// if the end of the configuration list has been reached (no more endpoints).
// To get the first endpoint and its interface, this should be called with ep = NULL.

BOOL USB_NextEndpoint( USB_CONTROLLER_STATE* State, const USB_ENDPOINT_DESCRIPTOR * &ep, const USB_INTERFACE_DESCRIPTOR* &itfc )
{
    const UINT8 *next;
    const UINT8 *end;
    const USB_CONFIGURATION_DESCRIPTOR *Config;

    // Locate the configuration descriptor
    Config = (const USB_CONFIGURATION_DESCRIPTOR *)USB_FindRecord( State, USB_CONFIGURATION_DESCRIPTOR_MARKER, NULL );
    if( NULL == Config )        // If configuration is bogus
        return FALSE;

    // Calculate the range of addresses where endpoint descriptors may be found
    next = (const UINT8 *)&Config[1];
    end  = ((const UINT8 *)Config) + Config->header.size;

    // If requesting the first endpoint
    if( NULL == ep )
    {
        // Possible location of first endpoint descriptor - actually, this will
        // always be the first interface descriptor, but it doesn't matter.
        ep = (const USB_ENDPOINT_DESCRIPTOR *)next;
        itfc = NULL;
    }
    // If not, make sure that both pointers are good
    else if( (UINT8 *)ep > next
             && (UINT8 *)ep < end
             && (UINT8 *)itfc >= next
             && (UINT8 *)itfc < end
             && USB_ENDPOINT_DESCRIPTOR_TYPE == ep->bDescriptorType
             && sizeof(USB_ENDPOINT_DESCRIPTOR) == ep->bLength
             && USB_INTERFACE_DESCRIPTOR_TYPE == itfc->bDescriptorType
             && sizeof(USB_INTERFACE_DESCRIPTOR) == itfc->bLength )
    {
        // Possible location of next endpoint descriptor is right after
        // the current endpoint descriptor
        ep = &ep[1];
    }
    else
    {
        // If ep or itfc are bad
        return FALSE;
    }

    // While still within the configuration descriptor
    while( (const UINT8 *)ep < end )
    {
        // Check for interfaces
        if( USB_INTERFACE_DESCRIPTOR_TYPE == ep->bDescriptorType
            && sizeof(USB_INTERFACE_DESCRIPTOR) == ep->bLength )
        {
            itfc = (const USB_INTERFACE_DESCRIPTOR *)ep;      // Remember the interface
        }
        // If current points to an endpoint descriptor
        else if( USB_ENDPOINT_DESCRIPTOR_TYPE == ep->bDescriptorType
            && sizeof(USB_ENDPOINT_DESCRIPTOR) == ep->bLength )
        {
            // Found next endpoint descriptor
            return TRUE;
        }

        // For all configuration descriptors, the first byte is its size.
        // Use this to find the next descriptor in the list
        next = (const UINT8 *)ep;
        next += ep->bLength;
        ep = (const USB_ENDPOINT_DESCRIPTOR *)next;
    }

    // If we've run past the end of the configuration descriptor,
    // then there are no more endpoints
    return FALSE;
}

//--//

STREAM_DRIVER_DETAILS* USB1_driver_details( UINT32 handle )
{   
    static STREAM_DRIVER_DETAILS details = { 
        SYSTEM_BUFFERED_IO, 
        NULL, 
        NULL, 
        1024, 
        1024, 
        TRUE, 
        TRUE, 
        FALSE 
    };
    
    return &details;
}

int USB1_read( char* buffer, size_t size )
{
    return USB_Read( ConvertCOM_UsbStream(USB1), buffer, size );
}

int USB1_write( char* buffer, size_t size )
{
    return USB_Write( ConvertCOM_UsbStream(USB1), buffer, size );
}

