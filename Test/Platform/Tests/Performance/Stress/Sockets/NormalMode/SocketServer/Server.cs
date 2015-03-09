////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Platform.Tests;
using System.Collections;
using System.Threading;
using Microsoft.SPOT.Input;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;


namespace Microsoft.SPOT.Platform.Tests
{
    public class SocketServerTest : IMFTestInterface
    {
        private AutoResetEvent m_evt = new AutoResetEvent(false);
        private AutoResetEvent m_socketCloseEvt = new AutoResetEvent(false);
        private ArrayList m_sockets = new ArrayList();
        private ArrayList m_threads = new ArrayList();
        //private static byte[] bufSend;
        //private static byte[] bufReceive;
        internal static double totalTime = 0;
        internal static long start = 0, stop = 0;
        internal static int m_totalThreadCount = 0;
        internal Time m_testTimer;
        internal List m_list;

        public SocketServerTest()
        {
            m_testTimer = new Time();
            m_list = new List();
        }

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
            Log.Comment("CUp");
        }

        [TestMethod]
        public MFTestResults ThroughputTest()
        {
            StartServer();

            return MFTestResults.Pass;
        }

        private void StartServer()
        {
            int randomPort = 12000;// +Math.Random(10000);
            Log.Comment("Port used in this test: " + randomPort);

            IPHostEntry ipHostInfo = Dns.GetHostEntry("");
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            Log.Comment("IP Address of the device: " + ipAddress.ToString());

            //const long buffLength = 5000;
            //bufSend = new byte[buffLength];
            //bufReceive = new byte[bufSend.Length];

            start = DateTime.Now.Ticks;
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, randomPort);
            server.Bind(localEndPoint);
            server.Listen(Int32.MaxValue);
            stop = DateTime.Now.Ticks;

            totalTime += stop - start;
            
            // Wait for a client to connect            
            while (true)
            {
                // Wait for a client to connect
                Socket clientSocket = server.Accept();

                // Process the client request (synchronously or asynchronously)                
                new ProcessClientRequest(clientSocket, true, m_testTimer, m_list);   // Asynchronous process selected
                
                //Debug.Print("Total Time: " + totalTime);                
            }
        }

        internal sealed class ProcessClientRequest
        {
            private Socket m_clientSocket;
            private Time m_testTimer;
            private List m_list;
            private static long m_count;

            public ProcessClientRequest(Socket clientSocket, Boolean asynchronously, Time timer, List list)
            {
                m_clientSocket = clientSocket;
                m_testTimer = timer;
                m_list = list;

                if (asynchronously)
                {
                    SocketServerTest.m_totalThreadCount++;
                    new Thread(ProcessRequest).Start();
                }
                else
                {
                    ProcessRequest();
                }
            }

            private void ProcessRequest()
            {
                bool isValid = false;

                using (m_clientSocket)
                {
                    const int waitForDataTimeout = 20; // ms
                    int waitForDataIterations = 5;

                    Byte[] buffer = new Byte[100];

                    while (waitForDataIterations-- > 0)
                    {
                        isValid = true;
                        start = DateTime.Now.Ticks;
                        Int32 bytesRead = m_clientSocket.Receive(buffer);
                        stop = DateTime.Now.Ticks;

                        if (bytesRead == 0)
                        {
                            isValid = false;
                            break;
                        }
                        else if (bytesRead == 5)
                        {
                            isValid = false;
                            string val = string.Empty;
                            for (int i = 0; i < 5; i++)
                            {
                                val += (char)buffer[i];
                            }
                            if (string.Equals(val, "close"))
                            {
                                for (int i = 0; i < m_list.DataList.Count; i++)
                                {
                                    Debug.Print(m_list.DataList[i].ToString());                                    
                                    //OutputToScreen(m_list.DataList);
                                }
                                lock (m_list)
                                {
                                    m_list.DataList.Clear();
                                }
                                lock (m_testTimer)
                                {
                                    m_testTimer.TotalTime = 0;
                                }
                                return;
                            }
                        }                        
                        else if (bytesRead == 15)
                        {                            
                            string val = string.Empty;
                            for (int i = 0; i < 3; i++)
                            {
                                val += (char)buffer[i];
                            }
                            if (val == "end")
                            {
                                for (int i = 14; i > 3; i--)
                                {
                                    int pow = 1;
                                    for (int j = 0; j < (14 - i); j++)
                                    {
                                        pow = pow * 10;
                                    }
                                    m_count += (int)buffer[i] * pow;
                                }

                                break;
                            }
                        }

                        lock (m_testTimer)
                        {
                            m_testTimer.TotalTime += stop - start;                            
                        }

                        Thread.Sleep(waitForDataTimeout);
                    }

                    if (isValid)
                    {
                        double timeInMilliSec = (m_testTimer.TotalTime + totalTime) / 10000;
                        double timeInSec = timeInMilliSec / 1000;
                        double tp = m_count / timeInSec;
                        lock (m_list)
                        {
                            m_list.DataList.Add(m_count + ", " + timeInSec + ", " + tp);
                            m_count = 0;
                        }
                        lock (m_testTimer)
                        {
                            m_testTimer.TotalTime = 0;
                        }
                    }
                }
            }

            //private void OutputToScreen(ArrayList list)
            //{
            //    Font NinaBFont = Resources.GetFont(Resources.FontResources.NinaB);
            //    Bitmap mp = new Bitmap(SystemMetrics.ScreenWidth, SystemMetrics.ScreenHeight);

            //    int x = SystemMetrics.ScreenWidth / 2;
            //    int y = SystemMetrics.ScreenHeight / 2;

            //    for (int i = 0; i < list.Count; i++)
            //    {
            //        mp.DrawText(list[i].ToString(), NinaBFont, Color.White, x - 100, height);
            //        height += 15;
            //    }
            //    mp.Flush();
            //}

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
        }
    }

    internal class Time
    {
        private static double m_totalTime;
        static Time()
        {
            m_totalTime = 0;
        }

        internal double TotalTime
        {
            get
            {
                return m_totalTime;
            }

            set
            {
                m_totalTime = value;
            }
        }
    }

    internal class List
    {
        private static ArrayList m_list;
        static List()
        {
            m_list = new ArrayList();
        }

        internal ArrayList DataList
        {
            get
            {
                return m_list;
            }

            set
            {
                m_list = value;
            }
        }
    }
}
