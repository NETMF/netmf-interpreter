using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

[SecuritySafeCritical]
public class X509CertificateLoadCertsFromFile
{
    private string c_INVALIDPATH = "foobaz_unique.cer";
    private string c_VALIDPATH   = "ca.cer";
    private const int c_MINFLAGS = -10;
    private const int c_MAXFLAGS = 10;

    public static int Main(string[] args)
    {
        X509CertificateLoadCertsFromFile x509;

        TestLibrary.TestFramework.BeginTestCase("X509CertificateLoadCertsFromFile");

        x509 = new X509CertificateLoadCertsFromFile();

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

        TestLibrary.TestFramework.LogInformation("");

        TestLibrary.TestFramework.LogInformation("[Negative]");
        retVal = NegTest1() && retVal;
        retVal = NegTest2() && retVal;
        retVal = NegTest3() && retVal;
        retVal = NegTest4() && retVal;

        return retVal;
    }

    public bool PosTest1()
    {
        bool            retVal = true;
        X509Certificate cer;

        TestLibrary.TestFramework.BeginScenario("PosTest1: Valid certificate");

        try
        {
            cer      = new X509Certificate(c_VALIDPATH);

            if (null == cer)
            {
                TestLibrary.TestFramework.LogError("-01", "Failed to load " + c_VALIDPATH);
                retVal = false;
            }
        }
        catch (Exception e)
        {
            TestLibrary.TestFramework.LogError("000", "Unexpected exception: " + e);
            retVal = false;
        }

        return retVal;
    }

    public bool NegTest1()
    {
        bool            retVal = true;
        X509Certificate cer;
        string          fileName;

        TestLibrary.TestFramework.BeginScenario("NegTest1: Null file name");

        try
        {
            fileName = null;
            cer      = new X509Certificate(fileName);

            TestLibrary.TestFramework.LogError("001", "Exception should have been thrown.");
            retVal = false;
        }
        catch (ArgumentNullException)
        {
            // expected
        }
        catch (Exception e)
        {
            TestLibrary.TestFramework.LogError("002", "Unexpected exception: " + e);
            retVal = false;
        }

        return retVal;
    }

    public bool NegTest2()
    {
        bool            retVal = true;
        X509Certificate cer;

        TestLibrary.TestFramework.BeginScenario("NegTest2: String.Empty fileName");

        try
        {
            cer   = new X509Certificate(String.Empty);

            TestLibrary.TestFramework.LogError("003", "Exception should have been thrown.");
            retVal = false;
        }
        catch (ArgumentException)
        {
            // expected
        }
        catch (Exception e)
        {
            TestLibrary.TestFramework.LogError("004", "Unexpected exception: " + e);
            retVal = false;
        }

        return retVal;
    }

    public bool NegTest3()
    {
        bool            retVal = true;
        X509Certificate cer;

        TestLibrary.TestFramework.BeginScenario("NegTest2: Path does not exist");

        try
        {
            cer = new X509Certificate(c_INVALIDPATH);

            TestLibrary.TestFramework.LogError("005", "Exception should have been thrown.");
            retVal = false;
        }
        catch (CryptographicException)
        {
            // expected
        }
        catch (Exception e)
        {
            TestLibrary.TestFramework.LogError("006", "Unexpected exception: " + e);
            retVal = false;
        }

        return retVal;
    }

    public bool NegTest4()
    {
        bool            retVal = true;
        X509Certificate cer;

        TestLibrary.TestFramework.BeginScenario("NegTest4: invalid X509StorageFlags");

        try
        {
            for (int flags=c_MINFLAGS; flags<c_MAXFLAGS; flags++)
            {
                if ((int)X509KeyStorageFlags.DefaultKeySet != flags)
                {
                    try
                    {
                        cer   = new X509Certificate(c_VALIDPATH, "", (X509KeyStorageFlags)flags);

                        TestLibrary.TestFramework.LogError("007", "Exception should have been thrown. Flags(" + flags + ")");
                        retVal = false;
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }
        }
        catch (Exception e)
        {
            TestLibrary.TestFramework.LogError("008", "Unexpected exception: " + e);
            retVal = false;
        }

        return retVal;
    }
}
