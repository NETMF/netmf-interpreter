using System;
using System.Threading;
using System.Collections;
using System.Runtime.CompilerServices;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace Microsoft.SPOT.Drivers.Bluetooth
{
    public delegate void NewCommandEventHandler(BTConnection conn);

    /// <summary>
    /// A high-level Bluetooth manager class that is responsible for creating
    /// and managing/multiplexing one or more Bluetooth connections through
    /// an underlying Bluetooth driver.
    ///
    /// Threads are scheduled in a FCFS fashion, and later maybe using round-robin,
    /// possibly with priorities.
    /// </summary>
    public class BTManager : IDisposable
    {
        private const int c_DefaultReconnectTimeout = 30 * 1000;

        //--//

        private static BTManager s_manager;                     // Single instance of this class

        //--//

        private IPowerPolicy m_pp;                           // The power policy for the Bluetooth driver
        private BTDriver m_driver;                       // Bluetooth driver
        private Thread m_scheduler;                    // Thread that schedules BT commands
        private ArrayList m_outstandingConnections;       // Queue of outstanding BT connections
        private ArrayList m_connectionsToBeProcessed;     // Queue of connections with BT commands to be processed
        private AutoResetEvent m_newCommand;                   // Event that is set when a new command is queued
        private bool m_disposed;                     // Whether or not this manager has been disposed

        //--//

        /// <summary>
        /// Creates and initializes a new BTManager
        /// </summary>
        protected BTManager(BTDriver driver, IPowerPolicy pp)
        {
            m_driver = null;

            try
            {
                m_driver = driver;
                m_pp = (null == pp) ? new DefaultPowerPolicy() : pp;

                m_outstandingConnections = new ArrayList();
                m_connectionsToBeProcessed = new ArrayList();
                m_newCommand = new AutoResetEvent(false);

                m_scheduler = new Thread(new ThreadStart(AggregateAndDispatch));
                m_scheduler.Start();

                m_disposed = false;
            }
            catch
            {
                m_disposed = true;

                throw;
            }

            // Now apply policy for no pending commands
            m_pp.ApplyPolicy(m_driver, null, null);
        }

        /// <summary>
        /// Disposes of the BTManager.
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Dispose()
        {
            if (!m_disposed)
            {
                // Shut down worker thread
                m_outstandingConnections.Clear();
                m_newCommand.Set();
                m_scheduler.Join();

                // Release HW resources
                m_driver.Disconnect();
                m_driver.Dispose();
            }

            s_manager = null;
            m_disposed = true;
        }

        /// <summary>
        /// Creates a new instance of BTManager.  Locks the BTManager type so that only one
        /// manager can be created.
        /// </summary>
        /// <param name="driver">
        /// The BTDriver to be used to communicate with the underlying Bluetooth chip.
        /// </param>
        /// <param name="pp">
        /// The power policy to be used while managing connections.
        /// </param>
        /// <returns>
        /// The singleton instance of BTManager
        /// </returns>
        public static BTManager GetManager(BTDriver driver, IPowerPolicy pp)
        {
            if (null == s_manager)
            {
                lock (typeof(BTManager))
                {
                    if (null == s_manager)
                    {
                        s_manager = new BTManager(driver, pp);
                    }
                }
            }

            return s_manager;
        }

        /// <summary>
        /// Creates a new BTConnection and ties it to this BTManager.
        /// </summary>
        /// <returns>
        /// The BTConnection
        /// </returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public BTConnection CreateBTConnection()
        {
            if (m_disposed) throw new InvalidOperationException("Manager was disposed");

            BTConnection conn = new BTConnection(this);

            m_outstandingConnections.Add(conn);

            return conn;
        }

        /// <summary>
        /// Removes a given BTConnection from the list of connections supported
        /// by this manager.
        /// </summary>
        /// <param name="btConn">
        /// The connection to remove
        /// </param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        internal void RemoveBTConnection(BTConnection conn)
        {
            m_outstandingConnections.Remove(conn);
        }

        /// <summary>
        /// Called by a BTConnection after queueing one or more new commands; sets
        /// the new command event so that the AggregateAndDispatch thread will wake
        /// up and execute the commands.
        /// </summary>
        /// <param name="conn">
        /// The calling BTConnection
        /// </param>
        internal void OnNewCommand(BTConnection conn)
        {
            lock (m_connectionsToBeProcessed)
            {
                m_connectionsToBeProcessed.Add(conn);
            }

            m_newCommand.Set();
        }

        internal int PendingConnections()
        {
            lock (m_connectionsToBeProcessed)
            {
                return m_connectionsToBeProcessed.Count;
            }
        }

        /// <summary>
        /// This thread method continually loops through the set of managed
        /// BluetoothConnections, aggregating and dispatching sequences
        /// of pending commands from each connection in a round-robin fashion.
        /// </summary>
        /// <remarks>
        /// Notes on scheduling:
        /// Step through and process all pending commands for now...
        ///
        /// Should really be doing time slicing because in many situations the next command
        /// from a connection may not come in until the last one was completed
        ///
        /// Each connection should maintain it's own device state/config.
        /// Depending on how well/whether device states/configs match between connections,
        /// we should be able to overlap certain AT commands (like device queries when
        /// in INQUIRY/PAGE/SCAN mode) and combine results of others (just did an INQUIRY,
        /// so timestamp result and return that result to any other connections asking).
        ///
        /// Should also guarantee that if a single thread exists, it will not be connected,
        /// disconnected, etc.  It just does what it needs to without interruption
        /// </remarks>
        private void AggregateAndDispatch()
        {
            while (true)
            {
                m_newCommand.WaitOne();

                // Check if we are terminating this thread and shutdown
                if (m_outstandingConnections.Count == 0) break;

                if (0 == PendingConnections()) continue;

                BTConnection currentConnection = null;

                do
                {
                    BTConnection.BTCmd cmd;

                    currentConnection = (BTConnection)m_connectionsToBeProcessed[0];

                    m_connectionsToBeProcessed.RemoveAt(0);
                    // Execute the command
                    while (null != (cmd = currentConnection.GetNextCommand()))
                    {
                        bool error = true;

                        m_pp.ApplyPolicy(m_driver, currentConnection, cmd);

                        try
                        {
                            cmd.Execute(currentConnection, m_driver);

                            error = false;

                            currentConnection.CmdCompletedCallback(cmd, error);
                        }
                        catch
                        {
                            // swallow the exception from the user-provided callback
                        }
                    }

                } while (0 < PendingConnections());

                m_pp.ApplyPolicy(m_driver, currentConnection, null);
            } // end while
            ///////////////////////////////////////
            // SHUTDOWN
            //

            // Clear the queue on shutdown
            m_connectionsToBeProcessed.Clear();

            // Apply default power policy
            m_pp.ApplyPolicy(m_driver, null, null);
        }

    } // class BTManager
} // namespace Bluetooth


