using System;
using System.Security.Cryptography;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    class RSAsign : IMFTestInterface
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
            Byte[] abData = new Byte[65536];
            Byte[] abSignature = null;
            Byte[] abSignature1 = null;

            int kl = keySize;

            for (int i = 0; i < 65536; i++) abData[i] = (Byte)(i % 256);

            try
            {
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(kl))
                {
                    rsa.HashAlgorithm = Cryptoki.MechanismType.SHA_1;

                    abSignature = rsa.SignData(abData);
                    abSignature1 = rsa.SignData(abData);
                    if (!Compare(abSignature, abSignature1))
                    {
                        Log.Comment("WRONG : two signing passes gave different signatures!");
                        bRes = false;
                    }
                    Log.Comment("Signature is : ");
                    PrintByteArray(abSignature);

                    if (rsa.VerifyData(abData, abSignature))
                    {
                        Log.Comment("CORRECT : Signature OK");
                    }
                    else
                    {
                        Log.Comment("WRONG : Signature is BAD");
                        bRes = false;
                    }

                    if (rsa.VerifyData(abData, abSignature))
                    {
                        Log.Comment("CORRECT : Pass 2 Signature OK");
                    }
                    else
                    {
                        Log.Comment("WRONG : Pass 2 Signature is BAD");
                        bRes = false;
                    }

                    abData[21]++;
                    if (rsa.VerifyData(abData, abSignature))
                    {
                        Log.Comment("WRONG : Signature OK");
                        bRes = false;
                    }
                    else
                    {
                        Log.Comment("CORRECT : Signature is BAD");
                    }

                    abData[21]--;
                    abSignature[1]++;
                    if (rsa.VerifyData(abData, abSignature))
                    {
                        Log.Comment("WRONG : Signature OK");
                        bRes = false;
                    }
                    else
                    {
                        Log.Comment("CORRECT : Signature is BAD");
                    }
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
        public MFTestResults RSASign_Test()
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
