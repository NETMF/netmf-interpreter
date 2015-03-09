////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Presentation.Controls;
using System.Threading;
using Microsoft.SPOT.Platform.Test;


namespace Microsoft.SPOT.Platform.Tests
{
    public class ColorTests : Master_Media, IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for Color tests.");
            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");

        }

        [TestMethod]
        public MFTestResults Color_Test1()
        {
            MFTestResults testResult = MFTestResults.Pass;
            Color darkBlue = ColorUtility.ColorFromRGB(0x00, 0x00, 0x8B);
            if (darkBlue != (Color)0x8B0000)
            {
                testResult = MFTestResults.Fail;
            }
            if (darkBlue != (Color)9109504)
            {
                testResult = MFTestResults.Fail;
            }           

            return testResult;
        }

        [TestMethod]
        public MFTestResults Color_ColorFromRGBTest2()
        {
            MFTestResults testResult = MFTestResults.Pass;
            Color c = ColorUtility.ColorFromRGB(0, 0, 0);
            if (Colors.Black != c)
            {
                Log.Comment("Failure : expected Black(" + c.ToString() +
                    ") but got " + Colors.Black.ToString());
                testResult = MFTestResults.Fail;
            }
            c = ColorUtility.ColorFromRGB(0, 128, 0);
            if (Colors.Green != c)
            {
                Log.Comment("Failure : expected Green(" + c.ToString() +
                    ") but got " + Colors.Green.ToString());
                testResult = MFTestResults.Fail;
            }
            c = ColorUtility.ColorFromRGB(0, 0, 255);
            if (Colors.Blue != c)
            {
                Log.Comment("Failure : expected Blue(" + c.ToString() +
                    ") but got " + Colors.Blue.ToString());
                testResult = MFTestResults.Fail;
            }
            c = ColorUtility.ColorFromRGB(0x80, 0x80, 0x80);
            if (Colors.Gray != c)
            {
                testResult = MFTestResults.Fail;
            }
            c = ColorUtility.ColorFromRGB(255, 255, 255);
            if (Color.White != c)
            {
                Log.Comment("Failure : expected White(" + c.ToString() +
                    ") but got " + Colors.Black.ToString());
                testResult = MFTestResults.Fail;
            }
            Color darkBlue = (Color)0x8B0000;
            c = ColorUtility.ColorFromRGB(0x00, 0x00, 0x8B);
            if (darkBlue != c)
            {
                Log.Comment("Failure : expected darkBlue(" + c.ToString() +
                    ") but got " + Colors.Black.ToString());
                testResult = MFTestResults.Fail;
            }
            return testResult;
        }

        [TestMethod]
        public MFTestResults Color_GetRValueTest3()
        {
            MFTestResults testResult = MFTestResults.Pass;
            byte b, g, rValue;
            Color c;
            Random random = new Random();

            for (int r = 0; r <= 255; r++)
            {
                g = (byte)random.Next();
                b = (byte)random.Next();
                c = ColorUtility.ColorFromRGB((byte)r, g, b);
                rValue = ColorUtility.GetRValue(c);
                if (rValue != r)
                {
                    Log.Comment("Expecting Blue value of " + r.ToString() + " got " +
                        rValue.ToString() + " for Color = " + c.ToString());
                    testResult = MFTestResults.Fail;
                }
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults Color_GetGValueTest4()
        {
            MFTestResults testResult = MFTestResults.Pass;
            byte r, b, gValue;
            Color c;
            Random random = new Random();

            for (int g = 0; g <= 255; g++)
            {
                r = (byte)random.Next();
                b = (byte)random.Next();
                c = ColorUtility.ColorFromRGB(r, (byte)g, b);
                gValue = ColorUtility.GetGValue(c);
                if (gValue != g)
                {
                    Log.Comment("Expecting Blue value of " + g.ToString() + " got " +
                       gValue.ToString() + " for Color = " + c.ToString());
                    testResult = MFTestResults.Fail;
                }
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults Color_GetBValueTest5()
        {
            MFTestResults testResult = MFTestResults.Pass;
            byte r, g, bValue;
            Color c;
            Random random = new Random();

            for (int b = 0; b <= 255; b++)
            {
                r = (byte)random.Next();
                g = (byte)random.Next();
                c = ColorUtility.ColorFromRGB(r, g, (byte)b);
                bValue = ColorUtility.GetBValue(c);
                if (bValue != b)
                {
                    Log.Comment("Expecting Blue value of " + b.ToString() + " got " +
                        bValue.ToString() + " for Color = " + c.ToString());
                    testResult = MFTestResults.Fail;
                }
            }

            return testResult;
        }      
    }
}
