using System.Diagnostics;

namespace Microsoft.SPOT.Debugger
{
    using System;
    using System.Diagnostics.Tracing;
    using Microsoft.SPOT.Debugger.WireProtocol;

    [EventSource( Name="MsOpenTech-NETMF-Debugger")]
    internal class DebuggerEventSource : EventSource
    {
        public static DebuggerEventSource Log { get { return Log_.Value;  } }
        private static readonly Lazy<DebuggerEventSource> Log_ = new Lazy<DebuggerEventSource>( ()=>new DebuggerEventSource() );

        [Event(1, Opcode=EventOpcode.Send )]
        public void WireProtocolTxHeader( uint cmd, uint flags, ushort seq, ushort seqReply )
        {
            Trace.TraceInformation( "TX: {0:X08} {1:X08} {2:X04} {3:X04}", cmd, flags, seq, seqReply );
            WriteCustomEvent( 1, cmd, flags, seq, seqReply );
        }

        [Event( 2, Opcode = EventOpcode.Receive )]
        public void WireProtocolRxHeader( uint cmd, uint flags, ushort seq, ushort seqReply )
        {
            Trace.TraceInformation( "RX: {0:X08} {1:X08} {2:X04} {3:X04}", cmd, flags, seq, seqReply );
            WriteCustomEvent( 2, cmd, flags, seq, seqReply );
        }

        [Event( 3 )]
        public void WireProtocolReceiveState( MessageReassembler.ReceiveState state )
        {
            WriteEvent( 3, ( int )state );
        }

        private DebuggerEventSource()
        {
        }

        [NonEvent]
        unsafe void WriteCustomEvent(int eventId, uint cmd, uint flags, ushort seq, ushort seqReply )
        {
            EventData* pDataDesc = stackalloc EventData[ 4 ];
            pDataDesc[ 0 ].DataPointer = (IntPtr)( &cmd );
            pDataDesc[ 0 ].Size = sizeof( int );
            pDataDesc[ 1 ].DataPointer = ( IntPtr )( &flags );
            pDataDesc[ 1 ].Size = sizeof( int );
            pDataDesc[ 2 ].DataPointer = ( IntPtr )( &seq );
            pDataDesc[ 2 ].Size = sizeof( ushort );
            pDataDesc[ 3 ].DataPointer = ( IntPtr )( &seqReply );
            pDataDesc[ 3 ].Size = sizeof( ushort );
            WriteEventCore( eventId, 4, pDataDesc );
        }
    }
}
