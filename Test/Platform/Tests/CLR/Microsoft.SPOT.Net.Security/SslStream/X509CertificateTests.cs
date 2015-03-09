using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Net.Security;
using Microsoft.SPOT.Platform.Test;
using System.Security.Cryptography.X509Certificates;


namespace Microsoft.SPOT.Platform.Tests
{
    class X509CertificateTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            try
            {
                X509Certificate cert = new X509Certificate(CertificatesAndCAs.caCert);
            }
            catch (System.NotSupportedException)
            {
                Log.Comment("If this feature throws an exception then it is assumed that it isn't supported on this device type");                
                return InitializeResult.Skip;
            }
            
            //CertificateStore.ClearAllCertificates();

            return InitializeResult.ReadyToGo; 
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");

            //CertificateStore.ClearAllCertificates();
        }

        [TestMethod]
        public MFTestResults ConstructorValid()
        {
            MFTestResults testResult = MFTestResults.Fail;

            Log.Comment("Create an X509 Certificate.  No Exceptions should be thrown.");

            try
            {
                X509Certificate cert = new X509Certificate(CertificatesAndCAs.caCert);
                testResult = MFTestResults.Pass;
            }
            catch (Exception e)
            {
                Log.Comment("Failed to create a valid certificate with exception: " + e.ToString());
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults ConstructorInvalidFuzzed()
        {
            MFTestResults testResult = MFTestResults.Fail;

            Log.Comment("Create a fuzzed X509 Certificate.  Should throw exception.");

            try
            {
                X509Certificate cert = new X509Certificate(CertificatesAndCAs.fuzzedCert);
                Log.Comment("Did not throw exception creating fuzzed Cert.");
            }
            catch (Exception e)
            {
                Log.Comment("Correctly threw exception creating fuzzed cert: " + e.ToString());
                testResult = MFTestResults.Pass;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults ConstructorInvalidNull()
        {
            MFTestResults testResult = MFTestResults.Fail;

            Log.Comment("Create a null X509 Certificate.  Should throw exception.");

            try
            {
                X509Certificate cert = new X509Certificate(null);
            }
            catch (Exception e)
            {
                Log.Comment("Correctly threw exception creating null cert: " + e.ToString());
                testResult = MFTestResults.Pass;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults IssuerGetCaCert()
        {
            MFTestResults testResult = MFTestResults.Fail;

            Log.Comment("Get the Issuer field of a valid certificate");

            try
            {
                X509Certificate cert = new X509Certificate(CertificatesAndCAs.caCert);
                string issuer = cert.Issuer;

                Log.Comment("TODO: Validate the issuer here");
                Log.Comment("The issuer is: " + issuer);
                testResult = MFTestResults.Pass;
            }
            catch (Exception e)
            {
                Log.Comment("Incorrectly threw exception calling Issuer: " + e.ToString());
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults IssuerGetNewCert()
        {
            MFTestResults testResult = MFTestResults.Fail;

            Log.Comment("Get the Issuer field of a valid certificate");

            try
            {
                X509Certificate cert = new X509Certificate(CertificatesAndCAs.newCert);
                string issuer = cert.Issuer;

                Log.Comment("TODO: Validate the issuer here");
                Log.Comment("The issuer is: " + issuer);
                testResult = MFTestResults.Pass;
            }
            catch (Exception e)
            {
                Log.Comment("Incorrectly threw exception calling Issuer: " + e.ToString());
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults SubjectGetCaCert()
        {
            MFTestResults testResult = MFTestResults.Fail;

            Log.Comment("Get the Subject field of a valid certificate");

            try
            {
                X509Certificate cert = new X509Certificate(CertificatesAndCAs.caCert);
                string subject = cert.Subject;

                Log.Comment("TODO: Validate the subject here");
                Log.Comment("The subject is: " + subject);
                testResult = MFTestResults.Pass;
            }
            catch (Exception e)
            {
                Log.Comment("Incorrectly threw exception calling Subject: " + e.ToString());
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }


        [TestMethod]
        public MFTestResults SubjectGetNewCert()
        {
            MFTestResults testResult = MFTestResults.Fail;

            Log.Comment("Get the Subject field of a valid certificate");

            try
            {
                X509Certificate cert = new X509Certificate(CertificatesAndCAs.newCert);
                string subject = cert.Subject;

                Log.Comment("TODO: Validate the subject here");
                Log.Comment("The subject is: " + subject);
                testResult = MFTestResults.Pass;
            }
            catch (Exception e)
            {
                Log.Comment("Incorrectly threw exception calling Subject: " + e.ToString());
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults GetEffectiveDateCaCert()
        {
            MFTestResults testResult = MFTestResults.Fail;

            Log.Comment("Get the Effective Date field of a valid certificate");

            try
            {
                X509Certificate cert = new X509Certificate(CertificatesAndCAs.caCert);
                DateTime date = cert.GetEffectiveDate();

                Log.Comment("TODO: Validate the DateTime here");
                Log.Comment("The DateTime is: " + date.ToString());
                testResult = MFTestResults.Pass;
            }
            catch (Exception e)
            {
                Log.Comment("Incorrectly threw exception calling Date: " + e.ToString());
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults GetEffectiveDateNewCert()
        {
            MFTestResults testResult = MFTestResults.Fail;

            Log.Comment("Get the Effective Date field of a valid certificate");

            try
            {
                X509Certificate cert = new X509Certificate(CertificatesAndCAs.newCert);
                DateTime date = cert.GetEffectiveDate();

                Log.Comment("TODO: Validate the DateTime here");
                Log.Comment("The DateTime is: " + date.ToString());
                testResult = MFTestResults.Pass;
            }
            catch (Exception e)
            {
                Log.Comment("Incorrectly threw exception calling Date: " + e.ToString());
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults GetExpirationDateCaCert()
        {
            MFTestResults testResult = MFTestResults.Fail;

            Log.Comment("Get the Expiration Date field of a valid certificate");

            try
            {
                X509Certificate cert = new X509Certificate(CertificatesAndCAs.caCert);
                DateTime date = cert.GetExpirationDate();

                Log.Comment("TODO: Validate the DateTime here");
                Log.Comment("The DateTime is: " + date.ToString());
                testResult = MFTestResults.Pass;
            }
            catch (Exception e)
            {
                Log.Comment("Incorrectly threw exception calling Date: " + e.ToString());
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }


        [TestMethod]
        public MFTestResults GetExpirationDateNewCert()
        {
            MFTestResults testResult = MFTestResults.Fail;

            Log.Comment("Get the Expiration Date field of a valid certificate");

            try
            {
                X509Certificate cert = new X509Certificate(CertificatesAndCAs.newCert);
                DateTime date = cert.GetExpirationDate();

                Log.Comment("TODO: Validate the DateTime here");
                Log.Comment("The DateTime is: " + date.ToString());
                testResult = MFTestResults.Pass;
            }
            catch (Exception e)
            {
                Log.Comment("Incorrectly threw exception calling Date: " + e.ToString());
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults GetRawDataCaCert()
        {
            MFTestResults testResult = MFTestResults.Fail;

            Log.Comment("Get the raw data from the valid certificate");

            try
            {
                X509Certificate cert = new X509Certificate(CertificatesAndCAs.caCert);
                byte[] rawCert = cert.GetRawCertData();

                if (Tools.CertificateCompare(CertificatesAndCAs.caCert, rawCert))
                    testResult = MFTestResults.Pass;
            }
            catch (Exception e)
            {
                Log.Comment("Incorrectly threw exception calling GetRawCertData: " + e.ToString());
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults GetRawDataNewCert()
        {
            MFTestResults testResult = MFTestResults.Fail;

            Log.Comment("Get the raw data from the valid certificate");

            try
            {
                X509Certificate cert = new X509Certificate(CertificatesAndCAs.newCert);
                byte[] rawCert = cert.GetRawCertData();

                if (Tools.CertificateCompare(CertificatesAndCAs.newCert, rawCert))
                    testResult = MFTestResults.Pass;
            }
            catch (Exception e)
            {
                Log.Comment("Incorrectly threw exception calling GetRawCertData: " + e.ToString());
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }


    }
}