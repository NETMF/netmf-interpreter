////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;

using System.IO;

namespace Microsoft.SPOT.Platform.Tests
{
    public class Combine : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");
            // Add your functionality here.                

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
        }

        #region helper methods
        private bool TestCombine(string arg1, string arg2, string expected)
        {
            string path = Path.Combine(arg1, arg2);
            Log.Comment("Arg1: '" + arg1 + "'");
            Log.Comment("Arg2: '" + arg2 + "'");
            Log.Comment("Expected: '" + expected + "'");
            if (path != expected)
            {
                Log.Exception("Got: '" + path + "'");
                return false;
            }
            return true;
        }
        # endregion helper methods

        #region Test Cases
        [TestMethod]
        public MFTestResults NullArguments()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Log.Comment("null 1st param");
                string path = Path.Combine(null, "");
                Log.Exception("Expected Argument exception, but got path: " + path);
            }
            catch (ArgumentException) { /* pass case */ }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                result = MFTestResults.Fail;
            }
            try
            {
                Log.Comment("null 2nd param");
                string path = Path.Combine("", null);
                Log.Exception("Expected Argument exception, but got path: " + path);
            }
            catch (ArgumentException) { /* pass case */ }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                result = MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults Root()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                string root = "\\";
                /// Expected output "\\" or root, See remarks section in MSDN Path.Combine
                /// http://msdn.microsoft.com/en-us/library/system.io.path.combine.aspx?PHPSESSID=ca9tbhkv7klmem4g3b2ru2q4d4
                if (!TestCombine(root, root, root))
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
        public MFTestResults SameRoot()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                string root = "\\nand1";
                if (!TestCombine(root, root, root))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestCombine(root + "\\dir", root + "\\dir", root + "\\dir"))
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
        public MFTestResults EmptyArgs()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                string croot = "\\sd1";
                if (!TestCombine(croot, "", croot))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestCombine("", croot, croot))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestCombine("", "", ""))
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
        public MFTestResults TwoUnique()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestCombine("Hello", "World", "Hello\\World"))
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
        public MFTestResults TwoUniqueWithRoot()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestCombine("\\sd1\\Hello\\", "World", "\\sd1\\Hello\\World"))
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
        public MFTestResults SecondBeginWithSlash()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestCombine("\\sd1\\Hello", "\\World", "\\World"))
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
        public MFTestResults UNCName()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                string unc = @"\\\\radt\VbSsDb\VbTests\shadow\FXBCL\test\auto\System_IO\Path\";
                if (!TestCombine(unc, "World", unc + "World"))
                {
                    result = MFTestResults.Fail;
                }

                if (!TestCombine("\\", unc, unc))
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
        public MFTestResults MultipleSubDirs()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestCombine("\\MyDir\\Hello\\", "World\\You\\Are\\My\\Creation", "\\MyDir\\Hello\\World\\You\\Are\\My\\Creation"))
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
        public MFTestResults FirstRootedSecondNot()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestCombine("\\sd1\\MyDirectory\\Sample", "Test", "\\sd1\\MyDirectory\\Sample\\Test"))
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
        public MFTestResults CombineDot()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestCombine("\\sd1\\Directory", ".\\SubDir", "\\sd1\\Directory\\.\\SubDir"))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestCombine("\\sd1\\Directory\\..", "SubDir", "\\sd1\\Directory\\..\\SubDir"))
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
        public MFTestResults WhiteSpace()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                //white space inside
                if (!TestCombine("\\sd1\\Directory Name", "Sub Dir", "\\sd1\\Directory Name\\Sub Dir"))
                {
                    result = MFTestResults.Fail;
                }
                //white space end of arg1
                /// Since path2 is rooted, it is also the expected result. See MSDN remarks section:
                /// http://msdn.microsoft.com/en-us/library/system.io.path.combine.aspx?PHPSESSID=ca9tbhkv7klmem4g3b2ru2q4d4
                if (!TestCombine("\\sd1\\Directory Name\\ ", "\\Sub Dir", "\\Sub Dir"))
                {
                    result = MFTestResults.Fail;
                }
                //white space start of arg2
                if (!TestCombine("\\sd1\\Directory Name", " \\Sub Dir", "\\sd1\\Directory Name\\ \\Sub Dir"))
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
                /// Forward slash is illegal for us, an exception should be thrown.
                TestCombine("//sd1//Directory Name//", "Sub//Dir", "//sd1//Directory Name//Sub//Dir");
                result = MFTestResults.Fail;
            }
            catch (Exception ex)
            {
                Log.Exception("Exception: " + ex.Message);                
            }
            return result;
        }

        #endregion Test Cases
    }
}
