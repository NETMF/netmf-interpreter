////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.Net;
using System.IO;
using System.Text;
using System.Xml;

namespace Microsoft.SPOT.Platform.Tests
{
    public class FunctionalTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");
            try
            {
                // Check networking - we need to make sure we can reach our proxy server
                Dns.GetHostEntry(HttpTests.Proxy);
            }
            catch (Exception ex)
            {
                Log.Exception("Unable to get address for " + HttpTests.Proxy, ex);
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

        [TestMethod]
        public MFTestResults VisitMicrosoft()
        {
            try
            {
                Log.Comment("Small web page - redirect");
                // Print for now, Parse later
                string data = new string(Encoding.UTF8.GetChars(GetRequested("http://www.microsoft.com", "APACHE")));
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
                return MFTestResults.Fail;
            }
            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults VisitNYTimes()
        {
            try
            {
                Log.Comment("SUN web page");
                // Print for now, Parse later
                string data = new string(Encoding.UTF8.GetChars(GetRequested("http://www.nytimes.com", "SUN", "APACHE")));
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
                return MFTestResults.Fail;
            }
            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults VisitApache()
        {
            try
            {
                Log.Comment("Apache Web server");
                // Print for now, Parse later
                string data = new string(Encoding.UTF8.GetChars(GetRequested("http://www.apache.org", "Apache")));
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
                return MFTestResults.Fail;
            }
            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults VisitGoogle()
        {
            try
            {
                Log.Comment("Google Web server");
                // Print for now, Parse later
                string data = new string(Encoding.UTF8.GetChars(GetRequested("http://www.google.com", "GWS")));
            }
            catch (ArgumentException) { /* Don't care if google doesn't return wrong header, happens at major 'holidays' like april 1 */ }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
                return MFTestResults.Fail;
            }
            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults VisitLighttpd()
        {
            try
            {
                Log.Comment("Lighttpd Web server");
                // Print for now, Parse later
                string data = new string(Encoding.UTF8.GetChars(GetRequested("http://redmine.lighttpd.net", "Lighttpd")));
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
                return MFTestResults.Fail;
            }
            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults GetMSDNRSS()
        {
            Debug.GC(true);
            try
            {
                Log.Comment("Get RSS feed");
                byte[] page = GetRequested("http://msdn.microsoft.com/en-us/magazine/rss/default.aspx?z=z&iss=1", "IIS");
                Log.Comment("Create xml document from feed");
                Log.Comment("Raw Page");
                string data = new string(Encoding.UTF8.GetChars(page));
                using (MemoryStream stream = new MemoryStream(page))
                {
                    XmlReader reader = XmlReader.Create(stream);
                    Log.Comment("Iterate through document");
                    string lastElement = "";
                    while (!reader.EOF)
                    {
                        reader.Read();
                        switch (reader.NodeType)
                        {
                            // ignores
                            case XmlNodeType.Whitespace:
                                break;
                            case XmlNodeType.XmlDeclaration:
                                break;
                            case XmlNodeType.ProcessingInstruction:
                                break;
                            case XmlNodeType.EndElement:
                                break;

                            // Process
                            case XmlNodeType.Element:
                                if (lastElement.Length > 0)
                                    Log.Comment(lastElement);
                                lastElement = reader.Name;
                                break;
                            case XmlNodeType.Text:
                                Log.Comment(lastElement + " - " + reader.Value);
                                lastElement = "";
                                break;

                            // Unknown
                            default:
                                Log.Comment("Default : " + reader.NodeType.ToString() + " : " + reader.Name + " : " + reader.Value);
                                break;
                        }
                    }
                }
            }
            catch (OutOfMemoryException)
            {
                // not much we can do here, consider it a pass
            }
            catch(NotSupportedException nse)
            {
                Log.Exception("Device does not support XML", nse );
                return MFTestResults.Skip;
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
                return MFTestResults.Fail;
            }
            return MFTestResults.Pass;
        }

        private byte[] GetRequested(string uri, params string[] servers)
        {
            byte[] page = null;

            // Create request.
            HttpWebRequest request = HttpWebRequest.Create(uri) as HttpWebRequest;
            // Get response from server.
            WebResponse resp = null;
            try
            {
                resp = request.GetResponse();
            }
            catch (Exception e)
            {
                Log.Exception("GetResponse Exception", e);
                throw e;
            }

            try
            {
                // Get Network response stream
                if (resp != null)
                {
                    Log.Comment("Headers - ");
                    foreach (string header in resp.Headers.AllKeys)
                    {
                        Log.Comment("    " + header + ": " + resp.Headers[header]);
                    }

                    using (Stream respStream = resp.GetResponseStream())
                    {
                        // Get all data:
                        if (resp.ContentLength != -1)
                        {
                            int respLength = (int)resp.ContentLength;
                            page = new byte[respLength];

                            // Now need to read all data. We read in the loop until resp.ContentLength or zero bytes read.
                            // Zero bytes read means there was error on server and it did not send all data.
                            for (int totalBytesRead = 0; totalBytesRead < respLength; )
                            {
                                int bytesRead = respStream.Read(page, totalBytesRead, respLength - totalBytesRead);
                                // If nothing is read - means server closed connection or timeout. In this case no retry.
                                if (bytesRead == 0)
                                {
                                    break;
                                }
                                // Adds number of bytes read on this iteration.  
                                totalBytesRead += bytesRead;
                            } 
                        }
                        else
                        {
                            byte[] byteData = new byte[4096];
                            char[] charData = new char[4096];
                            string data = null;
                            int bytesRead = 0;
                            Decoder UTF8decoder = System.Text.Encoding.UTF8.GetDecoder();
                            int totalBytes = 0;
                            while ((bytesRead = respStream.Read(byteData, 0, byteData.Length)) > 0)
                            {
                                int byteUsed, charUsed;
                                bool completed = false;
                                totalBytes += bytesRead;
                                UTF8decoder.Convert(byteData, 0, bytesRead, charData, 0, bytesRead, true, out byteUsed, out charUsed, out completed);
                                data = data + new String(charData, 0, charUsed);
                                Log.Comment("Bytes Read Now: " + bytesRead + " Total: " + totalBytes);
                            }
                            Log.Comment("Total bytes downloaded in message body : " + totalBytes);
                            page = Encoding.UTF8.GetBytes(data);
                        }

                        Log.Comment("Page downloaded");

                        respStream.Close();
                    }

                    bool fFoundExpectedServer = false;
                    string httpServer = resp.Headers["server"].ToLower();
                    foreach(string server in servers)
                    {
                        if (httpServer.IndexOf(server.ToLower()) >= 0)
                        {
                            fFoundExpectedServer = true;
                            break;
                        }
                    }
                    if(!fFoundExpectedServer)
                    {
                        Log.Exception("Expected server: " + servers[0] + ", but got server: " + resp.Headers["Server"]);
                        throw new ArgumentException("Unexpected Server type");
                    }

                    resp.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception processing response", ex);
                throw ex;
            }
            return page;
        }
    }
}
