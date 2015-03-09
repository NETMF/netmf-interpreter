////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.SPOT.Platform.Test
{
    static class SslTestEntryPoint
    {
        public static void Main(string [] args)
        {
            //There is only one server running that contains a socket.
            TestManager tms = new TestManager();

            try
            {
                if (args.Length < 2)
                {
                    Usage();
                    Console.Write("\nEnter IPAddress: ");
                    args[0] = Console.ReadLine();
                    Console.Write("Enter Port: ");
                    args[1] = Console.ReadLine();
                }

                IPAddress ipAddress = null;
                int port = 0;
                try
                {
                    //parse the ipaddress to convert it to a long
                    ipAddress = IPAddress.Parse(args[0].ToString());
                    port = int.Parse(args[1]);

                }
                catch
                {
                    Usage();
                    return;
                }

                //Starts the server connection.  Once a connection is made the server runs the test specified by the client.
                tms.Initialize(ipAddress, port);
                tms.Start(new SslTests(ipAddress));
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to start the server with exception:" + e.ToString());
            }
        }

        public static void Usage()
        {
            Console.WriteLine("Usage: ");
            Console.WriteLine("Microsoft.SPOT.Platform.Test.Ssl <ipAddress> <port number>");
            Console.WriteLine("Where <ipAddress> is the address of the device to connect to.  For Example: 157.64.21.10");
            Console.WriteLine("      <port> is the port number of the device to connect to. For Example 15000");
        }

    }
}
