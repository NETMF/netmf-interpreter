////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

using Microsoft.SPOT.Wireless.IEEE_802_15_4.Network;

namespace Microsoft.SPOT.Wireless.IEEE_802_15_4.Network.Fragmentation
{
    interface IFragmentation
    {

        /// <summary>
        /// Start the fragmentation module
        /// </summary>
        /// <param name="localAddress">short address of the local node.</param>
        void Start(UInt16 localAddress);

        /// <summary>
        /// Stop the fragmentation module
        /// </summary>
        void Stop();

        /// <summary>
        /// Send SDU to another device.
        /// </summary>
        /// <param name="targetShortAddr">targetAddr node, use 0xFFFF for broadcasting</param>
        /// <param name="sdu">data to send. Must have sufficient space in head/tail as indicated by GetFrameHeaders.</param>
        /// <param name="sduHandle">user-defined handle that is provided in result handler to identify the sdu.</param>
        /// <param name="handler">handler for result</param>
        void DataRequest(
            UInt16 targetShortAddr,
            ref Frame sdu,
            Byte sduHandle,
            DataConfirmHandler handler);

        /// <summary>
        /// Get required header space for sdu frames to be sent
        /// </summary>
        /// <param name="tail">maximum transmission unit in bytes</param>
        /// <param name="head">required space for header</param>
        /// <param name="tail">required space for tail</param>
        void GetMtuSize(out int mtu, out int head, out int tail);

        /// <summary>
        /// Handler for data indications
        /// </summary>
        DataIndicationHandler DataIndication { get; set; }

        /// <summary>
        /// Method handling data indication (SDU) received from the lower layer
        /// </summary>
        /// <param name="sender">sender of this message</param>
        /// <param name="originatorShortAddr">originating node of the sdu</param>
        /// <param name="targetShortAddr">targetAddr node of the sdu, 0xFFFF indicates broadcast</param>
        /// <param name="sdu">content of sdu</param>
        void HandleLowerLayerDataIndication(
            object sender,
            UInt16 originatorShortAddr,
            UInt16 targetShortAddr,
            Frame sdu);

        /// <summary>
        /// Handler for DataRequest transmitted to lower layer.
        /// </summary>
        //DataRequestHandler LowerLayerDataRequest { get; set; }
    }

    /// <summary>
    /// Delegate corresponding to the sending of SDU operation that must be implemented by lower layer
    /// </summary>
    /// <param name="targetShortAddr">targetAddr node, use 0xFFFF for broadcasting</param>
    /// <param name="sdu">data to send. Must have sufficient space in head/tail as indicated by GetFrameHeaders.</param>
    /// <param name="sduHandle">user-defined handle that is provided in result handler to identify the sdu.</param>
    /// <param name="handler">handler for result</param>
    public delegate void DataRequestHandler(
            UInt16 targetShortAddr,
            ref Frame sdu,
            Byte sduHandle,
            DataConfirmHandler handler);

}


