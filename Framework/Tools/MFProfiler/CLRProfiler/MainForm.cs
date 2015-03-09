////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////#define USESAFEHANDLES
using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Xml;
#if(USESAFEHANDLES)
using Microsoft.Win32.SafeHandles;
#endif

namespace CLRProfiler
{
    /// <summary>
    /// Summary description for Form1.
    /// </summary>
    public class MainForm
    {
        internal Font font;
        private string logFileName;
        private long logFileStartOffset;
        private long logFileEndOffset;
        internal ReadNewLog log;
        internal ReadLogResult lastLogResult;
        internal static MainForm instance;
        internal string prevlogFileName;
        internal string currlogFileName;
        internal Graph.GraphType graphtype = Graph.GraphType.Invalid;
        internal bool runaswindow = false;


        public MainForm()
        {
            instance = this;
            font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(204))); ;
        }

        internal void ViewGraph(ReadLogResult logResult, string exeName, Graph.GraphType graphType)
        {
            string fileName = log.fileName;
            if (exeName != null)
                fileName = exeName;
            Graph graph = null;
            string title = "";
            switch (graphType)
            {
                case Graph.GraphType.CallGraph:
                    graph = logResult.callstackHistogram.BuildCallGraph(new FilterForm());
                    graph.graphType = Graph.GraphType.CallGraph;
                    title = "Call Graph for: ";
                    break;

                case Graph.GraphType.AssemblyGraph:
                    graph = logResult.callstackHistogram.BuildAssemblyGraph(new FilterForm());
                    graph.graphType = Graph.GraphType.AssemblyGraph;
                    title = "Assembly Graph for: ";
                    break;

                case Graph.GraphType.AllocationGraph:
                    graph = logResult.allocatedHistogram.BuildAllocationGraph(new FilterForm());
                    graph.graphType = Graph.GraphType.AllocationGraph;
                    title = "Allocation Graph for: ";
                    break;

                case Graph.GraphType.HeapGraph:
                    graph = logResult.objectGraph.BuildTypeGraph(new FilterForm());
                    title = "Heap Graph for: ";
                    break;

                case Graph.GraphType.FunctionGraph:
                    graph = logResult.functionList.BuildFunctionGraph(new FilterForm());
                    graph.graphType = Graph.GraphType.FunctionGraph;
                    title = "Function Graph for: ";
                    break;

                case Graph.GraphType.ModuleGraph:
                    graph = logResult.functionList.BuildModuleGraph(new FilterForm());
                    graph.graphType = Graph.GraphType.ModuleGraph;
                    title = "Module Graph for: ";
                    break;

                case Graph.GraphType.ClassGraph:
                    graph = logResult.functionList.BuildClassGraph(new FilterForm());
                    graph.graphType = Graph.GraphType.ClassGraph;
                    title = "Class Graph for: ";
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }
            title += fileName;
            GraphViewForm graphViewForm = new GraphViewForm(graph, title);
            graphViewForm.Visible = true;
        }


        internal void LoadLogFile(string logFileName)
        {
            this.logFileName = logFileName;
            logFileStartOffset = 0;
            logFileEndOffset = long.MaxValue;

            log = new ReadNewLog(logFileName);
            lastLogResult = null;
            ObjectGraph.cachedGraph = null;
            ReadLogResult readLogResult = GetLogResult();
            log.ReadFile(logFileStartOffset, logFileEndOffset, readLogResult);
            lastLogResult = readLogResult;

            viewSummaryMenuItem_Click(null, null);
        }

        private ReadLogResult GetLogResult()
        {
            ReadLogResult readLogResult = lastLogResult;
            if (readLogResult == null)
            {
                readLogResult = new ReadLogResult();
            }
            readLogResult.liveObjectTable = new LiveObjectTable(log);
            readLogResult.sampleObjectTable = new SampleObjectTable(log);
            readLogResult.allocatedHistogram = new Histogram(log);
            readLogResult.callstackHistogram = new Histogram(log);
            readLogResult.relocatedHistogram = new Histogram(log);
            readLogResult.finalizerHistogram = new Histogram(log);
            readLogResult.criticalFinalizerHistogram = new Histogram(log);
            readLogResult.createdHandlesHistogram = new Histogram(log);
            readLogResult.destroyedHandlesHistogram = new Histogram(log);
            if (readLogResult.objectGraph != null)
                readLogResult.objectGraph.Neuter();
            readLogResult.objectGraph = new ObjectGraph(log, 0);
            readLogResult.functionList = new FunctionList(log);
            readLogResult.hadCallInfo = readLogResult.hadAllocInfo = false;
            readLogResult.handleHash = new Dictionary<ulong, HandleInfo>();

            // We may just have turned a lot of data into garbage - let's try to reclaim the memory
            GC.Collect();

            return readLogResult;
        }

        private void viewSummaryMenuItem_Click(object sender, System.EventArgs e)
        {
            if (lastLogResult != null)
            {
                string scenario = log.fileName;

                SummaryForm summaryForm = new SummaryForm(log, lastLogResult, scenario);
                summaryForm.Show();
            }
        }
    }
}
