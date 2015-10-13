////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.Net;

namespace Microsoft.SPOT.Platform.Tests
{
    public class HttpKnownHeaderNamesTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");
            try
            {
                Microsoft.SPOT.Net.NetworkInformation.NetworkInterface[] nis = Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
            }
            catch
            {
                return InitializeResult.Skip;
            }

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");

            // TODO: Add your clean up steps here.
        }


        #region Helper methods
        private bool VerifyHeaderIsIllegal( WebHeaderCollection wrc, string header, string content, Type exceptionType )
        {
            bool res = true;

            try
            {
                Log.Comment("Set Headers Properties for header: '" + header + "', with value: '" + content + "'");
                
                wrc.Set(header, content);
                Log.Comment("Illegal header was set:  Failed.");
                res = false;
            }
            catch (Exception ex)
            {
                if (!HttpTests.ValidateException(ex, exceptionType))
                {
                    res = false;
                }
            } 

            return res;
        }


        private bool VerifyHeaderIsLegal(WebHeaderCollection wrc, string header, string content, Type exceptionType)
        {
            bool res = true;

            try
            {
                Log.Comment("Set Headers Properties for header: '" + header + "', with value: '" + content + "'");

                wrc.Set(header, content);

            }
            catch (Exception ex)
            {
                Log.Exception("Exception thrown for legal header: '" + header + "'", ex);
                res = false;
            }

            return res;
        }

        #endregion Helper methods


        #region Test
        
        [TestMethod]
        public MFTestResults TestSetHTTPRequestHeaderAfterCreateHTTP1_1()
        {
            MFTestResults result = MFTestResults.Pass;
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/");
            wr.UserAgent = ".Net Micro Framwork Device/4.0";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1
            wr.ProtocolVersion = new Version(1, 1);

            try
            {
                // Setup server
                System.Net.WebHeaderCollection wrc = wr.Headers;

                if (!VerifyHeaderIsIllegal(wrc, HttpKnownHeaderNames.Accept, "text/plain; q=0.5, text/html, text/x-dvi; q=0.8, text/x-c", typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyHeaderIsIllegal(wrc, HttpKnownHeaderNames.Connection, "close", typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyHeaderIsIllegal(wrc, HttpKnownHeaderNames.ContentLength, "26012", typeof(ArgumentException))) result = MFTestResults.Fail;
                //if (!VerifyHeaderIsIllegal(wrc, HttpKnownHeaderNames.ContentType, "image/gif", typeof(ArgumentException))) result = MFTestResults.Fail;
                //if (!VerifyHeaderIsIllegal(wrc, HttpKnownHeaderNames.Date, System.DateTime.Today.ToString(), typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyHeaderIsIllegal(wrc, HttpKnownHeaderNames.Expect, "100", typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyHeaderIsIllegal(wrc, HttpKnownHeaderNames.Host, "www.w3.org", typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyHeaderIsIllegal(wrc, HttpKnownHeaderNames.IfModifiedSince, "Sat, 23 May 2009 19:43:31 GMT", typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyHeaderIsIllegal(wrc, HttpKnownHeaderNames.Range, "500-999", typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyHeaderIsIllegal(wrc, HttpKnownHeaderNames.Referer, "http://www.w3.org/hypertext/DataSources/Overview.html", typeof(ArgumentException))) result = MFTestResults.Fail;                
                if (!VerifyHeaderIsIllegal(wrc, HttpKnownHeaderNames.TransferEncoding, "chunked", typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyHeaderIsIllegal(wrc, HttpKnownHeaderNames.UserAgent, ".NetMicroFramework", typeof(ArgumentException))) result = MFTestResults.Fail;
                
            }
            catch (Exception ex)
            {
                Log.Exception("[Client] Unexpected Exception", ex);
                result = MFTestResults.Fail;
            }

            return result;
        }


        public MFTestResults ValidateAbleToSetPropertiesValueHTTP1_1()
        {
            MFTestResults result = MFTestResults.Pass;
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/");
            wr.UserAgent = ".Net Micro Framwork Device/4.0";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.0");
            wr.ProtocolVersion = new Version(1, 1);

            try
            {
                System.Net.WebHeaderCollection wrc = wr.Headers;

                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.AcceptCharset, "iso-8859-5, unicode-1-1;q=0.8", typeof(ArgumentException))) 
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.AcceptEncoding, "compress;q=0.5, gzip;q=1.0", typeof(ArgumentException))) 
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.AcceptLanguage, "en-US", typeof(ArgumentException))) 
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.Age, "2 days", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.Allow, "GET, PUT", typeof(ArgumentException))) 
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.CacheControl, "no-cache", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.ContentEncoding, "gzip", typeof(ArgumentException))) 
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.ContentLanguage, "mi, en", typeof(ArgumentException))) 
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.ContentMD5, "60e985979f1d55ab7542440fbb9659e5", typeof(ArgumentException))) 
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.ContentRange, "bytes 21010-47021/47022", typeof(ArgumentException))) 
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.Cookie, "www.google.com", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.Expires, "Thu, 01 Dec 1994 16:00:00 GMT", typeof(ArgumentException))) 
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.From, "webmaster@w3.org", typeof(ArgumentException))) 
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.IfMatch, "r2d2xxxx", typeof(ArgumentException))) 
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.IfNoneMatch, "xyzzy", typeof(ArgumentException))) 
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.IfRange, "TestIfRange: Need to have Range Header.", typeof(ArgumentException))) 
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.IfUnmodifiedSince, "Fri, 22 May 2009 12:43:31 GMT", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.KeepAlive, "true", typeof(ArgumentException))) 
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.LastModified, "Fri, 22 May 2009 12:43:31 GMT", typeof(ArgumentException))) 
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.MaxForwards, "10", typeof(ArgumentException))) 
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.Pragma, "no-cache", typeof(ArgumentException))) 
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.ProxyAuthenticate, "", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.ProxyAuthorization, "", typeof(ArgumentException))) 
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.RetryAfter, "100000", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.Server, "", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.SetCookie, "www.microsoft.com", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.SetCookie2, "www.bing.com", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.TE, "trailers, deflate;q=0.5", typeof(ArgumentException))) 
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.Trailer, "Test Code", typeof(ArgumentException))) 
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.Upgrade, "HTTP/2.0, SHTTP/1.3, IRC/6.9, RTA/x11", typeof(ArgumentException))) 
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.Via, "1.0 fred, 1.1 nowhere.com (Apache/1.1)", typeof(ArgumentException))) 
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.Vary, "*", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.Warning, "TestWarning", typeof(ArgumentException))) 
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.WWWAuthenticate, "WWW-Authenticate", typeof(ArgumentException))) 
                    result = MFTestResults.Fail;

            }
            catch (Exception ex)
            {
                Log.Exception("[Client] Unexpected Exception", ex);
                result = MFTestResults.Fail;
            }

            return result;
        }


        [TestMethod]
        public MFTestResults TestSetHTTPRequestHeaderAfterCreateHTTP1_0()
        {
            MFTestResults result = MFTestResults.Pass;
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/");
            wr.UserAgent = ".Net Micro Framwork Device/4.0";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1
            wr.ProtocolVersion = new Version(1, 0);
            Log.Comment("Set Version 1.0");

            try
            {
                // Setup server
                System.Net.WebHeaderCollection wrc = wr.Headers;

                if (!VerifyHeaderIsIllegal(wrc, HttpKnownHeaderNames.Accept, "text/plain; q=0.5, text/html, text/x-dvi; q=0.8, text/x-c", typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyHeaderIsIllegal(wrc, HttpKnownHeaderNames.Connection, "close", typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyHeaderIsIllegal(wrc, HttpKnownHeaderNames.ContentLength, "26012", typeof(ArgumentException))) result = MFTestResults.Fail;
                //if (!VerifyHeaderIsIllegal(wrc, HttpKnownHeaderNames.ContentType, "image/gif", typeof(ArgumentException))) result = MFTestResults.Fail;
                //if (!VerifyHeaderIsIllegal(wrc, HttpKnownHeaderNames.Date, System.DateTime.Today.ToString(), typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyHeaderIsIllegal(wrc, HttpKnownHeaderNames.Expect, "100", typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyHeaderIsIllegal(wrc, HttpKnownHeaderNames.Host, "www.w3.org", typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyHeaderIsIllegal(wrc, HttpKnownHeaderNames.IfModifiedSince, "Sat, 23 May 2009 19:43:31 GMT", typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyHeaderIsIllegal(wrc, HttpKnownHeaderNames.Range, "500-999", typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyHeaderIsIllegal(wrc, HttpKnownHeaderNames.Referer, "http://www.w3.org/hypertext/DataSources/Overview.html", typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyHeaderIsIllegal(wrc, HttpKnownHeaderNames.TransferEncoding, "chunked", typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyHeaderIsIllegal(wrc, HttpKnownHeaderNames.UserAgent, ".NetMicroFramework", typeof(ArgumentException))) result = MFTestResults.Fail;

            }
            catch (Exception ex)
            {
                Log.Exception("[Client] Unexpected Exception", ex);
                result = MFTestResults.Fail;
            }

            return result;
        }


        public MFTestResults ValidateAbleToSetPropertiesValueHTTP1_0()
        {
            MFTestResults result = MFTestResults.Pass;
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/");
            wr.UserAgent = ".Net Micro Framwork Device/4.0";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            wr.ProtocolVersion = new Version(1, 0);
            Log.Comment("Set Version 1.0");

            try
            {
                System.Net.WebHeaderCollection wrc = wr.Headers;

                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.AcceptCharset, "iso-8859-5, unicode-1-1;q=0.8", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.AcceptEncoding, "compress;q=0.5, gzip;q=1.0", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.AcceptLanguage, "en-US", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.Age, "2 days", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.Allow, "GET, PUT", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.CacheControl, "no-cache", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.ContentEncoding, "gzip", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.ContentLanguage, "mi, en", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.ContentMD5, "60e985979f1d55ab7542440fbb9659e5", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.ContentRange, "bytes 21010-47021/47022", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.Cookie, "www.google.com", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.Expires, "Thu, 01 Dec 1994 16:00:00 GMT", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.From, "webmaster@w3.org", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.IfMatch, "r2d2xxxx", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.IfNoneMatch, "xyzzy", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.IfRange, "TestIfRange: Need to have Range Header.", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.IfUnmodifiedSince, "Fri, 22 May 2009 12:43:31 GMT", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.KeepAlive, "true", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.LastModified, "Fri, 22 May 2009 12:43:31 GMT", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.MaxForwards, "10", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.Pragma, "no-cache", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.ProxyAuthenticate, "", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.ProxyAuthorization, "", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.RetryAfter, "100000", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.Server, "", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.SetCookie, "www.microsoft.com", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.SetCookie2, "www.bing.com", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.TE, "trailers, deflate;q=0.5", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.Trailer, "Test Code", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.Upgrade, "HTTP/2.0, SHTTP/1.3, IRC/6.9, RTA/x11", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.Via, "1.0 fred, 1.1 nowhere.com (Apache/1.1)", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.Vary, "*", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.Warning, "TestWarning", typeof(ArgumentException)))
                    result = MFTestResults.Fail;
                if (!VerifyHeaderIsLegal(wrc, HttpKnownHeaderNames.WWWAuthenticate, "WWW-Authenticate", typeof(ArgumentException)))
                    result = MFTestResults.Fail;


            }
            catch (Exception ex)
            {
                Log.Exception("[Client] Unexpected Exception", ex);
                result = MFTestResults.Fail;
            }

            return result;
        }

        #endregion Test

    }
}
