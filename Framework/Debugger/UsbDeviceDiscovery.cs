//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Microsoft .NET Micro Framework Porting Kit Code Samples and is unsupported. 
// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use these files except in compliance with the License.
// You may obtain a copy of the License at:
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing
// permissions and limitations under the License.
// 
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Text;
using System.Management;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.SPOT.Debugger
{
    public class UsbDeviceDiscovery : IDisposable
    {
        public enum DeviceChanged : ushort
        {
            None = 0,
            Configuration = 1,
            DeviceArrival = 2,
            DeviceRemoval = 3,
            Docking = 4,
        }

        public delegate void DeviceChangedEventHandler(DeviceChanged change);

        private const string c_EventQuery = "Win32_DeviceChangeEvent";
        private const string c_InstanceQuery = "SELECT * FROM __InstanceOperationEvent WITHIN 5 WHERE TargetInstance ISA \"Win32_PnPEntity\"";

        ManagementEventWatcher m_eventWatcher;
        DeviceChangedEventHandler m_subscribers;

        public UsbDeviceDiscovery()
        {
        }

        ~UsbDeviceDiscovery()
        {
            try
            {
                Dispose();
            }
            catch
            {
            }
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public void Dispose()
        {
            if (m_eventWatcher != null)
            {
                m_eventWatcher.Stop();

                m_eventWatcher = null;
                m_subscribers = null;
            }
            GC.SuppressFinalize(this);
        }

        // subscribing to this event allows applications to be notified when USB devices are plugged and unplugged
        // as well as configuration changed and docking; upon receiving teh notification the applicaion can decide
        // to call UsbDeviceDiscovery.EnumeratePorts to get an updated list of Usb devices
        public event DeviceChangedEventHandler OnDeviceChanged
        {
            [MethodImplAttribute(MethodImplOptions.Synchronized)]
            add
            {
                try
                {
                    TryEventNotification(value);
                }
                catch
                {
                    TryInstanceNotification(value);
                }
            }

            [MethodImplAttribute(MethodImplOptions.Synchronized)]
            remove
            {
                m_subscribers -= value;

                if (m_subscribers == null)
                {
                    if (m_eventWatcher != null)
                    {
                        m_eventWatcher.Stop();
                        m_eventWatcher = null;
                    }
                }
            }
        }

        private void TryEventNotification(DeviceChangedEventHandler handler)
        {
            m_eventWatcher = new ManagementEventWatcher(new WqlEventQuery(c_EventQuery));

            m_eventWatcher.EventArrived += new EventArrivedEventHandler(HandleDeviceEvent);

            if (m_subscribers == null)
            {
                m_eventWatcher.Start();
            }

            m_subscribers += handler;
        }

        private void TryInstanceNotification(DeviceChangedEventHandler handler)
        {
            m_eventWatcher = new ManagementEventWatcher(new WqlEventQuery(c_InstanceQuery));

            m_eventWatcher.EventArrived += new EventArrivedEventHandler(HandleDeviceInstance);

            if (m_subscribers == null)
            {
                m_eventWatcher.Start();
            }

            m_subscribers += handler;
        }

        private void HandleDeviceEvent(object sender, EventArrivedEventArgs args)
        {
            if (m_subscribers != null)
            {
                ManagementBaseObject deviceEvent = args.NewEvent;

                ushort eventType = (ushort)deviceEvent["EventType"];

                m_subscribers((DeviceChanged)eventType);
            }
        }

        private void HandleDeviceInstance(object sender, EventArrivedEventArgs args)
        {
            if (m_subscribers != null)
            {
                ManagementBaseObject deviceEvent = args.NewEvent;

                if (deviceEvent.ClassPath.ClassName.Equals("__InstanceCreationEvent"))
                {
                    m_subscribers(DeviceChanged.DeviceArrival);
                }
                else if (deviceEvent.ClassPath.ClassName.Equals("__InstanceDeletionEvent"))
                {
                    m_subscribers(DeviceChanged.DeviceRemoval);
                }
            }
        }
    }
}
