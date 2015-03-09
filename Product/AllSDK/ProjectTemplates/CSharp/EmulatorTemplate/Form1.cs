using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.SPOT.Emulator;

namespace $safeprojectname$
{
    public partial class Form1 : Form
    {
        private Emulator _emulator;

        public Form1( Emulator emulator )
        {
            _emulator = emulator;

            InitializeComponent( );
        }
    }
}
