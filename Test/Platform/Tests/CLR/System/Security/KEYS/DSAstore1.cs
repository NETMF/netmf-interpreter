using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Cryptoki;

namespace Microsoft.SPOT.Platform.Tests
{
    class DSAstore : IMFTestInterface
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
            byte[] hashval = new byte[20];
            for (int i = 0; i < hashval.Length; i++) hashval[i] = (Byte)i;

            DSACryptoServiceProvider dsa1 = new DSACryptoServiceProvider(session);
            DSACryptoServiceProvider dsa2 = new DSACryptoServiceProvider(session);
            //DSACryptoServiceProvider dsa3 = new DSACryptoServiceProvider(session);

            DSAParameters dsaParams = dsa1.ExportParameters(true);

            byte[] sig1 = dsa1.SignHash(hashval, MechanismType.SHA_1);

            //sign1 = (Convert.ToBase64String(sig1));

            //xml1 = dsa1.ToXmlString(true);


            dsa2.ImportParameters(dsaParams);
            //dsa2.FromXmlString(xml1);

            //xml2 = (dsa2.ToXmlString(true));
            //xml3 = (dsa3.ToXmlString(true));

            byte[] sig2 = dsa2.SignHash(hashval, MechanismType.SHA_1);
            //sign2 = (Convert.ToBase64String(sig2));

            //dsa3.HashAlgorithm = MechanismType.SHA_1;
            //byte[] sig3 = dsa3.SignHash(hashval);
            //sign3 = (Convert.ToBase64String(sig3));

            //if ((xml1 != xml2) || (xml2 != xml3))
            //{
            //    Log.Comment("WRONG : ToXmlString results are different");
            //    Log.Comment("XML1:\n" + xml1);
            //    Log.Comment("XML2:\n" + xml2);
            //    Log.Comment("XML3:\n" + xml3);
            //    bRes = false;
            //}

            //Log.Comment(xml1);

            /*        if ( (sign1!=sign2) || (sign2!=sign3) ) {
                        Log.Comment("WRONG : signatures are different");
                        Log.Comment("First: " + sign1);
                        Log.Comment("Second: " + sign2);
                        Log.Comment("Third: " + sign3);
        	
                        bRes = false;
                    } */

            //Log.Comment("\n" + sign1);

            if (!dsa1.VerifyHash(hashval, MechanismType.SHA_1, sig2))
            {
                Log.Comment("WRONG : Signature check (1) failed");
                bRes = false;
            }
            if (!dsa2.VerifyHash(hashval, MechanismType.SHA_1, sig1))
            {
                Log.Comment("WRONG : Signature check (1) failed");
                bRes = false;
            }
            //if (!dsa3.VerifyHash(hashval, sig1))
            //{
            //    Log.Comment("WRONG : Signature check (1) failed");
            //    bRes = false;
            //}

            return bRes;
        }


        [TestMethod]
        public MFTestResults DSAStore_Test()
        {
            bool bRes = true;

            try
            {
                using (Session sess = new Session("", MechanismType.DSA))
                {
                    bRes &= Test(sess);
                }

                if (m_isEmulator)
                {
                    using (Session sess = new Session("Emulator_Crypto", MechanismType.DSA))
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