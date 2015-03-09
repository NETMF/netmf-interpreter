////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using System.Windows.Forms;
using Microsoft.SPOT.Emulator;
using Microsoft.SPOT.Emulator.TouchPanel;

namespace Microsoft.SPOT.Emulator.Sample
{
    /// <summary>
    /// Sample emulator for the .NET Micro Framework.
    /// </summary>
    public class TestEmulator : Emulator
    {
        private TestEmulatorForm _form;

        public override void SetupComponent()
        {
            RegisterComponent(new TouchGpioPort(TouchGpioPort.DefaultTouchPin));

            base.SetupComponent();
        }

        /// <summary>
        /// Called by the emulator after all components have been set up and registered
        /// </summary>
        public override void InitializeComponent()
        {
            base.InitializeComponent();

            _form = new TestEmulatorForm(this.Emulator);

            _form.OnInitializeComponent();

            // Launch the UI thread.
            Thread uiThread = new Thread(RunForm);
            uiThread.SetApartmentState(ApartmentState.STA);
            uiThread.Start();
        }

        /// <summary>
        /// Called by the emulator after the program exits
        /// </summary>
        public override void UninitializeComponent()
        {
            base.UninitializeComponent();

            // When the emulator is shutting down, exit the the WinForm application as well.
            Application.Exit();
        }

        /// <summary>
        /// Call the .NET method that will drive the emulator's UI
        /// </summary>        
        private void RunForm()
        {
            Application.Run(_form);

            // When the form exits, shut down the emulator.
            this.Emulator.Stop();
        }

        [STAThread]
        public static void Main()
        {
            (new TestEmulator()).Start();
        }

    }
}
