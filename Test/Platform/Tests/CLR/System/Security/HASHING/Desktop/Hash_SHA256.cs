using System;
using System.Security.Cryptography; 
using System.IO; 


class Hash_SHA256
{

    static Boolean Test()
    {
    	Console.WriteLine("Testing SHA256 hash...");
    	SimpleHash	sh = new SimpleHash();
        SHA256 sha = new SHA256Managed();

		return sh.TestAlgorithm(sha);
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

