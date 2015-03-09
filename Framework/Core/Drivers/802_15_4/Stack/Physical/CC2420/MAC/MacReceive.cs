////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using Microsoft.SPOT.Wireless.IEEE_802_15_4.Phy;
using Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac.Frames;

namespace Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac
{
    public partial class MacLayer : IMac, SendReceiveCallbacks
    {
        /// <summary>
        /// process frame from Phy layer. FCS has been removed already.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="frame"></param>
        /// <param name="linkQuality"></param>
        public void PhyDataIndication(
            IPhyDataSap sender,
            ref Frame frame,
            Byte linkQuality)
        {
            Frames.Type type;
            bool ok = Header.GetType(frame, out type);

            if (ok && type == Frames.Type.Data)
            {
                frame.AllocBack(1);
                frame.Write(frame.LengthDataUsed - 1, linkQuality);
                lock (_rxFrameQueue)
                {
                    _rxFrameQueue.AddFrame(ref frame);
                }

                _rxEvent.Set();
                Frame.Release(ref frame);
                return;
            }

            Header hdr = new Header();
            if (hdr.ReadFromFrameHeader(frame, true))
            {
                switch (hdr.fcs.Type)
                {
                    case Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac.Frames.Type.Beacon:
                        ReceiveBeacon(hdr, ref frame, linkQuality);
                        break;
                    case Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac.Frames.Type.Cmd:
                        ReceiveCommand(hdr, ref frame, linkQuality);
                        break;
                    default:
                        // drop, data and ack frames are handled before
                        break;
                }
            }
        }

        /// <summary>
        /// Worker thread to indicate received frames to upper layer sequentially.
        /// </summary>
        private void DataIndicationThread()
        {
            while (true)
            {
                _rxEvent.WaitOne();
                if (_rxThreadExit)
                    break;

                while (true)
                {
                    Frame frame = null;
                    lock (_rxFrameQueue)
                    {
                        frame = _rxFrameQueue.GetFrame();
                    }

                    if (frame == null)
                        break;

                    byte lqi = frame.ReadByte(frame.LengthDataUsed - 1); // pull LQI from frame
                    frame.DeleteFromBack(1);

                    Header hdr = new Header();
                    if (hdr.ReadFromFrameHeader(frame, true))
                    {
                        ReceiveData(hdr, ref frame, lqi);
                    }

                    Frame.Release(ref frame);
                }
            }
        }

        private void ReceiveBeacon(
            Header hdr,
            ref Frame frame,
            Byte linkQuality)
        {
            bool ok = true;
            ok &= hdr.fcs.DstAddrMode == AddressingMode.None;
            ok &= (hdr.fcs.SrcAddrMode == AddressingMode.Short || hdr.fcs.SrcAddrMode == AddressingMode.Extended);
            ok &= (_state.macPromiscousMode || hdr.srcPanId == _state.macPanId || _state.macPanId == State.cBroadcastPanId);
            if (hdr.fcs.Security)
                ok &= (hdr.fcs.Version == Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac.Frames.Version.IEEE2006);
            else
                ok &= (hdr.fcs.Version == Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac.Frames.Version.IEEE2003);
            if (!ok)
                return;

            Beacon beacon = new Beacon();
            if (beacon.ReadFromFrame(ref frame))
            {
                PanDescriptor pd = new PanDescriptor();
                if (hdr.fcs.SrcAddrMode == AddressingMode.Short)
                    pd.coordAddr.ShortAddress = hdr.srcAddrShort;
                else
                    pd.coordAddr.ExtendedAddress = hdr.srcAddrExt;
                pd.coordPanId = hdr.srcPanId;
                pd.logicalChannel = _state.phyChannelNumber;
                pd.channelPage = _state.phyChannelPage;
                //pd.superframeSpec
                pd.gtsPermit = (beacon.gtsPermit > 0);
                pd.linkQuality = linkQuality;
                //pd.timeStamp
                //pd.securityFailure
                //pd.securityOptions

                if (!_state.scanning || beacon.payload != null)
                { // signal upper layer
                    BeaconNotifyIndicationHandler ind = _BeaconNotifyIndication;
                    if (ind != null)
                    {
                        ind.Invoke(this, hdr.seqNo, pd, beacon.shortAddrPending, beacon.extAddrPending, beacon.payload);
                        beacon.payload = null;
                    }
                }

                if (_state.scanning)
                {
                    lock (_scannedBeacons)
                    {
                        _scannedBeacons.Add(pd);
                    }
                }
            }
        }

        private void ReceiveCommand(
            Header hdr,
            ref Frame frame,
            Byte linkQuality)
        {
            bool ok = true;

            ok &= (frame != null && frame.LengthDataUsed > 0);
            ok &= (hdr.fcs.DstAddrMode != AddressingMode.Reserved);
            ok &= (hdr.fcs.SrcAddrMode != AddressingMode.Reserved);
            ok &= (hdr.fcs.DstAddrMode != AddressingMode.None || hdr.fcs.SrcAddrMode != AddressingMode.None);

            if (ok)
            {
                switch (hdr.fcs.DstAddrMode)
                {
                    case AddressingMode.Short:
                        ok &= (hdr.dstAddrShort == _state.macShortAddr || hdr.dstAddrShort == State.cBroadcastShortAddr);
                        ok &= (hdr.dstPanId == _state.macPanId || hdr.dstPanId == State.cBroadcastPanId);
                        break;
                    case AddressingMode.Extended:
                        ok &= (hdr.dstAddrExt == _state.aExtendedAddress);
                        ok &= (hdr.dstPanId == _state.macPanId || hdr.dstPanId == State.cBroadcastPanId);
                        break;
                    case AddressingMode.None:
                        ok &= (_state.panCoordinator);
                        ok &= (hdr.srcPanId == _state.macPanId);
                        break;
                }
            }

            if (ok)
            {
                switch (Command.DecodeType(frame))
                {
                    case Command.Type.AssociationRequest:
                        ReceiveAssociationRequest(hdr, ref frame, linkQuality);
                        break;
                    case Command.Type.AssociationResponse:
                        ReceiveAssociationResponse(hdr, ref frame, linkQuality);
                        break;
                    case Command.Type.DisassociationNotification:
                        ReceiveDisassociationNotification(hdr, ref frame, linkQuality);
                        break;
                    case Command.Type.DataRequest:
                        ReceiveDataRequest(hdr, ref frame, linkQuality);
                        break;
                    case Command.Type.PanIdConflictNotification:
                        ReceivePanIdConflictNotification(hdr, ref frame, linkQuality);
                        break;
                    case Command.Type.OrphanNotification:
                        ReceiveOrphanNotification(hdr, ref frame, linkQuality);
                        break;
                    case Command.Type.BeaconRequest:
                        ReceiveBeaconRequest(hdr, ref frame, linkQuality);
                        break;
                    case Command.Type.CoordinatorRealignment:
                        ReceiveCoordinatorRealignment(hdr, ref frame, linkQuality);
                        break;
                    case Command.Type.GtsRequest:
                        ReceiveGtsRequest(hdr, ref frame, linkQuality);
                        break;
                }
            }
        }

        private void ReceiveData(
            Header hdr,
            ref Frame frame,
            Byte linkQuality)
        {
            MacAddress srcAddr = new MacAddress(); // default: NoAddress
            MacAddress dstAddr = new MacAddress(); // default: NoAddress
            UInt16 srcPanId = State.cBroadcastPanId;
            UInt16 dstPanId = State.cBroadcastPanId;

            bool drop = (hdr.fcs.Version != Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac.Frames.Version.IEEE2003 &&
                hdr.fcs.Version != Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac.Frames.Version.IEEE2006);
            if (!drop)
            {
                switch (hdr.fcs.DstAddrMode)
                {
                    case AddressingMode.None:
                        if (!_state.macPromiscousMode && !_state.panCoordinator)
                        {
                            drop = true;
                        }
                        break;
                    case AddressingMode.Short:
                        if (!_state.macPromiscousMode &&
                            ((hdr.dstAddrShort != _state.macShortAddr && hdr.dstAddrShort != State.cBroadcastShortAddr) ||
                            (hdr.dstPanId != _state.macPanId && hdr.dstPanId != State.cBroadcastPanId)))
                        {
                            drop = true;
                        }
                        else
                        {
                            dstAddr.ShortAddress = hdr.dstAddrShort;
                            dstPanId = hdr.dstPanId;
                        }
                        break;
                    case AddressingMode.Extended:
                        if (!_state.macPromiscousMode &&
                            ((hdr.dstAddrExt != _state.aExtendedAddress ||
                            (hdr.dstPanId != _state.macPanId && hdr.dstPanId != State.cBroadcastPanId))))
                        {
                            drop = true;
                        }
                        else
                        {
                            dstAddr.ExtendedAddress = hdr.dstAddrExt;
                            dstPanId = hdr.dstPanId;
                        }
                        break;
                    case AddressingMode.Reserved:
                        drop = true;
                        break;
                }
            }

            if (!drop)
            {
                bool havePanId = false;
                switch (hdr.fcs.SrcAddrMode)
                {
                    case AddressingMode.None:
                        if (dstAddr.Mode == MacAddressingMode.NoAddress)
                            drop = true; // at least one address must be valid
                        break;
                    case AddressingMode.Short:
                        srcAddr.ShortAddress = hdr.srcAddrShort;
                        havePanId = true;
                        break;
                    case AddressingMode.Extended:
                        srcAddr.ExtendedAddress = hdr.srcAddrExt;
                        havePanId = true;
                        break;
                    case AddressingMode.Reserved:
                        drop = true;
                        break;
                }

                if (havePanId)
                {
                    if (hdr.fcs.PanIdCompression)
                    {
                        if (dstAddr.Mode == MacAddressingMode.NoAddress)
                        {
                            drop = true;
                        }
                        else
                        {
                            srcPanId = dstPanId;
                        }
                    }
                    else
                    {
                        srcPanId = hdr.srcPanId;
                    }
                }
            }

            if (!drop)
            {
                DataIndicationHandler ind = _DataIndication;
                if (ind != null)
                {
                    // synchronous from dataIndicationThread
                    try
                    {
                        ind.Invoke(this, srcAddr, srcPanId, dstAddr, dstPanId,
                            frame, linkQuality, hdr.seqNo, 0, new SecurityOptions());
                    }
                    catch (Exception) { }
                    frame = null;
                }
            }
        }

        private void ReceiveAssociationRequest(
            Header hdr,
            ref Frame frame,
            Byte linkQuality)
        {
            // see 7.3.1
            bool ok = true;
            ok &= (hdr.fcs.Version == Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac.Frames.Version.IEEE2003);
            ok &= (hdr.fcs.SrcAddrMode == AddressingMode.Extended);
            ok &= (hdr.fcs.DstAddrMode == AddressingMode.Extended && hdr.dstAddrExt == _state.aExtendedAddress) ||
                  (hdr.fcs.DstAddrMode == AddressingMode.Short && hdr.dstAddrShort == _state.macShortAddr);
            ok &= (hdr.fcs.PanIdCompression == false);
            ok &= (hdr.dstPanId == _state.macPanId);
            ok &= (hdr.srcPanId == State.cBroadcastPanId);

            Command.AssociationRequest msg = new Command.AssociationRequest();
            if (ok && msg.ReadFromFrame(ref frame))
            {
                AssociateIndicationHandler ind = AssociateIndication;
                if (ind != null)
                    ind.Invoke(this, hdr.srcAddrExt, msg.capabilities, new SecurityOptions());
            }
        }

        private void ReceiveAssociationResponse(
            Header hdr,
            ref Frame frame,
            Byte linkQuality)
        {
            // see 7.3.2
            bool ok = true;
            ok &= (hdr.fcs.Version == Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac.Frames.Version.IEEE2003);
            ok &= (hdr.fcs.DstAddrMode == AddressingMode.Extended && hdr.dstAddrExt == _state.aExtendedAddress);
            ok &= (hdr.fcs.SrcAddrMode == AddressingMode.Extended);
            ok &= (hdr.fcs.PanIdCompression == true);
            ok &= (hdr.dstPanId != State.cBroadcastPanId); // the real PAN ID

            Command.AssociationResponse msg = new Command.AssociationResponse();
            if (ok && msg.ReadFromFrame(ref frame))
            {
                // FIXME: not implemented
            }
        }

        private void ReceiveDisassociationNotification(
            Header hdr,
            ref Frame frame,
            Byte linkQuality)
        {
            // see 7.3.3
            bool ok = true;
            ok &= (hdr.fcs.Version == Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac.Frames.Version.IEEE2003);
            ok &= (hdr.fcs.SrcAddrMode == AddressingMode.Extended);
            ok &= (hdr.fcs.DstAddrMode == AddressingMode.Extended && hdr.dstAddrExt == _state.aExtendedAddress) ||
                (hdr.fcs.DstAddrMode == AddressingMode.Short && hdr.dstAddrShort == _state.macShortAddr);
            ok &= (hdr.fcs.PanIdCompression == true);
            ok &= (hdr.dstPanId == _state.macPanId);

            Command.DisassociationNotification msg = new Command.DisassociationNotification();
            if (ok && msg.ReadFromFrame(ref frame))
            {
                // FIXME: not implemented
            }
        }

        private void ReceiveDataRequest(
            Header hdr,
            ref Frame frame,
            Byte linkQuality)
        {
            // see 7.3.4
            bool ok = true;
            ok &= (hdr.fcs.Version == Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac.Frames.Version.IEEE2003);
            ok &= (hdr.fcs.DstAddrMode == AddressingMode.None && hdr.dstPanId == _state.macPanId) ||
                (hdr.fcs.DstAddrMode == AddressingMode.Extended && hdr.dstAddrExt == _state.aExtendedAddress) ||
                (hdr.fcs.DstAddrMode == AddressingMode.Short && hdr.dstAddrShort == _state.macShortAddr);
            ok &= (hdr.fcs.SrcAddrMode != AddressingMode.None);

            Command.DataRequest msg = new Command.DataRequest();
            if (ok && msg.ReadFromFrame(ref frame))
            {
                // FIXME: not implemented
            }
        }

        private void ReceivePanIdConflictNotification(
            Header hdr,
            ref Frame frame,
            Byte linkQuality)
        {
            // see 7.3.5
            bool ok = true;
            ok &= (hdr.fcs.Version == Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac.Frames.Version.IEEE2003);
            ok &= (hdr.fcs.DstAddrMode == AddressingMode.Extended);
            ok &= (hdr.fcs.SrcAddrMode == AddressingMode.Extended);
            ok &= (hdr.fcs.PanIdCompression == true);
            ok &= (hdr.dstPanId == _state.macPanId);

            Command.PanIdConflictNotification msg = new Command.PanIdConflictNotification();
            if (ok && msg.ReadFromFrame(ref frame))
            {
                // FIXME: not implemented
            }
        }

        private void ReceiveOrphanNotification(
            Header hdr,
            ref Frame frame,
            Byte linkQuality)
        {
            // see 7.3.6
            bool ok = true;
            ok &= (hdr.fcs.Version == Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac.Frames.Version.IEEE2003);
            ok &= (hdr.fcs.SrcAddrMode == AddressingMode.Extended);
            ok &= (hdr.fcs.DstAddrMode == AddressingMode.Short);
            ok &= (hdr.fcs.PanIdCompression == true);
            ok &= (hdr.dstPanId == State.cBroadcastPanId);
            ok &= (hdr.dstAddrShort == State.cBroadcastShortAddr);

            Command.OrphanNotification msg = new Command.OrphanNotification();
            if (ok && msg.ReadFromFrame(ref frame))
            {
                // FIXME: not implemented
            }
        }

        private void ReceiveBeaconRequest(
            Header hdr,
            ref Frame frame,
            Byte linkQuality)
        {
            // see 7.3.7
            bool ok = true;
            ok &= (hdr.fcs.Version == Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac.Frames.Version.IEEE2003);
            ok &= (hdr.fcs.DstAddrMode == AddressingMode.Short);
            ok &= (hdr.fcs.SrcAddrMode == AddressingMode.None);
            ok &= (hdr.dstPanId == State.cBroadcastPanId);
            ok &= (hdr.dstAddrShort == State.cBroadcastShortAddr);

            Command.BeaconRequest msg = new Command.BeaconRequest();
            if (ok && msg.ReadFromFrame(ref frame))
            {
                TaskBeaconRequest task = new TaskBeaconRequest();
                _taskQueue.Add(task);
            }
        }

        private void ReceiveCoordinatorRealignment(
            Header hdr,
            ref Frame frame,
            Byte linkQuality)
        {
            // see 7.3.8
            bool ok = true;
            ok &= (hdr.fcs.Version == Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac.Frames.Version.IEEE2003 ||
                hdr.fcs.Version == Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac.Frames.Version.IEEE2006);
            ok &= (hdr.fcs.DstAddrMode != AddressingMode.None);
            ok &= (hdr.fcs.SrcAddrMode == AddressingMode.Extended);
            ok &= (hdr.dstPanId == State.cBroadcastPanId);
            ok &= (hdr.fcs.DstAddrMode == AddressingMode.Extended && hdr.dstAddrExt == _state.aExtendedAddress) ||
                (hdr.fcs.DstAddrMode == AddressingMode.Short && hdr.dstAddrShort == _state.macShortAddr);
            ok &= (hdr.fcs.PanIdCompression == false);

            Command.CoordinatorRealignment msg = new Command.CoordinatorRealignment();
            if (ok && msg.ReadFromFrame(ref frame))
            {
                // FIXME: not implemented
            }
        }

        private void ReceiveGtsRequest(
            Header hdr,
            ref Frame frame,
            Byte linkQuality)
        {
            // see 7.3.9
            bool ok = true;
            ok &= (_state.panCoordinator == true);
            ok &= (hdr.fcs.Version == Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac.Frames.Version.IEEE2003);
            ok &= (hdr.fcs.DstAddrMode == AddressingMode.None);
            ok &= (hdr.fcs.SrcAddrMode == AddressingMode.Short);
            ok &= (hdr.srcPanId == _state.macPanId);

            Command.GtsRequest msg = new Command.GtsRequest();
            if (ok && msg.ReadFromFrame(ref frame))
            {
                // FIXME: not implemented
            }
        }
    }
}


