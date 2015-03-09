using System;
using System.Net;
using System.IO;
using System.Collections;
using Ws.Services.Soap;
using Ws.Services.Encoding;

namespace Ws.Services.Binding
{
    /// <summary>
    /// The colleciotn of binding elements that define the communication mechanism between two peers that use a WS2007HttpBinding taht is compatible with the homonymous binding of teh WCF object model
    /// </summary>        
    public class WS2007HttpBinding : Binding
    {
        private static int s_nextPort = 8090;

        /// <summary>
        /// Creates an instance of the WS2007HttpBinding class
        /// </summary>
        public WS2007HttpBinding() : this( new HttpTransportBindingConfig("urn:uuid:" + Guid.NewGuid().ToString(), s_nextPort++), null )
        {
        }

        /// <summary>
        /// Creates an instance of the WS2007HttpBinding class
        /// </summary>
        /// <param name="httpConfig">The configuration associated with this binding.</param>
        public WS2007HttpBinding( HttpTransportBindingConfig httpConfig ) : this( httpConfig, null )
        {
        }

        /// <summary>
        /// Creates an instance of the WS2007HttpBinding class
        /// </summary>
        /// <param name="httpConfig">The configuration associated with this binding.</param>
        /// <param name="bindingParams">The additional elements associated with this binding.</param>
        public WS2007HttpBinding( HttpTransportBindingConfig httpConfig, ArrayList bindingParams )
            : base(new HttpTransportBindingElement(httpConfig), new TextMessageEncodingBindingElement())   
        {
            if(bindingParams != null)
            {
                base.Elements.SetBindingConfiguration(bindingParams);
            }
        }
    }
}

