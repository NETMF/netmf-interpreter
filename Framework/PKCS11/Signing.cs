using System.Runtime.CompilerServices;
using System;
using System.Security.Cryptography;

namespace Microsoft.SPOT.Cryptoki
{
    /// <summary>
    /// Defines the Cryptoki signature object.
    /// </summary>
    public class CryptokiSign : SessionContainer
    {
        int m_signatureLength;
        private bool m_isInit;
        private Mechanism m_mech;
        private CryptoKey m_key;

        /// <summary>
        /// Creates a Cryptoki signature object with the specified session context, algorithm and key.
        /// </summary>
        /// <param name="session">The Cryptoki session context.</param>
        /// <param name="mechanism">The signature algorithm and parameters.</param>
        /// <param name="key">The key used to sign the input data.</param>
        public CryptokiSign(Session session, Mechanism mechanism, CryptoKey key) : 
            base(session, false)
        {
            m_signatureLength = (key.Size + 7) / 8;
            m_mech = mechanism;
            m_key  = key;
        }

        /// <summary>
        /// Creates a cryptographic signature of the specified data.
        /// </summary>
        /// <param name="data">Data to be signed.</param>
        /// <param name="index">Index into the data to begin signing.</param>
        /// <param name="length">The number of bytes to sign.</param>
        /// <returns></returns>
        public byte[] Sign(byte[] data, int index, int length)
        {
            if(!m_isInit) SignInit(m_session, m_mech, m_key);

            m_isInit = false;

            return SignInternal(data, index, length);
        }

        /// <summary>
        /// Performs a partial signature update.
        /// </summary>
        /// <param name="data">Data to be signed.</param>
        /// <param name="index">Index into the data to begin the signing.</param>
        /// <param name="length">Length in bytes of the partial singing.</param>
        public void SignUpdate(byte[] data, int index, int length)
        {
            if (!m_isInit) SignInit(m_session, m_mech, m_key);

            m_isInit = true;

            SignUpdateInternal(data, index, length);
        }

        /// <summary>
        /// Finalizes the partial signing process and returns the signature.
        /// </summary>
        /// <returns>The final signature value.</returns>
        public byte[] SignFinal()
        {
            if (!m_isInit) throw new InvalidOperationException();

            m_isInit = false;

            return SignFinalInternal();
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern void SignInit(Session session, Mechanism mechanism, CryptoKey key);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern byte[] SignInternal(byte[] data, int index, int length);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern void SignUpdateInternal(byte[] data, int index, int length);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern byte[] SignFinalInternal();
    }

    /// <summary>
    /// Defines the Cryptoki signature verification object.
    /// </summary>
    public class CryptokiVerify : SessionContainer
    {
        private bool m_isInit;
        private Mechanism m_mech;
        private CryptoKey m_key;

        /// <summary>
        /// Creates the signature verification object with specified the session context, signature algorithm and key.
        /// </summary>
        /// <param name="session">The Cryptoki session context.</param>
        /// <param name="mechanism">The signature algorithm.</param>
        /// <param name="key">The key used to verify the signature value.</param>
        public CryptokiVerify(Session session, Mechanism mechanism, CryptoKey key) :
            base(session, false)
        {
            m_mech = mechanism;
            m_key = key;
        }

        /// <summary>
        /// Performs signature verification on the given data and signature value.
        /// </summary>
        /// <param name="data">The data for which the signature is to be validated against.</param>
        /// <param name="index">The index into the data for which the verification is to begin.</param>
        /// <param name="length">The length of the data to be verified, in bytes.</param>
        /// <param name="signature">The signature value.</param>
        /// <param name="sigIndex">The start index for the signature value.</param>
        /// <param name="sigLenth">The length of the signature, in bytes.</param>
        /// <returns></returns>
        public bool Verify(byte[] data, int index, int length, byte[] signature, int sigIndex, int sigLenth)
        {
            if (!m_isInit) VerifyInit(m_session, m_mech, m_key);

            m_isInit = false;

            return VerifyInternal(data, index, length, signature, sigIndex, sigLenth);
        }

        /// <summary>
        /// Performs a partial signature verification.
        /// </summary>
        /// <param name="data">The data to be verified</param>
        /// <param name="index">Index to start the verification.</param>
        /// <param name="length">The length of the partial verification, in bytes.</param>
        public void VerifyUpdate(byte[] data, int index, int length)
        {
            if (!m_isInit) VerifyInit(m_session, m_mech, m_key);

            m_isInit = true;

            VerifyUpdateInternal(data, index, length);
        }

        /// <summary>
        /// Finalizes the partial signature verification.
        /// </summary>
        /// <param name="signature">The signature value.</param>
        /// <param name="index">The index of the start of the signature.</param>
        /// <param name="length">The length of the signature, in bytes.</param>
        /// <returns>true if the signature was verified, false otherwise.</returns>
        public bool VerifyFinal(byte[] signature, int index, int length)
        {
            if (!m_isInit) throw new InvalidOperationException();

            m_isInit = false;

            return VerifyFinalInternal(signature, index, length);
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern void VerifyInit(Session session, Mechanism mechanism, CryptoKey key);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern bool VerifyInternal(byte[] data, int index, int length, byte[] signature, int sigIndex, int sigLenth);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern void VerifyUpdateInternal(byte[] data, int index, int length);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern bool VerifyFinalInternal(byte[] signature, int index, int length);
    }
}