////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.IO;

namespace Microsoft.SPOT.Platform.Tests
{
    public class HasExtension : IMFTestInterface
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
        }

        #region helper methods

        private bool expected;

        private bool TestHasExtension(string path)
        {
            Log.Comment("Path: '" + path + "'");
            if (Path.HasExtension(path) & expected)
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
                if (TestHasExtension(null))
                {
                    result = MFTestResults.Fail;
                }
                if (TestHasExtension(""))
                {
                    result = MFTestResults.Fail;
                }
                if (TestHasExtension(string.Empty))
                {
                    result = MFTestResults.Fail;
                }
                if (TestHasExtension(@"\"))
                {
                    result = MFTestResults.Fail;
                }
                if (TestHasExtension("file"))
                {
                    result = MFTestResults.Fail;
                }
                if (TestHasExtension(@"\sd\file"))
                {
                    result = MFTestResults.Fail;
                }
                if (TestHasExtension(@"\\Machine"))
                {
                    result = MFTestResults.Fail;
                }
                if (TestHasExtension(@"\\Machine\directory"))
                {
                    result = MFTestResults.Fail;
                }
                if (TestHasExtension(@"\\Machine\directory\file"))
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
                if (!TestHasExtension(".txt"))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestHasExtension(@"\file.txt"))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestHasExtension("file.x"))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestHasExtension(@"\sd\file.comp"))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestHasExtension(@"\\Machine\directory.dir"))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestHasExtension(@"\\Machine\directory\file.zz"))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestHasExtension(@"\sd\file." + new string('f',256)))
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
                        bool dir = Path.HasExtension(path);
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
