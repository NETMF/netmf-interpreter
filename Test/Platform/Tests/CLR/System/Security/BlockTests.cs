using System;
using System.IO;
using System.Security.Cryptography;
using System.Collections;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{

    public static class BlockTest
    {
        public static bool DoBlockTest()
        {
            try
            {
                ArrayList inputs = new ArrayList();


                inputs.Add(GenerateInput(new int[] { 3, 1 }));
                inputs.Add(GenerateInput(new int[] { 2, 2 }));
                inputs.Add(GenerateInput(new int[] { 2, 1, 1 }));
                inputs.Add(GenerateInput(new int[] { 1, 3 }));
                inputs.Add(GenerateInput(new int[] { 1, 2, 1 }));
                inputs.Add(GenerateInput(new int[] { 1, 1, 2 }));
                inputs.Add(GenerateInput(new int[] { 1, 1, 1, 1 }));

                ArrayList results = new ArrayList();

                using (AesCryptoServiceProvider aes1 = new AesCryptoServiceProvider())
                using (AesCryptoServiceProvider aes2 = new AesCryptoServiceProvider("Emulator_Crypto"))
                {
                    aes1.Key = aes2.Key;
                    aes1.IV = aes2.IV;
                    aes1.Mode = aes2.Mode;
                    aes1.Padding = aes2.Padding;

                    for (int i = 0; i < inputs.Count; i++)
                    {
                        byte[] result = null;
                        if (!TestRoundTrip(aes1, aes2, (ArrayList)inputs[i], out result))
                        {
                            Log.Comment("Roundtrip fails");
                            return false;
                        }

                        results.Add(result);
                    }

                    for (int i = 0; i < results.Count - 1; i++)
                    {
                        if (!CompareBytes((byte[])results[i], (byte[])results[i + 1]))
                        {
                            Log.Comment("Mismatch");
                            return false;
                        }
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static ArrayList GenerateInput(int[] sizes)
        {
            ArrayList input = new ArrayList();

            byte b = 0;
            foreach (int size in sizes)
            {
                byte[] data = new byte[size * 16];
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = b;
                    b++;
                }

                input.Add(data);
            }

            if (b != 4 * 16)
                Log.Comment("Generated bad input");

            return input;
        }

        private static bool TestRoundTrip(SymmetricAlgorithm testAlgorithm, SymmetricAlgorithm baseline, ArrayList data, out byte[] result)
        {
            result = null;

            try
            {
                int len = 0;
                for(int i=0; i<data.Count; i++)
                {
                    len += ((byte[])data[i]).Length;
                }

                byte[] testCipherValue = new byte[len + testAlgorithm.BlockSize];
                byte[] baselineCipherValue = new byte[len + baseline.BlockSize];

                int testOffset = 0;
                int baseOffset = 0;

                //using (MemoryStream testEncrypted = new MemoryStream())
                //using (MemoryStream baselineEncrypted = new MemoryStream())
                using(ICryptoTransform testEncryptor = testAlgorithm.CreateEncryptor())
                using(ICryptoTransform baselineEncryptor = baseline.CreateEncryptor())
                {
                    //using (CryptoStream testEncryptor = new CryptoStream(testEncrypted, testAlgorithm.CreateEncryptor(), CryptoStreamMode.Write))
                    //using (CryptoStream baselineEncryptor = new CryptoStream(baselineEncrypted, baseline.CreateEncryptor(), CryptoStreamMode.Write))
                    //{
                        for(int i=0; i<data.Count-1; i++)
                        {
                            byte[] blocks = data[i] as byte[];
                            testOffset += testEncryptor.TransformBlock(blocks, 0, blocks.Length, testCipherValue, testOffset);
                            baseOffset += baselineEncryptor.TransformBlock(blocks, 0, blocks.Length, baselineCipherValue, baseOffset);
                            //testEncryptor.Write(blocks, 0, blocks.Length);
                            //baselineEncryptor.Write(blocks, 0, blocks.Length);
                        }

                        byte[] blks = data[data.Count-1] as byte[];

                        byte[] testFinal = testEncryptor.TransformFinalBlock(blks, 0, blks.Length);
                        byte[] baseFinal = baselineEncryptor.TransformFinalBlock(blks, 0, blks.Length);

                        Array.Copy(testFinal, 0, testCipherValue    , testOffset, testFinal.Length);
                        Array.Copy(baseFinal, 0, baselineCipherValue, baseOffset, baseFinal.Length);

                        testOffset += testFinal.Length;
                        baseOffset += baseFinal.Length;

                        //testEncryptor.Close();
                        //baselineEncryptor.Close();

                        //testCipherValue = testEncrypted.ToArray();
                        //baselineCipherValue = baselineEncrypted.ToArray();
                    //}
                }

                byte[] testRoundtrip;
                byte[] baselineRoundtrip;

                //using (MemoryStream testDecrypted = new MemoryStream())
                //using (MemoryStream baselineDecrypted = new MemoryStream())
                using(ICryptoTransform testDecryptor = testAlgorithm.CreateDecryptor())
                using(ICryptoTransform baselineDecryptor = baseline.CreateDecryptor())
                {
                    //using (CryptoStream testDecryptor = new CryptoStream(testDecrypted, testAlgorithm.CreateDecryptor(), CryptoStreamMode.Write))
                    //using (CryptoStream baselineDecryptor = new CryptoStream(baselineDecrypted, baseline.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        testRoundtrip = testDecryptor.TransformFinalBlock(baselineCipherValue, 0, baseOffset);
                        baselineRoundtrip = baselineDecryptor.TransformFinalBlock(testCipherValue, 0, testOffset);

                        //testDecryptor.Write(baselineCipherValue, 0, baselineCipherValue.Length);
                        //testDecryptor.Close();

                        //baselineDecryptor.Write(testCipherValue, 0, testCipherValue.Length);
                        //baselineDecryptor.Close();

                        //testRoundtrip = testDecrypted.ToArray();
                        //baselineRoundtrip = baselineDecrypted.ToArray();
                    }
                }

                if (!CompareBytes(testRoundtrip, baselineRoundtrip))
                {
                    Log.Comment("Roundtrip bytes do not match");
                    return false;
                }

                result = testRoundtrip;
                return true;
            }
            catch (Exception e)
            {
                Log.Exception("Got an exception, fail", e);
                return false;
            }
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
    }


}