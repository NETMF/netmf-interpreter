////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#include <stdio.h>
#include <string.h>
#include <ctype.h>

#include "tinyhal.h"

HAL_DECLARE_NULL_HEAP();

void ApplicationEntryPoint()
{
    int nSects = 0;
    const FLASH_SECTOR *sects;

    CPU_InitializeCommunication();
        
    Flash_EnumerateSectors( nSects, sects );

    //--//

    hal_printf( "Erasing deployment...........\r\n" );

    Flash_ChipReadOnly( FALSE );
    for(int i = 0; i < nSects; i++)
    {
        if((sects[ i ].Usage & MEMORY_USAGE_MASK) == MEMORY_USAGE_DEPLOYMENT)
        {
            hal_printf( "Erasing sector %d (0x%08x)\r\n", i, (UINT32)sects[ i ].Start );
            if(!Flash_IsSectorErased( &sects[ i ] ))
            {
                Flash_EraseSector( &sects[ i ] );
            }
        }
    } 
    Flash_ChipReadOnly( TRUE );

    hal_printf( "Erasing deployment finished!\r\n" );

    // allowing extra wait so that we can see the print statement!    
    Events_WaitForEvents( 0, (UINT32)10000 );
    CPU_Reset();
}
