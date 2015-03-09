using Microsoft.SPOT.Platform.Test;
using Microsoft.Win32;
using System;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Timers;

namespace Microsoft.SPOT.Platform.Test
{
    internal class TimedTest
    {
        #region Member variables

        private BaseTest m_test;
        private Harness m_harness = null;
        private int m_timeOut = 1800000; // 30 minutes default timeout.
        private bool m_didTestTimeOut = false;
        private bool m_didEmulatorCrash = false;
        private XmlLog m_log = null;
        private System.Timers.Timer aTimer;
        private static RegistryKey m_rk = 
            Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\.NETMicroFramework\NonVersionSpecific\Test");
        private int m_halt = (m_rk == null ? 0 : (int)m_rk.GetValue("HaltOnTimeout", 0)); 

        #endregion

        #region Constructors

        /// <summary>
        /// Overloaded Constructor 
        /// </summary>
        /// <param name="test">The name of the sln file.</param>
        /// <param name="harness">The Harness instance.</param>
        /// <param name="log">Path to log file. </param>
        internal TimedTest(BaseTest test, Harness harness, XmlLog log)
        {
            this.m_test = test;
            this.m_harness = harness;
            this.m_log = log;
        }
        
        #endregion

        #region Internal properties

        /// <summary>
        /// Property to get/set the test.
        /// </summary>
        internal BaseTest Test
        {
            get
            {
                return this.m_test;
            }

            set
            {                
                this.m_test = value;
            }
        }

        /// <summary>
        /// Property to get/set the harness.
        /// </summary>
        internal Harness Harness
        {
            get
            {
                return this.m_harness;
            }

            set
            {
                if (null != value)
                {
                    this.m_harness = value;
                }
            }
        }

        /// <summary>
        /// Property to get/set the test timeout.
        /// </summary>
        internal int TimeOut
        {
            get
            {
                return this.m_timeOut;
            }

            set
            {
                if (value > 0)
                {
                    this.m_timeOut = value;
                }
            }
        }

        /// <summary>
        /// This property gives info on whether a test timed out.
        /// </summary>
        internal bool DidTestTimeOut
        {
            get
            {
                return this.m_didTestTimeOut;
            }
        }

        /// <summary>
        /// This property gives info on whether emulator crashed during a run.
        /// </summary>
        internal bool DidEmulatorCrash
        {
            get
            {
                return this.m_didEmulatorCrash;
            }

            set
            {
                this.m_didEmulatorCrash = value;
            }
        }

        #endregion

        #region Execute

        internal HarnessExecutionResult Execute()
        {
            aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = m_timeOut;            
            aTimer.Start();
            HarnessExecutionResult harnessExecutionResult = this.m_harness.Run(m_test, m_log);
            aTimer.Stop();
            aTimer.Dispose();
            if (HarnessExecutionResult.Exception == harnessExecutionResult)
            {
                KillProcesses(true);
            }

            if (m_didTestTimeOut)
            {
                harnessExecutionResult = HarnessExecutionResult.TimeOut;
            }

            if (m_didEmulatorCrash)
            {
                harnessExecutionResult = HarnessExecutionResult.Success;
            }

            return harnessExecutionResult;
        }

        #endregion

        internal void SendMail()
        {
            string emailFrom = "mfalaba@microsoft.com";

            // Send results email                
            SendMail mail = new SendMail();
            mail.To = "mfar@microsoft.com,lorenzte@microsoft.com";            
            mail.Subject = string.Format("TEST TIMED OUT: Test {0} on machine {1} for build number {2} has timed out",
                m_test.Name, Environment.MachineName, TestSystem.BuildNumber);
            mail.From = emailFrom;
            mail.IsBodyHtml = true;
            string mailBody = string.Format(
                "On {0}, Test {1} for build number {2} has exceeded the {3} minute timeout rule for running the test. " +
                "<BR><BR>As a result the harness is {4}", Environment.MachineName, m_test.Name, TestSystem.BuildNumber,
                m_timeOut / 60000, "terminating executing the current test suite, and will continue.");

            if (m_halt != 0)
            {
                mailBody = string.Format(
                "On {0}, Test {1} for build number {2} has exceeded the {3} minute timeout rule for running the test. " +
                "<BR><BR>As a result the harness is {4}", Environment.MachineName, m_test.Name, TestSystem.BuildNumber,
                m_timeOut / 60000, "waiting for investigation. Tests will halt until manual action is taken.");
            }
            mail.Body = mailBody;
            Console.WriteLine("\tSending email..");
            mail.Execute();
        }

        #region Private methods

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Console.WriteLine("\tTIME OUT: Test timed out.");            
            m_didTestTimeOut = true;
            m_test.TimedOut = true;            

            // If registry key is set to any value but 0, then halt and wait for investigation   
            Utils.WriteToEventLog("Registry key to kill processes on time out is set to " + m_halt);
            if (m_halt == 0)
            {
                Utils.WriteToEventLog("Killing proceesses due to time out.");
                aTimer.Stop();
                KillProcesses(false);
            }
            m_harness.Close();
        }        

        private void KillProcesses(bool isComExceptionThrown)
        {
            bool emCrash = KillEmulator();

            foreach (Process p in Process.GetProcessesByName("DW20"))
            {
                p.Kill();
                if (emCrash)
                {
                    m_didEmulatorCrash = true;
                    m_test.EmulatorCrashed = true;
                }
            }

            if (m_didTestTimeOut || isComExceptionThrown)
            {
                try
                {
                    m_harness.SaveLogFile(m_test.Location);
                }
                catch (Exception ex)
                {
                    try
                    {
                        m_log.AddCommentToLog("An exception was thrown trying to save the log file: " + ex.ToString());
                    }
                    catch
                    {
                    }
                    Utils.WriteToEventLog(string.Format("An exception was thrown in TimedTest: {0}", ex.ToString()));
                }                
            }
        }

        internal bool KillEmulator()
        {
            bool emCrash = false;

            Harness.SetDeviceDoneEvent();
            foreach (Process p in Process.GetProcessesByName("Microsoft.SPOT.Emulator.Sample.SampleEmulator"))
            {
                p.Kill();
                emCrash = true;
            }

            return emCrash;
        }

        #endregion
    }
}