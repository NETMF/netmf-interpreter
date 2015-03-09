using System;
using Microsoft.SPOT;
using Ws.Services.WsaAddressing;
using Dpws.Device;
using Dpws.Device.Services;
using Microsoft.SPOT.Platform.Test;
using Ws.Services.Soap;

namespace Interop.EventingService
{

    // EventSimulator - simulates events sent by a service
    
    class EventSimulator
    {
        DpwsHostedService m_hostedService = null;
        static bool rentryFlag = false;
        
        // Constructor must remember which service contains our event
        public EventSimulator(DpwsHostedService hostedService)
        {
            m_hostedService = hostedService;
        }

        // This method is called by the timer delegate periodically to send events
        public void SendEvent(Object stateInfo)
        {
            if (rentryFlag == true)     // prevent this method from running concurrently with itself
                return;
            rentryFlag = true;

            // Fire SimpleEvent
            try
            {
                DpwsWseEventSource eventSource = m_hostedService.EventSources["SimpleEvent"];
                WsWsaHeader header = new WsWsaHeader("http://schemas.example.org/EventingService/SimpleEvent", null, null, null, null, null);

                WsMessage msg = new WsMessage(header, null, WsPrefix.Wse);

                msg.Body = System.Text.UTF8Encoding.UTF8.GetBytes(BuildSimpleEventMessage(m_hostedService));

                Device.SubscriptionManager.FireEvent(m_hostedService, eventSource, msg );
            }
            catch
            {
                //Log.Comment("");
                //Log.Comment("SimpleEvent FireEvent failed: " + e.Message);
                //Log.Comment("");
                rentryFlag = false;
            }

            // Fire IntegerEvent
            try
            {
                DpwsWseEventSource eventSource = m_hostedService.EventSources["IntegerEvent"];
                WsWsaHeader header = new WsWsaHeader("http://schemas.example.org/EventingService/IntegerEvent", null, null, null, null, null);

                WsMessage msg = new WsMessage(header, null, WsPrefix.Wse);

                msg.Body = System.Text.UTF8Encoding.UTF8.GetBytes(BuildIntegerEventMessage(m_hostedService));
                                
                Device.SubscriptionManager.FireEvent(m_hostedService, eventSource, msg);
            }
            catch
            {
                //Log.Comment("");
                //Log.Comment("IntegerEvent FireEvent failed: " + e.Message);
                //Log.Comment("");
                rentryFlag = false;
            }

            rentryFlag = false;
        }

        // Build a SimpleEvent
        private String BuildSimpleEventMessage(DpwsHostedService hostedService)
        {
            return "<evnt:SimpleEvent xmlns:evnt='" + hostedService.ServiceNamespace.NamespaceURI + "'/>";
        }

        Int32 integerEventSerial = 0;

        // Build an IntegerEvent
        private String BuildIntegerEventMessage(DpwsHostedService hostedService)
        {
            integerEventSerial++;
            return "<evnt:IntegerEvent xmlns:evnt='" + hostedService.ServiceNamespace.NamespaceURI + "'>" +
                "<evnt:Param>" + integerEventSerial + "</evnt:Param>" + 
                "</evnt:IntegerEvent>";
        }
    }
}
