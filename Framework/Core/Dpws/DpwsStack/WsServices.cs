using System;
using System.Collections;
using Ws.Services.Mtom;
using Ws.Services.Soap;
using Ws.Services.WsaAddressing;

using Microsoft.SPOT;
using System.Xml;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("MFDpwsDevice")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("MFDpwsClient")]

namespace Ws.Services
{
    /// <summary>
    /// Services hosted by the stack must implement this interface.
    /// </summary>
    /// <remarks>
    /// Implementation specific services must derive from this interface. The methods and properties defined
    /// in the interface are used by the stack services to dispatch request to service endpoints
    /// DpwsHostedService and DpwsClientBase classes implement this interface.
    /// This should only be used by experienced developers that intend to completely bypass the features
    /// provided by the DpwsHostedService and DpswClient base classes.
    /// </remarks>
    public interface IWsServiceEndpoint
    {
        /// <summary>
        /// Property determines if the ProcessRequest call will block. Set to true only if the method called
        /// is thread safe.
        /// </summary>
        bool BlockingCall { get; set; }

        /// <summary>
        /// Property containing a service endpoint address.
        /// </summary>
        string EndpointAddress { get; set; }

        /// <summary>
        /// Finds an implemented service operation or client callback method based on the soap header action property
        /// and calls invoke on the service method.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="envelope"></param>
        /// <returns></returns>
        WsMessage ProcessRequest(WsMessage request);

        /// <summary>
        /// Collection property used to store a target services exposed operations or methods.
        /// </summary>
        WsServiceOperations ServiceOperations { get; }
    }

    /// <summary>
    /// A collection of service endpoints managed by a transport host.
    /// </summary>
    /// <remarks>
    /// This collection is used to store a list of services hosted by a transport. A transport host uses this collection
    /// when dispatching calls to service endpoints.
    /// This base class is thread safe.
    /// </remarks>
    public class WsServiceEndpoints
    {
        private object    m_threadLock;
        private ArrayList m_Services;
        private Hashtable m_lookup;
        private IWsServiceEndpoint m_mexService;

        /// <summary>
        /// Create an instance of the WsServiceEndpoints collection.
        /// </summary>
        internal WsServiceEndpoints()
        {
            m_threadLock = new object();
            m_Services   = new ArrayList();
            m_lookup     = new Hashtable();
        }

        /// <summary>
        /// Gets the number of elements actually contained in the WsServiceEndpoints collection.
        /// </summary>
        /// <returns>
        /// The number of elements actually contained in the WsServiceEndpoints collection.
        /// </returns>
        public int Count
        {
            get
            {
                return m_Services.Count;
            }
        }

        public IWsServiceEndpoint DiscoMexService
        {
            get
            {
                return m_mexService;                
            }

            set
            {
                m_mexService = value;
            }
        }
            

        /// <summary>
        /// Gets the WsService element at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the element to get or set.
        /// </param>
        /// <returns>
        /// The WsService element at the specified index.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If index is less than zero.-or- index is equal to or greater than WsServiceEndpoints collection count.
        /// </exception>
        public IWsServiceEndpoint this[int index]
        {
            get
            {
                return (IWsServiceEndpoint)m_Services[index];
            }
        }

        public IWsServiceEndpoint this[String serviceAddress]
        {
            get
            {
                lock(m_lookup)
                {
                    if(m_lookup.Contains(serviceAddress))
                    {
                        return (IWsServiceEndpoint)m_lookup[serviceAddress];
                    }
                }
                
                lock (m_threadLock)
                {
                    int count = m_Services.Count;
                    IWsServiceEndpoint service;
                    for (int i = 0; i < count; i++)
                    {
                        service = (IWsServiceEndpoint)m_Services[i];
                        if (service.EndpointAddress == serviceAddress)
                        {
                            return service;
                        }

                    }

                    return null;
                }
            }
        }

        /// <summary>
        /// Adds a WsService to the end of the WsServiceEndpoints collection.
        /// </summary>
        /// <param name="service">
        /// The WsService to be added to the end of the WsServiceEndpoints collection.
        /// The value can be null.
        /// </param>
        /// <returns>
        /// The WsServiceEndpoints collection index at which the value has been added.
        /// </returns>
        public int Add(IWsServiceEndpoint service)
        {
            int retVal;
            
            string epService = service.EndpointAddress;

            lock (m_threadLock)
            {
                retVal = m_Services.Add(service);
            }
            
            if (epService.IndexOf("http") == 0)
            {
                // Convert to address to Uri
                Uri toUri = new Uri(epService);

                epService = "urn:uuid:" + toUri.AbsolutePath.Substring(1);

                lock(m_lookup)
                {
                    m_lookup[epService] = service;
                }
            }

            return retVal;
        }

        /// <summary>
        /// Removes all elements from the WsServiceEndpoints collection.
        /// </summary>
        public void Clear()
        {
            lock (m_threadLock)
            {
                m_Services.Clear();
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific IWsServiceEndpoint object from the WsServiceEndpoints collection.
        /// </summary>
        /// <param name="service">
        /// The IWsServiceEndpoint object to remove from the WsServiceEndpoints collection. The value
        /// can be null.
        /// </param>
        public void Remove(IWsServiceEndpoint service)
        {
            string epService = service.EndpointAddress;
            if(service.EndpointAddress.IndexOf("http") == 0)
            {
                // Convert to address to Uri
                Uri toUri = new Uri(epService);
                
                epService = "urn:uuid:" + toUri.AbsolutePath.Substring(1);

                lock(m_lookup)
                {                    
                    m_lookup.Remove(epService);
                }
            }
            
            lock (m_threadLock)
            {
                m_Services.Remove(service);
            }
        }
    }

    /// <summary>
    /// Class use to store information that identifies a target services exposed operations or action endpoint.
    /// </summary>
    /// <remarks>
    /// You must create a WsServiceOperation object for each method in a service that will be exposed to
    /// to a caller. When a request is dispatched to a service, the service uses this collection to
    /// validate and select the target method to invoke. The properties of this object must match the
    /// action property of a soap.header.action method.
    /// </remarks>
    public class WsServiceOperation
    {
        /// <summary>
        /// Creates an instance of a service operation(action) class.
        /// </summary>
        /// <param name="prefix">The operations namespace prefix.</param>
        /// <param name="namespaceUri">The operations namespace Uri.</param>
        /// <param name="name">The operations name.</param>
        public WsServiceOperation(string namespaceUri, string name)
        {
            String uri = namespaceUri[namespaceUri.Length - 1] == '/' ? namespaceUri : namespaceUri + '/';
            this.MethodName = name;
            this.NamespaceUri = namespaceUri;
            this.QualifiedName = uri + name;
        }

        /// <summary>
        /// Property containing the name of a method implemented by a service.
        /// </summary>
        public readonly String MethodName;

        /// <summary>
        /// Property containing the namespace of a method implemented by a service.
        /// </summary>
        public readonly String NamespaceUri;

        /// <summary>
        /// Property containing the fully qualified name of the operation. This property is composed of the
        /// NamespaceUri and the MethodName property. This name must match a soap request headers Action
        /// property.
        /// </summary>
        public readonly String QualifiedName;
    }

    /// <summary>
    /// A collection used to store target service operations.
    /// </summary>
    public class WsServiceOperations
    {
        // Fields
        private object    m_threadLock;
        private ArrayList m_serviceOperations;

        /// <summary>
        /// Creates an instance of the WsServiceOperation class.
        /// </summary>
        /// <remarks>
        /// This collection is thread safe.
        /// </remarks>
        internal WsServiceOperations()
        {
            m_threadLock        = new object();
            m_serviceOperations = new ArrayList();
        }

        /// <summary>
        /// Use to Get the number of elements actually contained in the WsServiceOperations collection.
        /// </summary>
        public int Count
        {
            get
            {
                return m_serviceOperations.Count;
            }
        }

        /// <summary>
        /// Use to Get or set the WsServiceOperation element at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the WsServiceOperation element to get or set.
        /// </param>
        /// <returns>
        /// A WsServiceOperation element.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If index is less than zero.-or- index is equal to or greater than WsServiceOperations collection count.
        /// </exception>
        public WsServiceOperation this[int index]
        {
            get
            {
                return (WsServiceOperation)m_serviceOperations[index];
            }
        }

        public WsServiceOperation this[String qualifiedName]
        {
            get
            {
                lock (m_threadLock)
                {
                    int count = m_serviceOperations.Count;
                    WsServiceOperation operation;
                    for (int i = 0; i < count; i++)
                    {
                        operation = (WsServiceOperation)m_serviceOperations[i];
                        if (operation.QualifiedName == qualifiedName)
                        {
                            return operation;
                        }
                    }

                    return null;
                }
            }
        }

        /// <summary>
        /// Adds an WsServiceOperation object to the end of the WsServiceOperations collection.
        /// </summary>
        /// <param name="serviceOperation">
        /// The WsServiceOperation object to be added to the end of the WsServiceOperations collection.
        /// The value can be null.
        /// </param>
        /// <returns>
        /// The WsServiceOperations collection index at which the value has been added.
        /// </returns>
        public int Add(WsServiceOperation serviceOperation)
        {
            lock (m_threadLock)
            {
                return m_serviceOperations.Add(serviceOperation);
            }
        }

        /// <summary>
        /// Removes all elements from the WsServiceOperations collection.
        /// </summary>
        public void Clear()
        {
            lock (m_threadLock)
            {
                m_serviceOperations.Clear();
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific WsServiceOperation from the WsServiceOperations collection.
        /// </summary>
        /// <param name="serviceOperation">
        /// The WsServiceOperation object to remove from the WsServiceOperations collection.
        /// The value can be null.
        /// </param>
        public void Remove(WsServiceOperation serviceOperation)
        {
            lock (m_threadLock)
            {
                m_serviceOperations.Remove(serviceOperation);
            }
        }
    }
}


