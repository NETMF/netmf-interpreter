////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////namespace System
namespace System
{
    using System;
    using System.Runtime.CompilerServices;

    /**
     * TimeSpan represents a duration of time.  A TimeSpan can be negative
     * or positive.</p>
     *
     * <p>TimeSpan is internally represented as a number of milliseconds.  While
     * this maps well into units of time such as hours and days, any
     * periods longer than that aren't representable in a nice fashion.
     * For instance, a month can be between 28 and 31 days, while a year
     * can contain 365 or 364 days.  A decade can have between 1 and 3 leapyears,
     * depending on when you map the TimeSpan into the calendar.  This is why
     * we do not provide Years() or Months().</p>
     *
     * @see System.DateTime
     */
    [Serializable]
    public struct TimeSpan
    {
        internal long m_ticks;

        public const long TicksPerMillisecond = 10000;
        public const long TicksPerSecond = TicksPerMillisecond * 1000;
        public const long TicksPerMinute = TicksPerSecond * 60;
        public const long TicksPerHour = TicksPerMinute * 60;
        public const long TicksPerDay = TicksPerHour * 24;

        public static readonly TimeSpan Zero = new TimeSpan(0);

        public static readonly TimeSpan MaxValue = new TimeSpan(Int64.MaxValue);
        public static readonly TimeSpan MinValue = new TimeSpan(Int64.MinValue);

        public TimeSpan(long ticks)
        {
            m_ticks = ticks;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public TimeSpan(int hours, int minutes, int seconds);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public TimeSpan(int days, int hours, int minutes, int seconds);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public TimeSpan(int days, int hours, int minutes, int seconds, int milliseconds);

        public long Ticks
        {
            get
            {
                return m_ticks;
            }
        }

        public int Days
        {
            get
            {
                return (int)(m_ticks / TicksPerDay);
            }
        }

        public int Hours
        {
            get
            {
                return (int)((m_ticks / TicksPerHour) % 24);
            }
        }

        public int Milliseconds
        {
            get
            {
                return (int)((m_ticks / TicksPerMillisecond) % 1000);
            }
        }

        public int Minutes
        {
            get
            {
                return (int)((m_ticks / TicksPerMinute) % 60);
            }
        }

        public int Seconds
        {
            get
            {
                return (int)((m_ticks / TicksPerSecond) % 60);
            }
        }

        public TimeSpan Add(TimeSpan ts)
        {
            return new TimeSpan(m_ticks + ts.m_ticks);
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public static int Compare(TimeSpan t1, TimeSpan t2);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public int CompareTo(Object value);

        public TimeSpan Duration()
        {
            return new TimeSpan(m_ticks >= 0 ? m_ticks : -m_ticks);
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public override bool Equals(Object value);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public static bool Equals(TimeSpan t1, TimeSpan t2);

        public TimeSpan Negate()
        {
            return new TimeSpan(-m_ticks);
        }

        public TimeSpan Subtract(TimeSpan ts)
        {
            return new TimeSpan(m_ticks - ts.m_ticks);
        }

        public static TimeSpan FromTicks(long val)
        {
            return new TimeSpan(val);
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public override String ToString();

        public static TimeSpan operator -(TimeSpan t)
        {
            return new TimeSpan(-t.m_ticks);
        }

        public static TimeSpan operator -(TimeSpan t1, TimeSpan t2)
        {
            return new TimeSpan(t1.m_ticks - t2.m_ticks);
        }

        public static TimeSpan operator +(TimeSpan t)
        {
            return t;
        }

        public static TimeSpan operator +(TimeSpan t1, TimeSpan t2)
        {
            return new TimeSpan(t1.m_ticks + t2.m_ticks);
        }

        public static bool operator ==(TimeSpan t1, TimeSpan t2)
        {
            return t1.m_ticks == t2.m_ticks;
        }

        public static bool operator !=(TimeSpan t1, TimeSpan t2)
        {
            return t1.m_ticks != t2.m_ticks;
        }

        public static bool operator <(TimeSpan t1, TimeSpan t2)
        {
            return t1.m_ticks < t2.m_ticks;
        }

        public static bool operator <=(TimeSpan t1, TimeSpan t2)
        {
            return t1.m_ticks <= t2.m_ticks;
        }

        public static bool operator >(TimeSpan t1, TimeSpan t2)
        {
            return t1.m_ticks > t2.m_ticks;
        }

        public static bool operator >=(TimeSpan t1, TimeSpan t2)
        {
            return t1.m_ticks >= t2.m_ticks;
        }

    }
}


