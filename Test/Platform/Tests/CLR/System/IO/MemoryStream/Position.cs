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
    public class Position : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");

            // TODO: Add your set up steps here.
            // if (Setup Fails)
            //    return InitializeResult.Skip;

            return InitializeResult.ReadyToGo;            
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");

            // TODO: Add your clean up steps here.
        }

        #region Helper methods
        private bool GetSetPosition(MemoryStream ms, int TestLength)
        {
            bool success = true;
            Log.Comment("Move forwards");
            for (int i = 0; i < TestLength; i++)
            {
                ms.Position = i;
                if (ms.Position != i)
                {
                    success = false;
                    Log.Exception("Expected position " + i + " but got position " + ms.Position);
                }
            }
            Log.Comment("Move backwards");
            for (int i = TestLength - 1; i >= 0; i--)
            {
                ms.Position = i;
                if (ms.Position != i)
                {
                    success = false;
                    Log.Exception("Expected position " + i + " but got position " + ms.Position);
                }
            } return success;
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
                    long position = ms.Position;
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ObjectDisposedException, but got position " + position);
                }
                catch (ObjectDisposedException) { /*Pass Case */ }

                try
                {
                    ms.Position = 0;
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ObjectDisposedException, but set position");
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
        public MFTestResults InvalidRange()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                byte[] buffer = new byte[100];
                using (MemoryStream ms = new MemoryStream(buffer))
                {
                    Log.Comment("Try -1 postion");
                    try
                    {
                        ms.Position = -1;
                        result = MFTestResults.Fail;
                        Log.Exception("Expected ArgumentOutOfRangeException");
                    }
                    catch (ArgumentOutOfRangeException) { /* pass case */ }

                    Log.Comment("Try Long.MinValue postion");
                    try
                    {
                        ms.Position = long.MinValue;
                        result = MFTestResults.Fail;
                        Log.Exception("Expected ArgumentOutOfRangeException");
                    }
                    catch (ArgumentOutOfRangeException) { /* pass case */ } 
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
        public MFTestResults GetSetStaticBuffer()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                byte[] buffer = new byte[1000];
                Log.Comment("Get/Set Position with static buffer");
                using (MemoryStream ms = new MemoryStream(buffer))
                {
                    if (!GetSetPosition(ms, buffer.Length))
                        result = MFTestResults.Fail;
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
        public MFTestResults GetSetDynamicBuffer()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Log.Comment("Get/Set Position with dynamic buffer");
                using (MemoryStream ms = new MemoryStream())
                {
                    if (!GetSetPosition(ms, 1000))
                        result = MFTestResults.Fail;
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
