using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Input;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Time;

namespace Clock
{
    public class Program : Microsoft.SPOT.Application
    {
        // In our sample we will use an arbitrary time server with address time-nw.nist.gov,
        // one of the public time servers listed here: http://tf.nist.gov/tf-cgi/servers.cgi
        public static byte[] TimeServerIPAddress = new byte[] { 131, 107, 13, 100 };

        Text time = new Text();

        public static void Main()
        {
            Program myApplication = new Program();

            Window mainWindow = myApplication.CreateWindow();

            // Create the object that configures the GPIO pins to buttons.
            GPIOButtonInputProvider inputProvider = new GPIOButtonInputProvider(null);

            // Start the application
            myApplication.Run(mainWindow);
        }

        private Window mainWindow;
        private DispatcherTimer clockTimer;

        public Window CreateWindow()
        {
            // Create a window object and set its size to the
            // size of the display.
            mainWindow = new Window();
            mainWindow.Height = SystemMetrics.ScreenHeight;
            mainWindow.Width = SystemMetrics.ScreenWidth;

            Text help = new Text();
            help.Font = Resources.GetFont(Resources.FontResources.nina14);
            help.TextContent = "Buttons: UP resets, SEL Syncs, DOWN schedules";
            help.HorizontalAlignment = HorizontalAlignment.Center;
            help.VerticalAlignment = VerticalAlignment.Top;

            time.Font = Resources.GetFont(Resources.FontResources.small);
            time.TextContent = "Initializing...";
            time.HorizontalAlignment = Microsoft.SPOT.Presentation.HorizontalAlignment.Center;
            time.VerticalAlignment = Microsoft.SPOT.Presentation.VerticalAlignment.Center;

            // Add the text controls to the window.
            Panel panel = new Panel();
            panel.Children.Add(help);
            panel.Children.Add(time);
            mainWindow.Child = panel;

            // Connect the button handler to all of the buttons.
            mainWindow.AddHandler(Buttons.ButtonUpEvent, new RoutedEventHandler(OnButtonUp), false);

            // Set the window visibility to visible.
            mainWindow.Visibility = Visibility.Visible;

            clockTimer = new DispatcherTimer(mainWindow.Dispatcher);
            clockTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            clockTimer.Tick += new EventHandler(ClockTimer_Tick);

            // Attach the button focus to the window.
            Buttons.Focus(mainWindow);

            TimeService.SystemTimeChanged += new SystemTimeChangedEventHandler(TimeService_SystemTimeChanged);
            TimeService.TimeSyncFailed += new TimeSyncFailedEventHandler(TimeService_TimeSyncFailed);

            clockTimer.Start();

            return mainWindow;
        }

        void ClockTimer_Tick(object sender, EventArgs evt)
        {
            time.TextContent = DateTime.Now.ToString("F");
            time.Invalidate();
        }

        void TimeService_TimeSyncFailed(object sender, TimeSyncFailedEventArgs evt)
        {
            string errorMsg = "Time Sync Failed with errorCode: " + evt.ErrorCode.ToString();
            Debug.Print(errorMsg);
        }

        void TimeService_SystemTimeChanged(object sender, SystemTimeChangedEventArgs evt)
        {
            Debug.Print("System Time Changed.");
        }

        private void OnButtonUp(object sender, RoutedEventArgs evt)
        {
            ButtonEventArgs e = (ButtonEventArgs)evt;

            const int timeZoneOffsetInMinutes = -8 * 60;

            switch (e.Button)
            {
                case Microsoft.SPOT.Hardware.Button.VK_UP:
                    // Reset the time to an arbitrary value.
                    TimeService.SetUtcTime(128752416000000000);
                    TimeService.SetTimeZoneOffset(timeZoneOffsetInMinutes);
                    break;

                case Microsoft.SPOT.Hardware.Button.VK_SELECT:
                    // Perform a one time sync with the time server.
                    TimeServiceStatus status = TimeService.UpdateNow(TimeServerIPAddress, 10);
                    TimeService.SetTimeZoneOffset(timeZoneOffsetInMinutes);
                    break;

                case Microsoft.SPOT.Hardware.Button.VK_DOWN:
                    // Start a scheduled periodic sync.
                    TimeServiceSettings settings = new TimeServiceSettings();

                    // Sync every 10 seconds.
                    settings.PrimaryServer = TimeServerIPAddress;
                    settings.RefreshTime = 10;

                    TimeService.Settings = settings;

                    TimeService.Start();
                    TimeService.SetTimeZoneOffset(timeZoneOffsetInMinutes);
                    break;
            }
        }
    }
}
