////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using Microsoft.SPOT.Emulator;
using Microsoft.SPOT.Emulator.BlockStorage;
using Microsoft.SPOT.Emulator.TouchPanel;

namespace Microsoft.SPOT.Emulator.Sample
{
    /// <summary>
    /// Sample emulator for the .NET Micro Framework.
    /// </summary>
    public class SampleEmulator : Emulator
    {
        private SampleEmulatorForm _form;

        /// <summary>
        /// 
        /// </summary>
        public override void SetupComponent()
        {
            RegisterComponent(new TouchGpioPort(TouchGpioPort.DefaultTouchPin));
            base.SetupComponent();
        }

        /// <summary>
        /// Called by the emulator after all components have been set up and 
        /// registered.
        /// </summary>
        public override void InitializeComponent()
        {
            base.InitializeComponent();

            _form = new SampleEmulatorForm(this.Emulator);

            _form.OnInitializeComponent();

            // Launch the UI thread.
            Thread uiThread = new Thread(RunForm);
            uiThread.SetApartmentState(ApartmentState.STA);
            uiThread.Start();
        }

        /// <summary>
        /// Destroys the component.  Called by the emulator after the program 
        /// exits.
        /// </summary>
        public override void UninitializeComponent()
        {
            base.UninitializeComponent();

            // When the emulator is shutting down, exit the WinForm application 
            // as well.
            Application.Exit();
        }

        /// <summary>
        /// Call the .NET method that will drive the emulator's UI.
        /// </summary>        
        private void RunForm()
        {
            Application.Run(_form);

            // When the form exits, shut down the emulator.
            this.Emulator.Stop();
        }

        /// <summary>
        /// Execution entry point.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            (new SampleEmulator()).Start();
        }

    }
}
