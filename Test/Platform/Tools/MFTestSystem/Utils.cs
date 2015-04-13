using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using Microsoft.Win32;
using System.IO;
using System.Collections;
using System.Management;
using Microsoft.SPOT.Debugger;
using Microsoft.SPOT.Debugger.WireProtocol;
using Microsoft.NetMicroFramework.Tools.MFProfilerTool;
using Debugging_Resolve_Assembly = Microsoft.SPOT.Debugger.WireProtocol.Commands.Debugging_Resolve_Assembly;

namespace Microsoft.SPOT.Platform.Test
{
    class Utils
    {
        private static bool m_isDevEnvironment = false;

        static string FindBuildRelativePath(string file)
        {
            if (m_isDevEnvironment)
            {
                string flav = Environment.GetEnvironmentVariable("FLAVOR");

                if (string.IsNullOrEmpty(flav))
                {
                    flav = "Release";
                }

                string devEnvFile =
                Environment.GetEnvironmentVariable("SPOCLIENT")
                                + @"\buildoutput\public\" + flav
                                + @"\client\" + file;

                if (!File.Exists(devEnvFile))
                {
                    flav = (flav.ToLower() == "release" ? "Debug" : "Release");

                    devEnvFile =
                        Environment.GetEnvironmentVariable("SPOCLIENT")
                                + @"\buildoutput\public\" + flav
                                + @"\client\" + file;
                }

                return devEnvFile;
            }
            else
            {
                return Path.Combine(TestSystem.RunMFTests.ProductInstallRoot, file);
            }
        }

        private struct OsInfo
        {
            public string OS;
            public string OSLocale;
        }

        internal static string VisualStudioSkew = "Visual Studio 2010";
        internal static void WriteToEventLog(string entry)
        {
            string log = ".NET MF Build Tool";
            string source = "TestSystem";

            try
            {
                EventLog evtLog = new EventLog(log);
                evtLog.Source = source;

                try
                {
                    evtLog.WriteEntry(entry);
                }
                catch (System.Security.SecurityException)
                {
                    // Failed to write to log, check to see if source key exists
                    string keyName = String.Format(@"SYSTEM\CurrentControlSet\Services\EventLog\{0}\{1}",
                        log, source);

                    RegistryKey rkEventSource = Registry.LocalMachine.OpenSubKey(keyName);

                    if (rkEventSource == null)
                    {
                        //key doesn't exist, create key using elevated process
                        Process proc = new Process();
                        ProcessStartInfo procStartInfo = new ProcessStartInfo("Reg.exe");
                        procStartInfo.Arguments = @"add ""HKLM\" + keyName + "\"";
                        procStartInfo.UseShellExecute = true;
                        procStartInfo.Verb = "runas";
                        proc.StartInfo = procStartInfo;
                        proc.Start();
                        proc.WaitForExit();
                    }
                    evtLog.WriteEntry(entry);
                }
            }
            catch
            {
            }
        }

        internal static void DeployToDevice(string buildPath, ArrayList referenceList, 
            Engine engine, string transport, bool isDevEnvironment, string assemblyName)
        {
            ArrayList peList = GetPeFileList(buildPath, referenceList, isDevEnvironment, assemblyName, engine);

            // Deploy the pe files to the device.
            Console.Write("\tDeploying the device test..");
            bool isDeployed = engine.Deployment_Execute(peList, false, null);

            if (string.Equals(transport.ToLower(), "emulator"))
            {
                isDeployed = true;
            }

            if (isDeployed)
            {
                Console.WriteLine(" deployed successfully");
            }
            else
            {
                Console.WriteLine(" deployment failure");
                throw new ApplicationException("Unable to deploy the test to the device");
            }
        }

        private static ArrayList GetPeFileList(string buildPath, ArrayList referenceList, 
            bool isDevEnvironment, string assemblyName, Engine engine)
        {
            m_isDevEnvironment = isDevEnvironment;
            Hashtable systemAssemblies = new Hashtable();
            foreach (Debugging_Resolve_Assembly resolvedAssembly in engine.ResolveAllAssemblies())
            {
                if ((resolvedAssembly.m_reply.m_flags & Debugging_Resolve_Assembly.Reply.c_Deployed) == 0)
                {
                    systemAssemblies[resolvedAssembly.m_reply.Name] = true;
                }
            }

            // Build an array list of the pe files that this test needs in order to execute.
            // This would include the pe files for all the references and the test itself.            
            ArrayList peList = new ArrayList();
            for (int i = 0; i < referenceList.Count; i++)
            {
                string peFile = referenceList[i] + ".pe";
                string pePath = string.Empty;

                // Look for the pe file in the Build Path.
                if (isDevEnvironment)
                {
                    if (engine.IsTargetBigEndian)
                    {
                        pePath = buildPath + @"\..\pe\be\" + peFile;
                    }
                    else
                    {
                        pePath = buildPath + @"\..\pe\le\" + peFile;
                    }
                }
                else
                {
                    if (engine.IsTargetBigEndian)
                    {
                        pePath = buildPath + @"\be\" + peFile;
                    }
                    else
                    {
                        pePath = buildPath + @"\le\" + peFile;
                    }
                }

                if (!File.Exists(pePath))
                {
                    // Look for the pe file in the MF Tools Path.
                    if (isDevEnvironment)
                    {
                        if (engine.IsTargetBigEndian)
                        {
                            pePath = FindBuildRelativePath(@"pe\be\" + peFile);
                        }
                        else
                        {
                            pePath = FindBuildRelativePath(@"pe\le\" + peFile);
                        }
                    }
                    else
                    {
                        pePath = FindBuildRelativePath( @"Tools\" + peFile );
                    }

                    if (!File.Exists(pePath))
                    {
                        // Look for the pe files in the MF Assemblies Path (used only on test machines.)
                        if (isDevEnvironment)
                        {
                            throw new FileNotFoundException("Unable to locate " + peFile);
                        }
                        else
                        {
                            if (engine.IsTargetBigEndian)
                            {
                                pePath = FindBuildRelativePath(@"Assemblies\be\" + peFile);
                            }
                            else
                            {
                                pePath = FindBuildRelativePath(@"Assemblies\le\" + peFile);
                            }

                            if (!File.Exists(pePath))
                            {
                                throw new FileNotFoundException("Unable to locate " + peFile);
                            }
                        }
                    }
                }

                pePath = pePath.Replace("\\\\", "\\");
                
                if (!systemAssemblies.ContainsKey(peFile.Replace(".pe", "")))
                {
                    peList.Add(GetAssemblyBinary(pePath));
                }
            }

            // Finally add the pe file for the current test to the array list.
            if (isDevEnvironment)
            {
                if (engine.IsTargetBigEndian)
                {
                    peList.Add(GetAssemblyBinary(buildPath + @"\..\pe\be\" + assemblyName + ".pe"));
                }
                else
                {
                    peList.Add(GetAssemblyBinary(buildPath + @"\..\pe\le\" + assemblyName + ".pe"));
                }

            }
            else
            {
                if (engine.IsTargetBigEndian)
                {
                    peList.Add(GetAssemblyBinary(buildPath + @"\be\" + assemblyName + ".pe"));
                }
                else
                {
                    peList.Add(GetAssemblyBinary(buildPath + @"\le\" + assemblyName + ".pe"));
                }
            }

            return peList;
        }

        private static byte[] GetAssemblyBinary(string assemblyPath)
        {
            using (FileStream fs = File.Open(assemblyPath, FileMode.Open, FileAccess.Read))
            {
                long length = (fs.Length + 3) / 4 * 4;
                byte[] buf = new byte[length];

                fs.Read(buf, 0, (int)fs.Length);
                return buf;
            }
        }

        private static Assembly LoadReflectedAssembly(Object sender, ResolveEventArgs args)
        {
            
            string[] tokens = args.Name.Split(',');
            if (tokens.Length > 0)
            {
                switch (tokens[0])
                {
                    case "System.IO":
                        if (m_isDevEnvironment)
                        {
                            return Assembly.ReflectionOnlyLoadFrom(
                                FindBuildRelativePath(@"dll\System.IO.dll"));
                        }
                        else
                        {
                            return Assembly.ReflectionOnlyLoadFrom(
                                FindBuildRelativePath(@"Assemblies\System.IO.dll"));
                        }     
                }
            }
            return null;
        }

        private static ArrayList GetProfilerTestPublicMethods(string exePath)
        {
            ArrayList list = new ArrayList();
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve +=
                new ResolveEventHandler(LoadReflectedAssembly);
            Assembly asm = Assembly.ReflectionOnlyLoadFrom(exePath);
            foreach (Type t in asm.GetModules()[0].GetTypes())
            {
                if (t.IsPublic && t.Name.StartsWith("Master_"))
                {
                    MethodInfo[] methods = t.GetMethods();
                    foreach (MethodInfo info in methods)
                    {
                        if (info.IsPublic && string.Equals(info.ReturnType.FullName.ToLower(), "system.void") &&
                            !string.Equals(info.Name.ToLower(), "main"))
                        {
                            list.Add(info.Name);
                        }
                    }
                }
            }

            return list;
        }

        internal static ArrayList ReadProfilerLogFile(string logFilePath, string exePath)
        {
            ArrayList publicMethodList = GetProfilerTestPublicMethods(exePath);
            ArrayList methodList = new ArrayList();
            string line = null;
            string[] sep = { "::" };
            StreamReader reader = File.OpenText(logFilePath);
            while ((line = reader.ReadLine()) != null)
            {
                //Microsoft.SPOT.Platform.Tests
                if ((line.Contains("Microsoft.SPOT.Platform.Tests")) &&
                    (!line.Contains(".ctor")) && (!line.Contains(".cctor")) && 
                    (!line.Contains("Main")))
                {
                    if (publicMethodList.Contains(
                            (line.Split('\t')[2]).Split(sep, StringSplitOptions.None)[1]))
                    {
                        methodList.Add(line);
                    }
                }
            }

            if (null != reader)
            {
                reader.Close();
            }

            return methodList;
        }

        internal static PortDefinition GetPort(string device, string transport, string exePath)
        {
            PortFilter[] args = { };
            switch (transport.ToLower())
            {
                case "emulator":
                    args = new PortFilter[] { PortFilter.Emulator };
                    device = "emulator";
                    PortDefinition pd = 
                        PortDefinition.CreateInstanceForEmulator("Launch 'Microsoft Emulator'", "Microsoft", 0);

                    PlatformInfo pi = new PlatformInfo(null);
                    PlatformInfo.Emulator emu = pi.FindEmulator(pd.Port);

                    if (emu != null)
                    {
                        string onboardFlash = Path.Combine(Path.GetDirectoryName(emu.application), "OnBoardFlash.dat");

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


                    PortDefinition_Emulator emuport = pd as PortDefinition_Emulator;
                    Console.WriteLine("\tLaunching Emulator..");
                    Process emuProc = EmulatorLauncher.LaunchEmulator(
                        emuport,
                        true, exePath);
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

            ArrayList list = new ArrayList();
            int nRetries = 20;

            while (list.Count == 0 && nRetries-- > 0)
            {
                list = PortDefinition.Enumerate(args);

                if (list.Count > 0) break;

                System.Threading.Thread.Sleep(500);
            }

            PortDefinition port = null;

            foreach (object prt in list)
            {
                port = (PortDefinition)prt;
                if (port.DisplayName.ToLower().Contains(device.ToLower()))
                {
                    break;
                }
                else
                {
                    port = null;
                }
            }

            if (null != port)
            {
                return port;
            }
            else
            {
                throw new ApplicationException(
                    "Could not find the specified device for the harness to connect to.");
            }
        }

        #region CreateTestLog

        internal static void CreateProfilerLog(string path, TestSystem ts)
        {
            StreamWriter w = WriteTestLogHeaders(path, ts, "iMXS Freescale Profiler Run Results");
            w.WriteLine("</table>");
            w.WriteLine("<br>");

            w.WriteLine("<table border=\"3\" bordercolor=\"black\" width=\"100%\" align=\"left\">");

            w.WriteLine("<td align=\"center\" bordercolor=\"black\" width = \"30%\">");
            w.WriteLine("<font size=\"2\">");
            w.WriteLine("<b>Test</b>");
            w.WriteLine("</font>");
            w.WriteLine("</td>");

            w.WriteLine("<td align=\"center\" bordercolor=\"black\" width = \"40%\">");
            w.WriteLine("<font size=\"2\">");
            w.WriteLine("<b>Location</b>");
            w.WriteLine("</font>");
            w.WriteLine("</td>");

            w.WriteLine("<td align=\"center\" bordercolor=\"black\" width = \"30%\">");
            w.WriteLine("<font size=\"2\">");
            w.WriteLine("<b>Log</b>");
            w.WriteLine("</font>");
            w.WriteLine("</td>");

            for (int i = 0; i < ts.TestResultList.Count; i++)
            {
                if (ts.TestResultList[i] is ProfilerTest)
                {
                    ProfilerTest test = ts.TestResultList[i] as ProfilerTest;
                    w.WriteLine("<tr bgcolor=\"#CCCCC1\">");
                    w.WriteLine("<td align=\"center\" bordercolor=\"white\" width = \"30%\">");
                    w.WriteLine("<font size=\"2\">");
                    w.WriteLine(test.Name);
                    w.WriteLine("</font></td>");

                    w.WriteLine("<td align=\"center\" bordercolor=\"white\" width = \"40%\">");
                    w.WriteLine("<font size=\"2\">");
                    w.WriteLine(test.Location);
                    w.WriteLine("</font></td>");

                    w.WriteLine("<td align=\"center\" bordercolor=\"white\" width = \"30%\">");
                    w.WriteLine("<font size=\"2\">");
                    w.WriteLine("<a href=");

                    // If a UNC path is not specified, assume that the test is being run locally 
                    // and keep all the links local.
                    if (!string.IsNullOrEmpty(ts.ResultsFolder) && ts.IsAlternateLogStoreSpecified)
                    {
                        string[] split = test.LogFile.Split('\\');
                        test.LogFile = ts.ResultsFolder.TrimEnd('\\') + "\\" + split[split.Length - 1];
                        w.WriteLine("file:" + ts.ResultsFolder.Replace('\\', '/').TrimEnd('/') + "/" + split[split.Length - 1]);
                    }
                    else
                    {
                        string[] frag = test.LogFile.Split('\\');
                        w.WriteLine(frag[frag.Length - 1]);
                    }
                    w.WriteLine("><b>" + "Open log" + "</b></a>");
                    w.WriteLine("</font></td>");
                    w.WriteLine("</tr>");
                }
            }

            w.WriteLine("</table>");
            w.WriteLine("</html>");
            w.Close();

            // Reset the values since the tests are done running.
            ts.Device = string.Empty;
            ts.Transport = TestSystem.TransportType.Emulator;
        }

        internal static void CreateTestLog(string path, TestSystem ts)
        {
            string device = "Emulator";
            string name = string.Empty;
            
            if (!string.IsNullOrEmpty(ts.FullDeviceName))
            {
                Utils.WriteToEventLog("Full device name = " + ts.FullDeviceName);
                name = ts.FullDeviceName.ToLower().Trim();
            }

            if (name.StartsWith("imxs_net"))
            {
                device = "iMXS Freescale";
            }
            else if (name.StartsWith("at91sam9261_ek"))
            {
                device = "Sam9";
            }
            else if (name.StartsWith("phytecpcm023"))
            {
                device = "PCM023";
            }            

            StreamWriter w = WriteTestLogHeaders(path, ts, string.Format("{0}{1}", device, " Run Results"));

            w.WriteLine("<tr><td bordercolor=\"white\">");
            w.WriteLine("<table border=\"1\" bordercolor=\"white\" width=\"100%\" align=\"center\">");
            w.WriteLine("<tr><td align=\"center\" bordercolor=\"white\"><b>");
            w.WriteLine("Total test cases executed in this run: " + ts.TotalTestCases);
            w.WriteLine("</b></td></tr>");
            w.WriteLine("<tr><td align=\"center\" bordercolor=\"white\"><font color=\"green\"><b>");
            w.WriteLine("Test Suites Passed: " + ts.PassCount);
            w.WriteLine("</b></font></td></tr>");
            w.WriteLine("<tr><td align=\"center\" bordercolor=\"white\"><font color=\"red\"><b>");
            w.WriteLine("Test Suites Failed: " + ts.FailCount);
            w.WriteLine("</b></font></td></tr>");
            if (ts.SkipCount > 0)
            {
                w.WriteLine("<tr><td align=\"center\" bordercolor=\"white\"><font color=\"gray\"><b>");
                w.WriteLine("Test Suites Skipped: " + ts.SkipCount);
                w.WriteLine("</b></font></td></tr>");
            }
            if (ts.KnownFailureCount > 0)
            {
                w.WriteLine("<tr><td align=\"center\" bordercolor=\"white\"><font color=\"#99FF99\"><b>");
                w.WriteLine("Test Suites with Known Failures: " + ts.KnownFailureCount);
                w.WriteLine("</b></font></td></tr>");
            }
            w.WriteLine("</table>");
            w.WriteLine("</td></tr>");
            w.WriteLine("</table>");
            w.WriteLine("<br>");

            w.WriteLine("<table border=\"3\" bordercolor=\"black\" width=\"100%\" align=\"left\">");

            w.WriteLine("<td align=\"center\" bordercolor=\"black\" width = \"18%\">");
            w.WriteLine("<font size=\"2\">");
            w.WriteLine("<b>Test</b>");
            w.WriteLine("</font>");
            w.WriteLine("</td>");

            w.WriteLine("<td align=\"center\" bordercolor=\"black\" width = \"4%\">");
            w.WriteLine("<font size=\"2\">");
            w.WriteLine("<b>Result</b>");
            w.WriteLine("</font>");
            w.WriteLine("</td>");

            w.WriteLine("<td align=\"center\" bordercolor=\"black\" width = \"8%\">");
            w.WriteLine("<font size=\"2\">");
            w.WriteLine("<b>Total Test Cases</b>");
            w.WriteLine("</font>");
            w.WriteLine("</td>");

            w.WriteLine("<td align=\"center\" bordercolor=\"black\" width = \"8%\">");
            w.WriteLine("<font size=\"2\">");
            w.WriteLine("<b>Total Passed</b>");
            w.WriteLine("</font>");
            w.WriteLine("</td>");

            w.WriteLine("<td align=\"center\" bordercolor=\"black\" width = \"8%\">");
            w.WriteLine("<font size=\"2\">");
            w.WriteLine("<b>Total Failed</b>");
            w.WriteLine("</font>");
            w.WriteLine("</td>");

            w.WriteLine("<td align=\"center\" bordercolor=\"black\" width = \"8%\">");
            w.WriteLine("<font size=\"2\">");
            w.WriteLine("<b>Total Skipped</b>");
            w.WriteLine("</font>");
            w.WriteLine("</td>");

            w.WriteLine("<td align=\"center\" bordercolor=\"black\" width = \"14%\">");
            w.WriteLine("<font size=\"2\">");
            w.WriteLine("<b>Total Known Failures</b>");
            w.WriteLine("</font>");
            w.WriteLine("</td>");

            w.WriteLine("<td align=\"center\" bordercolor=\"black\" width = \"6%\">");
            w.WriteLine("<font size=\"2\">");
            w.WriteLine("<b>Log File</b>");
            w.WriteLine("</font>");
            w.WriteLine("</td>");

            w.WriteLine("<td align=\"center\" bordercolor=\"black\" width = \"12%\">");
            w.WriteLine("<font size=\"2\">");
            w.WriteLine("<b>Start Time</b>");
            w.WriteLine("</font>");
            w.WriteLine("</td>");

            w.WriteLine("<td align=\"center\" bordercolor=\"black\" width = \"12%\">");
            w.WriteLine("<font size=\"2\">");
            w.WriteLine("<b>End Time</b>");
            w.WriteLine("</font>");
            w.WriteLine("</td>");

            WriteTestData(ts, w);

            // Reset the values since the tests are done running.
            ts.Device = string.Empty;
            ts.Transport = TestSystem.TransportType.Emulator;
        }

        private static StreamWriter WriteTestLogHeaders(string path, TestSystem ts, string title)
        {
            FileInfo file = new FileInfo(path + @"\" + TestSystem.ResultsFile);
            StreamWriter w = file.CreateText();
            w.WriteLine("<html>");
            w.WriteLine("<title>");
            w.WriteLine(title);
            w.WriteLine("</title>");
            w.WriteLine("<table border=\"2\" bordercolor=\"gray\" width=\"40%\" align=\"center\">");
            w.WriteLine("<tr><td align=\"center\" bordercolor=\"white\"><b><font size=\"6\">");
            w.WriteLine(title);
            w.WriteLine("</font></b></td></tr>");

            // Write the test automation machine name to the log.
            w.WriteLine("<tr><td align=\"center\" bordercolor=\"white\"><b><font size=\"4\" color=\"#006666\">");
            w.WriteLine("Machine Information");
            w.WriteLine("</font><br/>");
            w.WriteLine("<font size=\"2\" color=\"#3374EC\">");
            w.WriteLine(Environment.MachineName);
            OsInfo osInformation = GetOSInformation();
            string procArch = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER");
            string[] procInfoArray = procArch.Split(' ');
            w.WriteLine("<br/>");
            w.WriteLine(osInformation.OS
                + " (" + osInformation.OSLocale + ")"
                + " " + procInfoArray[0]
                + " " + procInfoArray[procInfoArray.Length - 1]);                     

            if (!string.IsNullOrEmpty(VisualStudioSkew))
            {
                w.WriteLine("<br/>");
                w.WriteLine(VisualStudioSkew);
            }
            w.WriteLine("</font></b></td></tr>");

            if (!string.IsNullOrEmpty(ts.FullDeviceName))
            {
                w.WriteLine("<tr><td align=\"center\" bordercolor=\"white\"><b><font size=\"3\" color=\"brown\">");
                w.WriteLine("Device: " + ts.FullDeviceName);
                w.WriteLine("</font></b></td></tr>");
            }

            if (!string.IsNullOrEmpty(ts.Transport.ToString()) &&
                !string.Equals(ts.Transport.ToString().ToLower(), "none"))
            {
                w.WriteLine("<tr><td align=\"center\" bordercolor=\"white\"><b><font size=\"3\" color=\"brown\">");
                w.WriteLine("Transport: " + ts.Transport);
                w.WriteLine("</font></b></td></tr>");
            }

            if (!string.IsNullOrEmpty(TestSystem.BuildNumber))
            {
                w.WriteLine("<tr><td align=\"center\" bordercolor=\"white\"><b><font size=\"3\">");
                if (!string.IsNullOrEmpty(ts.BuildFlavor))
                {
                    w.Write(ts.BuildFlavor);
                }
                w.WriteLine(" build " + TestSystem.BuildNumber + "<br>");
                if (!string.IsNullOrEmpty(ts.Branch))
                {
                    w.WriteLine(ts.Branch);
                }
                w.WriteLine("</font></b></td></tr>");
            }

            return w;
        }

        private static OsInfo GetOSInformation()
        {
            OsInfo inf = new OsInfo();
            WqlObjectQuery objQuery = new WqlObjectQuery("select * from win32_OperatingSystem");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(objQuery);

            object val; 
            foreach (ManagementObject share in searcher.Get())
            {
                val = share["Name"];
                if (null != val)
                {                    
                    inf.OS = val.ToString().Split('|')[0];
                }
                else
                {
                    inf.OS = string.Empty;
                }

                val = share["Locale"];
                if (null != val)
                {
                    switch(val.ToString())
                    {
                        case "0409":
                            inf.OSLocale = "ENG";
                            break;
                        case "0411":
                            inf.OSLocale = "JPN";
                            break;
                        case "0407":
                            inf.OSLocale = "GER";
                            break;
                    }
                }
                else
                {
                    inf.OSLocale = string.Empty;
                }
            }

            return inf;
        }

        private static void WriteTestData(TestSystem ts, StreamWriter w)
        {
            for (int i = 0; i < ts.TestResultList.Count; i++)
            {
                if (ts.TestResultList[i] is MicroFrameworkTest)
                {
                    MicroFrameworkTest test = ts.TestResultList[i] as MicroFrameworkTest;
                    char[] splitChar = { '|' };
                    string bgColor = string.Empty;

                    try
                    {
                        if (string.Compare(test.Result, "pass", true) == 0)
                        {
                            bgColor = "#99FF99";
                        }
                        else if (string.Compare(test.Result, "fail", true) == 0)
                        {
                            bgColor = "#FE1B42";
                        }
                        else if (string.Compare(test.Result, "skip", true) == 0)
                        {
                            bgColor = "#CFCFCF";
                        }
                        else if (string.Compare(test.Result, "knownfailure", true) == 0)
                        {
                            bgColor = "#99FF90";
                        }
                        else
                        {
                            bgColor = "white";
                        }
                    }
                    catch
                    {
                        bgColor = "#FE1B42";
                        test.Result = "fail";
                    }

                    w.WriteLine("<tr bgcolor=\"" + bgColor + "\">");
                    w.WriteLine("<td align=\"center\" bordercolor=\"white\" width = \"18%\">");
                    w.WriteLine("<font size=\"2\">");

                    // If a UNC path is not specified, do not hyperlink the sln file. Assume that
                    // the test is being run locally and keep all the links local.
                    if (string.IsNullOrEmpty(ts.ResultsFolder))
                    {
                        w.WriteLine("<a href=");
                        if (ts.IsDevEnvironment)
                        {
                            // The test path already contains the local drive information.
                            if (test.Name.Contains(@":\"))
                            {
                                w.WriteLine("\"" + test.Name + "\"");
                            }
                            else
                            {
                                // The test path is relative from the platform tests root.
                                w.WriteLine("\"" + TestSystem.RunMFTests.InstallRoot + @"Tests" + test.Name + "\"");
                            }
                        }
                        else
                        {
                            w.WriteLine("\"" + TestSystem.RunMFTests.InstallRoot + @"\TestCases\" + test.Name + "\"");
                        }

                        int idx = test.Name.LastIndexOf(@"\");
                        w.WriteLine("><b><font color=\"black\">" + test.Name.Substring(idx + 1) + "</font></b></a>");
                    }
                    else
                    {
                        w.WriteLine("<b><u>" + test.Name + "</u></b>");
                    }
                    w.WriteLine("</font></td>");

                    w.WriteLine("<td align=\"center\" bordercolor=\"white\" width = \"4%\"><b>");
                    w.WriteLine("<font size=\"2\">");
                    string result;
                    try
                    {
                        if (test.EmulatorCrashed)
                        {
                            result = test.Result + " (DEVICE CRASH)";
                        }
                        else if (test.TimedOut)
                        {
                            result = test.Result + " (TIMED OUT)";
                        }
                        else
                        {
                            result = test.Result;
                        }
                    }
                    catch
                    {
                        result = "Fail";
                    }

                    w.WriteLine(result);
                    w.WriteLine("</font></b></td>");

                    w.WriteLine("<td align=\"center\" bordercolor=\"white\" width = \"8%\"><b>");
                    w.WriteLine("<font size=\"2\">");
                    w.WriteLine(test.TotalTestCases);
                    w.WriteLine("</font></b></td>");

                    w.WriteLine("<td align=\"center\" bordercolor=\"white\" width = \"8%\"><b>");
                    w.WriteLine("<font size=\"2\">");
                    w.WriteLine(test.TestMethodPassCount);
                    w.WriteLine("</font></b></td>");

                    w.WriteLine("<td align=\"center\" bordercolor=\"white\" width = \"8%\"><b>");
                    w.WriteLine("<font size=\"2\">");
                    w.WriteLine(test.TestMethodFailCount);
                    w.WriteLine("</font></b></td>");

                    w.WriteLine("<td align=\"center\" bordercolor=\"white\" width = \"8%\"><b>");
                    w.WriteLine("<font size=\"2\">");
                    w.WriteLine(test.TestMethodSkipCount);
                    w.WriteLine("</font></b></td>");

                    w.WriteLine("<td align=\"center\" bordercolor=\"white\" width = \"14%\"><b>");
                    w.WriteLine("<font size=\"2\">");
                    w.WriteLine(test.TestMethodKnownFailureCount);
                    w.WriteLine("</font></b></td>");

                    w.WriteLine("<td align=\"center\" bordercolor=\"white\" width = \"6%\">");
                    w.WriteLine("<font size=\"2\">");
                    w.WriteLine("<a href=");

                    // If a UNC path is not specified, assume that the test is being run locally 
                    // and keep all the links local.
                    if (!string.IsNullOrEmpty(ts.ResultsFolder))
                    {
                        w.WriteLine("file:" + ts.ResultsFolder.Replace('\\', '/').TrimEnd('/').Replace(" ", "&#32;") + "/" 
                            + test.LogFile);
                    }
                    else
                    {
                        w.WriteLine(test.LogFile);
                    }
                    w.WriteLine("><b>" + "Open log" + "</b></a>");
                    w.WriteLine("</font></td>");

                    w.WriteLine("<td align=\"center\" bordercolor=\"white\" width = \"12%\"><b>");
                    w.WriteLine("<font size=\"2\">");
                    w.WriteLine(test.StartTime);
                    w.WriteLine("</font></b></td>");

                    w.WriteLine("<td align=\"center\" bordercolor=\"white\" width = \"12%\"><b>");
                    w.WriteLine("<font size=\"2\">");
                    w.WriteLine(test.EndTime);
                    w.WriteLine("</font></b></td>");

                    w.WriteLine("</tr>");
                }
            }

            w.WriteLine("</table>");
            w.WriteLine("</html>");
            w.Close();
        }
        #endregion
    }
}
