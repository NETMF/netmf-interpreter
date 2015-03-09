////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Net;
using System.Net.Sockets;
using System.Net;
using System.Collections;
using System.Threading;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class Loopback : IMFTestInterface
    {
        private Socket m_socketClient, m_socketServer;
        private Thread m_serverThread;
        private static byte[] m_bufReceive;
        private int m_buffLength = 10;
        
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");
            // Add your functionality here. 
            Log.Comment("Calling StartServer() to start the socket server on a new thread");
            m_bufReceive = new byte[m_buffLength];
            StartServer();     
            
            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");
        }

        [TestMethod]
        public MFTestResults TestMethod1()
        {
            Random random = new Random();
            // Run the test for 20 random buffer lengths.
            for (int count = 1; count <= 20; count++)
            {                                                              
                byte[] bufSend = new byte[m_buffLength]; // Buffer size
                Log.Comment("Buffer size for this test = " + m_buffLength);

                random.NextBytes(bufSend);

                int cBytes = 0;
                int sendCount = random.Next(100);
                Log.Comment("Sending the buffer \"" + sendCount + "\" times");
                for (int i = 0; i < sendCount; i++)
                {
                    cBytes = m_socketClient.Send(bufSend);
                    Log.Comment("CBytes = " + cBytes);
                    if (cBytes != m_buffLength)
                    {
                        Log.Comment("FAILURE: cBytes != m_buffLength: " + cBytes + " != " + m_buffLength);
                        return MFTestResults.Fail;
                    }
                }

                m_buffLength = random.Next(10000 * count);
            }

            return MFTestResults.Pass;
        }

        private void StartServer()
        {

            m_socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            m_socketClient.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            m_socketServer.Bind(new IPEndPoint(IPAddress.Loopback, 0));

            IPEndPoint epClient = (IPEndPoint)m_socketClient.LocalEndPoint;
            IPEndPoint epServer = (IPEndPoint)m_socketServer.LocalEndPoint;

            m_serverThread = new Thread(Prepare);
            m_serverThread.Start();

            Thread.Sleep(1);

            m_socketClient.Connect(epServer);
        }

        private void Prepare()
        {
            m_socketServer.Listen(1);

            using (Socket sock = m_socketServer.Accept())
            {
                while (true)
                {
                    if (sock.Poll(-1, SelectMode.SelectRead))
                    {
                        Array.Clear(m_bufReceive, 0, m_bufReceive.Length);
                        int cBytes = sock.Receive(m_bufReceive);
                        if (cBytes == 0)
                        {
                            break;
                        }
                    }
                }
            }
        }
    }
}
