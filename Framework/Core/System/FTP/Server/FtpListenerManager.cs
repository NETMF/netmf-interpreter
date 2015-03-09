using System.Collections;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System;

namespace Microsoft.SPOT.Net.Ftp
{
    /// <summary>
    /// The Ftp manager:
    ///     maintain the listening port of the ftp server
    ///     maintain the listeners
    ///     maintain the ftp sessions (connections)
    ///     maintain the default listener
    ///     dispatch ftp listener contexts to the right listener
    /// </summary>
    internal class FtpListenerManager : IContextManager, IDisposable
    {
        // Fields
        private SessionPool m_Sessions = null;          // a list of active sessions
        private ArrayList m_Listeners = null;           // a list of registered listeners
        private Socket m_ListenSocket = null;           // the socket listening to the ftp port
        private IPAddress m_HostIP = IPAddress.Any;     // host address
        private int m_HostPort = 21;                    // host port (we provide the possibility to choose host port)
        private Thread m_Daemon = null;                 // the work thread
        private bool m_IsStarted = false;               // flag indicates whether the manager is running
        private FtpDefaultListener m_DefaultListener = null;
                                                        // a reference to the default listener of this ftp server

        // currently only one manager is allowed
        private static FtpListenerManager m_ManagerSingleton = null;

        // Methods
        /// <summary>
        /// Default constructor
        /// </summary>
        private FtpListenerManager()
        {
            Logging.Print(" *** MiniFTPServer ***");
            foreach (IPAddress item in System.Net.Dns.GetHostEntry("").AddressList)
            {
                m_HostIP = item;
                if (null != m_HostIP) break;
            }
            m_Sessions = new SessionPool();
            m_Listeners = new ArrayList();
            Logging.Print("Running in domain " + AppDomain.CurrentDomain.FriendlyName);
        }

        /// <summary>
        /// Singleton pattern implementation
        /// </summary>
        /// <returns></returns>
        internal static FtpListenerManager GetFtpManager()
        {
            if (m_ManagerSingleton == null)
            {
                m_ManagerSingleton = new FtpListenerManager();
            }
            return m_ManagerSingleton;
        }

        /// <summary>
        /// Static method to add listener
        /// </summary>
        /// <param name="listener"></param>
 
        internal static void AddListener(FtpListener listener)
        {
            FtpListenerManager manager = GetFtpManager();
            manager.AddLis(listener);
        }

        /// <summary>
        /// Static method to remove listener
        /// </summary>
        /// <param name="listener"></param>
        internal static void RemoveListener(FtpListener listener)
        {
            FtpListenerManager manager = GetFtpManager();
            manager.RemoveLis(listener);
        }

        /// <summary>
        /// Private method to add a listener to the listener manager
        /// </summary>
        /// <param name="listener"></param>
        private void AddLis(FtpListener listener)
        {
            m_Listeners.Add(listener);
        }

        /// <summary>
        /// Private method to remove a listener to the listener manager
        /// </summary>
        /// <param name="listener"></param>
        private void RemoveLis(FtpListener listener)
        {
            m_Listeners.Remove(listener);
            if (m_Listeners.Count == 0)
            {
                // not listeners, stop the listener manager
                this.Stop();
            }
        }

        /// <summary>
        /// Start the ftp server
        /// </summary>
        internal void Start()
        {
            if (!m_IsStarted)
            {
                // Start the socket
                Logging.Print(" Host IP: " + m_HostIP.ToString());
                m_Daemon = new Thread(WorkerThread);
                m_IsStarted = true;
                m_Daemon.Start();
                if (m_DefaultListener != null)
                {
                    if (!m_DefaultListener.IsListening)
                        m_DefaultListener.Start();
                }
                else
                {
                    m_DefaultListener = new FtpDefaultListener();
                    m_DefaultListener.Start();
                }
            }
            
        }


        #region IContextManager
        void IContextManager.AddContext(FtpListenerContext context)
        {
            AddContext(context);
        } 
        #endregion

        /// <summary>
        /// Add context to the right listener
        ///     share the same prefix
        ///     the longer prefix wins
        /// </summary>
        /// <param name="context"></param>
        internal void AddContext(FtpListenerContext context)
        {
            // Decide which listen should this context added to
            int prefixLength = -1;
            FtpListener candidateListener = null;
            foreach (FtpListener listener in m_Listeners)
            {
                if (listener.IsListening)
                {
                    int length = listener.CheckPrefix(context.Request.QueryString);
                    if (length > prefixLength)
                    {
                        candidateListener = listener;
                        prefixLength = length;
                    }
                }
            }
            if (candidateListener != null)
            {
                candidateListener.AddContext(context);
            }
            else
            {
                if (m_DefaultListener == null)
                {
                    m_DefaultListener = new FtpDefaultListener();
                    m_DefaultListener.Start();
                }
                m_DefaultListener.AddContext(context);
            }
        }

        /// <summary>
        /// Work thread to handle new ftp sessions
        /// </summary>
        private void WorkerThread()
        {
            m_ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                m_ListenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            }
            catch{}

            IPAddress hostIp = new IPAddress(new byte[] { 0, 0, 0, 0 }); //IPAddress.Any;
            IPEndPoint ep = new IPEndPoint(hostIp, m_HostPort);

            m_ListenSocket.Bind(ep);
            m_ListenSocket.Listen(0);
            Socket aSocket = null;
            try
            {
                aSocket = m_ListenSocket.Accept();

                try
                {
                    aSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                }
                catch{}

                while (m_IsStarted)
                {
                    // remove dead connections
                    m_Sessions.CheckAlive();

                    // too many open connections
                    if (m_Sessions.Count >= m_Sessions.Capacity)
                    {
                        aSocket.Close();
                        Logging.Print("Too many open connections");
                    }
                    else
                    {
                        m_Sessions.AddSession(new FtpListenerSession(aSocket, this));
                        Logging.Print("number of active sessions: " + m_Sessions.Count);
                    }

                    aSocket = m_ListenSocket.Accept();
                }
            }
            catch (SocketException)
            { }
        }

        /// <summary>
        /// Stop the ftp server
        /// </summary>
        public void Stop()
        {
            if (!m_IsStarted)
                return;

            m_IsStarted = false;
            m_Sessions.Clear();
            if (m_DefaultListener != null)
            {
                m_DefaultListener.Stop();
            }
            try
            {
                if (m_ListenSocket != null)
                {
                    m_ListenSocket.Close();
                }
            }
            catch { /*ignored*/ }
            m_ListenSocket = null;

            try
            {
                if (m_Daemon.IsAlive && !m_Daemon.Join(500))
                {
                    m_Daemon.Abort();
                }
            }
            catch { /*ignored*/ }
            m_Daemon = null;
        }

        // Properties
        /// <summary>
        /// Retrive the list of listeners
        /// </summary>
        internal ArrayList Listeners
        {
            get { return m_Listeners; }
        }

        /// <summary>
        /// Get or set the default listener
        /// </summary>
        internal FtpDefaultListener DefaultListener
        {
            get { return m_DefaultListener; }
            set
            {
                if (m_IsStarted)
                {
                    throw new InvalidOperationException("Ftp server is running, cannot change default listener.");
                }
                else
                {
                    m_DefaultListener = value;
                }
            }
        }


        #region IDisposable Members

        public void Dispose()
        {
            this.Stop();
            m_DefaultListener = null;
        }

        #endregion
    }
}
