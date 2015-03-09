////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;

using System.IO;

namespace Microsoft.SPOT.Platform.Tests
{
    public class GetFullPath : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Set up for the tests");
            try
            {
                IOTests.IntializeVolume();
            }
            catch
            {
                return InitializeResult.Skip;
            }
            return InitializeResult.ReadyToGo;
        }
        [TearDown]
        public void CleanUp()
        {
        }

        #region helper methods

        private bool TestGetFullPath(string path, string expected)
        {
            string result = Path.GetFullPath(path);
            Log.Comment("Path: " + path);
            Log.Comment("Expected: " + expected);
            if (result != expected)
            {
                Log.Exception("Got: " + result);
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
                try
                {
                    string path = Path.GetFullPath("");
                    Log.Exception("Expected ArgumentException exception, but got: " + path);
                    result = MFTestResults.Fail;
                }
                catch (ArgumentException) { /* Pass Case */ }
                try
                {
                    string path = Path.GetFullPath(string.Empty);
                    Log.Exception("Expected ArgumentException exception, but got: " + path);
                    result = MFTestResults.Fail;
                }
                catch (ArgumentException) { /* Pass Case */ }
                try
                {
                    string path = Path.GetFullPath(null);
                    Log.Exception("Expected ArgumentNullException exception, but got: " + path);
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
        public MFTestResults ValidCases()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestGetFullPath(@"file.tmp", Directory.GetCurrentDirectory() + @"\file.tmp"))
                {
                    result = MFTestResults.Fail;
                }
                // unrooted
                if (!TestGetFullPath(@"directory\file.tmp", Directory.GetCurrentDirectory() + @"\directory\file.tmp"))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestGetFullPath(@"\ROOT\directory\file.tmp", @"\ROOT\directory\file.tmp"))
                {
                    result = MFTestResults.Fail;
                }
                // rooted
                if (!TestGetFullPath(@"\\machine\directory\file.tmp", @"\\machine\directory\file.tmp"))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestGetFullPath(@"\sd1\directory\file", @"\sd1\directory\file"))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestGetFullPath(@"\nand1\directory name\file name", @"\nand1\directory name\file name"))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestGetFullPath(@"\sd1\directory.t name\file.t name.exe", @"\sd1\directory.t name\file.t name.exe"))
                {
                    result = MFTestResults.Fail;
                }

                // special - Might need actuall FS access to create directory structures to navigate
                if (!TestGetFullPath(".", Directory.GetCurrentDirectory()))
                {
                    result = MFTestResults.Fail;
                }
                if (!TestGetFullPath(@".\", Directory.GetCurrentDirectory()))
                {
                    result = MFTestResults.Fail;
                }
                //if (!TestGetFullPath("..", ""))
                //{
                //    result = MFTestResults.Fail;
                //}
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                result = MFTestResults.Fail;
            }
            return result;
        }

        [TestMethod]
        public MFTestResults TooLongPath()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                string longBlock = new string('y', 135);
                try
                {
                    string path = Path.GetFullPath(longBlock + longBlock);
                    Log.Exception("Expected IOException(Path Too Long), but got: " + path);
                    result = MFTestResults.Fail;
                }
                catch (IOException) { /* Pass Case */ } // PathTooLong
                try
                {
                    string path = Path.GetFullPath(@"\SD1\" + longBlock + "\\" + longBlock + "\\" + longBlock + longBlock + ".exe");
                    Log.Exception("Expected IOException(Path Too Long), but got: " + path);
                    result = MFTestResults.Fail;
                }
                catch (IOException) { /* Pass Case */ } //PathTooLong
                try
                {
                    string path = Path.GetFullPath(new string('a', (int)UInt16.MaxValue + 1));
                    Log.Exception("Expected IOException(Path Too Long), but got: " + path);
                    result = MFTestResults.Fail;
                }
                catch (IOException) { /* Pass Case */ } //PathTooLong

                // chech bounds
                string boundResult;
                Directory.SetCurrentDirectory(IOTests.Volume.RootDirectory);
                string currDir = Directory.GetCurrentDirectory();
                int len = currDir.Length;
                int limit = len + 258; // 258 is the FsMaxPathLength

                if (currDir.Substring(currDir.Length - 1) != (Path.DirectorySeparatorChar.ToString()))
                    len++;

                for (int i = 225 - len; i < 275 - len; i++)
                {
                    try
                    {
                        string str1 = new String('a', 100) + "\\" + new String('a', i- 101); // make a string of i length (need a \ in there so we don't have filename over 255)
                        boundResult = Path.GetFullPath(str1);
                        if (boundResult.Length >= limit) 
                        {
                            result = MFTestResults.Fail;
                            Log.Exception("Err_3974g! Didn't Throw: " + (i + len) + " - " + boundResult.Length);
                        }
                    }
                    catch (IOException) // PathTooLong
                    {
                        if ((len + i) < limit)
                        {
                            result = MFTestResults.Fail;
                            Log.Exception("Err_245f! Threw too early: " + (i + len));
                        }
                    }
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
        public MFTestResults WildCard()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                try
                {
                    string path = Path.GetFullPath("file*");
                    Log.Exception("Expected ArgumentException exception, but got: " + path);
                    result = MFTestResults.Fail;
                }
                catch (ArgumentException) { /* Pass Case */ }
                try
                {
                    string path = Path.GetFullPath(@"\sd1\file*");
                    Log.Exception("Expected ArgumentException exception, but got: " + path);
                    result = MFTestResults.Fail;
                }
                catch (ArgumentException) { /* Pass Case */ }
                try
                {
                    string path = Path.GetFullPath(@"\sd1\file*.txt");
                    Log.Exception("Expected ArgumentException exception, but got: " + path);
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
                        string path = new string(new char[] { 'b', 'a', 'd', '\\', invalidChar, 'p', 'a', 't', 'h', invalidChar, '.', 't', 'x', 't' });
                        string dir = Path.GetFullPath(path);
                        if (invalidChar == 0)
                        {
                            Log.Exception("[Known issue] Expected Argument exception for for '" + path + "' but got: '" + dir + "'");
                            result = MFTestResults.KnownFailure;
                        }
                        else
                        {
                            Log.Exception("Expected Argument exception for '" + path + "' but got: '" + dir + "'");
                            result = MFTestResults.Fail;
                        }
                    }
                    catch (ArgumentException) { /* Pass Case */ }
                    catch (Exception)
                    {
                        /// There is one case where String.Split throws System.Exception instead of ArgumentException for certain invalid character.
                        /// Fix String.Split?
                    }
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
