using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using SilverlightWineMonitorApp.WineMonitorService;
using System.Windows.Controls.DataVisualization.Charting;
using System.IO.IsolatedStorage;
using System.Xml.Serialization;

namespace SilverlightWineMonitorApp
{
    public partial class Home : Page
    {
        delegate void UpdateGraphDelegate();
        delegate void UpdateSensorData();

        Timer m_timer, m_updateTheshTimer;
        WineSensorThreshold m_currentThresholds;
        bool m_thresholdsInit;
        string m_curCabId = "";
        WineMonitorClient m_client;
        string m_clientStorageDir = "NetMFDemo";
        string m_clientStorageFile = "NetMFDemo\\WineCabinetData";
        object m_storeLock = new object();
        DateTime m_ignoreAlertDate;
        bool m_thresScheduled = false;

        const int c_MaxRecordsToShow = 20;

        public class MyDataStorage
        {
            public string CabinetId;
            public List<WineSensorData> DataHistory;
            public List<WineSensorThreshold> ThresholdHistory;
        }

        internal MyDataStorage m_history = new MyDataStorage();

        void InitStorage()
        {
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForSite())
                {
                    if (!store.DirectoryExists(m_clientStorageDir))
                    {
                        store.CreateDirectory(m_clientStorageDir);
                    }

                    //store.DeleteFile(m_clientStorageFile);
                    
                    using (IsolatedStorageFileStream fs = new IsolatedStorageFileStream(m_clientStorageFile, System.IO.FileMode.OpenOrCreate, store))
                    {
                        if (fs.Length > 0)
                        {
                            XmlSerializer ser = new XmlSerializer(typeof(MyDataStorage));

                            m_history = ser.Deserialize(fs) as MyDataStorage;

                            if (m_history != null)
                            {
                                if (!comboCabinetId.Items.Contains(m_history.CabinetId))
                                {
                                    m_client_GetWineSensorDataHistoryCompleted(null, new GetWineSensorDataHistoryCompletedEventArgs(new object[]{ m_history.DataHistory.ToArray()}, null, false, this));
                                    m_client_GetThresholdHistoryCompleted(null, new GetThresholdHistoryCompletedEventArgs(new object[] { m_history.ThresholdHistory.ToArray() }, null, false, this));

                                    comboCabinetId.Items.Add(m_history.CabinetId);
                                    comboCabinetId.SelectedItem = m_history.CabinetId;

                                    if (m_history.ThresholdHistory.Count > 0)
                                    {
                                        m_currentThresholds = m_history.ThresholdHistory[0];
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public Home()
        {
            InitializeComponent();

            m_updateTheshTimer = new Timer(new TimerCallback(UpdateDeviceThresholds), null, Timeout.Infinite, Timeout.Infinite);

            m_thresholdsInit = false;

            if (m_currentThresholds == null)
            {
                m_currentThresholds = new WineSensorThreshold();
                m_currentThresholds.MaxHumidity = 100;
                //m_currentThresholds.MaxHumiditySpecified = true;
                m_currentThresholds.MinHumidity = 0;
                //m_currentThresholds.MinHumiditySpecified = true;
                m_currentThresholds.MaxTemperature = 100;
                //m_currentThresholds.MaxTemperatureSpecified = true;
                m_currentThresholds.MinTemperature = 0;
                //m_currentThresholds.MinTemperatureSpecified = true;
                m_currentThresholds.TimeStamp = DateTime.Now;
                //m_currentThresholds.TimeStampSpecified = true;
            }

            m_timer = new Timer(new TimerCallback(OnUpdate), null, Timeout.Infinite, Timeout.Infinite);
        }

        void SaveLocalFile()
        {
            lock (m_storeLock)
            {
                try
                {
                    using (var store = IsolatedStorageFile.GetUserStoreForSite())
                    {
                        using (IsolatedStorageFileStream fs = new IsolatedStorageFileStream(m_clientStorageFile, System.IO.FileMode.Create, store))
                        {
                            XmlSerializer ser = new XmlSerializer(typeof(MyDataStorage));

                            ser.Serialize(fs, m_history);
                        }
                    }
                }
                catch
                {
                }
            }
        }


        void m_client_GetWineSensorDataHistoryCompleted(object sender, GetWineSensorDataHistoryCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null && e.Result != null)
                {
                    List<WineSensorData> list = new List<WineSensorData>(e.Result);

                    for (int i = 0; i < list.Count; i++)
                    {
                        list[i].TimeStamp = list[i].TimeStamp.ToLocalTime();
                    }

                    lock (m_history)
                    {
                        m_history.DataHistory = list;
                    }

                    if (m_history.DataHistory != null && m_history.DataHistory.Count > 0)
                    {
                        LineSeries lineTemp = chartTemp.Series[0] as LineSeries;
                        LineSeries lineHumid = chartTemp.Series[3] as LineSeries;

                        lineTemp.ItemsSource = m_history.DataHistory;
                        lineHumid.ItemsSource = m_history.DataHistory;

                        //SaveLocalFile();
                    }
                }
            }
            catch
            {
            }
        }

        void m_client_GetWineCabinetsCompleted(object sender, GetWineCabinetsCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                foreach (CabinetItem cab in e.Result)
                {
                    comboCabinetId.Items.Add(cab.Cabinet);
                }
            }
        }

        void m_client_GetThresholdValuesCompleted(object sender, GetThresholdValuesCompletedEventArgs e)
        {
            if (e.Error == null && e.Result != null)
            {
                m_thresholdsInit = false;

                m_currentThresholds = e.Result;
                m_currentThresholds.TimeStamp = m_currentThresholds.TimeStamp.ToLocalTime();

                labelMinTemp.Content = string.Format("{0} F", (int)m_currentThresholds.MinTemperature);
                labelMaxTemp.Content = string.Format("{0} F", (int)m_currentThresholds.MaxTemperature);
                labelMinHumid.Content = string.Format("{0} %", (int)m_currentThresholds.MinHumidity);
                labelMaxHumid.Content = string.Format("{0} %", (int)m_currentThresholds.MaxHumidity);

                sliderMinTemp.Value = (int)m_currentThresholds.MinTemperature;
                sliderMaxTemp.Value = (int)m_currentThresholds.MaxTemperature;
                sliderMinHumid.Value = (int)m_currentThresholds.MinHumidity;
                sliderMaxHumid.Value = (int)m_currentThresholds.MaxHumidity;
            }

            m_thresholdsInit = true;
        }

        void m_client_GetThresholdHistoryCompleted(object sender, GetThresholdHistoryCompletedEventArgs e)
        {
            try
            {
                if (e != null && e.Error == null && e.Result != null)
                {
                    List<WineSensorThreshold> list = new List<WineSensorThreshold>(e.Result);

                    for (int i = 0; i < list.Count; i++)
                    {
                        list[i].TimeStamp = list[i].TimeStamp.ToLocalTime();
                    }

                    if (m_history.DataHistory != null && m_history.DataHistory.Count > 0)
                    {
                        if (list.Count == 0 || list[list.Count-1].TimeStamp > m_history.DataHistory[m_history.DataHistory.Count-1].TimeStamp)
                        {
                            WineSensorThreshold wst2 = new WineSensorThreshold();
                            wst2.MinHumidity = m_currentThresholds.MinHumidity;
                            wst2.MaxHumidity = m_currentThresholds.MaxHumidity;
                            wst2.MinTemperature = m_currentThresholds.MinTemperature;
                            wst2.MaxTemperature = m_currentThresholds.MaxTemperature;
                            wst2.MaxMinutesWithoutUpdate = m_currentThresholds.MaxMinutesWithoutUpdate;
                            wst2.TimeStamp = m_history.DataHistory[m_history.DataHistory.Count - 1].TimeStamp;

                            list.Add(wst2);
                        }

                        WineSensorThreshold wst = new WineSensorThreshold();
                        wst.MinHumidity = m_currentThresholds.MinHumidity;
                        wst.MaxHumidity = m_currentThresholds.MaxHumidity;
                        wst.MinTemperature = m_currentThresholds.MinTemperature;
                        wst.MaxTemperature = m_currentThresholds.MaxTemperature;
                        wst.MaxMinutesWithoutUpdate = m_currentThresholds.MaxMinutesWithoutUpdate;
                        wst.TimeStamp = m_history.DataHistory[0].TimeStamp;

                        list.Insert(0, wst);
                    }

                    // hack to make graph look nice
                    lock (m_history)
                    {
                        m_history.ThresholdHistory = list;
                    }

                    if (m_history.ThresholdHistory != null && m_history.ThresholdHistory.Count > 0)
                    {
                        LineSeries lineTempMax = chartTemp.Series[1] as LineSeries;
                        LineSeries lineTempMin = chartTemp.Series[2] as LineSeries;
                        LineSeries lineHumidMax = chartTemp.Series[4] as LineSeries;
                        LineSeries lineHumidMin = chartTemp.Series[5] as LineSeries;

                        lineTempMax.ItemsSource = m_history.ThresholdHistory;
                        lineTempMin.ItemsSource = m_history.ThresholdHistory;
                        lineHumidMax.ItemsSource = m_history.ThresholdHistory;
                        lineHumidMin.ItemsSource = m_history.ThresholdHistory;

                        //SaveLocalFile();
                    }
                }
            }
            catch
            {
            }
        }

        void m_client_GetSensorDataCompleted(object sender, GetSensorDataCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                DateTime dt = e.Result.TimeStamp.ToLocalTime();
                labelTemp.Content = string.Format("{0:F2} F", e.Result.Temperature);
                labelHumid.Content = string.Format("{0:F2} %", e.Result.Humidity);
                labelTimeStamp.Content = dt.ToString("M") + " " + dt.ToString("t");
            }
        }

        void m_client_GetAlertHistoryCompleted(object sender, GetAlertHistoryCompletedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        void m_client_GetAlertCompleted(object sender, GetAlertCompletedEventArgs e)
        {
            if (e.Error == null && e.Result != null)
            {
                if (m_ignoreAlertDate == null || e.Result.TimeStamp > m_ignoreAlertDate)
                {
                    string alert = "Alert: " + e.Result.Alert;

                    switch (e.Result.Alert)
                    {
                        case AlertFlags.HumidHigh:
                        case AlertFlags.HumidLow:
                        case AlertFlags.TempHigh:
                        case AlertFlags.TempLow:
                            alert += " (" + string.Format("{0:F2}", e.Result.AlertValue) + ")";
                            break;

                        case AlertFlags.None:
                            return;
                    }

                    labelAlert.Content = alert;
                    labelAlert.DataContext = e.Result.TimeStamp;
                    labelAlertTime.Content = e.Result.TimeStamp.ToString();
                    buttonIgnoreAlert.Visibility = Visibility.Visible;
                }
            }
        }

        // Executes when the user navigates to this page.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        void OnUpdate(object arg)
        {
            if (!m_thresScheduled)
            {
                this.Dispatcher.BeginInvoke(new UpdateGraphDelegate(UpdateThresholds));
            }
            this.Dispatcher.BeginInvoke(new UpdateGraphDelegate(UpdateData));
            this.Dispatcher.BeginInvoke(new UpdateGraphDelegate(UpdateGraph));
        }

        void UpdateGraph()
        {
            string cabId = (string)comboCabinetId.SelectedItem;

            if (string.IsNullOrEmpty(cabId)) return;

            DateTime dt = DateTime.Now.Subtract(new TimeSpan(0, 1, 0));
            dt = dt.ToUniversalTime();

            try
            {
                m_client.GetWineSensorDataHistoryAsync(cabId, dt);
                m_client.GetThresholdHistoryAsync(cabId, dt);
            }
            catch
            {
            }
        }

        private void UpdateThresholds()
        {
            string cabId = (string)comboCabinetId.SelectedItem;

            if (string.IsNullOrEmpty(cabId)) return;

            try
            {
                m_client.GetThresholdValuesAsync(cabId);
            }
            catch
            {
            }
        }

        private void UpdateDeviceThresholds(object arg)
        {
            if (string.IsNullOrEmpty(m_curCabId)) return;

            try
            {
                m_client.ChangeThresholdsAsync(m_curCabId, m_currentThresholds);
            }
            catch
            {
            }
            m_thresScheduled = false;
        }
        
        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            comboCabinetId.Items.Clear();

            try
            {
                m_timer.Change(Timeout.Infinite, Timeout.Infinite);
                m_client.GetWineCabinetsAsync();
            }
            catch
            {
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty((string)comboCabinetId.SelectedItem))
            {
                UpdateData();
                UpdateGraph();
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            m_curCabId = (string)comboCabinetId.SelectedItem;

            labelMinTemp.Content = string.Format("{0} F", (int)e.NewValue);

            if (m_thresholdsInit)
            {
                m_currentThresholds.MinTemperature = (int)e.NewValue;
                m_currentThresholds.TimeStamp = DateTime.UtcNow;

                m_thresScheduled = true;
                m_updateTheshTimer.Change(500, Timeout.Infinite);
            }
        }

        private void Slider_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            m_curCabId = (string)comboCabinetId.SelectedItem;

            labelMaxTemp.Content = string.Format("{0} F", (int)e.NewValue);

            if (m_thresholdsInit)
            {
                m_currentThresholds.MaxTemperature = (int)e.NewValue;
                m_currentThresholds.TimeStamp = DateTime.UtcNow;

                m_thresScheduled = true;
                m_updateTheshTimer.Change(500, Timeout.Infinite);
            }

        }

        private void Slider_ValueChanged_2(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            m_curCabId = (string)comboCabinetId.SelectedItem;

            labelMinHumid.Content = string.Format("{0} %", (int)e.NewValue);

            if (m_thresholdsInit)
            {
                m_currentThresholds.MinHumidity = (int)e.NewValue;
                m_currentThresholds.TimeStamp = DateTime.UtcNow;

                m_thresScheduled = true;
                m_updateTheshTimer.Change(500, Timeout.Infinite);
            }
        }

        private void Slider_ValueChanged_3(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            m_curCabId = (string)comboCabinetId.SelectedItem;

            labelMaxHumid.Content = string.Format("{0} %", (int)e.NewValue);

            if (m_thresholdsInit)
            {
                m_currentThresholds.MaxHumidity = (int)e.NewValue;
                m_currentThresholds.TimeStamp = DateTime.UtcNow;

                m_thresScheduled = true;
                m_updateTheshTimer.Change(500, Timeout.Infinite);
            }
        }

        void UpdateData()
        {
            m_curCabId = (string)comboCabinetId.SelectedItem;

            try
            {
                m_client.GetSensorDataAsync(m_curCabId);

                m_client.GetAlertAsync(m_curCabId);
            }
            catch
            {
            }
        }


        private void comboCabinetId_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            m_thresholdsInit = false;

            if (comboCabinetId.SelectedItem != null)
            {
                m_history.CabinetId = (string)comboCabinetId.SelectedItem;

                m_timer.Change(0, 5000);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                m_client = new WineMonitorClient();
                m_client.OpenAsync();

                sliderMaxHumid.Value = 100;
                sliderMaxTemp.Value = 100;

                m_client.GetAlertCompleted += new EventHandler<GetAlertCompletedEventArgs>(m_client_GetAlertCompleted);
                m_client.GetAlertHistoryCompleted += new EventHandler<GetAlertHistoryCompletedEventArgs>(m_client_GetAlertHistoryCompleted);
                m_client.GetSensorDataCompleted += new EventHandler<GetSensorDataCompletedEventArgs>(m_client_GetSensorDataCompleted);
                m_client.GetThresholdHistoryCompleted += new EventHandler<GetThresholdHistoryCompletedEventArgs>(m_client_GetThresholdHistoryCompleted);
                m_client.GetThresholdValuesCompleted += new EventHandler<GetThresholdValuesCompletedEventArgs>(m_client_GetThresholdValuesCompleted);
                m_client.GetWineCabinetsCompleted += new EventHandler<GetWineCabinetsCompletedEventArgs>(m_client_GetWineCabinetsCompleted);
                m_client.GetWineSensorDataHistoryCompleted += new EventHandler<GetWineSensorDataHistoryCompletedEventArgs>(m_client_GetWineSensorDataHistoryCompleted);
            }
            catch
            {
            }

            //InitStorage();
        }

        private void buttonIgnoreAlert_Click(object sender, RoutedEventArgs e)
        {
            labelAlert.Content = "";
            labelAlertTime.Content = "";
            buttonIgnoreAlert.Visibility = Visibility.Collapsed;
            m_ignoreAlertDate = (DateTime)labelAlert.DataContext;
        }
    }
}