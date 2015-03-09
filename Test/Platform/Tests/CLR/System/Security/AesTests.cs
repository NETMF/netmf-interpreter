using System;
using System.Reflection;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.Security.Cryptography;
using Microsoft.SPOT.Cryptoki;

namespace Microsoft.SPOT.Platform.Tests
{

    public class AesTests : IMFTestInterface
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

        [TestMethod]
        public MFTestResults AesTest_EncryptUpdate()
        {
            MFTestResults res;

            try
            {
                using (AesCryptoServiceProvider csp = new AesCryptoServiceProvider())
                {
                    res = SymmetricTestHelper.Test_EncryptUpdate(csp);
                }

                if (res == MFTestResults.Pass && m_isEmulator)
                {
                    using (AesCryptoServiceProvider csp = new AesCryptoServiceProvider("Emulator_Crypto"))
                    {
                        res = SymmetricTestHelper.Test_EncryptUpdate(csp);
                    }
                }
            }
            catch
            {
                res = MFTestResults.Fail;
            }

            return res;
        }

        [TestMethod]
        public MFTestResults AesTest_Encrypt()
        {
            MFTestResults res;

            try
            {
                using (AesCryptoServiceProvider csp = new AesCryptoServiceProvider())
                {
                    res = SymmetricTestHelper.Test_Encrypt(csp);
                }

                if (res == MFTestResults.Pass && m_isEmulator)
                {
                    using (AesCryptoServiceProvider csp = new AesCryptoServiceProvider("Emulator_Crypto"))
                    {
                        res = SymmetricTestHelper.Test_Encrypt(csp);
                    }
                }
            }
            catch
            {
                res = MFTestResults.Fail;
            }

            return res;
        }


        [TestMethod]
        public MFTestResults AesTest_FromDesktop()
        {
            if (m_isEmulator)
            {
                return AesTest.DoAesTest() ? MFTestResults.Pass : MFTestResults.Fail;
            }

            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults AesTest_BlockTest()
        {
            if (m_isEmulator)
            {
                return BlockTest.DoBlockTest() ? MFTestResults.Pass : MFTestResults.Fail;
            }

            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults AesTest_KeySizeTest()
        {
            return KeySizeTest.DoKeySizeTest(m_isEmulator) ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults AesTest_KeyTest()
        {
            return KeyTest.DoKeyTest() ? MFTestResults.Pass : MFTestResults.Fail;
        }


        [TestMethod]
        public MFTestResults AesTest_EncryptCsps()
        {
            MFTestResults res = MFTestResults.Skip;

            try
            {
                if (m_isEmulator)
                {
                    using (AesCryptoServiceProvider csp1 = new AesCryptoServiceProvider())
                    {
                        using (AesCryptoServiceProvider csp2 = new AesCryptoServiceProvider("Emulator_Crypto"))
                        {
                            res = SymmetricTestHelper.Test_EncryptCsp(csp1, csp2, m_key);
                        }
                    }
                }
            }
            catch
            {
                res = MFTestResults.Fail;
            }

            return res;
        }

        CryptokiAttribute[] m_key = new CryptokiAttribute[]
                    {
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.Class  , Utility.ConvertToBytes((int)CryptokiClass.SECRET_KEY)),
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.KeyType, Utility.ConvertToBytes((int)CryptoKey.KeyType.AES)),
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.Value  , new byte[0x100/8])
                    };

    }

    public static class AesTest
    {
        public static bool DoAesTest()
        {
            CipherMode[] cipherModes = new CipherMode[] { CipherMode.ECB, CipherMode.CBC };
            PaddingMode[] paddingModes = new PaddingMode[] { PaddingMode.None, PaddingMode.PKCS7 }; //, PaddingMode.Zeros, PaddingMode.ANSIX923, PaddingMode.ISO10126 };

            try
            {
                using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
                using (AesCryptoServiceProvider baseline = new AesCryptoServiceProvider("Emulator_Crypto"))
                {
                    foreach (KeySizes keySize in aes.LegalKeySizes)
                    {
                        aes.KeySize = keySize.MinSize;
                        do
                        {
                            aes.GenerateKey();
                            aes.GenerateIV();
                            aes.FeedbackSize = 64;

                            baseline.KeySize = aes.KeySize;
                            baseline.Key = aes.Key;
                            baseline.IV = aes.IV;
                            baseline.FeedbackSize = aes.FeedbackSize;

                            foreach (CipherMode cipherMode in cipherModes)
                            {
                                foreach (PaddingMode paddingMode in paddingModes)
                                {
                                    aes.Mode = cipherMode;
                                    aes.Padding = paddingMode;

                                    baseline.Mode = cipherMode;
                                    baseline.Padding = paddingMode;

                                    if (!Test(aes, baseline))
                                    {
                                        return false;
                                    }
                                }
                            }

                            aes.KeySize = aes.KeySize + keySize.SkipSize;
                        } while (aes.KeySize != keySize.MaxSize && keySize.SkipSize != 0);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool Test(SymmetricAlgorithm testAlgorithm, SymmetricAlgorithm baseline)
        {
            for (int i = 0; i <= (testAlgorithm.BlockSize * 4) / 8; i++)
            {
                if (!DeterministicTest(testAlgorithm, baseline, i))
                    return false;
            }

            return true;
        }

        private static bool TestRoundTrip(SymmetricAlgorithm testAlgorithm, SymmetricAlgorithm baseline, byte[] data)
        {
            if (testAlgorithm.Padding == PaddingMode.None && data.Length % testAlgorithm.BlockSize != 0)
                return true;

            try
            {
                byte[] testCipherValue;
                byte[] baselineCipherValue;

                //using (MemoryStream testEncrypted = new MemoryStream())
                //using (MemoryStream baselineEncrypted = new MemoryStream())
                using (ICryptoTransform testEncrypted = testAlgorithm.CreateEncryptor())
                using (ICryptoTransform baselineEncrypted = baseline.CreateEncryptor())
                {
                    //using (CryptoStream testEncryptor = new CryptoStream(testEncrypted, testAlgorithm.CreateEncryptor(), CryptoStreamMode.Write))
                    //using (CryptoStream baselineEncryptor = new CryptoStream(baselineEncrypted, baseline.CreateEncryptor(), CryptoStreamMode.Write))
                    //{
                    //testEncryptor.Write(data, 0, data.Length);
                    //testEncryptor.Close();

                    testCipherValue = testEncrypted.TransformFinalBlock(data, 0, data.Length);
                    baselineCipherValue = baselineEncrypted.TransformFinalBlock(data, 0, data.Length);

                    //baselineEncryptor.Write(data, 0, data.Length);
                    //baselineEncryptor.Close();

                    //testCipherValue = testEncrypted.ToArray();
                    //baselineCipherValue = baselineEncrypted.ToArray();
                    //}
                }

                byte[] testRoundtrip;
                byte[] baselineRoundtrip;

                //using (MemoryStream testDecrypted = new MemoryStream())
                //using (MemoryStream baselineDecrypted = new MemoryStream())
                using (ICryptoTransform testDecrypted = testAlgorithm.CreateDecryptor())
                using (ICryptoTransform baselineDecrypted = baseline.CreateDecryptor())
                {
                    testRoundtrip = testDecrypted.TransformFinalBlock(baselineCipherValue, 0, baselineCipherValue.Length);
                    baselineRoundtrip = baselineDecrypted.TransformFinalBlock(testCipherValue, 0, testCipherValue.Length);

                    //using (CryptoStream testDecryptor = new CryptoStream(testDecrypted, testAlgorithm.CreateDecryptor(), CryptoStreamMode.Write))
                    //using (CryptoStream baselineDecryptor = new CryptoStream(baselineDecrypted, baseline.CreateDecryptor(), CryptoStreamMode.Write))
                    //{
                    //testDecryptor.Write(baselineCipherValue, 0, baselineCipherValue.Length);
                    //testDecryptor.Close();

                    //baselineDecryptor.Write(testCipherValue, 0, testCipherValue.Length);
                    //baselineDecryptor.Close();

                    //testRoundtrip = testDecrypted.ToArray();
                    //baselineRoundtrip = baselineDecrypted.ToArray();
                    //}
                }

                if (!CompareBytes(testRoundtrip, baselineRoundtrip))
                {
                    Log.Comment("Roundtrip bytes do not match");
                    return false;
                }
                if (testAlgorithm.Padding != PaddingMode.Zeros && !CompareBytes(testRoundtrip, data))
                {
                    Log.Comment("Roundtrip does not match input");
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Log.Exception("Got an exception, fail", e);
                return false;
            }
        }

        private static bool DeterministicTest(SymmetricAlgorithm testAlgorithm, SymmetricAlgorithm baseline, int dataSize)
        {
            byte[] data = new byte[dataSize];
            for (int i = 0; i < dataSize; i++)
                data[i] = (byte)(i + 100);

            return TestRoundTrip(testAlgorithm, baseline, data);
        }

        private static bool CompareBytes(byte[] lhs, byte[] rhs)
        {
            if (lhs.Length != rhs.Length)
                return false;

            for (int i = 0; i < lhs.Length; i++)
            {
                if (lhs[i] != rhs[i])
                    return false;
            }

            return true;
        }
    }
}