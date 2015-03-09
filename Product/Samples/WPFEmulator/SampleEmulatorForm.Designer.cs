////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Microsoft.SPOT.Emulator.Sample
{
    partial class SampleEmulatorForm : Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.insertEjectMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.espToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonCollection = new Microsoft.SPOT.Emulator.Sample.ButtonCollection();
            this.buttonDown = new Microsoft.SPOT.Emulator.Sample.Button();
            this.buttonSelect = new Microsoft.SPOT.Emulator.Sample.Button();
            this.buttonRight = new Microsoft.SPOT.Emulator.Sample.Button();
            this.buttonUp = new Microsoft.SPOT.Emulator.Sample.Button();
            this.buttonLeft = new Microsoft.SPOT.Emulator.Sample.Button();
            this.lcdDisplay = new Microsoft.SPOT.Emulator.Sample.LcdControl();
            this.menuStrip.SuspendLayout();
            this.buttonCollection.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.insertEjectMenuItem,
            this.espToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(389, 24);
            this.menuStrip.TabIndex = 3;
            this.menuStrip.Text = "menuStrip1";
            this.menuStrip.Visible = false;
            // 
            // insertEjectMenuItem
            // 
            this.insertEjectMenuItem.Name = "insertEjectMenuItem";
            this.insertEjectMenuItem.Size = new System.Drawing.Size(84, 20);
            this.insertEjectMenuItem.Text = "Insert / Eject";
            this.insertEjectMenuItem.Visible = false;
            // 
            // espToolStripMenuItem
            // 
            this.espToolStripMenuItem.Name = "espToolStripMenuItem";
            this.espToolStripMenuItem.Size = new System.Drawing.Size(128, 20);
            this.espToolStripMenuItem.Text = "Emulator Serial Ports";
            // 
            // buttonCollection
            // 
            this.buttonCollection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCollection.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.buttonCollection.Controls.Add(this.buttonDown);
            this.buttonCollection.Controls.Add(this.buttonSelect);
            this.buttonCollection.Controls.Add(this.buttonRight);
            this.buttonCollection.Controls.Add(this.buttonUp);
            this.buttonCollection.Controls.Add(this.buttonLeft);
            this.buttonCollection.Location = new System.Drawing.Point(35, 330);
            this.buttonCollection.Name = "buttonCollection";
            this.buttonCollection.Size = new System.Drawing.Size(320, 244);
            this.buttonCollection.TabIndex = 2;
            this.buttonCollection.Text = "buttonCollection1";
            // 
            // buttonDown
            // 
            this.buttonDown.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.buttonDown.Key = System.Windows.Forms.Keys.None;
            this.buttonDown.Location = new System.Drawing.Point(133, 173);
            this.buttonDown.Name = "buttonDown";
            this.buttonDown.Port = null;
            this.buttonDown.Size = new System.Drawing.Size(56, 56);
            this.buttonDown.TabIndex = 1;
            this.buttonDown.TabStop = false;
            this.buttonDown.Text = "buttonDown";
            // 
            // buttonSelect
            // 
            this.buttonSelect.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.buttonSelect.Key = System.Windows.Forms.Keys.None;
            this.buttonSelect.Location = new System.Drawing.Point(133, 96);
            this.buttonSelect.Name = "buttonSelect";
            this.buttonSelect.Port = null;
            this.buttonSelect.Size = new System.Drawing.Size(56, 56);
            this.buttonSelect.TabIndex = 1;
            this.buttonSelect.TabStop = false;
            this.buttonSelect.Text = "buttonEnter";
            // 
            // buttonRight
            // 
            this.buttonRight.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.buttonRight.Key = System.Windows.Forms.Keys.None;
            this.buttonRight.Location = new System.Drawing.Point(209, 96);
            this.buttonRight.Name = "buttonRight";
            this.buttonRight.Port = null;
            this.buttonRight.Size = new System.Drawing.Size(56, 56);
            this.buttonRight.TabIndex = 1;
            this.buttonRight.TabStop = false;
            this.buttonRight.Text = "buttonRight";
            // 
            // buttonUp
            // 
            this.buttonUp.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.buttonUp.Key = System.Windows.Forms.Keys.None;
            this.buttonUp.Location = new System.Drawing.Point(133, 19);
            this.buttonUp.Name = "buttonUp";
            this.buttonUp.Port = null;
            this.buttonUp.Size = new System.Drawing.Size(56, 56);
            this.buttonUp.TabIndex = 1;
            this.buttonUp.TabStop = false;
            this.buttonUp.Text = "buttonUp";
            // 
            // buttonLeft
            // 
            this.buttonLeft.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.buttonLeft.Key = System.Windows.Forms.Keys.None;
            this.buttonLeft.Location = new System.Drawing.Point(56, 96);
            this.buttonLeft.Name = "buttonLeft";
            this.buttonLeft.Port = null;
            this.buttonLeft.Size = new System.Drawing.Size(56, 56);
            this.buttonLeft.TabIndex = 1;
            this.buttonLeft.TabStop = false;
            this.buttonLeft.Text = "buttonLeft";
            // 
            // lcdDisplay
            // 
            this.lcdDisplay.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lcdDisplay.BackColor = System.Drawing.Color.White;
            this.lcdDisplay.LcdDisplay = null;
            this.lcdDisplay.Location = new System.Drawing.Point(35, 71);
            this.lcdDisplay.Name = "lcdDisplay";
            this.lcdDisplay.Size = new System.Drawing.Size(320, 240);
            this.lcdDisplay.TabIndex = 0;
            this.lcdDisplay.Text = "lcdControl1";
            this.lcdDisplay.TouchPort = null;
            // 
            // SampleEmulatorForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.BackgroundImage = global::Microsoft.SPOT.Emulator.Sample.Properties.Resources.EmulatorSkin;
            this.ClientSize = new System.Drawing.Size(389, 616);
            this.Controls.Add(this.buttonCollection);
            this.Controls.Add(this.lcdDisplay);
            this.Controls.Add(this.menuStrip);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MainMenuStrip = this.menuStrip;
            this.MaximizeBox = false;
            this.Name = "SampleEmulatorForm";
            this.Text = "Sample Emulator";
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.buttonCollection.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Microsoft.SPOT.Emulator.Sample.LcdControl lcdDisplay;
        private Dictionary<String, Microsoft.SPOT.Emulator.BlockStorage.EmulatorRemovableBlockStorageDevice> removableBSDs;
        private Dictionary<String, Microsoft.SPOT.Emulator.Sample.EmulatorSerialPort> emulatorSPs;
        private Microsoft.SPOT.Emulator.Sample.Button buttonLeft;
        private Microsoft.SPOT.Emulator.Sample.Button buttonUp;
        private Microsoft.SPOT.Emulator.Sample.Button buttonRight;
        private Microsoft.SPOT.Emulator.Sample.Button buttonDown;
        private Microsoft.SPOT.Emulator.Sample.Button buttonSelect;
        private Microsoft.SPOT.Emulator.Sample.ButtonCollection buttonCollection;
        private MenuStrip menuStrip;
        private ToolStripMenuItem insertEjectMenuItem;
        private ToolStripMenuItem espToolStripMenuItem;        
    }
}