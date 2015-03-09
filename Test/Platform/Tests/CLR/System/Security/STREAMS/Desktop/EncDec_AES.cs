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

        if (AesCSPSupported())
		{
			Console.WriteLine("Testing AesCryptoServiceProvider encrypt/decrypt...");
			AesCryptoServiceProvider     aescsp = new AesCryptoServiceProvider();
			ed = new EncDec();
			edm = new EncDecMul();

			bResult = ed.TestAlgorithm(aescsp);
			bResult = edm.TestAlgorithm(aescsp) && bResult;
		}

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

	// AesCryptoServiceProvider is only supported in WinXP and higher (v5.1+)
	//
	public const int AesCSPSupportedMajorVer = 5;
	public const int AesCSPSupportedMinorVer = 1;
	
	public static bool AesCSPSupported()
	{
		int major = Environment.OSVersion.Version.Major;
		int minor = Environment.OSVersion.Version.Minor;

		if (major > AesCSPSupportedMajorVer)
			return true;

		if ((major == AesCSPSupportedMajorVer) && (minor >= AesCSPSupportedMinorVer))
			return true;

		return false;
	}

}
