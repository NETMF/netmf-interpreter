using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microsoft.SPOT.Net;
using Microsoft.SPOT.Net.Security;
using Microsoft.SPOT.Platform.Test;
using System.Security.Cryptography.X509Certificates;


namespace Microsoft.SPOT.Platform.Tests
{
    public class SslClient
    {
        // The message that is used for verification.
        public const string TERMINATOR = "<EOF>";
        public bool runOnce = true;
        public String messageSent = null;
        public String messageReceived = null;

        // This is the socket information
        public Socket clientSocket;
        public IPAddress ipAddress;
        public int port = 11111;
        public IPEndPoint serverEp;

        // These are the ssl specific variables
        public SslStream sslClient;
        public String targetHost;
        public X509Certificate cert;
        public X509Certificate[] ca;
        public SslVerification verify;
        public SslProtocols[] sslProtocols;

        public bool expectedException;

        public SslClient(IPAddress ipAddress, String targetHost, X509Certificate cert, X509Certificate [] ca, SslVerification verify, SslProtocols[] sslProtocols, bool expectedException)
        {
            // Initialize the Socket
            messageSent = MFUtilities.GetRandomSafeString(2000) + TERMINATOR;

            // Initialize the port that communication is using
            port = 11111;
            this.ipAddress = ipAddress;
            this.messageSent = "hello" + TERMINATOR;
            this.targetHost = targetHost;
            this.cert = cert;
            this.ca = ca;
            this.verify = verify;
            this.sslProtocols = sslProtocols;
            this.expectedException = expectedException;
        }

        public MFTestResults RunClient()
        {
             MFTestResults testResult = MFTestResults .Pass;
            try
            {
                if (ipAddress == null)
                    Debug.Print("IpAddress must be initialized before calling RunClient()");
                else
                    serverEp = new IPEndPoint(ipAddress, port);

                // Create a TCP/IP client socket.
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                clientSocket.Connect(serverEp);

                // Create an SSL stream that will close the client's stream.
                sslClient = new SslStream(clientSocket);

                Log.Comment("Calling AuthenticateAsClient()");
                // The server name must match the name on the server certificate.
                sslClient.AuthenticateAsClient(targetHost, cert, ca, verify, sslProtocols);

                // Send hello message to the server. 
                byte[] message = Encoding.UTF8.GetBytes(messageSent);
                sslClient.Write(message, 0, message.Length);
                Log.Comment("Sent:     " + messageSent);

                // Read message from the server.
                messageReceived = ReadMessage(sslClient);
                Log.Comment("Received: " + messageReceived);

                if (messageSent != messageReceived)
                    testResult = MFTestResults.Fail;

            }
            catch (SocketException e)
            {
                if (!expectedException)
                    testResult = MFTestResults.Fail;
                Log.Comment("ErrorCode: " + e.ErrorCode);
                Log.Comment("An exception occurred: " + e.Message);
            }
            catch (Exception e)
            {
                if (!expectedException)
                    testResult = MFTestResults.Fail;
                Log.Comment("An exception occurred: " + e.Message);
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
            byte[] buffer = new byte[2048];
            String messageData = "";
            int bytesRead = 0;
            do
            {
                if (sslStream.DataAvailable)
                {
                    int len = (int)sslStream.Length;

                    if (len > buffer.Length)
                    {
                        len = buffer.Length;
                    }

                    bytesRead += sslStream.Read(buffer, 0, len);

                    // Convert the bytes to a string
                    messageData += new String(Encoding.UTF8.GetChars(buffer));

                    // Check for EOF.
                    int index = messageData.IndexOf("<EOF>");
                    if (index != -1)
                    {
                        messageData = messageData.Substring(0, index + 5);
                        break;
                    }
                }
                Thread.Sleep(200);
            } while (sslStream.CanRead);

            return messageData.ToString();
        }
    }
}