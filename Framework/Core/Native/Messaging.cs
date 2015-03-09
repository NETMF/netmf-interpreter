////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.SPOT.Messaging
{
    public sealed class EndPoint
    {
        [Microsoft.SPOT.FieldNoReflection]
        private object m_handle;

        //--//

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public EndPoint(Type selector, uint id);
        //--//

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public bool Check(Type selector, uint id, int timeout);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public Message GetMessage(int timeout);
        //--//
        
        public object SendMessage(Type selector, uint id, int timeout, object payload)
        {
            byte[] res = SendMessageRaw(selector, id, timeout, Microsoft.SPOT.Reflection.Serialize(payload, null));

            if (res == null) return null;

            return Microsoft.SPOT.Reflection.Deserialize(res, null);
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public byte[] SendMessageRaw(Type selector, uint id, int timeout, byte[] payload);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern internal void ReplyRaw(Message msg, byte[] data);
        //--//
    }

    public sealed class Message
    {
        [Serializable()]
        public class RemotedException
        {
            public string m_message;

            public RemotedException(Exception payload)
            {
                m_message = payload.Message;
            }

            public void Raise()
            {

                throw new Exception(m_message);
            }
        }

        EndPoint m_source;
        Type m_selector;
        uint m_id;
        uint m_seq;
        byte[] m_payload;

        //--//

        public object Payload
        {
            get
            {
                return Microsoft.SPOT.Reflection.Deserialize(m_payload, null);
            }
        }

        public byte[] PayloadRaw
        {
            get
            {
                return m_payload;
            }
        }

        //--//

        public void Reply(object data)
        {
            m_source.ReplyRaw(this, Microsoft.SPOT.Reflection.Serialize(data, null));
        }

        public void ReplyRaw(byte[] data)
        {
            m_source.ReplyRaw(this, data);
        }
    }
}


