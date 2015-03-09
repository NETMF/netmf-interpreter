////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for MCBSTM32E board (STM32): Copyright (c) Oberon microsystems, Inc.
//
//  *** Block Storage AddDevice Configuration ***
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>


extern struct BlockStorageDevice  g_STM32_BS;
extern struct IBlockStorageDevice g_STM32_Flash_DeviceTable;
extern struct BLOCK_CONFIG        g_STM32_BS_Config;

extern struct BlockStorageDevice  g_M25P64_BS;
extern struct IBlockStorageDevice g_M25P64_Flash_DeviceTable;
extern struct BLOCK_CONFIG        g_M25P64_BS_Config;


void BlockStorage_AddDevices() {
    BlockStorageList::AddDevice( &g_STM32_BS,
                                 &g_STM32_Flash_DeviceTable,
                                 &g_STM32_BS_Config, FALSE );
    BlockStorageList::AddDevice( &g_M25P64_BS,
                                 &g_M25P64_Flash_DeviceTable,
                                 &g_M25P64_BS_Config, FALSE );
}

