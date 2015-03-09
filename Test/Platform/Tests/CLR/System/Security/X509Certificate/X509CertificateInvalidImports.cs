using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Linq;

[SecuritySafeCritical]
public class X509CertificateInvalidImports
{
    public delegate bool TestDelegate(string fileName);

    public static int Main(string[] args)
    {
        bool          retVal = true;
        DirectoryInfo di;
        X509CertificateInvalidImports x509;

        TestLibrary.TestFramework.BeginTestCase("X509CertificateInvalidImports");

        x509 = new X509CertificateInvalidImports();
        di = new DirectoryInfo( TestLibrary.Env.CurrentDirectory + TestLibrary.Env.FileSeperator + "invalid");

        foreach (FileInfo fi in di.EnumerateFiles("*"))
        {
            TestLibrary.TestFramework.LogInformation("==== " + fi.Name+" ====");
            retVal = x509.RunTests(fi.FullName) && retVal;
        }

        TestLibrary.TestFramework.EndTestCase();
        if (retVal)
        {
            TestLibrary.TestFramework.LogInformation("PASS");
            return 100;
        }
        else
        {
            TestLibrary.TestFramework.LogInformation("FAIL");
            return 101;
        }
    }

    public bool RunTests(string fileName)
    {
        bool retVal = true;

        TestLibrary.TestFramework.LogInformation("[Negative]");
        retVal = NegTest1(fileName) && retVal;
        retVal = NegTest2(fileName) && retVal;
        retVal = NegTest3(fileName) && retVal;
        retVal = NegTest4(fileName) && retVal;
        retVal = NegTest5(fileName) && retVal;
        retVal = NegTest6(fileName) && retVal;
        retVal = NegTest7(fileName) && retVal;
        retVal = NegTest8(fileName) && retVal;

        return retVal;
    }

    public bool NegTest1(string fileName) { return NegTest(1, "Import(String)", fileName,
                                                                 delegate(string fName)
                                                                 {
                                                                     X509Certificate cer;
                                                                     cer = new X509Certificate();
                                                                     cer.Import(fName);
                                                                     return false;
                                                                 } ); }
    public bool NegTest2(string fileName) { return NegTest(2, "Import(String,String,X509KeyStorageFlags)", fileName,
                                                                 delegate(string fName)
                                                                 {
                                                                     X509Certificate cer;
                                                                     cer = new X509Certificate();
                                                                     cer.Import(fName, "", X509KeyStorageFlags.DefaultKeySet);
                                                                     return false;
                                                                 } ); }
    public bool NegTest3(string fileName) { return NegTest(3, "Import(byte[])", fileName,
                                                                 delegate(string fName)
                                                                 {
                                                                     X509Certificate cer;
                                                                     cer = new X509Certificate();
                                                                     cer.Import(BytesFromFile(fName));
                                                                     return false;
                                                                 } ); }
    public bool NegTest4(string fileName) { return NegTest(4, "Import(byte[],String,X509KeyStorageFlags)", fileName,
                                                                 delegate(string fName)
                                                                 {
                                                                     X509Certificate cer;
                                                                     cer = new X509Certificate();
                                                                     cer.Import(BytesFromFile(fName), "", X509KeyStorageFlags.DefaultKeySet);
                                                                     return false;
                                                                 } ); }
    public bool NegTest5(string fileName) { return NegTest(5, ".ctor(String)", fileName,
                                                                 delegate(string fName)
                                                                 {
                                                                     X509Certificate cer;
                                                                     cer = new X509Certificate(fName);
                                                                     return false;
                                                                 } ); }
    public bool NegTest6(string fileName) { return NegTest(6, ".ctor(string,string)", fileName,
                                                                 delegate(string fName)
                                                                 {
                                                                     X509Certificate cer;
                                                                     cer = new X509Certificate(fName, "");
                                                                     return false;
                                                                 } ); }
    public bool NegTest7(string fileName) { return NegTest(7, ".ctor(String,String,X509KeyStorageFlags)", fileName,
                                                                 delegate(string fName)
                                                                 {
                                                                     X509Certificate cer;
                                                                     cer = new X509Certificate(fName, "", X509KeyStorageFlags.DefaultKeySet);
                                                                     return false;
                                                                 } ); }
    public bool NegTest8(string fileName) { return NegTest(8, "X509Certificate.CreateFromFile()", fileName,
                                                                 delegate(string fName)
                                                                 {
                                                                     X509Certificate cer;
                                                                     cer = X509Certificate.CreateFromCertFile(fName);
                                                                     return false;
                                                                 } ); }

    public bool NegTest(int id, string msg, string fileName, TestDelegate d)
    {
        bool retVal = true;

        TestLibrary.TestFramework.BeginScenario("NegTest" + id + ": " + msg);

        try
        {
            d(fileName);

            TestLibrary.TestFramework.LogError("000", "Exception should have been thrown.");
            TestLibrary.TestFramework.LogInformation("");
            retVal = false;
        }
        catch (CryptographicException e)
        {
            // expected
            // ensure that the message does NOT contain "Unknown error"
            if (e.Message.ToLower().Contains("unknown error"))
            {
                TestLibrary.TestFramework.LogError("001", "Unexpected exception message (should not be unkown): " + e);
                TestLibrary.TestFramework.LogInformation("");
                retVal = false;
            }
        }
        catch (Exception e)
        {
            TestLibrary.TestFramework.LogError("002", "Unexpected exception: " + e);
            TestLibrary.TestFramework.LogInformation("");
            retVal = false;
        }

        return retVal;
    }

    static byte[] BytesFromFile(string fname)
    {
        byte[]       retval = null;
        FileStream   fs     = null;
        BinaryReader br     = null;

        try
        {
            fs = File.Open(fname, FileMode.Open, FileAccess.Read);
            br = new BinaryReader(fs);

            retval = br.ReadBytes((int)fs.Length);
        }
        finally
        {
            if (null != fs) fs.Close();
        }

        return retval;
    }
}
