////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for STM32F4: Copyright (c) Oberon microsystems, Inc.
//
//  *** Security Driver ***
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

void SecurityKey_Copy( UINT8 KeyCopy[], INT32 BytesToCopy )
{
    NATIVE_PROFILE_HAL_PROCESSOR_SECURITY();
}

void SecurityKey_LowLevelCopy( UINT8 KeyCopy[], INT32 BytesToCopy )
{
    NATIVE_PROFILE_HAL_PROCESSOR_SECURITY();
    for(int i= 0; i<BytesToCopy; i++) KeyCopy[i]=1;
}

void SecurityKey_Print()
{
    NATIVE_PROFILE_HAL_PROCESSOR_SECURITY();
}


