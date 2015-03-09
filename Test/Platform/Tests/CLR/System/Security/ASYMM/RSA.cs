using System;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.SPOT.Platform.Tests;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Cryptoki;

/// <remarks>
///		This test was created to fill coverage holes, and is not intended as a
///		complete test suite for the RSA classes.  Look at other RSA tests in this
///		folder for more testing.
/// </remarks>
namespace Microsoft.SPOT.Platform.Tests
{
    public class RSADesktopTest : IMFTestInterface
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
        public MFTestResults RSADesktop_Test()
        {
            Exception[] cryptoEx = new Exception[] { new CryptographicException(), new CryptographicUnexpectedOperationException() };
            Exception[] notSupportedEx = new Exception[] { new NotSupportedException() };

            //TestGroup rsaExcept = new TestGroup("RSA Exception Tests", new TestCase[]
            //{
            //    new ExceptionTestCase("FromXmlString null", new ExceptionTestCaseImpl(RsaFromXmlStringNull), ExceptionTestCase.ArgumentExceptions),
            //    new ExceptionTestCase("FromXmlString no modulus", new ExceptionTestCaseImpl(RsaFromXmlStringNoMod), cryptoEx),
            //    new ExceptionTestCase("FromXmlString no exponent", new ExceptionTestCaseImpl(RsaFromXmlStringNoExp), cryptoEx)
            //});

            TestGroup cspApi = new TestGroup("RSA CSP API Tests", new TestCase[]
		{
			//new TestCase("Check KeyExchangeAlgorithm", new TestCaseImpl(CspCheckKeyExchange)),
			//new TestCase("Check SignatureAlgorithm", new TestCaseImpl(CspCheckSignatureAlgorithm)),
			//new TestCase("Check UseMachineStore", new TestCaseImpl(CspCheckUseMachineStore)),
			//new TestCase("Check PersistKeys, then set true", new TestCaseImpl(CspCheckPersistKeys)),
			new TestCase("Sign bytes", new TestCaseImpl(CspSignBytes)),
			//new TestCase("IsPublic on bad key", new TestCaseImpl(IsPublicBadKey)),
			new TestCase("Sign with MD5", new TestCaseImpl(SignMD5))
		});

            TestGroup cspExcept = new TestGroup("RSA CSP Exception Tests", new TestCase[]
		{
			new ExceptionTestCase("Create negative key size", new ExceptionTestCaseImpl(CspNegativeKey), ExceptionTestCase.ArgumentExceptions),
			new ExceptionTestCase("Sign null hash", new ExceptionTestCaseImpl(CspSignNullHash), ExceptionTestCase.NullExceptions),
			new ExceptionTestCase("Sign with only public key", new ExceptionTestCaseImpl(CspSignPublic), cryptoEx),
			new ExceptionTestCase("Verify null hash", new ExceptionTestCaseImpl(CspVerifyNullHash), ExceptionTestCase.NullExceptions),
			new ExceptionTestCase("Verify null signature", new ExceptionTestCaseImpl(CspVerifyNullSig), ExceptionTestCase.NullExceptions),
			new ExceptionTestCase("Decrypt null bytes", new ExceptionTestCaseImpl(CspDecryptNullBytes), ExceptionTestCase.NullExceptions),
			new ExceptionTestCase("Decrypt wrong sized bytes", new ExceptionTestCaseImpl(CspDecryptWrongSizeBytes), cryptoEx),
			//new ExceptionTestCase("Decrypt value", new ExceptionTestCaseImpl(CspDecryptValue), notSupportedEx),
			//new ExceptionTestCase("Encrypt value", new ExceptionTestCaseImpl(CspEncryptValue), notSupportedEx),
			//new ExceptionTestCase("IsPublic on null key", new ExceptionTestCaseImpl(IsPublicNullKey), ExceptionTestCase.NullExceptions)
		});

        //    TestGroup oaepKeyApi = new TestGroup("OAEP Key Formatting API Tests", new TestCase[]
        //{
        //    new TestCase("Check Parameters", new TestCaseImpl(OaepKeyParams)),
        //    new TestCase("Round trip, RSA CSP", new TestCaseImpl(OaepKeyCSPRT)),
        //    new TestCase("Round trip, non-CSP", new TestCaseImpl(OaepKeyNotCSPRT)),
        //    new TestCase("Set parameter null", new TestCaseImpl(OaepKeyParameterNull)),
        //    new TestCase("Set parameters not-null", new TestCaseImpl(OaepKeyParameterNotNull)),
        //    new TestCase("Set RNG", new TestCaseImpl(OaepKeySetRng))
        //});

        //    TestGroup oaepKeyExcept = new TestGroup("OAEP Key Formatting Exception Tests", new TestCase[]
        //{
        //    new ExceptionTestCase("Deformat null", new ExceptionTestCaseImpl(OaepKeyDeformatNull), ExceptionTestCase.NullExceptions),
        //    new ExceptionTestCase("Deformat bad key", new ExceptionTestCaseImpl(OaepKeyDeformatBad), cryptoEx),
        //    new ExceptionTestCase("Deformat null key", new ExceptionTestCaseImpl(OaepKeyDeformatSetNull), ExceptionTestCase.NullExceptions),
        //    new ExceptionTestCase("Format set null key", new ExceptionTestCaseImpl(OaepKeyFormatSetNull), ExceptionTestCase.NullExceptions),
        //    new ExceptionTestCase("Format null key", new ExceptionTestCaseImpl(OaepKeyFormatNull), ExceptionTestCase.NullExceptions)
        //});

        //    TestGroup pkcsKeyApi = new TestGroup("PKCS1 Key Formatting API Tests", new TestCase[]
        //{
        //    new TestCase("Set RNG", new TestCaseImpl(PkcsKeySetRng)),
        //    new TestCase("Set Parameters", new TestCaseImpl(PkcsKeySetParams)),
        //    new TestCase("Round trip, RSA CSP", new TestCaseImpl(PkcsKeyCSPRT)),
        //    new TestCase("Round trip, non-CSP", new TestCaseImpl(PkcsKeyNotCSPRT)),
        //    new TestCase("Check formatter parameters", new TestCaseImpl(PkcsKeyCheckFormatParams))
        //});

        //    TestGroup pkcsKeyExcept = new TestGroup("PKCS1 Key Formatting Exception Tests", new TestCase[]
        //{
        //    new ExceptionTestCase("Deformat null", new ExceptionTestCaseImpl(PkcsKeyDeformatNull), ExceptionTestCase.NullExceptions),
        //    new ExceptionTestCase("Deformat set null key", new ExceptionTestCaseImpl(PkcsKeySetNullKey), ExceptionTestCase.NullExceptions),
        //    new ExceptionTestCase("Format null key", new ExceptionTestCaseImpl(PkcsKeyFormatNull), ExceptionTestCase.NullExceptions),
        //    new ExceptionTestCase("Format set null key", new ExceptionTestCaseImpl(PkcsKeyFormatSetNullKey), ExceptionTestCase.NullExceptions)
        //});

        //    TestGroup pkcsSigApi = new TestGroup("PKCS1 Signature Formatting API Tests", new TestCase[]
        //{
        //    new TestCase("Round trip, RSA CSP", new TestCaseImpl(PkcsSigCSPRT)),
        //    new TestCase("Round trip, non-CSP", new TestCaseImpl(PkcsSigNotCSPRT))
        //});

        //    TestGroup pkcsSigExcept = new TestGroup("PKCS1 Signature Formatting Exception Tests", new TestCase[]
        //{
        //    new ExceptionTestCase("Deformat set null key", new ExceptionTestCaseImpl(PkcsSigDeformatSetNullKey), ExceptionTestCase.NullExceptions),
        //    new ExceptionTestCase("Verify null key", new ExceptionTestCaseImpl(PkcsSigVerifyNullKey), cryptoEx),
        //    new ExceptionTestCase("Verify null oid", new ExceptionTestCaseImpl(PkcsSigVerifyNullOid), cryptoEx),
        //    new ExceptionTestCase("Verify null hash", new ExceptionTestCaseImpl(PkcsSigVerifyNullHash), ExceptionTestCase.NullExceptions),
        //    new ExceptionTestCase("Verify null signature", new ExceptionTestCaseImpl(PkcsSigVerifyNullSig), ExceptionTestCase.NullExceptions),
        //    new ExceptionTestCase("Format set null key", new ExceptionTestCaseImpl(PkcsSigFormatSetNullKey), ExceptionTestCase.NullExceptions),
        //    new ExceptionTestCase("Sign null oid", new ExceptionTestCaseImpl(PkcsSigSignNullOid), cryptoEx),
        //    new ExceptionTestCase("Sign null key", new ExceptionTestCaseImpl(PkcsSigSignNullKey), cryptoEx),
        //    new ExceptionTestCase("Sign null hash", new ExceptionTestCaseImpl(PkcsSigSignNullHash), ExceptionTestCase.NullExceptions)
        //});

            TestRunner runner = new TestRunner(new TestGroup[] { /*rsaExcept,*/ cspApi, cspExcept /*, oaepKeyApi, oaepKeyExcept, pkcsKeyApi, pkcsKeyExcept, pkcsSigApi, pkcsSigExcept*/ });

            // test should only run on XP+
            return runner.Run() ? MFTestResults.Pass : MFTestResults.Fail;
        }

        #region RSA Exception Tests

        /// <summary>
        ///		FromXmlString null
        /// </summary>
        //private static void RsaFromXmlStringNull()
        //{
        //    RSA rsa = new RSACryptoServiceProvider();
        //    rsa.FromXmlString(null);
        //    return;
        //}

        ///// <summary>
        /////		FromXmlString no modulus
        ///// </summary>
        //private static void RsaFromXmlStringNoMod()
        //{
        //    RSA rsa = new RSACryptoServiceProvider();
        //    rsa.FromXmlString(@"<RSAKeyValue><Exponent>" + Convert.ToBase64String(new byte[] { 0, 1, 2, 3, 4 }) + "</Exponent></RSAKeyValue>");
        //    return;
        //}

        ///// <summary>
        /////		FromXmlString no exponent
        ///// </summary>
        //private static void RsaFromXmlStringNoExp()
        //{
        //    RSA rsa = new RSACryptoServiceProvider();
        //    rsa.FromXmlString(@"<RSAKeyValue><Modulus>" + Convert.ToBase64String(new byte[] { 0, 1, 2, 3, 4 }) + "</Modulus></RSAKeyValue>");
        //    return;
        //}
        //#endregion

        //#region RSA CSP API Tests

        ///// <summary>
        /////		Check KeyExchangeAlgorithm
        ///// </summary>
        //private static bool CspCheckKeyExchange()
        //{
        //    RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
        //    bool defaultPassed = rsa.KeyExchangeAlgorithm == "RSA-PKCS1-KeyEx";

        //    CspParameters cspParams = new CspParameters();
        //    cspParams.KeyNumber = 2;

        //    rsa = new RSACryptoServiceProvider(cspParams);
        //    return defaultPassed && rsa.KeyExchangeAlgorithm == null;
        //}

        /// <summary>
        ///		Check SignatureAlgorithm
        /// </summary>
        //private static bool CspCheckSignatureAlgorithm()
        //{
        //    RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
        //    return rsa.SignatureAlgorithm == "http://www.w3.org/2000/09/xmldsig#rsa-sha1";
        //}

        /// <summary>
        ///		Check UseMachineStore
        /// </summary>
        //private static bool CspCheckUseMachineStore()
        //{
        //    bool init = RSACryptoServiceProvider.UseMachineKeyStore;

        //    RSACryptoServiceProvider.UseMachineKeyStore = true;
        //    bool ok = RSACryptoServiceProvider.UseMachineKeyStore;

        //    RSACryptoServiceProvider.UseMachineKeyStore = false;
        //    ok = ok && !RSACryptoServiceProvider.UseMachineKeyStore;

        //    RSACryptoServiceProvider.UseMachineKeyStore = init;
        //    return ok;
        //}

        /// <summary>
        ///		Check PersistKeys, then set true
        /// </summary>
        //private static bool CspCheckPersistKeys()
        //{
        //    RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
        //    bool ok = !rsa.PersistKeyInCsp;

        //    rsa.PersistKeyInCsp = true;
        //    return ok & rsa.PersistKeyInCsp;
        //}

        /// <summary>
        ///		Sign bytes
        /// </summary>
        private static bool CspSignBytes()
        {
            byte[] data = new byte[1025];
            for (int i = 2; i < data.Length; i++)
                data[i] = (byte)((data[i - 1] + data[i - 2]) % Byte.MaxValue);

            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            using (HashAlgorithm sha1 = new HashAlgorithm(HashAlgorithmType.SHA1))
            {

                byte[] sig = rsa.SignData(data, 25, 150, sha1);

                byte[] bufferPortion = new byte[150];
                Array.Copy(data, 25, bufferPortion, 0, 150);

                return rsa.VerifyData(bufferPortion, sha1, sig);
            }
        }

        /// <summary>
        ///		IsPublic on bad key
        /// </summary>
        //private static bool IsPublicBadKey()
        //{
        //    // format a fake key so that a full check must be made on it
        //    byte[] key = new byte[12];
        //    key[0] = 0x06;
        //    key[11] = 0x31;
        //    key[10] = 0x41;
        //    key[9] = 0x53;
        //    key[8] = 0x51; // should be 0x52 to get a result of true

        //    // this is a private method, who's only access point is to attempt to import a key
        //    // since we don't have a real key to use, reflection is the best bet at making sure
        //    // the correct result is achieved

        //    MethodInfo[] methods = typeof(RSACryptoServiceProvider).GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
        //    for (int i = 0; i < methods.Length; i++)
        //    {
        //        if (methods[i].Name == "IsPublic" &&
        //            methods[i].GetParameters().Length == 1 &&
        //            methods[i].GetParameters()[0].ParameterType == key.GetType())
        //            return !(bool)methods[i].Invoke(null, new object[] { key });
        //    }

        //    // this method no longer exists
        //    return true;
        //}

        /// <summary>
        ///		Sign with MD5
        /// </summary>
        private static bool SignMD5()
        {
            byte[] data = new byte[] { 0, 1, 2, 3, 4, 5 };
            using(RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            using (HashAlgorithm hasher = new HashAlgorithm(HashAlgorithmType.MD5))
            {
                byte[] hash = hasher.ComputeHash(data);
                byte[] sig = rsa.SignHash(hash, Cryptoki.MechanismType.MD5);

                return rsa.VerifyHash(hash, Cryptoki.MechanismType.MD5, sig);
            }
        }
        #endregion

        #region RSA CSP Exception Tests

        /// <summary>
        ///		Create negative key size
        /// </summary>
        private static void CspNegativeKey()
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(-1))
            {
            }
        }

        /// <summary>
        ///		Sign null hash
        /// </summary>
        private static void CspSignNullHash()
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.SignHash((byte[])null, Cryptoki.MechanismType.SHA_1);
            }
        }

        /// <summary>
        ///		Sign with only public key
        /// </summary>
        private static void CspSignPublic()
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.KeyPair = CryptoKey.CreateObject(rsa.Session, m_importKeyPublic) as CryptoKey;
                rsa.SignHash(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }, Cryptoki.MechanismType.SHA_1);
            }
            return;
        }

        /// <summary>
        ///		Verify null hash
        /// </summary>
        private static void CspVerifyNullHash()
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.VerifyHash((byte[])null, Cryptoki.MechanismType.SHA_1, new byte[] { 0, 1, 2, 3 });
            }
        }

        /// <summary>
        ///		Verify null signature
        /// </summary>
        private static void CspVerifyNullSig()
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.VerifyHash(new byte[] { 0, 1, 2, 3, 4, 5 }, Cryptoki.MechanismType.SHA_1, (byte[])null);
            }
        }

        /// <summary>
        ///		Decrypt null bytes
        /// </summary>
        private static void CspDecryptNullBytes()
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.Decrypt((byte[])null);
            }
        }

        /// <summary>
        ///		Decrypt wrong sized bytes
        /// </summary>
        private static void CspDecryptWrongSizeBytes()
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.Decrypt(new byte[] { 0 });
            }
        }

        /// <summary>
        ///		Decrypt value
        /// </summary>
        //private static void CspDecryptValue()
        //{
        //    using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
        //    {
        //        rsa.DecryptValue(new byte[] { 0, 1, 2, 3, 4, 5 });
        //    }
        //}

        ///// <summary>
        /////		Encrypt value
        ///// </summary>
        //private static void CspEncryptValue()
        //{
        //    using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
        //    {
        //        rsa.EncryptValue(new byte[] { 0, 1, 2, 3, 4, 5 });
        //    }
        //}

        /// <summary>
        ///		IsPublic on null key
        /// </summary>
        //private static void IsPublicNullKey()
        //{
        //    byte[] key = new byte[] { }; // used only for GetType()

        //    // this is a private method, who's only access point is to attempt to import a key
        //    // since we don't have a real key to use, reflection is the best bet at making sure
        //    // the correct result is achieved

        //    try
        //    {
        //        MethodInfo[] methods = typeof(RSACryptoServiceProvider).GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
        //        for (int i = 0; i < methods.Length; i++)
        //        {
        //            if (methods[i].Name == "IsPublic" &&
        //                methods[i].GetParameters().Length == 1 &&
        //                methods[i].GetParameters()[0].ParameterType == key.GetType())
        //                methods[i].Invoke(null, new object[] { null });
        //        }

        //    }
        //    catch (TargetInvocationException e)
        //    {
        //        if (e.InnerException != null)
        //            throw e.InnerException;
        //        else
        //            throw;
        //    }

        //    // this method no longer exists
        //    return;
        //}
        #endregion

        #region OAEP Key Formatting API Tests
        /// <summary>
        ///		Check Parameters
        /// </summary>
        //private static bool OaepKeyParams()
        //{
        //    RSAOAEPKeyExchangeDeformatter deformat = new RSAOAEPKeyExchangeDeformatter(new RSACryptoServiceProvider());
        //    RSAOAEPKeyExchangeFormatter format = new RSAOAEPKeyExchangeFormatter(new RSACryptoServiceProvider());

        //    deformat.Parameters = "Noop";

        //    return deformat.Parameters == null && format.Parameters == null;
        //}

        ///// <summary>
        /////		Round trip, RSA CSP
        ///// </summary>
        //private static bool OaepKeyCSPRT()
        //{
        //    RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
        //    RSAOAEPKeyExchangeDeformatter deformat = new RSAOAEPKeyExchangeDeformatter(rsa);
        //    RSAOAEPKeyExchangeFormatter format = new RSAOAEPKeyExchangeFormatter(rsa);
        //    RijndaelManaged key = new RijndaelManaged();

        //    byte[] formatted = format.CreateKeyExchange(key.Key, key.GetType());
        //    byte[] rt = deformat.DecryptKeyExchange(formatted);
        //    return Util.CompareBytes(rt, key.Key);
        //}

        ///// <summary>
        /////		Round trip, non-CSP
        ///// </summary>
        //private static bool OaepKeyNotCSPRT()
        //{
        //    RSA rsa = new MyRSAManaged();
        //    RSAOAEPKeyExchangeDeformatter deformat = new RSAOAEPKeyExchangeDeformatter(rsa);
        //    RSAOAEPKeyExchangeFormatter format = new RSAOAEPKeyExchangeFormatter(rsa);
        //    RijndaelManaged key = new RijndaelManaged();

        //    byte[] formatted = format.CreateKeyExchange(key.Key, key.GetType());
        //    byte[] rt = deformat.DecryptKeyExchange(formatted);
        //    return Util.CompareBytes(rt, key.Key);
        //}

        ///// <summary>
        /////		Set parameter null
        ///// </summary>
        //private static bool OaepKeyParameterNull()
        //{
        //    RSAOAEPKeyExchangeFormatter format = new RSAOAEPKeyExchangeFormatter(new RSACryptoServiceProvider());
        //    format.Parameter = null;
        //    return format.Parameter == null;
        //}

        ///// <summary>
        /////		Set parameters not-null
        ///// </summary>
        //private static bool OaepKeyParameterNotNull()
        //{
        //    byte[] parameter = new byte[] { 0, 1, 2, 3 };
        //    RSAOAEPKeyExchangeFormatter format = new RSAOAEPKeyExchangeFormatter(new RSACryptoServiceProvider());
        //    format.Parameter = parameter;
        //    return Util.CompareBytes(parameter, format.Parameter);
        //}

        ///// <summary>
        /////		Set RNG
        ///// </summary>
        //private static bool OaepKeySetRng()
        //{
        //    RandomNumberGenerator rng = new RNGCryptoServiceProvider();
        //    RSAOAEPKeyExchangeFormatter format = new RSAOAEPKeyExchangeFormatter(new RSACryptoServiceProvider());
        //    format.Rng = rng;

        //    return format.Rng == rng;
        //}
        //#endregion

        //#region OAEP Key Formatting Exception Tests

        ///// <summary>
        /////		Deformat null
        ///// </summary>
        //private static void OaepKeyDeformatNull()
        //{
        //    RSAOAEPKeyExchangeDeformatter deformat = new RSAOAEPKeyExchangeDeformatter(new RSACryptoServiceProvider());
        //    deformat.DecryptKeyExchange(null);
        //    return;
        //}

        ///// <summary>
        /////		Deformat bad key
        ///// </summary>
        //private static void OaepKeyDeformatBad()
        //{
        //    RSAOAEPKeyExchangeDeformatter deformat = new RSAOAEPKeyExchangeDeformatter(new MyRSAManaged());
        //    deformat.DecryptKeyExchange(new byte[] { 0 });
        //    return;
        //}

        ///// <summary>
        /////		Deformat null key
        ///// </summary>
        //private static void OaepKeyDeformatSetNull()
        //{
        //    RSAOAEPKeyExchangeDeformatter deformat = new RSAOAEPKeyExchangeDeformatter(new RSACryptoServiceProvider());
        //    deformat.SetKey(null);
        //    return;
        //}

        ///// <summary>
        /////		Format set null key
        ///// </summary>
        //private static void OaepKeyFormatSetNull()
        //{
        //    RSAOAEPKeyExchangeFormatter format = new RSAOAEPKeyExchangeFormatter(new RSACryptoServiceProvider());
        //    format.SetKey(null);
        //    return;
        //}

        ///// <summary>
        /////		Format a null key
        ///// </summary>
        //private static void OaepKeyFormatNull()
        //{
        //    RSAOAEPKeyExchangeFormatter format = new RSAOAEPKeyExchangeFormatter(new RSACryptoServiceProvider());
        //    format.CreateKeyExchange(null, typeof(RijndaelManaged));
        //    return;
        //}
        #endregion

        #region PKCS1 Key Formatting API Tests

        /// <summary>
        ///		Set RNG
        /// </summary>
        //private static bool PkcsKeySetRng()
        //{
        //    RSAPKCS1KeyExchangeFormatter formatter = new RSAPKCS1KeyExchangeFormatter(new RSACryptoServiceProvider());
        //    RSAPKCS1KeyExchangeDeformatter deformatter = new RSAPKCS1KeyExchangeDeformatter(new RSACryptoServiceProvider());
        //    RandomNumberGenerator rng = new RNGCryptoServiceProvider();

        //    formatter.Rng = rng;
        //    deformatter.RNG = rng;

        //    return formatter.Rng == rng && deformatter.RNG == rng;
        //}

        ///// <summary>
        /////		Set Parameters
        ///// </summary>
        //private static bool PkcsKeySetParams()
        //{
        //    RSAPKCS1KeyExchangeDeformatter deformatter = new RSAPKCS1KeyExchangeDeformatter(new RSACryptoServiceProvider());
        //    deformatter.Parameters = "Noop";

        //    return deformatter.Parameters == null;
        //}

        ///// <summary>
        /////		Round trip, RSA CSP
        ///// </summary>
        //private static bool PkcsKeyCSPRT()
        //{
        //    RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
        //    RSAPKCS1KeyExchangeFormatter formatter = new RSAPKCS1KeyExchangeFormatter(rsa);
        //    RSAPKCS1KeyExchangeDeformatter deformatter = new RSAPKCS1KeyExchangeDeformatter(rsa);
        //    RijndaelManaged key = new RijndaelManaged();

        //    byte[] formatted = formatter.CreateKeyExchange(key.Key, key.GetType());
        //    byte[] deformatted = deformatter.DecryptKeyExchange(formatted);

        //    return Util.CompareBytes(key.Key, deformatted);
        //}

        ///// <summary>
        /////		Round trip, non-CSP
        ///// </summary>
        //private static bool PkcsKeyNotCSPRT()
        //{
        //    return true;
        //}

        ///// <summary>
        /////		Check formatter parameters
        ///// </summary>
        //private static bool PkcsKeyCheckFormatParams()
        //{
        //    RSA rsa = new MyRSAManaged();
        //    RSAPKCS1KeyExchangeFormatter formatter = new RSAPKCS1KeyExchangeFormatter(rsa);
        //    RSAPKCS1KeyExchangeDeformatter deformatter = new RSAPKCS1KeyExchangeDeformatter(rsa);
        //    RijndaelManaged key = new RijndaelManaged();

        //    byte[] formatted = formatter.CreateKeyExchange(key.Key, key.GetType());
        //    byte[] deformatted = deformatter.DecryptKeyExchange(formatted);

        //    return Util.CompareBytes(key.Key, deformatted);
        //}
        #endregion

        #region PKCS Key Formatting Exception Tests

        /// <summary>
        ///		Deformat null
        /// </summary>
        //private static void PkcsKeyDeformatNull()
        //{
        //    RSAPKCS1KeyExchangeDeformatter deformatter = new RSAPKCS1KeyExchangeDeformatter(new RSACryptoServiceProvider());
        //    deformatter.DecryptKeyExchange((byte[])null);
        //    return;
        //}

        ///// <summary>
        /////		Deformat set null key
        ///// </summary>
        //private static void PkcsKeySetNullKey()
        //{
        //    RSAPKCS1KeyExchangeFormatter formatter = new RSAPKCS1KeyExchangeFormatter(new RSACryptoServiceProvider());
        //    formatter.SetKey(null);
        //    return;
        //}

        ///// <summary>
        /////		Format set null key
        ///// </summary>
        //private static void PkcsKeyFormatSetNullKey()
        //{
        //    RSAPKCS1KeyExchangeDeformatter deformatter = new RSAPKCS1KeyExchangeDeformatter(new RSACryptoServiceProvider());
        //    deformatter.SetKey(null);
        //    return;
        //}
        ///// <summary>
        /////		Format null key
        ///// </summary>
        //private static void PkcsKeyFormatNull()
        //{
        //    RSAPKCS1KeyExchangeFormatter formatter = new RSAPKCS1KeyExchangeFormatter(new RSACryptoServiceProvider());
        //    formatter.CreateKeyExchange((byte[])null, typeof(RijndaelManaged));
        //    return;
        //}
        #endregion

        #region PKCS1 Signature Formatting API Tests

        /// <summary>
        ///		Round trip, RSA CSP
        /// </summary>
        //private static bool PkcsSigCSPRT()
        //{
        //    byte[] data = new byte[1025];
        //    for (int i = 2; i < data.Length; i++)
        //        data[i] = (byte)((data[i - 1] + data[i - 2]) % Byte.MaxValue);

        //    RSA rsa = new RSACryptoServiceProvider();
        //    RSAPKCS1SignatureFormatter formatter = new RSAPKCS1SignatureFormatter(rsa);
        //    RSAPKCS1SignatureDeformatter deformatter = new RSAPKCS1SignatureDeformatter(rsa);

        //    SHA1 hasher = new SHA1CryptoServiceProvider();
        //    formatter.SetHashAlgorithm("SHA1");
        //    deformatter.SetHashAlgorithm("SHA1");

        //    byte[] hash = hasher.ComputeHash(data);
        //    byte[] sig = formatter.CreateSignature(hash);

        //    return deformatter.VerifySignature(hash, sig);
        //}

        ///// <summary>
        /////		Round trip, non-CSP
        ///// </summary>
        //private static bool PkcsSigNotCSPRT()
        //{
        //    byte[] data = new byte[1025];
        //    for (int i = 2; i < data.Length; i++)
        //        data[i] = (byte)((data[i - 1] + data[i - 2]) % Byte.MaxValue);

        //    RSA rsa = new MyRSAManaged();
        //    RSAPKCS1SignatureFormatter formatter = new RSAPKCS1SignatureFormatter(rsa);
        //    RSAPKCS1SignatureDeformatter deformatter = new RSAPKCS1SignatureDeformatter(rsa);

        //    SHA1 hasher = new SHA1CryptoServiceProvider();
        //    formatter.SetHashAlgorithm("SHA1");
        //    deformatter.SetHashAlgorithm("SHA1");

        //    byte[] hash = hasher.ComputeHash(data);
        //    byte[] sig = formatter.CreateSignature(hash);

        //    return deformatter.VerifySignature(hash, sig);
        //}
        //#endregion

        //#region PKCS1 Signature Formatting Exception Tests

        ///// <summary>
        /////		Deformat set null key
        ///// </summary>
        //private static void PkcsSigDeformatSetNullKey()
        //{
        //    RSAPKCS1SignatureDeformatter deformatter = new RSAPKCS1SignatureDeformatter(new RSACryptoServiceProvider());
        //    deformatter.SetKey(null);
        //    return;
        //}

        ///// <summary>
        /////		Verify null key
        ///// </summary>
        //private static void PkcsSigVerifyNullKey()
        //{
        //    RSAPKCS1SignatureDeformatter deformatter = new RSAPKCS1SignatureDeformatter();
        //    deformatter.SetHashAlgorithm("SHA1");
        //    deformatter.VerifySignature(new byte[] { 0, 1, 2, 3, 4, 5 }, new byte[] { 0, 1, 2, 3, 4, 5 });
        //    return;
        //}

        ///// <summary>
        /////		Verify null oid
        ///// </summary>
        //private static void PkcsSigVerifyNullOid()
        //{
        //    RSAPKCS1SignatureDeformatter deformatter = new RSAPKCS1SignatureDeformatter(new RSACryptoServiceProvider());
        //    deformatter.VerifySignature(new byte[] { 0, 1, 2, 3, 4, 5 }, new byte[] { 0, 1, 2, 3, 4, 5 });
        //    return;
        //}

        ///// <summary>
        /////		Verify null hash
        ///// </summary>
        //private static void PkcsSigVerifyNullHash()
        //{
        //    RSAPKCS1SignatureDeformatter deformatter = new RSAPKCS1SignatureDeformatter(new RSACryptoServiceProvider());
        //    deformatter.SetHashAlgorithm("SHA1");
        //    deformatter.VerifySignature((byte[])null, new byte[] { 0, 1, 2, 3, 4, 5 });
        //    return;
        //}

        ///// <summary>
        /////		Verify null signature
        ///// </summary>
        //private static void PkcsSigVerifyNullSig()
        //{
        //    RSAPKCS1SignatureDeformatter deformatter = new RSAPKCS1SignatureDeformatter(new RSACryptoServiceProvider());
        //    deformatter.SetHashAlgorithm("SHA1");
        //    deformatter.VerifySignature(new byte[] { 0, 1, 2, 3, 4, 5 }, (byte[])null);
        //    return;
        //}

        ///// <summary>
        /////		Format set null key
        ///// </summary>
        //private static void PkcsSigFormatSetNullKey()
        //{
        //    RSAPKCS1SignatureFormatter formatter = new RSAPKCS1SignatureFormatter(new RSACryptoServiceProvider());
        //    formatter.SetKey(null);
        //    return;
        //}

        ///// <summary>
        /////		Sign null oid
        ///// </summary>
        //private static void PkcsSigSignNullOid()
        //{
        //    RSAPKCS1SignatureFormatter formatter = new RSAPKCS1SignatureFormatter(new RSACryptoServiceProvider());
        //    formatter.CreateSignature(new byte[] { 0, 1, 2, 3, 4, 5 });
        //    return;
        //}

        ///// <summary>
        /////		Sign null key
        ///// </summary>
        //private static void PkcsSigSignNullKey()
        //{
        //    RSAPKCS1SignatureFormatter formatter = new RSAPKCS1SignatureFormatter();
        //    formatter.SetHashAlgorithm("SHA1");
        //    formatter.CreateSignature(new byte[] { 0, 1, 2, 3, 4, 5 });
        //    return;
        //}

        ///// <summary>
        /////		Sign null hash
        ///// </summary>
        //private static void PkcsSigSignNullHash()
        //{
        //    RSAPKCS1SignatureFormatter formatter = new RSAPKCS1SignatureFormatter(new RSACryptoServiceProvider());
        //    formatter.SetHashAlgorithm("SHA1");
        //    formatter.CreateSignature((byte[])null);
        //    return;
        //}
        #endregion

        internal static CryptokiAttribute[] m_importKeyPublic = new CryptokiAttribute[]
                    {
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.Class  , Utility.ConvertToBytes((int)CryptokiClass.PUBLIC_KEY)),
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.KeyType, Utility.ConvertToBytes((int)CryptoKey.KeyType.RSA)),
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.Modulus, new byte[]
                            {
                                0xC6, 0x29, 0x73, 0xE3, 0xC8, 0xD4, 0xFC, 0xB6, 0x89, 0x36, 0x46, 0xF9, 0x58, 0xE5, 0xF5, 0xE5, 
                                0x25, 0xC2, 0xE4, 0x1E, 0xCC, 0xA8, 0xC3, 0xEF, 0xA2, 0x8D, 0x24, 0xDE, 0xFD, 0x19, 0xDA, 0x08, 
                                0x46, 0x9A, 0xA9, 0xBA, 0xAE, 0x77, 0x20, 0x28, 0xED, 0x51, 0x43, 0x8C, 0x28, 0x6F, 0x99, 0x5B, 
                                0x6B, 0x0C, 0x08, 0x7C, 0x4C, 0x7D, 0x6F, 0xCF, 0xD0, 0xF0, 0xAC, 0x2A, 0x9B, 0x28, 0x28, 0x62, 
                                0x52, 0x3F, 0x56, 0x3B, 0x6F, 0x49, 0x10, 0x11, 0x48, 0x45, 0x36, 0x51, 0x62, 0xAE, 0x8C, 0x66, 
                                0xE8, 0x53, 0x8D, 0x18, 0xDF, 0x21, 0x12, 0x30, 0x35, 0x79, 0xAD, 0x41, 0x0F, 0xED, 0x50, 0x41, 
                                0x26, 0xC3, 0x3E, 0xFE, 0x88, 0xEB, 0xA8, 0x7C, 0xF2, 0x48, 0x13, 0x84, 0x27, 0xCE, 0x19, 0x86, 
                                0x33, 0x14, 0x89, 0xEB, 0x7A, 0x90, 0x21, 0x46, 0x5C, 0xC2, 0x22, 0x23, 0x96, 0x06, 0x85, 0xF7,  
                            }),
                        new CryptokiAttribute(CryptokiAttribute.CryptokiType.PublicExponent, new byte[]
                            {
                                 0x01, 0x00, 0x01
                            }),
                    };

    }
}