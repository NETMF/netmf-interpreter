////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class SystemStringTests : IMFTestInterface
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

        //String Test methods
        [TestMethod]
        public MFTestResults Ctor_Test()
        {
            /// <summary>
            /// 1. Tests the constructors of the string type
            /// </summary>
            ///
            Log.Comment("Test of the standard constructor");
            bool testResult = true;
            char[] car = new Char[] { 'a', 'b', 'c', 'd' };

            Log.Comment("Char [], start, number");
            string str = new string(car, 1, 2);
            testResult &= (str == "bc");

            str = new string(car, 0, 4);
            testResult &= (str == "abcd");
            
            Log.Comment("Char []");
            str = new string(car);
            testResult &= (str == "abcd");

            Log.Comment("Char, number");
            str = new string('\n', 33);
            testResult &= (str.Length == 33);
            for (int i = 0; i < str.Length; i++)
                testResult &= (str[i] == '\n');

            Log.Comment("Char, string terminator known failure. ");
            char[] car2 = new char[] { (char)0, (char)65};
            string s = new string(car2);
            testResult &= (s == "\0A");
            Log.Comment("This was previously bug 20620");

            Log.Comment("new char[0]");
            str = new string(new char[0]);
            testResult &= (str == string.Empty);

            Log.Comment("null");
            str = new string(null);
            testResult &= (str == string.Empty);

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }
        [TestMethod]
        public MFTestResults CompareTo_Test3()
        {            
            /// <summary>
            /// 1. Tests the CompareTo methods of the string type
            /// </summary>
            ///
            Log.Comment("Test of the CompareTo method");
            bool testResult = true;
            string str = "hello";
            object ob = "Hello";
            Log.Comment("NormalCompareTo");
            testResult &= (str.CompareTo((object)"hello") == 0);
            testResult &= (str.CompareTo(ob) > 0);
            testResult &= (str.CompareTo((object)"zello") < 0);
            Log.Comment("CompareTo null");
            testResult &= (str.CompareTo((object)null) > 0);

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }
        [TestMethod]
        public MFTestResults GetHashCode_Test4()
        {
            /// <summary>
            /// 1. Tests the GetHashCode methods of the string type
            /// </summary>
            ///
            Log.Comment("Test of the GetHashCode method");
            bool testResult = true;
            string[] strs = new string[] { "abcd", "bcda", "cdab", "dabc" };

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    Log.Comment(strs[i].GetHashCode().ToString() + " - " + 
                        strs[j].GetHashCode().ToString());
                    if (i == j)
                        testResult &= (strs[i].GetHashCode() == strs[j].GetHashCode());
                    else
                        testResult &= (strs[i].GetHashCode() != strs[j].GetHashCode());
                }
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }
        [TestMethod]
        public MFTestResults Equals_Test5()
        {
            /// <summary>
            /// 1. Tests the Equals methods of the string type
            /// </summary>
            ///
            Log.Comment("This verifies the String.Equals functionality.");
            Log.Comment("It compares the string value using the Equals function");
            Log.Comment("to valid and invalid values as well as casted object values.");
            bool testResult = true;
            string str = "abcd";
            object ob2 = "bcd";
            object ob = str as object;

            testResult &= str.Equals(ob);
            Log.Comment("testResult == " + testResult.ToString());
            testResult &= !str.Equals((object)123);
            Log.Comment("testResult == " + testResult.ToString());
            testResult &= str.Equals((object)"abcd");
            Log.Comment("testResult == " + testResult.ToString());
            testResult &= !str.Equals((object)"bcd");
            Log.Comment("testResult == " + testResult.ToString());
            testResult &= !str.Equals(ob2);
            Log.Comment("testResult == " + testResult.ToString());
            string str1 = "abc\n";
            string str2 = "abcd";
            string str3 = "abc\n";
            testResult &= str1.Equals(str3);
            Log.Comment("testResult == " + testResult.ToString());
            testResult &= !str1.Equals(str2);
            Log.Comment("testResult == " + testResult.ToString());
            testResult &= str3.Equals(str1);
            Log.Comment("testResult == " + testResult.ToString());
            testResult &= str3.Equals("abc" + "\n");
            Log.Comment("testResult == " + testResult.ToString());
            testResult &= str2.Equals("a" + "b" + 'c' + "d");
            Log.Comment("testResult == " + testResult.ToString());

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }
        [TestMethod]
        public MFTestResults ToString_Test6()
        {
            /// <summary>
            /// 1. Tests the ToString method of the string type
            /// </summary>
            ///
            Log.Comment("Test of the ToString method");
            bool testResult = true;
            string str = "abc";
            testResult &= (str == str.ToString());
            testResult &= (str == str.ToString().ToString().ToString());
            testResult &= (str.ToString() == "abc");

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults ToCharArray_Test13()
        {
            /// <summary>
            /// 1. Tests the ToCharArray methods of the string type
            /// Epected Failure
            /// </summary>
            ///
            Log.Comment("Test of the ToCharArray method"); 
            bool testResult = true;
            char[] car1 = new Char[] { 'a', 'b', 'c', 'd' };
            char[] car2 = new Char[] { };

            string str1 = "abcd";
            string str2 = "abcde";
            string str3 = "ABCD";

            Log.Comment("With 0 args");
            testResult &= (str1.ToCharArray() == car1);
            testResult &= (str2.ToCharArray() != car1);
            testResult &= (str3.ToCharArray() != car1);
            testResult &= (str1.ToCharArray() != car2);

            Log.Comment("With 1 args");
            testResult &= (str1.ToCharArray(0,3) != car1);
            testResult &= (str2.ToCharArray(0,3) == car1);
            testResult &= (str3.ToCharArray(0,3) != car1);
            testResult &= (str1.ToCharArray(1,3) != car2);

            if (testResult)
            {
                Log.Comment("This failure indicates a test is now passing that previously failed by design.");
                Log.Comment("It previously marked as known failure because of bug # 14574");
                Log.Comment("The Test owner needs to verify that the change was intentional and remove the known failure.");
                return MFTestResults.Fail;
            }
            else
                return MFTestResults.Pass;
        }
        [TestMethod]
        public MFTestResults Split_Test15()
        {
            /// <summary>
            /// 1. Tests the Split method of the string type
            /// </summary>
            ///
            bool testResult = true;
            Log.Comment("Testing the Split method");
            char[] car1 = new Char[] { '@','q' };
            char[] car2 = new Char[] { };
            string str1 = "ab@cd";
            string str2 = "abcd@";

            testResult &= (str1.Split(car1)[0] == "ab");
            testResult &= (str1.Split(car1)[1] == "cd");
            testResult &= (str2.Split(car1)[0] == "abcd");
            testResult &= (str2.Split(car1)[1] == "");
            testResult &= (str1.Split(car2)[0] == "ab@cd");

            Log.Comment("Verify split with a count");
            Log.Comment("This is currently a known issue");
            Log.Comment("20659	String.Split with a count parameter always returns the whole string.");
            string[] oneTwoThree = "1 2 3".Split(new char[] { ' ' }, 1);
            testResult &= (oneTwoThree.Length <= 1);

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }
        [TestMethod]
        public MFTestResults Substring_Test17()
        {
            /// <summary>
            /// 1. Tests the Substring method of the string type
            /// </summary>
            ///
            bool testResult = true;
            Log.Comment("Testing the Substring method");
            string str1 = "abcde";

            testResult &= (str1.Substring(0) == str1);
            testResult &= (str1.Substring(0,5) == str1);
            testResult &= (str1.Substring(2) == "cde");
            testResult &= (str1.Substring(2,1) == "c");
            
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults Trim_Test19()
        {
            /// <summary>
            /// 1. Tests the Trim method of the string type
            /// </summary>
            ///
            bool testResult = true;
            Log.Comment("Testing the Trim method");
            
            char[] car1 = new Char[] { '@', 'q' };
            string str1 = " abc@de ";
            string str2 = "@abc  @  de@";
            testResult &= (str1.Trim() == "abc@de");
            testResult &= (str1.Trim(car1) == " abc@de ");
            testResult &= (str2.Trim() == "@abc  @  de@");
            testResult &= (str2.Trim(car1) == "abc  @  de");

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }
        [TestMethod]
        public MFTestResults TrimStart_Test20()
        {
            /// <summary>
            /// 1. Tests the TrimStart method of the string type
            /// </summary>
            ///
            bool testResult = true;
            Log.Comment("Testing the TrimStart method");

            char[] car1 = new Char[] { '@', 'q' };
            string str1 = " abc@de ";
            string str2 = "@abc  @  de@";
            testResult &= (str1.TrimStart() == "abc@de ");
            testResult &= (str1.TrimStart(car1) == " abc@de ");
            testResult &= (str2.TrimStart() == "@abc  @  de@");
            testResult &= (str2.TrimStart(car1) == "abc  @  de@");
            //System.String _string = new System.String( );
            //System.String var = _string.TrimStart( System.Char[] );
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }
        [TestMethod]
        public MFTestResults TrimEnd_Test21()
        {
            /// <summary>
            /// 1. Tests the TrimEnd method of the string type
            /// </summary>
            ///
            bool testResult = true;
            Log.Comment("Testing the TrimEnd method");

            char[] car1 = new Char[] { '@', 'q' };
            string str1 = " abc@de ";
            string str2 = "@abc  @  de@";
            testResult &= (str1.TrimEnd() == " abc@de");
            testResult &= (str1.TrimEnd(car1) == " abc@de ");
            testResult &= (str2.TrimEnd() == "@abc  @  de@");
            testResult &= (str2.TrimEnd(car1) == "@abc  @  de");
            //System.String _string = new System.String( );
            //System.String var = _string.TrimEnd( System.Char[] );
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }
        
        [TestMethod]
        public MFTestResults IndexOf_Test28()
        {
            /// <summary>
            /// 1. Tests the IndexOf method of the string type
            /// </summary>
            ///
            bool testResult = true;
            Log.Comment("Testing the IndexOf method");

            string str1 = "@ abc@de ";
            testResult &= (str1.IndexOf('@') == 0);
            testResult &= (str1.IndexOf("abc") == 2);
            testResult &= (str1.IndexOf('@', 1) == 5);
            testResult &= (str1.IndexOf('@', 1, 1) == -1);
            testResult &= (str1.IndexOf("abc",2) == 2);
            testResult &= (str1.IndexOf("abc", 1,1) == -1);

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }
        [TestMethod]
        public MFTestResults IndexOfAny_Test31()
        {
            /// <summary>
            /// 1. Tests the IndexOfAny method of the string type
            /// </summary>
            ///
            bool testResult = true;
            Log.Comment("Testing the IndexOfAny method");

            string str1 = "@ abc@de ";
            char[] car1 = new Char[] { '@', 'b' };

            testResult &= (str1.IndexOfAny(car1) == 0);
            testResult &= (str1.IndexOfAny(car1, 1) == 3);
            testResult &= (str1.IndexOfAny(car1, 2, 1) == -1);

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }
        [TestMethod]
        public MFTestResults LastIndexOf_Test37()
        {
            /// <summary>
            /// 1. Tests the LastIndexOf method of the string type
            /// </summary>
            ///
            bool testResult = true;
            Log.Comment("Testing the LastIndexOf method");

            string str1 = "@ abc@de ";
            testResult &= (str1.LastIndexOf('@') == 5);
            testResult &= (str1.LastIndexOf("abc") == 2);
            testResult &= (str1.LastIndexOf('@', 1) == 5);
            testResult &= (str1.LastIndexOf('@', 1, 1) == -1);
            testResult &= (str1.LastIndexOf("abc", 2) == 2);
            testResult &= (str1.LastIndexOf("@", 6, 1) == -1);

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }
        [TestMethod]
        public MFTestResults LastIndexOfAny_Test40()
        {
            /// <summary>
            /// 1. Tests the LastIndexOfAny method of the string type
            /// </summary>
            ///
            bool testResult = true;
            Log.Comment("Testing the LastIndexOfAny method");

            string str1 = "@ abc@de ";
            char[] car1 = new Char[] { '@', 'b' };

            testResult &= (str1.LastIndexOfAny(car1) == 5);
            testResult &= (str1.LastIndexOfAny(car1, 1) == 5);
            testResult &= (str1.LastIndexOfAny(car1, 4, 1) == -1);

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }
        [TestMethod]
        public MFTestResults ToLower_Test51()
        {
            /// <summary>
            /// 1. Tests the ToLower method of the string type
            /// </summary>
            ///
            bool testResult = true;
            Log.Comment("Testing the ToLower method");

            string str1 = "@ ABC@de ";
            testResult &= (str1.ToLower() == "@ abc@de ");
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }
        [TestMethod]
        public MFTestResults ToUpper_Test52()
        {
            /// <summary>
            /// 1. Tests the ToUpper method of the string type
            /// </summary>
            ///
            bool testResult = true;
            Log.Comment("Testing the ToUpper method"); ;

            string str1 = "@ ABC@de ";
            testResult &= (str1.ToUpper() == "@ ABC@DE ");  
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }


        [TestMethod]
        public MFTestResults Length_Test71()
        {
            /// <summary>
            /// 1. Tests the Length property of the string type
            /// </summary>
            ///
            bool testResult = true;
            Log.Comment("Testing the Length property"); ;

            string str1 = "@ ABC@de ";
            testResult &= (str1.Length == 9);
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }
        
        [TestMethod]
        public MFTestResults Concat_Test1()
        {
            /// <summary>
            /// 1. Tests the string concat of several object where one of the arguments returns a null value for ToString()
            /// </summary>
            ///
            
            try
            {
                string str = "a" + 1 + "b" + new ToStringReturnsNull();
                return (str == "a1b" ? MFTestResults.Pass : MFTestResults.Fail);
            }
            catch
            {
                return MFTestResults.Fail;
            }
        }
    }

    /// <summary>
    /// A class whose ToString method return null
    /// </summary>
    public class ToStringReturnsNull
    {
        public override string ToString()
        {
            return null;
        }
    }
}
