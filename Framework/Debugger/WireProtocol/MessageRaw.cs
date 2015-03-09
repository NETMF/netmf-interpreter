using System;

namespace Microsoft.SPOT.Debugger.WireProtocol
{
    [Serializable]
    public class MessageRaw
    {
        public byte[ ] m_header;
        public byte[ ] m_payload;
    }
}