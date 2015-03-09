////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace Microsoft.SPOT.Emulator.PKCS11
{
    public interface IEncryptionDriver
    {        
        bool EncryptInit  (int session, int    alg, IntPtr algParam, int algParamLen, int hKey);
        bool Encrypt      (int session, IntPtr Data, int DataLen, IntPtr EncData, ref int EncDataLen);
        bool EncryptUpdate(int session, IntPtr Part, int PartLen, IntPtr EncData, ref int EncDataLen);
        bool EncryptFinal (int session, IntPtr EncData, ref int EncDataLen);

        bool DecryptInit  (int session, int    alg, IntPtr algParam, int algParamLen, int hKey);
        bool Decrypt      (int session, IntPtr EncData, int EncDataLen, IntPtr Data, ref int DataLen);
        bool DecryptUpdate(int session, IntPtr EncData, int EncDataLen, IntPtr Data, ref int DataLen);
        bool DecryptFinal (int session, IntPtr Data, ref int DataLen);
    }
}

