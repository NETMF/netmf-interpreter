////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;

using NativeIO = Microsoft.SPOT.IO.NativeIO;

namespace System.IO
{
    public sealed class DirectoryInfo : FileSystemInfo
    {
        private DirectoryInfo()
        {
        }

        public DirectoryInfo(string path)
        {
            // path validation in Path.GetFullPath()

            m_fullPath = Path.GetFullPath(path);
        }

        public override string Name
        {
            get
            {
                return Path.GetFileName(m_fullPath);
            }
        }

        public DirectoryInfo Parent
        {
            get
            {
                string parentDirPath = Path.GetDirectoryName(m_fullPath);
                if (parentDirPath == null)
                    return null;

                return new DirectoryInfo(parentDirPath);
            }
        }

        public DirectoryInfo CreateSubdirectory(string path)
        {
            // path validatation in Path.Combine()

            string subDirPath = Path.Combine(m_fullPath, path);

            /// This will also ensure "path" is valid.
            subDirPath = Path.GetFullPath(subDirPath);

            return Directory.CreateDirectory(subDirPath);
        }

        public void Create()
        {
            Directory.CreateDirectory(m_fullPath);
        }

        public override bool Exists
        {
            get
            {
                return Directory.Exists(m_fullPath);
            }
        }

        public FileInfo[] GetFiles()
        {
            string[] fileNames = Directory.GetFiles(m_fullPath);

            FileInfo[] files = new FileInfo[fileNames.Length];

            for (int i = 0; i < fileNames.Length; i++)
            {
                files[i] = new FileInfo(fileNames[i]);
            }

            return files;
        }

        public DirectoryInfo[] GetDirectories()
        {
            // searchPattern validation in Directory.GetDirectories()

            string[] dirNames = Directory.GetDirectories(m_fullPath);

            DirectoryInfo[] dirs = new DirectoryInfo[dirNames.Length];

            for (int i = 0; i < dirNames.Length; i++)
            {
                dirs[i] = new DirectoryInfo(dirNames[i]);
            }

            return dirs;
        }

        public DirectoryInfo Root
        {
            get
            {
                return new DirectoryInfo(Path.GetPathRoot(m_fullPath));
            }
        }

        public void MoveTo(string destDirName)
        {
            // destDirName validation in Directory.Move()

            Directory.Move(m_fullPath, destDirName);
        }

        public override void Delete()
        {
            Directory.Delete(m_fullPath);
        }

        public void Delete(bool recursive)
        {
            Directory.Delete(m_fullPath, recursive);
        }

        public override string ToString()
        {
            return m_fullPath;
        }
    }
}


