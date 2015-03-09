////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class MicroFrameworkTestClass : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");
            
            // TODO: Add your set up steps here.
            // if (Setup Fails)
            // {
            //    return InitializeResult.Skip;
            // }

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");

            // TODO: Add your clean up steps here.
        }

        [TestMethod]
        public MFTestResults TestMethod1()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                //TODO: Add test code
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception", ex);
                result = MFTestResults.Fail;
            }

            return result;
        }
    }
}
