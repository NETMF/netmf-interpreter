using System;
using System.Reflection;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.Security.Cryptography;
using Microsoft.SPOT.Cryptoki;

namespace Microsoft.SPOT.Platform.Tests
{
    public class SHATests : IMFTestInterface
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

        private bool TestSha(HashAlgorithm alg1, HashAlgorithm alg2)
        {
            string tstStr = "This is a string that I will be getting the hash of!";

            byte[] testHash = System.Text.UTF8Encoding.UTF8.GetBytes(tstStr);

            byte[] hash1 = alg1.ComputeHash(testHash, 0, testHash.Length);
            byte[] hash2 = alg1.ComputeHash(testHash, 0, testHash.Length);
            byte[] hash3 = alg1.ComputeHash(testHash, 0, testHash.Length - 1);

            byte[] hash4 = alg2.ComputeHash(testHash);
            byte[] hash5 = alg2.ComputeHash(testHash, 0, testHash.Length - 1);

            if (hash1.Length != (alg1.HashSize/8)) throw new Exception();

            bool res1 = true, res2 = true, res3 = true, res4 = true;
            for (int i = 0; i < hash1.Length; i++)
            {
                res1 &= (hash1[i] == hash2[i]);
                res2 &= (hash1[i] == hash3[i]);
                res3 &= (hash1[i] == hash4[i]);
                res4 &= (hash3[i] == hash5[i]);
            }

            return res1 && !res2 && res3 && res4;
        }

        [TestMethod]
        public MFTestResults Sha1Test_Hash()
        {
            bool bRet = false;

            if (!m_isEmulator) return MFTestResults.Skip;

            try
            {
                using (HashAlgorithm csp1 = new HashAlgorithm(HashAlgorithmType.SHA1))
                {
                    using (HashAlgorithm csp2 = new HashAlgorithm(HashAlgorithmType.SHA1, "Emulator_Crypto"))
                    {
                        bRet = TestSha(csp1, csp2);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception: ", ex);
            }
            return bRet ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Sha256Test_Hash()
        {
            bool bRet = false;

            if (!m_isEmulator) return MFTestResults.Skip;

            try
            {
                using (HashAlgorithm csp1 = new HashAlgorithm(HashAlgorithmType.SHA256))
                {
                    using (HashAlgorithm csp2 = new HashAlgorithm(HashAlgorithmType.SHA256, "Emulator_Crypto"))
                    {

                        bRet = TestSha(csp1, csp2);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception: ", ex);
            }
            return bRet ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Sha384Test_Hash()
        {
            bool bRet = false;

            if (!m_isEmulator) return MFTestResults.Skip;

            try
            {
                using (HashAlgorithm csp1 = new HashAlgorithm(HashAlgorithmType.SHA384))
                {
                    using (HashAlgorithm csp2 = new HashAlgorithm(HashAlgorithmType.SHA384, "Emulator_Crypto"))
                    {

                        bRet = TestSha(csp1, csp2);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception: ", ex);
            }
            return bRet ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Sha512Test_Hash()
        {
            bool bRet = false;

            if (!m_isEmulator) return MFTestResults.Skip;

            try
            {
                using (HashAlgorithm csp1 = new HashAlgorithm(HashAlgorithmType.SHA512))
                {
                    using (HashAlgorithm csp2 = new HashAlgorithm(HashAlgorithmType.SHA512, "Emulator_Crypto"))
                    {
                        bRet = TestSha(csp1, csp2);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception: ", ex);
            }
            return bRet ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults MD5Test_Hash()
        {
            bool bRet = false;

            if (!m_isEmulator) return MFTestResults.Skip;

            try
            {
                using (HashAlgorithm csp1 = new HashAlgorithm(HashAlgorithmType.MD5))
                {
                    using (HashAlgorithm csp2 = new HashAlgorithm(HashAlgorithmType.MD5, "Emulator_Crypto"))
                    {
                        bRet = TestSha(csp1, csp2);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception: ", ex);
            }
            return bRet ? MFTestResults.Pass : MFTestResults.Fail;
        }
    }
}