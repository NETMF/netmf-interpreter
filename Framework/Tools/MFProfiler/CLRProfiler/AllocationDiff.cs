using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Data;



namespace CLRProfiler
{
	/// <summary>
	/// Code organize:
	///		AllocationDiff reads two log files one from previous build and one from current build. 
	///			see region - get base data methods
	///		also it builds three tables,
	///			see region - build Base table
	///			see region - build Caller and callee table
	///			see region - build Type Allocation table
	///			
	///	internal data structure:
	///		prevLog and currLog object hold base log data from log files
	///		prevG and currG object hold base Graph data, the main purpose was keep the call relations
	///		4 hash table (prevbasedata, currbasedata, prevtypeAllocdata, currtypeAllocdata) holds all useful data from base log/graph
	///		been used for build all diff, call relations, and type allocation tables. 
	///		
	///	Table Definitions:
	///		basedatatable contains all basic data: inclusive, exclusive, diff, childinclusive, childexclusive, timesBeenCalled, timesmakescalls.
	///		caller table contains function Id and its caller Ids 
	///		callee table contains function Id and its callee Ids
	///		typeAlloction table contains type exclusive and diff info
	///	
	///	Detail Definitions:
	///		Memory allocation report can show 9 different details based on allocated memory size
	///		detail0 - all size
	///		detail01 = max(prevIncl) / 8
	///		detail02 = max(prevIncl) /7
	///		...
	///		
	/// </summary>
	
	struct datanode
	{
		int _id;
		int _level;
#if(V_EXEC)
			double _incl;
			double _excl;
			

#else
		int _incl;
		int _excl;
		
#endif
			
		int _timesBeenCalled;
		int _timesMakeCalls;
		int _category;
		string _name;
		Dictionary<Vertex, Edge> _caller;
        Dictionary<Vertex, Edge> _callee;
		Hashtable _callerAlloc;
		Hashtable _calleeAlloc;

		public int level
		{
			get {return _level;}
			set {_level = value;}
		}
		public int id
		{
			get {return _id;}
			set {_id = value;}
		}
		public string name
		{
			get{return _name;}
			set{_name = value;}
		}
#if(V_EXEC)
			public double incl
			{
				get{return _incl;}
				set{_incl = value;}
			}
			public double excl
			{
				get{return _excl;}
				set{_excl = value;}
			}
			
#else		
		public int incl
		{
			get{return _incl;}
			set{_incl = value;}
		}
		public int excl
		{
			get{return _excl;}
			set{_excl = value;}
		}
		
#endif
		public int timesBeenCalled
		{
			get{return _timesBeenCalled;}
			set{_timesBeenCalled = value;}
		}
		public int timesMakeCalls
		{
			get{return _timesMakeCalls;}
			set{_timesMakeCalls = value;}
		}
		public int category
		{
			get{return _category;}
			set{_category = value;}
		}
        public Dictionary<Vertex, Edge> caller
		{
			get { return _caller;}
			set { _caller = value;}
		}
        public Dictionary<Vertex, Edge> callee
		{
			get { return _callee;}
			set { _callee = value;}
		}
		public Hashtable callerAlloc
		{
			get { return _callerAlloc;}
			set { _callerAlloc = value;}
		}
		public Hashtable calleeAlloc
		{
			get { return _calleeAlloc;}
			set { _calleeAlloc = value;}
		}

	}
	public class AllocationDiff 
	{
		class CompareIncl : IComparer
		{
			Hashtable inclOfNode;

			internal CompareIncl(Hashtable inclOfNode)
			{
				this.inclOfNode = inclOfNode;
			}

			int IComparer.Compare(object x, object y)
			{
				long inclX = (long)inclOfNode[x];
				long inclY = (long)inclOfNode[y];
				if (inclX < inclY)
					return 1;
				else if (inclX > inclY)
					return -1;
				else
					return 0;
			}
		}

		#region data member
		// log file names
		private string _prevFile = null;
		private string _currFile = null;
		private string _diffFile = null;
	
		// holds all useful data from base
		// for build all diff, call relations, and type allocation tables
		internal Hashtable _prevbasedata = null;
		internal Hashtable _currbasedata = null;


		// maps for match search
		public Hashtable basedataId =null;
		public Hashtable Idbasedata =null;
		public Hashtable typeAllocdataId = null;

		// hold log data		
		private LogBase _prevLog = null;
		private LogBase _currLog = null;
		// hold base graph related data
		private GraphBase _prevG = null;
		private GraphBase _currG = null;

		// hold base stacktrace info
		internal CallTreeForm _prevcallTrace = null;
		internal CallTreeForm _currcallTrace = null;
		internal DiffDataNode Root = null;
		internal DataTable diffTracetbl = null;
		private static int nodeidx = 0;
				
		internal Hashtable prevFuncExcl = new Hashtable();
		internal Hashtable currFuncExcl = new Hashtable();
		internal Hashtable prevTypeExcl = new Hashtable();
		internal Hashtable currTypeExcl = new Hashtable();


		StreamReader r;
		byte[] buffer;
		int c;
		int line;
		long pos;
		long lastLineStartPos;
		int bufPos;
		int bufLevel;
		private static int sumnodeidx = 0;
		private static int depth = 0;

		internal DataTable summaryTracetbl = null;
		internal Hashtable diffCallTreeNodes = new Hashtable();
				
		private const int idx_parentid = 0;
	//	private const int idx_parentname = 1;
		private const int idx_id = 2;
		private const int idx_name = 3;
		private const int idx_mapname = 4;
		private const int idx_prevIncl = 5;
		private const int idx_currIncl = 6;
		private const int idx_diffIncl = 7;
		
		private const int idx_prevCalls = 8;
		private const int idx_currCalls = 9;
		private const int idx_diffCalls = 10;
		
		private const int idx_prevFunid = 11;
		private const int idx_currFunid = 12;
		private const int idx_type = 13;
		private const int idx_depth = 14;
		

		// table details filter value
#if(V_EXEC)
		private const double detail01D = 8.0;
#else
		private const int detail01D = 8;
#endif
		private ulong maxIncl = 0;
		private ulong typemaxIncl = 0;
		public DetailFilter prevFilter = new DetailFilter();
		public DetailFilter currFilter =new DetailFilter();
		public DetailFilter prevTypedeFilter = new DetailFilter();
		public DetailFilter currTypedeFilter = new DetailFilter();
		
		// dataset and tables
		private DataSet _ds = null;
		private DataTable _callertbl = null;
		private DataTable _calleetbl = null;
		private DataTable _basedatatable = null;
		private DataTable _ContriTocallertbl = null;
		private DataTable _ContriTocalleetbl = null;
		
		#endregion
		
		#region struct data methods
		
		// caller and callee tables node
		struct callnode
		{
			int _id;
			int _callerid;
			int _calleeid;
			public int id
			{
				get {return _id;}
				set { _id = value;}
			}
			
			public int callerid
			{
				get{return _callerid;}
				set{_callerid = value;}
			}
			public int calleeid
			{
				get{return _calleeid;}
				set{_calleeid = value;}
			}
			
		}
		// typeAllocation table node
		struct typeAllocnode
		{
			int _typeid;
			int _funcid;
#if(V_EXEC)
			double _allocmem;
#else
			int _allocmem;
#endif
			public int typeid
			{
				get {return _typeid;}
				set { _typeid = value;}
			}
			
			public int funcid
			{
				get{return _funcid;}
				set{_funcid= value;}
			}
#if(V_EXEC)
			public double allocmem
			{
				get{return _allocmem;}
				set{_allocmem = value;}
			}
#else
			public int allocmem
			{
				get{return _allocmem;}
				set{_allocmem = value;}
			}
#endif
			
		}
		public DataTable basedatatable
		{
			get {return _basedatatable;}
			set { _basedatatable = value;}
		}
		public DataTable ContriTocallertbl
		{
			get {return _ContriTocallertbl;}
			set { _ContriTocallertbl = value;}
		}
		public DataTable ContriTocalleetbl
		{
			get {return _ContriTocalleetbl;}
			set { _ContriTocalleetbl = value;}
		}

		// detailds for reportform details RadioButton
		public struct DetailFilter
		{
			ulong _max;
#if(V_EXEC)
			double _detail01;
			double _detail02;
			double _detail05;
			double _detail1;
			double _detail2;
			double _detail5;
			double _detail10;
			public double detail01
			{
				get{return _detail01;}
				set{_detail01 = value;}
			}
			public double detail02
			{
				get{return _detail02;}
				set{_detail02 = value;}
			}
			public double detail05
			{
				get{return _detail05;}
				set{_detail05 = value;}
			}
			public double detail1
			{
				get{return _detail1;}
				set{_detail1 = value;}
			}
			public double detail2
			{
				get{return _detail2;}
				set{_detail2 = value;}
			}
			public double detail5
			{
				get{return _detail5;}
				set{_detail5 = value;}
			}
			public double detail10
			{
				get{return _detail10;}
				set{_detail10 = value;}
			}
#else
            ulong _detail01;
            ulong _detail02;
            ulong _detail05;
            ulong _detail1;
            ulong _detail2;
            ulong _detail5;
            ulong _detail10;
            internal ulong detail01
			{
				get{return _detail01;}
				set{_detail01 = value;}
			}
            internal ulong detail02
			{
				get{return _detail02;}
				set{_detail02 = value;}
			}
            internal ulong detail05
			{
				get{return _detail05;}
				set{_detail05 = value;}
			}
            internal ulong detail1
			{
				get{return _detail1;}
				set{_detail1 = value;}
			}
            internal ulong detail2
			{
				get{return _detail2;}
				set{_detail2 = value;}
			}
            internal ulong detail5
			{
				get{return _detail5;}
				set{_detail5 = value;}
			}
            internal ulong detail10
			{
				get{return _detail10;}
				set{_detail10 = value;}
			}
#endif
			internal ulong max
			{
				get{return _max;}
				set{_max = value;}
			}
		}
		
		#endregion

		#region constructor
		public AllocationDiff()
		{
			_prevLog = new LogBase();
			_currLog = new LogBase();
			_prevG = new GraphBase();
			_currG = new GraphBase();

			ds = new DataSet();
			_prevbasedata = new Hashtable();
			_currbasedata = new Hashtable();
			

			basedataId = new Hashtable();
			Idbasedata = new Hashtable();
			basedatatable = new DataTable("basedatatbl");
			callertbl = new DataTable("caller");
			calleetbl = new DataTable("callee");
			typeAllocdataId = new Hashtable();
			ContriTocallertbl = new DataTable("ContriTocallertbl");
			ContriTocalleetbl = new DataTable("ContriTocalleetbl");
			MakeBaseDataTable(basedatatable);
			MakeCallTables(callertbl, true);
			MakeCallTables(calleetbl, false);
			MakeBaseDataTable(ContriTocallertbl);
			MakeBaseDataTable(ContriTocalleetbl);
			prevFilter = new DetailFilter();
			currFilter = new DetailFilter();
			prevTypedeFilter = new DetailFilter();
			currTypedeFilter = new DetailFilter();
		}
		#endregion

		#region public property methods
		// DataSet used to collect tables and 
		// build relations between table in the near future
		// also it usded by DataViewManager in ReportForm
		public DataSet ds
		{
			get{ return _ds;}
			set{ _ds = value;}
		}
		
		public DataTable callertbl
		{
			get {return _callertbl;}
			set { _callertbl = value;}
		}
		
		public DataTable calleetbl
		{
			get {return _calleetbl;}
			set { _calleetbl = value;}
		}

		public string PrevLogFileName
		{
			get {return _prevFile;}
			set
			{
				_prevFile = value;
				_prevLog.LogFileName = value;
            }
		}

		public string CurrLogFileName
		{
			get {return _currFile;}
			set
			{
				_currFile = value;
				_currLog.LogFileName = value;
			}
		}

		public string diffLogFileName
		{
			get {return _diffFile;}
			set
			{
				_diffFile = value;
			}
		}
		#endregion

		#region public methods
		public void BuildAllocationDiffTable()
		{
			try
			{
				GetLogData();
		
				BuildBaseData(_prevG, _prevcallTrace, _prevbasedata, this.prevFuncExcl, this.prevTypeExcl);
				BuildBaseData(_currG, _currcallTrace, _currbasedata, this.currFuncExcl, this.currTypeExcl);
				BuildBaseDataTable();
				BuildBaseCallTables();
				getDetailFilter(ref prevFilter);
				getDetailFilter(ref currFilter);
				getDetailFilter(ref prevTypedeFilter);
				getDetailFilter(ref currTypedeFilter);
								
				BuildContributionCalleeTable();
				BuildContributionCallerTable();
			}
			catch(Exception e)
			{
				throw new Exception(e.Message + "\n");
			}
		}
		public bool IsAllocType(string name)
		{
			return (_prevLog.logResult.allocatedHistogram.readNewLog.typeSignatureIdHash.ContainsKey(name) ||
				_currLog.logResult.allocatedHistogram.readNewLog.typeSignatureIdHash.ContainsKey(name));
		}
		#endregion

		#region GetLogData
		private void GetLogData()
		{
			// get base data from log files
			try
			{
				_prevLog.readLogFile();
			}
			catch
			{
				throw new Exception("Bad log file: " + _prevLog.LogFileName + "\n");
			}
			try
			{
				_currLog.readLogFile();
			}
			catch
			{
				throw new Exception("Bad log file: " + _currLog.LogFileName +  "\n");
			}

			// get mixed data structure graph
			try
			{
				_prevG.GetAllocationGraph(_prevLog.logResult);
			}
			catch
			{
				throw new Exception("Bad data structure in log file: " + _prevLog.LogFileName +  "\n");
			}
			try
			{
				_currG.GetAllocationGraph(_currLog.logResult);
			}
			catch
			{
				throw new Exception("Bad data structure in log file: " + _currLog.LogFileName +  "\n");
			}

			// get more detailed allocation info from stack trace 
			try
			{
				
				_prevcallTrace = new CallTreeForm(_prevLog.LogFileName, _prevLog.logResult, true);
				ReadFile(_prevcallTrace, _prevLog.LogFileName, this.prevFuncExcl, this.prevTypeExcl);
			}
			catch
			{
				throw new Exception("Bad stacktrace content in log file: " + _prevLog.logResult +  "\n");
			}
			try
			{
				_currcallTrace = new CallTreeForm(_currLog.LogFileName, _currLog.logResult, true);
				ReadFile(_currcallTrace, _currLog.LogFileName, this.currFuncExcl, this.currTypeExcl);
			}
			catch
			{
				throw new Exception("Bad stacktrace content in log file: " + _currLog.logResult +  "\n");
			}
			nodeidx = 0;
			diffTracetbl = new DataTable("diffTrace");
			summaryTracetbl = new DataTable("summaryTracetbl");
			MakeDiffTreceTable(diffTracetbl);
			MakeDiffTreceTable(summaryTracetbl);
		

			string rname = _currcallTrace.MakeName((TreeNode)_currcallTrace.callTreeView.Root);
			Root = new DiffDataNode(rname);
			Root.prevIncl = ((TreeNode)_prevcallTrace.callTreeView.Root).data.bytesAllocated;
			Root.currIncl = ((TreeNode)_currcallTrace.callTreeView.Root).data.bytesAllocated;
			Root.diffIncl = Root.currIncl - Root.prevIncl;
			Root.prevCalls = ((TreeNode)_prevcallTrace.callTreeView.Root).data.numberOfFunctionsCalled;
			Root.currCalls = ((TreeNode)_currcallTrace.callTreeView.Root).data.numberOfFunctionsCalled;
			Root.diffCalls = Root.currCalls - Root.prevCalls;
			

			Root.nodeId = nodeidx++;
			Root.parentId = -1;
			Root.prevFunId = 0;
			Root.currFunId = 0;
			AddDiffTraceTableRow(diffTracetbl, Root);

			
			BuildDiffTraceTable(Root, (TreeNode)_currcallTrace.callTreeView.Root, (TreeNode)_prevcallTrace.callTreeView.Root);
			this.ds.Tables.Add(diffTracetbl);
			sumnodeidx = 0;
			depth = -1;
			BuildSummaryTable(Root, -1, "parentid = -1");
			Root = (DiffDataNode)Root.allkids[0];
			this.ds.Tables.Add(summaryTracetbl);

		

		}
		#endregion

		#region build Base table method
		private void BuildBaseData(GraphBase gb, CallTreeForm tmpcallTree,  Hashtable htbl, Hashtable FuncExcl, Hashtable TypeExcl)
		{
			Vertex selectedVertex;
			int selectedVertexCount = gb.SelectedVertexCount(out selectedVertex);
			int id = 1;
			
			string nameAndSignature = null;
			datanode n = new datanode();


			try
			{
				foreach (Vertex v in gb.basegraph.vertices.Values)
				{
					if( !v.name.StartsWith("????"))
					{
						if (v.selected || (selectedVertexCount == 0) )
						{
							nameAndSignature  = v.name;
							if(v.signature != null)
								nameAndSignature += ' ' + v.signature;
							n.name = nameAndSignature;
							n.incl = FormatSize((int)v.weight);
							n.caller = v.incomingEdges;
							n.callee = v.outgoingEdges;
							n.level = v.level;
							n.excl = 0;
							n.timesBeenCalled= n.timesMakeCalls = 0;
							FillCallAlloc(ref n, v);
							if(tmpcallTree.LogResult.allocatedHistogram.readNewLog.funcSignatureIdHash.ContainsKey(nameAndSignature))
							{
								n.category = 1;	// func
								id = tmpcallTree.LogResult.callstackHistogram.readNewLog.funcSignatureIdHash[nameAndSignature];
								if(FuncExcl.ContainsKey(nameAndSignature))
								{
									n.excl =  FormatSize((int)FuncExcl[nameAndSignature]);
								}
								if( id > 0 && id <tmpcallTree.CallStats.Length)
								{

									n.timesBeenCalled = (int)tmpcallTree.CallStats[id].timesCalled;
									n.timesMakeCalls = (int)tmpcallTree.CallStats[id].totalFunctionsCalled;
								}
								if( !htbl.ContainsKey(nameAndSignature))
								{
									htbl.Add(nameAndSignature, n);
								}
							}
							else if(tmpcallTree.LogResult.allocatedHistogram.readNewLog.typeSignatureIdHash.ContainsKey(nameAndSignature))
							{
								n.category = 2;	// type
								id = tmpcallTree.LogResult.allocatedHistogram.readNewLog.typeSignatureIdHash[nameAndSignature];
								if(TypeExcl.ContainsKey(nameAndSignature))
								{
									n.excl =  FormatSize((int)TypeExcl[nameAndSignature]);
								}
								if( id > 0 && id <tmpcallTree.AllocStats.Length)
								{
									n.timesBeenCalled = (int)tmpcallTree.AllocStats[id].timesAllocated;
								}
								if( !htbl.ContainsKey(nameAndSignature))
								{
									typemaxIncl = (typemaxIncl > v.weight) ? typemaxIncl : v.weight;
									htbl.Add(nameAndSignature, n);
								}
							}
							else
							{
								if( !htbl.ContainsKey(nameAndSignature))
								{
									maxIncl = v.weight;
									htbl.Add(nameAndSignature, n);
								}
							}

						}
			
					}
				}
			}
			catch
			{
				throw new Exception("Faild on build base data structure \n");
			}
			// max for caculate function/type 9 details 
			if( prevFilter.max == 0)
			{
				prevFilter.max = maxIncl;
				prevTypedeFilter.max = typemaxIncl;
			}
			else
			{
				currFilter.max = maxIncl;
				currTypedeFilter.max = typemaxIncl;
			}
			maxIncl = 0;
			typemaxIncl = 0;
		}
		private void FillCallAlloc(ref datanode n, Vertex v)
		{
			n.calleeAlloc = new Hashtable();
			n.callerAlloc = new Hashtable();
			foreach(Edge edge in v.outgoingEdges.Values)
			{
				string nameAndSignature = edge.ToVertex.name;
				if(edge.ToVertex.signature != null)
					nameAndSignature += ' ' + edge.ToVertex.signature;
				if(!n.calleeAlloc.ContainsKey(nameAndSignature))
				{
					n.calleeAlloc.Add(nameAndSignature, FormatSize((int)edge.weight));
				}
			}
			foreach(Edge edge in v.incomingEdges.Values)
			{
				string nameAndSignature = edge.FromVertex.name;
				if(edge.FromVertex.signature != null)
					nameAndSignature += ' ' + edge.FromVertex.signature;
				if(!n.callerAlloc.ContainsKey(nameAndSignature))
				{
					n.callerAlloc.Add(nameAndSignature, FormatSize((int)edge.weight));
				}
			}
		}

		private void BuildBaseDataTable()
		{
			 int id = 0;
			try
			{

				foreach(string nameAndSignature in _prevbasedata.Keys)
				{
				
					datanode pn= new datanode();
					datanode cn= new datanode();
					pn = (datanode)_prevbasedata[nameAndSignature];
					pn.id = id;
					if(_currbasedata.ContainsKey(nameAndSignature))
					{
						cn = (datanode)_currbasedata[nameAndSignature];
					}
					AddBaseTableRow(this.basedatatable, nameAndSignature, pn, cn);
					basedataId.Add(nameAndSignature, id);
					Idbasedata.Add(id, nameAndSignature);
					id++;
				}
				foreach( string CnameAndSignature in _currbasedata.Keys)
				{
					if(! _prevbasedata.ContainsKey(CnameAndSignature))
					{
						datanode pn= new datanode();
						datanode cn= new datanode();
						cn = (datanode)_currbasedata[CnameAndSignature];
						cn.id = id;
						AddBaseTableRow(this.basedatatable, CnameAndSignature, pn, cn);
						basedataId.Add(CnameAndSignature, id);
						Idbasedata.Add(id, CnameAndSignature);
						id++;
					}
				}
				ds.Tables.Add(this.basedatatable);
			}
			catch
			{
				throw new Exception("Faild on build base data Tables \n");
			}
		}
		private void AddBaseTableRow(DataTable tmptbl, string name, datanode pn,  datanode cn )
		{
			DataRow tmpRow = tmptbl.NewRow();
			if(pn.name != null)
				tmpRow["id"] = pn.id;
			else 
			{
				tmpRow["id"] = cn.id;
			}

			tmpRow["name"] = name;
			tmpRow["prevIncl"] = pn.incl;
			tmpRow["currIncl"] = cn.incl;
			tmpRow["prevExcl"] = pn.excl;
			tmpRow["currExcl"] = cn.excl;
#if(V_EXEC)
			//tmpRow["diffIncl"] = Convert.ToDouble(string.Format("{0:f2}", (cn.incl - pn.incl)));
			tmpRow["diffIncl"] = Math.Round((cn.incl - pn.incl), 2);
			tmpRow["diffExcl"] =  Math.Round((cn.excl - pn.excl), 2);
			tmpRow["prevChildIncl"] =  Math.Round((pn.incl - pn.excl), 2);
			tmpRow["currChildIncl"] =  Math.Round((cn.incl - cn.excl), 2);
			tmpRow["diffChildIncl"] =  Math.Round(((cn.incl - cn.excl) - (pn.incl - pn.excl)), 2);
#else
			tmpRow["diffIncl"] = cn.incl - pn.incl;
			tmpRow["diffExcl"] = cn.excl - pn.excl;
			tmpRow["prevChildIncl"] = pn.incl - pn.excl;
			tmpRow["currChildIncl"] = cn.incl - cn.excl;
			tmpRow["diffChildIncl"] = (cn.incl - cn.excl) - (pn.incl - pn.excl);
			
#endif
			tmpRow["prevTimesCalled"] = pn.timesBeenCalled;
			tmpRow["currTimesCalled"] = cn.timesBeenCalled;
			tmpRow["prevTimesMakecalls"] = pn.timesMakeCalls;
			tmpRow["currTimesMakecalls"] = cn.timesMakeCalls;
			tmpRow["diffTimesCalled"] = cn.timesBeenCalled - pn.timesBeenCalled;
			tmpRow["diffTimesMakecalls"] = cn.timesMakeCalls - pn.timesMakeCalls;
			
			tmptbl.Rows.Add(tmpRow);
		}
		public void MakeBaseDataTable(DataTable tbl)
		{
			addTableRow(tbl, "System.Int32", "id");
			addTableRow(tbl, "System.String", "name");
#if(V_EXEC)
			addTableRow(tbl, "System.Double", "prevIncl");
			addTableRow(tbl, "System.Double", "currIncl");
			addTableRow(tbl, "System.Double", "diffIncl");
			addTableRow(tbl, "System.Double", "prevExcl");
			addTableRow(tbl, "System.Double", "currExcl");
			addTableRow(tbl, "System.Double", "diffExcl");
			addTableRow(tbl, "System.Double", "prevChildIncl");
			addTableRow(tbl, "System.Double", "currChildIncl");
			addTableRow(tbl, "System.Double", "diffChildIncl");
#else
			addTableRow(tbl, "System.Int32", "prevIncl");
			addTableRow(tbl, "System.Int32", "currIncl");
			addTableRow(tbl, "System.Int32", "diffIncl");
			addTableRow(tbl, "System.Int32", "prevExcl");
			addTableRow(tbl, "System.Int32", "currExcl");
			addTableRow(tbl, "System.Int32", "diffExcl");
			addTableRow(tbl, "System.Int32", "prevChildIncl");
			addTableRow(tbl, "System.Int32", "currChildIncl");
			addTableRow(tbl, "System.Int32", "diffChildIncl");
#endif
			addTableRow(tbl, "System.Int32", "prevTimesCalled");
			addTableRow(tbl, "System.Int32", "currTimesCalled");
			addTableRow(tbl, "System.Int32", "diffTimesCalled");
			addTableRow(tbl, "System.Int32", "prevTimesMakecalls");
			addTableRow(tbl, "System.Int32", "currTimesMakecalls");
			addTableRow(tbl, "System.Int32", "diffTimesMakecalls");

			addTableRow(tbl, "System.Int32", "prevlevel");
			addTableRow(tbl, "System.Int32", "currlevel");
			addTableRow(tbl, "System.Int32", "prevcat");
			addTableRow(tbl, "System.Int32", "currcat");
						
		}
		
		#endregion


		#region build Caller and callee table
		private void BuildBaseCallTables()
		{
			int id = -1;
			string nameAndSignature = null;
			try
			{
				foreach(DictionaryEntry de in basedataId)
				{
					datanode pn= new datanode();
					datanode cn= new datanode();
					nameAndSignature = (string)de.Key;
					id = (int)basedataId[nameAndSignature];
					if(this._prevbasedata.ContainsKey(nameAndSignature))
					{
						pn = (datanode)_prevbasedata[nameAndSignature];
						pn.id = id;
						BuildCallTables(this.callertbl, id, pn.caller, true);
						BuildCallTables(this.calleetbl, id, pn.callee, false);
					}
					else
					{
						if(_currbasedata.ContainsKey(nameAndSignature))
						{
							cn = (datanode)_currbasedata[nameAndSignature];
							cn.id = id;
							BuildCallTables(this.callertbl, id, cn.caller, true);
							BuildCallTables(this.calleetbl, id, cn.callee, false);
						}
					}
				
				}
				ds.Tables.Add(this.callertbl);
				ds.Tables.Add(this.calleetbl);
			}
			catch
			{
				throw new Exception("Faild on build caller/callee data Tables \n");
			}
		}
		private void BuildCallTables(DataTable tbl, int id, Dictionary<Vertex, Edge> callhash, bool iscaller)
		{
			string nameAndSignature = null;
			callnode cn = new callnode();
			cn.id = id;
			foreach(Vertex cv in  callhash.Keys)
			{
				nameAndSignature = cv.name;
				if(cv.signature != null)
					nameAndSignature += ' ' + cv.signature;
				if(iscaller)
				{
					if( basedataId.ContainsKey(nameAndSignature))
					{
						cn.callerid = (int)basedataId[nameAndSignature];
						AddCallTableRow(tbl, cn, iscaller);
					}
				}
				else
				{
					if( basedataId.ContainsKey(nameAndSignature))
					{
						cn.calleeid = (int)basedataId[nameAndSignature];
						AddCallTableRow(tbl, cn, iscaller);
					}

				}
			}
		}

		private void AddCallTableRow(DataTable tmptbl, callnode n, bool iscaller)
		{
			DataRow tmpRow;

			tmpRow = tmptbl.NewRow();
			tmpRow["id"] = n.id;
			if(iscaller)
				tmpRow["callerid"] = n.callerid;
			else
				tmpRow["calleeid"] = n.calleeid;
			tmptbl.Rows.Add(tmpRow);
		}

		private void MakeCallTables(DataTable tbl, bool iscaller)
		{
			addTableRow(tbl, "System.Int32", "id");
			if(iscaller)
			{
				addTableRow(tbl, "System.Int32", "callerid");
			}
			else
			{
				addTableRow(tbl, "System.Int32", "calleeid");
			}
		}

		#endregion

		#region build Caller and callee Contribution table
		private void BuildContributionCalleeTable()
		{
			int id = -1;
			string nameAndSignature = null;
			bool exist = false;
			Hashtable cnnew;
			datanode pn1;
			datanode cn1;
			try
			{
				foreach(DictionaryEntry de in basedataId)
				{
					datanode pn= new datanode();
					datanode cn= new datanode();
					nameAndSignature = (string)de.Key;
					
					id = (int)basedataId[nameAndSignature];
					if(this._prevbasedata.ContainsKey(nameAndSignature))
					{
						pn = (datanode)_prevbasedata[nameAndSignature];
						if(_currbasedata.ContainsKey(nameAndSignature))
						{
							cn = (datanode)_currbasedata[nameAndSignature];
							exist = true;
						}
						else
						{
							exist = false;
						}
						cnnew = new Hashtable();
						foreach(string nameAndSignature1 in pn.calleeAlloc.Keys)
						{
							pn1 = new datanode();
							cn1 = new datanode();
						    
							if(this._prevbasedata.ContainsKey(nameAndSignature1))
							{
								pn1 = (datanode)_prevbasedata[nameAndSignature1];
								pn1.id = id;
								pn1.incl = FormatSize(int.Parse(pn.calleeAlloc[nameAndSignature1].ToString()));
								if(exist)
								{
									if(this._currbasedata.ContainsKey(nameAndSignature1))
									{
										cn1 = (datanode)_currbasedata[nameAndSignature1];
										cn1.id = id;
										if(cn.calleeAlloc.ContainsKey(nameAndSignature1))
										{
											cn1.incl = FormatSize(int.Parse(cn.calleeAlloc[nameAndSignature1].ToString()));
										}
										cnnew[nameAndSignature1] = 1;
									}
								}
								AddBaseTableRow(this.ContriTocalleetbl, nameAndSignature1, pn1, cn1);
								
							}
						}
						// adding item in new and not in old
						if(exist)
						{
							foreach(string nameAndSignature1 in cn.calleeAlloc.Keys)
							{
								pn1 = new datanode();
								if(!cnnew.ContainsKey(nameAndSignature1))
								{
									cn1 = (datanode)_currbasedata[nameAndSignature1];
									cn1.id = id;
									cn1.incl = FormatSize(int.Parse(cn.calleeAlloc[nameAndSignature1].ToString()));
									AddBaseTableRow(this.ContriTocalleetbl, nameAndSignature1, pn1, cn1);
								}
							}
						}
					}
					
					if(this._currbasedata.ContainsKey(nameAndSignature))
					{
						cn = (datanode)_currbasedata[nameAndSignature];
						pn1= new datanode();
						if(!this._prevbasedata.ContainsKey(nameAndSignature))
						{
							cn1 = (datanode)_currbasedata[nameAndSignature];
							foreach(string nameAndSignature1 in cn.calleeAlloc.Keys)
							{
								if(this._currbasedata.ContainsKey(nameAndSignature1))
								{
									cn1 = (datanode)_currbasedata[nameAndSignature1];
									cn1.id = id;
									cn1.incl = FormatSize(int.Parse(cn.calleeAlloc[nameAndSignature1].ToString()));
									AddBaseTableRow(this.ContriTocalleetbl, nameAndSignature1, pn1, cn1);
								}
								
							}
						}
					}
				}
				ds.Tables.Add(this.ContriTocalleetbl);
			}
			catch
			{
				throw new Exception("Faild on build caller/callee data Tables \n");
			}
		}


		private void BuildContributionCallerTable()
		{
			int id = -1;
			string nameAndSignature = null;
			bool exist = false;
			Hashtable cnnew;
			datanode pn1;
			datanode cn1;
			try
			{
				foreach(DictionaryEntry de in basedataId)
				{
					datanode pn= new datanode();
					datanode cn= new datanode();
					nameAndSignature = (string)de.Key;
					id = (int)basedataId[nameAndSignature];
					if(this._prevbasedata.ContainsKey(nameAndSignature))
					{
						pn = (datanode)_prevbasedata[nameAndSignature];
						if(_currbasedata.ContainsKey(nameAndSignature))
						{
							cn = (datanode)_currbasedata[nameAndSignature];
							exist = true;
						}
						else
						{
							exist = false;
						}
						cnnew = new Hashtable();
						foreach(Edge edge in pn.caller.Values)
						{
							pn1 = new datanode();
							cn1 = new datanode();
							string nameAndSignature1 = edge.FromVertex.name;
							if(edge.FromVertex.signature != null)
								nameAndSignature1 += ' ' + edge.FromVertex.signature;
							if(this._prevbasedata.ContainsKey(nameAndSignature1))
							{
								pn1 = (datanode)_prevbasedata[nameAndSignature1];
								pn1.id = id;
								pn1.incl = FormatSize((int)edge.weight);
								if(exist)
								{
									foreach(Edge edgec in cn.caller.Values)
									{
										if(edgec.FromVertex.name == edge.FromVertex.name && edgec.FromVertex.signature == edge.FromVertex.signature)
										{
											if(this._currbasedata.ContainsKey(nameAndSignature1))
											{
												cn1 = (datanode)_currbasedata[nameAndSignature1];
												cn1.id = id;
												cn1.incl = FormatSize((int)edgec.weight);
												cnnew[nameAndSignature1] = 1;
											}
										}
									}
								}
								AddBaseTableRow(this.ContriTocallertbl, nameAndSignature1, pn1, cn1);
								
							}
						}
						// adding item in new and not in old
						if(exist)
						{
							foreach(Edge edgec in cn.caller.Values)
							{
								pn1 = new datanode();
								string nameAndSignature1 = edgec.FromVertex.name;
								if(edgec.FromVertex.signature != null)
									nameAndSignature1 += ' ' + edgec.FromVertex.signature;

								if(!cnnew.ContainsKey(nameAndSignature1))
								{
									cn1 = (datanode)_currbasedata[nameAndSignature1];
									cn1.id = id;
									cn1.incl = FormatSize((int)edgec.weight);
									AddBaseTableRow(this.ContriTocallertbl, nameAndSignature1, pn1, cn1);
								}
							}
						}
					}
					
					if(this._currbasedata.ContainsKey(nameAndSignature))
					{
						cn = (datanode)_currbasedata[nameAndSignature];
						pn1= new datanode();
						if(!this._prevbasedata.ContainsKey(nameAndSignature))
						{
							cn1 = (datanode)_currbasedata[nameAndSignature];
							foreach(Edge edge in cn.caller.Values)
							{
								string nameAndSignature1 = edge.FromVertex.name;
								if(edge.FromVertex.signature != null)
									nameAndSignature1 += ' ' + edge.FromVertex.signature;
								if(this._currbasedata.ContainsKey(nameAndSignature1))
								{
									cn1 = (datanode)_currbasedata[nameAndSignature1];
									cn1.id = id;
									cn1.incl = FormatSize((int)edge.weight);
									AddBaseTableRow(this.ContriTocallertbl, nameAndSignature1, pn1, cn1);
								}
								
							}
						}
					}
				}
				ds.Tables.Add(this.ContriTocallertbl);
			}
			catch
			{
				throw new Exception("Faild on build caller/callee data Tables \n");
			}
		}
		#endregion

		#region share used functions
		private void addTableRow(DataTable tbl, string colType, string colName)
		{
			DataColumn tmpColumn;
			tmpColumn = new DataColumn();
			tmpColumn.DataType = System.Type.GetType(colType);
			tmpColumn.ColumnName = colName;
			tbl.Columns.Add(tmpColumn);
		}

		private void getDetailFilter(ref DetailFilter df)
		{
#if(V_EXEC)
			double max = FormatSize(df.max);
			df.detail01 = Math.Round( (max / detail01D), 2);
			df.detail02 = Math.Round((max / (detail01D - 1)), 2);
			df.detail05 = Math.Round((max / (detail01D - 2)), 2);
			df.detail1 = Math.Round((max / (detail01D - 3)), 2);
			df.detail2 = Math.Round((max / (detail01D - 4)), 2);
			df.detail5 = Math.Round((max / (detail01D - 5)), 2);
			df.detail10 = Math.Round((max / (detail01D - 6)), 2);
#else
			ulong max = df.max;
			df.detail01 = max / detail01D;
			df.detail02 = max / (detail01D - 1);
			df.detail05 = max /(detail01D - 2);
			df.detail1 = max / (detail01D - 3);
			df.detail2 = max / (detail01D - 4);
			df.detail5 = max / (detail01D - 5);
			df.detail10 = max / (detail01D - 6);
#endif
		}

#if(V_EXEC)
		double FormatSize(int size)
		{
			double w = size;
			w /= 1024;
			return Math.Round(w, 2);
		}
#else
		int FormatSize(int size)
		{
			return size;
		}
#endif
		#endregion

		#region CallTrace - MakeDiffTreceTable, BuildDiffTraceTable
		private void BuildDiffTraceTable(DiffDataNode parent, TreeNode currRoot, TreeNode prevRoot)
		{
			ArrayList currKids = new ArrayList();
			ArrayList prevKids = new ArrayList();
			ArrayList currDKids = new ArrayList();
			ArrayList prevDKids = new ArrayList();

			//get kids
			if(currRoot != null)
			{
				currKids = _currcallTrace.FetchKids(null, currRoot);
				if(currKids.Count >0)
				{
					currDKids = TransCurrTree(currKids);
				}
			}
			if(prevRoot != null)
			{
				prevKids = _prevcallTrace.FetchKids(null, prevRoot);
				if(prevKids.Count > 0)
				{
					prevDKids = TransPrevTree(prevKids);
				}
			}
          
			// get diff node
			ArrayList diffKids = GetDiffKids(parent, currDKids, prevDKids);

			// recursive for each diff node
			for(int i = 0; i < diffKids.Count; i++)
			{
				BuildDiffTraceTable(diffKids[i] as DiffDataNode, ((DiffDataNode)diffKids[i]).currTreenode as TreeNode, ((DiffDataNode)diffKids[i]).prevTreenode as TreeNode);
				
			}
			

		}
	
		private ArrayList TransCurrTree(ArrayList treeNode)
		{
			ArrayList diffnodes = new ArrayList();
			int functionId = 0;
			int [] kidStacktrace;
			
			for( int i = 0; i < treeNode.Count; i++)
			{
				TreeNode kidNode = treeNode[i] as TreeNode;
				if(kidNode.data.bytesAllocated >0)
				{
					
					kidStacktrace = _currcallTrace.IndexToStacktrace(kidNode.stackid);
					if (kidNode.nodetype == TreeNode.NodeType.Call)
					{
						functionId = kidStacktrace[ kidStacktrace.Length - 1 ];
					}
					else if (kidNode.nodetype == TreeNode.NodeType.Allocation)
					{
						functionId = kidStacktrace[ 0 ];
					
					}
				
					string name = _currcallTrace.MakeName(kidNode);
					DiffDataNode node = new DiffDataNode(name);
					node.currIncl = kidNode.data.bytesAllocated;
					node.currCalls = kidNode.data.numberOfFunctionsCalled;
														
				
					node.currTreenode = kidNode;
					node.nodetype = (DiffDataNode.NodeType)kidNode.nodetype;
					
					switch(node.nodetype)
					{
						case DiffDataNode.NodeType.Allocation:
							node.currFunId = functionId;
							break;

						case DiffDataNode.NodeType.Call:
							node.currFunId = functionId;
							node.mapname = _currcallTrace.names[functionId];
							string sig = _currcallTrace.signatures[functionId];
							if(sig != null)
							{
								node.mapname += ' ' + sig;
							}
							break;
					}
				
					diffnodes.Add(node);
				}
				
			}

			return diffnodes;
		}

		private ArrayList TransPrevTree(ArrayList treeNode)
		{
			ArrayList diffnodes = new ArrayList();
			int functionId = 0;
			int [] kidStacktrace;
			
			for( int i = 0; i < treeNode.Count; i++)
			{
				TreeNode kidNode = treeNode[i] as TreeNode;
				if(kidNode.data.bytesAllocated >0)
				{
					
					kidStacktrace = _prevcallTrace.IndexToStacktrace(kidNode.stackid);
					if (kidNode.nodetype == TreeNode.NodeType.Call)
					{
						functionId = kidStacktrace[ kidStacktrace.Length - 1 ];
					}
					else if (kidNode.nodetype == TreeNode.NodeType.Allocation)
					{
						functionId = kidStacktrace[ 0 ];
					
					}
				
					string name = _prevcallTrace.MakeName(kidNode);
					DiffDataNode node = new DiffDataNode(name);
					node.prevIncl = kidNode.data.bytesAllocated;
					node.prevCalls = kidNode.data.numberOfFunctionsCalled;
				
				
					node.prevTreenode = kidNode;
					node.nodetype = (DiffDataNode.NodeType)kidNode.nodetype;
					
					switch(node.nodetype)
					{
						case DiffDataNode.NodeType.Allocation:
							node.prevFunId = functionId;
							break;

						case DiffDataNode.NodeType.Call:
							node.prevFunId= functionId;
							node.mapname = _prevcallTrace.names[functionId];
							string sig = _prevcallTrace.signatures[functionId];
							if(sig != null)
							{
								node.mapname += ' ' + sig;
							}
							break;
					}
					diffnodes.Add(node);
				
				}
			}

			return diffnodes;
		}
		
		
		private ArrayList GetDiffKids(DiffDataNode parent, ArrayList currKids, ArrayList prevKids)
		{
			ArrayList curr = new ArrayList();
			Hashtable curr_inclOfNode = new Hashtable();
			ArrayList prev = new ArrayList();
			Hashtable prev_inclOfNode = new Hashtable();

			ArrayList diffnodes = new ArrayList();
			for(int i = 0; i < currKids.Count; i++)
			{
				if( !((DiffDataNode)currKids[i]).marked)
				{
					DiffDataNode node = new DiffDataNode( ((DiffDataNode)currKids[i]).name);
					int idx = CurrExactMatchIndex(prevKids, currKids[i] as DiffDataNode);
					if(idx >=0)
					{
						node.currFunId = ((DiffDataNode)currKids[i]).currFunId;
						node.prevFunId = ((DiffDataNode)prevKids[idx]).prevFunId;
						node.mapname = ((DiffDataNode)currKids[i]).mapname;
						node.currIncl = ((DiffDataNode)currKids[i]).currIncl;
						node.prevIncl = ((DiffDataNode)prevKids[idx]).prevIncl;
						node.diffIncl = node.currIncl - node.prevIncl;
						node.currCalls = ((DiffDataNode)currKids[i]).currCalls;
						node.prevCalls = ((DiffDataNode)prevKids[idx]).prevCalls;
						node.diffCalls = node.currCalls - node.prevCalls;
						
						node.nodeId = nodeidx;
						node.parentId = parent.nodeId;
						node.parentname = parent.name;
						node.currTreenode = ((DiffDataNode)currKids[i]).currTreenode;
						node.prevTreenode = ((DiffDataNode)prevKids[idx]).prevTreenode;
						node.nodetype = ((DiffDataNode)currKids[i]).nodetype;

						((DiffDataNode)currKids[i]).marked = true;
						((DiffDataNode)prevKids[idx]).marked = true;
						if(node.diffIncl != 0)
						{
							diffnodes.Add(node);
							AddDiffTraceTableRow(diffTracetbl, node);
							nodeidx++;
						
						}
					}
					else
					{
						long incl = ((DiffDataNode)currKids[i]).currIncl;
						curr_inclOfNode[currKids[i]] = incl; 
						curr.Add(currKids[i]);
					}
				}
			}

			for(int i = 0; i < prevKids.Count; i++)
			{
				if( !((DiffDataNode)prevKids[i]).marked)
				{
					DiffDataNode node = new DiffDataNode( ((DiffDataNode)prevKids[i]).name);
					int idx = PrevExactMatchIndex(currKids, prevKids[i] as DiffDataNode);
					if(idx >=0)
					{
						node.currFunId = ((DiffDataNode)currKids[idx]).currFunId;
						node.prevFunId = ((DiffDataNode)prevKids[i]).prevFunId;
						node.mapname = ((DiffDataNode)currKids[idx]).mapname;
						node.currIncl = ((DiffDataNode)currKids[idx]).currIncl;
						node.prevIncl = ((DiffDataNode)prevKids[i]).prevIncl;
						node.diffIncl = node.currIncl - node.prevIncl;
						node.currCalls = ((DiffDataNode)currKids[idx]).currCalls;
						node.prevCalls = ((DiffDataNode)prevKids[i]).prevCalls;
						node.diffCalls = node.currCalls - node.prevCalls;
						
						node.nodeId = nodeidx;
						node.parentId = parent.nodeId;
						node.parentname = parent.name;
						node.currTreenode = ((DiffDataNode)currKids[idx]).currTreenode;
						node.prevTreenode = ((DiffDataNode)prevKids[i]).prevTreenode;
						node.nodetype = ((DiffDataNode)prevKids[i]).nodetype;

						((DiffDataNode)currKids[idx]).marked = true;
						((DiffDataNode)prevKids[i]).marked = true;
						if(node.diffIncl != 0)
						{
							diffnodes.Add(node);
							AddDiffTraceTableRow(diffTracetbl, node);
							nodeidx++;
						
						}
					}
					else
					{
						long incl = ((DiffDataNode)prevKids[i]).prevIncl;
						prev_inclOfNode[prevKids[i]] = incl; 
						prev.Add(prevKids[i]);
					}
				}
			}
			curr.Sort(new CompareIncl(curr_inclOfNode));
			prev.Sort(new CompareIncl(prev_inclOfNode));
			for(int i = 0; i < curr.Count; i++)
			{
				
				if( !((DiffDataNode)curr[i]).marked)
				{
					DiffDataNode node = new DiffDataNode( ((DiffDataNode)curr[i]).name);
					int idx = FirstMatchIndex(prevKids, curr[i] as DiffDataNode);
					if(idx >=0)
					{
						node.currFunId = ((DiffDataNode)curr[i]).currFunId;
						node.prevFunId = ((DiffDataNode)prevKids[idx]).prevFunId;
						node.mapname = ((DiffDataNode)curr[i]).mapname;
						node.currIncl = ((DiffDataNode)curr[i]).currIncl;
						node.prevIncl = ((DiffDataNode)prevKids[idx]).prevIncl;
						node.diffIncl = node.currIncl - node.prevIncl;
						node.currCalls = ((DiffDataNode)curr[i]).currCalls;
						node.prevCalls = ((DiffDataNode)prevKids[idx]).prevCalls;
						node.diffCalls = node.currCalls - node.prevCalls;
						
						node.nodeId = nodeidx;
						node.parentId = parent.nodeId;
						node.parentname = parent.name;
						node.currTreenode = ((DiffDataNode)curr[i]).currTreenode;
						node.prevTreenode = ((DiffDataNode)prevKids[idx]).prevTreenode;
						node.nodetype = ((DiffDataNode)curr[i]).nodetype;

						((DiffDataNode)curr[i]).marked = true;
						((DiffDataNode)prevKids[idx]).marked = true;
						if(node.diffIncl != 0)
						{
							diffnodes.Add(node);
							AddDiffTraceTableRow(diffTracetbl, node);
							nodeidx++;
						
						}
					}
					else
					{
						node.currFunId = ((DiffDataNode)curr[i]).currFunId;
						node.mapname = ((DiffDataNode)curr[i]).mapname;
						node.currIncl = ((DiffDataNode)curr[i]).currIncl;
						node.prevIncl = 0;
						node.diffIncl = node.currIncl;
						node.currCalls = ((DiffDataNode)curr[i]).currCalls;
						node.prevCalls = 0;
						node.diffCalls = node.currCalls;
						
						node.nodeId = nodeidx;
						node.parentId = parent.nodeId;
						node.parentname = parent.name;
						node.currTreenode = ((DiffDataNode)curr[i]).currTreenode;
						node.nodetype = ((DiffDataNode)curr[i]).nodetype;
						((DiffDataNode)curr[i]).marked = true;
						if(node.diffIncl != 0)
						{
							diffnodes.Add(node);
							AddDiffTraceTableRow(diffTracetbl, node);
							nodeidx++;
						}
					}
				}
				
			}

			for(int i = 0; i < prev.Count; i++)
			{
				if(!((DiffDataNode)prev[i]).marked)
				{
					DiffDataNode node = new DiffDataNode( ((DiffDataNode)prev[i]).name);
					
					// prev not exist in curr
					node.prevFunId = ((DiffDataNode)prev[i]).prevFunId;
					node.mapname = ((DiffDataNode)prev[i]).mapname;
					node.currIncl = 0;
					node.prevIncl = ((DiffDataNode)prev[i]).prevIncl;
					node.diffIncl = -node.prevIncl;
					node.currCalls = 0;
					node.prevCalls = ((DiffDataNode)prev[i]).prevCalls;
					node.diffCalls = -node.prevCalls;
					
					node.nodeId = nodeidx;
					node.parentId = parent.nodeId;
					node.parentname = parent.name;
					node.prevTreenode = ((DiffDataNode)prev[i]).prevTreenode;
					node.nodetype = ((DiffDataNode)prev[i]).nodetype;

					((DiffDataNode)prev[i]).marked = true;
					if(node.diffIncl != 0)
					{
						diffnodes.Add(node);
						AddDiffTraceTableRow(diffTracetbl, node);
						nodeidx++;
					}
				}
				
			}
			for(int i = 0; i < currKids.Count; i++)
			{
				((DiffDataNode)currKids[i]).marked = false;
			}
			for(int i = 0; i < prevKids.Count; i++)
			{
				((DiffDataNode)prevKids[i]).marked = false;
			}

			return diffnodes;
		}
	
		

		private int CurrExactMatchIndex(ArrayList nodelst, DiffDataNode node)
		{
			for(int i = 0; i < nodelst.Count; i++)
			{
				if( ((DiffDataNode)nodelst[i]).name.Equals(node.name) && 
					!((DiffDataNode)nodelst[i]).marked &&
					(node.currIncl - ((DiffDataNode)nodelst[i]).prevIncl) == 0)
				{
					return i;
				}
			}
			return -1;
		}
		private int PrevExactMatchIndex(ArrayList nodelst, DiffDataNode node)
		{
			for(int i = 0; i < nodelst.Count; i++)
			{
				if( ((DiffDataNode)nodelst[i]).name.Equals(node.name) && 
					!((DiffDataNode)nodelst[i]).marked &&
					(node.prevIncl - ((DiffDataNode)nodelst[i]).currIncl) == 0)
				{
					return i;
				}
			}
			return -1;
		}
		private int FirstMatchIndex(ArrayList nodelst, DiffDataNode node)
		{
			
			int idx = -1;
			long savedalloc = long.MaxValue;
			long alloc = 0;

			for(int i = 0; i < nodelst.Count; i++)
			{
				if( ((DiffDataNode)nodelst[i]).name.Equals(node.name) && 
					!((DiffDataNode)nodelst[i]).marked )
				{
					alloc = Math.Abs(node.currIncl - ((DiffDataNode)nodelst[i]).prevIncl);
					if(alloc < savedalloc)
					{
						idx = i;
						savedalloc = alloc;
						continue;
					}
				}
			}
			
			return idx;
		}
		
		#endregion

		#region Summary table
		internal void RefreshCallTreeNodes(DiffDataNode node)
		{
			node.IsExpanded = false;
			for(int i = 0; i < node.allkids.Count; i++)
			{
				RefreshCallTreeNodes(node.allkids[i] as DiffDataNode);
			}

		}
		internal void GetAllKids(DiffDataNode root, string filter)
		{
			DataRow[] rKids = summaryTracetbl.Select(filter, "name asc");
			if(rKids.Length > 0)
			{
				root.HasKids = true;
				root.depth = 0;
			}
			for(int i = 0; i < rKids.Length; i++)
			{
				DiffDataNode kidNode = Row2Node(rKids[i]);
				root.allkids.Add(kidNode);
				
			}
		}
		private void BuildSummaryTable(DiffDataNode parent, int parentId, string filter)
		{
			depth++;
			parent.depth = depth;
			parent.allkids.Clear();
			parent.HasKids = false;
			Hashtable kidSum = new Hashtable();
			string name = null;
			
			DataRow[] kidsRows = diffTracetbl.Select(filter);
			for(int i = 0; i < kidsRows.Length; i++)
			{
				DiffDataNode sumNode = Row2Node(kidsRows[i]);
				name = sumNode.mapname;
				if(kidSum.ContainsKey(name))
				{
					DiffDataNode updateNode = kidSum[name] as DiffDataNode;
					updateNode.prevIncl += sumNode.prevIncl;
					updateNode.currIncl += sumNode.currIncl;
					updateNode.diffIncl = updateNode.currIncl - updateNode.prevIncl;
					if(sumNode.prevIncl != 0)
					{
						updateNode.prevCalls++;
					}
					if(sumNode.currIncl != 0)
					{
						updateNode.currCalls++;
					}
					updateNode.diffCalls = updateNode.currCalls - updateNode.prevCalls;
					updateNode.allkids.Add(sumNode.nodeId);
					updateNode.HasKids = true;
					
				}
				else
				{
					if(sumNode.prevIncl != 0)
					{
						sumNode.prevCalls = 1;
					}
					if(sumNode.currIncl != 0)
					{
						sumNode.currCalls = 1;
					}
					sumNode.parentId = parentId;
					sumNode.allkids.Add(sumNode.nodeId);
					sumNode.diffIncl = sumNode.currIncl - sumNode.prevIncl;
					sumNode.diffCalls = sumNode.currCalls - sumNode.prevCalls;
					kidSum.Add(name, sumNode);
					sumNode.HasKids = false;
					sumNode.depth = depth;
					sumNode.nodeId = sumnodeidx;
					sumnodeidx++;
				}
				
			}
			if(kidSum.Count > 0)
			{
				if(parent.nodetype == DiffDataNode.NodeType.Call)
				{
					parent.HasKids = true;
				}
				string diffkey = parent.mapname + parent.prevIncl + parent.currIncl + parent.diffIncl + parent.prevFunId + parent.currFunId;
				if(!diffCallTreeNodes.ContainsKey(diffkey))
				{
					diffCallTreeNodes.Add(diffkey, parent);
				}
				
			}
						

			foreach(string key in kidSum.Keys)
			{
				DiffDataNode sumNode = kidSum[key] as DiffDataNode;
				if(! (sumNode.diffIncl == 0))
				{
					parent.allkids.Add(sumNode);
                    AddDiffTraceTableRow(summaryTracetbl, sumNode);
				}
				string kidFilter = getFilter(sumNode.allkids);
				BuildSummaryTable(sumNode, sumNode.nodeId,kidFilter);
			}
			
			depth--;
			
		}

	
		private string getFilter(ArrayList kids)
		{
			string filter = "parentId in (";
			if(kids.Count > 1)
			{
				for(int i = 0; i < kids.Count-1; i++)
				{
					filter += kids[i].ToString() + ',';
				}
				filter += kids[kids.Count-1].ToString();
			}
			else if(kids.Count == 1)
			{
				filter += kids[0].ToString();
			}
			filter += ")";
			return filter;
		}
		internal DiffDataNode Row2Node(DataRow r)
		{
			string name = r[idx_name].ToString();
			DiffDataNode node = new DiffDataNode(name); 
			
			node.mapname = r[idx_mapname].ToString();
				
			node.prevIncl = int.Parse(r[idx_prevIncl].ToString());
			node.currIncl = int.Parse(r[idx_currIncl].ToString());
			node.diffIncl = int.Parse(r[idx_diffIncl].ToString());
			node.prevFunId = int.Parse(r[idx_prevFunid].ToString());
			node.currFunId = int.Parse(r[idx_currFunid].ToString());
			node.prevCalls = int.Parse(r[idx_prevCalls].ToString());
			node.currCalls = int.Parse(r[idx_currCalls].ToString());
			node.diffCalls = int.Parse(r[idx_diffCalls].ToString());
			int nodetype = int.Parse(r[idx_type].ToString());
			if(nodetype == 0)
			{
				node.nodetype = DiffDataNode.NodeType.Call;
			}
			else if(nodetype == 1)
			{
				node.nodetype = DiffDataNode.NodeType.Allocation;
			}
			else
			{
				node.nodetype = DiffDataNode.NodeType.AssemblyLoad;
			}
				
			node.nodeId = int.Parse(r[idx_id].ToString());
			node.parentId = int.Parse(r[idx_parentid].ToString());
			node.depth = int.Parse(r[idx_depth].ToString());
			return node;
		}

		private void MakeDiffTreceTable(DataTable tbl)
		{
			addTableRow(tbl, "System.Int32", "parentid");
			addTableRow(tbl, "System.String", "parentname");
			addTableRow(tbl, "System.Int32", "id");
			addTableRow(tbl, "System.String", "name");
			addTableRow(tbl, "System.String", "mapname");
			
			addTableRow(tbl, "System.Int32", "prevIncl");
			addTableRow(tbl, "System.Int32", "currIncl");
			addTableRow(tbl, "System.Int32", "diffIncl");
			addTableRow(tbl, "System.Int32", "prevCalls");
			addTableRow(tbl, "System.Int32", "currCalls");
			addTableRow(tbl, "System.Int32", "diffCalls");
			addTableRow(tbl, "System.Int32", "prevFunId");
			addTableRow(tbl, "System.Int32", "currFunId");
			addTableRow(tbl, "System.Int32", "nodetype");
			addTableRow(tbl, "System.Int32", "depth");
			
			
			

			tbl.Columns["parentid"].DefaultValue = -1;
			tbl.Columns["parentname"].DefaultValue = "";
			tbl.Columns["id"].DefaultValue = -1;
			tbl.Columns["name"].DefaultValue = "";
			tbl.Columns["mapname"].DefaultValue = "";
			tbl.Columns["prevIncl"].DefaultValue = 0;
			tbl.Columns["currIncl"].DefaultValue = 0;
			tbl.Columns["diffIncl"].DefaultValue = 0;
			tbl.Columns["prevCalls"].DefaultValue = 0;
			tbl.Columns["currCalls"].DefaultValue = 0;
			tbl.Columns["diffCalls"].DefaultValue = 0;
			tbl.Columns["prevFunId"].DefaultValue = -1;
			tbl.Columns["currFunId"].DefaultValue = -1;
			tbl.Columns["nodetype"].DefaultValue = -1;
			tbl.Columns["depth"].DefaultValue = 0;
			

			tbl.Columns["diffIncl"].Expression = "currIncl - prevIncl";
			tbl.Columns["diffCalls"].Expression = "currCalls - prevCalls";
		}
		private void AddDiffTraceTableRow(DataTable tmptbl, DiffDataNode node)
		{			
			DataRow tmpRow = tmptbl.NewRow();

			tmpRow["parentid"] = node.parentId;
			tmpRow["id"] = node.nodeId;
			tmpRow["parentname"] = node.parentname;
			tmpRow["name"] = node.name;
			tmpRow["mapname"] = node.mapname;
			tmpRow["prevIncl"] = node.prevIncl;
			tmpRow["currIncl"] = node.currIncl;
			tmpRow["prevCalls"] = node.prevCalls;
			tmpRow["currCalls"] = node.currCalls;
			tmpRow["prevFunId"] = node.prevFunId;
			tmpRow["currFunId"] = node.currFunId;
			if(node.nodetype == DiffDataNode.NodeType.Call)
			{
				tmpRow["nodetype"] = 0;
			}
			else if(node.nodetype == DiffDataNode.NodeType.Allocation)
			{
				tmpRow["nodetype"] = 1;
			}
			else if(node.nodetype == DiffDataNode.NodeType.AssemblyLoad)
			{
				tmpRow["nodetype"] = 2;
			}
			tmpRow["depth"] = node.depth;
			tmptbl.Rows.Add(tmpRow);
			
		}
		#endregion
	

		#region EXCLUSIVE
		private void ReadFile(CallTreeForm callTrace, string fileName, Hashtable FuncExcl, Hashtable TypeExcl)
		{
			Hashtable funcCalled = new Hashtable();
			Hashtable TypeAlloc = new Hashtable();
			Stream s = null;
			ProgressForm progressForm = null;
			try
			{
				s = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				r = new StreamReader(s);

				progressForm = new ProgressForm();
				progressForm.Text = "Preparing call tree view";
				progressForm.Visible = true;
				progressForm.setProgress(0);
				progressForm.TopMost = false;
				int maxProgress = (int)(r.BaseStream.Length/1024);
				progressForm.setMaximum(maxProgress);

				buffer = new byte[4096];
				bufPos = 0;
				bufLevel = 0;
				line = 1;
				StringBuilder sb = new StringBuilder();
				c = ReadChar();

				bool found;
				string assemblyName = null;
				int threadid = 0, stackid = 0;
				TreeNode.NodeType nodetype = TreeNode.NodeType.Call;

				while (c != -1)
				{
					found = false;
					if ((line % 1024) == 0)
					{
						int currentProgress = (int)(pos/1024);
						if (currentProgress <= maxProgress)
						{
							progressForm.setProgress(currentProgress);
						}
					}
					lastLineStartPos = pos-1;
					switch (c)
					{
						case    -1:
							break;
				
							// 'A' with thread identifier
						case    '!':
						{
							found = true;
							c = ReadChar();
							threadid = ReadInt();
							ReadInt();
							stackid = ReadInt();
							nodetype = TreeNode.NodeType.Allocation;
							if (c == -1)	{found = false;}
							break;
			
						}

						case    'C':
						case    'c':
						{
							found = true;
							c = ReadChar();
							nodetype = TreeNode.NodeType.Call;
							threadid = ReadInt();
							stackid = ReadInt();
							if (c == -1)	{found = false;}
							break;
						}

						
						case 'y':
						case 'Y':
						{
							found = true;
							c = ReadChar();
							nodetype = TreeNode.NodeType.AssemblyLoad;
							threadid = ReadInt();
							ReadInt();

							while (c == ' ' || c == '\t')
							{
								c = ReadChar();
							}
							sb.Length = 0;
							while (c > ' ')
							{
								sb.Append((char)c);
								c = ReadChar();
							}
							assemblyName = sb.ToString();
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
					if(!found)
					{
						continue;
					}
				
					string name = null;
					string typename = null;

					int[] stacktrace = callTrace.IndexToStacktrace(stackid);
					int functionId = (nodetype != TreeNode.NodeType.AssemblyLoad ? stacktrace[stacktrace.Length - 1] : 0);
					switch(nodetype)
					{
						case TreeNode.NodeType.Allocation:
							string key = null;
							if( (functionId < callTrace.LogResult.callstackHistogram.readNewLog.funcName.Length )&& 
								((name = callTrace.LogResult.callstackHistogram.readNewLog.funcName[functionId]) != null))
							{
								if( callTrace.LogResult.callstackHistogram.readNewLog.funcSignature[functionId] != null)
								{
									name += ' ' + callTrace.LogResult.callstackHistogram.readNewLog.funcSignature[functionId];
								}
							}
							else
							{
								name = "NATIVE FUNCTION ( UNKNOWN ARGUMENTS )";
							}
														
							// function Excl							
							if(FuncExcl.ContainsKey(name))
							{
								int alloc = (int)FuncExcl[(string)name];
								alloc += stacktrace[1];
								FuncExcl[name] = alloc;
							}
							else
							{
								FuncExcl.Add(name, stacktrace[1]);
							}
							
							// Type Excl
							if( stacktrace[0]>=0 && stacktrace[0] < callTrace.LogResult.callstackHistogram.readNewLog.typeName.Length)
							{
								typename = callTrace.LogResult.callstackHistogram.readNewLog.typeName[stacktrace[0]];
							}
							if(typename == null)
								typename = "NATIVE FUNCTION ( UNKNOWN ARGUMENTS )";
																											
							if(TypeExcl.ContainsKey(typename))
							{
								int alloc = (int)TypeExcl[(string)typename];
								alloc += stacktrace[1];
								TypeExcl[typename] = alloc;
							}
							else
							{
								TypeExcl.Add(typename, stacktrace[1]);
							}
							
							// Type Allocated by Excl
							if(name != "NATIVE FUNCTION ( UNKNOWN ARGUMENTS )")
								key = typename + '|' + functionId;
							else
								key = typename + '|' + 0;
							if( TypeAlloc.ContainsKey(key))
							{
								int alloc = (int)TypeAlloc[key];
								alloc += stacktrace[1];
								TypeAlloc[key] = alloc;
							}
							else
							{
								TypeAlloc.Add(key, stacktrace[1]);
							}
									
							break;
						case TreeNode.NodeType.Call:
							if(funcCalled.ContainsKey(functionId))
							{
								int calls = (int)funcCalled[functionId] + 1;;
								funcCalled[functionId]= calls;
							}
							else
							{
								funcCalled.Add(functionId,1);
							}
							break;
					}
				}

			}
			catch (Exception)
			{
				throw new Exception(string.Format("Bad format in log file {0} line {1}", fileName, line));
			}

			finally
			{
				progressForm.Visible = false;
				progressForm.Dispose();
				if (r != null)
					r.Close();
			}
		}
		internal int ReadChar()
		{
			pos++;
			if (bufPos < bufLevel)
				return buffer[bufPos++];
			else
				return FillBuffer();
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
					value = value*10 + c - '0';
					c = ReadChar();
				}

				if (negative)
					value = -value;
				return value;
			}
			else
			{
				return Int32.MinValue;
			}
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
				value = value*16 + digit;
			}
		}


		#endregion

	}
}
