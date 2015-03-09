////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace Microsoft.SPOT.Wireless.IEEE_802_15_4.Network.Fragmentation
{

    internal enum FragmentationMessageState
    {
        /// <summary>
        /// Initial state
        /// </summary>
        Initial,
        /// <summary>
        /// operation completed
        /// </summary>
        Final,
        /// <summary>
        /// waiting for sent confirmation
        /// </summary>
        WaitingSentStatus,
        /// <summary>
        /// fragment(s) sent, waiting for ack
        /// </summary>
        WaitingAck,
        /// <summary>
        /// last fragment could not be sent because stack busy. wait timeout before resending
        /// </summary>
        WaitingTimeoutBeforeResending,
        /// <summary>
        /// waiting for segments
        /// </summary>
        WaitingFragments,
        /// <summary>
        /// sending is not (currently) possible
        /// </summary>
        SendingImpossible,
        /// <summary>
        /// message has been disposed
        /// </summary>
        Disposed,
        /// <summary>
        /// unknown state
        /// </summary>
        Unknown,
    }

    internal abstract class FragmentationMessage : IDisposable
    {
        // The following parameters should be set according to the typical size of the message sent.
        // If messages are large, i.e. many segments, then the receiver will generally receive some packets and it is
        // his responsability to inform the sender which segments to resent, so c_timeoutForSendingAck << c_timeoutForResending
        // Conversely, if the messages are small, typically one segment, then the responsability to resend should be attributed
        // to the sender since the receiver is not aware that a message has been sent, so c_timeoutForSendingAck >> c_timeoutForResending
        // Currently, messages are small: 1 or 2 segments...
        // Idee: adapt timer based on message size...
        public const uint c_defaultTimeoutForResending = 500;  // if did not receive ACK within this time, should resend segments.
        public const uint c_defaultMaxFragmentsBeforeAck = 4;  // number of fragments sent before requesting an ACK.
        public const uint c_maxTimeoutForResending = 4000;
        // public const uint c_minTimeoutForResending = 50;
        public const uint c_timeoutWhenStackBusy = 500;
        public const uint c_maxNbTimeoutWithoutNewMessage = 6; // max time one send NACK after timeout without

        protected readonly UInt16 _source; // can not change node
        protected readonly UInt16 _destination;  // can not change node
        protected byte _messageSeqNumber;
        protected byte _lastFragmentSeqNumberReceived;
        protected byte _lastFragmentSeqNumberSent;
        protected FragmentationMessageState _currentState;
        protected object _lock;
        protected byte _lastHandleSent;
        // timer info
        protected uint _timerId;
        protected bool _timerEnabled;
        protected long _timeoutTicks;
        protected FragmentationMessageTimer _timer;

        protected int _maxPduSize;
        protected int _reservedHeaderSize;
        protected int _reservedTailSize;

        /// <summary>
        /// Sequence number associated to the message
        /// </summary>
        internal byte MessageNumber { get { return _messageSeqNumber; } }

        /// <summary>
        /// Source of the fragments.
        /// </summary>
        internal UInt16 Source { get { return _source; } }

        /// <summary>
        /// Destination of the fragments.
        /// </summary>
        internal UInt16 Destination { get { return _destination; } }

        /// <summary>
        /// ticks value at which timeout should take effect.
        /// value of long.MaxValue indicates that no timeout should be active on the message.
        /// </summary>
        internal long TimeoutTicks { get { return _timeoutTicks; } }

        internal uint TimerId
        {
            get { return _timerId; }
        }

        internal bool TimerEnabled { get { return _timerEnabled; } }

        internal void StoreTimerProperties(bool timerEnabled, uint timerId, long timeoutTicks)
        {
            lock (_lock)
            {
                _timeoutTicks = timeoutTicks;
                _timerId = timerId;
                _timerEnabled = timerEnabled;
            }
        }

        protected DataRequestHandler _lowerLayerRequestHandler;

        internal FragmentationMessage(
            UInt16 source,
            UInt16 destination,
            byte messageSeqNumber,
            DataRequestHandler lowerLayerRequestHandler,
            int maxPduSize,
            int reservedHeaderSize,
            int reservedTailSize,
            FragmentationMessageTimer timer)
        {
            _timer = timer;
            _lowerLayerRequestHandler = lowerLayerRequestHandler;
            _currentState = FragmentationMessageState.Initial;
            _source = source;
            _destination = destination;
            _messageSeqNumber = messageSeqNumber;
            _lock = new object();
            _timeoutTicks = long.MaxValue;
            _maxPduSize = maxPduSize;
            _reservedHeaderSize = reservedHeaderSize;
            _reservedTailSize = reservedTailSize;
            _lastFragmentSeqNumberReceived = 0xFF;
            _lastFragmentSeqNumberSent = 0xFF;
        }

        public virtual void Close()
        {
            Dispose();
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            lock (_lock)
            {
                _currentState = FragmentationMessageState.Disposed;
                if ((_timer != null) && (_timerEnabled))
                {
                    _timer.UnregisterItem(this);

                }
            }
        }  

        /// <summary>
        /// Switch to next state after receiving a new segment
        /// </summary>
        /// <param name="segment">The segment</param>
        internal abstract void HandleReceivedFragment(FragmentHeader header, ref Frame payload);

        /// <summary>
        /// Switch to next state after receiving a timeout
        /// </summary>
        internal abstract void HandleTimeout(Object sender, TimeoutEventArgs args);

        /// <summary>
        /// handle send confirmation from the lower layer
        /// </summary>
        /// <param name="sender">sender of the confirmaiton</param>
        /// <param name="sduHandle">handle of the frame sent</param>
        /// <param name="status">status of the send operation</param>
        internal abstract void HandleSendConfirmation(Object sender, Byte sduHandle, Status status);

        /// <summary>
        /// Send the next data fragments
        /// Note: Only valid for outbound message
        /// </summary>
        /// <returns></returns>
        internal abstract void SendNextDataFragment(bool force);

        /// <summary>
        /// Send an ack corresponding to fragment received so far.
        /// Note: Only valid for inbound message
        /// </summary>
        /// <returns></returns>
        internal abstract void SendAcknowledgement();
        internal event FragmentationMessageTerminatedEventHandler FragmentationMessageTerminated;

        private bool _notifcationOfFinalStateTransmitted = false;
        protected void OnFragmentationMessageTerminated(FragmentationMessageTerminatedEventArgs args)
        {
            FragmentationMessageTerminatedEventHandler handler = FragmentationMessageTerminated;
            if (!_notifcationOfFinalStateTransmitted && (handler != null))
            {
                _notifcationOfFinalStateTransmitted = true;
                handler(this, args);
            }
        }

    }

    internal delegate void FragmentationMessageTerminatedEventHandler(object sender, FragmentationMessageTerminatedEventArgs args);
    internal class FragmentationMessageTerminatedEventArgs
    {
        FragmentationMessageState _finalState;
        Status _finalStatus;
        internal FragmentationMessageState FinalState { get { return _finalState; } }
        internal Status FinalStatus { get { return _finalStatus; } }
        internal FragmentationMessageTerminatedEventArgs(FragmentationMessageState finalState, Status finalStatus)
        {
            _finalState = finalState;
            _finalStatus = finalStatus;
        }
    }

    internal class OutboundFragmentationMessage : FragmentationMessage
    {

        private Frame _sdu;
        private Byte _sduHandle;
        DataConfirmHandler _dataConfirmHandler;
        BitArray _successfullyTransmittedFragments; // segment acknowledged (if not broadcast) or successfully transmitted (if broadcast)
        BitArray _lastFragmentSent;
        DateTime _timeLastFragmentSent;
        int[] _endingIndices;
        uint _maxFragmentsBeforeAck;
        uint _timeoutForResending;
        int _nbTimeoutWithoutAck;
        bool _allFragmentTransmitted;
        int _nbFragmentsTransmittedLastSendTransition;
        bool _isBroadcast;
        byte _pduHandle;
        int _nbTransmissions;
        byte _nbFragments;

        internal OutboundFragmentationMessage(
            UInt16 source,
            UInt16 destination,
            byte messageSeqNumber,
            DataRequestHandler lowerLayerRequestHandler,
            ref Frame sdu,
            Byte sduHandle,
            DataConfirmHandler handler,
            int maxPduSize,
            int reservedHeaderSize,
            int reservedTailSize,
            FragmentationMessageTimer timer
            )
            : base(source, destination, messageSeqNumber, lowerLayerRequestHandler, maxPduSize, reservedHeaderSize, reservedTailSize, timer)
        {
            if (sdu.LengthDataUsed == 0)
            {
                Frame.Release(ref sdu);
                throw new ArgumentException("sdu is empty", "sdu");
            }

            int payloadSize = maxPduSize - 2 - 2; // 2 for our own header, 1 for nb fragment, 1 for fragment id
            if (payloadSize == 0)
            {
                Frame.Release(ref sdu);
                throw new ArgumentException("PDU is too small", "maxPduSize");
            }

            ushort flag = (ushort)(1 << 15);
            _isBroadcast = (destination & flag) > 0 ? true : false;

            _sdu = sdu;
            sdu = null;
            _sduHandle = sduHandle;
            _dataConfirmHandler = handler;
            _messageSeqNumber = messageSeqNumber;
            _timeoutForResending = c_defaultTimeoutForResending;
            _maxFragmentsBeforeAck = c_defaultMaxFragmentsBeforeAck;
            // compute required nb fragments and respective indices.

            int nbFragments = ((_sdu.LengthDataUsed % payloadSize) == 0) ? _sdu.LengthDataUsed / payloadSize : 1 + _sdu.LengthDataUsed / payloadSize;
            if (nbFragments > byte.MaxValue)
                throw new ArgumentException("sdu is too large.");
            _nbFragments = (byte)nbFragments;
            _endingIndices = new int[_nbFragments];
            int lastIndex = -1;
            for (int i = 0; i + 1 < _nbFragments; i++)
            {
                lastIndex += payloadSize;
                _endingIndices[i] = lastIndex;
            }

            lastIndex += ((_sdu.LengthDataUsed % payloadSize) == 0) ? payloadSize : _sdu.LengthDataUsed % payloadSize;
            _endingIndices[_nbFragments - 1] = lastIndex;
            _successfullyTransmittedFragments = new BitArray(_nbFragments, false);
            _lastFragmentSent = new BitArray(_nbFragments, false);
        }

        internal uint TimeoutForResending
        {
            get { return _timeoutForResending; }
            set { _timeoutForResending = value; }
        }

        internal uint MaxFragmentsBeforeAck
        {
            get { return _maxFragmentsBeforeAck; }
            set { _maxFragmentsBeforeAck = value; }
        }

        internal override void SendAcknowledgement()
        {
            throw new Exception("The method is not implemented for Oubound message.");
        }

        int _nbFragmentsSentWithoutAck = 0;

        /// <summary>
        /// Start sending segments.
        /// <param name="force">send a fragment in all case if true, do not send if no fragment pending</param>
        /// </summary>
        override internal void SendNextDataFragment(bool force)
        {
            Frame fragment;
            ushort target;
            byte pduHandle;

            lock (_lock)
            {
                if (_timerEnabled)
                {
                    _timer.UnregisterItem(this);
                    _timerEnabled = false;
                }

                if ((_currentState == FragmentationMessageState.Disposed) || (_allFragmentTransmitted))
                {
                    return;
                }

                // for broadcast message, we send fragment one by one to make sure that they are properly sent.
                byte indexOfFragmentToSend = 0;
                bool fragmentFound = false;
                bool lastFragmentToBeSent = true;
                for (int i = 0; i < _endingIndices.Length; i++)
                {
                    if (!_successfullyTransmittedFragments.Get(i) && !_lastFragmentSent.Get(i))
                    {
                        if (fragmentFound)
                        {
                            lastFragmentToBeSent = false;
                            break;
                        }

                        indexOfFragmentToSend = (byte)i;
                        fragmentFound = true;
                    }

                }

                bool alreadySentAll = false;
                if (!fragmentFound)
                {
                    if (force)
                    {
                        // no segment to send. Resent last one.
                        alreadySentAll = true;
                        indexOfFragmentToSend = (byte)(_endingIndices.Length - 1);
                    }
                    else
                    {
                        // no fragment to send. Wait for ack.
                        _timeoutTicks = DateTime.Now.Ticks + _timeoutForResending * TimeSpan.TicksPerMillisecond;
                        _timerId = _timer.RegisterItem(_timeoutTicks, this);
                        _timerEnabled = true;
                        _currentState = FragmentationMessageState.WaitingAck;
                        return;
                    }
                }

                bool requireAck = false;
                byte fragmentSeqNumber = ++_lastFragmentSeqNumberSent;

                if ((!_isBroadcast) && (lastFragmentToBeSent || (_nbFragmentsSentWithoutAck + 1 >= _maxFragmentsBeforeAck) || alreadySentAll))
                {
                    requireAck = true;
                }

                int startIndexInSdu = (indexOfFragmentToSend == 0) ? 0 : _endingIndices[indexOfFragmentToSend - 1] + 1;
                int endIndexInSdu = _endingIndices[indexOfFragmentToSend];

                fragment = Fragment.CreateDataFrame(_messageSeqNumber, fragmentSeqNumber, _isBroadcast, requireAck,
                    _maxPduSize, _reservedTailSize, _reservedHeaderSize,
                    _nbFragments, indexOfFragmentToSend, _sdu, startIndexInSdu, endIndexInSdu - startIndexInSdu + 1);

                _nbFragmentsSentWithoutAck++;
                _nbFragmentsTransmittedLastSendTransition = 0;
                _lastFragmentSent.Set(indexOfFragmentToSend, true);
#if PRINTALL
                Trace.Print("Sending part  " + indexOfFragmentToSend + "/" + _endingIndices.Length.ToString() + " of packet " + _messageSeqNumber);
#endif
                _currentState = FragmentationMessageState.WaitingSentStatus;

                pduHandle = _pduHandle++;
                target = _destination;
                _nbTransmissions++;
                _nbFragmentsTransmittedLastSendTransition++;

                // already set time of last fragment sent, but update when receive sent confirmation.
                // this prevent the situation where the ACK is actually handled before the send confirmation
                _timeLastFragmentSent = DateTime.Now;
            }

            // release lock before calling lower layer handler.
            _lowerLayerRequestHandler(target, ref fragment, pduHandle, this.HandleSendConfirmation);
            Frame.Release(ref fragment);
            return;
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected override void Dispose(bool disposing)
        {
            lock (_lock)
            {
                if (_sdu != null)
                {
                    Frame.Release(ref _sdu);
                }

                base.Dispose();
            }
        }

        internal override void HandleReceivedFragment(FragmentHeader header, ref Frame payload)
        {
            if (header.FragmentSeqNumber == _lastFragmentSeqNumberReceived)
            {
#if PRINTALL
                Trace.Print("Already received this fragment. Drop.");
#endif
                Frame.Release(ref payload);
                return;
            }

            _lastFragmentSeqNumberReceived = header.FragmentSeqNumber;

            if ((header.Type != FragmentType.ACK) || (header.MessageSeqNumber != _messageSeqNumber))
            {
                Frame.Release(ref payload);
                return;
            }

            FragmentationMessageTerminatedEventArgs messageTerminatedHandlerArgs = null;
            bool sendData = false;

            lock (_lock)
            {

                if (_currentState == FragmentationMessageState.Disposed)
                {
                    Frame.Release(ref payload);
                    return;
                }

                bool timerWasEnabled = _timerEnabled;
                if (_timerEnabled)
                {
                    _timer.UnregisterItem(this);
                    _timerEnabled = false;
                }

                _nbTimeoutWithoutAck = 0;
                if (header.Type == FragmentType.ACK)
                {
#if PRINTALL
                    Trace.Print("Received ACK for message " + header.MessageSeqNumber + " sduHandle=" + _sduHandle);
#endif
                    _nbFragmentsSentWithoutAck = 0;
                    BitArray fragmentMissingTable;
                    byte lastFragmentSeqNumberReceivedByReceived = 255;
                    try
                    {
                        Fragment.DecodeAckPayload(ref payload, out lastFragmentSeqNumberReceivedByReceived, out fragmentMissingTable);
                        Frame.Release(ref payload);
                        if (fragmentMissingTable.Length == _successfullyTransmittedFragments.Length)
                        {
                            BitArray fragmentReceived = fragmentMissingTable.Clone();
                            fragmentReceived.Not();
#if PRINTALL
                            string msg = "";
                            for (int i = 0; i < fragmentReceived.Length; i++)
                            {
                                if (fragmentReceived.Get(i))
                                {
                                    msg += i + ",";
                                }
                            }

                            Trace.Print("received so far: " + msg);
#endif
                            _successfullyTransmittedFragments.Or(fragmentReceived);
                            if (_successfullyTransmittedFragments.CheckAllSet(true))
                            {
                                _allFragmentTransmitted = true;
                                _currentState = FragmentationMessageState.Final;
                            }

                            // now check if we received  ACK for the last fragment that we sent.
                            if (lastFragmentSeqNumberReceivedByReceived == _lastFragmentSeqNumberSent)
                            {
                                long roundTripTime = (DateTime.Now.Ticks - _timeLastFragmentSent.Ticks) / TimeSpan.TicksPerMillisecond;
                                if (2 * roundTripTime <= (long)uint.MaxValue)
                                    _timeoutForResending = (uint)(2 * roundTripTime);

                                _maxFragmentsBeforeAck++;
                            }

                            _lastFragmentSent.SetAll(false);
                        }

                    }
                    catch (ArgumentException)
                    {
                        // could not decode payload.
                        if (timerWasEnabled)
                        {
                            _timer.RegisterItem(_timeoutTicks, this);
                            _timerEnabled = true;
                        }

                        Frame.Release(ref payload);
                        return;
                    }

                    if (_allFragmentTransmitted)
                    {
                        _currentState = FragmentationMessageState.Final;
                        messageTerminatedHandlerArgs = new FragmentationMessageTerminatedEventArgs(_currentState, Status.Success);
                    }
                    else
                    {
                        sendData = true;
                    }

                }
                else
                {
#if DEBUG
                    Trace.Print("Received bad fragment type for message " + header.MessageSeqNumber + " sdhHandle=" + _sduHandle);
#endif
                    Frame.Release(ref payload);
                    if (timerWasEnabled)
                    {
                        _timer.RegisterItem(_timeoutTicks, this);
                        _timerEnabled = true;
                    }
                }
            }

            // release the lock before calling OnFragmentationMessageFinalState since upper layer handler is called.
            if (messageTerminatedHandlerArgs != null)
            {
                OnFragmentationMessageTerminated(messageTerminatedHandlerArgs);
            }
            else if (sendData)
            {
                SendNextDataFragment(false);
            }
        }

        override internal void HandleTimeout(Object sender, TimeoutEventArgs args)
        {
            FragmentationMessageTerminatedEventArgs messageTerminatedHandlerArgs = null;
            bool sendData = false;
#if PRINTALL
            Trace.Print("Timeout in outbound message!");
#endif

            lock (_lock)
            {
                if (!_timerEnabled)
                {
#if PRINTALL
                    Trace.Print("timer is not enabled. Do nothing.");
#endif
                    return;
                }

                _timerEnabled = false; // no need to unregister since automatically unregister when timeout is fired.
                if (_currentState == FragmentationMessageState.Disposed)
                {
                    return;
                }

                if (this != args.Message)
                    throw new ArgumentException("timeout delivered with wrong message");
                _nbTimeoutWithoutAck++;
                if (_nbTimeoutWithoutAck >= c_maxNbTimeoutWithoutNewMessage)
                {
                    _sendStatus = Status.Timeout;
                    _currentState = FragmentationMessageState.SendingImpossible;

                    messageTerminatedHandlerArgs = new FragmentationMessageTerminatedEventArgs(_currentState, _sendStatus);
                }
                else
                {
                    _nbFragmentsSentWithoutAck = 0;
                    _lastFragmentSent.SetAll(false);
                    _maxFragmentsBeforeAck /= 2;
                    if (_maxFragmentsBeforeAck == 0)
                    {
                        _maxFragmentsBeforeAck = 1;
                    }

                    _timeoutForResending = (2 * _timeoutForResending);
                    if (_timeoutForResending > c_maxTimeoutForResending)
                    {
                        _timeoutForResending = c_maxTimeoutForResending;
                    }

                    sendData = true;
                }
            }

            if (messageTerminatedHandlerArgs != null)
            {
                OnFragmentationMessageTerminated(messageTerminatedHandlerArgs);
            }
            else if (sendData)
            {
                SendNextDataFragment(true);
            }
        }

        Status _sendStatus = Status.Success;

        internal override void HandleSendConfirmation(Object sender, byte sduHandle, Status status)
        {

            FragmentationMessageTerminatedEventArgs messageTerminatedHandlerArgs = null;
            bool sendData = false;

            lock (_lock)
            {
                if (_currentState == FragmentationMessageState.Disposed)
                {
                    return;
                }

                if (_currentState == FragmentationMessageState.WaitingSentStatus)
                {
                    if (_timerEnabled)
                    {
                        _timer.UnregisterItem(this);
                        _timerEnabled = false;
                    }

                    _sendStatus = status;

                    switch (status)
                    {
                        case Status.Success:
                            if (_isBroadcast)
                            {
                                _successfullyTransmittedFragments.Or(_lastFragmentSent);
                                _allFragmentTransmitted = _successfullyTransmittedFragments.CheckAllSet(true);
                                if (_allFragmentTransmitted)
                                {
                                    _currentState = FragmentationMessageState.Final;
                                    messageTerminatedHandlerArgs = new FragmentationMessageTerminatedEventArgs(_currentState, Status.Success);

                                }
                                else
                                {
                                    sendData = true;
                                }

                            }
                            else
                            {
                                if (!_allFragmentTransmitted)
                                {
                                    // check if continue sending fragment or wait for ACK.
                                    if (_nbFragmentsSentWithoutAck >= _maxFragmentsBeforeAck)
                                    {
                                        // wait for ack.
                                        _timeLastFragmentSent = DateTime.Now;
                                        _timeoutTicks = _timeLastFragmentSent.Ticks + _timeoutForResending * TimeSpan.TicksPerMillisecond;
                                        _timerId = _timer.RegisterItem(_timeoutTicks, this);
                                        _timerEnabled = true;
                                        _currentState = FragmentationMessageState.WaitingAck;
                                    }
                                    else
                                    {
                                        _timeLastFragmentSent = DateTime.Now;
                                        sendData = true;
                                    }
                                }
                                else
                                {
                                    _currentState = FragmentationMessageState.Final;
                                    messageTerminatedHandlerArgs = new FragmentationMessageTerminatedEventArgs(_currentState, Status.Success);
                                }
                            }
                            break;
                        case Status.Timeout:
                        case Status.Busy:
#if DEBUG
                            Trace.Print("stack busy. wait for " + FragmentationMessage.c_timeoutWhenStackBusy + " ms.");
#endif
                            _timeoutTicks = DateTime.Now.Ticks + FragmentationMessage.c_timeoutWhenStackBusy * TimeSpan.TicksPerMillisecond;
                            _timerId = _timer.RegisterItem(_timeoutTicks, this);
                            _timerEnabled = true;
                            _currentState = FragmentationMessageState.WaitingTimeoutBeforeResending;
                            break;
                        case Status.Error:
                        case Status.NoRoute:
                        case Status.NotRunning:
                        case Status.InvalidFrame:
                        default:
                            _currentState = FragmentationMessageState.SendingImpossible;
                            messageTerminatedHandlerArgs = new FragmentationMessageTerminatedEventArgs(_currentState, _sendStatus);
                            break;
                    }
                }
            }

            // need to release the lock before calling OnFragmentationMessageFinalState since upper layer handler might be called.
            if (messageTerminatedHandlerArgs != null)
            {
                OnFragmentationMessageTerminated(messageTerminatedHandlerArgs);
            }
            else if (sendData)
            {
                SendNextDataFragment(false);
            }
        }

        bool _senderNotified;
        internal void NotifySender(FragmentationMessageState finalState, Status finalStatus)
        {
            byte sduHandle;

            lock (_lock)
            {
                if (_currentState == FragmentationMessageState.Disposed)
                {
                    return;
                }

                if (_senderNotified)
                {
                    return;
                }

                _senderNotified = true;
                if (_dataConfirmHandler == null)
                {
                    return;
                }

                sduHandle = _sduHandle;
            }

            if (finalState == FragmentationMessageState.Final || finalState == FragmentationMessageState.SendingImpossible)
            {
                _dataConfirmHandler(this, sduHandle, finalStatus);
            }
            else
            {
                // unexpected state.
                _dataConfirmHandler(this, sduHandle, Status.Error);
            }
        }

    }

    class InboundFragmentationMessage : FragmentationMessage
    {

        bool _sduCompleted;
        bool _sduRetrieved;
        BitArray _fragmentMissingTable;
        Frame[] _fragmentData;
        long _lastAckSentTime; // expressed in ticks.
        int _nbAcksSent;
        internal InboundFragmentationMessage(
            UInt16 source,
            UInt16 destination,
            byte sequenceNumber,
            DataRequestHandler lowerLayerRequestHandler,
            int maxPduSize,
            int reservedHeaderSize,
            int reservedTailSize,
            FragmentationMessageTimer timer)
            : base(source, destination, sequenceNumber, lowerLayerRequestHandler, maxPduSize, reservedHeaderSize, reservedTailSize, timer)
        {

        }

        internal override void SendNextDataFragment(bool force)
        {
            throw new Exception("The method is not implemented for inbound message.");
        }

        override internal void SendAcknowledgement()
        {
            byte sduHandle;
            ushort targetShortAddr;
            Frame frame;

            lock (_lock)
            {
#if PRINTALL
                Trace.Print("sending ACK");
#endif
                long dateTimeNowTicks = DateTime.Now.Ticks;
                long intervalSinceLastAck = (dateTimeNowTicks - _lastAckSentTime) / TimeSpan.TicksPerMillisecond;

                if (_currentState == FragmentationMessageState.Disposed)
                {
                    return;
                }

                if (_currentState == FragmentationMessageState.WaitingSentStatus)
                {
#if PRINTALL
                    Trace.Print("do not send ack (still waiting sent status).");
#endif
                    // still waiting for status from previous acknowledgment sent. So, do nothing.
                    return;
                }

                if (_timerEnabled)
                {
                    _timer.UnregisterItem(this);
                    _timerEnabled = false;
                }

                frame = Fragment.CreateAckFragmentFrame(_messageSeqNumber, ++_lastFragmentSeqNumberSent, _maxPduSize, _reservedTailSize, _reservedHeaderSize, _lastFragmentSeqNumberReceived, _fragmentMissingTable);
                _currentState = FragmentationMessageState.WaitingSentStatus;
                sduHandle = ++_lastHandleSent;
                targetShortAddr = _source;
                _lastAckSentTime = dateTimeNowTicks;
#if PRINTALL
                Trace.Print("Sending ACK for " + _messageSeqNumber);
#endif
            }

            // release lock before calling lower layer call.
            _lowerLayerRequestHandler(targetShortAddr, ref frame, sduHandle, this.HandleSendConfirmation);

        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected override void Dispose(bool disposing)
        {
            lock (_lock)
            {
                _currentState = FragmentationMessageState.Disposed;
                if (_fragmentData != null)
                {
                    for (int i = 0; i < _fragmentData.Length; i++)
                    {
                        Frame frame = _fragmentData[i];
                        Frame.Release(ref frame);
                    }
                }

                base.Dispose();
            }
        }

        internal override void HandleReceivedFragment(FragmentHeader header, ref Frame payload)
        {
            if (header.FragmentSeqNumber == _lastFragmentSeqNumberReceived)
            {
#if DEBUG
                Trace.Print("Already received this fragment. Drop.");
#endif

                Frame.Release(ref payload);
                return;
            }

            _lastFragmentSeqNumberReceived = header.FragmentSeqNumber;

            if ((header.Type != FragmentType.DATA) || (payload.LengthDataUsed < 2) || (_messageSeqNumber != header.MessageSeqNumber))
            {
                Frame.Release(ref payload);
                return;
            }

            byte nbFragments;
            byte fragmentIndex;
            Frame data;
            try
            {
                Fragment.DecodeDataPayload(ref payload, out nbFragments, out fragmentIndex, out data);
            }
            catch (ArgumentException)
            {
                Frame.Release(ref payload);
                return;
            }

#if PRINTALL
            Trace.Print("Receiving part  " + fragmentIndex + "/" + nbFragments + " of packet " + _messageSeqNumber);
#endif

            FragmentationMessageTerminatedEventArgs messageTerminatedHandlerArgs = null;
            bool sendAck = false;
            lock (_lock)
            {
                if (_currentState == FragmentationMessageState.Disposed)
                {
                    Frame.Release(ref data);
                    return;
                }

                if (_timerEnabled)
                {
                    _timer.UnregisterItem(this);
                    _timerEnabled = false;
                }

                if ((_fragmentMissingTable != null) && (nbFragments != _fragmentMissingTable.Length))
                {
                    // mismatch between messages: no the same number of fragments
                    Frame.Release(ref data);
                    return;
                }

                FragmentationMessageState oldState = _currentState;
                if (_sduCompleted)
                {
                    Frame.Release(ref data);
                    if (header.IsAckRequired)
                    {
                        // resend acknowlegment
                        sendAck = true;
                    }
                    else
                    {
                        // leave the state unchanged
                        return;
                    }
                }
                else
                {
                    switch (_currentState)
                    {
                        case FragmentationMessageState.Initial:
                        case FragmentationMessageState.WaitingFragments:
                        case FragmentationMessageState.WaitingSentStatus:
                        case FragmentationMessageState.WaitingTimeoutBeforeResending:
                            if (_currentState == FragmentationMessageState.Initial)
                            {
                                // first fragment received.
                                _fragmentData = new Frame[nbFragments];
                                _fragmentMissingTable = new BitArray(nbFragments, true);
                            }

                            _fragmentMissingTable.Set(fragmentIndex, false);
                            if (_fragmentData[fragmentIndex] == null)
                            {
                                _fragmentData[fragmentIndex] = data;
                            }
                            else
                            {
                                Frame.Release(ref data);
                            }

                            _sduCompleted = _fragmentMissingTable.CheckAllSet(false);
                            if (header.IsAckRequired)
                            {
                                sendAck = true;
                            }
                            else
                            {
                                _currentState = FragmentationMessageState.WaitingFragments;
                            }

                            if (_sduCompleted)
                            {
                                _currentState = FragmentationMessageState.Final;
                                messageTerminatedHandlerArgs = new FragmentationMessageTerminatedEventArgs(_currentState, Status.Success);
                            }
                            break;
                        case FragmentationMessageState.WaitingAck:
                            Frame.Release(ref data);
                            throw new System.InvalidOperationException("Bad state.");
                        case FragmentationMessageState.Disposed:
                        case FragmentationMessageState.Final:
                        case FragmentationMessageState.Unknown:
                        default:
                            Frame.Release(ref data);
                            break;
                    }
                }
            }

            if (sendAck)
            {
                SendAcknowledgement();
            }

            if (messageTerminatedHandlerArgs != null)
            {
                OnFragmentationMessageTerminated(messageTerminatedHandlerArgs);
            }
        }

        override internal void HandleTimeout(Object sender, TimeoutEventArgs args)
        {
            bool sendAck = false;

            lock (_lock)
            {
                if (this != args.Message)
                    throw new ArgumentException("timeout delivered with wrong message");

                if (_currentState == FragmentationMessageState.Disposed)
                {
                    return;
                }

                if (_currentState == FragmentationMessageState.WaitingTimeoutBeforeResending)
                {
                    sendAck = true;
                }
            }

            if (sendAck) { SendAcknowledgement(); }
        }

        internal override void HandleSendConfirmation(Object sender, byte sduHandle, Status status)
        {
            lock (_lock)
            {
                if (_currentState == FragmentationMessageState.Disposed)
                {
                    return;
                }

                _nbAcksSent++;

                if (_timerEnabled)
                {
                    _timer.UnregisterItem(this);
                    _timerEnabled = false;
                }

                if ((_currentState != FragmentationMessageState.WaitingSentStatus) || (_lastHandleSent != sduHandle))
                {
                    return;
                }
                else
                {
                    switch (status)
                    {
                        case Status.Success:
                            _currentState = (_sduCompleted) ? FragmentationMessageState.Final : FragmentationMessageState.WaitingFragments;
                            break;
                        case Status.Timeout:
                        case Status.Busy:
                            // layers below were not able to send. set timeout before resending.
                            _currentState = FragmentationMessageState.WaitingTimeoutBeforeResending;
                            _timeoutTicks = DateTime.Now.Ticks + FragmentationMessage.c_timeoutWhenStackBusy * TimeSpan.TicksPerMillisecond;
                            _timerId = _timer.RegisterItem(_timeoutTicks, this);
                            _timerEnabled = true;
                            break;
                        case Status.NoRoute:
                        case Status.NotRunning:
                        case Status.Error:
                        case Status.InvalidFrame:
                        default:
                            Trace.Print("Could not send Ack. status: " + status);
                            break;
                    }

                }
            }
        }

        /// <summary>
        /// Retrieve the complete received message.
        /// </summary>
        /// <returns>complete received message if successfuly received and not yet retrieved, or null otherwise</returns>
        internal Frame RetrieveData()
        {
            lock (_lock)
            {
                if (_currentState == FragmentationMessageState.Disposed)
                {
                    return null;
                }

                if (!_sduCompleted && !_sduRetrieved)
                {
                    return null;
                }

                if (_fragmentData == null)
                {
                    return null;
                }

                int size = 0;
                for (int i = 0; i < _fragmentData.Length; i++)
                {
                    size += _fragmentData[i].LengthDataUsed;
                }

                Frame returnedFrame = Frame.GetFrame(size);
                for (int i = 0; i < _fragmentData.Length; i++)
                {
                    returnedFrame.AppendToBack(_fragmentData[i], 0, _fragmentData[i].LengthDataUsed);
                    Frame.Release(ref _fragmentData[i]);

                }

                _fragmentData = null;
                _sduRetrieved = true;
                return returnedFrame;
            }
        }

    }
}


