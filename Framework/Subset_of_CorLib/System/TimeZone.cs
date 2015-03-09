////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////namespace System
namespace System
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Globalization;

    [Serializable]
    public abstract class TimeZone
    {
        internal int m_id;

        protected TimeZone() { }

        public static TimeZone CurrentTimeZone
        {
            get
            {
                return new CurrentSystemTimeZone(GetTimeZoneOffset());
            }
        }

        public abstract String StandardName
        {
            get;
        }

        public abstract String DaylightName
        {
            get;
        }

        public abstract TimeSpan GetUtcOffset(DateTime time);

        public virtual DateTime ToUniversalTime(DateTime time)
        {
            if (time.Kind == DateTimeKind.Utc)
                return time;

            return new DateTime(time.Ticks - GetTimeZoneOffset(), DateTimeKind.Utc);
        }

        public virtual DateTime ToLocalTime(DateTime time)
        {
            if (time.Kind == DateTimeKind.Local)
                return time;

            return new DateTime(time.Ticks + GetTimeZoneOffset(), DateTimeKind.Local);
        }

        public abstract DaylightTime GetDaylightChanges(int year);

        public virtual bool IsDaylightSavingTime(DateTime time)
        {
            return false;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static long GetTimeZoneOffset();
    }

    [Serializable]
    internal class CurrentSystemTimeZone : TimeZone
    {
        protected long m_ticksOffset = 0;

        internal CurrentSystemTimeZone()
        {
        }

        internal CurrentSystemTimeZone(long ticksOffset)
        {
            m_ticksOffset = ticksOffset;
        }

        public override String StandardName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override String DaylightName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override DaylightTime GetDaylightChanges(int year)
        {
            throw new NotImplementedException();
        }

        public override TimeSpan GetUtcOffset(DateTime time)
        {
            if (time.Kind == DateTimeKind.Utc)
                return TimeSpan.Zero;

            return new TimeSpan(m_ticksOffset);
        }
    }
}


