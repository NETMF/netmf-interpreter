using System;
using System.Security.Cryptography; 
using System.IO; 
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Cryptoki;

namespace Microsoft.SPOT.Platform.Tests
{
    class Hash_SHA384 : IMFTestInterface
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

            Log.Comment("Testing SHA384 hash...");
            using (HashAlgorithm sh1 = new HashAlgorithm(HashAlgorithmType.SHA384, session))
            using (HashAlgorithm sh2 = new HashAlgorithm(HashAlgorithmType.SHA384, session2))
            {
                sh1.ComputeHash(abData1);
                sh2.ComputeHash(abData1);
                Log.Comment("The computed hash #1 is : ");
                PrintByteArray(sh1.Hash);
                Log.Comment("The correct hash #1 is : ");
                PrintByteArray(sh2.Hash);
                if (Compare(sh1.Hash, sh2.Hash))
                {
                    Log.Comment("CORRECT");
                }
                else
                {
                    Log.Comment("INCORRECT");
                    bRes = false;
                }
                sh1.ComputeHash(abData2);
                sh2.ComputeHash(abData2);
                Log.Comment("The computed hash #2 is : ");
                PrintByteArray(sh2.Hash);
                Log.Comment("The correct hash #2 is : ");
                PrintByteArray(sh1.Hash);
                if (Compare(sh2.Hash, sh1.Hash))
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
        public MFTestResults Hash_SHA384_Test()
        {
            bool bRes = true;

            try
            {
                if (m_isEmulator)
                {
                    using (Session sess = new Session("", MechanismType.SHA384))
                    using (Session sess2 = new Session("Emulator_Crypto", MechanismType.SHA384))
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