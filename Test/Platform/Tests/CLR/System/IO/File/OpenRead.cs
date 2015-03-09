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
    public class OpenRead : IMFTestInterface
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
        private const string testDir = "OpenRead";

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
                    file = File.OpenRead(null);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException, but got " + file.Name);
                }
                catch (ArgumentException) { /* pass case */ }
                try
                {
                    Log.Comment("String.Empty");
                    file = File.OpenRead(String.Empty);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException, but got " + file.Name);
                }
                catch (ArgumentException) { /* pass case */ }
                try
                {
                    Log.Comment("White Space");
                    file = File.OpenRead("       ");
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
            FileStream fs = null;
            try
            {
                Log.Comment("Current Directory: " + Directory.GetCurrentDirectory());
                try
                {
                    Log.Comment("non-existent file");
                    fs = File.OpenRead("non-existent.file");
                    result = MFTestResults.Fail;
                    Log.Exception("Expected IOException");
                }
                catch (IOException) { /* pass case */ }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                result = MFTestResults.Fail;
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
                Log.Comment("Create file, and write string to it");
                fs1 = new FileStream(file2Name, FileMode.Create);
                fs1.Write(writebytes, 0, writebytes.Length);
                fs1.Close();

                Log.Comment("OpenRead file");
                fs1 = File.OpenRead(file2Name);

                Log.Comment("Try to read from file");
                if (!fs1.CanRead)
                {
                    result = MFTestResults.Fail;
                    Log.Exception("Expected CanRead to be true!");
                }
                int read = fs1.Read(readbytes, 0, readbytes.Length);
                if (read != writebytes.Length)
                {
                    result = MFTestResults.Fail;
                    Log.Exception("Expected " + writebytes.Length + " bytes, but read " + read + " bytes");
                }
                string readStr = new string(UTF8Encoding.UTF8.GetChars(readbytes));
                if (file2Name != readStr)
                {
                    result = MFTestResults.Fail;
                    Log.Exception("Unexpected read data string: " + readStr + " - Expected: " + file2Name);
                }

                Log.Comment("Try to write to file");
                if (fs1.CanWrite)
                {
                    result = MFTestResults.Fail;
                    Log.Exception("Expected CanWrite to be false!");
                }
                try
                {
                    fs1.Write(writebytes, 0, writebytes.Length);
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
