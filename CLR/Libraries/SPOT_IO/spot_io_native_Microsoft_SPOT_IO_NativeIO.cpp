////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#include "spot_io.h"


//--//

HRESULT Library_spot_io_native_Microsoft_SPOT_IO_NativeIO::Format___STATIC__VOID__STRING__STRING__STRING__U4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_IO();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_String* hbFileSystemName;

    FileSystemVolume*            volume;
    LPCSTR                       fileSystemName;
    LPCSTR                       volumeLabel;
    UINT32                       i;
    UINT32                       parameters;
    FILESYSTEM_DRIVER_INTERFACE* originalFS = NULL;
    STREAM_DRIVER_INTERFACE*     originalStream = NULL;
    BOOL                         needInitialize = FALSE;
                    
    TINYCLR_CHECK_HRESULT(FindVolume( stack.Arg0(), volume ));

    hbFileSystemName = stack.Arg1().DereferenceString();
    volumeLabel      = stack.Arg2().RecoverString();
    parameters       = stack.Arg3().NumericByRef().u4;

    originalFS       = volume->m_fsDriver;
    originalStream   = volume->m_streamDriver;

    if(!hbFileSystemName)
    {
        needInitialize = TRUE;

        TINYCLR_SET_AND_LEAVE(volume->Format( volumeLabel, parameters ));
    }

    fileSystemName = hbFileSystemName->StringText();

    for (i = 0; i < g_InstalledFSCount; i++)
    {
        if(hal_strncmp_s( g_AvailableFSInterfaces[ i ].fsDriver->Name, fileSystemName, FS_NAME_DEFAULT_LENGTH ) == 0)
        {
            if(originalFS)
            {
                volume->UninitializeVolume();
            }

            // From this point on, even if we fail, we should try to re-initialize before returning
            needInitialize = TRUE;
            
            volume->m_fsDriver     = g_AvailableFSInterfaces[ i ].fsDriver;
            volume->m_streamDriver = g_AvailableFSInterfaces[ i ].streamDriver;

            TINYCLR_SET_AND_LEAVE(volume->Format( volumeLabel, parameters ));
        }
    }

    TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);

    TINYCLR_CLEANUP();

    if (needInitialize)
    {
        if(FAILED(hr)) // format did't succeed, restore the original drivers
        {
            volume->m_fsDriver     = originalFS;
            volume->m_streamDriver = originalStream;
        }

        if(!(volume->InitializeVolume()))
        {
            if (SUCCEEDED(hr)) // if we were successful up to this point, fail, otherwise leave the original hresult
            {
                hr = CLR_E_FILE_IO;
            }
        }
        else
        {
            volume->m_fsDriver->GetVolumeLabel(&volume->m_volumeId, volume->m_label, ARRAYSIZE(volume->m_label));
        }
    }

    TINYCLR_CLEANUP_END();
}

HRESULT Library_spot_io_native_Microsoft_SPOT_IO_NativeIO::Delete___STATIC__VOID__STRING( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_IO();
    TINYCLR_HEADER();

    FileSystemVolume*            driver;
    UnicodeString                pathW;

    TINYCLR_CHECK_HRESULT(FindVolume( stack.Arg0(), driver, pathW ));

    TINYCLR_CHECK_HRESULT(driver->Delete( pathW ));        

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_io_native_Microsoft_SPOT_IO_NativeIO::Move___STATIC__BOOLEAN__STRING__STRING( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_IO();
    TINYCLR_HEADER();

    UnicodeString                path1W;
    UnicodeString                path2W;
    FileSystemVolume*            driver1;
    FileSystemVolume*            driver2;

    TINYCLR_CHECK_HRESULT(FindVolume( stack.Arg0(), driver1, path1W ));
    TINYCLR_CHECK_HRESULT(FindVolume( stack.Arg1(), driver2, path2W ));

    // Check if the two namespaces are the same
    if(driver1 != driver2)
    {
        // Need cross-volume move, so return false and let managed code deal with it.
        stack.SetResult_Boolean( false );
    }
    else
    {
        TINYCLR_CHECK_HRESULT(driver1->Move( path1W, path2W ));

        stack.SetResult_Boolean( true );
    }

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_io_native_Microsoft_SPOT_IO_NativeIO::CreateDirectory___STATIC__VOID__STRING(CLR_RT_StackFrame& stack)
{
    NATIVE_PROFILE_CLR_IO();
    TINYCLR_HEADER();

    UnicodeString                pathW;
    FileSystemVolume*            driver;

    TINYCLR_CHECK_HRESULT(FindVolume( stack.Arg0(), driver, pathW ));

    if(pathW.Length() >= FS_MAX_DIRECTORY_LENGTH)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_PATH_TOO_LONG);
    }

    TINYCLR_CHECK_HRESULT(driver->CreateDirectory( pathW ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_io_native_Microsoft_SPOT_IO_NativeIO::GetAttributes___STATIC__U4__STRING(CLR_RT_StackFrame& stack)
{
    NATIVE_PROFILE_CLR_IO();
    TINYCLR_HEADER();

    UINT32                       attributes;
    UnicodeString                pathW;
    FileSystemVolume*            driver;

    TINYCLR_CHECK_HRESULT(FindVolume( stack.Arg0(), driver, pathW ));

    TINYCLR_CHECK_HRESULT(driver->GetAttributes( pathW, &attributes ));        

    stack.SetResult_U4( attributes );        

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_io_native_Microsoft_SPOT_IO_NativeIO::SetAttributes___STATIC__VOID__STRING__U4(CLR_RT_StackFrame& stack)
{
    NATIVE_PROFILE_CLR_IO();
    TINYCLR_HEADER();

    UINT32                       attributes;
    UnicodeString                pathW;
    FileSystemVolume*            driver;

    TINYCLR_CHECK_HRESULT(FindVolume( stack.Arg0(), driver, pathW ));

    attributes = stack.Arg1().NumericByRef().u4;

    TINYCLR_CHECK_HRESULT(driver->SetAttributes( pathW, attributes ));

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT Library_spot_io_native_Microsoft_SPOT_IO_NativeIO::FindVolume( CLR_RT_HeapBlock& hbNamespaceRef, FileSystemVolume*& volume )
{
    NATIVE_PROFILE_CLR_IO();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_String* hbName;
    LPCSTR                   nameSpace;
    UINT32                   nameSpaceLength;
    
    hbName      = hbNamespaceRef.DereferenceString(); FAULT_ON_NULL(hbName);
    nameSpace   = hbName->StringText();

    if(nameSpace[ 0 ] == '\\') nameSpace++;
    
    nameSpaceLength = hal_strlen_s(nameSpace);
    
    /// Retrieve appropriate driver that handles this namespace.
    if((volume = FileSystemVolumeList::FindVolume( nameSpace, nameSpaceLength )) == NULL)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_VOLUME_NOT_FOUND);
    }

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_io_native_Microsoft_SPOT_IO_NativeIO::FindVolume( CLR_RT_HeapBlock& hbPathRef, FileSystemVolume*& volume, UnicodeString& relativePathW )
{
    NATIVE_PROFILE_CLR_IO();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_String*     hbName;
    LPCSTR                       fullPath;
    LPCSTR                       nameSpace;
    LPCSTR                       relativePath;
    UINT32                       nameSpaceLength;

    hbName = hbPathRef.DereferenceString(); FAULT_ON_NULL(hbName);
    fullPath = hbName->StringText();
    TINYCLR_CHECK_HRESULT(CLR_RT_FileStream::SplitFilePath( fullPath, &nameSpace, &nameSpaceLength, &relativePath ));

    /// Retrieve appropriate driver that handles this namespace.
    if((volume = FileSystemVolumeList::FindVolume( nameSpace, nameSpaceLength )) == NULL)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_DRIVER);
    }

    TINYCLR_CHECK_HRESULT(relativePathW.Assign( relativePath ));

    TINYCLR_NOCLEANUP();
}
