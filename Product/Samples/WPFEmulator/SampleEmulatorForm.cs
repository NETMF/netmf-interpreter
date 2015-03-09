////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Microsoft.SPOT.Emulator.BlockStorage;
using Microsoft.SPOT.Emulator.TouchPanel;
using Microsoft.SPOT.Emulator.Com;
using System.Text;

namespace Microsoft.SPOT.Emulator.Sample
{
    /// <summary>
    /// The UI form for the emulator.
    /// </summary>
    public partial class SampleEmulatorForm : Form
    {
        Emulator _emulator;

        /// <summary>
        /// The default constructor.
        /// </summary>
        /// <param name="emulator"></param>
        public SampleEmulatorForm(Emulator emulator)
        {
            _emulator = emulator;

            // Initialize the component on the UI form.
            InitializeComponent();
        }

        /// <summary>
        /// Handles loading the form.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            // Give the buttonCollection focus to allow it to handle keyboard 
            // events.
            this.buttonCollection.Select();

            base.OnLoad(e);

            // Set up the Insert/Eject, if there are any removable block storage 
            // devices.
            List<EmulatorRemovableBlockStorageDevice> bsdList =
                new List<EmulatorRemovableBlockStorageDevice>();
            removableBSDs =
                new Dictionary<string, EmulatorRemovableBlockStorageDevice>();

            foreach (BlockStorageDevice bsd in _emulator.BlockStorageDevices)
            {
                if (bsd is EmulatorRemovableBlockStorageDevice)
                {
                    bsdList.Add((EmulatorRemovableBlockStorageDevice)bsd);
                }
            }

            if (bsdList.Count > 0)
            {
                foreach (EmulatorRemovableBlockStorageDevice removableBSD in bsdList)
                {
                    ToolStripItem item = new ToolStripMenuItem(
                        GetItemText(removableBSD), null, InsertEjectOnClick);

                    item.Name = removableBSD.Namespace;

                    insertEjectMenuItem.DropDownItems.Add(item);

                    removableBSDs.Add(removableBSD.Namespace, removableBSD);
                }
            }

            // Set up Emulator Serial Port menu items, if there are any EmulatorSerialPorts
            List<EmulatorSerialPort> espList = new List<EmulatorSerialPort>();
            emulatorSPs = new Dictionary<String, EmulatorSerialPort>();

            foreach (ComPort serialPort in _emulator.SerialPorts)
            {
                if (serialPort is EmulatorSerialPort)
                {
                    espList.Add(serialPort as EmulatorSerialPort);
                }
            }

            if (espList.Count > 0)
            {
                // Create a menu item for each EmulatorSerialPort
                foreach (EmulatorSerialPort serialPort in espList)
                {
                    String itemText = "Write to " + serialPort.ComPortHandle + " (" + serialPort.ComponentId + ")";
                    ToolStripItem item = new ToolStripMenuItem(
                        itemText, null, WriteToEmulatorSerialPortOnClick);

                    item.Name = serialPort.ComPortHandle.ToString();

                    espToolStripMenuItem.DropDownItems.Add(item);

                    emulatorSPs.Add(item.Name, serialPort);
                }
            }

            if (insertEjectMenuItem.DropDownItems.Count > 0)
            {
                menuStrip.Visible = true;
                insertEjectMenuItem.Visible = true;
            }

            if (espToolStripMenuItem.DropDownItems.Count > 0)
            {
                menuStrip.Visible = true;
                espToolStripMenuItem.Visible = true;
            }
        }

        /// <summary>
        /// Helper method to initialize buttons.
        /// </summary>
        /// <param name="button">The button to be initialized.</param>
        /// <param name="componentId">The string identifying the button, found 
        /// in the config file.</param>
        /// <param name="key">The key that this button should respond to.
        /// </param>
        private void InitializeButton(Button button, string componentId, 
            Keys key)
        {
            button.Port = 
                _emulator.FindComponentById(componentId) as Gpio.GpioPort;
            button.Key = key;
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnInitializeComponent()
        {
            // Initialize the LCD control with the LCD emulator component.
            this.lcdDisplay.LcdDisplay = _emulator.LcdDisplay;
            if (_emulator.LcdDisplay.Width > 320)
            {
                int border = lcdDisplay.Left;

                double borderRatio = border / (double)Width;
                double ratio = (_emulator.LcdDisplay.Width / 320.0);

                this.Width       = (int)(2 * border * ratio) + _emulator.LcdDisplay.Width;

                lcdDisplay.Left  = (int)(Width * borderRatio) - 1;
                lcdDisplay.Width = _emulator.LcdDisplay.Width;
            }
            if (_emulator.LcdDisplay.Height > 240)
            {
                int border = lcdDisplay.Top;
                int lower = this.Height - lcdDisplay.Bottom;

                double borderRatio = border / (double)Height;
                double ratio = _emulator.LcdDisplay.Height / 240.0;

                this.Height       = (int)(border * ratio) + _emulator.LcdDisplay.Height + lower;

                lcdDisplay.Top    = (int)(Height * borderRatio);
                lcdDisplay.Height = _emulator.LcdDisplay.Height;
            }
            Invalidate();

            this.BackgroundImageLayout = ImageLayout.Stretch;

            this.lcdDisplay.TouchPort = 
                (TouchGpioPort)_emulator.GpioPorts[TouchGpioPort.DefaultTouchPin];

            // Read the GPIO pins from the Emulator.config file.  This allows 
            // overriding the specific GPIO pin numbers, for example, without 
            // having to change code.
            InitializeButton(this.buttonDown, "Pin_Down", Keys.Down);
            InitializeButton(this.buttonLeft, "Pin_Left", Keys.Left);
            InitializeButton(this.buttonRight, "Pin_Right", Keys.Right);
            InitializeButton(this.buttonUp, "Pin_Up", Keys.Up);
            InitializeButton(this.buttonSelect, "Pin_Select", Keys.Enter);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InsertEjectOnClick(Object sender, EventArgs e)
        {
            ToolStripItem item = sender as ToolStripItem;
            if (item != null)
            {
                EmulatorRemovableBlockStorageDevice removableBSD = 
                    removableBSDs[item.Name];

                try
                {
                    if (removableBSD.Inserted)
                    {
                        removableBSD.Eject();
                    }
                    else
                    {
                        InsertMediaDialogBox insertMediaDialogBox = 
                            new InsertMediaDialogBox();
                        if (insertMediaDialogBox.ShowDialog() == 
                            DialogResult.OK)
                        {
                            if (insertMediaDialogBox.CreateNewMedia)
                            {
                                removableBSD.Insert(
                                    insertMediaDialogBox.FilePath, 
                                    insertMediaDialogBox.SectorsPerBlock, 
                                    insertMediaDialogBox.BytesPerSector, 
                                    insertMediaDialogBox.NumBlocks, 
                                    insertMediaDialogBox.SerialNumber);
                            }
                            else
                            {
                                removableBSD.Insert(
                                    insertMediaDialogBox.FilePath);
                            }
                        }
                    }

                    item.Text = GetItemText(removableBSD);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.ToString());
                    return;
                }
            }
        }

        /// <summary>
        /// OnClick handler for "Write To Emulator Serial Port" menu item. It launches a 
        /// SendToSerialPortDialogBox to get the string, and send the string to the emulator
        /// serial port using its StreamOut stream (from the inherited ComPortToMemoryStream).
        /// </summary>
        /// <param name="sender">the menu item that was clicked</param>
        /// <param name="e">Not used</param>
        private void WriteToEmulatorSerialPortOnClick(Object sender, EventArgs e)
        {
            ToolStripItem item = sender as ToolStripItem;
            if (item != null)
            {
                EmulatorSerialPort esp = emulatorSPs[item.Name];

                try
                {
                    SendToSerialPortDialogBox sendToSerialPortDialogBox = new SendToSerialPortDialogBox();

                    if (sendToSerialPortDialogBox.ShowDialog() == DialogResult.OK)
                    {
                        byte[] data = Encoding.UTF8.GetBytes(sendToSerialPortDialogBox.TextToSend);
                        esp.StreamOut.Write(data, 0, data.Length);
                        esp.StreamOut.Flush();
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.ToString());
                    return;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="removableBSD"></param>
        /// <returns></returns>
        private String GetItemText(
            EmulatorRemovableBlockStorageDevice removableBSD)
        {
            if (removableBSD.Inserted)
            {
                return removableBSD.Namespace + ": Eject Media";
            }
            else
            {
                return removableBSD.Namespace + ": Insert Media";
            }
        }
    }
}
