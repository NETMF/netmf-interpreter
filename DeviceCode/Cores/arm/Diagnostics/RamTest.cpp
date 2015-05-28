////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

#include "ramtest.h"


BOOL RamWordTest( volatile UINT32* WordToTest )
{
    BOOL result = TRUE;

#if !defined(TARGETLOCATION_RAM)
    // order is important here if we have interrupts on IRQs might modify RAM areas after we save
    GLOBAL_LOCK(irq);

    UINT32 WordSave = *WordToTest;

    // this routine cannot call any functions until memory word is restored!
    // The functions might live in RAM being tested

    *WordToTest = 0x00000000; if(*WordToTest != 0x00000000) result = FALSE;
    *WordToTest = 0xFFFFFFFF; if(*WordToTest != 0xFFFFFFFF) result = FALSE;
    *WordToTest = 0xAAAAAAAA; if(*WordToTest != 0xAAAAAAAA) result = FALSE;
    *WordToTest = 0x55555555; if(*WordToTest != 0x55555555) result = FALSE;
    *WordToTest = 0xAA55AA55; if(*WordToTest != 0xAA55AA55) result = FALSE;
    *WordToTest = 0x55AA55AA; if(*WordToTest != 0x55AA55AA) result = FALSE;

    *WordToTest = WordSave;
#endif

    return result;
}


void RamTest( UINT32 start, UINT32 size, const char* name )
{
    // Don't run the RamTest if the code is in RAM.
    if(start <= (UINT32)&RamWordTest && (start+size) > (UINT32)&RamWordTest) return;

    volatile UINT32* WordToTest = (volatile UINT32 *)start;

    for(int i = 0; i < size; i+=4)
    {
        // to slow to print every byte to LCD, only do 1024 byte steps
#if !defined(BUILD_RTM)
        if(0 == (i & 0x000003FF)) lcd_printf("%s:%3d K\r", name, i/1024);
#endif  // !defined(BUILD_RTM)

        if(!RamWordTest( WordToTest ))
        {
            hal_fprintf( STREAM_LCD, "\nRAM FAILURE:\r\n%08x\r\n", (UINT32)WordToTest);
            CPU_Halt();
        }

        ++WordToTest;
    }

    lcd_printf("%s:%3d K OK\r\n", name, size/1024);
}

