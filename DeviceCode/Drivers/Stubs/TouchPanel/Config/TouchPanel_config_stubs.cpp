////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata = "g_TouchPanelConfig"
#endif

SPI_CONFIGURATION g_TouchPanel_Config = { 61, false, false, false, false, 125, 1, 1, 0 };
TOUCH_PANEL_CalibrationData g_TouchPanel_Calibration_Config = {-256, -192, 390400, 319296, -1984, -1920};
TOUCH_PANEL_SamplingSettings g_TouchPanel_Sampling_Settings = {1, 2, 400, FALSE, {50, 200, 150, TOUCH_PANEL_SAMPLE_MS_TO_TICKS(50)}};


#define TOUCH_POINT_BUFFER_SIZE            4
#define TOUCH_POINT_RUNNINGAVG_BUFFER_SIZE 4

TouchPoint g_PAL_TouchPointBuffer[TOUCH_POINT_BUFFER_SIZE];
UINT32     g_PAL_TouchPointBufferSize = TOUCH_POINT_BUFFER_SIZE;
TOUCH_PANEL_Point g_PAL_RunningAvg_Buffer[TOUCH_POINT_RUNNINGAVG_BUFFER_SIZE];
UINT32     g_PAL_RunningAvg_Buffer_Size = TOUCH_POINT_RUNNINGAVG_BUFFER_SIZE;

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata
#endif
