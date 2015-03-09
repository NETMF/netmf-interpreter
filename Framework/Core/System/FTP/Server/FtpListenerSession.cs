using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Net;
using System;

namespace Microsoft.SPOT.Net.Ftp
{

    internal sealed class FtpListenerSession : IDisposable, IDataManager
    {
        // Fields
        internal int SessionId = -1;                        // the position of this session in the session pool
        private FilePath m_CurrentDirectory = new FilePath("/");
                                                            // default current path

        private Socket m_CommandSocket = null;              // the socket to exchange commands
        private Socket m_DataSocket = null;                 // the socket to exchange data
        private Socket m_ListenSocket = null;               // the socket to listen to the data connection

        private Thread m_CommandThread = null;              // the worker thread of commands handling
        private Thread m_DataThread = null;                 // the worker thread to establish data connection

        private NetworkStream m_DataStream = null;          // data stream from data socket

        private IContextManager m_SendContext = null;       // context manager which used to add contexts to listeners
        private AutoResetEvent m_DataReadyEvent = null;     // the event to say that the pasv data connection starts listening
        private ManualResetEvent m_DataChannelEstablished = null;
                                                            // the event to say that the data connection has been established
                                                            

        private bool m_PassThreadW = false;                 // flag for passive thread status
        private bool m_ActThreadW = false;                  // flag for active thread status
        private bool m_Welcomed = false;                    // flag for whether welcome information has been sent
        private bool m_IsAlive = true;                      // the status of this session
        private bool m_DataReady = false;                   // flag for data socket status
        private bool m_IsDisposed = false;                  // flag for whether the session is disposed

        private IPAddress m_HostIp = null;                  // host ip address
        private int m_HostPort = 0;                         // host port
        private IPAddress m_ClientIp = null;                // client ip address
        private int m_ClientPort = 0;                       // client port
        private object m_SyncRoot = new object();           // the lock

        private const string m_BadSequence = "503 Bad sequence of commands.\r\n";
                                                            // the response message of bad sequence
                                                            // TODO: rearrange those messages and put them to a seperated class


        private bool m_DataModeON                           // flag for listen socket status
        {
            get
            {
                return (m_PassThreadW || m_ActThreadW);
            }
        }

        // Methods
        internal FtpListenerSession(Socket control, IContextManager context)
        {
            m_CommandSocket = control;
            m_SendContext = context;
            Logging.Print("New connection established from " + control.RemoteEndPoint);
            m_HostIp = (control.LocalEndPoint as IPEndPoint).Address;
            m_DataReadyEvent = new AutoResetEvent(false);
            m_DataChannelEstablished = new ManualResetEvent(false);
            m_CommandThread = new Thread(WorkerThread);
            m_CommandThread.Start();
        }

        /// <summary>
        /// Close both active and passive data connection if opened
        /// </summary>
        private void CloseDataChannel()
        {
            Logging.Print("hit close data channel");
            if (m_DataStream != null)
            {
                m_DataStream.Close();
                m_DataStream = null;
            }
            if (m_ListenSocket != null)
            {
                m_ListenSocket.Close();
                m_ListenSocket = null;
            }
            if (m_DataSocket != null)
            {
                m_DataSocket.Close();
                m_DataSocket = null;
            }
            if (m_PassThreadW == true)
            {
                m_PassThreadW = false;
            }
            else if (m_ActThreadW == true)
            {
                m_ActThreadW = false;
            }
            m_DataReady = false;
        }

        /// <summary>
        /// Data thread running in passive mode
        /// </summary>
        private void PassiveThread()
        {
            m_HostPort = 0;
            Logging.Print("Passive Mode Activate");

            if (m_IsDisposed)
            {
                return;
            }

            // clean up data socket
            if (m_DataSocket != null)
            {
                m_DataSocket.Close();
                m_DataSocket = null;
            }

            IPAddress hostIp = IPAddress.Any;
            IPEndPoint ep = new IPEndPoint(hostIp, m_HostPort);
            try
            {
                m_ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                m_ListenSocket.Bind(ep);
                m_ListenSocket.Listen(0);
                m_HostPort = (m_ListenSocket.LocalEndPoint as IPEndPoint).Port;
                m_PassThreadW = true;
            }
            catch (SocketException se)
            {
                Logging.Print(se.Message);
            }
            finally
            {
                m_DataReadyEvent.Set();
            }
            try
            {
                m_DataSocket = m_ListenSocket.Accept();
                m_DataStream = new NetworkStream(m_DataSocket);
                lock (m_SyncRoot)
                {
                    m_DataReady = true;
                }
                m_DataChannelEstablished.Set();
            }
            catch (SocketException se)
            {
                Logging.Print("Data Socket Exception: " + se.ErrorCode.ToString());
        
            }
            finally
            {
                // do not need listen socket any longer
                Logging.Print("Passive Thread exit");
                m_ListenSocket.Close();
                m_ListenSocket = null;
            }
        }

        /// <summary>
        /// Data thread running in active mode
        /// </summary>
        private void ActiveThread()
        {
            Logging.Print("Active Mode Activate");

            if (m_IsDisposed)
            {
                return;
            }

            // clean up data socket
            if (m_DataSocket != null)
            {
                m_DataSocket.Close();
                m_DataSocket = null;
            }
            try
            {
                m_DataSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                m_DataSocket.Connect(new IPEndPoint(m_ClientIp, m_ClientPort));
                m_DataStream = new NetworkStream(m_DataSocket);
                lock (m_SyncRoot)
                {
                    m_DataReady = true;
                }
                m_DataChannelEstablished.Set();
            }
            catch (SocketException se)
            {
                Logging.Print("Data Socket Exception: " + se.ErrorCode.ToString());
        
            }
            finally
            {
                // do not need listen socket any longer
                Logging.Print("Active Thread exit");
            }
        }

        /// <summary>
        /// Control thread
        /// TODO: separate the functionalities into functions to reduce maintenance cost
        /// </summary>
        private void WorkerThread()
        {
            FtpCommand command;
            FtpState state = FtpState.WaitUser;

            string tempName = null;         // store old name for "rename from" command
            string response;
            int timeout = 0;

            try
            {
                while (m_IsAlive)
                {
                    if (m_IsDisposed)
                    {
                        break;
                    }
                    else if (!m_Welcomed)
                    {
                        response = "220 MyFTP " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() +
                            " Server [" + m_HostIp.ToString() + "] \r\n";
                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                        m_Welcomed = true;
                    }
                    else if (m_CommandSocket == null)
                    {
                        Logging.Print("Session socket has been closed.");
                        break;
                    }
                    else if (m_CommandSocket.Poll(1000, SelectMode.SelectRead))
                    {
                        if (m_CommandSocket.Available == 0)
                        {
                            Logging.Print("REMOTE DISCONNECT " + m_CommandSocket.RemoteEndPoint.ToString());
                            m_IsAlive = false;
                            break;
                        }
                        // read until find the end of a line
                        command = FtpCommandCreator.Create(ReadCommand());

                        if (command == null)
                        {
                            break;
                        }
                        else
                            switch (command.Type)
                            {
                                case FtpCommandType.User:
                                    response = "331 " + command.Content + " login ok \r\n";
                                    m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                    state = FtpState.WaitPwd;
                                    tempName = command.Content;
                                    break;
                                case FtpCommandType.Pass:
                                    if (state != FtpState.WaitPwd)
                                    {
                                        response = "332 Need Account for Login.\r\n";
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                    else 
                                    {
                                        UserAuthenticatorArgs args = new UserAuthenticatorArgs(tempName, command.Content);
                                        FtpListener.RaiseEvent(this, args);
                                        if (args.Result == UserAuthenticationResult.Approved)
                                        {
                                            User.UserName = tempName;
                                            User.PassWord = command.Content;
                                            response = "230 access granted for " + User.UserName + ", restrictions apply. \r\n";
                                            m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                            state = FtpState.WaitCommand;
                                            break;
                                        }
                                        else
                                        {
                                            response = "530 Login incorrect. \r\n";
                                            m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                            state = FtpState.WaitCommand;
                                            break;
                                        }
                                    }
                                case FtpCommandType.Cwd:
                                    if (state == FtpState.WaitPwd)
                                    {
                                        response = "331 Need Password.\r\n";
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                    else if (state == FtpState.WaitUser)
                                    {
                                        response = "332 Need Account for Login.\r\n";
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                    else if (state == FtpState.WaitCommand)
                                    {
                                        if (command.Content == null)
                                        {
                                            response = m_BadSequence;
                                            m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                            break;
                                        }
                                        else
                                        {
                                            FtpListenerRequest request = new FtpListenerRequest(WebRequestMethodsEx.Ftp.ChangeDirectory, m_CurrentDirectory.Combine(command.Content).GetNetPath(), this);
                                            FtpListenerContext context = new FtpListenerContext(this, request);
                                            m_SendContext.AddContext(context);
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        response = m_BadSequence;
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                case FtpCommandType.Quit:
                                    m_IsAlive = false;
                                    CloseDataChannel();
                                    response = "221 Goodbye.\r\n";
                                    m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                    m_CommandSocket.Close();
                                    m_CommandSocket = null;
                                    break;
                                case FtpCommandType.Pasv:
                                    if (state == FtpState.WaitPwd)
                                    {
                                        response = "331 Need Password.\r\n";
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                    else if (state == FtpState.WaitCommand)
                                    {
                                        CloseDataChannel();
                                        m_DataThread = new Thread(PassiveThread);
                                        m_DataThread.Start();
                                        m_DataReadyEvent.WaitOne();                // wait until port has been successfully assigned

                                        byte[] addrs = m_HostIp.GetAddressBytes();

                                        int upper = m_HostPort / 256;
                                        int lower = m_HostPort % 256;
                                        if (addrs.Length == 4) //IPv4
                                        {
                                            response = "227 Entering Passive Mode (";
                                            foreach (int i in addrs)
                                            {
                                                response += i.ToString() + ",";
                                            }
                                            response += upper.ToString() + "," + lower.ToString() + ")\r\n";
                                        }
                                        else // currently do not support IPv6
                                        {
                                            throw new NotImplementedException("currently does not support IPv6");
                                        }
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                    else
                                    {
                                        response = m_BadSequence;
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                case FtpCommandType.Type:
                                    if (state == FtpState.WaitPwd)
                                    {
                                        response = "331 Need Password.\r\n";
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                    else if (state == FtpState.WaitCommand)
                                    {
                                        response = "200 Command OK.\r\n";
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                    else
                                    {
                                        response = m_BadSequence;
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                case FtpCommandType.List:
                                    if (!m_DataModeON)
                                    {
                                        response = "425 Use PORT or PASV first.\r\n";
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                    else if (state == FtpState.WaitCommand)
                                    {
                                        response = "150 Opening UTF8 mode data connection for *** \r\n";
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        FtpListenerRequest request = new FtpListenerRequest(WebRequestMethods.Ftp.ListDirectoryDetails, m_CurrentDirectory.Combine(command.Content).GetNetPath(), this);
                                        FtpListenerContext context = new FtpListenerContext(this, request);
                                        m_SendContext.AddContext(context);
                                        break;
                                    }
                                    else
                                    {
                                        response = m_BadSequence;
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                case FtpCommandType.NList:
                                    if (!m_DataModeON)
                                    {
                                        response = "425 Use PORT or PASV first.\r\n";
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                    else if (state == FtpState.WaitCommand)
                                    {
                                        response = "150 Opening UTF8 mode data connection for *** \r\n";
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        FtpListenerRequest request = new FtpListenerRequest(WebRequestMethods.Ftp.ListDirectory, m_CurrentDirectory.Combine(command.Content).GetNetPath(), this);
                                        FtpListenerContext context = new FtpListenerContext(this, request);
                                        m_SendContext.AddContext(context);
                                        break;
                                    }
                                    else
                                    {
                                        response = m_BadSequence;
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                case FtpCommandType.Port:
                                    if (state == FtpState.WaitPwd)
                                    {
                                        response = "331 Need Password.\r\n";
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                    else if (state == FtpState.WaitCommand && command.Content != null)
                                    {
                                        CloseDataChannel();
                                        string[] epad = command.Content.Split(new char[] { ',' });
                                        if (epad.Length != 6)
                                        {
                                            response = "500 Invalid PORT command.\r\n";
                                            m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        }
                                        else
                                        {
                                            try
                                            {
                                                m_ClientIp = IPAddress.Parse(epad[0] + "." + epad[1] + "." + epad[2] + "." + epad[3]);

                                                m_ClientPort = Int32.Parse(epad[4]) * 256 + Int32.Parse(epad[5]);
                                                if (m_ClientPort <= 0 || m_ClientPort > 65535)
                                                {
                                                    response = "500 Invalid PORT command.\r\n";
                                                    m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                                }
                                                else
                                                {
                                                    m_ActThreadW = true;
                                                    m_DataThread = new Thread(ActiveThread);
                                                    m_DataThread.Start();

                                                    response = "200 PORT command successful.\r\n";
                                                    m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                                }
                                            }
                                            catch (Exception)
                                            {
                                                response = "500 Invalid PORT command.\r\n";
                                                m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                            }
                                        }
                                        break;
                                    }
                                    else
                                    {
                                        response = m_BadSequence;
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                case FtpCommandType.Sys:
                                    if (state == FtpState.WaitPwd)
                                    {
                                        response = "331 Need Password.\r\n";
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                    else if (state == FtpState.WaitCommand)
                                    {
                                        response = "215 System Type: EMIC\r\n";
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                    else
                                    {
                                        response = m_BadSequence;
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                case FtpCommandType.Feature:
                                    if (state == FtpState.WaitPwd)
                                    {
                                        response = "331 Need Password.\r\n";
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                    else if (state == FtpState.WaitCommand)
                                    {
                                        response = "211-Features:\r\n SIZE\r\n211 End\r\n";
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                    else
                                    {
                                        response = m_BadSequence;
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                case FtpCommandType.Pwd:
                                    if (state == FtpState.WaitPwd)
                                    {
                                        response = "331 Need Password.\r\n";
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                    else if (state == FtpState.WaitCommand)
                                    {
                                        response = "257 \"" + m_CurrentDirectory.GetNetPath() + "\" is the current directory. \r\n";
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                    else
                                    {
                                        response = m_BadSequence;
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                case FtpCommandType.Retr:
                                    if (state == FtpState.WaitPwd)
                                    {
                                        response = "331 Need Password.\r\n";
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                    else if (state == FtpState.WaitCommand)
                                    {
                                        if (!m_DataModeON)
                                        {
                                            response = "425 Use PORT or PASV first.\r\n";
                                            m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                            break;
                                        }
                                        response = "150 Opening BINARY mode data connection for " + command.Content + ". \r\n";
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        FtpListenerRequest request = new FtpListenerRequest(WebRequestMethods.Ftp.DownloadFile, m_CurrentDirectory.Combine(command.Content).GetNetPath(), this);
                                        FtpListenerContext context = new FtpListenerContext(this, request);
                                        m_SendContext.AddContext(context);
                                        break;
                                    }
                                    else
                                    {
                                        response = m_BadSequence;
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                case FtpCommandType.Opts:
                                    if (state == FtpState.WaitPwd)
                                    {
                                        response = "331 Need Password.\r\n";
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                    else if (state == FtpState.WaitCommand)
                                    {
                                        if (command.Content == "utf8")
                                            response = "213 Always in utf8 mode \r\n";
                                        else
                                            response = "550 Requested action not taken. Mode not support. \r\n";
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                    else
                                    {
                                        response = m_BadSequence;
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                case FtpCommandType.Size: 
                                    if (state == FtpState.WaitPwd)
                                    {
                                        response = "331 Need Password.\r\n";
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                    else if (state == FtpState.WaitCommand)
                                    {
                                        FtpListenerRequest request = new FtpListenerRequest(WebRequestMethods.Ftp.GetFileSize, m_CurrentDirectory.Combine(command.Content).GetNetPath(), this);
                                        FtpListenerContext context = new FtpListenerContext(this, request);
                                        m_SendContext.AddContext(context);
                                        break;
                                    }
                                    else
                                    {
                                        response = m_BadSequence;
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                case FtpCommandType.Store:
                                    if (state == FtpState.WaitPwd)
                                    {
                                        response = "331 Need Password.\r\n";
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                    else if (state == FtpState.WaitCommand)
                                    {
                                        if (!m_DataModeON)
                                        {
                                            response = "425 Use PORT or PASV first.\r\n";
                                            m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                            break;
                                        }
                                        response = "150 Opening BINARY mode data connection for " + command.Content + ". \r\n";
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        FtpListenerRequest request = new FtpListenerRequest(WebRequestMethods.Ftp.UploadFile, m_CurrentDirectory.Combine(command.Content).GetNetPath(), this);
                                        FtpListenerContext context = new FtpListenerContext(this, request);
                                        m_SendContext.AddContext(context);
                                        break;
                                    }
                                    else
                                    {
                                        response = m_BadSequence;
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                case FtpCommandType.Noop:
                                    response = "200 NOOP command successful. \r\n";
                                    m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                    break;
                                case FtpCommandType.Delete:
                                    if (state == FtpState.WaitPwd)
                                    {
                                        response = "331 Need Password.\r\n";
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                    else if (state == FtpState.WaitCommand)
                                    {
                                        FtpListenerRequest request = new FtpListenerRequest(WebRequestMethods.Ftp.DeleteFile, m_CurrentDirectory.Combine(command.Content).GetNetPath(), this);
                                        FtpListenerContext context = new FtpListenerContext(this, request);
                                        m_SendContext.AddContext(context);
                                        break;
                                    }
                                    else
                                    {
                                        response = m_BadSequence;
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                case FtpCommandType.MkDir:
                                    if (state == FtpState.WaitPwd)
                                    {
                                        response = "331 Need Password.\r\n";
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                    else if (state == FtpState.WaitCommand)
                                    {
                                        FtpListenerRequest request = new FtpListenerRequest(WebRequestMethods.Ftp.MakeDirectory, m_CurrentDirectory.Combine(command.Content).GetNetPath(), this);
                                        FtpListenerContext context = new FtpListenerContext(this, request);
                                        m_SendContext.AddContext(context);
                                        break;
                                    }
                                    else
                                    {
                                        response = m_BadSequence;
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                case FtpCommandType.Rmd:
                                    if (state == FtpState.WaitPwd)
                                    {
                                        response = "331 Need Password.\r\n";
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                    else if (state == FtpState.WaitCommand)
                                    {
                                        FtpListenerRequest request = new FtpListenerRequest(WebRequestMethods.Ftp.RemoveDirectory, m_CurrentDirectory.Combine(command.Content).GetNetPath(), this);
                                        FtpListenerContext context = new FtpListenerContext(this, request);
                                        m_SendContext.AddContext(context);
                                        break;
                                    }
                                    else
                                    {
                                        response = m_BadSequence;
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                case FtpCommandType.Rnfr:
                                    if (state == FtpState.WaitPwd)
                                    {
                                        response = "331 Need Password.\r\n";
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                    else if (state == FtpState.WaitCommand)
                                    {
                                        FtpListenerRequest request = new FtpListenerRequest(WebRequestMethodsEx.Ftp.RenameFrom, m_CurrentDirectory.Combine(command.Content).GetNetPath(), this);
                                        FtpListenerContext context = new FtpListenerContext(this, request);
                                        m_SendContext.AddContext(context);
                                        break;
                                    }
                                    else
                                    {
                                        response = m_BadSequence;
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                case FtpCommandType.Rnto:
                                    if (state == FtpState.WaitPwd)
                                    {
                                        response = "331 Need Password.\r\n";
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                    else if (state == FtpState.WaitCommand)
                                    {
                                        FtpListenerRequest request = new FtpListenerRequest(WebRequestMethodsEx.Ftp.RenameTo, m_CurrentDirectory.Combine(command.Content).GetNetPath(), this);
                                        FtpListenerContext context = new FtpListenerContext(this, request);
                                        m_SendContext.AddContext(context);
                                        break;
                                    }
                                    else
                                    {
                                        response = m_BadSequence;
                                        m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                        break;
                                    }
                                default:
                                    response = "502 Command not implemented. \r\n";
                                    if (state == FtpState.WaitPwd)
                                    {
                                        state = FtpState.WaitUser;
                                    }
                                    m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                                    break;
                            }
                        // reset time out
                        timeout = 0;
                    }
                    else if (m_CommandSocket.Poll(1000, SelectMode.SelectError))
                    {
                        Logging.Print("Disconnected unproperly.");
                        break;
                    }
                    else
                    {
                        if (!m_Welcomed)
                        {
                            response = "220 MyFTP " + typeof(FtpListenerSession).Assembly.GetName().Version.ToString() +
                                " Server [" + m_HostIp.ToString() + "] \r\n";
                            m_CommandSocket.Send(Encoding.UTF8.GetBytes(response));
                            m_Welcomed = true;
                        }
                        else
                        {
                            if (SessionTimeOut != -1)
                            {
                                if (timeout > SessionTimeOut)
                                {
                                    m_IsAlive = false;
                                    Logging.Print("Connection time out.");
                                    break;
                                }
                                timeout += 50;
                            }
                            Thread.Sleep(50);
                        }
                    }
                }
            }
            catch (SocketException se)
            {
                Logging.Print("Control Socket Exception : " + se.ErrorCode.ToString());
            }
            catch (IOException ioe)
            {
                Logging.Print("IOException: " + ioe.Message);
            }
            finally
            {
                m_IsAlive = false;
                if (m_CommandSocket != null)
                {
                    m_CommandSocket.Close();
                    m_CommandSocket = null;
                }
                if (m_ListenSocket != null)
                {
                    m_ListenSocket.Close();
                    m_CommandSocket = null;
                }
                Logging.Print("Client Disconnected.");
            }
        }

        /// <summary>
        /// read from the command socket 
        /// </summary>
        /// <returns></returns>
        private string ReadCommand()
        {
            int idxEndLine = -1;
            string receiveBuffer = "";
            int timeCountDown = SessionTimeOut;
            int len = 0;
            byte[] buffer;
            while (len == 0 || idxEndLine < 0)
            {
                if (len == 0)
                {
                    Thread.Sleep(50);
                    timeCountDown -= 50;
                }
                else
                {
                    timeCountDown = SessionTimeOut;
                }
                if (timeCountDown <= 0)
                {
                    m_IsAlive = false;
                    break;
                }
                len = m_CommandSocket.Available;
                buffer = new byte[len];
                len = m_CommandSocket.Receive(buffer, len, SocketFlags.None);
                receiveBuffer += new string(Encoding.UTF8.GetChars(buffer));
                idxEndLine = receiveBuffer.IndexOf('\n');
            }
            if (timeCountDown <= 0)
            {
                return null;
            }
            receiveBuffer = receiveBuffer.Substring(0, idxEndLine - 1);
            Logging.Print("Get: " + receiveBuffer + " :" + SessionId.ToString() + " in domain " + AppDomain.CurrentDomain.FriendlyName);
            return receiveBuffer;
        }


        // public properties
        internal readonly int SessionTimeOut = 60000;     // 1 min

        /// <summary>
        /// Get the host ip address
        /// </summary>
        internal IPAddress HostIp
        {
            get { return m_HostIp; }
        }

        /// <summary>
        /// Get the server port of this session
        /// </summary>
        internal int HostPort
        {
            get { return m_HostPort; }
        }

        /// <summary>
        /// Indicate whether this session is still alive
        /// </summary>
        internal bool IsAlive
        {
            get { return m_IsAlive; }
        }

        /// <summary>
        /// lock
        /// </summary>
        internal object SyncRoot
        {
            get { return m_SyncRoot; }
        }

        /// <summary>
        /// flag indicates whether the data stream is ready to use
        /// </summary>
        internal bool IsDataStreamAvailable
        {
            get
            {
                bool result;
                lock (m_SyncRoot)
                {
                    result = m_DataReady;
                }
                return result;
            }
            set
            {
                if (value == true)
                {
                    throw new NotSupportedException("Cannot assign this property to true.");
                }
                else
                {
                    lock (m_SyncRoot)
                    {
                        m_DataReady = false;
                    }
                    if (m_DataSocket != null)
                    {
                        m_DataSocket.Close();
                        m_DataSocket = null;
                    }
                    m_DataChannelEstablished.Reset();
                }
            }
        }

        #region IDataManager
        bool IDataManager.IsDataStreamAvailable
        {
            get
            {
                return IsDataStreamAvailable;
            }
            set
            {
                IsDataStreamAvailable = value;
            }
        }

        Stream IDataManager.DataStream
        {
            get
            {
                if (IsDataStreamAvailable)
                {
                    return m_DataStream;
                }
                else
                {
                    return null;
                }
            }
        }

        ManualResetEvent IDataManager.DataChannelEstablished
        {
            get
            {
                return m_DataChannelEstablished;
            }
        }

        void IDataManager.SendResponse(string message)
        {
            if (m_IsAlive && !m_IsDisposed && m_CommandSocket != null)
            {
                m_CommandSocket.Send(Encoding.UTF8.GetBytes(message));
            }
        }

        void IDataManager.ChangeCurrentDirectory(FilePath path)
        {
            m_CurrentDirectory = path;
        }

        void IDataManager.CloseDataChannel()
        {
            CloseDataChannel();
        }
        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            m_IsDisposed = true;
            if (m_DataThread != null && m_DataThread.IsAlive)
            {
                if (m_DataSocket != null)
                {
                    m_DataSocket.Close();
                    m_DataSocket = null;
                }
                if (m_ListenSocket != null)
                {
                    m_ListenSocket.Close();
                    m_ListenSocket = null;
                }
                if (!m_DataThread.Join(1000))
                {
                    m_DataThread.Abort();
                }
            }
            if (m_CommandThread != null && m_CommandThread.IsAlive)
            {
                if (m_CommandSocket != null)
                {
                    m_CommandSocket.Close();
                    m_CommandSocket = null;
                }
                if (!m_CommandThread.Join(1000))
                {
                    m_CommandThread.Abort();
                }
            }
        }

        #endregion

        internal UserInfo User = new UserInfo();
    }

    /// <summary>
    /// User information containing username and password
    /// </summary>
    public class UserInfo
    {
        public string UserName { get; internal set; }
        public string PassWord { get; internal set; }
    }
}
