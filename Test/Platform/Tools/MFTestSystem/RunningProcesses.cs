using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.SPOT.Platform.Test
{
    internal class RunningProcesses
    {
        private static ArrayList m_list = new ArrayList();

        internal static ArrayList List
        {
            get
            {
                return m_list;
            }
        }
    }
}