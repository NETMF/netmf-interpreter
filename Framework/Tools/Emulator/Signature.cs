////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.SPOT.Emulator.PKCS11
{
    internal class SignatureDriver : HalDriver<ISignatureDriver>, ISignatureDriver
    {
        private bool m_signHash;

        private bool Init(int session, AlgorithmType alg, AlgorithmType hash, int hKey, bool isVerify)
        {
            //bool bRet = false;
            try
            {
                SessionData ctx = ((SessionDriver)this.Hal.Session).GetSessionCtx(session);

                KeyData kd = null;
                CryptokiObject obj = ctx.ObjectCtx.GetObject(hKey);

                if (obj == null) return false;

                if (obj.Type == CryptokiObjectType.Key)
                {
                    kd = obj.Data as KeyData;
                }
                else if (obj.Type == CryptokiObjectType.Cert)
                {
                    X509Certificate2 cert = obj.Data as X509Certificate2;

                    kd = new KeyData(null, cert);
                }
                else
                {
                    return false;
                }

                byte[] keyData = kd.KeyBytes;

                // remove
                if (((uint)((uint)hash & (uint)CryptokiObjectMgrDriver.CryptokiAttribType.SIGN_NO_NODIGEST_FLAG)) != 0)
                {
                    m_signHash = true;
                    hash = (AlgorithmType)((uint)hash & ~(uint)CryptokiObjectMgrDriver.CryptokiAttribType.SIGN_NO_NODIGEST_FLAG);
                }
                else
                {
                    m_signHash = false;
                }

                switch (alg)
                {
                    case AlgorithmType.RSA_PKCS:
                        {
                            RSACryptoServiceProvider csp;
                            SignatureData sigData =  isVerify ? ctx.VerifyCtx : ctx.SignCtx;

                            if (keyData != null)
                            {
                                csp = new RSACryptoServiceProvider();
                                csp.ImportCspBlob(keyData);
                            }
                            else
                            {
                                X509Certificate2 cert = kd.KeyCsp as X509Certificate2;

                                if (isVerify)
                                {
                                    csp = cert.PublicKey.Key as RSACryptoServiceProvider;
                                }
                                else
                                {
                                    csp = cert.PrivateKey as RSACryptoServiceProvider;
                                }
                            }

                            if (isVerify) sigData.SignObject = new RSAPKCS1SignatureDeformatter(csp);
                            else          sigData.SignObject = new RSAPKCS1SignatureFormatter(csp);

                            switch (hash)
                            {
                                case AlgorithmType.SHA_1:
                                    sigData.SignHashAlg = new SHA1CryptoServiceProvider();
                                    if (isVerify) ((AsymmetricSignatureDeformatter)sigData.SignObject).SetHashAlgorithm("SHA1");
                                    else ((AsymmetricSignatureFormatter)sigData.SignObject).SetHashAlgorithm("SHA1");
                                    break;

                                case AlgorithmType.SHA256:
                                    sigData.SignHashAlg = new SHA256CryptoServiceProvider();
                                    if (isVerify) ((AsymmetricSignatureDeformatter)sigData.SignObject).SetHashAlgorithm("SHA256");
                                    else ((AsymmetricSignatureFormatter)sigData.SignObject).SetHashAlgorithm("SHA256");
                                    break;

                                case AlgorithmType.SHA512:
                                    sigData.SignHashAlg = new SHA512CryptoServiceProvider();
                                    if (isVerify) ((AsymmetricSignatureDeformatter)sigData.SignObject).SetHashAlgorithm("SHA512");
                                    else ((AsymmetricSignatureFormatter)sigData.SignObject).SetHashAlgorithm("SHA512");
                                    break;

                                case AlgorithmType.MD5:
                                    sigData.SignHashAlg = new MD5CryptoServiceProvider();
                                    if (isVerify) ((AsymmetricSignatureDeformatter)sigData.SignObject).SetHashAlgorithm("MD5");
                                    else ((AsymmetricSignatureFormatter)sigData.SignObject).SetHashAlgorithm("MD5");
                                    break;

                                // no hash, means that we are signing a hash value
                                case (AlgorithmType)(uint.MaxValue):
                                    break;

                                default:
                                    return false;
                            }
                        }
                        break;

                    case AlgorithmType.ECDSA:
                        {
                            ECDsaCng csp = kd.KeyCsp as ECDsaCng;

                            if (csp == null) return false;

                            SignatureData sigData = isVerify ? ctx.VerifyCtx : ctx.SignCtx;

                            sigData.SignObject = csp;

                            switch(hash)
                            {
                                case AlgorithmType.SHA_1:
                                    csp.HashAlgorithm = CngAlgorithm.Sha1;
                                    sigData.SignHashAlg = new SHA1CryptoServiceProvider();
                                    break;

                                case AlgorithmType.SHA256:
                                    csp.HashAlgorithm = CngAlgorithm.Sha256;
                                    sigData.SignHashAlg = new SHA256CryptoServiceProvider();
                                    break;

                                case AlgorithmType.SHA384:
                                    csp.HashAlgorithm = CngAlgorithm.Sha384;
                                    sigData.SignHashAlg = new SHA384CryptoServiceProvider();
                                    break;

                                case AlgorithmType.SHA512:
                                    csp.HashAlgorithm = CngAlgorithm.Sha512;
                                    sigData.SignHashAlg = new SHA512CryptoServiceProvider();
                                    break;

                                case AlgorithmType.MD5:
                                    csp.HashAlgorithm = CngAlgorithm.MD5;
                                    sigData.SignHashAlg = new MD5CryptoServiceProvider();
                                    break;

                                default:
                                    return false;
                            }
                        }
                        break;

                    case AlgorithmType.DSA:
                        {
                            DSACryptoServiceProvider csp = new DSACryptoServiceProvider();

                            csp.ImportCspBlob(keyData);

                            SignatureData sigData = isVerify ? ctx.VerifyCtx : ctx.SignCtx;

                            if (isVerify) sigData.SignObject = new DSASignatureDeformatter(csp);
                            else          sigData.SignObject = new DSASignatureFormatter(csp);

                            switch(hash)
                            {
                                case AlgorithmType.SHA_1:
                                    sigData.SignHashAlg = new SHA1CryptoServiceProvider();
                                    if (isVerify) ((AsymmetricSignatureDeformatter)sigData.SignObject).SetHashAlgorithm("SHA1");
                                    else ((AsymmetricSignatureFormatter)sigData.SignObject).SetHashAlgorithm("SHA1");
                                    break;

                                case AlgorithmType.SHA256:
                                    sigData.SignHashAlg = new SHA256CryptoServiceProvider();
                                    if (isVerify) ((AsymmetricSignatureDeformatter)sigData.SignObject).SetHashAlgorithm("SHA256");
                                    else ((AsymmetricSignatureFormatter)sigData.SignObject).SetHashAlgorithm("SHA256");
                                    break;

                                case AlgorithmType.SHA512:
                                    sigData.SignHashAlg = new SHA512CryptoServiceProvider();
                                    if (isVerify) ((AsymmetricSignatureDeformatter)sigData.SignObject).SetHashAlgorithm("SHA512");
                                    else ((AsymmetricSignatureFormatter)sigData.SignObject).SetHashAlgorithm("SHA512");
                                    break;

                                case AlgorithmType.MD5:
                                    sigData.SignHashAlg = new MD5CryptoServiceProvider();
                                    if (isVerify) ((AsymmetricSignatureDeformatter)sigData.SignObject).SetHashAlgorithm("MD5");
                                    else ((AsymmetricSignatureFormatter)sigData.SignObject).SetHashAlgorithm("MD5");
                                    break;

                                default:
                                    return false;
                            }
                        }
                        break;

                    default:
                        return false;
                }
            }
            catch (Exception e)
            {
                Debug.Print("Exception: " + e.Message);
                return false;
            }

            return true;
        }

        #region ISignatureDriver Members

        bool ISignatureDriver.SignInit(int session, int alg, int hashAlg, int hKey)
        {
            return Init(session, (AlgorithmType)alg, (AlgorithmType)hashAlg, hKey, false);
        }

        bool ISignatureDriver.Sign(int session, IntPtr Data, int DataLen, IntPtr Signature, ref int SignatureLen)
        {
            SessionData ctx = null;
            try
            {
                ctx = ((SessionDriver)this.Hal.Session).GetSessionCtx(session);
                unsafe
                {
                    byte[] data = new byte[DataLen];
                    byte[] sig = null;

                    Marshal.Copy(Data, data, 0, DataLen);

                    HashAlgorithm hash = ctx.SignCtx.SignHashAlg;

                    if (hash == null || m_signHash)
                    {
                        object signer = ctx.SignCtx.SignObject;

                        if (signer is AsymmetricSignatureFormatter)
                        {
                            sig = ((AsymmetricSignatureFormatter)signer).CreateSignature(data);
                        }
                        else if (signer is ECDsaCng)
                        {
                            sig = ((ECDsaCng)signer).SignHash(data);
                        }
                    }
                    else
                    {
                        hash.ComputeHash(data);

                        object signer = ctx.SignCtx.SignObject;

                        if (signer is AsymmetricSignatureFormatter)
                        {
                            sig = ((AsymmetricSignatureFormatter)signer).CreateSignature(hash);
                        }
                        else if (signer is ECDsaCng)
                        {
                            sig = ((ECDsaCng)signer).SignHash(hash.Hash);
                        }
                    }

                    if (Signature == IntPtr.Zero)
                    {
                        SignatureLen = sig.Length;
                        return true;
                    }

                    if (sig == null || sig.Length > SignatureLen) throw new ArgumentException();

                    Marshal.Copy(sig, 0, Signature, sig.Length);

                    SignatureLen = sig.Length;

                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.Print("Exception: " + e.ToString());
                return false;
            }
            finally
            {
                if (Signature != IntPtr.Zero && ctx != null)
                {
                    ctx.SignCtx.Clear();
                }
            }
        }

        bool ISignatureDriver.SignFinal(int session, IntPtr Signature, ref int SignatureLen)
        {
            SessionData ctx = null;
            try
            {
                ctx = ((SessionDriver)this.Hal.Session).GetSessionCtx(session);
                unsafe
                {
                    byte[] sig = null;

                    object signer = ctx.SignCtx.SignObject;
                    HashAlgorithm hash = ctx.SignCtx.SignHashAlg;

                    if (signer is AsymmetricSignatureFormatter)
                    {
                        sig = ((AsymmetricSignatureFormatter)signer).CreateSignature(hash);
                    }
                    else if (signer is ECDsaCng)
                    {
                        sig = ((ECDsaCng)signer).SignHash(hash.Hash);
                    }

                    if (sig.Length > SignatureLen) throw new ArgumentException();

                    Marshal.Copy(sig, 0, Signature, sig.Length);

                    SignatureLen = sig.Length;
                }
            }
            catch (Exception e)
            {
                Debug.Print("Exception: " + e.ToString());
                return false;
            }
            finally
            {
                if (ctx != null)
                {
                    ctx.SignCtx.Clear();
                }
            }

            return true;
        }

        bool ISignatureDriver.SignUpdate(int session, IntPtr Data, int DataLen)
        {
            SessionData ctx = null;
            try
            {
                ctx = ((SessionDriver)this.Hal.Session).GetSessionCtx(session);
                unsafe
                {
                    byte[] data = new byte[DataLen];

                    Marshal.Copy(Data, data, 0, DataLen);

                    HashAlgorithm hash = ctx.SignCtx.SignHashAlg;

                    if (hash == null)
                    {
                        return false;
                    }
                    else
                    {
                        hash.ComputeHash(data);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Print("Exception: " + e.ToString());
                if (ctx != null)
                {
                    ctx.SignCtx.Clear();
                }
                return false;
            }

            return true;
        }

        bool ISignatureDriver.VerifyInit(int session, int alg, int hashAlg, int hKey)
        {
            return Init(session, (AlgorithmType)alg, (AlgorithmType)hashAlg, hKey, true);
        }

        bool ISignatureDriver.Verify(int session, IntPtr Data, int DataLen, IntPtr Signature, int SignatureLen)
        {
            SessionData ctx = null;
            try
            {
                ctx = ((SessionDriver)this.Hal.Session).GetSessionCtx(session);
                unsafe
                {
                    byte[] data = new byte[DataLen];
                    byte[] sig = new byte[SignatureLen];

                    Marshal.Copy(Data, data, 0, DataLen);
                    Marshal.Copy(Signature, sig, 0, SignatureLen);

                    object verifier = ctx.VerifyCtx.SignObject;
                    HashAlgorithm hash = ctx.VerifyCtx.SignHashAlg;

                    if (hash == null || m_signHash)
                    {
                        if (verifier is AsymmetricSignatureDeformatter)
                        {
                            return ((AsymmetricSignatureDeformatter)verifier).VerifySignature(data, sig);
                        }
                        else if(verifier is ECDsaCng)
                        {
                            return ((ECDsaCng)verifier).VerifyHash(data, sig);
                        }
                    }
                    else
                    {
                        hash.ComputeHash(data);

                        if (verifier is AsymmetricSignatureDeformatter)
                        {
                            return ((AsymmetricSignatureDeformatter)verifier).VerifySignature(hash, sig);
                        }
                        else if (verifier is ECDsaCng)
                        {
                            return ((ECDsaCng)verifier).VerifyHash(hash.Hash, sig);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Print("Exception: " + e.ToString());
                return false;
            }
            finally
            {
                if (ctx != null)
                {
                    ctx.VerifyCtx.Clear();
                }
            }

            return false;
        }

        bool ISignatureDriver.VerifyFinal(int session, IntPtr Signature, int SignatureLen)
        {
            SessionData ctx = null;
            try
            {
                ctx = ((SessionDriver)this.Hal.Session).GetSessionCtx(session);
                unsafe
                {
                    byte[] sig = new byte[SignatureLen];

                    Marshal.Copy(Signature, sig, 0, SignatureLen);

                    object verifier = ctx.VerifyCtx.SignObject;
                    HashAlgorithm hash = ctx.VerifyCtx.SignHashAlg;

                    if(hash == null) return false;

                    if (verifier is AsymmetricSignatureDeformatter)
                    {
                        return ((AsymmetricSignatureDeformatter)verifier).VerifySignature(hash, sig);
                    }
                    else if (verifier is ECDsaCng)
                    {
                        return ((ECDsaCng)verifier).VerifyHash(hash.Hash, sig);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Print("Exception: " + e.ToString());
                return false;
            }
            finally
            {
                if (ctx != null)
                {
                    ctx.VerifyCtx.Clear();
                }
            }
            return false;
        }

        bool ISignatureDriver.VerifyUpdate(int session, IntPtr Data, int DataLen)
        {
            SessionData ctx = null;
            try
            {
                ctx = ((SessionDriver)this.Hal.Session).GetSessionCtx(session);
                unsafe
                {
                    byte[] data = new byte[DataLen];

                    Marshal.Copy(Data, data, 0, DataLen);

                    HashAlgorithm hash = ctx.VerifyCtx.SignHashAlg;

                    if (hash == null) return false;

                    hash.ComputeHash(data);
                }
            }
            catch (Exception e)
            {
                Debug.Print("Exception3: " + e.ToString());
                if (ctx != null)
                {
                    ctx.VerifyCtx.Clear();
                }
                return false;
            }

            return true;
        }

        #endregion
    }
}