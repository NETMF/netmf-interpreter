using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SocketClient
{
    class Program
    {
        /// <summary>
        /// Please specify the IP address of the device as the arguments.
        /// Example: If the device IP is 192.168.1.1
        /// SocketClient 192 168 1 1
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            int port = 12000;                        
            IPAddress ipAddress = new IPAddress(DottedDecimalToIp(
                Convert.ToByte(args[0]), 
                Convert.ToByte(args[1]), 
                Convert.ToByte(args[2]), 
                Convert.ToByte(args[3])));
            long buffLength = 1000;
            byte[] buf = new byte[1024];

            for (long dataLength = 100000; dataLength < 1000000; dataLength += 1000000)            
            {                
                byte[] bufSend = new byte[buffLength];
                for (long i = 0; i < bufSend.Length; i++)
                {
                    bufSend[i] = 1;
                }

                for (long j = 0; j < dataLength / buffLength; j++)
                {
                    Console.WriteLine("Length: " + dataLength + "  Buffer Iteration: " + j);
                    using (Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                    {
                        sock.Connect(ipAddress, port);
                        sock.Send(bufSend);                        
                    }
                }

                using (Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    sock.Connect(ipAddress, port);
                    byte[] bufSend1 = new byte[15];
                    bufSend1[0] = (byte)'e';
                    bufSend1[1] = (byte)'n';
                    bufSend1[2] = (byte)'d';
                    bufSend1[3] = (byte)':';

                    long temp = dataLength;
                    int o = 14;
                    while (temp > 0)
                    {
                        bufSend1[o--] = (byte)(temp % 10);
                        temp = temp / 10;
                    }
                    sock.Send(bufSend1);
                }
            }

            using (Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                sock.Connect(ipAddress, port);
                byte[] bufSend = new byte[5];
                bufSend[0] = (byte)'c';
                bufSend[1] = (byte)'l';
                bufSend[2] = (byte)'o';
                bufSend[3] = (byte)'s';
                bufSend[4] = (byte)'e';
                sock.Send(bufSend);
            }
        }

        private static byte[] StringToByteArray(string txt)
        {
            char[] cdata = txt.ToCharArray();
            byte[] data = new byte[cdata.Length];

            for (int i = 0; i < cdata.Length; i++)
            {
                data[i] = (byte)cdata[i];
            }
            return data;
        }

        private static long DottedDecimalToIp(byte a1, byte a2, byte a3, byte a4)
        {
            return (long)((ulong)a4 << 24 | (ulong)a3 << 16 | (ulong)a2 << 8 | (ulong)a1);
        }
    }
}
