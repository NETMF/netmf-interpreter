using System;
using System.Net;
using System.Threading;
using System.Collections;
using Ws.Services.Utilities;

using Microsoft.SPOT.Net.NetworkInformation;
using Microsoft.SPOT;

namespace Ws.Services.Transport
{
    /// <summary>
    /// Interface used to abstract a message processor from a specific transport service.
    /// WsThreadManager uses this insterface when calling a message processor.
    /// </summary>
    internal interface IWsTransportMessageProcessor
    {
        /// <summary>
        /// Adds a request object to be processed
        /// </summary>
        int AddRequest(object request);

        /// <summary>
        /// Method prototype that defines a transports message processing method.
        /// </summary>
        void ProcessRequest();

        /// <summary>
        /// Gets the current number of threads currently processing the given message transport
        /// </summary>
        int ThreadCount { get; }
    }

    /// <summary>
    /// Class used to manage Udp processing threads.
    /// </summary>
    /// <remarks>
    /// This class is used to create processing threads that processing request messages. The class monitors
    /// a max thread count and controllable by a consumer of this class. The default value is 2.
    /// </remarks>
    internal class WsThreadManager
    {
        private int m_maxThreadCount;
        private String m_transportName;

        public AutoResetEvent ThreadEvent;

        /// <summary>
        /// Creates an instance of a WsThreadManager class.
        /// </summary>
        /// <param name="maxThreadCount">
        /// An integer containing the maximum number of threads allowed by this thread manager.
        /// </param>
        public WsThreadManager(int maxThreadCount, String transportName)
        {
            m_maxThreadCount = maxThreadCount;
            m_transportName  = transportName;
            ThreadEvent      = new AutoResetEvent(false);
        }

        /// <summary>
        /// Method creates an instance of a message processor and returns a thread ready to run the process.
        /// </summary>
        /// <param name="processor">
        /// A instance of a transport message processor derived from IWsTransportMessageProcessor. This parameter
        /// is passed to the message processor. The message processor calls the ProcessRequest method on this
        /// class from the message processor stub.
        /// </param>
        /// <remarks>
        /// If the current thread count is less than the maximum number of threads allowed, this method
        /// creates a new instance of a MessageProcessor nested class and creates a new thread that
        /// calls the MessageProcessor.ProcessRequest method. If the max thread count is reached
        /// this method returns null.
        /// </remarks>
        public void StartNewThread(IWsTransportMessageProcessor processor, object request)
        {
            int add = processor.AddRequest(request);
            int cnt = processor.ThreadCount;

            if(add >= cnt)
            {
                // Check thread count
                if (cnt >= m_maxThreadCount)
                {
                    return;
                }

                System.Ext.Console.Write("New " + m_transportName + " thread number: " + cnt);

                Thread t = new Thread(new ThreadStart(processor.ProcessRequest));

                if (t != null)
                {
                    t.Start();
                }
            }
        }

        /// <summary>
        /// A property containing the maximun number of threads this thread manager allows.
        /// </summary>
        public int MaxThreadCount { get { return m_maxThreadCount; } set { m_maxThreadCount = value; } }
    }

    /// <summary>
    /// Class used to check for repeat messages.
    /// </summary>
    /// <remarks>Udp messages are typically repeated to insure delivery. This can cause excessive processing
    /// and require an applicaotin to do excessive testing of types to determine if duplicates exists.
    /// This class eliminates this potential problem by providing a function used to check for duplicates.
    /// See the IsDuplicate method for details. Use the MaxTestQSize property to set the number of message
    /// identifiers to queue for the test. The default is 20;
    /// </remarks>
    public class WsMessageCheck
    {
        private Queue m_headerSamples;
        private int m_maxQSize;

        public WsMessageCheck(int maxQSize)
        {
            m_headerSamples = new Queue();
            m_maxQSize = maxQSize;
        }

        public WsMessageCheck() : this(20)
        {
        }

        /// <summary>
        /// Check for a duplicate request message.
        /// </summary>
        /// <param name="messageID">A string containing the message ID obtained from a WsaWsHeader object.</param>
        /// <param name="remoteEndpoint">A string containing a remote endpoint address obtained from a receiving socket.</param>
        /// <returns>True is a match is found, false if no match is found.</returns>
        public bool IsDuplicate(string messageID, string remoteEndpoint)
        {
            String msg = messageID;

            if(remoteEndpoint.Length != 0)
            {
                msg += remoteEndpoint;
            }

            if (m_headerSamples.Contains(msg))
            {
                return true;
            }
            else
            {
                if (m_headerSamples.Count >= m_maxQSize)
                {
                    m_headerSamples.Dequeue();
                }
                m_headerSamples.Enqueue(msg);

                return false;
            }
        }
    }

    /// <summary>
    /// Class used to provide basic network services.
    /// </summary>
    internal static class WsNetworkServices
    {
        /// <summary>
        /// Method used to get the local IPV4 address.
        /// </summary>
        /// <returns>
        /// A string representing the local IPV4 address, null if a valid IPV4 addresss is not aquired.
        /// </returns>
        public static string GetLocalIPV4Address()
        {
            return IPAddress.GetDefaultLocalAddress().ToString();
        }

        public static UInt32 GetLocalIPV4AddressValue()
        {
            byte[] ipBytes = IPAddress.GetDefaultLocalAddress().GetAddressBytes();
            return (UInt32)((ipBytes[0] + (ipBytes[1] << 0x08) + (ipBytes[2] << 0x10) + (ipBytes[3] << 0x18)) & 0xFFFFFFFF);
        }
    }
}


