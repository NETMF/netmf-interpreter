////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "TinyCLR_Types.h"
#include "TinyCLR_Stream.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT CLR_RT_FileStream::CreateInstance( CLR_RT_HeapBlock& ref, LPCSTR path, int bufferSize )
{
    NATIVE_PROFILE_CLR_IO();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_BinaryBlob* blob;
    CLR_UINT32                   blobSize = sizeof(CLR_RT_FileStream);
    CLR_RT_FileStream*           fs;
    UnicodeString                pathW;

    LPCSTR     nameSpace;
    LPCSTR     relativePath;
    CLR_UINT32 nameSpaceLength;    

    int inputBufferSize  = 0;
    int outputBufferSize = 0;

    FileSystemVolume*        driver;
    STREAM_DRIVER_DETAILS*   sdd;

    TINYCLR_CHECK_HRESULT(CLR_RT_FileStream::SplitFilePath( path, &nameSpace, &nameSpaceLength, &relativePath ));

    /// Retrieve appropriate driver that handles this namespace.
    if((driver = FileSystemVolumeList::FindVolume( nameSpace, nameSpaceLength )) == NULL)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_VOLUME_NOT_FOUND);
    }

    if(!(driver->ValidateStreamDriver()))
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_DRIVER);
    }

    sdd = driver->DriverDetails();

    if(sdd->bufferingStrategy == SYSTEM_BUFFERED_IO)
    {
        if(bufferSize == 0)
        {
            inputBufferSize  = (sdd->inputBufferSize  > 0) ? sdd->inputBufferSize  : FS_DEFAULT_BUFFER_SIZE;
            outputBufferSize = (sdd->outputBufferSize > 0) ? sdd->outputBufferSize : FS_DEFAULT_BUFFER_SIZE;
        }
        else
        {
            inputBufferSize  = bufferSize;
            outputBufferSize = bufferSize;
        }

        blobSize += inputBufferSize + outputBufferSize;
    }

    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_BinaryBlob::CreateInstance( ref, blobSize, NULL, CLR_RT_FileStream::RelocationHandler, 0 ));

    blob = ref.DereferenceBinaryBlob();
    fs   = (CLR_RT_FileStream*)blob->GetData();

    // Clear the memory
    CLR_RT_Memory::ZeroFill( fs, blobSize );

    fs->m_driver        = driver;
    fs->m_driverDetails = sdd;

    switch(sdd->bufferingStrategy)
    {
        case SYSTEM_BUFFERED_IO: // I/O is asynchronous from a PAL level buffer: the runtime will alocate the necesary memory
            {
                BYTE* inputBuffer = (BYTE*)&(fs[ 1 ]);
                BYTE* outputBuffer = inputBuffer + inputBufferSize;

                TINYCLR_CHECK_HRESULT(fs->AssignStorage( inputBuffer, inputBufferSize, outputBuffer, outputBufferSize ));
            }
            break;
            
        case DRIVER_BUFFERED_IO: // I/O is asynchronous from a HAL level or HW buffer: the runtime will just handle the existing memory
            if((sdd->inputBuffer  == NULL && sdd->canRead ) ||
               (sdd->outputBuffer == NULL && sdd->canWrite))
            {
                TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_DRIVER);
            }

            TINYCLR_CHECK_HRESULT(fs->AssignStorage( sdd->inputBuffer,  sdd->inputBufferSize, 
                                                     sdd->outputBuffer, sdd->outputBufferSize ));
            break;
    }

    TINYCLR_CHECK_HRESULT(pathW.Assign( relativePath ));
    TINYCLR_CHECK_HRESULT(driver->Open( pathW, &(fs->m_handle) ));

    TINYCLR_NOCLEANUP();
}

void CLR_RT_FileStream::Relocate()
{
    NATIVE_PROFILE_CLR_IO();
    if(m_driverDetails->bufferingStrategy == SYSTEM_BUFFERED_IO)
    {
        CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_dataIn  );
        CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_dataOut );
    }
}

void CLR_RT_FileStream::RelocationHandler( CLR_RT_HeapBlock_BinaryBlob* ptr )
{
    NATIVE_PROFILE_CLR_IO();
    CLR_RT_FileStream* pThis = (CLR_RT_FileStream*)ptr->GetData();

    pThis->Relocate();
}

HRESULT CLR_RT_FileStream::SplitFilePath( LPCSTR fullPath, LPCSTR* nameSpace, UINT32* nameSpaceLength, LPCSTR* relativePath )
{
    NATIVE_PROFILE_CLR_IO();
    TINYCLR_HEADER();

    static const char root[] = "\\";

    char *c = (char *)fullPath;
    UINT32 nsLen = 0;

    if (!fullPath || !nameSpace || !nameSpaceLength || !relativePath || *c != '\\')
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }

    c++;
    *nameSpace = c;
    while((*c != '\\') && (*c != 0))
    {
        c++;        
        nsLen++;
    }    

    // relative path always have to start with a '\' (*c will be '\0' only when fullPath is \<namespace>)
    *relativePath = (*c == 0) ? (LPCSTR)&root : c;

    // Invalid paths should be taken care of by Path.NormalizePath() in the managed code.
    if(nsLen >= FS_NAME_MAXLENGTH || hal_strlen_s(*relativePath) >= FS_MAX_PATH_LENGTH)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_PATH_TOO_LONG);
    }
    
    *nameSpaceLength = nsLen;

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_FileStream::AssignStorage( BYTE* bufferIn, size_t sizeIn, BYTE* bufferOut, size_t sizeOut )
{   
    TINYCLR_HEADER();
   
    m_dataIn = bufferIn;
    m_dataInSize = sizeIn;
    m_dataOut = bufferOut;
    m_dataOutSize = sizeOut;

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT CLR_RT_FileStream::Read( BYTE* buffer, int count, int* bytesRead )
{
    TINYCLR_HEADER();

    switch(m_driverDetails->bufferingStrategy)
    {
    case SYNC_IO:
    case DIRECT_IO:
        TINYCLR_CHECK_HRESULT(m_driver->Read( m_handle, buffer, count, bytesRead ));

        break;

    case SYSTEM_BUFFERED_IO:
    case DRIVER_BUFFERED_IO:
        TINYCLR_CHECK_HRESULT(m_driver->Read( m_handle, m_dataIn, MIN(count, m_dataInSize), bytesRead ));

        if(*bytesRead > 0)
        {
            memcpy( buffer, m_dataIn, *bytesRead );
        }

        break;
    }
    
    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_FileStream::Write( BYTE* buffer, int count, int* bytesWritten )
{
    TINYCLR_HEADER();

    switch(m_driverDetails->bufferingStrategy)
    {
    case SYNC_IO:
    case DIRECT_IO:
        TINYCLR_CHECK_HRESULT(m_driver->Write( m_handle, buffer, count, bytesWritten ));

        break;

    case SYSTEM_BUFFERED_IO:
    case DRIVER_BUFFERED_IO:
        count = MIN(count, m_dataOutSize);

        memcpy( m_dataOut, buffer, count );

        TINYCLR_CHECK_HRESULT(m_driver->Write( m_handle, m_dataOut, count, bytesWritten ));

        break;
    }
    
    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_FileStream::Seek( INT64 offset, UINT32 origin, INT64* position )
{
    return m_driver->Seek( m_handle, offset, origin, position );
}

HRESULT CLR_RT_FileStream::Close()
{
    return m_driver->Close( m_handle );
}

HRESULT CLR_RT_FileStream::Flush()
{
    return m_driver->Flush( m_handle );
}

HRESULT CLR_RT_FileStream::GetLength( INT64 *length )
{
    return m_driver->GetLength( m_handle, length );
}

HRESULT CLR_RT_FileStream::SetLength( INT64 length )
{
    return m_driver->SetLength( m_handle, length );
}

//--//

HRESULT CLR_RT_FindFile::CreateInstance( CLR_RT_HeapBlock& ref, LPCSTR path, LPCSTR searchPattern )
{
    NATIVE_PROFILE_CLR_IO();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_BinaryBlob* blob;
    CLR_RT_FindFile*             ff;
    UnicodeString                pathW;
    CLR_UINT32                   size;
    CLR_UINT32                   i, j;
    CLR_UINT32                   fullPathBufferSize;

    LPCSTR     nameSpace;
    LPCSTR     relativePath;
    LPCWSTR    relativePathW;
    CLR_UINT32 nameSpaceLength;

    // We will support only the "*" search pattern for now
    if(hal_strncmp_s( searchPattern, "*", 2 ) != 0)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_NOT_SUPPORTED);
    }

    TINYCLR_CHECK_HRESULT(CLR_RT_FileStream::SplitFilePath( path, &nameSpace, &nameSpaceLength, &relativePath ));
    TINYCLR_CHECK_HRESULT(pathW.Assign( relativePath ));

    relativePathW = (LPCWSTR)pathW;

    fullPathBufferSize = FS_MAX_PATH_LENGTH + nameSpaceLength + 1; // '\' before the namespace

    size = sizeof(CLR_RT_FindFile) + fullPathBufferSize * sizeof(UINT16);  

    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_BinaryBlob::CreateInstance( ref, size, NULL, CLR_RT_FindFile::RelocationHandler, 0 ));

    blob = ref.DereferenceBinaryBlob();
    ff   = (CLR_RT_FindFile*)blob->GetData();

    // Clear the memory
    CLR_RT_Memory::ZeroFill( ff, sizeof(CLR_RT_FindFile) );


    /// Retrieve appropriate driver that handles this namespace.    
    if((ff->m_driver = FileSystemVolumeList::FindVolume( nameSpace, nameSpaceLength )) == NULL)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_VOLUME_NOT_FOUND);
    }

    // Validate all the find related function are present in the driver
    if(!(ff->m_driver->ValidateFind()))
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_NOT_SUPPORTED);
    }

    ff->m_fullPath = (UINT16*)(&ff[ 1 ]);
    ff->m_fullPathBufferSize = fullPathBufferSize;

    // Copy the '\' and nameSpace
    for(i = 0; i < nameSpaceLength + 1; i++)
    {
        ff->m_fullPath[ i ] = path[ i ];
    }

    // Copy the rest of the path from the UTF16 buffer
    for(j = 0; i < fullPathBufferSize && relativePathW[ j ] != 0; i++, j++)
    {
        ff->m_fullPath[ i ] = relativePathW[ j ];
    }

    // Make sure we always ends with '\\'
    if(ff->m_fullPath[ i - 1 ] != '\\')
    {
        ff->m_fullPath[ i++ ] = '\\';
    }

    ff->m_fi.FileName = &(ff->m_fullPath[ i ]);
    ff->m_fi.FileNameSize = fullPathBufferSize - i;

    TINYCLR_CHECK_HRESULT(ff->m_driver->FindOpen( relativePathW, &(ff->m_handle) ));

    TINYCLR_NOCLEANUP();
}

void CLR_RT_FindFile::Relocate()
{
    NATIVE_PROFILE_CLR_IO();

    CLR_RT_GarbageCollector::Heap_Relocate( (void**)&(m_fi.FileName) );
    CLR_RT_GarbageCollector::Heap_Relocate( (void**)&(m_fullPath)    );
}

void CLR_RT_FindFile::RelocationHandler( CLR_RT_HeapBlock_BinaryBlob* ptr )
{
    NATIVE_PROFILE_CLR_IO();
    CLR_RT_FindFile* pThis = (CLR_RT_FindFile*)ptr->GetData();

    pThis->Relocate();
}

HRESULT CLR_RT_FindFile::GetNext( FS_FILEINFO **fi, BOOL *found )
{
    *fi = &m_fi;

    return m_driver->FindNext( m_handle, &m_fi, found );
}

HRESULT CLR_RT_FindFile::CreateFilenameString( CLR_RT_HeapBlock& ref )
{
    return CLR_RT_HeapBlock_String::CreateInstance( ref, m_fullPath, m_fullPathBufferSize );
}

HRESULT CLR_RT_FindFile::Close()
{
    return m_driver->FindClose( m_handle );
}
