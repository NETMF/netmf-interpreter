////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;

using System.IO;

namespace Microsoft.SPOT.Platform.Tests
{
    public class GetPathRoot : IMFTestInterface
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

        private bool TestGetPathRoot(string path, string expected)
        {
            string result = Path.GetPathRoot(path);
            Log.Comment("Path: '" + path + "'");
            Log.Comment("Expected: '" + expected + "'");
            if (result != expected)
            {
                Log.Exception("Got: '" + result + "'");
                return false;
            }
            return true;
        }

        #endregion helper methods

        #region Test Cases

        [TestMethod]
        public MFTestResults Null()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestGetPathRoot(null, null))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestGetPathRoot(@"sd1\", ""))
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
        public MFTestResults ValidCases()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestGetPathRoot(@"\", @"\"))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestGetPathRoot(@"\dir1", @"\"))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestGetPathRoot(@"\file.text", @"\"))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestGetPathRoot(@"\file\text", @"\"))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestGetPathRoot(@"\sd1\\\\dir\\\\\file\\\\\text\\\\", @"\"))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestGetPathRoot(@"\\machine\dir1\file.tmp", @"\\machine\dir1"))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestGetPathRoot(@"\\machine", @"\\machine"))
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
        public MFTestResults StartWithColon()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                try
                {
                    string path = Path.GetFullPath(":file");
                    Log.Exception("Expected NullArgument exception, but got '" + path + "'");
                    result = MFTestResults.Fail;
                }
                catch (ArgumentException) { /* Pass Case */ }
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
                        string dir = Path.GetPathRoot(path);
                        if ((path.Length == 0) && (dir.Length == 0))
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

        #endregion Test Cases
    }
}
