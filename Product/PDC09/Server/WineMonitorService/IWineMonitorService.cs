using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Net.Security;
using System.Text;
using System.IO;

namespace WineMonitorService
{
    [ServiceContract(Namespace = "http://localhost/WineMonitorService/", ProtectionLevel = ProtectionLevel.None)]
    public interface IWineMonitor
    {
        [OperationContract(AsyncPattern = true)] //, Action = "http://localhost/WineMonitorService/IWineMonitor/GetSensorData", ProtectionLevel = ProtectionLevel.None)]
        IAsyncResult BeginGetSensorData(string cabinetId, AsyncCallback callback, Object state);
        WineSensorData EndGetSensorData(IAsyncResult result);

        [OperationContract(AsyncPattern = true)] //, Action = "http://localhost/WineMonitorService/IWineMonitor/GetAlert", ProtectionLevel = ProtectionLevel.None)]
        IAsyncResult BeginGetAlert(string cabinetId, AsyncCallback callback, Object state);
        AlertData EndGetAlert(IAsyncResult result);

        [OperationContract(AsyncPattern = true)] //, Action = "http://localhost/WineMonitorService/IWineMonitor/GetThresholdValues", ProtectionLevel = ProtectionLevel.None)]
        IAsyncResult BeginGetThresholdValues(string cabinetId, AsyncCallback callback, Object state);
        WineSensorThreshold EndGetThresholdValues(IAsyncResult result);

        [OperationContract(AsyncPattern = true)] //, Action = "http://localhost/WineMonitorService/IWineMonitor/GetWineSensorDataHistory", ProtectionLevel = ProtectionLevel.None)]
        IAsyncResult BeginGetWineSensorDataHistory(string cabinetId, DateTime dataSince, AsyncCallback callback, Object state);
        WineSensorData[] EndGetWineSensorDataHistory(IAsyncResult result);

        [OperationContract(AsyncPattern = true)] //, Action = "http://localhost/WineMonitorService/IWineMonitor/GetAlertHistory", ProtectionLevel = ProtectionLevel.None)]
        IAsyncResult BeginGetAlertHistory(string cabinetId, DateTime alertsSince, AsyncCallback callback, Object state);
        AlertData[] EndGetAlertHistory(IAsyncResult result);

        [OperationContract(AsyncPattern = true)] //, Action = "http://localhost/WineMonitorService/IWineMonitor/GetThresholdHistory", ProtectionLevel = ProtectionLevel.None)]
        IAsyncResult BeginGetThresholdHistory(string cabinetId, DateTime changesSince, AsyncCallback callback, Object state);
        WineSensorThreshold[] EndGetThresholdHistory(IAsyncResult result);

        [OperationContract(AsyncPattern = true)] //, Action = "http://localhost/WineMonitorService/IWineMonitor/GetWineCabinets", ProtectionLevel = ProtectionLevel.None)]
        IAsyncResult BeginGetWineCabinets(AsyncCallback callback, Object state);
        CabinetItem[] EndGetWineCabinets(IAsyncResult result);

        [OperationContract(AsyncPattern = true)] //, Action = "http://localhost/WineMonitorService/IWineMonitorUpdate/ChangeThresholds", ProtectionLevel = ProtectionLevel.None)]
        IAsyncResult BeginChangeThresholds(string cabinetId, WineSensorThreshold data, AsyncCallback callback, Object state);
        void EndChangeThresholds(IAsyncResult result);
    }

    [ServiceContract(Namespace = "http://localhost/WineMonitorService/", ProtectionLevel = ProtectionLevel.None)]
    public interface IWineMonitorWeb
    {
        [OperationContract(ProtectionLevel = ProtectionLevel.None)]
        [WebInvoke(Method = "GET", 
            RequestFormat = WebMessageFormat.Json, 
            ResponseFormat = WebMessageFormat.Json, 
            UriTemplate = "GetHtmlPage?cabinetId={cabinetId}")]
        Stream GetHtmlPage(string cabinetId);
    }

    [DataContract]
    public enum AlertFlags
    {
        [EnumMember()]
        None,
        [EnumMember()]
        TempHigh,
        [EnumMember()]
        TempLow,
        [EnumMember()]
        HumidHigh,
        [EnumMember()]
        HumidLow,
        [EnumMember()]
        NoComm,
    }

    [DataContract]
    public class CabinetItem
    {
        string cabinet;

        [DataMember]
        public string Cabinet
        {
            get { return cabinet; }
            set { cabinet = value; }
        }
    }

    [DataContract]
    public class AlertData
    {
        AlertFlags alert;
        double alertValue;
        DateTime timestamp;

        [DataMember]
        public AlertFlags Alert
        {
            get { return alert; }
            set { alert = value; }
        }

        [DataMember]
        public double AlertValue
        {
            get { return alertValue; }
            set { alertValue = value; }
        }

        [DataMember]
        public DateTime TimeStamp
        {
            get { return timestamp; }
            set { timestamp = value; }
        }
    }

    [DataContract]
    public class WineSensorThreshold
    {
        double minHumidity;
        double maxHumidity;
        double minTemperature;
        double maxTemperature;
        int maxMinutesWithoutUpdate;
        DateTime timestamp;

        [DataMember]
        public double MinHumidity
        {
            get { return minHumidity; }
            set { minHumidity = value; }
        }
        [DataMember]
        public double MaxHumidity
        {
            get { return maxHumidity; }
            set { maxHumidity = value; }
        }
        [DataMember]
        public double MinTemperature
        {
            get { return minTemperature; }
            set { minTemperature = value; }
        }
        [DataMember]
        public double MaxTemperature
        {
            get { return maxTemperature; }
            set { maxTemperature = value; }
        }

        [DataMember]
        public int MaxMinutesWithoutUpdate
        {
            get { return maxMinutesWithoutUpdate; }
            set { maxMinutesWithoutUpdate = value; }
        }

        [DataMember]
        public DateTime TimeStamp
        {
            get { return timestamp; }
            set { timestamp = value; }
        }
    }


    [DataContract]
    public class WineSensorData
    {
        double humidity;
        double temperature;
        DateTime timestamp;

        [DataMember]
        public double Humidity
        {
            get { return humidity; }
            set { humidity = value; }
        }

        [DataMember]
        public double Temperature
        {
            get { return temperature; }
            set { temperature = value; }
        }
        [DataMember]
        public DateTime TimeStamp
        {
            get { return timestamp; }
            set { timestamp = value; }
        }
    }
}
