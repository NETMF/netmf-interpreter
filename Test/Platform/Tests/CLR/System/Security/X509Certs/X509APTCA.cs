using System;
using System.Security.Cryptography.X509Certificates;
using System.Security ; 
using System.Security.Permissions ; 
[assembly:KeyContainerPermission( SecurityAction.RequestOptional, Unrestricted =true ),
	UIPermission( SecurityAction.RequestOptional, Unrestricted =true ),
	StorePermission( SecurityAction.RequestOptional, Unrestricted =true ),
	SecurityPermission( SecurityAction.RequestOptional , Flags = SecurityPermissionFlag.ControlPolicy | SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.Execution | SecurityPermissionFlag.Assertion | SecurityPermissionFlag.UnmanagedCode ),
	FileIOPermission( SecurityAction.RequestOptional , Unrestricted=true),
	EnvironmentPermission( SecurityAction.RequestOptional , Unrestricted=true)]
namespace X509Tester
{
	public class APTCATest
		{
		public static void Main( string[] args )
		{
			Test t = new Test() ; 
			t.Run( args ) ; 
		}
		}
}
