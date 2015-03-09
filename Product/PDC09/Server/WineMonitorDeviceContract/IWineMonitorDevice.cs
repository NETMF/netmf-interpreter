using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Net.Security;

namespace WineMonitorDevice
{
    [ServiceContract(Namespace = "http://localhost/WineMonitorDevice/", ProtectionLevel = ProtectionLevel.None)]
    public interface IWineMonitorRequest
    {
        [OperationContract(Action = "http://localhost/WineMonitorDevice/RequestUpdate", ProtectionLevel = ProtectionLevel.None)]
        void RequestUpdate();

        [OperationContract(Action = "http://localhost/WineMonitorDevice/SetThresholds", ProtectionLevel = ProtectionLevel.None)]
        void SetThresholds(WineSensorThresholdReq thresholds);
    }

    [DataContract]
    public class WineSensorThresholdReq
    {
        double minHumidity;
        double maxHumidity;
        double minTemperature;
        double maxTemperature;
        int maxMinutesWithoutUpdates;
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
            get { return maxMinutesWithoutUpdates; }
            set { maxMinutesWithoutUpdates = value; }
        }

        [DataMember]
        public DateTime TimeStamp
        {
            get { return timestamp; }
            set { timestamp = value; }
        }
    }
}
