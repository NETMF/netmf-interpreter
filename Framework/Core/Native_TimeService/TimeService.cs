////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.SPOT.Time
{
    /// <summary>
    /// Settings that control Time Service.
    /// </summary>
    public class TimeServiceSettings
    {
        private const uint c_DefautRefreshTime = 5 * 60 * 1000; // 5 minutes
        
        /// <summary>
        /// Primary SNTP Server IP address.
        /// </summary>
        protected uint PrimaryServerIP;

        /// <summary>
        /// Alternate server is used if communication with primary server failed.
        /// </summary>
        protected uint AlternateServerIP;

        /// <summary>
        /// Specifies the period, in seconds, between synchronizations with the SNTP server.
        /// </summary>
        public uint RefreshTime = c_DefautRefreshTime;

        /// <summary>
        /// Specifies the interval, in milliseconds, between the time on the SNTP server and the current time. If the difference between the SNTP server time and the local system time is larger,
        /// the update is ignored unless the system clock is presumed incorrect.
        /// </summary>
        public uint Tolerance;

        /// <summary>
        /// Syncs with SNTP server, after wakeup or cold boot.
        /// </summary>
        public bool ForceSyncAtWakeUp;

        /// <summary>
        /// System automatically updates for daylight savings.
        /// </summary>
        public bool AutoDayLightSavings;

        /// <summary>
        /// IP (in x.x.x.x form) of primary SNTP server.
        /// </summary>
        public byte[] PrimaryServer
        {
            get
            {
                return ToIPFragments(PrimaryServerIP);
            }

            set
            {
                PrimaryServerIP = FromIPFragments(value);
            }
        }

        /// <summary>
        /// IP (in x.x.x.x form) of alternate SNTP server.
        /// </summary>
        public byte[] AlternateServer
        {
            get
            {
                return ToIPFragments(AlternateServerIP);
            }

            set
            {
                AlternateServerIP = FromIPFragments(value);
            }
        }

        internal static byte[] ToIPFragments(uint ip)
        {
            byte[] ipFragments = new byte[4];
            ipFragments[0] = (byte)((ip >> 24) & 0xFF);
            ipFragments[1] = (byte)((ip >> 16) & 0xFF);
            ipFragments[2] = (byte)((ip >> 8) & 0xFF);
            ipFragments[3] = (byte)(ip & 0xFF);

            return ipFragments;
        }

        internal static uint FromIPFragments(byte[] fragments)
        {
            if (fragments == null)
                throw new ArgumentException("Fragment array cannot be null");

            if (fragments.Length != 4)
                throw new ArgumentException("Fragment array must of size four");

            uint ip = 0;
            int i = 0;
            for (i = 0; i < fragments.Length; i++)
            {
                ip <<= 8;
                ip |= fragments[i];
            }

            return ip;
        }
    }

    /// <summary>
    /// Status representing a Time service method call.
    /// </summary>
    public class TimeServiceStatus
    {
        /// <summary>
        /// Sync status.
        /// </summary>
        [Flags]
        public enum TimeServiceStatusFlags
        {
            SyncSucceeded = 0x0,
            SyncFailed = 0x1,
        }

        /// <summary>
        /// Flags value will be a combination of TimeServiceStatusFlags.
        /// </summary>
        public TimeServiceStatusFlags Flags;

        /// <summary>
        /// The server IP that was used for sync.
        /// </summary>
        public uint SyncSourceServer;

        /// <summary>
        /// Offset in milliseconds last time when successful sync happened.
        /// 0xFFFFFFFF if sync time unknown.
        /// </summary>
        public long SyncTimeOffset;

        /// <summary>
        /// Current time.
        /// </summary>
        protected long TimeUTC;

        /// <summary>
        /// UTC time when this method was called. This is just a handy way
        /// to get updated UTC time, after SNTP sync. Saving additional method call.
        /// </summary>
        public DateTime CurrentTimeUTC
        {
            get
            {
                return new DateTime(TimeUTC);
            }
        }
    }

    public class SystemTimeChangedEventArgs : EventArgs
    {
        public SystemTimeChangedEventArgs(DateTime eventTime)
        {
            EventTime = eventTime;
        }

        public readonly DateTime EventTime;
    }

    public class TimeSyncFailedEventArgs : EventArgs
    {
        public TimeSyncFailedEventArgs(DateTime eventTime, uint errorCode)
        {
            EventTime = eventTime;
            ErrorCode = errorCode;
        }

        public readonly DateTime EventTime;
        public readonly uint ErrorCode;
    }

    public delegate void SystemTimeChangedEventHandler(Object sender, SystemTimeChangedEventArgs e);
    public delegate void TimeSyncFailedEventHandler(Object sender, TimeSyncFailedEventArgs e);

    /// <summary>
    /// Static class that controls TimeService.
    /// </summary>
    public static class TimeService
    {
        [Flags]
        internal enum TimeServiceEventType : byte
        {
            SystemTimeChanged = 0x1,
            TimeSyncFailed = 0x2,
        }

        internal class TimeServiceEvent : BaseEvent
        {
            public TimeServiceEventType EventType;
            public DateTime EventTime;
            public uint Status;
        }

        internal class TimeServiceChangeListener : IEventListener, IEventProcessor
        {
            public void InitializeForEventSource()
            {
            }

            public BaseEvent ProcessEvent(uint data1, uint data2, DateTime time)
            {
                TimeServiceEvent timeServiceEvent = new TimeServiceEvent();
                timeServiceEvent.EventType = (TimeServiceEventType)(data1 & 0xFF);
                timeServiceEvent.EventTime = time;
                timeServiceEvent.Status = data2;

                return timeServiceEvent;
            }

            public bool OnEvent(BaseEvent ev)
            {
                if (ev is TimeServiceEvent)
                {
                    TimeService.OnTimeServiceEventCallback((TimeServiceEvent)ev);
                }

                return true;
            }
        }

        static TimeService()
        {
            TimeServiceChangeListener TimeServiceChangeListener = new TimeServiceChangeListener();
            Microsoft.SPOT.EventSink.AddEventProcessor(EventCategory.TimeService, TimeServiceChangeListener);
            Microsoft.SPOT.EventSink.AddEventListener(EventCategory.TimeService, TimeServiceChangeListener);
        }

        public static event SystemTimeChangedEventHandler SystemTimeChanged;
        public static event TimeSyncFailedEventHandler TimeSyncFailed;

        internal static void OnTimeServiceEventCallback(TimeServiceEvent timeServiceEvent)
        {
            switch (timeServiceEvent.EventType)
            {
                case TimeServiceEventType.SystemTimeChanged:
                    {
                        if (SystemTimeChanged != null)
                        {
                            SystemTimeChangedEventArgs args = new SystemTimeChangedEventArgs(timeServiceEvent.EventTime);

                            SystemTimeChanged(null, args);
                        }
                        break;
                    }
                case TimeServiceEventType.TimeSyncFailed:
                    {
                        if (TimeSyncFailed != null)
                        {
                            TimeSyncFailedEventArgs args = new TimeSyncFailedEventArgs(timeServiceEvent.EventTime, timeServiceEvent.Status);

                            TimeSyncFailed(null, args);
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        /// <summary>
        /// Get or set TimeServiceSettings.
        /// </summary>
        public static TimeServiceSettings Settings
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get
            {
                return null;
            }

            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            set
            {

            }
        }

        /// <summary>
        /// Starts scheduled time synchronization service. For periodic refresh it uses previously set refreshtime.
        /// Refresh time is updateable dynamically, and effective immediately.
        /// </summary>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void Start();

        /// <summary>
        /// Stops periodic time synchronization service. Timeservice APIs are still available, may return stale data
        /// unless manual sync is performed.
        /// </summary>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void Stop();

        /// <summary>
        /// Returns latest sync status, it may be scheduled or forced sync, whichever occured last. This
        /// can be verified from the TimeService_Status.Flags field. Optionally this will also
        /// return the latest UTC time.
        /// </summary>
        public static TimeServiceStatus LastSyncStatus
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Manual update of system time value from a given server. This can be called orthogonally along with
        /// scheduled time service.
        /// </summary>
        /// <param name="serverAddress"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static TimeServiceStatus UpdateNow(byte[] serverAddress, uint tolerance)
        {
            uint serverIP = TimeServiceSettings.FromIPFragments(serverAddress);

            return Update(serverIP, tolerance);
        }

        /// <summary>
        /// Immediately syncs with time servers using Primary and Alternate server IP that has already
        /// been set via settings.
        /// </summary>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static TimeServiceStatus UpdateNow(uint tolerance)
        {
            return Update(0, tolerance);
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern TimeServiceStatus Update(uint serverAddress, uint tolerance);

        /// <summary>
        /// Set UTC time of the system. Change is reflected right away.
        /// </summary>
        /// <param name="utcTimeInTicks"></param>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void SetUtcTime(long utcTimeInTicks);

        /// <summary>
        /// Sets time zone of the system. Effective immediately.
        /// </summary>
        /// <param name="offsetInMinutes"></param>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void SetTimeZoneOffset(int offsetInMinutes);
    }
}

namespace System
{
    public static partial class Environment
    {
        /// <summary>
        /// Returns machines tick count since last reboot in milliseconds.
        /// </summary>
        public static int TickCount
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get
            {
                return 0;
            }
        }
    }
}


