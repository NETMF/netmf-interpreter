using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace MFDpwsTestCaseGenerator
{
    public class MSBuildProcess
    {
        private Process m_Process = new Process();
        private string m_Project;
        private Dictionary<string, string> m_Properties = new Dictionary<string, string>();
        private string m_Target;
        private string m_WorkingDirectory;
        
        public Process Process
        {
            get { return m_Process; }
        }

        public string Project
        {
            get { return m_Project; }
            set 
            { 
                m_Project = value;

                if (!m_Project.StartsWith("\""))
                {
                    m_Project = "\"" + m_Project;
                }

                if (!m_Project.EndsWith("\""))
                {
                    m_Project += "\"";
                }
            }
        }

        public string Target
        {
            get { return m_Target; }
            set { m_Target = value; }
        }

        public void AddProperty(string name, string value)
        {
            m_Properties.Add(name, value);
        }

        public Dictionary<string, string> Properties
        {
            get { return m_Properties; }
        }

        public string WorkingDirectory
        {
            get { return m_WorkingDirectory; }
            set { m_WorkingDirectory = value; }
        }

        public int Build()
        {
            string msbuild = "";

            int maj = 0;
            int min = 0;
            int bld = 0;

            string msbuildPath = Environment.GetEnvironmentVariable("WINDIR") + "\\Microsoft.NET\\Framework\\";

            foreach(string dir in Directory.GetDirectories(msbuildPath))
            {
                if(!File.Exists(Path.Combine(dir, "msbuild.exe"))) continue;

                Regex exp = new Regex(@"v(\d+)\.(\d+)\.?(\d)?");
                Match m = exp.Match(Path.GetFileName(dir));
                if (m.Success)
                {
                    int b = 0;
                    int v1 = int.Parse(m.Groups[1].Value);
                    int v2 = int.Parse(m.Groups[2].Value);
                    if (m.Groups.Count > 3 && m.Groups[3].Success)
                    {
                        b = int.Parse(m.Groups[3].Value);
                    }

                    bool isNewer = false;

                    if (v1 > maj)
                    {
                        isNewer = true;
                    }
                    else if (v1 == maj)
                    {
                        if (v2 > min)
                        {
                            isNewer = true;
                        }
                        else if (v2 == min)
                        {
                            if (b > bld)
                            {
                                isNewer = true;
                            }
                        }
                    }

                    if(isNewer)
                    {
                        maj = v1;
                        min = v2;
                        bld = b;

                        msbuild = Path.Combine(dir, "msbuild.exe");
                    }
                }
            }

            ProcessStartInfo psi = new ProcessStartInfo("cmd.exe");
            string args = string.Format("/c {0} /nologo {1}",
                msbuild,
                m_Project
                );

            foreach (KeyValuePair<string, string> pair in m_Properties)
            {
                args += string.Format(" /p:{0}={1}", pair.Key, pair.Value);
            }

            if (!string.IsNullOrEmpty(m_Target))
            {
                args += string.Format(" /t:{0}", m_Target);
            }

            psi.Arguments = args;

            if (!string.IsNullOrEmpty(m_WorkingDirectory))
            {
                psi.WorkingDirectory = m_WorkingDirectory;
            }
            psi.CreateNoWindow = false;
            psi.UseShellExecute = false;
            
            m_Process.StartInfo = psi;

            m_Process.Start();
            
            m_Process.WaitForExit();

            int exitCode = m_Process.ExitCode;

            m_Process = new Process();

            return exitCode;
        }
    }
}
