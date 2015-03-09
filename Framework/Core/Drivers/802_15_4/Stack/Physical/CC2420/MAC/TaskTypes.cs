////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using Microsoft.SPOT.Wireless.IEEE_802_15_4;
using Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac;

namespace Microsoft.SPOT.Wireless.IEEE_802_15_4.Mac
{
    /// <summary>
    /// Types for internally queued taks.
    /// </summary>
    enum TaskType : int
    {
        none = 0,
        // request from upper layer
        DataRequest,
        PurgeRequest,
        AssociateRequest,
        AssociateResponse,
        DisassociateRequest,
        GetRequest,
        GTSRequest,
        OrphanResponse,
        ResetRequest,
        RXEnableRequest,
        ScanRequest,
        SetRequest,
        StartRequest,
        SyncRequest,
        PollRequest,
        // request from peers
        BeaconRequest
    }

    /// <summary>
    /// Container for any task.
    /// </summary>
    class Task
    {
        public readonly TaskType taskType;

        public Task(TaskType type)
        {
            taskType = type;
        }
    }

    class TaskDataRequest : Task
    {
        public readonly MacAddressingMode srcAddrMode;
        public readonly MacAddress dstAddr;
        public readonly UInt16 dstPANId;
        public Frame msdu;
        public readonly Byte msduHandle;
        public readonly TxOptions options;
        public readonly SecurityOptions securityOptions;
        public readonly DataConfirmHandler handler;

        public TaskDataRequest(
            MacAddressingMode srcAddrMode,
            MacAddress dstAddr,
            UInt16 dstPANId,
            Frame msdu,
            Byte msduHandle,
            TxOptions options,
            SecurityOptions securityOptions,
            DataConfirmHandler handler)
            : base(TaskType.DataRequest)
        {
            this.srcAddrMode = srcAddrMode;
            this.dstAddr = dstAddr;
            this.dstPANId = dstPANId;
            this.msdu = msdu;
            this.msduHandle = msduHandle;
            this.options = options;
            this.securityOptions = securityOptions;
            this.handler = handler;
        }
    }

    class TaskPurgeRequest : Task
    {
        public readonly Byte msduHandle;
        public readonly PurgeConfirmHandler handler;

        public TaskPurgeRequest(
            Byte msduHandle,
            PurgeConfirmHandler handler)
            : base(TaskType.PurgeRequest)
        {
            this.msduHandle = msduHandle;
            this.handler = handler;
        }
    }

    class TaskAssociateRequest : Task
    {
        public readonly Byte logicalChannel;
        public readonly Byte channelPage;
        public readonly MacAddress coordAddr;
        public readonly UInt16 coordPanId;
        public readonly CapabilityInformation capability;
        public readonly SecurityOptions securityOptions;
        public readonly AssociateConfirmHandler handler;

        public TaskAssociateRequest(
            Byte logicalChannel,
            Byte channelPage,
            MacAddress coordAddr,
            UInt16 coordPanId,
            CapabilityInformation capability,
            SecurityOptions securityOptions,
            AssociateConfirmHandler handler)
            : base(TaskType.AssociateRequest)
        {
            this.logicalChannel = logicalChannel;
            this.channelPage = channelPage;
            this.coordAddr = coordAddr;
            this.coordPanId = coordPanId;
            this.capability = capability;
            this.securityOptions = securityOptions;
            this.handler = handler;
        }
    }

    class TaskAssociateResponse : Task
    {
        public readonly UInt64 deviceAddress;
        public readonly UInt16 assocShortAddress;
        public readonly MacEnum status;
        public readonly SecurityOptions securityOptions;

        public TaskAssociateResponse(
            UInt64 deviceAddress,
            UInt16 assocShortAddress,
            MacEnum status,
            SecurityOptions securityOptions)
            : base(TaskType.AssociateResponse)
        {
            this.deviceAddress = deviceAddress;
            this.assocShortAddress = assocShortAddress;
            this.status = status;
            this.securityOptions = securityOptions;
        }
    }

    class TaskDisassociateRequest : Task
    {
        public readonly MacAddress deviceAddr;
        public readonly UInt16 devicePanId;
        public readonly DisassociationReason reason;
        public readonly bool txIndirect;
        public readonly SecurityOptions securityOptions;
        public readonly DisassociateConfirmHandler handler;

        public TaskDisassociateRequest(
            MacAddress deviceAddr,
            UInt16 devicePanId,
            DisassociationReason reason,
            bool txIndirect,
            SecurityOptions securityOptions,
            DisassociateConfirmHandler handler)
            : base(TaskType.DisassociateRequest)
        {
            this.deviceAddr = deviceAddr;
            this.devicePanId = devicePanId;
            this.reason = reason;
            this.txIndirect = txIndirect;
            this.securityOptions = securityOptions;
            this.handler = handler;
        }
    }

    class TaskGetRequest : Task
    {
        public readonly PibAttribute attribute;
        public readonly int attributeIndex;
        public readonly GetConfirmHandler handler;

        public TaskGetRequest(
            PibAttribute attribute,
            int attributeIndex,
            GetConfirmHandler handler)
            : base(TaskType.GetRequest)
        {
            this.attribute = attribute;
            this.attributeIndex = attributeIndex;
            this.handler = handler;
        }
    }

    class TaskGtsRequest : Task
    {
        public readonly GtsCharacteristics gtsCharacteristics;
        public readonly SecurityOptions securityOptions;
        public readonly GtsConfirmHandler handler;

        public TaskGtsRequest(
            GtsCharacteristics gtsCharacteristics,
            SecurityOptions securityOptions,
            GtsConfirmHandler handler)
            : base(TaskType.GTSRequest)
        {
            this.gtsCharacteristics = gtsCharacteristics;
            this.securityOptions = securityOptions;
            this.handler = handler;
        }
    }

    class TaskOrphanResponse : Task
    {
        public readonly UInt64 orphanAddress;
        public readonly UInt16 shortAddr;
        public readonly bool associatedMember;
        public readonly SecurityOptions securityOptions;

        public TaskOrphanResponse(
            UInt64 orphanAddress,
            UInt16 shortAddr,
            bool associatedMember,
            SecurityOptions securityOptions)
            : base(TaskType.OrphanResponse)
        {
            this.orphanAddress = orphanAddress;
            this.shortAddr = shortAddr;
            this.associatedMember = associatedMember;
            this.securityOptions = securityOptions;
        }
    }

    class TaskResetRequest : Task
    {
        public readonly bool setDefaultPIB;
        public readonly ResetConfirmHandler handler;

        public TaskResetRequest(
            bool setDefaultPIB,
            ResetConfirmHandler handler)
            : base(TaskType.ResetRequest)
        {
            this.setDefaultPIB = setDefaultPIB;
            this.handler = handler;
        }
    }

    class TaskRxEnableRequest : Task
    {
        public readonly bool deferPermit;
        public readonly UInt16 rxOnTime;
        public readonly UInt16 rxOnDuration;
        public readonly RxEnableConfirmHandler handler;

        public TaskRxEnableRequest(
            bool deferPermit,
            UInt16 rxOnTime,
            UInt16 rxOnDuration,
            RxEnableConfirmHandler handler)
            : base(TaskType.RXEnableRequest)
        {
            this.deferPermit = deferPermit;
            this.rxOnTime = rxOnTime;
            this.rxOnDuration = rxOnDuration;
            this.handler = handler;
        }
    }

    class TaskScanRequest : Task
    {
        public readonly ScanType scanType;
        public readonly UInt32 scanChannels;
        public readonly byte scanDuration;
        public readonly byte channelPage;
        public readonly SecurityOptions securityOptions;
        public readonly ScanChannelConfirmHandler handler;

        public TaskScanRequest(
            ScanType scanType,
            UInt32 scanChannels,
            byte scanDuration,
            byte channelPage,
            SecurityOptions securityOptions,
            ScanChannelConfirmHandler handler)
            : base(TaskType.ScanRequest)
        {
            this.scanType = scanType;
            this.scanChannels = scanChannels;
            this.scanDuration = scanDuration;
            this.channelPage = channelPage;
            this.securityOptions = securityOptions;
            this.handler = handler;
        }
    }

    class TaskSetRequest : Task
    {
        public readonly PibAttribute attribute;
        public readonly int index;
        public readonly PibValue value;
        public readonly SetConfirmHandler handler;

        public TaskSetRequest(
            PibAttribute attribute,
            int index,
            PibValue value,
            SetConfirmHandler handler)
            : base(TaskType.SetRequest)
        {
            this.attribute = attribute;
            this.index = index;
            this.value = value;
            this.handler = handler;
        }
    }

    class TaskStartRequest : Task
    {
        public readonly UInt16 panId;
        public readonly Byte logicalChannel;
        public readonly Byte channelPage;
        public readonly UInt16 startTime;
        public readonly byte beaconOrder;
        public readonly byte superframeOrder;
        public readonly bool panCoordinator;
        public readonly bool batteryLifeExtension;
        public readonly bool coordRealignment;
        public readonly SecurityOptions coordRealignSecutiryOptions;
        public readonly SecurityOptions beaconSecurityOptions;
        public readonly StartConfirmHandler handler;

        public TaskStartRequest(
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
            StartConfirmHandler handler)
            : base(TaskType.StartRequest)
        {
            this.panId = panId;
            this.logicalChannel = logicalChannel;
            this.channelPage = channelPage;
            this.startTime = startTime;
            this.beaconOrder = beaconOrder;
            this.superframeOrder = superframeOrder;
            this.panCoordinator = panCoordinator;
            this.batteryLifeExtension = batteryLifeExtension;
            this.coordRealignment = coordRealignment;
            this.coordRealignSecutiryOptions = coordRealignSecutiryOptions;
            this.beaconSecurityOptions = beaconSecurityOptions;
            this.handler = handler;
        }
    }

    class TaskSyncRequest : Task
    {
        public readonly ushort logicalChannel;
        public readonly ushort logicalPage;
        public readonly bool trackBeacon;

        public TaskSyncRequest(
            ushort logicalChannel,
            ushort logicalPage,
            bool trackBeacon)
            : base(TaskType.SyncRequest)
        {
            this.logicalChannel = logicalChannel;
            this.logicalPage = logicalPage;
            this.trackBeacon = trackBeacon;
        }
    }

    class TaskPollRequest : Task
    {
        public readonly MacAddress coordAddr;
        public readonly UInt16 coordPanId;
        public readonly SecurityOptions securityOptions;
        public readonly PollConfirmHandler handler;

        public TaskPollRequest(
            MacAddress coordAddr,
            UInt16 coordPanId,
            SecurityOptions securityOptions,
            PollConfirmHandler handler)
            : base(TaskType.PollRequest)
        {
            this.coordAddr = coordAddr;
            this.coordPanId = coordPanId;
            this.securityOptions = securityOptions;
            this.handler = handler;
        }
    }

    class TaskBeaconRequest : Task
    {
        public TaskBeaconRequest()
            : base(TaskType.BeaconRequest)
        { }
    }
}


