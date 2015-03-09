using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace Microsoft.SPOT.Debugger
{
    public class Emulator
    {
        string m_exe;
        string m_args;
        string m_pipe;

        Process m_proc;

        public bool Verbose = true;

        event ConsoleOutputEventHandler m_eventOutput;
        event ConsoleOutputEventHandler m_eventError;

        public Emulator(string exe, ArrayList args)
        {
            ArrayList lst = new ArrayList();
            int i;

            for (i = 0; i < args.Count; i++)
            {
                string arg = (string)args[i];

                lst.Add("\"" + arg.Replace("\"", "\\\"") + "\"");
            }

            m_exe = exe;
            m_args = String.Join(" ", (string[])lst.ToArray(typeof(string)));
        }

        public PortDefinition_Emulator CreatePortDefinition()
        {
            if (m_pipe == null)
            {
                throw new ApplicationException("Emulator not started yet -- not pipe created");
            }

            return new PortDefinition_Emulator(m_pipe, m_pipe, 0);
        }

        public static PortDefinition[] EnumeratePipes()
        {
            SortedList lst = new SortedList();
            Regex re = new Regex("^TinyCLR_([0-9]+)_Port1$");

            try
            {
                String[] pipeNames = Directory.GetFiles(@"\\.\pipe");

                foreach (string pipe in pipeNames)
                {
                    try
                    {
                        if (re.IsMatch(Path.GetFileName(pipe)))
                        {
                            int pid = Int32.Parse(re.Match(Path.GetFileName(pipe)).Groups[1].Value);
                            PortDefinition pd = PortDefinition.CreateInstanceForEmulator("Emulator - pid " + pid, pipe, pid);

                            lst.Add(pd.DisplayName, pd);
                        }
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }

            ICollection col = lst.Values;
            PortDefinition[] res = new PortDefinition[col.Count];

            col.CopyTo(res, 0);

            return res;
        }

        public event ConsoleOutputEventHandler OnStandardOutput
        {
            add
            {
                m_eventOutput += value;
            }

            remove
            {
                m_eventOutput -= value;
            }
        }

        public event ConsoleOutputEventHandler OnStandardError
        {
            add
            {
                m_eventError += value;
            }

            remove
            {
                m_eventError -= value;
            }
        }

        public Process Process
        {
            get
            {
                return m_proc;
            }
        }

        public void Start(ProcessStartInfo psi)
        {
            psi.FileName = m_exe;
            psi.Arguments = m_args;

            m_proc = new Process();
            m_proc.StartInfo = psi;

            if (psi.RedirectStandardOutput)
            {
                m_proc.OutputDataReceived += new DataReceivedEventHandler(StandardOutputHandler);
            }

            if (psi.RedirectStandardError)
            {
                m_proc.ErrorDataReceived += new DataReceivedEventHandler(StandardErrorHandler);
            }

            m_proc.Start();

            if (psi.RedirectStandardOutput)
            {
                m_proc.BeginOutputReadLine();
            }

            if (psi.RedirectStandardError)
            {
                m_proc.BeginErrorReadLine();
            }


            m_pipe = PortDefinition_Emulator.PipeNameFromPid(m_proc.Id);
        }

        public void Start()
        {
            if (Verbose)
            {
                Console.WriteLine("Launching '{0}' with params '{1}'", m_exe, m_args);
            }

            ProcessStartInfo psi = new ProcessStartInfo();

            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.UseShellExecute = false;
            psi.WorkingDirectory = Directory.GetCurrentDirectory();

            Start(psi);
        }

        public void WaitForExit()
        {
            m_proc.WaitForExit();
        }

        public void Stop()
        {
            if (m_proc != null)
            {
                if (m_proc.HasExited == false)
                {
                    try
                    {
                        m_proc.Kill();
                    }
                    catch
                    {
                    }
                }

                m_proc = null;
            }
        }

        private void StandardOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            m_eventOutput(outLine.Data);
        }

        private void StandardErrorHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            m_eventError(outLine.Data);
        }
    }
}