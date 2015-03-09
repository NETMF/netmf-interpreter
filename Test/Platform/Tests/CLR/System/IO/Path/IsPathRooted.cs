////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;

using System.IO;

namespace Microsoft.SPOT.Platform.Tests
{
    public class IsPathRooted : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests");

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");

            // TODO: Add your clean up steps here.
        }

        #region helper methods
        private bool expected;
        private bool TestIsPathRooted(string path)
        {
            Log.Comment("Path: '" + path + "'");
            if (Path.IsPathRooted(path) == expected)
                return true;
            Log.Exception("Expected " + expected);
            return false;
        }

        #endregion helper methods

        [TestMethod]
        public MFTestResults Negative()
        {
            MFTestResults result = MFTestResults.Pass;
            expected = false;
            try
            {
                if (!TestIsPathRooted(null))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestIsPathRooted(""))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestIsPathRooted(string.Empty))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestIsPathRooted("file"))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestIsPathRooted(".txt"))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestIsPathRooted("file.x"))
                {
                    result = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                result = MFTestResults.Fail;
            }
            return result;
        }

        [TestMethod]
        public MFTestResults Positive()
        {
            MFTestResults result = MFTestResults.Pass;
            expected = true;
            try
            {
                if (!TestIsPathRooted(@"\"))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestIsPathRooted(@"\sd\file"))
                {
                    result = MFTestResults.Fail;
                }

                if (!TestIsPathRooted(@"\sd\file.comp"))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestIsPathRooted(@"\\Machine\directory.dir"))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestIsPathRooted(@"\\Machine\directory\file.zz"))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestIsPathRooted(@"\sd\file." + new string('f', 256)))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestIsPathRooted(@"\\Machine"))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestIsPathRooted(@"\\Machine\directory"))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestIsPathRooted(@"\\Machine\directory\file"))
                {
                    result = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                result = MFTestResults.Fail;
            }
            return result;
        }

        [TestMethod]
        public MFTestResults ArgumentExceptionTests()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                foreach (char invalidChar in Path.GetInvalidPathChars())
                {
                    try
                    {
                        Log.Comment("Invalid char ascii val = " + (int)invalidChar);
                        string path = new string(new char[] { invalidChar, 'b', 'a', 'd', '.', invalidChar, 'p', 'a', 't', 'h', invalidChar });
                        bool dir = Path.IsPathRooted(path);
                        if ((path.Length == 0) && (dir == false))
                        {
                            /// If path is empty string, returned value is also empty string (same behavior in desktop)
                            /// no exception thrown.
                        }
                        else
                        {
                            Log.Exception("Expected Argument exception for '" + path + "' but got: '" + dir + "'");
                            result = MFTestResults.Fail;
                        }
                    }
                    catch (ArgumentException) { /* Pass Case */ }
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                result = MFTestResults.Fail;
            }
            return result;
        }
    }
}
