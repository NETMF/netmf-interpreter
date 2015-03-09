////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Microsoft.SPOT.MFUpdate
{
    using System;

    /// <summary>
    /// Represents a device key update.
    /// </summary>
    public class MFKeyUpdate : MFUpdate
    {
        /// <summary>
        /// Creates a device key update package.
        /// </summary>
        /// <param name="provider">The update provider name that will handle this update.</param>
        /// <param name="updateID">The unique identification number for this update.</param>
        /// <param name="version">The version of the update.</param>
        /// <param name="keyType">The key subtype for the update.</param>
        /// <param name="keySize">The key size (in bytes).</param>
        /// <param name="wrappedKey">The wrapped key data (handle, name, etc.) used to unwrap the update.</param>
        public MFKeyUpdate(
            string provider,
            uint updateID,
            Version version,
            MFUpdateSubType keyType,
            int keySize,
            byte[] wrappedKey
            )
            : base(provider, updateID, version, MFUpdateType.KeyUpdate, keyType, keySize, keySize)
        {
            base.AddPacket(new MFUpdatePkt(0, wrappedKey, null));
        }
    }
}