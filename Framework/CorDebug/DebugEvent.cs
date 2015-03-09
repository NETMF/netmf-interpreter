using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using System.Runtime.InteropServices;
using CorDebugInterop;
using Microsoft.Win32;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.SPOT.Debugger
{
    public class DebugEvent : IDebugEvent2
    {
        private uint m_attributes;

        public DebugEvent(uint attributes)
        {
            m_attributes = attributes;
        }

        #region IDebugEvent2 Members

        public int GetAttributes(out uint pdwAttrib)
        {
            pdwAttrib = m_attributes;
            return Utility.COM_HResults.S_OK;
        }

        #endregion

    }

}
