////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Microsoft.SPOT.Emulator.Sample
{
    partial class TestEmulatorForm : Form
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TestEmulatorForm));
            this.buttonCollection = new Microsoft.SPOT.Emulator.Sample.ButtonCollection();
            this.buttonDown = new Microsoft.SPOT.Emulator.Sample.Button();
            this.buttonSelect = new Microsoft.SPOT.Emulator.Sample.Button();
            this.buttonRight = new Microsoft.SPOT.Emulator.Sample.Button();
            this.buttonUp = new Microsoft.SPOT.Emulator.Sample.Button();
            this.buttonLeft = new Microsoft.SPOT.Emulator.Sample.Button();
            this.lcdDisplay = new Microsoft.SPOT.Emulator.Sample.LcdControl();
            this.buttonCollection.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCollection
            // 
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
            this.lcdDisplay.BackColor = System.Drawing.Color.White;
            this.lcdDisplay.LcdDisplay = null;
            this.lcdDisplay.Location = new System.Drawing.Point(35, 71);
            this.lcdDisplay.Name = "lcdDisplay";
            this.lcdDisplay.Size = new System.Drawing.Size(320, 240);
            this.lcdDisplay.TabIndex = 0;
            this.lcdDisplay.Text = "lcdControl1";
            this.lcdDisplay.TouchPort = null;
            // 
            // TestEmulatorForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.BackgroundImage = global::Microsoft.SPOT.Emulator.Sample.Properties.Resources.EmulatorSkin;
            this.ClientSize = new System.Drawing.Size(389, 616);
            this.Controls.Add(this.buttonCollection);
            this.Controls.Add(this.lcdDisplay);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "TestEmulatorForm";
            this.Text = ".NET Micro Framework Test Emulator";
            this.buttonCollection.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.SPOT.Emulator.Sample.LcdControl lcdDisplay;
        private Microsoft.SPOT.Emulator.Sample.Button buttonLeft;
        private Microsoft.SPOT.Emulator.Sample.Button buttonUp;
        private Microsoft.SPOT.Emulator.Sample.Button buttonRight;
        private Microsoft.SPOT.Emulator.Sample.Button buttonDown;
        private Microsoft.SPOT.Emulator.Sample.Button buttonSelect;
        private Microsoft.SPOT.Emulator.Sample.ButtonCollection buttonCollection;        
    }
}