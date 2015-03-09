using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Net;

namespace Microsoft.SPOT.Platform.Test
{
    class SslTestTable
    {
        public SslServer[] sslServer;
        public SslClient[] sslClient;
        public const String deviceName = "ebsnetinc.client";
        private X509Certificate2 cert = new X509Certificate2(Resource1.desktop, "alden");

        public SslTestTable(IPAddress ipAddress)
        {
            sslServer = new SslServer[] { 
                
                // Verify all the combinations of verification.
                new SslServer(new X509Certificate2(Resource1.desktop, "alden"), true, SslProtocols.Ssl3     , false),
                new SslServer(cert, true,  SslProtocols.Ssl3    , false),

                // Verify all the combinations of encrypted protocols.
                new SslServer(cert, false, SslProtocols.Default , false), 
                new SslServer(cert, false, SslProtocols.Ssl3    , false), 
                new SslServer(cert, false, SslProtocols.Default , false),
                new SslServer(cert, false, SslProtocols.Tls     , false),
                new SslServer(cert, false, SslProtocols.Default , false),
                new SslServer(cert, false, SslProtocols.Tls     , false),
                new SslServer(cert, false, SslProtocols.Ssl3    , false),
 //               new SslServer(cert, false, SslProtocols.Default , false),

                // Verify null combinations of verification.
                new SslServer(cert, true, SslProtocols.Default  , true),
                new SslServer(cert, true, SslProtocols.Default  , true),

            };

          
            
            sslClient = new SslClient[] { 
                
                // Verify all the combinations of verification.
                new SslClient(ipAddress, deviceName, new X509CertificateCollection(new X509Certificate2 [] {cert}), SslProtocols.Default , false),
                new SslClient(ipAddress, deviceName, new X509CertificateCollection(new X509Certificate2 [] {cert}), SslProtocols.Default , false),
                new SslClient(ipAddress, deviceName, new X509CertificateCollection(new X509Certificate2 [] {cert}), SslProtocols.Default , false),

                // Verify all the combinations of encrypted protocols.
                new SslClient(ipAddress, deviceName, new X509CertificateCollection(new X509Certificate2 [] {cert}), SslProtocols.Ssl3    , false),
                new SslClient(ipAddress, deviceName, new X509CertificateCollection(new X509Certificate2 [] {cert}), SslProtocols.Ssl3    , false),
                new SslClient(ipAddress, deviceName, new X509CertificateCollection(new X509Certificate2 [] {cert}), SslProtocols.Tls     , false),
                new SslClient(ipAddress, deviceName, new X509CertificateCollection(new X509Certificate2 [] {cert}), SslProtocols.Tls     , false),
                new SslClient(ipAddress, deviceName, new X509CertificateCollection(new X509Certificate2 [] {cert}), SslProtocols.Default , false),
                new SslClient(ipAddress, deviceName, new X509CertificateCollection(new X509Certificate2 [] {cert}), SslProtocols.Default , false),
                new SslClient(ipAddress, deviceName, new X509CertificateCollection(new X509Certificate2 [] {cert}), SslProtocols.Default , false),
            };
              
        }
    }
}
