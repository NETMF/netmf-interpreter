using System;
using System.Management;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.SPOT.Debugger
{
    public class EmulatorDiscovery : IDisposable
    {
        public delegate void EmulatorChangedEventHandler();

        ManagementEventWatcher m_eventWatcher_Start;
        ManagementEventWatcher m_eventWatcher_Stop;
        EmulatorChangedEventHandler m_subscribers;

        ~EmulatorDiscovery()
        {
            Dispose();
        }

        public EmulatorDiscovery()
        {
            m_eventWatcher_Start = new ManagementEventWatcher(new WqlEventQuery("Win32_ProcessStartTrace"));
            m_eventWatcher_Start.EventArrived += new EventArrivedEventHandler(HandleProcessEvent);

            m_eventWatcher_Stop = new ManagementEventWatcher(new WqlEventQuery("Win32_ProcessStopTrace"));
            m_eventWatcher_Stop.EventArrived += new EventArrivedEventHandler(HandleProcessEvent);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Dispose()
        {
            if (m_eventWatcher_Start != null)
            {
                m_eventWatcher_Start.Stop();
                m_eventWatcher_Start = null;
            }
            if (m_eventWatcher_Stop != null)
            {
                m_eventWatcher_Stop.Stop();
                m_eventWatcher_Stop = null;
            }
            m_subscribers = null;
            GC.SuppressFinalize(this);
        }

        //
        // Subscribing to this event allows applications to be notified when emulators are started or stopped.
        //
        public event EmulatorChangedEventHandler OnEmulatorChanged
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            add
            {
                // subscribe to Wmi for Win32_DeviceChangeEvent
                if (m_subscribers == null)
                {
                    m_eventWatcher_Start.Start();
                    m_eventWatcher_Stop.Start();
                }

                m_subscribers += value;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            remove
            {
                m_subscribers -= value;

                if (m_subscribers == null)
                {
                    m_eventWatcher_Start.Stop();
                    m_eventWatcher_Stop.Stop();
                }
            }
        }

        void HandleProcessEventCore()
        {
            EmulatorChangedEventHandler subscribers = m_subscribers;

            if (subscribers != null)
            {
                subscribers();
            }
        }

        void HandleProcessEvent(object sender, EventArrivedEventArgs args)
        {
            HandleProcessEventCore();

            Thread.Sleep(1000); // Give the Emulator some time to open the pipes.

            HandleProcessEventCore();
        }
    }
}