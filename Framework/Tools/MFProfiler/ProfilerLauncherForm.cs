using CLRProfiler;
using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

using _DBG = Microsoft.SPOT.Debugger;
using _PRF = Microsoft.SPOT.Profiler;
using _WP  = Microsoft.SPOT.Debugger.WireProtocol;

namespace Microsoft.NetMicroFramework.Tools.MFProfilerTool {
    public partial class ProfilerLauncherForm : Form
    {
        internal string c_Connect = "Connect";
        internal string c_Disconnect = "Disconnect";
        internal string c_Launch = "Launch...";
        internal string c_Cancel = "Cancel";
        internal string processFileName = ".NET Micro Framework";

        private enum ProfilingState
        {
            Disconnected,
            Connecting,
            Connected,
        }

        private bool m_closing = false;
        private ProfilingState m_state = ProfilingState.Disconnected;

        private bool m_killEmulator = false;
        private Process m_emuProcess = null;
        private bool m_emuLaunched = false;
        private _DBG.Engine m_engine = null;

        private _PRF.ProfilerSession m_session = null;
        private _PRF.Exporter m_exporter = null;

        private CLRProfiler.MainForm clrProfiler = new CLRProfiler.MainForm();

        private class BackgroundConnectorArguments
        {
            public string outputFileName;
            public _DBG.PortDefinition connectPort;
            public bool reboot;
            public enum ExporterType
            {
                CLRProfiler,
                OffProf,
            }
            public ExporterType exporter;
        }

        public ProfilerLauncherForm()
        {
            InitializeComponent();
#if !DEBUG
            this.radioCLRProfiler.Visible = false;
            this.radioOffProf.Visible = false;
#endif
        }

        #region Thread-safe Textbox Logging Code
        private delegate void LogCallback(string text);
        public void LogText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.textLog.InvokeRequired)
            {
                LogCallback d = new LogCallback(LogText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                textLog.Text += text + "\r\n";
                textLog.SelectionStart = textLog.TextLength;
                textLog.ScrollToCaret();
            }
        }
        #endregion

        private void ProfilerLauncherForm_Load(object sender, EventArgs e)
        {
            EnableDisableViewMenuItems();
            comboBoxDevice.Filter = comboBoxTransport.Filter; //Use whatever filter we have selected.

            // to make TcpIp discovery class similar to Emulator/Tcp so it runs in the background?

            // to get transport / device from application settings?

            // to select most recently inserted device/emulator on updates if not currently profiling? -- Needs rewritten emulator discovery code.
        }

        private void ProfilerLauncherForm_FormClosing(System.Object sender, FormClosingEventArgs e)
        {
            m_closing = true;
            Disconnect();

            Properties.Settings.Default.Save();

            Application.Exit();
        }

        private void comboBoxTransport_SelectedValueChanged(System.Object sender, System.EventArgs e)
        {
            comboBoxDevice.Filter = comboBoxTransport.Filter;
        }

        private void SetButtonText()
        {
            if (bwConnecter.IsBusy || m_state == ProfilingState.Connecting)
            {
                buttonConnect.Text = c_Cancel;
                checkReboot.Enabled = false;
            }
            else if (m_state == ProfilingState.Connected)
            {
                buttonConnect.Text = c_Disconnect;
            }
            else
            {
                _DBG.PortDefinition port = comboBoxDevice.PortDefinition;
                _DBG.PortDefinition_Emulator emuPort = port as _DBG.PortDefinition_Emulator;
                if (emuPort != null && emuPort.Pid == 0)
                {
                    buttonConnect.Text = c_Launch;
                    checkReboot.Enabled = false;
                }
                else
                {
                    buttonConnect.Text = c_Connect;
                    checkReboot.Enabled = true;
                }
            }

        }

        private void comboBoxDevice_SelectedValueChanged(System.Object sender, System.EventArgs e)
        {
            SetButtonText();
        }

        #region Connection/Disconnection code
        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (bwConnecter.IsBusy && !bwConnecter.CancellationPending)
            {
                buttonConnect.Enabled = false;
                bwConnecter.CancelAsync();
            }
            else if (m_state == ProfilingState.Connected)
            {
                UserDisconnect();
            }
            else
            {
                Connect();
            }
        }

        private void SetInvalidDevice()
        {
            errorProvider.SetError(groupBoxDevice, "Invalid Device");
            comboBoxDevice.SelectAll();
            comboBoxDevice.Focus();
        }

        private void Connect()
        {
            _DBG.PortDefinition port = comboBoxDevice.PortDefinition;
            if (port == null)
            {
                SetInvalidDevice();
                return;
            }

            try
            {
                comboBoxTransport.Enabled = false;
                comboBoxDevice.Enabled = false;
                buttonConnect.Enabled = false;
                groupBoxOutput.Enabled = false;
                groupBoxOptions.Enabled = false;
                m_state = ProfilingState.Connecting;

                if (port is _DBG.PortDefinition_Emulator)
                {
                    _DBG.PortDefinition_Emulator emuport = port as _DBG.PortDefinition_Emulator;
                    if (emuport.Pid == 0)
                    {
                        OpenFileDialog fd = new OpenFileDialog();
                        fd.DefaultExt = "exe";
                        fd.Filter = ".NET MF Exe files (*.exe)|*.exe";
                        fd.Title = "Choose an application to emulate";
                        if (fd.ShowDialog(this) == DialogResult.OK)
                        {
                            m_emuProcess = EmulatorLauncher.LaunchEmulator(emuport, true, fd.FileName);
                            if (m_emuProcess == null)
                            {
                                LogText("Could not launch emulator.");
                                Disconnect();
                                return;
                            }
                            else
                            {
                                m_emuLaunched = true;
                                LogText(string.Format("Started emulator process {0}", m_emuProcess.Id));

                                _DBG.PlatformInfo pi = new _DBG.PlatformInfo(null);
                                port = new _DBG.PortDefinition_Emulator("Emulator - pid " + m_emuProcess.Id, m_emuProcess.Id);
                                comboBoxDevice.SelectLaunchedEmulator((_DBG.PortDefinition_Emulator)port);
                            }
                        }
                        else
                        {
                            Disconnect();
                            return;
                        }
                    }
                    else
                    {
                        try
                        {
                            m_emuProcess = Process.GetProcessById(emuport.Pid);
                        }
                        catch
                        {
                            m_state = ProfilingState.Disconnected;
                            EnableUI();
                            SetInvalidDevice();
                            return;
                        }
                    }
                }

                buttonConnect.Text = c_Cancel;
                buttonConnect.Enabled = true;
                BackgroundConnectorArguments bca = new BackgroundConnectorArguments();
                bca.connectPort = port;
                bca.outputFileName = textLogFile.Text;
#if DEBUG
            if (radioOffProf.Checked)
            {
                bca.exporter = BackgroundConnectorArguments.ExporterType.OffProf;
            }
            else
            {
                bca.exporter = BackgroundConnectorArguments.ExporterType.CLRProfiler;
            }
#else
                bca.exporter = BackgroundConnectorArguments.ExporterType.CLRProfiler;
#endif

                //Rebooting the emulator just makes it quit.
                bca.reboot = checkReboot.Checked && !(port is _DBG.PortDefinition_Emulator);

                bwConnecter.RunWorkerAsync(bca);
            }
            catch (ApplicationException e)
            {
                comboBoxTransport.Enabled = true;
                comboBoxDevice.Enabled = true;
                buttonConnect.Enabled = true;
                groupBoxOutput.Enabled = true;
                groupBoxOptions.Enabled = true;
                m_state = ProfilingState.Disconnected;

                MessageBox.Show(this, e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConnectComplete()
        {
            m_state = ProfilingState.Connected;
            buttonConnect.Text = c_Disconnect;
            checkAllocations.Checked = checkAllocations.Checked && m_engine.Capabilities.ProfilingAllocations;
            checkCalls.Checked = checkCalls.Checked && m_engine.Capabilities.ProfilingCalls;
        }


        private void UserDisconnect()
        {
            if (m_emuProcess != null && m_emuLaunched && !m_emuProcess.HasExited)
            {
                m_killEmulator = (MessageBox.Show(string.Format("Emulator process {0} is still running. Do you wish to have it terminated?", m_emuProcess.Id),
                    "Kill emulator?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes);
            }
            SoftDisconnect();
        }

        private void SoftDisconnect()
        {
            /* A 'soft disconnect' is where we don't want to record any more events, but we do want to
             * finish decoding as much of the data as possible before pulling the plug to the device. */
            if (m_session != null)
            {
                buttonConnect.Enabled = false;
                m_session.OnDisconnect += SoftDisconnectDone;
                m_session.Disconnect();
            }
        }

        private void SoftDisconnectDone(object sender, EventArgs args)
        {
#if DEBUG
            LogText(String.Concat("Profiling Session Length: ", (m_session != null) ? m_session.BitsReceived : 0, " bits."));
#endif
            this.Invoke((MethodInvoker)Disconnect);
        }

        private void Disconnect()
        {
            if (m_state == ProfilingState.Disconnected) return;

            if (m_state == ProfilingState.Connected)
            {
                if (m_session != null)
                {
                    m_session.Disconnect();
                }

                if (m_exporter != null)
                {
                    m_exporter.Close();
                }
            }

            try
            {
                lock (m_engine)
                {
                    m_engine.Stop();
                }
            }
            catch { }  //Depending on when we get called, stopping the engine throws anything from NullReferenceException, ArgumentNullException, IOException, etc.

            m_engine = null;

            if (m_state == ProfilingState.Connected)
            {
                LogText("Disconnected from TinyCLR.");
            }

            KillEmulator();

            ProfilingState oldstate = m_state;
            m_state = ProfilingState.Disconnected;

            EnableUI();

            if (!m_closing && oldstate == ProfilingState.Connected && m_exporter is _PRF.Exporter_CLRProfiler)
            {
                clrProfiler.LoadLogFile(m_exporter.FileName);
                EnableDisableViewMenuItems();
            }
        }

        private void EnableUI()
        {
            comboBoxTransport.Enabled = true;
            comboBoxDevice.UpdateList();
            comboBoxDevice.Enabled = true;
            SetButtonText();
            buttonConnect.Enabled = true;
            groupBoxOutput.Enabled = true;
            groupBoxOptions.Enabled = true;
        }

        private void KillEmulator()
        {
            if (m_killEmulator)
            {
                LogText(string.Format("Attemping to kill process {0} - ", m_emuProcess.Id));
                m_emuProcess.Kill();
                m_killEmulator = false;
                LogText("done.");
            }
            m_emuProcess = null;
            m_emuLaunched = false;
        }

        public void OnWPCommand(_WP.IncomingMessage msg, bool fReply)
        {
            switch (msg.Header.m_cmd)
            {
                case _WP.Commands.c_Monitor_ProgramExit:
                    this.Invoke((MethodInvoker)SoftDisconnect);
                    break;
            }
        }

        public void OnWPMessage(_WP.IncomingMessage msg, string text)
        {
            char[] NEWLINE_CHARS = { '\r', '\n' };
            text = text.TrimEnd(NEWLINE_CHARS);
            if (String.IsNullOrEmpty(text))
            {
                LogText("");
            }
            else
            {
                LogText(String.Concat("Received message from device: ", text));
            }
        }

        private void bwConnecter_DoWork(System.Object sender, DoWorkEventArgs e)
        {
            BackgroundConnectorArguments bca = (BackgroundConnectorArguments)e.Argument;
            _DBG.PortDefinition port = bca.connectPort;

            Debug.Assert(m_engine == null);

            e.Result = false;

#if USE_CONNECTION_MANAGER
            m_engine = m_port.DebugPortSupplier.Manager.Connect(port);
#else
            m_engine = new _DBG.Engine(port);
#endif

            m_killEmulator = false;

            lock (m_engine)
            {
                m_engine.StopDebuggerOnConnect = true;
                m_engine.OnCommand += new _DBG.CommandEventHandler(OnWPCommand);
                m_engine.OnMessage += new _DBG.MessageEventHandler(OnWPMessage);
                m_engine.Start();

                const int retries = 50;
                bool connected = false;
                for (int i = 0; connected == false && i < retries; i++)
                {
                    if (bwConnecter.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    connected = m_engine.TryToConnect(1, 100, false, _DBG.ConnectionSource.TinyCLR);
                }

                if (connected)
                {
                    if (m_engine.Capabilities.Profiling == false)
                    {
                        throw new ApplicationException("This device is not running a version of TinyCLR that supports profiling.");
                    }

                    //Move IsDeviceInInitializeState(), IsDeviceInExitedState(), GetDeviceState(),EnsureProcessIsInInitializedState() to Debugger.dll?
                    uint executionMode = 0;
                    m_engine.SetExecutionMode(0, 0, out executionMode);
                    if (bca.reboot || (executionMode & _WP.Commands.Debugging_Execution_ChangeConditions.c_State_Mask) != _WP.Commands.Debugging_Execution_ChangeConditions.c_State_Initialize)
                    {
                        m_engine.RebootDevice(_DBG.Engine.RebootOption.RebootClrWaitForDebugger);
                        m_engine.TryToConnect(10, 1000);
                        m_engine.SetExecutionMode(0, 0, out executionMode);
                        Debug.Assert((executionMode & _WP.Commands.Debugging_Execution_ChangeConditions.c_State_Mask) == _WP.Commands.Debugging_Execution_ChangeConditions.c_State_Initialize);
                    }

                    m_engine.ThrowOnCommunicationFailure = true;
                    m_session = new _PRF.ProfilerSession(m_engine);
                    if (m_exporter != null)
                    {
                        m_exporter.Close();
                    }

                    switch (bca.exporter)
                    {
                        case BackgroundConnectorArguments.ExporterType.CLRProfiler:
                            m_exporter = new _PRF.Exporter_CLRProfiler(m_session, bca.outputFileName);
                            break;
#if DEBUG
                        case BackgroundConnectorArguments.ExporterType.OffProf:
                            m_exporter = new _PRF.Exporter_OffProf(m_session, bca.outputFileName);
                            break;
#endif
                        default:
                            throw new ArgumentException("Unsupported export format");
                    }
                    m_session.EnableProfiling();
                    e.Result = true;
                }
            }
            return;
        }

        private void bwConnecter_RunWorkerCompleted(System.Object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Exception ex = (Exception)e.Error;
                LogText(string.Format("Error connecting to device:\r\n{0}", ex.Message));
                Disconnect();
            }
            else if (e.Cancelled)
            {
                LogText("Connection canceled.");
                Disconnect();
                return;
            }
            else
            {
                bool result = false;
                if (e.Result != null) { result = (bool)e.Result; }
                if (result)
                {
                    m_session.SetProfilingOptions(checkCalls.Checked, checkAllocations.Checked);
                    m_engine.ResumeExecution();
                    LogText("Successfully connected to TinyCLR.");
                    LogText("Using file: " + m_exporter.FileName);
                    ConnectComplete();
                }
                else
                {
                    LogText("Device is unresponsive or cannot be found.");
                    Disconnect();
                }
            }
        }

        #endregion

        private void buttonLogFileBrowse_Click(System.Object sender, System.EventArgs e)
        {
            SaveFileDialog fd = new SaveFileDialog();

            if (radioCLRProfiler.Checked == true)
            {
                fd.DefaultExt = "log";
                fd.Filter = "CLRProfiler log files (*.log)|*.log";
            }
            else
            {
                fd.DefaultExt = "opf";
                fd.Filter = "OffView Files (*.opf)|*.opf";
            }

            fd.FileName = textLogFile.Text;
            if (fd.ShowDialog(this) == DialogResult.OK)
            {
                textLogFile.Text = fd.FileName;
                errorProvider.SetError(textLogFile, "");
            }
        }

        #region UI Validation code
        private void groupBoxOutput_Validating(System.Object sender, CancelEventArgs e)
        {
            errorProvider.SetIconAlignment(textLogFile, ErrorIconAlignment.MiddleLeft);
            errorProvider.SetIconPadding(textLogFile, 4);

            try
            {
                //There doesn't seem to be a good way to tell if a user has permissions to write to a file
                if (textLogFile.Text == "") { errorProvider.SetError(textLogFile, ""); }
                else if (Directory.Exists(textLogFile.Text))
                {
                    errorProvider.SetError(textLogFile, "Path exists as a directory.");
                    textLogFile.SelectAll();
                    textLogFile.Focus();
                    e.Cancel = true;
                }
                else if (!Directory.Exists(Path.GetDirectoryName(textLogFile.Text)))
                {
                    errorProvider.SetError(textLogFile, "Directory does not exist.");
                    textLogFile.SelectAll();
                    textLogFile.Focus();
                    e.Cancel = true;
                }
                else { errorProvider.SetError(textLogFile, ""); }
            }
            catch
            {
                e.Cancel = true;
                textLogFile.SelectAll();
                textLogFile.Focus();
                errorProvider.SetError(textLogFile, "Invalid path.");
                e.Cancel = true;
            }
        }

        private void groupBoxDevice_Validating(System.Object sender, CancelEventArgs e)
        {
            _DBG.PortDefinition port = comboBoxDevice.PortDefinition;
            if (port == null)
            {
                e.Cancel = true;
                errorProvider.SetError(groupBoxDevice, "Invalid device");
                comboBoxDevice.SelectAll();
                comboBoxDevice.Focus();
            }
            else
            {
                errorProvider.SetError(groupBoxDevice, "");
            }
        }
        #endregion

        private void SetProfilingOptions(System.Object sender, System.EventArgs e)
        {
            if (m_state == ProfilingState.Connected)
            {
                m_session.SetProfilingOptions(checkCalls.Checked, checkAllocations.Enabled);
            }
        }

        private void exitToolStripMenuItem_Click(System.Object sender, System.EventArgs e)
        {
            Application.Exit();
        }

        private void openToolStripMenuItem_Click(System.Object sender, System.EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.DefaultExt = "log";
            fd.Filter = "CLRProfiler log files (*.log)|*.log";
            fd.FileName = textLogFile.Text;
            if (fd.ShowDialog(this) == DialogResult.OK)
            {
                clrProfiler.LoadLogFile(fd.FileName);
                EnableDisableViewMenuItems();
            }
        }

        #region Menu Items taken from CLR Profiler UI
        private void viewByAddressMenuItem_Click(object sender, System.EventArgs e)
        {
            ViewByAddressForm viewByAddressForm = new ViewByAddressForm();
            viewByAddressForm.Visible = true;
        }

        private void viewTimeLineMenuItem_Click(object sender, System.EventArgs e)
        {
            TimeLineViewForm timeLineViewForm = new TimeLineViewForm();
            timeLineViewForm.Visible = true;
        }

        private void viewHistogram_Click(object sender, System.EventArgs e)
        {
            HistogramViewForm histogramViewForm = new HistogramViewForm();
            histogramViewForm.Visible = true;
        }

        private void viewHistogramRelocatedMenuItem_Click(object sender, System.EventArgs e)
        {
            if (clrProfiler.lastLogResult != null)
            {
                string title = "Histogram by Size for Relocated Objects";
                HistogramViewForm histogramViewForm = new HistogramViewForm(clrProfiler.lastLogResult.relocatedHistogram, title);
                histogramViewForm.Show();
            }
        }

        private void viewHistogramFinalizerMenuItem_Click(object sender, System.EventArgs e)
        {
            if (clrProfiler.lastLogResult != null)
            {
                string title = "Histogram by Size for Finalized Objects";
                HistogramViewForm histogramViewForm = new HistogramViewForm(clrProfiler.lastLogResult.finalizerHistogram, title);
                histogramViewForm.Show();
            }
        }

        private void viewHistogramCriticalFinalizerMenuItem_Click(object sender, System.EventArgs e)
        {
            if (clrProfiler.lastLogResult != null)
            {
                string title = "Histogram by Size for Critical Finalized Objects";
                HistogramViewForm histogramViewForm = new HistogramViewForm(clrProfiler.lastLogResult.criticalFinalizerHistogram, title);
                histogramViewForm.Show();
            }
        }

        private void viewAgeHistogram_Click(object sender, System.EventArgs e)
        {
            if (clrProfiler.lastLogResult != null)
            {
                string title = "Histogram by Age for Live Objects";
                AgeHistogram ageHistogram = new AgeHistogram(clrProfiler.lastLogResult.liveObjectTable, title);
                ageHistogram.Show();
            }
        }

        private void viewAllocationGraphmenuItem_Click(object sender, System.EventArgs e)
        {
            if (clrProfiler.lastLogResult != null)
                clrProfiler.ViewGraph(clrProfiler.lastLogResult, processFileName, Graph.GraphType.AllocationGraph);
        }

        private void viewAssemblyGraphmenuItem_Click(object sender, System.EventArgs e)
        {
            if (clrProfiler.lastLogResult != null)
                clrProfiler.ViewGraph(clrProfiler.lastLogResult, processFileName, Graph.GraphType.AssemblyGraph);
        }

        private void viewHeapGraphMenuItem_Click(object sender, System.EventArgs e)
        {
            if (clrProfiler.lastLogResult != null)
                clrProfiler.ViewGraph(clrProfiler.lastLogResult, processFileName, Graph.GraphType.HeapGraph);
        }

        private void viewCallGraphMenuItem_Click(object sender, System.EventArgs e)
        {
            if (clrProfiler.lastLogResult != null)
                clrProfiler.ViewGraph(clrProfiler.lastLogResult, processFileName, Graph.GraphType.CallGraph);
        }

        private void viewFunctionGraphMenuItem_Click(object sender, System.EventArgs e)
        {
            if (clrProfiler.lastLogResult != null)
                clrProfiler.ViewGraph(clrProfiler.lastLogResult, processFileName, Graph.GraphType.FunctionGraph);
        }

        private void viewModuleGraphMenuItem_Click(object sender, System.EventArgs e)
        {
            if (clrProfiler.lastLogResult != null)
                clrProfiler.ViewGraph(clrProfiler.lastLogResult, processFileName, Graph.GraphType.ModuleGraph);
        }

        private void viewClassGraphMenuItem_Click(object sender, System.EventArgs e)
        {
            if (clrProfiler.lastLogResult != null)
                clrProfiler.ViewGraph(clrProfiler.lastLogResult, processFileName, Graph.GraphType.ClassGraph);
        }

        private void viewCommentsMenuItem_Click(object sender, System.EventArgs e)
        {
            if (clrProfiler.log != null && clrProfiler.log.commentEventList.count != 0)
            {
                ViewCommentsForm viewCommentsForm = new ViewCommentsForm(clrProfiler.log);
                viewCommentsForm.Visible = true;
            }
        }

        private void viewCallTreeMenuItem_Click(object sender, System.EventArgs e)
        {
            if (clrProfiler.lastLogResult != null && clrProfiler.lastLogResult.hadCallInfo)
            {
                CallTreeForm callTreeForm = new CallTreeForm(clrProfiler.log.fileName, clrProfiler.lastLogResult);
            }
        }

        private void viewSummaryMenuItem_Click(object sender, System.EventArgs e)
        {
            if (clrProfiler.lastLogResult != null)
            {
                string scenario = clrProfiler.log.fileName;
                SummaryForm summaryForm = new SummaryForm(clrProfiler.log, clrProfiler.lastLogResult, scenario);
                summaryForm.Show();
            }
        }

        private void EnableDisableViewMenuItems()
        {
            if (clrProfiler.lastLogResult == null)
            {
                viewAllocationGraphMenuItem.Enabled = false;
                viewCallGraphMenuItem.Enabled = false;
                viewHeapGraphMenuItem.Enabled = false;
                viewHistogramAllocatedMenuItem.Enabled = false;
                viewHistogramRelocatedMenuItem.Enabled = false;
                viewHistogramFinalizerMenuItem.Enabled = false;
                viewHistogramCriticalFinalizerMenuItem.Enabled = false;
                viewHistogramByAgeMenuItem.Enabled = false;
                viewObjectsByAddressMenuItem.Enabled = false;
                viewTimeLineMenuItem.Enabled = false;
                viewFunctionGraphMenuItem.Enabled = false;
                viewModuleGraphMenuItem.Enabled = false;
                viewClassGraphMenuItem.Enabled = false;
                viewCallTreeMenuItem.Enabled = false;
                viewAssemblyGraphMenuItem.Enabled = false;
                viewSummaryMenuItem.Enabled = false;
            }
            else
            {
                viewAllocationGraphMenuItem.Enabled = !clrProfiler.lastLogResult.allocatedHistogram.Empty;
                viewCallGraphMenuItem.Enabled = !clrProfiler.lastLogResult.callstackHistogram.Empty;
                viewAssemblyGraphMenuItem.Enabled = !clrProfiler.lastLogResult.callstackHistogram.Empty;
                viewHeapGraphMenuItem.Enabled = !clrProfiler.lastLogResult.objectGraph.empty;
                viewHistogramAllocatedMenuItem.Enabled = !clrProfiler.lastLogResult.allocatedHistogram.Empty;
                viewHistogramRelocatedMenuItem.Enabled = !clrProfiler.lastLogResult.relocatedHistogram.Empty;
                viewHistogramFinalizerMenuItem.Enabled = !clrProfiler.lastLogResult.finalizerHistogram.Empty;
                viewHistogramCriticalFinalizerMenuItem.Enabled = !clrProfiler.lastLogResult.criticalFinalizerHistogram.Empty;
                viewHistogramByAgeMenuItem.Enabled = clrProfiler.lastLogResult.liveObjectTable != null;
                viewObjectsByAddressMenuItem.Enabled = clrProfiler.lastLogResult.liveObjectTable != null;
                viewTimeLineMenuItem.Enabled = clrProfiler.lastLogResult.sampleObjectTable != null;
                viewFunctionGraphMenuItem.Enabled = !clrProfiler.lastLogResult.functionList.Empty;
                viewModuleGraphMenuItem.Enabled = !clrProfiler.lastLogResult.functionList.Empty;
                viewClassGraphMenuItem.Enabled = !clrProfiler.lastLogResult.functionList.Empty;
                viewCallTreeMenuItem.Enabled = clrProfiler.lastLogResult.hadCallInfo;
                viewSummaryMenuItem.Enabled = true;
            }
            viewCommentsMenuItem.Enabled = (clrProfiler.log != null && clrProfiler.log.commentEventList.count != 0);
        }
        #endregion

        private void btnClear_Click(object sender, EventArgs e)
        {
            textLog.Text = String.Empty;
        }
    }
}
