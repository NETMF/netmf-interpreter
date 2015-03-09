////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.SPOT.Emulator;
using Microsoft.SPOT.Hardware;

namespace Microsoft.SPOT.Emulator.Sample
{
    /// <summary>
    /// A WinForm control that connects a button to a GPIO pin.
    /// </summary>
    public class Button : Control
    {
        // The GPIO pin this button controls.
        Gpio.GpioPort _port;
        // Whether the button is pressed.
        bool _pressed;

        Image _image;
        Image _imagePressed;

        // A key that will enable input via the keyboard.
        Keys _key;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fState"></param>
        delegate void PortWriteDelegate(bool fState);

        /// <summary>
        /// The default constructor.
        /// </summary>
        public Button()
        {
            _image = Properties.Resources.DefaultButtonUp;
            _imagePressed = Properties.Resources.DefaultButtonDown;

            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            this.SetStyle(ControlStyles.Opaque, true);
        }

        /// <summary>
        /// Gets or sets the GPIO pin that this button is responsible for 
        /// toggling.
        /// </summary>
        public Gpio.GpioPort Port
        {
            get { return _port; }
            set { _port = value; }
        }

        /// <summary>
        /// The key that this button should respond to.  Useful mainly if this 
        /// control is a child of a ButtonCollection control that has keyboard 
        /// focus.
        /// </summary>
        public Keys Key
        {
            get { return _key; }
            set { _key = value; }
        }

        /// <summary>
        /// Sets the state of the button.
        /// </summary>
        /// <param name="pressed">Whether the button is pressed.</param>
        internal void OnButtonStateChanged(bool pressed)
        {
            if (_port != null)
            {
                if (_pressed != pressed)
                {
                    _pressed = pressed;
                    bool val = false;

                    switch (_port.Resistor)
                    {
                        case 
                        Microsoft.SPOT.Emulator.Gpio.GpioResistorMode.Disabled:
                        case 
                        Microsoft.SPOT.Emulator.Gpio.GpioResistorMode.PullUp:
                            val = pressed;
                            break;
                        case 
                        Microsoft.SPOT.Emulator.Gpio.GpioResistorMode.PullDown:
                            val = !pressed;
                            break;
                    }

                    // Marshal the port-write to the Micro Framework thread.  
                    // There's no need to wait for a response.

                    _port.BeginInvoke(
                        new PortWriteDelegate(_port.Write), !val
                        );

                    this.Invalidate();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool IsInputKey(Keys keyData)
        {
            return _key == keyData;
        }

        /// <summary>
        /// Paint the control based on the button state.  Draw Image or 
        /// ImagePressed, as appropriate.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            Image image = _pressed ? _imagePressed : _image;

            e.Graphics.DrawImage(image, 0, 0, 
                new Rectangle(0, 0, image.Width, image.Height), 
                GraphicsUnit.Pixel);

            base.OnPaint(e);
        }

        /// <summary>
        /// If this control has focus, any keypress will trigger the associated 
        /// GPIO port.  Normally, this control will not have focus, but instead 
        /// be a child of a ButtonCollection.
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            OnButtonStateChanged(true);

            base.OnKeyDown(e);
        }

        /// <summary>
        /// If this control has focus, any keypress will trigger the GPIO port.  
        /// Normally, this control will not have focus, but instead be a child 
        /// of a ButtonCollection.
        /// </summary>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            OnButtonStateChanged(false);

            base.OnKeyUp(e);
        }

        /// <summary>
        /// Respond to a mouse-down event by pressing the button.
        /// </summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            OnButtonStateChanged(true);

            base.OnMouseDown(e);
        }

        /// <summary>
        /// Respond to a mouse-up event by releasing the button.
        /// </summary>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            OnButtonStateChanged(false);

            base.OnMouseUp(e);
        }
    }
}
