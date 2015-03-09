using System;
using System.Security.Cryptography;
using System.Security;

class SignDesc {


    public static Boolean Test()
    {
        Boolean bRes = true;

		SecurityElement el = new SecurityElement("whatever");
//		el.Text = "<Key>RSA</Key><Digest>SHA1</Digest><Formatter>System.Security.Cryptography.RSAPKCS1SignatureFormatter</Formatter><Deformatter>System.Security.Cryptography.RSAPKCS1SignatureFormatter</Deformatter>";
		SecurityElement el_key = new SecurityElement("Key");
		el_key.Text = "RSA";
		SecurityElement el_digest = new SecurityElement("Digest");
		el_digest.Text = "SHA1";
		SecurityElement el_form = new SecurityElement("Formatter");
		el_form.Text = "System.Security.Cryptography.RSAPKCS1SignatureFormatter";
		SecurityElement el_deform = new SecurityElement("Deformatter");
		el_deform.Text = "System.Security.Cryptography.RSAPKCS1SignatureDeformatter";

		el.AddChild(el_key);
		el.AddChild(el_digest);
		el.AddChild(el_form);
		el.AddChild(el_deform);

		SignatureDescription sd_empty = new SignatureDescription();
		
		SignatureDescription sd = new SignatureDescription(el);

		Console.WriteLine(sd.CreateDigest());
		Console.WriteLine(sd.CreateFormatter(RSA.Create()));
		Console.WriteLine(sd.CreateDeformatter(RSA.Create()));

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




