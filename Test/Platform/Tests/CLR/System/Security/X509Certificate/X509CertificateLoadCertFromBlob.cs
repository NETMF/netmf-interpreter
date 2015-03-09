using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Cryptoki;
using System.Text;

namespace Microsoft.SPOT.Platform.Tests
{
    class X509CertificateLoadCertsFromBlob : IMFTestInterface
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
        private int[] c_SIZES = new int[] { 2, 4, 64, 128, 1025, 2048, 4048, 28341, 123112 };
        private const int c_MINFLAGS = -10;
        private const int c_MAXFLAGS = 10;
        // ca.cer in bytes
        private static byte[] c_BYTES; // = new byte[] { 0x30, 0x82, 0x1, 0x9a, 0x30, 0x82, 0x1, 0x44, 0xa0, 0x3, 0x2, 0x1, 0x2, 0x2, 0x10, 0xcc, 0x70, 0x23, 0xf, 0xdb, 0x81, 0xd5, 0xaa, 0x47, 0x83, 0xb4, 0xb9, 0xf9, 0x33, 0xc1, 0xd5, 0x30, 0xd, 0x6, 0x9, 0x2a, 0x86, 0x48, 0x86, 0xf7, 0xd, 0x1, 0x1, 0x4, 0x5, 0x0, 0x30, 0x1d, 0x31, 0x1b, 0x30, 0x19, 0x6, 0x3, 0x55, 0x4, 0x3, 0x13, 0x12, 0x43, 0x6c, 0x6f, 0x73, 0x65, 0x42, 0x75, 0x67, 0x20, 0x54, 0x65, 0x73, 0x74, 0x20, 0x52, 0x6f, 0x6f, 0x74, 0x30, 0x20, 0x17, 0xd, 0x30, 0x30, 0x30, 0x32, 0x30, 0x31, 0x30, 0x37, 0x30, 0x30, 0x30, 0x30, 0x5a, 0x18, 0xf, 0x32, 0x30, 0x39, 0x39, 0x30, 0x31, 0x33, 0x31, 0x30, 0x37, 0x30, 0x30, 0x30, 0x30, 0x5a, 0x30, 0x1b, 0x31, 0x19, 0x30, 0x17, 0x6, 0x3, 0x55, 0x4, 0x3, 0x13, 0x10, 0x43, 0x6c, 0x6f, 0x73, 0x65, 0x42, 0x75, 0x67, 0x20, 0x54, 0x65, 0x73, 0x74, 0x20, 0x43, 0x41, 0x30, 0x5c, 0x30, 0xd, 0x6, 0x9, 0x2a, 0x86, 0x48, 0x86, 0xf7, 0xd, 0x1, 0x1, 0x1, 0x5, 0x0, 0x3, 0x4b, 0x0, 0x30, 0x48, 0x2, 0x41, 0x0, 0xc9, 0xd8, 0x0, 0xc9, 0x1d, 0x30, 0x1c, 0xf1, 0xa4, 0x8a, 0x72, 0x68, 0xd4, 0x66, 0xa, 0x7, 0xe, 0xcb, 0x23, 0xd6, 0x9, 0x13, 0x93, 0x67, 0x6b, 0x45, 0x18, 0x5d, 0xaf, 0x22, 0xe6, 0x76, 0xd0, 0xf5, 0x20, 0xac, 0xa, 0x4f, 0x60, 0x67, 0x85, 0x3b, 0x19, 0x6a, 0x13, 0xd7, 0x26, 0x8, 0x9a, 0xf5, 0x89, 0x5a, 0xd2, 0xc6, 0xb0, 0xcb, 0xad, 0x56, 0x7e, 0x18, 0xc5, 0x4, 0x4e, 0x59, 0x2, 0x3, 0x1, 0x0, 0x1, 0xa3, 0x60, 0x30, 0x5e, 0x30, 0xd, 0x6, 0x3, 0x55, 0x1d, 0xa, 0x4, 0x6, 0x30, 0x4, 0x3, 0x2, 0x7, 0x80, 0x30, 0x4d, 0x6, 0x3, 0x55, 0x1d, 0x1, 0x4, 0x46, 0x30, 0x44, 0x80, 0x10, 0x45, 0xde, 0xc, 0xe2, 0xdc, 0x52, 0xce, 0x27, 0x95, 0x75, 0x3c, 0xab, 0x41, 0x9, 0x7a, 0xe9, 0xa1, 0x1f, 0x30, 0x1d, 0x31, 0x1b, 0x30, 0x19, 0x6, 0x3, 0x55, 0x4, 0x3, 0x13, 0x12, 0x43, 0x6c, 0x6f, 0x73, 0x65, 0x42, 0x75, 0x67, 0x20, 0x54, 0x65, 0x73, 0x74, 0x20, 0x52, 0x6f, 0x6f, 0x74, 0x82, 0xf, 0xed, 0xcf, 0x4e, 0xd2, 0xda, 0x39, 0xac, 0x4c, 0x2c, 0xcc, 0x2d, 0xcc, 0x9d, 0x8f, 0x94, 0x30, 0xd, 0x6, 0x9, 0x2a, 0x86, 0x48, 0x86, 0xf7, 0xd, 0x1, 0x1, 0x4, 0x5, 0x0, 0x3, 0x41, 0x0, 0x7a, 0xdf, 0x6e, 0x4f, 0x59, 0xb, 0x18, 0x3a, 0xa0, 0xae, 0x57, 0x71, 0x2d, 0x53, 0x45, 0x17, 0x32, 0x39, 0x19, 0x29, 0x26, 0xff, 0xc3, 0xd9, 0xa6, 0xfd, 0x82, 0x9f, 0x55, 0xe6, 0x29, 0x58, 0xf6, 0x96, 0x37, 0x16, 0xc, 0x83, 0xe3, 0x31, 0xe1, 0x1a, 0x3e, 0x65, 0xcd, 0xcc, 0x8d, 0xf9, 0x39, 0x5a, 0xde, 0xcb, 0xbd, 0x46, 0xb8, 0xcd, 0xc, 0x88, 0x62, 0xfb, 0x1d, 0xb9, 0xca, 0x77 };

        [TestMethod]
        public MFTestResults X509CertificateLoadCertsFromBlob_Test()
        {
            bool bRet = true;
            X509CertificateLoadCertsFromBlob x509;

            //TestLibrary.TestFramework.BeginTestCase("X509CertificateLoadCertsFromBlob");

            x509 = new X509CertificateLoadCertsFromBlob();

            c_BYTES = Properties.Resources.GetBytes(Properties.Resources.BinaryResources.cacert);

            using(Session session = new Session("", MechanismType.RSA_PKCS))
            {
                bRet &= x509.RunTests(session);
            }
            if(m_isEmulator)
            {
                using (Session session = new Session("Emulator_Crypto", MechanismType.RSA_PKCS))
                {
                    bRet &= x509.RunTests(session);
                }
            }

            return bRet ? MFTestResults.Pass : MFTestResults.Fail;
        }

        public bool RunTests(Session session)
        {
            bool retVal = true;

            Log.Comment("[Positive]");
            retVal = PosTest1(session) && retVal;
            retVal = PosTestCER(session) && retVal;
            retVal = PosTestPK12(session) && retVal;

            Log.Comment("");

            Log.Comment("[Negative]");
            retVal = NegTest1(session) && retVal;
            retVal = NegTest2(session) && retVal;
            retVal = NegTest3(session) && retVal;
            //retVal = NegTest4(session) && retVal;

            return retVal;
        }

        public bool PosTest1(Session session)
        {
            bool retVal = true;
            X509Certificate2 cer;

            try
            {
                cer = new X509Certificate2(session, c_BYTES, "");

                if (null == cer)
                {
                    Log.Comment("-01: .ctor(byte[]) failed");
                    retVal = false;
                }

                if (cer.HasPrivateKey) // emulator cannot/willnot parse RSA PRIVATE KEY portion
                {
                    if(cer.KeyType == CryptoKey.KeyType.RSA)
                    {
                        RSACryptoServiceProvider priv = cer.PrivateKey as RSACryptoServiceProvider;
                        RSACryptoServiceProvider pub  = cer.PublicKey as RSACryptoServiceProvider;

                        string txt = "A text string that will be encrypted and signed";

                        byte[] data = UTF8Encoding.UTF8.GetBytes(txt);

                        byte[] enc = pub.Encrypt(data);
                        byte[] dec = priv.Decrypt(enc);

                        string newstr = new string(UTF8Encoding.UTF8.GetChars(dec));
                        retVal &= txt == newstr;

                        enc = priv.SignData(data);
                        retVal &= pub.VerifyData(data, enc);

                    }
                }
            }
            catch (Exception e)
            {
                Log.Comment("000: Unexpected exception: " + e);
                retVal = false;
            }

            return retVal;
        }

        public bool PosTestPK12(Session session)
        {
            bool retVal = true;
            X509Certificate2 cer;

            try
            {
                byte[] certBytes = Properties.Resources.GetBytes(Properties.Resources.BinaryResources.NetMFCert);

                cer = new X509Certificate2(session, certBytes, "NetMF");

                if (null == cer)
                {
                    Log.Comment("-01: .ctor(byte[]) failed");
                    retVal = false;
                }

                if (cer.HasPrivateKey)
                {
                    if (cer.KeyType == CryptoKey.KeyType.RSA)
                    {
                        RSACryptoServiceProvider priv = cer.PrivateKey as RSACryptoServiceProvider;
                        RSACryptoServiceProvider pub = cer.PublicKey as RSACryptoServiceProvider;

                        string txt = "A text string that will be encrypted and signed";

                        byte[] data = UTF8Encoding.UTF8.GetBytes(txt);

                        byte[] enc;
                        byte[] dec;

                        enc = pub.Encrypt(data);
                        dec = priv.Decrypt(enc);

                        string newstr = new string(UTF8Encoding.UTF8.GetChars(dec));
                        retVal &= txt == newstr;

                        enc = priv.SignData(data);
                        retVal &= pub.VerifyData(data, enc);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Comment("000: Unexpected exception: " + e);
                retVal = false;
            }

            return retVal;
        }

        public bool PosTestCER(Session session)
        {
            bool retVal = true;
            X509Certificate2 cer;

            //TestLibrary.TestFramework.BeginScenario("PosTest1: .ctor(byte[])");

            try
            {
                byte[] cert = Properties.Resources.GetBytes(Properties.Resources.BinaryResources.msft1);

                cer = new X509Certificate2(session, cert, "");

                DateTime dt = cer.NotAfter;
                dt = cer.NotBefore;

                if (null == cer)
                {
                    Log.Comment("-01: .ctor(byte[]) failed");
                    retVal = false;
                }
            }
            catch (Exception e)
            {
                Log.Comment("000: Unexpected exception: " + e);
                retVal = false;
            }

            return retVal;
        }

        public bool NegTest1(Session session)
        {
            bool retVal = true;
            X509Certificate2 cer;
            byte[] bytes;

            //TestLibrary.TestFramework.BeginScenario("NegTest1: Null blob");

            try
            {
                bytes = null;
                cer = new X509Certificate2(session, bytes, "");

                Log.Comment("001: Exception should have been thrown.");
                retVal = false;
            }
            catch (ArgumentException)
            {
                // expected
            }
            catch (Exception e)
            {
                Log.Comment("002: Unexpected exception: " + e);
                retVal = false;
            }

            return retVal;
        }

        public bool NegTest2(Session session)
        {
            bool retVal = true;
            X509Certificate2 cer;
            byte[] bytes;

            //TestLibrary.TestFramework.BeginScenario("NegTest2: 0 Length blob");

            try
            {
                bytes = new byte[0];
                cer = new X509Certificate2(session, bytes, "");

                Log.Comment("003: Exception should have been thrown.");
                retVal = false;
            }
            catch (ArgumentException)
            {
                // expected
            }
            catch (Exception e)
            {
                Log.Comment("004: Unexpected exception: " + e);
                retVal = false;
            }

            return retVal;
        }

        public bool NegTest3(Session session)
        {
            bool retVal = true;
            X509Certificate2 cer;
            byte[] bytes;
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider(session);

            //TestLibrary.TestFramework.BeginScenario("NegTest3: random blobs");

            try
            {
                foreach (int size in c_SIZES)
                {
                    try
                    {
                        Log.Comment("Blob size = " + size);
                        bytes = new byte[size];
                        rng.GetBytes(bytes);
                        cer = new X509Certificate2(session, bytes, "");

                        Log.Comment("003: Exception should have been thrown.");
                        retVal = false;
                    }
                    catch (CryptographicException)
                    {
                        // expected
                        // ensure that the message does NOT contain "Unknown error"
                    }
                }
            }
            catch (Exception e)
            {
                Log.Comment("004: Unexpected exception: " + e);
                retVal = false;
            }

            return retVal;
        }

        //public bool NegTest4(Session session)
        //{
        //    bool retVal = true;
        //    X509Certificate2 cer;

        //    //TestLibrary.TestFramework.BeginScenario("NegTest4: invalid X509StorageFlags");

        //    try
        //    {
        //        for (int flags = c_MINFLAGS; flags < c_MAXFLAGS; flags++)
        //        {
        //            if ((int)X509KeyStorageFlags.DefaultKeySet != flags)
        //            {
        //                try
        //                {
        //                    cer = new X509Certificate(c_BYTES, "", (X509KeyStorageFlags)flags);

        //                    Log.Comment("005: Exception should have been thrown. Flags(" + flags + ")");
        //                    retVal = false;
        //                }
        //                catch (ArgumentException)
        //                {
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Comment("006: Unexpected exception: " + e);
        //        retVal = false;
        //    }

        //    return retVal;
        //}
    }
}