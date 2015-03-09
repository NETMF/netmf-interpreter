////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using Microsoft.SPOT.Hardware;

namespace Microsoft.SPOT
{
    public class EventSink : NativeEventDispatcher
    {
        private class EventInfo
        {
            public EventInfo()
            {
                EventListener = null;
                EventFilter = null;
            }

            public IEventListener EventListener;
            public IEventListener EventFilter;
            public IEventProcessor EventProcessor;
            public EventCategory Category;
        }

        static EventSink()
        {
            _eventSink = new EventSink();
            _eventSink.OnInterrupt += new NativeEventHandler(_eventSink.EventDispatchCallback);
        }

        // Pass the name to the base so it connects to driver
        private EventSink()
            : base("EventSink", 0)
        {
        }

        private void ProcessEvent(EventInfo eventInfo, BaseEvent ev)
        {
            if (eventInfo == null)
                return;

            if (eventInfo.EventFilter != null)
            {
                if (!eventInfo.EventFilter.OnEvent(ev))
                    return;
            }

            if (eventInfo.EventListener != null)
            {
                eventInfo.EventListener.OnEvent(ev);
            }
        }

        private void EventDispatchCallback(uint data1, uint data2, DateTime time)
        {
            EventInfo eventInfo = null;
            BaseEvent ev = null;

            GetEvent(data1, data2, time, ref eventInfo, ref ev);

            ProcessEvent(eventInfo, ev);
        }

        ///

        /// Add/RemoveEventFilter/Listener/Processor today supports only one listener and one filter
        /// to reduce complexity, but this will certainly be not the case in future when
        /// multiple parties will want to listent or filter same EventCategory. This was
        /// one of the request from SideShow team, we will have to look into that.
        ///

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public static void AddEventListener(EventCategory eventCategory, IEventListener eventListener)
        {
            EventInfo eventInfo = GetEventInfo(eventCategory);
            eventInfo.EventListener = eventListener;
            eventListener.InitializeForEventSource();
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public static void AddEventFilter(EventCategory eventCategory, IEventListener eventFilter)
        {
            EventInfo eventInfo = GetEventInfo(eventCategory);
            eventInfo.EventFilter = eventFilter;
            eventFilter.InitializeForEventSource();

        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public static void AddEventProcessor(EventCategory eventCategory, IEventProcessor eventProcessor)
        {
            EventInfo eventInfo = GetEventInfo(eventCategory);
            eventInfo.EventProcessor = eventProcessor;
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public static void RemoveEventListener(EventCategory eventCategory, IEventListener eventListener)
        {
            EventInfo eventInfo = GetEventInfo(eventCategory);

            if (eventInfo.EventListener == eventListener)
            {
                eventInfo.EventListener = null;
            }
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public static void RemoveEventFilter(EventCategory eventCategory, IEventListener eventFilter)
        {
            EventInfo eventInfo = GetEventInfo(eventCategory);

            if (eventInfo.EventFilter == eventFilter)
            {
                eventInfo.EventFilter = null;
            }
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public static void RemoveEventProcessor(EventCategory eventCategory, IEventProcessor eventProcessor)
        {
            EventInfo eventInfo = GetEventInfo(eventCategory);

            if (eventInfo.EventProcessor == eventProcessor)
            {
                eventInfo.EventProcessor = null;
            }
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public static void PostManagerEvent(byte category, byte subCategory, ushort data1, uint data2)
        {
            if (_eventSink != null)
            {
                uint d = (uint)(((uint)data1 << 16) | ((uint)category << 8) | subCategory);
                DateTime time = DateTime.Now;
                _eventSink.EventDispatchCallback(d, data2, time);
            }
        }

        private static EventInfo GetEventInfo(EventCategory category)
        {

            /// What we need here is hashtable. Until we get one, we have this implementation where we browse through
            /// registered eventInfos and attempt retrieving matchine one.
            ///

            EventInfo eventInfo = null;
            for (int i = 0; i < _eventInfoTable.Count; i++)
            {
                if (((EventInfo)_eventInfoTable[i]).Category == category)
                {
                    eventInfo = (EventInfo)_eventInfoTable[i];
                    break;
                }
            }

            if (eventInfo == null)
            {
                eventInfo = new EventInfo();
                eventInfo.Category = category;
                _eventInfoTable.Add(eventInfo);
            }

            return eventInfo;
        }

        private static void GetEvent(uint data1, uint data2, DateTime time, ref EventInfo eventInfo, ref BaseEvent ev)
        {
            byte category = (byte)((data1 >> 8) & 0xFF);

            eventInfo = GetEventInfo((EventCategory)category);
            if (eventInfo.EventProcessor != null)
            {
                ev = eventInfo.EventProcessor.ProcessEvent(data1, data2, time);
            }
            else
            {
                GenericEvent genericEvent = new GenericEvent();
                genericEvent.Y = (int)(data2 & 0xFFFF);
                genericEvent.X = (int)((data2 >> 16) & 0xFFFF);
                genericEvent.Time = time;
                genericEvent.EventMessage = (byte)(data1 & 0xFF);
                genericEvent.EventCategory = category;
                genericEvent.EventData = (data1 >> 16) & 0xFFFF;

                ev = genericEvent;
            }
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern void EventConfig();

        private static EventSink _eventSink = null;
        private static ArrayList _eventInfoTable = new ArrayList();
    }
}


