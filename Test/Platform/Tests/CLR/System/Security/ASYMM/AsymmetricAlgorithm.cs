using System;
using System.Security.Cryptography;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{

    public class AsymmetricAlgorithmTest : IMFTestInterface
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
        public MFTestResults ASYMM_Test()
        {
            bool bRet = CheckKeySize();
            bRet &= SetInvalidRSAKey();
            bRet &= SetValidRSAKey();

            return bRet ? MFTestResults.Pass : MFTestResults.Fail;
        }


        #region AsymmetricAlgorithm API tests

        /// <summary>
        ///		Check key size of RSA
        /// </summary>
        private static bool CheckKeySize()
        {
            using (AsymmetricAlgorithm alg = new RSACryptoServiceProvider(384))
            {
                return alg.KeySize == 384;
            }
        }

        /// <summary>
        ///		Set an invalid RSA key size
        /// </summary>
        private static bool SetInvalidRSAKey()
        {
            try
            {
                using (AsymmetricAlgorithm alg = new RSACryptoServiceProvider())
                {
                    alg.KeySize = 256;
                }
            }
            catch (CryptographicException)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        ///		Set an valid RSA key size
        /// </summary>
        private static bool SetValidRSAKey()
        {
            using (AsymmetricAlgorithm alg = new RSACryptoServiceProvider())
            {
                alg.KeySize = 1024;
                return alg.KeySize == 1024;
            }
        }

        #endregion
    }
}