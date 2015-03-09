using System;
using System.Reflection;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.Security.Cryptography;
using Microsoft.SPOT.Cryptoki;

namespace Microsoft.SPOT.Platform.Tests
{

    public class HMACTests : IMFTestInterface
    {
        bool m_isEmulator = false;

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
            try
            {
            }
            catch
            { }
        }

        [TestMethod]
        public MFTestResults HmacTest_Compare()
        {
            bool testResult = false;

            if (!m_isEmulator) return MFTestResults.Skip;

            try
            {
                CryptokiAttribute[] secretKey = new CryptokiAttribute[]
                    {
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.Class  , new byte[] {    4, 0, 0, 0}),
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.KeyType, new byte[] { 0x10, 0, 0, 0}),
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.Value  , new byte[20])
                    };
                Mechanism mech = new Mechanism(MechanismType.SHA_1_HMAC);

                using (Session openSession = new Session("", mech.Type))
                {
                    using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider(openSession))
                    {
                        rng.GetBytes(secretKey[2].Value);
                    }

                    using (CryptoKey keyOpen = CryptoKey.LoadKey(openSession, secretKey))
                    {
                        string dataToSign = "This is a simple message to be encrypted";

                        byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes(dataToSign);

                        using (KeyedHashAlgorithm hmacOpenSSL = new KeyedHashAlgorithm(KeyedHashAlgorithmType.HMACSHA1, keyOpen))
                        {
                            byte[] hmac1 = hmacOpenSSL.ComputeHash(data);

                            using (Session emuSession = new Session("Emulator_Crypto", mech.Type))
                            {
                                using (CryptoKey keyEmu = CryptoKey.LoadKey(emuSession, secretKey))
                                {

                                    using (KeyedHashAlgorithm hmacEmu = new KeyedHashAlgorithm(KeyedHashAlgorithmType.HMACSHA1, keyEmu))
                                    {
                                        byte[] hmac2 = hmacEmu.ComputeHash(data);

                                        testResult = true;

                                        for (int i = 0; i < hmac1.Length; i++)
                                        {
                                            if (hmac1[i] != hmac2[i])
                                            {
                                                testResult = false;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);

        }

        [TestMethod]
        public MFTestResults HmacSHA256Test_Compare()
        {
            bool testResult = false;

            if (!m_isEmulator) return MFTestResults.Skip;

            try
            {
                CryptokiAttribute[] secretKey = new CryptokiAttribute[]
                    {
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.Class  , new byte[] {    4, 0, 0, 0}),
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.KeyType, new byte[] { 0x10, 0, 0, 0}),
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.Value  , new byte[256/8])
                    };

                Mechanism mech = new Mechanism(MechanismType.SHA256_HMAC);

                using (Session openSession = new Session("", mech.Type))
                {
                    using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider(openSession))
                    {
                        rng.GetBytes(secretKey[2].Value);
                    }

                    using (CryptoKey keyOpen = CryptoKey.LoadKey(openSession, secretKey))
                    {
                        string dataToSign = "This is a simple message to be encrypted";

                        byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes(dataToSign);

                        using (KeyedHashAlgorithm hmacOpenSSL = new KeyedHashAlgorithm(KeyedHashAlgorithmType.HMACSHA256, keyOpen))
                        {
                            byte[] hmac1 = hmacOpenSSL.ComputeHash(data);

                            using (Session emuSession = new Session("Emulator_Crypto", mech.Type))
                            {
                                using (CryptoKey keyEmu = CryptoKey.LoadKey(emuSession, secretKey))
                                {
                                    using (KeyedHashAlgorithm hmacEmu = new KeyedHashAlgorithm(KeyedHashAlgorithmType.HMACSHA256, keyEmu))
                                    {
                                        byte[] hmac2 = hmacEmu.ComputeHash(data);

                                        testResult = true;

                                        for (int i = 0; i < hmac1.Length; i++)
                                        {
                                            if (hmac1[i] != hmac2[i])
                                            {
                                                testResult = false;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);

        }

        [TestMethod]
        public MFTestResults Hmac384Test_Compare()
        {
            bool testResult = false;

            if (!m_isEmulator) return MFTestResults.Skip;

            try
            {
                CryptokiAttribute[] secretKey = new CryptokiAttribute[]
                    {
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.Class  , new byte[] {    4, 0, 0, 0}),
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.KeyType, new byte[] { 0x10, 0, 0, 0}),
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.Value  , new byte[384/8])
                    };

                Mechanism mech = new Mechanism(MechanismType.SHA384_HMAC);

                using (Session openSession = new Session("", mech.Type))
                {
                    using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider(openSession))
                    {
                        rng.GetBytes(secretKey[2].Value);
                    }

                    using (CryptoKey keyOpen = CryptoKey.LoadKey(openSession, secretKey))
                    {

                        string dataToSign = "This is a simple message to be encrypted";

                        byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes(dataToSign);

                        using (KeyedHashAlgorithm hmacOpenSSL = new KeyedHashAlgorithm(KeyedHashAlgorithmType.HMACSHA384, keyOpen))
                        {
                            byte[] hmac1 = hmacOpenSSL.ComputeHash(data);

                            using (Session emuSession = new Session("Emulator_Crypto", mech.Type))
                            {
                                using (CryptoKey keyEmu = CryptoKey.LoadKey(emuSession, secretKey))
                                {
                                    using (KeyedHashAlgorithm hmacEmu = new KeyedHashAlgorithm(KeyedHashAlgorithmType.HMACSHA384, keyEmu))
                                    {
                                        byte[] hmac2 = hmacEmu.ComputeHash(data);

                                        testResult = true;

                                        for (int i = 0; i < hmac1.Length; i++)
                                        {
                                            if (hmac1[i] != hmac2[i])
                                            {
                                                testResult = false;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);

        }

        [TestMethod]
        public MFTestResults Hmac512Test_Compare()
        {
            bool testResult = false;

            if (!m_isEmulator) return MFTestResults.Skip;

            try
            {
                CryptokiAttribute[] secretKey = new CryptokiAttribute[]
                    {
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.Class  , new byte[] {    4, 0, 0, 0}),
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.KeyType, new byte[] { 0x10, 0, 0, 0}),
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.Value  , new byte[512/8])
                    };

                Mechanism mech = new Mechanism(MechanismType.SHA512_HMAC);

                using (Session openSession = new Session("", mech.Type))
                {
                    using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider(openSession))
                    {
                        rng.GetBytes(secretKey[2].Value);
                    }

                    using (CryptoKey keyOpen = CryptoKey.LoadKey(openSession, secretKey))
                    {

                        string dataToSign = "This is a simple message to be encrypted";

                        byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes(dataToSign);

                        using (KeyedHashAlgorithm hmacOpenSSL = new KeyedHashAlgorithm(KeyedHashAlgorithmType.HMACSHA512, keyOpen))
                        {
                            byte[] hmac1 = hmacOpenSSL.ComputeHash(data);

                            using (Session emuSession = new Session("Emulator_Crypto", mech.Type))
                            {
                                using (CryptoKey keyEmu = CryptoKey.LoadKey(emuSession, secretKey))
                                {

                                    using (KeyedHashAlgorithm hmacEmu = new KeyedHashAlgorithm(KeyedHashAlgorithmType.HMACSHA512, keyEmu))
                                    {
                                        byte[] hmac2 = hmacEmu.ComputeHash(data);

                                        testResult = true;

                                        for (int i = 0; i < hmac1.Length; i++)
                                        {
                                            if (hmac1[i] != hmac2[i])
                                            {
                                                testResult = false;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);

        }

        [TestMethod]
        public MFTestResults HmacMD5Test_Compare()
        {
            bool testResult = false;

            if (!m_isEmulator) return MFTestResults.Skip;

            try
            {
                CryptokiAttribute[] secretKey = new CryptokiAttribute[]
                    {
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.Class  , new byte[] {    4, 0, 0, 0}),
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.KeyType, new byte[] { 0x10, 0, 0, 0}),
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.Value  , new byte[128/8])
                    };

                Mechanism mech = new Mechanism(MechanismType.MD5_HMAC);

                using (Session openSession = new Session("", mech.Type))
                {
                    using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider(openSession))
                    {
                        rng.GetBytes(secretKey[2].Value);
                    }

                    using (CryptoKey keyOpen = CryptoKey.LoadKey(openSession, secretKey))
                    {
                        string dataToSign = "This is a simple message to be encrypted";

                        byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes(dataToSign);

                        using (KeyedHashAlgorithm hmacOpenSSL = new KeyedHashAlgorithm(KeyedHashAlgorithmType.HMACMD5, keyOpen))
                        {
                            byte[] hmac1 = hmacOpenSSL.ComputeHash(data);

                            using (Session emuSession = new Session("Emulator_Crypto", mech.Type))
                            {
                                using (CryptoKey keyEmu = CryptoKey.LoadKey(emuSession, secretKey))
                                {
                                    using (KeyedHashAlgorithm hmacEmu = new KeyedHashAlgorithm(KeyedHashAlgorithmType.HMACMD5, keyEmu))
                                    {
                                        byte[] hmac2 = hmacEmu.ComputeHash(data);

                                        testResult = true;

                                        for (int i = 0; i < hmac1.Length; i++)
                                        {
                                            if (hmac1[i] != hmac2[i])
                                            {
                                                testResult = false;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);

        }

        [TestMethod]
        public MFTestResults RIPEMD160Test_Compare()
        {
            bool testResult = false;

            if (!m_isEmulator) return MFTestResults.Skip;

            try
            {
                CryptokiAttribute[] secretKey = new CryptokiAttribute[]
                    {
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.Class  , new byte[] {    4, 0, 0, 0}),
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.KeyType, new byte[] { 0x10, 0, 0, 0}),
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.Value  , new byte[128/8])
                    };

                Mechanism mech = new Mechanism(MechanismType.RIPEMD160_HMAC);

                using (Session openSession = new Session("", mech.Type))
                {
                    using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider(openSession))
                    {
                        rng.GetBytes(secretKey[2].Value);
                    }

                    using (CryptoKey keyOpen = CryptoKey.LoadKey(openSession, secretKey))
                    {
                        string dataToSign = "This is a simple message to be encrypted";

                        byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes(dataToSign);

                        using (KeyedHashAlgorithm hmacOpenSSL = new KeyedHashAlgorithm(KeyedHashAlgorithmType.HMACRIPEMD160, keyOpen))
                        {
                            byte[] hmac1 = hmacOpenSSL.ComputeHash(data);

                            using (Session emuSession = new Session("Emulator_Crypto", mech.Type))
                            {
                                using (CryptoKey keyEmu = CryptoKey.LoadKey(emuSession, secretKey))
                                {
                                    using (KeyedHashAlgorithm hmacEmu = new KeyedHashAlgorithm(KeyedHashAlgorithmType.HMACRIPEMD160, keyEmu))
                                    {
                                        byte[] hmac2 = hmacEmu.ComputeHash(data);

                                        testResult = true;

                                        for (int i = 0; i < hmac1.Length; i++)
                                        {
                                            if (hmac1[i] != hmac2[i])
                                            {
                                                testResult = false;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);

        }

        [TestMethod]
        public MFTestResults HmacTest_Test1()
        {
            bool testResult = false;

            try
            {
                string dataToSign = "This is a simple message to be encrypted";

                byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes(dataToSign);

                Mechanism mech = new Mechanism(MechanismType.SHA_1_HMAC);

                using (Session openSession = new Session("", mech.Type))
                {
                    using (KeyedHashAlgorithm hmacOpenSSL = new KeyedHashAlgorithm(KeyedHashAlgorithmType.HMACSHA1, openSession))
                    {

                        byte[] hmac1 = hmacOpenSSL.ComputeHash(data);

                        byte[] hmac2 = hmacOpenSSL.ComputeHash(data);

                        data[3] = (byte)((data[3] & 1) == 0 ? data[3] + 1 : data[3] - 1);

                        byte[] hmac3 = hmacOpenSSL.ComputeHash(data);

                        testResult = true;
                        bool difFound = false;

                        for (int i = 0; i < hmac1.Length; i++)
                        {
                            if (hmac1[i] != hmac2[i])
                            {
                                testResult = false;
                                break;
                            }
                            if (hmac1[i] != hmac1[3])
                            {
                                difFound = true;
                            }
                        }
                        testResult &= difFound;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);

        }
    }
}
