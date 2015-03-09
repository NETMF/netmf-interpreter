using System;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography ;
using System.Security.Permissions ; 
using System.Security ; 
using System.Net ;
using System.IO ; 
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Collections ; 
using System.Threading ; 
using System.Diagnostics ; 
using System.Text ; 
using System.Collections.Generic ; 
using X509Tester ; 
public class X509FuzzTest : Worker
{
	static string dir = String.Empty ; 
	static void ParseArgs(string[] args)
		{
		for( int i = 0 ; i < args.Length ; i+=2 )
			{
			if( args[i].ToLower()=="-dir" )
				dir = args[i+1];
			}
		}
	public static int Main(string[] args)
		{
		ParseArgs(args) ; 
		try
			{
			RunTest() ; 
			return 100 ; 
			}
		catch(Exception e)
			{
			Console.WriteLine(e);
			}
		return 101 ; 
		}
	static void RunTest()
		{
		X509FuzzTest t = new X509FuzzTest() ; 
//		X509Certificate2Collection certs = t.PreRun() ; 
//		t.PostRun(certs) ; 
		t.CreateInstances();
		t.DumpResults(false);
		}
	X509Certificate2Collection PreRun()
		{
		X509Certificate2Collection exc = Test043PreRun( false ) ; 
		PreRunCerts = exc ; 
		exit = true ; 
		return exc ; 
		}
	void PostRun(X509Certificate2Collection certs)
		{
		exit = false ; 
		Test043PostRun( false , certs ) ; 
		}
	void CreateInstances()
		{
		bool res = false ; 
		string cd = Environment.CurrentDirectory + "\\" ; 
		X509Certificate2 real = null , fuzzed = null ; 
		string test = String.Empty ; 
		Result r = (Result) 0;
		string[] realFiles = GetFiles( cd , allCerts ) ;
		for( int i = 0 ; i < realFiles.Length ; i++ )
			{			
			string fileName = realFiles[i].ToUpper().Replace( cd.ToUpper() , String.Empty ) ; 
			string[] fuzzFiles = GetFiles( dir , fileName.Substring( 0 , fileName.IndexOf(".") ) + "-*.c*r*" ) ;
			try
				{
				real = new X509Certificate2( realFiles[i] ) ;
				}
			catch(Exception)
				{
				Console.WriteLine( realFiles[i] ) ; 
				break ; 
				}
			Console.WriteLine( "Going to test {0}" , realFiles[i] ) ; 
			for( int j = 0 ; j < fuzzFiles.Length ; j++ )
				{
				res= false ; 
				if( fuzzFiles[j].ToLower().IndexOf(".tmp" ) > 0 )
					{
					j++ ; 
					if( j == fuzzFiles.Length )
						break ; 
					}
				Console.WriteLine( "     with {0}" , fuzzFiles[j] ) ; 
				try
					{
					fuzzed = new X509Certificate2( fuzzFiles[j] ) ;
					fuzzed.Verify() ; 
					
					res = fuzzed.ToString(true).Equals(real.ToString(true)) ;
					r = res ? Result.Equals : Result.NotEquals ;
					test = fuzzFiles[i] + "    \n" ; 
					test += String.Format( "{0}\n\n!=\n\n{1}" , fuzzed.ToString(true),real.ToString(true) ) ;
					}
				catch(CryptographicException)
					{
					res = true ; 
					r = Result.CryptographicException ;
					}
				catch(Exception e)
					{
					Console.WriteLine(e) ; 
					r = Result.Exception ;
					}
				finally
					{
					Console.WriteLine( r ) ; 
					Eval( r!=Result.Exception , test ) ; 
					}
				}
			}
		}
	string[] GetFiles(string Dir, string Pattern )
		{		
		return Directory.GetFiles( Dir , Pattern) ; 
		}
	string allCerts = "*.c*r*" ; 	
}
enum Result
{
	CryptographicException = 0 ,
	Exception = 1 , 
	Equals = 2 ,
	NotEquals = 3
}
