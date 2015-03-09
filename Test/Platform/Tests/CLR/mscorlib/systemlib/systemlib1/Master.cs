/*---------------------------------------------------------------------
* Master.cs - file description
* Main class, responsible for running all of the other *Tests.cs files
* Version: 1.0
* Author: REDMOND\a-grchat
* Created: 8/23/2007 10:07:28 AM 
* ---------------------------------------------------------------------*/

using System;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class Master_system
    {
        public static void Main()
        {
            string[] args = { "GuidTests", "SystemTimeSpanTests","ParseTests"};
            MFTestRunner runner = new MFTestRunner(args);
        }
    }
}