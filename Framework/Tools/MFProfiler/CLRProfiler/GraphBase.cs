using System;
using System.Collections;
using System.Text;
using System.Diagnostics;
using System.Drawing;

namespace CLRProfiler
{
	/// <summary>
	/// Summary description for GraphBase.
	/// </summary>
	public class GraphBase
	{
		#region private data member
		private bool placeVertices = true;
		private bool placeEdges = true;
        private ulong totalWeight;
		private int totalHeight = 100;
		private float scale = 1.0f;
		private Graph graph = null;
		#endregion
		#region public data member
		public ArrayList levelList;
		#endregion
		public GraphBase()
		{
		
		}
		#region public methods
		internal void GetAllocationGraph(ReadLogResult readLogResult)
		{
			graph = readLogResult.allocatedHistogram.BuildAllocationGraph(new FilterForm());
			PlaceVertices();
		}
		
		public int SelectedVertexCount(out Vertex selectedVertex)
		{
			int selectedCount = 0;
			selectedVertex = null;
			foreach (Vertex v in graph.vertices.Values)
			{
				if (v.selected)
				{
					selectedCount++;
					selectedVertex = v;
				}
			}
			return selectedCount;
		}
		#endregion
		#region private methods
		private ArrayList BuildLevels(Graph g)
		{
			ArrayList al = new ArrayList();
			for (int level = 0; level <= g.BottomVertex.level; level++)
			{
				al.Add(new ArrayList());
			}
			foreach (Vertex v in g.vertices.Values)
			{
				if (v.level <= g.BottomVertex.level)
				{
					ArrayList all = (ArrayList)al[v.level];
					all.Add(v);
				}
				else
				{
					Debug.Assert(v.level == int.MaxValue);
				}
			}
			foreach (ArrayList all in al)
			{
				all.Sort();
			}
			return al;
		}

        internal string formatWeight(ulong weight)
		{
			if (graph.graphType == Graph.GraphType.CallGraph)
			{
				if (weight == 1)
					return "1 call";
				else
					return string.Format("{0} calls", weight);
			}
			if(graph.graphType == Graph.GraphType.AssemblyGraph)
			{
				if(weight == 1)
				{
					return "1 assembly";
				}
				else
				{
					return weight + " assemblies";
				}
			}
			else
			{
				double w = weight;
				string byteString = "bytes";
				if (w >= 1024)
				{
					w /= 1024;
					byteString = "kB   ";
				}
				if (w >= 1024)
				{
					w /= 1024;
					byteString = "MB   ";
				}
				if (w >= 1024)
				{
					w /= 1024;
					byteString = "GB   ";
				}
				string format = "{0,4:f0} {1} ({2:f2}%)";
				if (w < 10)
					format = "{0,4:f1} {1} ({2:f2}%)";
				return string.Format(format, w, byteString, weight*100.0/totalWeight);
			}
		}

		private void PlaceEdges(float scale)
		{
			foreach (Vertex v in graph.vertices.Values)
			{
				PlaceEdges(v.incomingEdges.Values, true, scale);
				PlaceEdges(v.outgoingEdges.Values, false, scale);
			}
		}
		private void PlaceEdges(ICollection edgeCollection, bool isIncoming, float scale)
		{
			ArrayList edgeList = new ArrayList(edgeCollection);
			edgeList.Sort();
			foreach (Edge e in edgeList)
			{
				float fwidth = e.weight*scale;
			}
		}

		private void PlaceVertices()
		{
			graph.AssignLevelsToVertices();
			totalWeight = 0;
			foreach (Vertex v in graph.vertices.Values)
			{
				v.weight = v.incomingWeight;
				if (v.weight < v.outgoingWeight)
					v.weight = v.outgoingWeight;
				if (graph.graphType == Graph.GraphType.CallGraph)
				{
					if (totalWeight < v.weight)
						totalWeight = v.weight;
				}
			}
			if (graph.graphType != Graph.GraphType.CallGraph)
				totalWeight = graph.TopVertex.weight;
			if (totalWeight == 0)
			{
				totalWeight = 1;
			}

			ArrayList al = levelList = BuildLevels(graph);
			scale = (float)totalHeight/totalWeight;
			if (placeVertices)
			{
				for (int level = graph.TopVertex.level;
					level <= graph.BottomVertex.level;
					level++)
				{
					ArrayList all = (ArrayList)al[level];
					foreach (Vertex v in all)
					{
						if (graph.graphType == Graph.GraphType.CallGraph)
						{
							v.basicWeight = v.incomingWeight - v.outgoingWeight;
							if (v.basicWeight < 0)
								v.basicWeight = 0;
							v.weightString = string.Format("Gets {0}, causes {1}",
								formatWeight(v.basicWeight),
								formatWeight(v.outgoingWeight));
						}
						else
						{
							if (v.count == 0)
								v.weightString = formatWeight(v.weight);
							else if (v.count == 1)
								v.weightString = string.Format("{0}  (1 object, {1})", formatWeight(v.weight), formatWeight(v.basicWeight));
							else
								v.weightString = string.Format("{0}  ({1} objects, {2})", formatWeight(v.weight), v.count, formatWeight(v.basicWeight));
						}
						
					}
					int y = 10;
                    ulong levelWeight = 0;
					foreach (Vertex v in all)
						levelWeight += v.weight;
					float levelHeight = levelWeight*scale;
					if (levelHeight < totalHeight*0.5)
						y+= (int)((totalHeight - levelHeight)*2);
					foreach (Vertex v in all)
					{
						// For the in-between vertices, sometimes it's good
						// to shift them down a little to line them up with
						// whatever is going into them. Unless of course
						// we would need to shift too much...
						if (v.level < graph.BottomVertex.level-1)
						{
                            ulong highestWeight = 0;
							foreach (Edge e in v.incomingEdges.Values)
							{
								if (e.weight > highestWeight && e.FromVertex.level < level)
								{
									highestWeight = e.weight;
								}
							}
							
						}
						float fHeight = v.weight*scale;
						int iHeight = (int)fHeight;
						if (iHeight < 1)
							iHeight = 1;
						if (placeEdges)
							PlaceEdges(v.outgoingEdges.Values, false, scale);
					}
				}
			}
			if (placeEdges)
				PlaceEdges(scale);
		}
		#endregion
		internal Graph basegraph
		{
			get { return graph;}
		}
	}
}
