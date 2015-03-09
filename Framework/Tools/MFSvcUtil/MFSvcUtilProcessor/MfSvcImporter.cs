using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Web.Services.Description;
using System.IO;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Reflection;
using Ws.SvcUtilCodeGen;

namespace Ws.SvcImporter
{
    /// <summary>
    /// Contains methods used to generate DataContracts, DataContractSerailizers, HostedServices and Client proxies
    /// from a Wsdl files.
    /// </summary>
    public class MFSvcImporter
    {
        ServiceDescription m_svcDesc = null;
        ServiceDescriptionImporter m_svcDescImporter = new ServiceDescriptionImporter();
        DCCodeGen m_dcCodeGen = new DCCodeGen();
        private TargetPlatform m_platform = TargetPlatform.MicroFramework;
        private bool isParsing = false;

        // Flag used to enforce ws-i message binding sytle conformance
        public bool WsiCompliant { set { if (!isParsing) m_dcCodeGen.WsiCompliant = value; } }

        // Flag used to enable or disable parsing of Rpc sytle bindings
        public bool RpcBindingStyleSupport { set { if (!isParsing) m_dcCodeGen.RpcBindingStyleSupport = value; } }

        /// <summary>
        /// MFSvcImporter constructor.
        /// </summary>
        public MFSvcImporter()
        {
        }

        /// <summary>
        /// Open specified Wsdl file and parse content into ServiceDescription hierarchy. Method attempts
        /// to Import external schema references.
        /// </summary>
        /// <param name="fileName">A valid Wsdl file name.</param>
        /// <exception cref="FileNotFoundException">If specified file is cannot be opened.</exception>
        public void ParseWsdl(string fileName, List<string> schemaRefs)
        {
            isParsing = true;
            Logger.WriteLine("Reading WSDL file: " + fileName, LogLevel.Normal);

            // Use a validating xml reader to validate the schema. If it checks out
            // we only have to worry about building code fragements and data contracts.
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationFlags = XmlSchemaValidationFlags.AllowXmlAttributes |
                XmlSchemaValidationFlags.ProcessIdentityConstraints |
                XmlSchemaValidationFlags.ProcessInlineSchema |
                XmlSchemaValidationFlags.ProcessSchemaLocation |
                XmlSchemaValidationFlags.ReportValidationWarnings;

            // Create the reader
            settings.ValidationEventHandler += new ValidationEventHandler(Settings_ValidationEventHandler);

            XmlReader reader = XmlReader.Create(fileName, settings);

            if (!ServiceDescription.CanRead(reader)) throw new InvalidOperationException("The WSDL file is not compatible.");

            // Parse the Wsdl
            m_svcDesc = ServiceDescription.Read(reader, true);
            reader.Close();

            List<string> importFiles = new List<string>();
            importFiles.Add(fileName);

            ParseWsdl(m_svcDesc, schemaRefs, settings, importFiles);
        }

        private void ParseWsdl(ServiceDescription svcDesc, List<string> schemaRefs, XmlReaderSettings settings, List<string> importFiles)
        {
            foreach (Import imp in svcDesc.Imports)
            {
                string fileName = imp.Location;
                XmlReader reader;

                // Create the reader
                settings.ValidationEventHandler += new ValidationEventHandler(Settings_ValidationEventHandler);

                if (!importFiles.Contains(fileName))
                {
                    reader = XmlReader.Create(fileName, settings);
                    ServiceDescription sd = ServiceDescription.Read(reader, true);

                    for(int i=0; i<sd.Bindings.Count;i++)
                    {
                        m_svcDesc.Bindings.Add(sd.Bindings[i]);
                    }
                    reader.Close();

                    importFiles.Add(fileName);
                    ParseWsdl(sd, schemaRefs, settings, importFiles);
                }
            }

            // Add Import schemas
            foreach (XmlElement element in svcDesc.Types.Extensions)
            {
                string schemaNs = null;
                string schemaLocation = null;
                if (element.LocalName == "import")
                {
                    foreach (XmlAttribute attribute in element.Attributes)
                    {
                        if (attribute.LocalName == "schemaLocation")
                            schemaLocation = attribute.Value;
                        else if (attribute.LocalName == "namespace")
                            schemaNs = attribute.Value;
                    }


                    schemaNs = schemaNs == null ? svcDesc.TargetNamespace : schemaNs;

                    // If schemaLocation attribute is null check for external command line reference
                    if (schemaLocation == null)
                    {
                        XmlSchema extSchema = GetExternalSchema(schemaNs, schemaRefs);
                        if (extSchema == null)
                        {
                            throw new Exception("Invalid schema import element. Schema location for namespace" +
                                schemaNs +
                                "could not be resolved.");
                        }
                        else
                        {
                            Logger.WriteLine("Importing external schema. ", LogLevel.Normal);
                            m_svcDesc.Types.Schemas.Add(extSchema);
                        }
                    }
                    else
                    {
                        Logger.WriteLine("Importing Schema: " + schemaLocation, LogLevel.Normal);
                        XmlReader reader = XmlReader.Create(schemaLocation, settings);
                        XmlSchema xmlSchema = XmlSchema.Read(reader, null);
                        m_svcDesc.Types.Schemas.Add(xmlSchema);
                        reader.Close();
                    }
                }

            }

            XmlSchemas schemas = new XmlSchemas();
            foreach (System.Xml.Schema.XmlSchema wsdlSchema in svcDesc.Types.Schemas)
            {
                foreach (System.Xml.Schema.XmlSchemaObject externalSchema in wsdlSchema.Includes)
                {
                    if (externalSchema is System.Xml.Schema.XmlSchemaInclude)
                    {
                        string schemaLocation = ((XmlSchemaInclude)externalSchema).SchemaLocation;

                        // If schemaLocation attribute is null check for external command line reference
                        if (schemaLocation == null)
                        {
                            XmlSchema extSchema = GetExternalSchema(wsdlSchema.TargetNamespace, schemaRefs);
                            if (extSchema == null)
                            {
                                throw new Exception("Invalid schema include element. Schema location for namespace \"" +
                                    wsdlSchema.TargetNamespace +
                                    "\" could not be resolved.");
                            }
                            else
                            {
                                Logger.WriteLine("Including external schema. ", LogLevel.Normal);
                                schemas.Add(extSchema);
                            }
                        }
                        else
                        {
                            Logger.WriteLine("Including Schema: " + schemaLocation, LogLevel.Normal);
                            XmlReader reader = XmlReader.Create(schemaLocation, settings);
                            XmlSchema xmlSchema = XmlSchema.Read(reader, null);
                            schemas.Add(xmlSchema);
                            reader.Close();
                        }
                    }
                    else if (externalSchema is System.Xml.Schema.XmlSchemaImport)
                    {
                        string schemaLocation = ((XmlSchemaImport)externalSchema).SchemaLocation;

                        // If schemaLocation attribute is null check for external command line reference
                        if (schemaLocation == null)
                        {
                            XmlSchema extSchema = GetExternalSchema(((XmlSchemaImport)externalSchema).Namespace, schemaRefs);
                            if (extSchema == null)
                            {
                                throw new Exception("Invalid schema import element. Schema location for namespace \"" +
                                    ((XmlSchemaImport)externalSchema).Namespace +
                                    "\" could not be resolved.");
                            }
                            else
                            {
                                Logger.WriteLine("Importing external schema. ", LogLevel.Normal);
                                schemas.Add(extSchema);
                            }
                        }
                        else
                        {
                            Logger.WriteLine("Importing Schema: " + schemaLocation, LogLevel.Normal);
                            XmlReader reader = XmlReader.Create(schemaLocation, settings);
                            XmlSchema xmlSchema = XmlSchema.Read(reader, null);
                            schemas.Add(xmlSchema);
                            reader.Close();
                        }
                    }
                }
            }

            if (schemas.Count > 0)
            {
                foreach (XmlSchema schema in schemas)
                    m_svcDesc.Types.Schemas.Add(schema);
            }
        }

        /// <summary>
        /// Like Svcuilt, MfSvcUtil can accept external .xsd command line references in place of the
        /// schemaLocation attribute on an import or include element. If the schemaLocation attribute is not present
        /// this method checks command line schema references for a schema that matches the Import or target namespace
        /// </summary>
        /// <param name="targetNs">A string containing the schemas target namespace.</param>
        /// <param name="schemaRefs">A List of schema (xsd) files passed on the command line.</param>
        /// <returns>A valid XmlSchema object if an external schema is found and validated.</returns>
        private XmlSchema GetExternalSchema(string targetNs, List<string> schemaRefs)
        {
            // If no command line schema references were found return null here
            if (schemaRefs == null)
                return null;
            
            // Use a validating xml reader to validate the schema.
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationFlags = XmlSchemaValidationFlags.AllowXmlAttributes |
                XmlSchemaValidationFlags.ProcessIdentityConstraints |
                XmlSchemaValidationFlags.ProcessInlineSchema |
                XmlSchemaValidationFlags.ProcessSchemaLocation |
                XmlSchemaValidationFlags.ReportValidationWarnings;
            XmlReader reader;

            foreach (string schemaRef in schemaRefs)
            {
                reader = XmlReader.Create(schemaRef, settings);
                XmlSchema xmlSchema = XmlSchema.Read(reader, null);
                if (xmlSchema.TargetNamespace == targetNs)
                {
                    Logger.WriteLine("External Schema located: " + schemaRef, LogLevel.Normal);
                    schemaRefs.Remove(schemaRef);
                    reader.Close();
                    return xmlSchema;
                }
                reader.Close();
            }
            return null;
        }
        
        void Settings_ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            Logger.WriteLine(e.Message, LogLevel.Verbose);
            Logger.WriteLine("Severity: " + e.Severity, LogLevel.Verbose);
            Logger.WriteLine("", LogLevel.Verbose);
            if (e.Exception != null)
            {
                Logger.WriteLine("Exception: " + e.Exception, LogLevel.Verbose);
                Logger.WriteLine("SourceUri: " + e.Exception.SourceUri, LogLevel.Verbose);
                Logger.WriteLine("LineNumber: " + e.Exception.LineNumber.ToString(), LogLevel.Verbose);
                Logger.WriteLine("Position: " + e.Exception.LinePosition.ToString(), LogLevel.Verbose);
                Logger.WriteLine("StackTrace: " + e.Exception.StackTrace, LogLevel.Verbose);
            }
        }

        /// <summary>
        /// Parse Wsdl Schema and generate DataContract, DataContractSerializer types,
        /// HostedServices and Client Proxies.
        /// </summary>
        /// <remarks>Currently only generates C# source files.</remarks>
        /// <param name="contractFilename">The name of a contract source code (.cs) file.</param>
        /// <param name="hostedServiceFilename">The name of a hosted service source code (.cs) file.</param>
        /// <param name="clientProxyFilename">The name of a client proxy source code (.cs) file.</param>
        public void CreateSourceFiles(string contractFilename, string hostedServiceFilename, string clientProxyFilename)
        {
            CreateSourceFiles(contractFilename, hostedServiceFilename, clientProxyFilename, TargetPlatform.MicroFramework);
        }

        /// <summary>
        /// CreateSourceFiles - Parse Wsdl Schema and generate DataContract, DataContractSerializer types,
        /// HostedServices and Client Proxies.
        /// </summary>
        /// <remarks>Currently only generates C# source files.</remarks>
        /// <param name="contractFilename">The name of a contract source code (.cs) file.</param>
        /// <param name="hostedServiceFilename">The name of a hosted service source code (.cs) file.</param>
        /// <param name="clientProxyFilename">The name of a client proxy source code (.cs) file.</param>
        /// <param name="targetPlatform">Specifies the target runtime platform.</param>
        public void CreateSourceFiles(string contractFilename, string hostedServiceFilename, string clientProxyFilename, TargetPlatform targetPlatform)
        {
            m_platform = targetPlatform;

            Logger.WriteLine("", LogLevel.Normal);
            Logger.WriteLine("Generating contract source: " + contractFilename + "...", LogLevel.Normal);

            if (contractFilename == null)
                throw new ArgumentNullException("codeFilename", "You must pass a valid code filename.");

            if (m_svcDesc.Types == null)
            {
                throw new Exception("No wsdl types found.");
            }

            string path = Path.GetDirectoryName(contractFilename).Trim();

            if(!string.IsNullOrEmpty(path) && !Directory.Exists(path)) 
            {
                Directory.CreateDirectory(path);
            }

            // Create code file stream
            FileStream dcStream = new FileStream(contractFilename, FileMode.Create, FileAccess.Write, FileShare.None);
            StreamWriter dcStreamWriter = new StreamWriter(dcStream);

            // Write the auto generated header
            dcStreamWriter.Write(AutoGenTextHeader.Message);

            try
            {

                // Set up data contract code generator
                CSharpCodeProvider cSharpCP = new CSharpCodeProvider();
                ICodeGenerator codeGen = cSharpCP.CreateGenerator(dcStreamWriter);
                CodeGeneratorOptions codeGenOptions = new CodeGeneratorOptions();
                codeGenOptions.BracingStyle = "C";

                // Cobble up a valid .net namespace. Turn any progression that's not a-z or A-Z to a single '.'
                string targetNamespaceName = CodeGenUtils.GenerateDotNetNamespace(m_svcDesc.TargetNamespace);

                // For some reason we have to force schemas to compile. Though it was suppose to automatically. Huh!
                foreach (XmlSchema schema in m_svcDesc.Types.Schemas)
                {
                    XmlSchemaSet schemaSet = new XmlSchemaSet();
                    schemaSet.Add(schema);
                    schemaSet.Compile();
                }
                

                // Create new code namespace
                CodeNamespace targetNamespace = new CodeNamespace(targetNamespaceName);

                // Add data contract using directives
                CodeSnippetCompileUnit compileUnit = new CodeSnippetCompileUnit("using System;");
                codeGen.GenerateCodeFromCompileUnit(compileUnit, dcStreamWriter, codeGenOptions);
                compileUnit.Value = "using System.Xml;";
                codeGen.GenerateCodeFromCompileUnit(compileUnit, dcStreamWriter, codeGenOptions);
                if (m_platform == TargetPlatform.MicroFramework)
                {
                    compileUnit.Value = "using System.Ext;";
                    codeGen.GenerateCodeFromCompileUnit(compileUnit, dcStreamWriter, codeGenOptions);
                    compileUnit.Value = "using System.Ext.Xml;";
                    codeGen.GenerateCodeFromCompileUnit(compileUnit, dcStreamWriter, codeGenOptions);
                }
                compileUnit.Value = "using Ws.ServiceModel;";
                codeGen.GenerateCodeFromCompileUnit(compileUnit, dcStreamWriter, codeGenOptions);
                compileUnit.Value = "using Ws.Services.Mtom;";
                codeGen.GenerateCodeFromCompileUnit(compileUnit, dcStreamWriter, codeGenOptions);
                compileUnit.Value = "using Ws.Services.Serialization;";
                codeGen.GenerateCodeFromCompileUnit(compileUnit, dcStreamWriter, codeGenOptions);
                compileUnit.Value = "using XmlElement = Ws.Services.Xml.WsXmlNode;";
                codeGen.GenerateCodeFromCompileUnit(compileUnit, dcStreamWriter, codeGenOptions);
                compileUnit.Value = "using XmlAttribute = Ws.Services.Xml.WsXmlAttribute;";
                codeGen.GenerateCodeFromCompileUnit(compileUnit, dcStreamWriter, codeGenOptions);
                compileUnit.Value = "using XmlConvert = Ws.Services.Serialization.WsXmlConvert;";
                codeGen.GenerateCodeFromCompileUnit(compileUnit, dcStreamWriter, codeGenOptions);
                compileUnit.Value = "";
                codeGen.GenerateCodeFromCompileUnit(compileUnit, dcStreamWriter, codeGenOptions);
                compileUnit.Namespaces.Add(targetNamespace);
                m_dcCodeGen.CodeNamespaces = compileUnit.Namespaces;

                Logger.WriteLine("", LogLevel.Normal);

                // Create HostedServices and ClientProxies collections
                HostedServices hostedServices = new HostedServices(targetNamespaceName);
                ClientProxies clientProxies = new ClientProxies(targetNamespaceName);

                // For each PortType process
                foreach (PortType portType in m_svcDesc.PortTypes)
                {
                    // For each operation in the port type:
                    // Get input and output message parts.
                    // If the message part is a simple type:
                    //   Build HostedService operation.
                    // Else if the message part is an element:
                    //   Find elements in Schema
                    //   If element type is native xml type:
                    //     Build HostedService operation.
                    //   Else if element references a simple or complex type:
                    //     If simpleType is base xml type with restrictions:
                    //       Build HostedService operation.
                    //     Else
                    //       Build DataContract and DataContractSerializer.
                    //       Build HostedService Operation.
                    //     

                    if (!CodeGenUtils.IsSoapBinding(portType, m_svcDesc))
                    {
                        continue;
                    }

                    // Create instance of a HostedService to hold the port type details
                    HostedService hostedService = new HostedService(portType.Name, m_svcDesc.TargetNamespace, m_platform);

                    // Create instance of ClientProxyGenerator
                    ClientProxy clientProxy = new ClientProxy(portType.Name, m_platform);

                    // Create service contract interface
                    CodeTypeDeclaration serviceCodeType = new CodeTypeDeclaration("I" + portType.Name);
                    CodeAttributeArgument codeAttr = new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(m_svcDesc.TargetNamespace));
                    CodeAttributeDeclaration codeAttrDecl = new CodeAttributeDeclaration("ServiceContract", codeAttr);
                    serviceCodeType.CustomAttributes.Add(codeAttrDecl);

                    // Check for Policy assertions. If found add policy assertion attributes. Policy assertion attributes
                    // are required to regenerate policy assertions when converting a service to Wsdl.
                    List<PolicyAssertion> policyAssertions = GetPolicyAssertions();
                    bool OptimizedMimeEncoded = false;
                    foreach (PolicyAssertion assert in policyAssertions)
                    {
                        serviceCodeType.CustomAttributes.Add(CreatePolicyAssertions(assert.Name, assert.Namespace.ToString(), assert.PolicyID));
                    
                        // if Optimized Mime assertion id found set a processing flag
                        if (assert.Name == "OptimizedMimeSerialization")
                        {
                            OptimizedMimeEncoded = true;
                        }
                    }

                    // Add type declaration
                    serviceCodeType.TypeAttributes = TypeAttributes.Public;
                    serviceCodeType.IsInterface = true;

                    // Create service contract callback client interface
                    CodeTypeDeclaration serviceCallbackCodeType = new CodeTypeDeclaration("I" + portType.Name + "Callback");

                    // Add type declaration
                    serviceCallbackCodeType.TypeAttributes = TypeAttributes.Public;
                    serviceCallbackCodeType.IsInterface = true;

                    // If the binding contains a ref to Mtom encoding type set the Mtom flag
                    if (OptimizedMimeEncoded)
                    {
                        m_dcCodeGen.EncodingType = MessageEncodingType.Mtom;
                        hostedService.EncodingType = MessageEncodingType.Mtom;
                        clientProxy.EncodingType = MessageEncodingType.Mtom;
                    }

                    // Step through port operations, get method names and parse elements.
                    for (int pt_index = 0; pt_index < portType.Operations.Count; ++pt_index)
                    {
                        Operation operation = portType.Operations[pt_index];
                        string operationName = operation.Name;

                        string inputMessageName = null;
                        string outputMessageName = null;
                        MessagePartCollection inputMessageParts = null;
                        MessagePartCollection outputMessageParts = null;
                        string inAction = null;
                        string outAction = null;
                        GetAction(portType, operation, m_svcDesc.TargetNamespace, ref inAction, ref outAction);

                        // Oneway request port type
                        if (operation.Messages.Flow == OperationFlow.OneWay)
                        {
                            OperationInput input = operation.Messages.Input;
                            inputMessageName = input.Message.Name;

                            // Add operation for HostedService code generation
                            hostedService.AddOperation(operation, inAction, outAction);

                            // Add method for ClientProxy code generation
                            clientProxy.AddOperation(operation, inAction, outAction);
                        }
                        // Twoway request/response pattern
                        else if (operation.Messages.Flow == OperationFlow.RequestResponse)
                        {
                            OperationInput input = operation.Messages.Input;
                            inputMessageName = input.Message.Name;
                            OperationOutput output = operation.Messages.Output;
                            outputMessageName = output.Message.Name;

                            // Add operation for HostedService code generation
                            hostedService.AddOperation(operation, inAction, outAction);

                            // Add method for ClientProxy code generation
                            clientProxy.AddOperation(operation, inAction, outAction);
                        }
                        // Event pattern
                        else if (operation.Messages.Flow == OperationFlow.Notification)
                        {
                            OperationOutput output = operation.Messages.Output;
                            outputMessageName = output.Message.Name;

                            // Add operation for HostedService code generation
                            hostedService.AddOperation(operation, inAction, outAction);

                            // Add method for ClientProxy code generation
                            clientProxy.AddOperation(operation, inAction, outAction);
                        }

                        // Find input and output message parts collection in messages collection
                        // and store for later.
                        foreach (Message message in m_svcDesc.Messages)
                        {
                            if (inputMessageName != null)
                                if (message.Name == inputMessageName)
                                {
                                    inputMessageParts = message.Parts;

                                    // Add operation for HostedService code generation
                                    hostedService.Messages.Add(message);

                                    // Add Message to ClientProxy generator for later
                                    clientProxy.Messages.Add(message);
                                }

                            if (outputMessageName != null)
                                if (message.Name == outputMessageName)
                                {
                                    outputMessageParts = message.Parts;

                                    // Add operation for HostedService code generation
                                    hostedService.Messages.Add(message);

                                    // Add Message to ClientProxy generator for later
                                    clientProxy.Messages.Add(message);
                                }
                        }

                        try
                        {
                            // Try to generate Data Contracts and DataContractSerializers
                            GenerateTypeContracts(operation, inputMessageParts, outputMessageParts);

                            // If operation flow is notification (event) add OperationContract to ServiceContractCallback
                            // else add OperationContract to ServiceContract
                            if (operation.Messages.Flow == OperationFlow.Notification)
                            {
                                AddServiceOperationToInterface(operation, outAction, serviceCallbackCodeType);
                            }
                            else
                                AddServiceOperationToInterface(operation, inAction, serviceCodeType);
                        }
                        catch (Exception e)
                        {
                            dcStreamWriter.Close();
                            File.Delete(contractFilename);
                            Logger.WriteLine("Failed to generate service code. " + e.Message, LogLevel.Normal);
                            return;
                        }
                    }

                    // Add serviceCodeType Service Contract interface to namespace
                    // A serviceCodeType is added even if the wsdl only contains notifications. In that case
                    // the contract will be empty but the ServiceContract attribute and CallbackContract argument
                    // will be used to point to the notification or callback contract interace
                    targetNamespace.Types.Add(serviceCodeType);

                    // If service contract callback type contains members add callback contract to namespace
                    // and add CallbackContract reference attribute to serviceCodeType contract.
                    if (serviceCallbackCodeType.Members.Count > 0)
                    {
                        // Add the callback argument to the service description attribute
                        CodeAttributeArgument callbackArg = new CodeAttributeArgument("CallbackContract",
                            new CodeTypeOfExpression(serviceCallbackCodeType.Name)
                        );
                        serviceCodeType.CustomAttributes[0].Arguments.Add(callbackArg);

                        // Add the callback interface to namespace
                        targetNamespace.Types.Add(serviceCallbackCodeType);
                    }

                    // If the hosted service has opeations add to Hosted Services collection for Code Gen
                    if (hostedService.ServiceOperations.Count > 0)
                        hostedServices.Add(hostedService);

                    // If the client Proxy service has opeations add to client proxy collection for Code Gen
                    if (clientProxy.ServiceOperations.Count > 0)
                        clientProxies.Add(clientProxy);
                }

                // MOD: 12-02-08 Added code to handle multiple type namespaces
                // Generate contract source file
                foreach (CodeNamespace codeNamespace in compileUnit.Namespaces)
                {
                    codeGen.GenerateCodeFromNamespace(codeNamespace, dcStreamWriter, codeGenOptions);
                }
                dcStreamWriter.Flush();
                dcStreamWriter.Close();

                // Generate Hosted Service code
                Logger.WriteLine("Generating Hosted Service source: " + hostedServiceFilename + "...", LogLevel.Normal);
                HostedServiceGenerator hsGen = new HostedServiceGenerator();
                hsGen.GenerateCode(hostedServiceFilename, hostedServices);

                // Generate Client proxy code
                Logger.WriteLine("Generating Client Proxy source: " + clientProxyFilename + "...", LogLevel.Normal);
                ClientProxyGenerator cpGen = new ClientProxyGenerator();
                cpGen.GenerateCode(clientProxyFilename, clientProxies);
            }
            catch (Exception e)
            {
                dcStreamWriter.Close();
                File.Delete(contractFilename);
                Logger.WriteLine("Failed to generate service code. " + e.Message, LogLevel.Normal);
                throw new Exception("Failed to generate service code. ", e);
            }
        }

        CodeAttributeDeclaration CreatePolicyAssertions(string name, string ns, string policyID)
        {
            CodeAttributeArgument[] codeAttrArguments = new CodeAttributeArgument[3];
            codeAttrArguments[0] = new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(ns));
            codeAttrArguments[1] = new CodeAttributeArgument("Name", new CodePrimitiveExpression(name));
            codeAttrArguments[2] = new CodeAttributeArgument("PolicyID", new CodePrimitiveExpression(policyID));
            return new CodeAttributeDeclaration("PolicyAssertion", codeAttrArguments);
        }
        
        /// <summary>
        /// Parses the action attribute based on rules of Web Services Addressing 1.0 - WSDL Binding specification
        /// </summary>
        /// <param name="portType">A Web Service port type collection.</param>
        /// <param name="operation">A specific port type operation.</param>
        /// <param name="inAction">A string used to return the action uri.</param>
        /// <param name="outAction">A string used to return the out action uri.</param>
        /// <remarks>
        /// Action rules:
        /// If portType/operation/Input or output has addressing:Action attribute use it
        /// else If soap/binding/operation has a soap12:operation element containing a soapAction atribute use it
        /// else if portType/operation/input has a name attribute - Action = target namespace/port_type_name/input|output name
        /// else Action = target namespace/port_type_name/message_type_name + Request or Response or Solicit
        ///</remarks>
        private void GetAction(PortType portType, Operation operation, string targetNamespace, ref string inAction, ref string outAction)
        {
            // If portType/operation/Input or output has addressing:Action attribute use it
            if (operation.Messages.Input != null && operation.Messages.Input.ExtensibleAttributes != null)
            {
                foreach (XmlAttribute attribute in operation.Messages.Input.ExtensibleAttributes)
                    if (attribute.LocalName == "Action" && (attribute.NamespaceURI == "http://schemas.xmlsoap.org/ws/2004/08/addressing" || attribute.NamespaceURI == "http://www.w3.org/2006/05/addressing/wsdl"))
                    {
                        inAction = attribute.Value;
                        break;
                    }
            }
            if (operation.Messages.Output != null && operation.Messages.Output.ExtensibleAttributes != null)
            {
                foreach (XmlAttribute attribute in operation.Messages.Output.ExtensibleAttributes)
                    if (attribute.LocalName == "Action" && (attribute.NamespaceURI == "http://schemas.xmlsoap.org/ws/2004/08/addressing" || attribute.NamespaceURI == "http://www.w3.org/2006/05/addressing/wsdl"))
                    {
                        outAction = attribute.Value;
                        break;
                    }
            }

            // If in or out action is null then:
            // If soap/binding/operation has a soap12:operation element containing an Action attribute use it
            string soapAction = GetSoapAction(portType, operation.Name);
            inAction = (inAction == null && soapAction != null) ? soapAction : inAction;
            outAction = (outAction == null && soapAction != null) ? soapAction : outAction;

            if(!targetNamespace.EndsWith("/")) targetNamespace += "/";

            // If in or out action is null then:
            // if portType/operation/input has a name attribute - Action = target namespace/port type name/input|output name
            if (inAction == null)
            {
                if (operation.Messages.Input != null && operation.Messages.Input.Name != null)
                    inAction = targetNamespace + operation.PortType.Name + "/" + operation.Messages.Input.Name;
            }
            if (outAction == null)
            {
                if (operation.Messages.Output != null && operation.Messages.Output.Name != null)
                    outAction = targetNamespace + operation.PortType.Name + "/" + operation.Messages.Output.Name;
            }

            // If in or out action is null then:
            // Action = target namespace/port type name/operation name + Request or Response or Solicit
            if (inAction == null && operation.Messages.Input != null && operation.Messages.Input.Message != null)
            {
                int index = operation.Messages.Input.Message.Name.IndexOf(":");
                string inMessagetypeName = index == -1 ? operation.Messages.Input.Message.Name : operation.Messages.Input.Message.Name.Substring(index);
                inAction = targetNamespace + operation.PortType.Name + "/" + operation.Name; // + "Request";
            }
            if (outAction == null && operation.Messages.Output != null && operation.Messages.Output.Message != null)
            {
                int index = operation.Messages.Output.Message.Name.IndexOf(":");
                string outMessagetypeName = index == -1 ? operation.Messages.Output.Message.Name : operation.Messages.Output.Message.Name.Substring(index);
                outAction = targetNamespace + operation.PortType.Name + "/" + operation.Name + "Response";
            }
        }

        /// <summary>
        /// Helper method used to parse a soap bindings element looking for an operation
        /// with an Action property that defines the soapAction.
        /// </summary>
        /// <param name="portType">Service port type collection.</param>
        /// <returns>Soap action if found.</returns>
        private string GetSoapAction(PortType portType, string operationName)
        {
            string soapAction = null;
            Binding soapBinding = null;
            if (portType.ServiceDescription.Bindings != null)
            {
                foreach (Binding binding in portType.ServiceDescription.Bindings)
                {
                    // If this is the soap12 binding break;
                    if (binding.Extensions.Find(typeof(Soap12Binding)) != null)
                    {
                        if (binding.Type.Name == portType.Name)
                        {
                            soapBinding = binding;
                            break;
                        }
                    }
                }
                if (soapBinding != null)
                {
                    foreach (OperationBinding operationBinding in soapBinding.Operations)
                        if (operationBinding.Name == operationName)
                        {
                            Soap12OperationBinding soapActionBinding = (Soap12OperationBinding)operationBinding.Extensions.Find(typeof(Soap12OperationBinding));
                            if (soapActionBinding != null)
                            {
                                soapAction = soapActionBinding.SoapAction;
                                return soapAction;
                            }
                        }
                }
            }
            return null;
        }

        /// <summary>
        /// Add an interface method to a service interface.
        /// </summary>
        /// <param name="operation">A sericce operation.</param>
        /// <param name="action">A service action</param>
        /// <param name="codeType">A CodeTypeDeclaration object used to store this interface definition.</param>
        private void AddServiceOperationToInterface(Operation operation, string action, CodeTypeDeclaration codeType)
        {
            // Add method
            CodeMemberMethod codeMethod = new CodeMemberMethod();
            codeMethod.Name = operation.Name;

            // Find request and response element or type name that defines the interface method and return types
            string inputElementName = null;
            string inputTypeName = null;
            string outputElementName = null;
            string outputTypeName = null;
            foreach (Message message in m_svcDesc.Messages)
            {
                if (operation.Messages.Input != null && operation.Messages.Input.Message.Name == message.Name)
                {
                    inputElementName = CodeGenUtils.GetMessageElementName(m_svcDesc, message);
                    inputTypeName = CodeGenUtils.GetMessageTypeName(m_svcDesc, message);
                }
                else if (operation.Messages.Output != null && operation.Messages.Output.Message.Name == message.Name)
                {
                    outputElementName = CodeGenUtils.GetMessageElementName(m_svcDesc, message);
                    outputTypeName = CodeGenUtils.GetMessageTypeName(m_svcDesc, message);
                }
            }

            // If this is an event add event (notification) prototype
            if (operation.Messages.Flow == OperationFlow.Notification)
            {
                codeMethod.ReturnType = new CodeTypeReference(typeof(void));
                if (outputTypeName != null)
                    codeMethod.Parameters.Add(new CodeParameterDeclarationExpression(outputTypeName, "resp"));
            }
            // Else add request/response prototype
            else
            {
                if (operation.Messages.Flow != OperationFlow.RequestResponse || outputTypeName == null)
                    codeMethod.ReturnType = new CodeTypeReference(typeof(void));
                else
                    codeMethod.ReturnType = new CodeTypeReference(outputTypeName);

                if (inputTypeName != null)
                    codeMethod.Parameters.Add(new CodeParameterDeclarationExpression(inputTypeName, "req"));
            }

            // Create OperationContract custom attribute
            CodeAttributeDeclaration codeAttrDecl = new CodeAttributeDeclaration("OperationContract");
            CodeAttributeArgument codeAttr = new CodeAttributeArgument("Action", new CodePrimitiveExpression(action));
            codeAttrDecl.Arguments.Add(codeAttr);
            if (operation.Messages.Flow == OperationFlow.OneWay)
            {
                codeAttr = new CodeAttributeArgument("IsOneWay", new CodePrimitiveExpression(true));
                codeAttrDecl.Arguments.Add(codeAttr);
            }
            codeMethod.CustomAttributes.Add(codeAttrDecl);
            codeMethod.Attributes = MemberAttributes.Public;
            codeType.Members.Add(codeMethod);
        }

        /// <summary>
        /// Looks for a Bindings extension policy or policy reference.
        /// </summary>
        /// <returns>An array of policy assertions contained in the service description.</returns>
        private List<PolicyAssertion> GetPolicyAssertions()
        {
            List<PolicyAssertion> assertions = new List<PolicyAssertion>();

            // Iterate the Wsdl Bindings
            foreach (Binding binding in m_svcDesc.Bindings)
            {
                // Check for Policyrefenence Extensions
                XmlElement[] policyRefs = binding.Extensions.FindAll("PolicyReference", "http://schemas.xmlsoap.org/ws/2004/09/policy");

                if (policyRefs.Length > 0)
                {
                    // Foreach policy reference look for a matching policy element
                    foreach (XmlElement element in policyRefs)
                    {
                        // Find the URI attribute that identifies the policy ID
                        foreach (XmlAttribute attrib in element.Attributes)
                            if (attrib.Name == "URI")
                            {
                                // Foreach policy find the one that matches the ID reference
                                XmlElement[] policies = m_svcDesc.Extensions.FindAll("Policy", "http://schemas.xmlsoap.org/ws/2004/09/policy");
                                foreach (XmlElement policy in policies)
                                {
                                    foreach (XmlAttribute policyAttrib in policy.Attributes)
                                    {
                                        if (policyAttrib.LocalName == "Id" && "#" + policyAttrib.Value == attrib.Value)
                                        {
                                            foreach (XmlNode node in policy.ChildNodes)
                                            {
                                                XmlNode assert = node;
                                                bool isNested = false;

                                                while(assert != null && 
                                                    (0 == string.Compare(assert.LocalName, "ExactlyOne", true) || 0 == string.Compare(assert.LocalName, "All", true)) && 
                                                    assert.ChildNodes.Count > 0)
                                                {
                                                    isNested = true;
                                                    assert = assert.ChildNodes[0] as XmlNode;
                                                }

                                                if (isNested)
                                                {
                                                    foreach (XmlNode asrt in assert.ParentNode.ChildNodes)
                                                    {
                                                        if (asrt is XmlNode && asrt.NamespaceURI != "")
                                                        {
                                                            assertions.Add(new PolicyAssertion(asrt.LocalName, new Uri(asrt.NamespaceURI), policyAttrib.Value));
                                                        }
                                                    }
                                                }
                                                else if (assert is XmlNode && assert.NamespaceURI != "")
                                                {
                                                    assertions.Add(new PolicyAssertion(assert.LocalName, new Uri(assert.NamespaceURI), policyAttrib.Value));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                    }
                }
                else
                {
                    XmlElement[] policies = binding.Extensions.FindAll("Policy", "http://schemas.xmlsoap.org/ws/2004/09/policy");
                    foreach (XmlElement policy in policies)
                    {
                        // Look for the special DPWS mime assertion, return true if found
                        Stack<XmlNodeList> nodes = new Stack<XmlNodeList>();

                        nodes.Push(policy.ChildNodes);

                        while (nodes.Count > 0)
                        {
                            // Look for the special DPWS mime assertion, return true if found
                            foreach (XmlNode node in policy.ChildNodes)
                            {
                                XmlNode assert = node;
                                bool isNested = false;

                                while (assert != null &&
                                    (0 == string.Compare(assert.LocalName, "ExactlyOne", true) || 0 == string.Compare(assert.LocalName, "All", true)) &&
                                    assert.ChildNodes.Count > 0)
                                {
                                    assert = assert.ChildNodes[0] as XmlNode;
                                    isNested = true;
                                }

                                if (isNested)
                                {
                                    foreach (XmlNode asrt in assert.ParentNode.ChildNodes)
                                    {
                                        if (asrt is XmlNode && asrt.NamespaceURI != "")
                                        {
                                            assertions.Add(new PolicyAssertion(asrt.LocalName, new Uri(asrt.NamespaceURI), policy.Name));
                                        }
                                    }
                                }
                                else if (assert is XmlNode && assert.NamespaceURI != "")
                                {
                                    assertions.Add(new PolicyAssertion(assert.LocalName, new Uri(assert.NamespaceURI), policy.Name));
                                }
                            }
                        }
                    }
                }
            }

            return assertions;
        }

        /// <summary>
        /// For a given operation, generates a data contract for each element or type used by the operation. 
        /// </summary>
        /// <param name="operation">A port type operation.</param>
        /// <param name="inputMessageParts">A collection of input message parts used by this operation.</param>
        /// <param name="outputMessageParts">A collection of output message parts used by this operation.</param>
        /// <param name="codeNs">The contracts code namespace.</param>
        private void GenerateTypeContracts(Operation operation, MessagePartCollection inputMessageParts, MessagePartCollection outputMessageParts)
        {
            ArrayList inputMessageTypes = null;
            ArrayList outputMessageTypes = null;
            if (inputMessageParts != null)
                inputMessageTypes = GetMessageSchemaDefinitions(inputMessageParts);

            if (outputMessageParts != null)
                outputMessageTypes = GetMessageSchemaDefinitions(outputMessageParts);

            // If input or output message parts are found generate DataContracts and DataContractSerializers
            if (inputMessageTypes != null)
            {
                m_dcCodeGen.GenerateContracts(operation.Messages.Input, inputMessageParts, inputMessageTypes);
            }

            if (outputMessageTypes != null)
            {
                m_dcCodeGen.GenerateContracts(operation.Messages.Output, outputMessageParts, outputMessageTypes);
            }
        }

        /// <summary>
        /// Parses a Wsdl message parts collection and builds an array of required message elements and or types.
        /// </summary>
        /// <param name="messageParts">A collection of message parts.</param>
        /// <returns>An array list containing elements and types specified in a Wsdl message parts collection.</returns>
        private ArrayList GetMessageSchemaDefinitions(MessagePartCollection messageParts)
        {

            int elementCount = 0;   // Used to validate message parts
            int typeCount = 0;      // Used to validate message parts
            ArrayList schemaDefs = new ArrayList(); // When complete contains reference to schema elements for the parts
            
            // For each input message part find the schema element or type and add it to the inputSchemaParams hash
            foreach (MessagePart part in messageParts)
            {

                // This part defines an element
                if (part.Element.IsEmpty == false && part.Type.IsEmpty == true)
                {
                    // Make sure Element and Type parts are not mixed
                    if (typeCount > 0)
                        throw new XmlException("Invalid wsdl:message part. All parts of message in operation  must either contain type or element.");

                    XmlSchemaElement element = null;
                    if ((element = CodeGenUtils.FindSchemaElement(m_svcDesc, part.Element.Name, part.Element.Namespace)) != null)
                    {
                        schemaDefs.Add((XmlSchemaElement)element);
                        ++elementCount;
                    }
                    else
                    {
                        throw new XmlException("Missing element specified." +
                            "\n Message name     : " + part.Message.Name +
                            "\n Message part name: " + part.Name +
                            "\n Type name        : " + part.Type.Name +
                            "\n Type namespace   : " + part.Type.Namespace + 
                            "\n Element name     : " + part.Element.Name + 
                            "\n Element namespace: " + part.Element.Namespace );
                    }
                }
                // This part defines a type
                else if (part.Element.IsEmpty == true && part.Type.IsEmpty == false)
                {
                    // Make sure Element and Type parts are not mixed
                    if (elementCount > 0)
                        throw new XmlException("Invalid wsdl:message part. All parts of message in operation  must either contain type or element.");

                    XmlSchemaType type = null;
                    if ((type = CodeGenUtils.FindSchemaType(m_svcDesc, part.Type.Name, part.Type.Namespace)) != null)
                    {
                        schemaDefs.Add((XmlSchemaType)type);
                        ++typeCount;
                    }
                    else
                    {
                        XmlSchemaType typ = XmlSchemaType.GetBuiltInSimpleType(part.Type);
                        if (typ != null)
                        {
                            schemaDefs.Add(typ);
                            ++typeCount;
                        }
                        else
                        {
                            throw new XmlException("Missing type specified. Message name: " +
                                part.Message.Name + " Message part name: " +
                                part.Name + " Type name: " + part.Type.Name);
                        }
                    }
                }
            }

            if (elementCount == 0 && typeCount == 0)
                return null;
            else
                return schemaDefs;
        }

    }
}

