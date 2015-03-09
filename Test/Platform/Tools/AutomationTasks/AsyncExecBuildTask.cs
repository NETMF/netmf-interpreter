using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;

using Task = Microsoft.Build.Utilities.Task;

namespace Microsoft.SPOT.Automation.Build.Branch
{
    public class CountLock
    {
        public int counter = 0;
    }

    public class AsyncExecBuildTask : ExecBuildTask
    {
        ITaskItem syncEventName;
        EventWaitHandle handle;

        int maxThreads = 0;
        string lockName = "COMMON";
        CountLock threadLock = null;

        enum ThreadState { NOT_STARTED, WAITING, EXECUTING, DONE };

        ThreadState m_AsyncThreadState = ThreadState.NOT_STARTED;

        static Dictionary<string, CountLock> locks = new Dictionary<string, CountLock>();

        static AsyncExecBuildTask()
        {
            locks.Add("COMMON", new CountLock());
        }

        static CountLock GetLock(string lockName)
        {
            lock (locks)
            {
                if(!locks.ContainsKey(lockName))
                {
                    locks.Add(lockName, new CountLock());
                }

                return locks[lockName];
            }
        }

        public override bool Execute()
        {
            try
            {
                syncEventName = new TaskItem(Guid.NewGuid().ToString());
                
                handle = new EventWaitHandle(false, EventResetMode.ManualReset, syncEventName.ItemSpec);
                handle.Reset();

                threadLock = GetLock(lockName);

                new Thread(new ThreadStart(AsyncExecute)).Start();

                while (m_AsyncThreadState == ThreadState.NOT_STARTED)
                {
                    Thread.Sleep(500);
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

        protected void AsyncExecute()
        {
            if (maxThreads > 0)
            {
                lock (threadLock)
                {
                    while (threadLock.counter >= maxThreads)
                    {
                        m_AsyncThreadState = ThreadState.WAITING;
                        Monitor.Wait(threadLock);
                    }

                    threadLock.counter++;
                }
            }
            m_AsyncThreadState = ThreadState.EXECUTING;
            base.Execute();
            m_AsyncThreadState = ThreadState.DONE;
            handle.Set();

            if (maxThreads > 0)
            {
                lock (threadLock)
                {
                    threadLock.counter--;
                    Monitor.PulseAll(threadLock);
                }
            }
        }

        [Output]
        public ITaskItem SyncEventName
        {
            get { return syncEventName; }
        }

        public string LockName
        {
            set { lockName = value; }
        }

        public int MaxConcurrentThreads
        {
            set { maxThreads = value; }
        }
    }
}
