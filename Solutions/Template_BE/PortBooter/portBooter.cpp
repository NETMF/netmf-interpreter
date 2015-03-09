////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <stdio.h>
#include <string.h>
#include <ctype.h>

#include <tinyhal.h>

#define BUTTON_UP   BUTTON_B2
#define BUTTON_ENTR BUTTON_B4
#define BUTTON_DOWN BUTTON_B5

////////////////////////////////////////////////////////////////////////////////
// OEM modified this session for their platforms if needed.
#define PROGRAM_WORD_CHECK  0xE321F0DF

// if program needs to loaded from non-XIP to XIP area, use this routine
// and return the new address for the applicatoin.
void PortBooterLoadProgram(void ** StartAddress)
{
    hal_printf(" Prepare to launcah program %x \r\n", (UINT32)(*StartAddress));
    // if needed, reload data to the desired ram type storage.

}


////////////////////////////////////////////////////////////////////////////////

//--//

extern const ADS_PACKED USB_DYNAMIC_CONFIGURATION UsbDefaultConfiguration;

//--//

struct State
{
    static const int MAX_PROGRAMS = 8;

    BOOL                WaitForActivity;
    INT32               WaitInterval;

    UINT32              Programs[MAX_PROGRAMS];
    UINT32              ProgramCount;

    BOOL                SerialPortActive;
    COM_HANDLE          pStreamOutput;
    COM_HANDLE          UsartPort;

    BOOL                UsingUsb;
    COM_HANDLE          UsbPort;
    int                 UsbEventCode;

    //--//

    void Initialize()
    {
#if defined(PLATFORM_ARM_MOTE2)
        if (COM_IsUsb(HalSystemConfig.DebugTextPort))
            WaitInterval = 5000;        // The USB port must be selected and the Start button pressed all during this time
        else
            WaitInterval = 2000;        // The USART port may be selected before powering on the Imote2, so it takes less time
#else
        WaitInterval     = 2000;
#endif

        SerialPortActive = FALSE;

        UsartPort        = USART_DEFAULT_PORT;

        UsingUsb         = FALSE;
        UsbPort          = USB1;
        UsbEventCode     = USB_DEBUG_EVENT_IN;

        //--//

        // wait an extra second for buttons and COM port to stabilize
        Events_WaitForEvents( 0, 1000 );

        // COM init is now delayed for TinyBootloader, so we need to initialize it here
        CPU_InitializeCommunication();

        //--//

        // set default baud rate
        if(UsartPort != DEBUG_TEXT_PORT)
        {
            DebuggerPort_Initialize( UsartPort );
        }
        // extra time to allow USB setup, so that we can see the printf
        Events_WaitForEvents( 0, 1000 );


        hal_printf( "PortBooter v%d.%d.%d.%d\r\n", VERSION_MAJOR, VERSION_MINOR, VERSION_BUILD, VERSION_REVISION);
        hal_printf( "Build Date: %s %s\r\n",  __DATE__, __TIME__);

#if defined(__GNUC__)
        hal_printf( "GNU Compiler version %d\r\n", __GNUC__);
#elif defined(__ADSPBLACKFIN__)
        hal_printf( "Blackfin Compiler version %d\r\n", __VERSIONNUM__ );
#else
        hal_printf( "ARM Compiler version %d\r\n", __ARMCC_VERSION);
#endif
       
        //--//

        BlockStorageStream stream;
        FLASH_WORD ProgramWordCheck;

        if(!stream.Initialize(BlockUsage::CODE)) return;



        do
        {
            do
            {
            
                UINT32 addr = stream.CurrentAddress();
    			FLASH_WORD *pWord = &ProgramWordCheck;

                stream.Read( (BYTE**)&pWord, sizeof(FLASH_WORD) );

                if(*pWord == PROGRAM_WORD_CHECK)
                {
                    hal_printf("*** nXIP Program found at 0x%08x\r\n", addr );
    				Programs[ProgramCount++] = (UINT32)addr;
                }

                if(ProgramCount == MAX_PROGRAMS) break;
            }
            while( stream.Seek( BlockStorageStream::STREAM_SEEK_NEXT_BLOCK, BlockStorageStream::SeekCurrent ) );

        } while(stream.NextStream());

    }

};

State g_State;

void SignalActivity( void )
{
    g_State.WaitForActivity = FALSE;

    if(g_State.SerialPortActive) return;

    g_State.SerialPortActive = TRUE;

    hal_printf( "Receiving upload of SREC file...\r\n" );

    LCD_Clear();
    hal_fprintf( STREAM_LCD, "\fFlashing...\r\n\r\n" );
}

void StartApplication( void (*StartAddress)() )
{
    PortBooterLoadProgram((void**)&StartAddress);

    hal_printf( "Starting main application at 0x%08x\r\n", (size_t)StartAddress );

    LCD_Clear();

    USART_Flush( ConvertCOM_ComPort(g_State.UsartPort) );

    if(g_State.UsingUsb)
    {
        USB_Flush( ConvertCOM_UsbStream( g_State.UsbPort ) );
        USB_CloseStream( ConvertCOM_UsbStream(g_State.UsbPort) );
        USB_Uninitialize( ConvertCOM_UsbController(g_State.UsbPort) );     //disable the USB for the next application
    }

    DISABLE_INTERRUPTS();

    LCD_Uninitialize();

    CPU_DisableCaches();

    (*StartAddress)();

}

//--//

struct SREC_Handler
{
    int    m_Pos;
    char   m_LineBuffer[256];
    BOOL   m_Failures;
    UINT32 m_StartAddress;

    BlockStorageDevice *pDevice ;

    BOOL Process( char c )
    {
        switch(c)
        {
        case XON:
        case XOFF:
            // swallow these and go on
            break;

        case '\r':
        case '\n':
            {
                // terminate string!
                m_LineBuffer[m_Pos] = 0;

                if(ParseLine( m_LineBuffer ))
                {
                    if(m_StartAddress)
                    {
                        if(!m_Failures)
                        {
                            StartApplication( (void (*)())m_StartAddress );
                        }
                        else
                        {
                            // after getting a failed complete download, clear the error and start anew
                            m_StartAddress = 0;
                            m_Failures     = FALSE;

                            LCD_Clear();
                            hal_fprintf( STREAM_LCD, "\fFlashing...\r\n\r\n" );
                        }
                    }
                }
                else
                {
                    // bad characters! realign and don't exec
                    m_Failures = TRUE;

                    hal_fprintf( STREAM_LCD, "%12s\r\n", m_LineBuffer );
                }

                m_Pos = 0;

                return FALSE; // Always align again after getting a full record.
            }
            break;

        default:
            {
                if(!((c < 0x80) && (c > 0x20)))
                {
                    // bad characters!
                    m_Failures  = TRUE;
                    m_Pos       = 0;

                    hal_fprintf( STREAM_LCD, "\r\nL:%d\r\n", c );

                    return FALSE; // Realign.
                }
                else
                {
                    // if we have a non whitespace character, store it
                    m_LineBuffer[m_Pos++] = c;

                    // don't overrun stuff in case of bizarre failures that appear OK char by char
                    if(m_Pos >= sizeof(m_LineBuffer))
                    {
                        m_Failures  = TRUE;
                        m_Pos       = 0;

                        hal_fprintf( STREAM_LCD, "OVERFLOW\r\n" );

                        return FALSE; // Realign.
                    }
                }
            }
            break;
        }

        return TRUE;
    }

private:

    BOOL ParseLine( const char* SRECLine )
    {
        UINT32               Address;
        int                  i;
        UINT32               BytesRemaining = 0;
        UINT32               Temp           = 0;
        UINT32               Data32;
        UINT32               RecordType;
        UINT32               CheckSum;
        UINT32               WordCount = 0;
        UINT32               ByteCount = 0;
        FLASH_WORD           WordData[16/sizeof(FLASH_WORD)];     // we support 16 bytes of data per record max, 4 words, 8 shorts
        UINT8                ByteData[16];                        // we support 16 bytes of data per record max, 4 words, 8 shorts

        BlockStorageDevice         *pDevice;
        UINT32               m_size;
        ByteAddress WriteByteAddress;


        RecordType = *SRECLine++;

        SRECLine = htoi( SRECLine, 2, BytesRemaining ); if(!SRECLine) return FALSE;

        // start the checksum with this byte
        CheckSum = BytesRemaining;

        // get the destination address
        // do bytes only to make checksum calc easier
        Data32 = 0;
        for(i = 0; i < 4; i++)
        {
            SRECLine = htoi( SRECLine, 2, Temp ); if(!SRECLine) return FALSE;

            CheckSum += Temp;

            Data32 <<= 8;
            Data32  |= Temp;

            BytesRemaining -= 1;
        }
        Address = Data32;

        // take off one byte for CRC;
        m_size = BytesRemaining -1;

        switch(RecordType)
        {
        case '3':
            {

                while(BytesRemaining > 1)
                {
                    // get bytes into words, and checksum
                    Data32 = 0;
                    for(i = 0; i < sizeof(FLASH_WORD); i++)
                    {
                        SRECLine = htoi( SRECLine, 2, Temp ); if(!SRECLine) return FALSE;

                        CheckSum += Temp;

                        Data32 |= Temp << (i*8);    // little endian format

                        ByteData[ByteCount++] = Temp;

                        BytesRemaining -= 1;

                        // leave the checksum in place
                        if(1 == BytesRemaining) break;
                    }

                    ASSERT(WordCount < (16/sizeof(FLASH_WORD)));

                    WordData[WordCount++] = Data32;
                }
            }
            break;

        case '7':
            // just a return address (starting location)
            m_StartAddress = (UINT32)Address;
            break;

        default:
            // we only support S3 and S7 records, for now
            return FALSE;
        }

        // get the checksum
        SRECLine = htoi( SRECLine, 2, Temp ); if(!SRECLine) return FALSE;

        CheckSum += Temp;

        BytesRemaining -= 1;

        ASSERT(0 == BytesRemaining);

        // make sure we had a NULL terminator for line, and not more characters
        if(*SRECLine != 0)
        {
            return FALSE;
        }

        if(0xff != (CheckSum & 0xff))
        {
            return FALSE;
        }
        else
        {
            // only write if we succeeded the checksum entirely for whole line

            if(m_size > 0)
            {
                SignalActivity();

                // this slows things down to print every address, only print once per line

                hal_fprintf( STREAM_LCD, "WR: 0x%08x\r", (UINT32)Address );

                if (BlockStorageList::FindDeviceForPhysicalAddress( &pDevice, Address, WriteByteAddress)) 
                {

                        UINT32 regionIndex, rangeIndex;
                        const BlockDeviceInfo* deviceInfo = pDevice->GetDeviceInfo() ;

                        if(!(pDevice->FindRegionFromAddress(WriteByteAddress, regionIndex, rangeIndex))) 
                        {
#if !defined(BUILD_RTM)
                            hal_printf(" Invalid condition - Fail to find the block number from the ByteAddress %x \r\n",WriteByteAddress);  
#endif
                            return FALSE;
                        }

                        // start from the block where the sector sits.
                        UINT32        iRegion = regionIndex;
                        UINT32        accessPhyAddress = (UINT32)Address;
                                
                        BYTE*         bufPtr           = (BYTE*)ByteData;
                        BOOL          success          = TRUE;
                        INT32         writeLenInBytes  = m_size;


                        while (writeLenInBytes > 0)
                        {
                            for(; iRegion < deviceInfo->NumRegions; iRegion++)
                            {
                                const BlockRegionInfo *pRegion = &(deviceInfo->Regions[iRegion]);

                                ByteAddress blkAddr = pRegion->Start;

                                while(blkAddr < accessPhyAddress)
                                {
                                    blkAddr += pRegion->BytesPerBlock;
                                }

                                //writeMaxLength =the current largest number of bytes can be read from the block from the address to its block boundary.
                                UINT32 NumOfBytes = __min(pRegion->BytesPerBlock, writeLenInBytes);
                                if (accessPhyAddress == blkAddr && !pDevice->IsBlockErased(blkAddr, pRegion->BytesPerBlock))
                                {
                                    hal_fprintf( STREAM_LCD, "ER: 0x%08x\r", blkAddr );
                                    pDevice->EraseBlock(blkAddr);
                                    blkAddr += pRegion->BytesPerBlock;
                                }
                                success = pDevice->Write(accessPhyAddress , NumOfBytes, (BYTE *)bufPtr, FALSE);

                                writeLenInBytes -= NumOfBytes;

                                if ((writeLenInBytes <=0) || (!success)) break;

                                bufPtr += NumOfBytes;

                            }

                            if ((writeLenInBytes <=0) || (!success)) break;
                        }
                        
                    }
                else
                {
                    FLASH_WORD *Addr = (FLASH_WORD *) Address;
                    for(i = 0; i < WordCount; i++)
                    {
                        // must be RAM but don't verify, we write anyway, possibly causing a data abort if the address is bogus
                         *Addr++ = WordData[i];
                    }
                }
            }
        }

        return TRUE;
    }

    //--//

    static const char* htoi( const char* hexstring, UINT32 length, UINT32& value )
    {
        // internal limitation of result size right now, big enough for debug software
        ASSERT(length <= 4);

        UINT32 result = 0;

        while(length--)
        {
            char c = *hexstring++;

            if     (c >= '0' && c <= '9') c -= '0';
            else if(c >= 'a' && c <= 'f') c -= 'a' - 10;
            else if(c >= 'A' && c <= 'F') c -= 'A' - 10;
            else                          return NULL;

            result = (result << 4) + c;
        }

        value = result;

        return hexstring;
    }
};

SREC_Handler g_SREC;

//--//

struct XREC_Handler
{
    UINT32 m_address;
    UINT16 m_size;
    UINT32 m_crc;
    UINT8  m_data[256];

    int    m_phase;
    char*  m_ptr;
    int    m_len;

    BOOL Process( UINT8 c )
    {
        while(true)
        {
            switch(m_phase)
            {
                //
                // Setup 'address' reception.
                //
            case 0:
                m_ptr = (char*)&m_address;
                m_len = sizeof(m_address);
                m_phase++;
                break;
    
                //
                // Setup 'size' reception.
                //
            case 2:
                //printf( "Got address %08x\r\n", m_address );
    
                m_ptr = (char*)&m_size;
                m_len = sizeof(m_size);
                m_phase++;
                return TRUE;
    
                //
                // Setup 'crc' reception.
                //
            case 4:
                //printf( "Got size %08x\r\n", m_size );
    
                m_ptr = (char*)&m_crc;
                m_len = sizeof(m_crc);
                m_phase++;
                return TRUE;
    
                //
                // Setup 'data' reception or jump to entrypoint.
                //
            case 6:
                //printf( "Got crc %08x\r\n", m_crc );
    
                m_crc += m_address;
                m_crc += m_size;
    
                if(m_size == 0)
                {
                    if(m_crc != 0)
                    {
                        hal_fprintf( g_State.pStreamOutput, "X crc %08x %08x\r\n", m_address, m_crc );
    
                       // bad characters! realign
                        m_phase = 0;
                        return FALSE;
                    }
    
                    hal_fprintf( g_State.pStreamOutput, "X start %08x\r\n", m_address );

#if defined(PLATFORM_ARM_MOTE2)
                    CPU_GPIO_SetPinState(LED1_GREEN, LED_OFF);        // Turn off Green LED for iMote2
#endif                
    
                    StartApplication( (void (*)())m_address );
                }
    
                if(m_size > sizeof(m_data) || (m_size % sizeof(FLASH_WORD)) != 0)
                {
                    hal_fprintf( g_State.pStreamOutput, "X size %d\r\n", m_size );
    
                    // bad characters! realign
                    m_phase = 0;
                    return FALSE;
                }
    
                m_ptr = (char*)m_data;
                m_len =        m_size;
                m_phase++;
                return TRUE;
    
            case 8:
                {
                    FLASH_WORD* src     = (FLASH_WORD*)m_data;
                    FLASH_WORD* dst     = (FLASH_WORD*)m_address;
                    BOOL        success = TRUE;
                    int         i;
                    BlockStorageDevice * pDevice;
                    ByteAddress WriteByteAddress ;
    
                    for(i=0; i<m_size; i++)
                    {
                        m_crc += m_data[i];
                    }
    
                    if(m_crc != 0)
                    {
                        hal_fprintf( g_State.pStreamOutput, "X crc %08x %08x\r\n", m_address, m_crc );
    
                        // bad characters! realign
                        m_phase = 0;
                        return FALSE;
                    }
    
                    SignalActivity();

                    // this slows things down to print every address, only print once per line
                    hal_fprintf( STREAM_LCD, "WR: 0x%08x\r", (UINT32)dst );

                    // if it not Block Device, assume it is RAM 
                    if (BlockStorageList::FindDeviceForPhysicalAddress( & pDevice, m_address, WriteByteAddress))
                    {
                        UINT32 regionIndex, rangeIndex;
                        const BlockDeviceInfo* deviceInfo = pDevice->GetDeviceInfo() ;

                        if(!(pDevice->FindRegionFromAddress(WriteByteAddress, regionIndex, rangeIndex))) 
                        {
#if !defined(BUILD_RTM)
                           hal_printf(" Invalid condition - Fail to find the block number from the ByteAddress %x \r\n",WriteByteAddress);  
#endif
                            return FALSE;
                        }

                        // start from the block where the sector sits.
                        UINT32        iRegion = regionIndex;
                        UINT32        accessPhyAddress = (UINT32)m_address;
                                
                        BYTE*         bufPtr           = (BYTE*)src;
                        BOOL          success          = TRUE;
                        INT32         writeLenInBytes  = m_size;


                        while (writeLenInBytes > 0)
                        {
                            for(; iRegion < deviceInfo->NumRegions; iRegion++)
                            {
                                const BlockRegionInfo *pRegion = &(deviceInfo->Regions[iRegion]);

                                ByteAddress blkAddr = pRegion->Start;

                                while(blkAddr < accessPhyAddress)
                                {
                                    blkAddr += pRegion->BytesPerBlock;
                                }

                                //writeMaxLength =the current largest number of bytes can be read from the block from the address to its block boundary.
                                UINT32 NumOfBytes = __min(pRegion->BytesPerBlock, writeLenInBytes);
                                if (accessPhyAddress == blkAddr && !pDevice->IsBlockErased(blkAddr, pRegion->BytesPerBlock))
                                {
                                    hal_fprintf( STREAM_LCD, "ER: 0x%08x\r", blkAddr );
                                    pDevice->EraseBlock(blkAddr);
                                    blkAddr += pRegion->BytesPerBlock;
                                }

                                success = pDevice->Write(accessPhyAddress , NumOfBytes, (BYTE *)bufPtr, FALSE);

                                writeLenInBytes -= NumOfBytes;
								
                                if ((writeLenInBytes <=0) || (!success)) break;

                                bufPtr += NumOfBytes;

                            }

                            if ((writeLenInBytes <=0) || (!success)) break;
                        }
                        
                    }
                    else
                    {
                        // must be RAM but don't verify, we write anyway, possibly causing a data abort if the address is bogus
                        memcpy( dst, src, m_size );
                    }
    
                    hal_fprintf( g_State.pStreamOutput, "X %s %08x\r\n", success ? "ack" : "nack", m_address );
    
                    m_phase = 0;
                    return FALSE;
                }
                break;
    
                //
                // Read data.
                //
            case 1:
            case 3:
            case 5:
            case 7:
                *m_ptr++ = c; if(--m_len) return TRUE;
    
                m_phase++;
                break;
            }
        }
    }
};

XREC_Handler g_XREC;

//--//

HAL_DECLARE_NULL_HEAP();

void ApplicationEntryPoint()
{
    UINT32 ComEvent;

    g_State.Initialize();

    ComEvent = 0;
    if(COM_IsSerial(g_State.UsartPort) && (g_State.UsartPort != COM_NULL))
    {
        ComEvent = SYSTEM_EVENT_FLAG_COM_IN;
    }

#if !defined(TARGETLOCATION_RAM)
    g_State.WaitForActivity = (g_State.ProgramCount != 0); // is there a main app?

#else
    g_State.WaitForActivity = FALSE;      // forever
#endif

    {
        UINT32 ButtonsPressed;
        UINT32 ButtonsReleased;
        char   c;

        // clear any events present from startup
        while(Events_Get( SYSTEM_EVENT_FLAG_ALL ));

        // clear any junk from com port buffers
        while(DebuggerPort_Read( g_State.UsartPort, &c, sizeof(c) ));

        // clear any junk from button buffer
        while(Buttons_GetNextStateChange( ButtonsPressed, ButtonsReleased ));
    }

    {
        BOOL       ProcessingSREC     = FALSE;
        BOOL       ProcessingXREC     = FALSE;
        INT32      ProcessingZENFLASH = 0;
        INT32      Mode               = 0;
        INT32      MenuChoice         = 1;      // main application location when USB not enabled
        BOOL       MenuUpdate         = TRUE;
        UINT32     USBEvent           = 0;
        COM_HANDLE ReadPort           = COM_NULL;
        int        MenuOffset         = 1;      // no USB enabled
        INT64      WaitTimeout        = 0;

#if defined(PLATFORM_ARM_MOTE2)
        CPU_GPIO_SetPinState( LED1_GREEN, LED_ON );
#endif

        if(g_State.WaitForActivity)
        {
            WaitTimeout = HAL_Time_CurrentTime() + (INT64)g_State.WaitInterval * (10 * 1000);

            hal_printf( "Waiting %d.%03d second(s) for hex upload\r\n", g_State.WaitInterval / 1000, g_State.WaitInterval % 1000 );
        }
        else
        {
            hal_printf( "Waiting forever for hex upload\r\n" );
        }

        // checking the existence of Usb Driver, all the default values are set to no USB
        if( USB_DEVICE_STATE_NO_CONTROLLER != USB_GetStatus( ConvertCOM_UsbController(g_State.UsbPort) ) )
        {
            g_State.UsingUsb = TRUE;

            MenuChoice = 2;
            MenuOffset = 2;
        }


        while(true)
        {
            if(MenuUpdate)
            {
                MenuUpdate = FALSE;

                LCD_Clear();
                hal_fprintf( STREAM_LCD, "\f");

                switch(Mode)
                {
                case 0:
                    hal_fprintf( STREAM_LCD, "   Zen Boot\r\n\r\n" );

                    hal_fprintf( STREAM_LCD, "%c PortBooter\r\n", MenuChoice == 0 ? '*' : ' ' );

                    if(g_State.UsingUsb)
                    {
                        hal_fprintf( STREAM_LCD, "%c FlashUSB\r\n", MenuChoice == 1 ? '*' : ' ' );
                    }


                    for(int i = 0; i < g_State.ProgramCount; i++)
                    {
                        hal_fprintf( STREAM_LCD, "%c Prg:%08x\r\n", (i+MenuOffset) == MenuChoice ? '*' : ' ', g_State.Programs[i] );
                    }
                    break;

                case 1:
                    hal_fprintf( STREAM_LCD, "PortBooter v%d.%d.%d.%d\r\n", VERSION_MAJOR, VERSION_MINOR, VERSION_BUILD, VERSION_REVISION);
                    hal_fprintf( STREAM_LCD, "Waiting forever for hex upload\r\n"         );
                    break;

                case 2:
                    hal_fprintf( STREAM_LCD, "FlashUSB\r\n"                       );
                    hal_fprintf( STREAM_LCD, "Waiting forever for hex upload\r\n" );
                    break;
                }
            }

            UINT32 Events = Events_WaitForEvents( ComEvent | SYSTEM_EVENT_FLAG_BUTTON | SYSTEM_EVENT_FLAG_USB_IN, 2000 );

            if(Events & SYSTEM_EVENT_FLAG_BUTTON)
            {
                UINT32 ButtonsPressed;
                UINT32 ButtonsReleased;

                Events_Clear( SYSTEM_EVENT_FLAG_BUTTON );

                while(Buttons_GetNextStateChange( ButtonsPressed, ButtonsReleased ));
                {
                    if(g_State.SerialPortActive == FALSE)
                    {
                        //printf("%02x %02x\r\n", ButtonsPressed, ButtonsReleased);

                        // up
                        if(ButtonsPressed & BUTTON_UP)
                        {
                            switch(Mode)
                            {
                            case 0:
                                MenuChoice = __max( MenuChoice-1, 0 );
                                break;
                            }

                            g_State.WaitForActivity = FALSE;
                            MenuUpdate              = TRUE;
                        }

                        // down
                        if(ButtonsPressed & BUTTON_DOWN)
                        {
                            switch(Mode)
                            {
                            case 0:
                                MenuChoice = __min( MenuChoice+1, g_State.ProgramCount + MenuOffset-1 );
                                break;
                            }

                            g_State.WaitForActivity = FALSE;
                            MenuUpdate              = TRUE;
                        }

                        // enter button
                        if(ButtonsPressed & BUTTON_ENTR)
                        {
                            switch(Mode)
                            {
                            case 0:
                                if(MenuChoice == 0)
                                {
                                    Mode = 1;

                                    //UsingUsb = FALSE;
                                }
                                else if(g_State.UsingUsb && MenuChoice == 1)
                                {
                                    Mode = 2;
                                    USB_Configure( ConvertCOM_UsbController(g_State.UsbPort), &UsbDefaultConfiguration );
                                    USB_Initialize( ConvertCOM_UsbController(g_State.UsbPort) );
                                    USB_OpenStream( ConvertCOM_UsbStream(g_State.UsbPort), USB_DEBUG_EP_WRITE, USB_DEBUG_EP_READ );
                                    //UsingUsb = TRUE;
                                }
                                else
                                {
                                    StartApplication( (void (*)())g_State.Programs[MenuChoice-MenuOffset] );
                                }
                                break;

                            case 1:
                                Mode = 0;
                                break;

                            case 2:
                                // USB_Uninitialize();
                                Mode = 0;
                                break;
                            }

                            g_State.WaitForActivity = FALSE;
                            MenuUpdate              = TRUE;
                        }

                        if(ButtonsReleased)
                        {
                            MenuUpdate = TRUE;
                        }
                    }
                }
            }

            if((Events & ComEvent) || (Events & SYSTEM_EVENT_FLAG_USB_IN))
            {
                char c;

                if(Events & ComEvent)
                {
                    Events_Clear( ComEvent );

                    ReadPort              = g_State.UsartPort;
                    g_State.pStreamOutput = ReadPort;
                }
                else
                {
                    USBEvent = USB_GetEvent( ConvertCOM_UsbController(g_State.UsbPort), USB_EVENT_ALL );
                    if( !(USBEvent & g_State.UsbEventCode) )
                        continue;

                    g_State.pStreamOutput = g_State.UsbPort;
                    ReadPort              = g_State.UsbPort;
                }

                while(DebuggerPort_Read( ReadPort, &c, sizeof(c) ))
                {
                    if(ProcessingSREC)
                    {
                        ProcessingSREC = g_SREC.Process( c );
                    }
                    else if(ProcessingXREC)
                    {
                        ProcessingXREC = g_XREC.Process( c );
                    }
                    else if(ProcessingZENFLASH)
                    {
                        const char Signature[] = "ZENFLASH\r";

                        //printf( "Got %d at %d\r\n", c, ProcessingZENFLASH );
                        if(Signature[ProcessingZENFLASH++] == c)
                        {
                            if(Signature[ProcessingZENFLASH] == 0)
                            {
                                SignalActivity();
                                ProcessingZENFLASH = 0;
                            }
                        }
                        else
                        {
                            ProcessingZENFLASH = 0;
                        }
                    }
                    else if('S' == c)
                    {
                        ProcessingSREC = TRUE;
                    }
                    else if('X' == c)
                    {
                        ProcessingXREC = TRUE;
                    }
                    else if('Z' == c)
                    {
                        ProcessingZENFLASH = 1;
                    }
                }
            }

            if(g_State.WaitForActivity && WaitTimeout < HAL_Time_CurrentTime())
            {
#if defined(PLATFORM_ARM_MOTE2)
                CPU_GPIO_SetPinState(LED1_GREEN, LED_OFF);        // Turn off Green LED for iMOTE2
#endif
                // we didn't see anything on serial port for wait interval (2 seconds nominally),
                // continue with code - just run the normal application
                StartApplication( (void (*)())g_State.Programs[0] );
            }

        }
    }
}
