using System;
using System.Net;
using System.Threading;
using System.IO.Ports;

using Microsoft.SPOT;
using Microsoft.SPOT.Net.NetworkInformation;
using Microsoft.SPOT.Input;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Hardware;

//using ChipworkX.Hardware;


namespace Microsoft.SPOT.Samples.SaveMyWine
{
    public class WineMonitorView : View
    {
        private Text _currentTemperatureText;
        private Text _currentHumidityText;
        private Image _antennaImage;

        private Text _lastUpdateTimeValueText;

        private Text _minimumTemperatureText;
        private Text _maximumTemperatureText;

        private Text _minimumHumidityText;
        private Text _maximumHumidityText;

        private UIButton _resetButton;
        private UIButton _settingsButton;
        private UIButton _updateButton;
        private UIButton _alarmButton;

        DispatcherTimer _antennaAnimationTimer;
        int _animationStep;
 
        //--//

        public WineMonitorView( WineDataModel model )
            : base(model)
        {
            _animationStep = 0;

            // UI aspect 
            // 
            //     -----------------------------------
            //    |             title                 |
            //    |-----------------------------------
            //    |  old wine   |  T |/| H  | antenna |
            //    |             |   history           |
            //    |-----------------------------------
            //    |  reset        update     alarm    |            
            //     -----------------------------------
            //

            ////////////////////////////////////////////////////////////////////////////////////////
            // base stack panel
            // it will be divided vertically in three areas: title, picture and data, buttons
            //
            StackPanel basePanel = new StackPanel(Orientation.Vertical);
            basePanel.HorizontalAlignment = HorizontalAlignment.Center;
            basePanel.VerticalAlignment = VerticalAlignment.Center;

            ////////////////////////////////////////////////////////////////////////////////////////
            // top most area: title area
            //
            Text titleText = new Text(Resources.GetFont(Resources.FontResources.nina48), Resources.GetString(Resources.StringResources.Title));
            titleText.HorizontalAlignment = HorizontalAlignment.Center;
            titleText.VerticalAlignment = VerticalAlignment.Center;

            basePanel.Children.Add(titleText);
            //
            ////////////////////////////////////////////////////////////////////////////////////////

            ////////////////////////////////////////////////////////////////////////////////////////
            // middle area: picture and messages
            //
            StackPanel midPanel = new StackPanel(Orientation.Horizontal);
            midPanel.HorizontalAlignment = HorizontalAlignment.Center;
            midPanel.VerticalAlignment = VerticalAlignment.Center;

            StackPanel midLeftPanel = new StackPanel(Orientation.Vertical);
            midLeftPanel.HorizontalAlignment = HorizontalAlignment.Center;
            midLeftPanel.VerticalAlignment = VerticalAlignment.Center;

            StackPanel midRightPanel = new StackPanel(Orientation.Vertical);
            midRightPanel.HorizontalAlignment = HorizontalAlignment.Center;
            midRightPanel.VerticalAlignment = VerticalAlignment.Center;

            Image wineImage = new Image(Resources.GetBitmap(Resources.BitmapResources.OldWine));
            wineImage.SetMargin(0, 5, 20, 5);

            StackPanel midRightSplitPanel = new StackPanel(Orientation.Horizontal);
            midRightSplitPanel.HorizontalAlignment = HorizontalAlignment.Center;
            midRightSplitPanel.VerticalAlignment = VerticalAlignment.Center;

            StackPanel midRightDataPanel = new StackPanel(Orientation.Horizontal);
            midRightDataPanel.HorizontalAlignment = HorizontalAlignment.Center;
            midRightDataPanel.VerticalAlignment = VerticalAlignment.Center;

            StackPanel temperatureRangePanel = new StackPanel(Orientation.Horizontal);
            temperatureRangePanel.HorizontalAlignment = HorizontalAlignment.Left;
            temperatureRangePanel.VerticalAlignment = VerticalAlignment.Center;

            StackPanel humidityRangePanel = new StackPanel(Orientation.Horizontal);
            humidityRangePanel.HorizontalAlignment = HorizontalAlignment.Left;
            humidityRangePanel.VerticalAlignment = VerticalAlignment.Center;

            StackPanel lastUpdateTimePanel = new StackPanel(Orientation.Horizontal);
            lastUpdateTimePanel.HorizontalAlignment = HorizontalAlignment.Left;
            lastUpdateTimePanel.VerticalAlignment = VerticalAlignment.Center;

            _currentTemperatureText = new Text(Resources.GetFont(Resources.FontResources.nina48), "60");
            _currentTemperatureText.SetMargin(3, 0, 3, 0);
            //currentTemperatureText.TextAlignment = TextAlignment.Center;
            Text divisorCurrentText = new Text(Resources.GetFont(Resources.FontResources.nina48), Resources.GetString(Resources.StringResources.Divisor));
            divisorCurrentText.SetMargin(3, 0, 3, 0);
            _currentHumidityText = new Text(Resources.GetFont(Resources.FontResources.nina48), "50");
            _currentHumidityText.SetMargin(3, 0, 3, 0);
            //currentHumidityText.TextAlignment = TextAlignment.Center;

            Text temperatureRangeText = new Text(Resources.GetFont(Resources.FontResources.nina14), Resources.GetString(Resources.StringResources.TemperatureRange));
            temperatureRangeText.SetMargin(3, 0, 3, 0);

            _minimumTemperatureText = new Text();
            _minimumTemperatureText.Font = Resources.GetFont(Resources.FontResources.nina14);
            _minimumTemperatureText.SetMargin(3, 0, 3, 0);
            Text divisorTemperatureText = new Text(Resources.GetFont(Resources.FontResources.nina14), Resources.GetString(Resources.StringResources.Divisor));
            divisorTemperatureText.SetMargin(3, 0, 3, 0);
            _maximumTemperatureText = new Text();
            _maximumTemperatureText.Font = Resources.GetFont(Resources.FontResources.nina14);
            _maximumTemperatureText.SetMargin(3, 0, 3, 0);

            Text humidityRangeText = new Text(Resources.GetFont(Resources.FontResources.nina14), Resources.GetString(Resources.StringResources.HumidityRange));
            humidityRangeText.SetMargin(3, 0, 3, 0);

            _minimumHumidityText = new Text();
            _minimumHumidityText.Font = Resources.GetFont(Resources.FontResources.nina14);
            _minimumHumidityText.SetMargin(3, 0, 3, 0);
            Text divisorHumidityText = new Text(Resources.GetFont(Resources.FontResources.nina14), Resources.GetString(Resources.StringResources.Divisor));
            divisorHumidityText.SetMargin(3, 0, 3, 0);
            _maximumHumidityText = new Text();
            _maximumHumidityText.Font = Resources.GetFont(Resources.FontResources.nina14);
            _maximumHumidityText.SetMargin(3, 0, 3, 0);

            Text lastUpdateTimeText = new Text(Resources.GetFont(Resources.FontResources.nina14), Resources.GetString(Resources.StringResources.LastUpdateTime));
            lastUpdateTimeText.SetMargin(3, 0, 3, 0);

            Text IPAddressText = new Text();
            IPAddressText.SetMargin(3, 0, 3, 0);
            IPAddressText.Font = Resources.GetFont(Resources.FontResources.nina14);
            string ipaddress = Resources.GetString(Resources.StringResources.IPAddress);

            try
            {

                NetworkInterface[] nis = NetworkInterface.GetAllNetworkInterfaces();

                if (nis.Length > 0)
                {
                    bool isIPValid = false;
                    int index = 0;

                    IPAddress zeroIP = new IPAddress(new byte[] { 0x00, 0x00, 0x00, 0x00 });

                    foreach (NetworkInterface ni in nis)
                    {
                        IPAddress addr = IPAddress.Parse(ni.IPAddress);

                        if (!addr.Equals(zeroIP))
                        {
                            isIPValid = true;

                            break;
                        }

                        ++index;
                    }

                    if (isIPValid)
                    {
                        ipaddress += nis[index].IPAddress;
                    }
                    else
                    {
                        ipaddress += zeroIP.ToString();
                    }
                }
                else
                {
                    ipaddress += "No Network Interface";
                }

            }
            catch(Exception)
            {
                Debug.Print( "Exception in retriving network information" );
            }

            IPAddressText.TextContent = ipaddress;

            _lastUpdateTimeValueText = new Text();
            _lastUpdateTimeValueText.Font = Resources.GetFont(Resources.FontResources.nina14);
            _lastUpdateTimeValueText.SetMargin(3, 0, 3, 0);

            _antennaImage = new Image(Resources.GetBitmap(Resources.BitmapResources.Antenna0));
            _antennaImage.SetMargin(10, 5, 5, 10);

            midLeftPanel.Children.Add(wineImage);

            midRightDataPanel.Children.Add(_currentTemperatureText);
            midRightDataPanel.Children.Add(divisorCurrentText);
            midRightDataPanel.Children.Add(_currentHumidityText);

            midRightSplitPanel.Children.Add(midRightDataPanel);
            midRightSplitPanel.Children.Add(_antennaImage);

            midRightPanel.Children.Add(midRightSplitPanel);

            temperatureRangePanel.Children.Add(temperatureRangeText);
            temperatureRangePanel.Children.Add(_minimumTemperatureText);
            temperatureRangePanel.Children.Add(divisorTemperatureText);
            temperatureRangePanel.Children.Add(_maximumTemperatureText);

            humidityRangePanel.Children.Add(humidityRangeText);
            humidityRangePanel.Children.Add(_minimumHumidityText);
            humidityRangePanel.Children.Add(divisorHumidityText);
            humidityRangePanel.Children.Add(_maximumHumidityText);

            lastUpdateTimePanel.Children.Add(lastUpdateTimeText);
            lastUpdateTimePanel.Children.Add(_lastUpdateTimeValueText);

            midRightPanel.Children.Add(temperatureRangePanel);
            midRightPanel.Children.Add(humidityRangePanel);
            midRightPanel.Children.Add(lastUpdateTimePanel);
            midRightPanel.Children.Add(IPAddressText);

            midPanel.SetMargin(0, 0, 0, 10);
            midPanel.Children.Add(midLeftPanel);
            midPanel.Children.Add(midRightPanel);

            basePanel.Children.Add(midPanel);
            //
            ////////////////////////////////////////////////////////////////////////////////////////

            ////////////////////////////////////////////////////////////////////////////////////////
            // bottom area: buttons
            //        
            StackPanel bottomPanel = new StackPanel(Orientation.Horizontal);
            bottomPanel.HorizontalAlignment = HorizontalAlignment.Center;
            bottomPanel.VerticalAlignment = VerticalAlignment.Center;
            bottomPanel.SetMargin(0, 5, 0, 5);

            _resetButton = new UIButton("Reset", Resources.GetFont(Resources.FontResources.nina14), 100, 35);
            _resetButton.Click += new EventHandler(reset_Click);

            _settingsButton = new UIButton("Settings", Resources.GetFont(Resources.FontResources.nina14), 100, 35);
            _settingsButton.Click += new EventHandler(settings_Click);

            _updateButton = new UIButton("Update", Resources.GetFont(Resources.FontResources.nina14), 100, 35);
            _updateButton.Click += new EventHandler(update_Click);
            _updateButton.IsEnabled = false;

            _alarmButton = new UIButton(Resources.GetString(Resources.StringResources.AlarmOn), Resources.GetFont(Resources.FontResources.nina14), 100, 35);
            _alarmButton.Click += new EventHandler(alarm_Click);
            _alarmButton.IsEnabled = false;

            bottomPanel.Children.Add(_resetButton);
            bottomPanel.Children.Add(_settingsButton);
            bottomPanel.Children.Add(_updateButton);
            bottomPanel.Children.Add(_alarmButton);

            basePanel.Children.Add(bottomPanel);
            //
            ////////////////////////////////////////////////////////////////////////////////////////

            InitializeData(null);

            Child = basePanel;

            //
            ////////////////////////////////////////////////////////////////////////////////////////

            model.OnRadioStateChanged += new WineDataModel.RadioStateChangedEventHandler(RadioStateChanged);
            model.OnTemperatureChanged += new WineDataModel.TemperatureChangedEventHandler(TemperatureChanged);
            model.OnHumidityChanged += new WineDataModel.HumidityChangedEventHandler(HumidityChanged);
            model.OnTemperatureThresholdExceeded += new WineDataModel.TemperatureThresholdExceededEventHandler(TemperatureThresholdExceeded);
            model.OnHumidityThresholdExceeded += new WineDataModel.HumidityThresholdExceededEventHandler(HumidityThresholdExceeded);

            _antennaAnimationTimer = new DispatcherTimer(_antennaImage.Dispatcher);
            _antennaAnimationTimer.Interval = new TimeSpan(0, 0, 0, 0, 150);
        }

        private void InitializeData(object args)
        {
            _currentTemperatureText.TextContent = Resources.GetString(Resources.StringResources.NoData);
            _currentHumidityText.TextContent = Resources.GetString(Resources.StringResources.NoData);
            _minimumTemperatureText.TextContent = WineDataModel.MinimumTemperatureAllowed.ToString();
            _maximumTemperatureText.TextContent = WineDataModel.MaximumTemperatureAllowed.ToString();

            _minimumHumidityText.TextContent = WineDataModel.MinimumHumidityAllowed.ToString();
            _maximumHumidityText.TextContent = WineDataModel.MaximumHumidityAllowed.ToString();

            _lastUpdateTimeValueText.TextContent = Resources.GetString(Resources.StringResources.NoData);
        }

        //--//

        #region button handlers

        void reset_Click(object sender, EventArgs e)
        {
            InvokeUserRequest(this, new UserRequestEvent(UserRequestEvent.UserRequest.Reset));
        }

        void settings_Click(object sender, EventArgs e)
        {
            InvokeUserRequest(this, new UserRequestEvent(UserRequestEvent.UserRequest.ChangeSettings));
        }

        void update_Click(object sender, EventArgs e)
        {
            InvokeUserRequest(this, new UserRequestEvent(UserRequestEvent.UserRequest.UpdateSensorData));
        }

        void alarm_Click(object sender, EventArgs e)
        {
            UIButton button = (UIButton)sender;

            _alarmButton.ButtonCaption(Resources.GetString(Resources.StringResources.AlarmOn));
            _alarmButton.IsEnabled = false;
            
            InvokeUserRequest(this, new UserRequestEvent(UserRequestEvent.UserRequest.SilenceAlarm));
        }

        #endregion 

        #region radio state handlers

        private void RadioStateChanged(object sender, WineDataModel.RadioStateChangedEvent e)
        {
            _antennaAnimationTimer.Stop();

            switch (e.State)
            {
                case RadioState.Disconnected:
                    _antennaImage.Dispatcher.BeginInvoke(new DispatcherOperationCallback(RadioDisconnected), null);
                    _updateButton.Dispatcher.BeginInvoke(new DispatcherOperationCallback(EnableUpdateButton), false);
                    break;
                case RadioState.Initializing:
                    _antennaAnimationTimer.Tick += new EventHandler(RadioAnimation);
                    _updateButton.Dispatcher.BeginInvoke(new DispatcherOperationCallback(EnableUpdateButton), false);
                    _antennaAnimationTimer.Start();
                    break;
                case RadioState.InitializationFailed:
                    _antennaImage.Dispatcher.BeginInvoke(new DispatcherOperationCallback(RadioOff), null);
                    break;
                case RadioState.Initialized:
                    _antennaImage.Dispatcher.BeginInvoke(new DispatcherOperationCallback(RadioInitialized), null);
                    _updateButton.Dispatcher.BeginInvoke(new DispatcherOperationCallback(EnableUpdateButton), true);
                    break;
                case RadioState.Connected:
                    _antennaImage.Dispatcher.BeginInvoke(new DispatcherOperationCallback(RadioConnected), null);
                    break;
                default:
                    break;
            }
        }

        private object RadioOff(object args)
        {
            _antennaImage.Bitmap = Resources.GetBitmap(Resources.BitmapResources.AntennaNoRadio); return null;
        }

        private object RadioDisconnected(object args)
        {
            _antennaImage.Bitmap = Resources.GetBitmap(Resources.BitmapResources.Antenna0); return null;
        }
        private object RadioInitialized(object args)
        {
            _antennaImage.Bitmap = Resources.GetBitmap(Resources.BitmapResources.Antenna4); return null;
        }
        private object RadioConnected(object args)
        {
            _antennaImage.Bitmap = Resources.GetBitmap(Resources.BitmapResources.Antenna4); return null;
        }

        private object EnableUpdateButton(object args)
        {
            _updateButton.IsEnabled = (bool)args; return null;
        }

        private void RadioAnimation(object sender, EventArgs e)
        {
            switch (_animationStep++)
            {
                case 0:
                    _antennaImage.Bitmap = Resources.GetBitmap(Resources.BitmapResources.Antenna0);
                    break;
                case 1:
                    _antennaImage.Bitmap = Resources.GetBitmap(Resources.BitmapResources.Antenna1);
                    break;
                case 2:
                    _antennaImage.Bitmap = Resources.GetBitmap(Resources.BitmapResources.Antenna2);
                    break;
                case 3:
                    _antennaImage.Bitmap = Resources.GetBitmap(Resources.BitmapResources.Antenna3);
                    break;
                case 4:
                    _antennaImage.Bitmap = Resources.GetBitmap(Resources.BitmapResources.Antenna4);
                    break;
                default:
                    _animationStep = 0;
                    break;
            }

            Invalidate();
        }
        #endregion

        #region sensor data handlers

        private void TemperatureChanged(object sender, WineDataModel.SensorDataChangedEvent e)
        {
            UpdateText(_currentTemperatureText, e.Last);

            if (e.IsNewMinimum)
            {
                UpdateText( _minimumTemperatureText, e.Last );
            }
            
            if (e.IsNewMaximum)
            {
                UpdateText(_maximumTemperatureText, e.Last);
            }
        }


        private void HumidityChanged(object sender, WineDataModel.HumidityChangedEvent e)
        {
            UpdateText(_currentHumidityText, e.Last);

            if (e.IsNewMinimum)
            {
                UpdateText(_minimumHumidityText, e.Last);
            }
            
            if (e.IsNewMaximum)
            {
                UpdateText(_maximumHumidityText, e.Last);
            }
        }

        private void UpdateText(Text control, double data)
        {
            control.Dispatcher.BeginInvoke(delegate(object args)
            {
                control.TextContent = data.ToString();  return null;
            }, 
            null);

            _lastUpdateTimeValueText.Dispatcher.BeginInvoke(delegate(object args)
            {
                _lastUpdateTimeValueText.TextContent = DateTime.Now.ToString();  return null;
            }, 
            null);
        }

        private void TemperatureThresholdExceeded(object sender, WineDataModel.TemperatureThresholdExceededEvent e)
        {
            RingAlarm();
        }
        private void HumidityThresholdExceeded(object sender, WineDataModel.HumidityThresholdExceededEvent e)
        {
            RingAlarm();
        }

        private void RingAlarm()
        {
            _alarmButton.Dispatcher.BeginInvoke(delegate(object args)
                                                {
                                                    _alarmButton.ButtonCaption(Resources.GetString(Resources.StringResources.AlarmOff));
                                                    _alarmButton.IsEnabled = true;
                                                    return null;
                                                }, null);
        }

        #endregion
    }
}
