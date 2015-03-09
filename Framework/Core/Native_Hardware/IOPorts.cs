using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Collections;

namespace Microsoft.SPOT.Hardware
{

    public delegate void NativeEventHandler(uint data1, uint data2, DateTime time);

    //--//

    public class NativeEventDispatcher : IDisposable
    {
        protected NativeEventHandler m_threadSpawn = null;
        protected NativeEventHandler m_callbacks = null;
        protected bool m_disposed = false;
        private object m_NativeEventDispatcher;

        //--//

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public NativeEventDispatcher(string strDriverName, ulong drvData);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public virtual void EnableInterrupt();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public virtual void DisableInterrupt();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern protected virtual void Dispose(bool disposing);

        //--//

        ~NativeEventDispatcher()
        {
            Dispose(false);
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public virtual void Dispose()
        {
            if (!m_disposed)
            {
                Dispose(true);

                GC.SuppressFinalize(this);

                m_disposed = true;
            }
        }

        public event NativeEventHandler OnInterrupt
        {
            [MethodImplAttribute(MethodImplOptions.Synchronized)]
            add
            {
                if (m_disposed)
                {
                    throw new ObjectDisposedException();
                }

                NativeEventHandler callbacksOld = m_callbacks;
                NativeEventHandler callbacksNew = (NativeEventHandler)Delegate.Combine(callbacksOld, value);

                try
                {
                    m_callbacks = callbacksNew;

                    if (callbacksNew != null)
                    {
                        if (callbacksOld == null)
                        {
                            EnableInterrupt();
                        }

                        if (callbacksNew.Equals(value) == false)
                        {
                            callbacksNew = new NativeEventHandler(this.MultiCastCase);
                        }
                    }

                    m_threadSpawn = callbacksNew;
                }
                catch
                {
                    m_callbacks = callbacksOld;

                    if (callbacksOld == null)
                    {
                        DisableInterrupt();
                    }

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

                NativeEventHandler callbacksOld = m_callbacks;
                NativeEventHandler callbacksNew = (NativeEventHandler)Delegate.Remove(callbacksOld, value);

                try
                {
                    m_callbacks = (NativeEventHandler)callbacksNew;

                    if (callbacksNew == null && callbacksOld != null)
                    {
                        DisableInterrupt();
                    }
                }
                catch
                {
                    m_callbacks = callbacksOld;

                    throw;
                }
            }
        }

        private void MultiCastCase(uint port, uint state, DateTime time)
        {
            NativeEventHandler callbacks = m_callbacks;

            if (callbacks != null)
            {
                callbacks(port, state, time);
            }
        }
    }

    //--//

    public class Port : NativeEventDispatcher
    {
        public enum ResistorMode
        {
            Disabled = 0,
            PullDown = 1,
            PullUp = 2,
        }

        public enum InterruptMode
        {
            InterruptNone = 0,
            InterruptEdgeLow = 1,
            InterruptEdgeHigh = 2,
            InterruptEdgeBoth = 3,
            InterruptEdgeLevelHigh = 4,
            InterruptEdgeLevelLow = 5,
        }

        //--//

        [Microsoft.SPOT.FieldNoReflection]

        private InterruptMode m_interruptMode;
        private ResistorMode m_resistorMode;
        private uint m_portId;
        private uint m_flags;
        private bool m_glitchFilterEnable;
        private bool m_initialState;
        //--//

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern protected Port(Cpu.Pin portId, bool glitchFilter, ResistorMode resistor, InterruptMode interruptMode);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern protected Port(Cpu.Pin portId, bool initialState);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern protected Port(Cpu.Pin portId, bool initialState, bool glitchFilter, ResistorMode resistor);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern protected override void Dispose(bool disposing);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public bool Read();

        extern public Cpu.Pin Id
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public bool ReservePin(Cpu.Pin pin, bool fReserve);
    }

    //--//

    public class InputPort : Port
    {
        public InputPort(Cpu.Pin portId, bool glitchFilter, ResistorMode resistor)
            : base(portId, glitchFilter, resistor, InterruptMode.InterruptNone)
        {
        }

        protected InputPort(Cpu.Pin portId, bool glitchFilter, ResistorMode resistor, InterruptMode interruptMode)
            : base(portId, glitchFilter, resistor, interruptMode)
        {
        }

        protected InputPort(Cpu.Pin portId, bool initialState, bool glitchFilter, ResistorMode resistor)
            : base(portId, initialState, glitchFilter, resistor)
        {
        }

        extern public ResistorMode Resistor
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;

            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            set;
        }

        extern public bool GlitchFilter
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;

            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            set;
        }
    }

    //--//

    public class OutputPort : Port
    {
        public OutputPort(Cpu.Pin portId, bool initialState)
            : base(portId, initialState)
        {
        }

        protected OutputPort(Cpu.Pin portId, bool initialState, bool glitchFilter, ResistorMode resistor)
            : base(portId, initialState, glitchFilter, resistor)
        {
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public void Write(bool state);

        extern public bool InitialState
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }
    }

    //--//

    public sealed class TristatePort : OutputPort
    {
        public TristatePort(Cpu.Pin portId, bool initialState, bool glitchFilter, ResistorMode resistor)
            : base(portId, initialState, glitchFilter, resistor)
        {
        }

        extern public bool Active
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            set;
        }

        extern public ResistorMode Resistor
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;

            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            set;
        }

        extern public bool GlitchFilter
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }
    }

    //--//

    public sealed class InterruptPort : InputPort
    {
        //--//

        public InterruptPort(Cpu.Pin portId, bool glitchFilter, ResistorMode resistor, InterruptMode interrupt)
            : base(portId, glitchFilter, resistor, interrupt)
        {
            m_threadSpawn = null;
            m_callbacks = null;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public void ClearInterrupt();

        extern public InterruptMode Interrupt
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            set;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public override void EnableInterrupt();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public override void DisableInterrupt();

    }
}


