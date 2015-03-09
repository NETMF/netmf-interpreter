//#define TIME_SYNC

using System;
using System.Threading;

using Microsoft.SPOT.Time;

//--//

namespace Microsoft.SPOT.Samples.SaveMyWine
{
    public class TimeSync 
    {
        // (accessing a public time server will require adequate proxy settings though)
        public static byte[] TimeServerIPAddress = new byte[] { 10, 192, 53, 107 };

        //--//

        public TimeSync()
        {
#if TIME_SYNC
            TimeServiceSettings settings = new TimeServiceSettings();

            settings.PrimaryServer = TimeServerIPAddress;
            settings.RefreshTime = 10; // sycn every 10 seconds

            TimeService.Settings = settings;

            TimeService.SystemTimeChanged += new SystemTimeChangedEventHandler(TimeService_SystemTimeChanged);
            TimeService.TimeSyncFailed += new TimeSyncFailedEventHandler(TimeService_TimeSyncFailed);
#endif
        }

        public void Start()
        {
#if TIME_SYNC
            TimeService.Start();
#endif
        }

        //--//

        private void TimeService_TimeSyncFailed(object sender, TimeSyncFailedEventArgs e)
        {
            string errorMsg = "Time Sync Failed with errorCode: " + e.ErrorCode.ToString();
            Debug.Print(errorMsg);
        }

        private void TimeService_SystemTimeChanged(object sender, SystemTimeChangedEventArgs e)
        {
            Debug.Print("System Time Changed.");
        }
    }
}