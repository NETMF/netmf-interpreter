using System;
using System.Threading;
using Dpws.Device;
using Dpws.Device.Services;
using Ws.Services.Utilities;
using Microsoft.SPOT;

namespace Dpws.Device.Services
{
    /// <summary>
    /// This class manages event subscription expirations. A thread runs that monitors a collection of event sinks.
    /// When an event subscrition for the event sink expires, this class removes the event from the collectionn.
    /// </summary>
    sealed class DpwsWseEventSinkQMgr
    {
        private Timer m_timer;

        /// <summary>
        /// Creates an instance of the DpwsWseEventSinkQManager class.
        /// </summary>
        public DpwsWseEventSinkQMgr()
        {
            m_timer = new Timer(new TimerCallback(this.EventService), null, Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// Method used to start the event subscription monitoring process.
        /// </summary>
        public void Start()
        {
            m_timer.Change(0, 5000);
        }

        /// <summary>
        /// Method used to stop the expiration manager thread
        /// </summary>
        public void Stop()
        {
            m_timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// Method used to expire events and send EndTo notification to an event sink
        /// </summary>
        public void EventService(object arg)
        {
            DateTime curDateTime = DateTime.Now;

            try
            {
                // Loop through hosted services
                int servicesCount = Device.HostedServices.Count;
                for (int i = 0; i < servicesCount; i++)
                {
                    DpwsHostedService hostedService = (DpwsHostedService)Device.HostedServices[i];

                    // Loop through event sources
                    int eventSourcesCount = hostedService.EventSources.Count;
                    for (int j = 0; j < eventSourcesCount; j++)
                    {
                        DpwsWseEventSource eventSource = hostedService.EventSources[j];

                        // Loop through event sinks. Check expiration time. If an event has
                        // expired send subscription end message and delete event sink.
                        int eventSinkCount = eventSource.EventSinks.Count;
                        for (int k = eventSinkCount-1; k >= 0 ; k--)
                        {
                            DpwsWseEventSink eventSink = eventSource.EventSinks[k];

                            // If time has expired delete event sink
                            if (eventSink.StartTime + eventSink.Expires > curDateTime.Ticks)
                                eventSource.EventSinks.RemoveAt(k);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                System.Ext.Console.Write("Event Queue Manager threw and exception. " + e.Message);
            }
        }
    }
}


