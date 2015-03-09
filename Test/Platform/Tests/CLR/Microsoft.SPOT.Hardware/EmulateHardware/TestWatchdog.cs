using System;
using System.IO.Ports;
using System.Text;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Platform.Test;
using System.Reflection;

namespace Microsoft.SPOT.Platform.Tests
{
    public class TestWatchdog : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests");

            bool newVal = !Watchdog.Enabled;

            Watchdog.Enabled = newVal;

            if (newVal != Watchdog.Enabled)
            {
                return InitializeResult.Skip;
            }

            return InitializeResult.ReadyToGo;            
        }         

        [TearDown]
        public void CleanUp()
        {
        }

        [TestMethod]
        public MFTestResults Watchdog_1_Get_Set_GetAndCheck()
        {
            MFTestResults result = MFTestResults.Pass;

            bool originalEnabled = Watchdog.Enabled;
            WatchdogBehavior originalBehavior = Watchdog.Behavior;
            TimeSpan orginalTimeout = Watchdog.Timeout;

            try
            {
                // get the watchdog values 

                bool enabled = Watchdog.Enabled;
                WatchdogBehavior behavior = Watchdog.Behavior;
                TimeSpan timeout = Watchdog.Timeout;

                Log.Comment("Enabled :" + enabled);
                Log.Comment("Behavior:" + behavior);
                Log.Comment("Timeout :" + timeout);


                // set the watchdog values
                enabled = !enabled;
                behavior = behavior == WatchdogBehavior.SoftReboot ? WatchdogBehavior.HardReboot : WatchdogBehavior.SoftReboot;
                timeout += new TimeSpan(0, 0, 60);

                Watchdog.Enabled = enabled;
                Watchdog.Behavior = behavior;
                Watchdog.Timeout = timeout;

                // check the values
                if (Watchdog.Enabled != enabled)
                {
                    result = MFTestResults.Fail;
                }
                if (Watchdog.Behavior != behavior)
                {
                    result = MFTestResults.Fail;
                }
                if (Watchdog.Timeout != timeout)
                {
                    result = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                result = MFTestResults.Fail;

                Log.Exception(ex.Message);
            }
            finally
            {
                Watchdog.Enabled = originalEnabled;
                Watchdog.Behavior = originalBehavior;
                Watchdog.Timeout = orginalTimeout;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults Watchdog_2_SetTimeoutAndBehviourNoneAndCauseWatchdog()
        {
            MFTestResults result = MFTestResults.Fail;

            ///
            /// This test will not work under the debugger for network debugging 
            /// devices
            /// 
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU != 3)
            {
                return MFTestResults.Skip;
            }

            TimeSpan originalWatchdogTimeout = Watchdog.Timeout;
            WatchdogBehavior originalBehavior = Watchdog.Behavior;
            bool originalWatchdogEnabled = Watchdog.Enabled;

            try
            {
                //WatchdogEvent wev = Watchdog.LastOccurrence;

                TimeSpan timeout = new TimeSpan(0, 0, 0, 2);

                // FOR A MANUAL TEST YOU CAN ENABLE THE FOLLOWING to see if a Hard or Soft reboot occurs the first time
                //Watchdog.Behavior = wev == null ? WatchdogBehavior.HardReboot : WatchdogBehavior.None; 
                Watchdog.Behavior = WatchdogBehavior.None; // this will make sure the watchdog does not reboot us

                if(ValidateWatchdog(CauseWatchdog(timeout)))
                {
                    result = MFTestResults.Pass;
                }
            }
            catch (WatchdogException ex)
            {
                result = MFTestResults.Fail;

                Log.Exception(ex.Message);
            }
            catch (Exception ex)
            {
                result = MFTestResults.Fail;

                Log.Exception(ex.Message);
            }
            finally
            {
                Watchdog.Timeout = originalWatchdogTimeout;
                Watchdog.Behavior = originalBehavior;
                Watchdog.Enabled = originalWatchdogEnabled;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults Watchdog_3_WatchdogException()
        {
            MFTestResults result = MFTestResults.Fail;

            ///
            /// This test will not work under the debugger for network debugging 
            /// devices
            /// 
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU != 3)
            {
                return MFTestResults.Skip;
            }

            TimeSpan originalWatchdogTimeout  = Watchdog.Timeout;
            WatchdogBehavior originalBehavior = Watchdog.Behavior;
            bool originalWatchdogEnabled      = Watchdog.Enabled;

            try
            {
                TimeSpan timeout = new TimeSpan(0, 0, 0, 2);

                Watchdog.Behavior = WatchdogBehavior.DebugBreak_Managed;

                CauseWatchdog(timeout);

                Log.Comment("Error: no watchdog exception");
            }
            catch (WatchdogException)
            {
                Log.Comment("Successfully caught watchdog exception");

                if(ValidateWatchdog(null))
                {
                    result = MFTestResults.Pass;
                }
            }
            catch(Exception ex)
            {
                Log.Comment("Expected WatchdogException but got: " + ex.ToString());
                result = MFTestResults.Fail;
            }
            finally
            {
                Watchdog.Timeout = originalWatchdogTimeout;
                Watchdog.Behavior = originalBehavior;
                Watchdog.Enabled = originalWatchdogEnabled;
            }

            return result;
        }

        //--//

        private bool ValidateWatchdog(MethodInfo expectedWatchdogMethod)
        {
            bool fResult = false;
            // at this point the watchdog must have fired
            WatchdogEvent wev = Watchdog.LastOccurrence;

            if (wev != null)
            {
                DateTime occurred = wev.WatchdogEventTime;
                TimeSpan tsx = DateTime.Now - occurred;
                MethodInfo method = wev.OffendingMethod;
                TimeSpan timeout_at_occurrece = wev.WatchdogTimeoutValue;

                Log.Comment("occurred at: " + occurred);
                Log.Comment("with a timeout of: " + timeout_at_occurrece);

                if(tsx.Milliseconds > 1000)
                {
                    Log.Comment("Error: Watchdog occurred over 1 second ago");
                }
                else if (method != null)
                {
                    Log.Comment("for method : " + method.Name);

                    if (expectedWatchdogMethod != null)
                    {
                        fResult = expectedWatchdogMethod.Name == method.Name;
                    }
                    else
                    {
                        fResult = true;
                    }
                }
                else
                {
                    Log.Comment("Method Unknown");
                    fResult = true;
                }
            }

            return fResult;
        }

        private MethodInfo CauseWatchdog(TimeSpan timeout)
        {
            MethodInfo mi = null;

            string tst = new string('a', 0x400 * timeout.Seconds);

            string search = new string('a', 0x200 * timeout.Seconds) + "z";

            TimeSpan calibration = new TimeSpan(0,0,0,0,500);

            Watchdog.Timeout = timeout;

            TimeSpan callTime = new TimeSpan();

            // now do some internal call that takes more than timeout
            while (true)
            {
                Watchdog.Enabled = true;
                mi = LongInternalCall(tst, search, ref callTime);
                Watchdog.Enabled = false;

                if (callTime > (timeout + calibration)) break;

                tst += new string('a', 0x200);
                search = new string('a', 0x100) + search;

                Debug.GC(true);
            }

            Debug.GC(true);

            return mi;
        }

        private MethodInfo LongInternalCall( string text, string search, ref TimeSpan time )
        {
            DateTime start = DateTime.Now;

            // can't find this
            text.IndexOf(search, 0);

            DateTime stop = DateTime.Now;

            time = stop - start;

            return typeof(String).GetMethod( "IndexOf", new Type[] { typeof(String), typeof(int) } );
        }
    }
}
