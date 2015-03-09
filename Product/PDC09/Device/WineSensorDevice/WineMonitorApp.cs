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
using localhost.WineMonitorService;
using schemas.datacontract.org.WineMonitorService;
using schemas.datacontract.org.WineMonitorDevice;
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
        IWineMonitorUpdateClientProxy m_WineMonitorProxy;
        string                        m_cabinetId;
        Timer                         m_updateTimer;
        bool                          m_timerStarted;
        UpdateSensorData              m_updateReq;
        UpdateThresholds              m_thresReq;

        public WineServiceConnection()
        {
            m_threadLock = new object();
            m_WineMonitorProxy = new IWineMonitorUpdateClientProxy();
            m_cabinetId = "NetMFWineCabinet";
            m_timerStarted = false;
            m_updateReq = new UpdateSensorData();
            m_thresReq = new UpdateThresholds();

            m_updateReq.cabinetId = m_cabinetId;
            m_updateReq.data = new WineSensorData();

            m_thresReq.cabinetId = m_cabinetId;
            m_thresReq.data = new WineSensorThreshold();

            m_updateTimer = new Timer(new TimerCallback(OnUpdate), null, Timeout.Infinite, Timeout.Infinite);
            
            m_WineMonitorProxy.Init();

            // Turn listening to this IP on
            m_WineMonitorProxy.IgnoreRequestFromThisIP = false;

            m_WineMonitorProxy.ServiceEndpoint = "http://localhost:52305/WineMonitorUpdate.svc";

            //server part

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

            // Start the device
            _DV.Device.Start();

            // register with wine cabinet service
            RegisterWineCabinet req = new RegisterWineCabinet();
            req.cabinetId = m_cabinetId;
            req.endpointConfigName = _DV.Device.ThisDevice.FriendlyName;
            req.endpointAddress = _DV.Device.Host.EndpointRefs[0].Address.AbsoluteUri;
            m_WineMonitorProxy.RegisterWineCabinet(req);

            // update the current thresholds
            SendThresholds(null);
        }


        void SendAlert(AlertFlags alert, double value, DateTime timestamp)
        {
            Alert reqAlert = new Alert();

            reqAlert.cabinetId = m_cabinetId;
            reqAlert.alert = new AlertData();
            reqAlert.alert.Alert =  alert;
            reqAlert.alert.AlertValue = value;
            reqAlert.alert.TimeStamp = timestamp;

            m_WineMonitorProxy.Alert(reqAlert);
        }

        void SendThresholds(object arg)
        {
            UpdateThresholds reqThresh = new UpdateThresholds();
            reqThresh.cabinetId = m_cabinetId;
            reqThresh.data = new WineSensorThreshold();
            reqThresh.data.MaxHumidity = 94;
            reqThresh.data.MinHumidity = 22;
            reqThresh.data.MaxTemperature = 84;
            reqThresh.data.MinTemperature = 12;
            m_WineMonitorProxy.UpdateThresholds(reqThresh);
        }

        void OnUpdate(object arg)
        {
            lock (m_threadLock)
            {
                m_timerStarted = false;
            }

            m_WineMonitorProxy.UpdateSensorData(m_updateReq);
        }

        void DispatchTimer()
        {
            lock (m_threadLock)
            {
                if (!m_timerStarted)
                {
                    m_timerStarted = true;
                    m_updateTimer.Change(5000, 0);
                }
            }
        }

        #region IIWineMonitorRequest Members

        public RequestUpdateResponse RequestUpdate(RequestUpdate req)
        {
            RequestUpdateResponse resp = new RequestUpdateResponse();

            OnUpdate(null);

            return resp;
        }

        public SetThresholdsResponse SetThresholds(SetThresholds req)
        {
            SetThresholdsResponse resp = new SetThresholdsResponse();


            return resp;
        }

        #endregion

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
                if (m_WineMonitorProxy != null)
                {
                    m_WineMonitorProxy.Dispose();
                    m_WineMonitorProxy = null;
                }
            }
            // free native resources
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public static void Main()
        {
            string xml =
            "<?xml version='1.0' encoding='UTF-8'?>" +
                /*
                "<soap:Envelope " +
                "    xmlns:soap='http://www.w3.org/2003/05/soap-envelope' " +
                "    xmlns:wsa='http://www.w3.org/2005/08/addressing' " +
                "    xmlns:wsdp='http://schemas.xmlsoap.org/ws/2006/02/devprof' >" +
                "<soap:Header>" +
                "    <wsa:To soap:mustUnderstand=\"1\">http://www.w3.org/2005/08/addressing/anonymous</wsa:To>" +
                "    <wsa:Action soap:mustUnderstand=\"1\">http://schemas.exceptionalinnovation.com/ws/2006/04/Service/State/GetStateResponse</wsa:Action>" +
                "    <wsa:MessageID>urn:uuid:06fd1c39-2af7-6c8d-a067-466a37dd9c3c</wsa:MessageID>" +
                "    <wsa:RelatesTo>urn:uuid:370a600f-17f3-4208-a6fc-6b064da3b578</wsa:RelatesTo>" +
                "</soap:Header>" +
                "<soap:Body>" +
                */
            "<State xmlns=\"http://schemas.exceptionalinnovation.com/device/2006/04/ContactClosure\" " +
            "       xmlns:at1=\"http://schemas.example.org/at1\" " +
            "       at1:AnyAttribute_at1=\"Any attibute value = at1\" " +
            "       xmlns:at2=\"http://schemas.example.org/at2\" " +
            "       at2:AnyAttribute_at2=\"Any attibute value = at2\"> " +
            "<LastChanged>1601-01-01T00:00:00Z</LastChanged>" +
            "<ServiceId></ServiceId>" +
            "<Status>false</Status>" +
            "<AnyElement_te1 xmlns:te1=\"http://schemas.example.org/te1\" " +
            "       xmlns:at1=\"http://schemas.example.org/at1\" " +
            "       at1:AnyAttribute_at1=\"Any attibute value = at1\">Any Element value = te1</AnyElement_te1>" +
            "<AnyElement_te2 xmlns:te2=\"http://schemas.example.org/te2\" " +
            "       xmlns:at1=\"http://schemas.example.org/at1\" " +
            "       at1:AnyAttribute_at1=\"Any attibute value = at1\">Any Element value = te2</AnyElement_te2>" +
            "</State>";
            //"</soap:Body></soap:Envelope>";


            System.IO.MemoryStream ms = new System.IO.MemoryStream(System.Text.UTF8Encoding.UTF8.GetBytes(xml));
            System.Xml.XmlReader xr = System.Xml.XmlReader.Create(ms);

            xr.ReadStartElement();
           // while (xr.MoveToElement())
            {
                while (xr.Read())
                {
                    Debug.Print(xr.Name);
                    Debug.Print(xr.Prefix);
                    Debug.Print(xr.NamespaceURI);
                }
            }


            WineServiceConnection tst = new WineServiceConnection();
            tst.Start();
        }

        public void Start()
        {
            RegisterWineCabinet req = new RegisterWineCabinet();
            req.cabinetId = m_cabinetId;
            req.endpointConfigName = _DV.Device.ThisDevice.FriendlyName;
            req.endpointAddress = _DV.Device.Host.EndpointRefs[0].Address.AbsoluteUri;

            m_WineMonitorProxy.RegisterWineCabinet(req);

            m_thresReq.data.MaxHumidity = 84;
            m_thresReq.data.MinHumidity = 30;
            m_thresReq.data.MaxMinutesWithoutUpdate = 60;
            m_thresReq.data.MaxTemperature = 74;
            m_thresReq.data.MinTemperature = 54;
            m_thresReq.data.TimeStamp = DateTime.UtcNow;

            m_WineMonitorProxy.UpdateThresholds(m_thresReq);

            Random rand = new Random();
            // Continuous run loop
            while (true)
            {
                try
                {
                    m_WineMonitorProxy.RegisterWineCabinet(req);

                    UpdateSensorData data = new UpdateSensorData();
                    data.cabinetId = m_cabinetId;
                    data.data = new schemas.datacontract.org.WineMonitorService.WineSensorData();
                    data.data.Humidity = (m_thresReq.data.MaxHumidity - m_thresReq.data.MinHumidity + 4) * rand.NextDouble() + m_thresReq.data.MinHumidity + 2;
                    data.data.Temperature = (m_thresReq.data.MaxTemperature - m_thresReq.data.MinTemperature + 10) * rand.NextDouble() + m_thresReq.data.MinTemperature + 5;

                    data.data.TimeStamp = DateTime.UtcNow;

                    m_WineMonitorProxy.UpdateSensorData(data);

                    if (data.data.Temperature > m_thresReq.data.MaxTemperature)
                    {
                        SendAlert(AlertFlags.TempHigh, data.data.Temperature, data.data.TimeStamp);
                    }

                    Thread.Sleep(5000);

                }
                catch (Exception e)
                {
                    System.Ext.Console.Write("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! UpdateSensorData Failed. " + e.Message);
                }
            }
        }
    }
}
