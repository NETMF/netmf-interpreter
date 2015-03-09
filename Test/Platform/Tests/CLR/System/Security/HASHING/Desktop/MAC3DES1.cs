using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Cryptoki;

namespace Microsoft.SPOT.Platform.Tests
{
    class MAC3DES1 : IMFTestInterface
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


        [TestMethod]
        public MFTestResults MAC3DES1_Test()
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

        public static bool Test(Session session)
        {
            bool bRes = true;

            byte[] plaintext = new byte[16];
            for (int i = 0; i < plaintext.Length - 5; i++) plaintext[i] = (byte)i;
            for (int i = plaintext.Length - 5; i < plaintext.Length; i++) plaintext[i] = (byte)0;

            byte[] plaintext1 = new byte[plaintext.Length - 5];
            for (int i = 0; i < plaintext1.Length; i++) plaintext1[i] = (byte)i;

            SymmetricAlgorithm td = new TripleDESCryptoServiceProvider(session);
            td.Padding = PaddingMode.None;
            td.IV = new byte[8];

            ICryptoTransform sse = td.CreateEncryptor();
            //MemoryStream ms = new MemoryStream(plaintext);
            ICryptoTransform ct = td.CreateEncryptor();
            //CryptoStream cs1 = new CryptoStream(ms, sse, CryptoStreamMode.Write);
            //cs1.Write(plaintext, 0, plaintext.Length);
            //cs1.FlushFinalBlock();
            //Log.Comment(ms.Position);
            //byte[] ciphertext = ms.ToArray();
            //cs1.Close();
            byte[] ciphertext = ct.TransformFinalBlock(plaintext, 0, plaintext.Length);
            ct.Dispose();

            Log.Comment("CipherText:");
            PrintByteArray(ciphertext);

            td.Padding = PaddingMode.Zeros;
            ICryptoTransform sse1 = td.CreateEncryptor();
            //MemoryStream ms1 = new MemoryStream();
            //CryptoStream cs2 = new CryptoStream(ms1, sse1, CryptoStreamMode.Write);
            //cs2.Write(plaintext1, 0, plaintext1.Length);
            cs2.FlushFinalBlock();
            Log.Comment(ms1.Position);
            byte[] ciphertext1 = ms1.ToArray();
            cs2.Close();

            Log.Comment("CipherText #2:");
            PrintByteArray(ciphertext1);

            if (!Compare(ciphertext, ciphertext1))
            {
                bRes = false;
                Log.Comment("WRONG: ciphertexts are different. Probably padding problems.");
            }


            MACTripleDES mtd = new MACTripleDES(td.Key);
            byte[] hash = mtd.ComputeHash(plaintext1);

            Log.Comment("Hash:");
            PrintByteArray(hash);

            byte[] subciphertext = new byte[8];
            Array.Copy(ciphertext, ciphertext.Length - 8, subciphertext, 0, 8);

            if (!Compare(subciphertext, hash))
            {
                Log.Comment("WRONG: MAC3DES result is different from the last block of ciphertext!");
                bRes = false;
            }

            return bRes;
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
    }
}