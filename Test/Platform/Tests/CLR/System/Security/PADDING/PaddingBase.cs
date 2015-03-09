using System;
using System.Collections;
using System.Security.Cryptography; 
using System.IO; 
using Microsoft.SPOT.Cryptoki;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    /// <summary>
    ///		Base class for running Crypto padding tests
    /// </summary>
    public abstract class PaddingTestBase
    {
        // encryption key to use
        protected static readonly byte[] KEY = new byte[] 
        { 
            0x11, 0x22, 0x33, 0x44, 0x55, 0xF6, 0xE7, 0xD8,
            0x11, 0x22, 0x33, 0x44, 0x55, 0xF6, 0xE7, 0xD8,
            0x11, 0x22, 0x33, 0x44, 0x55, 0xF6, 0xE7, 0xD8,
            0x11, 0x22, 0x33, 0x44, 0x55, 0xF6, 0xE7, 0xD8,
        };
        // encryption IV to use
        protected static readonly byte[] IV = new byte[] 
        { 
            0x21, 0x21, 0x21, 0x21, 0x21, 0x21, 0x21, 0x21,
            0x21, 0x21, 0x21, 0x21, 0x21, 0x21, 0x21, 0x21
        };

        /// <summary>
        ///		Print an array of bytes
        /// </summary>
        /// <param name="arr">array to print</param>
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

        /// <summary>
        ///		Compare two byte arrays
        /// </summary>
        /// <param name="rgb1">first array to compare</param>
        /// <param name="rgb2">second array to compare</param>
        /// <returns>true if the arrays are equal, false otherwise</returns>
        protected static bool Compare(byte[] rgb1, byte[] rgb2)
        {
            if (rgb1.Length != rgb2.Length)
                return false;
            for (int i = 0; i < rgb1.Length; i++)
                if (rgb1[i] != rgb2[i])
                    return false;
            return true;
        }

        /// <summary>
        ///		Run a round trip test -- the data should encrypt and decrypt back to itself
        /// </summary>
        /// <param name="key">key to use</param>
        /// <param name="iv">IV to use</param>
        /// <param name="text">data to encrypt</param>
        /// <param name="padding">padding method to use</param>
        /// <returns>true if text encrypted and decrypted back to itself</returns>
        protected bool RunRoundTrip(Session session, byte[] key, byte[] iv, byte[] text, PaddingMode padding)
        {
            try
            {
                Log.Comment("Encrypting the following bytes:");
                PrintByteArray(text);

                // setup the encryption provider
                AesCryptoServiceProvider aes = new AesCryptoServiceProvider(CryptoKey.ImportKey(session, key, CryptoKey.KeyClass.Secret, CryptoKey.KeyType.AES, true));
                aes.IV = iv;
                aes.Mode = CipherMode.ECB;
                aes.Padding = padding;

                // encrypt the data
                ICryptoTransform sse = aes.CreateEncryptor();
                //MemoryStream ms = new MemoryStream();
                //CryptoStream cs = new CryptoStream(ms, sse, CryptoStreamMode.Write);
                //cs.Write(text, 0, text.Length);
                //cs.FlushFinalBlock();
                byte[] cipherText = sse.TransformFinalBlock(text, 0, text.Length); //ms.ToArray();
                //cs.Close();

                Log.Comment("Cyphertext:");
                PrintByteArray(cipherText);

                Log.Comment("Decrypting...");
                ICryptoTransform ssd = aes.CreateDecryptor();
                //cs = new CryptoStream(new MemoryStream(cipherText), ssd, CryptoStreamMode.Read);

                // decrypt the data
                byte[] newPlainText = ssd.TransformFinalBlock(cipherText, 0, cipherText.Length); // new byte[text.Length];
                //cs.Read(newPlainText, 0, text.Length);


                Log.Comment("Plaintext:");
                PrintByteArray(newPlainText);

                // make sure the roundtrip worked
                if (!Compare(text, newPlainText))
                {
                    Log.Comment("ERROR: roundtrip failed");
                    return false;
                }
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///		Get the padding created for a specific piece of data
        /// </summary>
        /// <param name="key">key used to encrypt</param>
        /// <param name="iv">IV to use to encrypt</param>
        /// <param name="text">data to get the padding for</param>
        /// <param name="padding">type of padding to use</param>
        /// <returns>the padding generated for the data</returns>
        protected byte[] GetPadding(Session session, byte[] key, byte[] iv, byte[] text, PaddingMode padding)
        {
            try
            {
                // setup the encryption provider
                AesCryptoServiceProvider aes = new AesCryptoServiceProvider(CryptoKey.ImportKey(session, key, CryptoKey.KeyClass.Secret, CryptoKey.KeyType.AES, true));
                aes.IV = iv;
                aes.Mode = CipherMode.ECB;
                aes.Padding = padding;

                // encrypt the data
                ICryptoTransform sse = aes.CreateEncryptor();
                //MemoryStream ms = new MemoryStream();
                //CryptoStream cs = new CryptoStream(ms, sse, CryptoStreamMode.Write);
                //cs.Write(text, 0, text.Length);
                //cs.FlushFinalBlock();
                byte[] cipherText = sse.TransformFinalBlock(text, 0, text.Length); //ms.ToArray();
                //cs.Close();

                // turn off padding so it is not removed by the decryptor
                aes.Padding = PaddingMode.None;
                ICryptoTransform ssd = aes.CreateDecryptor(); //key, iv);
                //cs = new CryptoStream(new MemoryStream(cipherText), ssd, CryptoStreamMode.Read);

                // remove the original data
                //for (int i = 0; i < text.Length; i++)
                //    cs.ReadByte();

                // now read out the padding
                Log.Comment("Extracting Padding...");
                //ArrayList paddingArr = new ArrayList();
                //for (int nextByte = cs.ReadByte(); nextByte != -1; nextByte = cs.ReadByte())
                //    paddingArr.Add((byte)nextByte);

                byte[] paddingBytes = new byte[cipherText.Length - text.Length]; //(byte[])paddingArr.ToArray(new Byte().GetType());
                byte[] otherBytes = ssd.TransformFinalBlock(cipherText, 0, cipherText.Length);

                Array.Copy(otherBytes, text.Length, paddingBytes, 0, paddingBytes.Length);
                Log.Comment("Padding:");
                PrintByteArray(paddingBytes);

                return paddingBytes;
            }
            catch
            {
                return new byte[0];
            }
        }

        /// <summary>
        ///		Run tests on a set of data
        /// </summary>
        /// <param name="padding">type of padding to test</param>
        /// <returns>true if the tests all pass, false otherwise</returns>
        public bool RunTests(Session session, PaddingMode padding)
        {
            bool bRet = true;

            foreach (byte[] data in GetData())
            {
                // run both tests on all given keys
                if (!RunRoundTrip(session, KEY, IV, data, padding) || !CheckPadding(session, data, GetPadding(session, KEY, IV, data, padding)))
                {
                    Log.Comment("Test Failed");
                    bRet = false;
                }
            }

            return bRet;
        }

        /// <summary>
        ///		Abstract method to get an array of data to test padding on when it is encrypted
        /// </summary>
        protected abstract byte[][] GetData();

        /// <summary>
        ///		Method to check that the padding given is the padding that was expected
        /// </summary>
        /// <param name="data">data that was encrypted</param>
        /// <param name="padding">padding that was generated</param>
        /// <returns>true if the padding was correct, false otherwise</returns>
        protected abstract bool CheckPadding(Session session, byte[] data, byte[] padding);
    }
}