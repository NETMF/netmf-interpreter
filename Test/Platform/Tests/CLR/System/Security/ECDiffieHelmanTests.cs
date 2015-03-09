using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using Microsoft.SPOT.Cryptoki;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class ECDiffieHellmanTest : IMFTestInterface
    {
        static internal bool m_isEmulator = false;

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

        private delegate bool Test();

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
        public MFTestResults ECDH_Test()
        {
            TestCase[] tests = new TestCase[]
            {
                new TestCase("Create tests", TestCreate),
                new TestCase("Key exchange (Hash)", TestKeyExchangeHash),
                //new TestCase("Key exchange (HMAC)", TestKeyExchangeHmac),
                //new TestCase("Key exchange (TLS)", TestKeyExchangeTls),
                //new TestCase("Load intereseting XML", TestLoadInterestingXml),
                //new TestCase("Roundtrip XML", TestXml)
            };

            bool passed = true;
            foreach (TestCase test in tests)
            {
                if (!RunTest(test))
                    passed = false;
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

        private static bool RunTest(TestCase test)
        {
            WriteStatus(test.Name);

            try
            {
                if (test.Test())
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

        private static bool TestCreate()
        {
            bool passed = true;

            using (ECDiffieHellmanCryptoServiceProvider dh = new ECDiffieHellmanCryptoServiceProvider())
                passed &= dh != null && dh is ECDiffieHellmanCryptoServiceProvider;

            if (ECDiffieHellmanTest.m_isEmulator)
            {
                using (ECDiffieHellmanCryptoServiceProvider dh = new ECDiffieHellmanCryptoServiceProvider("Emulator_Crypto"))
                    passed &= dh != null && dh is ECDiffieHellmanCryptoServiceProvider;
            }

            return passed;
        }

        private static bool TestKeyExchangeHash()
        {
            bool passed = true;

            int[] keySizes = new int[] { 256, 384, 521 };
            MechanismType[] hashAlgorithms = new MechanismType[]
            {
                MechanismType.MD5,
                MechanismType.SHA_1,
                MechanismType.SHA256,
                MechanismType.SHA384,
                MechanismType.SHA512
            };

            foreach (int keySize in keySizes)
            {
                WriteLine("  Using key size " + keySize);

                foreach (MechanismType hashAlgorithm in hashAlgorithms)
                {
                    WriteLine("    Hash algorithm " + hashAlgorithm.ToString());

                    using (ECDiffieHellmanCryptoServiceProvider dh = new ECDiffieHellmanCryptoServiceProvider())
                    using (ECDiffieHellmanCryptoServiceProvider dh2 = new ECDiffieHellmanCryptoServiceProvider())
                    {
                        dh.KeySize = keySize;
                        dh2.KeySize = keySize;

                        dh.HashAlgorithm = hashAlgorithm;
                        dh2.HashAlgorithm = hashAlgorithm;

                        CryptoKey k1 = dh.DeriveKeyMaterial(dh2.PublicKey);
                        CryptoKey k2 = dh2.DeriveKeyMaterial(dh.PublicKey);

                        byte[] key1 = k1.ExportKey(true);
                        byte[] key2 = k2.ExportKey(true);

                        WriteLine("      " + key1.Length * 8 + " bit key generated");

                        if (key1 == null || key2 == null)
                        {
                            WriteLine("Fail -- null key");
                            passed = false;
                        }

                        if (!CompareBytes(key1, key2))
                        {
                            WriteLine("Fail");
                            passed = false;
                        }
                    }
                }
            }

            return passed;
        }

        private static bool TestKeyExchangeHmac()
        {
            bool passed = true;

            int[] keySizes = new int[] { 256, 384, 521 };
            MechanismType[] hashAlgorithms = new MechanismType[]
            {
                MechanismType.MD5,
                MechanismType.SHA_1,
                MechanismType.SHA256,
                MechanismType.SHA384,
                MechanismType.SHA512
            };

            foreach (int keySize in keySizes)
            {
                WriteLine("  Using key size " + keySize);

                foreach (MechanismType hashAlgorithm in hashAlgorithms)
                {
                    WriteLine("    Hash algorithm " + hashAlgorithm.ToString());

                    using (ECDiffieHellmanCryptoServiceProvider dh = new ECDiffieHellmanCryptoServiceProvider())
                    using (ECDiffieHellmanCryptoServiceProvider dh2 = new ECDiffieHellmanCryptoServiceProvider())
                    {
                        dh.KeySize = keySize;
                        dh2.KeySize = keySize;

                        dh.HashAlgorithm = hashAlgorithm;
                        dh2.HashAlgorithm = hashAlgorithm;

                        dh.SecretAppend = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
                        dh2.SecretAppend = dh.SecretAppend;

                        dh.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hmac;
                        dh2.KeyDerivationFunction = dh.KeyDerivationFunction;

                        CryptoKey k1 = dh.DeriveKeyMaterial(dh2.PublicKey);
                        CryptoKey k2 = dh2.DeriveKeyMaterial(dh.PublicKey);

                        byte[] key1 = k1.ExportKey(true);
                        byte[] key2 = k2.ExportKey(true);

                        WriteLine("      " + key1.Length * 8 + " bit key generated");

                        if (key1 == null || key2 == null)
                        {
                            WriteLine("Fail -- null key");
                            passed = false;
                        }

                        if (!CompareBytes(key1, key2))
                        {
                            WriteLine("Fail");
                            passed = false;
                        }
                    }
                }
            }

            return passed;
        }

        //private static bool TestKeyExchangeTls()
        //{
        //    bool passed = true;

        //    int[] keySizes = new int[] { 256, 384, 521 };

        //    foreach (int keySize in keySizes)
        //    {
        //        WriteLine("  Using key size " + keySize);

        //        using (ECDiffieHellmanCryptoServiceProvider dh = new ECDiffieHellmanCryptoServiceProvider(keySize))
        //        using (ECDiffieHellmanCryptoServiceProvider dh2 = new ECDiffieHellmanCryptoServiceProvider(keySize))
        //        {
        //            dh.Seed = new byte[64];
        //            dh2.Seed = dh.Seed;

        //            dh.Label = new byte[32];
        //            dh2.Label = dh.Label;

        //            dh.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Tls;
        //            dh2.KeyDerivationFunction = dh.KeyDerivationFunction;

        //            byte[] key1 = dh.DeriveKeyMaterial(dh2.PublicKey);
        //            byte[] key2 = dh2.DeriveKeyMaterial(dh.PublicKey);
        //            WriteLine(String.Format("    {0} bit key generated", key1.Length * 8));

        //            if (key1 == null || key2 == null)
        //            {
        //                WriteLine("Fail -- null key");
        //                passed = false;
        //            }

        //            if (!CompareBytes(key1, key2))
        //            {
        //                WriteLine("Fail");
        //                passed = false;
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