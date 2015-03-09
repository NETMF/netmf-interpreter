/*---------------------------------------------------------------------
* Master.cs - file description
* Main class, responsible for running all of the other *Tests.cs files
* Version: 1.0
* Author: REDMOND\a-grchat
* Created: 8/31/2007 4:23:45 PM 
* ---------------------------------------------------------------------*/

using System;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class Master_threading
    {
        public static void Main()
        {
            // TODO: Add your other test classes to args.
            string[] args = {  "ThreadTests"};
            MFTestRunner runner = new MFTestRunner(args);
        }
    }
}