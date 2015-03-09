using System;
using System.Security.Cryptography;
using System.IO;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    class RSASignStream : IMFTestInterface
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



        public static Boolean Test()
        {
            Boolean bRes = true;
            Byte[] abSignature = null;
            Byte[] abSignature1 = null;

            Byte[] data = new byte[100];

            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)i;
            }

            using(MemoryStream fs = new MemoryStream(data))
            using (MemoryStream fs1 = new MemoryStream((byte[])data.Clone()))
            {
                Byte[] abData = new Byte[fs.Length];
                fs.Read(abData, 0, (int)fs.Length);
                fs.Position = 0;

                try
                {
                    using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                    {
                        rsa.HashAlgorithm = Cryptoki.MechanismType.SHA_1;
                        abSignature = rsa.SignData(fs);
                        abSignature1 = rsa.SignData(fs1);
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

                        if (rsa.VerifyData(abData, abSignature1))
                        {
                            Log.Comment("CORRECT : Pass 2 Signature OK");
                        }
                        else
                        {
                            Log.Comment("WRONG : Pass 2 Signature is BAD");
                            bRes = false;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Comment("Exception ocured :\n" + e.ToString());
                    bRes = false;
                }

                fs.Close();
                fs1.Close();
            }

            return bRes;
        }

        [TestMethod]
        public MFTestResults RSASignStream_Test()
        {
            bool bRes = true;

            try
            {
                bRes &= Test();
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
