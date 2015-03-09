using System;
using System.Security.Cryptography;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    class RSAEncrypt : IMFTestInterface
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
            for (i = 0; i < arr.Length; i++)
            {
                str += arr[i] + "    ";
                if ((i + 9) % 8 == 0)
                {
                    Log.Comment(str);
                    str = "";
                }
            }
            if (i % 8 != 0) Log.Comment(str);
        }



        public static Boolean Test(int keySize)
        {
            Boolean bRes = true;
            Byte[] abPlain = { 0, 1, 2, 3, 4, 5, 6, 7 };
            Byte[] abCipher = null;
            int kl = keySize;

            try
            {
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(kl))
                {
                    abCipher = rsa.Encrypt(abPlain);
                    Log.Comment("Cipher is : ");
                    PrintByteArray(abCipher);
                    abCipher = rsa.Decrypt(abCipher);
                }
                Log.Comment("Decrypted plaintext is : ");
                PrintByteArray(abCipher);

                if (!Compare(abPlain, abCipher))
                {
                    bRes = false;
                    Log.Comment("Failed to decrypt to the original plaintext");
                }

            }
            catch (Exception e)
            {
                Log.Comment("Exception ocured :\n" + e.ToString());
                bRes = false;
            }

            return bRes;
        }

        [TestMethod]
        public MFTestResults RSAEncr_Test()
        {
            bool bRes = true;

            try
            {
                for (int ks = 512; ks <= 1024; ks += 512)
                {
                    Log.Comment("KeySize: " + ks);
                    bRes &= Test(ks);
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
