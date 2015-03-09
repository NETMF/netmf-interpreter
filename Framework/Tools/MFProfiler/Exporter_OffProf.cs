using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using _PRF = Microsoft.SPOT.Profiler;

namespace Microsoft.SPOT.Profiler
{
#if DEBUG
    public class Exporter_OffProf : Exporter
    {
        private const string c_line = "{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}";

        private Dictionary<uint, ThreadState> m_htThreads = new Dictionary<uint, ThreadState>();
        private int m_cCalls = 0;
        private bool m_closed = false;

        private ProfilerSession m_sess;

        private uint m_currentThread;

        private class Call
        {
            public uint Function;
            public ulong Duration;
            public List<Call> Children = new List<Call>();
            public Call Parent;
            public int Invocations = 1;

            public bool IsRoot
            {
                get { return Parent == null; }
            }

            public void AddChild(Call c)
            {
                Children.Add(c);
                c.Parent = this;
            }
        }

        private class ThreadState
        {
            public uint m_threadID;
            public Call m_call = new Call();
        }

        public Exporter_OffProf(ProfilerSession ps, string file) : base(ps, file)
        {
            ps.OnEventAdd += new ProfilerSession.OnProfilerEventAddHandler(ProcessEvent);
            m_sess = ps;
        }

        public override void Close()
        {
            if (m_closed == false)
            {
                Filter();
                WriteData();
                base.Close();
                m_closed = true;
            }
        }

        private void ProcessEvent(_PRF.ProfilerSession ps, _PRF.ProfilerEvent pe)
        {
            switch (pe.Type)
            {
                case _PRF.ProfilerEvent.EventType.Call:
                    {
                        _PRF.FunctionCall f = (_PRF.FunctionCall)pe;
                        m_currentThread = f.m_thread;

                        ThreadState ts;
                        if (m_htThreads.ContainsKey(m_currentThread))
                        {
                            ts = m_htThreads[m_currentThread];
                        }
                        else
                        {
                            ts = new ThreadState();
                            ts.m_threadID = f.m_thread;
                            m_htThreads[f.m_thread] = ts;
                        }

                        Call c = new Call();
                        c.Function = f.m_callStack[f.m_callStack.Length - 1];

                        try
                        {
                            m_sess.m_engine.GetMethodName(c.Function, true);
                        }
                        catch { }

                        ts.m_call.AddChild(c);
                        ts.m_call = c;
                        break;
                    }
                case _PRF.ProfilerEvent.EventType.Return:
                    {
                        _PRF.FunctionReturn f = (_PRF.FunctionReturn)pe;
                        ThreadState ts;
                        if (m_htThreads.ContainsKey(m_currentThread))
                        {
                            ts = m_htThreads[m_currentThread];
                        }
                        else
                        {
                            ts = new ThreadState();
                            ts.m_threadID = f.m_thread;
                            m_htThreads[f.m_thread] = ts;
                        }

                        if ((f.duration & 0x80000000) == 0)
                        {
                            //OffView really really doesn't like negative times.
                            ts.m_call.Duration += f.duration;
                        }

                        if (ts.m_call.Parent != null)
                        {
                            //Make timing inclusive.
                            ts.m_call.Parent.Duration += ts.m_call.Duration;
                        }
                        ts.m_call = ts.m_call.Parent;
                        break;
                    }
                case _PRF.ProfilerEvent.EventType.ContextSwitch:
                    {
                        _PRF.ContextSwitch c = (_PRF.ContextSwitch)pe;
                        m_currentThread = c.m_thread;
                        break;
                    }
            }
        }

        private void CollapseCallsHelper(Call call1, Call call2)
        {
            call1.Duration += call2.Duration;
            call1.Invocations += call2.Invocations;
            foreach (Call child in call2.Children)
                call1.AddChild(child);
        }

        private void CollapseCalls(Call call)
        {
            List<Call> children = call.Children;
            call.Children = new List<Call>();
            Dictionary<uint,Call> ht = new Dictionary<uint,Call>();

            foreach (Call child in children)
            {
                if(ht.ContainsKey(child.Function))
                {
                    CollapseCallsHelper(ht[child.Function], child);
                }
                else
                {
                    call.AddChild(child);
                    ht[child.Function] = child;
                }
            }

            foreach (Call child in call.Children)
                CollapseCalls(child);
        }

        private void CollapseThreads()
        {
            foreach (ThreadState ts in m_htThreads.Values)
            {
                //When the user disconnects before all methods return, we need to progress back up.
                while (!ts.m_call.IsRoot)
                {
                    if ((ts.m_call.Duration & 0x80000000) == 0)
                    {
                        //OffView really really doesn't like negative times.
                        ts.m_call.Parent.Duration += ts.m_call.Duration;
                    }
                    ts.m_call = ts.m_call.Parent;
                }
                CollapseCalls(ts.m_call);
            }
        }

        private void Filter()
        {
            CollapseThreads();
        }


        private void WriteCallTree(StreamWriter sw, Call c, int depth)
        {
            if (depth > 0)
            {
                sw.WriteLine(c_line, c.Invocations, depth, ResolveMethodName(c.Function), "0", c.Duration, " ", " ");
                m_cCalls++;
            }
            foreach (Call child in c.Children)
                WriteCallTree(sw, child, depth + 1);
        }

        private void WriteData()
        {
            m_sw.WriteLine(c_line, "Calls", "Level", "Function", "Module", "Time", "Type", "Warnings");

            foreach (ThreadState ts in m_htThreads.Values)
            {
                m_sw.WriteLine(c_line, 0, 0, String.Format("[Thread {0}]", ts.m_threadID), "0", ts.m_call.Duration, " ", " ");
                WriteCallTree(m_sw, ts.m_call, 0);
            }

            /**********************************************************************/
            m_sw.WriteLine();
            m_sw.WriteLine("Hong Kong Phooey, number one super guy. Hong Kong Phooey, quicker than the");
            m_sw.WriteLine("human eye. He's got style, a groovy style, and a car that just won't stop. When");
            m_sw.WriteLine("the going gets tough, he's really rough, with a Hong Kong Phooey chop (Hi-Ya!).");
            m_sw.WriteLine("Hong Kong Phooey, number one super guy. Hong Kong Phooey, quicker than the");
            m_sw.WriteLine("human eye. Hong Kong Phooey, he's fan-riffic!");
            m_sw.WriteLine();
            m_sw.WriteLine("Children of the sun, see your time has just begun, searching for your ways,");
            m_sw.WriteLine("through adventures every day. Every day and night, with the condor in flight,");
            m_sw.WriteLine("with all your friends in tow, you search for the Cities of Gold.");
            m_sw.WriteLine("Ah-ah-ah-ah-ah... wishing for The Cities of Gold.  Ah-ah-ah-ah-ah... some day");
            m_sw.WriteLine("we will find The Cities of Gold.  Do-do-do-do ah-ah-ah, do-do-do-do, Cities of");
            m_sw.WriteLine("Gold. Do-do-do-do, Cities of Gold. Ah-ah-ah-ah-ah... some day we will find The");
            m_sw.WriteLine("Cities of Gold.");
            m_sw.WriteLine();
            m_sw.WriteLine("I never spend much time in school but I taught ladies plenty. It's true I hire");
            m_sw.WriteLine("my body out for pay, hey hey. I've gotten burned over Cheryl Tiegs, blown up");
            m_sw.WriteLine("for Raquel Welch. But when I end up in the hay it's only hay, hey hey. I might");
            m_sw.WriteLine("jump an open drawbridge, or Tarzan from a vine. 'Cause I'm the unknown stuntman");
            m_sw.WriteLine("that makes Eastwood look so fine.");
            /*********************************************************************/

            m_sw.WriteLine();
            m_sw.WriteLine("STATISTICS:");
            m_sw.WriteLine("Total Threads = " + m_htThreads.Count);
            m_sw.WriteLine("Maximum Concurrent Threads = 1");
            m_sw.WriteLine("Total Function Nodes = " + m_cCalls);
            m_sw.WriteLine("Total Function Calls = " + m_cCalls);
            m_sw.WriteLine("Memory Buffer Used = 1K");
            m_sw.WriteLine("Memory Buffer Allocated = 1K");
            m_sw.WriteLine("Memory Buffer Reserved = 2K");
            m_sw.Flush();
        }

        string ResolveMethodName(uint md)
        {
            string ret;
            try
            {
                //The Debugger just loves to throw exceptions if the Emulator quits before we're done processing.
                ret = m_sess.m_engine.GetMethodName(md, true);
            }
            catch
            {
                ret = "UNKNOWN_METHOD";
            }
            return ret;
        }
    }
#endif
}
