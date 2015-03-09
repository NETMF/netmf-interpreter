////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using System.Collections;
using System.Runtime.CompilerServices;

using Microsoft.SPOT;
using Microsoft.SPOT.Input;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;

namespace Microsoft.SPOT.Sample
{
    public class SampleApplication : Microsoft.SPOT.Application
    {
        public static void Main()
        {
            GPIOButtonInputProvider inputProvider = new GPIOButtonInputProvider(null);

            SampleApplication myApplication = new SampleApplication();

            Window mainWindow = myApplication.CreateUI();
            mainWindow.Background = new SolidColorBrush(Colors.Black);
            mainWindow.Visibility = Visibility.Visible;

            myApplication.Run(mainWindow);
        }

        public Window CreateUI()
        {

            Window      window       = new Window();
            StackPanel  panel        = new StackPanel();
            panel.Orientation        = Orientation.Vertical;

            Text   text         = new Text();
            text.Font           = Resources.GetFont(Resources.FontResources.FONT);
            text.TextContent    = "Hello";
            text.ForeColor      = Colors.Red;
            text.HorizontalAlignment = Microsoft.SPOT.Presentation.HorizontalAlignment.Left;
            text.VerticalAlignment   = Microsoft.SPOT.Presentation.VerticalAlignment.Stretch;

            panel.Children.Add(text);

            text                = new Text();
            text.Font           = Resources.GetFont(Resources.FontResources.FONT);
            text.TextContent    = "comma";
            text.ForeColor      = Colors.Green;
            text.HorizontalAlignment = Microsoft.SPOT.Presentation.HorizontalAlignment.Center;
            text.VerticalAlignment   = Microsoft.SPOT.Presentation.VerticalAlignment.Stretch;

            panel.Children.Add(text);

            text                = new Text();
            text.Font           = Resources.GetFont(Resources.FontResources.FONT);
            text.TextContent    = "World!";
            text.ForeColor      = Colors.Blue;
            text.HorizontalAlignment = Microsoft.SPOT.Presentation.HorizontalAlignment.Right;
            text.VerticalAlignment   = Microsoft.SPOT.Presentation.VerticalAlignment.Stretch;

            panel.Children.Add(text);            

            TextFlow textFlow = new TextFlow();
            textFlow.Height = 60;            
            textFlow.TextRuns.Add("[BEGIN] ", Resources.GetFont(Resources.FontResources.FONT), Colors.Green);
            textFlow.TextRuns.Add("Call me Ishmael. ", Resources.GetFont(Resources.FontResources.FONT), Colors.Gray);
            textFlow.TextRuns.Add("Some years ago - never mind how long precisely - having little or no money in my purse, and nothing particular to interest me on shore, I thought I would sail about a little and see the watery part of the world. ", Resources.GetFont(Resources.FontResources.FONT), Colors.Gray);
            textFlow.TextRuns.Add("It is a way I have of driving off the spleen, and regulating the circulation. ", Resources.GetFont(Resources.FontResources.FONT), Colors.Gray);
            textFlow.TextRuns.Add("A_very_long_sentence_with_no_whitespace_that_should_trigger_an_emergency_break. ", Resources.GetFont(Resources.FontResources.FONT), Colors.Blue);
            textFlow.TextRuns.Add("[END] ", Resources.GetFont(Resources.FontResources.FONT), Colors.Green);

            panel.Children.Add(textFlow);

            text = new Text();
            text.Font = Resources.GetFont(Resources.FontResources.FONT);
            text.TextContent = "";
            text.ForeColor = Colors.Gray;
            text.HorizontalAlignment = Microsoft.SPOT.Presentation.HorizontalAlignment.Center;
            text.VerticalAlignment = Microsoft.SPOT.Presentation.VerticalAlignment.Stretch;

            panel.Children.Add(text);

            _buttonText = text;

            text = new Text();
            text.Font = Resources.GetFont(Resources.FontResources.FONT);
            text.TextContent = "";
            text.ForeColor = Colors.Gray;
            text.HorizontalAlignment = Microsoft.SPOT.Presentation.HorizontalAlignment.Center;
            text.VerticalAlignment = Microsoft.SPOT.Presentation.VerticalAlignment.Stretch;

            panel.Children.Add(text);

            _timeText = text;            

            window.Child        = panel;

            window.AddHandler(Buttons.ButtonDownEvent, new RoutedEventHandler(OnButtonDown), false);
            window.AddHandler(Buttons.GotFocusEvent, new RoutedEventHandler(OnGotFocus), false);

            window.Width = SystemMetrics.ScreenWidth;
            window.Height = SystemMetrics.ScreenHeight;
            window.Visibility = Visibility.Visible;

            // Buttons.Focus(window);

            Buttons.Focus(textFlow);

            return window;
        }

        private Text _buttonText;
        private Text _timeText;

        private void OnButtonDown(object sender, RoutedEventArgs evt)
        {
            ButtonEventArgs e = (ButtonEventArgs)evt;
            _timeText.TextContent = (((DateTime.UtcNow - e.Timestamp).Ticks) / TimeSpan.TicksPerMillisecond).ToString() + "ms";
            _buttonText.TextContent = e.RoutedEvent.Name + " : " + buttonNames[(int)e.Button];
        }

        private void OnGotFocus(object sender, RoutedEventArgs evt)
        {
            FocusChangedEventArgs e = (FocusChangedEventArgs)evt;
            _timeText.TextContent = (((DateTime.UtcNow - e.Timestamp).Ticks) / TimeSpan.TicksPerMillisecond).ToString() + "ms";
            _buttonText.TextContent = e.RoutedEvent.Name;
        }

        private string[] buttonNames = new string[] { 
            "None", "Menu", "Select", "Up", "Down", "Left", "Right"
    };
    }
}
