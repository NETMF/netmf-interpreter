////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#define MICROFRAMEWORK

// error CS0414: The field '<identifier>' is assigned but its value is never used
#pragma warning disable 0414

using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT.Wireless.IEEE_802_15_4.Phy;
using Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac.Frames;

namespace Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac
{
    /// <summary>
    /// This is a partial implementation of the 802.15.4 Mac layer.
    /// Supported:
    /// - direct data transfer
    /// - scanning
    /// - beacons
    /// - crucial attributes
    /// Not supported:
    /// - associations, including orphan handling and indirect data transfers
    /// - GTS/superframes/synchronisation
    /// - security modes
    /// - PAN ID conflict resolution
    /// </summary>
    public partial class MacLayer : IMac, SendReceiveCallbacks, StateCallbacks,IDisposable
    {
        private int cQueueCmdSize = 10;
        private int cQueueDataSize = 32;
        private int cQueueRxSize = 32;

        private State _state;
        private IPhy _phy;
        private bool _phyCapPower;
        private AutoResetEvent _taskAddedEvent;
        private TaskQueue _taskQueue;
        private bool _taskProcessorExit;
        private Thread _taskProcessor;
        private PhySendReceive _sendReceive;
        private ArrayList _scannedBeacons;

        // data frames are queued again and indicated one after each other through the receive thread.
        // if the queue is full, frames are dropped.
        // this mechanism intentionally discards frames if the receiving rate is higher than the processing rate.
        private Frame.Queue _rxFrameQueue;  // queue of received frames
        private Thread _rxThread;          // A thread that handled new frames
        private AutoResetEvent _rxEvent;   // An event to signal a newly added raw frame
        private bool _rxThreadExit;        // A flag that tells the receive thread to exit

        public MacLayer(IPhy phy)
        {
            // poweron Phy first
            _phy = phy;
            _phy.IsCapabilitySupported(Capabilities.PowerOff, out _phyCapPower);
            if (_phyCapPower)
            {
                _phy.SetPower(true); // should be on-demand
            }

            _state = new State(this, phy);
            _taskAddedEvent = new AutoResetEvent(false);
            _taskQueue = new TaskQueue(cQueueCmdSize, cQueueDataSize, _taskAddedEvent);
            _taskProcessorExit = false;
            _taskProcessor = new Thread(TaskProcessor);
#if !MICROFRAMEWORK
            _taskProcessor.IsBackground = true;
#endif
            _sendReceive = new PhySendReceive(_state, _phy, this);
            _scannedBeacons = new ArrayList();

            ResetHandler();

            // initialize channels
            Phy.Status status;
            Phy.PibValue value;
            _phy.GetRequest(Phy.PibAttribute.phyCurrentPage, out status, out value);
            _state.phyChannelPage = (Byte)value.Int;
            _phy.GetRequest(Phy.PibAttribute.phyCurrentChannel, out status, out value);
            _state.phyChannelNumber = (Byte)value.Int;

            _rxFrameQueue = new Frame.Queue(cQueueRxSize, true, 0);
            _rxThread = new Thread(DataIndicationThread);
#if !MICROFRAMEWORK
            _rxThread.IsBackground = true;
#endif
            _rxEvent = new AutoResetEvent(false);
            _rxThreadExit = false;
            _rxThread.Start();

            _taskProcessor.Start();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Close()
        {
            Dispose();
        }     

        protected virtual void Dispose(bool disposing)
        {
            if (_taskProcessor != null)
            {
                _taskProcessorExit = true;
                _taskAddedEvent.Set();
                _taskProcessor.Join();
            }

            _rxThreadExit = true;
            if (_rxThread != null)
            {
                _rxEvent.Set();
                _rxThread.Join();
                _rxThread = null;
            }

#if !MICROFRAMEWORK
            if (_rxEvent != null) {
                _rxEvent.Close();
            }

            if (_taskAddedEvent != null) {
                _taskAddedEvent.Close();
            }    
#endif
        }


        private void ResetHandler()
        {
            _DataIndication = null;
            AssociateIndication = null;
            DisassociateIndication = null;
            _BeaconNotifyIndication = null;
            GtsIndication = null;
            OrphanIndication = null;
            CommStatusIndication = null;
            SyncLossIndication = null;
        }

        #region events/callbacks
        private DataIndicationHandler _DataIndication;
        public DataIndicationHandler DataIndication
        {
            get { return _DataIndication; }
            set { _DataIndication = value; }
        }

        public event AssociateIndicationHandler AssociateIndication;
        public event DisassociateIndicationHandler DisassociateIndication;

        private BeaconNotifyIndicationHandler _BeaconNotifyIndication;
        public BeaconNotifyIndicationHandler BeaconNotifyIndication
        {
            get { return _BeaconNotifyIndication; }
            set { _BeaconNotifyIndication = value; }
        }

        public event GtsIndicationHandler GtsIndication;
        public event OrphanIndicationHandler OrphanIndication;
        public event CommStatusIndicationHandler CommStatusIndication;
        public event SyncLossIndicationHandler SyncLossIndication;
        #endregion

        #region task queueing
        public void DataRequest(
            MacAddressingMode srcAddrMode,
            MacAddress dstAddr,
            UInt16 dstPanId,
            ref Frame msdu,
            Byte msduHandle,
            TxOptions options,
            SecurityOptions securityOptions,
            DataConfirmHandler handler)
        {
            TaskDataRequest task = new TaskDataRequest(
                srcAddrMode,
                dstAddr,
                dstPanId,
                msdu,
                msduHandle,
                options,
                securityOptions,
                handler);
            if (!_taskQueue.Add(task))
            {
                Frame.Release(ref msdu);
                if (handler != null)
                    handler.Invoke(this, msduHandle, MacEnum.Congested);
            }

            msdu = null; // in any case, remove frame ownership from caller
        }

        public void PurgeRequest(
            Byte msduHandle,
            PurgeConfirmHandler handler)
        {
            TaskPurgeRequest task = new TaskPurgeRequest(
                msduHandle,
                handler);
            if (!_taskQueue.Add(task) && handler != null)
            {
                handler.Invoke(this, msduHandle, MacEnum.Congested);
            }
        }

        public void AssociateRequest(
            Byte logicalChannel,
            Byte channelPage,
            MacAddress coordAddr,
            UInt16 coordPanId,
            CapabilityInformation capability,
            SecurityOptions securityOptions,
            AssociateConfirmHandler handler)
        {
            TaskAssociateRequest task = new TaskAssociateRequest(
                logicalChannel,
                channelPage,
                coordAddr,
                coordPanId,
                capability,
                securityOptions,
                handler);
            if (!_taskQueue.Add(task) && handler != null)
            {
                handler.Invoke(this, State.cReservedShortAddr, MacEnum.Congested, new SecurityOptions());
            }
        }

        public void AssociateResponse(
            UInt64 deviceAddress,
            UInt16 assocShortAddress,
            MacEnum status,
            SecurityOptions securityOptions)
        {
            TaskAssociateResponse task = new TaskAssociateResponse(
                deviceAddress,
                assocShortAddress,
                status,
                securityOptions);
            _taskQueue.Add(task);
        }

        public void DisassociateRequest(
            MacAddress deviceAddr,
            UInt16 devicePanId,
            DisassociationReason reason,
            bool txIndirect,
            SecurityOptions securityOptions,
            DisassociateConfirmHandler handler)
        {
            TaskDisassociateRequest task = new TaskDisassociateRequest(
                deviceAddr,
                devicePanId,
                reason,
                txIndirect,
                securityOptions,
                handler);
            if (!_taskQueue.Add(task) && handler != null)
            {
                handler.Invoke(this, MacEnum.Congested, new MacAddress(), 0);
            }
        }

        public void GetRequest(
            PibAttribute attribute,
            int attributeIndex,
            GetConfirmHandler handler)
        {
            TaskGetRequest task = new TaskGetRequest(
                attribute,
                attributeIndex,
                handler);
            if (!_taskQueue.Add(task) && handler != null)
            {
                handler.Invoke(this, MacEnum.Congested, attribute, attributeIndex, new PibValue());
            }
        }

        public void GtsRequest(
            GtsCharacteristics gtsCharacteristics,
            SecurityOptions securityOptions,
            GtsConfirmHandler handler)
        {
            TaskGtsRequest task = new TaskGtsRequest(
                gtsCharacteristics,
                securityOptions,
                handler);
            if (!_taskQueue.Add(task) && handler != null)
            {
                handler.Invoke(this, gtsCharacteristics, MacEnum.Congested);
            }
        }

        public void OrphanResponse(
            UInt64 orphanAddress,
            UInt16 shortAddr,
            bool associatedMember,
            SecurityOptions securityOptions)
        {
            TaskOrphanResponse task = new TaskOrphanResponse(
                orphanAddress,
                shortAddr,
                associatedMember,
                securityOptions);
            _taskQueue.Add(task);
        }

        public void ResetRequest(
            bool setDefaultPIB,
            ResetConfirmHandler handler)
        {
            TaskResetRequest task = new TaskResetRequest(
                setDefaultPIB,
                handler);
            if (!_taskQueue.Add(task) && handler != null)
            {
                handler.Invoke(this, MacEnum.Congested); // should never happen. reset should alyays work
            }
        }

        public void RxEnableRequest(
            bool deferPermit,
            UInt16 rxOnTime,
            UInt16 rxOnDuration,
            RxEnableConfirmHandler handler)
        {
            TaskRxEnableRequest task = new TaskRxEnableRequest(
                deferPermit,
                rxOnTime,
                rxOnDuration,
                handler);
            if (!_taskQueue.Add(task) && handler != null)
            {
                handler.Invoke(this, MacEnum.Congested);
            }
        }

        public void ScanRequest(
            ScanType scanType,
            UInt32 scanChannels,
            byte scanDuration,
            byte channelPage,
            SecurityOptions securityOptions,
            ScanChannelConfirmHandler handler)
        {
            TaskScanRequest task = new TaskScanRequest(
                scanType,
                scanChannels,
                scanDuration,
                channelPage,
                securityOptions,
                handler);
            if (!_taskQueue.Add(task) && handler != null)
            {
                handler.Invoke(this, MacEnum.Congested, scanType, channelPage, scanChannels, null, null);
            }
        }

        public void SetRequest(
            PibAttribute attribute,
            int index,
            PibValue value,
            SetConfirmHandler handler)
        {
            TaskSetRequest task = new TaskSetRequest(
                attribute,
                index,
                value,
                handler);
            if (!_taskQueue.Add(task) && handler != null)
            {
                handler.Invoke(this, MacEnum.Congested, attribute, index);
            }
        }

        public void StartRequest(
            UInt16 panId,
            Byte logicalChannel,
            Byte channelPage,
            UInt16 startTime,
            byte beaconOrder,
            byte superframeOrder,
            bool panCoordinator,
            bool batteryLifeExtension,
            bool coordRealignment,
            SecurityOptions coordRealignSecutiryOptions,
            SecurityOptions beaconSecurityOptions,
            StartConfirmHandler handler)
        {
            TaskStartRequest task = new TaskStartRequest(
                panId,
                logicalChannel,
                channelPage,
                startTime,
                beaconOrder,
                superframeOrder,
                panCoordinator,
                batteryLifeExtension,
                coordRealignment,
                coordRealignSecutiryOptions,
                beaconSecurityOptions,
                handler);
            if (!_taskQueue.Add(task) && handler != null)
            {
                handler.Invoke(this, MacEnum.Congested);
            }
        }

        public void SyncRequest(
            ushort logicalChannel,
            ushort logicalPage,
            bool trackBeacon)
        {
            TaskSyncRequest task = new TaskSyncRequest(
                logicalChannel,
                logicalPage,
                trackBeacon);
            _taskQueue.Add(task);
        }

        public void PollRequest(
            MacAddress coordAddr,
            UInt16 coordPanId,
            SecurityOptions securityOptions,
            PollConfirmHandler handler)
        {
            TaskPollRequest task = new TaskPollRequest(
                coordAddr,
                coordPanId,
                securityOptions,
                handler);
            if (!_taskQueue.Add(task) && handler != null)
            {
                handler.Invoke(this, MacEnum.Congested);
            }
        }

        #endregion

        #region IMacExtSap
        /// <summary>
        /// Get required header space for data frames to be sent
        /// </summary>
        public void GetMtuSize(out int mtu, out int head, out int tail)
        {
            _phy.GetMtuSize(out mtu, out head, out tail);

            bool b;
            _phy.IsCapabilitySupported(Capabilities.Ieee2006, out b);
            if (b)
            { // if header is small, payload can be larger
                mtu = State.aMaxMacPayloadSize;
            }
            else
            {
                mtu = State.aMaxMACSafePayloadSize;
            }

            head += State.aMaxFrameOverhead;
            tail += 2; // FCS
        }

        /// <summary>
        /// Get extended device address
        /// </summary>
        /// <returns></returns>
        public void GetDeviceAddress(out UInt64 address)
        {
            address = _state.aExtendedAddress;
        }

        #endregion IMacExtSap

        #region SendReceiveCallbacks
        public void SetPhyAddressFilter(
                   bool enable,
                   UInt16 shortAddr,
                   UInt16 panId,
                   bool panCoordinator)
        {
            if (_sendReceive != null)
                _sendReceive.SetAddressFilter(enable, shortAddr, panId, panCoordinator);
        }

        #endregion

        private Frame Encode(Header header, Command.Base cmd)
        {
            int lenHeader = header.Length();
            int len = _state.phyFrameHead + lenHeader; ;
            Frame frame = Frame.GetFrame(len, cmd.LengthMax + _state.phyFrameTail);
            if (!cmd.WriteToFrame(frame) || !header.WriteToFrameHeader(frame))
            {
                Frame.Release(ref frame);
                Trace.Print("Mac: unable to create command frame");
                return null;
            }

            return frame;
        }
    }
}


