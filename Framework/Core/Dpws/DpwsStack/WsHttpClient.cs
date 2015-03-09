using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using System.Text;

using System.Ext;

using Ws.Services.Mtom;
using Ws.Services.Soap;
using Ws.Services.Binding;
using Ws.Services.WsaAddressing;
using System.Xml;

namespace Ws.Services.Transport.HTTP
{
    /// <summary>
    /// Class used to send http request/response messages.
    /// </summary>
    public class WsHttpClient
    {
        private ProtocolVersion m_version;
        
        /// <summary>
        /// Creates an instance of a WsHttpClient class.
        /// </summary>
        public WsHttpClient(ProtocolVersion version)
        {
            m_version = version;
        }

        /// <summary>
        /// Gets or sets the time in milliseconds this client will wait to send data to the remote endpoint.
        /// The defaule value is 60000 or 1 minute.
        /// </summary>
        public int SendTimeout = 60000;

        /// <summary>
        /// Gets or sets the time in milliseconds that this client will wait for data from the remote endpoint.
        /// </summary>
        /// <remarks>The default value is 60000 or 1 minute.</remarks>
        public int ReceiveTimeout = 60000;

        /// <summary>
        /// Gets or sets the time in milliseconds that this client will wait for a HTTP response from the remote endpoint.
        /// </summary>
        /// <remarks>The default value is 60000 or 1 minute.</remarks>
        public int RequestTimeout = 60000;

        /// <summary>
        /// Send an Http request to an endpoint and waits for a response.
        /// </summary>
        /// <param name="soapMessage">A byte array containing the soap message to be sent.</param>
        /// <param name="remoteEndpoint">A sting containing the name of a remote listening endpoint.</param>
        /// <returns>
        /// A WsMessage containing a soap response to the request. This array will be null for OneWay request.
        /// </returns>
        public WsMessage SendRequest(WsMessage request, Uri remoteEndpoint)
        {
            WsMessage response = new WsMessage(new WsWsaHeader(), null, WsPrefix.None);

            HttpTransportBindingElement httpClient = new HttpTransportBindingElement(new HttpTransportBindingConfig(remoteEndpoint));
            ClientBindingContext ctx = new ClientBindingContext(m_version);

            ctx.ReceiveTimeout = new TimeSpan(0, 0, 0, 0, ReceiveTimeout);
            ctx.OpenTimeout    = new TimeSpan(0, 0, 0, 0, RequestTimeout);
            ctx.SendTimeout    = new TimeSpan(0, 0, 0, 0, SendTimeout);

            httpClient.EndpointAddress = remoteEndpoint;

            Stream stream = null;

            try
            {
                httpClient.Open(ref stream, ctx);

                httpClient.ProcessOutputMessage(ref request, ctx);

                ctx.BindingProperties.Clear();

                httpClient.ProcessInputMessage(ref response, ctx);

                ctx.BindingProperties.Clear();

                if(response.Body is byte[])
                {
                    response.Reader = WsSoapMessageParser.ParseSoapMessage((byte[])response.Body, ref response.Header, m_version);
                }
            }
            finally
            {
                httpClient.Close(stream, ctx);
            }

            return response;
        }

        public void SendRequestOneWay(WsMessage request, Uri remoteEndpoint)
        {
            WsMessage response = new WsMessage(new WsWsaHeader(), null, WsPrefix.None);

            HttpTransportBindingElement httpClient = new HttpTransportBindingElement(new HttpTransportBindingConfig(remoteEndpoint));
            ClientBindingContext ctx = new ClientBindingContext(m_version);
            httpClient.EndpointAddress = remoteEndpoint;

            ctx.OpenTimeout    = new TimeSpan(0, 0, 0, 0, RequestTimeout);
            ctx.SendTimeout    = new TimeSpan(0, 0, 0, 0, SendTimeout);

            Stream stream = null;

            try
            {
                httpClient.Open(ref stream, ctx);
                httpClient.ProcessOutputMessage(ref request, ctx);
            }
            finally
            {
                httpClient.Close(stream, ctx);
            }
        }
    }
}


