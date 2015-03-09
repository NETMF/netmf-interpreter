////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Platform.Tests;
using st = Microsoft.SPOT.Platform.Tests.SocketTools;
using System.Collections;
using System.Threading;
using Microsoft.SPOT.Input;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Platform.Tests.Properties;
namespace Microsoft.SPOT.Platform.Tests
{
    public class Sockets : IMFTestInterface
    {
        private const int PORTNUM_CLIENT = 100;
        private const int PORTNUM_SERVER = 110;
        private static byte[] bufSend;
        private static byte[] bufReceive;
        private Socket socketClient, socketServer;
        private int socketCount = 0;
        private Window mainWindow;
        private int height = 15;

        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");
            // Add your functionality here.   
            
            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");
        }

        public MFTestResults ThroughputTest()
        {
            double totalTime = 0;
            ArrayList list = new ArrayList();
            long start = 0, stop = 0;
            const long buffLength = 5000;
            bufSend = new byte[(int)buffLength];
            bufReceive = new byte[bufSend.Length];

            StartServer(buffLength);

            for (int i = 0; i < bufSend.Length; i++)
            {
                bufSend[i] = 1;
            }

            for (long dataLength = 1000000; dataLength < 10000000; dataLength = dataLength + 1000000)
            {                
                for (long j = 0; j < dataLength / buffLength; j++)
                {   
                    start = DateTime.Now.Ticks;                                    
                    socketClient.Send(bufSend);
                    stop = DateTime.Now.Ticks;
                    totalTime += (stop - start);
                    Debug.Print("Inner Iteration: " + j);
                }

                double timeInMilliSec = totalTime / 10000;
                double timeInSec = timeInMilliSec / 1000;
                double tp = dataLength/timeInSec;
                list.Add(dataLength + "," + tp);
                totalTime = 0;
                Debug.Print("Data Length: " + dataLength);
            }

            for (int i = 0; i < list.Count; i++)
            {
                //OutputToScreen(list[i].ToString());
                Debug.Print(list[i].ToString());
            }

            return MFTestResults.Skip;
        }

        private void StartServer(long buffLength)
        {
            Debug.Print("# of sockets created so far: " + socketCount.ToString());
            socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketCount++;
            socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketCount++;
            Debug.Print("Added two more.. new total: " + socketCount.ToString());

            socketClient.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            socketServer.Bind(new IPEndPoint(IPAddress.Loopback, 0));

            IPEndPoint epClient = (IPEndPoint)socketClient.LocalEndPoint;
            IPEndPoint epServer = (IPEndPoint)socketServer.LocalEndPoint;

            Thread serverThread = new Thread(Prepare);
            serverThread.Start();
            Thread.Sleep(1);

            socketClient.Connect(epServer);        
        }

        private void Prepare()
        {
            socketServer.Listen(1);
            if (socketServer.Poll(-1, SelectMode.SelectRead))
            {
                using (Socket sock = socketServer.Accept())
                {
                    while (true)
                    {
                        if (sock.Poll(-1, SelectMode.SelectRead))
                        {
                            Array.Clear(bufReceive, 0, bufReceive.Length);
                            int cBytes = sock.Receive(bufReceive);
                            Debug.Print("Server thread");
                            if (cBytes == 0)
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void OutputToScreen(string text)
        {
            mainWindow = new Window();
            mainWindow.Width = SystemMetrics.ScreenWidth;
            mainWindow.Height = SystemMetrics.ScreenHeight;

            Font NinaBFont = Resources.GetFont(Resources.FontResources.NinaB);
            Bitmap mp = new Bitmap(SystemMetrics.ScreenWidth, SystemMetrics.ScreenHeight);
            
            int x = SystemMetrics.ScreenWidth / 2 - text.Length * 2;
            int y = SystemMetrics.ScreenHeight / 2;
            mp.DrawText(text,NinaBFont, Color.White, x-70, height);
            height += 15;
            mp.Flush();
        }
    }
}
