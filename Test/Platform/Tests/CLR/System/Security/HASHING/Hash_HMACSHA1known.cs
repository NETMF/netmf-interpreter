// known test vectors test for HMACSHA1
//	test vectors came from rfc2202
//
using System;
using System.Security.Cryptography; 
using System.IO; 
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Cryptoki;

namespace Microsoft.SPOT.Platform.Tests
{
    class Hash_HMACSHA1known : IMFTestInterface
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
            Byte[] abKey1 = { 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b };
            Byte[] abData1 = (new System.Text.UTF8Encoding()).GetBytes("Hi There");
            Byte[] abDigest1 = { 0xb6, 0x17, 0x31, 0x86, 0x55, 0x05, 0x72, 0x64, 0xe2, 0x8b, 0xc0, 0xb6, 0xfb, 0x37, 0x8c, 0x8e, 0xf1, 0x46, 0xbe, 0x00 };
            Byte[] abKey2 = (new System.Text.UTF8Encoding()).GetBytes("Jefe");
            Byte[] abData2 = (new System.Text.UTF8Encoding()).GetBytes("what do ya want for nothing?");
            Byte[] abDigest2 = { 0xef, 0xfc, 0xdf, 0x6a, 0xe5, 0xeb, 0x2f, 0xa2, 0xd2, 0x74, 0x16, 0xd5, 0xf1, 0x84, 0xdf, 0x9c, 0x25, 0x9a, 0x7c, 0x79 };

            CryptoKey key1 = CryptoKey.LoadKey(session, new CryptokiAttribute[] { 
                    new CryptokiAttribute(CryptokiAttribute.CryptokiType.Class  , Utility.ConvertToBytes((int)CryptokiClass.SECRET_KEY)),
                    new CryptokiAttribute(CryptokiAttribute.CryptokiType.KeyType, Utility.ConvertToBytes((int)CryptoKey.KeyType.GENERIC_SECRET)),
                    new CryptokiAttribute(CryptokiAttribute.CryptokiType.Value  , abKey1)
            });

            CryptoKey key2 = CryptoKey.LoadKey(session, new CryptokiAttribute[] { 
                    new CryptokiAttribute(CryptokiAttribute.CryptokiType.Class  , Utility.ConvertToBytes((int)CryptokiClass.SECRET_KEY)),
                    new CryptokiAttribute(CryptokiAttribute.CryptokiType.KeyType, Utility.ConvertToBytes((int)CryptoKey.KeyType.GENERIC_SECRET)),
                    new CryptokiAttribute(CryptokiAttribute.CryptokiType.Value  , abKey2)
            });

            Log.Comment("Testing rc21 hash...");
            using(KeyedHashAlgorithm rc21 = new KeyedHashAlgorithm(KeyedHashAlgorithmType.HMACSHA1, key1))
            using (KeyedHashAlgorithm rc22 = new KeyedHashAlgorithm(KeyedHashAlgorithmType.HMACSHA1, key2))
            {
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
            }
            return bRes;
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [TestMethod]
        public MFTestResults Hash_HMACSHA1Known_Test()
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