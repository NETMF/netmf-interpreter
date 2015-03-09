////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Core.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

CLR_RT_HeapBlock_String* CLR_RT_HeapBlock_String::CreateInstance( CLR_RT_HeapBlock& reference, CLR_UINT32 length )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_UINT32               totLength = sizeof(CLR_RT_HeapBlock_String) + length + 1;
    CLR_RT_HeapBlock_String* str;

    reference.SetObjectReference( NULL );

    str = (CLR_RT_HeapBlock_String*)g_CLR_RT_ExecutionEngine.ExtractHeapBytesForObjects( DATATYPE_STRING, 0, totLength );
    if(str)
    {
        LPSTR szText = (LPSTR)&str[ 1 ]; szText[ 0 ] = 0;

#if defined(TINYCLR_NO_ASSEMBLY_STRINGS)
        str->SetStringText( szText );
#else
        str->SetStringText( szText, NULL );
#endif     

        reference.SetObjectReference( str );
    }

    return str;
}

HRESULT CLR_RT_HeapBlock_String::CreateInstance( CLR_RT_HeapBlock& reference, LPCSTR szText )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    if(!szText) szText = "";

    TINYCLR_SET_AND_LEAVE(CLR_RT_HeapBlock_String::CreateInstance( reference, szText, (CLR_UINT32)hal_strlen_s( szText ) ));

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock_String::CreateInstance( CLR_RT_HeapBlock& reference, LPCSTR szText, CLR_UINT32 length )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_String* str;
    LPSTR                    szTextDst;

    str = CreateInstance( reference, length ); CHECK_ALLOCATION(str);

    szTextDst = (LPSTR)str->StringText();

    memcpy( szTextDst, szText, length ); szTextDst[ length ] = 0;

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock_String::CreateInstance( CLR_RT_HeapBlock& reference, LPCSTR szText, CLR_RT_Assembly* assm )
{        
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_String* str;

    reference.SetObjectReference( NULL );

    str = (CLR_RT_HeapBlock_String*)g_CLR_RT_ExecutionEngine.ExtractHeapBytesForObjects( DATATYPE_STRING, 0, sizeof(CLR_RT_HeapBlock_String) ); CHECK_ALLOCATION(str);

    reference.SetObjectReference( str );

#if defined(TINYCLR_NO_ASSEMBLY_STRINGS)            
    TINYCLR_CHECK_HRESULT( CLR_RT_HeapBlock_String::CreateInstance( reference, assm->GetString( CLR_DataFromTk( token ) ) ));    
#else
    str->SetStringText( szText, assm );
#endif

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock_String::CreateInstance( CLR_RT_HeapBlock& reference, CLR_UINT32 token, CLR_RT_Assembly* assm )
{
    NATIVE_PROFILE_CLR_CORE();
    return CreateInstance( reference, assm->GetString( CLR_DataFromTk( token ) ), assm );
}

HRESULT CLR_RT_HeapBlock_String::CreateInstance( CLR_RT_HeapBlock& reference, CLR_UINT16* szText, CLR_UINT32 length )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_UnicodeHelper     uh;             uh.SetInputUTF16     ( szText );
    CLR_UINT32               lengthInBytes = uh.CountNumberOfBytes( length );
    CLR_RT_HeapBlock_String* str           = CreateInstance( reference, lengthInBytes ); CHECK_ALLOCATION(str);
    
    uh.m_outputUTF8      = (CLR_UINT8*)str->StringText();
    uh.m_outputUTF8_size = lengthInBytes + 1;

    uh.ConvertToUTF8( length, false );

    TINYCLR_NOCLEANUP();
}

CLR_RT_HeapBlock_String* CLR_RT_HeapBlock_String::GetStringEmpty()
{
    return g_CLR_RT_TypeSystem.m_assemblyMscorlib->GetStaticField( Library_corlib_native_System_String::FIELD_STATIC__Empty )->DereferenceString(); 
}
