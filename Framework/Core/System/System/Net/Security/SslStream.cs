////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Microsoft.SPOT.Net.Security;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.SPOT.Net.Security
{
    public class SslStream : NetworkStream
    {
        // Internal flags
        private int _sslContext;
        private bool _isServer;

        //--//

        public SslStream(Socket socket)
            : base(socket, false)
        {
            if (SocketType.Stream != (SocketType)_socketType)
            {
                throw new NotSupportedException();
            }

            _sslContext = -1;
            _isServer = false;
        }
        
        public void AuthenticateAsClient(string targetHost, params SslProtocols[] sslProtocols)
        {
            AuthenticateAsClient(targetHost, null, null, SslVerification.NoVerification, sslProtocols);
        }

        public void AuthenticateAsClient(string targetHost, X509Certificate cert, SslVerification verify, params SslProtocols[] sslProtocols)
        {
            AuthenticateAsClient(targetHost, cert, null, verify, sslProtocols);
        }

        public void AuthenticateAsClient(string targetHost, X509Certificate cert, X509Certificate[] ca, SslVerification verify, params SslProtocols[] sslProtocols)
        {
            Authenticate(false, targetHost, cert, ca, verify, sslProtocols);
        }

        public void AuthenticateAsServer(X509Certificate cert, SslVerification verify, params SslProtocols[] sslProtocols)
        {
            AuthenticateAsServer(cert, null, verify, sslProtocols);
        }

        public void AuthenticateAsServer(X509Certificate cert, X509Certificate[] ca, SslVerification verify, params SslProtocols[] sslProtocols)
        {
            Authenticate(true, "", cert, ca, verify, sslProtocols);
        }

        public void UpdateCertificates(X509Certificate cert, X509Certificate[] ca)
        {
            if(_sslContext == -1) throw new InvalidOperationException();
            
            SslNative.UpdateCertificates(_sslContext, cert, ca);
        }

        internal void Authenticate(bool isServer, string targetHost, X509Certificate certificate, X509Certificate[] ca, SslVerification verify, params SslProtocols[] sslProtocols)
        {
            SslProtocols vers = (SslProtocols)0;

            if (-1 != _sslContext) throw new InvalidOperationException();

            for (int i = sslProtocols.Length - 1; i >= 0; i--)
            {
                vers |= sslProtocols[i];
            }

            _isServer = isServer;

            try
            {
                if (isServer)
                {
                    _sslContext = SslNative.SecureServerInit((int)vers, (int)verify, certificate, ca);
                    SslNative.SecureAccept(_sslContext, _socket);
                }
                else
                {
                    _sslContext = SslNative.SecureClientInit((int)vers, (int)verify, certificate, ca);
                    SslNative.SecureConnect(_sslContext, targetHost, _socket);
                }
            }
            catch
            {
                if (_sslContext != -1)
                {
                    SslNative.ExitSecureContext(_sslContext);
                    _sslContext = -1;
                }

                throw;
            }
        }

        public bool IsServer { get { return _isServer; } }

        public override long Length
        {
            get
            {
                if (_disposed == true) throw new ObjectDisposedException();
                if (_socket == null) throw new IOException();

                return SslNative.DataAvailable(_socket);
            }
        }

        public override bool DataAvailable
        {
            get
            {
                if (_disposed == true) throw new ObjectDisposedException();
                if (_socket == null) throw new IOException();

                return (SslNative.DataAvailable(_socket) > 0);
            }
        }

        ~SslStream()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;

                if(_socket.m_Handle != -1)
                {
                    SslNative.SecureCloseSocket(_socket);
                    _socket.m_Handle = -1;
                }

                if (_sslContext != -1) 
                {
                    SslNative.ExitSecureContext(_sslContext);
                    _sslContext = -1;
                }
            }
        }

        public override int Read(byte[] buffer, int offset, int size)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException();
            }

            if (_disposed)
            {
                throw new ObjectDisposedException();
            }

            if (offset < 0 || offset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (size < 0 || size > buffer.Length - offset)
            {
                throw new ArgumentOutOfRangeException();
            }

            return SslNative.SecureRead(_socket, buffer, offset, size, _socket.ReceiveTimeout);
        }

        public override void Write(byte[] buffer, int offset, int size)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException();
            }

            if (_disposed)
            {
                throw new ObjectDisposedException();
            }

            if (offset < 0 || offset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (size < 0 || size > buffer.Length - offset)
            {
                throw new ArgumentOutOfRangeException();
            }

            SslNative.SecureWrite(_socket, buffer, offset, size, _socket.SendTimeout);
        }
    }
}


