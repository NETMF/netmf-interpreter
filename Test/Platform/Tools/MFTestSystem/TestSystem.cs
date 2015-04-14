using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SPOT.Platform.Test;
using Microsoft.Win32;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Timers;
using System.Xml;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using Microsoft.SPOT.Debugger;
using System.Reflection;

namespace Microsoft.SPOT.Platform.Test
{ 
    [Serializable]
    public class TestSystem
    {        
        #region Member variables

        private static int m_totalTestCases = 0, m_totalPassed = 0, m_totalFailed = 0,
            m_totalSkipped = 0, m_totalKnownFailures = 0;
        private static string[] m_args;
        private static RunMFTests tests = new RunMFTests();
        private static TestSystem m_testSystem = new TestSystem();
        private string m_mailTo, m_device, m_transport, m_fullDeviceName;
        private bool m_displayLog = false;
        private static string m_resultsFolder, m_resultsFile, 
            m_testResultId ,m_buildNumber = string.Empty;
        private string m_branch, m_buildFlavor;
        private static ArrayList m_mfTestList = new ArrayList();        

        #endregion

        #region Enumerations

        /// <summary>
        /// An enumeration for the transport types.
        /// </summary>
        public enum TransportType
        {
            USB,
            Emulator,
            Serial,
            TCPIP,
            None
        }

        private string[] m_transportType = {"USB", "Emulator", "Serial", "TCPIP"};

        public enum ExitCode
        {
            Success = 0,
            InvalidArguments = -1,
            UnexpectedException = -2,
            FailedTests = -3
        }

        #endregion

        #region Entry points

        static int Main(string[] args)
        {
            // Parse the command line arguments.
            if (args != null && args.Length != 0)
            {
                if (args[0] == "/?")
                {
                    DisplayHelp();
                    return (int)ExitCode.Success;
                }
                else if (String.Equals(args[0], "-help", StringComparison.InvariantCultureIgnoreCase))
                {
                    DisplayHelp();
                    return (int)ExitCode.Success;
                }
            }

            //Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Replace(@"file:\", ""));

            // This is the entry point when using the console app - runtests.exe.
            // Always display log file at the completion of tests when running the exe.
            m_testSystem.DisplayLog = true;

            m_args = args;
            try
            {
                m_testSystem.RunTests();
                if (m_testSystem.DidAllTestsPass)
                {
                    return (int)ExitCode.Success;
                }
                else
                {
                    Console.Error.WriteLine("Not all tests passed successfully!");
                    return (int)ExitCode.FailedTests;
                }
            }
            catch(ArgumentException)
            {
                return (int)ExitCode.InvalidArguments;
            }
            catch (Exception ex)
            {
                string errMsg = String.Format("TestSystem: " + "An uncaught exception was thrown when invoking RunTests: {0}", ex.ToString());
                Console.Error.WriteLine(errMsg);
                Utils.WriteToEventLog(errMsg);
                return (int)ExitCode.UnexpectedException;
            }
        }

        /// <summary>
        /// Use this method to execute tests.
        /// </summary>
        public void RunTests()
        {
            try
            {
                Utils.WriteToEventLog("TestSystem: Executing the tests.");
                string timeString = DateTime.Now.ToString().Replace('/', '_').Replace(':', '_');
                m_testResultId = "TestResults_" + timeString;
                m_resultsFile = "Results.htm";
                HarnessExecutionResult testRunResult = tests.Run(m_args);

                if (testRunResult == HarnessExecutionResult.InvalidArguments)
                {
                    throw new ArgumentException();
                }

                // Update the test run properties.
                m_totalTestCases = tests.TotalTestCases;
                m_totalPassed = tests.PassCount;
                m_totalFailed = tests.FailCount;
                m_totalSkipped = tests.SkipCount;
                m_totalKnownFailures = tests.KnownFailureCount;
                
                // Console app case, set m_mailTo.
                if (string.IsNullOrEmpty(m_mailTo))
                {
                    m_mailTo = tests.EmailList;
                }

                // Perform post run tasks and generate the results file.
                try
                {
                    PerformPostRunTasks();
                }
                catch (Exception ex) 
                {
                    Utils.WriteToEventLog(String.Format("TestSystem: " +
                        "An uncaught exception was thrown when calling PerformPostRunTasks: {0}", ex.ToString()));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nERROR: " + ex.ToString());
                Utils.WriteToEventLog(string.Format("Exception in TestSystem: {0}", ex.ToString()));
                throw;
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Total test cases executed in the run.
        /// </summary>
        public int TotalTestCases
        {
            get
            {
                return m_totalTestCases;
            }

            set
            {
                m_totalTestCases = value;
            }
        }

        /// <summary>
        /// Pass count for the run.
        /// </summary>
        public int PassCount
        {
            get
            {
                return m_totalPassed;
            }

            set
            {
                m_totalPassed = value;
            }
        }

        /// <summary>
        /// Fail Count for the run.
        /// </summary>
        public int FailCount
        {
            get
            {
                return m_totalFailed;
            }

            set
            {
                m_totalFailed = value;
            }
        }

        /// <summary>
        /// Skip count for the run.
        /// </summary>
        public int SkipCount
        {
            get
            {
                return m_totalSkipped;
            }

            set
            {
                m_totalSkipped = value;
            }
        }

        /// <summary>
        /// Known failure count for the run.
        /// </summary>
        public int KnownFailureCount
        {
            get
            {
                return m_totalKnownFailures;
            }

            set
            {
                m_totalKnownFailures = value;
            }
        }

        /// <summary>   
        /// List containing the list of tests that ran with the results.
        /// </summary>
        public ArrayList TestResultList
        {
            get
            {
                return m_mfTestList;
            }
        }

        /// <summary>
        /// Use this property to get/set the email address(s) to send the results mail to.
        /// </summary>
        public string MailTo
        {
            get
            {
                m_mailTo = tests.EmailList;
                return m_mailTo;
            }

            set
            {
                m_mailTo = value;
                tests.EmailList = value;
            }
        }

        /// <summary>
        /// This property can be used to specify the device for the tests.
        /// </summary>
        public string Device
        {
            get
            {
                m_device = tests.Device;
                return m_device;
            }

            set
            {
                m_device = value;
                tests.Device = value;
            }
        }

        public string FullDeviceName
        {
            get
            {
                m_fullDeviceName = tests.FullDeviceName;
                return m_fullDeviceName;
            }
            set
            {
                m_fullDeviceName = value;
                tests.FullDeviceName = value;
            }
        }

        /// <summary>
        /// This property can be used to get/set the transport type for the tests.
        /// </summary>
        public TransportType Transport
        {
            get
            {
                m_transport = tests.Transport;
                if (!string.IsNullOrEmpty(m_transport))
                {
                    switch (m_transport.ToLower())
                    {
                        case "usb":
                            return TransportType.USB;

                        case "serial":
                            return TransportType.Serial;

                        case "tcpip":
                            return TransportType.TCPIP;

                        default:
                            return TransportType.Emulator;
                    }
                }
                else
                {
                    if (!IncludesDeviceTest)
                    {
                        return TransportType.None;
                    }
                    else
                    {
                        return TransportType.Emulator;
                    }
                }
            }

            set
            {
                m_transport = m_transportType[(int)value];
                tests.Transport = m_transportType[(int)value];
            }
        }

        /// <summary>
        /// A boolean property that specifies whether to display the results log after
        /// the tests are completed.
        /// </summary>
        public bool DisplayLog
        {
            get
            {
                return m_displayLog;
            }

            set
            {
                m_displayLog = value;
            }
        }

        ///// <summary>
        ///// Property that specifies the UNC location for the test results.
        ///// </summary>
        public string ResultsFolder
        {
            get
            {
                m_resultsFolder = RunMFTests.ResultsPath;
                return m_resultsFolder;
            }

            set
            {
                m_resultsFolder = value;
                RunMFTests.ResultsPath = value;
            }
        }

        public bool IsAlternateLogStoreSpecified
        {
            get
            {
                return RunMFTests.IsAlternateLogStoreSpecified;
            }
        }

        /// <summary>
        /// Property that specifies the build number.
        /// </summary>
        public static string BuildNumber
        {
            get
            {
                m_buildNumber = RunMFTests.BuildNumber;
                return m_buildNumber;
            }

            set
            {
                m_buildNumber = value;
                RunMFTests.BuildNumber = value;
            }
        }

        public static string ResultsFile
        {
            get
            {
                return m_resultsFile;
            }
        }

        internal static string TestResultId
        {
            get
            {
                return m_testResultId;
            }
        }

        /// <summary>
        /// Property that specifies the branch the run is from.
        /// </summary>
        public string Branch
        {
            get
            {
                m_branch = RunMFTests.Branch;
                return m_branch;
            }

            set
            {
                m_branch = value;
                RunMFTests.Branch = value;
            }
        }

        /// <summary>
        /// Property that specifies the build type - ship, debug or rtm.
        /// </summary>
        public string BuildFlavor
        {
            get
            {
                m_buildFlavor = RunMFTests.BuildFlavor;
                return m_buildFlavor;
            }

            set
            {
                m_buildFlavor = value;
                RunMFTests.BuildFlavor = value;
            }
        }

        /// <summary>
        /// Property that specifies whether the test run was successful or not.
        /// </summary>
        public bool DidAllTestsPass
        {
            get
            {
                return tests.DidAllTestsPass;
            }
        }

        public bool IsProfilerRun
        {
            get
            {
                return tests.IsProfilerRun;
            }

            set
            {
                tests.IsProfilerRun = value;
            }
        }

        public bool IsDevEnvironment
        {
            get
            {
                return tests.IsDevEnvironment;
            }
        }

        internal static bool IsMultipleRun
        {
            get
            {
                return tests.IsMultipleRun;
            }
        }

        public static bool IncludesDeviceTest
        {
            get
            {
                return RunMFTests.IncludesDeviceTest;
            }

            set
            {
                RunMFTests.IncludesDeviceTest = value;
            }
        }

        #endregion

        #region Internal Class RunMFTests

        [Serializable]
        internal class RunMFTests
        {
            #region Member variables

            private static int m_totalTestCases = 0;
            private static bool m_didAllTestsPass = false, m_isAlternateLogStoreSpecified = false, m_includesDeviceTest = false;
            private bool m_isProfilerRun = false, m_isStaticList = false;
            private string m_staticList;
            private int m_runCount = 1, m_passCount = 0, m_failCount = 0, m_skipCount = 0, 
                m_knownFailCount = 0;
            private string m_emailList = string.Empty, m_transport = null, 
                m_device = string.Empty, m_test, m_fullDeviceName = string.Empty;
            private static string m_buildNumber = string.Empty, m_branch = string.Empty, m_buildFlavor = string.Empty,
                m_logStore = string.Empty, m_alternateLogStore = string.Empty, m_installRoot = null, m_productInstallRoot = null;
            private bool m_isDevEnv = false;
            private XmlLog m_log;

            #endregion

            #region Run

            internal HarnessExecutionResult Run(string[] args)
            {
                TimedTest test;
                HarnessExecutionResult hResult = HarnessExecutionResult.Unavailable;
                bool runTestsIndividually = false;
                m_log = new XmlLog();

                // Prelim: Set the env, parse the arguments, set the result paths and set the test list.
                SetEnvironment();
                if (args != null && args.Length > 0)
                {
                    if (!ParseArguments(args, ref runTestsIndividually))
                    {
                        return HarnessExecutionResult.InvalidArguments;
                    }
                }
                SetResultPaths();
                BaseTest[] testList = BuildTestList(runTestsIndividually);

                // Create a new harness object and set the properties.
                Harness harness = new Harness(IsDevEnvironment);
                if (m_transport != null)
                {
                    harness.Transport = m_transport;
                }
                else
                {
                    // harness constructor assigns default transport
                    m_transport = harness.Transport;
                }
                if (m_device != null)
                {
                    harness.Device = m_device;
                }
                else
                {
                    // harness constructor assigns default device
                    m_device = harness.Device;
                }

                // Execute each of the solution files using Harness.
                for (int i = 0; i < testList.Length; i++)
                {
                    if (testList[i] == null)
                    {
                        continue;
                    }

                    if (this.Transport.ToLower().Contains("tcpip") &&
                        (testList[i].Name.ToLower().Contains("netinfotests.sln")))
                    {
                        continue;
                    }

                    
                    hResult = HarnessExecutionResult.Unavailable;
                    int attempts = 0;
                    while ((hResult != HarnessExecutionResult.Success &&
                        hResult != HarnessExecutionResult.Abort) && attempts++ < 3)
                    {
                        test = new TimedTest(testList[i], harness, m_log);

                        // Kill any emulators running from previous runs.
                        TerminateRunningEmulators(test, testList[i]);

                        try
                        {
                            hResult = test.Execute();

                            if (hResult == HarnessExecutionResult.Unavailable)
                            {
                                Utils.WriteToEventLog("Harness returned an unavailable result after running the test: " +
                                    testList[i].Name + ". No of tries so far = " + attempts);
                                string deviceStatus = DeviceStatus(hResult);
                                Utils.WriteToEventLog("Device status after unavailable from harness: " + deviceStatus);
                            }

                            // Test did not execute because the device was dead.
                            // If so, reset power to the device and re-run test.
                            if (hResult == HarnessExecutionResult.NoConnection)
                            {
                                Utils.WriteToEventLog("Harness returned an NoConnection result after running the test: " +
                                    testList[i].Name + ". No of tries so far = " + attempts);
                                string deviceStatus = DeviceStatus(hResult);
                                Utils.WriteToEventLog("Device status after noconnection from harness: " + deviceStatus);                                
                            }

                            // Test did not succeed running in three attempts.
                            if (hResult == HarnessExecutionResult.TimeOut)
                            {
                                Utils.WriteToEventLog("Test: " + test.Test.Name + " failed.");
                                harness.MFTestResult = Harness.Result.Fail;
                                GetTestResultDetails(harness, testList[i]);
                                Console.WriteLine("Test Result: " + harness.MFTestResult);
                                test.SendMail();
                                break;
                            }

                            // Test did not succeed running in three attempts.
                            if (hResult != HarnessExecutionResult.Success && attempts >= 3)
                            {
                                Utils.WriteToEventLog("Test: " + test.Test.Name + " failed.");
                                harness.MFTestResult = Harness.Result.Fail;
                                GetTestResultDetails(harness, testList[i]);
                                Console.WriteLine("Test Result: " + harness.MFTestResult);
                            }

                            // Test succeeded with 3 attempts or an abort was sent by harness.
                            if ((hResult == HarnessExecutionResult.Success && attempts < 4) ||
                                (hResult == HarnessExecutionResult.Abort))
                            {
                                GetTestResultDetails(harness, testList[i]);
                                if (!string.IsNullOrEmpty(m_device))
                                {
                                    string deviceStatus = DeviceStatus(hResult);                                    
                                    Utils.WriteToEventLog("Device status after running " + testList[i].Name 
                                        + ": " + deviceStatus);
                                    if (!IsProfilerRun)
                                    {
                                        m_log.AddDeviceStatusToLog("Device ping result after running "
                                            + testList[i].Name + ":  " + deviceStatus);
                                    }
                                    if (string.Equals(deviceStatus.ToLower(), "noconnection"))
                                    {
                                        throw new ApplicationException("Device did not reboot correctly after " +
                                            "running the test: " + testList[i].Name);
                                    }
                                }

                                if (!IsProfilerRun)
                                {
                                    Console.WriteLine("Test Result: " + harness.MFTestResult);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (ex is FileNotFoundException)
                            {
                                if (!IsProfilerRun)
                                {
                                    Console.WriteLine(ex.ToString());
                                    try
                                    {
                                        m_log.AddCommentToLog(ex.ToString());
                                    }
                                    catch
                                    {
                                    }

                                    Utils.WriteToEventLog(
                                        string.Format("Exception in TestSystem.cs: {0}", ex.ToString()));
                                    hResult = HarnessExecutionResult.Abort;
                                }
                            }
                        }

                        // Wait for a few seconds before starting the next test when running on devices.
                        if (!string.Equals(m_transport.ToLower(), "emulator"))
                        {
                            System.Threading.Thread.Sleep(5000);
                        }
                    }
                }

                // Update test results and logs location.
                m_didAllTestsPass = ((tests.FailCount > 0) || (tests.PassCount == 0)) ? false : true;
                UpdateLogFolder();
                return hResult;
            }

            #endregion

            #region Internal Properties

            internal int PassCount
            {
                get
                {
                    return m_passCount;
                }

                set
                {
                    m_passCount = value;
                }
            }

            internal int FailCount
            {
                get
                {
                    return m_failCount;
                }

                set
                {
                    m_failCount = value;
                }
            }

            internal int SkipCount
            {
                get
                {
                    return m_skipCount;
                }

                set
                {
                    m_skipCount = value;
                }
            }

            internal int KnownFailureCount
            {
                get
                {
                    return m_knownFailCount;
                }

                set
                {
                    m_knownFailCount = value;
                }
            }

            internal bool DidAllTestsPass
            {
                get
                {
                    return m_didAllTestsPass;
                }
            }

            internal string EmailList
            {
                get
                {
                    return m_emailList;
                }

                set
                {
                    m_emailList = value;
                }
            }

            internal int TotalTestCases
            {
                get
                {
                    return m_totalTestCases;
                }
            }

            internal string Transport
            {
                get
                {
                    return m_transport;
                }

                set
                {
                    m_transport = value;
                }
            }

            internal string Device
            {
                get
                {
                    return m_device;
                }

                set
                {
                    m_device = value;
                }
            }

            internal string FullDeviceName
            {
                get
                {
                    return m_fullDeviceName;
                }

                set
                {
                    m_fullDeviceName = value;
                }
            }

            internal bool IsProfilerRun
            {
                get
                {
                    return m_isProfilerRun;
                }

                set
                {
                    m_isProfilerRun = value;
                }
            }

            internal bool IsDevEnvironment
            {
                get
                {
                    return m_isDevEnv;
                }

                set
                {
                    m_isDevEnv = value;
                }
            }

            internal static bool IsAlternateLogStoreSpecified
            {
                get
                {
                    if (string.IsNullOrEmpty(m_alternateLogStore))
                    {
                        return false;
                    }
                    return true;
                }
            }

            internal static string ResultsPath
            {
                get
                {
                    if (!string.IsNullOrEmpty(m_alternateLogStore))
                    {
                        return m_alternateLogStore;
                    }
                    else
                    {
                        return m_logStore;
                    }
                }

                set
                {
                    m_alternateLogStore = value;
                    m_isAlternateLogStoreSpecified = true;
                }
            }

            internal static string ProductInstallRoot
            {
                get
                {
                    if (null == m_productInstallRoot)
                    {
                        // Get the test install location.
                        RegistryKey testKey = null;
                        RegistryKey rootKey = Registry.LocalMachine;
                        

                        string ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(2);

                        // Check if we are running on a 64 bit OS.
                        if (IntPtr.Size == 8)
                        {
                            testKey =
                                rootKey.OpenSubKey(@"Software\Wow6432Node\Microsoft\.NETMicroFramework\v" + ver);
                        }

                        if(testKey == null)
                        {
                            testKey = rootKey.OpenSubKey(@"Software\Microsoft\.NETMicroFramework\v" + ver);

                        }

                        if (null != testKey)
                        {
                            m_productInstallRoot = (string)testKey.GetValue("InstallRoot");
                        }
                    }

                    return m_productInstallRoot;
                }
            }

            internal static string InstallRoot
            {
                get
                {
                    if (null == m_installRoot)
                    {
                        string ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(2);

                        // Get the test install location.
                        RegistryKey testKey = Registry.LocalMachine;
                        
                        // Check if we are running on a 64 bit OS.
                        if (IntPtr.Size == 8)
                        {
                            testKey = 
                                testKey.OpenSubKey(@"Software\Wow6432Node\Microsoft\.NETMicroFramework\TestSystem " + ver);
                        }
                        else
                        {
                            testKey = testKey.OpenSubKey(@"Software\Microsoft\.NETMicroFramework\TestSystem " + ver);
                        }
                        m_installRoot = (string)testKey.GetValue("InstallRoot");
                    }

                    return m_installRoot;
                }

                set
                {
                    m_installRoot = value;
                }
            }

            internal static string BuildNumber
            {
                get
                {
                    return m_buildNumber;
                }

                set
                {
                    m_buildNumber = value;
                }
            }

            internal static string Branch
            {
                get
                {
                    return m_branch;
                }

                set
                {
                    m_branch = value;
                }
            }

            internal static string BuildFlavor
            {
                get
                {
                    return m_buildFlavor;
                }

                set
                {
                    m_buildFlavor = value;
                }
            }

            internal bool IsMultipleRun
            {
                get
                {
                    if (m_runCount > 1)
                    {
                        return true;
                    }
                    return false;
                }
            }

            internal static bool IncludesDeviceTest
            {
                get
                {
                    return m_includesDeviceTest;
                }

                set
                {
                    m_includesDeviceTest = value;
                }
            }

            #endregion

            #region Private Methods

            private string DeviceStatus(HarnessExecutionResult prevResult)
            {
                string pingResponse = String.Empty;
                if (null != m_transport)
                {
                    // Continue running the next test if the device ping returns a tinyclr.
                    // Else, reset power to the device and continue.
                    pingResponse = PingDevice(prevResult);                    
                }
                return pingResponse;
            }

            private string PingDevice(HarnessExecutionResult prevResult)
            {
                MFDeploy mfDeploy = new MFDeploy();
                PingConnectionType pct = PingConnectionType.NoConnection;
                bool usbFound = false;
                int retries = 0;
                while (!usbFound && retries++ < 2)
                {
                    foreach (MFPortDefinition pd in mfDeploy.DeviceList)
                    {
                        if (string.Equals(pd.Transport.ToString(),
                            m_transport, StringComparison.InvariantCultureIgnoreCase))
                        {
                            usbFound = true;
                            Utils.WriteToEventLog("Previous result from harness: " + prevResult);
                            int attempts = 0;
                            while ((PingConnectionType.NoConnection == pct ||
                                PingConnectionType.TinyBooter == pct) &&
                                (attempts++ < 5))
                            {
                                try
                                {
                                    using (MFDevice dev = mfDeploy.Connect(pd))
                                    {
                                        pct = dev.Ping();
                                        Utils.WriteToEventLog("MFDevice.Ping() resulted in a " + pct.ToString());
                                        if (PingConnectionType.TinyBooter == pct)
                                        {
                                            // Clear the bootloader flag so that the device gets
                                            // back into tinyclr.
                                            dev.Execute(0);
                                        }
                                        dev.Dispose();
                                    }

                                    if (PingConnectionType.NoConnection == pct)
                                    {
                                        Utils.WriteToEventLog("Attempting to reset power since we have a " +
                                            "NoConnection status from device..");
                                        ResetDevicePower();
                                    }
                                }
                                catch
                                {
                                    Utils.WriteToEventLog("An exception was thrown when attempting a ping. " +
                                        "Reseting power contoller directly...");
                                    MFPowerController.Reset();
                                    System.Threading.Thread.Sleep(5000);
                                }

                                System.Threading.Thread.Sleep(1000);
                            }
                            break;
                        }                       
                    }
                    
                    if (!usbFound)
                    {
                        Utils.WriteToEventLog("USB not found after the attempt#: " + retries);
                        MFPowerController.Reset();
                        System.Threading.Thread.Sleep(5000);
                    }
                }
                
                return pct.ToString();
            }

            private void ResetDevicePower()
            {
                try
                {
                    MFPowerController.Reset();
                    RebootDevice(Engine.RebootOption.RebootClrWaitForDebugger);
                }
                catch
                {
                }
            }

            private bool RebootDevice(Engine.RebootOption option)
            {
                PortFilter[] args = { };
                switch (this.Transport.ToLower())
                {
                    case "emulator":
                        args = new PortFilter[] { PortFilter.Emulator };
                        break;
                    case "serial":
                        args = new PortFilter[] { PortFilter.Serial };
                        break;
                    case "tcpip":
                        args = new PortFilter[] { PortFilter.TcpIp };
                        break;
                    case "usb":
                        args = new PortFilter[] { PortFilter.Usb };
                        break;
                }

                ArrayList list = PortDefinition.Enumerate(args);
                PortDefinition port = null;
                foreach (object prt in list)
                {
                    port = (PortDefinition)prt;
                    if (port.DisplayName.ToLower().Contains(this.Device.ToLower()))
                    {
                        break;
                    }
                    else
                    {
                        port = null;
                    }
                }

                if (null == port)
                {
                    return false;
                }

                using (Engine engine = new Engine(port))
                {
                    engine.Start();
                    bool connect = false;

                    connect = engine.TryToConnect(200, 500, true, ConnectionSource.TinyCLR);

                    if (!connect)
                    {
                        return false;
                    }
                    engine.RebootDevice(option);
                }

                return true;
            }

            private void TerminateRunningEmulators(TimedTest test, BaseTest bTest)
            {
                if (string.Equals(m_transport.ToLower(), "emulator"))
                {
                    try
                    {
                        test.KillEmulator();
                    }
                    catch (Exception ex)
                    {
                        Utils.WriteToEventLog("An exception was thrown when killing the emulator "
                            + "before executing " + bTest.Name + " : " + ex.ToString());
                    }
                }

                string[] onboardFlashes = new string[] { 
                Path.Combine(Path.GetDirectoryName(bTest.Location)   , "OnBoardFlash.dat"), 
                Path.Combine(Directory.GetCurrentDirectory()         , "OnBoardFlash.dat") };

                foreach (string onboardFlash in onboardFlashes)
                {
                    if (File.Exists(onboardFlash))
                    {
                        try
                        {
                            File.Delete(onboardFlash);
                        }
                        catch
                        {
                        }
                    }
                }
            }

            private void SetEnvironment()
            {
                string spoClient = string.Empty;

                // Determine if you are running on a dev enlistment.
                try
                {
                    spoClient = Environment.GetEnvironmentVariable("SPOCLIENT");
                    if (File.Exists(spoClient + @"\test\platform\build.dirproj"))
                    {
                        InstallRoot = spoClient + @"\test\platform\";
                        this.IsDevEnvironment = true;
                    }
                }
                catch
                {
                }
            }

            private void UpdateLogFolder()
            {
                // Since the results file is created in the specified dir,
                // move all the xml log files to that directory.
                if (m_isAlternateLogStoreSpecified)
                {
                    foreach (string file in Directory.GetFiles(m_logStore + "\\..\\"))
                    {
                        if (file.EndsWith(".xml"))
                        {
                            File.Move(file, m_logStore + "\\" + file.Substring(file.LastIndexOf("\\") + 1));
                        }
                    }
                }

                // Save the log files to a unc path if the resultsfolder flag was set.
                if (!string.IsNullOrEmpty(m_alternateLogStore))
                {
                    // Copy the contents of the results folder to the shared folder.
                    try
                    {
                        string profilerRun = string.Empty;
                        if (m_isProfilerRun)
                        {
                            profilerRun = "_ProfilerRun";
                        }

                        m_alternateLogStore = m_alternateLogStore.TrimEnd('\\') + "\\" + m_branch + "_TestResults_" + 
                            m_device + "_" + m_buildFlavor + "_" + Environment.MachineName + profilerRun + "\\";

                        if (Directory.Exists(m_alternateLogStore))
                        {
                            Directory.Delete(m_alternateLogStore, true);
                        }

                        Utils.WriteToEventLog(
                            string.Format("TestSystem: Creating the new directory: {0}", m_alternateLogStore));
                        Directory.CreateDirectory(m_alternateLogStore);

                        foreach (string dir in Directory.GetDirectories(m_logStore))
                        {
                            string dirName = dir.Substring(dir.LastIndexOf("\\") + 1);
                            Utils.WriteToEventLog(
                                string.Format("TestSystem: Creating new directory at {0}", m_alternateLogStore + dirName));
                            Directory.CreateDirectory(m_alternateLogStore + dirName);
                            foreach (string file in Directory.GetFiles(m_logStore + "\\" + dirName))
                            {
                                File.Copy(file, m_alternateLogStore + dirName + "\\" +
                                    file.Substring(file.LastIndexOf("\\") + 1));
                            }
                        }
                        
                        // Copy the images and results.xsl this new directory.
                        foreach (string file in Directory.GetFiles(m_logStore))
                        {
                            File.Copy(file, m_alternateLogStore + "\\" + file.Substring(file.LastIndexOf("\\") + 1));
                        }                        
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Warning: TestSystem - An exception happened in UpdateLogFolder: " + ex.ToString());
                        Utils.WriteToEventLog(
                            string.Format("TestSystem: An exception happened in UpdateLogFolder: {0}", ex.ToString()));
                        
                        // Set the shared path to null.
                        m_alternateLogStore = null;
                    }
                }
            }

            private void SetResultPaths()
            {
                // Set default log stores.
                if (IsDevEnvironment)
                {
                    m_logStore = InstallRoot + "Tools\\MFTestSystem\\Results\\";
                }
                else
                {
                    m_logStore = InstallRoot + "Results\\" + m_testResultId + "\\";

                    // Create a log directory if one does not exist and copy the images and xsl files.
                    if (!Directory.Exists(m_logStore))
                    {
                        Directory.CreateDirectory(m_logStore);
                        File.Copy(InstallRoot + "Results\\results.xsl", m_logStore + "\\results.xsl");
                        Directory.CreateDirectory(m_logStore + "\\images\\");
                        foreach (string imgFile in Directory.GetFiles(InstallRoot + "Results\\images\\"))
                        {
                            File.Copy(imgFile, m_logStore + "\\images\\" + imgFile.Substring(imgFile.LastIndexOf("\\") + 1));
                        }
                    }
                }

                // If an alternate log store is specified, we need to copy the logs to that location
                // at the completion of the test run.
                if (m_isAlternateLogStoreSpecified)
                {
                    // Check if the alternate store exists.
                    if (!Directory.Exists(m_alternateLogStore))
                    {
                        throw new ApplicationException("Unable to find the specified share/folder: " + m_alternateLogStore);
                    }

                    m_alternateLogStore = string.Format("{0}{1}{2}", 
                        m_alternateLogStore.TrimEnd('\\'), @"\", m_testResultId.Replace(" ","_"));

                    // Create the store directory if it does not exist already.
                    if (!Directory.Exists(m_alternateLogStore))
                    {
                        Directory.CreateDirectory(m_alternateLogStore);
                    }
                }
            }

            private void GetTestResultDetails(Harness harness, BaseTest currentTest)
            {
                // Get the number of test methods in the log file for the test.
                // Open the xml file and get a count of number of <TestMethod> sections.                        
                int totalTestMethods = 0;
                XmlDocument doc = new XmlDocument();
                Harness.Result mfResult = Harness.Result.NotKnown;

                if (!m_isProfilerRun)
                {
                    try
                    {
                        doc.Load(m_logStore + harness.LogFile);
                        XmlNodeList testLogNodes = doc.SelectNodes("/SPOT_Platform_Test/TestLog");
                        foreach (XmlNode tlNode in testLogNodes)
                        {
                            XmlNodeList testMethodNodes = tlNode.SelectNodes("TestMethod");
                            totalTestMethods += testMethodNodes.Count;
                        }
                    }
                    catch (XmlException)
                    {
                        harness.TestResults = Harness.Result.Fail;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("WARNING: " + ex.ToString());
                    }

                    try
                    {
                        mfResult = harness.MFTestResult;
                    }
                    catch
                    {
                        mfResult = Harness.Result.Fail;
                    }
                }
             
                UpdateTestDetails(harness, currentTest, mfResult, totalTestMethods, doc);

                // Add to totalTestCases count.
                // This will give you the number of test cases executed during the entire run in all the tests.
                m_totalTestCases += totalTestMethods;
            }

            private void UpdateTestDetails(Harness harness, BaseTest currentTest, 
                Harness.Result mfResult, int totalTestMethods, XmlDocument doc)
            {
                string[] seperator = { "::" };
                try
                {
                    currentTest.EndTime = harness.EndTime;
                    currentTest.LogFile = harness.LogFile;
                    currentTest.Result = mfResult.ToString();
                    currentTest.StartTime = harness.StartTime;

                    if (m_isProfilerRun)
                    {
                        ArrayList methodList = Utils.ReadProfilerLogFile(currentTest.LogFile, currentTest.ExeLocation);

                        for (int i = 0; i < methodList.Count; i++)
                        {
                            string[] vals = methodList[i].ToString().Split('\t');

                            int exclTime = 0;
                            if (!string.IsNullOrEmpty(vals[3]))
                            {
                                exclTime = Convert.ToInt32(vals[3].Trim());
                            }

                            int inclTime = 0;
                            if (!string.IsNullOrEmpty(vals[4]))
                            {
                                inclTime = Convert.ToInt32(vals[4].Trim());
                            }

                            ProfilerTestMethod ptm = new ProfilerTestMethod();
                            ptm.TestMethod = vals[2].Split(seperator, StringSplitOptions.None)[1].Trim();                            
                            ptm.InclusiveTime = inclTime;
                            ptm.ExclusiveTime = exclTime;

                            currentTest.TestMethods.Add(ptm);
                        }
                    }
                    else
                    {
                        // Update specific results count.
                        switch (mfResult)
                        {
                            case Harness.Result.Pass:
                                m_passCount++;
                                break;

                            case Harness.Result.Fail:
                                m_failCount++;
                                break;

                            case Harness.Result.Skip:
                                m_skipCount++;
                                break;

                            case Harness.Result.KnownFailure:
                                m_knownFailCount++;
                                break;
                        }

                        int pc = 0, fc = 0, sc = 0, kc = 0;
                        try
                        {
                            // Get total passcount, failcount, skipcount and known failure count for the test
                            XmlNodeList passNodes = doc.GetElementsByTagName("PassCount");
                            foreach (XmlNode passNode in passNodes)
                            {
                                pc += Convert.ToInt32(passNode.ChildNodes[0].InnerText);
                            }

                            XmlNodeList failNodes = doc.GetElementsByTagName("FailCount");
                            foreach (XmlNode failNode in failNodes)
                            {
                                fc += Convert.ToInt32(failNode.ChildNodes[0].InnerText);
                            }

                            XmlNodeList skipNodes = doc.GetElementsByTagName("SkipCount");
                            foreach (XmlNode skipNode in skipNodes)
                            {
                                sc += Convert.ToInt32(skipNode.ChildNodes[0].InnerText);
                            }

                            XmlNodeList knownFailNodes = doc.GetElementsByTagName("KnownFailureCount");
                            foreach (XmlNode knownFailNode in knownFailNodes)
                            {
                                kc += Convert.ToInt32(knownFailNode.ChildNodes[0].InnerText);
                            }
                        }
                        catch
                        {
                        }

                        currentTest.TestMethodFailCount = fc;
                        currentTest.TestMethodKnownFailureCount = kc;
                        currentTest.TestMethodPassCount = pc;
                        currentTest.TestMethodSkipCount = sc;
                        currentTest.TotalTestCases = totalTestMethods;
                    }

                    m_mfTestList.Add(currentTest);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("WARNING: " + ex.ToString());
                }
            }

            private bool ParseArguments(string[] args, ref bool runTestsIndividually)
            {
                int pos = 0;
                while (pos < args.Length)
                {
                    switch (args[pos].ToLower())
                    {
                        case "-resultsfolder":
                            if (!IsArgumentValid("\nERROR: No folder specified with the -resultsfolder switch",
                                args, pos))
                            {
                                return false;
                            }

                            m_isAlternateLogStoreSpecified = true;
                            m_alternateLogStore = args[pos + 1];

                            if (!Path.IsPathRooted(m_alternateLogStore))
                            {
                                m_staticList = Path.Combine(Environment.CurrentDirectory, m_alternateLogStore);
                            }

                            break;

                        case "-transport":
                            if (!IsArgumentValid("\nERROR: No transport specified with the -transport switch",
                                args, pos))
                            {
                                return false;
                            }

                            m_transport = args[pos + 1];

                            break;

                        case "-device":
                            if (!IsArgumentValid("\nERROR: No device specified with the -device switch",
                                args, pos))
                            {
                                return false;
                            }

                            m_device = args[pos + 1];

                            break;

                        case "-mailto":
                            if (!IsArgumentValid("\nERROR: No email address specified with the -mailto switch",
                                args, pos))
                            {
                                return false;
                            }

                            m_emailList = args[pos + 1];

                            break;

                        case "-test":
                            if (!IsArgumentValid("\nERROR: No test specified with the -test switch",
                                args, pos))
                            {
                                return false;
                            }

                            if (!args[pos + 1].EndsWith(".sln", StringComparison.InvariantCultureIgnoreCase))
                            {
                                Console.WriteLine("\nERROR: Please specify a .sln file with the -test switch.");
                                return false;
                            }
                            
                            runTestsIndividually = true;
                            m_test = args[pos + 1];

                            if(!Path.IsPathRooted(m_test))
                            {
                                m_test = Path.Combine(Environment.CurrentDirectory, m_test);
                            }

                            break;

                        case "-testlist":
                            if (!IsArgumentValid("\nERROR: No file specified with the -testlist switch",
                                args, pos))
                            {
                                return false;
                            }
                            m_isStaticList = true;
                            m_staticList = args[pos + 1];

                            if(!Path.IsPathRooted(m_staticList))
                            {
                                m_staticList = Path.Combine(Environment.CurrentDirectory, m_staticList);
                            }
                            Console.WriteLine("testlist: **** " + m_staticList);
                            break;

                        case "-profile":
                            m_isProfilerRun = true;
                            pos--;

                            break;

                        case "-x":
                            if (!IsArgumentValid("\nERROR: No count specified with the -x option.",
                                args, pos))
                            {
                                return false;
                            }

                            m_runCount = Convert.ToInt32(args[pos + 1]);
                            break;

                        default:
                            Console.WriteLine("\nERROR: Invalid Argument: " + args[pos]);
                            DisplayHelp();
                            return false;
                    }
                    
                    // Increment the counter twice.
                    pos = pos + 2;
                }

                // If transport is specified without device or vice versa, display help.
                if ((!string.IsNullOrEmpty(m_transport) && string.IsNullOrEmpty(m_device)) ||
                    (string.IsNullOrEmpty(m_transport) && !string.IsNullOrEmpty(m_device)))
                {
                    DisplayHelp();
                    return false;
                }

                return true;
            }

            private bool IsArgumentValid(string mesg, string[] args, int pos)
            {
                if ((args.Length <= pos + 1) || (args[pos + 1].StartsWith("-")))
                {
                    Console.WriteLine(mesg);
                    DisplayHelp();
                    return false;
                }

                return true;
            }

            private BaseTest[] BuildTestList(bool runTestsIndividually)
            {
                DirectoryInfo[] dirs;
                BaseTest[] list;

                if (runTestsIndividually)
                {
                    list = new BaseTest[m_runCount];

                    for (int i = 0; i < m_runCount; i++)
                    {
                        if (m_isProfilerRun)
                        {
                            list[i] = new ProfilerTest();
                        }
                        else
                        {
                            list[i] = new MicroFrameworkTest();
                        }
                        list[i].Name = m_test;
                        list[i].Location = m_test;
                    }
                }
                else
                {
                    // The machine is a test box with the sdk installation and not a dev box.
                    if (!IsDevEnvironment)
                    {
                        if (m_isProfilerRun)
                        {
                            dirs = new DirectoryInfo(string.Format("{0}ManagedProfilerTests",
                                InstallRoot)).GetDirectories();
                        }
                        else
                        {
                            dirs = new DirectoryInfo(string.Format("{0}TestCases",
                                InstallRoot)).GetDirectories();
                        }

                        list = BuildSolutionFileList(dirs);
                    }
                    else
                    {
                        // The machine is a dev box.
                        if (m_isProfilerRun)
                        {
                            dirs = new DirectoryInfo(string.Format(@"{0}Tests\Performance\ProfilerTests",
                                InstallRoot)).GetDirectories();
                            list = BuildSolutionFileList(dirs);
                        }
                        else
                        {
                            list = BuildSolutionFileList(InstallRoot + "build.dirproj");
                        }
                    }
                }

                return list;
            }

            private BaseTest[] BuildSolutionFileList(DirectoryInfo[] dirs)
            {
                int index = 0;
                BaseTest[] list = new BaseTest[dirs.Length * m_runCount];

                for (int i = 0; i < m_runCount; i++)
                {
                    foreach (DirectoryInfo dir in dirs)
                    {
                        // The installed location will have only one sln file per test.
                        FileInfo[] slnFiles = dir.GetFiles("*.sln");
                        if (slnFiles.Length > 0)
                        {
                            string slnPath = dir.Name + "\\" + slnFiles[0].Name;
                            if (!string.IsNullOrEmpty(slnPath))
                            {
                                // Add only the tests specified in the testlist if one is supplied.
                                if (!m_isStaticList)
                                {
                                    AddTest(ref list, ref index, slnPath);
                                }
                                else
                                {
                                    List<string> staticList = new List<string>();
                                    foreach (string file in File.ReadAllLines(m_staticList))
                                    {
                                        if (string.Equals(slnFiles[0].Name, Path.GetFileName(file),
                                            StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            AddTest(ref list, ref index, slnPath);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return list;
            }

            private void AddTest(ref BaseTest[] list, ref int index, string slnPath)
            {
                if (m_isProfilerRun)
                {
                    list[index] = new ProfilerTest();
                }
                else
                {
                    list[index] = new MicroFrameworkTest();
                }

                list[index].Name = slnPath;
                list[index++].Location = slnPath;
            }

            private BaseTest[] BuildSolutionFileList(string dirProjPath)
            {
                // Scan build.dirproj to generate the test list.                                               
                //List<string> staticList = new List<string>();
                ArrayList test_list = new ArrayList();
                if (m_isStaticList)
                {
                    foreach (string file in File.ReadAllLines(m_staticList))
                    {
                        //staticList.Add(file.ToLower());
                        test_list.Add(file.ToLower());
                    }
                }
                else
                {
                    test_list = BuildTestList(dirProjPath, new ArrayList());

                    // Add the DPWS fixture tests which are built by the build system to the test list.
                    string dpwsFixtureTestPath = "";

                    string flav = Environment.GetEnvironmentVariable("FLAVOR");
                    
                    if (string.IsNullOrEmpty(flav))
                    {
                        flav = "Release";
                    }

                    dpwsFixtureTestPath = string.Format(@"{0}\BuildOutput\public\{1}\test\server\dpws",
                        Environment.GetEnvironmentVariable("SPOCLIENT"),
                        flav);

                    if (!Directory.Exists(dpwsFixtureTestPath))
                    {
                        if (flav.ToLower() == "debug") flav = "Release";
                        else flav = "Debug";

                        dpwsFixtureTestPath = string.Format(@"{0}\BuildOutput\public\{1}\test\server\dpws",
                            Environment.GetEnvironmentVariable("SPOCLIENT"),
                            flav);
                    }

                    if (Directory.Exists(dpwsFixtureTestPath))
                    {
                        FileInfo[] dpwsSlnFiles = new DirectoryInfo(dpwsFixtureTestPath).GetFiles("*.sln");
                        foreach (FileInfo dpwsSlnFile in dpwsSlnFiles)
                        {
                            test_list.Add(dpwsSlnFile.FullName);
                        }
                    }
                }

                // Copy the contents of the local arraylist to the string array.
                BaseTest[] list = new BaseTest[test_list.Count * m_runCount];
                for (int rc = 0; rc < m_runCount; rc++)
                {
                    for (int i = 0; i < list.Length; i++)
                    {
                        //if (m_isStaticList)
                        //{
                        //    string test = Path.GetFileName(test_list[i].ToString());
                        //    if (!staticList.Contains(test))
                        //        continue;
                        //}
                        if (m_isProfilerRun)
                        {
                            list[i] = new ProfilerTest();
                        }
                        else
                        {
                            list[i] = new MicroFrameworkTest();
                        }
                        list[i].Name = test_list[i].ToString();
                        list[i].Location = test_list[i].ToString();
                    }
                }

                return list;
            }

            private ArrayList BuildTestList(string dirProjPath, ArrayList test_list)
            {
                XmlTextReader dirprojReader = new XmlTextReader(dirProjPath); 
                while (dirprojReader.Read())
                {
                    // Look for the <project> node that contains the path for the 
                    // slnproj files in its attributes and replace "slnproj" with 
                    // "sln" to get the path for the sln files.
                    switch (dirprojReader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (string.Equals("project", dirprojReader.Name,
                                StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (dirprojReader.HasAttributes)
                                {
                                    string attr = dirprojReader.GetAttribute(0).ToLower();
                                    if (!attr.StartsWith(@"tests\performance",
                                            StringComparison.InvariantCultureIgnoreCase)
                                            && !attr.StartsWith(@"desktop",
                                            StringComparison.InvariantCultureIgnoreCase)
                                            && !attr.StartsWith(@"tools",
                                            StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        if (attr.EndsWith("build.dirproj",
                                            StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            test_list = BuildTestList(InstallRoot + attr, test_list);
                                        }
                                        else if (attr.EndsWith(@".slnproj",
                                            StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            string treeRoot = Path.GetDirectoryName(dirProjPath);                                            
                                            test_list.Add(treeRoot + @"\" + 
                                                    attr.Replace("slnproj", "sln"));
                                        }
                                    }
                                }
                            }
                            break;
                    }                    
                }
                return test_list;
            }

            #endregion
        }

        #endregion

        #region Private

        private void PerformPostRunTasks()
        {
            if (!tests.IsProfilerRun)
            {
                // Create the log file.
                try
                {
                    Utils.WriteToEventLog("Creating the test log at " + RunMFTests.ResultsPath);
                    Utils.CreateTestLog(RunMFTests.ResultsPath, m_testSystem);
                }
                catch (Exception ex)
                {
                    Utils.WriteToEventLog(
                        string.Format("TestSystem: An exception was thrown when creating the test log: {0}",ex.ToString()));
                }

                // Display the log file if specified by the caller.
                if (m_displayLog)
                {
                    Process.Start(RunMFTests.ResultsPath + @"\" + m_resultsFile);
                }

                if (!string.IsNullOrEmpty(m_mailTo))
                {
                    Utils.WriteToEventLog("Sending out automation results...");
                    SendMail();
                }
            }
            else
            {
                try
                {
                    Utils.CreateProfilerLog(RunMFTests.ResultsPath, m_testSystem);
                }
                catch (Exception ex)
                {
                    Utils.WriteToEventLog(
                        string.Format("TestSystem: An exception was thrown when creating the profiler test log: {0}",ex.ToString()));
                }
            }
        }

        private void SendMail()
        {
                string emailFrom = "mfalaba@microsoft.com";

                // Send results email                
                SendMail mail = new SendMail();
                mail.To = m_mailTo;
                if (!string.IsNullOrEmpty(m_buildNumber) && !string.IsNullOrEmpty(this.Branch)
                    && !string.IsNullOrEmpty(this.BuildFlavor))
                {
                    mail.Subject = "Test Results Rollup: " + this.BuildFlavor + " Build - "
                        + m_buildNumber + ", Branch - " + this.Branch;
                }
                else
                {
                    mail.Subject = "Test Results Rollup";
                }
                mail.From = emailFrom;
                mail.IsBodyHtml = true;
                StreamReader rd = new StreamReader(RunMFTests.ResultsPath + @"\" + m_resultsFile);
                mail.Body = rd.ReadToEnd();
                rd.Close();
                Console.WriteLine("Sending results email to: " + m_mailTo);
                Utils.WriteToEventLog(string.Format("TestSystem: Sending the results email to: {0}", m_mailTo));
                mail.Execute();
        }        

        private static void DisplayHelp()
        {
            Console.WriteLine("\r\n\r\nUSAGE:");
            Console.WriteLine("\t " + @"RunTests.exe");
            Console.WriteLine("\t " + @"RunTests.exe -test TestFolder\file.sln");
            Console.WriteLine("\t " + @"RunTests.exe -testlist <Path to File that contains sln list>");
            Console.WriteLine("\t " + @"RunTests.exe -test TestFolder\file.sln -x 5");
            Console.WriteLine("\t " + @"RunTests.exe -resultsfolder <FolderName>");
            Console.WriteLine("\t " + @"RunTests.exe -resultsfolder \\UNC\MyTestResultsFolder\");         
            Console.WriteLine("\t " + @"RunTests.exe -mailto user1@microsoft.com,user2@microsoft.com");
            Console.WriteLine("\t " + @"RunTests.exe -transport [USB | Emulator | Serial | TCPIP]");
            Console.WriteLine("\t " + @"RunTests.exe -transport [USB | Emulator | Serial | TCPIP] " 
                + @"-device [a7e70ea2 | Microsoft | COM1 | 157.56.166.67]");
            Console.WriteLine("\t " + @"RunTests.exe -transport USB -device a7e70ea2 -profile");
        }

        #endregion
    }
}
