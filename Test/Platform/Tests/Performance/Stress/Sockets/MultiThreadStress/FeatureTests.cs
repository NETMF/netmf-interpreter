////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class MultiThreadStress : IMFTestInterface
    {
        // This string needs to be changed to a valid server that has port 80 open
        private string server = "msw.dns.microsoft.com";
        private static ManualResetEvent s_evt = new ManualResetEvent(false);
        private static ManualResetEvent s_evtRelease = new ManualResetEvent(false);
        private static int s_cnt = 0;
        private const int c_NUM_CLIENTS = 10;

        //Ping the website to get the ipaddress and hard code it here. Work around for bug: 20845
        //private IPAddress ipAddress = new IPAddress(DottedDecimalToIp(157, 54, 80, 74));

        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");
            // Add your functionality here.   

            if (server == "internal.server.name")
            {
                Log.Comment("Server name needs to be customized before running test");
                return InitializeResult.Skip;
            }
            
            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");
        }

        [TestMethod]
        public MFTestResults StartStress()
        {
            //let the system settle
            Thread.Sleep(1000);
            Debug.GC(true);

            for (int i = 0; i < c_NUM_CLIENTS; i++)
            {
                new Thread(RunClient).Start();
            }
            RunClient();

            return MFTestResults.Pass;
        }

        public void RunClient()
        {
            int a = Thread.CurrentThread.GetHashCode();
            int i = 0;
            int byteCount = 0;
            DateTime last = DateTime.Now;
            TimeSpan refresh = new TimeSpan(0, 0, 20);
            //for (int q = 0; q < 100; q++)
            while(true)
            {
                for (int j = 0; j < 10; j++)
                {
                    byteCount += GetWebPage(server).Length;
                    i++;
                    TimeSpan current = DateTime.Now - last;
                    if (current > refresh)
                    {
                        Debug.Print("Thread " + a + " rcvd " + byteCount + " bytes in last " + current.Seconds + " seconds - Current Iteration: " + i);
                        byteCount = 0;
                    }
                    last = DateTime.Now;
                }

                s_evtRelease.Reset();

                if (Interlocked.Increment(ref s_cnt) == c_NUM_CLIENTS)
                {
                    Debug.GC(true);

                    Interlocked.Decrement(ref s_cnt);

                    s_evt.Set();
                }
                else if (s_evt.WaitOne())
                {
                    if (Interlocked.Decrement(ref s_cnt) == 0)
                    {
                        s_evtRelease.Set();
                        Thread.Sleep(0);
                    }
                }

                s_evtRelease.WaitOne();
                s_evt.Reset();

                last = DateTime.Now;
            }
        }



        // This method requests the home page content for the specified server.
        public String GetWebPage(String serverName)
        {
            const Int32 c_httpPort = 80;
            const Int32 c_microsecondsPerSecond = 1000000;

            // 'page' refers to the HTML data as it is built up.
            String page = String.Empty;

            // Create a socket connection to the specified server and port.
            using (Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                try
                {
                    serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, new byte[] { 0, 0, 0, 0 });

                    // Get server's IP address.
                    IPHostEntry hostEntry = Dns.GetHostEntry(serverName);

                    // Create socket and connect to the server's IP address and port
                    serverSocket.Connect(new IPEndPoint(hostEntry.AddressList[0], c_httpPort));

                    // Send request to the server.
                    String request = "GET / HTTP/1.1\r\nHost: " + serverName + "\r\nConnection: Close\r\n\r\n";
                    Byte[] bytesToSend = Encoding.UTF8.GetBytes(request);
                    serverSocket.Send(bytesToSend, bytesToSend.Length, 0);

                    // Allocate a buffer that we'll keep reusing to receive HTML chunks
                    Byte[] buffer = new Byte[5000];

                    // Poll for data until 5 second time out - Returns true for data and connection closed
                    int totRead = 0;
                    while (true)
                    {
                        if (totRead > 0 && serverSocket.Available == 0) break;

                        if (!serverSocket.Poll(2 * c_microsecondsPerSecond, SelectMode.SelectRead)) break;

                        // Zero all bytes in the re-usable buffer
                        Array.Clear(buffer, 0, buffer.Length);

                        // Read a buffer-sized HTML chunk
                        int len = (serverSocket.Available < buffer.Length ? serverSocket.Available : buffer.Length);
                        Int32 bytesRead = serverSocket.Receive(buffer, len, SocketFlags.None);
                        //Debug.Print("Bytes from wire: " + bytesRead);

                        // If 0 bytes in buffer, then connection is closed, or we have timed out
                        if (bytesRead == 0)
                            break;

                        // Append the chunk to the string
                        page = page + new String(Encoding.UTF8.GetChars(buffer));
                        totRead += bytesRead;
                    }
                }
                catch (SocketException ex)
                {
                    // Cast for easy diagnostics under debugger
                    SocketError error = (SocketError)ex.ErrorCode;
                    Debug.Print("Socket Error: " + error);
                    Debug.Print(ex.StackTrace);
                }
            }
            return page;   // Return the complete string
        }

        static public long DottedDecimalToIp(byte a1, byte a2, byte a3, byte a4)
        {
            return (long)((ulong)a4 << 24 | (ulong)a3 << 16 | (ulong)a2 << 8 | (ulong)a1);
        }
    }
}
