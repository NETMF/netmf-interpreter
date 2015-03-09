using System;
using System.Security.Cryptography; 
using System.IO; 

class SimpleEncDecMulDES
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

    static Boolean Test()
    {

        Byte[]  PlainText = {0, 1, 2, 3, 4, 5, 6, 7}; //, 8, 9, 10, 11, 12, 13, 14, 15};
        Byte[]  Key = {1, 1, 1, 1, 1, 1, 1, 1};
        Byte[]  IV = {1, 1, 1, 1, 1, 1, 1, 1};
        
        Console.WriteLine("Encrypting the following bytes:");
        PrintByteArray(PlainText);
        
        DESCryptoServiceProvider     des = new DESCryptoServiceProvider();
        des.Mode = CipherMode.ECB;
//        des.FeedbackSize = 0;
		des.Padding = PaddingMode.PKCS7;


		
		int depth = 10;


        Stream[] encStream_arr = new Stream[depth];
        Stream[] decStream_arr = new Stream[depth];
        ICryptoTransform[] decTrans_array = new ICryptoTransform[depth];

        encStream_arr[0] = new MemoryStream();
        decStream_arr[0] = new MemoryStream();

        for (int i=1; i<depth; i++) {
            des.GenerateKey();
            des.GenerateIV();
            encStream_arr[i] = new CryptoStream(encStream_arr[i-1],des.CreateEncryptor(),CryptoStreamMode.Write);
            decTrans_array[i] = des.CreateDecryptor();
        }

        for (int j=1; j<depth; j++) {
            decStream_arr[j] = new CryptoStream(decStream_arr[j-1],decTrans_array[depth-j],CryptoStreamMode.Write);
        }


        Console.WriteLine("DES default key size = " + des.KeySize);
        Console.WriteLine("SSE mode = " + des.Mode);

        encStream_arr[depth-1].Write(PlainText,0,PlainText.Length);
        ((CryptoStream)encStream_arr[depth-1]).FlushFinalBlock();
        byte[] CipherText = ((MemoryStream)encStream_arr[0]).ToArray();
        encStream_arr[depth-1].Close();

        Console.WriteLine("Cyphertext:");
        PrintByteArray(CipherText);
        

        Console.WriteLine("Decrypting...");

        Console.WriteLine("SSD mode = " + des.Mode);

        decStream_arr[depth-1].Write(CipherText,0,CipherText.Length);
        ((CryptoStream)decStream_arr[depth-1]).FlushFinalBlock();
        byte[] NewPlainText = ((MemoryStream)decStream_arr[0]).ToArray();
        decStream_arr[depth-1].Close();

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
            
            if (Test())
            {
                Console.WriteLine("PASSED");
                Environment.ExitCode = (100);
            } else {
                Console.WriteLine("FAILED");
                Environment.ExitCode = (1);
            }

        }
        catch(Exception e) {
            Console.Write("Exception: {0}", e.ToString());
            Environment.ExitCode = (1);
        }
        return;
    }
}
