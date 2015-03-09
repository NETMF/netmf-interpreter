using System;
using System.Reflection;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.Security.Cryptography;
using Microsoft.SPOT.Cryptoki;

namespace Microsoft.SPOT.Platform.Tests
{

    public class TDesTests : IMFTestInterface
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
        public MFTestResults TDesTest_EncryptUpdate()
        {
            MFTestResults res;

            try
            {
                using (TripleDESCryptoServiceProvider csp = new TripleDESCryptoServiceProvider())
                {
                    res = SymmetricTestHelper.Test_EncryptUpdate(csp);
                }

                if (res == MFTestResults.Pass && m_isEmulator)
                {
                    using (TripleDESCryptoServiceProvider csp = new TripleDESCryptoServiceProvider("Emulator_Crypto"))
                    {
                        res = SymmetricTestHelper.Test_EncryptUpdate(csp);
                    }
                }
            }
            catch
            {
                return MFTestResults.Fail;
            }

            return res;
        }

        /// <summary>
        /// Make sure DirectoryInfo could be constructed even when directory itself does not exist.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults TDesTest_Encrypt()
        {
            MFTestResults res;

            using (TripleDESCryptoServiceProvider csp = new TripleDESCryptoServiceProvider())
            {
                res = SymmetricTestHelper.Test_Encrypt(csp);
            }

            if (res == MFTestResults.Pass && m_isEmulator)
            {
                using (TripleDESCryptoServiceProvider csp = new TripleDESCryptoServiceProvider("Emulator_Crypto"))
                {
                    res = SymmetricTestHelper.Test_Encrypt(csp);
                }
            }

            return res;
        }
    }
}