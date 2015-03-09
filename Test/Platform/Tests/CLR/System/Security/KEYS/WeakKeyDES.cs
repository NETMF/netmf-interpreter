using System;
using System.Security.Cryptography; 
using System.IO; 

class WeakKeyDES
{

    static Boolean Test()
    {
        Byte[]  PlainText = {0, 1, 2, 3, 4, 5, 6, 7}; //, 8, 9, 10, 11, 12, 13, 14, 15};
        Byte[]  Key = {1, 1, 1, 1, 1, 1, 1, 1};
        Byte[]  IV = {0, 0, 0, 0, 0, 0, 0, 0};
        DESCryptoServiceProvider     des = new DESCryptoServiceProvider();
        des.Key = Key;
        
        return false;
    }

    public static void Main(String[] args) 
    {

        try {
            
            if (Test())
            {
                Console.WriteLine("FAILED");
                Environment.ExitCode = 101;
            } else {
                Console.WriteLine("FAILED");
                Environment.ExitCode = 101;
            }

        }
        catch(Exception e) {
            Console.Write("CORRECT: Exception: {0}", e.ToString());
            Environment.ExitCode = 100;
        }
        return;
    }
}
