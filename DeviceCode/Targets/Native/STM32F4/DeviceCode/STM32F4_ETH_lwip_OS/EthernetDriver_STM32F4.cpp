////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Open Technologies. All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// Leverages STMF32F4 port from Oberon + CSA Engineering

#include <tinyhal.h>
#include <STM32F4_ETH_lwip_adapter.h>

BOOL Network_Interface_Bind(int index)
{
    return STM32F4_ETH_LWIP_Driver::Bind();
}

int  Network_Interface_Open(int index)
{
    return STM32F4_ETH_LWIP_Driver::Open(index);
}

BOOL Network_Interface_Close(int index)
{
    return STM32F4_ETH_LWIP_Driver::Close();
}
