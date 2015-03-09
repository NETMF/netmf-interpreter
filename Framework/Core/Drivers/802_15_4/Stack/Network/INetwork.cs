////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using Microsoft.SPOT.Wireless.IEEE_802_15_4;
using Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac;

namespace Microsoft.SPOT.Wireless.IEEE_802_15_4.Network
{
    public interface INetworkLayer
    {
        /// <summary>
        /// To find nearby networks. Result list only include 802.15.4 networks that provide the protocol ID of this protocol in the beacons.
        /// </summary>
        /// <param name="handler">handler for result</param>
        void ScanRequest(
            ScanConfirmHandler handler);

        /// <summary>
        /// Starts a new network as coordinator. There shall be only one coordinator per network.
        /// In the current implementation, the PAN-ID is derived from the lower 16 bits of the network ID.
        /// The channel is automatically selected.
        /// </summary>
        /// <param name="netId">unique network ID</param>
        /// <param name="handler">handlder for result</param>
        void StartRequest(
            UInt16 panId,
            StartConfirmHandler handler);

        /// <summary>
        /// Joins a network that has been started by a coordinator before. This will scan for nearby devices internally.
        /// </summary>
        /// <param name="netId">unique network ID</param>
        /// <param name="handler">handler for result</param>
        void JoinRequest(
            UInt16 panId,
            JoinConfirmHandler handler);

        /// <summary>
        /// Leave network that has been started or joined before.
        /// </summary>
        /// <param name="handler">handler for result</param>
        void LeaveRequest(
            LeaveConfirmHandler handler);

        /// <summary>
        /// Send SDU to another device.
        /// </summary>
        /// <param name="targetShortAddr">targetAddr node, use 0xFFFF for broadcasting</param>
        /// <param name="sdu">data to send. Must have sufficient space in head/tail as indicated by GetFrameHeaders.</param>
        /// <param name="sduHandle">user-defined handle that is provided in result handler to identify the sdu.</param>
        /// <param name="handler">handler for result</param>
        void DataRequest(
            UInt16 targetShortAddr,
            ref Frame sdu,
            Byte sduHandle,
            DataConfirmHandler handler);

        /// <summary>
        /// Handler for data indications
        /// </summary>
        DataIndicationHandler DataIndication { get; set; }

        /// <summary>
        /// Send 6LoWPAN SDU to another device.
        /// SDU must begin with a valid dispatch value and not contain any mesh header or brodcast header.
        /// </summary>
        /// <param name="targetShortAddr">targetAddr node, use 0xFFFF for broadcasting</param>
        /// <param name="sdu">data to send. Must have sufficient space in head/tail as indicated by GetFrameHeaders.</param>
        /// <param name="sduHandle">user-defined handle that is provided in result handler to identify the sdu.</param>
        /// <param name="handler">handler for result</param>
        void DataRequest6Low(
            UInt16 targetShortAddr,
            ref Frame sdu,
            Byte sduHandle,
            DataConfirmHandler handler);

        /// <summary>
        /// Handler for data indications of 6LoWPAN SDUs.
        /// Mesh headers and brodcast headers are processed internally and are not exposed.
        /// </summary>
        DataIndicationHandler DataIndication6Low { get; set; }

        /// <summary>
        /// Get required header space for sdu frames to be sent
        /// </summary>
        /// <param name="tail">maximum transmission unit in bytes</param>
        /// <param name="head">required space for header</param>
        /// <param name="tail">required space for tail</param>
        void GetMtuSize(out int mtu, out int head, out int tail);

        /// <summary>
        /// Get required header space for 6LoWPAN sdu frames to be sent
        /// </summary>
        /// <param name="tail">maximum transmission unit in bytes</param>
        /// <param name="head">required space for header</param>
        /// <param name="tail">required space for tail</param>
        void GetMtuSize6Low(out int mtu, out int head, out int tail);

        /// <summary>
        /// Get extended device address
        /// </summary>
        /// <returns></returns>
        void GetDeviceAddress(out UInt64 address);

        /// <summary>
        /// Get current neighbors of this node
        /// </summary>
        /// <param name="handler">handler for result</param>
        void NeighborsRequest(NeighborConfirmHandler handler);

        /// <summary>
        /// Get available nodes of the network. Can only be run the coordinator.
        /// </summary>
        /// <param name="hanlder">handler of the result</param>
        void NodeDiscoveryRequest(NodeDiscoveryConfirmHandler handler);

        /// <summary>
        /// Event for updating the set of available nodes. Is only used at the coordinator.
        /// </summary>
        NodeChangedHandler NodeChangedIndication { get; set; }
    }

    /// <summary>
    /// Handler for result of scan request
    /// </summary>
    /// <param name="sender">sender of this message</param>
    /// <param name="status">result of scan operation</param>
    /// <param name="networks">on success, list of networks found during scanning</param>
    public delegate void ScanConfirmHandler(
        object sender,
        Status status,
        ScanResult[] networks);

    /// <summary>
    /// Handler for result of start request
    /// </summary>
    /// <param name="sender">sender of this message</param>
    /// <param name="status">result of procedure</param>
    /// <param name="panId">on success, PAN ID of the network</param>
    /// <param name="shortAddr">on success, short address of the local node</param>
    public delegate void StartConfirmHandler(
        object sender,
        Status status,
        UInt16 shortAddr,
        byte logicalChannel,
        byte channelPage);

    /// <summary>
    /// Handler for result of join request
    /// </summary>
    /// <param name="sender">sender of this message</param>
    /// <param name="status">result of procedure</param>
    /// <param name="panId">on success, PAN ID of the network</param>
    /// <param name="shortAddr">on success, short address of the local node</param>
    public delegate void JoinConfirmHandler(
        object sender,
        Status status,
        UInt16 shortAddr,
        byte logicalChannel,
        byte channelPage);

    /// <summary>
    /// Handler for result of leave request
    /// </summary>
    /// <param name="sender">sender of this message</param>
    /// <param name="status">result of procedure</param>
    public delegate void LeaveConfirmHandler(
        object sender,
        Status status);

    /// <summary>
    /// Handler for result of data request
    /// </summary>
    /// <param name="sender">sender of this message</param>
    /// <param name="sduHandle">handle to identify the sdu this callback refers to</param>
    /// <param name="status">result of operation</param>
    public delegate void DataConfirmHandler(
        object sender,
        Byte sduHandle,
        Status status);

    /// <summary>
    /// Handler to indicate data (sdu) retrieved for the local node
    /// </summary>
    /// <param name="sender">sender of this message</param>
    /// <param name="originatorShortAddr">originating node of the sdu</param>
    /// <param name="targetShortAddr">targetAddr node of the sdu, 0xFFFF indicates broadcast</param>
    /// <param name="sdu">content of sdu</param>
    public delegate void DataIndicationHandler(
        object sender,
        UInt16 originatorShortAddr,
        UInt16 targetShortAddr,
        Frame sdu);

    /// <summary>
    /// Handler to indicate current neighbors of local node
    /// </summary>
    /// <param name="sender">sender of this message</param>
    /// <param name="status">result of operation</param>
    /// <param name="neighbors">set of current neighbors</param>
    public delegate void NeighborConfirmHandler(
        object sender,
        Status status,
        Neighbor[] neighbors);

    /// <summary>
    /// Handler to indicate current nodes of the network
    /// </summary>
    /// <param name="sender">sender of this message</param>
    /// <param name="status">result of operation</param>
    /// <param name="nodes">set of current nodes</param>
    public delegate void NodeDiscoveryConfirmHandler(
        object sender,
        Status status,
        UInt16[] nodes);

    /// <summary>
    /// Handler to indicate change of available nodes
    /// </summary>
    /// <param name="sender">sender of this message</param>
    /// <param name="node">the short address of the node that has joined or left the network</param>
    /// <param name="isAvailable">true if the node has joined, false if the node has left the network</param>
    public delegate void NodeChangedHandler(
        object sender,
        UInt16 node,
        bool isAvailable);

    /// <summary>
    /// Status codes
    /// </summary>
    public enum Status : byte
    {
        /// <summary>
        /// no error
        /// </summary>
        Success,
        /// <summary>
        /// The network layer or one of its sublayers is busy and cannot handle the request right now. try again later.
        /// </summary>
        Busy,
        /// <summary>
        /// An unspecified error has occured
        /// </summary>
        Error,
        /// <summary>
        /// A timeout has occured when trying to send the SDU over the MAC layer.
        /// </summary>
        Timeout,
        /// <summary>
        /// No route to the target device could be found.
        /// </summary>
        NoRoute,
        /// <summary>
        /// The network layer is not running.
        /// </summary>
        NotRunning,
        /// <summary>
        /// The frame is invalid. Either it is larger than the Mtu or the header/tailer space is too small.
        /// </summary>
        InvalidFrame,
        /// <summary>
        /// The layer could not get an address from the address server when trying to join the network.
        /// </summary>
        NoAddress,
        /// <summary>
        /// No neighboring node could be found when trying to join the network.
        /// </summary>
        NoNeighbor
    }

    /// <summary>
    /// This type is used to describe a network found during scanning.
    /// </summary>
    public struct ScanResult
    {
        /// <summary>
        /// The PanId of the network
        /// </summary>
        public UInt16 panId;
        /// <summary>
        /// The link quality indicator towards this network
        /// </summary>
        public Byte linkQuality;
    }

    /// <summary>
    /// This describes a neighbor node
    /// </summary>
    public struct Neighbor
    {
        /// <summary>
        /// 802.15.4 short address
        /// </summary>
        public UInt16 shortAdr;
        /// <summary>
        /// 802.15.4 link quality indicator
        /// </summary>
        public Byte lqi;
    }
}


