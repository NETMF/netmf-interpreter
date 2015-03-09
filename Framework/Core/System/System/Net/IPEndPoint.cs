////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Net
{
    using System.Net.Sockets;
    using System.Globalization;

    using Microsoft.SPOT;

    [Serializable]
    public class IPEndPoint : EndPoint
    {
        public const int MinPort = 0x00000000;
        public const int MaxPort = 0x0000FFFF;

        private IPAddress m_Address;
        private int m_Port;

        public IPEndPoint(long address, int port)
        {
            m_Port = port;
            m_Address = new IPAddress(address);
        }

        public IPEndPoint(IPAddress address, int port)
        {
            m_Port = port;
            m_Address = address;
        }

        public IPAddress Address
        {
            get
            {
                return m_Address;
            }
        }

        public int Port
        {
            get
            {
                return m_Port;
            }
        }

        public override SocketAddress Serialize()
        {
            // create a new SocketAddress
            //
            SocketAddress socketAddress = new SocketAddress(AddressFamily.InterNetwork, SocketAddress.IPv4AddressSize);
            byte[] buffer = socketAddress.m_Buffer;
            //
            // populate it
            //
            buffer[2] = unchecked((byte)(this.m_Port >> 8));
            buffer[3] = unchecked((byte)(this.m_Port));

            buffer[4] = unchecked((byte)(this.m_Address.m_Address));
            buffer[5] = unchecked((byte)(this.m_Address.m_Address >> 8));
            buffer[6] = unchecked((byte)(this.m_Address.m_Address >> 16));
            buffer[7] = unchecked((byte)(this.m_Address.m_Address >> 24));

            return socketAddress;
        }

        public override EndPoint Create(SocketAddress socketAddress)
        {
            // strip out of SocketAddress information on the EndPoint
            //

            byte[] buf = socketAddress.m_Buffer;

            Debug.Assert(socketAddress.Family == AddressFamily.InterNetwork);

            int port = (int)(
                    (buf[2] << 8 & 0xFF00) |
                    (buf[3])
                    );

            long address = (long)(
                    (buf[4] & 0x000000FF) |
                    (buf[5] << 8 & 0x0000FF00) |
                    (buf[6] << 16 & 0x00FF0000) |
                    (buf[7] << 24)
                    ) & 0x00000000FFFFFFFF;

            IPEndPoint created = new IPEndPoint(address, port);

            return created;
        }

        public override string ToString()
        {
            return m_Address.ToString() + ":" + m_Port.ToString();
        }

        public override bool Equals(object obj)
        {
            IPEndPoint ep = obj as IPEndPoint;
            if (ep == null)
            {
                return false;
            }

            return ep.m_Address.Equals(m_Address) && ep.m_Port == m_Port;
        }

    } // class IPEndPoint
} // namespace System.Net


