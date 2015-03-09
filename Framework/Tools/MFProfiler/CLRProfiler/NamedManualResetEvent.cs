////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;

namespace CLRProfiler
{
    /// <summary>
    /// Summary description for NamedManualResetEvent.
    /// </summary>
    public class NamedManualResetEvent
    {
        private IntPtr eventHandle;

        struct SECURITY_ATTRIBUTES
        {
            public uint   nLength; 
            public IntPtr lpSecurityDescriptor; 
            public int    bInheritHandle; 
        }; 

        public NamedManualResetEvent(string eventName, bool initialState)
        {
            SECURITY_ATTRIBUTES sa;
            sa.nLength = 12;
            sa.bInheritHandle = 0;
            if (!ConvertStringSecurityDescriptorToSecurityDescriptor("D: (A;OICI;GRGWGXSDWDWO;;;AU)", 1, out sa.lpSecurityDescriptor, IntPtr.Zero))
                throw new Exception("ConvertStringSecurityDescriptorToSecurityDescriptor returned error");
            eventHandle = CreateEvent(ref sa, true, initialState, eventName);
            LocalFree(sa.lpSecurityDescriptor);
            if (eventHandle == IntPtr.Zero)
            {
                eventHandle = OpenEvent(0x00100002, false, eventName);
                if (eventHandle == IntPtr.Zero)
                    throw new Exception(string.Format("Couldn't create or open event {0}", eventName));
            }
        }

        ~NamedManualResetEvent()
        {
            CloseHandle(eventHandle);
        }

        public bool Reset()
        {
            return ResetEvent(eventHandle);
        }

        public bool Set()
        {
            return SetEvent(eventHandle);
        }

        public bool Wait(int timeOut)
        {
            return WaitForSingleObject(eventHandle, timeOut) == 0;
        }

        [DllImport("Advapi32.dll")]
        private static extern bool ConvertStringSecurityDescriptorToSecurityDescriptor(
            string StringSecurityDescriptor,
            uint StringSDRevision,
            out IntPtr SecurityDescriptor,
            IntPtr SecurityDescriptorSize
            );

        [DllImport("Kernel32.dll")]
        private static extern bool LocalFree(IntPtr ptr);

        [DllImport("Kernel32.dll", CharSet=CharSet.Auto)]
        private static extern IntPtr CreateEvent(ref SECURITY_ATTRIBUTES eventAttributes, bool manualReset, bool initialState, string eventName);

        [DllImport("Kernel32.dll", CharSet=CharSet.Auto)]
        private static extern IntPtr OpenEvent(uint desiredAccess, bool inheritHandle, string eventName);

        [DllImport("Kernel32.dll")]
        private static extern bool ResetEvent(IntPtr eventHandle);

        [DllImport("Kernel32.dll")]
        private static extern bool SetEvent(IntPtr eventHandle);

        [DllImport("Kernel32.dll")]
        private static extern bool CloseHandle(IntPtr eventHandle);

        [DllImport("Kernel32.dll")]
        private static extern int WaitForSingleObject(IntPtr handle, int milliseconds);
    }
}
