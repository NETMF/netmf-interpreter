////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Net
{

    using System;
    using System.Runtime.InteropServices;
    using System.Net.Sockets;
    using System.Text;
    using System.Globalization;
    using Microsoft.SPOT.Hardware;

    public class SocketAddress
    {
        internal const int IPv4AddressSize = 16;

        internal byte[] m_Buffer;

        public AddressFamily Family
        {
            get
            {
                return (AddressFamily)(m_Buffer[0] | (m_Buffer[1] << 8));
            }
        }

        internal SocketAddress(byte[] address)
        {
            m_Buffer = address;
        }

        public SocketAddress(AddressFamily family, int size)
        {
            Microsoft.SPOT.Debug.Assert(size > 2);

            m_Buffer = new byte[size]; //(size / IntPtr.Size + 2) * IntPtr.Size];//sizeof DWORD

            m_Buffer[0] = unchecked((byte)((int)family     ));
            m_Buffer[1] = unchecked((byte)((int)family >> 8));
        }

        public int Size
        {
            get { return m_Buffer.Length; }
        }

        public byte this[int offset]
        {
            get { return m_Buffer[offset]; }
            set { m_Buffer[offset] = value; }
        }

    } // class SocketAddress
} // namespace System.Net


