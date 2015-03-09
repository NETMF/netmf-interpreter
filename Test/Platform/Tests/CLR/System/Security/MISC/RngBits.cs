using System;
using System.Security.Cryptography;
using Microsoft.SPOT.Cryptoki;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class RngBits : IMFTestInterface
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

        public const int BytesToGenerate = 2000;

        public static bool Test(Session session)
        {
            byte[] output = new byte[BytesToGenerate];

            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider(session);
            rng.GetBytes(output);

            int oneCount = 0;
            int zeroCount = 0;

            for (int i = 0; i < output.Length; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (((output[i] >> j) & 1) == 1)
                        oneCount++;
                    else
                        zeroCount++;
                }
            }

            int totalCount = zeroCount + oneCount;
            float bitDifference = (float)(zeroCount - oneCount) / totalCount;
            if (bitDifference < 0) bitDifference = -bitDifference;

            Log.Comment("Count of 0s: " +  zeroCount + " " + (float)zeroCount / totalCount);
            Log.Comment("Count of 1s: " +  oneCount + " " + (float)oneCount / totalCount);

            // Set the tolerance for differences to 3%.  The probability of the difference being >3%
            // given 16000 bits is .0075% (approx. 1 in 13,333)
            //
            if (bitDifference > .03)
            {
                Log.Comment("Test failed - Bit difference > 3%");
                return false;
            }

            Log.Comment("Test passed - Bit difference " + bitDifference + " < 3%");
            return true;
        }

        [TestMethod]
        public MFTestResults RNGBits_Test()
        {
            bool bRes = true;

            try
            {
                using (Session sess = new Session("", MechanismType.RSA_PKCS))
                {
                    bRes &= Test(sess);
                }

                if (m_isEmulator)
                {
                    using (Session sess = new Session("Emulator_Crypto", MechanismType.RSA_PKCS))
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