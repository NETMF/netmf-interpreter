////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("MFUpdate")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Microsoft.SPOT.Platform.Tests.UpdateTests")]

namespace Microsoft.SPOT.MFUpdate
{
    /// <summary>
    /// Represents a user defined update property for an MFUpdate.
    /// </summary>
    public class MFUpdateProperty
    {
        /// <summary>
        /// Initializes and instance of the MFUpdateProperty class.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The binary representation of the property value.</param>
        public MFUpdateProperty(string name, byte[] value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// The name of the property;
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// The binary representation of the property value.
        /// </summary>
        public readonly byte[] Value;
    }

    /// <summary>
    /// Enumeration of the known update types.  This enumeration can be extended by
    /// the user by performing a bitwise 'OR' operation with the UserDefined value and
    /// the user identified value.  This must match the native code implemenation of 
    /// the MFUpdate component.
    /// </summary>
    public enum MFUpdateType : ushort
    {
        FirmwareUpdate = 0x0000,
        AssemblyUpdate = 0x0001,
        KeyUpdate      = 0x0002,
        UserDefined    = 0x8000
    }

    /// <summary>
    /// An enumeration of the MFUpdate sub types for an update.  These values must
    /// match the native code implemenation of the MFUpdate component.
    /// </summary>
    public enum MFUpdateSubType : ushort
    {
        None = 0,

        FirmwareUpdate_CLR = 0x0001,
        FirmwareUpdate_DAT = 0x0002,
        FirmwareUpdate_CFG = 0x0003,

        /// <summary>
        /// Adds a new assembly to the deployment sector
        /// </summary>
        AssemblyUpdate_NEW  = 0x0001,
        /// <summary>
        /// Replaces the entire deployment sector with the assemblies in the 
        /// update.
        /// </summary>
        AssemblyUpdate_REPLACE_DEPLOY = 0x0002,

        UserDefined        = 0x8000,
    }

    /// <summary>
    /// Abstract base class for MFUpdates.  The fields in this class
    /// represent the required information needed by the native MFUpdate
    /// component in order to perform an update.
    /// </summary>
    public abstract class MFUpdateBase
    {
        /// <summary>
        /// The update provider name.  This value is used to determine which
        /// MFUpdate package to use for the update.
        /// </summary>
        protected string          m_provider;
        /// <summary>
        /// The identification number for the update.  This number should be unique
        /// for a given update type/subtype.
        /// </summary>
        protected uint            m_updateID;
        /// <summary>
        /// The version of the update (Major, Minor, Build, Revision).
        /// </summary>
        protected Version         m_updateVersion;
        /// <summary>
        /// The type of the update (firmware, assembly, etc.).
        /// </summary>
        protected MFUpdateType    m_updateType;
        /// <summary>
        /// The subtype of the update.
        /// </summary>
        protected MFUpdateSubType m_updateSubType;
        /// <summary>
        /// The total amount of memory (in bytes) required to store the entire update.
        /// </summary>
        protected int             m_updateSize;
        /// <summary>
        /// The packet size in bytes.  All packets (except the last one) are required
        /// to be the same size.
        /// </summary>
        protected int             m_packetSize;
        /// <summary>
        /// The handle for the update.
        /// </summary>
        protected int             m_updateHandle;
    }

    /// <summary>
    /// The interop class for the MFUpdate feature.
    /// </summary>
    internal static class MFNativeUpdate
    {
        /// <summary>
        /// static constructor that initializes the MFUpdate system.
        /// </summary>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        static extern MFNativeUpdate();

        /// <summary>
        /// Initializes an MFUpdate.
        /// </summary>
        /// <param name="update">The required update properties.</param>
        /// <returns>Returns the update handle on success or -1 on failure.</returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern int Initialize(MFUpdateBase update);

        /// <summary>
        /// Performs an authentication command.  These commands are defined by the native
        /// code update provider.  This method is intented to enable handshaking during 
        /// authentication.  For our samples, this method is only used to determine what
        /// validation is supported on the device.
        /// </summary>
        /// <param name="updateHandle">The update handle.</param>
        /// <param name="command">The command ID for the authentication command.</param>
        /// <param name="args">The binary representation of the arguments for the command.</param>
        /// <param name="response">The binary representation of the response.</param>
        /// <returns>Returns true if successful, false otherwise.</returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool AuthCommand(int updateHandle, int command, byte[] args, ref byte[] response);

        /// <summary>
        /// Performs the final authentication step for the update.  For updates that 
        /// don't require authentication or perform authentication at a different level
        /// (SSL), the authentication value can be null.  Then native MFUpdate component
        /// requires that this call be made regardless if the underlying update package
        /// uses it.
        /// </summary>
        /// <param name="updateHandle">The handle of the update.</param>
        /// <param name="authentication">The data for the final authentication step.</param>
        /// <returns>Returns true if successful or false otherwise.</returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool Authenticate(int updateHandle, byte[] authentication);

        /// <summary>
        /// Attempts to open an existing update that matches the update properties assigned
        /// in the constructor.
        /// </summary>
        /// <param name="updateHandle">The handle to the update.</param>
        /// <returns>Returns false if the file could not be opened, true otherwise.</returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool Open(int updateHandle);

        /// <summary>
        /// Creates an update storage file for saving the udpate packets.
        /// </summary>
        /// <param name="updateHandle">The update handle.</param>
        /// <returns>Returns true if the file was created, false otherwise.</returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool Create(int updateHandle);

        /// <summary>
        /// Gets an array of uints that represents the missing packets.  Each bit of each
        /// uint represents an individual packet (starting with the first uint's least 
        /// significant bit).   Any bit that is a '1' is missing from the update.
        /// </summary>
        /// <param name="updateHandle">The update handle.</param>
        /// <param name="packetBitCheck">A bitfield representing the missing packets; 
        /// where '1's denote a missing packet and '0's denote a saved packet.</param>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void GetMissingPackets(int updateHandle, uint[] packetBitCheck);

        /// <summary>
        /// Gets an update property for the given update and property name.  The DeserializeParameter
        /// method can be used to deserialize simple objects.
        /// </summary>
        /// <param name="updateHandle">The update handle.</param>
        /// <param name="propName">The name of the property that will be retrieved.</param>
        /// <param name="propValue">The binary representation of the property value.</param>
        /// <returns>Returns true if the property exists, otherwise false.</returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool GetUpdateProperty(int updateHandle, string propName, ref byte[] propValue);

        /// <summary>
        /// Sets the update property for the given update and property name/value.  The SerializeParameter
        /// method can be used to serialize simple objects.
        /// </summary>
        /// <param name="updateHandle">The update handle.</param>
        /// <param name="propName">The name of the property to be stored.</param>
        /// <param name="propValue">The binary representation of the property to be stored.</param>
        /// <returns>Returns true if the property could be set, false otherwise.</returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool SetUpdateProperty(int updateHandle, string propName, byte[] propValue);

        /// <summary>
        /// Adds an indexed packet to the given updates storage file.
        /// </summary>
        /// <param name="updateHandle">The update handle.</param>
        /// <param name="packetIndex">The ordered index of the packet to be stored.</param>
        /// <param name="packetData">The packet data.</param>
        /// <param name="packetValidation">The packet validation data defined by the native update component (CRC value in moste cases).</param>
        /// <returns>Returns true if the packet could be saved, false otherwise.</returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool AddPacket(int updateHandle, int packetIndex, byte[] packetData, byte[] packetValidation);

        /// <summary>
        /// Validates the entire update package with the given validation data.  The
        /// validation data is determined by the native portion of the update component.
        /// This method allows the update software to determine if an update installation
        /// will likely succeed before calling the install which may require a reboot.
        /// </summary>
        /// <param name="updateHandle">The handle to the update.</param>
        /// <param name="validation">The validation data for the entire update package (signature, CRC, etc).</param>
        /// <returns>Returns true if the update was validated, false otherwise.</returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool Validate(int updateHandle, byte[] validation);

        /// <summary>
        /// Validates the update with the given validation data (unless already validated)
        /// and transfers control to the native install component (e.g. MicroBooter) for installation. 
        /// </summary>
        /// <param name="updateHandle">The update handle.</param>
        /// <param name="validation">The validation data for the entire update package.</param>
        /// <returns>Returns true if the install was successfully started, false otherwise.  Since
        /// the install may require a reboot this method may not return.</returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool Install(int updateHandle, byte[] validation);

        /// <summary>
        /// Deletes the update storage file.
        /// </summary>
        /// <param name="updateHandle">The handle of the update.</param>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void DeleteUpdate(int updateHandle);

        /// <summary>
        /// Serializes simple managed objects into a binary format similar to a C/C++ structure.  
        /// Nested objects are supported but any arrays should have fixed sizes.
        /// </summary>
        /// <param name="data">The simple data object to be serialized.</param>
        /// <returns>Returns true if the serialization was successful, false otherwise.</returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern byte[] SerializeParameter(object data);

        /// <summary>
        /// Deserializes binary data into a simple known managed object.
        /// </summary>
        /// <param name="data">The binary representation of the object.</param>
        /// <param name="parameter">The object for which the deserialized data will be stored.</param>
        /// <returns>Returns true if the deserialization was successful, false otherwise.</returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern bool DeserializeParameter(byte[] data, object parameter);
    }
}
