////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#define MICROFRAMEWORK

using System;
using System.Threading;
using Microsoft.SPOT.Wireless.IEEE_802_15_4.Phy;
using Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac;
using Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac.Frames;

namespace Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac
{
    interface SendReceiveCallbacks
    {
        void PhyDataIndication(
            IPhyDataSap sender,
            ref Frame frame,
            Byte linkQuality);
    }

    /// <summary>
    /// This class is responsible for:
    /// - receiving frames from Phy layer and filter them for relevant frames
    /// - implementing sending, including CSMA/CA and waiting for ack'es
    /// - FCS calculation
    /// This class is meant to be called from a single thread only to keep PHY layer in consistent state.
    /// </summary>
    class PhySendReceive
    {
        State _state;
        readonly IPhy _phy;
        readonly SendReceiveCallbacks _mac;

        bool _ackPending; // waiting for ack of recently sent frame
        byte _ackSeqNo; // seqNo of pending ack
        AutoResetEvent _ackReceived; // set when ack was received

        bool _phyCapAutoFCS;
        bool _phyCapAddressFilter;
        bool _phyCapAutoAck;
        bool _phyCapTxCache;

        bool _running;

        bool _filterEnabled;
        UInt16 _filterPanId;
        UInt16 _filterShortAddr;
        bool _filterCoordinator;

        public PhySendReceive(State state, IPhy phy, SendReceiveCallbacks mac)
        {
            _state = state;
            _phy = phy;
            _mac = mac;

            _ackPending = false;
            _ackReceived = new AutoResetEvent(false);

            _phy.IsCapabilitySupported(Capabilities.AutoFcs, out _phyCapAutoFCS);
            if (_phyCapAutoFCS)
            {
                _phy.SetAutoFCS(true);
            }

            _phy.IsCapabilitySupported(Capabilities.AddressFilter, out _phyCapAddressFilter);
            _phy.IsCapabilitySupported(Capabilities.AutoAck, out _phyCapAutoAck);
            _phy.IsCapabilitySupported(Capabilities.TxCache, out _phyCapTxCache);

            _filterEnabled = false;
        }

        /// <summary>
        /// Sets address filter on Phy layer
        /// </summary>
        /// <param name="enable">true if enabled</param>
        /// <param name="shortAddr">local short addr, 0xFFFF for any</param>
        /// <param name="panId">local pan ID, 0xFFFF for any</param>
        /// <param name="panCoordinator">true if device is the PAN coordinator</param>
        public void SetAddressFilter(
            bool enable,
            UInt16 shortAddr,
            UInt16 panId,
            bool panCoordinator)
        {
            if (_phyCapAddressFilter)
            {
                _phy.SetAddressFilter(enable, shortAddr, panId, panCoordinator);
                _phy.SetAutoAck(enable);
            }
            else
            {
                _filterEnabled = enable;
                _filterShortAddr = shortAddr;
                _filterPanId = panId;
                _filterCoordinator = panCoordinator;
            }
        }

        /// <summary>
        /// Indicates if object has been started
        /// </summary>
        public bool Running
        {
            get { return _running; }
        }

        /// <summary>
        /// Enabled receiving of frames from Phy
        /// </summary>
        public void Start()
        {
            if (!_running)
            {
                _running = true;
                _phy.DataIndication = PhyDataIndicationHandler;
                Phy.Status status;
                _phy.SetTrxStateRequest(Phy.State.RxOn, out status);
            }
        }

        /// <summary>
        /// Disabled receiving frame from Phy
        /// </summary>
        public void Stop()
        {
            if (_running)
            {
                _running = false;
                Phy.Status status;
                _phy.SetTrxStateRequest(Phy.State.TRxOff, out status);
                _phy.DataIndication = null;
            }
        }

        /// <summary>
        /// handler for data from Phy
        /// </summary>
        private void PhyDataIndicationHandler(
            IPhyDataSap sender,
            Frame frame,
            Byte linkQuality)
        {
            if (frame == null)
                return;

            if (frame.LengthDataUsed >= 3)
            { // min frame size
                // inspect frame type, light-weight decoding
                Frames.Type type;
                bool ok = Header.GetType(frame, out type);

                if (ok)
                {
                    switch (type)
                    {
                        case Frames.Type.Ack:
                            if (_ackPending)
                            {
                                // All other subfields than fcs.pending shall be set to zero and ignored on reception
                                byte seqNo = frame.ReadByte(2); // ack frame is FCF+seqNo
                                if (seqNo == _ackSeqNo)
                                    _ackReceived.Set();
                            }
                            break;
                        case Frames.Type.Beacon:
                        case Frames.Type.Data:
                        case Frames.Type.Cmd:
                            lock (_state)
                            { // keep state consistent while processing frame
                                if (_filterEnabled)
                                {
                                    // FIXME: SW filter is not implemented
                                }
                                else
                                {
                                    _mac.PhyDataIndication(sender, ref frame, linkQuality);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            Frame.Release(ref frame);
        }

        /// <summary>
        /// sends a frame using Phy, implements CCA, CSMA and waiting for frame acknowledgment
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="ackRequest"></param>
        /// <param name="seqNo"></param>
        /// <returns></returns>
        public MacEnum SendFrame(ref Frame frame, bool ackRequest, byte seqNo)
        {
            // calc fcs
            if (!_phyCapAutoFCS)
            {
                return MacEnum.NotImplemented;
                // software FCS is not implemented. to implement, fcs needs to be computed and appeneded to frame here
            }

#if MICROFRAMEWORK
            ThreadPriority oldPriority = Thread.CurrentThread.Priority;
# if RTEXTENSIONS
            Thread.CurrentThread.Priority = ThreadPriority.Highest; // RT thread
# else
            Thread.CurrentThread.Priority = (ThreadPriority)150000; // fake RT priority
# endif
#endif

            int symbolrate = _state.phySymbolrate;
            int macMaxFrameRetries = _state.macMaxFrameRetries;
            int macMaxCSMABackoffs = _state.macMaxCSMABackoffs;
            int macMinBE = _state.macMinBE;
            // loop until frame is ack'ed
            bool noClearChannel = true;
            bool ackPending = true;
            for (int retry = 0; ackPending && retry <= macMaxFrameRetries; retry++)
            {
                // loop for CCA
                int be = macMinBE;
                for (int nb = 0; nb <= macMaxCSMABackoffs; nb++)
                {
                    // Backoff periods: backoff exponent is 0..8
                    // period is (2^BE -1) * aUnitBackoffPeriod, with aUnitBackoffPeriod = 20 symbols = 0.32 msec (for 2.4GHz)
                    int backoff = ((1 << be) - 1) * State.aUnitBackoffPeriod; // symbols
                    backoff = Random.GetRandom(backoff + 1);
                    backoff = (backoff * 1000 * 1000) / symbolrate; // microseconds
                    backoff = (backoff + 500) / 1000; // milliseconds, rounded-up
                    if (backoff == 0 && nb > 0)
                        backoff++;
                    if (backoff > 0)
                        Thread.Sleep(backoff);

                    Phy.Status status;
                    _phy.CCARequest(out status);
                    if (status == Phy.Status.Idle)
                    {
                        _phy.SetTrxStateRequest(Phy.State.TxOn, out status);
                        _phy.DataRequest(frame, out status);
                        _phy.SetTrxStateRequest(Phy.State.RxOn, out status);
                        if (ackRequest)
                        {
                            _ackSeqNo = seqNo;
                            _ackPending = true; // signal recv thread to check for acks

                            int waitDurationMs = _state.AckWaitDuration(frame.LengthDataUsed); // micro seconds
                            waitDurationMs = (waitDurationMs) / 1000; // convert to milliseconds
#if MICROFRAMEWORK
                            // real PHY implementation, hard real-time
                            // works suprisingly without rounding up for all frame sizes
#else
                            // pseudo-PHY implementation, soft real-time
                            waitDurationMs += 3;
#endif

                            if (_ackReceived.WaitOne(waitDurationMs, false))
                            {
                                ackPending = false; // received the ack
                            }
                            else
                            {
                                _phy.DataPoll();
                                if (_ackReceived.WaitOne(0, false))
                                {
                                    ackPending = false; // received the ack
                                }
                                else
                                {
                                    //Trace.Print("ACK timeout");
                                }
                            }

                            _ackPending = false; // signal recv thread to not check for acks
                        }
                        else
                        {
                            ackPending = false; // not waiting for ack at all
                        }

                        noClearChannel = false;
                        break; // exit CCA loop
                    }

                    if (be < _state.macMaxBE)
                        be++;
                }
            }

            Frame.Release(ref frame);
            if (_phyCapTxCache)
                _phy.ClearTxBuffer();

#if MICROFRAMEWORK
            Thread.CurrentThread.Priority = oldPriority;
#endif

            if (noClearChannel)
                return MacEnum.ChannelAccessFailure;
            if (ackPending)
                return MacEnum.NoAck;
            return MacEnum.Success;
        }
    }
}


