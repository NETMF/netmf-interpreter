////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Net.Sockets
{
    using System;

    /// <devdoc>
    ///    <para>
    ///       Specifies the protocols that the <see cref='System.Net.Sockets.Socket'/> class supports.
    ///    </para>
    /// </devdoc>

    public enum ProtocolType
    {
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        IP = 0,    // dummy for IP

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        IPv6HopByHopOptions = 0,
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Icmp = 1,    // control message protocol
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Igmp = 2,    // group management protocol
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Ggp = 3,    // gateway^2 (deprecated)

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        IPv4 = 4,
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Tcp = 6,    // tcp
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Pup = 12,   // pup
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Udp = 17,   // user datagram protocol
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Idp = 22,   // xns idp
        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        IPv6 = 41,   // IPv4
        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        IPv6RoutingHeader = 43,   // IPv6RoutingHeader
        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        IPv6FragmentHeader = 44,   // IPv6FragmentHeader
        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        IPSecEncapsulatingSecurityPayload = 50,   // IPSecEncapsulatingSecurityPayload
        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        IPSecAuthenticationHeader = 51,   // IPSecAuthenticationHeader
        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        IcmpV6 = 58,   // IcmpV6
        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        IPv6NoNextHeader = 59,   // IPv6NoNextHeader
        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        IPv6DestinationOptions = 60,   // IPv6DestinationOptions
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        ND = 77,   // UNOFFICIAL net disk proto
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Raw = 255,  // raw IP packet

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Unspecified = 0,
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Ipx = 1000,
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Spx = 1256,
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        SpxII = 1257,

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Unknown = -1,   // unknown protocol type
    } // enum ProtocolType
} // namespace System.Net.Sockets


