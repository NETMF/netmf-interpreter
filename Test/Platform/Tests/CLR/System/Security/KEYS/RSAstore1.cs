using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Cryptoki;

namespace Microsoft.SPOT.Platform.Tests
{
    class RSAstore : IMFTestInterface
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

        public static Boolean Test(Session session)
        {
            Boolean bRes = true;
            //String xml1, xml2, xml3;
            //String sign1, sign2, sign3;
            byte[] sig1;
            byte[] hashval = new byte[20];
            for (int i = 0; i < hashval.Length; i++) hashval[i] = (Byte)i;

            RSACryptoServiceProvider RSA1 = new RSACryptoServiceProvider(session);
            RSACryptoServiceProvider RSA2 = new RSACryptoServiceProvider(session);
            //RSACryptoServiceProvider RSA3 = new RSACryptoServiceProvider();

            sig1 = RSA1.SignHash(hashval, MechanismType.SHA_1); //, CryptoConfig.MapNameToOID("SHA1"));


            RSAParameters RSAParams = RSA1.ExportParameters(true);

            //sign1 = (Convert.ToBase64String(sig1));

            //xml1 = RSA1.ToXmlString(true);

            RSA2.ImportParameters(RSAParams);
            //RSA2.FromXmlString(xml1);

            //xml2 = (RSA2.ToXmlString(true));
            //xml3 = (RSA3.ToXmlString(true));

            byte[] sig2 = RSA2.SignHash(hashval, MechanismType.SHA_1);

            //sign2 = (Convert.ToBase64String(sig2));

            //byte[] sig3 = RSA3.SignHash(hashval, CryptoConfig.MapNameToOID("SHA1"));
            //sign3 = (Convert.ToBase64String(sig3));

            //if ((xml1 != xml2) || (xml2 != xml3))
            //{
            //    Log.Comment("WRONG : ToXmlString results are different");
            //    bRes = false;
            //}

            //Log.Comment(xml1);

            //if ((sign1 != sign2) || (sign2 != sign3))
            //{
            //    Log.Comment("WRONG : signatures are different");
            //    Log.Comment("First: " + sign1);
            //    Log.Comment("Second: " + sign2);
            //    Log.Comment("Third: " + sign3);

            //    bRes = false;
            //}

            //Log.Comment("\n" + sign1);

            if (!RSA1.VerifyHash(hashval, MechanismType.SHA_1, sig2))
            {
                Log.Comment("WRONG : Signature check (1) failed");
                bRes = false;
            }
            if (!RSA2.VerifyHash(hashval, MechanismType.SHA_1, sig1))
            {
                Log.Comment("WRONG : Signature check (1) failed");
                bRes = false;
            }
            //if (!RSA3.VerifyHash(hashval, CryptoConfig.MapNameToOID("SHA1"), sig1))
            //{
            //    Log.Comment("WRONG : Signature check (1) failed");
            //    bRes = false;
            //}

            return bRes;
        }


        [TestMethod]
        public MFTestResults RSAStore_Test()
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
    }
}