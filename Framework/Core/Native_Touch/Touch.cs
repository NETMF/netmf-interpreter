////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

using Microsoft.SPOT;

namespace Microsoft.SPOT.Touch
{

    internal class TouchEventProcessor : IEventProcessor
    {
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public BaseEvent ProcessEvent(uint data1, uint data2, DateTime time);
    }

    //--//
    
    public static class Touch
    {
        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public static void Initialize(IEventListener touchEventListener)
        {
            if (_initialized)
                return;

            /// Lack of constructors in native code, forces to
            /// initialize TouchPanel before we initialize event sink.

            /// We have only one touch panel right now.
            /// But this is to keep the options open for future.
            _activeTouchPanel = new TouchPanel();
            _activeTouchPanel.Enabled = true;

            /// Add a touch event processor.
            Microsoft.SPOT.EventSink.AddEventProcessor(EventCategory.Touch, new TouchEventProcessor());

            /// Start the event sink process. This will pump
            /// events neatly out of the other world.
            Microsoft.SPOT.EventSink.AddEventListener(EventCategory.Touch, touchEventListener);

            /// Also add generic for Gesture stuff.
            Microsoft.SPOT.EventSink.AddEventListener(EventCategory.Gesture, touchEventListener);

            _initialized = true;
        }

        public static TouchPanel ActiveTouchPanel
        {
            get
            {
                return _activeTouchPanel;
            }
        }

        private static bool _initialized = false;
        private static TouchPanel _activeTouchPanel = null;
    }
}


