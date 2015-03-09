using System;
using System.Security.Cryptography;
using Microsoft.SPOT.Cryptoki;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{

    public class AnsiX923 : PaddingTestBase, IMFTestInterface
    {
        /// <summary>
        ///		Get an array of data to test padding on when it is encrypted
        /// </summary>
        protected override byte[][] GetData()
        {
            return new byte[][] {
			new byte[] {0x00},
			new byte[] {0x00, 0x01},
			new byte[] {0x00, 0x01, 0x02},
			new byte[] {0x00, 0x01, 0x02, 0x03},
			new byte[] {0x00, 0x01, 0x02, 0x03, 0x04},
			new byte[] {0x00, 0x01, 0x02, 0x03, 0x04, 0x05},
			new byte[] {0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06},
			new byte[] {0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07},
			new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00},
			new byte[] {0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x05},
			new byte[] {0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x04},
			new byte[] {0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
						0x09, 0x10, 0x11, 0x12, 0x13, 0x14},
			new byte[] {0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
						0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x04},
			new byte[] {0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
						0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x03}
		};
        }

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
        public MFTestResults AnsiX923_Test()
        {
            bool bRes = true;

            try
            {
                AnsiX923 tester = new AnsiX923();

                using (Session sess = new Session("", MechanismType.AES_ECB))
                {
                    bRes &= tester.RunTests(sess, PaddingMode.ANSIX923);
                }

                if (m_isEmulator)
                {
                    using (Session sess = new Session("Emulator_Crypto", MechanismType.AES_ECB))
                    {
                        bRes &= tester.RunTests(sess, PaddingMode.ANSIX923);
                    }
                }
            }
            catch (NotSupportedException)
            {
                return MFTestResults.Skip;
            }
            catch (Exception e)
            {
                Log.Exception("", e);
                bRes = false;
            }
            return bRes ? MFTestResults.Pass : MFTestResults.Fail;
        }

        /// <summary>
        ///		Method to check that the padding given is the padding that was expected
        /// </summary>
        /// <param name="data">data that was encrypted</param>
        /// <param name="padding">padding that was generated</param>
        /// <returns>true if the padding was correct, false otherwise</returns>
        protected override bool CheckPadding(Session session, byte[] data, byte[] padding)
        {
            int unPadded = (data.Length % 8);
            int padLen = 8 - unPadded;

            // make sure there was enough padding
            if (padding.Length != padLen)
            {
                Log.Comment("ERROR! Padding length incorrect, expected " + padLen + " got " + padding.Length);
                return false;
            }

            // make sure that all but the last byte of padding is a zero
            for (int i = 0; i < padding.Length - 1; i++)
            {
                if (padding[i] != 0)
                {
                    Log.Comment("ERROR! Padding Byte != 0 - " + padding[i]);
                    return false;
                }
            }

            // the last byte of the padding should be the length of the pad
            if (padding[padding.Length - 1] != padLen)
            {
                Log.Comment("ERROR! Padding length byte incorrect, expected " + padLen + " got " + padding[padding.Length - 1]);
                return false;
            }

            return true;
        }
    }
}