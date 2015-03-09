// known test vectors test for SHA512
//	test vectors came from http://csrc.nist.gov/cryptval/shs/sha256-384-512.pdf
//
using System;
using System.Security.Cryptography; 
using System.IO; 
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Cryptoki;

namespace Microsoft.SPOT.Platform.Tests
{
    class Hash_SHA512known : IMFTestInterface
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
            Byte[] abDigest1 = {0xdd, 0xaf, 0x35, 0xa1, 0x93, 0x61, 0x7a, 0xba,
        					0xcc, 0x41, 0x73, 0x49, 0xae, 0x20, 0x41, 0x31,
        					0x12, 0xe6, 0xfa, 0x4e, 0x89, 0xa9, 0x7e, 0xa2,
        					0x0a, 0x9e, 0xee, 0xe6, 0x4b, 0x55, 0xd3, 0x9a,
        					0x21, 0x92, 0x99, 0x2a, 0x27, 0x4f, 0xc1, 0xa8,
        					0x36, 0xba, 0x3c, 0x23, 0xa3, 0xfe, 0xeb, 0xbd,
        					0x45, 0x4d, 0x44, 0x23, 0x64, 0x3c, 0xe8, 0x0e,
        					0x2a, 0x9a, 0xc9, 0x4f, 0xa5, 0x4c, 0xa4, 0x9f};
            String sData2 = "abcdefghbcdefghicdefghijdefghijkefghijklfghijklmghijklmnhijklmnoijklmnopjklmnopqklmnopqrlmnopqrsmnopqrstnopqrstu";
            Byte[] abData2 = new Byte[sData2.Length];
            for (int i = 0; i < sData2.Length; i++) abData2[i] = (Byte)sData2[i];
            Byte[] abDigest2 = {0x8e, 0x95, 0x9b, 0x75, 0xda, 0xe3, 0x13, 0xda,
        					0x8c, 0xf4, 0xf7, 0x28, 0x14, 0xfc, 0x14, 0x3f,
        					0x8f, 0x77, 0x79, 0xc6, 0xeb, 0x9f, 0x7f, 0xa1,
        					0x72, 0x99, 0xae, 0xad, 0xb6, 0x88, 0x90, 0x18,
        					0x50, 0x1d, 0x28, 0x9e, 0x49, 0x00, 0xf7, 0xe4,
        					0x33, 0x1b, 0x99, 0xde, 0xc4, 0xb5, 0x43, 0x3a,
        					0xc7, 0xd3, 0x29, 0xee, 0xb6, 0xdd, 0x26, 0x54,
        					0x5e, 0x96, 0xe5, 0x5b, 0x87, 0x4b, 0xe9, 0x09};

            Log.Comment("Testing SHA1 hash...");
            HashAlgorithm sha1 = new HashAlgorithm(HashAlgorithmType.SHA512, session);
            HashAlgorithm sha2 = new HashAlgorithm(HashAlgorithmType.SHA512, session);
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
        public MFTestResults Hash_SHA512Known_Test()
        {
            bool bRes = true;

            try
            {
                using (Session sess = new Session("", MechanismType.SHA512))
                {
                    bRes &= Test(sess);
                }

                if (m_isEmulator)
                {
                    using (Session sess = new Session("Emulator_Crypto", MechanismType.SHA512))
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

