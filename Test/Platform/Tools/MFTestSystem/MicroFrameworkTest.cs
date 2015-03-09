using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.SPOT.Platform.Test
{
    [Serializable]
    public class BaseTest
    {
        #region Member Variables

        private string m_name;
        private string m_location;
        private string m_testOwner;
        private string m_devOwner;
        private string m_pmOwner;
        private string m_result;
        private string m_logfile;
        private string m_exePath;
        private DateTime m_startTime;
        private DateTime m_endTime;
        private bool m_timedOut;
        private bool m_emulatorCrashed;

        #endregion

        #region Properties

        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        public string Location
        {
            get { return m_location; }
            set { m_location = value; }
        }

        public string ExeLocation
        {
            get { return m_exePath; }
            set { m_exePath = value; }
        }

        public string TestOwner
        {
            get { return m_testOwner; }
            set { m_testOwner = value; }
        }

        public string DevOwner
        {
            get { return m_devOwner; }
            set { m_devOwner = value; }
        }

        public string PmOwner
        {
            get { return m_pmOwner; }
            set { m_pmOwner = value; }
        }

        public string Result
        {
            get { return m_result; }
            set { m_result = value; }
        }

        public string LogFile
        {
            get { return m_logfile; }
            set { m_logfile = value; }
        }

        public DateTime StartTime
        {
            get { return m_startTime; }
            set { m_startTime = value; }
        }

        public DateTime EndTime
        {
            get { return m_endTime; }
            set { m_endTime = value; }
        }

        public bool TimedOut
        {
            get { return m_timedOut; }
            set { m_timedOut = value; }
        }

        public bool EmulatorCrashed
        {
            get { return m_emulatorCrashed; }
            set { m_emulatorCrashed = value; }
        }

        public virtual int TotalTestCases
        {
            get;
            set;
        }

        public virtual int TestMethodPassCount 
        {
            get;
            set;
        }

        public virtual int TestMethodFailCount 
        {
            get;
            set;
        }

        public virtual int TestMethodSkipCount 
        {
            get;
            set;
        }

        public virtual int TestMethodKnownFailureCount 
        {
            get;
            set;
        }

        public virtual ArrayList TestMethods
        {
            get;
            set;
        }
        
        #endregion
    }

    [Serializable]
    public class MicroFrameworkTest : BaseTest
    {
        #region Member Variables

        private int m_totalTestCases;
        private int m_totalPassed;
        private int m_totalFailed;
        private int m_totalSkipped;
        private int m_totalKnownFailures;

        #endregion

        #region Public Properties

        public override int TotalTestCases
        {
            get { return m_totalTestCases; }
            set { m_totalTestCases = value; }
        }

        public override int TestMethodPassCount
        {
            get { return m_totalPassed; }
            set { m_totalPassed = value; }
        }

        public override int TestMethodFailCount
        {
            get { return m_totalFailed; }
            set { m_totalFailed = value; }
        }

        public override int TestMethodSkipCount
        {
            get { return m_totalSkipped; }
            set { m_totalSkipped = value; }
        }

        public override int TestMethodKnownFailureCount
        {
            get { return m_totalKnownFailures; }
            set { m_totalKnownFailures = value; }
        }

        #endregion
    }

    [Serializable]
    public class ProfilerTest : BaseTest
    {
        private ArrayList m_testMethodDetails =
            new ArrayList();

        public override ArrayList TestMethods
        {
            get { return m_testMethodDetails; }
            set { m_testMethodDetails = value; }
        }
    }

    [Serializable]
    public class ProfilerTestMethod
    {
        private string m_testMethod;
        private int m_inclTime;
        private int m_exclTime;

        public string TestMethod
        {
            get { return m_testMethod; }
            set { m_testMethod = value; }
        }

        public int InclusiveTime
        {
            get { return m_inclTime; }
            set { m_inclTime = value; }
        }

        public int ExclusiveTime
        {
            get { return m_exclTime; }
            set { m_exclTime = value; }
        }
    }

    
}
