using System;
using System.Collections;
using System.Text;
using System.Threading;
using Microsoft.SPOT;
using System.Diagnostics;

namespace Ws.Services.Utilities
{
    /// <summary>
    /// Class used to display process timing.
    /// </summary>
    /// <remarks>
    /// This class is provided to display process timinig information. Use this class to display user
    /// defined messages and current, elapsed or total time information that can be useful when testing
    /// processing times. Several instances of this class exist in the device stack. Setting the static Device
    /// property DebugTiming to true turns timing trace messages on. A device developer can include
    /// instances of this class in their code to measure timing parameters.
    /// and use the
    /// </remarks>
    internal class DebugTiming
    {
#if DEBUG
        private long m_lastTime = 0;
#endif

        /// <summary>
        /// Creates an instance of the DebugTiming class.
        /// </summary>
        public DebugTiming()
        {
        }

        /// <summary>
        /// Call this method to display a user defined message and reset the debug timers.
        /// </summary>
        /// <param name="message">
        /// User defined message.
        /// </param>
        /// <returns>
        /// Returns the current time in ticks. Store this return value and use it for the start time
        /// used by the PrintElapsedTime method.
        /// </returns>
        public long ResetStartTime(string message)
        {
#if DEBUG
            m_lastTime = DateTime.Now.Ticks;
            if (message.Length != 0 && message != null)
            {
                System.Ext.Console.Write(message);
            }
            return m_lastTime;
#else
            return 0;
#endif
        }

        /// <summary>
        /// Prints a user defined message and the elapse tick time since either the object was created
        /// or the last time ResstStartTime was called.
        /// </summary>
        /// User defined message.
        /// <param name="message"></param>
        [Conditional("DEBUG")]
        public void PrintElapsedTime(string message)
        {
            long curTime = DateTime.Now.Ticks;
            if (message.Length != 0 && message != null)
            {
                System.Ext.Console.Write(message);
            }

#if DEBUG
            System.Ext.Console.Write(((curTime - m_lastTime)/10000).ToString() + "ms");
            m_lastTime = curTime;
#endif
        }

        /// <summary>
        /// Prints a user defined message and the current tick time.
        /// </summary>
        /// <param name="message">
        /// User defined message.
        /// </param>
        [Conditional("DEBUG")]
        public void PrintCurrentTime(string message)
        {
            long curTime = DateTime.Now.Ticks;
            if (message.Length != 0 && message != null)
            {
                System.Ext.Console.Write(message);
            }

            System.Ext.Console.Write(curTime.ToString());
        }

        /// <summary>
        /// Prints a user defined message and the elapsed time between the current time
        /// and supplied start time.
        /// </summary>
        /// <param name="startTime">
        /// A start tick time used to calculate the elapsed time.
        /// </param>
        /// <param name="message">
        /// User defined message.
        /// </param>
        [Conditional("DEBUG")]
        public void PrintTotalTime(long startTime, string message)
        {
            long curTime = DateTime.Now.Ticks;
            if (message.Length != 0 && message != null)
            {
                System.Ext.Console.Write(message);
            }
            System.Ext.Console.Write(((curTime - startTime)/10000).ToString() + "ms");
        }
    }
}


