using System;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Threading;
using Microsoft.SPOT.Debugger.WireProtocol;

namespace Microsoft.SPOT.Debugger
{
    internal class MessageCall
    {
        public readonly string Name;
        public readonly object[ ] Args;

        public MessageCall( string name, object[ ] args )
        {
            Name = name;
            Args = args;
        }

        public static MessageCall CreateFromIMethodMessage( IMethodMessage message )
        {
            return new MessageCall( message.MethodName, message.Args );
        }

        public object CreateMessagePayload( )
        {
            return new object[ ] { Name, Args };
        }

        public static MessageCall CreateFromMessagePayload( object payload )
        {
            object[ ] data = ( object[ ] )payload;
            string name = ( string )data[ 0 ];
            object[ ] args = ( object[ ] )data[ 1 ];

            return new MessageCall( name, args );
        }
    }

    public class EndPoint
    {
        internal Engine m_eng;

        internal uint m_type;
        internal uint m_id;

        internal int m_seq;

        private object m_server;
        private Type m_serverClassToRemote;

        internal EndPoint(Type type, uint id, Engine engine)
        {
            m_type = BinaryFormatter.LookupHash(type);
            m_id = id;
            m_seq = 0;
            m_eng = engine;
        }

        public EndPoint(Type type, uint id, object server, Type classToRemote, Engine engine)
            : this(type, id, engine)
        {
            m_server = server;
            m_serverClassToRemote = classToRemote;
        }

        public void Register()
        {
            m_eng.RpcRegisterEndPoint(this);
        }

        public void Deregister()
        {
            m_eng.RpcDeregisterEndPoint(this);
        }

        internal bool CheckDestination(EndPoint ep)
        {
            return m_eng.RpcCheck(InitializeAddressForTransmission(ep));
        }

        internal bool IsRpcServer
        {
            get { return m_server != null; }
        }

        private Commands.Debugging_Messaging_Address InitializeAddressForTransmission(EndPoint epTo)
        {
            Commands.Debugging_Messaging_Address addr = new Commands.Debugging_Messaging_Address();

            addr.m_seq = (uint)Interlocked.Increment(ref m_seq);

            addr.m_from_Type = m_type;
            addr.m_from_Id = m_id;

            addr.m_to_Type = epTo.m_type;
            addr.m_to_Id = epTo.m_id;

            return addr;
        }

        internal Commands.Debugging_Messaging_Address InitializeAddressForReception()
        {
            Commands.Debugging_Messaging_Address addr = new Commands.Debugging_Messaging_Address();

            addr.m_seq = 0;

            addr.m_from_Type = 0;
            addr.m_from_Id = 0;

            addr.m_to_Type = m_type;
            addr.m_to_Id = m_id;

            return addr;
        }

        internal object SendMessage(EndPoint ep, int timeout, MessageCall call)
        {
            object data = call.CreateMessagePayload();

            byte[] payload = m_eng.CreateBinaryFormatter().Serialize(data);

            byte[] res = SendMessageInner(ep, timeout, payload);

            if (res == null)
            {
                throw new RemotingException(string.Format("Remote call '{0}' failed", call.Name));
            }

            object o = m_eng.CreateBinaryFormatter().Deserialize(res);

            Messaging.RemotedException ex = o as Messaging.RemotedException;

            if (ex != null)
            {
                ex.Raise();
            }

            return o;
        }

        internal void DispatchMessage(Message message)
        {
            object res = null;

            try
            {
                MessageCall call = MessageCall.CreateFromMessagePayload(message.Payload);

                object[] args = call.Args;
                Type[] argTypes = new Type[(args == null) ? 0 : args.Length];

                if (args != null)
                {
                    for (int i = args.Length - 1; i >= 0; i--)
                    {
                        object arg = args[i];

                        argTypes[i] = (arg == null) ? typeof(object) : arg.GetType();
                    }
                }

                MethodInfo mi = m_serverClassToRemote.GetMethod(call.Name, argTypes);

                if (mi == null) throw new Exception(string.Format("Could not find remote method '{0}'", call.Name));

                res = mi.Invoke(m_server, call.Args);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    //If an exception is thrown in the target method, it will be packaged up as the InnerException
                    ex = ex.InnerException;
                }

                res = new Messaging.RemotedException(ex);
            }

            try
            {
                message.Reply(res);
            }
            catch
            {
            }
        }

        internal byte[] SendMessageInner(EndPoint ep, int timeout, byte[] data)
        {
            return m_eng.RpcSend(InitializeAddressForTransmission(ep), timeout, data);
        }

        internal void ReplyInner(Message msg, byte[] data)
        {
            m_eng.RpcReply(msg.m_addr, data);
        }

        static public object GetObject(Engine eng, Type type, uint id, Type classToRemote)
        {
            return GetObject(eng, new EndPoint(type, id, eng), classToRemote);
        }

        static internal object GetObject(Engine eng, EndPoint ep, Type classToRemote)
        {
            uint id = eng.RpcGetUniqueEndpointId();

            EndPoint epLocal = new EndPoint(typeof(EndPointProxy), id, eng);

            EndPointProxy prx = new EndPointProxy(eng, epLocal, ep, classToRemote);

            return prx.GetTransparentProxy();
        }

        internal class EndPointProxy : RealProxy, IDisposable
        {
            private Engine m_eng;
            private Type m_type;
            private EndPoint m_from;
            private EndPoint m_to;

            internal EndPointProxy(Engine eng, EndPoint from, EndPoint to, Type type)
                : base(type)
            {
                from.Register();

                if (from.CheckDestination(to) == false)
                {
                    from.Deregister();

                    throw new ArgumentException("Cannot connect to device EndPoint");
                }

                m_eng = eng;
                m_from = from;
                m_to = to;
                m_type = type;
            }

            ~EndPointProxy()
            {
                Dispose();
            }

            public void Dispose()
            {
                try
                {
                    if (m_from != null)
                    {
                        m_from.Deregister();
                    }
                }
                catch
                {
                }
                finally
                {
                    m_eng = null;
                    m_from = null;
                    m_to = null;
                    m_type = null;
                }
            }

            public override IMessage Invoke(IMessage message)
            {
                IMethodMessage myMethodMessage = (IMethodMessage)message;

                if (myMethodMessage.MethodSignature is Array)
                {
                    foreach (Type t in (Array)myMethodMessage.MethodSignature)
                    {
                        if (t.IsByRef)
                        {
                            throw new NotSupportedException("ByRef parameters are not supported");
                        }
                    }
                }

                MethodInfo mi = myMethodMessage.MethodBase as MethodInfo;

                if (mi != null)
                {
                    BinaryFormatter.PopulateFromType(mi.ReturnType);
                }

                MessageCall call = MessageCall.CreateFromIMethodMessage(myMethodMessage);

                object returnValue = m_from.SendMessage(m_to, 60 * 1000, call);

                // Build the return message to pass back to the transparent proxy.
                return new ReturnMessage(returnValue, null, 0, null, (IMethodCallMessage)message);
            }
        }
    }
}