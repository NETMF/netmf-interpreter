////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "usart.h"

//--//

// XOFF bit flags
static const INT8 XOFF_FLAG_FULL  = 0x01;
static const INT8 XOFF_CLOCK_HALT = 0x02;

//--//


BOOL USART_Initialize( int ComPortNum, int BaudRate, int Parity, int DataBits, int StopBits, int FlowValue )
{
    return USART_Driver::Initialize( ComPortNum, BaudRate, Parity, DataBits, StopBits, FlowValue );
}

BOOL USART_Uninitialize( int ComPortNum )
{
    return USART_Driver::Uninitialize( ComPortNum );
}

int USART_Write( int ComPortNum, const char* Data, size_t size )
{
    return USART_Driver::Write( ComPortNum, Data, size );
}

int USART_Read( int ComPortNum, char* Data, size_t size )
{
    return USART_Driver::Read( ComPortNum, Data, size );
}

BOOL USART_Flush( int ComPortNum )
{
    return USART_Driver::Flush( ComPortNum );
}

BOOL USART_AddCharToRxBuffer( int ComPortNum, char c )
{
    return USART_Driver::AddCharToRxBuffer( ComPortNum, c );
}

BOOL USART_RemoveCharFromTxBuffer( int ComPortNum, char& c )
{
    return USART_Driver::RemoveCharFromTxBuffer( ComPortNum, c );
}

INT8 USART_PowerSave( int ComPortNum, INT8 Enable )
{
    return USART_Driver::PowerSave( ComPortNum, Enable );
}

void USART_PrepareForClockStop()
{
    USART_Driver::PrepareForClockStop();
}

void USART_ClockStopFinished()
{
    USART_Driver::ClockStopFinished();
}

void USART_ForceXON(int ComPortNum) 
{
    USART_Driver::SendXON( ComPortNum, XOFF_FLAG_FULL );
}

void USART_CloseAllPorts()
{   
    USART_Driver::CloseAllPorts();
}

int  USART_BytesInBuffer( int ComPortNum, BOOL fRx )
{
    return USART_Driver::BytesInBuffer( ComPortNum, fRx );
}

void USART_DiscardBuffer( int ComPortNum, BOOL fRx )
{
    USART_Driver::DiscardBuffer( ComPortNum, fRx );
}

BOOL USART_ConnectEventSink( int ComPortNum, int EventType, void *pContext, PFNUsartEvent pfnUsartEvtHandler, void **ppArg )
{
    return USART_Driver::ConnectEventSink( ComPortNum, EventType, pContext, pfnUsartEvtHandler, ppArg );
}

void USART_SetEvent( int ComPortNum, unsigned int event )
{
    USART_Driver::SetEvent( ComPortNum, event );
}


//--//

#define TX_USART_BUFFER_SIZE_DEBUG   16
#define RX_USART_BUFFER_SIZE_DEBUG   16
#define TX_USART_BUFFER_SIZE_HI_VOL  4096
#define RX_USART_BUFFER_SIZE_HI_VOL  512

//--//

#define USART_BUFFER_HIGH_WATER_MARK(x) (((x) * 3) / 4)
#define USART_BUFFER_LOW_WATER_MARK(x)  (((x) * 1) / 4)

#ifdef  PLATFORM_DEPENDENT_TX_USART_BUFFER_SIZE
#define TX_USART_BUFFER_SIZE    PLATFORM_DEPENDENT_TX_USART_BUFFER_SIZE
#else
#define TX_USART_BUFFER_SIZE    512
#endif

#ifdef  PLATFORM_DEPENDENT_RX_USART_BUFFER_SIZE
#define RX_USART_BUFFER_SIZE    PLATFORM_DEPENDENT_RX_USART_BUFFER_SIZE
#else
#define RX_USART_BUFFER_SIZE    512
#endif

//--//

#if TOTAL_USART_PORT == 0
UINT8 TxBuffer_Com[1];     // Allows compile to complete although in this case it won't be linked
UINT8 RxBuffer_Com[1];
#else
UINT8 TxBuffer_Com[TX_USART_BUFFER_SIZE * TOTAL_USART_PORT];
UINT8 RxBuffer_Com[RX_USART_BUFFER_SIZE * TOTAL_USART_PORT];
#endif

//--//

#define USART_FLAG_STATE(x,y)       (x.Flag & y) 
#define SET_USART_FLAG(x,y)         x.Flag |= y
#define CLEAR_USART_FLAG(x,y)       x.Flag &= ~y
#define IS_POWERSAVE_ENABLED(x)     (x.Flag & HAL_USART_STATE::c_POWERSAVE)
#define IS_USART_INITIALIZED(x)     (x.Flag & HAL_USART_STATE::c_INITIALIZED)

// RX_XOFF is a 2 bit operation
#define RX_XOFF_STATE(x)            ((x.Flag & HAL_USART_STATE::c_RX_XOFFSTATE)>>HAL_USART_STATE::c_RX_XOFFSTATE_SHIFT)
#define CLEAR_RX_XOFF_STATE(x,y)    x.Flag &= ~(y << HAL_USART_STATE::c_RX_XOFFSTATE_SHIFT)
#define SET_RX_XOFF_STATE(x,y)      x.Flag |= (y << HAL_USART_STATE::c_RX_XOFFSTATE_SHIFT)


struct HAL_USART_STATE
{
    Hal_Queue_UnknownSize< UINT8 > TxQueue;
    volatile UINT32                Flag;
    static const UINT32            c_TX_BUFFERXOFF    =0x1;
    static const UINT32            c_TX_BUFFERXON     =0x2;
    static const UINT32            c_TX_SWFLOW_CTRL   =0x4;
    static const UINT32            c_TX_XON_STATE     =0x8;
    static const UINT32            c_RX_SWFLOW_CTRL   =0x100;
    static const UINT32            c_RX_XOFFSTATE     =0x600;
    static const UINT32            c_RX_HWFLOW_CTRL   =0x800;
    static const UINT32            c_RX_HWFLOW_OFF    =0x1000;
    static const UINT32            c_RX_XOFFSTATE_SHIFT = 9;
    static const UINT32            c_POWERSAVE        =0x10000;
    static const UINT32            c_INITIALIZED      =0x20000;

    volatile UINT64                TicksStartTxXOFF;

    Hal_Queue_UnknownSize< UINT8 > RxQueue;
    size_t                         RxBufferHighWaterMark;
    size_t                         RxBufferLowWaterMark;

    BOOL fDataEventSet;
 
    UINT32 PortIndex;
    void * DataContext;
    void * ErrorContext;
    
    PFNUsartEvent UsartDataEventCallback;
    PFNUsartEvent UsartErrorEventCallback;
};

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata = "Hal_Usart_State"
#endif

#if TOTAL_USART_PORT == 0
HAL_USART_STATE Hal_Usart_State[1];     // Allows compile to complete although in this case it won't be linked
#else
HAL_USART_STATE Hal_Usart_State[TOTAL_USART_PORT];
#endif

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata
#endif

//--//

BOOL USART_Driver::Initialize( int ComPortNum, int BaudRate, int Parity, int DataBits, int StopBits, int FlowValue )
{
    NATIVE_PROFILE_PAL_COM();

    if((ComPortNum < 0) || (ComPortNum >= TOTAL_USART_PORT))
    {
        //DEBUG_TRACE1(TRACE_ALWAYS, "ERROR: VTE_USART_Initialize: invalid ComPortNum %u\r\n", ComPortNum);
        return FALSE;
    }

    {
        GLOBAL_LOCK(irq);

        HAL_USART_STATE& State = Hal_Usart_State[ComPortNum];

        if(!IS_USART_INITIALIZED(State))
        {
            State.fDataEventSet = FALSE;
            State.PortIndex     = ComPortNum;

            // DO NOT INITIALIZE EVENT CALLBACKS HERE BECAUSE THEY CAN BE SET PRIOR TO INITIALIZE

            State.Flag                      = 0;                //clear all the flag bits
            SET_USART_FLAG(State, HAL_USART_STATE::c_INITIALIZED | HAL_USART_STATE::c_TX_XON_STATE);

            // If SW flow control on input is enabled
            if (FlowValue & USART_FLOW_SW_IN_EN)
            {
                SET_USART_FLAG(State, HAL_USART_STATE::c_RX_SWFLOW_CTRL);
            }

            // If SW flow control on output is enabled
            if (FlowValue & USART_FLOW_SW_OUT_EN)
            {
                SET_USART_FLAG(State, HAL_USART_STATE::c_TX_SWFLOW_CTRL);
            }

            // If HW flow control on input is enabled (HW output control is either totally in hardware or in HAL only)
            if (FlowValue & USART_FLOW_HW_IN_EN)
            {
                SET_USART_FLAG(State, HAL_USART_STATE::c_RX_HWFLOW_CTRL);
            }

            State.RxBufferHighWaterMark = USART_BUFFER_HIGH_WATER_MARK( RX_USART_BUFFER_SIZE );
            State.RxBufferLowWaterMark  = USART_BUFFER_LOW_WATER_MARK ( RX_USART_BUFFER_SIZE );

            State.TicksStartTxXOFF      = 0;

            State.TxQueue.Initialize( &TxBuffer_Com[ComPortNum * TX_USART_BUFFER_SIZE], TX_USART_BUFFER_SIZE);
            State.RxQueue.Initialize( &RxBuffer_Com[ComPortNum * RX_USART_BUFFER_SIZE], RX_USART_BUFFER_SIZE );

            return CPU_USART_Initialize( ComPortNum, BaudRate, Parity, DataBits, StopBits, FlowValue );
        }

        return TRUE;
    }
}


BOOL USART_Driver::Uninitialize( int ComPortNum )
{
    NATIVE_PROFILE_PAL_COM();
    if((ComPortNum < 0) || (ComPortNum >= TOTAL_USART_PORT))
    {
        return FALSE;
    }

    {
        GLOBAL_LOCK(irq);

        HAL_USART_STATE& State = Hal_Usart_State[ComPortNum];

        if (IS_USART_INITIALIZED(State))
        {
            State.fDataEventSet  = FALSE;
            State.PortIndex = ComPortNum;

            CLEAR_USART_FLAG(State,HAL_USART_STATE::c_INITIALIZED);

            return CPU_USART_Uninitialize( ComPortNum );
        }

        return TRUE;
    }
}


int USART_Driver::Write( int ComPortNum, const char* Data, size_t size )
{
    NATIVE_PROFILE_PAL_COM();
    int         totWrite = 0;
    const char* ptr      = Data;
    int j;

    if((ComPortNum < 0) || (ComPortNum >= TOTAL_USART_PORT)) {ASSERT(FALSE); return -1;}
    if(0    == size                                        )                 return -1;
    if(NULL == Data                                        ) {ASSERT(FALSE); return -1;}

    HAL_USART_STATE& State = Hal_Usart_State[ComPortNum];

    if (IS_POWERSAVE_ENABLED(State) || (!IS_USART_INITIALIZED(State)))return -1;

    if (USART_FLAG_STATE(State, HAL_USART_STATE::c_TX_SWFLOW_CTRL) && (!USART_FLAG_STATE(State, HAL_USART_STATE::c_TX_XON_STATE)))
    {   
        // A timeout is used for XOFF incase the XOFF value was lost or never sent.
        // USART_TX_XOFF_TIMEOUT_TICKS is defined in the platform selector file
        if((USART_TX_XOFF_TIMEOUT_INFINITE != USART_TX_XOFF_TIMEOUT_TICKS              ) &&
           (HAL_Time_CurrentTicks() - State.TicksStartTxXOFF) > USART_TX_XOFF_TIMEOUT_TICKS)
        {
            SET_USART_FLAG(State, HAL_USART_STATE::c_TX_XON_STATE);
        }
        else
        {
            return 0;
        }
    }

    // loop twice if needed because of our implementaition of a circular buffered QUEUE
    for(j=0; (j < 2) && (totWrite < size); j++)
    {
        // Keep interrupts off to keep queue access atomic
        GLOBAL_LOCK(irq);
        UINT8 *Dst;
        size_t nWritten;

        nWritten = size - totWrite;
        Dst = State.TxQueue.Push( nWritten );
        if( Dst != NULL )
        {
            memcpy(Dst, ptr, nWritten); // Move characters to transmit queue from buffer
            totWrite += nWritten;
            ptr      += nWritten;
        }
        else if(!USART_FLAG_STATE(State, HAL_USART_STATE::c_RX_HWFLOW_CTRL))
        {
            SetEvent( ComPortNum, USART_EVENT_ERROR_TXFULL );
            break;
        }
    }

    // we need to be atomic on PowerSave/USART_TxBufferEmptyInterruptEnable
    // since it gets set/cleared from ISR, and disables the clock, but
    // USART_TxBufferEmptyInterruptEnable turns the clock back on!
    // We don't want to turn on the USART clock if in power save mode
    {
        GLOBAL_LOCK(irq);

        if(size && !IS_POWERSAVE_ENABLED(State))
        {
            // if we added chars, enable interrupts so characters will actually start flowing
            // we could do this early, then we race each iteration, and could cause a stall,
            // so we do this once to be efficient in the common case (buffer has room for all chars)
            CPU_USART_TxBufferEmptyInterruptEnable( ComPortNum, TRUE );
        }
    }

    return totWrite;
}


int USART_Driver::Read( int ComPortNum, char* Data, size_t size )
{
    NATIVE_PROFILE_PAL_COM();
    if((ComPortNum < 0) || (ComPortNum >= TOTAL_USART_PORT)) {ASSERT(FALSE); return -1;}
    if(Data == NULL                                        )                 return -1;

    HAL_USART_STATE& State = Hal_Usart_State[ComPortNum];

    if ( IS_POWERSAVE_ENABLED(State) || (!IS_USART_INITIALIZED(State))) return -1;


    int CharsRead = 0;

    while(CharsRead < size)
    {
        // keep interrupts off only during pointer movement so we are atomic
        GLOBAL_LOCK(irq);
        size_t toRead;
        UINT8 *Src;

        toRead = size - CharsRead;
        Src = State.RxQueue.Pop( toRead );
        if( NULL == Src )
            break;

        // Check if FIFO level has just passed or gotten down to the low water mark
        if(State.RxQueue.NumberOfElements() <= State.RxBufferLowWaterMark && (State.RxQueue.NumberOfElements() + toRead) > State.RxBufferLowWaterMark)
        {
            if( USART_FLAG_STATE(State, HAL_USART_STATE::c_RX_SWFLOW_CTRL) )
            {
                // Clear our XOFF state
                SendXON( ComPortNum, XOFF_FLAG_FULL );
            }
            if( USART_FLAG_STATE(State, HAL_USART_STATE::c_RX_HWFLOW_CTRL) )
            {
                CPU_USART_RxBufferFullInterruptEnable(ComPortNum, TRUE);
            }
        }
        memcpy(&Data[CharsRead], Src, toRead);   // Copy data from queue to Read buffer
        CharsRead += toRead;
    }

    {
        GLOBAL_LOCK(irq);
     
        State.fDataEventSet  = FALSE;        

        if(!State.RxQueue.IsEmpty())
        {
            SetEvent( ComPortNum, USART_EVENT_DATA_CHARS );
        }
     }

    return CharsRead;
}


BOOL USART_Driver::Flush( int ComPortNum )
{
    NATIVE_PROFILE_PAL_COM();
    if((ComPortNum < 0) || (ComPortNum >= TOTAL_USART_PORT)) {ASSERT(FALSE); return FALSE;}

    HAL_USART_STATE& State = Hal_Usart_State[ComPortNum];

    if ( IS_POWERSAVE_ENABLED(State) || (!IS_USART_INITIALIZED(State))) return TRUE;


    UINT32 IrqId = USART_TX_IRQ_INDEX(ComPortNum);

    if ((USART_FLAG_STATE(State, HAL_USART_STATE::c_TX_SWFLOW_CTRL) && (!USART_FLAG_STATE(State, HAL_USART_STATE::c_TX_XON_STATE)))
        || !CPU_USART_TxHandshakeEnabledState(ComPortNum))
    {
        return FALSE;
    }

    // 3 cases: IRQs off, IRQs on and TXINT off, IRQs on and TXINT on
    // treat first 2 as same (it kills ISR latency when the buffer is full)

    {
        GLOBAL_LOCK(irq);

        if(irq.WasDisabled() || !CPU_USART_TxBufferEmptyInterruptState( ComPortNum ) || 0 == CPU_INTC_InterruptEnableState( IrqId ))
        {
            while(State.TxQueue.IsEmpty() == false)
            {
                char c;

                // this could happen while we are spinning, if so drop everything and return
                if (IS_POWERSAVE_ENABLED(State) || !CPU_USART_TxHandshakeEnabledState(ComPortNum))
                {
                    return FALSE;
                }

                // wait for a place to put the character
                while(!CPU_USART_TxBufferEmpty( ComPortNum ))
                {
                    // The TxBuffer will never be empty as long as the handshake is preventing
                    // the character from being sent
                    if(!CPU_USART_TxHandshakeEnabledState(ComPortNum))
                        return FALSE;
                }
                // get next character

                RemoveCharFromTxBuffer( ComPortNum, c );

                // ok, put in Transmit holding register
                CPU_USART_WriteCharToTxBuffer( ComPortNum, c );

                // this loop waits ~100uS per character out at 115200 baud,
                // so it can take a long time to clear the whole queue
                irq.Probe();
            }
        }
        else
        {

            // all requisite interrupts on, just wait for the buffer to empty naturally
            while(State.TxQueue.IsEmpty() == false)
            {
                // this could happen while we are spinning, if so drop everything and return
                if(IS_POWERSAVE_ENABLED(State) || !CPU_USART_TxHandshakeEnabledState(ComPortNum))
                {
                    return FALSE;
                }

                irq.Release();

                // 1mSec time should be plenty = 1 characters at 9600 baud
                // 9600 bps / (8bits * 1000ms/s)
                HAL_Time_Sleep_MicroSeconds_InterruptEnabled(1000);

                // critical section queue
                irq.Acquire();
            }
        }
    }

    if(IS_POWERSAVE_ENABLED(State))
    {
        return FALSE;
    }

    // wait for the holding register to empty
    while(!CPU_USART_TxBufferEmpty( ComPortNum ) && CPU_USART_TxHandshakeEnabledState(ComPortNum));

    // also wait for shift register to empty
    while(!CPU_USART_TxShiftRegisterEmpty( ComPortNum ) && CPU_USART_TxHandshakeEnabledState(ComPortNum));
    // now, all characters have been transmitted

    return TRUE;
}

//--//

BOOL USART_Driver::AddCharToRxBuffer( int ComPortNum, char c )
{
    ASSERT_IRQ_MUST_BE_OFF();

    if((ComPortNum < 0) || (ComPortNum >= TOTAL_USART_PORT)) return FALSE;

    HAL_USART_STATE& State = Hal_Usart_State[ComPortNum];

    if (USART_FLAG_STATE(State, HAL_USART_STATE::c_TX_SWFLOW_CTRL))

    {
        switch( c )
        {
            case XOFF:
                State.TicksStartTxXOFF = HAL_Time_CurrentTicks();
                CLEAR_USART_FLAG(State, HAL_USART_STATE::c_TX_XON_STATE);
                return TRUE;
            case XON:
                SET_USART_FLAG(State, HAL_USART_STATE::c_TX_XON_STATE);
                return TRUE;
        }
    }


    {
        GLOBAL_LOCK(irq);

        UINT8* Dst = State.RxQueue.Push();

        if(Dst)
        {
            *Dst = c;

            if( State.RxQueue.NumberOfElements() >= State.RxBufferHighWaterMark )
            {
                if( USART_FLAG_STATE(State, HAL_USART_STATE::c_RX_SWFLOW_CTRL) )
                {
                    // Set our XOFF state
                    SendXOFF( ComPortNum, XOFF_FLAG_FULL );
                }
                if( USART_FLAG_STATE(State, HAL_USART_STATE::c_RX_HWFLOW_CTRL) )
                {
                    // Hold off receiving characters (should pull HW handshake automatically)
                    CPU_USART_RxBufferFullInterruptEnable(ComPortNum, FALSE);
                }
            }
        }
        
        else
        {
            SetEvent( ComPortNum, USART_EVENT_ERROR_RXOVER );
                
#if !defined(BUILD_RTM)
            lcd_printf("\fBuffer OVFLW\r\n");
            hal_printf("Buffer OVFLW\r\n");
#endif
            return FALSE;
        }
    }

    SetEvent( ComPortNum, USART_EVENT_DATA_CHARS );

    Events_Set( SYSTEM_EVENT_FLAG_COM_IN );

    return TRUE;
}

BOOL USART_Driver::RemoveCharFromTxBuffer( int ComPortNum, char& c )
{
    if((ComPortNum < 0) || (ComPortNum >= TOTAL_USART_PORT)) return FALSE;

    HAL_USART_STATE& State = Hal_Usart_State[ComPortNum];

    {
        GLOBAL_LOCK(irq);

        if (USART_FLAG_STATE(State,HAL_USART_STATE::c_TX_BUFFERXOFF))
        {
            CLEAR_USART_FLAG(State,HAL_USART_STATE::c_TX_BUFFERXOFF);
            c = XOFF;
            return TRUE;
        }

        if( USART_FLAG_STATE(State,HAL_USART_STATE::c_TX_BUFFERXON))
        {
            CLEAR_USART_FLAG(State, HAL_USART_STATE::c_TX_BUFFERXON);
            c = XON;
            return TRUE;
        }

        UINT8* Src = State.TxQueue.Pop();

        if(Src)
        {
            c = *Src;

            Events_Set(SYSTEM_EVENT_FLAG_COM_OUT);

            return TRUE;
        }

        return FALSE;
    }
}


INT8 USART_Driver::PowerSave( int ComPortNum, INT8 Enable )
{
    ASSERT_IRQ_MUST_BE_OFF();

    if((ComPortNum < 0) || (ComPortNum >= TOTAL_USART_PORT)) {ASSERT(FALSE); return 0;}

    HAL_USART_STATE& State = Hal_Usart_State[ComPortNum];

    INT8 previous = IS_POWERSAVE_ENABLED(State)? 1:0;
    
    if (Enable)
    {
        SET_USART_FLAG(State, HAL_USART_STATE::c_POWERSAVE);
    }
    else
    {
        CLEAR_USART_FLAG(State, HAL_USART_STATE::c_POWERSAVE);
    }

    return previous;
}


void USART_Driver::PrepareForClockStop()
{
    ASSERT_IRQ_MUST_BE_OFF();

    // set XOFF state and send if we haven't already
    for( int port = 0; port < TOTAL_USART_PORT; port++)
    {
        if ( USART_FLAG_STATE(Hal_Usart_State[port], HAL_USART_STATE::c_RX_SWFLOW_CTRL))
        {
            // set our XOFF state
            SendXOFF( port, XOFF_CLOCK_HALT );
        }
    }
}


void USART_Driver::ClockStopFinished()
{
    GLOBAL_LOCK(irq);

    // undo previous work

    // if we sent are in XOFF state, clear that, and if we weren't already in XOFF state at time of clock stop, set XON state
    for( int port = 0; port < TOTAL_USART_PORT; port++)
    {
        if ( USART_FLAG_STATE(Hal_Usart_State[port], HAL_USART_STATE::c_RX_SWFLOW_CTRL))
        {
            // clear our XOFF state
            SendXON( port, XOFF_CLOCK_HALT );
        }
    }
}

//--//

void USART_Driver::SendXOFF( INT32 ComPortNum, const UINT32 Flag )
{
    ASSERT_IRQ_MUST_BE_OFF();

    HAL_USART_STATE& State = Hal_Usart_State[ComPortNum];

    if(IS_POWERSAVE_ENABLED(State)) return;

    if(RX_XOFF_STATE(State) == 0)
    {
        // send the XOFF if we are the first to add the XOFF state
        SET_USART_FLAG(State, HAL_USART_STATE::c_TX_BUFFERXOFF);

        // enable interrupts to send the XOFF, but be expedient about it to improve response time
        CPU_USART_TxBufferEmptyInterruptEnable( ComPortNum, TRUE );
    }

    SET_RX_XOFF_STATE(State,Flag);
}

void USART_Driver::SendXON( INT32 ComPortNum, const UINT32 Flag )
{
    ASSERT_IRQ_MUST_BE_OFF();

    HAL_USART_STATE& State = Hal_Usart_State[ComPortNum];

    if(IS_POWERSAVE_ENABLED(State)) return;

    // clear our XOFF state
    CLEAR_RX_XOFF_STATE(State, Flag);

    if(RX_XOFF_STATE(State) == 0)
    {
        // send the XON if we are the last to remove the XOFF state
        SET_USART_FLAG(State, HAL_USART_STATE::c_TX_BUFFERXON);
            
        // enable interrupts to send the XON
        CPU_USART_TxBufferEmptyInterruptEnable( ComPortNum, TRUE );

        // coming out of XON doesn't benefit from a priority inversion like XOFF does
    }
}

void USART_Driver::CloseAllPorts()
{
    for( int port = 0; port < TOTAL_USART_PORT; port++)
    {
        if ( USART_FLAG_STATE(Hal_Usart_State[port], HAL_USART_STATE::c_INITIALIZED))
        {
            Uninitialize(port);
        }
    }
}

int USART_Driver::BytesInBuffer( int ComPortNum, BOOL fRx )
{
    if((ComPortNum < 0) || (ComPortNum >= TOTAL_USART_PORT)) return -1;

    HAL_USART_STATE& State = Hal_Usart_State[ComPortNum];

    return fRx? State.RxQueue.NumberOfElements(): State.TxQueue.NumberOfElements();
}

void USART_Driver::DiscardBuffer( int ComPortNum, BOOL fRx )
{
    if((ComPortNum < 0) || (ComPortNum >= TOTAL_USART_PORT)) return;

    HAL_USART_STATE& State = Hal_Usart_State[ComPortNum];

    {
        GLOBAL_LOCK(irq);

        // circular buffer may require 2 pops
        for(int i=0; i<2; i++)
        {
            if(fRx)
            {
                size_t nElements = State.RxQueue.NumberOfElements();
                State.RxQueue.Pop(nElements);

            }
            else
            {
                size_t nElements = State.TxQueue.NumberOfElements();
                State.TxQueue.Pop(nElements);

            }
        }

        // 
        // Re-enble RX after discarding the buffer (SW/HW handshaking)
        //
        if(fRx)
        {
            if( USART_FLAG_STATE(State, HAL_USART_STATE::c_RX_SWFLOW_CTRL) )
            {
                // Clear our XOFF state
                SendXON( ComPortNum, XOFF_FLAG_FULL );
            }
            if( USART_FLAG_STATE(State, HAL_USART_STATE::c_RX_HWFLOW_CTRL) )
            {
                CPU_USART_RxBufferFullInterruptEnable(ComPortNum, TRUE);
            }
        }        
    }
}

BOOL USART_Driver::ConnectEventSink( int ComPortNum, int EventType, void* pContext, PFNUsartEvent pfnUsartEvtHandler, void** ppArg )
{
    if((ComPortNum < 0) || (ComPortNum >= TOTAL_USART_PORT)) return FALSE;

    {
        GLOBAL_LOCK(irq);

        HAL_USART_STATE& State = Hal_Usart_State[ComPortNum];


        State.PortIndex = ComPortNum;
        
        if(ppArg != NULL) *ppArg = (void*)ComPortNum;

        if(EventType == USART_EVENT_TYPE_DATA)
        {
            State.UsartDataEventCallback = pfnUsartEvtHandler;
            State.DataContext            = pContext;
        }
        else if(EventType == USART_EVENT_TYPE_ERROR)
        {
            State.UsartErrorEventCallback = pfnUsartEvtHandler;
            State.ErrorContext            = pContext;
        }

    }


    return TRUE;
}

void USART_Driver::SetEvent( int ComPortNum, unsigned int event )
{
    if((ComPortNum < 0) || (ComPortNum >= TOTAL_USART_PORT)) return;

    {
        GLOBAL_LOCK(irq);
        
        HAL_USART_STATE& State = Hal_Usart_State[ComPortNum];

        // Inorder to reduce the number of methods, we combine Error events and Data events in native code
        // and the event codes go from 0 to 6 (0-4 for error events and 5-6 for data events).
        // In managed code the event values come from to separate enums (one for errors and one for data events)
        // Therefore the managed values are 0-4 for the error codes and 0-1 for data events.  This code transforms
        // the monolithic event codes for native code into the two separate sets for managed code
        // (USART_EVENT_DATA_CHARS is the first data event code).
        if(event < USART_EVENT_DATA_CHARS)
        {
            if(State.UsartErrorEventCallback != NULL)
            {
                State.UsartErrorEventCallback( State.ErrorContext, event );
            }
        }
        else
        {
            if(!State.fDataEventSet && State.UsartDataEventCallback != NULL)
            {
                State.fDataEventSet = TRUE;
                // convert the data event codes to 0-1 (expected by managed code)
                State.UsartDataEventCallback( State.DataContext, event - USART_EVENT_DATA_CHARS);
            }
        }
    }
}

//--//

STREAM_DRIVER_DETAILS* COM1_driver_details( UINT32 handle )
{
    static STREAM_DRIVER_DETAILS details = { 
        DRIVER_BUFFERED_IO, 
        &RxBuffer_Com[ RX_USART_BUFFER_SIZE * ConvertCOM_ComPort( COM1 ) ], 
        &TxBuffer_Com[ TX_USART_BUFFER_SIZE * ConvertCOM_ComPort( COM1 ) ], 
        RX_USART_BUFFER_SIZE, 
        TX_USART_BUFFER_SIZE, 
        TRUE, 
        TRUE, 
        FALSE 
    };
        
    return &details;
}

int COM1_read( char* buffer, size_t size )
{
    return USART_Read( ConvertCOM_ComPort( COM1 ), buffer, size );
}

int COM1_write( char* buffer, size_t size )
{
    return USART_Write( ConvertCOM_ComPort( COM1 ), buffer, size );
}

//--//

STREAM_DRIVER_DETAILS* COM2_driver_details( UINT32 handle )
{
    static STREAM_DRIVER_DETAILS details = { 
        DRIVER_BUFFERED_IO, 
        &RxBuffer_Com[ RX_USART_BUFFER_SIZE * ConvertCOM_ComPort( COM2 ) ], 
        &TxBuffer_Com[ TX_USART_BUFFER_SIZE * ConvertCOM_ComPort( COM2 ) ], 
        RX_USART_BUFFER_SIZE, 
        TX_USART_BUFFER_SIZE, 
        TRUE, 
        TRUE, 
        FALSE 
    };
    
    return &details;
}

int COM2_read( char* buffer, size_t size )
{
    return USART_Read(  ConvertCOM_ComPort( COM2 ), buffer, size );
}

int COM2_write( char* buffer, size_t size )
{
    return USART_Write(  ConvertCOM_ComPort( COM2 ), buffer, size );
}


