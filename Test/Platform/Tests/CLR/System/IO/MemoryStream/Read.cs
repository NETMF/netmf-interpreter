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
            Log.Comment("Adding set up for the tests.");

            // TODO: Add your set up steps here.  
            return InitializeResult.ReadyToGo;                
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");

            // TODO: Add your clean up steps here.
        }

        #region Local Helper methods
        private bool TestRead(MemoryStream ms, int length)
        {
            return TestRead(ms, length, length, length);
        }
        private bool TestRead(MemoryStream ms, int BufferLength, int BytesToRead, int BytesExpected)
        {
            bool result = true;
            int nextbyte = (int)ms.Position % 256;

            byte[] byteBuffer = new byte[BufferLength];

            int bytesRead = ms.Read(byteBuffer, 0, BytesToRead);
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
            MFTestResults result = MFTestResults.Pass;

            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    Log.Comment("null Buffer");
                    try
                    {
                        int read = ms.Read(null, 0, 0);
                        result = MFTestResults.Fail;
                        Log.Exception("Expected ArgumentNullException, but read " + read + " bytes");
                    }
                    catch (ArgumentNullException) { /* pass case */ }

                    Log.Comment("negative offset");
                    try
                    {
                        int read = ms.Read(new byte[]{1}, -1, 0);
                        result = MFTestResults.Fail;
                        Log.Exception("Expected ArgumentOutOfRangeException, but read " + read + " bytes");
                    }
                    catch (ArgumentOutOfRangeException) { /* pass case */ }

                    Log.Comment("negative count");
                    try
                    {
                        int read = ms.Read(new byte[] { 1 }, 0, -1);
                        result = MFTestResults.Fail;
                        Log.Exception("Expected ArgumentOutOfRangeException, but read " + read + " bytes");
                    }
                    catch (ArgumentOutOfRangeException) { /* pass case */ }

                    Log.Comment("offset exceeds buffer size");
                    try
                    {
                        int read = ms.Read(new byte[] { 1 }, 2, 0);
                        result = MFTestResults.Fail;
                        Log.Exception("Expected ArgumentException, but read " + read + " bytes");
                    }
                    catch (ArgumentException) { /* pass case */ }

                    Log.Comment("count exceeds buffer size");
                    try
                    {
                        int read = ms.Read(new byte[] { 1 }, 0, 2);
                        result = MFTestResults.Fail;
                        Log.Exception("Expected ArgumentException, but read " + read + " bytes");
                    }
                    catch (ArgumentException) { /* pass case */ }
                }

                MemoryStream ms2 = new MemoryStream();
                MemoryStreamHelper.Write(ms2, 100);
                ms2.Seek(0, SeekOrigin.Begin);
                ms2.Close();

                Log.Comment("Read from closed stream");
                try
                {
                    int readBytes = ms2.Read(new byte[] { 50 }, 0, 50);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ObjectDisposedException, but read " + readBytes + " bytes");
                }
                catch (ObjectDisposedException) { /* pass case */ }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception", ex);
                result = MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults VanillaRead()
        {
            MFTestResults result = MFTestResults.Pass;

            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    // Write to stream then reset to beginning
                    MemoryStreamHelper.Write(ms, 1000);
                    ms.Seek(0, SeekOrigin.Begin);

                    Log.Comment("Read 256 bytes of data");
                    if (!TestRead(ms, 256))
                        result = MFTestResults.Fail;

                    Log.Comment("Request less bytes then buffer");
                    if (!TestRead(ms, 256, 100, 100))
                        result = MFTestResults.Fail;

                    // 1000 - 256 - 100 = 644
                    Log.Comment("Request more bytes then file");
                    if (!TestRead(ms, 1000, 1000, 644))
                        result = MFTestResults.Fail;

                    Log.Comment("Request bytes after EOF");
                    if (!TestRead(ms, 100, 100, 0))
                        result = MFTestResults.Fail;

                    Log.Comment("Rewind and read entire file in one buffer larger then file");
                    ms.Seek(0, SeekOrigin.Begin);
                    if (!TestRead(ms, 1001, 1001, 1000))
                        result = MFTestResults.Fail;

                    Log.Comment("Rewind and read from middle");
                    ms.Position = 500;
                    if (!TestRead(ms, 256))
                        result = MFTestResults.Fail;

                    Log.Comment("Read position after EOS");
                    ms.Position = ms.Length + 10;
                    if (!TestRead(ms, 100, 100, 0))
                        result = MFTestResults.Fail;

                    Log.Comment("Verify Read validation with UTF8 string");
                    ms.SetLength(0);
                    string test = "MFFramework Test";
                    ms.Write(UTF8Encoding.UTF8.GetBytes(test), 0, test.Length);
                    ms.Flush();
                    ms.Seek(0, SeekOrigin.Begin);
                    byte[] readbuff = new byte[20];
                    ms.Read(readbuff, 0, readbuff.Length);
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
                Log.Exception("Unexpected exception", ex);
                result = MFTestResults.Fail;
            }

            return result;
        }
        #endregion Test Cases
    }
}
