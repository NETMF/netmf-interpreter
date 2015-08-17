////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Core.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

#include "StringTableData.cpp"

////////////////////////////////////////////////////////////////////////////////////////////////////

#if defined(TINYCLR_CREATE_STRINGTABLE)
#if !defined(BIG_ENDIAN)
void CLR_RT_Assembly::InitString()
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_STRING* dst    = c_CLR_StringTable_Lookup;
    LPCSTR      src    = c_CLR_StringTable_Data;
    size_t      len    = c_CLR_StringTable_Size;
    size_t      offset = 0;

    while(len-- > 0)
    {
        size_t strLen = hal_strlen_s( src ) + 1;

        *dst++ = (CLR_STRING)offset;

        offset += strLen;
        src    += strLen;
    }
}
#else

void CLR_RT_Assembly::InitString()
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_STRING* dst    = c_CLR_StringTable_Lookup;
    LPCSTR      src    = c_CLR_StringTable_Data;
    size_t      len    = c_CLR_StringTable_Size;
    size_t      offset = 0;

    while(len-- > 0)
    {
        size_t strLen = hal_strlen_s( src ) + 1;

        *dst++ = (CLR_STRING)offset;

        offset += strLen;
        src    += strLen;
    }
}

#endif

#else

void CLR_RT_Assembly::InitString()
{
    NATIVE_PROFILE_CLR_CORE();
}

#endif

LPCSTR CLR_RT_Assembly::GetString( CLR_STRING i )
{
    NATIVE_PROFILE_CLR_CORE();
    static const CLR_STRING iMax = 0xFFFF - c_CLR_StringTable_Size;

    if(i >= iMax)
    {
        return &c_CLR_StringTable_Data[ c_CLR_StringTable_Lookup[ (CLR_STRING)0xFFFF - i ] ];
    }

    return &(((LPCSTR)GetTable( TBL_Strings ))[ i ]);
}

#if defined(PLATFORM_WINDOWS)

void CLR_RT_Assembly::InitString( std::map<std::string,CLR_OFFSET>& map )
{
    NATIVE_PROFILE_CLR_CORE();
    const CLR_STRING* array = c_CLR_StringTable_Lookup;
    size_t            len   = c_CLR_StringTable_Size;
    CLR_STRING        idx   = 0xFFFF;

    map.clear();

    while(len-- > 0)
    {
        map[ &c_CLR_StringTable_Data[ *array++ ] ] = idx--;
    }
}

#endif
