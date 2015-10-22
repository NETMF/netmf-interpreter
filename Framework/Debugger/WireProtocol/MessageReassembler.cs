//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Microsoft .NET Micro Framework Porting Kit Code Samples and is unsupported. 
// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use these files except in compliance with the License.
// You may obtain a copy of the License at:
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing
// permissions and limitations under the License.
// 
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

namespace Microsoft.SPOT.Debugger.WireProtocol
{
    internal class MessageReassembler
    {
        internal enum ReceiveState
        {
            Idle             = 0,
            Initialize       = 1,
            WaitingForHeader = 2,
            ReadingHeader    = 3,
            CompleteHeader   = 4,
            ReadingPayload   = 5,
            CompletePayload  = 6,
        }

        Controller   m_parent;
        ReceiveState m_state;

        MessageRaw   m_raw;
        int          m_rawPos;
        MessageBase  m_base;

        internal MessageReassembler( Controller parent )
        {
            m_parent = parent;
            m_state  = ReceiveState.Initialize;
        }

        internal IncomingMessage GetCompleteMessage()
        {
            return new IncomingMessage( m_parent, m_raw, m_base );
        }

        /// <summary>
        /// Essential Rx method. Drives state machine by reading data and processing it. This works in
        /// conjunction with NotificationThreadWorker [Tx].
        /// </summary>
        internal void Process()
        {
            int count;
            int bytesRead;

            try
            {
                switch(m_state)
                {
                case ReceiveState.Initialize:
                    m_rawPos        = 0;

                    m_base          = new MessageBase();
                    m_base.m_header = new Packet();

                    m_raw           = new MessageRaw ();
                    m_raw .m_header = m_parent.CreateConverter().Serialize( m_base.m_header );

                    m_state = ReceiveState.WaitingForHeader;
                    DebuggerEventSource.Log.WireProtocolReceiveState( m_state );
                    goto case ReceiveState.WaitingForHeader;

                case ReceiveState.WaitingForHeader:
                    count = m_raw.m_header.Length - m_rawPos;

                    bytesRead = m_parent.Read(m_raw.m_header, m_rawPos, count);
                    m_rawPos += bytesRead;

                    while(m_rawPos > 0)
                    {
                        int flag_Debugger = ValidSignature( m_parent.marker_Debugger );
                        int flag_Packet   = ValidSignature( m_parent.marker_Packet   );
                                
                        if(flag_Debugger == 1 || flag_Packet == 1)
                        {
                            m_state = ReceiveState.ReadingHeader;
                            DebuggerEventSource.Log.WireProtocolReceiveState( m_state );
                            goto case ReceiveState.ReadingHeader;
                        }

                        if(flag_Debugger == 0 || flag_Packet == 0)
                        {
                            break; // Partial match.
                        }

                        m_parent.App.SpuriousCharacters( m_raw.m_header, 0, 1 );

                        Array.Copy( m_raw.m_header, 1, m_raw.m_header, 0, --m_rawPos );
                    }
                    break;

                case ReceiveState.ReadingHeader:
                    count = m_raw.m_header.Length - m_rawPos;

                    bytesRead = m_parent.Read(m_raw.m_header, m_rawPos, count);

                    m_rawPos += bytesRead;

                    if (bytesRead != count) break;

                    m_state = ReceiveState.CompleteHeader;
                    DebuggerEventSource.Log.WireProtocolReceiveState( m_state );
                    goto case ReceiveState.CompleteHeader;

                case ReceiveState.CompleteHeader:
                    try
                    {
                        m_parent.CreateConverter().Deserialize( m_base.m_header, m_raw.m_header );

                        if(VerifyHeader() == true)
                        {
                            bool fReply = (m_base.m_header.m_flags & Flags.c_Reply) != 0;

                            DebuggerEventSource.Log.WireProtocolRxHeader( m_base.m_header.m_crcHeader, m_base.m_header.m_crcData, m_base.m_header.m_cmd, m_base.m_header.m_flags, m_base.m_header.m_seq, m_base.m_header.m_seqReply, m_base.m_header.m_size );

                            if(m_base.m_header.m_size != 0)
                            {
                                m_raw.m_payload = new byte[m_base.m_header.m_size];
                                //reuse m_rawPos for position in header to read.
                                m_rawPos = 0;

                                m_state = ReceiveState.ReadingPayload;
                                DebuggerEventSource.Log.WireProtocolReceiveState( m_state );
                                goto case ReceiveState.ReadingPayload;
                            }
                            else
                            {
                                m_state = ReceiveState.CompletePayload;
                                DebuggerEventSource.Log.WireProtocolReceiveState( m_state );
                                goto case ReceiveState.CompletePayload;
                            }
                        }
                    }
                    catch(ThreadAbortException)
                    {
                        throw;
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine( "Fault at payload deserialization:\n\n{0}", e.ToString() );
                    }

                    m_state = ReceiveState.Initialize;
                    DebuggerEventSource.Log.WireProtocolReceiveState( m_state );

                    if((m_base.m_header.m_flags & Flags.c_NonCritical) == 0)
                    {
                        IncomingMessage.ReplyBadPacket( m_parent, Flags.c_BadHeader );
                    }

                    break;

                case ReceiveState.ReadingPayload:
                    count = m_raw.m_payload.Length - m_rawPos;

                    bytesRead = m_parent.Read(m_raw.m_payload, m_rawPos, count);

                    m_rawPos += bytesRead;

                    if (bytesRead != count) break;

                    m_state = ReceiveState.CompletePayload;
                    DebuggerEventSource.Log.WireProtocolReceiveState( m_state );
                    goto case ReceiveState.CompletePayload;

                case ReceiveState.CompletePayload:
                    if(VerifyPayload() == true)
                    {
                        try
                        {
                            bool fReply = (m_base.m_header.m_flags & Flags.c_Reply) != 0;

                            if((m_base.m_header.m_flags & Flags.c_NACK) != 0)
                            {
                                m_raw.m_payload = null;
                            }

                            m_parent.App.ProcessMessage( this.GetCompleteMessage(), fReply );

                            m_state = ReceiveState.Initialize;
                            DebuggerEventSource.Log.WireProtocolReceiveState( m_state );
                            return;
                        }
                        catch(ThreadAbortException)
                        {
                            throw;
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine( "Fault at payload deserialization:\n\n{0}", e.ToString() );
                        }
                    }

                    m_state = ReceiveState.Initialize;
                    DebuggerEventSource.Log.WireProtocolReceiveState( m_state );
                    if((m_base.m_header.m_flags & Flags.c_NonCritical) == 0)
                    {
                        IncomingMessage.ReplyBadPacket( m_parent, Flags.c_BadPayload );
                    }

                    break;             
                }
            }
            catch
            {
                m_state = ReceiveState.Initialize;
                DebuggerEventSource.Log.WireProtocolReceiveState( m_state );
                throw;
            }
        }
        
        private int ValidSignature( byte[] sig )
        {
            System.Diagnostics.Debug.Assert(sig != null && sig.Length == Packet.SIZE_OF_SIGNATURE);
            int markerSize = Packet.SIZE_OF_SIGNATURE;
            int iMax = System.Math.Min(m_rawPos, markerSize);

            for (int i = 0; i < iMax; i++)
            {
                if (m_raw.m_header[i] != sig[i]) return -1;
            }

            if(m_rawPos < markerSize) return 0;

            return 1;
        }

        private bool VerifyHeader()
        {
            uint crc = m_base.m_header.m_crcHeader;
            bool fRes;

            m_base.m_header.m_crcHeader = 0;

            fRes = CRC.ComputeCRC( m_parent.CreateConverter().Serialize( m_base.m_header ), 0 ) == crc;

            m_base.m_header.m_crcHeader = crc;

            return fRes;
        }

        private bool VerifyPayload()
        {
            if(m_raw.m_payload == null)
            {
                return (m_base.m_header.m_size == 0);
            }
            else
            {
                if(m_base.m_header.m_size != m_raw.m_payload.Length) return false;

                return CRC.ComputeCRC( m_raw.m_payload, 0 ) == m_base.m_header.m_crcData;
            }
        }
    }
}