using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Input;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Presentation.Controls;

namespace Microsoft.SPOT.Test
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


	    //
	    // Draw Diagonals
	    //
            bitmap1.DrawLine(Colors.Blue, 3, 0, 0, rx, ry);
            bitmap1.DrawLine(Colors.Blue, 3, rx, 0, 0, ry);

            bitmap2.DrawLine(Colors.Red, 3, 0, 0, rx, ry);
            bitmap2.DrawLine(Colors.Red, 3, rx, 0, 0, ry);


            for(int ioff=0; ioff < sd; ioff += 10)
	    {
		int oX  = ox+ioff;
		int oY  = oy+ioff;
		int wX  = rx - 2*ioff;
		int wY  = ry - 2*ioff;
                bitmap1.DrawRectangle(Colors.White, 1, oX, oY, wX, wY, 1, 1, 
                    Colors.Green, ox, oy, Colors.Green, rx, ry, 0xFF);
                bitmap2.DrawEllipse(Colors.White, rx/2, ry/2, wX, wY);
	    }

            


            while (true)
            {
                bitmap1.Flush();
                Thread.Sleep(250);
                bitmap2.Flush();
                Thread.Sleep(200);
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
