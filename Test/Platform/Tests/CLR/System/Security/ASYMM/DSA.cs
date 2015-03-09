using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Cryptoki;
//using System.Xml;

/// <remarks>
///		Note, these tests were created to fill coverage holes, and are not a full suite for DSA
///		Check other DSA* tests in this folder for more DSA testing
/// </remarks>
namespace Microsoft.SPOT.Platform.Tests
{
    public class DSADesktopTest : IMFTestInterface
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
        public MFTestResults DSA_Test()
        {
            Exception[] cryptoEx = new Exception[] { new CryptographicException(), new CryptographicUnexpectedOperationException() };

            //TestGroup dsaExcept = new TestGroup("DSA Exception Tests", new TestCase[] {
            //    new ExceptionTestCase("FromXml null", new ExceptionTestCaseImpl(FromXmlNull), ExceptionTestCase.ArgumentExceptions),
            //    new ExceptionTestCase("FromXml no P", new ExceptionTestCaseImpl(FromXmlNoP), cryptoEx),
            //    new ExceptionTestCase("FromXml no Q", new ExceptionTestCaseImpl(FromXmlNoQ), cryptoEx),
            //    new ExceptionTestCase("FromXml no G", new ExceptionTestCaseImpl(FromXmlNoG), cryptoEx),
            //    new ExceptionTestCase("FromXml no Y", new ExceptionTestCaseImpl(FromXmlNoY), cryptoEx),
            //    new ExceptionTestCase("FromXml seed, no pgencounter", new ExceptionTestCaseImpl(FromXmlSeedNoPgenCounter), cryptoEx),
            //    new ExceptionTestCase("FromXml pgencounter, no seed", new ExceptionTestCaseImpl(FromXmlNoSeedPgenCounter), cryptoEx)
            //});

            TestGroup dsaCspApi = new TestGroup("DSA CSP API Tests", new TestCase[] {
                //new TestCase("Create with CspParameters", new TestCaseImpl(CSPParameters)),
                //new TestCase("Check KeyExchangeAlgorithm", new TestCaseImpl(CSPCheckKeyExchangeAlg)),
                //new TestCase("Check SignatureAlgorithm", new TestCaseImpl(CSPCheckSignatureAlg)),
                //new TestCase("Check MachineStore", new TestCaseImpl(CSPCheckMachineStore)),
			    new TestCase("Sign stream", new TestCaseImpl(CSPSignStream)),
			    new TestCase("Sign buffer", new TestCaseImpl(CSPSignBuffer)),
			    //new TestCase("IsPublic for incorrectly formatted key", new TestCaseImpl(CSPIsPublicBadKey)),

		    });

            TestGroup dsaCspExcept = new TestGroup("DSA CSP Exception Tests", new TestCase[] {
			    new ExceptionTestCase("Create with negative keysize", new ExceptionTestCaseImpl(CSPNegativeKeySize), ExceptionTestCase.ArgumentExceptions),
			    new ExceptionTestCase("Clear, then try to use", new ExceptionTestCaseImpl(CSPClearUse), new Exception[] { new ObjectDisposedException("a") }),
			    new ExceptionTestCase("Sign null hash", new ExceptionTestCaseImpl(CSPSignNullHash), ExceptionTestCase.ArgumentExceptions),
			    new ExceptionTestCase("Sign without private key", new ExceptionTestCaseImpl(CSPSignNoPrivate), cryptoEx),
			    new ExceptionTestCase("Sign with wrong hash count", new ExceptionTestCaseImpl(CSPSignBadHashCount), cryptoEx),
			    new ExceptionTestCase("Verify null hash", new ExceptionTestCaseImpl(CSPVerifyNullHash), ExceptionTestCase.NullExceptions),
			    new ExceptionTestCase("Verify null signature", new ExceptionTestCaseImpl(CSPVerifyNullSig), ExceptionTestCase.NullExceptions),
			    new ExceptionTestCase("Verify signature of bad hash", new ExceptionTestCaseImpl(CSPVerifyBadHash), cryptoEx),
			    //new ExceptionTestCase("Check to see if the null key is public", new ExceptionTestCaseImpl(IsPublicNull), ExceptionTestCase.NullExceptions)
		    });

            //TestGroup dsaFormatExcept = new TestGroup("DSA Signature Formatter Exception Tests", new TestCase[]
            //{
            //    new ExceptionTestCase("Set null key", new ExceptionTestCaseImpl(FormatterSetKeyNull), ExceptionTestCase.NullExceptions),
            //    new ExceptionTestCase("Set non SHA1 algorithm", new ExceptionTestCaseImpl(FormatterNonSHA1), cryptoEx),
            //    new ExceptionTestCase("Sign null key", new ExceptionTestCaseImpl(FormatterSignNullKey), cryptoEx),
            //    new ExceptionTestCase("Sign null data", new ExceptionTestCaseImpl(FormatterSignNullHash), ExceptionTestCase.NullExceptions)
            //});

            //TestGroup dsaDeformatExcept = new TestGroup("DSA Signature Deformatter Exception Tests", new TestCase[]
            //{
            //    new ExceptionTestCase("Set null key", new ExceptionTestCaseImpl(DeformatterSetKeyNull), ExceptionTestCase.NullExceptions),
            //    new ExceptionTestCase("Set non SHA1 algorithm", new ExceptionTestCaseImpl(DeformatterNonSHA1), cryptoEx),
            //    new ExceptionTestCase("Verify null key", new ExceptionTestCaseImpl(DeformatterVerifyNullKey), cryptoEx),
            //    new ExceptionTestCase("Verify null hash", new ExceptionTestCaseImpl(DeformatterVerifyNullHash), ExceptionTestCase.NullExceptions),
            //    new ExceptionTestCase("Verify null signature", new ExceptionTestCaseImpl(DeformatterVerifyNullSig), ExceptionTestCase.NullExceptions)
            //});

            TestRunner runner = new TestRunner(new TestGroup[] { /*dsaExcept,*/ dsaCspApi, dsaCspExcept /*, dsaFormatExcept, dsaDeformatExcept*/ });
            return runner.Run() ? MFTestResults.Pass : MFTestResults.Fail;
        }

        #region DSA Exception Tests

        ///// <summary>
        /////		FromXml null
        ///// </summary>
        //private static void FromXmlNull()
        //{
        //    DSA alg = DSA.Create();
        //    alg.FromXmlString(null);
        //    return;
        //}

        ///// <summary>
        /////		FromXml no P
        ///// </summary>
        //private static void FromXmlNoP()
        //{
        //    XmlDocument doc = new XmlDocument();
        //    DSA alg = DSA.Create();
        //    doc.LoadXml(alg.ToXmlString(true));

        //    XmlElement remElem = doc.SelectSingleNode("/DSAKeyValue/P") as XmlElement;
        //    remElem.ParentNode.RemoveChild(remElem);

        //    alg.FromXmlString(doc.DocumentElement.OuterXml);
        //    return;
        //}

        ///// <summary>
        /////		FromXml no Q
        ///// </summary>
        //private static void FromXmlNoQ()
        //{
        //    XmlDocument doc = new XmlDocument();
        //    DSA alg = DSA.Create();
        //    doc.LoadXml(alg.ToXmlString(true));

        //    XmlElement remElem = doc.SelectSingleNode("/DSAKeyValue/Q") as XmlElement;
        //    remElem.ParentNode.RemoveChild(remElem);

        //    alg.FromXmlString(doc.DocumentElement.OuterXml);
        //    return;
        //}

        ///// <summary>
        /////		FromXml no G
        ///// </summary>
        //private static void FromXmlNoG()
        //{
        //    XmlDocument doc = new XmlDocument();
        //    DSA alg = DSA.Create();
        //    doc.LoadXml(alg.ToXmlString(true));

        //    XmlElement remElem = doc.SelectSingleNode("/DSAKeyValue/G") as XmlElement;
        //    remElem.ParentNode.RemoveChild(remElem);

        //    alg.FromXmlString(doc.DocumentElement.OuterXml);
        //    return;
        //}

        ///// <summary>
        /////		FromXml no Y
        ///// </summary>
        //private static void FromXmlNoY()
        //{
        //    XmlDocument doc = new XmlDocument();
        //    DSA alg = DSA.Create();
        //    doc.LoadXml(alg.ToXmlString(true));

        //    XmlElement remElem = doc.SelectSingleNode("/DSAKeyValue/Y") as XmlElement;
        //    remElem.ParentNode.RemoveChild(remElem);

        //    alg.FromXmlString(doc.DocumentElement.OuterXml);
        //    return;
        //}

        ///// <summary>
        /////		FromXml seed, no pgencounter
        ///// </summary>
        //private static void FromXmlSeedNoPgenCounter()
        //{
        //    XmlDocument doc = new XmlDocument();
        //    DSA alg = DSA.Create();
        //    doc.LoadXml(alg.ToXmlString(true));

        //    XmlElement remElem = doc.SelectSingleNode("/DSAKeyValue/PgenCounter") as XmlElement;
        //    remElem.ParentNode.RemoveChild(remElem);

        //    alg.FromXmlString(doc.DocumentElement.OuterXml);
        //    return;
        //}

        ///// <summary>
        /////		FromXml pgencounter, no seed
        ///// </summary>
        //private static void FromXmlNoSeedPgenCounter()
        //{
        //    XmlDocument doc = new XmlDocument();
        //    DSA alg = DSA.Create();
        //    doc.LoadXml(alg.ToXmlString(true));

        //    XmlElement remElem = doc.SelectSingleNode("/DSAKeyValue/Seed") as XmlElement;
        //    remElem.ParentNode.RemoveChild(remElem);

        //    alg.FromXmlString(doc.DocumentElement.OuterXml);
        //    return;
        //}
        #endregion

        #region DSA CSP API Tests
        /// <summary>
        ///		Create with CspParameters
        /// </summary>
        //private static bool CSPParameters()
        //{
        //    byte[] data = new byte[] { 0, 1, 2, 3, 4, 5 };

        //    CspParameters csp = new CspParameters(13);
        //    csp.KeyContainerName = "Custom Key Container";
        //    csp.KeyNumber = 2;

        //    DSACryptoServiceProvider dsaCsp = new DSACryptoServiceProvider(csp);
        //    byte[] signed = dsaCsp.SignData(data);
        //    bool passed = dsaCsp.VerifyData(data, signed);
        //    dsaCsp.Clear();

        //    return passed;
        //}

        ///// <summary>
        /////		Check KeyExchangeAlgorithm
        ///// </summary>
        //private static bool CSPCheckKeyExchangeAlg()
        //{
        //    return new DSACryptoServiceProvider().KeyExchangeAlgorithm == null;
        //}

        ///// <summary>
        /////		Check SignatureAlgorithm
        ///// </summary>
        //private static bool CSPCheckSignatureAlg()
        //{
        //    return new DSACryptoServiceProvider().SignatureAlgorithm == "http://www.w3.org/2000/09/xmldsig#dsa-sha1";
        //}

        ///// <summary>
        /////		Check MachineStore
        ///// </summary>
        //private static bool CSPCheckMachineStore()
        //{
        //    bool init = DSACryptoServiceProvider.UseMachineKeyStore;

        //    DSACryptoServiceProvider.UseMachineKeyStore = true;
        //    bool ok = DSACryptoServiceProvider.UseMachineKeyStore;

        //    DSACryptoServiceProvider.UseMachineKeyStore = false;
        //    ok = ok && !DSACryptoServiceProvider.UseMachineKeyStore;

        //    DSACryptoServiceProvider.UseMachineKeyStore = init;
        //    return ok;
        //}

        /// <summary>
        ///		Sign stream
        /// </summary>
        private static bool CSPSignStream()
        {
            byte[] data = new byte[1025];
            for (int i = 2; i < data.Length; i++)
                data[i] = (byte)((data[i - 1] + data[i - 2]) % Byte.MaxValue);

            using (DSACryptoServiceProvider dsa = new DSACryptoServiceProvider())
            using (MemoryStream ms = new MemoryStream(data))
            {
                byte[] streamSig = dsa.SignData(ms);

                return dsa.VerifyData(data, streamSig);
            }
        }

        /// <summary>
        ///		Sign buffer
        /// </summary>
        private static bool CSPSignBuffer()
        {
            byte[] data = new byte[1025];
            for (int i = 2; i < data.Length; i++)
                data[i] = (byte)((data[i - 1] + data[i - 2]) % Byte.MaxValue);

            using (DSACryptoServiceProvider dsa = new DSACryptoServiceProvider())
            {
                byte[] sig = dsa.SignData(data, 25, 150);

                byte[] bufferPortion = new byte[150];
                Array.Copy(data, 25, bufferPortion, 0, 150);

                return dsa.VerifyData(bufferPortion, sig);
            }
        }

        /// <summary>
        ///		IsPublic for incorrectly formatted key
        /// </summary>
        //private static bool CSPIsPublicBadKey()
        //{
        //    // format a fake key so that a full check must be made on it
        //    byte[] key = new byte[12];
        //    key[0] = 0x06;
        //    key[11] = 0x31;
        //    key[10] = 0x53;
        //    key[9] = 0x53;
        //    key[8] = 0x45; // should be 0x44 to get a result of true

        //    // this is a private method, who's only access point is to attempt to import a key
        //    // since we don't have a real key to use, reflection is the best bet at making sure
        //    // the correct result is achieved

        //    MethodInfo[] methods = typeof(DSACryptoServiceProvider).GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
        //    for (int i = 0; i < methods.Length; i++)
        //    {
        //        if (methods[i].Name == "IsPublic") // &&
        //            //methods[i].GetParameters().Length == 1 &&
        //            //methods[i].GetParameters()[0].ParameterType == key.GetType())
        //            return !(bool)methods[i].Invoke(null, new object[] { key });
        //    }

        //    // this method no longer exists
        //    return true;
        //}
        #endregion

        #region DSA CSP Exception Tests
        /// <summary>
        ///		Create with negative keysize
        /// </summary>
        private static void CSPNegativeKeySize()
        {
            using (DSACryptoServiceProvider dsa = new DSACryptoServiceProvider(-1))
            {
            }
        }

        /// <summary>
        ///		Clear, then try to use
        /// </summary>
        private static void CSPClearUse()
        {
            using (DSACryptoServiceProvider dsa = new DSACryptoServiceProvider())
            {
                dsa.SignData(new byte[] { 0, 1, 2, 3, 4, 5 });
                dsa.Clear();
                dsa.SignData(new byte[] { 0, 1, 2, 3, 4, 5 });
            }
        }

        /// <summary>
        ///		Sign null hash
        /// </summary>
        private static void CSPSignNullHash()
        {
            using (DSACryptoServiceProvider dsa = new DSACryptoServiceProvider())
            {
                dsa.SignHash(null, MechanismType.SHA_1);
            }
        }

        /// <summary>
        ///		Sign without private key
        /// </summary>
        private static void CSPSignNoPrivate()
        {
            //DSACryptoServiceProvider dsaPriv = new DSACryptoServiceProvider();
            //byte[] publicBlob = dsaPriv.ExportCspBlob(false);

            using (Session session = new Session("", MechanismType.DSA))
            {
                CryptoKey key = CryptoKey.CreateObject(session, m_publicDsaKey) as CryptoKey;

                using (DSACryptoServiceProvider dsa = new DSACryptoServiceProvider(key))
                {
                    //dsa.ImportCspBlob(publicBlob);

                    dsa.SignData(new byte[] { 0, 1, 2, 3, 4, 5 });
                }
            }
        }

        /// <summary>
        ///		Sign with wrong hash count
        /// </summary>
        private static void CSPSignBadHashCount()
        {
            using (DSACryptoServiceProvider dsa = new DSACryptoServiceProvider())
            {
                dsa.SignHash(new byte[0], MechanismType.SHA_1); // "SHA1");
            }
        }

        /// <summary>
        ///		Verify null hash
        /// </summary>
        private static void CSPVerifyNullHash()
        {
            using (DSACryptoServiceProvider dsa = new DSACryptoServiceProvider())
            {
                dsa.VerifyHash(null, MechanismType.SHA_1, new byte[] { 0, 1, 2 });
            }
        }

        /// <summary>
        ///		Verify null signature
        /// </summary>
        private static void CSPVerifyNullSig()
        {
            using (DSACryptoServiceProvider dsa = new DSACryptoServiceProvider())
            {
                dsa.VerifyHash(new byte[] { 0, 1, 2, 3, 4, 5 }, MechanismType.SHA_1, null);
            }
        }

        /// <summary>
        ///		Verify signature of bad hash
        /// </summary>
        private static void CSPVerifyBadHash()
        {
            using (DSACryptoServiceProvider dsa = new DSACryptoServiceProvider())
            {
                dsa.VerifyHash(new byte[0], MechanismType.SHA_1, new byte[] { 0, 1, 2, 3, 4, 5 });
            }
        }

        /// <summary>
        /// 	Check to see if the null key is public
        /// </summary>
        //private static void IsPublicNull()
        //{
        //    byte[] key = new byte[] { }; // used for GetType only

        //    // this is a private method, who's only access point is to attempt to import a key
        //    // since we don't have a real key to use, reflection is the best bet at making sure
        //    // the correct result is achieved

        //    try
        //    {
        //        MethodInfo[] methods = typeof(DSACryptoServiceProvider).GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
        //        for (int i = 0; i < methods.Length; i++)
        //        {
        //            if (methods[i].Name == "IsPublic" )// &&
        //                //methods[i].GetParameters().Length == 1 &&
        //                //methods[i].GetParameters()[0].ParameterType == key.GetType())
        //                methods[i].Invoke(null, new object[] { null });
        //        }
        //    }
        //    catch (Exception e)
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

        #region DSA Signature Formatter Exception Tests

        /// <summary>
        ///		Set null key
        /// </summary>
        //private static void FormatterSetKeyNull()
        //{
        //    DSASignatureFormatter formatter = new DSASignatureFormatter();
        //    formatter.SetKey(null);
        //    return;
        //}

        ///// <summary>
        /////		Set non SHA1 algorithm
        ///// </summary>
        //private static void FormatterNonSHA1()
        //{
        //    DSASignatureFormatter formatter = new DSASignatureFormatter();
        //    formatter.SetHashAlgorithm("SHA256");
        //    return;
        //}

        ///// <summary>
        /////		Sign null key
        ///// </summary>
        //private static void FormatterSignNullKey()
        //{
        //    DSASignatureFormatter formatter = new DSASignatureFormatter();
        //    formatter.CreateSignature(new byte[] { 0, 1, 2, 3 });
        //    return;
        //}

        ///// <summary>
        /////		Sign null data
        ///// </summary>
        //private static void FormatterSignNullHash()
        //{
        //    DSASignatureFormatter formatter = new DSASignatureFormatter(new DSACryptoServiceProvider());
        //    formatter.CreateSignature((byte[])null);
        //    return;
        //}
        #endregion

        #region DSA Signature Deformatter Exception Tests

        /// <summary>
        ///		Set null key
        /// </summary>
        //private static void DeformatterSetKeyNull()
        //{
        //    DSASignatureDeformatter deformatter = new DSASignatureDeformatter();
        //    deformatter.SetKey(null);
        //    return;
        //}

        ///// <summary>
        /////		Set non SHA1 algorithm
        ///// </summary>
        //private static void DeformatterNonSHA1()
        //{
        //    DSASignatureDeformatter deformatter = new DSASignatureDeformatter();
        //    deformatter.SetHashAlgorithm("SHA512");
        //    return;
        //}

        ///// <summary>
        /////		Verify null key
        ///// </summary>
        //private static void DeformatterVerifyNullKey()
        //{
        //    DSASignatureDeformatter deformatter = new DSASignatureDeformatter();
        //    deformatter.VerifySignature(new byte[] { 0, 1, 2 }, new byte[] { 0, 1, 2 });
        //    return;
        //}

        ///// <summary>
        /////		Verify null hash
        ///// </summary>
        //private static void DeformatterVerifyNullHash()
        //{
        //    DSASignatureDeformatter deformatter = new DSASignatureDeformatter(new DSACryptoServiceProvider());
        //    deformatter.VerifySignature((byte[])null, new byte[] { 0, 1, 2 });
        //    return;
        //}

        ///// <summary>
        /////		Verify null signature
        ///// </summary>
        //private static void DeformatterVerifyNullSig()
        //{
        //    DSASignatureDeformatter deformatter = new DSASignatureDeformatter(new DSACryptoServiceProvider());
        //    deformatter.VerifySignature(new byte[] { 0, 1, 2 }, (byte[])null);
        //    return;
        //}
        #endregion

        internal static CryptokiAttribute[] m_publicDsaKey = new CryptokiAttribute[]
        {
            new CryptokiAttribute(CryptokiAttribute.CryptokiType.Class  , Utility.ConvertToBytes((int)CryptokiClass.PUBLIC_KEY)),
            new CryptokiAttribute(CryptokiAttribute.CryptokiType.KeyType, Utility.ConvertToBytes((int)CryptoKey.KeyType.DSA)),
            new CryptokiAttribute(CryptokiAttribute.CryptokiType.Prime, new byte[]
                {
                    0xEA, 0x91, 0xF3, 0x4A, 0xB3, 0xC2, 0x2E, 0x08, 0x5F, 0x6C, 0x32, 0x41, 0xC7, 0xA4, 0xD6, 0x76,
                    0x40, 0xEB, 0x31, 0xCF, 0x4E, 0x7D, 0x4E, 0xF4, 0x80, 0x10, 0x9F, 0x37, 0x02, 0xEB, 0x72, 0x11,
                    0xCD, 0x38, 0xD6, 0x59, 0x41, 0x71, 0x7E, 0xC5, 0x1B, 0xBF, 0x0C, 0x2A, 0x6B, 0x8E, 0x3A, 0x75,
                    0x8C, 0xC8, 0xBC, 0x6A, 0xD4, 0x1B, 0x45, 0x57, 0xAF, 0x37, 0x9A, 0xFF, 0xEC, 0x82, 0xE7, 0x7F, 
                    0x08, 0xB1, 0x12, 0xF2, 0xEE, 0xE4, 0xAB, 0x4B, 0xA0, 0x65, 0xCB, 0x0B, 0xD0, 0xE4, 0x35, 0x79, 
                    0x12, 0x3A, 0x79, 0xB0, 0x2C, 0xF7, 0x25, 0x13, 0x2B, 0x77, 0x21, 0x92, 0x30, 0x8A, 0x71, 0xD7,
                    0x7F, 0xB9, 0xC4, 0x22, 0x5A, 0xFD, 0xE3, 0xEE, 0x36, 0xD4, 0x71, 0xF0, 0xB2, 0x13, 0x30, 0x15, 
                    0x74, 0xC6, 0xFA, 0xE0, 0xA6, 0x25, 0x9B, 0x90, 0x9C, 0x1E, 0xA6, 0x25, 0x8F, 0x09, 0x82, 0x15, 
                }),
            new CryptokiAttribute(CryptokiAttribute.CryptokiType.Subprime, new byte[]
                {
                    0xBE, 0x03, 0x3C, 0xAB, 0x5E, 0x1F, 0x30, 0x91, 0x06, 0x8B, 0x2E, 0xFA, 0x05, 0x2A, 0x65, 0x38, 
                    0x31, 0x00, 0xEE, 0xEF,
                }),
            new CryptokiAttribute(CryptokiAttribute.CryptokiType.Base, new byte[]
                {
                    0xA0, 0x2C, 0x0D, 0x4F, 0x11, 0x7C, 0x7C, 0x29, 0xC6, 0x63, 0xEC, 0x1F, 0x74, 0x04, 0xB5, 0xBC, 
                    0xB0, 0x77, 0xFF, 0xD1, 0x8C, 0xB0, 0xE2, 0x76, 0xD2, 0x5E, 0xD0, 0x95, 0xAF, 0x1E, 0xFB, 0xF3,
                    0x8B, 0x5B, 0x2D, 0x43, 0x85, 0x85, 0x5C, 0x8A, 0xC6, 0x81, 0x7A, 0xD8, 0x73, 0xD2, 0xE0, 0xEB,
                    0x3D, 0x01, 0xD6, 0x48, 0x32, 0xD9, 0xDE, 0x0B, 0x98, 0xC7, 0x71, 0x7E, 0xA8, 0x25, 0x77, 0xAD, 
                    0xCB, 0x7E, 0x88, 0x1A, 0x7E, 0x7E, 0x96, 0x7C, 0xB1, 0xD7, 0x93, 0xE5, 0xEB, 0xFB, 0x75, 0x8A,
                    0xC7, 0x5D, 0x76, 0x9B, 0xA4, 0xFF, 0x65, 0x7E, 0x28, 0xE6, 0x60, 0x37, 0x1F, 0x5E, 0x0B, 0x37,
                    0xE2, 0xE4, 0x53, 0xEC, 0xBD, 0xFA, 0xF7, 0x42, 0xEB, 0x8F, 0xFB, 0x07, 0x42, 0x24, 0x73, 0x39,
                    0x77, 0x47, 0x9C, 0xB8, 0xBA, 0x87, 0xDF, 0x61, 0xF4, 0xA6, 0xE1, 0x86, 0xBB, 0xF1, 0xB6, 0x18, 

                }),
                        
            new CryptokiAttribute(CryptokiAttribute.CryptokiType.Value, new byte[]
                {
                    0x11, 0x7F, 0x45, 0x01, 0x35, 0xE1, 0x55, 0xFB, 0x1C, 0x9A, 0x35, 0xB9, 0x35, 0xD7, 0xDE, 0x39, 
                    0x39, 0x19, 0x15, 0xE5, 0xB8, 0x88, 0xC1, 0xA2, 0xFD, 0x17, 0x63, 0x72, 0x93, 0x14, 0xC1, 0xB4,
                    0xCC, 0xB1, 0xDC, 0x20, 0x09, 0x6D, 0xAD, 0xED, 0x35, 0x60, 0xE2, 0x79, 0x76, 0x84, 0x2D, 0x58,
                    0xE5, 0x15, 0xBE, 0xF1, 0x6B, 0x09, 0xA8, 0xA0, 0xE5, 0xBF, 0x86, 0xFD, 0x5F, 0xC0, 0xC9, 0x86,
                    0xD1, 0x35, 0x8A, 0x12, 0xC0, 0xDD, 0x48, 0x2C, 0x90, 0x94, 0x2B, 0x3C, 0x08, 0xA3, 0xDB, 0x50,
                    0x4F, 0x90, 0xEE, 0x78, 0xA8, 0x0F, 0x34, 0x20, 0xD5, 0x71, 0xFB, 0x98, 0x41, 0x91, 0x05, 0x5F,
                    0x6B, 0xEC, 0x58, 0x0B, 0x8C, 0x2F, 0x74, 0x02, 0x49, 0x11, 0x81, 0xB8, 0xE6, 0x96, 0x87, 0x7B,
                    0x9C, 0x11, 0x99, 0xCC, 0xAA, 0xD9, 0x6E, 0xC0, 0xE8, 0x39, 0x3C, 0x3D, 0x46, 0x06, 0x87, 0xBA,
                }),
        };
    }
}