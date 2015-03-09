////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using Microsoft.SPOT.Wireless.IEEE_802_15_4;

namespace Microsoft.SPOT.Wireless.IEEE_802_15_4.Network.Fragmentation
{
    /// <summary>
    /// Type of fragment
    /// </summary>
    public enum FragmentType
    {
        /// <summary>
        /// fragment containing data
        /// </summary>
        DATA = 0,
        /// <summary>
        /// fragment containing acknowledgment
        /// </summary>
        ACK = 1,
    }

    /// <summary>
    /// position of fields within the fragment header
    /// </summary>
    internal enum FragmentHeaderField : byte
    {
        /// <summary>
        /// header type
        /// </summary>
        Type = 0,
        /// <summary>
        /// flag whether acknowledgment is required for this fragment
        /// </summary>
        RequireAck = 1,
        /// <summary>
        ///
        /// </summary>
        FragmentSeqNumber = 2,
        /// <summary>
        /// Number to which this fragment applies
        /// </summary>
        MessageNumber = 8,
    }

    public class FragmentHeader
    {
        private FragmentType _type;
        /// <summary>
        /// Type of fragment
        /// </summary>
        public FragmentType Type { get { return _type; } }

        private byte _messageSeqNumber;
        /// <summary>
        /// Message Sequence number of the message to which the framgent belongs.
        /// </summary>
        public byte MessageSeqNumber { get { return _messageSeqNumber; } }

        private byte _fragmentSeqNumber;
        /// <summary>
        /// Sequence number of the fragment
        /// </summary>
        public byte FragmentSeqNumber { get { return _fragmentSeqNumber; } }

        private bool _isAckRequired;
        /// <summary>
        /// Does the received need to send an acknowledgment
        /// </summary>
        public bool IsAckRequired
        {
            get { return _isAckRequired; }
            set { _isAckRequired = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">type of fragment</param>
        /// <param name="messageSeqNumber">message sequence number</param>
        /// <param name="isAckRequired">is ack required</param>
        /// <param name="fragmentSeqNumber">fragment sequence number</param>
        public FragmentHeader(FragmentType type, byte messageSeqNumber, bool isAckRequired, byte fragmentSeqNumber)
        {
            _type = type;
            _messageSeqNumber = messageSeqNumber;
            _isAckRequired = isAckRequired;
            _fragmentSeqNumber = fragmentSeqNumber;
        }

        /// <summary>
        /// Generate UInt16 corresponding to header.
        /// </summary>
        /// <param name="header">header to be encoded</param>
        /// <returns>UInt16 representation of the header</returns>
        static public UInt16 Encode(FragmentHeader header)
        {
            UInt16 mask;
            UInt16 value = 0;
            UInt16 utype = (UInt16)header.Type;
            mask = 0x0001; // 1 bit for type
            value |= (UInt16)((utype & mask) << (byte)FragmentHeaderField.Type);
            value |= (UInt16)(header.MessageSeqNumber << (byte)FragmentHeaderField.MessageNumber);
            mask = 0x003F; // 6 bits for fragmentSeqNumber
            value |= (UInt16)((header.FragmentSeqNumber & mask) << (byte)FragmentHeaderField.FragmentSeqNumber);
            if (header.IsAckRequired)
            {
                mask = 0x0001;
                mask = (UInt16)(mask << (byte)FragmentHeaderField.RequireAck);
                value |= mask;
            }

            return value;
        }

        /// <summary>
        /// Convert UInt16 into a FragmentHeader
        /// </summary>
        /// <param name="value">encode header value</param>
        /// <returns>a new FragmentHeade</returns>
        static public FragmentHeader Decode(UInt16 value)
        {
            ushort mask = 0x0001; // 1 bit for type
            byte btype = (byte)((value >> (byte)FragmentHeaderField.Type) & mask);
            FragmentType type = (FragmentType)btype;
            mask = 0x0001 << (byte)FragmentHeaderField.RequireAck;
            bool isAckRequired = ((value & mask) > 0);
            mask = 0x00FF; // 8 bits for MessageNumber
            byte messageNumber = (byte)((value >> (byte)FragmentHeaderField.MessageNumber) & mask);
            mask = 0x003F; // 6 bits for FragmentSeqNumber
            byte fragmentSeqNumber = (byte)((value >> (byte)FragmentHeaderField.FragmentSeqNumber) & mask);
            return new FragmentHeader(type, messageNumber, isAckRequired, fragmentSeqNumber);
        }

    }

    /// General idea.
    /// One message is splitted into DATA fragments according to the maximum size of the PDU.
    /// As the DATA fragments are sent, if an acknowledgement is required, a ACK fragment is sent by the receiver, detailing
    /// which DATA fragments have already been received.
    ///
    /// Each DATA fragment has an index corresponding to the position of the fragment data with the message.
    /// The sequence number of a fragment is generated as the fragments are sent and its function is to identify duplicates.

    /// Structure of the fragment:
    /// -----------------------------------------------------
    /// | header   |               payload                   |
    /// | 2 bytes  |              (variable)                 |
    /// -----------------------------------------------------
    ///
    /// header:
    /// ------------------------------------------------------------------------
    /// | type    | isAckRequired | fragmentSequenceNumber |  messageSeqNumber |
    /// | (1 bit) |   (1 bit)     |        (6 bits)        |   (8 bits)        |
    /// ------------------------------------------------------------------------
    ///
    ///  Data fragment: (type = 0)
    /// ---------------------------------------------------------------------------
    /// | header   | Number of fragments |  Index of  fragment |   data           |
    /// | 2 bytes  |      (1byte)        |        (1byte)      |  (variable)      |
    /// ---------------------------------------------------------------------------
    ///
    ///
    /// ACK : (type = 1)
    /// ------------------------------------------------------------------------------------------------
    /// | header   | Number of fragments  |  frag. 0 missing | frag. 1 missing| ...| last frag missing |
    /// | 2 bytes  |        (1byte)       |    (1bit)        |  (1 bit)       |    |       (1 bit)     |
    /// ------------------------------------------------------------------------------------------------
    ///

    public abstract class Fragment
    {
        /// <summary>
        /// Create a frame containing an ACK fragment
        /// </summary>
        /// <param name="messageSeqNumber">message sequence number</param>
        /// <param name="fragmentSeqNumber">fragment sequence number</param>
        /// <param name="pduSize">max size for the sublayer payload</param>
        /// <param name="reservedTail">max size for the sublayer tail</param>
        /// <param name="reservedHead">max size for the sublayer header</param>
        /// <param name="lastFragmentSeqNumberReceived">last fragment sequence number received by the receiver</param>
        /// <param name="fragmentMissingTable">table of missing fragments</param>
        /// <returns>the generated frame</returns>
        public static Frame CreateAckFragmentFrame(byte messageSeqNumber, byte fragmentSeqNumber, int pduSize, int reservedTail, int reservedHead, byte lastFragmentSeqNumberReceived, BitArray fragmentMissingTable)
        {
            byte[] byteArray = BitArray.ToByteArray(fragmentMissingTable);
            if (pduSize < 2 /* header */ + 1 /*last frag nb rec. */  + byteArray.Length + 1 /* size bit array */)
                throw new ArgumentException("pdu size is too small", "pduSize");
            FragmentHeader header = new FragmentHeader(FragmentType.ACK, messageSeqNumber, false, fragmentSeqNumber);

            Frame frame = Frame.GetFrame(reservedHead + reservedTail + 2 + 1 + byteArray.Length + 1);
            frame.ReserveHeader(reservedHead);
            UInt16 serializedHdr = FragmentHeader.Encode(header);
            frame.AllocBack(4);
            frame.Write(0, serializedHdr);
            frame.Write(2, lastFragmentSeqNumberReceived);
            frame.Write(3, (byte)fragmentMissingTable.Length);
            frame.AppendToBack(byteArray, 0, byteArray.Length);
            return frame;
        }

        /// <summary>
        /// Create a frame containing a DATA fragment
        /// </summary>
        /// <param name="messageSeqNumber">message sequence number</param>
        /// <param name="fragmentSeqNumber">fragment sequence number</param>
        /// <param name="isBroadcast">true if fragment is broadcasted</param>
        /// <param name="requireAck">true if an acknowledgment is required</param>
        /// <param name="pduSize">max size for the sublayer payload</param>
        /// <param name="reservedTail">max size for the sublayer tail</param>
        /// <param name="reservedHead">max size for the sublayer header</param>
        /// <param name="nbFragments">number of DATA fragments in the message</param>
        /// <param name="fragmentIndex">index of the DATA fragment</param>
        /// <param name="payloadData">payload</param>
        /// <param name="indexPayloadData">start index in the payload </param>
        /// <param name="nbBytes">number of bytes in the payload</param>
        /// <returns>the generated frame</returns>
        public static Frame CreateDataFrame(byte messageSeqNumber, byte fragmentSeqNumber, bool isBroadcast, bool requireAck,
            int pduSize, int reservedTail, int reservedHead, byte nbFragments, byte fragmentIndex, Frame payloadData, int indexPayloadData, int nbBytes)
        {
            if (pduSize < 2 + 2 + nbBytes)
                throw new ArgumentException("pdu size is too small", "pduSize");
            FragmentHeader header = new FragmentHeader(FragmentType.DATA, messageSeqNumber, requireAck, fragmentSeqNumber);

            Frame frame = Frame.GetFrame(reservedHead + reservedTail + 2 + 2 + nbBytes);
            frame.ReserveHeader(reservedHead);
            UInt16 serializedHdr = FragmentHeader.Encode(header);
            frame.AllocBack(4);
            frame.Write(0, serializedHdr);
            frame.Write(2, nbFragments);
            frame.Write(3, fragmentIndex);
            frame.AppendToBack(payloadData, indexPayloadData, nbBytes);
            return frame;
        }

        /// <summary>
        /// Decode an ACK fragment payload
        /// </summary>
        /// <param name="payload">payload</param>
        /// <param name="lastFragmentSeqNumberReceived">last fragment sequence number received</param>
        /// <param name="fragmentMissingTable">table of indices of the missing fragments</param>
        public static void DecodeAckPayload(ref Frame payload, out byte lastFragmentSeqNumberReceived, out BitArray fragmentMissingTable)
        {
            if (payload.LengthDataUsed < 2)
                throw new ArgumentException("payload content too small", "payload");
            lastFragmentSeqNumberReceived = payload.ReadByte(0);
            int nbFragments = (int)payload.ReadByte(1);
            byte[] byteArray = new byte[payload.LengthDataUsed - 2];
            payload.ReadBytes(byteArray, 0, 2, payload.LengthDataUsed - 2);
            fragmentMissingTable = BitArray.FromByteArray(nbFragments, byteArray);
            Frame.Release(ref payload);
        }

        /// <summary>
        /// Decode DATA fragment payload
        /// </summary>
        /// <param name="payload">payload</param>
        /// <param name="nbFragments">number of fragments in the messge</param>
        /// <param name="fragmentIndex">index of the fragment</param>
        /// <param name="data">payload of the data fragment</param>
        public static void DecodeDataPayload(ref Frame payload, out byte nbFragments, out byte fragmentIndex, out Frame data)
        {
            if (payload.LengthDataUsed < 2)
                throw new ArgumentException("payload content too small", "payload");
            nbFragments = payload.ReadByte(0);
            fragmentIndex = payload.ReadByte(1);
            if (nbFragments == 0)
                throw new ArgumentOutOfRangeException("nbFragments");
            if (fragmentIndex >= nbFragments)
                throw new ArgumentOutOfRangeException("fragmentId");
            payload.DeleteFromFront(2);
            data = payload;
            payload = null;
        }
    }
}


