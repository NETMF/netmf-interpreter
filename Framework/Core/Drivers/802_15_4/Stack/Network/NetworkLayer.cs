////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////#define USE_FRAG // use fragmentation layer
#define USE_FRAG // use fragmentation layer
using System;
using System.Threading;
using Microsoft.SPOT.Wireless.IEEE_802_15_4;
using Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac;
using Microsoft.SPOT.Wireless.IEEE_802_15_4.Network.Fragmentation;

namespace Microsoft.SPOT.Wireless.IEEE_802_15_4.Network
{
    /// <summary>
    /// This class implements the MicroBee network layer
    /// </summary>
    public class NetworkLayer : INetworkLayer, IDataCallbacks ,IDisposable
    {
        IMac _mac;
        Routing _route;
        NetworkMgmt _mgmt;
#if USE_FRAG
        IFragmentation _frag;
#endif

        /// <summary>
        /// Unique Id used to identify networks running this protocol version through Mac beacons.
        /// </summary>
        public static readonly UInt64 cMeshId = 0x519ECFF73DEA43B8;

        public NetworkLayer(IMac mac)
        {
            _mac = mac;
            _mgmt = new NetworkMgmt(this);
            _route = new Routing(this, mac, this);

#if USE_FRAG
            int mtu, head, tail;
            _route.GetMtuSize(out mtu, out head, out tail);
            _frag = new Fragmentation.Fragmentation(10, _route.DataRequest, mtu, head, tail);
#endif

            _mac.BeaconNotifyIndication = MacBeaconNotifyIndication;
            _mac.ResetRequest(true, null);
        }

        #region accessors
        internal IMac Mac
        { get { return _mac; } }
        internal Routing Routing
        { get { return _route; } }
        #endregion accessors

        #region API dispatchers
        /// <summary>
        /// To find nearby networks. Result list only include 802.15.4 networks that provide the protocol ID of this protocol in the beacons.
        /// </summary>
        /// <param name="handler">handler for result</param>
        public void ScanRequest(
            ScanConfirmHandler handler)
        {
            _mgmt.ScanRequest(handler);
        }

        /// <summary>
        /// Starts a new network as coordinator. There shall be only one coordinator per network.
        /// In the current implementation, the PAN-ID is derived from the lower 16 bits of the network ID.
        /// The channel is automatically selected.
        /// </summary>
        /// <param name="netId">unique network ID</param>
        /// <param name="handler">handlder for result</param>
        public void StartRequest(
            UInt16 panId,
            StartConfirmHandler handler)
        {
            _mgmt.StartRequest(panId, handler);
        }

        /// <summary>
        /// Joins a network that has been started by a coordinator before. This will scan for nearby devices internally.
        /// </summary>
        /// <param name="netId">unique network ID</param>
        /// <param name="handler">handler for result</param>
        public void JoinRequest(
            UInt16 panId,
            JoinConfirmHandler handler)
        {
            _mgmt.JoinRequest(panId, handler);
        }

        /// <summary>
        /// Leave network that has been started or joined before.
        /// </summary>
        /// <param name="handler">handler for result</param>
        public void LeaveRequest(
            LeaveConfirmHandler handler)
        {
            _mgmt.LeaveRequest(handler);
        }

        /// <summary>
        /// Send SDU to another device.
        /// </summary>
        /// <param name="targetShortAddr">targetAddr node, use 0xFFFF for broadcasting</param>
        /// <param name="sdu">data to send. Must have sufficient space in head/tail as indicated by GetFrameHeaders.</param>
        /// <param name="sduHandle">user-defined handle that is provided in result handler to identify the sdu.</param>
        /// <param name="handler">handler for result</param>
        public void DataRequest(
            UInt16 targetShortAddr,
            ref Frame sdu,
            Byte sduHandle,
            DataConfirmHandler handler)
        {
#if USE_FRAG
            _frag.DataRequest(targetShortAddr, ref sdu, sduHandle, handler);
#else
            _route.DataRequest(targetShortAddr, ref sdu, sduHandle, handler);
#endif
        }

        /// <summary>
        /// Send 6LoWPAN SDU to another device.
        /// SDU must begin with a valid dispatch value and not contain any mesh header or brodcast header.
        /// </summary>
        /// <param name="targetShortAddr">targetAddr node, use 0xFFFF for broadcasting</param>
        /// <param name="sdu">data to send. Must have sufficient space in head/tail as indicated by GetFrameHeaders.</param>
        /// <param name="sduHandle">user-defined handle that is provided in result handler to identify the sdu.</param>
        /// <param name="handler">handler for result</param>
        public void DataRequest6Low(
            UInt16 targetShortAddr,
            ref Frame sdu,
            Byte sduHandle,
            DataConfirmHandler handler)
        {
            _route.DataRequest6LoWPAN(targetShortAddr, ref sdu, sduHandle, handler);
        }

        /// <summary>
        /// Get required header space for sdu frames to be sent
        /// </summary>
        /// <param name="tail">maximum transmission unit in bytes</param>
        /// <param name="head">required space for header</param>
        /// <param name="tail">required space for tail</param>
        public void GetMtuSize(out int mtu, out int head, out int tail)
        {
#if USE_FRAG
            _frag.GetMtuSize(out mtu, out head, out tail);
#else
            _route.GetMtuSize(out mtu, out head, out tail);
#endif
        }

        /// <summary>
        /// Get required header space for 6LoWPAN sdu frames to be sent
        /// </summary>
        /// <param name="tail">maximum transmission unit in bytes</param>
        /// <param name="head">required space for header</param>
        /// <param name="tail">required space for tail</param>
        public void GetMtuSize6Low(out int mtu, out int head, out int tail)
        {
            _route.GetMtuSize6Low(out mtu, out head, out tail);
        }

        /// <summary>
        /// Get current neighbors of this node
        /// </summary>
        /// <param name="handler">handler for result</param>
        public void NeighborsRequest(NeighborConfirmHandler handler)
        {
            _route.NeighborRequest(handler);
        }

        /// <summary>
        /// Get available nodes of the network. Can only be run the coordinator.
        /// </summary>
        /// <param name="hanlder">handler of the result</param>
        public void NodeDiscoveryRequest(NodeDiscoveryConfirmHandler handler)
        {
            _route.NodeDiscoveryRequest(handler);
        }

        #endregion API dispatchers

        #region IRoutingTableCallbacks
        public void HandleDataIndication(UInt16 origAddr, UInt16 targetAddr, ref Frame sdu)
        {
#if USE_FRAG
            _frag.HandleLowerLayerDataIndication(this, origAddr, targetAddr, sdu);
            sdu = null;
#else
            DataIndicationHandler ind = _dataIndicationHandler;
            if (ind != null) {
                ind.Invoke(this, origAddr, targetAddr, sdu);
                sdu = null;
            }

#endif
        }

        public void HandleDataIndication6Low(UInt16 origAddr, UInt16 targetAddr, ref Frame sdu)
        {
            DataIndicationHandler ind = _dataIndicationHandler6Low;
            if (ind != null)
            {
                ind.Invoke(this, origAddr, targetAddr, sdu);
                sdu = null;
            }
        }

        public void StartData(UInt16 localAddr)
        {
#if USE_FRAG
            _frag.Start(localAddr);
#endif
        }

        public void StopData()
        {
#if USE_FRAG
            _frag.Stop();
#endif
        }

        #endregion

        #region API implementation
#if USE_FRAG
        /// <summary>
        /// Handler for data indications
        /// </summary>
        public DataIndicationHandler DataIndication
        {
            get { return _frag.DataIndication; }
            set { _frag.DataIndication = value; }
        }

#else
        private DataIndicationHandler _dataIndicationHandler;

        /// <summary>
        /// Handler for data indications
        /// </summary>
        public DataIndicationHandler DataIndication
        {
            get { return _dataIndicationHandler; }
            set { _dataIndicationHandler = value; }
        }

#endif

        private DataIndicationHandler _dataIndicationHandler6Low;

        /// <summary>
        /// Handler for data indications of 6LoWPAN SDUs.
        /// Mesh headers and brodcast headers are processed internally and are not exposed.
        /// </summary>
        public DataIndicationHandler DataIndication6Low
        {
            get { return _dataIndicationHandler6Low; }
            set { _dataIndicationHandler6Low = value; }
        }

        /// <summary>
        /// Get extended device address
        /// </summary>
        /// <returns></returns>
        public void GetDeviceAddress(out UInt64 address)
        {
            _mac.GetDeviceAddress(out address);
        }

        private NodeChangedHandler _nodeChangedIndication;

        public NodeChangedHandler NodeChangedIndication
        {
            get { return _nodeChangedIndication; }
            set { _nodeChangedIndication = value; }
        }

        #endregion API implementation

        #region Mac handlers
        private void ResetConfirmHandler(
            IMacMgmtSap sender,
            MacEnum status)
        {
            // don't care
        }

        private void MacBeaconNotifyIndication(
            IMacMgmtSap sender,
            ushort BSN,
            PanDescriptor panDescriptor,
            ushort[] pendingShortAddrs,
            UInt64[] pendingExtendedAddrs,
            Frame beaconPayload)
        {
            _mgmt.MacBeaconNotifyIndication(sender, BSN, panDescriptor, pendingShortAddrs, pendingExtendedAddrs, beaconPayload);
        }

        #endregion Mac handlers

        public virtual void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(_mac!=null) _mac.Dispose();  
            if(_route!=null)_route.Dispose();  
        }

    }
}


