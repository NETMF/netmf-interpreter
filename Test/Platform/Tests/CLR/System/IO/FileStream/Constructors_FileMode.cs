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
    public class Constructors_FileMode : IMFTestInterface
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
                Directory.CreateDirectory(TestDir);
                Directory.SetCurrentDirectory(TestDir);
            }
            catch (Exception ex)
            {
                Log.Exception("Skipping: Unable to initialize file system " + ex.StackTrace);
                return InitializeResult.Skip;
            }
            return InitializeResult.ReadyToGo;
        }


        [TearDown]
        public void CleanUp()
        {
        }

        #region local vars
        private const string TestDir = "FileMode";
        private const string fileName = "test.tmp";
        #endregion local vars

        #region Helper Methods

        #endregion Helper Methods

        #region Test Cases
        [TestMethod]
        public MFTestResults InvalidArguments()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                try
                {
                    Log.Comment("Null Filename");
                    using (FileStream fs = new FileStream(null, FileMode.Create)) { }
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentNullException");
                }
                catch (ArgumentNullException) { /* pass case */ }

                try
                {
                    Log.Comment("String.Empty Filename");
                    using (FileStream fs = new FileStream(String.Empty, FileMode.Create)) { }
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException");
                }
                catch (ArgumentException) { /* pass case */ }

                try
                {
                    Log.Comment("White Space FileName");
                    using (FileStream fs = new FileStream("       ", FileMode.Create)) { }
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException");
                }
                catch (ArgumentException) { /* pass case */ }

                try
                {
                    Log.Comment(". FileName");
                    using (FileStream fs = new FileStream(".", FileMode.Create)) { }
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException");
                }
                catch (IOException) { /* pass case */ }

                try
                {
                    Log.Comment("Use Directory for FileName");
                    using (FileStream fs = new FileStream(IOTests.Volume.RootDirectory + @"\" + TestDir, FileMode.Create)) { }
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException");
                }
                catch (IOException) { /* pass case */ }

                try
                {
                    Log.Comment("Bad filemode -1");
                    using (FileStream fs = new FileStream(fileName, (FileMode)(-1))) { }
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException");
                }
                catch (ArgumentException) { /* pass case */ }

                try
                {
                    Log.Comment("Bad filemode 10");
                    using (FileStream fs = new FileStream(fileName, (FileMode)(10))) { }
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException");
                }
                catch (ArgumentException) { /* pass case */ }

                try
                {
                    Log.Comment("Negative buffer");
                    using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, -1)) { }
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
        public MFTestResults FileMode_Append()
        {
            // Clean up incase file exists
            if (File.Exists(fileName))
                File.Delete(fileName);

            MFTestResults result = MFTestResults.Pass;
            try
            {
                // From MSDN:
                // Opens the file if it exists and seeks to the end of the file, 
                // or creates a new file. FileMode.Append can only be used in 
                // conjunction with FileAccess.Write. Attempting to seek to a position 
                // before the end of the file will throw an IOException and any attempt 
                // to read fails and throws an NotSupportedException. 
                
                Log.Comment("Non-existent file, should create file");
                using (FileStream fs = new FileStream(fileName, FileMode.Append))
                {
                    fs.WriteByte((byte)0);
                    fs.WriteByte((byte)1);
                    fs.WriteByte((byte)2);
                }

                Log.Comment("Existing file, should seek to end on open");
                using (FileStream fs = new FileStream(fileName, FileMode.Append))
                {
                    if (fs.Position != 3)
                    {
                        result = MFTestResults.Fail;
                        Log.Exception("Expected postion 3, but got " + fs.Position);
                    }

                    Log.Comment("Try to Seek before append");
                    try
                    {
                        fs.Seek(1, SeekOrigin.Begin);
                        result = MFTestResults.Fail;
                        Log.Exception("Unexpectedly able to seek");
                    }
                    catch (IOException) { /* pass case */ }

                    Log.Comment("Try to read");
                    try
                    {
                        byte[] buff = new byte[1];
                        fs.Read(buff, 0, buff.Length);
                        result = MFTestResults.Fail;
                        Log.Exception("Unexpectedly able to read");
                    }
                    catch (NotSupportedException) { /* pass case */ }

                    Log.Comment("append to file");
                    fs.WriteByte((byte)3);
                    fs.WriteByte((byte)4);
                    fs.WriteByte((byte)5);

                    Log.Comment("try to seek to start of append");
                    fs.Seek(-3, SeekOrigin.End);
                }

                if (!FileStreamHelper.VerifyFile(fileName, 6))
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
        public MFTestResults FileMode_Create()
        {
            // Clean up incase file exists
            if (File.Exists(fileName))
                File.Delete(fileName);

            MFTestResults result = MFTestResults.Pass;
            try
            {
                // From MSDN:
                // Specifies that the operating system should create a new file. 
                // If the file already exists, it will be overwritten. This requires 
                // FileIOPermissionAccess..::.Write. System.IO.FileMode.Create is 
                // equivalent to requesting that if the file does not exist, use 
                // CreateNew; otherwise, use Truncate. 

                Log.Comment("Non-existent file, should create file");
                using (FileStream fs = new FileStream(fileName, FileMode.Create))
                {
                    if (!FileStreamHelper.WriteReadEmpty(fs))
                        result = MFTestResults.Fail;

                    // add additional 50 bytes, to test that truncation really happens
                    FileStreamHelper.Write(fs, 50);
                }

                Log.Comment("Add Attribute, to verify Truncate behavior");
                File.SetAttributes(fileName, FileAttributes.Hidden);

                Log.Comment("Existing file, should treat as truncate");
                using (FileStream fs = new FileStream(fileName, FileMode.Create))
                {
                    if (!FileStreamHelper.Write(fs, 300))
                        result = MFTestResults.Fail;

                    Log.Comment("Trying to read file while its open Truncate should fail");
                    try
                    {
                        using (FileStream fs2 = new FileStream(fileName, FileMode.Open, FileAccess.Read)) { }
                        result = MFTestResults.Fail;
                        Log.Exception("Unexpectedly able to read file");
                    }
                    catch (IOException) { /* pass case */ }
                }

                Log.Comment("Verify hidden property is still set");
                FileAttributes fa = File.GetAttributes(fileName);
                if ((fa & FileAttributes.Hidden) == 0)
                {
                    result = MFTestResults.Fail;
                    Log.Exception("Expected hidden attribute, but got " + fa);
                }

                // Verify the 300 bytes we wrote (not original 350)
                if (!FileStreamHelper.VerifyFile(fileName, 300))
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
        public MFTestResults FileMode_CreateNew()
        {
            // Clean up incase file exists
            if (File.Exists(fileName))
                File.Delete(fileName);

            MFTestResults result = MFTestResults.Pass;
            try
            {
                // From MSDN:
                // Specifies that the operating system should create a new file. 
                // This requires FileIOPermissionAccess..::.Write. If the file 
                // already exists, an IOException is thrown. 

                Log.Comment("Non-existent file, should create file");
                using (FileStream fs = new FileStream(fileName, FileMode.CreateNew))
                {
                    if (!FileStreamHelper.WriteReadEmpty(fs))
                        result = MFTestResults.Fail;
                }

                if (!FileStreamHelper.VerifyFile(fileName, 300))
                    result = MFTestResults.Fail;

                try
                {
                    Log.Comment("Exists file CreateNew");
                    using (FileStream fs = new FileStream(fileName, FileMode.CreateNew)) { }
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
        public MFTestResults FileMode_Open()
        {
            // Clean up incase file exists
            if (File.Exists(fileName))
                File.Delete(fileName);

            MFTestResults result = MFTestResults.Pass;
            try
            {
                // From MSDN:
                // Specifies that the operating system should open an existing file. 
                // The ability to open the file is dependent on the value specified 
                // by FileAccess. A System.IO..::.FileNotFoundException is thrown
                // if the file does not exist. 

                try
                {
                    Log.Comment("Non-existent file open should throw");
                    using (FileStream fs = new FileStream(fileName, FileMode.Open)) { }
                    result = MFTestResults.Fail;
                    Log.Exception("Expected IOException");
                }
                catch (IOException) { /* pass case */ }

                // Create file
                File.Create(fileName).Close();

                Log.Comment("Open existing file");
                using (FileStream fs = new FileStream(fileName, FileMode.Open))
                {
                    if (!FileStreamHelper.WriteReadEmpty(fs))
                        result = MFTestResults.Fail;
                }

                if (!FileStreamHelper.VerifyFile(fileName, 300))
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
        public MFTestResults FileMode_OpenOrCreate()
        {
            // Clean up incase file exists
            if (File.Exists(fileName))
                File.Delete(fileName);

            MFTestResults result = MFTestResults.Pass;
            try
            {
                // From MSDN:
                // Specifies that the operating system should open a file if it exists; otherwise,
                // a new file should be created. If the file is opened with FileAccess.Read, 
                // FileIOPermissionAccess..::.Read is required. If the file access is 
                // FileAccess.Write then FileIOPermissionAccess..::.Write is required. If the file 
                // is opened with FileAccess.ReadWrite, both FileIOPermissionAccess..::.Read and 
                // FileIOPermissionAccess..::.Write are required. If the file access is 
                // FileAccess.Append, then FileIOPermissionAccess..::.Append is required. 

                Log.Comment("Non-existent file, should create file");
                using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate))
                {
                    if (!FileStreamHelper.WriteReadEmpty(fs))
                        result = MFTestResults.Fail;
                }

                Log.Comment("Open existing file");
                using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate))
                {
                    if (!FileStreamHelper.WriteReadVerify(fs))
                        result = MFTestResults.Fail;
                }

                if (!FileStreamHelper.VerifyFile(fileName, 600))
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
        public MFTestResults FileMode_Truncate()
        {
            // Clean up incase file exists
            if (File.Exists(fileName))
                File.Delete(fileName);

            MFTestResults result = MFTestResults.Pass;
            try
            {
                // From MSDN:
                // Specifies that the operating system should open an existing file. 
                // Once opened, the file should be truncated so that its size is zero 
                // bytes. This requires FileIOPermissionAccess..::.Write. Attempts to 
                // read from a file opened with Truncate cause an exception. 

                try
                {
                    Log.Comment("Non-existent file truncate");
                    using (FileStream fs = new FileStream(fileName, FileMode.Truncate)) { }
                    result = MFTestResults.Fail;
                    Log.Exception("Expected IOException");
                }
                catch (IOException) { /* pass case */ }

                Log.Comment("Create a new file and populate");
                using (FileStream fs = new FileStream(fileName, FileMode.CreateNew))
                {
                    if (!FileStreamHelper.WriteReadVerify(fs))
                        result = MFTestResults.Fail;

                    // add extra bytes, to make sure we really truncate
                    FileStreamHelper.Write(fs, 50);
                }
                Log.Comment("Add Attribute, to verify Truncate behavior");
                File.SetAttributes(fileName, FileAttributes.Hidden);

                Log.Comment("Open Truncate");
                using (FileStream fs = new FileStream(fileName, FileMode.Truncate))
                {
                    if (!FileStreamHelper.WriteReadEmpty(fs))
                        result = MFTestResults.Fail;

                    Log.Comment("Trying to read file while its open Truncate should fail");
                    try
                    {
                        using (FileStream fs2 = new FileStream(fileName, FileMode.Open, FileAccess.Read)) {}
                        result = MFTestResults.Fail;
                        Log.Exception("Unexpectedly able to read file");
                    }
                    catch (IOException) { /* pass case */ }
                }

                Log.Comment("Verify hidden property is still set");
                FileAttributes fa = File.GetAttributes(fileName);
                if ((fa & FileAttributes.Hidden) == 0)
                {
                    result = MFTestResults.Fail;
                    Log.Exception("Expected hidden attribute, but got " + fa);
                }

                if (!FileStreamHelper.VerifyFile(fileName, 300))
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
