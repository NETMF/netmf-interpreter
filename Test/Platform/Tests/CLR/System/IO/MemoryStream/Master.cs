/*---------------------------------------------------------------------
* Master.cs - file description
* Main class, responsible for running all of the other *Tests.cs files
* Version: 1.0
* Author: REDMOND\jamesweb
* Created: 8/8/2008 2:20:51 PM 
* ---------------------------------------------------------------------*/
using System;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.IO;
using System.IO;
using System.Collections;


namespace Microsoft.SPOT.Platform.Tests
{
    public class IOTests : IOTestsBase 
    {
        public static void Main()
        {
            Log.Comment("These tests might create a directory DOTNETMF_FS_EMULATION in the location where the test solution is");

            // Tests that don't need file system
            string[] tests = {
                "CanRead",
                "CanSeek",
                "CanWrite",
                "Close",
                "Flush",
                "Length",
                "MemoryStream_Ctor",
                "Position",
                "Read",
                "ReadByte",
                "Seek",
                "SetLength",
                "ToArray",
                "Write",
                "WriteByte",
            };

            // Tests that need file system
            IOTests.Tests = new string[] {
                "WriteTo",
            };

            String[] allTests = new String[tests.Length + IOTests.Tests.Length];
            tests.CopyTo(allTests, 0);
            IOTests.Tests.CopyTo(allTests, tests.Length);

            MFTestRunner runner = new MFTestRunner(allTests);
        }
    }
}
