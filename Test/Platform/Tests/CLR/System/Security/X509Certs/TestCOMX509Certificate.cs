using System;
using System.IO ;
using System.Threading;
using System.Security ;
using System.Security.Permissions ;
using System.Security.Cryptography.X509Certificates ; 
using System.Security.Cryptography ;
using System.Collections ; 

public class Test
{
	static Oid oid = new Oid( "SHA1" ) ; 
	static RSACryptoServiceProvider rsa = new RSACryptoServiceProvider() ; 
	public static int Main()
		{
		int res = 0 ; 
		res |= TestGetFriendlyNameFromOid() ; 
		Console.WriteLine( "Result is {0}:  {1}" , res , res == 0 ? "Test Passed" : "Test Failed" ) ; 
		return res == 0 ? 100 : res ; 
		}
	static int TestGetFriendlyNameFromOid()
		{
		byte[] data = {1,2,3,4,5,6,7,8,9,0} ;
		byte[] signature = rsa.SignData( data , oid.Value ) ; 
		bool res = rsa.VerifyData( data , oid.FriendlyName , signature ) ; 
		return res ? 0 : 0x1 ; 
		}
}
