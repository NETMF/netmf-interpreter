////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class SystemMathTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests");

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests");
        }

        //SystemMath Test methods
        [TestMethod]
        public MFTestResults SystemMath1_PI_Test()
        {
            /// <summary>
            /// 1. Tests that the Math.PI constant is accurate to 15 significant digits
            /// </summary>
            ///
            Log.Comment("Tests that the Math.PI constant is accurate to 15 significant digits");
            bool testResult = true;
            double PIFloor = 3.141592653589793;
            double PICeil = 3.141592653589794;
            testResult &= (PIFloor <= System.Math.PI && System.Math.PI <= PICeil);
            
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SystemMath2_E_Test()
        {
            /// <summary>
            /// 1. Tests that the Math.E constant is accurate to 15 significant digits
            /// </summary>
            ///
            Log.Comment("Tests that the Math.E constant is accurate to 15 significant digits");
            bool testResult = true;
            double EFloor = 2.718281828459045;
            double ECeil = 2.718281828459046;
            testResult &= (EFloor <= System.Math.E && System.Math.E <= ECeil);

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SystemMath3_Abs_Test()
        {
            /// <summary>
            /// 1. Tests the Math.Abs method
            /// </summary>
            ///
            Log.Comment("Tests the Math.Abs method");
            bool testResult = true;
            int positive = new Random().Next(1000);
            Log.Comment("With " + positive);
            int negative = -positive;
            testResult &= ((positive + negative) == 0);
            testResult &= (positive == System.Math.Abs(negative));
            testResult &= ((System.Math.Abs(-positive) + negative) == 0);
            testResult &= (System.Math.Abs(positive+negative) == 0); 

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SystemMath4_Ceiling_Test()
        {
            /// <summary>
            /// 1. Tests the Math.Ceiling method
            /// </summary>
            ///
            Log.Comment("Tests the Math.Ceiling method");
            bool testResult = true;
            double base1 = new Random().Next(1000);
            Log.Comment("With " + base1);
            testResult &= (System.Math.Ceiling(base1) == base1); 
            for (double d = .1; d < 1;d += .1 )
                testResult &= (System.Math.Ceiling(base1 + d) == base1 + 1);

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SystemMath5_Floor_Test()
        {
            /// <summary>
            /// 1. Tests the Math.Floor method
            /// </summary>
            ///
            Log.Comment("Tests the Math.Floor method");
            bool testResult = true;
            double base1 = new Random().Next(1000);
            Log.Comment("With " + base1);
            for (double d = .0; d < .9; d += .1)
                testResult &= (System.Math.Floor(base1 + d) == base1);

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SystemMath6_Round_Test()
        {
            /// <summary>
            /// 1. Tests the Math.Round method
            /// </summary>
            ///
            Log.Comment("Tests the Math.Round method");
            bool testResult = true;
            double base1 = 334;
            Log.Comment("With " + base1);
            for (double d = .0; d < .6; d += .1)
            {
                testResult &= (System.Math.Round(base1 + d) == base1);
                Log.Comment(d.ToString() + " " + (System.Math.Round(base1 + d)).ToString());
            }
            for (double d = .6; d < 1; d += .1)
            {
                testResult &= (System.Math.Round(base1 + d) == base1 + 1);
                Log.Comment(d.ToString() + " " + (System.Math.Round(base1 + d)).ToString());
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SystemMath7_Max_Test()
        {
            /// <summary>
            /// 1. Tests the Math.Max method
            /// </summary>
            ///
            Log.Comment("Tests the Math.Max method");
            bool testResult = true;
            Random random = new Random();
            int big = 51 + random.Next(1000);
            Log.Comment("With " + big);
            int small = random.Next(50);
            Log.Comment("and " + small);
            testResult &= (System.Math.Max(big,small) == big);
            testResult &= (System.Math.Max(small,big) == big);
            testResult &= (System.Math.Max(small, -big) == small);
            testResult &= (System.Math.Max(-small, -big) == -small); 
            testResult &= (System.Math.Max(0, small) == small);
            testResult &= (System.Math.Max(-small, 0) == 0);
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SystemMath8_Min_Test()
        {
            /// <summary>
            /// 1. Tests the Math.Min method
            /// </summary>
            ///
            Log.Comment("Tests the Math.Min method");
            bool testResult = true;
            Random random = new Random();
            int big = 51 + random.Next(1000);
            Log.Comment("With " + big);
            int small = random.Next(50);
            Log.Comment("and " + small);
            testResult &= (System.Math.Min(big, small) == small);
            testResult &= (System.Math.Min(small, big) == small);
            testResult &= (System.Math.Min(small, -big) == -big);
            testResult &= (System.Math.Min(-small, -big) == -big);
            testResult &= (System.Math.Min(0, small) == 0);
            testResult &= (System.Math.Min(-small, 0) == -small);
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults SystemMath5_Pow_Test()
        {
            /// <summary>
            /// 1. Tests the Math.Pow method
            /// </summary>
            ///
            Log.Comment("Tests that the Math.Pow method");
            bool testResult = true;
            double base1 = new Random().Next(10);
            Log.Comment("with " + base1);
            double result = 0;
            for (double d = 0; d <= 10; d++)
            {
                result = 1;
                for (int i = 1; i <= d; i++)
                    result *= base1;
                testResult &= (result == System.Math.Pow(base1,d));
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }
    }
}
