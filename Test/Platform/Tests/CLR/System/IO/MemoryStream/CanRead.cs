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
    public class CanRead : IMFTestInterface
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
        public MFTestResults CanRead_Default_Ctor()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Log.Comment("Verify CanRead is true for default Ctor");
                using (MemoryStream fs = new MemoryStream())
                {
                    if (!fs.CanRead)
                    {
                        result = MFTestResults.Fail;
                        Log.Exception("Expected CanRead == true, but got CanRead == false");
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

        [TestMethod]
        public MFTestResults CanRead_Byte_Ctor()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Log.Comment("Verify CanRead is true for Byte[] Ctor");
                byte[] buffer = new byte[1024];
                using (MemoryStream fs = new MemoryStream(buffer))
                {
                    if (!fs.CanRead)
                    {
                        result = MFTestResults.Fail;
                        Log.Exception("Expected CanRead == true, but got CanRead == false");
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
