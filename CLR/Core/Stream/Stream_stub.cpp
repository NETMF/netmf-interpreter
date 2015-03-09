////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "TinyCLR_Types.h"
#include "TinyCLR_Stream.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT CLR_RT_FileStream::CreateInstance( CLR_RT_HeapBlock& ref, LPCSTR path, int bufferSize )
{
    TINYCLR_FEATURE_STUB_RETURN();
}

void CLR_RT_FileStream::Relocate()
{
}

void CLR_RT_FileStream::RelocationHandler( CLR_RT_HeapBlock_BinaryBlob* ptr )
{
}

HRESULT CLR_RT_FileStream::SplitFilePath( LPCSTR fullPath, LPCSTR* nameSpace, UINT32* nameSpaceLength, LPCSTR* relativePath )
{
    TINYCLR_FEATURE_STUB_RETURN();
}

HRESULT CLR_RT_FileStream::AssignStorage( BYTE* bufferIn, size_t sizeIn, BYTE* bufferOut, size_t sizeOut )
{   
    TINYCLR_FEATURE_STUB_RETURN();
}

HRESULT CLR_RT_FileStream::Read( BYTE* buffer, int count, int* bytesRead )
{
    TINYCLR_FEATURE_STUB_RETURN();
}

HRESULT CLR_RT_FileStream::Write( BYTE* buffer, int count, int* bytesWritten )
{
    TINYCLR_FEATURE_STUB_RETURN();
}

HRESULT CLR_RT_FileStream::Seek( INT64 offset, UINT32 origin, INT64* position )
{
    TINYCLR_FEATURE_STUB_RETURN();
}

HRESULT CLR_RT_FileStream::Close()
{
    TINYCLR_FEATURE_STUB_RETURN();
}

HRESULT CLR_RT_FileStream::Flush()
{
    TINYCLR_FEATURE_STUB_RETURN();
}

HRESULT CLR_RT_FileStream::GetLength( INT64 *length )
{
    TINYCLR_FEATURE_STUB_RETURN();
}

HRESULT CLR_RT_FileStream::SetLength( INT64 length )
{
    TINYCLR_FEATURE_STUB_RETURN();
}

//--//

HRESULT CLR_RT_FindFile::CreateInstance( CLR_RT_HeapBlock &ref, LPCSTR path, LPCSTR searchPattern )
{
    TINYCLR_FEATURE_STUB_RETURN();
}

HRESULT CLR_RT_FindFile::GetNext( FS_FILEINFO **fi, BOOL *found )
{
    TINYCLR_FEATURE_STUB_RETURN();
}

HRESULT CLR_RT_FindFile::CreateFilenameString( CLR_RT_HeapBlock& ref )
{
    TINYCLR_FEATURE_STUB_RETURN();
}

HRESULT CLR_RT_FindFile::Close()
{
    TINYCLR_FEATURE_STUB_RETURN();
}



