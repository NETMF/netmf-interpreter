////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

//--//

void Instrumentation_Initialize()
{
    NATIVE_PROFILE_PAL_IO();
}

void Instrumentation_Uninitialize()
{
    NATIVE_PROFILE_PAL_IO();
}

void Instrumentation_Running()
{
    NATIVE_PROFILE_PAL_IO();
}

void Instrumentation_Sleeping()
{
    NATIVE_PROFILE_PAL_IO();
}

//--//

BOOL OUTPUT_GPIO_Driver::Initialize( OUTPUT_GPIO_CONFIG* Config )
{
    NATIVE_PROFILE_PAL_IO();
    return TRUE;
}


void OUTPUT_GPIO_Driver::Uninitialize( OUTPUT_GPIO_CONFIG* Config  )
{
    NATIVE_PROFILE_PAL_IO();
}

void OUTPUT_GPIO_Driver::Set( OUTPUT_GPIO_CONFIG* Config, BOOL On )
{
    NATIVE_PROFILE_PAL_IO();
}


void OUTPUT_GPIO_Driver::RefCount( OUTPUT_GPIO_CONFIG* Config, BOOL Add )
{
    NATIVE_PROFILE_PAL_IO();
}
