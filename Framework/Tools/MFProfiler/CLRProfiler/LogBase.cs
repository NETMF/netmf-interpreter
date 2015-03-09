using System;
using System.IO;

namespace CLRProfiler
{
	/// <summary>
	/// Summary description for LogBase.
	/// </summary>
	public class LogBase
	{
		#region private data member
		private long logFileStartOffset;
		private long logFileEndOffset;
		private string logFileName;
		private ReadNewLog log = null;
		#endregion

		#region public member
		internal ReadLogResult logResult = null;
		#endregion
		
		public LogBase()
		{
			logFileStartOffset = 0;
			logFileEndOffset = long.MaxValue;
		}
		#region public property methods
		public string LogFileName
		{
			get 
			{
				return logFileName;
			}
			set
			{
				logFileName = value;
			}
		}
		#endregion
		#region public methods
		public void readLogFile()
		{
			log = new ReadNewLog(logFileName);
			logResult = GetLogResult();
			log.ReadFile(logFileStartOffset, logFileEndOffset, logResult);
			
		}
		#endregion

		#region private methods
		// from form1.cs
		internal ReadLogResult GetLogResult()
		{
			logResult = new ReadLogResult();
			logResult.liveObjectTable = new LiveObjectTable(log);
			logResult.sampleObjectTable = new SampleObjectTable(log);
			logResult.allocatedHistogram = new Histogram(log);
			logResult.callstackHistogram = new Histogram(log);
			logResult.relocatedHistogram = new Histogram(log);
			logResult.objectGraph = new ObjectGraph(log, 0);
			logResult.functionList = new FunctionList(log);
			logResult.hadCallInfo = logResult.hadAllocInfo = false;

			// We may just have turned a lot of data into garbage - let's try to reclaim the memory
			GC.Collect();
			return logResult;
		}
		#endregion
	}
}
