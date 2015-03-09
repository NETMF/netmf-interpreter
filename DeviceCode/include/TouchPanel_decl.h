////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <CPU_GPIO_decl.h>
#include <CPU_SPI_decl.h>

#ifndef _DRIVERS_TOUCHPANEL_DECL_H_
#define _DRIVERS_TOUCHPANEL_DECL_H_ 1

//--//

//--//  Touch Panel Driver  //--//

typedef UINT32 TOUCH_PANEL_SAMPLE_FLAGS;
#define TOUCH_PANEL_SAMPLE_RATE_LOW             100
#define TOUCH_PANEL_SAMPLE_RATE_HIGH            100

#define TOUCH_PANEL_SAMPLE_RATE_ID             0
#define TOUCH_PANEL_SAMPLE_MS_TO_TICKS(x) (x * TIME_CONVERSION__TO_MILLISECONDS)
struct TOUCH_PANEL_SAMPLE_RATE
{
    INT32   SamplesPerSecondLow;
    INT32   SamplesPerSecondHigh;
    INT32   CurrentSampleRateSetting;
    INT32   MaxTimeForMoveEvent_ticks;
};

typedef UINT32 TOUCH_PANEL_CALIBRATION_FLAGS;

#define TOUCH_PANEL_CALIBRATION_POINT_COUNT_ID	1
struct TOUCH_PANEL_CALIBRATION_POINT_COUNT
{
    TOUCH_PANEL_CALIBRATION_FLAGS   flags;
    INT32                           cCalibrationPoints;
};

#define TOUCH_PANEL_CALIBRATION_POINT_ID       2
struct TOUCH_PANEL_CALIBRATION_POINT
{
    INT32   PointNumber;
    INT32   cDisplayWidth;
    INT32   cDisplayHeight;
    INT32   CalibrationX;
    INT32   CalibrationY;
};


enum TouchPanel_SampleFlags 
{
  TouchSampleValidFlag = 0x01,
  TouchSampleDownFlag = 0x02,
  TouchSampleIsCalibratedFlag = 0x04,
  TouchSamplePreviousDownFlag = 0x08,
  TouchSampleIgnore = 0x10,
  TouchSampleMouse = 0x40000000
};

enum TouchPanel_StylusFlags
{
    TouchPanelStylusInvalid = 0x0,
    TouchPanelStylusDown    = 0x1,
    TouchPanelStylusUp      = 0x2,
    TouchPanelStylusMove    = 0x3,
};

enum TouchPanel_TouchCollectorFlags
{
    TouchCollector_EnableCollection     = 0x1,
    TouchCollector_DirectInk            = 0x2,
    TouchCollector_GestureRecognition   = 0x4,
};

enum TouchPanel_TouchInfoFlags
{
    TouchInfo_Set                    = 0x1,
    TouchInfo_LastTouchPoint         = 0x2,
    TouchInfo_SamplingDistance       = 0x4,
    TouchInfo_StylusMoveFrequency    = 0x8,
    TouchInfo_SamplingReadsToIgnore  = 0x10,
    TouchInfo_SamplingReadsPerSample = 0x20,
    TouchInfo_SamplingFilterDistance = 0x40,
};

struct TOUCH_SPI_CONFIGURATION
{
    SPI_CONFIGURATION SpiConfiguration;
    GPIO_PIN          InterruptPin; 
};

struct TOUCH_PANEL_CalibrationData
{
    INT32 Mx;
    INT32 My;

    INT32 Cx;
    INT32 Cy;

    INT32 Dx;
    INT32 Dy;
};

#define RollingAverageBufferSize 8

struct TOUCH_PANEL_SamplingSettings
{    
    INT32 ReadsToIgnore;
    INT32 ReadsPerSample;
    INT32 MaxFilterDistance; /// This is actually sqaured value of the max distance allowed between two points.
    BOOL  ActivePinStateForTouchDown;
    TOUCH_PANEL_SAMPLE_RATE SampleRate;
};

enum TouchPointLocationFlags
{
    TouchPointLocationFlags_ContactDown = 0x3FF,
    TouchPointLocationFlags_ContactUp   = 0x3FE,
};

enum TouchPointContactFlags
{
    TouchPointContactFlags_Primary = 0x80000000,
    TouchPointContactFlags_Pen     = 0x40000000,
    TouchPointContactFlags_Palm    = 0x20000000,
};

struct TouchPoint
{
    /// Location is a composite of source, x and y. bits 0-13: x, 14-27: y, 28-31: source.
    /// Source is for multi touch support; 16 sources can be reported.
    UINT32 location; 
    /// Contact is a composite of flags, width, and height. bits 0-13: width, 14-27: height, 28-31: flags
    UINT32 contact;
    INT64  time;
};

struct TOUCH_PANEL_Point
{
    UINT16 sx;
    UINT16 sy;
};

extern TOUCH_PANEL_CalibrationData g_TouchPanel_Calibration_Config;
extern TOUCH_PANEL_SamplingSettings g_TouchPanel_Sampling_Settings;

extern TouchPoint g_PAL_TouchPointBuffer[];
extern UINT32     g_PAL_TouchPointBufferSize;
extern TOUCH_PANEL_Point g_PAL_RunningAvg_Buffer[];
extern UINT32     g_PAL_RunningAvg_Buffer_Size;

#define TOUCH_PANEL_DEFAULT_TRANSIENT_BUFFER_SIZE 100
#define TOUCH_PANEL_DEFAULT_STROKE_BUFFER_SIZE 200
#define TOUCH_PANEL_MINIMUM_GESTURE_DISTANCE 10
struct TOUCH_PANEL_TouchCollectorData
{
    /// ISSUE: Use something like private alloc here?
    /// For now these are static buffers.
    TOUCH_PANEL_Point TransientBuffer[TOUCH_PANEL_DEFAULT_TRANSIENT_BUFFER_SIZE];
    TOUCH_PANEL_Point StrokeBuffer[TOUCH_PANEL_DEFAULT_STROKE_BUFFER_SIZE];
    unsigned char TouchCollectorFlags;
    INT32 TouchCollectorX1;
    INT32 TouchCollectorX2;
    INT32 TouchCollectorY1;
    INT32 TouchCollectorY2;
    Hal_Queue_UnknownSize<TOUCH_PANEL_Point> TransientPointBuffer;
    Hal_Queue_UnknownSize<TOUCH_PANEL_Point> StrokePointBuffer;
    
    TOUCH_PANEL_Point InkLastPoint;
    INT32 GestureCurrentStateX;
    INT32 GestureCurrentStateY;
    TOUCH_PANEL_Point GestureLastPoint;

    /// ISSUE: Some questions here. I need partial redraw support, and also
    /// need to lock this bitmap memory so gc won't move it.
    PAL_GFX_Bitmap* CollectorBitmap;    
    UINT32*     BitmapData;
};


/// Touch Panel Driver functions.
BOOL    HAL_TOUCH_PANEL_Enable(GPIO_INTERRUPT_SERVICE_ROUTINE touchIsrProc);
BOOL    HAL_TOUCH_PANEL_Disable();
void    HAL_TOUCH_PANEL_GetPoint( TOUCH_PANEL_SAMPLE_FLAGS* pTipState, INT32* pSource, INT32* pUnCalX, INT32* pUnCalY);
HRESULT HAL_TOUCH_PANEL_GetDeviceCaps(UINT32 iIndex, void* lpOutput);

/// Touch Panel PAL functions. 
HRESULT TOUCH_PANEL_Initialize();
HRESULT TOUCH_PANEL_Uninitialize();
HRESULT TOUCH_PANEL_GetDeviceCaps(UINT32 iIndex, void* lpOutput);
HRESULT TOUCH_PANEL_ResetCalibration();
HRESULT TOUCH_PANEL_SetCalibration( INT32 pointCount, short* sx, short* sy, short* ux, short* uy );
HRESULT TOUCH_PANEL_EnableTouchCollection(INT32 flags, INT32 x1, INT32 x2, INT32 y1, INT32 y2, PAL_GFX_Bitmap* bitmap);
HRESULT TOUCH_PANEL_GetTouchPoints(INT32* pointCount, short* sx, short* sy);
HRESULT TOUCH_PANEL_GetSetTouchInfo(UINT32 flags, INT32* param1, INT32* param2, INT32* param3);

enum GetTouchPointFlags
{
    GetTouchPointFlags_LatestPoint              = 0x0,
    GetTouchPointFlags_EarliestPoint            = 0x1,
    GetTouchPointFlags_NextPoint                = 0x2,
    GetTouchPointFlags_PreviousPoint            = 0x3,
    GetTouchPointFlags_UseTime                  = 0x10,
    GetTouchPointFlags_UseSource                = 0x20,
};

/// High 16bit of the flags may contain an index.
HRESULT TOUCH_PANEL_GetTouchPoint(UINT32* flags, UINT16* source, UINT16* x, UINT16* y, INT64* time);

//--//

#endif // _DRIVERS_TOUCHPANEL_DECL_H_

