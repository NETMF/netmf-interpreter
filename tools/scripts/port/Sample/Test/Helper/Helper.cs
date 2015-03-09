////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Threading;

namespace Microsoft.SPOT.TestFramework
{
    
    public abstract class Test 
    {
        public string Name;
		public bool   Pass;
        
        protected Test( string comment )
        {
            Name    = this.ToString(); 
			Pass    = false;

			Debug.Print( "Test        : " + Name                                      );
			Debug.Print( "Test Comment: " + comment                                   );
        }
        protected Test()
        {
			Name    = this.ToString(); 
			Pass    = false;

			Debug.Print( "Test        : " + Name                                      );
			Debug.Print( "Test Comment: N/A"                                          );
		}
		public void Finished()
		{
			Debug.Print( "Test " +(Pass==true?"Passed":"Failed") );
			Debug.Print("==================================================================");
		}        

		public abstract void Run();
		
		protected internal void UnexpectedException( Exception e )
        {
            Pass = false;
			Debug.Print( "Test        : " + Name   + " Unexpected Exception occurred." );                                          
            Debug.Print( e.Message                                                    );
            Debug.Print( "Exception Stack Trace: "                                    );
            Debug.Print( e.StackTrace                                                 );   
        }

        protected internal void UnexpectedBehavior()
        {
			Pass = false;
            Debug.Print( "Test        : " + Name   + " Unexpected Behavior."            );
		}
    }

    public class TestSuite
    {
		private int testsPassed;
		private int testsTotal;

        public TestSuite()
        {
			Debug.Print("==================================================================");
			testsPassed = 0;
			testsTotal = 0;
        }
		public void Finished()
		{
			Debug.Print("Total Tests Passed =   " + testsPassed);
			Debug.Print("Total Tests Failed =   " + (testsTotal - testsPassed));
			Debug.Print("Total Tests Executed = " + testsTotal );
			Debug.Print("==================================================================");
		}

		public void RunTest( Test testToRun )
		{ 
			testsTotal++;
			try
			{
				testToRun.Run();
			}
			catch(Exception e)
			{
				testToRun.Pass = false;
				Debug.Print( "Unexpected Exception" + e.StackTrace );    
			}
			if( testToRun.Pass )
				++testsPassed;

			testToRun.Finished();
		}
    }
}
