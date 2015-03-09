////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

using Microsoft.SPOT;
using Microsoft.SPOT.TestFramework;

namespace Microsoft.SPOT.Test.Template
{
    public class SampleTest :            Microsoft.SPOT.TestFramework.Test
    {
        public SampleTest( string comment ):base( comment ){}

        public override void Run()
        {
            try
            {
                int a = 2;
                int b = 2;
                if( a == b )
                    Pass = true;
            }
            catch(Exception e)
            {
                UnexpectedException( e );
            }
        }
    }
    public class SampleTest2 :            Microsoft.SPOT.TestFramework.Test
    {
        public SampleTest2( string comment ):base( comment ){}

        public override void Run()
        {
            try
            {
                int a = 2;
                int b = 3;
                if( a == b )
                    Pass = true;
            }
            catch(Exception e)
            {
                UnexpectedException( e );
            }
        }
    }


	public class MainEntryPoint
	{
		public static void Main()
		{
			TestSuite suite = new TestSuite();

            suite.RunTest( new SampleTest         ("Verify 2==2") );
            suite.RunTest( new SampleTest2        ("Verify 2==3 results in Failed Test") );

			suite.Finished();
		}
	}
}
