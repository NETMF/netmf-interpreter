// known test vectors test for SHA384
//	test vectors came from http://csrc.nist.gov/cryptval/shs/SHA384-384-512.pdf
//
using System;
using System.Security.Cryptography; 
using System.IO; 
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Cryptoki;

namespace Microsoft.SPOT.Platform.Tests
{
    class Hash_SHA384known : IMFTestInterface
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
            Byte[] abDigest1 = {0xcb, 0x00, 0x75, 0x3f, 0x45, 0xa3, 0x5e, 0x8b,
        					0xb5, 0xa0, 0x3d, 0x69, 0x9a, 0xc6, 0x50, 0x07,
        					0x27, 0x2c, 0x32, 0xab, 0x0e, 0xde, 0xd1, 0x63,
        					0x1a, 0x8b, 0x60, 0x5a, 0x43, 0xff, 0x5b, 0xed,
        					0x80, 0x86, 0x07, 0x2b, 0xa1, 0xe7, 0xcc, 0x23,
        					0x58, 0xba, 0xec, 0xa1, 0x34, 0xc8, 0x25, 0xa7};
            String sData2 = "abcdefghbcdefghicdefghijdefghijkefghijklfghijklmghijklmnhijklmnoijklmnopjklmnopqklmnopqrlmnopqrsmnopqrstnopqrstu";
            Byte[] abData2 = new Byte[sData2.Length];
            for (int i = 0; i < sData2.Length; i++) abData2[i] = (Byte)sData2[i];
            Byte[] abDigest2 = {0x09, 0x33, 0x0c, 0x33, 0xf7, 0x11, 0x47, 0xe8,
        				    0x3d, 0x19, 0x2f, 0xc7, 0x82, 0xcd, 0x1b, 0x47,
        				    0x53, 0x11, 0x1b, 0x17, 0x3b, 0x3b, 0x05, 0xd2,
        				    0x2f, 0xa0, 0x80, 0x86, 0xe3, 0xb0, 0xf7, 0x12,
        				    0xfc, 0xc7, 0xc7, 0x1a, 0x55, 0x7e, 0x2d, 0xb9,
        				    0x66, 0xc3, 0xe9, 0xfa, 0x91, 0x74, 0x60, 0x39};

            Log.Comment("Testing SHA1 hash...");
            HashAlgorithm sha1 = new HashAlgorithm(HashAlgorithmType.SHA384, session);
            HashAlgorithm sha2 = new HashAlgorithm(HashAlgorithmType.SHA384, session);
            sha1.ComputeHash(abData1);
            sha2.ComputeHash(abData2);
            Log.Comment("The computed hash #1 is : ");
            PrintByteArray(sha1.Hash);
            Log.Comment("The correct hash #1 is : ");
            PrintByteArray(abDigest1);
            if (Compare(sha1.Hash, abDigest1))
            {
                Log.Comment("CORRECT");
            }
            else
            {
                Log.Comment("INCORRECT");
                bRes = false;
            }
            Log.Comment("The computed hash #2 is : ");
            PrintByteArray(sha2.Hash);
            Log.Comment("The correct hash #2 is : ");
            PrintByteArray(abDigest2);
            if (Compare(sha2.Hash, abDigest2))
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
        public MFTestResults Hash_SHA384Known_Test()
        {
            bool bRes = true;

            try
            {
                using (Session sess = new Session("", MechanismType.SHA384))
                {
                    bRes &= Test(sess);
                }

                if (m_isEmulator)
                {
                    using (Session sess = new Session("Emulator_Crypto", MechanismType.SHA384))
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
