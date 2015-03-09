////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT.Platform.Test;


namespace Microsoft.SPOT.Platform.Tests
{
    public class Master_SslStream
    {
        public static void Main()
        {
            //Set the system time to be valid relative to the certificate expiration dates.
            DateTime systemTime = new DateTime(2008, 01, 01, 01, 01, 01);
            
            Log.Comment("Setting System Time to: " + systemTime.ToString());
            Hardware.Utility.SetLocalTime(systemTime);

            string[] args = { 
                "X509CertificateTests",
                "SslStreamTests",
                //"CertificateStoreTests", 
            };

            MFTestRunner runner = new MFTestRunner(args);
        }
    }
}