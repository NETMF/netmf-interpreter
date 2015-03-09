////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace CLRProfiler
{
    /// <summary>
    /// Summary description for ObjectGraph.
    /// </summary>
    public class ObjectGraph
    {
        internal class IntEqualityComparer : IEqualityComparer<int>
        {
            public bool Equals(int a, int b)
            {
                return a == b;
            }
            public int GetHashCode(int a)
            {
                return a;
            }
        }

        internal class IdToObject
        {
            internal const int lowAddressBits = 25; // one subtable for 32 MB of address space
            internal const int lowAddressMask = (1 << lowAddressBits) - 1;
            internal const int bucketBits = 9; // one bucket for 512 bytes of address space
            internal const int bucketSize = (1 << bucketBits);
            internal const int bucketMask = bucketBits - 1;
            internal const int alignBits = 2; // assume at least DWORD alignment so the lower two bits don't have to be represented
            internal const int idBits = bucketBits - alignBits; // therefore only 7 bits have to be stored in the object
            internal const int idMask = (1 << idBits) - 1;

            GcObject[][] masterTable;

            internal IdToObject()
            {
                masterTable = new GcObject[1][];
            }

            internal void GrowMasterTable()
            {
                GcObject[][] newmasterTable = new GcObject[masterTable.Length * 2][];
                for (int i = 0; i < masterTable.Length; i++)
                    newmasterTable[i] = masterTable[i];
                masterTable = newmasterTable;
            }

            internal GcObject this[ulong objectID]
            {
                get
                {
                    GcObject o;
                    int lowBits = (int)(objectID & lowAddressMask);
                    int highBits = (int)(objectID >> lowAddressBits);
                    if (highBits >= masterTable.Length)
                        return null;
                    GcObject[] subTable = masterTable[highBits];
                    if (subTable == null)
                        return null;
                    int bucket = lowBits >> bucketBits;
                    lowBits = (lowBits >> alignBits) & idMask;
                    o = subTable[bucket];
                    while (o != null && o.Id != lowBits)
                        o = o.nextInHash;
                    return o;
                }
                set
                {
                    int lowBits = (int)(objectID & lowAddressMask);
                    int highBits = (int)(objectID >> lowAddressBits);
                    while (highBits >= masterTable.Length)
                    {
                        GrowMasterTable();
                    }
                    GcObject[] subTable = masterTable[highBits];
                    if (subTable == null)
                    {
                        masterTable[highBits] = subTable = new GcObject[1 << (lowAddressBits - bucketBits)];
                    }
                    int bucket = lowBits >> bucketBits;
                    lowBits = (lowBits >> alignBits) & idMask;
                    value.Id = lowBits;
                    value.nextInHash = subTable[bucket];
                    subTable[bucket] = value;
                }
            }

            public IEnumerator<KeyValuePair<ulong, GcObject>> GetEnumerator()
            {
                for (int i = 0; i < masterTable.Length; i++)
                {
                    GcObject[] subTable = masterTable[i];
                    if (subTable == null)
                        continue;
                    for (int j = 0; j < subTable.Length; j++)
                    {
                        for (GcObject gcObject = subTable[j]; gcObject != null; gcObject = gcObject.nextInHash)
                        {
                            yield return new KeyValuePair<ulong, GcObject>(((ulong)i<<lowAddressBits)+ 
                                                                           ((ulong)j << bucketBits) +
                                                                           ((ulong)gcObject.Id << alignBits), gcObject);
                        }
                    }
                }
            }

            public IEnumerable<GcObject> Values
            {
                get
                {
                    for (int i = 0; i < masterTable.Length; i++)
                    {
                        GcObject[] subTable = masterTable[i];
                        if (subTable == null)
                            continue;
                        for (int j = 0; j < subTable.Length; j++)
                        {
                            for (GcObject gcObject = subTable[j]; gcObject != null; gcObject = gcObject.nextInHash)
                            {
                                yield return gcObject;
                            }
                        }
                    }
                }
            }
        }

        internal IdToObject idToObject;
        internal Dictionary<string, GcType> typeNameToGcType;
        internal Dictionary<int, GcType> typeIdToGcType;
        internal int internalTypeCount;

        internal int unknownTypeId;
        internal ReadNewLog readNewLog;
        internal int tickIndex;

        internal GcObject[] roots;
        internal ulong[] rootIDs;

        internal int rootCount;
        const int initialRootCount = 100;
        internal bool empty;
        IntEqualityComparer intEqualityComparer;

        internal void Neuter()
        {
            idToObject = new IdToObject();
            typeNameToGcType = new Dictionary<string, GcType>();
            typeIdToGcType = new Dictionary<int, GcType>(intEqualityComparer);
            addressToForwardReferences = new Dictionary<ulong, ForwardReference>();
            roots = null;
            rootIDs = null;
        }

        internal ObjectGraph(ReadNewLog readNewLog, int tickIndex)
        {
            //
            // need to add constructor logic here
            //
            idToObject = new IdToObject();
            typeNameToGcType = new Dictionary<string, GcType>();
            intEqualityComparer = new IntEqualityComparer();
            typeIdToGcType = new Dictionary<int, GcType>(intEqualityComparer);
            addressToForwardReferences = new Dictionary<ulong, ForwardReference>();
            unknownTypeId = GetOrCreateGcType("<unknown type>");   
            this.readNewLog = readNewLog;
            this.tickIndex = tickIndex;
            empty = true;
        }

        // this is optimized for space at the expense of programming convenience
        // heap dumps can contain many millions of objects
        internal class GcObject
        {
            internal GcObject parent;
            internal Vertex vertex;
            internal int idTypeSizeStackTraceId;
            const int idBits = IdToObject.idBits;
            const int idMask = IdToObject.idMask;
            const int typeSizeStackTraceIdBits = 32 - idBits;
            internal int Id
            {
                get
                {
                    return idTypeSizeStackTraceId & idMask;
                }
                set
                {
                    idTypeSizeStackTraceId = (idTypeSizeStackTraceId & ~idMask) | (value & idMask);
                }
            }
            internal int TypeSizeStackTraceId
            {
                get
                {
                    return idTypeSizeStackTraceId >> idBits;
                }
                set
                {
                    idTypeSizeStackTraceId = (idTypeSizeStackTraceId & idMask) | (value << idBits);
                }
            }
            internal uint Size(ObjectGraph graph)
            {
                int typeSizeStackTraceId = TypeSizeStackTraceId;
                if (typeSizeStackTraceId < 0)
                    return 0;
                int[] stackTrace = graph.readNewLog.stacktraceTable.IndexToStacktrace(typeSizeStackTraceId);
                return (uint)stackTrace[1];
            }

            internal GcType Type(ObjectGraph graph)
            {
                int typeSizeStackTraceId = TypeSizeStackTraceId;
                if (typeSizeStackTraceId < 0)
                    return graph.typeIdToGcType[typeSizeStackTraceId];
                else
                {
                    int[] stackTrace = graph.readNewLog.stacktraceTable.IndexToStacktrace(typeSizeStackTraceId);
                    int typeID = stackTrace[0];
                    return graph.typeIdToGcType[typeID];
                }
            }

            internal int interestLevelAllocTickIndex; // high 8 bits for interest level, low 24 bits for allocTickIndex;
            internal InterestLevel InterestLevel
            {
                get
                {
                    return (InterestLevel)(interestLevelAllocTickIndex >> 24);
                }
                set
                {
                    interestLevelAllocTickIndex &= 0x00ffffff;
                    interestLevelAllocTickIndex |= (int)value << 24;
                }
            }
            internal int AllocTickIndex
            {
                get
                {
                    return interestLevelAllocTickIndex & 0x00ffffff;
                }
                set
                {
                    interestLevelAllocTickIndex &= unchecked((int)0xff000000);
                    interestLevelAllocTickIndex |= value & 0x00ffffff;
                }
            }
            internal virtual IEnumerable<GcObject> References
            {
                get
                {
                    yield break;
                }
            }
            internal virtual void SetReference(int referenceNumber, GcObject target)
            {
            }
            internal GcObject nextInHash;

            internal static GcObject CreateGcObject(int numberOfReferences)
            {
                switch (numberOfReferences)
                {
                    case 0: return new GcObject();
                    case 1: return new GcObjectWith1Reference();
                    case 2: return new GcObjectWith2References();
                    case 3: return new GcObjectWith3References();
                    case 4: return new GcObjectWith4References();
                    case 5: return new GcObjectWith5References();
                    default: return new GcObjectWithManyReferences(numberOfReferences);
                }
            }
        }

        class GcObjectWith1Reference : GcObject
        {
            protected GcObject reference0;
            internal override IEnumerable<GcObject> References
            {
                get
                {
                    yield return reference0;
                }
            }
            internal override void SetReference(int referenceNumber, GcObject target)
            {
                Debug.Assert(referenceNumber == 0);
                reference0 = target;
            }
        }

        class GcObjectWith2References : GcObjectWith1Reference
        {
            protected GcObject reference1;
            internal override IEnumerable<GcObject> References
            {
                get
                {
                    yield return reference0;
                    yield return reference1;
                }
            }

            internal override void SetReference(int referenceNumber, GcObject target)
            {
                switch (referenceNumber)
                {
                    case 0: reference0 = target; break;
                    case 1: reference1 = target; break;
                    default: Debug.Assert(referenceNumber == 0); break;
                }
            }
        }

        class GcObjectWith3References : GcObjectWith2References
        {
            protected GcObject reference2;
            internal override IEnumerable<GcObject> References
            {
                get
                {
                    yield return reference0;
                    yield return reference1;
                    yield return reference2;
                }
            }

            internal override void SetReference(int referenceNumber, GcObject target)
            {
                switch (referenceNumber)
                {
                    case 0: reference0 = target; break;
                    case 1: reference1 = target; break;
                    case 2: reference2 = target; break;
                    default: Debug.Assert(referenceNumber == 0); break;
                }
            }
        }

        class GcObjectWith4References : GcObjectWith3References
        {
            protected GcObject reference3;
            internal override IEnumerable<GcObject> References
            {
                get
                {
                    yield return reference0;
                    yield return reference1;
                    yield return reference2;
                    yield return reference3;
                }
            }

            internal override void SetReference(int referenceNumber, GcObject target)
            {
                switch (referenceNumber)
                {
                    case 0: reference0 = target; break;
                    case 1: reference1 = target; break;
                    case 2: reference2 = target; break;
                    case 3: reference3 = target; break;
                    default: Debug.Assert(referenceNumber == 0); break;
                }
            }
        }

        class GcObjectWith5References : GcObjectWith4References
        {
            protected GcObject reference4;
            internal override IEnumerable<GcObject> References
            {
                get
                {
                    yield return reference0;
                    yield return reference1;
                    yield return reference2;
                    yield return reference3;
                    yield return reference4;
                }
            }

            internal override void SetReference(int referenceNumber, GcObject target)
            {
                switch (referenceNumber)
                {
                    case 0: reference0 = target; break;
                    case 1: reference1 = target; break;
                    case 2: reference2 = target; break;
                    case 3: reference3 = target; break;
                    case 4: reference4 = target; break;
                    default: Debug.Assert(referenceNumber == 0); break;
                }
            }
        }

        class GcObjectWithManyReferences : GcObject
        {
            GcObject[] references;
            internal GcObjectWithManyReferences(int numberOfReferences)
            {
                references = new GcObject[numberOfReferences];
            }

            internal override IEnumerable<GcObject> References
            {
                get
                {
                    for (int i = 0; i < references.Length; i++)
                        yield return references[i];
                }
            }

            internal override void SetReference(int referenceNumber, GcObject target)
            {
                references[referenceNumber] = target;
            }
        }

        class ForwardReference
        {
            internal ForwardReference(GcObject source, int referenceNumber, ForwardReference next)
            {
                this.source = source;
                this.referenceNumber = referenceNumber;
                this.next = next;
            }
            internal GcObject         source;            // the object having the forward reference
            internal int              referenceNumber;   // the number of the reference within the object
            internal ForwardReference next;              // next forward reference to the same address
        }

        Dictionary<ulong, ForwardReference> addressToForwardReferences;

        void CreateForwardReference(ulong targetAddress, GcObject source, int referenceNumber)
        {
            ForwardReference nextForwardReference;
            addressToForwardReferences.TryGetValue(targetAddress, out nextForwardReference);
            addressToForwardReferences[targetAddress] = new ForwardReference(source, referenceNumber, nextForwardReference);
        }

        void FillInForwardReferences(ulong address, GcObject target)
        {
            ForwardReference forwardReference;
            if (addressToForwardReferences.TryGetValue(address, out forwardReference))
            {
                while (forwardReference != null)
                {
                    forwardReference.source.SetReference(forwardReference.referenceNumber, target);
                    forwardReference = forwardReference.next;
                }
            }
            addressToForwardReferences.Remove(address);
        }

        internal class GcType
        {
            internal GcType(string name, int typeID)
            {
                this.name = name;
                this.typeID = typeID;
            }
            internal string name;

            internal int index;
            internal int typeID;

            internal InterestLevel interestLevel;
        }

        internal GcObject CreateObject(int typeSizeStackTraceId, int numberOfReferences, ulong[] references)
        {
            GcObject o = GcObject.CreateGcObject(numberOfReferences);
            o.TypeSizeStackTraceId = typeSizeStackTraceId;

            for (int i = 0; i < numberOfReferences; i++)
            {
                GcObject target = idToObject[references[i]];
                if (target != null)
                    o.SetReference(i, target);
                else
                    CreateForwardReference(references[i], o, i);
            }
            empty = false;
            return o;
        }

        internal GcObject CreateAndEnterObject(ulong objectID, int typeSizeStackTraceId, int numberOfReferences, ulong[] references)
        {
            GcObject o = CreateObject(typeSizeStackTraceId, numberOfReferences, references);

            idToObject[objectID] = o;

            FillInForwardReferences(objectID, o);

            return o;
        }

        internal void GetOrCreateGcType(int typeID)
        {
            GcType t;
            if (!typeIdToGcType.TryGetValue(typeID, out t))
            {
                string typeName = readNewLog.typeName[typeID];
                t = new GcType(typeName, typeID);
                typeIdToGcType[typeID] = t;
            }
        }

        internal int GetOrCreateGcType(string typeName)
        {
            Debug.Assert(typeName != null);
            GcType t;
            if (!typeNameToGcType.TryGetValue(typeName, out t))
            {
                internalTypeCount++;
                int typeID = -internalTypeCount;
                t = new GcType(typeName, typeID);
                typeNameToGcType[typeName] = t;
                typeIdToGcType[typeID] = t;
            }
            return t.typeID;
        }

        internal void GrowRoots()
        {
            GcObject[] newRoots = new GcObject[roots.Length*2];
            ulong[] newRootIDs = new ulong[roots.Length*2];
            for (int i = 0; i < roots.Length; i++)
            {
                newRoots[i] = roots[i];
                newRootIDs[i] = rootIDs[i];
            }
            roots = newRoots;
            rootIDs = newRootIDs;
        }

        internal void AddRoots(int count, ulong[] refIDs)
        {
            if (roots == null)
            {
                roots = new GcObject[initialRootCount];
                rootIDs = new ulong[initialRootCount];
            }
            while (roots.Length < rootCount + count)
                GrowRoots();
            for (int i = 0; i < count; i++)
            {
                roots[rootCount] = null;
                rootIDs[rootCount] = refIDs[i];
                rootCount++;
            }
        }

        internal void AddRootObject(GcObject rootObject, ulong rootID)
        {
            if (roots == null)
            {
                roots = new GcObject[initialRootCount];
                rootIDs = new ulong[initialRootCount];
            }
            if (roots.Length < rootCount + 1)
                GrowRoots();
            rootIDs[rootCount] = rootID;
            roots[rootCount] = rootObject;
            rootCount++;
        }

        void AssignParents(GcObject rootObject)
        {
            // We use a breadth first traversal of the object graph.
            // To do this, we make use of a queue of objects still to process.

            // Initialize
            Queue<GcObject> queue = new Queue<GcObject>();
            queue.Enqueue(rootObject);

            // Loop
            while (queue.Count != 0)
            {
                GcObject head = queue.Dequeue();
                foreach (GcObject refObject in head.References)
                {
                    if (refObject.parent == null)
                    {
                        refObject.parent = head;
                        queue.Enqueue(refObject);
                    }
                }
            }
        }

        int[] typeHintTable;

        private string FormatAddress(ulong addr)
        {
            if (addr > uint.MaxValue)
                return string.Format("{0:X2}.{1:X4}.{2:X4}", addr >> 32, (addr >> 16) & 0xffff, addr & 0xffff);
            else
                return string.Format("{0:X4}.{1:X4}", (addr >> 16) & 0xffff, addr & 0xffff);
        }

        internal string SignatureOfObject(ulong id, GcObject gcObject, BuildTypeGraphOptions options)
        {
            StringBuilder sb = new StringBuilder();
            if (gcObject.parent != null)
            {
                switch (options)
                {
                    case    BuildTypeGraphOptions.IndividualObjects:
                        if (gcObject.Type(this).name == "Stack" || gcObject.Type(this).name.StartsWith("Stack, "))
                        {
                            if (id < (ulong)readNewLog.funcName.Length)
                                sb.AppendFormat(readNewLog.funcName[id] + "  " + readNewLog.funcSignature[id]);
                            else
                                sb.AppendFormat("Function id = {0}", id);
                        }
                        else
                            sb.AppendFormat("Address = {0}, size = {1:n0} bytes", FormatAddress(id), gcObject.Size(this));
                        break;

                    case    BuildTypeGraphOptions.LumpBySignature:
                        sb.Append(gcObject.parent.Type(this).name);
                        sb.Append("->");
                        sb.Append(gcObject.Type(this).name);
                        List<GcObject> references = new List<GcObject>();
                        foreach (GcObject refObject in gcObject.References)
                        {
                            references.Add(refObject);
                        }

                        if (references.Count > 0)
                        {
                            sb.Append("->(");

                            const int MAXREFTYPECOUNT = 3;
                            List<string> typeNameList = new List<string>(MAXREFTYPECOUNT);
                            string separator = "";
                            int refTypeCount = 0;
                            for (int i = 0; i < references.Count; i++)
                            {
                                GcObject refObject = references[i];
                                GcType refType = refObject.Type(this);
                                if (typeHintTable[refType.index] < i && references[typeHintTable[refType.index]].Type(this) == refType)
                                {
                                    ;   // we already found this type - ignore further occurrences
                                }
                                else
                                {
                                    typeHintTable[refType.index] = i;
                                    refTypeCount++;
                                    if (refTypeCount <= MAXREFTYPECOUNT)
                                    {
                                        typeNameList.Add(refType.name);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                i++;
                            }
                            typeNameList.Sort();

                            foreach (string typeName in typeNameList)
                            {
                                sb.Append(separator);
                                separator = ",";
                                sb.Append(typeName);
                            }

                            if (refTypeCount > MAXREFTYPECOUNT)
                                sb.Append(",...");

                            sb.Append(")");
                        }
                        break;

                    default:
                        Debug.Assert(false);
                        break;
                }
            }
            return sb.ToString();
        }

        internal Vertex FindVertex(ulong id, GcObject gcObject, Graph graph, BuildTypeGraphOptions options)
        {
            Vertex vertex = gcObject.vertex;
            if (vertex != null)
                return vertex;

            string signature = SignatureOfObject(id, gcObject, options);
            vertex = graph.FindOrCreateVertex(gcObject.Type(this).name, signature, null);
            gcObject.vertex = vertex;

            return vertex;
        }

        internal static Graph cachedGraph;

        const int historyDepth = 3;

        private void MarkDescendants(GcObject gcObject)
        {
            foreach (GcObject refObject in gcObject.References)
            {
                if (refObject.InterestLevel == InterestLevel.Ignore)
                {
                    refObject.InterestLevel |= InterestLevel.Display;
                    MarkDescendants(refObject);
                }
            }
        }

        internal void AssignInterestLevelsToTypes(BuildTypeGraphOptions options, FilterForm filterForm)
        {
            foreach (GcType gcType in typeIdToGcType.Values)
            {
                // Otherwise figure which the interesting types are.

                gcType.interestLevel = filterForm.InterestLevelOfTypeName(gcType.name, null, readNewLog.finalizableTypes.ContainsKey(gcType.typeID));
            }
        }

        // Check whether we have a parent object interested in this one
        private bool CheckForParentMarkingDescendant(GcObject gcObject)
        {
            GcObject parentObject = gcObject.parent;
            if (parentObject == null)
                return false;
            switch (parentObject.InterestLevel & InterestLevel.InterestingChildren)
            {
                // Parent says it wants to show children
                case InterestLevel.InterestingChildren:
                    gcObject.InterestLevel |= InterestLevel.Display;
                    return true;

                // Parent is not interesting - check its parent
                case InterestLevel.Ignore:
                    if (CheckForParentMarkingDescendant(parentObject))
                    {
                        gcObject.InterestLevel |= InterestLevel.Display;
                        return true;
                    }
                    else
                        return false;

                default:
                    return false;
            }
        }

        internal void AssignInterestLevelToObject(ulong id, GcObject gcObject, BuildTypeGraphOptions options, FilterForm filterForm)
        {
            // The initial interest level in objects is the one of their type
            InterestLevel interestLevel = gcObject.Type(this).interestLevel;

            if (filterForm.signatureFilters.Length != 0)
            {
                string signature = SignatureOfObject(id, gcObject, BuildTypeGraphOptions.LumpBySignature);
                interestLevel &= filterForm.InterestLevelOfTypeName(gcObject.Type(this).name, signature, readNewLog.finalizableTypes.ContainsKey(gcObject.Type(this).typeID));
            }

            if (filterForm.addressFilters.Length != 0)
            {
                interestLevel &= filterForm.InterestLevelOfAddress(id);
            }

            gcObject.InterestLevel |= interestLevel;

            // Check if this is an interesting object, and we are supposed to mark its ancestors
            if ((gcObject.InterestLevel & InterestLevel.InterestingParents) == InterestLevel.InterestingParents)
            {
                for (GcObject parentObject = gcObject.parent; parentObject != null; parentObject = parentObject.parent)
                {
                    // As long as we find uninteresting objects, mark them for display
                    // When we find an interesting object, we stop, because either it
                    // will itself mark its parents, or it isn't interested in them (and we
                    // respect that despite the interest of the current object, somewhat arbitrarily).
                    if ((parentObject.InterestLevel & InterestLevel.InterestingParents) == InterestLevel.Ignore)
                        parentObject.InterestLevel |= InterestLevel.Display;
                    else
                        break;
                }
            }

            // It's tempting here to mark the descendants as well, but they may be reached via
            // long reference paths, when there are shorter ones that were deemed uninteresting
            // Instead, we look whether our parent objects are interesting and want to show their
            // descendants, but we have to do that in a separate pass.
        }

        internal enum BuildTypeGraphOptions
        {
            LumpBySignature,
            IndividualObjects
        }

        internal Graph BuildTypeGraph(FilterForm filterForm)
        {
            return BuildTypeGraph(-1, int.MaxValue, BuildTypeGraphOptions.LumpBySignature, filterForm);
        }

        internal double TickIndexToTime(int tickIndex)
        {
            if (tickIndex < 0)
                tickIndex = 0;
            if (tickIndex > MainForm.instance.log.maxTickIndex)
                tickIndex = MainForm.instance.log.maxTickIndex;
            return MainForm.instance.log.TickIndexToTime(tickIndex);
        }

        internal Graph BuildTypeGraph(int allocatedAfterTickIndex, int allocatedBeforeTickIndex, BuildTypeGraphOptions options, FilterForm filterForm)
        {
            Graph graph;

			if (   filterForm.filterVersion != 0
                || options != BuildTypeGraphOptions.LumpBySignature
                || allocatedAfterTickIndex > 0
                || allocatedBeforeTickIndex < int.MaxValue)
            {
                graph = new Graph(this);
                graph.graphType = Graph.GraphType.HeapGraph;
                graph.previousGraphTickIndex = allocatedAfterTickIndex;
            }
            else
            {
                Graph previousGraph = cachedGraph;
                if (previousGraph != null && previousGraph.graphSource == this)
                    return previousGraph;
                cachedGraph = graph = new Graph(this);
                graph.graphType = Graph.GraphType.HeapGraph;
                graph.graphSource = this;
                if (previousGraph != null)
                {
                    graph.previousGraphTickIndex = ((ObjectGraph)previousGraph.graphSource).tickIndex;
                    foreach (Vertex v in previousGraph.vertices.Values)
                    {
                        Vertex newV = graph.FindOrCreateVertex(v.name, v.signature, v.moduleName);
                        if (v.weightHistory == null)
                            newV.weightHistory = new ulong[1];
                        else
                        {
                            ulong[] weightHistory = v.weightHistory;
                            newV.weightHistory = new ulong[Math.Min(weightHistory.Length + 1, historyDepth)];
                            for (int i = v.weightHistory.Length - 1; i > 0; i--)
                                newV.weightHistory[i] = weightHistory[i - 1];
                        }
                        newV.weightHistory[0] = v.weight;
                    }
                }
            }
            graph.typeGraphOptions = options;
            graph.filterVersion = filterForm.filterVersion;
            if (graph.previousGraphTickIndex < graph.allocatedAfterTickIndex)
                graph.previousGraphTickIndex = graph.allocatedAfterTickIndex;
            graph.allocatedAfterTickIndex = allocatedAfterTickIndex;
            graph.allocatedBeforeTickIndex = allocatedBeforeTickIndex;

            GcObject rootObject = CreateRootObject();

            for (int i = 0; i < rootCount; i++)
            {
                roots[i].InterestLevel = InterestLevel.Ignore;
            }
            foreach (GcObject gcObject in idToObject.Values)
            {
                gcObject.parent = null;
                gcObject.vertex = null;
                gcObject.InterestLevel = InterestLevel.Ignore;
            }

            AssignParents(rootObject);

            int index = 0;
            foreach (GcType gcType in typeIdToGcType.Values)
            {
                gcType.index = index++;
            }
            GcType[] gcTypes = new GcType[index];
            typeHintTable = new int[index];

            foreach (GcType gcType in typeIdToGcType.Values)
            {
                gcTypes[gcType.index] = gcType;
            }

            AssignInterestLevelsToTypes(options, filterForm);
            for (int i = 0; i < rootCount; i++)
            {
                AssignInterestLevelToObject(rootIDs[i], roots[i], options, filterForm);
            }

            foreach (KeyValuePair<ulong, GcObject> keyValuePair in idToObject)
            {
                AssignInterestLevelToObject(keyValuePair.Key, keyValuePair.Value, options, filterForm);
            }
            foreach (GcObject gcObject in idToObject.Values)
            {
                if (gcObject.InterestLevel == InterestLevel.Ignore)
                    CheckForParentMarkingDescendant(gcObject);
            }

            FindVertex(0, rootObject, graph, options);
            for (int i = 0; i < rootCount; i++)
            {
                roots[i].vertex = null;
                FindVertex(rootIDs[i], roots[i], graph, options);
            }

            foreach (KeyValuePair<ulong, GcObject> keyValuePair in idToObject)
            {
                ulong id = keyValuePair.Key;
                GcObject gcObject = keyValuePair.Value;
                if (    gcObject.parent == null
                    || (gcObject.InterestLevel & (InterestLevel.Interesting | InterestLevel.Display)) == InterestLevel.Ignore)
                {
                    continue;
                }
                FindVertex(id, gcObject, graph, options);
            }

            Vertex[] pathFromRoot = new Vertex[32];

            foreach (GcObject gcObject in idToObject.Values)
            {
                if (    gcObject.parent == null
                    || (gcObject.InterestLevel & (InterestLevel.Interesting | InterestLevel.Display)) == InterestLevel.Ignore
                    || gcObject.AllocTickIndex <= allocatedAfterTickIndex
                    || gcObject.AllocTickIndex >= allocatedBeforeTickIndex)
                {
                    continue;
                }

                int levels = 0;
                for (GcObject pathObject = gcObject; pathObject != null; pathObject = pathObject.parent)
                {
                    if (pathObject.vertex != null)
                        levels++;
                }

                while (pathFromRoot.Length < levels + 1)
                {
                    pathFromRoot = new Vertex[pathFromRoot.Length * 2];
                }

                int level = levels;
                for (GcObject pathObject = gcObject; pathObject != null; pathObject = pathObject.parent)
                {
                    if (pathObject.vertex != null)
                    {
                        level--;
                        pathFromRoot[level] = pathObject.vertex;
                    }
                }

                levels = Vertex.SqueezeOutRepetitions(pathFromRoot, levels);

                for (int j = 0; j < levels - 1; j++)
                {
                    Vertex fromVertex = pathFromRoot[j];
                    Vertex toVertex = pathFromRoot[j + 1];
                    Edge edge = graph.FindOrCreateEdge(fromVertex, toVertex);
                    edge.AddWeight(gcObject.Size(this));
                }

                Vertex thisVertex = pathFromRoot[levels - 1];
                thisVertex.basicWeight += gcObject.Size(this);
                thisVertex.count += 1;
            }

            foreach (Vertex v in graph.vertices.Values)
            {
                if (v.weight < v.outgoingWeight)
                    v.weight = v.outgoingWeight;
                if (v.weight < v.incomingWeight)
                    v.weight = v.incomingWeight;
                if (v.weightHistory == null)
                    v.weightHistory = new ulong[1];
            }

            foreach (Vertex v in graph.vertices.Values)
                v.active = true;
            graph.BottomVertex.active = false;

            return graph;
        }

        internal void PrintGCRoot(ulong[] path)
        {
            Console.WriteLine("<GcRoot>");
            Console.WriteLine("<!-- ");
            for (int j = 0; j < path.Length; j++)
            {
                GcObject temp = idToObject[path[j]];
                if (temp != null)
                    System.Console.WriteLine("{0}, {1:X} ->", temp.Type(this).name, path[j]);
            }
            Console.WriteLine("-->");
            Console.WriteLine("</GcRoot>");
        }

        internal void PrintStackTrace(ulong[] path)
        {
            System.Console.WriteLine("<StackTrace>");
            if ((path != null) && (path.Length > 0))
            {
                GcObject tempGcObject = idToObject[path[path.Length - 1]];
                Console.WriteLine("<!-- ");
                int[] stacktrace = readNewLog.stacktraceTable.IndexToStacktrace(tempGcObject.TypeSizeStackTraceId);
                for (int i = stacktrace.Length - 1; i >= 2; i--)
                    System.Console.WriteLine("{0} <-", readNewLog.funcName[stacktrace[i]]);
                Console.WriteLine("-->");
            }
            System.Console.WriteLine("</StackTrace>");
        }

        Dictionary<GcObject, ulong> objectToId;        // reverse mapping from gc objects to their addresses

		internal void WriteVertexPaths(int allocatedAfterTickIndex, int allocatedBeforeTickIndex, string typeName)
		{
            BuildTypeGraph(new FilterForm());

            if (objectToId == null)
            {
                objectToId = new Dictionary<GcObject, ulong>();
                // initialize the reverse mapping
                foreach (KeyValuePair<ulong, GcObject> keyValuePair in idToObject)
                {
                    objectToId[keyValuePair.Value] = keyValuePair.Key;
                }
            }

			ulong[][] idsFromRoot = new ulong[1][];
			Vertex[] pathFromRoot = new Vertex[32];
			int counter = 0;
			foreach (GcObject gcObject in idToObject.Values)
			{
                if (gcObject.Type(this).name.CompareTo(typeName) != 0)
                {
                    continue;
                }

                if (   gcObject.AllocTickIndex <= allocatedAfterTickIndex
                    || gcObject.AllocTickIndex >= allocatedBeforeTickIndex)
				{
					continue;
				}
				ulong[][] _idsFromRoot = idsFromRoot;
				if (_idsFromRoot.Length <= counter)
				{
					idsFromRoot = new ulong[counter + 1][];
					for (int i = 0; i < _idsFromRoot.Length; i++)
						idsFromRoot[i] = _idsFromRoot[i];
				}

                int levels = 0;
                for (GcObject pathObject = gcObject; pathObject != null; pathObject = pathObject.parent)
                {
                    if (pathObject.vertex != null)
                        levels++;
                }

                while (pathFromRoot.Length < levels + 1)
				{
					pathFromRoot = new Vertex[pathFromRoot.Length * 2];
				}

                int level = levels;
                for (GcObject pathObject = gcObject; pathObject != null; pathObject = pathObject.parent)
                {
                    if (pathObject.vertex != null)
                    {
                        level--;
                        pathFromRoot[level] = pathObject.vertex;
                        pathObject.vertex.id = 0;
                        objectToId.TryGetValue(pathObject, out pathObject.vertex.id);
                    }
                }

                levels = Vertex.SqueezeOutRepetitions(pathFromRoot, levels);
                idsFromRoot[counter] = new ulong[levels];
                for (int i = 0; i < levels; i++)
                {
                    idsFromRoot[counter][i] = pathFromRoot[i].id;
                }
                counter++;
            }
            Console.WriteLine("<TotalAllocations>{0}</TotalAllocations>", idsFromRoot.Length);
            Console.WriteLine("</Difference>");
            Console.WriteLine("</Summary>");
            Console.WriteLine("<PossibleCulPrits>");
            // Display the reference list and stack trace for 0th object

            // Find Culprit here.
            ArrayList mismatchedObjects = new ArrayList();
            ArrayList differentCulprits = new ArrayList();
            for (int j = 1; j < idsFromRoot.Length; j++)
            {
                for (int i = 0; i < idsFromRoot[0].Length; i++)
                {
                    if ((i > idsFromRoot[j].Length) || ((i < idsFromRoot[j].Length) && (idsFromRoot[0][i] != idsFromRoot[j][i])))
                    {
                        if (i < idsFromRoot[0].Length - 1)
                        {
                            mismatchedObjects.Add(j);
                        }
                        GcObject temp = idToObject[idsFromRoot[0][i - 1]];
                        if (temp != null)
                        {
                            if (!differentCulprits.Contains(temp.Type(this).name))
                            {
                                differentCulprits.Add(temp.Type(this).name);
                                if (differentCulprits.Count <= 5)
                                    System.Console.WriteLine("<CulPrit><!--{0}--></CulPrit>", temp.Type(this).name);
                            }
                        }
                        break;
                    }
                }
            }
            Console.WriteLine("</PossibleCulPrits>");
            Console.WriteLine("<FirstObject>");
            PrintGCRoot(idsFromRoot[0]);
            PrintStackTrace(idsFromRoot[0]);
            Console.WriteLine("</FirstObject>");
            Console.WriteLine("<MisMatchedObjects>");

            if (mismatchedObjects.Count > 0)
            {
                Console.WriteLine();
                int limit = (mismatchedObjects.Count > 5) ? 5 : mismatchedObjects.Count;
                for (int i = 0; i < limit; i++)
                {
                    Console.WriteLine("<Object>");
                    PrintGCRoot(idsFromRoot[(int)mismatchedObjects[i]]);
                    PrintStackTrace(idsFromRoot[(int)mismatchedObjects[i]]);
                    Console.WriteLine("</Object>");
                }
            }
            Console.WriteLine("</MisMatchedObjects>");
        }

        internal Graph BuildReferenceGraph(Graph orgGraph)
        {
            Graph graph = new Graph(this);
            graph.graphType = Graph.GraphType.ReferenceGraph;
            Vertex[] pathFromRoot = new Vertex[32];

            GcObject rootObject = CreateRootObject();
            FindVertex(0, rootObject, graph, BuildTypeGraphOptions.LumpBySignature);
            rootObject.InterestLevel = InterestLevel.Interesting;

            foreach (GcObject gcObject in idToObject.Values)
            {
                gcObject.parent = null;
            }

            // We wish to find all references to certain selected objects,
            // or, to be precise, all references that keep these objects alive.
            // To do this, we use a breadth first traversal of the object graph, using
            // a queue of objects still to process. If we find a reference to one of the
            // selected objects, we don't actually include this object, but instead
            // just make note of the reference

            // Initialize
            rootObject.parent = null;

            GcObject foundBeforeMarker = new GcObject();

            Queue<GcObject> queue = new Queue<GcObject>();
            queue.Enqueue(rootObject);

            // Loop
            while (queue.Count != 0)
            {
                GcObject head = queue.Dequeue();
                foreach (GcObject refObject in head.References)
                {
                    if (refObject.parent == null || refObject.parent == foundBeforeMarker)
                    {
                        // this is a reference to either one of the "selected" objects
                        // or just to a new object
                        if (   refObject.vertex != null && refObject.vertex.selected
                            && (refObject.InterestLevel & (InterestLevel.Interesting | InterestLevel.Display)) != InterestLevel.Ignore
                            && refObject.AllocTickIndex > orgGraph.allocatedAfterTickIndex
                            && refObject.AllocTickIndex < orgGraph.allocatedBeforeTickIndex)
                        {
                            // add <root> -> ... -> head -> refObject to the reference graph
                            int levels = 0;
                            for (GcObject pathObject = head; pathObject != null; pathObject = pathObject.parent)
                                levels++;

                            while (pathFromRoot.Length < levels + 2)
                            {
                                pathFromRoot = new Vertex[pathFromRoot.Length*2];
                            }

                            pathFromRoot[levels+1] = graph.FindOrCreateVertex(refObject.vertex.name, refObject.vertex.signature, refObject.vertex.moduleName);
                            int level = levels;
                            for (GcObject pathObject = head; pathObject != null; pathObject = pathObject.parent)
                            {
                                if (  (pathObject.InterestLevel & (InterestLevel.Interesting | InterestLevel.Display)) == InterestLevel.Ignore
                                    || pathObject.vertex == null)
                                    pathFromRoot[level] = null;
                                else
                                    pathFromRoot[level] = graph.FindOrCreateVertex(pathObject.vertex.name, pathObject.vertex.signature, pathObject.vertex.moduleName);
                                level--;
                            }

                            int nonZeroLevels = 0;
                            for (int j = 0; j <= levels+1; j++)
                            {
                                if (pathFromRoot[j] != null)
                                    pathFromRoot[nonZeroLevels++] = pathFromRoot[j];
                            }

                            levels = Vertex.SqueezeOutRepetitions(pathFromRoot, nonZeroLevels);

                            for (int j = 0; j < levels-1; j++)
                            {
                                Vertex fromVertex = pathFromRoot[j];
                                Vertex toVertex = pathFromRoot[j+1];
                                Edge edge = graph.FindOrCreateEdge(fromVertex, toVertex);
                                edge.AddWeight(1);
                            }

                            Vertex thisVertex = pathFromRoot[levels-1];
                            thisVertex.basicWeight += 1;
                            if (refObject.parent == null)
                            {
                                thisVertex.count += 1;
                                refObject.parent = foundBeforeMarker;
                            }
                        }
                        else
                        {
                            refObject.parent = head;
                            queue.Enqueue(refObject);
                        }
                    }
                }
            }

            foreach (Vertex v in graph.vertices.Values)
            {
                if (v.weight < v.outgoingWeight)
                    v.weight = v.outgoingWeight;
                if (v.weight < v.incomingWeight)
                    v.weight = v.incomingWeight;
                if (v.weightHistory == null)
                    v.weightHistory = new ulong[1];
            }

            foreach (Vertex v in graph.vertices.Values)
                v.active = true;
            graph.BottomVertex.active = false;

            return graph;
        }

        GcObject CreateRootObject()
        {
            GcObject rootObject = GcObject.CreateGcObject(rootCount);
            rootObject.TypeSizeStackTraceId = GetOrCreateGcType("<root>");
            GcObject unknownObject = GcObject.CreateGcObject(0);
            unknownObject.TypeSizeStackTraceId = GetOrCreateGcType("<unknown type>");
            for (int i = 0; i < rootCount; i++)
            {
                if (roots[i] == null)
                {
                    roots[i] = idToObject[rootIDs[i]];
                    if (roots[i] == null)
                        roots[i] = unknownObject;
                }
                roots[i].parent = null;
                roots[i].InterestLevel = InterestLevel.Interesting;
                rootObject.SetReference(i, roots[i]);
            }
            // patch up any unresolved forward references by making them point to "unknownObject"
            foreach (ForwardReference head in addressToForwardReferences.Values)
            {
                for (ForwardReference forwardReference = head; forwardReference != null; forwardReference = forwardReference.next)
                {
                    forwardReference.source.SetReference(forwardReference.referenceNumber, unknownObject);
                }
            }
            addressToForwardReferences.Clear();
            return rootObject;
        }
    }
}
