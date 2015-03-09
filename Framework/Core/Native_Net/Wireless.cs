////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.SPOT.Net.NetworkInformation
{
    public class Wireless80211 : NetworkInterface
    {
        [Flags]
        public enum AuthenticationType
        {
            None = 0,
            EAP,
            PEAP,
            WCN,
            Open,
            Shared,
        }

        [Flags]
        public enum EncryptionType
        {
            None = 0,
            WEP,
            WPA,
            WPAPSK,
            Certificate,
        }

        [Flags]
        public enum RadioType
        {
            a = 1,
            b = 2,
            g = 4,
            n = 8,
        }

        private Wireless80211(int interfaceIndex)
            : base(interfaceIndex)
        {
            NetworkKey = null;
            ReKeyInternal = null;
            Id = 0xFFFFFFFF;
        }

        public static void SaveConfiguration(Wireless80211[] wirelessConfigurations, bool useEncryption)
        {
            /// Before we update validate whether settings conform to right characteristics.
            for (int i = 0; i < wirelessConfigurations.Length; i++)
            {
                ValidateConfiguration(wirelessConfigurations[i]);
            }

            for (int i = 0; i < wirelessConfigurations.Length; i++)
            {
                UpdateConfiguration(wirelessConfigurations[i], useEncryption);
            }

            SaveAllConfigurations();
        }

        public static void ValidateConfiguration(Wireless80211 wirelessConfiguration)
        {
            if ((wirelessConfiguration.Authentication < AuthenticationType.None  ) || 
                (wirelessConfiguration.Authentication > AuthenticationType.Shared) ||
                 (wirelessConfiguration.Encryption < EncryptionType.None         ) || 
                 (wirelessConfiguration.Encryption > EncryptionType.Certificate  ) ||
                 (wirelessConfiguration.Radio < RadioType.a                      ) || 
                 (wirelessConfiguration.Radio > (RadioType.n | RadioType.g | RadioType.b | RadioType.a)))
            {
                throw new ArgumentOutOfRangeException();
            }

            if ((wirelessConfiguration.PassPhrase    == null) || 
                (wirelessConfiguration.NetworkKey    == null) || 
                (wirelessConfiguration.ReKeyInternal == null) || 
                (wirelessConfiguration.Ssid          == null))
            {
                throw new ArgumentNullException();
            }

            if ((wirelessConfiguration.PassPhrase.Length    >= MaxPassPhraseLength) || 
                (wirelessConfiguration.NetworkKey.Length    >  NetworkKeyLength   ) || 
                (wirelessConfiguration.ReKeyInternal.Length >  ReKeyInternalLength) || 
                (wirelessConfiguration.Ssid.Length          >= SsidLength         ))
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public AuthenticationType Authentication;
        public EncryptionType Encryption;
        public RadioType Radio;
        public string PassPhrase;
        public byte[] NetworkKey;
        public byte[] ReKeyInternal;
        public string Ssid;
        public readonly uint Id;

        public const int NetworkKeyLength = 256;
        public const int ReKeyInternalLength = 32;
        public const int SsidLength = 32;
        public const int MaxPassPhraseLength = 64;

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern static void UpdateConfiguration(Wireless80211 wirelessConfigurations, bool useEncryption);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern static void SaveAllConfigurations();
    }
}


