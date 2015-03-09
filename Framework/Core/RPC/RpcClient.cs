using System;
using System.Threading;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Microsoft.SPOT
{
    public class RpcClient
    {
        static uint s_idNext;

        Type m_selector;
        uint m_id;
        Microsoft.SPOT.Messaging.EndPoint m_ep;

        public RpcClient(Type selector, uint id)
        {
            uint idClient;

            lock (typeof(RpcClient))
            {
                idClient = s_idNext++;
            }

            m_selector = selector;
            m_id = id;
            m_ep = new Microsoft.SPOT.Messaging.EndPoint(typeof(RpcClient), idClient);
        }

        public object Invoke(string methodName, params object[] args)
        {
            object cmd = new object[] { methodName, args };

            object res = m_ep.SendMessage(m_selector, m_id, 1000, cmd);

            if (res != null)
            {
                Microsoft.SPOT.Messaging.Message.RemotedException ex = res as Microsoft.SPOT.Messaging.Message.RemotedException;

                if (ex != null)
                {
                    ex.Raise();
                }
            }

            return res;
        }

        public bool IsServerAlive
        {
            get
            {
                return m_ep.Check(m_selector, m_id, 1000);
            }
        }
    }
}


