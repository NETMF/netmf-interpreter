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
    public class getip
    {                   
        public static void Main()
        {
            Debug.Print("Get Host IP test");
                   
            while (true)
            {
                Thread.Sleep(2500);

                IPHostEntry hostEntry = Dns.GetHostEntry( "" );

                foreach(IPAddress ip in hostEntry.AddressList)
                {
                    if(ip != null)
                    {
                        Debug.Print("Your host ip: " + ip);
                    }
                }
            }
        }
    }
}
