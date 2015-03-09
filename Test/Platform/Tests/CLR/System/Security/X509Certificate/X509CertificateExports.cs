using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

[SecuritySafeCritical]
public class X509CertificateExports
{
    private string c_CERTFILE = "arcan1.cer";

    public delegate byte[] TestDelegate(X509Certificate cer, X509ContentType type);

    public static int Main(string[] args)
    {
        X509CertificateExports x509;

        TestLibrary.TestFramework.BeginTestCase("X509CertificateExports");

        x509 = new X509CertificateExports();


        if (x509.RunTests())
        {
            TestLibrary.TestFramework.EndTestCase();
            TestLibrary.TestFramework.LogInformation("PASS");
            return 100;
        }
        else
        {
            TestLibrary.TestFramework.EndTestCase();
            TestLibrary.TestFramework.LogInformation("FAIL");
            return 101;
        }
    }

    public bool RunTests()
    {
        bool retVal = true;

        TestLibrary.TestFramework.LogInformation("[Positive]");
        retVal = PosTest1() && retVal;
        retVal = PosTest2() && retVal;

        TestLibrary.TestFramework.LogInformation("");

        TestLibrary.TestFramework.LogInformation("[Negative]");
        retVal = NegTest1() && retVal;
        retVal = NegTest2() && retVal;
        retVal = NegTest3() && retVal;
        retVal = NegTest4() && retVal;
        retVal = NegTest5() && retVal;
        retVal = NegTest6() && retVal;
        retVal = NegTest7() && retVal;
        retVal = NegTest8() && retVal;
        retVal = NegTest9() && retVal;
        retVal = NegTest10() && retVal;
        retVal = NegTest11() && retVal;
        retVal = NegTest12() && retVal;
        retVal = NegTest13() && retVal;
        retVal = NegTest14() && retVal;

        return retVal;
    }

    public bool PosTest1() { return PosTest(1, "Export(X509ContentType)", X509ContentType.Cert, 
                                                          delegate(X509Certificate cer, X509ContentType type)
                                                          {
                                                              return cer.Export(type);
                                                          } ); }
    public bool PosTest2() { return PosTest(2, "Export(X509ContentType, string)", X509ContentType.Cert, 
                                                          delegate(X509Certificate cer, X509ContentType type)
                                                          {
                                                              return cer.Export(type, "");
                                                          } ); }

    public bool NegTest1() { return NegTest(1, "Export(X509ContentType)", X509ContentType.Unknown, 
                                                          delegate(X509Certificate cer, X509ContentType type)
                                                          {
                                                              return cer.Export(type);
                                                          } ); }
    public bool NegTest2() { return NegTest(2, "Export(X509ContentType)", (X509ContentType)0x02, //X509ContentType.SerializedCert
                                                          delegate(X509Certificate cer, X509ContentType type)
                                                          {
                                                              return cer.Export(type);
                                                          } ); }
    public bool NegTest3() { return NegTest(3, "Export(X509ContentType)", (X509ContentType)0x03, //X509ContentType.Pfx
                                                          delegate(X509Certificate cer, X509ContentType type)
                                                          {
                                                              return cer.Export(type);
                                                          } ); }
    public bool NegTest4() { return NegTest(4, "Export(X509ContentType)", (X509ContentType)0x03, //X509ContentType.Pkcs12
                                                          delegate(X509Certificate cer, X509ContentType type)
                                                          {
                                                              return cer.Export(type);
                                                          } ); }
    public bool NegTest5() { return NegTest(5, "Export(X509ContentType)", (X509ContentType)0x04, //X509ContentType.SerializedStore
                                                          delegate(X509Certificate cer, X509ContentType type)
                                                          {
                                                              return cer.Export(type);
                                                          } ); }
    public bool NegTest6() { return NegTest(6, "Export(X509ContentType)", (X509ContentType)0x05, //X509ContentType.Pkcs7
                                                          delegate(X509Certificate cer, X509ContentType type)
                                                          {
                                                              return cer.Export(type);
                                                          } ); }
    public bool NegTest7() { return NegTest(7, "Export(X509ContentType)", (X509ContentType)0x06, //X509ContentType.Authenticode 
                                                          delegate(X509Certificate cer, X509ContentType type)
                                                          {
                                                              return cer.Export(type);
                                                          } ); }
    public bool NegTest8() { return NegTest(8, "Export(X509ContentType, string)", X509ContentType.Unknown, 
                                                          delegate(X509Certificate cer, X509ContentType type)
                                                          {
                                                              return cer.Export(type, "");
                                                          } ); }
    public bool NegTest9() { return NegTest(9, "Export(X509ContentType, string)", (X509ContentType)0x02, //X509ContentType.SerializedCert 
                                                          delegate(X509Certificate cer, X509ContentType type)
                                                          {
                                                              return cer.Export(type, "");
                                                          } ); }
    public bool NegTest10() { return NegTest(10, "Export(X509ContentType, string)", (X509ContentType)0x03, //X509ContentType.Pfx
                                                          delegate(X509Certificate cer, X509ContentType type)
                                                          {
                                                              return cer.Export(type, "");
                                                          } ); }
    public bool NegTest11() { return NegTest(11, "Export(X509ContentType, string)", (X509ContentType)0x03, //X509ContentType.Pkcs12
                                                          delegate(X509Certificate cer, X509ContentType type)
                                                          {
                                                              return cer.Export(type, "");
                                                          } ); }
    public bool NegTest12() { return NegTest(12, "Export(X509ContentType, string)", (X509ContentType)0x04, //X509ContentType.SerializedStore
                                                          delegate(X509Certificate cer, X509ContentType type)
                                                          {
                                                              return cer.Export(type, "");
                                                          } ); }
    public bool NegTest13() { return NegTest(13, "Export(X509ContentType, string)", (X509ContentType)0x05, //X509ContentType.Pkcs7
                                                          delegate(X509Certificate cer, X509ContentType type)
                                                          {
                                                              return cer.Export(type, "");
                                                          } ); }
    public bool NegTest14() { return NegTest(14, "Export(X509ContentType, string)", (X509ContentType)0x06, //X509ContentType.Authenticode
                                                          delegate(X509Certificate cer, X509ContentType type)
                                                          {
                                                              return cer.Export(type, "");
                                                          } ); }

    public bool PosTest(int id, string msg, X509ContentType x509ConType, TestDelegate d)
    {
        bool            retVal = true;
        X509Certificate cer;
        byte[]          bytes;

        TestLibrary.TestFramework.BeginScenario("PosTest"+id+": " + msg + " = " + x509ConType);

        try
        {
            cer   = new X509Certificate(c_CERTFILE);

            bytes = d(cer, x509ConType);

            if (null == bytes)
            {
                TestLibrary.TestFramework.LogError("000", "Export return null!");
                TestLibrary.TestFramework.LogInformation("");
                retVal = false;
            }
        }
        catch (Exception e)
        {
            TestLibrary.TestFramework.LogError("001", "Unexpected exception: " + e);
            TestLibrary.TestFramework.LogInformation("");
            retVal = false;
        }

        return retVal;
    }

    public bool NegTest(int id, string msg, X509ContentType x509ConType, TestDelegate d)
    {
        bool            retVal = true;
        X509Certificate cer;
        byte[]          bytes;

        TestLibrary.TestFramework.BeginScenario("NegTest"+id+": "+msg+" = " + x509ConType);

        try
        {
            cer = new X509Certificate(c_CERTFILE);

            bytes = d(cer, x509ConType);

            TestLibrary.TestFramework.LogError("002", "Exception should have been thrown.");
            TestLibrary.TestFramework.LogInformation("");
            retVal = false;
        }
        catch (CryptographicException e)
        {
            // expected
            // ensure that the message does NOT contain "Unknown error"
            if (e.Message.ToLower().Contains("unknown error"))
            {
                TestLibrary.TestFramework.LogError("003", "Unexpected exception message (should not be unkown): " + e);
                TestLibrary.TestFramework.LogInformation("");
                retVal = false;
            }
        }
        catch (Exception e)
        {
            TestLibrary.TestFramework.LogError("004", "Unexpected exception: " + e);
            TestLibrary.TestFramework.LogInformation("");
            retVal = false;
        }

        return retVal;
    }
}
