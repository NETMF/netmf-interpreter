////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Threading;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.SPOT.Emulator.PKCS11
{
    internal class EncryptionData
    {
        internal AlgorithmType    CryptoAlgorithm;
        internal IDisposable      CryptoObject    = null;
        internal ICryptoTransform CryptoTransform = null;

        internal void Clear()
        {
            CryptoObject    = null;
            CryptoTransform = null;
        }
    }

    internal class DigestData
    {
        internal HashAlgorithm Digest = null;
        internal bool isPartialUpdate = false;

        internal void Clear()
        {
            Digest = null;
            isPartialUpdate = false;
        }
    }

    internal class KeyData
    {
        internal KeyData(byte[] key, object csp)
        {
            KeyBytes = key;
            KeyCsp = csp;
        }

        internal byte[] KeyBytes;
        internal object KeyCsp;
    }

    internal enum CryptokiObjectType
    {
        Key,
        Data,
        Cert,
    }

    internal class CryptokiObject
    {
        internal CryptokiObject(CryptokiObjectType type, object data)
        {
            Type = type;
            Data = data;
        }
        internal CryptokiObjectType Type;
        internal object Data;
        internal Dictionary<string, object> Properties = new Dictionary<string, object>();
    }


    internal class SignatureData
    {
        internal HashAlgorithm SignHashAlg = null;
        internal object        SignObject  = null;

        internal void Clear()
        {
            if (SignHashAlg != null)
            {
                SignHashAlg.Dispose();
                SignHashAlg = null;
            }
            SignObject = null;
        }
    }

    internal class ObjectContext
    {
        internal Dictionary<int, CryptokiObject> m_objects = new Dictionary<int, CryptokiObject>();

        internal int AddObject(CryptokiObjectType type, object data)
        {
            int handle = Interlocked.Increment(ref s_nextHandle);

            m_objects[handle] = new CryptokiObject(type, data);

            return handle;
        }

        internal CryptokiObject GetObject(int handle)
        {
            if (!m_objects.ContainsKey(handle)) return null;

            return m_objects[handle];
        }

        internal void DestroyObject(int handle)
        {
            if (!m_objects.ContainsKey(handle)) return;

            object o = m_objects[handle];

            m_objects.Remove(handle);

            if (o is KeyData)
            {
                IDisposable disp = ((KeyData)o).KeyCsp as IDisposable;

                if (disp != null)
                {
                    disp.Dispose();
                }
            }
        }

        internal void Clear()
        {
            m_objects.Clear();
        }

        internal static int s_nextHandle = 1;
    }

    internal class FindObjectsContext 
    {
        internal CryptokiObjectType Type;
        internal string FileName;
        internal string Group;
    }

    internal class SessionData
    {
        internal EncryptionData EncryptCtx    = new EncryptionData();
        internal EncryptionData DecryptCtx    = new EncryptionData();
        internal DigestData     DigestCtx     = new DigestData();
        internal SignatureData  SignCtx       = new SignatureData();
        internal SignatureData  VerifyCtx     = new SignatureData();
        internal ObjectContext  ObjectCtx     = new ObjectContext();
        internal FindObjectsContext FindObjCtx = null;
        internal bool           ReadWriteMode = false;
    }

    internal class SessionDriver : HalDriver<ISessionDriver>, ISessionDriver
    {
        Dictionary<int, SessionData> m_sessionMap = new Dictionary<int,SessionData>();
        int s_NextSessionHandle = 0;

        internal SessionData GetSessionCtx(int session)
        {
            if(!m_sessionMap.ContainsKey(session)) return null;

            return m_sessionMap[session];
        }

        #region ISessionDriver Members

        bool ISessionDriver.CloseSession(int session)
        {
            try
            {
                if (m_sessionMap.ContainsKey(session))
                {
                    m_sessionMap.Remove(session);

                    return true;
                }
            }
            catch
            {
            }

            return false;
        }

        bool ISessionDriver.Login(int session, int userType, string pin)
        {
            return false;
        }

        bool ISessionDriver.Logout(int session)
        {
            return false;
        }

        bool ISessionDriver.OpenSession(bool fReadWrite, out int session)
        {
            session = -1;

            try
            {
                SessionData ctx = new SessionData();

                ctx.ReadWriteMode = fReadWrite;

                session = s_NextSessionHandle++;

                m_sessionMap[session] = ctx;
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