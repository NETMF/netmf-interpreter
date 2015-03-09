////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;

namespace Microsoft.SPOT.Emulator.FS
{
    // !! KEEP IN-SYNC WITH FS_BUFFERING_STRATEGY in FS_decl.h !!
    public enum BufferingStrategy
    {
        SyncIO = 1, // I/O is synchronous and does not require buffering 
        DirectIO = 2, // I/O is asynchronous from the managed application heap
        SystemBufferedIO = 3, // I/O is asynchronous from a PAL level buffer 
        DriverBufferedIO = 4, // I/O is asynchronous from a HAL level or HW buffer
    }

    public struct InternalDriverDetails
    {
        public byte[]            Namespace;
        public uint              SerialNumber;
        public uint              DeviceFlags;
        
        public bool              IsNative;

        public BufferingStrategy BufferingStrategy;
        public byte[]            InputBuffer;
        public byte[]            OutputBuffer;
        public int               InputBufferSize;
        public int               OutputBufferSize;
        public bool              CanRead;
        public bool              CanWrite;
        public bool              CanSeek;
        public int               ReadTimeout;
        public int               WriteTimeout;

        public String FileSystemName;
        public uint BlockStorageDeviceContext;
        public uint VolumeId;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FsFileInfo
    {
        public uint Attributes;
        public long CreationTime;
        public long LastAccessTime;
        public long LastWriteTime;
        public long Size;
        public string FileName;
    }

    public interface IFSDriver
    {
        InternalDriverDetails[] GetVolumesInfo();

        int Open(uint fsId, string path, ref uint handle);
        int Close(uint handle);
        int Read(uint handle, IntPtr buffer, int count, ref int bytesRead);
        int Write(uint handle, IntPtr buffer, int count, ref int bytesWritten);
        int Flush(uint handle);
        int Seek(uint handle, long offset, uint origin, ref long position);
        int GetLength(uint handle, ref long length);
        int SetLength(uint handle, long length);

        int FindOpen(uint fsId, string fileSpec, ref uint searchHandle);
        int FindNext(uint searchHandle, ref FsFileInfo findData, ref bool found);
        int FindClose(uint searchHandle);

        int GetFileInfo(uint fsId, string path, ref FsFileInfo fileInfo, ref bool found);

        int CreateDirectory(uint fsId, string path);
        int Move(uint fsId, string source, string dest);
        int Delete(uint fsId, string path);
        int GetAttributes(uint fsId, string path, ref uint attributes);
        int SetAttributes(uint fsId, string path, uint attributes);
        int Format(uint fsId, string volumeLabel, uint parameter);
        int GetSizeInfo(uint fsId, ref long totalSize, ref long totalFreeSpace);
        int FlushAll(uint fsId);
        int GetVolumeLabel(uint fsId, IntPtr volumeLabel, int volumeLabelLen);
    }
}

