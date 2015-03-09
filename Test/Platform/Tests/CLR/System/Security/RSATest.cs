using System;
using System.Reflection;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.Security.Cryptography;
using Microsoft.SPOT.Cryptoki;

namespace Microsoft.SPOT.Platform.Tests
{

    public class RSATests : IMFTestInterface
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
        }

        private bool CompareByteArray(byte[] d1, byte[] d2)
        {
            if (d1 == null || d2 == null) return d1 == d2;
            if (d1.Length != d2.Length) return false;

            for (int i = 0; i < d1.Length; i++)
            {
                if (d1[i] != d2[i]) return false;
            }

            return true;
        }

        [TestMethod]
        public MFTestResults RsaTest_ExportImportTest()
        {
            bool testResult = true;

            try
            {
                using (Session session = new Session("", MechanismType.RSA_PKCS))
                using (CryptoKey privateKey = CryptoKey.LoadKey(session, m_importKeyPrivate))
                {
                    string dataToSign = "This is a simple message to be encrypted";

                    byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes(dataToSign);

                    using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(privateKey))
                    {
                        RSAParameters kp1 = rsa.ExportParameters(true);

                        byte[] sig = rsa.SignData(data);

                        rsa.ImportParameters(kp1);

                        RSAParameters kp2 = rsa.ExportParameters(true);

                        testResult &= CompareByteArray(kp1.D, kp2.D);
                        testResult &= CompareByteArray(kp1.DP, kp2.DP);
                        testResult &= CompareByteArray(kp1.DQ, kp2.DQ);
                        testResult &= CompareByteArray(kp1.Exponent, kp2.Exponent);
                        testResult &= CompareByteArray(kp1.InverseQ, kp2.InverseQ);
                        testResult &= CompareByteArray(kp1.Modulus, kp2.Modulus);
                        testResult &= CompareByteArray(kp1.P, kp2.P);
                        testResult &= CompareByteArray(kp1.Q, kp2.Q);

                        testResult &= CompareByteArray(m_importKeyPrivate[2].Value, kp1.Modulus);
                        testResult &= CompareByteArray(m_importKeyPrivate[3].Value, kp1.Exponent);
                        testResult &= CompareByteArray(m_importKeyPrivate[4].Value, kp1.D);
                        testResult &= CompareByteArray(m_importKeyPrivate[5].Value, kp1.P);
                        testResult &= CompareByteArray(m_importKeyPrivate[6].Value, kp1.Q);
                        testResult &= CompareByteArray(m_importKeyPrivate[7].Value, kp1.DP);
                        testResult &= CompareByteArray(m_importKeyPrivate[8].Value, kp1.DQ);
                        testResult &= CompareByteArray(m_importKeyPrivate[9].Value, kp1.InverseQ);


                        testResult &= rsa.VerifyData(data, sig);
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
                testResult = false;
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults RsaTest_ImportKeyTest()
        {
            bool testResult = false;

            try
            {
                using (Session session = new Session("", MechanismType.RSA_PKCS))
                using (CryptoKey privateKey = CryptoKey.LoadKey(session, m_importKeyPrivate))
                {
                    string dataToSign = "This is a simple message to be encrypted";

                    byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes(dataToSign);

                    using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(privateKey))
                    {
                        byte[] signature = rsa.SignData(data, null);

                        testResult = rsa.VerifyData(data, null, signature);

                        byte[] encr = rsa.Encrypt(data);

                        byte[] res = rsa.Decrypt(encr);

                        string resStr = new string(System.Text.UTF8Encoding.UTF8.GetChars(res));

                        Debug.Print(resStr);

                        testResult &= (resStr == dataToSign);
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
        public MFTestResults RsaTest_Signature()
        {
            bool testResult = false;

            try
            {
                using (RSACryptoServiceProvider csp = new RSACryptoServiceProvider())
                {
                    string dataToSign = "This is a simple message to be encrypted";

                    byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes(dataToSign);

                    byte[] signature = csp.SignData(data, null);

                    testResult = csp.VerifyData(data, null, signature);

                    HashAlgorithm sha = new HashAlgorithm(HashAlgorithmType.SHA1, csp.Session);

                    byte[] hash = sha.ComputeHash(data);

                    signature = csp.SignHash(hash, MechanismType.SHA_1);
                    testResult &= csp.VerifyHash(hash, MechanismType.SHA_1, signature);
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults RsaTest_SignComparison()
        {
            bool testResult = false;

            if (!m_isEmulator) return MFTestResults.Skip;

            try
            {
                string dataToEncrypt = "This is a simple message to be encrypted";

                byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes(dataToEncrypt);

                using (Session sessionOpen = new Session("", MechanismType.RSA_PKCS))
                using (CryptoKey keysOpen = CryptoKey.LoadKey(sessionOpen, m_importKeyPrivate))
                using (RSACryptoServiceProvider rsaOpen = new RSACryptoServiceProvider(keysOpen))
                {
                    byte[] sig = rsaOpen.SignData(data);

                    using (Session sessionEmu = new Session("Emulator_Crypto", MechanismType.RSA_PKCS))
                    using (CryptoKey keysEmu = CryptoKey.LoadKey(sessionEmu, m_importKeyPrivate))
                    using (RSACryptoServiceProvider rsaEmu = new RSACryptoServiceProvider(keysEmu))
                    {
                        byte[] sig2 = rsaEmu.SignData(data);
                        testResult = rsaEmu.VerifyData(data, sig2);
                        testResult = rsaEmu.VerifyData(data, sig);
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
        public MFTestResults RsaTest_Encryption()
        {
            bool testResult = false;

            try
            {
                using (RSACryptoServiceProvider csp = new RSACryptoServiceProvider(""))
                {
                    string dataToEncrypt = "This is a simple message to be encrypted";

                    byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes(dataToEncrypt);

                    byte[] encr = csp.Encrypt(data);

                    byte[] res = csp.Decrypt(encr);

                    string resStr = new string(System.Text.UTF8Encoding.UTF8.GetChars(res));

                    Debug.Print(resStr);

                    testResult = (resStr == dataToEncrypt);
                }
            }
            catch(Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults RsaTest_EncryptionComparison()
        {
            bool testResult = false;

            if (!m_isEmulator) return MFTestResults.Skip;

            try
            {
                string dataToEncrypt = "This is a simple message to be encrypted";

                byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes(dataToEncrypt);

                using (Session sessionOpen = new Session("", MechanismType.RSA_PKCS))
                using (CryptoKey keysOpen = CryptoKey.LoadKey(sessionOpen, m_importKeyPrivate))
                using (RSACryptoServiceProvider rsaOpen = new RSACryptoServiceProvider(keysOpen))
                {
                    byte[] encr = rsaOpen.Encrypt(data);

                    using (Session sessionEmu = new Session("Emulator_Crypto", MechanismType.RSA_PKCS))
                    using (CryptoKey keysEmu = CryptoKey.LoadKey(sessionEmu, m_importKeyPrivate))
                    using (RSACryptoServiceProvider rsaEmu = new RSACryptoServiceProvider(keysEmu))
                    {
                        byte[] encr2 = rsaEmu.Encrypt(data);

                        byte[] res = rsaEmu.Decrypt(encr);

                        string resStr = new string(System.Text.UTF8Encoding.UTF8.GetChars(res));

                        Debug.Print(resStr);

                        testResult = (resStr == dataToEncrypt);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        internal static CryptokiAttribute[] m_importKeyPublic = new CryptokiAttribute[]
                    {
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.Class  , Utility.ConvertToBytes((int)CryptokiClass.PUBLIC_KEY)),
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.KeyType, Utility.ConvertToBytes((int)CryptoKey.KeyType.RSA)),
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.Modulus, new byte[]
                            {
                                0xC6, 0x29, 0x73, 0xE3, 0xC8, 0xD4, 0xFC, 0xB6, 0x89, 0x36, 0x46, 0xF9, 0x58, 0xE5, 0xF5, 0xE5, 
                                0x25, 0xC2, 0xE4, 0x1E, 0xCC, 0xA8, 0xC3, 0xEF, 0xA2, 0x8D, 0x24, 0xDE, 0xFD, 0x19, 0xDA, 0x08, 
                                0x46, 0x9A, 0xA9, 0xBA, 0xAE, 0x77, 0x20, 0x28, 0xED, 0x51, 0x43, 0x8C, 0x28, 0x6F, 0x99, 0x5B, 
                                0x6B, 0x0C, 0x08, 0x7C, 0x4C, 0x7D, 0x6F, 0xCF, 0xD0, 0xF0, 0xAC, 0x2A, 0x9B, 0x28, 0x28, 0x62, 
                                0x52, 0x3F, 0x56, 0x3B, 0x6F, 0x49, 0x10, 0x11, 0x48, 0x45, 0x36, 0x51, 0x62, 0xAE, 0x8C, 0x66, 
                                0xE8, 0x53, 0x8D, 0x18, 0xDF, 0x21, 0x12, 0x30, 0x35, 0x79, 0xAD, 0x41, 0x0F, 0xED, 0x50, 0x41, 
                                0x26, 0xC3, 0x3E, 0xFE, 0x88, 0xEB, 0xA8, 0x7C, 0xF2, 0x48, 0x13, 0x84, 0x27, 0xCE, 0x19, 0x86, 
                                0x33, 0x14, 0x89, 0xEB, 0x7A, 0x90, 0x21, 0x46, 0x5C, 0xC2, 0x22, 0x23, 0x96, 0x06, 0x85, 0xF7,  
                            }),
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.PublicExponent, new byte[]
                            {
                                 0x01, 0x00, 0x01
                            }),
                    };

        internal static CryptokiAttribute[] m_importKeyPrivate = new CryptokiAttribute[]
                    {
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.Class  , Utility.ConvertToBytes((int)CryptokiClass.PRIVATE_KEY)),
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.KeyType, Utility.ConvertToBytes((int)CryptoKey.KeyType.RSA)),
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.Modulus, new byte[]
                            {
                                0xC6, 0x29, 0x73, 0xE3, 0xC8, 0xD4, 0xFC, 0xB6, 0x89, 0x36, 0x46, 0xF9, 0x58, 0xE5, 0xF5, 0xE5, 
                                0x25, 0xC2, 0xE4, 0x1E, 0xCC, 0xA8, 0xC3, 0xEF, 0xA2, 0x8D, 0x24, 0xDE, 0xFD, 0x19, 0xDA, 0x08, 
                                0x46, 0x9A, 0xA9, 0xBA, 0xAE, 0x77, 0x20, 0x28, 0xED, 0x51, 0x43, 0x8C, 0x28, 0x6F, 0x99, 0x5B, 
                                0x6B, 0x0C, 0x08, 0x7C, 0x4C, 0x7D, 0x6F, 0xCF, 0xD0, 0xF0, 0xAC, 0x2A, 0x9B, 0x28, 0x28, 0x62, 
                                0x52, 0x3F, 0x56, 0x3B, 0x6F, 0x49, 0x10, 0x11, 0x48, 0x45, 0x36, 0x51, 0x62, 0xAE, 0x8C, 0x66, 
                                0xE8, 0x53, 0x8D, 0x18, 0xDF, 0x21, 0x12, 0x30, 0x35, 0x79, 0xAD, 0x41, 0x0F, 0xED, 0x50, 0x41, 
                                0x26, 0xC3, 0x3E, 0xFE, 0x88, 0xEB, 0xA8, 0x7C, 0xF2, 0x48, 0x13, 0x84, 0x27, 0xCE, 0x19, 0x86, 
                                0x33, 0x14, 0x89, 0xEB, 0x7A, 0x90, 0x21, 0x46, 0x5C, 0xC2, 0x22, 0x23, 0x96, 0x06, 0x85, 0xF7,  
                            }),
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.PublicExponent, new byte[]
                            {
                                0x01, 0x00, 0x01
                            }),
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.PrivateExponent, new byte[]
                            {
                                0x6A, 0xBE, 0x93, 0xAD, 0xE5, 0x56, 0x4E, 0x17, 0x6A, 0x0C, 0x71, 0xE9, 0x09, 0xA9, 0x3E, 0x6F, 
                                0x44, 0x8B, 0x1A, 0x65, 0x38, 0xEB, 0xC4, 0x38, 0x47, 0x00, 0xEF, 0x16, 0xAB, 0x92, 0x8C, 0x6F, 
                                0x9E, 0xD0, 0xDB, 0x93, 0x33, 0x3D, 0xFA, 0x75, 0xF1, 0x78, 0xB0, 0x01, 0x45, 0x1A, 0xF0, 0xAA, 
                                0x5D, 0x1C, 0xAB, 0x49, 0x81, 0xCE, 0xA4, 0x37, 0x77, 0x1E, 0xDE, 0x2F, 0x49, 0x4B, 0x35, 0x8C, 
                                0xE5, 0xCB, 0x24, 0xA0, 0x31, 0x51, 0xDB, 0x2B, 0xF9, 0x16, 0x3A, 0xCA, 0xAB, 0x7B, 0x2A, 0x61, 
                                0xB6, 0xE7, 0xE1, 0x4E, 0x9B, 0xB3, 0xC2, 0x99, 0x23, 0x6B, 0xB8, 0x43, 0x97, 0x7B, 0xFD, 0x33, 
                                0x95, 0x73, 0xA5, 0xC2, 0xCF, 0xCC, 0xC4, 0x27, 0x30, 0xCD, 0xBC, 0x51, 0x2D, 0xDD, 0x22, 0x89, 
                                0xF2, 0xA3, 0x93, 0x46, 0x65, 0x84, 0x03, 0x8A, 0x8F, 0x6C, 0x30, 0xB4, 0xF5, 0x67, 0xC4, 0x81, 
                            }),
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.Prime1, new byte[]
                            {
                                0xF3, 0x73, 0x8B, 0x7C, 0xA0, 0x95, 0x28, 0x10, 0xD3, 0x75, 0x4A, 0x69, 0xC9, 0x7A, 0xCB, 0xE5, 
                                0x28, 0xBD, 0x9A, 0x36, 0x53, 0xCA, 0x00, 0x1A, 0x21, 0x56, 0x00, 0x7A, 0xE8, 0xDF, 0xC2, 0x3D, 
                                0x12, 0x5C, 0xE1, 0x7E, 0x0F, 0xC8, 0xCC, 0x9C, 0x50, 0x01, 0xF1, 0xAE, 0xC3, 0x3B, 0x7B, 0x01, 
                                0xA4, 0xAC, 0xAF, 0x77, 0xED, 0xAD, 0x20, 0x56, 0x77, 0x89, 0x99, 0x39, 0xFC, 0xA9, 0xE7, 0x41, 
                            }),
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.Prime2, new byte[]
                            {
                                0xD0, 0x60, 0x4A, 0xE6, 0x18, 0xB4, 0x3C, 0x7B, 0xAC, 0x0B, 0xA3, 0x78, 0x58, 0x05, 0x00, 0xA3, 
                                0xD9, 0xA2, 0xD7, 0x24, 0xF5, 0xF6, 0xB0, 0x4C, 0xE6, 0x62, 0x24, 0x44, 0xF6, 0x25, 0x02, 0x26, 
                                0xE5, 0x8D, 0xAE, 0x6E, 0xBF, 0x16, 0x57, 0xD7, 0xA2, 0x94, 0x3C, 0xBE, 0x99, 0x9D, 0x80, 0x34, 
                                0xB5, 0x68, 0x62, 0x20, 0x96, 0x5C, 0x89, 0xDC, 0xF8, 0x1E, 0xBD, 0x81, 0xB0, 0xFF, 0x17, 0x37, 
                            }),
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.Exponent1, new byte[]
                            {
                                0x42, 0x95, 0x23, 0x5D, 0x1E, 0x7E, 0x2C, 0xCB, 0x0D, 0x4A, 0x52, 0xE3, 0xC3, 0xDA, 0xF5, 0xD0, 
                                0xE2, 0xE7, 0x98, 0x39, 0xAB, 0x88, 0xDF, 0xA6, 0x45, 0xDF, 0xC3, 0x99, 0xD9, 0xFE, 0xF8, 0x9C, 
                                0xC3, 0x5C, 0xEB, 0xBF, 0x12, 0x8A, 0x14, 0x8B, 0xDB, 0xC5, 0xEC, 0x57, 0xA3, 0xC5, 0xAC, 0xCA, 
                                0xB2, 0x43, 0x18, 0x6A, 0x70, 0x72, 0x9D, 0x19, 0x88, 0xEF, 0xF5, 0x1C, 0x4A, 0xE2, 0x1D, 0x01, 
                            }),
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.Exponent2, new byte[]
                            {
                                0xC5, 0x7A, 0x0C, 0x61, 0x66, 0x16, 0x21, 0x9F, 0xDE, 0xDB, 0xA4, 0xCF, 0x5F, 0x33, 0x56, 0x78, 
                                0xF1, 0xBF, 0x76, 0x7F, 0x6B, 0xAE, 0x9F, 0x44, 0x31, 0xAD, 0xDE, 0xCB, 0x90, 0x2E, 0x60, 0x8C, 
                                0xB6, 0x4E, 0x00, 0x7A, 0xAA, 0x13, 0xA5, 0xAA, 0x11, 0x44, 0xC5, 0x10, 0xA9, 0x0A, 0x6F, 0xBF, 
                                0x04, 0x10, 0xE9, 0xB6, 0x12, 0x69, 0x9E, 0xA9, 0xD0, 0x67, 0x69, 0x97, 0x68, 0x43, 0x48, 0x1F, 
                            }),
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.Coefficent, new byte[]
                            {
                                0xCF, 0x52, 0x93, 0x95, 0xDC, 0xBA, 0x71, 0x56, 0x13, 0x1C, 0x7A, 0x08, 0x97, 0x11, 0x8E, 0x55, 
                                0xBC, 0x30, 0xC4, 0xE1, 0xE3, 0x17, 0xC4, 0x94, 0xB2, 0x6B, 0x7A, 0x2D, 0xBD, 0x1F, 0x2C, 0x6D, 
                                0x78, 0x34, 0x8F, 0x65, 0x22, 0x97, 0xCD, 0xC4, 0x02, 0x66, 0x10, 0x61, 0x5A, 0x3E, 0x02, 0xCB, 
                                0xB6, 0x3A, 0x7A, 0x60, 0xEF, 0xAA, 0xB2, 0xB9, 0x05, 0x9E, 0x76, 0xC9, 0xED, 0x59, 0x77, 0x16, 
                            }),
                    };
    }
}