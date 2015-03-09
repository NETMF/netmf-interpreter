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
    public class Close : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
        }

        [TestMethod]
        public MFTestResults VerifyClose()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                MemoryStream ms = new MemoryStream();
                ms.WriteByte(0);
                Log.Comment("Close stream");
                ms.Close();

                try
                {
                    Log.Comment("Verify actually closed by writing to it");
                    ms.WriteByte(0);
                    result = MFTestResults.Fail;
                    Log.Exception("Expected ObjectDisposedException");
                }
                catch (ObjectDisposedException) { /* pass case */ }
            }
            catch (Exception ex)
            {
                result = MFTestResults.Fail;
                Log.Exception("Unexpected exception", ex);
            }

            return result;
        }
    }
}
