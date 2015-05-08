////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.SPOT
{
    public enum EventCategory
    {
        Unknown     = 0,
        Touch       = 1,
        Gesture     = 2,
        Storage     = 3,
        Network     = 4,
        SleepLevel  = 5,
        PowerLevel  = 6,
        TimeService = 7,
        LargeBuffer = 8,
        Gpio        = 9,
        Custom      = 100,
    }

    public class BaseEvent
    {
        public ushort Source;
        public byte EventMessage;
    }

    public class GenericEvent : BaseEvent
    {
        public byte EventCategory;
        public uint EventData;
        public int X;
        public int Y;
        public DateTime Time;
    }

    namespace Touch
    {
        [FlagsAttribute]
        public enum TouchInputFlags : uint
        {
            None = 0x00,
            Primary = 0x0010,  //The Primary flag denotes the input that is passed to the single-touch Stylus events provided

            //no controls handle the Touch events.  This flag should be set on the TouchInput structure that represents
            //the first finger down as it moves around up to and including the point it is released.

            Pen = 0x0040,     //Hardware support is optional, but providing it allows for potentially richer applications.
            Palm = 0x0080,     //Hardware support is optional, but providing it allows for potentially richer applications.
        }
    
        ///
        /// IMPORTANT - This must be in sync with code in PAL and also TinyCore
        ///
        public enum TouchMessages : byte
        {
            Down = 1,
            Up = 2,
            Move = 3,
        }

        public class TouchInput
        {
            public int X;
            public int Y;
            public byte SourceID;
            public TouchInputFlags Flags;
            public uint ContactWidth;
            public uint ContactHeight;
        }

        public class TouchEvent : BaseEvent
        {
            public DateTime Time;
            public TouchInput[] Touches;
        }
        
        public class TouchScreenEventArgs : EventArgs
        {
            // Fields
            public TouchInput[] Touches;
            public DateTime TimeStamp;
            public object Target;
        
            // Methods
            public TouchScreenEventArgs(DateTime timestamp, TouchInput[] touches, object target)
            {
                this.Touches = touches;
                this.TimeStamp = timestamp;
                this.Target = target;
            }
        
            public void GetPosition(int touchIndex, out int x, out int y)
            {
                x = Touches[touchIndex].X;
                y = Touches[touchIndex].Y;
            }
        }
        
        //--//
        
        public delegate void TouchScreenEventHandler(object sender, TouchScreenEventArgs e);

        //--//
        
        public enum TouchGesture : uint
        {
            NoGesture = 0,          //Can be used to represent an error gesture or unknown gesture

            //Standard Win7 Gestures
            Begin = 1,       //Used to identify the beginning of a Gesture Sequence; App can use this to highlight UIElement or some other sort of notification.
            End = 2,       //Used to identify the end of a gesture sequence; Fired when last finger involved in a gesture is removed.

            // Standard stylus (single touch) gestues
            Right = 3,
            UpRight = 4,
            Up = 5,
            UpLeft = 6,
            Left = 7,
            DownLeft = 8,
            Down = 9,
            DownRight = 10,
            Tap = 11,
            DoubleTap = 12,

            // Multi-touch gestures
            Zoom = 114,      //Equivalent to your "Pinch" gesture
            Pan = 115,      //Equivalent to your "Scroll" gesture
            Rotate = 116,
            TwoFingerTap = 117,
            Rollover = 118,      // Press and tap

            //Additional NetMF gestures
            UserDefined = 200,
        }

        public class TouchGestureEventArgs : EventArgs
        {
            public readonly DateTime Timestamp;

            public TouchGesture Gesture;

            ///<note> X and Y form the center location of the gesture for multi-touch or the starting location for single touch </note>
            public int X;
            public int Y;

            /// <note>2 bytes for gesture-specific arguments.
            /// TouchGesture.Zoom: Arguments = distance between fingers
            /// TouchGesture.Rotate: Arguments = angle in degrees (0-360)
            /// </note>
            public ushort Arguments;

            public double Angle
            {
                get
                {
                    return (double)(Arguments);
                }
            }
        }

        //--//
        
        public delegate void TouchGestureEventHandler(object sender, TouchGestureEventArgs e);
    }

    public interface IEventProcessor
    {
        /// <summary>
        /// IEventProcessor should return null if it cannot process an event,
        /// in that case next processor will be given an opportunity.
        /// </summary>
        /// <param name="data1"></param>
        /// <param name="data2"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        BaseEvent ProcessEvent(uint data1, uint data2, DateTime time);
    }

    public interface IEventListener
    {
        void InitializeForEventSource();
        bool OnEvent(BaseEvent ev);
    }

    //--//

    public interface ILog
    {
        void Log(object o);
    }
}


