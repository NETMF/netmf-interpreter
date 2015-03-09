////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace Microsoft.SPOT.Emulator.PKCS11
{
    [Flags]
    public enum KeyAttribute
    {
        Private = 1,
        Public  = 2,
        Secret  = 4,
        PrivatePublic = Private | Public,
    }

    public interface IKeyManagementDriver
    {
        int  GetKeySize       (int session, int hKey);
        KeyType GetKeyType(int session, int hKey);
        bool LoadSymmetricKey (int session, IntPtr secret, int secretLen, KeyType keyType, out int hKey);
        bool LoadAsymmetricKey(int session, IntPtr keyData, int keyLen, KeyType keyType, out int hKey);
        bool GenerateKey      (int session, int alg, int keyLength, out int hKey);
        bool GenerateKeyPair  (int session, int alg, int keyLength, out int hPubKey, out int hPrivKey);
        bool DeriveKey        (int session, int alg, IntPtr pParam, int paramLen, int hBaseKey, out int hKey);
        bool WrapKey          (int session, int alg, int hWrappingKey, out IntPtr wrappedKey, out int wrappedKeyLen);
        bool UnWrapKey        (int session, int alg, int hWrappingKey, IntPtr wrappedKey, int wrappedKeyLen, out int hKey);
        bool GetPublicKeyData (int session, int hKey, IntPtr data, ref int dataLen);
        bool LoadKeyBlob      (int session, IntPtr pKey, int keyLen, KeyType keyType, KeyAttribute keyAttrib, out int hKey);
    }
}

