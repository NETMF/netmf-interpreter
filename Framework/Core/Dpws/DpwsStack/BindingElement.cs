using System;
using System.Net;
using System.IO;
using System.Collections;
using Ws.Services.Soap;

namespace Ws.Services.Binding
{
    /// <summary>
    /// Defines the methods to handle a message in a chain of resposibility from one BindingElement to the next
    /// </summary>
    public abstract class BindingElement
    {
        /// <summary>
        /// the reference to teh next BindingElement
        /// </summary>
        protected BindingElement m_Next;

        /// <summary>
        /// The return result of any methods that handles a message context through a BindingElement
        /// </summary>
        protected enum ChainResult
        {
            Handled,
            Continue,
            Abort
        }
        
        /// <summary>
        ///  Chains a BindingElement to another one
        /// </summary>
        /// <param name="Next">The BindingElement to cascade to this one.</param>
        public void SetNext(BindingElement Next)
        {
            m_Next = Next;
        }
        
        /// <summary>
        /// Opens a comunication stream for a specific BindingContext
        /// </summary>
        /// <param name="stream">The stream to open.</param>
        /// <param name="ctx">The context to open the stream for.</param>
        public void Open( ref Stream stream, BindingContext ctx )
        {
            if (this.m_Next != null)
            {
                this.m_Next.Open(ref stream, ctx);
            }

            this.OnOpen(ref stream, ctx);
        }

        /// <summary>
        /// Closes a comunication stream for a specific BindingContext
        /// </summary>
        /// <param name="stream">The stream to close.</param>
        /// <param name="ctx">The context to close the stream for.</param>
        public void Close( Stream stream, BindingContext ctx )
        {
            if(this.OnClose( stream, ctx ) == ChainResult.Continue && m_Next != null)
            {
                m_Next.Close( stream, ctx );
            }
        }

        /// <summary>
        /// Processes an output message for a specific context
        /// </summary>
        /// <param name="message">The message to process.</param>
        /// <param name="ctx">The context to process the message for.</param>
        /// <returns><c>true</c> if the processing was succesfull, <c>false</c>otherwise.</returns>
        public bool ProcessOutputMessage(ref WsMessage message, BindingContext ctx)
        {
            ChainResult res = this.OnProcessOutputMessage( ref message, ctx );
            if( ChainResult.Continue == res && m_Next != null)
            {
                return m_Next.ProcessOutputMessage( ref message, ctx );
            }

            return res == ChainResult.Handled;
        }

        /// <summary>
        /// Processes an input message for a specific context
        /// </summary>
        /// <param name="message">The message to process.</param>
        /// <param name="ctx">The context to process the message for.</param>
        /// <returns><c>true</c> if the processing was succesfull, <c>false</c>otherwise.</returns>
        public bool ProcessInputMessage(ref WsMessage message, BindingContext ctx)
        {
            if (m_Next != null)
            {
                if(!m_Next.ProcessInputMessage( ref message, ctx ))
                {
                    return false;
                }
            }

            return this.OnProcessInputMessage(ref message, ctx) != ChainResult.Abort;
        }

        /// <summary>
        /// Sets a configuration for a specific binding
        /// </summary>
        /// <param name="cfg">The opaque configuration object. The binding element will know how to handle the configuration.</param>
        public void SetBindingConfiguration( object cfg )
        {
            this.OnSetBindingConfiguration( cfg );

            if(this.m_Next != null)
            {
                this.m_Next.SetBindingConfiguration( cfg );
            }
        }
        
        /// <summary>
        /// Extracts a property from a BindingElement
        /// </summary>
        /// <param name="propName">The name of teh property to extract.</param>
        /// <returns>The property object.</returns>
        public object GetBindingProperty(string propName)
        {
            object retVal = this.OnGetBindingProperty( propName );

            if(retVal != null) return retVal;

            if(this.m_Next != null)
            {
                return this.m_Next.GetBindingProperty( propName );
            }

            return null;
        }

        /// <summary>
        /// Signaled by the <ref>Open</ref> method to allow subclasses to create a sub-class of Binding Element with specific behaviour
        /// </summary>
        /// <param name="stream">The stream that is being opened.</param>
        /// <param name="ctx">The context for which the stream is being opened.</param>
        /// <returns>The handling status for this operation.</returns>
        protected virtual ChainResult OnOpen(ref Stream stream, BindingContext ctx) { return ChainResult.Continue; }
        /// <summary>
        /// Signaled by the <ref>Close</ref> method to allow subclasses to create a sub-class of Binding Element with specific behaviour
        /// </summary>
        /// <param name="stream">The stream that is being closed.</param>
        /// <param name="ctx">The context for which the stream is being closed.</param>
        /// <returns>The handling status for this operation.</returns>
        protected virtual ChainResult OnClose(Stream stream, BindingContext ctx) { return ChainResult.Continue; }
        /// <summary>
        /// Signaled by the <ref>SetBindingConfiguration</ref> method to allow subclasses to create a sub-class of Binding Element with specific behaviour
        /// </summary>
        /// <param name="parms">The configuration object.</param>
        protected virtual void OnSetBindingConfiguration(object parms) { }
        /// <summary>
        /// Signaled by the <ref>GetBindingProperty</ref> method to allow subclasses to create a sub-class of Binding Element with specific behaviour
        /// </summary>
        /// <param name="propName">The name of the property being extracted.</param>
        /// <returns>The property object.</returns>
        protected virtual object OnGetBindingProperty(string propName) { return null; }
        /// <summary>
        /// Signaled by the <ref>ProcessOutputMessage</ref> method to allow subclasses to create a sub-class of Binding Element with specific behaviour
        /// </summary>
        /// <param name="message">The message being processed.</param>
        /// <param name="ctx">The context associated with the message being processed.</param>
        /// <returns>The handling status for this operation.</returns>
        protected abstract ChainResult OnProcessOutputMessage(ref WsMessage message, BindingContext ctx);
        /// <summary>
        /// Signaled by the <ref>ProcessInputMessage</ref> method to allow subclasses to create a sub-class of Binding Element with specific behaviour
        /// </summary>
        /// <param name="message">The message being processed.</param>
        /// <param name="ctx">The context associated with the message being processed.</param>
        /// <returns>The handling status for this operation.</returns>
        protected abstract ChainResult OnProcessInputMessage(ref WsMessage message, BindingContext ctx);
                
    }

    /// <summary>
    /// Handles the processing chain for the message encoding
    /// </summary>
    public abstract class MessageEncodingBindingElement : BindingElement
    {
    }

    /// <summary>
    /// Handles the processing chain for the chosen transport
    /// </summary>
    public abstract class TransportBindingElement : BindingElement
    {
        protected Stream m_stream;
        protected Uri    m_serviceUrn;
        protected Uri    m_endpointUri;
        protected Uri    m_transportUri;

        public Uri ServiceUrn       { get { return m_serviceUrn;       } set { m_serviceUrn   = value; } }
        public Uri EndpointAddress  { get { return m_endpointUri;      } set { m_endpointUri  = value; } }
        public Uri TransportAddress { get { return m_transportUri;     } set { m_transportUri = value; } }
        public int EndpointPort     { get { return m_endpointUri.Port; } }
    }
}

