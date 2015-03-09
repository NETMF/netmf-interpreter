using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Microsoft.SPOT.Platform.Test
{
    class SslServer
    {
        //X509 Certificate used for the connection.
        X509Certificate2 certificate = null;
        bool clientCertificateRequired = false;
        SslProtocols enabledSslProtocols = SslProtocols.Default;
        bool expectedException = false;
        
        //Socket configuration information.
        IPAddress ipAddress = null;
        Int32 port = 11111;

        //The Socket thats used as an Ssl Server.
        TcpListener sslServer = null;

        public SslServer(X509Certificate2 cert, bool certRequired, SslProtocols enabledProt, bool expectedExcep)
        {
            certificate = cert;
            clientCertificateRequired = certRequired;
            enabledSslProtocols = enabledProt;
            expectedException = expectedExcep;

            if (!certificate.HasPrivateKey)
            {
                Console.WriteLine("ERROR: The cerfiticate does not have a private key.");
                throw new Exception("No Private Key in the certificate file");
            }

            //Get the IPV4 address on this machine which is all that the MF devices support.
            foreach (IPAddress address in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipAddress = address;
                    break;
                }
            }

            if (ipAddress == null)
                throw new Exception("No IPV4 address found.");

            //Console.WriteLine("ipAddress is: " + ipAddress.ToString());
            //Console.WriteLine("port          is: " + port.ToString());
        }

        public MFTestResults RunServer()
        {
            MFTestResults testResult = MFTestResults.Fail;
            SslStream sslStream = null;

            try
            {
                sslServer = new TcpListener(ipAddress, port);

                // Start listening for client requests.
                sslServer.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[2048];
                String data = null;

                // Start Listening for a connection.
                Console.Write("Waiting for a connection... ");

                // Perform a blocking call to accept requests.
                // You could also user server.AcceptSocket() here.
                TcpClient client = sslServer.AcceptTcpClient();
                Console.WriteLine("Connected!");

                data = null;

                // Get a stream object for reading and writing
                sslStream = new SslStream(client.GetStream());

                sslStream.AuthenticateAsServer(certificate, clientCertificateRequired, enabledSslProtocols, false);

                TestUtilities.PrintSslStreamProperties(sslStream);

                int i = 0;
                // Loop to receive all the data sent by the client.
                while ((i = sslStream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    // Translate data bytes to a string.
                    // The encoding used is application specific.
                    data = System.Text.Encoding.UTF8.GetString(bytes, 0, i);
                    Console.WriteLine("Received: {0}", data);

                    byte[] msg = System.Text.Encoding.UTF8.GetBytes(data);

                    // Send back a response.
                    sslStream.Write(msg, 0, msg.Length);
                    Console.WriteLine("Sent:     {0}", data);
                }
                testResult = MFTestResults.Pass;
            }
            catch (SocketException e)
            {
                if (expectedException)
                    testResult = MFTestResults.Pass;

                Console.WriteLine("SocketException in StartServer(): " + e.ToString());
                Console.WriteLine("ErrorCode: " + e.ErrorCode.ToString());
            }
            catch (Exception e)
            {
                if (expectedException)
                    testResult = MFTestResults.Pass;

                Console.WriteLine("Exception in StartServer(): " + e.ToString());
            }
            finally
            {
                if (sslStream != null)
                {
                    sslStream.Dispose();
                    sslStream = null;
                }
                if (sslServer != null)
                {
                    sslServer.Stop();
                    sslServer = null;
                }
            }

            return testResult;
        }


    }
}
