////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Microsoft.SPOT.MFUpdate
{
    using System;
    using System.Text;
    using System.Collections;
    using Microsoft.SPOT.Hardware;

    /// <summary>
    /// Represents a the firmware version as defined in native code.
    /// </summary>
    public class FirmwareVersion
    {
        private static FirmwareVersion s_Instance;

        private FirmwareVersion()
        {
            Version = SystemInfo.Version;
            OemName = SystemInfo.OEMString;
            OemId = SystemInfo.SystemID.OEM;
            Model = SystemInfo.SystemID.Model;
            SKU = SystemInfo.SystemID.SKU;
            IsBigEndian = SystemInfo.IsBigEndian;
        }

        /// <summary>
        /// Returns the singleton FirmwareVersion object for the device.
        /// </summary>
        public static FirmwareVersion Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    lock (typeof(FirmwareVersion))
                    {
                        if (s_Instance == null)
                        {
                            s_Instance = new FirmwareVersion();
                        }
                    }
                }

                return s_Instance;
            }
        }

        /// <summary>
        /// Readonly version for the device's firmware.
        /// </summary>
        public readonly Version Version;
        /// <summary>
        /// Readonly OEM name for the device.
        /// </summary>
        public readonly string OemName;
        /// <summary>
        /// Readonly OEM identification number for the device.
        /// </summary>
        public readonly byte OemId;
        /// <summary>
        /// Readonly model number for the device.
        /// </summary>
        public readonly byte Model;
        /// <summary>
        /// Readonly SKU number for the device.
        /// </summary>
        public readonly ushort SKU;
        /// <summary>
        /// Property to determine if the device is big endiean.
        /// </summary>
        public readonly bool IsBigEndian;
    }

    /// <summary>
    /// Represents a firmware update package.
    /// </summary>
    public class MFFirmwareUpdate : MFUpdate
    {
        /// <summary>
        /// Creates a firmware update package.
        /// </summary>
        /// <param name="provider">The update package provider name.</param>
        /// <param name="updateID">The unique identification number for the update.</param>
        /// <param name="version">The version of the update.</param>
        /// <param name="firmwareType">The firmware subtype for the udpate.</param>
        /// <param name="updateSize">The amount of space (in bytes) to store the update.</param>
        /// <param name="pktSize">The size (in bytes) of each packet.</param>
        public MFFirmwareUpdate(
            string provider,
            uint updateID,
            Version version,
            MFUpdateSubType firmwareType,
            int updateSize,
            int pktSize
            )
            : base(provider, updateID, version, MFUpdateType.FirmwareUpdate, firmwareType, updateSize, pktSize)
        {
        }

        /// <summary>
        /// Gets the current firmware version.
        /// </summary>
        public FirmwareVersion CurrentFirmwareVersion
        {
            get
            {
                return FirmwareVersion.Instance;
            }
        }
    }
}