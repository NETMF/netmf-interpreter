using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Microsoft.SPOT.Platform.Tests
{
    public class SocketTools
    {
        static public Socket socketClient;
        static public Socket socketServer;
        static public byte[] bufSend;
        static public byte[] bufReceive;
        static public IPEndPoint epClient;
        static public IPEndPoint epServer;

        public static void Startup(ProtocolType protocolType, SocketType socketType, int size)
        {
            TearDown();

            socketClient = new Socket(AddressFamily.InterNetwork, socketType, protocolType);
            socketServer = new Socket(AddressFamily.InterNetwork, socketType, protocolType);

            socketClient.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            socketServer.Bind(new IPEndPoint(IPAddress.Loopback, 0));

            epClient = (IPEndPoint)socketClient.LocalEndPoint;
            epServer = (IPEndPoint)socketServer.LocalEndPoint;

            bufSend = new byte[size];
            for (int i = 0; i < bufSend.Length; i++)
            {
                bufSend[i] = 1;                
            }
            
            bufReceive = new byte[bufSend.Length];
        }

        public static void TearDown()
        {
            CloseSocket(ref socketClient);
            CloseSocket(ref socketServer);
        }

        private static void CloseSocket(ref Socket socket)
        {
            if (socket != null)
            {
                socket.Close();
                socket = null;
            }
        }
        public static void AssertDataReceived(int cBytes)
        {
            if (cBytes != bufSend.Length)
                throw new Exception("Recieve failed, wrong size " + cBytes + " " + bufSend.Length);

            for (int i = 0; i < bufReceive.Length; i++)
            {
                if (bufSend[i] != bufReceive[i])
                    throw new Exception("Recieve failed, wrong data");
            }
        }

        public static long DottedDecimalToIp(byte a1, byte a2, byte a3, byte a4)
        {
            return (long)((ulong)a4 << 24 | (ulong)a3 << 16 | (ulong)a2 << 8 | (ulong)a1);
        }


        public static bool ArrayEquals(bool[] array1, bool[] array2)
        {
            if (array1.Length != array2.Length)
                return false;
            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                    return false;
            }
            return true;
        }

        public static bool ArrayEquals(byte[] array1, byte[] array2)
        {
            if (array1.Length != array2.Length)
                return false;
            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                    return false;
            }
            return true;
        }

        public static void AddConnection()
        {
            try
            {
                Socket socketAdd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socketAdd.Bind(new IPEndPoint(IPAddress.Loopback, 0));
                EndPoint epAdd = (IPEndPoint)socketAdd.LocalEndPoint;
                socketAdd.Connect(epServer);
            }
            //catch(SocketException e)
            catch (Exception e)
            {
                Log.Comment(e.Message);
                //if ((SocketError)e.ErrorCode != SocketError.AccessDenied)
                //    Log.Comment("Incorrect ErrorCode in SocketException during add " + e.ErrorCode);
            }
        }

        static public void TestAsyncThreadProc()
        {
            int cBytes;

            using (Socket sock = socketServer.Accept())
            {
                cBytes = sock.Receive(bufReceive);
            }

            AssertDataReceived(cBytes);
        }
    }
}