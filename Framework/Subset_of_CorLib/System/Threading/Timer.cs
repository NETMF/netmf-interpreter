////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System.Threading
{
    using System.Threading;
    using System;
    using System.Runtime.CompilerServices;

    public delegate void TimerCallback(Object state);

    public sealed class Timer : MarshalByRefObject, IDisposable
    {
        [System.Reflection.FieldNoReflection]
        private object m_timer;
        private object m_state;
        private TimerCallback m_callback;
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public Timer(TimerCallback callback, Object state, int dueTime, int period);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public Timer(TimerCallback callback, Object state, TimeSpan dueTime, TimeSpan period);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public bool Change(int dueTime, int period);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public bool Change(TimeSpan dueTime, TimeSpan period);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public void Dispose();
    }
}


