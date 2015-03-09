using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Net.Security;
using Microsoft.SPOT.Platform.Test;
using System.Threading;


namespace Microsoft.SPOT.Platform.Tests
{
    class CertificateStoreTests : IMFTestInterface
    {
        //private static int numNotificationCalls = 0;
        //private static string notifiedCertName;
        //private static CertificateStore.CertificateNotificationType notifiedType;
        
        //private static void CertificateMessage(CertificateStore.CertificateNotificationType type, string certName)
        //{

        //    Log.Comment("CertificateMessage received " + type.ToString() + " " + certName.ToString());
        //    notifiedType = type;
        //    notifiedCertName = certName;
        //    numNotificationCalls++;
        //}

        //public CertificateStoreTests()
        //{
        //}
        
        [SetUp]
        public InitializeResult Initialize()
        {
            //try
            //{
            //    X509Certificate cert = new X509Certificate(CertificatesAndCAs.caCert);
            //}
            //catch (NotSupportedException)
            //{
            //    Log.Comment("If this feature throws an exception then it is assumed that it isn't supported on this device type");
            //    return InitializeResult.Skip;
            //}
            
            //Log.Comment("Adding set up for the tests.");
            //Log.Comment("Thus debugging without knowing if there is a cert in flash without starting from scratch is disasterous.");
            //CertificateStore.ClearAllCertificates();
            //Debug.GC(true);

            return InitializeResult.ReadyToGo; 
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");

            //CertificateStore.ClearAllCertificates();
        }
        
        //[TestMethod]
        //public MFTestResults AddThenGetCACertificate()
        //{
        //    bool testResult = true;

        //    try
        //    {
        //        Log.Comment("Add Certificates, then Get those that were added");
        //        X509Certificate cert1a = new X509Certificate(CertificatesAndCAs.caCert);
        //        X509Certificate cert2a = new X509Certificate(CertificatesAndCAs.newCert);

        //        CertificateStore.AddCACertificate("cert1", cert1a);
        //        CertificateStore.AddCACertificate("cert2", cert2a);

        //        Log.Comment("Get the certificates and make sure they are what we put in");
        //        X509Certificate cert1 = CertificateStore.GetCACertificate("cert1");
        //        X509Certificate cert2 = CertificateStore.GetCACertificate("cert2");

        //        testResult &= Tools.CertificateCompare(cert1, cert1a);
        //        testResult &= Tools.CertificateCompare(cert2, cert2a);

        //        Log.Comment("clean up the cert that was added.");
        //        CertificateStore.RemoveCACertificate("cert1");
        //        CertificateStore.RemoveCACertificate("cert2");
        //    }
        //    finally
        //    {
        //        CertificateStore.ClearAllCertificates();
        //    }

        //    return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        //}

        //[TestMethod]
        //public MFTestResults AddThenRemoveThenGetCACertificate()
        //{
        //    bool testResult = true;

        //    try
        //    {
        //        Log.Comment("Add Certificates, then Get those that were added");
        //        X509Certificate cert1a = new X509Certificate(CertificatesAndCAs.caCert);
        //        X509Certificate cert2a = new X509Certificate(CertificatesAndCAs.newCert);
        //        X509Certificate cert3a = new X509Certificate(CertificatesAndCAs.newCert);

        //        CertificateStore.AddCACertificate("cert1", cert1a);
        //        CertificateStore.AddCACertificate("cert2", cert2a);
        //        CertificateStore.AddCACertificate("cert3", cert3a);

        //        CertificateStore.RemoveCACertificate("cert2");

        //        Log.Comment("Get the certificates and make sure they are what we put in");
        //        X509Certificate cert1 = CertificateStore.GetCACertificate("cert1");
        //        X509Certificate cert3 = CertificateStore.GetCACertificate("cert3");

        //        testResult &= Tools.CertificateCompare(cert1, cert1a);
        //        testResult &= Tools.CertificateCompare(cert3, cert3a);

        //        Log.Comment("clean up the cert that was added.");
        //        CertificateStore.RemoveCACertificate("cert1");
        //        CertificateStore.RemoveCACertificate("cert3");
        //    }
        //    finally
        //    {
        //        CertificateStore.ClearAllCertificates();
        //    }
        
        //    return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        //}

        //[TestMethod]
        //public MFTestResults AddThenGetPersonalCertificate()
        //{
        //    bool testResult = true;

        //    try
        //    {
        //        Log.Comment("Add Certificates, then Get those that were added");
        //        X509Certificate cert1a = new X509Certificate(CertificatesAndCAs.caCert);
        //        X509Certificate cert2a = new X509Certificate(CertificatesAndCAs.newCert);

        //        CertificateStore.AddPersonalCertificate("cert1", cert1a);
        //        CertificateStore.AddPersonalCertificate("cert2", cert2a);

        //        Log.Comment("Get the certificates and make sure they are what we put in");
        //        X509Certificate cert1 = CertificateStore.GetPersonalCertificate("cert1");
        //        X509Certificate cert2 = CertificateStore.GetPersonalCertificate("cert2");

        //        testResult &= Tools.CertificateCompare(cert1, cert1a);
        //        testResult &= Tools.CertificateCompare(cert2, cert2a);

        //        Log.Comment("clean up the cert that was added.");
        //        CertificateStore.RemovePersonalCertificate("cert1");
        //        CertificateStore.RemovePersonalCertificate("cert2");
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Comment("Incorrectly threw exception : " + e.ToString());
        //        testResult = false;
        //    }
        //    finally
        //    {
        //        CertificateStore.ClearAllCertificates();
        //    }

        //    return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        //}

        //[TestMethod]
        //public MFTestResults AddThenRemoveThenGetPersonalCertificate()
        //{
        //    bool testResult = true;

        //    try
        //    {

        //        Log.Comment("Add Certificates, then Get those that were added");
        //        X509Certificate cert1a = new X509Certificate(CertificatesAndCAs.caCert);
        //        X509Certificate cert2a = new X509Certificate(CertificatesAndCAs.newCert);
        //        X509Certificate cert3a = new X509Certificate(CertificatesAndCAs.newCert);

        //        CertificateStore.AddPersonalCertificate("cert1", cert1a);
        //        CertificateStore.AddPersonalCertificate("cert2", cert2a);
        //        CertificateStore.AddPersonalCertificate("cert3", cert3a);

        //        CertificateStore.RemovePersonalCertificate("cert2");

        //        Log.Comment("Get the certificates and make sure they are what we put in");
        //        X509Certificate cert1 = CertificateStore.GetPersonalCertificate("cert1");
        //        X509Certificate cert3 = CertificateStore.GetPersonalCertificate("cert3");

        //        testResult &= Tools.CertificateCompare(cert1, cert1a);
        //        testResult &= Tools.CertificateCompare(cert3, cert3a);

        //        Log.Comment("clean up the cert that was added.");
        //        CertificateStore.RemovePersonalCertificate("cert1");
        //        CertificateStore.RemovePersonalCertificate("cert3");
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Comment("Incorrectly threw exception : " + e.ToString());
        //        testResult = false;
        //    }
        //    finally
        //    {
        //        CertificateStore.ClearAllCertificates();
        //    }

        //    return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        //}

        //[TestMethod]
        //public MFTestResults AddThenVerifyAddNotificationMessage()
        //{
        //    MFTestResults testResult = MFTestResults.Pass;

        //    try
        //    {
        //        Log.Comment("Sign up to get the notification messages");
        //        CertificateStore.CertificateNotificationMessage message = new CertificateStore.CertificateNotificationMessage(CertificateMessage);
        //        CertificateStore.OnCACertificateChange += message;


        //        Log.Comment("Add Certificates then Verify the events received.");
        //        CertificateStore.AddCACertificate("cert1", new X509Certificate(CertificatesAndCAs.caCert));

        //        Thread.Sleep(100);
        //        if (notifiedCertName != "cert1")
        //        {
        //            Log.Comment("invalid certificate notification message");
        //            testResult = MFTestResults.Fail;
        //        }

        //        if (notifiedType != CertificateStore.CertificateNotificationType.Added)
        //        {
        //            Log.Comment("invalid certificate notification type");
        //            testResult = MFTestResults.Fail;
        //        }

        //        Log.Comment("Add Certificates then Verify the events received.");
        //        CertificateStore.AddCACertificate("cert2", new X509Certificate(CertificatesAndCAs.newCert));

        //        Thread.Sleep(100);
        //        if (notifiedCertName != "cert2")
        //        {
        //            Log.Comment("invalid certificate notification message");
        //            testResult = MFTestResults.Fail;
        //        }

        //        if (notifiedType != CertificateStore.CertificateNotificationType.Added)
        //        {
        //            Log.Comment("invalid certificate notification type");
        //            testResult = MFTestResults.Fail;
        //        }

        //        if (numNotificationCalls != 2)
        //        {
        //            Log.Comment("the CertificatNotification message should be called twice but is called: " + numNotificationCalls);
        //            testResult = MFTestResults.Fail;
        //        }
        //        numNotificationCalls = 0;

        //        CertificateStore.OnCACertificateChange -= message;
        //        CertificateStore.RemoveCACertificate("cert1");
        //        CertificateStore.RemoveCACertificate("cert2");
        //    }
        //    catch
        //    {
        //        testResult = MFTestResults.Fail;
        //    }
        //    finally
        //    {
        //        CertificateStore.ClearAllCertificates();
        //    }

        //    return testResult;
        //}

        //[TestMethod]
        //public MFTestResults AddThenVerifyRemoveNotificationMessage()
        //{
        //    MFTestResults testResult = MFTestResults.Pass;

        //    try
        //    {
        //        Log.Comment("Add Certificates then Verify the events received.");
        //        CertificateStore.AddCACertificate("cert1", new X509Certificate(CertificatesAndCAs.caCert));
        //        CertificateStore.AddCACertificate("cert2", new X509Certificate(CertificatesAndCAs.newCert));

        //        Log.Comment("Sign up to get the notification messages");
        //        CertificateStore.CertificateNotificationMessage message = new CertificateStore.CertificateNotificationMessage(CertificateMessage);
        //        CertificateStore.OnCACertificateChange += message;

        //        Log.Comment("Removing cert1");
        //        CertificateStore.RemoveCACertificate("cert1");
        //        Thread.Sleep(100);
        //        if (notifiedCertName != "cert1")
        //        {
        //            Log.Comment("invalid certificate notification message");
        //            testResult = MFTestResults.Fail;
        //        }

        //        if (notifiedType != CertificateStore.CertificateNotificationType.Removed)
        //        {
        //            Log.Comment("invalid certificate notification type");
        //            testResult = MFTestResults.Fail;
        //        }

        //        Log.Comment("Removing cert2");
        //        CertificateStore.RemoveCACertificate("cert2");

        //        Thread.Sleep(100);
        //        if (notifiedCertName != "cert2")
        //        {
        //            Log.Comment("invalid certificate notification message");
        //            testResult = MFTestResults.Fail;
        //        }

        //        if (notifiedType != CertificateStore.CertificateNotificationType.Removed)
        //        {
        //            Log.Comment("invalid certificate notification type");
        //            testResult = MFTestResults.Fail;
        //        }

        //        if (numNotificationCalls != 2)
        //        {
        //            Log.Comment("the CertificatNotification message should be called twice but is called: " + numNotificationCalls);
        //            testResult = MFTestResults.Fail;
        //        }
        //        numNotificationCalls = 0;

        //        CertificateStore.OnCACertificateChange -= message;
        //    }
        //    catch
        //    {
        //        testResult = MFTestResults.Fail;
        //    }
        //    finally
        //    {
        //        CertificateStore.ClearAllCertificates();
        //    }
        //    return testResult;
        //}

        //[TestMethod]
        //public MFTestResults AddThenVerifyUpdateNotificationMessage()
        //{
        //    MFTestResults testResult = MFTestResults.Pass;

        //    try
        //    {
        //        Log.Comment("Add Certificates then Verify the events received.");
        //        CertificateStore.AddCACertificate("cert1", new X509Certificate(CertificatesAndCAs.caCert));
        //        CertificateStore.AddCACertificate("cert2", new X509Certificate(CertificatesAndCAs.newCert));

        //        Log.Comment("Sign up to get the notification messages");
        //        CertificateStore.CertificateNotificationMessage message = new CertificateStore.CertificateNotificationMessage(CertificateMessage);
        //        CertificateStore.OnCACertificateChange += message;

        //        Log.Comment("Updating cert1");
        //        CertificateStore.UpdateCACertificate("cert1", new X509Certificate(CertificatesAndCAs.newCert));
        //        Thread.Sleep(100);
        //        if (notifiedCertName != "cert1")
        //        {
        //            Log.Comment("invalid certificate notification message");
        //            testResult = MFTestResults.Fail;
        //        }

        //        if (notifiedType != CertificateStore.CertificateNotificationType.Updated)
        //        {
        //            Log.Comment("invalid certificate notification type");
        //            testResult = MFTestResults.Fail;
        //        }

        //        Log.Comment("Updating cert2");
        //        CertificateStore.UpdateCACertificate("cert2", new X509Certificate(CertificatesAndCAs.caCert));

        //        Thread.Sleep(100);
        //        if (notifiedCertName != "cert2")
        //        {
        //            Log.Comment("invalid certificate notification message");
        //            testResult = MFTestResults.Fail;
        //        }

        //        if (notifiedType != CertificateStore.CertificateNotificationType.Updated)
        //        {
        //            Log.Comment("invalid certificate notification type");
        //            testResult = MFTestResults.Fail;
        //        }


        //        if (numNotificationCalls != 2)
        //        {
        //            Log.Comment("the CertificatNotification message should be called twice but is called: " + numNotificationCalls);
        //            testResult = MFTestResults.Fail;
        //        }
        //        numNotificationCalls = 0;

        //        CertificateStore.OnCACertificateChange -= message;
        //        Log.Comment("clean up the cert that was added.");
        //        CertificateStore.RemoveCACertificate("cert1");
        //        CertificateStore.RemoveCACertificate("cert2");
        //    }
        //    catch
        //    {
        //        testResult = MFTestResults.Fail;
        //    }
        //    finally
        //    {
        //        CertificateStore.ClearAllCertificates();
        //    }

        //    return testResult;
        //}

        //[TestMethod]
        //public MFTestResults AddNullStringCA()
        //{
        //    MFTestResults testResult = MFTestResults.Fail;

        //    try
        //    {
        //        Log.Comment("Add a null string cert name should throw an exception.");
        //        CertificateStore.AddCACertificate(null, new X509Certificate(CertificatesAndCAs.caCert));
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Comment("correctly threw exception on null string name" + e.ToString());
        //        testResult = MFTestResults.Pass;
        //    }
        //    finally
        //    {
        //        CertificateStore.ClearAllCertificates();
        //    }

        //    return testResult;
        //}

        //[TestMethod]
        //public MFTestResults AddNullCertCA()
        //{
        //    MFTestResults testResult = MFTestResults.Fail;

        //    try
        //    {
        //        Log.Comment("Null cert should throw an exception.");
        //        CertificateStore.AddCACertificate("cert1", null);
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Comment("correctly threw exception on null string name" + e.ToString());
        //        testResult = MFTestResults.Pass;
        //    }
        //    finally
        //    {
        //        CertificateStore.ClearAllCertificates();
        //    }
        //    return testResult;
        //}

        //[TestMethod]
        //public MFTestResults AddEmptyStringCA()
        //{
        //    MFTestResults testResult = MFTestResults.Fail;

        //    try
        //    {
        //        Log.Comment("Add a empty string cert name should be valid");
        //        if(CertificateStore.AddCACertificate("", new X509Certificate(CertificatesAndCAs.caCert)))
        //            testResult = MFTestResults.Pass;

        //        CertificateStore.RemoveCACertificate("");
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Comment("incorrectly threw exception on empty string name" + e.ToString());
        //        testResult = MFTestResults.Fail;
        //    }
        //    finally
        //    {
        //        CertificateStore.ClearAllCertificates();
        //    }

        //    return testResult;
        //}

        //[TestMethod]
        //public MFTestResults AddTwoCertsSameNameCA()
        //{
        //    MFTestResults testResult = MFTestResults.Fail;

        //    try
        //    {
        //        Log.Comment("Add two should throw exception.");
        //        CertificateStore.AddCACertificate("cert1", new X509Certificate(CertificatesAndCAs.caCert));

        //        CertificateStore.AddCACertificate("cert1", new X509Certificate(CertificatesAndCAs.newCert));
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Comment("Correctly threw exception on dual insert" + e.ToString());
        //        testResult = MFTestResults.Pass;

        //        Log.Comment("clean up the cert that was added.");
        //        CertificateStore.RemoveCACertificate("");
        //    }
        //    finally
        //    {
        //        CertificateStore.ClearAllCertificates();
        //    }

        //    return testResult;
        //}

        //[TestMethod]
        //public MFTestResults AddNullStringPersonal()
        //{
        //    MFTestResults testResult = MFTestResults.Fail;

        //    try
        //    {
        //        Log.Comment("Add a null string cert name should throw an exception.");
        //        CertificateStore.AddPersonalCertificate(null, new X509Certificate(CertificatesAndCAs.caCert));
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Comment("correctly threw exception on null string name" + e.ToString());
        //        testResult = MFTestResults.Pass;
        //    }
        //    finally
        //    {
        //        CertificateStore.ClearAllCertificates();
        //    }

        //    return testResult;
        //}

        //[TestMethod]
        //public MFTestResults AddNullCertPersonal()
        //{
        //    MFTestResults testResult = MFTestResults.Fail;

        //    try
        //    {
        //        Log.Comment("Null cert should throw an exception.");
        //        CertificateStore.AddPersonalCertificate("cert1", null);
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Comment("correctly threw exception on null string name" + e.ToString());
        //        testResult = MFTestResults.Pass;
        //    }
        //    finally
        //    {
        //        CertificateStore.ClearAllCertificates();
        //    }

        //    return testResult;
        //}

        //[TestMethod]
        //public MFTestResults AddEmptyStringPersonal()
        //{
        //    MFTestResults testResult = MFTestResults.Fail;

        //    try
        //    {
        //        try
        //        {
        //            Log.Comment("Add a empty string cert name should be valid");
        //            if (!CertificateStore.AddPersonalCertificate("", new X509Certificate(CertificatesAndCAs.caCert)))
        //            {
        //                Log.Comment("incorrectly unable to add the empty string certificate");
        //                testResult = MFTestResults.Fail;
        //            }
        //            else
        //                testResult = MFTestResults.Pass;
        //        }
        //        catch (Exception e)
        //        {
        //            Log.Comment("incorrectly threw exception on empty string name" + e.ToString());
        //            testResult = MFTestResults.Fail;
        //        }

        //        Log.Comment("clean up the cert that was added.");
        //        try
        //        {
        //            CertificateStore.RemovePersonalCertificate("");
        //        }
        //        catch (Exception e)
        //        {
        //            Log.Comment("incorrectly threw exception trying to remove empty string cert name" + e.ToString());
        //            testResult = MFTestResults.Fail;
        //        }
        //    }
        //    finally
        //    {
        //        CertificateStore.ClearAllCertificates();
        //    }
              
        //    return testResult;
        //}

        //[TestMethod]
        //public MFTestResults AddTwoPersonalCertsSameName()
        //{
        //    MFTestResults testResult = MFTestResults.Fail;

        //    try
        //    {
        //        try
        //        {
        //            Log.Comment("Add two should throw an exception.");
        //            CertificateStore.AddPersonalCertificate("cert1", new X509Certificate(CertificatesAndCAs.caCert));

        //            CertificateStore.AddPersonalCertificate("cert1", new X509Certificate(CertificatesAndCAs.newCert));
        //        }
        //        catch (Exception e)
        //        {
        //            Log.Comment("correctly threw exception on dual insert" + e.ToString());
        //            testResult = MFTestResults.Pass;
        //        }

        //        Log.Comment("clean up the cert that was added.");
        //        try
        //        {
        //            CertificateStore.RemovePersonalCertificate("cert1");
        //        }
        //        catch (Exception e)
        //        {
        //            Log.Comment("incorrectly threw exception trying to remove string cert name" + e.ToString());
        //            testResult = MFTestResults.Fail;
        //        }
        //    }
        //    finally
        //    {
        //        CertificateStore.ClearAllCertificates();
        //    }

        //    return testResult;
        //}

        //[TestMethod]
        //public MFTestResults GetNonExistingCACertificate()
        //{
        //    try
        //    {
        //        Log.Comment("Try to get a bogus certificate");
        //        X509Certificate cert1 = CertificateStore.GetCACertificate("bogusCert");

        //        if (cert1 == null)
        //            return MFTestResults.Pass;
        //        else
        //        {
        //            Log.Comment("Incorrectly retieved a cert that doesn't exist");
        //            return MFTestResults.Fail;
        //        }
        //    }
        //    finally
        //    {
        //        CertificateStore.ClearAllCertificates();
        //    }
        //}

        //[TestMethod]
        //public MFTestResults GetNonExistingPersonalCertificate()
        //{
        //    try
        //    {
        //        Log.Comment("Try to get a bogus certificate");
        //        X509Certificate cert1 = CertificateStore.GetPersonalCertificate("bogusCert");

        //        if (cert1 == null)
        //            return MFTestResults.Pass;
        //        else
        //        {
        //            Log.Comment("Incorrectly retieved a cert that doesn't exist");
        //            return MFTestResults.Fail;
        //        }
        //    }
        //    finally
        //    {
        //        CertificateStore.ClearAllCertificates();
        //    }
        //}

        //[TestMethod]
        //public MFTestResults AddThenRemoveCaCertificate()
        //{
        //    MFTestResults testResult = MFTestResults.Pass;

        //    try
        //    {
        //        Log.Comment("Add Certificates, then Remove and verify they're gone");
        //        CertificateStore.AddCACertificate("cert1", new X509Certificate(CertificatesAndCAs.caCert));
        //        CertificateStore.AddCACertificate("cert2", new X509Certificate(CertificatesAndCAs.newCert));
        //        CertificateStore.AddCACertificate("cert3", new X509Certificate(CertificatesAndCAs.newCert));

        //        if (!CertificateStore.RemoveCACertificate("cert2"))
        //        {
        //            Log.Comment("unable to remove cert2");
        //            testResult = MFTestResults.Fail;
        //        }

        //        if (CertificateStore.GetCACertificate("cert2") != null)
        //        {
        //            Log.Comment("cert2 should be removed but isn't.");
        //            testResult = MFTestResults.Fail;
        //        }

        //        Log.Comment("clean up the certs that were added");
        //        CertificateStore.RemoveCACertificate("cert1");
        //        CertificateStore.RemoveCACertificate("cert3");
        //    }
        //    catch
        //    {
        //        testResult = MFTestResults.Fail;
        //    }
        //    finally
        //    {
        //        CertificateStore.ClearAllCertificates();
        //    }

        //    return testResult;
        //}

        //[TestMethod]
        //public MFTestResults RemoveNonExistingCaCertificate()
        //{
        //    Log.Comment("Remove non existent cert and verify it's gone");

        //    try
        //    {
        //        if (CertificateStore.RemoveCACertificate("cert10"))
        //        {
        //            Log.Comment("Returned true trying to remove non existent cert.");
        //            return MFTestResults.Fail;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Comment("Incorrectly threw exception: " + e.ToString());
        //        return MFTestResults.Fail;
        //    }
        //    finally
        //    {
        //        CertificateStore.ClearAllCertificates();
        //    }

        //    return MFTestResults.Pass;
        //}

        //[TestMethod]
        //public MFTestResults AddThenRemovePersonalCertificate()
        //{
        //    MFTestResults testResult = MFTestResults.Pass;

        //    try
        //    {
        //        Log.Comment("Add Certificates, then Remove and verify their gone");
        //        CertificateStore.AddPersonalCertificate("cert1", new X509Certificate(CertificatesAndCAs.caCert));
        //        CertificateStore.AddPersonalCertificate("cert2", new X509Certificate(CertificatesAndCAs.newCert));
        //        CertificateStore.AddPersonalCertificate("cert3", new X509Certificate(CertificatesAndCAs.newCert));

        //        if (!CertificateStore.RemovePersonalCertificate("cert2"))
        //        {
        //            Log.Comment("Unable to remove cert2");
        //            testResult = MFTestResults.Fail;
        //        }

        //        if (CertificateStore.GetPersonalCertificate("cert2") != null)
        //        {
        //            Log.Comment("cert2 should be removed but isn't.");
        //            testResult = MFTestResults.Fail;
        //        }

        //        Log.Comment("clean up the certs that were added");
        //        CertificateStore.RemovePersonalCertificate("cert1");
        //        CertificateStore.RemovePersonalCertificate("cert3");
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Comment("Incorrectly threw exception: " + e.ToString());
        //        testResult = MFTestResults.Fail;
        //    }
        //    finally
        //    {
        //        CertificateStore.ClearAllCertificates();
        //    }

        //    return  testResult;
        //}

        //[TestMethod]
        //public MFTestResults RemoveNonExistingPersonalCertificate()
        //{
        //    Log.Comment("Remove non existent cert and verify it's gone");

        //    try
        //    {
        //        if (CertificateStore.RemoveCACertificate("cert10"))
        //        {
        //            Log.Comment("Returned true trying to remove non existent cert.");
        //            return MFTestResults.Fail;
        //        }
        //    }
        //    finally
        //    {
        //        CertificateStore.ClearAllCertificates();
        //    }

        //    return MFTestResults.Pass;
        //}

        //[TestMethod]
        //public MFTestResults UpdateWithInvalidCACertificate()
        //{
        //    MFTestResults testResult = MFTestResults.Fail;

        //    try
        //    {
        //        Log.Comment("Add Certificates, then update with an invalid cert.");
        //        CertificateStore.AddCACertificate("cert1", new X509Certificate(CertificatesAndCAs.caCert));
        //        CertificateStore.AddCACertificate("cert2", new X509Certificate(CertificatesAndCAs.newCert));

        //        try
        //        {
        //            if (CertificateStore.UpdateCACertificate("cert1", new X509Certificate(CertificatesAndCAs.fuzzedCert)))
        //                Log.Comment("Incorrectly returned true trying to update with an invalid cert.");
        //        }
        //        catch
        //        {
        //            testResult = MFTestResults.Pass;
        //        }

        //        Log.Comment("Verify that the cert was not corrupted");
        //        if (!Tools.CertificateCompare(CertificateStore.GetCACertificate("cert1").GetRawCertData(), CertificatesAndCAs.caCert))
        //        {
        //            Log.Comment("The cert was corrupted");
        //            testResult = MFTestResults.Fail;
        //        }

        //        Log.Comment("clean up the certs that were added");
        //        CertificateStore.RemoveCACertificate("cert1");
        //        CertificateStore.RemoveCACertificate("cert2");
        //    }
        //    finally
        //    {
        //        CertificateStore.ClearAllCertificates();
        //    }

        //    return testResult;
        //}

        //[TestMethod]
        //public MFTestResults UpdateWithValidCACertificate()
        //{
        //    MFTestResults testResult = MFTestResults.Fail;

        //    try
        //    {
        //        Log.Comment("Add Certificates, then update with an invalid cert.");
        //        CertificateStore.AddCACertificate("cert1", new X509Certificate(CertificatesAndCAs.caCert));
        //        CertificateStore.AddCACertificate("cert2", new X509Certificate(CertificatesAndCAs.newCert));

        //        if (CertificateStore.UpdateCACertificate("cert1", new X509Certificate(CertificatesAndCAs.newCert)))
        //            testResult = MFTestResults.Pass;

        //        Log.Comment("Verify that the cert was updated");
        //        if (!Tools.CertificateCompare(CertificateStore.GetCACertificate("cert1").GetRawCertData(), CertificatesAndCAs.newCert))
        //        {
        //            Log.Comment("failed to update the certs data");
        //            testResult = MFTestResults.Fail;
        //        }

        //        Log.Comment("clean up the certs that were added");
        //        CertificateStore.RemoveCACertificate("cert1");
        //        CertificateStore.RemoveCACertificate("cert2");
        //    }
        //    catch
        //    {
        //        testResult = MFTestResults.Fail;
        //    }
        //    finally
        //    {
        //        CertificateStore.ClearAllCertificates();
        //    }

        //    return testResult;
        //}

        //[TestMethod]
        //public MFTestResults UpdateNonExistingCACertificate()
        //{
        //    MFTestResults testResult = MFTestResults.Pass;

        //    try
        //    {
        //        if (CertificateStore.UpdateCACertificate("cert10", new X509Certificate(CertificatesAndCAs.newCert)))
        //        {
        //            Log.Comment("incorrectly able to update a non existing certificate.");
        //            testResult = MFTestResults.Fail;
        //        }
        //    }
        //    catch
        //    {
        //        testResult = MFTestResults.Fail;
        //    }
        //    finally
        //    {
        //        CertificateStore.ClearAllCertificates();
        //    }

        //    return testResult;
        //}

        //[TestMethod]
        //public MFTestResults UpdateWithInvalidPersonalCertificate()
        //{
        //    MFTestResults testResult = MFTestResults.Fail;

        //    try
        //    {

        //        Log.Comment("Add Certificates, then update with an invalid cert.");
        //        CertificateStore.AddPersonalCertificate("cert1", new X509Certificate(CertificatesAndCAs.caCert));
        //        CertificateStore.AddPersonalCertificate("cert2", new X509Certificate(CertificatesAndCAs.newCert));

        //        try
        //        {
        //            if (CertificateStore.UpdatePersonalCertificate("cert1", new X509Certificate(CertificatesAndCAs.fuzzedCert)))
        //                Log.Comment("Incorrectly returned true trying to update with an invalid cert.");
        //        }
        //        catch(Exception)
        //        {
        //            testResult = MFTestResults.Pass;
        //        }

        //        Log.Comment("Verify that the cert was not corrupted");
        //        if (!Tools.CertificateCompare(CertificateStore.GetPersonalCertificate("cert1").GetRawCertData(), CertificatesAndCAs.caCert))
        //        {
        //            testResult = MFTestResults.Fail;
        //        }

        //        Log.Comment("clean up the certs that were added");
        //        CertificateStore.RemovePersonalCertificate("cert1");
        //        CertificateStore.RemovePersonalCertificate("cert2");
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Comment("Incorrect exception: " + e.ToString());
        //        testResult = MFTestResults.Fail;
        //    }
        //    finally
        //    {
        //        CertificateStore.ClearAllCertificates();
        //    }

        //    return testResult;
        //}

        //[TestMethod]
        //public MFTestResults UpdateWithValidPersonalCertificate()
        //{
        //    MFTestResults testResult = MFTestResults.Fail;

        //    try
        //    {
        //        Log.Comment("Add Certificates, then update with an invalid cert.");
        //        CertificateStore.AddPersonalCertificate("cert1", new X509Certificate(CertificatesAndCAs.caCert));
        //        CertificateStore.AddPersonalCertificate("cert2", new X509Certificate(CertificatesAndCAs.newCert));

        //        if (CertificateStore.UpdatePersonalCertificate("cert1", new X509Certificate(CertificatesAndCAs.newCert)))
        //            testResult = MFTestResults.Pass;

        //        Log.Comment("Verify that the cert was updated");
        //        if (!Tools.CertificateCompare(CertificateStore.GetPersonalCertificate("cert1").GetRawCertData(), CertificatesAndCAs.newCert))
        //        {
        //            Log.Comment("failed to update the certs data");
        //            testResult = MFTestResults.Fail;
        //        }

        //        Log.Comment("clean up the certs that were added");
        //        CertificateStore.RemovePersonalCertificate("cert1");
        //        CertificateStore.RemovePersonalCertificate("cert2");
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Comment("Incorrect Exception: " + e.ToString());
        //        testResult = MFTestResults.Fail;
        //    }
        //    finally
        //    {
        //        CertificateStore.ClearAllCertificates();
        //    }

        //    return testResult;
        //}

        //[TestMethod]
        //public MFTestResults UpdateNonExistingPersonalCertificate()
        //{
        //    MFTestResults testResult = MFTestResults.Pass;

        //    try
        //    {
        //        if (CertificateStore.UpdatePersonalCertificate("cert10", new X509Certificate(CertificatesAndCAs.newCert)))
        //        {
        //            Log.Comment("incorrectly able to update a non existing certificate.");
        //            testResult = MFTestResults.Fail;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Comment("Incorrect exception: " + e.ToString());
        //        testResult = MFTestResults.Fail;
        //    }
        //    finally
        //    {
        //        CertificateStore.ClearAllCertificates();
        //    }

        //    return testResult;
        //}

    }
}