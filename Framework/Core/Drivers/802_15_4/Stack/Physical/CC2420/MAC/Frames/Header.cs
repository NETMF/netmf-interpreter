////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using Microsoft.SPOT.Wireless.IEEE_802_15_4;

namespace Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac.Frames
{
    /// <summary>
    /// Table 79�Values of the Frame Type subfield
    /// </summary>
    public enum Type : byte
    {
        Beacon = 0,
        Data = 1,
        Ack = 2,
        Cmd = 3,
        Res4 = 4,
        Res5 = 5,
        Res6 = 6,
        Res7 = 7
    }

    /// <summary>
    /// Table 80�Possible values of the Destination Addressing Mode and Source Addressing Mode subfields
    /// </summary>
    public enum AddressingMode : byte
    {
        None = 0,
        Reserved = 1,
        Short = 2,
        Extended = 3
    }

    /// <summary>
    /// 7.2.1.1.7 Frame Version subfield
    /// </summary>
    public enum Version : byte
    {
        IEEE2003 = 0,
        IEEE2006 = 1,
        Res2 = 2,
        Res3 = 3
    }

    /// <summary>
    /// 7.2.1.1 Frame Control field
    /// </summary>
    public struct FCS
    {
        private UInt16 _value;

        private enum Offset : byte
        {
            Type = 0,
            Security = 3,
            Pending = 4,
            Ack = 5,
            PanIdCompression = 6,
            DstAddrMode = 10,
            Version = 12,
            SrcAddrMode = 14
        }

        private enum Mask : byte
        {
            Type = 0x7, // 3 bits
            Security = 1, // 1 bit
            Pending = 1, // 1 bit
            Ack = 1, // 1 bit
            PanIdCompression = 1, // 1 bit
            DstAddrMode = 0x3, // 2 bits
            Version = 0x3, // 2 bits
            SrcAddrMode = 0x3 // 2 bits
        }

        private UInt16 GetBits(Offset offset, Mask mask)
        {
            int i = (_value >> ((int)offset)) & ((int)mask);
            return (UInt16)i;
        }

        private void SetBits(Offset offset, Mask mask, int value)
        {
            int i = ((int)mask) << ((int)offset);
            _value &= (UInt16)(~i);
            _value |= (UInt16)(value << (int)offset);
        }

        private void SetBits(Offset offset, Mask mask, bool value)
        {
            SetBits(offset, mask, (value ? 1 : 0));
        }

        /// <summary>
        /// Encoded value of frame control field
        /// </summary>
        public UInt16 Value
        {
            get { return _value; }
            set { _value = value; }
        }

        /// <summary>
        /// 7.2.1.1.1 Frame Type subfield
        /// </summary>
        public Type Type
        {
            get { return (Type)GetBits(Offset.Type, Mask.Type); }
            set { SetBits(Offset.Type, Mask.Type, (int)value); }
        }

        /// <summary>
        /// 7.2.1.1.2 Security Enabled subfield
        /// </summary>
        public bool Security
        {
            get { return GetBits(Offset.Security, Mask.Security) != 0; }
            set { SetBits(Offset.Security, Mask.Security, value); }
        }

        /// <summary>
        /// 7.2.1.1.3 Frame Pending subfield
        /// </summary>
        public bool Pending
        {
            get { return GetBits(Offset.Pending, Mask.Pending) != 0; }
            set { SetBits(Offset.Pending, Mask.Pending, value); }
        }

        /// <summary>
        /// 7.2.1.1.4 Acknowledgment Request subfield
        /// </summary>
        public bool Ack
        {
            get { return GetBits(Offset.Ack, Mask.Ack) != 0; }
            set { SetBits(Offset.Ack, Mask.Ack, value); }
        }

        /// <summary>
        /// 7.2.1.1.5 PAN ID Compression subfield
        /// </summary>
        public bool PanIdCompression
        {
            get { return GetBits(Offset.PanIdCompression, Mask.PanIdCompression) != 0; }
            set { SetBits(Offset.PanIdCompression, Mask.PanIdCompression, value); }
        }

        /// <summary>
        /// 7.2.1.1.6 Destination Addressing Mode subfield
        /// </summary>
        public AddressingMode DstAddrMode
        {
            get { return (AddressingMode)GetBits(Offset.DstAddrMode, Mask.DstAddrMode); }
            set { SetBits(Offset.DstAddrMode, Mask.DstAddrMode, (int)value); }
        }

        /// <summary>
        /// 7.2.1.1.7 Frame Version subfield
        /// </summary>
        public Version Version
        {
            get { return (Version)GetBits(Offset.Version, Mask.Version); }
            set { SetBits(Offset.Version, Mask.Version, (int)value); }
        }

        /// <summary>
        /// 7.2.1.1.8 Source Addressing Mode subfield
        /// </summary>
        public AddressingMode SrcAddrMode
        {
            get { return (AddressingMode)GetBits(Offset.SrcAddrMode, Mask.SrcAddrMode); }
            set { SetBits(Offset.SrcAddrMode, Mask.SrcAddrMode, (int)value); }
        }

        /// <summary>
        /// Returns a string representation
        /// </summary>
        /// <returns>string representation</returns>
        public override string ToString()
        {
            string description = "";

            switch (Type)
            {
                case Type.Beacon:
                    description += "type: Beacon";
                    break;
                case Type.Data:
                    description += "type: Data";
                    break;
                case Type.Ack:
                    description += "type: Ack";
                    break;
                case Type.Cmd:
                    description += "type: Cmd";
                    break;
                default:
                    description += "type: res";
                    break;
            }

            if (Ack)
            {
                description += ", ack req.";
            }

            if (PanIdCompression)
            {
                description += ", PanId comp.";
            }

            if (Security)
            {
                description += ", security enabled";
            }

            if (Pending)
            {
                description += ", frame pending";
            }

            switch (Version)
            {
                case Version.IEEE2003:
                    description += "version: IEEE2003";
                    break;
                case Version.IEEE2006:
                    description += "version: IEEE2006";
                    break;
                case Version.Res2:
                    description += "version: Res2";
                    break;
                case Version.Res3:
                    description += "version: Res3";
                    break;
                default:
                    break;
            }

            switch (DstAddrMode)
            {
                case AddressingMode.None:
                    description += ", DstAddrMode: None";
                    break;
                case AddressingMode.Reserved:
                    description += ", DstAddrMode: Reserved";
                    break;
                case AddressingMode.Short:
                    description += ", DstAddrMode: Short";
                    break;
                case AddressingMode.Extended:
                    description += ", DstAddrMode: Ext.";
                    break;
                default:
                    break;
            }

            switch (SrcAddrMode)
            {
                case AddressingMode.None:
                    description += ", SrcAddrMode: None";
                    break;
                case AddressingMode.Reserved:
                    description += ", SrcAddrMode: Reserved";
                    break;
                case AddressingMode.Short:
                    description += ", SrcAddrMode: Short";
                    break;
                case AddressingMode.Extended:
                    description += ", SrcAddrMode: Ext.";
                    break;
                default:
                    break;
            }

            return description;
        }
    }

    /// <summary>
    /// 7.2.1 General Mac frame header
    /// </summary>
    public struct Header
    {
        /// <summary>
        /// 7.2.1.1 Frame Control field
        /// </summary>
        public FCS fcs;
        /// <summary>
        /// 7.2.1.2 Sequence Number field
        /// </summary>
        public Byte seqNo;
        /// <summary>
        /// 7.2.1.3 Destination PAN Identifier field. Valid if fcs.DstAddrMode!=none
        /// </summary>
        public UInt16 dstPanId;
        /// <summary>
        /// 7.2.1.4 Destination Address field: short address. Valid if fcs.DstAddrMode==short
        /// </summary>
        public UInt16 dstAddrShort;
        /// <summary>
        /// 7.2.1.4 Destination Address field: extended address. Valid if fcs.DstAddrMode==extended
        /// </summary>
        public UInt64 dstAddrExt;
        /// <summary>
        /// 7.2.1.5 Source PAN Identifier field. Valid if fcs.SrcAddrMode!=none and fcs.PanIdCompression==false
        /// </summary>
        public UInt16 srcPanId;
        /// <summary>
        /// 7.2.1.6 Source Address field: short address. Valid if fcs.SrcAddrMode==short
        /// </summary>
        public UInt16 srcAddrShort; // present if fcs.SrcAddrMode==short
        /// <summary>
        /// 7.2.1.6 Source Address field: extended address. Valid if fcs.SrcAddrMode==extended
        /// </summary>
        public UInt64 srcAddrExt; // present if fcs.SrcAddrMode==extended
        /// <summary>
        /// Calculates the size of the encoded header in bytes
        /// </summary>
        /// <returns></returns>
        public int Length()
        {
            int len = 2 + 1; // fcs + seqNo

            AddressingMode dstMode = fcs.DstAddrMode;
            AddressingMode srcMode = fcs.SrcAddrMode;
            bool panIdCompression = fcs.PanIdCompression;
            if (dstMode != AddressingMode.None)
                len += 2; // dstPanId
            if (dstMode == AddressingMode.Short)
                len += 2; // dstAddrShort
            if (dstMode == AddressingMode.Extended)
                len += 8; // dstAddrShort
            if (srcMode != AddressingMode.None && !panIdCompression)
                len += 2; // srcPanId
            if (srcMode == AddressingMode.Short)
                len += 2; // srcAddrShort
            if (srcMode == AddressingMode.Extended)
                len += 8; // srcAddrShort
            // FIXME: secHeader
            return len;
        }

        /// <summary>
        /// Encodes the header into the front of the frame
        /// </summary>
        /// <param name="frame">The header is placed at the front of this frame</param>
        /// <returns>True on success, false if the frame is too small or the header is invalid</returns>
        public bool WriteToFrameHeader(Frame frame)
        {
            int len = Length();
            if (frame.LengthHeaderAvail < len)
                return false;

            frame.AllocFront(len);
            frame.WriteCanonical(0, fcs.Value);
            frame.Write(2, seqNo);
            int offset = 3;
            AddressingMode dstMode = fcs.DstAddrMode;
            AddressingMode srcMode = fcs.SrcAddrMode;
            bool panIdCompression = fcs.PanIdCompression;

            if (dstMode == AddressingMode.Reserved || srcMode == AddressingMode.Reserved ||
                (panIdCompression && dstMode == AddressingMode.None))
                return false;

            if (dstMode != AddressingMode.None)
            { // dstPanId
                frame.WriteCanonical(offset, dstPanId);
                offset += 2;
            }

            if (dstMode == AddressingMode.Short)
            { // dstAddrShort
                frame.WriteCanonical(offset, dstAddrShort);
                offset += 2;
            }

            if (dstMode == AddressingMode.Extended)
            { // dstAddrShort
                frame.WriteCanonical(offset, dstAddrExt);
                offset += 8;
            }

            if (srcMode != AddressingMode.None && !panIdCompression)
            { // srcPanId
                frame.WriteCanonical(offset, srcPanId);
                offset += 2;
            }

            if (srcMode == AddressingMode.Short)
            { // dstAddrShort
                frame.WriteCanonical(offset, srcAddrShort);
                offset += 2;
            }

            if (srcMode == AddressingMode.Extended)
            { // dstAddrShort
                frame.WriteCanonical(offset, srcAddrExt);
                offset += 8;
            }

            // FIXME: secHeader
            return true;
        }

        /// <summary>
        /// Decodes a header
        /// </summary>
        /// <param name="frame">The frame containing the header</param>
        /// <param name="removeHeader">If true, the header will be removed from the frame after decoding</param>
        /// <returns>True on success, false on decoding error</returns>
        public bool ReadFromFrameHeader(Frame frame, bool removeHeader)
        {
            int len = 2 + 1; // fcs + seqNo

            // check minimum length
            if (frame.LengthDataUsed < len)
                return false;
            fcs.Value = frame.ReadUInt16Canonical(0);
            seqNo = frame.ReadByte(2);

            // calculate variable length
            len = Length();
            if (frame.LengthDataUsed < len)
                return false;

            AddressingMode dstMode = fcs.DstAddrMode;
            AddressingMode srcMode = fcs.SrcAddrMode;
            bool panIdCompression = fcs.PanIdCompression;
            int offset = 3;
            if (dstMode != AddressingMode.None)
            { // dstPanId
                dstPanId = frame.ReadUInt16Canonical(offset);
                offset += 2;
            }

            if (dstMode == AddressingMode.Short)
            { // dstAddrShort
                dstAddrShort = frame.ReadUInt16Canonical(offset);
                offset += 2;
            }

            if (dstMode == AddressingMode.Extended)
            { // dstAddrShort
                dstAddrExt = frame.ReadUInt64Canonical(offset);
                offset += 8;
            }

            if (srcMode != AddressingMode.None && !panIdCompression)
            { // srcPanId
                srcPanId = frame.ReadUInt16Canonical(offset);
                offset += 2;
            }

            if (srcMode == AddressingMode.Short)
            { // dstAddrShort
                srcAddrShort = frame.ReadUInt16Canonical(offset);
                offset += 2;
            }

            if (srcMode == AddressingMode.Extended)
            { // dstAddrShort
                srcAddrExt = frame.ReadUInt64Canonical(offset);
                offset += 8;
            }

            // FIXME: secHeader

            if (removeHeader)
                frame.DeleteFromFront(len);
            return true;
        }

        /// <summary>
        /// ReadFromFrame frame type
        /// </summary>
        /// <param name="frame">frame containing encoded Mac SDU</param>
        /// <returns>type, reserved value on error</returns>
        static public bool GetType(Frame frame, out Type type)
        {
            if (frame != null && frame.LengthDataUsed >= 2)
            {
                FCS fcs = new FCS();
                fcs.Value = frame.ReadUInt16Canonical(0);
                type = fcs.Type;
                return true;
            }

            type = Type.Ack; // any value
            return false;
        }

        /// <summary>
        /// Returns a string representation  of the object
        /// </summary>
        /// <returns>a string represetnation</returns>
        public override string ToString()
        {
            string description = fcs.ToString();
            description += ", seqNo: " + seqNo.ToString();
            AddressingMode dstMode = fcs.DstAddrMode;
            AddressingMode srcMode = fcs.SrcAddrMode;

            switch (dstMode)
            {
                case AddressingMode.None:
                    description += ", no dst";
                    break;
                case AddressingMode.Reserved:
                    description += ", bad dst";
                    break;
                case AddressingMode.Short:
                    description += ", dstPanId: " + dstPanId.ToString();
                    description += ", dstAddrShort: " + HexConverter.ConvertUintToHex(dstAddrShort, 4);
                    break;
                case AddressingMode.Extended:
                    description += ", dstPanId: " + dstPanId.ToString();
                    description += ", dstAddrExt: " + HexConverter.ConvertUint64ToHex(dstAddrExt, 16);
                    break;
                default:
                    description += ", bad dst";
                    break;
            }

            switch (srcMode)
            {
                case AddressingMode.None:
                    description += ", no src";
                    break;
                case AddressingMode.Reserved:
                    description += ", bad src";
                    break;
                case AddressingMode.Short:
                    if (!fcs.PanIdCompression) { description += ", srcPanId: " + srcPanId.ToString(); }
                    description += ", srcAddrShort: " + HexConverter.ConvertUintToHex(srcAddrShort, 4);
                    break;
                case AddressingMode.Extended:
                    if (!fcs.PanIdCompression) { description += ", srcPanId: " + srcPanId.ToString(); }
                    description += ", srcAddrExt: " + HexConverter.ConvertUint64ToHex(srcAddrExt, 16);
                    break;
                default:
                    description += ", bad src";
                    break;
            }

            // FIXME: secHeader
            return description;
        }
    }
}


