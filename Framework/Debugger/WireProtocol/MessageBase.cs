using System;

namespace Microsoft.SPOT.Debugger.WireProtocol
{
    public class MessageBase
    {
        public Packet m_header;
        public object m_payload;

        //[System.Diagnostics.Conditional("TRACE_DBG_HEADERS")]
        //public void DumpHeader( bool tx )
        //{
        
        //Console.WriteLine( "{0}: {1:X08} {2:X08} {3} {4}", txt, m_header.m_cmd, m_header.m_flags, m_header.m_seq, m_header.m_seqReply );          
        //}
    }
}