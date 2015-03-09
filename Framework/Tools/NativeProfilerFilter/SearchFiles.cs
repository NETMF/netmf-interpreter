using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Runtime;

namespace NativeProfilerFilter
{
    public class SearchFiles
    {
        public string searchPattern = "*.*";

        public delegate void FileEventHandler(object sender, FileInfo e);
        public event FileEventHandler FileEvent;

        public delegate void DirectoryEventHandler(object sender, DirectoryInfo e);
        public event DirectoryEventHandler DirectoryEvent;

        public void SearchDirectory(string path, bool searchSubDirs)
        {
            if (path != null && path.Length != 0)
            {
                this.SearchDirectories(new DirectoryInfo(path), searchSubDirs);
            }
        }

        public void SearchDirectory(DirectoryInfo directory, bool searchSubDirs)
        {
            this.SearchDirectories(directory, searchSubDirs);
        }

        private void SearchDirectories(DirectoryInfo directoryInfo, bool searchSubDirs)
        {
            if (this.FileEvent != null)
            {
                ProcessFilesInDirectory(directoryInfo);
            }

            if (searchSubDirs)
            {
                DirectoryInfo[] subDirectoriesInfo = directoryInfo.GetDirectories();

                foreach (DirectoryInfo subDirectoryInfo in subDirectoriesInfo)
                {
                    if ((subDirectoryInfo.Attributes & FileAttributes.ReparsePoint) != 0)
                    {
                        continue;
                    }

                    this.SearchDirectory(subDirectoryInfo, searchSubDirs);

                }
            }
            if (this.DirectoryEvent != null)
            {
                DirectoryEvent(this, directoryInfo);
            }
        }

        private void ProcessFilesInDirectory(DirectoryInfo directoryInfo)
        {
            foreach (FileInfo fileInfo in directoryInfo.GetFiles(searchPattern))
            {
                FileEvent(this, fileInfo);
            }
        }
    }
}
