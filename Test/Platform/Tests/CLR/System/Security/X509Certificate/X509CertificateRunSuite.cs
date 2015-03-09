using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using TestLibrary;
using System.Linq;

public class X509CertificateRunSuite
{
    private static X509KeyStorageFlags[] m_allKeyStorageFlags = new X509KeyStorageFlags[] 
                                                                {
                                                                X509KeyStorageFlags.DefaultKeySet
                                                                };

    [System.Security.SecuritySafeCritical]
    static bool DoValidate(FileInfo fi)
    {
        bool                        retVal;
        FileStream                  fs;
        StreamReader                fsr;
        IDictionary<string, string> certVals;
        X509Certificate             cer;

        retVal             = true;
        fs                 = File.Open(fi.FullName + ".bsl", FileMode.Open);
        fsr                = new StreamReader(fs);
        certVals           = new Dictionary<string, string>();

	// open baseline file and retain expected values
        while (fsr.Peek() >= 0)
        {
            string[] lr = fsr.ReadLine().Split(new char[] { '|' });
            certVals.Add(lr[0], lr[1]);
        }
        fs.Close();

        // X509Certificate.CreateFromCertFile(String)
        TestFramework.LogInformation("-- importing certificate using X509Certificate.CreateFromCertFile()");
        cer = X509Certificate.CreateFromCertFile(fi.FullName);
        if (!ValidateCertAgainstBaseline(certVals, cer))
        {
            retVal = false;
            TestFramework.LogError("011", "Certificate built with X509Certificate.CreateFromCertFile() failed");
        }

        // Import(String)
        TestFramework.LogInformation("-- importing certificate using .ctor() and Import(String)");
        cer = new X509Certificate();
        cer.Import(fi.FullName);
        if (!ValidateCertAgainstBaseline(certVals, cer))
        {
            retVal = false;
            TestFramework.LogError("011", "Certificate built with default .ctor and Import(string) failed");
        }

        // Import(String,String,X509KeyStorageFlags)
        TestFramework.LogInformation("-- importing certificate using .ctor() and Import(String,String,X509KeyStorageFlags)");
        cer = new X509Certificate();
        foreach (X509KeyStorageFlags ksf in m_allKeyStorageFlags)
        {
            cer.Import(fi.FullName, "", ksf);
            if (!ValidateCertAgainstBaseline(certVals, cer))
            {
                retVal = false;
                TestFramework.LogError("012", "Certificate built with default .ctor and Import(string, string, X509StorageKeys) failed for " + ksf.ToString());
            }
        }

        // Import(byte[])
        cer = new X509Certificate();
        TestFramework.LogInformation("-- importing certificate using .ctor() and Import(byte[])");
        cer.Import(BytesFromFile(fi.FullName));
        if (!ValidateCertAgainstBaseline(certVals, cer))
        {
            retVal = false;
            TestFramework.LogError("013", "Certificate built with default .ctor and Import(byte[]) failed");
        }

        // Import(byte[],String,X509KeyStorageFlags)
        TestFramework.LogInformation("-- importing certificate using .ctor() and Import(byte[],String,X509KeyStorageFlags)");
        cer = new X509Certificate();
        cer.Import(BytesFromFile(fi.FullName), "", X509KeyStorageFlags.DefaultKeySet);
        if (!ValidateCertAgainstBaseline(certVals, cer))
        {
            retVal = false;
            TestFramework.LogError("014", "Certificate built with default .ctor and Import(byte[],string,X509StorageKeys) failed");
        }


        // second case: validate string ctor
        TestFramework.LogInformation("-- importing certificate using .ctor(String)");
        cer = new X509Certificate(fi.FullName);
        if (!ValidateCertAgainstBaseline(certVals, cer))
        {
            retVal = false;
            TestFramework.LogError("015", "Certificate built with .ctor(String) failed");
        }


        // third case: validate string ctor with password
        TestFramework.LogInformation("-- importing certificate using .ctor(string,string) ");
        cer = new X509Certificate(fi.FullName, "");
        if (!ValidateCertAgainstBaseline(certVals, cer))
        {
            retVal = false;
            TestFramework.LogError("016", "Certificate built with .ctor(String,String) failed");
        }


        // fourth case: validate string ctor with password and KeyStorageFlags
        TestFramework.LogInformation("-- importing certificate using .ctor() (String,String,X509KeyStorageFlags)");
        foreach (X509KeyStorageFlags ksf in m_allKeyStorageFlags)
        {
            cer = new X509Certificate(fi.FullName, "", ksf);
            if (!ValidateCertAgainstBaseline(certVals, cer))
            {
                retVal = false;
                TestFramework.LogError("017", "Certificate built with default .ctor and Import(string, string, X509StorageKeys) failed for " + ksf.ToString());
            }
        }

        // .ctor(IntPtr)
        TestFramework.LogInformation("-- importing certificate using .ctor(IntPtr) ");
	X509Certificate cerTmp = new X509Certificate(fi.FullName);
        cer = new X509Certificate( GetHandle(cerTmp) );
        if (!ValidateCertAgainstBaseline(certVals, cer))
        {
            retVal = false;
            TestFramework.LogError("016", "Certificate built with .ctor(IntPtr) failed");
        }
	GC.KeepAlive(cerTmp);

        // .ctor(X509Certificate)
        TestFramework.LogInformation("-- importing certificate using .ctor(X509Certificate) ");
        cer = new X509Certificate( new X509Certificate(fi.FullName) );
        if (!ValidateCertAgainstBaseline(certVals, cer))
        {
            retVal = false;
            TestFramework.LogError("016", "Certificate built with .ctor(X509Certificate) failed");
        }

        return retVal;
    }

    [System.Security.SecuritySafeCritical]
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
    static bool ValidateCertAgainstBaseline(IDictionary<string, string> certVals, X509Certificate cer)
    {

        bool retVal = true;
        long effectiveDateFound, expiryDateFound, effectiveDateBsl, expiryDateBsl;
        byte[] bytes;
        string str;
        int    hash;
        IntPtr handle;
        // now validate against the actual cert

        if (!certVals["HashString"].Equals(cer.GetCertHashString()))
        {
            TestFramework.LogError("001", "Expected hash string: " + certVals["HashString"] + ", found: " + cer.GetCertHashString());
            retVal = false;
        }

	// check the dates
	try
	{
            effectiveDateBsl   = Convert.ToInt64(certVals["EffectiveDateStringInTicks"]);
            effectiveDateFound = DateTime.Parse(cer.GetEffectiveDateString(), CultureInfo.InvariantCulture).ToUniversalTime().Ticks;
            expiryDateBsl      = Convert.ToInt64(certVals["ExpirationDateStringInTicks"]);
            expiryDateFound    = DateTime.Parse(cer.GetExpirationDateString(), CultureInfo.InvariantCulture).ToUniversalTime().Ticks;

            if (effectiveDateBsl != effectiveDateFound)
            {
                TestFramework.LogError("002", "Expected \"Valid From\": [" + (new DateTime(effectiveDateBsl, DateTimeKind.Utc)).ToString() + "], found: [" + cer.GetEffectiveDateString() +" nonUTC]");
                TestFramework.LogError("002", "                       ticks(" + effectiveDateBsl + ") found ticks(" + effectiveDateFound + ")");
                retVal = false;
            }

            if (expiryDateBsl != expiryDateFound)
            {
                TestFramework.LogError("003", "Expected \"Valid To\": [" + (new DateTime(expiryDateBsl)).ToString() + "], found: [" + cer.GetExpirationDateString() + " nonUTC]");
                TestFramework.LogError("003", "                       ticks(" + expiryDateBsl + ") found ticks(" + expiryDateFound + ")");
                retVal = false;
            }
        }
        catch (Exception e)
        {
            TestFramework.LogError("103", "Unexpected exception: " + e);
            retVal = false;
        }

        TestFramework.LogInformation("  Validating field: Format");
        if (!certVals["Format"].Equals(cer.GetFormat()))
        {
            TestFramework.LogError("004", "Expected format: " + certVals["Format"] + ", found: " + cer.GetFormat());
            retVal = false;
        }

        TestFramework.LogInformation("  Validating field: Issuer");
        if (!certVals["Issuer"].Equals(cer.Issuer))
        {
            TestFramework.LogError("005", "Expected issuer: " + certVals["Issuer"] + ", found: " + cer.Issuer);
            retVal = false;
        }

        TestFramework.LogInformation("  Validating field: KeyAlgorithm");
        if (!certVals["KeyAlgorithm"].Equals(cer.GetKeyAlgorithm()))
        {
            TestFramework.LogError("006", "Expected key algorithm: " + certVals["KeyAlgorithm"] + ", found: " + cer.GetKeyAlgorithm());
            retVal = false;
        }

        TestFramework.LogInformation("  Validating field: KeyAlgorithmParameters");
        if (!certVals["KeyAlgorithmParameters"].Equals(cer.GetKeyAlgorithmParametersString()))
        {
            TestFramework.LogError("007", "Expected key alg parameters :" + certVals["KeyAlgorithmParameters"] + ", found :" +
                    cer.GetKeyAlgorithmParametersString());
            retVal = false;
        }

        TestFramework.LogInformation("  Validating field: PublicKeyString");
        if (!certVals["PublicKeyString"].Equals(cer.GetPublicKeyString()))
        {
            TestFramework.LogError("008", "Expected public key: " + certVals["PublicKeyString"] + ", found: " +
                cer.GetPublicKeyString());
            retVal = false;
        }

        TestFramework.LogInformation("  Validating field: SerialNumberString");
        if (!certVals["SerialNumberString"].Equals(cer.GetSerialNumberString()))
        {
            TestFramework.LogError("009", "Expected serial number: " + certVals["SerialNumberString"] + ", found: " + cer.GetSerialNumberString());
            retVal = false;
        }

        TestFramework.LogInformation("  Validating field: Subject");
        if (!certVals["Subject"].Equals(cer.Subject))
        {
            TestFramework.LogError("010", "Expected subject: " + certVals["Subject"] + ", found: " + cer.Subject);
            retVal = false;
        }

        TestFramework.LogInformation("  Retrieving field: CertHash");
        bytes = cer.GetCertHash();

        TestFramework.LogInformation("  Retrieving field: HashCode");
        hash = cer.GetHashCode();

        TestFramework.LogInformation("  Retrieving field: RawCertHash");
        bytes = cer.GetRawCertData();

        TestFramework.LogInformation("  Retrieving field: RawCertDataString");
        str = cer.GetRawCertDataString();

        TestFramework.LogInformation("  Retrieving field: SerialNumber");
        bytes = cer.GetSerialNumber();

        TestFramework.LogInformation("  Retrieving field: ToString()");
        str = cer.ToString();

        TestFramework.LogInformation("  Retrieving field: ToString(true)");
        str = cer.ToString(true);

        TestFramework.LogInformation("  Retrieving field: Handle");
        handle = GetHandle(cer);

        TestFramework.LogInformation("  Testing: Equality with a string");
        if (cer.Equals(str))
        {
            TestFramework.LogError("110", "X509Certificate \"equals\" a string?");
            retVal = false;
        }

        TestFramework.LogInformation("  Testing: Equality with itself(1)");
        if (!cer.Equals((object)cer))
        {
            TestFramework.LogError("120", "X509Certificate does not equal itself");
            retVal = false;
        }

        TestFramework.LogInformation("  Testing: Equality with itself(2)");
        if (!cer.Equals(cer))
        {
            TestFramework.LogError("130", "X509Certificate does not equal itself");
            retVal = false;
        }      

        return retVal;
    }

    [System.Security.SecuritySafeCritical]
    static IntPtr GetHandle(X509Certificate cer)
    {
        return cer.Handle;
    }

    [System.Security.SecurityCritical]
    static DirectoryInfo GetCurrentDir()
    {
        return new DirectoryInfo(".");
    }

    [System.Security.SecuritySafeCritical]
    public static int Main(string[] args)
    {
        bool retVal = true;
        DirectoryInfo di = GetCurrentDir();
        foreach (FileInfo fi in di.EnumerateFiles("*.cer"))
        {
            TestFramework.LogInformation("==== " + fi.Name+" ====");
            retVal = DoValidate(fi) && retVal;
        }

        if (retVal)
        {
            TestFramework.LogInformation("SUCCESS!");
            return 100;
        }
        else
        {
            TestFramework.LogInformation("FAILED.");
            return 101;
        }
    }
}