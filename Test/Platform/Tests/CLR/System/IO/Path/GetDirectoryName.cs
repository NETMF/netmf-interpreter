////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;

using System.IO;

namespace Microsoft.SPOT.Platform.Tests
{
    public class GetDirectoryName : IMFTestInterface
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

        private bool TestGetDirectoryName(string path, string expected)
        {
            string result = Path.GetDirectoryName(path);
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
        public MFTestResults NullPath()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestGetDirectoryName(null, null))
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
        public MFTestResults Vanilla()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestGetDirectoryName("Hello\\file.tmp", "Hello"))
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
        public MFTestResults StartWithSlash()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                //single slash
                if (!TestGetDirectoryName("\\Root\\File", "\\Root"))
                {
                    result = MFTestResults.Fail;
                }
                //root double slash
                if (!TestGetDirectoryName("\\\\Machine\\Directory\\File", "\\\\Machine\\Directory"))
                {
                    result = MFTestResults.Fail;
                }
                //root triple slash, this will throw an exception.

                try
                {
                    TestGetDirectoryName("\\\\\\Machine\\Directory\\File", null);
                    result = MFTestResults.Fail;
                }
                catch (ArgumentException)
                {                    
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
        public MFTestResults WhiteSpace()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                //white space in directory
                if (!TestGetDirectoryName("\\root\\Directory Name\\Hello.tmp file.tmp", "\\root\\Directory Name"))
                {
                    result = MFTestResults.Fail;
                }
                //white space in file
                if (!TestGetDirectoryName("\\root\\Directory Name\\File Name.tmp file.tmp", "\\root\\Directory Name"))
                {
                    result = MFTestResults.Fail;
                } 
                //white space at Root
                if (!TestGetDirectoryName("\\root\\Hello.tmp file.tmp", "\\root"))
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
        public MFTestResults ForwardSlashes()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                /// Forward slash is invalid.
                TestGetDirectoryName("//root//Director//file.tmp", "\\root\\Directory");
                result = MFTestResults.Fail;
            }
            catch (ArgumentException)
            {
                result = MFTestResults.Pass;
            }
            catch (Exception ex)
            {
                Log.Exception("Exception: " + ex.Message);
                result = MFTestResults.Fail;
            }
            return result;
        }

        [TestMethod]
        public MFTestResults EndingDirectory()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestGetDirectoryName("\\root\\Dir1\\Dir2\\", "\\root\\Dir1\\Dir2"))
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
        public MFTestResults DeepTree()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                string deepTree = "\\root\\Dir1\\Dir2\\Dir1\\Dir2\\Dir1\\Dir2\\Dir1\\Dir2\\Dir1\\Dir2\\Dir1\\Dir2\\Dir1\\Dir2\\Dir1\\Dir2\\Dir1\\Dir2\\Dir1\\Dir2\\Dir1\\Dir2\\Dir2\\Dir1\\Dir2\\Dir1\\Dir2";
                if (!TestGetDirectoryName(deepTree + "\\File.exe", deepTree))
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
        public MFTestResults RootPath()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestGetDirectoryName("\\", null))
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
        public MFTestResults UNCPath()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestGetDirectoryName("\\\\Machine\\Directory\\File", "\\\\Machine\\Directory"))
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
        public MFTestResults DotsAtEnd()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestGetDirectoryName("\\root\\test\\.", "\\root\\test"))
                {
                    result = MFTestResults.Fail;
                }
                try
                {
                    Path.GetDirectoryName("\\root\\test\\ .");
                    Log.Exception("Expected Argument exception");
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
                string[] invalidArgs = { "", " ", "  ", "\t", "\n", "\r\n" };
                foreach (string path in invalidArgs)
                {
                    try
                    {
                        string dir = Path.GetDirectoryName(path);
                        Log.Exception("Expected Argument exception for '" + path + "' but got: '" + dir + "'");
                        result = MFTestResults.Fail;
                    }
                    catch (ArgumentException) { /* Pass Case */ }
                }
                foreach (char invalidChar in Path.GetInvalidPathChars())
                {
                    try
                    {
                        Log.Comment("Invalid char ascii val = " + (int)invalidChar);
                        string path = new string(new char[] { invalidChar, 'b', 'a', 'd', invalidChar, 'p', 'a', 't', 'h', invalidChar });
                        string dir = Path.GetDirectoryName(path);
                        Log.Exception("Expected Argument exception for '" + path + "' but got: '" + dir + "'");
                        result = MFTestResults.Fail;
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
