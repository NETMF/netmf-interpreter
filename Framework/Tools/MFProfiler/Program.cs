using System;
using System.Collections;
using System.Windows.Forms;

using _DBG = Microsoft.SPOT.Debugger;
using _PRF = Microsoft.SPOT.Profiler;
using _WP = Microsoft.SPOT.Debugger.WireProtocol;

using System.Text;
using System.Diagnostics;
using System.Threading;

//[assembly: System.Security.Permissions.SecurityPermissionAttribute(System.Security.Permissions.SecurityAction.RequestMinimum, Unrestricted = true)]

namespace Microsoft.NetMicroFramework.Tools.MFProfilerTool
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ProfilerLauncherForm());
        }
    }
}