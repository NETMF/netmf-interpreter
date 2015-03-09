using System;
using System.Runtime.CompilerServices;
using Microsoft.SPOT.Hardware;

namespace Microsoft.SPOT.Cryptoki
{
    /// <summary>
    /// Defines the Cryptoki slot object.
    /// </summary>
    public class Slot
    {
        /// <summary>
        /// Defines the slot event type.
        /// </summary>
        public enum SlotEventType
        {
            Removed=0,
            Inserted=1,
        }

        //public enum SessionEventType
        //{
        //    Surrender,
        //}

        //public enum SessionNotifyResult
        //{
        //    OK,
        //    Cancel,
        //}

        public delegate void OnSlotEvent(SlotEventType evt);
        //public delegate SessionNotifyResult OnSessionNotify(SessionEventType evt, object arg);

        /// <summary>
        /// Defines Cryptoki slot properties.
        /// </summary>
        [FlagsAttribute]
        public enum SlotFlag
        {
            TokenPresent    = 0x0001,
            RemovableDevice = 0x0002,
            HardwareSlot    = 0x0004,
        }

        /// <summary>
        /// Defines Cryptoki slot information.
        /// </summary>
        public class SlotInfo
        {
            public readonly string          Description;
            public readonly string          ManufactureID;
            public readonly SlotFlag        Flags;
            public readonly CryptokiVersion HardwareVersion = new CryptokiVersion();
            public readonly CryptokiVersion FirmwareVersion = new CryptokiVersion();
        }

        private OnSlotEvent           m_slotEvent;
        private int                   m_slotIndex;
        private NativeEventDispatcher m_evtDispatcher;
        private bool                  m_disposed;
        private SlotInfo              m_slotInfo;

        private void SlotEventHandler(uint evt, uint data2, DateTime timestamp)
        {
            if (m_slotEvent != null)
            {
                m_slotEvent((SlotEventType)evt);
            }
        }

        /// <summary>
        /// Gets the Cryptoki slot information.
        /// </summary>
        public SlotInfo Info
        {
            get
            {
                if (m_slotInfo == null)
                {
                    m_slotInfo = new SlotInfo();

                    GetSlotInfoInternal(m_slotInfo);
                }

                return m_slotInfo;
            }
        }

        /// <summary>
        /// Adds or removes a slot event handler.
        /// </summary>
        public event OnSlotEvent SlotEvent
        {
            [MethodImplAttribute(MethodImplOptions.Synchronized)]
            add
            {
                if (m_disposed)
                {
                    throw new ObjectDisposedException();
                }

                if (m_evtDispatcher == null)
                {
                    m_evtDispatcher = new NativeEventDispatcher("CryptokiSlotEvent", (ulong)m_slotIndex);
                }

                OnSlotEvent callbacksOld = m_slotEvent;
                OnSlotEvent callbacksNew = (OnSlotEvent)Delegate.Combine(callbacksOld, value);

                try
                {
                    m_slotEvent = callbacksNew;

                    if (callbacksOld == null && m_slotEvent != null)
                    {
                        m_evtDispatcher.OnInterrupt += new NativeEventHandler(SlotEventHandler);
                    }
                }
                catch
                {
                    m_slotEvent = callbacksOld;

                    throw;
                }
            }

            [MethodImplAttribute(MethodImplOptions.Synchronized)]
            remove
            {
                if (m_disposed)
                {
                    throw new ObjectDisposedException();
                }

                OnSlotEvent callbacksOld = m_slotEvent;
                OnSlotEvent callbacksNew = (OnSlotEvent)Delegate.Remove(callbacksOld, value);

                try
                {
                    m_slotEvent = callbacksNew;

                    if (m_slotEvent == null)
                    {
                        m_evtDispatcher.OnInterrupt -= new NativeEventHandler(SlotEventHandler);
                        m_evtDispatcher.Dispose();
                        m_evtDispatcher = null;
                    }
                }
                catch
                {
                    m_slotEvent = callbacksOld;

                    throw;
                }
            }
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern void GetSlotInfoInternal(SlotInfo info);

        /// <summary>
        /// Gets the Cryptoki token information.
        /// </summary>
        /// <param name="info">Token information to be loaded.</param>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern void GetTokenInfo(ref TokenInfo info);

        /// <summary>
        /// Gets the supported mechanism types for the slot.
        /// </summary>
        public extern MechanismType[] SupportedMechanisms
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        /// <summary>
        /// Gets the mechanism information for the specified mechanism type.
        /// </summary>
        /// <param name="type">The target mechanism type</param>
        /// <param name="info">The mechanism information to be loaded.</param>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern void GetMechanismInfo(MechanismType type, ref MechanismInfo info);

        /// <summary>
        /// Opens a session on the slot with the specified flags.
        /// </summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern Session OpenSession(Session.SessionFlag flags);

        /// <summary>
        /// Initializes the token in the slot.
        /// </summary>
        /// <param name="pin">Pin value.</param>
        /// <param name="label">Token label.</param>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern void InitializeToken(string pin, string label);
    }

    /// <summary>
    /// Defines the Cryptoki token information.
    /// </summary>
    public class TokenInfo
    {
        /// <summary>
        /// Defines token propeties.
        /// </summary>
        [FlagsAttribute]
        public enum TokenFlag
        {
            RandomNumberGenerator       = 0x00000001,
            WriteProtected              = 0x00000002,
            LoginRequired               = 0x00000004,
            UserPinInitialized          = 0x00000008,
            RestoreKeyNotNeeded         = 0x00000020,
            ClockOnToken                = 0x00000040,
            ProtectedAuthenticationPath = 0x00000100,
            DualCryptoOperations        = 0x00000200,
            TokenInitialized            = 0x00000400,
            SecondaryAuthentication     = 0x00000800,
            UserPinCountLow             = 0x00010000,
            UserPinFinalTry             = 0x00020000,
            UserPinLocked               = 0x00040000,
            UserPinToBeChanged          = 0x00080000,
            SOPinCountLow               = 0x00100000,
            SOPinFinalTry               = 0x00200000,
            SOPinLocked                 = 0x00400000,
            SOPinToBeChanged            = 0x00800000,
            ErrorState                  = 0x01000000,
        }

        public readonly string Label;
        public readonly string Manufacturer;
        public readonly string Model;
        public readonly string SerialNumber;
        public readonly TokenFlag Flags;
        public readonly ulong MaxSessionCount;
        public readonly ulong SessionCount;
        public readonly ulong MaxRwSessionCount;
        public readonly ulong MaxPinLen;
        public readonly ulong MinPinLen;
        public readonly ulong TotalPublicMemory;
        public readonly ulong FreePublicMemory;
        public readonly ulong TotalPrivateMemory;
        public readonly ulong FreePrivateMemory;
        public readonly CryptokiVersion HardwareVersion = new CryptokiVersion();
        public readonly CryptokiVersion FirmwareVersion = new CryptokiVersion();
        private string m_UtcTimeString;

        public DateTime UtcTime
        {
            get
            {
                DateTime ret = new DateTime();
                
                // TODO: Add parsing here

                return ret;
            }
        }
    }
}