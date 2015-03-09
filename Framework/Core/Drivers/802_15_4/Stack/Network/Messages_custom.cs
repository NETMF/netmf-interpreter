////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace Microsoft.SPOT.Wireless.IEEE_802_15_4.Network
{
    public sealed class Message
    {
        // message type
        public enum Type : byte
        {
            Reserved = 0,
            AddressRequest = 1,
            AddressReply = 2,
            RouteRequest = 3,
            RouteReply = 4,
            RouteError = 5,
            NeighborhoodDiscovery = 6,
            Data = 7,
            //DiscoveryRequest = 8,
            DiscoveryReply = 8
        }

        /// <summary>
        /// Decode message type from frame.
        /// </summary>
        /// <param name="sdu"></param>
        /// <param name="type"></param>
        /// <returns>Returns true on success</returns>
        public static bool GetType(Frame sdu, out Type type)
        {
            type = Type.Reserved;
            if (sdu == null || sdu.LengthDataUsed < 1)
                return false;
            byte b = (byte)(sdu.ReadByte(0) >> 2);
            if (b > 8)
                return false;
            type = (Message.Type)b;
            return true;
        }

        /// <summary>
        /// Container for AddressRequest
        /// </summary>
        public struct AddressRequest
        {
            public const int cLength = 1 + 1 + 2 + 8;
            public Byte HopsLeft;
            public UInt16 BrokerAddr;
            public UInt64 DeviceAddr;

            /// <summary>
            /// Encode message into frame
            /// </summary>
            /// <param name="frame">Frame to add message</param>
            /// <returns>True on success</returns>
            public bool WriteToFrame(Frame frame)
            {
                if (frame == null || frame.LengthDataAvail < cLength)
                    return false;
                frame.AllocBack(cLength);
                byte Header = (Byte)((int)Type.AddressRequest << 2);
                frame.Write(0, Header);
                frame.Write(1, HopsLeft);
                frame.WriteCanonical(2, BrokerAddr);
                frame.WriteCanonical(4, DeviceAddr);
                return true;
            }

            /// <summary>
            /// Decode message from frame
            /// </summary>
            /// <param name="frame">Frame containing message</param>
            /// <returns>True on success</returns>
            public bool ReadFromFrame(Frame frame)
            {
                if (frame == null || frame.LengthDataUsed != cLength)
                    return false;
                byte Header = frame.ReadByte(0);
                if ((Type)(Header >> 2) != Type.AddressRequest)
                    return false;
                HopsLeft = frame.ReadByte(1);
                BrokerAddr = frame.ReadUInt16Canonical(2);
                DeviceAddr = frame.ReadUInt64Canonical(4);
                return true;
            }

            public override string ToString()
            {
                return "AddressRequest [HopsLeft: " + HopsLeft +
                    ", BrokerAddr: 0x" + HexConverter.ConvertUintToHex(BrokerAddr, 4) +
                    ", DeviceAddr: 0x" + HexConverter.ConvertUint64ToHex(DeviceAddr, 16) + "]";
            }
        }

        /// <summary>
        /// Container for AddressReply
        /// </summary>
        public struct AddressReply
        {
            public const int cLength = 1 + 1 + 2 + 8 + 2 + 2;
            public Byte HopsLeft;
            public UInt16 BrokerAddr;
            public UInt64 DeviceAddr;
            public UInt16 ShortAddr;
            public UInt16 DiscoveryInterval;

            /// <summary>
            /// Encode message into frame
            /// </summary>
            /// <param name="frame">Frame to add message</param>
            /// <returns>True on success</returns>
            public bool WriteToFrame(Frame frame)
            {
                if (frame == null || frame.LengthDataAvail < cLength)
                    return false;
                frame.AllocBack(cLength);
                byte Header = (Byte)((int)Type.AddressReply << 2);
                frame.Write(0, Header);
                frame.Write(1, HopsLeft);
                frame.WriteCanonical(2, BrokerAddr);
                frame.WriteCanonical(4, DeviceAddr);
                frame.WriteCanonical(12, ShortAddr);
                frame.Write(14, DiscoveryInterval);
                return true;
            }

            /// <summary>
            /// Decode message from frame
            /// </summary>
            /// <param name="frame">Frame containing message</param>
            /// <returns>True on success</returns>
            public bool ReadFromFrame(Frame frame)
            {
                if (frame == null || frame.LengthDataUsed != cLength)
                    return false;
                byte Header = frame.ReadByte(0);
                if ((Type)(Header >> 2) != Type.AddressReply)
                    return false;
                HopsLeft = frame.ReadByte(1);
                BrokerAddr = frame.ReadUInt16Canonical(2);
                DeviceAddr = frame.ReadUInt64Canonical(4);
                ShortAddr = frame.ReadUInt16Canonical(12);
                DiscoveryInterval = frame.ReadUInt16(14);
                return true;
            }

            public override string ToString()
            {
                return "AddressReply [HopsLeft: " + HopsLeft +
                    ", BrokerAddr: 0x" + HexConverter.ConvertUintToHex(BrokerAddr, 4) +
                    ", DeviceAddr: 0x" + HexConverter.ConvertUint64ToHex(DeviceAddr, 16) +
                    ", ShortAddr: 0x" + HexConverter.ConvertUintToHex(ShortAddr, 4) +
                    ", DiscoveryInterval: " + DiscoveryInterval + "]";
            }
        }

        /// <summary>
        /// Container for RoutingMessage
        /// </summary>
        public struct RoutingMessage
        {
            public const int cLength = 5 + 2 + 2;
            public bool IsRequest; // RReq or RRep?
            public Byte HopsLeft;
            public Byte HopCount;
            public Byte MinLQI;
            public Byte SeqNo;
            public UInt16 TargetAddr;
            public UInt16 OriginatorAddr;

            /// <summary>
            /// Encode message into frame
            /// </summary>
            /// <param name="frame">Frame to add message</param>
            /// <returns>True on success</returns>
            public bool WriteToFrame(Frame frame)
            {
                if (frame == null || frame.LengthDataAvail < cLength)
                    return false;
                frame.AllocBack(cLength);
                byte Header;
                if (IsRequest)
                    Header = (Byte)((int)Type.RouteRequest << 2);
                else
                    Header = (Byte)((int)Type.RouteReply << 2);
                frame.Write(0, Header);
                frame.Write(1, HopsLeft);
                frame.Write(2, HopCount);
                frame.Write(3, MinLQI);
                frame.Write(4, SeqNo);
                frame.WriteCanonical(5, TargetAddr);
                frame.WriteCanonical(7, OriginatorAddr);
                return true;     
            }                 

            /// <summary>
            /// Decode message from frame
            /// </summary>
            /// <param name="frame">Frame containing message</param>
            /// <returns>True on success</returns>
            public bool ReadFromFrame(Frame frame)
            {
                if (frame == null || frame.LengthDataUsed != cLength)
                    return false;
                byte Header = frame.ReadByte(0);
                Type type = (Type)(Header >> 2);
                if (type == Type.RouteRequest)
                {
                    IsRequest = true;
                }
                else if (type == Type.RouteReply)
                {
                    IsRequest = false;
                }
                else
                {
                    return false;
                }

                HopsLeft = frame.ReadByte(1);
                HopCount = frame.ReadByte(2);
                MinLQI = frame.ReadByte(3);
                SeqNo = frame.ReadByte(4);
                TargetAddr = frame.ReadUInt16Canonical(5);
                OriginatorAddr = frame.ReadUInt16Canonical(7);
                return true;
            }

            public override string ToString()
            {
                string res;
                if (IsRequest)
                    res = "RouteRequest";
                else
                    res = "RouteReply";

                return res + " [HopsLeft: " + HopsLeft +
                    ", HopCount: " + HopCount +
                    ", MinLQI: " + MinLQI +
                    ", SeqNo: " + SeqNo +
                    ", TargetAddr: 0x" + HexConverter.ConvertUintToHex(TargetAddr, 4) +
                    ", OriginatorAddr: 0x" + HexConverter.ConvertUintToHex(OriginatorAddr, 4) + "]";
            }
        }

        /// <summary>
        /// Container for RouteError
        /// </summary>
        public struct RouteError
        {
            public const int cLength = 1 + 1 + 2 + 2 + 2;
            public bool FatalError;
            public Byte HopsLeft;
            public UInt16 TargetAddr;
            public UInt16 OriginatorAddr;
            public UInt16 UnreachableAddr;

            /// <summary>
            /// Encode message into frame
            /// </summary>
            /// <param name="frame">Frame to add message</param>
            /// <returns>True on success</returns>
            public bool WriteToFrame(Frame frame)
            {
                if (frame == null || frame.LengthDataAvail < cLength)
                    return false;
                frame.AllocBack(cLength);
                byte Header = (Byte)((int)Type.RouteError << 2);
                if (FatalError)
                    Header |= 2;
                frame.Write(0, Header);
                frame.Write(1, HopsLeft);
                frame.WriteCanonical(2, TargetAddr);
                frame.WriteCanonical(4, OriginatorAddr);
                frame.WriteCanonical(6, UnreachableAddr);
                return true;
            }

            /// <summary>
            /// Decode message from frame
            /// </summary>
            /// <param name="frame">Frame containing message</param>
            /// <returns>True on success</returns>
            public bool ReadFromFrame(Frame frame)
            {
                if (frame == null || frame.LengthDataUsed != cLength)
                    return false;
                byte Header = frame.ReadByte(0);
                if ((Type)(Header >> 2) != Type.RouteError)
                    return false;
                FatalError = ((Header & 2) > 0);
                HopsLeft = frame.ReadByte(1);
                TargetAddr = frame.ReadUInt16Canonical(2);
                OriginatorAddr = frame.ReadUInt16Canonical(4);
                UnreachableAddr = frame.ReadUInt16Canonical(6);
                return true;
            }

            public override string ToString()
            {
                return "RouteError [HopsLeft: " + HopsLeft +
                    ", TargetAddr: 0x" + HexConverter.ConvertUintToHex(TargetAddr, 4) +
                    ", OriginatorAddr: 0x" + HexConverter.ConvertUintToHex(OriginatorAddr, 4) +
                    ", UnreachableAddr: 0x" + HexConverter.ConvertUintToHex(UnreachableAddr, 4) + "]";
            }
        }

        /// <summary>
        /// Container for NeighborhoodDiscovery
        /// </summary>
        public struct NeighborhoodDiscovery
        {
            public struct Neighbour
            {
                public UInt16 Address;
                public Byte Lqi;
            }

            /// <summary>
            /// maximum number of neighbors in one message
            /// </summary>
            public const int cMaxNeighbours = 30; // FIXME: depends on MAC configuration

            public Neighbour[] Neighbours;

            public int Length()
            {
                int n = 0;
                if (Neighbours != null)
                {
                    n = Neighbours.Length;
                    if (n > cMaxNeighbours)
                        n = cMaxNeighbours;
                }

                return 1 + 3 * n;
            }

            /// <summary>
            /// Encode message into frame
            /// </summary>
            /// <param name="frame">Frame to add message</param>
            /// <returns>True on success</returns>
            public bool WriteToFrame(Frame frame)
            {
                int n = 0;
                if (Neighbours != null)
                {
                    n = Neighbours.Length;
                    if (n > cMaxNeighbours)
                        n = cMaxNeighbours;
                }

                int len = 1 + 3 * n;

                if (frame == null || frame.LengthDataAvail < len)
                    return false;
                frame.AllocBack(len);
                byte Header = (Byte)((int)Type.NeighborhoodDiscovery << 2);
                frame.Write(0, Header);

                int offset = 1;
                for (int i = 0; i < n; i++)
                {
                    frame.WriteCanonical(offset, Neighbours[i].Address);
                    offset += 2;
                    frame.Write(offset, Neighbours[i].Lqi);
                    offset++;
                }

                return true;
            }

            /// <summary>
            /// Decode message from frame
            /// </summary>
            /// <param name="frame">Frame containing message</param>
            /// <returns>True on success</returns>
            public bool ReadFromFrame(Frame frame)
            {
                if (frame == null || frame.LengthDataUsed < 1)
                    return false;
                byte Header = frame.ReadByte(0);
                if ((Type)(Header >> 2) != Type.NeighborhoodDiscovery)
                    return false;
                int n = frame.LengthDataUsed - 1;
                if (n % 3 != 0)
                    return false;
                if (n > 0)
                {
                    n /= 3;
                    Neighbours = new Neighbour[n];
                    int offset = 1;
                    for (int i = 0; i < n; i++)
                    {
                        Neighbours[i].Address = frame.ReadUInt16Canonical(offset);
                        offset += 2;
                        Neighbours[i].Lqi = frame.ReadByte(offset);
                        offset++;
                    }
                }

                return true;
            }

            public override string ToString()
            {
                string res = "NeighborhoodDiscovery [";
                if (Neighbours != null)
                {
                    int length = Neighbours.Length;
                    for (int i = 0; i < length; i++)
                    {
                        if (i > 0)
                            res += ", ";
                        res += "[Address: 0x" + HexConverter.ConvertUintToHex(Neighbours[i].Address, 4) +
                            ", LQI: " + Neighbours[i].Lqi + "]";
                    }
                }

                res += "]";
                return res;
            }
        }

        /// <summary>
        /// Container for Data (non-6LoWPAN encapsulation)
        /// </summary>
        public struct Data
        {
            public const int cLength = 1;

            /// <summary>
            /// Encode message into frame header
            /// </summary>
            /// <param name="frame">Frame to add message</param>
            /// <returns>True on success</returns>
            public bool WriteToFrameHeader(Frame frame)
            {
                if (frame == null || frame.LengthDataAvail < cLength)
                    return false;
                frame.AllocFront(cLength);
                byte Header = (Byte)((int)Type.Data << 2);
                frame.Write(0, Header);
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
                byte Header = frame.ReadByte(0);
                if ((Type)(Header >> 2) != Type.Data)
                    return false;
                frame.DeleteFromFront(1);
                return true;
            }

            public override string ToString()
            {
                return "Data";
            }
        }

        /// <summary>
        /// Container for DiscoveryRequest
        /// </summary>
        public struct DiscoveryReply
        {
            public const int cLength = 1;

            /// <summary>
            /// Encode message into frame
            /// </summary>
            /// <param name="frame">Frame to add message</param>
            /// <returns>True on success</returns>
            public bool WriteToFrame(Frame frame)
            {
                if (frame == null || frame.LengthDataAvail < cLength)
                    return false;
                frame.AllocBack(cLength);
                byte Header = (Byte)((int)Type.DiscoveryReply << 2);
                frame.Write(0, Header);
                return true;
            }

            /// <summary>
            /// Decode message from frame
            /// </summary>
            /// <param name="frame">Frame containing message</param>
            /// <returns>True on success</returns>
            public bool ReadFromFrame(Frame frame)
            {
                if (frame == null || frame.LengthDataUsed != cLength)
                    return false;
                byte Header = frame.ReadByte(0);
                if ((Type)(Header >> 2) != Type.DiscoveryReply)
                    return false;
                return true;
            }

            public override string ToString()
            {
                return "DiscoveryReply";
            }
        }
    }
}


