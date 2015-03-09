using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Cryptoki;

namespace Microsoft.SPOT.Platform.Tests
{
    /// <summary>
    /// This is a test scenario from Windows Live Core team.
    /// We basically need to verify that we can roundtrip with RSAServiceProvider and FromBase64Transform
    /// The RSAParameter was genereated on Desktop.
    /// </summary>
    class WLCScenario : IMFTestInterface
    {
        // ticket without signature
        string ticket = "<Assertion xmlns:saml=\"urn:oasis:names:tc:SAML:2.0:assertion\" ID=\"21a1dab3-2dcd-4dd1-8cd0-9383908f9b5d\" IssueInstant=\"5/22/2007 5:10:08 AM\" Version=\"SAML 2.0\"><Issuer>https://account.wlc.live.com</Issuer><Subject><NameID>/Identities/BULWPPLWLWSEDC6UWXFRYEZDYE</NameID></Subject><Conditions NotBefore=\"5/22/2007 5:10:08 AM\" NotOnOrAfter=\"5/22/2007 1:10:08 PM\"><AudienceRestriction><Audience>/Devices/SNYITERJA3CUBAM7WGZEZOIQFU</Audience></AudienceRestriction></Conditions><AttributeStatement><Attribute Name=\"Identity\"></Attribute></AttributeStatement></Assertion>";

        // ticket signature
        //string ticketSignature = "<Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><SignedInfo><CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\" /><SignatureMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#rsa-sha1\" /><Reference URI=\"\"><Transforms><Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\" /></Transforms><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\" /><DigestValue>9fvcmLu9v2vb4xNVS7wjW9QyVtU=</DigestValue></Reference></SignedInfo><SignatureValue>GF8rS87lF22i5wuyl4/ZevmNK8RHi4OPjZCLuegwFc3Hrxmm/ZYY7dyO2hsmNA5lgMhsu/x1Z4v1ZPpi4URTX2Eg/Bk94QFWKEJmBTZ0RF9HkBafAT5A6j2gzSUwKnIuOenBHtR31dtmK313mcBiyPURXvC03Aib/sOOSDkzduA=</SignatureValue></Signature>";

        string signature = "GF8rS87lF22i5wuyl4/ZevmNK8RHi4OPjZCLuegwFc3Hrxmm/ZYY7dyO2hsmNA5lgMhsu/x1Z4v1ZPpi4URTX2Eg/Bk94QFWKEJmBTZ0RF9HkBafAT5A6j2gzSUwKnIuOenBHtR31dtmK313mcBiyPURXvC03Aib/sOOSDkzduA=";
        string signinfo = "<SignedInfo xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\"></CanonicalizationMethod><SignatureMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#rsa-sha1\"></SignatureMethod><Reference URI=\"\"><Transforms><Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\"></Transform></Transforms><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\"></DigestMethod><DigestValue>9fvcmLu9v2vb4xNVS7wjW9QyVtU=</DigestValue></Reference></SignedInfo>";
        string digest = "9fvcmLu9v2vb4xNVS7wjW9QyVtU=";

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
        public MFTestResults WLCScenario_Test()
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

        public bool Test(Session session)
        {
            // The RSA Parameter is extracted from Desktop x509certificate provided by WLC team
            RSACryptoServiceProvider key = new RSACryptoServiceProvider(session);
            key.ImportParameters(WLCRSAData.GetKeyParameters());
            bool b = VerifyTicketHelper(ticket, signature, key);

            if (b)
            {
                Log.Comment("Pass");
                return true;
            }
            else
            {
                Log.Comment("Fail");
                return false;
            }
        }

        private byte[] ConvertToByteArray(string str)
        {
            char[] ch = str.ToCharArray();
            byte[] ret = new byte[ch.Length];
            for (int i = 0; i < ch.Length; i++)
                ret[i] = (byte)(ch[i]);
            return ret;
        }

        private bool VerifyTicketHelper(string ticket, string signature, RSACryptoServiceProvider key)
        {
            //byte[] byteSig = ConvertToByteArray(signature);
            //FromBase64Transform transform = new FromBase64Transform();
            //byte[] rawSigBuf = new byte[byteSig.Length];  // big enough
            //int rawSigSize = transform.TransformBlock(byteSig, 0, byteSig.Length, rawSigBuf, 0);
            byte[] rawSig = Convert.FromBase64String(signature);

            byte[] byteSignedInfo = ConvertToByteArray(signinfo);

            // Compute SignedInfo hash
            ////SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
            HashAlgorithm sha1 = new HashAlgorithm(HashAlgorithmType.SHA1, key.Session);
            byte[] signedInfoHash = sha1.ComputeHash(byteSignedInfo);

            // Verify signature
            bool f = key.VerifyHash(signedInfoHash, MechanismType.SHA_1, rawSig);
            if (!f)
            {
                Log.Comment("Fail to key.VerifyHash");
                return false;
            }
            // Get ticket hash from reference.

            //byte[] byteDigest = ConvertToByteArray(digest);

            //byte[] rawDigestBuf = new byte[byteDigest.Length];
            //int rawDigestSize = transform.TransformBlock(byteDigest, 0, byteDigest.Length, rawDigestBuf, 0);
            //byte[] rawDigest = new byte[rawDigestSize];
            //Array.Copy(rawDigestBuf, 0, rawDigest, 0, rawDigestSize);
            byte[] rawDigest = Convert.FromBase64String(digest);

            //byte[] rawDigest2 = transform.TransformFinalBlock(byteDigest, 0, byteDigest.Length);

            // Computer hash from ticket.
            string canonicalTicket = ticket;
            byte[] byteTicket = ConvertToByteArray(canonicalTicket);
            byte[] ticketHash = sha1.ComputeHash(byteTicket);

            bool equal = Compare(rawDigest, ticketHash); // && Compare(rawDigest, rawDigest2);

            if (equal == false)
                Log.Comment("Hash not equal");
            return equal;
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
