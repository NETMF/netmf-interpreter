////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#define MICROFRAMEWORK

using System;
using System.Threading;
using Microsoft.SPOT.Wireless.IEEE_802_15_4;
using Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac;
using Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac.Frames;

namespace Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac
{
    public partial class MacLayer
    {
        /// <summary>
        /// This method processes all requests from upper layer in sequence.
        /// </summary>
        private void TaskProcessor()
        {
            for (; ; )
            {
                _taskAddedEvent.WaitOne();
                if (_taskProcessorExit)
                    return;

                Task task;
                while (_taskQueue.Dequeue(out task))
                {
                    if (task == null)
                        continue;

                    try
                    {
                        switch (task.taskType)
                        {
                            case TaskType.DataRequest:
                                DataRequest(task as TaskDataRequest);
                                break;
                            case TaskType.PurgeRequest:
                                PurgeRequest(task as TaskPurgeRequest);
                                break;
                            case TaskType.AssociateRequest:
                                AssociateRequest(task as TaskAssociateRequest);
                                break;
                            case TaskType.AssociateResponse:
                                AssociateResponse(task as TaskAssociateResponse);
                                break;
                            case TaskType.DisassociateRequest:
                                DisassociateRequest(task as TaskDisassociateRequest);
                                break;
                            case TaskType.GetRequest:
                                GetRequest(task as TaskGetRequest);
                                break;
                            case TaskType.GTSRequest:
                                GTSRequest(task as TaskGtsRequest);
                                break;
                            case TaskType.OrphanResponse:
                                OrphanResponse(task as TaskOrphanResponse);
                                break;
                            case TaskType.ResetRequest:
                                ResetRequest(task as TaskResetRequest);
                                break;
                            case TaskType.RXEnableRequest:
                                RXEnableRequest(task as TaskRxEnableRequest);
                                break;
                            case TaskType.ScanRequest:
                                ScanRequest(task as TaskScanRequest);
                                break;
                            case TaskType.SetRequest:
                                SetRequest(task as TaskSetRequest);
                                break;
                            case TaskType.StartRequest:
                                StartRequest(task as TaskStartRequest);
                                break;
                            case TaskType.SyncRequest:
                                SyncRequest(task as TaskSyncRequest);
                                break;
                            case TaskType.PollRequest:
                                PollRequest(task as TaskPollRequest);
                                break;
                            case TaskType.BeaconRequest:
                                BeaconRequest();
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        // catch all, keep taskProcessor alive
                    }
                }
            }
        }

        private void DataRequest(TaskDataRequest task)
        {
            if (task == null)
                return;

            MacEnum result = MacEnum.Success;
            bool is2006 = false;
            // check frame
            if (task.msdu == null || task.msdu.LengthDataUsed == 0)
            {
                result = MacEnum.InvalidParameter;
            }
            else if (task.msdu.LengthDataUsed > State.aMaxMACSafePayloadSize)
            {
                if (_state.phySupports2006)
                {
                    is2006 = true;
                }
                else
                {
                    result = MacEnum.FrameTooLong;
                }
            }

            if (result == MacEnum.Success)
            {
                // data request is a value type, cannot be null
                Header hdr = new Header();

                hdr.fcs.Type = Frames.Type.Data;
                if (is2006)
                    hdr.fcs.Version = Frames.Version.IEEE2006;
                else
                    hdr.fcs.Version = Frames.Version.IEEE2003;
                if (task.options.AcknowledgedTransmission)
                    hdr.fcs.Ack = true;
                // FIXME: security

                lock (_state)
                {
                    hdr.seqNo = _state.macDSN++;

                    // dst address
                    hdr.dstPanId = task.dstPANId;
                    switch (task.dstAddr.Mode)
                    {
                        case MacAddressingMode.NoAddress:
                            hdr.fcs.DstAddrMode = AddressingMode.None;
                            break;
                        case MacAddressingMode.ShortAddress:
                            hdr.fcs.DstAddrMode = AddressingMode.Short;
                            hdr.dstAddrShort = task.dstAddr.ShortAddress;
                            break;
                        case MacAddressingMode.ExtendedAddress:
                            hdr.fcs.DstAddrMode = AddressingMode.Extended;
                            hdr.dstAddrExt = task.dstAddr.ExtendedAddress;
                            break;
                    }

                    // src addr
                    if (_state.macPanId == task.dstPANId)
                    {
                        hdr.fcs.PanIdCompression = true;
                    }
                    else
                    {
                        hdr.srcPanId = _state.macPanId;
                    }

                    switch (task.srcAddrMode)
                    {
                        case MacAddressingMode.NoAddress:
                            hdr.fcs.SrcAddrMode = AddressingMode.None;
                            break;
                        case MacAddressingMode.ShortAddress:
                            hdr.fcs.SrcAddrMode = AddressingMode.Short;
                            hdr.srcAddrShort = _state.macShortAddr;
                            break;
                        case MacAddressingMode.ExtendedAddress:
                            hdr.fcs.SrcAddrMode = AddressingMode.Extended;
                            hdr.srcAddrExt = _state.aExtendedAddress;
                            break;
                    }
                }

                // encode
                result = MacEnum.Congested;
                if (hdr.WriteToFrameHeader(task.msdu))
                {
                    if (task.msdu.LengthDataUsed > State.aMaxPhyPacketSize)
                    {
                        result = MacEnum.FrameTooLong;
                    }
                    else
                    {
                        result = _sendReceive.SendFrame(ref task.msdu, task.options.AcknowledgedTransmission, hdr.seqNo);
                    }
                }
                else
                {
                    Trace.Print("Mac: DataRequest: cannot add Mac header");
                }
            }

            Frame.Release(ref task.msdu);

            if (task.handler != null)
            {
                task.handler.Invoke(this, task.msduHandle, result);
            }
        }

        private void PurgeRequest(TaskPurgeRequest task)
        {
            if (task == null)
                return;
            // FIXME: not implemented
            Trace.Print("Mac: PurgeRequest: not implemented");
            if (task.handler != null)
            {
                task.handler.Invoke(this, task.msduHandle, MacEnum.NotImplemented);
            }
        }

        private void AssociateRequest(TaskAssociateRequest task)
        {
            if (task == null)
                return;
            // FIXME: not implemented
            Trace.Print("Mac: AssociateRequest: not implemented");
            if (task.handler != null)
            {
                task.handler.Invoke(this, State.cReservedShortAddr, MacEnum.NotImplemented, new SecurityOptions());
            }
        }

        private void AssociateResponse(TaskAssociateResponse task)
        {
            if (task == null)
                return;
            // FIXME: not implemented
            Trace.Print("Mac: AssociateResponse: not implemented");
            // drop
        }

        private void DisassociateRequest(TaskDisassociateRequest task)
        {
            if (task == null)
                return;
            // FIXME: not implemented
            Trace.Print("Mac: DisassociateRequest: not implemented");
            if (task.handler != null)
            {
                task.handler.Invoke(this, MacEnum.NotImplemented, task.deviceAddr, task.devicePanId);
            }
        }

        private void GetRequest(TaskGetRequest task)
        {
            if (task == null)
                return;
            if (task.handler == null)
                return;

            MacEnum status = MacEnum.Success;
            PibValue value = new PibValue();
            lock (_state)
            {
                switch (task.attribute)
                {
                    case PibAttribute.phyCurrentChannel:
                    case PibAttribute.phyChannelsSupported:
                    case PibAttribute.phyTransmitPower:
                    case PibAttribute.phyCCAMode:
                    case PibAttribute.phyCurrentPage:
                    case PibAttribute.phyMaxFrameDuration:
                    case PibAttribute.phySHRDuration:
                    case PibAttribute.phySymbolsPerOctet:
                        {
                            Phy.Status statusPhy;
                            Phy.PibValue valPhy;
                            _phy.GetRequest((Phy.PibAttribute)task.attribute, out statusPhy, out valPhy);

                            if (statusPhy == Phy.Status.Success)
                            {
                                switch (valPhy.Type)
                                {
                                    case Phy.PibValue.ValueType.Int:
                                        value.Int = valPhy.Int;
                                        break;
                                    case Phy.PibValue.ValueType.IntArray:
                                        value.IntArray = valPhy.IntArray;
                                        break;
                                    case Phy.PibValue.ValueType.Float:
                                        value.Float = valPhy.Float;
                                        break;
                                }
                            }
                            else
                            {
                                status = MacEnum.UnsupportedAttribute;
                            }
                            break;
                        }
                    case PibAttribute.macBeaconPayload:
                        {
                            Frame frame = null;
                            Frame payload = _state.macBeaconPayload;
                            if (payload != null)
                            {
                                frame = Frame.GetFrame(payload.LengthDataUsed);
                                frame.WriteToBack(payload);
                            }

                            value.Frame = frame;
                            break;
                        }
                    case PibAttribute.macPanId:
                        {
                            value.Int = _state.macPanId;
                            break;
                        }
                    case PibAttribute.macPromiscuousMode:
                        {
                            value.Bool = _state.macPromiscousMode;
                            break;
                        }
                    case PibAttribute.macShortAddress:
                        {
                            value.Int = _state.macShortAddr;
                            break;
                        }
                    case PibAttribute.macBeaconOrder:
                        {
                            value.Int = _state.macBeaconOrder;
                            break;
                        }
                    case PibAttribute.macSuperframeOrder:
                        {
                            value.Int = _state.macBeaconOrder;
                            break;
                        }
                    case PibAttribute.macMinBE:
                        {
                            value.Int = _state.macMinBE;
                            break;
                        }
                    case PibAttribute.macMaxBE:
                        {
                            value.Int = _state.macMaxBE;
                            break;
                        }
                    case PibAttribute.macMaxCSMABackoffs:
                        {
                            value.Int = _state.macMaxCSMABackoffs;
                            break;
                        }
                    case PibAttribute.macMaxFrameRetries:
                        {
                            value.Int = _state.macMaxFrameRetries;
                            break;
                        }
                    case PibAttribute.macAckWaitDuration:
                    case PibAttribute.macAssociatedPANCoord:
                    case PibAttribute.macAssociationPermit:
                    case PibAttribute.macAutoRequest:
                    case PibAttribute.macBattLifeExt:
                    case PibAttribute.macBattLifeExtPeriods:
                    case PibAttribute.macBeaconTxTime:
                    case PibAttribute.macBSN:
                    case PibAttribute.macCoordExtendedAddress:
                    case PibAttribute.macCoordShortAddress:
                    case PibAttribute.macDSN:
                    case PibAttribute.macGTSPermit:
                    case PibAttribute.macMaxFrameTotalWaitTime:
                    case PibAttribute.macResponseWaitTime:
                    case PibAttribute.macRxOnWhenIdle:
                    case PibAttribute.macSecurityEnabled:
                    case PibAttribute.macSyncSymbolOffset:
                    case PibAttribute.macTimestampSupported:
                    case PibAttribute.macTransactionPersistenceTime:
                        status = MacEnum.UnsupportedAttribute;
                        Trace.Print("MacGetRequest: unsupported attribute");
                        break;
                }
            }

            if (task.handler != null)
            {
                task.handler.Invoke(this, status, task.attribute, task.attributeIndex, value);
            }
        }

        private void GTSRequest(TaskGtsRequest task)
        {
            if (task == null)
                return;
            // FIXME: not implemented
            Trace.Print("Mac: GTSRequest: not implemented");
            if (task.handler != null)
            {
                task.handler.Invoke(this, new GtsCharacteristics(), MacEnum.NotImplemented);
            }
        }

        private void OrphanResponse(TaskOrphanResponse task)
        {
            if (task == null)
                return;
            // FIXME: not implemented
            Trace.Print("Mac: OrphanResponse: not implemented");
            // drop
        }

        private void ResetRequest(TaskResetRequest task)
        {
            if (task == null)
                return;
            _sendReceive.Stop();
            if (task.setDefaultPIB)
            {
                lock (_state)
                {
                    _state.Reset();
                }
            }

            if (task.handler != null)
            {
                task.handler.Invoke(this, MacEnum.Success);
            }
        }

        private void RXEnableRequest(TaskRxEnableRequest task)
        {
            if (task == null)
                return;
            Phy.Status status;
            if (task.rxOnDuration == 0) // disable rx
                _phy.SetTrxStateRequest(Phy.State.TRxOff, out status);
            else // FIXME: poweroff after durtion is not implemented
                _phy.SetTrxStateRequest(Phy.State.RxOn, out status);
            if (task.handler != null)
            {
                task.handler.Invoke(this, MacEnum.Success);
            }
        }

        private void ScanRequest(TaskScanRequest task)
        {
            if (task == null)
                return;
            if (task.handler == null)
                return;

            UInt16 oldPanId;
            lock (_state)
            { // recv thread is holding lock while processing frames. this ensures consistency
                _state.scanning = true;
                oldPanId = _state.macPanId;
                _state.macPanId = State.cBroadcastPanId;
            }

            bool wasRunning = _sendReceive.Running;
            if (!wasRunning)
                _sendReceive.Start();

            // calculate scan duration
            UInt32 durationSymbols = task.scanDuration;
            if (durationSymbols > 14)
                durationSymbols = 14;
            durationSymbols = (UInt32)(State.aBaseSuperframeDuration * ((1 << (int)durationSymbols) + 1));

            // result
            UInt32 unscannedChannels = 0; // bitmap
            byte[] energyDetectList = null;
            PanDescriptor[] panDescriptorList = null;

            // set page
            Phy.Status status;
            Phy.PibValue pibValue = new Phy.PibValue();
            pibValue.Int = task.channelPage;
            _phy.SetRequest(Phy.PibAttribute.phyCurrentPage, pibValue, out status);
            if (status != Phy.Status.Success)
            {
                unscannedChannels = task.scanChannels;
            }
            else
            {
                _state.phyChannelPage = task.channelPage;

                // energyDetectList
                int channelCount = 0;
                if (task.scanType == ScanType.ED)
                {
                    for (int channel = 0; channel < 27; channel++)
                    {
                        if (((1 << channel) & task.scanChannels) != 0)
                        {
                            channelCount++;
                        }
                    }

                    energyDetectList = new byte[channelCount];
                    channelCount = 0;
                }

                // Note: there can be a race between switching the channels here and getting the current channel when
                // receiving beacons in MacReceive. This is solved through thread priorities: recv thread has higher
                // priority and should not be blocked from this thread. There is still a small chance that we switch
                // channel and recv assigns the new (wrong) channel, but this is by design and cannot be changed.

                // iterate through all channels
                for (int channel = 0; channel < 27; channel++)
                {
                    if (((1 << channel) & task.scanChannels) != 0)
                    {
                        // set channel
                        pibValue.Int = channel;
                        _phy.SetRequest(Phy.PibAttribute.phyCurrentChannel, pibValue, out status);
                        if (status != Phy.Status.Success)
                        {
                            unscannedChannels |= (UInt32)(1 << channel);
                            channelCount++;
                        }
                        else
                        {
                            _state.phyChannelNumber = (Byte)channel;
                            // max value of durationSymb is 2^24, correct would be: dur*1000/rate, however this exceeds int range
                            // as symbolrate is multiple of 100, this works:
                            int durationMS = (int)(durationSymbols * 10) / (_state.phySymbolrate / 100); // symbolrate is non-zero

                            // perform the actual channel request
                            switch (task.scanType)
                            {
                                case ScanType.ED:
                                    {
                                        // Continuously read ED values in software for given amount of time.
#if MICROFRAMEWORK
                                        ThreadPriority oldPriority = Thread.CurrentThread.Priority;
                                        Thread.CurrentThread.Priority = ThreadPriority.Highest; // RT thread
#endif
                                        Byte result = 0;
                                        bool resultValid = false;
                                        DateTime dtStart = System.DateTime.Now;
                                        for (; ; )
                                        {
                                            Byte b;
                                            _phy.EDRequest(out status, out b);
                                            if (status == Phy.Status.Success)
                                            {
                                                resultValid = true;
                                                if (b > result)
                                                    result = b;
                                            }

                                            //Thread.Sleep(1);
                                            DateTime dtNow = System.DateTime.Now;
                                            TimeSpan ts = dtNow - dtStart;
                                            int elapsedMS = ts.Milliseconds + 1000 * (ts.Seconds + 60 * ts.Minutes);
                                            if (elapsedMS >= durationMS)
                                                break;
                                        }

#if MICROFRAMEWORK
                                        Thread.CurrentThread.Priority = oldPriority;
#endif
                                        if (resultValid)
                                        {
                                            energyDetectList[channelCount] = result;
                                        }
                                        else
                                        {
                                            unscannedChannels |= (UInt32)(1 << channel);
                                        }

                                        channelCount++;
                                        Thread.Sleep(0); // reschedule
                                        break;
                                    }
                                case ScanType.ActiveScan:
                                case ScanType.PassiveScan:
                                    {
                                        // clear any old beacon
                                        lock (_scannedBeacons)
                                        {
                                            _scannedBeacons.Clear();
                                        }

                                        // send beacon request
                                        if (task.scanType == ScanType.ActiveScan)
                                        {
                                            Header hdr = new Header();
                                            Command.BeaconRequest cmd = new Command.BeaconRequest();
                                            hdr.fcs.Type = Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac.Frames.Type.Cmd;
                                            hdr.fcs.DstAddrMode = AddressingMode.Short;
                                            hdr.dstAddrShort = State.cBroadcastShortAddr;
                                            hdr.dstPanId = State.cBroadcastPanId;
                                            hdr.fcs.SrcAddrMode = AddressingMode.None;
                                            hdr.seqNo = _state.macDSN++;

                                            Frame frame = Encode(hdr, cmd);
                                            if (frame != null)
                                                _sendReceive.SendFrame(ref frame, false, hdr.seqNo);
                                            Frame.Release(ref frame);
                                        }

                                        // wait
                                        Thread.Sleep(durationMS);

                                        // create result list
                                        lock (_scannedBeacons)
                                        {
                                            if (_scannedBeacons.Count > 0)
                                            {
                                                int cntOld = 0;
                                                int cntNew = _scannedBeacons.Count;
                                                if (panDescriptorList == null)
                                                {
                                                    panDescriptorList = new PanDescriptor[cntNew];
                                                }
                                                else
                                                {
                                                    cntOld = panDescriptorList.Length;
                                                    PanDescriptor[] tmp = new PanDescriptor[cntOld + cntNew];
                                                    for (int i = 0; i < cntOld; i++)
                                                        tmp[i] = panDescriptorList[i];
                                                    panDescriptorList = tmp;
                                                }

                                                for (int i = 0; i < cntNew; i++)
                                                    panDescriptorList[i + cntOld] = (PanDescriptor)_scannedBeacons[i];
                                                _scannedBeacons.Clear();
                                            }
                                        }
                                        break;
                                    }
                                case ScanType.OrphanScan:
                                    // FIXME: not implemented
                                    Trace.Print("Mac: ScanRequest: OrphanScan not implemented");
                                    unscannedChannels |= (UInt32)(1 << channel);
                                    break;
                            }
                        }
                    }
                }
            }

            if (!wasRunning)
                _sendReceive.Stop();

            lock (_state)
            {
                _state.scanning = false;
                _state.macPanId = oldPanId;
            }

            task.handler.Invoke(this, MacEnum.Success,
                task.scanType, task.channelPage, unscannedChannels, energyDetectList, panDescriptorList);
        }

        private byte FixParameter(byte value, byte min, byte max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        private void SetRequest(TaskSetRequest task)
        {
            if (task == null)
                return;

            MacEnum status = MacEnum.Success;
            lock (_state)
            {
                switch (task.attribute)
                {
                    case PibAttribute.phyCurrentChannel:
                    case PibAttribute.phyChannelsSupported:
                    case PibAttribute.phyTransmitPower:
                    case PibAttribute.phyCCAMode:
                    case PibAttribute.phyCurrentPage:
                    case PibAttribute.phyMaxFrameDuration:
                    case PibAttribute.phySHRDuration:
                    case PibAttribute.phySymbolsPerOctet:
                        {
                            Phy.Status statusPhy;
                            Phy.PibValue value = new Phy.PibValue();
                            switch (task.value.Type)
                            {
                                case PibValue.ValueType.Int:
                                    value.Int = task.value.Int;
                                    break;
                                case PibValue.ValueType.IntArray:
                                    value.IntArray = task.value.IntArray;
                                    break;
                                case PibValue.ValueType.Float:
                                    value.Float = task.value.Float;
                                    break;
                            }

                            _phy.SetRequest((Phy.PibAttribute)task.attribute, value, out statusPhy);

                            if (statusPhy != Phy.Status.Success)
                            {
                                if (statusPhy == Phy.Status.InvalidParam)
                                    status = MacEnum.InvalidParameter;
                                else
                                    status = MacEnum.UnsupportedAttribute;
                            }
                            break;
                        }
                    case PibAttribute.macBeaconPayload:
                        if (task.value.Type == PibValue.ValueType.Frame)
                        {
                            Frame.Release(ref _state.macBeaconPayload);
                            _state.macBeaconPayload = task.value.Frame;
                        }
                        else
                        {
                            status = MacEnum.InvalidParameter;
                        }
                        break;
                    case PibAttribute.macPanId:
                        if (task.value.Type == PibValue.ValueType.Int)
                        {
                            _state.macPanId = (UInt16)task.value.Int;
                        }
                        else
                        {
                            status = MacEnum.InvalidParameter;
                        }
                        break;
                    case PibAttribute.macPromiscuousMode:
                        if (task.value.Type == PibValue.ValueType.Bool)
                        {
                            _state.macPromiscousMode = task.value.Bool;
                        }
                        else
                        {
                            status = MacEnum.InvalidParameter;
                        }
                        break;
                    case PibAttribute.macShortAddress:
                        if (task.value.Type == PibValue.ValueType.Int)
                        {
                            _state.macShortAddr = (UInt16)task.value.Int;
                        }
                        else
                        {
                            status = MacEnum.InvalidParameter;
                        }
                        break;
                    case PibAttribute.macBeaconOrder:
                        if (task.value.Type == PibValue.ValueType.Int)
                        {
                            _state.macBeaconOrder = FixParameter((byte)task.value.Int, 0, 15);
                        }
                        else
                        {
                            status = MacEnum.InvalidParameter;
                        }
                        break;
                    case PibAttribute.macSuperframeOrder:
                        if (task.value.Type == PibValue.ValueType.Int)
                        {
                            _state.macSuperframeOrder = FixParameter((byte)task.value.Int, 0, 15);
                        }
                        else
                        {
                            status = MacEnum.InvalidParameter;
                        }
                        break;
                    case PibAttribute.macMinBE:
                        if (task.value.Type == PibValue.ValueType.Int)
                        {
                            _state.macMinBE = FixParameter((byte)task.value.Int, 0, _state.macMaxBE);
                        }
                        else
                        {
                            status = MacEnum.InvalidParameter;
                        }
                        break;
                    case PibAttribute.macMaxBE:
                        if (task.value.Type == PibValue.ValueType.Int)
                        {
                            _state.macMaxBE = FixParameter((byte)task.value.Int, 3, 8);
                            if (_state.macMinBE > _state.macMaxBE)
                                _state.macMinBE = _state.macMaxBE;
                        }
                        else
                        {
                            status = MacEnum.InvalidParameter;
                        }
                        break;
                    case PibAttribute.macMaxCSMABackoffs:
                        if (task.value.Type == PibValue.ValueType.Int)
                        {
                            _state.macMaxCSMABackoffs = FixParameter((byte)task.value.Int, 0, 5);
                        }
                        else
                        {
                            status = MacEnum.InvalidParameter;
                        }
                        break;
                    case PibAttribute.macMaxFrameRetries:
                        if (task.value.Type == PibValue.ValueType.Int)
                        {
                            _state.macMaxFrameRetries = FixParameter((byte)task.value.Int, 0, 7);
                        }
                        else
                        {
                            status = MacEnum.InvalidParameter;
                        }
                        break;
                    case PibAttribute.macAckWaitDuration:
                    case PibAttribute.macAssociatedPANCoord:
                    case PibAttribute.macAssociationPermit:
                    case PibAttribute.macAutoRequest:
                    case PibAttribute.macBattLifeExt:
                    case PibAttribute.macBattLifeExtPeriods:
                    case PibAttribute.macBeaconTxTime:
                    case PibAttribute.macBSN:
                    case PibAttribute.macCoordExtendedAddress:
                    case PibAttribute.macCoordShortAddress:
                    case PibAttribute.macDSN:
                    case PibAttribute.macGTSPermit:
                    case PibAttribute.macMaxFrameTotalWaitTime:
                    case PibAttribute.macResponseWaitTime:
                    case PibAttribute.macRxOnWhenIdle:
                    case PibAttribute.macSecurityEnabled:
                    case PibAttribute.macSyncSymbolOffset:
                    case PibAttribute.macTimestampSupported:
                    case PibAttribute.macTransactionPersistenceTime:
                        status = MacEnum.UnsupportedAttribute;
                        Trace.Print("Mac: SetRequest: unsupported attribute");
                        break;
                }
            }

            if (task.handler != null)
            {
                task.handler.Invoke(this, status, task.attribute, task.index);
            }
        }

        private void StartRequest(TaskStartRequest task)
        {
            if (task == null)
                return;

            MacEnum status = MacEnum.Success;
            lock (_state)
            {
                // test short addr
                if (status == MacEnum.Success)
                {
                    if (_state.macShortAddr == State.cReservedShortAddr)
                    {
                        status = MacEnum.NoShortAddress;
                    }
                }

                // test beacon
                if (status == MacEnum.Success)
                {
                    // need to check if beacon does not exceed size when applying given security.
                    // as security is not yet implemented, nothing to do here
                    //MacEnum.FrameTooLong
                }

                // set page
                if (status == MacEnum.Success)
                {
                    if (_state.phyChannelPage != task.channelPage)
                    {
                        int channelPage = task.channelPage;
                        Phy.Status statusPhy;
                        Phy.PibValue value = new Phy.PibValue();
                        value.Int = channelPage;
                        _phy.SetRequest(Phy.PibAttribute.phyCurrentPage, value, out statusPhy);
                        if (statusPhy == Phy.Status.Success)
                        {
                            _state.phyChannelPage = task.channelPage;
                        }
                        else
                        {
                            status = MacEnum.InvalidParameter;
                        }
                    }
                }

                // set channel
                if (status == MacEnum.Success)
                {
                    if (_state.phyChannelNumber != task.logicalChannel)
                    {
                        Phy.Status statusPhy;
                        Phy.PibValue value = new Phy.PibValue();
                        value.Int = task.logicalChannel;
                        _phy.SetRequest(Phy.PibAttribute.phyCurrentChannel, value, out statusPhy);
                        if (statusPhy == Phy.Status.Success)
                        {
                            _state.phyChannelNumber = task.logicalChannel;
                        }
                        else
                        {
                            status = MacEnum.InvalidParameter;
                        }
                    }
                }

                // FIXME: ignoring various parameters for now
                //UInt16 startTime;
                //byte beaconOrder;
                //byte superframeOrder;
                //bool batteryLifeExtension;
                //bool coordRealignment;
                //SecurityOptions coordRealignSecutiryOptions;
                //SecurityOptions beaconSecurityOptions;

                if (status == MacEnum.Success)
                {
                    _state.macPanId = task.panId;
                    _state.panCoordinator = task.panCoordinator;
                    _state.autoBeacon = true;
                    _sendReceive.Start();
                }
            }

            if (task.handler != null)
                task.handler.Invoke(this, status);
        }

        private void SyncRequest(TaskSyncRequest task)
        {
            if (task == null)
                return;
            Trace.Print("Mac: SyncRequest: not implemented");
            // ignore
        }

        private void PollRequest(TaskPollRequest task)
        {
            if (task == null)
                return;
            // FIXME: not implemented
            Trace.Print("Mac: PollRequest: not implemented");
            if (task.handler != null)
            {
                task.handler.Invoke(this, MacEnum.NotImplemented);
            }
        }

        private void BeaconRequest()
        {
            bool send = true;
            Frame frame = null;

            lock (_state)
            {
                if (_state.autoBeacon)
                {
                    Header hdr = new Header();
                    hdr.fcs.SrcAddrMode = AddressingMode.Short;
                    hdr.srcAddrShort = _state.macShortAddr;
                    hdr.srcPanId = _state.macPanId;
                    hdr.seqNo = _state.macBSN++;

                    Beacon bc = new Beacon();
                    bc.beaconOrder = _state.macBeaconOrder;
                    bc.superframeOrder = _state.macSuperframeOrder;
                    // finalCapSlot
                    // batteryLifeExtension
                    if (_state.panCoordinator)
                        bc.panCoordinator = 1;
                    bc.associationPermit = 0;
                    // gtsPermit
                    // gtsDirectionsMask
                    // gtsDescriptor;
                    // shortAddrPending;
                    // extAddrPending;
                    bc.payload = _state.macBeaconPayload;

                    int lenHeader = hdr.Length();
                    int len = _state.phyFrameHead + lenHeader; ;
                    frame = Frame.GetFrame(len + bc.Length() + _state.phyFrameTail);
                    frame.ReserveHeader(len);
                    if (bc.WriteToFrame(frame) && hdr.WriteToFrameHeader(frame))
                    {
                        send = true;
                    }
                }
            }

            if (send)
            {
                _sendReceive.SendFrame(ref frame, false, 0);
            }

            Frame.Release(ref frame);
        }
    }
}


