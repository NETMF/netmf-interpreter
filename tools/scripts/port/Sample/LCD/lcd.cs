using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Input;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Presentation.Controls;

namespace Microsoft.SPOT.Sample.LCD
{
    //public class Program : Microsoft.SPOT.Application
    public class LCD
    {
        public static void Main()
        {
            LCD myApplication = new LCD();

            Window mainWindow = myApplication.CreateWindow();
            
            // Start the application
            //myApplication.Run(mainWindow);

            Bitmap bitmap1 = new Bitmap(SystemMetrics.ScreenWidth,SystemMetrics.ScreenHeight);
            Bitmap bitmap2 = new Bitmap(SystemMetrics.ScreenWidth, SystemMetrics.ScreenHeight);

            int ox = 0;
            int oy = 0;
            int rx = bitmap1.Width;
            int ry = bitmap1.Height;
            int sd=0;
            if (rx < ry)
                sd = rx;
            else
                sd = ry;


            bitmap1.DrawLine(Colors.Blue, 1, 0, 0, rx, ry);
            bitmap1.DrawLine(Colors.Blue, 1, rx, 0, 0, ry);
            bitmap1.DrawRectangle(Color.White, 10, ox, oy, rx, ry, 1, 1, 
                Color.White, ox, oy, Color.White, rx, ry, 0xFF);

            for(int i=0; i < sd; i += 10)
                bitmap1.DrawRectangle(Color.White, 1, ox+i, oy+i, rx-i, ry-i, 1, 1, 
                    Colors.Green, ox, oy, Colors.Green, rx, ry, 0xFF);

            
            bitmap2.DrawLine(Colors.Black, 1, 0, 0, rx, ry);
            bitmap2.DrawLine(Microsoft.SPOT.Presentation.Media.Color.Black, 1, rx, 0, 0, ry);

            for (int i = 0; i < sd; i += 10)
                bitmap1.DrawRectangle(Color.White, 1, ox + i, oy + i, rx - i, ry - i, 1, 1,
                    Colors.Green, ox, oy, Colors.Green, rx, ry, 0xFF);

            while (true)
            {
                bitmap1.Flush();
                Thread.Sleep(50);
                bitmap2.Flush();
                Thread.Sleep(50);
            }

        }

        private Window mainWindow;

        public Window CreateWindow()
        {
            // Create a window object and set its size to the
            // size of the display.
            mainWindow = new Window();
            mainWindow.Height = SystemMetrics.ScreenHeight;
            mainWindow.Width = SystemMetrics.ScreenWidth;         

            // Set the window visibility to visible.
            mainWindow.Visibility = Visibility.Visible;

            return mainWindow;
        }

        private void OnButtonUp(object sender, ButtonEventArgs e)
        {
            // Print the button code to the Visual Studio output window.
            //Debug.Print(e.Button.ToString());
        }
    }
}
