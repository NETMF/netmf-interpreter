////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _TINYCLR_STREAM_H_
#define _TINYCLR_STREAM_H_

#include <TinyCLR_Types.h>
#include <TinyCLR_Runtime.h>

//--//

struct CLR_RT_FileStream
{
private:
    FileSystemVolume*            m_driver;
    STREAM_DRIVER_DETAILS*       m_driverDetails;
    BYTE*                        m_dataIn;
    BYTE*                        m_dataOut;
    int                          m_dataInSize;
    int                          m_dataOutSize;
    UINT32                       m_handle;

    //--// 

public:
    static HRESULT CreateInstance( CLR_RT_HeapBlock& ref, LPCSTR path, int bufferSize );

    static void RelocationHandler( CLR_RT_HeapBlock_BinaryBlob* ptr );

    static HRESULT SplitFilePath( LPCSTR fullPath, LPCSTR* nameSpace, UINT32* nameSpaceLength, LPCSTR* relativePath );

    //--//

    void Relocate();

    //--//

    HRESULT AssignStorage ( BYTE* storageIn, size_t sizeIn, BYTE* storageOut, size_t sizeOut );

    HRESULT Read          ( BYTE* buffer, int    count , int*   bytesRead    );
    HRESULT Write         ( BYTE* buffer, int    count , int*   bytesWritten );
    HRESULT Seek          ( INT64 offset, UINT32 origin, INT64* position     );

    HRESULT Flush         ( );

    HRESULT GetLength     ( INT64* length );
    HRESULT SetLength     ( INT64  length );

    HRESULT Close         ( );

    BOOL CanRead ( ) { return m_driverDetails->canRead;  }
    BOOL CanWrite( ) { return m_driverDetails->canWrite; }
    BOOL CanSeek ( ) { return m_driverDetails->canSeek;  }
    
    int GetReadTimeout ( ) { return m_driverDetails->readTimeout;  }
    int GetWriteTimeout( ) { return m_driverDetails->writeTimeout; }

    FS_BUFFERING_STRATEGY GetBufferingStrategy( ) { return m_driverDetails->bufferingStrategy; }
};

//--//

struct CLR_RT_FindFile
{
private:
    FileSystemVolume* m_driver;
    UINT32            m_handle;
    FS_FILEINFO       m_fi;
    UINT16*           m_fullPath;
    UINT32            m_fullPathBufferSize;

public:
    static HRESULT CreateInstance( CLR_RT_HeapBlock& ref, LPCSTR path, LPCSTR searchPattern );

    static void RelocationHandler( CLR_RT_HeapBlock_BinaryBlob* ptr );

    //--//

    void Relocate();

    //--//

    HRESULT GetNext( FS_FILEINFO **fi, BOOL *found );
    HRESULT CreateFilenameString( CLR_RT_HeapBlock& ref );
    HRESULT Close();
};

//--//

#endif // _TINYCLR_STREAM_H_

