using System;
using System.Threading;
using Dpws.Client.Discovery;
using Dpws.Client.Eventing;
using Dpws.Client;
using _DV=Dpws.Device;
using Dpws.Device.Services;
using Ws.Services;
using Ws.Services.Xml;
using Ws.Services.Utilities;
using Ws.Services.WsaAddressing;
using localhost.WineMonitorDevice;
using schemas.datacontract.org.WineMonitorDevice;
using localhost.WineMonitorAlert;
using schemas.datacontract.org.WineMonitorService;
using Microsoft.SPOT.Time;

#if !Windows && !WindowsCE
using System.Ext;
#endif

namespace Microsoft.SPOT.Samples.SaveMyWine
{
    /// <summary>
    /// SimpleService test application class.
    /// </summary>
    public class WineServiceConnection : IIWineMonitorRequest, IDisposable
    {
        object                        m_threadLock;
        WineSensorData                m_currentData;
        WineDataModel                 m_model;
        bool                          m_started;
        IWineMonitorAlertClientProxy  m_alertProxy;

        public WineServiceConnection(WineDataModel model)
        {
            m_threadLock = new object();
            m_started = false;

            m_currentData = new WineSensorData();

            m_model = model;

            // Add a Host service type
            _DV.Device.Host = new IWineMonitorRequest(this);

            // Set device information
            _DV.Device.EndpointAddress = _DV.Device.Host.ServiceID;
            _DV.Device.ThisModel.Manufacturer = "Microsoft Corporation";
            _DV.Device.ThisModel.ManufacturerUrl = "http://www.microsoft.com/";
            _DV.Device.ThisModel.ModelName = "WineMonitorDevice Test Device";
            _DV.Device.ThisModel.ModelNumber = "1.0";
            _DV.Device.ThisModel.ModelUrl = "http://www.microsoft.com/";
            _DV.Device.ThisModel.PresentationUrl = "http://www.microsoft.com/";
            _DV.Device.ThisDevice.FriendlyName = "WineMonitorDevice";
            _DV.Device.ThisDevice.FirmwareVersion = "alpha";
            _DV.Device.ThisDevice.SerialNumber = "12345678";

            // Add Dpws hosted service(s) to the device
            _DV.Device.HostedServices.Add(_DV.Device.Host);

            // Set this device property if you want to ignore this clients request
            _DV.Device.IgnoreLocalClientRequest = false;

            // Turn console messages on
            Console.Verbose = true;
        }

        public bool StartService()
        {
            if (m_started) return true;

            try
            {
                // Start the device
                _DV.Device.Start();

                m_model.OnHumidityChanged += new WineDataModel.HumidityChangedEventHandler(model_OnHumidityChanged);
                m_model.OnTemperatureChanged += new WineDataModel.TemperatureChangedEventHandler(model_OnTemperatureChanged);
                 
                ///
                /// CUSTOMIZATION POINT: Change this ip address to match the host PC that will run the WineMonitorApp
                ///
                m_alertProxy = new IWineMonitorAlertClientProxy();
                m_alertProxy.ServiceEndpoint = "http://172.30.178.218:8085/WineMonitorAlert";

                m_model.OnHumidityThresholdExceeded += new WineDataModel.HumidityThresholdExceededEventHandler(m_model_OnHumidityThresholdExceeded);
                m_model.OnTemperatureThresholdExceeded += new WineDataModel.TemperatureThresholdExceededEventHandler(m_model_OnTemperatureThresholdExceeded);
               
                m_started = true;
            }
            catch
            {
                return false;
            }

            return true;
        }

        void m_model_OnTemperatureThresholdExceeded(object sender, WineDataModel.TemperatureThresholdExceededEvent e)
        {
            try
            {
                Alert req = new Alert();
                req.alert = new AlertData();
                req.alert.Alert = e.Last >= m_model.TemperatureAlarmThresholdRange.Maximum ? AlertType.TemperatureHigh : AlertType.TemperatureLow;
                req.alert.AlertValue = e.Last;
                req.alert.Timestamp = e.TimeOccurred.ToUniversalTime();
                m_alertProxy.Alert(req);
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }
        }

        void m_model_OnHumidityThresholdExceeded(object sender, WineDataModel.HumidityThresholdExceededEvent e)
        {
            try
            {
                Alert req = new Alert();
                req.alert = new AlertData();
                req.alert.Alert = e.Last >= m_model.HumidityAlarmThresholdRange.Maximum ? AlertType.HumidityHigh : AlertType.HumidityLow;
                req.alert.AlertValue = e.Last;
                req.alert.Timestamp = e.TimeOccurred.ToUniversalTime();
                m_alertProxy.Alert(req);
            }
            catch(Exception ex)
            {
                Console.Write(ex.ToString());
            }
        }

        public void StopService()
        {
            if (!m_started) return;

            m_model.OnHumidityChanged               -= new WineDataModel.HumidityChangedEventHandler             (model_OnHumidityChanged);
            m_model.OnTemperatureChanged            -= new WineDataModel.TemperatureChangedEventHandler          (model_OnTemperatureChanged);
            m_model.OnHumidityThresholdExceeded     -= new WineDataModel.HumidityThresholdExceededEventHandler   (m_model_OnHumidityThresholdExceeded);
            m_model.OnTemperatureThresholdExceeded  -= new WineDataModel.TemperatureThresholdExceededEventHandler(m_model_OnTemperatureThresholdExceeded);
            
            try
            {
                _DV.Device.Stop();
            }
            catch
            {
            }

            m_started = false;
        }


        void model_OnTemperatureChanged(object sender, WineDataModel.TemperatureChangedEvent e)
        {
            m_currentData.Temperature = e.Last;
            m_currentData.TimeStamp = e.TimeOccurred;
        }

        void model_OnHumidityChanged(object sender, WineDataModel.HumidityChangedEvent e)
        {
            m_currentData.Humidity = e.Last;
            m_currentData.TimeStamp = e.TimeOccurred;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_started)
                {
                    StopService();
                }
            }
            // free native resources
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #region IIWineMonitorRequest Members

        public GetSensorDataResponse GetSensorData(GetSensorData req)
        {
            GetSensorDataResponse res = new GetSensorDataResponse();
            res.GetSensorDataResult = m_currentData;
            res.GetSensorDataResult.TimeStamp = res.GetSensorDataResult.TimeStamp.ToUniversalTime();

            return res;
        }

        #endregion
    }
}
