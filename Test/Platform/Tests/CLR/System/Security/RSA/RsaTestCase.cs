using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Cryptoki;

/// <summary>
/// This test is to verify RSAServiceProvider VerifyHash API
/// The RSAParameter was genereated on Desktop.
/// </summary>
namespace Microsoft.SPOT.Platform.Tests
{
    public class RsaSignatureTest : IMFTestInterface
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
        public MFTestResults RSATestCase_Test()
        {
            bool bRes = true;

            try
            {
                using (Session sess = new Session("", MechanismType.RSA_PKCS))
                {
                    bRes &= Test(sess);
                }

                if (m_isEmulator)
                {
                    using (Session sess = new Session("Emulator_Crypto", MechanismType.RSA_PKCS))
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
            bool bRet = true;

#if DEFINED_BASE64TRANSFORM
            // Make sure FromBase64Transform works correctly
            if (!RunBase64Test(session))
                bRet = false;

            // Make sure FromBase64Transform works for some exception scenarios
            if (!RunBase64ExceptionTest(session))
                bRet = false;
#endif
            // Make sure VerifyHash works correctly
            if (!RunBasicScenario(session))
                bRet = false;

            // Make sure RSAServiceProvider can handle various exception scenarios
            if (!RunRSAExceptionTest(session))
                bRet = false;

            return bRet;
        }

#if DEFINED_BASE64TRANSFORM
        static Boolean RunBase64ExceptionTest(Session session)
        {
            string origninal = "Hello from RSA Crypto.";
            byte[] origin = ConvertToByteArray(origninal);

            string transformed = Convert.ToBase64String(origin);
            string s1 = transformed.Insert(5, "=");
            string s2 = transformed.Insert(5, ":");
            byte[] ba1 = ConvertToByteArray(s1);
            byte[] ba2 = ConvertToByteArray(s2);


            return WriteToBase64StreamAndThrow(ba1)
                && WriteToBase64StreamAndThrow(ba2);
        }

        static bool WriteToBase64StreamAndThrow(byte[] ba1)
        {
            try
            {
                MemoryStream ms1 = new MemoryStream();
                CryptoStream cs1 = new CryptoStream(ms1, new FromBase64Transform(), CryptoStreamMode.Write);
                cs1.Write(ba1, 0, ba1.Length);
                cs1.Close();
            }
            catch (FormatException)
            {
                return true;
            }
            Log.Comment("Fail to see FormatExcption.");
            return false;
        }

        static Boolean RunBase64Test()
        {
            string origninal = "Hello from RSA Crypto.";
            byte[] origin = ConvertToByteArray(origninal);

            string transformed = Convert.ToBase64String(origin);
            byte[] ba1 = ConvertToByteArray(transformed);
            List<byte> b = new List<byte>(ba1);
            b.Insert(0, 0x0d);
            b.Insert(5, 0x0a);
            byte[] ba2 = b.ToArray();

            MemoryStream ms1 = new MemoryStream();
            CryptoStream cs1 = new CryptoStream(ms1, new FromBase64Transform(), CryptoStreamMode.Write);
            cs1.Write(ba1, 0, ba1.Length);
            cs1.Close();

            MemoryStream ms2 = new MemoryStream();
            CryptoStream cs2 = new CryptoStream(ms2, new FromBase64Transform(), CryptoStreamMode.Write);
            cs2.Write(ba2, 0, (int)ba2.Length);
            cs2.Close();

            FromBase64Transform f = new FromBase64Transform();
            if (f.CanReuseTransform == false)
            {
                Log.Comment("Transofrm should be able to be reused");
                return false;
            }

            f.Clear();

            if (!Compare(ms1.ToArray(), ms2.ToArray())) return false;
            if (!Compare(ms1.ToArray(), origin)) return false;

            return true;
        }
#endif
        static bool RunRSAExceptionTest(Session session)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(session);
            rsa.ImportParameters(RsaSignatureTestData.GetKeyParameters());

            byte[] r = new byte[16]; r[0] = 9; // Fake MD5 hash value

            try
            {
                if (rsa.VerifyHash(r, MechanismType.MD5, RsaSignatureTestData.GetSignatureValue()))
                {
                    Log.Comment("Should fail for a faked hash");
                    return false;
                }
            }
            catch (CryptographicException)
            { }

            try
            {
                rsa.VerifyHash(RsaSignatureTestData.GetHashValue(), (MechanismType)(0xffffffff), RsaSignatureTestData.GetSignatureValue());
                Log.Comment("Shoul throw");
                return false;
            }
            catch (CryptographicException)
            {
            }

            try
            {
                rsa.VerifyHash(null, MechanismType.SHA_1, null);
                Log.Comment("Shoul throw");
                return false;
            }
            catch (ArgumentNullException)
            {
            }


            byte[] hash = RsaSignatureTestData.GetHashValue();
            byte[] sig = RsaSignatureTestData.GetSignatureValue();

            hash[0] = 9;

            if (rsa.VerifyHash(hash, MechanismType.SHA_1, sig))
            {
                Log.Comment("VerifyHash should return false, because I changed one byte.");
                return false;
            }
            return true;
        }

        static bool RunBasicScenario(Session session)
        {
            try
            {
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(session))
                {
                    Log.Comment("Importing RSA public key");
                    rsa.ImportParameters(RsaSignatureTestData.GetKeyParameters());

                    Log.Comment("RSA key size: " + rsa.KeySize);

                    Log.Comment("Verifying SHA-1 signature");
                    bool verified = rsa.VerifyHash(RsaSignatureTestData.GetHashValue(), MechanismType.SHA_1, RsaSignatureTestData.GetSignatureValue());
                    Log.Comment("Complete verifying SHA-1 signature");

                    if (!verified)
                    {
                        Log.Comment("Signature failed to verify - fail.");
                        return false;
                    }

#if DEFINED_BASE64TRANSFORM
                    byte[] signature = RsaSignatureTestData.GetSignatureValue();

                    string before = Convert.ToBase64String(signature);
                    byte[] after = ConvertToByteArray(before);

                    MemoryStream ms = new MemoryStream();
                    CryptoStream cs = new CryptoStream(ms, new FromBase64Transform(), CryptoStreamMode.Write);
                    cs.Write(after, 0, after.Length);
                    cs.Close();

                    if (!Compare(ms.ToArray(), signature))
                    {
                        Log.Comment("Base64 Transform failed.");
                        return false;
                    }
#endif
                }
            }
            catch (Exception e)
            {
                Log.Comment("Fail - got an exception");
                Log.Exception("",e);
                return false;
            }
            return true;
        }

        private static byte[] ConvertToByteArray(string str)
        {
            char[] ch = str.ToCharArray();
            byte[] ret = new byte[ch.Length];
            for (int i = 0; i < ch.Length; i++)
                ret[i] = (byte)(ch[i]);
            return ret;
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
    }
}