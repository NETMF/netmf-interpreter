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
    public class CanWrite : IMFTestInterface
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
        public MFTestResults CanWrite_Default_Ctor()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Log.Comment("Verify CanWrite is true for default Ctor");
                using (MemoryStream fs = new MemoryStream())
                {
                    if (!fs.CanWrite)
                    {
                        result = MFTestResults.Fail;
                        Log.Exception("Expected CanWrite == true, but got CanWrite == false");
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
        public MFTestResults CanWrite_Byte_Ctor()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Log.Comment("Verify CanWrite is true for Byte[] Ctor");
                byte[] buffer = new byte[1024];
                using (MemoryStream fs = new MemoryStream(buffer))
                {
                    if (!fs.CanWrite)
                    {
                        result = MFTestResults.Fail;
                        Log.Exception("Expected CanWrite == true, but got CanWrite == false");
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
