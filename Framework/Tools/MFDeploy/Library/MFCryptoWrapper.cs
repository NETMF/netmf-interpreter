//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Microsoft .NET Micro Framework Porting Kit Code Samples and is unsupported. 
// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use these files except in compliance with the License.
// You may obtain a copy of the License at:
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing
// permissions and limitations under the License.
// 
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;
using System.Runtime.InteropServices;

namespace dotNetMFCrypto
{
    public sealed class CryptoWrapper
    {
        public enum CRYPTO_RESULT
        {
            CONTINUE = 1,	// operation should continue
            SUCCESS = 0,		// alles gute
            SIGNATUREFAIL = -1,
            BADPARMS = -2,
            KEYEXPIRED = -3,
            UNKNOWNKEY = -4,
            UNKNOWNERROR = -5,
            NOMEMORY = -6,
            ACTIVATIONBADSYNTAX = -7,
            ACTIVATIONBADCONTROLCHAR = -8,
            FAILURE = -9
        }

        public enum RSA_OPS
        {
            ENCRYPT,
            DECRYPT,
            VERIFYSIGNATURE
        }

        [DllImport(@"Crypto.dll", CallingConvention = CallingConvention.Cdecl ) ]
        static public extern bool Crypto_Encrypt([In] byte[] Key, [In, Out] byte[] IV, int cbIVSize, [In] byte[] pPlainText, int cbPlainText, [Out] byte[] pCypherText, int cbCypherText);

        [DllImport(@"Crypto.dll", CallingConvention = CallingConvention.Cdecl )]
        static public extern bool Crypto_Decrypt([In] byte[] Key, [In, Out] byte[] IV, int cbIVSize, [In] byte[] pCypherText, int cbCypherText, [Out] byte[] pPlainText, int cbPlainText);

        [DllImport(@"Crypto.dll", CallingConvention = CallingConvention.Cdecl )]
        static public extern int Crypto_CreateZenithKey([In] byte[] seed, out ushort delta1, out ushort delta2);

        [DllImport(@"Crypto.dll", CallingConvention = CallingConvention.Cdecl )]
        static public extern int Crypto_SignBuffer([In] byte[] buffer, int bufLen, [In] byte[] key, [Out] byte[] signature, int siglen);

        [DllImport(@"Crypto.dll", CallingConvention = CallingConvention.Cdecl )]
        static public extern int Crypto_GeneratePrivateKey([In] byte[] seed, [Out] byte[] privateKey);

        [DllImport(@"Crypto.dll", CallingConvention = CallingConvention.Cdecl )]
        static public extern int Crypto_PublicKeyFromPrivate([In] byte[] privkey, [Out] byte[] pubkey);
    }
}

