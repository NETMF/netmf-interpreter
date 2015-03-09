////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Microsoft.SPOT.Emulator;
using Microsoft.SPOT.Emulator.Lcd;
using Microsoft.SPOT.Emulator.TouchPanel;

namespace Microsoft.SPOT.Emulator.Sample
{
    /// <summary>
    /// A WinForm control to display the contents of an LCD of a .NET Micro 
    /// Framework application.
    /// </summary>
    public class LcdControl : Control
    {
        // The .NET Micro Framework LCD emulator component.
        Lcd.LcdDisplay _lcd;
        TouchGpioPort _touchPort = null;

        // A bitmap to store the current LCD contents.
        Bitmap _bitmap;

        /// <summary>
        /// The default constructor.
        /// </summary>
        public LcdControl()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.Opaque, true);
        }

        /// <summary>
        /// Gets or sets the LCD emulator component.
        /// </summary>
        public Lcd.LcdDisplay LcdDisplay
        {
            get { return _lcd; }
            set
            {
                if (_lcd != null)
                {
                    _lcd.OnDevicePaint -= 
                        new OnDevicePaintEventHandler(OnDevicePaint);
                }

                _lcd = value;

                if (_lcd != null)
                {
                    _lcd.OnDevicePaint += 
                        new OnDevicePaintEventHandler(OnDevicePaint);
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the GPIO port
        /// </summary>
        public TouchGpioPort TouchPort
        {
            get
            {
                return _touchPort;
            }

            set
            {
                _touchPort = value;
            }
        }

        /// <summary>
        /// Callback that runs when the application flushes the LCD buffer to 
        /// the screen.  Copies the emulator's internal bitmap to the control.
        /// </summary>
        /// <param name="sender">The emulator component firing the event.
        /// </param>
        /// <param name="args">What is being redrawn.</param>
        private void OnDevicePaint(object sender, OnDevicePaintEventArgs args)
        {
            Bitmap bitmap = args.Bitmap;

            if (_bitmap == null)
            {
                // The first time the callback occurs, simply make a copy of the 
                // LCD bitmap.  This is necessary so the .NET Micro Framework 
                // can draw on its frame buffer after this callback returns.
                _bitmap = (Bitmap)bitmap.Clone();
            }
            else
            {
                // Synchronize the _bitmap object to prevent conflict between 
                // the Micro Framework thread (which this callback runs on) and 
                // the UI thread (which runs during paint).
                lock (_bitmap)
                {
                    Rectangle rectangle =
                        new Rectangle(0, 0, _bitmap.Width, _bitmap.Height);

                    // Lock the source and target bitmaps in memory so they 
                    // can't move while we're copying them.
                    BitmapData bdSrc = bitmap.LockBits(rectangle,
                        System.Drawing.Imaging.ImageLockMode.ReadOnly,
                        bitmap.PixelFormat);
                    BitmapData bdDst = _bitmap.LockBits(rectangle,
                        System.Drawing.Imaging.ImageLockMode.WriteOnly,
                        bitmap.PixelFormat);

                    // Copy the bitmap data, 4 bytes (an int) at a time, using 
                    // unsafe code.  Copying the entire frame buffer can be 
                    // substantially slower in safe code
                    unsafe
                    {
                        int* src = (int*)bdSrc.Scan0.ToPointer();
                        int* dst = (int*)bdDst.Scan0.ToPointer();
                        int cInts = bdSrc.Stride / 4 * bitmap.Height;

                        Debug.Assert(bdSrc.Stride > 0);
                        Debug.Assert(bitmap.Width == _bitmap.Width);
                        Debug.Assert(bitmap.Height == _bitmap.Height);
                        Debug.Assert(bitmap.PixelFormat == _bitmap.PixelFormat);

                        for (int i = 0; i < cInts; i++)
                        {
                            *dst++ = *src++;
                        }
                    }

                    // Unlock the source and target bitmaps.
                    bitmap.UnlockBits(bdSrc);
                    _bitmap.UnlockBits(bdDst);
                }
            }

            // Force this control to be redrawn.
            this.Invalidate();
        }

        /// <summary>
        /// Handles painting.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            // Paint the LCD control.  Use the contents of the LCD if available; 
            // otherwise call through to the base class.
            if (_bitmap != null)
            {
                // Synchonize access to the bitmap with the .NET Micro Framework 
                // thread.
                lock (_bitmap)
                {
                    // Draw the LCD contents.
                    e.Graphics.DrawImage(_bitmap, 0, 0);

                    if (m_currentGesture == TouchGesture.Rotate && 
                        (flags & (TouchSampleValidFlag | TouchSampleDownFlag)) == (TouchSampleValidFlag | TouchSampleDownFlag))
                    {
                        e.Graphics.DrawEllipse(Pens.Red, _lastMouseDownX - c_GestureRotateXOffset, _lastMouseDownY, 3, 3);
                    }
                }
            }
            else
            // We have no private LCD bitmap, because the .NET Micro Framework 
            // hasn't called OnDevicePaint yet.
            {
                base.OnPaintBackground(e);
            }

            if (this.DesignMode)
            {
                // At design time, paint a dotted outline around the control.
                OnPaintDesignMode(e);
            }

            base.OnPaint(e);
        }

        /// <summary>
        /// Simple design mode to allow this control to be used with the WinForm 
        /// designer.
        /// </summary>
        private void OnPaintDesignMode(PaintEventArgs e)
        {
            Rectangle rc = this.ClientRectangle;
            Color penColor;

            // Choose black or white pen, depending on the color of the control.
            if (this.BackColor.GetBrightness() < .5)
            {
                penColor = ControlPaint.Light(this.BackColor);
            }
            else
            {
                penColor = ControlPaint.Dark(this.BackColor); ;
            }

            using (Pen pen = new Pen(penColor))
            {
                pen.DashStyle = DashStyle.Dash;

                rc.Width--;
                rc.Height--;
                e.Graphics.DrawRectangle(pen, rc);
            }
        }

        /// <summary>
        /// Send mouse events as touch events to PAL.
        /// _touchPort.WriteTouchData works like an actual hardware event.
        /// </summary>

        const int TouchSampleValidFlag = 0x01;
        const int TouchSampleDownFlag = 0x02;
        const int TouchSampleIsCalibratedFlag = 0x04;
        const int TouchSamplePreviousDownFlag = 0x08;
        const int TouchSampleIgnore = 0x10;
        const int TouchSampleMouse = 0x40000000;

        int flags = 0;

        ///
        /// Make sure that this enum is kept up to date with TinyCore's Gesture enum
        ///
        public enum TouchGesture : uint
        {
            NoGesture = 0,          //Can be used to represent an error gesture or unknown gesture
        
            //Standard Win7 Gestures
            Begin       = 1,       //Used to identify the beginning of a Gesture Sequence; App can use this to highlight UIElement or some other sort of notification.
            End         = 2,       //Used to identify the end of a gesture sequence; Fired when last finger involved in a gesture is removed.
        
            // Standard stylus (single touch) gestues
            Right       = 3,
            UpRight     = 4,
            Up          = 5,
            UpLeft      = 6,
            Left        = 7,
            DownLeft    = 8,
            Down        = 9,
            DownRight   = 10,
            Tap         = 11,
            DoubleTap   = 12,
        
            // Multi-touch gestures
            Zoom         = 114,      //Equivalent to your "Pinch" gesture
            Pan          = 115,      //Equivalent to your "Scroll" gesture
            Rotate       = 116,
            TwoFingerTap = 117,
            Rollover     = 118,      // Press and tap               
        
            //Additional NetMF gestures
            UserDefined = 65537
        }


        TouchGesture m_currentGesture        = TouchGesture.NoGesture;
        int          m_GestureNumKeysPressed = 0;
        const int    c_GestureRotateXOffset  = 20;

        ///
        /// Emulate multitouch gestures - By pressing CTRL+x (where x is one of the keyboard keys in the switch statement below).
        /// This currently enables all multitouch gestures supported by .Net MF.
        ///
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Control && m_currentGesture == TouchGesture.NoGesture)
            {
                switch (e.KeyCode)
                {
                    case Keys.NumPad2:
                        m_GestureNumKeysPressed = 2;
                        break;
                    case Keys.NumPad3:
                        m_GestureNumKeysPressed = 3;
                        break;

                    case Keys.P:
                        m_currentGesture = TouchGesture.Pan;
                        break;
                    
                    case Keys.Z:
                        m_currentGesture = TouchGesture.Zoom;
                        break;
                    
                    case Keys.R:
                        m_currentGesture = TouchGesture.Rotate;
                        break;

                    case Keys.T:
                        m_currentGesture = TouchGesture.TwoFingerTap;
                        break;

                    case Keys.O:
                        m_currentGesture = TouchGesture.Rollover;
                        break;
                }

                if (m_currentGesture != TouchGesture.NoGesture)
                {
                    _touchPort.PostGesture((int)TouchGesture.Begin, _lastMouseDownX, _lastMouseDownY, 0);
                    if (m_currentGesture == TouchGesture.TwoFingerTap || m_currentGesture == TouchGesture.Rollover)
                    {
                        _touchPort.PostGesture((int)m_currentGesture, _lastMouseDownX, _lastMouseDownY, 0);
                    }
                }
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.NumPad2:
                case Keys.NumPad3:
                    m_GestureNumKeysPressed = 0;
                    break;

                case Keys.P:
                case Keys.Z:
                case Keys.R:
                case Keys.T:
                case Keys.O:
                    m_currentGesture = TouchGesture.NoGesture;
                    _touchPort.PostGesture((int)TouchGesture.End, _lastMouseDownX, _lastMouseDownY, 0);
                    break;
            }

            base.OnKeyUp(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);

            if (0 != (flags & TouchSampleDownFlag))
            {
                if (m_currentGesture != TouchGesture.NoGesture)
                {
                    m_currentGesture = TouchGesture.NoGesture;
                    _touchPort.PostGesture((int)TouchGesture.End, _lastMouseDownX, _lastMouseDownY, 0);
                }

                MouseEventArgs mea = new MouseEventArgs(MouseButtons.Left, 1, _lastMouseDownX, _lastMouseDownY, 0);

                OnMouseUp(mea);
            }
        }

        int _lastMouseDownX = 0;
        int _lastMouseDownY = 0;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            Focus();

            flags = TouchSampleValidFlag | TouchSampleDownFlag;

            _lastMouseDownX = e.X;
            _lastMouseDownY = e.Y;

            _touchPort.WriteTouchData(flags, 0, e.X, e.Y);

            switch(m_GestureNumKeysPressed)
            {
                case 2:
                    _touchPort.WriteTouchData(flags, 1, e.X, e.Y);
                    break;
                case 3:
                    _touchPort.WriteTouchData(flags, 1, e.X, e.Y);
                    _touchPort.WriteTouchData(flags, 2, e.X, e.Y);
                    break;
            }
        }

        /// <summary>
        /// Handles a mouse-up event.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (m_currentGesture != TouchGesture.NoGesture)
            {
                HandleGesture(e.X, e.Y, true);

                m_currentGesture = TouchGesture.NoGesture;
                _touchPort.PostGesture((int)TouchGesture.End, _lastMouseDownX, _lastMouseDownY, 0);
            }

            flags = TouchSampleValidFlag | TouchSamplePreviousDownFlag;

            _touchPort.WriteTouchData(flags, 0, e.X, e.Y);

            switch (m_GestureNumKeysPressed)
            {
                case 2:
                    _touchPort.WriteTouchData(flags, 1, e.X, e.Y);
                    break;
                case 3:
                    _touchPort.WriteTouchData(flags, 1, e.X, e.Y);
                    _touchPort.WriteTouchData(flags, 2, e.X, e.Y);
                    break;
            }

            m_GestureNumKeysPressed = 0;
            _lastAngle = 0.0;
            _lastDist = 0.0;
            _lastMouseDownX = 0;
            _lastMouseDownY = 0;

        }

        double _lastDist = 0.0;
        double _lastAngle = 0.0;

        private void HandleGesture(int x, int y, bool fForce)
        {
            if (m_currentGesture != TouchGesture.NoGesture)
            {
                ushort data = 0;

                int xCenter = _lastMouseDownX, yCenter = _lastMouseDownY;

                switch (m_currentGesture)
                {
                    case TouchGesture.Pan:
                    case TouchGesture.Zoom:
                        double dist = Math.Sqrt(Math.Pow(x - xCenter, 2.0) + Math.Pow(yCenter - y, 2.0));
                        if (Math.Abs(dist - _lastDist) > 10 || fForce)
                        {
                            if (x < xCenter && yCenter < y)
                            {
                                m_currentGesture = TouchGesture.Pan;
                                data = (ushort)dist;
                                _lastDist = dist;
                            }
                            else if(x > xCenter && yCenter > y)
                            {
                                m_currentGesture = TouchGesture.Zoom;
                                data = (ushort)dist;
                                _lastDist = dist;
                            }
                        }
                        break;
                    case TouchGesture.Rotate:
                        xCenter -= c_GestureRotateXOffset;

                        double dx = (x - xCenter);
                        double dy = (yCenter - y);

                        double angle = 180.0 * Math.Atan(Math.Abs(dy) / Math.Abs(dx)) / Math.PI;

                        if (dx < 0)
                        {
                            if (dy < 0)
                            {
                                angle += 180;
                            }
                            else
                            {
                                angle = 180 - angle;
                            }
                        }
                        else if (dx > 0 && dy < 0)
                        {
                            angle = 360 - angle;
                        }

                        if (angle < 0) angle += 360.0;

                        // switch to clockwise rotational angle
                        angle = 360.0 - angle;

                        if ((Math.Abs(angle - _lastAngle) > c_GestureRotateXOffset && (360.0 - (angle - _lastAngle)) > c_GestureRotateXOffset) || fForce)
                        {
                            data = (ushort)(angle);
                            _lastAngle = angle;
                        }

                        break;
                }

                if (data != 0)
                {
                    _touchPort.PostGesture((int)m_currentGesture, xCenter, yCenter, data);
                }
            }
        }

        /// <summary>
        /// Handles a mouse move event.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if ((flags & (TouchSampleValidFlag | TouchSampleDownFlag)) == 
                (TouchSampleValidFlag | TouchSampleDownFlag))
            {
                flags = TouchSampleValidFlag | TouchSamplePreviousDownFlag | 
                    TouchSampleDownFlag;

                if ((e.X >= 0) && (e.Y >= 0))
                {
                    _touchPort.WriteTouchData(flags, 0, e.X, e.Y);

                    switch (m_GestureNumKeysPressed)
                    {
                        case 2:
                            _touchPort.WriteTouchData(flags, 1, e.X, e.Y);
                            break;
                        case 3:
                            _touchPort.WriteTouchData(flags, 1, e.X, e.Y);
                            _touchPort.WriteTouchData(flags, 2, e.X, e.Y);
                            break;
                    }

                    HandleGesture(e.X, e.Y, false);
                }
            }
        }
    }
}
