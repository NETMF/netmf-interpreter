using System.IO;
using System.Net;
using System;
using Microsoft.SPOT.Net.Ftp;

namespace Microsoft.SPOT.Net
{
    /// <summary>
    /// The ftp response
    /// </summary>
    public class FtpListenerResponse
    {
        // Fields
        private bool m_NeedStream = false;                      // this response need data transmission
        private bool m_NeedInputStream = false;                 // whethere there is a input stream coming with the request
        private bool m_IsStreamExists = false;                  // "stream ready" flag
        private FtpListenerRequest m_Request = null;            // the reference of the request (need ftp method info from it)
        private Stream m_OutputeStream = null;                  // the reference of network stream from the underlying layer
        private FtpResponseStream m_InputStream = null;         // the reference of the response stream
        private IDataManager m_ResponseManager = null;          // the data manager which communicates with the underlying layer
        private FtpStatusCode m_StatusCode = 0;                 // ftp status code

        // Methods
        internal FtpListenerResponse(FtpListenerRequest request, IDataManager sender)
        {
            if (request.Method == WebRequestMethods.Ftp.DownloadFile || request.Method == WebRequestMethods.Ftp.ListDirectory || request.Method == WebRequestMethods.Ftp.ListDirectoryDetails)
            {
                m_NeedStream = true;
                m_NeedInputStream = true;
            }
            if (request.Method == WebRequestMethods.Ftp.GetFileSize)
            {
                m_NeedInputStream = true;
            }
            m_ResponseManager = sender;
            m_Request = request;
        }

        /// <summary>
        /// send response to the ftp client request, depending on the status code set by api users
        /// </summary>
        internal void SendResponse()
        {
            if (m_StatusCode == FtpStatusCode.Undefined)
            {
                throw new ArgumentException("Response status code unspecified.");
            }
            switch (m_Request.Method)
            {
                case WebRequestMethodsEx.Ftp.ChangeDirectory:
                    if (m_StatusCode == FtpStatusCode.FileActionOK)
                    { 
                        m_ResponseManager.ChangeCurrentDirectory(new FilePath(Utility.ReplaceBackSlash(m_Request.QueryString)));
                        SendResponse("250", "CWD command successful \r\n");
                    }
                    else
                    {
                        SendResponse("550", "No such file or directory. \r\n");
                    }
                    break;
                case WebRequestMethods.Ftp.DeleteFile:
                    if (m_StatusCode == FtpStatusCode.FileActionOK)
                    {
                        SendResponse("250", "File deletion completed. \r\n");
                    }
                    else
                    {
                        SendResponse("550", "Requested action not taken. File unavailable. \r\n");
                    }
                    break;
               case WebRequestMethods.Ftp.DownloadFile:
                    if (m_StatusCode == FtpStatusCode.ClosingData)
                    {
                        SendResponse("226", "File send OK. \r\n");
                    }
                    else
                    {
                        SendResponse("550", "Requested action not taken. File unavailable. \r\n");
                    }
                    m_ResponseManager.IsDataStreamAvailable = false;
                    break;
                case WebRequestMethods.Ftp.GetFileSize:
                    if (m_StatusCode == FtpStatusCode.FileStatus)
                    {
                        string str;
                        (m_InputStream as FtpResponseStream).Read(out str);
                        SendResponse("213", str + "\r\n");
                    }
                    else
                    {
                        SendResponse("550", "Requested action not taken. Permission denied. \r\n");
                    }
                    break;
               case WebRequestMethods.Ftp.ListDirectory:
                    // TODO: distinguish between list and Nlist
                    if (m_StatusCode == FtpStatusCode.ClosingData)
                    {
                        SendResponse("226", "Listing completed. \r\n");
                    }
                    else
                    {
                        SendResponse("503", "Bad sequence of commands. \r\n");
                    }
                    m_ResponseManager.IsDataStreamAvailable = false;
                    break;
                case WebRequestMethods.Ftp.ListDirectoryDetails:
                    if (m_StatusCode == FtpStatusCode.ClosingData)
                    {
                        SendResponse("226", "Listing completed. \r\n");
                    }
                    else
                    {
                        SendResponse("503", "Bad sequence of commands. \r\n");
                    }
                    m_ResponseManager.IsDataStreamAvailable = false;
                    break;
                case WebRequestMethods.Ftp.MakeDirectory:
                    if (m_StatusCode == FtpStatusCode.PathnameCreated)
                    {
                        SendResponse("257", "\"" + m_Request.QueryString + "\" created. \r\n");
                    }
                    else
                    {
                        SendResponse("550", "Requested action not taken. Directory exists. \r\n");
                    }
                    break;
                case WebRequestMethods.Ftp.RemoveDirectory:
                    if (m_StatusCode == FtpStatusCode.FileActionOK)
                    {
                        SendResponse("250", "Folder deletion completed. \r\n");
                    }
                    else
                    {
                        SendResponse("550", "Requested action not taken. Permission denied. \r\n");
                    }
                    break;
                case WebRequestMethodsEx.Ftp.RenameFrom:
                    if (m_StatusCode == FtpStatusCode.FileCommandPending)
                    {
                        SendResponse("350", "Request file action pending further information. \r\n");
                    }
                    else
                    {
                        SendResponse("550", "Requested action not taken. File/Folder unavailable. \r\n");
                    }
                    break;
                case WebRequestMethodsEx.Ftp.RenameTo:
                    if (m_StatusCode == FtpStatusCode.FileActionOK)
                    {
                        SendResponse("250", "Rename action succeeded. \r\n");
                    }
                    else if (m_StatusCode == FtpStatusCode.ArgumentSyntaxError)
                    {
                        SendResponse("501", "New name is not allowed. \r\n");
                    }
                    else
                    {
                        SendResponse("503", "Bad sequence of commands. \r\n");
                    }
                    break;
                case WebRequestMethods.Ftp.UploadFile:
                    if (m_StatusCode == FtpStatusCode.ClosingData)
                    {
                        SendResponse("226", "File upload OK. \r\n");
                    }
                    else
                    {
                        SendResponse("503", "Bad sequence of commands. \r\n");
                    }
                    m_ResponseManager.IsDataStreamAvailable = false;
                    break;
                default:
                    throw new NotSupportedException("Ftp method(" + m_Request.Method + ") not allowed.");
            }
        }

        /// <summary>
        /// overloads of send response method
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        private void SendResponse(string code, string message)
        {
            m_ResponseManager.SendResponse(code + " " + message);
            if (m_NeedStream)
                m_ResponseManager.CloseDataChannel();
        }


        // Properties
        /// <summary>
        /// the stream that data would be written to
        /// </summary>
        public FtpResponseStream OutputStream
        {
            get
            {
                if (!m_NeedStream && !m_NeedInputStream)
                {
                    m_InputStream = new FtpResponseStream(this);
                }
                else if (!m_IsStreamExists)
                {
                    if (m_NeedStream)
                    {
                        m_ResponseManager.DataChannelEstablished.WaitOne();         // block until data stream ready
                        if (!m_ResponseManager.IsDataStreamAvailable)
                        {
                            return null;
                        }
                        m_OutputeStream = m_ResponseManager.DataStream;
                    }
                    if (m_NeedInputStream)
                    {
                        if (m_NeedStream)
                        {
                            m_InputStream = new FtpResponseStream(this, m_OutputeStream);
                        }
                        else
                        {
                            m_InputStream = new FtpResponseStream(this);
                        }
                    }
                    m_IsStreamExists = true;
                }
                return m_InputStream;
            }
        }

        /// <summary>
        /// status code set by users
        /// </summary>
        public FtpStatusCode StatusCode
        {
            get
            {
                return m_StatusCode;
            }
            set
            {
                if ((int)value < 100 || (int)value > 999)
                {
                    throw new ArgumentException("Protocol violation: not such status code.");
                }
                m_StatusCode = value;
            }
        }

        /// <summary>
        /// ftp method of the request
        /// </summary>
        internal string RequestMethod
        {
            get
            {
                return m_Request.Method;
            }
        }
    }
}
