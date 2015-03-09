////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;

using System.IO;

namespace Microsoft.SPOT.Platform.Tests
{
    public class ChangeExtensions : IMFTestInterface
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
        
        #region class vars
        private const string defaultPath = @"de\\fault.path";
        private const string exe = ".exe";
        private const string cool = ".cool";
        #endregion class vars

        #region helper funtions
        private bool TestChangeExtension(String path, String extension)
        {
            string expected = "";
            int iIndex = path.LastIndexOf(".") ;
            if ( iIndex > -1 ){
                switch (extension)
                {
                    case null:
                        expected = path.Substring(0, iIndex);
                        break;
                    case "":
                        expected = path.Substring(0, iIndex + 1);
                        break;
                    default:
                        expected = path.Substring(0, iIndex) + extension;
                        break;
                }
            }        
            else
                expected = path + extension ;

            Log.Comment("Original Path: " + path);
            Log.Comment("Expected Path: " + expected);
            string result = Path.ChangeExtension(path, extension);
            if (result != expected)
            {
                Log.Exception("Got Path: " + result);
                return false;
            }
            return true;
        }
        #endregion helper functions

        #region Test Cases
        [TestMethod]
        public MFTestResults NullArgumentPath()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                string strExtension = Path.ChangeExtension(null, exe);
                Log.Comment("Expect: null");
                if (strExtension != null)
                {
                    Log.Exception("FAIL - Got: " + strExtension);
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
        public MFTestResults NullArgumentExtension()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestChangeExtension(defaultPath, null))
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
        public MFTestResults ZeroLengthPath()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                string strExtension = Path.ChangeExtension("", exe);
                Log.Comment("Expect empty result");
                if ( strExtension != "")
                {
                    Log.Exception("Got: '" + strExtension + "'");
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
        public MFTestResults ZeroLengthExtension()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestChangeExtension(defaultPath, ""))
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
        public MFTestResults StringEmptyPath()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                string strExtension = Path.ChangeExtension("", exe);
                if (strExtension != String.Empty)
                {
                    Log.Exception("Got: '" + strExtension + "'");
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
        public MFTestResults StringEmptyExtension()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestChangeExtension(defaultPath, string.Empty))
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
                string strExtension = Path.ChangeExtension("   ", exe);
                Log.Comment("BUG? - The Desktop has the same behavior, but this is their test case, so don't know right behavior");
                Log.Comment("We will wait to hear back from CLR team to decide what the correct behavior is.");
                Log.Exception("Expected ArgumentException, got " + strExtension);
                result = MFTestResults.KnownFailure;
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
        public MFTestResults InvalidChars()
        {
            MFTestResults result = MFTestResults.Pass;
            foreach (char badChar in Path.InvalidPathChars)
            {
                try
                {
                    string path = new string(new char[] { badChar, 'b', 'a', 'd', badChar, 'p', 'a', 't', 'h', badChar });
                    Log.FilteredComment("Testing path: " + path);
                    string strExtension = Path.ChangeExtension(path, exe);
                }
                catch (ArgumentException) { /* pass case */ }
                catch (Exception ex)
                {
                    Log.Exception("Unexpected exception: " + ex.Message);
                    result = MFTestResults.Fail;
                }
            }
            return result;
        }

        [TestMethod]
        public MFTestResults NoExtensionPath()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                string path = "jabba\\de\\hutt";
                if (! TestChangeExtension(path, exe))
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
        public MFTestResults MultiDotPath()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                string path = "jabba..de..hutt...";
                if (!TestChangeExtension(path, exe))
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
        public MFTestResults ValidExtension()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                string path = "jabba\\de\\hutt.solo";
                if (!TestChangeExtension(path, exe))
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
        public MFTestResults SpecialSymbolPath()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                string path = "foo.bar.fkl;fkds92-509450-4359.213213213@*?2-3203-=210";
                if (!TestChangeExtension(path, cool))
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
        public MFTestResults SpecialSymbolExtension()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                string extension = ".$#@$_)+_)!@@!!@##&_$)#_";
                if (!TestChangeExtension(defaultPath, extension))
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
        public MFTestResults LongExtension()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                string path = new string('a', 256) + exe;
                string extension = "." + new string('b', 256);
                string strExtension = Path.ChangeExtension(path, extension);
                if (!TestChangeExtension(path, extension))
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
        public MFTestResults OneCharExtension()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                string extension = ".z";
                if (!TestChangeExtension(defaultPath, extension))
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

        #endregion Test Cases
    }
}
