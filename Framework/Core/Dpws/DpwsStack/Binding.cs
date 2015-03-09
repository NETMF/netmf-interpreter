using System;
using System.Net;
using System.IO;
using System.Collections;
using Ws.Services.Soap;
using Ws.Services.Serialization;
using Ws.Services.WsaAddressing;
using System.Xml;
using Ws.Services.Encoding;

namespace Ws.Services.Binding
{   
    /// <summary>
    /// ICommunicationObject interface defines a set of timeout properties that are
    /// applied to the implementing class's message context
    /// </summary>
    public interface ICommunicationObject
    {
        /// <summary>
        /// The amount of time allowed for a communication object to open a
        /// connection.
        /// </summary>
        TimeSpan OpenTimeout    { get; set; }
        /// <summary>
        /// The amount of time allowed for a communication object to close a
        /// connection.
        /// </summary>
        TimeSpan CloseTimeout { get; set; }
        /// <summary>
        /// The amount of time allowed for a communication object to receive data.
        /// </summary>
        TimeSpan ReceiveTimeout { get; set; }
        /// <summary>
        /// The amount of time allowed for a communication object to send data.
        /// </summary>
        TimeSpan SendTimeout { get; set; }        
    }

    /// <summary>
    /// Default implementation of the ICommunicationObject interface.  Timeouts are:
    ///   Open    - 10 seconds
    ///   Close   -  1 minute
    ///   Receive -  5 minutes
    ///   Send    - 10 seconds
    /// </summary>
    public class CommunicationObject : ICommunicationObject
    {
        /// <summary>
        /// Default open timeout (10 seconds).
        /// </summary>
        public static readonly TimeSpan DefaultOpenTimeout    = new TimeSpan(0, 0, 10);
        /// <summary>
        /// Default close timeout (10 seconds).
        /// </summary>
        public static readonly TimeSpan DefaultCloseTimeout   = new TimeSpan(0, 0, 10);
        /// <summary>
        /// Default receive timeout (5 minutes).
        /// </summary>
        public static readonly TimeSpan DefaultReceiveTimeout = new TimeSpan(0, 5,  0);
        /// <summary>
        /// Default send timeout (10 seconds).
        /// </summary>
        public static readonly TimeSpan DefaultSendTimeout    = new TimeSpan(0, 0, 10);
        
        TimeSpan m_openTimeout;
        TimeSpan m_closeTimeout;
        TimeSpan m_receiveTimeout;
        TimeSpan m_sendTimeout;

        /// <summary>
        /// Default constructor
        /// </summary>
        public CommunicationObject()
        {
            this.m_openTimeout     = DefaultOpenTimeout;
            this.m_closeTimeout    = DefaultCloseTimeout;
            this.m_receiveTimeout  = DefaultReceiveTimeout;
            this.m_sendTimeout     = DefaultSendTimeout;
        }

        /// <summary>
        /// Gets or sets the timeout for an 'Open' operation.
        /// </summary>
        public TimeSpan OpenTimeout    { get { return m_openTimeout;    } set { m_openTimeout    = value; } }
        /// <summary>
        /// Gets or sets the timeout for an 'Close' operation.
        /// </summary>
        public TimeSpan CloseTimeout { get { return m_closeTimeout; } set { m_closeTimeout = value; } }
        /// <summary>
        /// Gets or sets the timeout for an 'Receive' operation.
        /// </summary>
        public TimeSpan ReceiveTimeout { get { return m_receiveTimeout; } set { m_receiveTimeout = value; } }
        /// <summary>
        /// Gets or sets the timeout for an 'Send' operation.
        /// </summary>
        public TimeSpan SendTimeout { get { return m_sendTimeout; } set { m_sendTimeout = value; } }        
    }

    /// <summary>
    /// The IRequestChannel interface defines the properties and methods for a DPWS request.
    /// </summary>
    public interface IRequestChannel
    {
        /// <summary>
        /// The remote endpoint address for the request.
        /// </summary>
        Uri RemoteAddress { get; }
        //Uri ViaAddress    { get; }
        
        /// <summary>
        /// Opens the communication channel to the remote endpoint.
        /// </summary>
        void Open();
        /// <summary>
        /// Sends the given request message to the remote endpoint and returns the result.
        /// </summary>
        /// <param name="message">Request message</param>
        /// <returns>Result message</returns>
        WsMessage Request ( WsMessage message );
        /// <summary>
        /// Sends the given one-way request message to the remote endpoint.
        /// </summary>
        /// <param name="message">One-way request message</param>
        void RequestOneWay( WsMessage message );
        /// <summary>
        /// Closes the communication channel with the remote endpoint.
        /// </summary>
        void Close();
    }

    /// <summary>
    /// An implementation of the IRequestChannel interface, which allows sending DPWS 
    /// request messages to a remote endpoint.
    /// </summary>
    public class RequestChannel : IRequestChannel
    {
        Binding        m_binding;
        BindingContext m_context;
        Stream         m_stream;

        /// <summary>
        /// RequestChannel can only be created via Binding.CreateClientChannel.
        /// </summary>
        /// <param name="binding">The binding associated with the request</param>
        /// <param name="context">The binding context associated with the request</param>
        internal RequestChannel( Binding binding, BindingContext context )
        {
            this.m_binding = binding;
            this.m_context = context;
        }

        /// <summary>
        /// Gets the remote endpoint address for this request channel.
        /// </summary>
        public Uri RemoteAddress { get { return m_binding.Transport.EndpointAddress; } }
        //public Uri ViaAddress    { get { return ((ClientBindingContext)m_context).ViaAddress;      } }

        /// <summary>
        /// Opens the communication channel with the remote endpoint.
        /// </summary>
        public void Open()
        {
            m_binding.Elements.Open( ref m_stream, m_context );
        }

        /// <summary>
        /// Closes the communication channel with the remote endpoint.
        /// </summary>
        public void Close()
        {
            m_binding.Elements.Close( m_stream, m_context );
        }

        /// <summary>
        /// Sends a one-way DPWS request to the remote endpoint.
        /// </summary>
        /// <param name="request">One-way request message</param>
        public void RequestOneWay( WsMessage request )
        {
            // Clone binding context so that properties added by the send/receive
            // do not persist beyond this request
            BindingContext ctx = m_context.Clone();

            ctx.ContextObject = m_context.ContextObject;

            SendMessage(request, ctx);

            m_context.ContextObject = ctx.ContextObject;
        }

        /// <summary>
        /// Sends a DPWS request to the remote endpoint and returns the response message.
        /// </summary>
        /// <param name="request">Request message</param>
        /// <returns>Response message for the given request</returns>
        public WsMessage Request(WsMessage request)
        {
            // Clone binding context so that properties added by the send/receive
            // do not persist beyond this request
            BindingContext ctx = m_context.Clone();
            
            SendMessage( request, ctx );

            return ReceiveMessage( request.Deserializer, ctx );
        }


        private void SendMessage( WsMessage message, BindingContext ctx )
        {
            m_binding.Elements.ProcessOutputMessage( ref message, ctx );
        }

        private WsMessage ReceiveMessage( object deserializer, BindingContext ctx )
        {
            WsMessage msg = new WsMessage( new WsWsaHeader(), null, WsPrefix.None );

            msg.Deserializer = deserializer;

            // reset the binding properties as they likely were modified during the Send
            ctx.BindingProperties = (ArrayList)m_context.BindingProperties.Clone();

            m_binding.Elements.ProcessInputMessage( ref msg, ctx );

            return msg;
        }

    }

    /// <summary>
    /// The RequestContext class contains information about the endpoint and binding context 
    /// for a given incomming request.  The data will be used when the message is processed. 
    /// This allows for asynchoronous behavior of message processing.
    /// </summary>
    public class RequestContext 
    {
        WsMessage    m_message;
        ReplyChannel m_channel;
        internal BindingContext m_context;

        /// <summary>
        /// A RequestContext object is created by the ReplyChannel when a request is received.
        /// </summary>
        /// <param name="message">The request incomming message</param>
        /// <param name="channel">The communication channel associated with the message</param>
        /// <param name="ctx">The binding context associated with the channel</param>
        internal RequestContext( WsMessage message, ReplyChannel channel, BindingContext ctx )
        {
            this.m_message = message;
            this.m_channel = channel;
            this.m_context = ctx;
        }
        
        /// <summary>
        /// Gets the incomming request message.
        /// </summary>
        public WsMessage Message { get { return this.m_message; } }

        /// <summary>
        /// Gets the ProtocolVersion associated with this message.
        /// </summary>
        public ProtocolVersion Version { get { return m_context.Version; } }

        /// <summary>
        /// Sends the given reply message to the remote endpoint.
        /// </summary>
        /// <param name="reply">Reply message for this request context</param>
        public void Reply( WsMessage reply )
        {
            m_channel.SendMessage( reply, m_context );
        }
    }

    /// <summary>
    /// Defines the interface to handle a reply
    /// </summary>
    public interface IReplyChannel
    {
        /// <summary>
        /// Extracts the Uri associated with this reply
        /// </summary>
        Uri Uri { get; }

        /// <summary>
        /// Opens the reply channel 
        /// </summary>
        void Open();
        /// <summary>
        ///  Closes the reply channel
        /// </summary>
        void Close();

        /// <summary>
        /// Extracts the request from the channel
        /// </summary>
        /// <returns>The Context of the request.</returns>
        RequestContext ReceiveRequest();
    }

    /// <summary>
    /// The channel to send a reply through
    /// </summary>
    public class ReplyChannel : IReplyChannel
    {
        Binding        m_binding;
        BindingContext m_context;
        Stream         m_stream;

        internal ReplyChannel( Binding binding, BindingContext context )
        {
            this.m_binding = binding;
            this.m_context = context;
        }

        /// <summary>
        /// Extracts the Uri associated with this reply
        /// </summary>
        public Uri Uri { get { return m_binding.Transport.EndpointAddress; } }

        /// <summary>
        /// Opens the reply channel 
        /// </summary>
        public void Open()
        {
            m_binding.Elements.Open( ref m_stream, m_context );
        }

        /// <summary>
        ///  Closes the reply channel
        /// </summary>
        public void Close()
        {
            m_binding.Elements.Close( m_stream,  m_context );
        }

        internal void SendMessage( WsMessage message, BindingContext ctx )
        {
            // Reset the binding properties as they were likely changed by the receive
            ctx.BindingProperties = (ArrayList)m_context.BindingProperties.Clone();

            m_binding.Elements.ProcessOutputMessage( ref message, ctx );
        }

        internal WsMessage ReceiveMessage( BindingContext ctx )
        {
            WsMessage msg = new WsMessage(new WsWsaHeader(), null, WsPrefix.None);

            while(true)
            {
                try
                {
                    if (!m_binding.Elements.ProcessInputMessage(ref msg, ctx))
                    {
                        msg = null;
                    }
                    // only need to loop if we received a bad request
                    break;
                }
                catch (Faults.WsFaultException ex)
                {
                    WsMessage faultResp = Faults.WsFault.GenerateFaultResponse(ex, ctx.Version);

                    if (faultResp != null)
                    {
                        SendMessage(faultResp, ctx);
                    }
                }
            }

            return msg;
        }

        /// <summary>
        /// Extracts the request from the channel
        /// </summary>
        /// <returns>The Context of the request.</returns>
        public RequestContext ReceiveRequest()
        {
            // Clone binding context so that properties added by the send/receive
            // do not persist beyond this request
            BindingContext ctx = m_context.Clone();

            WsMessage msg = ReceiveMessage(ctx);

            if (msg != null)
            {
                return new RequestContext(msg, this, ctx);
            }

            return null;
        }
    }

    /// <summary>
    /// Defines a property associated with a Binding
    /// </summary>
    public class BindingProperty
    {
        /// <summary>
        /// Contructs an instance of BindingProperty
        /// </summary>
        /// <param name="container">The container for the property.</param>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value for the property.</param>
        public BindingProperty( string container, string name, object value )
        {
            Name      = name;
            Value     = value;
            Container = container.ToLower();
        }
        
        /// <summary>
        /// Retrieves the name of the property
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// Retrieves the value of the property
        /// </summary>
        public readonly object Value;
        /// <summary>
        /// Retrieves the container of the property
        /// </summary>
        public readonly string Container; 
    }

    /// <summary>
    /// The context associated with a binding
    /// </summary>
    public class BindingContext : CommunicationObject
    {
        /// <summary>
        /// Retrieves the list of the properties associated with a binding 
        /// </summary>
        public ArrayList BindingProperties;
        /// <summary>
        /// Retrieves the ContextObject with this BindingContext
        /// </summary>
        public object    ContextObject;
        /// <summary>
        /// Retrieves the protocol version associated with this context
        /// </summary>
        public readonly ProtocolVersion Version;

        /// <summary>
        /// Creates an instance of a BindingContext object
        /// </summary>
        /// <param name="version">The protocol version associated with this context.</param>
        /// <param name="bindingProperties">The properties associated with this binding.</param>
        protected BindingContext( ProtocolVersion version, params BindingProperty[] bindingProperties )
        {
            Version = version;
            BindingProperties = new ArrayList();

            if (bindingProperties != null)
            {
                int len = bindingProperties.Length;

                for (int i = 0; i < len; i++)
                {
                    if(!WebHeaderCollection.IsRestricted(bindingProperties[i].Name))
                    {
                        this.BindingProperties.Add(bindingProperties[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Clones this object
        /// </summary>
        /// <returns></returns>
        public BindingContext Clone()
        {
            BindingContext ctx = (BindingContext)this.MemberwiseClone();

            ctx.BindingProperties = (ArrayList)BindingProperties.Clone();

            return ctx;
        }
        
    }

    /// <summary>
    ///  Defines the binding context for a client
    /// </summary>
    public class ClientBindingContext : BindingContext
    {
        /// <summary>
        /// Creates an instance of the ClientBindingContext class
        /// </summary>
        /// <param name="version">The protocol version associated with this context.</param>
        /// <param name="bindingProperties">The properties associated with this binding.</param>
        public ClientBindingContext( ProtocolVersion version,  params BindingProperty[] bindingProperties ) : base( version, bindingProperties )
        {
        }
    }

    /// <summary>
    ///  Defines the binding context for a server
    /// </summary>
    public class ServerBindingContext : BindingContext
    {
        /// <summary>
        /// Creates an instance of the ClientBindingContext class
        /// </summary>
        /// <param name="version">The protocol version associated with this context.</param>
        /// <param name="bindingProperties">The properties associated with this binding.</param>
        public ServerBindingContext( ProtocolVersion version, params BindingProperty[] bindingProperties ) : base( version, bindingProperties )
        {
        }
    }

    /// <summary>
    ///  Defines a custom binding
    /// </summary>
    public class CustomBinding : Binding
    {
        /// <summary>
        /// Creates an instance of a CustomBinding class
        /// </summary>
        /// <param name="transport">The transport associated with a custom binding class.</param>
        /// <param name="extraBindings">The additional binding elements associated with the customer binding.</param>
        public CustomBinding( TransportBindingElement transport, params BindingElement[] extraBindings ) 
            : base( transport, extraBindings )
        {
        }            
    }

    /// <summary>
    /// The collection of binding elements that define the communication mechanism between two peers
    /// </summary>
    public abstract class Binding
    {
        private Binding()
        {
        }

        /// <summary>
        /// Retrieves the <c>TransportBindingElement</c>
        /// </summary>
        public readonly TransportBindingElement Transport;
        /// <summary>
        /// Retrieves the <c>MessageEncodingBindingElement</c>
        /// </summary>
        public readonly MessageEncodingBindingElement Encoding;
        /// <summary>
        /// Retrieves any other <c>BindingElements</c> associated with this binding
        /// </summary>
        public readonly BindingElement Elements;

        /// <summary>
        /// Creates a BindingElement instance
        /// </summary>
        /// <param name="transport">The trasport of this binding element.</param>
        /// <param name="extraBindings">The other elements for ths binding.</param>
        protected Binding( TransportBindingElement transport, params BindingElement[] extraBindings )
        {
            this.Transport = transport;
            
            BindingElement prev = null;

            if(extraBindings != null)
            {
                int len = extraBindings.Length;

                if(len > 0)
                {
                    prev = null;
                    
                    for(int i=len-1; i>=0; i--)
                    {
                        BindingElement next = extraBindings[i];

                        if (prev == null)
                        {
                            this.Elements = next;
                        }
                        else
                        {
                            prev.SetNext(next);
                        }

                        prev = next;

                        if(next is MessageEncodingBindingElement)
                        {
                            this.Encoding = (MessageEncodingBindingElement)next;
                        }
                    }
                }
            }

            if (prev == null)
            {
                this.Encoding = new TextMessageEncodingBindingElement();
                this.Elements = this.Encoding;
                this.Elements.SetNext(this.Transport);
            }
            else if(this.Encoding == null)
            {
                this.Encoding = new TextMessageEncodingBindingElement();
                prev.SetNext(this.Encoding);
                this.Encoding.SetNext(this.Transport);
            }
            else
            {
                prev.SetNext(this.Transport);
            }
        }

        /// <summary>
        /// Creates the channel to communicate with the client
        /// </summary>
        /// <param name="context">The context associated with this channel.</param>
        /// <returns></returns>
        public IRequestChannel CreateClientChannel( ClientBindingContext context )
        {
            return new RequestChannel( this, context );
        }

        /// <summary>
        /// Creates the channel to comunicate with the server
        /// </summary>
        /// <param name="context">The context associated with this server.</param>
        /// <returns></returns>
        public IReplyChannel CreateServerChannel( ServerBindingContext context )
        {
            return new ReplyChannel( this, context );
        }
    }
}

