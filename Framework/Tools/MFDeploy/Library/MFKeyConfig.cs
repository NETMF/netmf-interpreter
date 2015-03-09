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
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;

using Microsoft.SPOT.Debugger.WireProtocol;
using _DBG = Microsoft.SPOT.Debugger;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.PlugIns;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using dotNetMFCrypto;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
    [Serializable]
    public class KeyPair
    {
        public byte[] PrivateKey;
        public byte[] PublicKey;
    }

    public class MFKeyConfig
    {
        internal const int PublicKeySize = 260;
        internal const int PrivateKeySize = 260;
        internal const int SignatureSize = 128;
        internal const int RandomSeedSize = 16;

        public void SaveKeyPair(KeyPair keyPair, string fileName)
        {
            XmlSerializer xmls = new XmlSerializer(typeof(KeyPair));

            using (FileStream fs = File.Create(fileName))
            {
                xmls.Serialize(fs, keyPair);
            }
        }

        public KeyPair LoadKeyPair(string fileName)
        {
            XmlSerializer xmls = new XmlSerializer(typeof(KeyPair));
            KeyPair keyPair = null;

            using (FileStream fs = File.OpenRead(fileName))
            {
                keyPair = xmls.Deserialize(fs) as KeyPair;
            }

            return keyPair;
        }

        public KeyPair CreateEmptyKeyPair()
        {
            KeyPair keyPair = new KeyPair();
            keyPair.PrivateKey = new byte[PrivateKeySize];
            keyPair.PublicKey = new byte[PublicKeySize];

            for (int i = 0; i < PrivateKeySize; i++)
            {
                keyPair.PrivateKey[i] = 0xFF;
            }
            for (int i = 0; i < PublicKeySize; i++)
            {
                keyPair.PublicKey[i] = 0xFF;
            }
            return keyPair;
        }

        public KeyPair CreateKeyPair()
        {
            byte[] seed = new byte[RandomSeedSize + 2*sizeof(UInt16)];
            byte[] keyPrivate = new byte[PrivateKeySize];
            byte[] keyPublic = new byte[PublicKeySize];

            ushort delta1, delta2;

            Random random = new Random();

            for (int i = 0; i < 100; i++)
            {
                random.NextBytes(seed);

                //I'm not sure what this does, it was ported from MetaDataProcessor
                if (CryptoWrapper.Crypto_CreateZenithKey(seed, out delta1, out delta2) != (int)dotNetMFCrypto.CryptoWrapper.CRYPTO_RESULT.SUCCESS)
                    continue;

                byte []d0 = BitConverter.GetBytes(delta1);
                byte []d1 = BitConverter.GetBytes(delta2);

                seed[RandomSeedSize    ] = d0[0];
                seed[RandomSeedSize + 1] = d0[1];
                seed[RandomSeedSize + 2] = d1[0];
                seed[RandomSeedSize + 3] = d1[1];

                if (CryptoWrapper.Crypto_GeneratePrivateKey(seed, keyPrivate) != (int)dotNetMFCrypto.CryptoWrapper.CRYPTO_RESULT.SUCCESS)
                    continue;

                if (CryptoWrapper.Crypto_PublicKeyFromPrivate(keyPrivate, keyPublic) != (int)dotNetMFCrypto.CryptoWrapper.CRYPTO_RESULT.SUCCESS)
                    continue;

                KeyPair keyPair = new KeyPair();
                keyPair.PrivateKey = keyPrivate;
                keyPair.PublicKey = keyPublic;

                return keyPair;
            }

            throw new ApplicationException("Could not generate key pair");
        }

        public byte[] SignData(byte[] data, byte[] keyPrivate)
        {
            byte[] signature = new byte[SignatureSize];

            int result;
            bool fEmpty = true;

            for (int i = 0; i < keyPrivate.Length; i++)
            {
                if (keyPrivate[i] != (byte)0xFF)
                {
                    fEmpty = false;
                    break;
                }
            }

            if (fEmpty) // if we don't have a key any signature will do, so return 0x0
            {
                return signature;
            }            
            
            while ((result = CryptoWrapper.Crypto_SignBuffer(data, data.Length, keyPrivate, signature, signature.Length)) == (int)CryptoWrapper.CRYPTO_RESULT.CONTINUE)
            {
            }
            
            if (result != (int)dotNetMFCrypto.CryptoWrapper.CRYPTO_RESULT.SUCCESS)
            {
                throw new ApplicationException("Could not sign data");
            }

            return signature;
        }

        public void UpdateDeviceKey(MFDevice device, PublicKeyUpdateInfo.KeyIndex index, byte[] publicKey)
        {
            UpdateDeviceKey(device, index, publicKey, null);
        }

        public void UpdateDeviceKey(MFDevice device, PublicKeyUpdateInfo.KeyIndex index, byte[] publicKey, byte[] publicKeySignature)
        {
            MFConfigHelper cfg = new MFConfigHelper(device);

            PublicKeyUpdateInfo pku = new PublicKeyUpdateInfo();
            pku.NewPublicKey          = publicKey;
            pku.NewPublicKeySignature = publicKeySignature;
            pku.PublicKeyIndex        = index;

            cfg.UpdatePublicKey(pku);

            cfg.Dispose();
            cfg = null;
        }

        public string FormatPublicKey(KeyPair keyPair)
        {
            string retVal;
            int i = 0;
            int j = 0;

            retVal = "// exponent length\r\n";

            foreach (byte b in keyPair.PublicKey)
            {
                if (i == 4) { retVal += "\r\n\r\n// module\r\n"; j = 0; }

                if (j != 0 && (0 == (j % 10))) retVal += "\r\n";
                if (i == 132) { retVal += "\r\n\r\n// exponent\r\n"; j = 0; }

                retVal += string.Format("0x{0:x02}, ", b);

                j++;
                i++;
            }
            return retVal;
        }
    }
}
