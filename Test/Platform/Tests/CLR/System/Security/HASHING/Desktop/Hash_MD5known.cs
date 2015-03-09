// known test vectors test for MD5
//	test vectors came from rfc1321
//
using System;
using System.Security.Cryptography; 
using System.IO; 
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Cryptoki;

namespace Microsoft.SPOT.Platform.Tests
{
    class Hash_MD5known : IMFTestInterface
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
            Boolean bRes = true;
            Byte[] abData1 = { (Byte)'a', (Byte)'b', (Byte)'c' };
            Byte[] abDigest1 = { 0x90, 0x01, 0x50, 0x98, 0x3c, 0xd2, 0x4f, 0xb0, 0xd6, 0x96, 0x3f, 0x7d, 0x28, 0xe1, 0x7f, 0x72 };
            String sData2 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Byte[] abData2 = new Byte[sData2.Length];
            for (int i = 0; i < sData2.Length; i++) abData2[i] = (Byte)sData2[i];
            Byte[] abDigest2 = { 0xd1, 0x74, 0xab, 0x98, 0xd2, 0x77, 0xd9, 0xf5, 0xa5, 0x61, 0x1c, 0x2c, 0x9f, 0x41, 0x9d, 0x9f };

            Log.Comment("Testing rc21 hash...");
            HashAlgorithm rc21 = new HashAlgorithm(HashAlgorithmType.MD5, session);
            HashAlgorithm rc22 = new HashAlgorithm(HashAlgorithmType.MD5, session);
            rc21.ComputeHash(abData1);
            rc22.ComputeHash(abData2);
            Log.Comment("The computed hash #1 is : ");
            PrintByteArray(rc21.Hash);
            Log.Comment("The correct hash #1 is : ");
            PrintByteArray(abDigest1);
            if (Compare(rc21.Hash, abDigest1))
            {
                Log.Comment("CORRECT");
            }
            else
            {
                Log.Comment("INCORRECT");
                bRes = false;
            }
            Log.Comment("The computed hash #2 is : ");
            PrintByteArray(rc22.Hash);
            Log.Comment("The correct hash #2 is : ");
            PrintByteArray(abDigest2);
            if (Compare(rc22.Hash, abDigest2))
            {
                Log.Comment("CORRECT");
            }
            else
            {
                Log.Comment("INCORRECT");
                bRes = false;
            }

            return bRes;
        }

        [TestMethod]
        public MFTestResults Hash_MD5Known_Test()
        {
            bool bRes = true;

            try
            {
                using (Session sess = new Session("", MechanismType.SHA_1_HMAC))
                {
                    bRes &= Test(sess);
                }

                if (m_isEmulator)
                {
                    using (Session sess = new Session("Emulator_Crypto", MechanismType.SHA_1_HMAC))
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
