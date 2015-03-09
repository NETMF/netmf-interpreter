/*---------------------------------------------------------------------
* Master.cs - file description
* Main class, responsible for running all of the other *Tests.cs files
* Version: 1.0
* Author: REDMOND\a-grchat
* Created: 9/4/2007 10:20:51 AM 
* ---------------------------------------------------------------------*/
using System;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Cryptoki;
using System.IO;

namespace Microsoft.SPOT.Platform.Tests
{
    public class SecurityTests 
    {
        public static void Main()
        {
            Log.Comment("These tests might create a directory DOTNETMF_FS_EMULATION in the location where the test solution is");

            string []tests = new string[]
            {
                "RngBits",
                "rng3",
                "rng2",
                "rng1",
            };
            
            MFTestRunner runner = new MFTestRunner(tests);
        }
    }
}