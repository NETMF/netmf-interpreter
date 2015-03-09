////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include "AD7466.h"

//--//

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata = "g_AD7466_Driver"
#endif

AD7466_Driver g_AD7466_Driver;

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section zidata
#endif

//--//

void VALUE_FILTER::Initialize( INT32 values )
{
    ASSERT(values > 0);

    m_numValues     = 0;      // INT32 m_numValues;
    m_maxValues     = values; // INT32 m_maxValues;
    m_averagedValue = 0;      // INT64 m_averagedValue;
}

void VALUE_FILTER::Add( INT32 NewValue )
{
    ASSERT(m_maxValues > 0);

    INT64 value = m_averagedValue;

    if(m_numValues < m_maxValues)
    {
        m_numValues += 1;

    }
    else
    {
        value = ScaleAndRound( value * (m_maxValues - 1), m_maxValues );
    }

    m_averagedValue = value + NewValue;
}

INT32 VALUE_FILTER::Get()
{
    if(m_numValues)
    {
        return ScaleAndRound( m_averagedValue, m_numValues );
    }
    
    return 0;
}

//--//

BOOL AD7466_Driver::Initialize()
{
    AD7466_CONFIG* Config = &g_AD7466_Config;

    HAL_CONFIG_BLOCK::ApplyConfig( Config->GetDriverName(), Config, sizeof(*Config) );

    g_AD7466_Driver.m_Rb = Config->Thermistor_Rb;  // 3.24k nominally, but may change for earlier P3s to 32.4k

    VoltageFilter_Reset();

    // General Rule: Enable all GPIO OUTPUTs to peripherals before checking inputs from it

    // enable the CS for the ADC and bring it high (inactive)
    CPU_GPIO_EnableOutputPin( Config->SPI_Config.DeviceCS, !Config->SPI_Config.CS_Active );

    // enable the ADMUX GPIO output control lines, and leave it inactive
    CPU_GPIO_EnableOutputPin( Config->ADMUX_EN_L_GPIO_PIN, TRUE  );
    CPU_GPIO_EnableOutputPin( Config->ADMUX_A0_GPIO_PIN,   FALSE );
    CPU_GPIO_EnableOutputPin( Config->ADMUX_A1_GPIO_PIN,   FALSE );

    // read the battery temp, at init, to:
    // 1. clear the over temp state set as default
    // 2. decide which bottom resistor we have
    INT32 DegreesCelcius_x10;

    if(!Temperature( DegreesCelcius_x10 ))
    {
        ASSERT(0);
        return FALSE;
    }

    return TRUE;
}


BOOL AD7466_Driver::Uninitialize()
{
    AD7466_CONFIG* Config = &g_AD7466_Config;

    // disable the CS for the ADC and bring it high (inactive)
    CPU_GPIO_EnableInputPin( Config->SPI_Config.DeviceCS, FALSE, NULL, GPIO_INT_NONE, RESISTOR_PULLUP );

    // disable the ADMUX GPIO output control lines, and leave it inactive
    CPU_GPIO_EnableInputPin( Config->ADMUX_EN_L_GPIO_PIN, FALSE, NULL, GPIO_INT_NONE, RESISTOR_PULLUP );
    CPU_GPIO_EnableInputPin( Config->ADMUX_A0_GPIO_PIN,   FALSE, NULL, GPIO_INT_NONE, RESISTOR_PULLUP );
    CPU_GPIO_EnableInputPin( Config->ADMUX_A1_GPIO_PIN,   FALSE, NULL, GPIO_INT_NONE, RESISTOR_PULLUP );

    return TRUE;
}

BOOL AD7466_Driver::Voltage( INT32& Millivolts )
{
    AD7466_CONFIG* Config = &g_AD7466_Config;

    UINT16 ADC_Value = Read( AD7466_CONFIG::c_ADMUX_BATTERY_CHANNEL );

    // undo gain resistors R44 and R50

    UINT64 Voltage;
    UINT32 GainN = Config->Battery_High_Divider_Resistor + Config->Battery_Low_Divider_Resistor;
    UINT32 GainD = Config->Battery_Low_Divider_Resistor;

    Voltage = ADC_Value;
    Voltage *= Config->Battery_ADC_VMAX;        // Vdd = 1.8V nominally
    Voltage *= GainN;       // resistor divider
    Voltage /= (1 << 12);   // 12 bit A/D
    Voltage /= GainD;       // resistor divider too
    Voltage += Config->Battery_Offset_mV;

    // limit for bad readings (we should be off at this voltage)
    if(Voltage >= 2800)
    {
        // good reading, so add it to filter
        g_AD7466_Driver.m_VoltageFilter.Add( Voltage );
    }

    //printf("V=%4d mV, ADC=%03x\r\n", (UINT32) Voltage, ADC_Value);

    g_AD7466_Driver.m_MostRecent_Millivolts = g_AD7466_Driver.m_VoltageFilter.Get();

    Millivolts = g_AD7466_Driver.m_MostRecent_Millivolts;

    return TRUE;
}

BOOL AD7466_Driver::Temperature( INT32& DegreesCelcius_x10 )
{
    AD7466_CONFIG* Config = &g_AD7466_Config;

    UINT16 ADC_Value = Read( AD7466_CONFIG::c_ADMUX_TEMP_CHANNEL );

    // now find the value of the thermistor, Rt, knowing we have a base resistor, Rb
    // ADC mv = (Rb / (Rt + Rb)) * 1800, but voltage drops out in the end
    // so:
    //      mv/1800             = Rb / (Rt + Rb)
    //      mv/1800 * (Rt + Rb) = Rb
    //      Rt + Rb             = (Rb / (mv/1800))
    //      Rt                  = (Rb / (mv/1800)) - Rb
    //          mv = (ADC * 1800)/4096
    //      Rt                  = (Rb / (ADC/4096)) - Rb
    //      Rt                  = (Rb * 4096) / ADC - Rb
    //      Rt                  = (Rb * (4096/ADC - 1))
    //      Rt                  = (Rb * 4096 / ADC) - Rb)

    INT64 Rt;   // INT64 since an INT32 would limit us to 1 megohm Rb with a 12-bit ADC

    while(true)
    {
        // watch for div 0
        if(ADC_Value > 0)
        {
            Rt  = (g_AD7466_Driver.m_Rb * (1 << 12));
            Rt /= (UINT32) ADC_Value;
            Rt -= g_AD7466_Driver.m_Rb;
            ASSERT(Rt > 0);
        }
        else
        {
            // absurd reading
            Rt = 0xffffffff;
        }

        // hack alert! some P3s have a 32.4k resistor, which causes Rt to be
        if((Config->Thermistor_Rb == g_AD7466_Driver.m_Rb) && (Rt < 3000)) // about 52.7C with normal Rb, about 3.9C with Rb=32400, but calc as 3240
        {
            // we only change once (static INT32 Rb)
            // so this works as long as we start running at normal temps,
            // but if we boot up very hot, this can be wrong.
            debug_printf("AD7466:upgrading thermistor Rb from %d to %d Ohms\r\n", g_AD7466_Driver.m_Rb, g_AD7466_Driver.m_Rb*10 );

            g_AD7466_Driver.m_Rb *= 10;

            continue;
        }

        break;
    }

    //printf("Rt=%6lld ohms, ADC=%03x\r\n", Rt, ADC_Value);

    // limit for bad readings to < 1 Megohm
    if(Rt < 1000000)
    {
        // good reading, so add it to filter
        g_AD7466_Driver.m_ResistanceFilter.Add( Rt );
    }

    Rt = g_AD7466_Driver.m_ResistanceFilter.Get();

    ASSERT(Rt >= 0);

    INT32 Temperature = Thermistor_ResistanceToTemperature( (UINT32)(UINT64)Rt,
                                                            Config->Thermistor_R_Nominal,
                                                            Config->Thermistor_T_Nominal_x100,
                                                            Config->Thermistor_B_VAL
                                                          );

    // everytime we read the temperature (app controlled), we update the charger info too
    Charger_SetTemperature( Temperature );

    g_AD7466_Driver.m_MostRecent_DegreesCelcius_x10 = Temperature;

    DegreesCelcius_x10 = Temperature;

    return TRUE;
}

void AD7466_Driver::VoltageFilter_Reset()
{
    g_AD7466_Driver.m_VoltageFilter   .Initialize( 32 );
    g_AD7466_Driver.m_ResistanceFilter.Initialize( 32 );
}

//--//

UINT16 AD7466_Driver::Read( INT32 Channel )
{
    AD7466_CONFIG* Config = &g_AD7466_Config;

    UINT16 ADC_Value;
    UINT16 Write16;

    // select the NO3 on MAX4704
    CPU_GPIO_SetPinState( Config->ADMUX_A0_GPIO_PIN, Config->ADMUX_Address[Channel].A0 );
    CPU_GPIO_SetPinState( Config->ADMUX_A1_GPIO_PIN, Config->ADMUX_Address[Channel].A1 );

    // data that is written is ignored, but reduce energy by keeping output high
    Write16 = 0xffff;

    CPU_GPIO_SetPinState( Config->ADMUX_EN_L_GPIO_PIN, FALSE );    // bring enable active (low)

    // delay 5 uSecs per RAY after enabling MUX to allow inrush current to settle and voltage to stabilize
    HAL_Time_Sleep_MicroSeconds_InterruptEnabled( 5 );

    {
        // we must ensure a radio operation ISR doesn't intervene - keep this short!
        GLOBAL_LOCK(irq);

        // W
        // R
        CPU_SPI_Xaction_Start( Config->SPI_Config );

        {
            SPI_XACTION_16 Transaction;

            Transaction.Read16          = &ADC_Value;
            Transaction.ReadCount       = 1;
            Transaction.ReadStartOffset = 1-1;
            Transaction.Write16         = &Write16;
            Transaction.WriteCount      = 1;

            CPU_SPI_Xaction_nWrite16_nRead16( Transaction );
        }

        CPU_SPI_Xaction_Stop( Config->SPI_Config );
    }

    CPU_GPIO_SetPinState( Config->ADMUX_EN_L_GPIO_PIN, TRUE );    // bring enable inactive (high)

    // scale down by 1 bit, since this is how it returns the result
    ADC_Value >>= 1;

    return ADC_Value;
}
