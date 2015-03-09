using System;
using System.Security.Cryptography; 
using System.IO; 

class SimpleEncDecRijndael
{
    static void PrintByteArray(Byte[] arr)
    {
        int i;
        for (i=0; i<arr.Length; i++) {
            Console.Write(arr[i] + "    ");
            if ( (i+9)%8 == 0 ) Console.WriteLine();
        }
        if (i%8 != 0) Console.WriteLine();
    }

    static Boolean Compare(Byte[] rgb1, Byte[] rgb2) { 
        int     i;
        if (rgb1.Length != rgb2.Length) return false;
        for (i=0; i<rgb1.Length; i++) {
            if (rgb1[i] != rgb2[i]) return false;
        }
        return true;
    }

    static Boolean Test(CipherMode md)
    {

        Byte[]  PlainText = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15};
        Byte[]  Key = {1, 1, 1, 1, 1, 1, 1, 1,2,2,2,2,2,2,2,2};
        Byte[]  IV = {100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115};
        
        Console.WriteLine("Encrypting the following bytes:");
        PrintByteArray(PlainText);
        
        RijndaelManaged     des = new RijndaelManaged();
        des.Mode = md;
//        des.FeedbackSize = 0;
//		des.Padding = PaddingMode.PKCS7;

        Console.WriteLine("DES default key size = " + des.KeySize);
        ICryptoTransform sse = des.CreateEncryptor(Key, IV);
        Console.WriteLine("SSE mode = " + des.Mode);
        MemoryStream ms = new MemoryStream();
        CryptoStream    cs = new CryptoStream(ms, sse, CryptoStreamMode.Write);
        cs.Write(PlainText,0,PlainText.Length);
        cs.FlushFinalBlock();
        byte[] CipherText = ms.ToArray();
        cs.Close();

        Console.WriteLine("Cyphertext:");
        PrintByteArray(CipherText);
        

        Console.WriteLine("Decrypting...");

//        RijndaelManaged     des = new RijndaelManaged();
//        des.Mode = CipherMode.ECB;
//        des.FeedbackSize = 0;
        ICryptoTransform ssd = des.CreateDecryptor(Key, IV);
        Console.WriteLine("SSD mode = " + des.Mode);
        cs = new CryptoStream(new MemoryStream(CipherText), ssd, CryptoStreamMode.Read);

        byte[] NewPlainText = new byte[PlainText.Length];
        cs.Read(NewPlainText,0,PlainText.Length);

        PrintByteArray(NewPlainText);
        
        if (!Compare(PlainText, NewPlainText)) {
        	Console.WriteLine("ERROR: roundtrip failed");
        	return false;
        }
        
        return true;
    }

    public static void Main(String[] args) 
    {

        try {
            
            if (Test(CipherMode.ECB)&&Test(CipherMode.CBC))
            {
                Console.WriteLine("PASSED");
                Environment.ExitCode = (100);
            } else {
                Console.WriteLine("FAILED");
                Environment.ExitCode = (123);
            }

        }
        catch(Exception e) {
            Console.Write("Exception: {0}", e.ToString());
            Environment.ExitCode = (123);
        }
        return;
    }
}
