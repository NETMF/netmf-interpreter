using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

using _DBG = Microsoft.SPOT.Debugger;
using _PRF = Microsoft.SPOT.Profiler;
using _WP  = Microsoft.SPOT.Debugger.WireProtocol;

namespace Microsoft.SPOT.Profiler
{
    internal class Tracing
    {
        [Conditional("PACKET_TRACE")]
        internal static void PacketTrace(string format, params object[] args)
        {
            System.Diagnostics.Debug.Print(format, args);
        }
    }

    internal class ProfilerPacketFactory
    {
        internal static Packets.ProfilerPacket Decode(_DBG.BitStream stream)
        {
            uint type = stream.ReadBits(Packets.Commands.Bits.CommandHeader);
            Tracing.PacketTrace("New Packet {0}", type);
            switch(type)
            {
                case Packets.Commands.c_Profiling_Timestamp:
                    return new Packets.TimestampPacket(stream);
                case Packets.Commands.c_Profiling_Memory_Layout:
                    return new Packets.MemoryLayoutPacket(stream);
                case Packets.Commands.c_Profiling_HeapDump_Start:
                    return new Packets.HeapDumpStartPacket(stream);
                case Packets.Commands.c_Profiling_HeapDump_Stop:
                    return new Packets.HeapDumpStopPacket(stream);
                case Packets.Commands.c_Profiling_HeapDump_Root:
                    return new Packets.HeapDumpRootPacket(stream);
                case Packets.Commands.c_Profiling_HeapDump_Object:
                    return new Packets.HeapDumpObjectPacket(stream);
                case Packets.Commands.c_Profiling_Calls_Call:
                    return new Packets.CallPacket(stream);
                case Packets.Commands.c_Profiling_Calls_Return:
                    return new Packets.ReturnPacket(stream);
                case Packets.Commands.c_Profiling_Calls_CtxSwitch:
                    return new Packets.ContextSwitchPacket(stream);
                case Packets.Commands.c_Profiling_Allocs_Alloc:
                    return new Packets.ObjectAllocationPacket(stream);
                case Packets.Commands.c_Profiling_Allocs_Relloc:
                    return new Packets.ObjectRelocationPacket(stream);
                case Packets.Commands.c_Profiling_Allocs_Delete:
                    return new Packets.ObjectDeletionPacket(stream);
                case Packets.Commands.c_Profiling_GarbageCollect_Begin:
                    return new Packets.GarbageCollectionBeginPacket(stream);
                case Packets.Commands.c_Profiling_GarbageCollect_End:
                    return new Packets.GarbageCollectionEndPacket(stream);
                case Packets.Commands.c_Profiling_HeapCompact_Begin:
                    return new Packets.HeapCompactionBeginPacket(stream);
                case Packets.Commands.c_Profiling_HeapCompact_End:
                    return new Packets.HeapCompactionEndPacket(stream);
                default:
                    throw new ApplicationException("Unable to decode packet.");
            }
        }
    }
}

namespace Microsoft.SPOT.Profiler.Packets
{
    internal static class Commands
    {
        internal const byte c_Profiling_Timestamp            = 0x01;
        internal const byte c_Profiling_Memory_Layout        = 0x02;

        internal const byte c_Profiling_HeapDump_Start       = 0x03;
        internal const byte c_Profiling_HeapDump_Root        = 0x04;
        internal const byte c_Profiling_HeapDump_Object      = 0x05;
        internal const byte c_Profiling_HeapDump_Stop        = 0x06;

        internal const byte c_Profiling_Calls_Call           = 0x07;
        internal const byte c_Profiling_Calls_Return         = 0x08;
        internal const byte c_Profiling_Calls_CtxSwitch      = 0x09;

        internal const byte c_Profiling_Allocs_Alloc         = 0x0a;
        internal const byte c_Profiling_Allocs_Relloc        = 0x0b;
        internal const byte c_Profiling_Allocs_Delete        = 0x0c;

        internal const byte c_Profiling_GarbageCollect_Begin = 0x0d;
        internal const byte c_Profiling_GarbageCollect_End   = 0x0e;

        internal const byte c_Profiling_HeapCompact_Begin    = 0x0f;
        internal const byte c_Profiling_HeapCompact_End      = 0x10;

        internal static class RootTypes
        {
            internal const byte Root_Finalizer = 0x01;
            internal const byte Root_AppDomain = 0x02;
            internal const byte Root_Assembly  = 0x03;
            internal const byte Root_Thread    = 0x04;
            internal const byte Root_Stack     = 0x05;
        }

        internal static class Bits
        {
            internal const int AssemblyShift       = 16;
            internal const int CallTimingShift     = 4;
            internal const int CommandHeader       = 8;
            internal const int DataType            = 8;
            internal const int NibbleCount         = 3;
            internal const int RootTypes           = 3;
            internal const int TimestampShift      = 8;
        }
    }

    internal abstract class ProfilerPacket
    {
        private byte m_type;
        internal byte Type
        {
            get { return m_type; }
        }

        private   ProfilerPacket(         ) { }
        protected ProfilerPacket(byte type) { m_type = type; }

        internal abstract void Process(ProfilerSession sess);

        static protected uint ReadAndUnpackBits(_DBG.BitStream stream)
        {
            const int SHIFT_PER_NIBBLE = 2; //2^2 = 4 bits per nibble
            uint nibbles = stream.ReadBits(Packets.Commands.Bits.NibbleCount) + 1;
            return stream.ReadBits((int)nibbles << SHIFT_PER_NIBBLE);
        }

        private const int ASSEMBLY_BIT_OFFSET = 16;

        static protected uint ReadTypeDefIndex(_DBG.BitStream stream)
        {
            uint assembly = ReadAndUnpackBits(stream);
            uint type = ReadAndUnpackBits(stream);
            return assembly << ASSEMBLY_BIT_OFFSET | type;
        }

        static protected uint ReadMethodDefIndex(_DBG.BitStream stream)
        {
            uint assembly = ReadAndUnpackBits(stream);
            uint method = ReadAndUnpackBits(stream);
            return assembly << ASSEMBLY_BIT_OFFSET | method;
        }

        static protected bool ReadBoolean(_DBG.BitStream stream)
        {
            return (stream.ReadBits(1) == 1);
        }
    }

    internal class TimestampPacket : ProfilerPacket
    {
        private ulong m_time;

        public TimestampPacket(_DBG.BitStream stream) : base(Commands.c_Profiling_Timestamp)
        {
            ulong l = (ulong)ReadAndUnpackBits(stream) << Packets.Commands.Bits.TimestampShift;
            m_time = l / TimeSpan.TicksPerMillisecond;
            Tracing.PacketTrace("time {0}", m_time);
        }

        internal override void Process(ProfilerSession sess)
        {
            sess.LastKnownTime += m_time;
        }
    }

    internal class MemoryLayoutPacket : ProfilerPacket
    {
        private uint m_heapAddress;
        private uint m_heapLength;

        public MemoryLayoutPacket(_DBG.BitStream stream) : base(Commands.c_Profiling_Memory_Layout)
        {
            m_heapAddress = ReadAndUnpackBits(stream);
            m_heapLength  = ReadAndUnpackBits(stream);
            Tracing.PacketTrace("layout {0}:{1}", m_heapAddress, m_heapLength);
        }

        internal override void Process(ProfilerSession sess)
        {
            sess.HeapStart = m_heapAddress;
            sess.HeapBytesReserved = m_heapLength;
        }
    }

    internal class HeapDumpStartPacket : ProfilerPacket
    {

        public HeapDumpStartPacket(_DBG.BitStream stream)
            : base(Commands.c_Profiling_HeapDump_Start) { }

        internal override void Process(ProfilerSession sess)
        {
            if(sess.m_currentHeapDump != null)
            {
                //We lost a heap-dump end packet somewhere.
                //Insert one to clean up the last heap dump.
                HeapDumpStopPacket hdep = new HeapDumpStopPacket(null);
                hdep.Process(sess);
            }

            // Heap dump should update live object table.

            sess.m_currentHeapDump = new HeapDump();
            Tracing.PacketTrace("HEAP DUMP BEGIN======================>");
        }
    }

    internal class HeapDumpStopPacket : ProfilerPacket
    {
        private uint m_heapBytesUsed;

        public HeapDumpStopPacket(_DBG.BitStream stream)
            : base(Commands.c_Profiling_HeapDump_Stop)
        {
             m_heapBytesUsed = ReadAndUnpackBits(stream);
        }

        internal override void Process(ProfilerSession sess)
        {
            if (sess.m_currentHeapDump == null) { return; } //No heap dump to finish

            sess.HeapBytesUsed = m_heapBytesUsed;
            sess.AddEvent(sess.m_currentHeapDump);
            sess.m_currentHeapDump = null;
            Tracing.PacketTrace("<======================HEAP DUMP END");
        }
    }

    internal class HeapDumpRootPacket : ProfilerPacket
    {
        private uint m_address;
        private uint m_source;
        private uint m_method;

        public HeapDumpRootPacket(_DBG.BitStream stream)
            : base(Commands.c_Profiling_HeapDump_Root)
        {
            m_address = ReadAndUnpackBits(stream);
            m_source = stream.ReadBits(Packets.Commands.Bits.RootTypes);
            switch(m_source)
            {
                case Packets.Commands.RootTypes.Root_Stack:
                    m_method = ReadMethodDefIndex(stream);
                    break;
            }
            Tracing.PacketTrace("root type:{0} at {1}", m_source, m_address);
        }

        internal override void Process(ProfilerSession sess)
        {
            if (sess.m_currentHeapDump == null)
            {
                //Lost heap-dump start packet.
                throw new System.IO.IOException();
            }

            HeapDumpRoot hdr = new HeapDumpRoot();
            hdr.m_address = m_address;
            switch(m_source)
            {
                case Packets.Commands.RootTypes.Root_AppDomain:
                    hdr.m_type = HeapDumpRoot.RootType.AppDomain;
                    break;
                case Packets.Commands.RootTypes.Root_Assembly:
                    hdr.m_type = HeapDumpRoot.RootType.Assembly;
                    break;
                case Packets.Commands.RootTypes.Root_Finalizer:
                    hdr.m_type = HeapDumpRoot.RootType.Finalizer;
                    break;
                case Packets.Commands.RootTypes.Root_Stack:
                    hdr.m_type = HeapDumpRoot.RootType.Stack;
                    break;
                case Packets.Commands.RootTypes.Root_Thread:
                    hdr.m_type = HeapDumpRoot.RootType.Thread;
                    break;
            }
            hdr.m_flags = HeapDumpRoot.RootFlags.None;
            hdr.m_method = m_method;

            sess.m_currentHeapDump.m_rootTable.Add(hdr);
        }
    }

    internal class HeapDumpObjectPacket : ProfilerPacket
    {
        private uint m_address;
        private uint m_size;
        private _DBG.RuntimeDataType m_dt;
        private uint m_typedef;
        
        private ushort m_arrayLevels;
        private uint m_arrayElementType;

        private List<uint> m_refs;

        public HeapDumpObjectPacket(_DBG.BitStream stream)
            : base(Commands.c_Profiling_HeapDump_Object)
        {
            m_address = ReadAndUnpackBits(stream);
            m_size = ReadAndUnpackBits(stream) * ProfilerSession.HeapBlockSize;
            m_dt = (_DBG.RuntimeDataType)stream.ReadBits(Commands.Bits.DataType);
            if (m_dt == _DBG.RuntimeDataType.DATATYPE_CLASS || m_dt == _DBG.RuntimeDataType.DATATYPE_VALUETYPE)
            {
                m_typedef = ReadTypeDefIndex(stream);
            }
            else if (m_dt == _DBG.RuntimeDataType.DATATYPE_SZARRAY)
            {
                m_arrayElementType = ReadTypeDefIndex(stream);
                m_arrayLevels = (ushort)ReadAndUnpackBits(stream);
            }

            m_refs = new List<uint>();

            bool moreRefs;
            while (moreRefs = ReadBoolean(stream))
            {
                m_refs.Add(ReadAndUnpackBits(stream));
            }
        }

        internal override void Process(ProfilerSession sess)
        {
            if (sess.m_currentHeapDump == null)
            {
                //Lost heap-dump start packet, and probably the roots too.
                throw new System.IO.IOException();
            }

            HeapDumpObject hdo = new HeapDumpObject();
            hdo.m_address = m_address;
            hdo.m_size = m_size;

            if(m_dt == _DBG.RuntimeDataType.DATATYPE_CLASS || m_dt == _DBG.RuntimeDataType.DATATYPE_VALUETYPE)
            {
                sess.ResolveTypeName(m_typedef); //Cache type name.
                hdo.m_type = new ObjectType(m_typedef);
            } else {
                _DBG.RuntimeDataType dt = (_DBG.RuntimeDataType)m_dt;
                if (dt == _DBG.RuntimeDataType.DATATYPE_SZARRAY)
                {
                    sess.ResolveTypeName(m_arrayElementType); //Cache type name.
                    hdo.m_type = new ObjectType(m_arrayElementType, m_arrayLevels);
                }
                else
                {
                    hdo.m_type = new ObjectType((uint)m_dt);
                }
            }

            hdo.m_references = m_refs;

            Tracing.PacketTrace("object @ {0} ({1})", m_address, hdo.m_type);

            sess.m_currentHeapDump.m_objectTable.Add(hdo);
        }
    }

    internal class CallPacket : ProfilerPacket
    {
        private uint m_thread;
        private uint m_assembly;
        private uint m_method;

        public CallPacket(_DBG.BitStream stream)
            : base(Commands.c_Profiling_Calls_Call)
        {
            if (ReadBoolean(stream))
            {
                m_assembly = ReadAndUnpackBits(stream);
                Debug.Assert(m_assembly != 0);
            }
            m_method = ReadAndUnpackBits(stream);
        }

        internal override void Process(ProfilerSession sess)
        {
            m_thread = sess.m_currentThreadPID;

            if (m_assembly == 0)
            {
                m_assembly = sess.m_currentAssembly;
            }
            else
            {
                sess.m_currentAssembly = m_assembly;
            }

            uint md = m_assembly << Packets.Commands.Bits.AssemblyShift | m_method;

            Tracing.PacketTrace("CALLS: Thread {0} called function {1}", m_thread, md);
            if (!sess.m_threadCallStacks.ContainsKey(m_thread))
            {
                sess.m_threadCallStacks.Add(m_thread, new Stack<uint>());
            }
            sess.m_threadCallStacks[m_thread].Push(md);

            FunctionCall f = new FunctionCall();
            f.m_thread = m_thread;
            f.m_callStack = sess.m_threadCallStacks[m_thread].ToArray();
            Array.Reverse(f.m_callStack);
            sess.m_engine.GetMethodName(md, true);    //Cache method name information while we still have a device.
            sess.AddEvent(f);
        }
    }

    internal class ReturnPacket : ProfilerPacket
    {
        private ulong m_duration;

        public ReturnPacket(_DBG.BitStream stream)
            : base(Commands.c_Profiling_Calls_Call)
        {
            m_duration = (ulong)ReadAndUnpackBits(stream) << Packets.Commands.Bits.CallTimingShift;
        }

        internal override void Process(ProfilerSession sess)
        {
            Tracing.PacketTrace("CALLS: Function returned on thread {0}", sess.m_currentThreadPID);

            if (sess.m_threadCallStacks.ContainsKey(sess.m_currentThreadPID))
            {
                Debug.Assert(sess.m_threadCallStacks[sess.m_currentThreadPID].Count > 0);
                sess.m_threadCallStacks[sess.m_currentThreadPID].Pop();
            }

            FunctionReturn f = new FunctionReturn();
            f.m_thread = sess.m_currentThreadPID;
            f.duration = m_duration;
            sess.AddEvent(f);
        }
    }

    internal class ContextSwitchPacket : ProfilerPacket
    {
        private uint m_thread;

        public ContextSwitchPacket(_DBG.BitStream stream)
            : base(Commands.c_Profiling_Calls_Call)
        {
            m_thread = ReadAndUnpackBits(stream);
        }

        internal override void Process(ProfilerSession sess)
        {
            Tracing.PacketTrace("CALLS: Switched to thread {0}", m_thread);
            ContextSwitch c = new ContextSwitch();
            c.m_thread = m_thread;
            sess.m_currentThreadPID = m_thread;
            sess.AddEvent(c);
        }
    }

    internal class ObjectAllocationPacket : ProfilerPacket
    {
        private uint m_address;
        private uint m_size;
        private _DBG.RuntimeDataType m_dt;
        private uint m_type;
        private ushort m_rank;

        public ObjectAllocationPacket(_DBG.BitStream stream)
            : base(Commands.c_Profiling_Allocs_Alloc)
        {
            m_address = ReadAndUnpackBits(stream);
            m_size = ReadAndUnpackBits(stream) * ProfilerSession.HeapBlockSize;
            m_dt = (_DBG.RuntimeDataType)stream.ReadBits(Commands.Bits.DataType);
            if (m_dt == _DBG.RuntimeDataType.DATATYPE_CLASS || m_dt == _DBG.RuntimeDataType.DATATYPE_VALUETYPE ||
                m_dt == _DBG.RuntimeDataType.DATATYPE_SZARRAY)
            {
                m_type = ReadTypeDefIndex(stream);
                if (m_dt == _DBG.RuntimeDataType.DATATYPE_SZARRAY)
                {
                    m_rank = (ushort)ReadAndUnpackBits(stream);
                }
            }
            else
            {
                m_type = (uint)m_dt;
            }
        }

        internal override void Process(ProfilerSession sess)
        {
            Tracing.PacketTrace("ALLOC: Object allocated at address {0}", m_address);
            ObjectAllocation alloc = new ObjectAllocation();
            alloc.m_thread = sess.m_currentThreadPID;
            alloc.m_address = m_address;
            alloc.m_size = m_size;
            if (!sess.m_threadCallStacks.ContainsKey(sess.m_currentThreadPID))
            {
                sess.m_threadCallStacks.Add(sess.m_currentThreadPID, new Stack<uint>());
            }
            alloc.m_callStack = sess.m_threadCallStacks[sess.m_currentThreadPID].ToArray();
            Array.Reverse(alloc.m_callStack);
            sess.ResolveTypeName(m_type);   //Cache type name.

            if (sess.m_liveObjectTable.BinarySearch(m_address) < 0)
            {
                sess.m_liveObjectTable.Add(m_address);
                sess.m_liveObjectTable.Sort();
            }

            alloc.m_objectType = new ObjectType(m_type, m_rank);
            sess.AddEvent(alloc);
        }
    }

    internal class ObjectRelocationPacket : ProfilerPacket
    {
        private ObjectRelocation.RelocationRegion[] reloc;


        public ObjectRelocationPacket(_DBG.BitStream stream)
            : base(Commands.c_Profiling_Allocs_Relloc)
        {
            uint count = ReadAndUnpackBits(stream);
            reloc = new ObjectRelocation.RelocationRegion[count];

            for (uint i = 0; i < count; i++)
            {
                reloc[i] = new ObjectRelocation.RelocationRegion();
                reloc[i].m_start = ReadAndUnpackBits(stream);
                reloc[i].m_end = ReadAndUnpackBits(stream);
                reloc[i].m_offset = ReadAndUnpackBits(stream);
            }
        }

        internal override void Process(ProfilerSession sess)
        {
            Tracing.PacketTrace("ALLOC: Objects relocated");

            List<uint> newTable = new List<uint>();
            for (int i = 0; i < sess.m_liveObjectTable.Count; i++ )
            {
                uint ptr = sess.m_liveObjectTable[i];
                uint j;
                for (j = 0; j < reloc.Length; j++)
                {
                    if (ptr >= reloc[j].m_start && ptr <= reloc[j].m_end)
                    {
                        newTable.Add(ptr + reloc[j].m_offset);
                        break;
                    }
                }
                if (j == reloc.Length)
                {
                    //No relocation for this object.
                    newTable.Add(ptr);
                }
            }
            newTable.Sort();
            sess.m_liveObjectTable = newTable;

            ObjectRelocation or = new ObjectRelocation();
            or.m_relocationRegions = reloc;
            sess.AddEvent(or);
        }
    }

    internal class ObjectDeletionPacket : ProfilerPacket
    {
        private uint m_address;

        public ObjectDeletionPacket(_DBG.BitStream stream)
            : base(Commands.c_Profiling_Allocs_Delete)
        {
            m_address = ReadAndUnpackBits(stream);
        }

        internal override void Process(ProfilerSession sess)
        {
            Tracing.PacketTrace("ALLOC: Object freed from address {0}", m_address);

            int objIndex = sess.m_liveObjectTable.BinarySearch(m_address);
            if (objIndex > -1)
            {
                sess.m_liveObjectTable.RemoveAt(objIndex);

                ObjectDeletion delete = new ObjectDeletion();
                delete.address = m_address;
                sess.AddEvent(delete);
            }
        }
    }

    internal class GarbageCollectionBeginPacket : ProfilerPacket
    {
        private uint m_freeBytes;

        public GarbageCollectionBeginPacket(_DBG.BitStream stream)
            : base(Commands.c_Profiling_GarbageCollect_Begin)
        {
            m_freeBytes = ReadAndUnpackBits(stream);
        }

        internal override void Process(ProfilerSession sess)
        {
            Tracing.PacketTrace("GARBAGE COLLECTION BEGIN >>>");

            sess.HeapBytesFree = m_freeBytes;

            GarbageCollectionBegin gc = new GarbageCollectionBegin();
            sess.AddEvent(gc);
        }
    }

    internal class GarbageCollectionEndPacket : ProfilerPacket
    {
        private uint m_freeBytes;

        public GarbageCollectionEndPacket(_DBG.BitStream stream)
            : base(Commands.c_Profiling_GarbageCollect_End)
        {
            m_freeBytes = ReadAndUnpackBits(stream);
        }

        internal override void Process(ProfilerSession sess)
        {
            Tracing.PacketTrace("<<< GARBAGE COLLECTION BEGIN");

            sess.HeapBytesFree = m_freeBytes;

            GarbageCollectionEnd gc = new GarbageCollectionEnd();
            gc.liveObjects = new List<uint>(sess.m_liveObjectTable);
            sess.AddEvent(gc);
        }
    }
    
    internal class HeapCompactionBeginPacket : ProfilerPacket
    {
        private uint m_freeBytes;

        public HeapCompactionBeginPacket(_DBG.BitStream stream)
            : base(Commands.c_Profiling_HeapCompact_Begin)
        {
            m_freeBytes = ReadAndUnpackBits(stream);
        }

        internal override void Process(ProfilerSession sess)
        {
            Tracing.PacketTrace("HEAP COMPACTION BEGIN >>>");

            sess.HeapBytesFree = m_freeBytes;

            HeapCompactionBegin hc = new HeapCompactionBegin();
            sess.AddEvent(hc);
        }
    }

    internal class HeapCompactionEndPacket : ProfilerPacket
    {
        private uint m_freeBytes;

        public HeapCompactionEndPacket(_DBG.BitStream stream)
            : base(Commands.c_Profiling_HeapCompact_End)
        {
            m_freeBytes = ReadAndUnpackBits(stream);
        }

        internal override void Process(ProfilerSession sess)
        {
            Tracing.PacketTrace("<<< HEAP COMPACTION END");

            // on CLR Profiler side: Need to preserve objects not relocated.

            sess.HeapBytesFree = m_freeBytes;

            HeapCompactionEnd hc = new HeapCompactionEnd();
            sess.AddEvent(hc);
        }
    }
}