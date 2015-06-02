////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "sddl.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

using namespace Microsoft::SPOT::Emulator;

struct EmuSerialPortEvent
{
    UINT32 PortIndex;
    void * DataContext;
    void * ErrorContext;
    
    PFNUsartEvent UsartDataEventCallback;
    PFNUsartEvent UsartErrorEventCallback;
};

const UINT32 INVALID_PORT_INDEX = (UINT32)-1;

static struct EmuSerialPortEvent s_EmuUsartState[16] =
{
    { INVALID_PORT_INDEX, NULL, NULL, NULL, NULL },
    { INVALID_PORT_INDEX, NULL, NULL, NULL, NULL },
    { INVALID_PORT_INDEX, NULL, NULL, NULL, NULL },
    { INVALID_PORT_INDEX, NULL, NULL, NULL, NULL },
    { INVALID_PORT_INDEX, NULL, NULL, NULL, NULL },
    { INVALID_PORT_INDEX, NULL, NULL, NULL, NULL },
    { INVALID_PORT_INDEX, NULL, NULL, NULL, NULL },
    { INVALID_PORT_INDEX, NULL, NULL, NULL, NULL },
    { INVALID_PORT_INDEX, NULL, NULL, NULL, NULL },
    { INVALID_PORT_INDEX, NULL, NULL, NULL, NULL },
    { INVALID_PORT_INDEX, NULL, NULL, NULL, NULL },
    { INVALID_PORT_INDEX, NULL, NULL, NULL, NULL },
    { INVALID_PORT_INDEX, NULL, NULL, NULL, NULL },
    { INVALID_PORT_INDEX, NULL, NULL, NULL, NULL },
    { INVALID_PORT_INDEX, NULL, NULL, NULL, NULL },
    { INVALID_PORT_INDEX, NULL, NULL, NULL, NULL },
};

static bool s_HandlerInitialized = false;

BOOL USART_Initialize( int ComPortNum, int BaudRate, int Parity, int DataBits, int StopBits, int FlowValue )
{
    return EmulatorNative::GetISerialDriver()->Initialize(ComPortNum, BaudRate, Parity, DataBits, StopBits, FlowValue);
}

BOOL USART_Uninitialize( int ComPortNum )
{
    return EmulatorNative::GetISerialDriver()->Uninitialize( ComPortNum );    
}

int USART_Write( int ComPortNum, const char* Data, size_t size )
{       
    return EmulatorNative::GetISerialDriver()->Write( ComPortNum, (System::IntPtr)(void*)Data, (int)size );     
}

int USART_Read( int ComPortNum, char* Data, size_t size )
{
    return EmulatorNative::GetISerialDriver()->Read( ComPortNum, (System::IntPtr)(void*)Data, (int)size );         
}

BOOL USART_Flush( int ComPortNum )
{
    return EmulatorNative::GetISerialDriver()->Flush( ComPortNum );    
}

BOOL USART_AddCharToRxBuffer( int ComPortNum, char c )
{
    return EmulatorNative::GetISerialDriver()->AddCharToRxBuffer( ComPortNum, c );
}

BOOL USART_RemoveCharFromTxBuffer( int ComPortNum, char& c )
{
    return EmulatorNative::GetISerialDriver()->RemoveCharFromTxBuffer( ComPortNum, (wchar_t %)c);
}

INT8 USART_PowerSave( int ComPortNum, INT8 Enable )
{
    return EmulatorNative::GetISerialDriver()->PowerSave( ComPortNum, Enable ); 
}

void USART_PrepareForClockStop()
{
    EmulatorNative::GetISerialDriver()->PrepareForClockStop();
}

void USART_ClockStopFinished()
{
    EmulatorNative::GetISerialDriver()->ClockStopFinished();    
}

void USART_CloseAllPorts()
{
    EmulatorNative::GetISerialDriver()->CloseAllPorts();    
}

int  USART_BytesInBuffer( int ComPortNum, BOOL fRx )
{
    return EmulatorNative::GetISerialDriver()->BytesInBuffer( ComPortNum, (fRx == TRUE) );
}

void USART_DiscardBuffer( int ComPortNum, BOOL fRx )
{
    EmulatorNative::GetISerialDriver()->DiscardBuffer( ComPortNum, (fRx == TRUE) );
}

UINT32 CPU_USART_PortsCount()
{
    return EmulatorNative::GetISerialDriver()->PortsCount();
}

void CPU_USART_GetPins( int ComPortNum, GPIO_PIN& rxPin, GPIO_PIN& txPin,GPIO_PIN& ctsPin, GPIO_PIN& rtsPin )
{   
    EmulatorNative::GetISerialDriver()->GetPins(ComPortNum, rxPin, txPin, ctsPin, rtsPin );
}

BOOL CPU_USART_SupportNonStandardBaudRate ( int ComPortNum )
{
    return EmulatorNative::GetISerialDriver()->SupportNonStandardBaudRate(ComPortNum);
}

void CPU_USART_GetBaudrateBoundary( int ComPortNum, UINT32& maxBaudrateHz, UINT32& minBaudrateHz )
{
    EmulatorNative::GetISerialDriver()->BaudrateBoundary(ComPortNum, maxBaudrateHz, minBaudrateHz);
}

BOOL CPU_USART_IsBaudrateSupported( int ComPortNum, UINT32 & BaudrateHz )
{   
    return EmulatorNative::GetISerialDriver()->IsBaudrateSupported(ComPortNum, BaudrateHz);
}

BOOL USART_ConnectEventSink( int ComPortNum, int EventType, void* pContext, PFNUsartEvent pfnUsartEvtHandler, void** ppArg )
{
    if(ComPortNum < 0 || ComPortNum >= ARRAYSIZE(s_EmuUsartState) || ComPortNum >= (int)CPU_USART_PortsCount()) return FALSE;

    s_EmuUsartState[ComPortNum].PortIndex = ComPortNum;
    
    if(ppArg != NULL) *ppArg = (void*)ComPortNum;
    
    if(EventType == USART_EVENT_TYPE_DATA)
    {
        s_EmuUsartState[ComPortNum].UsartDataEventCallback = pfnUsartEvtHandler;
        s_EmuUsartState[ComPortNum].DataContext = pContext;
    }
    else if(EventType == USART_EVENT_TYPE_ERROR)
    {
        s_EmuUsartState[ComPortNum].UsartErrorEventCallback = pfnUsartEvtHandler;
        s_EmuUsartState[ComPortNum].ErrorContext = pContext;
    }
    else
    {
        return FALSE;
    }

    if(!s_HandlerInitialized)
    {
        EmulatorNative::GetISerialDriver()->SetDataEventHandler(ComPortNum, (IntPtr)USART_SetEvent );
        s_HandlerInitialized = true;
    }
        
    return TRUE;
}

void USART_SetEvent( int ComPortNum, unsigned int event )
{
    if((ComPortNum < 0) || (ComPortNum >= ARRAYSIZE(s_EmuUsartState)) ||(ComPortNum >= (int)CPU_USART_PortsCount())) return;

    // Inorder to reduce the number of methods, we combine Error events and Data events in native code
    // and the event codes go from 0 to 6 (0-4 for error events and 5-6 for data events).
    // In managed code the event values come from to separate enums (one for errors and one for data events)
    // Therefore the managed values are 0-4 for the error codes and 0-1 for data events.  This code transforms
    // the monolithic event codes for native code into the two separate sets for managed code
    // (USART_EVENT_DATA_CHARS is the first data event code).
    if(event < USART_EVENT_DATA_CHARS)
    {
        if(s_EmuUsartState[ComPortNum].UsartErrorEventCallback != NULL)
        {
            s_EmuUsartState[ComPortNum].UsartErrorEventCallback( s_EmuUsartState[ComPortNum].ErrorContext, event );
        }
    }
    else
    {
        if(s_EmuUsartState[ComPortNum].UsartDataEventCallback != NULL)
        {
            // convert the data event codes to 0-1 (expected by managed code)
            s_EmuUsartState[ComPortNum].UsartDataEventCallback( s_EmuUsartState[ComPortNum].DataContext, event - USART_EVENT_DATA_CHARS);
        }
    }
}

