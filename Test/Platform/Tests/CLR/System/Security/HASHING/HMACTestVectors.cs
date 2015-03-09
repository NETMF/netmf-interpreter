using System;
using System.Collections;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Cryptoki;
using System.Text;

namespace Microsoft.SPOT.Platform.Tests
{
    class TestVectorInfo
    {
        public TestVectorInfo(byte[] d, byte[] v, byte[] k, int trunc)
        {
            data = d;
            key = k;
            value = v;
            truncation = trunc;
        }
        public byte[] data;
        public byte[] key;
        public byte[] value;
        public int truncation;
    }

    class HMACTestVector : IMFTestInterface
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

        int ParseChar(char c)
        {
            if ('0' <= c && c <= '9')
            {
                return c - '0';
            }
            else if ('a' <= c && c <= 'f')
            {
                return c - 'a' + 10;
            }
            else if ('A' <= c && c <= 'F')
            {
                return c - 'A' + 10;
            }
            return 0;
        }

        byte[] ParseHexString(string hex)
        {
            byte[] ret = new byte[hex.Length / 2];
            for (int i = 0, j=0; i < hex.Length; i += 2, j++)
            {
                ret[j] = (byte)(ParseChar(hex[i]) << 4 | ParseChar(hex[i + 1]));
            }
            return ret;
        }

        [TestMethod]
        public MFTestResults HMACTestVector_Test()
        {
            bool bRet = true;
            TestVectorInfo[] vectorsSHA1 = new TestVectorInfo[]
            {
                new TestVectorInfo( UTF8Encoding.UTF8.GetBytes("Hi There"), ParseHexString("b617318655057264e28bc0b6fb378c8ef146be00"), ParseHexString("0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b"), 0),
                new TestVectorInfo( ParseHexString("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD"), ParseHexString("125d7342b9ac11cd91a39af48aa17b4f63f175d3"), ParseHexString("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), 0),
                new TestVectorInfo( ParseHexString("cdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcd"), ParseHexString("4c9007f4026250c6bc8414f9bf50c86c2d7235da"), ParseHexString("0102030405060708090a0b0c0d0e0f10111213141516171819"), 0),
                new TestVectorInfo( UTF8Encoding.UTF8.GetBytes("Test With Truncation"), ParseHexString("4c1a03424b55e07fe7f27be1d58bb9324a9a5a04"), ParseHexString("0c0c0c0c0c0c0c0c0c0c0c0c0c0c0c0c0c0c0c0c"), 0),
                new TestVectorInfo( UTF8Encoding.UTF8.GetBytes("Test Using Larger Than Block-Size Key - Hash Key First"), ParseHexString("aa4ae5e15272d00e95705637ce8a3b55ed402112"), ParseHexString("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), 0),
                new TestVectorInfo( UTF8Encoding.UTF8.GetBytes("Test Using Larger Than Block-Size Key and Larger Than One Block-Size Data"), ParseHexString("e8e99d0f45237d786d6bbaa7965c7808bbff1a91"), ParseHexString("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), 0),
                new TestVectorInfo( UTF8Encoding.UTF8.GetBytes("what do ya want for nothing?"), ParseHexString("effcdf6ae5eb2fa2d27416d5f184df9c259a7c79"), ParseHexString("4A656665"), 0),
            };
            TestVectorInfo[] vectorsSHA256 = new TestVectorInfo[]
            {
                new TestVectorInfo( UTF8Encoding.UTF8.GetBytes("abc"), ParseHexString("a21b1f5d4cf4f73a4dd939750f7a066a7f98cc131cb16a6692759021cfab8181"), ParseHexString("0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20"), 0),
                new TestVectorInfo( UTF8Encoding.UTF8.GetBytes("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq"), ParseHexString("104fdc1257328f08184ba73131c53caee698e36119421149ea8c712456697d30"), ParseHexString("0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20"), 0),
                new TestVectorInfo( UTF8Encoding.UTF8.GetBytes("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopqabcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq"), ParseHexString("470305fc7e40fe34d3eeb3e773d95aab73acf0fd060447a5eb4595bf33a9d1a3"), ParseHexString("0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20"), 0),
                new TestVectorInfo( UTF8Encoding.UTF8.GetBytes("Hi There"), ParseHexString("198a607eb44bfbc69903a0f1cf2bbdc5ba0aa3f3d9ae3c1c7a3b1696a0b68cf7"), ParseHexString("0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b"), 0),
                new TestVectorInfo(             ParseHexString("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD"), ParseHexString("cdcb1220d1ecccea91e53aba3092f962e549fe6ce9ed7fdc43191fbde45c30b0"), ParseHexString("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), 0),
                new TestVectorInfo(             ParseHexString("CDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCDCD"), ParseHexString("d4633c17f6fb8d744c66dee0f8f074556ec4af55ef07998541468eb49bd2e917"), ParseHexString("0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f202122232425"), 0),
                new TestVectorInfo( UTF8Encoding.UTF8.GetBytes("Test With Truncation"), ParseHexString("7546af01841fc09b1ab9c3749a5f1c17d4f589668a587b2700a9c97c1193cf42"), ParseHexString("0c0c0c0c0c0c0c0c0c0c0c0c0c0c0c0c0c0c0c0c0c0c0c0c0c0c0c0c0c0c0c0c"), 0),
                new TestVectorInfo( UTF8Encoding.UTF8.GetBytes("Test Using Larger Than Block-Size Key - Hash Key First"), ParseHexString("6953025ed96f0c09f80a96f78e6538dbe2e7b820e3dd970e7ddd39091b32352f"), ParseHexString("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), 0),
                new TestVectorInfo( UTF8Encoding.UTF8.GetBytes("Test Using Larger Than Block-Size Key and Larger Than One Block-Size Data"), ParseHexString("6355ac22e890d0a3c8481a5ca4825bc884d3e7a1ff98a2fc2ac7d8e064c3b2e6"), ParseHexString("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), 0),
                new TestVectorInfo( UTF8Encoding.UTF8.GetBytes("Hi There"), ParseHexString("b0344c61d8db38535ca8afceaf0bf12b881dc200c9833da726e9376c2e32cff7"), ParseHexString("0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b"), 0),
                new TestVectorInfo( UTF8Encoding.UTF8.GetBytes("what do ya want for nothing?"), ParseHexString("5bdcc146bf60754e6a042426089575c75a003f089d2739839dec58b964ec3843"), ParseHexString("4A656665"), 0),
                new TestVectorInfo(             ParseHexString("dddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd"), ParseHexString("773ea91e36800e46854db8ebd09181a72959098b3ef8c122d9635514ced565fe"), ParseHexString("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"), 0),
                new TestVectorInfo(             ParseHexString("cdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcd"), ParseHexString("82558a389a443c0ea4cc819899f2083a85f0faa3e578f8077a2e3ff46729665b"), ParseHexString("0102030405060708090a0b0c0d0e0f10111213141516171819"), 0),
                new TestVectorInfo( UTF8Encoding.UTF8.GetBytes("Test With Truncation"), ParseHexString("a3b6167473100ee06e0c796c2955552b"), ParseHexString("0c0c0c0c0c0c0c0c0c0c0c0c0c0c0c0c0c0c0c0c"), 128),
                new TestVectorInfo( UTF8Encoding.UTF8.GetBytes("Test Using Larger Than Block-Size Key - Hash Key First"), ParseHexString("60e431591ee0b67f0d8a26aacbf5b77f8e0bc6213728c5140546040f0ee37f54"), ParseHexString("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"), 0),
                new TestVectorInfo( UTF8Encoding.UTF8.GetBytes("This is a test using a larger than block-size key and a larger than block-size data. The key needs to be hashed before being used by the HMAC algorithm."), ParseHexString("9b09ffa71b942fcb27635fbcd5b0e944bfdc63644f0713938a7f51535c3a35e2"), ParseHexString("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"), 0),
            };

            using (Session session = new Session("", MechanismType.SHA_1_HMAC))
            {
                KeyedHashAlgorithm hmac = new KeyedHashAlgorithm(KeyedHashAlgorithmType.HMACSHA1, session);
                bRet &= Test(session, hmac, vectorsSHA1);
                hmac = new KeyedHashAlgorithm(KeyedHashAlgorithmType.HMACSHA256, session);
                bRet &= Test(session, hmac, vectorsSHA256);
                //hmac = new KeyedHashAlgorithm(KeyedHashAlgorithmType.HMACSHA384, session);
                //bRet &= Test(session, hmac, vectors);
                //hmac = new KeyedHashAlgorithm(KeyedHashAlgorithmType.HMACSHA512, session);
                //bRet &= Test(session, hmac, vectors);
                //hmac = new KeyedHashAlgorithm(KeyedHashAlgorithmType.HMACMD5, session);
                //bRet &= Test(session, hmac, vectors);
            }

            if (m_isEmulator)
            {
                using (Session session = new Session("Emulator_Crypto", MechanismType.SHA_1_HMAC))
                {
                    KeyedHashAlgorithm hmac = new KeyedHashAlgorithm(KeyedHashAlgorithmType.HMACSHA1, session);
                    bRet &= Test(session, hmac, vectorsSHA1);
                    hmac = new KeyedHashAlgorithm(KeyedHashAlgorithmType.HMACSHA256, session);
                    bRet &= Test(session, hmac, vectorsSHA256);
                    //hmac = new KeyedHashAlgorithm(KeyedHashAlgorithmType.HMACSHA384, session);
                    //bRet &= Test(session, hmac, vectors);
                    //hmac = new KeyedHashAlgorithm(KeyedHashAlgorithmType.HMACSHA512, session);
                    //bRet &= Test(session, hmac, vectors);
                    //hmac = new KeyedHashAlgorithm(KeyedHashAlgorithmType.HMACMD5, session);
                    //bRet &= Test(session, hmac, vectors);
                }
            }

            return bRet ? MFTestResults.Pass : MFTestResults.Fail;
        }

        private bool Test(Session session, KeyedHashAlgorithm hmac, TestVectorInfo[] vectors)
        {
            if (!RunTest(hmac, vectors))
            {
                Log.Comment("Error: Test vectors mismatch");
                return false;
            }

            Log.Comment("Test passed");
            return true;
        }

        //public static HMAC GetHMACFromName(string name)
        //{
        //    switch (name)
        //    {
        //        case "HMACSHA1":
        //            return new HMACSHA1();

        //        case "HMACSHA256":
        //            return new HMACSHA256();

        //        default:
        //            Log.Comment("Error: Unknown HMAC algorithm: {0}", name);
        //            return null;
        //    }
        //}

        //[SecuritySafeCritical]
        //public static List<TestVectorInfo> ReadTestVectors(string file)
        //{
        //    List<TestVectorInfo> vectors = new List<TestVectorInfo>();
        //    StreamReader sr = new StreamReader(file);
        //    Match match;
        //    TestVectorInfo vector;
        //    string dataType;

        //    string line = sr.ReadLine();

        //    while (line != null)
        //    {
        //        vector = new TestVectorInfo();

        //        // Get data type
        //        //
        //        match = Regex.Match(line, "                new TestVectorInfo( UTF8Encoding.UTF8.GetBytes(\"(\\w+)\"", RegexOptions.IgnoreCase);

        //        if (!match.Success)
        //        {
        //            Log.Comment("Unable to match 'datatype' on line: {0}", line);
        //            return null;
        //        }

        //        dataType = match.Groups[1].Captures[0].Value;

        //        // Get data
        //        //
        //        match = Regex.Match(line, "data=\"([^\"]+)\"", RegexOptions.IgnoreCase);

        //        if (!match.Success)
        //        {
        //            Log.Comment("Unable to match 'data' on line: {0}", line);
        //            return null;
        //        }

        //        switch (dataType)
        //        {
        //            case "string":
        //                vector.data = new System.Text.UTF8Encoding().GetBytes(match.Groups[1].Captures[0].Value);
        //                break;

        //            case "hex":
        //                vector.data = ParseHexBytes(match.Groups[1].Captures[0].Value);
        //                break;

        //            case "base64":
        //                vector.data = Convert.FromBase64String(match.Groups[1].Captures[0].Value);
        //                break;

        //            default:
        //                Log.Comment("Unknown data type '{0}'", dataType);
        //                break;
        //        }

        //        // Get key
        //        //
        //        match = Regex.Match(line, "key=\"([a-f0-9]+)\"", RegexOptions.IgnoreCase);

        //        if (!match.Success)
        //        {
        //            Log.Comment("Unable to match 'key' on line: {0}", line);
        //            return null;
        //        }

        //        vector.key = ParseHexBytes(match.Groups[1].Captures[0].Value);

        //        // Get expected value
        //        //
        //        match = Regex.Match(line, "value=\"([a-f0-9]+)\"", RegexOptions.IgnoreCase);

        //        if (!match.Success)
        //        {
        //            Log.Comment("Unable to match 'value' on line: {0}", line);
        //            return null;
        //        }

        //        vector.value = ParseHexBytes(match.Groups[1].Captures[0].Value);

        //        // Get truncation (if it exists)
        //        //
        //        match = Regex.Match(line, "truncation=\"(\\d+)\"", RegexOptions.IgnoreCase);

        //        if (match.Success)
        //            vector.truncation = Int32.Parse(match.Groups[1].Captures[0].Value);
        //        else
        //            vector.truncation = 0;

        //        vectors.Add(vector);
        //        line = sr.ReadLine();
        //    }

        //    return vectors;
        //}

        private static string ByteToString(byte b)
        {
            char []str = new char[2];

            byte nib = (byte)((b >> 4) & 0xF);

            if (0 <= nib && nib <= 9)
            {
                str[0] = (char)('0' + nib);
            }
            else
            {
                str[0] = (char)('F' + (nib - 10));
            }

            nib = (byte)(b & 0xF);

            if (0 <= nib && nib <= 9)
            {
                str[1] = (char)('0' + nib);
            }
            else
            {
                str[1] = (char)('F' + (nib - 10));
            }

            return new string(str);
        }

        public static string ByteArrayToString(byte[] bytes)
        {
            string str = "";

            for (int i = 0; i < bytes.Length; i++)
                str += ByteToString(bytes[i]);

            return str;
        }

        public static bool CompareBytes(byte[] x, byte[] y)
        {
            if (x.Length != y.Length)
                return false;

            for (int i = 0; i < x.Length; i++)
                if (x[i] != y[i])
                    return false;

            return true;
        }

        public static bool RunTest(KeyedHashAlgorithm hmac, TestVectorInfo[] vectors)
        {
            byte[] computedHash = null;
            byte[] computedHashTruncated = null;

            foreach (TestVectorInfo vector in vectors)
            {
                CryptoKey key = CryptoKey.LoadKey(hmac.Session, new CryptokiAttribute[] { 
                    new CryptokiAttribute(CryptokiAttribute.CryptokiType.Class  , Utility.ConvertToBytes((int)CryptokiClass.SECRET_KEY)),
                    new CryptokiAttribute(CryptokiAttribute.CryptokiType.KeyType, Utility.ConvertToBytes((int)CryptoKey.KeyType.GENERIC_SECRET)),
                    new CryptokiAttribute(CryptokiAttribute.CryptokiType.Value  , vector.key)
                });

                hmac.Key = key;
                computedHash = hmac.ComputeHash(vector.data);

                if (vector.truncation > 0)
                {
                    computedHashTruncated = new byte[vector.truncation / 8];
                    Array.Copy(computedHash, computedHashTruncated, vector.truncation / 8);
                }
                else
                {
                    computedHashTruncated = computedHash;
                }

                if (!CompareBytes(computedHashTruncated, vector.value))
                {
                    Log.Comment("Error - HMAC value miscomparison");
                    Log.Comment("Data: " + ByteArrayToString(vector.data));
                    Log.Comment("Key: " + ByteArrayToString(vector.key));
                    Log.Comment("Actual Result: " + ByteArrayToString(computedHash));
                    Log.Comment("Expected Result: " + ByteArrayToString(vector.value));
                    return false;
                }
            }

            return true;
        }
    }
}