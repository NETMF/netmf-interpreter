
namespace System.Net
{

    // Summary:
    //     Specifies the status codes returned for a File Transfer Protocol (FTP) operation.
    public enum FtpStatusCode
    {
        // Summary:
        //     Included for completeness, this value is never returned by servers.
        Undefined = 0,
        //
        // Summary:
        //     Specifies that the response contains a restart marker reply. The text of
        //     the description that accompanies this status contains the user data stream
        //     marker and the server marker.
        //RestartMarker = 110,
        //
        // Summary:
        //     Specifies that the service is not available now; try your request later.
        //ServiceTemporarilyNotAvailable = 120,
        //
        // Summary:
        //     Specifies that the data connection is already open and the requested transfer
        //     is starting.
        //DataAlreadyOpen = 125,
        //
        // Summary:
        //     Specifies that the server is opening the data connection.
        OpeningData = 150,
        //
        // Summary:
        //     Specifies that the command completed successfully.
        CommandOK = 200,
        //
        // Summary:
        //     Specifies that the command is not implemented by the server because it is
        //     not needed.
        //CommandExtraneous = 202,
        //
        // Summary:
        //     Specifies the status of a directory.
        //DirectoryStatus = 212,
        //
        // Summary:
        //     Specifies the status of a file.
        FileStatus = 213,
        //
        // Summary:
        //     Specifies the system type name using the system names published in the Assigned
        //     Numbers document published by the Internet Assigned Numbers Authority.
        //SystemType = 215,
        //
        // Summary:
        //     Specifies that the server is ready for a user login operation.
        //SendUserCommand = 220,
        //
        // Summary:
        //     Specifies that the server is closing the control connection.
        //ClosingControl = 221,
        //
        // Summary:
        //     Specifies that the server is closing the data connection and that the requested
        //     file action was successful.
        ClosingData = 226,
        //
        // Summary:
        //     Specifies that the server is entering passive mode.
        EnteringPassive = 227,
        //
        // Summary:
        //     Specifies that the user is logged in and can send commands.
        //LoggedInProceed = 230,
        //
        // Summary:
        //     Specifies that the server accepts the authentication mechanism specified
        //     by the client, and the exchange of security data is complete.
        //ServerWantsSecureSession = 234,
        //
        // Summary:
        //     Specifies that the requested file action completed successfully.
        FileActionOK = 250,
        //
        // Summary:
        //     Specifies that the requested path name was created.
        PathnameCreated = 257,
        //
        // Summary:
        //     Specifies that the server expects a password to be supplied.
        SendPasswordCommand = 331,
        //
        // Summary:
        //     Specifies that the server requires a login account to be supplied.
        NeedLoginAccount = 332,
        //
        // Summary:
        //     Specifies that the requested file action requires additional information.
        FileCommandPending = 350,
        //
        // Summary:
        //     Specifies that the service is not available.
        //ServiceNotAvailable = 421,
        //
        // Summary:
        //     Specifies that the data connection cannot be opened.
        //CantOpenData = 425,
        //
        // Summary:
        //     Specifies that the connection has been closed.
        //ConnectionClosed = 426,
        //
        // Summary:
        //     Specifies that the requested action cannot be performed on the specified
        //     file because the file is not available or is being used.
        //ActionNotTakenFileUnavailableOrBusy = 450,
        //
        // Summary:
        //     Specifies that an error occurred that prevented the request action from completing.
        ActionAbortedLocalProcessingError = 451,
        //
        // Summary:
        //     Specifies that the requested action cannot be performed because there is
        //     not enough space on the server.
        //ActionNotTakenInsufficientSpace = 452,
        //
        // Summary:
        //     Specifies that the command has a syntax error or is not a command recognized
        //     by the server.
        //CommandSyntaxError = 500,
        //
        // Summary:
        //     Specifies that one or more command arguments has a syntax error.
        ArgumentSyntaxError = 501,
        //
        // Summary:
        //     Specifies that the command is not implemented by the FTP server.
        CommandNotImplemented = 502,
        //
        // Summary:
        //     Specifies that the sequence of commands is not in the correct order.
        BadCommandSequence = 503,
        //
        // Summary:
        //     Specifies that login information must be sent to the server.
        NotLoggedIn = 530,
        //
        // Summary:
        //     Specifies that a user account on the server is required.
        AccountNeeded = 532,
        //
        // Summary:
        //     Specifies that the requested action cannot be performed on the specified
        //     file because the file is not available.
        ActionNotTakenFileUnavailable = 550,
        //
        // Summary:
        //     Specifies that the requested action cannot be taken because the specified
        //     page type is unknown. Page types are described in RFC 959 Section 3.1.2.3
        ActionAbortedUnknownPageType = 551,
        //
        // Summary:
        //     Specifies that the requested action cannot be performed.
        FileActionAborted = 552,
        //
        // Summary:
        //     Specifies that the requested action cannot be performed on the specified
        //     file.
        ActionNotTakenFilenameNotAllowed = 553,
    }

    public static class WebRequestMethods
    {
        // Summary:
        //     Represents the types of FTP protocol methods that can be used with an FTP
        //     request. This class cannot be inherited.
        public static class Ftp
        {
            // Summary:
            //     Represents the FTP APPE protocol method that is used to append a file to
            //     an existing file on an FTP server.
            //public const string AppendFile = "APPE";
            //
            // Summary:
            //     Represents the FTP DELE protocol method that is used to delete a file on
            //     an FTP server.
            public const string DeleteFile = "DELE";
            //
            // Summary:
            //     Represents the FTP RETR protocol method that is used to download a file from
            //     an FTP server.
            public const string DownloadFile = "RETR";
            //
            // Summary:
            //     Represents the FTP MDTM protocol method that is used to retrieve the date-time
            //     stamp from a file on an FTP server.
            //public const string GetDateTimestamp = "MDTM";
            //
            // Summary:
            //     Represents the FTP SIZE protocol method that is used to retrieve the size
            //     of a file on an FTP server.
            public const string GetFileSize = "SIZE";
            //
            // Summary:
            //     Represents the FTP NLIST protocol method that gets a short listing of the
            //     files on an FTP server.
            public const string ListDirectory = "NLST";
            //
            // Summary:
            //     Represents the FTP LIST protocol method that gets a detailed listing of the
            //     files on an FTP server.
            public const string ListDirectoryDetails = "LIST";
            //
            // Summary:
            //     Represents the FTP MKD protocol method creates a directory on an FTP server.
            public const string MakeDirectory = "MKD";
            //
            // Summary:
            //     Represents the FTP PWD protocol method that prints the name of the current
            //     working directory.
            //public const string PrintWorkingDirectory = "PWD";
            //
            // Summary:
            //     Represents the FTP RMD protocol method that removes a directory.
            public const string RemoveDirectory = "RMD";
            //
            // Summary:
            //     Represents the FTP RENAME protocol method that renames a directory.
            public const string Rename = "RENAME";
            //
            // Summary:
            //     Represents the FTP STOR protocol method that uploads a file to an FTP server.
            public const string UploadFile = "STOR";
            //
            // Summary:
            //     Represents the FTP STOU protocol that uploads a file with a unique name to
            //     an FTP server.
            //public const string UploadFileWithUniqueName = "STOU";
        }
    }
}
