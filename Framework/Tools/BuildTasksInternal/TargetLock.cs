using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;

using Task = Microsoft.Build.Utilities.Task;

namespace Microsoft.SPOT.Tasks.Internal
{
    public class TargetLock : Task
    {
        private string m_Name;
        private bool m_Lock;
        public override bool Execute()
        {
            System.Threading.EventWaitHandle handle = new System.Threading.EventWaitHandle(
                true, System.Threading.EventResetMode.AutoReset, m_Name);

            if (m_Lock)
            {
                handle.WaitOne();
                handle.Reset();
            }
            else
            {
                handle.Set();
            }

            return true;
        }

        [Required]
        public string EventName
        {
            set { m_Name = value; }
        }

        [Required]
        public bool Lock
        {
            set { m_Lock = value; }
        }
    }
}
