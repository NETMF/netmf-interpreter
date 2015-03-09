// Test various SHA* algorithms using various methods against SHAVS test vectors
//
// Secure Hash Algorithm Validation System - http://csrc.nist.gov/cryptval/shs/SHAVS.pdf
//
using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;

using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Cryptoki;

namespace Microsoft.SPOT.Platform.Tests
{
    class SHAVS : IMFTestInterface
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

        internal class TestDigestVector
        {
            internal TestDigestVector(string data, string digest)
            {
                Data = data;
                Digest = digest;
            }
            internal string Data;
            internal string Digest;
        }

        // SHAVS test vectors.  Only included are ones with bit length a multiple of 8 since the APIs only work with bytes
        //
        public static TestDigestVector[] SHA1TestVectors = {
		new TestDigestVector( "5e",									"5e6f80a34a9798cafc6a5db96cc57ba4c4db59c2" ),
		new TestDigestVector( "9a7dfdf1ecead06ed646aa55fe757146",	"82abff6605dbe1c17def12a394fa22a82b544a35" ),
		new TestDigestVector( "f78f92141bcd170ae89b4fba15a1d59f" +
		  "3fd84d223c9251bdacbbae61d05ed115" +
		  "a06a7ce117b7beead24421ded9c32592" +
		  "bd57edeae39c39fa1fe8946a84d0cf1f" +
		  "7beead1713e2e0959897347f67c80b04" +
		  "00c209815d6b10a683836fd5562a56ca" +
		  "b1a28e81b6576654631cf16566b86e3b" +
		  "33a108b05307c00aff14a768ed735060" +
		  "6a0f85e6a91d396f5b5cbe577f9b3880" +
		  "7c7d523d6d792f6ebc24a4ecf2b3a427" +
		  "cdbbfb",								"cb0082c8f197d260991ba6a460e76e202bad27b3" ),
		new TestDigestVector( "d0569cb3665a8a43eb6ea23d75a3c4d2" +
		  "054a0d7dd0569cb3665a8a43eb6ea23d" +
		  "75a3c4d2054a0d7dd0569cb3665a8a43" +
		  "eb6ea23d75a3c4d2054a0d7d",			"5c1f6ab1dd1a1b92313ef55bd94e5d90eee5fd42" ),
		new TestDigestVector( "d0569cb3665a8a43eb6ea23d75a3c4d2" +
		  "054a0d7dd0569cb3665a8a43eb6ea23d" +
		  "75a3c4d2054a0d7d5c1f6ab1dd1a1b92" +
		  "313ef55bd94e5d90eee5fd42",			"3dc3b220871bb3488a66c39dea577c2d1ce40168" ),
		new TestDigestVector( "d0569cb3665a8a43eb6ea23d75a3c4d2" +
		  "054a0d7d5c1f6ab1dd1a1b92313ef55b" +
		  "d94e5d90eee5fd423dc3b220871bb348" +
		  "8a66c39dea577c2d1ce40168",			"dd4b25777737c82cf228ac14cf04b905b081ac72" ),
		new TestDigestVector( "5c1f6ab1dd1a1b92313ef55bd94e5d90" +
		  "eee5fd423dc3b220871bb3488a66c39d" +
		  "ea577c2d1ce40168dd4b25777737c82c" +
		  "f228ac14cf04b905b081ac72",			"129aef9d88d256d5a667637ea215d7d84d710156" ),
	};

        public static TestDigestVector[] SHA256TestVectors = {
		new TestDigestVector( "19",									"68aa2e2ee5dff96e3355e6c7ee373e3d6a4e17f75f9518d843709c0c9bc3e3d4" ),
		new TestDigestVector( "e3d72570dcdd787ce3887ab2cd684652",	"175ee69b02ba9b58e2b0a5fd13819cea573f3940a94f825128cf4209beabb4e8" ),
		new TestDigestVector( "8326754e2277372f4fc12b20527afef0" +
		  "4d8a056971b11ad57123a7c137760000" +
		  "d7bef6f3c1f7a9083aa39d810db31077" +
		  "7dab8b1e7f02b84a26c773325f8b2374" +
		  "de7a4b5a58cb5c5cf35bcee6fb946e5b" +
		  "d694fa593a8beb3f9d6592ecedaa66ca" +
		  "82a29d0c51bcf9336230e5d784e4c0a4" +
		  "3f8d79a30a165cbabe452b774b9c7109" +
		  "a97d138f129228966f6c0adc106aad5a" +
		  "9fdd30825769b2c671af6759df28eb39" +
		  "3d54d6",								"97dbca7df46d62c8a422c941dd7e835b8ad3361763f7e9b2d95f4f0da6e1ccbc" ),
		new TestDigestVector( "f41ece2613e4573915696b5adcd51ca3" +
		  "28be3bf566a9ca99c9ceb0279c1cb0a7" +
		  "f41ece2613e4573915696b5adcd51ca3" +
		  "28be3bf566a9ca99c9ceb0279c1cb0a7" +
		  "f41ece2613e4573915696b5adcd51ca3" +
		  "28be3bf566a9ca99c9ceb0279c1cb0a7",	"fddf1b37dd34b3b201d43c57bcde115838f0df701da93c3bf2c9c86896e7e6c7" ),
		new TestDigestVector( "f41ece2613e4573915696b5adcd51ca3" +
		  "28be3bf566a9ca99c9ceb0279c1cb0a7" +
		  "f41ece2613e4573915696b5adcd51ca3" +
		  "28be3bf566a9ca99c9ceb0279c1cb0a7" +
		  "fddf1b37dd34b3b201d43c57bcde1158" +
		  "38f0df701da93c3bf2c9c86896e7e6c7",	"3b9e2613dc71d49925cc3258a3a4201aea4336c2a648ca8dffb45bbdad4835e8" ),
		new TestDigestVector( "f41ece2613e4573915696b5adcd51ca3" +
		  "28be3bf566a9ca99c9ceb0279c1cb0a7" +
		  "fddf1b37dd34b3b201d43c57bcde1158" +
		  "38f0df701da93c3bf2c9c86896e7e6c7" +
		  "3b9e2613dc71d49925cc3258a3a4201a" +
		  "ea4336c2a648ca8dffb45bbdad4835e8",	"9fbac41c7453a2c88fd3fed1f685ef27587bebcc573209bcc1b9f9eecf03c1fd" ),
		new TestDigestVector( "fddf1b37dd34b3b201d43c57bcde1158" +
		  "38f0df701da93c3bf2c9c86896e7e6c7" +
		  "3b9e2613dc71d49925cc3258a3a4201a" +
		  "ea4336c2a648ca8dffb45bbdad4835e8" +
		  "9fbac41c7453a2c88fd3fed1f685ef27" +
		  "587bebcc573209bcc1b9f9eecf03c1fd",	"b125c98b1a9d25f337b5a78815b6b7a7f091d32880e8681bdec8584b92aa3bf8" ),
	};

        public static TestDigestVector[] SHA384TestVectors = {
		new TestDigestVector( "b9",									"bc8089a19007c0b14195f4ecc74094fec64f01f90929282c2fb392881578208ad466828b1c6c283d2722cf0ad1ab6938" ),
		new TestDigestVector( "a41c497779c0375ff10a7f4e08591739",	"c9a68443a005812256b8ec76b00516f0dbb74fab26d665913f194b6ffb0e91ea9967566b58109cbc675cc208e4c823f7" ),
		new TestDigestVector( "399669e28f6b9c6dbcbb6912ec10ffcf" +
		  "74790349b7dc8fbe4a8e7b3b5621db0f" +
		  "3e7dc87f823264bbe40d1811c9ea2061" +
		  "e1c84ad10a23fac1727e7202fc3f5042" +
		  "e6bf58cba8a2746e1f64f9b9ea352c71" +
		  "1507053cf4e5339d52865f25cc22b5e8" +
		  "7784a12fc961d66cb6e89573199a2ce6" +
		  "565cbdf13dca403832cfcb0e8b7211e8" +
		  "3af32a11ac17929ff1c073a51cc027aa" +
		  "edeff85aad7c2b7c5a803e2404d96d2a" +
		  "77357bda1a6daeed17151cb9bc5125a4" +
		  "22e941de0ca0fc5011c23ecffefdd096" +
		  "76711cf3db0a3440720e1615c1f22fbc" +
		  "3c721de521e1b99ba1bd557740864214" +
		  "7ed096",								"4f440db1e6edd2899fa335f09515aa025ee177a79f4b4aaf38e42b5c4de660f5de8fb2a5b2fbd2a3cbffd20cff1288c0" ),
		new TestDigestVector( "8240bc51e4ec7ef76d18e35204a19f51" +
		  "a5213a73a81d6f944680d3075948b7e4" +
		  "63804ea3d26e13ea820d65a484be7453" +
		  "8240bc51e4ec7ef76d18e35204a19f51" +
		  "a5213a73a81d6f944680d3075948b7e4" +
		  "63804ea3d26e13ea820d65a484be7453" +
		  "8240bc51e4ec7ef76d18e35204a19f51" +
		  "a5213a73a81d6f944680d3075948b7e4" +
		  "63804ea3d26e13ea820d65a484be7453",	"8ff53918fb4da5566f074f641f2ce018a688fcbcb2efdb13ba6b3578bcd5a4898d0b2d88702dbbd8310bea5a975adf20" ),
		new TestDigestVector( "8240bc51e4ec7ef76d18e35204a19f51" +
		  "a5213a73a81d6f944680d3075948b7e4" +
		  "63804ea3d26e13ea820d65a484be7453" +
		  "8240bc51e4ec7ef76d18e35204a19f51" +
		  "a5213a73a81d6f944680d3075948b7e4" +
		  "63804ea3d26e13ea820d65a484be7453" +
		  "8ff53918fb4da5566f074f641f2ce018" +
		  "a688fcbcb2efdb13ba6b3578bcd5a489" +
		  "8d0b2d88702dbbd8310bea5a975adf20",	"b5d6eb75ae9bd90a27e46f0d732b53bcb74b8ff2f5df4d534ea78e8dbeb3127378bfafb135d59ec53d1f15f7c839cfaf" ),
		new TestDigestVector( "8240bc51e4ec7ef76d18e35204a19f51" +
		  "a5213a73a81d6f944680d3075948b7e4" +
		  "63804ea3d26e13ea820d65a484be7453" +
		  "8ff53918fb4da5566f074f641f2ce018" +
		  "a688fcbcb2efdb13ba6b3578bcd5a489" +
		  "8d0b2d88702dbbd8310bea5a975adf20" +
		  "b5d6eb75ae9bd90a27e46f0d732b53bc" +
		  "b74b8ff2f5df4d534ea78e8dbeb31273" +
		  "78bfafb135d59ec53d1f15f7c839cfaf",	"7870a434da5d941c4cdde45ade8e4fd1178ea3d36ecd1231fe675eb9a5e61c0134e495ba2a4c3b111a861abc679f9a37" ),
		new TestDigestVector( "8ff53918fb4da5566f074f641f2ce018" +
		  "a688fcbcb2efdb13ba6b3578bcd5a489" +
		  "8d0b2d88702dbbd8310bea5a975adf20" +
		  "b5d6eb75ae9bd90a27e46f0d732b53bc" +
		  "b74b8ff2f5df4d534ea78e8dbeb31273" +
		  "78bfafb135d59ec53d1f15f7c839cfaf" +
		  "7870a434da5d941c4cdde45ade8e4fd1" +
		  "178ea3d36ecd1231fe675eb9a5e61c01" +
		  "34e495ba2a4c3b111a861abc679f9a37",	"1ae75e92d9a24accfe1bc1b6abe0b64376efbb19cc49fa08fbd3bc17724fb7745045e826daa4f0defca7adcf165561a4" ),
	};

        public static TestDigestVector[] SHA512TestVectors = {
		new TestDigestVector( "d0",									"9992202938e882e73e20f6b69e68a0a7149090423d93c81bab3f21678d4aceeee50e4e8cafada4c85a54ea8306826c4ad6e74cece9631bfa8a549b4ab3fbba15" ),
		new TestDigestVector( "8d4e3c0e3889191491816e9d98bff0a0",	"cb0b67a4b8712cd73c9aabc0b199e9269b20844afb75acbdd1c153c9828924c3ddedaafe669c5fdd0bc66f630f6773988213eb1b16f517ad0de4b2f0c95c90f8" ),
		new TestDigestVector( "a55f20c411aad132807a502d65824e31" +
		  "a2305432aa3d06d3e282a8d84e0de1de" +
		  "6974bf495469fc7f338f8054d58c26c4" +
		  "9360c3e87af56523acf6d89d03e56ff2" +
		  "f868002bc3e431edc44df2f0223d4bb3" +
		  "b243586e1a7d924936694fcbbaf88d95" +
		  "19e4eb50a644f8e4f95eb0ea95bc4465" +
		  "c8821aacd2fe15ab4981164bbb6dc32f" +
		  "969087a145b0d9cc9c67c22b76329941" +
		  "9cc4128be9a077b3ace634064e6d9928" +
		  "3513dc06e7515d0d73132e9a0dc6d3b1" +
		  "f8b246f1a98a3fc72941b1e3bb2098e8" +
		  "bf16f268d64f0b0f4707fe1ea1a1791b" +
		  "a2f3c0c758e5f551863a96c949ad47d7" +
		  "fb40d2",								"c665befb36da189d78822d10528cbf3b12b3eef726039909c1a16a270d48719377966b957a878e720584779a62825c18da26415e49a7176a894e7510fd1451f5" ),
		new TestDigestVector( "473ff1b9b3ffdfa126699ac7ef9e8e78" +
		  "7773095824c642557c1399d98e422044" +
		  "8dc35b99bfdd44779543924c1ce93bc5" +
		  "941538895db988261b00774b12272039" +
		  "473ff1b9b3ffdfa126699ac7ef9e8e78" +
		  "7773095824c642557c1399d98e422044" +
		  "8dc35b99bfdd44779543924c1ce93bc5" +
		  "941538895db988261b00774b12272039" +
		  "473ff1b9b3ffdfa126699ac7ef9e8e78" +
		  "7773095824c642557c1399d98e422044" +
		  "8dc35b99bfdd44779543924c1ce93bc5" +
		  "941538895db988261b00774b12272039",	"ab96e447e4a2302835b06c3ec33da49972bfef0b8678068bc55859452292eaafcc0be7572c2d14a52591fa92357e58970f39ebfcd25877ae1ba5159eb251d029" ),
		new TestDigestVector( "473ff1b9b3ffdfa126699ac7ef9e8e78" +
		  "7773095824c642557c1399d98e422044" +
		  "8dc35b99bfdd44779543924c1ce93bc5" +
		  "941538895db988261b00774b12272039" +
		  "473ff1b9b3ffdfa126699ac7ef9e8e78" +
		  "7773095824c642557c1399d98e422044" +
		  "8dc35b99bfdd44779543924c1ce93bc5" +
		  "941538895db988261b00774b12272039" +
		  "ab96e447e4a2302835b06c3ec33da499" +
		  "72bfef0b8678068bc55859452292eaaf" +
		  "cc0be7572c2d14a52591fa92357e5897" +
		  "0f39ebfcd25877ae1ba5159eb251d029",	"94b957d0fa320e5938fa342a753b16ae74a6f3f1e27e226040ab7ce11a3ea232413539d5dc08a6d074673f63975628d130da35fdfce322a8fe3f63de44532669" ),
		new TestDigestVector( "473ff1b9b3ffdfa126699ac7ef9e8e78" +
		  "7773095824c642557c1399d98e422044" +
		  "8dc35b99bfdd44779543924c1ce93bc5" +
		  "941538895db988261b00774b12272039" +
		  "ab96e447e4a2302835b06c3ec33da499" +
		  "72bfef0b8678068bc55859452292eaaf" +
		  "cc0be7572c2d14a52591fa92357e5897" +
		  "0f39ebfcd25877ae1ba5159eb251d029" +
		  "94b957d0fa320e5938fa342a753b16ae" +
		  "74a6f3f1e27e226040ab7ce11a3ea232" +
		  "413539d5dc08a6d074673f63975628d1" +
		  "30da35fdfce322a8fe3f63de44532669",	"8d6751b5554672e23219132dad6bb041b650bea935012c41f488e84720a1310ead9133a16109627cfe9895f24d5fee884aa754a34c6549e8d17bd2c683261b43" ),
		new TestDigestVector( "ab96e447e4a2302835b06c3ec33da499" +
		  "72bfef0b8678068bc55859452292eaaf" +
		  "cc0be7572c2d14a52591fa92357e5897" +
		  "0f39ebfcd25877ae1ba5159eb251d029" +
		  "94b957d0fa320e5938fa342a753b16ae" +
		  "74a6f3f1e27e226040ab7ce11a3ea232" +
		  "413539d5dc08a6d074673f63975628d1" +
		  "30da35fdfce322a8fe3f63de44532669" +
		  "8d6751b5554672e23219132dad6bb041" +
		  "b650bea935012c41f488e84720a1310e" +
		  "ad9133a16109627cfe9895f24d5fee88" +
		  "4aa754a34c6549e8d17bd2c683261b43",	"69a635e914950e8ee15c27dbd833cb87a41669515049938a3acf7ef3a8c6c5ece13300b10bb02b8dfcc7d0bf1b504d3a45cc5623f07d938d1aa9902ebeae48ec" ),
	};

        [TestMethod]
        public MFTestResults SHAVS_Test()
        {
            bool fRes = true;
            Session sessEmu = null;

            try
            {
                using (Session sess = new Session("", MechanismType.SHA512))
                {
                    if (m_isEmulator)
                    {
                        sessEmu = new Session("Emulator_Crypto", MechanismType.SHA512);
                    }

                    if (!TestSHA1(sess, sessEmu))
                    {
                        Log.Comment("SHA1 test failed");
                        fRes = false;
                    }

                    if (!TestSHA256(sess, sessEmu))
                    {
                        Log.Comment("SHA256 test failed");
                        fRes = false;
                    }

                    if (!TestSHA384(sess, sessEmu))
                    {
                        Log.Comment("SHA384 test failed");
                        fRes = false;
                    }

                    if (!TestSHA512(sess, sessEmu))
                    {
                        Log.Comment("SHA512 test failed");
                        fRes = false;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Exception("", e);
                fRes = false;
            }
            finally
            {
                if(sessEmu != null)
                {
                    sessEmu.Dispose();
                }
            }

            if (fRes) Log.Comment("Test passed");
            return fRes ? MFTestResults.Pass : MFTestResults.Fail;
        }

        // Test loops for each algorithm
        //
        public static bool TestSHA1(Session s1, Session s2)
        {
            string dataStr;
            byte[] data;
            byte[] digest;

            for (int i = 0; i < SHA1TestVectors.Length; i++)
            {
                dataStr = SHA1TestVectors[i].Data;
                data = ConvertHexStrToBytes(dataStr);
                digest = ConvertHexStrToBytes(SHA1TestVectors[i].Digest);

                if (!TestHashVariousMethods(new HashAlgorithm(HashAlgorithmType.SHA1, s1), data, digest))
                {
                    Log.Comment("SHA1CryptoServiceProvider hash test failure");
                    Log.Comment("Data : " + dataStr);
                    return false;
                }

                if (s2 != null && !TestHashVariousMethods(new HashAlgorithm(HashAlgorithmType.SHA1, s2), data, digest))
                {
                    Log.Comment("SHA1 emu hash test failure");
                    Log.Comment("Data : " + dataStr);
                    return false;
                }
            }

            return true;
        }

        public static bool TestSHA256(Session s1, Session s2)
        {
            string dataStr;
            byte[] data;
            byte[] digest;

            for (int i = 0; i < SHA256TestVectors.Length; i++)
            {
                dataStr = SHA256TestVectors[i].Data;
                data = ConvertHexStrToBytes(dataStr);
                digest = ConvertHexStrToBytes(SHA256TestVectors[i].Digest);

                if (!TestHashVariousMethods(new HashAlgorithm(HashAlgorithmType.SHA256, s1), data, digest))
                {
                    Log.Comment("SHA256CryptoServiceProvider hash test failure");
                    Log.Comment("Data : " + dataStr);
                    return false;
                }

                if (s2 != null && !TestHashVariousMethods(new HashAlgorithm(HashAlgorithmType.SHA256, s2), data, digest))
                {
                    Log.Comment("SHA256 emu hash test failure");
                    Log.Comment("Data : " + dataStr);
                    return false;
                }
            }

            return true;
        }

        public static bool TestSHA384(Session s1, Session s2)
        {
            string dataStr;
            byte[] data;
            byte[] digest;

            for (int i = 0; i < SHA384TestVectors.Length; i++)
            {
                dataStr = SHA384TestVectors[i].Data;
                data = ConvertHexStrToBytes(dataStr);
                digest = ConvertHexStrToBytes(SHA384TestVectors[i].Digest);

                if (!TestHashVariousMethods(new HashAlgorithm(HashAlgorithmType.SHA384, s1), data, digest))
                {
                    Log.Comment("SHA384CryptoServiceProvider hash test failure");
                    Log.Comment("Data : " + dataStr);
                    return false;
                }

                if (s2 != null && !TestHashVariousMethods(new HashAlgorithm(HashAlgorithmType.SHA384, s2), data, digest))
                {
                    Log.Comment("SHA384 Emu hash test failure");
                    Log.Comment("Data : " + dataStr);
                    return false;
                }
            }

            return true;
        }

        public static bool TestSHA512(Session s1, Session s2)
        {
            string dataStr;
            byte[] data;
            byte[] digest;

            for (int i = 0; i < SHA512TestVectors.Length; i++)
            {
                dataStr = SHA512TestVectors[i].Data;
                data = ConvertHexStrToBytes(dataStr);
                digest = ConvertHexStrToBytes(SHA512TestVectors[i].Digest);

                if (!TestHashVariousMethods(new HashAlgorithm(HashAlgorithmType.SHA512, s1), data, digest))
                {
                    Log.Comment("SHA512CryptoServiceProvider hash test failure");
                    Log.Comment("Data : " + dataStr);
                    return false;
                }

                if (s2 != null && !TestHashVariousMethods(new HashAlgorithm(HashAlgorithmType.SHA512, s2), data, digest))
                {
                    Log.Comment("SHA512 Emu hash test failure");
                    Log.Comment("Data : " + dataStr);
                    return false;
                }
            }

            return true;
        }

        // Various testing approaches
        //
        public static bool TestHashVariousMethods(HashAlgorithm hash, byte[] data, byte[] digest)
        {
            return TestComputeHash(hash, data, digest) &&
                TestSingleBlock(hash, data, digest) &&
                TestMultiBlock(hash, data, digest); //&&
                //TestStream(hash, data, digest);
        }

        public static bool TestComputeHash(HashAlgorithm hash, byte[] data, byte[] digest)
        {
            hash.Initialize();

            if (!CompareBytes(hash.ComputeHash(data), digest))
            {
                Log.Comment("ComputeHash test failed");
                return false;
            }

            return true;
        }

        public static bool TestSingleBlock(HashAlgorithm hash, byte[] data, byte[] digest)
        {
            hash.Initialize();
            hash.TransformFinalBlock(data, 0, data.Length);

            if (!CompareBytes(hash.Hash, digest))
            {
                Log.Comment("SingleBlock test failed");
                return false;
            }

            return true;
        }

        public static bool TestMultiBlock(HashAlgorithm hash, byte[] data, byte[] digest)
        {
            int pos = 0;

            hash.Initialize();

            while (data.Length - pos >= 1)
                pos += hash.TransformBlock(data, pos, 1, data, pos);

            hash.TransformFinalBlock(data, pos, data.Length - pos);

            if (!CompareBytes(hash.Hash, digest))
            {
                Log.Comment("MultiBlock test failed");
                return false;
            }

            return true;
        }

        //public static bool TestStream(HashAlgorithm hash, byte[] data, byte[] digest)
        //{
        //    StreamReader sr = new StreamReader(new CryptoStream(new MemoryStream(data), hash, CryptoStreamMode.Read));

        //    hash.Initialize();
        //    sr.ReadToEnd();

        //    if (!CompareBytes(hash.Hash, digest))
        //    {
        //        Log.Comment("Stream test failed");
        //        return false;
        //    }

        //    return true;
        //}

        // Utility functions
        //

        static int ParseChar(char c)
        {
            if ('0' <= c && c <= '9')
            {
                return c - '0';
            }
            else if ('a' <= c && c <= 'f')
            {
                return c - 'a' + 10;
            }
            else if ('A' <= c && c <= 'F')
            {
                return c - 'A' + 10;
            }
            return 0;
        }

        static byte[] ConvertHexStrToBytes(string hex)
        {
            byte[] ret = new byte[hex.Length / 2];
            for (int i = 0, j = 0; i < hex.Length; i += 2, j++)
            {
                ret[j] = (byte)(ParseChar(hex[i]) << 4 | ParseChar(hex[i + 1]));
            }
            return ret;
        }

        private static string ByteToString(byte b)
        {
            char[] str = new char[2];

            byte nib = (byte)((b >> 4) & 0xF);

            if (0 <= nib && nib <= 9)
            {
                str[0] = (char)('0' + nib);
            }
            else
            {
                str[0] = (char)('F' + (nib - 10));
            }

            nib = (byte)(b & 0xF);

            if (0 <= nib && nib <= 9)
            {
                str[1] = (char)('0' + nib);
            }
            else
            {
                str[1] = (char)('F' + (nib - 10));
            }

            return new string(str);
        }

        public static string ConvertBytesToHexStr(byte[] bytes)
        {
            string str = "";

            for (int i = 0; i < bytes.Length; i++)
                str += ByteToString(bytes[i]);

            return str;
        }

        public static bool CompareBytes(byte[] actual, byte[] expected)
        {
            if (actual.Length != expected.Length)
            {
                Log.Comment("Byte array length mismatch");
                return false;
            }

            for (int i = 0; i < actual.Length; i++)
            {
                if (actual[i] != expected[i])
                {
                    Log.Comment("Byte array mismatach.");
                    Log.Comment("Actual   : " + ConvertBytesToHexStr(actual));
                    Log.Comment("Expected : " + ConvertBytesToHexStr(expected));
                    return false;
                }
            }

            return true;
        }
    }
}