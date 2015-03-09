////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT.Platform.Test;
using System.Diagnostics;
using System.Collections;
using System.Threading;

namespace Microsoft.SPOT.Platform.Tests
{
    public class Strings : IMFTestInterface
    {        
        private long start, stop;
        private ArrayList results = new ArrayList();
        private enum StringTests
        {
            Ctor0,
            Ctor1,
            Ctor2,
            Compare,
            Concat0,
            Concat1,
            Concat2,
            Concat3,
            Concat4,
            Concat5,
            Concat6,
            Concat7,
            Empty,
            Equals0,
            Equals1,
            Intern,
            IsInterned,
            ReferenceEquals,
            CompareTo0,
            CompareTo1,
            Equals2,
            GetHashCode,
            GetType,
            IndexOf0,
            IndexOf1,
            IndexOf2,
            IndexOf3,
            IndexOf4,
            IndexOf5,
            IndexOfAny0,
            IndexOfAny1,
            IndexOfAny2,
            LastIndexOf0,
            LastIndexOf1,
            LastIndexOf2,
            LastIndexOf3,
            LastIndexOf4,
            LastIndexOf5,
            LastIndexOfAny0,
            LastIndexOfAny1,
            LastIndexOfAny2,
            Length,
            Split0,
            Split1,
            SubString0,
            SubString1,
            ToCharArray0,
            ToCharArray1,
            ToLower,
            ToString,
            ToUpper,
            Trim0,
            Trim1,
            TrimEnd,
            TrimStart
        }

        private string[] StringTestsList =
        {
            "String(char[] value)",
            "String(char c, int count)",
            "String(char[] value, int startIndex, int length)",
            "String.Compare(string strA, string strB)",
            "String.Concat(object arg0)",
            "String.Concat(params object[] args)",
            "String.Concat(params string[] values)",
            "String.Concat(object arg0, object arg1)",
            "String.Concat(string str0, string str1)",
            "String.Concat(object arg0, object arg1, object arg3)",
            "String.Concat(string str0, string str1, string str2)",
            "String.Concat(string str0, string str1, string str2, string str3)",
            "String.Empty",
            "String.Equals(object objA, object objB)",
            "String.Equals(string a, string b)",
            "String.Intern(string str)",
            "String.IsInterned(string str)",
            "String.ReferenceEquals(object objA, object objB)",
            "String.CompareTo(object value)",
            "String.CompareTo(string strB)",
            "Object.Equals(object obj)",
            "Object.GetHashCode()",
            "Object.GetType()",
            "String.IndexOf(char value)",
            "String.IndexOf(string value)",
            "String.IndexOf(char value, int startIndex)",
            "String.IndexOf(string value, int startIndex)",
            "String.IndexOf(char value, int startIndex, int count)",
            "String.IndexOf(string value, int startIndex, int count)",
            "String.IndexOfAny(char[] anyOf)",
            "String.IndexOfAny(char[] anyOf, int startIndex)",
            "String.IndexOfAny(char[] anyOf, int startIndex, int count)",
            "String.LastIndexOf(char value)",
            "String.LastIndexOf(string value)",
            "String.LastIndexOf(char value, int startIndex)",
            "String.LastIndexOf(string value, int startIndex)",
            "String.LastIndexOf(char value, int startIndex, int count)",
            "String.LastIndexOf(string value, int startIndex, int count)",
            "String.LastIndexOfAny(char[] anyOf)",
            "String.LastIndexOfAny(char[] anyOf, int startIndex)",
            "String.LastIndexOfAny(char[] anyOf, int startIndex, int count)",
            "String.Length",
            "String.Split(params char[] seperator)",
            "String.Split(char[] seperator, int count)",
            "String.SubString(int startIndex)",
            "String.SubString(int startIndex, int length)",
            "String.ToCharArray()",
            "String.ToCharArray(int startIndex, int length)",
            "String.ToLower()",
            "String.ToString()",
            "String.ToUpper()",
            "String.Trim()",
            "String.Trim(param char[] trimChars)",
            "String.TrimEnd(param char[] trimChars)",
            "String.TrimStart(param char[] trimChars)"
        };

        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");
            // Add your functionality here.   

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Gathering up the test results.");

            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].ToString() != ",")
                {
                    Log.Comment(results[i].ToString().TrimEnd(','));
                }
                System.Threading.Thread.Sleep(1000);
            }
        }

        [TestMethod]
        public MFTestResults Ctor_Test_0()
        {
            RunTest(StringTests.Ctor0);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults Ctor_Test_1()
        {
            RunTest(StringTests.Ctor1);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults Ctor_Test_2()
        {
            RunTest(StringTests.Ctor2);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults Compare_Test()
        {
            RunTest(StringTests.Compare);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults Concat_Test_0()
        {
            RunTest(StringTests.Concat0);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults Concat_Test_1()
        {
            RunTest(StringTests.Concat1);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults Concat_Test_2()
        {
            RunTest(StringTests.Concat2);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults Concat_Test_3()
        {
            RunTest(StringTests.Concat3);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults Concat_Test_4()
        {
            RunTest(StringTests.Concat4);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults Concat_Test_5()
        {
            RunTest(StringTests.Concat5);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults Concat_Test_6()
        {
            RunTest(StringTests.Concat6);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults Concat_Test_7()
        {
            RunTest(StringTests.Concat7);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults Empty_Test()
        {
            RunTest(StringTests.Empty);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults Equals_Test_0()
        {
            RunTest(StringTests.Equals0);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults Equals_Test_1()
        {
            RunTest(StringTests.Equals1);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults Intern_Test()
        {
            RunTest(StringTests.Intern);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults IsInterned_Test()
        {
            RunTest(StringTests.IsInterned);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults ReferenceEquals_Test()
        {
            RunTest(StringTests.ReferenceEquals);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults CompareTo_Test0()
        {
            RunTest(StringTests.CompareTo0);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults CompareTo_Test1()
        {
            RunTest(StringTests.CompareTo1);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults Equals_Test2()
        {
            RunTest(StringTests.Equals2);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults GetHashCode_Test()
        {
            RunTest(StringTests.GetHashCode);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults GetType_Test()
        {
            RunTest(StringTests.GetType);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults IndexOf_Test_0()
        {
            RunTest(StringTests.IndexOf0);
            return MFTestResults.Skip;
        }

        //[TestMethod]
        //public MFTestResults IndexOf_Test_1()
        //{
        //    RunTest(StringTests.IndexOf1);
        //    return MFTestResults.Skip;
        //}

        [TestMethod]
        public MFTestResults IndexOf_Test_2()
        {
            RunTest(StringTests.IndexOf2);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults IndexOf_Test_3()
        {
            RunTest(StringTests.IndexOf3);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults IndexOf_Test_4()
        {
            RunTest(StringTests.IndexOf4);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults IndexOf_Test_5()
        {
            RunTest(StringTests.IndexOf5);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults IndexOfAny_Test_0()
        {
            RunTest(StringTests.IndexOfAny0);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults IndexOfAny_Test_1()
        {
            RunTest(StringTests.IndexOfAny1);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults IndexOfAny_Test_2()
        {
            RunTest(StringTests.IndexOfAny2);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults LastIndexOf_Test_0()
        {
            RunTest(StringTests.LastIndexOf0);
            return MFTestResults.Skip;
        }

        //[TestMethod]
        //public MFTestResults LastIndexOf_Test_1()
        //{
        //    RunTest(StringTests.LastIndexOf1);
        //    return MFTestResults.Skip;
        //}

        [TestMethod]
        public MFTestResults LastIndexOf_Test_2()
        {
            RunTest(StringTests.LastIndexOf2);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults LastIndexOf_Test_3()
        {
            RunTest(StringTests.LastIndexOf3);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults LastIndexOf_Test_4()
        {
            RunTest(StringTests.LastIndexOf4);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults LastIndexOf_Test_5()
        {
            RunTest(StringTests.LastIndexOf5);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults Length_Test()
        {
            RunTest(StringTests.Length);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults Split_Test_0()
        {
            RunTest(StringTests.Split0);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults Split_Test_1()
        {
            RunTest(StringTests.Split1);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults SubString_Test_0()
        {
            RunTest(StringTests.SubString0);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults SubString_Test_1()
        {
            RunTest(StringTests.SubString1);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults ToCharArray_Test_0()
        {
            RunTest(StringTests.ToCharArray0);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults ToCharArray_Test_1()
        {
            RunTest(StringTests.ToCharArray1);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults ToLower_Test()
        {
            RunTest(StringTests.ToLower);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults ToString_Test()
        {
            RunTest(StringTests.ToString);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults ToUpper_Test()
        {
            RunTest(StringTests.ToUpper);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults Trim_Test_0()
        {
            RunTest(StringTests.Trim0);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults Trim_Test_1()
        {
            RunTest(StringTests.Trim1);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults TrimEnd_Test()
        {
            RunTest(StringTests.TrimEnd);
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults TrimStart_Test()
        {
            RunTest(StringTests.TrimStart);
            return MFTestResults.Skip;
        }

        private void RunTest(StringTests test)
        {
            int maxLength = 3000;
            int iterations = 10;
            int[] time = new int[iterations];
            int currLength = 1;
            double duration = 0;

            while (currLength < maxLength + 1)
            {
                for (int i = 0; i < iterations; i++)
                {
                    switch (test)
                    {
                        case StringTests.Ctor0:
                            String str0;
                            char[] token00 = GetTokenCharArray(currLength);
                            start = DateTime.Now.Ticks;
                            str0 = new String(token00);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.Ctor1:
                            String str1;
                            char token01 = GetTokenCharArray(1)[0];
                            start = DateTime.Now.Ticks;
                            str1 = new String(token01, currLength);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.Ctor2:
                            String str2;
                            char[] token02 = GetTokenCharArray(currLength);
                            start = DateTime.Now.Ticks;
                            str2 = new String(token02, 0, currLength);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.Compare:
                            int compResult;
                            String token11 = new String(GetTokenCharArray(currLength));
                            String token12 = new String(GetTokenCharArray(currLength));
                            start = DateTime.Now.Ticks;
                            compResult = String.Compare(token11, token12);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.Concat0:
                            String concatString0;
                            object token200 = (object)GetTokenCharArray(currLength);
                            start = DateTime.Now.Ticks;
                            concatString0 = String.Concat(token200);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.Concat1:
                            String concatString1;
                            object[] token201 = GetTokenObjectArray(currLength);
                            start = DateTime.Now.Ticks;
                            concatString1 = String.Concat(token201);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.Concat2:
                            String concatString2;
                            string[] token202 = GetTokenStringArray(currLength);
                            start = DateTime.Now.Ticks;
                            concatString2 = String.Concat(token202);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.Concat3:
                            String concatString3;
                            object token203 = (object)GetTokenCharArray(currLength);
                            object token204 = (object)GetTokenCharArray(currLength);
                            start = DateTime.Now.Ticks;
                            concatString3 = String.Concat(token203, token204);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.Concat4:
                            String concatString4;
                            String token205 = new String(GetTokenCharArray(currLength));
                            String token206 = new String(GetTokenCharArray(currLength));
                            start = DateTime.Now.Ticks;
                            concatString4 = String.Concat(token205, token206);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.Concat5:
                            String concatString5;
                            object token207 = (object)GetTokenCharArray(currLength);
                            object token208 = (object)GetTokenCharArray(currLength);
                            object token209 = (object)GetTokenCharArray(currLength);
                            start = DateTime.Now.Ticks;
                            concatString5 = String.Concat(token207, token208, token209);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.Concat6:
                            String concatString6;
                            String token210 = new String(GetTokenCharArray(currLength));
                            String token211 = new String(GetTokenCharArray(currLength));
                            String token212 = new String(GetTokenCharArray(currLength));
                            start = DateTime.Now.Ticks;
                            concatString6 = String.Concat(token210, token211, token212);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.Concat7:
                            String concatString7;
                            String token213 = new String(GetTokenCharArray(currLength));
                            String token214 = new String(GetTokenCharArray(currLength));
                            String token215 = new String(GetTokenCharArray(currLength));
                            String token216 = new String(GetTokenCharArray(currLength));
                            start = DateTime.Now.Ticks;
                            concatString7 = String.Concat(token213, token214, token215, token216);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.Empty:
                            String token3;
                            start = DateTime.Now.Ticks;
                            token3 = String.Empty;
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.Equals0:
                            bool equalsResult0;
                            object token41 = (object)GetTokenCharArray(currLength);
                            object token42 = (object)GetTokenCharArray(currLength);
                            start = DateTime.Now.Ticks;
                            equalsResult0 = String.Equals(token41, token42);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.Equals1:
                            bool equalsResult1;
                            String token43 = new String(GetTokenCharArray(currLength));
                            String token44 = new String(GetTokenCharArray(currLength));
                            start = DateTime.Now.Ticks;
                            equalsResult1 = String.Equals(token43, token44);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.Equals2:
                            bool equalsResult2;
                            String token45 = new String(GetTokenCharArray(currLength));
                            object token46 = (object)GetTokenCharArray(currLength);
                            start = DateTime.Now.Ticks;
                            equalsResult2 = token45.Equals(token46);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.Intern:
                            string internString;
                            String token5 = new String(GetTokenCharArray(currLength));
                            start = DateTime.Now.Ticks;
                            internString = String.Intern(token5);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.IsInterned:
                            string isInternString;
                            String token6 = new String(GetTokenCharArray(currLength));
                            start = DateTime.Now.Ticks;
                            isInternString = String.IsInterned(token6);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.ReferenceEquals:
                            bool referenceEquals;
                            object token71 = (object)GetTokenCharArray(currLength);
                            object token72 = (object)GetTokenCharArray(currLength);
                            start = DateTime.Now.Ticks;
                            referenceEquals = String.ReferenceEquals(token71, token72);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.CompareTo0:
                            int compareResult0;
                            object token80 = (object)GetTokenCharArray(currLength);
                            String token81 = new String(GetTokenCharArray(currLength));
                            start = DateTime.Now.Ticks;
                            compareResult0 = token81.CompareTo(token80);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.CompareTo1:
                            int compareResult1;
                            String token82 = new String(GetTokenCharArray(currLength));
                            String token83 = new String(GetTokenCharArray(currLength));
                            start = DateTime.Now.Ticks;
                            compareResult1 = token82.CompareTo(token83);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.GetHashCode:
                            int getHashCodeResult;
                            String token9 = new String(GetTokenCharArray(currLength));
                            start = DateTime.Now.Ticks;
                            getHashCodeResult = token9.GetHashCode();
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.GetType:
                            Type getTypeResult;
                            String tokenA = new String(GetTokenCharArray(currLength));
                            start = DateTime.Now.Ticks;
                            getTypeResult = tokenA.GetType();
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.IndexOf0:
                            int indexOfResult0;
                            String tokenB00 = new String(GetTokenCharArray(currLength));
                            char tokenB01 = GetTokenCharArray(currLength)[0];
                            start = DateTime.Now.Ticks;
                            indexOfResult0 = tokenB00.IndexOf(tokenB01);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.IndexOf1:
                            int indexOfResult1;
                            String tokenB10 = new String(GetTokenCharArray(currLength));
                            String tokenB11 = new String(GetTokenCharArray(currLength));
                            start = DateTime.Now.Ticks;
                            Log.Comment("Iteration: " + i);
                            Log.Comment("CurrentLength: " + currLength);
                            indexOfResult1 = tokenB10.IndexOf(tokenB11);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.IndexOf2:
                            int indexOfResult2;
                            String tokenB20 = new String(GetTokenCharArray(currLength));
                            char tokenB21 = GetTokenCharArray(currLength)[0];
                            int tokenB22 = 0;// Math.Random(1);
                            start = DateTime.Now.Ticks;
                            indexOfResult2 = tokenB20.IndexOf(tokenB21, tokenB22);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.IndexOf3:
                            int indexOfResult3;
                            String tokenB30 = new String(GetTokenCharArray(currLength));
                            String tokenB31 = new String(GetTokenCharArray(currLength));
                            int tokenB32 = 0;// Math.Random(1);
                            start = DateTime.Now.Ticks;
                            indexOfResult3 = tokenB30.IndexOf(tokenB31, tokenB32);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.IndexOf4:
                            String tokenB40 = new String(GetTokenCharArray(currLength));
                            char tokenB41 = GetTokenCharArray(currLength)[0];
                            int tokenB42 = 0;// Math.Random(1);
                            int tokenB43;
                            if (currLength == 1)
                            {
                                tokenB43 = 1;
                            }
                            else
                            {
                                tokenB43 = 0;//Math.Random(currLength - 1);
                            }

                            if (currLength == 1)
                            {
                                tokenB43 = 1;
                            }
                            start = DateTime.Now.Ticks;
                            int indexOfResult4 = tokenB40.IndexOf(tokenB41, tokenB42, tokenB43);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.IndexOf5:
                            int indexOfResult5;
                            String tokenB50 = new String(GetTokenCharArray(currLength));
                            String tokenB51 = new String(GetTokenCharArray(currLength));
                            int tokenB52 = 0;// Math.Random(1);
                            int tokenB53;
                            if (currLength == 1)
                            {
                                tokenB53 = 1;
                            }
                            else
                            {
                                tokenB53 = 0;// Math.Random(currLength - 1);
                            }
                            start = DateTime.Now.Ticks;
                            indexOfResult5 = tokenB50.IndexOf(tokenB51, tokenB52, tokenB53);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.IndexOfAny0:
                            int indexOfAnyResult0;
                            char[] tokenC00 = GetTokenCharArray(currLength);
                            String tokenC01 = new String(GetTokenCharArray(currLength));
                            start = DateTime.Now.Ticks;
                            indexOfAnyResult0 = tokenC01.IndexOfAny(tokenC00);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.IndexOfAny1:
                            int indexOfAnyResult1;
                            char[] tokenC10 = GetTokenCharArray(currLength);
                            String tokenC11 = new String(GetTokenCharArray(currLength));
                            int tokenC12 = 0;// Math.Random(1);                            
                            start = DateTime.Now.Ticks;
                            indexOfAnyResult1 = tokenC11.IndexOfAny(tokenC10, tokenC12);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.IndexOfAny2:
                            int indexOfAnyResult2;
                            char[] tokenC20 = GetTokenCharArray(currLength);
                            String tokenC21 = new String(GetTokenCharArray(currLength));
                            int tokenC22 = 0;//Math.Random(1);
                            int tokenC23;
                            if (currLength == 1)
                            {
                                tokenC23 = 1;
                            }
                            else
                            {
                                tokenC23 = 0;//Math.Random(currLength - 1);
                            }

                            if (currLength == 1)
                            {
                                tokenC23 = 1;
                            }

                            start = DateTime.Now.Ticks;
                            indexOfAnyResult2 = tokenC21.IndexOfAny(tokenC20, tokenC22, tokenC23);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.LastIndexOf0:
                            String tokenD00 = new String(GetTokenCharArray(currLength));
                            char tokenD01 = GetTokenCharArray(currLength)[0];
                            start = DateTime.Now.Ticks;
                            int lastindexOfResult0 = tokenD00.LastIndexOf(tokenD01);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.LastIndexOf1:
                            int lastindexOfResult1;
                            String tokenD10 = new String(GetTokenCharArray(currLength));
                            String tokenD11 = new String(GetTokenCharArray(currLength));
                            start = DateTime.Now.Ticks;
                            lastindexOfResult1 = tokenD10.LastIndexOf(tokenD11);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.LastIndexOf2:
                            int lastindexOfResult2;
                            String tokenD20 = new String(GetTokenCharArray(currLength));
                            char tokenD21 = GetTokenCharArray(currLength)[0];
                            int tokenD22 = 0;//Math.Random(1);
                            start = DateTime.Now.Ticks;
                            lastindexOfResult2 = tokenD20.LastIndexOf(tokenD21, tokenD22);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.LastIndexOf3:
                            int lastindexOfResult3;
                            String tokenD30 = new String(GetTokenCharArray(currLength));
                            String tokenD31 = new String(GetTokenCharArray(currLength));
                            int tokenD32 = 0;//Math.Random(1);
                            start = DateTime.Now.Ticks;
                            lastindexOfResult3 = tokenD30.LastIndexOf(tokenD31, tokenD32);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.LastIndexOf4:
                            int lastindexOfResult4;
                            String tokenD40 = new String(GetTokenCharArray(currLength));
                            char tokenD41 = GetTokenCharArray(currLength)[0];
                            int tokenD42 = 0;//Math.Random(1);
                            int tokenD43;
                            if (currLength == 1)
                            {
                                tokenD43 = 1;
                            }
                            else
                            {
                                tokenD43 = 0;//Math.Random(currLength - 1);
                            }

                            start = DateTime.Now.Ticks;
                            lastindexOfResult4 = tokenD40.LastIndexOf(tokenD41, tokenD42, tokenD43);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.LastIndexOf5:
                            String tokenD50 = new String(GetTokenCharArray(currLength));
                            String tokenD51 = new String(GetTokenCharArray(currLength));
                            int tokenD52 = 0;//Math.Random(1);
                            int tokenD53;
                            if (currLength == 1)
                            {
                                tokenD53 = 1;
                            }
                            else
                            {
                                tokenD53 = 0;//Math.Random(currLength - 1);
                            }

                            start = DateTime.Now.Ticks;
                            int lastindexOfResult5 = tokenD50.LastIndexOf(tokenD51, tokenD52, tokenD53);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.LastIndexOfAny0:
                            int lastindexOfAnyResult0;
                            char[] tokenE00 = GetTokenCharArray(currLength);
                            String tokenE01 = new String(GetTokenCharArray(currLength));
                            start = DateTime.Now.Ticks;
                            lastindexOfAnyResult0 = tokenE01.IndexOfAny(tokenE00);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.LastIndexOfAny1:
                            int lastindexOfAnyResult1;
                            char[] tokenE10 = GetTokenCharArray(currLength);
                            String tokenE11 = new String(GetTokenCharArray(currLength));
                            int tokenE12 = 0;//Math.Random(1);
                            start = DateTime.Now.Ticks;
                            lastindexOfAnyResult1 = tokenE11.IndexOfAny(tokenE10, tokenE12);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.LastIndexOfAny2:
                            int lastindexOfAnyResult2;
                            char[] tokenE20 = GetTokenCharArray(currLength);
                            String tokenE21 = new String(GetTokenCharArray(currLength));
                            int tokenE22 = 0;//Math.Random(1);
                            int tokenE23;
                            if (currLength == 1)
                            {
                                tokenE23 = 1;
                            }
                            else
                            {
                                tokenE23 = 0;//Math.Random(currLength - 1);
                            }

                            start = DateTime.Now.Ticks;
                            lastindexOfAnyResult2 = tokenE21.IndexOfAny(tokenE20, tokenE22, tokenE23);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.Length:
                            int lengthResult;
                            String tokenF21 = new String(GetTokenCharArray(currLength));
                            start = DateTime.Now.Ticks;
                            lengthResult = tokenF21.Length;
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.Split0:
                            String[] splitResult0 = new String[currLength];
                            char[] tokenG00 = GetTokenCharArray(currLength);
                            String tokenG01 = new String(tokenG00);
                            start = DateTime.Now.Ticks;
                            splitResult0 = tokenG01.Split(tokenG00);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.Split1:
                            String[] splitResult1 = new String[currLength];
                            char[] tokenG10 = GetTokenCharArray(currLength);
                            String tokenG11 = new String(tokenG10);
                            start = DateTime.Now.Ticks;
                            splitResult1 = tokenG11.Split(tokenG10);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.SubString0:
                            String subStringResult0;
                            int tokenH0 = 0;//Math.Random(1);
                            String tokenH1 = new String(GetTokenCharArray(currLength));
                            start = DateTime.Now.Ticks;
                            subStringResult0 = tokenH1.Substring(tokenH0);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.SubString1:
                            String subStringResult1;
                            int tokenH2 = 0;//Math.Random(1);
                            String tokenH3 = new String(GetTokenCharArray(currLength));
                            int tokenH4;
                            if (currLength == 1)
                            {
                                tokenH4 = 1;
                            }
                            else
                            {
                                tokenH4 = 0;//Math.Random(currLength - 1);
                            }

                            start = DateTime.Now.Ticks;
                            subStringResult1 = tokenH3.Substring(tokenH2, tokenH4);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.ToCharArray0:
                            char[] toCharArrayResult0 = new char[currLength];
                            String tokenI0 = new String(GetTokenCharArray(currLength));
                            start = DateTime.Now.Ticks;
                            toCharArrayResult0 = tokenI0.ToCharArray();
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.ToCharArray1:
                            char[] toCharArrayResult1 = new char[currLength];
                            String tokenI1 = new String(GetTokenCharArray(currLength));
                            int tokenI2 = 0;//Math.Random(1);
                            int tokenI3;
                            if (currLength == 1)
                            {
                                tokenI3 = 1;
                            }
                            else
                            {
                                tokenI3 = 0;//Math.Random(currLength - 1);
                            }

                            start = DateTime.Now.Ticks;
                            toCharArrayResult1 = tokenI1.ToCharArray(tokenI2, tokenI3);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.ToLower:
                            string toLowerResult;
                            String tokenJ1 = new String(GetTokenCharArray(currLength));
                            start = DateTime.Now.Ticks;
                            toLowerResult = tokenJ1.ToLower();
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.ToString:
                            string toStringResult;
                            String tokenK1 = new String(GetTokenCharArray(currLength));
                            start = DateTime.Now.Ticks;
                            toStringResult = tokenK1.ToString();
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.ToUpper:
                            string toUpperResult;
                            String tokenL1 = new String(GetTokenCharArray(currLength));
                            start = DateTime.Now.Ticks;
                            toUpperResult = tokenL1.ToUpper();
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.Trim0:
                            string trimResult0;
                            String tokenM0 = new String(GetTokenCharArray(currLength));
                            start = DateTime.Now.Ticks;
                            trimResult0 = tokenM0.Trim();
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.Trim1:
                            string trimResult1;
                            char[] tokenM1 = new char[currLength];
                            String tokenM2 = new String(GetTokenCharArray(currLength));
                            start = DateTime.Now.Ticks;
                            trimResult1 = tokenM2.Trim(tokenM1);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.TrimEnd:
                            string trimEndResult;
                            char[] tokenN0 = new char[currLength];
                            String tokenN1 = new String(GetTokenCharArray(currLength));
                            start = DateTime.Now.Ticks;
                            trimEndResult = tokenN1.TrimEnd(tokenN0);
                            stop = DateTime.Now.Ticks;
                            break;

                        case StringTests.TrimStart:
                            string trimStartResult;
                            char[] tokenO0 = new char[currLength];
                            String tokenO1 = new String(GetTokenCharArray(currLength));
                            start = DateTime.Now.Ticks;
                            trimStartResult = tokenO1.TrimStart(tokenO0);
                            stop = DateTime.Now.Ticks;
                            break;
                    }

                    duration += stop - start;
                }

                double callTime = duration / (iterations * 10000);

                if (currLength < maxLength)
                {
                    results.Add(StringTestsList[(int)test] + ":" + currLength + ":" + callTime + "ms,");
                }
                else
                {
                    results.Add(StringTestsList[(int)test] + ":" + currLength + ":" + callTime + "ms");
                }

                if (currLength == maxLength)
                {
                    break;
                }

                currLength *= 10;

                if (currLength > maxLength)
                {
                    currLength = maxLength;
                }
            }
        }

        private char[] GetTokenCharArray(int length)
        {
            char[] token = new char[length];

            for (int i = 0; i < length; i++)
            {
                //UInt16 val = (UInt16)Math.Random(100);
                //token[i] = Convert.ToChar(val);
                if (i % 2 == 0)
                {
                    token[i] = 'a';
                }
                else
                {
                    token[i] = '\u0066';
                }
            }

            return token;
        }

        private object[] GetTokenObjectArray(int length)
        {
            object[] token = new object[length];

            for (int i = 0; i < length; i++)
            {
                //UInt16 val = (UInt16)Math.Random(126);
                //token[i] = Convert.ToChar(val);
                if (i % 2 == 0)
                {
                    token[i] = 'a';
                }
                else
                {
                    token[i] = "\uD840DC99";
                }
            }

            return token;
        }

        private string[] GetTokenStringArray(int length)
        {
            string[] token = new string[length];

            for (int i = 0; i < length; i++)
            {
                //UInt16 val = (UInt16)Math.Random(126);
                //token[i] = Convert.ToChar(val).ToString();
                if (i % 2 == 0)
                {
                    token[i] = "a";
                }
                else
                {
                    token[i] = "\uD840DC99";
                }
            }

            return token;
        }
    }
}
