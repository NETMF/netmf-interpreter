////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "tinyhal.h"
#include "FAT_FS.h"
#include "FAT_FS_Utility.h"
#include "TinyCLR_Interop.h"


//--------------------------------------------------------
//-----------------wchar string operation utility--------------------
//--------------------------------------------------------

LPCWSTR FAT_Utility::FindCharReverse( LPCWSTR str, UINT32 strLen, WCHAR c, UINT32* retLen ) 
{
    for(int i = strLen - 1; i >= 0; i--)
    {
        if(str[i] == c)
        {
            *retLen = strLen - i;
            return str + i;
        }
    }

    *retLen = 0;
    return str + strLen;
}

LPCWSTR FAT_Utility::FindChar( LPCWSTR str, UINT32 strLen, WCHAR c, UINT32* retLen ) 
{
    if(!str) return NULL;

    for(; strLen > 0; strLen--, str++)
    {
        if(*str == c)
        {
            *retLen = strLen;
            return str;
        }
    }

    *retLen = 0;
    return str;
}

HRESULT FAT_Utility::ValidatePathLength( LPCWSTR path, UINT32* pathLen )
{
    TINYCLR_HEADER();
    
    *pathLen = MF_wcslen( path );

    if(*pathLen >= FS_MAX_PATH_LENGTH)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_PATH_TOO_LONG);
    }

    TINYCLR_NOCLEANUP();
}

INT64 FAT_Utility::FATTimeToTicks( UINT16 date, UINT16 time, UINT8 timeTenth )
{
    SYSTEMTIME st;
    INT64      ticks;

    st.wDay    =  (date >> FAT_TIME_DAY_SHIFT   ) & FAT_TIME_DAY_MASK   ;
    st.wMonth  =  (date >> FAT_TIME_MONTH_SHIFT ) & FAT_TIME_MONTH_MASK ;
    st.wYear   = ((date >> FAT_TIME_YEAR_SHIFT  ) & FAT_TIME_YEAR_MASK  ) + FAT_TIME_YEAR_OFFSET;

    st.wHour   =  (time >> FAT_TIME_HOUR_SHIFT  ) & FAT_TIME_HOUR_MASK  ;
    st.wMinute =  (time >> FAT_TIME_MINUTE_SHIFT) & FAT_TIME_MINUTE_MASK;
    st.wSecond = ((time >> FAT_TIME_SECOND_SHIFT) & FAT_TIME_SECOND_MASK) * FAT_TIME_SECOND_MULTIPLIER + (timeTenth / 100);

    st.wMilliseconds = (timeTenth % 100) * 10;

    ticks = Time_FromSystemTime( &st );

    return ticks;
}

void FAT_Utility::GetCurrentFATTime( UINT16* date, UINT16* time, UINT8* timeTenth )
{
    SYSTEMTIME st;

    Time_ToSystemTime( Time_GetLocalTime(), &st );

    if(date)
    {
        *date = (st.wYear   - FAT_TIME_YEAR_OFFSET      ) << FAT_TIME_YEAR_SHIFT   | 
                (st.wMonth                              ) << FAT_TIME_MONTH_SHIFT  | 
                (st.wDay                                ) << FAT_TIME_DAY_SHIFT    ;
    }
    
    if(time)
    {
        INT16 sec = FAT_TIME_SECOND_CALC_GRANULARITY(st.wSecond);
        
        *time = (st.wHour                        ) << FAT_TIME_HOUR_SHIFT   |
                (st.wMinute                      ) << FAT_TIME_MINUTE_SHIFT |
                (sec / FAT_TIME_SECOND_MULTIPLIER) << FAT_TIME_SECOND_SHIFT ;
    }

    if(timeTenth)
    {
        *timeTenth = (UINT8)((st.wSecond % FAT_TIME_SECOND_MULTIPLIER) * 100 + (st.wMilliseconds / 10));
    }
}

/////////////////////////////////////////////////////////
// Description:
//	 wchar version of  strlen()
// 
// Input:
//	 wchar string
//
// Remarks:
// 
// Returns:  wchar count in string
//	
size_t MF_wcslen(LPCWSTR str)
{
    if(str == NULL)
        return 0;

    size_t  size=0;

    while(str[size++] != 0)
        ;
    return size-1;
}

/////////////////////////////////////////////////////////
// Description:
//	transform one wchar to upper case
// 
// Input:
//	 one wchar to transform
//
// Remarks:
// 
// Returns:
//	upper-case wchar
WCHAR MF_towupper( WCHAR c )
{
    if(c >= 'a' && c <= 'z')
    {
        c -= 32;
    }
    return c;
}

//--//



