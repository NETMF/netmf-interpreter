////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using Microsoft.SPOT.Wireless.IEEE_802_15_4;
using Microsoft.SPOT.Wireless.IEEE_802_15_4.Phy;

namespace Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac
{
    public interface IMac : IMacDataSap, IMacMgmtSap, IMacExtSap, IDisposable
    { }

    #region IMacDataSap
    public partial interface IMacDataSap
    {
        /// <summary>
        /// (7.1.1.1) Request to send a Mac data unit
        /// </summary>
        /// <param name="srcAddrMode">The source addressing mode for this primitive and subsequent MPDU</param>
        /// <param name="dstAddrMode">The destination addressing mode for this primitive and subsequent MPDU</param>
        /// <param name="dstPAnId">The PAN id of the destination</param>
        /// <param name="dstAddr">The destination address</param>
        /// <param name="msdu">msdu</param>
        /// <param name="options"></param>
        /// <param name="securityOptions">the security options to be used</param>
        /// <param name="handler"></param>
        void DataRequest(
            MacAddressingMode srcAddrMode,
            MacAddress dstAddr,
            UInt16 dstPanId,
            ref Frame msdu,
            Byte msduHandle,
            TxOptions options,
            SecurityOptions securityOptions,
            DataConfirmHandler handler);
    }

    /// <summary>
    /// (7.1.1.2) Delegate used for the callback in the DataRequest call
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="msdu">The msdu being confirmed</param>
    /// <param name="status">The status of the last MSDU transmission.</param>
    /// <param name="timeStamp">the time (in symbols) at which the data was transmitted</param>
    public delegate void DataConfirmHandler(
        IMacDataSap sender,
        Byte msduHandle,
        MacEnum status);
    public partial interface IMacDataSap
    {
        /// <summary>
        /// (7.1.1.3) event triggered when a new msdu has been received
        /// </summary>
        DataIndicationHandler DataIndication { set; }
    }

    /// <summary>
    /// (7.1.1.3) Delegate used for the DataIndication event
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="srcAddrMode">The source addressing mode for this primitive corresponding to the received MPDU</param>
    /// <param name="srcPanId"> The pan id of the source</param>
    /// <param name="srcAddr">The source address</param>
    /// <param name="dstAddrMode">The destination addressing mode for this primitive corresponding to the received MPDU</param>
    /// <param name="dstPanId"> the pan id of the destination</param>
    /// <param name="dstAddr">The destination address</param>
    /// <param name="msdu">msdu</param>
    /// <param name="linkQuality">LQI value measured during reception of the MPDU. Lower values represent lower LQI</param>
    /// <param name="DSN">The DSN of the received data frame</param>
    /// <param name="timeStamp">the time (in symbols) at which the data were received</param>
    /// <param name="securityOptions"></param>
    public delegate void DataIndicationHandler(
        IMacDataSap sender,
        MacAddress srcAddr,
        UInt16 srcPanId,
        MacAddress dstAddr,
        UInt16 dstPanId,
        Frame msdu,
        Byte linkQuality,
        Byte DSN,
        UInt32 timeStamp,
        SecurityOptions securityOptions);

    public partial interface IMacDataSap
    {
        /// <summary>
        /// (7.1.1.4) The MCPS-PURGE.request primitive allows the next higher layer to purge an MSDU from the transaction queue.
        /// </summary>
        /// <param name="msdu">msdu</param>
        /// <param name="handler"></param>
        void PurgeRequest(
            Byte msduHandle,
            PurgeConfirmHandler handler);
    }

    /// <summary>
    /// (7.1.1.5) Delegate used for the callback in  the PurgeRequest call.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="msdu">the msdu requested to be purge</param>
    /// <param name="status">The status of the request to be purged an MSDU from the transaction queue</param>
    public delegate void PurgeConfirmHandler(
        IMacDataSap sender,
        Byte msduHandle,
        MacEnum status);

    #endregion IMacDataSap

    #region IMacMgmtSap
    public partial interface IMacMgmtSap
    {
        /// <summary>
        /// (7.1.3.1) The MLME-ASSOCIATE.request primitive allows a device to request an association with a coordinator.
        /// </summary>
        /// <param name="logicalChannel">The logical channel on which to attempt association.</param>
        /// <param name="channelPage">The channel page on which to attempt association</param>
        /// <param name="coordAddrMode">The coordinator addressing mode for this primitive and subsequent MPDU.</param>
        /// <param name="coordPanId"> The PAN id of the coordinator</param>
        /// <param name="coordAddr">The coordinator address</param>
        /// <param name="capability">Specifies the operational capabilities of the associating device.</param>
        /// <param name="securityOptions">the security options to be used</param>
        /// <param name="handler"></param>
        void AssociateRequest(
            Byte logicalChannel,
            Byte channelPage,
            MacAddress coordAddr,
            UInt16 coordPanId,
            CapabilityInformation capability,
            SecurityOptions securityOptions,
            AssociateConfirmHandler handler);

        /// <summary>
        /// (7.1.3.2) The MLME-ASSOCIATE.indication primitive is used to indicate the reception of an association request command.
        /// </summary>
        event AssociateIndicationHandler AssociateIndication;
    }

    /// <summary>
    /// (7.1.3.2)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="deviceAddress">The IEEE address of the device requesting association.</param>
    /// <param name="capability">The operational capabilities of the device requesting association</param>
    /// <param name="securityOptions">The security level purportedly used by the received Mac command frame</param>
    public delegate void AssociateIndicationHandler(
        IMacMgmtSap sender,
        UInt64 deviceAddress,
        CapabilityInformation capability,
        SecurityOptions securityOptions);

    public partial interface IMacMgmtSap
    {
        /// <summary>
        /// (7.1.3.3) The MLME-ASSOCIATE.response primitive is used to initiate a response to an MLMEASSOCIATE.indication primitive.
        /// </summary>
        ///
        void AssociateResponse(
            UInt64 deviceAddress,
            UInt16 assocShortAddress,
            MacEnum status,
            SecurityOptions securityOptions);
    }

    /// <summary>
    /// (7.1.3.4) Delegate used for the callback in  the AssociateRequest call.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="assocShortAddr">The short device address allocated by the coordinator on successful
    /// association. This parameter will be equal to 0xffff if the association attempt was unSuccessful.</param>
    /// <param name="status">the status of the corresponding request</param>
    /// <param name="securityOptions">If the primitive was generated following failed outgoing processing
    /// of an association request command: The security level to be used.
    /// If the primitive was generated following receipt of an association response command:
    /// The security level purportedly used by the received frame.</param>
    public delegate void AssociateConfirmHandler(
        IMacMgmtSap sender,
        UInt16 assocShortAddr,
        MacEnum status,
        SecurityOptions securityOptions);

    public partial interface IMacMgmtSap
    {
        /// <summary>
        /// (7.1.4.1) The MLME-DISASSOCIATE.request primitive is used by an associated device to notify the coordinator of
        /// its intent to leave the PAN. It is also used by the coordinator to instruct an associated device
        /// to leave the PAN.
        /// </summary>
        /// <param name="deviceAddrMode">The addressing mode of the device to which
        /// to send the disassociation notification command</param>
        /// <param name="devicePanId">the panId of the device to which to send the disassociation notification demand</param>
        /// <param name="deviceAddr">The address of the device to which to send the
        /// disassociation notification command</param>
        /// <param name="reason">The reason for the disassociation</param>
        /// <param name="txIndirect">TRUE if the disassociation notification command is to be sent indirectly.</param>
        /// <param name="securityOptions">The security options to be used</param>
        /// <param name="handler"></param>
        void DisassociateRequest(
            MacAddress deviceAddr,
            UInt16 devicePanId,
            DisassociationReason reason,
            bool txIndirect,
            SecurityOptions securityOptions,
            DisassociateConfirmHandler handler);

        /// <summary>
        /// (7.1.4.2) The MLME-DISASSOCIATE.indication primitive is used to indicate the reception of a disassociation notification command.
        /// </summary>
        event DisassociateIndicationHandler DisassociateIndication;
    }

    /// <summary>
    /// (7.1.4.2) Delegate used for the DisassociateIndication event.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="deviceAddr">The IEEE address of the device requesting disassociation.</param>
    /// <param name="reason">The reason for the disassociation</param>
    /// <param name="securityOptions">The security options purportedly used by the received Mac command frame</param>
    public delegate void DisassociateIndicationHandler(
        IMacMgmtSap sender,
        UInt64 deviceAddr,
        DisassociationReason reason,
        SecurityOptions securityOptions);

    /// <summary>
    /// (7.1.4.3) Delegate used for the callback in  the DisassociateRequest call.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="status">the status of the corresponding request</param>
    /// <param name="devicePanId">The pan Id of the device that has either requested disassocaiation or
    /// been instructed to disassociate by its coordinator</param>
    /// <param name="deviceAddrMode">The addressing mode of the device that has either requested
    /// disassociation or been instructed to disassociate by its coordinator</param>
    /// <param name="deviceAddr">The address of the device that has either requested disassociation or
    /// been instructed to disassociate by its coordinator.</param>
    public delegate void DisassociateConfirmHandler(
        IMacMgmtSap sender,
        MacEnum status,
        MacAddress deviceAddr,
        UInt16 devicePanId);

    public partial interface IMacMgmtSap
    {
        /// <summary>
        /// (7.1.5.1) The MLME-BEACON-NOTIFY.indication primitive is used to send parameters contained within a beacon
        /// frame received by the Mac sublayer to the next higher layer. The primitive also sends a measure of the LQI
        /// and the time the beacon frame was received
        /// </summary>
        BeaconNotifyIndicationHandler BeaconNotifyIndication { set; }
    }

    /// <summary>
    /// (7.1.5.1) Delegate used for the BeaconIndication event.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="BSN">The beacon sequence number</param>
    /// <param name="panDescriptor">The PanDescriptor for the received beacon.</param>
    /// <param name="pendingShortAddrs">The list of short addresses of the devices for which the beacon source has data.</param>
    /// <param name="pendingExtendedAddrs">The list of addresses of the devices for which the beacon source has data.</param>
    /// <param name="beaconPayload">The beacon payload</param>
    public delegate void BeaconNotifyIndicationHandler(
        IMacMgmtSap sender,
        UInt16 BSN,
        PanDescriptor panDescriptor,
        UInt16[] pendingShortAddrs,
        UInt64[] pendingExtendedAddrs,
        Frame beaconPayload);

    public partial interface IMacMgmtSap
    {
        /// <summary>
        /// (7.1.6.1) The MLME-GET.request primitive requests information about a given PIB attribute
        /// </summary>
        /// <param name="attribute">The identifier of the PIB attribute to read.</param>
        /// <param name="attributeIndex">The index within the table of the specified PIB
        /// attribute to read. This parameter is valid only for Mac PIB attributes that are tables; it is
        /// ignored when accessing Phy PIB attributes</param>
        /// <param name="handler"></param>
        void GetRequest(
            PibAttribute attribute,
            int attributeIndex,
            GetConfirmHandler handler);
    }

    /// <summary>
    /// (7.1.6.2) Delegate used for the callback in  the GetRequest call.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="status">the status of the corresponding request</param>
    /// <param name="attribute">The identifier of the PIB attribute that was read.</param>
    /// <param name="attributeIndex">The index within the table or array of the specified PIB attribute to read.
    /// This parameter is valid only for Mac PIB attributes that are tables or arrays; it is
    /// ignored when accessing Phy PIB attributes.</param>
    /// <param name="value">The value of the indicated PIB attribute that was read.</param>
    public delegate void GetConfirmHandler(
        IMacMgmtSap sender,
        MacEnum status,
        PibAttribute attribute,
        int attributeIndex,
        PibValue value);

    public partial interface IMacMgmtSap
    {
        /// <summary>
        /// (7.1.7.1)
        /// </summary>
        /// <param name="gtsCharacteristics">The characteristics of the GTS request, including
        /// whether the request is for the allocation of a new GTS or the deallocation of an existing GTS.</param>
        /// <param name="securityOptions">Teh security options to be used</param>
        /// <param name="handler"></param>
        void GtsRequest(
            GtsCharacteristics gtsCharacteristics,
            SecurityOptions securityOptions,
            GtsConfirmHandler handler);
    }

    /// <summary>
    /// (7.1.7.2) Delegate used for the callback in  the GtsRequest call.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="gtsCharacteristics">The characteristics of the GTS.</param>
    /// <param name="status">the status of the corresponding request</param>
    public delegate void GtsConfirmHandler(
        IMacMgmtSap sender,
        GtsCharacteristics gtsCharacteristics,
        MacEnum status);

    public partial interface IMacMgmtSap
    {
        /// <summary>
        /// (7.1.7.3)
        /// The MLME-GTS.indication primitive indicates that a GTS has been allocated or that a previously
        /// allocated GTS has been deallocated.
        /// </summary>
        event GtsIndicationHandler GtsIndication;
    }

    /// <summary>
    /// (7.1.7.3) Delegate used for the GTSIndication event.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="deviceAddr">The 16-bit short address of the device that has been allocated or deallocated a GTS.</param>
    /// <param name="gtsCharacteristics">The characteristics of the GTS.</param>
    /// <param name="securityOptions">If the primitive was generated when a GTS deallocation is initiated by the PAN
    /// coordinator itself, the security level in the security options to be used is set to 0x00.
    /// If the primitive was generated whenever a GTS is allocated or deallocated following
    /// the reception of a GTS request command: The security options purportedly used by the received Mac command frame</param>
    public delegate void GtsIndicationHandler(
        IMacMgmtSap sender,
        UInt16 deviceAddr,
        GtsCharacteristics gtsCharacteristics,
        SecurityOptions securityOptions);

    public partial interface IMacMgmtSap
    {
        /// <summary>
        /// (7.1.8.1)
        /// The MLME-ORPHAN.indication primitive allows the MLME of a coordinator to notify the next higher
        /// layer of the presence of an orphaned device.
        /// </summary>
        event OrphanIndicationHandler OrphanIndication;
    }

    /// <summary>
    /// (7.1.8.1)
    /// Delegate used for the OrphanIndication event.
    /// </summary>
    /// <param name="orphanAddr">the IEEE address of the orphaned device.</param>
    /// <param name="securityOptions">The security options purportedly used by the received Mac command frame</param>
    /// <param name="handler"></param>
    public delegate void OrphanIndicationHandler(
        UInt64 orphanAddr,
        SecurityOptions securityOptions);
    //OrphanResponseHandler handler

    public partial interface IMacMgmtSap
    {
        /// <summary>
        /// (7.1.8.2)
        /// </summary>
        void OrphanResponse(
            UInt64 orphanAddress,
            UInt16 shortAddr,
            bool associatedMember,
            SecurityOptions securityOptions);

        /// <summary>
        /// (7.1.9.1)
        /// The MLME-RESET.request primitive allows the next higher layer to request that the MLME performs a reset operation.
        /// </summary>
        /// <param name="setDefaultPIB">If TRUE, the Mac sublayer is reset, and all Mac
        /// PIB attributes are set to their default values. If FALSE, the Mac sublayer is reset, but all Mac PIB
        /// attributes retain their values prior to the generation of the MLME-RESET.request primitive</param>
        /// <param name="handler"></param>
        void ResetRequest(
            bool setDefaultPIB,
            ResetConfirmHandler handler);
    }

    /// <summary>
    /// (7.1.9.2)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="status">the status of the corresponding request</param>
    public delegate void ResetConfirmHandler(
        IMacMgmtSap sender,
        MacEnum status);

    public partial interface IMacMgmtSap
    {
        /// <summary>
        /// (7.1.10.1)
        /// The MLME-RX-ENABLE.request primitive allows the next higher layer to request that the receiver is either
        /// enabled for a finite period of time or disabled.
        /// </summary>
        /// <param name="deferPermit"> TRUE if the requested operation can be deferred until the next superframe if the requested time has already
        /// passed. FALSE if the requested operation is only to be  attempted in the current superframe. This parameter is
        /// ignored for nonbeacon-enabled PANs. If the issuing device is the PAN coordinator, the term
        /// superframe refers to its own superframe. Otherwise, the term refers to the superframe of the coordinator
        /// through which the issuing device is associated.</param>
        /// <param name="rxOnTime">The number of symbols measured from the start of the
        /// superframe before the receiver is to be enabled or disabled. This is a 24-bit value, and the precision of
        /// this value shall be a minimum of 20 bits, with the lowest 4 bits being the least significant. This
        /// parameter is ignored for nonbeacon-enabled PANs. If the issuing device is the PAN coordinator, the term
        /// superframe refers to its own superframe. Otherwise, the term refers to the superframe of the coordinator
        /// through which the issuing device is associated.</param>
        /// <param name="rxOnDuration">The number of symbols for which the receiver is to be enabled.
        /// If this parameter is equal to 0x000000, the receiver is to be disabled.</param>
        /// <param name="handler"></param>
        void RxEnableRequest(
            bool deferPermit,
            UInt16 rxOnTime,
            UInt16 rxOnDuration,
            RxEnableConfirmHandler handler);
    }

    /// <summary>
    /// (7.1.10.2) Delegate used for the callback in  the RxEnableRequest call.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="status">the status of the corresponding request</param>
    public delegate void RxEnableConfirmHandler(
        IMacMgmtSap sender,
        MacEnum status);

    public partial interface IMacMgmtSap
    {
        /// <summary>
        /// (7.1.11.1)
        /// The MLME-SCAN.request primitive is used to initiate a channel scan over a given list of channels. A device
        /// can use a channel scan to measure the energy on the channel, search for the coordinator with which it
        /// associated, or search for all coordinators transmitting beacon frames within the POS of the scanning device.
        /// </summary>
        /// <param name="scanType">Indicates the type of scan performed</param>
        /// <param name="scanChannels">The 27 bits (b0, b1,... b26) indicate which channels are to be scanned (1 = scan, 0 = do not
        /// scan) for each of the 27 channels supported by the ChannelPage parameter.</param>
        /// <param name="scanDuration">A value used to calculate the length of time to
        /// spend scanning each channel for ED, active, and passive scans. This parameter is ignored for
        /// orphan scans. The time spent scanning each channel is [aBaseSuperframeDuration * (2n + 1)]
        /// symbols, where n is the value of the ScanDuration parameter.</param>
        /// <param name="channelPage">The channel page on which to perform the scan</param>
        /// <param name="securityOptions">The security option s to be used</param>
        /// <param name="handler"></param>
        void ScanRequest(
            ScanType scanType,
            UInt32 scanChannels,
            byte scanDuration,
            byte channelPage,
            SecurityOptions securityOptions,
            ScanChannelConfirmHandler handler);
    }

    /// <summary>
    /// (7.1.11.2) Delegate used for the callback in  the ScanChannelRequest call.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="status">the status of the corresponding request</param>
    /// <param name="scanType">Indicates the type of scan performed</param>
    /// <param name="channelPage">The channel page on which the scan was performed</param>
    /// <param name="unscannedChannel">Indicates which channels given in the
    /// request were not scanned (1 = not scanned, 0 = scanned or not requested). This parameter is ALSO valid
    /// for ED scans.</param>
    /// <param name="energyDetectList">The array of energy measurements, one for each channel REQUESTED during an
    /// ED scan. This parameter is null for active, passive, and orphan scans.</param>
    /// <param name="panDescriptorList">The array of PAN descriptors, one for each beacon found during an active or
    /// passive scan if macAutoRequest is set to TRUE. This parameter is null for ED and orphan scans or when
    /// macAutoRequest is set to FALSE during an active or passive scan.</param>
    public delegate void ScanChannelConfirmHandler(
        IMacMgmtSap sender,
        MacEnum status,
        ScanType scanType,
        Byte channelPage,
        UInt32 unscannedChannel,
        byte[] energyDetectList,
        PanDescriptor[] panDescriptorList);

    public partial interface IMacMgmtSap
    {
        /// <summary>
        /// 7.1.12.1
        /// The MLME-COMM-STATUS.indication primitive allows the MLME to indicate a communications status.
        /// </summary>
        event CommStatusIndicationHandler CommStatusIndication;
    }

    /// <summary>
    /// 7.1.12.1 Delegate used for the CommStatusIndication event.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="panId">The 16-bit PAN identifier of the device from which the frame was received or to
    /// which the frame was being sent</param>
    /// <param name="srcAddrMode">The source addressing mode</param>
    /// <param name="srcAddr">The source address</param>
    /// <param name="dstAddrMode">The destination addressing mode</param>
    /// <param name="dstAddr">The destination address</param>
    /// <param name="status">The communications status.</param>
    /// <param name="securityOptions">If the primitive was generated following a transmission instigated through a
    /// response primitive: The security level to be used.
    /// If the primitive was generated on receipt of a frame that generates an error in its security processing:
    /// The security level purportedly used by the received frame</param>
    public delegate void CommStatusIndicationHandler(
        IMacMgmtSap sender,
        UInt16 panId,
        MacAddress srcAddr,
        MacAddress dstAddr,
        MacEnum status,
        SecurityOptions securityOptions);

    public partial interface IMacMgmtSap
    {
        /// <summary>
        /// (7.1.13.1) The MLME-SET.request primitive attempts to write the given value to the indicated PIB attribute
        /// </summary>
        /// <param name="attribute">The identifier of the PIB attribute to write</param>
        /// <param name="index">The index within the table of the specified
        /// PIB attribute to write. This parameter is valid only for Mac PIB attributes that are tables; it
        /// is ignored when accessing Phy PIB attributes</param>
        /// <param name="value">The value to write to the indicated PIB attribute.</param>
        /// <param name="handler"></param>
        void SetRequest(
            PibAttribute attribute,
            int index,
            PibValue value,
            SetConfirmHandler handler);
    }

    /// <summary>
    /// (7.1.13.2) Delegate used for the callback in  the SetRequest call.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="status">the status of the corresponding request</param>
    /// <param name="attribute">The identifier of the PIB attribute that was written</param>
    /// <param name="index">The index within the table of the specified PIB attribute to write. This
    /// parameter is valid only for Mac PIB attributes that are tables; it is ignored
    /// when accessing Phy PIB attributes.</param>
    public delegate void SetConfirmHandler(
        IMacMgmtSap sender,
        MacEnum status,
        PibAttribute attribute,
        int index);

    public partial interface IMacMgmtSap
    {
        /// <summary>
        /// (7.1.14.1)
        ///  The MLME-START.request primitive allows the PAN coordinator to initiate a new PAN or to begin using a
        ///  new superframe configuration. This primitive may also be used by a device already associated with an
        /// existing PAN to begin using a new superframe configuration.
        /// </summary>
        /// <param name="panId">The PAN identifier to be used by the device.</param>
        /// <param name="logicalChannel">The logical channel on which to start using the new superframe configuration.</param>
        /// <param name="channelPage">The channel page on which to begin using the new superframe configuration.</param>
        /// <param name="startTime">The time at which to begin transmitting beacons. If this
        /// parameter is equal to 0x000000, beacon transmissions will begin immediately. Otherwise, the specified
        /// time is relative to the received beacon of the coordinator with which the device synchronizes.
        /// This parameter is ignored if either the BeaconOrder parameter has a value
        /// of 15 or the PANCoordinator parameter is TRUE.
        /// The time is specified in symbols and is rounded to a backoff slot boundary.
        /// This is a 24-bit value, and the precision of this value shall be a
        /// minimum of 20 bits, with the lowest 4 bits being the least significant.</param>
        /// <param name="beaconOrder">How often the beacon is to be transmitted. A value of 15 indicates
        /// that the coordinator will not transmit periodic beacons.</param>
        /// <param name="superframeOrder">The length of the active portion of the superframe, including the beacon
        /// frame. If the BeaconOrder parameter (BO) has a value of 15, this parameter is ignored.</param>
        /// <param name="panCoordinator">If this value is TRUE, the device will become the PAN coordinator of a new
        /// PAN. If this value is FALSE, the device will begin using a new superframe configuration on the PAN
        /// with which it is associated.</param>
        /// <param name="batteryLifeExtension">If this value is TRUE, the receiver of the beaconing device is disabled
        /// macBattLifeExtPeriods full backoff periods after the interframe spacing
        /// (IFS) period following the beacon frame. If this value is FALSE, the receiver of the beaconing device
        /// remains enabled for the entire CAP.
        /// This parameter is ignored if the BeaconOrder parameter has a value of 15.</param>
        /// <param name="coordRealignment">TRUE if a coordinator realignment command is to be transmitted prior to
        /// changing the superframe configuration or FALSE otherwise.</param>
        /// <param name="coordRealignSecutiryOptions">The security options to be used for coordinator realignment command frames</param>
        /// <param name="beaconSecurityOptions">The security options to be used for beacon frames</param>
        /// <param name="handler"></param>
        void StartRequest(
            UInt16 panId,
            Byte logicalChannel,
            Byte channelPage,
            UInt16 startTime,
            byte beaconOrder,
            byte superframeOrder,
            bool panCoordinator,
            bool batteryLifeExtension,
            bool coordRealignment,
            SecurityOptions coordRealignSecutiryOptions,
            SecurityOptions beaconSecurityOptions,
            StartConfirmHandler handler);
    }

    /// <summary>
    /// Delegate used for the callback in  the StartRequest call.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="status">the status of the corresponding request</param>
    public delegate void StartConfirmHandler(
        IMacMgmtSap sender,
        MacEnum status);

    public partial interface IMacMgmtSap
    {
        /// <summary>
        /// (7.1.15.1)
        /// The MLME-SYNC.request primitive requests to synchronize with the coordinator by acquiring and, if
        /// specified, tracking its beacons.
        /// </summary>
        /// <param name="logicalChannel">The logical channel on which to attempt coordinator synchronization. </param>
        /// <param name="logicalPage">The channel page on which to attempt coordinator synchronization</param>
        /// <param name="trackBeacon">TRUE if the MLME is to synchronize with the
        /// next beacon and attempt to track all future beacons. FALSE if the MLME is to synchronize with only the next beacon</param>
        void SyncRequest(
            ushort logicalChannel,
            ushort logicalPage,
            bool trackBeacon);

        /// <summary>
        /// (7.1.15.2)
        /// The MLME-SYNC-LOSS.indication primitive indicates the loss of synchronization with a coordinator
        /// </summary>
        event SyncLossIndicationHandler SyncLossIndication;
    }

    /// <summary>
    /// (7.1.15.2) Delegate used for the SyncLossIndication event.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="lossReason">The reason that synchronization was lost (PanIdConflict, Realignment, or BEACON_LOST).</param>
    /// <param name="panId">The PAN identifier with which the device lost synchronization or to which it was realigned.</param>
    /// <param name="logicalChannel">The logical channel on which the device lost synchronization or to which it was realigned.</param>
    /// <param name="channelPage">The channel page on which the device lost synchronization or to which it was realigned.</param>
    /// <param name="securityOptions">If the primitive was either generated by
    /// the device itself following loss of synchronization or generated by the PAN
    /// coordinator upon detection of a PAN ID conflict, the security level od the security options is set to 0x00.
    /// If the primitive was generated following the reception of either a coordinator
    /// realignment command or a PAN ID conflict notification command:
    /// The security options purportedly used by the received Mac frame</param>
    public delegate void SyncLossIndicationHandler(
        IMacMgmtSap sender,
        MacEnum lossReason,
        ushort panId,
        ushort logicalChannel,
        ushort channelPage,
        SecurityOptions securityOptions);

    public partial interface IMacMgmtSap
    {
        /// <summary>
        /// (7.1.16.1) The MLME-POLL.request primitive prompts the device to request data from the coordinator.
        /// </summary>
        /// <param name="coordAddrMode">The addressing mode of the coordinator to which the poll is intended.</param>
        /// <param name="coordPanId"> the pan id of the coordinator</param>
        /// <param name="coordAddr">The address of the coordinator to which the poll is intended</param>
        /// <param name="securityOptions">The security options to be used</param>
        /// <param name="handler"></param>
        void PollRequest(
            MacAddress coordAddr,
            UInt16 coordPanId,
            SecurityOptions securityOptions,
            PollConfirmHandler handler);
    }

    /// <summary>
    /// (7.1.16.2) Delegate used for the callback in the PollRequest call.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="status">the status of the corresponding request</param>
    public delegate void PollConfirmHandler(IMacMgmtSap sender, MacEnum status);
    #endregion IMacMgmtSap

    #region IMacExtSap
    public interface IMacExtSap
    {
        /// <summary>
        /// Get required header space for data frames to be sent
        /// </summary>
        /// <param name="head">required space for header</param>
        /// <param name="tail">required space for tail</param>
        void GetMtuSize(out int mtu, out int head, out int tail);

        /// <summary>
        /// Get extended device address
        /// </summary>
        /// <returns></returns>
        void GetDeviceAddress(out UInt64 address);
    }

    #endregion IMacExtSap

    #region Constants and types
    /// <summary>
    /// (7.1.17) Mac enumerations description
    /// </summary>
    public enum MacEnum : byte
    {
        /// <summary>
        /// The requested operation was completed Successfully. For a transmission request, this value indicates a Successful transmission.
        /// </summary>
        Success = 0x00,
        /// <summary>
        /// The beacon was lost following a synchronization request.
        /// </summary>
        BeaconLoss = 0xe0,
        /// <summary>
        /// A transmission could not take place due to activity on the channel, i.e., the CSMA-CA mechanism has failed.
        /// </summary>
        ChannelAccessFailure = 0xe1,
        /// <summary>
        /// The frame counter purportedly applied by the originator of the received frame is invalid.
        /// </summary>
        CounterError = 0xdb,
        /// <summary>
        /// The GTS request has been denied by the PAN coordinator.
        /// </summary>
        Denied = 0xe2,
        /// <summary>
        /// The attempt to disable the transceiver has failed.
        /// </summary>
        DisableTrxFailure = 0xe3,
        /// <summary>
        /// Either a frame resulting from processing has a length that is greater than aMaxPhyPacketSize or a requested transaction is too large to fit in the CAP or GTS.
        /// </summary>
        FrameTooLong = 0xe5,
        /// <summary>
        /// The key purportedly applied by the originator of the received frame is not allowed to be used with that frame type according to the key usage policy of the recipient.
        /// </summary>
        ImproperKeyType = 0xdc,
        /// <summary>
        /// The security level purportedly applied by the originator of the received frame does not meet the minimum security level required/expected by the recipient for that frame type.
        /// </summary>
        ImproperSecurityLevel = 0xdd,
        /// <summary>
        /// A request to send data was unSuccessful because neither the source address parameters nor the destination address parameters were present.
        /// </summary>
        InvalidGtsAddress = 0xf5,
        /// <summary>
        /// The requested GTS transmission failed because the specified GTS either did not have a transmit GTS direction or was not defined.
        /// </summary>
        InvalidGts = 0xe6,
        /// <summary>
        /// A request to purge an MSDU from the transaction queue was made using an MSDU handle that was not found in the transaction table.
        /// </summary>
        InvalidHandle = 0xe7,
        /// <summary>
        /// An attempt to write to a Mac PIB attribute that is in a table failed because the specified table index was out of range.
        /// </summary>
        InvalidIndex = 0xf9,
        /// <summary>
        /// A parameter in the primitive is either not supported or is out of the valid range.
        /// </summary>
        InvalidParameter = 0xe8,
        /// <summary>
        /// A scan operation terminated prematurely because the number of PAN descriptors stored reached an implementationspecified maximum.
        /// </summary>
        LimitReached = 0xfa,
        /// <summary>
        /// No acknowledgment was received after macMaxFrameRetries.
        /// </summary>
        NoAck = 0xe9,
        /// <summary>
        /// A scan operation failed to find any network beacons.
        /// </summary>
        NoBeacon = 0xea,
        /// <summary>
        /// No response data were available following a request.
        /// </summary>
        NoData = 0xeb,
        /// <summary>
        ///  The operation failed because a 16-bit short address was not allocated.
        /// </summary>
        NoShortAddress = 0xec,
        /// <summary>
        /// A receiver enable request was unSuccessful because it specified a number of symbols that was longer than the beacon interval.
        /// </summary>
        OnTimeTooLong = 0xf6,
        /// <summary>
        /// A receiver enable request was unSuccessful because it could not be completed within the CAP. The enumeration description is not used in this standard, and it is included only to meet the backwards compatibility requirements for IEEE Std 802.15.4-2003.
        /// </summary>
        OutOfCap = 0xed,
        /// <summary>
        /// A PAN identifier conflict has been detected and communicated to the PAN coordinator.
        /// </summary>
        PanIdConflict = 0xee,
        /// <summary>
        /// A receiver enable request was unSuccessful because it could not be completed within the current superframe and was not permitted to be deferred until the next superframe.
        /// </summary>
        PastTime = 0xf7,
        /// <summary>
        /// A SET/GET request was issued with the identifier of an attribute that is read only.
        /// </summary>
        ReadOnly = 0xfb,
        /// <summary>
        /// A coordinator realignment command has been received.
        /// </summary>
        Realignment = 0xef,
        /// <summary>
        /// A request to perform a scan operation failed because the MLME was in the process of performing a previously initiated scan operation.
        /// </summary>
        ScanInProgress = 0xfc,
        /// <summary>
        /// Cryptographic processing of the received secured frame failed.
        /// </summary>
        SecurityError = 0xe4,
        /// <summary>
        /// The device was instructed to start sending beacons based on the timing of the beacon transmissions of its coordinator, but the instructed start time overlapped the transmission time of the beacon of its coordinator.
        /// </summary>
        SuperframeOverlap = 0xfd,
        /// <summary>
        /// The device was instructed to start sending beacons based on the timing of the beacon transmissions of its coordinator, but the device is not currently tracking the beacon of its coordinator.
        /// </summary>
        TrackingOff = 0xf8,
        /// <summary>
        /// The transaction has expired and its information was discarded.
        /// </summary>
        TransactionExpired = 0xf0,
        /// <summary>
        /// There is no capacity to store the transaction.
        /// </summary>
        TransactionOverflow = 0xf1,
        /// <summary>
        /// The transceiver was in the transmitter enabled state when the receiver was requested to be enabled.
        /// </summary>
        TxActive = 0xf2,
        /// <summary>
        /// The key purportedly used by the originator of the received frame is not available or, if available, the originating device is not known or is blacklisted with that particular key.
        /// </summary>
        UnavailableKey = 0xf3,
        /// <summary>
        /// A SET/GET request was issued with the identifier of a PIB attribute that is not supported.
        /// </summary>
        UnsupportedAttribute = 0xf4,
        /// <summary>
        /// The received frame was purportedly secured using security based on IEEE Std 802.15.4-2003, and such security is not supported by this standard.
        /// </summary>
        UnsupportedLegacy = 0xde,
        /// <summary>
        /// The security purportedly applied by the originator of the received frame is not supported.
        /// </summary>
        UnsupportedSecurity = 0xdf,

        /// <summary>
        /// The system is congested. Try again later. (custom status)
        /// </summary>
        Congested = 0xfe,
        /// <summary>
        /// The requested functionality is not implemented (custom status)
        /// </summary>
        NotImplemented = 0xff
    }

    /// <summary>
    /// Table 55�Elements of PanDescriptor
    /// </summary>
    public struct PanDescriptor
    {
        public MacAddress coordAddr;
        public UInt16 coordPanId;
        public Byte logicalChannel;
        public Byte channelPage;
        public int superframeSpec; // FIXME
        public bool gtsPermit;
        public Byte linkQuality;
        public UInt32 timeStamp;
        public MacEnum securityFailure;
        public SecurityOptions securityOptions;
    }

    /// <summary>
    /// 7.3.1.2 Capability Information field
    /// </summary>
    public struct CapabilityInformation
    {
        private byte _val;

        private bool GetValue(CapabilityInformationField cap)
        {
            return (_val & (1 << (byte)cap)) != 0;
        }

        private void SetValue(CapabilityInformationField cap, bool value)
        {
            if (value)
                _val |= (byte)(1 << (byte)cap);
            else
                _val &= (byte)~(1 << (byte)cap);
        }

        public byte ToByte()
        {
            return _val;
        }

        public void FromByte(byte val)
        {
            _val = val;
        }

        /// <summary>
        /// True if the device is capable of becoming the PAN coordinator.
        /// </summary>
        public bool AlternatePanCoordinator
        {
            get { return GetValue(CapabilityInformationField.AlternatePanCoord); }
            set { SetValue(CapabilityInformationField.AlternatePanCoord, value); }
        }

        /// <summary>
        /// True if the device is an FFD.
        /// False if the device in an RFD.
        /// </summary>
        public bool DeviceType
        {
            get { return GetValue(CapabilityInformationField.DeviceType); }
            set { SetValue(CapabilityInformationField.DeviceType, value); }
        }

        /// <summary>
        /// True if the device is receiving power from the alternating current mains.
        /// </summary>
        public bool PowerSource
        {
            get { return GetValue(CapabilityInformationField.PowerSource); }
            set { SetValue(CapabilityInformationField.PowerSource, value); }
        }

        /// <summary>
        /// True if the device does not disable its receiver to conserve power during idle periods.
        /// </summary>
        public bool ReceiverOnWhenIdle
        {
            get { return GetValue(CapabilityInformationField.ReceiverOnWhenIdle); }
            set { SetValue(CapabilityInformationField.ReceiverOnWhenIdle, value); }
        }

        /// <summary>
        /// Reserved1
        /// </summary>
        public bool Reserved1
        {
            get { return GetValue(CapabilityInformationField.Reserved1); }
            set { SetValue(CapabilityInformationField.Reserved1, value); }
        }

        /// <summary>
        /// Reserved2
        /// </summary>
        public bool Reserved2
        {
            get { return GetValue(CapabilityInformationField.Reserved2); }
            set { SetValue(CapabilityInformationField.Reserved2, value); }
        }

        /// <summary>
        /// true if the device is capable of sending and receiving cryptographically protected Mac frames
        /// as specified in 7.5.8.2
        /// </summary>
        public bool SecurityCapability
        {
            get { return GetValue(CapabilityInformationField.SecurityCapability); }
            set { SetValue(CapabilityInformationField.SecurityCapability, value); }
        }

        /// <summary>
        /// true if the device wishes the coordinator to allocate a 16-bit short address as a result of
        /// the association procedure
        /// </summary>
        public bool AllocateAddress
        {
            get { return GetValue(CapabilityInformationField.AllocateAddress); }
            set { SetValue(CapabilityInformationField.AllocateAddress, value); }
        }

        /// <summary>
        /// Returns a textual representation
        /// </summary>
        /// <returns>textual representation</returns>
        public override string ToString()
        {
            string description = "";
            description += "AllocateAddress: " + AllocateAddress.ToString();
            description += ", AlternatePanCoordinator: " + AlternatePanCoordinator.ToString();
            description += ", DeviceType: " + DeviceType.ToString();
            description += ", Reserved1: " + Reserved1.ToString();
            description += ", Reserved2: " + Reserved2.ToString();
            description += ", PowerSource: " + PowerSource.ToString();
            description += ", ReceiverOnWhenIdle: " + ReceiverOnWhenIdle.ToString();
            description += ", SecurityCapability: " + SecurityCapability.ToString();
            return description;
        }
    }

    /// <summary>
    /// Container for single Mac address, either short or extended addr.
    /// </summary>
    public struct MacAddress
    {
        MacAddressingMode _mode;
        UInt16 _shortAddress;
        UInt64 _extendedAddress;

        public MacAddressingMode Mode
        {
            get { return _mode; }
        }

        public ushort ShortAddress
        {
            get
            {
                if (_mode != MacAddressingMode.ShortAddress)
                    throw new ApplicationException("ShortAddress not defined.");
                return _shortAddress;
            }

            set
            {
                _mode = MacAddressingMode.ShortAddress;
                _shortAddress = value;
            }
        }

        public UInt64 ExtendedAddress
        {
            get
            {
                if (_mode != MacAddressingMode.ExtendedAddress)
                    throw new ApplicationException("Extended address not defined.");
                return _extendedAddress;
            }

            set
            {
                _mode = MacAddressingMode.ExtendedAddress;
                _extendedAddress = value;
            }
        }

        public MacAddress(UInt16 shortAddress)
        {
            _mode = MacAddressingMode.ShortAddress;
            _shortAddress = shortAddress;
            _extendedAddress = 0;
        }

        public MacAddress(UInt64 extendedAddress)
        {
            _mode = MacAddressingMode.ExtendedAddress;
            _shortAddress = 0;
            _extendedAddress = extendedAddress;
        }
    }

    public enum MacAddressingMode : byte
    {
        NoAddress = 0x00,
        ShortAddress = 0x02,
        ExtendedAddress = 0x03,
    }

    public enum ScanType : byte
    {
        ED = 0x01,
        ActiveScan = 0x02,
        PassiveScan = 0x03,
        OrphanScan = 0x04,
    }

    public struct TxOptions
    {
        public bool AcknowledgedTransmission;
        public bool GTSTransmission;
        public bool IndirectTransmission;
    }

    public struct RxOptions
    {

    }

    public enum AssociationResponseStatus : byte
    {
        Successful = 0x00,
        PanAtCapacity = 0x01,
        PanAccessDenied = 0x02,
    }

    public enum DisassociationReason : byte
    {
        DeviceMustLeave = 0x01,
        DeviceWishesToLeave = 0x02,
    }

    /// <summary>
    /// Table 86
    /// </summary>
    public enum PibAttribute : byte
    {
        /// <summary>
        /// int, 0..26
        /// </summary>
        phyCurrentChannel = Phy.PibAttribute.phyCurrentChannel,
        /// <summary>
        /// int[], 5 most significant bits: channel page, 27 lsb: boolean
        /// </summary>
        phyChannelsSupported = Phy.PibAttribute.phyChannelsSupported,
        /// <summary>
        /// int, -32..31 dBm
        /// </summary>
        phyTransmitPower = Phy.PibAttribute.phyTransmitPower,
        /// <summary>
        /// int
        /// </summary>
        phyCCAMode = Phy.PibAttribute.phyCCAMode,
        /// <summary>
        /// int
        /// </summary>
        phyCurrentPage = Phy.PibAttribute.phyCurrentPage,
        /// <summary>
        /// int
        /// </summary>
        phyMaxFrameDuration = Phy.PibAttribute.phyMaxFrameDuration,
        /// <summary>
        /// int
        /// </summary>
        phySHRDuration = Phy.PibAttribute.phySHRDuration,
        /// <summary>
        /// float
        /// </summary>
        phySymbolsPerOctet = Phy.PibAttribute.phySymbolsPerOctet,
        /// <summary>
        /// int
        /// </summary>
        macAckWaitDuration = 0x40,
        /// <summary>
        /// bool
        /// </summary>
        macAssociatedPANCoord = 0x56,
        /// <summary>
        /// bool
        /// </summary>
        macAssociationPermit = 0x41,
        /// <summary>
        /// bool
        /// </summary>
        macAutoRequest = 0x42,
        /// <summary>
        /// bool
        /// </summary>
        macBattLifeExt = 0x43,
        /// <summary>
        /// int
        /// </summary>
        macBattLifeExtPeriods = 0x44,
        /// <summary>
        /// Frame
        /// </summary>
        macBeaconPayload = 0x45,
        //macBeaconPayloadLength = 0x46, // frame, combined with payload
        /// <summary>
        /// int
        /// </summary>
        macBeaconOrder = 0x47,
        /// <summary>
        /// int
        /// </summary>
        macBeaconTxTime = 0x48,
        /// <summary>
        /// int
        /// </summary>
        macBSN = 0x49,
        /// <summary>
        /// UInt64
        /// </summary>
        macCoordExtendedAddress = 0x4a,
        /// <summary>
        /// int
        /// </summary>
        macCoordShortAddress = 0x4b,
        /// <summary>
        /// bool
        /// </summary>
        macDSN = 0x4c,
        /// <summary>
        /// bool
        /// </summary>
        macGTSPermit = 0x4d,
        /// <summary>
        /// int
        /// </summary>
        macMaxBE = 0x57,
        /// <summary>
        /// int
        /// </summary>
        macMaxCSMABackoffs = 0x4e,
        /// <summary>
        /// int
        /// </summary>
        macMaxFrameTotalWaitTime = 0x58,
        /// <summary>
        /// int
        /// </summary>
        macMaxFrameRetries = 0x59,
        /// <summary>
        /// int
        /// </summary>
        macMinBE = 0x4f,
        //macMinLIFSPeriod
        //macMinSIFSPeriod
        /// <summary>
        /// int
        /// </summary>
        macPanId = 0x50,
        /// <summary>
        /// bool
        /// </summary>
        macPromiscuousMode = 0x51,
        /// <summary>
        /// int
        /// </summary>
        macResponseWaitTime = 0x5a,
        /// <summary>
        /// bool
        /// </summary>
        macRxOnWhenIdle = 0x52,
        /// <summary>
        /// bool
        /// </summary>
        macSecurityEnabled = 0x5d,
        /// <summary>
        /// int
        /// </summary>
        macShortAddress = 0x53,
        /// <summary>
        /// int
        /// </summary>
        macSuperframeOrder = 0x54,
        /// <summary>
        /// int
        /// </summary>
        macSyncSymbolOffset = 0x5b,
        /// <summary>
        /// bool
        /// </summary>
        macTimestampSupported = 0x5c,
        /// <summary>
        /// int
        /// </summary>
        macTransactionPersistenceTime = 0x55
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
            Float,
            Bool,
            Frame
        }

        private ValueType _type;
        private int _int;
        private int[] _intArray;
        private float _float;
        private bool _bool;
        private Frame _frame;

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

        public bool Bool
        {
            get
            {
                if (_type != ValueType.Bool)
                    throw new ApplicationException("Bool value not defined");
                return _bool;
            }

            set
            {
                _bool = value;
                _type = ValueType.Bool;
            }
        }

        public Frame Frame
        {
            get
            {
                if (_type != ValueType.Frame)
                    throw new ApplicationException("Frame value not defined");
                return _frame;
            }

            set
            {
                _frame = value;
                _type = ValueType.Frame;
            }
        }
    }

    // 7.3.9.2
    public struct GtsCharacteristics
    {
        // FIXME
    }

    /// <summary>
    /// Ssecurity level identifier enumeration
    /// </summary>
    public enum SecurityLevelIdentifier : byte
    {
        /// <summary>
        ///  No security
        /// </summary>
        None = 0,
        /// <summary>
        /// CBC-Mac, MIC of 4 bytes.
        /// </summary>
        Mic32 = 1,
        /// <summary>
        /// CBC-Mac, MIC of 8 bytes.
        /// </summary>
        Mic64 = 2,
        /// <summary>
        /// CBC-Mac, MIC of 16 bytes.
        /// </summary>
        Mic128 = 3,
        /// <summary>
        /// Ctr encryption/decryption
        /// </summary>
        Enc = 4,
        /// <summary>
        /// Ccm, MIC of 4 bytes.
        /// </summary>
        EncMic32 = 5,
        /// <summary>
        /// Ccm, MIC of 8 bytes.
        /// </summary>
        EncMic64 = 6,
        /// <summary>
        /// Ccm, MIC of 16 bytes.
        /// </summary>
        EncMic128 = 7,
    }

    /// <summary>
    /// Security mode enumeration
    /// </summary>
    public enum SecurityMode : byte
    {
        /// <summary>
        /// No security
        /// </summary>
        InLineSecurityDisabled = 0,
        /// <summary>
        /// CbcMac authentication
        /// </summary>
        CbcMac = 1,
        /// <summary>
        /// Ctr encryption
        /// </summary>
        Ctr = 2,
        /// <summary>
        /// Ccm authentication +  encryption
        /// </summary>
        Ccm = 3,
    }

    /// <summary>
    /// Security key identifier enumeration
    /// </summary>
    public enum SecurityKeyIdentifierMode : byte
    {
        /// <summary>
        /// Key is determined implicitly from the
        /// originator and receipient(s) of the frame,
        /// as indicated in the frame header.
        /// </summary>
        ImplicitFromOriginatorAndRecipient = 0,
        /// <summary>
        /// Key is determined from the 1-octet Key
        /// Index subfield of the Key Identifier field
        /// of the auxiliary security header in
        /// conjunction with macDefaultKeySource
        /// </summary>
        FromKeyIndexSubfield = 1,
        /// <summary>
        /// Key is determined explicitly from the
        /// 4-octet Key Source subfield and the
        /// 1-octet Key Index subfield of the Key
        /// Identifier field of the auxiliary security
        /// header.
        /// </summary>
        From4OctetsSourceAndKeyIndexSubfields = 2,
        /// <summary>
        /// Key is determined explicitly from the
        /// 8-octet Key Source subfield and the
        /// 1-octet Key Index subfield of the Key
        /// Identifier field of the auxiliary security
        /// header.
        /// </summary>
        From8OctetsSourceAndKeyIndexSubfields = 3,
    }

    public struct SecurityOptions
    {
        public SecurityLevelIdentifier SecurityLevel;
        public SecurityKeyIdentifierMode KeyIdMode;
        public byte[] KeySource;
        public byte keyIndex;
    }

    /// <summary>
    /// (7.3.1.2) Capability information for the Radio.
    /// </summary>
    internal enum CapabilityInformationField : byte
    {
        /// <summary>
        /// If set, device is capable to act as a pan coordinator.
        /// </summary>
        AlternatePanCoord = 0, // 0: Not capable of becoming a PAN coord, 1: capable
        /// <summary>
        /// If 0, device is a Reduced function device.
        /// If 1, device is a Full function device.
        /// </summary>
        DeviceType = 1, // 0 : RFD device, , 1 : FFD device
        /// <summary>
        /// If 1, device is receiving power form the alternating current mains. Otherwise 0.
        /// </summary>
        PowerSource = 2, // 1: deice is receiving power form the alternating current mains. Otherwise 0
        /// <summary>
        /// If set, device is listening even if idle. Otherwise, device switches of
        /// received during idle times to reduce power consumption.
        /// </summary>
        ReceiverOnWhenIdle = 3, // 1 if the device does not disable its receiver to conserve power during idle time, 0 otherwise

        Reserved1 = 4,

        Reserved2 = 5,

        /// <summary>
        /// If set, device is able to handle security options.
        /// </summary>
        SecurityCapability = 6, // 1: device capable of using security, 0 otherwise
        /// <summary>
        /// If 1, device wishes the coordinator to allocate a short address as a result of the association porocedure.
        ///  If 0, special short address 0xfffe shall be allocated to the device and returned through the association response command.
        ///  In this case, the device shall communicate on the PAN using only its 64 bit extended address.
        /// </summary>
        AllocateAddress = 7, // 1 if the device wishes the coordinator to allocate a short address as a result of the association porocedure.
        //  0: special short address 0xfffe shall be allocated to the device and returned through the association response command.
        //  In this case, the device shall communicate on the PAN using only its 64 bit extended address.
    }

    #endregion
}


