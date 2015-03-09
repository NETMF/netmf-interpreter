////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;

using NativeIO = Microsoft.SPOT.IO.NativeIO;
using Microsoft.SPOT.IO;

namespace System.IO
{
    [Serializable]
    public sealed class FileInfo : FileSystemInfo
    {
        public FileInfo(String fileName)
        {
            // path validation in Path.GetFullPath()

            m_fullPath = Path.GetFullPath(fileName);
        }

        public override String Name
        {
            get
            {
                return Path.GetFileName(m_fullPath);
            }
        }

        public long Length
        {
            get
            {
                RefreshIfNull();
                return (long)_nativeFileInfo.Size;
            }
        }

        public String DirectoryName
        {
            get
            {
                return Path.GetDirectoryName(m_fullPath);
            }
        }

        public DirectoryInfo Directory
        {
            get
            {
                String dirName = DirectoryName;

                if (dirName == null)
                {
                    return null;
                }

                return new DirectoryInfo(dirName);
            }
        }

        public FileStream Create()
        {
            return File.Create(m_fullPath);
        }

        public override void Delete()
        {
            File.Delete(m_fullPath);
        }

        public override bool Exists
        {
            get
            {
                return File.Exists(m_fullPath);
            }
        }

        public override String ToString()
        {
            return m_fullPath;
        }
    }
}


