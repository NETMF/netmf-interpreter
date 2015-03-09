////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Input;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Presentation;

//
//  This class should be defined on a per-OEM basis based on their hardware beshavior.
//


namespace Microsoft.SPOT.Sample
{
    public sealed class GPIOButtonInputProvider
    {
        internal class ButtonPad
        {
            public ButtonPad(GPIOButtonInputProvider sink, Button button, Cpu.Pin pin, Port.InterruptMode mode)
            {
                _sink     = sink;
                _button   = button;
                _buttonDevice = InputManager.CurrentInputManager.ButtonDevice;

                _port = new InterruptPort( pin, true, Port.ResistorMode.PullUp, mode );

                _port.OnInterrupt += new NativeEventHandler(this.Interrupt);
            }

            void Interrupt( uint data1, uint data2, DateTime time )
            {
                // queue the button press to the input provider site.
                RawButtonActions action = (data2 != 0) ? 
                    RawButtonActions.ButtonUp : RawButtonActions.ButtonDown;

                RawButtonInputReport report = 
                    new RawButtonInputReport(_sink._source, time, _button, action);

                _sink.Dispatcher.BeginInvoke(_sink._callback, new InputReportArgs(_buttonDevice, report));
            }

            Button                      _button;
            InterruptPort               _port;
            GPIOButtonInputProvider     _sink;
            ButtonDevice                _buttonDevice;
        }

        public GPIOButtonInputProvider(PresentationSource source)
        {
            _source   = source;
            _site     = InputManager.CurrentInputManager.RegisterInputProvider(this);
            _callback = new DispatcherOperationCallback(delegate(object report)
                    {
                        InputReportArgs args = (InputReportArgs)report;
                        return _site.ReportInput(args.Device, args.Report);
                    });
            Dispatcher = Dispatcher.CurrentDispatcher;

            ButtonPad[] buttons = new ButtonPad[]
            {
                new ButtonPad(this, Button.VK_MENU,   Pins.GPIO_PORT_A_3, Port.InterruptMode.InterruptEdgeBoth),
                new ButtonPad(this, Button.VK_SELECT, Pins.GPIO_PORT_A_8, Port.InterruptMode.InterruptEdgeBoth),
                new ButtonPad(this, Button.VK_LEFT,   Pins.GPIO_PORT_A_7, Port.InterruptMode.InterruptEdgeBoth),
                new ButtonPad(this, Button.VK_RIGHT,  Pins.GPIO_PORT_A_5, Port.InterruptMode.InterruptEdgeBoth),
                new ButtonPad(this, Button.VK_UP,     Pins.GPIO_PORT_A_4, Port.InterruptMode.InterruptEdgeBoth),
                new ButtonPad(this, Button.VK_DOWN,   Pins.GPIO_PORT_A_6, Port.InterruptMode.InterruptEdgeBoth),
                };

            _buttons = buttons;
        }

        public  readonly Dispatcher Dispatcher;

        private ButtonPad[]                 _buttons;
        private DispatcherOperationCallback _callback;
        private InputProviderSite           _site;
        private PresentationSource          _source;
    }
}


