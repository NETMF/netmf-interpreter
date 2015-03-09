////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using Microsoft.SPOT.Emulator.BlockStorage;

namespace Microsoft.SPOT.Emulator.FS
{
    internal class FSDriver : HalDriver<IFSDriver>, IFSDriver
    {
        // !! KEEP IN-SYNC WITH FS_NAME_DEFAULT_LENGTH in FS_decl.h !!
        public const int FsNameDefaultLength         = 7;

        private ManagedFileSystem GetManagedFileSystem(uint volumeId)
        {
            return (ManagedFileSystem)this.Emulator.FileSystems[volumeId];
        }

        private void ParseHandle(uint handle, out uint volumeId, out ushort emulatorHandle)
        {
            volumeId = (handle & 0xFFFF0000) >> 16;
            emulatorHandle = (ushort)(handle & 0x0000FFFF);
        }

        private uint MakeHandle(uint volumeId, ushort emulatorHandle)
        {
            return (uint)((volumeId & 0xFFFF) << 16) | (uint)emulatorHandle;
        }

        private int TranslateException(Exception e)
        {
            if (e is FileNotFoundException) return unchecked((int)(uint)CLR_ERRORCODE.CLR_E_FILE_NOT_FOUND);
            else if (e is DirectoryNotFoundException) return unchecked((int)(uint)CLR_ERRORCODE.CLR_E_DIRECTORY_NOT_FOUND);
            else if (e is PathTooLongException) return unchecked((int)(uint)CLR_ERRORCODE.CLR_E_PATH_TOO_LONG);
            else if (e is UnauthorizedAccessException) return unchecked((int)(uint)CLR_ERRORCODE.CLR_E_UNAUTHORIZED_ACCESS);
            else if (e is IOException) return unchecked((int)(uint)CLR_ERRORCODE.CLR_E_FILE_IO);
            else if (e is ArgumentOutOfRangeException) return unchecked((int)(uint)CLR_ERRORCODE.CLR_E_OUT_OF_RANGE);
            else if (e is ArgumentException) return unchecked((int)(uint)CLR_ERRORCODE.CLR_E_INVALID_PARAMETER);
            // Default to CLR_E_FILE_IO
            return unchecked((int)(uint)CLR_ERRORCODE.CLR_E_FILE_IO);
        }

        #region IFSDriver Members

        InternalDriverDetails[] IFSDriver.GetVolumesInfo()
        {
            int numVolumes = this.Emulator.FileSystems.Count;

            InternalDriverDetails[] details = new InternalDriverDetails[numVolumes];
            int i = 0;

            foreach(FileSystem fs in this.Emulator.FileSystems)
            {
                details[i].Namespace = Encoding.ASCII.GetBytes(fs.Namespace);
                details[i].SerialNumber = fs.SerialNumber;

                if (fs is ManagedFileSystem)
                {
                    ManagedFileSystem mfs = (ManagedFileSystem)fs;

                    details[i].IsNative = false;
                    details[i].BufferingStrategy = mfs.BufferingStrategy;
                    details[i].InputBuffer = mfs.InputBuffer;
                    details[i].OutputBuffer = mfs.OutputBuffer;
                    details[i].CanRead = mfs.CanRead;
                    details[i].CanWrite = mfs.CanWrite;
                    details[i].CanSeek = mfs.CanSeek;
                    details[i].InputBufferSize = mfs.DefaultInputBufferSize;
                    details[i].OutputBufferSize = mfs.DefaultOutputBufferSize;
                    details[i].ReadTimeout = mfs.ReadTimeout;
                    details[i].WriteTimeout = mfs.WriteTimeout;
                }
                else if (fs is NativeFileSystem)
                {
                    NativeFileSystem nfs = (NativeFileSystem)fs;

                    details[i].IsNative = true;
                    details[i].FileSystemName = nfs.FileSystemName;
                    details[i].BlockStorageDeviceContext = (uint)Emulator.BlockStorageDevices.GetContext(nfs.BlockStorageDevice);
                    details[i].VolumeId = nfs.VolumeId;
                }
                else
                {
                    throw new InvalidOperationException("Invalid file system found: " + fs.ToString());
                }

                i++;
            }

            return details;
        }

        int IFSDriver.Open(uint volumeId, string path, ref uint handle)
        {
            try
            {
                ushort emulatorHandle = GetManagedFileSystem(volumeId).Open(path);
                handle = MakeHandle(volumeId, emulatorHandle);
            }
            catch (Exception e)
            {
                return TranslateException(e);
            }

            return 0;
        }

        int IFSDriver.Close(uint handle)
        {
            try
            {
                uint volumeId;
                ushort emulatorHandle;

                ParseHandle(handle, out volumeId, out emulatorHandle);

                GetManagedFileSystem(volumeId).Close(emulatorHandle);
            }
            catch (Exception e)
            {
                return TranslateException(e);
            }

            return 0;
        }

        int IFSDriver.Read(uint handle, IntPtr buffer, int count, ref int bytesRead)
        {
            try
            {
                uint volumeId;
                ushort emulatorHandle;

                ParseHandle(handle, out volumeId, out emulatorHandle);
                
                byte[] buf = new byte[count];

                bytesRead = GetManagedFileSystem(volumeId).Read(emulatorHandle, buf);

                Marshal.Copy(buf, 0, buffer, bytesRead);
            }
            catch (Exception e)
            {
                return TranslateException(e);
            }

            return 0;
        }

        int IFSDriver.Write(uint handle, IntPtr buffer, int count, ref int bytesWritten)
        {
            try
            {
                uint volumeId;
                ushort emulatorHandle;

                ParseHandle(handle, out volumeId, out emulatorHandle);

                byte[] buf = new byte[count];

                Marshal.Copy(buffer, buf, 0, count);

                bytesWritten = GetManagedFileSystem(volumeId).Write(emulatorHandle, buf);
            }
            catch (Exception e)
            {
                return TranslateException(e);
            }

            return 0;
        }

        int IFSDriver.Flush(uint handle)
        {
            try
            {
                uint volumeId;
                ushort emulatorHandle;

                ParseHandle(handle, out volumeId, out emulatorHandle);

                GetManagedFileSystem(volumeId).Flush(emulatorHandle);
            }
            catch (Exception e)
            {
                return TranslateException(e);
            }

            return 0;
        }

        int IFSDriver.Seek(uint handle, long offset, uint origin, ref long position)
        {
            try
            {
                uint volumeId;
                ushort emulatorHandle;

                ParseHandle(handle, out volumeId, out emulatorHandle);

                position = GetManagedFileSystem(volumeId).Seek(emulatorHandle, offset, (SeekOrigin)origin);
            }
            catch (Exception e)
            {
                return TranslateException(e);
            }

            return 0;
        }

        int IFSDriver.GetLength(uint handle, ref long length)
        {
            try
            {
                uint volumeId;
                ushort emulatorHandle;

                ParseHandle(handle, out volumeId, out emulatorHandle);

                length = GetManagedFileSystem(volumeId).GetLength(emulatorHandle);
            }
            catch (Exception e)
            {
                return TranslateException(e);
            }

            return 0;
        }

        int IFSDriver.SetLength(uint handle, long length)
        {
            try
            {
                uint volumeId;
                ushort emulatorHandle;

                ParseHandle(handle, out volumeId, out emulatorHandle);

                GetManagedFileSystem(volumeId).SetLength(emulatorHandle, length);
            }
            catch (Exception e)
            {
                return TranslateException(e);
            }

            return 0;
        }


        int IFSDriver.FindOpen(uint volumeId, string fileSpec, ref uint searchHandle)
        {
            try
            {
                ushort emulatorHandle = GetManagedFileSystem(volumeId).FindOpen(fileSpec);
                searchHandle = MakeHandle(volumeId, emulatorHandle);
            }
            catch (Exception e)
            {
                return TranslateException(e);
            }

            return 0;
        }

        int IFSDriver.FindNext(uint searchHandle, ref FsFileInfo fileInfo, ref bool found)
        {
            try
            {
                uint volumeId;
                ushort emulatorHandle;

                ParseHandle(searchHandle, out volumeId, out emulatorHandle);

                found = GetManagedFileSystem(volumeId).FindNext(emulatorHandle, out fileInfo);
            }
            catch (Exception e)
            {
                return TranslateException(e);
            }

            return 0;
        }

        int IFSDriver.FindClose(uint searchHandle)
        {
            try
            {
                uint volumeId;
                ushort emulatorHandle;

                ParseHandle(searchHandle, out volumeId, out emulatorHandle);

                GetManagedFileSystem(volumeId).FindClose(emulatorHandle);
            }
            catch (Exception e)
            {
                return TranslateException(e);
            }

            return 0;
        }

        int IFSDriver.GetFileInfo(uint volumeId, string path, ref FsFileInfo fileInfo, ref bool found)
        {
            try
            {
                found = GetManagedFileSystem(volumeId).GetFileInfo(path, out fileInfo);
            }
            catch (Exception e)
            {
                return TranslateException(e);
            }

            return 0;
        }

        int IFSDriver.CreateDirectory(uint volumeId, string path)
        {
            try
            {
                GetManagedFileSystem(volumeId).CreateDirectory(path);
            }
            catch (Exception e)
            {
                return TranslateException(e);
            }

            return 0;
        }

        int IFSDriver.Move(uint volumeId, string source, string dest)
        {
            try
            {
                GetManagedFileSystem(volumeId).Move(source, dest);
            }
            catch (Exception e)
            {
                return TranslateException(e);
            }

            return 0;
        }

        int IFSDriver.Delete(uint volumeId, string path)
        {
            try
            {
                GetManagedFileSystem(volumeId).Delete(path);
            }
            catch (Exception e)
            {
                return TranslateException(e);
            }

            return 0;
        }

        int IFSDriver.GetAttributes(uint volumeId, string path, ref uint attributes)
        {
            try
            {
                attributes = GetManagedFileSystem(volumeId).GetAttributes(path);
            }
            catch (Exception e)
            {
                return TranslateException(e);
            }

            return 0;
        }

        int IFSDriver.SetAttributes(uint volumeId, string path, uint attributes)
        {
            try
            {
                GetManagedFileSystem(volumeId).SetAttributes(path, attributes);
            }
            catch (Exception e)
            {
                return TranslateException(e);
            }

            return 0;
        }

        int IFSDriver.Format(uint volumeId, string volumeLabel, uint parameter)
        {
            try
            {
                GetManagedFileSystem(volumeId).Format(volumeLabel, parameter);
            }
            catch (Exception e)
            {
                return TranslateException(e);
            }

            return 0;
        }

        int IFSDriver.GetSizeInfo(uint volumeId, ref long totalSize, ref long totalFreeSpace)
        {
            try
            {
                GetManagedFileSystem(volumeId).GetSizeInfo(out totalSize, out totalFreeSpace);
            }
            catch (Exception e)
            {
                return TranslateException(e);
            }

            return 0;
        }

        int IFSDriver.FlushAll(uint volumeId)
        {
            try
            {
                GetManagedFileSystem(volumeId).FlushAll();
            }
            catch (Exception e)
            {
                return TranslateException(e);
            }

            return 0;
        }

        int IFSDriver.GetVolumeLabel(uint volumeId, IntPtr volumeName, int volumeNameLen)
        {
            string label = GetManagedFileSystem(volumeId).GetVolumeLabel();

            if (label == null) return 0;

            byte[] data = UTF8Encoding.UTF8.GetBytes(label);

            int len = Math.Min(volumeNameLen, data.Length);

            Marshal.Copy(data, 0, volumeName, len);

            return len;
            
        }

        #endregion
    }

    public class FileSystemCollection : EmulatorComponentCollection
    {
        List<FileSystem> _fileSystems;

        public FileSystemCollection()
            : base(typeof(FileSystem))
        {
            _fileSystems = new List<FileSystem>();
        }

        public FileSystem this[uint volumeId]
        {
            get
            {
                if (volumeId >= _fileSystems.Count)
                {
                    throw new ArgumentException("The specified volumeId, " + volumeId + ", does not exist.");
                }

                return _fileSystems[(int)volumeId];
            }
        }

        public override int Count
        {
            get { return _fileSystems.Count; }
        }

        public override IEnumerator GetEnumerator()
        {
            return _fileSystems.GetEnumerator();
        }

        public override void CopyTo(Array array, int index)
        {
            FileSystem[] fsArray = array as FileSystem[];
            if (fsArray == null)
            {
                throw new ArgumentException("Cannot cast array into FileSystem[]");
            }
            _fileSystems.CopyTo(fsArray, index);
        }

        internal override void RegisterInternal(EmulatorComponent ec)
        {
            // The list of File system cannot change after configuration is completed
            // or the volumeId will get mixed up
            ThrowIfNotSetup();

            FileSystem fs = ec as FileSystem;

            if (fs == null)
            {
                throw new Exception("Attempt to register a non FileSystem with FileSystemCollection.");
            }

            if (Exists(fs.Namespace))
            {
                throw new Exception("An FileSystem with namespace " + fs.Namespace + " is already set.");
            }

            _fileSystems.Add(fs);

            base.RegisterInternal(ec);
        }

        internal override void UnregisterInternal(EmulatorComponent ec)
        {
            // The list of File system cannot change after configuration is completed
            // or the volumeId will get mixed up
            ThrowIfNotSetup();

            FileSystem fs = ec as FileSystem;

            if (fs != null)
            {
                if (_fileSystems.Remove(fs) == false)
                {
                    Debug.Assert(false);
                    return;
                }

                base.UnregisterInternal(ec);
            }
        }

        private bool Exists(String nameSpace)
        {
            return _fileSystems.Exists(delegate(FileSystem fs) { return fs.Namespace == nameSpace; });
        }
    }

    public abstract class FileSystem : EmulatorComponent
    {
        String _namespace;
        String _label;
        uint _serialNumber;

        protected FileSystem()
        {
            _namespace = null;
            _label = String.Empty;
            _serialNumber = 0;
        }

        protected FileSystem(String ns)
            : this()
        {
            _namespace = ns;
        }

        public String Namespace
        {
            get { return _namespace; }
            set
            {
                ThrowIfNotConfigurable();
                _namespace = value;
            }
        }

        public String Label
        {
            get { return _label; }
            set
            {
                ThrowIfNotConfigurable();
                _label = value;
            }
        }

        public uint SerialNumber
        {
            get { return _serialNumber; }
            set
            {
                ThrowIfNotConfigurable();
                _serialNumber = value;
            }
        }

        public override void SetupComponent()
        {
            if (String.IsNullOrEmpty(_namespace))
            {
                throw new Exception("Error component + " + this.ComponentId + ": FileSystem Emulator Component has an invalid namespace.");
            }
            else if (_namespace.Length > FSDriver.FsNameDefaultLength)
            {
                throw new Exception("Error component + " + this.ComponentId + ": The namespace " + _namespace + " exceeds the maximum length allowed for a file system namespace.");
            }

            if (_label != null && _label.Length > FSDriver.FsNameDefaultLength)
            {
                throw new Exception("Error component + " + this.ComponentId + ": The label " + _label + " exceeds the maximum length allowed for a file system namespace.");
            }
        }

        public override bool IsReplaceableBy(EmulatorComponent ec)
        {
            FileSystem fs = ec as FileSystem;

            if (fs != null)
            {
                return fs.Namespace == _namespace;
            }

            return false;
        }
    }

    public class NativeFileSystem : FileSystem
    {
        String _fileSystemName = String.Empty;
        String _blockStorageDeviceComponentId = String.Empty;
        uint _volumeId = 0;

        public String FileSystemName
        {
            get { return _fileSystemName; }
            set
            {
                ThrowIfNotConfigurable();
                _fileSystemName = value;
            }
        }

        public String BlockStorageDeviceComponentId
        {
            get { return _blockStorageDeviceComponentId; }
            set
            {
                ThrowIfNotConfigurable();
                _blockStorageDeviceComponentId = value;
            }
        }

        public uint VolumeId
        {
            get { return _volumeId; }
            set
            {
                ThrowIfNotConfigurable();
                _volumeId = value;
            }
        }

        public BlockStorageDevice BlockStorageDevice
        {
            get { return Emulator.FindComponentById(_blockStorageDeviceComponentId) as BlockStorageDevice; }
        }

        public override void SetupComponent()
        {
            base.SetupComponent();

            if (String.IsNullOrEmpty(_fileSystemName))
            {
                throw new Exception("Error component + " + this.ComponentId + ": NativeFileSystem Emulator Component has invalid file system name.");
            }

            if (this.BlockStorageDevice == null)
            {
                throw new Exception("Error component + " + this.ComponentId + ": NativeFileSystem Emulator Component has invalid BlockStorageDeviceComponentId.");
            }
        }
    }

    public abstract class ManagedFileSystem : FileSystem
    {
        BufferingStrategy _bufferingStrategy;
        byte[] _inputBuffer;
        byte[] _outputBuffer;
        bool _canRead;
        bool _canWrite;
        bool _canSeek;
        int _inputBufferSize;
        int _outputBufferSize;
        int _readTimeout;
        int _writeTimeout;

        protected ManagedFileSystem()
        {
            _bufferingStrategy = BufferingStrategy.SyncIO;

            _inputBuffer = null;
            _outputBuffer = null;

            _canRead = true;
            _canWrite = true;
            _canSeek = false;

            _readTimeout = Timeout.Infinite;
            _writeTimeout = Timeout.Infinite;

            _inputBufferSize = -1;
            _outputBufferSize = -1;
        }

        protected ManagedFileSystem(String ns)
            : base(ns)
        {
        }

        public BufferingStrategy BufferingStrategy
        {
            get { return _bufferingStrategy; }
            set
            {
                ThrowIfNotConfigurable();
                if (value < BufferingStrategy.SyncIO || value > BufferingStrategy.DriverBufferedIO)
                {
                    throw new ArgumentException("Invalid buffering strategy -- " + value.ToString());
                }

                _bufferingStrategy = value;
            }
        }

        public byte[] InputBuffer
        {
            get { return _inputBuffer; }
            set
            {
                ThrowIfNotConfigurable();

                _inputBuffer = value;
            }
        }

        public byte[] OutputBuffer
        {
            get { return _outputBuffer; }
            set
            {
                ThrowIfNotConfigurable();

                _outputBuffer = value;
            }
        }

        public bool CanRead
        {
            get { return _canRead; }
            set
            {
                ThrowIfNotConfigurable();

                _canRead = value;
            }
        }

        public bool CanWrite
        {
            get { return _canWrite; }
            set
            {
                ThrowIfNotConfigurable();

                _canWrite = value;
            }
        }

        public bool CanSeek
        {
            get { return _canSeek; }
            set
            {
                ThrowIfNotConfigurable();

                _canSeek = value;
            }
        }

        public int DefaultInputBufferSize
        {
            get { return _inputBufferSize; }
            set
            {
                ThrowIfNotConfigurable();

                _inputBufferSize = value;
            }
        }

        public int DefaultOutputBufferSize
        {
            get { return _outputBufferSize; }
            set
            {
                ThrowIfNotConfigurable();

                _outputBufferSize = value;
            }
        }

        public int ReadTimeout
        {
            get { return _readTimeout; }
            set
            {
                ThrowIfNotConfigurable();

                _readTimeout = value;
            }
        }

        public int WriteTimeout
        {
            get { return _writeTimeout; }
            set
            {
                ThrowIfNotConfigurable();

                _writeTimeout = value;
            }
        }

        public virtual ushort Open(string path)
        {
            throw new NotImplementedException();
        }

        public virtual void Close(ushort handle)
        {
            throw new NotImplementedException();
        }

        public virtual int Read(ushort handle, byte[] buffer)
        {
            if (_canRead) throw new NotImplementedException();
            throw new NotSupportedException();
        }

        public virtual int Write(ushort handle, byte[] buffer)
        {
            if (_canWrite) throw new NotImplementedException();
            throw new NotSupportedException();
        }

        public virtual void Flush(ushort handle)
        {
            throw new NotImplementedException();
        }

        public virtual long Seek(ushort handle, long offset, SeekOrigin origin)
        {
            if (_canSeek) throw new NotImplementedException();
            throw new NotSupportedException();
        }

        public virtual long GetLength(ushort handle)
        {
            if (_canSeek) throw new NotImplementedException();
            throw new NotSupportedException();
        }

        public virtual void SetLength(ushort handle, long length)
        {
            if (_canSeek && _canWrite) throw new NotImplementedException();
            throw new NotSupportedException();
        }

        public virtual ushort FindOpen(string fileSpec)
        {
            throw new NotImplementedException();
        }

        public virtual bool FindNext(ushort handle, out FsFileInfo fileInfo)
        {
            throw new NotImplementedException();
        }

        public virtual void FindClose(ushort handle)
        {
            throw new NotImplementedException();
        }

        public virtual bool GetFileInfo(string path, out FsFileInfo fileInfo)
        {
            throw new NotImplementedException();
        }

        public virtual void CreateDirectory(string path)
        {
            throw new NotImplementedException();
        }

        public virtual void Move(string source, string dest)
        {
            throw new NotImplementedException();
        }

        public virtual void Delete(string path)
        {
            throw new NotImplementedException();
        }

        public virtual uint GetAttributes(string path)
        {
            throw new NotImplementedException();
        }

        public virtual void SetAttributes(string path, uint attributes)
        {
            throw new NotImplementedException();
        }

        public virtual void Format(string volumeLabel, uint parameter)
        {
            throw new NotImplementedException();
        }

        public virtual void GetSizeInfo(out long totalSize, out long totalFreeSpace)
        {
            throw new NotImplementedException();
        }

        public virtual void FlushAll()
        {
            throw new NotImplementedException();
        }

        public virtual string GetVolumeLabel()
        {
            throw new NotImplementedException();
        }

        public override void SetupComponent()
        {
            base.SetupComponent();

            if (_bufferingStrategy != BufferingStrategy.DriverBufferedIO)
            {
                if (_inputBuffer != null || _outputBuffer != null)
                {
                    throw new Exception("Error component + " + this.ComponentId + ": InputBuffer and OutputBuffer can only be specified when BufferingStrategy is DriverBufferedIO.");
                }
            }

            if (_bufferingStrategy == BufferingStrategy.DriverBufferedIO)
            {
                if (_inputBuffer == null && _canRead)
                {
                    throw new Exception("Error component + " + this.ComponentId + ": Please set up an InputBuffer since BufferingStrategy is DriverBufferedIO.");
                }
                if (_outputBuffer == null && _canWrite)
                {
                    throw new Exception("Error component + " + this.ComponentId + ": Please set up an OutputBuffer since BufferingStrategy is DriverBufferedIO.");
                }
            }

            if (_bufferingStrategy == BufferingStrategy.SystemBufferedIO)
            {
                if (_inputBufferSize <= 0 || _outputBufferSize <= 0)
                {
                    throw new Exception("Error component + " + this.ComponentId + ": Please specify valid default input and output buffer sizes when BufferingStrategy is SystemBufferedIO.");
                }
            }

        }
    }

    /// <summary>
    /// Emulators Windows File System. WinFSDriver wraps desktop file system
    /// and makes it available for MF code running in emulator. Any desktop file
    /// in emulation folder will appear as a file in from MF code. This is a strictly
    /// emulator only file system, not available in devices.
    /// </summary>
    public class WindowsFileSystem : ManagedFileSystem
    {
        private const string c_DOTNETMF_FS_ROOT = @"DOTNETMF_FS_EMULATION";

        String _root;
        Dictionary<ushort, FileStream> _openedFiles;
        Dictionary<ushort, SearchData> _openedSearches;
        long _totalSize = 0;

        private class SearchData
        {
            private string[] _files = null;
            private string[] _directories = null;
            private int _dirIndex = 0;
            private int _fileIndex = 0;

            public SearchData(string path)
            {
                _files = Directory.GetFiles(path);
                _directories = Directory.GetDirectories(path);
            }

            public bool GetNext(out FsFileInfo fileInfo)
            {
                if ((_directories != null) && (_dirIndex < _directories.Length))
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(_directories[_dirIndex]);

                    fileInfo.Attributes = WindowsFileSystem.ResolveAttributes(dirInfo.Attributes);
                    fileInfo.CreationTime = dirInfo.CreationTime.ToFileTime();
                    fileInfo.LastAccessTime = dirInfo.LastAccessTime.ToFileTime();
                    fileInfo.LastWriteTime = dirInfo.LastWriteTime.ToFileTime();
                    fileInfo.Size = 0;
                    fileInfo.FileName = dirInfo.Name;

                    _dirIndex++;

                    return true;
                }

                if ((_files != null) && (_fileIndex < _files.Length))
                {
                    FileInfo fi = new FileInfo(_files[_fileIndex]);

                    if(fi.Name == "_vol_")
                    {
                        _fileIndex++;

                        if(_fileIndex < _files.Length)
                        {
                            fi = new FileInfo(_files[_fileIndex]);
                        }
                        else
                        {
                            fi = null;
                        }
                    }

                    if(fi != null)
                    {
                        fileInfo.Attributes = WindowsFileSystem.ResolveAttributes(fi.Attributes);
                        fileInfo.CreationTime = fi.CreationTime.ToFileTime();
                        fileInfo.LastAccessTime = fi.LastAccessTime.ToFileTime();
                        fileInfo.LastWriteTime = fi.LastWriteTime.ToFileTime();
                        fileInfo.Size = fi.Length;
                        fileInfo.FileName = fi.Name;

                        _fileIndex++;

                        return true;
                    }
                }

                fileInfo.Attributes = 0;
                fileInfo.CreationTime = 0;
                fileInfo.LastAccessTime = 0;
                fileInfo.LastWriteTime = 0;
                fileInfo.Size = 0;
                fileInfo.FileName = "";

                return false;
            }
        }

        public WindowsFileSystem()
        {
            _openedFiles = new Dictionary<ushort, FileStream>();
            _openedSearches = new Dictionary<ushort, SearchData>();
        }

        public WindowsFileSystem(String ns)
            : base(ns)
        {
        }

        public override void InitializeComponent()
        {
            _root = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + c_DOTNETMF_FS_ROOT + Path.DirectorySeparatorChar + Namespace;
            
            // Create the directory if it doesn't exist
            Directory.CreateDirectory(_root);

            if (_totalSize - GetDirectorySize(_root) < 0)
            {
                throw new Exception("Error component + " + this.ComponentId + ": TotalSize is smaller than the size of the existing directory.");
            }

            base.InitializeComponent();
        }

        public override void UninitializeComponent()
        {
            foreach (KeyValuePair<ushort, FileStream> openFile in _openedFiles)
            {
                openFile.Value.Close();
            }

            base.UninitializeComponent();
        }

        public override void SetupComponent()
        {
            base.SetupComponent();

            if (_totalSize <= 0)
            {
                throw new Exception("Error component + " + this.ComponentId + ": TotalSize has to be greater than 0");
            }
        }

        public long TotalSize
        {
            get { return _totalSize; }
            set
            {
                ThrowIfNotConfigurable();
                _totalSize = value;
            }
        }

        private long GetDirectorySize(String path)
        {
            long size = 0;
            DirectoryInfo currentDir = new DirectoryInfo(path);
            foreach(DirectoryInfo dir in currentDir.GetDirectories())
            {
                size += GetDirectorySize(dir.FullName); 
            }

            foreach (FileInfo file in currentDir.GetFiles())
            {
                size += file.Length;
            }

            return size;
        }

        public override ushort Open(string path)
        {
            String fullPath = ConvertMFPathToPath(path);

            FileStream fs = new FileStream(fullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

            ushort handle = (ushort)fs.GetHashCode();

            while (_openedFiles.ContainsKey(handle)) handle++;

            _openedFiles.Add(handle, fs);

            return handle;
        }

        public override void Close(ushort handle)
        {
            FileStream fs = _openedFiles[handle];

            fs.Close();

            _openedFiles.Remove(handle);
        }

        public override int Read(ushort handle, byte[] buffer)
        {
            if (!this.CanRead) throw new NotSupportedException();

            FileStream fs = _openedFiles[handle];

            return fs.Read(buffer, 0, buffer.Length);
        }

        public override int Write(ushort handle, byte[] buffer)
        {
            if (!this.CanWrite) throw new NotSupportedException();

            FileStream fs = _openedFiles[handle];

            fs.Write(buffer, 0, buffer.Length);

            fs.Flush();
            return buffer.Length;
        }

        public override void Flush(ushort handle)
        {
            FileStream fs = _openedFiles[handle];

            fs.Flush();
        }

        public override long Seek(ushort handle, long offset, SeekOrigin origin)
        {
            if (!this.CanSeek) throw new NotSupportedException();

            FileStream fs = _openedFiles[handle];

            return fs.Seek(offset, origin);
        }

        public override long GetLength(ushort handle)
        {
            if (!this.CanSeek) throw new NotSupportedException();

            FileStream fs = _openedFiles[handle];

            return fs.Length;
        }

        public override void SetLength(ushort handle, long length)
        {
            if (!this.CanSeek || !this.CanWrite) throw new NotSupportedException();

            FileStream fs = _openedFiles[handle];

            fs.SetLength(length);
        }

        public override ushort FindOpen(string path)
        {
            SearchData search = new SearchData(ConvertMFPathToPath(path));

            ushort handle = (ushort)search.GetHashCode();

            while (_openedSearches.ContainsKey(handle)) handle++;

            _openedSearches.Add(handle, search);

            return handle;
        }

        public override bool FindNext(ushort handle, out FsFileInfo fileInfo)
        {
            SearchData search = _openedSearches[handle];

            return search.GetNext(out fileInfo);
        }

        public override void FindClose(ushort handle)
        {
            _openedSearches.Remove(handle);
        }

        public override bool GetFileInfo(string path, out FsFileInfo fileInfo)
        {
            String convertedPath = ConvertMFPathToPath(path);
            bool isDirectory = Directory.Exists(convertedPath);
            bool isFile = File.Exists(convertedPath);

            if(!isDirectory && !isFile)
            {
                fileInfo.Attributes = 0;
                fileInfo.CreationTime = 0;
                fileInfo.LastAccessTime = 0;
                fileInfo.LastWriteTime = 0;
                fileInfo.Size = 0;
                fileInfo.FileName = "";

                return false;
            }

            FileSystemInfo info = (isDirectory) ? (FileSystemInfo)new DirectoryInfo(convertedPath) : (FileSystemInfo)new FileInfo(convertedPath);

            fileInfo.Attributes = ResolveAttributes(info.Attributes);
            fileInfo.CreationTime = info.CreationTime.ToFileTime();
            fileInfo.LastAccessTime = info.LastAccessTime.ToFileTime();
            fileInfo.LastWriteTime = info.LastWriteTime.ToFileTime();
            fileInfo.Size = (isDirectory) ? 0 : ((FileInfo)info).Length;
            fileInfo.FileName = null; // no need to set the filename, as the caller already have it.

            return true;
        }

        public override void CreateDirectory(string path)
        {
            Directory.CreateDirectory(ConvertMFPathToPath(path));
        }

        public override void Move(string source, string dest)
        {
            String src = ConvertMFPathToPath(source);
            String dst = ConvertMFPathToPath(dest);

            if (Directory.Exists(src))
            {
                Directory.Move(src, dst);
            }
            else
            {
                File.Move(src, dst);
            }
        }

        public override void Delete(string path)
        {
            String p = ConvertMFPathToPath(path);

            if (Directory.Exists(p))
            {
                Directory.Delete(p, true);
            }
            else
            {
                File.Delete(p);
            }
        }

        public override uint GetAttributes(string path)
        {
            String p = ConvertMFPathToPath(path);

            if (!File.Exists(p) && !Directory.Exists(p))
                return 0xFFFFFFFF;

            return ResolveAttributes(File.GetAttributes(p));
        }

        public override void SetAttributes(string path, uint attributes)
        {
            File.SetAttributes(ConvertMFPathToPath(path), (FileAttributes)attributes);
        }

        public override void Format(string volumeName, uint parameter)
        {
            /// By formatting we mean wipe out emulated folder
            /// content. Under certain circumstances this may prove to be
            /// tricky. For example files may have turned to read-only (curable)
            /// or files hold onto by MF process (curable), files hold on
            /// to by other Windows process (beyond our scope), file's
            /// ACL changed (beyond our scope). Anyway, we have our custom
            /// recursive code that tries to mitigate as much as possible.
            DirectoryDelete(_root);
            Directory.CreateDirectory(_root);

            if(!string.IsNullOrEmpty(volumeName))
            {
                using(FileStream fs = File.Create(_root + "\\_vol_", 128))
                {
                    byte[] bytes = UTF8Encoding.UTF8.GetBytes(volumeName);
                    fs.Write( bytes, 0, bytes.Length );
                }
            }
        }

        public override void GetSizeInfo(out long totalSize, out long totalFreeSpace)
        {
            totalSize = _totalSize;
            totalFreeSpace = _totalSize - GetDirectorySize(_root);

            if (totalFreeSpace < 0)
            {
                totalFreeSpace = 0;
            }
        }

        public override void FlushAll()
        {
        }

        public override string GetVolumeLabel()
        {
            string volPath = _root + "\\_vol_";
            string volName = null;
            
            if(File.Exists(volPath))
            {
                using(FileStream fs = File.Open(volPath, FileMode.Open))
                {
                    byte[] bytes = new byte[fs.Length];

                    fs.Read(bytes, 0, (int)fs.Length);

                    volName = UTF8Encoding.UTF8.GetString(bytes);
                }
            }

            return volName;
        }

        private void DirectoryDelete(string folderPath)
        {
            string[] files = Directory.GetFiles(folderPath);
            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            string[] dirs = Directory.GetDirectories(folderPath);
            foreach (string dir in dirs)
            {
                DirectoryDelete(dir);
            }

            Directory.Delete(folderPath);
        }

        private String ConvertMFPathToPath(String mfPath)
        {
            return Path.Combine(_root, mfPath.Substring(1));
        }

        private static uint ResolveAttributes(FileAttributes fileAttributes)
        {
            // We preserve only the attributes that we support
            return (uint)fileAttributes & (uint)(FileAttributes.Directory | FileAttributes.Archive |
                FileAttributes.Hidden | FileAttributes.Normal | FileAttributes.ReadOnly | FileAttributes.System);
        }
    }

    public class AsyncWindowsFileSystem : WindowsFileSystem
    {
        int _delay;
        bool _isDataAvailable;
        Timer _timer;
        
        delegate void SetDataAvailableDelegate();

        public AsyncWindowsFileSystem() : base()
        {
            _isDataAvailable = false;
            _timer = new Timer(delegate(Object o) { this.Invoke(new SetDataAvailableDelegate(SetDataAvailable)); });
            _delay = 1000;
                
        }

        public int Delay
        {
            get { return _delay; }
            set
            {
                ThrowIfNotConfigurable();

                _delay = value;
            }
        }

        public override int Read(ushort handle, byte[] buffer)
        {
            if (_isDataAvailable)
            {
                _isDataAvailable = false;

                return base.Read(handle, buffer);
            }
            else
            {
                _timer.Change(_delay, Timeout.Infinite);

                return 0;
            }
        }

        public override int Write(ushort handle, byte[] buffer)
        {
            if (_isDataAvailable)
            {
                _isDataAvailable = false;

                return base.Write(handle, buffer);
            }
            else
            {
                _timer.Change(_delay, Timeout.Infinite);

                return 0;
            }
        }

        private void SetDataAvailable()
        {
            _isDataAvailable = true;
            this.Emulator.SetSystemEvents(Microsoft.SPOT.Emulator.Events.SystemEvents.IO);
        }
    }
}

