using System;
using System.IO.Ports;
using System.Text;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class TestPower : IMFTestInterface
    {
        private AutoResetEvent _wakeup;
        private AutoResetEvent _change;
        private AutoResetEvent _timerFired;
        private SleepEvent _wakeupEvent;
        private SleepEvent _changeEvent;
        private DateTime _timerTime;

        //--//

        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests");

            _wakeup = new AutoResetEvent(false);
            _change = new AutoResetEvent(false);
            _timerFired = new AutoResetEvent(false);

            /// Touch notifications are not "Turned ON" by default.
            /// You need to explicitly inform Touch engine that you want touch
            /// events to be pumped to your direction, and you want to work
            /// with rest of the architecture.
            
            return InitializeResult.ReadyToGo;            
        }         

        [TearDown]
        public void CleanUp()
        {
        }


        [TestMethod]
        public MFTestResults Uptime_CheckCorrectnessOnTimeChanges()
        {
            MFTestResults result = MFTestResults.Pass;

            try
            {
                DateTime now = DateTime.UtcNow;
                DateTime nowOneDayAfter = now.AddDays(1);
                DateTime nowOneDayBefore = now.Subtract(new TimeSpan(0,0,1));

                Log.Comment("Original local time            : " + now.ToString());
                Log.Comment("original local time now +1 day : " + nowOneDayAfter.ToString());
                Log.Comment("original local time now -1 day : " + nowOneDayBefore.ToString());

                Microsoft.SPOT.Hardware.Utility.SetLocalTime(now);

                TimeSpan uptimeNow = PowerState.Uptime;

                Microsoft.SPOT.Hardware.Utility.SetLocalTime(nowOneDayAfter);

                Log.Comment("... now loose some time... ");
                for (int i = 0; i < 100; ++i)
                {
                    if (PowerState.Uptime > new TimeSpan(300, 0, 0, 0, i))
                    {
                        Log.Comment("This test has been running 300 days...");
                    }
                }

                TimeSpan uptimeOneDayAfter = PowerState.Uptime;

                Log.Comment("Uptime on original local time        : " + uptimeNow.ToString());
                Log.Comment("Uptime on original local time +1 day : " + uptimeOneDayAfter.ToString());

                // certinly we can't have a negative difference
                if (
                    (uptimeOneDayAfter < uptimeNow)
                   )
                {
                    result = MFTestResults.Fail;
                }
                // assume we are well below one minute of delay...
                if (
                    ((uptimeOneDayAfter - uptimeNow) > new TimeSpan(0, 0, 59))
                   )
                {
                    result = MFTestResults.Fail;
                }

                Log.Comment("Device Uptime is: " + uptimeOneDayAfter.ToString());

                Microsoft.SPOT.Hardware.Utility.SetLocalTime(nowOneDayBefore);

                Log.Comment("... now loose some more time... ");
                for (int i = 0; i < 100; ++i)
                {
                    if (PowerState.Uptime > new TimeSpan(300, 0, 0, 0, i))
                    {
                        Log.Comment("This test has been running 300 days...");
                    }
                }

                TimeSpan uptimeOneDayBefore = PowerState.Uptime;

                Log.Comment("Uptime on original local time        : " + uptimeNow.ToString());
                Log.Comment("Uptime on original local time -1 day : " + uptimeOneDayBefore.ToString());

                // certinly we can't have a negative difference
                if (
                    (uptimeOneDayBefore < uptimeNow)
                   )
                {
                    result = MFTestResults.Fail;
                }
                // assume we are well below one minute of delay...
                if (
                    ((uptimeOneDayBefore - uptimeNow) > new TimeSpan(0, 0, 59))
                   )
                {
                    result = MFTestResults.Fail;
                }

                Log.Comment("Device Uptime is: " + uptimeOneDayBefore.ToString());
            }
            catch (Exception ex)
            {
                result = MFTestResults.Fail;

                Log.Exception(ex.Message);
            }

            return result;
        }

        [TestMethod]
        public MFTestResults Sleep_1_Get_Set_GetAndCheck()
        {
            MFTestResults result = MFTestResults.Fail;

            try
            {
                TimeSpan uptime = PowerState.Uptime;
                TimeSpan tta = PowerState.MaximumTimeToActive;
                HardwareEvent events = PowerState.WakeupEvents;

                Log.Comment("Uptime        : " + uptime);
                Log.Comment("Time-to-Active: " + tta);
                Log.Comment("Wakeup events : " + events);

                TimeSpan newTta = new TimeSpan(0, 0, 2);
                HardwareEvent newEvents = HardwareEvent.SerialIn | HardwareEvent.Socket | HardwareEvent.USBIn | HardwareEvent.SystemTimer | HardwareEvent.GeneralPurpose;

                PowerState.MaximumTimeToActive = newTta;
                PowerState.WakeupEvents = newEvents;

                tta = PowerState.MaximumTimeToActive;
                events = PowerState.WakeupEvents;

                if (
                    (tta == newTta) &&
                    (events == newEvents)
                    )
                {
                    result = MFTestResults.Pass;
                }

            }
            catch (Exception ex)
            {
                result = MFTestResults.Fail;

                Log.Exception(ex.Message);
            }

            return result;
        }

        [TestMethod]
        public MFTestResults Sleep_2_ChangeLevelFromManagedCode()
        {
            MFTestResults result = MFTestResults.Fail;

            try
            {
                const int c_Timeout = 5000; // ms

                HardwareEvent newEvents = HardwareEvent.SerialIn | HardwareEvent.USBIn | HardwareEvent.SystemTimer | HardwareEvent.GeneralPurpose;

                PowerState.OnSleepChange += new  SleepChangeEventHandler(this.SleepChange); 

                Timer t = new Timer( new TimerCallback( this.TimerCallback ), null, c_Timeout, Timeout.Infinite );

                PowerState.Sleep(SleepLevel.Sleep, newEvents);

                // we must have received the heads-up on power change...
                if (_change.WaitOne(1 * c_Timeout, false))
                {
                    // and timers must still work as scheduled
                    if(_timerFired.WaitOne(2 * c_Timeout, false))
                    {
                        if (_wakeup.WaitOne(3 * c_Timeout, false))
                        {
                            result = MFTestResults.Pass;
                        }
                    }
                }

                t.Dispose();
            }
            catch (Exception ex)
            {
                result = MFTestResults.Fail;

                Log.Exception(ex.Message);
            }

            PowerState.OnSleepChange -= new SleepChangeEventHandler(this.SleepChange); 

            return result;
        }

        [TestMethod]
        public MFTestResults Sleep_3_NoSystemTimerTest()
        {
            bool result = true;

            TimeSpan backup = PowerState.MaximumTimeToActive;

            try
            {
                PowerState.MaximumTimeToActive = new TimeSpan(0,0,0,2);
                TimeSpan tsTimeout = PowerState.MaximumTimeToActive;

                PowerState.OnSleepChange += new SleepChangeEventHandler(this.SleepChange);

                HardwareEvent newEvents = HardwareEvent.OEMReserved1;

                _timerFired.Reset();
                _wakeupEvent = null;

                Timer t = new Timer(new TimerCallback(this.TimerCallback), null, tsTimeout.Milliseconds / 2, Timeout.Infinite);

                DateTime dtStart = DateTime.Now;

                // this will wake up with the max time allowed
                PowerState.Sleep(SleepLevel.Sleep, newEvents);


                ///
                /// When the debugging is enbled on a device the debug port is selected as a wakeup event by default,
                /// so we ignore this test if we get any events.
                /// 
                if (_timerFired.WaitOne(1000, false) && (_wakeupEvent == null || _wakeupEvent.WakeUpEvents == 0))
                {
                    result = (_timerTime - dtStart) > tsTimeout;
                }

                PowerState.OnSleepChange -= new SleepChangeEventHandler(this.SleepChange);
            }
            finally
            {
                PowerState.MaximumTimeToActive = backup;
            }
            
            return result ? MFTestResults.Pass : MFTestResults.Fail;
        }


        [TestMethod]
        public MFTestResults Sleep_4_WaitForIdleCPU()
        {
            Timer t = new Timer(new TimerCallback(TimerCallback), null, 20, 20);

            bool fResult = !PowerState.WaitForIdleCPU( 200, 2000 );

            t.Change(Timeout.Infinite, Timeout.Infinite);
            t.Dispose();

            fResult = fResult && PowerState.WaitForIdleCPU(200, 2000);

            return fResult ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults UptimeTest_1_TestValue()
        {
            TimeSpan tsMin = new TimeSpan(0,0,0,0,500);
            TimeSpan tsMax = new TimeSpan(0,0,0,0,550);

            TimeSpan tsStart = PowerState.Uptime;

            Thread.Sleep(tsMin.Milliseconds);

            TimeSpan tsDiff = PowerState.Uptime - tsStart;

            return (tsDiff > tsMin && tsDiff < tsMax) ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Power_1_TestLevels()
        {
            bool fResult;

            PowerState.ChangePowerLevel(PowerLevel.Medium);

            fResult = PowerState.CurrentPowerLevel == PowerLevel.Medium;

            PowerState.ChangePowerLevel(PowerLevel.Low);

            fResult = fResult && PowerState.CurrentPowerLevel == PowerLevel.Low;

            PowerState.ChangePowerLevel(PowerLevel.High);

            fResult = fResult && PowerState.CurrentPowerLevel == PowerLevel.High;

            return fResult ? MFTestResults.Pass : MFTestResults.Fail;
        }
        
        //--//

        void SleepChange(SleepEvent ev)
        {
            if (ev.EventType == SleepEventType.ChangeRequested)
            {
                _change.Set();
                _changeEvent = ev;
            }
            else if (ev.EventType == SleepEventType.WakeUp)
            {
                _wakeup.Set();
                _wakeupEvent = ev;
            }
            else
            {
                Debug.Assert( false );
            }
        }

        void TimerCallback(object state)
        {
            _timerTime = DateTime.Now;
            _timerFired.Set();
        }
    }
}
