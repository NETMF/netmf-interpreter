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

namespace Microsoft.SPOT.Platform.Tests
{
    public class IOTests : IOTestsBase 
    {
        public static void Main()
        {
            Log.Comment("These tests might create a directory DOTNETMF_FS_EMULATION in the location where the test solution is");

            IOTests.Tests = new string[] {
                "CanRead",
                "CanSeek",
                "CanWrite",
                "Constructors_FileMode",
                "Constructors_FileAccess",
                "Constructors_FileShare",
                "Flush",
                "PropertyTests",
                "Read",
                "Seek",
                "Write",
            };

            MFTestRunner runner = new MFTestRunner(IOTests.Tests);
        }
    }
}
