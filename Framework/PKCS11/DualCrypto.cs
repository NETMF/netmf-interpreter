using System.Runtime.CompilerServices;
using System;
using System.Security.Cryptography;

namespace Microsoft.SPOT.Cryptoki
{
    public class CryptokiDigestEncrypt : SessionContainer
    {
        Encryptor m_encryptor;
        CryptokiDigest m_digest;

        public CryptokiDigestEncrypt(Session session, Mechanism digestMechanism, Mechanism cryptoMechanism, CryptoKeyHandle cryptoKey) :
            base(session, true)
        {
            //m_encryptor = new Encryptor(m_session, cryptoMechanism, cryptoKey);
            //m_digest = new CryptokiDigest(m_session, digestMechanism);
        }

        /*
        public CryptokiDigestEncrypt(string providerName, Mechanism digestMechanism, Mechanism cryptoMechanism, CryptoKeyHandle cryptoKey) :
            base(new Session(providerName, digestMechanism, cryptoMechanism), true)
        {
            m_encryptor = new Encryptor(m_session, cryptoMechanism, cryptoKey);
            m_digest = new CryptokiDigest(m_session, digestMechanism);
        }
        */

        public Encryptor EncryptorMechanism
        {
            get { return m_encryptor; }
        }

        public CryptokiDigest DigestMechanism
        {
            get { return m_digest; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_encryptor.Dispose();
                m_digest.Dispose();
            }
            base.Dispose(disposing);
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern byte[] Update(byte[] part, int partIndex, int partLen);
    }

    public class CryptokiDecryptDigest : SessionContainer
    {
        Decryptor m_decryptor;
        CryptokiDigest m_digest;

        public CryptokiDecryptDigest(Session session, Mechanism digestMechanism, Mechanism cryptoMechanism, CryptoKeyHandle cryptoKey) :
            base(session, true)
        {
            //m_decryptor = new Decryptor(m_session, cryptoMechanism, cryptoKey);
            //m_digest = new CryptokiDigest(m_session, digestMechanism);
        }

        /*
        public CryptokiDecryptDigest(string providerName, Mechanism digestMechanism, Mechanism cryptoMechanism, CryptoKeyHandle cryptoKey) :
            base(new Session(providerName, digestMechanism, cryptoMechanism), true)
        {
            m_decryptor = new Decryptor(m_session, cryptoMechanism, cryptoKey);
            m_digest = new CryptokiDigest(m_session, digestMechanism);
        }
        */

        public Decryptor DecryptorMechanism
        {
            get { return m_decryptor; }
        }

        public CryptokiDigest DigestMechanism
        {
            get { return m_digest; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_decryptor.Dispose();
                m_digest.Dispose();
            }
            base.Dispose(disposing);
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern byte[] Update(byte[] part, int partIndex, int partLen);
    }

    public class CryptokiSignEncrypt : SessionContainer
    {
        Encryptor m_encryptor;
        CryptokiSign m_sign;

        public CryptokiSignEncrypt(Session session, Mechanism signMechanism, CryptoKeyHandle signKey, Mechanism cryptoMechanism, CryptoKeyHandle cryptoKey) :
            base(session, true)
        {
            //m_encryptor = new Encryptor(m_session, cryptoMechanism, cryptoKey);
            //m_sign = new CryptokiSign(m_session, signMechanism, signKey);
        }

        /*
        public CryptokiSignEncrypt(string providerName, Mechanism signMechanism, CryptoKeyHandle signKey, Mechanism cryptoMechanism, CryptoKeyHandle cryptoKey) :
            base(new Session(providerName, signMechanism, cryptoMechanism), true)
        {
            m_encryptor = new Encryptor(m_session, cryptoMechanism, cryptoKey);
            m_sign      = new CryptokiSign(m_session, signMechanism, signKey);
        }
        */

        public Encryptor EncryptorMechanism
        {
            get { return m_encryptor; }
        }

        public CryptokiSign SignMecahnism
        {
            get { return m_sign; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_encryptor.Dispose();
                m_sign.Dispose();
            }
            base.Dispose(disposing);
        }


        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern byte[] Update(byte[] part, int partIndex, int partLen);
    }

    public class CryptokiDecryptVerify : SessionContainer
    {
        Decryptor m_decryptor;
        CryptokiVerify m_verify;

        public CryptokiDecryptVerify(Session session, Mechanism cryptoMechanism, CryptoKeyHandle cryptoKey, Mechanism signMechanism, CryptoKeyHandle signKey) :
            base(session, true)
        {
            //m_decryptor = new Decryptor(m_session, cryptoMechanism, cryptoKey);
            //m_verify = new CryptokiVerify(m_session, signMechanism, signKey);
        }

        /*
        public CryptokiDecryptVerify(string providerName, Mechanism cryptoMechanism, CryptoKeyHandle cryptoKey, Mechanism signMechanism, CryptoKeyHandle signKey) :
            base(new Session(providerName, signMechanism, cryptoMechanism), true)
        {
            m_decryptor = new Decryptor     (m_session, cryptoMechanism, cryptoKey);
            m_verify    = new CryptokiVerify(m_session, signMechanism, signKey);
        }
        */

        public Decryptor DecryptorMechanism
        {
            get { return m_decryptor; }
        }

        public CryptokiVerify VerifyMecahnism
        {
            get { return m_verify; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_decryptor.Dispose();
                m_verify.Dispose();
            }
            base.Dispose(disposing);
        }

        // throws signature failed exception?
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern byte[] Update(byte[] part, int partIndex, int partLen);
    }
}