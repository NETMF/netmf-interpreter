//
//	Make our own implementation of PBKDF1 and compare with PasswordDeriveBytes operation
//
//
using System;
using System.IO;
using System.Security.Cryptography;
using TestFW;
using TestFW.Logging;
using TestFW.Utility;

public class PasswordDeriveBytesTest
{
	public const int PASS_CODE = 100;
	public const int FAIL_CODE = 101;

    public static int Main(String[] args) 
    {
		Exception[] cryptoEx = new Exception[] { new CryptographicException() };

		TestGroup apiTests = new TestGroup("API tests", new TestCase[]
		{
			new TestCase("GetBytes with small values", new TestCaseImpl(GetBytesSmall)),
			new TestCase("Reset and ensure same", new TestCaseImpl(Reset)),
			new TestCase("Set and Check Hash", new TestCaseImpl(SetCheckHash)),
			new TestCase("Set and Check Salt", new TestCaseImpl(SetCheckSalt)),
			new TestCase("Set and Check IC", new TestCaseImpl(SetCheckIC)),
			new TestCase("Create with password bytes", new TestCaseImpl(CreateWithPasswordBytes)),
			new TestCase("Create with password bytes and CSP", new TestCaseImpl(CreateWithPasswordBytesCSP))
		});

		TestGroup exceptionTests = new TestGroup("Exception tests", new TestCase[]
		{
			new ExceptionTestCase("Derive key, negative key size", new ExceptionTestCaseImpl(DeriveNegKey), cryptoEx),
			new ExceptionTestCase("Derive key, bad algorithm", new ExceptionTestCaseImpl(DeriveKeyBadAlg), cryptoEx),
			new ExceptionTestCase("Derive key, null IV", new ExceptionTestCaseImpl(DeriveKeyNullIv), cryptoEx),
			new ExceptionTestCase("Null HashName", new ExceptionTestCaseImpl(NullHashName), ExceptionTestCase.ArgumentExceptions),
			new ExceptionTestCase("Set HashName after Calculating", new ExceptionTestCaseImpl(HashNameAfterCalc), cryptoEx),
			new ExceptionTestCase("Set Salt after Calculating", new ExceptionTestCaseImpl(SaltAfterCalc), cryptoEx),
			new ExceptionTestCase("Zero iteration count", new ExceptionTestCaseImpl(ZeroIC), ExceptionTestCase.ArgumentExceptions),
			new ExceptionTestCase("Negative iteration count", new ExceptionTestCaseImpl(NegativeIC), ExceptionTestCase.ArgumentExceptions),
			new ExceptionTestCase("Set iteration count after calcuating", new ExceptionTestCaseImpl(ICAfterCalc), cryptoEx)
		});

		TestGroup customTests = new TestGroup("Custom Impl tests", new TestCase[]
		{
			new TestCase("Compare with custom impl", new TestCaseImpl(CompareWithCustom))
		});

		TestGroup otherTests = new TestGroup("Other tests", new TestCase[]
		{
			new TestCase("Change hash", new TestCaseImpl(ChangeHash)),
			new TestCase("Change salt", new TestCaseImpl(ChangeSalt))
		});

		TestRunner runner = new TestRunner(new TestGroup[] { apiTests, exceptionTests, customTests, otherTests }, new ILogger[] { new ConsoleLogger(), new XmlFailureLogger() });
        return runner.Run() ? PASS_CODE : FAIL_CODE;
    }

#region API tests

	/// <summary>
	///		GetBytes with small values
	/// </summary>
	private static bool GetBytesSmall()
	{
		string password = "pwd";
		byte[] salt		= new byte[] {0, 1, 2, 3, 4, 5, 6, 7};
		int	   c		= 5;

    	PasswordDeriveBytes pdb = new PasswordDeriveBytes(password, salt, "SHA1", c);
    	pdb.Salt = salt;

    	byte[] res1 = pdb.GetBytes(1);
    	byte[] res2 = pdb.GetBytes(2);

		return
			res1 != null && res1.Length == 1 &&
			res2 != null && res2.Length == 2;
	}

	/// <summary>
	///		Reset and ensure same
	/// </summary>
	private static bool Reset()
	{
		string password = "pwd";
		byte[] salt = new byte[] {0, 1, 2, 3, 4, 5, 6, 7};
		
		PasswordDeriveBytes pdb = new PasswordDeriveBytes(password, salt, "SHA512", 1025);
		byte[] res1 = pdb.GetBytes(1025);

		pdb.Reset();
		byte[] res2 = pdb.GetBytes(1025);

		return Util.CompareBytes(res1, res2);
	}

	/// <summary>
	///		Set and Check Hash
	/// </summary>
	private static bool SetCheckHash()
	{
		string password = "A password that's longer than the others";
		byte[] salt = new byte[] { 0xF0, 0xF1, 0xF2, 0xF3, 0xF4, 0xF5, 0xF6, 0xFF };

		PasswordDeriveBytes pdb1 = new PasswordDeriveBytes(password, salt);
		pdb1.HashName = "SHA512";

		// make sure the property took
		if(pdb1.HashName != "SHA512")
			return false;

		PasswordDeriveBytes pdb2 = new PasswordDeriveBytes(password, salt, "SHA512", pdb1.IterationCount);

		return Util.CompareBytes(pdb1.GetBytes(1025), pdb2.GetBytes(1025));
	}

    /// <summary>
	///		Set and Check Salt
	/// </summary>
	private static bool SetCheckSalt()
	{
		string password = "A password that's longer than the others";
		byte[] salt = new byte[] { 0xF0, 0xF1, 0xF2, 0xF3, 0xF4, 0xF5, 0xF6, 0xFF };

		PasswordDeriveBytes pdb1 = new PasswordDeriveBytes(password, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 });
		pdb1.Salt = salt;

		// make sure the property took
		if(!Util.CompareBytes(pdb1.Salt, salt))
			return false;

		PasswordDeriveBytes pdb2 = new PasswordDeriveBytes(password, salt);

		return Util.CompareBytes(pdb1.GetBytes(1025), pdb2.GetBytes(1025));
	}

	/// <summary>
	/// 	Set and Check IC
	/// </summary>
	private static bool SetCheckIC()
	{
		string password = "A password that's longer than the others";
		byte[] salt = new byte[] { 0xF0, 0xF1, 0xF2, 0xF3, 0xF4, 0xF5, 0xF6, 0xFF };

		PasswordDeriveBytes pdb1 = new PasswordDeriveBytes(password, salt);
		pdb1.IterationCount = 1025;

		// make sure the property took
		if(pdb1.IterationCount != 1025)
			return false;

		PasswordDeriveBytes pdb2 = new PasswordDeriveBytes(password, salt, pdb1.HashName, 1025);

		return Util.CompareBytes(pdb1.GetBytes(1025), pdb2.GetBytes(1025));
	}

	/// <summary>
	///		Create with password bytes
	/// </summary>
	private static bool CreateWithPasswordBytes()
	{
		string password = "password";
		byte[] salt = new byte[] { 0x21, 0x05, 0x33, 0x27, 0x16, 0x07, 0x08 };

		PasswordDeriveBytes pdb1 = new PasswordDeriveBytes(password, salt);
		PasswordDeriveBytes pdb2 = new PasswordDeriveBytes(System.Text.Encoding.UTF8.GetBytes(password), salt);

		return Util.CompareBytes(pdb1.GetBytes(1025), pdb2.GetBytes(1025));
	}

	/// <summary>
	///		Create with password bytes and CSP
	/// </summary>
	private static bool CreateWithPasswordBytesCSP()
	{
		string password = "password";
		byte[] salt = new byte[] { 0x21, 0x05, 0x33, 0x27, 0x16, 0x07, 0x08 };
		CspParameters csp = new CspParameters();

		PasswordDeriveBytes pdb1 = new PasswordDeriveBytes(password, salt, csp);
		PasswordDeriveBytes pdb2 = new PasswordDeriveBytes(System.Text.Encoding.UTF8.GetBytes(password), salt, csp);

		return Util.CompareBytes(pdb1.GetBytes(1025), pdb2.GetBytes(1025));
	}
#endregion
	
#region Exception Tests

    /// <summary>
	///		Derive key, negative key size
	/// </summary>
	private static void DeriveNegKey()
	{
		string password = "pwd";
		byte[] salt = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };

		PasswordDeriveBytes pdb = new PasswordDeriveBytes(password, salt);
		pdb.CryptDeriveKey("RC2", "MD5", -21, new byte[] { 0, 1, 2, 3, 4, 5 });
		return;
	}

	/// <summary>
	/// 	Derive key, bad algorithm
	/// </summary>
	private static void DeriveKeyBadAlg()
	{
		string password = "pwd";
		byte[] salt = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };

		PasswordDeriveBytes pdb = new PasswordDeriveBytes(password, salt);
		pdb.CryptDeriveKey("MD5", "MD5", 21, new byte[] { 0, 1, 2, 3, 4, 5 });
		return;
	}

	/// <summary>
	///		Derive key, null IV
	/// </summary>
	private static void DeriveKeyNullIv()
	{
		string password = "pwd";
		byte[] salt = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };

		PasswordDeriveBytes pdb = new PasswordDeriveBytes(password, salt);
		pdb.CryptDeriveKey("RC2", "MD5", 21, null);
		return;
	}

	/// <summary>
	///		Null HashName
	/// </summary>
	private static void NullHashName()
	{
		PasswordDeriveBytes pdb = new PasswordDeriveBytes("pwd", new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 });
		pdb.HashName = null;
		return;
	}

	/// <summary>
	///		Set HashName after Calculating
	/// </summary>
	private static void HashNameAfterCalc()
	{
		PasswordDeriveBytes pdb = new PasswordDeriveBytes("pwd", new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 });
		pdb.GetBytes(1);
		pdb.HashName = "MD5";
		return;
	}

	/// <summary>
	///		Set Salt after Calculating
	/// </summary>
	private static void SaltAfterCalc()
	{
		PasswordDeriveBytes pdb = new PasswordDeriveBytes("pwd", new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 });
		pdb.GetBytes(1);
		pdb.Salt = new byte[]{0, 1, 2, 3, 4, 5, 6, 7};
		return;
	}

	/// <summary>
	///		Zero iteration count
	/// </summary>
	private static void ZeroIC()
	{
		PasswordDeriveBytes pdb = new PasswordDeriveBytes("pwd", new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 });
		pdb.IterationCount = 0;
		return;
	}

	/// <summary>
	///		Negative iteration count
	/// </summary>
	private static void NegativeIC()
	{
		PasswordDeriveBytes pdb = new PasswordDeriveBytes("pwd", new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 });
		pdb.IterationCount = -21;
		return;
	}

	/// <summary>
	///		Set iteration count after calcuating
	/// </summary>
	private static void ICAfterCalc()
	{
		PasswordDeriveBytes pdb = new PasswordDeriveBytes("pwd", new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 });
		pdb.GetBytes(1);
		pdb.IterationCount = 21;
		return;
	}

#endregion
				
#region Custom Impl Tests
	/// <summary>
	///		Compare with custom impl
	/// </summary>
	private static bool CompareWithCustom()
	{
		string password = "pwd";
		byte[] salt		= new byte[] {0, 1, 2, 3, 4, 5, 6, 7};
		int	   c		= 5;
		int	   dkLen	= 8;
		byte[] res1, res2;

		res1 = MyPBKDF1(password, salt, c, dkLen, "SHA1");
		PasswordDeriveBytes pdb = new PasswordDeriveBytes(password, salt, "SHA1", c);
		res2 = pdb.GetBytes(dkLen);
			
		return Util.CompareBytes(res1, res2);
	}

	/// <summary>
	///		Compare with custom, using byte arrays
	/// </summary>
	private static bool CompareWithCustomByteArray()
	{
		byte[] salt		= new byte[] {0, 1, 2, 3, 4, 5, 6, 7};
		int	   c		= 5;
		int	   dkLen	= 8;

		byte[] pwdBytes = new byte[] { 0x00, 0xFF, 0xF0, 0x10, 0xCC, 0xD8, 0x21, 0x05, 0xFF };
		PasswordDeriveBytes pdbBytes = new PasswordDeriveBytes(pwdBytes, salt, "SHA1", c);
		byte[] pdbBytesRes = pdbBytes.GetBytes(dkLen);
		byte[] pdbBytesCmp = MyPBKDF1(pwdBytes, salt, c, dkLen, "SHA1");

		return Util.CompareBytes(pwdBytes, pdbBytesCmp);
	}
#endregion

#region Other Tests

	/// <summary>
	///		Change hash
	/// </summary>
	private static bool ChangeHash()
	{
		string password = "pwd";
		byte[] salt		= new byte[] {0, 1, 2, 3, 4, 5, 6, 7};
		int	   c		= 5;

    	PasswordDeriveBytes pdb = new PasswordDeriveBytes(password, salt, "SHA1", c);
    	pdb.HashName = "MD5";
    	byte[] res = pdb.GetBytes(16);

    	return res != null && res.Length == 16;
	}

	/// <summary>
	///		Change the salt
	/// </summary>
	private static bool ChangeSalt()
	{
		string password = "pwd";
		byte[] salt		= new byte[] {0, 1, 2, 3, 4, 5, 6, 7};
		int	   c		= 5;

    	PasswordDeriveBytes pdb = new PasswordDeriveBytes(password, salt, "SHA1", c);
    	pdb.Salt = new byte[7] {7, 6, 5, 4, 3, 2, 1};

    	byte[] res = pdb.GetBytes(24);
    	return res != null && res.Length == 24;
	}
#endregion

#region IvanMed custom implementation of PBKDF1
	
    public static Byte[] MyPBKDF1(String pwd, Byte[] salt, int c, int dkLen, String alg)
    {
    	Byte[] pwd_bytes = (new System.Text.UTF8Encoding()).GetBytes(pwd);
		return MyPBKDF1(pwd_bytes, salt, c, dkLen, alg);
    }
    
    public static Byte[] MyPBKDF1(Byte[] pwd_bytes, Byte[] salt, int c, int dkLen, String alg)
    {
    	HashAlgorithm ha = HashAlgorithm.Create(alg);
    	
    	Byte[] pwd_all = new Byte[pwd_bytes.Length + salt.Length];
    	
    	Array.Copy(pwd_bytes, pwd_all, pwd_bytes.Length);
    	Array.Copy(salt, 0, pwd_all, pwd_bytes.Length, salt.Length);

    	for(int i=0; i<c; i++) {
    		pwd_all = ha.ComputeHash(pwd_all);
    	}

    	Byte[] res = new Byte[dkLen];
    	
    	Array.Copy(pwd_all, res, res.Length);
    	
    	return res;
    }
#endregion

}
