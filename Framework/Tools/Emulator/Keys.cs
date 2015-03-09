////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Microsoft.SPOT.Emulator.PKCS11
{
    internal class KeyManagementDriver : HalDriver<IKeyManagementDriver>, IKeyManagementDriver
    {
        internal class SecretKey
        {
            internal SecretKey(int size, byte[] data)
            {
                Size = size;
                Data = data;
            }
            internal int Size;
            internal byte[] Data;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class BLOB
        {
            internal uint size;
            internal IntPtr pBlobData;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class BLOBHEADER
        {
            internal byte bType;
            internal byte bVersion;
            internal UInt16 reserved;
            internal UInt32 alg_id;
            internal UInt32 magic;
            internal UInt32 bitLen;
        }

        internal class ECDH_Params
        {
            internal AlgorithmType kdf;
            internal byte[] SharedData;
            internal byte[] PublicData;

            internal ECDH_Params(IntPtr data, int paramlen)
            {
                if (paramlen < 4 * 3) throw new ArgumentException();

                kdf     = (AlgorithmType)Marshal.ReadInt32(data); data+=4; paramlen -= 4;
                int len = Marshal.ReadInt32(data); data += 4; paramlen -= 4;

                if (len > 0)
                {
                    if (paramlen < (len + 4)) throw new ArgumentException();

                    SharedData = new byte[len];
                    Marshal.Copy(data, SharedData, 0, len);
                    paramlen -= len;
                    data += len;
                }

                len = Marshal.ReadInt32(data); data += 4; paramlen -= 4;
                if (len > 0)
                {
                    if (paramlen < len) throw new ArgumentException();

                    PublicData = new byte[len];
                    Marshal.Copy(data, PublicData, 0, len);
                    data += len;
                }
            }
        }

        #region IKeyManagementDriver Members

        bool IKeyManagementDriver.GetPublicKeyData(int session, int hKey, IntPtr data, ref int dataLen)
        {
            try
            {
                SessionData ctx = ((SessionDriver)this.Hal.Session).GetSessionCtx(session);
                KeyData kd = ctx.ObjectCtx.GetObject(hKey).Data as KeyData;

                if (kd == null) return false;

                if (kd.KeyCsp is RSACryptoServiceProvider)
                {
                    RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)kd.KeyCsp;

                    byte []key = rsa.ExportCspBlob(false);

                    if (dataLen < key.Length) return false;

                    Marshal.Copy(key, 0, data, key.Length);

                    dataLen = key.Length;
                }
                else if (kd.KeyCsp is DSACryptoServiceProvider)
                {
                    DSACryptoServiceProvider dsa = (DSACryptoServiceProvider)kd.KeyCsp;

                    byte[] key = dsa.ExportCspBlob(false);

                    if (dataLen < key.Length) return false;

                    Marshal.Copy(key, 0, data, key.Length);

                    dataLen = key.Length;
                }
                else if (kd.KeyCsp is ECDsaCng)
                {
                    ECDsaCng cng = (ECDsaCng)kd.KeyCsp;

                    byte[] key = cng.Key.Export(CngKeyBlobFormat.EccPublicBlob);

                    if (dataLen < (key.Length-8)) return false;

                    Marshal.Copy(key, 8, data, key.Length-8);

                    dataLen = key.Length-8;
                }
                else if (kd.KeyCsp is ECDiffieHellmanCng)
                {
                    ECDiffieHellmanCng cng = (ECDiffieHellmanCng)kd.KeyCsp;

                    byte[] key = cng.Key.Export(CngKeyBlobFormat.EccPublicBlob);

                    if (dataLen < (key.Length-8)) return false;

                    Marshal.Copy(key, 8, data, key.Length-8);

                    dataLen = key.Length-8;
                }
                else if (kd.KeyCsp is SecretKey)
                {
                    SecretKey key = (SecretKey)kd.KeyCsp;

                    if(dataLen < key.Data.Length) return false;

                    Marshal.Copy(key.Data, 0, data, key.Data.Length);

                    dataLen = key.Data.Length;
                }
                else if (kd.KeyCsp is AesCryptoServiceProvider)
                {
                    AesCryptoServiceProvider aes = (AesCryptoServiceProvider)kd.KeyCsp;

                    if (dataLen < aes.Key.Length) return false;

                    Marshal.Copy(aes.Key, 0, data, aes.Key.Length);

                    dataLen = aes.Key.Length;
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

        int IKeyManagementDriver.GetKeySize(int session, int hKey)
        {
            try
            {
                SessionData ctx = ((SessionDriver)this.Hal.Session).GetSessionCtx(session);
                KeyData kd = ctx.ObjectCtx.GetObject(hKey).Data as KeyData;

                if (kd == null) return -1;

                if (kd.KeyCsp is RSACryptoServiceProvider)
                {
                    RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)kd.KeyCsp;

                    return (int)rsa.KeySize;
                }
                else if (kd.KeyCsp is AesCryptoServiceProvider)
                {
                    AesCryptoServiceProvider aes = (AesCryptoServiceProvider)kd.KeyCsp;

                    return (int)aes.KeySize;
                }
                else if (kd.KeyCsp is TripleDESCryptoServiceProvider)
                {
                    TripleDESCryptoServiceProvider tdes = (TripleDESCryptoServiceProvider)kd.KeyCsp;

                    return (int)tdes.KeySize;
                }
                else if (kd.KeyCsp is DSACryptoServiceProvider)
                {
                    DSACryptoServiceProvider dsa = (DSACryptoServiceProvider)kd.KeyCsp;

                    return (int)dsa.KeySize;
                }
                else if (kd.KeyCsp is SecretKey)
                {
                    SecretKey sk = (SecretKey)kd.KeyCsp;
                    return sk.Size;
                }
                else if (kd.KeyCsp is ECDsaCng)
                {
                    ECDsaCng cng = (ECDsaCng)kd.KeyCsp;

                    return cng.KeySize;
                }
                else if (kd.KeyCsp is ECDiffieHellmanCng)
                {
                    ECDiffieHellmanCng cng = (ECDiffieHellmanCng)kd.KeyCsp;

                    return cng.KeySize;
                }
            }
            catch
            {
                }

            return -1;
        }

        KeyType IKeyManagementDriver.GetKeyType(int session, int hKey)
        {
            try
            {
                SessionData ctx = ((SessionDriver)this.Hal.Session).GetSessionCtx(session);
                KeyData kd = ctx.ObjectCtx.GetObject(hKey).Data as KeyData;

                if (kd == null)
                {
                    unchecked
                    {
                        return (KeyType)(-1);
                    }
                }

                if (kd.KeyCsp is RSACryptoServiceProvider)
                {
                    return KeyType.RSA;
                }
                else if (kd.KeyCsp is AesCryptoServiceProvider)
                {
                    return KeyType.AES;
                }
                else if (kd.KeyCsp is TripleDESCryptoServiceProvider)
                {
                    return KeyType.DES3;
                }
                else if (kd.KeyCsp is DSACryptoServiceProvider)
                {
                    return KeyType.DSA;
                }
                else if (kd.KeyCsp is SecretKey)
                {
                    return KeyType.GENERIC_SECRET;
                }
                else if (kd.KeyCsp is ECDsaCng)
                {
                    return KeyType.EC;
                }
                else if (kd.KeyCsp is ECDiffieHellmanCng)
                {
                    return KeyType.EC;
                }
            }
            catch
            {
            }

            unchecked
            {
                return (KeyType)(-1);
            }
        }

        bool IKeyManagementDriver.LoadSymmetricKey(int session, IntPtr secret, int secretLen, KeyType keyType, out int hKey)
        {
            hKey = -1;

            try
            {
                SessionData ctx = ((SessionDriver)this.Hal.Session).GetSessionCtx(session);

                switch (keyType)
                {
                    case KeyType.GENERIC_SECRET:
                    case KeyType.AES:
                    case KeyType.DES3:
                        {
                            byte[] data = new byte[secretLen];

                            unsafe
                            {
                                Marshal.Copy(secret, data, 0, secretLen);
                            }

                            SecretKey key = new SecretKey(secretLen * 8, data);

                            hKey = ctx.ObjectCtx.AddObject(CryptokiObjectType.Key, new KeyData(data, key));
                        }
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

        bool IKeyManagementDriver.LoadAsymmetricKey(int session, IntPtr keyData, int keyDataLen, KeyType keyType, out int hKey)
        {
            bool bRet = false;
            byte []key;
            BLOB blob = new BLOB();
            BLOBHEADER header = new BLOBHEADER();

            hKey = -1;

            try
            {
                SessionData ctx = ((SessionDriver)this.Hal.Session).GetSessionCtx(session);

                Marshal.PtrToStructure(keyData, blob);

                //Debug.Print("   KeySize: " + blob.size.ToString());
                //Debug.Print("   Header:  " + blob.pBlobData.ToString());

                key = new byte[blob.size];

                Marshal.Copy(blob.pBlobData, key, 0, (int)blob.size);

                //for (int i = 0; i < (int)key.Length; i+=8)
                //{
                //    string tmp = "";
                //    for (int q = i; (q - i < 8) && q < blob.size; q++)
                //    {
                //        tmp += string.Format("{0:x2} ", key[q]);
                //    }

                //    Debug.Print(tmp);
                //}

                Marshal.PtrToStructure(blob.pBlobData, header);

                //Debug.Print("   Type:  " + header.bType.ToString());
                //Debug.Print("   Ver :  " + header.bVersion.ToString());
                //Debug.Print("   Res :  " + header.reserved.ToString());
                //Debug.Print("   Alg :  " + header.alg_id.ToString());


                if (header.bType == 7 || header.bType == 6)
                {
                    if ((header.alg_id & (7 << 9)) == (2 << 9))
                    {
                        RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();

                        //for (int i = 0; i < key.Length; i++)
                        //{
                        //    Debug.Write(string.Format("0x{0:X02}, ", key[i]));
                        //}

                        //RSAParameters rp = new RSAParameters();
                        //rp.Modulus = new byte[1];
                        //for (int i = 0; i < rp.Modulus.Length; i++) rp.Modulus[i] = 19;
                        //rp.Exponent = new byte[] { 91 };
                        //rsa.ImportParameters(rp);
                        //byte[] blb = rsa.ExportCspBlob(false);
                        //Debug.Print(blb[0].ToString());

                        rsa.ImportCspBlob(key);

                        hKey = ctx.ObjectCtx.AddObject(CryptokiObjectType.Key, new KeyData(key, rsa));

                        bRet = true;
                    }
                    else if (header.alg_id == 0x2200)
                    {
                        DSACryptoServiceProvider dsa = new DSACryptoServiceProvider();

                        dsa.ImportCspBlob(key);

                        hKey = ctx.ObjectCtx.AddObject(CryptokiObjectType.Key, new KeyData(key, dsa));

                        bRet = true;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Print("   " + e.ToString());
                return false;
            }

            return bRet;
            // throw new NotImplementedException();
        }

        bool IKeyManagementDriver.DeriveKey(int session, int alg, IntPtr pParam, int paramLen, int hBaseKey, out int hKey)
        {
            hKey = -1;

            try
            {
                SessionData ctx = ((SessionDriver)this.Hal.Session).GetSessionCtx(session);
                KeyData kd = ctx.ObjectCtx.GetObject(hBaseKey).Data as KeyData;

                if (kd == null) return false;

                switch((AlgorithmType)alg)
                {
                    case AlgorithmType.ECDH1_DERIVE:

                        ECDH_Params ecdh = new ECDH_Params(pParam, paramLen);

                        ECDiffieHellmanCng ec = kd.KeyCsp as ECDiffieHellmanCng;

                        ECDiffieHellmanCng cng = new ECDiffieHellmanCng(ec.Key);

                        byte[] pubData = new byte[ecdh.PublicData.Length + 8];

                        pubData[0] = (byte)'E';
                        pubData[1] = (byte)'C';
                        pubData[2] = (byte)'K';
                        switch (ec.KeySize)
                        {
                            case 521:
                                pubData[3] = (byte)'5';
                                pubData[4] = (byte)((521 + 7) / 8);
                                break;
                            case 384:
                                pubData[3] = (byte)'3';
                                pubData[4] = (byte)((384 + 7) / 8);
                                break;
                            case 256:
                                pubData[3] = (byte)'1';
                                pubData[4] = (byte)((256 + 7) / 8);
                                break;
                        }
                        pubData[5] = 0;
                        pubData[6] = 0;
                        pubData[7] = 0;

                        Array.Copy(ecdh.PublicData, 0, pubData, 8, ecdh.PublicData.Length);

                        //CngKey otherPublicKey = CngKey.Import(pubData, CngKeyBlobFormat.EccPublicBlob);

                        ECDiffieHellmanPublicKey otherPublicKey =  ECDiffieHellmanCngPublicKey.FromByteArray(pubData, CngKeyBlobFormat.EccPublicBlob);


                        //byte[] otherKeyData = otherPublicKey.Export(CngKeyBlobFormat.EccPublicBlob);

                        //Debug.Print(otherKeyData[0].ToString());

                        switch(ecdh.kdf)
                        {
                            case AlgorithmType.NULL_KEY_DERIVATION:
                                cng.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
                                cng.HashAlgorithm = CngAlgorithm.Sha1;
                                break;

                            case AlgorithmType.SHA1_KEY_DERIVATION:
                                cng.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
                                cng.HashAlgorithm = CngAlgorithm.Sha1;
                                break;

                            case AlgorithmType.SHA256_KEY_DERIVATION:
                                cng.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
                                cng.HashAlgorithm = CngAlgorithm.Sha256;
                                break;

                            case AlgorithmType.SHA512_KEY_DERIVATION:
                                cng.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
                                cng.HashAlgorithm = CngAlgorithm.Sha512;
                                break;

                            case AlgorithmType.MD5_KEY_DERIVATION:
                                cng.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
                                cng.HashAlgorithm = CngAlgorithm.MD5;
                                break;

                            case AlgorithmType.SHA224_HMAC:
                            case AlgorithmType.TLS_MASTER_KEY_DERIVE_DH:
                            default:
                                return false;
                        }
                        cng.SecretPrepend = null;
                        cng.SecretAppend = null;

                        byte[] keyData = cng.DeriveKeyMaterial(otherPublicKey);

                        SecretKey key = new SecretKey(keyData.Length * 8, keyData);

                        hKey = ctx.ObjectCtx.AddObject(CryptokiObjectType.Key, new KeyData(keyData, key));

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

        bool IKeyManagementDriver.GenerateKey(int session, int alg, int keySize, out int hKey)
        {
            hKey = -1;

            try
            {
                SessionData ctx = ((SessionDriver)this.Hal.Session).GetSessionCtx(session);

                switch ((AlgorithmType)alg)
                {
                    case AlgorithmType.DES3_KEY_GEN:
                        {
                            TripleDESCryptoServiceProvider des3 = new TripleDESCryptoServiceProvider();

                            des3.KeySize = keySize;

                            des3.GenerateKey();

                            hKey = ctx.ObjectCtx.AddObject(CryptokiObjectType.Key, new KeyData(des3.Key, des3));
                        }
                        break;
                    case AlgorithmType.AES_KEY_GEN:
                        {
                            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();

                            aes.KeySize = keySize;

                            aes.GenerateKey();

                            hKey = ctx.ObjectCtx.AddObject(CryptokiObjectType.Key, new KeyData(aes.Key, aes));
                        }
                        break;
                    case AlgorithmType.GENERIC_SECRET_KEY_GEN:
                        {
                            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                            byte[] keyBytes = new byte[keySize];

                            rng.GetBytes(keyBytes);

                            SecretKey key = new SecretKey(keySize, keyBytes);

                            hKey = ctx.ObjectCtx.AddObject(CryptokiObjectType.Key, new KeyData(keyBytes, key));
                        }
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

        bool IKeyManagementDriver.GenerateKeyPair(int session, int alg, int keySize, out int hPubKey, out int hPrivKey)
        {
            bool bRet = false;

            hPrivKey = -1;
            hPubKey  = -1;

            try
            {
                SessionData ctx = ((SessionDriver)this.Hal.Session).GetSessionCtx(session);
                CryptokiObjectMgrDriver objMgr = (CryptokiObjectMgrDriver)Hal.CryptokiObjectMgr;

                switch ((AlgorithmType)alg)
                {
                    case AlgorithmType.RSA_PKCS_KEY_PAIR_GEN:
                        {
                            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(keySize);

                            //byte[] data = rsa.ExportCspBlob(true);

                            //Debug.WriteLine(string.Format("bType: 0x{0:X02}", data[0]));
                            //Debug.WriteLine(string.Format("bVer : 0x{0:X02}", data[1]));
                            //Debug.WriteLine(string.Format("Res  : 0x{0:X04}", (ushort)((uint)data[2] << 0 | (uint)data[3] << 8)));
                            //Debug.WriteLine(string.Format("AlgID: 0x{0:X08}", (uint)((uint)data[4] << 0 | (uint)data[5] << 8 | (uint)data[6] << 16 | (uint)data[7] << 24)));

                            //Debug.WriteLine("KeyData:");
                            //for (int q = 0; q < data.Length; q++)
                            //{
                            //    Debug.Write(string.Format("0x{0:X02}, ", data[q]));
                            //}
                            //Debug.WriteLine("");

                            //RSAParameters parms = rsa.ExportParameters(true);

                            //Debug.WriteLine("Modulus:");
                            //for (int q = 0; q < parms.Modulus.Length; q++)
                            //{
                            //    Debug.Write(string.Format("0x{0:X02}, ", parms.Modulus[q]));
                            //}
                            //Debug.WriteLine("");

                            //Debug.WriteLine("PubExponent:");
                            //for (int q = 0; q < parms.Exponent.Length; q++)
                            //{
                            //    Debug.Write(string.Format("0x{0:X02}, ", parms.Exponent[q]));
                            //}
                            //Debug.WriteLine("");

                            //Debug.WriteLine("PrivateExponent:");
                            //for (int q = 0; q < parms.D.Length; q++)
                            //{
                            //    Debug.Write(string.Format("0x{0:X02}, ", parms.D[q]));
                            //}
                            //Debug.WriteLine("");

                            //Debug.WriteLine("Prime1:");
                            //for (int q = 0; q < parms.P.Length; q++)
                            //{
                            //    Debug.Write(string.Format("0x{0:X02}, ", parms.P[q]));
                            //}
                            //Debug.WriteLine("");

                            //Debug.WriteLine("Prime2:");
                            //for (int q = 0; q < parms.Q.Length; q++)
                            //{
                            //    Debug.Write(string.Format("0x{0:X02}, ", parms.Q[q]));
                            //}
                            //Debug.WriteLine("");

                            //Debug.WriteLine("Exp1:");
                            //for (int q = 0; q < parms.DP.Length; q++)
                            //{
                            //    Debug.Write(string.Format("0x{0:X02}, ", parms.DP[q]));
                            //}
                            //Debug.WriteLine("");

                            //Debug.WriteLine("Exp2:");
                            //for (int q = 0; q < parms.DQ.Length; q++)
                            //{
                            //    Debug.Write(string.Format("0x{0:X02}, ", parms.DQ[q]));
                            //}
                            //Debug.WriteLine("");

                            //Debug.WriteLine("Coeff:");
                            //for (int q = 0; q < parms.InverseQ.Length; q++)
                            //{
                            //    Debug.Write(string.Format("0x{0:X02}, ", parms.InverseQ[q]));
                            //}
                            //Debug.WriteLine("");

                            hPubKey = ctx.ObjectCtx.AddObject(CryptokiObjectType.Key, new KeyData(rsa.ExportCspBlob(true), rsa));
                            hPrivKey = hPubKey;

                            bRet = true;
                        }
                        break;

                    case AlgorithmType.DSA_KEY_PAIR_GEN:
                        {
                            DSACryptoServiceProvider dsa = new DSACryptoServiceProvider(keySize);

                            hPubKey = ctx.ObjectCtx.AddObject(CryptokiObjectType.Key, new KeyData(dsa.ExportCspBlob(true), dsa));
                            hPrivKey = hPubKey;

                            /*
                            byte[] data = dsa.ExportCspBlob(false);

                            Debug.WriteLine(string.Format("bType: 0x{0:X02}", data[0]));
                            Debug.WriteLine(string.Format("bVer : 0x{0:X02}", data[1]));
                            Debug.WriteLine(string.Format("Res  : 0x{0:X04}", (ushort)((uint)data[2] << 0 | (uint)data[3] << 8)));
                            Debug.WriteLine(string.Format("AlgID: 0x{0:X08}", (uint)((uint)data[4] << 0 | (uint)data[5] << 8 | (uint)data[6] << 16 | (uint)data[7] << 24)));

                            Debug.WriteLine("Prime:");
                            for (int q = 0; q < data.Length; q++)
                            {
                                Debug.Write(string.Format("0x{0:X02}, ", data[q]));
                            }
                            Debug.WriteLine("");
                            */

                            /* 
                            DSAParameters parms = dsa.ExportParameters(true);

                            Debug.WriteLine("Prime:");
                            for (int q = 0; q < parms.P.Length; q++)
                            {
                                Debug.Write(string.Format("0x{0:X02}, ", parms.P[q]));
                            }
                            Debug.WriteLine("");

                            Debug.WriteLine("SubPrime:");
                            for (int q = 0; q < parms.Q.Length; q++)
                            {
                                Debug.Write(string.Format("0x{0:X02}, ", parms.Q[q]));
                            }
                            Debug.WriteLine("");

                            Debug.WriteLine("Base:");
                            for (int q = 0; q < parms.G.Length; q++)
                            {
                                Debug.Write(string.Format("0x{0:X02}, ", parms.G[q]));
                            }
                            Debug.WriteLine("");

                            Debug.WriteLine("Public:");
                            for (int q = 0; q < parms.Y.Length; q++)
                            {
                                Debug.Write(string.Format("0x{0:X02}, ", parms.Y[q]));
                            }
                            Debug.WriteLine("");

                            Debug.WriteLine("Private:");
                            for (int q = 0; q < parms.X.Length; q++)
                            {
                                Debug.Write(string.Format("0x{0:X02}, ", parms.X[q]));
                            }
                            Debug.WriteLine("");
                            */

                            bRet = true;
                        }
                        break;

                    case AlgorithmType.ECDSA_KEY_PAIR_GEN:
                        {
                            ECDsaCng ecdsa = new ECDsaCng(keySize);

                            hPubKey = ctx.ObjectCtx.AddObject(CryptokiObjectType.Key, new KeyData(ecdsa.Key.Export(CngKeyBlobFormat.EccPublicBlob), ecdsa));
                            hPrivKey = hPubKey;

                            bRet = true;
                        }
                        break;

                    case AlgorithmType.ECDH_KEY_PAIR_GEN:
                        {
                            ECDiffieHellmanCng ecdh = new ECDiffieHellmanCng(keySize);

                            hPubKey = ctx.ObjectCtx.AddObject(CryptokiObjectType.Key, new KeyData(ecdh.Key.Export(CngKeyBlobFormat.EccPublicBlob), ecdh));
                            hPrivKey = hPubKey;

                            bRet = true;
                        }
                        break;
                }
            }
            catch
            {
                return false;
            }

            return bRet;
        }

        bool IKeyManagementDriver.UnWrapKey(int session, int alg, int hWrappingKey, IntPtr wrappedKey, int wrappedKeyLen, out int hKey)
        {
            hKey = -1;

            return false;
        }

        bool IKeyManagementDriver.WrapKey(int session, int alg, int hWrappingKey, out IntPtr wrappedKey, out int wrappedKeyLen)
        {
            wrappedKey = IntPtr.Zero;
            wrappedKeyLen = 0;

            return false;
        }

        bool IKeyManagementDriver.LoadKeyBlob(int session, IntPtr pKey, int keyLen, KeyType keyType, KeyAttribute keyAttrib, out int hKey)
        {
            bool bRet = false;

            hKey = -1;

            try
            {
                SessionData ctx = ((SessionDriver)this.Hal.Session).GetSessionCtx(session);
                CryptokiObjectMgrDriver objMgr = (CryptokiObjectMgrDriver)Hal.CryptokiObjectMgr;

                byte[] keyData = new byte[keyLen];

                Marshal.Copy(pKey, keyData, 0, keyLen);

                if (keyAttrib == KeyAttribute.Secret)
                {
                    SecretKey key = new SecretKey(keyLen * 8, keyData);

                    hKey = ctx.ObjectCtx.AddObject(CryptokiObjectType.Key, new KeyData(keyData, key));

                    bRet = true;
                }
                else
                {
                    switch(keyType)
                    {
                        case KeyType.RSA:
                            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();

                            rsa.ImportCspBlob(keyData);

                            hKey = ctx.ObjectCtx.AddObject(CryptokiObjectType.Key, new KeyData(rsa.ExportCspBlob(0 != (keyAttrib & KeyAttribute.Private)), rsa));

                            bRet = true;
                            break;

                        case KeyType.DSA:
                            DSACryptoServiceProvider dsa = new DSACryptoServiceProvider();
                            dsa.ImportCspBlob(keyData);

                            hKey = ctx.ObjectCtx.AddObject(CryptokiObjectType.Key, new KeyData(dsa.ExportCspBlob(0 != (keyAttrib & KeyAttribute.Private)), dsa));

                            bRet = true;
                            break;

                        case KeyType.ECDSA:
                            {
                                CngKeyBlobFormat fmt = (0 == (keyAttrib & KeyAttribute.Private)) ? CngKeyBlobFormat.EccPublicBlob : CngKeyBlobFormat.EccPrivateBlob;

                                CngKey key = CngKey.Import(keyData, fmt);
                                ECDsaCng ec = new ECDsaCng(key);

                                hKey = ctx.ObjectCtx.AddObject(CryptokiObjectType.Key, new KeyData(ec.Key.Export(fmt), ec));
                                bRet = true;
                            }
                            break;

                        case KeyType.DH:
                            {
                                CngKeyBlobFormat fmt = (0 == (keyAttrib & KeyAttribute.Private)) ? CngKeyBlobFormat.EccPublicBlob : CngKeyBlobFormat.EccPrivateBlob;
                                CngKey key = CngKey.Import(keyData, fmt);

                                ECDiffieHellmanCng ecdh = new ECDiffieHellmanCng(key);

                                hKey = ctx.ObjectCtx.AddObject(CryptokiObjectType.Key, new KeyData(ecdh.Key.Export(fmt), ecdh));
                                bRet = true;
                            }
                            break;
                    }
                }
            }
            catch
            {
                return false;
            }

            return bRet;
        }

        #endregion
    }
}
