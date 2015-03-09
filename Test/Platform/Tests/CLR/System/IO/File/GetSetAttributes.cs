////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using Microsoft.SPOT;
using Microsoft.SPOT.IO;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class GetSetAttributes : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            // These tests rely on underlying file system so we need to make
            // sure we can format it before we start the tests.  If we can't
            // format it, then we assume there is no FS to test on this platform.

            // delete the directory DOTNETMF_FS_EMULATION
            try
            {
                IOTests.IntializeVolume();

                Directory.CreateDirectory(testDir);
                Directory.SetCurrentDirectory(testDir);
                File.Create(file1Name).Close();
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

        #region Local vars
        private const string file1Name = "File1.tmp";
        private const string testDir = "GetAttributesDir";
        #endregion Local vars

        #region Helper methods

        private bool TestSetGetAttributes(string path, FileAttributes expected)
        {
            Log.Comment("Setting file " + path + " to attribute " + expected);
            File.SetAttributes(path, expected);
            return TestGetAttributes(path, expected);
        }
        private bool TestGetAttributes(string path, FileAttributes expected)
        {
            Log.Comment("Checking file " + path + " for attribute " + expected);
            FileAttributes fa = File.GetAttributes(path);
            if (fa != expected)
            {
                Log.Exception("Unexpected value - got " + fa);
                return false;
            }
            return true;
        }
        #endregion Helper methods

        #region Test Cases

        [TestMethod]
        public MFTestResults InvalidArguments()
        {
            FileAttributes file;
            MFTestResults result = MFTestResults.Pass;
            try
            {
                try
                {
                    Log.Comment("Get Null");
                    file = File.GetAttributes(null);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException, but got " + file);
                }
                catch (ArgumentException) { /* pass case */ }
                try
                {
                    Log.Comment("Get String.Empty");
                    file = File.GetAttributes(String.Empty);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException, but got " + file);
                }
                catch (ArgumentException) { /* pass case */ }
                try
                {
                    Log.Comment("Get White Space");
                    file = File.GetAttributes("       ");
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException, but got " + file);
                }
                catch (ArgumentException) { /* pass case */ }

                try
                {
                    Log.Comment("Set Null");
                    File.SetAttributes(null, FileAttributes.Normal);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException");
                }
                catch (ArgumentException) { /* pass case */ }
                try
                {
                    Log.Comment("Set String.Empty");
                    File.SetAttributes(String.Empty, FileAttributes.Normal);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException");
                }
                catch (ArgumentException) { /* pass case */ }
                try
                {
                    Log.Comment("Set White Space");
                    File.SetAttributes("       ", FileAttributes.Normal);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException");
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
        public MFTestResults IOExceptions()
        {
            FileAttributes file;
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Log.Comment("Get Check Directory");
                file = File.GetAttributes(Directory.GetCurrentDirectory()); 
                
                try
                {
                    Log.Comment("Get non-existent file");
                    file = File.GetAttributes("non-existent");
                    result = MFTestResults.Fail;
                }
                catch (IOException) { /* pass case */ } // FileNotFound

                Log.Comment("Set Check Directory");
                File.SetAttributes(Directory.GetCurrentDirectory(), FileAttributes.Normal);                     

                try
                {
                    Log.Comment("Set non-existent file");
                    File.SetAttributes("non-existent", FileAttributes.Normal);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected IOException");
                }
                catch (IOException) { /* pass case */ }

                Log.Comment("Set Directory");
                File.SetAttributes(file1Name, FileAttributes.Directory);                 

                Log.Comment("Set Normal | ReadOnly");
                File.SetAttributes(file1Name, FileAttributes.Normal | FileAttributes.ReadOnly);                 

                Log.Comment("Set Normal | Hidden");
                File.SetAttributes(file1Name, FileAttributes.Normal | FileAttributes.Hidden);

                result = MFTestResults.Pass;
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
                /// Initial status is Hidden because of the test above.
                /// Log.Comment("Default Normal attribute");
                /// if (!TestGetAttributes(file1Name, FileAttributes.Normal))
                ///     result = MFTestResults.Fail;

                Log.Comment("Read Only attribute");
                if (!TestSetGetAttributes(file1Name, FileAttributes.ReadOnly))
                    result = MFTestResults.Fail;

                Log.Comment("Hidden attribute");
                if (!TestSetGetAttributes(file1Name, FileAttributes.Hidden))
                    result = MFTestResults.Fail;

                Log.Comment("ReadOnly & Hidden attribute");
                if (!TestSetGetAttributes(file1Name, FileAttributes.Hidden | FileAttributes.ReadOnly))
                    result = MFTestResults.Fail;

                Log.Comment("Back to Normal attribute");
                if (!TestSetGetAttributes(file1Name, FileAttributes.Normal))
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
