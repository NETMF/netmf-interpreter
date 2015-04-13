#region Using

using System;
using System.IO;
using System.IO.Pipes;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.AccessControl;
using Microsoft.Win32;
using Microsoft.SPOT.Debugger;
using Microsoft.SPOT.Debugger.WireProtocol;
using System.Threading;
using System.Reflection;
using Microsoft.Build.BuildEngine;
using _BE = Microsoft.Build.Evaluation;
using System.Text.RegularExpressions;

#endregion

namespace Microsoft.SPOT.Platform.Test
{
    internal class Harness
    {
        #region Member Variables

        // Class variables.
        private DateTime m_initialTime;
        private XmlLog m_log;
        private string m_outputXmlFileName = null;
        private string m_emulatorText = string.Empty;
        private string m_exitText = string.Empty;
        private string m_mfLogDirectory = string.Empty;
        private string m_mfSourcesDirectory = string.Empty;
        private string m_installPath = string.Empty;
        private string m_assemblyName = string.Empty;

        private bool m_useCustomLog = false;
        private string m_transport = TransportType.Emulator.ToString();
        private string m_device = "Microsoft";
        private static string m_logFileName = string.Empty;
        private bool m_displayResults = false;
        private static DateTime m_startTime = new DateTime();
        private static DateTime m_endTime = new DateTime();
        private string m_currentSourceFileName = string.Empty;
        private static Result m_testResult = Result.NotKnown;
        private bool m_isDevEnvironment;
        private bool m_logFileSaved;
        private Microsoft.SPOT.Debugger.Engine m_engine;
        private StringBuilder m_debugText;
        private ManualResetEvent m_deviceDone;
        private static int m_logCount = 0;
        private string m_desktopAppName = string.Empty;
        private string m_xslPath = string.Empty;
        private string m_desktopXmlFile = string.Empty;
        private TestType m_currentAppType = TestType.Device;

        #endregion

        #region Enumerations

        /// <summary>
        /// An enumeration for the test results.
        /// </summary>
        internal enum Result
        {
            Pass,
            Fail,
            KnownFailure,
            Skip,
            NotKnown
        }

        /// <summary>
        /// An enumeration for the transport types.
        /// </summary>
        internal enum TransportType
        {
            USB,
            Emulator,
            Serial,
            TCPIP
        }

        internal enum TestType
        {
            Device,
            DeviceDesktop,
            Desktop
        }

        #endregion

        #region Constructor/Destructor

        public Harness(bool IsDevEnvironment)
        {
            m_deviceDone = new ManualResetEvent(false);
            m_isDevEnvironment = IsDevEnvironment;
        }

        #endregion

        #region Run

        internal HarnessExecutionResult Run(BaseTest test, XmlLog log)
        {
            m_initialTime = new DateTime();
            string file = string.Empty;
            string csprojFilePath = string.Empty;
            m_logFileSaved = false;
            m_debugText = new StringBuilder();
            Thread desktopThread = null;
            NamedPipeServerStream pipeStream = null;                     

            try
            {
                // Set the file paths.
                file = BuildFilePath(test, ref m_xslPath);
                Console.WriteLine("\nTest: " + test.Name);
                string pathForFile = file.Substring(0, file.LastIndexOf(@"\"));
                if (!pathForFile.EndsWith(@"\"))
                {
                    pathForFile = string.Format(@"{0}\", pathForFile);
                }
                string slnFileName = test.Name.Substring(test.Name.LastIndexOf(@"\") + 1);

                try
                {
                    // Get reference list and build the test
                    ArrayList referenceList = GetProjectReferences(file, ref csprojFilePath);
                    if (m_currentAppType != TestType.DeviceDesktop)
                    {
                        BuildTest(csprojFilePath, m_currentAppType);
                    }
                    else
                    {
                        pipeStream = new
                                    NamedPipeServerStream("MFHarnessPipe", PipeDirection.InOut, 1,
                                    PipeTransmissionMode.Message, PipeOptions.WriteThrough); 
                    }

                    // Set log file name.
                    SetLogFileName(test.Name);
                    test.LogFile = m_outputXmlFileName;

                    if (!(test is ProfilerTest))
                    {
                        m_log = log;
                        m_log.StartLog(m_outputXmlFileName, m_xslPath);
                    }

                    // Get build and exe path.
                    string buildPath = GetBuildPath(file);
                    test.ExeLocation = buildPath + m_assemblyName + ".exe";

                    // If this is a profiler test, run the test and return.
                    if (test is ProfilerTest)
                    {
                        return RunProfilerTest(file, test.ExeLocation, buildPath, referenceList);
                    }                    

                    string testName = m_logFileName.Replace(".xml", string.Empty);
                    StartCodeCoverage(testName);
                    m_startTime = DateTime.Now;

                    switch(m_currentAppType)
                    {
                        case TestType.Device:
                            try
                            {
                                RunDeviceTest(buildPath, test.ExeLocation, referenceList);
                            }
                            catch (Exception ex)
                            {
                                Close();
                                return HarnessResult(ex);
                            }
                            break;

                        case TestType.DeviceDesktop:
                            // Desktop apps under 
                            //              DevBox: %spoclient%\Test\Platform\Tests\Desktop\Applications\
                            //              TestBox: %programfiles%\Microsoft .NET Micro Framework\<version>\Tests\Desktop\
                            desktopThread = StartDesktopApplication(pathForFile, ref csprojFilePath);
                            pipeStream.WaitForConnection();                            
                            
                            // Get references and build the device test.
                            GetProjectReferences(file, ref csprojFilePath);
                            BuildTest(csprojFilePath, TestType.Device);                               
                            RunDeviceTest(buildPath, test.ExeLocation, referenceList);
                            break;

                        case TestType.Desktop:
                            RunDesktopTest(test.ExeLocation);
                            break;
                    }

                    m_endTime = DateTime.Now;
                    StopCodeCoverage(testName);
                }
                catch (Exception ex)
                {                    
                    m_endTime = DateTime.Now;
                    if (null != m_log)
                    {
                        m_log.WriteElementString("Test_Exception", ex.Message + ex.StackTrace);
                    }
                    Utils.WriteToEventLog("Exception: " + ex.ToString());
                    Close();
                    if (ex is ApplicationException && ex.Message.ToLower().Contains("build failure"))
                    {
                        return HarnessExecutionResult.Abort;
                    }
                    else
                    {
                        return HarnessExecutionResult.Unavailable;
                    }
                }
                finally
                {
                    if (m_currentAppType == TestType.DeviceDesktop)
                    {
                        StopDesktopApplication(pipeStream, desktopThread);
                    }

                    Close();

                    if (null != m_log)
                    {
                        switch(m_currentAppType)
                        {
                            case TestType.DeviceDesktop:
                                SaveLogFile(test.Location, TestType.DeviceDesktop);
                                break;

                            default:
                                SaveLogFile(test.Location);
                                break;                            
                        }                        
                    }

                    // Change test results back to unknown for the next test.
                    TestResults = Result.NotKnown;
                }

                if (!string.IsNullOrEmpty(file))
                {
                    if (!string.Equals(m_transport.ToLower(), "emulator"))
                    {
                        m_log.SynchronizeLogTime(file, m_initialTime);
                    }
                }
                else
                {
                    return HarnessExecutionResult.Unavailable;
                }                
            }
            catch (Exception ex)
            {
                if (ex is FileNotFoundException)
                {
                    throw ex;
                }

                Utils.WriteToEventLog(string.Format("Exception in Harness: {0}", ex.ToString()));
                return HarnessExecutionResult.Abort;
            }

            return HarnessExecutionResult.Success;
        }

        #endregion

        #region Internal Properties

        /// <summary>
        /// Property to get the result of a test run.
        /// </summary>
        internal Result MFTestResult
        {
            get
            {
                if (Result.NotKnown != m_testResult)
                {
                    return m_testResult;
                }
                else
                {
                    SetTestResult(XmlFileName);
                    return m_testResult;
                }
            }

            set
            {
                m_testResult = value;
            }
        }

        /// <summary>
        /// Property to get/set the transport for the device.
        /// </summary>
        internal string Transport
        {
            get
            {
                return m_transport;
            }

            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    m_transport = value;
                }
            }
        }

        /// <summary>
        /// Property to get/set the device.
        /// </summary>
        internal string Device
        {
            get
            {
                return m_device;
            }

            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    m_device = value;
                }
            }
        }

        /// <summary>
        ///  Property to get the test start time.
        /// </summary>
        internal DateTime StartTime
        {
            get
            {
                return m_startTime;
            }
        }

        /// <summary>
        /// Property to get the test end time.
        /// </summary>
        internal DateTime EndTime
        {
            get
            {
                return m_endTime;
            }
        }

        /// <summary>
        /// Property to specify the log file name.
        /// </summary>
        internal string LogFile
        {
            get
            {
                return m_logFileName;
            }

            set
            {
                m_logFileName = value;
                m_useCustomLog = true;
            }
        }

        /// <summary>
        /// Property to specify the value whether to display the log file at the end of the test.
        /// </summary>
        internal bool DisplayLogAfterTest
        {
            get
            {
                return m_displayResults;
            }

            set
            {
                m_displayResults = value;
            }
        }

        /// <summary>
        /// A property to return the output file name.
        /// </summary>
        internal string XmlFileName
        {
            get
            {
                return m_outputXmlFileName;
            }
        }

        #endregion

        #region Internal Methods

        internal void SetDeviceDoneEvent()
        {
            m_deviceDone.Set();
        }

        internal void Close()
        {
            DetachFromEngine();
        }

        private void DetachFromEngine()
        {
            if (m_engine != null)
            {
                Thread.Sleep(500);

                m_engine.OnMessage -= new MessageEventHandler(OnMessage);
                m_engine.OnCommand -= new CommandEventHandler(OnCommand);
                m_engine.OnNoise -= new NoiseEventHandler(OnNoise);

                try
                {
                    m_engine.Dispose();
                }
                catch
                {
                    // Depending on when we get called, stopping the engine 
                    // throws anything from NullReferenceException, ArgumentNullException, IOException, etc.
                }

                m_engine = null;

                if (m_emulatorProcess != null && !m_emulatorProcess.WaitForExit(4000))
                {
                    if (m_emulatorProcess != null && !m_emulatorProcess.HasExited)
                    {
                        m_emulatorProcess.Kill();
                        m_emulatorProcess = null;
                    }

                    foreach (Process p in Process.GetProcessesByName("Microsoft.SPOT.Emulator.Sample.SampleEmulator"))
                    {
                        p.Kill();
                    }
                }

                GC.Collect();
            }

            ///
            /// Make sure process exit thread dies
            /// 
            if (m_waitForProc != null && m_waitForProc.IsAlive)
            {
                try
                {
                    if (!m_waitForProc.Join(1000))
                    {
                        m_waitForProc.Abort();
                    }
                }
                catch
                {
                }
            }
        }



        #endregion

        #region Private

        internal Result TestResults
        {
            set
            {
                m_testResult = value;
            }
        }

        private bool IsTestMsiInstalled
        {
            get
            {
                m_installPath = TestSystem.RunMFTests.InstallRoot;
                if (!string.IsNullOrEmpty(m_installPath))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private void RunDeviceTest(string buildPath, string exeLocation, ArrayList referenceList)
        {
            m_deviceDone.Reset();
            ConnectToDevice(buildPath, exeLocation, referenceList);
        }

        private HarnessExecutionResult HarnessResult(Exception ex)
        {
            // Dispose engine on all exceptions so that the device is not locked for next automation.
            Close();

            Console.WriteLine("\tERROR: " + ex.ToString());
            m_log.AddCommentToLog("A Exception was thrown when running the test: " + ex.ToString());            

            if (ex is IOException &&
                ex.Message.ToLower().Contains("request failed"))
            {
                // The request got aborted on the device somehow. In this case, we will rerun the test.
                // Send an unavailable result.
                return HarnessExecutionResult.Unavailable;
            }

            if ((ex is ApplicationException &&
                ex.Message.Contains("Could not find the specified device for the harness to connect to."))
                ||(ex.Message.ToLower().Contains("connection failed")))
            {
                return HarnessExecutionResult.NoConnection;
            }            

            return HarnessExecutionResult.Abort;
        }

        private Thread StartDesktopApplication(string path, ref string csprojFilePath)
        {
            string desktopApp = string.Empty;
            m_desktopAppName = GetDesktopApplication(path + @"\" + m_desktopXmlFile);
            if (m_isDevEnvironment)
            {
                desktopApp = Environment.GetEnvironmentVariable("spoclient") + @"\test\platform\tests\Scratch\" + m_desktopAppName;
            }
            else
            {
                desktopApp = string.Format("{0}{1}", m_installPath, @"Server\") + m_desktopAppName;
            }

            // Get references and build the test.
            GetProjectReferences(desktopApp, ref csprojFilePath);            
            BuildTest(csprojFilePath, TestType.Desktop);

            // Start the desktop exe in a new thread.
            string exePath = GetBuildPath(desktopApp) + m_desktopAppName.Split('\\')[0] + ".exe";
            Thread dtThread = new Thread(new ParameterizedThreadStart(RunDesktopTest));
            dtThread.Start(exePath);
            while (!dtThread.IsAlive)
            {
                Console.WriteLine("Waiting for the desktop application thread to start");
            }
            return dtThread;
        }

        private void StopDesktopApplication(NamedPipeServerStream pipeStream, Thread dtThread)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            string message1 = "TestSuite Run Completed";
            Byte[] bytes;

            bytes = encoding.GetBytes(message1);
            pipeStream.Write(bytes, 0, bytes.Length);
            pipeStream.Dispose();

            if (dtThread != null)
            {
                dtThread.Abort();
            }
        }

        private string GetDesktopApplication(string xmlFile)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlFile);
            XmlNode node = doc.SelectSingleNode("/DesktopApplication");
            return node.InnerText;
        }

        private void RunDesktopTest(object exePath)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = exePath.ToString();            
            psi.UseShellExecute = true;
            Process proc = new Process();
            proc.StartInfo = psi;
            Console.WriteLine("\tExecuting the desktop test.. ");
            proc.Start();
            proc.WaitForExit();            
            int val = proc.ExitCode;
        }

        private Process m_emulatorProcess;

        private void WaitForProcessExit()
        {
            try
            {
                if (string.Equals(m_transport.ToLower(), "emulator"))
                {
                    Process[] p = Process.GetProcessesByName("Microsoft.SPOT.Emulator.Sample.SampleEmulator");

                    if (p != null && p.Length > 0)
                    {
                        m_emulatorProcess = p[p.Length - 1];

                        m_emulatorProcess.WaitForExit();
                        m_deviceDone.Set();
                    }
                }
                else
                {
                    int retries = 90; // 15 minutes

                    while (true)
                    {
                        if (!m_deviceDone.WaitOne(10000))
                        {
                            if(m_engine.TryToConnect(10, 500, true, ConnectionSource.Unknown))
                            {
                                Commands.Monitor_Ping.Reply ping = m_engine.GetConnectionSource();

                                if (ping != null)
                                {
                                    if (0 != (ping.m_dbg_flags & Commands.Monitor_Ping.c_Ping_DbgFlag_AppExit))
                                    {
                                        m_deviceDone.Set();
                                        break;
                                    }
                                }
                            }
                            else if (retries-- <= 0)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }

        Thread m_waitForProc = null;

        private void AttachToProcess()
        {
            m_waitForProc = new Thread(new ThreadStart(WaitForProcessExit));

            m_waitForProc.Start();
        }

        private void ConnectToDevice(string buildPath, string exePath, ArrayList referenceList)
        {
            TestSystem.IncludesDeviceTest = true;
            PortDefinition port = Utils.GetPort(m_device, m_transport, exePath);

            try
            {
                for (int retry = 0; retry < 3; retry++)
                {
                    m_engine = new Microsoft.SPOT.Debugger.Engine(port);
                    m_engine.StopDebuggerOnConnect = true;
                    m_engine.Start();

                    bool connected = false;

                    connected = m_engine.TryToConnect(200, 500, true, ConnectionSource.TinyCLR);

                    if (connected)
                    {
                        m_engine.PauseExecution();

                        if (!string.Equals(m_transport.ToLower(), "emulator"))
                        {
                            // Deploy the test files to the device.                    
                            Utils.DeployToDevice(buildPath, referenceList, m_engine, m_transport, m_isDevEnvironment, m_assemblyName);

                            // Connect to the device and execute the deployed test.
                            m_engine.RebootDevice(Microsoft.SPOT.Debugger.Engine.RebootOption.RebootClrWaitForDebugger);

                            // give the device some time to restart (especially for tcp/ip)
                            Thread.Sleep(500);

                            if (m_engine.PortDefinition is PortDefinition_Tcp)
                            {
                                Thread.Sleep(1000);
                            }

                            connected = false;

                            connected = m_engine.TryToConnect(200, 500, true, ConnectionSource.TinyCLR);
                        }


                        if (!connected)
                        {
                            DetachFromEngine();
                            throw new ApplicationException("Reboot Failed");
                        }

                        AttachToProcess();

                        m_engine.ThrowOnCommunicationFailure = true;
                        m_engine.OnMessage += new MessageEventHandler(OnMessage);
                        m_engine.OnCommand += new CommandEventHandler(OnCommand);
                        m_engine.OnNoise += new NoiseEventHandler(OnNoise);

                        Console.WriteLine("\tExecuting the device test..");
                        m_initialTime = DateTime.Now;
                        m_engine.ResumeExecution();

                        m_deviceDone.WaitOne();
                        break;
                    }
                    else
                    {
                        DetachFromEngine();
                        //throw new ApplicationException("Connection failed");
                    }
                }
            }
            catch(Exception ex)
            {
                DetachFromEngine();
                throw new ApplicationException("Connection failed: " + ex.ToString());
            }
        }

        void OnNoise(byte[] buf, int offset, int count)
        {
            System.Diagnostics.Debug.Print(new string(UTF8Encoding.UTF8.GetChars(buf, offset, count)));
        }

        private void OnCommand(IncomingMessage msg, bool reply)
        {
            // The event will signal the program exit. Set the manual reset event once the program exits
            // and dispose the engine. 
            if (msg.Header.m_cmd == Microsoft.SPOT.Debugger.WireProtocol.Commands.c_Monitor_ProgramExit)
            {
                m_deviceDone.Set();
            }
        }

        private void OnMessage(IncomingMessage msg, string mesg)
        {
            // Append the debug message to the output string object.
            m_debugText.Append(mesg);
        }

        private void ShutEngine()
        {
            DetachFromEngine();
        }

        private void SetTestResult(string fileName)
        {
            int passCount = 0;

            if (null == fileName)
            {
                throw new NullReferenceException("Parameter fileName is null");
            }

            if (File.Exists(fileName))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(fileName);

                // Set MFTestResult.
                // If there is a <Fail> or an <Exception> node in the result, the test has failed.
                // If the log file does not contain a TestMethodResult section, then it means the test
                // did not run completely and was aborted.
                XmlNodeList nodes = doc.GetElementsByTagName("TestMethodResult");
                if (nodes.Count == 0)
                {
                    m_testResult = Result.Fail;
                    doc.Save(fileName);
                    return;
                }

                XmlNodeList failureNodes = doc.GetElementsByTagName("Test_Exception");
                if (failureNodes.Count > 0)
                {
                    m_testResult = Result.Fail;
                    doc.Save(fileName);
                    return;
                }

                // Go through each of the testlog nodes.
                XmlNodeList testLogNodes = doc.GetElementsByTagName("TestLog");
                foreach (XmlNode testLogNode in testLogNodes)
                {
                    int localPassCount = 0, localSkipCount = 0, localKnownFailCount = 0;

                    XmlNodeList tmResultNodes = testLogNode.SelectNodes("TestMethod/TestMethodResult");
                    foreach (XmlNode node in tmResultNodes)
                    {
                        if (string.Equals(node.Attributes["Result"].Value, "Fail", StringComparison.InvariantCultureIgnoreCase))
                        {
                            m_testResult = Result.Fail;
                            doc.Save(fileName);
                            return;
                        }
                        else if (string.Equals(node.Attributes["Result"].Value, "Skip", StringComparison.InvariantCultureIgnoreCase))
                        {
                            m_testResult = Result.Skip;
                            localSkipCount++;
                        }
                        else if (string.Equals(node.Attributes["Result"].Value, "KnownFailure", StringComparison.InvariantCultureIgnoreCase))
                        {
                            m_testResult = Result.KnownFailure;
                            localKnownFailCount++;
                        }
                        else if (string.Equals(node.Attributes["Result"].Value, "Pass", StringComparison.InvariantCultureIgnoreCase))
                        {
                            passCount++;
                            localPassCount++;
                        }
                    }

                    if ((localPassCount == 0) && (localSkipCount == 0) && (localKnownFailCount == 0))
                    {
                        m_testResult = Result.Fail;
                        doc.Save(fileName);
                        return;
                    }
                }
                // If the passcount is more than 1, return a pass though the test might contain skipped or known failures.
                if (passCount > 0)
                {
                    m_testResult = Result.Pass;
                }

                doc.Save(fileName);
            }
        }

        private void RecurseProjDep(string proj, ref ArrayList refs)
        {
            try
            {
                if (!File.Exists(proj)) return;

                Assembly asm = Assembly.LoadFile(proj);
                string path = Path.GetDirectoryName(proj);

                foreach (AssemblyName asmRef in asm.GetReferencedAssemblies())
                {
                    if (!refs.Contains(asmRef.Name))
                    {
                        refs.Add(asmRef.Name);
                    }

                    RecurseProjDep(Path.Combine(path, asmRef.Name + ".dll"), ref refs);
                }
            }
            catch
            {
            }
        }


        private ArrayList GetProjectReferences(string slnFilePath, ref string csprojFilePath)
        {
            ArrayList refList = new ArrayList();
            csprojFilePath = FindProject(slnFilePath, slnFilePath.Substring(slnFilePath.LastIndexOf(@"\") + 1));

            // Load the csproj file using msbuild and get the list of added references.
            // We need this list to generate the list of pe files.
            _BE.Project prj = new _BE.Project(csprojFilePath);
            char[] splitChar = { ',' };
            foreach (_BE.ProjectItem item in prj.GetItems("Reference"))
            {
                string[] referenceItem = item.EvaluatedInclude.Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
                if (!refList.Contains(referenceItem[0].Trim()))
                {
                    refList.Add(referenceItem[0].Trim());
                }

                if (item.HasMetadata("HintPath"))
                {
                    string hint = item.GetMetadataValue("HintPath");

                    RecurseProjDep(hint, ref refList);
                }
                else
                {
                    string output = Environment.GetEnvironmentVariable("BUILD_TREE_CLIENT") + "\\dll\\" + referenceItem[0] + ".dll";

                    if (File.Exists(output))
                    {
                        RecurseProjDep(output, ref refList);
                    }
                }
            }

            // Set the assembly name.
            m_assemblyName = prj.GetPropertyValue("AssemblyName");

            // Determine if the test is a desktop test.
            string value = prj.GetPropertyValue("ProjectTypeGuids");
            m_currentAppType = string.IsNullOrEmpty(value) ? TestType.Desktop:TestType.Device;            

            // Determine the desktop app if this is a device-desktop test.
            foreach (_BE.ProjectItem item in prj.Items)
            {
                if (item.ItemType.ToLower().Contains("content"))
                {
                    m_desktopXmlFile = item.EvaluatedInclude;
                    m_currentAppType = TestType.DeviceDesktop;
                }
            }                       

            return refList;
        }        

        private string FindProject(string file, string name)
        {
            // Load the solution file using msbuild and get the csproj file in it.
            StringReader reader = new StringReader(SolutionWrapperProject.Generate(file, null, null));
            XmlReader xml = XmlReader.Create(reader);
            _BE.Project prj = new _BE.Project( xml );
            string csprjName = string.Empty;
            foreach (_BE.ProjectItem bi in prj.Items)
            {
                if (string.Equals("_SolutionProjectProjects", bi.ItemType, StringComparison.InvariantCultureIgnoreCase))
                {
                    csprjName = bi.EvaluatedInclude;
                }
            }
            file = file.Replace(name, csprjName);
            return file;
        }

        private string FindLatestMsbuildVersion()
        {
            string version = "";
            string baseDir = Environment.SystemDirectory;
                
            baseDir = baseDir + @"\..\Microsoft.NET\Framework\";

            Regex exp = new Regex(@"v\d+\.\d+[\.\d+]?[\.\d+]?");

            if (Directory.Exists(baseDir))
            {
                string[] dirs = Directory.GetDirectories(baseDir);
                
                Array.Sort<string>(dirs);

                foreach (string dir in dirs)
                {
                    string ver = Path.GetFileName(dir);
                    if (exp.IsMatch(ver))
                    {
                        version = ver;
                    }
                }
            }

            return baseDir + version + "\\";
        }

        private void BuildTest(string file, TestType type)
        {
            // Build the csproj.
            bool isBuildDone = false;
            int attempts = 0;
            string output = string.Empty;
            string msbuildexe = FindLatestMsbuildVersion() + "MSBuild.exe";

            // Attempting building upto three times.
            while (!isBuildDone && attempts++ < 3)
            {
                Console.Write("\tBuilding the " + type.ToString().ToLower() + " test..");
                ProcessStartInfo psi = new ProcessStartInfo(msbuildexe, "\"" + file + "\"");
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                Process proc = System.Diagnostics.Process.Start(psi);
                output = proc.StandardOutput.ReadToEnd();
                isBuildDone = proc.WaitForExit(60000);                

                if (isBuildDone && proc.ExitCode == 0)
                {
                    Console.WriteLine(" built successfully");
                    return;
                }
                else
                {
                    isBuildDone = false;
                    Console.WriteLine(" build failure.. retrying..");
                }
            }

            if (!isBuildDone)
            {
                Console.WriteLine(" BUILD FAILURE.. quitting..");
                throw new ApplicationException("\r\nBUILD FAILURE: \r\n" + output + "\r\n File: \r\n" + file);
            }
        }

        private string GetBuildPath(string file)
        {
            string buildPath = string.Empty;
            string pathValue = (m_currentAppType == TestType.Desktop) ? "server" : "client";

            if (m_isDevEnvironment)
            {
                string flav = Environment.GetEnvironmentVariable("FLAVOR");
                
                if (string.IsNullOrEmpty(flav))
                {
                    flav = "Release";
                }

                buildPath = Environment.GetEnvironmentVariable("SPOCLIENT")
                    + @"\buildoutput\public\" + flav
                    + @"\test\" + pathValue + @"\dll\";

                if (!Directory.Exists(buildPath))
                {
                    flav = flav.ToLower() == "release" ? "Debug" : "Release";

                    buildPath = Environment.GetEnvironmentVariable("SPOCLIENT")
                        + @"\buildoutput\public\" + flav
                        + @"\test\" + pathValue + @"\dll\";
                }
            }
            else
            {
                buildPath = file.Replace(".sln", "") + @"\bin\debug\";
                if (!Directory.Exists(buildPath))
                {
                    buildPath = file.Replace(file.Substring(file.LastIndexOf("\\") + 1), "") + @"bin\debug\";                    
                }
            }

            return buildPath;
        }

        private HarnessExecutionResult RunProfilerTest(string file, string pathToProfTestExe, string buildPath, ArrayList referenceList)
        {
            m_logFileName = m_mfLogDirectory + "\\" + m_logFileName.Replace("_Log", "_Profile").Replace(".xml", ".log");
            Profile profiler = new Profile();

            try
            {
                profiler.StartProfiler(this.Device, m_logFileName,
                    this.Transport, pathToProfTestExe, buildPath, referenceList, m_isDevEnvironment, m_assemblyName);
                profiler.Done.WaitOne();

            }
            catch (Exception ex)
            {
                Console.WriteLine("An Exception was thrown: " + ex.ToString());
                Utils.WriteToEventLog(string.Format("Exception in Harness when running the profiler: {0}", ex.ToString()));
                Thread.Sleep(2000);
                switch(ex.Message.ToLower())
                {
                    case "request failed":
                        return HarnessExecutionResult.Unavailable;
                        
                    case "noconnection":
                        return HarnessExecutionResult.NoConnection;                        
                }
            }

            return HarnessExecutionResult.Success;
        }

        private void SetLogFileName(string test)
        {
            // When the log file name is auto generated and is not custom.
            if (!m_useCustomLog)
            {
                m_logFileName = test.Substring(test.LastIndexOf(@"\") + 1);
                m_logFileName = m_logFileName.Replace(".sln", string.Empty) + "_Log.xml";
            }
            else
            {
                // Check if the custom log file name ends with a ".xml".
                if (m_logFileName.ToLower().EndsWith(".xml"))
                {
                    m_logFileName = m_logFileName.ToLower().Replace(".xml", String.Empty);
                }

                m_logFileName = string.Format("{0}.xml", m_logFileName);
            }

            if (!m_mfLogDirectory.EndsWith("\\"))
            {
                m_mfLogDirectory = string.Format("{0}{1}", m_mfLogDirectory, "\\");
            }
            
            if (TestSystem.IsMultipleRun && File.Exists(m_outputXmlFileName))
            {
                m_logFileName = m_logFileName.Replace("_Log.xml", "_Log" + m_logCount++ + ".xml");
            }
            m_outputXmlFileName = m_mfLogDirectory + m_logFileName;
        }

        private string BuildFilePath(BaseTest test, ref string xslPath)
        {
            string toolSrcPath = @"\test\Platform\";
            string srcPath = @"\test\Platform\Tests\";
            string enlistmentPath = GetEnlistmentPath;
            bool isTestMsiInstalled = IsTestMsiInstalled;
            string file = string.Empty;
            xslPath = "Results.xsl";

            if (!String.IsNullOrEmpty(test.Name))
            {
                // If dev environment, use the test files from the enlistment.
                // Else If the test msi is installed, use the test files from the installed location.
                // Else, neither enlistment exists nor test msi is installed - throw a file not found exception.
                if (m_isDevEnvironment)
                {
                    if (!Path.IsPathRooted(test.Name))
                    {
                        test.Name = Path.Combine(Environment.GetEnvironmentVariable("SPOCLIENT"), test.Name);
                    }

                    if (File.Exists(test.Name))
                    {
                        file = Path.GetFullPath(test.Name);
                    }
                    else
                    {
                        if (test is ProfilerTest)
                        {
                            file = enlistmentPath + srcPath + @"\Performance\ProfilerTests\" + test.Name;
                        }
                        else
                        {
                            file = enlistmentPath + srcPath + test.Name;
                        }
                    }

                    if (!File.Exists(file))
                    {
                        throw new FileNotFoundException();
                    }

                    // Set the log folder path.
                    m_mfLogDirectory = enlistmentPath + toolSrcPath + @"Tools\MFTestSystem\Results";
                }
                else if (isTestMsiInstalled)
                {
                    m_mfLogDirectory = m_mfLogDirectory = string.Format("{0}{1}\\{2}\\", 
                        m_installPath, "Results", TestSystem.TestResultId);
                    
                    if (test is ProfilerTest)
                    {
                        srcPath = string.Format("{0}{1}", m_installPath, @"ManagedProfilerTests\");
                    }
                    else
                    {
                        // If the file doesn't exist assume that the path is not an absolute path.                        
                        srcPath = string.Format("{0}{1}", m_installPath, @"TestCases\");
                        toolSrcPath = string.Format("{0}{1}", m_installPath, @"Tools\");                        
                    }

                    if (!File.Exists(test.Location))
                    {
                        file = srcPath + test.Location;
                    }
                    else
                    {
                        file = test.Location;
                    }
                }
                else
                {
                    throw new System.Exception("Could not find the tests. Please install the test msi");
                }

                if (!File.Exists(file))
                {
                    Console.WriteLine("\tERROR: File not found: " + file);
                    Utils.WriteToEventLog("File Not Found: " + file);
                    throw new System.ArgumentException(
                        "Specified file " + file + " could not be found.");
                }
            }

            test.Location = file;
            return file;
        }

        internal void SaveLogFile(string testName, TestType type)
        {
            switch(type)
            {
                case TestType.Device:
                    CloseAndSaveLog(m_debugText.ToString());
                    break;

                case TestType.Desktop:
                    CloseAndSaveLog(ReadTempDesktopLog());
                    break;

                case TestType.DeviceDesktop:
                    string dtOutput = ReadTempDesktopLog();
                    m_debugText.Append(dtOutput);
                    CloseAndSaveLog(m_debugText.ToString());                    
                    break;
            }            
        }

        internal void SaveLogFile(string testName)
        {
            SaveLogFile(testName, m_currentAppType);
        }

        private string ReadTempDesktopLog()
        {
            string path = Environment.GetEnvironmentVariable("temp") + @"\DesktopTestRunner.log";
            StreamReader sr = new StreamReader(path);
            string dtOutput = sr.ReadToEnd();
            sr.Close();
            sr.Dispose();

            return dtOutput;
        }

        private void CloseAndSaveLog(string value)
        {
            if (!m_logFileSaved)
            {
                m_log.CloseLog(m_log.ConvertUnsafeXmlStrings(value));
                m_logFileSaved = true;
            }
        }

        private void StartCodeCoverage(string testName)
        {
            // Start code coverage recording only if the current build is an instrumented build.
            if (IsCurrentBuildInstrumented)
            {
                // Reset the code coverage information.
                System.Diagnostics.Process proc = System.Diagnostics.Process.Start("covercmd.exe", @" /reset");
                proc.Close();
                proc.Dispose();
            }
        }

        private void StopCodeCoverage(string testName)
        {
            if (IsCurrentBuildInstrumented)
            {
                // Stop the code coverage and save the collected information.
                System.Diagnostics.Process proc = System.Diagnostics.Process.Start("covercmd.exe", @" /save /as " + testName);

                // Get the location where magellan is saving the files.
                string ccPath = Environment.GetEnvironmentVariable("Coverage") + @"\TinyCLR.dll";

            }
        }

        private bool IsCurrentBuildInstrumented
        {
            get
            {
                return false;
            }
        }

        private string GetEnlistmentPath
        {
            get
            {
                return Environment.GetEnvironmentVariable("CLRROOT");
            }
        }

        private void CopyFiles(DirectoryInfo diSource, DirectoryInfo diTarget, string sourceDir)
        {
            try
            {
                // Copy all the files in that directory.
                foreach (FileInfo fi in diSource.GetFiles())
                {
                    fi.CopyTo(Path.Combine(diTarget.FullName, fi.Name), true);
                }

                // Copy the sub directories and their content if they exist.
                foreach (DirectoryInfo subDir in diSource.GetDirectories())
                {
                    string currDir = diTarget.FullName + "\\" + subDir;
                    if (Directory.Exists(currDir))
                    {
                        Directory.Delete(currDir);
                    }
                    DirectoryInfo newdiTarget = Directory.CreateDirectory(currDir);
                    CopyFiles(new DirectoryInfo(diSource + "\\" + subDir), newdiTarget, subDir.FullName);
                }
            }
            catch (System.IO.DirectoryNotFoundException ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        #endregion
    }

    #region HarnessExecutionResult Enumeration

    /// <summary>
    /// This enumeration specifies whether the harness was able to execute the
    /// test successfully or if a failure happened which could be due to a ComException
    /// being thrown or a TimeOut.
    /// </summary>
    internal enum HarnessExecutionResult
    {
        Abort,
        Exception,
        NoConnection,
        Success,
        TimeOut,
        Unavailable,
        InvalidArguments
    }

    #endregion
}
