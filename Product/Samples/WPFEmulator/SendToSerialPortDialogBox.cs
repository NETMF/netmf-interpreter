using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Microsoft.SPOT.Emulator.Sample
{
    public partial class SendToSerialPortDialogBox : Form
    {
        String _textToSend;

        public SendToSerialPortDialogBox()
        {
            InitializeComponent();
        }

        public String TextToSend
        {
            get { return _textToSend; }
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            _textToSend = textTextBox.Text;
        }
    }
}
