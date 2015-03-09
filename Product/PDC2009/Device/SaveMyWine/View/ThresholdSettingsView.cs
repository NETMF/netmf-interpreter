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

    public class ThresholdSettingsView : View
    {
        UpDownArrowButton _temperatureMinimum;
        UpDownArrowButton _temperatureMaximum;
        UpDownArrowButton _humidityMinimum;
        UpDownArrowButton _humidityMaximum;

        //--//

        public ThresholdSettingsView(WineDataModel model)
            : base(model)
        {
            // UI aspect 
            // 
            //     -----------------------------------
            //    |   Temperature   |   Humidity      |
            //    |-----------------------------------
            //    |        |        |        |        |
            //    |        |        |        |        |
            //     -----------------------------------
            //    |     Monitor     |     Commit      |
            //     -----------------------------------
            //

            Font f = Resources.GetFont(Resources.FontResources.nina14);

            StackPanel basePanel = new StackPanel(Orientation.Vertical);
            basePanel.HorizontalAlignment = HorizontalAlignment.Center;
            basePanel.VerticalAlignment = VerticalAlignment.Center;

            StackPanel regulatorsPanel = new StackPanel(Orientation.Horizontal);

            StackPanel temperaturePanel = new StackPanel(Orientation.Vertical);
            temperaturePanel.HorizontalAlignment = HorizontalAlignment.Center;
            temperaturePanel.VerticalAlignment = VerticalAlignment.Center;
            Text temperatureTitle = new Text(f, "Temperature");
            //temperatureTitle.TextAlignment = TextAlignment.Center;
            temperaturePanel.Children.Add(temperatureTitle);

            StackPanel humidityPanel = new StackPanel(Orientation.Vertical);
            humidityPanel.HorizontalAlignment = HorizontalAlignment.Center;
            humidityPanel.VerticalAlignment = VerticalAlignment.Center;
            Text humidityTitle = new Text(f, "Humidity");
            //humidityTitle.TextAlignment = TextAlignment.Center;
            humidityPanel.Children.Add(humidityTitle);

            /////////////////////////////////////////////////////////////////////////////

            StackPanel temperatureRegulatorsPanel = new StackPanel(Orientation.Horizontal);
            //temperatureRegulatorsPanel.SetMargin(20, 10, 20, 10);

            _temperatureMinimum = new UpDownArrowButton((int)WineDataModel.MinimumTemperatureAllowed, 0, 99);
            _temperatureMaximum = new UpDownArrowButton((int)WineDataModel.MaximumTemperatureAllowed, 0, 99);
            temperatureRegulatorsPanel.Children.Add(_temperatureMinimum);
            temperatureRegulatorsPanel.Children.Add(_temperatureMaximum);

            temperaturePanel.Children.Add(temperatureRegulatorsPanel);

            /////////////////////////////////////////////////////////////////////////////

            StackPanel humidityRegulatorsPanel = new StackPanel(Orientation.Horizontal);
            //humidityRegulatorsPanel.SetMargin(20, 10, 20, 10);

            _humidityMinimum = new UpDownArrowButton((int)WineDataModel.MinimumHumidityAllowed, 0, 99);
            _humidityMaximum = new UpDownArrowButton((int)WineDataModel.MaximumHumidityAllowed, 0, 99);
            humidityRegulatorsPanel.Children.Add(_humidityMinimum);
            humidityRegulatorsPanel.Children.Add(_humidityMaximum);

            humidityPanel.Children.Add(humidityRegulatorsPanel);

            //////////////////////////////////////////////////////////////////////////////

            Panel padding = new Panel();
            padding.SetMargin(10, 0, 10, 0);

            regulatorsPanel.Children.Add(temperaturePanel);
            regulatorsPanel.Children.Add(padding);
            regulatorsPanel.Children.Add(humidityPanel);

            basePanel.Children.Add(regulatorsPanel);

            //////////////////////////////////////////////////////////////////////////////

            StackPanel buttonsPanel = new StackPanel(Orientation.Horizontal);
            buttonsPanel.HorizontalAlignment = HorizontalAlignment.Center;
            buttonsPanel.VerticalAlignment = VerticalAlignment.Center;
            buttonsPanel.SetMargin(0, 10, 0, 10);

            UIButton back = new UIButton("Back to Monitor", f);
            back.Click += new EventHandler(OnBack);

            buttonsPanel.Children.Add(back);

            basePanel.Children.Add(buttonsPanel);

            Child = basePanel;
        }

        //--//

        private void OnBack(object sender, EventArgs e)
        {
            InvokeUserRequest(this,
                new UserRequestEvent(UserRequestEvent.UserRequest.SaveSettings,
                    new Range[] { 
                        new Range(_temperatureMinimum.Position, _temperatureMaximum.Position), 
                        new Range(_humidityMinimum.Position, _humidityMaximum.Position)
                                }));
        }
    }
}
