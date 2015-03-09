using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Net;
using Microsoft.SPOT.Net.NetworkInformation;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public enum TestType
    {
        Linger = 0,
        ReceiveTimeout,
        SendTimeout,
        SendReceiveBufferSize,
        NoDelay,
        None
    }


    public class SocketServer : Socket
    {
        private const Int32 c_microsecondsPerSecond = 1000000;
        private bool _running = false;
        private Thread _serverThread;
        public TestType testId = TestType.None;
        public MFTestResults testResult;
        private bool m_linger;

        public SocketServer(TestType testType, AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType) : base (addressFamily, socketType, protocolType)
        {
            testId = testType;
            m_linger = false;
        }

        /// <summary>
        /// Start processing connections
        /// </summary>
        public void Start()
        {
            this.Stop();
            _running = true;
            _serverThread = new Thread(new ThreadStart(StartServer));
            _serverThread.Start();
        }

        /// <summary>
        /// Stop processing new connections
        /// </summary>
        public void Stop()
        {
            // If server is running, stop it first
            if (_running)
            {
                _running = false;
                _serverThread.Abort();
                _serverThread.Join();
            }
        }

        /// <summary>
        /// Internal thread that handles all communications.
        /// </summary>
        private void StartServer()
        {
            Socket lingerSock = null;
            try
            {
                while (_running)
                {
                    // Wait for a client to connect
                    Socket clientSocket = this.Accept();
                    {
                        Log.Comment("Starting server with the tests type " + testId.ToString());
                        switch (testId)
                        {
                            case TestType.Linger:
                                lingerSock = clientSocket;
                                Linger(lingerSock);
                                break;
                            case TestType.NoDelay:
                            case TestType.ReceiveTimeout:
                            case TestType.SendReceiveBufferSize:
                            case TestType.SendTimeout:
                                clientSocket.Close();
                                break;
                        }
                    }
                }
            }
            catch (ThreadAbortException)
            {
            }
            finally
            {
                if (lingerSock != null)
                {
                    lingerSock.Close();
                }
                _running = false;
            }
        }

        public bool LingerValue
        {
            set
            {
                m_linger = value;
            }
        }

        public void Linger(Socket sock)
        {
            try
            {
                byte[] bytesReceived = new byte[400];
                sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, m_linger ? 3000: 0 );
                int cnt = sock.Receive(bytesReceived);
                sock.Send(bytesReceived, cnt, SocketFlags.None);
                testResult = MFTestResults.Pass;
                sock.Close();
            }
            catch
            {
            }
        }
    }
}
