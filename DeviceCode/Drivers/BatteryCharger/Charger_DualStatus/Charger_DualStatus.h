////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////#include <tinyhal.h>
#include <tinyhal.h>
#ifndef _DRIVERS_BATTERYCHARGER_CHARGER_DUALSTATUS_H_
#define _DRIVERS_BATTERYCHARGER_CHARGER_DUALSTATUS_H_ 1

//--//


struct Charger_DualStatus
{
    volatile UINT32    m_disableFlags;

    volatile BOOL      m_ACPowerConnected_RAW;
    volatile BOOL      m_ACPowerConnected_DEBOUNCED;

    volatile BOOL      m_charging;
    volatile BOOL      m_fault;
    volatile BOOL      m_overTemperature;
    volatile BOOL      m_chargeComplete;

    volatile BOOL      m_ACPR_PinState;
    volatile BOOL      m_STAT1_PinState;
    volatile BOOL      m_STAT2_PinState;

    HAL_STATE_DEBOUNCE m_Charger_Debounce;
    HAL_STATE_DEBOUNCE m_ACPower_Debounce;

    //--//

    static BOOL Initialize();

    static BOOL Uninitialize();

    static BOOL Status( UINT32& Status );

    static BOOL Shutdown( UINT32 FlagMask );
    static BOOL Restart ( UINT32 FlagMask );

    static void SetTemperature( INT16 DegreesCelcius_x10 );

    //--//

    static void UpdateState();
    static void ControlPins();

    static void Decode_Status_Pins ( void* StateVal                           );
    static void Shutdown_Pin       ( BOOL Active                              );
    static void Status_Pins        ( BOOL Active                              );
    static void Status_ISR         ( GPIO_PIN Pin, BOOL PinState, void* Param );
    static void ACPower_ISR        ( GPIO_PIN Pin, BOOL PinState, void* Param );
    static void ACPower_StateChange( void* ACPowerState                       );
};

extern Charger_DualStatus g_Charger_DualStatus;

//--//

struct DECODED_STATUS
{
    BOOL m_charging;
    BOOL m_fault;
    BOOL m_chargeComplete;
};

struct CHARGER_DUALSTATUS_CONFIG
{
    HAL_DRIVER_CONFIG_HEADER Header;

    //--//

    GPIO_FLAG      Shutdown_GPIO_PIN;
    GPIO_FLAG      FastCharge_GPIO_PIN;
    GPIO_FLAG      ACPower_GPIO_PIN;
    GPIO_RESISTOR  ACPower_Resistor;
    GPIO_FLAG      Stat1_GPIO_PIN;
    GPIO_FLAG      Stat2_GPIO_PIN;
    GPIO_RESISTOR  Stat1_Resistor;
    GPIO_RESISTOR  Stat2_Resistor;
    INT16          Over_Temperature_Shutdown;
    INT16          Under_Temperature_Shutdown;
    BOOL           Shutdown_ActiveStateDriven;
    BOOL           Shutdown_InactiveStateDriven;
    GPIO_RESISTOR  Shutdown_ActiveUndriven_Resistor;
    GPIO_RESISTOR  Shutdown_InactiveUndriven_Resistor;
    DECODED_STATUS StatusDecodes[4];
    UINT32         ACPower_On_Debounce_uSec;
    UINT32         Status_Change_Debounce_uSec;
    UINT8          Shutdown_LowPowerState;

    //--//

    static LPCSTR GetDriverName() { return "CHARGER_DUALSTATUS"; }
};

extern CHARGER_DUALSTATUS_CONFIG g_DualStatus_Config;

#endif  // _DRIVERS_BATTERYCHARGER_CHARGER_DUALSTATUS_H_

