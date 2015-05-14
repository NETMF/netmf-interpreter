////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#ifndef _DRIVERS_EVENTSINK_DECL_H_
#define _DRIVERS_EVENTSINK_DECL_H_ 1

#define EVENT_TOUCH       1
#define EVENT_GESTURE     2
#define EVENT_STORAGE     3
#define EVENT_NETWORK     4
#define EVENT_SLEEPLEVEL  5
#define EVENT_POWERLEVEL  6
#define EVENT_TIMESERVICE 7
#define EVENT_LARGEBUFFER 8
#define EVENT_GPIO        9

void PostManagedEvent(UINT8 category, UINT8 subCategory, UINT16 data1, UINT32 data2);

//--//

#define PAL_EVENT_TOUCH 0x1
#define PAL_EVENT_KEY   0x2
#define PAL_EVENT_MOUSE 0x4

typedef void (*PALEVENTLISTENER) (unsigned int e, unsigned int param);


struct PalEventListener : public HAL_DblLinkedNode<PalEventListener>
{

    PALEVENTLISTENER m_palEventListener;
    unsigned int     m_eventMask;
};


HRESULT PalEvent_Initialize();
HRESULT PalEvent_Uninitialize();
HRESULT PalEvent_Post(unsigned int e, unsigned int param);
HRESULT PalEvent_Enlist(PalEventListener* listener);


//--//

#endif // _DRIVERS_EVENTSINK_DECL_H_

