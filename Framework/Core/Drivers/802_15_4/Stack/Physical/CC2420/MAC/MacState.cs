////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using Microsoft.SPOT.Wireless.IEEE_802_15_4.Phy;
using Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac.Frames;

namespace Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac
{
    interface StateCallbacks
    {
        /// <summary>
        /// Updates address filter. If disabled, all frames are received. If enabled, only frames that match the address of this
        /// device or are sent to broadcast addresses are received.
        /// </summary>
        void SetPhyAddressFilter(
            bool enable,
            UInt16 shortAddr,
            UInt16 panId,
            bool panCoordinator);
    }

    /// <summary>
    /// This class contains the state variables of the Mac layer
    /// </summary>
    class State
    {
        #region static
        public static readonly UInt16 cReservedShortAddr = 0xFFFF;
        public static readonly UInt16 cUnassignedShortAddr = 0xFFFE;
        public static readonly UInt16 cBroadcastShortAddr = 0xFFFF;
        public static readonly UInt16 cBroadcastPanId = 0xFFFF;

        /// <summary>
        /// The number of symbols forming a superframe slot when the superframe order is equal to 0 (see 7.5.1.1).
        /// </summary>
        public static readonly int aBaseSlotDuration = 60;

        /// <summary>
        /// The number of symbols forming a superframe when the superframe order is equal to 0.
        /// (aBaseSlotDuration*aNumSuperframeSlots)
        /// </summary>
        public static readonly int aBaseSuperframeDuration = 960;

        /// <summary>
        /// The number of slots contained in any superframe.
        /// </summary>
        public static readonly int aNumSuperframeSlots = 16;

        /// <summary>
        /// The maximum number of octets added by the MAC sublayer to its payload without security. If security is
        /// required on a frame, its secure processing may inflate the frame length so that it is greater than this value. In
        /// this case, an error is generated through the appropriate .confirm or MLME-COMM-STATUS.indication primitives.
        /// </summary>
        public static readonly int aMaxFrameOverhead = 25;

        /// <summary>
        /// The maximum number of octets that can be transmitted in the Mac Payload field.
        /// (aMaxPhyPacketSize � aMinMPDUOverhead)
        /// </summary>
        public static readonly int aMaxMacPayloadSize = 118;

        /// <summary>
        /// The maximum number of octets that can be transmitted in the MAC Payload field of an unsecured
        /// MAC frame that will be guaranteed not to exceed aMaxPHYPacketSize.
        /// (aMaxPHYPacketSize � aMaxMPDUUnsecuredOverhead)
        /// </summary>
        public static readonly int aMaxMACSafePayloadSize = (127 - 25);

        /// <summary>
        /// The minimum number of octets added by the Mac sublayer to the PSDU.
        /// </summary>
        public static readonly int aMinMPDUOverhead = 9;

        /// <summary>
        /// The number of symbols forming the basic time period used by the CSMA-CA algorithm.
        /// </summary>
        public static readonly int aUnitBackoffPeriod = 20;

        /// <summary>
        /// The maximum PSDU size (in octets) the Phy shall be able to receive.
        /// </summary>
        public static readonly int aMaxPhyPacketSize = 127;
        #endregion static

        #region readonly
        /// <summary>
        /// The 64-bit (IEEE) address assigned to the device.
        /// </summary>
        public readonly UInt64 aExtendedAddress;

        /// <summary>
        /// The frame header space required by Phy layer.
        /// </summary>
        public readonly int phyFrameHead;

        /// <summary>
        /// The frame tail space required by Phy layer.
        /// </summary>
        public readonly int phyFrameTail;

        /// <summary>
        /// Phy layer supports handling of frames according to 802.15.4-2006 spec as opposed to 2003 only.
        /// </summary>
        public readonly bool phySupports2006;
        #endregion static

        #region variables
        /// <summary>
        /// 0..15
        /// </summary>
        public Byte macBeaconOrder;
        /// <summary>
        /// 0..15
        /// </summary>
        public Byte macSuperframeOrder;
        /// <summary>
        /// 0-macMaxBE, default: 3
        /// </summary>
        public Byte macMinBE;
        /// <summary>
        /// 3-8, default: 5
        /// </summary>
        public Byte macMaxBE;
        /// <summary>
        /// 0..5, default: 4
        /// </summary>
        public Byte macMaxCSMABackoffs;
        /// <summary>
        /// 0..7, default: 3
        /// </summary>
        public Byte macMaxFrameRetries;

        /// <summary>
        /// Data sequence number
        /// </summary>
        public Byte macDSN;

        /// <summary>
        /// Beacon sequence number
        /// </summary>
        public Byte macBSN;

        /// <summary>
        /// The 16-bit identifier of the PAN on which the device is operating. If this value is 0xffff, the device is not associated.
        /// </summary>
        public UInt16 macPanId
        {
            get { return _macPanId; }
            set
            {
                if (value != _macPanId)
                {
                    _macPanId = value;
                    UpdateAddressFilter();
                }
            }
        }

        private UInt16 _macPanId;

        /// <summary>
        /// The 16-bit address that the device uses to communicate in the PAN. If the device is the PAN coordinator, this
        /// value shall be chosen before a PAN is started. Otherwise, the address is allocated by a coordinator during
        /// association. A value of 0xfffe indicates that the device has associated but has not been allocated an address.
        /// A value of 0xffff indicates that the device does not have a short address.
        /// </summary>
        public UInt16 macShortAddr
        {
            get { return _macShortAddr; }
            set
            {
                if (value != _macShortAddr)
                {
                    _macShortAddr = value;
                    UpdateAddressFilter();
                }
            }
        }

        private UInt16 _macShortAddr;

        /// <summary>
        /// Indication of whether the Mac sublayer is in a promiscuous (receive all) mode. A value of TRUE indicates that the Mac
        /// sublayer accepts all frames received from the Phy.
        /// </summary>
        public bool macPromiscousMode
        {
            get { return _macPromiscousMode; }
            set
            {
                if (value != _macPromiscousMode)
                {
                    _macPromiscousMode = value;
                    UpdateAddressFilter();
                }
            }
        }

        private bool _macPromiscousMode;

        /// <summary>
        /// True is this device is the PAN coordinator.
        /// </summary>
        public bool panCoordinator
        {
            get { return _panCoordinator; }
            set
            {
                if (value != _panCoordinator)
                {
                    _panCoordinator = value;
                    UpdateAddressFilter();
                }
            }
        }

        private bool _panCoordinator;

        /// <summary>
        /// The current channel page of the radio.
        /// </summary>
        public byte phyChannelPage
        {
            get { return _phyChannelPage; }
            set
            {
                _phyChannelPage = value;
                _phySymbolrate = Symbolrate(_phyChannelPage, _phyChannelNumber);
            }
        }

        private byte _phyChannelPage;

        /// <summary>
        /// The current channel number of the radio.
        /// </summary>
        public byte phyChannelNumber
        {
            get { return _phyChannelNumber; }
            set
            {
                _phyChannelNumber = value;
                _phySymbolrate = Symbolrate(_phyChannelPage, _phyChannelNumber);
            }
        }

        private byte _phyChannelNumber;

        /// <summary>
        /// The symbol rate of the current radio channel in symbols/second.
        /// </summary>
        public int phySymbolrate
        {
            get { return _phySymbolrate; }
        }

        private int _phySymbolrate;

        /// <summary>
        /// The contents of the beacon payload.
        /// </summary>
        public Frame macBeaconPayload;

        /// <summary>
        /// Indicates if the Mac layer is currently performing a scan operation.
        /// </summary>
        public bool scanning;

        /// <summary>
        /// Indicates if the Mac layer automatically answers beacon requests
        /// </summary>
        public bool autoBeacon;
        #endregion variables

        #region private
        private StateCallbacks _callback;
        #endregion

        #region methods
        public State(StateCallbacks callback, IPhy phy)
        {
            _callback = callback;
            phy.GetDeviceAddress(out aExtendedAddress);
            int phyMtu;
            phy.GetMtuSize(out phyMtu, out phyFrameHead, out phyFrameTail);

            phy.IsCapabilitySupported(Capabilities.Ieee2006, out phySupports2006);

            Reset();
        }

        /// <summary>
        /// Resets Mac state to default state
        /// </summary>
        public void Reset()
        {
            macBeaconOrder = 15;
            macSuperframeOrder = 15;
            macMinBE = 3;
            macMaxBE = 5;
            macMaxCSMABackoffs = 4;
            macMaxFrameRetries = 3;

            macDSN = (Byte)Random.GetRandom(Byte.MaxValue + 1);
            macBSN = (Byte)Random.GetRandom(Byte.MaxValue + 1);

            _macPanId = cBroadcastPanId;
            _macShortAddr = cReservedShortAddr;
            _macPromiscousMode = false;
            _panCoordinator = false;
            UpdateAddressFilter();

            Frame.Release(ref macBeaconPayload);

            scanning = false;
            autoBeacon = false;
        }

        private void UpdateAddressFilter()
        {
            _callback.SetPhyAddressFilter(!_macPromiscousMode, _macShortAddr, _macPanId, _panCoordinator);
        }

        /// <summary>
        /// calculates the symbol rate of the given channel in symbols/second.
        /// </summary>
        private int Symbolrate(int phyChannelPage, int phyChannelNumber)
        {
            // see Phy Table 1 & 3
            switch (phyChannelPage)
            {
                case 0:
                    if (phyChannelNumber == 0) // Channel 0 is in 868 MHz band using BPSK
                        return 20000;
                    else if (phyChannelNumber <= 10) // Channels 1 to 10 are in 915 MHz band using BPSK
                        return 40000;
                    else // Channels 11 to 26 are in 2.4 GHz band using O-QPSK
                        return 62500;
                case 1:
                    if (phyChannelNumber == 0) // Channel 0 is in 868 MHz band using ASK
                        return 12500;
                    else // Channels 1 to 10 are in 915 MHz band using ASK
                        return 50000;
                case 2:
                    if (phyChannelNumber == 0) // Channel 0 is in 868 MHz band using O-QPSK
                        return 25000;
                    else // Channels 1 to 10 are in 915 MHz band using O-QPSK
                        return 62500;
            }

            // FIXME: assert here
            return 62500; // default value
        }

        /// <summary>
        /// calculates duration in symbols for transmissing this frame and time to get ack frame
        /// </summary>
        /// <param name="frameSize"></param>
        /// <returns></returns>
        public int AckWaitDuration(int frameSize)
        {
            frameSize += 5; // ack frame
            int symbols = 0;

            // see Phy Table 19&20: preamble and SFD length for two frame
            if (phyChannelPage == 1)
            { // ASK
                if (phyChannelNumber == 0) // Channel 0 is in 868 MHz band using ASK
                    symbols = 2 * (2 + 1) + (frameSize * 2) / 5; // *0.4;
                else // Channels 1 to 10 are in 915 MHz band using ASK
                    symbols = 2 * (6 + 1) + (frameSize * 8) / 5; // *1.6
            }
            else if (phyChannelNumber == 0 && phyChannelNumber <= 10)
            { // all BPSK channels
                symbols = 2 * (32 + 8) + frameSize * 8;
            }
            else
            {
                symbols = 2 * (8 + 2) + frameSize * 2;
            }

            symbols += 12; // inter frame spaceing

            return (symbols * 1000 * 1000) / _phySymbolrate;
        }

        #endregion
    }
}


