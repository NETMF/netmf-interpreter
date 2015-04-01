using System;
using System.Threading;
using Microsoft.SPOT;

namespace Threading
{
    /// <summary>
    /// Demonstrates various threading functions of the .NET Micro Framework.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The execution entry point.
        /// </summary>
        public static void Main()
        {
            // Demonstrates how to start a new thread and wait for it to complete.
            ThreadJoinDemo();

            // Demonstrates how to start a timer to call your method peridically 
            // using a thread from the thread pool.
            TimerDemo();

            // Demonstrates how to create an event and have a thread wait for 
            // another thread to signal it.
            EventDemo();
        }

        /// <summary>
        /// Demonstrates how to start a new thread and wait for it to complete.
        /// </summary>
        private static void ThreadJoinDemo()
        {
            Debug.Print("Thread join demo");

            // Create a new thread and start it executing some code (in 
            // ThreadJoinMethod).
            Thread threadJoinMethodThread = new Thread(ThreadJoinMethod);
            threadJoinMethodThread.Start();

            // Have this thread wait until the new thread has terminated or until 
            // 100 milliseconds have elapsed (whichever happens first).
            while (!threadJoinMethodThread.Join(100))
            {
                // If the thread hasn't terminated in 100ms, then show 
                // that this thread is still waiting, and loop around.
                Debug.Print("   Waiting for thread to exit");
            }

            // The new thread has terminated.
            Debug.Print("Thread exited.");
        }

        /// <summary>
        /// Performs any processing that is needed when the thread is joined.
        /// </summary>
        private static void ThreadJoinMethod()
        {
            // The thread executing this method simulates doing work by sleeping.
            Thread.Sleep(500);
        }

        /// <summary>
        /// Demonstrates how to start a timer to call your method peridically using 
        /// a thread from the thread pool.
        /// </summary>
        private static void TimerDemo()
        {
            Debug.Print("Timer demo");

            // Create a Timer, which tells the thread pool to call TimerDemoMethod 
            // every 200ms.  The first call happens immediately.
            // The second parameter is the object state (none in this case).
            using (new Timer(TimerDemoMethod, null, 0, 200))
            {
                // Simulate doing other work here by sleeping.
                Thread.Sleep(1000);
            }

            // The Timer object is automatically disposed and stopped.
        }

        /// <summary>
        /// Demonstrates the timer.
        /// </summary>
        /// <param name="o"></param>
        private static void TimerDemoMethod(Object o)
        {
            // A Timer calls this method every 200ms; show the date and time of each 
            // call.
            Debug.Print("In TimerDemoMethod: " + DateTime.Now);
        }

        // Signaled by the DoSomething method.
        private static AutoResetEvent s_doSomethingThreadDone = new AutoResetEvent(false);

        /// <summary>
        /// Demonstrates how to create an event and have a thread wait for another 
        /// thread to signal it.
        /// </summary>
        private static void EventDemo()
        {
            Debug.Print("Event demo");

            // Create and start another thread which does something.
            Thread doSomethingThread = new Thread(DoSomething);
            doSomethingThread.Start();

            // Wait for the other thread to signal us.
            s_doSomethingThreadDone.WaitOne();

            Debug.Print("Signal received from DoSomething thread");
        }

        /// <summary>
        /// Demonstrates processing on a thread.
        /// </summary>
        private static void DoSomething()
        {
            // This thread does something for a little while.
            for (int i = 0; i < 5; i++)
            {
                Debug.Print("   " + i.ToString());
                Thread.Sleep(100);
            }

            // Signal the event when done, to wake the thread in the EventDemo 
            // method.
            s_doSomethingThreadDone.Set();
        }
    }
}
