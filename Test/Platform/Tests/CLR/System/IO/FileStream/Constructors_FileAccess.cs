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
    public class Constructors_FileAccess : IMFTestInterface
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
        private const string TestDir = "FileAccess";
        private const string fileName = "test.tmp";
        #endregion local vars

        #region Test Cases
        [TestMethod]
        public MFTestResults InvalidArguments()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                try
                {
                    Log.Comment("Bad fileaccess -1");
                    using (FileStream fs = new FileStream(fileName, FileMode.Create, (FileAccess)(-1))) { }
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException");
                }
                catch (ArgumentException) { /* pass case */ }

                try
                {
                    Log.Comment("Bad fileaccess 10");
                    using (FileStream fs = new FileStream(fileName, FileMode.Create, (FileAccess)(10))) { }
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
        public MFTestResults FileAccess_Read()
        {
            // Clean up incase file exists
            if (File.Exists(fileName))
                File.Delete(fileName);

            MFTestResults result = MFTestResults.Pass;
            try
            {
                // From MSDN:
                // Read access to the file. Data can be read from the file. 
                // Combine with Write for read/write access. 

                Log.Comment("Create file for testing");
                using (FileStream fs = new FileStream(fileName, FileMode.CreateNew))
                {
                    FileStreamHelper.WriteReadEmpty(fs);
                }

                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    Log.Comment("Should be able to read");
                    if (!FileStreamHelper.VerifyRead(fs))
                        result = MFTestResults.Fail;

                    Log.Comment("Shouldn't be able to write");
                    try
                    {
                        fs.Write(new byte[] { 1, 2, 3 }, 0, 3);
                        result = MFTestResults.Fail;
                        Log.Exception("Expected NotSupportedException");
                    }
                    catch (NotSupportedException) { /* pass case */ }
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
        public MFTestResults FileAccess_Write()
        {
            // Clean up incase file exists
            if (File.Exists(fileName))
                File.Delete(fileName);

            MFTestResults result = MFTestResults.Pass;
            try
            {
                // From MSDN:
                // Write access to the file. Data can be written to the file. 
                // Combine with Read for read/write access.

                Log.Comment("Create file for testing");
                using (FileStream fs = new FileStream(fileName, FileMode.CreateNew))
                {
                    FileStreamHelper.WriteReadEmpty(fs);
                }

                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Write))
                {
                    Log.Comment("Should be able to write");
                    if (!FileStreamHelper.Write(fs, 1000))
                        result = MFTestResults.Fail;

                    if (!FileStreamHelper.Write(fs, 500))
                        result = MFTestResults.Fail;

                    Log.Comment("Shouldn't be able to read");
                    try
                    {
                        fs.Seek(0, SeekOrigin.Begin);
                        int data = fs.ReadByte();
                        result = MFTestResults.Fail;
                        Log.Exception("Expected NotSupportedException");
                    }
                    catch (NotSupportedException) { /* pass case */ }
                }

                // 300 bytes original + 1000 bytes + 500 bytes
                if (!FileStreamHelper.VerifyFile(fileName, 1800))
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
        public MFTestResults FileAccess_ReadWrite()
        {
            // Clean up incase file exists
            if (File.Exists(fileName))
                File.Delete(fileName);

            MFTestResults result = MFTestResults.Pass;
            try
            {
                // From MSDN:
                // Write access to the file. Data can be written to the file. 
                // Combine with Read for read/write access.

                Log.Comment("Create file for testing");
                using (FileStream fs = new FileStream(fileName, FileMode.CreateNew))
                {
                    FileStreamHelper.WriteReadEmpty(fs);
                }

                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite))
                {
                    Log.Comment("Should be able to write");
                    if (!FileStreamHelper.Write(fs, 1000))
                        result = MFTestResults.Fail;

                    Log.Comment("Should be able to read");
                    fs.Seek(0, SeekOrigin.Begin);
                    if (!FileStreamHelper.VerifyRead(fs))
                        result = MFTestResults.Fail;

                    Log.Comment("Write after read");
                    fs.Seek(0, SeekOrigin.End);
                    if (!FileStreamHelper.Write(fs, 500))
                        result = MFTestResults.Fail;
                }

                // 300 bytes original + 1000 bytes + 500 bytes
                if (!FileStreamHelper.VerifyFile(fileName, 1800))
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
