#ifndef _CMSIS_GENERIC_H
#define _CMSIS_GENERIC_H

#define __CMSIS_GENERIC    /* disable implementation specific functions (i.e. NVIC and Systick ) */

#if defined (CORTEX_M7)
    #include "core_cm7.h"
#elif defined (CORTEX_M4)
    #include "core_cm4.h"
#elif defined (CORTEX_M3)
    #include "core_cm3.h"
#elif defined (CORTEX_M0)
    #include "core_cm0.h"
#elif defined (CORTEX_M0PLUS)
    #include "core_cm0plus.h"
#else
    #error "Processor not specified or unsupported."
#endif

#endif
