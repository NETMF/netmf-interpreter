////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.TestFramework;

namespace Microsoft.SPOT.Test.Timers
{
    public class WaitOne :            Microsoft.SPOT.TestFramework.Test
    {
        AutoResetEvent mre = new AutoResetEvent(false);
        
        public WaitOne( string comment ):base( comment ){}

        public override void Run()
        {
            try
            {
                State state = new State();
                Timer tmr = new Timer( new TimerCallback( Callback ), state, 0, 1000 );

                DateTime start = DateTime.Now;
                mre.WaitOne();
                TimeSpan ts = (DateTime.Now-start);
                Pass = ts.Milliseconds <= 1005;

                Pass &= state.m_cnt == 1;
                mre.Reset();
            }
            catch(Exception e)
            {
                UnexpectedException( e );
            }
        }
    
    
        internal class State
        {
            public uint m_cnt = 0;
            public AutoResetEvent m_are = new AutoResetEvent(false);
        }
       
        private void Callback( object state)
        {
            State st = state as State;
            if( st != null )
            {
                st.m_cnt++;
                st.m_are.Set();
            }
            mre.Set();
        }
    }

    public class MainEntryPoint
    {
        public static void Main()
        {
            TestSuite suite = new TestSuite();

            suite.RunTest( new WaitOne         ("Verify Timer.WaitOne with one second intervals") );

            suite.Finished();
        }
    }
}
