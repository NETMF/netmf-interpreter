#include <tinyhal.h>
#include "loopback_driver.h"

//--//
#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata = "g_loopback_Config"
#endif

LOOPBACK_DRIVER_CONFIG g_LOOPBACK_Config =
{
    {
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
    },
    0,
    0
};

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata 
#endif

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rwdata
#endif

//--//

#if defined(DRIVER_LOOPBACK)

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rodata  
#endif

#if defined(ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED)
#pragma arm section rodata
#endif

#endif
