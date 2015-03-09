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
    internal class EncryptionDriver : HalDriver<IEncryptionDriver>, IEncryptionDriver
    {
        #region IEncryptionDriver Members

        bool IEncryptionDriver.DecryptInit(int session, int alg, IntPtr algParam, int algParamLen, int hKey)
        {
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

                    AsymmetricAlgorithm decAlg = cert.PrivateKey;

                    kd = new KeyData(null, decAlg);
                }
                else
                {
                    return false;
                }

                byte[] keyData = kd.KeyBytes;
                byte[] IV = null;

                if(algParam != IntPtr.Zero)
                {
                    IV = new byte[algParamLen];

                    Marshal.Copy(algParam, IV, 0, algParamLen);
                }

                ctx.DecryptCtx.CryptoAlgorithm = (AlgorithmType)alg;

                switch ((AlgorithmType)alg)
                {
                    case AlgorithmType.DES3_CBC:
                        {
                            TripleDESCryptoServiceProvider des3 = new TripleDESCryptoServiceProvider();
                            des3.Padding = PaddingMode.None;
                            ctx.DecryptCtx.CryptoObject = des3;
                            ctx.DecryptCtx.CryptoTransform = des3.CreateDecryptor(keyData, IV);
                        }
                        break;
                    case AlgorithmType.DES3_CBC_PAD:
                        {
                            TripleDESCryptoServiceProvider des3 = new TripleDESCryptoServiceProvider();
                            des3.Padding = PaddingMode.PKCS7;
                            ctx.DecryptCtx.CryptoObject = des3;
                            ctx.DecryptCtx.CryptoTransform = des3.CreateDecryptor(keyData, IV);
                        }
                        break;
                    case AlgorithmType.AES_CBC_PAD:
                        {
                            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
                            aes.Padding = PaddingMode.PKCS7;
                            aes.Mode = CipherMode.CBC;
                            ctx.DecryptCtx.CryptoObject = aes;
                            ctx.DecryptCtx.CryptoTransform = aes.CreateDecryptor(keyData, IV);
                        }
                        break;
                    case AlgorithmType.AES_ECB_PAD:
                        {
                            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
                            aes.Padding = PaddingMode.PKCS7;
                            aes.Mode = CipherMode.ECB;
                            ctx.DecryptCtx.CryptoObject = aes;
                            ctx.DecryptCtx.CryptoTransform = aes.CreateDecryptor(keyData, IV);
                        }
                        break;
                    case AlgorithmType.AES_CBC:
                        {
                            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
                            aes.Padding = PaddingMode.None;
                            aes.Mode = CipherMode.CBC;
                            ctx.DecryptCtx.CryptoObject = aes;
                            ctx.DecryptCtx.CryptoTransform = aes.CreateDecryptor(keyData, IV);
                        }
                        break;
                    case AlgorithmType.AES_ECB:
                        {
                            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
                            aes.Padding = PaddingMode.None;
                            aes.Mode = CipherMode.ECB;
                            ctx.DecryptCtx.CryptoObject = aes;
                            ctx.DecryptCtx.CryptoTransform = aes.CreateDecryptor(keyData, IV);
                        }
                        break;
                    case AlgorithmType.RSA_PKCS:
                        if (keyData == null)
                        {
                            ctx.DecryptCtx.CryptoObject = kd.KeyCsp as IDisposable;
                        }
                        else
                        {
                            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                            ctx.DecryptCtx.CryptoObject = rsa;
                            rsa.ImportCspBlob(keyData);
                            ctx.DecryptCtx.CryptoTransform = null;
                        }
                        break;

                    default:
                        return false;
                }

            }
            catch (Exception e)
            {
                Debug.Print(e.ToString());
                return false;
            }

            return true;
        }

        bool IEncryptionDriver.Decrypt(int session, IntPtr EncData, int EncDataLen, IntPtr Data, ref int DataLen)
        {
            SessionData ctx = null;
            try
            {
                ctx = ((SessionDriver)this.Hal.Session).GetSessionCtx(session);
                byte[] data = null;
                unsafe
                {
                    switch (ctx.DecryptCtx.CryptoAlgorithm)
                    {
                        case AlgorithmType.DES3_CBC:
                        case AlgorithmType.DES3_CBC_PAD:
                        case AlgorithmType.AES_CBC:
                        case AlgorithmType.AES_ECB:
                        case AlgorithmType.AES_ECB_PAD:
                        case AlgorithmType.AES_CBC_PAD:
                            {
                                if (Data == IntPtr.Zero)
                                {
                                    int blockSize = ctx.DecryptCtx.CryptoTransform.OutputBlockSize;
                                    int mod = EncDataLen % blockSize;
                                       
                                    DataLen = EncDataLen + (blockSize - mod);

                                    return true;
                                }

                                byte[] encData = new byte[EncDataLen];

                                Marshal.Copy(EncData, encData, 0, EncDataLen);

                                data = ctx.DecryptCtx.CryptoTransform.TransformFinalBlock(encData, 0, encData.Length);
                            }
                            break;

                        case AlgorithmType.RSA_PKCS:
                            {
                                RSACryptoServiceProvider rsa = ctx.DecryptCtx.CryptoObject as RSACryptoServiceProvider;

                                if (Data == IntPtr.Zero)
                                {
                                    int blockSize = (rsa.KeySize + 7) / 8;
                                    int mod = EncDataLen % blockSize;

                                    DataLen = EncDataLen + (blockSize - mod);

                                    return true;
                                }


                                byte[] encData = new byte[EncDataLen];

                                Marshal.Copy(EncData, encData, 0, EncDataLen);

                                data = rsa.Decrypt(encData, false);
                            }
                            break;

                        default:
                            return false;
                    }
                }

                if (data.Length > DataLen) throw new ArgumentException();

                Marshal.Copy(data, 0, Data, Math.Min(DataLen, data.Length));

                DataLen = data.Length;
            }
            catch (Exception e)
            {
                Debug.Print(e.ToString());
                return false;
            }
            finally
            {
                if (Data != IntPtr.Zero && ctx.DecryptCtx != null)
                {
                    ctx.DecryptCtx.Clear();
                }
            }

            return true;
        }

        bool IEncryptionDriver.DecryptFinal(int session, IntPtr Data, ref int DataLen)
        {
            try
            {
                SessionData ctx = ((SessionDriver)this.Hal.Session).GetSessionCtx(session);
                unsafe
                {
                    switch (ctx.DecryptCtx.CryptoAlgorithm)
                    {
                        case AlgorithmType.DES3_CBC:
                        case AlgorithmType.DES3_CBC_PAD:
                        case AlgorithmType.AES_CBC:
                        case AlgorithmType.AES_ECB:
                        case AlgorithmType.AES_ECB_PAD:
                        case AlgorithmType.AES_CBC_PAD:
                            {
                                byte[] final = new byte[0];

                                byte[] data = ctx.DecryptCtx.CryptoTransform.TransformFinalBlock(final, 0, final.Length);

                                if (data.Length > DataLen) throw new ArgumentException();

                                Marshal.Copy(data, 0, Data, data.Length);

                                DataLen = data.Length;

                                ctx.DecryptCtx.Clear();
                            }
                            break;
                        case AlgorithmType.RSA_PKCS:
                            DataLen = 0;
                            ctx.DecryptCtx.Clear();
                            break;

                        default:
                            return false;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Print(e.ToString());
                return false;
            }

            return true;
        }

        bool IEncryptionDriver.DecryptUpdate(int session, IntPtr EncData, int EncDataLen, IntPtr Data, ref int DataLen)
        {
            try
            {
                SessionData ctx = ((SessionDriver)this.Hal.Session).GetSessionCtx(session);
                unsafe
                {
                    switch (ctx.DecryptCtx.CryptoAlgorithm)
                    {
                        case AlgorithmType.DES3_CBC:
                        case AlgorithmType.DES3_CBC_PAD:
                        case AlgorithmType.AES_CBC:
                        case AlgorithmType.AES_ECB:
                        case AlgorithmType.AES_ECB_PAD:
                        case AlgorithmType.AES_CBC_PAD:
                            {
                                byte[] encData = new byte[EncDataLen];

                                Marshal.Copy(EncData, encData, 0, EncDataLen);

                                byte[] data = new byte[DataLen];

                                int len = ctx.DecryptCtx.CryptoTransform.TransformBlock(encData, 0, encData.Length, data, 0);

                                if (len > DataLen) throw new ArgumentException();

                                Marshal.Copy(data, 0, Data, len);

                                DataLen = len;
                            }
                            break;

                        case AlgorithmType.RSA_PKCS:
                            {
                                RSACryptoServiceProvider rsa = ctx.DecryptCtx.CryptoObject as RSACryptoServiceProvider;

                                byte[] encData = new byte[EncDataLen];

                                Marshal.Copy(EncData, encData, 0, EncDataLen);

                                byte[] data = rsa.Decrypt(encData, false);

                                if (data.Length > DataLen) throw new ArgumentException();

                                Marshal.Copy(data, 0, Data, data.Length);

                                DataLen = data.Length;
                            }
                            break;

                        default:
                            return false;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Print(e.ToString());
                return false;
            }

            return true;
        }

        bool IEncryptionDriver.EncryptInit(int session, int alg, IntPtr algParam, int algParamLen, int hKey)
        {
            try
            {
                SessionData ctx = ((SessionDriver)this.Hal.Session).GetSessionCtx(session);

                KeyData kd = null;
                CryptokiObject obj = ctx.ObjectCtx.GetObject(hKey);
     
                if(obj ==  null) return false;
               
                if(obj.Type == CryptokiObjectType.Key)
                {
                    kd = obj.Data as KeyData;
                }
                else if(obj.Type == CryptokiObjectType.Cert)
                {
                    X509Certificate2 cert = obj.Data as X509Certificate2;

                    AsymmetricAlgorithm encAlg = cert.PublicKey.Key;

                    kd = new KeyData(null, encAlg);
                }
                else
                {
                    return false;
                }

                byte[] keyData = kd.KeyBytes;
                byte[] IV      = null;

                if (algParam != IntPtr.Zero)
                {
                    IV = new byte[algParamLen];

                    Marshal.Copy(algParam, IV, 0, algParamLen);
                }

                ctx.EncryptCtx.CryptoAlgorithm = (AlgorithmType)alg;

                switch ((AlgorithmType)alg)
                {
                    case AlgorithmType.DES3_CBC_PAD:
                        {
                            TripleDESCryptoServiceProvider des3 = new TripleDESCryptoServiceProvider();
                            des3.Padding = PaddingMode.PKCS7;
                            ctx.EncryptCtx.CryptoObject = des3;
                            ctx.EncryptCtx.CryptoTransform = des3.CreateEncryptor(keyData, IV);
                        }
                        break;
                    case AlgorithmType.DES3_CBC:
                        {
                            TripleDESCryptoServiceProvider des3 = new TripleDESCryptoServiceProvider();
                            des3.Padding = PaddingMode.None;
                            ctx.EncryptCtx.CryptoObject = des3;
                            ctx.EncryptCtx.CryptoTransform = des3.CreateEncryptor(keyData, IV);
                        }
                        break;
                    case AlgorithmType.AES_CBC_PAD:
                        {
                            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
                            aes.Padding = PaddingMode.PKCS7;
                            aes.Mode = CipherMode.CBC;
                            ctx.EncryptCtx.CryptoObject = aes;
                            ctx.EncryptCtx.CryptoTransform = aes.CreateEncryptor(keyData, IV);
                        }
                        break;
                    case AlgorithmType.AES_ECB_PAD:
                        {
                            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
                            aes.Padding = PaddingMode.PKCS7;
                            aes.Mode = CipherMode.ECB;
                            ctx.EncryptCtx.CryptoObject = aes;
                            ctx.EncryptCtx.CryptoTransform = aes.CreateEncryptor(keyData, IV);
                        }
                        break;
                    case AlgorithmType.AES_CBC:
                        {
                            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
                            aes.Padding = PaddingMode.None;
                            aes.Mode = CipherMode.CBC;
                            ctx.EncryptCtx.CryptoObject = aes;
                            ctx.EncryptCtx.CryptoTransform = aes.CreateEncryptor(keyData, IV);
                        }
                        break;
                    case AlgorithmType.AES_ECB:
                        {
                            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
                            aes.Padding = PaddingMode.None;
                            aes.Mode = CipherMode.ECB;
                            ctx.EncryptCtx.CryptoObject = aes;
                            ctx.EncryptCtx.CryptoTransform = aes.CreateEncryptor(keyData, IV);
                        }
                        break;
                    case AlgorithmType.RSA_PKCS:
                        if (keyData == null)
                        {
                            ctx.EncryptCtx.CryptoObject = kd.KeyCsp as IDisposable;
                        }
                        else
                        {
                            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                            ctx.EncryptCtx.CryptoObject = rsa;
                            rsa.ImportCspBlob(keyData);
                            ctx.EncryptCtx.CryptoTransform = null;
                        }
                        break;
                    case AlgorithmType.DSA:
                    case AlgorithmType.DSA_SHA1:
                        if (keyData == null)
                        {
                            ctx.EncryptCtx.CryptoObject = kd.KeyCsp as IDisposable;
                        }
                        else
                        {
                            DSACryptoServiceProvider dsa = new DSACryptoServiceProvider();
                            ctx.EncryptCtx.CryptoObject = dsa;
                            ctx.EncryptCtx.CryptoTransform = null;
                            dsa.ImportCspBlob(keyData);
                        }
                        break;
                    default:
                        return false;
                }
            }
            catch (Exception e)
            {
                Debug.Print(e.ToString());
                return false;
            }

            return true;
        }

        bool IEncryptionDriver.Encrypt(int session, IntPtr Data, int DataLen, IntPtr EncData, ref int EncDataLen)
        {
            SessionData ctx = null;
            try
            {
                byte[] encData = null;

                ctx = ((SessionDriver)this.Hal.Session).GetSessionCtx(session);
                unsafe
                {
                    switch (ctx.EncryptCtx.CryptoAlgorithm)
                    {
                        case AlgorithmType.DES3_CBC:
                        case AlgorithmType.DES3_CBC_PAD:
                        case AlgorithmType.AES_CBC:
                        case AlgorithmType.AES_ECB:
                        case AlgorithmType.AES_ECB_PAD:
                        case AlgorithmType.AES_CBC_PAD:
                            {
                                if (EncData == IntPtr.Zero)
                                {
                                    int blockSize = ctx.EncryptCtx.CryptoTransform.OutputBlockSize;
                                    int mod = DataLen % blockSize;

                                    EncDataLen = DataLen + (blockSize - mod);

                                    return true;
                                }

                                if (AlgorithmType.AES_CBC == ctx.EncryptCtx.CryptoAlgorithm ||
                                    AlgorithmType.AES_ECB == ctx.EncryptCtx.CryptoAlgorithm)
                                {
                                    int blockSize = ctx.EncryptCtx.CryptoTransform.OutputBlockSize;
                                    int mod = DataLen % blockSize;

                                    DataLen = DataLen + (blockSize - mod);
                                }

                                byte[] data = new byte[DataLen];

                                Marshal.Copy(Data, data, 0, DataLen);

                                encData = ctx.EncryptCtx.CryptoTransform.TransformFinalBlock(data, 0, data.Length);
                            }
                            break;
                        case AlgorithmType.RSA_PKCS:
                            {
                                RSACryptoServiceProvider rsa = ctx.EncryptCtx.CryptoObject as RSACryptoServiceProvider;

                                if (EncData == IntPtr.Zero)
                                {
                                    int blockSize = (rsa.KeySize + 7) / 8;
                                    int mod = DataLen % blockSize;

                                    EncDataLen = DataLen + (blockSize - mod);

                                    return true;
                                }

                                byte[] data = new byte[DataLen];

                                Marshal.Copy(Data, data, 0, DataLen);

                                encData = rsa.Encrypt(data, false);

                            }
                            break;
                        case AlgorithmType.DSA:
                        case AlgorithmType.DSA_SHA1:
                            {
                                DSACryptoServiceProvider dsa = ctx.EncryptCtx.CryptoObject as DSACryptoServiceProvider;

                                if (EncData == IntPtr.Zero)
                                {
                                    int blockSize = (dsa.KeySize + 7) / 8;
                                    int mod = DataLen % blockSize;

                                    EncDataLen = DataLen + (blockSize - mod);

                                    return true;
                                }


                                byte[] data = new byte[DataLen];

                                Marshal.Copy(Data, data, 0, DataLen);

                                encData = dsa.SignHash(data, "SHA1");
                            }
                            break;
                        default:
                            return false;
                    }
                }

                if (encData.Length > EncDataLen) throw new ArgumentException();

                Marshal.Copy(encData, 0, EncData, encData.Length);

                EncDataLen = encData.Length;
            }
            catch (Exception e)
            {
                Debug.Print(e.ToString());
                return false;
            }
            finally
            {
                if (EncData != IntPtr.Zero && ctx != null)
                {
                    ctx.EncryptCtx.Clear();
                }
            }

            return true;
        }

        bool IEncryptionDriver.EncryptFinal(int session, IntPtr EncData, ref int EncDataLen)
        {
            try
            {
                SessionData ctx = ((SessionDriver)this.Hal.Session).GetSessionCtx(session);
                unsafe
                {
                    switch (ctx.EncryptCtx.CryptoAlgorithm)
                    {
                        case AlgorithmType.DES3_CBC:
                        case AlgorithmType.DES3_CBC_PAD:
                        case AlgorithmType.AES_CBC:
                        case AlgorithmType.AES_ECB:
                        case AlgorithmType.AES_ECB_PAD:
                        case AlgorithmType.AES_CBC_PAD:
                            {
                                byte[] encData;

                                byte[] input = new byte[0];

                                encData = ctx.EncryptCtx.CryptoTransform.TransformFinalBlock(input, 0, input.Length);

                                if (encData.Length > EncDataLen) throw new ArgumentException();

                                Marshal.Copy(encData, 0, EncData, encData.Length);

                                EncDataLen = encData.Length;

                                ctx.EncryptCtx.Clear();
                            }
                            break;
                        case AlgorithmType.RSA_PKCS:
                            {
                                EncDataLen = 0;
                                ctx.EncryptCtx.Clear();
                            }
                            break;

                        default:
                            return false;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Print(e.ToString());
                return false;
            }

            return true;
        }

        bool IEncryptionDriver.EncryptUpdate(int session, IntPtr Part, int PartLen, IntPtr EncData, ref int EncDataLen)
        {
            try
            {
                SessionData ctx = ((SessionDriver)this.Hal.Session).GetSessionCtx(session);
                unsafe
                {
                    switch (ctx.EncryptCtx.CryptoAlgorithm)
                    {
                        case AlgorithmType.DES3_CBC:
                        case AlgorithmType.DES3_CBC_PAD:
                        case AlgorithmType.AES_CBC:
                        case AlgorithmType.AES_ECB:
                        case AlgorithmType.AES_ECB_PAD:
                        case AlgorithmType.AES_CBC_PAD:
                            {
                                byte[] data = new byte[PartLen];

                                Marshal.Copy(Part, data, 0, PartLen);

                                byte[] encData = new byte[EncDataLen];

                                int len = ctx.EncryptCtx.CryptoTransform.TransformBlock(data, 0, data.Length, encData, 0);

                                if (len > EncDataLen) throw new ArgumentException();

                                Marshal.Copy(encData, 0, EncData, len);

                                EncDataLen = len;
                            }
                            break;
                        case AlgorithmType.RSA_PKCS:
                            {
                                RSACryptoServiceProvider rsa = ctx.EncryptCtx.CryptoObject as RSACryptoServiceProvider;

                                byte[] data = new byte[PartLen];

                                Marshal.Copy(Part, data, 0, PartLen);

                                byte[] encData = rsa.Encrypt(data, false);

                                if (encData.Length > EncDataLen) throw new ArgumentException();

                                Marshal.Copy(encData, 0, EncData, encData.Length);

                                EncDataLen = encData.Length;
                            }
                            break;
                        default:
                            return false;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Print(e.ToString());
                return false;
            }

            return true;
        }

        #endregion
    }
}