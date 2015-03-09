// Compare hash results of random data between pairs of hash algorithms
//
using System;
using System.Collections;
using System.Security.Cryptography;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Cryptoki;

namespace Microsoft.SPOT.Platform.Tests
{
    class HashCompare : IMFTestInterface
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

        internal class AlgorithmPairs
        {
            internal AlgorithmPairs(HashAlgorithm h1, HashAlgorithm h2)
            {
                Hash1 = h1;
                Hash2 = h2;
            }
            internal HashAlgorithm Hash1;
            internal HashAlgorithm Hash2;
        }

        public const int DefaultMaxLength = 1000;
        public const int DefaultIterations = 100;

        public static AlgorithmPairs[] m_AlgorithmPairs;

        // The SHA{256,284,512}CSP algorithms are only supported on Win2k3 and higher (version 5.2+)
        //
        public static void InitAlgorithmList(Session s1, Session s2)
        {

            m_AlgorithmPairs = new AlgorithmPairs[] {
			    new AlgorithmPairs( new HashAlgorithm(HashAlgorithmType.SHA1  , s1), new HashAlgorithm(HashAlgorithmType.SHA1  , s2) ),
			    new AlgorithmPairs( new HashAlgorithm(HashAlgorithmType.SHA256, s1), new HashAlgorithm(HashAlgorithmType.SHA256, s2) ),
			    new AlgorithmPairs( new HashAlgorithm(HashAlgorithmType.SHA384, s1), new HashAlgorithm(HashAlgorithmType.SHA384, s2) ),
			    new AlgorithmPairs( new HashAlgorithm(HashAlgorithmType.SHA512, s1), new HashAlgorithm(HashAlgorithmType.SHA512, s2) ),
			    new AlgorithmPairs( new HashAlgorithm(HashAlgorithmType.MD5   , s1), new HashAlgorithm(HashAlgorithmType.MD5   , s2) ), 
		    };
        }

        [TestMethod]
        public MFTestResults Hash_SHA512_Test()
        {
            bool bRes = true;
            Random rand = new Random();
            int maxLength = DefaultMaxLength;
            int iterations = DefaultIterations;

            try
            {
                if (m_isEmulator)
                {
                    using (Session sess = new Session("", MechanismType.SHA512))
                    using (Session sess2 = new Session("Emulator_Crypto", MechanismType.SHA512))
                    {
                        InitAlgorithmList(sess, sess2);

                        for (int i = 0; i < iterations; i++)
                        {
                            // Create hash data -- random byte length with random data
                            //
                            byte[] data = new byte[rand.Next(maxLength + 1) + 1];
                            rand.NextBytes(data);

                            // For the given data, compare the hash value produced by each pair of hash algorithms
                            //
                            for (int j = 0; j < m_AlgorithmPairs.Length; j++)
                            {
                                if (!CompareHashes(m_AlgorithmPairs[j].Hash1, m_AlgorithmPairs[j].Hash2, data))
                                {
                                    Log.Comment("Hash mismatch, test fails");
                                    bRes = false;
                                }
                            }
                        }
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

        public static bool CompareHashes(HashAlgorithm algorithm1, HashAlgorithm algorithm2, byte[] data)
        {
            algorithm1.Initialize();
            algorithm2.Initialize();

            byte[] hash1 = algorithm1.ComputeHash(data);
            byte[] hash2 = algorithm2.ComputeHash(data);

            if (!CompareBytes(hash1, hash2))
            {
                Log.Comment("ERROR - HASH VALUE MISCOMPARISON!\n");
                Log.Comment("Algorithm 1 : " + algorithm1.ToString());
                Log.Comment("Algorithm 2 : " + algorithm2.ToString());
                Log.Comment("Data: " + ByteArrayToString(data));
                return false;
            }

            return true;
        }

        public static string ByteArrayToString(byte[] bytes)
        {
            string str = "";

            for (int i = 0; i < bytes.Length; i++)
                str = str + bytes[i].ToString("X2");

            return str;
        }

        public static bool CompareBytes(byte[] x, byte[] y)
        {
            if (x.Length != y.Length)
            {
                Log.Comment("Byte array length mismatch\n");
                Log.Comment("Array 1 : " + x.Length);
                Log.Comment("Array 2 : " + y.Length);
                return false;
            }

            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i])
                {
                    Log.Comment("Byte array mismatch\n");
                    Log.Comment("Array 1 : " + ByteArrayToString(x));
                    Log.Comment("Array 2 : " + ByteArrayToString(y));
                    return false;
                }
            }

            return true;
        }
    }
}