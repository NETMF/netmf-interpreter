using System;
using System.IO;

namespace Microsoft.SPOT.Debugger
{
    [Serializable]
    public class PortDefinition_Emulator : PortDefinition
    {
        int m_pid;

        public PortDefinition_Emulator(string displayName, string port, int pid)
            : base(displayName, port)
        {
            m_pid = pid;
        }

        public PortDefinition_Emulator(string displayName, int pid)
            : this(displayName, PipeNameFromPid(pid), pid)
        {
        }

        public int Pid
        {
            get
            {
                return m_pid;
            }
        }

        public override Stream CreateStream()
        {
            AsyncFileStream afs = null;

            afs = new AsyncFileStream(m_port, FileShare.ReadWrite);

            return afs;
        }

        internal static string PipeNameFromPid(int pid)
        {
            return string.Format(@"\\.\pipe\TinyCLR_{0}_Port1", pid.ToString());
        }
    }
}