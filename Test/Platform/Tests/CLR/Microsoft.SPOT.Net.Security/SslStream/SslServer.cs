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
    public class SslServer
    {

        // Data and configuration of the server thread behavior
        private Thread threadServer;
        private bool running = false;


        const string TERMINATOR = "<EOF>";
        public bool runOnce                     = true;
        public String messageReceived = null;

        // This is the server socket information
        public Socket serverSocket;
        public IPEndPoint serverEp;
        public SocketType serverSocketType;
        public ProtocolType serverProtocolType;

        // These are the ssl specific variables
        public SslStream sslServer;
        public X509Certificate cert;
        public X509Certificate[] ca;
        public SslVerification verify; 
        public SslProtocols[] sslProtocols;

        // Initialize all of the default certificates and protocols
        public SslServer()
        {
            // Initialize the Socket
            serverSocketType = SocketType.Stream;
            serverProtocolType = ProtocolType.Tcp;

            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
            {
                cert = new X509Certificate(CertificatesAndCAs.emuCert, "NetMF");
                ca = new X509Certificate[] { new X509Certificate(CertificatesAndCAs.caEmuCert) };
            }
            else
            {
                // Initialize the SslStream
                cert = new X509Certificate(CertificatesAndCAs.newCert);
                ca = new X509Certificate[] { new X509Certificate(CertificatesAndCAs.caCert) };
            }
            verify = SslVerification.NoVerification;
            sslProtocols = new SslProtocols[] { SslProtocols.Default };

            // Create a TCP/IP (IPv4) socket and listen for incoming connections.
            serverSocket = new Socket(AddressFamily.InterNetwork, serverSocketType, serverProtocolType);
            serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, false);

            serverSocket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            serverEp = (IPEndPoint)serverSocket.LocalEndPoint;

            Log.Comment("Listening for a client to connect...");
            serverSocket.Listen(1);
        }

        /// <summary>
        /// Creates the socket object passed to the sslstream and starts a server listening on loopback.
        /// </summary>
         public void RunServer()
        {
            // If for some reason a server is started Stop it.
            StopServer();

            // Start the Server thread to accept the client requests.
            running = true;
            threadServer = new Thread(ProcessClient);
            threadServer.Start();

            Thread.Sleep(0);
        }

        /// <summary>
        /// Processes client socket requests.  Sends the message received back to the client.
        /// </summary>
        internal void ProcessClient()
        {
            // Application blocks while waiting for an incoming connection.
            while (true)
            {
                try
                {
                    Log.Comment("Waiting for a client to connect...");
                    Socket clientSocket = serverSocket.Accept();

                    Log.Comment("Socket Connected");

                    // A client has connected. Create the 
                    // SslStream using the client's network stream.
                    sslServer = new SslStream(clientSocket);
                    // Authenticate the server but don't require the client to authenticate.

                    sslServer.AuthenticateAsServer(cert, ca, verify, sslProtocols);
                    //Display the properties and settings for the authenticated stream.
                    //DisplaySecurityLevel(sslStream);
                    //DisplaySecurityServices(sslStream);
                    //DisplayCertificateInformation(sslStream);
                    //DisplayStreamProperties(sslStream);

                    // Read a message from the client.   
                    Log.Comment("Waiting for client message...");
                    messageReceived = ReadMessage(sslServer);
                    Log.Comment("Received: " + messageReceived);

                    // Write a message to the client.
                    Log.Comment("Resending message back to client...");
                    sslServer.Write(Encoding.UTF8.GetBytes(messageReceived), 0, messageReceived.Length);

                    // Do not close the SslStream here.  that is to be handled by the test case.

                }
                catch (Exception ex)
                {
                    Log.Exception("Unhandled exception " + ex.Message);
                }
                if (runOnce)
                    return;
            }
        }

        /// <summary>
        /// Read the messages sent over the socket.
        /// </summary>
         internal string ReadMessage(SslStream sslStream)
        {
            // Read the  message sent by the client.
            // The client signals the end of the message using the
            // "<EOF>" marker.
            byte[] buffer = new byte[2048];
            String messageData = "";
            int bytesRead = -1;
            do
            {
                // Read the client's test message.
                bytesRead = sslStream.Read(buffer, 0, buffer.Length);

                messageData += new String(Encoding.UTF8.GetChars(buffer));
                
                // Check for <EOF> or an empty message.
                if (messageData.IndexOf(TERMINATOR) != -1)
                {
                    break;
                }
            } while (bytesRead > 0);

            return messageData;
        }

 
        /// <summary>
        /// Close the server thread and all sockets associated with it.
        /// </summary>
         public void Close()
        {
            StopServer();

            if (sslServer != null)
                sslServer.Close();
        }


        /// <summary>
        /// Stop processing new connections
        /// </summary>
        public void StopServer()
        {
            // If server is running, stop it first
            if (running)
            {
                running = false;
                threadServer.Abort();
                threadServer.Join();
            }
        }

        //public void DisplaySecurityLevel(SslStream stream)
        //{
        //    Log.Comment("Cipher: {0} strength {1}", stream.CipherAlgorithm, stream.CipherStrength);
        //    Log.Comment("Hash: {0} strength {1}", stream.HashAlgorithm, stream.HashStrength);
        //    Log.Comment("Key exchange: {0} strength {1}", stream.KeyExchangeAlgorithm, stream.KeyExchangeStrength);
        //    Log.Comment("Protocol: {0}", stream.SslProtocol);
        //}
        //public void DisplaySecurityServices(SslStream stream)
        //{
        //    Log.Comment("Is authenticated: {0} as server? {1}", stream.IsAuthenticated, stream.IsServer);
        //    Log.Comment("IsSigned: {0}", stream.IsSigned);
        //    Log.Comment("Is Encrypted: {0}", stream.IsEncrypted);
        //}
        //public void DisplayStreamProperties(SslStream stream)
        //{
        //    Log.Comment("Can read: {0}, write {1}", stream.CanRead, stream.CanWrite);
        //    Log.Comment("Can timeout: {0}", stream.CanTimeout);
        //}
        //public void DisplayCertificateInformation(SslStream stream)
        //{
        //    Log.Comment("Certificate revocation list checked: {0}", stream.CheckCertRevocationStatus);

        //    X509Certificate localCertificate = stream.LocalCertificate;
        //    if (stream.LocalCertificate != null)
        //    {
        //        Log.Comment("Local cert was issued to {0} and is valid from {1} until {2}.",
        //            localCertificate.Subject,
        //            localCertificate.GetEffectiveDateString(),
        //            localCertificate.GetExpirationDateString());
        //    }
        //    else
        //    {
        //        Log.Comment("Local certificate is null.");
        //    }
        //    // Display the properties of the client's certificate.
        //    X509Certificate remoteCertificate = stream.RemoteCertificate;
        //    if (stream.RemoteCertificate != null)
        //    {
        //        Log.Comment("Remote cert was issued to {0} and is valid from {1} until {2}.",
        //            remoteCertificate.Subject,
        //            remoteCertificate.GetEffectiveDateString(),
        //            remoteCertificate.GetExpirationDateString());
        //    }
        //    else
        //    {
        //        Log.Comment("Remote certificate is null.");
        //    }
        //}
  
    }
}

