using System;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography ;
using System.Security.Permissions ; 
using System.Security ; 
using System.IO ; 
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Collections ; 
using System.Threading ; 
using System.Diagnostics ; 
using System.Text ; 

public class Test
{
	public static int Main()
		{
		int rv = 100 ; 
		rv |= TestOid( "3.3.100" ) ; 
		rv |= TestOid( "3" ) ; 
		rv |= TestOid( "0" ) ; 
		rv |= TestOid( "1" ) ; 
		rv |= TestOid( "2" ) ; 
		rv |= TestOid( "" ) ; 
		rv |= TestOid( "3.3.3.3.3.3.3" ) ; 
		rv |= TestOid( "1." ) ; 
		rv |= TestOid( "." ) ; 
		rv |= TestOid( "......" ) ; 
		rv |= TestOid( "   " ) ; 
		string t = "1."  ; 
		for( int i = 0 ; i < 10 ; i++ )
			t += t ;
		t+= "2" ; 
		rv |= TestOid( t ) ; 
		Console.WriteLine( rv == 100 ? "Test Passed" : "Test Failed" ) ; 
		return rv ; 
		}
	static int TestOid( string oidValue )
		{
		try
			{
			Oid oid = new Oid() ; 
			oid.Value = oidValue ; 
			Console.WriteLine( oid.Value.Length ) ; 
			Console.WriteLine( oid.FriendlyName ) ; 
			return 100 ; 
			}
		catch( Exception  e )
			{
			Console.WriteLine( "Failed for {0}" , oidValue ) ;
			Console.WriteLine( e ) ; 
			return 101 ; 
			}
		}
}
