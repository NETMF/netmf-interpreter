////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////namespace CLRProfiler
namespace CLRProfiler
{
    using System;
    using System.Drawing;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    ///    Summary description for Vertex.
    /// </summary>
    public class Vertex : IComparable
    {
        internal string name;
        internal string signature;
        internal string weightString;
        internal Dictionary<Vertex, Edge> incomingEdges;
        internal Dictionary<Vertex, Edge> outgoingEdges;
        internal int level;
        internal ulong weight;
        internal ulong incomingWeight;
        internal ulong outgoingWeight;
        internal Rectangle rectangle;
        internal Rectangle selectionRectangle;
        internal bool selected;
        internal bool visible;
        internal string nameSpace;
        internal string basicName;
        internal string basicSignature;
        internal bool signatureCurtated;
        internal bool active;
        private  Edge cachedOutgoingEdge;
        internal ulong[] weightHistory;
        private int hint;
        internal ulong basicWeight;
        internal string moduleName;
        internal int count;
        internal InterestLevel interestLevel;
        internal Graph containingGraph;
        internal ulong id;

        string nameSpaceOf(string name, out string basicName)
        {
            int prevSeparatorPos = -1;
            int thisSeparatorPos = -1;

            // We don't want to find the separators *inside* the angle brackets for
            // generics. So we stop before any angle brackets.
            // On the other hand, we need to handle "<root>"
            int endPos = name.IndexOf('<');
            if (endPos <= 0)
                endPos = name.Length;

            while (true)
            {
                int nextSeparatorPos = name.IndexOf('.', thisSeparatorPos+1, endPos - (thisSeparatorPos+1));
                if (nextSeparatorPos < 0 && thisSeparatorPos < name.Length-2)
                    nextSeparatorPos = name.IndexOf("::", thisSeparatorPos+1, endPos - (thisSeparatorPos+1));
                if (nextSeparatorPos < 0 || nextSeparatorPos >= name.Length-1)
                    break;
                prevSeparatorPos = thisSeparatorPos;
                thisSeparatorPos = nextSeparatorPos;
            }
            if (prevSeparatorPos >= 0)
            {
                basicName = name.Substring(prevSeparatorPos+1);
                return name.Substring(0, prevSeparatorPos+1);
            }
            else
            {
                basicName = name;
                return "";
            }
        }

        internal Vertex(string name, string signature, string module, Graph containingGraph)
        {
            this.signature = signature;
            this.incomingEdges = new Dictionary<Vertex, Edge>();
            this.outgoingEdges = new Dictionary<Vertex, Edge>();
            this.level = Int32.MaxValue;
            this.weight = 0;
            this.containingGraph = containingGraph;
            this.moduleName = module;

            nameSpace = nameSpaceOf(name, out basicName);
            this.name = name;
            basicSignature = signature;
            if (nameSpace.Length > 0 && signature != null)
            {
                int index;
                while ((index = basicSignature.IndexOf(nameSpace)) >= 0)
                {
                    basicSignature = basicSignature.Remove(index, nameSpace.Length);
                }
            }
        }

        internal Edge FindOrCreateOutgoingEdge(Vertex toVertex)
        {
            Edge edge = cachedOutgoingEdge;
            if (edge != null && edge.ToVertex == toVertex)
                return edge;
            if (!outgoingEdges.TryGetValue(toVertex, out edge))
            {
                edge = new Edge(this, toVertex);
                this.outgoingEdges[toVertex] = edge;
                toVertex.incomingEdges[this] = edge;
            }
            cachedOutgoingEdge = edge;

            return edge;
        }

        internal void AssignLevel(int level)
        {
            if (this.level > level)
            {
                this.level = level;
                foreach (Edge outgoingEdge in outgoingEdges.Values)
                {
                    outgoingEdge.ToVertex.AssignLevel(level + 1);
                }
            }
        }

        public int CompareTo(Object obj)
        {
            Vertex v = (Vertex)obj;
            if (v.weight < this.weight)
                return -1;
            else if (v.weight > this.weight)
                return 1;
            else
                return 0;
        }

        static bool IdenticalSequence(Vertex[] path, int i, int j, int length)
        {
            for (int k = 0; k < length; k++)
                if (path[i+k] != path[j+k])
                    return false;
            return true;
        }

        static int LargestRepetition(Vertex[] path, int i, int j, int length)
        {
            int len = i;
            if (len > length - j)
                len = length - j;
            for ( ; len > 0; len--)
            {
                int repLen = 0;
                while (j + repLen + len <= length && IdenticalSequence(path, i - len, j + repLen, len))
                    repLen += len;
                if (repLen > 0)
                    return repLen;
            }
            return 0;
        }

        static bool RepeatedElementsPresent(Vertex[] path, int length)
        {
            for (int i = 0; i < length; i++)
            {
                Vertex element = path[i];
                int hint = element.hint;
                if (hint < i && path[hint] == element)
                    return true;
                element.hint = i;
            }
            return false;
        }

        internal static string RemoveRecursionCount(string name)
        {
            int i = name.Length-1;
            if (i <= 0 || name[i] != ')')
                return name;
            i--;
            if (i <= 0 || !char.IsDigit(name[i]))
                return name;
            i--;
            while (i >= 0 && char.IsDigit(name[i]))
                i--;
            if (i <= 0 || name[i] != '(')
                return name;

            return name.Substring(0, i);
        }

        internal static int SqueezeOutRepetitions(Vertex[] path, int length)
        {
            if (!RepeatedElementsPresent(path, length))
                return length;

            int k = 0;
            int l = 0;
            while (l < length)
            {
                int repetitionLength = LargestRepetition(path, k, l, length);
                if (repetitionLength == 0)
                    path[k++] = path[l++];
                else
                    l += repetitionLength;
            }

            if (RepeatedElementsPresent(path, k))
            {
                int[] level = new int[k];
                for (int i = 0; i < k; i++)
                {
                    level[i] = 0;
                    Vertex element = path[i];
                    int hint = element.hint;
                    if (hint < i && path[hint] == element)
                        level[i] = level[hint] + 1;
                    element.hint = i;
                }

                for (int i = 0; i < k; i++)
                {
                    if (level[i] != 0)
                    {
                        Vertex v = path[i];
                        string newName = v.name + "(" + level[i] + ")";
                        path[i] = path[i].containingGraph.FindOrCreateVertex(newName, v.signature, v.moduleName);
                    }
                }
            }
            return k;
        }
    }
}
