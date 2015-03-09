////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;

namespace System.IO
{

    [Serializable]
    public class IOException : SystemException
    {
        public enum IOExceptionErrorCode : int
        {
            Others = unchecked((int)0xE0000000), // CLR_E_FILE_IO
            InvalidDriver = unchecked((int)0xE1000000), // CLR_E_INVALID_DRIVER
            FileNotFound = unchecked((int)0xE2000000), // CLR_E_FILE_NOT_FOUND
            DirectoryNotFound = unchecked((int)0xE3000000), // CLR_E_DIRECTORY_NOT_FOUND
            VolumeNotFound = unchecked((int)0xE4000000), // CLR_E_VOLUME_NOT_FOUND
            PathTooLong = unchecked((int)0xE5000000), // CLR_E_PATH_TOO_LONG
            DirectoryNotEmpty = unchecked((int)0xE6000000), // CLR_E_DIRECTORY_NOT_EMPTY
            UnauthorizedAccess = unchecked((int)0xE7000000), // CLR_E_UNAUTHORIZED_ACCESS
            PathAlreadyExists = unchecked((int)0xE8000000), // CLR_E_PATH_ALREADY_EXISTS
            TooManyOpenHandles = unchecked((int)0xE9000000), // CLR_E_TOO_MANY_OPEN_HANDLES
        }

        public IOException()
            : base()
        {
        }

        public IOException(String message)
            : base(message)
        {
        }

        public IOException(String message, int hresult)
            : base(message)
        {
            m_HResult = hresult;
        }

        public IOException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        public IOExceptionErrorCode ErrorCode
        {
            get { return (IOExceptionErrorCode)m_HResult; }
        }
    }
}


