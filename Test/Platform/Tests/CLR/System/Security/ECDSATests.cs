using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using Microsoft.SPOT.Cryptoki;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class ECDsaTest : IMFTestInterface
    {
        static bool m_isEmulator = false;

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
    
        private delegate bool Test(Session session);

        private struct TestCase
        {
            public Test Test;
            public string Name;

            public TestCase(string name, Test test)
            {
                Test = test;
                Name = name;
            }
        }

        [TestMethod]
        public MFTestResults ECDsa_Test()
        {
            TestCase[] tests = new TestCase[]
            {
                new TestCase("Create tests", TestCreate),
                new TestCase("SignHash", TestSignHash),
                new TestCase("SignData (buffer)", TestSignDataBuffer),
                new TestCase("SignData (stream)", TestSignDataStream),
                //new TestCase("Roundtrip XML", TestXml)
            };

            bool passed = true;

            using (Session session = new Session("", MechanismType.ECDSA))
            {
                foreach (TestCase test in tests)
                {
                    if (!RunTest(session, test))
                        passed = false;
                }
            }

            if (m_isEmulator)
            {
                using (Session session = new Session("Emulator_Crypto", MechanismType.ECDSA))
                {
                    foreach (TestCase test in tests)
                    {
                        if (!RunTest(session, test))
                            passed = false;
                    }
                }
            }

            return passed ? MFTestResults.Pass : MFTestResults.Fail;
        }

        private static bool CompareBytes(byte[] lhs, byte[] rhs)
        {
            if (lhs == null)
                return rhs == null;

            if (lhs.Length != rhs.Length)
                return false;

            for (int i = 0; i < lhs.Length; i++)
            {
                if (lhs[i] != rhs[i])
                    return false;
            }

            return true;
        }

        private static bool RunTest(Session session, TestCase test)
        {
            WriteStatus(test.Name);

            try
            {
                if (test.Test(session))
                {
                    WriteStatus("    OK");
                    return true;
                }
                else
                {
                    WriteStatus("    Fail");
                    return false;
                }
            }
            catch (Exception e)
            {
                WriteStatus("Failed -- got an exception");
                WriteStatus(e.ToString());
                return false;
            }
        }

        private static bool TestCreate(Session session)
        {
            bool passed = true;

            using (ECDsaCryptoServiceProvider dsa = new ECDsaCryptoServiceProvider(session))
                passed &= dsa != null && dsa is ECDsaCryptoServiceProvider;

            return passed;
        }

        private static bool TestSignHash(Session session)
        {
            bool passed = true;
            byte[] hashValue = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 };

            using (ECDsaCryptoServiceProvider dsa = new ECDsaCryptoServiceProvider(session))
            {
                byte[] signature = dsa.SignHash(hashValue, MechanismType.SHA_1);
                passed &= dsa.VerifyHash(hashValue, MechanismType.SHA_1, signature);
            }

            return passed;
        }

        private static bool TestSignDataBuffer(Session session)
        {
            bool passed = true;

            byte[] data = new byte[10000];
            for (int i = 0; i < data.Length; i++)
                data[i] = (byte)(i % 256);

            MechanismType[] hashAlgorithms = new MechanismType[]
        {
            // MechanismType.MD5, - currently not supported in OpenSSL for ECDSA
            MechanismType.SHA_1,
            MechanismType.SHA256,
            MechanismType.SHA384,
            MechanismType.SHA512,
        };

            int[] keySizes = new int[] { 256, 384, 521 };

            foreach (int keySize in keySizes)
            {
                WriteLine("Using key size " + keySize);

                foreach (MechanismType hashAlgorithm in hashAlgorithms)
                {
                    WriteLine("Using hash algorithm " + hashAlgorithm.ToString());

                    using (ECDsaCryptoServiceProvider sign = new ECDsaCryptoServiceProvider(session, keySize))
                    using (ECDsaCryptoServiceProvider verify = new ECDsaCryptoServiceProvider(sign.KeyPair))
                    {
                        sign.HashAlgorithm = hashAlgorithm;
                        verify.HashAlgorithm = hashAlgorithm;

                        try
                        {
                            byte[] signature = sign.SignData(data);
                            bool verified = verify.VerifyData(data, signature);

                            if (!verified)
                                WriteLine("Did not verify");

                            passed &= verified;
                        }
                        catch
                        {
                            passed = false;
                        }
                    }
                }
            }

            return passed;
        }

        private static bool TestSignDataStream(Session session)
        {
            bool passed = true;

            byte[] data = new byte[10000];
            for (int i = 0; i < data.Length; i++)
                data[i] = (byte)(i % 256);

            MechanismType[] hashAlgorithms = new MechanismType[]
            {
                //MechanismType.MD5,  currently not supported in OpenSSL for DSA
                MechanismType.SHA_1,
                MechanismType.SHA256,
                MechanismType.SHA384,
                MechanismType.SHA512
            };
            int[] keySizes = new int[] { 256, 384, 521 };

            foreach (int keySize in keySizes)
            {
                WriteLine("Using key size " + keySize);

                foreach (MechanismType hashAlgorithm in hashAlgorithms)
                {
                    WriteLine("Using hash algorithm " + hashAlgorithm.ToString());

                    using (ECDsaCryptoServiceProvider sign = new ECDsaCryptoServiceProvider(session))
                    {
                        sign.KeySize = keySize;

                        using (ECDsaCryptoServiceProvider verify1 = new ECDsaCryptoServiceProvider(sign.KeyPair))
                        using (ECDsaCryptoServiceProvider verify2 = new ECDsaCryptoServiceProvider(sign.KeyPair))
                        //using (MemoryStream ms = new MemoryStream(data))
                        {
                            sign.HashAlgorithm = hashAlgorithm;
                            verify1.HashAlgorithm = hashAlgorithm;
                            verify2.HashAlgorithm = hashAlgorithm;

                            byte[] signature = sign.SignData(data); //ms);

                            //ms.Position = 0;
                            bool verified = verify1.VerifyData(data, signature);
                            if (!verified)
                                WriteLine("Did not verify via stream");
                            passed &= verified;

                            verified = verify2.VerifyData(data, signature);
                            if (!verified)
                                WriteLine("Did not verify via array");
                            passed &= verified;
                        }
                    }
                }
            }

            return passed;
        }

        //private static bool TestXml()
        //{
        //    bool passed = true;

        //    int[] keySizes = new int[] { 256, 384, 521 };
        //    foreach (int keySize in keySizes)
        //    {
        //        WriteLine("Using key size " + keySize);

        //        using (ECDsaCryptoServiceProvider dsa = new ECDsaCryptoServiceProvider(keySize))
        //        {
        //            string xml = dsa.ToXmlString(ECKeyXmlFormat.Rfc4050);
        //            WriteLine("Key XML:");
        //            WriteLine(xml);

        //            using (ECDsaCng dsa2 = new ECDsaCng())
        //            {
        //                WriteLine("Importing");
        //                dsa2.FromXmlString(xml, ECKeyXmlFormat.Rfc4050);

        //                WriteLine("Comparing public key blobs");
        //                byte[] originalKey = dsa.Key.Export(CngKeyBlobFormat.EccPublicBlob);
        //                byte[] roundtripKey = dsa2.Key.Export(CngKeyBlobFormat.EccPublicBlob);

        //                if (!CompareBytes(originalKey, roundtripKey))
        //                {
        //                    passed &= false;
        //                }

        //                WriteLine("Doing signature verification");
        //                byte[] hashValue = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 };
        //                byte[] signature = dsa.SignHash(hashValue);
        //                passed &= dsa2.VerifyHash(hashValue, signature);
        //            }
        //        }
        //    }

        //    return passed;
        //}

        private static void WriteLine(string data)
        {
            Log.Comment(data);
        }

        private static void WriteStatus(string data)
        {
            Log.Comment(data);
        }
    }
}