////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;

using System.IO;

namespace Microsoft.SPOT.Platform.Tests
{
    public class GetFileNameWithoutExtension : IMFTestInterface
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

        private bool TestGetFileNameWithoutExtension(string path, string expected)
        {
            string result = Path.GetFileNameWithoutExtension(path);
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
                if (!TestGetFileNameWithoutExtension(null, null))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestGetFileNameWithoutExtension("", ""))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestGetFileNameWithoutExtension(string.Empty, string.Empty))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestGetFileNameWithoutExtension(".ext", ""))
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
                if (!TestGetFileNameWithoutExtension("file", "file"))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestGetFileNameWithoutExtension("file.txt", "file"))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestGetFileNameWithoutExtension(@"\sd1\dir1\file.txt", "file"))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestGetFileNameWithoutExtension(@"dir1\dir2\file", "file"))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestGetFileNameWithoutExtension("file.........ext...", "file.........ext.."))
                {
                    result = MFTestResults.Fail;
                }
                string file = "foo.bar.fkl;fkds92-509450-4359.$#%()#%().%#(%)_#(%_)";
                if (!TestGetFileNameWithoutExtension(file + ".cool", file))
                {
                    result = MFTestResults.Fail;
                }
                file = new string('x', 256);
                if (!TestGetFileNameWithoutExtension("cool." + file, "cool"))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestGetFileNameWithoutExtension(file + ".c", file))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestGetFileNameWithoutExtension(file + "." + file, file))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestGetFileNameWithoutExtension("file.....ext", "file...."))
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
                        string dir = Path.GetFileNameWithoutExtension(path);
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
