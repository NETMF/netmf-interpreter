////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace Microsoft.SPOT.Emulator.PKCS11
{
    public interface ISignatureDriver
    {
        bool SignInit    (int session, int alg, int hashAlg, int hKey);
        bool Sign        (int session, IntPtr Data, int DataLen, IntPtr Signature, ref int SignatureLen);
        bool SignUpdate  (int session, IntPtr Data, int DataLen);
        bool SignFinal   (int session, IntPtr Signature, ref int SignatureLen);

        bool VerifyInit  (int session, int alg, int hashAlg, int hKey);
        bool Verify      (int session, IntPtr Data, int DataLen, IntPtr Signature, int SignatureLen);
        bool VerifyUpdate(int session, IntPtr Data, int DataLen);
        bool VerifyFinal (int session, IntPtr Signature, int SignatureLen);
    }
}

