////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#include "spot_io.h"

HRESULT Library_spot_io_native_Microsoft_SPOT_IO_VolumeInfo::_ctor___VOID__STRING( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_IO();
    TINYCLR_HEADER();

    FileSystemVolume* volume;

    TINYCLR_CHECK_HRESULT(Library_spot_io_native_Microsoft_SPOT_IO_NativeIO::FindVolume( stack.Arg1(), volume ));

    TINYCLR_CHECK_HRESULT(UpdateVolumeInfo( stack.This(), volume ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_io_native_Microsoft_SPOT_IO_VolumeInfo::_ctor___VOID__U4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_IO();
    TINYCLR_HEADER();

    FileSystemVolume* volume;

    volume = (FileSystemVolume*)(stack.Arg1().NumericByRef().u4);

    if(FileSystemVolumeList::Contains( volume ))
    {
        TINYCLR_SET_AND_LEAVE(UpdateVolumeInfo( stack.This(), volume ));
    }

    TINYCLR_SET_AND_LEAVE(CLR_E_FILE_IO);

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_io_native_Microsoft_SPOT_IO_VolumeInfo::Refresh___VOID( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pThis = stack.This();
    FileSystemVolume* volume;

    TINYCLR_CHECK_HRESULT(Library_spot_io_native_Microsoft_SPOT_IO_NativeIO::FindVolume( pThis[ FIELD__Name ], volume ));

    TINYCLR_CHECK_HRESULT(UpdateVolumeInfo( pThis, volume ));

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_io_native_Microsoft_SPOT_IO_VolumeInfo::GetVolumes___STATIC__SZARRAY_MicrosoftSPOTIOVolumeInfo( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_IO();
    TINYCLR_HEADER();
    UINT32 i;
    UINT32 numVolumes = FileSystemVolumeList::GetNumVolumes();
    CLR_RT_HeapBlock& ret = stack.PushValueAndClear();
    CLR_RT_HeapBlock* hbVolumes;
    FileSystemVolume* volume;

    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance( ret, numVolumes, g_CLR_RT_WellKnownTypes.m_VolumeInfo ));

    hbVolumes = (CLR_RT_HeapBlock*)ret.DereferenceArray()->GetFirstElement();
    volume = FileSystemVolumeList::GetFirstVolume();

    for (i = 0; i < numVolumes; i++)
    {
        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex( hbVolumes[ i ], g_CLR_RT_WellKnownTypes.m_VolumeInfo ));

        TINYCLR_CHECK_HRESULT(UpdateVolumeInfo( hbVolumes[ i ].Dereference(), volume ));

        volume = FileSystemVolumeList::GetNextVolume( *volume );
    }

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_io_native_Microsoft_SPOT_IO_VolumeInfo::GetFileSystems___STATIC__SZARRAY_STRING( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_IO();
    TINYCLR_HEADER();

    UINT32 i;
    CLR_RT_HeapBlock& ret = stack.PushValueAndClear();
    CLR_RT_HeapBlock* fsNames;

    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance( ret, g_InstalledFSCount, g_CLR_RT_WellKnownTypes.m_String ));

    fsNames = (CLR_RT_HeapBlock*)ret.DereferenceArray()->GetFirstElement();

    for (i = 0; i < g_InstalledFSCount; i++)
    {
        TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_String::CreateInstance( fsNames[ i ], g_AvailableFSInterfaces[ i ].fsDriver->Name ));
    }
    
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_io_native_Microsoft_SPOT_IO_VolumeInfo::FlushAll___VOID( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* pThis = stack.This();
    FileSystemVolume* volume;

    TINYCLR_CHECK_HRESULT(Library_spot_io_native_Microsoft_SPOT_IO_NativeIO::FindVolume( pThis[ FIELD__Name ], volume ));

    TINYCLR_CHECK_HRESULT(volume->FlushAll());

    TINYCLR_NOCLEANUP();

}
//--//

HRESULT Library_spot_io_native_Microsoft_SPOT_IO_VolumeInfo::UpdateVolumeInfo( CLR_RT_HeapBlock* hbVolume, FileSystemVolume* volume )
{
    NATIVE_PROFILE_CLR_IO();
    TINYCLR_HEADER();

    INT64 totalSize;
    INT64 totalFreeSpace;

    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_String::CreateInstance( hbVolume[ FIELD__Name        ], volume->m_nameSpace ));
    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_String::CreateInstance( hbVolume[ FIELD__VolumeLabel ], volume->m_label     ));

    hbVolume[ FIELD__DeviceFlags    ].SetInteger( volume->m_deviceFlags       );
    hbVolume[ FIELD__SerialNumber   ].SetInteger( volume->m_serialNumber      );
    hbVolume[ FIELD__VolumeID       ].SetInteger( volume->m_volumeId.volumeId );
    hbVolume[ FIELD__VolumePtr      ].SetInteger( (CLR_INT32)volume           );

    if(volume->m_fsDriver)
    {
        TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_String::CreateInstance( hbVolume[ FIELD__FileSystem ], volume->m_fsDriver->Name ));
        hbVolume[ FIELD__FileSystemFlags ].SetInteger( volume->m_fsDriver->Flags );

        if(FAILED(volume->GetSizeInfo( &totalSize, &totalFreeSpace )))
        {
            totalSize = 0;
            totalFreeSpace = 0;
        }
    }
    else
    {
        hbVolume[ FIELD__FileSystem ].SetObjectReference( NULL );
        hbVolume[ FIELD__FileSystemFlags ].SetInteger( 0 );

        totalSize = 0;
        totalFreeSpace = 0;
    }

    hbVolume[ FIELD__TotalSize      ].SetInteger( totalSize      );
    hbVolume[ FIELD__TotalFreeSpace ].SetInteger( totalFreeSpace );

    TINYCLR_NOCLEANUP();
}
