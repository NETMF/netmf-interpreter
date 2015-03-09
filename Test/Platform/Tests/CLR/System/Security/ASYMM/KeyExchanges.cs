using System;
using System.Security.Cryptography;

class KeyExchanges {


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



    public static Boolean Test()
    {
        Boolean bRes = true;

		RSAPKCS1KeyExchangeFormatter pcef1 = new RSAPKCS1KeyExchangeFormatter(RSA.Create());
		RSAPKCS1KeyExchangeDeformatter pced1 = new RSAPKCS1KeyExchangeDeformatter(RSA.Create());
		Console.WriteLine("pcef1 parameters: " + pcef1.Parameters + "\npced1 parameters: " + pced1.Parameters);

		bRes = TestKeyExchange(pcef1, pced1, false) && bRes;

		RSA rsa = RSA.Create();
		RandomNumberGenerator rng = new RNGCryptoServiceProvider();
		RSAPKCS1KeyExchangeFormatter pcef2 = new RSAPKCS1KeyExchangeFormatter();
		RSAPKCS1KeyExchangeDeformatter pced2 = new RSAPKCS1KeyExchangeDeformatter(rsa);
		RSA rsa1 = RSA.Create();
		rsa1.ImportParameters(rsa.ExportParameters(false));
		pcef2.SetKey(rsa1);
		pcef2.Rng = rng;
		pced2.RNG = rng;
		Console.WriteLine("pcef2 parameters: " + pcef2.Parameters + "\npced2 parameters: " + pced2.Parameters);

		bRes = TestKeyExchange(pcef2, pced2, true) && bRes;

		RSAOAEPKeyExchangeFormatter ocef1 = new RSAOAEPKeyExchangeFormatter(RSA.Create());
		RSAOAEPKeyExchangeDeformatter oced1 = new RSAOAEPKeyExchangeDeformatter(RSA.Create());
		Console.WriteLine("ocef1 parameters: " + ocef1.Parameters + "\noced1 parameters: " + oced1.Parameters);

		bRes = TestKeyExchange(ocef1, oced1, false) && bRes;

		rsa = RSA.Create();
		rng = new RNGCryptoServiceProvider();
		RSAOAEPKeyExchangeFormatter ocef2 = new RSAOAEPKeyExchangeFormatter();
		RSAOAEPKeyExchangeDeformatter oced2 = new RSAOAEPKeyExchangeDeformatter(rsa);
		rsa1 = RSA.Create();
		rsa1.ImportParameters(rsa.ExportParameters(false));
		ocef2.SetKey(rsa1);
		ocef2.Rng = rng;
//		oced2.RNG = rng;
		Console.WriteLine("ocef2 parameters: " + ocef2.Parameters + "\noced2 parameters: " + oced2.Parameters);

		bRes = TestKeyExchange(ocef2, oced2, true) && bRes;

        return bRes;
    }

	public static bool TestKeyExchange(AsymmetricKeyExchangeFormatter f, AsymmetricKeyExchangeDeformatter d, bool expct)
	{
		bool bRes = true;

		Random rnd = new Random();
		int len = rnd.Next(12)+5;
        byte[] data = new byte[len];

		byte[] exc = f.CreateKeyExchange(data);
		byte[] exct = f.CreateKeyExchange(data, typeof(RC2CryptoServiceProvider));

		try 
		{
			byte[] res = d.DecryptKeyExchange(exc);
			byte[] rest = d.DecryptKeyExchange(exct);

			if (!Compare(res, data)) 
			{
				Console.WriteLine("KeyExchangeFormatter/Deformatter failed to roundtrip #1");
				bRes = false;
			}

			if (!Compare(rest, data)) 
			{
				Console.WriteLine("KeyExchangeFormatter/Deformatter failed to roundtrip #2");
				bRes = false;
			}
		}
		catch(CryptographicException e)
		{
			Console.Write("EXCEPTION: " + e.Message);
			bRes = false;
		}

		bRes = (bRes==expct);

		if (bRes)
			Console.WriteLine("OK  (expct was " + expct + ")");
		else
			Console.WriteLine("FAIL  (expct was " + expct + ")");

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




