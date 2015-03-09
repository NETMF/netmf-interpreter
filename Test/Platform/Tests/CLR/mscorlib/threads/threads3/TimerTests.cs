////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.Reflection;
using System.Threading;

namespace Microsoft.SPOT.Platform.Tests
{
    public class TimerTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
            {
                Log.Comment("We may only need to run Time Dependent Tests on a Device");

                return InitializeResult.ReadyToGo;
            }
            Log.Comment("Adding set up for the tests");

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests");
            Thread.Sleep(1000);
        }

        class StatusChecker
        {

            public bool m_result = false;
            public bool z_flag = false;
            public bool c_flag = false;
            public bool m_flag = false;
            public int counter = 0;
            public DateTime t2;
            public DateTime t1;
            public int m_invokeCount, m_maxCount;
            public StatusChecker(int count)
            {
                m_invokeCount = 0;
                m_maxCount = count;
            }

            // This method is called by the timer delegate.
            public void CheckStatus(Object stateInfo)
            {
                if (z_flag)
                {
                    t2 = DateTime.Now;
                    z_flag = false;
                }
                if (m_flag)
                {
                    t1 = DateTime.Now;
                    z_flag = true;
                    m_flag = false;
                }
                if (c_flag)
                {
                    counter++;
                }
                AutoResetEvent autoEvent = (AutoResetEvent)stateInfo;
                Log.Comment("Checking status: " +
                    DateTime.Now.ToString() + " " +
                    (++m_invokeCount).ToString());

                if (m_invokeCount == m_maxCount)
                {
                    Log.Comment("Reset the counter and signal Main.");
                    m_invokeCount = 0;
                    autoEvent.Set();
                    m_result = true;
                }
            }
        }

        [TestMethod]
        public MFTestResults Timer_TimerCallback_Test1()
        {
            /// <summary>
            /// 1. Creates a TimerCallback
            /// 2. Creates a Timer that calls it at 1 sec and every quarter sec thereafter
            /// 3. Verifies that the TimerCallback gets called
            /// 4. Change period to half sec
            /// 5. Disposes of the timer after it signals
            /// 6. Verifies that the TimerCallback has been called
            /// </summary>
            ///
            Log.Comment("Tests the Timer and TimerCallback classes, ");
            Log.Comment("as well as the Change and Dispose methods");
            MFTestResults testResult = MFTestResults.Pass;
            AutoResetEvent autoEvent = new AutoResetEvent(false);
            StatusChecker statusChecker = new StatusChecker(15);

            Log.Comment("Creating the TimerCallback");
            TimerCallback timerDelegate =
                new TimerCallback(statusChecker.CheckStatus);

            Log.Comment("Creating timer: " +
                DateTime.Now.ToString());
            Timer stateTimer =
                    new Timer(timerDelegate, autoEvent, 1000, 250);

            statusChecker.m_result = false;
            Log.Comment("Waiting and verifying");
            autoEvent.WaitOne(5000, false);
            if (!statusChecker.m_result)
            {
                Log.Comment("Failure : expected callback '" + statusChecker.m_maxCount +
                    "' times but got '" + statusChecker.m_invokeCount + "'");
                testResult = MFTestResults.Fail;
            }
            statusChecker.m_result = false;
            Log.Comment("Changing period to 500msec");
            stateTimer.Change(0, 500);
            Log.Comment("Waiting for 7500msec and verifying");
            autoEvent.WaitOne(7500, false);
            stateTimer.Dispose();
            Log.Comment("Destroying timer.");
            if (!statusChecker.m_result)
            {
                Log.Comment("Failure : expected callback '" + statusChecker.m_maxCount +
                    "' times but got '" + statusChecker.m_invokeCount + "'");
                testResult = MFTestResults.Fail;
            }
            return testResult;
        }

        [TestMethod]
        public MFTestResults Timer_TimerCallback_ZeroTest2()
        {
            /// <summary>
            /// 1. Creates a TimerCallback
            /// 2. Creates a Timer that calls it at 1 sec and every quarter sec thereafter
            /// 3. Verifies that the TimerCallback gets called
            /// 4. Change duetime to zero sec
            /// 5. Disposes of the timer after it signals
            /// 6. Verifies that the TimerCallback restarted immediately
            /// </summary>
            ///

            Log.Comment("Tests the Timer and TimerCallback classes, ");
            Log.Comment("as well as the Change and Dispose methods");
            MFTestResults testResult = MFTestResults.Pass;
            AutoResetEvent autoEvent = new AutoResetEvent(false);
            StatusChecker statusChecker = new StatusChecker(15);

            Log.Comment("Creating the TimerCallback");
            TimerCallback timerDelegate =
                new TimerCallback(statusChecker.CheckStatus);

            Log.Comment("Creating timer: " +
                DateTime.Now.ToString());
            Timer stateTimer =
                    new Timer(timerDelegate, autoEvent, 1000, 250);

            statusChecker.m_result = false;
            Log.Comment("Waiting and verifying");
            autoEvent.WaitOne(5000, false);
            if (!statusChecker.m_result)
            {
                Log.Comment("Failure : expected callback '" + statusChecker.m_maxCount +
                    "' times but got '" + statusChecker.m_invokeCount + "'");
                testResult = MFTestResults.Fail;
            }
            statusChecker.m_result = false;
            statusChecker.z_flag = true;
            Log.Comment("Changing duetime to zero and Verifying the timer started Immediately.");
            DateTime t1 = DateTime.Now;
            stateTimer.Change(0, 500);
            Thread.Sleep(1);
            TimeSpan time = DateTime.Now - statusChecker.t2;
            Log.Comment("callback method called within " + time.ToString());
            if (time.CompareTo(new TimeSpan(0, 0, 0, 0, 100)) > 0)
            {
                Log.Comment("The timer didn't start immediately, started after '" + time.ToString() + "'");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Waiting and verifying");
            autoEvent.WaitOne(7500, false);
            Log.Comment("Destroying timer.");
            stateTimer.Dispose();
            if (!statusChecker.m_result)
            {
                Log.Comment("Failure : expected callback '" + statusChecker.m_maxCount +
                    "' times but got '" + statusChecker.m_invokeCount + "'");
                testResult = MFTestResults.Fail;
            }
            return testResult;
        }

        [TestMethod]
        public MFTestResults Timer_Disable_Periodic_Signaling_Test3()
        {
            /// <summary>
            /// 1. Creates a TimerCallback
            /// 2. Creates a Timer that calls it at 1 sec and every quarter sec thereafter
            /// 3. Verifies that the TimerCallback gets called
            /// 4. Changes period to zero (0) 
            /// 5. Verifies periodic signaling is disabled
            /// 6. Changes period to Infinite
            /// 7. Verifies periodic signaling is disabled
            /// 8. Changes period to quarter sec
            /// 9. Disposes the timer after it signals
            /// 10. Verifies that the TimerCallback has been called
            /// </summary>
            ///

            Log.Comment("Tests the Timer and TimerCallback classes, ");
            Log.Comment("as well as the Change and Dispose methods");
            MFTestResults testResult = MFTestResults.Pass;
            AutoResetEvent autoEvent = new AutoResetEvent(false);
            StatusChecker statusChecker = new StatusChecker(15);

            Log.Comment("Creating the TimerCallback");
            TimerCallback timerDelegate =
                new TimerCallback(statusChecker.CheckStatus);

            Log.Comment("Creating timer: " +
                DateTime.Now.ToString());
            Timer stateTimer =
                    new Timer(timerDelegate, autoEvent, 1000, 250);

            Log.Comment("Waiting and verifying");
            autoEvent.WaitOne(5000, false);
            if (!statusChecker.m_result)
            {
                Log.Comment("Failure : expected callback '" + statusChecker.m_maxCount +
                    "' times but got '" + statusChecker.m_invokeCount + "'");
                testResult = MFTestResults.Fail;
            }

            statusChecker.m_result = false;
            statusChecker.c_flag = true;
            Log.Comment("Changing period to zero (0) ");
            stateTimer.Change(0, 0);

            Log.Comment("Waiting and verifying the callback method is invoked once");
            autoEvent.WaitOne(5000, false);
            if (statusChecker.counter != 1)
            {
                Log.Comment("Failure : expected callback '1' times but got '" + statusChecker.counter + "'");
                testResult = MFTestResults.Fail;
            }

            Log.Comment("Reseting counter to zero");
            statusChecker.counter = 0;
            Log.Comment("Changing period to Timeout.Infinite");
            stateTimer.Change(0, Timeout.Infinite);
            Log.Comment("Waiting and verifying the callback method is invoked once");
            autoEvent.WaitOne(5000, false);
            if (statusChecker.counter != 1)
            {
                Log.Comment("Failure : expected callback '1' times but got '" + statusChecker.counter + "'");
                testResult = MFTestResults.Fail;
            }

            Log.Comment("Changing period to quarter sec ");
            stateTimer.Change(0, 250);
            Log.Comment("Waiting and verifying");
            autoEvent.WaitOne(5000, false);
            Log.Comment("Destroying timer.");
            stateTimer.Dispose();
            if (!statusChecker.m_result)
            {
                Log.Comment("Failure : expected callback '" + statusChecker.m_maxCount +
                    "' times but got '" + statusChecker.m_invokeCount + "'");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults Timer_TimerCallback_Duetime_Infinite_Test4()
        {
            /// <summary>
            /// 1. Creates a TimerCallback
            /// 2. Creates a Timer that calls it after infinite time
            /// 3. Verifies the callback method is never invoked
            /// </summary>
            ///

            Log.Comment("Tests the TimerCallback is never invoked if it's duetime is infinite ");
            MFTestResults testResult = MFTestResults.Pass;
            AutoResetEvent autoEvent = new AutoResetEvent(false);
            StatusChecker statusChecker = new StatusChecker(15);
            statusChecker.c_flag = true;
            Log.Comment("Creating the TimerCallback");
            TimerCallback timerDelegate =
                new TimerCallback(statusChecker.CheckStatus);

            Log.Comment("Creating timer: " +
                DateTime.Now.ToString());
            Timer stateTimer =
                    new Timer(timerDelegate, autoEvent, Timeout.Infinite, 250);

            Log.Comment("Waiting and verifying");
            autoEvent.WaitOne(5000, false);
            Log.Comment("Destroying timer.");
            stateTimer.Dispose();
            if (statusChecker.counter != 0)
            {
                Log.Comment("Failure : expected callback  method never invoked" +
                    " but is invoked '" + statusChecker.counter + "' times");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults Timer_Dispose_Test5()
        {
            /// <summary>
            /// 1. Creates a TimerCallback
            /// 2. Creates a Timer that calls it at 1 sec and every quarter sec thereafter
            /// 3. Immediately Disposes the timer
            /// 4. Verifies that the Timer is disposed
            /// </summary>
            ///

            Log.Comment("Tests the Timer and TimerCallback classes, ");
            Log.Comment("as well as the Change and Dispose methods");
            MFTestResults testResult = MFTestResults.Pass;
            AutoResetEvent autoEvent = new AutoResetEvent(false);
            StatusChecker statusChecker = new StatusChecker(15);
            statusChecker.c_flag = true;
            Log.Comment("Creating the TimerCallback");
            TimerCallback timerDelegate =
                new TimerCallback(statusChecker.CheckStatus);

            Log.Comment("Creating timer: " +
                DateTime.Now.ToString());
            Timer stateTimer =
                    new Timer(timerDelegate, autoEvent, 1000, 250);
            Log.Comment("Immediately Destroying timer");
            stateTimer.Dispose();
            Log.Comment("Waiting and verifying for Timer disposed");
            autoEvent.WaitOne(5000, false);
            if (statusChecker.counter != 0)
            {
                Log.Comment("Failure : expected timer destroyed immediately hence callback method" +
                    " never invoked but is invoked '" + statusChecker.counter + "' times");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults Timer_Negative_Period_Test6()
        {

            /// <summary>
            /// 1. Creates a TimerCallback
            /// 2. Creates a Timer that calls it at 1 sec and every quarter sec thereafter
            /// 3. Verifies that the TimerCallback gets called
            /// 4. Changes period to -2 (negative two) sec
            /// 5. Verifies ArgumentOutOfRangeException exception is thrown
            /// </summary>
            ///

            Log.Comment("Tests the Timer and TimerCallback classes, ");
            Log.Comment("as well as the Change and Dispose methods");
            MFTestResults testResult = MFTestResults.Fail;
            AutoResetEvent autoEvent = new AutoResetEvent(false);
            StatusChecker statusChecker = new StatusChecker(15);

            Log.Comment("Creating the TimerCallback");
            TimerCallback timerDelegate =
                new TimerCallback(statusChecker.CheckStatus);

            Log.Comment("Creating timer: " +
                DateTime.Now.ToString());
            Timer stateTimer =
                    new Timer(timerDelegate, autoEvent, 1000, 250);

            statusChecker.m_result = false;
            Log.Comment("Waiting and verifying");
            autoEvent.WaitOne(5000, false);
            statusChecker.m_result = false;
            Log.Comment("Changing period to -ve");
            try
            {
                Log.Comment("period is negative and is not equal to Infinite (or -1) should throw an exception");
                stateTimer.Change(0, -2);
                Log.Comment("Waiting and verifying");
                autoEvent.WaitOne(7500, false);
            }
            catch (ArgumentOutOfRangeException e)
            {
                Log.Comment("Correctly threw exception on negative period " + e.ToString());
                testResult = MFTestResults.Pass;
            }
            finally
            {
                Log.Comment("Destroying timer.");
                stateTimer.Dispose();
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults Timer_Negative_Duetime_Test7()
        {
            /// <summary>
            /// 1. Creates a TimerCallback
            /// 2. Creates a Timer that calls it at 1 sec and every quarter sec thereafter
            /// 3. Verifies that the TimerCallback gets called
            /// 4. Changes duetime to -2 (negative two) sec
            /// 5. Verifies ArgumentOutOfRangeException exception is thrown
            /// </summary>
            ///

            Log.Comment("Tests the Timer Change method for ArgumentOutOfRangeException");
            MFTestResults testResult = MFTestResults.Fail;
            AutoResetEvent autoEvent = new AutoResetEvent(false);
            StatusChecker statusChecker = new StatusChecker(15);

            Log.Comment("Creating the TimerCallback");
            TimerCallback timerDelegate =
                new TimerCallback(statusChecker.CheckStatus);

            Log.Comment("Creating timer: " +
                DateTime.Now.ToString());
            Timer stateTimer =
                    new Timer(timerDelegate, autoEvent, 1000, 250);

            statusChecker.m_result = false;
            Log.Comment("Waiting and verifying");
            autoEvent.WaitOne(5000, false);
            statusChecker.m_result = false;
            Log.Comment("Changing period -ve");
            try
            {
                Log.Comment("duetime is negative and is not equal to Infinite(or -1) should throw an exception");
                stateTimer.Change(-2, 500);
                Log.Comment("Waiting and verifying");
                autoEvent.WaitOne(7500, false);
            }
            catch (ArgumentOutOfRangeException e)
            {
                Log.Comment("Correctly threw exception on negative duetime name " + e.ToString());
                testResult = MFTestResults.Pass;
            }
            finally
            {
                Log.Comment("Destroying timer.");
                stateTimer.Dispose();
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults Timer_TimerCallback_Null_Test8()
        {
            /// <summary>
            /// 1. Creates a Timer with callback parameter null
            /// 2. Verifies ArgumentNullException exception is thrown
            /// </summary>
            ///
            Log.Comment("Tests the Timer for ArgumentNullException");

            MFTestResults testResult = MFTestResults.Fail;
            AutoResetEvent autoEvent = new AutoResetEvent(false);
            StatusChecker statusChecker = new StatusChecker(15);

            Log.Comment("Creating timer: " +
                DateTime.Now.ToString());
            try
            {
                Log.Comment("Passing callback parameter to a timer should throw exception");
                Timer stateTimer = new Timer(null, autoEvent, 1000, 250);
                Log.Comment("Waiting and verifying");
                autoEvent.WaitOne(7500, false);
                Log.Comment("Destroying timer.");
                stateTimer.Dispose();
            }
            catch (Exception e)
            {
                Log.Comment("Correctly threw exception on null callback parameter name " + e.ToString());
                testResult = MFTestResults.Pass;
            }
            return testResult;
        }

        [TestMethod]
        public MFTestResults Timer_MaxInt_DueTime_Test9()
        {
            /// <summary>
            /// 1. Creates a TimerCallback
            /// 2. Creates a Timer that calls it at 1 sec and every quarter sec thereafter
            /// 3. Verifies that the TimerCallback gets called
            /// 4. Changes duetime to MaxInt (in this case MaxInt assumed to be 4 sec)
            /// 5. Disposes of the timer after it signals
            /// 6. Verifies that the TimerCallback starts after 4 sec
            /// </summary>
            ///

            Log.Comment("Tests the Timer and TimerCallback classes, ");
            Log.Comment("as well as the Change and Dispose methods");
            MFTestResults testResult = MFTestResults.Pass;
            AutoResetEvent autoEvent = new AutoResetEvent(false);
            StatusChecker statusChecker = new StatusChecker(15);

            Log.Comment("Creating the TimerCallback");
            TimerCallback timerDelegate =
                new TimerCallback(statusChecker.CheckStatus);

            Log.Comment("Creating timer: " +
                DateTime.Now.ToString());
            Timer stateTimer =
                    new Timer(timerDelegate, autoEvent, 1000, 250);

            statusChecker.m_result = false;
            Log.Comment("Waiting and verifying");
            autoEvent.WaitOne(5000, false);
            if (!statusChecker.m_result)
            {
                Log.Comment("Failure : expected callback '" + statusChecker.m_maxCount +
                    "' times but got '" + statusChecker.m_invokeCount + "'");
                testResult = MFTestResults.Fail;
            }
            statusChecker.m_result = false;
            statusChecker.z_flag = true;
            Log.Comment("Changing duetime to 4 sec (Assumed MaxInt)");
            DateTime t1 = DateTime.Now;
            stateTimer.Change(4000, 250);
            Log.Comment("Waiting and verifying");
            autoEvent.WaitOne(8000, false);
            TimeSpan duration = statusChecker.t2 - t1;
            Log.Comment("Verifying callback method 1st invoked after 4000msec.");
            if (duration.CompareTo(new TimeSpan(4000)) <= 0)
            {
                Log.Comment("Failure : expected 1st callback happens more than" +
                    " '4000msec.' but happened after '" + duration.ToString() + "'");
                testResult = MFTestResults.Fail;
            }          
            if (!statusChecker.m_result)
            {
                Log.Comment("Failure : expected callback '" + statusChecker.m_maxCount +
                    "' times but got '" + statusChecker.m_invokeCount + "'");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Destroying timer.");
            stateTimer.Dispose();

            return testResult;
        }

        [TestMethod]
        public MFTestResults Timer_MaxInt_Period_Test10()
        {
            /// <summary>
            /// 1. Creates a TimerCallback
            /// 2. Creates a Timer that calls it at 1 sec and every quarter sec thereafter
            /// 3. Verifies that the TimerCallback gets called
            /// 4. Changes period to MaxInt (in this case MaxInt assumed to be 4 sec)
            /// 5. Disposes of the timer after it signals
            /// 6. Verifies that the TimerCallback starts every 4 sec
            /// </summary>
            ///

            Log.Comment("Tests the Timer and TimerCallback classes, ");
            Log.Comment("as well as the Change and Dispose methods");
            MFTestResults testResult = MFTestResults.Pass;
            AutoResetEvent autoEvent = new AutoResetEvent(false);
            StatusChecker statusChecker = new StatusChecker(15);

            Log.Comment("Creating the TimerCallback");
            TimerCallback timerDelegate =
                new TimerCallback(statusChecker.CheckStatus);

            Log.Comment("Creating timer: " +
                DateTime.Now.ToString());
            Timer stateTimer =
                    new Timer(timerDelegate, autoEvent, 1000, 250);

            statusChecker.m_result = false;
            Log.Comment("Waiting and verifying");
            autoEvent.WaitOne(5000, false);
            if (!statusChecker.m_result)
            {
                Log.Comment("Failure : expected callback '" + statusChecker.m_maxCount +
                    "' times but got '" + statusChecker.m_invokeCount + "'");
                testResult = MFTestResults.Fail;
            }
            statusChecker.m_result = false;
            statusChecker.m_flag = true;
            Log.Comment("Changing period to 4 sec (Assumed MaxInt)");
            stateTimer.Change(0, 4000);
            Log.Comment("Waiting and verifying callback method is invoked every 4 sec");
            autoEvent.WaitOne(60000, false);
            if (!statusChecker.m_result)
            {
                Log.Comment("Failure : after 60sec. expected callback invoked '" + statusChecker.m_maxCount +
                    "' times but got '" + statusChecker.m_invokeCount + "'");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Verifying time interval b/n callback invoke is atleast 4sec.");
            TimeSpan duration = statusChecker.t2 - statusChecker.t1;
            if(duration.CompareTo(new TimeSpan(4000)) < 0)
            {
                Log.Comment("Failure : expected interval b/n callbacks at least '4sec' but got '"+duration.Seconds.ToString()+"sec'");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Destroying timer.");
            stateTimer.Dispose();           

            return testResult;
        }
    }
}
