////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using System.Globalization;

namespace Microsoft.SPOT.Platform.Tests
{
    public class SystemTimeZoneTests : IMFTestInterface
    {
        string[] TimeZoneNames = { "SPACER 0", "SPACER 1", "Dateline", "Samoa/Midway",
            "Hawaii", "Alaska", "Pacific-US", "Arizona", "Mountain-US", "Cent. America", 
            "Central-US", "Saskatchewan", "Mexico City", "Indiana", "Bogota,Lima", "Eastern-US", 
            "Caracas,La Paz", "Santiago", "Atlantic", "Newfoundland", "Brasilia", "Greenland", 
            "Buenos Aires", "Mid-Atlantic", "Cape Verde", "Azores", "Casablanca", "GMT", 
            "London,Dublin", "W Cent Africa", "Prague,Budapest", "Warsaw,Sofjia", "Paris,Madrid", 
            "Berlin,Rome", "Cairo", "Pretoria", "Bucharest", "Helsinki", "Athens", 
            "Riyadh,Kuwait", "Nairobi", "Moscow", "Baghdad", "Tehran", "Baku,Tbilisi", "Kabul", 
            "Abu Dhabi", "Yekaterinburg", "Islamabad", "New Delhi", "Kathmandu", "Astana,Dhaka", 
            "Sri Lanka", "Almaty", "Yangon", "Bangkok", "Krasnoyarsk", "Beijing, HK", "Malaysia", 
            "Taipei", "Perth", "Ulaanbataar", "Seoul", "Tokyo,Osaka", "Yakutsk", "Darwin", 
            "Adelaide", "Sydney", "Brisbane", "Hobart", "Guam", "Vladivostok", "Magadan", 
            "Fiji Islands", "New Zealand", "Tonga" };
            
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
        public MFTestResults NotImplemented()
        {
            Log.Comment("All cases were disabled because of change to ExtendedTimeZone.  These cases need to be refactors when Time Sync feature is ready for test.");
            return MFTestResults.KnownFailure;
        }

        //TimeZone Test methods
        /*[TestMethod]
        public MFTestResults Ctor_Test0()
        {
            /// <summary>
            /// 1. Test the TimeZone constructor
            /// </summary>
            ///
            Log.Comment("Tests the constructor of the TimeZone object");
            Log.Comment("references a list of the time zone names in this file");
            bool testResult = true;
            TimeZone tz = TimeZone.CurrentTimeZone;
            Log.Comment(tz.StandardName);
            for (TimeZoneId tzid = TimeZoneId.Dateline; tzid <= TimeZoneId.Tonga; tzid++)
            {
                tz = ExtendedTimeZone.GetTimeZone(tzid);
                string tzName = TimeZoneNames[(int)tzid];
                Log.Comment(tz.StandardName + " == " + tzName);
                testResult &= tz.StandardName == tzName;
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }
        [TestMethod]
        public MFTestResults IsDaylightSavingTime_Test1()
        {
            /// <summary>
            /// 1. Test the IsDaylightSavingTime method
            /// Each of: Start, Immediately pre-start, End, Immediate pre-end
            /// For: Pacific 2003, 2009, Europe 2003, 2009
            /// </summary>
            ///
            /// IsDaylightSavingTime is failing.  This is bug number:
            /// 21290	IsDaylightSavingTime is failing by returning true when it isn't daylight savings time.

            Log.Comment("Tests the IsDaylightSavingTime method");
            bool testResult = true;
            Log.Comment("TimeZone::IsDaylightSavingTime - Normal ");
            TimeZone tz = ExtendedTimeZone.GetTimeZone(TimeZoneId.Pacific);

            Log.Comment("just before daylight savings");
            DateTime beforeSavingTime2007 = new DateTime(2007, 3, 11, 1, 59, 59, 999);
            testResult &= !tz.IsDaylightSavingTime(beforeSavingTime2007);

            Log.Comment("start of daylight savings");
            DateTime afterSavingTime2007 = new DateTime(2007, 3, 11, 2, 0, 0, 0);
            testResult &= tz.IsDaylightSavingTime(afterSavingTime2007);
            
            Log.Comment("end of daylight savings");
            testResult &= !tz.IsDaylightSavingTime(new DateTime(2007, 11, 4, 2, 0, 0, 0));
            Log.Comment("just before end of daylight savings");
            testResult &= tz.IsDaylightSavingTime(new DateTime(2007, 11, 4, 1, 59, 59, 999));

            for (int i = 0; i < 2; i++)
            {
                DateTime DT1 = new DateTime(2007, 3,
                    Microsoft.SPOT.Math.Random(19) + 12, Microsoft.SPOT.Math.Random(23),
                    Microsoft.SPOT.Math.Random(59), Microsoft.SPOT.Math.Random(59),
                    Microsoft.SPOT.Math.Random(999));
                testResult &= tz.IsDaylightSavingTime(DT1);
                Log.Comment(DT1 + " " + testResult);

                DateTime DT2 = new DateTime(2007, 
                    Microsoft.SPOT.Math.Random(1) + 1, Microsoft.SPOT.Math.Random(28)+1, 
                    Microsoft.SPOT.Math.Random(23), Microsoft.SPOT.Math.Random(59), 
                    Microsoft.SPOT.Math.Random(59), Microsoft.SPOT.Math.Random(999));
                testResult &= !tz.IsDaylightSavingTime(DT2);
                Log.Comment(DT2 + " " + testResult);

                DateTime DT3 = new DateTime(2003,
                    Microsoft.SPOT.Math.Random(1) + 11, Microsoft.SPOT.Math.Random(25)+5,
                    Microsoft.SPOT.Math.Random(23), Microsoft.SPOT.Math.Random(59),
                    Microsoft.SPOT.Math.Random(59), Microsoft.SPOT.Math.Random(999));
                testResult &= !tz.IsDaylightSavingTime(DT3);
                Log.Comment(DT3 + " " + testResult);
            }

            Log.Comment("Future: 2009");
            Log.Comment(" start of daylight savings");
            testResult &= tz.IsDaylightSavingTime(new DateTime(2009, 3, 8, 3, 0, 0, 0));
            Log.Comment(testResult.ToString());
            Log.Comment(" just before daylight savings");
            testResult &= !tz.IsDaylightSavingTime(new DateTime(2009, 3, 8, 1, 59, 59, 999));
            Log.Comment(testResult.ToString());
            Log.Comment(" end of daylight savings");
            testResult &= !tz.IsDaylightSavingTime(new DateTime(2009, 11, 1, 2, 0, 0, 0));
            Log.Comment(testResult.ToString());
            Log.Comment(" just before end of daylight savings");
            testResult &= tz.IsDaylightSavingTime(new DateTime(2009, 11, 1, 1, 59, 59, 999));
            Log.Comment(testResult.ToString());

            Log.Comment("Far Future: 2207");
            Log.Comment(" start of daylight savings");
            testResult &= tz.IsDaylightSavingTime(new DateTime(2207, 3, 8, 2, 0, 0, 0));
            Log.Comment(testResult.ToString());
            Log.Comment(" just before daylight savings");
            testResult &= !tz.IsDaylightSavingTime(new DateTime(2207, 3, 8, 1, 59, 59, 999));
            Log.Comment(testResult.ToString());
            Log.Comment(" end of daylight savings");
            testResult &= !tz.IsDaylightSavingTime(new DateTime(2207, 11, 1, 2, 0, 0, 0));
            Log.Comment(testResult.ToString());
            Log.Comment(" just before end of daylight savings");
            testResult &= tz.IsDaylightSavingTime(new DateTime(2207, 11, 1, 1, 59, 59, 999));
            Log.Comment(testResult.ToString());

            Log.Comment("British Summer Time : In 2003:  -  March 30 October 26 ");
            testResult = true;
            tz = ExtendedTimeZone.GetTimeZone(TimeZoneId.London);
            Log.Comment(" start of daylight savings");
            testResult &= tz.IsDaylightSavingTime(new DateTime(2003, 3, 30, 1, 0, 0, 0));
            Log.Comment(testResult.ToString());
            Log.Comment(" just before daylight savings");
            testResult &= !tz.IsDaylightSavingTime(new DateTime(2003, 3, 30, 0, 59, 59, 999));
            Log.Comment(testResult.ToString());
            Log.Comment(" end of daylight savings");
            testResult &= !tz.IsDaylightSavingTime(new DateTime(2003, 10, 26, 2, 0, 0, 0));
            Log.Comment(testResult.ToString());
            Log.Comment(" just before end of daylight savings");
            testResult &= tz.IsDaylightSavingTime(new DateTime(2003, 10, 26, 0, 59, 59, 999));
            Log.Comment(testResult.ToString());

            Log.Comment(" Future - British Summer Time : In 2009: the Sundays of 29 March and 25 October  - http://www.berr.gov.uk/employment/bank-public-holidays/bst/page12528.html ");
            testResult &= tz.IsDaylightSavingTime(new DateTime(2009, 3, 29, 1, 0, 0, 0));
            Log.Comment(testResult.ToString());
            Log.Comment(" just before daylight savings");
            testResult &= !tz.IsDaylightSavingTime(new DateTime(2009, 3, 29, 0, 59, 59, 999));
            Log.Comment(testResult.ToString());
            Log.Comment(" end of daylight savings");
            testResult &= !tz.IsDaylightSavingTime(new DateTime(2009, 10, 25, 2, 0, 0, 0));
            Log.Comment(testResult.ToString());
            Log.Comment(" just before end of daylight savings");
            testResult &= tz.IsDaylightSavingTime(new DateTime(2009, 10, 25, 1, 59, 59, 999));
            Log.Comment(testResult.ToString());

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }
        // NOT IMPLEMENENTED
        [TestMethod]
        public MFTestResults GetDaylightChanges_Test()
        {
            /// <summary>
            /// 1. GetDaylightChanges nethod NYI: Returns None
            /// </summary>
            ///
            Log.Comment("This would test GetDaylightChanges if it were implemented in the MF");
            return MFTestResults.Skip;
        }
        [TestMethod]
        public MFTestResults ToLocalTime_Test2()
        {
            /// <summary>
            ///  1. Calls DateTime.ToLocalTime
            ///  2. Verifies the output
            /// </summary>
            ///
            bool testResult = true;
            ExtendedTimeZone.SetTimeZone(TimeZoneId.Pacific);
            Log.Comment(ExtendedTimeZone.CurrentTimeZone.StandardName);
            TimeZone tz = ExtendedTimeZone.GetTimeZone(TimeZoneId.Pacific);
            for (TimeZoneId tzid = TimeZoneId.FIRST; tzid <= TimeZoneId.LAST; tzid++)
            {
                tz = ExtendedTimeZone.GetTimeZone(tzid);
                Log.Comment(tz.StandardName);
                Microsoft.SPOT.Math.Randomize();
                for (int i = 0; i < 2; i++)
                {
                    DateTime dt = new DateTime(Microsoft.SPOT.Math.Random(10) + 2000, 
                        Microsoft.SPOT.Math.Random(12) + 1, Microsoft.SPOT.Math.Random(28) + 1, 
                        Microsoft.SPOT.Math.Random(23), Microsoft.SPOT.Math.Random(59), 
                        Microsoft.SPOT.Math.Random(59), Microsoft.SPOT.Math.Random(999));
                    // Set dt to UTC time kind. 
                    dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                    Log.Comment("Calling DateTime.ToLocalTime");
                    DateTime dt2 = tz.ToLocalTime(dt);
                    Log.Comment("DST: " + tz.IsDaylightSavingTime(dt).ToString());
                    Log.Comment("Offset: " + tz.GetUtcOffset(dt).ToString());
                    bool bDST = tz.IsDaylightSavingTime(dt);
                    dt = dt.Add(tz.GetUtcOffset(dt));
                    Log.Comment("Verifying");
                    testResult &= (dt2 == dt);
                }
                if (!testResult)
                {
                    break;
                }
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }
        [TestMethod]
        public MFTestResults ToUniversalTime_Test3()
        {
            /// <summary>
            ///  1. Calls DateTime.ToUniversalTime
            ///  2. Verifies the output
            /// </summary>
            ///
            bool testResult = true;
            ExtendedTimeZone.SetTimeZone(TimeZoneId.Pacific);
            Log.Comment(ExtendedTimeZone.CurrentTimeZone.StandardName);
            TimeZone tz = ExtendedTimeZone.GetTimeZone(TimeZoneId.Pacific);
            for (TimeZoneId tzid = TimeZoneId.Dateline; tzid <= TimeZoneId.Tonga; tzid++)
            {
                tz = ExtendedTimeZone.GetTimeZone(tzid);
                Log.Comment(tz.StandardName);
                Microsoft.SPOT.Math.Randomize();
                for (int i = 0; i < 2; i++)
                {
                    DateTime dt = new DateTime(Microsoft.SPOT.Math.Random(10) + 2000,
                        Microsoft.SPOT.Math.Random(12) + 1, Microsoft.SPOT.Math.Random(28) + 1, 
                        Microsoft.SPOT.Math.Random(23), Microsoft.SPOT.Math.Random(59), 
                        Microsoft.SPOT.Math.Random(59), Microsoft.SPOT.Math.Random(999));
                    Log.Comment("Calling DateTime.ToUniversalTime");
                    DateTime dt2 = tz.ToUniversalTime(dt);
                    Log.Comment("DST: " + tz.IsDaylightSavingTime(dt).ToString());
                    Log.Comment("Offset: " + tz.GetUtcOffset(dt).ToString());
                    bool bDST = tz.IsDaylightSavingTime(dt);
                    dt = dt.Subtract(tz.GetUtcOffset(dt));
                    Log.Comment("Verifying");
                    testResult &= (dt2 == dt);
                }
                if (!testResult)
                {
                    break;
                }
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }
        [TestMethod]
        public MFTestResults GetUtcOffset_Test4()
        {
            /// <summary>
            ///  1. Calls DateTime.GetUtcOffset for Pacific, Arizona, Dateline and London
            ///  2. Verifies the output for each
            /// </summary>
            ///
            bool testResult = true;
            Log.Comment("TimeZone::GetUtcOffset - Normal ");
            ExtendedTimeZone.SetTimeZone(TimeZoneId.Pacific);
            Log.Comment("Pacific");
            TimeZone tz = ExtendedTimeZone.GetTimeZone(TimeZoneId.Pacific);
            testResult &= (tz.GetUtcOffset(new DateTime(2003, 1, 1, 0, 0, 0, 0)).Hours == -8);
            testResult &= (tz.GetUtcOffset(new DateTime(2003, 4, 6, 2, 0, 0, 0)).Hours == -7);
            
            Log.Comment("Arizona"); 
            tz = ExtendedTimeZone.GetTimeZone(TimeZoneId.Arizona);
            testResult &= (tz.GetUtcOffset(new DateTime(2003, 1, 1, 0, 0, 0, 0)).Hours == -7);
            testResult &= (tz.GetUtcOffset(new DateTime(2003, 4, 6, 5, 0, 0, 0)).Hours == -7);
            
            Log.Comment("Dateline");
            tz = ExtendedTimeZone.GetTimeZone(TimeZoneId.Dateline);
            testResult &= (tz.GetUtcOffset(new DateTime(2003, 1, 1, 0, 0, 0, 0)).Hours == -12);
            testResult &= (tz.GetUtcOffset(new DateTime(2003, 6, 6, 5, 0, 0, 0)).Hours == -12);
            
            Log.Comment("London");
            tz = ExtendedTimeZone.GetTimeZone(TimeZoneId.London);
            testResult &= (tz.GetUtcOffset(new DateTime(2003, 1, 1, 0, 0, 0, 0)).Hours == 0);
            testResult &= (tz.GetUtcOffset(new DateTime(2003, 6, 6, 5, 0, 0, 0)).Hours == 1);

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }
        [TestMethod]
        public MFTestResults CurrentTimeZone_Test5()
        {
            /// <summary>
            ///  1. Calls DateTime.CurrentTimeZone for each time zone
            ///  2. Verifies the output for each
            /// </summary>
            ///
            bool testResult = true;
            Log.Comment("Tests CurrentTimeZone for each TimeZone");
            for (TimeZoneId tzid = TimeZoneId.FIRST; tzid <= TimeZoneId.LAST; tzid++)
            {
                TimeZone tz1 = ExtendedTimeZone.GetTimeZone(tzid);
                Log.Comment(tz1.ToString());
                ExtendedTimeZone.SetTimeZone(tzid);
                TimeZone tz2 = TimeZone.CurrentTimeZone;
                testResult &= (tz1.StandardName == tz2.StandardName);
                testResult &= (tz1.DaylightName == tz2.DaylightName);
                if (!testResult)
                {
                    Log.Comment(tz1.StandardName + " == " + tz2.StandardName);
                }
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }
        [TestMethod]
        public MFTestResults StandardName_Test6()
        {
            /// <summary>
            ///  1. Tests the StandardName property of each TimeZone
            /// </summary>
            ///
            bool testResult = true;
            Log.Comment("Tests the StandardName property of each TimeZone");
            for (TimeZoneId tzid = TimeZoneId.Dateline; tzid <= TimeZoneId.Tonga; tzid++)
            {
                TimeZone tz1 = ExtendedTimeZone.GetTimeZone(tzid);
                string tzName = TimeZoneNames[(int)tzid];
                Log.Comment(tz1.StandardName);
                testResult &= tz1.StandardName == tzName;
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }
        [TestMethod]
        public MFTestResults DaylightName_Test7()
        {
            /// <summary>
            ///  1. Tests the DaylightName property of each TimeZone
            /// </summary>
            ///
            bool testResult = true;
            Log.Comment("Tests the DaylightName property of each TimeZone");
            for (TimeZoneId tzid = TimeZoneId.Dateline; tzid <= TimeZoneId.Tonga; tzid++)
            {
                TimeZone tz1 = ExtendedTimeZone.GetTimeZone(tzid);
                string tzName = TimeZoneNames[(int)tzid];
                Log.Comment(tz1.DaylightName);
                testResult &= tz1.DaylightName == tzName;
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }
         * */
    }
}
