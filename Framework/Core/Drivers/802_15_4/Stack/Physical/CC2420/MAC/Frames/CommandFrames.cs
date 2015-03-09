////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using Microsoft.SPOT.Wireless.IEEE_802_15_4;

namespace Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac.Frames
{
    // Command frames: Header+Command+Payload+FCS

    /// <summary>
    /// This class implements the Mac command frames specified in 7.3
    /// </summary>
    public sealed class Command
    {
        /// <summary>
        /// Table 82�Mac command frames
        /// </summary>
        public enum Type : byte
        {
            AssociationRequest = 0x01,
            AssociationResponse = 0x02,
            DisassociationNotification = 0x03,
            DataRequest = 0x04,
            PanIdConflictNotification = 0x05,
            OrphanNotification = 0x06,
            BeaconRequest = 0x07,
            CoordinatorRealignment = 0x08,
            GtsRequest = 0x09,
            reserved = 0xFF
        }

        /// <summary>
        /// Get type of frame containing Mac command frame
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        public static Type DecodeType(Frame frame)
        {
            if (frame == null || frame.LengthDataUsed == 0)
                return Type.reserved;
            return (Type)frame.ReadByte(0);
        }

        /// <summary>
        /// This class implements a generic encoder/decoder for Mac command frames
        /// </summary>
        abstract public class Base
        {
            readonly Type _type;
            readonly int _lengthMin;
            readonly int _lengthMax;

            protected Base(Type type, int lengthPayloadMin, int lengthPayloadMax)
            {
                _type = type;
                _lengthMin = lengthPayloadMin + 1; // 1 byte for command type
                _lengthMax = lengthPayloadMax + 1;
            }

            /// <summary>
            /// Maximum size of encoded frame
            /// </summary>
            public int LengthMax
            {
                get { return _lengthMax; }
            }

            /// <summary>
            /// Encode command frame
            /// </summary>
            /// <param name="frame">target buffer</param>
            /// <returns>True on success</returns>
            public bool WriteToFrame(Frame frame)
            {
                if (frame == null || frame.LengthDataAvail < _lengthMax || frame.LengthDataUsed != 0)
                    return false;
                frame.ReserveHeader(frame.LengthHeaderAvail + 1); // increase header space by one byte
                frame.AllocBack(_lengthMax - 1); // reserve space without command type, so payload is at 0..n-1
                EncodePayload(frame);
                frame.AllocFront(1);
                frame.Write(0, (byte)_type);
                return true;
            }

            /// <summary>
            /// Decode command frame
            /// </summary>
            /// <param name="frame">source buffer</param>
            /// <returns>True on success</returns>
            public bool ReadFromFrame(ref Frame frame)
            {
                if (frame == null || frame.LengthDataUsed < _lengthMin || frame.LengthDataUsed > _lengthMax)
                    return false;
                if (_type != (Type)frame.ReadByte(0))
                    return false;
                frame.DeleteFromFront(1);
                DecodePayload(frame);
                Frame.Release(ref frame);
                return true;
            }

            abstract protected void EncodePayload(Frame frame);

            abstract protected void DecodePayload(Frame frame);
        }

        /// <summary>
        /// This class implements a command frame that has no payload
        /// </summary>
        public class CommandFrameNoParams : Base
        {
            public CommandFrameNoParams(Type type)
                : base(type, 0, 0)
            { }

            protected override void EncodePayload(Frame frame)
            { }

            protected override void DecodePayload(Frame frame)
            { }
        }

        /// <summary>
        /// 7.3.1 Association request command
        /// </summary>
        public class AssociationRequest : Base
        {
            public CapabilityInformation capabilities;

            public AssociationRequest()
                : base(Type.AssociationRequest, 1, 1)
            { }

            protected override void EncodePayload(Frame frame)
            {
                frame.Write(0, capabilities.ToByte());
            }

            protected override void DecodePayload(Frame frame)
            {
                capabilities.FromByte(frame.ReadByte(0));
            }

            public override string ToString()
            {
                return "AssociationRequest [" + capabilities.ToString() + "]";
            }

        }

        /// <summary>
        /// 7.3.2 Association response command
        /// </summary>
        public class AssociationResponse : Base
        {
            public enum Status : byte
            {
                AssociationSuccessful = 0x00,
                PanAtCapacity = 0x01,
                PanAccessDenied = 0x02
            }

            public UInt16 shortAddr;
            public Status status;

            public AssociationResponse() : base(Type.AssociationResponse, 3, 3) { }

            protected override void EncodePayload(Frame frame)
            {
                frame.Write(0, shortAddr);
                frame.Write(2, (byte)status);
            }

            protected override void DecodePayload(Frame frame)
            {
                shortAddr = frame.ReadUInt16(0);
                status = (Status)frame.ReadByte(2);
            }

            public override string ToString()
            {
                switch (status)
                {
                    case Status.AssociationSuccessful:
                        return "AssociationResponse [status: AssociationSuccessful, shortAddr: " + HexConverter.ConvertUintToHex(shortAddr, 4) + "]";

                    case Status.PanAtCapacity:
                        return "AssociationResponse [status: PanAtCapacity]";

                    case Status.PanAccessDenied:
                        return "AssociationResponse [status: PanAccessDenied]";

                    default:
                        return "AssociationResponse [unknown status]";
                }

            }
        }

        /// <summary>
        /// 7.3.3 Disassociation notification command
        /// </summary>
        public class DisassociationNotification : Base
        {
            public enum Reason : byte
            {
                CoordinatorInitiated = 0x01,
                DeviceInitiated = 0x02
            }

            public Reason reason;

            public DisassociationNotification() : base(Type.DisassociationNotification, 1, 1) { }

            protected override void EncodePayload(Frame frame)
            {
                frame.Write(0, (byte)reason);
            }

            protected override void DecodePayload(Frame frame)
            {
                reason = (Reason)frame.ReadByte(0);
            }

            public override string ToString()
            {
                switch (reason)
                {
                    case Reason.CoordinatorInitiated:
                        return "DisassociationNotification [CoordinatorInitiated]";
                    case Reason.DeviceInitiated:
                        return "DisassociationNotification [DeviceInitiated]";
                    default:
                        return "DisassociationNotification [unknown reason]";
                }
            }
        }

        /// <summary>
        /// 7.3.4 Data request command
        /// </summary>
        public class DataRequest : CommandFrameNoParams
        {
            public DataRequest() : base(Type.DataRequest) { }

            public override string ToString()
            {
                return "DataRequest";
            }
        }

        /// <summary>
        /// 7.3.5 PAN ID conflict notification command
        /// </summary>
        public class PanIdConflictNotification : CommandFrameNoParams
        {
            public PanIdConflictNotification() : base(Type.PanIdConflictNotification) { }

            public override string ToString()
            {
                return "PanIdConflictNotification";
            }
        }

        /// <summary>
        /// 7.3.6 Orphan notification command
        /// </summary>
        public class OrphanNotification : CommandFrameNoParams
        {
            public OrphanNotification() : base(Type.OrphanNotification) { }

            public override string ToString()
            {
                return "OrphanNotification";
            }
        }

        /// <summary>
        /// 7.3.7 Beacon request command
        /// </summary>
        public class BeaconRequest : CommandFrameNoParams
        {
            public BeaconRequest() : base(Type.BeaconRequest) { }

            public override string ToString()
            {
                return "BeaconRequest";
            }
        }

        /// <summary>
        /// 7.3.8 Coordinator realignment command
        /// </summary>
        public class CoordinatorRealignment : Base
        {
            public UInt16 panId;
            public UInt16 coordinatorShortAddr;
            public Byte channel;
            public UInt16 shortAddr;
            public Byte channelPage;
            public bool channelPagePresent;

            public CoordinatorRealignment() : base(Type.CoordinatorRealignment, 7, 8) { }

            protected override void EncodePayload(Frame frame)
            {
                frame.Write(0, panId);
                frame.Write(2, coordinatorShortAddr);
                frame.Write(4, channel);
                frame.Write(5, shortAddr);
                if (channelPagePresent)
                    frame.Write(7, channelPage);
                else
                    frame.DeleteFromBack(1);
            }

            protected override void DecodePayload(Frame frame)
            {
                panId = frame.ReadUInt16(0);
                coordinatorShortAddr = frame.ReadUInt16(2);
                channel = frame.ReadByte(4);
                shortAddr = frame.ReadUInt16(5);
                if (frame.LengthDataUsed == 8)
                {
                    channelPage = frame.ReadByte(7);
                    channelPagePresent = true;
                }
                else
                {
                    channelPagePresent = false;
                }
            }

            /// <summary>
            /// Returns textual representation
            /// </summary>
            /// <returns>textual repressentation</returns>
            public override string ToString()
            {
                string description = "CoordinatorRealignment [";
                description += "panId: " + HexConverter.ConvertUintToHex(panId, 4);
                description += ", coordinatorShortAddr: " + HexConverter.ConvertUintToHex(coordinatorShortAddr, 4);
                description += ", channel: " + channel.ToString();
                description += ", shortAddr: " + HexConverter.ConvertUintToHex(shortAddr, 4);
                description += "]";
                return description;
            }
        }

        /// <summary>
        /// 7.3.9 GTS request command
        /// </summary>
        public class GtsRequest : Base
        {
            public Byte gtsLength; // 4 bits
            public Byte gtsDirection; // 1 bit
            public Byte characteristicsType; // 1 bit

            public GtsRequest() : base(Type.GtsRequest, 1, 1) { }

            protected override void EncodePayload(Frame frame)
            {
                int i = gtsLength | (gtsDirection << 4) | (characteristicsType << 5);
                frame.Write(0, (byte)i);
            }

            protected override void DecodePayload(Frame frame)
            {
                byte b = frame.ReadByte(0);
                gtsLength = (byte)(b & 0xF);
                gtsDirection = (byte)((b >> 4) & 1);
                characteristicsType = (byte)((b >> 5) & 1);
            }

            public override string ToString()
            {
                string description = "GtsRequest [";
                description += "gtsLength: " + gtsLength.ToString();
                description += ", gtsDirection: " + gtsDirection.ToString();
                description += ", characteristicType: " + characteristicsType.ToString();
                description += "]";
                return description;
            }
        }
    }
}


