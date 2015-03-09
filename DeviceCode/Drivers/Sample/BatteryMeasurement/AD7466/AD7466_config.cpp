////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include <Drivers\BatteryMeasurement\AD7466\AD7466.h>

// this will be platform dependant, check whether themistor is used or not.
// Default to the stub one.
#if defined(DRIVER_THERMISTOR)
#define THERMISTOR_R_NOMINAL                        10000
#define THERMISTOR_T_NOMINAL_X100                   (25*100 + ZEROC_KELVINS_X100)
#define THERMISTOR_B_VAL                            4250

#else

#define THERMISTOR_R_NOMINAL                        0
#define THERMISTOR_T_NOMINAL_X100                   0
#define THERMISTOR_B_VAL                            0
#endif

//--//

#define BATTERY_TIMEOUT_CHARGING                    120         
#define BATTERY_TIMEOUT_CHARGED                     60          
//--//
#define BATTERY_LIFE_MIN                            10
#define BATTERY_LIFE_LOW                            20
#define BATTERY_LIFE_MED                            30
#define BATTERY_LIFE_MAX                            98

#define BATTERY_LIFE_FULLMIN                        40
#define BATTERY_LIFE_HYSTERESIS                     6

#define BATTERY_TIMEOUT_CHARGER                     5
#define BATTERY_TIMEOUT_BACKLIGHT                   5


//--//

#define ADMUX_EN_L                                  GPIO_PIN_NONE
#define ADMUX_A0                                    GPIO_PIN_NONE
#define ADMUX_A1                                    GPIO_PIN_NONE
#define AD7466_SPI_CS_GPIO_PIN                      GPIO_PIN_NONE
#define AD7466_SPI_CS_ACTIVE                        FALSE
#define AD7466_SPI_MSK_IDLE                         TRUE
#define AD7466_SPI_MSK_SAMPLE_EDGE                  TRUE
#define AD7466_SPI_CLOCK_RATE                       2300
#define AD7466_SPI_CS_SETUP_TIME                    5
#define AD7466_SPI_CS_HOLD_TIME                     0
#define AD7466_SPI_MODULE                           GPIO_PIN_NONE
#define AD7466_SPI_BUSYPIN                          GPIO_PIN_NONE
#define AD7466_SPI_BUSYPIN_ACTIVESTATE              FALSE
#define AD7466_ADC_BATTERY_ADC_VMAX                 1800
#define AD7466_ADC_BATTERY_OFFSET_MV                0
#define AD7466_ADC_MUX_BASE_RESISTOR                3240
#define AD7466_ADC_BATTERY_HIGH_DIVIDER_RESISTOR    24900
#define AD7466_ADC_THERMISTOR_RB                    AD7466_ADC_MUX_BASE_RESISTOR
#define AD7466_ADC_BATTERY_LOW_DIVIDER_RESISTOR     AD7466_ADC_MUX_BASE_RESISTOR

//--//

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata = "g_AD7466_Config"
#endif

AD7466_CONFIG g_AD7466_Config =
{
    { // BATTERY_COMMON_CONFIG CommonConfig;

        { TRUE }, // HAL_DRIVER_CONFIG_HEADER Header;

        //--//

        BATTERY_LIFE_MIN,          // UINT8  Battery_Life_Min;
        BATTERY_LIFE_LOW,          // UINT8  Battery_Life_Low;
        BATTERY_LIFE_MED,          // UINT8  Battery_Life_Med;
        BATTERY_LIFE_MAX,          // UINT8  Battery_Life_Max;
                                //
        BATTERY_LIFE_FULLMIN,      // UINT8  Battery_Life_FullMin;
        BATTERY_LIFE_HYSTERESIS,   // UINT8  Battery_Life_Hysteresis;
                                //
        BATTERY_TIMEOUT_CHARGING,  // UINT8  Battery_Timeout_Charging;
        BATTERY_TIMEOUT_CHARGED,   // UINT8  Battery_Timeout_Charged;
        BATTERY_TIMEOUT_CHARGER,   // UINT16 Battery_Timeout_Charger;
        BATTERY_TIMEOUT_BACKLIGHT, // UINT16 Battery_Timeout_Backlight;
    },

    ADMUX_EN_L,
    ADMUX_A0,
    ADMUX_A1,
    {
        { FALSE, FALSE },   // c_ADMUX_UNUSED_CHANNEL
        { TRUE,  FALSE },   // c_ADMUX_TEMP_CHANNEL
        { FALSE, TRUE },    // c_ADMUX_LIGHT_CHANNEL
        { TRUE,  TRUE },    // c_ADMUX_BATTERY_CHANNEL
    },

    { // SPI_CONFIGURATION SPI_Config;
        AD7466_SPI_CS_GPIO_PIN,         // GPIO_PIN       DeviceCS;
        AD7466_SPI_CS_ACTIVE,           // BOOL           CS_Active;
        AD7466_SPI_MSK_IDLE,            // BOOL           MSK_IDLE;
        AD7466_SPI_MSK_SAMPLE_EDGE,     // BOOL           MSK_SampleEdge;
        TRUE,                           // BOOL           MD_16bits;
        AD7466_SPI_CLOCK_RATE,          // UINT32         Clock_RateKHz;
        AD7466_SPI_CS_SETUP_TIME,       // UINT32         CS_Setup_uSecs;
        AD7466_SPI_CS_HOLD_TIME,        // UINT32         CS_Hold_uSecs;
        AD7466_SPI_MODULE,              // UINT16         SPI_mod;
        {
            AD7466_SPI_BUSYPIN,             // SPI busy pin
            AD7466_SPI_BUSYPIN_ACTIVESTATE, // SPI busy pin active state
        }
    },

    AD7466_ADC_THERMISTOR_RB,                   // UINT32                      Thermistor_Rb;
    THERMISTOR_R_NOMINAL,                       // UINT32                      Thermistor_R_Nominal;
    THERMISTOR_T_NOMINAL_X100,                  // INT32                       Thermistor_T_Nominal_x100;
    THERMISTOR_B_VAL,                           // INT32                       Thermistor_B_VAL;
    AD7466_ADC_BATTERY_LOW_DIVIDER_RESISTOR,    // UINT32                      Battery_Low_Divider_Resistor;
    AD7466_ADC_BATTERY_HIGH_DIVIDER_RESISTOR,   // UINT32                      Battery_High_Divider_Resistor;
    AD7466_ADC_BATTERY_ADC_VMAX,                // UINT16                      Battery_ADC_VMAX;
    AD7466_ADC_BATTERY_OFFSET_MV                // INT16                       Battery_Offset_mV;
};

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata
#endif


