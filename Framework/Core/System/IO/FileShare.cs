////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;

namespace System.IO
{
    // Contains constants for controlling file sharing options while
    // opening files.  You can specify what access other processes trying
    // to open the same file concurrently can have.
    [Serializable, Flags]
    public enum FileShare
    {
        // No sharing. Any request to open the file (by this process or another
        // process) will fail until the file is closed.
        None = 0,

        // Allows subsequent opening of the file for reading. If this flag is not
        // specified, any request to open the file for reading (by this process or
        // another process) will fail until the file is closed.
        Read = 1,

        // Allows subsequent opening of the file for writing. If this flag is not
        // specified, any request to open the file for writing (by this process or
        // another process) will fail until the file is closed.
        Write = 2,

        // Allows subsequent opening of the file for writing or reading. If this flag
        // is not specified, any request to open the file for writing or reading (by
        // this process or another process) will fail until the file is closed.
        ReadWrite = 3,
    }
}


