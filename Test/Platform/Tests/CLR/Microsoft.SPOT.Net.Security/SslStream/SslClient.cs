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
        const string TERMINATOR             = "<EOF>";
        public bool runOnce                     = true;
        public String messageSent            = "Hello from the client."+TERMINATOR;
        public String messageReceived    = null;

        // This is the server socket information
        public Socket clientSocket;
        public IPEndPoint serverEp;
        public SocketType clientSocketType = SocketType.Stream;
        public ProtocolType clientProtocolType = ProtocolType.Tcp;

        // These are the ssl specific variables
        public SslStream sslClient;
        public String targetHost;
        public X509Certificate cert;
        public X509Certificate[] ca;
        public SslVerification verify;
        public SslProtocols[] sslProtocols;


        public SslClient()
        {
            // Initialize the Socket
            clientSocketType = SocketType.Stream;
            clientProtocolType = ProtocolType.Tcp;

            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
            {
                cert = new X509Certificate(CertificatesAndCAs.emuCert, "NetMF");
                ca = new X509Certificate[] { new X509Certificate(CertificatesAndCAs.caEmuCert) };
                targetHost = "ZACHL-SBA1.redmond.corp.microsoft.com";
            }
            else
            {
                cert = new X509Certificate(CertificatesAndCAs.newCert);
                ca = new X509Certificate[] { new X509Certificate(CertificatesAndCAs.caCert) };
                targetHost = "Device.Microsoft.Com";
            }
            verify = SslVerification.CertificateRequired;
            sslProtocols = new SslProtocols[] { SslProtocols.Default };
        }

        public void RunClient()
        {
            // Create a TCP/IP client socket.
            clientSocket = new Socket(AddressFamily.InterNetwork, clientSocketType, clientProtocolType);

            clientSocket.Bind(new IPEndPoint(IPAddress.Loopback, 0));

            while (true)
            {
                try
                {
                    // Connect to the server
                    clientSocket.Connect(serverEp);
                    break;
                }
                catch (SocketException)
                {
                    // Server down, try again
                    Thread.Sleep(1000);
                }
            }
            Log.Comment("Client connected.");
           
            // Create an SSL stream that will close the client's stream.
            sslClient = new SslStream(clientSocket);

            // The server name must match the name on the server certificate.
            sslClient.AuthenticateAsClient(targetHost, cert, ca, verify, sslProtocols);

            // Send hello message to the server. 
            Log.Comment("Sending message to Server: " + messageSent);
            byte[] message = Encoding.UTF8.GetBytes(messageSent);
            sslClient.Write(message, 0, message.Length);

            Log.Comment("do we need to flush? after a write?");
            sslClient.Flush();

            // Read message from the server.
            messageReceived = ReadMessage(sslClient);
            Log.Comment("Server says:" + messageReceived);
                
            // Do not close the clientSslStream.  That is handled by the test case.
        }

        public string ReadMessage(SslStream sslStream)
        {
            // Read the  message sent by the server.
            // The end of the message is signaled using the
            // "<EOF>" marker.
            byte[] buffer = new byte[2048];
            String messageData = "";
            int bytes = -1;
            do
            {
                bytes = sslStream.Read(buffer, 0, buffer.Length);

                // Convert the bytes to a string
                messageData += new String(Encoding.UTF8.GetChars(buffer));

                // Check for EOF.
                if (messageData.ToString().IndexOf(TERMINATOR) != -1)
                {
                    break;
                }
            } while (bytes > 0);

            return messageData.ToString();
        }

        public void Close()
        {
            if (sslClient != null)
                sslClient.Close();
        }
    }

}