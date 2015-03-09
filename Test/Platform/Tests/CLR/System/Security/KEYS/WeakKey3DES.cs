using System;
using System.Security.Cryptography; 
using System.IO; 
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Cryptoki;

namespace Microsoft.SPOT.Platform.Tests
{
    class WeakKey3DES : IMFTestInterface
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

        static Boolean Test(Session session)
        {
            try
            {
                Byte[] PlainText = { 0, 1, 2, 3, 4, 5, 6, 7 }; //, 8, 9, 10, 11, 12, 13, 14, 15};
                Byte[] Key = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 3, 4, 5, 6, 7, 8, 1, 2, 3, 4, 5, 6, 7, 8 };
                Byte[] IV = { 0, 0, 0, 0, 0, 0, 0, 0 };
                TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider(session);
                des.Key = CryptoKey.ImportKey(session, Key, CryptoKey.KeyClass.Secret, CryptoKey.KeyType.DES3, true);

                return false;
            }
            catch
            {
                return true;
            }
        }

        [TestMethod]
        public MFTestResults WeakKey3DES_Test()
        {
            bool bRes = true;

            try
            {
                using (Session sess = new Session("", MechanismType.DSA))
                {
                    bRes &= Test(sess);
                }

                if (m_isEmulator)
                {
                    using (Session sess = new Session("Emulator_Crypto", MechanismType.DSA))
                    {
                        bRes &= Test(sess);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Exception("", e);
                bRes = false;
            }
            return bRes ? MFTestResults.Pass : MFTestResults.Fail;
        }
    }
}