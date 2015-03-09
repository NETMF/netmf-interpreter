using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{

    public static class KeySizeTest
    {
        public static bool DoKeySizeTest(bool isEmulator)
        {
            try
            {
                using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
                {
                    if (!TestSizes(aes))
                        return false;
                }

                if (isEmulator)
                {
                    using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider("Emulator_Crypto"))
                    {
                        if (!TestSizes(aes))
                            return false;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Log.Exception("Fail: got an exception", e);
                return false;
            }
        }

        private static bool TestSizes(AesCryptoServiceProvider aes)
        {
            bool aes128 = false;
            bool aes192 = false;
            bool aes256 = false;

            foreach (KeySizes keySize in aes.LegalKeySizes)
            {
                int size = keySize.MinSize;
                do
                {
                    if (size == 128)
                        aes128 = true;
                    else if (size == 192)
                        aes192 = true;
                    else if (size == 256)
                        aes256 = true;

                    size += keySize.SkipSize;
                } while (size <= keySize.MaxSize && keySize.SkipSize != 0);
            }

            if (!aes128 || !aes192 || !aes256)
            {
                return false;
            }

            aes.KeySize = 128;
            aes.KeySize = 192;
            aes.KeySize = 256;

            foreach (KeySizes blockSize in aes.LegalBlockSizes)
            {
                int size = blockSize.MinSize;
                do
                {
                    if (size != 128)
                    {
                        Log.Comment("Unexpected block size found, fail");
                        return false;
                    }
                } while (size <= blockSize.MaxSize && blockSize.SkipSize != 0);
            }

            return true;
        }
    }


}