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
    public class WriteByte : IMFTestInterface
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

        #region Helper methods
        private bool TestWrite(MemoryStream ms, int BytesToWrite)
        {
            return TestWrite(ms, BytesToWrite, ms.Position + BytesToWrite);
        }

        private bool TestWrite(MemoryStream ms, int BytesToWrite, long ExpectedLength)
        {
            bool result = true;
            long startLength = ms.Position;
            long nextbyte = startLength % 256;

            for (int i = 0; i < BytesToWrite; i++)
            {
                ms.WriteByte((byte)nextbyte);

                // Reset if wraps past 255
                if (++nextbyte > 255)
                    nextbyte = 0;
            }

            ms.Flush();
            if (ExpectedLength < ms.Length)
            {
                result = false;
                Log.Exception("Expeceted final length of " + ExpectedLength + " bytes, but got " + ms.Length + " bytes");
            }
            return result;
        }
        #endregion Helper methods

        #region Test Cases

        [TestMethod]
        public MFTestResults ExtendBuffer()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    Log.Comment("Set Position past end of stream");
                    // Internal buffer is initialized to 256, if this changes, this test is no longer valid.  
                    // Exposing capcity would have made this test easier/dynamic.
                    ms.Position = 300;
                    ms.WriteByte(123);
                    if (ms.Length != 301)
                    {
                        result = MFTestResults.Fail;
                        Log.Exception("Expected length 301, got length " + ms.Length);
                    }
                    ms.Position = 300;
                    int read = ms.ReadByte();
                    if (read != 123)
                    {
                        result = MFTestResults.Fail;
                        Log.Exception("Expected value 123, but got value " + result);
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

        [TestMethod]
        public MFTestResults InvalidRange()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                byte[] buffer = new byte[100];
                using (MemoryStream ms = new MemoryStream(buffer))
                {
                    Log.Comment("Set Position past end of static stream");
                    ms.Position = buffer.Length + 1;
                    try
                    {
                        ms.WriteByte(1);
                        result = MFTestResults.Fail;
                        Log.Exception("Expected NotSupportedException");
                    }
                    catch (NotSupportedException) { /* pass case */ }
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception", ex);
                result = MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults VanillaWrite()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Log.Comment("Static Buffer");
                byte[] buffer = new byte[100];
                using (MemoryStream ms = new MemoryStream(buffer))
                {
                    Log.Comment("Write 50 bytes of data");
                    if (!TestWrite(ms, 50, 100))
                        result = MFTestResults.Fail;

                    Log.Comment("Write final 50 bytes of data");
                    if (!TestWrite(ms, 50, 100))
                        result = MFTestResults.Fail;

                    Log.Comment("Any more bytes written should throw");
                    try
                    {
                        ms.WriteByte(50);
                        result = MFTestResults.Fail;
                        Log.Exception("Expected NotSupportedException");
                    }
                    catch (NotSupportedException) { /* pass case */ }

                    Log.Comment("Rewind and verify all bytes written");
                    ms.Seek(0, SeekOrigin.Begin);
                    if (!MemoryStreamHelper.VerifyRead(ms))
                        result = MFTestResults.Fail;      
                }

                Log.Comment("Dynamic Buffer");
                using (MemoryStream ms = new MemoryStream())
                {
                    Log.Comment("Write 100 bytes of data");
                    if (!TestWrite(ms, 100))
                        result = MFTestResults.Fail;

                    Log.Comment("Extend internal buffer, write 160");
                    if (!TestWrite(ms, 160))
                        result = MFTestResults.Fail;

                    Log.Comment("Double extend internal buffer, write 644");
                    if (!TestWrite(ms, 644))
                        result = MFTestResults.Fail;

                    Log.Comment("write another 1100");
                    if (!TestWrite(ms, 1100))
                        result = MFTestResults.Fail;

                    Log.Comment("Rewind and verify all bytes written");
                    ms.Seek(0, SeekOrigin.Begin);
                    if (!MemoryStreamHelper.VerifyRead(ms))
                        result = MFTestResults.Fail;                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception", ex);
                result = MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults BoundaryCheck()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                for (int i = 250; i < 260; i++)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        MemoryStreamHelper.Write(ms, i);
                        ms.Position = 0;
                        if (!MemoryStreamHelper.VerifyRead(ms))
                            result = MFTestResults.Fail;

                        Log.Comment("Position: " + ms.Position);
                        Log.Comment("Length: " + ms.Length);
                        if (i != ms.Position | i != ms.Length)
                        {
                            result = MFTestResults.Fail;
                            Log.Exception("Expected Position and Length to be " + i);
                        }
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
