////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT.Platform.Test;
using System.Threading;

namespace Microsoft.SPOT.Platform.Tests
{
    public class Master_ProfilerThreadTests
    {
        bool sleepZero = true;
        object lockObject = new object();
        AutoResetEvent signalProducer = new AutoResetEvent(false);
        AutoResetEvent signalConsumer = new AutoResetEvent(false);
        int timerCount = 0;

        //--//

        public static void Main()
        {
            Master_ProfilerThreadTests tests = new Master_ProfilerThreadTests();
            tests.SleepTest();
            tests.MonitorTest();
            tests.TimerTest();
        }

        //--//
        
        private void SleepZero()
        {
            while (sleepZero);
        }

        public void SleepTest()
        {
            Thread newThread = new Thread(new ThreadStart(this.SleepZero));
            newThread.Start();
            
            Thread.Sleep(0);
            Thread.Sleep(1);
            Thread.Sleep(10);
            Thread.Sleep(100);

            this.sleepZero = false;

            newThread.Join();
        }

        //--//

        private void Producer()
        {
            System.Threading.Monitor.Enter(this.lockObject);
            this.signalConsumer.Set();
            System.Threading.Monitor.Exit(this.lockObject);
            this.signalProducer.WaitOne();
            
            // repeat
            System.Threading.Monitor.Enter(this.lockObject);
            this.signalConsumer.Set();
            System.Threading.Monitor.Exit(this.lockObject);
            this.signalProducer.WaitOne();
        }

        public void MonitorTest()
        {
            Thread newThread = new Thread(new ThreadStart(this.Producer));
            newThread.Start();

            System.Threading.Monitor.Enter(this.lockObject);
            this.signalProducer.Set();
            System.Threading.Monitor.Exit(this.lockObject);
            this.signalConsumer.WaitOne();

            // repeat
            System.Threading.Monitor.Enter(this.lockObject);
            this.signalProducer.Set();
            System.Threading.Monitor.Exit(this.lockObject);
            this.signalConsumer.WaitOne();
            
            newThread.Join();
        }

        //--//

        private void TimerCallback(object context)
        {
            this.timerCount += 1;
        }

        public void TimerTest()
        {
            Timer timer = new Timer(new TimerCallback(this.TimerCallback), null, 1000, 1000);

            while (this.timerCount < 5) ;

            timer.Dispose();
        }
    }
}