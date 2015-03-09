////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT.IO;

namespace System.IO
{
    public abstract class FileSystemInfo : MarshalByRefObject
    {
        protected String m_fullPath;  // fully qualified path of the directory

        //--//

        public virtual String FullName
        {
            get
            {
                return m_fullPath;
            }
        }

        public String Extension
        {
            get
            {
                return Path.GetExtension(FullName);
            }
        }

        public abstract String Name
        {
            get;
        }

        public abstract bool Exists
        {
            get;
        }

        public abstract void Delete();

        public FileAttributes Attributes
        {
            get
            {
                RefreshIfNull();
                return (FileAttributes)_nativeFileInfo.Attributes;
            }
        }

        public DateTime CreationTime
        {
            get
            {
                return CreationTimeUtc.ToLocalTime();
            }
        }

        public DateTime CreationTimeUtc
        {
            get
            {
                RefreshIfNull();
                return new DateTime(_nativeFileInfo.CreationTime);
            }
        }

        public DateTime LastAccessTime
        {
            get
            {
                return LastAccessTimeUtc.ToLocalTime();
            }
        }

        public DateTime LastAccessTimeUtc
        {
            get
            {
                RefreshIfNull();
                return new DateTime(_nativeFileInfo.LastAccessTime);
            }
        }

        public DateTime LastWriteTime
        {
            get
            {
                return LastWriteTimeUtc.ToLocalTime();
            }
        }

        public DateTime LastWriteTimeUtc
        {
            get
            {
                RefreshIfNull();
                return new DateTime(_nativeFileInfo.LastWriteTime);
            }
        }

        public void Refresh()
        {
            Object record = FileSystemManager.AddToOpenListForRead(m_fullPath);

            try
            {
                _nativeFileInfo = NativeFindFile.GetFileInfo(m_fullPath);

                if (_nativeFileInfo == null)
                {
                    IOException.IOExceptionErrorCode errorCode = (this is FileInfo) ? IOException.IOExceptionErrorCode.FileNotFound : IOException.IOExceptionErrorCode.DirectoryNotFound;
                    throw new IOException("", (int)errorCode);
                }
            }
            finally
            {
                FileSystemManager.RemoveFromOpenList(record);
            }
        }

        protected void RefreshIfNull()
        {
            if (_nativeFileInfo == null)
            {
                Refresh();
            }
        }

        internal NativeFileInfo _nativeFileInfo;
    }
}


