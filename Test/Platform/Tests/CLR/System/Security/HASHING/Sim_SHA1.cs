using System;
using System.Security.Cryptography; 
using System.IO; 
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Cryptoki;

namespace Microsoft.SPOT.Platform.Tests
{
    class Sim_SHA1 : IMFTestInterface
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

        static void PrintByteArray(Byte[] arr)
        {
            int i;
            string str = "";
            Log.Comment("Length: " + arr.Length);
            for (i = 0; i < arr.Length; i++)
            {
                str += arr[i].ToString() + "    ";
                if ((i + 9) % 8 == 0)
                {
                    Log.Comment(str);
                    str = "";
                }
            }
            if (i % 8 != 0) Log.Comment(str);
        }


        static Boolean Test(Session session)
        {
            Byte[] Data = { 7, 6, 5, 4, 3, 2, 1, 0 };
            Byte[] Data1 = { 7, 6, 5, 4, 3, 2, 1, 1 };

            HashAlgorithm sha1 = new HashAlgorithm(HashAlgorithmType.SHA1, session);

            sha1.ComputeHash(Data);
            PrintByteArray(sha1.Hash);

            Byte[] hash1 = new Byte[sha1.Hash.Length];
            sha1.Hash.CopyTo(hash1, 0);

            sha1.ComputeHash(Data);
            PrintByteArray(sha1.Hash);

            if (!Compare(sha1.Hash, hash1))
            {
                Log.Comment("FAILURE: 1");
                return false;
            }

            sha1.ComputeHash(Data1);

            if (Compare(sha1.Hash, hash1))
            {
                Log.Comment("FAILURE: 2");
                return false;
            }


            HashAlgorithm sha2 = new HashAlgorithm(HashAlgorithmType.SHA1, session);

            sha2.ComputeHash(Data);
            PrintByteArray(sha2.Hash);

            if (!Compare(sha2.Hash, hash1))
            {
                Log.Comment("FAILURE: 3");
                return false;
            }

            return true;
        }

        [TestMethod]
        public MFTestResults Sim_SHA1_Test()
        {
            bool bRes = true;

            try
            {
                using (Session sess = new Session("", MechanismType.SHA256))
                {
                    bRes &= Test(sess);
                }

                if (m_isEmulator)
                {
                    using (Session sess = new Session("Emulator_Crypto", MechanismType.SHA256))
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
