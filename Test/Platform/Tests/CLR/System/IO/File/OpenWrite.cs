////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using System.Text;
using Microsoft.SPOT;
using Microsoft.SPOT.IO;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class OpenWrite : IMFTestInterface
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
        private const string file1Name = "file1.tmp";
        private const string file2Name = "file2.txt";
        private const string testDir = "OpenWrite";

        #endregion Local vars

        #region Test Cases

        [TestMethod]
        public MFTestResults InvalidArguments()
        {
            MFTestResults result = MFTestResults.Pass;
            FileStream file = null;
            try
            {
                try
                {
                    Log.Comment("Null");
                    file = File.OpenWrite(null);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException, but got " + file.Name);
                }
                catch (ArgumentException) { /* pass case */ }
                try
                {
                    Log.Comment("String.Empty");
                    file = File.OpenWrite(String.Empty);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException, but got " + file.Name);
                }
                catch (ArgumentException) { /* pass case */ }
                try
                {
                    Log.Comment("White Space");
                    file = File.OpenWrite("       ");
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException, but got " + file.Name);
                }
                catch (ArgumentException) { /* pass case */ }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                result = MFTestResults.Fail;
            }
            finally
            {
                if (file != null)
                    file.Close();
            }

            return result;
        }

        [TestMethod]
        public MFTestResults IOExceptionTests()
        {
            MFTestResults result = MFTestResults.Pass;
            new FileStream(file1Name, FileMode.Create).Close();
            try
            {
                Log.Comment("Current Directory: " + Directory.GetCurrentDirectory());
                try
                {
                    Log.Comment("ReadOnly file");
                    File.SetAttributes(file1Name, FileAttributes.ReadOnly);
                    FileStream fs = File.OpenWrite(file1Name);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected IOException");
                    fs.Close();
                }
                catch (IOException)
                {
                    /// Validate IOException.ErrorCode.
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                result = MFTestResults.Fail;
            }
            finally
            {
                // clean up
                File.SetAttributes(file1Name, FileAttributes.Normal);
                File.Delete(file1Name);
            }


            return result;
        }

        [TestMethod]
        public MFTestResults ValidCase()
        {
            MFTestResults result = MFTestResults.Pass;
            FileStream fs1 = null;
            byte[] writebytes = Encoding.UTF8.GetBytes(file2Name);
            byte[] readbytes = new byte[writebytes.Length + 10];
            try
            {
                // Clean up
                if (File.Exists(file2Name))
                    File.Delete(file2Name);

                Log.Comment("OpenWrite file");
                fs1 = File.OpenWrite(file2Name);

                Log.Comment("Try to write to file");
                if (!fs1.CanWrite)
                {
                    result = MFTestResults.Fail;
                    Log.Exception("Expected CanWrite to be true!");
                }
                fs1.Write(writebytes, 0, writebytes.Length);


                Log.Comment("Try to read from file");
                if (fs1.CanRead)
                {
                    result = MFTestResults.Fail;
                    Log.Exception("Expected CanRead to be false!");
                }
                try
                {
                    fs1.Read(readbytes, 0, readbytes.Length);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected IOException");
                }
                catch (NotSupportedException) { /* pass case */ }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                Log.Exception("Stack: " + ex.StackTrace);
                result = MFTestResults.Fail;
            }
            finally
            {
                if (fs1 != null)
                    fs1.Close();
            }

            return result;
        }
        #endregion Test Cases
    }
}
