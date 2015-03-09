using System;
using System.IO;
using System.Security.Cryptography;

public class MaskGeneration {


    public static Boolean Test() {
    	bool bRes = true;

       	Console.WriteLine("Easy as");

    	PKCS1MaskGenerationMethod mgm = new PKCS1MaskGenerationMethod();

    	Console.WriteLine("One...");
    	byte[] seed1 = new byte[10] {0,1,2,3,4,5,6,7,8,9};
    	byte[] res1 = mgm.GenerateMask(seed1, 4);

    	if ( (res1 == null) || (res1.Length != 4) ) {
    		Console.WriteLine("ERROR while attempt #1");
    		bRes = false;
    	}

    	Console.WriteLine("Two...");
    	mgm.HashName = "MD5";
    	byte[] seed2 = new byte[2] {4,3};
    	byte[] res2 = mgm.GenerateMask(seed2, 16);

    	if ( (res2 == null) || (res2.Length != 16) ) {
    		Console.WriteLine("ERROR while attempt #2");
    		bRes = false;
    	}

    	Console.WriteLine("Three...");
    	mgm.HashName = "SHA1";
       	byte[] seed3 = new byte[100000];
       	for(int i=0; i<100000; i++) seed3[i] = (byte)(i%256);
    	byte[] res3 = mgm.GenerateMask(seed3, 65536);

    	if ( (res3 == null) || (res3.Length != 65536) ) {
    		Console.WriteLine("ERROR while attempt #3");
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
