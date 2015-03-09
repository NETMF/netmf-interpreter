using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using Microsoft.SPOT.Net.NetworkInformation;

namespace Microsoft.SPOT.Samples.Sockets
{
    public class PingPong
    {
        Socket socketServer;
        Thread udpThread;

        private long DottedDecimalToIp(byte a1, byte a2, byte a3, byte a4)
        {
            return (long)((ulong)a4 << 24 | (ulong)a3 << 16 | (ulong)a2 << 8 | (ulong)a1); 
        }

        private byte[] StringToByteArray(string txt)
        {
            char[] cdata = txt.ToCharArray();
            byte[] data = new byte[cdata.Length];

            for (int i = 0; i < cdata.Length; i++)
            {
                data[i] = (byte)cdata[i];
            }
            return data;
        }

        private void SocketThread()
        {
            byte[] buff = new byte[1024];
            Socket s = m_sockets[m_sockets.Count - 1] as Socket;
            m_evt.Set();

            try
            {
                s.Send(StringToByteArray("Socket " + m_sockets.Count + " created\r\n"));

                while (true)
                {
                    int len = s.Receive(buff);
                    if (len > 0)
                    {
                        s.Send(buff, len, SocketFlags.None);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch
            {
            }
            finally
            {
                try
                {
                    s.Close();
                }
                catch { }
                m_socketCloseEvt.Set();
                lock (m_sockets)
                {
                    m_sockets.Remove(s);
                }
            }
        }
        private AutoResetEvent m_evt = new AutoResetEvent(false);
        private AutoResetEvent m_socketCloseEvt = new AutoResetEvent(false);
        private ArrayList m_sockets = new ArrayList();
        private ArrayList m_threads = new ArrayList();
        public void StartServer()
        {
            socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            NetworkInterface[] nis = NetworkInterface.GetAllNetworkInterfaces();
            IPAddress addr = null;

            foreach (NetworkInterface ni in nis)
            {
                if (ni.IPAddress.Length <= 15)
                {
                    addr = IPAddress.Parse(ni.IPAddress);
                    break;
                }
            }

            if (addr == null) return;

            Debug.Print("Device IP Address is: " + addr.ToString());

            socketServer.Bind(new IPEndPoint(addr, 33555));
            //socketServer.Bind(new IPEndPoint(DottedDecimalToIp(127, 0, 0, 1), 123));
            //socketServer.Bind(new IPEndPoint(IPAddress.Any, 0));
            //socketServer.Bind(new IPEndPoint(DottedDecimalToIp(192, 168, 187, 163), 0));

            int retries = (int)Debug.GC(false) / 1000;

            socketServer.Listen(1);

            try
            {
                while (true)
                {
                    try
                    {
                        Debug.Print("Waiting for client to connect");
                        Socket s = socketServer.Accept();

                        s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, 0);
                        lock (m_sockets)
                        {
                            m_sockets.Add(s);
                        }
                        Debug.Print("Socket " + m_sockets.Count + " connected");
                        Thread th = new Thread(new ThreadStart(SocketThread));
                        m_evt.Reset();
                        th.Start();
                        m_threads.Add(th);
                        m_evt.WaitOne(500, true);

                        foreach (Thread th2 in m_threads.ToArray())
                        {
                            if (!th2.IsAlive)
                            {
                                m_threads.Remove(th2);
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        Debug.Print("Exception: " + e.ToString());
                        m_socketCloseEvt.WaitOne(500, true);
                    }
                    if (retries <= 0)
                    {
                        retries = (int)Debug.GC(true) / 1000;
                    }
                    else
                    {
                        retries -= 100;
                    }
                }
            }
            finally
            {
                foreach (Thread th in m_threads)
                {
                    if (th.IsAlive)
                    {
                        th.Abort();
                        th.Join();
                    }
                }
                socketServer.Close();
            }
        }

        void UdpThread()
        {
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            NetworkInterface ni = NetworkInterface.GetAllNetworkInterfaces()[0];

            IPEndPoint localEP = new IPEndPoint(IPAddress.Parse(ni.IPAddress), 54321);
            listener.Bind(localEP);

            byte[] data = new byte[1024];


            while (true)
            {
                try
                {
                    int len;
                    EndPoint ep = new IPEndPoint(IPAddress.Any, 0);
                    if (0 < (len = listener.ReceiveFrom(data, SocketFlags.None, ref ep)))
                    {
                        ep = new IPEndPoint(((IPEndPoint)ep).Address, 1237);

                        listener.SendTo(data, len, SocketFlags.None, ep);
                    }
                }
                catch
                {
                    Thread.Sleep(100);
                }
            }
        }

        public void StartUDP()
        {
            udpThread = new Thread(new ThreadStart(UdpThread));

            udpThread.Start();
        }

        public void WaitForNetwork()
        {
            while (true)
            {
                NetworkInterface[] nis = NetworkInterface.GetAllNetworkInterfaces();

                if (nis[0].IPAddress != "192.168.5.100" && nis[0].IPAddress != "0.0.0.0")
                    break;

                Thread.Sleep(1000);
            }
        }


        static void Main(string[] args)
        {
            PingPong png = new PingPong();

            png.WaitForNetwork();

            png.StartUDP();
            png.StartServer();
            Debug.Print("Ping Pong Server Exited!");
        }
    }
}
