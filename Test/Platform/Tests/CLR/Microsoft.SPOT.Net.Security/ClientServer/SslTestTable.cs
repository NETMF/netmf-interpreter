using Microsoft.SPOT;
using Microsoft.SPOT.Net;
using Microsoft.SPOT.Net.Security;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Platform.Tests;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.SPOT.Platform.Tests
{
    class SslTestTable 
    {
        public SslServer[] sslServer;
        public SslClient[] sslClient;
        public const String desktopName = "ebsnetinc";

        public SslTestTable(IPAddress ipAddress)
        {
            sslClient = new SslClient[] {
                
                // Verify all the combinations of verification.
                new SslClient(ipAddress, desktopName, new X509Certificate(CertificatesAndCAs.device), new X509Certificate []{ new X509Certificate(CertificatesAndCAs.desktop)}, SslVerification.CertificateRequired  , new SslProtocols[] {  SslProtocols.SSLv3 }    , false),
                new SslClient(ipAddress, desktopName, new X509Certificate(CertificatesAndCAs.device), new X509Certificate []{ new X509Certificate(CertificatesAndCAs.desktop)}, SslVerification.VerifyPeer           , new SslProtocols[] {  SslProtocols.SSLv3 }    , false),

                // Verify all the combinations of encrypted protocols.
                new SslClient(ipAddress, desktopName, new X509Certificate(CertificatesAndCAs.device), new X509Certificate []{ new X509Certificate(CertificatesAndCAs.desktop)}, SslVerification.NoVerification       , new SslProtocols[] {  SslProtocols.SSLv3 }    , false),
                new SslClient(ipAddress, desktopName, new X509Certificate(CertificatesAndCAs.device), new X509Certificate []{ new X509Certificate(CertificatesAndCAs.desktop)}, SslVerification.NoVerification       , new SslProtocols[] {  SslProtocols.SSLv3 }    , false),
                new SslClient(ipAddress, desktopName, new X509Certificate(CertificatesAndCAs.device), new X509Certificate []{ new X509Certificate(CertificatesAndCAs.desktop)}, SslVerification.NoVerification       , new SslProtocols[] {  SslProtocols.TLSv1 }    , false),
                new SslClient(ipAddress, desktopName, new X509Certificate(CertificatesAndCAs.device), new X509Certificate []{ new X509Certificate(CertificatesAndCAs.desktop)}, SslVerification.NoVerification       , new SslProtocols[] {  SslProtocols.TLSv1 }    , false),
                new SslClient(ipAddress, desktopName, new X509Certificate(CertificatesAndCAs.device), new X509Certificate []{ new X509Certificate(CertificatesAndCAs.desktop)}, SslVerification.NoVerification       , new SslProtocols[] {  SslProtocols.Default }  , false),
                new SslClient(ipAddress, desktopName, new X509Certificate(CertificatesAndCAs.device), new X509Certificate []{ new X509Certificate(CertificatesAndCAs.desktop)}, SslVerification.NoVerification       , new SslProtocols[] {  SslProtocols.Default }  , false),
                new SslClient(ipAddress, desktopName, new X509Certificate(CertificatesAndCAs.device), new X509Certificate []{ new X509Certificate(CertificatesAndCAs.desktop)}, SslVerification.NoVerification       , new SslProtocols[] {  SslProtocols.Default }  , false),
                //new SslClient(ipAddress, desktopName, null                                           , null                                                                  , SslVerification.NoVerification       , new SslProtocols[] {  SslProtocols.Default }  , false),

                // Verify null combinations throw exception.
                new SslClient(ipAddress, desktopName, new X509Certificate(CertificatesAndCAs.device), new X509Certificate []{ new X509Certificate(CertificatesAndCAs.desktop)}, SslVerification.NoVerification       , null                                          , true),
                new SslClient(ipAddress, null       , new X509Certificate(CertificatesAndCAs.device), new X509Certificate []{ new X509Certificate(CertificatesAndCAs.desktop)}, SslVerification.NoVerification       , new SslProtocols[] {  SslProtocols.Default }  , true),
            };

            
            sslServer = new SslServer[] { 

                // Verify all the combinations of verification.
                new SslServer(new X509Certificate(CertificatesAndCAs.device), new X509Certificate [] { new X509Certificate(CertificatesAndCAs.desktop)}, SslVerification.VerifyPeer         , new SslProtocols [] { SslProtocols.Default }    , false),
                new SslServer(new X509Certificate(CertificatesAndCAs.device), new X509Certificate [] { new X509Certificate(CertificatesAndCAs.desktop)}, SslVerification.VerifyClientOnce   , new SslProtocols [] { SslProtocols.Default }    , false),
                new SslServer(new X509Certificate(CertificatesAndCAs.device), new X509Certificate [] { new X509Certificate(CertificatesAndCAs.desktop)}, SslVerification.CertificateRequired, new SslProtocols [] { SslProtocols.Default }    , false),

                // Verify all the combinations of encrypted protocols.
                new SslServer(new X509Certificate(CertificatesAndCAs.device), new X509Certificate [] { new X509Certificate(CertificatesAndCAs.desktop)}, SslVerification.NoVerification     , new SslProtocols [] { SslProtocols.Default }    , false), 
                new SslServer(new X509Certificate(CertificatesAndCAs.device), new X509Certificate [] { new X509Certificate(CertificatesAndCAs.desktop)}, SslVerification.NoVerification     , new SslProtocols [] { SslProtocols.SSLv3 }      , false), 
                new SslServer(new X509Certificate(CertificatesAndCAs.device), new X509Certificate [] { new X509Certificate(CertificatesAndCAs.desktop)}, SslVerification.NoVerification     , new SslProtocols [] { SslProtocols.Default }    , false),
                new SslServer(new X509Certificate(CertificatesAndCAs.device), new X509Certificate [] { new X509Certificate(CertificatesAndCAs.desktop)}, SslVerification.NoVerification     , new SslProtocols [] { SslProtocols.TLSv1 }      , false),
                new SslServer(new X509Certificate(CertificatesAndCAs.device), new X509Certificate [] { new X509Certificate(CertificatesAndCAs.desktop)}, SslVerification.NoVerification     , new SslProtocols [] { SslProtocols.Default }    , false),
                new SslServer(new X509Certificate(CertificatesAndCAs.device), new X509Certificate [] { new X509Certificate(CertificatesAndCAs.desktop)}, SslVerification.NoVerification     , new SslProtocols [] { SslProtocols.TLSv1 }      , false),
                new SslServer(new X509Certificate(CertificatesAndCAs.device), new X509Certificate [] { new X509Certificate(CertificatesAndCAs.desktop)}, SslVerification.NoVerification     , new SslProtocols [] { SslProtocols.SSLv3 }      , false),
            };
             
        }
    }
}