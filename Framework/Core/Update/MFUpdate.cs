////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Microsoft.SPOT.MFUpdate
{
    using System;
    using System.Collections;

    /// <summary>
    /// Represents a missing packet enumeraion for an update. This helper class
    /// converts the interop class's bitfield into a more user friendly enumerator.
    /// </summary>
    public class MissingPacketEnumerator : IEnumerator
    {
        private uint []m_pktBitChk;
        private int m_currentIdx;
        private int m_maxPkt;

        /// <summary>
        /// Creates a missing packet enumerator from the given bitfield.
        /// </summary>
        /// <param name="pktBitCheck">The bitfield retured by MFNativeUpdate.GetMissingPackets.</param>
        internal MissingPacketEnumerator(uint[] pktBitCheck)
        {
            m_pktBitChk = pktBitCheck;
            m_currentIdx = -1;
            m_maxPkt = pktBitCheck.Length << 5;
        }

        #region IEnumerator Members
        /// <summary>
        /// (Inherited from IEnumerator).  Gets the current missing packet index. 
        /// MoveNext must be called prior to calling this method.
        /// </summary>
        public object  Current
        {
            get 
            { 
                if(m_currentIdx ==       -1) throw new InvalidOperationException();
                if(m_currentIdx >= m_maxPkt) throw new ArgumentOutOfRangeException();

                return m_currentIdx;
            }
        }

        /// <summary>
        /// (Inherited from IEnumerator).  Moves to the next available missing packet.
        /// </summary>
        /// <returns>Returns true if a missing packet remains, false otherwise.</returns>
        public bool MoveNext()
        {
            m_currentIdx++;

            while(m_currentIdx < m_maxPkt)
            {
                uint part = m_pktBitChk[m_currentIdx >> 5];

                for(int i=m_currentIdx % 32; i<32; i++)
                {
                    if((part & (1u << i)) != 0)
                    {
                        return true;
                    }
                    m_currentIdx++;
                }                
            }

            return false;
        }

        /// <summary>
        /// (Inherited from IEnumerator).  Resets the enumerator.
        /// </summary>
        public void  Reset()
        {
            m_currentIdx = -1;
        }

        #endregion
    }

    /// <summary>
    /// Represents the abstract base class for MFUpdates.
    /// </summary>
    public abstract class MFUpdate : MFUpdateBase
    {
        private uint [] m_pktBitChk;
        private int m_maxPkt;
        private bool m_authenticated;

        /// <summary>
        /// Creates an mfupdate object.
        /// </summary>
        /// <param name="provider">The name of the update provider that will service this update.</param>
        /// <param name="updateID">The unique identification number of the update.</param>
        /// <param name="version">The version of the update.</param>
        /// <param name="updateType">The update type.</param>
        /// <param name="updateSubType">The update subtype.</param>
        /// <param name="updateSize">The total update size (in bytes).</param>
        /// <param name="pktSize">The size (in bytes) of each packet.</param>
        public MFUpdate(string provider, uint updateID, Version version, MFUpdateType updateType, MFUpdateSubType updateSubType, int updateSize, int pktSize)
        {
            m_maxPkt = (updateSize + pktSize - 1) / pktSize;
            m_pktBitChk = new uint[(m_maxPkt + 31) >> 5];

            for (int i = 0; i < m_pktBitChk.Length; i++)
            {
                m_pktBitChk[i] = 0xFFFFFFFF;
            }

            m_provider      = provider;
            m_updateID      = updateID;
            m_updateType    = updateType;
            m_updateSubType = updateSubType;
            m_updateVersion = version;
            m_updateSize    = updateSize;
            m_packetSize    = pktSize;
            m_authenticated = false;

            m_updateHandle = MFNativeUpdate.Initialize(this);

            if (m_updateHandle == -1) throw new Exception();
        }

        /// <summary>
        /// Performs an authentication command.  These commands are defined by the native
        /// code update provider.  This method is intented to enable handshaking during 
        /// authentication.  For our samples, this method is only used to determine what
        /// validation is supported on the device.
        /// </summary>
        /// <param name="command">The command ID for the authentication call.</param>
        /// <param name="args">The argument object that corresponds to the command ID.</param>
        /// <param name="response">The response object for the command.</param>
        /// <returns>Returns true if the command succeeded, false otherwise.</returns>
        public bool AuthenticationCommand(int command, object args, ref object response)
        {
            byte[] resp = response == null ? null : MFNativeUpdate.SerializeParameter(response);

            bool fRet = MFNativeUpdate.AuthCommand(m_updateHandle, command, MFNativeUpdate.SerializeParameter(args), ref resp);

            if (fRet && resp != null)
            {
                response = MFNativeUpdate.DeserializeParameter(resp, response);
            }

            return fRet;
        }

        /// <summary>
        /// Authenticates the update and opens the update storage file.  This method must be called prior to adding packets.
        /// If authentication is not required by the underlying native update component null can be passed for the 
        /// authenticationData parameter.
        /// </summary>
        /// <param name="authenticationData">The authentication data for starting the update (can be null).</param>
        /// <returns></returns>
        public bool Open(object authenticationData)
        {
            m_authenticated = MFNativeUpdate.Authenticate(m_updateHandle, authenticationData != null ? MFNativeUpdate.SerializeParameter(authenticationData) : null);

            if (m_authenticated)
            {
                if (MFNativeUpdate.Open(m_updateHandle))
                {
                    MFNativeUpdate.GetMissingPackets(m_updateHandle, m_pktBitChk);
                }
                else
                {
                    if (!MFNativeUpdate.Create(m_updateHandle)) throw new OutOfMemoryException();
                }
            }

            return m_authenticated;
        }

        /// <summary>
        /// Validates and stores an indexed packet to the update storage file.
        /// </summary>
        /// <param name="packet">The packet data to be stored.</param>
        /// <returns>Returns true if successful, false otherwise.</returns>
        public bool AddPacket(MFUpdatePkt packet)
        {
            if (m_updateHandle == -1 || !m_authenticated) throw new InvalidOperationException();

            int pktIndex = packet.PacketIndex;
            if (pktIndex >= m_maxPkt) throw new IndexOutOfRangeException();
            if (packet.Data == null || packet.Data.Length > m_packetSize) throw new ArgumentException();

            int div = pktIndex >> 5;
            int rem = pktIndex % 32;

            // We already have this packet
            if (0 == (m_pktBitChk[div] & (1u << rem))) return true;

            if (MFNativeUpdate.AddPacket(m_updateHandle, packet.PacketIndex, packet.Data, packet.ValidationData))
            {
                m_pktBitChk[pktIndex >> 5] &= ~(1u << (pktIndex % 32));
                
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the update property given the property name.
        /// </summary>
        /// <param name="propName">The name of the property.</param>
        /// <param name="propValue">The returned property object.</param>
        /// <returns>Returns false if the property was not set or is not supported.  Returns true otherwise.</returns>
        public bool GetUpdateProperty(string propName, ref object propValue)
        {
            if (m_updateHandle == -1) throw new InvalidOperationException();

            byte[] ser = MFNativeUpdate.SerializeParameter(propValue);

            if(MFNativeUpdate.GetUpdateProperty(m_updateHandle, propName, ref ser))
            {
                return MFNativeUpdate.DeserializeParameter(ser, propValue);
            }

            return false;
        }

        /// <summary>
        /// Gets the raw property value for a given property name.
        /// </summary>
        /// <param name="propName">The name of the property.</param>
        /// <param name="propValue">The binary representation of the property value.</param>
        /// <returns>Returns false if the property was not set or is not supported.  Returns true otherwise.</returns>
        public bool GetUpdateProperty(string propName, ref byte[] propValue)
        {
            if (m_updateHandle == -1) throw new InvalidOperationException();

            return MFNativeUpdate.GetUpdateProperty(m_updateHandle, propName, ref propValue);
        }

        /// <summary>
        /// Sets the update property for the given property name.
        /// </summary>
        /// <param name="propName">The name of the property to be set.</param>
        /// <param name="propValue">The value of property to be set.</param>
        /// <returns>Returns true if the property was set, false otherwise.</returns>
        public bool SetUpdateProperty(string propName, object propValue)
        {
            byte[] data = MFNativeUpdate.SerializeParameter(propValue);

            return MFNativeUpdate.SetUpdateProperty(m_updateHandle, propName, data);
        }

        /// <summary>
        /// Sets the raw update property for the given property name.
        /// </summary>
        /// <param name="propName">The name of the property to be set.</param>
        /// <param name="propValue">The binary representation of the property value to be set.</param>
        /// <returns>Returns true if the property was set, false otherwise.</returns>
        public bool SetUpdateProperty(string propName, byte[] propValue)
        {
            if (m_updateHandle == -1) throw new InvalidOperationException();

            return MFNativeUpdate.SetUpdateProperty(m_updateHandle, propName, propValue);
        }

        /// <summary>
        /// Gets a missing packet enumerator object that represents the number of missing packets.
        /// </summary>
        /// <returns>returns a MissingPktEnum enumerator object.</returns>
        public MissingPacketEnumerator GetMissingPacketsEnumerator()
        {
            if (m_updateHandle == -1) throw new InvalidOperationException();

            return new MissingPacketEnumerator(m_pktBitChk);
        }

        /// <summary>
        /// Deletes the current update's storage file.
        /// </summary>
        public void DeleteUpdate()
        {
            if (m_updateHandle == -1 || !m_authenticated) throw new InvalidOperationException();

            MFNativeUpdate.DeleteUpdate(m_updateHandle);
        }

        /// <summary>
        /// Validates the entire update package with the given validation data.  The validation data
        /// is determined by the native update component.  
        /// </summary>
        /// <param name="updateValidation">The validation data for the entire update package.</param>
        /// <returns>True if the validation succeeds, false otherwise.</returns>
        public bool ValidateUpdate(byte[] updateValidation)
        {
            if (m_updateHandle == -1 || !m_authenticated) throw new InvalidOperationException();

            return MFNativeUpdate.Validate(m_updateHandle, updateValidation);
        }

        /// <summary>
        /// Validates and installs the update.  Because the install may reboot the device, a 
        /// caller may want to call ValidateUpdate first in order to have confidence the install
        /// will succeed.
        /// </summary>
        /// <param name="updateValidation">The validation data for the entire update package.</param>
        /// <returns>true if successful, false otherwise.  This method may not return if the 
        /// installer requires a reboot.</returns>
        public bool InstallUpdate(byte[] updateValidation)
        {
            if (m_updateHandle == -1 || !m_authenticated) throw new InvalidOperationException();

            return MFNativeUpdate.Install(m_updateHandle, updateValidation);
        }
    }
}
