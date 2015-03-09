/*---------------------------------------------------------------------
* Master.cs - Tests for the StringBuilder Library
* Main class, responsible for running all of the other *Tests.cs files
* Author: snd\juliusfriedman_cp
* ---------------------------------------------------------------------*/
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;

namespace TextTests.StringBuilder
{
    class StringBuilderTestsMaster
    {
        public static void Main()
        {
            string[] tests = new string[]{                
                "StringBuilderTests"                
            };

            MFTestRunner runner = new MFTestRunner(tests);
        }
    }
}
