////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _TOUCH_PANEL_DRIVER_H_
#define _TOUCH_PANEL_DRIVER_H_ 1


#include <tinyhal.h>

//--//

class TouchPanel_Driver
{
public:
    static HRESULT Initialize();
    static HRESULT Uninitialize();

    static HRESULT GetDeviceCaps(unsigned int iIndex, void* lpOutput);
    static HRESULT ResetCalibration();
    static HRESULT SetCalibration(int pointCount, short* sx, short* sy, short* ux, short* uy);
    static HRESULT SetNativeBufferSize(int transientBufferSize, int strokeBufferSize);
    static HRESULT GetTouchPoints(int* pointCount, short* sx, short* sy);
    static HRESULT GetSetTouchInfo(UINT32 flags, INT32* param1, INT32* param2, INT32* param3);

    static HRESULT GetTouchPoint(UINT32* flags, UINT16* source, UINT16* x, UINT16* y, INT64* time);
    static HRESULT GetTouchPoint(UINT32* flags, UINT32* location, INT64* time);

private:

    static HRESULT GetTouchPoint(UINT32* flags, TouchPoint **point);

    static void    TouchIsrProc   ( GPIO_PIN pin, BOOL pinState, void* context );
    static void    TouchCompletion( void* arg );

    //--//

    void           TouchPanelCalibratePoint( int UncalX, int UncalY,int *pCalX, int   *pCalY );
    UINT16         GetTouchStylusFlags     ( unsigned int sampleFlags );
    void           SetDriverDefaultCalibrationData();
    
    void           PollTouchPoint( void* arg );
    TouchPoint*    AddTouchPoint(UINT16 source, UINT16 x, UINT16 y, INT64 time, BOOL fIgnoreDuplicate = FALSE);

    //

    int                             m_touchMoveIndex;
    TouchPoint*                     m_startMovePtr;
    TouchPoint                      m_tmpUpTouch;
    
    HAL_COMPLETION                  m_touchCompletion;
    TOUCH_PANEL_CalibrationData     m_calibrationData;
    INT32                           m_samplingTimespan;   
    INT32                           m_InternalFlags;
    INT32                           m_readCount;
    INT32                           m_runavgTotalX;
    INT32                           m_runavgTotalY;
    INT32                           m_runavgCount;
    INT32                           m_runavgIndex;

    INT32                           m_head;
    INT32                           m_tail;


    enum InternalFlags
    {
        Contact_Down                = 0x1,
        Contact_WasDown             = 0x2,
    };
};

#endif


