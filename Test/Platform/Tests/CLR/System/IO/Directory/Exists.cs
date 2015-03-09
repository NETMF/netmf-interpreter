////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.IO;
using Microsoft.SPOT.Platform.Test;

using System.IO;

namespace Microsoft.SPOT.Platform.Tests
{
    public class Exists : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");
            // Add your functionality here.           
            try
            {
                IOTests.IntializeVolume();
                Directory.CreateDirectory(TestDir);
            }
            catch (Exception ex)
            {
                Log.Comment("Skipping: Unable to initialize file system" + ex.StackTrace);
                return InitializeResult.Skip;
            }     
            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
        }

        #region local vars
        private const string TestDir = "ExistsDirectory";
        #endregion local vars

        #region Helper functions
        private bool TestExists(string path, bool exists)
        {
            Log.Comment("Checking for " + path);
            if (Directory.Exists(path) != exists)
            {
                Log.Exception("Expeceted " + exists + " but got " + !exists);
                return false;
            }
            return true;
        }
        #endregion Helper functions
        #region Test Cases

        [TestMethod]
        public MFTestResults InvalidArguments()
        {
            bool dir;
            MFTestResults result = MFTestResults.Pass;
            try
            {
                try
                {
                    Log.Comment("Null");
                    dir = Directory.Exists(null);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentNullException, but got " + dir);
                }
                catch (ArgumentNullException) { /* pass case */ }
                try
                {
                    Log.Comment("String.Empty");
                    dir = Directory.Exists(String.Empty);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException, but got " + dir);
                }
                catch (ArgumentException) { /* pass case */ }
                try
                {
                    Log.Comment("White Space");
                    dir = Directory.Exists("       ");
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException, but got " + dir);
                }
                catch (ArgumentException) { /* pass case */ }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                result = MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults CurrentDirectory()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestExists(".", true))
                    result = MFTestResults.Fail;

                if (!TestExists("..", true))
                    result = MFTestResults.Fail;

                if (!TestExists(Directory.GetCurrentDirectory(), true))
                    result = MFTestResults.Fail;

                Log.Comment("Set relative Directory");
                Directory.SetCurrentDirectory(TestDir);

                if (!TestExists(".", true))
                    result = MFTestResults.Fail;

                if (!TestExists(Directory.GetCurrentDirectory(), true))
                    result = MFTestResults.Fail;

                if (!TestExists("..", true))
                    result = MFTestResults.Fail;
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                result = MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults NonExistentDirs()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestExists("unknown directory", false))
                    result = MFTestResults.Fail;

                if (!TestExists(IOTests.Volume.RootDirectory + @"\Dir1\dir2", false))
                    result =  MFTestResults.Fail;

                if (!TestExists(@"BAR\", false))
                    result = MFTestResults.Fail;

                try
                {
                    bool test = TestExists("XX:\\", false);
                    Log.Comment("Expected ArgumentException, got " + test);
                    result = MFTestResults.Fail;
                }
                catch (ArgumentException) { /* pass case */ }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                result = MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults PathTooLong()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                string path = new string('x', 500);
                bool exists = Directory.Exists(path);
                Log.Exception("Expected IOException, got " + exists);
                result = MFTestResults.Fail;
            }
            catch (IOException) { /* pass case */ } // PathTooLong
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                result = MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults CaseInsensitive()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Directory.CreateDirectory(TestDir);

                if (!TestExists(TestDir.ToLower(), true))
                    result = MFTestResults.Fail;

                if (!TestExists(TestDir.ToUpper(), true))
                    result = MFTestResults.Fail;
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                result = MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults MultiSpaceExists()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                string dir = Directory.GetCurrentDirectory() + @"\Microsoft Visual Studio .NET\Frame work\V1.0.0.0000";
                Directory.CreateDirectory(dir);

                if (!TestExists(dir, true))
                    result = MFTestResults.Fail;
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
