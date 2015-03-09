////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.SPOT.Emulator.Com;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Security.Principal;
using System.IO;
using Microsoft.Win32.SafeHandles;
using System.Runtime.CompilerServices;

namespace Microsoft.SPOT.Emulator.Usb
{    
    internal class UsbDriver : HalDriver<IUsbDriver>, IUsbDriver
    {
        #region IUsbDriver Members

        bool IUsbDriver.Initialize()
        {
            throw new NotImplementedException();
        }

        bool IUsbDriver.Uninitialize()
        {
            throw new NotImplementedException();
        }

        int IUsbDriver.Write(int ComPortNum, IntPtr Data, uint size)
        {
            throw new NotImplementedException();
        }

        int IUsbDriver.Read(int ComPortNum, IntPtr Data, uint size)
        {
            throw new NotImplementedException();
        }

        bool IUsbDriver.Flush(int ComPortNum)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
