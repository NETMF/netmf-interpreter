using System;
using System.Threading;
using System.Collections;
using System.Runtime.CompilerServices;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace Microsoft.SPOT.Drivers.Bluetooth
{
    //////////////////////////////////////////////////////////////////////////////////////////////////
    /// Delegates

    public delegate void BTCmdCompletedEvent(BTConnection sender, BTConnection.BTCmd cmd, bool error);

    /// <summary>
    /// A class that abstracts a Bluetooth connection with Serial Port Profile (RFCOMM).
    /// </summary>
    public class BTConnection : IDisposable
    {
        /// <summary>
        /// A user command for a BTConnection.
        /// </summary>
        public abstract class BTCmd
        {
            public DateTime Timestamp;
            public bool Success;

            protected BTCmd()
            {
                Timestamp = DateTime.Now;
                Success = false;
            }

            public abstract void Execute(BTConnection conn, BTDriver driver);
        }

        //--//

        /// <summary>
        /// Command to check if the local Bluetooth device is attached.
        /// </summary>
        public class CheckDevicePresentCmd : BTCmd
        {
            public CheckDevicePresentCmd() : base() { }

            public override void Execute(BTConnection conn, BTDriver driver)
            {
                Success = driver.CheckDevicePresent();
            }
        }

        //--//

        /// <summary>
        /// Command to search for other devices.  The member "Devices" will contain an
        /// array of found devices or will be null if no devices were found.
        /// </summary>
        public class InquiryCmd : BTCmd
        {
            public BTDeviceInfo[] Devices;

            //--//

            public InquiryCmd()
                : base()
            {
            }

            public override void Execute(BTConnection conn, BTDriver driver)
            {
                Devices = driver.Inquiry();

                if (null != Devices) Success = true;
            }
        }

        //--//

        /// <summary>
        /// Command to become discoverable to other devices for a specified timeout
        /// in milliseconds.
        /// </summary>
        public class InquiryScanCmd : BTCmd
        {
            public int Timeout;

            //--//

            public InquiryScanCmd(int to)
                : base()
            {
                Timeout = to;
            }

            public override void Execute(BTConnection conn, BTDriver driver)
            {
                Success = driver.InquiryScan(Timeout);
            }
        }

        //--//

        /// <summary>
        /// Command to accept a Bluetooth connection from another device (possibly
        /// specified by address) within the specified timeout in milliseconds.
        /// </summary>
        public class PageScanCmd : BTCmd
        {
            public string Address;
            public int Timeout;

            //--//

            public PageScanCmd(string addr, int to)
                : base()
            {
                Address = addr;
                Timeout = to;
            }

            public PageScanCmd(int to)
                : base()
            {
                Timeout = to;
            }

            public override void Execute(BTConnection conn, BTDriver driver)
            {
                conn.Connected = false;
                conn.IsMaster = false;
                conn.ConnectedToAddr = null;

                if (null != this.Address)
                {
                    this.Success = driver.PageScanForAddress(this.Address, Timeout);
                }
                else
                {
                    this.Success = driver.InquiryAndPageScans(Timeout);

                    if (this.Success)
                    {
                        try
                        {
                            this.Address = driver.GetLastConnectedAddress();
                        }
                        catch
                        {
                            this.Address = null;
                        }
                    }
                }

                conn.Connected = this.Success;
                conn.ConnectedToAddr = this.Address;
            }
        }

        //--//

        /// <summary>
        /// Command to attempt to connect to a device with the specified address or
        /// partial name for a specified timeout in milliseconds.
        /// </summary>
        public class PageCmd : BTCmd
        {
            public string Address;
            public int Timeout;

            //--//

            public PageCmd(string addr, int to)
                : base()
            {
                Address = addr;
                Timeout = to;
            }

            public override void Execute(BTConnection conn, BTDriver driver)
            {
                conn.Connected = false;
                conn.IsMaster = true;
                conn.ConnectedToAddr = null;

                if (null != Address)
                {
                    Success = driver.PageAddress(Address, Timeout);

                    conn.Connected = this.Success;
                    conn.IsMaster = true;
                    conn.ConnectedToAddr = this.Address;
                }
            }
        }

        //--//

        /// <summary>
        /// Command to disconnect from the current connection.
        /// </summary>
        public class DisconnectCmd : BTCmd
        {
            public DisconnectCmd() : base() { }

            public override void Execute(BTConnection conn, BTDriver driver)
            {
                this.Success = driver.Disconnect();

                conn.Connected = !this.Success;
            }
        }

        //--//

        /// <summary>
        /// Command to read data from the current Bluetooth connection.
        /// </summary>
        public class ReceiveCmd : BTCmd
        {
            public byte[] Data;
            public int NumReceived;
            public int Timeout;

            //--//

            public ReceiveCmd(ref byte[] buf, int to)
                : base()
            {
                Data = buf;
                NumReceived = 0;
                Timeout = to;
            }

            public override void Execute(BTConnection conn, BTDriver driver)
            {
                NumReceived = driver.Receive(ref Data, Timeout);
                Success = (0 <= NumReceived);

                // Check for disconnect
                if (!Success) conn.Connected = false;
            }
        }

        //--//

        /// <summary>
        /// Command to write data to the current Bluetooth connection.
        /// </summary>
        public class SendCmd : BTCmd
        {
            public byte[] Data;
            public int NumSent;

            //--//

            public SendCmd(byte[] data)
                : base()
            {
                Data = data;
                NumSent = 0;
            }

            public override void Execute(BTConnection conn, BTDriver driver)
            {
                if (null != Data)
                {
                    NumSent = driver.Send(Data);
                    Success = (0 <= NumSent);

                    // Check for disconnect
                    if (!Success) conn.Connected = false;
                }
            }
        }

        //--//

        /// <summary>
        /// Command to set the friendly (human-readable) device name of the local Bluetooth device.
        /// </summary>
        /// <remarks>
        /// Up to 32 alphanumeric characters can be used in a friendly device name.
        /// </remarks>
        public class SetFriendlyDeviceNameCmd : BTCmd
        {
            public string Name;

            //--//

            public SetFriendlyDeviceNameCmd(string name)
                : base()
            {
                Name = name;
            }

            public override void Execute(BTConnection conn, BTDriver driver)
            {
                Success = driver.SetFriendlyDeviceName(Name);
            }
        }

        //--//

        /// <summary>
        /// Command to set the pass key of the local Bluetooth device.
        /// </summary>
        /// <remarks>
        /// Up to 16 alphanumeric characters can be used in a pass key.
        /// </remarks>
        public class SetPassKeyCmd : BTCmd
        {
            public string PassKey;

            //--//

            public SetPassKeyCmd(string key)
                : base()
            {
                PassKey = (null != key) ? key : "1234";
            }

            public override void Execute(BTConnection conn, BTDriver driver)
            {
                Success = driver.SetPassKey(PassKey);
            }
        }

        //--//

        private BTCmdCompletedEvent m_cmdComplete;          // The delegate for callbacks on BT command completion

        //--//

        private BTManager m_manager;                        // The Bluetooth Manager that manages this connection
        private ArrayList m_commandQueue;                   // The queue of commands that the manager processes
        private bool m_disposed;                       // Whether or not this connection has been disposed

        //--//

        internal bool Connected;                          // Whether or not this connection is connected
        internal bool IsMaster;                           // Whether or not this connection is connected as a master
        internal string ConnectedToAddr;                    // The address of the device to which this connection is connected to

        //--//

        /// <summary>
        /// Creates a new BTConnection that is associated with the specified BTManager.
        /// </summary>
        /// <param name="manager">
        /// The BTManager
        /// </param>
        public BTConnection(BTManager manager)
        {
            m_manager = manager;
            m_commandQueue = new ArrayList();
            m_disposed = false;

            Connected = false;
            IsMaster = false;
            ConnectedToAddr = null;
        }

        /// <summary>
        /// Disposes of this BTConnection by marking it disposed and removing it
        /// for the BTManager.
        /// <\summary>
        public void Dispose()
        {
            if (!m_disposed)
            {
                m_manager.RemoveBTConnection(this);
                m_disposed = true;
            }
        }

        /// <summary>
        /// Queues a list of BTCmd objects and tells the BTManager to schedule and
        /// dispatch them.
        /// </summary>
        /// <param name="cmds">
        /// The list of BTCmd objects
        /// </param>
        public void QueueCommands(BTCmd[] cmds)
        {
            if (m_disposed) throw new InvalidOperationException("BTConnection has been disposed");

            lock (m_commandQueue)
            {
                for (int i = 0; i < cmds.Length; ++i) m_commandQueue.Add(cmds[i]);
            }

            m_manager.OnNewCommand(this);
        }

        /// <summary>
        /// Retrieves the next pending command from this connection's command queue.
        /// </summary>
        /// <returns>
        /// The next pending command, or null if there are none.
        /// </returns>
        internal BTCmd GetNextCommand()
        {
            if (m_commandQueue.Count == 0) return null;

            BTCmd next = null;

            lock (m_commandQueue)
            {
                next = (BTCmd)m_commandQueue[0];

                m_commandQueue.RemoveAt(0);
            }

            return next;
        }

        /// <summary>
        /// The event to register command completion callbacks with.
        /// </summary>
        public event BTCmdCompletedEvent OnCommandComplete
        {
            add
            {
                m_cmdComplete = (BTCmdCompletedEvent)Microsoft.SPOT.WeakDelegate.Combine(m_cmdComplete, value);
            }

            remove
            {
                m_cmdComplete = (BTCmdCompletedEvent)Microsoft.SPOT.WeakDelegate.Remove(m_cmdComplete, value);
            }
        }

        /// <summary>
        /// Callback from the BTManager that indicates a command has been completed.
        /// </summary>
        /// <param name="cmd">
        /// The completed command; describes the command and its result
        /// </param>
        /// <param name="error">
        /// Whether or not the command resulted in an error.
        /// </param>
        internal void CmdCompletedCallback(BTCmd cmd, bool error)
        {
            m_cmdComplete(this, cmd, error);
        }

    } // class BTConnection
} // namespace Drivers.Bluetooth


