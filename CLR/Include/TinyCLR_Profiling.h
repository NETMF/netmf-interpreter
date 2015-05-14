////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _TINYCLR_PROFILING_H_
#define _TINYCLR_PROFILING_H_

#if defined(TINYCLR_PROFILE_NEW)

/*
 * See RFC2234 for Augmented Backus Naur form documentation.
 * All terminal literals are bits, not ASCII characters.
 * Bits are packed such that the first bit goes to the most significant bit,
 * the second goes to the second-most significant, etc.

short    = 16BIT
int      = 32BIT
long     = 64BIT
pointer  = 32BIT
datatype = 8BIT
typedef-idx = 32BIT
method-idx  = 32BIT
    TinyCLR Method_Idx value for identifying a specific function

stream-packet = sequence-id length stream-payload
    A chunk of data sent out over the WireProtocol.

sequence-id = short
    Number used to order the packets

length = short
    Number in bits in stream-payload

stream-payload = *profiler-packet
    Payload is a sequence of profiler packets.

profiling-stream = *stream-payload
    The reconstructed stream of data to be processed.

profiler-packet = timestamp-packet / memory-layout-packet heapdump-start-packet / heapdump-root-packet /
                  heapdump-object-packet / heapdump-stop-packet

timestamp-header = "0000001"
timestamp-time = long
timestamp-packet = timestamp-header timestamp-time
    Marks time intervals to allow object lifetime estimates and histograms

memory-layout-packet = memory-layout-header memory-layout-heap-address memory-layout-heap-length
memory-layout-header = "00000010"
memory-layout-heap-address = pointer
memory-layout-heap-length = int
    Contains heap layout information useful for showing memory usage statistics.

heapdump = heapdump-start-packet *heapdump-root-packet *heapdump-object-packet heapdump-stop-packet
    Heapdumps always occur in this form. All roots are listed before all objects, and both roots and objects fall between
    a pair of 'start' and 'stop' packets. Heap dumps may be split up across stream-payload boundaries, but they may not be
    nested, and packets may not be sent in any other order.

heapdump-start-header = "00000011"
heapdump-start-packet = heapdump-start-header
    Marks the beginning of a heap dump. Multiple <heapdump-start-packets> can exist in the same profiling-stream
    if and only if there is a heapdump-stop-packet between every pair of heapdump-start-packets.

heapdump-root-packet = heapdump-root-header ( heapdump-root-stack / heapdump-root-other )
heapdump-root-header = "00000011"

heapdump-root-stack = heapdump-root-stack-header method-idx
heapdump-root-stack-header = "101"

heapdump-root-other = heapdump-root-finalizer / heapdump-root-appdomain / heapdump-root-assembly / heapdump-root-thread
heapdump-root-finalizer = "001"
heapdump-root-appdomain = "010"
heapdump-root-assembly = "011"
heapdump-root-thread = "100"

heapdump-object-packet = heapdump-object-header  heapdump-object-type heapdump-object-references
heapdump-object-header = heapdump-object-address heapdump-object-size
heapdump-object-address = pointer
heapdump-object-size = short
    Size is the number of HeapBlock chunks used, not the number of bytes used.

heapdump-object-type = heapdump-object-classvaluetype / heapdump-object-array / heapdump-object-other

heapdump-object-classvaluetype = heapdump-object-classvaluetype-header heapdump-object-classvaluetype-typedef
heapdump-object-classvaluetype-header = "00010001" / "00010010"
heapdump-object-classvaluetype-typedef = typedef-idx

heapdump-object-array = heapdump-object-array-header heapdump-object-array-datatype heapdump-object-array-levels
heapdump-object-array-header = "00010011"
heapdump-object-array-typedef = typedef-idx
heapdump-object-array-levels = short
    Rank of the array

heapdump-object-special-cases = heapdump-object-classvaluetype-header / heapdump-object-array-header
heapdump-object-other = datatype
    datatype must not be one of <heapdump-object-special-cases>; If it is, the appropriate special case should be used instead.

heapdump-object-references = *heapdump-object-reference heapdump-object-end-of-references
heapdump-object-reference = "1" pointer
heapdump-object-end-of-references = "0"

heapdump-stop-packet = heapdump-stop-header heapdump-stop-blocks-used
    Used to mark the end of a specific heap-dump. Must not follow another <heap-dump-stop-packet>
    unless another <heap-dump-start-packet> came first.

heapdump-stop-header = "00000110"
heapdump-stop-blocks-used = int
    Reports how many heap-blocks were used by objects, including objects that might have been filtered out from reporting.

 */

class CLR_PRF_CMDS
{
    public:
    static const CLR_UINT32 c_Profiling_Timestamp            = 0x01;
    static const CLR_UINT32 c_Profiling_Memory_Layout        = 0x02;

    static const CLR_UINT32 c_Profiling_HeapDump_Start       = 0x03;
    static const CLR_UINT32 c_Profiling_HeapDump_Root        = 0x04;
    static const CLR_UINT32 c_Profiling_HeapDump_Object      = 0x05;
    static const CLR_UINT32 c_Profiling_HeapDump_Stop        = 0x06;

    static const CLR_UINT32 c_Profiling_Calls_Call           = 0x07;
    static const CLR_UINT32 c_Profiling_Calls_Return         = 0x08;
    static const CLR_UINT32 c_Profiling_Calls_CtxSwitch      = 0x09;

    static const CLR_UINT32 c_Profiling_Allocs_Alloc         = 0x0a;
    static const CLR_UINT32 c_Profiling_Allocs_Relloc        = 0x0b;
    static const CLR_UINT32 c_Profiling_Allocs_Delete        = 0x0c;

    static const CLR_UINT32 c_Profiling_GarbageCollect_Begin = 0x0d;
    static const CLR_UINT32 c_Profiling_GarbageCollect_End   = 0x0e;

    static const CLR_UINT32 c_Profiling_HeapCompact_Begin    = 0x0f;
    static const CLR_UINT32 c_Profiling_HeapCompact_End      = 0x10;

    class Bits
    {
    public:
        static const CLR_UINT32 CommandHeader       = 8;
        static const CLR_UINT32 DataType            = 8;
        static const CLR_UINT32 NibbleCount         = 3;
        static const CLR_UINT32 RootTypes           = 3;
        static const CLR_UINT32 TimestampShift      = 8;
        static const CLR_UINT32 CallTimingShift     = 4;
    };

    class RootTypes
    {
        public:
        static const CLR_UINT32 Root_Finalizer = 0x01;
        static const CLR_UINT32 Root_AppDomain = 0x02;
        static const CLR_UINT32 Root_Assembly  = 0x03;
        static const CLR_UINT32 Root_Thread    = 0x04;
        static const CLR_UINT32 Root_Stack     = 0x05;
    };
};

struct CLR_PRF_Profiler
{
    // This is not implemented:
    // Write formatter functions to send only as many bits as needed for uint32,uint64 numbers, cast bytes, etc.
    static HRESULT CreateInstance();
    static HRESULT DeleteInstance();

    HRESULT Stream_Send();
    HRESULT Stream_Flush();

    HRESULT Profiler_Cleanup();
    HRESULT DumpHeap();

#if defined(TINYCLR_PROFILE_NEW_CALLS)
    HRESULT RecordContextSwitch( CLR_RT_Thread* nextThread );
    HRESULT RecordFunctionCall( CLR_RT_Thread* th, CLR_RT_MethodDef_Index md );
    HRESULT RecordFunctionReturn( CLR_RT_Thread* th, CLR_PROF_CounterCallChain& prof );
#endif

#if defined(TINYCLR_PROFILE_NEW_ALLOCATIONS)
    void TrackObjectCreation( CLR_RT_HeapBlock* ptr );
    void TrackObjectDeletion( CLR_RT_HeapBlock* ptr );
    void TrackObjectRelocation();
    void RecordGarbageCollectionBegin();
    void RecordGarbageCollectionEnd();
    void RecordHeapCompactionBegin();
    void RecordHeapCompactionEnd();
#endif

    void SendMemoryLayout();

private:
    void SendTrue();
    void SendFalse();
    void Timestamp();
    void PackAndWriteBits( CLR_UINT32 value );
    void PackAndWriteBits( const CLR_RT_TypeDef_Index& typeDef );
    void PackAndWriteBits( const CLR_RT_MethodDef_Index& methodDef );
    CLR_RT_HeapBlock* FindReferencedObject( CLR_RT_HeapBlock* ref );
    HRESULT DumpRoots();
    void DumpRoot( CLR_RT_HeapBlock* root, CLR_UINT32 type, CLR_UINT32 flags, CLR_RT_MethodDef_Index* source );
    void DumpObject( CLR_RT_HeapBlock* ptr );
    void DumpEndOfRefsList();
    void DumpPointer( void* ref );
    void DumpSingleReference( CLR_RT_HeapBlock* ptr );
    void DumpListOfReferences( CLR_RT_DblLinkedList& list );
    void DumpListOfReferences( CLR_RT_HeapBlock* firstItem, CLR_UINT16 count );

    CLR_RT_HeapBlock_MemoryStream*  m_stream;
    CLR_UINT16                      m_packetSeqId;
    CLR_UINT32                      m_lastTimestamp;
    CLR_IDX                         m_currentAssembly;
    int                             m_currentThreadPID;
};

extern CLR_PRF_Profiler g_CLR_PRF_Profiler;

#endif //defined(TINYCLR_PROFILE_NEW)
#endif // _TINYCLR_PROFILING_H_

