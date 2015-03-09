using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SPOT.Hardware;
using System.Diagnostics;

namespace Microsoft.SPOT.Hardware
{
    public static class Cpu
    {
        public enum Pin : int
        {
            GPIO_NONE = -1,
        }
    }
}

namespace Microsoft.SPOT.Emulator.Gpio
{
    internal class GpioDriver : HalDriver<IGpioDriver>, IGpioDriver
    {
        private GpioPort GetGpioPort( Cpu.Pin pin, bool writeTraceOnError )
        {
            return this.Emulator.GpioPorts.DeviceGet( pin, writeTraceOnError );
        }

        #region IGpioDriver Members

        bool IGpioDriver.Initialize()
        {
            return true;
        }

        bool IGpioDriver.Uninitialize()
        {
            for(uint i=0; i<((IGpioDriver)this).GetPinCount(); i++)
            {
               ((IGpioDriver)this).ReservePin(i, false);
            }
            return true;
        }

        uint IGpioDriver.Attributes(uint pin)
        {
            return (uint)GetGpioPort((Cpu.Pin)pin, false).DeviceModesAllowed;
        }

        void IGpioDriver.DisablePin( uint pin, int resistorState, uint direction, uint altFunction )
        {
            lock (this.Emulator.IsrLock)
            {
                GetGpioPort( (Cpu.Pin)pin, true ).DeviceDisablePort();
            }
        }

        void IGpioDriver.EnableOutputPin( uint pin, bool initialState )
        {
            lock (this.Emulator.IsrLock)
            {
                GetGpioPort( (Cpu.Pin)pin, true ).DeviceEnableAsOutputPort( initialState );
            }
        }

        bool IGpioDriver.EnableInputPin( uint pin, bool glitchFilterEnable, IntPtr isr, int intEdge, int resistorState )
        {
            return ((IGpioDriver)this).EnableInputPin( pin, glitchFilterEnable, isr, IntPtr.Zero, intEdge, resistorState );
        }

        bool IGpioDriver.EnableInputPin( uint pin, bool glitchFilterEnable, IntPtr isr, IntPtr isrParam, int intEdge, int resistorState )
        {
            lock (this.Emulator.IsrLock)
            {
                GetGpioPort((Cpu.Pin)pin, true).DeviceEnableAsInputPort(glitchFilterEnable, (GpioResistorMode)resistorState, (GpioInterruptMode)intEdge, isr, isrParam);
            }

            return true;
        }

        bool IGpioDriver.GetPinState( uint pin )
        {
            return GetGpioPort( (Cpu.Pin)pin, true ).DeviceRead();
        }

        void IGpioDriver.SetPinState( uint pin, bool pinState )
        {
            GetGpioPort( (Cpu.Pin)pin, true ).DeviceWrite( pinState );
        }

        bool IGpioDriver.PinIsBusy( uint pin )
        {
            return GetGpioPort( (Cpu.Pin)pin, false ).DeviceIsBusy();
        }

        bool IGpioDriver.ReservePin( uint pin, bool reserve )
        {
            lock (this.Emulator.IsrLock)
            {
                return GetGpioPort( (Cpu.Pin)pin, false ).DeviceReserve( reserve );
            }
        }

        uint IGpioDriver.GetDebounce()
        {
            return this.Emulator.GpioPorts.DeviceDebounceTime;
        }

        bool IGpioDriver.SetDebounce( long debounceTime )
        {
            this.Emulator.GpioPorts.DeviceDebounceTime = (uint)debounceTime;
            return true;
        }

        int IGpioDriver.GetPinCount()
        {
            return (int)this.Emulator.GpioPorts.MaxPorts;
        }

        void IGpioDriver.GetPinsMap( IntPtr pins,  int  size )
        {
            int pinCount = (int)Emulator.GpioPorts.MaxPorts;
            byte[] attributes = new byte[pinCount];
            int i = 0;

            for (i = 0; i < attributes.Length; i++)
            {
                attributes[i] = (byte)GetGpioPort((Cpu.Pin)i, false).DeviceModesAllowed;
            }

            System.Runtime.InteropServices.Marshal.Copy(attributes, 0, pins, Math.Min(attributes.Length, size));
        }

        byte IGpioDriver.GetSupportedResistorModes(uint pin)
        {
            return (byte)GetGpioPort((Cpu.Pin)pin, false).Resistor;
        }

        byte IGpioDriver.GetSupportedInterruptModes( uint pin ) 
        {
            return (byte)GetGpioPort((Cpu.Pin)pin, false).Interrupt;
        }

        int IGpioDriver.GetVirtualKeyPin(uint virtualKey)
        {
            foreach(GpioPort gpioPort in Emulator.GpioPorts)
            {
                if ((uint)gpioPort.VirtualKey == virtualKey)
                    return (int)gpioPort.Pin;
            }

            return (int)Cpu.Pin.GPIO_NONE;
        }

        #endregion
    }

    public class GpioCollection : EmulatorComponentCollection
    {
        GpioPort[] _ports;
        uint _debounceMs;
        bool _hardwareDebounceSupported;
        uint _maxPorts;
        List<GpioPort> _validPorts;

        public GpioCollection()
            : base( typeof( GpioPort ) )
        {
            _maxPorts = 128;
            _debounceMs = 0;
            _hardwareDebounceSupported = false;
        }

        public override void SetupComponent()
        {
            _ports = new GpioPort[_maxPorts];
            _validPorts = new List<GpioPort>();
        }

        internal GpioPort DeviceGet( Cpu.Pin pin, bool writeTraceOnError )
        {
            // If pin as index is out of range of the _ports array, then we return GpioPortNull for that index
            // In particular it happens for GPOI_PORT_NONE ( -1 )
            if ((int)pin < 0 || (int)pin >= _ports.Length)
            {
                return new GpioPortNull( pin, GpioPortMode.None );
            }

            GpioPort port = _ports[(int)pin];

            if (port == null)
            {
                port = new GpioPortNull( pin );
                _ports[(int)pin] = port;
            }

            if (writeTraceOnError && port is GpioPortNull)
            {
                GpioPortNull nullPort = (GpioPortNull)port;

                if (nullPort.DisplayWarning || Emulator.Verbose)
                {
                    Trace.WriteLine( "Warning: System attempts to access a GPIO port at pin " + pin + " that was not configured." );
                    nullPort.TurnOffWarning();
                }
            }

            return port;
        }

        public GpioPort this[Cpu.Pin pin]
        {
            get
            {
                if (Exists( pin ) == false)
                {
                    throw new ArgumentException( "GpioPort at pin " + pin + " does not exist." );
                }

                return _ports[(int)pin];
            }
        }

        internal override void RegisterInternal(EmulatorComponent ec)
        {
            GpioPort port = ec as GpioPort;

            if (port == null)
            {
                throw new Exception( "Attempt to register a non GpioPort with GpioPortCollection." );
            }

            Cpu.Pin pin = port.Pin;

            Debug.Assert( !(port is GpioPortNull) );

            if (Exists( pin ))
            {
                throw new Exception( "GPIO port " + pin + " is already set." );
            }

            _ports[(int)pin] = port;
            _validPorts.Add(port);

            base.RegisterInternal(ec);
        }

        internal override void UnregisterInternal(EmulatorComponent ec)
        {
            GpioPort gpioPort = ec as GpioPort;

            if (gpioPort != null)
            {
                Debug.Assert( _ports[(int)gpioPort.Pin] == gpioPort );

                _ports[(int)gpioPort.Pin] = null;
                _validPorts.Remove(gpioPort);

                base.UnregisterInternal( ec );
            }
        }

        internal bool Exists( Cpu.Pin pin )
        {
            GpioPort port = _ports[(int)pin];

            return (port != null) && ((port is GpioPortNull) == false);
        }

        internal void IsrCallback( IntPtr isr, Cpu.Pin pin, bool pinState, IntPtr param )
        {
            this.Emulator.ExecuteWithInterruptsDisabled(
                delegate
                {
                    this.Emulator.EmulatorNative.GpioIsrCallback( isr, (uint)pin, pinState, param );
                }
            );
        }        

        internal uint DeviceDebounceTime
        {
            get { return _debounceMs; }
            set { _debounceMs = value; }
        }

        public uint DebounceTime
        {
            get { return _debounceMs; }
        }

        public bool HardwareDebounceSupported
        {
            get
            {
                return _hardwareDebounceSupported;
            }
            set
            {
                ThrowIfNotConfigurable();
                _hardwareDebounceSupported = value;
            }
        }

        public uint MaxPorts
        {
            get
            {
                return _maxPorts;
            }
            set
            {
                ThrowIfNotConfigurable();
                _maxPorts = value;
            }
        }

        public override System.Collections.IEnumerator GetEnumerator()
        {
            return _validPorts.GetEnumerator();
        }

        public override int Count
        {
            get { return _validPorts.Count; }
        }

        public override void CopyTo(Array array, int index)
        {
            GpioPort[] gpioPortArray = array as GpioPort[];
            if (gpioPortArray == null)
            {
                throw new ArgumentException("Cannot cast array into GpioPort[]");
            }
            _validPorts.CopyTo(gpioPortArray, index);
        }
    }

    // Need to keep in-sync with CLR_RT_HeapBlock_IOPort struct in TinyCLR_Runtime__HeapBlock.h AND GPIO_ATTRIBUTE_* #defines in CPU_GPIO_decl.h
    [FlagsAttribute]
    public enum GpioPortMode : uint
    {
        None = 0,
        InputPort = 1,
        OutputPort = 2,
        InputOutputPort = InputPort | OutputPort
    }

    public enum GpioResistorMode : uint
    {
        Disabled = 0,
        PullDown = 1,
        PullUp = 2
    }

    public enum GpioInterruptMode : uint
    {
        None = 0,
        EdgeLow = 1,
        EdgeHigh = 2,
        EdgeBoth = 3,
        LevelHigh = 4,
        LevelLow = 5
    }

    public delegate void GpioActivity( GpioPort sender, bool edge );

    public class GpioPort : EmulatorComponent
    {
        // common
        protected Cpu.Pin _pin;
        GpioPortMode _modesExpected;
        GpioPortMode _modesAllowed;
        bool _isReserved;
        GpioPortMode _mode;
        protected VirtualKey _virtualKey = VirtualKey.None;

        protected bool _state;
        protected bool _stateInitialized;

        // InputPort only
        protected IntPtr _isr;
        protected IntPtr _isrParam;
        protected GpioInterruptMode _interruptMode;
        protected GpioResistorMode _resistorMode;

        protected DebounceMode _debounceMode;

        protected enum DebounceMode
        {
            HardwareDebounce,
            SoftwareDebounce,
            None
        }

        // OutputPort only
        protected GpioActivity _evtActivity;

        public GpioPort()
        {
            _pin = Cpu.Pin.GPIO_NONE;
            _modesExpected = GpioPortMode.None;
            _modesAllowed = GpioPortMode.None;
        }

        protected GpioPort( Cpu.Pin pin, GpioPortMode expected, GpioPortMode allowed )
        {
            _pin = pin;
            _modesExpected = expected;
            _modesAllowed = allowed;
        }

        public override void SetupComponent()
        {
            if (_pin == Cpu.Pin.GPIO_NONE || _pin < 0)
            {
                throw new Exception( "The GpioPort has an invalid pin." );
            }

            if ((_modesExpected & _modesAllowed) != _modesExpected)
            {
                throw new InvalidOperationException( "All modes expected have to be allowed: expected = " +
                    _modesExpected.ToString() + "; allowed = " + _modesAllowed.ToString() );
            }

            _isReserved = false;
            _mode = GpioPortMode.None;
            _state = false;
            _stateInitialized = false;

            _interruptMode = GpioInterruptMode.None;
            _resistorMode = GpioResistorMode.Disabled;
            _debounceMode = DebounceMode.None;
        }

        public Cpu.Pin Pin
        {
            get { return _pin; }
            set
            {
                ThrowIfNotConfigurable();

                if (_pin == Cpu.Pin.GPIO_NONE)
                {
                    _pin = value;
                }
                else
                {
                    throw new Exception( "Pin can only be set once." );
                }
            }
        }

        public override bool IsReplaceableBy( EmulatorComponent ec )
        {
            if (ec is GpioPort)
            {
                return (((GpioPort)ec).Pin == _pin);
            }
            else
            {
                return false;
            }
        }

        internal void DeviceEnableAsOutputPort( bool initialState )
        {
            if ((_modesExpected & GpioPortMode.OutputPort) == 0)
            {
                if ((this is GpioPortNull) == false) // We put an warning about accessing a non-existing port in DeviceGet already
                {
                    Trace.WriteLine( "Attempt to initialize GPIO Port " + Pin + " as an output port when it's expecting " + _modesExpected.ToString() );
                }
                return;
            }

            if (_mode != GpioPortMode.None)
            {
                // return the port to a Disabled state first if it wasn't previously disabled.
                DeviceDisablePort();
            }

            _mode = GpioPortMode.OutputPort;
            _state = initialState;
            _stateInitialized = true;
        }

        internal void DeviceEnableAsInputPort( bool glitchFilter, GpioResistorMode resistorMode, GpioInterruptMode interruptMode, IntPtr isr, IntPtr isrParam )
        {
            if ((_modesExpected & GpioPortMode.InputPort) == 0)
            {
                if ((this is GpioPortNull) == false) // We put an warning about accessing a non-existing port in DeviceGet already
                {
                    Trace.WriteLine( "Attempt to initialize GPIO Port " + Pin + " as an input port when it's expecting " + _modesExpected.ToString() );
                }
                return;
            }

            if (_mode != GpioPortMode.None)
            {
                // return the port to a Disabled state first if it wasn't previously disabled.
                DeviceDisablePort();
            }

            _interruptMode = interruptMode;
            _isr = isr;
            _isrParam = isrParam;
            _resistorMode = resistorMode;

            if (glitchFilter)
            {
                _debounceMode = (this.Emulator.GpioPorts.HardwareDebounceSupported) ? DebounceMode.HardwareDebounce : DebounceMode.SoftwareDebounce;
            }
            else
            {
                _debounceMode = DebounceMode.None;
            }

            _mode = GpioPortMode.InputPort;

            if (_stateInitialized == false)
            {
                // Since the pullup/pulldown are meant for as a way to avoid floating values, we shouldn't count the state as initialized.
                // We just simply set the state appropriately.
                if (_resistorMode == GpioResistorMode.PullUp)
                {
                    _state = true;
                }
                else if (_resistorMode == GpioResistorMode.PullDown)
                {
                    _state = false;
                }
            }

            if (_interruptMode != GpioInterruptMode.None)
            {
                if (isr.Equals( IntPtr.Zero ))
                {
                    Debug.Assert( false, "Invalid ISR." );
                    Trace.WriteLine( "Error: Interrupt port " + _pin + " has an invalid ISR." );
                    return;
                }

                if ((interruptMode == GpioInterruptMode.LevelHigh && _state == true) ||
                    (interruptMode == GpioInterruptMode.LevelLow && _state == false))
                {
                    HandleDebounce( true );

                    // Note that HandleDebounce will make this function re-entrant if it's a level interrupt.
                    return;
                }
            }
            else
            {
                _isr = IntPtr.Zero;
                _isrParam = IntPtr.Zero;
            }
        }

        internal void DeviceDisablePort()
        {
            _isr = IntPtr.Zero;
            _isrParam = IntPtr.Zero;
            _interruptMode = GpioInterruptMode.None;
            _resistorMode = GpioResistorMode.Disabled;
            _mode = GpioPortMode.None;
        }

        internal bool DeviceRead()
        {
            return _state;
        }

        internal virtual void DeviceWrite( bool state )
        {
            if (_mode != GpioPortMode.OutputPort)
            {
                Debug.Assert( false, "Attempt to write on an invalid GPIO port." );
                Trace.WriteLine( "Warning: Attempt to write on an invalid GPIO port.(Pin " + Pin + ", Mode: " + _mode + ")" );
                return;
            }

            Debug.Assert( _stateInitialized == true ); // An output port should always have a value.

            if (state == _state) return;

            _state = state;

            if (_evtActivity != null)
            {
                _evtActivity( this, state );
            }
        }

        internal bool DeviceReserve( bool reserve )
        {
            if (reserve && _isReserved)
            {
                return false;
            }

            _isReserved = reserve;
            return true;
        }

        internal bool DeviceIsBusy()
        {
            return _isReserved;
        }

        internal GpioPortMode DeviceModesAllowed
        {
            get { return _modesAllowed; }
        }

        public GpioPortMode ModesExpected
        {
            get { return _modesExpected; }
            set
            {
                ThrowIfNotConfigurable();
                _modesExpected = value;
            }
        }

        public GpioPortMode ModesAllowed
        {
            get { return _modesAllowed; }
            set
            {
                ThrowIfNotConfigurable();
                _modesAllowed = value;
            }
        }

        public bool IsBusy
        {
            get { return _isReserved; }
        }

        public virtual bool Read()
        {
            return _state;
        }

        public virtual void Write( bool state )
        {
            //The isr lock needs to be taken before deciding whether to fire the interrupt.  Otherwise, 
            //a race condition can occur where the state of the pin changes, an interrupt is about to be fired,
            //and then the interpreter thread decides to disable the pin, for example
            lock (this.Emulator.IsrLock)
            {
                if (_mode != GpioPortMode.InputPort)
                {
                    Trace.WriteLine( "Warning: Attempt to write on an invalid GPIO port.(Pin " + Pin + ", Mode: " + _mode + ")" );
                }

                _stateInitialized = true;

                if (state != _state)
                {
                    _state = state;

                    if (_interruptMode != GpioInterruptMode.None)
                    {
                        // check if this transition is of interest
                        if ((_state && ((_interruptMode == GpioInterruptMode.LevelHigh) || (_interruptMode == GpioInterruptMode.EdgeHigh) || (_interruptMode == GpioInterruptMode.EdgeBoth))) ||
                            (!_state && ((_interruptMode == GpioInterruptMode.LevelLow) || (_interruptMode == GpioInterruptMode.EdgeLow) || (_interruptMode == GpioInterruptMode.EdgeBoth))))
                        {
                            HandleDebounce( _state );
                        }
                    }
                }
            }
        }

        public GpioPortMode Mode
        {
            get
            {
                return _mode;
            }
        }

        void HandleDebounce( bool edge )
        {
            switch (_debounceMode)
            {
                //Let the debouncing cases fall through for now
                case DebounceMode.HardwareDebounce:
                case DebounceMode.SoftwareDebounce:

                case DebounceMode.None:
                    this.Emulator.GpioPorts.IsrCallback( _isr, _pin, edge, _isrParam );
                    break;
            }
        }

        public event GpioActivity OnGpioActivity
        {
            add { _evtActivity += value; }
            remove { _evtActivity -= value; }
        }

        public GpioInterruptMode Interrupt
        {
            get
            {
                return _interruptMode;
            }

            set
            {
                _interruptMode = value;
            }
        }

        public GpioResistorMode Resistor
        {
            get
            {
                return _resistorMode;
            }

            set
            {
                _resistorMode = value;
            }
        }

        public VirtualKey VirtualKey
        {
            get
            {
                return _virtualKey;
            }

            set
            {
                _virtualKey = value;
            }
        }
    }

    internal sealed class GpioPortNull : GpioPort
    {
        public GpioPortNull( Cpu.Pin pin )
            : base( pin, GpioPortMode.None, GpioPortMode.InputOutputPort )
        {
        }

        public GpioPortNull(Cpu.Pin pin, GpioPortMode portCapabilities )
            : base(pin, GpioPortMode.None, portCapabilities)
        {
        }


        internal override void DeviceWrite( bool value )
        {
        }

        private bool _displayWarning = true;

        internal bool DisplayWarning
        {
            get { return _displayWarning; }
        }

        internal void TurnOffWarning()
        {
            _displayWarning = false;
        }
    }

    public enum VirtualKey
    {

        /// </summary>
        None = 0,
        VK_LBUTTON = 0x01,
        VK_RBUTTON = 0x02,
        VK_CANCEL = 0x03,
        VK_MBUTTON = 0x04,    /* NOT contiguous with L & RBUTTON */

        VK_BACK = 0x08,
        VK_TAB = 0x09,

        VK_CLEAR = 0x0C,
        VK_RETURN = 0x0D,

        VK_SHIFT = 0x10,
        VK_CONTROL = 0x11,
        VK_MENU = 0x12,
        VK_PAUSE = 0x13,
        VK_CAPITAL = 0x14,

        VK_KANA = 0x15,
        VK_HANGEUL = 0x15,  /* old name - should be here for compatibility */
        VK_HANGUL = 0x15,

        VK_JUNJA = 0x17,
        VK_FINAL = 0x18,
        VK_HANJA = 0x19,
        VK_KANJI = 0x19,

        VK_ESCAPE = 0x1B,

        VK_CONVERT = 0x1c,
        VK_NOCONVERT = 0x1d,

        VK_SPACE = 0x20,
        VK_PRIOR = 0x21,
        VK_NEXT = 0x22,
        VK_END = 0x23,
        VK_HOME = 0x24,
        //     The LEFT button.
        VK_LEFT = 0x25,
        //     The UP button.
        VK_UP = 0x26,
        //     The RIGHT button.
        VK_RIGHT = 0x27,
        //     The DOWN button.
        VK_DOWN = 0x28,
        VK_SELECT = 0x29,
        VK_PRINT = 0x2A,
        VK_EXECUTE = 0x2B,
        VK_SNAPSHOT = 0x2C,
        VK_INSERT = 0x2D,
        VK_DELETE = 0x2E,
        VK_HELP = 0x2F,

        /* VK_0 thru VK_9 are the same as ASCII '0' thru '9' (0x30 - 0x39) */
        VK_0 = 0x30,
        VK_1 = 0x31,
        VK_2 = 0x32,
        VK_3 = 0x33,
        VK_4 = 0x34,
        VK_5 = 0x35,
        VK_6 = 0x36,
        VK_7 = 0x37,
        VK_8 = 0x38,
        VK_9 = 0x39,

        /* VK_A thru VK_Z are the same as ASCII 'A' thru 'Z' (0x41 - 0x5A) */
        VK_A = 0x41,
        VK_B = 0x42,
        VK_C = 0x43,
        VK_D = 0x44,
        VK_E = 0x45,
        VK_F = 0x46,
        VK_G = 0x47,
        VK_H = 0x48,
        VK_I = 0x49,
        VK_J = 0x4A,
        VK_K = 0x4B,
        VK_L = 0x4C,
        VK_M = 0x4D,
        VK_N = 0x4E,
        VK_O = 0x4F,
        VK_P = 0x50,
        VK_Q = 0x51,
        VK_R = 0x52,
        VK_S = 0x53,
        VK_T = 0x54,
        VK_U = 0x55,
        VK_V = 0x56,
        VK_W = 0x57,
        VK_X = 0x58,
        VK_Y = 0x59,
        VK_Z = 0x5A,



        VK_LWIN = 0x5B,
        VK_RWIN = 0x5C,
        VK_APPS = 0x5D,

        VK_SLEEP = 0x5F,

        VK_NUMPAD0 = 0x60,
        VK_NUMPAD1 = 0x61,
        VK_NUMPAD2 = 0x62,
        VK_NUMPAD3 = 0x63,
        VK_NUMPAD4 = 0x64,
        VK_NUMPAD5 = 0x65,
        VK_NUMPAD6 = 0x66,
        VK_NUMPAD7 = 0x67,
        VK_NUMPAD8 = 0x68,
        VK_NUMPAD9 = 0x69,
        VK_MULTIPLY = 0x6A,
        VK_ADD = 0x6B,
        VK_SEPARATOR = 0x6C,
        VK_SUBTRACT = 0x6D,
        VK_DECIMAL = 0x6E,
        VK_DIVIDE = 0x6F,
        VK_F1 = 0x70,
        VK_F2 = 0x71,
        VK_F3 = 0x72,
        VK_F4 = 0x73,
        VK_F5 = 0x74,
        VK_F6 = 0x75,
        VK_F7 = 0x76,
        VK_F8 = 0x77,
        VK_F9 = 0x78,
        VK_F10 = 0x79,
        VK_F11 = 0x7A,
        VK_F12 = 0x7B,
        VK_F13 = 0x7C,
        VK_F14 = 0x7D,
        VK_F15 = 0x7E,
        VK_F16 = 0x7F,
        VK_F17 = 0x80,
        VK_F18 = 0x81,
        VK_F19 = 0x82,
        VK_F20 = 0x83,
        VK_F21 = 0x84,
        VK_F22 = 0x85,
        VK_F23 = 0x86,
        VK_F24 = 0x87,

        VK_NUMLOCK = 0x90,
        VK_SCROLL = 0x91,

        /*
        * VK_L* & VK_R* - left and right Alt, Ctrl and Shift virtual keys.
        * Used only as parameters to GetAsyncKeyState() and GetKeyState().
        * No other API or message will distinguish left and right keys in this way.
        */
        VK_LSHIFT = 0xA0,
        VK_RSHIFT = 0xA1,
        VK_LCONTROL = 0xA2,
        VK_RCONTROL = 0xA3,
        VK_LMENU = 0xA4,
        VK_RMENU = 0xA5,

        VK_EXTEND_BSLASH = 0xE2,
        VK_OEM_102 = 0xE2,

        VK_PROCESSKEY = 0xE5,

        VK_ATTN = 0xF6,
        VK_CRSEL = 0xF7,
        VK_EXSEL = 0xF8,
        VK_EREOF = 0xF9,
        VK_PLAY = 0xFA,
        VK_ZOOM = 0xFB,
        VK_NONAME = 0xFC,
        VK_PA1 = 0xFD,
        VK_OEM_CLEAR = 0xFE,


        VK_SEMICOLON = 0xBA,
        VK_EQUAL = 0xBB,
        VK_COMMA = 0xBC,
        VK_HYPHEN = 0xBD,
        VK_PERIOD = 0xBE,
        VK_SLASH = 0xBF,
        VK_BACKQUOTE = 0xC0,

        VK_BROWSER_BACK = 0xA6,
        VK_BROWSER_FORWARD = 0xA7,
        VK_BROWSER_REFRESH = 0xA8,
        VK_BROWSER_STOP = 0xA9,
        VK_BROWSER_SEARCH = 0xAA,
        VK_BROWSER_FAVORITES = 0xAB,
        VK_BROWSER_HOME = 0xAC,
        VK_VOLUME_MUTE = 0xAD,
        VK_VOLUME_DOWN = 0xAE,
        VK_VOLUME_UP = 0xAF,
        VK_MEDIA_NEXT_TRACK = 0xB0,
        VK_MEDIA_PREV_TRACK = 0xB1,
        VK_MEDIA_STOP = 0xB2,
        VK_MEDIA_PLAY_PAUSE = 0xB3,
        VK_LAUNCH_MAIL = 0xB4,
        VK_LAUNCH_MEDIA_SELECT = 0xB5,
        VK_LAUNCH_APP1 = 0xB6,
        VK_LAUNCH_APP2 = 0xB7,

        VK_LBRACKET = 0xDB,
        VK_BACKSLASH = 0xDC,
        VK_RBRACKET = 0xDD,
        VK_APOSTROPHE = 0xDE,
        VK_OFF = 0xDF,

        VK_DBE_ALPHANUMERIC = 0x0f0,
        VK_DBE_KATAKANA = 0x0f1,
        VK_DBE_HIRAGANA = 0x0f2,
        VK_DBE_SBCSCHAR = 0x0f3,
        VK_DBE_DBCSCHAR = 0x0f4,
        VK_DBE_ROMAN = 0x0f5,
        VK_DBE_NOROMAN = 0x0f6,
        VK_DBE_ENTERWORDREGISTERMODE = 0x0f7,
        VK_DBE_ENTERIMECONFIGMODE = 0x0f8,
        VK_DBE_FLUSHSTRING = 0x0f9,
        VK_DBE_CODEINPUT = 0x0fa,
        VK_DBE_NOCODEINPUT = 0x0fb,
        VK_DBE_DETERMINESTRING = 0x0fc,
        VK_DBE_ENTERDLGCONVERSIONMODE = 0x0fd,


        /// <summary>
        ///     The MENU button.
        /// </summary>
        Menu = 0x100,

        /// <summary>
        ///     The SELECT button.
        /// </summary>
        Select = 0x101,

        /// <summary>
        ///     The PLAY button.
        /// </summary>
        Play = 0x106,

        /// <summary>
        ///     The PAUSE button.
        /// </summary>
        Pause = 0x107,

        /// <summary>
        ///     The FAST FORWARD button.
        /// </summary>
        FastForward = 0x108,

        /// <summary>
        ///     The REWIND button.
        /// </summary>
        Rewind = 0x109,

        /// <summary>
        ///     The STOP button.
        /// </summary>
        Stop = 0x10A,

        /// <summary>
        ///     The BACK button.
        /// </summary>
        Back = 0x10B,

        /// <summary>
        ///     The HOME button.
        /// </summary>
        Home = 0x10C,

        /// <summary>
        /// Last in the standard MF buttons enumeration
        /// </summary>
        LastSystemDefinedButton = 0x110,

        // Users may define their button definitions with values larger than
        // Button.LastSystemDefinedButton 
        // Values less that Button.LastSystemDefinedButton are reserved for standard buttons.
        // Values above Button.LastSystemDefinedButton are for third party extensions.
        AppDefined1 = LastSystemDefinedButton + 1,
        AppDefined2 = LastSystemDefinedButton + 2,
        AppDefined3 = LastSystemDefinedButton + 3,
        AppDefined4 = LastSystemDefinedButton + 4,
        AppDefined5 = LastSystemDefinedButton + 5,
        AppDefined6 = LastSystemDefinedButton + 6,
        AppDefined7 = LastSystemDefinedButton + 7,
        AppDefined8 = LastSystemDefinedButton + 8,

    }
}
