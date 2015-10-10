////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Net;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Time;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class TimeServiceTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");

            try
            {
                // Check networking - we need to make sure we can reach www.microsoft.com.
                Dns.GetHostEntry("www.microsoft.com");
            }
            catch (Exception ex)
            {
                Log.Exception("Unable to get address for www.microsoft.com", ex);
                Log.Comment("The device does not have networking support and hence skipping the test");
                return InitializeResult.Skip;
            }

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");
        }

        private ManualResetEvent timerEvent = new ManualResetEvent(false);
        TimeSpan utcTimeShiftAmount = new TimeSpan(5, 0, 0);
        DateTime timerTime;
        private void TimerCallback( object state )
        {
            TimeSpan diff = DateTime.UtcNow - timerTime;
            if(( diff.Seconds >= 25 ) && ( diff.Seconds <= 55 ))
            {
                timerEvent.Set();
            }
        }

        [TestMethod]
        public MFTestResults TimerTest0()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                timerEvent.Reset();
                /// Save the current time shifted by 5 hours.

                timerTime = DateTime.UtcNow - utcTimeShiftAmount;
                using(Timer t = new Timer( new TimerCallback( TimerCallback ), null, new TimeSpan( 0, 0, 30 ), new TimeSpan( -TimeSpan.TicksPerMillisecond ) ))
                {
                    /// We shift the utc back by 5 hours.
                    TimeService.SetUtcTime( timerTime.Ticks );

                    /// timer should still fire after 30 seconds even though absolute time has been manipulated.
                    if(!timerEvent.WaitOne( 2 * 60 * 1000, false ))
                    {
                        result = MFTestResults.Fail;
                    }

                    /// Reset the changes.
                    TimeService.SetUtcTime( ( DateTime.UtcNow + utcTimeShiftAmount ).Ticks );

                    t.Change( -1, -1 );
                }
            }
            catch(Exception ex)
            {
                Log.Exception( "Unexpected exception", ex );
                result = MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults TimeServiceConfigTest0()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                byte[] primaryServer = new byte[] { 192, 43, 244, 18 };
                byte[] alternateServer = new byte[] { 129, 6, 15, 28 };
                TimeServiceSettings settings = new TimeServiceSettings();
                settings.PrimaryServer = primaryServer;
                settings.AlternateServer = alternateServer;
                settings.Tolerance = 100;
                settings.RefreshTime = 60;

                /// Save the settings.
                TimeService.Settings = settings;

                /// Validate we have what we saved.
                settings = TimeService.Settings;
                if (settings.Tolerance != 100) throw new ArgumentException("Tolerance value unexpected.");
                if (settings.RefreshTime != 60) throw new ArgumentException("RefreshTime value unexpected.");
                int i = 0;
                for (i = 0; i < 4; i++)
                {
                    if (primaryServer[i] != settings.PrimaryServer[i]) throw new ArgumentException("PrimaryServer value unexpected.");
                }

                for (i = 0; i < 4; i++)
                {
                    if (alternateServer[i] != settings.AlternateServer[i]) throw new ArgumentException("AlternateServer value unexpected.");
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception", ex);
                result = MFTestResults.Fail;
            }

            return result;
        }

        private IPAddress GetTimeServiceAddress()
        {
            IPHostEntry entry = Dns.GetHostEntry("time.nist.gov");
            if (entry == null || entry.AddressList == null)
            {
                throw new ApplicationException("Get time service ip failed"); ;
            }

            IPAddress timeServiceAddress = null;
            for (int i = 0; i < entry.AddressList.Length; ++i)
            {
                timeServiceAddress = entry.AddressList[i];
                if (timeServiceAddress != null)
                    break;
            }

            if (timeServiceAddress == null)
            {
                throw new ApplicationException("Get time service ip failed"); ;
            }

            return timeServiceAddress;
        }

        [TestMethod]
        public MFTestResults TimeServiceUpdateTest0()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                /// Now call UpdateNow this should set the time back to correct one.
                TimeService.UpdateNow(GetTimeServiceAddress().GetAddressBytes(), 10);

                DateTime now = DateTime.Now;
                DateTime nowEbs = now;
                // 
                // SKU == 3 indicates the device is the emulator.
                //
                if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU != 3)
                {
                    /// EBS bug returns value in local time instead of UTC.
                    /// Remove following line once they fixed the bug.
                    nowEbs = nowEbs.AddHours( 7 );
                }

                TimeService.SetUtcTime(119600064000000000); /// 1/1/1980.
                                                            /// 
                DateTime old = DateTime.Now;

                /// Now call UpdateNow this should set the time back to correct one.
                TimeService.UpdateNow(GetTimeServiceAddress().GetAddressBytes(), 10);

                DateTime end = DateTime.Now;

                TimeSpan diff    = end < now    ? now    - end : end - now;
                TimeSpan diffEbs = end < nowEbs ? nowEbs - end : end - nowEbs;
                TimeSpan diff2   = now - old;

                // ten minutes (DNS may take a while)
                if(( diff > new TimeSpan( 0, 10, 0 ) && diffEbs > new TimeSpan( 0, 10, 0 ) ) && diff2 > new TimeSpan( 20 * 365, 0, 0, 0 ))
                {
                    throw new ArgumentException( "Update time invalid." );
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception", ex);
                result = MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults TimeServiceUpdateTest1()
        {
            MFTestResults result = MFTestResults.Pass;
            SystemTimeChangedEventHandler handler = new SystemTimeChangedEventHandler(TimeService_SystemTimeChanged);
            try
            {
                timerEvent.Reset();

                TimeService.SystemTimeChanged += handler;
                TimeService.UpdateNow(GetTimeServiceAddress().GetAddressBytes(), 10);
                if (!timerEvent.WaitOne(60 * 1000, false)) /// Wait some time for the event to be fired.
                {
                    result = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception", ex);
                result = MFTestResults.Fail;
            }
            finally
            {
                TimeService.SystemTimeChanged -= handler;
            }

            return result;
        }

        ManualResetEvent syncEvent = new ManualResetEvent(false);
        [TestMethod]
        public MFTestResults TimeServiceUpdateTest2()
        {
            /// This failure only happens in device. Skipping it for now.
            MFTestResults result = MFTestResults.Skip;
            TimeSyncFailedEventHandler handler = new TimeSyncFailedEventHandler(TimeService_TimeSyncFailed);
            try
            {
                syncEvent.Reset();

                TimeService.TimeSyncFailed += handler;
                TimeService.UpdateNow(GetTimeServiceAddress().GetAddressBytes(), 10);
                if (!syncEvent.WaitOne(5 * 1000, false)) /// Wait some time for the event to be fired.
                {
                    result = MFTestResults.Skip;
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception", ex);
                result = MFTestResults.Fail;
            }
            finally
            {
                TimeService.TimeSyncFailed -= handler;
            }

            return result;
        }

        void TimeService_TimeSyncFailed(object sender, TimeSyncFailedEventArgs e)
        {
            syncEvent.Set();
        }

        void TimeService_SystemTimeChanged(object sender, SystemTimeChangedEventArgs e)
        {
            timerEvent.Set();
        }

        [TestMethod]
        public MFTestResults TimeServiceSetUTCTimeTest0()
        {
            MFTestResults result = MFTestResults.Pass;

            try
            {
                TimeService.SetUtcTime(119600064000000000); /// This is 1/1/1980:00:00:00.000. 

                DateTime now = DateTime.UtcNow;
                if ((now.Day != 1) || (now.Month != 1) || (now.Year != 1980))
                {
                    throw new ArgumentException("SetUtcTime could not set time correctly.");
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception", ex);
                result = MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults TimeServiceSetTimezoneTest0()
        {
            MFTestResults result = MFTestResults.Pass;

            try
            {
                TimeService.SetUtcTime(119600064000000000); /// This is 1/1/1980:00:00:00.000. 
                TimeService.SetTimeZoneOffset(-480);  /// Setting the time zone -8 hours off GMT, so it should be 12/31/1979 there.                                                          

                DateTime now = DateTime.Now;
                if ((now.Day != 31) || (now.Month != 12) || (now.Year != 1979))
                {
                    throw new ArgumentException("SetUtcTime could not set time correctly.");
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception", ex);
                result = MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults TimeServiceStartTest0()
        {
            MFTestResults result = MFTestResults.Pass;
            SystemTimeChangedEventHandler handler = new SystemTimeChangedEventHandler(TimeService_SystemTimeChanged);
            try
            {
                timerEvent.Reset();

                TimeService.SystemTimeChanged += handler;

                TimeServiceSettings settings = new TimeServiceSettings();
                settings.PrimaryServer = GetTimeServiceAddress().GetAddressBytes();
                settings.Tolerance = 100;
                settings.RefreshTime = 60;

                /// Save the settings.
                TimeService.Settings = settings;
                TimeService.Start(); /// This should fire a sync event right away.

                if (!timerEvent.WaitOne(60 * 1000, false))
                {
                    result = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception", ex);
                result = MFTestResults.Fail;
            }
            finally
            {
                TimeService.SystemTimeChanged -= handler;
                TimeService.Stop();
            }

            return result;
        }

        [TestMethod]
        public MFTestResults TimeServiceStartTest1()
        {
            MFTestResults result = MFTestResults.Pass;
            
            try
            {
                DateTime now = DateTime.Now;

                /// EBS bug returns value in local time instead of UTC.
                /// Remove following line once they fixed the bug.
                now = now.AddHours(12);

                TimeService.SetUtcTime(119600064000000000); /// 1/1/1980.
                /// 
                TimeServiceSettings settings = new TimeServiceSettings();
                settings.PrimaryServer = GetTimeServiceAddress().GetAddressBytes();
                settings.Tolerance = 100;
                settings.RefreshTime = 60;                

                /// Save the settings.
                TimeService.Settings = settings;
                TimeService.Start(); /// This should fire a sync event right away.

                Thread.Sleep(5 * 1000); /// Sleep for some time, we should have time synced by now.
                                         
                if (now.Year == 1980)
                {
                    throw new ArgumentException("Time not synced correctly.");
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception", ex);
                result = MFTestResults.Fail;
            }
            finally
            {
                TimeService.Stop();
            }

            return result;
        }

        [TestMethod]
        public MFTestResults TimeServiceLastSyncStatusTest0()
        {
            MFTestResults result = MFTestResults.Pass;

            try
            {
                DateTime now = DateTime.Now;

                /// EBS bug returns value in local time instead of UTC.
                /// Remove following line once they fixed the bug.
                now = now.AddHours(12);

                TimeService.SetUtcTime(119600064000000000); /// 1/1/1980.
                /// 
                TimeServiceSettings settings = new TimeServiceSettings();
                settings.PrimaryServer = GetTimeServiceAddress().GetAddressBytes();
                settings.Tolerance = 100;
                settings.RefreshTime = 60;
                TimeServiceStatus status = null;

                timerEvent.Reset();

                TimeService.SystemTimeChanged += new SystemTimeChangedEventHandler( TimeService_SystemTimeChanged );

                /// Save the settings.
                TimeService.Settings = settings;
                TimeService.Start(); /// This should fire a sync event right away.

                timerEvent.WaitOne( 5000, false );
                /// 
                for (int i = 0; i < 10; i++)
                {
                    status = TimeService.LastSyncStatus;

                    if (status.Flags == TimeServiceStatus.TimeServiceStatusFlags.SyncSucceeded)
                    {
                        break;
                    }

                    timerEvent.WaitOne( 1000, false );
                }

                if (status.CurrentTimeUTC.Year != DateTime.UtcNow.Year)
                {
                    throw new ArgumentException("Time not synced correctly.");
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception", ex);
                result = MFTestResults.Fail;
            }
            finally
            {
                TimeService.SystemTimeChanged -= new SystemTimeChangedEventHandler( TimeService_SystemTimeChanged );

                TimeService.Stop();
            }

            return result;
        }
         
    }
}
