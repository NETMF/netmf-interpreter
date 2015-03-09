using System ; 
using System.Diagnostics ; 

public class XmlDsigTest
{
	static int result = 100 ; 
	static int Result
		{
		get
			{
			Console.WriteLine( result == 100 ? "Test passed" : "Test failed" ) ; 
	        	return result ;
			}
		set
			{
			result |= value ; 
			}
		}
	public static int Main()
		{
//		RunTest() ; 
		Result = RunTest() ; 
		return Result ; 
		}	
	static int RunTest()
		{
		string verify = "Verify -v signature.xml" ; 
		string test1 = "Sign -pfx certs\\exportable.pfx -pwd exportable -sha1 \"941A A864 8343 1B45 2A97 C1E6 01BF 4D23 8032 3DFF\" -detached -v result.txt signature.xml" ;
		string test2 = "Sign -l CU -sha1 \"941A A864 8343 1B45 2A97 C1E6 01BF 4D23 8032 3DFF\" -include 2 -v result.txt signature.xml" ; 
		string test3 = "Sign -pfx certs\\exportable.pfx -pwd exportable -sha1 \"941A A864 8343 1B45 2A97 C1E6 01BF 4D23 8032 3DFF\" -detached -v -include 3 result.txt signature.xml" ;
		string test4 = "Sign -pfx certs\\exportable.pfx -pwd exportable -sha1 \"941A A864 8343 1B45 2A97 C1E6 01BF 4D23 8032 3DFF\" -detached -v -include 4 result.txt signature.xml" ;
		string test5 = "Sign -pfx certs\\exportable.pfx -pwd exportable -sha1 \"941A A864 8343 1B45 2A97 C1E6 01BF 4D23 8032 3DFF\" -detached -v -include 5 result.txt signature.xml" ;
		string verify2 = "verify signature-x509-is.xml" ; 
		int Result = 0 ; 
        	Execute( test1 ) ; 
        	Result = Execute( verify ) ; 
        	Execute( test2 ) ; 
        	Result &= Execute( verify ) ; 
        	Execute( test3 ) ; 
        	Result &= Execute( verify ) ; 
        	Execute( test4 ) ; 
        	Result &= Execute( verify ) ; 
        	Execute( test5 ) ; 
        	Result &= Execute( verify ) ; 
        	Result &= Execute( verify2 ) ; 
        	return Result ; 
		}
	public static int Execute(string CommandLineArgs )
	{
		int exitCode = 0 ; 
		try
		{
			Process process=new Process();
			process.StartInfo.FileName="xmlsign.exe" ; 
			process.StartInfo.UseShellExecute=false;
			process.StartInfo.RedirectStandardOutput=true;
			process.StartInfo.RedirectStandardInput=true;
			process.StartInfo.RedirectStandardError=true;
			process.StartInfo.CreateNoWindow=true;
			if( CommandLineArgs != null ) 
				process.StartInfo.Arguments = CommandLineArgs ; 
			process.Start();
			Console.WriteLine( process.StandardOutput.ReadToEnd() ) ; 
			process.WaitForExit();
			exitCode = process.ExitCode ; 
			process.Close();
		}
		catch(Exception e)
		{
			Console.WriteLine( e ) ; 
			exitCode = -1;
		}
		return exitCode ; 				
	}
}
