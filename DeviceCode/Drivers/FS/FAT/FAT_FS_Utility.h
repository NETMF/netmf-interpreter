////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef FAT_FS_UTILITY_H
#define FAT_FS_UTILITY_H  

#include "FS_decl.h"

#define FAT_TIME_DAY_MASK            0x1F
#define FAT_TIME_DAY_SHIFT           0
#define FAT_TIME_MONTH_MASK          0x0F
#define FAT_TIME_MONTH_SHIFT         5
#define FAT_TIME_YEAR_MASK           0x7F
#define FAT_TIME_YEAR_SHIFT          9
#define FAT_TIME_YEAR_OFFSET         1980
#define FAT_TIME_HOUR_MASK           0x1F
#define FAT_TIME_HOUR_SHIFT          11
#define FAT_TIME_MINUTE_MASK         0x3F
#define FAT_TIME_MINUTE_SHIFT        5
#define FAT_TIME_SECOND_MASK         0x1F
#define FAT_TIME_SECOND_SHIFT        0
#define FAT_TIME_SECOND_MULTIPLIER   2 

#ifndef FAT_TIME_SECOND_GRANULARITY
#define FAT_TIME_SECOND_GRANULARITY  1 // only update every 8 seconds (This needs be multiple of two because of macro below)
#endif //FAT_TIME_SECOND_GRANULARITY

#define FAT_TIME_SECOND_CALC_GRANULARITY(x) ((x) & ~(FAT_TIME_SECOND_GRANULARITY-1))

struct FAT_Utility
{
    static LPCWSTR FindCharReverse( LPCWSTR str, UINT32 strLen, WCHAR c, UINT32* retLen );
    static LPCWSTR FindChar       ( LPCWSTR str, UINT32 strLen, WCHAR c, UINT32* retLen );
    
    static HRESULT ValidatePathLength( LPCWSTR path, UINT32* pathLen );

    static INT64 FATTimeToTicks( UINT16 date, UINT16 time, UINT8 timeTenth );

    static void GetCurrentFATTime( UINT16* date, UINT16* time, UINT8* timeTenth );
};

//--//

size_t MF_wcslen(LPCWSTR str);
WCHAR MF_towupper(WCHAR c);

//--//


#endif
