using System;
using System.Threading;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Microsoft.SPOT
{
    public class RpcServer
    {
        Type m_selector;
        uint m_id;
        object m_target;
        Type m_remotedMethods;
        Thread m_worker;

        public RpcServer(Type selector, uint id, object target, Type remotedMethods)
        {
            m_selector = selector;
            m_id = id;
            m_target = target;
            m_remotedMethods = remotedMethods;
        }

        public void Start()
        {
            if (m_worker == null)
            {
                m_worker = new Thread(new ThreadStart(this.Worker));
                m_worker.Start();
            }
        }

        public void Stop()
        {
            if (m_worker != null)
            {
                m_worker.Abort();
                m_worker.Join();
                m_worker = null;
            }
        }

        //--//

        void Worker()
        {
            Microsoft.SPOT.Messaging.EndPoint ep = new Microsoft.SPOT.Messaging.EndPoint(m_selector, m_id);
            Microsoft.SPOT.Messaging.Message msg;

            while ((msg = ep.GetMessage(-1)) != null)
            {
                ProcessMessage(msg);
            }
        }

        private void ProcessMessage(Microsoft.SPOT.Messaging.Message msg)
        {
            try
            {
                object[] cmd = (object[])msg.Payload;
                string name = (string)cmd[0];
                object[] args = (object[])cmd[1];

                Type[] argTypes;

                if (args == null)
                {
                    argTypes = new Type[0];
                }
                else
                {
                    argTypes = new Type[args.Length];

                    for (int i = args.Length - 1; i >= 0; i--)
                    {
                        object arg = args[i];

                        argTypes[i] = (arg == null) ? typeof(object) : arg.GetType();
                    }
                }

                System.Reflection.MethodInfo mi = m_remotedMethods.GetMethod(name, argTypes);
                object reply = mi.Invoke(m_target, args);

                msg.Reply(reply);
            }
            catch (Exception ex)
            {
                try
                {
                    msg.Reply(new Microsoft.SPOT.Messaging.Message.RemotedException(ex));
                }
                catch
                {
                    msg.Reply(null);
                }
            }
        }
    }
}


