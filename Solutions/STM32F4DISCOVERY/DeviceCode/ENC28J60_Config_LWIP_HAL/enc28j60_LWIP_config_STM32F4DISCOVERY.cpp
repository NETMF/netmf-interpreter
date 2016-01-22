#include <tinyhal.h>
#include <enc28j60_lwip_driver.h>

//--//

#define ENC28J60_INT                   ((2 * 16) + 14) // ((2 * 16) + 14) PC14 // ((0 * 16) + 14) PA14 //GPIO_PIN_NONE   // Socket 5 | Socket 6
#define ENC28J60_CS                    ((2 * 16) + 15) // ((2 * 16) + 15) PC13 // ((0 * 16) + 13) PA13 //GPIO_PIN_NONE   // Socket 5 | Socket 6
#define ENC28J60_CS_ACTIVE             FALSE
#define ENC28J60_MSK_IDLE              FALSE
#define ENC28J60_MSK_SAMPLE_EDGE       TRUE
#define ENC28J60_16BIT_OP              FALSE
#define ENC28J60_CLOCK_RATE_KHZ        8000 //25000
#define ENC28J60_CS_SETUP_USEC         0
#define ENC28J60_CS_HOLD_USEC          0
#define ENC28J60_MODULE                0 //GPIO_PIN_NONE
#define ENC28J60_BUSYPIN               GPIO_PIN_NONE
#define ENC28J60_BUSYPIN_ACTIVESTATE   FALSE

//--//

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata = "g_enc28j60_LWIP_Config"
#endif


ENC28J60_LWIP_DEVICE_CONFIG g_ENC28J60_LWIP_Config =
{
    {
        {                                   // ENC28J60_DRIVER_CONFIG
            {                               // SPI_CONFIGURATION
                ENC28J60_CS,
                ENC28J60_CS_ACTIVE,
                ENC28J60_MSK_IDLE,
                ENC28J60_MSK_SAMPLE_EDGE,
                ENC28J60_16BIT_OP,
                ENC28J60_CLOCK_RATE_KHZ,
                ENC28J60_CS_SETUP_USEC,
                ENC28J60_CS_HOLD_USEC,
                ENC28J60_MODULE,
				//GPIO_PIN_NONE,
                {
                    ENC28J60_BUSYPIN,
                    ENC28J60_BUSYPIN_ACTIVESTATE,
                },
            },
            
            ENC28J60_INT,                    // Interrupt Pin
        },
    }
};

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata
#endif


