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
    public class Flush : IMFTestInterface
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
        public MFTestResults VerifyFlush()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    byte[] data = MFUtilities.GetRandomBytes(5000);
                    ms.Write(data, 0, data.Length);
                    ms.Flush();
                    if (ms.Length != 5000)
                    {
                        result = MFTestResults.Fail;
                        Log.Exception("Expected 5000 bytes, but got " + ms.Length);
                    }
                }
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
