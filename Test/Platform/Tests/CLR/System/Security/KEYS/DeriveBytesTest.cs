using System;
using System.Security.Cryptography; 
using System.IO; 
using System.Text;

class DeriveBytesTest
{
    static void PrintByteArray(Byte[] arr)
    {
        int i;
        Console.WriteLine("Length: " + arr.Length);
        for (i=0; i<arr.Length; i++) {
            Console.Write("{0:X}", arr[i]);
            Console.Write("    ");
            if ( (i+9)%8 == 0 ) Console.WriteLine();
        }
        if (i%8 != 0) Console.WriteLine();
    }

    public static void Main() {
        string PlainText = "Titan";
        byte[] PlainBytes = new byte[5];
        PlainBytes = Encoding.ASCII.GetBytes(PlainText.ToCharArray());
        PrintByteArray(PlainBytes);
        byte[] CipherBytes = new byte[8];
        PasswordDeriveBytes pdb = new PasswordDeriveBytes("Titan", null);
        byte[] IV = new byte[8];
        byte[] Key = pdb.CryptDeriveKey("RC2", "SHA1", 40, IV);
        PrintByteArray(Key);
        PrintByteArray(IV);

        // Now use the data to encrypt something
        RC2CryptoServiceProvider rc2 = new RC2CryptoServiceProvider();
        Console.WriteLine(rc2.Padding);
        Console.WriteLine(rc2.Mode);
        ICryptoTransform sse = rc2.CreateEncryptor(Key, IV);
        MemoryStream ms = new MemoryStream();
        CryptoStream cs1 = new CryptoStream(ms, sse, CryptoStreamMode.Write);
        cs1.Write(PlainBytes, 0, PlainBytes.Length);
        cs1.FlushFinalBlock();
        CipherBytes = ms.ToArray();
        cs1.Close();
        Console.WriteLine(Encoding.ASCII.GetString(CipherBytes));
        PrintByteArray(CipherBytes);

        ICryptoTransform ssd = rc2.CreateDecryptor(Key, IV);
        CryptoStream cs2 = new CryptoStream(new MemoryStream(CipherBytes), ssd, CryptoStreamMode.Read);
        byte[] InitialText = new byte[5];
        cs2.Read(InitialText, 0, 5);
        Console.WriteLine(Encoding.ASCII.GetString(InitialText));
    	PrintByteArray(InitialText);
    }
}

