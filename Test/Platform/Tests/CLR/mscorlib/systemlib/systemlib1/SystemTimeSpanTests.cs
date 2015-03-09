////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class SystemTimeSpanTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            return InitializeResult.ReadyToGo;
        }
        
        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests");
        }

        //TimeSpan Test methods
		const long m_TicksPerDay			= 864000000000;
		const long m_TicksPerHour			= 36000000000;
		const long m_TicksPerMinute		    = 600000000;
		const long m_TicksPerSecond		    = 10000000;
		const long m_TicksPerMillisecond	= 10000;

		private bool CCtorHelper( ref TimeSpan ts )
		{
			bool testResult = true;
			try
			{
				ts = new TimeSpan();
				testResult &= ts.Ticks == 0;
				testResult &= ts.Days == 0;
				testResult &= ts.Hours == 0;
				testResult &= ts.Minutes == 0;
				testResult &= ts.Seconds == 0;
				testResult &= ts.Milliseconds == 0;
				if( !testResult )
					Log.Comment( ts.ToString() );
			}
			catch
			{
				Log.Comment("Exception Caught");
				testResult = false;
			}
			Log.Comment( (testResult?"PASS":"FAIL") );
			return testResult;
		}
		[TestMethod]
		public MFTestResults CCtor_Test(  )
		{
            /// <summary>
            /// 1. Test copy constructor
            /// </summary>
            ///
			Log.Comment( "Copy Constructor test" );
			bool testResult = true;
			TimeSpan ts = new TimeSpan();
			Log.Comment(ts.ToString());
			testResult &= CCtorHelper( ref ts );
			ts = TimeSpan.MaxValue;
			ts = new TimeSpan();
			Log.Comment( ts.ToString() );
			testResult &= CCtorHelper( ref ts );
			ts = TimeSpan.MinValue;
			testResult &= CCtorHelper( ref ts );
			int mxd = 24000;
			int mx = 1000;

            Random random = new Random();
			for( int i = 0; i<5; i++ )
			{
				int id = (random.Next(1)==0? -1: 1)*
                    random.Next(mxd);
				int ih = (random.Next(1)==0? -1: 1)*
                    random.Next(mx);
				int im = (random.Next(1)==0? -1: 1)*
                    random.Next(mx);
				int ise = (random.Next(1)==0? -1: 1)*
                    random.Next(mx);
				int ims = (random.Next(1)==0? -1: 1)*
                    random.Next(mx);
				ts = new TimeSpan( id, ih, im, ise, ims );
				testResult &= CCtorHelper( ref ts );
			}
	
			return (testResult? MFTestResults.Pass: MFTestResults.Fail);
		}
		bool Ctor64Helper( Int64 i64 )
		{
            /// <summary>
            /// 1. Test constructor
            /// </summary>
            ///
            Log.Comment("Constructor test");
			bool testResult = true;
			try
			{
				TimeSpan ts = new TimeSpan( i64 );
				long days			= i64/m_TicksPerDay;
				long hours			= (i64-ts.Days*m_TicksPerDay)/m_TicksPerHour;
				long minutes		= (i64-ts.Days*m_TicksPerDay-ts.Hours*
                    m_TicksPerHour)/m_TicksPerMinute;
				long seconds		= (i64-ts.Days*m_TicksPerDay-ts.Hours*
                    m_TicksPerHour-ts.Minutes*m_TicksPerMinute)/m_TicksPerSecond;
				long milliseconds	= (i64-ts.Days*m_TicksPerDay-ts.Hours*
                    m_TicksPerHour-ts.Minutes*m_TicksPerMinute-ts.Seconds*m_TicksPerSecond)/
                    m_TicksPerMillisecond;
				testResult &= (ts.Days == days);
				testResult &= (ts.Hours == hours);
				testResult &= (ts.Minutes == minutes);
				testResult &= (ts.Seconds == seconds);
				testResult &= (ts.Milliseconds == milliseconds);
				Log.Comment( ts.ToString() );
			}
			catch
			{
				Log.Comment( "Exception occurred" );
				testResult = false;
			}
			return testResult;
		}
		[TestMethod]
		public MFTestResults Ctor_Test(  )
		{
            /// <summary>
            /// 1. Test constructor with Int64
            /// </summary>
            ///
            Log.Comment("Constructor test for 64 bit int");
			bool testResult = true;
			Int64 i64 = 0;
            Log.Comment("Normal");
            Random random = new Random();

			for( int i = 0; i < 5; i++ )
			{
				i64 = random.Next(int.MaxValue);
				i64 *= random.Next(10000);
				testResult &= Ctor64Helper( i64 );
			}

            Log.Comment("Max and Min values");
			i64 = 0x7fffffffffffffff;
			testResult &= Ctor64Helper( i64 );
			testResult &= Ctor64Helper( TimeSpan.MaxValue.Ticks );
			i64 = 0;
			testResult &= Ctor64Helper( i64 );
			i64 = -9223372036854775808;
			testResult &= Ctor64Helper( i64 );
			Log.Comment( "MAX VALUES: " + TimeSpan.MaxValue.Ticks.ToString() + " AND " + 
                Int64.MaxValue );
			testResult &= Ctor64Helper( Int64.MaxValue );
			testResult &= Ctor64Helper( TimeSpan.MinValue.Ticks );

			return (testResult? MFTestResults.Pass: MFTestResults.Fail);
		}
		bool CtorHelper( int[] vals )
		{
			bool testResult = true;
			try
			{
				TimeSpan ts = new TimeSpan(0);
				Int64 i64 = 0;

				string str = "";
				if( vals.Length == 3 )
					str = "0 : ";
				for( int i=0; i<vals.Length; i++ )
					str += vals[i].ToString() + " : ";
				for( int i=vals.Length; i<5; i++ )
					str += "0 : ";
				Log.Comment( str );

				switch( vals.Length )
				{
					case 3:
						ts = new TimeSpan( vals[0], vals[1], vals[2] );
						i64 = vals[0]*m_TicksPerHour + vals[1]*m_TicksPerMinute + vals[2]*
                            m_TicksPerSecond;
						break;
					case 4:
						ts = new TimeSpan( vals[0], vals[1], vals[2], vals[3] );
						i64 = vals[0]*m_TicksPerDay + vals[1]*m_TicksPerHour + vals[2]*
                            m_TicksPerMinute + vals[3]*m_TicksPerSecond;
						break;
					case 5:
						ts = new TimeSpan( vals[0], vals[1], vals[2], vals[3], vals[4] );
						i64 = vals[0]*m_TicksPerDay + vals[1]*m_TicksPerHour + vals[2]*
                            m_TicksPerMinute + vals[3]*m_TicksPerSecond + vals[4]*m_TicksPerMillisecond;
						break;
					default:
						Log.Comment( "Invalid parameter!" );
						testResult = false;
						break;
				}
				testResult &= (ts.Days == i64/m_TicksPerDay);
				testResult &= (ts.Hours == (i64-ts.Days*m_TicksPerDay)/m_TicksPerHour);
				testResult &= (ts.Minutes == (i64-ts.Days*m_TicksPerDay-ts.Hours*
                    m_TicksPerHour)/m_TicksPerMinute);
				testResult &= (ts.Seconds == (i64-ts.Days*m_TicksPerDay-ts.Hours*
                    m_TicksPerHour-ts.Minutes*m_TicksPerMinute)/m_TicksPerSecond);
				testResult &= (ts.Milliseconds == (i64-ts.Days*m_TicksPerDay-ts.Hours*
                    m_TicksPerHour-ts.Minutes*m_TicksPerMinute-ts.Seconds*m_TicksPerSecond)/m_TicksPerMillisecond);

				Log.Comment( ts.Days.ToString() + " : " + 
					ts.Hours.ToString() + " : " + 
					ts.Minutes.ToString() + " : " + 
					ts.Seconds.ToString() + " : " +
					ts.Milliseconds.ToString() );
			}
			catch
			{
				testResult = false;
				Log.Comment( "Exception Occurred" );
			}
			Log.Comment( testResult.ToString() );
			return testResult;
		}
		[TestMethod]
		public MFTestResults Ctor_Test1(  )
        {            
            /// <summary>
            /// 1. Test constructor with Hour Minute Second
            /// </summary>
            ///
            Log.Comment("Constructor test H:M:S");
			bool testResult = true;

			Log.Comment( m_TicksPerDay.ToString() + " == " + TimeSpan.TicksPerDay.ToString() );
			Log.Comment( m_TicksPerHour.ToString() + " == " + TimeSpan.TicksPerHour.ToString() );
			Log.Comment( m_TicksPerMinute.ToString() + " == " + 
                TimeSpan.TicksPerMinute.ToString() );
			Log.Comment( m_TicksPerSecond.ToString() + " == " + 
                TimeSpan.TicksPerSecond.ToString() );
			Log.Comment( m_TicksPerMillisecond.ToString() + " == " + 
                TimeSpan.TicksPerMillisecond.ToString() );

			int []vals = new int[3];
            Random random = new Random();

			for( int i = 0; i < 5; i++ )
			{	
				vals[0] = random.Next( 23 ); //hours
				vals[1] = random.Next( 59 ); //min
				vals[2] = random.Next( 59 ); //sec
				testResult &= CtorHelper( vals );
			}
			return (testResult? MFTestResults.Pass: MFTestResults.Fail);
		}
		[TestMethod]
		public MFTestResults Ctor_Test2(  )
		{
            /// <summary>
            /// 1. Test constructor with Day Hour Minute Second
            /// </summary>
            ///
            Log.Comment("Constructor test D:H:M:S");
			bool testResult = true;

			int []vals = new int[4];
            Random random = new Random();

			for( int i = 0; i < 5; i++ )
			{
                vals[0] = random.Next( 10*365 ) + 1;  //days
				vals[1] = random.Next( 23 );  //hours
				vals[2] = random.Next( 59 );  //minutes
				vals[3] = random.Next( 59 );  //seconds
				testResult &= CtorHelper( vals );
			}
			return (testResult? MFTestResults.Pass: MFTestResults.Fail);
		}
		[TestMethod]
		public MFTestResults Ctor_Test3(  )
		{
            /// <summary>
            /// 1. Test constructor with Day Hour Minute Second Millisecond
            /// </summary>
            ///
            Log.Comment("Constructor test D:H:M:S:MS");
			bool testResult = true;
			Log.Comment( "TimeSpan::Ctor - Normal ");

			int []vals = new int[5];
            Random random = new Random();

			for( int i = 0; i < 5; i++ )
			{	
				vals[0] = random.Next( 10*365 ) + 1;  //days
				vals[1] = random.Next( 23 );  //hours
				vals[2] = random.Next( 59 );  //minutes
				vals[3] = random.Next( 59 );  //seconds
				vals[4] = random.Next( 999 );  //milliseconds
				testResult &= CtorHelper( vals );
			}
			return (testResult? MFTestResults.Pass: MFTestResults.Fail);
		}
		[TestMethod]
		public MFTestResults CompareTo_Test4(  )
        {
            /// <summary>
            /// 1. Construct 2 Timespans
            /// 2. Test the CompareTo method
            /// </summary>
            ///
            Log.Comment("Testing the CompareTo method");
            bool testResult = true;
            Random random = new Random();
            Log.Comment("Creating TimeSpan");
            int day = random.Next(10 * 365) + 1;  //days
            int hour = random.Next(23);  //hours
            int minute = random.Next(59);  //minutes
            int second = random.Next(59);  //seconds
            int msec = random.Next(999);  //milliseconds
            TimeSpan ts1 = new TimeSpan(day, hour, minute, second, msec );

            Log.Comment("Testing CompareTo");
			testResult &= (-1 == ts1.CompareTo( new TimeSpan(day+1,hour,minute,second,msec) ) );
            Log.Comment(ts1.CompareTo(new TimeSpan(day+1, hour, minute, second, msec)).ToString());
			Log.Comment( testResult.ToString() );
            testResult &= (1 == ts1.CompareTo(new TimeSpan(day, hour-1, minute, second, msec)));
            Log.Comment(ts1.CompareTo(new TimeSpan(day, hour-1, minute, second, msec)).ToString());
			Log.Comment( testResult.ToString() );
            testResult &= (0 == ts1.CompareTo(new TimeSpan(day, hour, minute, second, msec)));
            Log.Comment(ts1.CompareTo(new TimeSpan(day, hour, minute, second, msec)).ToString());
			Log.Comment( testResult.ToString() );
			return (testResult? MFTestResults.Pass: MFTestResults.Fail);
		}
		[TestMethod]
		public MFTestResults GetHashCode_Test5(  )
		{
            /// <summary>
            /// 1. Test that GetHashCode returns the same for the same TimeSpan
            /// 2. Test that GetHashCode returns differently for different TimeSpans
            /// </summary>
            ///
            Log.Comment("Testing the GetHashCode method");
			bool testResult = true;
            Random random = new Random();
            Log.Comment("Test that GetHashCode returns the same for the same TimeSpan");
			for( int i = 0; i<30; i++ )
			{
				int hours = random.Next(23);
				int minutes = random.Next(59);
				int seconds = random.Next(59);
				TimeSpan ts01 = new TimeSpan( hours, minutes, seconds );
				TimeSpan ts02 = new TimeSpan( hours, minutes, seconds );
				Log.Comment( ts01.GetHashCode().ToString() + " == " + 
                    ts02.GetHashCode().ToString() );
				testResult &= (ts01.GetHashCode() == ts02.GetHashCode());
			}

			TimeSpan ts1 = new TimeSpan( 1,1,1 );
            Log.Comment("Test that GetHashCode returns differently for different TimeSpans");
            Log.Comment("This may fail erroneously.");  
            Log.Comment("From the docs two different TimeSpans may have same hashcode" );
			Log.Comment( "But, for the most part the values should be different." );
			for( int i = 0; i < 5; i++ )
			{
				TimeSpan ts2 = new TimeSpan( random.Next(23),
                    random.Next(59),random.Next(59) );
				Log.Comment( ts1.GetHashCode().ToString() + " Does Not Equal " + 
                    ts2.GetHashCode().ToString() );
				if( ts1 != ts2 )
					testResult &= (ts1.GetHashCode() != ts2.GetHashCode());
				else
					testResult &= (ts1.GetHashCode() == ts2.GetHashCode());
			}

			return (testResult? MFTestResults.Pass: MFTestResults.Fail);
		}
		[TestMethod]
		public MFTestResults Equals_Test6(  )
        {
            /// <summary>
            /// 1. Test the Equals method
            /// </summary>
            ///
            Log.Comment("Testing the Equals method");
			bool testResult = true;
			try
			{
				Random random = new Random();
				// verify same timespan computes to same hash
				for( int i = 0; i<5; i++ )
				{
					int hours = random.Next(23);
					int minutes = random.Next(59);
					int seconds = random.Next(59);
					TimeSpan ts01 = new TimeSpan( hours, minutes, seconds );
					TimeSpan ts02 = new TimeSpan( hours, minutes, seconds );
					Log.Comment( ts01.ToString() + " == " + ts02.ToString() );
                    Log.Comment("Expected true");
					testResult &= ts01.Equals(ts02);
					Log.Comment( testResult.ToString() );
                    TimeSpan ts03 = new TimeSpan(hours, minutes, seconds );
				    TimeSpan ts04 = new TimeSpan(hours + 1, minutes - 1, seconds + 1);
                    Log.Comment("Expected false");
                    testResult &= !ts03.Equals(ts04);
				}
			}
			catch
			{
				testResult = false;
			}
			return (testResult? MFTestResults.Pass: MFTestResults.Fail);
		}
		[TestMethod]
		public MFTestResults ToString_Test7(  )
        {
            /// <summary>
            /// 1. Test the ToString method
            /// </summary>
            ///
            Log.Comment("Testing ToString method");
			bool testResult = true;
			Random random = new Random();
			for( int i = 0; i<5; i++ )
			{
				bool b = true;
				int hours = random.Next(23);
				int minutes = random.Next(59);
				int seconds = random.Next(59);
				TimeSpan ts01 = new TimeSpan( hours, minutes, seconds );
				string str = 
					(hours<10?"0":"") + hours.ToString() + ":" + 
					(minutes<10?"0":"") + minutes.ToString() + ":" + 
					(seconds<10?"0":"") + seconds.ToString();
				b &= (str == ts01.ToString());
				Log.Comment( str + " == " + ts01.ToString() );
				Log.Comment( b.ToString() );
				testResult &= b;
			}

			return (testResult? MFTestResults.Pass: MFTestResults.Fail);
		}
		bool CompareTimeSpan( TimeSpan ts1, TimeSpan ts2 )
		{
			bool testResult = true;
			testResult &= ts1.Days == ts2.Days;
			testResult &= ts1.Hours == ts2.Hours;
			testResult &= ts1.Minutes == ts2.Minutes;
			testResult &= ts1.Seconds == ts2.Seconds;
			testResult &= ts1.Milliseconds == ts2.Milliseconds;
			return testResult;
		}
		[TestMethod]
		public MFTestResults Add_Test8(  )
        {
            /// <summary>
            /// 1. Test the binary + operator
            /// </summary>
            ///
            Log.Comment("Testing the + operator");
			bool testResult = true;
			Log.Comment( "TimeSpan::Add - Normal ");
			Random random = new Random();
			for( int i = 0; i<5; i++ )
			{
				bool b = true;
				TimeSpan ts01 = new TimeSpan( random.Next(12), 
                    random.Next(29), random.Next(29) );
				int hours = random.Next(11);
				int minutes = random.Next(30);
				int seconds = random.Next(30);
				TimeSpan ts02 = new TimeSpan( hours, minutes, seconds );
				TimeSpan ts03 = ts01.Add( ts02 );
				b &= (ts01 != ts03);
				b &= !CompareTimeSpan( ts01, ts03 );
				b &= CompareTimeSpan( ts03, ts01 + ts02 );
				b &= (ts03.Days == (ts01.Days + ts02.Days));
				b &= (ts03.Hours == (ts01.Hours + hours));
				b &= (ts03.Minutes == (ts01.Minutes + minutes));
				b &= (ts03.Seconds == (ts01.Seconds + seconds));
				b &= (ts03.Milliseconds == (ts01.Milliseconds + ts02.Milliseconds));
				Log.Comment( b.ToString() );
				testResult &= b;
			}
			TimeSpan ts1 = new TimeSpan(2,2,2,2,2);
			TimeSpan ts2 = new TimeSpan(0,0,0,0,999);
			TimeSpan ts3 = ts1.Add(ts2);
			testResult &= (ts3.Milliseconds == 1);
			testResult &= (ts3.Seconds == 3);
			testResult &= (ts3.Minutes == 2);
			testResult &= (ts3.Hours == 2);
			testResult &= (ts3.Days == 2);

			ts2 = new TimeSpan(0,0,0,58,0);
			ts3 = ts1.Add(ts2);
			testResult &= (ts3.Milliseconds == 2);
			testResult &= (ts3.Seconds == 0);
			testResult &= (ts3.Minutes == 3);
			testResult &= (ts3.Hours == 2);
			testResult &= (ts3.Days == 2);

			ts2 = new TimeSpan(0,0,59,0,0);
			ts3 = ts1.Add(ts2);
			testResult &= (ts3.Milliseconds == 2);
			testResult &= (ts3.Seconds == 2);
			testResult &= (ts3.Minutes == 1);
			testResult &= (ts3.Hours == 3);
			testResult &= (ts3.Days == 2);

			ts2 = new TimeSpan(0,22,0,0,0);
			ts3 = ts1.Add(ts2);
			testResult &= (ts3.Milliseconds == 2);
			testResult &= (ts3.Seconds == 2);
			testResult &= (ts3.Minutes == 2);
			testResult &= (ts3.Hours == 0);
			testResult &= (ts3.Days == 3);

			return (testResult? MFTestResults.Pass: MFTestResults.Fail);
		}
		[TestMethod]
		public MFTestResults Compare_Test9(  )
        {
            /// <summary>
            /// 1. Test the Compare method
            /// </summary>
            ///

            Log.Comment("Testing the Compare method");
            bool testResult = true;
            Random random = new Random();
            int day = random.Next(10 * 365) + 1;  //days
            int hour = random.Next(23);  //hours
            int minute = random.Next(59);  //minutes
            int second = random.Next(59);  //seconds
            int msec = random.Next(999);  //milliseconds
            TimeSpan ts1 = new TimeSpan(day, hour, minute, second, msec);

            testResult &= (-1 == TimeSpan.Compare(ts1, new TimeSpan
                (day, hour, minute, second+1, msec)));
            Log.Comment(TimeSpan.Compare(ts1, new TimeSpan
                (day, hour, minute, second+1, msec)).ToString());
			Log.Comment( testResult.ToString() );

            testResult &= (1 == TimeSpan.Compare(ts1, new TimeSpan
                (day, hour, minute, second, msec-1)));
            Log.Comment(TimeSpan.Compare(ts1, new TimeSpan
                (day, hour, minute, second, msec-1)).ToString());
			Log.Comment( testResult.ToString() );
            testResult &= (0 == TimeSpan.Compare(ts1, new TimeSpan
                (day, hour, minute, second, msec)));
            Log.Comment(TimeSpan.Compare(ts1, new TimeSpan
                (day, hour, minute, second, msec)).ToString());
			Log.Comment( testResult.ToString() );

			return (testResult? MFTestResults.Pass: MFTestResults.Fail);
		}
		[TestMethod]
		public MFTestResults Duration_Test10(  )
		{
            /// <summary>
            /// 1. Test the Duration property with several random TimeSpans
            /// </summary>
            ///
            Log.Comment("Testing Duration property");
			bool testResult = true;
			Random random = new Random();
			for( int i = 0; i<5; i++ )
			{
				bool b = true;
				int hours = random.Next(23);
				int minutes = random.Next(59);
				int seconds = random.Next(59);
				TimeSpan ts = new TimeSpan( -hours, -minutes, -seconds );
				testResult &= ts.Duration() == new TimeSpan( hours, minutes, seconds );
				ts = new TimeSpan( hours, minutes, seconds );
				testResult &= ts.Duration() == new TimeSpan( hours, minutes, seconds );
				testResult &= b;
			}

			return (testResult? MFTestResults.Pass: MFTestResults.Fail);
		}
		[TestMethod]
		public MFTestResults Negate_Test12(  )
        {
            /// <summary>
            /// 1. Test the Negate method
            /// </summary>
            ///
            Log.Comment("Testing the Negate method");
			bool testResult = true;
			Random random = new Random();
			for( int i = 0; i<5; i++ )
			{
				bool b = true;
				int hours = random.Next(23);
				int minutes = random.Next(59);
				int seconds = random.Next(59);
				TimeSpan tsN = new TimeSpan( -hours, -minutes, -seconds );
				TimeSpan tsP = new TimeSpan( hours, minutes, seconds );
				testResult &= tsN.Negate() == tsP;
				testResult &= tsP.Negate() == tsN;
				testResult &= tsN.Negate().Negate() == tsN;
				testResult &= tsP.Negate().Negate() == tsP;
				testResult &= b;
			}

			return (testResult? MFTestResults.Pass: MFTestResults.Fail);
		}
		[TestMethod]
		public MFTestResults Subtract_Test13(  )
        {
            /// <summary>
            /// 1. Test subtraction, the binary - operator
            /// </summary>
            ///
            Log.Comment("Testing the binary - operator");
			bool testResult = true;
			Random random = new Random();
			for( int i = 0; i<5; i++ )
			{
				bool b = true;
				TimeSpan ts01 = new TimeSpan( 23, 59, 59 );
				int hours = random.Next(23);
				int minutes = random.Next(59);
				int seconds = random.Next(59);
				TimeSpan ts02 = new TimeSpan( hours, minutes, seconds );
				TimeSpan ts03 = ts01.Subtract( ts02 );
				b &= (ts01 != ts03) || (ts02.Ticks==0);
				b &= !CompareTimeSpan( ts01, ts03 );
				b &= CompareTimeSpan( ts03, ts01 - ts02 );
				b &= (ts03.Days == (ts01.Days - ts02.Days));
				b &= (ts03.Hours == (ts01.Hours - hours));
				b &= (ts03.Minutes == (ts01.Minutes - minutes));
				b &= (ts03.Seconds == (ts01.Seconds - seconds));
				b &= (ts03.Milliseconds == (ts01.Milliseconds - ts02.Milliseconds));
				testResult &= b;
			}
			TimeSpan ts1 = new TimeSpan(2,2,2,2,2);
			TimeSpan ts2 = new TimeSpan(0,0,0,0,3);
			TimeSpan ts3 = ts1.Subtract(ts2);
			testResult &= (ts3.Milliseconds == 999);
			testResult &= (ts3.Seconds == 1);
			testResult &= (ts3.Minutes == 2);
			testResult &= (ts3.Hours == 2);
			testResult &= (ts3.Days == 2);

			ts2 = new TimeSpan(0,0,0,3,0);
			ts3 = ts1.Subtract(ts2);
			testResult &= (ts3.Milliseconds == 2);
			testResult &= (ts3.Seconds == 59);
			testResult &= (ts3.Minutes == 1);
			testResult &= (ts3.Hours == 2);
			testResult &= (ts3.Days == 2);

			ts2 = new TimeSpan(0,0,3,0,0);
			ts3 = ts1.Subtract(ts2);
			testResult &= (ts3.Milliseconds == 2);
			testResult &= (ts3.Seconds == 2);
			testResult &= (ts3.Minutes == 59);
			testResult &= (ts3.Hours == 1);
			testResult &= (ts3.Days == 2);

			ts2 = new TimeSpan(0,3,0,0,0);
			ts3 = ts1.Subtract(ts2);
			testResult &= (ts3.Milliseconds == 2);
			testResult &= (ts3.Seconds == 2);
			testResult &= (ts3.Minutes == 2);
			testResult &= (ts3.Hours == 23);
			testResult &= (ts3.Days == 1);

			ts2 = new TimeSpan(3,0,0,0,0);
			ts3 = ts1.Subtract(ts2);
			testResult &= (ts3.Milliseconds == -998);
			testResult &= (ts3.Seconds == -57);
			testResult &= (ts3.Minutes == -57);
			testResult &= (ts3.Hours == -21);
			testResult &= (ts3.Days == 0);

			return (testResult? MFTestResults.Pass: MFTestResults.Fail);
		}
		private bool FromTicksHelper( int days, int hours, int mins, int secs, int ms )
		{
			bool testResult = true;
			try
			{
				long ticks = 
					m_TicksPerDay*days + 
					m_TicksPerHour*hours + 
					m_TicksPerMinute*mins + 
					m_TicksPerSecond*secs + 
					m_TicksPerMillisecond*ms;
				TimeSpan ts = TimeSpan.FromTicks( ticks );

				int dys = (int)(ticks / m_TicksPerDay);
				Log.Comment( dys.ToString() );
				testResult &= (ts.Days == dys );
				Log.Comment(testResult.ToString() );

				int hrs = (int)(ticks/m_TicksPerHour)%24;
				Log.Comment( hrs.ToString() );
				testResult &= (ts.Hours == hrs );
				Log.Comment(testResult.ToString() );

				int mns = (int)(ticks/m_TicksPerMinute)%60;
				Log.Comment( mns.ToString() );
				testResult &= (ts.Minutes == mns );
				Log.Comment(testResult.ToString() );

				int scs = (int)(ticks/m_TicksPerSecond)%60;
				Log.Comment( scs.ToString() );
				testResult &= (ts.Seconds == scs);
				Log.Comment(testResult.ToString() );

				int mss = (int)ms%1000;
				Log.Comment( "MS: " + mss.ToString() );
				testResult &= (ts.Milliseconds == mss);
				Log.Comment(testResult.ToString() );

				Log.Comment( "Days= " + days + " Hours= " + hours + 
                    " Minutes= " + mins + " Secs= " + secs + " ms= " + ms );
				Log.Comment( ts.ToString() );
			}
			catch
			{
				Log.Comment( "Exception caught" );
				testResult = false;
			}
			Log.Comment( (testResult?"PASS":"FAIL" ));
			return testResult;
		}
		[TestMethod]
		public MFTestResults FromTicks_Test14(  )
        {
            /// <summary>
            /// 1. Testing the use of ticks in constructors
            /// </summary>
            ///
            Log.Comment("Testing the use of ticks, ticks per increment specified in this file");

			bool testResult = true;
			Log.Comment( "TimeSpan::FromTicks - Normal ");
			Random random = new Random();
			int max = 5000;
			int maxOthers = 200;
			for( int i = 0; i<5; i++ )
			{
				testResult &= FromTicksHelper( random.Next(max), 
                    random.Next(maxOthers),random.Next(maxOthers),
                    random.Next(maxOthers), random.Next(1000));
			}
			return (testResult? MFTestResults.Pass: MFTestResults.Fail);
		}

		private bool UnaryHelper( int days, int hours, int mins, int secs, int ms, bool neg )
		{
			bool testResult = true;
			try
			{
				long ticks = 
					m_TicksPerDay*days + 
					m_TicksPerHour*hours + 
					m_TicksPerMinute*mins + 
					m_TicksPerSecond*secs + 
					m_TicksPerMillisecond*ms;

				TimeSpan ts = new TimeSpan( days, hours, mins, secs, ms );
				TimeSpan nts = (neg? -ts: +ts);
				int d = (int)(ticks/m_TicksPerDay);
				int h = (int)((ticks%m_TicksPerDay)/m_TicksPerHour);
				int m = (int)((ticks%m_TicksPerDay%m_TicksPerHour)/m_TicksPerMinute);
				int s = (int)((ticks%m_TicksPerDay%m_TicksPerHour%m_TicksPerMinute)
                    /m_TicksPerSecond);
				int m_s = (int)((ticks%m_TicksPerDay%m_TicksPerHour%m_TicksPerMinute
                    %m_TicksPerSecond)/m_TicksPerMillisecond);
				
				testResult &= (nts.Days == (neg? -d: +d));
				Log.Comment( nts.Days.ToString() + " == " + (neg?-d:+d).ToString() );

				testResult &= (nts.Hours == (neg? -h: +h));
				Log.Comment( nts.Hours.ToString() + " == " + ((neg? -h: +h)).ToString() );

				testResult &= (nts.Minutes == (neg? -m: +m));
				Log.Comment( nts.Minutes.ToString() + " == " + ((neg? -m: +m)).ToString() );

				testResult &= (nts.Seconds == (neg? -s: +s));
				Log.Comment( nts.Seconds.ToString() + " == " + ((neg? -s: +s)).ToString() );
				
				testResult &= (nts.Milliseconds == (neg? -m_s: +m_s));
				Log.Comment( nts.Milliseconds.ToString() + " == " +((neg? -m_s: +m_s)).ToString());

				Log.Comment( ts.ToString() );
				Log.Comment( nts.ToString() );
			}
			catch
			{
				Log.Comment( "Exception occurred" );
				testResult = false;
			}
			Log.Comment( (testResult?"PASS":"FAIL") );
			return testResult;
		}
		[TestMethod]
		public MFTestResults op_UnaryNegation_Test15(  )
        {
            /// <summary>
            /// 1. Test negation, the unary - operator
            /// </summary>
            ///
            Log.Comment("Testing the unary - operator");
			bool testResult = true;
			Random random = new Random();
			int mxd = 24000;
			int mx = 10000;
			for( int i=0; i<5; i++ )
			{
				int id = (random.Next(1)==0? -1: 1);
				int ih = (random.Next(1)==0? -1: 1);
				int im = (random.Next(1)==0? -1: 1);
				int ise = (random.Next(1)==0? -1: 1);
				int ims = (random.Next(1)==0? -1: 1);
				testResult &= UnaryHelper( id*random.Next(mxd),
                    ih*random.Next(mx),im*random.Next(mx),
                    ise*random.Next(mx),ims*random.Next(mx), true );
			}

			return (testResult? MFTestResults.Pass: MFTestResults.Fail);
		}
		
		private bool OSubAddHelper( int days1, int hours1, int mins1, int secs1,
            int ms1, int days2, int hours2, int mins2, int secs2, int ms2, bool add )
		{
			bool testResult = true;
			try
			{
				long ticks1 = 
					m_TicksPerDay*days1 + 
					m_TicksPerHour*hours1 + 
					m_TicksPerMinute*mins1 + 
					m_TicksPerSecond*secs1 + 
					m_TicksPerMillisecond*ms1;
				long ticks2 = 
					m_TicksPerDay*days2 + 
					m_TicksPerHour*hours2 + 
					m_TicksPerMinute*mins2 + 
					m_TicksPerSecond*secs2 + 
					m_TicksPerMillisecond*ms2;
				long ticks3 = (add? (ticks1 + ticks2): (ticks1 - ticks2));

				int d1 = (int)(ticks3/m_TicksPerDay);
				int h1 = (int)((ticks3%m_TicksPerDay)/m_TicksPerHour);
				int m1 = (int)((ticks3%m_TicksPerDay%m_TicksPerHour)/m_TicksPerMinute);
				int s1 = (int)((ticks3%m_TicksPerDay%m_TicksPerHour%m_TicksPerMinute)
                    /m_TicksPerSecond);
				int m_s1 = (int)((ticks3%m_TicksPerDay%m_TicksPerHour%m_TicksPerMinute
                    %m_TicksPerSecond)/m_TicksPerMillisecond);

				TimeSpan ts1 = new TimeSpan( days1, hours1, mins1, secs1, ms1 );
				TimeSpan ts2 = new TimeSpan( days2, hours2, mins2, secs2, ms2 );
				TimeSpan ts3 = (add? (ts1 + ts2): (ts1 - ts2));

				testResult &= (ts3.Days == d1);
				Log.Comment( ts3.Days.ToString() + " == " + d1.ToString() );

				testResult &= (ts3.Hours == h1);
				Log.Comment( ts3.Hours.ToString() + " == " + h1.ToString() );

				testResult &= (ts3.Minutes == m1);
				Log.Comment( ts3.Minutes.ToString() + " == " + m1.ToString() );

				testResult &= (ts3.Seconds == s1);
				Log.Comment( ts3.Seconds.ToString() + " == " + s1.ToString() );
				
				testResult &= (ts3.Milliseconds == m_s1);
				Log.Comment( ts3.Milliseconds.ToString() + " == " + m_s1.ToString() );

				Log.Comment( ts1.ToString() );
				Log.Comment( ts2.ToString() );
				Log.Comment( ts3.ToString() );
			}
			catch
			{
				Log.Comment( "Exception caught" );
				testResult = false;
			}
			Log.Comment( (testResult?"PASS":"FAIL") );
			Log.Comment("");
			return testResult;
		}
		[TestMethod]
		public MFTestResults op_Subtraction_Test16(  )
        {            
            /// <summary>
            /// 1. Test subtraction, the binary - operator with non TimeSpan args
            /// </summary>
            ///
            Log.Comment("Testing the binary - operator with non TimeSpan args");
			bool testResult = true;
			Random random = new Random();
			int mxd = 24000;
			int mx = 10000;
			for( int i = 0; i<5; i++ )
			{
				int id = (random.Next(1)==0? -1: 1);
				int ih = (random.Next(1)==0? -1: 1);
				int im = (random.Next(1)==0? -1: 1);
				int ise = (random.Next(1)==0? -1: 1);
				int ims = (random.Next(1)==0? -1: 1);																	
				testResult &= OSubAddHelper( id*random.Next(mxd),
                    ih*random.Next(mx),im*random.Next(mx),
                    ise*random.Next(mx),ims*random.Next(mx),
					id*random.Next(mxd),ih*random.Next(mx),
                    im*random.Next(mx),ise*random.Next(mx),
                    ims*random.Next(mx), false );
			}

			return (testResult? MFTestResults.Pass: MFTestResults.Fail);
		}
		[TestMethod]
		public MFTestResults op_UnaryPlus_Test17(  )
		{
            /// <summary>
            /// 1. Test the unary + operator
            /// </summary>
            ///
            Log.Comment("Testing the unary + operator");
			bool testResult = true;
			Random random = new Random();
			int mxd = 24000;
			int mx = 10000;
			for( int i=0; i<5; i++ )
			{
				int id = (random.Next(1)==0? -1: 1);
				int ih = (random.Next(1)==0? -1: 1);
				int im = (random.Next(1)==0? -1: 1);
				int ise = (random.Next(1)==0? -1: 1);
				int ims = (random.Next(1)==0? -1: 1);
				testResult &= UnaryHelper( id*random.Next(mxd),
                    ih*random.Next(mx),im*random.Next(mx),
                    ise*random.Next(mx),ims*random.Next(mx), 
                    false );
			}
			return (testResult? MFTestResults.Pass: MFTestResults.Fail);
		}
		[TestMethod]
		public MFTestResults op_Addition_Test18(  )
		{
            /// <summary>
            /// 1. Test the binary + operator with non-TimeSpan args
            /// </summary>
            ///
            Log.Comment("Testing the binary + operator with non-TimeSpan args");
			bool testResult = true;
			Random random = new Random();
			int mxd = 24000;
			int mx = 10000;
			for( int i = 0; i<5; i++ )
			{
				int id = (random.Next(1)==0? -1: 1);
				int ih = (random.Next(1)==0? -1: 1);
				int im = (random.Next(1)==0? -1: 1);
				int ise = (random.Next(1)==0? -1: 1);
				int ims = (random.Next(1)==0? -1: 1);																	
				testResult &= OSubAddHelper( id*random.Next(mxd),
                    ih*random.Next(mx),im*random.Next(mx),
                    ise*random.Next(mx),ims*random.Next(mx),
					id*random.Next(mxd),ih*random.Next(mx),
                    im*random.Next(mx),ise*random.Next(mx),
                    ims*random.Next(mx), true );
			}
			return (testResult? MFTestResults.Pass: MFTestResults.Fail);
		}
		private bool EqualityHelper( int days1, int hours1, int mins1, int secs1, int ms1, 
            int days2, int hours2, int mins2, int secs2, int ms2, bool expected, bool ineq )
		{
			bool testResult = true;
			try
			{
				TimeSpan ts1 = new TimeSpan( days1, hours1, mins1, secs1, ms1 );
				TimeSpan ts2 = new TimeSpan( days2, hours2, mins2, secs2, ms2 );
				testResult = ((ts1==ts2) == expected);
			}
			catch
			{
				Log.Comment( "Exception Caught" );
				testResult = false;
			}
			return testResult;
		}
		[TestMethod]
		public MFTestResults op_Equality_Test19(  )
        {
            /// <summary>
            /// 1. Test  equality, the binary == operator 
            /// </summary>
            ///
            Log.Comment("Testing the == operator");
			bool testResult = true;
			Random random = new Random();
			int mxd = 24000;
			int mx = 10000;
			for( int i = 0; i<5; i++ )
			{
				int id = (random.Next(1)==0? -1: 1)*random.Next(mxd);
				int ih = (random.Next(1)==0? -1: 1)*random.Next(mx);
				int im = (random.Next(1)==0? -1: 1)*random.Next(mx);
				int ise = (random.Next(1)==0? -1: 1)*random.Next(mx);
				int ims = (random.Next(1)==0? -1: 1)*random.Next(mx);																	
				testResult &= EqualityHelper( id, ih, im, ise, ims, id, ih, im, ise, ims, 
                                            true, false );
				testResult &= EqualityHelper( id, ih, im, ise, ims, id+1, ih, im, ise, ims, 
                                            false, false );
				testResult &= EqualityHelper( id, ih, im, ise, ims, id, ih+1, im, ise, ims, 
                                            false, false );
				testResult &= EqualityHelper( id, ih, im, ise, ims, id, ih, im+1, ise, ims, 
                                            false, false );
				testResult &= EqualityHelper( id, ih, im, ise, ims, id, ih, im, ise+1, ims, 
                                            false, false );
				testResult &= EqualityHelper( id, ih, im, ise, ims, id, ih, im, ise, ims+1, 
                                            false, false );

			}
			return (testResult? MFTestResults.Pass: MFTestResults.Fail);
		}
        private bool InequalityHelper(int days1, int hours1, int mins1, int secs1, int ms1, int days2, int hours2, int mins2, int secs2, int ms2, bool expected, bool ineq)
        {
            bool testResult = true;
            try
            {
                TimeSpan ts1 = new TimeSpan(days1, hours1, mins1, secs1, ms1);
                TimeSpan ts2 = new TimeSpan(days2, hours2, mins2, secs2, ms2);
                testResult = ((ts1 != ts2) == expected);
            }
            catch
            {
                Log.Comment("Exception Caught");
                testResult = false;
            }
            return testResult;
        }
		[TestMethod]
		public MFTestResults op_Inequality_Test20(  )
		{
            /// <summary>
            /// 1. Test  inequality, the binary != operator 
            /// </summary>
            ///
            Log.Comment("Testing the != operator");
			bool testResult = true;
			Random random = new Random();
			int mxd = 24000;
			int mx = 10000;
			for( int i = 0; i<5; i++ )
			{
				int id = (random.Next(1)==0? -1: 1)*random.Next(mxd);
				int ih = (random.Next(1)==0? -1: 1)*random.Next(mx);
				int im = (random.Next(1)==0? -1: 1)*random.Next(mx);
				int ise = (random.Next(1)==0? -1: 1)*random.Next(mx);
				int ims = (random.Next(1)==0? -1: 1)*random.Next(mx);																	
				testResult &= InequalityHelper( id, ih, im, ise, ims, id, ih, im, ise, ims, 
                                                false, false );
                testResult &= InequalityHelper(id, ih, im, ise, ims, id + 1, ih, im, ise, ims, 
                                                true, false);
                testResult &= InequalityHelper(id, ih, im, ise, ims, id, ih + 1, im, ise, ims, 
                                                true, false);
                testResult &= InequalityHelper(id, ih, im, ise, ims, id, ih, im + 1, ise, ims, 
                                                true, false);
                testResult &= InequalityHelper(id, ih, im, ise, ims, id, ih, im, ise + 1, ims, 
                                                true, false);
                testResult &= InequalityHelper(id, ih, im, ise, ims, id, ih, im, ise, ims + 1, 
                                                true, false);
			}
			return (testResult? MFTestResults.Pass: MFTestResults.Fail);
		}
		
		private bool LTGTHelper( int days1, int hours1, int mins1, int secs1, int ms1, int days2, int hours2, int mins2, int secs2, int ms2, bool expected, int opcode )
		{
			bool testResult = true;
			try
			{
				TimeSpan ts1 = new TimeSpan( days1, hours1, mins1, secs1, ms1 );
				TimeSpan ts2 = new TimeSpan( days2, hours2, mins2, secs2, ms2 );
				if( opcode == 0 )
					testResult &= ((ts1 < ts2)==expected);
				else if( opcode == 1 )
					testResult &= ((ts1 <= ts2)==expected);
				else if( opcode == 2 )
					testResult &= ((ts1 > ts2)==expected);
				else if( opcode == 3 )
					testResult &= ((ts1 >= ts2)==expected);
				else
				{
					Log.Comment( "Test Error: Invalid opcode" );
					testResult = false;
				}
			}
			catch
			{
				Log.Comment( "Exception Caught" );
				testResult = false;
			}
			return testResult;
		}

		[TestMethod]
		public MFTestResults op_LessThan_Test21(  )
        {
            /// <summary>
            /// 1. Testing the binary Less Than operator
            /// </summary>
            ///
            Log.Comment("Testing the Less Than operator");
			bool testResult = true;
			Random random = new Random();
			int mxd = 24000;
			int mx = 10000;
			for( int i = 0; i<5; i++ )
			{
				int id = (random.Next(1)==0? -1: 1)*random.Next(mxd);
				int ih = (random.Next(1)==0? -1: 1)*random.Next(mx);
				int im = (random.Next(1)==0? -1: 1)*random.Next(mx);
				int ise = (random.Next(1)==0? -1: 1)*random.Next(mx);
				int ims = (random.Next(1)==0? -1: 1)*random.Next(mx);																	
				testResult &= LTGTHelper( id, ih, im, ise, ims, id, ih, im, ise, ims, false, 0 );
				testResult &= LTGTHelper( id, ih, im, ise, ims, id+1, ih, im, ise, ims, true, 0 );
				testResult &= LTGTHelper( id, ih, im, ise, ims, id, ih+1, im, ise, ims, true, 0 );
				testResult &= LTGTHelper( id, ih, im, ise, ims, id, ih, im+1, ise, ims, true, 0 );
				testResult &= LTGTHelper( id, ih, im, ise, ims, id, ih, im, ise+1, ims, true, 0 );
				testResult &= LTGTHelper( id, ih, im, ise, ims, id, ih, im, ise, ims+1, true, 0 );
				testResult &= LTGTHelper( id+1, ih, im, ise, ims, id, ih, im, ise, ims, false, 0 );
				testResult &= LTGTHelper( id, ih+1, im, ise, ims, id, ih, im, ise, ims, false, 0 );
				testResult &= LTGTHelper( id, ih, im+1, ise, ims, id, ih, im, ise, ims, false, 0 );
				testResult &= LTGTHelper( id, ih, im, ise+1, ims, id, ih, im, ise, ims, false, 0 );
				testResult &= LTGTHelper( id, ih, im, ise, ims+1, id, ih, im, ise, ims, false, 0 );
			}

			return (testResult? MFTestResults.Pass: MFTestResults.Fail);
		}
		[TestMethod]
		public MFTestResults op_LessThanOrEqual_Test22(  )
        {            
            /// <summary>
            /// 1. Test the binary Less Than Equals operator 
            /// </summary>
            ///
            Log.Comment("Testing the Less Than Equals operator");
			bool testResult = true;
			Random random = new Random();
			int mxd = 24000;
			int mx = 10000;
			for( int i = 0; i<5; i++ )
			{
				int id = (random.Next(1)==0? -1: 1)*random.Next(mxd);
				int ih = (random.Next(1)==0? -1: 1)*random.Next(mx);
				int im = (random.Next(1)==0? -1: 1)*random.Next(mx);
				int ise = (random.Next(1)==0? -1: 1)*random.Next(mx);
				int ims = (random.Next(1)==0? -1: 1)*random.Next(mx);																	
				testResult &= LTGTHelper( id, ih, im, ise, ims, id, ih, im, ise, ims, true, 1 );
				testResult &= LTGTHelper( id, ih, im, ise, ims, id+1, ih, im, ise, ims, true, 1 );
				testResult &= LTGTHelper( id, ih, im, ise, ims, id, ih+1, im, ise, ims, true, 1 );
				testResult &= LTGTHelper( id, ih, im, ise, ims, id, ih, im+1, ise, ims, true, 1 );
				testResult &= LTGTHelper( id, ih, im, ise, ims, id, ih, im, ise+1, ims, true, 1 );
				testResult &= LTGTHelper( id, ih, im, ise, ims, id, ih, im, ise, ims+1, true, 1 );
				testResult &= LTGTHelper( id+1, ih, im, ise, ims, id, ih, im, ise, ims, false, 1 );
				testResult &= LTGTHelper( id, ih+1, im, ise, ims, id, ih, im, ise, ims, false, 1 );
				testResult &= LTGTHelper( id, ih, im+1, ise, ims, id, ih, im, ise, ims, false, 1 );
				testResult &= LTGTHelper( id, ih, im, ise+1, ims, id, ih, im, ise, ims, false, 1 );
				testResult &= LTGTHelper( id, ih, im, ise, ims+1, id, ih, im, ise, ims, false, 1 );
			}
			return (testResult? MFTestResults.Pass: MFTestResults.Fail);
		}
		[TestMethod]
		public MFTestResults op_GreaterThan_Test23(  )
        {
            /// <summary>
            /// 1. Test the binary Greater Than operator 
            /// </summary>
            ///
            Log.Comment("Testing the Greater Than operator");
			bool testResult = true;
			Random random = new Random();
			int mxd = 24000;
			int mx = 10000;
			for( int i = 0; i<5; i++ )
			{
				int id = (random.Next(1)==0? -1: 1)*random.Next(mxd);
				int ih = (random.Next(1)==0? -1: 1)*random.Next(mx);
				int im = (random.Next(1)==0? -1: 1)*random.Next(mx);
				int ise = (random.Next(1)==0? -1: 1)*random.Next(mx);
				int ims = (random.Next(1)==0? -1: 1)*random.Next(mx);																	
				testResult &= LTGTHelper( id, ih, im, ise, ims, id, ih, im, ise, ims, false, 2 );
				testResult &= LTGTHelper( id, ih, im, ise, ims, id+1, ih, im, ise, ims, false, 2 );
				testResult &= LTGTHelper( id, ih, im, ise, ims, id, ih+1, im, ise, ims, false, 2 );
				testResult &= LTGTHelper( id, ih, im, ise, ims, id, ih, im+1, ise, ims, false, 2 );
				testResult &= LTGTHelper( id, ih, im, ise, ims, id, ih, im, ise+1, ims, false, 2 );
				testResult &= LTGTHelper( id, ih, im, ise, ims, id, ih, im, ise, ims+1, false, 2 );
				testResult &= LTGTHelper( id+1, ih, im, ise, ims, id, ih, im, ise, ims, true, 2 );
				testResult &= LTGTHelper( id, ih+1, im, ise, ims, id, ih, im, ise, ims, true, 2 );
				testResult &= LTGTHelper( id, ih, im+1, ise, ims, id, ih, im, ise, ims, true, 2 );
				testResult &= LTGTHelper( id, ih, im, ise+1, ims, id, ih, im, ise, ims, true, 2 );
				testResult &= LTGTHelper( id, ih, im, ise, ims+1, id, ih, im, ise, ims, true, 2 );
			}
			return (testResult? MFTestResults.Pass: MFTestResults.Fail);
		}
		[TestMethod]
		public MFTestResults op_GreaterThanOrEqual_Test24(  )
        {
            /// <summary>
            /// 1. Test the binary Greater Than Equals operator 
            /// </summary>
            ///
            Log.Comment("Testing the Greater Than Equals operator");
			bool testResult = true;
            Random random = new Random();
			int mxd = 24000;
			int mx = 10000;
			for( int i = 0; i<5; i++ )
			{
				int id = (random.Next(1)==0? -1: 1)*random.Next(mxd);
				int ih = (random.Next(1)==0? -1: 1)*random.Next(mx);
				int im = (random.Next(1)==0? -1: 1)*random.Next(mx);
				int ise = (random.Next(1)==0? -1: 1)*random.Next(mx);
				int ims = (random.Next(1)==0? -1: 1)*random.Next(mx);																	
				testResult &= LTGTHelper( id, ih, im, ise, ims, id, ih, im, ise, ims, true, 3 );
				testResult &= LTGTHelper( id, ih, im, ise, ims, id+1, ih, im, ise, ims, false, 3 );
				testResult &= LTGTHelper( id, ih, im, ise, ims, id, ih+1, im, ise, ims, false, 3 );
				testResult &= LTGTHelper( id, ih, im, ise, ims, id, ih, im+1, ise, ims, false, 3 );
				testResult &= LTGTHelper( id, ih, im, ise, ims, id, ih, im, ise+1, ims, false, 3 );
				testResult &= LTGTHelper( id, ih, im, ise, ims, id, ih, im, ise, ims+1, false, 3 );
				testResult &= LTGTHelper( id+1, ih, im, ise, ims, id, ih, im, ise, ims, true, 3 );
				testResult &= LTGTHelper( id, ih+1, im, ise, ims, id, ih, im, ise, ims, true, 3 );
				testResult &= LTGTHelper( id, ih, im+1, ise, ims, id, ih, im, ise, ims, true, 3 );
				testResult &= LTGTHelper( id, ih, im, ise+1, ims, id, ih, im, ise, ims, true, 3 );
				testResult &= LTGTHelper( id, ih, im, ise, ims+1, id, ih, im, ise, ims, true, 3 );
			}
			return (testResult? MFTestResults.Pass: MFTestResults.Fail);
		}
		

		private bool TicksHelper( int days, int hours, int mins, int secs, int ms )
		{
			bool testResult = true;
			try
			{
				TimeSpan ts = new TimeSpan( days, hours, mins, secs, ms );
				long ticks = 
					days * m_TicksPerDay +
					hours * m_TicksPerHour + 
					mins * m_TicksPerMinute +
					secs * m_TicksPerSecond +
					ms * m_TicksPerMillisecond;
				testResult &= (ts.Ticks == ticks);
				Log.Comment( ts.Ticks.ToString() + " == " + ticks.ToString() );
			}
			catch
			{
				testResult = false;
				Log.Comment( "Exception caught" );
			}
			Log.Comment( (testResult?"PASS":"FAIL") );
			return testResult;
		}
		[TestMethod]
		public MFTestResults Ticks_Test28(  )
		{
            /// <summary>
            /// 1. Test the Ticks Property
            /// </summary>
            ///
            Log.Comment("Testing the Ticks Property");
			bool testResult = true;
			Log.Comment( "TimeSpan::Ticks - Normal ");
            Random random = new Random();
			int max = 24000;
			int maxOthers = 10000;
			for( int i=0; i<5; i++ )
			{
				testResult &= TicksHelper( random.Next(max), 
                    random.Next(maxOthers),random.Next(maxOthers), 
                    random.Next(maxOthers), random.Next(maxOthers));
			}
			return (testResult? MFTestResults.Pass: MFTestResults.Fail);
		}
    
    
    }
}
