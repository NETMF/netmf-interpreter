////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace Microsoft.SPOT
{
    [Serializable]
    public enum TimeZoneId
    {
        Current = 0,
        Network = 1,

        Dateline,
        Samoa,
        Hawaii,
        Alaska,
        Pacific,
        Arizona,
        Mountain,
        CentAmerica,
        Central,
        Saskatchewan,
        MexicoCity,
        Indiana,
        Bogota,
        Eastern,
        Caracas,
        Santiago,
        Atlantic,
        Newfoundland,
        Brasilia,
        Greenland,
        BuenosAires,
        MidAtlantic,
        CapeVerde,
        Azores,
        Casablanca,
        GMT,
        London,
        WCentAfrica,
        Prague,
        Warsaw,
        Paris,
        Berlin,
        Cairo,
        Pretoria,
        Bucharest,
        Helsinki,
        Athens,
        Riyadh,
        Nairobi,
        Moscow,
        Baghdad,
        Tehran,
        Baku,
        Kabul,
        AbuDhabi,
        Yekaterinburg,
        Islamabad,
        NewDelhi,
        Kathmandu,
        Astana,
        SriLanka,
        Almaty,
        Yangon,
        Bangkok,
        Krasnoyarsk,
        Beijing,
        Malaysia,
        Taipei,
        Perth,
        Ulaanbataar,
        Seoul,
        Tokyo,
        Yakutsk,
        Darwin,
        Adelaide,
        Sydney,
        Brisbane,
        Hobart,
        Guam,
        Vladivostok,
        Magadan,
        FijiIslands,
        NewZealand,
        Tonga,

        FIRST = Network,
        LAST = Tonga,
        COUNT = (Tonga - Network + 1),

        FIRST_RESOURCE = Dateline,
    }

    public class SystemTime
    {
        public short Year;
        public short Month;
        public short DayOfWeek;
        public short Day;
        public short Hour;
        public short Minute;
        public short Second;
        public short Milliseconds;
    }

    public class TimeZoneInformation
    {
        public int Bias;
        public string StandardName;
        public SystemTime StandardDate;
        public int StandardBias;
        public string DaylightName;
        public SystemTime DaylightDate;
        public int DaylightBias;
    }

    //--//

    public sealed class ExtendedTimer : IDisposable
    {
        public enum TimeEvents
        {
            Second,
            Minute,
            Hour,
            Day,
            TimeZone,
            SetTime,
        }

        [Microsoft.SPOT.FieldNoReflection]
        private object m_timer;
        private object m_state;
        private System.Threading.TimerCallback m_callback;

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public ExtendedTimer(System.Threading.TimerCallback callback, object state, int dueTime, int period);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public ExtendedTimer(System.Threading.TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public ExtendedTimer(System.Threading.TimerCallback callback, object state, DateTime dueTime, TimeSpan period);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public ExtendedTimer(System.Threading.TimerCallback callback, object state, TimeEvents ev);
        //--//

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public void Dispose();
        //--//

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public void Change(int dueTime, int period);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public void Change(TimeSpan dueTime, TimeSpan period);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public void Change(DateTime dueTime, TimeSpan period);
        //--//
        extern public TimeSpan LastExpiration
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }
    }
}


