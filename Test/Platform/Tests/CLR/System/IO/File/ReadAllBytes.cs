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
    public class RWAllBytes : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests");
            
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
        private const string testDir = "ReadAllBytes";

        #endregion Local vars

        #region Test Cases

        [TestMethod]
        public MFTestResults InvalidArguments()
        {
            MFTestResults result = MFTestResults.Pass;
            byte[] file = null;
            try
            {
                try
                {
                    Log.Comment("ReadAllBytes Null");
                    file = File.ReadAllBytes(null);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException, but got " + file.Length);
                }
                catch (ArgumentException) { /* pass case */ }
                try
                {
                    Log.Comment("ReadAllBytes String.Empty");
                    file = File.ReadAllBytes(String.Empty);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException, but got " + file.Length);
                }
                catch (ArgumentException) { /* pass case */ }
                try
                {
                    Log.Comment("ReadAllBytes White Space");
                    file = File.ReadAllBytes("       ");
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException, but got " + file.Length);
                }
                catch (ArgumentException) { /* pass case */ }

                try
                {
                    Log.Comment("WriteAllBytes Null path");
                    File.WriteAllBytes(null, new byte[10]);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException, but got " + file.Length);
                }
                catch (ArgumentException) { /* pass case */ }
                try
                {
                    Log.Comment("WriteAllBytes Null bytes");
                    File.WriteAllBytes(file1Name, null);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException, but got " + file.Length);
                }
                catch (ArgumentException) { /* pass case */ }
                try
                {
                    Log.Comment("WriteAllBytes String.Empty path");
                    File.WriteAllBytes(String.Empty, new byte[20]);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException, but got " + file.Length);
                }
                catch (ArgumentException) { /* pass case */ }
                try
                {
                    Log.Comment("WriteAllBytes White Space path");
                    File.WriteAllBytes("       ", new byte[30]);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException, but got " + file.Length);
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
        public MFTestResults IOExceptionTests()
        {
            MFTestResults result = MFTestResults.Pass;
            new FileStream(file1Name, FileMode.Create);
            Log.Comment("Current Directory: " + Directory.GetCurrentDirectory());
            try
            {

                try
                {
                    Log.Comment("non-existent file");
                    File.ReadAllBytes("non-existent.file");
                    result = MFTestResults.Fail;
                    Log.Exception("Expected IOException");
                }
                catch (IOException) { /* pass case */ }

                try
                {
                    Log.Comment("ReadOnly file");
                    File.SetAttributes(file1Name, FileAttributes.ReadOnly);
                    File.WriteAllBytes(file1Name, new byte[4]);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected IOException");
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

            return result;
        }

        [TestMethod]
        public MFTestResults ValidCase()
        {
            MFTestResults result = MFTestResults.Pass;
            byte[] writebytes;
            byte[] readbytes;
            try
            {
                // Clean up
                if (File.Exists(file2Name))
                    File.Delete(file2Name);

                Log.Comment("Create 0 length file");
                File.Create(file2Name).Close();

                Log.Comment("Read all bytes - expect 0");
                readbytes = File.ReadAllBytes(file2Name);
                if (readbytes.Length != 0)
                {
                    result = MFTestResults.Fail;
                    Log.Exception("Got " + readbytes.Length + " bytes!");
                }

                Log.Comment("Write bytes 0-255 to file");
                writebytes = new byte[256];
                for (int i = 0; i < writebytes.Length; i++)
                {
                    writebytes[i] = (byte)i;
                }
                File.WriteAllBytes(file2Name, writebytes);

                Log.Comment("Read all bytes - expect 256");
                readbytes = File.ReadAllBytes(file2Name);
                if (readbytes.Length != writebytes.Length)
                {
                    result = MFTestResults.Fail;
                    Log.Exception("Read unexpected number of bytes: " + readbytes.Length);
                }
                for (int i = 0; i < readbytes.Length; i++)
                {
                    if (readbytes[i] != i)
                    {
                        result = MFTestResults.Fail;
                        Log.Exception("Expected byte " + i + " but got byte " + readbytes[i]);
                    }
                }

                Log.Comment("Write again, 255-0");
                writebytes = new byte[256];
                for (int i = 0; i < writebytes.Length; i++)
                {
                    writebytes[i] = (byte)~(i);
                }
                File.WriteAllBytes(file2Name, writebytes);

                Log.Comment("Read all bytes - expect 256");
                readbytes = File.ReadAllBytes(file2Name);
                if (readbytes.Length != writebytes.Length)
                {
                    result = MFTestResults.Fail;
                    Log.Exception("Read unexpected number of bytes: " + readbytes.Length);
                }
                for (int i = 0; i < readbytes.Length; i++)
                {
                    if (readbytes[i] != (byte)~(i))
                    {
                        result = MFTestResults.Fail;
                        Log.Exception("Expected byte " + (byte)~(i) + " but got byte " + readbytes[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                Log.Exception("Stack: " + ex.StackTrace);
                result = MFTestResults.Fail;
            }

            return result;
        }
        #endregion Test Cases
    }
}
