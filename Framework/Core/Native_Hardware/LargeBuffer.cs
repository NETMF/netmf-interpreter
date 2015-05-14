////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.CompilerServices;

namespace Microsoft.SPOT.Hardware
{
    /// <summary>
    /// The LargeBufferMarshaller class enables marshalling of LargeBuffer objects to and from 
    /// Native code.
    /// 
    /// This class corresponds to the Native methods LargeBuffer_NativeToManaged, 
    /// LargeBuffer_ManagedToNative, and LargeBuffer_GetNativeBufferSize.
    /// </summary>
    public class LargeBufferMarshaller : IEventListener
    {
        private ushort m_id;
        private static bool m_initialized = false;
        
        /// <summary>
        /// Delegate method for the OnLargeBufferRequest event
        /// </summary>
        /// <param name="marshalId">The ID of the LargeBufferMarshaller object for which the event occurred</param>
        public delegate void LargeBufferEventHandler( ushort  marshalId );

        /// <summary>
        /// Raises events when the Native code indicates a LargeBuffer object is ready for marshalling
        /// </summary>
        public static event LargeBufferEventHandler OnLargeBufferRequest;
        
        /// <summary>
        /// Constructor for LargeBufferMarshaller objects
        /// </summary>
        /// <param name="marshalId">The marshal ID that will be passed to the Native code methods for sending and retreiving LargeBuffer objects</param>
        public LargeBufferMarshaller(ushort marshalId)
        {
            m_id = marshalId;

            if(!m_initialized)
            {
                Microsoft.SPOT.EventSink.AddEventListener(EventCategory.LargeBuffer, this);
            
                m_initialized = true;
            }
        }

        /// <summary>
        /// IEventListener method for initializing the event source
        /// </summary>
        public void InitializeForEventSource()
        {
        }

        /// <summary>
        /// IEventListener method for handling GenericEvents
        /// </summary>
        /// <param name="ev"></param>
        /// <returns></returns>
        public bool OnEvent(BaseEvent ev)
        {
            if(ev is GenericEvent)
            {
                if(OnLargeBufferRequest != null)
                {
                    ushort id = (ushort)((GenericEvent)ev).EventData;
                    
                    OnLargeBufferRequest(id);
                }
            }

            return true;
        }
        
        /// <summary>
        /// Marshals a LargeBuffer object from managed code to native code.
        /// </summary>
        /// <param name="buffer">The LargeBuffer object to be marshalled</param>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern void MarshalBuffer(LargeBuffer buffer);
        
        /// <summary>
        /// UnMarshals a LargeBuffer object from native to managed code.
        ///
        /// Note: The 'buffer' parameter's internal size maybe changed during the call.
        /// </summary>
        /// <param name="buffer">The LargeBuffer object to be unmarshalled</param>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern void UnMarshalBuffer(ref LargeBuffer buffer);
    }
    
    /// <summary>
    /// The LargeBuffer class allows you to create a byte array that is larger than the 
    /// allowable size by the managed heap.  The LargeBuffer item is allocated in a secondary
    /// heap (that must be supported by the device).  This class can be used in conjunction 
    /// with the LargeBufferMarshaller to transfer large data items to and from native code.
    /// </summary>
    public class LargeBuffer : IDisposable
    {
        private byte[]      m_bytes;
        private bool        m_fDisposed;

        /// <summary>
        /// LargeBuffer construtor - creates a byte array buffer of the specified size.
        /// </summary>
        /// <param name="size">Size in bytes of the buffer to be created</param>
        public LargeBuffer(int size)
        {
            m_bytes              = null;
            m_fDisposed          = false;

            InternalCreateBuffer(size);
        }

        ~LargeBuffer()
        {
            Dispose( false );
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        private void Dispose( bool fDisposing )
        {
            if (!m_fDisposed)
            {
                try
                {
                    if(m_bytes != null)
                    {
                        InternalDestroyBuffer();

                        m_bytes = null;
                    }
                }
                finally
                {
                    m_fDisposed = true;
                }
            }
        }

        /// <summary>
        /// Disposes the LargeBuffer object and frees the internal buffer associated with it.
        /// </summary>
        public void Dispose()   
        {
            Dispose( true );

            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Gets the byte[] value associated with this LargeBuffer object
        /// </summary>
        public byte[] Bytes{ get { return m_bytes; } }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern void InternalCreateBuffer(int size);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern void InternalDestroyBuffer();

    }
}
