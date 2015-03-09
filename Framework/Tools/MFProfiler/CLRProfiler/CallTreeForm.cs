////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.Win32;

namespace CLRProfiler
{
    /// <summary>
    /// Summary description for CallTreeForm.
    /// </summary>
    internal class CallTreeForm : System.Windows.Forms.Form, IComparer, ITreeOwner
    {
        /* info given from the outside */
        private string logFileName;
        private ReadLogResult logResult;
        internal string[] names, types, signatures;

        /* everything about the backing store */
        private Stream backingStore;
        private string backingStoreFileName;
        private BitReader reader;
        private BitWriter writer;

        private Font defaultFont;

        /* configuration information */
        private RegistryKey rkProfiler;
        private Rectangle formRect = new Rectangle( -1, -1, -1, -1 );
        private int splitterX = -1;

        /* sorting info for the stackview control */
        private int sortCol = -1;
        private int prevSortCol = -1;
        private bool fReverseSort = false;
        private int subtreeline = -1;
        private int contextSelection;

        /*flag for Compare view*/
        internal bool forCompare = false;

        /* various collections */
        internal struct GlobalCallStats
        {
            internal bool calledAlready;
            internal int timesCalled;
            internal int totalBytesAllocated;
            internal int totalFunctionsCalled;
            internal int totalNewFunctionsBroughtIn;
        }

        internal struct GlobalAllocStats
        {
            internal int timesAllocated;
            internal int totalBytesAllocated;
        }

        internal class SortingBehaviour
        {
            internal int sortingOrder;
            internal int counterId;
        }

        internal class ThreadState
        {
            internal int[] prevStackTrace;
            internal int prevStackLen;
            internal int prevDepth;
            internal ArrayList stack;
            internal ArrayList queuedNodes;
            internal SortedList functions;
            internal TreeListView callTreeView;
        };

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

        internal struct FnViewFilter
        {
            internal TreeNode.NodeType nodetype;
            internal int functionId;

            internal FnViewFilter( TreeNode.NodeType Nodetype, int FunctionId)
            {
                nodetype = Nodetype;
                functionId = FunctionId;
            }
        };

        /* threads */
        private int firstThread;
        private Dictionary<int, ThreadState> threads;

        /* cache of display information about the current thread */
        private int currentThreadId;
        private bool manyThreads;
        private ViewState viewState;
        internal TreeListView callTreeView;

        /* random stuff */
        private Label threadIDLabel;
        private ComboBox threadIDList;
        private int previousSplitterLocation;
        private int firstNewStack;

        /* global stats */
        private GlobalCallStats[] globalCallStats;
        private GlobalAllocStats[] globalAllocStats;

        /* some lookup info about the assemblies */
        private ArrayList assemblyNames;
        private Hashtable assemblyNameToIdMap;

        /* filters */
        private FnViewFilter[] filterInclude;
        private FnViewFilter[] filterExclude;
        private bool fShowSubtree;
        
                
        internal ReadLogResult LogResult
        {
            get {return logResult;}
        }
        internal GlobalCallStats[] CallStats
        {
            get {return globalCallStats;}
        }

        internal GlobalAllocStats[] AllocStats
        {
            get {return globalAllocStats;}
        }

        
        /* controls */
        private System.Windows.Forms.Button allFunctionsButton;
        private System.Windows.Forms.Panel controlCollection;
        private System.Windows.Forms.Button allAllocationsButton;
        private System.Windows.Forms.ListView stackView;
        private System.Windows.Forms.Splitter splitter;
        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.MainMenu mainMenu1;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem selectColumns;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        StreamReader r;
        byte[] buffer;
        int c;
        int line;
        long pos;
        long lastLineStartPos;
        int bufPos;
        private System.Windows.Forms.MenuItem menuItem2;
        private System.Windows.Forms.MenuItem menuItem3;
        private System.Windows.Forms.MenuItem menuItem4;
        private System.Windows.Forms.MenuItem menuItem5;
        private System.Windows.Forms.MenuItem menuItem6;
        private System.Windows.Forms.MenuItem menuItem7;
        private System.Windows.Forms.MenuItem menuItemShowFuture;
        private System.Windows.Forms.MenuItem menuItemCopyStack;
        int bufLevel;

        /* <parsers> */
        internal int ReadChar()
        {
            pos++;
            if (bufPos < bufLevel)
                return buffer[bufPos++];
            else
                return FillBuffer();
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

        int ForcePosInt()
        {
            int value = ReadInt();
            if (value >= 0)
                return value;
            else
                throw new Exception(string.Format("Bad format in log file {0} line {1}", logFileName, line));
        }
        /* </parsers> */

        private Column AddColumn(TreeListView treeView, int counterId)
        {
            Column c = treeView.AddColumn(new ColumnInformation(counterId,
                Statistics.GetCounterName(counterId),
                ColumnInformation.ColumnTypes.String),
                60);
            if(Statistics.IsInclusive(counterId))
            {
                c.Font = new Font(c.Font, FontStyle.Bold);
            }
            return c;
        }

        internal int[] IndexToStacktrace(int stackid)
        {
            if(stackid < 0)
            {
                int[] fakeStackTrace = new int[1 - stackid];
                fakeStackTrace[0] = 0;
                for(int i = 1; i < 1 - stackid; i++)
                {
                    fakeStackTrace[i] = -1;
                }
                return fakeStackTrace;
            }
            else
            {
                return logResult.callstackHistogram.readNewLog.stacktraceTable.IndexToStacktrace(
                    logResult.callstackHistogram.readNewLog.stacktraceTable.MapTypeSizeStacktraceId(stackid));
            }
        }

        
        
        
        
        internal CallTreeForm(string in_logFileName, ReadLogResult in_result, bool forCompare)
        {
            this.forCompare = forCompare;
            
            logFileName = in_logFileName;
            logResult = in_result;
            names = logResult.callstackHistogram.readNewLog.funcName;
            types = logResult.callstackHistogram.readNewLog.typeName;
            signatures = logResult.callstackHistogram.readNewLog.funcSignature;

            filterInclude = new FnViewFilter[] { new FnViewFilter(TreeNode.NodeType.Call, -1), new FnViewFilter(TreeNode.NodeType.Call, -1) };
            filterExclude = new FnViewFilter[] { new FnViewFilter(TreeNode.NodeType.Call, -1), new FnViewFilter(TreeNode.NodeType.Call, -1) };

            fShowSubtree = false;
            firstNewStack = -1;

            GetConfiguration();

            InitForm( true );
        }

        

        

        internal CallTreeForm(string in_logFileName, ReadLogResult in_result)
        {
            logFileName = in_logFileName;
            logResult = in_result;
            names = logResult.callstackHistogram.readNewLog.funcName;
            types = logResult.callstackHistogram.readNewLog.typeName;
            signatures = logResult.callstackHistogram.readNewLog.funcSignature;

            filterInclude = new FnViewFilter[] { new FnViewFilter(TreeNode.NodeType.Call, -1), new FnViewFilter(TreeNode.NodeType.Call, -1) };
            filterExclude = new FnViewFilter[] { new FnViewFilter(TreeNode.NodeType.Call, -1), new FnViewFilter(TreeNode.NodeType.Call, -1) };

            fShowSubtree = false;
            firstNewStack = -1;

            GetConfiguration();

            InitForm( true );
        }

        private void InitForm( bool fFirstTime )
        {
            Controls.Clear();
            if (controlCollection != null)
            {
                controlCollection.Controls.Clear();
            }
            InitializeComponent();
            if(!forCompare)
            {
                /* extract font for the call tree */
                defaultFont = MainForm.instance.font;
                stackView.Font = defaultFont;
                menuItemShowFuture.Checked = fShowSubtree;

                ResizeForm();
            }
            

            /* intialize backing store */
            backingStoreFileName = Path.GetTempFileName();
            backingStore = new FileStream(backingStoreFileName, FileMode.Truncate, FileAccess.ReadWrite, FileShare.ReadWrite);
            writer = new BitWriter(backingStore);
            pos = 0;

            threads = new Dictionary<int, ThreadState>();
            globalCallStats = new GlobalCallStats[1 + names.Length];
            globalAllocStats = new GlobalAllocStats[1 + types.Length];

            assemblyNames = new ArrayList();
            assemblyNameToIdMap = new Hashtable();

            /* compute everything about the run */
            if(!BuildTreeAndComputeStats())
            {
                Dispose();
                return;
            }

            backingStore.Position = 0;
            reader = new BitReader(backingStore);

            manyThreads = (threads.Count > 5);

            if(manyThreads)
            {
                threadIDLabel.Location = new System.Drawing.Point(8, 12);
                threadIDLabel.Name = "threadIDLabel";
                threadIDLabel.Size = new System.Drawing.Size(64, 16);
                threadIDLabel.TabIndex = 2;
                threadIDLabel.Text = "Thread ID";

                threadIDList.Location = new System.Drawing.Point(72, 10);
                threadIDList.Name = "threadIDList";
                threadIDList.TabIndex = 0;
                threadIDList.SelectedValueChanged += new EventHandler(threadIDList_SelectedValueChanged);

                Controls.Add(threadIDList);
                Controls.Add(threadIDLabel);
            }
            else
            {
                tabs = new TabControl();
                tabs.Dock = System.Windows.Forms.DockStyle.Left;
                tabs.Font = defaultFont;
                tabs.Size = new System.Drawing.Size(332, 108);
                tabs.TabIndex = 0;
                tabs.SelectedIndexChanged += new EventHandler(TabsSelectedIndexChanged);

                controlCollection.Controls.Add(tabs);
            }

            foreach(int e in threads.Keys)
            {
                TabPage page = new TabPage(e.ToString());
                page.BorderStyle = BorderStyle.None;

                TreeListView treeView = new TreeListView(this);
                treeView.Dock = DockStyle.Fill;
                treeView.Font = defaultFont;

                /* initial sorting and highlighting behaviour:
                    * 1) sort in order of execution,
                    * 2) highlight the one that allocated the most */
                SortingBehaviour sort = new SortingBehaviour();
                SortingBehaviour highlight = new SortingBehaviour();
                sort.sortingOrder = highlight.sortingOrder = -1;
                sort.counterId = -1;
                highlight.counterId = 2;

                /* add columns */
                treeView.AddColumn(new ColumnInformation(-1, "Function name", ColumnInformation.ColumnTypes.Tree), 250);
                foreach(int counter in Statistics.DefaultCounters)
                {
                    AddColumn(treeView, counter);
                }

                treeView.ColumnClick += new EventHandler(SortOn);
                treeView.SelectedIndexChanged += new EventHandler(ShowCurrentStack);

                treeView.TokenObject = new ViewState(sort, highlight);
                treeView.Root = (TreeNode)((ThreadState)threads[e]).stack[0];

                if(manyThreads)
                {
                    threadIDList.Items.Add(e);
                    ((ThreadState)threads[e]).callTreeView = treeView;
                    treeView.Visible = false;
                    treeView.Size = new System.Drawing.Size(332, 108);
                    controlCollection.Controls.Add(treeView);
                }
                else
                {
                    page.Controls.Add(treeView);
                    tabs.Controls.Add(page);
                    if(e == firstThread)
                    {
                        tabs.SelectedTab = null;
                        tabs.SelectedTab = page;
                    }
                }
            }
        

            if(manyThreads)
            {
                currentThreadId = -1;
                threadIDList.SelectedItem = firstThread;
            }
            
            CallTreeForm_Resize(null, null);
            
            
            if (splitterX != -1)
            {
                splitter.SplitPosition = splitterX;
            }

            previousSplitterLocation = splitter.Location.X;
            if(!forCompare)
            {
                Visible = true;
            

                // Create a blank context menu for the stackview control.  We'll fill it in when the user right clicks
                ContextMenu contextMenu = new ContextMenu();
                stackView.ContextMenu = contextMenu;
            }
            
        }

        private void SetcallTreeView()
        {
            if (tabs.SelectedTab == null)
                return;

            foreach(Control c in tabs.SelectedTab.Controls)
            {
                TreeListView v = null;
                try
                {
                    v = (TreeListView)c;
                }
                catch
                {
                    /* not interested in exceptions */
                }

                if(v != null)
                {
                    callTreeView = v;
                    viewState = (ViewState)v.TokenObject;
                    ShowCurrentStack(null, null);
                    return;
                }
            }
            Debug.Fail("Cannot find tree view on the tab page");
        }
        /* show the stack and/or some other info */
        private void ShowCurrentStack(object obj, EventArgs args)
        {
            if(callTreeView == null)
            {
                SetcallTreeView();
            }
            TreeNode node = (TreeNode)callTreeView.SelectedItem;
            if(node == null)
            {
                return;
            }

            int stackid = node.stackid;
            if(stackid < 0)
            {
                return;
            }

            stackView.ListViewItemSorter = null;
            stackView.Items.Clear();

            int[] stacktrace = IndexToStacktrace(stackid);

            /* show stack trace */
            subtreeline = -1;
            for(int i = (node.nodetype == TreeNode.NodeType.Allocation ? 2 : 0); i < stacktrace.Length; i++)
            {
                int functionId = stacktrace[i];
                GlobalCallStats s = globalCallStats[functionId];

                string[] subitems = new string[]
                {
                    names[functionId],
                    s.timesCalled.ToString(),
                    s.totalBytesAllocated.ToString(),
                    s.totalFunctionsCalled.ToString(),
                    s.totalNewFunctionsBroughtIn.ToString()
                };
                stackView.Items.Add(new ListViewItem(subitems));
            }

            /* build and show child subtree */
            if (node.nodetype == TreeNode.NodeType.Call && fShowSubtree)
            {
                SortedList fns = new SortedList();
                GetChildren( node, fns );

                if (fns.Count > 0 )
                {
                    //  Add a separator
                    string functionName;

                    if (stacktrace == null || stacktrace.Length == 0)
                    {
                        functionName = "All";
                    }
                    else
                    {
                        functionName = names[stacktrace[stacktrace.Length - 1]];
                    }

                    subtreeline = stackView.Items.Count;
                    stackView.Items.Add( "<-------- Subtree of " + functionName + " --------> " );
                }

                IDictionaryEnumerator enumFns = fns.GetEnumerator();

                while(enumFns.MoveNext())
                {
                    GlobalCallStats s = (GlobalCallStats)enumFns.Value;
                    int fid = (int)enumFns.Key;
                    string[] subitems = new string[]
                    {
                        fid > 0 ? names[fid] : types[-fid],
                        s.timesCalled.ToString(),
                        s.totalBytesAllocated.ToString(),
                        s.totalFunctionsCalled.ToString(),
                        s.totalNewFunctionsBroughtIn.ToString()
                    };

                    ListViewItem item = new ListViewItem(subitems);
                    if (fid < 0)
                    {
                        //  Allocation item
                        item.ForeColor = Color.Green;
                    }

                    stackView.Items.Add(item);
                }
            }

            /* and optionally some global info about the allocated object */
            if(node.nodetype == TreeNode.NodeType.Allocation)
            {
                int typeId = stacktrace[0];
                GlobalAllocStats s = globalAllocStats[typeId];
                string[] subitems = new string[]
                {
                    types[typeId],  // "Name"
                    s.timesAllocated.ToString(),
                    s.totalBytesAllocated.ToString()
                };
                ListViewItem item = new ListViewItem(subitems);
                item.ForeColor = Color.Green;
                stackView.Items.Add(item);
            }

            /* [re]set the columns if necessary */
            if(stackView.Columns.Count != 5 && node.nodetype != TreeNode.NodeType.Allocation)
            {
                stackView.Columns.Clear();
                stackView.Columns.Add("Name", 150, HorizontalAlignment.Left);
                stackView.Columns.Add("Times Called", 60, HorizontalAlignment.Left);
                stackView.Columns.Add("Bytes Allocated", 60, HorizontalAlignment.Left);
                stackView.Columns.Add("Functions Called", 60, HorizontalAlignment.Left);
                stackView.Columns.Add("New Functions", 60, HorizontalAlignment.Left);
            }
            if(stackView.Columns.Count != 4 && node.nodetype == TreeNode.NodeType.Allocation)
            {
                stackView.Columns.Clear();
                stackView.Columns.Add("Name", 150, HorizontalAlignment.Left);
                stackView.Columns.Add("Times Called/Allocated", 100, HorizontalAlignment.Left);
                stackView.Columns.Add("Bytes", 60, HorizontalAlignment.Left);
            }
        }

        private bool GetChildren( TreeNode node, SortedList fns )
        {
            int[] rootStacktrace = IndexToStacktrace(node.stackid);
            int rootStackLength = rootStacktrace.Length;
            int prevStackLength = rootStackLength;
            int [] kidStacktrace;
            int functionId = 0;

            try
            {
                ArrayList kids = FetchKids( null, node );
                foreach( TreeNode kidNode in kids )
                {
                    GlobalCallStats s;
                    bool fAddNode = false;

                    kidStacktrace = IndexToStacktrace( kidNode.stackid );
                    if (kidNode.nodetype == TreeNode.NodeType.Call)
                    {
                        functionId = kidStacktrace[ kidStacktrace.Length - 1 ];
                        fAddNode = true;
                    }
                    else if (kidNode.nodetype == TreeNode.NodeType.Allocation)
                    {
                        functionId = -kidStacktrace[ 0 ];
                        fAddNode = true;
                    }

                    if (fAddNode)
                    {
                        if ( !fns.ContainsKey( functionId ))
                        {
                            s = new GlobalCallStats();
                            s.timesCalled = 1;
                            s.totalBytesAllocated = (int)kidNode.data.bytesAllocated;
                            s.totalFunctionsCalled = (int)kidNode.data.numberOfFunctionsCalled;
                            s.totalNewFunctionsBroughtIn = (int)kidNode.data.numberOfNewFunctionsBroughtIn;
                        }
                        else
                        {
                            s = (GlobalCallStats)fns[functionId];
                            s.timesCalled++;
                            s.totalBytesAllocated += (int)kidNode.data.bytesAllocated;
                            s.totalFunctionsCalled += (int)kidNode.data.numberOfFunctionsCalled;
                            s.totalNewFunctionsBroughtIn += (int)kidNode.data.numberOfNewFunctionsBroughtIn;
                        }

                        GetChildren( kidNode, fns );
                        fns[functionId] = s;
                    }
                }               
            }
            catch
            {
                /* exceptions are no good */
                MessageBox.Show(this, "Error getting subtree", "Failure");
                return false;
            }

            return true;
        }

        public CallTreeForm.FnViewFilter[] GetIncludeFilters()
        {
            return filterInclude;
        }

        public CallTreeForm.FnViewFilter[] GetExcludeFilters()
        {
            return filterExclude;
        }

        public void SetIncludeFilters( CallTreeForm.FnViewFilter[] filters)
        {
            filterInclude = filters;            
        }

        public void SetExcludeFilters( CallTreeForm.FnViewFilter[] filters)
        {
            filterExclude = filters;
        }

        internal int GetMaxFnId()
        {
            return names.Length - 1;
        }

        private int GetFunctionId( string functionName )
        {
            int i;

            for( i = 0; i < names.Length; i++ )
            {
                if (names[i] == functionName )
                {
                    return i;
                }
            }
            
            // Function not found
            return -1;

        }

        private int GetTypeId( string typeName )
        {
            int i;

            for( i = 0; i < types.Length; i++ )
            {
                if (types[i] == typeName )
                {
                    return i;
                }
            }
            
            // Type not found
            return -1;
        }

        /* construct a human-readable name for a function call */
        public string MakeNameForFunction(int functionId)
        {
            if(functionId < 0)
            {
                return "STUB FUNCTION (for calls before profiling was enabled)";
            }

            string res = "";
            string name = names[functionId];
            string signature = signatures[functionId];
            if (name == null)
            {
                return null;
            }

            if(name == "NATIVE")
            {
                /* transition into the unmanaged code */
                return name + ' ' + signature;
            }
            else
            {
                /* something we can actually make sense of */
                int argumentsStart = signature.IndexOf('(');
                if(argumentsStart != -1)
                {
                    /* parse (beautify) the strings */
                    string arguments = signature.Substring(argumentsStart).Trim();
                    string[] argv = arguments.Split(" ".ToCharArray());

                    res = signature.Substring(0, argumentsStart).Trim() + ' ' + name.Trim();
                    for(int i = 0; i < argv.Length; i++)
                    {
                        if(i != 0)
                        {
                            res += ", ";
                        }
                        res += argv[i];
                    }
                }
                else
                {
                    res = name + ' ' + signature;
                }
            }

            return res;
        }

        public string MakeNameForAllocation( int typeId, int bytes)
        {
            string res;

            if (types[typeId] == null)
                return null;

            if (bytes == 0)
            {
                res = types[typeId];
            }
            else
            {
                res = string.Format("{0} ({1} bytes)", types[typeId], bytes);
            }
            return res;
        }

        /* make a name for either call or allocation */
        internal string MakeName(TreeNode n)
        {
            if(n.nodetype == TreeNode.NodeType.AssemblyLoad)
            {
                return "[assembly] " + (string)assemblyNames[n.nameId];
            }

            /* other special cases */
            if(n.stackid == 0)
            {
                return "NATIVE FUNCTION (UNKNOWN ARGUMENTS)";
            }
            else if(n.stackid < 0)
            {
                return "STUB FUNCTION (for calls before profiling was enabled)";
            }

            int[] stacktrace = IndexToStacktrace(n.stackid);

            string res = null;
            switch(n.nodetype)
            {
                case TreeNode.NodeType.Allocation:
                    res = MakeNameForAllocation( stacktrace[0], stacktrace[1]);
                    break;

                case TreeNode.NodeType.Call:
                    int functionId = stacktrace[stacktrace.Length - 1];
                    res = MakeNameForFunction(functionId);
                    break;
            }
            return res;
        }

        /*  Get the id of the selected allocation or call node */
        public int GetNodeId( TreeNodeBase node )
        {
            TreeNode n = (TreeNode)node;
            int id = -1;

            if(n.nodetype == TreeNode.NodeType.AssemblyLoad)
            {
                return id;
            }

            /* other special cases */
            if(n.stackid <= 0)
            {
                return id;
            }

            int[] stacktrace = IndexToStacktrace(n.stackid);

            switch(n.nodetype)
            {
                case TreeNode.NodeType.Allocation:
                    id = stacktrace[0];
                    break;

                case TreeNode.NodeType.Call:
                    id = stacktrace[stacktrace.Length - 1];
                    break;
            }
            return id;
        }

        /* get the current function given the stack trace */
        internal int GetFunctionIdFromStackId(int stackid)
        {
            if(stackid == -1)
            {
                return -1;
            }
            int[] stacktrace = IndexToStacktrace(stackid);
            return stacktrace[stacktrace.Length - 1];
        }

        /* record call to a function */
        void EnterFunction(SortedList functions, int functionId)
        {
            int index = functions.IndexOfKey(functionId);
            if(index == -1)
            {
                functions.Add(functionId, 1);
            }
            else
            {
                /* if in the list, add 1 to its counter (need to keep keys unique) */
                functions.SetByIndex(index, 1 + (int)functions.GetByIndex(index));
            }
        }

        /* record leaving of a function */
        void LeaveFunction(SortedList functions, int functionId)
        {
            int index = functions.IndexOfKey(functionId);
            if(index != -1)
            {
                int newValue = (int)functions.GetByIndex(index) - 1;
                if(newValue <= 0)
                {
                    functions.RemoveAt(index);
                }
                else
                {
                    functions.SetByIndex(index, newValue);
                }
            }
        }

        /* incorporate the information computed about the kid (k) into its parent (r) */
        void UpdateStats(object r, object k)
        {
            TreeNode root = (TreeNode)r;
            TreeNode kid = (TreeNode)k;

            root.data.bytesAllocated += kid.data.bytesAllocated;
            root.data.bytesAllocatedByKids += kid.data.bytesAllocated;

            root.data.numberOfFunctionsCalled += (kid.nodetype == TreeNode.NodeType.Call ? 1 : 0) + kid.data.numberOfFunctionsCalled;
            root.data.numberOfFunctionsCalledByKids += kid.data.numberOfFunctionsCalledByKids;

            root.data.numberOfUnmanagedTransitions += (kid.isunmanaged ? 1 : 0) + kid.data.numberOfUnmanagedTransitions;
            root.data.numberOfUnmanagedTransitionsByKids += kid.data.numberOfUnmanagedTransitions;

            root.data.numberOfAssembliesLoaded += (kid.nodetype == TreeNode.NodeType.AssemblyLoad ? 1 : 0) + kid.data.numberOfAssembliesLoaded;
            root.data.numberOfAssembliesLoadedByKids += kid.data.numberOfAssembliesLoaded;

            root.data.numberOfObjectsAllocated += (kid.nodetype == TreeNode.NodeType.Allocation ? 1 : 0) + kid.data.numberOfObjectsAllocated;
            root.data.numberOfObjectsAllocatedByKids += kid.data.numberOfObjectsAllocated;
        }

        /* read kids of a node from the backing store */
        public ArrayList FetchKids(object tokenObject, TreeNodeBase nodebase)
        {
            TreeNode node = (TreeNode)nodebase;
            ArrayList kids = new ArrayList();

            for(long offset = node.kidOffset; offset != -1; offset = node.prevOffset)
            {
                reader.Position = offset;
                node = new TreeNode(reader);
                node.HasKids = (node.kidOffset != -1);
                kids.Add(node);
            }

            return kids;
        }
        

        /* record node to the backing store and return its starting location */
        long Dump(object node)
        {
            long retValue = (long)writer.Position;
            ((TreeNode)node).Write(writer);
            return retValue;
        }
    
        
        /* compute the tree and all the counters and write all that to the backing store */
        bool BuildTreeAndComputeStats()
        {
            /* id of the previous thread */
            int prevThreadId = -1;

            /* information about the current thread (to speed up lookup) */
            int prevDepth = 0;
            ArrayList stack = null;
            SortedList functions = null;
            ArrayList queuedNodes = null;

            Stream s = null;
            ProgressForm progressForm = null;

            const int prevStackInitialSize = 100;
            int prevStackMaxSize = prevStackInitialSize;
            int[] prevStackTrace = new int[prevStackInitialSize]; 
            int prevStackLen = 0;
            StacktraceTable stacktraceTable = logResult.callstackHistogram.readNewLog.stacktraceTable;

            //  Preprocessing for function filters
            int nIncludeCallFilters = 0;
            int nIncludeAllocFilters = 0;

            for (int j=0; j < filterInclude.Length; j++)
            {
                if (filterInclude[j].functionId != -1)
                {
                    if (filterInclude[j].nodetype == TreeNode.NodeType.Call)
                        nIncludeCallFilters++;
                    else if (filterInclude[j].nodetype == TreeNode.NodeType.Allocation)
                        nIncludeAllocFilters++;
                }
            }
 

            if (firstNewStack != -1)
            {
                stacktraceTable.FreeEntries( firstNewStack );
            }
            firstNewStack = -1;

            try
            {
                /* log parser code (straight from the ReadNewLog.cs) */
                s = new FileStream(logFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
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
                            Application.DoEvents();
                        }
                    }
                    lastLineStartPos = pos-1;

                    /* only parse calls and allocations */
                    switch (c)
                    {
                        case -1:
                            break;

                            // 'A' with thread identifier
                        case '!':
                        {
                            found = true;
                            c = ReadChar();
                            threadid = ReadInt();
                            ReadInt();
                            stackid = ReadInt();
                            nodetype = TreeNode.NodeType.Allocation;
                            if (c == -1)    {found = false;}
                            break;
                        }

                        case 'C':
                        case 'c':
                        {
                            found = true;
                            c = ReadChar();
                            nodetype = TreeNode.NodeType.Call;
                            threadid = ReadInt();
                            stackid = ReadInt();
                            if (c == -1)    {found = false;}
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

                    int[] stacktrace = IndexToStacktrace(stackid);
                    bool fFoundCallInclude = true;
                    bool fFoundCallExclude = false;
                    bool fFoundAllocInclude = true;
                    bool fFoundAllocExclude = false;

                    int includeMatches;

                    //  Apply filters to current node
                    if (filterInclude.Length != 0 || filterExclude.Length != 0)
                    {
                        if (nodetype == TreeNode.NodeType.Call ||
                            nodetype == TreeNode.NodeType.Allocation)
                        {
                            int i = 0;
                            int includesFound = 0;
                            fFoundCallInclude = false;

                            if (nodetype == TreeNode.NodeType.Allocation)
                            {
                                i = 2;
                            }

                            for (; i < stacktrace.Length; i++)
                            {
                                includeMatches = 0;

                                //  See if the stack contain the required include functions
                                for (int j=0; j < filterInclude.Length; j++)
                                {
                                    if (filterInclude[j].nodetype == TreeNode.NodeType.Call && filterInclude[j].functionId != -1)
                                    {
                                        if (stacktrace[i] == filterInclude[j].functionId )
                                            includeMatches++;
                                    }

                                }

                                if (includeMatches > 0)
                                    includesFound++;


                                //  Now see if the stack contains any exclude functions
                                for (int j=0; j < filterExclude.Length; j++)
                                {
                                    if (filterExclude[j].nodetype == TreeNode.NodeType.Call &&
                                        stacktrace[i] == filterExclude[j].functionId )
                                    {
                                        //  Any exclusion match gets this node bounced
                                        fFoundCallExclude = true;
                                        break;
                                    }
                                }
                            }

                            //  This node can pass the filter only if all include filters are found
                            if (includesFound == nIncludeCallFilters)
                                fFoundCallInclude = true;
                        }
                        else 
                        {
                            fFoundCallInclude = false;
                        }
                                                
                        if (nodetype == TreeNode.NodeType.Allocation)
                        {
                            includeMatches = 0;
                            fFoundAllocInclude = false;

                            //  See if the stack contain the required include allocations
                            for (int j=0; j < filterInclude.Length; j++)
                                if (filterInclude[j].nodetype == TreeNode.NodeType.Allocation)
                                {
                                    if (stacktrace[0] == filterInclude[j].functionId || filterInclude[j].functionId == -1)
                                        includeMatches++;
                                }

                            if (includeMatches > 0 || nIncludeAllocFilters == 0)
                                fFoundAllocInclude = true;

                            //  Now see if the stack contains any exclude allocations
                            for (int j=0; j < filterExclude.Length; j++)
                            {
                                if (filterExclude[j].nodetype == TreeNode.NodeType.Allocation &&
                                    stacktrace[0] == filterExclude[j].functionId)
                                {
                                    fFoundAllocExclude = true;
                                    break;
                                }
                            }
                        }
                    }

                    //  Proceed to process this node only if the trace has all functions 
                    //  and no exclude functions.
                    if ( !fFoundCallInclude || fFoundCallExclude ||
                        !fFoundAllocInclude || fFoundAllocExclude)
                    {
                        //  Skip this node
                        continue;
                    }

                    /* if thread changed, have to retrieve information about it.
                     * info about the last thread is cached to speed up the process */
                    if(threadid != prevThreadId)
                    {
                        if(prevThreadId != -1)
                        {
                            /* store everything about the previous thread */
                            ThreadState prevState = threads[prevThreadId];
                            prevState.prevStackLen = prevStackLen;
                            prevState.prevStackTrace = prevStackTrace;
                            prevState.functions = functions;
                            prevState.prevDepth = prevDepth;
                            prevState.stack = stack;
                            prevState.queuedNodes = queuedNodes;
                        }
                        else
                        {
                            /* this is the first call ever, mark the
                             * thread where it occured as the main thread */
                            firstThread = threadid;
                        }

                        /* get the information about the current (new) thread */
                        ThreadState state;
                        if (!threads.TryGetValue(threadid, out state))
                        {
                            /* create if necessary */
                            state = new ThreadState();
                            state.prevStackLen = 0;
                            state.prevStackTrace = new int[prevStackInitialSize];
                            state.prevDepth = 0;
                            state.stack = new ArrayList();
                            state.functions = new SortedList();
                            state.queuedNodes = new ArrayList();

                            TreeNode threadRoot = new TreeNode(TreeNode.NodeType.Call, 0);
                            state.stack.Add(threadRoot);

                            threads[threadid] = state;
                        }

                        prevStackLen = state.prevStackLen;
                        prevStackTrace = state.prevStackTrace;
                        prevStackMaxSize = prevStackTrace.Length;

                        prevDepth = state.prevDepth;
                        stack = state.stack;
                        functions = state.functions;
                        queuedNodes = state.queuedNodes;
                        prevThreadId = threadid;
                    }

                    /* if we're here, `iscall`, `stackid`, and `threadid` are set correctly */
                    int depth = 0;

                    if ( nodetype == TreeNode.NodeType.Allocation )
                    {
                        //  To build a call tree from the allocation log 
                        //  Need to recreate the stacks and Call nodes that got us here.

                        //  Ignore first 2 ints in the stack trace.  They are allocation information.
                        int curStackLen = stacktrace.Length - 2;
                        int i;
                        bool fNewStack;

                        //  Find out how much of the callstack we need to construct
                        fNewStack = curStackLen != prevStackLen;
                        for (i = 0; i < curStackLen && i < prevStackLen; i++)
                        {
                            if (prevStackTrace[i] != stacktrace[i+2]) 
                            {
                                fNewStack = true;
                                break;
                            }
                        }

                        int nextStackIndex = stacktraceTable.Length;
                        for ( ; i < curStackLen; i++)
                        {
                            // We blindly add a new entry to the stack table, even though this stack may already
                            // exist.   Searching for a match would be expensive, so for now just do a new allocation.
                            // If this becomes hugely expensive, it will be worth making the stack trace searchable.
                            stacktraceTable.Add( nextStackIndex, stacktrace, 2, i+1, false);
                            TreeNode callnode = new TreeNode(TreeNode.NodeType.Call, nextStackIndex);
                            callnode.nodeOffset = lastLineStartPos;
                            queuedNodes.Add( callnode );

                            if (firstNewStack == -1)
                            {
                                //  Remember which stacks we created
                                firstNewStack = nextStackIndex;
                            }

                            nextStackIndex++;
                        }

                        if (fNewStack)
                        {
                            //  Reallocate prev stack if neccessary
                            if (curStackLen > prevStackMaxSize)
                            {
                                prevStackMaxSize += prevStackInitialSize;
                                prevStackTrace = new int[prevStackMaxSize];
                            }

                            //  Save a copy of the current stack
                            for (i = 0; i < curStackLen; i++)
                            {

                                prevStackTrace[i] = stacktrace[i+2];
                            }
                            prevStackLen = curStackLen;
                        }
                    }
                    else if ( nodetype == TreeNode.NodeType.Call )
                    {
                        //  Reallocate prev stack if neccessary
                        if (stacktrace.Length > prevStackMaxSize)
                        {
                            prevStackMaxSize += prevStackInitialSize;
                            prevStackTrace = new int[prevStackMaxSize];
                        }
                        prevStackLen = stacktrace.Length;
                        stacktrace.CopyTo( prevStackTrace, 0 );
                    }

                    TreeNode node = new TreeNode(nodetype, stackid);
                    int functionId = (nodetype != TreeNode.NodeType.AssemblyLoad ? stacktrace[stacktrace.Length - 1] : 0);
                    
                    
                    switch(nodetype)
                    {
                        case TreeNode.NodeType.Allocation:
                            node.data.bytesAllocated = stacktrace[1];
                            break;

                        case TreeNode.NodeType.Call:
                            if(functionId == 0)
                            {
                                node.isunmanaged = true;
                            }
                            break;

                        case TreeNode.NodeType.AssemblyLoad:
                            if(!assemblyNameToIdMap.Contains(assemblyName))
                            {
                                assemblyNameToIdMap.Add(assemblyName, null);
                                node.nameId = assemblyNames.Add(assemblyName);

                                queuedNodes.Add(node);
                            }
                            continue;
                    }

                    queuedNodes.Add(node);
                    for(int i = 0; i < queuedNodes.Count; i++)
                    {
                        node = (TreeNode)queuedNodes[i];
                        nodetype = node.nodetype;
                        stacktrace = IndexToStacktrace(node.stackid);
                        int stackLength = stacktrace.Length;

                        if(nodetype == TreeNode.NodeType.Allocation)
                        {
                            //  Skip first 2 entries in the stack.  They are type-id and bytes-allocated.
                            //  Add 1 to depth so allocation looks like a call to allocation function.
                            depth = stackLength - 2 + 1;
                        }
                        else if(nodetype == TreeNode.NodeType.Call)
                        {
                            depth = stackLength;
                        }
                        else if(nodetype == TreeNode.NodeType.AssemblyLoad)
                        {
                            depth = stackLength;
                        }
                        if(depth <= 0)
                        {
                            continue;
                        }

                        if(depth > prevDepth)
                        {
                            /* kids go to the stack */
                            if(depth - prevDepth > 1)
                            {
                                for(int idx = 1; idx < depth; idx++)
                                {
                                    TreeNode n = new TreeNode(TreeNode.NodeType.Call, -idx);
                                    n.nodeOffset = lastLineStartPos;
                                    stack.Add(n);
                                }
                            }
                            stack.Add(node);
                        }
                        else
                        {
                            /* moving up or sideways, have to adjust the stats
                            * and dump some of the branches to the backing store */
                            for(int j = 1 + prevDepth; j-- > depth + 1;)
                            {
                                if(((TreeNode)stack[j]).nodetype == TreeNode.NodeType.Call)
                                {
                                    /* record functions left */
                                    LeaveFunction(functions, GetFunctionIdFromStackId(((TreeNode)stack[j]).stackid));
                                }
                                UpdateStats(stack[j - 1], stack[j]);
                                ((TreeNode)stack[j - 1]).kidOffset = Dump(stack[j]);
                            }
                            if(((TreeNode)stack[depth]).nodetype == TreeNode.NodeType.Call)
                            {
                                LeaveFunction(functions, GetFunctionIdFromStackId(((TreeNode)stack[depth]).stackid));
                            }
                            UpdateStats(stack[depth - 1], stack[depth]);
                            node.prevOffset = Dump(stack[depth]);
                            stack[depth] = node;
                            stack.RemoveRange(1 + depth, stack.Count - depth - 1);
                        }

                        /* adjust the global statistics */
                        if(nodetype == TreeNode.NodeType.Call)
                        {
                            functionId = stacktrace[stacktrace.Length - 1];
                            globalCallStats[functionId].timesCalled++;
                            foreach(int fid in functions.Keys)
                            {
                                globalCallStats[fid].totalFunctionsCalled++;
                            }

                            if(!globalCallStats[functionId].calledAlready)
                            {
                                globalCallStats[functionId].calledAlready = true;
                                node.data.firstTimeBroughtIn = true;
                                foreach(TreeNode n in stack)
                                {
                                    n.data.numberOfNewFunctionsBroughtIn++;
                                }
                                /* `node` (the new function itself) is on the
                                * stack too, so we have to reverse its counter */
                                node.data.numberOfNewFunctionsBroughtIn--;

                                foreach(int fid in functions.Keys)
                                {
                                    globalCallStats[fid].totalNewFunctionsBroughtIn++;
                                }
                            }

                            /* record entering the function */
                            EnterFunction(functions, functionId);
                        }
                        else if(nodetype == TreeNode.NodeType.Allocation)
                        {
                            foreach(int fid in functions.Keys)
                            {
                                globalCallStats[fid].totalBytesAllocated += stacktrace[1];
                            }
                            globalAllocStats[stacktrace[0]].timesAllocated++;
                            globalAllocStats[stacktrace[0]].totalBytesAllocated += stacktrace[1];
                        }

                        prevDepth = depth;
                    }
                    queuedNodes.Clear();
                }
            }
            catch
            {
                /* exceptions are no good */
                MessageBox.Show(this, "Error creating backing store file, check if there is\nenough space on drive that holds your TEMP directory", "Failure");
                return false;
            }
            finally
            {
                /* get rid of the progress form and close the log file */
                if(progressForm != null)
                {
                    progressForm.Visible = false;
                    progressForm.Dispose();
                }
                if(s != null)
                {
                    s.Close();
                }
            }

            /* dump the root and the remains of the tree
             * that are still in memory to the backing store */
            foreach(ThreadState state in threads.Values)
            {
                stack = state.stack;
                for(int j = stack.Count; j-- > 1;)
                {
                    LeaveFunction(functions, GetFunctionIdFromStackId(((TreeNode)stack[j]).stackid));
                    UpdateStats(stack[j - 1], stack[j]);
                    ((TreeNode)stack[j - 1]).kidOffset = Dump(stack[j]);
                }
                ((TreeNode)stack[0]).HasKids = true;
                stack.RemoveRange(1, stack.Count - 1);
            }

            /* remove spurious threads from the thread array. don't think
             * it's an issue anymore but the code doesn't do anybody no harm */
            List<int> nulls = new List<int>();
            foreach(int key in threads.Keys)
            {
                if (threads[key] == null)
                {
                    nulls.Add(key);
                }
            }
            foreach(int key in nulls)
            {
                threads.Remove(key);
            }

            writer.Flush();
            return true;
        }

        private void GetConfiguration()
        {
            RegistryKey rkMsft;

            //  Open or create the CLR Profiler registry key
            rkMsft = Registry.CurrentUser.OpenSubKey( "Software\\Microsoft", true);
            rkProfiler = rkMsft.OpenSubKey( "CLRProfiler", true );
            if (rkProfiler == null)
            {
                rkProfiler = rkMsft.CreateSubKey( "CLRProfiler" );
                if (rkProfiler == null)
                {
                    MessageBox.Show( this, "Unable to open regkey " + rkMsft.ToString() + "\\CLRProfiler"  );
                }
            }

            rkMsft.Close();


            try 
            {
                String value;
                string svalue;
                int o1, o2;

                //  Read the saved rectangle from the registry and restore
                //  the app to that size.
                value = (String)rkProfiler.GetValue( "Rectangle" );
                if (value != null)
                {
                    int left, top, right, bottom;

                    o1 = 0;
                    o2 = value.IndexOf(",", o1);
                    svalue = value.Substring( o1, o2-o1 );
                    left = Int32.Parse( svalue );

                    o1 = o2 + 1;
                    o2 = value.IndexOf(",", o1);
                    svalue = value.Substring( o1, o2-o1 );
                    top = Int32.Parse( svalue );

                    o1 = o2 + 1;
                    o2 = value.IndexOf(",", o1);
                    svalue = value.Substring( o1, o2-o1 );
                    right = Int32.Parse( svalue );
                
                    o1 = o2 + 1;
                    svalue = value.Substring( o1 );
                    bottom = Int32.Parse( svalue );

                    formRect = new Rectangle( left, top, right - left, bottom - top );
                }

                value = (String)rkProfiler.GetValue( "SplitterX" );
                if (value != null)
                {
                    splitterX = Int32.Parse( value );
                }
            }
            catch (Exception )
            {
                //  No error.  This is expected on the first run of this app on a new machine.
            }
        }

        private void ResizeForm()
        {
            this.Location = formRect.Location;
            this.Size = formRect.Size;
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
            this.threadIDLabel = new System.Windows.Forms.Label();
            this.threadIDList = new System.Windows.Forms.ComboBox();
            this.allFunctionsButton = new System.Windows.Forms.Button();
            this.allAllocationsButton = new System.Windows.Forms.Button();
            this.controlCollection = new System.Windows.Forms.Panel();
            this.stackView = new System.Windows.Forms.ListView();
            this.splitter = new System.Windows.Forms.Splitter();
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.menuItem4 = new System.Windows.Forms.MenuItem();
            this.menuItem5 = new System.Windows.Forms.MenuItem();
            this.menuItem6 = new System.Windows.Forms.MenuItem();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.selectColumns = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.menuItem7 = new System.Windows.Forms.MenuItem();
            this.menuItemShowFuture = new System.Windows.Forms.MenuItem();
            this.menuItemCopyStack = new System.Windows.Forms.MenuItem();
            this.controlCollection.SuspendLayout();
            this.SuspendLayout();
            // 
            // threadIDLabel
            // 
            this.threadIDLabel.Location = new System.Drawing.Point(0, 0);
            this.threadIDLabel.Name = "threadIDLabel";
            this.threadIDLabel.TabIndex = 0;
            // 
            // threadIDList
            // 
            this.threadIDList.Location = new System.Drawing.Point(0, 0);
            this.threadIDList.Name = "threadIDList";
            this.threadIDList.Size = new System.Drawing.Size(121, 21);
            this.threadIDList.TabIndex = 0;
            // 
            // allFunctionsButton
            // 
            this.allFunctionsButton.Location = new System.Drawing.Point(200, 8);
            this.allFunctionsButton.Name = "allFunctionsButton";
            this.allFunctionsButton.Size = new System.Drawing.Size(136, 21);
            this.allFunctionsButton.TabIndex = 3;
            this.allFunctionsButton.Text = "Display All Functions...";
            // 
            // allAllocationsButton
            // 
            this.allAllocationsButton.Location = new System.Drawing.Point(352, 8);
            this.allAllocationsButton.Name = "allAllocationsButton";
            this.allAllocationsButton.Size = new System.Drawing.Size(136, 21);
            this.allAllocationsButton.TabIndex = 4;
            this.allAllocationsButton.Text = "Display All Allocations...";
            // 
            // controlCollection
            // 
            this.controlCollection.Controls.Add(this.stackView);
            this.controlCollection.Controls.Add(this.splitter);
            this.controlCollection.Location = new System.Drawing.Point(16, 40);
            this.controlCollection.Name = "controlCollection";
            this.controlCollection.Size = new System.Drawing.Size(400, 288);
            this.controlCollection.TabIndex = 5;
            // 
            // stackView
            // 
            this.stackView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.stackView.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(204)));
            this.stackView.FullRowSelect = true;
            this.stackView.GridLines = true;
            this.stackView.Location = new System.Drawing.Point(4, 0);
            this.stackView.Name = "stackView";
            this.stackView.Size = new System.Drawing.Size(396, 288);
            this.stackView.TabIndex = 1;
            this.stackView.View = System.Windows.Forms.View.Details;
            this.stackView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.stackView_MouseDown);
            this.stackView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.stackView_ColumnClick);
            // 
            // splitter
            // 
            this.splitter.Location = new System.Drawing.Point(0, 0);
            this.splitter.Name = "splitter";
            this.splitter.Size = new System.Drawing.Size(4, 288);
            this.splitter.TabIndex = 1;
            this.splitter.TabStop = false;
            this.splitter.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitter_SplitterMoved);
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                      this.menuItem4,
                                                                                      this.menuItem1});
            // 
            // menuItem4
            // 
            this.menuItem4.Index = 0;
            this.menuItem4.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                      this.menuItem5,
                                                                                      this.menuItem6});
            this.menuItem4.Text = "&View";
            // 
            // menuItem5
            // 
            this.menuItem5.Index = 0;
            this.menuItem5.Text = "All &functions";
            this.menuItem5.Click += new System.EventHandler(this.menuItem5_Click);
            // 
            // menuItem6
            // 
            this.menuItem6.Index = 1;
            this.menuItem6.Text = "All &objects";
            this.menuItem6.Click += new System.EventHandler(this.menuItem6_Click);
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 1;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                      this.selectColumns,
                                                                                      this.menuItem2,
                                                                                      this.menuItem3,
                                                                                      this.menuItem7,
                                                                                      this.menuItemShowFuture,
                                                                                      this.menuItemCopyStack});
            this.menuItem1.Text = "&Options";
            // 
            // selectColumns
            // 
            this.selectColumns.Index = 0;
            this.selectColumns.Text = "Select &columns...";
            this.selectColumns.Click += new System.EventHandler(this.selectColumns_Click);
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 1;
            this.menuItem2.Text = "&Sort options...";
            this.menuItem2.Click += new System.EventHandler(this.menuItem2_Click);
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 2;
            this.menuItem3.Text = "&Filtering...";
            this.menuItem3.Click += new System.EventHandler(this.menuItem3_Click);
            // 
            // menuItem7
            // 
            this.menuItem7.Index = 3;
            this.menuItem7.Text = "Filter F&unctions...";
            this.menuItem7.Click += new System.EventHandler(this.menuItem7_Click);
            // 
            // menuItemShowFuture
            // 
            this.menuItemShowFuture.Index = 4;
            this.menuItemShowFuture.Text = "Show Su&btree in Stack Window";
            this.menuItemShowFuture.Click += new System.EventHandler(this.menuItem8_Click);

            // 
            // menuItemCopyStack
            // 
            this.menuItemCopyStack.Index = 5;
            this.menuItemCopyStack.Text = "Copy Stac&k View";
            this.menuItemCopyStack.Click += new System.EventHandler(this.menuItem9_Click);

            // 
            // CallTreeForm
            // 
            this.ClientSize = new System.Drawing.Size(632, 493);
            this.Controls.Add(this.controlCollection);
            this.Menu = this.mainMenu1;
            this.Name = "CallTreeForm";
            this.Text = "Call Tree View";
            this.Resize += new System.EventHandler(this.CallTreeForm_Resize);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.CallTreeForm_Closing);
            this.controlCollection.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        /* handler for the mouse click on the column header.
         * changes the current sorting order and behaviour */
        private void SortOn(object obj, EventArgs e)
        {
            ColumnInformation ci = ((Column)obj).ColumnInformation;
            if(viewState.sort.counterId == (int)ci.Token)
            {
                viewState.sort.sortingOrder *= -1;
            }
            else
            {
                viewState.sort.counterId = (int)ci.Token;
                viewState.sort.sortingOrder = (viewState.sort.counterId == -1 ? -1 : 1);
            }
            callTreeView.Resort();
        }

        /* implements IComparer that compares the nodes according to the current sorting order */
        public int Compare(object x, object y)
        {
            TreeNode a = (TreeNode)x;
            TreeNode b = (TreeNode)y;

            if(viewState.sort.counterId == -1)
            {
                // compare based on the invokation order
                return a.prevOffset.CompareTo(b.prevOffset);
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

        /* returns font used to display the item (part of the ITreeOwner interface) */
        public Font GetFont(object obj, TreeNodeBase in_node)
        {
            TreeNode node = (TreeNode)in_node;
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

        /* returns color used to display the item (part of the ITreeOwner interface) */
        public Color GetColor(object obj, TreeNodeBase root, bool positive)
        {
            TreeNode node = (TreeNode)root;
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

        /* returns data about the item for a given counter.
         * object's ToString() is used to display that data */
        private object GetInfo(object obj, TreeNodeBase node, int counterId)
        {
            long number = 0;
            TreeNode root = (TreeNode)node;
            if(counterId < 0)
            {
                return MakeName(root);
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

        /* sort nodes at the branch level */
        public ArrayList ProcessNodes(object obj, ArrayList nodes)
        {
            bool add = false;
            ArrayList nodesAtOneLevel = new ArrayList();
            foreach(TreeNode node in nodes)
            {
                switch(node.nodetype)
                {
                    case TreeNode.NodeType.Call:
                        add = true;
                        break;

                    case TreeNode.NodeType.Allocation:
                        add = viewState.showAllocs;
                        break;

                    case TreeNode.NodeType.AssemblyLoad:
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
            TreeNode currentBest = (TreeNode)nodesAtOneLevel[0];

            currentBest.highlighted = false;
            nodesToHighlight.Add(currentBest);
            for(int i = 1; i < nodesAtOneLevel.Count; i++)
            {
                TreeNode n = (TreeNode)nodesAtOneLevel[i];
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

            foreach(TreeNode n in nodesToHighlight)
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

        /* respond to resize event */
        private void CallTreeForm_Resize(object sender, System.EventArgs e)
        {
            controlCollection.SuspendLayout();
            controlCollection.Left = 0;
            controlCollection.Top = (manyThreads ? 36 : 0);
            controlCollection.Width = this.ClientSize.Width;
            controlCollection.Height = this.ClientSize.Height - controlCollection.Top;
            controlCollection.ResumeLayout();
        }

        private void CallTreeForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(backingStore != null)
            {
                backingStore.Close();
            }
            if(backingStoreFileName != null)
            {
                File.Delete(backingStoreFileName);
            }

            // Save configuration
            if (rkProfiler == null)
            {
                return;
            }

            try
            {
                string strValue;

                //  Save rectangle of the app
                strValue = Left.ToString() + "," + Top.ToString() + "," + Right.ToString() + "," + Bottom.ToString();
                rkProfiler.SetValue( "Rectangle", strValue );

                strValue = splitter.Location.X.ToString();
                rkProfiler.SetValue( "SplitterX", strValue );
            }
            catch( Exception ex)
            {
                MessageBox.Show(this, "Unable to save configuration to the registry.  " + ex.Message);
            }
            finally
            {
                rkProfiler.Close();
                rkProfiler = null;
            }
        }

        private void threadIDList_SelectedValueChanged(object sender, System.EventArgs e)
        {
            int selectedThread = (int)threadIDList.SelectedItem;
            if(selectedThread != currentThreadId)
            {
                TreeListView oldTreeView = callTreeView;
                if(oldTreeView != null)
                {
                    oldTreeView.Visible = false;
                    oldTreeView.Dock = DockStyle.None;
                }
                currentThreadId = selectedThread;

                callTreeView = threads[selectedThread].callTreeView;
                callTreeView.Dock = DockStyle.Left;
                if(oldTreeView != null)
                {
                    callTreeView.Size = oldTreeView.Size;
                }
                viewState = (ViewState)callTreeView.TokenObject;

                CallTreeForm_Resize(null, null);
                callTreeView.Visible = true;
                ShowCurrentStack(null, null);
            }
        }

        private void TabsSelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (tabs.SelectedTab == null)
                return;

            foreach(Control c in tabs.SelectedTab.Controls)
            {
                TreeListView v = null;
                try
                {
                    v = (TreeListView)c;
                }
                catch
                {
                    /* not interested in exceptions */
                }

                if(v != null)
                {
                    callTreeView = v;
                    viewState = (ViewState)v.TokenObject;
                    ShowCurrentStack(null, null);
                    return;
                }
            }
            Debug.Fail("Cannot find tree view on the tab page");
        }

        private void selectColumns_Click(object sender, System.EventArgs e)
        {
            SelectColumns f = new SelectColumns();

            /* 0 is "function name", it's irrelevant */
            ArrayList l = callTreeView.GetColumns();
            for(int i = 1; i < l.Count; i++)
            {
                f.Set((int)((Column)l[i]).ColumnInformation.Token);
            }

            DialogResult res = f.ShowDialog(this);
            if(res == DialogResult.OK)
            {
                for(int i = l.Count; i-- > 1;)
                {
                    ((Control)l[i]).Dispose();
                    l.RemoveAt(i);
                }

                ArrayList ids = f.GetCheckedColumns();
                ids.Sort();
                foreach(int id in ids)
                {
                    AddColumn(callTreeView, id);
                }
                callTreeView.Invalidate(true);
            }
        }

        private void menuItem2_Click(object sender, System.EventArgs e)
        {
            SortAndHighlightSelector s = new SortAndHighlightSelector(viewState.sort, viewState.highlight);
            DialogResult res = s.ShowDialog(this);
            if(res == DialogResult.OK)
            {
                SortingBehaviour ss = new SortingBehaviour();
                SortingBehaviour hh = new SortingBehaviour();
                s.GetSortResults(ss, hh);

                if(ss.sortingOrder != viewState.sort.sortingOrder || ss.counterId != viewState.sort.counterId ||
                    hh.sortingOrder != viewState.highlight.sortingOrder || hh.counterId != viewState.highlight.counterId)
                {
                    viewState.sort = ss;
                    viewState.highlight = hh;
                    callTreeView.Resort();
                }
            }
        }

        private void menuItem3_Click(object sender, System.EventArgs e)
        {
            ViewFilter vf = new ViewFilter(viewState.showCalls, viewState.showAllocs, viewState.showAssemblies);
            DialogResult res = vf.ShowDialog(this);
            if(res == DialogResult.OK)
            {
                if(   viewState.showCalls != vf.callsCheckbox.Checked
                    || viewState.showAllocs != vf.allocationsCheckbox.Checked
                    || viewState.showAssemblies != vf.assembliesCheckbox.Checked)
                {
                    viewState.showCalls = vf.callsCheckbox.Checked;
                    viewState.showAllocs = vf.allocationsCheckbox.Checked;
                    viewState.showAssemblies = vf.assembliesCheckbox.Checked;
                    callTreeView.Resort();
                }
            }
        }

        private void menuItem5_Click(object sender, System.EventArgs e)
        {
            ListViewer lv = new ListViewer();
            lv.Text = "Functions";

            lv.list.Columns.Clear();
            lv.list.Columns.Add("Name", 250, HorizontalAlignment.Left);
            lv.list.Columns.Add("Times Called", 90, HorizontalAlignment.Left);
            lv.list.Columns.Add("Bytes Allocated", 90, HorizontalAlignment.Left);
            lv.list.Columns.Add("Functions Called", 90, HorizontalAlignment.Left);
            lv.list.Columns.Add("Functions Introduced", 110, HorizontalAlignment.Left);

            for(int i = 1; i < names.Length; i++)
            {
                GlobalCallStats s = globalCallStats[i];
                if(names[i] == null || names[i].Length == 0)
                {
                    break;
                }
                string[] subitems = new string[]
                {
                    names[i],
                    s.timesCalled.ToString(),
                    s.totalBytesAllocated.ToString(),
                    s.totalFunctionsCalled.ToString(),
                    s.totalNewFunctionsBroughtIn.ToString()
                };
                lv.list.Items.Add(new ListViewItem(subitems));
            }
            lv.list.Sort();

            // exact behaviour is not important, just make sure
            // that as much information as possible is displayed
            lv.Width = 670;
            lv.Visible = true;
        }

        private void menuItem6_Click(object sender, System.EventArgs e)
        {
            ListViewer lv = new ListViewer();
            lv.Text = "Functions";

            lv.list.Columns.Clear();
            lv.list.Columns.Add("Name", 250, HorizontalAlignment.Left);
            lv.list.Columns.Add("Times Allocated", 90, HorizontalAlignment.Left);
            lv.list.Columns.Add("Total Bytes Allocated", 120, HorizontalAlignment.Left);

            for(int i = 1; i < types.Length; i++)
            {
                GlobalAllocStats s = globalAllocStats[i];
                if(types[i] == null || types[i].Length == 0)
                {
                    break;
                }
                string[] subitems = new string[]
                {
                    types[i],
                    s.timesAllocated.ToString(),
                    s.totalBytesAllocated.ToString()
                };
                lv.list.Items.Add(new ListViewItem(subitems));
            }
            lv.list.Sort();

            // exact behaviour is not important, just make sure
            // that as much information as possible is displayed
            lv.Width = 500;
            lv.Visible = true;
        }

        private void splitter_SplitterMoved(object sender, System.Windows.Forms.SplitterEventArgs e)
        {
            if(callTreeView == null)
            {
                SetcallTreeView();
            }
            ArrayList columns = callTreeView.GetColumns();
            Column callTreeColumn = callTreeView.GetColumns()[0] as Column;
            int violations = 0, newWidth = callTreeColumn.Width + splitter.Location.X - previousSplitterLocation;
            if(newWidth < 20)
            {
                newWidth = 20;
                violations++;
            }
            if(newWidth > callTreeView.Width - 10)
            {
                newWidth = callTreeView.Width - 10;
                violations++;
            }
            if(violations < 2)
            {
                callTreeColumn.Width = newWidth;
                callTreeView.RepaintTreeView();
            }
            previousSplitterLocation = splitter.Location.X;
        }

        private void menuItem7_Click(object sender, System.EventArgs e)
        {
            DlgFunctionFilter dlgFunctionFilter = new DlgFunctionFilter( this );

            if ( dlgFunctionFilter.ShowDialog()== DialogResult.OK)
            {
                // Dialog has updated filterInclude and filterExclude
                InitForm( false );
            }
        }

        private void menuItem8_Click(object sender, System.EventArgs e)
        {
            fShowSubtree = !fShowSubtree;

            menuItemShowFuture.Checked = fShowSubtree;
            ShowCurrentStack( null, null );
        }

        // copy the stack
        private void menuItem9_Click(object sender, System.EventArgs e)
        {
            StringBuilder sb = new StringBuilder();

            bool fFirstHeader = true;

            foreach (System.Windows.Forms.ColumnHeader h in stackView.Columns)
            {
                if (!fFirstHeader)
                    sb.Append("\t");

                fFirstHeader = false;
                
                sb.Append(h.Text);
            }

            sb.Append("\r\n");
            
            foreach (ListViewItem itm in stackView.Items)
            {
                bool fFirst = true;

                foreach (ListViewItem.ListViewSubItem sitm in itm.SubItems)
                {
                    if (!fFirst)
                        sb.Append("\t");

                    fFirst = false;
                    
                    sb.Append(sitm.Text);
                }
                sb.Append("\r\n");
            }         
            
            Clipboard.SetDataObject(sb.ToString());
        }


        public void RegenerateTree()
        {
            formRect.Location = this.Location;
            formRect.Size = this.Size;

            InitForm( false );
        }

        private void stackView_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
        {
            //  Only sort if subtree is showing
            if (subtreeline == -1)
                return;

            prevSortCol = sortCol;
            sortCol = e.Column;

            if (prevSortCol == sortCol)
            {
                // Second click on a column reverse direction of the sort
                fReverseSort = !fReverseSort;
            }
            else
            {
                // New column.  Forward sort
                fReverseSort = false;
            }

            // Set the ListViewItemSorter property to a new ListViewItemComparer object.
            stackView.ListViewItemSorter = new ListViewItemComparer(e.Column, fReverseSort, subtreeline);

            // Call the sort method to manually sort the column based on the ListViewItemComparer implementation.
            stackView.Sort();
        }

        private void stackView_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            ListViewItem item = stackView.GetItemAt(e.X, e.Y);
            int index;
            string strFn;

            if (e.Button != MouseButtons.Right)
            {
                return;
            }

            ContextMenu contextMenu = stackView.ContextMenu;
            contextMenu.MenuItems.Clear();

            if(item == null)
            {
                return;
            }

            index = item.Index;

            //  Customize the context menu
            contextSelection = index;
            EventHandler eventHandler = new EventHandler( this.stackView_ContextMenuSelection );

            for (int i = 0; i < 2; i++)
            {
                if (filterInclude[i].functionId > 0)
                {
                    if (filterInclude[i].nodetype == TreeNode.NodeType.Call)
                        strFn = MakeNameForFunction( filterInclude[i].functionId );
                    else
                        strFn = MakeNameForAllocation( filterInclude[i].functionId, 0);
                }
                else
                {
                    strFn = "none";
                }
                contextMenu.MenuItems.Add( new MenuItem( "Set Include filter " + (i+1).ToString() + " (" + strFn + ")", eventHandler));
            }

            for (int i = 0; i < 2; i++)
            {
                if (filterExclude[i].functionId > 0)
                {
                    if (filterExclude[i].nodetype == TreeNode.NodeType.Call)
                        strFn = MakeNameForFunction( filterExclude[i].functionId );
                    else
                        strFn = MakeNameForAllocation( filterExclude[i].functionId, 0 );
                }
                else
                {
                    strFn = "none";
                }
                contextMenu.MenuItems.Add( new MenuItem( "Set Exclude filter " + (i+1).ToString() + " (" + strFn + ")", eventHandler));
            }

            contextMenu.MenuItems.Add( new MenuItem( "Clear Filters" , eventHandler ) );
            contextMenu.MenuItems.Add( new MenuItem( "Regenerate Tree" , eventHandler ) );      
        }

        private void stackView_ContextMenuSelection(object sender, System.EventArgs e) 
        {
            MenuItem miClicked = (MenuItem)sender;

            switch (miClicked.Index)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    // Add function to filters
                    TreeNode.NodeType nodeType;
                    int idCurrent;
                    string name;
                    FnViewFilter []filters;

                    // include or exclude filters?
                    bool fIncludes = miClicked.Index == 0 || miClicked.Index == 1;
                    int filterId = (miClicked.Index) & 1;  // 0 or 1

                    nodeType = TreeNode.NodeType.Call;

                    // A bit of a hack.  Use font color to distinguish allocations from functions.
                    if (stackView.Items[contextSelection].ForeColor == Color.Green)
                    {
                        nodeType = TreeNode.NodeType.Allocation;
                    }

                    name = stackView.Items[ contextSelection ].Text;
                    idCurrent = nodeType == TreeNode.NodeType.Call ? GetFunctionId(name) : GetTypeId(name);

                    filters = fIncludes ? filterInclude : filterExclude;
                    filters[ filterId ].nodetype = nodeType;
                    filters[ filterId ].functionId = idCurrent;

                    break;

                case 4:
                    //  Reset filters
                    filterInclude[0].functionId = -1;
                    filterInclude[1].functionId = -1;
                    filterExclude[0].functionId = -1;
                    filterExclude[1].functionId = -1;
                    break;

                case 5:
                    //  Regenerate tree
                    RegenerateTree();
                    break;
            }
        }
    }

    // Implements the manual sorting of items by columns.
    class ListViewItemComparer : IComparer 
    {
        private int sortCol;
        private bool fReverseSort;
        private int subtreeline;

        internal ListViewItemComparer() 
        {
            sortCol = 0;
            fReverseSort = false;
            subtreeline = -1;
        }

        internal ListViewItemComparer(int column, bool fReverse, int subtreeline) 
        {
            sortCol = column;
            fReverseSort = fReverse;
            this.subtreeline = subtreeline;
        }


        public int Compare(object x, object y) 
        {
            int retval;

            if (((ListViewItem)x).Index <= subtreeline || subtreeline == -1)
            {
                // Don't sort items above the "subtree" line
                return 0;
            }

            if (sortCol == 0)
            {
                // function name column.  compare as text
                retval = String.Compare(((ListViewItem)x).SubItems[sortCol].Text, ((ListViewItem)y).SubItems[sortCol].Text);
            }
            else
            {
                // data column. compare a number
                int valX = 0, valY = 0;

                if (((ListViewItem)x).SubItems.Count > sortCol)
                {                                                              
                    valX = Convert.ToInt32(((ListViewItem)x).SubItems[sortCol].Text);
                }

                if (((ListViewItem)y).SubItems.Count > sortCol)
                {                                                              
                    valY = Convert.ToInt32(((ListViewItem)y).SubItems[sortCol].Text);
                }               

                if (  valX < valY)                  
                {
                    retval = -1;
                }
                else if (valX == valY)
                {
                    retval = 0;
                }
                else
                {
                    retval = 1;
                }
            }

            if (fReverseSort)
            {
                retval = 0 - retval;
            }

            return retval;
        }
    }

    internal class Statistics
    {
        private readonly static string[] CounterNames =
        {
            "Calls (incl)",
            "Calls",
            "Bytes (incl)",
            "Bytes",
            "Objects (incl)",
            "Objects",
            "New functions (incl)",
            "Unmanaged calls (incl)",
            "Unmanaged calls",
            "Assemblies loaded (incl)",
            "Assemblies loaded"
        };

        internal readonly static int[] DefaultCounters = {0, 2, 4, 6, 9};

        internal static bool IsInclusive(int id)
        {
            return CounterNames[id].EndsWith("(incl)");
        }

        internal long GetCounterValue(int id)
        {
            switch(id)
            {
                case 0:  return this.numberOfFunctionsCalled;
                case 1:  return this.numberOfFunctionsCalled - this.numberOfFunctionsCalledByKids;
                case 2:  return this.bytesAllocated;
                case 3:  return this.bytesAllocated - this.bytesAllocatedByKids;
                case 4:  return this.numberOfObjectsAllocated;
                case 5:  return this.numberOfObjectsAllocated - this.numberOfObjectsAllocatedByKids;
                case 6:  return this.numberOfNewFunctionsBroughtIn;
                case 7:  return this.numberOfUnmanagedTransitions;
                case 8:  return this.numberOfUnmanagedTransitions - this.numberOfUnmanagedTransitionsByKids;
                case 9:  return this.numberOfAssembliesLoaded;
                case 10: return this.numberOfAssembliesLoaded - this.numberOfAssembliesLoadedByKids;

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

        /* made internal since they are fairly independent
         * and creating accessors wouldn't gain anything */
        internal long bytesAllocated;
        internal long bytesAllocatedByKids;
        internal long numberOfObjectsAllocated;
        internal long numberOfObjectsAllocatedByKids;
        internal long numberOfFunctionsCalled;
        internal long numberOfFunctionsCalledByKids;
        internal long numberOfNewFunctionsBroughtIn;
        internal long numberOfUnmanagedTransitions;
        internal long numberOfUnmanagedTransitionsByKids;
        internal long numberOfAssembliesLoaded;
        internal long numberOfAssembliesLoadedByKids;

        internal bool firstTimeBroughtIn;
        
        internal Statistics()
        {
            bytesAllocated = 0;
            bytesAllocatedByKids = 0;
            numberOfObjectsAllocated = 0;
            numberOfObjectsAllocatedByKids = 0;
            numberOfFunctionsCalled = 0;
            numberOfFunctionsCalledByKids = 0;
            numberOfNewFunctionsBroughtIn = 0;
            numberOfUnmanagedTransitions = 0;
            numberOfUnmanagedTransitionsByKids = 0;
            numberOfAssembliesLoaded = 0;
            numberOfAssembliesLoadedByKids = 0;

            firstTimeBroughtIn = false;
        }

        /* write the contents to the backing store */
        internal void Write(BitWriter bw)
        {
            Helpers.WriteNumber(bw, bytesAllocated);
            Helpers.WriteNumber(bw, bytesAllocatedByKids);
            Helpers.WriteNumber(bw, numberOfObjectsAllocated);
            Helpers.WriteNumber(bw, numberOfObjectsAllocatedByKids);
            Helpers.WriteNumber(bw, numberOfFunctionsCalled);
            Helpers.WriteNumber(bw, numberOfFunctionsCalledByKids);
            Helpers.WriteNumber(bw, numberOfNewFunctionsBroughtIn);
            Helpers.WriteNumber(bw, numberOfUnmanagedTransitions);
            Helpers.WriteNumber(bw, numberOfUnmanagedTransitionsByKids);
            Helpers.WriteNumber(bw, numberOfAssembliesLoaded);
            Helpers.WriteNumber(bw, numberOfAssembliesLoadedByKids);
            bw.WriteBits(firstTimeBroughtIn ? 1ul : 0ul, 1);
        }

        /* read the contents to the backing store */
        internal void Read(BitReader br)
        {
            bytesAllocated = Helpers.ReadNumber(br);
            bytesAllocatedByKids = Helpers.ReadNumber(br);
            numberOfObjectsAllocated = Helpers.ReadNumber(br);
            numberOfObjectsAllocatedByKids = Helpers.ReadNumber(br);
            numberOfFunctionsCalled = Helpers.ReadNumber(br);
            numberOfFunctionsCalledByKids = Helpers.ReadNumber(br);
            numberOfNewFunctionsBroughtIn = Helpers.ReadNumber(br);
            numberOfUnmanagedTransitions = Helpers.ReadNumber(br);
            numberOfUnmanagedTransitionsByKids = Helpers.ReadNumber(br);
            numberOfAssembliesLoaded = Helpers.ReadNumber(br);
            numberOfAssembliesLoadedByKids = Helpers.ReadNumber(br);
            firstTimeBroughtIn = (br.ReadBits(1) != 0);
        }

        /* initialize from the backing store */
        internal Statistics(BitReader br)
        {
            Read(br);
        }
    }

    internal class TreeNode : TreeNodeBase
    {
        /* not to be stored externally */
        internal bool isunmanaged;
        internal bool highlighted;

        /* stored */
        internal enum NodeType {Call = 0, Allocation, AssemblyLoad};

        internal NodeType nodetype;
        internal int stackid, nameId;
        internal long nodeOffset;       // Offset of this node in the trace log

        internal long prevOffset, kidOffset;

        internal Statistics data;

        internal TreeNode(NodeType in_nodetype, int in_stackid) : base()
        {
            highlighted = false;
            data = new Statistics();
            nodetype = in_nodetype;
            stackid = in_stackid;

            prevOffset = kidOffset = nodeOffset = -1;
        }

        internal void Write(BitWriter writer)
        {
            writer.WriteBits((ulong)nodetype, 2);
            Helpers.WriteNumber(writer, stackid);
            if(nodetype == NodeType.AssemblyLoad)
            {
                Helpers.WriteNumber(writer, nameId);
            }
            Helpers.WriteNumber(writer, 1 + kidOffset);
            Helpers.WriteNumber(writer, 1 + prevOffset);
            Helpers.WriteNumber(writer, nodeOffset);
            data.Write(writer);
        }

        internal void Read(BitReader reader)
        {
            nodetype = (NodeType)reader.ReadBits(2);
            stackid = (int)Helpers.ReadNumber(reader);
            if(nodetype == NodeType.AssemblyLoad)
            {
                nameId = (int)Helpers.ReadNumber(reader);
            }
            kidOffset = Helpers.ReadNumber(reader) - 1;
            prevOffset = Helpers.ReadNumber(reader) - 1;
            nodeOffset = Helpers.ReadNumber(reader);
            data = new Statistics(reader);
        }

        /* initialize from the backing store */
        internal TreeNode(BitReader reader)
        {
            Read(reader);
        }
    };
}
