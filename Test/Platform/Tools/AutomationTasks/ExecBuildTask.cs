using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using Microsoft.SPOT.Automation.Build.Common;

using Diagnostics = System.Diagnostics;

using Task = Microsoft.Build.Utilities.Task;

namespace Microsoft.SPOT.Automation.Build.Branch
{
    public class ExecBuildTask : Task
    {
        private static object lockObject = new object();
        private static Random fileRandom = new Random();

        string command;
        string batchParameter;
        ITaskItem[] taskItems = null;
        string defaultEnvironment = null;

        ITaskItem[] allBuildFlavors = null;
        string prefBuildFlavor = null;
        string exclBuildFlavor = null;
        
        bool automatedBuild = false;
        string buildDrop = null;
        string localBuildDrop = null;
        string resultsFile = null;
        string branch;
        string target;
        string resultString;

        int timeout = 0;

        List<ITaskItem> exeBuildFlavors = new List<ITaskItem>();

        public override bool Execute()
        {
            try
            {
                if (automatedBuild)
                {
                    if (string.IsNullOrEmpty(buildDrop) || !Directory.Exists(buildDrop))
                    {
                        throw new ArgumentException("Build drop \"" + buildDrop + "\" was either not specified or does not exist");
                    }

                    if (string.IsNullOrEmpty(resultsFile))
                    {
                        throw new ArgumentException("Results File not specified");
                    }

                    if (string.IsNullOrEmpty(resultString))
                    {
                        throw new ArgumentException("Result String not specified");
                    }
                }

                // Figure out what build flavors we want to build
                foreach (ITaskItem flavorItem in allBuildFlavors)
                {
                    ITaskItem flavorCandidate = null;
                    if (!string.IsNullOrEmpty(prefBuildFlavor))
                    {
                        if (flavorItem.ItemSpec.Equals(prefBuildFlavor, StringComparison.OrdinalIgnoreCase))
                        {
                            flavorCandidate = flavorItem;
                        }
                    }
                    else
                    {
                        flavorCandidate = flavorItem;
                    }

                    if (!string.IsNullOrEmpty(exclBuildFlavor) && flavorItem.ItemSpec.Equals(exclBuildFlavor, StringComparison.OrdinalIgnoreCase))
                    {
                        flavorCandidate = null;
                    }

                    if (flavorCandidate != null)
                    {
                        exeBuildFlavors.Add(flavorCandidate);
                    }
                }

                // Do the build for each flavor (flavor loop)
                foreach (ITaskItem exeBuildFlavor in exeBuildFlavors)
                {
                    if (taskItems != null && taskItems.Length > 0)
                    {
                        foreach (ITaskItem taskItem in taskItems)
                        {
                            string preferredTaskFlavor = taskItem.GetMetadata("PreferredBuildFlavor");
                            // If the task specified a preferred build flavor and the current build flavor does not 
                            // match the preferred build flavor skip it (ie. continue with the flavor loop)
                            if (!string.IsNullOrEmpty(preferredTaskFlavor) && !preferredTaskFlavor.Equals(exeBuildFlavor.ItemSpec))
                            {
                                continue;
                            }

                            string []excludeTaskFlavors = taskItem.GetMetadata("ExcludeBuildFlavor").Split(new char[]{';'}, StringSplitOptions.RemoveEmptyEntries);
                            // If the task specified an flavor to exclude (do not build) and the current build flavor
                            // matches that flavor skip it (ie. continue with the flavor loop).
                            bool fSkip = false;
                            foreach (string excludeTaskFlavor in excludeTaskFlavors)
                            {
                                if (!string.IsNullOrEmpty(excludeTaskFlavor) && excludeTaskFlavor.Equals(exeBuildFlavor.ItemSpec))
                                {
                                    fSkip = true;
                                    break;
                                }
                            }

                            if (fSkip) continue;


                            string script = taskItem.GetMetadata("EnvironmentScript");

                            if (string.IsNullOrEmpty(script))
                            {
                                script = defaultEnvironment;
                            }

                            foreach (string variableName in taskItem.MetadataNames)
                            {
                                if (variableName.Equals("EnvironmentScript", StringComparison.OrdinalIgnoreCase) ||
                                    !variableName.StartsWith("env_"))
                                {
                                    continue;
                                }
                                string varName = variableName.Substring(4);
                                exeBuildFlavor.SetMetadata(varName, taskItem.GetMetadata(variableName));
                            }
                            
                            ExecuteCommand(script, exeBuildFlavor);
                        }
                    }
                    else
                    {
                        ExecuteCommand(defaultEnvironment, exeBuildFlavor);
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                try
                {
                    Log.LogErrorFromException(e);
                }
                catch { }
                return false;
            }
        }

        protected void ExecuteCommand(string envScript, ITaskItem flavorVariables)
        {
            Process buildProcess = new Process();

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "cmd.exe";
            string buildCmd = command;

            string logFile = null;
            BuildInfo result = new BuildInfo();
            lock (lockObject)
            {
                if (automatedBuild)
                {
                    string logDir = buildDrop + "\\Logs";

                    if (!Directory.Exists(logDir))
                    {
                        Directory.CreateDirectory(logDir);
                    }

                    do
                    {
                        Thread.Sleep(1);
                        logFile = logDir + "\\" +
                            string.Format("{0}.{1}.{2}.{3}.log",
                                branch,
                                target,
                                flavorVariables.ItemSpec,
                                fileRandom.Next());

                    } while (File.Exists(logFile));

                    string cmdFile = Path.GetDirectoryName(logFile) + "\\" + Path.GetFileNameWithoutExtension(logFile) + ".cmd";

                    StreamWriter writer = new StreamWriter(cmdFile);

                    string flavor_cmd = command;

                    if (flavor_cmd.ToLower().TrimStart().StartsWith("msbuild"))
                    {
                        flavor_cmd += " /p:FLAVOR=" + flavorVariables.ItemSpec;

                        if (automatedBuild) flavor_cmd += " /p:AUTOMATED_BUILD=true";
                    }

                    writer.WriteLine("@echo off");
                    writer.WriteLine("@echo Setting Env: {0} >{1}", envScript, logFile);
                    writer.WriteLine("echo Executing Build Task: {0} >{1}", flavor_cmd, logFile);
                    writer.WriteLine("call {0} >>{1} 2>&1", flavor_cmd, logFile);

                    writer.Close();

                    buildCmd = string.Format("call {0}", cmdFile);
                }

                psi.Arguments = string.Format("/c {0}&{1}", envScript, buildCmd);
                psi.UseShellExecute = false;
                psi.CreateNoWindow = false;

                // Override the build drop location if specified.
                if (localBuildDrop != null)
                {
                    psi.EnvironmentVariables["COMMON_BUILD_ROOT"] = localBuildDrop;
                }

                // Set up the environment variables for the process
                foreach (string variableName in flavorVariables.MetadataNames)
                {
                    if (!psi.EnvironmentVariables.ContainsKey(variableName))
                    {
                        psi.EnvironmentVariables.Add(variableName, flavorVariables.GetMetadata(variableName));
                    }
                    else
                    {
                        psi.EnvironmentVariables[variableName] = flavorVariables.GetMetadata(variableName);
                    }
                }

                buildProcess.StartInfo = psi;
                try
                {
                    Log.LogMessage("Building {0} {1}", resultString, flavorVariables.ItemSpec);
                }
                catch { }

                result.Branch = branch;
                result.Cmd = psi.Arguments;
                result.Description = resultString;
                result.Flavor = flavorVariables.ItemSpec;
                result.Log = logFile;
                result.StartTime = DateTime.Now;

                buildProcess.Start();
            }

            // Wait for the process to complete. If the process timeout occurs
            // then we clean up.  If timeout == 0 set to max, which is infinite.
            if (!buildProcess.WaitForExit(timeout == 0 ? int.MaxValue : timeout * (60 * 1000)))
            {
                // Attempt to clean up the process.
                // It doesn't really matter how good of a job
                // this is becuase we will reboot the host machine
                // at the end of the build process.
                buildProcess.Kill();

                // Try to kill any Doc Watson processes we find.
                // Why?  Just because I hate Doc Watson.  He is the main reason
                // we need a timeout.
                foreach (Process docWatson in Process.GetProcessesByName("DW20"))
                {
                    docWatson.Kill();
                }

                if (automatedBuild)
                {
                    result.Result = BuildResult.Timeout;
                    AppendResult(result);
                }
                throw new ApplicationException("Build Task execution time exceeded " + timeout + " minutes");
            }
            else if (automatedBuild)
            {
                result.Result = buildProcess.ExitCode == 0 ? BuildResult.Pass : BuildResult.Fail;
                AppendResult(result);
            }
        }

        public void AppendResult(BuildInfo result)
        {
            result.EndTime = DateTime.Now;
            if (!File.Exists(resultsFile))
            {
                try
                {
                    Log.LogWarning("Unable to log build result. Result file \"" + resultsFile + "\" does not exist");
                }
                catch { }
                return;
            }

            BuildInfoCollection objResults = BuildResultsFactory.AddBuildInfo(result, resultsFile);

            FireBuildEvent(result, objResults.BuildNumber);
        }

        private void FireBuildEvent(BuildInfo result, int buildNumber)
        {
            try
            {
                BuildTaskConnector.GetRemoteInstance().FireBuildEvent(
                    result, 
                    buildDrop, 
                    buildNumber.ToString());
            }
            catch (Exception e)
            {
                try
                {
                    Log.LogWarning("Unable to send build event to the service controller.\r\n" + e.Message);
                }
                catch { }
            }
        }

        [Required]
        public string Command
        {
            get { return command; }
            set { command = value; }
        }
        
        public ITaskItem[] TaskItemGroup
        {
            get { return taskItems; }
            set { taskItems = value; }
        }

        public string BatchParameter
        {
            get { return batchParameter; }
            set { batchParameter = value; }
        }

        [Required]
        public ITaskItem[] BuildFlavors
        {
            get { return allBuildFlavors; }
            set { allBuildFlavors = value; }
        }

        public string PreferredBuildFlavor
        {
            get { return prefBuildFlavor; }
            set { prefBuildFlavor = value; }
        }

        public string ExcludeBuildFlavor
        {
            get { return exclBuildFlavor; }
            set { exclBuildFlavor = value; }
        }

        [Required]
        public string DefaultEnvironment
        {
            get { return defaultEnvironment; }
            set { defaultEnvironment = value; }
        }

        [Required]
        public bool AutomatedBuild
        {
            get { return automatedBuild; }
            set { automatedBuild = value; }
        }

        public string BuildDrop
        {
            get { return buildDrop; }
            set { buildDrop = value; }
        }

        public string LocalBuildDrop
        {
            set { localBuildDrop = value; }
        }

        public string ResultFile
        {
            get { return resultsFile; }
            set { resultsFile = value; }
        }

        [Required]
        public string Branch
        {
            get { return branch; }
            set { branch = value; }
        }

        [Required]
        public string Target
        {
            get { return target; }
            set { target = value; }
        }

        public string ResultString
        {
            get { return resultString; }
            set { resultString = value; }
        }

        public int Timeout
        {
            set { timeout = value; }
        }
    }
}
