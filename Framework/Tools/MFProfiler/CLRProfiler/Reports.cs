using System;
using System.Collections;

namespace CLRProfiler
{
	/// <summary>
	/// Summary description for Reports.
	/// </summary>
	public class Reports
	{
		public Reports()
		{
			//
			// need to add constructor logic here
			//
		}

        private static ReadLogResult GetLogResult(ReadNewLog log)
        {
            ReadLogResult readLogResult = new ReadLogResult();
            readLogResult.liveObjectTable = new LiveObjectTable(log);
            readLogResult.sampleObjectTable = new SampleObjectTable(log);

            readLogResult.allocatedHistogram = new Histogram(log);
            readLogResult.callstackHistogram = new Histogram(log);
            readLogResult.relocatedHistogram = new Histogram(log);
            readLogResult.finalizerHistogram = new Histogram(log);
            readLogResult.criticalFinalizerHistogram = new Histogram(log);
            readLogResult.objectGraph = new ObjectGraph(log, 0);
            readLogResult.functionList = new FunctionList(log);
            readLogResult.hadCallInfo = readLogResult.hadAllocInfo = false;
            readLogResult.heapDumpHistograms = new Histogram[0];

            return readLogResult;
        }

        private static int FindMarkerTickIndex(string marker, ReadNewLog log)
        {
            int result = -1;

            if (marker == CommentRangeForm.startCommentString)
                result = 0;
            else if (marker == CommentRangeForm.shutdownCommentString)
                result = log.maxTickIndex;
            else
            {
                for (int i = 0; i < log.commentEventList.count; i++)
                {
                    if (log.commentEventList.eventString[i] == marker)
                        result = log.commentEventList.eventTickIndex[i];
                }
            }

            if (result == -1)
            {
                double time;
                if (double.TryParse(marker, System.Globalization.NumberStyles.Float, null, out time))
                {
                    result = log.TimeToTickIndex(time);
                }
                else
                {
                    Console.WriteLine("Marker {0} not found", marker);
                    return 0;
                }
            }
            return result;
        }

        class TypeDescriptor : IComparable
        {
            internal int[] size;
            internal int[] count;
            internal int typeIndex;

            internal TypeDescriptor(int typeIndex, int slotCount)
            {
                this.typeIndex = typeIndex;
                this.size = new int[slotCount];
                this.count = new int[slotCount];
            }

            public int CompareTo(Object o)
            {
                TypeDescriptor that = (TypeDescriptor)o;
                if (that.size[0] < this.size[0])
                    return -1;
                else if (that.size[0] > this.size[0])
                    return 1;
                else
                    return 0;
            }
        }

        private static void WriteReport(Histogram histogram, string timeMarker)
        {
            WriteReport(new Histogram[1] { histogram }, new string[1] { timeMarker } );
        }

        private static void WriteReport(Histogram[] histogram, string[] timeMarker)
        {
            TypeDescriptor[] typeIndexToTypeDescriptor = new TypeDescriptor[histogram[0].readNewLog.typeName.Length];

            int[] totalSize = new int[histogram.Length];
            int[] totalCount = new int[histogram.Length];
            for (int h = 0; h < histogram.Length; h++)
            {
                int[] typeSizeStacktraceToCount = histogram[h].typeSizeStacktraceToCount;

                for (int i = 0; i < typeSizeStacktraceToCount.Length; i++)
                {
                    int count = typeSizeStacktraceToCount[i];
                    if (count == 0)
                        continue;

                    int[] stacktrace = histogram[h].readNewLog.stacktraceTable.IndexToStacktrace(i);
                    int typeIndex = stacktrace[0];
                    int size = stacktrace[1];

                    if (typeIndexToTypeDescriptor[typeIndex] == null)
                        typeIndexToTypeDescriptor[typeIndex] = new TypeDescriptor(typeIndex, histogram.Length);
                    typeIndexToTypeDescriptor[typeIndex].size[h] += size*count;
                    typeIndexToTypeDescriptor[typeIndex].count[h] += count;

                    totalSize[h] += size*count;
                    totalCount[h] += count;
                }
            }

            ArrayList al = new ArrayList();
            for (int i = 0; i < typeIndexToTypeDescriptor.Length; i++)
            {
                if (typeIndexToTypeDescriptor[i] == null)
                    continue;
                al.Add(typeIndexToTypeDescriptor[i]);
            }
            al.Sort();

            Console.Write("Typename");
            for (int h = 0; h < histogram.Length; h++)
                Console.Write(",Size({0}),#Instances({1})", timeMarker[h], timeMarker[h]);
            Console.WriteLine();
            
            Console.Write("Grand total");
            for (int h = 0; h < histogram.Length; h++)
                Console.Write(",{0},{1}", totalSize[h], totalCount[h]);
            Console.WriteLine();

            foreach (TypeDescriptor td in al)
            {
                Console.Write("{0}", histogram[0].readNewLog.typeName[td.typeIndex]);
                for (int h = 0; h < histogram.Length; h++)
                    Console.Write(",{0},{1}", td.size[h], td.count[h]);
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Added by Sonal: 
        /// Writes Difference between two heap dumps
        /// </summary>
        /// <param name="histogram"></param>
        /// <param name="timeMarker"></param>
        private static void WriteDiffReport(Histogram[] histogram, string[] timeMarker, ReadLogResult entireLogResult)
        {
            TypeDescriptor[] typeIndexToTypeDescriptor = new TypeDescriptor[histogram[0].readNewLog.typeName.Length];

            int[] totalSize = new int[histogram.Length];
            int[] totalCount = new int[histogram.Length];
            for (int h = 0; h < histogram.Length; h++)
            {
                int[] typeSizeStacktraceToCount = histogram[h].typeSizeStacktraceToCount;

                for (int i = 0; i < typeSizeStacktraceToCount.Length; i++)
                {
                    int count = typeSizeStacktraceToCount[i];
                    if (count == 0)
                        continue;

                    int[] stacktrace = histogram[h].readNewLog.stacktraceTable.IndexToStacktrace(i);
                    int typeIndex = stacktrace[0];
                    int size = stacktrace[1];

                    if (typeIndexToTypeDescriptor[typeIndex] == null)
                        typeIndexToTypeDescriptor[typeIndex] = new TypeDescriptor(typeIndex, histogram.Length);
                    typeIndexToTypeDescriptor[typeIndex].size[h] += size * count;
                    typeIndexToTypeDescriptor[typeIndex].count[h] += count;

                    totalSize[h] += size * count;
                    totalCount[h] += count;
                }
            }

            ArrayList al = new ArrayList();
            for (int i = 0; i < typeIndexToTypeDescriptor.Length; i++)
            {
                if (typeIndexToTypeDescriptor[i] == null)
                    continue;
                al.Add(typeIndexToTypeDescriptor[i]);
            }
            al.Sort();
            Console.WriteLine("<?xml version=\"1.0\"?>");
            Console.WriteLine("<DetailedLeakReport>");
            Console.WriteLine("<LeakSummary>");
            int counter = 0;
            foreach (TypeDescriptor td in al)
            {
                int diffCount = td.count[histogram.Length - 1] - td.count[0];
                int diffSize = td.size[histogram.Length - 1] - td.size[0];
                if (diffCount > 0)
                {
                    counter++;
                    Console.WriteLine("<Object>");
                    Console.WriteLine("<Counter>{0}</Counter>", counter);
                    Console.WriteLine("<ObjectName><!--{0}--></ObjectName>", histogram[0].readNewLog.typeName[td.typeIndex]);
                    for (int h = 0; h < histogram.Length; h++)
                    {
                        Console.WriteLine("<CheckPoint{0}>", h + 1);
                        Console.WriteLine("<Instances>{0}</Instances>", td.count[h]);
                        Console.WriteLine("<Size>{0}</Size>", td.size[h]);
                        Console.WriteLine("</CheckPoint{0}>", h + 1);
                    }
                    Console.WriteLine("<Difference>");
                    Console.WriteLine("<Instances>{0}</Instances>", diffCount);
                    Console.WriteLine("<Size>{0}</Size>", diffSize);
                    Console.WriteLine("</Difference>");
                    Console.WriteLine("</Object>");
                }
            }
            Console.WriteLine("</LeakSummary>");
            Console.WriteLine("<LeakDetails>");
            counter = 0;
            foreach (TypeDescriptor td in al)
            {
                int diffCount = td.count[histogram.Length - 1] - td.count[0];
                int diffSize = td.size[histogram.Length - 1] - td.size[0];
                if (diffCount > 0)
                {
                    counter++;
                    Console.WriteLine("<LeakedObject>");
                    Console.WriteLine("<Summary>");
                    Console.WriteLine("<Counter>{0}</Counter>", counter);
                    Console.WriteLine("<ObjectName><!--{0}--></ObjectName>", histogram[0].readNewLog.typeName[td.typeIndex]);
                    for (int h = 0; h < histogram.Length; h++)
                    {
                        Console.WriteLine("<CheckPoint{0}>", h + 1);
                        Console.WriteLine("<Instances>{0}</Instances>", td.count[h]);
                        Console.WriteLine("<Size>{0}</Size>", td.size[h]);
                        Console.WriteLine("</CheckPoint{0}>", h + 1);
                    }
                    Console.WriteLine("<Difference>");
                    Console.WriteLine("<Instances>{0}</Instances>", diffCount);
                    Console.WriteLine("<Size>{0}</Size>", diffSize);
                    if (entireLogResult.requestedObjectGraph != null)
                        entireLogResult.requestedObjectGraph.WriteVertexPaths(histogram[0].tickIndex, histogram[histogram.Length - 1].tickIndex, histogram[0].readNewLog.typeName[td.typeIndex]);
                    else
                        if (entireLogResult.objectGraph != null)
                            entireLogResult.objectGraph.WriteVertexPaths(histogram[0].tickIndex, histogram[histogram.Length - 1].tickIndex, histogram[0].readNewLog.typeName[td.typeIndex]);
                    Console.WriteLine("</LeakedObject>");
                }
            }
            Console.WriteLine("</LeakDetails>");
            Console.WriteLine("</DetailedLeakReport>");
        }

        private static void AllocationRelocationReport(string logFileName, string startMarker, string endMarker, bool relocationReport)
        {
            // first read the entire file
            ReadNewLog log = new ReadNewLog(logFileName, false);
            ReadLogResult entireLogResult = GetLogResult(log);
            log.ReadFile(0, long.MaxValue, entireLogResult);

            // if we were given a start or an end marker, we need to re-read a portion of the file.
            ReadLogResult logResult = entireLogResult;
            if (startMarker != null || endMarker != null)
            {
                int startTickIndex = 0;
                int endTickIndex = entireLogResult.sampleObjectTable.lastTickIndex;

                if (startMarker != null)
                    startTickIndex = FindMarkerTickIndex(startMarker, log);

                if (endMarker != null)
                    endTickIndex = FindMarkerTickIndex(endMarker, log);

                long startPos = log.TickIndexToPos(startTickIndex);
                long endPos = log.TickIndexToPos(endTickIndex);

                // Read the selected portion of the log again
                logResult = new ReadLogResult();
                if (relocationReport)
                {
                    logResult.relocatedHistogram = new Histogram(log);
                    logResult.liveObjectTable = new LiveObjectTable(log);
                }
                else
                    logResult.allocatedHistogram = new Histogram(log);
                log.ReadFile(startPos, endPos, logResult);

                if (startMarker == null)
                    startMarker = CommentRangeForm.startCommentString;
                if (endMarker == null)
                    endMarker = CommentRangeForm.shutdownCommentString;
                Console.WriteLine("{0} summary for {1} between {2} ({3} secs) and {4} ({5} secs)",
                                    relocationReport ? "Relocation" : "Allocation",
                                                    logFileName, 
                                                                startMarker, 
                                                                     log.TickIndexToTime(startTickIndex),
                                                                                   endMarker,
                                                                                        log.TickIndexToTime(endTickIndex));
            }
            else
                Console.WriteLine("{0} summary for {1}",
                                    relocationReport ? "Relocation" : "Allocation",
                                                    logFileName);

            // now we are ready to produce the allocation report from the allocation histogram
            WriteReport(relocationReport ? logResult.relocatedHistogram : logResult.allocatedHistogram, "");
        }

        internal static void AllocationReport(string logFileName, string startMarker, string endMarker)
        {
            AllocationRelocationReport(logFileName, startMarker, endMarker, false);
        }

        internal static void RelocationReport(string logFileName, string startMarker, string endMarker)
        {
            AllocationRelocationReport(logFileName, startMarker, endMarker, true);
        }

        internal static Histogram GetSurvivorHistogram(ReadNewLog log, ReadLogResult entireLogResult, int startTickIndex, int endTickIndex, string timeMarker)
        {
            ReadLogResult logResult = entireLogResult;
            int timeTickIndex = entireLogResult.sampleObjectTable.lastTickIndex;
            if (timeMarker != null)
            {
                timeTickIndex = FindMarkerTickIndex(timeMarker, log);

                long endPos = log.TickIndexToPos(timeTickIndex);

                // Read the selected portion of the log again
                logResult = new ReadLogResult();
                logResult.liveObjectTable = new LiveObjectTable(log);
                log.ReadFile(0, endPos, logResult);
            }

            Histogram histogram = new Histogram(log);
            LiveObjectTable.LiveObject o;
            for (logResult.liveObjectTable.GetNextObject(0, ulong.MaxValue, out o);
                o.id < ulong.MaxValue;
                logResult.liveObjectTable.GetNextObject(o.id + o.size, ulong.MaxValue, out o))
            {
                if (startTickIndex <= o.allocTickIndex && o.allocTickIndex < endTickIndex)
                    histogram.AddObject(o.typeSizeStacktraceIndex, 1);
            }

            return histogram;
        }

        internal static void SurvivorReport(string logFileName, string startMarker, string endMarker, string[] timeMarker)
        {
            // first read the entire file
            ReadNewLog log = new ReadNewLog(logFileName, false);
            ReadLogResult entireLogResult = GetLogResult(log);
            log.ReadFile(0, long.MaxValue, entireLogResult);

            if (startMarker == null)
                startMarker = CommentRangeForm.startCommentString;
            if (endMarker == null)
                endMarker = CommentRangeForm.shutdownCommentString;
            int startTickIndex = FindMarkerTickIndex(startMarker, log);
            int endTickIndex = FindMarkerTickIndex(endMarker, log);

            if (timeMarker == null || timeMarker.Length == 0)
            {
                timeMarker = new String[1];
                timeMarker[0] = CommentRangeForm.shutdownCommentString;
            }

            Histogram[] histogram = new Histogram[timeMarker.Length];
            Console.Write("Surviving objects for {0} allocated between {1} ({2} secs) and {3} ({4} secs) at",
                                                  logFileName,
                                                                        startMarker, 
                                                                             log.TickIndexToTime(startTickIndex),
                                                                                           endMarker,
                                                                                                log.TickIndexToTime(endTickIndex));
            string separator = "";
            for (int i = 0; i < timeMarker.Length; i++)
            {
                if (timeMarker[i] == null)
                    timeMarker[i] = CommentRangeForm.shutdownCommentString;
                histogram[i] = GetSurvivorHistogram(log, entireLogResult, startTickIndex, endTickIndex, timeMarker[i]);
                int timeTickIndex = FindMarkerTickIndex(timeMarker[i], log);
                Console.Write("{0} {1} ({2} secs) ", separator, timeMarker[i], log.TickIndexToTime(timeTickIndex));
                separator = ",";
            }

            Console.WriteLine();

            WriteReport(histogram, timeMarker);
        }

        class DiffTypeDescriptor : IComparable
        {
            internal int aSize;
            internal int aCount;
            internal int bSize;
            internal int bCount;
            internal int diffSize;
            internal int diffCount;
            internal int typeIndex;

            internal DiffTypeDescriptor(int typeIndex)
            {
                this.typeIndex = typeIndex;
            }

            public int CompareTo(Object o)
            {
                DiffTypeDescriptor that = (DiffTypeDescriptor)o;
                if (that.diffSize < this.diffSize)
                    return -1;
                else if (that.diffSize > this.diffSize)
                    return 1;
                else
                    return 0;
            }
        }

        private static void FillHistogramIntoDiffTypeDescriptor(Histogram histogram, DiffTypeDescriptor[] typeIndexToDiffTypeDescriptor)
        {
            for (int i = 0; i < histogram.typeSizeStacktraceToCount.Length; i++)
            {
                int count = histogram.typeSizeStacktraceToCount[i];
                if (count == 0)
                    continue;

                int[] stacktrace = histogram.readNewLog.stacktraceTable.IndexToStacktrace(i);
                int typeIndex = stacktrace[0];
                int size = stacktrace[1];

                if (typeIndexToDiffTypeDescriptor[typeIndex] == null)
                    typeIndexToDiffTypeDescriptor[typeIndex] = new DiffTypeDescriptor(typeIndex);
                typeIndexToDiffTypeDescriptor[typeIndex].bSize += size*count;
                typeIndexToDiffTypeDescriptor[typeIndex].diffSize += size*count;
                typeIndexToDiffTypeDescriptor[typeIndex].bCount += count;
                typeIndexToDiffTypeDescriptor[typeIndex].diffCount += count;
            }
        }

        private static void WriteReport(Histogram aHistogram, Histogram bHistogram)
        {
            DiffTypeDescriptor[] typeIndexToDiffTypeDescriptor = new DiffTypeDescriptor[aHistogram.readNewLog.typeName.Length];

            FillHistogramIntoDiffTypeDescriptor(aHistogram, typeIndexToDiffTypeDescriptor);
            for (int i = 0; i < typeIndexToDiffTypeDescriptor.Length; i++)
            {
                DiffTypeDescriptor td = typeIndexToDiffTypeDescriptor[i];
                if (td != null)
                {
                    td.aSize     =   td.bSize;
                    td.diffSize  = - td.bSize;
                    td.aCount    =   td.bCount;
                    td.diffCount = - td.bCount;
                    td.bSize     = 0;
                    td.bCount    = 0;
                }
            }
            FillHistogramIntoDiffTypeDescriptor(bHistogram, typeIndexToDiffTypeDescriptor);

            ArrayList al = new ArrayList();
            DiffTypeDescriptor totalTd = new DiffTypeDescriptor(0);
            for (int i = 0; i < typeIndexToDiffTypeDescriptor.Length; i++)
            {
                DiffTypeDescriptor td = typeIndexToDiffTypeDescriptor[i];
                if (td != null)
                {
                    al.Add(td);

                    totalTd.aCount    += td.aCount;
                    totalTd.aSize     += td.aSize;
                    totalTd.diffCount += td.diffCount;
                    totalTd.diffSize  += td.diffSize;
                    totalTd.bCount    += td.bCount;
                    totalTd.bSize     += td.bSize;
                }
            }
            al.Sort();

            Console.WriteLine("Start size,Start #instances,End size,End #instances,Diff size,Diff#instances,Typename");
            Console.WriteLine("{0},{1},{2},{3},{4},{5},{6}", totalTd.aSize, totalTd.aCount, totalTd.bSize, totalTd.bCount, totalTd.diffSize, totalTd.diffCount, "Grand total");
            foreach (DiffTypeDescriptor td in al)
            {
                Console.WriteLine("{0},{1},{2},{3},{4},{5},{6}", td.aSize, td.aCount, td.bSize, td.bCount, td.diffSize, td.diffCount, aHistogram.readNewLog.typeName[td.typeIndex]);
            }
        }

        internal static void SurvivorDifferenceReport(string logFileName, string startMarker, string endMarker)
        {
            // first read the entire file
            ReadNewLog log = new ReadNewLog(logFileName, false);
            ReadLogResult entireLogResult = GetLogResult(log);
            log.ReadFile(0, long.MaxValue, entireLogResult);

            if (startMarker == null)
                startMarker = CommentRangeForm.startCommentString;
            if (endMarker == null)
                endMarker = CommentRangeForm.shutdownCommentString;
            int startTickIndex = FindMarkerTickIndex(startMarker, log);
            int endTickIndex = FindMarkerTickIndex(endMarker, log);

            Histogram startHistogram = GetSurvivorHistogram(log, entireLogResult, 0, int.MaxValue, startMarker);
            Histogram endHistogram = GetSurvivorHistogram(log, entireLogResult, 0, int.MaxValue, endMarker);

            Console.WriteLine("Difference in surviving objects for {0} between {1} ({2} secs) and {3} ({4} secs)",
                                                                    logFileName, 
                                                                                startMarker,
                                                                                     log.TickIndexToTime(startTickIndex),
                                                                                                   endMarker, 
                                                                                                        log.TickIndexToTime(endTickIndex));

            WriteReport(startHistogram, endHistogram);
        }

        /// <summary>
        /// Sonal: Function to generate and print Leak Report for the dump file.
        /// </summary>
        /// <param name="logFileName">dump file name</param>
        /// <param name="startMarker">int {0,NumHeapDumps}</param>
        /// <param name="endMarker">int {0,NumHeapDumps}</param>
        internal static void LeakReport(string logFileName, string startMarker, string endMarker)
        {
            if (startMarker == null)
                startMarker = "1";
            if (endMarker == null)
                endMarker = "2";

            int startIndex;
            int endIndex;
            try
            {
                startIndex = int.Parse(startMarker);
                endIndex = int.Parse(endMarker);
            }
            catch
            {
                throw new ArgumentException("Markers have to be positive integral values");
            }
            startIndex--;
            endIndex--;
            if ((startIndex < 0) || (endIndex < 0))
            {
                throw new ArgumentException("Markers can not be negative");
            }
            ReadNewLog log = new ReadNewLog(logFileName, false);
            ReadLogResult entireLogResult = GetLogResult(log);
            log.ReadFile(0, long.MaxValue, entireLogResult, endIndex + 1);
            if (entireLogResult.requestedObjectGraph == null)
            {
                throw new ArgumentException("Invalid EndIndex");
            }

            Histogram[] heapDumpHistograms = entireLogResult.heapDumpHistograms;
            string[] timeMarkers = new String[2];
            if (startIndex < endIndex)
            {
                heapDumpHistograms = new Histogram[endIndex - startIndex + 1];
                for (int i = startIndex; i <= endIndex; i++)
                {
                    heapDumpHistograms[i - startIndex] = entireLogResult.heapDumpHistograms[i];
                }
                timeMarkers = new string[endIndex - startIndex + 1];
            }
            else
            {
                heapDumpHistograms = new Histogram[0];
                timeMarkers = new string[0];
            }
            for (int i = 0; i < timeMarkers.Length; i++)
            {
                timeMarkers[i] = string.Format("Heap dump #{0}", i + 1);
            }
            if (heapDumpHistograms.Length > 0)
                WriteDiffReport(heapDumpHistograms, timeMarkers, entireLogResult);
            else
                Console.WriteLine("***** No heap dumps found *****");
        }

        internal static void HeapDumpReport(string logFileName, string startMarker, string endMarker)
        {
            ReadNewLog log = new ReadNewLog(logFileName, false);
            ReadLogResult entireLogResult = GetLogResult(log);
            log.ReadFile(0, long.MaxValue, entireLogResult);

            Histogram[] heapDumpHistograms = entireLogResult.heapDumpHistograms;
            string[] timeMarkers = new String[heapDumpHistograms.Length];
            if (startMarker != null || endMarker != null)
            {
                int startTickIndex = 0;
                int endTickIndex = entireLogResult.sampleObjectTable.lastTickIndex;

                if (startMarker != null)
                    startTickIndex = FindMarkerTickIndex(startMarker, log);

                if (endMarker != null)
                    endTickIndex = FindMarkerTickIndex(endMarker, log);

                int startIndex = 0;
                int endIndex = 0;
                for (int i = 0; i < log.heapDumpEventList.count; i++)
                {
                    if (log.heapDumpEventList.eventTickIndex[i] < startTickIndex)
                        startIndex = i + 1;
                    if (log.heapDumpEventList.eventTickIndex[i] < endTickIndex)
                        endIndex = i + 1;
                }

                if (endMarker == null)
                {
                    Console.WriteLine("Heap dump for {0} after {1}", logFileName, startMarker);
                    if (startIndex < log.heapDumpEventList.count)
                        endIndex = startIndex + 1;
                    else
                        endIndex = startIndex;
                }
                else
                {
                    Console.WriteLine("Heap dump for {0} between {1} and {2}", logFileName, startMarker, endMarker);
                }
                if (startIndex < endIndex)
                {
                    heapDumpHistograms = new Histogram[endIndex - startIndex];
                    for (int i = startIndex; i < endIndex; i++)
                    {
                        heapDumpHistograms[i - startIndex] = entireLogResult.heapDumpHistograms[i];
                    }
                    timeMarkers = new string[endIndex - startIndex];
                }
                else
                {
                    heapDumpHistograms = new Histogram[0];
                    timeMarkers = new string[0];
                }
            }
            else
            {
                Console.WriteLine("Heap dumps for {0}", logFileName);
            }
            for (int i = 0; i < timeMarkers.Length; i++)
            {
                timeMarkers[i] = string.Format("Heap dump #{0}", i);
            }
            if (heapDumpHistograms.Length > 0)
                WriteReport(heapDumpHistograms, timeMarkers);
            else
                Console.WriteLine("***** No heap dumps found *****");
        }

        internal static void FinalizerReport(bool criticalFinalizers, string logFileName, string startMarker, string endMarker)
        {
            // first read the entire file
            ReadNewLog log = new ReadNewLog(logFileName, false);
            ReadLogResult entireLogResult = GetLogResult(log);
            log.ReadFile(0, long.MaxValue, entireLogResult);

            // if we were given a start or an end marker, we need to re-read a portion of the file.
            ReadLogResult logResult = entireLogResult;
            if (startMarker != null || endMarker != null)
            {
                int startTickIndex = 0;
                int endTickIndex = entireLogResult.sampleObjectTable.lastTickIndex;

                if (startMarker != null)
                    startTickIndex = FindMarkerTickIndex(startMarker, log);

                if (endMarker != null)
                    endTickIndex = FindMarkerTickIndex(endMarker, log);

                long startPos = log.TickIndexToPos(startTickIndex);
                long endPos = log.TickIndexToPos(endTickIndex);

                // Read the selected portion of the log again
                logResult = new ReadLogResult();
                logResult.liveObjectTable = new LiveObjectTable(log);
                logResult.finalizerHistogram = new Histogram(log);
                logResult.criticalFinalizerHistogram = new Histogram(log);

                log.ReadFile(startPos, endPos, logResult);

                if (startMarker == null)
                    startMarker = CommentRangeForm.startCommentString;
                if (endMarker == null)
                    endMarker = CommentRangeForm.shutdownCommentString;
                Console.WriteLine("{0} summary for {1} Objects between {2} ({3} secs) and {4} ({5} secs)",
                                    criticalFinalizers ? "Critical Finalized" : "Finalized",
                                                    logFileName, 
                                                                startMarker, 
                                                                     log.TickIndexToTime(startTickIndex),
                                                                                   endMarker,
                                                                                        log.TickIndexToTime(endTickIndex));
            }
            else
                Console.WriteLine("{0} summary for {1}",
                                    criticalFinalizers ? "Critical Finalized" : "Finalized",
                                                    logFileName);

            // now we are ready to produce the allocation report from the allocation histogram
            WriteReport(criticalFinalizers ? logResult.criticalFinalizerHistogram : logResult.finalizerHistogram, "");
        }

        internal static void CommentReport(string logFileName)
        {
            // first read the entire file
            ReadNewLog log = new ReadNewLog(logFileName, false);
            ReadLogResult entireLogResult = GetLogResult(log);
            log.ReadFile(0, long.MaxValue, entireLogResult);

            Console.WriteLine("Comments logged in {0}", logFileName);
            Console.WriteLine("Time (seconds),Comment");
            for (int i = 0; i < log.commentEventList.count; i++)
                Console.WriteLine("{1:f3},{0}", log.commentEventList.eventString[i], log.TickIndexToTime(log.commentEventList.eventTickIndex[i]));
        }
	}
}
