////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using Microsoft.SPOT;

namespace Microsoft.SPOT.Test
{
    public class Network
    {                   
        public static void Main()
        {
        IPEndPoint localEnd  = new IPEndPoint(IPAddress.Loopback,0);
        Socket localSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        localSocket.Bind(localEnd);

        IPEndPoint remoteEnd = new IPEndPoint(IPAddress.Loopback,0);
        Socket remoteSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        remoteSocket.Bind(remoteEnd);
          
        Byte[] sendMsg = Encoding.UTF8.GetBytes("Send2Remote");
        Byte[] receiveMsg = new Byte[64];

        //set up end-points for both local & remote sides


        remoteSocket.Listen(1);
        EndPoint remotePoint = (EndPoint) remoteSocket.LocalEndPoint;
        EndPoint localPoint = (EndPoint) localSocket.LocalEndPoint;

        localSocket.Connect(remotePoint);

        Socket r = remoteSocket.Accept();
        while (true)
        {
            int cnt = localSocket.Send(sendMsg);
         
            r.Receive(receiveMsg);

            char[] rMsg = System.Text.Encoding.UTF8.GetChars(receiveMsg);

            Thread.Sleep(500);
            Debug.Print("Socket loopback test");
        }

      }
    }
}
