// known test vectors test for SHA256
//	test vectors came from http://csrc.nist.gov/cryptval/shs/sha256-384-512.pdf
//
using System;
using System.Security.Cryptography; 
using System.IO; 
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Cryptoki;

namespace Microsoft.SPOT.Platform.Tests
{
    class Hash_SHA256known : IMFTestInterface
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
            Byte[] abDigest1 = {0xba, 0x78, 0x16, 0xbf, 0x8f, 0x01, 0xcf, 0xea,
        					0x41, 0x41, 0x40, 0xde, 0x5d, 0xae, 0x22, 0x23,
        					0xb0, 0x03, 0x61, 0xa3, 0x96, 0x17, 0x7a, 0x9c,
        					0xb4, 0x10, 0xff, 0x61, 0xf2, 0x00, 0x15, 0xad};
            String sData2 = "abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq";
            Byte[] abData2 = new Byte[sData2.Length];
            for (int i = 0; i < sData2.Length; i++) abData2[i] = (Byte)sData2[i];
            Byte[] abDigest2 = {0x24, 0x8d, 0x6a, 0x61, 0xd2, 0x06, 0x38, 0xb8,
        					0xe5, 0xc0, 0x26, 0x93, 0x0c, 0x3e, 0x60, 0x39,
        					0xa3, 0x3c, 0xe4, 0x59, 0x64, 0xff, 0x21, 0x67,
        					0xf6, 0xec, 0xed, 0xd4, 0x19, 0xdb, 0x06, 0xc1};

            Log.Comment("Testing SHA1 hash...");
            HashAlgorithm sha1 = new HashAlgorithm(HashAlgorithmType.SHA256, session);
            HashAlgorithm sha2 = new HashAlgorithm(HashAlgorithmType.SHA256, session);
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

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [TestMethod]
        public MFTestResults Hash_SHA256Known_Test()
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
