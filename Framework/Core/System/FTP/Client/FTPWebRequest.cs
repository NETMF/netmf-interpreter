using System.Collections;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Net.Ftp;

namespace System.Net
{
    /// <summary>
    /// FtpWebRequest
    ///  - first implement Active mode 
    ///  - this version does not support dns
    /// </summary>
    public class FtpWebRequest : WebRequest
    {
        /// <summary>
        /// Ftp status code
        /// </summary>
        internal enum FTPStatusCode
        {
            LoggedInProceed = 1,
            NotLoggedIn = 2,
            CommandNotImplemented = 3,
            ClosingData = 4,
            CantOpenData = 5,
            ResourceUnavailableOrBusy = 6,
            CommandProceed = 7,
            CommandFailed = 8,
            FilenameNotAllowed = 9,
            InsufficientSpace = 10,
            Unknown = 100
        }

        public string[] RequestPath = null;
        public string ServerIP = null;
        public int ServerPort = 21;
        public string FTPActiveAddress = null;
        public int FTPActivePort = 0;

        private NetworkCredential m_Credential = new NetworkCredential("anonymous", "anonymous@");
                                                        // credential used for the connection
        private string m_FtpMethod = null;              // the ftp request method type

        internal Socket DataSocket = null;              // data connnection socket
        internal Socket CommandSocket = null;           // command connection socket

        internal Thread DaemonThread = null;            // worker thread

        private AutoResetEvent m_StreamReady = new AutoResetEvent(false);
                                                        // event to indicate that data connection has been established

        private NetworkStream m_DataStream = null;      // data connnection stream
        private FtpWebResponse m_FtpResponse = null;    // the ftp response

        // System running flags
        public bool DataSocketReady = false;            // indicates that the data transmission has started
        public bool TransmissionFinished = false;       // indicates that the data transmission has finished


        // Configuration Variables
        private bool m_IsClosed = false;                // whether the request has been closed
        private bool m_IsPassive = false;               // whether working under passive mode
        private int m_CommandTimeout = 1000;            // time for waiting a single ftp command
        private int m_DataTimeout = 5000;               // time for waiting a active data connection
        private bool m_IsStarted = false;               // make sure that the request can only be executed once (restricted by current version)   
        private string m_RenameTo = null;               // the new name towards the rename method
        private FTPStatusCode m_StatusCode = FTPStatusCode.Unknown;
                                                        // the response status code


        // Properties
        internal FTPStatusCode StatusCode
        {
            get
            {
                return m_StatusCode;
            }
        }

        /// <summary>
        /// The new name of the file/directory for rename command
        /// </summary>
        public string RenameTo
        {
            get
            {
                return m_RenameTo;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentException("The value is a null reference.");
                }
                else if (m_IsStarted)
                {
                    throw new InvalidOperationException("A new value was specified for this property for a request that is already in progress.");
                }
                else
                {
                    m_RenameTo = value;
                }
            }
        }

        /// <summary>
        /// Response number
        /// </summary>
        internal int ResponseCode
        {
            get;
            set;
        }

        internal string ResponseMessage
        {
            get;
            set;
        }

        internal ArrayList ResponseStack
        {
            get;
            set;
        }

        /// <summary>
        /// Set ftp mode
        /// </summary>
        public bool UsePassive
        {
            get
            {
                return m_IsPassive;
            }
            set
            {
                if (value == true)
                {
                    throw new NotSupportedException("This version does not support passive mode.");
                }
            }
        }

        public NetworkCredential Credentials
        {
            get
            {
                return m_Credential;
            }
            set
            {
                m_Credential = value;
            }
        }

        public override string Method
        {
            get
            {
                return m_FtpMethod;
            }
            set
            {
                m_FtpMethod = value;
            }
        }


        // TODO: we need a new method to parse connection string. currently only support Uri of the form ftp://hostIP:port/LogcialPath
        internal FtpWebRequest(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("URI is not resolvable.");
            }
            else
            {
                // TODO: check uri is a valid ftp uri
                // only ftp://xxx.xxx.xxx.xxx:yy/path and ftp://xxx.xxx.xxx.xxx/path are valid

                int idxPath = 1;                                                            // the position of logical path in the array
                string s = uri.AbsolutePath.Trim().TrimStart(new char[] { '/' });           // remove heading slashes and all white-spaces
                string[] sArray = s.Split(new char[] { ':', '/' });                         // split path and address
                ServerIP = sArray[0];           // TODO: dns support for public ftp server
                if (s.IndexOf(':') >= 0)                                                    // contains port information
                {
                    ServerPort = int.Parse(sArray[1]);
                    idxPath++;
                }

                bool ok = true;
                try
                {
                    String[] parts = ServerIP.Split('.');
                    ok &= (null != parts) && (parts.Length == 4);
                    for (int i = 0; ok && (i < parts.Length); i++)
                    {
                        string part = parts[i];
                        ok &= (null != part) && (part.Length > 0 && part.Length <= 3);
                        if (!ok) break;

                        int val = 0;
                        for (int j = 0; j < part.Length; j++)
                        {
                            if (part[j] < '0' || part[j] > '9') ok = false;

                            val = (val * 10) + (part[j] - '0');
                        }

                        ok &= (val >= 0) && (val <= 255);
                    }

                    //if (ok)
                    //{
                    //    IPAddress ip = IPAddress.Parse(ServerIP);
                    //    if (null == ip) throw new ArgumentException();
                    //}
                }
                catch
                {
                    ok = false;
                }

                if (!ok)
                {
                    foreach (var item in System.Net.Dns.GetHostEntry(ServerIP).AddressList)
                    {
                        if (null == item) continue;

                        ServerIP = item.ToString().Trim();
                    }
                }

                RequestPath = new string[sArray.Length - idxPath];
                for (int i = idxPath; i < sArray.Length; i++)                               // copy all logical path into the path array
                {
                    RequestPath[i - idxPath] = sArray[i];
                }
                // find the address for the device in case of active transmission mode
                
                foreach (var item in System.Net.Dns.GetHostEntry("").AddressList)
                {
                    if (null == item) continue;

                    byte[] addr = item.GetAddressBytes();
                    if (addr == null || addr.Length != 4) continue;
                    if ((addr[0] == 127) && (addr[1] == 0) && (addr[2] == 0) && (addr[3] == 1)) continue;

                    FTPActiveAddress = item.ToString().Trim();

                    break;
                }
            }
        }

        /// <summary>
        /// Get response from the ftp server
        /// </summary>
        /// <returns></returns>
        public override WebResponse GetResponse()
        {
            
            if (m_FtpMethod == null) 
            {
                throw new ArgumentNullException("FTP method is not specified.");
            }
            if (Method == "STOR")
            {
                m_FtpResponse = new FtpWebResponse();
                return m_FtpResponse;
            }
            else  if (Login() == 0 && !m_IsStarted)       // login succeed
            {
                m_IsStarted = true;
                switch (Method)
                {
                    case "NLST":
                        DaemonThread = new Thread(NLSTThread);
                        DaemonThread.Start();
                        m_StreamReady.WaitOne();
                        break;
                    case "LIST":
                        DaemonThread = new Thread(LISTThread);
                        DaemonThread.Start();
                        m_StreamReady.WaitOne();
                        break;
                    case "DELE":
                        DaemonThread = new Thread(DELEThread);
                        DaemonThread.Start();
                        break;
                    case "MKD":
                        DaemonThread = new Thread(MKDThread);
                        DaemonThread.Start();
                        break;
                    case "RMD":
                        DaemonThread = new Thread(RMDThread);
                        DaemonThread.Start();
                        break;
                    case "RENAME":
                        DaemonThread = new Thread(RenameThread);
                        DaemonThread.Start();
                        break;
                    case "RETR":
                        DaemonThread = new Thread(DownloadThread);
                        DaemonThread.Start();
                        m_StreamReady.WaitOne();
                        break;
                    default:
                        throw new ArgumentException("FTP method " + Method + " does not implemented."); ;
                }
                return m_FtpResponse;
            }
            else
            {
                Debug.Print("Login failed");
                DataSocketReady = true;
                return null;
            }
        }

        /// <summary>
        /// Get request stream for upload command
        /// </summary>
        /// <returns></returns>
        public override Stream GetRequestStream()
        {
            if (Method == "STOR" && Login() == 0 && !m_IsStarted)       // login succeed
            {
                m_IsStarted = true;
                DaemonThread = new Thread(UploadThread);
                DaemonThread.Start();
                m_StreamReady.WaitOne();
                return m_DataStream;
            }
            else
            {
                Debug.Print("stream is not available");
                DataSocketReady = true;
                return null;
            }
        }

        /// <summary>
        /// Worker thread for make directory
        /// </summary>
        private void MKDThread()
        {
            if (RequestPath.Length < 2)
            {
                Debug.Print("the path is not long enough");
                throw new ArgumentException("The path is not a directory");
            }
            else if (RequestPath[RequestPath.Length - 1] != "")
            {
                // the path is a directory
                Debug.Print("the path does not lead to a directory");
                throw new ArgumentException("The path is not a directory");
            }
            try
            {
                string command;
                int responseNumber;

                // need to change directory
                if (RequestPath.Length > 2)
                {
                    command = "CWD ";
                    for (int i = 0; i < RequestPath.Length - 2; i++)
                    {
                        command += RequestPath[i] + "/";
                    }
                    command += "\r\n";
                    CommandSocket.Send(Encoding.UTF8.GetBytes(command));
                    responseNumber = WaitResponse();
                    if (responseNumber != 250)
                    {
                        // change directory failed
                        m_StatusCode = FTPStatusCode.CommandFailed;
                        return;
                    }
                }
                command = "MKD " + RequestPath[RequestPath.Length - 2] + "\r\n";
                CommandSocket.Send(Encoding.UTF8.GetBytes(command));
                responseNumber = WaitResponse();
                if (responseNumber != 257)
                {
                    Debug.Print("cannot create directory with wrong number: " + responseNumber.ToString());
                    m_StatusCode = FTPStatusCode.CommandFailed;
                    return;
                }
            }
            catch (SocketException se)
            {
                Debug.Print(se.Message);
                throw new WebException("Making directory fail due to connection problem.");
            }
        }

        /// <summary>
        /// Worker thread for remove directory
        /// </summary>
        private void RMDThread()
        {
            if (RequestPath.Length < 2)
            {
                Debug.Print("the path is not long enough");
                throw new ArgumentException("The path is not a directory");
            }
            else if (RequestPath[RequestPath.Length - 1] != "")
            {
                // the path is a directory
                Debug.Print("the path does not lead to a directory");
                throw new ArgumentException("The path is not a directory");
            }
            try
            {
                string command;
                int responseNumber;
                if (RequestPath.Length > 2)
                {
                    command = "CWD ";
                    for (int i = 0; i < RequestPath.Length - 2; i++)
                    {
                        command += RequestPath[i] + "/";
                    }
                    command += "\r\n";
                    CommandSocket.Send(Encoding.UTF8.GetBytes(command));
                    responseNumber = WaitResponse();
                    if (responseNumber != 250)
                    {
                        // change directory failed
                        return;
                    }
                }
                command = "RMD " + RequestPath[RequestPath.Length - 2] + "\r\n";
                CommandSocket.Send(Encoding.UTF8.GetBytes(command));
                responseNumber = WaitResponse();
                if (responseNumber != 250)
                {
                    Debug.Print("cannot remove directory with wrong number: " + responseNumber.ToString());
                    if (responseNumber == 550)
                    {
                        m_StatusCode = FTPStatusCode.ResourceUnavailableOrBusy;
                    }
                    else
                    {
                        m_StatusCode = FTPStatusCode.CommandFailed;
                    }
                    return;
                }
            }
            catch (SocketException se)
            {
                Debug.Print(se.Message);
                throw new WebException("Deleting directory fail due to connection problem.");
            }
        }

        /// <summary>
        /// Worker thread for delete file
        /// </summary>
        private void DELEThread()
        {
            if (RequestPath[RequestPath.Length - 1] == "")
            {
                // the path is a directory
                Debug.Print("the path leads to a directory");
                throw new ArgumentException("The path is a directory");
            }
            try
            {
                string command = "CWD ";
                for (int i = 0; i < RequestPath.Length - 1; i++)
                {
                    command += RequestPath[i] + "/";
                }
                command += "\r\n";
                CommandSocket.Send(Encoding.UTF8.GetBytes(command));
                int responseNumber = WaitResponse();
                if (responseNumber != 250)
                {
                    // change directory failed
                    m_StatusCode = FTPStatusCode.CommandFailed;
                    return;
                }
                command = "DELE " + RequestPath[RequestPath.Length - 1] + "\r\n";
                CommandSocket.Send(Encoding.UTF8.GetBytes(command));
                responseNumber = WaitResponse();
                if (responseNumber != 250)
                {
                    Debug.Print("deletion fail with wrong number: " + responseNumber.ToString());
                    if (responseNumber == 550)
                    {
                        m_StatusCode = FTPStatusCode.ResourceUnavailableOrBusy;
                    }
                    else
                    {
                        m_StatusCode = FTPStatusCode.CommandFailed;
                    }
                    return;
                }
            }
            catch (SocketException se)
            {
                Debug.Print(se.Message);
                throw new WebException("Deleting file fail due to connection problem.");
            }
        }

        /// <summary>
        /// Worker thread to rename a file or directory
        /// </summary>
        private void RenameThread()
        {
            if (RenameTo == null)
            {
                Debug.Print("New name is not selected");
                throw new ArgumentNullException("New name is not selected");
            }
            int oldIndex;
            if (RequestPath[RequestPath.Length - 1] == "")
            {
                throw new WebException("The requested URI is invalid for this FTP command.");
            }
            else
            {
                oldIndex = RequestPath.Length - 1;
            }
            try
            {
                string command = "CWD ";
                for (int i = 0; i < oldIndex; i++)
                {
                    command += RequestPath[i] + "/";
                }
                command += "\r\n";
                CommandSocket.Send(Encoding.UTF8.GetBytes(command));
                int responseNumber = WaitResponse();
                if (responseNumber != 250)
                {
                    // change directory failed
                    m_StatusCode = FTPStatusCode.CommandFailed;
                    return;
                }
                command = "RNFR " + RequestPath[oldIndex] + "\r\n";
                CommandSocket.Send(Encoding.UTF8.GetBytes(command));
                responseNumber = WaitResponse();
                if (responseNumber != 350)
                {
                    Debug.Print("REFR fail with wrong number: " + responseNumber.ToString());
                    if (responseNumber == 550)
                    {
                        m_StatusCode = FTPStatusCode.ResourceUnavailableOrBusy;
                    }
                    else
                    {
                        m_StatusCode = FTPStatusCode.CommandFailed;
                    }
                    return;
                }
                command = "RNTO " + RenameTo + "\r\n";
                CommandSocket.Send(Encoding.UTF8.GetBytes(command));
                responseNumber = WaitResponse();
                if (responseNumber != 250)
                {
                    Debug.Print("RETO fail with wrong number: " + responseNumber.ToString());
                    m_StatusCode = FTPStatusCode.CommandFailed;
                    return;
                }
            }
            catch (SocketException se)
            {
                Debug.Print(se.Message);
                throw new WebException("Renaming fail due to connection problem.");
            }
        }

        private static Random s_port = new Random((int)DateTime.Now.Ticks);
        private bool ConfigureListenSocket(Socket listenSocket)
        {
            for (int i = 0; i < 10; i++)
            {
                int hostPort = 10000 + (s_port.Next() % 10000);
                IPAddress hostIp = IPAddress.Parse(FTPActiveAddress);
                IPEndPoint ep = new IPEndPoint(hostIp, hostPort);
                try
                {
                    listenSocket.Bind(ep);
                    listenSocket.Listen(1);

                    return true;
                }
                catch (SocketException se)
                {
                    Logging.Print(se.Message);
                }
            }

            return false;
        }

        /// <summary>
        /// Worker thread to download file
        /// </summary>
        private void DownloadThread()
        {
            if (RequestPath[RequestPath.Length - 1] == "")
            {
                // the path is a directory
                Debug.Print("the path leads to a directory");
                throw new ArgumentException("The path is a directory");
            }
            try
            {
                string command = "CWD ";

                for (int i = 0; i < RequestPath.Length - 1; i++)
                {
                    command += RequestPath[i] + "/";
                }
                command += "\r\n";
                CommandSocket.Send(Encoding.UTF8.GetBytes(command));
                int responseNumber = WaitResponse();
                if (responseNumber != 250)
                {
                    // change directory failed
                    m_StatusCode = FTPStatusCode.CommandFailed;
                    return;
                }
                using (Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    ConfigureListenSocket(listenSocket);

                    FTPActivePort = (listenSocket.LocalEndPoint as IPEndPoint).Port;
                    command = "PORT " + FormatFTPAddress(FTPActiveAddress, FTPActivePort) + "\r\n";
                    CommandSocket.Send(Encoding.UTF8.GetBytes(command));
                    responseNumber = WaitResponse();
                    while (responseNumber == 230)
                    {
                        responseNumber = WaitResponse();
                    }
                    if (responseNumber != 200)
                    {
                        Debug.Print("port with wrong number: " + responseNumber.ToString());
                        m_StatusCode = FTPStatusCode.CommandFailed;
                        return;
                    }
                    CommandSocket.Send(Encoding.UTF8.GetBytes("RETR " + RequestPath[RequestPath.Length - 1] + "\r\n"));
                    responseNumber = WaitResponse();
                    if (!SetStatusCode(responseNumber))
                    {
                        return;
                    }

                    using (Timer timer = new Timer(TimerCallback, null, m_DataTimeout, -1))
                    {
                        DataSocket = listenSocket.Accept();
                    }

                    m_DataStream = new NetworkStream(DataSocket);
                    m_FtpResponse = new FtpWebResponse(m_DataStream);

                    DataSocketReady = true;
                    m_StreamReady.Set();

                    responseNumber = WaitResponse(-1);
                    TransmissionFinished = true;
                    if (responseNumber != 226)
                    {
                        Debug.Print("download end with wrong number: " + responseNumber.ToString());
                        m_StatusCode = FTPStatusCode.CommandFailed;
                        return;
                    }
                    m_StatusCode = FTPStatusCode.CommandProceed;
                    //while (!m_RequestClosed)
                    //{
                    //    // do not release the resource until the request is closed
                    //}
                }
            }
            catch (SocketException se)
            {
                Debug.Print(se.Message);
                throw new WebException("Downloading fail due to connection problem.");
            }

            finally
            {
                if (!DataSocketReady)
                {
                    DataSocketReady = true;
                    m_StreamReady.Set();
                }
            }

        }

        /// <summary>
        /// Worker thread to upload file
        /// </summary>
        private void UploadThread()
        {
            if (RequestPath[RequestPath.Length - 1] == "")
            {
                Debug.Print("the path is not a file");
                throw new ArgumentException("The path is a directory");
            }
            try
            {
                string command = "CWD ";

                for (int i = 0; i < RequestPath.Length - 1; i++)
                {
                    command += RequestPath[i] + "/";
                }
                command += "\r\n";
                CommandSocket.Send(Encoding.UTF8.GetBytes(command));
                int responseNumber = WaitResponse();
                if (responseNumber != 250)
                {
                    // change directory failed
                    m_StatusCode = FTPStatusCode.CommandFailed;
                    return;
                }
                using (Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    ConfigureListenSocket(listenSocket);

                    FTPActivePort = (listenSocket.LocalEndPoint as IPEndPoint).Port;
                    command = "PORT " + FormatFTPAddress(FTPActiveAddress, FTPActivePort) + "\r\n";
                    CommandSocket.Send(Encoding.UTF8.GetBytes(command));
                    responseNumber = WaitResponse();
                    while (responseNumber == 230)
                    {
                        responseNumber = WaitResponse();
                    }
                    if (responseNumber != 200)
                    {
                        Debug.Print("port with wrong number: " + responseNumber.ToString());
                        m_StatusCode = FTPStatusCode.CommandFailed;
                        return;
                    }
                    CommandSocket.Send(Encoding.UTF8.GetBytes("STOR " + RequestPath[RequestPath.Length - 1] + "\r\n"));
                    responseNumber = WaitResponse();
                    if (!SetStatusCode(responseNumber))
                    {
                        return;
                    }

                    using (Timer timer = new Timer(TimerCallback, null, m_DataTimeout, -1))
                    {
                        DataSocket = listenSocket.Accept();
                    }
               
                    m_DataStream = new NetworkStream(DataSocket, true);
                    m_FtpResponse = new FtpWebResponse(m_DataStream);
                    DataSocketReady = true;

                    m_StreamReady.Set();

                    responseNumber = WaitResponse(-1);
                    TransmissionFinished = true;
                    if (responseNumber != 226)
                    {
                        Debug.Print("upload end with wrong number: " + responseNumber.ToString());
                        m_StatusCode = FTPStatusCode.CommandFailed;
                        return;
                    }
                    m_StatusCode = FTPStatusCode.CommandProceed;
                    //while (!m_RequestClosed)
                    //{
                    //    // do not release the resource until the request is closed
                    //}
                }
            }
            catch (SocketException se)
            {
                Debug.Print(se.Message);
                throw new WebException("Downloading fail due to connection problem.");
            }
        }

        /// <summary>
        /// Functionality to run NLST scenario
        /// </summary>
        private void NLSTThread()
        {
            if (RequestPath[RequestPath.Length - 1] != "")
            {
                // the path is a directory
                Debug.Print("the path does not lead to a directory");
                throw new ArgumentException("The path is not a directory");
            }
            try
            {
                string command = "CWD ";
                for (int i = 0; i < RequestPath.Length - 1; i++)
                {
                    command += RequestPath[i] + "/";
                }
                command += "\r\n";
                CommandSocket.Send(Encoding.UTF8.GetBytes(command));
                int responseNumber = WaitResponse();
                if (responseNumber != 250)
                {
                    // change directory failed
                    m_StatusCode = FTPStatusCode.CommandFailed;
                    return;
                }
                using (Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    ConfigureListenSocket(listenSocket);

                    FTPActivePort = (listenSocket.LocalEndPoint as IPEndPoint).Port;
                    command = "PORT " + FormatFTPAddress(FTPActiveAddress, FTPActivePort) + "\r\n";
                    CommandSocket.Send(Encoding.UTF8.GetBytes(command));
                    responseNumber = WaitResponse();
                    while (responseNumber == 230)
                    {
                        responseNumber = WaitResponse();
                    }
                    if (responseNumber != 200)
                    {
                        Debug.Print("port with wrong number: " + responseNumber.ToString());
                        m_StatusCode = FTPStatusCode.CommandFailed;
                        return;
                    }
                    CommandSocket.Send(Encoding.UTF8.GetBytes("NLST\r\n"));
                    responseNumber = WaitResponse();
                    if (!SetStatusCode(responseNumber))
                    {
                        return;
                    }

                    using (Timer timer = new Timer(TimerCallback, null, m_DataTimeout, -1))
                    {
                        DataSocket = listenSocket.Accept();
                    }

                    DataSocketReady = true;
                    m_DataStream = new NetworkStream(DataSocket);
                    m_FtpResponse = new FtpWebResponse(m_DataStream);
                    m_StreamReady.Set();

                    responseNumber = WaitResponse(-1);
                    TransmissionFinished = true;
                    if (responseNumber != 226)
                    {
                        Debug.Print("list end with wrong number: " + responseNumber.ToString());
                        m_StatusCode = FTPStatusCode.CommandFailed;
                        return;
                    }
                    m_StatusCode = FTPStatusCode.CommandProceed;

                    //while (!m_RequestClosed)
                    //{
                    //    // do not release the resource until the request is closed
                    //}
                    m_StatusCode = FTPStatusCode.ClosingData;
                }
            }
            catch (SocketException se)
            {
                Debug.Print(se.Message);
                throw new WebException("Downloading fail due to connection problem.");
            }
        }

        /// <summary>
        /// Functionality to run LIST scenario
        /// </summary>
        private void LISTThread()
        {
            if (RequestPath[RequestPath.Length - 1] != "")
            {
                // the path is a directory
                Debug.Print("the path does not lead to a directory");
                throw new ArgumentException("The path is not a directory");
            }
            try
            {
                string command = "CWD ";
                for (int i = 0; i < RequestPath.Length - 1; i++)
                {
                    command += RequestPath[i] + "/";
                }
                command += "\r\n";
                CommandSocket.Send(Encoding.UTF8.GetBytes(command));
                int responseNumber = WaitResponse();
                if (responseNumber != 250)
                {
                    // change directory failed
                    m_StatusCode = FTPStatusCode.CommandFailed;
                    return;
                }
                using (Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    ConfigureListenSocket(listenSocket);

                    FTPActivePort = (listenSocket.LocalEndPoint as IPEndPoint).Port;
                    command = "PORT " + FormatFTPAddress(FTPActiveAddress, FTPActivePort) + "\r\n";
                    CommandSocket.Send(Encoding.UTF8.GetBytes(command));
                    responseNumber = WaitResponse();
                    while (responseNumber == 230)
                    {
                        responseNumber = WaitResponse();
                    }
                    if (responseNumber != 200)
                    {
                        Debug.Print("port with wrong number: " + responseNumber.ToString());
                        m_StatusCode = FTPStatusCode.CommandFailed;
                        return;
                    }
                    CommandSocket.Send(Encoding.UTF8.GetBytes("LIST\r\n"));
                    responseNumber = WaitResponse();
                    if (!SetStatusCode(responseNumber))
                    {
                        return;
                    }

                    using (Timer timer = new Timer(TimerCallback, null, m_DataTimeout, -1))
                    {
                        DataSocket = listenSocket.Accept();
                    }

                    DataSocketReady = true;
                    m_DataStream = new NetworkStream(DataSocket);
                    m_FtpResponse = new FtpWebResponse(m_DataStream);
                    m_StreamReady.Set();

                    responseNumber = WaitResponse(-1);
                    TransmissionFinished = true;
                    if (responseNumber != 226)
                    {
                        Debug.Print("list end with wrong number: " + responseNumber.ToString());
                        m_StatusCode = FTPStatusCode.CommandFailed;
                        return;
                    }
                    m_StatusCode = FTPStatusCode.CommandProceed;

                    //while (!m_RequestClosed)
                    //{
                    //    // do not release the resource until the request is closed
                    //}
                    m_StatusCode = FTPStatusCode.ClosingData;
                }
            }
            catch (SocketException se)
            {
                Debug.Print(se.Message);
                throw new WebException("Downloading fail due to connection problem.");
            }
        }

        /// <summary>
        /// construct endpoint in the form of (xxx,xxx,xxx,xxx,yyy,yyy)
        /// </summary>
        /// <param name="address">ip address</param>
        /// <param name="port">connection port</param>
        /// <returns></returns>
        private string FormatFTPAddress(string address, int port)
        {
            string[] addresses = address.Split(new char[] { '.' });
            if (addresses.Length != 4)
            {
                // not a valid IPv4 address
                throw new NotSupportedException("Currently only support IPv4 address.");
            }
            if (port > 65535)
            {
                throw new ArgumentOutOfRangeException("Port number too large.");
            }
            string result = "";

            foreach (string s in addresses)
            {
                result += s + ",";
            }
            result += (port / 256).ToString() + "," + (port % 256).ToString();
            return result;
        }


        /// <summary>
        /// Read a line of command from the socket, return reply code
        /// </summary>
        /// <returns></returns>
        private int WaitResponse()
        {
            return WaitResponse(m_CommandTimeout);
        }

        /// <summary>
        /// Read several lines from the socket, until reply code is found
        /// </summary>
        /// <param name="timeout">specify the time to wait for a response</param>
        /// <returns></returns>
        private int WaitResponse(int timeout)
        {
            byte[] buffer = null;
            string bufferString = "";                       // using string to manage the buffer
            int readLength = 0;
            bool responseArrived = false;

            if (m_IsClosed)
            {
                ResponseCode = 0;
                ResponseMessage = "Class disposed.";
                return ResponseCode;
            }
            else if (ResponseStack == null)
            {
                ResponseStack = new ArrayList();
            }
            else if (ResponseStack.Count > 0)
            {
                ResponseStack.Clear();
            }
            while (!responseArrived)
            {
                if (m_IsClosed)
                {
                    ResponseCode = 0;
                    ResponseMessage = "Class disposed.";
                    break;
                }
                // block the method until get something or throw a socket exception
                if (CommandSocket.Poll(1000 * timeout, SelectMode.SelectRead))
                {
                    if (CommandSocket.Available == 0)
                    {
                        throw new SocketException(SocketError.SocketError);
                    }
                    buffer = new byte[CommandSocket.Available];
                    readLength = CommandSocket.Receive(buffer, buffer.Length, 0);

                    bufferString += new string(Encoding.UTF8.GetChars(buffer));

                    if (bufferString.IndexOf('\n') >= 0)
                    {
                        string[] tempArray = bufferString.Split(new char[] { '\n' });
                        int replyNumber = 0;
                        int lastReplyNumber = 0;
                        for (int i = 0; i < tempArray.Length - 1; i++)
                        {
                            replyNumber = ParseCommand(tempArray[i]);
                            if (i == 0)
                            {
                                lastReplyNumber = replyNumber;
                            }
                            else if(replyNumber != lastReplyNumber && replyNumber != (0 - lastReplyNumber))
                            {
                                ResponseCode = -1;
                                ResponseMessage = "Command socket polling error.";
                                return ResponseCode;
                            }
                            if (replyNumber > 0 && !responseArrived)
                            {
                                responseArrived = true;
                                ResponseCode = replyNumber;
                                ResponseMessage = tempArray[i].Substring(4);
                                break;
                            }
                            ResponseStack.Add(tempArray[i]);
                        }
                        bufferString = tempArray[tempArray.Length - 1];
                    }
                }
                else
                {
                    ResponseCode = -1;
                    ResponseMessage = "Command socket polling error.";
                    throw new SocketException(SocketError.SocketError);
                }
            }
            return ResponseCode;
        }

        /// <summary>
        /// parse the response number to a status code
        /// </summary>
        /// <param name="responseNumber"></param>
        /// <returns>
        /// true if response number is less than 400
        /// </returns>
        private bool SetStatusCode(int responseNumber)
        {
            if (responseNumber >= 500)
            {
                Debug.Print(Method + " with wrong number: " + responseNumber.ToString());
                if (responseNumber == 502)
                {
                    m_StatusCode = FTPStatusCode.CommandNotImplemented;
                }
                else if (responseNumber == 550)
                {
                    m_StatusCode = FTPStatusCode.ResourceUnavailableOrBusy;
                }
                else if (responseNumber == 553)
                {
                    m_StatusCode = FTPStatusCode.FilenameNotAllowed;
                }
                else
                {
                    m_StatusCode = FTPStatusCode.CommandFailed;
                }
                return false;
            }
            else if (responseNumber >= 400)
            {
                Debug.Print(Method + " with wrong number: " + responseNumber.ToString());
                if (responseNumber == 452)
                {
                    m_StatusCode = FTPStatusCode.InsufficientSpace;
                }
                else
                {
                    m_StatusCode = FTPStatusCode.ResourceUnavailableOrBusy;
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Extract the real ftp command from response
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private int ParseCommand(string command)
        {
            int result = -1;
            if (command.Length > 3)
            {
                char divider = command[3];
                try
                {
                    result = int.Parse(command.Substring(0, 3));
                }
                catch (ArgumentException ae)
                {
                    Debug.Print("format error: " + ae.Message);
                }
                if (divider == '-')
                {
                    result = 0 - result;
                }
                else if (divider != ' ')
                {
                    result = -1;
                }
            }
            return result;
        }


        /// <summary>
        /// Logging into the ftp server
        /// </summary>
        /// <returns>
        ///  -1: unknown errors or exceptions happened
        ///   0: login succeed
        ///  >0: reply code tells why server rejects the login process 
        /// </returns>
        private int Login()
        {
            string command = null;
            byte[] dataBuffer = new byte[1000];
            int responseNumber = 0;
            m_StatusCode = FTPStatusCode.NotLoggedIn;
            try
            {
                CommandSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                CommandSocket.ReceiveTimeout = m_CommandTimeout;
                IPAddress hostIP = IPAddress.Parse(ServerIP);
                CommandSocket.Connect(new IPEndPoint(hostIP, ServerPort));
                responseNumber = WaitResponse();
                if (responseNumber != 220)
                {
                    // server not ready for new user login
                    return responseNumber;
                }
                command = "USER " + m_Credential.UserName + "\r\n";
                CommandSocket.Send(Encoding.UTF8.GetBytes(command));
                responseNumber = WaitResponse();
                if (responseNumber != 331)
                {
                    // user name is not allowed
                    return responseNumber;
                }
                command = "PASS " + m_Credential.Password + "\r\n";
                CommandSocket.Send(Encoding.UTF8.GetBytes(command));
                responseNumber = WaitResponse();
                if (responseNumber != 230)
                {
                    // password does not match
                    return responseNumber;
                }
                m_StatusCode = FTPStatusCode.LoggedInProceed;
                responseNumber = 0;

            }
            catch (SocketException se)
            {
                Debug.Print(se.Message);
                throw new WebException("Login fail due to connection problem.");
            }
            return responseNumber;
        }

        private IPEndPoint GetEndPoint(string response)
        {
            if (response == null)
            {
                return null;
            }
            IPEndPoint result = null;
            string[] responseArray = response.Split(new char[] { '(', ')' });
            if (responseArray.Length < 2)
            {
                return null;
            }
            string[] addrs = responseArray[1].Split(new char[] { ',' });
            if (addrs.Length != 6)
            {
                return null;
            }
            try
            {
                IPAddress addr = IPAddress.Parse(addrs[0] + "." + addrs[1] + "." + addrs[2] + "." + addrs[3]);
                int port = int.Parse(addrs[4]) * 256 + int.Parse(addrs[5]);
                result = new IPEndPoint(addr, port);
            }
            catch (ArgumentException ae)
            {
                Debug.Print("Parse error: " + ae.Message);
            }
            return result;
        }

        /// <summary>
        /// Timer callback
        /// </summary>
        /// <param name="state"></param>
        private void TimerCallback(object state)
        {
            Debug.Print("Timer hit!!");
            throw new WebException("active data connection timeout, server no response.");
        }

        /// <summary>
        /// Close the request
        /// </summary>
        internal void Close()
        {
            if (!m_IsClosed)
            {
                m_IsClosed = true;
                if (m_DataStream != null)
                {
                    m_DataStream.Close();
                    m_DataStream = null;
                }
                if (DataSocket != null)
                {
                    DataSocket.Close();
                    DataSocket = null;
                }
                if (CommandSocket != null)
                {
                    int length = CommandSocket.Available;
                    if (length > 0)
                    {
                        // clean up the command socket
                        CommandSocket.Receive(new byte[length]);
                    }
                    if (!CommandSocket.Poll(1000, SelectMode.SelectError))
                        CommandSocket.Send(Encoding.UTF8.GetBytes("QUIT\r\n"));
                    CommandSocket.Close();
                    CommandSocket = null;
                }
            }
        }
    }
}
