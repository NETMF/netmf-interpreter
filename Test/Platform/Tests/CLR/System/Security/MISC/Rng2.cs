using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.SPOT.Cryptoki;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{

    public class rng2 : IMFTestInterface
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

        static Random Rnd = new Random();

        static Boolean Compare(Byte[] rgb1, Byte[] rgb2)
        {
            int i;
            if (rgb1.Length != rgb2.Length) return false;
            for (i = 0; i < rgb1.Length; i++)
            {
                if (rgb1[i] != rgb2[i]) return false;
            }
            return true;
        }

        public static Boolean Test(Session session)
        {
            Byte[] barr1, barr2;
            RNGCryptoServiceProvider rndcsp1 = new RNGCryptoServiceProvider(session);
            RNGCryptoServiceProvider rndcsp2 = new RNGCryptoServiceProvider(session);
            int l;

            for (int i = 0; i < 1000; i++)
            {
                l = Rnd.Next() % 1500 + 4;
                barr1 = new Byte[l];
                barr2 = new Byte[l];
                rndcsp1.GetBytes(barr1);
                rndcsp2.GetBytes(barr2);
                if (Compare(barr1, barr2)) return false;
            }

            return true;
        }

        [TestMethod]
        public MFTestResults RNG2_Test()
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

