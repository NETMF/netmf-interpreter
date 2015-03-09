////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _DRIVERS_FS_DECL_H_
#define _DRIVERS_FS_DECL_H_ 1

//--//

struct BlockStorageDevice;
struct FileSystemVolume;
struct VOLUME_ID;


// All these length includes the NULL termination at the end of the string
#define FS_MAX_PATH_LENGTH      (260 - 2) // To maintain compatibility with the desktop, the max "relative" path we can allow. 2 is the MAX_DRIVE, i.e. "C:", "D:" ... etc
#define FS_MAX_FILENAME_LENGTH  256
#define FS_MAX_DIRECTORY_LENGTH (FS_MAX_PATH_LENGTH - 12) // As required by desktop, the longest directory path is MAX_PATH - 12 (size of an 8.3 file name)

#define FS_NAME_DEFAULT_LENGTH  7
#define FS_NAME_MAXLENGTH       (FS_NAME_DEFAULT_LENGTH+1)

#define FS_LABEL_DEFAULT_LENGTH 11
#define FS_LABEL_MAXLENGTH      (FS_LABEL_DEFAULT_LENGTH+1)

#define FS_DEFAULT_BUFFER_SIZE  512
#define FS_DEFAULT_TIMEOUT      (-1)

#define SEEKORIGIN_BEGIN        0
#define SEEKORIGIN_CURRENT      1
#define SEEKORIGIN_END          2

// Keep in-sync with Framework\Core\System\IO\FileAttributes.cs
#define FILEATTRIBUTE_READONLY  0x01
#define FILEATTRIBUTE_HIDDEN    0x02
#define FILEATTRIBUTE_SYSTEM    0x04
#define FILEATTRIBUTE_DIRECTORY 0x10
#define FILEATTRIBUTE_ARCHIVE   0x20
#define FILEATTRIBUTE_NORMAL    0x80

//--//

struct FS_FILEINFO
{
    UINT32      Attributes;
    INT64       CreationTime;
    INT64       LastAccessTime;
    INT64       LastWriteTime;
    INT64       Size;
    UINT16*     FileName;     // This will point to a buffer of size FileNameSize
    UINT32      FileNameSize; // This is the size of the buffer, determined by the size of the base path given in FindOpen
};

enum FS_BUFFERING_STRATEGY
{
    SYNC_IO              = 1, // I/O is synchronous and does not require buffering 
    DIRECT_IO            = 2, // I/O is asynchronous from the managed application heap
    SYSTEM_BUFFERED_IO   = 3, // I/O is asynchronous from a PAL level buffer 
    DRIVER_BUFFERED_IO   = 4, // I/O is asynchronous from a HAL level or HW buffer
};

struct STREAM_DRIVER_DETAILS
{
    FS_BUFFERING_STRATEGY bufferingStrategy;
    BYTE*                 inputBuffer;
    BYTE*                 outputBuffer;
    int                   inputBufferSize;
    int                   outputBufferSize;
    BOOL                  canRead;
    BOOL                  canWrite;
    BOOL                  canSeek;
    int                   readTimeout;
    int                   writeTimeout;
};

//--//

typedef BOOL    (*FS_ISLOADABLEMEDIA)( BlockStorageDevice* /*driverInterface*/, UINT32* /*numVolumes*/ );
typedef HRESULT (*FS_FORMAT)         ( const VOLUME_ID* /*volume*/, LPCSTR /*volumeLabel*/, UINT32 /*parameters*/ );
typedef HRESULT (*FS_GETSIZEINFO)    ( const VOLUME_ID* /*volume*/, INT64* /*totalSize*/, INT64* /*totalFreeSpace*/ );
typedef HRESULT (*FS_FLUSHALL)       ( const VOLUME_ID* /*volume*/ );
typedef HRESULT (*FS_GETVOLUMELABEL) ( const VOLUME_ID* /*volume*/, LPSTR  /*volumeLabel*/, INT32 /*volumeLabelLen*/ );

typedef HRESULT (*FS_FINDOPEN)       ( const VOLUME_ID* /*volume*/, LPCWSTR /*path*/, UINT32* /*findHandle*/ );
typedef HRESULT (*FS_FINDNEXT)       ( UINT32 /*findHandle*/, FS_FILEINFO* /*findData*/, BOOL* /*found*/ );
typedef HRESULT (*FS_FINDCLOSE)      ( UINT32 /*findHandle*/ );

typedef HRESULT (*FS_GETFILEINFO)    ( const VOLUME_ID* /*volume*/, LPCWSTR /*path*/, FS_FILEINFO* /*fileInfo*/, BOOL* /*found*/ );

typedef HRESULT (*FS_CREATEDIRECTORY)( const VOLUME_ID* /*volume*/, LPCWSTR /*path*/ );
typedef HRESULT (*FS_MOVE)           ( const VOLUME_ID* /*volume*/, LPCWSTR /*oldPath*/, LPCWSTR /*newPath*/ );
typedef HRESULT (*FS_DELETE)         ( const VOLUME_ID* /*volume*/, LPCWSTR /*path*/ );
typedef HRESULT (*FS_GETATTRIBUTES)  ( const VOLUME_ID* /*volume*/, LPCWSTR /*path*/, UINT32* /*attributes*/ );
typedef HRESULT (*FS_SETATTRIBUTES)  ( const VOLUME_ID* /*volume*/, LPCWSTR /*path*/, UINT32 /*attributes*/ );

//--//

#ifdef CreateDirectory
#undef CreateDirectory
#endif

#define FS_DRIVER_ATTRIBUTE__FORMAT_REQUIRES_ERASE 0x10000000

struct FILESYSTEM_DRIVER_INTERFACE
{
    FS_FINDOPEN        FindOpen;
    FS_FINDNEXT        FindNext;
    FS_FINDCLOSE       FindClose;

    FS_GETFILEINFO     GetFileInfo;

    FS_CREATEDIRECTORY CreateDirectory;
    FS_MOVE            Move;
    FS_DELETE          Delete;

    FS_GETATTRIBUTES   GetAttributes;
    FS_SETATTRIBUTES   SetAttributes;
    
    FS_FORMAT          Format;
    FS_ISLOADABLEMEDIA IsLoadableMedia;
    FS_GETSIZEINFO     GetSizeInfo;
    FS_FLUSHALL        FlushAll;
    FS_GETVOLUMELABEL  GetVolumeLabel;

    LPCSTR             Name;
    UINT32             Flags;
};

//--//

typedef void                   (*STREAM_INITIALIZE)        ();
typedef BOOL                   (*STREAM_INITIALIZEVOLUME)  ( const VOLUME_ID* /*volume*/ );
typedef BOOL                   (*STREAM_UNINITIALIZEVOLUME)( const VOLUME_ID* /*volume*/ );
typedef STREAM_DRIVER_DETAILS* (*STREAM_DRIVERDETAILS)     ( const VOLUME_ID* /*volume*/ );
typedef HRESULT                (*STREAM_OPEN)              ( const VOLUME_ID* /*volume*/, LPCWSTR /*path*/, UINT32* /*handle*/ );
typedef HRESULT                (*STREAM_CLOSE)             ( UINT32 /*handle*/ );
typedef HRESULT                (*STREAM_READ)              ( UINT32 /*handle*/, BYTE* /*buffer*/, int /*count*/, int* /*bytesRead*/ );
typedef HRESULT                (*STREAM_WRITE)             ( UINT32 /*handle*/, BYTE* /*buffer*/, int /*count*/, int* /*bytesWritten*/ );
typedef HRESULT                (*STREAM_FLUSH)             ( UINT32 /*handle*/ );
typedef HRESULT                (*STREAM_SEEK_)             ( UINT32 /*handle*/, INT64 /*offset*/, UINT32 /*origin*/, INT64* /*position*/); //STREAM_SEEK conflicts with some windows header, use STREAM_SEEK_ instead
typedef HRESULT                (*STREAM_GETLENGTH)         ( UINT32 /*handle*/, INT64* /*length*/ );
typedef HRESULT                (*STREAM_SETLENGTH)         ( UINT32 /*handle*/, INT64 /*length*/ );

struct STREAM_DRIVER_INTERFACE
{
    STREAM_INITIALIZE         Initialize;
    STREAM_INITIALIZEVOLUME   InitializeVolume;
    STREAM_UNINITIALIZEVOLUME UninitializeVolume;
    STREAM_DRIVERDETAILS      DriverDetails;
    STREAM_OPEN               Open;
    STREAM_CLOSE              Close;
    STREAM_READ               Read;
    STREAM_WRITE              Write;
    STREAM_FLUSH              Flush;
    STREAM_SEEK_              Seek;
    STREAM_GETLENGTH          GetLength;
    STREAM_SETLENGTH          SetLength;
};

//--//

// Storage event bit fields
// 0-7  : Event Sub Category (Insert, Eject etc.)
// 8-15 : Category (always EVENT_STORAGE)
// 16-23: Internal flags.
// 24-31: Driver supplied custom flags. 

#define EVENT_STORAGE                  3
#define EVENT_SUBCATEGORY_MEDIAINSERT  1
#define EVENT_SUBCATEGORY_MEDIAEJECT   2

void FS_MountVolume( LPCSTR nameSpace, UINT32 serialNumber, UINT32 deviceFlags, BlockStorageDevice* blockStorageDevice );
void FS_UnmountVolume( BlockStorageDevice* blockStorageDevice );

void FS_Initialize();

struct FILESYSTEM_INTERFACES
{
    FILESYSTEM_DRIVER_INTERFACE* fsDriver;
    STREAM_DRIVER_INTERFACE*     streamDriver;
};

extern const size_t g_InstalledFSCount;
extern FILESYSTEM_INTERFACES g_AvailableFSInterfaces[];

//--//

struct VOLUME_ID
{
    BlockStorageDevice*             blockStorageDevice;
    UINT32                          volumeId;
};


struct FileSystemVolume : public HAL_DblLinkedNode<FileSystemVolume>
{

public:
    BOOL InitializeVolume()
    {
        if(!m_streamDriver || !(m_streamDriver->InitializeVolume)) return FALSE;

        return m_streamDriver->InitializeVolume( &m_volumeId );
    }

    BOOL UninitializeVolume()
    {
        // it's we don't have valid stream driver, do nothing
        if(!m_streamDriver || !(m_streamDriver->UninitializeVolume)) return TRUE;

        return m_streamDriver->UninitializeVolume( &m_volumeId );
    }

    HRESULT Format( LPCSTR volumeLabel, UINT32 parameters )
    {
        if(!m_fsDriver || !(m_fsDriver->Format))
        {
            return CLR_E_NOT_SUPPORTED;
        }

        return m_fsDriver->Format( &m_volumeId, volumeLabel, parameters );
    }

    HRESULT GetSizeInfo( INT64* totalSize, INT64* totalFreeSpace )
    {
        if(!m_fsDriver || !(m_fsDriver->GetSizeInfo))
        {
            return CLR_E_NOT_SUPPORTED;
        }

        return m_fsDriver->GetSizeInfo( &m_volumeId, totalSize, totalFreeSpace );
    }

    HRESULT FlushAll()
    {
        if(!m_fsDriver || !(m_fsDriver->FlushAll))
        {
            return CLR_E_NOT_SUPPORTED;
        }

        return m_fsDriver->FlushAll( &m_volumeId );
    }

    HRESULT FindOpen( LPCWSTR path, UINT32* findHandle )
    {
        // Use ValidateFind() to validate, this assert is for debug purpose only
        _ASSERTE(m_fsDriver && m_fsDriver->FindOpen);

        return m_fsDriver->FindOpen( &m_volumeId, path, findHandle );
    }

    HRESULT FindNext( UINT32 findHandle, FS_FILEINFO* findData, BOOL* found )
    {
        // Use ValidateFind() to validate, this assert is for debug purpose only
        _ASSERTE(m_fsDriver && m_fsDriver->FindNext);

        return m_fsDriver->FindNext( findHandle, findData, found );
    }

    HRESULT FindClose( UINT32 findHandle )
    {
        // Use ValidateFind() to validate, this assert is for debug purpose only
        _ASSERTE(m_fsDriver && m_fsDriver->FindClose);

        return m_fsDriver->FindClose( findHandle );
    }

    HRESULT GetFileInfo( LPCWSTR path, FS_FILEINFO* fileInfo, BOOL* found )
    {
        if(!m_fsDriver || !(m_fsDriver->GetFileInfo))
        {
            return CLR_E_NOT_SUPPORTED;
        }

        return m_fsDriver->GetFileInfo( &m_volumeId, path, fileInfo, found );
    }

    HRESULT CreateDirectory( LPCWSTR path )
    {
        if(!m_fsDriver || !(m_fsDriver->CreateDirectory))
        {
            return CLR_E_NOT_SUPPORTED;
        }

        return m_fsDriver->CreateDirectory( &m_volumeId, path );
    }

    HRESULT Move( LPCWSTR oldPath, LPCWSTR newPath )
    {
        if(!m_fsDriver || !(m_fsDriver->Move))
        {
            return CLR_E_NOT_SUPPORTED;
        }

        return m_fsDriver->Move( &m_volumeId, oldPath, newPath );
    }

    HRESULT Delete( LPCWSTR path )
    {
        if(!m_fsDriver || !(m_fsDriver->Delete))
        {
            return CLR_E_NOT_SUPPORTED;
        }

        return m_fsDriver->Delete( &m_volumeId, path );
    }

    HRESULT GetAttributes( LPCWSTR path, UINT32* attributes )
    {
        if(!m_fsDriver || !(m_fsDriver->GetAttributes))
        {
            return CLR_E_NOT_SUPPORTED;
        }

        return m_fsDriver->GetAttributes( &m_volumeId, path, attributes );
    }

    HRESULT SetAttributes( LPCWSTR path, UINT32 attributes )
    {
        if(!m_fsDriver || !(m_fsDriver->SetAttributes))
        {
            return CLR_E_NOT_SUPPORTED;
        }

        return m_fsDriver->SetAttributes( &m_volumeId, path, attributes );
    }

    STREAM_DRIVER_DETAILS* DriverDetails()
    {
        // Use ValidateStreamDriver() to validate, this assert is for debug purpose only
        _ASSERTE(m_streamDriver && m_streamDriver->DriverDetails);

        return m_streamDriver->DriverDetails( &m_volumeId );
    }

    HRESULT Open( LPCWSTR path, UINT32* handle )
    {
        // Use ValidateStreamDriver() to validate, this assert is for debug purpose only
        _ASSERTE(m_streamDriver && m_streamDriver->Open);

        return m_streamDriver->Open( &m_volumeId, path, handle );
    }

    HRESULT Close( UINT32 handle )
    {
        // Use ValidateStreamDriver() to validate, this assert is for debug purpose only
        _ASSERTE(m_streamDriver && m_streamDriver->Close);

        return m_streamDriver->Close( handle );
    }

    HRESULT Read( UINT32 handle, BYTE* buffer, int count, int* bytesRead )
    {
        // Use ValidateStreamDriver() to validate, this assert is for debug purpose only
        _ASSERTE(m_streamDriver && m_streamDriver->Read);

        return m_streamDriver->Read( handle, buffer, count, bytesRead );
    }

    HRESULT Write( UINT32 handle, BYTE* buffer, int count, int* bytesWritten )
    {
        // Use ValidateStreamDriver() to validate, this assert is for debug purpose only
        _ASSERTE(m_streamDriver && m_streamDriver->Write);

        return m_streamDriver->Write( handle, buffer, count, bytesWritten );
    }

    HRESULT Flush( UINT32 handle )
    {
        // Use ValidateStreamDriver() to validate, this assert is for debug purpose only
        _ASSERTE(m_streamDriver && m_streamDriver->Flush);

        return m_streamDriver->Flush( handle );
    }

    HRESULT Seek( UINT32 handle, INT64 offset, UINT32 origin, INT64* position )
    {
        // Use ValidateStreamDriver() to validate, this assert is for debug purpose only
        _ASSERTE(m_streamDriver && m_streamDriver->Seek);

        return m_streamDriver->Seek( handle, offset, origin, position );
    }

    HRESULT GetLength( UINT32 handle, INT64* length )
    {
        // Use ValidateStreamDriver() to validate, this assert is for debug purpose only
        _ASSERTE(m_streamDriver && m_streamDriver->GetLength);

        return m_streamDriver->GetLength( handle, length );
    }

    HRESULT SetLength( UINT32 handle, INT64 length )
    {
        // Use ValidateStreamDriver() to validate, this assert is for debug purpose only
        _ASSERTE(m_streamDriver && m_streamDriver->SetLength);

        return m_streamDriver->SetLength( handle, length );
    }

    //--//

    BOOL ValidateStreamDriver()
    {
        STREAM_DRIVER_DETAILS* details;

        if(!m_streamDriver || !(m_streamDriver->DriverDetails)) // invalid streamDriver, or invalid DriverDetails Fn pointer
        {
            return FALSE;
        }

        details = m_streamDriver->DriverDetails( &m_volumeId );

        if( (!details)                                                                                || // Check for valid stream driver details
            (details->bufferingStrategy < SYNC_IO || details->bufferingStrategy > DRIVER_BUFFERED_IO) || // Check for valid bufferingStrategy
            (!(m_streamDriver->Open) || !(m_streamDriver->Close) || !(m_streamDriver->Flush)||           // Open, Close, Flush, InitializeVolume, and 
             !(m_streamDriver->InitializeVolume) || !(m_streamDriver->UninitializeVolume))            || //    UninitializeVolume is required on all streams
            (details->canRead && !(m_streamDriver->Read))                                             || // if the stream can read, Read is required
            (details->canWrite && !(m_streamDriver->Write))                                           || // if the stream can write, Write is required
            (details->canSeek && (!(m_streamDriver->Seek) || !(m_streamDriver->GetLength)))           || // if the stream can seek, Seek and GetLength is required
            (details->canSeek && details->canWrite && (!(m_streamDriver->SetLength))) )                  // if the stream and seek and write, SetLength is required
        {
            return FALSE;
        }

        return TRUE;
    }

    BOOL ValidateFind()
    {
        return m_fsDriver && (m_fsDriver->FindOpen) && (m_fsDriver->FindNext) && (m_fsDriver->FindClose);
    }

    //--//

    char                            m_nameSpace[FS_NAME_MAXLENGTH];
    char                            m_label[FS_LABEL_MAXLENGTH];
    UINT32                          m_serialNumber;
    UINT32                          m_deviceFlags;
    STREAM_DRIVER_INTERFACE*        m_streamDriver;
    FILESYSTEM_DRIVER_INTERFACE*    m_fsDriver;
    VOLUME_ID                       m_volumeId;
};

struct FileSystemVolumeList
{
    // initailize the list
    static void Initialize();
    
    // walk through list of volumes and calls Init() function
    static BOOL InitializeVolumes();

    // walk through list of volumes and calls Uninit() function
    static BOOL UninitializeVolumes();

    // add fsv to the list
    // If init=true, the InitializeVolume() will be called.
    static BOOL AddVolume( FileSystemVolume* fsv, LPCSTR nameSpace, UINT32 serialNumber, UINT32 deviceFlags,
                           STREAM_DRIVER_INTERFACE* streamDriver, FILESYSTEM_DRIVER_INTERFACE* fsDriver,
                           BlockStorageDevice* blockStorageDevice, UINT32 volumeId, BOOL init );

    // remove fsv from the list
    // uninit = true, UninitializeVolume() will be called.
    static BOOL RemoveVolume( FileSystemVolume* fsv, BOOL uninit );
    
    // returns a pointer to the first volume in the list or NULL if
    // there is none.
    static FileSystemVolume* GetFirstVolume();
    
    static FileSystemVolume* GetNextVolume( FileSystemVolume& volume );

    // returns number of volumes has been declared in the system
    static UINT32 GetNumVolumes();

    // returns the volume driver for the specified namespace
    static FileSystemVolume* FindVolume( LPCSTR nameSpace, UINT32 nameSpaceLength );

    // returns true if fsv is in the list, false otherwise
    static BOOL Contains( FileSystemVolume* fsv );

    //--//
    
    static HAL_DblLinkedList<FileSystemVolume> s_volumeList;

    // The zombie list is for the file system volumes of a removable media after it's been ejected
    // so it can be cleaned up properly later
    static HAL_DblLinkedList<FileSystemVolume> s_zombieVolumeList;
};

//--//

// Implement this function to add all the built-in, static volumes (i.e. ones that can't be removed) to the system
// Use FileSystemVolumeList::AddVolume() to add individual volumes
void FS_AddVolumes();

// Implement this function to add all the removable volumes *that's inserted in the system at startup* to the system
// Use FS_MountVolume() to add individual volumes
void FS_MountRemovableVolumes();

#endif // _DRIVERS_FS_DECL_H_

