////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _DRIVERS_IO_DECL_H_
#define _DRIVERS_IO_DECL_H_ 1

//--//

struct OUTPUT_GPIO_CONFIG
{
    HAL_DRIVER_CONFIG_HEADER Header;

    //--//

    GPIO_FLAG Output_GPIO_PIN;
    INT32     RefCount;
    BOOL      On;
};

//--//

struct OUTPUT_GPIO_Driver
{
    static BOOL Initialize  ( OUTPUT_GPIO_CONFIG* Config           );
    static void Uninitialize( OUTPUT_GPIO_CONFIG* Config           );
    static void Set         ( OUTPUT_GPIO_CONFIG* Config, BOOL On  );
    static void RefCount    ( OUTPUT_GPIO_CONFIG* Config, BOOL Add );
};

//--//

#endif // _DRIVERS_IO_DECL_H_
