using System;
using System.Threading;
using System.IO.Ports;

using Microsoft.SPOT;
using Microsoft.SPOT.Input;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Hardware;

using Microsoft.SPOT.Samples.SaveMyWine.Controls;

//using ChipworkX.Hardware;



namespace Microsoft.SPOT.Samples.SaveMyWine
{
    public class Program : Microsoft.SPOT.Application
    {
        private Window mainWindow;

        //--//

        public static void Main()
        {
            Program myApplication = new Program();

            //ChipworkX.System.Net.Interface.Set(ChipworkX.System.Net.NetworkingInterfaces.ZG2100);

            WineDataModel model = new WineDataModel();

            WineMonitorView monitorView = new WineMonitorView(model);
            ThresholdSettingsView settingsView = new ThresholdSettingsView(model);

            WineServiceConnection dpws = new WineServiceConnection(model);

            Window mainWindow = myApplication.CreateWindow(monitorView);

            WineController controller = new WineController(myApplication, model, monitorView, settingsView, dpws );


            Microsoft.SPOT.Touch.Touch.Initialize(myApplication);

            myApplication.Run(mainWindow);

            controller.Shutdown();
        }

        public void ChangeView( View view )
        {
            mainWindow.Child = view;
        }

        //--//

        private Window CreateWindow( UIElement view )
        {
            mainWindow = new Window();
            mainWindow.Height = SystemMetrics.ScreenHeight;
            mainWindow.Width = SystemMetrics.ScreenWidth;
            mainWindow.Background = new LinearGradientBrush(ColorUtility.ColorFromRGB(125, 0, 80), ColorUtility.ColorFromRGB(255, 255, 255));

            mainWindow.Child = view;
            mainWindow.Visibility = Visibility.Visible;

            return mainWindow;
        }

    }
}