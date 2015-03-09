using System;
using System.Threading;
using Ws.Services;
using Ws.Services.Xml;
using Dpws.Device.Services;

namespace Interop.EventingService
{

    // EventingService - Provides events
    // Events supported:
    //      SimpleEvent: Message only, does not send any data
    //      IntegerEvent: Sends a message with an integer value
    //
    // See also EventSimulator, the class that actually sends the events.

    class EventingService : DpwsHostedService, IDisposable
    {

        // Temporary event simulator declarations
        AutoResetEvent m_autoEvent = null;
        EventSimulator m_eventSim = null;
        Timer m_stateTimer = null;

        // Constructor sets service properties and adds event sources
        public EventingService(ProtocolVersion version) : base(version)
        {
            // Add ServiceNamespace. Set ServiceID and ServiceTypeName
            ServiceNamespace = new WsXmlNamespace("evnt", "http://schemas.example.org/EventingService");
            ServiceID = "urn:uuid:3cb0d1ba-cc3a-46ce-b416-212ac2419b91";
            ServiceTypeName = "EventingService";
            
            // Add event sources
            EventSources.Add(new DpwsWseEventSource("evnt", "http://schemas.example.org/EventingService", "SimpleEvent"));
            EventSources.Add(new DpwsWseEventSource("evnt", "http://schemas.example.org/EventingService", "IntegerEvent"));
            this.AddEventServices();

            // Start the event simulator
            StartEventSimulator();
        }

        public EventingService() : 
            this(new ProtocolVersion10())
        {
        }

        // Dispose method stops and kills Event Simulator timers
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Release resources used by this object
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopEventSimulator();
                m_autoEvent = null;
                m_eventSim = null;
                m_stateTimer = null;
            }
            // no unmanaged resources to release
        }

        // Initialize and start event simulator
        private void StartEventSimulator()
        {
            m_autoEvent = new AutoResetEvent(true);
            m_eventSim = new EventSimulator(this);

            // Create the delegate that invokes methods for the timer.
            TimerCallback timerDelegate = new TimerCallback(m_eventSim.SendEvent);

            m_stateTimer = new Timer(timerDelegate, m_autoEvent, 10000, 20000);
            return;
        }

        // Stop event simulator
        private void StopEventSimulator()
        {
            if (m_stateTimer != null)
            {
                m_stateTimer.Dispose();
                m_stateTimer = null;
            }
            if (m_autoEvent != null)
            {
                m_autoEvent = null;
            }
            return;
        }
        
    }

}
