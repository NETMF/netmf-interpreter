////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Reflection;
using Microsoft.SPOT;

//--//

namespace Microsoft.SPOT.Hardware
{
    [Serializable()]
    public class WatchdogException : SystemException
    {
        protected WatchdogException()
            : base()
        {
        }
    }

    /// <summary>
    /// Indicates what type of sleep event occurred.
    ///     Invalid         - Invalid event
    ///     ChangeRequested - A managed thread has requested the system to go into a sleep state
    ///     WakeUp          - The sytem woke up from the a sleep state
    /// </summary>
    public enum SleepEventType : byte
    {
        Invalid = 0,
        ChangeRequested = 1,
        WakeUp = 2,
    }

    /// <summary>
    /// Indicates what type of power event occurred.
    ///     Invalid     - Invalid event
    ///     PreNotify   - A notification that the indicated power event is about to happen
    ///     PostNotify  - A notification that the current power state just changed
    /// </summary>
    public enum PowerEventType : byte
    {
        Invalid = 0,
        PreNotify = 1,
        PostNotify = 2
    }

    /// <summary>
    /// The SleepEvent object contains information about a sleep event.  It is passed to PowerState.OnSleepChange event handlers.
    ///     EventType    - Indicates the type of this event (ChangeRequested or WakeUp)
    ///     Level        - The new sleep level for this event
    ///     WakeUpEvents - The events (if any) that caused this event.  This is only used on the WakeUp event.
    ///     Time         - The timestamp of when this event occurred
    /// </summary>
    public class SleepEvent : BaseEvent
    {
        public SleepEventType EventType;
        public SleepLevel Level;
        public HardwareEvent WakeUpEvents;
        public DateTime Time;
    }

    /// <summary>
    /// The PowerEvent object contains information about a power event.  It is passed to PowerState.OnPowerLevelChange event handlers.
    ///     EventType   - Indicates the type of this event (PreNotify or PostNotify)
    ///     Level       - The new power level introduced by this event
    ///     Time        - The timestamp of when this event occurred
    /// </summary>
    public class PowerEvent : BaseEvent
    {
        public PowerEventType EventType;
        public PowerLevel Level;
        public DateTime Time;
    }

    internal class PowerStateEventProcessor : IEventProcessor
    {
        public BaseEvent ProcessEvent(uint data1, uint data2, DateTime time)
        {
            EventCategory ec = (EventCategory)(0xFF & (data1 >> 8));
            BaseEvent be = null;

            if (ec == EventCategory.SleepLevel)
            {
                SleepEvent ev = new SleepEvent();

                ev.EventType = (SleepEventType)(data1 & 0xFF);    // data1 encodes the type in the lower 16 bits and...
                ev.Level = SleepLevel.Awake;
                ev.WakeUpEvents = (HardwareEvent)data2;
                ev.Time = time;

                be = ev;
            }
            else if (ec == EventCategory.PowerLevel)
            {
                PowerEvent ev = new PowerEvent();

                ev.EventType = PowerEventType.PostNotify;
                ev.Level = (PowerLevel)data2;
                ev.Time = time;

                be = ev;
            }

            return be;
        }
    }

    internal class PowerStateEventListener : IEventListener
    {
        public void InitializeForEventSource()
        {
        }

        public bool OnEvent(BaseEvent ev)
        {
            if (ev is SleepEvent)
            {
                PowerState.PostEvent((SleepEvent)ev);
            }
            else if (ev is PowerEvent)
            {
                PowerState.PostEvent((PowerEvent)ev);
            }

            return true;
        }
    }

    //--//

    /// <summary>
    /// The event handler delegate for the PowerState.OnSleepChange event.
    /// </summary>
    /// <param name="e">The SleepEvent object that contains details about the sleep event</param>
    public delegate void SleepChangeEventHandler(SleepEvent e);

    /// <summary>
    /// The event handler delegate for the PowerState.OnPowerLevelChange event.
    /// </summary>
    /// <param name="e">The PowerEvent object that contains information about the power level change event</param>
    public delegate void PowerLevelChangeEventHandler(PowerEvent e);

    /// <summary>
    /// The event handler delegate for the PowerState.OnReboot event.
    /// </summary>
    /// <param name="fSoftReboot"></param>
    public delegate void RebootEventHandler(bool fSoftReboot);

    //--//

    /// <summary>
    /// The SleepLevel enumeration contains the default sleep levels for the .Net Micro Framework.  The behavior for each of these
    /// states is determined by the hardware vendor.  The general guideline is that each increased value indicates a deeper sleep.
    /// Therefore, SelectiveOff might simply dim the display backlight, where as Sleep may turn off the display, and DeepSleep may
    /// go into a hibernation state.
    /// </summary>
    public enum SleepLevel : byte
    {
        Awake = 0x00,
        SelectiveOff = 0x10,
        Sleep = 0x20,
        DeepSleep = 0x30,
        Off = 0x40,
    }

    /// <summary>
    /// The PowerLevel enumeration contains the default power state levels for a .Net Micro Framework device.  The behavior for each
    /// of the levels is determined by the hardware vendor.  The general guideline is that each increased value indicates a lower
    /// power consumption state.  This enumeration may also be extended by hardware vendors to include additional PowerLevels.
    /// </summary>
    public enum PowerLevel : byte
    {
        High = 0x10,
        Medium = 0x20,
        Low = 0x30,
    }

    /// <summary>
    /// The HardwareEvent flags allow users to choose which events cause the system to awake from a sleep state.  These event can be bitwise OR'ed
    /// together to produce a set of events.
    ///
    /// The SystemTimer event is a timer which is responsible for waking sleeping threads and eventing managed timers.  If this event is not
    /// included in the WakeupEvents property or the wakeupEvents parameter in the Sleep method, then the system will will not wakeup for managed
    /// timers or thread sleeps.
    ///
    /// The GeneralPurpose event flag represents General Purpose Input/Output (GPIO) ports and any number of peripherals that use a GPIO pin as an
    /// eventing mechanism.  These peripherals may include storage devices, touch panels, network devices, etc.
    ///
    /// The OEMReserved events can be defined by the Device Manufacturer.
    /// </summary>
    [Flags()]
    public enum HardwareEvent
    {
        SerialIn = 0x00000001,
        SerialOut = 0x00000002,
        USBIn = 0x00000004,
        USBOut = 0x00000008,
        SystemTimer = 0x00000010,
        Timer1 = 0x00000020,
        Timer2 = 0x00000040,
        Socket = 0x00004000,
        Spi = 0x00008000,
        Charger = 0x00010000,
        OEMReserved1 = 0x00020000,
        OEMReserved2 = 0x00040000,
        FileSystemIO = 0x00080000,
        GeneralPurpose = 0x08000000, // GPIO, Touch, Gesture, Storage, Network, etc
        I2C = 0x10000000,
    }

    /// <summary>
    /// The PowerState class encapsulates the power management functionality of the .Net Micro Framework.  It enables the managed
    /// application to adjust power settings for the device.
    /// </summary>
    public static class PowerState
    {
        private static PowerLevel s_CurrentPowerLevel = PowerLevel.High;

        /// <summary>
        /// This event notifies listeners when a sleep event occurs.  The listeners will be notified
        /// prior to a sleep event and when the system wakes from a sleep.
        /// </summary>
        public static event SleepChangeEventHandler OnSleepChange;

        /// <summary>
        /// This event notifies listeners when a power level event occurs.
        /// </summary>
        public static event PowerLevelChangeEventHandler OnPowerLevelChange;

        /// <summary>
        /// This event notifies listeners prior to a device reboot (soft or hard).  The event handlers may have an execution
        /// constraint placed on them by the caller of the Reboot method.  Therefore, it is recommended that the event handlers
        /// be short atomic operations.
        /// </summary>
        public static event RebootEventHandler OnRebootEvent;

        /// <summary>
        /// The Sleep method puts the device into a sleeps state that is only woken by the events described in the paramter wakeUpEvents.
        /// Please note that if the event SystemTimer is not used, the system will not be woken up by any managed timers.  In addition,
        /// other threads will not be executed until this sleep call exits.  This method raises the OnSleepChange event.
        ///
        /// The MaximumTimeToActive property contains the timeout value for this call.
        /// </summary>
        /// <param name="level">Determines what level of sleep the system should enter.  The behavior of the level is determined
        /// by the hardware vendor.</param>
        /// <param name="wakeUpEvents">Determines the events that will cause the system to exit the given sleep level</param>
        public static void Sleep(SleepLevel level, HardwareEvent wakeUpEvents)
        {
            SleepChangeEventHandler h = OnSleepChange;

            if (h != null)
            {
                SleepEvent e = new SleepEvent();

                e.EventType = SleepEventType.ChangeRequested;
                e.Level = level;
                e.WakeUpEvents = wakeUpEvents;
                e.Time = DateTime.UtcNow;

                h(e);
            }

            InternalSleep(level, wakeUpEvents);
        }

        /// <summary>
        /// The ChangePowerLevel method enables the caller to adjust the current power level of the device.
        /// The behavior of the power levels are determined by the hardware vendor.  This method raises the
        /// OnPowerLevelChange event.
        /// </summary>
        /// <param name="level">Describes the power level for which the system should enter</param>
        public static void ChangePowerLevel(PowerLevel level)
        {
            if (s_CurrentPowerLevel == level) return;

            PowerLevelChangeEventHandler h = OnPowerLevelChange;

            if (h != null)
            {
                PowerEvent e = new PowerEvent();

                e.EventType = PowerEventType.PreNotify;
                e.Level = level;
                e.Time = DateTime.UtcNow;

                h(e);
            }

            InternalChangePowerLevel(level);

            s_CurrentPowerLevel = level;
        }

        /// <summary>
        /// The RebootDevice method enables the caller to force a soft or hard reboot of the device.
        /// This method raises the OnRebootEvent.  With this method, there are no execution constraints
        /// placed on the event handlers, so the reboot will only happen after each handler finishes.
        /// </summary>
        /// <param name="soft">Determines whether the reboot request is for a soft or hard reboot.  Note,
        /// some devices may not support soft reboot.
        /// </param>
        public static void RebootDevice(bool soft)
        {
            RebootDevice(soft, -1);
        }

        /// <summary>
        /// The RebootDevice method enables the caller to force a soft or hard reboot of the device.
        /// This method raises the OnRebootEvent.
        /// </summary>
        /// <param name="soft">Determines whether the reboot request is for a soft or hard reboot.  Note,
        /// some devices may not support soft reboot.</param>
        /// <param name="exeConstraintTimeout_ms">Execution constraint timeout (in milliseconds) for
        /// the event handlers. If the event handlers take longer than the given value, then
        /// the handlers will be aborted and the reboot will be executed.
        /// </param>
        public static void RebootDevice(bool soft, int exeConstraintTimeout_ms)
        {
            try
            {
                ExecutionConstraint.Install(exeConstraintTimeout_ms, 4);

                RebootEventHandler h = OnRebootEvent;

                if (h != null)
                {
                    h(soft);
                }
            }
            catch
            {
            }
            finally
            {
                Reboot(soft);
            }
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern internal static void Reboot(bool soft);

        /// <summary>
        /// The WaitForIdleCPU method waits for the system to enter an idle state (no active threads).  The call
        /// will return either when the timeout has expired or when the current idle time is greater than or equal
        /// to the expectedWorkItemDuration parameter (in milliseconds).
        /// </summary>
        /// <param name="expectedWorkItemDuration">The amount of idle time required to run the task</param>
        /// <param name="timeout">The timeout in milliseconds for the system to wait for the appropriate idle time</param>
        /// <returns>
        /// Returns true if the current idle time is greater than or equal to the time indicated by exeConstraintTimeout_ms.
        /// Returns false otherwise.
        ///</returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public bool WaitForIdleCPU(int expectedWorkItemDuration, int timeout);

        /// <summary>
        /// Gets the current power level of the system.  The behavior of the power level is determined by the hardware vendor.
        /// </summary>
        /// <returns></returns>
        public static PowerLevel CurrentPowerLevel
        {
            get
            {
                return s_CurrentPowerLevel;
            }
        }

        /// <summary>
        /// Gets and Sets the maximum timeout value for all system sleep calls (including internal system sleep calls).  Set
        /// this property to determine the maximum time the system should sleep before processing non-masked events.
        /// </summary>
        extern public static TimeSpan MaximumTimeToActive
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            set;
        }

        /// <summary>
        /// Gets or Sets the default wakeup events for all Thread.Sleep calls and internal system sleep calls.  Please note that
        /// if SystemTimer is not specified for this property, then managed code timers and thread sleeps will not be handled until
        /// the system wakes up.
        /// </summary>
        extern public static HardwareEvent WakeupEvents
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            set;
        }

        /// <summary>
        /// Gets the TimeSpan for which the system has been up and running.
        /// </summary>
        extern public static TimeSpan Uptime
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        //--//

        static PowerState()
        {
            PowerStateEventProcessor psep = new PowerStateEventProcessor();
            PowerStateEventListener psel = new PowerStateEventListener();

            Microsoft.SPOT.EventSink.AddEventProcessor(EventCategory.SleepLevel, psep);
            Microsoft.SPOT.EventSink.AddEventListener(EventCategory.SleepLevel, psel);
            Microsoft.SPOT.EventSink.AddEventProcessor(EventCategory.PowerLevel, psep);
            Microsoft.SPOT.EventSink.AddEventListener(EventCategory.PowerLevel, psel);
        }

        static internal void PostEvent(BaseEvent ev)
        {
            Timer messagePseudoThread = new Timer(MessageHandler, ev, 1, Timeout.Infinite);
        }

        private static void MessageHandler(object args)
        {
            if (args is SleepEvent)
            {
                SleepChangeEventHandler h = OnSleepChange;

                if (h != null)
                {
                    h((SleepEvent)args);
                }
            }
            else if (args is PowerEvent)
            {
                PowerEvent pe = (PowerEvent)args;

                PowerLevelChangeEventHandler h = OnPowerLevelChange;

                s_CurrentPowerLevel = pe.Level;

                if (h != null)
                {
                    h(pe);
                }
            }
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private static void InternalSleep(SleepLevel level, HardwareEvent wakeUpEvents);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private static void InternalChangePowerLevel(PowerLevel level);
    }

    //--//
    //--//
    //--//

    /// <summary>
    /// The WatchdogBehavior enumeration lists the different ways in which the system can handle watchdog events. All
    /// behavior types will attempt to log a watchdog event that can be retrieved with LastOccurrence property on the
    /// Watchdog class.
    ///     None                - Continues execution (may leave the system in a stalled state)
    ///     SoftReboot          - Performs a software reboot of the CLR (if the device does not support soft reboot, a hard reboot will occur)
    ///     HardReboot          - Performs a hardware reboot of the device
    ///     EnterBooter         - Enters the bootloader and waits for commands.
    ///     DebugBreak_Managed  - Injects a Watchdog exception into the current managed thread.  Note this will only work for native methods that
    ///                           take longer than the allotted watchdog timeout.  If the system is truly hung then the exception will not be seen.
    ///     DebugBreak_Native   - Intended for native debugging (porting kit users).  Stops execution at the native level.
    /// </summary>
    public enum WatchdogBehavior
    {
        None = 0x00000000,
        SoftReboot = 0x00000001,
        HardReboot = 0x00000002,
        EnterBooter = 0x00000003,
        DebugBreak_Managed = 0x00000004,
        DebugBreak_Native = 0x00000005,
    }

    /// <summary>
    /// The WatchdogEvent class is the object that is sent during Watchdog events.
    /// </summary>
    public class WatchdogEvent
    {
        /// <summary>
        /// The time stamp when the watchdog event occurred.
        /// </summary>
        public DateTime WatchdogEventTime;
        /// <summary>
        /// The watchdog timeout value when the watchdog event occured.
        /// </summary>
        public TimeSpan WatchdogTimeoutValue;
        /// <summary>
        /// The offending managed call that was being executed when the watchdog occurred.
        /// </summary>
        public MethodInfo OffendingMethod;

        internal WatchdogEvent(DateTime time, TimeSpan timeout, MethodInfo method)
        {
            this.WatchdogEventTime = time;
            this.WatchdogTimeoutValue = timeout;
            this.OffendingMethod = method;
        }
    }

    /// <summary>
    /// The static Watchdog class contains methods that enables managed code to determine the watchdog behavior.
    /// </summary>
    public static class Watchdog
    {
        /// <summary>
        /// Gets or sets the enabled state of the watchdog.  The watchdog can be turned off if the inter-op method
        /// takes an indeterminate time.  Note, the watchdog does not need to be turned off for managed code becuase
        /// the interpreter will make sure the watchdog is reset.
        /// </summary>
        extern public static bool Enabled
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            set;
        }

        /// <summary>
        /// Gets or sets the timeout value of for the watchdog.  Please note that some hardware vendors may
        /// set bounds and granularity on the timeout value for the watchdog.  Please check with the hardware
        /// vendor for specifications.
        /// </summary>
        extern public static TimeSpan Timeout
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            set;
        }

        /// <summary>
        /// Gets or sets the behavior of the watchdog event handler.  See description for the WatchdogBehavior enumeration.
        /// </summary>
        extern public static WatchdogBehavior Behavior
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            set;
        }

        /// <summary>
        /// Gets the last occurrence of a watchdog event (if one exists).
        /// </summary>
        public static WatchdogEvent LastOccurrence
        {
            get
            {
                DateTime time;
                TimeSpan timeout;
                MethodInfo info;

                if (GetLastOcurrenceDetails(out time, out timeout, out info))
                {
                    return new WatchdogEvent(time, timeout, info);
                }

                return null;
            }
        }

        /// <summary>
        /// Gets or sets the ILog interface for watchdog logging.
        /// </summary>
        extern public static ILog Log
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            set;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private static bool GetLastOcurrenceDetails(out DateTime time, out TimeSpan timeout, out MethodInfo info);
    }
}


