using System;
using System.Security.Cryptography; 
using System.IO; 


class Sim_SHA1
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
    Byte[] Data = {7,6,5,4,3,2,1,0};
    Byte[] Data1 = {7,6,5,4,3,2,1,1};

    SHA1 sha1 = (SHA1)new SHA1CryptoServiceProvider();

    sha1.ComputeHash(Data);
    PrintByteArray(sha1.Hash);
    
   	Byte[] hash1 = new Byte[sha1.Hash.Length];
   	sha1.Hash.CopyTo(hash1, 0);

    sha1.ComputeHash(Data);
    PrintByteArray(sha1.Hash);
    
    if (!Compare(sha1.Hash, hash1)) {
    	Console.WriteLine("FAILURE: 1");
	    return false;
	}
    
    sha1.ComputeHash(Data1);
    
    if (Compare(sha1.Hash, hash1)) {
    	Console.WriteLine("FAILURE: 2");
    	return false;
    }
    
    
    SHA1 sha2 = (SHA1)new SHA1CryptoServiceProvider();

    sha2.ComputeHash(Data);
    PrintByteArray(sha2.Hash);

    if (!Compare(sha2.Hash, hash1)) {
    	Console.WriteLine("FAILURE: 3");
	    return false;
    }

    return true;
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

