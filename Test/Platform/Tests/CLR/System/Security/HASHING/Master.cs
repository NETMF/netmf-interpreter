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
                "SHAVS",
                "HashCompare",
                "Hash_SHA512known",
                "Hash_SHA512",
                "Hash_SHA384known",
                "Hash_SHA384",
                "Hash_MD5known",
                "Sim_SHA1",
                "Hash_SHA256known",
                "Hash_SHA256",
                "HMACTestVector",
                "Hash_HMACSHA1known",
                "GetHashVsStream",
            };

            MFTestRunner runner = new MFTestRunner(tests);
        }
    }
}