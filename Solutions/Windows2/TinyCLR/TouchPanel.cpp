////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"


using namespace Microsoft::SPOT::Emulator;

#define TOUCH_POINT_BUFFER_SIZE 200
#define TOUCH_POINT_RUNNINGAVG_BUFFER_SIZE 4

TOUCH_SPI_CONFIGURATION g_Emulator_Config = { {100, false, false, false, false, 125, 1, 1, 0}, 100};
TOUCH_PANEL_CalibrationData g_TouchPanel_Calibration_Config = {1, 1, 0, 0, 1, 1};
TOUCH_PANEL_SamplingSettings g_TouchPanel_Sampling_Settings = {0, 1, 500*500, FALSE, {50, 200, 100, TOUCH_PANEL_SAMPLE_MS_TO_TICKS(50)}}; // the emulator is "perfect" hardware, so we don't need to ignore readings

TouchPoint g_PAL_TouchPointBuffer[TOUCH_POINT_BUFFER_SIZE];
UINT32     g_PAL_TouchPointBufferSize = TOUCH_POINT_BUFFER_SIZE;
TOUCH_PANEL_Point g_PAL_RunningAvg_Buffer[TOUCH_POINT_RUNNINGAVG_BUFFER_SIZE];
UINT32     g_PAL_RunningAvg_Buffer_Size = TOUCH_POINT_RUNNINGAVG_BUFFER_SIZE;

/// Touch Panel HAL Driver methods for emulator.

BOOL HAL_TOUCH_PANEL_Enable( GPIO_INTERRUPT_SERVICE_ROUTINE touchIsrProc )
{
    if (!::CPU_GPIO_EnableInputPin2(    
            g_Emulator_Config.InterruptPin,
            true,
            touchIsrProc,
            NULL,
            GPIO_INT_EDGE_BOTH,
            RESISTOR_PULLDOWN))
    {
        return false;
    }

    return true;
}

BOOL HAL_TOUCH_PANEL_Disable()
{
    CPU_GPIO_DisablePin( g_Emulator_Config.InterruptPin, RESISTOR_DISABLED, 0, GPIO_ALT_PRIMARY);

    return true;
}

void HAL_TOUCH_PANEL_GetPoint( TOUCH_PANEL_SAMPLE_FLAGS* pTipState, int* pSource, int* pUnCalX, int* pUnCalY )
{
    int tipState = 0;
    int x = 0;
    int y = 0;
    int source = 0;

    EmulatorNative::GetITouchPanelDriver()->TouchPanel_GetPoint(
        (int %)tipState,
        (int %)source,
        (int %)x,
        (int %)y
        );

    *pTipState = tipState;
    *pUnCalX = x;
    *pUnCalY = y;
    *pSource = source;
}

bool TSP_CalibrationPointGet(TOUCH_PANEL_CALIBRATION_POINT *pTCP)
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

        // SetLastError(ERROR_INVALID_PARAMETER);
        return (FALSE);
    }
    
    return (TRUE);
}


HRESULT HAL_TOUCH_PANEL_GetDeviceCaps(unsigned int iIndex, void* lpOutput)
{
    if ( lpOutput == NULL )
    {
        return (FALSE);
    }

    switch ( iIndex )
    {
    case TOUCH_PANEL_SAMPLE_RATE_ID:
        {
            TOUCH_PANEL_SAMPLE_RATE   *pTSR = (TOUCH_PANEL_SAMPLE_RATE*)lpOutput;    

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
        return(TSP_CalibrationPointGet((TOUCH_PANEL_CALIBRATION_POINT*)lpOutput));

    default:        
        return (FALSE);
    }

    return (TRUE);
}


