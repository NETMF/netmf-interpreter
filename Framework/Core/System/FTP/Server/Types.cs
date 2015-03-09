using System.IO;
using System.Threading;
using Microsoft.SPOT.Net.Ftp;
using System;

namespace Microsoft.SPOT.Net
{
    public static class WebRequestMethodsEx
    {

        public static class Ftp
        {
            //
            // Summary:
            //     Represents the FTP CWD protocol method that changes the current directory.
            public const string ChangeDirectory = "CWD";

            //
            // Summary:
            //     Represents the FTP RENAME protocol method that renames a directory.
            public const string RenameFrom = "RENAMEFROM";
            public const string RenameTo = "RENAMETO";
        }
    }

    /// <summary>
    /// Interface to communcate with context manager
    /// </summary>
    internal interface IContextManager
    {
        void AddContext(FtpListenerContext context);
    }

    /// <summary>
    /// Interface to communcate with ftp session
    /// </summary>
    internal interface IDataManager
    {
        bool IsDataStreamAvailable
        {
            get;
            set;
        }

        Stream DataStream
        {
            get;
        }

        ManualResetEvent DataChannelEstablished
        {
            get;
        }

        void ChangeCurrentDirectory(FilePath path);

        void CloseDataChannel();

        void SendResponse(string s);
    }
    
    public delegate void UserAuthenticator(object sender, UserAuthenticatorArgs e);

    public class UserAuthenticatorArgs : 
#if MICROFRAMEWORK
        Microsoft.SPOT.EventArgs
#else
        EventArgs
#endif
    {
        public string User;
        public string Password;
        private UserAuthenticationResult m_Result;

        public UserAuthenticationResult Result
        {
            get
            {
                return m_Result;
            }
            set
            {
                if (m_Result == UserAuthenticationResult.Unspecified)
                {
                    m_Result = value;
                }
                else if (m_Result != value)
                {
                    m_Result = UserAuthenticationResult.Conflicting;
                }
            }
        }

        public UserAuthenticatorArgs(string user, string pass)
        {
            User = user;
            Password = pass;
            m_Result = UserAuthenticationResult.Unspecified;
        }
    }

    /// <summary>
    /// User anthentication result
    /// </summary>
    public enum UserAuthenticationResult
    {
        Unspecified = 0,
        Approved = 1,
        Denied = 2,
        Conflicting = 3
    }


    /// <summary>
    /// FTP Command Type
    /// </summary>
    [Serializable]
    internal enum FtpCommandType
    {
        User = 0,
        Pass = 1,
        Cwd = 2,
        Quit = 3,
        Pasv = 4,
        Type = 5,
        List = 6,
        Port = 7,
        Sys = 8,
        Feature = 9,
        Pwd = 10,
        Retr = 11,
        Mdtm = 12,
        Size = 13,
        Store = 14,
        Noop = 15,
        Delete = 16,
        MkDir = 17,
        Rmd = 18,
        Rnfr = 19,
        Rnto = 20,
        NList = 21,
        Opts = 22,
        Unknown = 100
    }

   [Serializable]
    internal enum FtpState
    {
        WaitUser = 1,
        WaitPwd = 2,
        WaitCommand = 3,
        WaitRname = 4,
        Unknown = 100
    }
}
