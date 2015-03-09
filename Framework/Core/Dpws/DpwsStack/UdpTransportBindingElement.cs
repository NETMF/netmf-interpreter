using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections;
using System.Threading;
using Ws.Services.Soap;
using Ws.Services.Transport;
using Ws.Services.Discovery;

namespace Ws.Services.Binding
{
    /// <summary>
    /// Abtracts the configuration for the UDP transport
    /// </summary>
    public class UdpTransportBindingConfig
    {
        /// <summary>
        /// Creates an instance of the UDP configuration
        /// </summary>
        /// <param name="discoveryAddress">The discovery address of the UDP service.</param>
        /// <param name="discoveryPort">The discovery port of the UDP service.</param>
        /// <param name="ignoreRequestsFromThisIp">When <c>true</c>, all requests from thsi IP address will be ignored.</param>
        public UdpTransportBindingConfig(IPAddress discoveryAddress, int discoveryPort, bool ignoreRequestsFromThisIp)
        {
            this.DiscoveryAddress         = discoveryAddress;
            this.DiscoveryPort            = discoveryPort;
            this.IgnoreRequestsFromThisIp = ignoreRequestsFromThisIp;
        }

        /// <summary>
        /// When true, all requests from thsi IP address will be ignored
        /// </summary>
        public readonly bool      IgnoreRequestsFromThisIp;
        /// <summary>
        /// The discovery address of this service.
        /// </summary>
        public readonly IPAddress DiscoveryAddress;
        /// <summary>
        /// The discovery port of this service
        /// </summary>
        public readonly int       DiscoveryPort;
    }

    /// <summary>
    /// Abtracts the BindingElement for the UDP transport
    /// </summary>
    public class UdpTransportBindingElement : TransportBindingElement
    {
        Socket                    m_udpReceiveClient;
        UdpTransportBindingConfig m_config;
        static ArrayList          s_repeats  = new ArrayList();
        static Timer              s_udpTimer = new Timer(new TimerCallback(UdpTimer), null, -1, -1);
        static IPAddress          s_localIP;

        /// <summary>
        /// Contains the data required for sending repeated UDP responses
        /// </summary>
        internal class UdpResend
        {
            internal UdpResend( byte[] data, IPEndPoint remoteEP, int repeatCount)
            {
                Data           = data;
                RemoteEndpoint = new IPEndPoint(remoteEP.Address, remoteEP.Port);
                RepeatCount    = repeatCount;
            }
            internal byte[]   Data;
            internal EndPoint RemoteEndpoint;
            internal int      RepeatCount;
        }

        /// <summary>
        ///  The maximum size of a UDP packet 
        /// </summary>
        public const int MaxUdpPacketSize = 5229;

        /// <summary>
        /// Creates an instance of the UDP binding element
        /// </summary>
        /// <param name="cfg">The configuration associated with this binding element.</param>
        public UdpTransportBindingElement(UdpTransportBindingConfig cfg)
        {
            m_config = cfg;

            if(s_localIP == null)
            {
                s_localIP = IPAddress.Parse(WsNetworkServices.GetLocalIPV4Address());
            }
        }

        /// <summary>
        /// Sets the configuration for the UDP transport binding 
        /// </summary>
        /// <param name="cfg">The configuration for this binding.</param>
        protected override void OnSetBindingConfiguration(object cfg)
        {
            if (cfg is UdpTransportBindingConfig)
            {
                m_config = (UdpTransportBindingConfig)cfg;
            }

            if (m_config == null) throw new Exception(); // no binding configuration
        }

        /// <summary>
        /// Opens the stream for the UDP tansport binding 
        /// </summary>
        /// <param name="stream">The stream for this binding.</param>
        /// <param name="ctx">The context associated with the stream for this binding.</param>
        /// <returns>The handling status for this operation.</returns>
        protected override ChainResult OnOpen( ref Stream stream, BindingContext ctx )
        {
            m_udpReceiveClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint localEP = new IPEndPoint(s_localIP, m_config.DiscoveryPort);
            try
            {
                m_udpReceiveClient.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            }
            catch{}
            try
            {
                m_udpReceiveClient.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 0x5000);
            }
            catch{}

            // Join Multicast Group
            byte[] discoveryAddr = m_config.DiscoveryAddress.GetAddressBytes();
            byte[] ipAddr        = s_localIP.GetAddressBytes();
            byte[] multicastOpt  = new byte[] {  discoveryAddr[0], discoveryAddr[1], discoveryAddr[2], discoveryAddr[3],   // WsDiscovery Multicast Address: 239.255.255.250
                                                 ipAddr       [0], ipAddr       [1], ipAddr       [2], ipAddr       [3] }; // Local IPAddress
            m_udpReceiveClient.Bind(localEP);

            try
            {
                m_udpReceiveClient.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, ipAddr );
                m_udpReceiveClient.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, multicastOpt);
            }
            catch{}

            return ChainResult.Continue;
        }

        /// <summary>
        /// Closes the stream for the UDP transport binding
        /// </summary>
        /// <param name="stream">The stream for this binding.</param>
        /// <param name="ctx">The context associated with the stream for this binding.</param>
        /// <returns>The handling status for this operation.</returns>
        protected override ChainResult OnClose( Stream stream, BindingContext ctx )
        {
            m_udpReceiveClient.Close();

            s_udpTimer.Change(-1,-1);
            s_udpTimer.Dispose();

            return ChainResult.Handled;
        }

        /// <summary>
        /// Processes a message
        /// </summary>
        /// <param name="stream">The message being processed.</param>
        /// <param name="ctx">The context associated with the message.</param>
        /// <returns>The handling status for this operation.</returns>
        protected override ChainResult OnProcessInputMessage( ref WsMessage msg, BindingContext ctx )
        {
            byte[] soapMessage = null;
            byte[] buffer = new byte[MaxUdpPacketSize];

            while (true)
            {
                EndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);

                int size = m_udpReceiveClient.ReceiveFrom(buffer, MaxUdpPacketSize, SocketFlags.None, ref remoteEndpoint);

                // If the stack is set to ignore request from this address do so
                if (m_config.IgnoreRequestsFromThisIp && ((IPEndPoint)remoteEndpoint).Address.Equals(s_localIP))
                {
                    continue;
                }

                if (size > 0)
                {
                    soapMessage = new byte[size];
                    Array.Copy(buffer, soapMessage, size);
                }
                else
                {
                    System.Ext.Console.Write("UDP Receive returned 0 bytes");
                }

                ctx.ContextObject = remoteEndpoint;

                System.Ext.Console.Write("UDP Request From: " + remoteEndpoint.ToString());
                System.Ext.Console.Write(soapMessage);

                break;
            }


            msg.Body = soapMessage;

            return ChainResult.Continue;
        }

        /// <summary>
        /// UDP resend timer callback.  Resends repsonse data for UDP messages.
        /// </summary>
        /// <param name="arg"></param>
        static private void UdpTimer(object arg)
        {
            int cnt;
            UdpResend[] resend = null;

            // get a copy of the resend list so that we don't have to lock the list
            // while we resend
            lock(s_repeats)
            {
                cnt = s_repeats.Count;
                if (cnt > 0)
                {
                    resend = (UdpResend[])s_repeats.ToArray(typeof(UdpResend));
                }
                else
                {
                    s_udpTimer.Change(-1, -1);
                }
            }

            if(cnt > 0)
            {
                using(Socket udpSendClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    IPEndPoint localEP = new IPEndPoint(s_localIP, 0);
                    try
                    {
                        udpSendClient.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, (int)WsNetworkServices.GetLocalIPV4AddressValue());
                    }
                    catch{}
                    udpSendClient.Bind(localEP);

                    for (int i = cnt - 1; i >= 0; i--)
                    {
                        udpSendClient.SendTo(resend[i].Data, resend[i].Data.Length, SocketFlags.None, resend[i].RemoteEndpoint);

                        if(0 >= --((UdpResend)s_repeats[i]).RepeatCount)
                        {
                            s_repeats.RemoveAt(i);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Processes a message 
        /// </summary>
        /// <param name="msg">The message being processed.</param>
        /// <param name="ctx">The context associated with the message.</param>
        /// <returns>The handling status for this operation.</returns>
        protected override ChainResult OnProcessOutputMessage( ref WsMessage msg, BindingContext ctx )
        {
            if (ctx.ContextObject == null) throw new Exception();

            byte []message = msg.Body as byte[];

            if (message == null) return ChainResult.Abort;

            IPEndPoint epRemote = (IPEndPoint)ctx.ContextObject;

            if(!m_config.IgnoreRequestsFromThisIp && epRemote.Address == IPAddress.GetDefaultLocalAddress())
            {
                epRemote = new IPEndPoint(IPAddress.Loopback, epRemote.Port);
            }

            System.Ext.Console.Write("UDP Message Sent To: " + epRemote.ToString());
            System.Ext.Console.Write(message);
            
            try
            {
                /// Add a UDP repeat record to the current list of UDP responses to be processed
                /// by a common timer.
                lock (s_repeats)
                {
                    s_repeats.Add(new UdpResend(message, epRemote, 3));

                    if (s_repeats.Count == 1) s_udpTimer.Change(50, 100);
                }
            }
            catch
            {
                return ChainResult.Abort;
            }

            return ChainResult.Handled;
        }
    }
}
