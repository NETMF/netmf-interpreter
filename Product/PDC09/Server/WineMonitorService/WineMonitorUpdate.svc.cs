using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Activation;

namespace WineMonitorService
{
    [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class WineMonitorUpdate : IWineMonitorUpdate
    {
        #region IWineMonitorUpdate Members

        public void RegisterWineCabinet(string cabinetId, string endpointConfigName, string endpointAddress)
        {
            lock (WineMonitorService.s_wineCabinets)
            {
                if (!WineMonitorService.s_wineCabinets.ContainsKey(cabinetId))
                {
                    WineMonitorService.s_wineCabinets[cabinetId] = new WineCabinetData(cabinetId, endpointConfigName, endpointAddress);
                }
                else
                {
                    WineMonitorService.s_wineCabinets[cabinetId].EndpointAddress = endpointAddress;
                    WineMonitorService.s_wineCabinets[cabinetId].EndpointConfigName = endpointConfigName;
                }
            }
        }

        public void Alert(string cabinetId, AlertData alert)
        {
            if (alert == null) return;

            if (!WineMonitorService.s_wineCabinets.ContainsKey(cabinetId))
            {
                return;
            }

            lock (WineMonitorService.s_wineCabinets)
            {
                if (WineMonitorService.s_wineCabinets[cabinetId].AlertData.Count == 0 || alert.TimeStamp > WineMonitorService.s_wineCabinets[cabinetId].AlertData[0].TimeStamp)
                {
                    WineMonitorService.s_wineCabinets[cabinetId].AlertData.Insert(0, alert);
                }
            }

        }

        public void UpdateSensorData(string cabinetId, WineSensorData data)
        {
            if (data == null) return;

            if (!WineMonitorService.s_wineCabinets.ContainsKey(cabinetId))
            {
                return;
            }

            lock (WineMonitorService.s_wineCabinets)
            {
                if (WineMonitorService.s_wineCabinets[cabinetId].SensorData.Count == 0 || data.TimeStamp > WineMonitorService.s_wineCabinets[cabinetId].SensorData[0].TimeStamp)
                {
                    WineMonitorService.s_wineCabinets[cabinetId].SensorData.Insert(0, data);
                }
            }
        }

        public void UpdateThresholds(string cabinetId, WineSensorThreshold data)
        {
            if (data == null) return;

            if (!WineMonitorService.s_wineCabinets.ContainsKey(cabinetId))
            {
                return;
            }

            lock (WineMonitorService.s_wineCabinets)
            {
                if (WineMonitorService.s_wineCabinets[cabinetId].ThresholdData.Count == 0 || data.TimeStamp > WineMonitorService.s_wineCabinets[cabinetId].ThresholdData[0].TimeStamp)
                {
                    WineMonitorService.s_wineCabinets[cabinetId].ThresholdData.Insert(0, data);
                }
            }
        }

        #endregion
    }
}
