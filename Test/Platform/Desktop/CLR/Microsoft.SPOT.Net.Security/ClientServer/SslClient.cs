using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Test
{
    public class SslClient
    {
        // The message that is used for verification.
        public const string TERMINATOR = "<EOF>";
        public bool runOnce = true;
        public String messageSent = null;
        public String messageReceived = null;

        // This is the socket information
        public TcpClient clientSocket;
        public IPAddress ipAddress;
        public int port = 11155;
        public IPEndPoint serverEp;

        // These are the ssl specific variables
        public SslStream sslClient;
        public String targetHost;
        public X509CertificateCollection certificateCollection;
        public SslProtocols sslProtocols;

        public bool expectedException;

        public SslClient(IPAddress ipAddress, String targetHost, X509CertificateCollection certificateCollection, SslProtocols sslProtocols, bool expectedException)
        {
            // Initialize the Socket
            messageSent = TestUtilities.GetRandomSafeString(2000) + TERMINATOR;

            // Initialize the port that communication is using
            this.ipAddress = ipAddress;
            this.messageSent = "hello" + TERMINATOR;
            this.targetHost = targetHost;
            this.certificateCollection = certificateCollection;
            this.sslProtocols = sslProtocols;
            this.expectedException = expectedException;
        }

        public MFTestResults RunClient()
        {
             MFTestResults testResult = MFTestResults .Pass;
            try
            {
                if (ipAddress == null)
                    Console.WriteLine("IpAddress must be initialized before calling RunClient()");
                else
                    serverEp = new IPEndPoint(ipAddress, port);

                IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress localAddr = null;
                foreach (IPAddress addr in hostEntry.AddressList)
                {
                    if (addr.AddressFamily == AddressFamily.InterNetwork)
                    {
                        localAddr = addr;
                    }
                }

                IPEndPoint localEnpoint = new IPEndPoint(localAddr, port);

                Console.WriteLine("Connect to IPAddress: " + serverEp.Address.ToString() + " Port Number: " + serverEp.Port.ToString());

                // Create a TCP/IP client socket.
                bool connected = false;
                int retries = 0;
                while (!connected)
                {
                    try
                    {

                        clientSocket = new TcpClient(localEnpoint);
                        clientSocket.Connect(serverEp);
                        connected = true;
                    }
                    catch { }

                    Thread.Sleep(1000);
                    retries++;
                    if (retries > 20)
                    {
                        Console.WriteLine("Tried to connect 20 times without success.  Failing test.");
                        return MFTestResults.Fail;
                    }
                }

                // Create an SSL stream that will close the client's stream.
                sslClient = new SslStream(clientSocket.GetStream());

                Console.WriteLine("Calling AuthenticateAsClient()");
                // The server name must match the name on the server certificate.
                sslClient.AuthenticateAsClient(targetHost, certificateCollection, sslProtocols, false);

                // Send hello message to the server. 
                byte[] message = Encoding.UTF8.GetBytes(messageSent);
                sslClient.Write(message, 0, message.Length);
                Console.WriteLine("Sent:     " + messageSent);

                // Read message from the server.
                messageReceived = ReadMessage(sslClient);
                Console.WriteLine("Received: " + messageReceived);

                if (messageSent != messageReceived)
                    testResult = MFTestResults.Fail;

            }
            catch (SocketException e)
            {
                if (!expectedException)
                    testResult = MFTestResults.Fail;
                Console.WriteLine("ErrorCode: " + e.ErrorCode);
                Console.WriteLine("An exception occurred: " + e.Message);
            }
            catch (Exception e)
            {
                if (!expectedException)
                    testResult = MFTestResults.Fail;
                Console.WriteLine("An exception occurred: " + e.Message);
            }
            finally
            {
                if (sslClient != null)
                {
                    Thread.Sleep(50);
                    sslClient.Dispose();
                    sslClient = null;
                }
                if (clientSocket != null)
                {
                    clientSocket.Close();
                    clientSocket = null;
                }
            }
            return testResult;
        }

        public string ReadMessage(SslStream sslStream)
        {
            // Read the  message sent by the server.
            // The end of the message is signaled using the
            // "<EOF>" marker.
            String messageData = "";
            byte[] buffer = new byte[2048];

            do
            {
                int read = sslStream.Read(buffer, 0, buffer.Length);

                // Convert the bytes to a string
                messageData += new String(Encoding.UTF8.GetChars(buffer), 0, read);

                // Check for EOF.
                if (messageData.ToString().IndexOf("<EOF>") != -1)
                {
                    break;
                }
            } while (sslStream.CanRead);

            return messageData.ToString();
        }
    }
}