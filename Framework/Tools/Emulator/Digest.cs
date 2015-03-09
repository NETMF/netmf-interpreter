////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

namespace Microsoft.SPOT.Emulator.PKCS11
{
    internal class DigestDriver : HalDriver<IDigestDriver>, IDigestDriver
    {
        #region IDigestDriver Members

        bool IDigestDriver.Digest(int session, IntPtr Data, int DataLen, ref IntPtr Digest, ref int DigestLen)
        {
            try
            {
                SessionData ctx = ((SessionDriver)this.Hal.Session).GetSessionCtx(session);

                if (ctx.DigestCtx.Digest != null)
                {
                    byte[] data = new byte[DataLen];
                    byte[] digest;

                    Marshal.Copy(Data, data, 0, DataLen);

                    digest = ctx.DigestCtx.Digest.ComputeHash(data, 0, DataLen);

                    if (digest.Length > DigestLen) return false;

                    Marshal.Copy(digest, 0, Digest, digest.Length);

                    DigestLen = digest.Length;

                    ctx.DigestCtx.Clear();
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        bool IDigestDriver.DigestFinal(int session, ref IntPtr Digest, ref int DigestLen)
        {
            try
            {
                SessionData ctx = ((SessionDriver)this.Hal.Session).GetSessionCtx(session);

                if (ctx.DigestCtx.Digest != null)
                {
                    if (ctx.DigestCtx.isPartialUpdate)
                    {
                        ctx.DigestCtx.Digest.TransformFinalBlock(new byte[0], 0, 0);
                    }

                    byte[] digest = ctx.DigestCtx.Digest.Hash;

                    if (digest.Length > DigestLen) return false;

                    Marshal.Copy(digest, 0, Digest, digest.Length);

                    DigestLen = digest.Length;

                    ctx.DigestCtx.Clear();
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        bool IDigestDriver.DigestKey(int session, int hKey)
        {
            return false;
        }

        bool IDigestDriver.DigestUpdate(int session, IntPtr Data, int DataLen)
        {
            try
            {
                SessionData ctx = ((SessionDriver)this.Hal.Session).GetSessionCtx(session);

                if (ctx.DigestCtx.Digest != null)
                {
                    ctx.DigestCtx.isPartialUpdate = true;
                    byte[] data = new byte[DataLen];

                    Marshal.Copy(Data, data, 0, DataLen);

                    ctx.DigestCtx.Digest.TransformBlock(data, 0, DataLen, data, 0);
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
            
            return true;
        }

        bool IDigestDriver.DigestInit(int session, int alg, int hHmacKey)
        {
            try
            {
                SessionData ctx = ((SessionDriver)this.Hal.Session).GetSessionCtx(session);

                byte[] hmacKey = null;

                if (hHmacKey != -1)
                {
                    KeyData kd = ctx.ObjectCtx.GetObject(hHmacKey).Data as KeyData;

                    if (kd == null) return false;

                    hmacKey = kd.KeyBytes;
                }


                switch ((AlgorithmType)alg)
                {
                    case AlgorithmType.SHA_1:
                        ctx.DigestCtx.Digest = new SHA1CryptoServiceProvider();
                        break;

                    case AlgorithmType.SHA256:
                        ctx.DigestCtx.Digest = new SHA256CryptoServiceProvider();
                        break;

                    case AlgorithmType.SHA384:
                        ctx.DigestCtx.Digest = new SHA384CryptoServiceProvider();
                        break;

                    case AlgorithmType.SHA512:
                        ctx.DigestCtx.Digest = new SHA512CryptoServiceProvider();
                        break;

                    case AlgorithmType.MD5:
                        ctx.DigestCtx.Digest = new MD5CryptoServiceProvider();
                        break;

                    case AlgorithmType.RIPEMD160:
                        ctx.DigestCtx.Digest = new RIPEMD160Managed();
                        break;

                    case AlgorithmType.RIPEMD160_HMAC:
                        ctx.DigestCtx.Digest = new HMACRIPEMD160(hmacKey);
                        break;

                    case AlgorithmType.MD5_HMAC:
                        if (hmacKey == null) return false;
                        ctx.DigestCtx.Digest = new HMACMD5(hmacKey);
                        break;

                    case AlgorithmType.SHA_1_HMAC:
                        if (hmacKey == null) return false;
                        ctx.DigestCtx.Digest = new HMACSHA1(hmacKey);
                        break;

                    case AlgorithmType.SHA256_HMAC:
                        if (hmacKey == null) return false;
                        ctx.DigestCtx.Digest = new HMACSHA256(hmacKey);
                        break;

                    case AlgorithmType.SHA384_HMAC:
                        if (hmacKey == null) return false;
                        ctx.DigestCtx.Digest = new HMACSHA384(hmacKey);
                        break;

                    case AlgorithmType.SHA512_HMAC:
                        if (hmacKey == null) return false;
                        ctx.DigestCtx.Digest = new HMACSHA512(hmacKey);
                        break;

                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        #endregion
    }
}

