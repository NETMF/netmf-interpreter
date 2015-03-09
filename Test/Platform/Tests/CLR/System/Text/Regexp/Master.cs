/*---------------------------------------------------------------------
* Master.cs - Tests for the Regex_Core Library
* Main class, responsible for running all of the other *Tests.cs files
* Author: snd\juliusfriedman_cp
* ---------------------------------------------------------------------*/
using System;
using System.Collections;
using System.Text.RegularExpressions;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.TextTests
{
    public class TextTests
    {        
        public static void Main()
        {
            string[] tests = new string[]{                
                "PrecompiledTests",
                "Split_Grep_Tests",                
                "SubstringTests",
                "RegexpOptionsTest",
                "CaptureTests",
                "GroupTests",
                "MatchTests",
                "CacheTests"
            };

            MFTestRunner runner = new MFTestRunner(tests);
        }
    }
}