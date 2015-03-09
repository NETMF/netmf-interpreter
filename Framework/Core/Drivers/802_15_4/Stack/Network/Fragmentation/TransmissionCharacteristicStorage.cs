////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Microsoft.SPOT.Wireless.IEEE_802_15_4.Network.Fragmentation
{
    /// <summary>
    /// Class used to store information regarding the connection between two nodes
    /// </summary>
    internal class TransmissionCharacteristic
    {
        UInt16 _shortAddress;
        byte _messageSeqNumber;
        uint _timeoutForResending;
        uint _maxFragmentsSentBeforeAck;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="shortAddress">address of destination</param>
        /// <param name="sqn">last message sequence number used for the communication</param>
        /// <param name="timeoutForResending">timeout beofre resending framgent when not receiving acknowledgment</param>
        /// <param name="maxFragmentsSentBeforeAck">maximum number of fragment to send before requesting an acknowledgment</param>
        public TransmissionCharacteristic(UInt16 shortAddress, byte sqn, uint timeoutForResending, uint maxFragmentsSentBeforeAck)
        {
            _shortAddress = shortAddress;
            _messageSeqNumber = sqn;
            _timeoutForResending = timeoutForResending;
            _maxFragmentsSentBeforeAck = maxFragmentsSentBeforeAck;
        }

        public uint TimeoutForResending
        {
            get { return _timeoutForResending; }
            set { _timeoutForResending = value; }
        }

        public uint MaxFragmentsSentBeforeAck
        {
            get { return _maxFragmentsSentBeforeAck; }
            set { _maxFragmentsSentBeforeAck = value; }
        }

        public UInt16 ShortAddress
        {
            get { return _shortAddress; }
        }

        public byte MessageSeqNumber
        {
            get { return _messageSeqNumber; }
            set { _messageSeqNumber = value; }
        }

    }

    /// <summary>
    /// Class used to store the transmission characteristics for the communication with all nodes
    /// </summary>
    public class TransmissionCharacteristicStorage
    {
        private Object _lock;
        private ArrayList _sqnArray;

        public TransmissionCharacteristicStorage()
        {
            _sqnArray = new ArrayList();
            _lock = new object();
        }

        /// <summary>
        /// Get the characterisitcs associated to a node
        /// </summary>
        /// <param name="shortAddress">the address of the node</param>
        /// <returns>the characteristic if present, null otherwise</returns>
        private TransmissionCharacteristic GetNodeCharacteristic(UInt16 shortAddress)
        {
            lock (_lock)
            {
                int sqnArrayCount = _sqnArray.Count;
                for (int i = 0; i < sqnArrayCount; i++)
                {
                    TransmissionCharacteristic holder = (TransmissionCharacteristic)_sqnArray[i];
                    if (holder.ShortAddress == shortAddress)
                    {
                        return holder;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Get the seq. number associated to a node.
        /// </summary>
        public bool GetTransmissionCharacteristic(UInt16 shortAddress, out byte messageSeqNumber, out  uint timeoutForResending, out uint maxFragmentsSentBeforeAck)
        {
            lock (_lock)
            {
                TransmissionCharacteristic holder = GetNodeCharacteristic(shortAddress);
                if (holder != null)
                {
                    messageSeqNumber = holder.MessageSeqNumber;
                    timeoutForResending = holder.TimeoutForResending;
                    maxFragmentsSentBeforeAck = holder.MaxFragmentsSentBeforeAck;
                    return true;
                }

                messageSeqNumber = 0;
                timeoutForResending = 0;
                maxFragmentsSentBeforeAck = 0;
                return false;
            }
        }

        /// <summary>
        /// Set transmission characteristic (including sequence number).
        /// </summary>
        public void SetTransmissionCharacteristic(UInt16 shortAddress, byte sqn, uint timeoutForResending, uint maxFragmentsSentBeforeAck)
        {
            lock (_lock)
            {
                TransmissionCharacteristic holder = GetNodeCharacteristic(shortAddress);
                if (holder != null)
                {
                    holder.MessageSeqNumber = sqn;
                    holder.TimeoutForResending = timeoutForResending;
                    holder.MaxFragmentsSentBeforeAck = maxFragmentsSentBeforeAck;
                }
                else
                {
                    holder = new TransmissionCharacteristic(shortAddress, sqn, timeoutForResending, maxFragmentsSentBeforeAck);
                    _sqnArray.Add(holder);
                }
            }
        }

        /// <summary>
        /// Set transmission characteristic.
        /// <returns>true if update was successful and false otherwise</returns>
        /// </summary>
        public bool UpdateTransmissionCharacteristic(UInt16 shortAddress, uint timeoutForResending, uint maxFragmentsSentBeforeAck)
        {
            lock (_lock)
            {
                TransmissionCharacteristic holder = GetNodeCharacteristic(shortAddress);
                if (holder != null)
                {
#if PRINTALL

                    Trace.Print("Updating trans. char for node " + shortAddress + ": timeoutForResend = " + timeoutForResending + ", maxFrag = " + maxFragmentsSentBeforeAck);
#endif

                    holder.TimeoutForResending = timeoutForResending;
                    holder.MaxFragmentsSentBeforeAck = maxFragmentsSentBeforeAck;
                    return true;
                }

                return false;
            }
        }
    }

}


