using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.SPOT.Debugger
{
    public class AsyncNetworkStream : /*NetworkStream,*/Stream,  WireProtocol.IStreamAvailableCharacters //, IDisposable
    {
        Socket m_socket = null;
        NetworkStream m_ns = null;
        SslStream m_ssl = null;

        public AsyncNetworkStream(Socket socket, bool ownsSocket)
            //: base(socket, ownsSocket)
        {
            m_socket = socket;
            m_ns = new NetworkStream(socket);
        }

        public bool IsUsingSsl { get { return m_ssl != null; } }

        public override bool CanRead { get { return true; } }
        public override bool CanSeek { get { return false; } }
        public override bool CanWrite { get { return true; } }
        public override long Length { get { throw new NotSupportedException(); } }
        public override long Position { get { throw new NotSupportedException(); } set { throw new NotSupportedException(); } }
        public override void Flush() 
        {
            if (m_ssl != null)
            {
                m_ssl.Flush();
            }
            else if (m_ns != null)
            {
                m_ns.Flush();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (m_ssl != null)
            {
                return m_ssl.Read(buffer, offset, count);
            }
            else if (m_ns != null)
            {
                return m_ns.Read(buffer, offset, count);
            }

            throw new InvalidOperationException();
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (m_ssl != null)
            {
                m_ssl.Write(buffer, offset, count);
            }
            else if (m_ns != null)
            {
                m_ns.Write(buffer, offset, count);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_ssl != null) m_ssl.Dispose();
                if (m_ns != null) m_ns.Dispose();
                if (m_socket != null) m_socket.Close();
            }

            base.Dispose(disposing);
        }

        internal IAsyncResult BeginUpgradeToSSL(X509Certificate2 cert, bool requiresClientCert)
        {
            m_ssl = new SslStream(m_ns, true);

            return m_ssl.BeginAuthenticateAsServer(cert, requiresClientCert, System.Security.Authentication.SslProtocols.Tls, true, null, null);
        }

        internal bool EndUpgradeToSSL(IAsyncResult iar)
        {
            m_ssl.EndAuthenticateAsServer(iar);

            return iar.IsCompleted;
        }


        #region IStreamAvailableCharacters

        int WireProtocol.IStreamAvailableCharacters.AvailableCharacters
        {
            get
            {
                return m_socket.Available;
            }
        }
        #endregion 
    }
}