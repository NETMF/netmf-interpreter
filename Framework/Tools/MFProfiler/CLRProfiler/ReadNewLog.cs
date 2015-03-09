////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CLRProfiler
{
    internal class Histogram
    {
        internal int[] typeSizeStacktraceToCount;
        internal ReadNewLog readNewLog;

        internal Histogram(ReadNewLog readNewLog)
        {
            typeSizeStacktraceToCount = new int[10];
            this.readNewLog = readNewLog;
        }

        // Keep track of when this heapdump was taken
        internal int tickIndex;

        internal Histogram(ReadNewLog readNewLog, int tickindex)
        {
            typeSizeStacktraceToCount = new int[10];
            this.readNewLog = readNewLog;
            this.tickIndex = tickindex;
        }
        internal void AddObject(int typeSizeStacktraceIndex, int count)
        {
            while (typeSizeStacktraceIndex >= typeSizeStacktraceToCount.Length)
                typeSizeStacktraceToCount = ReadNewLog.GrowIntVector(typeSizeStacktraceToCount);
            typeSizeStacktraceToCount[typeSizeStacktraceIndex] += count;
        }

        internal bool Empty
        {
            get
            {
                foreach (int count in typeSizeStacktraceToCount)
                    if (count != 0)
                        return false;
                return true;
            }
        }

        internal int BuildVertexStack(int stackTraceIndex, Vertex[] funcVertex, ref Vertex[] vertexStack, int skipCount)
        {
            int[] stackTrace = readNewLog.stacktraceTable.IndexToStacktrace(stackTraceIndex);
                
            while (vertexStack.Length < stackTrace.Length + 3)
            {
                vertexStack = new Vertex[vertexStack.Length*2];
            }

            for (int i = skipCount; i < stackTrace.Length; i++)
            {
                vertexStack[i-skipCount] = funcVertex[stackTrace[i]];
            }

            return stackTrace.Length - skipCount;
        }

        internal void BuildAllocationTrace(Graph graph, int stackTraceIndex, int typeIndex, ulong size, Vertex[] typeVertex, Vertex[] funcVertex, ref Vertex[] vertexStack, FilterForm filterForm)
        {
            int stackPtr = BuildVertexStack(stackTraceIndex, funcVertex, ref vertexStack, 2);

            Vertex toVertex = graph.TopVertex;
            Vertex fromVertex;
            Edge edge;
            if ((typeVertex[typeIndex].interestLevel & InterestLevel.Interesting) == InterestLevel.Interesting
                && ReadNewLog.InterestingCallStack(vertexStack, stackPtr, filterForm))
            {
                vertexStack[stackPtr] = typeVertex[typeIndex];
                stackPtr++;
                stackPtr = ReadNewLog.FilterVertices(vertexStack, stackPtr);
                stackPtr = Vertex.SqueezeOutRepetitions(vertexStack, stackPtr);
                for (int i = 0; i < stackPtr; i++)
                {
                    fromVertex = toVertex;
                    toVertex = vertexStack[i];
                    edge = graph.FindOrCreateEdge(fromVertex, toVertex);
                    edge.AddWeight(size);
                }
                fromVertex = toVertex;
                toVertex = graph.BottomVertex;
                edge = graph.FindOrCreateEdge(fromVertex, toVertex);
                edge.AddWeight(size);
            }
        }

        internal void BuildAssemblyTrace(Graph graph, int stackTraceIndex, Vertex assembly, Vertex typeVertex, Vertex[] funcVertex, ref Vertex[] vertexStack)
        {
            int stackPtr = BuildVertexStack(Math.Abs(stackTraceIndex), funcVertex, ref vertexStack, stackTraceIndex < 0 ? 2 : 0);

            Vertex toVertex = graph.TopVertex;
            Vertex fromVertex;
            Edge edge;

            if(typeVertex != null)
            {
                vertexStack[stackPtr++] = typeVertex;
            }
            vertexStack[stackPtr++] = assembly;

            stackPtr = ReadNewLog.FilterVertices(vertexStack, stackPtr);
            stackPtr = Vertex.SqueezeOutRepetitions(vertexStack, stackPtr);
            for (int i = 0; i < stackPtr; i++)
            {
                fromVertex = toVertex;
                toVertex = vertexStack[i];
                edge = graph.FindOrCreateEdge(fromVertex, toVertex);
                edge.AddWeight(1);
            }
            fromVertex = toVertex;
            toVertex = graph.BottomVertex;
            edge = graph.FindOrCreateEdge(fromVertex, toVertex);
            edge.AddWeight(1);
        }

        internal void BuildCallTrace(Graph graph, int stackTraceIndex, Vertex[] funcVertex, ref Vertex[] vertexStack, int count, FilterForm filterForm)
        {
            int stackPtr = BuildVertexStack(stackTraceIndex, funcVertex, ref vertexStack, 0);

            Vertex toVertex = graph.TopVertex;
            Vertex fromVertex;
            Edge edge;
            if (ReadNewLog.InterestingCallStack(vertexStack, stackPtr, filterForm))
            {
                stackPtr = ReadNewLog.FilterVertices(vertexStack, stackPtr);
                stackPtr = Vertex.SqueezeOutRepetitions(vertexStack, stackPtr);
                for (int i = 0; i < stackPtr; i++)
                {
                    fromVertex = toVertex;
                    toVertex = vertexStack[i];
                    edge = graph.FindOrCreateEdge(fromVertex, toVertex);
                    edge.AddWeight((uint)count);
                }
            }
        }

        internal void BuildHandleAllocationTrace(Graph graph, int stackTraceIndex, uint count, Vertex[] funcVertex, ref Vertex[] vertexStack, FilterForm filterForm)
        {
            int stackPtr = BuildVertexStack(stackTraceIndex, funcVertex, ref vertexStack, 0);

            Vertex handleVertex = graph.FindOrCreateVertex("Handle", null, null);
            handleVertex.interestLevel = InterestLevel.Interesting;

            Vertex toVertex = graph.TopVertex;
            Vertex fromVertex;
            Edge edge;
            if (ReadNewLog.InterestingCallStack(vertexStack, stackPtr, filterForm))
            {
                vertexStack[stackPtr] = handleVertex;
                stackPtr++;
                stackPtr = ReadNewLog.FilterVertices(vertexStack, stackPtr);
                stackPtr = Vertex.SqueezeOutRepetitions(vertexStack, stackPtr);
                for (int i = 0; i < stackPtr; i++)
                {
                    fromVertex = toVertex;
                    toVertex = vertexStack[i];
                    edge = graph.FindOrCreateEdge(fromVertex, toVertex);
                    edge.AddWeight(count);
                }
                fromVertex = toVertex;
                toVertex = graph.BottomVertex;
                edge = graph.FindOrCreateEdge(fromVertex, toVertex);
                edge.AddWeight(count);
            }
        }

        internal void BuildTypeVertices(Graph graph, ref Vertex[] typeVertex, FilterForm filterForm)
        {
            for (int i = 0; i < readNewLog.typeName.Length; i++)
            {
                string typeName = readNewLog.typeName[i];
                if (typeName == null)
                    typeName = string.Format("???? type {0}", i);
                readNewLog.AddTypeVertex(i, typeName, graph, ref typeVertex, filterForm);
            }
        }

        internal int BuildAssemblyVertices(Graph graph, ref Vertex[] typeVertex, FilterForm filterForm)
        {
            int count = 0;
            foreach(string c in readNewLog.assemblies.Keys)
            {
                readNewLog.AddTypeVertex(count++, c, graph, ref typeVertex, filterForm);
            }
            return count;
        }

        internal void BuildFuncVertices(Graph graph, ref Vertex[] funcVertex, FilterForm filterForm)
        {
            for (int i = 0; i < readNewLog.funcName.Length; i++)
            {
                string name = readNewLog.funcName[i];
                string signature = readNewLog.funcSignature[i];
                if (name == null)
                    name = string.Format("???? function {0}", i);
                if (signature == null)
                    signature = "( ???????? )";
                readNewLog.AddFunctionVertex(i, name, signature, graph, ref funcVertex, filterForm);
            }
        }

        internal Graph BuildAllocationGraph(FilterForm filterForm)
        {
            Vertex[] typeVertex = new Vertex[1];
            Vertex[] funcVertex = new Vertex[1];
            Vertex[] vertexStack = new Vertex[1];

            Graph graph = new Graph(this);
            graph.graphType = Graph.GraphType.AllocationGraph;

            BuildTypeVertices(graph, ref typeVertex, filterForm);
            BuildFuncVertices(graph, ref funcVertex, filterForm);

            for (int i = 0; i < typeSizeStacktraceToCount.Length; i++)
            {
                if (typeSizeStacktraceToCount[i] > 0)
                {
                    int[] stacktrace = readNewLog.stacktraceTable.IndexToStacktrace(i);

                    int typeIndex = stacktrace[0];
                    ulong size = (ulong)stacktrace[1] * (ulong)typeSizeStacktraceToCount[i];

                    BuildAllocationTrace(graph, i, typeIndex, size, typeVertex, funcVertex, ref vertexStack, filterForm);
                }
            }

            foreach (Vertex v in graph.vertices.Values)
                v.active = true;
            graph.BottomVertex.active = false;

            return graph;
        }

        internal Graph BuildAssemblyGraph(FilterForm filterForm)
        {
            Vertex[] assemblyVertex = new Vertex[1];
            Vertex[] funcVertex = new Vertex[1];
            Vertex[] typeVertex = new Vertex[1];
            Vertex[] vertexStack = new Vertex[1];

            Graph graph = new Graph(this);
            graph.graphType = Graph.GraphType.AssemblyGraph;

            int count = BuildAssemblyVertices(graph, ref assemblyVertex, filterForm);
            BuildTypeVertices(graph, ref typeVertex, filterForm);
            BuildFuncVertices(graph, ref funcVertex, filterForm);

            for(int i = 0; i < count; i++)
            {
                Vertex v = (Vertex)assemblyVertex[i], tv = null;

                string c = v.name;
                int stackid = readNewLog.assemblies[c];
                if(stackid < 0)
                {
                    int[] stacktrace = readNewLog.stacktraceTable.IndexToStacktrace(-stackid);
                    tv = typeVertex[stacktrace[0]];
                }
                BuildAssemblyTrace(graph, stackid, v, tv, funcVertex, ref vertexStack);
            }

            foreach (Vertex v in graph.vertices.Values)
            {
                v.active = true;
            }
            graph.BottomVertex.active = false;
            return graph;
        }

        internal Graph BuildCallGraph(FilterForm filterForm)
        {
            Vertex[] funcVertex = new Vertex[1];
            Vertex[] vertexStack = new Vertex[1];

            Graph graph = new Graph(this);
            graph.graphType = Graph.GraphType.CallGraph;

            BuildFuncVertices(graph, ref funcVertex, filterForm);

            for (int i = 0; i < typeSizeStacktraceToCount.Length; i++)
            {
                if (typeSizeStacktraceToCount[i] > 0)
                {
                    int[] stacktrace = readNewLog.stacktraceTable.IndexToStacktrace(i);

                    BuildCallTrace(graph, i, funcVertex, ref vertexStack, typeSizeStacktraceToCount[i], filterForm);
                }
            }

            foreach (Vertex v in graph.vertices.Values)
                v.active = true;
            graph.BottomVertex.active = false;

            return graph;
        }

        internal void CalculateCallCounts(uint[] callCount)
        {
            for (int i = 0; i < typeSizeStacktraceToCount.Length; i++)
            {
                if (typeSizeStacktraceToCount[i] > 0)
                {
                    int[] stacktrace = readNewLog.stacktraceTable.IndexToStacktrace(i);

                    callCount[stacktrace[stacktrace.Length-1]] += (uint)typeSizeStacktraceToCount[i];
                }
            }
        }

        internal Graph BuildHandleAllocationGraph(FilterForm filterForm)
        {
            Vertex[] funcVertex = new Vertex[1];
            Vertex[] vertexStack = new Vertex[1];

            Graph graph = new Graph(this);
            graph.graphType = Graph.GraphType.HandleAllocationGraph;

            BuildFuncVertices(graph, ref funcVertex, filterForm);

            for (int i = 0; i < typeSizeStacktraceToCount.Length; i++)
            {
                if (typeSizeStacktraceToCount[i] > 0)
                {
                    int[] stacktrace = readNewLog.stacktraceTable.IndexToStacktrace(i);

                    uint count = (uint)typeSizeStacktraceToCount[i];

                    BuildHandleAllocationTrace(graph, i, count, funcVertex, ref vertexStack, filterForm);
                }
            }

            foreach (Vertex v in graph.vertices.Values)
                v.active = true;
            graph.BottomVertex.active = false;

            return graph;
        }

    }

    internal class SampleObjectTable
    {
        internal class SampleObject
        {
            internal int typeIndex;
            internal int changeTickIndex;
            internal int origAllocTickIndex;
            internal SampleObject prev;

            internal SampleObject(int typeIndex, int changeTickIndex, int origAllocTickIndex, SampleObject prev)
            {
                this.typeIndex = typeIndex;
                this.changeTickIndex = changeTickIndex;
                this.origAllocTickIndex = origAllocTickIndex;
                this.prev = prev;
            }
        }

        internal SampleObject[][] masterTable;
        internal ReadNewLog readNewLog;

        internal const int firstLevelShift = 25;
        internal const int initialFirstLevelLength = 1<<(31-firstLevelShift); // covering 2 GB of address space
        internal const int secondLevelShift = 10;
        internal const int secondLevelLength = 1<<(firstLevelShift-secondLevelShift);
        internal const int sampleGrain = 1<<secondLevelShift;
        internal int lastTickIndex;
        internal SampleObject gcTickList;

        void GrowMasterTable()
        {
            SampleObject[][] newMasterTable = new SampleObject[masterTable.Length * 2][];
            for (int i = 0; i < masterTable.Length; i++)
                newMasterTable[i] = masterTable[i];
            masterTable = newMasterTable;
        }

        internal SampleObjectTable(ReadNewLog readNewLog)
        {
            masterTable = new SampleObject[initialFirstLevelLength][];
            this.readNewLog = readNewLog;
            lastTickIndex = 0;
            gcTickList = null;
        }

        bool IsGoodSample(ulong start, ulong end)
        {
            // We want it as a sample if and only if it crosses a boundary
            return (start >> secondLevelShift) != (end >> secondLevelShift);
        }

        internal void RecordChange(ulong start, ulong end, int changeTickIndex, int origAllocTickIndex, int typeIndex)
        {
            lastTickIndex = changeTickIndex;
            for (ulong id = start; id < end; id += sampleGrain)
            {
                uint index = (uint)(id >> firstLevelShift);
                while (masterTable.Length <= index)
                    GrowMasterTable();
                SampleObject[] so = masterTable[index];
                if (so == null)
                {
                    so = new SampleObject[secondLevelLength];
                    masterTable[index] = so;
                }
                index = (uint)((id >> secondLevelShift) & (secondLevelLength-1));
                Debug.Assert(so[index] == null || so[index].changeTickIndex <= changeTickIndex);
                SampleObject prev = so[index];
                if (prev != null && prev.typeIndex == typeIndex && prev.origAllocTickIndex == origAllocTickIndex)
                {
                    // no real change - can happen when loading files where allocation profiling was off
                    // to conserve memory, don't allocate a new sample object in this case
                }
                else
                {
                    so[index] = new SampleObject(typeIndex, changeTickIndex, origAllocTickIndex, prev);
                }
            }
        }

        internal void Insert(ulong start, ulong end, int changeTickIndex, int origAllocTickIndex, int typeIndex)
        {
            if (IsGoodSample(start, end))
                RecordChange(start, end, changeTickIndex, origAllocTickIndex, typeIndex);
        }

        internal void Delete(ulong start, ulong end, int changeTickIndex)
        {
            if (IsGoodSample(start, end))
                RecordChange(start, end, changeTickIndex, 0, 0);
        }

        internal void AddGcTick(int tickIndex, int gen)
        {
            lastTickIndex = tickIndex;

            gcTickList = new SampleObject(gen, tickIndex, 0, gcTickList);
        }

        internal void RecordComment(int tickIndex, int commentIndex)
        {
            lastTickIndex = tickIndex;
        }
    }

    internal class LiveObjectTable
    {
        internal struct LiveObject
        {
            internal ulong id;
            internal uint size;
            internal int typeIndex;
            internal int typeSizeStacktraceIndex;
            internal int allocTickIndex;
        }

        class IntervalTable
        {
            class Interval
            {
                internal ulong loAddr;
                internal ulong hiAddr;
                internal int generation;
                internal Interval next;
                internal bool hadRelocations;
                internal bool justHadGc;

                internal Interval(ulong loAddr, ulong hiAddr, int generation, Interval next)
                {
                    this.loAddr = loAddr;
                    this.hiAddr = hiAddr;
                    this.generation = generation;
                    this.next = next;
                }
            }

            const int allowableGap = 1024*1024;

            Interval liveRoot;
            Interval newLiveRoot;
            Interval updateRoot;
            bool nullRelocationsSeen;

            LiveObjectTable liveObjectTable;

            internal IntervalTable(LiveObjectTable liveObjectTable)
            {
                liveRoot = null;
                this.liveObjectTable = liveObjectTable;
            }

            private Interval OverlappingInterval(Interval i)
            {
                for (Interval ii = liveRoot; ii != null; ii = ii.next)
                {
                    if (ii != i)
                    {
                        if (ii.hiAddr > i.loAddr && ii.loAddr < i.hiAddr)
                            return ii;
                    }
                }
                return null;
            }

            private void DeleteInterval(Interval i)
            {   
                Interval prevInterval = null;
                for (Interval ii = liveRoot; ii != null; ii = ii.next)
                {
                    if (ii == i)
                    {
                        if (prevInterval != null)
                            prevInterval.next = ii.next;
                        else
                            liveRoot = ii.next;
                        break;
                    }
                    prevInterval = ii;
                }
            }

            private void MergeInterval(Interval i)
            {
                Interval overlappingInterval = OverlappingInterval(i);
                i.loAddr = Math.Min(i.loAddr, overlappingInterval.loAddr);
                i.hiAddr = Math.Max(i.hiAddr, overlappingInterval.hiAddr);
                DeleteInterval(overlappingInterval);
            }

            internal bool AddObject(ulong id, uint size, int allocTickIndex, SampleObjectTable sampleObjectTable)
            {
                size = (size + 3) & (uint.MaxValue - 3);
                Interval prevInterval = null;
                Interval bestInterval = null;
                Interval prevI = null;
                bool emptySpace = false;
                // look for the best interval to put this object in.
                for (Interval i = liveRoot; i != null; i = i.next)
                {
                    if (i.loAddr < id + size && id <= i.hiAddr + allowableGap)
                    {
                        if (bestInterval == null || bestInterval.loAddr < i.loAddr)
                        {
                            bestInterval = i;
                            prevInterval = prevI;
                        }
                    }
                    prevI = i;
                }
                if (bestInterval != null)
                {
                    if (bestInterval.loAddr > id)
                    {
                        bestInterval.loAddr = id;
                    }
                    if (id < bestInterval.hiAddr)
                    {
                        if (bestInterval.hadRelocations && bestInterval.justHadGc)
                        {
                            // Interval gets shortened
                            liveObjectTable.RemoveObjectRange(id, bestInterval.hiAddr - id, allocTickIndex, sampleObjectTable);
                            bestInterval.hiAddr = id + size;
                            bestInterval.justHadGc = false;
                        }
                    }
                    else
                    {
                        bestInterval.hiAddr = id + size;
                        emptySpace = true;
                    }                   
                    if (prevInterval != null)
                    {
                        // Move to front to speed up future searches.
                        prevInterval.next = bestInterval.next;
                        bestInterval.next = liveRoot;
                        liveRoot = bestInterval;
                    }
                    if (OverlappingInterval(bestInterval) != null)
                        MergeInterval(bestInterval);
                    return emptySpace;
                }
                liveRoot = new Interval(id, id + size, -1, liveRoot);
                Debug.Assert(OverlappingInterval(liveRoot) == null);
                return emptySpace;
            }

            internal void GenerationInterval(ulong rangeStart, ulong rangeLength, int generation)
            {
                newLiveRoot = new Interval(rangeStart, rangeStart + rangeLength, generation, newLiveRoot);
            }

            internal int GenerationOfObject(ulong id)
            {
                Interval prev = null;
                for (Interval i = liveRoot; i != null; i = i.next)
                {
                    if (i.loAddr <= id && id < i.hiAddr)
                    {
                        if (prev != null)
                        {
                            // Move to front to speed up future searches.
                            prev.next = i.next;
                            i.next = liveRoot;
                            liveRoot = i;
                        }
                        return i.generation;
                    }
                    prev = i;
                }
                return -1;
            }

            internal void Preserve(ulong id, ulong length)
            {
                if (updateRoot != null && updateRoot.hiAddr == id)
                    updateRoot.hiAddr = id + length;
                else
                    updateRoot = new Interval(id, id + length, -1, updateRoot);
            }

            internal void Relocate(ulong oldId, ulong newId, uint length)
            {
                if (oldId == newId)
                    nullRelocationsSeen = true;

                if (updateRoot != null && updateRoot.hiAddr == newId)
                    updateRoot.hiAddr = newId + length;
                else
                    updateRoot = new Interval(newId, newId + length, -1, updateRoot);

                for (Interval i = liveRoot; i != null; i = i.next)
                {
                    if (i.loAddr <= oldId && oldId < i.hiAddr)
                        i.hadRelocations = true;
                }
                Interval bestInterval = null;
                for (Interval i = liveRoot; i != null; i = i.next)
                {
                    if (i.loAddr <= newId + length && newId <= i.hiAddr + allowableGap)
                    {
                        if (bestInterval == null || bestInterval.loAddr < i.loAddr)
                            bestInterval = i;
                    }
                }
                if (bestInterval != null)
                {
                    if (bestInterval.hiAddr < newId + length)
                        bestInterval.hiAddr = newId + length;
                    if (bestInterval.loAddr > newId)
                        bestInterval.loAddr = newId;
                    if (OverlappingInterval(bestInterval) != null)
                        MergeInterval(bestInterval);
                }
                else
                {
                    liveRoot = new Interval(newId, newId + length, -1, liveRoot);
                    Debug.Assert(OverlappingInterval(liveRoot) == null);
                }
            }

            private Interval SortIntervals(Interval root)
            {
                // using insertion sort for now...
                Interval next;
                Interval newRoot = null;
                for (Interval i = root; i != null; i = next)
                {
                    next = i.next;
                    Interval prev = null;
                    Interval ii;
                    for (ii = newRoot; ii != null; ii = ii.next)
                    {
                        if (i.loAddr < ii.loAddr)
                            break;
                        prev = ii;
                    }
                    if (prev == null)
                    {
                        i.next = newRoot;
                        newRoot = i;
                    }
                    else
                    {
                        i.next = ii;
                        prev.next = i;
                    }
                }
                return newRoot;
            }

            private void RemoveRange(ulong loAddr, ulong hiAddr, int tickIndex, SampleObjectTable sampleObjectTable)
            {
                Interval next;
                for (Interval i = liveRoot; i != null; i = next)
                {
                    next = i.next;
                    ulong lo = Math.Max(loAddr, i.loAddr);
                    ulong hi = Math.Min(hiAddr, i.hiAddr);
                    if (lo >= hi)
                        continue;
                    liveObjectTable.RemoveObjectRange(lo, hi - lo, tickIndex, sampleObjectTable);
                    if (i.hiAddr == hi)
                    {
                        if (i.loAddr == lo)
                            DeleteInterval(i);
                        else
                            i.hiAddr = lo;
                    }
                }
            }

            internal void RecordGc(int tickIndex, SampleObjectTable sampleObjectTable, bool simpleForm)
            {
                if (simpleForm && nullRelocationsSeen || newLiveRoot != null)
                {
                    // in this case assume anything not reported is dead
                    updateRoot = SortIntervals(updateRoot);
                    ulong prevHiAddr = 0;
                    for (Interval i = updateRoot; i != null; i = i.next)
                    {
                        if (prevHiAddr < i.loAddr)
                        {
                            RemoveRange(prevHiAddr, i.loAddr, tickIndex, sampleObjectTable);
                        }
                        if (prevHiAddr < i.hiAddr)
                            prevHiAddr = i.hiAddr;
                    }
                    RemoveRange(prevHiAddr, ulong.MaxValue, tickIndex, sampleObjectTable);
                    updateRoot = null;
                    if (newLiveRoot != null)
                    {
                        liveRoot = newLiveRoot;
                        newLiveRoot = null;
                    }
                }
                else
                {
                    for (Interval i = liveRoot; i != null; i = i.next)
                        i.justHadGc = true;
                }
                nullRelocationsSeen = false;
            }
        }

        IntervalTable intervalTable;
        internal ReadNewLog readNewLog;
        internal int lastTickIndex;
        private long lastPos;

        const int alignShift = 2;
        const int firstLevelShift = 20;
        const int initialFirstLevelLength = 1 << (31 - alignShift - firstLevelShift);  // covering 2 GB of address space
        const int secondLevelLength = 1<<firstLevelShift;
        const int secondLevelMask = secondLevelLength-1;

        ushort[][] firstLevelTable;

        void GrowFirstLevelTable()
        {
            ushort[][] newFirstLevelTable = new ushort[firstLevelTable.Length*2][];
            for (int i = 0; i < firstLevelTable.Length; i++)
                newFirstLevelTable[i] = firstLevelTable[i];
            firstLevelTable = newFirstLevelTable;
        }

        internal LiveObjectTable(ReadNewLog readNewLog)
        {
            firstLevelTable = new ushort[initialFirstLevelLength][];
            intervalTable = new IntervalTable(this);
            this.readNewLog = readNewLog;
            lastGcGen0Count = 0;
            lastGcGen1Count = 0;
            lastGcGen2Count = 0;
            lastTickIndex = 0;
            lastPos = 0;
        }

        internal ulong FindObjectBackward(ulong id)
        {
            id >>= alignShift;
            uint i = (uint)(id >> firstLevelShift);
            uint j = (uint)(id & secondLevelMask);
            if (i >= firstLevelTable.Length)
            {
                i = (uint)(firstLevelTable.Length - 1);
                j = (uint)(secondLevelLength - 1);
            }
            while (i != uint.MaxValue)
            {
                ushort[] secondLevelTable = firstLevelTable[i];
                if (secondLevelTable != null)
                {
                    while (j != uint.MaxValue)
                    {
                        if ((secondLevelTable[j] & 0x8000) != 0)
                            break;
                        j--;
                    }
                    if (j != uint.MaxValue)
                        break;
                }
                j = secondLevelLength - 1;
                i--;
            }
            if (i == uint.MaxValue)
                return 0;
            else
                return (((ulong)i<<firstLevelShift) + j) << alignShift;
        }

        ulong FindObjectForward(ulong startId, ulong endId)
        {
            startId >>= alignShift;
            endId >>= alignShift;
            uint i = (uint)(startId >> firstLevelShift);
            uint iEnd = (uint)(endId >> firstLevelShift);
            uint j = (uint)(startId & secondLevelMask);
            uint jEnd = (uint)(endId & secondLevelMask);
            if (iEnd >= firstLevelTable.Length)
            {
                iEnd = (uint)(firstLevelTable.Length - 1);
                jEnd = (uint)(secondLevelLength - 1);
            }
            while (i <= iEnd)
            {
                ushort[] secondLevelTable = firstLevelTable[i];
                if (secondLevelTable != null)
                {
                    while (j < secondLevelLength && (j <= jEnd || i < iEnd))
                    {
                        if ((secondLevelTable[j] & 0x8000) != 0)
                            break;
                        j++;
                    }
                    if (j < secondLevelLength)
                        break;
                }
                j = 0;
                i++;
            }
            if (i > iEnd || (i == iEnd && j > jEnd))
                return ulong.MaxValue;
            else
                return (((ulong)i<<firstLevelShift) + j) << alignShift;
        }

        internal void GetNextObject(ulong startId, ulong endId, out LiveObject o)
        {
            ulong id = FindObjectForward(startId, endId);
            o.id = id;
            id >>= alignShift;
            uint i = (uint)(id >> firstLevelShift);
            uint j = (uint)(id & secondLevelMask);
            ushort[] secondLevelTable = null;
            if (i < firstLevelTable.Length)
                secondLevelTable = firstLevelTable[i];
            if (secondLevelTable != null)
            {
                ushort u1 = secondLevelTable[j];
                if ((u1 & 0x8000) != 0)
                {
                    j++;
                    if (j >= secondLevelLength)
                    {
                        j = 0;
                        i++;
                        secondLevelTable = firstLevelTable[i];
                    }
                    ushort u2 = secondLevelTable[j];
                    j++;
                    if (j >= secondLevelLength)
                    {
                        j = 0;
                        i++;
                        secondLevelTable = firstLevelTable[i];
                    }
                    ushort u3 = secondLevelTable[j];

                    o.allocTickIndex = (u2 >> 7) + (u3 << 8);

                    o.typeSizeStacktraceIndex = (u1 & 0x7fff) + ((u2 & 0x7f) << 15);

                    int[] stacktrace = readNewLog.stacktraceTable.IndexToStacktrace(o.typeSizeStacktraceIndex);

                    o.typeIndex = stacktrace[0];
                    o.size = (uint)stacktrace[1];

                    return;
                }
            }
            o.size = 0;
            o.allocTickIndex = o.typeIndex = o.typeSizeStacktraceIndex = 0;
        }

        void Write3WordsAt(ulong id, ushort u1, ushort u2, ushort u3)
        {
            id >>= alignShift;
            uint i = (uint)(id >> firstLevelShift);
            uint j = (uint)(id & secondLevelMask);
            while (firstLevelTable.Length <= i+1)
                GrowFirstLevelTable();
            ushort[] secondLevelTable = firstLevelTable[i];
            if (secondLevelTable == null)
            {
                secondLevelTable = new ushort[secondLevelLength];
                firstLevelTable[i] = secondLevelTable;
            }
            secondLevelTable[j] = u1;
            j++;
            if (j >= secondLevelLength)
            {
                j = 0;
                i++;
                secondLevelTable = firstLevelTable[i];
                if (secondLevelTable == null)
                {
                    secondLevelTable = new ushort[secondLevelLength];
                    firstLevelTable[i] = secondLevelTable;
                }
            }
            secondLevelTable[j] = u2;
            j++;
            if (j >= secondLevelLength)
            {
                j = 0;
                i++;
                secondLevelTable = firstLevelTable[i];
                if (secondLevelTable == null)
                {
                    secondLevelTable = new ushort[secondLevelLength];
                    firstLevelTable[i] = secondLevelTable;
                }
            }
            secondLevelTable[j] = u3;
        }

        internal void Zero(ulong id, uint size)
        {
            uint count = ((size + 3) & (uint.MaxValue - 3))/4;
            id >>= alignShift;
            uint i = (uint)(id >> firstLevelShift);
            uint j = (uint)(id & secondLevelMask);
            ushort[] secondLevelTable = null;
            if (i < firstLevelTable.Length)
                secondLevelTable = firstLevelTable[i];
            while (count > 0)
            {
                // Does the piece to clear fit within the secondLevelTable?
                if (j + count <= secondLevelLength)
                {
                    // yes - if there is no secondLevelTable, there is nothing left to do
                    if (secondLevelTable == null)
                        break;
                    while (count > 0)
                    {
                        secondLevelTable[j] = 0;
                        count--;
                        j++;
                    }
                }
                else
                {
                    // no - if there is no secondLevelTable, skip it
                    if (secondLevelTable == null)
                    {
                        count -= secondLevelLength - j;
                    }
                    else
                    {
                        while (j < secondLevelLength)
                        {
                            secondLevelTable[j] = 0;
                            count--;
                            j++;
                        }
                    }
                    j = 0;
                    i++;
                    secondLevelTable = null;
                    if (i < firstLevelTable.Length)
                        secondLevelTable = firstLevelTable[i];
                }
            }
        }

        internal void Zero(ulong id, ulong size)
        {
            while (size >= uint.MaxValue)
            {
                Zero(id, uint.MaxValue);
                id += uint.MaxValue;
                size -= uint.MaxValue;
            }
            Zero(id, (uint)size);
        }

        internal bool CanReadObjectBackCorrectly(ulong id, uint size, int typeSizeStacktraceIndex, int allocTickIndex)
        {
            LiveObject o;
            GetNextObject(id, id + size, out o);
            return o.id == id && o.typeSizeStacktraceIndex == typeSizeStacktraceIndex && o.allocTickIndex == allocTickIndex;
        }

        internal void InsertObject(ulong id, int typeSizeStacktraceIndex, int allocTickIndex, int nowTickIndex, bool newAlloc, SampleObjectTable sampleObjectTable)
        {
            if (lastPos >= readNewLog.pos && newAlloc)
                return;
            lastPos = readNewLog.pos;

            lastTickIndex = nowTickIndex;
            int[] stacktrace = readNewLog.stacktraceTable.IndexToStacktrace(typeSizeStacktraceIndex);
            int typeIndex = stacktrace[0];
            uint size = (uint)stacktrace[1];
            bool emptySpace = false;
            if (newAlloc)
            {
                emptySpace = intervalTable.AddObject(id, size, allocTickIndex, sampleObjectTable);
            }
            if (!emptySpace)
            {
                ulong prevId = FindObjectBackward(id - 4);
                LiveObject o;
                GetNextObject(prevId, id, out o);
                if (o.id < id && (o.id + o.size > id || o.id + 12 > id))
                {
                    Zero(o.id, id - o.id);
                }
            }
            Debug.Assert(FindObjectBackward(id-4)+12 <= id);
            if (size >= 12)
            {
                ushort u1 = (ushort)(typeSizeStacktraceIndex | 0x8000);
                ushort u2 = (ushort)((typeSizeStacktraceIndex >> 15) | ((allocTickIndex & 0xff) << 7));
                ushort u3 = (ushort)(allocTickIndex >> 8);
                Write3WordsAt(id, u1, u2, u3);
                if (!emptySpace)
                    Zero(id + 12, size - 12);
                Debug.Assert(CanReadObjectBackCorrectly(id, size, typeSizeStacktraceIndex, allocTickIndex));
            }
            if (sampleObjectTable != null)
                sampleObjectTable.Insert(id, id + size, nowTickIndex, allocTickIndex, typeIndex);
        }

        void RemoveObjectRange(ulong firstId, ulong length, int tickIndex, SampleObjectTable sampleObjectTable)
        {
            ulong lastId = firstId + length;

            if (sampleObjectTable != null)
                sampleObjectTable.Delete(firstId, lastId, tickIndex);

            Zero(firstId, length);
        }

        internal void GenerationInterval(ulong rangeStart, ulong rangeLength, int generation, int tickIndex)
        {
            lastPos = readNewLog.pos;

            lastTickIndex = tickIndex;
            intervalTable.GenerationInterval(rangeStart, rangeLength, generation);            
        }

        internal int GenerationOfObject(ref LiveObject o)
        {
            int generation = intervalTable.GenerationOfObject(o.id);
            if (generation < 0)
            {
                generation = 0;
                if (o.allocTickIndex <= gen2LimitTickIndex)
                    generation = 2;
                else if (o.allocTickIndex <= gen1LimitTickIndex)
                    generation = 1;
            }
            return generation;
        }

        internal void Preserve(ulong id, ulong length, int tickIndex)
        {
            if (lastPos >= readNewLog.pos)
                return;
            lastPos = readNewLog.pos;

            lastTickIndex = tickIndex;
            intervalTable.Preserve(id, length);            
        }

        internal void UpdateObjects(Histogram relocatedHistogram, ulong oldId, ulong newId, uint length, int tickIndex, SampleObjectTable sampleObjectTable)
        {
            if (lastPos >= readNewLog.pos)
                return;
            lastPos = readNewLog.pos;

            lastTickIndex = tickIndex;
            intervalTable.Relocate(oldId, newId, length);

            if (oldId == newId)
                return;

            ulong nextId;
            ulong lastId = oldId + length;
            LiveObject o;
            for (GetNextObject(oldId, lastId, out o); o.id < lastId; GetNextObject(nextId, lastId, out o))
            {
                nextId = o.id + o.size;
                ulong offset = o.id - oldId;
                if (sampleObjectTable != null)
                    sampleObjectTable.Delete(o.id, o.id + o.size, tickIndex);
                Zero(o.id, o.size);
                InsertObject(newId + offset, o.typeSizeStacktraceIndex, o.allocTickIndex, tickIndex, false, sampleObjectTable);
                if (relocatedHistogram != null)
                    relocatedHistogram.AddObject(o.typeSizeStacktraceIndex, 1);
            }
        }

        internal int lastGcGen0Count;
        internal int lastGcGen1Count;
        internal int lastGcGen2Count;

        internal int gen1LimitTickIndex;
        internal int gen2LimitTickIndex;

        internal void RecordGc(int tickIndex, int gen, SampleObjectTable sampleObjectTable, bool simpleForm)
        {
            lastTickIndex = tickIndex;

            if (sampleObjectTable != null)
                sampleObjectTable.AddGcTick(tickIndex, gen);
    
            intervalTable.RecordGc(tickIndex, sampleObjectTable, simpleForm);

            if (gen >= 1)
                gen2LimitTickIndex = gen1LimitTickIndex;
            gen1LimitTickIndex = tickIndex;

            lastGcGen0Count++;
            if (gen > 0)
            {
                lastGcGen1Count++;
                if (gen > 1)
                    lastGcGen2Count++;
            }
        }

        internal void RecordGc(int tickIndex, int gcGen0Count, int gcGen1Count, int gcGen2Count, SampleObjectTable sampleObjectTable)
        {
            int gen = 0;
            if (gcGen2Count != lastGcGen2Count)
                gen = 2;
            else if (gcGen1Count != lastGcGen1Count)
                gen = 1;

            RecordGc(tickIndex, gen, sampleObjectTable, false);

            lastGcGen0Count = gcGen0Count;
            lastGcGen1Count = gcGen1Count;
            lastGcGen2Count = gcGen2Count;
        }
    }

    internal class StacktraceTable
    {
        private int[][] stacktraceTable;
        private int[] mappingTable;
        private int maxID = 0;
        // logs created by the debugger have lots of duplicate type id/size combinations
        private Dictionary<long, int> allocHashtable;

        internal StacktraceTable()
        {
            stacktraceTable = new int[1000][];
            stacktraceTable[0] = new int[0];
            allocHashtable = new Dictionary<long, int>();
        }

        int LookupAlloc(int typeId, int size)
        {
            long key = typeId + ((long)size << 32);
            int data;
            if (allocHashtable.TryGetValue(key, out data))
            {
                return data;
            }
            return -1;
        }

        internal int MapTypeSizeStacktraceId(int id)
        {
            if (mappingTable != null)
                return mappingTable[id];
            return id;
        }

        void EnterAlloc(int id, int typeId, int size)
        {
            long key = typeId + ((long)size << 32);
            allocHashtable[key] = id;
        }

        internal void Add(int id, int[] stack, int length, bool isAllocStack)
        {
            Add( id, stack, 0, length, isAllocStack );
        }

        void CreateMappingTable()
        {
            mappingTable = new int[stacktraceTable.Length];
            for (int i = 0; i < mappingTable.Length; i++)
                mappingTable[i] = i;
        }

        void GrowMappingTable()
        {
            int[] newMappingTable = new int[mappingTable.Length*2];
            for (int i = 0; i < mappingTable.Length; i++)
                newMappingTable[i] = mappingTable[i];
            mappingTable = newMappingTable;
        }

        internal void Add(int id, int[] stack, int start, int length, bool isAllocStack)
        {
            int oldId = -1;
            if (isAllocStack && length == 2)
                oldId = LookupAlloc(stack[start], stack[start+1]);

            if (oldId >= 0)
            {
                if (mappingTable == null)
                    CreateMappingTable();
                while (mappingTable.Length <= id)
                    GrowMappingTable();
                mappingTable[id] = oldId;
            }
            else
            {
                int[] stacktrace = new int[length];
                for (int i = 0; i < stacktrace.Length; i++)
                    stacktrace[i] = stack[start++];

                if (mappingTable != null)
                {
                    int newId = maxID + 1;
                    while (mappingTable.Length <= id)
                        GrowMappingTable();
                    mappingTable[id] = newId;
                    id = newId;
                }

                if (isAllocStack && length == 2)
                {
                    EnterAlloc(id, stacktrace[0], stacktrace[1]);
                }

                while (stacktraceTable.Length <= id)
                {
                    int[][] newStacktraceTable = new int[stacktraceTable.Length*2][];
                    for (int i = 0; i < stacktraceTable.Length; i++)
                        newStacktraceTable[i] = stacktraceTable[i];
                    stacktraceTable = newStacktraceTable;
                }

                stacktraceTable[id] = stacktrace;

                if (id > maxID)
                {
                    maxID = id;
                }
            }
        }

        internal int GetOrCreateTypeSizeId(int typeId, int size)
        {
            int id = LookupAlloc(typeId, size);
            if (id > 0)
                return id;
            if (mappingTable == null)
                CreateMappingTable();
            id = ++maxID;

            EnterAlloc(id, typeId, size);

            while (stacktraceTable.Length <= id)
            {
                int[][] newStacktraceTable = new int[stacktraceTable.Length * 2][];
                for (int i = 0; i < stacktraceTable.Length; i++)
                    newStacktraceTable[i] = stacktraceTable[i];
                stacktraceTable = newStacktraceTable;
            }

            int[] stacktrace = new int[2];
            stacktrace[0] = typeId;
            stacktrace[1] = size;
            stacktraceTable[id] = stacktrace;

            return id;
        }

        internal int[] IndexToStacktrace(int index)
        {
            if (index < 0 || index >= stacktraceTable.Length || stacktraceTable[index] == null)
                Console.WriteLine("bad index {0}", index);
            return stacktraceTable[index];
        }

        internal void FreeEntries( int firstIndex )
        {
            maxID = firstIndex;
        }

        internal int Length 
        {
            get 
            { 
                return maxID + 1;
            }
        }
    }

    internal struct TimePos
    {
        internal double time;
        internal long pos;

        internal TimePos(double time, long pos)
        {
            this.time = time;
            this.pos = pos;
        }
    }

    internal class FunctionList
    {
        internal class FunctionDescriptor
        {
            internal FunctionDescriptor(int functionId, int funcCallStack, uint funcSize, int funcModule)
            {
                this.functionId = functionId;
                this.funcCallStack = funcCallStack;
                this.funcSize = funcSize;
                this.funcModule = funcModule;
            }

            internal int functionId;
            internal int funcCallStack;
            internal uint funcSize;
            internal int funcModule;
        }
    
        ReadNewLog readNewLog;
        ArrayList functionList;

        internal FunctionList(ReadNewLog readNewLog)
        {
            this.readNewLog = readNewLog;
            this.functionList = new ArrayList();
        }

        internal void Add(int functionId, int funcCallStack, uint funcSize, int funcModule)
        {
            functionList.Add(new FunctionDescriptor(functionId, funcCallStack, funcSize, funcModule));
        }

        internal bool Empty
        {
            get
            {
                return functionList.Count == 0;
            }
        }

        void BuildFuncVertices(Graph graph, ref Vertex[] funcVertex, FilterForm filterForm)
        {
            for (int i = 0; i < readNewLog.funcName.Length; i++)
            {
                string name = readNewLog.funcName[i];
                string signature = readNewLog.funcSignature[i];
                if (name != null && signature != null)
                    readNewLog.AddFunctionVertex(i, name, signature, graph, ref funcVertex, filterForm);
            }
        }

        int BuildVertexStack(int stackTraceIndex, Vertex[] funcVertex, ref Vertex[] vertexStack, int skipCount)
        {
            int[] stackTrace = readNewLog.stacktraceTable.IndexToStacktrace(stackTraceIndex);
                
            while (vertexStack.Length < stackTrace.Length+1)
                vertexStack = new Vertex[vertexStack.Length*2];

            for (int i = skipCount; i < stackTrace.Length; i++)
                vertexStack[i-skipCount] = funcVertex[stackTrace[i]];

            return stackTrace.Length - skipCount;
        }

        void BuildFunctionTrace(Graph graph, int stackTraceIndex, int funcIndex, ulong size, Vertex[] funcVertex, ref Vertex[] vertexStack, FilterForm filterForm)
        {
            int stackPtr = BuildVertexStack(stackTraceIndex, funcVertex, ref vertexStack, 0);

            Vertex toVertex = graph.TopVertex;
            Vertex fromVertex;
            Edge edge;
            if ((funcVertex[funcIndex].interestLevel & InterestLevel.Interesting) == InterestLevel.Interesting
                && ReadNewLog.InterestingCallStack(vertexStack, stackPtr, filterForm))
            {
                vertexStack[stackPtr] = funcVertex[funcIndex];
                stackPtr++;
                stackPtr = ReadNewLog.FilterVertices(vertexStack, stackPtr);
                stackPtr = Vertex.SqueezeOutRepetitions(vertexStack, stackPtr);
                for (int i = 0; i < stackPtr; i++)
                {
                    fromVertex = toVertex;
                    toVertex = vertexStack[i];
                    edge = graph.FindOrCreateEdge(fromVertex, toVertex);
                    edge.AddWeight(size);
                }
                fromVertex = toVertex;
                toVertex = graph.BottomVertex;
                edge = graph.FindOrCreateEdge(fromVertex, toVertex);
                edge.AddWeight(size);
            }
        }

        internal Graph BuildFunctionGraph(FilterForm filterForm)
        {
            Vertex[] funcVertex = new Vertex[1];
            Vertex[] vertexStack = new Vertex[1];

            Graph graph = new Graph(this);
            graph.graphType = Graph.GraphType.FunctionGraph;

            BuildFuncVertices(graph, ref funcVertex, filterForm);

            foreach (FunctionDescriptor fd in functionList)
            {
                BuildFunctionTrace(graph, fd.funcCallStack, fd.functionId, fd.funcSize, funcVertex, ref vertexStack, filterForm);
            }

            foreach (Vertex v in graph.vertices.Values)
                v.active = true;
            graph.BottomVertex.active = false;

            return graph;
        }

        void BuildModVertices(Graph graph, ref Vertex[] modVertex, FilterForm filterForm)
        {
            for (int i = 0; i < readNewLog.modBasicName.Length; i++)
            {
                string basicName = readNewLog.modBasicName[i];
                string fullName = readNewLog.modFullName[i];
                if (basicName != null && fullName != null)
                {
                    readNewLog.AddFunctionVertex(i, basicName, fullName, graph, ref modVertex, filterForm);
                    modVertex[i].basicName = basicName;
                    modVertex[i].basicSignature = fullName;
                }
            }
        }

        int FunctionsInSameModule(int modIndex, int stackTraceIndex)
        {
            int[] stackTrace = readNewLog.stacktraceTable.IndexToStacktrace(stackTraceIndex);
            int result = 0;
            for (int i = stackTrace.Length - 1; i >= 0; i--)
            {
                int funcIndex = stackTrace[i];
                if (readNewLog.funcModule[funcIndex] == modIndex)
                    result++;
                else
                    break;
            }
            return result;
        }

        void BuildModuleTrace(Graph graph, int stackTraceIndex, int modIndex, ulong size, Vertex[] funcVertex, Vertex[] modVertex, ref Vertex[] vertexStack, FilterForm filterForm)
        {
            int functionsToSkip = FunctionsInSameModule(modIndex, stackTraceIndex);
            int stackPtr = BuildVertexStack(stackTraceIndex, funcVertex, ref vertexStack, 0) - functionsToSkip;

            Vertex toVertex = graph.TopVertex;
            Vertex fromVertex;
            Edge edge;
            if (ReadNewLog.InterestingCallStack(vertexStack, stackPtr, filterForm))
            {
                vertexStack[stackPtr] = modVertex[modIndex];
                stackPtr++;
                stackPtr = ReadNewLog.FilterVertices(vertexStack, stackPtr);
                stackPtr = Vertex.SqueezeOutRepetitions(vertexStack, stackPtr);
                for (int i = 0; i < stackPtr; i++)
                {
                    fromVertex = toVertex;
                    toVertex = vertexStack[i];
                    edge = graph.FindOrCreateEdge(fromVertex, toVertex);
                    edge.AddWeight(size);
                }
                fromVertex = toVertex;
                toVertex = graph.BottomVertex;
                edge = graph.FindOrCreateEdge(fromVertex, toVertex);
                edge.AddWeight(size);
            }
        }

        internal Graph BuildModuleGraph(FilterForm filterForm)
        {
            Vertex[] funcVertex = new Vertex[1];
            Vertex[] vertexStack = new Vertex[1];
            Vertex[] modVertex = new Vertex[1];

            Graph graph = new Graph(this);
            graph.graphType = Graph.GraphType.ModuleGraph;

            BuildFuncVertices(graph, ref funcVertex, filterForm);
            BuildModVertices(graph, ref modVertex, filterForm);

            foreach (FunctionDescriptor fd in functionList)
            {
                BuildModuleTrace(graph, fd.funcCallStack, fd.funcModule, fd.funcSize, funcVertex, modVertex, ref vertexStack, filterForm);
            }

            foreach (Vertex v in graph.vertices.Values)
                v.active = true;
            graph.BottomVertex.active = false;

            return graph;
        }

        string ClassNameOfFunc(int funcIndex)
        {
            string funcName = readNewLog.funcName[funcIndex];
            int colonColonIndex = funcName.IndexOf("::");
            if (colonColonIndex > 0)
                return funcName.Substring(0, colonColonIndex);
            else
                return funcName;
        }

        int FunctionsInSameClass(string className, int stackTraceIndex)
        {
            int[] stackTrace = readNewLog.stacktraceTable.IndexToStacktrace(stackTraceIndex);
            int result = 0;
            for (int i = stackTrace.Length - 1; i >= 0; i--)
            {
                int funcIndex = stackTrace[i];
                if (ClassNameOfFunc(funcIndex) == className)
                    result++;
                else
                    break;
            }
            return result;
        }

        void BuildClassTrace(Graph graph, int stackTraceIndex, int funcIndex, ulong size, Vertex[] funcVertex, ref Vertex[] vertexStack, FilterForm filterForm)
        {
            string className = ClassNameOfFunc(funcIndex);
            int functionsToSkip = FunctionsInSameClass(className, stackTraceIndex);
            int stackPtr = BuildVertexStack(stackTraceIndex, funcVertex, ref vertexStack, 0) - functionsToSkip;

            Vertex toVertex = graph.TopVertex;
            Vertex fromVertex;
            Edge edge;
            if (ReadNewLog.InterestingCallStack(vertexStack, stackPtr, filterForm))
            {
                vertexStack[stackPtr] = graph.FindOrCreateVertex(className, null, null);
                vertexStack[stackPtr].interestLevel = filterForm.InterestLevelOfMethodName(className, null);
                stackPtr++;
                stackPtr = ReadNewLog.FilterVertices(vertexStack, stackPtr);
                stackPtr = Vertex.SqueezeOutRepetitions(vertexStack, stackPtr);
                for (int i = 0; i < stackPtr; i++)
                {
                    fromVertex = toVertex;
                    toVertex = vertexStack[i];
                    edge = graph.FindOrCreateEdge(fromVertex, toVertex);
                    edge.AddWeight(size);
                }
                if (toVertex != graph.TopVertex)
                {
                    fromVertex = toVertex;
                    toVertex = graph.BottomVertex;
                    edge = graph.FindOrCreateEdge(fromVertex, toVertex);
                    edge.AddWeight(size);
                }
            }
        }

        internal Graph BuildClassGraph(FilterForm filterForm)
        {
            Vertex[] funcVertex = new Vertex[1];
            Vertex[] vertexStack = new Vertex[1];

            Graph graph = new Graph(this);
            graph.graphType = Graph.GraphType.ClassGraph;

            BuildFuncVertices(graph, ref funcVertex, filterForm);

            foreach (FunctionDescriptor fd in functionList)
            {
                BuildClassTrace(graph, fd.funcCallStack, fd.functionId, fd.funcSize, funcVertex, ref vertexStack, filterForm);
            }

            foreach (Vertex v in graph.vertices.Values)
                v.active = true;
            graph.BottomVertex.active = false;

            return graph;
        }

        internal void ReportCallCountSizes(Histogram callstackHistogram)
        {
            uint[] callCount = new uint[readNewLog.funcName.Length];
            callstackHistogram.CalculateCallCounts(callCount);
            Console.WriteLine("{0},{1},{2} {3}", "# Calls", "Function Size", "Function Name", "Function Signature");
            foreach (FunctionDescriptor fd in functionList)
            {
                Console.WriteLine("{0},{1},{2} {3}", callCount[fd.functionId], fd.funcSize, readNewLog.funcName[fd.functionId], readNewLog.funcSignature[fd.functionId]);
            }
        }
    }

    internal class ReadLogResult
    {
        internal Histogram allocatedHistogram;
        internal Histogram relocatedHistogram;
        internal Histogram callstackHistogram;
        internal Histogram finalizerHistogram;
        internal Histogram criticalFinalizerHistogram;
        internal Histogram[] heapDumpHistograms;
        internal Histogram createdHandlesHistogram;
        internal Histogram destroyedHandlesHistogram;
        internal LiveObjectTable liveObjectTable;
        internal SampleObjectTable sampleObjectTable;
        internal ObjectGraph objectGraph;
        internal ObjectGraph requestedObjectGraph; // accomodate more than one objectGraph
        internal FunctionList functionList;
        internal bool hadAllocInfo, hadCallInfo;
        internal Dictionary<ulong, HandleInfo> handleHash;
    }

    internal class HandleInfo
    {
        internal HandleInfo(int allocThreadId, ulong handleId, ulong initialObjectId, int allocTickIndex, int allocStacktraceId)
        {
            this.allocThreadId = allocThreadId;
            this.handleId = handleId;
            this.initialObjectId = initialObjectId;
            this.allocTickIndex = allocTickIndex;
            this.allocStacktraceId = allocStacktraceId;
        }

        internal int allocThreadId;
        internal ulong handleId;
        internal ulong initialObjectId;
        internal int allocTickIndex;
        internal int allocStacktraceId;
    };

    internal class ReadNewLog
    {
        // helper class to keep track of events like log file comments, garbage collections, and heap dumps
        internal class EventList
        {
            internal int count;
            internal int[] eventTickIndex;
            internal string[] eventString;

            internal EventList()
            {
                eventTickIndex = new int[10];
                eventString = new string[10];
            }

            internal bool AddEvent(int newTickIndex, string newString)
            {
                if (count > 0 && newTickIndex <= eventTickIndex[count-1])
                    return false;
                EnsureIntCapacity(count, ref eventTickIndex);
                EnsureStringCapacity(count, ref eventString);
                eventTickIndex[count] = newTickIndex;
                eventString[count] = newString;
                count++;

                return true;
            }
        }

        internal ReadNewLog(string fileName)
        {
            //
            // need to add constructor logic here
            //
            assemblies = new Dictionary<string/*assembly name*/, int/*stack id*/>();
            assembliesJustLoaded = new Dictionary<int/*thread id*/, List<string>>();
            typeName = new string[1000];
            funcName = new string[1000];
            funcSignature = new string[1000];
            funcModule = new int[1000];
            modBasicName = new string[10];
            modFullName = new string[10];
            commentEventList = new EventList();
            gcEventList = new EventList();
            heapDumpEventList = new EventList();
            finalizableTypes = new Dictionary<int/*type id*/,bool>();
            gcCount = new int[4];
            inducedGcCount = new int[3];
            generationSize = new ulong[4];
            cumulativeGenerationSize = new ulong[4];

            this.fileName = fileName;

            this.typeSignatureIdHash = new Dictionary<string/*type name*/, int/*type id*/>();
            this.funcSignatureIdHash = new Dictionary<string/*func name*/, int/*func id*/>();

            this.progressFormVisible = true;
        }

        internal ReadNewLog(string fileName, bool progressFormVisible) : this(fileName)
        {
            this.progressFormVisible = progressFormVisible;
        }

        internal StacktraceTable stacktraceTable;
        internal string fileName;

        StreamReader r;
        byte[] buffer;
        int bufPos;
        int bufLevel;
        int c;
        int line;
        internal long pos;
        long lastLineStartPos;
        internal Dictionary<int/*thread id*/, List<string>> assembliesJustLoaded;
        internal Dictionary<string/*assembly name*/, int/*stack id*/> assemblies;
        internal string[] typeName;
        internal string[] funcName;
        internal string[] funcSignature;
        internal int[] funcModule;
        internal string[] modBasicName;
        internal string[] modFullName;
        internal Dictionary<int/*type id*/, bool> finalizableTypes;
        internal Dictionary<string/*type name*/, int/*type id*/> typeSignatureIdHash;
        internal Dictionary<string/*func name*/, int/*func id*/> funcSignatureIdHash;
        internal int maxTickIndex;
        bool progressFormVisible;
        internal int[] inducedGcCount;
        internal int[] gcCount;
        internal ulong[] generationSize;
        internal ulong[] cumulativeGenerationSize;
        internal EventList commentEventList;
        internal EventList gcEventList;
        internal EventList heapDumpEventList;

        static void EnsureVertexCapacity(int id, ref Vertex[] vertexArray)
        {
            Debug.Assert(id >= 0);
            if (id < vertexArray.Length)
                return;
            int newLength = vertexArray.Length*2;
            if (newLength <= id)
                newLength = id + 1;
            Vertex[] newVertexArray = new Vertex[newLength];
            Array.Copy(vertexArray, 0, newVertexArray, 0, vertexArray.Length);
            vertexArray = newVertexArray;
        }

        static void EnsureStringCapacity(int id, ref string[] stringArray)
        {
            Debug.Assert(id >= 0);
            if (id < stringArray.Length)
                return;
            int newLength = stringArray.Length*2;
            if (newLength <= id)
                newLength = id + 1;
            string[] newStringArray = new string[newLength];
            Array.Copy(stringArray, 0, newStringArray, 0, stringArray.Length);
            stringArray = newStringArray;
        }

        static void EnsureIntCapacity(int id, ref int[] intArray)
        {
            Debug.Assert(id >= 0);
            if (id < intArray.Length)
                return;
            int newLength = intArray.Length*2;
            if (newLength <= id)
                newLength = id + 1;
            int[] newIntArray = new int[newLength];
            Array.Copy(intArray, 0, newIntArray, 0, intArray.Length);
            intArray = newIntArray;
        }

        internal void AddTypeVertex(int typeId, string typeName, Graph graph, ref Vertex[] typeVertex, FilterForm filterForm)
        {
            EnsureVertexCapacity(typeId, ref typeVertex);
            typeVertex[typeId] = graph.FindOrCreateVertex(typeName, null, null);
            typeVertex[typeId].interestLevel = filterForm.InterestLevelOfTypeName(typeName, null, finalizableTypes.ContainsKey(typeId));
        }

        internal void AddFunctionVertex(int funcId, string functionName, string signature, Graph graph, ref Vertex[] funcVertex, FilterForm filterForm)
        {
            EnsureVertexCapacity(funcId, ref funcVertex);
            int moduleId = funcModule[funcId];
            string moduleName = null;
            if (moduleId >= 0)
                moduleName = modBasicName[moduleId];
            funcVertex[funcId] = graph.FindOrCreateVertex(functionName, signature, moduleName);
            funcVertex[funcId].interestLevel = filterForm.InterestLevelOfMethodName(functionName, signature);
        }

        void AddTypeName(int typeId, string typeName)
        {
            EnsureStringCapacity(typeId, ref this.typeName);
            this.typeName[typeId] = typeName;
            typeSignatureIdHash[typeName] = typeId;
        }

        int FillBuffer()
        {
            bufPos = 0;
            bufLevel = r.BaseStream.Read(buffer, 0, buffer.Length);
            if (bufPos < bufLevel)
                return buffer[bufPos++];
            else
                return -1;
        }

        internal int ReadChar()
        {
            pos++;
            if (bufPos < bufLevel)
                return buffer[bufPos++];
            else
                return FillBuffer();
        }

        int ReadHex()
        {
            int value = 0;
            while (true)
            {
                c = ReadChar();
                int digit = c;
                if (digit >= '0' && digit <= '9')
                    digit -= '0';
                else if (digit >= 'a' && digit <= 'f')
                    digit -= 'a' - 10;
                else if (digit >= 'A' && digit <= 'F')
                    digit -= 'A' - 10;
                else
                    return value;
                value = value * 16 + digit;
            }
        }

        int ReadInt()
        {
            while (c == ' ' || c == '\t')
                c = ReadChar();
            bool negative = false;
            if (c == '-')
            {
                negative = true;
                c = ReadChar();
            }
            if (c >= '0' && c <= '9')
            {
                int value = 0;
                if (c == '0')
                {
                    c = ReadChar();
                    if (c == 'x' || c == 'X')
                        value = ReadHex();
                }
                while (c >= '0' && c <= '9')
                {
                    value = value * 10 + c - '0';
                    c = ReadChar();
                }

                if (negative)
                    value = -value;
                return value;
            }
            else
            {
                return -1;
            }
        }

        uint ReadUInt()
        {
            return (uint)ReadInt();
        }

        long ReadLongHex()
        {
            long value = 0;
            while (true)
            {
                c = ReadChar();
                int digit = c;
                if (digit >= '0' && digit <= '9')
                    digit -= '0';
                else if (digit >= 'a' && digit <= 'f')
                    digit -= 'a' - 10;
                else if (digit >= 'A' && digit <= 'F')
                    digit -= 'A' - 10;
                else
                    return value;
                value = value * 16 + digit;
            }
        }

        long ReadLong()
        {
            while (c == ' ' || c == '\t')
                c = ReadChar();
            bool negative = false;
            if (c == '-')
            {
                negative = true;
                c = ReadChar();
            }
            if (c >= '0' && c <= '9')
            {
                long value = 0;
                if (c == '0')
                {
                    c = ReadChar();
                    if (c == 'x' || c == 'X')
                        value = ReadLongHex();
                }
                while (c >= '0' && c <= '9')
                {
                    value = value * 10 + c - '0';
                    c = ReadChar();
                }

                if (negative)
                    value = -value;
                return value;
            }
            else
            {
                return -1;
            }
        }

        ulong ReadULong()
        {
            return (ulong)ReadLong();
        }

        string ReadString(StringBuilder sb, char delimiter, bool stopAfterRightParen, int maxLength)
        {
            // Name may contain spaces if they are in angle brackets.
            // Example: <Module>::std_less<unsigned void>.()
            // The name may be truncated at 255 chars by profilerOBJ.dll
            sb.Length = 0;
            int angleBracketsScope = 0;
            while (c > delimiter || angleBracketsScope != 0 && sb.Length < maxLength)
            {
                if (c == '\\')
                {
                    c = ReadChar();
                    if (c == 'u')
                    {
                        // handle unicode escape
                        c = ReadChar();
                        int count = 0;
                        int hexVal = 0;
                        while (count < 4 && ('0' <= c && c <= '9' || 'a' <= c && c <= 'f'))
                        {
                            count++;
                            hexVal = 16 * hexVal + ((c <= '9') ? (c - '0') : (c - 'a' + 10));
                            c = ReadChar();
                        }
                        sb.Append((char)hexVal);
                    }
                    else
                    {
                        // handle other escaped character - append it without inspecting it,
                        // it doesn't count as a delimiter or anything else
                        sb.Append((char)c);
                        c = ReadChar();
                    }
                    continue;
                }

                // non-escaped character - always append it
                sb.Append((char)c);

                if (c == '<')
                    angleBracketsScope++;
                else if (c == '>' && angleBracketsScope > 0)
                    angleBracketsScope--;
                else if (stopAfterRightParen && c == ')')
                {
                    // we have already appened it above - now read the character after it.
                    c = ReadChar();
                    break;
                }

                c = ReadChar();
            }
            return sb.ToString();
        }

        int ForcePosInt()
        {
            int value = ReadInt();
            if (value >= 0)
                return value;
            else
                throw new Exception(string.Format("Bad format in log file {0} line {1}", fileName, line));
        }

        internal static int[] GrowIntVector(int[] vector)
        {
            int[] newVector = new int[vector.Length*2];
            for (int i = 0; i < vector.Length; i++)
                newVector[i] = vector[i];
            return newVector;
        }

        internal static ulong[] GrowULongVector(ulong[] vector)
        {
            ulong[] newVector = new ulong[vector.Length*2];
            for (int i = 0; i < vector.Length; i++)
                newVector[i] = vector[i];
            return newVector;
        }

        internal static bool InterestingCallStack(Vertex[] vertexStack, int stackPtr, FilterForm filterForm)
        {
            if (stackPtr == 0)
                return filterForm.methodFilters.Length == 0;
            if ((vertexStack[stackPtr-1].interestLevel & InterestLevel.Interesting) == InterestLevel.Interesting)
                return true;
            for (int i = stackPtr-2; i >= 0; i--)
            {
                switch (vertexStack[i].interestLevel & InterestLevel.InterestingChildren)
                {
                    case    InterestLevel.Ignore:
                        break;

                    case    InterestLevel.InterestingChildren:
                        return true;

                    default:
                        return false;
                }
            }
            return false;
        }

        internal static int FilterVertices(Vertex[] vertexStack, int stackPtr)
        {
            bool display = false;
            for (int i = 0; i < stackPtr; i++)
            {
                Vertex vertex = vertexStack[i];
                switch (vertex.interestLevel & InterestLevel.InterestingChildren)
                {
                    case    InterestLevel.Ignore:
                        if (display)
                            vertex.interestLevel |= InterestLevel.Display;
                        break;

                    case    InterestLevel.InterestingChildren:
                        display = true;
                        break;

                    default:
                        display = false;
                        break;
                }
            }
            display = false;
            for (int i = stackPtr-1; i >= 0; i--)
            {
                Vertex vertex = vertexStack[i];
                switch (vertex.interestLevel & InterestLevel.InterestingParents)
                {
                    case    InterestLevel.Ignore:
                        if (display)
                            vertex.interestLevel |= InterestLevel.Display;
                        break;

                    case    InterestLevel.InterestingParents:
                        display = true;
                        break;

                    default:
                        display = false;
                        break;
                }
            }
            int newStackPtr = 0;
            for (int i = 0; i < stackPtr; i++)
            {
                Vertex vertex = vertexStack[i];
                if ((vertex.interestLevel & (InterestLevel.Display|InterestLevel.Interesting)) != InterestLevel.Ignore)
                {
                    vertexStack[newStackPtr++] = vertex;
                    vertex.interestLevel &= ~InterestLevel.Display;
                }
            }
            return newStackPtr;
        }

        TimePos[] timePos;
        int timePosCount, timePosIndex;
        const int maxTimePosCount = (1<<23)-1; // ~8,000,000 entries

        void GrowTimePos()
        {
            TimePos[] newTimePos = new TimePos[2*timePos.Length];
            for (int i = 0; i < timePos.Length; i++)
                newTimePos[i] = timePos[i];
            timePos = newTimePos;
        }

        int AddTimePos(int tick, long pos)
        {
            double time = tick*0.001;
            
            // The time stamps can not always be taken at face value.
            // The two problems we try to fix here are:
            // - the time may wrap around (after about 50 days).
            // - on some MP machines, different cpus could drift apart
            // We solve the first problem by adding 2**32*0.001 if the
            // time appears to jump backwards by more than 2**31*0.001.
            // We "solve" the second problem by ignoring time stamps
            // that still jump backward in time.
            double lastTime = 0.0;
            if (timePosIndex > 0)
                lastTime = timePos[timePosIndex-1].time;
            // correct possible wraparound
            while (time + (1L<<31)*0.001 < lastTime)
                time += (1L<<32)*0.001;

            // ignore times that jump backwards
            if (time < lastTime)
                return timePosIndex - 1;

            while (timePosCount >= timePos.Length)
                GrowTimePos();

            // we have only 23 bits to encode allocation time.
            // to avoid running out for long running measurements, we decrease time resolution
            // as we chew up slots. below algorithm uses 1 millisecond resolution for the first
            // million slots, 2 milliseconds for the second million etc. this gives about
            // 2 million seconds time range or 23 days. This is if we really have a time stamp
            // every millisecond - if not, the range is much larger...
            double minimumTimeInc = 0.000999*(1<<timePosIndex/(maxTimePosCount/8));
            if (timePosCount < maxTimePosCount && (time - lastTime >= minimumTimeInc))
            {
                if (timePosIndex < timePosCount)
                {
                    // This is the case where we read the file again for whatever reason
                    Debug.Assert(timePos[timePosIndex].time == time && timePos[timePosIndex].pos == pos);
                    return timePosIndex++;
                }
                else
                {
                    timePos[timePosCount] = new TimePos(time, pos);
                    timePosIndex++;
                    return timePosCount++;
                }
            }
            else
                return timePosIndex - 1;
        }

        // variant of above to give comments their own tick index
        int AddTimePos(long pos)
        {
            double lastTime = 0.0;
            if (timePosIndex > 0)
                lastTime = timePos[timePosIndex-1].time;

            while (timePosCount >= timePos.Length)
                GrowTimePos();

            // stop giving comments their own tick index if we have already
            // burned half the available slots
            if (timePosCount < maxTimePosCount/2)
            {
                if (timePosIndex < timePosCount)
                {
                    // This is the case where we read the file again for whatever reason
                    Debug.Assert(timePos[timePosIndex].time == lastTime && timePos[timePosIndex].pos == pos);
                    return timePosIndex++;
                }
                else
                {
                    timePos[timePosCount] = new TimePos(lastTime, pos);
                    timePosIndex++;
                    return timePosCount++;
                }
            }
            else
                return timePosIndex - 1;
        }

        internal double TickIndexToTime(int tickIndex)
        {
            return timePos[tickIndex].time;
        }

        internal long TickIndexToPos(int tickIndex)
        {
            return timePos[tickIndex].pos;
        }

        internal int TimeToTickIndex(double time)
        {
            int l = 0;
            int r = timePosCount-1;
            if (time < timePos[l].time)
                return l;
            if (timePos[r].time <= time)
                return r;

            // binary search - loop invariant is timePos[l].time <= time && time < timePos[r].time
            // loop terminates because loop condition implies l < m < r and so the interval
            // shrinks on each iteration
            while (l + 1 < r)
            {
                int m = (l + r) / 2;
                if (time < timePos[m].time)
                {
                    r = m;
                }
                else
                {
                    l = m;
                }
            }

            // we still have the loop invariant timePos[l].time <= time && time < timePos[r].time
            // now we just return the index that gives the closer match.
            if (time - timePos[l].time < timePos[r].time - time)
                return l;
            else
                return r;
        }

        enum GcRootKind
        {
            Other = 0x0,
            Stack = 0x1,
            Finalizer = 0x2,
            Handle = 0x3,
        };

        enum GcRootFlags
        {
            Pinning = 0x1,
            WeakRef = 0x2,
            Interior = 0x4,
            Refcounted = 0x8,
        };

        internal void ReadFile(long startFileOffset, long endFileOffset, ReadLogResult readLogResult)
        {
            ReadFile(startFileOffset, endFileOffset, readLogResult, -1);
        }

        internal void ReadFile(long startFileOffset, long endFileOffset, ReadLogResult readLogResult, int requestedIndex)
        {
            ProgressForm progressForm = new ProgressForm();
            progressForm.Text = string.Format("Progress loading {0}", fileName);
            progressForm.Visible = progressFormVisible;
            progressForm.setProgress(0);
            if (stacktraceTable == null)
                stacktraceTable = new StacktraceTable();
            if (timePos == null)
                timePos = new TimePos[1000];
            AddTypeName(0, "Free Space");
            try
            {
                Stream s = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                r = new StreamReader(s);
                for (timePosIndex = timePosCount; timePosIndex > 0; timePosIndex--)
                    if (timePos[timePosIndex-1].pos <= startFileOffset)
                        break;
                // start at the beginning if no later start point available or asked for info that can only
                // be constructed by reading the whole file.
                if (timePosIndex <= 1 || readLogResult.relocatedHistogram != null || readLogResult.finalizerHistogram != null
                                      || readLogResult.criticalFinalizerHistogram != null || readLogResult.liveObjectTable != null)
                {
                    pos = 0;
                    timePosIndex = 1;
                }
                else
                {
                    timePosIndex--;
                    pos = timePos[timePosIndex].pos;
                }
                if (timePosCount == 0)
                {
                    timePos[0] = new TimePos(0.0, 0);
                    timePosCount = timePosIndex = 1;
                }
                s.Position = pos;
                buffer = new byte[4096];
                bufPos = 0;
                bufLevel = 0;
                int maxProgress = (int)(r.BaseStream.Length/1024);
                progressForm.setMaximum(maxProgress);
                line = 1;
                StringBuilder sb = new StringBuilder();
                ulong[] ulongStack = new ulong[1000];
                int[] intStack = new int[1000];
                int stackPtr = 0;
                c = ReadChar();
                bool thisIsR = false, previousWasR;
                bool extendedRootInfoSeen = false;
                int lastTickIndex = 0;
                bool newGcEvent = false;

                while (c != -1)
                {
                    if (pos > endFileOffset)
                        break;
                    if ((line % 1024) == 0)
                    {
                        int currentProgress = (int)(pos/1024);
                        if (currentProgress <= maxProgress)
                        {
                            progressForm.setProgress(currentProgress);
                            Application.DoEvents();
                            if (progressForm.DialogResult == DialogResult.Cancel)
                                break;
                        }
                    }
                    lastLineStartPos = pos-1;
                    previousWasR = thisIsR;
                    thisIsR = false;
                    switch (c)
                    {
                        case    -1:
                            break;

                        case    'F':
                        case    'f':
                        {
                            c = ReadChar();
                            int funcIndex = ReadInt();
                            while (c == ' ' || c == '\t')
                                c = ReadChar();
                            string name = ReadString(sb, ' ', false, 255);
                            while (c == ' ' || c == '\t')
                                c = ReadChar();
                            string signature = ReadString(sb, '\r', true, 1023);

                            ulong addr = ReadULong();
                            uint size = ReadUInt();
                            int modIndex = ReadInt();
                            int stackIndex = ReadInt();

                            if (c != -1)
                            {
                                EnsureStringCapacity(funcIndex, ref funcName);
                                funcName[funcIndex] = name;
                                EnsureStringCapacity(funcIndex, ref funcSignature);
                                funcSignature[funcIndex] = signature;
                                EnsureIntCapacity(funcIndex, ref funcModule);
                                funcModule[funcIndex] = modIndex;

                                string nameAndSignature = name;
                                if (signature != null)
                                    nameAndSignature = name + ' '+signature;

                                if (stackIndex >= 0 && readLogResult.functionList != null)
                                {
                                    funcSignatureIdHash[nameAndSignature] = funcIndex;
                                    readLogResult.functionList.Add(funcIndex, stackIndex, size, modIndex);
                                }
                            }
                            break;
                        }

                        case    'T':
                        case    't':
                        {
                            c = ReadChar();
                            int typeIndex = ReadInt();
                            while (c == ' ' || c == '\t')
                                c = ReadChar();
                            if (c != -1 && Char.IsDigit((char)c))
                            {
                                if (ReadInt() != 0)
                                {
                                    finalizableTypes[typeIndex] = true;
                                }
                            }
                            while (c == ' ' || c == '\t')
                                c = ReadChar();
                            string typeName = ReadString(sb, '\r', false, 1023);
                            if (c != -1)
                            {
                                AddTypeName(typeIndex, typeName);
                            }
                            break;
                        }

                        // 'A' with thread identifier
                        case    '!':
                        {
                            c = ReadChar();
                            int threadId = ReadInt();
                            ulong id = ReadULong();
                            int typeSizeStackTraceIndex = ReadInt();
                            typeSizeStackTraceIndex = stacktraceTable.MapTypeSizeStacktraceId(typeSizeStackTraceIndex);
                            if (c != -1)
                            {
                                if (readLogResult.liveObjectTable != null)
                                    readLogResult.liveObjectTable.InsertObject(id, typeSizeStackTraceIndex, lastTickIndex, lastTickIndex, true, readLogResult.sampleObjectTable);
                                if (pos >= startFileOffset && pos < endFileOffset && readLogResult.allocatedHistogram != null)
                                {
                                    readLogResult.allocatedHistogram.AddObject(typeSizeStackTraceIndex, 1);
                                }
                                List<string> prev;
                                if (assembliesJustLoaded.TryGetValue(threadId, out prev) && prev.Count != 0)
                                {
                                    foreach(string assemblyName in prev)
                                    {
                                        assemblies[assemblyName] = -typeSizeStackTraceIndex;
                                    }
                                    prev.Clear();
                                }
                            }
                            readLogResult.hadAllocInfo = true;
                            readLogResult.hadCallInfo = true;
                            break;
                        }

                        case    'A':
                        case    'a':
                        {
                            c = ReadChar();
                            ulong id = ReadULong();
                            int typeSizeStackTraceIndex = ReadInt();
                            typeSizeStackTraceIndex = stacktraceTable.MapTypeSizeStacktraceId(typeSizeStackTraceIndex);
                            if (c != -1)
                            {
                                if (readLogResult.liveObjectTable != null)
                                    readLogResult.liveObjectTable.InsertObject(id, typeSizeStackTraceIndex, lastTickIndex, lastTickIndex, true, readLogResult.sampleObjectTable);
                                if (pos >= startFileOffset && pos < endFileOffset && readLogResult.allocatedHistogram != null)
                                {
                                    readLogResult.allocatedHistogram.AddObject(typeSizeStackTraceIndex, 1);
                                }
                            }
                            readLogResult.hadAllocInfo = true;
                            readLogResult.hadCallInfo = true;
                            break;
                        }

                        case    'C':
                        case    'c':
                        {
                            c = ReadChar();
                            if (pos <  startFileOffset || pos >= endFileOffset)
                            {
                                while (c >= ' ')
                                    c = ReadChar();
                                break;
                            }
                            int threadIndex = ReadInt();
                            int stackTraceIndex = ReadInt();
                            stackTraceIndex = stacktraceTable.MapTypeSizeStacktraceId(stackTraceIndex);
                            if (c != -1)
                            {
                                if (readLogResult.callstackHistogram != null)
                                {
                                    readLogResult.callstackHistogram.AddObject(stackTraceIndex, 1);
                                }
                                List<string> prev;
                                if (assembliesJustLoaded.TryGetValue(threadIndex, out prev) && prev.Count != 0)
                                {
                                    foreach(string assemblyName in prev)
                                    {
                                        assemblies[assemblyName] = stackTraceIndex;
                                    }
                                    prev.Clear();
                                }
                            }
                            readLogResult.hadCallInfo = true;
                            break;
                        }

                        case    'E':
                        case    'e':
                        {
                            c = ReadChar();
                            extendedRootInfoSeen = true;
                            thisIsR = true;
                            if (pos <  startFileOffset || pos >= endFileOffset)
                            {
                                while (c >= ' ')
                                    c = ReadChar();
                                break;
                            }
                            if (!previousWasR)
                            {
                                heapDumpEventList.AddEvent(lastTickIndex, null);
                                if (readLogResult.objectGraph != null && !readLogResult.objectGraph.empty)
                                {
                                    readLogResult.objectGraph.BuildTypeGraph(new FilterForm());
                                    readLogResult.objectGraph.Neuter();
                                }
                                Histogram[] h = readLogResult.heapDumpHistograms;
                                if (h != null)
                                {
                                    if (h.Length == requestedIndex)
                                        readLogResult.requestedObjectGraph = readLogResult.objectGraph;
                                    readLogResult.heapDumpHistograms = new Histogram[h.Length + 1];
                                    for (int i = 0; i < h.Length; i++)
                                        readLogResult.heapDumpHistograms[i] = h[i];
                                    readLogResult.heapDumpHistograms[h.Length] = new Histogram(this, lastTickIndex);
                                }
                                readLogResult.objectGraph = new ObjectGraph(this, lastTickIndex);
                            }
                            ulong objectID = ReadULong();
                            GcRootKind rootKind = (GcRootKind)ReadInt();
                            GcRootFlags rootFlags = (GcRootFlags)ReadInt();
                            ulong rootID = ReadULong();
                            ObjectGraph objectGraph = readLogResult.objectGraph;
                            if (c != -1 && objectID > 0 && objectGraph != null && (rootFlags & GcRootFlags.WeakRef) == 0)
                            {
                                string rootName;
                                switch (rootKind)
                                {
                                case    GcRootKind.Stack:      rootName = "Stack";        break;
                                case    GcRootKind.Finalizer:  rootName = "Finalizer";    break;
                                case    GcRootKind.Handle:     rootName = "Handle";       break;
                                default:                       rootName = "Other";        break;                      
                                }

                                if ((rootFlags & GcRootFlags.Pinning) != 0)
                                    rootName += ", Pinning";
                                if ((rootFlags & GcRootFlags.WeakRef) != 0)
                                    rootName += ", WeakRef";
                                if ((rootFlags & GcRootFlags.Interior) != 0)
                                    rootName += ", Interior";
                                if ((rootFlags & GcRootFlags.Refcounted) != 0)
                                    rootName += ", RefCounted";

                                int rootTypeId = objectGraph.GetOrCreateGcType(rootName);
                                ulongStack[0] = objectID;
                                ObjectGraph.GcObject rootObject = objectGraph.CreateObject(rootTypeId, 1, ulongStack);

                                objectGraph.AddRootObject(rootObject, rootID);
                            }
                            break;
                        }
                            
                        case    'R':
                        case    'r':
                        {
                            c = ReadChar();
                            thisIsR = true;
                            if (extendedRootInfoSeen || pos <  startFileOffset || pos >= endFileOffset)
                            {
                                while (c >= ' ')
                                    c = ReadChar();
                                Histogram[] h = readLogResult.heapDumpHistograms;
                                if (h != null)
                                {
                                    if (h.Length == requestedIndex)
                                        readLogResult.requestedObjectGraph = readLogResult.objectGraph;
                                }
                                break;
                            }
                            if (!previousWasR)
                            {
                                heapDumpEventList.AddEvent(lastTickIndex, null);
                                if (readLogResult.objectGraph != null && !readLogResult.objectGraph.empty)
                                {
                                    readLogResult.objectGraph.BuildTypeGraph(new FilterForm());
                                    readLogResult.objectGraph.Neuter();
                                }
                                Histogram[] h = readLogResult.heapDumpHistograms;
                                if (h != null)
                                {
                                    if (h.Length == requestedIndex)
                                        readLogResult.requestedObjectGraph = readLogResult.objectGraph;
                                    readLogResult.heapDumpHistograms = new Histogram[h.Length + 1];
                                    for (int i = 0; i < h.Length; i++)
                                        readLogResult.heapDumpHistograms[i] = h[i];
                                    readLogResult.heapDumpHistograms[h.Length] = new Histogram(this, lastTickIndex);
                                }
                                readLogResult.objectGraph = new ObjectGraph(this, lastTickIndex);
                            }
                            stackPtr = 0;
                            ulong objectID;
                            while ((objectID = ReadULong()) != ulong.MaxValue)
                            {
                                if (objectID > 0)
                                {
                                    ulongStack[stackPtr] = objectID;
                                    stackPtr++;
                                    if (stackPtr >= ulongStack.Length)
                                        ulongStack = GrowULongVector(ulongStack);
                                }
                            }
                            if (c != -1)
                            {
                                if (readLogResult.objectGraph != null)
                                    readLogResult.objectGraph.AddRoots(stackPtr, ulongStack);
                            }
                            break;
                        }

                        case    'O':
                        case    'o':
                        {
                            c = ReadChar();
                            if (pos <  startFileOffset || pos >= endFileOffset || readLogResult.objectGraph == null)
                            {
                                while (c >= ' ')
                                    c = ReadChar();
                                break;
                            }
                            ulong objectId = ReadULong();
                            int typeIndex = ReadInt();
                            uint size = ReadUInt();
                            stackPtr = 0;
                            ulong objectID;
                            while ((objectID = ReadULong()) != ulong.MaxValue)
                            {
                                if (objectID > 0)
                                {
                                    ulongStack[stackPtr] = objectID;
                                    stackPtr++;
                                    if (stackPtr >= ulongStack.Length)
                                        ulongStack = GrowULongVector(ulongStack);
                                }
                            }
                            if (c != -1)
                            {
                                ObjectGraph objectGraph = readLogResult.objectGraph;
                                objectGraph.GetOrCreateGcType(typeIndex);

                                int typeSizeStackTraceId = -1;
                                int allocTickIndex = 0;
                                // try to find the allocation stack trace and allocation time
                                // from the live object table
                                if (readLogResult.liveObjectTable != null)
                                {
                                    LiveObjectTable.LiveObject liveObject;
                                    readLogResult.liveObjectTable.GetNextObject(objectId, objectId, out liveObject);
                                    if (liveObject.id == objectId)
                                    {
                                        typeSizeStackTraceId = liveObject.typeSizeStacktraceIndex;
                                        allocTickIndex = liveObject.allocTickIndex;
                                        Histogram[] h = readLogResult.heapDumpHistograms;
                                        if (h != null)
                                            h[h.Length-1].AddObject(liveObject.typeSizeStacktraceIndex, 1);
                                    }
                                }
                                if (typeSizeStackTraceId == -1)
                                    typeSizeStackTraceId = stacktraceTable.GetOrCreateTypeSizeId(typeIndex, (int)size);
                                ObjectGraph.GcObject gcObject = objectGraph.CreateAndEnterObject(objectId, typeSizeStackTraceId, stackPtr, ulongStack);
                                gcObject.AllocTickIndex = allocTickIndex;
                            }
                            break;
                        }

                        case    'M':
                        case    'm':
                        {
                            c = ReadChar();
                            int modIndex = ReadInt();
                            sb.Length = 0;
                            while (c > '\r')
                            {
                                sb.Append((char)c);
                                c = ReadChar();
                            }
                            if (c != -1)
                            {
                                string lineString = sb.ToString();
                                int addrPos = lineString.LastIndexOf(" 0x");
                                if (addrPos <= 0)
                                    addrPos = lineString.Length;
                                int backSlashPos = lineString.LastIndexOf(@"\");
                                if (backSlashPos <= 0)
                                    backSlashPos = -1;
                                string basicName = lineString.Substring(backSlashPos + 1, addrPos - backSlashPos - 1);
                                string fullName = lineString.Substring(0, addrPos);

                                EnsureStringCapacity(modIndex, ref modBasicName);
                                modBasicName[modIndex] = basicName;
                                EnsureStringCapacity(modIndex, ref modFullName);
                                modFullName[modIndex] = fullName;
                            }
                            break;
                        }

                        case    'U':
                        case    'u':
                        {
                            c = ReadChar();
                            ulong oldId = ReadULong();
                            ulong newId = ReadULong();
                            uint length = ReadUInt();
                            Histogram reloHist = null;
                            if (pos >= startFileOffset && pos < endFileOffset)
                                reloHist = readLogResult.relocatedHistogram;
                            if (readLogResult.liveObjectTable != null)
                                readLogResult.liveObjectTable.UpdateObjects(reloHist, oldId, newId, length, lastTickIndex, readLogResult.sampleObjectTable);
                            break;
                        }

                        case    'V':
                        case    'v':
                        {
                            c = ReadChar();
                            ulong startId = ReadULong();
                            uint length = ReadUInt();
                            Histogram reloHist = null;
                            if (pos >= startFileOffset && pos < endFileOffset)
                                reloHist = readLogResult.relocatedHistogram;
                            if (readLogResult.liveObjectTable != null)
                                readLogResult.liveObjectTable.UpdateObjects(reloHist, startId, startId, length, lastTickIndex, readLogResult.sampleObjectTable);
                            break;
                        }

                        case    'B':
                        case    'b':
                            c = ReadChar();
                            int startGC = ReadInt();
                            int induced = ReadInt();
                            int condemnedGeneration = ReadInt();
                            if (startGC != 0)
                                newGcEvent = gcEventList.AddEvent(lastTickIndex, null);
                            if (newGcEvent)
                            {
                                if (startGC != 0)
                                {
                                    if (induced != 0)
                                    {
                                        for (int gen = 0; gen <= condemnedGeneration; gen++)
                                            inducedGcCount[gen]++;
                                    }
                                }
                                else
                                {
                                    int condemnedLimit = condemnedGeneration;
                                    if (condemnedLimit == 2)
                                        condemnedLimit = 3;
                                    for (int gen = 0; gen <= condemnedLimit; gen++)
                                    {
                                        cumulativeGenerationSize[gen] += generationSize[gen];
                                        gcCount[gen]++;
                                    }
                                }
                            }

                            for (int gen = 0; gen <= 3; gen++)
                                generationSize[gen] = 0;

                            while (c >= ' ')
                            {
                                ulong rangeStart = ReadULong();
                                ulong rangeLength = ReadULong();
                                ulong rangeLengthReserved = ReadULong();
                                int rangeGeneration = ReadInt();
                                if (c == -1 || rangeGeneration < 0)
                                    break;
                                if (readLogResult.liveObjectTable != null)
                                {
                                    if (startGC != 0)
                                    {
                                        if (rangeGeneration > condemnedGeneration && condemnedGeneration != 2)
                                            readLogResult.liveObjectTable.Preserve(rangeStart, rangeLength, lastTickIndex);
                                    }
                                    else
                                    {
                                        readLogResult.liveObjectTable.GenerationInterval(rangeStart, rangeLength, rangeGeneration, lastTickIndex);
                                    }
                                }
                                generationSize[rangeGeneration] += rangeLength;
                            }
                            if (startGC == 0 && readLogResult.liveObjectTable != null)
                            {
                                readLogResult.liveObjectTable.RecordGc(lastTickIndex, condemnedGeneration, readLogResult.sampleObjectTable, false);
                            }
                            break;

                        case    'L':
                        case    'l':
                        {
                            c = ReadChar();
                            int isCritical = ReadInt();
                            ulong objectId = ReadULong();
                            if (pos >= startFileOffset && pos < endFileOffset && readLogResult.liveObjectTable != null)
                            {
                                // try to find the allocation stack trace and allocation time
                                // from the live object table
                                LiveObjectTable.LiveObject liveObject;
                                readLogResult.liveObjectTable.GetNextObject(objectId, objectId, out liveObject);
                                if (liveObject.id == objectId)
                                {
                                    if (isCritical != 0 && readLogResult.criticalFinalizerHistogram != null)
                                        readLogResult.criticalFinalizerHistogram.AddObject(liveObject.typeSizeStacktraceIndex, 1);
                                    if (readLogResult.finalizerHistogram != null)
                                        readLogResult.finalizerHistogram.AddObject(liveObject.typeSizeStacktraceIndex, 1);
                                }
                            }
                            break;
                        }

                        case    'I':
                        case    'i':
                            c = ReadChar();
                            int tickCount = ReadInt();
                            if (c != -1)
                            {
                                lastTickIndex = AddTimePos(tickCount, lastLineStartPos);
                                if (maxTickIndex < lastTickIndex)
                                    maxTickIndex = lastTickIndex;
                            }
                            break;

                        case    'G':
                        case    'g':
                            c = ReadChar();
                            int gcGen0Count = ReadInt();
                            int gcGen1Count = ReadInt();
                            int gcGen2Count = ReadInt();
                            // if the newer 'b' lines occur, disregard the 'g' lines.
                            if (gcCount[0] == 0 && readLogResult.liveObjectTable != null)
                            {
                                if (c == -1 || gcGen0Count < 0)
                                    readLogResult.liveObjectTable.RecordGc(lastTickIndex, 0, readLogResult.sampleObjectTable, gcGen0Count < 0);
                                else
                                    readLogResult.liveObjectTable.RecordGc(lastTickIndex, gcGen0Count, gcGen1Count, gcGen2Count, readLogResult.sampleObjectTable);
                            }
                            break;

                        case    'N':
                        case    'n':
                        {
                            c = ReadChar();
                            int funcIndex;
                            int stackTraceIndex = ReadInt();
                            stackPtr = 0;

                            int flag = ReadInt();
                            int matched = flag / 4;
                            int hadTypeId = (flag & 2);
                            bool hasTypeId = (flag & 1) == 1;

                            if (hasTypeId)
                            {
                                intStack[stackPtr++] = ReadInt();
                                intStack[stackPtr++] = ReadInt();
                            }

                            if (matched > 0 && c != -1)
                            {
                                /* use some other stack trace as a reference */
                                int otherStackTraceId = ReadInt();
                                otherStackTraceId = stacktraceTable.MapTypeSizeStacktraceId(otherStackTraceId);
                                int[] stacktrace = stacktraceTable.IndexToStacktrace(otherStackTraceId);
                                if (matched > stacktrace.Length - hadTypeId)
                                    matched = stacktrace.Length - hadTypeId;
                                for(int i = 0; i < matched; i++)
                                {
                                    int funcId = stacktrace[i + hadTypeId];
                                    Debug.Assert(funcId < funcName.Length);
                                    if (funcName[funcId] == null)
                                        funcName[funcId] = String.Empty;
                                    intStack[stackPtr++] = funcId;
                                    if (stackPtr >= intStack.Length)
                                    {
                                        intStack = GrowIntVector(intStack);
                                    }
                                }
                            }

                            while ((funcIndex = ReadInt()) >= 0)
                            {
                                intStack[stackPtr] = funcIndex;
                                stackPtr++;
                                if (stackPtr >= intStack.Length)
                                    intStack = GrowIntVector(intStack);
                            }

                            if (c != -1)
                            {
                                stacktraceTable.Add(stackTraceIndex, intStack, stackPtr, hasTypeId);
                            }
                            break;
                        }

                        case 'y':
                        case 'Y':
                        {
                            c = ReadChar();
                            int threadid = ReadInt();
                            if(!assembliesJustLoaded.ContainsKey(threadid))
                            {
                                assembliesJustLoaded[threadid] = new List<string>();
                            }
                            ReadInt();

                            while (c == ' ' || c == '\t')
                            {
                                c = ReadChar();
                            }
                            sb.Length = 0;
                            while (c > '\r')
                            {
                                sb.Append((char)c);
                                c = ReadChar();
                            }
                            string assemblyName = sb.ToString();
                            assembliesJustLoaded[threadid].Add(assemblyName);
                            break;
                        }

                        case 'S':
                        case 's':
                        {
                            c = ReadChar();
                            int stackTraceIndex = ReadInt();
                            int funcIndex;
                            stackPtr = 0;
                            while ((funcIndex = ReadInt()) >= 0)
                            {
                                intStack[stackPtr] = funcIndex;
                                stackPtr++;
                                if (stackPtr >= intStack.Length)
                                    intStack = GrowIntVector(intStack);
                            }
                            if (c != -1)
                            {
                                stacktraceTable.Add(stackTraceIndex, intStack, stackPtr, false);
                            }
                            break;
                        }

                        case    'Z':
                        case    'z':
                        {
                            sb.Length = 0;
                            c = ReadChar();
                            while (c == ' ' || c == '\t')
                                c = ReadChar();
                            while (c > '\r')
                            {
                                sb.Append((char)c);
                                c = ReadChar();
                            }
                            if (c != -1)
                            {
                                lastTickIndex = AddTimePos(lastLineStartPos);
                                if (maxTickIndex < lastTickIndex)
                                    maxTickIndex = lastTickIndex;
                                commentEventList.AddEvent(lastTickIndex, sb.ToString());
                            }
                            break;
                        }

                        case    'H':
                        case    'h':
                        {
                            c = ReadChar();
                            int threadId = ReadInt();
                            ulong handleId = ReadULong();
                            ulong initialObjectId = ReadULong();
                            int stacktraceId = ReadInt();
                            if (c != -1)
                            {
                                if (readLogResult.handleHash != null)
                                    readLogResult.handleHash[handleId] = new HandleInfo(threadId, handleId, initialObjectId, lastTickIndex, stacktraceId);
                                if (readLogResult.createdHandlesHistogram != null)
                                    readLogResult.createdHandlesHistogram.AddObject(stacktraceId, 1);
                            }
                            break;
                        }

                        case    'J':
                        case    'j':
                        {
                            c = ReadChar();
                            int threadId = ReadInt();
                            ulong handleId = ReadULong();
                            int stacktraceId = ReadInt();
                            if (c != -1)
                            {
                                if (readLogResult.handleHash != null)         
                                {
                                    if (readLogResult.handleHash.ContainsKey(handleId))
                                        readLogResult.handleHash.Remove(handleId);
                                    else
                                    {
                                    }
                                }
                                if (readLogResult.destroyedHandlesHistogram != null)
                                    readLogResult.destroyedHandlesHistogram.AddObject(stacktraceId, 1);
                            }
                            break;
                        }
                        
                        default:
                        {
                            // just ignore the unknown
                            while(c != '\n' && c != '\r')
                            {
                                c = ReadChar();
                            }
                            break;
                        }
                    }
                    while (c == ' ' || c == '\t')
                        c = ReadChar();
                    if (c == '\r')
                        c = ReadChar();
                    if (c == '\n')
                    {
                        c = ReadChar();
                        line++;
                    }
                }
            }
            finally
            {
                progressForm.Visible = false;
                progressForm.Dispose();
                if (r != null)
                    r.Close();
            }
        }
    }
}
