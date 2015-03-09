using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using _PRF = Microsoft.SPOT.Profiler;

namespace Microsoft.SPOT.Profiler
{
    public class Exporter_CLRProfiler : Exporter
    {
        private bool m_processedFirstEvent;

        private Dictionary<uint, uint>          m_methodIdLookup;
        private Dictionary<ObjectType, ulong>   m_typeIdLookup;

        private uint m_nextMethodId;
        private ulong m_nextTypeId;
        private ulong m_nextCallstackId;

        private ulong m_startTime;
        private ulong m_lastWrittenTime;

        public Exporter_CLRProfiler(ProfilerSession ps, string file) : base(ps, file)
        {
            m_methodIdLookup = new Dictionary<uint, uint>();
            m_typeIdLookup = new Dictionary<ObjectType, ulong>();

            m_nextMethodId = 1;
            m_nextTypeId = 1;
            m_nextCallstackId = 1;

            ps.OnEventAdd += new ProfilerSession.OnProfilerEventAddHandler(ProcessEvent);
        }

        internal void ProcessEvent(_PRF.ProfilerSession ps, _PRF.ProfilerEvent pe)
        {
            ulong callstack;

            if (m_processedFirstEvent == false)
            {
                m_processedFirstEvent = true;
                m_startTime = pe.Time;
                m_lastWrittenTime = pe.Time;
            }
            else
            {
                if (pe.Time >= m_lastWrittenTime + 1)
                {   // Mimic the 5-ms granularity of the CLRProfiler.
                    m_sw.WriteLine("i {0}", pe.Time - m_startTime);
                    m_lastWrittenTime = pe.Time;
                }
            }

            switch (pe.Type)
            {
                case _PRF.ProfilerEvent.EventType.HeapDump:
                    {
                        _PRF.HeapDump hd = (_PRF.HeapDump)pe;
                        foreach (_PRF.HeapDumpRoot hdr in hd.RootTable)
                        {
                            uint md = 0;

                            if (hdr.m_type == HeapDumpRoot.RootType.Stack)
                            {
                                /* This needs to come first because it has the side effect of writing a
                                 * method definition out if one doesn't exist for it yet.
                                 */
                                md = FindMethod(ps, hdr.m_method);
                            }

                            m_sw.Write("e 0x{0:x8} ", hdr.m_address);
                            switch (hdr.m_type)
                            {
                                case HeapDumpRoot.RootType.Finalizer:
                                    m_sw.Write("2 ");
                                    break;
                                case HeapDumpRoot.RootType.Stack:
                                    m_sw.Write("1 ");
                                    break;
                                default:
                                    m_sw.Write("0 ");
                                    break;
                            }
                            switch (hdr.m_flags)
                            {
                                case HeapDumpRoot.RootFlags.Pinned:
                                    m_sw.Write("1 ");
                                    break;
                                default:
                                    m_sw.Write("0 ");
                                    break;
                            }
                            switch (hdr.m_type)
                            {
                                case HeapDumpRoot.RootType.Stack:
                                    m_sw.WriteLine("{0}", md);
                                    break;
                                default:
                                    m_sw.WriteLine("0");
                                    break;
                            }
                        }
                        foreach (_PRF.HeapDumpObject hdo in hd.ObjectTable)
                        {
                            ulong typeid = FindType(ps, hdo.m_type);

                            m_sw.Write("o 0x{0:x8} {1} {2}", hdo.m_address, typeid, hdo.m_size);
                            if (hdo.m_references != null)
                            {
                                foreach (uint ptr in hdo.m_references)
                                {
                                    m_sw.Write(" 0x{0:x8}", ptr);
                                }
                            }
                            m_sw.WriteLine();
                        }
                        break;
                    }
                case _PRF.ProfilerEvent.EventType.Call:
                    {
                        _PRF.FunctionCall f = (_PRF.FunctionCall)pe;
                        callstack = MakeCallStack(ps, f.m_callStack);

                        m_sw.WriteLine("c {0} {1}", f.m_thread, callstack);
                        break;
                    }
                case _PRF.ProfilerEvent.EventType.Return:
                case _PRF.ProfilerEvent.EventType.ContextSwitch:
                    //The CLRProfiler does not care about function timing; it's primary goal is to display memory usage over time, and barely considers function calls
                    break;
                case _PRF.ProfilerEvent.EventType.Allocation:
                    {
                        _PRF.ObjectAllocation a = (_PRF.ObjectAllocation)pe;
                        callstack = MakeCallStack(ps, a.m_callStack, a.m_objectType, a.m_size);
                        m_sw.WriteLine("! {0} 0x{1:x} {2}", a.m_thread, a.m_address, callstack);
                        break;
                    }
                case _PRF.ProfilerEvent.EventType.Relocation:
                    {
                        _PRF.ObjectRelocation r = (_PRF.ObjectRelocation)pe;
                        for (uint i = 0; i < r.m_relocationRegions.Length; i++)
                        {
                            m_sw.WriteLine("u 0x{0:x} 0x{1:x} {2}", r.m_relocationRegions[i].m_start,
                                r.m_relocationRegions[i].m_start + r.m_relocationRegions[i].m_offset,
                                r.m_relocationRegions[i].m_end - r.m_relocationRegions[i].m_start);
                        }
                        break;
                    }
                case _PRF.ProfilerEvent.EventType.Deallocation:
                    {
                        _PRF.ObjectDeletion d = (_PRF.ObjectDeletion)pe;
                        break;
                    }
                case _PRF.ProfilerEvent.EventType.GarbageCollectionBegin:
                    {
                        _PRF.GarbageCollectionBegin gc = (_PRF.GarbageCollectionBegin)pe;
                        uint lastObjAddress = ps.m_liveObjectTable[ps.m_liveObjectTable.Count - 1] + 1;
                        m_sw.WriteLine("b 1 0 0 0x{0:x} {1} {2} 0", ps.HeapStart, lastObjAddress, ps.HeapBytesReserved);
                        break;
                    }
                case _PRF.ProfilerEvent.EventType.GarbageCollectionEnd:
                    {
                        _PRF.GarbageCollectionEnd gc = (_PRF.GarbageCollectionEnd)pe;
                        for (int i = 0; i < gc.liveObjects.Count; i++)
                        {
                            //Send length of 1 for single object, regardless of true object length.
                            m_sw.WriteLine("v 0x{0:x} 1", gc.liveObjects[i]);
                        }
                        uint lastObjAddress = ps.m_liveObjectTable[ps.m_liveObjectTable.Count - 1] + 1;
                        m_sw.WriteLine("b 0 0 0 0x{0:x} {1} {2} 0", ps.HeapStart, lastObjAddress, ps.HeapBytesReserved);
                        break;
                    }
                case _PRF.ProfilerEvent.EventType.HeapCompactionBegin:
                    {
                        _PRF.HeapCompactionBegin gc = (_PRF.HeapCompactionBegin)pe;
                        uint lastObjAddress = ps.m_liveObjectTable[ps.m_liveObjectTable.Count - 1] + 1;
                        m_sw.WriteLine("b 1 0 0 0x{0:x} {1} {2} 0", ps.HeapStart, lastObjAddress, ps.HeapBytesReserved);
                        break;
                    }
                case _PRF.ProfilerEvent.EventType.HeapCompactionEnd:
                    {
                        _PRF.HeapCompactionEnd gc = (_PRF.HeapCompactionEnd)pe;
                        uint lastObjAddress = ps.m_liveObjectTable[ps.m_liveObjectTable.Count - 1] + 1;
                        m_sw.WriteLine("b 0 0 0 0x{0:x} {1} {2} 0", ps.HeapStart, lastObjAddress, ps.HeapBytesReserved);
                        break;
                    }
            }
            m_sw.Flush();
        }

        private ulong FindType(_PRF.ProfilerSession ps, ObjectType type)
        {
            if (m_typeIdLookup.ContainsKey(type))
            {
                return m_typeIdLookup[type];
            }
            else
            {
                //FIXME: Need to know which types are finalizable
                StringBuilder typeName = new StringBuilder(ps.ResolveTypeName(type.Type));

                for (int i = 0; i < type.Rank; i++)
                {
                    typeName.Append("[]");
                }
                ulong typeid = m_nextTypeId++;
                m_typeIdLookup.Add(type, typeid);
                m_sw.WriteLine("t {0} 0 {1}", typeid, typeName.ToString());
                return typeid;
            }
        }

        private uint FindMethod(_PRF.ProfilerSession ps, uint md)
        {
            if (m_methodIdLookup.ContainsKey(md))
            {
                return m_methodIdLookup[md];
            }
            else
            {
                uint methodid = m_nextMethodId++;
                m_methodIdLookup.Add(md, methodid);
                string methodName;
                try
                {
                    methodName = ps.m_engine.GetMethodName(md, true);
                }
                catch
                {
                    methodName = "UNKNOWN METHOD";
                }
                m_sw.WriteLine("f {0} {1} (UNKNOWN_ARGUMENTS) 0 0", methodid, methodName);
                return methodid;
            }
        }

        private ulong MakeCallStack(ProfilerSession ps, uint[] callStack)
        {
            return MakeCallStackInternal(ps, callStack, "0");   //"0" means no referenced call stack or type or size.
        }

        private ulong MakeCallStack(ProfilerSession ps, uint[] callStack, ObjectType type, uint size)
        {
            ulong typeid = FindType(ps, type);
            string flags = String.Format("1 {0} {1}", typeid, size);  //"1" means a type id and size come before the call stack.
            return MakeCallStackInternal(ps, callStack, flags);
        }

        private ulong MakeCallStackInternal(ProfilerSession ps, uint[] callStack, string flags)
        {
            uint[] transStack = new uint[callStack.Length];
            ulong id = m_nextCallstackId;
            m_nextCallstackId++;

            for (uint i = 0; i < callStack.Length; i++)
            {
                transStack[i] = FindMethod(ps, callStack[i]);
            }

            m_sw.Write("n {0} {1}", id, flags);

            for (uint i = 0; i < transStack.Length; i++)
            {
                m_sw.Write(" {0}", transStack[i]);
            }
            m_sw.WriteLine();

            return id;
        }
    }
}
