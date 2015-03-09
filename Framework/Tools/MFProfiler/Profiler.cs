using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

using _DBG = Microsoft.SPOT.Debugger;
using _WP  = Microsoft.SPOT.Debugger.WireProtocol;

namespace Microsoft.SPOT.Profiler
{
    // Look into ways of compacting data...

    public class ProfilerSession
    {
        internal const uint HeapBlockSize = 12;

        private bool m_connected;
        private ulong m_bitsReceived;

        private ulong m_startTime;
        private ulong m_lastKnownTime;

        //These shouldn't change once the TinyCLR starts executing managed code, correct?
        private uint m_heapStart;
        private uint m_heapLength;
        private uint m_heapBytesUsed;

        internal HeapDump m_currentHeapDump;

        internal bool m_firstPacket;
        internal ushort m_lastSeenStreamPacketID;

        internal Dictionary<uint, Stack<uint>> m_threadCallStacks;
        internal uint m_currentThreadPID;
        internal uint m_currentAssembly;

        internal List<uint> m_liveObjectTable;

        private Thread m_receiverThread;
        private _DBG.BitStream m_incomingStream;

        internal _DBG.Engine m_engine;

        public delegate void OnProfilerEventAddHandler(ProfilerSession ps, ProfilerEvent pe);
        public event OnProfilerEventAddHandler OnEventAdd;
        public event EventHandler OnDisconnect;

        public ProfilerSession(_DBG.Engine engine)
        {
            if (engine == null)
            {
                throw new ArgumentNullException();
            }

            m_connected = true;
            m_engine = engine;
            m_engine.OnCommand += new _DBG.CommandEventHandler(OnDeviceCommand);
            m_incomingStream = new _DBG.BitStream(true);

            m_startTime = 0;
            m_lastKnownTime = 0;
            m_currentHeapDump = null;
            m_threadCallStacks = new Dictionary<uint, Stack<uint>>();

            m_liveObjectTable = new List<uint>();

            m_firstPacket = true;

            m_receiverThread = new Thread(WorkerThread);
            m_receiverThread.Start();
        }

        public void EnableProfiling()
        {
            m_engine.SetProfilingMode(_WP.Commands.Profiling_Command.ChangeConditionsFlags.c_Enabled, 0);

            //Move IsDeviceInInitializeState(), IsDeviceInExitedState(), GetDeviceState(),EnsureProcessIsInInitializedState() to Debugger.dll?
            uint executionMode = 0;
            m_engine.SetExecutionMode(0, 0, out executionMode);

            //Device should be stopped when we try to build the stack traces.
            System.Diagnostics.Debug.Assert((executionMode & _WP.Commands.Debugging_Execution_ChangeConditions.c_Stopped) != 0);

            uint[] threads = m_engine.GetThreadList();
            if (threads != null)
            {
                for (int i = 0; i < threads.Length; i++)
                {
                    _WP.Commands.Debugging_Thread_Stack.Reply reply = m_engine.GetThreadStack(threads[i]);
                    if (reply != null)
                    {
                        Stack<uint> stack = new Stack<uint>();
                        for (int j = 0; j < reply.m_data.Length; j++)
                        {
                            stack.Push(reply.m_data[j].m_md);
                        }
                        m_threadCallStacks.Add(threads[i], stack);
                    }
                }
            }
        }

        public void SetProfilingOptions(bool calls, bool allocations)
        {
            uint set = 0;
            uint reset = 0;

            if (m_engine.Capabilities.ProfilingCalls)
            {
                if (calls)
                {
                    set |= _WP.Commands.Profiling_Command.ChangeConditionsFlags.c_Calls;
                }
                else
                {
                    reset |= _WP.Commands.Profiling_Command.ChangeConditionsFlags.c_Calls;
                }
            }

            if (m_engine.Capabilities.ProfilingAllocations)
            {
                if (allocations)
                {
                    set |= _WP.Commands.Profiling_Command.ChangeConditionsFlags.c_Allocations;
                }
                else
                {
                    reset |= _WP.Commands.Profiling_Command.ChangeConditionsFlags.c_Allocations;
                }
            }

            m_engine.SetProfilingMode(set, reset);
        }

        public void DisableProfiling()
        {
            m_engine.SetProfilingMode(0, 
                _WP.Commands.Profiling_Command.ChangeConditionsFlags.c_Enabled | 
                _WP.Commands.Profiling_Command.ChangeConditionsFlags.c_Calls |
                _WP.Commands.Profiling_Command.ChangeConditionsFlags.c_Allocations
                );
        }

        public void Disconnect()
        {
            if (m_connected)
            {
                m_connected = false;
                try
                {
                    DisableProfiling();
                    /* When this 'flush' returns, all data in the stream should have been sent out.
                     * so setting the end of stream marker will not cause race conditions with the WP packet receiving thread.
                     */
                    m_engine.FlushProfilingStream();
                }
                catch { /* Ignore errors if we are already disconnected from the device. */}

                m_incomingStream.MarkStreamEnd();
            }
        }

        /* Make sure that Disconnect gets called, otherwise EndOfStreamException will never get thrown and it will block forever. */
        private void WorkerThread()
        {
            while (true)
            {
                try
                {
                    Packets.ProfilerPacket pp = ProfilerPacketFactory.Decode(m_incomingStream);
                    pp.Process(this);
                    // Don't write out incomplete lines if we lose the device during processing? Insert 'safe' values instead?
                }
                catch(System.IO.IOException)
                {
                    /* The CLR is allowed to resume/quit now; we have all data we need. */
                    try
                    {
                        m_engine.ResumeExecution();
                    }
                    catch { }

                    /*We've been disconnected -- shutdown thread. */
                    if (OnDisconnect != null) { OnDisconnect(this, EventArgs.Empty); }
                    return;
                }
            }
        }

        public ulong BitsReceived
        {
            get { return m_bitsReceived; }
        }

        public ulong StartTime
        {
            get { return m_startTime; }
            internal set { m_startTime = value; }
        }

        public ulong LastKnownTime
        {
            get { return m_lastKnownTime; }
            internal set { m_lastKnownTime = value; }
        }

        public uint HeapStart
        {
            get { return m_heapStart; }
            internal set { m_heapStart = value; }
        }

        public uint HeapBytesUsed
        {
            get { return m_heapBytesUsed; }
            internal set { m_heapBytesUsed = value; }
        }

        public uint HeapBytesReserved
        {
            get { return m_heapLength; }
            internal set { m_heapLength = value; }
        }

        public uint HeapBytesFree
        {
            get { return m_heapLength - m_heapBytesUsed; }
            internal set {
                if (m_heapLength > value)
                {
                    m_heapBytesUsed = m_heapLength - value;
                }
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void OnDeviceCommand(_WP.IncomingMessage msg, bool fReply)
        {
            if (!fReply)
            {
                switch (msg.Header.m_cmd)
                {
                    case _WP.Commands.c_Profiling_Stream:
                        _WP.Commands.Profiling_Stream pay = (_WP.Commands.Profiling_Stream)msg.Payload;
                        //Some sort of packet ordering with 'pay.seqId' to gurantee packets are need to arrived in the correct order and aren't lost.
                        if (m_firstPacket)
                        {
                            m_firstPacket = false;
                            m_lastSeenStreamPacketID = pay.seqId;
                        }
                        else
                        {
                            if (pay.seqId == 0)
                            {
                                //Sometimes the device manages to send out a packet before the device restarts and manages to throw off sequence id checking.
                                m_lastSeenStreamPacketID = 0;
                            }
                            else
                            {
                                System.Diagnostics.Debug.Assert(pay.seqId == m_lastSeenStreamPacketID + 1);
                                m_lastSeenStreamPacketID = pay.seqId;
                            }
                        }
                        m_bitsReceived += pay.bitLen;
                        m_incomingStream.AppendChunk(pay.payload, 0, pay.bitLen);
                        break;
                }
            }
        }

        internal void AddEvent(ProfilerEvent pe)
        {
            pe.Time = LastKnownTime;

            //Call event to allow for output streaming.
            try
            {
                OnEventAdd(this, pe);
            }
            catch { }
        }

        internal string ResolveTypeName(uint type)
        {
            if ((type & 0xffff0000) == 0)
            {
                //We have a DataType, not a TypeDef index.
                return ((_DBG.RuntimeDataType)type).ToString();
            }
            else
            {
                return m_engine.ResolveType(type).m_name;
            }
        }
    }

    public class ProfilerEvent
    {
        private ulong m_time;

        public enum EventType
        {
            HeapDump = 0x00,
            Call = 0x01,
            Return = 0x02,
            ContextSwitch = 0x03,
            Allocation = 0x04,
            Relocation = 0x05,
            Deallocation = 0x06,
            GarbageCollectionBegin = 0x07,
            GarbageCollectionEnd = 0x08,
            HeapCompactionBegin = 0x09,
            HeapCompactionEnd = 0x0a,
        }

        public ulong Time
        {
            get { return m_time; }
            internal set { m_time = value; }
        }
        private EventType m_type;

        public EventType Type
        {
            get { return m_type; }
            set { m_type = value; }
        }
    }

    /*
     * There's got to be a better way to separate all these commands from the wire-protocol's deserialization
     * and whichever way we want to export the data.
     * It seems a combination of Model-View-Controller and Command patterns is appropriate, but I don't have time to refactor.
     */

    public class HeapDump : ProfilerEvent
    {
        internal List<HeapDumpObject> m_objectTable;
        internal List<HeapDumpRoot> m_rootTable;

        public HeapDump()
        {
            base.Type = EventType.HeapDump;
            m_objectTable = new List<HeapDumpObject>();
            m_rootTable = new List<HeapDumpRoot>();
        }

        public IList<HeapDumpObject> ObjectTable
        {
            get { return m_objectTable.AsReadOnly(); }
        }

        public IList<HeapDumpRoot> RootTable
        {
            get { return m_rootTable.AsReadOnly(); }
        }
    }

    public class HeapDumpRoot
    {
        public enum RootType
        {
            Finalizer,
            AppDomain,
            Assembly,
            Thread,
            Stack,
        }

        [Flags]
        public enum RootFlags
        {
            None = 0,
            Pinned = 1,
        }

        public uint m_address;
        public RootType m_type;
        public RootFlags m_flags;
        public uint m_method;
    }

    public class HeapDumpObject
    {
        public uint m_address;
        public ObjectType m_type;
        public uint m_size;
        public List<uint> m_references;
    }

    public class ObjectType
    {
        private uint m_type;
        private ushort m_arrayRank;

        public ObjectType(uint type)
        {
            m_type = type;
            m_arrayRank = 0;
        }

        public ObjectType(uint type, ushort arrayRank)
        {
            m_type = type;
            m_arrayRank = arrayRank;
        }

        public uint Type
        {
            get { return m_type; }
        }

        public ushort Rank
        {
            get { return m_arrayRank; }
        }

        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types.
            if (obj == null || GetType() != obj.GetType()) return false;
            ObjectType hdot = (ObjectType)obj;
            return m_type == hdot.m_type && m_arrayRank == hdot.m_arrayRank;
        }

        public override int GetHashCode()
        {
            /* From MSDN Object.GetHashCode():
             * For derived classes of Object, GetHashCode can delegate to the Object.GetHashCode implementation, if and only if
             * that derived class defines value equality to be reference equality and the type is not a value type.
             */
            return m_arrayRank.GetHashCode() ^ m_type.GetHashCode();
        }
    }

    public class FunctionCall : ProfilerEvent
    {
        public uint m_thread;
        public uint[] m_callStack;

        public FunctionCall()
        {
            base.Type = EventType.Call;
        }
    }

    public class FunctionReturn : ProfilerEvent
    {
        public uint m_thread;
        public ulong duration;

        public FunctionReturn()
        {
            base.Type = EventType.Return;
        }
    }

    public class ContextSwitch : ProfilerEvent
    {
        public uint m_thread;

        public ContextSwitch()
        {
            base.Type = EventType.ContextSwitch;
        }
    }

    public class ObjectAllocation : ProfilerEvent
    {
        public uint m_thread;
        public uint m_address;
        public uint[] m_callStack;
        public uint m_size;
        public ObjectType m_objectType;

        public ObjectAllocation()
        {
            base.Type = EventType.Allocation;
        }
    }

    public class ObjectRelocation : ProfilerEvent
    {
        public class RelocationRegion
        {
            public uint m_start;
            public uint m_end;
            public uint m_offset;
        }

        public RelocationRegion[] m_relocationRegions;

        public ObjectRelocation()
        {
            base.Type = EventType.Relocation;
        }
    }

    public class ObjectDeletion : ProfilerEvent
    {
        public uint address;

        public ObjectDeletion()
        {
            base.Type = EventType.Deallocation;
        }
    }

    public class GarbageCollectionBegin : ProfilerEvent
    {
        public GarbageCollectionBegin()
        {
            base.Type = EventType.GarbageCollectionBegin;
        }
    }

    public class GarbageCollectionEnd : ProfilerEvent
    {
        public List<uint> liveObjects;

        public GarbageCollectionEnd()
        {
            base.Type = EventType.GarbageCollectionEnd;
        }
    }

    public class HeapCompactionBegin : ProfilerEvent
    {
        public HeapCompactionBegin()
        {
            base.Type = EventType.HeapCompactionBegin;
        }
    }

    public class HeapCompactionEnd : ProfilerEvent
    {
        public HeapCompactionEnd()
        {
            base.Type = EventType.HeapCompactionEnd;
        }
    }

    public abstract class Exporter
    {
        protected FileStream m_fs;
        protected StreamWriter m_sw;
        protected string m_fileName;

        public Exporter(ProfilerSession ps, string file)
        {
            if (!String.IsNullOrEmpty(file))
            {
                m_fileName = file;
            }
            else
            {
                m_fileName = Path.GetTempFileName();
            }

            m_fs = new FileStream(m_fileName, FileMode.Create, FileAccess.Write, FileShare.Read);
            m_sw = new StreamWriter(m_fs);
        }

        public virtual void Close()
        {
            //Need a way to explicitly close the file; otherwise file doesn't get closed until finalizer runs and then user can't overwrite to the same file.
            m_sw.Close();
        }

        public string FileName
        {
            get { return m_fileName; }
        }
    }
}
