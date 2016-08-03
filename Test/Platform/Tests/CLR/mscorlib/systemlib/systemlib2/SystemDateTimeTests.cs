////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT;

namespace Microsoft.SPOT.Platform.Tests
{
    public class SystemDateTimeTests : IMFTestInterface
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

        //DateTime Test methods

        [TestMethod]
        public MFTestResults DateTime_ConstructorTest1()
        {
            /// <summary>
            ///  1. Creates a DateTime
            ///  2. Verifies that the created object is a DateTime
            /// </summary>

            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Creating new DateTime Object");
                DateTime dt = new DateTime();
                Log.Comment(dt.ToString());
                Type type = dt.GetType();
                Log.Comment("Verifying its type");
                if (!(type == Type.GetType("System.DateTime")))
                {
                    Log.Comment("Expected Type 'System.DateTime' but got '" + type.ToString());
                    testResult = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_ConstructorTest2()
        {
            /// <summary>
            ///  1. Creates a DateTime
            ///  2. Verifies that the created object is a DateTime
            /// </summary>
            /// 

            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Generating 10 random DateTime and Verifying");
                for (int i = 0; i < 10; i++)
                {
                    DateTime dt = GetRandomDateTime();
                    if (dt.Year != year || dt.Month != month || dt.Day != day ||
                   dt.Hour != hour || dt.Minute != minute ||
                   dt.Second != second || dt.Millisecond != millisec)
                    {
                        Log.Comment("Expected DateTime '" + month + "/" + day + "/" + year +
                            " " + hour + ":" + minute + ":" + second + ":" + millisec +
                            "' but got '" + dt.Month + "/" + dt.Day + "/" + dt.Year + " " +
                            dt.Hour + ":" + dt.Minute + ":" + dt.Second + ":" + dt.Millisecond + "'");
                        testResult = MFTestResults.Fail;
                    }
                    Type t = dt.GetType();
                    if (t != Type.GetType("System.DateTime"))
                    {
                        Log.Comment("Expected Type 'System.DateTime' but got '" + t.ToString() + "'");
                        testResult = MFTestResults.Fail;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_ConstructorTest3()
        {
            /// <summary>
            ///  1. Creates Minimum and Maximum DateTimes
            ///  2. Verifies the DateTimes are equal to DateTime.MinValue, DateTime.MaxValue
            /// </summary>
            /// 

            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Creating Minimum DateTime and verifying");
                DateTime minDT1 = DateTime.MinValue;
                DateTime minDT2 = new DateTime();
                DateTime minDT3 = new DateTime(0);
                DateTime minDT4 = new DateTime(1601, 1, 1, 0, 0, 0, 0);

                if ((DateTime.Compare(minDT1, minDT2) != 0) ||
                    (DateTime.Compare(minDT2, minDT3) != 0) ||
                    (DateTime.Compare(minDT3, minDT4) != 0))
                {
                    Log.Comment("Expected 'DateTime.MinValue' is equal to 'new DateTime()', " +
                        "equal to 'new DateTime(0)', equal to 'new DateTime(1, 1, 1, 0, 0, 0, 0)' but got ");
                    Log.Comment("DateTime.MinValue = '" + minDT1.Ticks + "'ticks,");
                    Log.Comment(" new DateTime() = '" + minDT2.Ticks + "'ticks,");
                    Log.Comment("new DateTime(0) = '" + minDT3.Ticks + "'ticks.");
                    testResult = MFTestResults.Fail;
                }

                Log.Comment("Creating Maximum DateTime and verifying");
                DateTime maxDateTime = new DateTime(441796895990000000);
                if (!DateTime.MaxValue.Equals(maxDateTime))
                {
                    Log.Comment("Expected Ticks '441796895990000000' but got '" + DateTime.MaxValue.Ticks + "'");
                    testResult = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_CompareToTest4()
        {
            /// <summary>
            ///  1. Creates random DateTimes
            ///  2. Verifies that they CompareTo each other
            /// </summary>
            /// 

            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Generating random DateTimes b/n 1000 - 9000");
                Log.Comment("comparing eachother with DateTime.CompareTo and verifying");
                DateTime dt1 = DateTime_btwn_1801_And_2801();
                DateTime dt2 = new DateTime(year, month, day, hour, minute, second, millisec);
                Log.Comment("Comparing two equal DateTimes");
                if (dt1.CompareTo(dt2) != 0)
                {
                    Log.Comment("Expected " + dt1.ToString() + " .CompareTo( " +
                        dt2.ToString() + " ) returns '0' but got '" + dt1.CompareTo(dt2) + "'");
                    testResult = MFTestResults.Fail;
                }
                Log.Comment("Comparing Unequal DateTimes and Verifying");
                dt2 = dt1.Add(new TimeSpan(1));
                if (dt1.CompareTo(dt2) >= 0)
                {
                    Log.Comment("Expected " + dt1.ToString() + " .CompareTo( " +
                      dt2.ToString() + " ) returns '-ve' value but got '" + dt1.CompareTo(dt2) + "'");
                    testResult = MFTestResults.Fail;
                }
                if (dt2.CompareTo(dt1) <= 0)
                {
                    Log.Comment("Expected " + dt2.ToString() + " .CompareTo( " +
                      dt1.ToString() + " ) returns '+ve' value but got '" + dt2.CompareTo(dt1) + "'");
                    testResult = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_EqualsTest5()
        {
            /// <summary>
            ///  1. Creates two DateTimes
            ///  2. Verifies that they Equals each other
            /// </summary>
            /// 

            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Generating random DateTime");
                DateTime dt1 = GetRandomDateTime();
                DateTime dt2 = new DateTime(year, month, day, hour, minute, second, millisec);
                if (!dt1.Equals(dt2))
                {
                    Log.Comment("Expected '" + dt1.ToString() + "' Equals '" + dt2.ToString() + "'");
                    testResult = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_ToStringTest6()
        {
            /// <summary>
            ///  1. Creates a DateTime
            ///  2. Verifies that it correctly returns a string from ToString
            /// </summary>
            /// 

            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Generating random DateTime");
                DateTime dt = GetRandomDateTime();
                int[] intArr = new int[] { dt.Month, dt.Day, dt.Year, dt.Hour, dt.Minute, dt.Second };
                string[] strArr = new string[] { "", "", "", "", "", "" };
                for (int i = 0; i < intArr.Length; i++)
                {
                    if (i == 2)
                    {
                        if (intArr[2] < 100)
                            strArr[2] += "00" + intArr[2];
                        else if (intArr[2] < 1000)
                            strArr[2] += "0" + intArr[2];
                        else
                            strArr[2] += intArr[2];
                    }
                    else
                    {
                        if (intArr[i] < 10)
                        {
                            strArr[i] += "0" + intArr[i];
                        }
                        else
                            strArr[i] += intArr[i];
                    }
                }
                string str = strArr[0] + "/" + strArr[1] + "/" + strArr[2] + " " + strArr[3] + ":" + strArr[4] + ":" + strArr[5];
                if (dt.ToString() != str)
                {
                    Log.Comment("Expected string '" + str + "', but got '" + dt.ToString() + "'");
                    testResult = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_ToStringTest7()
        {
            /// <summary>
            ///  1. Creates a DateTime
            ///  2. Verifies DateTime.ToString (String) returns correct String using a specified format
            /// </summary>
            /// 

            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Generating random DateTime");
                DateTime dt = GetRandomDateTime();
                Log.Comment("DateTime.ToString(String) using Standard Formats and Verifying");
                string[] standardFmts = { "d", "D", "f", "F", "g", "G", "m", "M", "o", "R", "r", "s", "t", "T", "u", "U", "Y", "y" };
                foreach (string standardFmt in standardFmts)
                {
                    try
                    {
                        if (dt.ToString(standardFmt).Length < 1)
                        {
                            Log.Comment("Expected a String length greater than '0' but got '" +
                                dt.ToString(standardFmt).Length + "'");
                            testResult = MFTestResults.Fail;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Comment("This currently fails, DateTime.ToString(String)" +
                            " throws ArgumentException for some string formats, see 22837 for details");
                        Log.Comment("Caught " + ex.Message + " when Trying DateTime.ToString(" + standardFmt + ")");
                        testResult = MFTestResults.KnownFailure;
                    }
                }
                Log.Comment("DateTime.ToString(String) using Custom Formats and Verifying");
                string[] customFmts = {"h:mm:ss.ff t", "d MMM yyyy", "HH:mm:ss.f","dd MMM HH:mm:ss", 
                  @"\Mon\t\h\: M", "MM/dd/yyyy", "dddd, dd MMMM yyyy", "MMMM dd", "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'",
                             "yyyy'-'MM'-'dd'T'HH':'mm':'ss", "HH:mm", "yyyy'-'MM'-'dd HH':'mm':'ss'Z'", "yyyy MMMM"};
                foreach (string customFmt in customFmts)
                {
                    try
                    {
                        if (dt.ToString(customFmt).Length < 1)
                        {
                            Log.Comment("Expected a String length greater than '0' but got '" +
                                dt.ToString(customFmt).Length + "'");
                            testResult = MFTestResults.Fail;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Comment("Caught " + ex.Message + " when Trying DateTime.ToString(" + customFmt + ")");
                        testResult = MFTestResults.KnownFailure;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_AddTest8()
        {
            /// <summary>
            ///  1. Creates a DateTime
            ///  2. Runs the Add function and verifies output
            /// </summary>
            ///                       

            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Generating random DateTimes ");
                DateTime[] dt1Arr = Get_ArrayOfRandomDateTimes();
                TimeSpan ts;
                Random random = new Random();
                for (int i = 0; i < dt1Arr.Length; i++)
                {
                    DateTime dt1 = dt1Arr[i];
                    if (i % 2 == 0)
                    {
                        ts = new TimeSpan(-random.Next(1000));
                    }
                    else
                    {
                        ts = new TimeSpan(random.Next(1000));
                    }
                    Log.Comment("Adding '" + ts.ToString() + "' Timespan to '" + dt1.ToString() + "'");
                    DateTime dt2 = dt1.Add(ts);
                    if (dt2.Ticks != (dt1.Ticks + ts.Ticks))
                    {
                        Log.Comment("Adding  Timespan of " + ts.ToString() + " to a DateTime of " + dt1Arr.ToString() +
                            " expected a new DateTime '" + (dt1.Ticks + ts.Ticks) + "' Ticks but got '" + dt2.Ticks + "'");
                        testResult = MFTestResults.Fail;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_AddDays_PositiveTest9()
        {
            /// <summary>
            ///  1. Creates a DateTime
            ///  2. Runs the AddDays function and verifies output
            /// </summary>
            /// 

            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Generating random DateTime");
                DateTime[] dt1Arr = Get_ArrayOfRandomDateTimes();
                Random random = new Random();
                Log.Comment("Adding Random +ve Days and verifying");
                for (int i = 0; i < dt1Arr.Length; i++)
                {
                    DateTime dt1 = dt1Arr[i];
                    double dy = random.Next(1000) * rdmFraction[random.Next(rdmFraction.Length)];
                    Log.Comment("Adding '" + dy + "' days to '" + dt1.ToString() + "'");
                    DateTime dt2 = dt1.AddDays(dy);
                    if (!CheckDeviation((long)(dt1.Ticks + (dy * TimeSpan.TicksPerDay)), dt2.Ticks))
                    {
                        Log.Comment("After Adding +ve day = '" + dy + "' to a DateTime = '" + dt1 +
                            "', expected Ticks = '" + (long)(dt1.Ticks + (dy * TimeSpan.TicksPerDay)) +
                            "' but got Ticks = '" + dt2.Ticks + "'");
                        testResult = MFTestResults.Fail;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_AddDays_NegativeTest10()
        {
            /// <summary>
            ///  1. Creates a DateTime
            ///  2. Runs the AddDays function and verifies output
            /// </summary>
            /// 

            Log.Comment("This is fixed, DateTime.AddXXXX methods do not handle negative");
            Log.Comment("values correctly on Device, see 22728 for details");
            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Generating random DateTime");
                DateTime[] dt1Arr = Get_ArrayOfRandomDateTimes();
                Random random = new Random();
                Log.Comment("Adding Random -ve Days and verifying");
                for (int i = 0; i < dt1Arr.Length; i++)
                {
                    DateTime dt1 = dt1Arr[i];
                    double dy = -random.Next(1000) * rdmFraction[random.Next(rdmFraction.Length)];
                    Log.Comment("Adding '" + dy + "' days to '" + dt1.ToString() + "'");
                    DateTime dt2 = dt1.AddDays(dy);
                    if (!CheckDeviation((long)(dt1.Ticks + (dy * TimeSpan.TicksPerDay)), dt2.Ticks))
                    {
                        Log.Comment("After Adding -ve day = '" + dy + "' to a DateTime = '" + dt1 +
                            "', expected Ticks = '" + (long)(dt1.Ticks + (dy * TimeSpan.TicksPerDay)) +
                            "' but got Ticks = '" + dt2.Ticks + "'");
                        testResult = MFTestResults.Fail;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_AddHours_PositiveTest11()
        {
            /// <summary>
            ///  1. Creates a DateTime
            ///  2. Runs the AddHours function and verifies output
            /// </summary>
            /// 

            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Generating random DateTime");
                DateTime[] dt1Arr = Get_ArrayOfRandomDateTimes();
                Random random = new Random();
                Log.Comment("Adding Random +ve Hours and verifying");
                for (int i = 0; i < dt1Arr.Length; i++)
                {
                    DateTime dt1 = dt1Arr[i];
                    double hr = random.Next(1000) * rdmFraction[random.Next(rdmFraction.Length)];
                    Log.Comment("Adding '" + hr + "' hours to '" + dt1.ToString() + "'");
                    DateTime dt2 = dt1.AddHours(hr);
                    if (!CheckDeviation((long)(dt1.Ticks + (hr * TimeSpan.TicksPerHour)), dt2.Ticks))
                    {
                        Log.Comment("After Adding +ve hour = '" + hr + "' to a DateTime = '" + dt1 +
                            "', expected Ticks = '" + (long)(dt1.Ticks + (hr * TimeSpan.TicksPerHour)) +
                            "' but got Ticks = '" + dt2.Ticks + "'");
                        testResult = MFTestResults.Fail;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_AddHours_NegativeTest12()
        {
            /// <summary>
            ///  1. Creates a DateTime
            ///  2. Runs the AddHours function and verifies output
            /// </summary>
            /// 

            Log.Comment("This is fixed, DateTime.AddXXXX methods do not handle negative");
            Log.Comment("values correctly on Device, see 22728 for details");
            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Generating random DateTime");
                DateTime[] dt1Arr = Get_ArrayOfRandomDateTimes();
                Random random = new Random();
                Log.Comment("Adding Random -ve Hours and verifying");
                for (int i = 0; i < dt1Arr.Length; i++)
                {
                    DateTime dt1 = dt1Arr[i];
                    double hr = -random.Next(1000) * rdmFraction[random.Next(rdmFraction.Length)];
                    Log.Comment("Adding '" + hr + "' hours to '" + dt1.ToString() + "'");
                    DateTime dt2 = dt1.AddHours(hr);
                    if (!CheckDeviation((long)(dt1.Ticks + (hr * TimeSpan.TicksPerHour)), dt2.Ticks))
                    {
                        Log.Comment("After Adding -ve hour = '" + hr + "' to a DateTime = '" + dt1 +
                            "', expected Ticks = '" + (long)(dt1.Ticks + (hr * TimeSpan.TicksPerHour)) +
                            "' but got Ticks = '" + dt2.Ticks + "'");
                        testResult = MFTestResults.Fail;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_AddMilliseconds_PositiveTest13()
        {
            /// <summary>
            ///  1. Creates a DateTime
            ///  2. Runs the AddMilliseconds function and verifies output
            /// </summary>
            /// 

            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Generating random DateTime");
                DateTime[] dt1Arr = Get_ArrayOfRandomDateTimes();
                Random random = new Random();
                Log.Comment("Adding Random +ve Milliseconds and verifying");
                for (int i = 0; i < dt1Arr.Length; i++)
                {
                    DateTime dt1 = dt1Arr[i];
                    double msec = random.Next(1000) * rdmFraction[random.Next(rdmFraction.Length)];
                    Log.Comment("Adding '" + msec + "' milliseconds to '" + dt1.ToString() + "'");
                    DateTime dt2 = dt1.AddMilliseconds(msec);
                    if (!CheckDeviation((long)(dt1.Ticks + (msec * TimeSpan.TicksPerMillisecond)), dt2.Ticks))
                    {
                        Log.Comment("After Adding +ve milliseconds = '" + msec + "' to a DateTime = '" + dt1 +
                            "', expected Ticks = '" + (long)(dt1.Ticks + (msec * TimeSpan.TicksPerMillisecond)) +
                            "' but got Ticks = '" + dt2.Ticks + "'");
                        testResult = MFTestResults.Fail;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_AddMilliseconds_NegativeTest14()
        {
            /// <summary>
            ///  1. Creates a DateTime
            ///  2. Runs the AddMilliseconds function and verifies output
            /// </summary>
            /// 

            Log.Comment("This is fixed, DateTime.AddXXXX methods do not handle negative");
            Log.Comment("values correctly on Device, see 22728 for details");
            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Generating random DateTime");
                DateTime[] dt1Arr = Get_ArrayOfRandomDateTimes();
                Random random = new Random();
                Log.Comment("Adding Random -ve Milliseconds and verifying");
                for (int i = 0; i < dt1Arr.Length; i++)
                {
                    DateTime dt1 = dt1Arr[i];
                    double msec = -random.Next(1000) * rdmFraction[random.Next(rdmFraction.Length)];
                    Log.Comment("Adding '" + msec + "' milliseconds to '" + dt1.ToString() + "'");
                    DateTime dt2 = dt1.AddMilliseconds(msec);
                    if (!CheckDeviation((long)(dt1.Ticks + (msec * TimeSpan.TicksPerMillisecond)), dt2.Ticks))
                    {
                        Log.Comment("After Adding -ve milliseconds = '" + msec + "' to a DateTime = '" + dt1 +
                            "', expected Ticks = '" + (long)(dt1.Ticks + (msec * TimeSpan.TicksPerMillisecond)) +
                            "' but got Ticks = '" + dt2.Ticks + "'");
                        testResult = MFTestResults.Fail;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_AddMinutes_PositiveTest15()
        {
            /// <summary>
            ///  1. Creates a DateTime
            ///  2. Runs the AddMinutes function and verifies output
            /// </summary>
            /// 

            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Generating random DateTime");
                DateTime[] dt1Arr = Get_ArrayOfRandomDateTimes();
                Random random = new Random();
                Log.Comment("Adding Random +ve Minutes and verifying");
                for (int i = 0; i < dt1Arr.Length; i++)
                {
                    DateTime dt1 = dt1Arr[i];
                    double mnts = random.Next(1000) * rdmFraction[random.Next(rdmFraction.Length)];
                    Log.Comment("Adding '" + mnts + "' minutes to '" + dt1.ToString() + "'");
                    DateTime dt2 = dt1.AddMinutes(mnts);
                    if (!CheckDeviation((long)(dt1.Ticks + (mnts * TimeSpan.TicksPerMinute)), dt2.Ticks))
                    {
                        Log.Comment("After Adding +ve minute = '" + mnts + "' to a DateTime = '" + dt1 +
                            "', expected Ticks = '" + (long)(dt1.Ticks + (mnts * TimeSpan.TicksPerMinute)) +
                            "' but got Ticks = '" + dt2.Ticks + "'");
                        testResult = MFTestResults.Fail;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_AddMinutes_NegativeTest16()
        {
            /// <summary>
            ///  1. Creates a DateTime
            ///  2. Runs the AddMinutes function and verifies output
            /// </summary>
            /// 

            Log.Comment("This is fixed, DateTime.AddXXXX methods do not handle negative");
            Log.Comment("values correctly on Device, see 22728 for details");
            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Generating random DateTime");
                DateTime[] dt1Arr = Get_ArrayOfRandomDateTimes();
                Random random = new Random();
                Log.Comment("Adding Random -ve Minutes and verifying");
                for (int i = 0; i < dt1Arr.Length; i++)
                {
                    DateTime dt1 = dt1Arr[i];
                    double mnts = -random.Next(1000) * rdmFraction[random.Next(rdmFraction.Length)];
                    Log.Comment("Adding '" + mnts + "' minutes to '" + dt1.ToString() + "'");
                    DateTime dt2 = dt1.AddMinutes(mnts);
                    if (!CheckDeviation((long)(dt1.Ticks + (mnts * TimeSpan.TicksPerMinute)), dt2.Ticks))
                    {
                        Log.Comment("After Adding -ve minute = '" + mnts + "' to a DateTime = '" + dt1 +
                            "', expected Ticks = '" + (long)(dt1.Ticks + mnts * TimeSpan.TicksPerMinute) +
                            "' but got Ticks = '" + dt2.Ticks + "'");
                        testResult = MFTestResults.Fail;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_AddMonths_Test17()
        {
            /// <summary>
            ///  1. NYI: Returns None
            /// </summary>
            ///
            Log.Comment("DateTime::AddMonths - Normal ");
            Log.Comment("Not implemented yet.");

            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults DateTime_AddSeconds_PositiveTest18()
        {
            /// <summary>
            ///  1. Creates a DateTime
            ///  2. Runs the AddSeconds function and verifies output
            /// </summary>
            /// 

            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Generating random DateTime");
                DateTime[] dt1Arr = Get_ArrayOfRandomDateTimes();
                Random random = new Random();
                Log.Comment("Adding Random +ve Seconds and verifying");
                for (int i = 0; i < dt1Arr.Length; i++)
                {
                    DateTime dt1 = dt1Arr[i];
                    double sec = random.Next(1000) * rdmFraction[random.Next(rdmFraction.Length)];
                    Log.Comment("Adding '" + sec + "' seconds to '" + dt1.ToString() + "'");
                    DateTime dt2 = dt1.AddSeconds(sec);
                    if (!CheckDeviation((long)(dt1.Ticks + (sec * TimeSpan.TicksPerSecond)), dt2.Ticks))
                    {
                        Log.Comment("After Adding +ve seconds = '" + sec + "' to a DateTime = '" + dt1 +
                            "', expected Ticks = '" + (long)(dt1.Ticks + (sec * TimeSpan.TicksPerSecond)) +
                            "' but got Ticks = '" + dt2.Ticks + "'");
                        testResult = MFTestResults.Fail;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_AddSeconds_NegativeTest19()
        {
            /// <summary>
            ///  1. Creates a DateTime
            ///  2. Runs the AddSeconds function and verifies output
            /// </summary>
            /// 

            Log.Comment("This is fixed, DateTime.AddXXXX methods do not handle negative");
            Log.Comment("values correctly on Device, see 22728 for details");
            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Generating random DateTime");
                DateTime[] dt1Arr = Get_ArrayOfRandomDateTimes();
                Random random = new Random();
                Log.Comment("Adding Random -ve Seconds and verifying");
                for (int i = 0; i < dt1Arr.Length; i++)
                {
                    DateTime dt1 = dt1Arr[i];
                    double sec = -random.Next(1000) * rdmFraction[random.Next(rdmFraction.Length)];
                    Log.Comment("Adding '" + sec + "' seconds to '" + dt1.ToString() + "'");
                    DateTime dt2 = dt1.AddSeconds(sec);
                    if (!CheckDeviation((long)(dt1.Ticks + (sec * TimeSpan.TicksPerSecond)), dt2.Ticks))
                    {
                        Log.Comment("After Adding -ve seconds = '" + sec + "' to a DateTime = '" + dt1 +
                            "', expected Ticks = '" + (long)(dt1.Ticks + sec * TimeSpan.TicksPerSecond) +
                            "' but got Ticks = '" + dt2.Ticks + "'");
                        testResult = MFTestResults.Fail;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_AddTicks_PositiveTest20()
        {
            /// <summary>
            ///  1. Creates a DateTime
            ///  2. Runs the AddTicks function and verifies output
            /// </summary>
            /// 

            MFTestResults testResult = MFTestResults.Pass;
            Log.Comment("Generating random DateTime");
            try
            {
                DateTime[] dt1Arr = Get_ArrayOfRandomDateTimes();
                Random random = new Random();
                Log.Comment("Adding Random +ve Ticks and verifying");
                for (int i = 0; i < dt1Arr.Length; i++)
                {
                    DateTime dt1 = dt1Arr[i];
                    long ticks = (long)random.Next(1000);
                    Log.Comment("Adding '" + ticks + "' ticks to '" + dt1.ToString() + "'");
                    DateTime dt2 = dt1.AddTicks(ticks);
                    if (dt2.Ticks != (dt1.Ticks + ticks))
                    {
                        Log.Comment("After Adding +ve ticks = '" + ticks + "' to a DateTime = '" + dt1 + "', expected Ticks = '" +
                            (dt1.Ticks + ticks) + "' but got Ticks = '" + dt2.Ticks + "'");
                        testResult = MFTestResults.Fail;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_AddTicks_NegativeTest21()
        {
            /// <summary>
            ///  1. Creates a DateTime
            ///  2. Runs the AddTicks function and verifies output
            /// </summary>
            /// 

            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Generating random DateTime");
                DateTime[] dt1Arr = Get_ArrayOfRandomDateTimes();
                Random random = new Random();
                Log.Comment("Adding Random -ve Ticks and verifying");
                for (int i = 0; i < 10; i++)
                {
                    DateTime dt1 = dt1Arr[i];
                    long ticks = -(long)random.Next(1000);
                    Log.Comment("Adding '" + ticks + "' ticks to '" + dt1.ToString() + "'");
                    DateTime dt2 = dt1.AddTicks(ticks);
                    if (dt2.Ticks != (dt1.Ticks + ticks))
                    {
                        Log.Comment("After Adding -ve ticks = '" + ticks + "' to a DateTime = '" + dt1 + "', expected Ticks = '" +
                            (dt1.Ticks + ticks) + "' but got Ticks = '" + dt2.Ticks + "'");
                        testResult = MFTestResults.Fail;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_AddYears_Test22()
        {
            /// <summary>
            ///  1. NYI: Returns None
            /// </summary>
            ///
            Log.Comment("DateTime::AddYears - Normal ");
            Log.Comment("Not implemented yet.");

            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults DateTime_Compare_Test23()
        {
            /// <summary>
            ///  1. Creates two DateTimes
            ///  2. Verifies that they Compare with each other
            /// </summary>
            /// 

            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Creating two Random but equal DateTime b/n 1000 - 9000");
                Log.Comment("Comparing eachother with DateTime.Compare and Verifying");
                DateTime dt1 = DateTime_btwn_1801_And_2801();
                DateTime dt2 = new DateTime(year, month, day, hour, minute, second, millisec);
                Log.Comment("Comparing equal DateTimes and Verifying");
                if (DateTime.Compare(dt1, dt2) != 0)
                {
                    Log.Comment("Expected DateTime.Compare(" + dt1.ToString() + ", " + dt2.ToString() +
                        ") returns '0' but got '" + DateTime.Compare(dt1, dt2) + "'");
                    testResult = MFTestResults.Fail;
                }
                Log.Comment("Comparing Unequal DateTimes and Verifying");
                dt2 = dt1.Add(new TimeSpan(1));
                if (DateTime.Compare(dt1, dt2) >= 0)
                {
                    Log.Comment("Expected DateTime.Compare(" + dt1.ToString() + ", " + dt2.ToString() +
                        ") returns '-ve' value but got '" + DateTime.Compare(dt1, dt2) + "'");
                    testResult = MFTestResults.Fail;
                }
                if (DateTime.Compare(dt2, dt1) <= 0)
                {
                    Log.Comment("Expected DateTime.CompareTo(" + dt1.ToString() + ", " + dt2.ToString() +
                        ") returns '+ve' value but got '" + DateTime.Compare(dt1, dt2) + "'");
                    testResult = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }


        [TestMethod]
        public MFTestResults DateTime_DaysInMonth_Test24()
        {
            /// <summary>
            ///  1. Verifies the accuracy of the DaysInMonth method
            /// </summary>
            /// 

            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Generates a random year and month, and");
                Random random = new Random();
                Log.Comment("Verifies the number of days in the specific month/year");
                for (int i = 0; i < 100; i++)
                {
                    int yr = random.Next(9999) + 1;
                    int mnth = random.Next(12) + 1;
                    if (DaysInMonthTest(yr, mnth) != MFTestResults.Pass)
                    {
                        Log.Comment("Failure : verifying no. of days in mm/yr = '" + mnth + "/" + yr + "'");
                        testResult = MFTestResults.Fail;
                    }
                }
                Log.Comment("Verifying no. of days in Feb, for 20th and 21st centuries");
                for (int yr = 1900; yr < 2100; yr += 4)
                {
                    if (DaysInMonthTest(yr, 2) != MFTestResults.Pass)
                    {
                        Log.Comment("Failure : verifying no. of days in mm/yr = '02/" + yr + "'");
                        testResult = MFTestResults.Fail;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_EqualsTest25()
        {
            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Creating random-equal DateTimes");
                Log.Comment("And Verifying they are equal");
                DateTime dt1 = DateTime_btwn_1801_And_2801();
                DateTime dt2 = new DateTime(year, month, day, hour, minute, second, millisec);
                if (!DateTime.Equals(dt1, dt2))
                {
                    Log.Comment("DateTime.Equals('" + dt1.ToString() + "'" + ",'" +
                        dt2.ToString() + "'' returned false");
                    testResult = MFTestResults.Fail;
                }
                object obj1 = (object)dt1, obj2 = (object)dt2;
                try
                {
                    if (!object.Equals(obj1, obj2))
                    {
                        Log.Comment("DateTime.Equals('(object)" + dt1.ToString() + "'" +
                            ",'(object)" + dt2.ToString() + "'' returned false");
                        testResult = MFTestResults.Fail;
                    }
                }

                catch (Exception) { };
                if (!dt1.Equals(obj2))
                {
                    Log.Comment("'" + dt1.ToString() + "'.Equals('" +
                        dt2.ToString() + "'' returned false");
                    testResult = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_Subtract_DateTimeTest26()
        {
            Log.Comment("Creating two Random DateTimes,");
            Log.Comment("dt1.Subtract(dt2) and verifying");
            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                DateTime dt1 = GetRandomDateTime();
                DateTime dt2 = GetRandomDateTime();
                TimeSpan ts1 = dt1.Subtract(dt2);
                TimeSpan ts2 = new TimeSpan(dt1.Ticks - dt2.Ticks);

                if (ts1 != ts2)
                {
                    Log.Comment("Failure : dt1.Subtract(dt2), expected result '" + ts2.ToString() +
                        "' but got '" + ts1.ToString() + "'");
                    testResult = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_Subtract_TimeSpanTest27()
        {
            Log.Comment("Creating now DateTime");
            Log.Comment("Subtracting random timespans and ");
            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                DateTime[] dtArr = Get_ArrayOfRandomDateTimes();
                Random random = new Random();
                TimeSpan ts;
                for (int i = 0; i < dtArr.Length; i++)
                {
                    DateTime dt1 = dtArr[i];
                    if (i % 2 == 0)
                    {
                        ts = new TimeSpan(-random.Next(1000));
                    }
                    else
                    {
                        ts = -new TimeSpan(random.Next(1000));
                    }

                    Log.Comment(dt1.ToString());
                    Log.Comment(ts.ToString());
                    DateTime dt2 = dt1.Subtract(ts);
                    Log.Comment(dt2.ToString());
                    DateTime dt3 = new DateTime(dt1.Ticks - ts.Ticks);
                    Log.Comment(dt3.ToString());
                    if (DateTime.Compare(dt2, dt3) != 0)
                    {
                        Log.Comment("Failure : subtracting '" + ts.ToString() + "' from a DateTime = '" + dt1.ToString() +
                            "', expected DateTime = '" + dt3.ToString() + "' but got '" + dt2.ToString() + "'");
                        testResult = MFTestResults.Fail;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.ToString());
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_ToLocalTimeTest28()
        {
            /// <summary>
            ///  1. Calls DateTime.ToLocalTime
            ///  2. Verifies the output
            /// </summary>
            ///21293	ToLocalTime Fails with incorrect offset

            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Creating Local Now DateTime");
                System.DateTime test0 = new System.DateTime(DateTime.Now.Ticks);
                Log.Comment("Calling ToLocalTime and verifying");
                System.DateTime testDateTime = test0.ToLocalTime();

                // 1. Time is still the same:
                if (testDateTime != test0)
                {
                    Log.Comment("Failure: it's already local DateTime hence the 'Kind' should be same");
                    testResult = MFTestResults.Fail;
                }

                if (testDateTime.Ticks != test0.Ticks)
                {
                    Log.Comment("Failure : ticks not same");
                    testResult = MFTestResults.Fail;
                }
                // 3. Both times are local.
                if ((testDateTime.Kind != DateTimeKind.Local) || (test0.Kind != DateTimeKind.Local))
                {
                    Log.Comment("Failure : expected both DateTimeKind = '" + DateTimeKind.Local +
                        "' but got new(DateTime.Now) = '" + testDateTime.Kind.ToString() +
                        "' and new(DateTime.Now).ToLocalTime()'" + test0.Kind.ToString() + "'");
                    testResult = MFTestResults.Fail;
                }
                Log.Comment("Setting to UTC time");
                test0 = DateTime.SpecifyKind(test0, DateTimeKind.Utc);
                Log.Comment("Converting now UTC time to local and verifying");
                testDateTime = test0.ToLocalTime();

                Log.Comment("Verifying Time now different");
                if (testDateTime == test0 && TimeZone.CurrentTimeZone.GetUtcOffset(testDateTime).Ticks != 0)
                {
                    Log.Comment("Failure : expected different time but got equal '" + test0 + "'");

                    testResult = MFTestResults.Fail;
                }
                Log.Comment("Verifying change in tick due to conversion since we are not in zero time zone");
                long diff1 = testDateTime.Ticks - test0.Ticks;
                long diff2 = TimeZone.CurrentTimeZone.GetUtcOffset(testDateTime).Ticks;
                long diff3 = TimeZone.CurrentTimeZone.GetUtcOffset(test0).Ticks;
                if ((diff1 != diff2) || (0 != diff3))
                {
                    Log.Comment("Failure : expected timezone difference in ticks = '" +
                        diff2.ToString() + "(" + diff3.ToString() + ")' but got '" + diff1.ToString() + "'");
                    testResult = MFTestResults.Fail;
                }
                Log.Comment("Verifying converted DateTime is UTC, local DateTime is local");
                if ((testDateTime.Kind != DateTimeKind.Local) || (test0.Kind != DateTimeKind.Utc))
                {
                    Log.Comment("expected converted DateTime kind '" + DateTimeKind.Utc + "' but got '" + testDateTime.Kind + "'");
                    Log.Comment("expected local DateTime kind '" + DateTimeKind.Local + "' but got '" + test0.Kind + "'");
                    testResult = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }


            return testResult;
        }
        [TestMethod]
        public MFTestResults DateTime_ToUniversalTimeTest29()
        {
            /// <summary>
            ///  1. Calls DateTime.ToUniversalTime
            ///  2. Verifies the output
            /// </summary>
            ///21294	ToUniversalTime fails with incorrect time.

            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Creating Now DateTime");
                DateTime test0 = new DateTime(DateTime.Now.Ticks);
                Log.Comment("creating another DateTime by calling ToUniversalTime");
                DateTime testDateTime = test0.ToUniversalTime();

                if (testDateTime == test0 && TimeZone.CurrentTimeZone.GetUtcOffset(test0).Ticks != 0)
                {
                    Log.Comment("Failure : expected Time now different because we compare times in different zone");
                    testResult = MFTestResults.Fail;
                }
                Log.Comment("Tick do change due to conversion since we are not in zero time zone");
                long diff1 = test0.Ticks - testDateTime.Ticks;
                long diff2 = TimeZone.CurrentTimeZone.GetUtcOffset(testDateTime).Ticks;
                long diff3 = TimeZone.CurrentTimeZone.GetUtcOffset(test0).Ticks;
                if ((diff1 != diff3) || (diff2 != 0))
                {
                    Log.Comment("Failure : expected timezone difference in ticks = '" +
                       diff2.ToString() + "(" + diff3.ToString() + ")' but got '" + diff1.ToString() + "'");
                    testResult = MFTestResults.Fail;
                }
                Log.Comment("Verifying DateTime Kind's to be local,to be UTC");
                if ((testDateTime.Kind != DateTimeKind.Utc) || (test0.Kind != DateTimeKind.Local))
                {
                    Log.Comment("expected converted DateTime kind '" + DateTimeKind.Utc + "' but got '" + testDateTime.Kind + "'");
                    Log.Comment("expected local DateTime kind '" + DateTimeKind.Local + "' but got '" + test0.Kind + "'");
                    testResult = MFTestResults.Fail;
                }

                Log.Comment("Setting new(DateTime.Now) to UTC time and verifying");
                test0 = DateTime.SpecifyKind(test0, DateTimeKind.Utc);
                Log.Comment("Converting now UTC time to UTC - no change.");
                testDateTime = test0.ToUniversalTime();

                if (testDateTime != test0)
                {
                    Log.Comment("Failure : expected '" + testDateTime.ToString() + "' but got '" + test0.ToString() + "'");
                    testResult = MFTestResults.Fail;
                }
                Log.Comment("Comparing ticks and verifying");
                if (testDateTime.Ticks != test0.Ticks)
                {
                    Log.Comment("Failure : expected ticks '" + testDateTime.Ticks + "' but got '" + test0.Ticks + "'");
                    testResult = MFTestResults.Fail;
                }
                Log.Comment("Verifying Both times are Utc");
                if ((testDateTime.Kind != DateTimeKind.Utc) || (test0.Kind != DateTimeKind.Utc))
                {
                    Log.Comment("expected converted DateTime kind '" + DateTimeKind.Utc + "' but got '" + testDateTime.Kind + "'");
                    Log.Comment("expected UTC setted DateTime kind '" + DateTimeKind.Utc + "' but got '" + test0.Kind + "'");
                    testResult = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_op_AdditionTest30()
        {
            Log.Comment("Creating Random DateTimes,");
            Log.Comment("Adds a specified period of time and verifying");
            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                DateTime[] dtArr = Get_ArrayOfRandomDateTimes();
                Random random = new Random();
                TimeSpan ts;
                for (int i = 0; i < dtArr.Length; i++)
                {
                    DateTime dt1 = dtArr[i];
                    if (i % 2 == 0)
                    {
                        ts = new TimeSpan(-random.Next(1000));
                    }
                    else
                    {
                        ts = new TimeSpan(random.Next(1000));
                    }
                    DateTime dt2 = dt1 + ts;
                    DateTime dt3 = new DateTime(dt1.Ticks + ts.Ticks);
                    if (DateTime.Compare(dt2, dt3) != 0)
                    {
                        Log.Comment("Failure : DateTime'" + dt1.ToString() + "' + TimeSpan'" + ts.ToString() +
                            "' expected result '" + dt3.ToString() + "' but got '" + dt2.ToString() + "'");
                        testResult = MFTestResults.Fail;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_op_Subtraction_DateTimeTest31()
        {
            Log.Comment("Creating Random DateTimes,");
            Log.Comment("Subtracting one from the other and verifying");
            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                DateTime[] dtArr = Get_ArrayOfRandomDateTimes();
                Random random = new Random();
                for (int i = 0; i < dtArr.Length; i++)
                {
                    DateTime dt1 = dtArr[i];
                    DateTime dt2 = new DateTime(random.Next(1000) + 1);
                    TimeSpan ts = dt1 - dt2;
                    if (ts.Ticks != (dt1.Ticks - dt2.Ticks))
                    {
                        Log.Comment("Failure : '" + dt1.ToString() + "' - '" + dt2.ToString() +
                            "', expected difference = '" + (dt1.Ticks - dt2.Ticks) + "' ticks but got '" + ts.ToString() + "' ticks");
                        testResult = MFTestResults.Fail;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_op_Subtraction_TimeSpanTest32()
        {
            Log.Comment("Creating Random DateTime,");
            Log.Comment("Subtracting random TimeSpan and verifying");
            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                DateTime[] dtArr = Get_ArrayOfRandomDateTimes();
                Random random = new Random();
                for (int i = 0; i < dtArr.Length; i++)
                {
                    DateTime dt1 = dtArr[i];
                    TimeSpan ts = new TimeSpan(random.Next(10000));
                    DateTime dt2 = dt1 - ts;

                    if (dt2.Ticks != (dt1.Ticks - ts.Ticks))
                    {
                        Log.Comment("Failure : DateTime('" + dt1.ToString() + ")' - TimeSpan'" + ts.ToString() +
                            "', expected difference = '" + (dt1.Ticks - ts.Ticks) + "' ticks but got '" + dt2.ToString() + "' ticks");
                        testResult = MFTestResults.Fail;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_op_EqualityTest33()
        {
            Log.Comment("Creating Random DateTime,");
            Log.Comment("Creating another DateTime equal to previous one");
            Log.Comment("Verifying the two DateTimes are equal using '=='");
            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                DateTime dt1 = GetRandomDateTime();
                DateTime dt2 = new DateTime(year, month, day, hour, minute, second, millisec);

                if (!(dt1 == dt2))
                {
                    Log.Comment("Failure : expected the two DateTimes '" + dt1.ToString() +
                        "'  and '" + dt2.ToString() + "' to be equal but are not");
                    testResult = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_op_InequalityTest34()
        {
            Log.Comment("Creating Random DateTime,");
            Log.Comment("Creating another Different DateTime");
            Log.Comment("Verifying the two DateTimes are not equal using '!='");
            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                DateTime dt1 = GetRandomDateTime();
                DateTime dt2 = new DateTime(year + 1, month, day, hour, minute, second, millisec);

                if (!(dt1 != dt2))
                {
                    Log.Comment("Failure : expected the two DateTimes '" + dt1.ToString() +
                        "'  and '" + dt2.ToString() + "' not to be equal but are equal");
                    testResult = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_op_LessThanTest35()
        {
            Log.Comment("Creating Random DateTime,");
            Log.Comment("Creating another Different DateTime greater than previous one");
            Log.Comment("Verifying 1st DateTime is less than 2nd using '<'");
            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                DateTime dt1 = GetRandomDateTime();
                DateTime dt2 = new DateTime(year + 1, month, day, hour, minute, second, millisec);

                if (!(dt1 < dt2))
                {
                    Log.Comment("Failure : expected  DateTime '" + dt1.ToString() +
                        "'  less than '" + dt2.ToString() + "'");
                    testResult = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_op_LessThanOrEqualTest36()
        {
            Log.Comment("Creating Random DateTime, Creaing 2nd equal DateTime");
            Log.Comment("Creating 3rd Different DateTime greater than previous two");
            Log.Comment("Verifying 1st DateTime is less than or equal to 2nd DateTime using '<='");
            Log.Comment("Verifying 1st DateTime is less than or equal to 3rd DateTime using '<='");
            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                DateTime dt1 = GetRandomDateTime();
                DateTime dt2 = new DateTime(year, month, day, hour, minute, second, millisec);
                DateTime dt3 = new DateTime(year + 1, month, day, hour, minute, second, millisec);
                if (!((dt1 <= dt2) || (dt2 <= dt1)))
                {
                    Log.Comment("Failure : expected  DateTime '" + dt1.ToString() + "'  lessthan or equal to '" +
                        dt2.ToString() + "' (dt1 <= dt2) = '" + (dt1 <= dt2) + "' and (dt2<=dt1) = '" + (dt2 <= dt1) + "'");
                    testResult = MFTestResults.Fail;
                }
                if (!(dt1 <= dt3))
                {
                    Log.Comment("Failure : expected  DateTime '" + dt1.ToString() +
                        "'  less than or equal to '" + dt2.ToString() + "'");
                    testResult = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_op_GreaterThanTest37()
        {
            Log.Comment("Creating Random DateTime,");
            Log.Comment("Creating another Different DateTime greater than previous one");
            Log.Comment("Verifying 2nd DateTime is greater than 1st using '>'");
            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                DateTime dt1 = GetRandomDateTime();
                DateTime dt2 = new DateTime(year + 1, month, day, hour, minute, second, millisec);

                if (!(dt2 > dt1))
                {
                    Log.Comment("Failure : expected  DateTime '" + dt1.ToString() +
                        "'  greater than '" + dt2.ToString() + "'");
                    testResult = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_op_GreaterThanOrEqualTest38()
        {
            Log.Comment("Creating Random DateTime, Creaing 2nd equal DateTime");
            Log.Comment("Creating 3rd Different DateTime greater than previous two");
            Log.Comment("Verifying 1st DateTime is greater than or equal to 2nd DateTime using '>='");
            Log.Comment("Verifying 3rd DateTime is greater than or equal to 1st DateTime using '>='");
            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                DateTime dt1 = GetRandomDateTime();
                DateTime dt2 = new DateTime(year, month, day, hour, minute, second, millisec);
                DateTime dt3 = new DateTime(year + 1, month, day, hour, minute, second, millisec);
                if (!((dt1 >= dt2) || (dt2 >= dt1)))
                {
                    Log.Comment("Failure : expected  DateTime '" + dt1.ToString() +
                        "'  lessthan or equal to '" + dt2.ToString() + "'");
                    testResult = MFTestResults.Fail;
                }
                if (!(dt1 <= dt3))
                {
                    Log.Comment("Failure : expected  DateTime '" + dt1.ToString() +
                        "'  greater than or equal to '" + dt2.ToString() + "'");
                    testResult = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_MinValueTest39()
        {
            /// <summary>
            ///  1. Verifies the MinValue property
            /// </summary>
            ///
            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Getting the Min. DateTime and Verifying");
                DateTime field = DateTime.MinValue;
                if (field.Ticks != 0)
                {
                    Log.Comment("Failure : expected DateTime.MinValue = '0' but got '" + field.Ticks + "'");
                    testResult = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_MaxValueTest40()
        {
            /// <summary>
            ///  1. Verifies the MinValue property
            /// </summary>
            ///
            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Getting the Max. DateTime and Verifying");
                DateTime field = DateTime.MaxValue;
                Log.Comment(field.Ticks.ToString());
                if (field.Ticks != 441796895990000000)
                {
                    Log.Comment("Failure : expected DateTime.MinValue = '441796895990000000'" +
                        " but got '" + field.Ticks + "'");
                    testResult = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }
            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_DateTest41()
        {
            /// <summary>
            ///  1. Verifies the Date property
            /// </summary>
            ///
            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Creating a DateTime, getting the Date and Verifying");
                DateTime dt = GetRandomDateTime();
                DateTime _date = dt.Date;
                if ((_date.Year != dt.Year) || (_date.Month != dt.Month) || (_date.Day != dt.Day) ||
                    (_date.Hour != 0) || (_date.Minute != 0) | (_date.Second != 0) || (_date.Millisecond != 0))
                {
                    Log.Comment("Failure : expected Date(mm/dd/yr/hr/mn/sec/msec) = '" + dt.Month + "/" + dt.Day +
                        "/" + dt.Year + "/0:0:0:0' but got '" + _date.Month + "/" + _date.Day + "/" +
                        _date.Year + "/" + _date.Hour + ":" + _date.Minute + ":" + _date.Second + ":" + _date.Millisecond + "'");
                    testResult = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_DayTest42()
        {
            /// <summary>
            ///  1. Verifies the Day property
            /// </summary>
            ///
            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Creating a DateTime, getting the Day and Verifying");
                DateTime testDateTime = GetRandomDateTime();
                Int32 _day = testDateTime.Day;
                if (_day != day)
                {
                    Log.Comment("Failure : Expected Day = '" + day + "' but got '" + _day + "'");
                    testResult = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_DayOfWeekTest43()
        {
            /// <summary>
            ///  1. Verifies the DayOfWeek property
            /// </summary>
            ///
            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Creating a DateTime, getting the DayOfWeek and Verifying");
                DateTime testDateTime = new DateTime(2005, 1, 28);
                DayOfWeek prop = testDateTime.DayOfWeek;
                if (prop != DayOfWeek.Friday)
                {
                    Log.Comment("Failure : Expected DayOfWeek = '" + DayOfWeek.Friday + "' but got '" + prop + "'");
                    testResult = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_DayOfYearTest44()
        {
            /// <summary>
            ///  1. Verifies the DayOfYear property
            /// </summary>
            ///
            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Creating a DateTime, getting the DayOfYear and Verifying");
                Log.Comment("DateTime::DayOfYear - Normal ");
                DateTime testDateTime = new DateTime(2005, 1, 1);
                int _dayOfYear = testDateTime.DayOfYear;
                if (_dayOfYear != 1)
                {
                    Log.Comment("Failure : Expected DayOfYear '1' but got '" + _dayOfYear + "'");
                    testResult = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_HourTest45()
        {
            /// <summary>
            ///  1. Verifies the Hour property
            /// </summary>
            ///
            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Creating a DateTime, getting the Hour and Verifying");
                DateTime testDateTime = GetRandomDateTime();
                Int32 _hour = testDateTime.Hour;
                if (_hour != hour)
                {
                    Log.Comment("Failure : Expected hour '" + hour + "' but got '" + _hour + "'");
                    testResult = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_MillisecondTest46()
        {
            /// <summary>
            ///  1. Verifies the Millisecond property
            /// </summary>
            ///
            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Creating a DateTime, getting the Milliseconds and Verifying");
                DateTime testDateTime = GetRandomDateTime();
                Int32 _mSec = testDateTime.Millisecond;
                if (_mSec != millisec)
                {
                    Log.Comment("Failure : Expected millisecond '" + millisec + "' but got '" + _mSec + "'");
                    testResult = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_MinuteTest47()
        {
            /// <summary>
            ///  1. Verifies the Minute property
            /// </summary>
            ///
            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Creating a DateTime, getting the Minute and Verifying");
                DateTime testDateTime = GetRandomDateTime();
                Int32 _minute = testDateTime.Minute;
                if (_minute != minute)
                {
                    Log.Comment("Failure : Expected minute '" + minute + "' but got '" + _minute + "'");
                    testResult = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_MonthTest48()
        {
            /// <summary>
            ///  1. Verifies the Month property
            /// </summary>
            ///
            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Creating a DateTime, getting the Month and Verifying");
                DateTime testDateTime = GetRandomDateTime();
                Int32 _month = testDateTime.Month;
                if (_month != month)
                {
                    Log.Comment("Failure : Expected month '" + month + "' but got '" + _month + "'");
                    testResult = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_NowTest49()
        {
            /// <summary>
            /// 1. Creates 2 DateTimes
            /// 2. Verifies they are equal in all but Seconds and Millisecond
            /// </summary>
            ///
            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Creating 2 DateTimes and verifying they are equal in yy/mm/dd/hr/mn");
                DateTime test0 = DateTime.Now;
                DateTime test1 = DateTime.Now;
                Log.Comment("Verifying");
                if ((test0.Year != test1.Year) || (test0.Month != test1.Month) ||
                    (test0.Day != test1.Day) || (test0.Hour != test1.Hour) ||
                    (test0.Minute != test1.Minute))
                {
                    Log.Comment("Failure : Expected the two DateTimes ('" + test0.ToString() +
                        "' and '" + test1.ToString() + "') are equal in all but seconds and milliseconds");
                    testResult = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_UtcNowTest50()
        {
            /// <summary>
            /// 1. Creates 2 DateTimes, 1 Now and 1 UtcNow
            /// 2. Calls ToUniversalTime on the Now
            /// 2. Verifies the UtcNow property is equal in all but Seconds and Millisecond
            /// </summary>
            ///
            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Creating 2 DateTimes, 1 Now and 1 UtcNow");
                DateTime test0 = DateTime.UtcNow;
                DateTime test1 = DateTime.Now;
                Log.Comment("Calling ToUniversalTime on the Now");
                test1 = test1.ToUniversalTime();
                Log.Comment("Verifying both are equal in all but seconds and Milliseconds");
                if ((test0.Year != test1.Year) || (test0.Month != test1.Month) ||
                    (test0.Day != test1.Day) || (test0.Hour != test1.Hour) ||
                    (test0.Minute != test1.Minute))
                {
                    Log.Comment("Failure : Expected the two DateTimes ('" + test0.ToString() +
                       "' and '" + test1.ToString() + "') are equal in all but seconds and milliseconds");
                    testResult = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_SecondTest51()
        {
            /// <summary>
            ///  1. Verifies the Second property
            /// </summary>
            ///
            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Creating a DateTime, getting the Second and Verifying");
                DateTime testDateTime = GetRandomDateTime();
                Int32 _sec = testDateTime.Second;
                if (_sec != second)
                {
                    Log.Comment("Failure : Expected second '" + second + "' but got '" + _sec + "'");
                    testResult = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_TicksTest52()
        {
            /// <summary>
            ///  1. Verifies the Ticks property
            /// </summary>
            ///
            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Creating a DateTime, getting the Ticks and Verifying");
                DateTime testDateTime = new System.DateTime(0);
                long _ticks = testDateTime.Ticks;
                if (_ticks != 0)
                {
                    Log.Comment("Failure : Expected ticks '0' but got '" + _ticks + "'");
                    testResult = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_TodayTest53()
        {
            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("This currently fails on device, returns 01/01/2004 on device");
                Log.Comment("please see 23092 for details");
                Log.Comment("Creating a DateTime.Today, getting the year and Verifying");
                DateTime testDateTime = DateTime.Today;
                long _year = testDateTime.Year;
                if (_year != 2009)
                {
                    Log.Comment("Failure : Expected year '2009' but got '" + _year + "'");
                    testResult = MFTestResults.KnownFailure;
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_YearTest54()
        {
            /// <summary>
            ///  1. Verifies the Year property
            /// </summary>
            ///
            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                Log.Comment("Creating a DateTime.Today, getting the year and Verifying");
                DateTime testDateTime = GetRandomDateTime();
                Int32 _year = testDateTime.Year;
                if (_year != year)
                {
                    testResult = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }
        //===================================================================================
        //The Following test are commented out because they throw Debug Assert in build Envt.
        //Please see 23143 for details
        //===================================================================================

        [TestMethod]
        public MFTestResults DateTime_Year_ArgumentOutOfRangeExceptionTest55()
        {

            //    MFTestResults testResult = MFTestResults.Fail;
            try
            {
                //    Log.Comment("Creating a DateTime with -ve, 0 or above max year and,");
                //    Log.Comment("verifying ArgumentOutOfRangeException is thrown");
                //    int exCounter = 0;
                //    int[] yr = new int[] { -(random.Next(100) + 1), 0, random.Next(10) + 10000 };
                //    for (int i = 0; i < yr.Length; i++)
                //    {
                //        Log.Comment("Year = " + yr[i]);
                //        try
                //        {
                //            DateTime dt = new DateTime(yr[i], 1, 1);
                //        }
                //        catch (ArgumentOutOfRangeException ex)
                //        {
                //            Log.Comment("Correctly caught ArgumentOutOfRangeException : " + ex.Message);
                //            exCounter++;
                //        }
                //    }
                //    if (exCounter != yr.Length)
                //    {
                //        Log.Comment("Expected ArgumentOutOfRangeException '" + yr.Length + "' but got '" + yr.Length + "'");
                //        testResult = MFTestResults.Fail;
                //    }

                //    return testResult;
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            Log.Comment("Creating DateTime with invalid year generates Debug Assert in Build Envt. Bug 23143");
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults DateTime_Month_ArgumentOutOfRangeExceptionTest56()
        {

            //    MFTestResults testResult = MFTestResults.Fail;
            try
            {
                //    Log.Comment("Creating a DateTime with -ve, 0 or above max month and,");
                //    Log.Comment("verifying ArgumentOutOfRangeException is thrown");
                //    int exCounter = 0;
                //    int[] mth = new int[] { -(random.Next(100) + 1), 0, random.Next(10) + 13 };
                //    for (int i = 0; i < mth.Length; i++)
                //    {
                //        Log.Comment("Month = " + mth[i]);
                //        try
                //        {
                //            DateTime dt = new DateTime(random.Next(9999) + 1, mth[i], 1);
                //        }
                //        catch (ArgumentOutOfRangeException ex)
                //        {
                //            Log.Comment("Correctly caught ArgumentOutOfRangeException : " + ex.Message);
                //            exCounter++;
                //        }
                //    }
                //    if (exCounter != mth.Length)
                //    {
                //        Log.Comment("Expected ArgumentOutOfRangeException '" + mth.Length + "' but got '" + mth.Length + "'");
                //        testResult = MFTestResults.Fail;
                //    }

                //    return testResult;
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            Log.Comment("Creating DateTime with invalid Month generates Debug Assert in Build Envt. Bug 23143");
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults DateTime_Day_ArgumentOutOfRangeExceptionTest57()
        {

            //    MFTestResults testResult = MFTestResults.Fail;
            try
            {
                //    Log.Comment("Creating a DateTime with -ve, 0 or above max day and,");
                //    Log.Comment("verifying ArgumentOutOfRangeException is thrown");            
                //    int exCounter = 0;
                //    int[] dy = new int[] { -(random.Next(100) + 1), 0, random.Next(10) + 32 };
                //    for (int i = 0; i < dy.Length; i++)
                //    {
                //        Log.Comment("Day = " + dy[i]);
                //        try
                //        {
                //            DateTime dt = new DateTime(random.Next(9999) + 1, random.Next(12) + 1, dy[i]);
                //        }
                //        catch (ArgumentOutOfRangeException ex)
                //        {
                //            Log.Comment("Correctly caught ArgumentOutOfRangeException : " + ex.Message);
                //            exCounter++;
                //        }
                //    }
                //    Log.Comment("Getting a leap year, and creating a DateTime with 30 days in Feb.,");
                //    Log.Comment("verifying ArgumentOutOfRangeException is thrown");
                //    DateTime leapDateTime = GetLeapYearDateTime();
                //    try
                //    {
                //        DateTime dt = new DateTime(leapYear[random.Next(leapYear.Length)], 2, 30);
                //    }
                //    catch (ArgumentOutOfRangeException ex)
                //    {
                //        Log.Comment("Correctly caught ArgumentOutOfRangeException : " + ex.Message);
                //        exCounter++;
                //    }
                //    if (exCounter != (dy.Length + 1))
                //    {
                //        Log.Comment("Expected ArgumentOutOfRangeException '" + dy.Length + "' but got '" + dy.Length + "'");
                //        testResult = MFTestResults.Fail;
                //    }

                //    return testResult;
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            Log.Comment("Creating DateTime with invalid Day generates Debug Assert in Build Envt. Bug 23143");
            return MFTestResults.Skip;
        }

        [TestMethod]
        public MFTestResults DateTime_BelowMinDateTime_ArgumentOutOfRangeExceptionTest58()
        {

            MFTestResults testResult = MFTestResults.Fail;
            try
            {
                Log.Comment("Creating a DateTime with -ve Ticks and,");
                Log.Comment("verifying ArgumentOutOfRangeException is thrown");
                try
                {
                    DateTime dt = new DateTime(-(new Random().Next(10) + 1));
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    Log.Comment("Correctly caught ArgumentOutOfRangeException : " + ex.Message);
                    testResult = MFTestResults.Pass;
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults DateTime_AboveMaxDatTime_ArgumentOutOfRangeExceptionTest59()
        {

            MFTestResults testResult = MFTestResults.Pass;
            try
            {
                bool bRest1 = false, bRest2 = false;
                Log.Comment("Creating a DateTime later than DateTime.MaxValue and,");
                Log.Comment("verifying ArgumentOutOfRangeException is thrown");
                try
                {
                    DateTime dt1 = new DateTime(DateTime.MaxValue.Ticks + 1);
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    Log.Comment("Correctly caught ArgumentOutOfRangeException : " + ex.Message);
                    bRest1 = true;
                }
                try
                {
                    DateTime dt2 = new DateTime(10000, 1, 1, 0, 0, 0, 0);
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    Log.Comment("Correctly caught ArgumentOutOfRangeException : " + ex.Message);
                    bRest2 = true;
                }
                if (!bRest1)
                {
                    Log.Comment("new DateTime(DateTime.MaxValue.Ticks + 1) didn't throw ArgumentOutOfRangeException");
                    testResult = MFTestResults.Fail;
                }
                else if (!bRest2)
                {
                    Log.Comment("new DateTime(10000, 1, 1, 0, 0, 0, 0)  didn't throw ArgumentOutOfRangeException");
                    testResult = MFTestResults.KnownFailure;
                }
            }
            catch (Exception ex)
            {
                Log.Comment("Caught Unexpected Exception : " + ex.Message);
                return MFTestResults.Fail;
            }

            return testResult;
        }

        static double[] rdmFraction = new double[] { 0.0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9 };

        static int year, month, day, hour, minute, second, millisec;

        int[] leapYear = new int[]{2000, 2004, 2008, 2012, 2016, 2020, 2024, 2028, 2032, 2036, 2040, 2044, 2048,
                2052, 2056, 2060, 2064, 2068, 2072, 2076, 2080, 2084, 2088, 2092, 2096};

        private DateTime[] Get_ArrayOfRandomDateTimes()
        {
            DateTime[] _dateTimeArr = new DateTime[]{DateTime.Now,
                DateTime_btwn_1801_And_2801(), DateTime_btwn_1801_And_2801(), DateTime_btwn_1801_And_2801(),                 
                GetLeapYearDateTime() ,GetLeapYearDateTime() , GetLeapYearDateTime(),
            DateTime_btwn_1801_And_2801(), DateTime_btwn_1801_And_2801(), DateTime_btwn_1801_And_2801(),
             GetLeapYearDateTime() ,GetLeapYearDateTime() , GetLeapYearDateTime()};

            return _dateTimeArr;
        }

        private DateTime DateTime_btwn_1801_And_2801()
        {
            //Generates random DateTime b/n 1000 and 9000
            Random random = new Random();
            year = random.Next(999) + 1801;
            month = random.Next(12) + 1;
            if (month == 2 && IsLeapYear(year))
                day = random.Next(29) + 1;
            else if (month == 2 && (!IsLeapYear(year)))
                day = random.Next(28) + 1;
            else if (((month <= 7) && ((month + 1) % 2 == 0)) ||
                ((month > 7) && ((month % 2) == 0)))
                day = random.Next(31) + 1;
            else
                day = random.Next(30) + 1;
            hour = random.Next(24);
            minute = random.Next(60);
            second = random.Next(60);
            millisec = random.Next(1000);

            return new DateTime(year, month, day, hour, minute, second, millisec);
        }

        private DateTime GetRandomDateTime()
        {
            //Generates random DateTime
            Random random = new Random();
            year = random.Next(1399) + 1601;
            month = random.Next(12) + 1;
            if (month == 2 && IsLeapYear(year))
                day = random.Next(29) + 1;
            else if (month == 2 && (!IsLeapYear(year)))
                day = random.Next(28) + 1;
            else if (((month <= 7) && ((month + 1) % 2 == 0)) ||
                ((month > 7) && ((month % 2) == 0)))
                day = random.Next(31) + 1;
            else
                day = random.Next(30) + 1;
            hour = random.Next(24);
            minute = random.Next(60);
            second = random.Next(60);
            millisec = random.Next(1000);

            return new DateTime(year, month, day, hour, minute, second, millisec);
        }

        private DateTime GetLeapYearDateTime()
        {
            Random random = new Random();
            year = leapYear[random.Next(leapYear.Length)];
            month = random.Next(12) + 1;
            day = random.Next(29) + 1;
            hour = random.Next(24);
            minute = random.Next(60);
            second = random.Next(60);
            millisec = random.Next(1000);
            return new DateTime(year, month, day, hour, minute, second, millisec);
        }

        private bool IsLeapYear(int yr)
        {
            if ((yr % 400 == 0) || ((yr % 100 != 0) && (yr % 4 == 0)))
                return true;
            else
                return false;
        }

        private bool CheckDeviation(long dTicks1, long dTicks2)
        {
            long diff = System.Math.Abs((int)(dTicks2 - dTicks1));
            //fail if deviates by more than 0.05ms (500 ticks)
            if (diff > 500)
            {
                Log.Comment("Difference ticks = '" + diff.ToString() + "'");
                return false;
            }
            return true;
        }

        private MFTestResults DaysInMonthTest(int yr, int mnth)
        {
            MFTestResults tResult = MFTestResults.Pass;
            int daysInMonth = DateTime.DaysInMonth(yr, mnth);
            Log.Comment("Got " + daysInMonth + " number of days in " + mnth + "/" + yr + " mm/yr");
            if (mnth == 2)
            {
                if (IsLeapYear(yr))
                {
                    if (daysInMonth != 29)
                    {
                        Log.Comment("Year '" + yr + "' is a LeapYear, expected '29' days but got '" +
                            daysInMonth + "' in Month '" + mnth + "'");
                        tResult = MFTestResults.Fail;
                    }
                }
                else if (daysInMonth != 28)
                {
                    Log.Comment("Year '" + yr + "' Month '" + mnth +
                        "', expected '28' days but got '" + daysInMonth + "'");
                    tResult = MFTestResults.Fail;
                }
            }
            else if (((mnth <= 7) && ((mnth + 1) % 2 == 0)) ||
            ((mnth > 7) && ((mnth % 2) == 0)))
            {
                if (daysInMonth != 31)
                {
                    Log.Comment("Year '" + yr + "' Month '" + mnth +
                        "', expected '31' days but got '" + daysInMonth + "'");
                    tResult = MFTestResults.Fail;
                }
            }
            else
            {
                if (daysInMonth != 30)
                {
                    Log.Comment("Year '" + yr + "' Month '" + mnth +
                        "', expected '30' days but got '" + daysInMonth + "'");
                    tResult = MFTestResults.Fail;
                }
            }
            return tResult;
        }
    }
}
