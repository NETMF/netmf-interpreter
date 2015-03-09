using System;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Cryptoki;
using System.IO;

namespace Microsoft.SPOT.Platform.Tests
{
    public delegate void ExceptionTestCaseImpl();
    public delegate bool TestCaseImpl();

    public class TestRunner
    {
        TestGroup[] m_groups;

        public TestRunner(TestGroup[] groups)
        {
            m_groups = groups;
        }

        public bool Run()
        {
            bool ret = true;

            try
            {
                for (int i = 0; i < m_groups.Length; i++)
                {
                    ret &= m_groups[i].RunTestCases();
                }
            }
            catch
            {
                ret = false;
            }

            return ret;
        }
    }

    public class TestGroup
    {
        string m_details;
        TestCase[] m_cases;

        public TestGroup(string groupDetails, params TestCase[] cases)
        {
            m_details = groupDetails;
            m_cases = cases;
        }

        public bool RunTestCases()
        {
            bool bRet = true;

            Log.Comment("Run Test Group: " + m_details);

            for (int i = 0; i < m_cases.Length; i++)
            {
                Log.Comment("Running Test: " + m_cases[i].Description);
                if (!m_cases[i].RunTest())
                {
                    Log.Comment("Test Failed");
                    bRet = false;
                }
            }

            return bRet;
        }
    }

    public class TestCase
    {
        protected string m_detail;
        protected TestCaseImpl m_test;

        protected TestCase(string testDetails)
        {
            m_detail = testDetails;
        }

        public TestCase(string testDetails, TestCaseImpl test)
        {
            m_detail = testDetails;
            m_test = test;
        }

        public virtual bool RunTest()
        {
            try
            {
                return m_test();
            }
            catch(Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
                return false;
            }
        }

        public virtual string Description
        {
            get { return m_detail; }
        }
    }

    public class ExceptionTestCase : TestCase
    {
        ExceptionTestCaseImpl m_exTest;
        Exception[] m_expected;

        public static Exception[] ArgumentExceptions = new Exception[] { new ArgumentException(), new ArgumentNullException(), new ArgumentOutOfRangeException() };
        public static Exception[] NullExceptions = new Exception[] { new ArgumentNullException(), new NullReferenceException() };

        public ExceptionTestCase(string testDetails, ExceptionTestCaseImpl test, Exception[] expectedExceptions)
            : base(testDetails)
        {
            m_exTest = test;
            m_expected = expectedExceptions;
        }

        public override bool RunTest()
        {
            bool bRet = false;
            try
            {
                m_exTest();
            }
            catch (Exception e)
            {
                foreach (Exception ex in m_expected)
                {
                    if (e.GetType() == ex.GetType())
                    {
                        bRet = true;
                        break;
                    }
                }
            }

            return bRet;
        }
    }
}