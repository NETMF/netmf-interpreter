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

namespace Microsoft.SPOT.Debugger.WireProtocol
{
    public class OutgoingMessage
    {
        IController  m_parent;

        MessageRaw   m_raw;
        MessageBase  m_base;

        public OutgoingMessage( IController parent, Converter converter, uint cmd, uint flags, object payload )
        {
            InitializeForSend( parent, converter, cmd, flags, payload );

            UpdateCRC( converter );
        }

        internal OutgoingMessage( IncomingMessage req, Converter converter, uint flags, object payload )
        {
            InitializeForSend( req.Parent, converter, req.Header.m_cmd, flags, payload );

            m_base.m_header.m_seqReply  = req.Header.m_seq;
            m_base.m_header.m_flags    |= Flags.c_Reply;

            UpdateCRC( converter );
        }

        public bool Send()
        {
            try
            {
                DebuggerEventSource.Log.WireProtocolTxHeader( m_base.m_header.m_crcHeader
                                                            , m_base.m_header.m_crcData
                                                            , m_base.m_header.m_cmd
                                                            , m_base.m_header.m_flags
                                                            , m_base.m_header.m_seq
                                                            , m_base.m_header.m_seqReply
                                                            , m_base.m_header.m_size
                                                            );
                return m_parent.QueueOutput( m_raw );
            }
            catch
            {
                return false;
            }
        }

        public Packet Header
        {
            get
            {
                return m_base.m_header;
            }
        }

        public object Payload
        {
            get
            {
                return m_base.m_payload;
            }
        }

        internal void InitializeForSend( IController parent, Converter converter, uint cmd, uint flags, object payload )
        {
            Packet header = parent.NewPacket();

            header.m_cmd   = cmd;
            header.m_flags = flags;

            m_parent              = parent;

            m_raw                 = new MessageRaw ();
            m_base                = new MessageBase();
            m_base.m_header       = header;
            m_base.m_payload      = payload;

            if(payload != null)
            {                
                m_raw.m_payload = converter.Serialize( payload );

                m_base.m_header.m_size    = (uint)m_raw.m_payload.Length;
                m_base.m_header.m_crcData = CRC.ComputeCRC( m_raw.m_payload, 0 );
            }
        }

        private void UpdateCRC( Converter converter )
        {
            Packet header = m_base.m_header;

            //
            // The CRC for the header is computed setting the CRC field to zero and then running the CRC algorithm.
            //
            header.m_crcHeader = 0;
            header.m_crcHeader = CRC.ComputeCRC( converter.Serialize( header ), 0 );

            m_raw.m_header = converter.Serialize( header );
        }
    }
}