using System;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;

using Task = Microsoft.Build.Utilities.Task;
using Timeout2 = System.Threading.Timeout;

namespace Microsoft.SPOT.Automation.Build.Branch
{
    public class AsyncWait : Task
    {
        string[] syncEventNames;
        EventWaitHandle[] handles;
        AutoResetEvent allEvents = new AutoResetEvent(false);
        int timeout = Timeout2.Infinite;
        bool allSignaled = true;
        bool failOnTimeout = false;

        public override bool Execute()
        {
            try
            {
                if (syncEventNames.Length == 0)
                {
                    return true;
                }

                List<EventWaitHandle> possibleHandles = new List<EventWaitHandle>();
                
                for (int i = 0; i < syncEventNames.Length; i++)
                {
                    try
                    {
                        possibleHandles.Add(EventWaitHandle.OpenExisting(syncEventNames[i]));
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            Log.LogWarningFromException(e);
                        }
                        catch { }
                    }
                }

                handles = new EventWaitHandle[possibleHandles.Count];

                possibleHandles.CopyTo(handles, 0);

                Thread waitThread = new Thread(new ThreadStart(WaitAll));
                waitThread.SetApartmentState(ApartmentState.MTA);
                waitThread.Start();

                allEvents.WaitOne();

                if (failOnTimeout)
                {
                    return allSignaled;
                }

                return true;
            }
            catch (Exception e)
            {
                try
                {
                    Log.LogErrorFromException(e);
                }
                catch { }
                return false;
            }
        }

        private void WaitAll()
        {
            allSignaled = EventWaitHandle.WaitAll(
                handles,
                timeout == Timeout2.Infinite ? timeout : timeout * 60 * 1000,
                false);
            
            allEvents.Set();
        }

        [Required]
        public string[] SyncEventNames
        {
            set { syncEventNames = value; }
        }

        public int Timeout
        {
            set { timeout = value; }
        }

        public bool FailOnTimeout
        {
            set { failOnTimeout = value; }
        }

        [Output]
        public bool Signaled
        {
            get { return allSignaled; }
        }
    }
}
