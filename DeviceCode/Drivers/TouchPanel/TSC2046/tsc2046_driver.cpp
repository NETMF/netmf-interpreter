////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tsc2046_driver.h>

extern TOUCH_SPI_CONFIGURATION g_TSC2046_Config;
extern TOUCH_PANEL_SamplingSettings g_TouchPanel_Sampling_Settings;

BOOL TSC2046_Driver::Enable( GPIO_INTERRUPT_SERVICE_ROUTINE touchIsrProc )
{     
    if (!::CPU_GPIO_ReservePin( g_TSC2046_Config.SpiConfiguration.DeviceCS, TRUE ))
    {
        return FALSE;
    }    

    ::CPU_GPIO_EnableOutputPin( g_TSC2046_Config.SpiConfiguration.DeviceCS, TRUE );    

    CPU_GPIO_SetDebounce(3);

    if (!::CPU_GPIO_EnableInputPin2(    
            g_TSC2046_Config.InterruptPin,
            TRUE,
            touchIsrProc,
            NULL,
            GPIO_INT_EDGE_BOTH,
            RESISTOR_PULLUP))
    {        
        return FALSE;
    }    

    ::CPU_SPI_Initialize();

    return TRUE;
}

BOOL TSC2046_Driver::Disable()
{
    CPU_GPIO_DisablePin( g_TSC2046_Config.InterruptPin, RESISTOR_DISABLED, 0, GPIO_ALT_PRIMARY);
    
    return true;
}



void TSC2046_Driver::GetPoint( TOUCH_PANEL_SAMPLE_FLAGS* pTipState, int* pSource, int* pUnCalX, int* pUnCalY )
{
    *pTipState = 0;
    *pUnCalX = 0;
    *pUnCalY = 0;
    *pSource = 0;

    static bool  stylusDown = false;

    /// Apparently there's a lot of noise from the touch hardware. We will take several
    /// independent measures to compensate for them:
    /// 1. Settle down time (instead of reading right away, wait few moments) --> ReadsToIgnore
    /// 2. Read multiple samples (read a number of them, and then take average) --> ReadsPerSample.
    ///

    UINT16 i = 0;
    UINT16 totalReads = g_TouchPanel_Sampling_Settings.ReadsToIgnore + g_TouchPanel_Sampling_Settings.ReadsPerSample;

    int x = -1;
    int y = -1;

    INT32 validReadCount = 0;

    UINT32 d1 = 0xFFFF;
    UINT32 d2 = 0;

    for(i = 0; i < totalReads; i++)
    {    
        static unsigned char writeBufferX[4] = {0x90, 0, 0, 0};
        static unsigned char readBuffer[3] =  {0, 0, 0};

        d2 = d1;
        ::CPU_SPI_nWrite8_nRead8(g_TSC2046_Config.SpiConfiguration, writeBufferX, 4, readBuffer, 3, 1);
    
        d1 = readBuffer[0];                
        d1 <<= 8;
        d1 |=  readBuffer[1];
        d1 >>= 3;    

        if (d1 == d2)
            break;
    }

    y = d1;

    d1 = 0xFFFF;
    d2 = 0;
    for(i = 0; i < g_TouchPanel_Sampling_Settings.ReadsPerSample; i++)
    {
        static unsigned char writeBufferY[4] = {0xD0, 0, 0, 0};
        static unsigned char readBuffer[3] =  {0, 0, 0};

        d2 = d1;
        ::CPU_SPI_nWrite8_nRead8(g_TSC2046_Config.SpiConfiguration, writeBufferY, 4, readBuffer, 3, 1);
    
        d1 = readBuffer[0];                
        d1 <<= 8;
        d1 |=  readBuffer[1];
        d1 >>= 3;                 

        if (d1 == d2)
            break;
    }

    x = d1;

    if (x >= 3700)
    {
        validReadCount = 0;
    }
    else
    {
        validReadCount = 1;
    }

    
    if (stylusDown)
    {
        *pTipState |= TouchSamplePreviousDownFlag;
    }

    if (validReadCount > 0)
    {
        *pTipState |= TouchSampleValidFlag;
        *pUnCalX = x;
        *pUnCalY = y;
        *pTipState |= TouchSampleDownFlag;
        stylusDown = true;
    }
    else
    {
        *pUnCalX = -1;
        *pUnCalY = -1;
        stylusDown = false;       
    }    
}

BOOL TSC2046_Driver::CalibrationPointGet(TOUCH_PANEL_CALIBRATION_POINT *pTCP)
{

    INT32   cDisplayWidth  = pTCP->cDisplayWidth;
    INT32   cDisplayHeight = pTCP->cDisplayHeight;

    int CalibrationRadiusX = cDisplayWidth  / 20;
    int CalibrationRadiusY = cDisplayHeight / 20;

    switch (pTCP -> PointNumber)
    {
    case    0:
        pTCP->CalibrationX = cDisplayWidth  / 2;
        pTCP->CalibrationY = cDisplayHeight / 2;
        break;

    case    1:
        pTCP->CalibrationX = CalibrationRadiusX * 2;
        pTCP->CalibrationY = CalibrationRadiusY * 2;
        break;

    case    2:
        pTCP->CalibrationX = CalibrationRadiusX * 2;
        pTCP->CalibrationY = cDisplayHeight - CalibrationRadiusY * 2;
        break;

    case    3:
        pTCP->CalibrationX = cDisplayWidth  - CalibrationRadiusX * 2;
        pTCP->CalibrationY = cDisplayHeight - CalibrationRadiusY * 2;
        break;

    case    4:
        pTCP->CalibrationX = cDisplayWidth - CalibrationRadiusX * 2;
        pTCP->CalibrationY = CalibrationRadiusY * 2;
        break;

    default:
        pTCP->CalibrationX = cDisplayWidth  / 2;
        pTCP->CalibrationY = cDisplayHeight / 2;
        
        return FALSE;
    }
    
    return TRUE;
}


HRESULT TSC2046_Driver::GetDeviceCaps(unsigned int iIndex, void* lpOutput)
{
    if ( lpOutput == NULL )
    {
        return FALSE;
    }

    switch ( iIndex )
    {
    case TOUCH_PANEL_SAMPLE_RATE_ID:
        {
            TOUCH_PANEL_SAMPLE_RATE  *pTSR  = (TOUCH_PANEL_SAMPLE_RATE*)lpOutput;    

            pTSR->SamplesPerSecondLow       = g_TouchPanel_Sampling_Settings.SampleRate.SamplesPerSecondLow;
            pTSR->SamplesPerSecondHigh      = g_TouchPanel_Sampling_Settings.SampleRate.SamplesPerSecondHigh;
            pTSR->CurrentSampleRateSetting  = g_TouchPanel_Sampling_Settings.SampleRate.CurrentSampleRateSetting;
            pTSR->MaxTimeForMoveEvent_ticks = g_TouchPanel_Sampling_Settings.SampleRate.MaxTimeForMoveEvent_ticks;
        }
        break; 

    case TOUCH_PANEL_CALIBRATION_POINT_COUNT_ID:
        {
            TOUCH_PANEL_CALIBRATION_POINT_COUNT *pTCPC = (TOUCH_PANEL_CALIBRATION_POINT_COUNT*)lpOutput;            

            pTCPC->flags              = 0;
            pTCPC->cCalibrationPoints = 5;
        }
        break;

    case TOUCH_PANEL_CALIBRATION_POINT_ID:        
        return(CalibrationPointGet((TOUCH_PANEL_CALIBRATION_POINT*)lpOutput));

    default:        
        return FALSE;
    }

    return TRUE;
}

