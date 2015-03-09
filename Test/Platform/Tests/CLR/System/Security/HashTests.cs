using System;
using System.Security.Cryptography;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{

    public class HashTests : IMFTestInterface
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
        public MFTestResults Hashes_Test()
        {

            bool passed = true;

            if (!m_isEmulator) return MFTestResults.Skip;

            try
            {
                using(HashAlgorithm test     = new HashAlgorithm(HashAlgorithmType.MD5))
                using(HashAlgorithm baseline = new HashAlgorithm(HashAlgorithmType.MD5, "Emulator_Crypto"))
                {
                    passed &= Test(test, baseline);
                }

                using(HashAlgorithm test     = new HashAlgorithm(HashAlgorithmType.SHA1))
                using(HashAlgorithm baseline = new HashAlgorithm(HashAlgorithmType.SHA1, "Emulator_Crypto"))
                {
                    passed &= Test(test, baseline);
                }
                
                using(HashAlgorithm test     = new HashAlgorithm(HashAlgorithmType.SHA256))
                using(HashAlgorithm baseline = new HashAlgorithm(HashAlgorithmType.SHA256, "Emulator_Crypto"))
                {
                    passed &= Test(test, baseline);
                }
                
                    
                using(HashAlgorithm test     = new HashAlgorithm(HashAlgorithmType.SHA384))
                using(HashAlgorithm baseline =  new HashAlgorithm(HashAlgorithmType.SHA384, "Emulator_Crypto"))
                {
                    passed &= Test(test, baseline);
                }
                using(HashAlgorithm test     = new HashAlgorithm(HashAlgorithmType.SHA512))
                using (HashAlgorithm baseline = new HashAlgorithm(HashAlgorithmType.SHA512, "Emulator_Crypto"))
                {
                    passed &= Test(test, baseline);
                }

                using (HashAlgorithm test = new HashAlgorithm(HashAlgorithmType.RIPEMD160))
                using (HashAlgorithm baseline = new HashAlgorithm(HashAlgorithmType.RIPEMD160, "Emulator_Crypto"))
                {
                    passed &= Test(test, baseline);
                }
            }
            catch
            {
                passed = false;
            }

            return passed ? MFTestResults.Pass : MFTestResults.Fail;
        }

        private static bool Test(HashAlgorithm testAlgorithm, HashAlgorithm baseline)
        {
            for (int i = 0; i < (testAlgorithm.HashSize * 2) / 8; i++)
            {
                if (!DeterministicTest(testAlgorithm, baseline, i))
                    return false;
            }

            return true;
        }

        private static bool TestHash(HashAlgorithm testAlgorithm, HashAlgorithm baseline, byte[] data)
        {
            try
            {
                byte[] testValue = testAlgorithm.ComputeHash(data);
                byte[] baseValue = baseline.ComputeHash(data);
                return CompareBytes(testValue, baseValue);
            }
            catch (Exception e)
            {
                Log.Exception("Got an exception, fail", e);
                return false;
            }
        }

        private static bool DeterministicTest(HashAlgorithm testAlgorithm, HashAlgorithm baseline, int dataSize)
        {
            byte[] data = new byte[dataSize];
            for (int i = 0; i < dataSize; i++)
                data[i] = (byte)i;

            return TestHash(testAlgorithm, baseline, data);
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