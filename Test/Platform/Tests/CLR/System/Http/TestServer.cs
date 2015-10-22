using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.Net;
using System.Threading;

namespace Microsoft.SPOT.Platform.Tests
{
    public class HttpTestServer
    {
        private HttpListener listener;
        private MFTestResults result;
        private bool running = false;
        AutoResetEvent evtStarted = new AutoResetEvent(false);
        Thread server;

        public static int s_CurrentPort = 8080;

        public HttpTestServer(string prefix, ref MFTestResults Result)
        {
            listener = new HttpListener(prefix, s_CurrentPort);
            result = Result;
        }

        public HttpTestServer(string prefix, int Port, ref MFTestResults Result)
        {
            listener = new HttpListener(prefix, Port);
            result = Result;
        }

        public string ResponseString { get; set; }

        public bool SendChunked { get; set; }

        public Uri RequestUri { get; set; }
        public WebHeaderCollection RequestHeaders { get; set; }

        public void StartServer()
        {
            server = new Thread(new ThreadStart(RunServer));
            server.Start();

            if (!evtStarted.WaitOne(5000, false))
            {
                s_CurrentPort++;
                throw new Exception("Unable to start server");
            }

            s_CurrentPort++;
        }

        public void StopServer()
        {
            if (running)
            {
                running = false;
                listener.Abort();
                server.Join();
            }
        }

        private void RunServer()
        {
            try
            {
                running = true;
                listener.Start();
                Log.Comment("[Server] Starting listener...");

                evtStarted.Set();

                while (running)
                {
                    HttpListenerContext context = listener.GetContext();
                    if (context == null)
                        continue;

                    HttpListenerRequest request = context.Request;

                    // request header validation
                    if (this.RequestHeaders != null)
                    {
                        foreach (string header in this.RequestHeaders.AllKeys)
                        {
                            if (this.RequestHeaders[header] != request.Headers[header])
                            {
                                this.result = MFTestResults.Fail;
                                Log.Exception("[Server] Expected " + header + ":" + this.RequestHeaders[header] +
                                    ", but got " + header + ":" + request.Headers[header]);
                            }
                        }
                    }

                    HttpTests.PrintHeaders("Server", request.Headers);
                    // BUGBUG: Check inbound stream once 54503 & 54507 are fixed
                    //HttpTests.ReadStream("Client", request.InputStream);

                    HttpListenerResponse response = context.Response;
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(this.ResponseString);
                    response.Headers.Add("Server", ".Net Micro Framework Device/4.0");
                    response.SendChunked = this.SendChunked;
                    if (!this.SendChunked)
                        response.ContentLength64 = buffer.Length;

                    response.OutputStream.Write(buffer, 0, buffer.Length);
                    response.Close();
                }
                listener.Stop();
            }
            catch (Exception ex)
            {
                Log.Exception("[Server] Unexpected Server Exception", ex);
            }
            finally
            {
                if (listener.IsListening)
                    listener.Abort();

                running = false;
            }
        }
    }
}