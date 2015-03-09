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
    public class SetLength : IMFTestInterface
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
        private bool TestLength(MemoryStream ms, long expectedLength)
        {
            if (ms.Length != expectedLength)
            {
                Log.Exception("Expected length " + expectedLength + " but got, " + ms.Length);
                return false;
            }
            return true;
        }

        private bool TestPosition(MemoryStream ms, long expectedPosition)
        {
            if (ms.Position != expectedPosition)
            {
                Log.Exception("Expected position " + expectedPosition + " but got, " + ms.Position);
                return false;
            }
            return true;
        }
        #endregion Helper methods

        #region Test Cases
        [TestMethod]
        public MFTestResults ObjectDisposed()
        {
            MemoryStream ms = new MemoryStream();
            ms.Close();

            MFTestResults result = MFTestResults.Pass;
            try
            {
                try
                {
                    long length = ms.Length;
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ObjectDisposedException, but got length " + length);
                }
                catch (ObjectDisposedException) { /*Pass Case */ }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                result = MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults LengthTests()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    Log.Comment("Set initial length to 50, and position to 50");
                    ms.SetLength(50);
                    ms.Position = 50;
                    if (!TestLength(ms, 50))
                        result = MFTestResults.Fail;

                    Log.Comment("Write 'foo bar'");
                    StreamWriter sw = new StreamWriter(ms);
                    sw.Write("foo bar");
                    sw.Flush();
                    if (!TestLength(ms, 57))
                        result = MFTestResults.Fail;

                    Log.Comment("Shorten Length to 30");
                    ms.SetLength(30);
                    if (!TestLength(ms, 30))
                        result = MFTestResults.Fail;

                    Log.Comment("Verify position was adjusted");
                    if (!TestPosition(ms, 30))
                        result = MFTestResults.Fail;

                    Log.Comment("Extend length to 100");
                    ms.SetLength(100);
                    if (!TestLength(ms, 100))
                        result = MFTestResults.Fail;
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
        public MFTestResults InvalidSetLength()
        {
            MFTestResults result = MFTestResults.Pass;

            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    try
                    {
                        Log.Comment("-1");
                        ms.SetLength(-1);
                        result = MFTestResults.Fail;
                        Log.Exception("Expected ArgumentOutOfRangeException, but set length");
                    }
                    catch (ArgumentOutOfRangeException) { /* Pass Case */ }

                    try
                    {
                        Log.Comment("-10000");
                        ms.SetLength(-10000);
                        result = MFTestResults.Fail;
                        Log.Exception("Expected ArgumentOutOfRangeException, but set length");
                    }
                    catch (ArgumentOutOfRangeException) { /* Pass Case */ }

                    try
                    {
                        Log.Comment("long.MinValue");
                        ms.SetLength(long.MinValue);
                        result = MFTestResults.Fail;
                        Log.Exception("Expected ArgumentOutOfRangeException, but set length");
                    }
                    catch (ArgumentOutOfRangeException) { /* Pass Case */ }

                    try
                    {
                        Log.Comment("long.MaxValue");
                        ms.SetLength(long.MaxValue);
                        result = MFTestResults.Fail;
                        Log.Exception("Expected IOException, but set length");
                    }
                    catch (ArgumentOutOfRangeException) { /* Pass Case */ }
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
