using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using Ws.SvcUtilCodeGen;

namespace Ws.SvcExporter
{
    internal class MetadataCodeGen
    {
        /// <summary>
        /// Generate a DPWSClientProxy source file from a wsdl service description.
        /// </summary>
        /// <param name="serviceDesc">A valid wsdl service description.</param>
        public void GenerateClientProxy(ServiceDescription serviceDesc, TargetPlatform platform)
        {
            // Well here's a nasty used in an attempt to make up a name
            string clientProxyClassName = serviceDesc.Name;
            string clientProxyNs = CodeGenUtils.GenerateDotNetNamespace(serviceDesc.TargetNamespace);
            string filename = serviceDesc.Name + "ClientProxy.cs";

            ClientProxies clientProxies = new ClientProxies(clientProxyNs);

            string ns = serviceDesc.TargetNamespace.Trim();
            if(!ns.EndsWith("/")) ns += "/";

            foreach (PortType portType in serviceDesc.PortTypes)
            {
                ClientProxy clientProxy = new ClientProxy(portType.Name, platform);
                foreach (Operation operation in portType.Operations)
                {
                    // Create action names
                    // Apply special naming rules if this is a notification (event) operation
                    string inAction = ns + operation.Name + "Request";
                    string outAction = ns + operation.Name + ((operation.Messages.Flow == OperationFlow.Notification) ? "" : "Response");
                    clientProxy.AddOperation(operation, inAction, outAction);
                }

                foreach (Message message in serviceDesc.Messages)
                {
                    clientProxy.Messages.Add(message);
                }

                if (clientProxy.ServiceOperations.Count > 0)
                    clientProxies.Add(clientProxy);
            }

            ClientProxyGenerator clientProxyGen = new ClientProxyGenerator();
            clientProxyGen.GenerateCode(filename, clientProxies);
        }

        /// <summary>
        /// Generate a DPWSHostedService source file from a Wsdl service description.
        /// </summary>
        /// <param name="serviceDesc">A valid wsdl service description.</param>
        public void GenerateHostedService(ServiceDescription serviceDesc, TargetPlatform platform)
        {
            // Well here's a nasty used in an attempt to make up a name
            string hostedServiceClassName = serviceDesc.Name;
            string hostedServiceNs = CodeGenUtils.GenerateDotNetNamespace(serviceDesc.TargetNamespace);
            string filename = serviceDesc.Name + "HostedService.cs";

            HostedServices hostedServices = new HostedServices(hostedServiceNs);

            string ns = serviceDesc.TargetNamespace.Trim();
            if(!ns.EndsWith("/")) ns += "/";

            foreach (PortType portType in serviceDesc.PortTypes)
            {
                HostedService hostedService = new HostedService(portType.Name, hostedServiceNs, platform);
                foreach (Operation operation in portType.Operations)
                {
                    // Create action names
                    // Apply special naming rules if this is a notification (event) operation
                    string inAction = ns + operation.Name + "Request";
                    string outAction = ns + operation.Name + ((operation.Messages.Flow == OperationFlow.Notification) ? "" : "Response");
                    hostedService.AddOperation(operation, inAction, outAction);
                }

                foreach (Message message in serviceDesc.Messages)
                {
                    hostedService.Messages.Add(message);
                }
            }

            HostedServiceGenerator hostedServiceGen = new HostedServiceGenerator();
            hostedServiceGen.GenerateCode(filename, hostedServices);
        }

        /// <summary>
        /// Iterate a collection of service descriptions and generate Wsdl's
        /// </summary>
        /// <param name="serviceDescriptions">A ServiceDescriptionCollection containing the colleciton of service descriptions.</param>
        /// <param filename="serviceDescriptions">A string containing the name of th wsdl file. If null the name of the service description is used.</param>
        /// <param name="destinationPath">A string containing the name of a directory where the wsdl file is created.</param>
        public void GenerateWsdls(ServiceDescriptionCollection serviceDescriptions, string filename, string destinationPath)
        {
            // Fix namespaces to Dpws spec
            string dpwsNs = "http://schemas.xmlsoap.org/ws/2006/02/devprof";
            string policyNs = "http://schemas.xmlsoap.org/ws/2004/09/policy";
            XmlSerializerNamespaces serviceNamespaces = new XmlSerializerNamespaces();
            serviceNamespaces.Add("a", "http://schemas.xmlsoap.org/ws/2004/08/addressing");
            serviceNamespaces.Add("l", "http://schemas.xmlsoap.org/wsdl/");
            serviceNamespaces.Add("p", dpwsNs);
            serviceNamespaces.Add("e", "http://schemas.xmlsoap.org/ws/2004/08/eventing");
            serviceNamespaces.Add("s12", "http://schemas.xmlsoap.org/wsdl/soap12/");
            serviceNamespaces.Add("po", policyNs);
            serviceNamespaces.Add("u", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");

            // For each service description in the service description collection generate a wsdl
            bool newFile = true;
            foreach (ServiceDescription serviceDesc in serviceDescriptions)
            {
                // Fix the namespaces
                serviceNamespaces.Add("tns", serviceDesc.TargetNamespace);
                serviceDesc.Namespaces = serviceNamespaces;
                
                XmlSchemaSet schemaSet = new XmlSchemaSet();
                foreach (XmlSchema schema in serviceDesc.Types.Schemas)
                    schemaSet.Add(schema);
                schemaSet.Compile();
                if (filename == null)
                {
                    Logger.WriteLine("Writing Wsdl File: " + destinationPath + serviceDesc.Name + ".wsdl", LogLevel.Normal);

                    // Write the content
                    System.IO.FileStream fileStream = null;
                    fileStream = new System.IO.FileStream(destinationPath + serviceDesc.Name + ".wsdl", System.IO.FileMode.Create);
                    serviceDesc.Write(fileStream);
                    fileStream.Flush();
                    fileStream.Close();
                }
                else
                {
                    System.IO.FileStream fileStream = null;
                    if (serviceDescriptions.Count > 1 && !newFile)
                    {
                        Logger.WriteLine("Appending " + serviceDesc.Name + " Service Description to: " + destinationPath + filename, LogLevel.Normal);
                        fileStream = new System.IO.FileStream(destinationPath + filename, System.IO.FileMode.Append);
                    }
                    else
                    {
                        Logger.WriteLine("Writing " + serviceDesc.Name + " Service Description to: " + destinationPath + filename, LogLevel.Normal);
                        fileStream = new System.IO.FileStream(destinationPath + filename, System.IO.FileMode.Create);
                        newFile = false;
                    }

                    // Write the content
                    serviceDesc.Write(fileStream);
                    fileStream.Flush();
                    fileStream.Write(new byte[] { 0x0D, 0x0A, 0x0D, 0x0A }, 0, 4);
                    fileStream.Flush();
                    fileStream.Close();
                }
            }
        }
    }
}
