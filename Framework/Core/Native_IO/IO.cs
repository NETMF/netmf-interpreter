////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Runtime.CompilerServices;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("System.IO")]

namespace Microsoft.SPOT.IO
{
    internal class NativeFileStream
    {
        object m_fs;

        public const int TimeoutDefault = 0;
        public const int BufferSizeDefault = 0;

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern NativeFileStream(string path, int bufferSize);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int Read(byte[] buf, int offset, int count, int timeout);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int Write(byte[] buf, int offset, int count, int timeout);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern long Seek(long offset, uint origin);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern void Flush();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern long GetLength();
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern void SetLength(long length);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern void GetStreamProperties(out bool canRead, out bool canWrite, out bool canSeek);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern void Close();
    }

    internal class NativeFileInfo
    {
        public uint Attributes;
        public long CreationTime;
        public long LastAccessTime;
        public long LastWriteTime;
        public long Size;
        public String FileName;
    }

    internal class NativeFindFile
    {
        object m_ff;

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern NativeFindFile(string path, string searchPattern);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern NativeFileInfo GetNext();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern void Close();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern NativeFileInfo GetFileInfo(String path);
    }

    internal static class NativeIO
    {
        internal const string FSRoot = @"\";
        internal const int FSMaxPathLength = 260 - 2; // From FS_decl.h
        internal const int FSMaxFilenameLength = 256; // From FS_decl.h
        internal const int FSNameMaxLength = 7 + 1;   // From FS_decl.h

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void Format(String nameSpace, String fileSystem, String volumeLabel, uint parameter);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void Delete(string path);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool Move(string sourceFileName, string destFileName);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void CreateDirectory(string path);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern uint GetAttributes(string path);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void SetAttributes(string path, uint attributes);
    }
}


