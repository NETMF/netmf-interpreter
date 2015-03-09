// known test vectors test for HMACSHA1
//	test vectors came from rfc2202
//
using System;
using System.Security.Cryptography; 
using System.IO; 


class Hash_HMACSHA1known
{

    static Boolean Compare(Byte[] rgb1, Byte[] rgb2) { 
        int     i;
        if (rgb1.Length != rgb2.Length) return false;
        for (i=0; i<rgb1.Length; i++) {
            if (rgb1[i] != rgb2[i]) return false;
        }
        return true;
    }

    
    static void PrintByteArray(Byte[] arr)
    {
        int i;
        for (i=0; i<arr.Length; i++) {
            Console.Write(arr[i] + "    ");
            if ( (i+9)%8 == 0 ) Console.WriteLine();
        }
        if (i%8 != 0) Console.WriteLine();
    }

    static Boolean Test()
    {
        Boolean bRes = true;
        Byte[] abKey1 = {0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b};
        Byte[] abData1 = (new System.Text.ASCIIEncoding()).GetBytes("Hi There");
        Byte[] abDigest1 = {0xb6, 0x17, 0x31, 0x86, 0x55, 0x05, 0x72, 0x64, 0xe2, 0x8b, 0xc0, 0xb6, 0xfb, 0x37, 0x8c, 0x8e, 0xf1, 0x46, 0xbe, 0x00};
        Byte[] abKey2 = (new System.Text.ASCIIEncoding()).GetBytes("Jefe");
        Byte[] abData2 = (new System.Text.ASCIIEncoding()).GetBytes("what do ya want for nothing?");
        Byte[] abDigest2 = {0xef, 0xfc, 0xdf, 0x6a, 0xe5, 0xeb, 0x2f, 0xa2, 0xd2, 0x74, 0x16, 0xd5, 0xf1, 0x84, 0xdf, 0x9c, 0x25, 0x9a, 0x7c, 0x79};

        Console.WriteLine("Testing rc21 hash...");
        HMACSHA1 rc21 = new HMACSHA1(abKey1);
        HMACSHA1 rc22 = new HMACSHA1(abKey2);
        rc21.ComputeHash(abData1);
        rc22.ComputeHash(abData2);
        Console.WriteLine("The computed hash #1 is : ");
        PrintByteArray(rc21.Hash);
        Console.WriteLine("The correct hash #1 is : ");
        PrintByteArray(abDigest1);
        if(Compare(rc21.Hash, abDigest1)) {
            Console.WriteLine("CORRECT");
        } else {
            Console.WriteLine("INCORRECT");
            bRes = false;
        }
        Console.WriteLine("The computed hash #2 is : ");
        PrintByteArray(rc22.Hash);
        Console.WriteLine("The correct hash #2 is : ");
        PrintByteArray(abDigest2);
        if(Compare(rc22.Hash, abDigest2)) {
            Console.WriteLine("CORRECT");
        } else {
            Console.WriteLine("INCORRECT");
            bRes = false;
        }
        
        return bRes;
    }

    public static void Main(String[] args) 
    {

        try {
            
            if (Test()) {
                Console.WriteLine("PASSED");
                Environment.ExitCode = 100;
            } else {
                Console.WriteLine("FAILED");
                Environment.ExitCode = 101;
            }

        }
        catch(Exception e) {
            Console.WriteLine();
            Console.Write("Exception: {0}", e.ToString());
            Environment.ExitCode = 101;
        }
        return;
    }
}

