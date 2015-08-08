////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////namespace System
namespace System
{

    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Globalization;

    // Summary:
    //     Specifies whether a System.DateTime object represents a local time, a Coordinated
    //     Universal Time (UTC), or is not specified as either local time or UTC.
    [Serializable]
    public enum DateTimeKind
    {
        // Summary:
        //     The time represented is not specified as either local time or Coordinated
        //     Universal Time (UTC).
        //     MF does not support Unspecified type. Constructor for DateTime always creates local time.
        //     Use SpecifyKind to set Kind property to UTC or ToUniversalTime to convert local to UTC
        //Unspecified = 0,
        //
        // Summary:
        //     The time represented is UTC.
        Utc = 1,
        //
        // Summary:
        //     The time represented is local time.
        Local = 2,
    }

    /**
     * This value type represents a date and time.  Every DateTime
     * object has a private field (Ticks) of type Int64 that stores the
     * date and time as the number of 100 nanosecond intervals since
     * 12:00 AM January 1, year 1601 A.D. in the proleptic Gregorian Calendar.
     *
     * <p>For a description of various calendar issues, look at
     * <a href="http://serendipity.nofadz.com/hermetic/cal_stud.htm">
     * Calendar Studies web site</a>, at
     * http://serendipity.nofadz.com/hermetic/cal_stud.htm.
     *
     * <p>
     * <h2>Warning about 2 digit years</h2>
     * <p>As a temporary hack until we get new DateTime &lt;-&gt; String code,
     * some systems won't be able to round trip dates less than 1930.  This
     * is because we're using OleAut's string parsing routines, which look
     * at your computer's default short date string format, which uses 2 digit
     * years on most computers.  To fix this, go to Control Panel -&gt; Regional
     * Settings -&gt; Date and change the short date style to something like
     * "M/d/yyyy" (specifying four digits for the year).
     *
     */
    [Serializable()]
    public struct DateTime
    {
        // Number of 100ns ticks per time unit
        private const long TicksPerMillisecond = 10000;
        private const long TicksPerSecond = TicksPerMillisecond * 1000;
        private const long TicksPerMinute = TicksPerSecond * 60;
        private const long TicksPerHour = TicksPerMinute * 60;
        private const long TicksPerDay = TicksPerHour * 24;

        // Number of milliseconds per time unit
        private const int MillisPerSecond = 1000;
        private const int MillisPerMinute = MillisPerSecond * 60;
        private const int MillisPerHour = MillisPerMinute * 60;
        private const int MillisPerDay = MillisPerHour * 24;

        // Number of days in a non-leap year
        private const int DaysPerYear = 365;
        // Number of days in 4 years
        private const int DaysPer4Years = DaysPerYear * 4 + 1;
        // Number of days in 100 years
        private const int DaysPer100Years = DaysPer4Years * 25 - 1;
        // Number of days in 400 years
        private const int DaysPer400Years = DaysPer100Years * 4 + 1;

        // Number of days from 1/1/0001 to 12/31/1600
        private const int DaysTo1601 = DaysPer400Years * 4;
        // Number of days from 1/1/0001 to 12/30/1899
        private const int DaysTo1899 = DaysPer400Years * 4 + DaysPer100Years * 3 - 367;
        // Number of days from 1/1/0001 to 12/31/9999
        private const int DaysTo10000 = DaysPer400Years * 25 - 366;

        private const long MinTicks = 0;
        private const long MaxTicks = 441796895990000000;
        private const long MaxMillis = (long)DaysTo10000 * MillisPerDay;

        // This is mask to extract ticks from m_ticks
        private const ulong TickMask = 0x7FFFFFFFFFFFFFFFL;
        private const ulong UTCMask = 0x8000000000000000L;

        public static readonly DateTime MinValue = new DateTime(MinTicks);
        public static readonly DateTime MaxValue = new DateTime(MaxTicks);

        private ulong m_ticks;

        public DateTime(long ticks)
        {
            if (((ticks & (long)TickMask) < MinTicks) || ((ticks & (long)TickMask) > MaxTicks))
            {
                throw new ArgumentOutOfRangeException("ticks", "Ticks must be between DateTime.MinValue.Ticks and DateTime.MaxValue.Ticks.");
            }

            m_ticks = (ulong)ticks;
        }

        public DateTime(long ticks, DateTimeKind kind)
            : this(ticks)
        {
            if (kind == DateTimeKind.Local)
            {
                m_ticks &= ~UTCMask;
            }
            else
            {
                m_ticks |= UTCMask;
            }
        }

        public DateTime(int year, int month, int day)
            : this(year, month, day, 0, 0, 0)
        {
        }

        public DateTime(int year, int month, int day, int hour, int minute, int second)
            : this(year, month, day, hour, minute, second, 0)
        {
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern DateTime(int year, int month, int day, int hour, int minute, int second, int millisecond);

        public DateTime Add(TimeSpan val)
        {
            return new DateTime((long)m_ticks + val.Ticks);
        }

        private DateTime Add(double val, int scale)
        {
            return new DateTime((long)((long)m_ticks + (long)(val * scale * TicksPerMillisecond + (val >= 0 ? 0.5 : -0.5))));
        }

        public DateTime AddDays(double val)
        {
            return Add(val, MillisPerDay);
        }

        public DateTime AddHours(double val)
        {
            return Add(val, MillisPerHour);
        }

        public DateTime AddMilliseconds(double val)
        {
            return Add(val, 1);
        }

        public DateTime AddMinutes(double val)
        {
            return Add(val, MillisPerMinute);
        }

        public DateTime AddSeconds(double val)
        {
            return Add(val, MillisPerSecond);
        }

        public DateTime AddTicks(long val)
        {
            return new DateTime((long)m_ticks + val);
        }

        public static int Compare(DateTime t1, DateTime t2)
        {
            // Get ticks, clear UTC mask
            ulong t1_ticks = t1.m_ticks & TickMask;
            ulong t2_ticks = t2.m_ticks & TickMask;

            // Compare ticks, ignore the Kind property.
            if (t1_ticks > t2_ticks)
            {
                return 1;
            }

            if (t1_ticks < t2_ticks)
            {
                return -1;
            }

            // Values are equal
            return 0;
        }

        public int CompareTo(Object val)
        {
            if (val == null) return 1;

            return DateTime.Compare(this, (DateTime)val);
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern static int DaysInMonth(int year, int month);

        public override bool Equals(Object val)
        {
            if (val is DateTime)
            {
                // Call compare for proper comparison of 2 DateTime objects
                // Since DateTime is optimized value and internally represented by int64
                // "this" may still have type int64.
                // Convertion to object and back is a workaround.
                object o = this;
                DateTime thisTime = (DateTime)o;
                return Compare(thisTime, (DateTime)val) == 0;
            }

            return false;
        }

        public static bool Equals(DateTime t1, DateTime t2)
        {
            return Compare(t1, t2) == 0;
        }

        public DateTime Date
        {
            get
            {
                // Need to remove UTC mask before arithmetic operations. Then set it back.
                if ((m_ticks & UTCMask) != 0)
                {
                    return new DateTime((long)(((m_ticks & TickMask) - (m_ticks & TickMask) % TicksPerDay) | UTCMask));
                }
                else
                {
                    return new DateTime((long)(m_ticks - m_ticks % TicksPerDay));
                }
            }
        }

        public int Day
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get
            {
                return 0;
            }
        }

        public DayOfWeek DayOfWeek
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get
            {
                return DayOfWeek.Monday;
            }
        }

        public int DayOfYear
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get
            {
                return 0;
            }
        }

        /// Reduce size by calling a single method?
        public int Hour
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get
            {
                return 0;
            }
        }

        public DateTimeKind Kind
        {
            get
            {
                // If mask for UTC time is set - return UTC. If no maskk - return local.
                return (m_ticks & UTCMask) != 0 ? DateTimeKind.Utc : DateTimeKind.Local;
            }

        }

        public static DateTime SpecifyKind(DateTime value, DateTimeKind kind)
        {
            DateTime retVal = new DateTime((long)value.m_ticks);

            if (kind == DateTimeKind.Utc)
            {
                // Set UTC mask
                retVal.m_ticks = value.m_ticks | UTCMask;
            }
            else
            {   // Clear UTC mask
                retVal.m_ticks = value.m_ticks & ~UTCMask;
            }

            return retVal;
        }

        public int Millisecond
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get
            {
                return 0;
            }
        }

        public int Minute
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get
            {
                return 0;
            }
        }

        public int Month
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get
            {
                return 0;
            }
        }

        public static DateTime Now
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get
            {
                return new DateTime();
            }
        }

        public static DateTime UtcNow
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get
            {
                return new DateTime();
            }
        }

        public int Second
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get
            {
                return 0;
            }
        }

        /// Our origin is at 1601/01/01:00:00:00.000
        /// While desktop CLR's origin is at 0001/01/01:00:00:00.000.
        /// There are 504911232000000000 ticks between them which we are subtracting.
        /// See DeviceCode\PAL\time_decl.h for explanation of why we are taking
        /// year 1601 as origin for our HAL, PAL, and CLR.
        // static Int64 ticksAtOrigin = 504911232000000000;
        static Int64 ticksAtOrigin = 0;
        public long Ticks
        {
            get
            {
                return (long)(m_ticks & TickMask) + ticksAtOrigin;
            }
        }

        public TimeSpan TimeOfDay
        {
            get
            {
                return new TimeSpan((long)((m_ticks & TickMask) % TicksPerDay));
            }
        }

        public static DateTime Today
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get
            {
                return new DateTime();
            }
        }

        public int Year
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get
            {
                return 0;
            }
        }

        public TimeSpan Subtract(DateTime val)
        {
            return new TimeSpan((long)(m_ticks & TickMask) - (long)(val.m_ticks & TickMask));
        }

        public DateTime Subtract(TimeSpan val)
        {
            return new DateTime((long)(m_ticks - (ulong)val.m_ticks));
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern DateTime ToLocalTime();

        public override String ToString()
        {
            return DateTimeFormat.Format(this, null, DateTimeFormatInfo.CurrentInfo);
        }

        public String ToString(String format)
        {
            return DateTimeFormat.Format(this, format, DateTimeFormatInfo.CurrentInfo);
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern DateTime ToUniversalTime();

        public static DateTime operator +(DateTime d, TimeSpan t)
        {
            return new DateTime((long)(d.m_ticks + (ulong)t.m_ticks));
        }

        public static DateTime operator -(DateTime d, TimeSpan t)
        {
            return new DateTime((long)(d.m_ticks - (ulong)t.m_ticks));
        }

        public static TimeSpan operator -(DateTime d1, DateTime d2)
        {
            return d1.Subtract(d2);
        }

        public static bool operator ==(DateTime d1, DateTime d2)
        {
            return Compare(d1, d2) == 0;
        }

        public static bool operator !=(DateTime t1, DateTime t2)
        {
            return Compare(t1, t2) != 0;
        }

        public static bool operator <(DateTime t1, DateTime t2)
        {
            return Compare(t1, t2) < 0;
        }

        public static bool operator <=(DateTime t1, DateTime t2)
        {
            return Compare(t1, t2) <= 0;
        }

        public static bool operator >(DateTime t1, DateTime t2)
        {
            return Compare(t1, t2) > 0;
        }

        public static bool operator >=(DateTime t1, DateTime t2)
        {
            return Compare(t1, t2) >= 0;
        }
    }
}


