////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;
using System.Threading;
using Microsoft.SPOT.Platform.Test;


namespace Microsoft.SPOT.Platform.Tests
{
    public class Utf8EncodingTests : IMFTestInterface
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

        [TestMethod]
        public MFTestResults Utf8EncodingTests_Test1()
        {
            bool result = true;

            string str = "this is a normal string that will be used to convert to bytes then back to a string";

            byte[] data = new byte[128];
            int len = str.Length;
            int idx = 0;
            Random rand = new System.Random();
            int cBytes = 0;

            while(len > 0)
            {
                int size = (len <= 2) ? len : rand.Next(len/2) + 1;
                len -= size;

                int cnt = UTF8Encoding.UTF8.GetBytes(str, idx, size, data, cBytes);

                result &= str.Substring(idx, size) == new string(UTF8Encoding.UTF8.GetChars(data, cBytes, cnt));

                cBytes += cnt;
                idx += size;
            }

            result &= cBytes == str.Length;

            string strAfter = new string(UTF8Encoding.UTF8.GetChars(data, 0, cBytes));
            result &= (str == strAfter);
            
            return result ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Utf8EncodingTests_Test2()
        {
            bool result = true;

            string str = "this is a normal string that will be used to convert to bytes then back to a string";

            byte[] data = UTF8Encoding.UTF8.GetBytes(str);

            result &= data.Length == str.Length;

            string strAfter = new string(UTF8Encoding.UTF8.GetChars(data));

            result &= (str == strAfter);

            return result ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults Utf8EncodingTests_Test3()
        {
            // This tests involves a string with a special character

            bool result = true;

            string str = "AB\u010DAB";

            byte[] data = new byte[4];
            int count = UTF8Encoding.UTF8.GetBytes(str, 1, 3, data, 0);

            result &= count == 4;

            result &= (new string(UTF8Encoding.UTF8.GetChars(data)) == "B\u010DA");

            return result ? MFTestResults.Pass : MFTestResults.Fail;
        }
    }
}