////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

void Listener(unsigned int e, unsigned int param);

struct GestureDriver
{
    static const int c_IgnoreCount = 2;
private:
    static BOOL      s_initialized;

    
    PalEventListener m_gestureListener;
    HAL_COMPLETION   m_gestureCompletion;
    UINT32           m_index;
    UINT32           m_currentState;
    UINT16           m_lastx;
    UINT16           m_lasty;
    UINT16           m_startx;
    UINT16           m_starty;

    UINT32           m_stateIgnoreIndex;
    UINT32           m_stateIgnoreHead;
    UINT32           m_stateIgnoreTail;
    UINT32           m_stateIgnoreBuffer[c_IgnoreCount];


public:
    static HRESULT Initialize();
    static HRESULT Uninitialize();
    static BOOL ProcessPoint(UINT32 flags, UINT16 source, UINT16 x, UINT16 y, INT64 time);

    static void ResetRecognition();
    static void EventListener(unsigned int e, unsigned int param);
    static void GestureContinuationRoutine(void *arg);

};

extern GestureDriver g_GestureDriver;

