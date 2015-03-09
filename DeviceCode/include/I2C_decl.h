////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _DRIVERS_I2C_DECL_H_
#define _DRIVERS_I2C_DECL_H_ 1

//--//

typedef UINT8 I2C_WORD;

//--//

struct I2C_USER_CONFIGURATION
{
    UINT16         Address;
    UINT32         ClockRate;
};

//--//

struct I2C_HAL_XACTION_UNIT
{   
    Hal_Queue_UnknownSize<I2C_WORD> m_dataQueue;
    size_t                          m_bytesTransferred;
    size_t                          m_bytesToTransfer;
    BOOL                            m_fRead;

    //--//
    
    void Initialize( I2C_WORD* src, I2C_WORD* dst, size_t size, BOOL fRead );      

    __inline void CopyBuffer( I2C_WORD* data, size_t length )
    {
        ASSERT(data);
        
        if(length > m_bytesTransferred)
        {
            length = m_bytesTransferred; 
        }
        
        for( ; length > 0; --length)
        {
            *data = *m_dataQueue.Pop(); ++data;
        }
    }
    
    __inline size_t TransferredBytes()
    {
        return m_bytesTransferred;
    }

    __inline BOOL IsReadXActionUnit()
    {
        return m_fRead; 
    }    
};

struct I2C_HAL_XACTION : HAL_CONTINUATION
{
    static const UINT8 c_Status_Idle       = 0x01;
    static const UINT8 c_Status_Scheduled  = 0x02;
    static const UINT8 c_Status_Processing = 0x04;
    static const UINT8 c_Status_Completed  = 0x08;
    static const UINT8 c_Status_Aborted    = 0x10;
    static const UINT8 c_Status_Cancelled  = 0x20;

    //--//

    I2C_HAL_XACTION_UNIT** m_xActionUnits;
    size_t                 m_numXActionUnits;
    size_t                 m_current;
    UINT8                  m_clockRate;     // primary clock factor to generate the i2c clock
    UINT8                  m_address;
    UINT8                  m_status;
    UINT8                  m_clockRate2;   // additional clock factors, if more than one is needed for the clock (optional)
    //--//

    void Initialize( I2C_USER_CONFIGURATION& config, I2C_HAL_XACTION_UNIT** xActionUnits, size_t numXActions );

    void Signal    ( UINT8 state, BOOL signal = TRUE );

    void SetCallback( HAL_CALLBACK_FPN xActionCompleted )
    {
        InitializeCallback( xActionCompleted, this );
    }

    __inline void SetState( UINT8 state )
    {
        m_status = state;
    }
    
    __inline UINT8 GetState()
    {
        return m_status;
    }

    __inline BOOL CheckState( UINT8 states )
    {
        return ((m_status & states) != 0) ? TRUE : FALSE;
    }

    __inline size_t TransactedBytes()
    {
        size_t bytes = 0; 
        
        for(UINT32 unit = 0; unit < m_numXActionUnits; ++unit)
        {
            bytes += m_xActionUnits[ unit ]->TransferredBytes();
        }

        return bytes;
    }

    
    __inline bool ProcessingLastUnit()
    {
        return m_current == m_numXActionUnits;
    }
};

//--//

BOOL I2C_Initialize               (                                       );
BOOL I2C_Uninitialize             (                                       );
BOOL I2C_Enqueue                  ( I2C_HAL_XACTION* xAction              );
void I2C_Cancel                   ( I2C_HAL_XACTION* xAction, bool signal );
void I2C_InitializeTransaction    ( I2C_HAL_XACTION* xAction, I2C_USER_CONFIGURATION& config, I2C_HAL_XACTION_UNIT** xActions, size_t numXActions );
void I2C_InitializeTransactionUnit( I2C_HAL_XACTION_UNIT* xActionUnit, I2C_WORD* src, I2C_WORD* dst, size_t size, BOOL fRead                      );

void   I2C_XAction_SetState       ( I2C_HAL_XACTION* xAction, UINT8 state                            );
UINT8  I2C_XAction_GetState       ( I2C_HAL_XACTION* xAction                                         );
BOOL   I2C_XAction_CheckState     ( I2C_HAL_XACTION* xAction, UINT8 state                            );
size_t I2C_XAction_TransactedBytes( I2C_HAL_XACTION* xAction                                         );
void   I2C_XActionUnit_CopyBuffer ( I2C_HAL_XACTION_UNIT* xActionUnit, I2C_WORD* data, size_t length );
BOOL   I2C_XActionUnit_IsRead     ( I2C_HAL_XACTION_UNIT* xActionUnit                                );

//--//

BOOL  I2C_Internal_Initialize  (                                              );
BOOL  I2C_Internal_Uninitialize(                                              );
void  I2C_Internal_XActionStart( I2C_HAL_XACTION* xAction, bool repeatedStart );
void  I2C_Internal_XActionStop (                                              );
void  I2C_Internal_GetClockRate( UINT32 rateKhz, UINT8& clockRate, UINT8& clockRate2);
void  I2C_Internal_GetPins     ( GPIO_PIN& scl, GPIO_PIN& sda                 );

//--//

#endif // _DRIVERS_I2C_DECL_H_
