////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Threading;
using Microsoft.SPOT.IO;

namespace System.IO
{

    public class FileStream : Stream
    {
        // Driver data

        private NativeFileStream _nativeFileStream;
        private FileSystemManager.FileRecord _fileRecord;
        private String _fileName;
        private bool _canRead;
        private bool _canWrite;
        private bool _canSeek;

        private long _seekLimit;

        private bool _disposed;

        //--//

        public FileStream(String path, FileMode mode)
            : this(path, mode, (mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite), FileShare.Read, NativeFileStream.BufferSizeDefault)
        {
        }

        public FileStream(String path, FileMode mode, FileAccess access)
            : this(path, mode, access, FileShare.Read, NativeFileStream.BufferSizeDefault)
        {
        }

        public FileStream(String path, FileMode mode, FileAccess access, FileShare share)
            : this(path, mode, access, share, NativeFileStream.BufferSizeDefault)
        {
        }

        public FileStream(String path, FileMode mode, FileAccess access, FileShare share, int bufferSize)
        {
            // This will perform validation on path
            _fileName = Path.GetFullPath(path);

            // make sure mode, access, and share are within range
            if (mode < FileMode.CreateNew || mode > FileMode.Append ||
                access < FileAccess.Read || access > FileAccess.ReadWrite ||
                share < FileShare.None || share > FileShare.ReadWrite)
            {
                throw new ArgumentOutOfRangeException();
            }

            // Get wantsRead and wantsWrite from access, note that they cannot both be false
            bool wantsRead = (access & FileAccess.Read) == FileAccess.Read;
            bool wantsWrite = (access & FileAccess.Write) == FileAccess.Write;

            // You can't open for readonly access (wantsWrite == false) when
            // mode is CreateNew, Create, Truncate or Append (when it's not Open or OpenOrCreate)
            if (mode != FileMode.Open && mode != FileMode.OpenOrCreate && !wantsWrite)
            {
                throw new ArgumentException();
            }

            // We need to register the share information prior to the actual file open call (the NativeFileStream ctor)
            // so subsequent file operation on the same file will behave correctly
            _fileRecord = FileSystemManager.AddToOpenList(_fileName, (int)access, (int)share);

            try
            {
                uint attributes = NativeIO.GetAttributes(_fileName);
                bool exists = (attributes != 0xFFFFFFFF);
                bool isReadOnly = (exists) ? (((FileAttributes)attributes) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly : false;

                // If the path specified is an existing directory, fail
                if (exists && ((((FileAttributes)attributes) & FileAttributes.Directory) == FileAttributes.Directory))
                {
                    throw new IOException("", (int)IOException.IOExceptionErrorCode.UnauthorizedAccess);
                }

                // The seek limit is 0 (the beginning of the file) for all modes except Append
                _seekLimit = 0;

                switch (mode)
                {
                    case FileMode.CreateNew: // if the file exists, IOException is thrown
                        if (exists) throw new IOException("", (int)IOException.IOExceptionErrorCode.PathAlreadyExists);
                        _nativeFileStream = new NativeFileStream(_fileName, bufferSize);
                        break;

                    case FileMode.Create: // if the file exists, it should be overwritten
                        _nativeFileStream = new NativeFileStream(_fileName, bufferSize);
                        if (exists) _nativeFileStream.SetLength(0);
                        break;

                    case FileMode.Open: // if the file does not exist, IOException/FileNotFound is thrown
                        if (!exists) throw new IOException("", (int)IOException.IOExceptionErrorCode.FileNotFound);
                        _nativeFileStream = new NativeFileStream(_fileName, bufferSize);
                        break;

                    case FileMode.OpenOrCreate: // if the file does not exist, it is created
                        _nativeFileStream = new NativeFileStream(_fileName, bufferSize);
                        break;

                    case FileMode.Truncate: // the file would be overwritten. if the file does not exist, IOException/FileNotFound is thrown
                        if (!exists) throw new IOException("", (int)IOException.IOExceptionErrorCode.FileNotFound);
                        _nativeFileStream = new NativeFileStream(_fileName, bufferSize);
                        _nativeFileStream.SetLength(0);
                        break;

                    case FileMode.Append: // Opens the file if it exists and seeks to the end of the file. Append can only be used in conjunction with FileAccess.Write
                        // Attempting to seek to a position before the end of the file will throw an IOException and any attempt to read fails and throws an NotSupportedException
                        if (access != FileAccess.Write) throw new ArgumentException();
                        _nativeFileStream = new NativeFileStream(_fileName, bufferSize);
                        _seekLimit = _nativeFileStream.Seek(0, (uint)SeekOrigin.End);
                        break;

                    // We've already checked the mode value previously, so no need for default
                    //default:
                    //    throw new ArgumentOutOfRangeException();
                }

                // Now that we have a valid NativeFileStream, we add it to the FileRecord, so it could gets clean up
                // in case an eject or force format
                _fileRecord.NativeFileStream = _nativeFileStream;

                // Retrive the filesystem capabilities
                _nativeFileStream.GetStreamProperties(out _canRead, out _canWrite, out _canSeek);

                // If the file is readonly, regardless of the filesystem capability, we'll turn off write
                if (isReadOnly)
                {
                    _canWrite = false;
                }

                // Make sure the requests (wantsRead / wantsWrite) matches the filesystem capabilities (canRead / canWrite)
                if ((wantsRead && !_canRead) || (wantsWrite && !_canWrite))
                {
                    throw new IOException("", (int)IOException.IOExceptionErrorCode.UnauthorizedAccess);
                }

                // finally, adjust the _canRead / _canWrite to match the requests
                if (!wantsWrite)
                {
                    _canWrite = false;
                }
                else if (!wantsRead)
                {
                    _canRead = false;
                }
            }
            catch
            {
                // something went wrong, clean up and re-throw the exception
                if (_nativeFileStream != null)
                {
                    _nativeFileStream.Close();
                }

                FileSystemManager.RemoveFromOpenList(_fileRecord);

                throw;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        _canRead = false;
                        _canWrite = false;
                        _canSeek = false;
                    }

                    if (_nativeFileStream != null)
                    {
                        _nativeFileStream.Close();
                    }
                }
                finally
                {
                    if (_fileRecord != null)
                    {
                        FileSystemManager.RemoveFromOpenList(_fileRecord);
                        _fileRecord = null;
                    }

                    _nativeFileStream = null;
                    _disposed = true;
                }
            }
        }

        ~FileStream()
        {
            Dispose(false);
        }

        // This is for internal use to support proper atomic CopyAndDelete
        internal void DisposeAndDelete()
        {
            _nativeFileStream.Close();
            _nativeFileStream = null; // so Dispose(true) won't close the stream again
            NativeIO.Delete(_fileName);

            Dispose(true);
        }

        public override void Flush()
        {
            if (_disposed) throw new ObjectDisposedException();
            _nativeFileStream.Flush();
        }

        public override void SetLength(long value)
        {
            if (_disposed) throw new ObjectDisposedException();
            if (!_canWrite || !_canSeek) throw new NotSupportedException();

            // argument validation in interop layer
            _nativeFileStream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_disposed) throw new ObjectDisposedException();
            if (!_canRead) throw new NotSupportedException();

            lock (_nativeFileStream)
            {
                // argument validation in interop layer
                return _nativeFileStream.Read(buffer, offset, count, NativeFileStream.TimeoutDefault);
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (_disposed) throw new ObjectDisposedException();
            if (!_canSeek) throw new NotSupportedException();

            long oldPosition = this.Position;
            long newPosition = _nativeFileStream.Seek(offset, (uint)origin);

            if (newPosition < _seekLimit)
            {
                this.Position = oldPosition;
                throw new IOException();
            }

            return newPosition;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_disposed) throw new ObjectDisposedException();
            if (!_canWrite) throw new NotSupportedException();

            // argument validation in interop layer
            int bytesWritten;

            lock (_nativeFileStream)
            {
                // we check for count being != 0 because we want to handle negative cases
                // as well in the interop layer
                while (count != 0)
                {
                    bytesWritten = _nativeFileStream.Write(buffer, offset, count, NativeFileStream.TimeoutDefault);

                    if (bytesWritten == 0) throw new IOException();

                    offset += bytesWritten;
                    count -= bytesWritten;
                }
            }
        }

        public override bool CanRead
        {
            get { return _canRead; }
        }

        public override bool CanWrite
        {
            get { return _canWrite; }
        }

        public override bool CanSeek
        {
            get { return _canSeek; }
        }

        public virtual bool IsAsync
        {
            get { return false; }
        }

        public override long Length
        {
            get
            {
                if (_disposed) throw new ObjectDisposedException();
                if (!_canSeek) throw new NotSupportedException();

                return _nativeFileStream.GetLength();
            }
        }

        public String Name
        {
            get { return _fileName; }
        }

        public override long Position
        {
            get
            {
                if (_disposed) throw new ObjectDisposedException();
                if (!_canSeek) throw new NotSupportedException();

                // argument validation in interop layer
                return _nativeFileStream.Seek(0, (uint)SeekOrigin.Current);
            }

            set
            {
                if (_disposed) throw new ObjectDisposedException();
                if (!_canSeek) throw new NotSupportedException();
                if (value < _seekLimit) throw new IOException();

                // argument validation in interop layer
                _nativeFileStream.Seek(value, (uint)SeekOrigin.Begin);
            }
        }
    }
}


