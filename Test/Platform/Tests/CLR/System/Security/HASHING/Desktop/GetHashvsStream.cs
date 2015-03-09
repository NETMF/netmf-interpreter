//
//	This test finds all possible hash algorithms and sees if they
//	perform equally with GetHash vs Streaming model.
//

using System;
using System.Security.Cryptography;
using System.Reflection;
using System.Collections;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Cryptoki;

namespace Microsoft.SPOT.Platform.Tests
{
    class GetHashVsStream2 : IMFTestInterface
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

        public const int MAX_PASSES = 20;
        public const int MAX_SIZE = 100000;

        static Random m_Rnd = new Random();

        public static bool Test()
        {
            ArrayList alAlgorithms = null;
            HashAlgorithm hash = null;
            bool bRes = true;

            // find all non-abstract classes in the mscorlib that derive from hashalgorithm
            alAlgorithms = GetHashClasses();
            Log.Comment("Found " + alAlgorithms.Count + " hash classes.");

            // go through all of them
            foreach (Type t in alAlgorithms)
            {
                Log.Comment("--- Class " + t.FullName + " ---");
                // instantiate the class
                try
                {
                    hash = (HashAlgorithm)(Activator.CreateInstance(t));
                }
                catch (Exception e)
                {
                    Log.Comment("Exception " + e.ToString() + "\n has been thrown" +
                                    "when tried to instantiate the class");
                    continue;
                }

                // @todo: may be set the key in KeyedHashAlgorithms

                // test the hash object
                bRes = TestHash(hash) && bRes;
            }

            return bRes;
        }

        // returns all nonabstract hash classes in mscorlib
        public static ArrayList GetHashClasses()
        {
            Type[] atAllTypes = Assembly.GetAssembly(typeof(Object)).GetTypes();
            ArrayList alRes = new ArrayList();
            Type tHashType = typeof(System.Security.Cryptography.HashAlgorithm);

            foreach (Type t in atAllTypes)
                if ((t.IsSubclassOf(tHashType)) && (!t.IsAbstract)) alRes.Add(t);

            return alRes;
        }

        // tests a hash algorithm instance 
        public static bool TestHash(HashAlgorithm hash)
        {
            bool bRes = true;
            // decide on the number of passes
            int nPasses = m_Rnd.Next(MAX_PASSES) + 1;
            Log.Comment("Doing " + nPasses + " passes...");

            while (0 != nPasses--)
            {
                // init the hash object
                hash.Initialize();

                // create a random data blob
                int nSize = m_Rnd.Next(MAX_SIZE);
                byte[] abBlob = new byte[nSize];
                Log.Comment("Test buffer size is " + nSize);

                // first try ComputeHash
                byte[] hash1 = hash.ComputeHash(abBlob);

                //			Log.Comment("Hash1:");
                //			PrintByteArray(hash1);

                // now try stream
                hash.Initialize();
                CryptoStream cs = new CryptoStream(CryptoStream.Null, hash, CryptoStreamMode.Write);
                cs.Write(abBlob, 0, abBlob.Length);
                cs.Close();
                byte[] hash2 = hash.Hash;

                //			Log.Comment("Hash2:");
                //			PrintByteArray(hash2);

                if (Compare(hash1, hash2))
                {
                    Log.Comment(" OK.");
                }
                else
                {
                    bRes = false;
                    Log.Comment(" FAILED. Hashes are different.");
                }

            }
            return bRes;
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

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [TestMethod]
        public MFTestResults GetHashvsStream2_Test()
        {
            bool bRes = true;

            try
            {
                using (Session sess = new Session("", MechanismType.SHA256))
                {
                    bRes &= Test(sess);
                }

                if (m_isEmulator)
                {
                    using (Session sess = new Session("Emulator_Crypto", MechanismType.SHA256))
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