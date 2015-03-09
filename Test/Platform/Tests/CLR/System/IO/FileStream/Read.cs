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
    public class Read : IMFTestInterface
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
        private const string TestDir = "Read";
        private const string fileName = "test.tmp";
        #endregion local vars

        #region Local Helper methods
        private bool TestRead(FileStream fs, int length)
        {
            return TestRead(fs, length, length, length);
        }
        private bool TestRead(FileStream fs, int BufferLength, int BytesToRead, int BytesExpected)
        {
            bool result = true;
            int nextbyte = (int)fs.Position % 256;

            byte[] byteBuffer = new byte[BufferLength];

            int bytesRead = fs.Read(byteBuffer, 0, BytesToRead);
            if (bytesRead != BytesExpected)
            {
                result = false;
                Log.Exception("Expected " + BytesToRead + " bytes, but got " + bytesRead + " bytes");
            }

            for (int i = 0; i < bytesRead; i++)
            {
                if (byteBuffer[i] != nextbyte)
                {
                    result = false;
                    Log.Exception("Byte in position " + i + " has wrong value: " + byteBuffer[i]);
                }

                // Reset if wraps past 255
                if (++nextbyte > 255)
                    nextbyte = 0;
            }
            return result;
        }
        #endregion Local Helper methods

        #region Test Cases
        [TestMethod]
        public MFTestResults InvalidCases()
        {
            if (File.Exists(fileName))
                File.Delete(fileName);

            FileStream fs = new FileStream(fileName, FileMode.Create);
            FileStreamHelper.Write(fs, 1000);
            fs.Seek(0, SeekOrigin.Begin);

            byte[] readbuff = new byte[1024];
            
            MFTestResults result = MFTestResults.Pass;
            try
            {
                try
                {
                    Log.Comment("Read to null buffer");
                    int read = fs.Read(null, 0, readbuff.Length);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentNullException, but got " + read + " + bytes");
                }
                catch (ArgumentNullException) { /* pass case */ }

                try
                {
                    Log.Comment("Read to negative offset");
                    int read = fs.Read(readbuff, -1, readbuff.Length);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentOutOfRangeException, but got " + read + " + bytes");
                }
                catch (ArgumentOutOfRangeException) { /* pass case */ }

                try
                {
                    Log.Comment("Read to out of range offset");
                    int read = fs.Read(readbuff, readbuff.Length + 1, readbuff.Length);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException, but got " + read + " + bytes");
                }
                catch (ArgumentException) { /* pass case */ }

                try
                {
                    Log.Comment("Read negative count");
                    int read = fs.Read(readbuff, 0, -1);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentOutOfRangeException, but got " + read + " + bytes");
                }
                catch (ArgumentOutOfRangeException) { /* pass case */ }

                try
                {
                    Log.Comment("Read count larger then buffer");
                    int read = fs.Read(readbuff, 0, readbuff.Length + 1);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ArgumentException, but got " + read + " + bytes");
                }
                catch (ArgumentException) { /* pass case */ }

                try
                {
                    Log.Comment("Read closed stream");
                    fs.Close();
                    int read = fs.Read(readbuff, 0, readbuff.Length);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ObjectDisposedException, but got " + read + " + bytes");
                }
                catch (ObjectDisposedException) { /* pass case */ }

                try
                {
                    Log.Comment("Read disposed stream");
                    fs = new FileStream(fileName, FileMode.Open);
                    fs.Dispose();
                    int read = fs.Read(readbuff, 0, readbuff.Length);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ObjectDisposedException, but got " + read + " + bytes");
                }
                catch (ObjectDisposedException) { /* pass case */ }

            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                result = MFTestResults.Fail;
            }
            finally
            {
                if (fs != null)
                    fs.Dispose();
            }

            return result;
        }

        [TestMethod]
        public MFTestResults VanillaRead()
        {
            if (File.Exists(fileName))
                File.Delete(fileName);

            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            {
                FileStreamHelper.Write(fs, 1000);
            }
            
            MFTestResults result = MFTestResults.Pass;
            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite))
                {
                    Log.Comment("Read 256 bytes of data");
                    if (!TestRead(fs, 256))
                        result = MFTestResults.Fail;

                    Log.Comment("Request less bytes then buffer");
                    if (!TestRead(fs, 256, 100, 100))
                        result = MFTestResults.Fail;

                    // 1000 - 256 - 100 = 644
                    Log.Comment("Request more bytes then file");
                    if (!TestRead(fs, 1000, 1000, 644))
                        result = MFTestResults.Fail;

                    Log.Comment("Request bytes after EOF");
                    if (!TestRead(fs, 100, 100, 0))
                        result = MFTestResults.Fail;

                    Log.Comment("Rewind and read entire file in one buffer larger then file");
                    fs.Seek(0, SeekOrigin.Begin);
                    if (!TestRead(fs, 1001, 1001, 1000))
                        result = MFTestResults.Fail;

                    Log.Comment("Verify Read validation with UTF8 string");
                    fs.SetLength(0);
                    string test = "MFFramework Test";
                    fs.Write(UTF8Encoding.UTF8.GetBytes(test), 0, test.Length);
                    fs.Flush();
                    fs.Seek(0, SeekOrigin.Begin);
                    byte[] readbuff = new byte[20];
                    fs.Read(readbuff, 0, readbuff.Length);
                    string testResult = new string(Encoding.UTF8.GetChars(readbuff));
                    if (test != testResult)
                    {
                        result = MFTestResults.Fail;
                        Log.Comment("Exepected: " + test + ", but got: " + testResult);
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
