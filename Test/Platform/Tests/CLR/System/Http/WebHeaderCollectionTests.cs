////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.Net;

namespace Microsoft.SPOT.Platform.Tests
{
    public class WebHeaderCollectionTests : IMFTestInterface
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


        #region Helper methods

        private MFTestResults VerifyHttpHeaders(System.Net.WebHeaderCollection wrc, System.Net.WebHeaderCollection wrs)
        {
            MFTestResults result = MFTestResults.Pass;

            try
            {
                string[] headers = wrc.AllKeys;
                string sValue = String.Empty;

                for (int i = 0; i < wrc.Count; i++)
                {
                    sValue = wrc[headers[i]];
                    if (sValue != wrs[headers[i]])
                    {
                        Log.Exception(headers[i] + "property value is incorrect.");
                        result = MFTestResults.Fail;
                    }
                }
            }
            catch (Exception ex)
            {
                if (!HttpTests.ValidateException(ex, typeof(InvalidOperationException)))
                    result = MFTestResults.Fail;
            }

            return result;
        }


        private bool VerifyAddHeaderIsIllegal(WebHeaderCollection wrc, string header, string content, Type exceptionType)
        {
            bool res = true;

            try
            {
                Log.Comment("Set Headers Properties for header: '" + header + "', with value: '" + content + "'");

                if (null == content)
                    wrc.Add(header);
                else
                    wrc.Add(header, content);

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

                if(null == content)
                    wrc.Add(header);
                else
                    wrc.Add(header, content);

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
        public MFTestResults TestWebHeaderCollectionAddIllegal()
        {
            MFTestResults result = MFTestResults.Pass;

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/");  //expect 200 - OK
            wr.UserAgent = ".Net Micro Framwork Device/4.0";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            try
            {
                WebHeaderCollection wrc = wr.Headers;

                //Attempt to add Restricted header
                if (!VerifyAddHeaderIsIllegal(wrc, HttpKnownHeaderNames.Accept, null, typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyAddHeaderIsIllegal(wrc, HttpKnownHeaderNames.Connection, null, typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyAddHeaderIsIllegal(wrc, HttpKnownHeaderNames.ContentLength, null, typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyAddHeaderIsIllegal(wrc, HttpKnownHeaderNames.ContentType, null, typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyAddHeaderIsIllegal(wrc, HttpKnownHeaderNames.Date, null, typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyAddHeaderIsIllegal(wrc, HttpKnownHeaderNames.Expect, null, typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyAddHeaderIsIllegal(wrc, HttpKnownHeaderNames.Host, null, typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyAddHeaderIsIllegal(wrc, HttpKnownHeaderNames.IfModifiedSince, null, typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyAddHeaderIsIllegal(wrc, HttpKnownHeaderNames.Range, null, typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyAddHeaderIsIllegal(wrc, HttpKnownHeaderNames.Referer, null, typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyAddHeaderIsIllegal(wrc, HttpKnownHeaderNames.TransferEncoding, null, typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyAddHeaderIsIllegal(wrc, HttpKnownHeaderNames.UserAgent, null, typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyAddHeaderIsIllegal(wrc, HttpKnownHeaderNames.ProxyConnection, null, typeof(ArgumentException))) result = MFTestResults.Fail;

            }
            catch (Exception ex)
            {
                Log.Exception("[Client] Unexpected Exception", ex);
                result = MFTestResults.Fail;
            }

            return result;
        }


        [TestMethod]
        public MFTestResults TestWebHeaderCollectionAddIllegal2()
        {
            MFTestResults result = MFTestResults.Pass;

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/");  //expect 200 - OK
            wr.UserAgent = ".Net Micro Framwork Device/4.0";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            try
            {
                WebHeaderCollection wrc = wr.Headers;

                //Attempt to add Restricted header
                if (!VerifyAddHeaderIsIllegal(wrc, HttpKnownHeaderNames.Accept, "text/plain; q=0.5, text/html, text/x-dvi; q=0.8, text/x-c", typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyAddHeaderIsIllegal(wrc, HttpKnownHeaderNames.Connection, "close", typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyAddHeaderIsIllegal(wrc, HttpKnownHeaderNames.ContentLength, "26012", typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyAddHeaderIsIllegal(wrc, HttpKnownHeaderNames.ContentType, "image/gif", typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyAddHeaderIsIllegal(wrc, HttpKnownHeaderNames.Date, System.DateTime.Today.ToString(), typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyAddHeaderIsIllegal(wrc, HttpKnownHeaderNames.Expect, "100", typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyAddHeaderIsIllegal(wrc, HttpKnownHeaderNames.Host, "www.w3.org", typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyAddHeaderIsIllegal(wrc, HttpKnownHeaderNames.IfModifiedSince, "Sat, 23 May 2009 19:43:31 GMT", typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyAddHeaderIsIllegal(wrc, HttpKnownHeaderNames.Range, "500-999", typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyAddHeaderIsIllegal(wrc, HttpKnownHeaderNames.Referer, "http://www.w3.org/hypertext/DataSources/Overview.html", typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyAddHeaderIsIllegal(wrc, HttpKnownHeaderNames.TransferEncoding, "chunked", typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyAddHeaderIsIllegal(wrc, HttpKnownHeaderNames.UserAgent, ".NetMicroFramework", typeof(ArgumentException))) result = MFTestResults.Fail;
                if (!VerifyAddHeaderIsIllegal(wrc, HttpKnownHeaderNames.ProxyConnection, "www.microsoft.com", typeof(ArgumentException))) result = MFTestResults.Fail;

            }
            catch (Exception ex)
            {
                Log.Exception("[Client] Unexpected Exception", ex);
                result = MFTestResults.Fail;
            }

            return result;
        }


        [TestMethod]
        public MFTestResults TestWebHeaderCollectionAddLegal2()
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
        public MFTestResults TestWebHeaderCollectionIsRestricted()
        {
            MFTestResults result = MFTestResults.Pass;

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://"+ Utilities.GetLocalIpAddress() + ":" + HttpServer.s_CurrentPort.ToString() + "/");  //expect 200 - OK
            wr.UserAgent = ".Net Micro Framwork Device/4.0";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            try
            {
                WebHeaderCollection wrc = wr.Headers;

                //Attempt to add Restricted header
                if (!WebHeaderCollection.IsRestricted(HttpKnownHeaderNames.Accept)) result = MFTestResults.Fail;
                if (!WebHeaderCollection.IsRestricted(HttpKnownHeaderNames.Connection)) result = MFTestResults.Fail;
                if (!WebHeaderCollection.IsRestricted(HttpKnownHeaderNames.ContentLength)) result = MFTestResults.Fail;
                if (!WebHeaderCollection.IsRestricted(HttpKnownHeaderNames.ContentType)) result = MFTestResults.Fail;
                if (!WebHeaderCollection.IsRestricted(HttpKnownHeaderNames.Date)) result = MFTestResults.Fail;
                if (!WebHeaderCollection.IsRestricted(HttpKnownHeaderNames.Expect)) result = MFTestResults.Fail;
                if (!WebHeaderCollection.IsRestricted(HttpKnownHeaderNames.Host)) result = MFTestResults.Fail;
                if (!WebHeaderCollection.IsRestricted(HttpKnownHeaderNames.IfModifiedSince)) result = MFTestResults.Fail;
                if (!WebHeaderCollection.IsRestricted(HttpKnownHeaderNames.Range)) result = MFTestResults.Fail;
                if (!WebHeaderCollection.IsRestricted(HttpKnownHeaderNames.Referer)) result = MFTestResults.Fail;
                if (!WebHeaderCollection.IsRestricted(HttpKnownHeaderNames.TransferEncoding)) result = MFTestResults.Fail;
                if (!WebHeaderCollection.IsRestricted(HttpKnownHeaderNames.UserAgent)) result = MFTestResults.Fail;
                if (!WebHeaderCollection.IsRestricted(HttpKnownHeaderNames.ProxyConnection)) result = MFTestResults.Fail;

            }
            catch (Exception ex)
            {
                Log.Exception("[Client] Unexpected Exception", ex);
                result = MFTestResults.Fail;
            }

            return result;
        }


        [TestMethod]
        public MFTestResults TestWebHeaderCollectionRemove()
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

                int initialCount = wrc.Count;

                //set and remove individual header
                Log.Comment("Set and Remove HttpKnownHeaderNames.AcceptCharset");
                wrc.Set(HttpKnownHeaderNames.AcceptCharset, "iso-8859-5, unicode-1-1;q=0.8");
                wrc.Remove(HttpKnownHeaderNames.AcceptCharset);

                if (wrc.Count != initialCount)
                {
                    result = MFTestResults.Fail;
                }

                Log.Comment("Set and Remove HttpKnownHeaderNames.AcceptEncoding");
                wrc.Set(HttpKnownHeaderNames.AcceptEncoding, "compress;q=0.5, gzip;q=1.0");
                wrc.Remove(HttpKnownHeaderNames.AcceptEncoding);

                if (wrc.Count != initialCount)
                {
                    result = MFTestResults.Fail;
                }

                Log.Comment("Set and Remove HttpKnownHeaderNames.AcceptLanguage");
                wrc.Set(HttpKnownHeaderNames.AcceptLanguage, "en-US");
                wrc.Remove(HttpKnownHeaderNames.AcceptLanguage);

                if (wrc.Count != initialCount)
                {
                    result = MFTestResults.Fail;
                }
                
                //Set and remove group of headers
                Log.Comment("Set group of headers...");
                wrc.Set(HttpKnownHeaderNames.Age, "2 days");
                wrc.Set(HttpKnownHeaderNames.Allow, "GET, PUT");
                wrc.Set(HttpKnownHeaderNames.CacheControl, "no-cache");
                wrc.Set(HttpKnownHeaderNames.ContentEncoding, "gzip");
                wrc.Set(HttpKnownHeaderNames.ContentLanguage, "mi, en");
                wrc.Set(HttpKnownHeaderNames.ContentMD5, "60e985979f1d55ab7542440fbb9659e5");
                wrc.Set(HttpKnownHeaderNames.ContentRange, "bytes 21010-47021/47022");
                wrc.Set(HttpKnownHeaderNames.Cookie, "www.google.com");
                wrc.Set(HttpKnownHeaderNames.Expires, "Thu, 01 Dec 1994 16:00:00 GMT");
                wrc.Set(HttpKnownHeaderNames.From, "webmaster@w3.org");
                wrc.Set(HttpKnownHeaderNames.IfMatch, "r2d2xxxx");
                wrc.Set(HttpKnownHeaderNames.IfNoneMatch, "xyzzy");
                wrc.Set(HttpKnownHeaderNames.IfRange, "TestIfRange: Need to have Range Header.");
                wrc.Set(HttpKnownHeaderNames.IfUnmodifiedSince, "Fri, 22 May 2009 12:43:31 GMT");
                wrc.Set(HttpKnownHeaderNames.KeepAlive, "true");
                wrc.Set(HttpKnownHeaderNames.LastModified, "Fri, 22 May 2009 12:43:31 GMT");
                wrc.Set(HttpKnownHeaderNames.MaxForwards, "10");
                wrc.Set(HttpKnownHeaderNames.Pragma, "no-cache");
                wrc.Set(HttpKnownHeaderNames.ProxyAuthenticate, "");
                wrc.Set(HttpKnownHeaderNames.ProxyAuthorization, "");
                wrc.Set(HttpKnownHeaderNames.RetryAfter, "100000");
                wrc.Set(HttpKnownHeaderNames.Server, "");
                wrc.Set(HttpKnownHeaderNames.SetCookie, "www.microsoft.com");
                wrc.Set(HttpKnownHeaderNames.SetCookie2, "www.bing.com");
                wrc.Set(HttpKnownHeaderNames.TE, "trailers, deflate;q=0.5");
                wrc.Set(HttpKnownHeaderNames.Trailer, "Test Code");
                wrc.Set(HttpKnownHeaderNames.Upgrade, "HTTP/2.0, SHTTP/1.3, IRC/6.9, RTA/x11");
                wrc.Set(HttpKnownHeaderNames.Via, "1.0 fred, 1.1 nowhere.com (Apache/1.1)");
                wrc.Set(HttpKnownHeaderNames.Vary, "*");
                wrc.Set(HttpKnownHeaderNames.Warning, "TestWarning");
                wrc.Set(HttpKnownHeaderNames.WWWAuthenticate, "WWW-Authenticate");

                //remove headers
                Log.Comment("Remove group of headers...");
                wrc.Remove(HttpKnownHeaderNames.Age);
                wrc.Remove(HttpKnownHeaderNames.Allow);
                wrc.Remove(HttpKnownHeaderNames.CacheControl);
                wrc.Remove(HttpKnownHeaderNames.ContentEncoding);
                wrc.Remove(HttpKnownHeaderNames.ContentLanguage);
                wrc.Remove(HttpKnownHeaderNames.ContentMD5);
                wrc.Remove(HttpKnownHeaderNames.ContentRange);
                wrc.Remove(HttpKnownHeaderNames.Cookie);
                wrc.Remove(HttpKnownHeaderNames.Expires);
                wrc.Remove(HttpKnownHeaderNames.From);
                wrc.Remove(HttpKnownHeaderNames.IfMatch);
                wrc.Remove(HttpKnownHeaderNames.IfNoneMatch);
                wrc.Remove(HttpKnownHeaderNames.IfRange);
                wrc.Remove(HttpKnownHeaderNames.IfUnmodifiedSince);
                wrc.Remove(HttpKnownHeaderNames.KeepAlive);
                wrc.Remove(HttpKnownHeaderNames.LastModified);
                wrc.Remove(HttpKnownHeaderNames.MaxForwards);
                wrc.Remove(HttpKnownHeaderNames.Pragma);
                wrc.Remove(HttpKnownHeaderNames.ProxyAuthenticate);
                wrc.Remove(HttpKnownHeaderNames.ProxyAuthorization);
                wrc.Remove(HttpKnownHeaderNames.RetryAfter);
                wrc.Remove(HttpKnownHeaderNames.Server);
                wrc.Remove(HttpKnownHeaderNames.SetCookie);
                wrc.Remove(HttpKnownHeaderNames.SetCookie2);
                wrc.Remove(HttpKnownHeaderNames.TE);
                wrc.Remove(HttpKnownHeaderNames.Trailer);
                wrc.Remove(HttpKnownHeaderNames.Upgrade);
                wrc.Remove(HttpKnownHeaderNames.Via);
                wrc.Remove(HttpKnownHeaderNames.Vary);
                wrc.Remove(HttpKnownHeaderNames.Warning);
                wrc.Remove(HttpKnownHeaderNames.WWWAuthenticate);

                if (wrc.Count != initialCount)
                {
                    result = MFTestResults.Fail;
                }

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
