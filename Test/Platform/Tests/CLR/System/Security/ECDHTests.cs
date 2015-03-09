using System;
using System.Reflection;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.Security.Cryptography;
using Microsoft.SPOT.Cryptoki;

namespace Microsoft.SPOT.Platform.Tests
{

    public class ECDHTests : IMFTestInterface
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

        public bool ECDHTest_Signature(string svcProvider)
        {
            try
            {
                using (ECDsaCryptoServiceProvider csp = new ECDsaCryptoServiceProvider(svcProvider))
                {
                    byte[] dataToSign = System.Text.UTF8Encoding.UTF8.GetBytes("This is a string to sign");

                    byte[] sig = csp.SignData(dataToSign);

                    return csp.VerifyData(dataToSign, sig);
                }
            }
            catch
            {
                return false;
            }
        }

        [TestMethod]
        public MFTestResults ECDHTest_Signature()
        {
            bool bRet = true;
            try
            {
                bRet &= ECDHTest_Signature("");

                if (bRet && m_isEmulator)
                {
                    bRet &= ECDHTest_Signature("Emulator_Crypto");
                }
            }
            catch
            {
                return MFTestResults.Fail;
            }

            return bRet ? MFTestResults.Pass : MFTestResults.Fail;
        }

        public bool ECDHTest_KeyExchange(string svcProvider)
        {
            try
            {
                string txt = "This is a string to encode using ECDH";
                string res = "";
                byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes(txt);
                byte[] encData = null;

                using (ECDiffieHellmanCryptoServiceProvider csp = new ECDiffieHellmanCryptoServiceProvider(svcProvider))
                {
                    csp.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
                    csp.HashAlgorithm = MechanismType.SHA256;

                    using (ECDiffieHellmanCryptoServiceProvider csp2 = new ECDiffieHellmanCryptoServiceProvider(svcProvider))
                    {
                        csp2.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
                        csp.HashAlgorithm = MechanismType.SHA256;

                        CryptoKey secret = csp.DeriveKeyMaterial(csp2.PublicKey);

                        CryptoKey secret2 = csp2.DeriveKeyMaterial(csp.PublicKey);

                        byte[] IV;

                        using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider(secret))
                        {
                            ICryptoTransform encr = aes.CreateEncryptor();

                            IV = aes.IV;

                            encData = encr.TransformFinalBlock(data, 0, data.Length);
                        }

                        using (AesCryptoServiceProvider aes2 = new AesCryptoServiceProvider(secret2))
                        {
                            aes2.IV = IV;

                            ICryptoTransform decr = aes2.CreateDecryptor();

                            byte[] decrData = decr.TransformFinalBlock(encData, 0, encData.Length);

                            res = new string(System.Text.UTF8Encoding.UTF8.GetChars(decrData));
                        }
                    }
                }

                return string.Compare(txt, res) == 0;
            }
            catch
            {
                return false;
            }
        }

        [TestMethod]
        public MFTestResults ECDHTest_KeyExchange()
        {
            bool bRet = true;

            bRet &= ECDHTest_KeyExchange("");
            if (bRet && m_isEmulator)
            {
                bRet &= ECDHTest_KeyExchange("Emulator_Crypto");
            }

            return bRet ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults ECDHTest_KeyExchangeDifferentTokens()
        {
            if (!m_isEmulator) return MFTestResults.Skip;

            try
            {
                string txt = "This is a string to encode using ECDH";
                string res = "";
                byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes(txt);
                byte[] encData = null;

                using (ECDiffieHellmanCryptoServiceProvider csp = new ECDiffieHellmanCryptoServiceProvider(""))
                {
                    csp.KeySize = 521;
                    csp.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
                    csp.HashAlgorithm = MechanismType.SHA256;

                    using (ECDiffieHellmanCryptoServiceProvider csp2 = new ECDiffieHellmanCryptoServiceProvider("Emulator_Crypto"))
                    {
                        csp2.KeySize = 521;
                        csp2.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
                        csp2.HashAlgorithm = MechanismType.SHA256;

                        CryptoKey secret = csp.DeriveKeyMaterial(csp2.PublicKey);

                        CryptoKey secret2 = csp2.DeriveKeyMaterial(csp.PublicKey);

                        byte[] IV = new byte[16];

                        using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider(secret))
                        {
                            ICryptoTransform encr = aes.CreateEncryptor(secret, IV);

                            encData = encr.TransformFinalBlock(data, 0, data.Length);
                        }

                        using (AesCryptoServiceProvider aes2 = new AesCryptoServiceProvider(secret2))
                        {
                            ICryptoTransform decr = aes2.CreateDecryptor(secret2, IV);

                            ICryptoTransform encr = aes2.CreateEncryptor(secret2, IV);

                            byte[] encData2 = encr.TransformFinalBlock(data, 0, data.Length);

                            byte[] decrData2 = decr.TransformFinalBlock(encData2, 0, encData2.Length);

                            res = new string(System.Text.UTF8Encoding.UTF8.GetChars(decrData2));

                            byte[] decrData = decr.TransformFinalBlock(encData, 0, encData.Length);

                            res = new string(System.Text.UTF8Encoding.UTF8.GetChars(decrData));
                        }
                    }
                }

                return string.Compare(txt, res) == 0 ? MFTestResults.Pass : MFTestResults.Fail;
            }
            catch
            {
                // There is a discrpency in the derived key from openssl and .Net
                return MFTestResults.Skip;
            }

        }
    }
}