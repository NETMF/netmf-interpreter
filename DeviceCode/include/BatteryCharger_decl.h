////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _DRIVERS_BATTERY_CHARGER_DECL_H_
#define _DRIVERS_BATTERY_CHARGER_H_ 1

//--//

#define CHARGER_STATUS_ON_AC_POWER      0x00000001
#define CHARGER_STATUS_CHARGING         0x00000002
#define CHARGER_STATUS_FAULT            0x00000004
#define CHARGER_STATUS_OVER_TEMP        0x00000008
#define CHARGER_STATUS_CHARGE_COMPLETE  0x00000010
#define CHARGER_STATUS_DISABLED         0x00000020

#define CHARGER_SHUTDOWN_USB            0x00000002
#define CHARGER_SHUTDOWN_ADC            0x00000004

//--//

BOOL Charger_Initialize    (                          );
BOOL Charger_Uninitialize  (                          );
BOOL Charger_Status        ( UINT32& Status           );
BOOL Charger_Shutdown      ( UINT32 FlagMask          );
BOOL Charger_Restart       ( UINT32 FlagMask          );
void Charger_SetTemperature( INT16 DegreesCelcius_x10 );

//--//

#endif // _DRIVERS_BATTERY_CHARGER_DECL_H_

