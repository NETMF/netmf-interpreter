using System;
using System.Security.Cryptography; 
using System.IO; 
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Cryptoki;

namespace Microsoft.SPOT.Platform.Tests
{
    class Hash_SHA512 : IMFTestInterface
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



        static Boolean Test(Session session, Session session2)
        {
            Boolean bRes = true;
            Byte[] abData1 = (new System.Text.UTF8Encoding()).GetBytes("Hi There");
            Byte[] abData2 = (new System.Text.UTF8Encoding()).GetBytes("what do ya want for nothing?");

            Log.Comment("Testing rc21 hash...");
            using (HashAlgorithm rc21 = new HashAlgorithm(HashAlgorithmType.SHA512, session))
            using (HashAlgorithm rc22 = new HashAlgorithm(HashAlgorithmType.SHA512, session2))
            {
                rc21.ComputeHash(abData1);
                rc22.ComputeHash(abData1);
                Log.Comment("The computed hash #1 is : ");
                PrintByteArray(rc21.Hash);
                Log.Comment("The correct hash #1 is : ");
                PrintByteArray(rc22.Hash);
                if (Compare(rc21.Hash, rc22.Hash))
                {
                    Log.Comment("CORRECT");
                }
                else
                {
                    Log.Comment("INCORRECT");
                    bRes = false;
                }
                rc21.ComputeHash(abData2);
                rc22.ComputeHash(abData2);
                Log.Comment("The computed hash #2 is : ");
                PrintByteArray(rc22.Hash);
                Log.Comment("The correct hash #2 is : ");
                PrintByteArray(rc21.Hash);
                if (Compare(rc22.Hash, rc21.Hash))
                {
                    Log.Comment("CORRECT");
                }
                else
                {
                    Log.Comment("INCORRECT");
                    bRes = false;
                }
            }
            return bRes;
        }

        [TestMethod]
        public MFTestResults Hash_SHA512_Test()
        {
            bool bRes = true;

            try
            {
                if (m_isEmulator)
                {
                    using (Session sess = new Session("", MechanismType.SHA512))
                    using (Session sess2 = new Session("Emulator_Crypto", MechanismType.SHA512))
                    {
                        bRes &= Test(sess, sess2);
                    }
                }
                else
                {
                    return MFTestResults.Skip;
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
