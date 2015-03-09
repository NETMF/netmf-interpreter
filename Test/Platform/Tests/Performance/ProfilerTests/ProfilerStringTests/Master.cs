////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class Master_ProfilerStringTests
    {
        public static void Main()
        {
            Master_ProfilerStringTests tests = new Master_ProfilerStringTests();
            tests.LengthTest();
            tests.CompareTest();
        }

        public void LengthTest()
        {
            string testString = MFUtilities.GetRandomString();
            int length = testString.Length;
        }
        
        public void CompareTest()
        {
            string testString1 = MFUtilities.GetRandomString();
            string testString2 = MFUtilities.GetRandomString();
            String.Compare(testString1, testString2);
        }
    }
}