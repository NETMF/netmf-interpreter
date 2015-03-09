using System.Security.Cryptography;
using System.IO;
using System;

public class RSATest {

    static void PrintByteArray(Byte[] arr)
    {
        int i;
        Console.WriteLine("Length: " + arr.Length);
        for (i=0; i<arr.Length; i++) {
            Console.Write("{0:X2}", arr[i]);
            Console.Write("  ");
            if ( (i+9)%8 == 0 ) Console.WriteLine();
        }
        if (i%8 != 0) Console.WriteLine();
    }

    static bool CompareByteArray(byte[] arr1, byte[] arr2) {
        if (arr1.Length != arr2.Length) return false;
        for (int i = 0; i < arr1.Length; i++) {
            if (arr1[i] != arr2[i]) return false;
        }
        return true;
    }


    public static void Main(String[] args) {
        RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
        RijndaelManaged aes = new RijndaelManaged();
        aes.KeySize = 128; //  192;
        aes.Key = new byte[] {0xC7, 0x42, 0xD1, 0x37, 0x4B, 0xAC, 0xFE, 0x94,
                              0x9D, 0x59, 0x79, 0x92, 0x71, 0x48, 0xD6, 0x8E};
//                              ,1,2,3,4,5,6,7,8};

        byte[] aesKey = aes.Key;
        if (args.Length > 0) {
            FileStream fs = new FileStream(args[0], FileMode.Open);
            StreamReader sr = new StreamReader(fs);
            String xmlString = sr.ReadToEnd();
            rsa.FromXmlString(xmlString);
        } else {
            FileStream fs = new FileStream("rsa.xml", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(rsa.ToXmlString(true));
            sw.Close();
        }
        Console.WriteLine("RSA Key is:\n"+rsa.ToXmlString(true));

        Console.WriteLine("AES Key is:");
        PrintByteArray(aesKey);
        byte[] encryptedKey1 = rsa.Encrypt(aesKey, true);
        Console.WriteLine("Encrypted AES Key is:");
        PrintByteArray(encryptedKey1);
        byte[] decryptedKey1 = rsa.Decrypt(encryptedKey1, true);
        Console.WriteLine("Decrypted AES Key is:");
        PrintByteArray(decryptedKey1);

        byte[] encryptedKey2 = rsa.Encrypt(aesKey, false);
        Console.WriteLine("Encrypted2 AES Key is:");
        PrintByteArray(encryptedKey2);
        byte[] decryptedKey2 = rsa.Decrypt(encryptedKey2, false);
        Console.WriteLine("Decrypted AES Key is:");
        PrintByteArray(decryptedKey2);

    }
}
