using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace WineMonitorService
{
    [ServiceContract(Namespace = "http://localhost/WineMonitorAlert")]
    public interface IWineMonitorAlert
    {
        [OperationContract]
        void Alert(AlertData alert);
    }

    [DataContract]
    public enum AlertType
    {
        [EnumMember()]
        None,
        [EnumMember()]
        RadioFailure,
        [EnumMember()]
        TemperatureHigh,
        [EnumMember()]
        TemperatureLow,
        [EnumMember()]
        HumidityHigh,
        [EnumMember()]
        HumidityLow,
    }

    [DataContract]
    public class AlertData
    {
        AlertType alertType;
        double alertValue;
        DateTime timestamp;

        [DataMember]
        public AlertType Alert
        {
            get { return alertType; }
            set { alertType = value; }
        }

        [DataMember]
        public double AlertValue
        {
            get { return alertValue; }
            set { alertValue = value; }
        }

        [DataMember]
        public DateTime Timestamp
        {
            get { return timestamp; }
            set { timestamp = value; }
        }
    }
}
