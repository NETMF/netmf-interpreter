////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef NATIVE_PROFILE_H
#define NATIVE_PROFILE_H

//--//

#if defined(ENABLE_NATIVE_PROFILER)

#include <tinyhal.h>

// Set default to profiling all functions.
#ifndef NATIVE_PROFILE_CLR
#define NATIVE_PROFILE_CLR  0xFFFFFFFF 
#endif
#ifndef NATIVE_PROFILE_PAL
#define NATIVE_PROFILE_PAL  0xFFFFFFFF
#endif
#ifndef NATIVE_PROFILE_HAL
#define NATIVE_PROFILE_HAL  0xFFFFFFFF
#endif
#ifndef NATIVE_PROFILE_USER
#define NATIVE_PROFILE_USER 0xFFFFFFFF
#endif


void Native_Profiler_Init();
void Native_Profiler_Dump();
void Native_Profiler_WriteToCOM(void *buffer, UINT32 size);
UINT64 Native_Profiler_TimeInMicroseconds();
void Native_Profiler_Start();
void Native_Profiler_Stop();
void Native_Profiler_Lock();
void Native_Profiler_Unlock();

class Native_Profiler
{
private:
         UINT32 functionAddress;
public:
         Native_Profiler();
         ~Native_Profiler();
};

struct native_profiler_data
{
    UINT64 initTime;
    UINT32 *position;
    UINT32 ticksPerMicrosecond;
    UINT32 engineTimeOffset;
    BOOL useBuffer;
    BOOL isOn;
    BOOL writtenData;
    BOOL lock;
};

#define NATIVE_PROFILE_LEVEL_CLR    0x00000001
#define NATIVE_PROFILE_LEVEL_HAL    0x00000002
#define NATIVE_PROFILE_LEVEL_PAL    0x00000004
#define NATIVE_PROFILE_LEVEL_USER   0x00000008

//--//  CLR level

#define NATIVE_PROFILE_CLR_DEBUGGER__flag              0x00000001
#define NATIVE_PROFILE_CLR_UTF8_DECODER__flag          0x00000002
#define NATIVE_PROFILE_CLR_CORE__flag                  0x00000004
#define NATIVE_PROFILE_CLR_MESSAGING__flag             0x00000008
#define NATIVE_PROFILE_CLR_SERIALIZATION__flag         0x00000010
#define NATIVE_PROFILE_CLR_NETWORK__flag               0x00000020
#define NATIVE_PROFILE_CLR_I2C__flag                   0x00000040
#define NATIVE_PROFILE_CLR_DIAGNOSTICS__flag           0x00000080
#define NATIVE_PROFILE_CLR_HARDWARE__flag              0x00000100
#define NATIVE_PROFILE_CLR_GRAPHICS__flag              0x00000200
#define NATIVE_PROFILE_CLR_STARTUP__flag               0x00000400
#define NATIVE_PROFILE_CLR_HEAP_PERSISTENCE__flag      0x00000800
#define NATIVE_PROFILE_CLR_IOPORT__flag                0x00001000
#define NATIVE_PROFILE_CLR_IO__flag                    0x00002000
#define NATIVE_PROFILE_CLR__flag_ALL                   0x00002FFF

//--//  HAL level

#define NATIVE_PROFILE_HAL_PROCESSOR_TIME__flag        0x00000001
#define NATIVE_PROFILE_HAL_PROCESSOR_EBIU__flag        0x00000002
#define NATIVE_PROFILE_HAL_PROCESSOR_SECURITY__flag    0x00000004
#define NATIVE_PROFILE_HAL_DRIVERS_FLASH__flag         0x00000008
#define NATIVE_PROFILE_HAL_PROCESSOR_TIMER__flag       0x00000010
#define NATIVE_PROFILE_HAL_PROCESSOR_I2C__flag         0x00000020
#define NATIVE_PROFILE_HAL_DRIVERS_BACKLIGHT__flag     0x00000040
#define NATIVE_PROFILE_HAL_PROCESSOR_CMU__flag         0x00000080
#define NATIVE_PROFILE_HAL_PROCESSOR_DMA__flag         0x00000100
#define NATIVE_PROFILE_HAL_PROCESSOR_GPIO__flag        0x00000200
#define NATIVE_PROFILE_HAL_DRIVERS_DISPLAY__flag       0x00000400
#define NATIVE_PROFILE_HAL_PROCESSOR_WATCHDOG__flag    0x00000800
#define NATIVE_PROFILE_HAL_PROCESSOR_SPI__flag         0x00001000
#define NATIVE_PROFILE_HAL_PROCESSOR_LCD__flag         0x00002000
#define NATIVE_PROFILE_HAL_DRIVERS_ETHERNET__flag      0x00004000
#define NATIVE_PROFILE_HAL_PROCESSOR_POWER__flag       0x00008000
#define NATIVE_PROFILE_HAL__flag_ALL                   0x0000FFFF

//--//  PAL level

#define NATIVE_PROFILE_PAL_EVENTS__flag                0x00000001
#define NATIVE_PROFILE_PAL_COM__flag                   0x00000002
#define NATIVE_PROFILE_PAL_HEAP__flag                  0x00000004
#define NATIVE_PROFILE_PAL_ASYNC_PROC_CALL__flag       0x00000008
#define NATIVE_PROFILE_PAL_FLASH__flag                 0x00000010
#define NATIVE_PROFILE_PAL_NETWORK__flag               0x00000020
#define NATIVE_PROFILE_PAL_CRT__flag                   0x00000040
#define NATIVE_PROFILE_PAL_IO__flag                    0x00000080
#define NATIVE_PROFILE_PAL_BUTTONS__flag               0x00000100
#define NATIVE_PROFILE_PAL_GRAPHICS__flag              0x00000200
#define NATIVE_PROFILE_PAL__flag_ALL                   0x000002FF

//--// User level (Porting Kit user defined behavior)

#define NATIVE_PROFILE_USER_1__flag                    0x00000001
#define NATIVE_PROFILE_USER_2__flag                    0x00000002
#define NATIVE_PROFILE_USER_3__flag                    0x00000004


#if NATIVE_PROFILE_CLR & NATIVE_PROFILE_CLR_DEBUGGER__flag
    #define NATIVE_PROFILE_CLR_DEBUGGER() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_CLR_DEBUGGER()
#endif

#if NATIVE_PROFILE_CLR & NATIVE_PROFILE_CLR_UTF8_DECODER__flag
    #define NATIVE_PROFILE_CLR_UTF8_DECODER() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_CLR_UTF8_DECODER()
#endif

#if NATIVE_PROFILE_CLR & NATIVE_PROFILE_CLR_CORE__flag
    #define NATIVE_PROFILE_CLR_CORE() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_CLR_CORE()
#endif

#if NATIVE_PROFILE_CLR & NATIVE_PROFILE_CLR_MESSAGING__flag
    #define NATIVE_PROFILE_CLR_MESSAGING() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_CLR_MESSAGING()
#endif

#if NATIVE_PROFILE_CLR & NATIVE_PROFILE_CLR_SERIALIZATION__flag
    #define NATIVE_PROFILE_CLR_SERIALIZATION() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_CLR_SERIALIZATION()
#endif

#if NATIVE_PROFILE_CLR & NATIVE_PROFILE_CLR_NETWORK__flag
    #define NATIVE_PROFILE_CLR_NETWORK() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_CLR_NETWORK()
#endif

#if NATIVE_PROFILE_CLR & NATIVE_PROFILE_CLR_I2C__flag
    #define NATIVE_PROFILE_CLR_I2C() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_CLR_I2C()
#endif

#if NATIVE_PROFILE_CLR & NATIVE_PROFILE_CLR_DIAGNOSTICS__flag
    #define NATIVE_PROFILE_CLR_DIAGNOSTICS() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_CLR_DIAGNOSTICS()
#endif

#if NATIVE_PROFILE_CLR & NATIVE_PROFILE_CLR_HARDWARE__flag
    #define NATIVE_PROFILE_CLR_HARDWARE() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_CLR_HARDWARE()
#endif

#if NATIVE_PROFILE_CLR & NATIVE_PROFILE_CLR_GRAPHICS__flag
    #define NATIVE_PROFILE_CLR_GRAPHICS() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_CLR_GRAPHICS()
#endif

#if NATIVE_PROFILE_CLR & NATIVE_PROFILE_CLR_STARTUP__flag
    #define NATIVE_PROFILE_CLR_STARTUP() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_CLR_STARTUP()
#endif

#if NATIVE_PROFILE_CLR & NATIVE_PROFILE_CLR_HEAP_PERSISTENCE__flag
    #define NATIVE_PROFILE_CLR_HEAP_PERSISTENCE() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_CLR_HEAP_PERSISTENCE()
#endif

#if NATIVE_PROFILE_CLR & NATIVE_PROFILE_CLR_IOPORT__flag
    #define NATIVE_PROFILE_CLR_IOPORT() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_CLR_IOPORT()
#endif

#if NATIVE_PROFILE_CLR & NATIVE_PROFILE_CLR_IO__flag
    #define NATIVE_PROFILE_CLR_IO() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_CLR_IO()
#endif

#if NATIVE_PROFILE_HAL & NATIVE_PROFILE_HAL_PROCESSOR_TIME__flag
    #define NATIVE_PROFILE_HAL_PROCESSOR_TIME() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_HAL_PROCESSOR_TIME()
#endif

#if NATIVE_PROFILE_HAL & NATIVE_PROFILE_HAL_PROCESSOR_EBIU__flag
    #define NATIVE_PROFILE_HAL_PROCESSOR_EBIU() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_HAL_PROCESSOR_EBIU()
#endif

#if NATIVE_PROFILE_HAL & NATIVE_PROFILE_HAL_PROCESSOR_SECURITY__flag
    #define NATIVE_PROFILE_HAL_PROCESSOR_SECURITY() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_HAL_PROCESSOR_SECURITY()
#endif

#if NATIVE_PROFILE_HAL & NATIVE_PROFILE_HAL_DRIVERS_FLASH__flag
    #define NATIVE_PROFILE_HAL_DRIVERS_FLASH() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_HAL_DRIVERS_FLASH()
#endif

#if NATIVE_PROFILE_HAL & NATIVE_PROFILE_HAL_PROCESSOR_TIMER__flag
    #define NATIVE_PROFILE_HAL_PROCESSOR_TIMER() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_HAL_PROCESSOR_TIMER()
#endif

#if NATIVE_PROFILE_HAL & NATIVE_PROFILE_HAL_PROCESSOR_I2C__flag
    #define NATIVE_PROFILE_HAL_PROCESSOR_I2C() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_HAL_PROCESSOR_I2C()
#endif

#if NATIVE_PROFILE_HAL & NATIVE_PROFILE_HAL_DRIVERS_BACKLIGHT__flag
    #define NATIVE_PROFILE_HAL_DRIVERS_BACKLIGHT() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_HAL_DRIVERS_BACKLIGHT()
#endif

#if NATIVE_PROFILE_HAL & NATIVE_PROFILE_HAL_PROCESSOR_CMU__flag
    #define NATIVE_PROFILE_HAL_PROCESSOR_CMU() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_HAL_PROCESSOR_CMU()
#endif

#if NATIVE_PROFILE_HAL & NATIVE_PROFILE_HAL_PROCESSOR_DMA__flag
    #define NATIVE_PROFILE_HAL_PROCESSOR_DMA() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_HAL_PROCESSOR_DMA()
#endif

#if NATIVE_PROFILE_HAL & NATIVE_PROFILE_HAL_PROCESSOR_GPIO__flag
    #define NATIVE_PROFILE_HAL_PROCESSOR_GPIO() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_HAL_PROCESSOR_GPIO()
#endif

#if NATIVE_PROFILE_HAL & NATIVE_PROFILE_HAL_DRIVERS_DISPLAY__flag
    #define NATIVE_PROFILE_HAL_DRIVERS_DISPLAY() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_HAL_DRIVERS_DISPLAY()
#endif

#if NATIVE_PROFILE_HAL & NATIVE_PROFILE_HAL_PROCESSOR_WATCHDOG__flag
    #define NATIVE_PROFILE_HAL_PROCESSOR_WATCHDOG() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_HAL_PROCESSOR_WATCHDOG()
#endif

#if NATIVE_PROFILE_HAL & NATIVE_PROFILE_HAL_PROCESSOR_SPI__flag
    #define NATIVE_PROFILE_HAL_PROCESSOR_SPI() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_HAL_PROCESSOR_SPI()
#endif

#if NATIVE_PROFILE_HAL & NATIVE_PROFILE_HAL_PROCESSOR_LCD__flag
    #define NATIVE_PROFILE_HAL_PROCESSOR_LCD() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_HAL_PROCESSOR_LCD()
#endif

#if NATIVE_PROFILE_HAL & NATIVE_PROFILE_HAL_DRIVERS_ETHERNET__flag
    #define NATIVE_PROFILE_HAL_DRIVERS_ETHERNET() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_HAL_DRIVERS_ETHERNET()
#endif

#if NATIVE_PROFILE_HAL & NATIVE_PROFILE_HAL_PROCESSOR_POWER__flag
    #define NATIVE_PROFILE_HAL_PROCESSOR_POWER() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_HAL_PROCESSOR_POWER()
#endif

#if NATIVE_PROFILE_PAL & NATIVE_PROFILE_PAL_EVENTS__flag
    #define NATIVE_PROFILE_PAL_EVENTS() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_PAL_EVENTS()
#endif

#if NATIVE_PROFILE_PAL & NATIVE_PROFILE_PAL_COM__flag
    #define NATIVE_PROFILE_PAL_COM() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_PAL_COM()
#endif

#if NATIVE_PROFILE_PAL & NATIVE_PROFILE_PAL_HEAP__flag
    #define NATIVE_PROFILE_PAL_HEAP() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_PAL_HEAP()
#endif

#if NATIVE_PROFILE_PAL & NATIVE_PROFILE_PAL_ASYNC_PROC_CALL__flag
    #define NATIVE_PROFILE_PAL_ASYNC_PROC_CALL() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_PAL_ASYNC_PROC_CALL()
#endif

#if NATIVE_PROFILE_PAL & NATIVE_PROFILE_PAL_FLASH__flag
    #define NATIVE_PROFILE_PAL_FLASH() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_PAL_FLASH()
#endif

#if NATIVE_PROFILE_PAL & NATIVE_PROFILE_PAL_NETWORK__flag
    #define NATIVE_PROFILE_PAL_NETWORK() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_PAL_NETWORK()
#endif

#if NATIVE_PROFILE_PAL & NATIVE_PROFILE_PAL_CRT__flag
    #define NATIVE_PROFILE_PAL_CRT() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_PAL_CRT()
#endif

#if NATIVE_PROFILE_PAL & NATIVE_PROFILE_PAL_IO__flag
    #define NATIVE_PROFILE_PAL_IO() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_PAL_IO()
#endif

#if NATIVE_PROFILE_PAL & NATIVE_PROFILE_PAL_BUTTONS__flag
    #define NATIVE_PROFILE_PAL_BUTTONS() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_PAL_BUTTONS()
#endif

#if NATIVE_PROFILE_PAL & NATIVE_PROFILE_PAL_GRAPHICS__flag
    #define NATIVE_PROFILE_PAL_GRAPHICS() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_PAL_GRAPHICS()
#endif


//--//  User defined profiling macros (Porting Kit user defined behavior)

#if NATIVE_PROFILE_USER & NATIVE_PROFILE_USER_1__flag
    #define NATIVE_PROFILE_USER_1() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_USER_1()
#endif

#if NATIVE_PROFILE_USER & NATIVE_PROFILE_USER_2__flag
    #define NATIVE_PROFILE_USER_2() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_USER_2()
#endif

#if NATIVE_PROFILE_USER & NATIVE_PROFILE_USER_3__flag
    #define NATIVE_PROFILE_USER_3() Native_Profiler profiler_obj
#else
    #define NATIVE_PROFILE_USER_3()
#endif


#else // defined(ENABLE_NATIVE_PROFILER)

#define NATIVE_PROFILE_CLR_DEBUGGER()
#define NATIVE_PROFILE_CLR_UTF8_DECODER()
#define NATIVE_PROFILE_CLR_CORE()
#define NATIVE_PROFILE_CLR_MESSAGING()
#define NATIVE_PROFILE_CLR_SERIALIZATION()
#define NATIVE_PROFILE_CLR_NETWORK()
#define NATIVE_PROFILE_CLR_I2C()
#define NATIVE_PROFILE_CLR_DIAGNOSTICS()
#define NATIVE_PROFILE_CLR_HARDWARE()
#define NATIVE_PROFILE_CLR_GRAPHICS()
#define NATIVE_PROFILE_CLR_STARTUP()
#define NATIVE_PROFILE_CLR_HEAP_PERSISTENCE()
#define NATIVE_PROFILE_CLR_IOPORT()
#define NATIVE_PROFILE_CLR_IO()

#define NATIVE_PROFILE_HAL_PROCESSOR_TIME()
#define NATIVE_PROFILE_HAL_PROCESSOR_EBIU()
#define NATIVE_PROFILE_HAL_PROCESSOR_SECURITY()
#define NATIVE_PROFILE_HAL_DRIVERS_FLASH()
#define NATIVE_PROFILE_HAL_PROCESSOR_TIMER()
#define NATIVE_PROFILE_HAL_PROCESSOR_I2C()
#define NATIVE_PROFILE_HAL_DRIVERS_BACKLIGHT()
#define NATIVE_PROFILE_HAL_PROCESSOR_CMU()
#define NATIVE_PROFILE_HAL_PROCESSOR_DMA()
#define NATIVE_PROFILE_HAL_PROCESSOR_GPIO()
#define NATIVE_PROFILE_HAL_DRIVERS_DISPLAY()
#define NATIVE_PROFILE_HAL_PROCESSOR_WATCHDOG()
#define NATIVE_PROFILE_HAL_PROCESSOR_SPI()
#define NATIVE_PROFILE_HAL_PROCESSOR_LCD()
#define NATIVE_PROFILE_HAL_DRIVERS_ETHERNET()
#define NATIVE_PROFILE_HAL_PROCESSOR_POWER()

#define NATIVE_PROFILE_PAL_EVENTS()
#define NATIVE_PROFILE_PAL_COM()
#define NATIVE_PROFILE_PAL_HEAP()
#define NATIVE_PROFILE_PAL_ASYNC_PROC_CALL()
#define NATIVE_PROFILE_PAL_FLASH()
#define NATIVE_PROFILE_PAL_NETWORK()
#define NATIVE_PROFILE_PAL_CRT()
#define NATIVE_PROFILE_PAL_IO()
#define NATIVE_PROFILE_PAL_BUTTONS()
#define NATIVE_PROFILE_PAL_GRAPHICS()


#define NATIVE_PROFILE_USER_1()
#define NATIVE_PROFILE_USER_2()
#define NATIVE_PROFILE_USER_3()

#endif  // defined(ENABLE_NATIVE_PROFILER)
#endif  // defined(NATIVE_PROFILER_H)
