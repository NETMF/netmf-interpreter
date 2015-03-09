////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include "Charger_DualStatus.h"

//--//

#undef  TRACE_ALWAYS
#undef  TRACE_STAT_CHANGES
#undef  TRACE_GPIO_CHANGES

#define TRACE_ALWAYS            0x00000001
#define TRACE_STAT_CHANGES      0x00000002
#define TRACE_GPIO_CHANGES      0x00000004

#undef  DEBUG_TRACE
#define DEBUG_TRACE (TRACE_ALWAYS)
//#define DEBUG_TRACE (TRACE_ALWAYS | TRACE_GPIO_CHANGES)
//#define DEBUG_TRACE (TRACE_ALWAYS | TRACE_STAT_CHANGES)
//#define DEBUG_TRACE (TRACE_ALWAYS | TRACE_GPIO_CHANGES | TRACE_STAT_CHANGES)

//--//

// assume we are connected to AC power, until we know otherwise

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata = "g_Charger_DualStatus"
#endif

Charger_DualStatus g_Charger_DualStatus;

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata
#endif

//--//



BOOL Charger_DualStatus::Initialize()
{
    ASSERT_IRQ_MUST_BE_OFF();

    CHARGER_DUALSTATUS_CONFIG* Config = &g_DualStatus_Config;

    HAL_CONFIG_BLOCK::ApplyConfig( Config->GetDriverName(), Config, sizeof(*Config) );

    memset( &g_Charger_DualStatus, 0, sizeof(g_Charger_DualStatus) );

    g_Charger_DualStatus.m_ACPowerConnected_RAW       = TRUE;
    g_Charger_DualStatus.m_ACPowerConnected_DEBOUNCED = TRUE;
    g_Charger_DualStatus.m_STAT1_PinState             = TRUE;
    g_Charger_DualStatus.m_STAT2_PinState             = TRUE;

    //--//

    g_Charger_DualStatus.m_Charger_Debounce.Initialize( Config->Status_Change_Debounce_uSec, Decode_Status_Pins  );
    g_Charger_DualStatus.m_ACPower_Debounce.Initialize( Config->ACPower_On_Debounce_uSec   , ACPower_StateChange );

    // disable the charger control line, disable Stat1, Stat2 interrupts
    // before testing AC power pin (avoid under supply condition)
    Status_Pins ( FALSE );
    Shutdown_Pin( TRUE  );

    if(GPIO_PIN_NONE != Config->FastCharge_GPIO_PIN.Pin)
    {
        CPU_GPIO_EnableOutputPin( Config->FastCharge_GPIO_PIN.Pin, !Config->FastCharge_GPIO_PIN.ActiveState );
    }

    // enable inputs on charger GPIOs, with interrupts on any edge

    if(GPIO_PIN_NONE != Config->ACPower_GPIO_PIN.Pin)
    {
        // enable, read current state, opposite state of next interrupt
        CPU_GPIO_EnableInputPin( Config->ACPower_GPIO_PIN.Pin, FALSE, ACPower_ISR, GPIO_INT_EDGE_BOTH, Config->ACPower_Resistor );
        g_Charger_DualStatus.m_ACPR_PinState        = CPU_GPIO_GetPinState(Config->ACPower_GPIO_PIN.Pin);
        g_Charger_DualStatus.m_ACPowerConnected_RAW = (g_Charger_DualStatus.m_ACPR_PinState == Config->ACPower_GPIO_PIN.ActiveState);
    }
    else
    {
        g_Charger_DualStatus.m_ACPowerConnected_RAW = TRUE;  // assume always AC power if no pin assigned
    }

    g_Charger_DualStatus.m_ACPowerConnected_DEBOUNCED = !g_Charger_DualStatus.m_ACPowerConnected_RAW;     // force a state change, regardless of what it is

    // we debounce only the OFF->ON transition
    if(g_Charger_DualStatus.m_ACPowerConnected_RAW)
    {
        g_Charger_DualStatus.m_ACPower_Debounce.Change( g_Charger_DualStatus.m_ACPowerConnected_RAW );
    }
    else
    {
        g_Charger_DualStatus.m_ACPower_Debounce.Abort();

        ACPower_StateChange( (void*)(size_t)g_Charger_DualStatus.m_ACPowerConnected_RAW );
    }

    return TRUE;
}

BOOL Charger_DualStatus::Uninitialize()
{
    CHARGER_DUALSTATUS_CONFIG* Config = &g_DualStatus_Config;

    g_Charger_DualStatus.m_Charger_Debounce.Abort();
    g_Charger_DualStatus.m_ACPower_Debounce.Abort();
        
    // disable everything we enabled in Initialize
    if(GPIO_PIN_NONE != Config->Shutdown_GPIO_PIN.Pin)
    {
        if(Config->Shutdown_InactiveStateDriven)
        {
            CPU_GPIO_EnableInputPin( Config->Shutdown_GPIO_PIN.Pin, FALSE, NULL, GPIO_INT_NONE, (GPIO_RESISTOR) (Config->Shutdown_GPIO_PIN.ActiveState ? RESISTOR_PULLDOWN : RESISTOR_PULLUP) );
        }
        else
        {
            CPU_GPIO_EnableInputPin( Config->Shutdown_GPIO_PIN.Pin, FALSE, NULL, GPIO_INT_NONE, Config->Shutdown_InactiveUndriven_Resistor );
        }
    }

    if(GPIO_PIN_NONE != Config->FastCharge_GPIO_PIN.Pin)
    {
        CPU_GPIO_EnableInputPin( Config->FastCharge_GPIO_PIN.Pin, FALSE, NULL, GPIO_INT_NONE, RESISTOR_PULLUP );
    }

    if(GPIO_PIN_NONE != Config->Stat2_GPIO_PIN.Pin)
    {
        CPU_GPIO_EnableInputPin( Config->Stat2_GPIO_PIN.Pin, FALSE, NULL, GPIO_INT_NONE, RESISTOR_PULLUP );
    }

    if(GPIO_PIN_NONE != Config->Stat1_GPIO_PIN.Pin)
    {
        CPU_GPIO_EnableInputPin( Config->Stat1_GPIO_PIN.Pin, FALSE, NULL, GPIO_INT_NONE, RESISTOR_PULLUP );
    }

    if(GPIO_PIN_NONE != Config->ACPower_GPIO_PIN.Pin)
    {
        CPU_GPIO_EnableInputPin( Config->ACPower_GPIO_PIN.Pin, FALSE, NULL, GPIO_INT_NONE, RESISTOR_PULLUP );
    }

    return TRUE;
}


BOOL Charger_DualStatus::Status( UINT32& Status )
{
    GLOBAL_LOCK(irq);

    // get value of global set via interrupt
    UINT32 val = 0;

    if(g_Charger_DualStatus.m_ACPowerConnected_DEBOUNCED) val |= CHARGER_STATUS_ON_AC_POWER;
    if(g_Charger_DualStatus.m_charging                  ) val |= CHARGER_STATUS_CHARGING;
    if(g_Charger_DualStatus.m_fault                     ) val |= CHARGER_STATUS_FAULT;
    if(g_Charger_DualStatus.m_overTemperature           ) val |= CHARGER_STATUS_OVER_TEMP;
    if(g_Charger_DualStatus.m_chargeComplete            ) val |= CHARGER_STATUS_CHARGE_COMPLETE;
    if(g_Charger_DualStatus.m_disableFlags != 0         ) val |= CHARGER_STATUS_DISABLED;

    Status = val;

    return TRUE;
}


BOOL Charger_DualStatus::Shutdown( UINT32 FlagMask )
{
    GLOBAL_LOCK(irq);

#if !defined(BUILD_RTM)
    UINT64 now             = Time_CurrentTicks();
    UINT32 wasDisableFlags = g_Charger_DualStatus.m_disableFlags;
#endif

    g_Charger_DualStatus.m_disableFlags |= FlagMask;

    ControlPins();

#if !defined(BUILD_RTM)
    if(wasDisableFlags != g_Charger_DualStatus.m_disableFlags)
    {
        DEBUG_TRACE3(TRACE_STAT_CHANGES, "SHUT:%08x %s=%012llx\r\n", FlagMask, g_Charger_DualStatus.m_disableFlags ? "ON " : "OFF", now );
    }
#endif  // !defined(BUILD_RTM)

    return TRUE;
}

BOOL Charger_DualStatus::Restart( UINT32 FlagMask )
{
    GLOBAL_LOCK(irq);

#if !defined(BUILD_RTM)
    UINT32 wasDisableFlags = g_Charger_DualStatus.m_disableFlags;
#endif

    g_Charger_DualStatus.m_disableFlags &= ~FlagMask;

    ControlPins();

#if !defined(BUILD_RTM)
    if(wasDisableFlags != g_Charger_DualStatus.m_disableFlags)
    {
        DEBUG_TRACE3(TRACE_STAT_CHANGES, "SHUT:%08x %s=%012llx\r\n", FlagMask, g_Charger_DualStatus.m_disableFlags ? "ON " : "OFF", Time_CurrentTicks() );
    }
#endif  // !defined(BUILD_RTM)

    return TRUE;
}

void Charger_DualStatus::SetTemperature( INT16 DegreesCelcius_x10 )
{
    ASSERT(!SystemState_Query(SYSTEM_STATE_ISR));

    GLOBAL_LOCK(irq);

    CHARGER_DUALSTATUS_CONFIG* Config = &g_DualStatus_Config;

    //
    // Change charging due to over/under temp without changing overall enable setting
    //
    if((DegreesCelcius_x10 >= Config->Over_Temperature_Shutdown ) ||
       (DegreesCelcius_x10 <= Config->Under_Temperature_Shutdown)  )
    {
        g_Charger_DualStatus.m_overTemperature = TRUE;
    }
    else
    {
        g_Charger_DualStatus.m_overTemperature = FALSE;
    }

    ControlPins();
}

//--//

void Charger_DualStatus::UpdateState()
{
    ASSERT_IRQ_MUST_BE_OFF();

    CHARGER_DUALSTATUS_CONFIG* Config = &g_DualStatus_Config;
    UINT32                     StateVal;

    StateVal  = (g_Charger_DualStatus.m_STAT1_PinState == Config->Stat1_GPIO_PIN.ActiveState) ? 2 : 0;
    StateVal += (g_Charger_DualStatus.m_STAT2_PinState == Config->Stat2_GPIO_PIN.ActiveState) ? 1 : 0;

    g_Charger_DualStatus.m_Charger_Debounce.Change( StateVal );
}

void Charger_DualStatus::ControlPins()
{
    ASSERT_IRQ_MUST_BE_OFF();

    if(g_Charger_DualStatus.m_ACPowerConnected_DEBOUNCED)
    {
        Status_Pins ( g_Charger_DualStatus.m_disableFlags == 0 );  // enable status only if we are not shutdown
        Shutdown_Pin( g_Charger_DualStatus.m_disableFlags != 0 );
    }
}

//--//

void Charger_DualStatus::Status_ISR( GPIO_PIN Pin, BOOL PinState, void* Param )
{
    GLOBAL_LOCK(irq);

    DEBUG_TRACE2(TRACE_GPIO_CHANGES, "I%02d:%1d\r\n", Pin, PinState);

    if(g_Charger_DualStatus.m_ACPowerConnected_DEBOUNCED)
    {
        BOOL* StatusPin = (BOOL*)Param;

        if(StatusPin)
        {
            *StatusPin = PinState;

            UpdateState();
        }
        else
        {
            ASSERT(0);
        }
    }
}


void Charger_DualStatus::ACPower_ISR( GPIO_PIN Pin, BOOL PinState, void* Param )
{
    GLOBAL_LOCK(irq);

    CHARGER_DUALSTATUS_CONFIG* Config = &g_DualStatus_Config;

    DEBUG_TRACE2(TRACE_GPIO_CHANGES, "I%02d:%1d\r\n", Pin, PinState);

    g_Charger_DualStatus.m_ACPR_PinState        = PinState;
    g_Charger_DualStatus.m_ACPowerConnected_RAW = (g_Charger_DualStatus.m_ACPR_PinState == Config->ACPower_GPIO_PIN.ActiveState);

    // we debounce only the OFF->ON transition
    if(g_Charger_DualStatus.m_ACPowerConnected_RAW)
    {
        ASSERT(!g_Charger_DualStatus.m_ACPowerConnected_DEBOUNCED);

        Shutdown_Pin( TRUE );     // for Swatch PS 6798, avoid oscillations and keep charger off during debounce period

        g_Charger_DualStatus.m_ACPower_Debounce.Change( g_Charger_DualStatus.m_ACPowerConnected_RAW );
    }
    else
    {
        g_Charger_DualStatus.m_ACPower_Debounce.Abort();

        ACPower_StateChange( (void*)(size_t)g_Charger_DualStatus.m_ACPowerConnected_RAW );
    }
}


void Charger_DualStatus::Decode_Status_Pins( void* Param )
{
    ASSERT_IRQ_MUST_BE_OFF();

    CHARGER_DUALSTATUS_CONFIG* Config   = &g_DualStatus_Config;
    UINT32                     StateVal = (UINT32)Param;

#if !defined(BUILD_RTM)
    UINT64 Now = Time_CurrentTicks();
#endif

    ASSERT(g_Charger_DualStatus.m_ACPowerConnected_DEBOUNCED);

    BOOL wasCharging       = g_Charger_DualStatus.m_charging;
    BOOL wasFault          = g_Charger_DualStatus.m_fault;
    BOOL wasChargeComplete = g_Charger_DualStatus.m_chargeComplete;
    BOOL set               = FALSE;

    g_Charger_DualStatus.m_charging = Config->StatusDecodes[StateVal].m_charging;
    g_Charger_DualStatus.m_fault    = Config->StatusDecodes[StateVal].m_fault;

#if 0
    // this is "unsticky" version of charge complete, only for testing purposes
    g_Charger_DualStatus.m_chargeComplete  = Config->StatusDecodes[StateVal].m_chargeComplete;
#else
    // we make "charge complete" stick on until we lose AC power
    if(g_Charger_DualStatus.m_chargeComplete == false)
    {
        g_Charger_DualStatus.m_chargeComplete = Config->StatusDecodes[StateVal].m_chargeComplete;
    }
#endif

    if(wasCharging != g_Charger_DualStatus.m_charging)
    {
        set = TRUE;

        if(g_Charger_DualStatus.m_charging)
        {
            DEBUG_TRACE1(TRACE_STAT_CHANGES, "CHRG  ON =%012llx\r\n", Now);
        }
        else
        {
            DEBUG_TRACE1(TRACE_STAT_CHANGES, "CHRG  OFF=%012llx\r\n", Now);
        }
    }

    if(wasFault != g_Charger_DualStatus.m_fault)
    {
        set = TRUE;

        if(g_Charger_DualStatus.m_fault)
        {
            DEBUG_TRACE1(TRACE_STAT_CHANGES, "FAULT ON =%012llx\r\n", Now);
        }
        else
        {
            DEBUG_TRACE1(TRACE_STAT_CHANGES, "FAULT OFF=%012llx\r\n", Now);
        }
    }

    if(wasChargeComplete != g_Charger_DualStatus.m_chargeComplete)
    {
        set = TRUE;

        if(g_Charger_DualStatus.m_chargeComplete)
        {
            DEBUG_TRACE1(TRACE_STAT_CHANGES, "DONE  ON =%012llx\r\n", Now);
        }
        else
        {
            DEBUG_TRACE1(TRACE_STAT_CHANGES, "DONE  OFF=%012llx\r\n", Now);
        }
    }

    if(set)
    {
        Events_Set( SYSTEM_EVENT_FLAG_CHARGER_CHANGE );
    }
}


void Charger_DualStatus::ACPower_StateChange( void* Param )
{
    ASSERT_IRQ_MUST_BE_OFF();

    UINT32 ACPowerState = (UINT32)Param;

    CHARGER_DUALSTATUS_CONFIG* Config = &g_DualStatus_Config;

    //DEBUG_TRACE3(TRACE_STAT_CHANGES, "ACPR  %1d %1d=%012llx\r\n", g_Charger_DualStatus.m_ACPowerConnected_DEBOUNCED, g_Charger_DualStatus.m_ACPowerConnected_DEBOUNCED, Time_CurrentTicks());

    // if we have AC power connected, then make sure the charger is on
    // but, only if we are not asked to shut it down
    if(g_Charger_DualStatus.m_ACPowerConnected_DEBOUNCED != ACPowerState)
    {
        g_Charger_DualStatus.m_ACPowerConnected_DEBOUNCED = ACPowerState;

        Events_Set( SYSTEM_EVENT_FLAG_CHARGER_CHANGE );

        // on change of AC power, reset voltage filter to more quickly track any quick rise or drop in voltage
        Battery_VoltageFilter_Reset();

        DEBUG_TRACE2(TRACE_STAT_CHANGES, "ACPR  %s=%012llx\r\n", g_Charger_DualStatus.m_ACPowerConnected_DEBOUNCED ? "ON " : "OFF", Time_CurrentTicks());

        if(g_Charger_DualStatus.m_ACPowerConnected_DEBOUNCED)
        {
            g_Charger_DualStatus.m_disableFlags = 0; // for Swatch bug PS 6798, re-enable charging regardless always on AC power ON transition

            ControlPins();
        }
        else
        {
            Status_Pins ( FALSE                          ); // loss of AC power, disable status pins
            Shutdown_Pin( Config->Shutdown_LowPowerState );

            g_Charger_DualStatus.m_charging       = FALSE;
            g_Charger_DualStatus.m_fault          = FALSE;
            g_Charger_DualStatus.m_chargeComplete = FALSE;
        }

        CPU_ProtectCommunicationGPIOs( !g_Charger_DualStatus.m_ACPowerConnected_DEBOUNCED );
    }
    else
    {
        ASSERT(!ACPowerState);    // we are likely to see lots of quick OFFs, but never to ONs
    }
}


void Charger_DualStatus::Shutdown_Pin( BOOL Active )
{
    ASSERT_IRQ_MUST_BE_OFF();

    CHARGER_DUALSTATUS_CONFIG* Config = &g_DualStatus_Config;

    if(GPIO_PIN_NONE != Config->Shutdown_GPIO_PIN.Pin)
    {
        if(Active)
        {
            if(Config->Shutdown_ActiveStateDriven)
            {
                // Shutdown ON: charger OFF: drive
                CPU_GPIO_EnableOutputPin( Config->Shutdown_GPIO_PIN.Pin, Config->Shutdown_GPIO_PIN.ActiveState );
            }
            else
            {
                // Shutdown ON: charger OFF: input HI-Z, no resistor
                CPU_GPIO_EnableInputPin( Config->Shutdown_GPIO_PIN.Pin, FALSE, NULL, GPIO_INT_NONE, Config->Shutdown_ActiveUndriven_Resistor );
            }
        }
        else
        {
            if(Config->Shutdown_InactiveStateDriven)
            {
                // Shutdown OFF: charger ON: drive
                CPU_GPIO_EnableOutputPin( Config->Shutdown_GPIO_PIN.Pin, !Config->Shutdown_GPIO_PIN.ActiveState );
            }
            else
            {
                // Shutdown OFF: charger ON: input HI-Z, no resistor, or resistor
                CPU_GPIO_EnableInputPin( Config->Shutdown_GPIO_PIN.Pin, FALSE, NULL, GPIO_INT_NONE, Config->Shutdown_InactiveUndriven_Resistor );
            }
        }
    }
}


void Charger_DualStatus::Status_Pins( BOOL Active )
{
    ASSERT_IRQ_MUST_BE_OFF();

    CHARGER_DUALSTATUS_CONFIG* Config = &g_DualStatus_Config;

    if(Active)
    {
        // enable status pin

        ASSERT(g_Charger_DualStatus.m_ACPowerConnected_DEBOUNCED);

        // charge will have cleared, so re-enable this before allowing "charging" to re-assert itself
        // the eventual ISR will cause the state change
        // we could miss a transition during the off period,
        // but by design the chip will reassert any real faults again (not timer faults, which are lost anyway)
        // we will not miss a charge on->off transition (used for charge complete), because the charger will restart the charging cycle, and complete quickly
        if(GPIO_PIN_NONE != Config->Stat1_GPIO_PIN.Pin)
        {
            // enable
            CPU_GPIO_EnableInputPin2( Config->Stat1_GPIO_PIN.Pin, FALSE, Status_ISR, (void*)&g_Charger_DualStatus.m_STAT1_PinState, GPIO_INT_EDGE_BOTH, Config->Stat1_Resistor );
            
            g_Charger_DualStatus.m_STAT1_PinState = CPU_GPIO_GetPinState( Config->Stat1_GPIO_PIN.Pin );

            DEBUG_TRACE2(TRACE_GPIO_CHANGES, "R%02d:%1d\r\n", Config->Stat1_GPIO_PIN.Pin, g_Charger_DualStatus.m_STAT1_PinState);
        }

        if(GPIO_PIN_NONE != Config->Stat2_GPIO_PIN.Pin)
        {
            // enable
            CPU_GPIO_EnableInputPin2( Config->Stat2_GPIO_PIN.Pin, FALSE, Status_ISR, (void*)&g_Charger_DualStatus.m_STAT2_PinState, GPIO_INT_EDGE_BOTH, Config->Stat2_Resistor );
            
            g_Charger_DualStatus.m_STAT2_PinState = CPU_GPIO_GetPinState( Config->Stat2_GPIO_PIN.Pin );

            DEBUG_TRACE2(TRACE_GPIO_CHANGES, "R%02d:%1d\r\n", Config->Stat2_GPIO_PIN.Pin, g_Charger_DualStatus.m_STAT2_PinState);
        }

        //
        // Here we grab the pre-charger enable values, and set the undebounced state
        // we expect that the charger will assert the charging/fault lines within the debounce time,
        // avoiding a false state being published.
        //
        UpdateState();
    }
    else
    {
        g_Charger_DualStatus.m_Charger_Debounce.Abort();

        // disable status pins
        if(GPIO_PIN_NONE != Config->Stat1_GPIO_PIN.Pin)
        {
            CPU_GPIO_EnableInputPin( Config->Stat1_GPIO_PIN.Pin, FALSE, NULL, GPIO_INT_NONE, Config->Stat1_Resistor );
        }

        if(GPIO_PIN_NONE != Config->Stat2_GPIO_PIN.Pin)
        {
            CPU_GPIO_EnableInputPin( Config->Stat2_GPIO_PIN.Pin, FALSE, NULL, GPIO_INT_NONE, Config->Stat2_Resistor );
        }
    }
}
