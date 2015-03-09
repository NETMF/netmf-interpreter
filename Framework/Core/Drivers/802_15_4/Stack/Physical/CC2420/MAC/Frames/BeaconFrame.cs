////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using Microsoft.SPOT.Wireless.IEEE_802_15_4;

namespace Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac.Frames
{
    /// <summary>
    /// Local storage of a beacon frame as specified in 7.2.2.1
    /// </summary>
    public class Beacon : IDisposable
    {
        // 7.2.2.1.2 Superframe Specification field
        public byte beaconOrder; // 4 bits
        public byte superframeOrder; // 4 bits
        public byte finalCapSlot; // 4 bits
        public byte batteryLifeExtension; // 1 bit
        public byte panCoordinator; // 1 bit
        public byte associationPermit; // 1 bit

        // 7.2.2.1.3 GTS Specification field
        // public byte gtsDescriptorCount; // 3 bits, see gtsDescriptor.Length
        public byte gtsPermit; // 1 bit

        // 7.2.2.1.4 GTS Directions field
        public byte gtsDirectionsMask; // 7 bits

        // 7.2.2.1.5 GTS List field
        public struct GtsDescriptor
        {
            public UInt16 deviceShortAddr;
            public byte gtsStartingSlot; // 4 bits
            public byte gtsLength; // 4 bits
        }

        public GtsDescriptor[] gtsDescriptor;

        // 7.2.2.1.6+7 Pending Address Specification field
        public UInt16[] shortAddrPending;
        public UInt64[] extAddrPending;

        // 7.2.2.1.8 Beacon Payload field
        public Frame payload;

        /// <summary>
        /// Dispose method
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Close()
        {
            Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            Frame.Release(ref payload);
        }

        /// <summary>
        /// Decode beacon from frame
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        public bool ReadFromFrame(ref Frame frame)
        {
            if (frame == null)
                return false;

            // 7.2.2.1.2 Superframe Specification field
            if (frame.LengthDataUsed < 3)
                return false;
            int i = (int)frame.ReadUInt16(0);
            beaconOrder = (Byte)(i & 0xf);
            superframeOrder = (Byte)((i >> 4) & 0xf);
            finalCapSlot = (Byte)((i >> 8) & 0xf);
            batteryLifeExtension = (Byte)((i >> 12) & 1);
            panCoordinator = (Byte)((i >> 14) & 1);
            associationPermit = (Byte)((i >> 15) & 1);

            // 7.2.2.1.3 GTS Specification field
            i = (int)frame.ReadByte(2);
            gtsLength = (Byte)(i & 7);
            gtsPermit = (Byte)((i >> 7) & 1);

            frame.DeleteFromFront(3);

            // 7.2.2.1.4 GTS Directions field
            gtsDirectionsMask = 0;
            if (gtsLength > 0)
            {
                if (frame.LengthDataUsed < 1)
                    return false;
                i = (int)frame.ReadByte(0);
                gtsDirectionsMask = (Byte)(i & 0x7F);
                frame.DeleteFromFront(1);
            }

            // 7.2.2.1.5 GTS List field
            if (frame.LengthDataUsed < 3 * gtsLength)
                return false;
            gtsDescriptor = new GtsDescriptor[gtsLength];
            for (int k = 0; k < gtsLength; k++)
            {
                gtsDescriptor[k].deviceShortAddr = frame.ReadUInt16(0);
                i = (int)frame.ReadByte(2);
                gtsDescriptor[k].gtsStartingSlot = (Byte)(i & 0xF);
                gtsDescriptor[k].gtsLength = (Byte)((i >> 4) & 0xF);
                frame.DeleteFromFront(3);
            }

            // 7.2.2.1.6 Pending Address Specification field
            if (frame.LengthDataUsed < 1)
                return false;
            i = (int)frame.ReadByte(0);
            pendingShort = (i & 7);
            pendingExt = ((i >> 4) & 7);
            frame.DeleteFromFront(1);

            // 7.2.2.1.7 Address List field
            if (frame.LengthDataUsed < pendingShort * 2 + pendingExt * 8)
                return false;
            shortAddrPending = new UInt16[pendingShort];
            extAddrPending = new UInt64[pendingExt];
            for (int k = 0; k < pendingShort; k++)
            {
                shortAddrPending[k] = frame.ReadUInt16(0);
                frame.DeleteFromFront(2);
            }

            for (int k = 0; k < pendingExt; k++)
            {
                extAddrPending[k] = frame.ReadUInt64(0);
                frame.DeleteFromFront(8);
            }

            // 7.2.2.1.8 Beacon Payload field
            if (frame.LengthDataUsed > 0)
            {
                payload = frame;
                frame = null;
            }
            else
            {
                payload = null;
                Frame.Release(ref frame);
            }

            return true;
        }

        private int gtsLength; // actual size of gtsDescriptor when encoding/decoding
        private int pendingShort; // actual size of shortAddrPending when encoding/decoding
        private int pendingExt; // actual size of extAddrPending when encoding/decoding

        /// <summary>
        /// Calculate size of encoded beacon
        /// </summary>
        /// <returns>size in bytes</returns>
        public int Length()
        {
            int len = 0;
            len += 2; // Superframe Specification
            len += 1; // GTS Specification
            gtsLength = 0;
            if (gtsDescriptor != null)
                gtsLength = gtsDescriptor.Length;
            if (gtsLength > 7)
                gtsLength = 7;
            if (gtsLength > 0)
                len += 1; // GTS Directions
            len += 3 * gtsLength; // GTS List

            pendingShort = 0;
            if (shortAddrPending != null)
                pendingShort = shortAddrPending.Length;
            if (pendingShort > 7)
                pendingShort = 7;
            pendingExt = 0;
            if (extAddrPending != null)
                pendingExt = extAddrPending.Length;
            if (pendingExt + pendingShort > 7)
                pendingExt = 7 - pendingShort;

            len += 1; // Pending Address Specification field
            len += 2 * pendingShort;
            len += 8 * pendingExt;

            if (payload != null)
                len += payload.LengthDataUsed;
            return len;
        }

        /// <summary>
        /// Encode beacon into frame
        /// </summary>
        /// <param name="frame">target frame</param>
        /// <returns>True on success</returns>
        public bool WriteToFrame(Frame frame)
        {
            int len = Length();
            if (frame == null || frame.LengthDataAvail < len || frame.LengthDataUsed > 0)
                return false;
            if (payload != null)
                len -= payload.LengthDataUsed;
            frame.AllocBack(len);

            int offset = 0;

            // 7.2.2.1.2 Superframe Specification field
            int i;
            i = (beaconOrder & 0xf) |
                ((superframeOrder & 0xf) << 4) |
                ((finalCapSlot & 0xf) << 8) |
                ((batteryLifeExtension & 1) << 12) |
                ((panCoordinator & 1) << 14) |
                ((associationPermit & 1) << 15);
            frame.Write(offset, (UInt16)i);
            offset += 2;

            // 7.2.2.1.3 GTS Specification field
            if (gtsLength > 7)
                return false; // assert
            i = (gtsLength & 7) |
                 ((gtsPermit & 1) << 7);
            frame.Write(offset, (Byte)i);
            offset += 1;

            // 7.2.2.1.4 GTS Directions field
            if (gtsLength > 0)
            {
                i = (gtsDirectionsMask & 0x7F); // 7 bits
                frame.Write(offset, (Byte)i);
                offset += 1;
            }

            // 7.2.2.1.5 GTS List field
            for (int k = 0; k < gtsLength; k++)
            {
                frame.Write(offset, gtsDescriptor[k].deviceShortAddr);
                offset += 2;
                i = (gtsDescriptor[k].gtsStartingSlot & 0xF) |
                    ((gtsDescriptor[k].gtsLength & 0xF) << 4);
                frame.Write(offset, (Byte)i);
                offset++;
            }

            // 7.2.2.1.6 Pending Address Specification field
            i = (pendingShort & 7) |
                ((pendingExt & 7) << 4);
            frame.Write(offset, (Byte)i);
            offset++;

            // 7.2.2.1.7 Address List field
            for (int k = 0; k < pendingShort; k++)
            {
                frame.Write(offset, shortAddrPending[k]);
                offset += 2;
            }

            for (int k = 0; k < pendingExt; k++)
            {
                frame.Write(offset, extAddrPending[k]);
                offset += 8;
            }

            // 7.2.2.1.8 Beacon Payload field
            if (payload != null)
                frame.WriteToBack(payload);

            return true;
        }
    }
}


