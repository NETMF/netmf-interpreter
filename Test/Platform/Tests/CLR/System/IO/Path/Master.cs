/*---------------------------------------------------------------------
* Master.cs - file description
* Main class, responsible for running all of the other *Tests.cs files
* Version: 1.0
* Author: REDMOND\jamesweb
* Created: 8/8/2008 10:20:51 AM 
* ---------------------------------------------------------------------*/
using System;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.IO;
using System.IO;

namespace Microsoft.SPOT.Platform.Tests
{
    public class IOTests : IOTestsBase 
    {
        public static void Main()
        {
            Log.Comment("These tests might create a directory DOTNETMF_FS_EMULATION in the location where the test solution is");

            // Tests that don't need file system
            string[] tests = {
                "ChangeExtensions",
                "Combine",
                "GetDirectoryName",
                "GetExtension",
                "GetFileName",
                "GetFileNameWithoutExtension",
                "GetPathRoot",
                "HasExtension",
                "IsPathRooted",
                //"FunctionalCases",
            };

            // Tests that need file system
            IOTests.Tests = new string[] {
                "GetFullPath",
            };

            String[] allTests = new String[tests.Length + IOTests.Tests.Length];
            tests.CopyTo(allTests, 0);
            IOTests.Tests.CopyTo(allTests, tests.Length);

            MFTestRunner runner = new MFTestRunner(allTests);
        }
    }
}
