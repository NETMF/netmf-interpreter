////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#ifndef _SPOT_IO_NATIVE_H_
#define _SPOT_IO_NATIVE_H_

#include <TinyCLR_Interop.h>
#include <TinyCLR_Stream.h>

struct Library_spot_io_native_System_IO_FileSystemManager
{
    static const int FIELD_STATIC__m_openFiles = 0;
    static const int FIELD_STATIC__m_lockedDirs = 1;
    static const int FIELD_STATIC__CurrentDirectory = 2;
    static const int FIELD_STATIC__m_currentDirectoryRecord = 3;


    //--//

};

struct Library_spot_io_native_System_IO_FileSystemManager__FileRecord
{
    static const int FIELD__FullName = 1;
    static const int FIELD__NativeFileStream = 2;
    static const int FIELD__Share = 3;


    //--//

};

struct Library_spot_io_native_Microsoft_SPOT_IO_MediaEventArgs
{
    static const int FIELD__Time   = 1;
    static const int FIELD__Volume = 2;


    //--//

};

struct Library_spot_io_native_Microsoft_SPOT_IO_NativeFileInfo
{
    static const int FIELD__Attributes     = 1;
    static const int FIELD__CreationTime   = 2;
    static const int FIELD__LastAccessTime = 3;
    static const int FIELD__LastWriteTime  = 4;
    static const int FIELD__Size           = 5;
    static const int FIELD__FileName       = 6;


    //--//

};

struct Library_spot_io_native_Microsoft_SPOT_IO_NativeFileStream
{
    static const int FIELD__m_fs = 1;

    TINYCLR_NATIVE_DECLARE(_ctor___VOID__STRING__I4);
    TINYCLR_NATIVE_DECLARE(Read___I4__SZARRAY_U1__I4__I4__I4);
    TINYCLR_NATIVE_DECLARE(Write___I4__SZARRAY_U1__I4__I4__I4);
    TINYCLR_NATIVE_DECLARE(Seek___I8__I8__U4);
    TINYCLR_NATIVE_DECLARE(Flush___VOID);
    TINYCLR_NATIVE_DECLARE(GetLength___I8);
    TINYCLR_NATIVE_DECLARE(SetLength___VOID__I8);
    TINYCLR_NATIVE_DECLARE(GetStreamProperties___VOID__BYREF_BOOLEAN__BYREF_BOOLEAN__BYREF_BOOLEAN);
    TINYCLR_NATIVE_DECLARE(Close___VOID);

    //--//

    static HRESULT ReadWriteHelper( CLR_RT_StackFrame& stack, BOOL isRead );

    static HRESULT GetFileStream( CLR_RT_StackFrame& stack, CLR_RT_FileStream*& fs );

};

struct Library_spot_io_native_Microsoft_SPOT_IO_NativeFindFile
{
    static const int FIELD__m_ff = 1;

    TINYCLR_NATIVE_DECLARE(_ctor___VOID__STRING__STRING);
    TINYCLR_NATIVE_DECLARE(GetNext___MicrosoftSPOTIONativeFileInfo);
    TINYCLR_NATIVE_DECLARE(Close___VOID);
    TINYCLR_NATIVE_DECLARE(GetFileInfo___STATIC__MicrosoftSPOTIONativeFileInfo__STRING);

    //--//

    static HRESULT GetFindFile( CLR_RT_StackFrame& stack, CLR_RT_FindFile*& ff );
};

struct Library_spot_io_native_Microsoft_SPOT_IO_NativeIO
{
    TINYCLR_NATIVE_DECLARE(Format___STATIC__VOID__STRING__STRING__STRING__U4);
    TINYCLR_NATIVE_DECLARE(Delete___STATIC__VOID__STRING);
    TINYCLR_NATIVE_DECLARE(Move___STATIC__BOOLEAN__STRING__STRING);
    TINYCLR_NATIVE_DECLARE(CreateDirectory___STATIC__VOID__STRING);
    TINYCLR_NATIVE_DECLARE(GetAttributes___STATIC__U4__STRING);
    TINYCLR_NATIVE_DECLARE(SetAttributes___STATIC__VOID__STRING__U4);

    //--//
    static HRESULT FindVolume( CLR_RT_HeapBlock& hbNamespaceRef, FileSystemVolume*& volume                               );
    static HRESULT FindVolume( CLR_RT_HeapBlock& hbPathRef     , FileSystemVolume*& volume, UnicodeString& relativePathW );

};

struct Library_spot_io_native_Microsoft_SPOT_IO_RemovableMedia
{
    static const int FIELD_STATIC__Insert   = 4;
    static const int FIELD_STATIC__Eject    = 5;
    static const int FIELD_STATIC___volumes = 6;
    static const int FIELD_STATIC___events  = 7;

    TINYCLR_NATIVE_DECLARE(MountRemovableVolumes___STATIC__VOID);

    //--//

};

struct Library_spot_io_native_Microsoft_SPOT_IO_StorageEvent
{
    static const int FIELD__EventType = 3;
    static const int FIELD__Handle    = 4;
    static const int FIELD__Time      = 5;


    //--//

};


struct Library_spot_io_native_Microsoft_SPOT_IO_VolumeInfo
{
    static const int FIELD__Name            = 1;
    static const int FIELD__VolumeLabel     = 2;
    static const int FIELD__VolumeID        = 3;
    static const int FIELD__FileSystem      = 4;
    static const int FIELD__FileSystemFlags = 5;
    static const int FIELD__DeviceFlags     = 6;
    static const int FIELD__SerialNumber    = 7;
    static const int FIELD__TotalFreeSpace  = 8;
    static const int FIELD__TotalSize       = 9;
    static const int FIELD__VolumePtr       = 10;

    TINYCLR_NATIVE_DECLARE(_ctor___VOID__STRING);
    TINYCLR_NATIVE_DECLARE(_ctor___VOID__U4);
    TINYCLR_NATIVE_DECLARE(Refresh___VOID);
    TINYCLR_NATIVE_DECLARE(FlushAll___VOID);
    TINYCLR_NATIVE_DECLARE(GetVolumes___STATIC__SZARRAY_MicrosoftSPOTIOVolumeInfo);
    TINYCLR_NATIVE_DECLARE(GetFileSystems___STATIC__SZARRAY_STRING);

    //--//

    static HRESULT UpdateVolumeInfo( CLR_RT_HeapBlock* hbVolume, FileSystemVolume* volume );

};



extern const CLR_RT_NativeAssemblyData g_CLR_AssemblyNative_Microsoft_SPOT_IO;

#endif  //_SPOT_IO_NATIVE_H_
