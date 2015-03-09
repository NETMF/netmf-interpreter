////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using Microsoft.SPOT.Wireless.IEEE_802_15_4.Phy;
using Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac;

namespace Microsoft.SPOT.Wireless.IEEE_802_15_4.Network
{
    public sealed class Singleton
    {
        // singleton
        private Singleton() { }
        private static NetworkLayer s_instance;

        /// <summary>
        /// Singleton instance of network layer on top of default 802.15.4 stack
        /// </summary>
        public static NetworkLayer Instance
        {
            get
            {
                if (null == s_instance)
                {
                    IPhy phy = new CC2420(CC2420PinConfig.DefaultiMXS());
                    IMac mac = new MacLayer(phy);
                    s_instance = new NetworkLayer(mac);
                }

                return s_instance;
            }
        }
    }
}


