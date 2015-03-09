////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class ArithmeticTests2 : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests");

            // Add your functionality here.                

            return InitializeResult.ReadyToGo;
        }


        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests");
        }

        //Arithmetic Test methods
        //The following tests were ported from folder current\test\cases\client\CLR\Conformance\10_classes\Arithmetic
        //opt001a,opt001b,opt002a,opt002b,opt003a,opt003b,opt004a,opt004b,mult0001,mult0002,mult0003,mult0004,mult0008,mult0009,mult0010,mult0011,mult0015,mult0016,mult0017,mult0018,mult0022,mult0023,mult0024,mult0025,mult0050,mult0051,mult0052,mult0053,mult0057,mult0058,mult0059,mult0060,mult0064,mult0065,mult0066,mult0067,mult0071,mult0072,mult0073,mult0074,div0001,div0002,div0008,div0009,div0015,div0016,div0022,div0023,div0050,div0051,div0057,div0058,div0064,div0065,div0071,div0072,rem0001,rem0002,rem0008,rem0009,rem0015,rem0016,rem0022,rem0023,rem0050,rem0051,rem0057,rem0058,rem0064,rem0065,rem0071,rem0072,add0001,add0002,add0003,add0007,add0008,add0009,add0013,add0014,add0015,add0037,add0038,add0039,add0043,add0044,add0045,add0049,add0050,add0051,sub0001,sub0002,sub0003,sub0007,sub0008,sub0009,sub0013,sub0014,sub0015,sub0037,sub0038,sub0039,sub0043,sub0044,sub0045,sub0049,sub0050,sub0051

        //Test Case Calls 
        //Test Case Calls 

        [TestMethod]
        public MFTestResults Arith_opt001_Test()
        {
            if (Arith_TestClass_opt001.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Arith_opt002_Test()
        {
            if (Arith_TestClass_opt002.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Arith_opt003_Test()
        {
            if (Arith_TestClass_opt003.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_opt004_Test()
        {
            if (Arith_TestClass_opt004.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Arith_mult0001_Test()
        {
            Log.Comment("Section 7.7.1");
            if (Arith_TestClass_mult0001.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_mult0002_Test()
        {
            Log.Comment("Section 7.7.1");
            if (Arith_TestClass_mult0002.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_mult0003_Test()
        {
            Log.Comment("Section 7.7.1");
            if (Arith_TestClass_mult0003.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_mult0004_Test()
        {
            Log.Comment("Section 7.7.1");
            if (Arith_TestClass_mult0004.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_mult0008_Test()
        {
            Log.Comment("Section 7.7.1");
            if (Arith_TestClass_mult0008.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_mult0009_Test()
        {
            Log.Comment("Section 7.7.1");
            if (Arith_TestClass_mult0009.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_mult0010_Test()
        {
            Log.Comment("Section 7.7.1");
            if (Arith_TestClass_mult0010.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_mult0011_Test()
        {
            Log.Comment("Section 7.7.1");
            if (Arith_TestClass_mult0011.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_mult0015_Test()
        {
            Log.Comment("Section 7.7.1");
            if (Arith_TestClass_mult0015.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_mult0016_Test()
        {
            Log.Comment("Section 7.7.1");
            if (Arith_TestClass_mult0016.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_mult0017_Test()
        {
            Log.Comment("Section 7.7.1");
            if (Arith_TestClass_mult0017.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_mult0018_Test()
        {
            Log.Comment("Section 7.7.1");
            if (Arith_TestClass_mult0018.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_mult0022_Test()
        {
            Log.Comment("Section 7.7.1");
            if (Arith_TestClass_mult0022.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_mult0023_Test()
        {
            Log.Comment("Section 7.7.1");
            if (Arith_TestClass_mult0023.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_mult0024_Test()
        {
            Log.Comment("Section 7.7.1");
            if (Arith_TestClass_mult0024.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_mult0025_Test()
        {
            Log.Comment("Section 7.7.1");
            if (Arith_TestClass_mult0025.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_mult0050_Test()
        {
            Log.Comment("Section 7.7.1");
            if (Arith_TestClass_mult0050.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_mult0051_Test()
        {
            Log.Comment("Section 7.7.1");
            if (Arith_TestClass_mult0051.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_mult0052_Test()
        {
            Log.Comment("Section 7.7.1");
            if (Arith_TestClass_mult0052.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_mult0053_Test()
        {
            Log.Comment("Section 7.7.1");
            if (Arith_TestClass_mult0053.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_mult0057_Test()
        {
            Log.Comment("Section 7.7.1");
            if (Arith_TestClass_mult0057.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_mult0058_Test()
        {
            Log.Comment("Section 7.7.1");
            if (Arith_TestClass_mult0058.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_mult0059_Test()
        {
            Log.Comment("Section 7.7.1");
            if (Arith_TestClass_mult0059.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_mult0060_Test()
        {
            Log.Comment("Section 7.7.1");
            if (Arith_TestClass_mult0060.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_mult0064_Test()
        {
            Log.Comment("Section 7.7.1");
            if (Arith_TestClass_mult0064.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_mult0065_Test()
        {
            Log.Comment("Section 7.7.1");
            if (Arith_TestClass_mult0065.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_mult0066_Test()
        {
            Log.Comment("Section 7.7.1");
            if (Arith_TestClass_mult0066.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_mult0067_Test()
        {
            Log.Comment("Section 7.7.1");
            if (Arith_TestClass_mult0067.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_mult0071_Test()
        {
            Log.Comment("Section 7.7.1");
            if (Arith_TestClass_mult0071.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_mult0072_Test()
        {
            Log.Comment("Section 7.7.1");
            if (Arith_TestClass_mult0072.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_mult0073_Test()
        {
            Log.Comment("Section 7.7.1");
            if (Arith_TestClass_mult0073.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_mult0074_Test()
        {
            Log.Comment("Section 7.7.1");
            if (Arith_TestClass_mult0074.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_div0001_Test()
        {
            Log.Comment("Section 7.7.2");
            if (Arith_TestClass_div0001.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_div0002_Test()
        {
            Log.Comment("Section 7.7.2");
            if (Arith_TestClass_div0002.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_div0008_Test()
        {
            Log.Comment("Section 7.7.2");
            if (Arith_TestClass_div0008.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_div0009_Test()
        {
            Log.Comment("Section 7.7.2");
            if (Arith_TestClass_div0009.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_div0015_Test()
        {
            Log.Comment("Section 7.7.2");
            if (Arith_TestClass_div0015.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_div0016_Test()
        {
            Log.Comment("Section 7.7.2");
            if (Arith_TestClass_div0016.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_div0022_Test()
        {
            Log.Comment("Section 7.7.2");
            if (Arith_TestClass_div0022.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_div0023_Test()
        {
            Log.Comment("Section 7.7.2");
            if (Arith_TestClass_div0023.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_div0050_Test()
        {
            Log.Comment("Section 7.7.2");
            if (Arith_TestClass_div0050.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_div0051_Test()
        {
            Log.Comment("Section 7.7.2");
            if (Arith_TestClass_div0051.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_div0057_Test()
        {
            Log.Comment("Section 7.7.2");
            if (Arith_TestClass_div0057.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_div0058_Test()
        {
            Log.Comment("Section 7.7.2");
            if (Arith_TestClass_div0058.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_div0064_Test()
        {
            Log.Comment("Section 7.7.2");
            if (Arith_TestClass_div0064.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_div0065_Test()
        {
            Log.Comment("Section 7.7.2");
            if (Arith_TestClass_div0065.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_div0071_Test()
        {
            Log.Comment("Section 7.7.2");
            if (Arith_TestClass_div0071.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_div0072_Test()
        {
            Log.Comment("Section 7.7.2");
            if (Arith_TestClass_div0072.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_rem0001_Test()
        {
            Log.Comment("Section 7.7.3");
            if (Arith_TestClass_rem0001.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_rem0002_Test()
        {
            Log.Comment("Section 7.7.3");
            if (Arith_TestClass_rem0002.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_rem0008_Test()
        {
            Log.Comment("Section 7.7.3");
            if (Arith_TestClass_rem0008.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_rem0009_Test()
        {
            Log.Comment("Section 7.7.3");
            if (Arith_TestClass_rem0009.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_rem0015_Test()
        {
            Log.Comment("Section 7.7.3");
            if (Arith_TestClass_rem0015.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_rem0016_Test()
        {
            Log.Comment("Section 7.7.3");
            if (Arith_TestClass_rem0016.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_rem0022_Test()
        {
            Log.Comment("Section 7.7.3");
            if (Arith_TestClass_rem0022.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_rem0023_Test()
        {
            Log.Comment("Section 7.7.3");
            if (Arith_TestClass_rem0023.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_rem0050_Test()
        {
            Log.Comment("Section 7.7.3");
            if (Arith_TestClass_rem0050.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_rem0051_Test()
        {
            Log.Comment("Section 7.7.3");
            if (Arith_TestClass_rem0051.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_rem0057_Test()
        {
            Log.Comment("Section 7.7.3");
            if (Arith_TestClass_rem0057.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_rem0058_Test()
        {
            Log.Comment("Section 7.7.3");
            if (Arith_TestClass_rem0058.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_rem0064_Test()
        {
            Log.Comment("Section 7.7.3");
            if (Arith_TestClass_rem0064.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_rem0065_Test()
        {
            Log.Comment("Section 7.7.3");
            if (Arith_TestClass_rem0065.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_rem0071_Test()
        {
            Log.Comment("Section 7.7.3");
            if (Arith_TestClass_rem0071.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_rem0072_Test()
        {
            Log.Comment("Section 7.7.3");
            if (Arith_TestClass_rem0072.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_add0001_Test()
        {
            Log.Comment("Section 7.7.4");
            if (Arith_TestClass_add0001.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_add0002_Test()
        {
            Log.Comment("Section 7.7.4");
            if (Arith_TestClass_add0002.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_add0003_Test()
        {
            Log.Comment("Section 7.7.4");
            if (Arith_TestClass_add0003.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_add0007_Test()
        {
            Log.Comment("Section 7.7.4");
            if (Arith_TestClass_add0007.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_add0008_Test()
        {
            Log.Comment("Section 7.7.4");
            if (Arith_TestClass_add0008.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_add0009_Test()
        {
            Log.Comment("Section 7.7.4");
            if (Arith_TestClass_add0009.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_add0013_Test()
        {
            Log.Comment("Section 7.7.4");
            if (Arith_TestClass_add0013.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_add0014_Test()
        {
            Log.Comment("Section 7.7.4");
            if (Arith_TestClass_add0014.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_add0015_Test()
        {
            Log.Comment("Section 7.7.4");
            if (Arith_TestClass_add0015.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_add0037_Test()
        {
            Log.Comment("Section 7.7.4");
            if (Arith_TestClass_add0037.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_add0038_Test()
        {
            Log.Comment("Section 7.7.4");
            if (Arith_TestClass_add0038.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_add0039_Test()
        {
            Log.Comment("Section 7.7.4");
            if (Arith_TestClass_add0039.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_add0043_Test()
        {
            Log.Comment("Section 7.7.4");
            if (Arith_TestClass_add0043.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_add0044_Test()
        {
            Log.Comment("Section 7.7.4");
            if (Arith_TestClass_add0044.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_add0045_Test()
        {
            Log.Comment("Section 7.7.4");
            if (Arith_TestClass_add0045.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_add0049_Test()
        {
            Log.Comment("Section 7.7.4");
            if (Arith_TestClass_add0049.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_add0050_Test()
        {
            Log.Comment("Section 7.7.4");
            if (Arith_TestClass_add0050.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_add0051_Test()
        {
            Log.Comment("Section 7.7.4");
            if (Arith_TestClass_add0051.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_sub0001_Test()
        {
            Log.Comment("Section 7.7.5");
            if (Arith_TestClass_sub0001.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_sub0002_Test()
        {
            Log.Comment("Section 7.7.5");
            if (Arith_TestClass_sub0002.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_sub0003_Test()
        {
            Log.Comment("Section 7.7.5");
            if (Arith_TestClass_sub0003.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_sub0007_Test()
        {
            Log.Comment("Section 7.7.5");
            if (Arith_TestClass_sub0007.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_sub0008_Test()
        {
            Log.Comment("Section 7.7.5");
            if (Arith_TestClass_sub0008.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_sub0009_Test()
        {
            Log.Comment("Section 7.7.5");
            if (Arith_TestClass_sub0009.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_sub0013_Test()
        {
            Log.Comment("Section 7.7.5");
            if (Arith_TestClass_sub0013.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_sub0014_Test()
        {
            Log.Comment("Section 7.7.5");
            if (Arith_TestClass_sub0014.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_sub0015_Test()
        {
            Log.Comment("Section 7.7.5");
            if (Arith_TestClass_sub0015.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_sub0037_Test()
        {
            Log.Comment("Section 7.7.5");
            if (Arith_TestClass_sub0037.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_sub0038_Test()
        {
            Log.Comment("Section 7.7.5");
            if (Arith_TestClass_sub0038.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_sub0039_Test()
        {
            Log.Comment("Section 7.7.5");
            if (Arith_TestClass_sub0039.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_sub0043_Test()
        {
            Log.Comment("Section 7.7.5");
            if (Arith_TestClass_sub0043.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_sub0044_Test()
        {
            Log.Comment("Section 7.7.5");
            if (Arith_TestClass_sub0044.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_sub0045_Test()
        {
            Log.Comment("Section 7.7.5");
            if (Arith_TestClass_sub0045.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_sub0049_Test()
        {
            Log.Comment("Section 7.7.5");
            if (Arith_TestClass_sub0049.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_sub0050_Test()
        {
            Log.Comment("Section 7.7.5");
            if (Arith_TestClass_sub0050.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        [TestMethod]
        public MFTestResults Arith_sub0051_Test()
        {
            Log.Comment("Section 7.7.5");
            if (Arith_TestClass_sub0051.testMethod())
            {
                return MFTestResults.Pass;
            }
            return MFTestResults.Fail;
        }
        //Compiled Test Cases 



        //Compiled Test Cases 
        class Arith_TestClass_opt001
        {
            static int Main_old()
            {
                double t0 = 1.5;
                int i = 0;
                for (i = 0; i < 1; i++)
                {
                    double dd = t0 / 3;
                    t0 -= dd;
                    if (dd > 2)
                    {
                        break;
                    }
                }
                if (t0 != 1)
                    return 1;// Failed.
                else
                    return 0; // No problem.		
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_opt002
        {
            static int Main_old()
            {
                double t0 = 1.5;
                int i = 0;
                for (i = 0; i < 1; i++)
                {
                    double dd = t0 / 3;
                    t0 += dd;
                    if (dd > 2)
                    {
                        break;
                    }
                }
                if (t0 != 2)
                    return 1;// Failed.
                else
                    return 0; // No problem.		
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_opt003
        {
            static int Main_old()
            {
                double t0 = 1.5;
                int i = 0;
                for (i = 0; i < 1; i++)
                {
                    double dd = t0 / 3;
                    t0 *= dd;
                    if (dd > 2)
                    {
                        break;
                    }
                }
                if (t0 != 0.75)
                    return 1;// Failed.
                else
                    return 0; // No problem.		
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_opt004
        {
            static int Main_old()
            {
                double t0 = 1.5;
                int i = 0;
                for (i = 0; i < 1; i++)
                {
                    double dd = t0 / 3;
                    t0 /= dd;
                    if (dd > 2)
                    {
                        break;
                    }
                }
                if (t0 != 3)
                    return 1;// Failed.
                else
                    return 0; // No problem.		
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }


        class Arith_TestClass_mult0001
        {
            public static int Main_old()
            {
                float f1 = 2.0f;
                float f2 = 2.0f;
                float f3 = f1 * f2;
                if (f3 == 4.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_mult0002
        {
            public static int Main_old()
            {
                float f1 = 2.0f;
                float f2 = -2.0f;
                float f3 = f1 * f2;
                if (f3 == -4.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_mult0003
        {
            public static int Main_old()
            {
                float f1 = 2.0f;
                float f2 = 0.0f;
                float f3 = f1 * f2;
                if (f3 == 0.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_mult0004
        {
            public static int Main_old()
            {
                float f1 = 2.0f;
                float f2 = -0.0f;
                float f3 = f1 * f2;
                if (f3 == -0.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_mult0008
        {
            public static int Main_old()
            {
                float f1 = -2.0f;
                float f2 = 2.0f;
                float f3 = f1 * f2;
                if (f3 == -4.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_mult0009
        {
            public static int Main_old()
            {
                float f1 = -2.0f;
                float f2 = -2.0f;
                float f3 = f1 * f2;
                if (f3 == 4.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_mult0010
        {
            public static int Main_old()
            {
                float f1 = -2.0f;
                float f2 = 0.0f;
                float f3 = f1 * f2;
                if (f3 == -0.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_mult0011
        {
            public static int Main_old()
            {
                float f1 = -2.0f;
                float f2 = -0.0f;
                float f3 = f1 * f2;
                if (f3 == 0.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_mult0015
        {
            public static int Main_old()
            {
                float f1 = 0.0f;
                float f2 = 2.0f;
                float f3 = f1 * f2;
                if (f3 == 0.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_mult0016
        {
            public static int Main_old()
            {
                float f1 = 0.0f;
                float f2 = -2.0f;
                float f3 = f1 * f2;
                if (f3 == -0.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_mult0017
        {
            public static int Main_old()
            {
                float f1 = 0.0f;
                float f2 = 0.0f;
                float f3 = f1 * f2;
                if (f3 == 0.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_mult0018
        {
            public static int Main_old()
            {
                float f1 = 0.0f;
                float f2 = -0.0f;
                float f3 = f1 * f2;
                if (f3 == -0.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_mult0022
        {
            public static int Main_old()
            {
                float f1 = -0.0f;
                float f2 = 2.0f;
                float f3 = f1 * f2;
                if (f3 == -0.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_mult0023
        {
            public static int Main_old()
            {
                float f1 = -0.0f;
                float f2 = -2.0f;
                float f3 = f1 * f2;
                if (f3 == 0.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_mult0024
        {
            public static int Main_old()
            {
                float f1 = -0.0f;
                float f2 = 0.0f;
                float f3 = f1 * f2;
                if (f3 == -0.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_mult0025
        {
            public static int Main_old()
            {
                float f1 = -0.0f;
                float f2 = -0.0f;
                float f3 = f1 * f2;
                if (f3 == 0.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_mult0050
        {
            public static int Main_old()
            {
                double d1 = 2.0;
                double d2 = 2.0;
                double d3 = d1 * d2;
                if (d3 == 4.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_mult0051
        {
            public static int Main_old()
            {
                double d1 = 2.0;
                double d2 = -2.0;
                double d3 = d1 * d2;
                if (d3 == -4.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_mult0052
        {
            public static int Main_old()
            {
                double d1 = 2.0;
                double d2 = 0.0;
                double d3 = d1 * d2;
                if (d3 == 0.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_mult0053
        {
            public static int Main_old()
            {
                double d1 = 2.0;
                double d2 = -0.0;
                double d3 = d1 * d2;
                if (d3 == -0.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_mult0057
        {
            public static int Main_old()
            {
                double d1 = -2.0;
                double d2 = 2.0;
                double d3 = d1 * d2;
                if (d3 == -4.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_mult0058
        {
            public static int Main_old()
            {
                double d1 = -2.0;
                double d2 = -2.0;
                double d3 = d1 * d2;
                if (d3 == 4.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_mult0059
        {
            public static int Main_old()
            {
                double d1 = -2.0;
                double d2 = 0.0;
                double d3 = d1 * d2;
                if (d3 == -0.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_mult0060
        {
            public static int Main_old()
            {
                double d1 = -2.0;
                double d2 = -0.0;
                double d3 = d1 * d2;
                if (d3 == 0.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_mult0064
        {
            public static int Main_old()
            {
                double d1 = 0.0;
                double d2 = 2.0;
                double d3 = d1 * d2;
                if (d3 == 0.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_mult0065
        {
            public static int Main_old()
            {
                double d1 = 0.0;
                double d2 = -2.0;
                double d3 = d1 * d2;
                if (d3 == -0.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_mult0066
        {
            public static int Main_old()
            {
                double d1 = 0.0;
                double d2 = 0.0;
                double d3 = d1 * d2;
                if (d3 == 0.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_mult0067
        {
            public static int Main_old()
            {
                double d1 = 0.0;
                double d2 = -0.0;
                double d3 = d1 * d2;
                if (d3 == -0.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_mult0071
        {
            public static int Main_old()
            {
                double d1 = -0.0;
                double d2 = 2.0;
                double d3 = d1 * d2;
                if (d3 == -0.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_mult0072
        {
            public static int Main_old()
            {
                double d1 = -0.0;
                double d2 = -2.0;
                double d3 = d1 * d2;
                if (d3 == 0.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_mult0073
        {
            public static int Main_old()
            {
                double d1 = -0.0;
                double d2 = 0.0;
                double d3 = d1 * d2;
                if (d3 == -0.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_mult0074
        {
            public static int Main_old()
            {
                double d1 = -0.0;
                double d2 = -0.0;
                double d3 = d1 * d2;
                if (d3 == 0.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_div0001
        {
            public static int Main_old()
            {
                float f1 = 2.0f;
                float f2 = 2.0f;
                float f3 = f1 / f2;
                if (f3 == 1.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_div0002
        {
            public static int Main_old()
            {
                float f1 = 2.0f;
                float f2 = -2.0f;
                float f3 = f1 / f2;
                if (f3 == -1.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_div0008
        {
            public static int Main_old()
            {
                float f1 = -2.0f;
                float f2 = 2.0f;
                float f3 = f1 / f2;
                if (f3 == -1.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_div0009
        {
            public static int Main_old()
            {
                float f1 = -2.0f;
                float f2 = -2.0f;
                float f3 = f1 / f2;
                if (f3 == 1.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_div0015
        {
            public static int Main_old()
            {
                float f1 = 0.0f;
                float f2 = 2.0f;
                float f3 = f1 / f2;
                if (f3 == 0.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_div0016
        {
            public static int Main_old()
            {
                float f1 = 0.0f;
                float f2 = -2.0f;
                float f3 = f1 / f2;
                if (f3 == -0.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_div0022
        {
            public static int Main_old()
            {
                float f1 = -0.0f;
                float f2 = 2.0f;
                float f3 = f1 / f2;
                if (f3 == -0.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_div0023
        {
            public static int Main_old()
            {
                float f1 = -0.0f;
                float f2 = -2.0f;
                float f3 = f1 / f2;
                if (f3 == 0.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_div0050
        {
            public static int Main_old()
            {
                double d1 = 2.0;
                double d2 = 2.0;
                double d3 = d1 / d2;
                if (d3 == 1.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_div0051
        {
            public static int Main_old()
            {
                double d1 = 2.0;
                double d2 = -2.0;
                double d3 = d1 / d2;
                if (d3 == -1.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_div0057
        {
            public static int Main_old()
            {
                double d1 = -2.0;
                double d2 = 2.0;
                double d3 = d1 / d2;
                if (d3 == -1.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_div0058
        {
            public static int Main_old()
            {
                double d1 = -2.0;
                double d2 = -2.0;
                double d3 = d1 / d2;
                if (d3 == 1.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_div0064
        {
            public static int Main_old()
            {
                double d1 = 0.0;
                double d2 = 2.0;
                double d3 = d1 / d2;
                if (d3 == 0.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_div0065
        {
            public static int Main_old()
            {
                double d1 = 0.0;
                double d2 = -2.0;
                double d3 = d1 / d2;
                if (d3 == -0.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_div0071
        {
            public static int Main_old()
            {
                double d1 = -0.0;
                double d2 = 2.0;
                double d3 = d1 / d2;
                if (d3 == -0.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_div0072
        {
            public static int Main_old()
            {
                double d1 = -0.0;
                double d2 = -2.0;
                double d3 = d1 / d2;
                if (d3 == 0.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_rem0001
        {
            public static int Main_old()
            {
                float f1 = 3.0f;
                float f2 = 2.0f;
                float f3 = f1 % f2;
                if (f3 == 1.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_rem0002
        {
            public static int Main_old()
            {
                float f1 = 3.0f;
                float f2 = -2.0f;
                float f3 = f1 % f2;
                if (f3 == 1.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_rem0008
        {
            public static int Main_old()
            {
                float f1 = -3.0f;
                float f2 = 2.0f;
                float f3 = f1 % f2;
                if (f3 == -1.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_rem0009
        {
            public static int Main_old()
            {
                float f1 = -3.0f;
                float f2 = -2.0f;
                float f3 = f1 % f2;
                if (f3 == -1.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_rem0015
        {
            public static int Main_old()
            {
                float f1 = 0.0f;
                float f2 = 2.0f;
                float f3 = f1 % f2;
                if (f3 == 0.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_rem0016
        {
            public static int Main_old()
            {
                float f1 = 0.0f;
                float f2 = -2.0f;
                float f3 = f1 % f2;
                if (f3 == 0.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_rem0022
        {
            public static int Main_old()
            {
                float f1 = -0.0f;
                float f2 = 2.0f;
                float f3 = f1 % f2;
                if (f3 == -0.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_rem0023
        {
            public static int Main_old()
            {
                float f1 = -0.0f;
                float f2 = -2.0f;
                float f3 = f1 % f2;
                if (f3 == -0.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_rem0050
        {
            public static int Main_old()
            {
                double d1 = 3.0;
                double d2 = 2.0;
                double d3 = d1 % d2;
                if (d3 == 1.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_rem0051
        {
            public static int Main_old()
            {
                double d1 = 3.0;
                double d2 = -2.0;
                double d3 = d1 % d2;
                if (d3 == 1.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_rem0057
        {
            public static int Main_old()
            {
                double d1 = -3.0;
                double d2 = 2.0;
                double d3 = d1 % d2;
                if (d3 == -1.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_rem0058
        {
            public static int Main_old()
            {
                double d1 = -3.0;
                double d2 = -2.0;
                double d3 = d1 % d2;
                if (d3 == -1.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_rem0064
        {
            public static int Main_old()
            {
                double d1 = 0.0;
                double d2 = 2.0;
                double d3 = d1 % d2;
                if (d3 == 0.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_rem0065
        {
            public static int Main_old()
            {
                double d1 = 0.0;
                double d2 = -2.0;
                double d3 = d1 % d2;
                if (d3 == 0.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_rem0071
        {
            public static int Main_old()
            {
                double d1 = -0.0;
                double d2 = 2.0;
                double d3 = d1 % d2;
                if (d3 == -0.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_rem0072
        {
            public static int Main_old()
            {
                double d1 = -0.0;
                double d2 = -2.0;
                double d3 = d1 % d2;
                if (d3 == -0.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_add0001
        {
            public static int Main_old()
            {
                float f1 = 3.0f;
                float f2 = -2.0f;
                float f3 = f1 + f2;
                if (f3 == 1.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_add0002
        {
            public static int Main_old()
            {
                float f1 = 3.0f;
                float f2 = 0.0f;
                float f3 = f1 + f2;
                if (f3 == 3.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_add0003
        {
            public static int Main_old()
            {
                float f1 = 3.0f;
                float f2 = -0.0f;
                float f3 = f1 + f2;
                if (f3 == 3.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_add0007
        {
            public static int Main_old()
            {
                float f1 = 0.0f;
                float f2 = -2.0f;
                float f3 = f1 + f2;
                if (f3 == -2.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_add0008
        {
            public static int Main_old()
            {
                float f1 = 0.0f;
                float f2 = 0.0f;
                float f3 = f1 + f2;
                if (f3 == 0.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_add0009
        {
            public static int Main_old()
            {
                float f1 = 0.0f;
                float f2 = -0.0f;
                float f3 = f1 + f2;
                if (f3 == 0.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_add0013
        {
            public static int Main_old()
            {
                float f1 = -0.0f;
                float f2 = -2.0f;
                float f3 = f1 + f2;
                if (f3 == -2.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_add0014
        {
            public static int Main_old()
            {
                float f1 = -0.0f;
                float f2 = 0.0f;
                float f3 = f1 + f2;
                if (f3 == 0.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_add0015
        {
            public static int Main_old()
            {
                float f1 = -0.0f;
                float f2 = -0.0f;
                float f3 = f1 + f2;
                if (f3 == -0.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_add0037
        {
            public static int Main_old()
            {
                double d1 = 3.0;
                double d2 = -2.0;
                double d3 = d1 + d2;
                if (d3 == 1.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_add0038
        {
            public static int Main_old()
            {
                double d1 = 3.0;
                double d2 = 0.0;
                double d3 = d1 + d2;
                if (d3 == 3.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_add0039
        {
            public static int Main_old()
            {
                double d1 = 3.0;
                double d2 = -0.0;
                double d3 = d1 + d2;
                if (d3 == 3.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_add0043
        {
            public static int Main_old()
            {
                double d1 = 0.0;
                double d2 = -2.0;
                double d3 = d1 + d2;
                if (d3 == -2.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_add0044
        {
            public static int Main_old()
            {
                double d1 = 0.0;
                double d2 = 0.0;
                double d3 = d1 + d2;
                if (d3 == 0.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_add0045
        {
            public static int Main_old()
            {
                double d1 = 0.0;
                double d2 = -0.0;
                double d3 = d1 + d2;
                if (d3 == 0.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_add0049
        {
            public static int Main_old()
            {
                double d1 = -0.0;
                double d2 = -2.0;
                double d3 = d1 + d2;
                if (d3 == -2.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_add0050
        {
            public static int Main_old()
            {
                double d1 = -0.0;
                double d2 = 0.0;
                double d3 = d1 + d2;
                if (d3 == 0.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_add0051
        {
            public static int Main_old()
            {
                double d1 = -0.0;
                double d2 = -0.0;
                double d3 = d1 + d2;
                if (d3 == -0.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_sub0001
        {
            public static int Main_old()
            {
                float f1 = 3.0f;
                float f2 = -2.0f;
                float f3 = f1 - f2;
                if (f3 == 5.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_sub0002
        {
            public static int Main_old()
            {
                float f1 = 3.0f;
                float f2 = 0.0f;
                float f3 = f1 - f2;
                if (f3 == 3.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_sub0003
        {
            public static int Main_old()
            {
                float f1 = 3.0f;
                float f2 = -0.0f;
                float f3 = f1 - f2;
                if (f3 == 3.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_sub0007
        {
            public static int Main_old()
            {
                float f1 = 0.0f;
                float f2 = -2.0f;
                float f3 = f1 - f2;
                if (f3 == 2.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_sub0008
        {
            public static int Main_old()
            {
                float f1 = 0.0f;
                float f2 = 0.0f;
                float f3 = f1 - f2;
                if (f3 == 0.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_sub0009
        {
            public static int Main_old()
            {
                float f1 = 0.0f;
                float f2 = -0.0f;
                float f3 = f1 - f2;
                if (f3 == 0.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_sub0013
        {
            public static int Main_old()
            {
                float f1 = -0.0f;
                float f2 = -2.0f;
                float f3 = f1 - f2;
                if (f3 == 2.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_sub0014
        {
            public static int Main_old()
            {
                float f1 = -0.0f;
                float f2 = 0.0f;
                float f3 = f1 - f2;
                if (f3 == -0.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_sub0015
        {
            public static int Main_old()
            {
                float f1 = -0.0f;
                float f2 = -0.0f;
                float f3 = f1 - f2;
                if (f3 == 0.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_sub0037
        {
            public static int Main_old()
            {
                double d1 = 3.0;
                double d2 = -2.0;
                double d3 = d1 - d2;
                if (d3 == 5.0f)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_sub0038
        {
            public static int Main_old()
            {
                double d1 = 3.0;
                double d2 = 0.0;
                double d3 = d1 - d2;
                if (d3 == 3.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_sub0039
        {
            public static int Main_old()
            {
                double d1 = 3.0;
                double d2 = -0.0;
                double d3 = d1 - d2;
                if (d3 == 3.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_sub0043
        {
            public static int Main_old()
            {
                double d1 = 0.0;
                double d2 = -2.0;
                double d3 = d1 - d2;
                if (d3 == 2.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_sub0044
        {
            public static int Main_old()
            {
                double d1 = 0.0;
                double d2 = 0.0;
                double d3 = d1 - d2;
                if (d3 == 0.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_sub0045
        {
            public static int Main_old()
            {
                double d1 = 0.0;
                double d2 = -0.0;
                double d3 = d1 - d2;
                if (d3 == 0.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_sub0049
        {
            public static int Main_old()
            {
                double d1 = -0.0;
                double d2 = -2.0;
                double d3 = d1 - d2;
                if (d3 == 2.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_sub0050
        {
            public static int Main_old()
            {
                double d1 = -0.0;
                double d2 = 0.0;
                double d3 = d1 - d2;
                if (d3 == -0.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }
        class Arith_TestClass_sub0051
        {
            public static int Main_old()
            {
                double d1 = -0.0;
                double d2 = -0.0;
                double d3 = d1 - d2;
                if (d3 == 0.0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            public static bool testMethod()
            {
                return (Main_old() == 0);
            }
        }

    }
}
