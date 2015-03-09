using System;
using System.Reflection;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.Security.Cryptography;
using Microsoft.SPOT.Cryptoki;

namespace Microsoft.SPOT.Platform.Tests
{
    public class RNGTests : IMFTestInterface
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

        bool testRng(Session session)
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider(session);

            byte[] data1 = new byte[1024];
            byte[] data2 = new byte[1024];
            byte[] data3 = new byte[1024];

            rng.GetBytes(data1);
            rng.GetBytes(data2);
            rng.Dispose();

            rng = new RNGCryptoServiceProvider(session);

            rng.GetBytes(data3);
            rng.Dispose();

            int same = 0;
            for (int i = 0; i < data1.Length; i++)
            {
                if (data1[i] == data2[i] || data1[i] == data3[i] || data2[i] == data3[i]) same++;
            }

            return same < 32; // ~3% matching elements
        }

        [TestMethod]
        public MFTestResults RNGTest_rng()
        {
            bool bRet = false;

            try
            {
                using (Session session = new Session(""))
                {
                    bRet = testRng(session);
                }
                if (m_isEmulator)
                {
                    using (Session session = new Session("Emulator_Crypto"))
                    {
                        bRet &= testRng(session);
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
