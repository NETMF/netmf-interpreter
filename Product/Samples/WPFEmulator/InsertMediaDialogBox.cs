////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Microsoft.SPOT.Emulator.Sample
{
    /// <summary>
    /// Dialog box for media insert options.
    /// </summary>
    public partial class InsertMediaDialogBox : Form
    {
        // Declare private data members.
        private bool _createNewMedia = true;
        private string _filePath = null;
        private uint _bytesPerSector = 0;
        private uint _sectorsPerBlock = 0;
        private uint _numBlocks = 0;
        private uint _serialNumber = 0;

        /// <summary>
        /// The default constructor.
        /// </summary>
        public InsertMediaDialogBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenExistingRadio_CheckedChanged(object sender,
            EventArgs e)
        {
            if (openFileDialogBox.ShowDialog() == DialogResult.OK)
            {
                _createNewMedia = false;
                _filePath = openFileDialogBox.FileName;
                DialogResult = DialogResult.OK;
            }
            else
            {
                DialogResult = DialogResult.Cancel;
            }

            Close();
        }

        /// <summary>
        /// Gets whether to create new media.
        /// </summary>
        public bool CreateNewMedia
        {
            get
            {
                return _createNewMedia;
            }
        }

        /// <summary>
        /// Gets the number of bytes per sector.
        /// </summary>
        public uint BytesPerSector
        {
            get
            {
                return _bytesPerSector;
            }
        }

        /// <summary>
        /// Gets the number of sectors per block.
        /// </summary>
        public uint SectorsPerBlock
        {
            get
            {
                return _sectorsPerBlock;
            }
        }

        /// <summary>
        /// Gets the number of blocks.
        /// </summary>
        public uint NumBlocks
        {
            get
            {
                return _numBlocks;
            }
        }

        /// <summary>
        /// Gets the serial number.
        /// </summary>
        public uint SerialNumber
        {
            get
            {
                return _serialNumber;
            }
        }

        /// <summary>
        /// Gets the filepath.
        /// </summary>
        public string FilePath
        {
            get
            {
                return _filePath;
            }
        }

        /// <summary>
        /// Handles the Browse button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseButton_Click(object sender, EventArgs e)
        {
            if (saveFileDialogBox.ShowDialog() == DialogResult.OK)
            {
                FilePathTextBox.Text = saveFileDialogBox.FileName;
            }
        }

        /// <summary>
        /// Handles the OK button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, EventArgs e)
        {
            _filePath = FilePathTextBox.Text;
            _bytesPerSector = Convert.ToUInt32(BytesPerSectorComboBox.Text);
            _sectorsPerBlock = Convert.ToUInt32(SectorsPerBlockTextBox.Text);
            _numBlocks = Convert.ToUInt32(NumberOfBlocksTextBox.Text);
            _serialNumber = Convert.ToUInt32(SerialNumberTextBox.Text);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InsertMediaDialogBox_Load(object sender, EventArgs e)
        {
            BytesPerSectorComboBox.SelectedIndex = 8;
            UpdateSize();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SectorsPerBlockTextBox_Validating(object sender,
            CancelEventArgs e)
        {
            e.Cancel = !IsValidNumber(SectorsPerBlockTextBox.Text);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private bool IsValidNumber(string text)
        {
            try
            {
                uint u = Convert.ToUInt32(text);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumberOfBlocksTextBox_Validating(object sender,
            CancelEventArgs e)
        {
            e.Cancel = !IsValidNumber(NumberOfBlocksTextBox.Text);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerialNumberTextBox_Validating(object sender,
            CancelEventArgs e)
        {
            e.Cancel = !IsValidNumber(SerialNumberTextBox.Text);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BytesPerSectorComboBox_SelectedIndexChanged(object sender,
            EventArgs e)
        {
        }

        /// <summary>
        /// Update total size value, use KB, MB, GB etc.
        /// </summary>
        private void UpdateSize()
        {
            try
            {
                uint bytesPerSector =
                    Convert.ToUInt32(BytesPerSectorComboBox.Text);
                uint sectorsPerBlock =
                    Convert.ToUInt32(SectorsPerBlockTextBox.Text);
                uint numBlocks = Convert.ToUInt32(NumberOfBlocksTextBox.Text);

                double size =
                    ((double)bytesPerSector * sectorsPerBlock * numBlocks);

                if (size < 1024)
                {
                    MediaSizeLabel.Text = String.Format(
                        "{0:0.} Bytes", size);
                }
                else if (size < (1024 * 1024))
                {
                    MediaSizeLabel.Text = String.Format(
                        "{0:0.00} KB", size / 1024);
                }
                else if (size < (1024 * 1024 * 1024))
                {
                    MediaSizeLabel.Text = String.Format(
                        "{0:0.00} MB", size / (1024 * 1024));
                }
                else
                {
                    MediaSizeLabel.Text = String.Format(
                        "{0:0.00} GB", size / (1024 * 1024 * 1024));
                }
            }
            catch (FormatException)
            {
                MediaSizeLabel.Text = "0";
            }
            catch (OverflowException)
            {
                MediaSizeLabel.Text = "Too large.";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SectorsPerBlockTextBox_TextChanged(object sender,
            EventArgs e)
        {
            UpdateSize();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumberOfBlocksTextBox_TextChanged(object sender,
            EventArgs e)
        {
            UpdateSize();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerialNumberTextBox_TextChanged(object sender, EventArgs e)
        {
            UpdateSize();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SectorsPerBlockTextBox_KeyDown(object sender,
            KeyEventArgs e)
        {
            ProcessNumericTextBoxKeyDown(e);
        }

        /// <summary>
        /// Don't let users press non-numeric keys. Keys such as DEL, BKSPACE, 
        /// LEFT, RIGHT are allowed.
        /// </summary>
        /// <param name="e"></param>
        private void ProcessNumericTextBoxKeyDown(KeyEventArgs e)
        {
            if (((e.KeyValue >= '0') && (e.KeyValue <= '9')) ||
                (e.KeyValue == 8) || (e.KeyValue == 46) || (e.KeyValue == 37) ||
                (e.KeyValue == 39))
            {
                e.SuppressKeyPress = false;
            }
            else
            {
                e.SuppressKeyPress = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumberOfBlocksTextBox_KeyDown(object sender,
            KeyEventArgs e)
        {
            ProcessNumericTextBoxKeyDown(e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerialNumberTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            ProcessNumericTextBoxKeyDown(e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BytesPerSectorComboBox_SelectedValueChanged(object sender,
            EventArgs e)
        {
            UpdateSize();
        }
    }
}
