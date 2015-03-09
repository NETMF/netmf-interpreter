using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Microsoft.SPOT.IO;

namespace System.IO
{
    internal class FileSystemManager
    {
        // KEEP IN-SYNC WITH FileAccess.cs and FileShare.cs
        private const int FileAccessRead = 1;
        private const int FileAccessWrite = 2;
        private const int FileAccessReadWrite = 3;

        private const int FileShareNone = 0;
        private const int FileShareRead = 1;
        private const int FileShareWrite = 2;
        private const int FileShareReadWrite = 3;

        //--//

        internal class FileRecord
        {
            public String FullName;
            public NativeFileStream NativeFileStream;
            public int Share;

            public FileRecord(String fullName, int share)
            {
                FullName = fullName;
                Share = share;
            }
        }

        //

        private static ArrayList m_openFiles = new ArrayList();
        private static ArrayList m_lockedDirs = new ArrayList();

        //--//

        public static Object AddToOpenList(String fullName)
        {
            return AddToOpenList(fullName, FileAccessReadWrite, FileShareNone);
        }

        public static Object AddToOpenListForRead(String fullName)
        {
            return AddToOpenList(fullName, FileAccessRead, FileShareReadWrite);
        }

        public static FileRecord AddToOpenList(String fullName, int access, int share)
        {
            fullName = fullName.ToUpper();

            FileRecord record = new FileRecord(fullName, share);
            lock (m_openFiles)
            {
                int count = m_lockedDirs.Count;

                for (int i = 0; i < count; i++)
                {
                    if (IsInDirectory(fullName, (String)m_lockedDirs[i]))
                    {
                        throw new IOException("", (int)IOException.IOExceptionErrorCode.UnauthorizedAccess);
                    }
                }

                FileRecord current;
                count = m_openFiles.Count;

                for (int i = 0; i < count; ++i)
                {
                    current = (FileRecord)m_openFiles[i];
                    if (current.FullName == fullName)
                    {
                        // Given the previous fileshare info and the requested fileaccess and fileshare
                        // the following is the ONLY combinations that we should allow -- All others
                        // should failed with IOException
                        // (Behavior verified on desktop .NET)
                        //
                        // Previous FileShare   Requested FileAccess    Requested FileShare
                        // Read                 Read                    ReadWrite
                        // Write                Write                   ReadWrite
                        // ReadWrite            Read                    ReadWrite
                        // ReadWrite            Write                   ReadWrite
                        // ReadWrite            ReadWrite               ReadWrite
                        //
                        // The following check take advantage of the fact that the value for
                        // Read, Write, and ReadWrite in FileAccess enum and FileShare enum are
                        // identical.
                        if ((share != FileShareReadWrite) ||
                           ((current.Share & access) != access))
                        {
                            throw new IOException("", (int)IOException.IOExceptionErrorCode.UnauthorizedAccess);
                        }
                    }
                }

                m_openFiles.Add(record);
            }

            return record;
        }

        public static void RemoveFromOpenList(Object record)
        {
            lock (m_openFiles)
            {
                m_openFiles.Remove(record);
            }
        }

        public static Object LockDirectory(String directory)
        {
            directory = directory.ToUpper();

            lock (m_openFiles)
            {
                int count = m_openFiles.Count;
                for (int i = 0; i < count; i++)
                {
                    if (IsInDirectory(((FileRecord)m_openFiles[i]).FullName, directory))
                    {
                        throw new IOException("", (int)IOException.IOExceptionErrorCode.UnauthorizedAccess);
                    }
                }

                count = m_lockedDirs.Count;
                for (int i = 0; i < count; i++)
                {
                    if (((String)m_lockedDirs[i]) == directory)
                    {
                        throw new IOException("", (int)IOException.IOExceptionErrorCode.UnauthorizedAccess);
                    }
                }

                m_lockedDirs.Add(directory);
            }

            return (Object)directory;
        }

        public static void UnlockDirectory(Object record)
        {
            lock (m_openFiles)
            {
                m_lockedDirs.Remove(record);
            }
        }

        public static void UnlockDirectory(String directory)
        {
            directory = directory.ToUpper();

            lock (m_openFiles)
            {
                int count = m_lockedDirs.Count;
                for (int i = 0; i < count; i++)
                {
                    if (((String)m_lockedDirs[i]) == directory)
                    {
                        m_lockedDirs.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        public static void ForceRemoveNameSpace(String nameSpace)
        {
            String root = "\\" + nameSpace.ToUpper();

            FileRecord record;
            lock (m_openFiles)
            {
                int count = m_openFiles.Count;
                for (int i = 0; i < count; i++)
                {
                    record = (FileRecord)m_openFiles[i];
                    if (IsInDirectory(record.FullName, root))
                    {
                        if (record.NativeFileStream != null)
                        {
                            record.NativeFileStream.Close();
                        }

                        m_openFiles.RemoveAt(i);
                    }
                }
            }
        }

        public static bool IsInDirectory(String path, String directory)
        {
            if (path.IndexOf(directory) == 0)
            {
                int directoryLength = directory.Length;

                if (path.Length > directoryLength)
                {
                    return path[directoryLength] == '\\';
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        //--//

        public static String CurrentDirectory = NativeIO.FSRoot;
        private static Object m_currentDirectoryRecord = null;

        //--//

        internal static void SetCurrentDirectory(String path)
        {
            if (m_currentDirectoryRecord != null) // implies that CurrentDirectory != NativeIO.FSRoot
            {
                RemoveFromOpenList(m_currentDirectoryRecord);
            }

            if (path != NativeIO.FSRoot)
            {
                m_currentDirectoryRecord = AddToOpenListForRead(path);
            }
            else
            {
                m_currentDirectoryRecord = null;
            }

            CurrentDirectory = path;
        }
    }
}


