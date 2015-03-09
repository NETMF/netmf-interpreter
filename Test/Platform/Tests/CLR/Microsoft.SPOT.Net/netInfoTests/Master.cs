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


namespace Microsoft.SPOT.Platform.Tests
{
    public class Master_network
    {
        public static void Main()
        {
            string[] args = { "NetworkInterfaceTests" };
            MFTestRunner runner = new MFTestRunner(args);
        }
    }
}