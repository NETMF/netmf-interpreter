////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#define MICROFRAMEWORK

using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT.Wireless.IEEE_802_15_4;
using Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac;

namespace Microsoft.SPOT.Wireless.IEEE_802_15_4.Network
{
    public class NetworkMgmt
    {
        NetworkLayer _net;
        bool _running; // network is active
        bool _scanning; // a scan is in progress

        UInt16 _panId;
        Byte _logicalChannel;
        Byte _channelPage;
        StartConfirmHandler _startConfirmHandler;
        JoinConfirmHandler _joinConfirmHandler;
        ScanConfirmHandler _scanConfirmHandler; // handler to deliver scan results
        Status _scanStatus; // result of scanning
        ArrayList _scanResults; // ScanResultInternal
        int[] _scanChannels; // supported channels of phy layer, format according to phy api
        int _scanChannelsCurrent; // index to above array for next item to be scanned

        private struct ScanResultInternal
        {
            public PanDescriptor panDescriptor;
            public bool isMeshProtocol;
        }

        public NetworkMgmt(NetworkLayer net)
        {
            _net = net;
            _scanResults = new ArrayList();
        }

        #region public API
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
            if (_running || _scanning)
            {
                if (handler != null)
                    handler.Invoke(_net, Status.Busy, 0, 0, 0);
                return;
            }

            _running = true;
            _panId = panId;
            _startConfirmHandler = handler;
            StartScanning(ScanHandlerStartRequest);
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
            if (_running || _scanning)
            {
                if (handler != null)
                    handler.Invoke(_net, Status.Busy, 0, 0, 0);
                return;
            }

            _running = true;
            _panId = panId;
            _joinConfirmHandler = handler;
            StartScanning(ScanHandlerJoinRequest);
        }

        /// <summary>
        /// Leave network that has been started or joined before.
        /// </summary>
        /// <param name="handler">handler for result</param>
        public void LeaveRequest(
            LeaveConfirmHandler handler)
        {
            if (!_running)
            {
                if (handler != null)
                    handler.Invoke(_net, Status.NotRunning);
                return;
            }

            _net.Routing.Stop();
            _running = false;
            if (handler != null)
                handler.Invoke(_net, Status.Success);
            Trace.Print("Leaving network");
        }

        /// <summary>
        /// To find nearby networks. Result list only include 802.15.4 networks that provide the protocol ID of this protocol in the beacons.
        /// </summary>
        /// <param name="handler">handler for result</param>
        public void ScanRequest(
            ScanConfirmHandler handler)
        {
            if (handler == null)
                return;
            if (_running || _scanning)
            {
                handler.Invoke(_net, Status.Busy, null);
                return;
            }

            StartScanning(handler);
        }

        #endregion

        #region Scanning
        private void StartScanning(
            ScanConfirmHandler handler)
        {
            _scanning = true;
            _scanConfirmHandler = handler; // if scannig is true, handler must be valid!
            _scanResults.Clear();

            // get supported radio channels
            _net.Mac.GetRequest(PibAttribute.phyChannelsSupported, 0, HandlerChannelsSupported);
        }

        // handler for MacGetRequest for supported channels
        private void HandlerChannelsSupported(
            IMacMgmtSap sender,
            MacEnum status,
            PibAttribute attribute,
            int attributeIndex,
            PibValue value)
        {
            if (!_scanning || attribute != PibAttribute.phyChannelsSupported)
                return;

            if (status != MacEnum.Success)
            {
                FinishScanning(Status.Error);
                return;
            }

            _scanChannels = value.IntArray;
            _scanChannelsCurrent = 0;

            ScanNextPage();
        }

        // iterator to scan next channel page
        private void ScanNextPage()
        {
            for (; ; )
            {
                if (_scanChannels == null || _scanChannelsCurrent > _scanChannels.Length - 1) // array starts at 0
                {
                    FinishScanning(Status.Success);
                    return;
                }

                UInt32 channels = (UInt32)_scanChannels[_scanChannelsCurrent];
                UInt32 page = (channels >> 27);
                channels &= 0x07FFFFFF; // remove page
                _scanChannelsCurrent++;

                if (channels != 0) // skip empty pages
                {
                    SecurityOptions so = new SecurityOptions();
                    so.SecurityLevel = SecurityLevelIdentifier.None;

                    // FIXME: how to select scanDuration?
                    byte scanDuration = 5;
                    _net.Mac.ScanRequest(ScanType.ActiveScan, channels, scanDuration, (byte)page, so, HandlerScanResult);
                    return;
                }
            }
        }

        // handler for scan result
        private void HandlerScanResult(
            IMacMgmtSap sender,
            MacEnum status,
            ScanType scanType,
            Byte channelPage,
            UInt32 unscannedChannel,
            byte[] energyDetectList,
            PanDescriptor[] panDescriptorList)
        {
            // store networks
            if (panDescriptorList != null)
            {
                lock (_scanResults)
                {
                    int length = panDescriptorList.Length;
                    for (int i = 0; i < length; i++)
                    {
                        ScanResultInternal res = new ScanResultInternal();
                        res.panDescriptor = panDescriptorList[i];
                        res.isMeshProtocol = false;
                        _scanResults.Add(res);
                    }
                }
            }

            ScanNextPage();
        }

        /// <summary>
        /// handler for mac beacon notifications
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="BSN"></param>
        /// <param name="panDescriptor"></param>
        /// <param name="pendingShortAddrs"></param>
        /// <param name="pendingExtendedAddrs"></param>
        /// <param name="beaconPayload"></param>
        public void MacBeaconNotifyIndication(
            IMacMgmtSap sender,
            ushort BSN,
            PanDescriptor panDescriptor,
            ushort[] pendingShortAddrs,
            UInt64[] pendingExtendedAddrs,
            Frame beaconPayload)
        {
            if (!_scanning)
                return;

            ScanResultInternal res = new ScanResultInternal();
            res.panDescriptor = panDescriptor;
            res.isMeshProtocol = false;

            if (beaconPayload.LengthDataUsed == 8)
            {
                UInt64 meshId = beaconPayload.ReadUInt64Canonical(0);

                if (meshId == NetworkLayer.cMeshId)
                {
                    res.isMeshProtocol = true;
                }

                Frame.Release(ref beaconPayload);
            }

            lock (_scanResults)
            {
                _scanResults.Add(res);
            }
        }

        private void FinishScanning(
            Status status)
        {
            // start actual work from a different thread than Mac callback.
            // Start/join handlers call into Mac and wait for result, so caller cannot run in context of Mac callback.
            // This is a workaround for missing BeginInvoke on MicroFramework
            _scanStatus = status;
            Thread worker = new Thread(FinishScanningThread);
#if !MICROFRAMEWORK
            worker.IsBackground = true;
#endif
            worker.Start();
        }

        private void FinishScanningThread()
        {
            Status status = _scanStatus;
            ScanResult[] networks = null;

            if (status == Status.Success)
            {
                lock (_scanResults)
                {
                    int countMesh = 0;
                    int countAll = _scanResults.Count;
                    for (int i = 0; i < countAll; i++)
                    {
                        ScanResultInternal itemInt = (ScanResultInternal)_scanResults[i];
                        if (itemInt.isMeshProtocol)
                            countMesh++;
                    }

                    if (countMesh > 0)
                    {
                        networks = new ScanResult[countMesh];
                        countMesh = 0;
                        for (int i = 0; i < countAll; i++)
                        {
                            ScanResultInternal itemInt = (ScanResultInternal)_scanResults[i];
                            if (itemInt.isMeshProtocol)
                            {
                                ScanResult itemExt = new ScanResult();
                                itemExt.panId = itemInt.panDescriptor.coordPanId;
                                itemExt.linkQuality = itemInt.panDescriptor.linkQuality;
                                networks[countMesh++] = itemExt;
                            }
                        }
                    }
                }
            }

            ScanConfirmHandler handler = _scanConfirmHandler;
            _scanning = false; // when invoking handler, handler can start next scan
            handler.Invoke(_net, status, networks);
        }

        #endregion

        #region internal handlers
        private void ScanHandlerStartRequest(
            object sender,
            Status status,
            ScanResult[] networks)
        {
            _logicalChannel = 0;
            _channelPage = 0;
            if (status == Status.Success)
            {
                // first check if there are nodes of target network already -> use same channel
                UInt16[] neighborAddr;

                status = FindNetwork(out neighborAddr);
                if (status == Status.NoNeighbor)
                {
                    // perform ED scan
                    bool found = true;
                    byte energy = 0xff; // max
                    AutoResetEvent callbackEvent = new AutoResetEvent(false);

                    Trace.Print("Performing ED scan");

                    int length = _scanChannels.Length;
                    for (int i = 0; i < length; i++)
                    {
                        UInt32 channels = (UInt32)_scanChannels[i];
                        UInt32 page = (channels >> 27);
                        channels &= 0x07FFFFFF; // remove page
                        // FIXME: how to select scanDuration?
                        byte scanDuration = 5;
                        _net.Mac.ScanRequest(ScanType.ED, channels, scanDuration, (byte)page, new SecurityOptions(), delegate(
                            IMacMgmtSap senderDlgt,
                            MacEnum statusDlgt,
                            ScanType scanTypeDlgt,
                            Byte channelPageDlgt,
                            UInt32 unscannedChannelDlgt,
                            byte[] energyDetectListDlgt,
                            PanDescriptor[] panDescriptorListDlgt)
                            {
                                if (statusDlgt == MacEnum.Success && energyDetectListDlgt != null)
                                {
                                    int idx = 0;
                                    for (int c = 0; c < 27; c++)
                                    {
                                        if ((channels & (1 << c)) > 0)
                                        { // channel 'c' was requested
                                            if (((unscannedChannelDlgt & (1 << c)) == 0) && // channel was measured
                                                (idx < energyDetectListDlgt.Length))
                                            { // result is avaiable
                                                Trace.Print("channel " + c + ": energy level " + energyDetectListDlgt[idx]);
                                                if (energyDetectListDlgt[idx] < energy)
                                                { // result is better
                                                    found = true;
                                                    energy = energyDetectListDlgt[idx];
                                                    _logicalChannel = (byte)c;
                                                    _channelPage = channelPageDlgt;
                                                }
                                            }

                                            idx++;
                                        }
                                    }
                                }

                                callbackEvent.Set();
                            });
                        callbackEvent.WaitOne();
                    }

                    if (found)
                    {
                        status = Status.Success;
                        Trace.Print("Starting new network on channel " + _logicalChannel + ", page " + _channelPage);
                    }
                    else
                    {
                        status = Status.Error;
                        Trace.Print("ED scan failed");
                    }
                }

                if (status == Status.Success)
                {
                    Trace.Print("Using Pan Id=0x" + HexConverter.ConvertUintToHex(_panId, 4) +
                        ", logicalChannel=" + _logicalChannel + ", channelPage=" + _channelPage);
                    _net.Routing.Start(_panId, true, _logicalChannel, _channelPage, null, StartHandlerStartRequest);
                    return;
                }
            }

            // scanning failed
            StartHandlerStartRequest(null, status, 0);
        }

        private void StartHandlerStartRequest(
            Routing sender,
            Status status,
            UInt16 shortAddr)
        {
            if (!_running)
                return;
            if (status == Status.Success)
                SetBeaconPayload();
            if (_startConfirmHandler != null)
                _startConfirmHandler.Invoke(_net, status, shortAddr, _logicalChannel, _channelPage);
            if (status != Status.Success)
                _running = false;
        }

        private void ScanHandlerJoinRequest(
            object sender,
            Status status,
            ScanResult[] networks)
        {
            _logicalChannel = 0;
            _channelPage = 0;
            if (status == Status.Success)
            {
                // find properties of target network
                UInt16[] neighborAddr;

                status = FindNetwork(out neighborAddr);
                if (status == Status.Success)
                {
                    Trace.Print("Joining network on Pan Id=0x" + HexConverter.ConvertUintToHex(_panId, 4) +
                        ", logicalChannel=" + _logicalChannel + ", channelPage=" + _channelPage);
                    _net.Routing.Start(_panId, false, _logicalChannel, _channelPage, neighborAddr, StartHandlerJoinRequest);
                    return;
                }
            }

            StartHandlerJoinRequest(null, status, 0);
        }

        // get network parameters from scan result
        private Status FindNetwork(out UInt16[] neighborAddr)
        {
            _logicalChannel = 0;
            _channelPage = 0;
            neighborAddr = null;

            bool found = false;
            int countNeighbors = 0;
            int countAll = _scanResults.Count;
            for (int i = 0; i < countAll; i++)
            {
                ScanResultInternal sr = (ScanResultInternal)_scanResults[i];
                if (sr.panDescriptor.coordPanId == _panId &&
                    sr.isMeshProtocol &&
                    sr.panDescriptor.coordAddr.Mode == MacAddressingMode.ShortAddress)
                {
                    if (!found)
                    {
                        found = true;
                        _logicalChannel = sr.panDescriptor.logicalChannel;
                        _channelPage = sr.panDescriptor.channelPage;
                    }

                    if (_logicalChannel == sr.panDescriptor.logicalChannel &&
                        _channelPage == sr.panDescriptor.channelPage)
                        countNeighbors++;
                }
            }

            if (countNeighbors == 0)
            {
                return Status.NoNeighbor;
            }

            // get neighbors
            neighborAddr = new UInt16[countNeighbors];
            countNeighbors = 0;
            for (int i = 0; i < countAll; i++)
            {
                ScanResultInternal sr = (ScanResultInternal)_scanResults[i];
                if (_panId == sr.panDescriptor.coordPanId &&
                    sr.isMeshProtocol &&
                    sr.panDescriptor.coordAddr.Mode == MacAddressingMode.ShortAddress &&
                    _logicalChannel == sr.panDescriptor.logicalChannel &&
                    _channelPage == sr.panDescriptor.channelPage)
                {
                    neighborAddr[countNeighbors++] = sr.panDescriptor.coordAddr.ShortAddress;
                }
            }

            return Status.Success;
        }

        private void StartHandlerJoinRequest(
            Routing sender,
            Status status,
            UInt16 shortAddr)
        {
            if (!_running)
                return;
            if (status == Status.Success)
                SetBeaconPayload();
            if (_joinConfirmHandler != null)
                _joinConfirmHandler.Invoke(_net, status, shortAddr, _logicalChannel, _channelPage);
            if (status != Status.Success)
                _running = false;
        }

        private void SetBeaconPayload()
        {
            Frame frame = Frame.GetFrame(8);
            frame.AllocBack(8);
            frame.WriteCanonical(0, NetworkLayer.cMeshId);

            PibValue value = new PibValue();
            value.Frame = frame;
            _net.Mac.SetRequest(PibAttribute.macBeaconPayload, 0, value, null);
        }

        #endregion

    }
}


