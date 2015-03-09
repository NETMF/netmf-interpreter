////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include <Drivers\Backlight\SPI\spi_backlight.h>


    #define SPI_BACKLIGHT_CS                    GPIO_PIN_NONE
    #define SPI_BACKLIGHT_CS_ACTIVE             FALSE
    #define SPI_BACKLIGHT_MSK_IDLE              FALSE
    #define SPI_BACKLIGHT_MSK_SAMPLE_EDGE       TRUE
    #define SPI_BACKLIGHT_16BIT_OP              TRUE
    #define SPI_BACKLIGHT_CLOCK_RATE_KHZ        1500
    #define SPI_BACKLIGHT_CS_SETUP_USEC         0
    #define SPI_BACKLIGHT_CS_HOLD_USEC          0
    #define SPI_BACKLIGHT_MODULE                GPIO_PIN_NONE

    #define BACKLIGHT_SPI_OFF_CMD               0
    #define BACKLIGHT_SPI_ON_CMD                0
    #define BACKLIGHT_SPI_CMD_REPEAT_COUNT      1

//--//

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata = "g_SPI_BackLight_Config"
#endif

SPI_BACKLIGHT_CONFIG g_SPI_BackLight_Config =
{
    { TRUE }, // HAL_DRIVER_CONFIG_HEADER Header;

    //--//
    { 
        SPI_BACKLIGHT_CS,
        SPI_BACKLIGHT_CS_ACTIVE,
        SPI_BACKLIGHT_MSK_IDLE,
        SPI_BACKLIGHT_MSK_SAMPLE_EDGE,
        SPI_BACKLIGHT_16BIT_OP,
        SPI_BACKLIGHT_CLOCK_RATE_KHZ,
        SPI_BACKLIGHT_CS_SETUP_USEC,
        SPI_BACKLIGHT_CS_HOLD_USEC,
        SPI_BACKLIGHT_MODULE
    }, // SPI_CONFIG Output_SPI_GPIO_PIN;
    
    BACKLIGHT_SPI_OFF_CMD,
    BACKLIGHT_SPI_ON_CMD,
    BACKLIGHT_SPI_CMD_REPEAT_COUNT,
        
    TRUE,                 // BOOL       On;
};

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata
#endif

