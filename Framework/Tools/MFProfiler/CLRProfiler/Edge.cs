////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////namespace CLRProfiler
namespace CLRProfiler
{
    using System;
    using System.Drawing;

    /// <summary>
    ///    Summary description for Edge.
    /// </summary>
    internal class Edge : IComparable
    {
        Vertex fromVertex;
        Vertex toVertex;
        internal bool selected;
        internal Brush brush;
        internal Pen pen;
        internal Vertex ToVertex 
        {
            get
            {
                return toVertex;
            }
        }
        internal Vertex FromVertex 
        {
            get
            {
                return fromVertex;
            }
            set
            {
                fromVertex = value;
            }
        }
        internal ulong weight;
        internal int width;
        internal Point fromPoint, toPoint;
        public int CompareTo(Object o)
        {
            Edge e = (Edge)o;
            int diff = this.ToVertex.rectangle.Top - e.ToVertex.rectangle.Top;
            if (diff != 0)
                return diff;
            diff = this.FromVertex.rectangle.Top - e.FromVertex.rectangle.Top;
            if (diff != 0)
                return diff;
            if (e.weight < this.weight)
                return -1;
            else if (e.weight > this.weight)
                return 1;
            else
                return 0;
        }
        internal Edge(Vertex fromVertex, Vertex toVertex)
        {
            this.fromVertex = fromVertex;
            this.toVertex = toVertex;
            this.weight = 0;
        }

        internal void AddWeight(ulong weight)
        {
            this.weight += weight;
            this.fromVertex.outgoingWeight += weight;
            this.toVertex.incomingWeight += weight;
        }
    }
}
