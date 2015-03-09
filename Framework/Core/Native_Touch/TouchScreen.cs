////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.SPOT.Touch
{
    public class TouchScreen : IEventListener
    {
        public class ActiveRectangle
        {   
            public ActiveRectangle(int x, int y, int width, int height, object target)
            {
                this.X = x;
                this.Y = y;
                this.Width = width;
                this.Height = height;
                this.Target = target;
            }
            
            public bool Contains(TouchInput input)
            {
                if(
                    input.X >= this.X               &&
                    input.X <  this.X + this.Width  && 
                    input.Y >= this.Y               &&
                    input.Y <  this.Y + this.Height    
                  )
                {
                    return true;
                }
                return false;
            }
            
            //--//
            
            public readonly int X;
            public readonly int Y;
            public readonly int Width;
            public readonly int Height;
            public readonly object Target;
        }

        //--//
        
        private ActiveRectangle[] _activeRegions;
        private readonly int _maxWidth;
        private readonly int _maxHeight;

        //--//
        
        public TouchScreen(ActiveRectangle[] activeRectangles)
        {
            int bpp, orientation;
            Microsoft.SPOT.Hardware.HardwareProvider hwProvider = Microsoft.SPOT.Hardware.HardwareProvider.HwProvider;
            hwProvider.GetLCDMetrics(out _maxWidth, out _maxHeight, out bpp, out orientation);

            if (activeRectangles == null || activeRectangles.Length == 0)
            {
                this.ActiveRegions = new ActiveRectangle[] { new ActiveRectangle(0, 0, _maxWidth, _maxHeight, null) };
            }
            else
            {
                this.ActiveRegions = activeRectangles;
            }
        }

        //--//
        public event TouchScreenEventHandler OnTouchDown;
        public event TouchScreenEventHandler OnTouchMove;
        public event TouchScreenEventHandler OnTouchUp;
        //--//
        public event TouchGestureEventHandler OnGestureStarted;
        public event TouchGestureEventHandler OnGestureChanged;
        public event TouchGestureEventHandler OnGestureEnded;
        //--//

        public ActiveRectangle[] ActiveRegions
        {
            set
            {
                // check
                for(int i = 0; i < value.Length; ++i)
                {
                    ActiveRectangle ar = value[i];
                    if( ar.X < 0 || ar.X >= _maxWidth || ar.Y < 0 || ar.Y >= _maxHeight)                    
                    {
                        throw new ArgumentException();
                    }
                }
                
                _activeRegions = value;
            }
            get 
            {
                return _activeRegions;
            }
        }
        
        //--//
        
        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        void IEventListener.InitializeForEventSource()
        {
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        bool IEventListener.OnEvent(BaseEvent ev)
        {
            /// Process known events, otherwise forward as generic to MainWindow.
            ///

            TouchEvent touchEvent = ev as TouchEvent;
            if (touchEvent != null)
            {
                // dispatch only when the event is in the active area            
                for(int i = 0; i < _activeRegions.Length; ++i)
                {
                    ActiveRectangle ar = _activeRegions[i];
                    
                    // only check the first 
                    if(ar.Contains(touchEvent.Touches[0]))
                    {
                        TouchScreenEventArgs tea = new TouchScreenEventArgs(touchEvent.Time, touchEvent.Touches, ar.Target);
                        
                        switch((TouchMessages)touchEvent.EventMessage)
                        {
                            case TouchMessages.Down:
                                if(OnTouchDown != null) 
                                {
                                    OnTouchDown(this, tea);
                                }                            
                                break;
                            case TouchMessages.Up:
                                if(OnTouchUp != null) 
                                {
                                    OnTouchUp(this, tea);
                                }
                                break;
                            case TouchMessages.Move:
                                if(OnTouchMove != null) 
                                {
                                    OnTouchMove(this, tea);
                                }
                                break;
                        }
                    }
                }
                
                return true;
            }
            else if(ev is GenericEvent)
            {
                GenericEvent genericEvent = (GenericEvent)ev;
                switch (genericEvent.EventCategory)
                {
                    case (byte)EventCategory.Gesture:
                    {
                        TouchGestureEventArgs ge = new TouchGestureEventArgs();

                        ge.Gesture = (TouchGesture)genericEvent.EventMessage;
                        ge.X = genericEvent.X;
                        ge.Y = genericEvent.Y;
                        ge.Arguments = (ushort)genericEvent.EventData;

                        if (ge.Gesture == TouchGesture.Begin && OnGestureStarted != null)
                        {
                            OnGestureStarted(this, ge);
                        }
                        else if (ge.Gesture == TouchGesture.End && OnGestureEnded!= null)
                        {
                            OnGestureEnded(this, ge);
                        }
                        else if(OnGestureChanged != null)
                        {
                            OnGestureChanged(this, ge);
                        }

                        break;
                    }
                    default:
                       break;
                }
            }
            
            return false;
        }
        
    }
}


