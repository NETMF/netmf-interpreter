////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using Microsoft.SPOT.Wireless.IEEE_802_15_4;

namespace Microsoft.SPOT.Wireless.IEEE_802_15_4.Phy
{

    public interface IPhy : IPhyDataSap, IPhyMgmtSap, IPhyExtSap, IDisposable
    { }

    #region IPhyDataSap
    public interface IPhyDataSap
    {
        /// <summary>
        /// (6.2.1.1-2) The PD-DATA.request primitive requests the transfer of an MPDU (i.e., PSDU)
        /// from the Mac sublayer to the local Phy entity.
        /// </summary>
        /// <param name="frame">the PSDU to be transmitted by the Phy entity. Frame is not released by Phy layer!</param>
        /// <param name="Status">the results of the operation</param>
        void DataRequest(
            Frame frame,
            out Status status);

        /// <summary>
        /// (6.2.1.3) The PD-DATA.indication primitive indicates the transfer of an MPDU (i.e., PSDU) from
        /// the Phy to the local Mac sublayer entity.
        /// There can be at most one Mac object subscribed to this indication.
        /// </summary>
        DataIndicationHandler DataIndication { get; set; }
    }

    /// <summary>
    /// (6.2.1.3) The PD-DATA.indication primitive indicates the transfer of an MPDU (i.e., PSDU) from
    /// the Phy to the local Mac sublayer entity.
    /// </summary>
    /// <param name="sender">sender of this indication</param>
    /// <param name="frame">Frame containing the payload</param>
    /// <param name="linkQuality">LQI indicator as specified</param>
    public delegate void DataIndicationHandler(
        IPhyDataSap sender,
        Frame frame,
        Byte linkQuality);
    #endregion IPhyDataSap

    #region IPhyMgmtSap
    public interface IPhyMgmtSap
    {
        /// <summary>
        /// (6.2.2.1-2) The PLME-CCA.request primitive requests that the PLME perform a CCA as defined in 6.9.9.
        /// </summary>
        /// <param name="Status">Operation result</param>
        void CCARequest(
            out Status status);

        /// <summary>
        /// (6.2.2.3-4) The PLME-ED.request primitive requests that the PLME perform an ED measurement (see 6.9.7).
        /// </summary>
        /// <param name="status">The result of the request to perform an ED measurement.</param>
        /// <param name="energyLevel">ED level for the current channel. If status is set to Success, this is
        /// the ED level for the current channel. Otherwise, the value of this parameter will be ignored.</param>
        void EDRequest(
            out Status status,
            out Byte energyLevel);

        /// <summary>
        /// (6.2.2.5-6) The PLME-GET.request primitive requests information about a given Phy PIB attribute.
        /// </summary>
        /// <param name="attribute">The identifier of the Phy PIB attribute to get.</param>
        /// <param name="status">The result of the request for Phy PIB attribute information.</param>
        /// <param name="attributeValue">The value of the indicated Phy PIB attribute that was requested. This
        /// parameter has zero length when the status parameter is set to UnsupportedAttribute.</param>
        void GetRequest(
            PibAttribute attribute,
            out Status status,
            out PibValue attributeValue);

        /// <summary>
        /// (6.2.2.7-8) The PLME-SET-TRX-STATE.request primitive requests that the Phy entity change the internal
        /// operating state of the transceiver.
        /// </summary>
        /// <param name="state">The new state in which to configure the transceiver.</param>
        /// <param name="status">The result of the request to change the state of the transceiver.</param>
        void SetTrxStateRequest(
            State state,
            out Status status);

        /// <summary>
        /// (6.2.2.9-10) The PLME-SET.request primitive attempts to set the indicated Phy PIB attribute to the given value.
        /// </summary>
        /// <param name="attribute">The identifier of the PIB attribute to set.</param>
        /// <param name="attributeValue">The value of the indicated PIB attribute to set.</param>
        /// <param name="status">The status of the attempt to set the requested PIB attribute.</param>
        void SetRequest(
            PibAttribute attribute,
            PibValue attributeValue,
            out Status status);

    }

    #endregion IPhyMgmtSap

    #region IPhyExtSap
    /// <summary>
    /// Non-standard extension to support extended radio features
    /// </summary>
    public interface IPhyExtSap
    {
        /// <summary>
        /// Get required header space for data frames to be sent
        /// </summary>
        /// <param name="tail">maximum transmission unit in bytes</param>
        /// <param name="head">required space for header</param>
        /// <param name="tail">required space for tail</param>
        void GetMtuSize(out int mtu, out int head, out int tail);

        /// <summary>
        /// Get extended device address
        /// </summary>
        void GetDeviceAddress(out UInt64 address);

        /// <summary>
        /// Test if radio supports given capability
        /// </summary>
        /// <param name="cap">the requested capability</param>
        /// <param name="supported">true if capability is supported</param>
        void IsCapabilitySupported(Capabilities cap, out bool supported);

        /// <summary>
        /// radio can be powered off
        /// </summary>
        /// <param name="enable">true to enable the radio</param>
        void SetPower(bool enable);

        /// <summary>
        /// radio can calculate/check Mac FCS. If enabled, FCS is not part of frame on data request/indication.
        /// </summary>
        /// <param name="enable">true to enable automatic FCS calculation</param>
        void SetAutoFCS(bool enable);

        /// <summary>
        /// radio can filter for Mac addresses
        /// </summary>
        /// <param name="enable">true to enable the addres filter</param>
        /// <param name="shortAddr">short address of local device</param>
        /// <param name="panId">Pan Id of local device</param>
        /// <param name="panCoordinator">local device is the Pan coordinator</param>
        void SetAddressFilter(
            bool enable,
            UInt16 shortAddr,
            UInt16 panId,
            bool panCoordinator);

        /// <summary>
        /// radio can ack frames, required address filter to be set
        /// </summary>
        /// <param name="enable">true to enable acknowledgment</param>
        void SetAutoAck(bool enable);

        /// <summary>
        /// If the radio has an internal TX buffer, the previously sent frame is cached in the buffer.
        /// This enables efficient retransmissions. However, when the frame is returned to the frame pool and used to sent another frame,
        /// the Phy layer might be unable to detect that the frame content has changed. Use this function to mark the TX buffer as invalid.
        /// </summary>
        void ClearTxBuffer();

        /// <summary>
        /// Poll receive buffer. This function is used to overcome delayed interrupt processing on .NET MF platform
        /// due to missing priorization of interrupt handlers.
        /// </summary>
        void DataPoll();
    }

    // hardware capabilities
    public enum Capabilities : byte
    {
        /// <summary>
        /// Radio can be physically powered off (SetPower). Initial state is off.
        /// </summary>
        PowerOff,
        /// <summary>
        /// Radio can calculate/check Mac FCS. Phy layer automatically adds/removes FCS field. (SetAutoFCS)
        /// </summary>
        AutoFcs,
        /// <summary>
        /// Radio can filter for Mac addresses (SetAddressFilter).
        /// </summary>
        AddressFilter,
        /// <summary>
        /// Radio can ack frames, requires address filter to be set (SetAutoAck)
        /// </summary>
        AutoAck,
        /// <summary>
        /// Radio is using an internal TX buffer. See ClearTxBuffer.
        /// </summary>
        TxCache,
        /// <summary>
        /// Radio supports handling of IEEE802.15.4-2006 frames (as opposed to -2003 only)
        /// </summary>
        Ieee2006
    }

    #endregion IPhyExtSap

    #region constants
    /// <summary>
    /// (6.2.3) Phy enumeration values defined in the Phy specification.
    /// </summary>
    public enum Status : byte
    {
        /// <summary>
        /// The CCA attempt has detected a Busy channel
        /// </summary>
        Busy = 0x00,
        /// <summary>
        /// The transceiver is asked to change its state while receiving
        /// </summary>
        BusyRx = 0x01,
        /// <summary>
        /// The transceiver is asked to change its state while transmitting
        /// </summary>
        BusyTx = 0x02,
        /// <summary>
        /// The transceiver is to be switched off immediately
        /// </summary>
        ForceTxOff = 0x03,
        /// <summary>
        /// The CCA attempt has detected an idle channel
        /// </summary>
        Idle = 0x04,
        /// <summary>
        /// A SET/GET request was issued with a parameter in the primitive that is out of the valid range
        /// </summary>
        InvalidParam = 0x05,
        /// <summary>
        /// The transceiver is in or is to be configured into the receiver enabled state
        /// </summary>
        RxOn = 0x06,
        /// <summary>
        /// A SET/GET, an ED operation, or a transceiver state change was Successful
        /// </summary>
        Success = 0x07,
        /// <summary>
        /// The transceiver is in or is to be configured into the transceiver disabled state
        /// </summary>
        TRxOff = 0x08,
        /// <summary>
        /// The transceiver is in or is to be configured into the transmitter enabled state
        /// </summary>
        TxOn = 0x09,
        /// <summary>
        /// A SET/GET request was issued with the identifier of an attribute that is not supported
        /// </summary>
        UnsupportedAttr = 0x0a,
        /// <summary>
        /// A SET/GET request was issued with the identifier of an attribute that is read-only
        /// </summary>
        ReadOnly = 0x0b
    }

    /// <summary>
    /// (6.2.2.7.1) PLME-SET-TRX-STATE.request parameters
    /// </summary>
    public enum State : byte
    {
        /// <summary>
        /// The transceiver is in or is to be configured into the receiver enabled state
        /// </summary>
        RxOn,
        /// <summary>
        /// The transceiver is in or is to be configured into the transceiver disabled state
        /// </summary>
        TRxOff,
        /// <summary>
        /// The transceiver is to be switched off immediately
        /// </summary>
        ForceTRxOff,
        /// <summary>
        /// The transceiver is in or is to be configured into the transmitter enabled state
        /// </summary>
        TxOn,
    }

    /// <summary>
    /// (6.4.2) Phy PIB attributes
    /// </summary>
    public enum PibAttribute : byte
    {
        /// <summary>
        /// int, 0..26
        /// </summary>
        phyCurrentChannel = 0x00,
        /// <summary>
        /// int[], 5 most significant bits: channel page, 27 lsb: boolean
        /// </summary>
        phyChannelsSupported = 0x01,
        /// <summary>
        /// int, -32..31 dBm
        /// </summary>
        phyTransmitPower = 0x02,
        /// <summary>
        /// int
        /// </summary>
        phyCCAMode = 0x03,
        /// <summary>
        /// int
        /// </summary>
        phyCurrentPage = 0x04,
        /// <summary>
        /// int
        /// </summary>
        phyMaxFrameDuration = 0x05,
        /// <summary>
        /// int
        /// </summary>
        phySHRDuration = 0x06,
        /// <summary>
        /// float
        /// </summary>
        phySymbolsPerOctet = 0x07,
    }

    /// <summary>
    /// attribute value
    /// </summary>
    public struct PibValue
    {
        public enum ValueType : int
        {
            Undefined = 0,
            Int,
            IntArray,
            Float
        }

        private ValueType _type;
        private int _int;
        private int[] _intArray;
        private float _float;

        public ValueType Type
        {
            get { return _type; }
        }

        public int Int
        {
            get
            {
                if (_type != ValueType.Int)
                    throw new ApplicationException("Int value not defined");
                return _int;
            }

            set
            {
                _int = value;
                _type = ValueType.Int;
            }
        }

        public int[] IntArray
        {
            get
            {
                if (_type != ValueType.IntArray)
                    throw new ApplicationException("IntArray value not defined");
                return _intArray;
            }

            set
            {
                _intArray = value;
                _type = ValueType.IntArray;
            }
        }

        public float Float
        {
            get
            {
                if (_type != ValueType.Float)
                    throw new ApplicationException("Float value not defined");
                return _float;
            }

            set
            {
                _float = value;
                _type = ValueType.Float;
            }
        }
    }

    #endregion constants

}


