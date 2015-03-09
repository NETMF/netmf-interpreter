using System;
using System.Reflection;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.Security.Cryptography;
using Microsoft.SPOT.Cryptoki;

namespace Microsoft.SPOT.Platform.Tests
{

    public class SessionTests : IMFTestInterface
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

        bool SessionTest_CloseWithCreateKeyObjects_internal(string svcProvider)
        {
            bool res = true;

            CryptoKey key;

            using (Session sess = new Session(svcProvider, MechanismType.AES_CBC))
            {
                AesCryptoServiceProvider aes = new AesCryptoServiceProvider(sess);

                aes.GenerateKey();

                key = aes.Key;

                SymmetricTestHelper.Test_EncryptUpdate(aes);
            }

            try
            {
                using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider(svcProvider))
                {
                    aes.Key = key;

                    SymmetricTestHelper.Test_EncryptUpdate(aes);
                }

                res = false;
            }
            catch (Exception)
            {
            }

            return res;
        }

        [TestMethod]
        public MFTestResults SessionTest_CloseWithCreateKeyObjects()
        {
            bool res = SessionTest_CloseWithCreateKeyObjects_internal("");

            if (m_isEmulator)
            {
                res &= SessionTest_CloseWithCreateKeyObjects_internal("Emulator_Crypto");
            }

            return res ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults SessionTest_CloseWithOpenKeyObjects()
        {
            bool res = SessionTest_CloseWithOpenKeyObjects_internal("");

            if (m_isEmulator)
            {
                res &= SessionTest_CloseWithOpenKeyObjects_internal("Emulator_Crypto");
            }
            return res ? MFTestResults.Pass : MFTestResults.Fail;
        }

        bool SessionTest_CloseWithOpenKeyObjects_internal(string svcProvider)
        {
            bool res = true;

            CryptoKey key;

            using (Session sess = new Session(svcProvider, MechanismType.AES_CBC))
            {
                using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider(sess))
                {
                    key = CryptoKey.LoadKey(sess, m_key);

                    aes.Key = key;

                    SymmetricTestHelper.Test_EncryptUpdate(aes);
                }

                using (AesCryptoServiceProvider aes2 = new AesCryptoServiceProvider(sess))
                {
                    key = CryptoKey.LoadKey(sess, m_key);

                    aes2.Key = key;

                    SymmetricTestHelper.Test_EncryptUpdate(aes2);
                }

            }

            try
            {
                // new session should fail, key should be disposed
                using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider(svcProvider))
                {
                    aes.Key = key;

                    SymmetricTestHelper.Test_EncryptUpdate(aes);
                }

                res = false;
            }
            catch (Exception)
            {
            }

            return res;
        }

        CryptokiAttribute[] m_key = new CryptokiAttribute[]
                    {
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.Class  , new byte[] {    4, 0, 0, 0}),
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.KeyType, new byte[] { 0x1F, 0, 0, 0}),
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.Value  , new byte[0x100/8])
                    };
    }
}