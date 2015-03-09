////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT;
using System.Threading;
using Microsoft.SPOT.Platform.Test;


namespace Microsoft.SPOT.Platform.Test.Interop
{
    public partial class InteropTest : IMFTestInterface
    {
        bool PlatformIsEmulator=false;
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("set up for interop tests.");
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
	    {
		       //PlatformIsEmulator = true; 
		       //
		       //force test interop to skip
		       //
		       PlatformIsEmulator = false; 
	    }
            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");

        }

        [TestMethod]
        public MFTestResults SignedValues()
       	{ 
            if (!PlatformIsEmulator)
	    {
                return MFTestResults.Skip;
	    }
            MFTestResults testResult = TestSignedValues(); 
            return testResult;
        }

        [TestMethod]
        public MFTestResults UnsignedValues()
        {
            if (!PlatformIsEmulator)
	    {
                return MFTestResults.Skip;
	    }
            MFTestResults testResult = TestUnsignedValues();

            return testResult;
        }
        [TestMethod]
        public MFTestResults FloatPointValues()
        {
            if (!PlatformIsEmulator)
	    {
                return MFTestResults.Skip;
	    }
            MFTestResults testResult = TestFloatPointValues();

            return testResult;
        }
        [TestMethod]
        public MFTestResults StringValues()
        {
            if (!PlatformIsEmulator)
	    {
                return MFTestResults.Skip;
	    }
            MFTestResults testResult = TestStringValues();

            return testResult;
        }
        [TestMethod]
        public MFTestResults ArrayValues()
        {
            if (!PlatformIsEmulator)
	    {
                return MFTestResults.Skip;
	    }
            MFTestResults testResult = TestArrayValues();

            return testResult;
        }
        [TestMethod]
        public MFTestResults Callback()
        {
            if (!PlatformIsEmulator)
	    {
                return MFTestResults.Skip;
	    }
            MFTestResults testResult = TestCallback();

            return testResult;
        }
        [TestMethod]
        public MFTestResults MemberFunctions()
        {
            if (!PlatformIsEmulator)
	    {
                return MFTestResults.Skip;
	    }
            MFTestResults testResult = TestMemberFunctions();

            return testResult;
        }
        [TestMethod]
        public MFTestResults CompactionForNotFixedArray()
        {
            if (!PlatformIsEmulator)
	    {
                return MFTestResults.Skip;
	    }
            MFTestResults testResult = TestCompactionForNotFixedArray();

            return testResult;
        }
        [TestMethod]
        public MFTestResults CompactionForFixedArray()
        {
            if (!PlatformIsEmulator)
	    {
                return MFTestResults.Skip;
	    }
            MFTestResults testResult = TestCompactionForFixedArray();

            return testResult;
        }
    }
}
