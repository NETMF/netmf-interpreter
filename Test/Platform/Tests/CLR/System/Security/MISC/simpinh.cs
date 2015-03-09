using System;
using System.Security.Cryptography;


public class InheritanceTest
{

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


    public static Boolean Test()
	{
		Boolean bRes = true;
		try {
			MySymmetricAlgorithm msa = new MySymmetricAlgorithm();
			MyDESAlgorithm mda = new MyDESAlgorithm();
            My3DESAlgorithm m3a = new My3DESAlgorithm();
            MyRC2Algorithm mrca = new MyRC2Algorithm();
            MyRijndaelAlgorithm mra = new MyRijndaelAlgorithm();

            // to cover rc2.set_EffectiveKeySize (accessible only through inheritance)
            mrca.EffectiveKeySize = mrca.KeySize;
            Console.WriteLine("rc2.EffectiveKeySize = " + mrca.EffectiveKeySize);
		}
		catch(Exception e) {
			Console.WriteLine("Exception caught:\n" + e.ToString());
			bRes = false;
		}
		return bRes;
	}

}


public class MySymmetricAlgorithm : SymmetricAlgorithm
{
	public MySymmetricAlgorithm() : base()
	{
		Console.WriteLine("My SymmetricAlgorithm constructor");
	}

	public override ICryptoTransform CreateEncryptor(byte[] key, byte[] iv)
	{
		return null;
	}

   	public override ICryptoTransform CreateDecryptor(byte[] key, byte[] iv)
	{
		return null;
	}

	public override void GenerateKey()
	{
	}

	public override void GenerateIV()
	{
	}

}

public class MyDESAlgorithm : DES
{
	public MyDESAlgorithm() : base()
	{
		Console.WriteLine("My DES Alg constructor");
	}

	public override ICryptoTransform CreateEncryptor(byte[] key, byte[] iv)
	{
		return null;
	}

   	public override ICryptoTransform CreateDecryptor(byte[] key, byte[] iv)
	{
		return null;
	}

	public override void GenerateKey()
	{
	}

	public override void GenerateIV()
	{
	}

}

public class My3DESAlgorithm : TripleDES
{
	public My3DESAlgorithm() : base()
	{
		Console.WriteLine("My 3DES Alg constructor");
	}

	public override ICryptoTransform CreateEncryptor(byte[] key, byte[] iv)
	{
		return null;
	}

   	public override ICryptoTransform CreateDecryptor(byte[] key, byte[] iv)
	{
		return null;
	}

	public override void GenerateKey()
	{
	}

	public override void GenerateIV()
	{
	}

}


public class MyRC2Algorithm : RC2
{
	public MyRC2Algorithm() : base()
	{
		Console.WriteLine("My RC2 Alg constructor");
	}

	public override ICryptoTransform CreateEncryptor(byte[] key, byte[] iv)
	{
		return null;
	}

   	public override ICryptoTransform CreateDecryptor(byte[] key, byte[] iv)
	{
		return null;
	}

	public override void GenerateKey()
	{
	}

	public override void GenerateIV()
	{
	}

}


public class MyRijndaelAlgorithm : Rijndael
{
	public MyRijndaelAlgorithm() : base()
	{
		Console.WriteLine("My Rijndael Alg constructor");
	}

	public override ICryptoTransform CreateEncryptor(byte[] key, byte[] iv)
	{
		return null;
	}

   	public override ICryptoTransform CreateDecryptor(byte[] key, byte[] iv)
	{
		return null;
	}

	public override void GenerateKey()
	{
	}

	public override void GenerateIV()
	{
	}

}