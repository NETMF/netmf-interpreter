using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Activation;
using System.Threading;
using System.IO.IsolatedStorage;
using System.IO;
using System.Xml.Serialization;
using System.Security.Permissions;

namespace WineMonitorService
{
    internal class WineCabinetData
    {
        internal WineCabinetData(string name, string epConfig, string epAddress)
        {
            Name = name;
            EndpointConfigName = epConfig;
            EndpointAddress = epAddress;
        }
        internal string Name = "";
        internal string EndpointConfigName;
        internal string EndpointAddress;
        internal List<WineSensorData> SensorData = new List<WineSensorData>();
        internal List<WineSensorThreshold> ThresholdData = new List<WineSensorThreshold>();
        internal List<AlertData> AlertData = new List<AlertData>();
    }

    [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class WineMonitorService : IWineMonitor
    {
        internal static Dictionary<string, WineCabinetData> s_wineCabinets = new Dictionary<string, WineCabinetData>();

        delegate void IsolatedStorageDelegate();

        void InitStorage()
        {
            try
            {
                using (var store = IsolatedStorageFile.GetMachineStoreForApplication())
                {
                    string dir = "NetMFDemo";
                    string file = "WineCabinetData";
                    store.CreateDirectory(dir);

                    using (IsolatedStorageFileStream fs = new IsolatedStorageFileStream(dir + "\\" + file, System.IO.FileMode.OpenOrCreate, store))
                    {
                        if (fs.Length > 0)
                        {
                            XmlSerializer ser = new XmlSerializer(typeof(Dictionary<string, WineSensorData>));

                            Dictionary<string, WineSensorData> data = ser.Deserialize(fs) as Dictionary<string, WineSensorData>;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        //void InitStorage()
        //{
        //    try
        //    {
        //        using (var store = IsolatedStorageFile.GetMachineStoreForApplication())
        //        {
        //            string dir = "NetMFDemo";
        //            string file = "WineCabinetData";
        //            store.CreateDirectory(dir);

        //            using (IsolatedStorageFileStream fs = new IsolatedStorageFileStream(Path.Combine(dir, file), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
        //            {
        //                XmlSerializer ser = new XmlSerializer(typeof(Dictionary<string, WineCabinetData>));

        //                s_wineCabinets = ser.Deserialize(fs) as Dictionary<string, WineCabinetData>;
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.ToString());
        //    }
        //}

        public WineMonitorService()
        {
            //AppDomain.CurrentDomain.ApplicationTrust.IsApplicationTrustedToRun = true;
            
            //AppDomain.CurrentDomain.ApplicationTrust.ApplicationIdentity = new ApplicationIdentity(".Net Micro Framework Demo - Wine Cabinet Monitor");

            //IsolatedStorageDelegate del = new IsolatedStorageDelegate(InitStorage);

            //del.Invoke();
        }

        #region IWineMonitorImpl Members

        WineSensorData[] GetWineSensorDataHistory(string cabinetId, DateTime dataSince)
        {
            if (!s_wineCabinets.ContainsKey(cabinetId))
            {
                return null;
            }

            List<WineSensorData> ret = new List<WineSensorData>();

            lock (s_wineCabinets)
            {
                for (int i = 0; i < s_wineCabinets[cabinetId].SensorData.Count; i++)
                {
                    WineSensorData data = s_wineCabinets[cabinetId].SensorData[i];

                    if (data != null && data.TimeStamp >= dataSince)
                    {
                        ret.Add(data);
                    }
                }
            }

            return ret.ToArray<WineSensorData>();
        }

        AlertData[] GetAlertHistory(string cabinetId, DateTime alertsSince)
        {
            if (!s_wineCabinets.ContainsKey(cabinetId))
            {
                return null;
            }

            List<AlertData> ret = new List<AlertData>();

            lock (s_wineCabinets)
            {
                for (int i = 0; i < s_wineCabinets[cabinetId].AlertData.Count; i++)
                {
                    AlertData data = s_wineCabinets[cabinetId].AlertData[i];

                    if (data != null && data.TimeStamp >= alertsSince)
                    {
                        ret.Add(data);
                    }
                }
            }

            return ret.ToArray<AlertData>();
        }

        WineSensorThreshold[] GetThresholdHistory(string cabinetId, DateTime changesSince)
        {
            if (!s_wineCabinets.ContainsKey(cabinetId))
            {
                return null;
            }

            List<WineSensorThreshold> ret = new List<WineSensorThreshold>();

            lock (s_wineCabinets)
            {
                for (int i = 0; i < s_wineCabinets[cabinetId].ThresholdData.Count; i++)
                {
                    WineSensorThreshold data = s_wineCabinets[cabinetId].ThresholdData[i];

                    if (data != null && data.TimeStamp >= changesSince)
                    {
                        ret.Add(data);
                    }
                }
            }

            return ret.ToArray<WineSensorThreshold>();
        }

        WineSensorData GetSensorData(string cabinetId)
        {
            //InitStorage();

            if (!s_wineCabinets.ContainsKey(cabinetId) || s_wineCabinets[cabinetId].SensorData.Count == 0)
            {
                return null;
            }

            WineSensorData wsd = null;

            lock (s_wineCabinets)
            {
                wsd = s_wineCabinets[cabinetId].SensorData.Count > 0 ? s_wineCabinets[cabinetId].SensorData[0] : null;
            }

            if ((DateTime.Now - wsd.TimeStamp) > new TimeSpan(0, 0, 10))
            {
                try
                {
                    WineMonitorRequestClient client = new WineMonitorRequestClient(s_wineCabinets[cabinetId].EndpointConfigName, s_wineCabinets[cabinetId].EndpointAddress);

                    client.RequestUpdate();

                    client.Close();
                }
                catch
                {
                }
            }

            return wsd;
        }

        AlertData GetAlert(string cabinetId)
        {
            if (!s_wineCabinets.ContainsKey(cabinetId))
            {
                return null;
            }

            AlertData ad = null;

            lock (s_wineCabinets)
            {
                ad = s_wineCabinets[cabinetId].AlertData.Count > 0 ? s_wineCabinets[cabinetId].AlertData[0] : null;
            }

            return ad;
        }

        WineSensorThreshold GetThresholdValues(string cabinetId)
        {
            if (!s_wineCabinets.ContainsKey(cabinetId))
            {
                return null;
            }

            WineSensorThreshold wst = null;

            lock (s_wineCabinets)
            {
                wst = s_wineCabinets[cabinetId].ThresholdData.Count > 0 ? s_wineCabinets[cabinetId].ThresholdData[0] : null;
            }

            return wst;
        }

        void ChangeThresholds(string cabinetId, WineSensorThreshold data)
        {
            if (cabinetId == null || data == null) return;

            if (!WineMonitorService.s_wineCabinets.ContainsKey(cabinetId))
            {
                return;
            }

            lock (WineMonitorService.s_wineCabinets)
            {
                WineMonitorService.s_wineCabinets[cabinetId].ThresholdData.Insert(0, data);
            }

            try
            {
                WineMonitorRequestClient client = new WineMonitorRequestClient(WineMonitorService.s_wineCabinets[cabinetId].EndpointConfigName, WineMonitorService.s_wineCabinets[cabinetId].EndpointAddress);

                WineSensorThresholdReq req = new WineSensorThresholdReq();

                req.MaxHumidity = data.MaxHumidity;
                req.MaxHumiditySpecified = true;
                req.MinHumidity = data.MinHumidity;
                req.MinHumiditySpecified = true;
                req.MaxTemperature = data.MaxTemperature;
                req.MaxTemperatureSpecified = true;
                req.MinTemperature = data.MinTemperature;
                req.MinTemperatureSpecified = true;
                req.TimeStamp = data.TimeStamp;
                req.TimeStampSpecified = true;
                req.MaxMinutesWithoutUpdate = data.MaxMinutesWithoutUpdate;
                req.MaxMinutesWithoutUpdateSpecified = true;

                client.SetThresholds(req);

                client.Close();
            }
            catch (Exception e)
            {
                string foo = e.ToString();
                Console.WriteLine(foo);
            }
        }

        CabinetItem[] GetWineCabinets()
        {
            List<CabinetItem> cabs = new List<CabinetItem>();

            foreach (string key in s_wineCabinets.Keys)
            {
                CabinetItem cab = new CabinetItem();
                cab.Cabinet = key;
                cabs.Add(cab);
            }

            return cabs.ToArray();
        }

        #endregion

        delegate WineSensorData GetSensorDataDelegate(string cabinetId);
        delegate WineSensorThreshold GetThresholdDelegate(string cabinetId);
        delegate AlertData GetAlertDelegate(string cabinetId);
        delegate WineSensorData[] GetSensorDataHistoryDelegate(string cabinetId, DateTime changesSince);
        delegate WineSensorThreshold[] GetThresholdHistoryDelegate(string cabinetId, DateTime changesSince);
        delegate AlertData[] GetAlertHistoryDelegate(string cabinetId, DateTime changesSince);
        delegate CabinetItem[] GetWineCabinetsDelegate();

        //delegate void RegisterWineCabinetDelegate(string cabinetId, string endpointConfigName, string endpointAddress);
        //delegate void AlertDelegate(string cabinetId, AlertData alert);
        //delegate void UpdateSensorDataDelegate(string cabinetId, WineSensorData data);
        delegate void ChangeThresholdsDelegate(string cabinetId, WineSensorThreshold data);

        Delegate GetAsyncDelegate(IAsyncResult key)
        {
            Delegate del = null;

            lock (m_DelegateMap)
            {
                if (m_DelegateMap.ContainsKey(key))
                {
                    del = m_DelegateMap[key];

                    m_DelegateMap.Remove(key);
                }
            }

            return del;
        }

        void AddAysncDelegate(IAsyncResult key, Delegate asyncDelegate)
        {
            lock (m_DelegateMap)
            {
                m_DelegateMap[key] = asyncDelegate;
            }
        }


        Dictionary<IAsyncResult, Delegate> m_DelegateMap = new Dictionary<IAsyncResult, Delegate>();

        #region IWineMonitor Members

        public IAsyncResult BeginGetSensorData(string cabinetId, AsyncCallback callback, Object state)
        {
            GetSensorDataDelegate del = new GetSensorDataDelegate(GetSensorData);
            IAsyncResult res = del.BeginInvoke(cabinetId, callback, state);

            AddAysncDelegate(res, del);

            return res;
        }

        public WineSensorData EndGetSensorData(IAsyncResult result)
        {
            GetSensorDataDelegate del = GetAsyncDelegate(result) as GetSensorDataDelegate;

            return del.EndInvoke(result);
        }

        public IAsyncResult BeginGetAlert(string cabinetId, AsyncCallback callback, Object state)
        {
            GetAlertDelegate del = new GetAlertDelegate(GetAlert);
            IAsyncResult     res = del.BeginInvoke(cabinetId, callback, state);

            AddAysncDelegate(res, del);

            return res;
        }

        public AlertData EndGetAlert(IAsyncResult result)
        {
            GetAlertDelegate del = GetAsyncDelegate(result) as GetAlertDelegate;

            return del.EndInvoke(result);
        }

        public IAsyncResult BeginGetThresholdValues(string cabinetId, AsyncCallback callback, Object state)
        {
            GetThresholdDelegate del = new GetThresholdDelegate(GetThresholdValues);
            IAsyncResult res = del.BeginInvoke(cabinetId, callback, state);

            AddAysncDelegate(res, del);

            return res;
        }

        public WineSensorThreshold EndGetThresholdValues(IAsyncResult result)
        {
            GetThresholdDelegate del = GetAsyncDelegate(result) as GetThresholdDelegate;

            return del.EndInvoke(result);
        }

        public IAsyncResult BeginGetWineSensorDataHistory(string cabinetId, DateTime dataSince, AsyncCallback callback, Object state)
        {
            GetSensorDataHistoryDelegate del = new GetSensorDataHistoryDelegate(GetWineSensorDataHistory);
            IAsyncResult res = del.BeginInvoke(cabinetId, dataSince, callback, state);

            AddAysncDelegate(res, del);

            return res;
        }

        public WineSensorData[] EndGetWineSensorDataHistory(IAsyncResult result)
        {
            GetSensorDataHistoryDelegate del = GetAsyncDelegate(result) as GetSensorDataHistoryDelegate;

            return del.EndInvoke(result);
        }

        public IAsyncResult BeginGetAlertHistory(string cabinetId, DateTime alertsSince, AsyncCallback callback, Object state)
        {
            GetAlertHistoryDelegate del = new GetAlertHistoryDelegate(GetAlertHistory);
            IAsyncResult res = del.BeginInvoke(cabinetId, alertsSince, callback, state);

            AddAysncDelegate(res, del);

            return res;
        }

        public AlertData[] EndGetAlertHistory(IAsyncResult result)
        {
            GetAlertHistoryDelegate del = GetAsyncDelegate(result) as GetAlertHistoryDelegate;

            return del.EndInvoke(result);
        }

        public IAsyncResult BeginGetThresholdHistory(string cabinetId, DateTime changesSince, AsyncCallback callback, Object state)
        {
            GetThresholdHistoryDelegate del = new GetThresholdHistoryDelegate(GetThresholdHistory);
            IAsyncResult res = del.BeginInvoke(cabinetId, changesSince, callback, state);

            AddAysncDelegate(res, del);

            return res;
        }

        public WineSensorThreshold[] EndGetThresholdHistory(IAsyncResult result)
        {
            GetThresholdHistoryDelegate del = GetAsyncDelegate(result) as GetThresholdHistoryDelegate;

            return del.EndInvoke(result);
        }

        public IAsyncResult BeginGetWineCabinets(AsyncCallback callback, Object state)
        {
            GetWineCabinetsDelegate del = new GetWineCabinetsDelegate(GetWineCabinets);
            IAsyncResult res = del.BeginInvoke(callback, state);

            AddAysncDelegate(res, del);

            return res;
        }

        public CabinetItem[] EndGetWineCabinets(IAsyncResult result)
        {
            GetWineCabinetsDelegate del = GetAsyncDelegate(result) as GetWineCabinetsDelegate;

            return del.EndInvoke(result);
        }



        public IAsyncResult BeginChangeThresholds(string cabinetId, WineSensorThreshold data, AsyncCallback callback, object state)
        {
            ChangeThresholdsDelegate del = new ChangeThresholdsDelegate(ChangeThresholds);
            IAsyncResult res = del.BeginInvoke(cabinetId, data, callback, state);

            AddAysncDelegate(res, del);

            return res;
        }

        public void EndChangeThresholds(IAsyncResult result)
        {
            ChangeThresholdsDelegate del = GetAsyncDelegate(result) as ChangeThresholdsDelegate;

            del.EndInvoke(result);
        }

        #endregion
    }
}
