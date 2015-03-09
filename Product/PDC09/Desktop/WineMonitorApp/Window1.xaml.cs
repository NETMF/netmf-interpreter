using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;

namespace WineMonitorApp
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        delegate void UpdateGraphDelegate();
        delegate void UpdateSensorData();

        Timer m_timer, m_updateTheshTimer;
        WineSensorThreshold m_currentThresholds;
        bool m_thresholdsInit;
        string m_curCabId = "";
        Timer m_updateTimer;

        public Window1()
        {
            InitializeComponent();

            m_updateTheshTimer = new Timer(new TimerCallback(UpdateDeviceThresholds));

            m_thresholdsInit = false;
        }

        void OnUpdate(object arg)
        {
            this.Dispatcher.Invoke(new UpdateGraphDelegate(UpdateGraph));
        }

        void UpdateGraph()
        {
            if (!canvasGraph.IsVisible) return;

            string cabId = (string)comboBoxCabinets.SelectedItem;

            if (string.IsNullOrEmpty(cabId)) return;

            DateTime dt = DateTime.Now.Subtract(new TimeSpan(0, 20, 0)).ToUniversalTime();

            WineMonitorClient wineMonitor = new WineMonitorClient();

            WineSensorData[] array = wineMonitor.GetWineSensorDataHistory(cabId, dt);

            wineMonitor.Close();

            graphTemp.Points.Clear();
            graphHumid.Points.Clear();

            double width = canvasGraph.RenderSize.Width;

            double w = width;
            double maxT = m_currentThresholds.MaxTemperature + 10;
            double minT = m_currentThresholds.MinTemperature - 10;

            double maxH = m_currentThresholds.MaxHumidity + 10;
            double minH = m_currentThresholds.MinHumidity - 10;

            int maxShow = Math.Min((int)(width / 5), array.Length);

            double cw = width / (maxShow - 1);

            for (int i = 0; i < maxShow; i++)
            {
                WineSensorData data = array[i];
                if (data.Temperature > maxT) maxT = data.Temperature;
                if (data.Humidity > maxH) maxH = data.Humidity;
                if (data.Temperature < minT) minT = data.Temperature;
                if (data.Humidity < minH) minH = data.Humidity;
            }

            double scaleT;
            double scaleH;

            double heightT = canvasGraph.RenderSize.Height;
            double heightH = canvasGraphHumid.RenderSize.Height;

            if (maxT == minT) scaleT = (heightT / (2 * maxT));
            else scaleT = (heightT / (maxT - minT));

            if (maxH == minH) scaleH = (heightH / (2 * maxH));
            else scaleH = (heightH / (maxH - minH));

            for (int i = 0; i < maxShow; i++)
            {
                WineSensorData data = array[i];
                graphTemp.Points.Add(new Point(w, heightT - scaleT * (data.Temperature - minT)));
                graphHumid.Points.Add(new Point(w, heightH - scaleH * (data.Humidity - minH)));
                w -= cw;
            }

            lineMaxTemp.X1 = 0;
            lineMaxTemp.X2 = width;
            lineMaxTemp.Y1 = heightT - scaleT * (m_currentThresholds.MaxTemperature - minT);
            lineMaxTemp.Y2 = lineMaxTemp.Y1;

            lineMinTemp.X1 = 0;
            lineMinTemp.X2 = width;
            lineMinTemp.Y1 = heightT - scaleT * (m_currentThresholds.MinTemperature - minT);
            lineMinTemp.Y2 = lineMinTemp.Y1;

            lineMaxHumid.X1 = 0;
            lineMaxHumid.X2 = width;
            lineMaxHumid.Y1 = heightH - scaleH * (m_currentThresholds.MaxHumidity - minH);
            lineMaxHumid.Y2 = lineMaxHumid.Y1;

            lineMinHumid.X1 = 0;
            lineMinHumid.X2 = width;
            lineMinHumid.Y1 = heightH - scaleH * (m_currentThresholds.MinHumidity - minH);
            lineMinHumid.Y2 = lineMinHumid.Y1;

            labelGraphMaxTemp.Content = string.Format("{0:F2}", maxT);
            labelGraphMinTemp.Content = string.Format("{0:F2}", minT);

            labelGraphMaxHumid.Content = string.Format("{0:F2}", maxH);
            labelGraphMinHumid.Content = string.Format("{0:F2}", minH);
        }

        private void UpdateThresholds()
        {
            WineMonitorClient wineMonitor = new WineMonitorClient();

            string cabId = (string)comboBoxCabinets.SelectedItem;

            if (string.IsNullOrEmpty(cabId)) return;

            m_currentThresholds = wineMonitor.GetThresholdValues(cabId);

            wineMonitor.Close();

            if (m_currentThresholds == null)
            {
                m_currentThresholds = new WineSensorThreshold();
                m_currentThresholds.MaxHumidity = 100;
                m_currentThresholds.MaxHumiditySpecified = true;
                m_currentThresholds.MinHumidity = 0;
                m_currentThresholds.MinHumiditySpecified = true;
                m_currentThresholds.MaxTemperature = 100;
                m_currentThresholds.MaxTemperatureSpecified = true;
                m_currentThresholds.MinTemperature = 0;
                m_currentThresholds.MinTemperatureSpecified = true;
                m_currentThresholds.TimeStamp = DateTime.Now.ToUniversalTime();
                m_currentThresholds.TimeStampSpecified = true;
            }
            labelMinTemp.Content = m_currentThresholds.MinTemperature;
            labelMaxTemp.Content = m_currentThresholds.MaxTemperature;
            labelMinHumid.Content = m_currentThresholds.MinHumidity;
            labelMaxHumid.Content = m_currentThresholds.MaxHumidity;

            sliderMinTemp.Value = (int)m_currentThresholds.MinTemperature;
            sliderMaxTemp.Value = (int)m_currentThresholds.MaxTemperature;
            sliderMinHumid.Value = (int)m_currentThresholds.MinHumidity;
            sliderMaxHumid.Value = (int)m_currentThresholds.MaxHumidity;

            m_thresholdsInit = true;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            m_timer = new Timer(new TimerCallback(OnUpdate), null, 0, 2000);
        }

        private void UpdateDeviceThresholds(object arg)
        {
            if (string.IsNullOrEmpty(m_curCabId)) return;

            WineMonitorClient wineMonitor = new WineMonitorClient();
            wineMonitor.ChangeThresholds(m_curCabId, m_currentThresholds);
            wineMonitor.Close();
        }

        private void slider2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            m_curCabId = (string)comboBoxCabinets.SelectedItem;

            labelMaxTemp.Content = (int)e.NewValue;
            
            if (m_thresholdsInit)
            {
                m_currentThresholds.MaxTemperature = (int)e.NewValue;
                m_currentThresholds.TimeStamp = DateTime.Now.ToUniversalTime();

                m_updateTheshTimer.Change(500, Timeout.Infinite);
            }
        }

        private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            m_curCabId = (string)comboBoxCabinets.SelectedItem;

            labelMinTemp.Content = (int)e.NewValue;

            if (m_thresholdsInit)
            {
                m_currentThresholds.MinTemperature = (int)e.NewValue;
                m_currentThresholds.TimeStamp = DateTime.Now.ToUniversalTime();
                m_updateTheshTimer.Change(500, Timeout.Infinite);
            }
        }

        private void sliderMinHumid_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            m_curCabId = (string)comboBoxCabinets.SelectedItem;

            labelMinHumid.Content = (int)e.NewValue;

            if (m_thresholdsInit)
            {
                m_currentThresholds.MinHumidity = (int)e.NewValue;
                m_currentThresholds.TimeStamp = DateTime.Now.ToUniversalTime();
                m_updateTheshTimer.Change(500, Timeout.Infinite);
            }
        }

        private void sliderMaxHumid_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            m_curCabId = (string)comboBoxCabinets.SelectedItem;

            labelMaxHumid.Content = (int)e.NewValue;

            if (m_thresholdsInit)
            {
                m_currentThresholds.MaxHumidity = (int)e.NewValue;
                m_currentThresholds.TimeStamp = DateTime.Now.ToUniversalTime();
                m_updateTheshTimer.Change(500, Timeout.Infinite);
            }
        }

        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            WineMonitorClient wineMonitor = new WineMonitorClient();

            comboBoxCabinets.Items.Clear();

            foreach (CabinetItem cab in wineMonitor.GetWineCabinets())
            {
                comboBoxCabinets.Items.Add(cab.Cabinet);
            }

            wineMonitor.Close();
        }

        private void comboBoxCabinets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty((string)comboBoxCabinets.SelectedItem) && m_updateTimer != null)
            {
                m_updateTimer = new Timer(new TimerCallback(OnUpdateTimer), null, 4000, 4000);
            }

            m_thresholdsInit = false;
            UpdateData();
            UpdateThresholds();
        }

        void UpdateData()
        {
            string cabId = (string)comboBoxCabinets.SelectedItem;
            if (string.IsNullOrEmpty(cabId)) return;

            WineMonitorClient wineMonitor = new WineMonitorClient();

            WineSensorData data = wineMonitor.GetSensorData(cabId);


            tempLabel.Content = string.Format("{0:F2} F", data.Temperature);
            humidLabel.Content = string.Format("{0:F2} %", data.Humidity);
            labelTimeStamp.Content = data.TimeStamp.ToString();

            wineMonitor.Close();
        }

        void OnUpdateTimer(object arg)
        {
            this.Dispatcher.Invoke(new UpdateSensorData(this.UpdateData));
        }

    }
}
