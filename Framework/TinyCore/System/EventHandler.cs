using System;
using System.Collections;
using System.Threading;

// need move this into mscorlib, also get the real implementation.
namespace Microsoft.SPOT
{
    public delegate void EventHandler(object sender, EventArgs e);

    public delegate void CancelEventHandler(object sender, CancelEventArgs e);

    public class CancelEventArgs : EventArgs
    {
        public bool Cancel;
    }
}


