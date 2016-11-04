////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <stddef.h>
#include <tinyhal.h>

////////////////////////////////////////////////////////////////////////////////

static void RuntimeFault( LPCSTR szText )
{
    lcd_printf("\014ERROR:\r\n%s\r\n", szText );
    debug_printf( "ERROR: %s\r\n", szText );

    // let watchdog take over
    CPU_Halt();
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

// the following remapping seems to be required by the crypto libs, as soon as those are reworked this should be removed
// remap private_malloc to standard C malloc
void *private_malloc(size_t size)
{
    return  malloc(size); 
}

// remap private_free to standard C free
void private_free(void *ptr)
{
    free(ptr);
}
