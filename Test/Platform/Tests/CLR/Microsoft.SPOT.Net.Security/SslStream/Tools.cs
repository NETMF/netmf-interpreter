using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.SPOT.Platform.Tests
{
    class Tools
    {
        static private int m_portCounter = 0;
        static public int nextPort
        {
            get
            {
                int rangeOfPorts = 65000;
                int startPort = 1024;
                int retPort = m_portCounter % rangeOfPorts + startPort;
                m_portCounter++;
                return retPort;
            }
        }

        static public bool CertificateCompare(X509Certificate cert1, X509Certificate cert2)
        {
            if( cert1 == null || cert2 == null )
            {
                Log.Comment("One of the certificates is null");
                return false;
            }

            byte [] cert1Bytes = cert1.GetRawCertData();
            byte [] cert2Bytes = cert2.GetRawCertData();

            return CertificateCompare(cert1Bytes, cert2Bytes);
        }

        static public bool CertificateCompare(byte[] cert1Bytes, byte[] cert2Bytes)
        {
            if(cert1Bytes == null || cert2Bytes == null)
            {
                Log.Comment("One of the byte arrays is null");
                return false;
            }

            if( cert1Bytes.Length != cert2Bytes.Length )
            {
                Log.Comment("The certificates are not the same length");
                return false;
            }

            for( int i=0; i< cert1Bytes.Length; ++i)
            {
                if( cert1Bytes[i] != cert2Bytes[i] )
                {
                    Log.Comment("The certificates are not equal");
                    return false;
                }
            }
            return true;
        }


    }
}
