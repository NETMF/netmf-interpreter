////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.IO;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class Constructors_FileShare : IMFTestInterface
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
        private const string TestDir = "FileShare";
        private const string fileName = "test.tmp";
        private bool threadResult;
        private bool restricted;
        public struct MatrixTestCase
        {
            public FileShare previousShare;
            public FileAccess requestedAccess;
            public FileShare requestedShare;
            public bool expectSuccess;
            public MatrixTestCase(FileShare PreviousShare, FileAccess RequestedAccess, FileShare RequestedShare, bool ExpectSuccess)
            {
                previousShare = PreviousShare;
                requestedAccess = RequestedAccess;
                requestedShare = RequestedShare;
                expectSuccess = ExpectSuccess;
            }
        }
        #endregion local vars

        #region Helper methods
        private void OpenRestrictedFile()
        {
            threadResult = false;
            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)) { }
                Log.Exception("No exception!  Expected IOException");
            }
            catch (IOException) 
            { 
                /* pass case */ 
                threadResult = true; 
            }
            catch (Exception ex) 
            { 
                Log.Exception("Expected IOException, but got " + ex.Message); 
            }
        }
        private void OpenRead()
        {
            try
            {
                threadResult &= FileStreamHelper.VerifyFile(fileName);
            }
            catch (IOException)
            {
                if (!restricted)
                {
                    threadResult = false;
                    Log.Exception("Unexpected IOException");
                }
            }
            catch (Exception ex)
            {
                threadResult = false;
                Log.Exception("Unexpected Exception " + ex.Message);
            }
        }
        private void OpenWrite()
        {
            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
                {
                    threadResult &= FileStreamHelper.Write(fs, 200);
                }
            }
            catch (IOException)
            {
                if (!restricted)
                {
                    threadResult = false;
                    Log.Exception("Unexpected IOException");
                }
            }
            catch (Exception ex)
            {
                threadResult = false;
                Log.Exception("Unexpected Exception " + ex.Message);
            }
        }

        private void OpenReadWrite()
        {
            threadResult = true;
            OpenRead();
            OpenWrite();
            OpenRead();
        }

        #endregion Helper methods

        #region Test Cases
        [TestMethod]
        public MFTestResults InvalidArguments()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                try
                {
                    Log.Comment("Bad fileshare -1");
                    using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, (FileShare)(-1))) { }
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException");
                }
                catch (ArgumentException) { /* pass case */ }

                try
                {
                    Log.Comment("Bad fileshare 10");
                    using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, (FileShare)(10))) { }
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
        public MFTestResults FileShare_Open_Matrix()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                // Given the previous fileshare info and the requested fileaccess and fileshare
                // the following is the ONLY combinations that we should allow -- All others
                // should failed with IOException
                // (Behavior verified on desktop .NET)
                //
                // Previous FileShare   Requested FileAccess    Requested FileShare
                // Read                 Read                    ReadWrite
                // Write                Write                   ReadWrite
                // ReadWrite            Read                    ReadWrite
                // ReadWrite            Write                   ReadWrite
                // ReadWrite            ReadWrite               ReadWrite
                MatrixTestCase[] testCases = new MatrixTestCase[] {
                    // pass cases
                    new MatrixTestCase(FileShare.Read, FileAccess.Read, FileShare.ReadWrite, true),
                    new MatrixTestCase(FileShare.Write, FileAccess.Write, FileShare.ReadWrite, true),
                    new MatrixTestCase(FileShare.ReadWrite, FileAccess.Read, FileShare.ReadWrite, true),
                    new MatrixTestCase(FileShare.ReadWrite, FileAccess.Write, FileShare.ReadWrite, true),
                    new MatrixTestCase(FileShare.ReadWrite, FileAccess.ReadWrite, FileShare.ReadWrite, true),

                    // fail cases
                    new MatrixTestCase(FileShare.Read, FileAccess.Read, FileShare.Read, false),
                    new MatrixTestCase(FileShare.Read, FileAccess.Read, FileShare.Write, false),
                    new MatrixTestCase(FileShare.Read, FileAccess.Read, FileShare.None, false),

                    new MatrixTestCase(FileShare.Read, FileAccess.Write, FileShare.Read, false),
                    new MatrixTestCase(FileShare.Read, FileAccess.Write, FileShare.Write, false),
                    new MatrixTestCase(FileShare.Read, FileAccess.Write, FileShare.ReadWrite, false),
                    new MatrixTestCase(FileShare.Read, FileAccess.Write, FileShare.None, false),

                    new MatrixTestCase(FileShare.Read, FileAccess.ReadWrite, FileShare.Read, false),
                    new MatrixTestCase(FileShare.Read, FileAccess.ReadWrite, FileShare.Write, false),
                    new MatrixTestCase(FileShare.Read, FileAccess.ReadWrite, FileShare.ReadWrite, false),
                    new MatrixTestCase(FileShare.Read, FileAccess.ReadWrite, FileShare.None, false),

                    new MatrixTestCase(FileShare.Write, FileAccess.Read, FileShare.Read, false),
                    new MatrixTestCase(FileShare.Write, FileAccess.Read, FileShare.Write, false),
                    new MatrixTestCase(FileShare.Write, FileAccess.Read, FileShare.ReadWrite, false),
                    new MatrixTestCase(FileShare.Write, FileAccess.Read, FileShare.None, false),

                    new MatrixTestCase(FileShare.Write, FileAccess.Write, FileShare.Read, false),
                    new MatrixTestCase(FileShare.Write, FileAccess.Write, FileShare.Write, false),
                    new MatrixTestCase(FileShare.Write, FileAccess.Write, FileShare.None, false),

                    new MatrixTestCase(FileShare.Write, FileAccess.ReadWrite, FileShare.Read, false),
                    new MatrixTestCase(FileShare.Write, FileAccess.ReadWrite, FileShare.Write, false),
                    new MatrixTestCase(FileShare.Write, FileAccess.ReadWrite, FileShare.ReadWrite, false),
                    new MatrixTestCase(FileShare.Write, FileAccess.ReadWrite, FileShare.None, false),

                    new MatrixTestCase(FileShare.ReadWrite, FileAccess.Read, FileShare.Read, false),
                    new MatrixTestCase(FileShare.ReadWrite, FileAccess.Read, FileShare.Write, false),
                    new MatrixTestCase(FileShare.ReadWrite, FileAccess.Read, FileShare.None, false),

                    new MatrixTestCase(FileShare.ReadWrite, FileAccess.Write, FileShare.Read, false),
                    new MatrixTestCase(FileShare.ReadWrite, FileAccess.Write, FileShare.Write, false),
                    new MatrixTestCase(FileShare.ReadWrite, FileAccess.Write, FileShare.None, false),

                    new MatrixTestCase(FileShare.ReadWrite, FileAccess.ReadWrite, FileShare.Read, false),
                    new MatrixTestCase(FileShare.ReadWrite, FileAccess.ReadWrite, FileShare.Write, false),
                    new MatrixTestCase(FileShare.ReadWrite, FileAccess.ReadWrite, FileShare.None, false),

                    new MatrixTestCase(FileShare.None, FileAccess.Read, FileShare.Read, false),
                    new MatrixTestCase(FileShare.None, FileAccess.Read, FileShare.Write, false),
                    new MatrixTestCase(FileShare.None, FileAccess.Read, FileShare.ReadWrite, false),
                    new MatrixTestCase(FileShare.None, FileAccess.Read, FileShare.None, false),

                    new MatrixTestCase(FileShare.None, FileAccess.Write, FileShare.Read, false),
                    new MatrixTestCase(FileShare.None, FileAccess.Write, FileShare.Write, false),
                    new MatrixTestCase(FileShare.None, FileAccess.Write, FileShare.ReadWrite, false),
                    new MatrixTestCase(FileShare.None, FileAccess.Write, FileShare.None, false),

                    new MatrixTestCase(FileShare.None, FileAccess.ReadWrite, FileShare.Read, false),
                    new MatrixTestCase(FileShare.None, FileAccess.ReadWrite, FileShare.Write, false),
                    new MatrixTestCase(FileShare.None, FileAccess.ReadWrite, FileShare.ReadWrite, false),
                    new MatrixTestCase(FileShare.None, FileAccess.ReadWrite, FileShare.None, false)
                };

                // create file if it doesn't exist.  We are just opening, so worries what it contains
                if (!File.Exists(fileName))
                    File.Create(fileName).Close();

                foreach (MatrixTestCase testcase in testCases)
                {
                    Log.Comment("Previous share: " + testcase.previousShare);
                    Log.Comment("Requested access: " + testcase.requestedAccess);
                    Log.Comment("Requested share: " + testcase.requestedShare);
                    try
                    {
                        using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite, testcase.previousShare))
                        {
                            using (FileStream fs2 = new FileStream(fileName, FileMode.Open, testcase.requestedAccess, testcase.requestedShare)) { }
                            if (!testcase.expectSuccess)
                            {
                                Log.Exception("Unexpected IOException - Expected failure");
                                result = MFTestResults.Fail;
                            }
                            else
                            {
                                Log.Comment("Case passes");
                            }
                        }
                    }
                    catch (IOException)
                    {
                        if (testcase.expectSuccess)
                        {
                            Log.Exception("Unexpected IOException - Expected success");
                            result = MFTestResults.Fail;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Exception("Unexpected exception: " + ex.Message);
                        result = MFTestResults.Fail;
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
        public MFTestResults FileShare_None()
        {
            // Clean up incase file exists
            if (File.Exists(fileName))
                File.Delete(fileName);

            MFTestResults result = MFTestResults.Pass;
            try
            {
                // From MSDN:
                // Declines sharing of the current file. Any request to open the file 
                // (by this process or another process) will fail until the file is closed. 

                Log.Comment("Create file for testing");
                using (FileStream fs = new FileStream(fileName, FileMode.CreateNew))
                {
                    FileStreamHelper.WriteReadEmpty(fs);
                }

                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    Log.Comment("Should be able to write");
                    if (!FileStreamHelper.Write(fs, 1000))
                        result = MFTestResults.Fail;

                    Log.Comment("Should be able to read");
                    fs.Seek(0, SeekOrigin.Begin);
                    if (!FileStreamHelper.VerifyRead(fs))
                        result = MFTestResults.Fail;

                    Log.Comment("Try to open in same thread");
                    try
                    {
                        using (FileStream fs2 = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) { }
                        result = MFTestResults.Fail;
                        Log.Exception("FileAccess.Read - Expected IOException");
                    }
                    catch (IOException) { /* pass case */ }
                    try
                    {
                        using (FileStream fs2 = new FileStream(fileName, FileMode.Open, FileAccess.Write, FileShare.ReadWrite)) { }
                        result = MFTestResults.Fail;
                        Log.Exception("FileAccess.Write - Expected IOException");
                    }
                    catch (IOException) { /* pass case */ }

                    Log.Comment("Try to open in another thread");
                    Thread worker = new Thread(OpenRestrictedFile);
                    worker.Start();
                    worker.Join();
                    if (!threadResult)
                        result = MFTestResults.Fail;

                    Log.Comment("Write after read other access attempt");
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

        [TestMethod]
        public MFTestResults FileShare_Read()
        {
            // Clean up incase file exists
            if (File.Exists(fileName))
                File.Delete(fileName);

            MFTestResults result = MFTestResults.Pass;
            try
            {
                // From MSDN:
                // Declines sharing of the current file. Any request to open the file 
                // (by this process or another process) will fail until the file is closed. 

                Log.Comment("Create file for testing");
                using (FileStream fs = new FileStream(fileName, FileMode.CreateNew))
                {
                    FileStreamHelper.WriteReadEmpty(fs);
                }

                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
                {
                    Log.Comment("Should be able to write");
                    if (!FileStreamHelper.Write(fs, 1000))
                        result = MFTestResults.Fail;

                    Log.Comment("Should be able to read");
                    fs.Seek(0, SeekOrigin.Begin);
                    if (!FileStreamHelper.VerifyRead(fs))
                        result = MFTestResults.Fail;

                    Log.Comment("Try to Read in same thread");
                    if (!FileStreamHelper.VerifyFile(fileName))
                        result = MFTestResults.Fail;

                    Log.Comment("Try to Read in another thread");
                    restricted = false;
                    threadResult = true;
                    Thread worker = new Thread(OpenRead);
                    worker.Start();
                    worker.Join();
                    if (!threadResult)
                        result = MFTestResults.Fail;

                    Log.Comment("Try to Write in same thread");
                    try
                    {
                        using (FileStream fs2 = new FileStream(fileName, FileMode.Open, FileAccess.Write, FileShare.ReadWrite)) { }
                        result = MFTestResults.Fail;
                        Log.Exception("Expected IOException");
                    }
                    catch (IOException) { /* pass case */ }

                    Log.Comment("Try to Write in another thread");
                    restricted = true;
                    threadResult = true;
                    worker = new Thread(OpenWrite);
                    worker.Start();
                    worker.Join();
                    if (!threadResult)
                        result = MFTestResults.Fail;

                    Log.Comment("Write after read other access attempt");
                    fs.Seek(0, SeekOrigin.End);
                    if (!FileStreamHelper.Write(fs, 500))
                        result = MFTestResults.Fail;
                }

                Log.Comment("Verify file after close - 300 bytes original + 1000 bytes + 500 bytes");
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
        public MFTestResults FileShare_ReadWrite()
        {
            // Clean up incase file exists
            if (File.Exists(fileName))
                File.Delete(fileName);

            MFTestResults result = MFTestResults.Pass;
            try
            {
                // From MSDN:
                // Allows subsequent opening of the file for reading or writing. If this 
                // flag is not specified, any request to open the file for reading or 
                // writing (by this process or another process) will fail until the file 
                // is closed. However, even if this flag is specified, additional 
                // permissions might still be needed to access the file. 

                // 300 bytes
                Log.Comment("Create file for testing");
                using (FileStream fs = new FileStream(fileName, FileMode.CreateNew))
                {
                    FileStreamHelper.WriteReadEmpty(fs);
                }

                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    // 1000 bytes
                    Log.Comment("Should be able to write");
                    if (!FileStreamHelper.Write(fs, 1000))
                        result = MFTestResults.Fail;

                    Log.Comment("Should be able to read");
                    fs.Seek(0, SeekOrigin.Begin);
                    if (!FileStreamHelper.VerifyRead(fs))
                        result = MFTestResults.Fail;

                    Log.Comment("Try to Read in same thread");
                    if (!FileStreamHelper.VerifyFile(fileName))
                        result = MFTestResults.Fail;

                    Log.Comment("Try to Read in another thread");
                    restricted = false;
                    threadResult = true;
                    Thread worker = new Thread(OpenRead);
                    worker.Start();
                    worker.Join();
                    if (!threadResult)
                        result = MFTestResults.Fail;

                    // 500 bytes
                    Log.Comment("Try to Write in same thread");
                    using (FileStream fs2 = new FileStream(fileName, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
                    {
                        FileStreamHelper.Write(fs2, 500);
                    }

                    // 200 bytes
                    Log.Comment("Try to Write in another thread");
                    restricted = false;
                    threadResult = true;
                    worker = new Thread(OpenWrite);
                    worker.Start();
                    worker.Join();
                    if (!threadResult)
                        result = MFTestResults.Fail;

                    // 300 bytes
                    Log.Comment("Try to ReadWrite");
                    using (FileStream fs2 = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        if (!FileStreamHelper.WriteReadVerify(fs2))
                            result = MFTestResults.Fail;
                    }

                    // 500 bytes
                    Log.Comment("Write after read other access attempt");
                    fs.Seek(0, SeekOrigin.End);
                    if (!FileStreamHelper.Write(fs, 500))
                        result = MFTestResults.Fail;
                }

                // 300 + 1000 + 500 + 200 + 300 + 500 = 2800
                Log.Comment("Verify file after close");
                if (!FileStreamHelper.VerifyFile(fileName, 2800))
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
        public MFTestResults FileShare_Write()
        {
            // Clean up incase file exists
            if (File.Exists(fileName))
                File.Delete(fileName);

            MFTestResults result = MFTestResults.Pass;
            try
            {
                // From MSDN:
                // Allows subsequent opening of the file for writing. If this flag 
                // is not specified, any request to open the file for writing (by 
                // this process or another process) will fail until the file is 
                // closed. However, even if this flag is specified, additional 
                // permissions might still be needed to access the file. 

                // 300 bytes
                Log.Comment("Create file for testing");
                using (FileStream fs = new FileStream(fileName, FileMode.CreateNew))
                {
                    FileStreamHelper.WriteReadEmpty(fs);
                }

                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Write))
                {
                    // 1000 bytes
                    Log.Comment("Should be able to write");
                    if (!FileStreamHelper.Write(fs, 1000))
                        result = MFTestResults.Fail;

                    Log.Comment("Should be able to read");
                    fs.Seek(0, SeekOrigin.Begin);
                    if (!FileStreamHelper.VerifyRead(fs))
                        result = MFTestResults.Fail;

                    Log.Comment("Try to Read in same thread");
                    try
                    {
                        using (FileStream fs2 = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) { }
                        result = MFTestResults.Fail;
                        Log.Exception("FileAccess.Read - Expected IOException");
                    }
                    catch (IOException) { /* pass case */ }

                    Log.Comment("Try to Read in another thread");
                    restricted = true;
                    threadResult = true;
                    Thread worker = new Thread(OpenRead);
                    worker.Start();
                    worker.Join();
                    if (!threadResult)
                        result = MFTestResults.Fail;

                    // 500 bytes
                    Log.Comment("Try to Write in same thread");
                    using (FileStream fs2 = new FileStream(fileName, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
                    {
                        FileStreamHelper.Write(fs2, 500);
                    }

                    // 200 bytes
                    Log.Comment("Try to Write in another thread");
                    restricted = false;
                    threadResult = true;
                    worker = new Thread(OpenWrite);
                    worker.Start();
                    worker.Join();
                    if (!threadResult)
                        result = MFTestResults.Fail;

                    // 500 bytes
                    Log.Comment("Write after read other access attempt");
                    fs.Seek(0, SeekOrigin.End);
                    if (!FileStreamHelper.Write(fs, 500))
                        result = MFTestResults.Fail;
                }

                // 300 + 1000 + 500 + 200 500 = 2500
                Log.Comment("Verify file after close");
                if (!FileStreamHelper.VerifyFile(fileName, 2500))
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
