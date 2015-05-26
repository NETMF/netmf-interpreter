using System;
using System.Collections;
using Microsoft.SPOT;

namespace Windows.Devices.Gpio
{
    internal class GpioPinEvent : BaseEvent
    {
        public int PinNumber;
        public GpioPinEdge Edge;
    }

    internal class GpioPinEventListener : IEventProcessor, IEventListener
    {
        // Map of pin numbers to GpioPin objects.
        private IDictionary m_pinMap = new Hashtable();

        public GpioPinEventListener()
        {
            EventSink.AddEventProcessor(EventCategory.Gpio, this);
            EventSink.AddEventListener(EventCategory.Gpio, this);
        }

        public BaseEvent ProcessEvent(uint data1, uint data2, DateTime time)
        {
            return new GpioPinEvent
            {
                // Data1 is packed by PostManagedEvent, so we need to unpack the high word.
                PinNumber = (int)(data1 >> 16),
                Edge = (data2 == 0) ? GpioPinEdge.FallingEdge : GpioPinEdge.RisingEdge,
            };
        }

        public void InitializeForEventSource()
        {
        }

        public bool OnEvent(BaseEvent ev)
        {
            var pinEvent = (GpioPinEvent)ev;
            GpioPin pin = null;

            lock (m_pinMap)
            {
                if (m_pinMap.Contains(pinEvent.PinNumber))
                {
                    pin = (GpioPin)m_pinMap[pinEvent.PinNumber];
                }
            }

            // Avoid calling this under a lock to prevent a potential lock inversion.
            if (pin != null)
            {
                pin.OnPinChangedInternal(pinEvent.Edge);
            }

            return true;
        }

        public void AddPin(int pinNumber, GpioPin pin)
        {
            lock (m_pinMap)
            {
                m_pinMap[pin.PinNumber] = pin;
            }
        }

        public void RemovePin(int pinNumber)
        {
            lock (m_pinMap)
            {
                if (m_pinMap.Contains(pinNumber))
                {
                    m_pinMap.Remove(pinNumber);
                }
            }
        }
    }
}
