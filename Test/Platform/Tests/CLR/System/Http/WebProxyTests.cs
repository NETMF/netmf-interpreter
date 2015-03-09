////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.Net;

namespace Microsoft.SPOT.Platform.Tests
{
    public class WebProxyTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");
            // Add your functionality here.   

            return InitializeResult.ReadyToGo;
        }


        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");

            // TODO: Add your clean up steps here.
        }


        [TestMethod]
        public MFTestResults TestDefaultWebProxy()
        {
            MFTestResults result = MFTestResults.Pass;

            Log.Comment("Set proxy using WebProxy()");
            UriProperties props = new UriProperties("http", "www.microsoft.com");
            Uri uri = new Uri(props.OriginalUri);
            WebProxy proxyObject = new WebProxy();

            WebRequest wr = WebRequest.Create(uri);
            wr.Proxy = proxyObject;

            Uri wrUri = wr.Proxy.GetProxy(uri);

            if (wrUri != uri)
            {
                result = MFTestResults.Fail;
            }

            return result;
        }


        [TestMethod]
        public MFTestResults TestWebProxyConstructor1()
        {
            MFTestResults result = MFTestResults.Pass;

            Log.Comment("Set proxy using WebProxy(string)");
            UriProperties props = new UriProperties("http", "www.microsoft.com");
            Uri uri = new Uri(props.OriginalUri);

            string proxy = "itgproxy.redmond.corp.microsoft.com";
            WebProxy proxyObject = new WebProxy(proxy);

            WebRequest wr = WebRequest.Create(uri);
            wr.Proxy = proxyObject;

            Uri wrUri = wr.Proxy.GetProxy(uri);

            if (wrUri.Host != proxy)
            {
                result = MFTestResults.Fail;
            }

            return result;
        }


        [TestMethod]
        public MFTestResults TestWebProxyConstructor2()
        {
            MFTestResults result = MFTestResults.Pass;

            Log.Comment("Set proxy using WebProxy(string, bool)");
            UriProperties props = new UriProperties("http", "www.microsoft.com");
            Uri uri = new Uri(props.OriginalUri);

            string proxy = "itgproxy.redmond.corp.microsoft.com";
            WebProxy proxyObject = new WebProxy(proxy, true);

            WebRequest wr = WebRequest.Create(uri);
            wr.Proxy = proxyObject;

            Uri wrUri = wr.Proxy.GetProxy(uri);

            if (wrUri.Host != proxy)
            {
                result = MFTestResults.Fail;
            }

            return result;
        }


        [TestMethod]
        public MFTestResults TestWebProxyConstructor3()
        {
            MFTestResults result = MFTestResults.Pass;

            Log.Comment("Set proxy using WebProxy(string, int)");
            UriProperties props = new UriProperties("http", "www.microsoft.com");
            Uri uri = new Uri(props.OriginalUri);

            string proxy = "itgproxy.redmond.corp.microsoft.com";
            WebProxy proxyObject = new WebProxy(proxy, 80);

            WebRequest wr = WebRequest.Create(uri);
            wr.Proxy = proxyObject;

            Uri wrUri = wr.Proxy.GetProxy(uri);

            if (wrUri.Host != proxy)
            {
                result = MFTestResults.Fail;
            }

            return result;
        }


        [TestMethod]
        public MFTestResults TestWebProxyConstructor4()
        {
            MFTestResults result = MFTestResults.Pass;

            Log.Comment("Set proxy using WebProxy(System.Uri, bool)");
            UriProperties props = new UriProperties("http", "www.microsoft.com");
            Uri uri = new Uri(props.OriginalUri);

            WebProxy proxyObject = new WebProxy(uri, true);

            WebRequest wr = WebRequest.Create(uri);
            wr.Proxy = proxyObject;

            Uri wrUri = wr.Proxy.GetProxy(uri);

            if (wrUri != uri)
            {
                result = MFTestResults.Fail;
            }

            return result;
        }


        [TestMethod]
        public MFTestResults TestWebProxyConstructor5()
        {
            MFTestResults result = MFTestResults.Pass;

            Log.Comment("Set proxy using WebProxy(System.Uri)");
            UriProperties props = new UriProperties("http", "www.microsoft.com");
            Uri uri = new Uri(props.OriginalUri);

            WebProxy proxyObject = new WebProxy(uri);

            if (proxyObject.BypassProxyOnLocal)
            {
                result = MFTestResults.Fail;
            }

            WebRequest wr = WebRequest.Create(uri);
            wr.Proxy = proxyObject;

            Uri wrUri = wr.Proxy.GetProxy(uri);

            if (wrUri != uri)
            {
                result = MFTestResults.Fail;
            }

            return result;
        }


        [TestMethod]
        public MFTestResults TestWebProxyInvalidserverAddress()
        {
            MFTestResults result = MFTestResults.Pass;

            Log.Comment("Set proxy using WebProxy(string, bool)");
            UriProperties props = new UriProperties("http", "www.microsoft.com");
            Uri uri = new Uri(props.OriginalUri);

            try
            {
                WebProxy proxyObject = new WebProxy("ht1p:itgproxy", true);
                result = MFTestResults.Fail;
            }
            catch (ArgumentException ex)
            {
                Log.Exception("Expect ArgumentException: ", ex);
            }
            
            try
            {
                WebProxy proxyObject = new WebProxy(string.Empty, true);
                result = MFTestResults.Fail;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Log.Exception("Expect ArgumentOutOfRangeException: ", ex);
            }

            return result;
        }
    }
}
