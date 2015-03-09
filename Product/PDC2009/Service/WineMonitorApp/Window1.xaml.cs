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
        delegate void UpdateUIDelegate();

        List<WineSensorData> m_history;
        WineMonitorService.WineMonitorAlert m_alert;
        DateTime m_lastIgnore;
        Timer m_updateTimer;
        WineMonitorService.WineMonitorService m_service;

        public Window1()
        {
            InitializeComponent();
        }


        void UpdateUI()
        {
            if(m_history.Count == 0) return;

            tempLabel.Content = string.Format("{0:F1} F", m_history[0].Temperature);
            humidLabel.Content = string.Format("{0:F1} %", m_history[0].Humidity);
            labelTimeStamp.Content = m_history[0].TimeStamp.ToLocalTime().ToLongTimeString();

            if (!canvasGraph.IsVisible) return;

            graphTemp.Points.Clear();
            graphHumid.Points.Clear();

            double width = canvasGraph.RenderSize.Width;

            double w = width;
            double maxT = m_history[0].Temperature;
            double minT = m_history[0].Temperature;

            double maxH = m_history[0].Humidity;
            double minH = m_history[0].Humidity;

            int maxShow = Math.Min((int)(width / 4), m_history.Count);

            double cw = width / (maxShow == 1 ? 1 : (maxShow - 1));

            for (int i = 0; i < maxShow; i++)
            {
                WineSensorData data = m_history[i];
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
                WineSensorData data = m_history[i];
                graphTemp.Points.Add(new Point(w, heightT - (scaleT * (data.Temperature - minT))));
                graphHumid.Points.Add(new Point(w, heightH - (scaleH * (data.Humidity - minH))));
                w -= cw;
            }

            labelGraphMaxHumid.Content = string.Format("{0:F1} %", maxH);
            labelGraphMinHumid.Content = string.Format("{0:F1} %", minH);
            labelGraphMaxTemp.Content = string.Format("{0:F1} F", maxT);
            labelGraphMinTemp.Content = string.Format("{0:F1} F", minT);

        }
        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (btnAutoRefresh.IsChecked.Value)
            {
                int val;

                if (int.TryParse(textBox1.Text, out val) && val != 0)
                {
                    val *= 1000;
                    m_updateTimer.Change(0, val);
                }
                else
                {
                    m_updateTimer.Change(0, 60000);
                }
            }
            else
            {
                m_updateTimer.Change(0, Timeout.Infinite);
            }
        }

        void OnUpdateTimer(object arg)
        {
            try
            {
                WineMonitorRequestClient device = new WineMonitorRequestClient("WineMonitorDevice");
                WineSensorData wsd = device.GetSensorData();
                device.Close();
                m_history.Insert(0, wsd);

                Dispatcher.Invoke(new UpdateUIDelegate(UpdateUI), null);
            }
            catch
            {
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            m_lastIgnore = DateTime.UtcNow;

            m_history = new List<WineSensorData>();

            m_updateTimer = new Timer(new TimerCallback(OnUpdateTimer), null, Timeout.Infinite, Timeout.Infinite);

            m_alert = new WineMonitorService.WineMonitorAlert();

            m_alert.OnAlert += new WineMonitorService.AlertNotification(m_alert_OnAlert);

            m_service = new WineMonitorService.WineMonitorService();
            m_service.StartService(m_alert);
        }

        void OnAlert(WineMonitorService.AlertData data)
        {
            if (data.Timestamp <= m_lastIgnore) return;

            string alertText = "";
            Label lb = null;

            switch (data.Alert)
            {
                case WineMonitorService.AlertType.HumidityHigh:
                    alertText = "% (High)";
                    lb = labelAlertHumid;
                    break;
                case WineMonitorService.AlertType.HumidityLow:
                    alertText = "% (Low)";
                    lb = labelAlertHumid;
                    break;
                case WineMonitorService.AlertType.TemperatureHigh:
                    alertText = "F (High)";
                    lb = labelAlertTemp;
                    break;
                case WineMonitorService.AlertType.TemperatureLow:
                    alertText = "F (Low)";
                    lb = labelAlertTemp;
                    break;
            }

            if (alertText.Length != 0)
            {
                lb.Content = string.Format("Alert: {0:F1} {1} {2}", data.AlertValue, alertText, data.Timestamp.ToLocalTime().ToShortTimeString());
                buttonIgnoreAlert.Visibility = Visibility.Visible;
            }
        }

        void m_alert_OnAlert(WineMonitorService.AlertData data)
        {
            Dispatcher.Invoke(new WineMonitorService.AlertNotification(OnAlert), data);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_alert.OnAlert -= new WineMonitorService.AlertNotification(m_alert_OnAlert);
            m_service.StopService();
        }

        private void btnAutoRefresh_Checked(object sender, RoutedEventArgs e)
        {
            if (btnAutoRefresh.IsChecked.Value)
            {
                int val;

                if (int.TryParse(textBox1.Text, out val) && val != 0)
                {
                    val *= 1000;
                }
                else
                {
                    val = 60000;
                }

                textBox1.IsEnabled = true;

                m_updateTimer.Change(0, val);
            }
            else
            {
                textBox1.IsEnabled = false;

                m_updateTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        private void buttonIgnoreAlert_Click(object sender, RoutedEventArgs e)
        {
            m_lastIgnore = DateTime.UtcNow;
            labelAlertHumid.Content = "";
            labelAlertTemp.Content = "";
            buttonIgnoreAlert.Visibility = Visibility.Hidden;
        }

        private void listBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            int val;

            if (m_updateTimer == null) return;

            if (int.TryParse(textBox1.Text, out val) && val != 0)
            {
                val *= 1000;
                m_updateTimer.Change(val, val);
            }
        }

    }
}
