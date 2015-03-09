////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections;
using Microsoft.SPOT.IO;
using NativeIO = Microsoft.SPOT.IO.NativeIO;

namespace System.IO
{
    public enum FileEnumFlags
    {
        Files = 0x0001,
        Directories = 0x0002,
        FilesAndDirectories = Files | Directories,
    }

    public class FileEnum : IEnumerator, IDisposable
    {
        private NativeFindFile  m_findFile;
        private NativeFileInfo  m_currentFile;
        private FileEnumFlags   m_flags;
        private string          m_path;
        private bool            m_disposed;
        private object          m_openForReadHandle;

        public FileEnum(string path, FileEnumFlags flags)
        {
            m_flags = flags;
            m_path  = path;

            m_openForReadHandle = FileSystemManager.AddToOpenListForRead(m_path);
            m_findFile          = new NativeFindFile(m_path, "*");
        }

        #region IEnumerator Members

        public object Current
        {
            get
            {
                if (m_disposed) throw new ObjectDisposedException();

                return m_currentFile.FileName;
            }
        }

        public bool MoveNext()
        {
            if (m_disposed) throw new ObjectDisposedException();

            NativeFileInfo fileinfo = m_findFile.GetNext();

            while (fileinfo != null)
            {
                if (m_flags != FileEnumFlags.FilesAndDirectories)
                {
                    uint targetAttribute = (0 != (m_flags & FileEnumFlags.Directories) ? (uint)FileAttributes.Directory : 0);

                    if ((fileinfo.Attributes & (uint)FileAttributes.Directory) == targetAttribute)
                    {
                        m_currentFile = fileinfo;
                        break;
                    }
                }
                else
                {
                    m_currentFile = fileinfo;
                    break;
                }

                fileinfo = m_findFile.GetNext();
            }

            if (fileinfo == null)
            {
                m_findFile.Close();
                m_findFile = null;

                FileSystemManager.RemoveFromOpenList(m_openForReadHandle);
                m_openForReadHandle = null;
            }

            return fileinfo != null;
        }

        public void Reset()
        {
            if (m_disposed) throw new ObjectDisposedException();

            if (m_findFile != null)
            {
                m_findFile.Close();
            }

            if(m_openForReadHandle == null)
            {
                m_openForReadHandle = FileSystemManager.AddToOpenListForRead(m_path);
            }

            m_findFile = new NativeFindFile(m_path, "*");
        }

        #endregion

        protected virtual void Dispose(bool disposing)
        {
            if (m_findFile != null)
            {
                m_findFile.Close();
                m_findFile = null;
            }

            if (m_openForReadHandle != null)
            {
                FileSystemManager.RemoveFromOpenList(m_openForReadHandle);
                m_openForReadHandle = null;
            }

            m_disposed = true;
        }

        ~FileEnum()
        {
            Dispose(false);
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }

    public class FileEnumerator : IEnumerable
    {
        private string m_path;
        private FileEnumFlags m_flags;

        public FileEnumerator(string path, FileEnumFlags flags)
        {
            m_path  = Path.GetFullPath(path);
            m_flags = flags;

            if (!Directory.Exists(m_path)) throw new IOException("", (int)IOException.IOExceptionErrorCode.DirectoryNotFound);
        }

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return new FileEnum(m_path, m_flags);
        }

        #endregion
    }
}