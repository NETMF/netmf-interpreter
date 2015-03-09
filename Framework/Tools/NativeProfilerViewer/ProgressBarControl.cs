using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Microsoft.NetMicroFramework.Tools.NativeProfilerViewer
{
    public partial class ProgressBarControl : UserControl
    {
        public bool needToCancel = false;
        public ProgressBarControl()
        {
            InitializeComponent();
        }
    }
}
