////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#define MICROFRAMEWORK

using System;
using System.Threading;
using Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac;

namespace Microsoft.SPOT.Wireless.IEEE_802_15_4.Network
{
    /// <summary>
    /// Interface for callbacks of data indication handler
    /// </summary>
    public interface IDataCallbacks
    {
        /// <summary>
        /// Handle received data
        /// </summary>
        /// <param name="origAddr">originator address</param>
        /// <param name="targetAddr">target address</param>
        /// <param name="sdu">data</param>
        void HandleDataIndication(UInt16 origAddr, UInt16 targetAddr, ref Frame sdu);

        /// <summary>
        /// Handle received data with 6LoWPAN header
        /// </summary>
        /// <param name="origAddr">originator address</param>
        /// <param name="targetAddr">target address</param>
        /// <param name="sdu">data</param>
        void HandleDataIndication6Low(UInt16 origAddr, UInt16 targetAddr, ref Frame sdu);

        /// <summary>
        /// Start processing of received data
        /// </summary>
        /// <param name="localAddr">address of local node</param>
        void StartData(UInt16 localAddr);

        /// <summary>
        /// Stop processing of received data
        /// </summary>
        void StopData();
    }

    /// <summary>
    /// This class handles all sdu transmission and corresponding routing information.
    /// </summary>
    public class Routing : INeighborTableCallbacks, IRoutingTableCallbacks, IDiscoveryServerCallbacks,IDisposable 
    {
        public const UInt16 cInvalidShortAddr = 0xFFFF;
        public const UInt16 cBroadcastShortAddr = 0xFFFF;
        public const UInt16 cUnallocatedShortAddr = 0xFFFE; // Mac spec
        public const UInt16 cCoordinatorShortAddr = 0; // == address server
        public const int cAddressRequestTimeout = 5000; // milliseconds

        public const byte cSeqNoWraparound = 4;
        private const byte cDefaultHopLimit = 8;

        private INetworkLayer _net;
        private IMac _mac;
        private IDataCallbacks _data;
        private NeighborTable _neighbourTable;
        private RoutingTable _routingTable;
        private MessageContext _messageContext;
        private AddressAndDiscoveryServer _addrServer;

        private byte _seqNoDYMO; // local seqNo for route requests
        private byte _seqNoBroadcast; // local seqNo for broadcast messaged
        private UInt16 _panId; // local PAN ID
        private UInt16 _addrShort; // local short address
        private UInt64 _addrExt; // local extended address
        private int _macHeader; // header space requested by Mac
        private int _macTailer; // tail space requested by Mac
        private int _macMtu; // MTU of Mac
        private int _netHeader6Low; // header space requested by us
        private int _netTailer6Low; // tail space requested by us
        private int _netMtu6Low; // MTU of us
        private bool _isRunning; // true if layer was started
        private bool _isAddrServer; // true is we are the address server
        private StartConfirmHandler _startConfirmHandler; // handler for startup
        private UInt16 _discoverReplyInterval;
        private Timer _discoverTimer;

        private UInt16[] _getAddressNeighbors;
        private Thread _getAddressThread; // thread to request local short addr
        private bool _getAddressCancel; // flag to stop thread
        private AutoResetEvent _getAddressEvent; // event raised when we got an address

        public Routing(INetworkLayer net, IMac mac, IDataCallbacks data)
        {
            _net = net;
            _mac = mac;
            _data = data;

            _mac.GetMtuSize(out _macMtu, out _macHeader, out _macTailer);

            // calculate the header sizes for 6LoWPAN
            // private encapsulation requires one byte (Message.Data.cLength) additional header space
            int myHeader = Messages6LoWPAN.MeshHeader.cLengthMax + Messages6LoWPAN.BroadcastHeader.cLength;
            _netHeader6Low = _macHeader + myHeader;
            _netMtu6Low = _macMtu - myHeader;
            _netTailer6Low = _macTailer;

            _neighbourTable = new NeighborTable(this, _macHeader, _macTailer);
            _routingTable = new RoutingTable(this);
            _messageContext = new MessageContext();

            _seqNoDYMO = 0;
            _seqNoBroadcast = 0;
            _panId = 0;
            _addrShort = cInvalidShortAddr;
            _addrExt = 0; // to be set at start
            _isRunning = false;
            _getAddressEvent = new AutoResetEvent(false);
        }

        #region start and stop
        /// <summary>
        /// delegate for confirmation of start procedure
        /// </summary>
        /// <param name="sender">sender of the message</param>
        /// <param name="status">result of the procedure</param>
        /// <param name="shortAddr">assigned short address</param>
        public delegate void StartConfirmHandler(
            Routing sender,
            Status status,
            UInt16 shortAddr);

        /// <summary>
        /// To start the network layer
        /// </summary>
        /// <param name="panId">Pan ID of the network to set on Mac layer</param>
        /// <param name="panCoordinator">True if this node is the network coordinator</param>
        /// <param name="logicalChannel">Logical channel to use</param>
        /// <param name="channelPage">Channel page to use</param>
        /// <param name="neighours">Set of neighbors to bootstrap. Not used when started as network coordinator</param>
        /// <param name="handler">Handler for confirmation message</param>
        public void Start(UInt16 panId, bool netCoordinator, byte logicalChannel, byte channelPage, UInt16[] neighours, StartConfirmHandler handler)
        {
            if (_isRunning)
            {
                if (handler != null)
                    handler.Invoke(this, Status.Busy, 0);
                return;
            }

            _isRunning = true;
            _panId = panId;
            _startConfirmHandler = handler;

            if (netCoordinator)
            {
                _addrShort = cCoordinatorShortAddr;
                _addrServer = new AddressAndDiscoveryServer(this); // always exists if isAddrServer is true
            }
            else
            {
                _addrShort = cUnallocatedShortAddr;
                _addrServer = null;
            }

            _isAddrServer = netCoordinator;
            _mac.DataIndication = MacDataIndHandler;
            _mac.GetDeviceAddress(out _addrExt);

            PibValue value = new PibValue();
            value.Int = _addrShort;
            _mac.SetRequest(PibAttribute.macShortAddress, 0, value, null);

            AutoResetEvent startEvent = new AutoResetEvent(false);
            Status result = Status.Error;
            _mac.StartRequest(_panId,
                logicalChannel,
                channelPage,
                0, 15, 0,
                netCoordinator,
                false, false,
                new SecurityOptions(), new SecurityOptions(), delegate(
                    IMacMgmtSap sender,
                    MacEnum status)
                    {
                        if (status == MacEnum.Success)
                            result = Status.Success;
                        startEvent.Set();
                    });

            startEvent.WaitOne();

            if (result == Status.Success && !_isAddrServer)
            {
                _getAddressNeighbors = neighours;
                _getAddressCancel = false;
                _getAddressEvent.Reset();
                _getAddressThread = new Thread(GetAddressThread);
#if !MICROFRAMEWORK
                _getAddressThread.IsBackground = true;
#endif
                _getAddressThread.Start();
            }
            else
            {
                FinalizeStartup(result);
            }
        }

        private void FinalizeStartup(Status result)
        {
            if (result == Status.Success)
            {
                _data.StartData(_addrShort);
                _neighbourTable.Start(_addrShort);

                // tune MAC attributes
                PibValue value = new PibValue();
                // set macMinBE (backoff exponent) to 1. SW-MAC is slow enough
                value.Int = 1;
                _mac.SetRequest(PibAttribute.macMinBE, 0, value, null);
                // set macMaxFrameRetries to 7. Agressive retries to avoid upper layer dealing with link errors
                value.Int = 7;
                _mac.SetRequest(PibAttribute.macMaxFrameRetries, 0, value, null);
            }
            else
            {
                _isRunning = false;
            }

            if (_startConfirmHandler != null)
                _startConfirmHandler.Invoke(this, result, _addrShort);
        }

        /// <summary>
        /// Shutdown the network layer
        /// </summary>
        public void Stop()
        {
            if (!_isRunning)
                return;
            if (_getAddressThread != null)
            {
                _getAddressCancel = true;
                _getAddressEvent.Set();
                _getAddressThread.Join();
                _getAddressThread = null;
            }

            if (_discoverTimer != null)
            {
                _discoverTimer.Dispose();
                _discoverTimer = null;
                _discoverReplyInterval = 0;
            }

            if (_addrServer != null)
            {
                _addrServer.Dispose();
                _addrServer = null;
            }

            _data.StopData();

            _isRunning = false;
            _mac.DataIndication = null;
            _neighbourTable.Stop(true);
        }

        #endregion

        #region NetworkLayer API
        public void DataRequest(
            UInt16 targetAddr,
            ref Frame sdu,
            Byte sduHandle,
            DataConfirmHandler handler)
        {
            if (targetAddr == _addrShort)
            {
                // deliver frame to ourself
                if (handler != null)
                    handler.Invoke(_net, sduHandle, Status.Success);

                _data.HandleDataIndication(targetAddr, targetAddr, ref sdu);
            }
            else
            {
                // encapsulate and sent as 6LoWPAN
                Message.Data data = new Message.Data();
                if (data.WriteToFrameHeader(sdu))
                {
                    DataRequest6LoWPAN(targetAddr, ref sdu, sduHandle, handler);
                }
                else
                {
                    if (handler != null)
                        handler.Invoke(_net, sduHandle, Status.InvalidFrame);
                }
            }

            Frame.Release(ref sdu);
        }

        public void DataRequest6LoWPAN(
            UInt16 targetAddr,
            ref Frame sdu,
            Byte sduHandle,
            DataConfirmHandler handler)
        {
            Status result = Status.Success;
            if (sdu == null || sdu.LengthHeaderAvail < _netHeader6Low || sdu.LengthDataAvail < _netTailer6Low || sdu.LengthDataUsed > _netMtu6Low)
            {
                result = Status.InvalidFrame;
            }
            else if (!_isRunning)
            {
                result = Status.NotRunning;
            }
            else
            {
                if (targetAddr == _addrShort)
                {
                    // deliver frame to ourself
                    if (handler != null)
                        handler.Invoke(_net, sduHandle, Status.Success);

                    _data.HandleDataIndication(targetAddr, targetAddr, ref sdu);
                }
                else
                {
                    // send frame to another node
                    if (IsMulticast(targetAddr))
                    {
                        SendData(_addrShort, targetAddr, cBroadcastShortAddr, ref sdu, sduHandle, handler);
                    }
                    else
                    {
                        // unicast
                        _routingTable.DataRequest(targetAddr, ref sdu, sduHandle, handler, false);
                    }
                }
            }

            if (result != Status.Success)
            { // perform callback
                if (handler != null)
                    handler.Invoke(_net, sduHandle, result);
            }

            Frame.Release(ref sdu);
        }

        /// <summary>
        /// Get required header space for sdu frames to be sent
        /// </summary>
        /// <param name="tail">maximum transmission unit in bytes</param>
        /// <param name="head">required space for header</param>
        /// <param name="tail">required space for tail</param>
        public void GetMtuSize(out int mtu, out int head, out int tail)
        {
            int privateHeader = Message.Data.cLength;
            mtu = _netMtu6Low - privateHeader;
            head = _netHeader6Low + privateHeader;
            tail = _netTailer6Low;
        }

        /// <summary>
        /// Get required header space for sdu frames to be sent
        /// </summary>
        /// <param name="tail">maximum transmission unit in bytes</param>
        /// <param name="head">required space for header</param>
        /// <param name="tail">required space for tail</param>
        public void GetMtuSize6Low(out int mtu, out int head, out int tail)
        {
            mtu = _netMtu6Low;
            head = _netHeader6Low;
            tail = _netTailer6Low;
        }

        /// <summary>
        /// Get current neighbors of this node
        /// </summary>
        /// <param name="handler">handler for result</param>
        public void NeighborRequest(NeighborConfirmHandler handler)
        {
            if (handler == null)
                return;

            if (_isRunning)
            {
                Neighbor[] res = _neighbourTable.GetNeighbors();
                handler.Invoke(_net, Status.Success, res);
            }
            else
            {
                handler.Invoke(_net, Status.NotRunning, null);
            }
        }

        /// <summary>
        /// Get available nodes of the network. Can only be run the coordinator.
        /// </summary>
        /// <param name="hanlder">handler of the result</param>
        public void NodeDiscoveryRequest(NodeDiscoveryConfirmHandler handler)
        {
            if (handler == null)
                return;
            Status status = Status.Success;
            UInt16[] nodes = null;

            if (!_isRunning)
            {
                status = Status.NotRunning;
            }
            else if (!_isAddrServer)
            {
                status = Status.Error;
            }

            if (status == Status.Success)
            {
                nodes = _addrServer.GetNodes();
            }

            handler.Invoke(_net, status, nodes);
        }

        #endregion

        #region INeighborTableCallbacks
        /// <summary>
        /// send beacon message
        /// </summary>
        /// <param name="sdu">beacon encoded into frame</param>
        public void SendBeacon(ref Frame sdu)
        {
            MacDataRequest(cBroadcastShortAddr, ref sdu);
        }

        #endregion INeighborTableCallbacks

        #region IRoutingTableCallbacks
        /// <summary>
        /// trigger route request from local node
        /// </summary>
        /// <param name="target">target node</param>
        public void RouteRequest(UInt16 target)
        {
            SendRouteRequest(target);
        }

        /// <summary>
        /// trigger data transmission from local node
        /// </summary>
        /// <param name="targetAddr">target node</param>
        /// <param name="nextHopAddr">next hop address</param>
        /// <param name="sdu">data to send</param>
        /// <param name="sduHandle">handle to use in confirmation message</param>
        /// <param name="handler">confirmation handler</param>
        /// <param name="isControlMsg">true is message is control message, so no additional header will be added</param>
        public void DataRequest(
            UInt16 targetAddr,
            UInt16 nextHopAddr,
            ref Frame sdu,
            Byte sduHandle,
            DataConfirmHandler handler,
            bool isControlMsg)
        {
            if (isControlMsg)
            {
                MacDataRequest(nextHopAddr, ref sdu); // do not add any headers
            }
            else
            {
                SendData(_addrShort, targetAddr, nextHopAddr, ref sdu, sduHandle, handler);
            }
        }

        /// <summary>
        /// discard queued data at local node
        /// </summary>
        /// <param name="target">target node of message</param>
        /// <param name="sduHandle">sdu handle of message</param>
        /// <param name="handler">confirmation handler provided for message</param>
        /// <param name="isControlMsg">true if message is control message</param>
        public void DataRequestTimeout(
            UInt16 target,
            Byte sduHandle,
            DataConfirmHandler handler,
            bool isControlMsg)
        {
            if (handler != null)
                handler.Invoke(_net, sduHandle, Status.NoRoute);
        }

        #endregion

        #region IDiscoveryServerCallbacks
        /// <summary>
        /// Handler to indicate change of available nodes
        /// </summary>
        /// <param name="node">the short address of the node that has joined or left the network</param>
        /// <param name="isAvailable">true if the node has joined, false if the node has left the network</param>
        public void NodeChanged(
            UInt16 node,
            bool isAvailable)
        {
            NodeChangedHandler handler = _net.NodeChangedIndication;
            if (handler != null)
            {
                handler.Invoke(_net, node, isAvailable);
            }
        }

        #endregion

        #region helpers
        /// <summary>
        /// Adds broadcast/mesh headers and sends SDU to MAC
        /// </summary>
        /// <returns>True on success</returns>
        private void SendData(
            UInt16 orgAddr,
            UInt16 tgtAddr,
            UInt16 nextHopAddr,
            ref Frame sdu,
            Byte sduHandle,
            DataConfirmHandler handler)
        {
            bool success = false;
            if (orgAddr != cInvalidShortAddr)
            {
                bool ok = true;
                bool multicast = false;
                if (IsMulticast(tgtAddr))
                {
                    multicast = true;
                    Messages6LoWPAN.BroadcastHeader bh = new Messages6LoWPAN.BroadcastHeader();
                    bh.seqNo = IncSeqNoBroadcast();
                    ok = bh.WriteToFrameHeader(sdu);
                }

                if (ok && (multicast || (tgtAddr != nextHopAddr)))
                {
                    // add mesh header
                    Messages6LoWPAN.MeshHeader mh = new Messages6LoWPAN.MeshHeader();
                    mh.originatorAddress = orgAddr;
                    mh.finalAddress = tgtAddr;
                    mh.HopsLeft = cDefaultHopLimit;
                    ok = mh.WriteToFrameHeader(sdu);
                }

                if (ok)
                {
                    MacDataRequest(nextHopAddr, ref sdu, sduHandle, handler);
                    success = true;
                }
            }

            Frame.Release(ref sdu);
            if (!success && handler != null)
                handler.Invoke(_net, sduHandle, Status.Busy);
        }

        private void SendRouteRequest(UInt16 tgtAddrShort)
        {
            Message.RoutingMessage rm = new Message.RoutingMessage();
            rm.IsRequest = true;
            rm.HopsLeft = cDefaultHopLimit;
            rm.HopCount = 0; // counter from originator, initially set to zero
            rm.MinLQI = 0xFF; // initial value
            rm.SeqNo = IncSeqNoDYMO();
            rm.TargetAddr = tgtAddrShort;
            rm.OriginatorAddr = _addrShort;

            SendRoutingMessage(rm);
        }

        private void SendRoutingMessage(Message.RoutingMessage rm)
        {
            Frame frame = Frame.GetFrame(_macHeader + Message.RoutingMessage.cLength + _macTailer);
            if (frame == null)
                return;
            frame.ReserveHeader(_macHeader);
            if (!rm.WriteToFrame(frame))
            {
                Frame.Release(ref frame);
                return;
            }

            bool ok;
            UInt16 nextHop;
            if (rm.IsRequest)
            {
                ok = true;
                nextHop = cBroadcastShortAddr;
            }
            else
            {
                ok = _routingTable.GetRoute(rm.TargetAddr, out nextHop);
            }

            if (ok)
            {
                Trace.Print("Sending " + rm.ToString());
                MacDataRequest(nextHop, ref frame);
            }

            Frame.Release(ref frame);
        }

        private void SendRouteError(
            UInt16 tgtAddr,
            UInt16 unrAddr,
            bool fatalError)
        {
            Message.RouteError re = new Message.RouteError();
            re.FatalError = fatalError;
            re.HopsLeft = cDefaultHopLimit; // time-to-live, drop when 0
            re.TargetAddr = tgtAddr;
            re.OriginatorAddr = _addrShort;
            re.UnreachableAddr = unrAddr;
            SendRouteError(re);
        }

        private void SendRouteError(Message.RouteError re)
        {
            Frame frame = Frame.GetFrame(_macHeader + Message.RouteError.cLength + _macTailer);
            if (frame == null)
                return;
            frame.ReserveHeader(_macHeader);
            if (re.WriteToFrame(frame))
            {
                UInt16 nextHop;
                if (_routingTable.GetRoute(re.TargetAddr, out nextHop))
                {
                    MacDataRequest(nextHop, ref frame);
                }
            }

            Frame.Release(ref frame);
        }

        private void MacDataRequest(
            UInt16 nextHopAddr,
            ref Frame sdu,
            Byte sduHandle,
            DataConfirmHandler handler)
        {
            MacDataRequestReal(false, nextHopAddr, 0, ref sdu, sduHandle, handler);
        }

        private void MacDataRequest(
            UInt16 nextHopAddr,
            ref Frame sdu)
        {
            MacDataRequestReal(false, nextHopAddr, 0, ref sdu, 0, null);
        }

        private void MacDataRequest(
            UInt64 nextHopAddr,
            ref Frame sdu)
        {
            MacDataRequestReal(true, 0, nextHopAddr, ref sdu, 0, null);
        }

        /// <summary>
        /// Send a Mac SDU using context information. SrcAddr is automatically choosen.
        /// </summary>
        /// <param name="msdu">The frame to sent.</param>
        /// <param name="context">Sent context. If null, frame is broadcasted</param>
        private void MacDataRequestReal(
            bool useExtAddr,
            UInt16 nextHopAddrShort,
            UInt64 nextHopAddrExt,
            ref Frame sdu,
            Byte sduHandle,
            DataConfirmHandler handler)
        {
            // srcAddr: always use short addr when available
            MacAddressingMode srcAddrMode = new MacAddressingMode();
            if (_addrShort == cUnallocatedShortAddr)
                srcAddrMode = MacAddressingMode.ExtendedAddress;
            else
                srcAddrMode = MacAddressingMode.ShortAddress;

            MacAddress dstAddr = new MacAddress();
            if (useExtAddr)
                dstAddr.ExtendedAddress = nextHopAddrExt;
            else
                dstAddr.ShortAddress = nextHopAddrShort;

            byte msduHandle;
            Mac.DataConfirmHandler macHandler;

            bool broadcast = (!useExtAddr) && IsMulticast(nextHopAddrShort);
            if (!broadcast || handler != null)
            { // need a send context
                MessageContext.Context context = _messageContext.GetFreeContext();
                if (context == null)
                { // busy
                    if (handler != null)
                        handler.Invoke(_net, sduHandle, Status.Busy);
                    Frame.Release(ref sdu);
                    return;
                }

                context.useExtAddr = useExtAddr;
                context.nextHopShort = nextHopAddrShort;
                context.nextHopExt = nextHopAddrExt;
                context.dataSduHandle = sduHandle;
                context.dataHandler = handler;

                macHandler = MacDataConfirmHandler;
                msduHandle = context.macSduHandle;
            }
            else
            {
                macHandler = null;
                msduHandle = MessageContext.cHandleDontCare;
            }

            TxOptions tx = new TxOptions();
            if (!broadcast)
                tx.AcknowledgedTransmission = true;

            _mac.DataRequest(
                srcAddrMode,
                dstAddr,
                _panId,
                ref sdu,
                msduHandle,
                tx,
                new SecurityOptions(),
                macHandler);
            Frame.Release(ref sdu);
        }

        private bool IsMulticast(UInt16 shortAddr)
        {
            return shortAddr >= (1 << 15); // see 6LoWPAN Multicast Address Mapping
        }

        private byte IncSeqNoDYMO()
        {
            if (_seqNoDYMO == byte.MaxValue)
                _seqNoDYMO = cSeqNoWraparound;
            else
                _seqNoDYMO++;
            return _seqNoDYMO;
        }

        private byte IncSeqNoBroadcast()
        {
            if (_seqNoBroadcast == byte.MaxValue)
                _seqNoBroadcast = cSeqNoWraparound;
            else
                _seqNoBroadcast++;
            return _seqNoBroadcast;
        }

        private void GetAddressThread()
        {
            if (_getAddressNeighbors != null)
            {
                int length = _getAddressNeighbors.Length;
                for (int retry = 0; retry < 3; retry++)
                {
                    for (int i = 0; i < length; i++)
                    {
                        UInt16 addr = _getAddressNeighbors[i];
                        Message.AddressRequest areq = new Message.AddressRequest();
                        areq.HopsLeft = cDefaultHopLimit;
                        areq.BrokerAddr = cCoordinatorShortAddr;
                        areq.DeviceAddr = _addrExt;
                        Frame frame = Frame.GetFrame(_macHeader, Message.AddressRequest.cLength + _macTailer);
                        if (areq.WriteToFrame(frame))
                        {
                            MacDataRequest(addr, ref frame);
                            if (_getAddressEvent.WaitOne(cAddressRequestTimeout, false))
                            {
                                // success
                                FinalizeStartup(Status.Success);
                                return;
                            }

                            if (_getAddressCancel)
                                return; // abort thread
                        }

                        Frame.Release(ref frame);
                    }
                }
            }

            FinalizeStartup(Status.NoAddress);
        }

        private void SetDiscoveryTimer(UInt16 timeInterval)
        {
            _discoverReplyInterval = timeInterval;

            if (_discoverReplyInterval == 0)
            { // stop timer
                if (_discoverTimer != null)
                {
                    _discoverTimer.Dispose();
                    _discoverTimer = null;
                }
            }
            else
            { // start/change timer
                int dueTime = Random.GetRandom(_discoverReplyInterval) * 1000;
                int period = _discoverReplyInterval * 1000;
                if (_discoverTimer != null)
                {
                    _discoverTimer.Change(dueTime, period);
                }
                else
                {
                    _discoverTimer = new Timer(DiscoverTimerCallback, null, dueTime, period);
                }
            }
        }

        private void DiscoverTimerCallback(object state)
        {
            Message.DiscoveryReply drep = new Message.DiscoveryReply();
            Frame frame = Frame.GetFrame(_netMtu6Low, Message.DiscoveryReply.cLength + _macTailer);
            if (!drep.WriteToFrame(frame))
                return;

            // sent as normal data request, adding mesh header if needed
            _routingTable.DataRequest(cCoordinatorShortAddr, ref frame, 0, null, false);
        }

        #endregion

        #region Mac event handlers
        /// <summary>
        /// Handler for Mac sdu confirmations
        /// </summary>
        private void MacDataConfirmHandler(
                IMacDataSap sender,
                Byte msduHandle,
                MacEnum mStatus)
        {
            MessageContext.Context context = _messageContext.GetContext(msduHandle);
            if (context == null)
                return;

            Status status = Status.Success;
            if (mStatus != MacEnum.Success)
            {
                status = Status.Error;
                if (!context.useExtAddr && mStatus != MacEnum.Congested)
                {
                    // if network is congested, avoid route repairs causing additional traffic
                    Trace.Print("Link failure for 0x" + HexConverter.ConvertUintToHex(context.nextHopShort, 4));
                    _routingTable.HandleLinkFailure(context.nextHopShort);
                }
            }

            if (context.dataHandler != null)
                context.dataHandler.Invoke(_net, context.dataSduHandle, status);

            _messageContext.ReleaseContext(context);
        }

        /// <summary>
        /// Handler for Mac sdu frames
        /// </summary>
        private void MacDataIndHandler(
            IMacDataSap sender,
            MacAddress srcAddr,
            UInt16 srcPanId,
            MacAddress dstAddr,
            UInt16 dstPanId,
            Frame sdu,
            Byte linkQuality,
            Byte DSN,
            UInt32 timeStamp,
            SecurityOptions securityOptions)
        {
            bool ok = true;
            ok &= (srcPanId == _panId && dstPanId == _panId);
            switch (dstAddr.Mode)
            {
                case MacAddressingMode.ShortAddress:
                    ok &= (dstAddr.ShortAddress == _addrShort) || (dstAddr.ShortAddress == cBroadcastShortAddr);
                    break;
                case MacAddressingMode.ExtendedAddress:
                    ok &= (dstAddr.ExtendedAddress == _addrExt);
                    break;
                default:
                    ok &= false;
                    break;
            }

            if (srcAddr.Mode == MacAddressingMode.NoAddress)
                ok = false;
            if (sdu == null || sdu.LengthDataUsed == 0)
                ok = false;

            if (ok)
            {
                Messages6LoWPAN.Dispatch dispatch;
                ok = Messages6LoWPAN.GetType(sdu, out dispatch);
                if (ok)
                {
                    if (dispatch == Messages6LoWPAN.Dispatch.NonLowPan)
                    {
                        Message.Type type;
                        ok = Message.GetType(sdu, out type);
                        if (ok)
                        {
                            switch (type)
                            {
                                case Message.Type.AddressRequest:
                                    HandleAddressRequest(srcAddr, dstAddr, ref sdu);
                                    break;
                                case Message.Type.AddressReply:
                                    HandleAddressReply(srcAddr, dstAddr, ref sdu);
                                    break;
                                case Message.Type.RouteRequest:
                                case Message.Type.RouteReply:
                                    HandleRoutingMessage(srcAddr, dstAddr, ref sdu, linkQuality);
                                    break;
                                case Message.Type.RouteError:
                                    HandleRouteError(srcAddr, dstAddr, ref sdu);
                                    break;
                                case Message.Type.NeighborhoodDiscovery:
                                    HandleNeighborhoodDiscovery(srcAddr, dstAddr, ref sdu, linkQuality);
                                    break;
                                case Message.Type.Data:
                                    if (srcAddr.Mode == MacAddressingMode.ShortAddress && dstAddr.Mode == MacAddressingMode.ShortAddress)
                                    {
                                        HandleData(srcAddr.ShortAddress, dstAddr.ShortAddress, ref sdu);
                                    }
                                    break;
                                case Message.Type.DiscoveryReply:
                                    {
                                        if (srcAddr.Mode == MacAddressingMode.ShortAddress && _isAddrServer)
                                        {
                                            Message.DiscoveryReply drep = new Message.DiscoveryReply();
                                            if (drep.ReadFromFrame(sdu))
                                            {
                                                _addrServer.HandleDiscoveryReply(srcAddr.ShortAddress);
                                            }
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                    else
                    {
                        if (srcAddr.Mode == MacAddressingMode.ShortAddress && dstAddr.Mode == MacAddressingMode.ShortAddress)
                        {
                            HandleData(srcAddr.ShortAddress, dstAddr.ShortAddress, ref sdu);
                        }
                    }
                }
            }

            Frame.Release(ref sdu);
        }

        private void HandleAddressRequest(
            MacAddress srcAddr,
            MacAddress dstAddr,
            ref Frame sdu)
        {
            Message.AddressRequest areq = new Message.AddressRequest();
            if (areq.ReadFromFrame(sdu))
            {
                if (_isAddrServer)
                {
                    // process message as address server
                    Message.AddressReply arep = new Message.AddressReply();
                    if (_addrServer.AllocateAddress(areq.DeviceAddr, out arep.ShortAddr))
                    {
                        // send address
                        arep.HopsLeft = cDefaultHopLimit;
                        arep.BrokerAddr = areq.BrokerAddr;
                        arep.DeviceAddr = areq.DeviceAddr;
                        arep.DiscoveryInterval = _addrServer.TimeInterval;

                        Frame frame = Frame.GetFrame(_macHeader, Message.AddressReply.cLength + _macTailer);
                        if (arep.WriteToFrame(frame))
                        {
                            if (areq.BrokerAddr == cCoordinatorShortAddr)
                            {
                                // direct response
                                if (srcAddr.Mode == MacAddressingMode.ExtendedAddress)
                                {
                                    MacDataRequest(srcAddr.ExtendedAddress, ref frame);
                                }
                            }
                            else
                            {
                                // send to broker
                                _routingTable.DataRequest(areq.BrokerAddr, ref frame, 0, null, true);
                            }
                        }

                        Frame.Release(ref frame);
                    }
                }
                else
                {
                    // forward towards address server
                    if (areq.HopsLeft > 0)
                    {
                        areq.HopsLeft--;
                        if (areq.BrokerAddr == cCoordinatorShortAddr)
                        {
                            areq.BrokerAddr = _addrShort;
                        }

                        Frame frame = Frame.GetFrame(_macHeader, Message.AddressRequest.cLength + _macTailer);
                        if (areq.WriteToFrame(frame))
                        {
                            // send to address server
                            _routingTable.DataRequest(cCoordinatorShortAddr, ref frame, 0, null, true);
                        }

                        Frame.Release(ref frame);
                    }
                }
            }
        }

        private void HandleAddressReply(
            MacAddress srcAddr,
            MacAddress dstAddr,
            ref Frame sdu)
        {
            Message.AddressReply arep = new Message.AddressReply();
            if (arep.ReadFromFrame(sdu))
            {
                if (dstAddr.Mode == MacAddressingMode.ExtendedAddress && dstAddr.ExtendedAddress == _addrExt)
                {
                    // address is for us
                    if (_addrShort == cUnallocatedShortAddr)
                    {
                        _addrShort = arep.ShortAddr;

                        PibValue value = new PibValue();
                        value.Int = _addrShort;
                        _mac.SetRequest(PibAttribute.macShortAddress, 0, value, null);

                        _getAddressEvent.Set();

                        SetDiscoveryTimer(arep.DiscoveryInterval);
                    }
                }
                else
                {
                    if (arep.HopsLeft > 0)
                    {
                        arep.HopsLeft--;
                        Frame frame = Frame.GetFrame(_macHeader, Message.AddressRequest.cLength + _macTailer);
                        if (arep.WriteToFrame(frame))
                        {
                            if (arep.BrokerAddr == _addrShort)
                            {
                                // we are the broker
                                MacDataRequest(arep.DeviceAddr, ref frame);
                            }
                            else
                            {
                                // forward to broker
                                _routingTable.DataRequest(arep.BrokerAddr, ref frame, 0, null, true);
                            }
                        }

                        Frame.Release(ref frame);
                    }
                }
            }
        }

        private void HandleRoutingMessage(
            MacAddress srcAddr,
            MacAddress dstAddr,
            ref Frame sdu,
            byte lqi)
        {
            Message.RoutingMessage rm = new Message.RoutingMessage();
            if (srcAddr.Mode == MacAddressingMode.ShortAddress && rm.ReadFromFrame(sdu))
            {
                Trace.Print("Received: " + rm.ToString());
                rm.HopCount++;

                _neighbourTable.UpdateLqi(srcAddr.ShortAddress, lqi);

                if (rm.OriginatorAddr == _addrShort) // ignore message originating from us
                    return;

                // use bidirectional quality indicator
                if (!_neighbourTable.IsNeighbor(srcAddr.ShortAddress, out lqi))
                    lqi = 0;

                if (lqi < rm.MinLQI)
                    rm.MinLQI = lqi;

                bool usefull = _routingTable.UpdateRoute(rm.OriginatorAddr, srcAddr.ShortAddress,
                    rm.SeqNo, rm.HopCount, rm.MinLQI);

                bool send = false;
                if (rm.TargetAddr == _addrShort)
                {
                    // message directed to us
                    if (usefull && rm.IsRequest)
                    {
                        // respond to message
                        Message.RoutingMessage rrep = new Message.RoutingMessage();
                        rrep.IsRequest = false;
                        rrep.HopsLeft = cDefaultHopLimit;
                        rrep.HopCount = 0;
                        rrep.MinLQI = 0xFF;
                        rrep.SeqNo = _seqNoDYMO;
                        rrep.TargetAddr = rm.OriginatorAddr;
                        rrep.OriginatorAddr = _addrShort;
                        rm = rrep;
                        send = true;
                    }
                }
                else
                {
                    // message directed to someone else.
                    // alays forward route response, forward route request when usefull
                    if (!rm.IsRequest)
                        usefull = true;
                    if (usefull && rm.HopsLeft > 0)
                    {
                        rm.HopsLeft--;
                        send = true;
                    }
                }

                if (send)
                {
                    if (rm.IsRequest)
                    {
                        SendRoutingMessage(rm); // RReq is broadcast
                    }
                    else
                    { // RRep is unicast
                        UInt16 nextHop;
                        if (_routingTable.GetRoute(rm.TargetAddr, out nextHop))
                        {
                            SendRoutingMessage(rm);
                        }
                        else
                        {
                            // no route to forward this message. someone has a stale routing table!
                            SendRouteError(rm.OriginatorAddr, rm.TargetAddr, true /*fatal*/);
                        }
                    }
                }
            }
        }

        private void HandleRouteError(
            MacAddress srcAddr,
            MacAddress dstAddr,
            ref Frame sdu)
        {
            Message.RouteError re = new Message.RouteError();
            if (re.ReadFromFrame(sdu))
            {
                // check message
                Trace.Print("Received " + re.ToString());
                if (re.UnreachableAddr != cInvalidShortAddr && srcAddr.Mode == MacAddressingMode.ShortAddress)
                {
                    bool validRoute = _routingTable.HandleRouteError(re.UnreachableAddr, srcAddr.ShortAddress, re.FatalError);
                    if (validRoute)
                    {
                        if (re.TargetAddr == _addrShort)
                        { // sent to us
                            SendRouteRequest(re.UnreachableAddr);
                        }
                        else
                        { // forward error
                            if (re.HopsLeft > 0)
                            {
                                re.HopsLeft--;
                                SendRouteError(re);
                            }
                        }
                    }
                }
            }
        }

        private void HandleNeighborhoodDiscovery(
            MacAddress srcAddr,
            MacAddress dstAddr,
            ref Frame sdu,
            Byte linkQuality)
        {
            if (srcAddr.Mode == MacAddressingMode.ShortAddress)
            {
                _neighbourTable.ReceiveBeacon(srcAddr.ShortAddress, ref sdu, linkQuality);
            }
        }

        private void HandleData(
            UInt16 srcAddr,
            UInt16 dstAddr,
            ref Frame sdu)
        {
            Messages6LoWPAN.Dispatch dispatch;
            if (!Messages6LoWPAN.GetType(sdu, out dispatch))
                return;

            bool consume = false; // receive locally
            bool forward = false; // forward to another node

            UInt16 origAddr = srcAddr; // originator
            UInt16 targetAddr = dstAddr; // target
            bool haveMeshHeader = false;
            Messages6LoWPAN.MeshHeader mh = new Messages6LoWPAN.MeshHeader();
            if (dispatch == Messages6LoWPAN.Dispatch.Mesh)
            {
                if (!mh.ReadFromFrameHeader(sdu) || !Messages6LoWPAN.GetType(sdu, out dispatch))
                    return;
                haveMeshHeader = true;
                origAddr = mh.originatorAddress;
                targetAddr = mh.finalAddress;
            }

            bool haveBroadcastHeader = false;
            Messages6LoWPAN.BroadcastHeader bh = new Messages6LoWPAN.BroadcastHeader();
            if (dispatch == Messages6LoWPAN.Dispatch.IpPacket && bh.ReadFromFrameHeader(sdu))
            {
                haveBroadcastHeader = true;
                bool usefull = _routingTable.CheckBroadcastSeqNo(origAddr, bh.seqNo) && (origAddr != _addrShort);
                if (!usefull || !haveMeshHeader || !Messages6LoWPAN.GetType(sdu, out dispatch))
                    return;
            }

            bool multicast = IsMulticast(targetAddr);
            consume = (targetAddr == _addrShort || multicast);
            forward = (targetAddr != _addrShort || multicast) && haveMeshHeader && (mh.HopsLeft > 0);

            if (forward)
            {
                mh.HopsLeft--;
                // currently we always need to reallocate a frame to be forwarded, since mac header space is worst case assumption
                Frame frame = Frame.GetFrame(_macHeader + Messages6LoWPAN.MeshHeader.cLengthMax + Messages6LoWPAN.BroadcastHeader.cLength,
                    sdu.LengthDataUsed + _macTailer);
                frame.WriteToBack(sdu);

                bool ok = true;
                UInt16 nextHop;
                if (multicast)
                {
                    nextHop = cBroadcastShortAddr;
                    ok = haveBroadcastHeader && bh.WriteToFrameHeader(frame);
                }
                else
                {
                    bool routeHasError;
                    ok = _routingTable.GetRoute(targetAddr, out nextHop, out routeHasError);
                    if (!ok || routeHasError)
                    {
                        SendRouteError(origAddr, targetAddr, !routeHasError);
                    }
                }

                ok &= mh.WriteToFrameHeader(frame);
                if (ok)
                {
                    MacDataRequest(nextHop, ref frame);
                }

                Frame.Release(ref frame);
            }

            if (consume)
            {
                if (dispatch == Messages6LoWPAN.Dispatch.NonLowPan)
                {
                    Message.Type type;
                    if (Message.GetType(sdu, out type))
                    {
                        switch (type)
                        {
                            case Message.Type.Data:
                                {
                                    Message.Data data = new Message.Data();
                                    if (data.ReadFromFrameHeader(sdu))
                                    {
                                        _data.HandleDataIndication(origAddr, targetAddr, ref sdu);
                                    }
                                    break;
                                }
                            case Message.Type.DiscoveryReply:
                                {
                                    Message.DiscoveryReply drep;
                                    if (_isAddrServer && drep.ReadFromFrame(sdu))
                                    {
                                        _addrServer.HandleDiscoveryReply(origAddr);
                                    }
                                    break;
                                }
                        }
                    }
                }
                else
                { // must be 6LoWPAN frame
                    _data.HandleDataIndication6Low(origAddr, targetAddr, ref sdu);
                }
            }

            Frame.Release(ref sdu);
        }

        #endregion Mac event handlers

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
            if (_neighbourTable != null) _neighbourTable.Dispose();
            if (_addrServer != null) _addrServer.Dispose();
            if (_routingTable != null) _routingTable.Dispose();
            if (_discoverTimer != null) _discoverTimer.Dispose();
        }
    }
}


