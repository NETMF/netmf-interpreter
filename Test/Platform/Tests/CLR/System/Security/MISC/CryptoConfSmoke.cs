//
//	This is a CryptoConfig system test.
//
using System;
using System.IO;
using System.Security.Cryptography;

public class cryptoconf {


    public static Boolean Test() {

    	Object o = CryptoConfig.CreateFromName("SHA1");

    	Console.WriteLine(o.ToString());
    	
    	if (o is SHA1) return true;
    
        return false;
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
