////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace Microsoft.SPOT.Wireless.IEEE_802_15_4.Network
{
    /// <summary>
    /// This class implements some of the 6LoWPAN messages
    /// </summary>
    public sealed class Messages6LoWPAN
    {
        /// <summary>
        /// Specified meaning of lower two bits of LoWPAN frames
        /// </summary>
        public enum Dispatch : byte
        {
            NonLowPan = 0,
            IpPacket = 1,
            Mesh = 2,
            Fragmentation = 3
        }

        /// <summary>
        /// ReadFromFrame message type from frame.
        /// </summary>
        /// <param name="sdu"></param>
        /// <param name="type"></param>
        /// <returns>Returns true on success</returns>
        public static bool GetType(Frame sdu, out Dispatch type)
        {
            type = Dispatch.NonLowPan; // set out type
            if (sdu == null || sdu.LengthDataUsed < 1)
                return false;
            byte b = (byte)(sdu.ReadByte(0) >> 6);
            type = (Dispatch)b;
            return true;
        }

        /// <summary>
        /// Container for MeshHeader
        /// </summary>
        public struct MeshHeader
        {
            public const int cLengthMin = 5;
            public const int cLengthMax = 6;

            public byte HopsLeft;
            public UInt16 originatorAddress;
            public UInt16 finalAddress;

            /// <summary>
            /// Encode message into frame header
            /// </summary>
            /// <param name="frame">Frame to add message</param>
            /// <returns>True on success</returns>
            public bool WriteToFrameHeader(Frame frame)
            {
                if (frame == null || frame.LengthHeaderAvail < cLengthMax)
                    return false;

                byte b = (byte)((int)Dispatch.Mesh << 6);
                b |= 32; // originator is short addr
                b |= 16; // target is short addr

                if (HopsLeft < 15)
                {
                    frame.AllocFront(cLengthMin);
                    b |= HopsLeft;
                    frame.Write(0, b);
                    frame.WriteCanonical(1, originatorAddress);
                    frame.WriteCanonical(3, finalAddress);
                }
                else
                {
                    frame.AllocFront(cLengthMax);
                    b |= 15;
                    frame.Write(0, b);
                    frame.Write(1, HopsLeft);
                    frame.WriteCanonical(2, originatorAddress);
                    frame.WriteCanonical(4, finalAddress);
                }

                return true;
            }

            /// <summary>
            /// Decode message from frame header
            /// </summary>
            /// <param name="frame">Frame containing message</param>
            /// <returns>True on success</returns>
            public bool ReadFromFrameHeader(Frame frame)
            {
                if (frame == null || frame.LengthDataUsed < cLengthMin)
                    return false;

                byte b = frame.ReadByte(0);
                if ((b >> 6) != (byte)Dispatch.Mesh ||
                    (b & 32) == 0 ||
                    (b & 16) == 0)
                    return false;
                HopsLeft = (byte)(b & 15);
                if (HopsLeft < 15)
                {
                    originatorAddress = frame.ReadUInt16Canonical(1);
                    finalAddress = frame.ReadUInt16Canonical(3);
                    frame.DeleteFromFront(cLengthMin);
                }
                else
                {
                    if (frame.LengthDataUsed < cLengthMax)
                        return false;
                    HopsLeft = frame.ReadByte(1);
                    originatorAddress = frame.ReadUInt16Canonical(2);
                    finalAddress = frame.ReadUInt16Canonical(4);
                    frame.DeleteFromFront(cLengthMax);
                }

                return true;
            }

            public override string ToString()
            {
                return "MeshHeader [HopsLeft: " + HopsLeft +
                    ", OriginatorAddr: 0x" + HexConverter.ConvertUintToHex(originatorAddress, 4) +
                    ", FinalAddress: 0x" + HexConverter.ConvertUintToHex(finalAddress, 4) + "]";
            }
        }

        /// <summary>
        /// Container for BroadcastHeader
        /// </summary>
        public struct BroadcastHeader
        {
            private const byte cDispatch = 80; // 01 010000 : LOWPAN_BC0
            public const int cLength = 2;

            public byte seqNo;

            /// <summary>
            /// Encode message into frame header
            /// </summary>
            /// <param name="frame">Frame to add message</param>
            /// <returns>True on success</returns>
            public bool WriteToFrameHeader(Frame frame)
            {
                if (frame == null || frame.LengthHeaderAvail < cLength)
                    return false;
                frame.AllocFront(cLength);
                frame.Write(0, cDispatch);
                frame.Write(1, seqNo);
                return true;
            }

            /// <summary>
            /// Decode message from frame header
            /// </summary>
            /// <param name="frame">Frame containing message</param>
            /// <returns>True on success</returns>
            public bool ReadFromFrameHeader(Frame frame)
            {
                if (frame == null || frame.LengthDataUsed < cLength)
                    return false;

                byte b = frame.ReadByte(0);
                if (b != cDispatch)
                    return false;
                seqNo = frame.ReadByte(1);
                frame.DeleteFromFront(cLength);
                return true;
            }

            public override string ToString()
            {
                return "BroadcastHeader [SeqNo: " + seqNo + "]";
            }
        }
    }
}


