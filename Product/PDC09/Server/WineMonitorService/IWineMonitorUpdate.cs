using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Net.Security;

namespace WineMonitorService
{
    [ServiceContract(Namespace = "http://localhost/WineMonitorService/", ProtectionLevel = ProtectionLevel.None)]
    public interface IWineMonitorUpdate
    {
        [OperationContract(Action = "http://localhost/WineMonitorService/IWineMonitorUpdate/RegisterWineCabinet", ProtectionLevel = ProtectionLevel.None)]
        void RegisterWineCabinet(string cabinetId, string endpointConfigName, string endpointAddress);

        [OperationContract(Action = "http://localhost/WineMonitorService/IWineMonitorUpdate/UpdateSensorData", ProtectionLevel = ProtectionLevel.None)]
        void UpdateSensorData(string cabinetId, WineSensorData data);

        [OperationContract(Action = "http://localhost/WineMonitorService/IWineMonitorUpdate/UpdateThresholds", ProtectionLevel = ProtectionLevel.None)]
        void UpdateThresholds(string cabinetId, WineSensorThreshold data);

        [OperationContract(Action = "http://localhost/WineMonitorService/IWineMonitorUpdate/Alert", ProtectionLevel = ProtectionLevel.None)]
        void Alert(string cabinetId, AlertData alert);
    }
}
