////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.SPOT.Emulator;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using Microsoft.SPOT.Emulator.Lcd;

using Microsoft.SPOT.Emulator.TouchPanel;

namespace Microsoft.SPOT.Emulator.Sample
{    
    /// <summary>
    /// A WinForm control to display the contents of an LCD of a .NET Micro Framework application
    /// </summary>
    public class LcdControl : Control
    {
        // The .NET Micro Framework LCD emulator component
        Lcd.LcdDisplay _lcd;
        TouchGpioPort _touchPort = null;

        // A bitmap to store the current LCD contents
        Bitmap _bitmap;

        public LcdControl()
        {            
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.Opaque, true);            
        }

        /// <summary>
        /// The LCD emulator component
        /// </summary>
        public Lcd.LcdDisplay LcdDisplay
        {
            get { return _lcd; }
            set
            {
                if (_lcd != null)
                {
                    _lcd.OnDevicePaint -= new OnDevicePaintEventHandler(OnDevicePaint);
                }
                
                _lcd = value;

                if (_lcd != null)
                {
                    _lcd.OnDevicePaint += new OnDevicePaintEventHandler(OnDevicePaint);
                }
            }
        }

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
        /// Callback that runs when the application flushes the LCD buffer to the screen to copy the emulator's internal bitmap to our control.
        /// </summary>
        /// <param name="sender">The emulator component firing the event</param>
        /// <param name="args">What is being redrawn</param>
        private void OnDevicePaint(object sender, OnDevicePaintEventArgs args)
        {
            Bitmap bitmap = args.Bitmap;

            if (_bitmap == null)
            {
                // The first time the callback occurs, simply make a copy of the LCD bitmap.  Necessary
                // so the .NET Micro Framework can draw on its frame buffer after this callback returns.
                _bitmap = (Bitmap)bitmap.Clone();
            }
            else
            {
                // Synchronize the _bitmap object to prevent conflict between the Micro Framework thread (which this 
                // callback runs on) and the UI thread (which runs during paint).
                lock (_bitmap)
                {
                    Rectangle rectangle = new Rectangle(0, 0, _bitmap.Width, _bitmap.Height);

                    // Lock the source and target bitmaps in memory so they can't move while we're copying them
                    BitmapData bdSrc = bitmap.LockBits(rectangle, System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);
                    BitmapData bdDst = _bitmap.LockBits(rectangle, System.Drawing.Imaging.ImageLockMode.WriteOnly, bitmap.PixelFormat);

                    // Copy the bitmap data, 4 bytes (an int) at a time, using unsafe code.
                    // Copying the entire frame buffer can be substantially slower in safe code
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
                    
                    // Unlock the source and target bitmaps
                    bitmap.UnlockBits(bdSrc);
                    _bitmap.UnlockBits(bdDst);                     
                }
            }            

            // Force this control to be redrawn
            this.Invalidate(); 
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            // Paint the LCD control.  Use the contents of the LCD if available, otherwise call through to the base class.
            if (_bitmap != null)
            {
                // Synchonize access to the bitmap with the .NET Micro Framework thread
                lock (_bitmap)
                {
                    e.Graphics.DrawImage(_bitmap, 0, 0); // draw our LCD contents
                }
            }
            else // we have no private LCD bitmap (the .NET Micro Framework hasn't called our OnDevicePaint yet)
            {
                base.OnPaintBackground(e);
            }

            if (this.DesignMode)
            {             
                // At design time, paint a dotted outline around the control
                OnPaintDesignMode(e);
            }

            base.OnPaint(e);
        }

        /// <summary>
        /// Simple design mode to allow this control to be used with the WinForm designer
        /// </summary>
        private void OnPaintDesignMode(PaintEventArgs e)
        {
            Rectangle rc = this.ClientRectangle;
            Color penColor;

            // Choose black or white pen depending on the color of the control
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

        const int TouchSampleValidFlag = 0x01;
        const int TouchSampleDownFlag = 0x02;
        const int TouchSampleIsCalibratedFlag = 0x04;
        const int TouchSamplePreviousDownFlag = 0x08;
        const int TouchSampleIgnore = 0x10;
        const int TouchSampleMouse = 0x40000000;

        int flags = 0;
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            flags = TouchSampleValidFlag | TouchSampleDownFlag; 

            _touchPort.WriteTouchData(flags, 0, e.X, e.Y);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            flags = TouchSampleValidFlag | TouchSamplePreviousDownFlag;

            _touchPort.WriteTouchData(flags, 0, e.X, e.Y);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if ((flags & (TouchSampleValidFlag | TouchSampleDownFlag)) == (TouchSampleValidFlag | TouchSampleDownFlag))
            {
                flags = TouchSampleValidFlag | TouchSamplePreviousDownFlag | TouchSampleDownFlag;

                if ((e.X >= 0) && (e.Y >= 0))
                {
                    _touchPort.WriteTouchData(flags, 0, e.X, e.Y);
                }
            }
        }
    }
}
