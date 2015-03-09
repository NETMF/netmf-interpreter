using System;
using System.Security.Cryptography;

public static class KeyTest
{
    public static bool DoKeyTest()
    {
        if (!TestKeyGeneration())
        {
            return false;
        }

        if (!TestKeySet())
        {
            return false;
        }

        return true;
    }

    private static bool CompareBytes(byte[] lhs, byte[] rhs)
    {
        if (lhs.Length != rhs.Length)
            return false;

        for (int i = 0; i < lhs.Length; i++)
        {
            if (lhs[i] != rhs[i])
                return false;
        }

        return true;
    }

    private static bool TestKeyGeneration()
    {
        try
        {
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {

                // Try creating the other sized keys
                aes.KeySize = 128;
                if (aes.Key.Size != 128)
                    return false;

                // Make sure consecutive reads don't change the key
                aes.GenerateKey();
                byte[] read1 = aes.Key.ExportKey(true);
                byte[] read2 = aes.Key.ExportKey(true);

                if (!CompareBytes(read1, read2))
                    return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool TestKeySet()
    {
        try
        {
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                byte[] key = new byte[aes.KeySize / 8];
                for (int i = 0; i < key.Length; i++)
                    key[i] = (byte)i;

                aes.Key = CryptoKey.ImportKey(aes.Session, key, CryptoKey.KeyClass.Secret, CryptoKey.KeyType.AES, true);
                byte[] aesKey = aes.Key.ExportKey(true);

                if (!CompareBytes(aesKey, key))
                    return false;


                byte[] key192 = new byte[192 / 8];
                for (int i = 0; i < key192.Length; i++)
                    key192[i] = (byte)i;

                aes.Key = CryptoKey.ImportKey(aes.Session, key192, CryptoKey.KeyClass.Secret, CryptoKey.KeyType.AES, true);
                byte[] aesKey192 = aes.Key.ExportKey(true);

                if (!CompareBytes(aesKey192, key192))
                    return false;

                if (aes.KeySize != 192)
                    return false;

                aes.KeySize = 192;

                aesKey192 = aes.Key.ExportKey(true);

                if (CompareBytes(aesKey192, key192))
                    return false;

                aes.KeySize = 256;
                aesKey = aes.Key.ExportKey(true);

                if (aesKey.Length * 8 != 256)
                    return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}

