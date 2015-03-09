using System;
using System.Security.Cryptography; 
using System.IO; 


class RijndaelEncDec
{

    static Boolean Test()
    {
        Boolean bResult;

        Console.WriteLine("Testing AesManaged encrypt/decrypt...");
        AesManaged     aes = new AesManaged();
        EncDec      ed = new EncDec();
        EncDecMul   edm = new EncDecMul();

        bResult = ed.TestAlgorithm(aes);
        bResult = edm.TestAlgorithm(aes) && bResult;

        return bResult;
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
