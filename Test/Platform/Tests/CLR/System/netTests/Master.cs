/*---------------------------------------------------------------------
* Master.cs - file description
* Main class, responsible for running all of the other *Tests.cs files
* Version: 1.0
* Author: REDMOND\a-grchat
* Created: 9/4/2007 10:20:51 AM 
* ---------------------------------------------------------------------*/
using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Microsoft.SPOT.Net.NetworkInformation;

namespace Microsoft.SPOT.Platform.Tests
{
    public class Master_network
    {
        public static void Main()
        {
            for (int i = 0; i < 10; i++)
            {
                NetworkInterface ni = NetworkInterface.GetAllNetworkInterfaces()[0];

                if (!ni.IsDhcpEnabled || ni.IPAddress != "0.0.0.0")
                {
                    break;
                }
                Thread.Sleep(500);
            }

            string[] args = { "NetTests" };
            MFTestRunner runner = new MFTestRunner(args);
        }
    }
}