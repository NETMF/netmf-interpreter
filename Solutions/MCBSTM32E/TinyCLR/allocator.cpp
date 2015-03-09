////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <stddef.h>
#include <tinyhal.h>

////////////////////////////////////////////////////////////////////////////////

static void RuntimeFault( const char* szText )
{
    lcd_printf("\014ERROR:\r\n%s\r\n", szText );
    debug_printf( "ERROR: %s\r\n", szText );

    HARD_BREAKPOINT();
}

void *operator new(size_t) 
{
    RuntimeFault( "new(size_t)" );

    return NULL;
}

void *operator new[](size_t)
{
    RuntimeFault( "new[](size_t)" );

    return NULL;
}

void operator delete (void*)
{
    RuntimeFault( "delete(void*)" );
}

void operator delete[] (void*)
{
    RuntimeFault( "delete[](void*)" );
}

////////////////////////////////////////////////////////////////////////////////
