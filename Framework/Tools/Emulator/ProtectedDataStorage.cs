////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Text;

namespace Microsoft.SPOT.Emulator.PKCS11
{
    internal class CryptokiObjectMgrDriver : HalDriver<ICryptokiObjectDriver>, ICryptokiObjectDriver
    {
        [StructLayout(LayoutKind.Sequential)]
        internal class CryptokiAttribute
        {
            internal uint   type;
            internal IntPtr pValue;
            internal uint   ulValueLen;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct DATE_TIME_INFO
        {
            public int year;           /* year, AD                   */
            public int month;          /* 1 = January, 12 = December */
            public int day;            /* 1 = first of the month     */
            public int hour;           /* 0 = midnight, 12 = noon    */
            public int minute;         /* minutes past the hour      */
            public int second;         /* seconds in minute          */
            public int msec;           /* milliseconds in second     */

            public int dlsTime;        /* boolean; daylight savings time is in effect                      */
            public int tzOffset;       /* signed int; difference in seconds imposed by timezone (from GMT) */

            internal DATE_TIME_INFO(byte[] data)
            {
                if (data.Length < 36) throw new ArgumentException();

                year = Marshal.ReadInt32(data, 0);
                month = Marshal.ReadInt32(data, 4);
                day = Marshal.ReadInt32(data, 8);
                hour = Marshal.ReadInt32(data, 12);
                minute = Marshal.ReadInt32(data, 16);
                second = Marshal.ReadInt32(data, 20);
                msec = Marshal.ReadInt32(data, 24);
                dlsTime = Marshal.ReadInt32(data, 28);
                tzOffset = Marshal.ReadInt32(data, 32);
            }
        }

        private RSACryptoServiceProvider m_deviceKey = new RSACryptoServiceProvider(1024);

        #region ICryptokiObjectDriver Members

        public bool CopyObject(int session, int hObject, IntPtr pTemplate, int ulCount, out int phNewObject)
        {
            phNewObject = -1;
            return false;
        }

        public bool CreateObject(int session, IntPtr pTemplate, int ulCount, out int phObject)
        {
            phObject = -1;

            if (ulCount == 0) return false;

            try
            {
                SessionData ctx = ((SessionDriver)this.Hal.Session).GetSessionCtx(session);

                CryptokiAttribute attrib = new CryptokiAttribute();
                Marshal.PtrToStructure(pTemplate, attrib);

                if ((CryptokiAttribType)attrib.type == CryptokiAttribType.Class)
                {
                    CryptokiClass type = (CryptokiClass)Marshal.ReadInt32(attrib.pValue);

                    if (type == CryptokiClass.CERTIFICATE)
                    {
                        string password = "";

                        while (0 < --ulCount)
                        {
                            pTemplate += Marshal.SizeOf(attrib);

                            Marshal.PtrToStructure(pTemplate, attrib);

                            switch ((CryptokiAttribType)attrib.type)
                            {
                                case CryptokiAttribType.Password:
                                    {
                                        byte[] data = new byte[attrib.ulValueLen];

                                        Marshal.Copy(attrib.pValue, data, 0, data.Length);

                                        password = UTF8Encoding.UTF8.GetString(data);
                                    }
                                    break;

                                case CryptokiAttribType.Value:
                                    {
                                        byte[] data = new byte[attrib.ulValueLen];

                                        Marshal.Copy(attrib.pValue, data, 0, data.Length);

                                        X509Certificate2 x509 = new X509Certificate2(data, password);
                                        phObject = ctx.ObjectCtx.AddObject(CryptokiObjectType.Cert, x509);

                                        return true;
                                    }

                                default:
                                    break;
                            }
                        }
                    }
                }
            }
            catch
            {
            }

            return false;
        }

        public bool DestroyObject(int session, int hObject)
        {
            try
            {
                SessionData ctx = ((SessionDriver)this.Hal.Session).GetSessionCtx(session);

                ctx.ObjectCtx.DestroyObject(hObject);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool FindObjects(int session, IntPtr phObjects, int ulMaxCount, out int pulObjectCount)
        {
            pulObjectCount = 0;

            try
            {
                SessionData ctx = ((SessionDriver)this.Hal.Session).GetSessionCtx(session);
                FindObjectsContext foc = ctx.FindObjCtx;

                if (foc == null) return false;

                int[] handles = new int[ctx.ObjectCtx.m_objects.Count];
                int idx = 0;

                Dictionary<int, CryptokiObject>.Enumerator enm = ctx.ObjectCtx.m_objects.GetEnumerator();

                while (enm.MoveNext())
                {
                    CryptokiObject obj = enm.Current.Value;
                    if (foc.Type == obj.Type)
                    {
                        if ((string)obj.Properties["Group"] == foc.Group)
                        {
                            if ((string)obj.Properties["FileName"] == foc.FileName || foc.FileName == "")
                            {
                                handles[idx++] = enm.Current.Key;
                            }
                        }
                    }
                }

                if (idx == 0 && foc.FileName == "NetMF_DeviceKey")
                {
                    handles[idx++] = ctx.ObjectCtx.AddObject(CryptokiObjectType.Key, m_deviceKey);
                }

                pulObjectCount = idx;

                if (idx > 0 && phObjects != IntPtr.Zero)
                {
                    Marshal.Copy(handles, 0, phObjects, idx);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool FindObjectsFinal(int session)
        {
            try
            {
                SessionData ctx = ((SessionDriver)this.Hal.Session).GetSessionCtx(session);

                if (ctx.FindObjCtx == null) return false;

                ctx.FindObjCtx = null;

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool FindObjectsInit(int session, IntPtr pTemplate, int ulCount)
        {
            try
            {
                SessionData ctx = ((SessionDriver)this.Hal.Session).GetSessionCtx(session);

                if (ctx.FindObjCtx != null) return false;

                ctx.FindObjCtx = new FindObjectsContext();
                string fileName = "";
                string group = "";
                CryptokiObjectType type = CryptokiObjectType.Data;
                byte[] data;

                while (ulCount-- > 0)
                {
                    CryptokiAttribute attrib = new CryptokiAttribute();
                    Marshal.PtrToStructure(pTemplate, attrib);

                    switch ((CryptokiAttribType)attrib.type)
                    {
                        case CryptokiAttribType.Class:

                            switch((CryptokiClass)Marshal.ReadInt32(attrib.pValue))
                            {
                                case CryptokiClass.CERTIFICATE:
                                    type = CryptokiObjectType.Cert;
                                    break;

                                case CryptokiClass.OTP_KEY:
                                    type = CryptokiObjectType.Key;
                                    break;
                            }
                            break;

                        case CryptokiAttribType.ObjectID:
                            data = new byte[attrib.ulValueLen];
                            Marshal.Copy(attrib.pValue, data, 0, data.Length);
                            fileName = UTF8Encoding.UTF8.GetString(data);
                            break;

                        case CryptokiAttribType.Label:
                            data = new byte[attrib.ulValueLen];
                            Marshal.Copy(attrib.pValue, data, 0, data.Length);
                            group = UTF8Encoding.UTF8.GetString(data);
                            break;
                    }

                    pTemplate += Marshal.SizeOf(attrib);
                }

                ctx.FindObjCtx.FileName = fileName;
                ctx.FindObjCtx.Group = group;
                ctx.FindObjCtx.Type = type;

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool GetAttributeValue(int session, int hObject, IntPtr pTemplate, int ulCount)
        {
            try
            {
                SessionData ctx = ((SessionDriver)this.Hal.Session).GetSessionCtx(session);
                CryptokiObject obj = ctx.ObjectCtx.GetObject(hObject);

                if (obj == null) return false;

                DSAParameters dsaParms = new DSAParameters();
                bool hasDsaParms = false;
                int valueLen = 0;
                RSAParameters rsaParms = new RSAParameters();

                while (ulCount-- > 0)
                {
                    CryptokiAttribute attrib = new CryptokiAttribute();
                    Marshal.PtrToStructure(pTemplate, attrib);

                    if (obj.Type == CryptokiObjectType.Cert)
                    {
                        X509Certificate2 cert = (X509Certificate2)obj.Data;
                        switch ((CryptokiAttribType)attrib.type)
                        {
                            case CryptokiAttribType.Class:
                                Marshal.WriteInt32(attrib.pValue, (int)CryptokiClass.CERTIFICATE);
                                valueLen = 4;
                                break;

                            case CryptokiAttribType.Private:
                                Marshal.WriteInt32(attrib.pValue, cert.HasPrivateKey ? 1 : 0);
                                valueLen = 4;
                                break;

                            case CryptokiAttribType.KeyType:
                                switch(cert.GetKeyAlgorithm())
                                {
                                    case "1.2.840.113549.1.1.1":
                                        Marshal.WriteInt32(attrib.pValue, (int)KeyType.RSA);
                                        break;

                                    default:
                                        Marshal.WriteInt32(attrib.pValue, -1);
                                        break;
                                }
                                valueLen = 4;
                                break;

                            case CryptokiAttribType.Issuer:
                                {
                                    byte []data = UTF8Encoding.UTF8.GetBytes(cert.Issuer);
                                    Marshal.Copy(data, 0, attrib.pValue, data.Length);
                                    valueLen = data.Length;
                                }
                                break;

                            case CryptokiAttribType.Subject:
                                {
                                    byte []data = UTF8Encoding.UTF8.GetBytes(cert.Subject);
                                    Marshal.Copy(data, 0, attrib.pValue, data.Length);
                                    valueLen = data.Length;
                                }
                                break;

                            case CryptokiAttribType.SerialNumber:
                                {
                                    byte[] data = cert.GetSerialNumber();
                                    Marshal.Copy(data, 0, attrib.pValue, data.Length);
                                    valueLen = data.Length;
                                }
                                break;

                            case CryptokiAttribType.PublicExponent:
                                {
                                }
                                break;

                            case CryptokiAttribType.StartDate:
                            case CryptokiAttribType.EndDate:
                                {
                                    DATE_TIME_INFO dti = new DATE_TIME_INFO();
                                    DateTime dt = (CryptokiAttribType)attrib.type == CryptokiAttribType.StartDate ? cert.NotBefore : cert.NotAfter;

                                    dti.year = dt.Year;
                                    dti.month = dt.Month;
                                    dti.day = dt.Day;
                                    dti.hour = dt.Hour;
                                    dti.minute = dt.Minute;
                                    dti.second = dt.Second;
                                    dti.msec = dt.Millisecond;
                                    dti.tzOffset = TimeZoneInfo.Local.BaseUtcOffset.Hours;

                                    Marshal.StructureToPtr(dti, attrib.pValue, true);
                                    valueLen = Marshal.SizeOf(dti);
                                }
                                break;

                            case CryptokiAttribType.MechanismType:
                                {
                                    switch (cert.SignatureAlgorithm.Value)
                                    {
                                        case "1.2.840.113549.1.1.5":
                                            Marshal.WriteInt32(attrib.pValue, (int)AlgorithmType.SHA1_RSA_PKCS);
                                            break;

                                        case "1.2.840.113549.1.1.4":
                                            Marshal.WriteInt32(attrib.pValue, (int)AlgorithmType.MD5_RSA_PKCS);
                                            break;

                                        case "1.2.840.113549.1.1.11":
                                            Marshal.WriteInt32(attrib.pValue, (int)AlgorithmType.SHA256_RSA_PKCS);
                                            break;

                                        case "1.2.840.113549.1.1.12":
                                            Marshal.WriteInt32(attrib.pValue, (int)AlgorithmType.SHA384_RSA_PKCS);
                                            break;

                                        case "1.2.840.113549.1.1.13":
                                            Marshal.WriteInt32(attrib.pValue, (int)AlgorithmType.SHA512_RSA_PKCS);
                                            break;
                                    }
                                    valueLen = 4;
                                }
                                break;

                            case CryptokiAttribType.Value:
                                valueLen = cert.RawData.Length;
                                Marshal.Copy(cert.RawData, 0, attrib.pValue, valueLen);
                                break;

                            case CryptokiAttribType.ValueLen:
                                Marshal.WriteInt32(attrib.pValue, valueLen);
                                break;
                        }
                    }
                    else if (obj.Type == CryptokiObjectType.Key)
                    {
                        object key = ((KeyData)obj.Data).KeyCsp;

                        switch ((CryptokiAttribType)attrib.type)
                        {
                            case CryptokiAttribType.Class:
                                Marshal.WriteInt32(attrib.pValue, (int)CryptokiClass.OTP_KEY);
                                valueLen = 4;
                                break;

                            case CryptokiAttribType.PrivateExponent:
                                if (key is DSACryptoServiceProvider)
                                {
                                    if (!hasDsaParms) dsaParms = ((DSACryptoServiceProvider)key).ExportParameters(true);

                                    valueLen = Math.Min((int)attrib.ulValueLen, dsaParms.X.Length);

                                    Marshal.Copy(dsaParms.X, 0, attrib.pValue, valueLen);
                                }
                                else if (key is AesCryptoServiceProvider)
                                {
                                    AesCryptoServiceProvider aes = (AesCryptoServiceProvider)key;

                                    valueLen = Math.Min((int)attrib.ulValueLen, aes.Key.Length);

                                    Marshal.Copy(aes.Key, 0, attrib.pValue, valueLen);
                                }
                                else if (key is KeyManagementDriver.SecretKey)
                                {
                                    KeyManagementDriver.SecretKey sk = (KeyManagementDriver.SecretKey)key;

                                    valueLen = Math.Min((int)attrib.ulValueLen, sk.Data.Length);

                                    Marshal.Copy(sk.Data, 0, attrib.pValue, valueLen);
                                }
                                else if (key is RSACryptoServiceProvider)
                                {
                                    RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)key;

                                    if (rsaParms.Modulus == null) rsaParms = rsa.ExportParameters(true);

                                    valueLen = Math.Min((int)attrib.ulValueLen, rsaParms.D.Length);

                                    Marshal.Copy(rsaParms.D, 0, attrib.pValue, valueLen);
                                }
                                break;
                            case CryptokiAttribType.PublicExponent:
                                if (key is DSACryptoServiceProvider)
                                {
                                    if (!hasDsaParms) dsaParms = ((DSACryptoServiceProvider)key).ExportParameters(true);

                                    valueLen = Math.Min((int)attrib.ulValueLen, dsaParms.Y.Length);

                                    Marshal.Copy(dsaParms.Y, 0, attrib.pValue, valueLen);
                                }
                                else if (key is ECDiffieHellman)
                                {
                                    ECDiffieHellman ecdh = (ECDiffieHellman)key;

                                    byte[] pubKey = ecdh.PublicKey.ToByteArray();

                                    valueLen = Math.Min((int)attrib.ulValueLen, pubKey.Length - 8);

                                    Marshal.Copy(pubKey, 8, attrib.pValue, valueLen);
                                }
                                else if (key is RSACryptoServiceProvider)
                                {
                                    RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)key;

                                    if (rsaParms.Modulus == null) rsaParms = rsa.ExportParameters(true);

                                    valueLen = Math.Min((int)attrib.ulValueLen, rsaParms.Exponent.Length);

                                    Marshal.Copy(rsaParms.Exponent, 0, attrib.pValue, valueLen);
                                }
                                break;
                            case CryptokiAttribType.Prime:
                                if (key is DSACryptoServiceProvider)
                                {
                                    if (!hasDsaParms) dsaParms = ((DSACryptoServiceProvider)key).ExportParameters(true);

                                    valueLen = Math.Min((int)attrib.ulValueLen, dsaParms.P.Length);

                                    Marshal.Copy(dsaParms.P, 0, attrib.pValue, valueLen);
                                }
                                break;
                            case CryptokiAttribType.Subprime:
                                if (key is DSACryptoServiceProvider)
                                {
                                    if (!hasDsaParms) dsaParms = ((DSACryptoServiceProvider)key).ExportParameters(true);

                                    valueLen = Math.Min((int)attrib.ulValueLen, dsaParms.Q.Length);

                                    Marshal.Copy(dsaParms.Q, 0, attrib.pValue, valueLen);
                                }
                                break;
                            case CryptokiAttribType.Base:
                                if (key is DSACryptoServiceProvider)
                                {
                                    if (!hasDsaParms) dsaParms = ((DSACryptoServiceProvider)key).ExportParameters(true);

                                    valueLen = Math.Min((int)attrib.ulValueLen, dsaParms.G.Length);

                                    Marshal.Copy(dsaParms.G, 0, attrib.pValue, valueLen);
                                }
                                break;

                            case CryptokiAttribType.ValueLen:
                                Marshal.WriteInt32(attrib.pValue, valueLen);
                                break;

                            case CryptokiAttribType.KeyType:
                                if (key is DSACryptoServiceProvider)
                                {
                                    Marshal.WriteInt32(attrib.pValue, (int)KeyType.DSA);
                                }
                                else if (key is RSACryptoServiceProvider)
                                {
                                    Marshal.WriteInt32(attrib.pValue, (int)KeyType.RSA);
                                }
                                else if (key is ECDsaCng)
                                {
                                    Marshal.WriteInt32(attrib.pValue, (int)KeyType.ECDSA);
                                }
                                else if (key is ECDiffieHellman)
                                {
                                    Marshal.WriteInt32(attrib.pValue, (int)KeyType.EC);
                                }
                                else if (key is AesCryptoServiceProvider)
                                {
                                    Marshal.WriteInt32(attrib.pValue, (int)KeyType.AES);
                                }
                                else if (key is TripleDESCryptoServiceProvider)
                                {
                                    Marshal.WriteInt32(attrib.pValue, (int)KeyType.DES3);
                                }
                                else if (key is KeyManagementDriver.SecretKey)
                                {
                                    Marshal.WriteInt32(attrib.pValue, (int)KeyType.GENERIC_SECRET);
                                }
                                break;

                            case CryptokiAttribType.ValueBits:
                                if (key is DSACryptoServiceProvider)
                                {
                                    Marshal.WriteInt32(attrib.pValue, ((DSACryptoServiceProvider)key).KeySize);
                                }
                                else if (key is RSACryptoServiceProvider)
                                {
                                    Marshal.WriteInt32(attrib.pValue, ((RSACryptoServiceProvider)key).KeySize);
                                }
                                else if (key is ECDsaCng)
                                {
                                    Marshal.WriteInt32(attrib.pValue, ((ECDsaCng)key).KeySize);
                                }
                                else if (key is ECDiffieHellman)
                                {
                                    Marshal.WriteInt32(attrib.pValue, ((ECDiffieHellman)key).KeySize);
                                }
                                else if (key is AesCryptoServiceProvider)
                                {
                                    Marshal.WriteInt32(attrib.pValue, ((AesCryptoServiceProvider)key).KeySize);
                                }
                                else if (key is TripleDESCryptoServiceProvider)
                                {
                                    Marshal.WriteInt32(attrib.pValue, ((TripleDESCryptoServiceProvider)key).KeySize);
                                }
                                else if (key is KeyManagementDriver.SecretKey)
                                {
                                    Marshal.WriteInt32(attrib.pValue, ((KeyManagementDriver.SecretKey)key).Size);
                                }
                                break;

                            case CryptokiAttribType.Value:
                                if (key is AesCryptoServiceProvider)
                                {
                                    AesCryptoServiceProvider aes = (AesCryptoServiceProvider)key;

                                    valueLen = Math.Min((int)attrib.ulValueLen, aes.Key.Length);

                                    Marshal.Copy(aes.Key, 0, attrib.pValue, valueLen);
                                }
                                else if (key is KeyManagementDriver.SecretKey)
                                {
                                    KeyManagementDriver.SecretKey sk = (KeyManagementDriver.SecretKey)key;

                                    valueLen = Math.Min((int)attrib.ulValueLen, sk.Data.Length);

                                    Marshal.Copy(sk.Data, 0, attrib.pValue, valueLen);
                                }
                                else if (key is ECDiffieHellman)
                                {
                                    ECDiffieHellman ecdh = (ECDiffieHellman)key;

                                    byte[] pubKey = ecdh.PublicKey.ToByteArray();

                                    valueLen = Math.Min((int)attrib.ulValueLen, pubKey.Length);

                                    Marshal.Copy(pubKey, 0, attrib.pValue, valueLen);
                                }
                                break;

                            case CryptokiAttribType.Modulus:
                                if (key is RSACryptoServiceProvider)
                                {
                                    RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)key;

                                    if (rsaParms.Modulus == null) rsaParms = rsa.ExportParameters(true);

                                    valueLen = Math.Min((int)attrib.ulValueLen, rsaParms.Modulus.Length);

                                    Marshal.Copy(rsaParms.Modulus, 0, attrib.pValue, valueLen);
                                }
                                break;

                            case CryptokiAttribType.Prime1:
                                if (key is RSACryptoServiceProvider)
                                {
                                    RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)key;

                                    if (rsaParms.Modulus == null) rsaParms = rsa.ExportParameters(true);

                                    valueLen = Math.Min((int)attrib.ulValueLen, rsaParms.P.Length);

                                    Marshal.Copy(rsaParms.P, 0, attrib.pValue, valueLen);
                                }
                                break;
                            case CryptokiAttribType.Prime2:
                                if (key is RSACryptoServiceProvider)
                                {
                                    RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)key;

                                    if (rsaParms.Modulus == null) rsaParms = rsa.ExportParameters(true);

                                    valueLen = Math.Min((int)attrib.ulValueLen, rsaParms.Q.Length);

                                    Marshal.Copy(rsaParms.Q, 0, attrib.pValue, valueLen);
                                }
                                break;
                            case CryptokiAttribType.Exponent1:
                                if (key is RSACryptoServiceProvider)
                                {
                                    RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)key;

                                    if (rsaParms.Modulus == null) rsaParms = rsa.ExportParameters(true);

                                    valueLen = Math.Min((int)attrib.ulValueLen, rsaParms.DP.Length);

                                    Marshal.Copy(rsaParms.DP, 0, attrib.pValue, valueLen);
                                }
                                break;
                            case CryptokiAttribType.Exponent2:
                                if (key is RSACryptoServiceProvider)
                                {
                                    RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)key;

                                    if (rsaParms.Modulus == null) rsaParms = rsa.ExportParameters(true);

                                    valueLen = Math.Min((int)attrib.ulValueLen, rsaParms.DQ.Length);

                                    Marshal.Copy(rsaParms.DQ, 0, attrib.pValue, valueLen);
                                }
                                break;
                            case CryptokiAttribType.Coefficent:
                                if (key is RSACryptoServiceProvider)
                                {
                                    RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)key;

                                    if (rsaParms.Modulus == null) rsaParms = rsa.ExportParameters(true);

                                    valueLen = Math.Min((int)attrib.ulValueLen, rsaParms.InverseQ.Length);

                                    Marshal.Copy(rsaParms.InverseQ, 0, attrib.pValue, valueLen);
                                }
                                break;
                        }
                    }

                    pTemplate += Marshal.SizeOf(attrib);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public bool GetObjectSize(int session, int hObject, out int pulSize)
        {
            pulSize = 0;
            return false;
        }

        public bool SetAttributeValue(int session, int hObject, IntPtr pTemplate, int ulCount)
        {
            try
            {
                SessionData ctx = ((SessionDriver)this.Hal.Session).GetSessionCtx(session);
                CryptokiObject obj = ctx.ObjectCtx.GetObject(hObject);

                if (obj == null) return false;

                if (obj.Type == CryptokiObjectType.Cert)
                {
                    X509Certificate2 cert = obj.Data as X509Certificate2;
                    bool fSave = false;
                    bool fDelete = false;
                    string group = "", fileName = "";
                    uint len = 0;
                    byte[] data;

                    for (int i = 0; i < (int)ulCount; i++)
                    {
                        CryptokiAttribute attrib = new CryptokiAttribute();
                        Marshal.PtrToStructure(pTemplate, attrib);

                        switch ((CryptokiAttribType)attrib.type)
                        {
                            case CryptokiAttribType.Persist:
                                fSave = Marshal.ReadInt32(attrib.pValue) == 1;
                                fDelete = !fSave;
                                break;

                            case CryptokiAttribType.Label:
                                len = attrib.ulValueLen;

                                data = new byte[len];

                                Marshal.Copy(attrib.pValue, data, 0, (int)len);

                                group = UTF8Encoding.UTF8.GetString(data);

                                break;

                            case CryptokiAttribType.ObjectID:
                                len = attrib.ulValueLen;

                                data = new byte[len];

                                Marshal.Copy(attrib.pValue, data, 0, (int)len);

                                fileName = UTF8Encoding.UTF8.GetString(data);

                                break;

                            default:
                                return false;
                        }

                        pTemplate += Marshal.SizeOf(attrib);
                    }

                    if (fDelete)
                    {
                        ctx.ObjectCtx.DestroyObject(hObject);
                    }
                    else if (fSave)
                    {
                        // TODO: Store in persistant storage for emulator

                        obj.Properties["FileName"] = fileName;
                        obj.Properties["Group"] = group;
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        private const uint c_ArrayAttribute = 0x40000000;

        [Flags]
        public enum CryptokiClass : uint
        {
            DATA = 0,
            CERTIFICATE,
            PUBLIC_KEY,
            PRIVATE_KEY,
            SECRET_KEY,
            HW_FEATURE,
            DOMAIN_PARAMETERS,
            MECHANISM,
            OTP_KEY,

            VENDOR_DEFINED = 0x80000000,
        }

        public enum CryptokiAttribType : uint
        {
            Class = 0x00000000,
            Token = 0x00000001,
            Private = 0x00000002,
            Label = 0x00000003,
            Application = 0x00000010,
            Value = 0x00000011,
            ObjectID = 0x00000012,
            CertificateType = 0x00000080,
            Issuer = 0x00000081,
            SerialNumber = 0x00000082,
            AC_Issuer = 0x00000083,
            Owner = 0x00000084,
            AttrTypes = 0x00000085,
            Trusted = 0x00000086,
            CertficateCategory = 0x00000087,
            JavaMidpSecurityDomain = 0x00000088,
            URL = 0x00000089,
            HashOfSubjectPublicKey = 0x0000008A,
            HashOfIssuerPublicKey = 0x0000008B,
            NameHashAlgorithm = 0x0000008C,
            CheckValue = 0x00000090,
            KeyType = 0x00000100,
            Subject = 0x00000101,
            ID = 0x00000102,
            Sensitive = 0x00000103,
            Encrypt = 0x00000104,
            Decrypt = 0x00000105,
            Wrap = 0x00000106,
            Unwrap = 0x00000107,
            Sign = 0x00000108,
            SignRecover = 0x00000109,
            Verify = 0x0000010A,
            VerifyRecover = 0x0000010B,
            Derive = 0x0000010C,
            StartDate = 0x00000110,
            EndDate = 0x00000111,
            Modulus = 0x00000120,
            ModulusBits = 0x00000121,
            PublicExponent = 0x00000122,
            PrivateExponent = 0x00000123,
            Prime1 = 0x00000124,
            Prime2 = 0x00000125,
            Exponent1 = 0x00000126,
            Exponent2 = 0x00000127,
            Coefficent = 0x00000128,
            Prime = 0x00000130,
            Subprime = 0x00000131,
            Base = 0x00000132,
            PrimeBits = 0x00000133,
            SubprimeBits = 0x00000134,
            ValueBits = 0x00000160,
            ValueLen = 0x00000161,
            Extractable = 0x00000162,
            Local = 0x00000163,
            NeverExtractable = 0x00000164,
            AlwaysSensitive = 0x00000165,
            KeyGenMecahnism = 0x00000166,
            Modifiable = 0x00000170,
            Copyable = 0x00000171,
            EcdsaParams = 0x00000180,
            EcParams = 0x00000180,
            EcPoint = 0x00000181,
            SecondaryAuth = 0x00000200, /* Deprecated */
            AuthPinFlags = 0x00000201, /* Deprecated */
            AlwaysAuthenticate = 0x00000202,
            WrapWithTrusted = 0x00000210,
            WrapTemplate = (c_ArrayAttribute | 0x00000211),
            UnwrapTemplate = (c_ArrayAttribute | 0x00000212),
            HardwareFeatureType = 0x00000300,
            ResetOnInit = 0x00000301,
            HasReset = 0x00000302,
            PixelX = 0x00000400,
            PixelY = 0x00000401,
            Resolution = 0x00000402,
            CharRows = 0x00000403,
            CharColumns = 0x00000404,
            Color = 0x00000405,
            BitsPerPixel = 0x00000406,
            CharSets = 0x00000480,
            EncodingMethods = 0x00000481,
            MimeTypes = 0x00000482,
            MechanismType = 0x00000500,
            RequiredCmsAttributes = 0x00000501,
            DefaultCmsAttributes = 0x00000502,
            SupportedCmsAttributes = 0x00000503,
            AllowedMechanisms = (c_ArrayAttribute | 0x00000600),
            SIGN_NO_NODIGEST_FLAG = 0x40000000,
            VendorDefined = 0x80000000,
            Persist = (VendorDefined | 0x00000001),
            Password = (VendorDefined | 0x00000002),
        }
    }
}