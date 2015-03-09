using System;
using System.Net;

namespace Ws.Services.Discovery
{   
    /// <summary>
    /// Provides the global information about the discovery protocol
    /// </summary>
    public static class WsDiscovery
    {
        /// <summary>
        /// 3702 is defined in the ws-discovery specification as the one and only
        /// ws-discovery port (section 2.4 Protocol Assignments).
        /// </summary>
        public const int WsDiscoveryPort = 3702;

        /// <summary>
        /// 239.255.255.250 is defined in the ws-discovery specification as the
        /// one and only discovery address (section 2.4 Protocol Assignments).
        /// </summary>
        public static readonly IPAddress WsDiscoveryAddress = IPAddress.Parse("239.255.255.250");
        
        /// <summary>
        /// The discovery endpoint
        /// </summary>
        public static readonly IPEndPoint WsDiscoveryEndPoint = new IPEndPoint(WsDiscoveryAddress, WsDiscoveryPort);
    }
}

