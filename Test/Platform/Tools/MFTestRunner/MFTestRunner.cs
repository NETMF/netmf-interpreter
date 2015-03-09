using System;
using System.Text;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Threading;
using Microsoft.SPOT;

namespace Microsoft.SPOT.Platform.Test
{
    public class MFTestRunner
    {
        /// Thread used to execute cases, can be aborted for timeout
        private Thread m_harnessWorker;
        private Timer m_timeoutTimer;
        private int m_timeOut = 900000; // 15 minutes default time out.
        private object[] m_args;
        private bool m_runTests;
        
        /// <summary>
        /// An overloaded constructor that takes the test objects in its arguments.
        /// </summary>
        /// <param name="args">A list of test objects.</param>
        public MFTestRunner(object[] objects, int timeout) 
            : this(objects)
        {
            m_timeOut = timeout;
        }

        /// <summary>
        /// An overloaded constructor that takes the test objects in its arguments.
        /// </summary>
        /// <param name="args">A list of test objects.</param>
        public MFTestRunner(object[] args)
        {
            if (args.Length == 0)
            {
                throw new ArgumentNullException();
            }

            m_args = args;

            // Do work in separate thread so we can control abort on timeout
            m_harnessWorker = new Thread( new ThreadStart(this.Run) );

            // Create watch dog timer
            TimerCallback timerDelegate = new TimerCallback(this.AbortTests);
            m_timeoutTimer = new Timer(timerDelegate, null, m_timeOut, Timeout.Infinite);

            m_runTests = true;
            m_harnessWorker.Start();

            // wait for work to complete
            m_harnessWorker.Join();
        }

        private void Run()
        {
            while (m_runTests)
            {
                try
                {
                    Run(m_args);
                }
                catch (Exception e)
                {
                    Log.Comment("An exception occurred when running the test: " + e.Message);
                    Log.Exception(e.StackTrace);
                }
                finally
                {
                    // To debug failures between the harness, VS, and the emulator
                    // simply comment out or place a breakpoint on the line below
                    m_runTests = false;
                }
            }
        }

        private void AbortTests(object stateInfo)
        {
            Log.Exception("Tests have exceeded timeout, aborting...");
            m_timeoutTimer.Dispose();

            if (m_runTests)
            {
                Log.Exception("The Worker thread that runs the tests was not stopped properly by the harness...");
                m_runTests = false;
            }

            try
            {
                if(m_harnessWorker != null && m_harnessWorker.IsAlive)
                {
                    if (!m_harnessWorker.Join(5000))
                    {
                        Log.Exception("The Worker thread that runs the tests did not join the main thread after 5 secs");
                        m_harnessWorker.Abort();
                    }
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// This method will execute the tests specified in the argument.
        /// </summary>
        /// <param name="objects">A list of test objects.</param>
        public void Run(object[] objects)
        {
            foreach (string arg in objects)
            {
                // Binding flags for the type of methods in the test assembly you want to get info about.
                BindingFlags bf = BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod;

                // Get all the types that implement the IMFTestInterface.
                Type[] interfaces = Reflection.GetTypesImplementingInterface(typeof(IMFTestInterface));

                // Find the ones that match the list of tests we are trying to run. 
                foreach (Type t in interfaces)
                {
                    if (string.Equals(t.Name.ToLower(), arg.ToLower()))
                    {
                        ConstructorInfo ci = t.GetConstructor(new Type[0]);
                        IMFTestInterface imf = null;
                        bool initializedLog = false;
                        InitializeResult initResult;

                        try
                        {
                            // Invoke the constructor.
                            imf = (IMFTestInterface)ci.Invoke(new object[0]);
                            
                            // Call Initialize.
                            Log.Initialize(t.Name);
                            initializedLog = true;
                            initResult = imf.Initialize();
                        }
                        catch (NotSupportedException)
                        {
                            initResult = InitializeResult.Skip;
                        }

                        if (InitializeResult.ReadyToGo == initResult)
                        {
                            Log.EndMethod("Initialize");
                        }
                        else
                        {
                            if (initializedLog)
                            {
                                Log.EndMethod("Initialize");
                            }
                            else
                            {
                                SpotTestLog.StartTestLog(t.Name);
                            }

                            // Get all the methods in the suite and log them as skipped.
                            MethodInfo[] skipMethods = t.GetMethods(bf);
                            foreach (MethodInfo method in skipMethods)
                            {
                                Type returnType = method.ReturnType;
                                if (returnType == typeof(MFTestResults))
                                {
                                    Log.StartTestMethod(method.Name);
                                    Log.TestResult("TEST: " + method.Name, MFTestResults.Skip);
                                    Log.EndTestMethod();
                                }
                            }

                            Log.StartMethod("CleanUp");
                            Log.CleanUp(t.Name);

                            // Reset counts and continue to the next test suite class.
                            Log.ResetCounts();
                            continue;
                        }                            

                        // Get all the methods and call the one that matches the return type for test methods.
                        MethodInfo[] methods = t.GetMethods(bf);
                        foreach (MethodInfo method in methods)
                        {
                            Type returnType = method.ReturnType;
                            if (returnType != typeof(MFTestResults))
                            {
                                continue;
                            }
                            else
                            {
                                Log.StartTestMethod(method.Name);

                                try
                                {
                                    object result = method.Invoke(imf, new object[0]);
                                    switch((int)result)
                                    {
                                        case (int)MFTestResults.Fail:
                                            Log.TestResult("TEST: " + method.Name, MFTestResults.Fail);
                                            break;
                                        case (int)MFTestResults.Pass:
                                            Log.TestResult("TEST: " + method.Name, MFTestResults.Pass);
                                            break;
                                        case (int)MFTestResults.Skip:
                                            Log.TestResult("TEST: " + method.Name, MFTestResults.Skip);
                                            break;
                                        case (int)MFTestResults.KnownFailure:
                                            Log.TestResult("TEST: " + method.Name, MFTestResults.KnownFailure);
                                            break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    //Log.Comment("#########START STACK TRACE#########");
                                    Log.Comment(ex.StackTrace);
                                    //Log.Comment("#########END STACK TRACE#########");
                                    Log.TestResult("TEST: " + method.Name + " threw an unhandled exception", 
                                        MFTestResults.Fail);
                                    //Log.EndTestMethod();
                                    //Log.StartMethod("CleanUp");
                                    //Log.CleanUp(t.Name);
                                    //throw ex;
                                }
                                Log.EndTestMethod();
                            }
                        }

                        // Call cleanup.
                        Log.StartMethod("CleanUp");
                        imf.CleanUp();
                        Log.CleanUp(t.Name);
                    }
                }
            }
        }
    }
}
