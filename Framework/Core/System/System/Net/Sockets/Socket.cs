////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("System.Net.Security")]

namespace System.Net.Sockets
{
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using Microsoft.SPOT;
    using Microsoft.SPOT.Net;
    using Microsoft.SPOT.Hardware;

    using NativeSocket = Microsoft.SPOT.Net.SocketNative;

    public class Socket : IDisposable
    {
        /* WARNING!!!!
         * The m_Handle field MUST be the first field in the Socket class; it is expected by
         * the SPOT.NET.SocketNative class.
         */
        [FieldNoReflection]
        internal int m_Handle = -1;

        private bool m_fBlocking = true;
        private EndPoint m_localEndPoint = null;

        // timeout values are stored in uSecs since the Poll method requires it.
        private int m_recvTimeout = System.Threading.Timeout.Infinite;
        private int m_sendTimeout = System.Threading.Timeout.Infinite;

        public Socket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            m_Handle = NativeSocket.socket((int)addressFamily, (int)socketType, (int)protocolType);
        }

        private Socket(int handle)
        {
            m_Handle = handle;
        }

        public int Available
        {
            get
            {
                if (m_Handle == -1)
                {
                    throw new ObjectDisposedException();
                }

                uint cBytes = 0;

                NativeSocket.ioctl(this, NativeSocket.FIONREAD, ref cBytes);

                return (int)cBytes;
            }
        }

        private EndPoint GetEndPoint(bool fLocal)
        {
            if (m_Handle == -1)
            {
                throw new ObjectDisposedException();
            }

            EndPoint ep = null;

            if (m_localEndPoint == null)
            {
                m_localEndPoint = new IPEndPoint(IPAddress.Any, 0);
            }

            byte[] address = null;

            if (fLocal)
            {
                NativeSocket.getsockname(this, out address);
            }
            else
            {
                NativeSocket.getpeername(this, out address);
            }

            SocketAddress socketAddress = new SocketAddress(address);
            ep = m_localEndPoint.Create(socketAddress);

            if (fLocal)
            {
                m_localEndPoint = ep;
            }

            return ep;
        }

        public EndPoint LocalEndPoint
        {
            get
            {
                return GetEndPoint(true);
            }
        }

        public EndPoint RemoteEndPoint
        {
            get
            {
                return GetEndPoint(false);
            }
        }

        public int ReceiveTimeout
        {
            get
            {
                return m_recvTimeout;
            }

            set
            {
                if (value < Timeout.Infinite) throw new ArgumentOutOfRangeException();

                // desktop implementation treats 0 as infinite
                m_recvTimeout = ((value == 0) ? Timeout.Infinite : value);
            }
        }

        public int SendTimeout
        {
            get
            {
                return m_sendTimeout;
            }

            set
            {
                if (value < Timeout.Infinite) throw new ArgumentOutOfRangeException();

                // desktop implementation treats 0 as infinite
                m_sendTimeout = ((value == 0) ? Timeout.Infinite : value);
            }
        }

        public void Bind(EndPoint localEP)
        {
            if (m_Handle == -1)
            {
                throw new ObjectDisposedException();
            }

            NativeSocket.bind(this, localEP.Serialize().m_Buffer);

            m_localEndPoint = localEP;
        }

        public void Connect(EndPoint remoteEP)
        {
            if (m_Handle == -1)
            {
                throw new ObjectDisposedException();
            }

            NativeSocket.connect(this, remoteEP.Serialize().m_Buffer, !m_fBlocking);

            if (m_fBlocking)
            {
                Poll(-1, SelectMode.SelectWrite);
            }
        }

        public void Close()
        {
            ((IDisposable)this).Dispose();
        }

        public void Listen(int backlog)
        {
            if (m_Handle == -1)
            {
                throw new ObjectDisposedException();
            }

            NativeSocket.listen(this, backlog);
        }

        public Socket Accept()
        {
            if (m_Handle == -1)
            {
                throw new ObjectDisposedException();
            }

            int socketHandle;

            if (m_fBlocking)
            {
                Poll(-1, SelectMode.SelectRead);
            }

            socketHandle = NativeSocket.accept(this);

            Socket socket = new Socket(socketHandle);

            socket.m_localEndPoint = this.m_localEndPoint;

            return socket;
        }

        public int Send(byte[] buffer, int size, SocketFlags socketFlags)
        {
            return Send(buffer, 0, size, socketFlags);
        }

        public int Send(byte[] buffer, SocketFlags socketFlags)
        {
            return Send(buffer, 0, buffer != null ? buffer.Length : 0, socketFlags);
        }

        public int Send(byte[] buffer)
        {
            return Send(buffer, 0, buffer != null ? buffer.Length : 0, SocketFlags.None);
        }

        public int Send(byte[] buffer, int offset, int size, SocketFlags socketFlags)
        {
            if (m_Handle == -1)
            {
                throw new ObjectDisposedException();
            }

            return NativeSocket.send(this, buffer, offset, size, (int)socketFlags, m_sendTimeout);
        }

        public int SendTo(byte[] buffer, int offset, int size, SocketFlags socketFlags, EndPoint remoteEP)
        {
            if (m_Handle == -1)
            {
                throw new ObjectDisposedException();
            }

            byte[] address = remoteEP.Serialize().m_Buffer;

            return NativeSocket.sendto(this, buffer, offset, size, (int)socketFlags, m_sendTimeout, address);
        }

        public int SendTo(byte[] buffer, int size, SocketFlags socketFlags, EndPoint remoteEP)
        {
            return SendTo(buffer, 0, size, socketFlags, remoteEP);
        }

        public int SendTo(byte[] buffer, SocketFlags socketFlags, EndPoint remoteEP)
        {
            return SendTo(buffer, 0, buffer != null ? buffer.Length : 0, socketFlags, remoteEP);
        }

        public int SendTo(byte[] buffer, EndPoint remoteEP)
        {
            return SendTo(buffer, 0, buffer != null ? buffer.Length : 0, SocketFlags.None, remoteEP);
        }

        public int Receive(byte[] buffer, int size, SocketFlags socketFlags)
        {
            return Receive(buffer, 0, size, socketFlags);
        }

        public int Receive(byte[] buffer, SocketFlags socketFlags)
        {
            return Receive(buffer, 0, buffer != null ? buffer.Length : 0, socketFlags);
        }

        public int Receive(byte[] buffer)
        {
            return Receive(buffer, 0, buffer != null ? buffer.Length : 0, SocketFlags.None);
        }

        public int Receive(byte[] buffer, int offset, int size, SocketFlags socketFlags)
        {
            if (m_Handle == -1)
            {
                throw new ObjectDisposedException();
            }

            return NativeSocket.recv(this, buffer, offset, size, (int)socketFlags, m_recvTimeout);
        }

        public int ReceiveFrom(byte[] buffer, int offset, int size, SocketFlags socketFlags, ref EndPoint remoteEP)
        {
            if (m_Handle == -1)
            {
                throw new ObjectDisposedException();
            }

            byte[] address = remoteEP.Serialize().m_Buffer;
            int len = 0;

            len = NativeSocket.recvfrom(this, buffer, offset, size, (int)socketFlags, m_recvTimeout, ref address);

            SocketAddress socketAddress = new SocketAddress(address);
            remoteEP = remoteEP.Create(socketAddress);

            return len;
        }

        public int ReceiveFrom(byte[] buffer, int size, SocketFlags socketFlags, ref EndPoint remoteEP)
        {
            return ReceiveFrom(buffer, 0, size, socketFlags, ref remoteEP);
        }

        public int ReceiveFrom(byte[] buffer, SocketFlags socketFlags, ref EndPoint remoteEP)
        {
            return ReceiveFrom(buffer, 0, buffer != null ? buffer.Length : 0, socketFlags, ref remoteEP);
        }

        public int ReceiveFrom(byte[] buffer, ref EndPoint remoteEP)
        {
            return ReceiveFrom(buffer, 0, buffer != null ? buffer.Length : 0, SocketFlags.None, ref remoteEP);
        }

        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int optionValue)
        {
            if (m_Handle == -1)
            {
                throw new ObjectDisposedException();
            }

            //BitConverter.GetBytes(int). Or else deal with endianness here?
            byte[] val;
            if(SystemInfo.IsBigEndian)
                val = new byte[4] { (byte)(optionValue >> 24), (byte)(optionValue >> 16), (byte)(optionValue >> 8), (byte)(optionValue >> 0) };
            else
                val = new byte[4] { (byte)(optionValue >> 0), (byte)(optionValue >> 8), (byte)(optionValue >> 16), (byte)(optionValue >> 24) };

            switch (optionName)
            {
                case SocketOptionName.SendTimeout:
                    m_sendTimeout = optionValue;
                    break;
                case SocketOptionName.ReceiveTimeout:
                    m_recvTimeout = optionValue;
                    break;
            }

            NativeSocket.setsockopt(this, (int)optionLevel, (int)optionName, val);
        }

        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, bool optionValue)
        {
            SetSocketOption(optionLevel, optionName, (optionValue ? 1 : 0));
        }

        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue)
        {
            if (m_Handle == -1)
            {
                throw new ObjectDisposedException();
            }

            NativeSocket.setsockopt(this, (int)optionLevel, (int)optionName, optionValue);
        }

        public object GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName)
        {
            if (optionName == SocketOptionName.DontLinger ||
                optionName == SocketOptionName.AddMembership ||
                optionName == SocketOptionName.DropMembership)
            {
                //special case linger?
                throw new NotSupportedException();
            }

            byte[] val = new byte[4];

            GetSocketOption(optionLevel, optionName, val);

            //Use BitConverter.ToInt32
            //endianness?
            int iVal;

            if(SystemInfo.IsBigEndian)
                iVal = (val[3] << 0 | val[2] << 8 | val[1] << 16 | val[0] << 24);
            else
                iVal = (val[0] << 0 | val[1] << 8 | val[2] << 16 | val[3] << 24);

            
            return (object)iVal;
        }

        public void GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] val)
        {
            if (m_Handle == -1)
            {
                throw new ObjectDisposedException();
            }

            NativeSocket.getsockopt(this, (int)optionLevel, (int)optionName, val);
        }

        public bool Poll(int microSeconds, SelectMode mode)
        {
            if (m_Handle == -1)
            {
                throw new ObjectDisposedException();
            }

            return NativeSocket.poll(this, (int)mode, microSeconds);
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        protected virtual void Dispose(bool disposing)
        {
            if (m_Handle != -1)
            {
                NativeSocket.close(this);
                m_Handle = -1;
            }
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Socket()
        {
            Dispose(false);
        }
    }
}


