using System;
using System.Security.Cryptography; 
using System.Security.Cryptography.X509Certificates; 
using System.IO; 
using System.Text;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Cryptoki;

namespace Microsoft.SPOT.Platform.Tests
{
    class CertTest : IMFTestInterface
    {
        bool m_isEmulator;

        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");

            // Add your functionality here.                
            try
            {
                m_isEmulator = (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3);
            }
            catch
            {
                return InitializeResult.Skip;
            }

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
        }

        static void PrintByteArray(Byte[] arr)
        {
            int i;
            string str = "";
            for (i = 0; i < arr.Length; i++)
            {
                str += arr[i] + "    ";
                if ((i + 9) % 8 == 0)
                {
                    Log.Comment(str);
                    str = "";
                }
            }
            if (i % 8 != 0) Log.Comment(str);
        }

        [TestMethod]
        public MFTestResults CertTest_Test()
        {
            bool bRes = true;

            try
            {
                //string filename = "microsoft.cer";
                using (Session session = new Session("", MechanismType.RSA_PKCS))
                {
                    X509Certificate2 cert = new X509Certificate2(session, Properties.Resources.GetBytes(Properties.Resources.BinaryResources.microsoft));
                    Log.Comment(cert.Subject);

                    Log.Comment(cert.Issuer);

                    byte[] serialNumber = new byte[cert.GetSerialNumber().Length];
                    Array.Copy(cert.GetSerialNumber(), 0,
                                         serialNumber, 0,
                                         cert.GetSerialNumber().Length);
                    PrintByteArray(serialNumber);

                    Log.Comment(cert.GetKeyAlgorithm());


                    byte[] publicKey = new byte[cert.GetPublicKey().Length];
                    Array.Copy(cert.GetPublicKey(), 0,
                                         publicKey, 0,
                                         cert.GetPublicKey().Length);
                    PrintByteArray(publicKey);

                    Log.Comment(cert.GetEffectiveDateString());
                    Log.Comment(cert.GetExpirationDateString());
                }
            }
            catch
            {
                bRes = false;
            }

            return bRes ? MFTestResults.Pass : MFTestResults.Fail;
        }
    }
}