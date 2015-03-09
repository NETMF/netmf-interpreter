using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Description;
using System.Threading;

namespace WineMonitorService
{
    public delegate void AlertNotification(AlertData data);


    public class WineMonitorService
    {
        ServiceHost m_host;
        volatile bool m_started = false;

        public void StartService(WineMonitorAlert alertObj)
        {
            try
            {
                m_host = new ServiceHost(alertObj);

                WSHttpBinding binding = new WSHttpBinding("WineMonitorService");

                // CONFIGURATION POINT: set the IP address of the host where this application is running 
                m_host.AddServiceEndpoint(typeof(IWineMonitorAlert), binding, "http://172.30.176.71:8085/WineMonitorAlert");

                m_host.OpenTimeout = new TimeSpan(10, 0, 0, 0);
                
                // Open the ServiceHost to start listening for messages.
                m_host.Open();

                m_started = true;
            }
            catch
            {
                if (m_host != null)
                {
                    m_host.Abort();
                }
            }
        }
      
        public void StopService()
        {
            if (m_started)
            {
                m_host.Close();
            }
        }
    }

    [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any, InstanceContextMode = InstanceContextMode.Single)]
    public class WineMonitorAlert : IWineMonitorAlert
    {
        public event AlertNotification OnAlert;

        #region IWineMonitorAlert Members

        public void Alert(AlertData alert)
        {
            if (OnAlert != null)
            {
                OnAlert(alert);
            }
        }

        #endregion
    }
}
