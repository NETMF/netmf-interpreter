using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using System.Runtime.InteropServices;
using CorDebugInterop;
using Microsoft.Win32;
using Microsoft.VisualStudio.OLE.Interop;

namespace Microsoft.SPOT.Debugger
{
    public class ConnectionPoint : IConnectionPoint
    {
        public class Connections : IEnumerable
        {
            private uint m_dwCookieNext;
            private Hashtable m_ht;

            public Connections()
            {
                m_dwCookieNext = 0;
                m_ht = new Hashtable();
            }

            public void Advise(object pUnkSink, out uint pdwCookie)
            {
                pdwCookie = m_dwCookieNext++;            
                m_ht[pdwCookie] = pUnkSink;
            }

            public void Unadvise(uint dwCookie)
            {
                m_ht.Remove(dwCookie);
            }

            #region IEnumerable Members

            public IEnumerator GetEnumerator()
            {
                ArrayList al = new ArrayList ();

                foreach (object o in m_ht.Values)
                {
                    al.Add (o);
                }

                return al.GetEnumerator ();
            }
            
            #endregion
        }

        private IConnectionPointContainer m_container;
        private Guid m_iid;
        public readonly Connections m_connections;
        
        public ConnectionPoint(IConnectionPointContainer container, Guid iid)
        {
            m_container = container;
            m_iid = iid;
            m_connections = new Connections();
        }

        public Connections Sinks
        {
            get {return m_connections;}
        }

        #region IConnectionPoint Members

        public void Advise(object pUnkSink, out uint pdwCookie)
        {
            m_connections.Advise(pUnkSink, out pdwCookie);
        }

        public void Unadvise(uint dwCookie)
        {
            m_connections.Unadvise(dwCookie);
        }

        public void GetConnectionInterface(out Guid pIID)
        {
            pIID = m_iid;
        }

        public void EnumConnections(out IEnumConnections ppEnum)
        {            
            throw new NotImplementedException();           
        }

        public void GetConnectionPointContainer(out IConnectionPointContainer ppCPC)
        {
            ppCPC = m_container;
        }

        #endregion
    }
}
