////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#include "spot_io.h"


HRESULT Library_spot_io_native_Microsoft_SPOT_IO_NativeFindFile::_ctor___VOID__STRING__STRING( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_IO();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_String* hbPath;
    CLR_RT_HeapBlock_String* hbPattern;

    CLR_RT_HeapBlock* pThis =   stack.This();
    CLR_RT_HeapBlock* pArgs = &(stack.Arg1());

    hbPath    = pArgs[ 0 ].DereferenceString(); FAULT_ON_NULL(hbPath   );
    hbPattern = pArgs[ 1 ].DereferenceString(); FAULT_ON_NULL(hbPattern);
    
    TINYCLR_CHECK_HRESULT(CLR_RT_FindFile::CreateInstance( pThis[ FIELD__m_ff ], hbPath->StringText(), hbPattern->StringText() ));
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_io_native_Microsoft_SPOT_IO_NativeFindFile::GetNext___MicrosoftSPOTIONativeFileInfo( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_IO();
    TINYCLR_HEADER();

    CLR_RT_FindFile* ff;
    FS_FILEINFO*     fi;
    BOOL             found;
    
    CLR_RT_HeapBlock& top = stack.PushValueAndClear();

    TINYCLR_CHECK_HRESULT(GetFindFile( stack, ff ));

    TINYCLR_CHECK_HRESULT(ff->GetNext( &fi, &found ));

    if(found)
    {
        CLR_RT_HeapBlock* managedFi;

        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex( top, g_CLR_RT_WellKnownTypes.m_NativeFileInfo ));

        managedFi = top.Dereference();

        managedFi[ Library_spot_io_native_Microsoft_SPOT_IO_NativeFileInfo::FIELD__Attributes     ].SetInteger( (CLR_UINT32)fi->Attributes );
        managedFi[ Library_spot_io_native_Microsoft_SPOT_IO_NativeFileInfo::FIELD__CreationTime   ].SetInteger( (CLR_INT64)fi->CreationTime );
        managedFi[ Library_spot_io_native_Microsoft_SPOT_IO_NativeFileInfo::FIELD__LastAccessTime ].SetInteger( (CLR_INT64)fi->LastAccessTime );
        managedFi[ Library_spot_io_native_Microsoft_SPOT_IO_NativeFileInfo::FIELD__LastWriteTime  ].SetInteger( (CLR_INT64)fi->LastWriteTime );
        managedFi[ Library_spot_io_native_Microsoft_SPOT_IO_NativeFileInfo::FIELD__Size           ].SetInteger( (CLR_INT64)fi->Size );

        TINYCLR_CHECK_HRESULT(ff->CreateFilenameString( managedFi[ Library_spot_io_native_Microsoft_SPOT_IO_NativeFileInfo::FIELD__FileName ] ));
    }
    else
    {
        top.SetObjectReference( NULL );
    }

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_io_native_Microsoft_SPOT_IO_NativeFindFile::Close___VOID( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_IO();
    TINYCLR_HEADER();

    CLR_RT_FindFile* ff;

    TINYCLR_CHECK_HRESULT(GetFindFile( stack, ff ));

    TINYCLR_CHECK_HRESULT(ff->Close());

    TINYCLR_NOCLEANUP();
}


HRESULT Library_spot_io_native_Microsoft_SPOT_IO_NativeFindFile::GetFileInfo___STATIC__MicrosoftSPOTIONativeFileInfo__STRING( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_IO();
    TINYCLR_HEADER();

    FS_FILEINFO                  fileInfo;
    BOOL                         found;
    UnicodeString                pathW;
    FileSystemVolume*            driver;

    CLR_RT_HeapBlock&            top = stack.PushValueAndClear();

    TINYCLR_CHECK_HRESULT(Library_spot_io_native_Microsoft_SPOT_IO_NativeIO::FindVolume( stack.Arg0(), driver, pathW ));

    TINYCLR_CHECK_HRESULT(driver->GetFileInfo( pathW, &fileInfo, &found ));

    if(found)
    {
        CLR_RT_HeapBlock* managedFi;

        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex( top, g_CLR_RT_WellKnownTypes.m_NativeFileInfo ));

        managedFi = top.Dereference();

        managedFi[ Library_spot_io_native_Microsoft_SPOT_IO_NativeFileInfo::FIELD__Attributes     ].SetInteger( (CLR_UINT32)fileInfo.Attributes );
        managedFi[ Library_spot_io_native_Microsoft_SPOT_IO_NativeFileInfo::FIELD__CreationTime   ].SetInteger( (CLR_INT64)fileInfo.CreationTime );
        managedFi[ Library_spot_io_native_Microsoft_SPOT_IO_NativeFileInfo::FIELD__LastAccessTime ].SetInteger( (CLR_INT64)fileInfo.LastAccessTime );
        managedFi[ Library_spot_io_native_Microsoft_SPOT_IO_NativeFileInfo::FIELD__LastWriteTime  ].SetInteger( (CLR_INT64)fileInfo.LastWriteTime );
        managedFi[ Library_spot_io_native_Microsoft_SPOT_IO_NativeFileInfo::FIELD__Size           ].SetInteger( (CLR_INT64)fileInfo.Size );
        managedFi[ Library_spot_io_native_Microsoft_SPOT_IO_NativeFileInfo::FIELD__FileName       ].SetObjectReference( NULL );
    }
    else
    {
        top.SetObjectReference( NULL );
    }

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT Library_spot_io_native_Microsoft_SPOT_IO_NativeFindFile::GetFindFile( CLR_RT_StackFrame& stack, CLR_RT_FindFile*& ff )
{
    NATIVE_PROFILE_CLR_IO();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_BinaryBlob* blob;
    CLR_RT_HeapBlock* pThis = stack.This(); FAULT_ON_NULL(pThis);

    blob = pThis[ FIELD__m_ff ].DereferenceBinaryBlob(); 
    
    if(!blob || blob->DataType() != DATATYPE_BINARY_BLOB_HEAD) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);

    ff = (CLR_RT_FindFile*)blob->GetData();
    
    TINYCLR_NOCLEANUP();
}
