////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Xml;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class XmlNameTableTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for XmlNameTable tests.");

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");
        }

        [TestMethod]
        public MFTestResults AddGetTest1()
        {
            try
            {
                XmlNameTable nt = new XmlNameTable();
                String[] strings = new String[100];
                String[] atomizedStrings = new String[100];
                String str;
                String atomized;


                for(int i =0 ; i <100; i++)
                {
                    str = ToNumString(i);
                    strings[i] = str;

                    atomizedStrings[i] = nt.Add(str);

                    Assert.AreEqual(str, atomizedStrings[i]);
                }

                for (int i = 99; i >=0; i--)
                {
                    str = ToNumString(i);
                    atomized = nt.Get(str);
                    if (Object.ReferenceEquals(str, atomized))
                    {
                        Log.Comment("The new string and atomized string are of the same reference");
                    }

                    Assert.AreEqual(str, atomized);
                    Assert.IsTrue(Object.ReferenceEquals(atomizedStrings[i], atomized), "String not atomized");
                }

            }
            catch (Exception e)
            {
                Log.Exception("Unexpected exception", e);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults AddGetTest2()
        {
            try
            {
                XmlNameTable nt = new XmlNameTable();

                String atomized = nt.Add("http://www.microsoft.com/netmf");

                Assert.AreEqual(null, nt.Get("http://www.microsoft.com/netmf/v4"));
                Assert.AreEqual(null, nt.Get("http://www.microsoft.com/"));
                Assert.AreEqual(null, nt.Get("http://www.microsoft.com/netMF"));
            }
            catch (Exception e)
            {
                Log.Exception("Unexpected exception", e);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults AddEmptyStringTest()
        {
            try
            {
                XmlNameTable nt = new XmlNameTable();

                String atomized = nt.Add("");

                Assert.IsTrue(Object.ReferenceEquals(atomized, String.Empty), "Atomized Empty String is not String.Empty");
            }
            catch (Exception e)
            {
                Log.Exception("Unexpected exception", e);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

        static String[] Numbers = { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine" };

        String ToNumString(int num)
        {
            String str = String.Empty;

            long n = num * num * num;

            do
            {
                str = Numbers[n % 10] + str;

                n /= 10;
            }
            while (n > 0);

            return str;
        }
    }
}
