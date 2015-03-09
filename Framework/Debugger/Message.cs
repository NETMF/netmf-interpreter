using Microsoft.SPOT.Debugger.WireProtocol;

namespace Microsoft.SPOT.Debugger
{
    internal class Message
    {
        internal readonly EndPoint m_source;
        internal readonly Commands.Debugging_Messaging_Address m_addr;
        internal readonly byte[] m_payload;

        internal Message(EndPoint source, Commands.Debugging_Messaging_Address addr, byte[] payload)
        {
            m_source = source;
            m_addr = addr;
            m_payload = payload;
        }

        public object Payload
        {
            get
            {
                return m_source.m_eng.CreateBinaryFormatter().Deserialize(m_payload);
            }
        }

        public void Reply(object data)
        {
            byte[] payload = m_source.m_eng.CreateBinaryFormatter().Serialize(data);
            m_source.ReplyInner(this, payload);
        }
    }
}