using System;
using System.Reflection;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.Security.Cryptography;
using Microsoft.SPOT.Cryptoki;

namespace Microsoft.SPOT.Platform.Tests
{

    public static class SymmetricTestHelper
    {
        public static MFTestResults Test_EncryptUpdate(SymmetricAlgorithm csp)
        {
            bool testResult = false;

            try
            {
                string dataToEncrypt = "This is a simple message to be encrypted..";

                byte[] pTxt = System.Text.UTF8Encoding.UTF8.GetBytes(dataToEncrypt);
                byte[] data = new byte[1033];
                byte[] newData = new byte[1033];

                int o = 0;
                while ((o + pTxt.Length) < data.Length)
                {
                    Array.Copy(pTxt, 0, data, o, pTxt.Length);
                    o += pTxt.Length;
                }
                while (o < data.Length)
                {
                    data[o++] = (byte)'a';
                }

                dataToEncrypt = new string(System.Text.UTF8Encoding.UTF8.GetChars(data));

                byte[] output = new byte[data.Length * 2];
                byte[] encData = null;
                int offset = 0, outputOffset = 0;
                int encrLength = 0;

                using (ICryptoTransform encr = csp.CreateEncryptor())
                {
                    int chunk = csp.BlockSize * 2;

                    while (offset + chunk < data.Length)
                    {
                        outputOffset += encr.TransformBlock(data, offset, chunk, output, outputOffset);
                        offset += chunk;
                    }

                    encData = encr.TransformFinalBlock(data, offset, data.Length - offset);

                    Array.Copy(encData, 0, output, outputOffset, encData.Length);

                    encrLength = outputOffset + encData.Length;

                    offset = 0;
                    outputOffset = 0;
                }

                using (ICryptoTransform decr = csp.CreateDecryptor())
                {
                    int chunk = csp.BlockSize * 2;

                    while (offset + chunk < encrLength)
                    {
                        outputOffset += decr.TransformBlock(output, offset, chunk, newData, outputOffset);
                        offset += chunk;
                    }

                    encData = decr.TransformFinalBlock(output, offset, encrLength - offset);

                    Array.Copy(encData, 0, newData, outputOffset, encData.Length);
                }

                testResult = true;
                for (int i = 0; i < newData.Length; i++)
                {
                    if (newData[i] != data[i])
                    {
                        testResult = false;
                        break;
                    }
                }

                if (testResult)
                {
                    string res = new string(System.Text.UTF8Encoding.UTF8.GetChars(newData));

                    //Debug.Print(dataToEncrypt);
                    //Debug.Print(res);

                    testResult = string.Compare(dataToEncrypt, res) == 0;
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        public static MFTestResults Test_Encrypt(SymmetricAlgorithm csp)
        {
            bool testResult = false;

            try
            {
                string dataToEncrypt = "This is a simple message to be encrypted  dfjdfh ";

                byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes(dataToEncrypt);
                byte[] encData = null;
                byte[] newData = null;

                using (ICryptoTransform encr = csp.CreateEncryptor())
                {
                    encData = encr.TransformFinalBlock(data, 0, data.Length);
                }
                using (ICryptoTransform decr = csp.CreateDecryptor())
                {
                    newData = decr.TransformFinalBlock(encData, 0, encData.Length);
                }

                string res = new string(System.Text.UTF8Encoding.UTF8.GetChars(newData));

                //Debug.Print(dataToEncrypt);
                //Debug.Print(res);

                testResult = string.Compare(dataToEncrypt, res) == 0;
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        public static MFTestResults Test_EncryptCsp(SymmetricAlgorithm csp1, SymmetricAlgorithm csp2, CryptokiAttribute[] keyTemplate)
        {
            bool testResult = false;

            try
            {
                string dataToEncrypt = "This is a simple message to be encrypted  dfjdfh ";

                byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes(dataToEncrypt);
                byte[] encData = null;
                byte[] newData = null;

                csp2.Key = CryptoKey.CreateObject(csp2.Session, keyTemplate) as CryptoKey;
                csp1.Key = CryptoKey.CreateObject(csp1.Session, keyTemplate) as CryptoKey;

                csp2.IV  = csp1.IV;

                using (ICryptoTransform encr = csp1.CreateEncryptor())
                {
                    encData = encr.TransformFinalBlock(data, 0, data.Length);
                }
                using (ICryptoTransform decr = csp2.CreateDecryptor())
                {
                    newData = decr.TransformFinalBlock(encData, 0, encData.Length);
                }

                string res = new string(System.Text.UTF8Encoding.UTF8.GetChars(newData));

                //Debug.Print(dataToEncrypt);
                //Debug.Print(res);

                testResult = string.Compare(dataToEncrypt, res) == 0;
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }
    }
}