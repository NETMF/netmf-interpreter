////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Text;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Hardware;

namespace Microsoft.SPOT.Platform.Tests
{
    public class Master_ExtendedWeakReferenceTests
    {
        private static ExtendedWeakReference[] ewrs = new ExtendedWeakReference[10];
        private static TestRunTracker runTracker;
        public static void Main()
        {
            // This test suite does not use the Test Runner infrastructure as it requires a 
            // reboot of the physical device to verify the references have been saved.  It relies
            // on the test harness to time it out if it fails to complete because of issues with
            // EWR persistence.

            // Skip test on Emulator
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
            {
                StartTestLog("Emulator, skipping...");
                EndTestLog(0, 0, 1);
            }

            else
            {
                ewrs[0] = ExtendedWeakReference.Recover(typeof(TestRunTracker), 0);
                if (ewrs[0] == null)
                {
                    ewrs[0] = ExtendedWeakReference.RecoverOrCreate(
                      typeof(TestRunTracker),
                      0,
                      ExtendedWeakReference.c_SurviveBoot | ExtendedWeakReference.c_SurvivePowerdown);
                    runTracker = new TestRunTracker() { RebootCount = -1, TestState = TestRunTracker.State.Initialize };
                }
                else
                {
                    runTracker = (TestRunTracker)ewrs[0].Target;
                }

                Log.Comment("State: " + runTracker.TestState + ", Reboot Count: " + runTracker.RebootCount);

                switch (runTracker.TestState)
                {
                    // Initial state
                    case TestRunTracker.State.Initialize:
                        if (runTracker.RebootCount == -1)
                            StartTestLog("No EWR found, initializing for first run...");
                        else
                            StartTestLog("Previous EWR found.  Re-initializing...");

                        runTracker.TestState = TestRunTracker.State.HardReboot;
                        runTracker.RebootCount = 0;
                        runTracker.TestState = TestRunTracker.State.HardReboot;
                        for (uint i = 1; i < 10; i++)
                        {
                            ewrs[i] = ExtendedWeakReference.RecoverOrCreate(typeof(TestObj), i,
                                ExtendedWeakReference.c_SurvivePowerdown |
                                ExtendedWeakReference.c_SurviveBoot);
                            TestObj obj = new TestObj();
                            ewrs[i].Target = obj;
                            runTracker.objHashes[i] = obj.GetHashCode();
                        }
                        break;

                    // HardReboot cases - Reboots 1 - 3
                    case TestRunTracker.State.HardReboot:
                        Debug.Print("<TestMethod name=\"HardReboot" + runTracker.RebootCount + "\">");

                        // Validate objects
                        if (ValidateEWR(true))
                        {
                            Debug.Print("    <TestMethodResult Result=\"Pass\">");
                            runTracker.PassCount++;
                        }
                        else
                        {
                            Debug.Print("    <TestMethodResult Result=\"Fail\">");
                            runTracker.FailCount++;
                        }
                        Debug.Print("        <Text><![CDATA[TEST: HardReboot" + runTracker.RebootCount + "]]></Text>" + TimeStamp());
                        Debug.Print("    </TestMethodResult>");
                        Debug.Print("</TestMethod>");

                        // End test
                        if (runTracker.RebootCount >= 3)
                            runTracker.TestState = TestRunTracker.State.SoftReboot;

                        break;

                    // SoftReboot cases - Reboots 4 - 6
                    case TestRunTracker.State.SoftReboot:
                        Debug.Print("<TestMethod name=\"SoftReboot" + runTracker.RebootCount + "\">");

                        // Validate objects
                        if (ValidateEWR(true))
                        {
                            Debug.Print("    <TestMethodResult Result=\"Pass\">");
                            runTracker.PassCount++;
                        }
                        else
                        {
                            Debug.Print("    <TestMethodResult Result=\"Fail\">");
                            runTracker.FailCount++;
                        }
                        Debug.Print("        <Text><![CDATA[TEST: SoftReboot" + runTracker.RebootCount + "]]></Text>" + TimeStamp());
                        Debug.Print("    </TestMethodResult>");
                        Debug.Print("</TestMethod>");

                        // End test
                        if (runTracker.RebootCount >= 6)
                            runTracker.TestState = TestRunTracker.State.Restore;
                        break;

                    // Restore cases - Reboots 7 - 8
                    case TestRunTracker.State.Restore:
                        Debug.Print("<TestMethod name=\"Restore" + runTracker.RebootCount + "\">");
                        if (runTracker.RebootCount == 7)
                            Log.Comment("Restore and Pushback with Soft Reboot");
                        else
                            Log.Comment("Restore and Pushback with Hard Reboot");

                        // Validate objects
                        if (ValidateEWR(false))
                        {
                            Debug.Print("    <TestMethodResult Result=\"Pass\">");
                            runTracker.PassCount++;
                        }
                        else
                        {
                            Debug.Print("    <TestMethodResult Result=\"Fail\">");
                            runTracker.FailCount++;
                        }
                        Debug.Print("        <Text><![CDATA[TEST: SoftReboot" + runTracker.RebootCount + "]]></Text>" + TimeStamp());
                        Debug.Print("    </TestMethodResult>");
                        Debug.Print("</TestMethod>");

                        if (runTracker.RebootCount > 8)
                        {
                            runTracker.TestState = TestRunTracker.State.Complete;
                        }
                        break;

                    // Tests complete
                    case TestRunTracker.State.Complete:
                        runTracker.TestState = TestRunTracker.State.Initialize;
                        // Close logs
                        EndTestLog(runTracker.PassCount, runTracker.FailCount, 0);
                        break;
                }
                runTracker.RebootCount++;
                ewrs[0].Target = runTracker;
                // Need to sleep to make sure debug buffer has chance to flush before rebooting
                // This should not be needed for EWR any longer
                System.Threading.Thread.Sleep(15000);

                switch (runTracker.TestState)
                {
                    case TestRunTracker.State.HardReboot:
                        PowerState.RebootDevice(false);
                        break;
                    case TestRunTracker.State.SoftReboot:
                        PowerState.RebootDevice(true);
                        break;
                    case TestRunTracker.State.Restore:
                        if (runTracker.RebootCount == 8)
                            PowerState.RebootDevice(true);
                        else
                            PowerState.RebootDevice(false);
                        break;
                }
                // fall through to end test suite
            }
        }

        private static void StartTestLog(string status)
        {
            Debug.Print("<TestLog Test=\"ExtendedWeakReferenceTests\">");
            Debug.Print("<Initialize>");
            Log.Comment(status);
            Debug.Print("</Initialize>");
        }

        private static void EndTestLog(int Pass, int Fail, int Skip)
        {
            string timeStamp = TimeStamp();
            Debug.Print("<CleanUp>");
            Log.Comment("Cleaning up after tests...");
            Debug.Print("</CleanUp>");
            Debug.Print("<Results>");
            Debug.Print("    <PassCount><Text>" + Pass + "</Text>" + timeStamp + "</PassCount>");
            Debug.Print("    <FailCount><Text>" + Fail + "</Text>" + timeStamp + "</FailCount>");
            Debug.Print("    <SkipCount><Text>" + Skip + "</Text>" + timeStamp + "</SkipCount>");
            Debug.Print("    <KnownFailureCount><Text>0</Text>" + timeStamp + "</KnownFailureCount>");
            Debug.Print("</Results>");
            Debug.Print("</TestLogs>");
        }

        private static string TimeStamp()
        {
            DateTime now = DateTime.Now;
            return "<Date>" + now.ToString("M/d/yyyy") + "</Date><Time>" + now.ToString("hh:mm:ss.ff") + "</Time>";
        }

        private static bool ValidateEWR(bool NewData)
        {
            bool success = true;
            for (uint i = 1; i < 10; i++)
            {
                ewrs[i] = ExtendedWeakReference.Recover(typeof(TestObj), i);
                if (ewrs[i] == null)
                {
                    success = false;
                    Log.Exception("Unable to recover object #" + i + " after reboot " + runTracker.RebootCount);
                    ewrs[i] = ExtendedWeakReference.RecoverOrCreate(typeof(TestObj), i,
                        ExtendedWeakReference.c_SurviveBoot |
                        ExtendedWeakReference.c_SurvivePowerdown);
                }
                TestObj obj = (TestObj)ewrs[i].Target;
                if (obj == null)
                {
                    success = false;
                    Log.Exception("Recovered object #" + i + " null after reboot " + runTracker.RebootCount);
                }
                if (runTracker.objHashes[i] != obj.GetHashCode())
                {
                    success = false;
                    Log.Exception("Recovered object #" + i + " hash value does not match stored hash value");
                }
                Log.Comment("Verified object #" + i);
                if (NewData)
                {
                    Log.Comment("Creating new object and setting back to ewr");
                    obj = new TestObj();
                    ewrs[i].Target = obj;
                    runTracker.objHashes[i] = obj.GetHashCode();
                }
                else
                {
                    Log.Comment("Pushing object back into list");
                    ewrs[i].PushBackIntoRecoverList();
                }
            }
            return success;
        }
        [Serializable]
        internal sealed class TestRunTracker
        {
            public enum State
            {
                Initialize,
                HardReboot,
                SoftReboot,
                Restore,
                Complete
            }

            public TestRunTracker()
            {
                this.RebootCount = 0;
                this.objHashes = new int[10];
            }
            public State TestState { get; set; }
            public int RebootCount { get; set; }
            public int PassCount { get; set; }
            public int FailCount { get; set; }
            public int[] objHashes { get; set; }
        }

        [Serializable]
        internal sealed class TestObj
        {
            public TestObj()
            {
                Random rand = new Random(DateTime.Now.Millisecond);
                this.Count = rand.Next();
                this.uCount = (uint)rand.Next();
                this.LongCount = rand.Next();
                this.uLongCount = (ulong)rand.Next();
                this.Double = rand.NextDouble();
                this.byteArray = new byte[1024];
                rand.NextBytes(this.byteArray);
                this.charArray = MFUtilities.GetRandomString().ToCharArray();
                this.Data = new string(this.charArray);
            }
            public int Count { get; set; }
            public uint uCount { get; set; }
            public long LongCount { get; set; }
            public ulong uLongCount { get; set; }
            public double Double { get; set; }
            byte[] byteArray { get; set; }
            char[] charArray { get; set; }
            bool Bool { get; set; }
            public string Data { get; set; }
        }
    }
}