////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

#ifndef _TOUCH_PANEL_DRIVER_H_
#include "touchpanel_driver.h"
#endif

//--//

#define abs(a) (((a) < 0) ? -(a) : (a))
#define sign(a) (((a) < 0) ? -1 : 1)

TOUCH_PANEL_CalibrationData g_TouchPanel_DefaultCalibration_Config = {1, 1, 0, 0, 1, 1};
TouchPanel_Driver g_TouchPanel_Driver;

/// Divide a by b, and return rounded integer.
INT32 RoundDiv(INT32 a, INT32 b)
{
    if (b == 0)
        return 0xFFFF;

    INT32 d = a / b;
    INT32 r = abs(a) % abs(b);

    if ((r * 2) > abs(a))
    {
        d = (abs(d) + 1) * sign(b) * sign(a);
    }

    return d;
}

HRESULT TouchPanel_Driver::Initialize()
{     
    g_TouchPanel_Driver.m_samplingTimespan = ONE_MHZ / g_TouchPanel_Sampling_Settings.SampleRate.CurrentSampleRateSetting;

    g_TouchPanel_Driver.m_calibrationData = g_TouchPanel_Calibration_Config;        
    g_TouchPanel_Driver.m_InternalFlags = 0;

    g_TouchPanel_Driver.m_head = 0;
    g_TouchPanel_Driver.m_tail = 0;

    g_TouchPanel_Driver.m_runavgTotalX = 0;
    g_TouchPanel_Driver.m_runavgTotalY = 0;
    g_TouchPanel_Driver.m_runavgCount = 0;
    g_TouchPanel_Driver.m_runavgIndex = 0;

    g_TouchPanel_Driver.m_startMovePtr = NULL;

    g_TouchPanel_Driver.m_touchMoveIndex = 0;

    /// Following four should be done at HAL_Initialize(), currently an issue blocking the move.
    PalEvent_Initialize();
    Gesture_Initialize();
    Ink_Initialize();

    /// Enable the touch hardware.
    if (!HAL_TOUCH_PANEL_Enable(TouchIsrProc))
    {
        return CLR_E_FAIL;
    }

    g_TouchPanel_Driver.m_touchCompletion.InitializeForISR(TouchPanel_Driver::TouchCompletion, NULL);      

    /// At this point we should be ready to recieve touch inputs.    

    return S_OK;
}

HRESULT TouchPanel_Driver::Uninitialize()
{
    if(g_TouchPanel_Driver.m_touchCompletion.IsLinked())
    {
        g_TouchPanel_Driver.m_touchCompletion.Abort();
    }

    HAL_TOUCH_PANEL_Disable();    

    return S_OK;
}
    
/// Calibrate an uncalibrated point.
void TouchPanel_Driver::TouchPanelCalibratePoint(INT32 UncalX, INT32 UncalY, INT32 *pCalX, INT32 *pCalY)
{
    /// Doing simple linear calibration for now.
    /// ASSUMPTION: uncalibrated x, y touch co-ordinates are independent. In reality this is not correct.
    /// We should consider doing 2D calibration that takes dependeng_Cy issue into account.
    
    *pCalX = RoundDiv((g_TouchPanel_Driver.m_calibrationData.Mx * UncalX + g_TouchPanel_Driver.m_calibrationData.Cx), g_TouchPanel_Driver.m_calibrationData.Dx);
    *pCalY = RoundDiv((g_TouchPanel_Driver.m_calibrationData.My * UncalY + g_TouchPanel_Driver.m_calibrationData.Cy), g_TouchPanel_Driver.m_calibrationData.Dy);

    /// FUTURE - 07/10/2008-munirula- Negative co-ords are meaningful or meaningless. I am leaning
    /// towards meaningless for now, but this needs further thought which I agree.
    if (*pCalX < 0)
        *pCalX = 0;

    if (*pCalY < 0)
        *pCalY = 0;
}

HRESULT TouchPanel_Driver::GetDeviceCaps(UINT32 iIndex, void* lpOutput)
{
    return HAL_TOUCH_PANEL_GetDeviceCaps(iIndex, lpOutput);
}

HRESULT TouchPanel_Driver::ResetCalibration()
{
    g_TouchPanel_Driver.m_calibrationData = g_TouchPanel_DefaultCalibration_Config;

    return S_OK;
}

HRESULT TouchPanel_Driver::SetCalibration(INT32 pointCount, short* sx, short* sy, short* ux, short* uy)
{
    if (pointCount != 5)
        return CLR_E_FAIL;

    /// Calculate simple 1D calibration parameters.
    g_TouchPanel_Driver.m_calibrationData.Mx = sx[2] - sx[3];
    g_TouchPanel_Driver.m_calibrationData.Cx = ux[2]*sx[3] - ux[3]*sx[2];
    g_TouchPanel_Driver.m_calibrationData.Dx = ux[2] - ux[3];

    g_TouchPanel_Driver.m_calibrationData.My = sy[1] - sy[2];
    g_TouchPanel_Driver.m_calibrationData.Cy = uy[1]*sy[2] - uy[2]*sy[1];
    g_TouchPanel_Driver.m_calibrationData.Dy = uy[1] - uy[2];

    return S_OK;
}

//--//

/// Completion routine, called in every 10ms or so, when we are actively sampling
/// stylus.
void TouchPanel_Driver::TouchCompletion( void* arg )
{
    g_TouchPanel_Driver.PollTouchPoint(arg);
}

void TouchPanel_Driver::PollTouchPoint( void* arg )
{
    TOUCH_PANEL_SAMPLE_FLAGS flags;
    INT32 x = 0;
    INT32 y = 0;
    INT32 ux = 0; /// Uncalibrated x.
    INT32 uy = 0; /// Uncalibrated y.
    INT32 source = 0;
    bool fProcessUp = false;

    INT64 time = ::Time_GetMachineTime();


    GLOBAL_LOCK(irq);

    /// Get the point information from driver.
    HAL_TOUCH_PANEL_GetPoint(&flags, &source, &ux, &uy);  

    if ((flags & TouchSampleValidFlag) == TouchSampleValidFlag)
    {        
        /// Calibrate a point;
        /// Driver should be doing all filter/averaging of the points.
        TouchPanelCalibratePoint(ux, uy, &x, &y);   
        
        TouchPoint* point = NULL;
                
        if ((m_InternalFlags & Contact_Down) && (!(m_InternalFlags & Contact_WasDown)))
        {
            AddTouchPoint(0, TouchPointLocationFlags_ContactDown, TouchPointLocationFlags_ContactDown, time);            
            /// Stylus down.
            PalEvent_Post(PAL_EVENT_TOUCH, TouchPanelStylusDown);

            if(NULL != (point = AddTouchPoint(0, x, y, time)))
            {
                PostManagedEvent(EVENT_TOUCH, TouchPanelStylusDown, 1, (UINT32)point);
            }

            m_InternalFlags |= Contact_WasDown;
        }
        else if ((!(m_InternalFlags & Contact_Down)) && (m_InternalFlags & Contact_WasDown))
        {
            fProcessUp = true;

            point = AddTouchPoint(0, x, y, time);
        }    
        else if ((m_InternalFlags & Contact_Down) && (m_InternalFlags & Contact_WasDown))
        {
            /// Stylus Move.
            if(NULL != (point = AddTouchPoint(0, x, y, time, TRUE)))
            {
                if(m_startMovePtr == NULL)
                {
                    m_startMovePtr = point;
                    m_touchMoveIndex = 1;
                }
                else
                {
                    if(((UINT32)m_startMovePtr > (UINT32)point) ||
                        (time - m_startMovePtr->time) > (g_TouchPanel_Sampling_Settings.SampleRate.MaxTimeForMoveEvent_ticks))
                    {
                        PostManagedEvent(EVENT_TOUCH, TouchPanelStylusMove, m_touchMoveIndex, (UINT32)m_startMovePtr);

                        m_startMovePtr = NULL;
                        m_touchMoveIndex = 0;
                    }
                    else if(m_startMovePtr != point)
                    {
                        m_touchMoveIndex++;
                    }
                }
            }
            // we will get here if the stylus is in a TouchDown state but hasn't moved, so if we don't have a current move ptr, then
            // set the start move ptr to the latest point
            else if(m_startMovePtr == NULL)
            {
                UINT32 touchflags = GetTouchPointFlags_LatestPoint | GetTouchPointFlags_UseTime | GetTouchPointFlags_UseSource;
                
                if(SUCCEEDED(GetTouchPoint(&touchflags, &point)))
                {
                    m_startMovePtr = point;
                    m_touchMoveIndex = 1;
                }
            }
            // We should always send a move event if we cross the max time boundary for a move event, even if it is just one point
            else if((time - m_startMovePtr->time) > (g_TouchPanel_Sampling_Settings.SampleRate.MaxTimeForMoveEvent_ticks))
            {
                PostManagedEvent(EVENT_TOUCH, TouchPanelStylusMove, m_touchMoveIndex, (UINT32)m_startMovePtr);
                      
                if( m_touchMoveIndex > 1 )
                {
                    m_startMovePtr = &m_startMovePtr[m_touchMoveIndex-1];
                    m_touchMoveIndex = 1;
                }

                m_startMovePtr->time = time;      
            }
        }
    }

    if ((!(m_InternalFlags & Contact_Down)) && (m_InternalFlags & Contact_WasDown))
    {
        fProcessUp = true;
    }

    if(fProcessUp)
    {
        UINT32 touchflags = GetTouchPointFlags_LatestPoint | GetTouchPointFlags_UseTime | GetTouchPointFlags_UseSource;
        
        TouchPoint* point = NULL;

        /// Only send a move event if we have substantial data (more than two items) otherwise, the
        /// TouchUp event should suffice
        if(m_touchMoveIndex > 2 && m_startMovePtr != NULL)
        {
            PostManagedEvent(EVENT_TOUCH, TouchPanelStylusMove, m_touchMoveIndex, (UINT32)m_startMovePtr);
        }

        m_startMovePtr = NULL;
        m_touchMoveIndex = 0;

        if(FAILED(GetTouchPoint(&touchflags, &point)))
        {
            point = NULL;
        }
        
        /// Now add special contactup delimeter.
        AddTouchPoint(0, TouchPointLocationFlags_ContactUp, TouchPointLocationFlags_ContactUp, time);
        
        /// Stylus up.
        PalEvent_Post(PAL_EVENT_TOUCH, TouchPanelStylusUp);
        
        //Make a copy of the point for the Microsoft.SPOT.Touch handler.
        if(point == NULL)
        {
            m_tmpUpTouch.contact = 0;
            m_tmpUpTouch.time = time;
            m_tmpUpTouch.location = 0;

            point = &m_tmpUpTouch;
        }
    
        PostManagedEvent(EVENT_TOUCH, TouchPanelStylusUp, 1, (UINT32)point); 
        
        m_InternalFlags &= ~Contact_WasDown;
    }
    
    /// Schedule or unschedule completion.
    if (!g_TouchPanel_Driver.m_touchCompletion.IsLinked())
    {
        if (m_InternalFlags & Contact_Down)
        {
            g_TouchPanel_Driver.m_touchCompletion.EnqueueDelta(g_TouchPanel_Driver.m_samplingTimespan);
        }
    }   
    else
    {
        if (!(m_InternalFlags & Contact_Down))
        {
            g_TouchPanel_Driver.m_touchCompletion.Abort();
        }
    }
}

TouchPoint* TouchPanel_Driver::AddTouchPoint(UINT16 source, UINT16 x, UINT16 y, INT64 time, BOOL fIgnoreDuplicate)
{
    static UINT16 lastAddedX = 0xFFFF;
    static UINT16 lastAddedY = 0xFFFF;

    GLOBAL_LOCK(irq);

    if ((x == TouchPointLocationFlags_ContactUp) || (x == TouchPointLocationFlags_ContactDown))
    {
        /// Reset the points.
        lastAddedX = 0xFFFF;
        lastAddedY = 0xFFFF;

        m_runavgIndex = 0;
        m_runavgCount = 0;
        m_runavgTotalX = 0;
        m_runavgTotalY = 0;
    }
    else
    {
        if (lastAddedX != 0xFFFF)
        {
            /// dist2 is distance squared.
            INT32 dx = abs(x - lastAddedX);
            INT32 dy = abs(y - lastAddedY);
            INT32 dist2 = dx * dx + dy * dy;

            /// Ignore this point if it is too far from last point.
            if (dist2 > g_TouchPanel_Sampling_Settings.MaxFilterDistance) return NULL; 
        }

        if(g_PAL_RunningAvg_Buffer_Size > 1)
        {
            if (m_runavgIndex >= (INT32)g_PAL_RunningAvg_Buffer_Size)
                m_runavgIndex = 0;

            if (m_runavgCount >= (INT32)g_PAL_RunningAvg_Buffer_Size)
            {
                m_runavgTotalX -= g_PAL_RunningAvg_Buffer[m_runavgIndex].sx;
                m_runavgTotalY -= g_PAL_RunningAvg_Buffer[m_runavgIndex].sy;            
            }
            else
            {
                m_runavgCount++;
            }

            m_runavgTotalX += x;
            m_runavgTotalY += y;

            g_PAL_RunningAvg_Buffer[m_runavgIndex].sx = x;
            g_PAL_RunningAvg_Buffer[m_runavgIndex].sy = y;

            m_runavgIndex++;

            x = m_runavgTotalX / m_runavgCount;
            y = m_runavgTotalY / m_runavgCount;
        }

        ///
        /// This is mainly intended for TouchMove events.  We don't want to add duplicate points
        /// if the touch down is not moving
        ///
        if(fIgnoreDuplicate)
        {
            UINT32 dx = abs(x - lastAddedX);
            UINT32 dy = abs(y - lastAddedY);
            
            if(dx <= 2 && dy <= 2) return NULL;
        }

        lastAddedX = x;
        lastAddedY = y;        
    }
    
    UINT32 location = (x & 0x3FFF) | ((y & 0x3FFF) << 14) | (source << 28);
    UINT32 contact  = TouchPointContactFlags_Primary | TouchPointContactFlags_Pen;
    TouchPoint& point = g_PAL_TouchPointBuffer[m_tail];
    point.time = time;
    point.location = location;
    point.contact = contact;

    if(g_PAL_TouchPointBufferSize > 1)
    {
        m_tail ++;

        if(m_tail >= (INT32)g_PAL_TouchPointBufferSize) m_tail = 0;

        if (m_tail == m_head)
        {
            m_head++;

            if(m_head >= (INT32)g_PAL_TouchPointBufferSize) m_head = 0;
        }
    }

    return &point;
}

void TouchPanel_Driver::TouchIsrProc( GPIO_PIN pin, BOOL pinState, void* context )
{
    if(pinState == g_TouchPanel_Sampling_Settings.ActivePinStateForTouchDown)
    {
        g_TouchPanel_Driver.m_InternalFlags |= Contact_Down; /// Toggle contact flag.
        g_TouchPanel_Driver.m_readCount = 0;
    }
    else
    {
        g_TouchPanel_Driver.m_InternalFlags &= ~Contact_Down; /// Toggle contact flag.        
    }

    if(g_TouchPanel_Driver.m_touchCompletion.IsLinked())
    {
        g_TouchPanel_Driver.m_touchCompletion.Abort();
    }
    g_TouchPanel_Driver.m_touchCompletion.EnqueueDelta(0);
}

HRESULT TouchPanel_Driver::GetTouchPoints(int* pointCount, short* sx, short* sy)
{
    GLOBAL_LOCK(irq);

    
    return S_OK;
}

HRESULT TouchPanel_Driver::GetSetTouchInfo(UINT32 flags, INT32* param1, INT32* param2, INT32* param3)
{
    if (flags & TouchInfo_Set) /// SET.
    {        
        if (flags & TouchInfo_SamplingDistance)
        {
            INT32 samplesPerSecond = ONE_MHZ / *param1;
            
            /// Minimum of 500us otherwise the system will be overrun
            if (samplesPerSecond >= g_TouchPanel_Sampling_Settings.SampleRate.SamplesPerSecondLow && 
                samplesPerSecond <= g_TouchPanel_Sampling_Settings.SampleRate.SamplesPerSecondHigh)
            {
                g_TouchPanel_Driver.m_samplingTimespan = *param1;

                g_TouchPanel_Sampling_Settings.SampleRate.CurrentSampleRateSetting = samplesPerSecond;
            }
            else
            {
                return CLR_E_OUT_OF_RANGE;
            }
        }
        else if (flags & TouchInfo_StylusMoveFrequency)
        {
            INT32 ms_per_touchmove_event = *param1; // *param1 is in milliseconds
            INT32 min_ms_per_touchsample = 1000 / g_TouchPanel_Sampling_Settings.SampleRate.SamplesPerSecondLow;
            INT32 ticks;

            // zero value indicates turning move notifications based on time off
            if(ms_per_touchmove_event == 0)         ticks = 0x7FFFFFFF; 
            // min_ms_per_touchsample is the sample frequency for the touch screen driver
            // Touch Move events are queued up and sent at the given time frequency (StylusMoveFrequency)
            // We should not set the move frequency to be less than the sample frequency, otherwise there
            // would be no data available occassionally.
            else
            {
              ticks = TOUCH_PANEL_SAMPLE_MS_TO_TICKS(ms_per_touchmove_event);
              if(ticks < min_ms_per_touchsample) return CLR_E_OUT_OF_RANGE;
            }

            g_TouchPanel_Sampling_Settings.SampleRate.MaxTimeForMoveEvent_ticks = ticks;
        }
        else if (flags & TouchInfo_SamplingReadsToIgnore)
        {
            g_TouchPanel_Sampling_Settings.ReadsToIgnore = *param1;
        }
        else if (flags & TouchInfo_SamplingReadsPerSample)
        {
            g_TouchPanel_Sampling_Settings.ReadsPerSample = *param1;
        }
        else if (flags & TouchInfo_SamplingFilterDistance)
        {
            g_TouchPanel_Sampling_Settings.MaxFilterDistance = *param1;
        }
        else
        {
            return CLR_E_INVALID_PARAMETER;
        }
    }
    else /// GET
    {
        *param1 = 0;
        *param2 = 0;
        *param3 = 0;

        if (flags & TouchInfo_LastTouchPoint)
        {   
            UINT16 x = 0;
            UINT16 y = 0;
            UINT32 flags = GetTouchPointFlags_LatestPoint | GetTouchPointFlags_UseTime | GetTouchPointFlags_UseSource;
            UINT16 source = 0;
            INT64 time = 0;
            GetTouchPoint(&flags, &source, &x, &y, &time);
            *param1 = x;
            *param2 = y;         
        }
        else if (flags & TouchInfo_SamplingDistance)
        {
            *param1 = g_TouchPanel_Driver.m_samplingTimespan;
        }
        else if (flags & TouchInfo_StylusMoveFrequency)
        {
            int ticks = g_TouchPanel_Sampling_Settings.SampleRate.MaxTimeForMoveEvent_ticks;

            if(ticks == 0x7FFFFFFF) *param1 = 0;
            else                    *param1 = ticks / TIME_CONVERSION__TO_MILLISECONDS;
        }
        else if (flags & TouchInfo_SamplingReadsToIgnore)
        {
            *param1 = g_TouchPanel_Sampling_Settings.ReadsToIgnore;
        }
        else if (flags & TouchInfo_SamplingReadsPerSample)
        {
            *param1 = g_TouchPanel_Sampling_Settings.ReadsPerSample;
        }
        else if (flags & TouchInfo_SamplingFilterDistance)
        {
            *param1 = g_TouchPanel_Sampling_Settings.MaxFilterDistance;
        }
        else
        {
            return CLR_E_INVALID_PARAMETER;
        }
    }

    return S_OK;
}

HRESULT TouchPanel_Driver::GetTouchPoint(UINT32* flags, TouchPoint **point)
{
    UINT8 searchFlag        = *flags & 0xF;
    UINT8 conditionalFlag   = *flags & 0xF0;
    UINT32 index = 0;

    GLOBAL_LOCK(irq);

    if ((g_TouchPanel_Driver.m_head == g_TouchPanel_Driver.m_tail) && g_PAL_TouchPointBufferSize > 1)
        return CLR_E_FAIL;

    if (searchFlag == GetTouchPointFlags_LatestPoint)
    {
        index = g_TouchPanel_Driver.m_tail > 0 ? (g_TouchPanel_Driver.m_tail - 1) : (g_PAL_TouchPointBufferSize - 1);        
    }
    else if (searchFlag == GetTouchPointFlags_EarliestPoint)
    {
        index = g_TouchPanel_Driver.m_head;
    }
    else if (searchFlag == GetTouchPointFlags_NextPoint)
    {
        if (conditionalFlag & GetTouchPointFlags_UseTime)
        {
            index = (*flags >> 16);
            index = (index + 1) % g_PAL_TouchPointBufferSize;
            if ((index == g_TouchPanel_Driver.m_tail) && g_PAL_TouchPointBufferSize > 1)
                return CLR_E_FAIL;
        }
        else return CLR_E_NOT_SUPPORTED;
    }

    *point = &g_PAL_TouchPointBuffer[index];

    *flags &= 0xFFFF; /// Clear high 16 bit.
    *flags |= (index << 16);

    return S_OK;
}

HRESULT TouchPanel_Driver::GetTouchPoint(UINT32* flags, UINT32* location, INT64* time)
{
    TouchPoint* point;

    HRESULT hr = GetTouchPoint(flags, &point);
    if (hr != S_OK)
    {
        return hr;
    }

    *location = point->location;
    *time = point->time;

    return S_OK;
}

HRESULT TouchPanel_Driver::GetTouchPoint(UINT32* flags, UINT16* source, UINT16* x, UINT16* y, INT64* time)
{
    UINT32 location = 0;
    HRESULT hr = GetTouchPoint(flags, &location, time);
    if (hr != S_OK)
    {
        return hr;
    }

    *source = (location >> 28);
    *x = location & 0x3FFF;
    *y = (location >> 14) & 0x3FFF;    

    return S_OK;
}

