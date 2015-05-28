////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

#if !defined(BUILD_RTM) && defined( ABORT_HANDLER_DISPLAY )

void monitor_debug_printf( const char* format, ... )
{
    char    buffer[256];
    va_list arg_ptr;

    va_start( arg_ptr, format );

    int len = hal_vsnprintf( buffer, sizeof(buffer)-1, format, arg_ptr );

    // flush existing characters
    USART_Flush( ConvertCOM_ComPort(USART_DEFAULT_PORT) );

    // write string
    USART_Write( ConvertCOM_ComPort(USART_DEFAULT_PORT), buffer, len );

    // flush new characters
    USART_Flush( ConvertCOM_ComPort(USART_DEFAULT_PORT) );

    va_end( arg_ptr );
}

#define RDUMP8(r)  monitor_debug_printf("%-60s0x%02x\r\n"  , #r, (UINT8 )r)
#define RDUMP16(r) monitor_debug_printf("%-60s0x%04x\r\n"  , #r, (UINT16)r)
#define RDUMP32(r) monitor_debug_printf("%-60s0x%08x\r\n"  , #r, (UINT32)r)
#define RDUMP64(r) monitor_debug_printf("%-60s0x%16llx\r\n", #r, (UINT64)r)

#undef RDUMP8
#undef RDUMP16
#undef RDUMP32
#undef RDUMP64


void RegisterDump()
{
}

void CharReady()
{
}

BOOL AbortHandler_CheckMemoryRange( UINT8* start, UINT8* end )
{
#define INRANGE(start,end,mem) ((UINT32)(start) >= HalSystemConfig.mem.Base && (UINT32)(end) <= (HalSystemConfig.mem.Base+HalSystemConfig.mem.Size))

    if(INRANGE(start,end,RAM1 )) return TRUE;
    if(INRANGE(start,end,FLASH)) return TRUE;

#undef INRANGE

    return FALSE;
}


UINT32 ABORT_recursion_counter = 0;


void ABORT_HandlerDisplay(
    UINT32 *registers_const,
    UINT32 sp,
    UINT32 lr,
    UINT32 pc_offset,
    const char *Title,
    BOOL SerialOutput
    )
{
    UINT32              cpsr;
    UINT32              pc;
    int                 i;
    int                 j;
#if !defined(PLATFORM_ARM_OS_PORT)
    UINT8*              stackbytes;
    const UINT32* const stackwords = (UINT32 *)sp;
#endif
    int                 page;
    UINT32*             registers;

    ++ABORT_recursion_counter;

dump_again:

    page = 0;
    registers = registers_const;

    cpsr =  *registers++;
    pc   = (*registers++) - pc_offset;

    if(SerialOutput)
    {
        // unprotect the GPIO pins for USART, allowing USART output

        USART_Initialize( ConvertCOM_ComPort(USART_DEFAULT_PORT), USART_DEFAULT_BAUDRATE, USART_PARITY_NONE, 8, USART_STOP_BITS_ONE, USART_FLOW_NONE );

        monitor_debug_printf("ERROR: %s\r\n", Title);
        monitor_debug_printf("  cpsr=0x%08x\r\n", cpsr);
        monitor_debug_printf("  pc  =0x%08x\r\n", pc);
        monitor_debug_printf("  lr  =0x%08x\r\n", lr);

        for(i = 0; i <= 12; i++)
        {
            monitor_debug_printf("  r%d=0x%08x\r\n", i, registers[i]);
        }

        monitor_debug_printf("  sp  =0x%08x\r\n", sp);

        RegisterDump();
    }

    // don't let the screen go away if the watchdog was enabled.
    Watchdog_Disable();

    //Flash_ChipReadOnly(TRUE);

    // first dump stuff to the com port, then go interactive with the buttons and LCD

    if(SerialOutput)
    {
        monitor_debug_printf("ERROR: %s\r\n", Title);
        monitor_debug_printf("  cpsr=0x%08x\r\n", cpsr);
        monitor_debug_printf("  pc  =0x%08x\r\n", pc);
        monitor_debug_printf("  lr  =0x%08x\r\n", lr);
        for(i = 0; i <= 12; i++)
        {
            monitor_debug_printf("  r%d=0x%08x\r\n", i, registers[i]);
        }
        monitor_debug_printf("  sp  =0x%08x\r\n", sp);

#if !defined(PLATFORM_ARM_OS_PORT)
        stackbytes = (UINT8 *)sp;
        for(i = 0; i < 16; i++)
        {
            monitor_debug_printf("[0x%08x] :", (UINT32)&stackbytes[i*16]);
            for(j = 0; j < 16; j++)
            {
                // don't cause a data abort here!
                if((UINT32) &stackbytes[i*16 + j] < (UINT32) &StackTop)
                {
                    monitor_debug_printf(" %02x", stackbytes[i*16 + j]);
                }
                else
                {
                    monitor_debug_printf("   ");
                }
            }
            monitor_debug_printf(" ");
            for(j = 0; j < 16; j++)
            {
                if((UINT32) &stackbytes[i*16 + j] < (UINT32) &StackTop)
                {
                    monitor_debug_printf("%c", (stackbytes[i*16 + j] >= ' ') ? stackbytes[i*16 + j] : '.');
                }
            }
            monitor_debug_printf("\r\n");
        }
#endif
    }

    // make sure USART is ready to go (clock enabled, pins set active) regardless of charger state, idem potent call if it was already on
    CPU_ProtectCommunicationGPIOs( FALSE );

    // go interactive with the buttons and LCD
    while(true)
    {
        UINT32 CurrentButtonsState;

        lcd_printf("\f");

        switch(page)
        {
        case 0:
            lcd_printf("%-12s \r\n\r\n", Title);
            lcd_printf("Build Date:\r\n");
            lcd_printf("%-12s \r\n"    , __DATE__);
            lcd_printf("%-12s \r\n"    , __TIME__);
            lcd_printf("\r\n");
            lcd_printf("cps=0x%08x\r\n", cpsr);
            lcd_printf("pc =0x%08x\r\n", pc);
            lcd_printf("lr =0x%08x\r\n", lr);
            lcd_printf("sp =0x%08x\r\n", sp);
            break;

        case 1:
            lcd_printf("Registers 1\r\n\r\n");
            for(i = 0; i < 10; i++)
            {
                lcd_printf("r%1d =0x%08x\r\n", i, registers[i]);
            }
            break;

        case 2:
            lcd_printf("Registers 2\r\n\r\n");
            for(i = 10; i < 13; i++)
            {
                lcd_printf("r%2d=0x%08x\r\n", i, registers[i]);
            }
            break;

        case 3:
        default:
            {
#if !defined(PLATFORM_ARM_OS_PORT)
                UINT32 index =  (page-3)*10;

                lcd_printf("Stack %08x\r\n\r\n", (UINT32)&stackwords[index]);

                for(i = 0; i < 10; i++)
                {
                    stackbytes = (UINT8 *)&stackwords[index+i];

                    // don't cause a data abort displaying past the end of the stack!
                    if((UINT32) &stackwords[index+i] < (UINT32) &StackTop)
                    {
                        lcd_printf("%08x %c%c%c%c\r\n", stackwords[index+i],
                            (stackbytes[0] >= ' ') ? stackbytes[0] : '.',
                            (stackbytes[1] >= ' ') ? stackbytes[1] : '.',
                            (stackbytes[2] >= ' ') ? stackbytes[2] : '.',
                            (stackbytes[3] >= ' ') ? stackbytes[3] : '.'
                            );
                    }
                    else
                    {
                        lcd_printf("             \r\n");
                    }
                }
#endif
            }
            break;
        }


        // wait for a button press
        while(0 == (CurrentButtonsState = Buttons_CurrentHWState()))
        {
        }

        // wait for 10 mSec
        HAL_Time_Sleep_MicroSeconds(10000/64);
        
        // wait for it to release
        while(0 == (CurrentButtonsState ^ Buttons_CurrentHWState()));
        
        // wait for 10 mSec        
        HAL_Time_Sleep_MicroSeconds(10000/64);
        
        if(CurrentButtonsState & BUTTON_B5)
        {
            page = (page + 1);  // no limit of stack pages
            LCD_Clear();
        }

        if(CurrentButtonsState & BUTTON_B2)
        {
            page = (page > 0) ? page - 1 : 0;
            LCD_Clear();
        }

        if((CurrentButtonsState & BUTTON_B4) || (CurrentButtonsState & BUTTON_B0))
        {
            goto dump_again;
        }

        continue;


        {
            char   data[16];
            UINT32 pos = 0;

            LCD_Clear();
            lcd_printf("\fMONITOR MODE\r\n");

            HAL_Time_Sleep_MicroSeconds(10000);

            while(1)
            {
                    switch(data[0])
                    {        
                    case 'L':
                        monitor_debug_printf("rb=0x%08x\r\n", HalSystemConfig.RAM1.Base  );
                        monitor_debug_printf("rs=0x%08x\r\n", HalSystemConfig.RAM1.Size  );
                        monitor_debug_printf("fb=0x%08x\r\n", HalSystemConfig.FLASH.Base);
                        monitor_debug_printf("fs=0x%08x\r\n", HalSystemConfig.FLASH.Size);
                        break;
                        
                    case 'M':
                        if(pos < 12) continue;
                        
                        {
                            UINT8* start = *(UINT8**)&data[4];
                            UINT8* end   = *(UINT8**)&data[8];

                            lcd_printf( "\f\r\n\r\nREAD:\r\n  %08x\r\n  %08x", (UINT32)start, (UINT32)end );

                            if(AbortHandler_CheckMemoryRange( start, end ))
                            {   
                                while(start < end)
                                {
                                    monitor_debug_printf("[0x%08x]", (UINT32)start);
                                    for(j = 0; j < 128; j++)
                                    {
                                        monitor_debug_printf("%02x", *start++);

                                        if(start == end) break;
                                    }
                                    monitor_debug_printf("\r\n");
                                }
                            }
                            else
                            {
                                monitor_debug_printf("ERROR: invalid range: %08x-%08x\r\n", (UINT32)start, (UINT32)end );
                            }
                        }
                        break;

                    case 'R':
                        for(i = 0; i <= 12; i++)
                        {
                            monitor_debug_printf("r%d=%08x\r\n", i, registers[i]);
                        }

                        monitor_debug_printf("sp=%08x\r\n", sp);
                        monitor_debug_printf("lr=%08x\r\n", lr);
                        monitor_debug_printf("pc=%08x\r\n", pc);
                        monitor_debug_printf("cpsr=%08x\r\n", cpsr);
                        break;

                    case '\r':
                    case '\n':
                    case ' ':
                        break;

                    default:
                        monitor_debug_printf("ERROR: unknown command %c\r\n", data[0] );
                        break;
                    }

                    pos = 0;
                }
                else if(Buttons_CurrentHWState())
                {
                    // wait for it to release
                    while (Buttons_CurrentHWState());

                    LCD_Clear();
                    break;
                }
            }
        }
    }
}

#endif  // !defined(BUILD_RTM)

extern "C"
{
    
void UNDEF_Handler( UINT32* registers, UINT32 sp, UINT32 lr )
{    
    ASSERT_IRQ_MUST_BE_OFF();
    
#if !defined(BUILD_RTM) && defined( ABORT_HANDLER_DISPLAY )
    Verify_RAMConstants((void *) FALSE);

    ABORT_HandlerDisplay(registers, sp, lr, 4, "Undef Instr", TRUE);

    CPU_Halt();
#else
    CPU_Reset();
#endif  // !defined(BUILD_RTM)
}


void ABORTP_Handler( UINT32* registers, UINT32 sp, UINT32 lr )
{
    ASSERT_IRQ_MUST_BE_OFF();

#if !defined(BUILD_RTM) && defined( ABORT_HANDLER_DISPLAY )
    Verify_RAMConstants((void *) FALSE);

    ABORT_HandlerDisplay(registers, sp, lr, 4, "ABORT Prefetch", TRUE);

    CPU_Halt();
#else
    CPU_Reset();
#endif  // !defined(BUILD_RTM)
}


void ABORTD_Handler( UINT32* registers, UINT32 sp, UINT32 lr )
{
    ASSERT_IRQ_MUST_BE_OFF();

#if !defined(BUILD_RTM) && defined( ABORT_HANDLER_DISPLAY )
    Verify_RAMConstants((void *) FALSE);

    ABORT_HandlerDisplay(registers, sp, lr, 8, "ABORT Data", TRUE);

    CPU_Halt();
#else
    CPU_Reset();
#endif  // !defined(BUILD_RTM)
}


void
HARD_Breakpoint_Handler(
    UINT32 *registers,
    UINT32 sp,
    UINT32 lr
    )
{    
#if !defined(BUILD_RTM) && defined( ABORT_HANDLER_DISPLAY )

    if(1 == ++ABORT_recursion_counter)
    {
        GLOBAL_LOCK(irq);

        //Verify_RAMConstants((void *) FALSE);

        ABORT_HandlerDisplay(registers, sp, lr, 4, "", TRUE);

        CPU_Halt();
    }

#else

    CPU_Reset();

#endif  // !defined(BUILD_RTM)
}

#if !defined(BUILD_RTM) && defined( ABORT_HANDLER_DISPLAY )

void NULL_Pointer_Write()
{
    GLOBAL_LOCK(irq);

    UINT32* ResetVector = 0;

    lcd_printf("\fERROR:\r\nNULL Pointer write\r\nRESET vector modified to 0x%08x\r\n", *ResetVector);

    monitor_debug_printf("ERROR: vector area modified\r\n");

    // dump many words
    for(int i = 0; i < 0x100; i++)
    {
        monitor_debug_printf("0x%08x=0x%08x\r\n", (UINT32) ResetVector, *ResetVector);
    }

    CPU_Halt();
}

#endif  // !defined(BUILD_RTM)


void StackOverflow( UINT32 sp )
{
    ASSERT_IRQ_MUST_BE_OFF();

#if !defined(BUILD_RTM) && !defined(PLATFORM_ARM_OS_PORT) && defined( ABORT_HANDLER_DISPLAY )
    lcd_printf("\fSTACK OVERFLOW\r\nsp=0x%08x\r\nStackBottom:\r\n0x%08x", sp, (UINT32)&StackBottom);

    CPU_Halt();
#else
    CPU_Reset();
#endif  // !defined(BUILD_RTM)
}

}

void HAL_Assert( LPCSTR Func, int Line, LPCSTR File )
{
#if !defined(BUILD_RTM) && defined( ABORT_HANDLER_DISPLAY )
    lcd_printf( "\r\nAssert in\r\n%s\r\nline:%d\r\nfile:%s\r\n", Func, Line, File );
    debug_printf( "Assert in %s line %d of file %s\r\n"        , Func, Line, File );
#endif
    CPU_Halt();
}

