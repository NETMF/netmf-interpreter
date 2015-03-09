namespace System.Security.Cryptography.X509Certificates 
{
    using System;
    using System.Collections;
    using System.Text;
    using System.Security;
    using System.Globalization;
    using System.Runtime.InteropServices;    
    using System.Security.Cryptography;
    using System.Runtime.CompilerServices;

    public class X509Certificate
    {
        private byte[] m_certificate;
        private string m_password;

        protected string m_issuer;
        protected string m_subject;
        protected DateTime m_effectiveDate;
        protected DateTime m_expirationDate;
        protected byte[] m_handle;
        protected byte[] m_sessionHandle;

        public X509Certificate()
        {
        }

        public X509Certificate(byte[] certificate)
            : this(certificate, "")
        {
        }

        public X509Certificate(byte[] certificate, string password)
        {
            m_certificate = certificate;
            m_password    = password;

            ParseCertificate(certificate, password, ref m_issuer, ref m_subject, ref m_effectiveDate, ref m_expirationDate);
        }

        public virtual string Issuer
        {
            get { return m_issuer; }
        }

        public virtual string Subject
        {
            get { return m_subject; }
        }

        public virtual DateTime GetEffectiveDate()
        {
            return m_effectiveDate;
        }

        public virtual DateTime GetExpirationDate()
        {
            return m_expirationDate;
        }

        public virtual byte[] GetRawCertData()
        {
            return m_certificate;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ParseCertificate(byte[] cert, string password, ref string issuer, ref string subject, ref DateTime effectiveDate, ref DateTime expirationDate);
    }
}

