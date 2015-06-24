////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for STM32: Copyright (c) Oberon microsystems, Inc.
//
//  CORTEX-M3 Abort Handling 
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

//--//

#if !defined(BUILD_RTM)

void monitor_debug_printf( const char* format, ... )
{
    char buffer[256];
    va_list arg_ptr;

    va_start( arg_ptr, format );

    int len = hal_vsnprintf( buffer, sizeof(buffer)-1, format, arg_ptr );

    // flush existing characters
    USART_Flush( ConvertCOM_ComPort(DEBUG_TEXT_PORT) );

    // write string
    USART_Write( ConvertCOM_ComPort(DEBUG_TEXT_PORT), buffer, len );

    // flush new characters
    USART_Flush( ConvertCOM_ComPort(DEBUG_TEXT_PORT) );

    va_end( arg_ptr );
}

void FAULT_HandlerDisplay( UINT32 *registers, UINT32 exception )
{
    int i;

    USART_Initialize( ConvertCOM_ComPort(USART_DEFAULT_PORT), USART_DEFAULT_BAUDRATE, USART_PARITY_NONE, 8, USART_STOP_BITS_ONE, USART_FLOW_NONE );

    if( exception )
        monitor_debug_printf("EXCEPTION 0x%02x:\r\n", exception);
    else
        monitor_debug_printf("ERROR:\r\n");

    monitor_debug_printf("  cpsr=0x%08x\r\n", registers[15]);
    monitor_debug_printf("  pc  =0x%08x\r\n", registers[14]);
    monitor_debug_printf("  lr  =0x%08x\r\n", registers[13]);
    monitor_debug_printf("  sp  =0x%08x\r\n", registers + 16);
    
    for(i = 0; i <= 12; i++)
        monitor_debug_printf("  r%02d =0x%08x\r\n", i, registers[i]);
}

#endif  // !defined(BUILD_RTM)

extern "C"
{
    void FAULT_Handler( UINT32* registers, UINT32 exception )
    {    
        ASSERT_IRQ_MUST_BE_OFF();
    #if !defined(BUILD_RTM)
        FAULT_HandlerDisplay(registers, exception);
        CPU_Halt();
    #else
        CPU_Reset();
    #endif  // !defined(BUILD_RTM)
    }

    void HARD_Breakpoint_Handler(UINT32 *registers)
    {    
    #if !defined(BUILD_RTM)
        GLOBAL_LOCK(irq);
        FAULT_HandlerDisplay(registers, 0);
        CPU_Halt();
    #else
        CPU_Reset();
    #endif  // !defined(BUILD_RTM)
    }
}
