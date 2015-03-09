////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Microsoft.SPOT.MFUpdate
{
    using System.Text;
    using System.Collections;

    /// <summary>
    /// Represents an single indexed update packet.  All packets for an update must
    /// be the same length so that the storage facility can track which packets have
    /// been installed.  The one exception is that the last packet may be smaller than 
    /// the packet size if the update size is not a multiple of the packet size.
    /// </summary>
    public class MFUpdatePkt
    {
        /// <summary>
        /// Creates an update packet.
        /// </summary>
        /// <param name="packetIndex">The index of the packet.</param>
        /// <param name="data">The packet data to be stored.</param>
        /// <param name="validationData">The validation data for the packet (e.g. CRC, signature, etc).</param>
        public MFUpdatePkt(int packetIndex, byte[] data, byte[] validationData)
        {
            PacketIndex = packetIndex;
            ValidationData = validationData;
            Data = data;
        }

        /// <summary>
        /// The readonly zero based packet index.
        /// </summary>
        public readonly int PacketIndex;
        /// <summary>
        /// The validation data for the packet.
        /// </summary>
        public readonly byte[] ValidationData;
        /// <summary>
        /// The data bytes for the packet.
        /// </summary>
        public readonly byte[] Data;
    }
}
