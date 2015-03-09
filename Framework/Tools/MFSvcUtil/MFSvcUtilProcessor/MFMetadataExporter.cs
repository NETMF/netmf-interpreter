using System;
using System.Collections.Generic;
using System.CodeDom;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ServiceModel;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Ws.SvcUtilCodeGen;

namespace Ws.SvcExporter
{
    public class MFMetadataExporter
    {
        private class DataMemberAttrib
        {
            public bool EmitDefaultValue = true;
            public bool IsAttribute = false;
            public bool IsRequired = true;
            public bool IsNillable = true;
            public string Name = null;
            public int Order = 0;
        }
        
        private string m_assemblyRefPath = null;
        private MetadataCodeGen m_codeGen = new MetadataCodeGen();

        /// <summary>
        /// Given an assembly, use reflection to parse the assemblies module(s) and gerenate a wsdl.
        /// </summary>
        /// <param name="assemblyName">A String containing the path/name of an assembly to process.</param>
        /// <param name="referencePath">A string containing a path used to load refernced assemblies not found in the Gac.</param>
        /// <param name="wsdlFilenameOverride">A string containing a specified wsdl filename.</param>
        /// <param name="contractName">A string containing the name of a specific service contract in the assembly to process.</param>
        /// <param name="destinationPath">A string containing the name of a directory where generated files are created.</param>
        /// <param name="generateSourceFiles">If true generate DPWS source files.</param>
        public void ParseCode(string assemblyName, string referencePath, string wsdlFilenameOverride, string contractName, string destinationPath, bool generateSourceFiles)
        {
            m_assemblyRefPath = referencePath;
            AppDomain domain = AppDomain.CurrentDomain;
            domain.AssemblyResolve += new ResolveEventHandler(domain_AssemblyResolve);

            if (assemblyName == null)
                throw new ArgumentNullException("assemblyName", "Must not be null");

            // If a specific contract is specified only process it
            bool contractFound = false;

            // Load the assembly
            Assembly asm = Assembly.LoadFrom(assemblyName);
            Module[] mods = asm.GetLoadedModules();
            ServiceDescriptionCollection serviceDescriptions = new ServiceDescriptionCollection();
            List<Type> m_processedTypes = new List<Type>();
            bool m_includeBinding = true;

            // Iterate the colleciton of modules in the assembly
            foreach (Module mod in mods)
            {
                Logger.WriteLine("Module name = " + mod.Name, LogLevel.Verbose);
                // Iterate the types in a module looking for types with key attributes
                Type[] types = mod.GetTypes();
                foreach (Type t in types)
                {
                    bool hasPolicy = false;
                    bool hasEvents = false;
                    bool hasOptimizedMimePolicy = false;

                    if (m_processedTypes.Contains(t))
                        continue;

                    // Inintialize a service description used to store the type information if key types are found
                    ServiceDescription serviceDesc = null;
                    PortType portType = null;
                    
                    // For a type iterate an optional list of attributes looking for ServiceContract or DataContract attributes
                    object[] customAttribs = t.GetCustomAttributes(true);
                    for (int i = 0; i < customAttribs.Length; ++i)
                    {
                        // Look for ServiceContract and DataContract attributes
                        Attribute attrib = (Attribute)customAttribs[i];
                        switch (((Type)attrib.TypeId).Name)
                        {
                            case "PolicyAssertionAttribute":
                            case "PolicyAssertion":
                                
                                // Set policy flag
                                hasPolicy = true;

                                // If an Mtom policy assertinon attribute is found set the mtom flag
                                string policy = (string)attrib.GetType().GetProperty("Name", typeof(string)).GetValue(attrib, null);
                                if (policy != null && policy == "OptimizedMimeSerialization")
                                    hasOptimizedMimePolicy = true;
                                break;
                            case "ServiceContractAttribute":
                            case "ServiceContract":

                                // This flag enforces processing of the specified service contract only if specified
                                if (contractFound == true)
                                    break;

                                // If the specified contract is found set the flag    
                                if (contractName != null && contractName == t.Name)
                                    contractFound = true;

                                // If a matching service description is found, append this contract to it else
                                // create a new service description and add it to the service description collection
                                if (serviceDesc == null)
                                {
                                    // If a service description with the same namespace exists append this types information
                                    string typeNamespace = (string)attrib.GetType().GetProperty("Namespace", typeof(string)).GetValue(attrib, null);
                                    typeNamespace = typeNamespace == null ? FixupNamespace(t.Namespace) : typeNamespace;
                                    if ((serviceDesc = FindServiceDescription(typeNamespace, serviceDescriptions)) == null)
                                    {
                                        serviceDesc = new ServiceDescription();
                                        serviceDesc.TargetNamespace = typeNamespace;
                                        serviceDescriptions.Add(serviceDesc);
                                    }
                                }

                                // Process contract type
                                if ((portType = ProcessServiceContract(t, false, serviceDesc)) == null)
                                    throw new Exception("Contract " + t.FullName + " failed to process.");
                                else
                                {
                                    // Set the port type name
                                    portType.Name = (string)attrib.GetType().GetProperty("Name", typeof(string)).GetValue(attrib, null);
                                    if (portType.Name == null)
                                        portType.Name = (t.IsInterface && t.Name[0] == 'I') ? t.Name.Substring(1) : t.Name;

                                    // The the port type to the service description
                                    serviceDesc.PortTypes.Add(portType);
                                    serviceDesc.Name = serviceDesc.Name == null ? portType.Name : serviceDesc.Name;
                                }

                                // Check to see if a callback contract is specified by the service contract attribute
                                // If the contract has an associated callback contract process it
                                Type callbackType = (Type)attrib.GetType().GetProperty("CallbackContract").GetValue(attrib, null);
                                if (callbackType != null)
                                {
                                    // Process callback type
                                    PortType callbackPortType = new PortType();
                                    if ((callbackPortType = ProcessServiceContract(callbackType, true, serviceDesc)) == null)
                                        throw new Exception("Callback contract " + callbackType.Name + " failed to process.");
                                    else
                                    {
                                        // Add callback operations to existing contract porttype operations collection
                                        foreach (Operation operation in callbackPortType.Operations)
                                            portType.Operations.Add(operation);
                                    }

                                    // Set eventing policy indicator
                                    hasEvents = true;

                                    // Add callback type to list of processed types
                                    m_processedTypes.Add(callbackType);
                                }

                                // Add binding if requested
                                if (m_includeBinding)
                                    serviceDesc.Bindings.Add(CreateBinding(portType));

                                // Add contract type to list of processed types
                                m_processedTypes.Add(t);

                                break;
                            case "DataContractAttribute":
                            case "DataContract":

                                string dataTypeNamespace = (string)attrib.GetType().GetProperty("Namespace", typeof(string)).GetValue(attrib, null);
                                dataTypeNamespace = dataTypeNamespace == null ? FixupNamespace(t.Namespace) : dataTypeNamespace;

                                // If a service contract is found set the service description
                                if (serviceDesc == null)
                                {
                                    // If a service description with the same namespace exists append this types information
                                    if ((serviceDesc = FindServiceDescription(dataTypeNamespace, serviceDescriptions)) == null)
                                    {
                                        serviceDesc = new ServiceDescription();
                                        serviceDescriptions.Add(serviceDesc);
                                        serviceDesc.TargetNamespace = dataTypeNamespace;
                                    }
                                }

                                ProcessDataContract(t, dataTypeNamespace, serviceDesc);
                                break;
                            default:
                                break;
                        }
                    }

                    // Add Policy element to service description
                    if (hasPolicy)
                        AddPolicyBindingElement(serviceDesc, hasEvents, hasOptimizedMimePolicy);
                }
            }

            // Generate Wsdls from service descriptions collection
            m_codeGen.GenerateWsdls(serviceDescriptions, wsdlFilenameOverride, destinationPath);
        }

        /// <summary>
        /// Used to reolve assembly load failures
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal Assembly domain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                string[] assemInfo = args.Name.Split(',');
                return Assembly.LoadFrom(m_assemblyRefPath + "\\" + assemInfo[0] + ".dll");
            }
            catch (Exception e)
            {
                Logger.WriteLine("Falied to load external type: " + args.Name, LogLevel.Normal);
                Logger.WriteLine(e.Message, LogLevel.Normal);
                throw e;
            }
        }

        /// <summary>
        /// Find a service description given a targetnamespace
        /// </summary>
        /// <param name="targetNamespace">A string containing the targetnamespace of the service description to find.</param>
        /// <param name="serviceDescriptions">A ServiceDescriptionCollection to search.</param>
        /// <returns>A ServiceDescription if found, null if a matching service is not found.</returns>
        private static ServiceDescription FindServiceDescription(string targetNamespace, ServiceDescriptionCollection serviceDescriptions)
        {
            foreach (ServiceDescription serviceDesc in serviceDescriptions)
            {
                if (serviceDesc.TargetNamespace == targetNamespace)
                    return serviceDesc;
            }
            return null;
        }

        /// <summary>
        /// This method attempts to create an Xml namespace form a .net namespace
        /// </summary>
        /// <param name="originalNs">A string containing a .net namespace.</param>
        /// <returns>A string containing the xml namespace generated from the .net namespace.</returns>
        /// <remarks>
        /// Rules: prefix namespace with http:// if it doesn't already have this prefix. Look for ".com", if
        /// found replace any '.' after ".com" with '/'
        /// </remarks>
        private string FixupNamespace(string originalNs)
        {
            string newNs;
            if (originalNs == null)
                return null;
            newNs = ((originalNs.IndexOf("http://") == 0) ? originalNs : "http://" + originalNs);
            int comIndex = newNs.IndexOf(".com");
            if (comIndex > -1)
            {
                string endNs = newNs.Substring(comIndex + 4);
                endNs = endNs.Replace('.', '/');
                newNs = newNs.Substring(0, comIndex + 4) + endNs;
            }
            return newNs; 
        }

        /// <summary>
        /// Iterate the operations in a service contract type interface and build a Wsdl PortType.
        /// </summary>
        /// <param name="t">An interface type that contains a services operations.</param>
        /// <param name="isCallback">A flag used to signal that this service interface contains callback methods.
        /// Callback methods have an output operation but no input operation.
        /// </param>
        /// <param name="serviceDescription">A service description used to store Wsdl Message elements created when
        /// processing operation parameters.
        /// </param>
        /// <returns>A Wsdl PortType containing the operations implemented by this interface object.</returns>
        private PortType ProcessServiceContract(Type t, bool isCallback, ServiceDescription serviceDescription)
        {
            Logger.WriteLine("", LogLevel.Normal);
            Logger.WriteLine("  ServiceContract = " + t.Name, LogLevel.Normal);

            // Create a temporary PortType
            PortType portType = new PortType();
            portType.Name = (t.IsInterface && t.Name[0] == 'I') ? t.Name.Substring(1) : t.Name;

            // Iterate contracts operations
            MemberInfo[] memberInfo = t.GetMembers();
            foreach (MemberInfo info in memberInfo)
            {
                if (info.MemberType == MemberTypes.Method)
                {
                    // Process the operation
                    Operation operation = ProcessOperation((MethodInfo)info, isCallback, serviceDescription);
                    if (operation == null)
                        throw new Exception("Contract method " + info.Name + " failed to process.");
                    else
                        portType.Operations.Add(operation);
               }
            }

            // If no operation are processed this is an empty contract
            if (portType.Operations.Count == 0)
                Logger.WriteLine("There were no operations found in the contract " + t.FullName, LogLevel.Verbose);

            // Create a binding element
            return portType;
        }

        /// <summary>
        /// Creates a Wsdl Binding object from a PortType object. Wrapped/Doc/Literal binding is assumed.
        /// </summary>
        /// <param name="portType">A PortType object containing service operation definitions.</param>
        /// <returns>A Wsdl Binding object containing operation bindings (wrapped/doc/lit style) generated
        /// from the specified PortType.</returns>
        private Binding CreateBinding(PortType portType)
        {
            Binding binding = new Binding();
            binding.Name = portType.Name + "Binding";
            binding.Type = new XmlQualifiedName(portType.Name, portType.ServiceDescription.TargetNamespace);

            // Add soap binding extension
            XmlDocument xDoc = new XmlDocument();
            XmlElement extElement = xDoc.CreateElement("wsoap12", "binding", "http://schemas.xmlsoap.org/wsdl/soap12/");
            XmlAttribute attrib = xDoc.CreateAttribute("style");
            attrib.Value = "document";
            extElement.Attributes.Append(attrib);
            attrib = xDoc.CreateAttribute("transport");
            attrib.Value = "http://schemas.xmlsoap.org/soap/http";
            extElement.Attributes.Append(attrib);
            binding.Extensions.Add(extElement);

            // Add policy reference extension
            extElement = xDoc.CreateElement("po", "PolicyReference", "http://schemas.xmlsoap.org/ws/2004/09/policy");
            attrib = xDoc.CreateAttribute("URI");
            attrib.Value = "#" + portType.Name + "Policy";
            extElement.Attributes.Append(attrib);
            attrib = xDoc.CreateAttribute("l", "required", "http://schemas.xmlsoap.org/wsdl/");
            attrib.Value = "true";
            extElement.Attributes.Append(attrib);
            binding.Extensions.Add(extElement);

            foreach (Operation operation in portType.Operations)
                binding.Operations.Add(CreateOperationBinding(operation));

            return binding;
        }

        /// <summary>
        /// Creates a single OperationBinding from a PortType operation.
        /// </summary>
        /// <param name="operation">A PortType operation.</param>
        /// <returns>An operation binding object.</returns>
        private OperationBinding CreateOperationBinding(Operation operation)
        {
            OperationBinding operationBinding = new OperationBinding();
            operationBinding.Name = operation.Name;
            XmlDocument xDoc = new XmlDocument();
            XmlElement operationExt = xDoc.CreateElement("wsoap12", "operation", "http://schemas.xmlsoap.org/wsdl/soap12/");
            operationBinding.Extensions.Add(operationExt);
            XmlAttribute attrib = xDoc.CreateAttribute("use");
            attrib.Value = "literal";
            operationExt = xDoc.CreateElement("wsoap12", "body", "http://schemas.xmlsoap.org/wsdl/soap12/");
            operationExt.Attributes.Append(attrib);

            if (operation.Messages.Flow == OperationFlow.OneWay || operation.Messages.Flow == OperationFlow.RequestResponse)
            {
                operationBinding.Input = new InputBinding();
                operationBinding.Input.Extensions.Add(operationExt);
            }
            if (operation.Messages.Flow == OperationFlow.RequestResponse || operation.Messages.Flow == OperationFlow.Notification)
            {
                operationBinding.Output = new OutputBinding();
                operationBinding.Output.Extensions.Add(operationExt);
            }
            return operationBinding;
        }

        /// <summary>
        /// Parses a MethodInfo type and generates an Wsdl Operation element and Wsdl Message element information objects.
        /// </summary>
        /// <param name="methodInfo">A method info object containing a reflected types method.</param>
        /// <param name="isCallback">A flag used to signal that this is a callback operation.</param>
        /// <param name="serviceDescription">A ServiceDescription object used to obtain a targetNamespace
        /// and store Wsdl Message element information objects.</param>
        /// <returns></returns>
        private Operation ProcessOperation(MethodInfo methodInfo, bool isCallback, ServiceDescription serviceDescription)
        {
            Operation operation = new Operation();
            Uri actionUri = null;
            Uri replyActionUri = null;
            bool isOneWay = false;
            string operationName = null;
            XmlDocument xmlDoc = new XmlDocument();

            // Look for the OperationContractAttribute
            object[] customAttributes = methodInfo.GetCustomAttributes(false);
            foreach (Attribute customAttrib in customAttributes)
            {
                string customAttribName = customAttrib.GetType().Name;
                if (customAttribName == "OperationContractAttribute" || customAttribName == "OperationContract")
                {

                    object o = customAttrib.GetType().GetProperty("IsOneWay", typeof(bool)).GetValue(customAttrib, null);
                    if(o != null)
                    {
                        isOneWay = (bool)o;
                    }
                    
                    // Create in and out action attributes. If no action contract attribute is found create an action
                    // using the class namespace and operation name. If ReplyAction attribute is not found but action
                    // is use the action attribute to build the replyto attribute.
                    // Create in action uri
                    string action = (string)customAttrib.GetType().GetProperty("Action", typeof(string)).GetValue(customAttrib, null);
                    string actionNamespace = null;
                    string actionName = null;

                    string ns = serviceDescription.TargetNamespace.Trim();
                    if(!ns.EndsWith("/")) ns += "/";
                    
                    if (action == null)
                        actionUri = new Uri(ns + methodInfo.DeclaringType.Name + "/" + methodInfo.Name);
                    else
                    {
                        int actionIndex = action.LastIndexOf(':');
                        actionIndex = actionIndex == -1 || actionIndex < action.LastIndexOf('/') ? action.LastIndexOf('/') : actionIndex;
                        actionNamespace = actionIndex == -1 ? action : action.Substring(0, actionIndex);
                        actionName = actionIndex == -1 ? action : action.Substring(actionIndex + 1, action.Length - (actionIndex + 1));
                        actionUri = new Uri(actionNamespace + "/" + actionName);
                    }
                    
                    // Create out action Uri
                    string replyAction = (string)customAttrib.GetType().GetProperty("ReplyAction", typeof(string)).GetValue(customAttrib, null);
                    if (replyAction == null)
                    {
                        if (action == null)
                            replyActionUri = new Uri(ns + methodInfo.DeclaringType.Name + "/" + methodInfo.Name + "Response");
                        else
                            replyActionUri = new Uri(actionNamespace + "/" + methodInfo.Name + "Response");
                    }
                    else
                    {
                        int actionIndex = replyAction.LastIndexOf(':');
                        actionIndex = actionIndex == -1 || actionIndex < replyAction.LastIndexOf('/') ? replyAction.LastIndexOf('/') : actionIndex;
                        actionNamespace = actionIndex == -1 ? replyAction : replyAction.Substring(0, actionIndex);
                        actionName = actionIndex == -1 ? replyAction : replyAction.Substring(actionIndex + 1, replyAction.Length - (actionIndex + 1));
                        replyActionUri = new Uri(actionNamespace + "/" + actionName);
                    }

                    operation.Name = operationName == null ? methodInfo.Name : operationName;
                    Logger.WriteLine("      Operation Name = " + operation.Name, LogLevel.Normal);

                    // Process the operations request parameters
                    ProcessOperationParams(methodInfo, operation, isCallback, serviceDescription);

                    // If this is a callback don't include an input type
                    if (!isCallback)
                    {
                        OperationInput inMessage = new OperationInput();
                        inMessage.Message = new XmlQualifiedName(methodInfo.Name + "In", serviceDescription.TargetNamespace);
                        
                        // Add the action extension attribute to the message (Wsdapi needs it to be in the operation)
                        XmlAttribute inActionAttrib = xmlDoc.CreateAttribute("a", "Action", "http://schemas.xmlsoap.org/ws/2004/08/addressing");
                        XmlAttribute[] inAttribs = new XmlAttribute[1];
                        inActionAttrib.Value = actionUri.ToString();
                        inAttribs.SetValue(inActionAttrib, 0);
                        inMessage.ExtensibleAttributes = inAttribs;

                        operation.Messages.Add(inMessage);

                        // If the message is a oneway, skip creating a out message
                        if (!isOneWay)
                        {
                            ProcessReturnType(methodInfo, operation, isCallback, serviceDescription);
                            OperationOutput outMessage = new OperationOutput();
                            outMessage.Message = new XmlQualifiedName(methodInfo.Name + "Out", serviceDescription.TargetNamespace);
                            
                            // Add the extension action attribute to the message (Wsdapi needs it to be in the operaiton)
                            XmlAttribute outActionAttrib = xmlDoc.CreateAttribute("a", "Action", "http://schemas.xmlsoap.org/ws/2004/08/addressing");
                            XmlAttribute[] outAttribs = new XmlAttribute[1];
                            outActionAttrib.Value = replyActionUri.ToString();
                            outAttribs.SetValue(outActionAttrib, 0);
                            outMessage.ExtensibleAttributes = outAttribs;
                            
                            operation.Messages.Add(outMessage);
                        }
                    }
                    // Else if this is a callback operation the method parameters become out parameter
                    else
                    {
                        OperationOutput outMessage = new OperationOutput();
                        outMessage.Message = new XmlQualifiedName(methodInfo.Name + "Out", serviceDescription.TargetNamespace);

                        // Add the extension action attribute to the message (Wsdapi needs it to be in the operaiton)
                        XmlAttribute actionAttrib = xmlDoc.CreateAttribute("a", "Action", "http://schemas.xmlsoap.org/ws/2004/08/addressing");
                        XmlAttribute[] attribs = new XmlAttribute[1];
                        actionAttrib.Value = actionUri.ToString();
                        attribs.SetValue(actionAttrib, 0);
                        outMessage.ExtensibleAttributes = attribs;

                        operation.Messages.Add(outMessage);
                    }

                    return operation;
                }
            }
            return null;
        }

        /// <summary>
        /// Create and add an element to a service descriptions schema. Used to create a native XmlType element.
        /// </summary>
        /// <param name="elementName">A string contininig the name of an element.</param>
        /// <param name="elementTypeNs">A string containing the namespace of the type.</param>
        /// <param name="xmlTypeName">A string containing the name of an element type.</param>
        /// <param name="isArray">True if the element represents an array of native types.</param>
        /// <param name="serviceDesc">The ServiceDescription used to store Wsdl definition.</param>
        private void AddNativeTypeElement(string elementName, string elementNs, string xmlTypeName, bool isArray, XmlSchemaSequence seq, XmlSchema schema )
        {
            XmlSchemaElement elemType = new XmlSchemaElement();

            elemType.Name = elementName;
            if (isArray)
                elemType.SchemaTypeName = new XmlQualifiedName(elementName + "ListType", elementNs);
            else
                elemType.SchemaTypeName = new XmlQualifiedName(xmlTypeName, "http://www.w3.org/2001/XMLSchema");

            seq.Items.Add(elemType);

            // If this is an array add a simple list type
            if (isArray)
            {
                XmlSchemaSimpleType simpleType = new XmlSchemaSimpleType();
                simpleType.Name = elementName + "ListType";
                XmlSchemaSimpleTypeList simpleContent = new XmlSchemaSimpleTypeList();
                simpleContent.ItemTypeName = new XmlQualifiedName(xmlTypeName, "http://www.w3.org/2001/XMLSchema");
                simpleType.Content = simpleContent;
                AddSchemaItem(schema, simpleType);
            }
            return;
        }
        
        /// <summary>
        /// Generates a Wsdl Message information object for each method parameter.
        /// </summary>
        /// <param name="methodInfo">A MethodInfo object containing a reflected types method information.</param>
        /// <param name="operation">Used to gnerate a Wsdl Message name.</param>
        /// <param name="isCallback">A flag used to signal that the operation is a callback operation.
        /// Callback operations have a different operaiton nameing convension.</param>
        /// <param name="serviceDescription">A ServiceDescription object used to store the generated Message(s).</param>
        /// <returns>True if the operation contains message parameters.</returns>
        bool ProcessOperationParams(MethodInfo methodInfo, Operation operation, bool isCallback, ServiceDescription serviceDescription)
        {
            bool messageFound = false;

            XmlSchema schema = FindSchemaByNamespace(serviceDescription.TargetNamespace, serviceDescription.Types.Schemas);
            if (schema == null)
            {
                schema = new XmlSchema();
                serviceDescription.Types.Schemas.Add(schema);
                schema.TargetNamespace = FixupNamespace(serviceDescription.TargetNamespace);
                schema.Namespaces.Add("xs", "http://www.w3.org/2001/XMLSchema");
            }

            XmlSchemaSequence seq = new XmlSchemaSequence();

            // Iterate the collection or parameters
            ParameterInfo[] parameters = methodInfo.GetParameters();
            for (int i = 0; i < parameters.Length; ++i)
            {
                Logger.WriteLine("        Param " + i + " Name: " + parameters[i].Name, LogLevel.Verbose);
                Logger.WriteLine("        Param " + i + " Type: " + parameters[i].ParameterType, LogLevel.Verbose);

                Message message = new Message();
                MessagePart part = new MessagePart();

                // Create the message, part and element
                message.Name = operation.Name + (isCallback ? "Out" : "In");
                part.Name = "parameters";

                // If the type is a native type, create an element name using the method name
                string elementName;
                bool isArray = parameters[i].ParameterType.FullName.EndsWith("[]");
                if (!isArray && (elementName = CodeGenUtils.GetXmlType(parameters[i].ParameterType.FullName)) == null)
                    elementName = parameters[i].ParameterType.Name;
                else
                    elementName = parameters[i].Name;

                part.Element = new XmlQualifiedName(parameters[i].ParameterType.Name, serviceDescription.TargetNamespace);

                // Add the new part to the message
                message.Parts.Add(part);

                // Add message to service description
                serviceDescription.Messages.Add(message);
                messageFound = true;

                // If the method returns a single native type create the element here
                // while we have all of the information. We do not forse a developer 
                // to create an element for simple types.
                string xmlTypeName = parameters[i].ParameterType.FullName;
                if (isArray)
                    xmlTypeName = xmlTypeName.Substring(0, xmlTypeName.Length - 2);
                xmlTypeName = CodeGenUtils.GetXmlType(xmlTypeName);
                if (xmlTypeName != null)
                {
                    AddNativeTypeElement(elementName, serviceDescription.TargetNamespace, xmlTypeName, isArray, seq, schema);
                }
            }

            return messageFound;
        }

        /// <summary>
        /// Generate a ServiceDescription Message object from a reflected methods return type.
        /// </summary>
        /// <param name="methodInfo">A MethodInfo object containing a reflected types method information.</param>
        /// <param name="operation">Used to gnerate a Wsdl Message name.</param>
        /// <param name="isCallback">A flag used to signal that the operation is a callback operation.
        /// Callback operations have a different operaiton nameing convension.</param>
        /// <param name="serviceDescription">A ServiceDescription object used to store generated Message.</param>
        /// <remarks>Since method return types are not named. Message names are generated from the operation name plus the "Response"
        /// unless the method is a callback method.</remarks>
        void ProcessReturnType(MethodInfo methodInfo, Operation operation, bool isCallback, ServiceDescription serviceDescription)
        {
            // Create return message
            Logger.WriteLine("        Return Type: " + methodInfo.ReturnType, LogLevel.Verbose);

            // If the return type is a native type, create an element name using the method name
            string elementName = methodInfo.Name + "Result";
            bool isArray = methodInfo.ReturnType.FullName.EndsWith("[]");

            Message message = new Message();
            MessagePart part = new MessagePart();

            // Create the message, part and element
            message.Name = operation.Name + "Out";
            if (elementName != "Void")
            {
                part.Name = "parameters";
                part.Element = new XmlQualifiedName(methodInfo.Name + "Response", serviceDescription.TargetNamespace);

                // Add the new part to the message
                message.Parts.Add(part);
            }

            // Add message to service description
            serviceDescription.Messages.Add(message);

            // If the method returns a single native type create the element here
            // while we have all of the informaiton. We do not forse a developer 
            // to create an element for simple types.
            string xmlTypeName = methodInfo.ReturnType.FullName;
            if (isArray)
                xmlTypeName = xmlTypeName.Substring(0, xmlTypeName.Length - 2);
            xmlTypeName = CodeGenUtils.GetXmlType(xmlTypeName);
            if (xmlTypeName != null)
            {
                XmlSchema schema = FindSchemaByNamespace(serviceDescription.TargetNamespace, serviceDescription.Types.Schemas);
                if (schema == null)
                {
                    schema = new XmlSchema();
                    serviceDescription.Types.Schemas.Add(schema);
                    schema.TargetNamespace = FixupNamespace(serviceDescription.TargetNamespace);
                    schema.Namespaces.Add("xs", "http://www.w3.org/2001/XMLSchema");
                }

                // Add the element to the schema
                XmlSchemaElement element = new XmlSchemaElement();
                element.Name = methodInfo.Name + "Response";

                XmlSchemaComplexType complexType = new XmlSchemaComplexType();
                XmlSchemaSequence seq = new XmlSchemaSequence();
                AddSchemaItem(schema, element);
                
                element.SchemaType = complexType;
                complexType.Particle = seq;

                AddNativeTypeElement(elementName, serviceDescription.TargetNamespace, xmlTypeName, isArray, seq, schema);
            }
        }

        /// <summary>
        /// For a given reflected type. Create and XmlSchema element that defines the type.
        /// </summary>
        /// <param name="t">A reflected data type.</param>
        /// <param name="typeNamespace">A string containing the targetnamespace of the data type.</param>
        /// <param name="serviceDesc">A ServiceDescription object used to store the schema element.</param>
        private void ProcessDataContract(Type t, string typeNamespace, ServiceDescription serviceDesc)
        {
            Logger.WriteLine("", LogLevel.Normal);
            Logger.WriteLine("  DataContract = " + t.Name, LogLevel.Normal);

            // See if a schema mathing the types namespace exist. If so add to it, else create a new one
            XmlSchema schema = FindSchemaByNamespace(typeNamespace, serviceDesc.Types.Schemas);
            if (schema == null)
            {
                schema = new XmlSchema();
                serviceDesc.Types.Schemas.Add(schema);
                schema.TargetNamespace = typeNamespace;
            }

            if (t.IsEnum)
            {
                ProcessEnumMemberType(t, schema, typeNamespace, serviceDesc);
                return;
            }

            // Create XmlSchemaElement used to store the data type
            XmlSchemaElement element = new XmlSchemaElement();
            element.Name = t.Name;

            // Iterate list of data members and create schema elements and types
            MemberInfo[] members = GetDataMembers(t);

            // If data type has a single, native translatable member create an element that references a SimpleType
            if (members != null && members.Length == 1)
            {
                // Check for native xml type
                FieldInfo fieldInfo = (FieldInfo)members[0];
                string xmlTypeName = CodeGenUtils.GetXmlType(fieldInfo.FieldType.FullName);
                if (xmlTypeName != null)
                {
                    element.SchemaTypeName = new XmlQualifiedName(xmlTypeName, "http://www.w3.org/2001/XMLSchema");
                    AddSchemaItem(schema, element);
                    return;
                }
            }
            ProcessDataMemberType(members, ref element, schema, typeNamespace, serviceDesc);

            return;
        }

        /// <summary>
        /// Find by namespace name, the specified XmlSchema in a collection of XmlSchema.
        /// </summary>
        /// <param name="schemaNs">A string containing the namespace name of the XmlSchema to find.</param>
        /// <param name="schemas">A collection of XmlSchema</param>
        /// <returns>An XmlSchema if a match is found or null.</returns>
        private XmlSchema FindSchemaByNamespace(string schemaNs, XmlSchemas schemas)
        {
            // Find a schema with a matching namespace or create a new one
            foreach (XmlSchema matchingSchema in schemas)
            {
                if (matchingSchema.TargetNamespace == schemaNs)
                    return matchingSchema;
            }
            return null;
        }

        /// <summary>
        /// If the FieldInfo has a DataMemberAttribute parse it and store the results
        /// </summary>
        /// <param name="fieldInfo">An attributed FieldInfo class.</param>
        /// <returns>A DataMemberAttrib class containing parsed attrib values or null if a DataMemberAttribute is not found.</returns>
        private DataMemberAttrib GetDataMemberAttribute(FieldInfo fieldInfo)
        {
            DataMemberAttrib dataMemberAttrib = null;
            object[] customAttribs = fieldInfo.GetCustomAttributes(true);
            if (customAttribs != null)
            {
                dataMemberAttrib = new DataMemberAttrib();
                foreach (Attribute customAttrib in customAttribs)
                {
                    if (((Type)customAttrib.TypeId).Name == "DataMemberAttribute" || ((Type)customAttrib.TypeId).Name == "DataMember")
                    {
                        dataMemberAttrib.Name = (string)customAttrib.GetType().GetProperty("Name", typeof(string)).GetValue(customAttrib, null);
                        dataMemberAttrib.IsAttribute = (bool)customAttrib.GetType().GetProperty("IsAttribute", typeof(bool)).GetValue(customAttrib, null);
                        dataMemberAttrib.IsRequired = (bool)customAttrib.GetType().GetProperty("IsRequired", typeof(bool)).GetValue(customAttrib, null);
                        dataMemberAttrib.IsNillable = (bool)customAttrib.GetType().GetProperty("IsNillable", typeof(bool)).GetValue(customAttrib, null);
                        dataMemberAttrib.EmitDefaultValue = (bool)customAttrib.GetType().GetProperty("EmitDefaultValue", typeof(bool)).GetValue(customAttrib, null);
                        dataMemberAttrib.Order = (int)customAttrib.GetType().GetProperty("Order", typeof(int)).GetValue(customAttrib, null);
                        break;
                    }
                }
                
            }
            return dataMemberAttrib;
        }

        /// <summary>
        /// For each field in a given type, add a MemberInfo object any field marked with a DataMember attibute.
        /// </summary>
        /// <param name="t">A reflected data type object.</param>
        /// <returns>An array of MemberInfo object that are marked with the DataMember attribute.</returns>
        private MemberInfo[] GetDataMembers(Type t)
        {
            List<MemberInfo> dataMemberMethods = new List<MemberInfo>();
            MemberInfo[] members = t.GetMembers();
            foreach (MemberInfo member in members)
            {
                object[] customAttribs = member.GetCustomAttributes(true);
                foreach (Attribute attrib in customAttribs)
                {
                    switch (((Type)attrib.TypeId).Name)
                    {
                        case "DataMemberAttribute":
                        case "DataMember":
                            dataMemberMethods.Add(member);
                            break;
                        case "EnumMemberAttribute":
                        case "EnumMember":
                            dataMemberMethods.Add(member);
                            break;
                        default:
                            break;
                    }
                }
            }
            members = null;
            members = new MemberInfo[dataMemberMethods.Count];
            dataMemberMethods.CopyTo(members);
            return (members.Length > 0 ? members : null);
        }

        /// <summary>
        /// Adds a schema item to the schema if the item is not a duplicate.
        /// </summary>
        /// <param name="schema">A Schema that the item may be added to.</param>
        /// <param name="item">A schema item to add.</param>
        /// <returns></returns>
        private bool AddSchemaItem(XmlSchema schema, XmlSchemaObject item)
        {
            bool conflictingTypes = false;
            bool isDuplicate = false;
            string itemName = null;
            if (item is XmlSchemaElement)
                itemName = ((XmlSchemaElement)item).Name;
            else if (item is XmlSchemaComplexType)
                itemName = ((XmlSchemaComplexType)item).Name;
            else if (item is XmlSchemaSimpleType)
                itemName = ((XmlSchemaSimpleType)item).Name;
            else
            {
                Logger.WriteLine("        Unsupported schema type " + item.GetType().ToString() + " detected.", LogLevel.Verbose);
                return false;
            }

            foreach (XmlSchemaObject obj in schema.Items)
            {
                if (obj is XmlSchemaElement)
                {
                    if (((XmlSchemaElement)obj).Name == itemName)
                    {
                        if (item is XmlSchemaElement)
                        {
                            XmlSchemaElement element = (XmlSchemaElement)item;
                            if (element.SchemaTypeName != ((XmlSchemaElement)obj).SchemaTypeName)
                                conflictingTypes = true;
                            isDuplicate = true;
                        }
                        else
                            conflictingTypes = true;    
                        break;
                    }
                }
                else if (obj is XmlSchemaSimpleType)
                {
                    if (((XmlSchemaSimpleType)obj).Name == itemName)
                    {
                        if (item is XmlSchemaSimpleType)
                            isDuplicate = true;
                        else
                            conflictingTypes = true;
                        break;
                    }
                }
                else if (obj is XmlSchemaComplexType)
                {
                    if (((XmlSchemaComplexType)obj).Name == itemName)
                    {
                        if (item is XmlSchemaComplexType)
                            isDuplicate = true;
                        else
                            conflictingTypes = true;
                        break;
                    }
                }
            }
            
            // If conflicting type names are detected throw
            if (conflictingTypes)
            {
                string errMsg = "\nException: Conflicting parameter \"" + itemName + "\" detected in schema " + schema.TargetNamespace + ".";
                      errMsg += "Multiple operation parameters with the same name that reference different types results in invalid schema.";
                      errMsg += "Change the name of this parameter in code.";
                throw new Exception(errMsg);
            }

            // If this is a new item add it to the schema
            if (!isDuplicate)
                    schema.Items.Add(item);
                else
                    Logger.WriteLine("        Duplicate parameter \"" + itemName + "\" detected in schema " + schema.TargetNamespace + ". Duplicate was discarded.", LogLevel.Normal);

            return isDuplicate;
        }

        /// <summary>
        /// For a given type, generate a complex schema element. Recursively generate a complex type for any non
        /// native type referenced by this type. 
        /// </summary>
        /// <param name="members">An array of MemberInfo objects that contain the reflected details of the sub
        /// types continined within this type.</param>
        /// <param name="element">A reference to an XmlElement used to store the new element information.</param>
        /// <param name="schema">A Schema object used to store the new element.</param>
        /// <param name="schemaNamespace">A string containing the Schemas targetnamespace.</param>
        /// <param name="serviceDesc"></param>
        void ProcessDataMemberType(MemberInfo[] members, ref XmlSchemaElement element, XmlSchema schema, string schemaNamespace, ServiceDescription serviceDesc)
        {
            element.SchemaTypeName = new XmlQualifiedName(element.Name + "Type", schemaNamespace);
  
            if (AddSchemaItem(schema, element))
                return;

            XmlSchemaComplexType complexType = new XmlSchemaComplexType();
            complexType.Name = element.Name + "Type";
            complexType.Particle = new XmlSchemaSequence();
            if (AddSchemaItem(schema, complexType))
                return;

            // If members is null this type is an empty wrapper
            if (members == null)
                return;

            foreach (MemberInfo member in members)
            {
                FieldInfo fieldInfo = (FieldInfo)member;

                DataMemberAttrib dataMemberAttrib = GetDataMemberAttribute(fieldInfo);

                if (fieldInfo.FieldType.FullName != null)
                {
                    // Special handling if the field type is an array
                    bool isArray = false;
                    string fieldTypeName = fieldInfo.FieldType.FullName;
                    if (fieldTypeName.EndsWith("[]"))
                    {
                        isArray = true;
                        fieldTypeName = fieldTypeName.Substring(0, fieldTypeName.Length - 2);
                    }
                    
                    // If this is not a native type recurse and create the base type
                    string xmlTypeName = CodeGenUtils.GetXmlType(fieldTypeName);
                    if (xmlTypeName == null)
                    {
                        XmlSchemaElement elementItem = new XmlSchemaElement();
                        elementItem.RefName = new XmlQualifiedName(fieldInfo.Name, schemaNamespace);
                        elementItem.MinOccurs = 0;
                        elementItem.MaxOccurs = 1;

                        // If the data member has modifiers reset values
                        bool isAttribute = false;
                        if (dataMemberAttrib != null)
                        {
                            elementItem.MinOccurs = dataMemberAttrib.IsRequired ? 1 : 0;
                            if (dataMemberAttrib.Name != null)
                                elementItem.RefName = new XmlQualifiedName(dataMemberAttrib.Name, schemaNamespace);
                            isAttribute = dataMemberAttrib.IsAttribute;
                        }
                        
                        if (isArray)
                        {
                            string fieldName = fieldInfo.FieldType.Name;
                            fieldName = fieldName.Substring(0, fieldName.Length - 2);
                            ProcessDataContract(fieldInfo.FieldType.GetElementType(), schemaNamespace, serviceDesc);
                            elementItem.MaxOccursString = "unbounded";
                        }
                        else
                        {
                            ProcessDataContract(fieldInfo.FieldType, schemaNamespace, serviceDesc);
                        }

                        // If order is set insert element at order position
                        if (dataMemberAttrib != null && dataMemberAttrib.Order >= 0)
                            ((XmlSchemaSequence)complexType.Particle).Items.Insert(dataMemberAttrib.Order, elementItem);
                        else
                            ((XmlSchemaSequence)complexType.Particle).Items.Add(elementItem);
                    }
                    else{

                        Logger.WriteLine("        Field = " + fieldInfo.FieldType.UnderlyingSystemType + " " + fieldInfo.Name, LogLevel.Verbose);
                        XmlSchemaElement elementItem = new XmlSchemaElement();
                        elementItem.Name = fieldInfo.Name;

                        // Process any types
                        if (xmlTypeName == "any")
                        {
                            XmlSchemaAny anyElement = new XmlSchemaAny();
                            anyElement.MinOccurs = 0;
                            anyElement.MaxOccursString = "unbounded";
                            anyElement.Namespace = "##other";
                            anyElement.ProcessContents = XmlSchemaContentProcessing.Lax;
                            ((XmlSchemaSequence)complexType.Particle).Items.Add(anyElement);
                        }
                        else if (xmlTypeName == "anyAttribute")
                        {
                            XmlSchemaAnyAttribute anyAttribute = new XmlSchemaAnyAttribute();
                            anyAttribute.Namespace = "##any";
                            anyAttribute.ProcessContents = XmlSchemaContentProcessing.Lax;
                            complexType.AnyAttribute = anyAttribute;
                        }
                        else
                        {
                            elementItem.MinOccurs = 1;
                            elementItem.MaxOccurs = 1;
                            if (dataMemberAttrib != null)
                            {
                                if (dataMemberAttrib.IsAttribute)
                                {
                                    XmlSchemaAttribute attrib = new XmlSchemaAttribute();
                                    attrib.SchemaTypeName = new XmlQualifiedName(xmlTypeName, "http://www.w3.org/2001/XMLSchema");
                                    attrib.Name = fieldInfo.FieldType.Name;
                                    if (dataMemberAttrib.IsRequired)
                                        attrib.Use = XmlSchemaUse.Required;
                                    return;
                                }
                                elementItem.MinOccurs = dataMemberAttrib.IsRequired ? 1 : 0;
                                elementItem.IsNillable = dataMemberAttrib.IsNillable;
                            }
                            
                            // If the type is an array create a simple List type before adding the element particle
                            if (isArray && xmlTypeName != "base64Binary")
                            {
                                XmlSchemaSimpleType simpleType = new XmlSchemaSimpleType();
                                simpleType.Name = fieldInfo.Name + "ListType";
                                XmlSchemaSimpleTypeList simpleContent = new XmlSchemaSimpleTypeList();
                                string itemTypeName = fieldInfo.FieldType.FullName;
                                itemTypeName = itemTypeName.Substring(0, itemTypeName.Length - 2);
                                simpleContent.ItemTypeName = new XmlQualifiedName(CodeGenUtils.GetXmlType(itemTypeName), "http://www.w3.org/2001/XMLSchema");
                                simpleType.Content = simpleContent;
                                AddSchemaItem(schema, simpleType);
                                elementItem.SchemaTypeName = new XmlQualifiedName(simpleType.Name, schemaNamespace);
                            }
                            else
                                elementItem.SchemaTypeName = new XmlQualifiedName(xmlTypeName, "http://www.w3.org/2001/XMLSchema");
                            ((XmlSchemaSequence)complexType.Particle).Items.Add(elementItem);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// For a given enum type, generate a simple schema type. 
        /// </summary>
        /// <param name="t">A reflected enum data type.</param>
        /// <param name="schema">A Schema object used to store the new simple type.</param>
        /// <param name="schemaNamespace">A string containing the Schemas targetnamespace.</param>
        /// <param name="serviceDesc"></param>
        void ProcessEnumMemberType(Type t, XmlSchema schema, string schemaNamespace, ServiceDescription serviceDesc)
        {
            // Create the type
            XmlSchemaSimpleType simpleType = new XmlSchemaSimpleType();
            simpleType.Name = t.Name + "Type";
            XmlSchemaSimpleTypeRestriction restriction = new XmlSchemaSimpleTypeRestriction();
            restriction.BaseTypeName = new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema");
            simpleType.Content = restriction;

            // Create an element used to reference the type
            XmlSchemaElement element = new XmlSchemaElement();
            element.Name = t.Name;
            element.SchemaTypeName = new XmlQualifiedName(simpleType.Name, schemaNamespace);

            // Iterate list of enum members and create schema elements and types
            MemberInfo[] members = GetDataMembers(t);

            foreach (MemberInfo member in members)
            {
                XmlSchemaEnumerationFacet enumFacet = new XmlSchemaEnumerationFacet();
                enumFacet.Value = member.Name;
                restriction.Facets.Add(enumFacet);
            }
            AddSchemaItem(schema, element);
            AddSchemaItem(schema, simpleType);
        }

        /// <summary>
        /// Add Dpws Policy element to service description
        /// </summary>
        /// <param name="serviceDesc">A Service Description.</param>
        /// <param name="hasEvents">A flag indicating that the Service Description has events.</param>
        /// <param name="hasOptimizedMimeContent">A flag indicating a Service Description uses Optimized Mime encoding.</param>
        void AddPolicyBindingElement(ServiceDescription serviceDesc, bool hasEvents, bool hasOptimizedMimeContent)
        {
            string dpwsNs = "http://schemas.xmlsoap.org/ws/2006/02/devprof";
            string policyNs = "http://schemas.xmlsoap.org/ws/2004/09/policy";
            string mtomNs = "http://schemas.xmlsoap.org/ws/2004/09/policy/optimizedmimeserialization";
            
            // Create a policy element
            XmlDocument xDoc = new XmlDocument();
            XmlElement policy = xDoc.CreateElement("po", "Policy", policyNs);
            XmlAttribute policyAttrib = (XmlAttribute)xDoc.CreateAttribute("u", "Id", null);
            policyAttrib.Value = serviceDesc.Name + "Policy";
            policy.Attributes.Append(policyAttrib);
            policy.AppendChild(xDoc.CreateElement("p", "profile", dpwsNs));
            if (hasEvents)
            {
                policy.AppendChild(xDoc.CreateElement("p", "PushDelivery", dpwsNs));
                policy.AppendChild(xDoc.CreateElement("p", "DurationExpiration", dpwsNs));
                policy.AppendChild(xDoc.CreateElement("p", "ActionFilter", dpwsNs));
            }
            if (hasOptimizedMimeContent)
            {
                policy.AppendChild(xDoc.CreateElement("om", "OptimizedMimeSerialization", mtomNs));
            }

            // Add Dpws Eventing policy in case a port type has a callback extension
            serviceDesc.Extensions.Add(policy);
        }
    }
}
