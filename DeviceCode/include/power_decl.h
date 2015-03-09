////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _DRIVERS_POWER_DECL_H_
#define _DRIVERS_POWER_DECL_H_ 1

//--//

//
// !!! KEEP IN SYNC WITH Microsoft.SPOT.Hardware.SleepLevel !!!
//
enum SLEEP_LEVEL
{
    SLEEP_LEVEL__AWAKE         = 0x00,
    SLEEP_LEVEL__SELECTIVE_OFF = 0x10,
    SLEEP_LEVEL__SLEEP         = 0x20,
    SLEEP_LEVEL__DEEP_SLEEP    = 0x30,
    SLEEP_LEVEL__OFF           = 0x40,
};

//
// !!! KEEP IN SYNC WITH Microsoft.SPOT.Hardware.SleepEventType !!!
//
enum SLEEP_LEVEL_CATEGORY 
{
    SLEEP_LEVEL_CATEGORY__INVALID         = 0x00,
    SLEEP_LEVEL_CATEGORY__CHANGEREQUESTED = 0x01,
    SLEEP_LEVEL_CATEGORY__WAKEUP          = 0x02,
};

//
// !!! KEEP IN SYNC WITH Microsoft.SPOT.Hardware.PowerLevel !!!
//
enum POWER_LEVEL
{
    POWER_LEVEL__HIGH_POWER = 0x10,
    POWER_LEVEL__MID_POWER  = 0x20,
    POWER_LEVEL__LOW_POWER  = 0x30,    
};

//
// !!! KEEP IN SYNC WITH Microsoft.SPOT.Hardware.PowerEventType !!!
//
enum POWER_LEVEL_CATEGORY
{
    POWER_LEVEL_CATEGORY__INVALID     = 0x00,
    POWER_LEVEL_CATEGORY__PRENOTIFY   = 0x01,
    POWER_LEVEL_CATEGORY__POSTNOTIFY  = 0x02,    
};

    
//--//
BOOL CPU_Initialize      ();
void CPU_Reset           ();
void CPU_Sleep           (SLEEP_LEVEL level, UINT64 wakeEvents);
void CPU_ChangePowerLevel(POWER_LEVEL level);
void CPU_Halt            ();
BOOL CPU_IsSoftRebootSupported();

//--//

#endif // _DRIVERS_POWER_DECL_H_

