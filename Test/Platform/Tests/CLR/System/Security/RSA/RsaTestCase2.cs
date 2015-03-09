using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Cryptoki;

namespace Microsoft.SPOT.Platform.Tests
{
    /// <summary>
    /// This test is to verify RSAServiceProvider VerifyHash API
    /// The RSAParameter was genereated on Desktop.
    /// It verifies that for key sizes of the following values, appropriate exceptions are thrown.
    /// the biggest value that can be passed to the array size and does not cause OutOfMemoryException is Int32.MaxValue / 8 + 1
    /// </summary>
    public class RsaSignatureTest2 : IMFTestInterface
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
        public MFTestResults RSATestCase2_Test()
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


        static int[] ArraySizes = new int[] { /*0,*/ 1, 127, 128, 129, Byte.MaxValue, Byte.MaxValue - 1, Byte.MaxValue + 1 }; //, Int16.MaxValue, Int16.MaxValue + 1, Int16.MaxValue - 1, Int32.MaxValue / 8, Int32.MaxValue / 8 + 1, Int32.MaxValue / 8 - 1 };
        //static private byte[] Int32MAXDevBy8;
        //static private byte[] Int32MAXDevBy8Plus1;
        //static private byte[] Int32MAXDevBy8Minus1;

        public static bool Test(Session session)
        {
            bool bRet = true;

            //Int32MAXDevBy8 = CreateByteArray(Int32.MaxValue / 8);
            //Int32MAXDevBy8Plus1 = CreateByteArray(Int32.MaxValue / 8 + 1);
            //Int32MAXDevBy8Minus1 = CreateByteArray(Int32.MaxValue / 8 - 1);

            // Make sure RSAServiceProvider can handle various exception scenarios
            if (!RunRSATest(session))
                bRet = false;

            return bRet;
        }

        public static byte[] CreateByteArray(int size)
        {
            Log.Comment("Create an array of size " + size);
            byte[] bArray = new byte[size];

            // fill in the array

            for (int i = 0; i < size; i++)
            {
                bArray[i] = 1;
            }
            return bArray;
        }

        /*  need to cover the following interesting combination of exponent and modulus
         * a.	Exponent 1, modulus 122 – the target is to make the entire sequence 127
            b.	Exponent 1,  modulus 123, the target is 128
            c.	Exponent is 1, modulus is Byte.MaxValue-6 (3 for modulus + 3 for type and size of exponent), target is Byte.MaxValue
            d.	Exponent is 2, modulus is Byte.MaxValue-6, target is Byte.MaxValue+1
            e.	Exponent is 1, modulus is Int16.MaxValue - 7 (3 for exponent+1 for type of modulus+3 for size of length field of modulus), target is Int16.MaxValue
            f.	Exponent is 2, modulus is Int16.MaxValue - 7 (3 for exponent+1 for type of modulus+3 for size of length field of modulus), target is Int16.MaxValue+1
    */
        static void InterestingExponentModulusPairs(int i, out byte[] Exponent, out byte[] Modulus)
        {
            switch (i)
            {
                case 1:
                    Log.Comment("Exponent size 1, Modulus size 122");
                    Exponent = CreateByteArray(1);
                    Modulus = CreateByteArray(122);
                    break;
                case 2:
                    Log.Comment("Exponent size 1, Modulus size 123");
                    Exponent = CreateByteArray(1);
                    Modulus = CreateByteArray(123);
                    break;
                case 3:
                    Log.Comment("Exponent size 1, Modulus size Byte.MaxValue - 6");
                    Exponent = CreateByteArray(1);
                    Modulus = CreateByteArray(Byte.MaxValue - 6);
                    break;
                case 4:
                    Log.Comment("Exponent size 2, Modulus size Byte.MaxValue - 6");
                    Exponent = CreateByteArray(2);
                    Modulus = CreateByteArray(Byte.MaxValue - 6);
                    break;
                //case 5:
                //    Log.Comment("Exponent size 1, Modulus size Int16.MaxValue - 7");
                //    Exponent = CreateByteArray(1);
                //    Modulus = CreateByteArray(Int16.MaxValue - 7);
                //    break;
                //case 6:
                //    Log.Comment("Exponent size 2, Modulus size Int16.MaxValue - 7");
                //    Exponent = CreateByteArray(2);
                //    Modulus = CreateByteArray(Int16.MaxValue - 7);
                //    break;
                default:
                    throw new Exception("Wrong combination of Exponent and Modulus values are passed");
            }
        }

        static bool VerifyPairs(Session session, byte[] pair1, byte[] pair2)
        {
            bool bRet = true;
            RSAParameters key = new RSAParameters();
            try
            {
                key.Exponent = pair1;
                key.Modulus = pair2;

                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(session))
                {
                    rsa.ImportParameters(key);
                    //rsa.VerifyHash(RsaTestData2.GetHashValue(), RsaTestData2.HashAlgorithm, RsaTestData2.GetSignature());
                }
            }
            catch (CryptographicException)
            {
                bRet = false;
            }

            // switch Exponent and Modulus values
            try
            {
                key.Exponent = pair2;
                key.Modulus = pair1;

                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(session))
                {
                    rsa.ImportParameters(key);
                    //rsa.VerifyHash(RsaTestData2.GetHashValue(), RsaTestData2.HashAlgorithm, RsaTestData2.GetSignature());
                }
            }
            catch (CryptographicException)
            {
                bRet = false;
                //
            }
            return bRet;
        }

        static bool RunRSATest(Session session)
        {
            RSAParameters key = new RSAParameters();

            //for (int i = 0; i < ArraySizes.Length; i++)
            //{
            //    for (int j = 0; j < ArraySizes.Length; j++)
            //    {
            //        if (!VerifyPairs(session, new byte[ArraySizes[i]], new byte[ArraySizes[j]]))
            //            return false;
            //    }
            //}

            Log.Comment("Begin testing intersting Exponent/Modulus pairs");
            for (int i = 1; i <= 4; i++)
            {
                try
                {
                    byte[] Exponent, Modulus;
                    InterestingExponentModulusPairs(i, out Exponent, out Modulus);

                    key.Exponent = Exponent;
                    key.Modulus = Modulus;
                    using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(session))
                    {
                        rsa.ImportParameters(key);
                        bool verified = rsa.VerifyHash(RsaTestData2.GetHashValue(), RsaTestData2.HashAlgorithm, RsaTestData2.GetSignature());
                        if (verified) // should not verify
                        {
                            Log.Comment("Should not have verified the Exponent and Modulus pair");
                            Log.Comment("key.Exponent.Length = " + key.Exponent.Length + ", key.Modulus.Length = " + key.Modulus.Length);
                            return false;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Comment("Caught unexpected exception: " + e.ToString());
                    return false;
                }
            }

            return true;

        }
    }
}