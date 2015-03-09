////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _DRIVERS_USART_DECL_H_
#define _DRIVERS_USART_DECL_H_ 1

#define XON         17
#define XOFF        19

// Make sure these match up with the defines in SerialPort.cs
#define USART_FLOW_NONE        0x00
#define USART_FLOW_HW_IN_EN    0x02
#define USART_FLOW_HW_OUT_EN   0x04
#define USART_FLOW_SW_IN_EN    0x08
#define USART_FLOW_SW_OUT_EN   0x10


#define USART_PARITY_NONE  0
#define USART_PARITY_ODD   1
#define USART_PARITY_EVEN  2
#define USART_PARITY_MARK  3
#define USART_PARITY_SPACE 4

#define USART_STOP_BITS_NONE          0
#define USART_STOP_BITS_ONE           1
#define USART_STOP_BITS_TWO           2
#define USART_STOP_BITS_ONEPOINTFIVE  3

#define USART_COM_LOOBACK_1    997
#define USART_COM_LOOBACK_2    998

#define USART_COM_ISLOOPBACK(x) ((x == USART_COM_LOOBACK_1) || (x == USART_COM_LOOBACK_2))

#if !defined(USART_TX_XOFF_TIMEOUT_INFINITE)
#define USART_TX_XOFF_TIMEOUT_INFINITE   0xFFFFFFFF
#endif

// USART_TX_XOFF_TIMEOUT_TICKS = the number of ticks to leave TX in the XOFF state.  The 
// timeout is a failsafe so that if the XON is lost or an spurious XOFF is received the 
// TX won't be off indefinitely.
//
// The platform selector file should override this value, default to 1 min
#if !defined(USART_TX_XOFF_TIMEOUT_TICKS) 
#define USART_TX_XOFF_TIMEOUT_TICKS      (CPU_TicksPerSecond() * 60)
#endif

//--//

#define TX_USART_BUFFER_SIZE_GENERAL PLATFORM_DEPENDENT_TX_USART_BUFFER_SIZE
#define RX_USART_BUFFER_SIZE_GENERAL PLATFORM_DEPENDENT_RX_USART_BUFFER_SIZE

//--//

typedef void (*PFNUsartEvent) (void* context, unsigned int event);

#define USART_EVENT_TYPE_ERROR     1
#define USART_EVENT_ERROR_TXFULL   0x00    // The application tried to transmit a character, but the output buffer was full. 
#define USART_EVENT_ERROR_RXOVER   0x01    // An input buffer overflow has occurred. There is either no room in the input buffer, or a character was received after the end-of-file (EOF) character. 
#define USART_EVENT_ERROR_OVERRUN  0x02    // A character-buffer overrun has occurred. The next character is lost. 
#define USART_EVENT_ERROR_RXPARITY 0x03    // The hardware detected a parity error. 
#define USART_EVENT_ERROR_FRAME    0x04    // The hardware detected a framing error.

#define USART_EVENT_TYPE_DATA      2
#define USART_EVENT_DATA_CHARS     0x05    // A character was received and placed in the input buffer. 
#define USART_EVENT_DATA_EOF       0x06    // The end of file character was received and placed in the input buffer. 


BOOL USART_Initialize( int ComPortNum, int BaudRate, int Parity, int DataBits, int StopBits, int FlowValue );
BOOL USART_Uninitialize( int ComPortNum );
int  USART_Write( int ComPortNum, const char* Data, size_t size );
int  USART_Read( int ComPortNum, char* Data, size_t size );
BOOL USART_Flush( int ComPortNum );
BOOL USART_AddCharToRxBuffer( int ComPortNum, char c );
BOOL USART_RemoveCharFromTxBuffer( int ComPortNum, char& c );
INT8 USART_PowerSave( int ComPortNum, INT8 Enable );
void USART_PrepareForClockStop();
void USART_ClockStopFinished();
void USART_ForceXON( int ComPortNum );
void USART_CloseAllPorts();
int  USART_BytesInBuffer( int ComPortNum, BOOL fRx );
void USART_DiscardBuffer( int ComPortNum, BOOL fRx );
BOOL USART_ConnectEventSink( int ComPortNum, int EventType, void* pContext, PFNUsartEvent pfnUsartEvtHandler, void** ppArg );
void USART_SetEvent( int ComPortNum, unsigned int event );

//--//

BOOL CPU_USART_Initialize                  ( int ComPortNum, int BaudRate, int Parity, int DataBits, int StopBits, int FlowValue );
BOOL CPU_USART_Uninitialize                ( int ComPortNum               );
BOOL CPU_USART_TxBufferEmpty               ( int ComPortNum               );
BOOL CPU_USART_TxShiftRegisterEmpty        ( int ComPortNum               );
void CPU_USART_WriteCharToTxBuffer         ( int ComPortNum, UINT8 c      );
void CPU_USART_TxBufferEmptyInterruptEnable( int ComPortNum, BOOL Enable  );
BOOL CPU_USART_TxBufferEmptyInterruptState ( int ComPortNum               );
void CPU_USART_RxBufferFullInterruptEnable ( int ComPortNum, BOOL Enable  );
BOOL CPU_USART_RxBufferFullInterruptState  ( int ComPortNum               );
BOOL CPU_USART_TxHandshakeEnabledState     ( int comPort                  );
void CPU_USART_ProtectPins                 ( int ComPortNum, BOOL On      );
UINT32 CPU_USART_PortsCount                (                              );
void CPU_USART_GetPins                     ( int ComPortNum, GPIO_PIN& rxPin, GPIO_PIN& txPin,GPIO_PIN& ctsPin, GPIO_PIN& rtsPin );
void CPU_USART_GetBaudrateBoundary         ( int ComPortNum, UINT32 & maxBaudrateHz, UINT32 & minBaudrateHz );
BOOL CPU_USART_SupportNonStandardBaudRate  ( int ComPortNum               );
BOOL CPU_USART_IsBaudrateSupported         ( int ComPortNum, UINT32& BaudrateHz );

//--//

#endif // _DRIVERS_USART_DECL_H_

