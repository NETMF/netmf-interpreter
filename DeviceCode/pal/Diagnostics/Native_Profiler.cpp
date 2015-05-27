////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include <CPU_WATCHDOG_decl.h>

#ifndef NATIVE_PROFILER_CPP
#define NATIVE_PROFILER_CPP

#define NATIVE_PROFILER_START_TAG 0xbaadf00d
#define NATIVE_PROFILER_END_TAG   0xd00fdaab

//--//
#if defined(ENABLE_NATIVE_PROFILER)

#include <Native_Profiler.h>

#pragma arm section rwdata = "SectionForProfilerBufferBegin"
UINT32 ProfilerBufferBegin __section("SectionForProfilerBufferBegin");
#pragma arm section rwdata

#pragma arm section rwdata = "SectionForProfilerBufferEnd"
UINT32 ProfilerBufferEnd   __section("SectionForProfilerBufferEnd");
#pragma arm section rwdata

#pragma arm section code = "SectionForFlashOperations"

struct native_profiler_data s_native_profiler = {0, 0, 0, 0, 0, 0, 0, 0};

__section("SectionForFlashOperations") Native_Profiler::Native_Profiler()
{
    if(s_native_profiler.isOn)
    {
        s_native_profiler.isOn = FALSE;
        s_native_profiler.writtenData = TRUE;
        UINT64 entryTime = Native_Profiler_TimeInMicroseconds() - s_native_profiler.initTime;
        functionAddress = __return_address();

        if(s_native_profiler.useBuffer)
        {
            *s_native_profiler.position++ = functionAddress;
            *s_native_profiler.position++ = (UINT32)(entryTime);
            if((s_native_profiler.position + 2) >= &ProfilerBufferEnd)
            {
                *s_native_profiler.position++ = NATIVE_PROFILER_END_TAG;
                Native_Profiler_Dump();
            }
        }
        else
        {
            UINT32 tempBuffer[2];
            tempBuffer[0] = functionAddress;
            tempBuffer[1] = (UINT32)(entryTime);
            Native_Profiler_WriteToCOM(tempBuffer, 8);
        }
        s_native_profiler.isOn = TRUE;
    }
}


__section("SectionForFlashOperations") Native_Profiler::~Native_Profiler()
{
    if(s_native_profiler.isOn)
    {
        s_native_profiler.isOn = FALSE;
        s_native_profiler.writtenData = TRUE;
        UINT64 returnTime = Native_Profiler_TimeInMicroseconds() - s_native_profiler.initTime;

        if(s_native_profiler.useBuffer)
        {
            *s_native_profiler.position++ = ((unsigned int)functionAddress) + 1;
            *s_native_profiler.position++ = (UINT32)(returnTime);
            if((s_native_profiler.position + 2) >= &ProfilerBufferEnd)
            {
                *s_native_profiler.position++ = NATIVE_PROFILER_END_TAG;
                Native_Profiler_Dump();
            }
        }
        else
        {
            UINT32 tempBuffer[2];
            tempBuffer[0] = ((unsigned int)functionAddress) + 1;
            tempBuffer[1] = (UINT32)(returnTime);
            Native_Profiler_WriteToCOM(tempBuffer, 8);
        }
        s_native_profiler.isOn = TRUE;
    }
}


void __section("SectionForFlashOperations") Native_Profiler_Init()
{
    s_native_profiler.ticksPerMicrosecond = CPU_TicksPerSecond() / 1000000;
    unsigned int availableSpace   = &ProfilerBufferEnd - &ProfilerBufferBegin;
    s_native_profiler.useBuffer   = FALSE;
    s_native_profiler.initTime    = Native_Profiler_TimeInMicroseconds();
    s_native_profiler.writtenData = FALSE;

    if(availableSpace >= 10)
    {
        GLOBAL_LOCK(irq);
        
        s_native_profiler.useBuffer = TRUE;
        s_native_profiler.isOn      = TRUE;
        UINT64 timeBegin;
        UINT64 timeEnd;
        UINT64 timeToGetTicks;
        s_native_profiler.position = &ProfilerBufferBegin;

        // Measure time introduced by profiling.

        timeToGetTicks              = Time_CurrentTicks();
        timeBegin                   = Time_CurrentTicks();
        {
            Native_Profiler testObj;
        }
        {
            Native_Profiler testObj;
        }
        timeEnd                     = Time_CurrentTicks();
        
        // Count mean time spend in Native_Profiler constructor and destructor.
        s_native_profiler.engineTimeOffset = (UINT32)((timeEnd - timeBegin - 2 * (timeBegin - timeToGetTicks)) / (2 * s_native_profiler.ticksPerMicrosecond));
        s_native_profiler.position         = &ProfilerBufferBegin;
        *s_native_profiler.position++      = NATIVE_PROFILER_START_TAG;
        *s_native_profiler.position++      = s_native_profiler.engineTimeOffset;
        s_native_profiler.isOn             = FALSE;
    }
}


void __section("SectionForFlashOperations")  Native_Profiler_Dump()
{
    lcd_printf("Buffer is full. Dumping...\r\n");
    UINT64 time1, time2;
    time1 = Native_Profiler_TimeInMicroseconds();

    USART_Initialize( ConvertCOM_ComPort(USART_DEFAULT_PORT), HalSystemConfig.USART_DefaultBaudRate, USART_PARITY_NONE, 8, USART_STOP_BITS_ONE, USART_FLOW_NONE );

    // clear_watchdog
    // if we do not disable the watchdog, it can be called while we dump data
    Watchdog_GetSetEnabled( FALSE, TRUE );
    
    // flush existing characters
    USART_Flush( ConvertCOM_ComPort(USART_DEFAULT_PORT) );
    // write string
    char *offset = (char *)&ProfilerBufferBegin;
    UINT32 size = (char *)s_native_profiler.position - (char *)&ProfilerBufferBegin;
    do
    {
        UINT32 bytes_written = USART_Write(ConvertCOM_ComPort(USART_DEFAULT_PORT),
                                                              offset, size);
        offset += bytes_written;
        size -= bytes_written;
    } while(size);
    // flush new characters
    USART_Flush( ConvertCOM_ComPort(USART_DEFAULT_PORT) );
    Watchdog_GetSetEnabled( TRUE, TRUE );
    time2 = Native_Profiler_TimeInMicroseconds();
    s_native_profiler.initTime += (time2 - time1);
    s_native_profiler.position = &ProfilerBufferBegin;
    *s_native_profiler.position++ = NATIVE_PROFILER_START_TAG;
    *s_native_profiler.position++ = s_native_profiler.engineTimeOffset;
    s_native_profiler.writtenData = FALSE;
}


void __section("SectionForFlashOperations")  Native_Profiler_WriteToCOM(void *buffer, UINT32 size)
{
    UINT64 time1, time2;
    time1 = Native_Profiler_TimeInMicroseconds();

    USART_Initialize( ConvertCOM_ComPort(USART_DEFAULT_PORT), HalSystemConfig.USART_DefaultBaudRate, USART_PARITY_NONE, 8, USART_STOP_BITS_ONE, USART_FLOW_NONE );

    // disable watchdog
    Watchdog_GetSetEnabled( FALSE, TRUE );
    
    // flush existing characters
    USART_Flush( ConvertCOM_ComPort(USART_DEFAULT_PORT) );
    // write string
    char *offset = (char *) buffer;
    do
    {
        UINT32 bytes_written = USART_Write(ConvertCOM_ComPort(USART_DEFAULT_PORT),
                                                              offset, size);
        offset += bytes_written;
        size -= bytes_written;
    } while(size);

    // flush new characters
    USART_Flush( ConvertCOM_ComPort(USART_DEFAULT_PORT) );
    Watchdog_GetSetEnabled( TRUE, TRUE );
    
    time2 = Native_Profiler_TimeInMicroseconds();

    s_native_profiler.initTime += (time2 - time1);
}


UINT64 __section("SectionForFlashOperations") Native_Profiler_TimeInMicroseconds()
{
    return Time_CurrentTicks() / s_native_profiler.ticksPerMicrosecond;
}


void __section("SectionForFlashOperations") Native_Profiler_Start()
{
    if(s_native_profiler.lock == FALSE)
    {
        s_native_profiler.isOn = TRUE;
    }
}


void __section("SectionForFlashOperations") Native_Profiler_Stop()
{
    if(s_native_profiler.isOn && s_native_profiler.writtenData && s_native_profiler.lock == FALSE)
    {
        s_native_profiler.isOn = FALSE;
        if(s_native_profiler.useBuffer)
        {
            // Send all data that is still in the buffer.
            *s_native_profiler.position++ = NATIVE_PROFILER_END_TAG;
            UINT32 size = (char *)s_native_profiler.position - (char *)&ProfilerBufferBegin;
            Native_Profiler_WriteToCOM(&ProfilerBufferBegin, size);
            s_native_profiler.position = &ProfilerBufferBegin;
            *s_native_profiler.position++ = NATIVE_PROFILER_START_TAG;
            *s_native_profiler.position++ = s_native_profiler.engineTimeOffset;
            s_native_profiler.writtenData = FALSE;
        }
    }
}


void __section("SectionForFlashOperations") Native_Profiler_Lock()
{
    s_native_profiler.lock = TRUE;
}


void __section("SectionForFlashOperations") Native_Profiler_Unlock()
{
    s_native_profiler.lock = FALSE;
}

#pragma arm section code


#endif  // defined(ENABLE_NATIVE_PROFILER)
#endif  // defined(NATIVE_PROFILER_CPP)
