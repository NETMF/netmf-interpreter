////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#define MICROFRAMEWORK

using System;
#if MICROFRAMEWORK
using Microsoft.SPOT;
#endif
using System.Threading;

namespace Microsoft.SPOT.Wireless.IEEE_802_15_4.Network.Fragmentation
{
    public class Fragmentation : IFragmentation, IDisposable
    {

        //private members
        int _lowerMtu;
        int _lowerTail;
        int _lowerHead;
        int _maxRequestNumberPerDestination;
        DataRequestQueueSet _dataRequestQueueSet;
        AutoResetEvent _dataRequestQueueEvent;
        DataIndicationHandler _dataIndicationHandler;
        DataRequestHandler _lowerLayerDataRequest;
        FragmentationMessageAssociationSet _outboundAssociations;
        FragmentationMessageAssociationSet _inboundAssociations;
        TransmissionCharacteristicStorage _transmissionCharacteristicStorage;
        bool _started;
        UInt16 _localShortAddress;
        Object _lock;
        FragmentationMessageTimer _timer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxRequestNumberPerDestination">max incoming requests buffered per destination</param>
        /// <param name="lowerLayerDataRequestHandler">lower layer handler for sending PDU</param>
        /// <param name="tail">maximum transmission unit in bytes for the lower layer</param>
        /// <param name="head">required space for header for the lower layer</param>
        /// <param name="tail">required space for tail for the lower layer</param>
        public Fragmentation(int maxRequestNumberPerDestination,
            DataRequestHandler lowerLayerDataRequestHandler, int mtu, int head, int tail)
        {
            _maxRequestNumberPerDestination = maxRequestNumberPerDestination;
            _started = false;
            _lowerLayerDataRequest = lowerLayerDataRequestHandler;
            _lock = new Object();
            _lowerHead = head;
            _lowerTail = tail;
            _lowerMtu = mtu;

        }

        public void Start(UInt16 localAddress)
        {
            lock (_lock)
            {
                if (_started)
                {
                    Stop();
                }

                _localShortAddress = localAddress;
                _outboundAssociations = new FragmentationMessageAssociationSet();
                _inboundAssociations = new FragmentationMessageAssociationSet();
                _dataRequestQueueEvent = new AutoResetEvent(false);
                _dataRequestQueueSet = new DataRequestQueueSet(_maxRequestNumberPerDestination, _dataRequestQueueEvent);
                _started = true;
                _timer = new FragmentationMessageTimer(50);
                _transmissionCharacteristicStorage = new TransmissionCharacteristicStorage();
            }
        }

        public void Stop()
        {
            lock (_lock)
            {
                _started = false;
                if (_outboundAssociations != null)
                {
                    _outboundAssociations.Dispose();
                    _outboundAssociations = null;
                }

                if (_inboundAssociations != null)
                {
                    _inboundAssociations.Dispose();
                    _inboundAssociations = null;
                }

                if (_dataRequestQueueEvent != null)
                {
                    _dataRequestQueueEvent.Set();
                    _dataRequestQueueEvent = null;
                }

                if (_dataRequestQueueEvent != null)
                {
                    _dataRequestQueueSet.Dispose();
                    _dataRequestQueueSet = null;
                }

                if (_timer != null)
                {
                    _timer.Dispose();
                }
            }
        }

        private void HandleFragmentationMessageTerminated(object sender, FragmentationMessageTerminatedEventArgs args)
        {
            if (sender is InboundFragmentationMessage)
            {
                if ((args.FinalState != FragmentationMessageState.Final) || (args.FinalStatus != Status.Success))
                {
                    // do nothing.
                    return;
                }

                InboundFragmentationMessage inMsg = sender as InboundFragmentationMessage;

                Frame receivedMessage = inMsg.RetrieveData();

                if (receivedMessage != null)
                {
                    DataIndicationHandler handler = _dataIndicationHandler;
                    if (handler != null)
                    {
                        handler.Invoke(this, inMsg.Source, inMsg.Destination, receivedMessage);
                    }
                    else
                    {
                        Frame.Release(ref receivedMessage);
                    }
                }

            }
            else if (sender is OutboundFragmentationMessage)
            {
                OutboundFragmentationMessage outMsg, newOutMsg;
                lock (_lock)
                {
                    outMsg = sender as OutboundFragmentationMessage;
                    ushort previousDestination = outMsg.Destination;
                    _transmissionCharacteristicStorage.UpdateTransmissionCharacteristic(outMsg.Destination, outMsg.TimeoutForResending,
                    outMsg.MaxFragmentsBeforeAck);
                    // remove active message from repository
                    _outboundAssociations.RemoveFragmentationMessage(outMsg);
                    // try to start new message (if possible)
                    DataRequestItem newItem;
                    newOutMsg = null;
                    if (_dataRequestQueueSet.Dequeue(previousDestination, out newItem))
                    {
                        newOutMsg = CreateNewOutboundMessage(newItem);
                    }

                }

                // after releasing the lock, call method in FragmentationMessage class.
                outMsg.NotifySender(args.FinalState, args.FinalStatus);
                outMsg.Dispose();

                if (newOutMsg != null) { newOutMsg.SendNextDataFragment(true); }
            }
        }

        #region IFragmentation Members

        public void DataRequest(ushort targetShortAddr, ref Frame sdu, byte sduHandle, DataConfirmHandler handler)
        {
            if (!_started)
            {
                if (handler != null)
                {
                    handler.Invoke(this, sduHandle, Status.NotRunning);
                }

                return;
            }

            if (targetShortAddr == _localShortAddress)
            {
                // deliver frame to ourself
                if (handler != null)
                {
                    handler.Invoke(this, sduHandle, Status.Success);
                }

                DataIndicationHandler ind = _dataIndicationHandler;
                if (ind != null)
                {
                    ind.Invoke(this, targetShortAddr, targetShortAddr, sdu);
                    sdu = null;
                }

                return;
            }

            DataRequestItem newItem = new DataRequestItem(targetShortAddr, ref sdu, sduHandle, handler);

            OutboundFragmentationMessage outMsg = null;
            bool queueFull = false;
            lock (_lock)
            {
                // check if one can send directly the message.
                if (null == _outboundAssociations.GetFragmentationMessage(_localShortAddress, targetShortAddr))
                {
                    outMsg = CreateNewOutboundMessage(newItem);
                }
                else
                {
                    // there is already a message being sent to this address. Store to queue.
                    if (!_dataRequestQueueSet.Add(newItem))
                    {
                        queueFull = true;

                    }
                }
            }

            // release the lock before calling method in FragmentMessage class
            if (outMsg != null)
            {
                outMsg.SendNextDataFragment(true);
            }
            else if (queueFull)
            {
                // queue is full.
                if (handler != null)
                {
                    handler.Invoke(this, sduHandle, Status.Busy);
                }

                newItem.Dispose();
            }
        }

        public DataIndicationHandler DataIndication
        {
            get { return _dataIndicationHandler; }
            set { _dataIndicationHandler = value; }
        }

        public void HandleLowerLayerDataIndication(object sender, ushort originatorShortAddr, ushort targetShortAddr, Frame sdu)
        {
            FragmentationMessage fragMsg = null;
            FragmentHeader header;

            lock (_lock)
            {
                if ((!_started) || (sdu.LengthDataUsed < 2))
                {
                    Frame.Release(ref sdu);
                    return;
                }

                // extract fragment header.
                header = FragmentHeader.Decode(sdu.ReadUInt16(0));
                sdu.DeleteFromFront(2);
                FragmentationMessageAssociationSet associations;
                switch (header.Type)
                {
                    case FragmentType.DATA:
                        associations = _inboundAssociations;
                        if (associations == null)
                        {
                            Frame.Release(ref sdu);
                            return;
                        }

                        fragMsg = (InboundFragmentationMessage)associations.GetFragmentationMessage(originatorShortAddr, targetShortAddr);
                        if (fragMsg != null)
                        {
                            // there is already one message with this peer. Check freshness.
                            if (fragMsg.MessageNumber != header.MessageSeqNumber)
                            {
                                int sqnOld = (int)fragMsg.MessageNumber;
                                int sqnNew = (int)header.MessageSeqNumber;
                                int diff = (sqnNew > sqnOld) ? sqnNew - sqnOld : sqnNew - sqnOld + 256;
                                if ((header.MessageSeqNumber == 0) || (diff < 128))
                                {
                                    // data received is most recent.
                                    fragMsg.Dispose();
                                    fragMsg = null;
                                }
                                else
                                {
                                    // data received is old stuff.
                                    Frame.Release(ref sdu);
                                    return;
                                }
                            }
                        }

                        if (fragMsg == null)
                        {
                            // new message must be started
                            fragMsg = new InboundFragmentationMessage(originatorShortAddr, targetShortAddr,
                                header.MessageSeqNumber, _lowerLayerDataRequest, _lowerMtu, _lowerHead, _lowerTail, _timer);
                            fragMsg.FragmentationMessageTerminated += this.HandleFragmentationMessageTerminated;
                            associations.SetFragmentationMessage(originatorShortAddr, targetShortAddr, fragMsg);
                        }
                        break;
                    case FragmentType.ACK:
                        associations = _outboundAssociations;
                        if (associations == null)
                        {
                            Frame.Release(ref sdu);
                            return;
                        }

                        fragMsg = (OutboundFragmentationMessage)associations.GetFragmentationMessage(targetShortAddr, originatorShortAddr);
                        if ((fragMsg == null) || (fragMsg.MessageNumber != header.MessageSeqNumber))
                        {
                            //no one waiting for this segment. Discard.
#if DEBUG
                            Trace.Print("No one waiting for this segment. Discard.");
#endif

                            Frame.Release(ref sdu);
                            return;
                        }
                        break;
                    default:
                        break;
                }
            }

            // release the lock before calling method in FragmentMessage class
            if (fragMsg != null)
            {
                fragMsg.HandleReceivedFragment(header, ref sdu);

            }
        }

        public DataRequestHandler LowerLayerDataRequest
        {
            get { return _lowerLayerDataRequest; }
            set { _lowerLayerDataRequest = value; }
        }

        public void GetMtuSize(out int mtu, out int head, out int tail)
        {
            // add layer specific: 2 bytes for generic fragment header and 2 bytes for DATA header
            mtu = 255 * (_lowerMtu - 2 - 2); // max 255 fragments (nb frag stored in a byte).
            head = 0; // fragment frames are rebuilt so no head and tail required.
            tail = 0;
        }

        #endregion

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
            Stop();
        }

        #region helper functions
        /// <summary>
        /// Note: caller msut hold the lock _lock
        /// </summary>
        /// <param name="dataRequestItem"></param>
        private OutboundFragmentationMessage CreateNewOutboundMessage(DataRequestItem dataRequestItem)
        {

            byte messageSeqNumber;
            uint timeoutForResending, maxFragmentsBeforeAck;
            if (!_transmissionCharacteristicStorage.GetTransmissionCharacteristic(dataRequestItem.TargetShortAddr, out messageSeqNumber, out timeoutForResending, out maxFragmentsBeforeAck))
            {
                // characteristic not present. Create default one.
                messageSeqNumber = 0;
                timeoutForResending = FragmentationMessage.c_defaultTimeoutForResending;
                maxFragmentsBeforeAck = FragmentationMessage.c_defaultMaxFragmentsBeforeAck;
                _transmissionCharacteristicStorage.SetTransmissionCharacteristic(dataRequestItem.TargetShortAddr, messageSeqNumber, timeoutForResending, maxFragmentsBeforeAck);
            }
            else
            {
                // characteristic already present. increment the sequence number.
                messageSeqNumber++;
                if (messageSeqNumber == 0) { messageSeqNumber++; } // sequence number = 0 is reserved for new communication
                _transmissionCharacteristicStorage.SetTransmissionCharacteristic(dataRequestItem.TargetShortAddr, messageSeqNumber, timeoutForResending, maxFragmentsBeforeAck);
            }

            Frame sdu = dataRequestItem.Sdu;
            OutboundFragmentationMessage newMsg = new OutboundFragmentationMessage(_localShortAddress,
                dataRequestItem.TargetShortAddr, messageSeqNumber, _lowerLayerDataRequest,
                ref sdu, dataRequestItem.SduHandle, dataRequestItem.Handler, _lowerMtu, _lowerHead, _lowerTail, _timer);
            newMsg.FragmentationMessageTerminated += HandleFragmentationMessageTerminated;
            newMsg.MaxFragmentsBeforeAck = maxFragmentsBeforeAck;
            newMsg.TimeoutForResending = timeoutForResending;
            _outboundAssociations.SetFragmentationMessage(_localShortAddress, dataRequestItem.TargetShortAddr, newMsg);
            return newMsg;
        }

        #endregion

    }
}


