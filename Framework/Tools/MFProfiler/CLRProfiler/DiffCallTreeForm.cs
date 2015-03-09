using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

using System.IO;
using System.Text;
using System.Diagnostics;
using Microsoft.Win32;


namespace CLRProfiler
{
	
	/// <summary>
	/// Summary description for DiffCallTreeForm.
	/// </summary>
	internal class DiffCallTreeForm : System.Windows.Forms.Form, IComparer, IDiffTreeOwner
	{
		
		internal class SortingBehaviour
		{
			internal int sortingOrder;
			internal int counterId;
		}
		internal class ViewState
		{
			internal SortingBehaviour sort, highlight;
			internal bool showCalls, showAllocs, showAssemblies;

			internal ViewState(SortingBehaviour in_sort, SortingBehaviour in_highlight)
			{
				sort = in_sort;
				highlight = in_highlight;

				showCalls = showAllocs = showAssemblies = true;
			}
		};
		
		private AllocationDiff	_allocDiff= null;
		private ViewState viewState;
		private Font defaultFont;
		private TreeNodeBase Root;
		internal CLRProfiler.DiffTreeListView diffCallTreeView;
		private Rectangle formRect = new Rectangle( -1, -1, -1, -1 );
		

		private System.Windows.Forms.Panel controlCollection;
		
		/// <summary>
		/// Required designer variable.
		/// </summary>
		/// 
		private System.ComponentModel.Container components = null;
		
		public DiffCallTreeForm(TreeNodeBase root, AllocationDiff allocDiff)
		{
			this.Root = root;
			this._allocDiff = allocDiff;
			
			InitForm();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.controlCollection = new System.Windows.Forms.Panel();
			this.SuspendLayout();
			// 
			// controlCollection
			// 
			this.controlCollection.Dock = System.Windows.Forms.DockStyle.Fill;
			this.controlCollection.Location = new System.Drawing.Point(0, 0);
			this.controlCollection.Name = "controlCollection";
			this.controlCollection.Size = new System.Drawing.Size(632, 446);
			this.controlCollection.TabIndex = 0;
			// 
			// DiffCallTreeForm
			// 
			this.ClientSize = new System.Drawing.Size(632, 446);
			this.Controls.Add(this.controlCollection);
			this.Name = "DiffCallTreeForm";
			this.Text = "DiffCallTreeForm";
			this.Resize += new System.EventHandler(this.DiffCallTreeForm_Resize);
			this.Closing += new System.ComponentModel.CancelEventHandler(this.DiffCallTreeForm_Closing);
			this.ResumeLayout(false);

		}
		#endregion

		
		private void InitForm()
		{
			Controls.Clear();
			if (controlCollection != null)
			{
				controlCollection.Controls.Clear();
			}
			InitializeComponent();
			defaultFont = new Font(new FontFamily("Tahoma"), 8);
			
			DiffTreeListView treeView = new DiffTreeListView(this);
			treeView.Dock = DockStyle.Fill;
			treeView.Font = defaultFont;

			SortingBehaviour sort = new SortingBehaviour();
			SortingBehaviour highlight = new SortingBehaviour();
			sort.sortingOrder = highlight.sortingOrder = -1;
			sort.counterId = -1;
			highlight.counterId = 2;

			/* add columns */
			treeView.AddColumn(new ColumnInformation(-1, "Function name", ColumnInformation.ColumnTypes.Tree), 250);
			foreach(int counter in DiffStatistics.DefaultCounters)
			{
				AddColumn(treeView, counter);
			}

			treeView.ColumnClick += new EventHandler(SortOn);
			
			treeView.TokenObject = new ViewState(sort, highlight);
			treeView.Root = Root;
			
			
			treeView.Size = new System.Drawing.Size(332, 108);
			treeView.Visible = true;
			controlCollection.Controls.Add(treeView);
			SetcallTreeView();
			this.Visible = true;
		}
		

		public ArrayList FetchKids(object tokenObject, TreeNodeBase nodebase)
		{
			return nodebase.allkids;
		}
		private void SortOn(object obj, EventArgs e)
		{
			ColumnInformation ci = ((DiffColumn)obj).ColumnInformation;
			if(viewState.sort.counterId == (int)ci.Token)
			{
				viewState.sort.sortingOrder *= -1;
			}
			else
			{
				viewState.sort.counterId = (int)ci.Token;
				viewState.sort.sortingOrder = (viewState.sort.counterId == -1 ? -1 : 1);
			}
			diffCallTreeView.Resort();
		}
		private void SetcallTreeView()
		{
			foreach(Control c in controlCollection.Controls)
			{
				DiffTreeListView v = null;
				try
				{
					v = (DiffTreeListView)c;
				}
				catch
				{
					/* not interested in exceptions */
				}

				if(v != null)
				{
					diffCallTreeView = v;
					viewState = (ViewState)v.TokenObject;
					return;
				}
			}
			Debug.Fail("Cannot find tree view on the tab page");
		}
	
		
		public Color GetColor(object obj, TreeNodeBase root, bool positive)
		{
			DiffDataNode node = (DiffDataNode)root;
			int idx = (int)node.nodetype + (positive ? 0 : 3);
			return new Color[]
			{
				Color.Black,
				Color.Green,
				Color.BlueViolet,
				Color.White,
				Color.Yellow,
				Color.Beige
			}[idx];
		}

		
		/* returns font used to display the item (part of the ITreeOwner interface) */
		public Font GetFont(object obj, TreeNodeBase in_node)
		{
			DiffDataNode node = (DiffDataNode)in_node;
			FontStyle fs = FontStyle.Regular;
			if(node.data.firstTimeBroughtIn)
			{
				fs |= FontStyle.Italic;
			}
			if(node.highlighted)
			{
				fs |= FontStyle.Bold;
			}
			return (fs == FontStyle.Regular ? defaultFont : new Font(defaultFont, fs));

			
		}

		/* sort nodes at the branch level */
		public ArrayList ProcessNodes(object obj, ArrayList nodes)
		{
			bool add = false;
			ArrayList nodesAtOneLevel = new ArrayList();
			foreach(DiffDataNode node in nodes)
			{
				switch(node.nodetype)
				{
					case DiffDataNode.NodeType.Call:
						add = true;
						break;

					case DiffDataNode.NodeType.Allocation:
						add = viewState.showAllocs;
						break;

					case DiffDataNode.NodeType.AssemblyLoad:
						add = viewState.showAssemblies;
						break;
				}

				if(add)
				{
					nodesAtOneLevel.Add(node);
				}
			}
			if(nodesAtOneLevel.Count == 0)
			{
				return nodesAtOneLevel;
			}

			/* sort nodes first */
			nodesAtOneLevel.Sort(this);

			/* then choose the nodes to highlight */
			SortingBehaviour ss = viewState.sort;
			/* this is needed to use the default Compare method */
			viewState.sort = viewState.highlight;
			ArrayList nodesToHighlight = new ArrayList();
			DiffDataNode currentBest = (DiffDataNode)nodesAtOneLevel[0];

			currentBest.highlighted = false;
			nodesToHighlight.Add(currentBest);
			for(int i = 1; i < nodesAtOneLevel.Count; i++)
			{
				DiffDataNode n = (DiffDataNode)nodesAtOneLevel[i];
				n.highlighted = false;

				int res = Compare(currentBest, n) * viewState.highlight.sortingOrder;
				if(res == 0)
				{
					nodesToHighlight.Add(n);
				}
				else if(res > 0)
				{
					currentBest = n;
					nodesToHighlight.Clear();
					nodesToHighlight.Add(currentBest);
				}
			}
			viewState.sort = ss;

			foreach(DiffDataNode n in nodesToHighlight)
			{
				n.highlighted = true;
			}

			/* reverse order if required */
			if(viewState.sort.sortingOrder > 0)
			{
				nodesAtOneLevel.Reverse();
			}
			return nodesAtOneLevel;
		}

		/* implements IComparer that compares the nodes according to the current sorting order */
		public int Compare(object x, object y)
		{
			DiffDataNode a = (DiffDataNode)x;
			DiffDataNode b = (DiffDataNode)y;

			if(viewState.sort.counterId == -1)
			{
				return a.nodeId.CompareTo(b.nodeId);
			}

			IComparable aa = (IComparable)GetInfo(null, a, viewState.sort.counterId);
			IComparable bb = (IComparable)GetInfo(null, b, viewState.sort.counterId);
			try
			{
				return aa.CompareTo(bb);
			}
			catch
			{
				/* if string ("" is used instead of 0) is being compared against a number */
				bool aazero = (aa.ToString() == "");
				bool bbzero = (bb.ToString() == "");
				return aazero && bbzero ? 0 : aazero ? -1 : 1;
			}
		}
		
		/* returns data about the item for a given counter.
		 * object's ToString() is used to display that data */
		private object GetInfo(object obj, TreeNodeBase node, int counterId)
		{
			long number = 0;
			DiffDataNode root = (DiffDataNode)node;
			if(counterId < 0)
			{
				return root.name;
			}
			else
			{
				number = root.data.GetCounterValue(counterId);
			}
			/* use empty string to denote `0` */
			if(number == 0)
			{
				return "";
			}
			return number;
		}

		/* returns data about the item for a given counter.
		 * object's ToString() is used to display that data */
		public object GetInfo(object obj, TreeNodeBase node, ColumnInformation info)
		{
			return GetInfo(obj, node, info == null ? -1 : (int)info.Token);
		}

		#region GUI function
		private DiffColumn AddColumn(DiffTreeListView treeView, int counterId)
		{
			DiffColumn c = treeView.AddColumn(new ColumnInformation(counterId,
				DiffStatistics.GetCounterName(counterId),
				ColumnInformation.ColumnTypes.String),
				60);
			if(DiffStatistics.IsInclusive(counterId))
			{
				c.Font = new Font(c.Font, FontStyle.Bold);
			}
			return c;
		}
		#endregion

		private void DiffCallTreeForm_Resize(object sender, System.EventArgs e)
		{
			controlCollection.SuspendLayout();
			controlCollection.Left = 0;
			controlCollection.Top = 0;
			controlCollection.Width = this.ClientSize.Width;
			controlCollection.Height = this.ClientSize.Height - controlCollection.Top;
			controlCollection.ResumeLayout();
		}

				
		private void DiffCallTreeForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			_allocDiff.RefreshCallTreeNodes(_allocDiff.Root);
		}
	}


	internal class DiffStatistics
	{
		private DiffDataNode node;
		private readonly static string[] CounterNames =
		{
			"Prev (incl)",
			"curr (incl)",
			"diff (incl)",
			"prev (Calls)",
			"curr (Calls)",
			"diff (Calls)"
		};
		internal readonly static int[] DefaultCounters = {0, 1, 2, 3, 4, 5};

		internal static bool IsInclusive(int id)
		{
			return CounterNames[id].EndsWith("(incl)");
		}

		internal long GetCounterValue(int id)
		{
			switch(id)
			{
				case 0:  return node.prevIncl;
				case 1:  return node.currIncl;
				case 2:  return node.currIncl - node.prevIncl;
				case 3:  return node.prevCalls;
				case 4:  return node.currCalls;
				case 5:  return node.currCalls - node.prevCalls;
				default:
				{
					return -1;
				}
			}
		}

		internal static int GetNumberOfCounters()
		{
			return CounterNames.Length;
		}

		internal static string GetCounterName(int id)
		{
			return CounterNames[id];
		}

		internal bool firstTimeBroughtIn;
		
		internal DiffStatistics(DiffDataNode node)
		{
			this.node = node;
			
			firstTimeBroughtIn = false;
		}

		
	}
}
