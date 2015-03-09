using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web.Services.Description;
using System.Xml;

namespace Ws.SvcUtilCodeGen
{

    /// <summary>
    /// Used to store information required to generate a hosted service operation
    /// </summary>
    internal class ServiceOp
    {
        private Operation m_operation = null;
        private string m_inAction = null;
        private string m_outAction = null;

        public ServiceOp(Operation operation, string inAction, string outAction)
        {
            m_operation = operation;
            m_inAction = inAction;
            m_outAction = outAction;
        }

        public Operation Operation { get { return m_operation; } set { m_operation = value; } }
        public string InAction { get { return m_inAction; } set { m_inAction = value; } }
        public string OutAction { get { return m_outAction; } set { m_outAction = value; } }
    }

    internal class HostedService
    {
        private string m_serviceName;
        private string m_serviceNamespace;
        private Guid m_serviceID = Guid.NewGuid();
        private List<ServiceOp> m_operations = new List<ServiceOp>();
        private List<Message> m_messages = new List<Message>();
        private MessageEncodingType m_encodingType = MessageEncodingType.Soap;

        public HostedService(string serviceName, string serviceNamespace, TargetPlatform platform)
        {
            // If service name is null extract it from the file name
            if (serviceName == null)
                throw new ArgumentNullException("serviceName");
            if (serviceNamespace == null)
                throw new ArgumentNullException("serviceNamespace");

            m_serviceName = serviceName;
            m_serviceNamespace = serviceNamespace;
        }

        public void AddOperation(Operation operation, string inAction, string outAction)
        {
            ServiceOp serviceOp = new ServiceOp(operation, inAction, outAction);
            m_operations.Add(serviceOp);
        }

        public MessageEncodingType EncodingType { get { return m_encodingType; } set { m_encodingType = value; } }

        public List<Message> Messages { get { return m_messages; } }

        public List<ServiceOp> ServiceOperations { get { return m_operations; } }

        public string Name { get { return m_serviceName; } }

        public string Namespace { get { return m_serviceNamespace; } }

        public Guid ServiceID { get { return m_serviceID; } }

    }

    internal class HostedServices : List<HostedService>
    {
        private string m_namespace;

        public HostedServices(string classNamespace)
        {
            if (classNamespace == null)
                throw new ArgumentNullException("targetNamesapce", "Must not be null.");
            m_namespace = classNamespace;
        }

        public string Namespace { get { return m_namespace; } }
    }

    internal class HostedServiceGenerator
    {
        private CodeConstructor BuildConstructor2(bool hasRequest, HostedService hostedService)
        {
            CodeConstructor constructor = new CodeConstructor();
            constructor.Name = hostedService.Name;
            constructor.Attributes = MemberAttributes.Public;
            
            if (hasRequest)
            {
                constructor.Parameters.Add(new CodeParameterDeclarationExpression("I" + hostedService.Name, "service"));
                constructor.ChainedConstructorArgs.Add(new CodeVariableReferenceExpression("service"));
            }

            constructor.ChainedConstructorArgs.Add(new CodeObjectCreateExpression("ProtocolVersion10"));

            return constructor;
        }
        
        private CodeConstructor BuildConstructor(bool hasRequest, HostedService hostedService)
        {
            CodeConstructor constructor = new CodeConstructor();
            constructor.Name = hostedService.Name;
            constructor.Attributes = MemberAttributes.Public;
            
            if (hasRequest)
            {
                constructor.Parameters.Add(new CodeParameterDeclarationExpression("I" + hostedService.Name, "service"));
            }

            constructor.Parameters.Add(new CodeParameterDeclarationExpression("ProtocolVersion", "version"));
            constructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("version"));

            if (hasRequest)
            {
                constructor.Statements.Add(new CodeCommentStatement("Set the service implementation properties"));
                // Assign service parameter to local variable
                constructor.Statements.Add(
                    new CodeAssignStatement(
                        new CodeVariableReferenceExpression("m_service"),
                        new CodeVariableReferenceExpression("service")
                    )
                );
            }

            constructor.Statements.Add(new CodeSnippetStatement(""));
            constructor.Statements.Add(new CodeCommentStatement("Set base service properties"));
            // Add Service Namespace assignment
            constructor.Statements.Add(
                new CodeAssignStatement(
                    new CodeVariableReferenceExpression("ServiceNamespace"),
                    new CodeObjectCreateExpression(
                        "WsXmlNamespace",
                        new CodeExpression[] {
                            new CodePrimitiveExpression(hostedService.Name.Substring(0, 3).ToLower()),
                            new CodePrimitiveExpression(hostedService.Namespace)
                        }
                    )
               )
            );

            // Add Service ID assignment
            constructor.Statements.Add(
                new CodeAssignStatement(
                    new CodeVariableReferenceExpression("ServiceID"),
                    new CodePrimitiveExpression("urn:uuid:" + hostedService.ServiceID.ToString())
               )
            );

            // Add Service Name assignment
            constructor.Statements.Add(
                new CodeAssignStatement(
                    new CodeVariableReferenceExpression("ServiceTypeName"),
                    new CodePrimitiveExpression(hostedService.Name)
               )
            );

            constructor.Statements.Add(new CodeSnippetStatement(""));
            constructor.Statements.Add(new CodeCommentStatement("Add service types here"));

            // Add operations
            // Note: Only reason to loop twice is to insure operations and events are seperated for readability
            foreach (ServiceOp serviceOp in hostedService.ServiceOperations)
            {
                if (serviceOp.Operation.Messages.Flow == OperationFlow.OneWay || serviceOp.Operation.Messages.Flow == OperationFlow.RequestResponse)
                {
                    // Get action property or build default
                    string action = serviceOp.InAction;
                    int actionIndex = action.LastIndexOf(':');
                    actionIndex = actionIndex == -1 || actionIndex < action.LastIndexOf('/') ? action.LastIndexOf('/') : actionIndex;
                    string actionNamespace = actionIndex == -1 ? action : action.Substring(0, actionIndex);
                    string actionName = actionIndex == -1 ? action : action.Substring(actionIndex + 1, action.Length - (actionIndex + 1));

                    constructor.Statements.Add(
                        new CodeMethodInvokeExpression(
                            new CodeVariableReferenceExpression("ServiceOperations"),
                            "Add",
                            new CodeObjectCreateExpression(
                                "WsServiceOperation",
                                new CodeExpression[] {
                                    new CodePrimitiveExpression(actionNamespace),
                                    new CodePrimitiveExpression(actionName) } )));
                }
                else if (serviceOp.Operation.Messages.Flow != OperationFlow.Notification)
                    throw new ArgumentException("Unsupported operation type detected. " + serviceOp.Operation.Messages.Flow.ToString());
            }

            constructor.Statements.Add(new CodeSnippetStatement(""));
            constructor.Statements.Add(new CodeCommentStatement("Add event sources here"));

            bool fEventsAdded = false;

            // Add events
            foreach (ServiceOp serviceOp in hostedService.ServiceOperations)
            {
                if (serviceOp.Operation.Messages.Flow == OperationFlow.Notification)
                {
                    // Get action property or build default
                    string action = serviceOp.OutAction;
                    int actionIndex = action.LastIndexOf(':');
                    actionIndex = actionIndex == -1 || actionIndex < action.LastIndexOf('/') ? action.LastIndexOf('/') : actionIndex;
                    string actionNamespace = actionIndex == -1 ? action : action.Substring(0, actionIndex);
                    string actionName = actionIndex == -1 ? action : action.Substring(actionIndex + 1, action.Length - (actionIndex + 1));

                    constructor.Statements.Add(
                        new CodeMethodInvokeExpression(
                            new CodeVariableReferenceExpression("EventSources"),
                            "Add",
                            new CodeObjectCreateExpression(
                                "DpwsWseEventSource",
                                new CodeExpression[] {
                                    new CodePrimitiveExpression(hostedService.Name.Substring(0, 3).ToLower()),
                                    new CodePrimitiveExpression(actionNamespace),
                                    new CodePrimitiveExpression(actionName) })));

                    fEventsAdded = true;
                }
            }

            if(fEventsAdded)
            {
                constructor.Statements.Add(
                    new CodeMethodInvokeExpression(
                        new CodeThisReferenceExpression(),
                        "AddEventServices"
                    )
                );
            }
            
            return constructor;
        }

        private CodeMemberMethod BuildOperation(ServiceOp serviceOp, IList<Message> messages, MessageEncodingType encodingType)
        {
            string reqTypeName = null;
            string reqElementName = null;
            string reqElementNamespace = null;
            string respTypeName = null;
            string respElementName = null;
            string respElementNamespace = null;
            foreach (Message message in messages)
            {
                if (serviceOp.Operation.Messages.Input != null && serviceOp.Operation.Messages.Input.Message.Name == message.Name)
                {
                    if (message.Parts.Count != 0)
                    {
                        reqElementName = CodeGenUtils.GetMessageElementName(serviceOp.Operation.PortType.ServiceDescription, message);
                        reqElementNamespace = message.Parts[0].Element == null ? message.Parts[0].Type.Namespace : message.Parts[0].Element.Namespace;
                        reqTypeName = CodeGenUtils.GetMessageTypeName(serviceOp.Operation.PortType.ServiceDescription, message);
                    }
                }
                else if (serviceOp.Operation.Messages.Output != null && serviceOp.Operation.Messages.Output.Message.Name == message.Name)
                {
                    if (message.Parts.Count != 0)
                    {
                        respElementName = CodeGenUtils.GetMessageElementName(serviceOp.Operation.PortType.ServiceDescription, message);
                        respElementNamespace = message.Parts[0].Element == null ? message.Parts[0].Type.Namespace : message.Parts[0].Element.Namespace;
                        respTypeName = CodeGenUtils.GetMessageTypeName(serviceOp.Operation.PortType.ServiceDescription, message);
                    }
                }
            }

            // If either element type is null set to void type
            reqElementName = reqElementName == null ? serviceOp.Operation.Name + "Request" : reqElementName;
            respElementName = respElementName == null ? serviceOp.Operation.Name + "Response" : respElementName;

            // If either type is null set to void type
            reqTypeName = reqTypeName == null ? "void" : reqTypeName;
            respTypeName = respTypeName == null ? "void" : respTypeName;

            // Set the method name from Action. The Hosted service dispatcher maps method calls to the action endpoint
            string methodName = serviceOp.InAction;
            if (methodName == null)
                methodName = serviceOp.Operation.Name;
            else
            {
                int index = methodName.LastIndexOfAny(new char[] { ':', '/', '\\' });
                methodName = (index == -1 || index == (methodName.Length - 1)) ? methodName : methodName.Substring(index + 1);
            }

            // Create the operation member
            CodeMemberMethod codeMethod = new CodeMemberMethod();
            codeMethod.Name = methodName;
            codeMethod.Attributes = MemberAttributes.Public;
            codeMethod.ReturnType = new CodeTypeReference("WsMessage"); //"Byte", 1);
            codeMethod.Parameters.Add(new CodeParameterDeclarationExpression("WsMessage", "request"));
            //codeMethod.Parameters.Add(new CodeParameterDeclarationExpression("WsWsaHeader", "header"));
            //codeMethod.Parameters.Add(new CodeParameterDeclarationExpression("XmlReader", "reader"));

            // If there is a request object (i.e. not void) create an instance of the request serializer
            if (reqTypeName != "void")
            {
                codeMethod.Statements.Add(new CodeCommentStatement("Build request object"));
                // Generated Code: TypeNameDataContractSerializer reqDcs;
                codeMethod.Statements.Add(
                    new CodeVariableDeclarationStatement(
                        CodeGenUtils.GenerateDcsName(reqTypeName),
                        "reqDcs"
                    )
                );
                // Generated Code: reqDcs = new TypeNameDataContractSerializer("InMessagePartName", InputMessageTypeNamespace);
                codeMethod.Statements.Add(
                    new CodeAssignStatement(
                        new CodeVariableReferenceExpression("reqDcs"),
                        new CodeObjectCreateExpression(
                            CodeGenUtils.GenerateDcsName(reqTypeName),
                            new CodeExpression[] {
                            new CodePrimitiveExpression(reqElementName),
                            new CodePrimitiveExpression(reqElementNamespace)
                        }
                        )
                    )
                );
            }
            // If the message is Mtom encoded set serializers Mtom body parts collection
            // req.BodyParts = this.BodyParts;
            if (encodingType == MessageEncodingType.Mtom)
                codeMethod.Statements.Add(
                    new CodeAssignStatement(
                        new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("reqDcs"), "BodyParts"),
                        new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("request"), "BodyParts")
                    )
                );
            // If there is a request object (i.e. not void) create it and set it.
            if (reqTypeName != "void")
            {
                // Generated Code: InMessagePartName req;
                codeMethod.Statements.Add(
                    new CodeVariableDeclarationStatement(
                        reqTypeName,
                        "req"
                    )
                );
                // req = reqDcs.ReadObject(reader);
                codeMethod.Statements.Add(
                    new CodeAssignStatement(
                        new CodeVariableReferenceExpression("req"),
                        new CodeCastExpression(
                            reqTypeName,
                            new CodeMethodInvokeExpression(
                                new CodeVariableReferenceExpression("reqDcs"),
                                "ReadObject",
                                new CodeExpression[] { new CodeFieldReferenceExpression(
                                    new CodeVariableReferenceExpression("request"), "Reader") }
                            )
                        )
                    )
                );
            }

            // Add request.Reader.Dispose() method call
            codeMethod.Statements.Add( 
                new CodeMethodInvokeExpression(
                    new CodeFieldReferenceExpression( 
                        new CodeVariableReferenceExpression("request"), 
                        "Reader"
                    ),
                    "Dispose", new CodeExpression[] { }
                )
            );

            // reqest.Reader = null;
            codeMethod.Statements.Add(
                new CodeAssignStatement(
                    new CodeFieldReferenceExpression( 
                        new CodeVariableReferenceExpression("request"), 
                        "Reader"
                    ),
                    new CodePrimitiveExpression(null)
                )
            );
                

            // Set the request param. If the request is void don't pass anything.
            CodeExpression[] reqParams = reqTypeName == "void" ? new CodeExpression[] { } : new CodeExpression[] { new CodeVariableReferenceExpression("req") };

            // If this is a oneway message or void response return null after processing the request
            if (serviceOp.Operation.Messages.Flow == OperationFlow.OneWay || respTypeName == "void")
            {
                codeMethod.Statements.Add(new CodeSnippetStatement(""));
                codeMethod.Statements.Add(new CodeCommentStatement("Call service operation to process request."));

                // Add OneWay call to device developer supplied service method implementation
                // Generated Code: m_service.MethodName(req);
                codeMethod.Statements.Add(
                    new CodeMethodInvokeExpression(
                        new CodeVariableReferenceExpression("m_service"),
                        serviceOp.Operation.Name,
                        reqParams
                    )
                );

                codeMethod.Statements.Add(new CodeSnippetStatement(""));
                codeMethod.Statements.Add(new CodeCommentStatement("Return a OneWayResponse message for oneway messages"));
                // Generated Code: return null;
                codeMethod.Statements.Add(
                    new CodeMethodReturnStatement(
                        new CodeMethodInvokeExpression(
                            new CodeVariableReferenceExpression("WsMessage"),
                            "CreateOneWayResponse"
                        )
                    )
                );
                return codeMethod;
            }

            // If a response object is required create it and call the processing method
            codeMethod.Statements.Add(new CodeSnippetStatement(""));
            codeMethod.Statements.Add(new CodeCommentStatement("Create response object"));
            codeMethod.Statements.Add(new CodeCommentStatement("Call service operation to process request and return response."));
            // OutMessagePartName resp;
            codeMethod.Statements.Add(
                new CodeVariableDeclarationStatement(
                    respTypeName,
                    "resp"
                )
            );
            // Generated Code: resp = m_service.MethodName(req);
            codeMethod.Statements.Add(
                new CodeAssignStatement(
                    new CodeVariableReferenceExpression("resp"),
                    new CodeMethodInvokeExpression(
                        new CodeVariableReferenceExpression("m_service"),
                        serviceOp.Operation.Name,
                        reqParams
                    )
                )
            );

            codeMethod.Statements.Add(new CodeSnippetStatement(""));
            codeMethod.Statements.Add(new CodeCommentStatement("Create response header"));
            // Generated Code: WsWsaHeader respHeader = new WsWsaHeader(
            //          "http://Some/Namespace/OperationNameResponse",
            //          request.Header.MessageID,
            //          WsWellKnownUri.WsaAnonymousUri,
            //          null, null, null);
            codeMethod.Statements.Add(
                new CodeVariableDeclarationStatement(
                    "WsWsaHeader",
                    "respHeader",
                    new CodeObjectCreateExpression(
                        "WsWsaHeader",
                        new CodeExpression[] {
                            new CodePrimitiveExpression(serviceOp.OutAction),
                            new CodeFieldReferenceExpression(new CodeFieldReferenceExpression(
                                new CodeVariableReferenceExpression("request"), "Header"), "MessageID"),
                            new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("m_version"), "AnonymousUri"),
                            new CodePrimitiveExpression(null),
                            new CodePrimitiveExpression(null),
                            new CodePrimitiveExpression(null)
                        }
                    )
                )
            );

            // Generated Code: WsMessage response = new WsMessage(
            //           respHeader, 
            //           resp, 
            //           WsPrefix.Wsdp);
            codeMethod.Statements.Add(
                new CodeVariableDeclarationStatement(
                    "WsMessage",
                    "response",
                    new CodeObjectCreateExpression(
                        "WsMessage",
                        new CodeExpression[] {
                            new CodeVariableReferenceExpression("respHeader"),
                            new CodeVariableReferenceExpression("resp"),
                            new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("WsPrefix"), "Wsdp")
                        }
                    )
                )
            );

            if (respTypeName != "void")
            {
                codeMethod.Statements.Add(new CodeSnippetStatement(""));
                codeMethod.Statements.Add(new CodeCommentStatement("Create response serializer"));
                //Generated Code: TypeNameDataContractSerializer respDcs;
                codeMethod.Statements.Add(
                    new CodeVariableDeclarationStatement(
                        CodeGenUtils.GenerateDcsName(respTypeName),
                        "respDcs"
                    )
                );
                // Generated Code: respDcs = new TypeNameDataContractSerializer("OutMessagePartName", OuputObjectsTypeNamespace);
                codeMethod.Statements.Add(
                    new CodeAssignStatement(
                        new CodeVariableReferenceExpression("respDcs"),
                        new CodeObjectCreateExpression(
                            CodeGenUtils.GenerateDcsName(respTypeName),
                            new CodeExpression[] {
                            new CodePrimitiveExpression(respElementName),
                            new CodePrimitiveExpression(respElementNamespace)
                        }
                        )
                    )
                );
                codeMethod.Statements.Add(
                    new CodeAssignStatement(
                        new CodeFieldReferenceExpression( new CodeVariableReferenceExpression("response"), "Serializer" ),
                        new CodeVariableReferenceExpression("respDcs")
                    )
                );
            }

            // If this is an mtom message return null after setting body parts
            // If the message encoding is Mtom set the mtom encoded flag and return null else return soap message
            if (encodingType == MessageEncodingType.Mtom)
            {
                codeMethod.Statements.Add(new CodeSnippetStatement(""));
                codeMethod.Statements.Add(new CodeCommentStatement("Indicate that message is Mtom encoded"));

                // response.BodyParts = new WsMtomBodyParts();
                codeMethod.Statements.Add(
                    new CodeAssignStatement(
                        new CodeFieldReferenceExpression( 
                            new CodeVariableReferenceExpression("response"), "BodyParts"),
                        new CodeObjectCreateExpression(
                            "WsMtomBodyParts"
                        )
                    )
                );

                /*
                codeMethod.Statements.Add(new CodeSnippetStatement(""));
                codeMethod.Statements.Add(new CodeCommentStatement("Build response message and store as first Mtom Bodypart"));
                // MessageType = WsMessageType.Mtom;
                codeMethod.Statements.Add(
                    new CodeAssignStatement(
                        new CodeVariableReferenceExpression("MessageType"),
                        new CodeFieldReferenceExpression(
                            new CodeVariableReferenceExpression("WsMessageType"),
                            "Mtom"
                        )
                    )
                );
                // BodyParts.Clear();
                codeMethod.Statements.Add(
                    new CodeMethodInvokeExpression(
                        new CodeVariableReferenceExpression("BodyParts"),
                        "Clear",
                        new CodeExpression[] { }
                    )
                );
                // BodyParts.Start = "<soap@soap>";
                codeMethod.Statements.Add(
                    new CodeAssignStatement(
                        new CodeFieldReferenceExpression(
                            new CodeVariableReferenceExpression("BodyParts"),
                            "Start"
                        ),
                        new CodePrimitiveExpression("<soap@soap>")
                    )
                );
                // BodyParts.Add(respDcs.CreateNewBodyPart(SoapMessageBuilder.BuildSoapMessage(respHeader, respDcs, resp), "soap@soap"));
                CodeExpression respParams = respTypeName == "void" ? new CodeExpression() : new CodeVariableReferenceExpression("resp");
                codeMethod.Statements.Add(
                    new CodeMethodInvokeExpression(
                        new CodeVariableReferenceExpression("BodyParts"),
                        "Add",
                        new CodeExpression[] {
                            new CodeMethodInvokeExpression(
                                new CodeVariableReferenceExpression("respDcs"),
                                "CreateNewBodyPart",
                                new CodeExpression[] {
                                        new CodeMethodInvokeExpression(
                                            new CodeVariableReferenceExpression("SoapMessageBuilder"),
                                            "BuildSoapMessage",
                                            new CodeExpression[] {
                                                new CodeVariableReferenceExpression("respHeader"),
                                                new CodeVariableReferenceExpression("respDcs"),
                                                respParams
                                            }
                                        ),
                                    new CodePrimitiveExpression("<soap@soap>")
                                }
                            )
                        }
                    )
                );
                // BodyParts.Add(respDcs.BodyParts[0]);
                codeMethod.Statements.Add(
                    new CodeMethodInvokeExpression(
                        new CodeVariableReferenceExpression("BodyParts"),
                        "Add",
                        new CodeExpression[] {
                            new CodeIndexerExpression(
                                new CodeFieldReferenceExpression(
                                    new CodeVariableReferenceExpression("respDcs"),
                                    "BodyParts"
                                ),
                                new CodeExpression[] {
                                    new CodePrimitiveExpression(0)
                                }
                            )
                        }
                    )
                );
                // Generated Code: return null;
                codeMethod.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));
                */
            }
            /*
            else
            {
                codeMethod.Statements.Add(new CodeSnippetStatement(""));
                codeMethod.Statements.Add(new CodeCommentStatement("Build response message and return"));
                CodeExpression respDcsParam;
                CodeExpression respParam;
                if (respTypeName == "void")
                {
                    respDcsParam = new CodePrimitiveExpression(null);
                    respParam = new CodePrimitiveExpression(null);
                }
                else
                {
                    respDcsParam = new CodeVariableReferenceExpression("respDcs");
                    respParam = new CodeVariableReferenceExpression("resp");
                }

                // Generated Code: return SoapMessageBuilder.BuildSoapMessage(respHeader, respDcs, resp);
                codeMethod.Statements.Add(
                    new CodeMethodReturnStatement(
                        new CodeMethodInvokeExpression(
                            new CodeVariableReferenceExpression("SoapMessageBuilder"),
                            "BuildSoapMessage",
                            new CodeExpression[] {
                            new CodeVariableReferenceExpression("respHeader"),
                            respDcsParam,
                            respParam
                        }
                        )
                    )
                );
            }
            */
            // Generated Code: return SoapMessageBuilder.BuildSoapMessage(respHeader, respDcs, resp);
            codeMethod.Statements.Add(
                new CodeMethodReturnStatement(
                    new CodeVariableReferenceExpression("response")
                )
            );

            return codeMethod;
        }

        private CodeMemberMethod BuildEventSource(ServiceOp serviceOp, IList<Message> messages)
        {
            string reqElementName = null;
            string reqTypeName = null;
            string reqElementNamespace = null;
            foreach (Message message in messages)
            {
                if (serviceOp.Operation.Messages.Output != null && serviceOp.Operation.Messages.Output.Message.Name == message.Name)
                {
                    reqTypeName = CodeGenUtils.GetMessageTypeName(serviceOp.Operation.PortType.ServiceDescription, message);
                    reqElementName = CodeGenUtils.GetMessageElementName(serviceOp.Operation.PortType.ServiceDescription, message);
                    reqElementNamespace = message.Parts[0].Element == null ? message.Parts[0].Type.Namespace : message.Parts[0].Element.Namespace;
                }
            }

            // If the element name is null set default name
            reqElementName = reqElementName == null ? serviceOp.Operation.Name + "Request" : reqElementName;

            // If the type name is null set it to the default name "void"
            reqTypeName = reqTypeName == null ? "void" : reqTypeName;

            // Set the DataContractSerailizer name prefix
            // If the type is an array in the prefix replace "[]" in the type name with "Array"
            string reqDCSPrefix;
            if (reqTypeName.Length > 2 && reqTypeName.Substring(reqTypeName.Length - 2) == "[]")
                reqDCSPrefix = reqTypeName.Substring(0, reqTypeName.Length - 2) + "Array";

            // Set the method name from Action. The Hosted service dispatcher maps method calls to the action endpoint
            string methodName = serviceOp.OutAction;
            if (methodName == null)
                methodName = serviceOp.Operation.Name;
            else
            {
                int index = methodName.LastIndexOfAny(new char[] { ':', '/', '\\' });
                methodName = (index == -1 || index == (methodName.Length - 1)) ? methodName : methodName.Substring(index + 1);
            }

            CodeMemberMethod codeMethod = new CodeMemberMethod();
            codeMethod.Name = methodName;
            codeMethod.Attributes = MemberAttributes.Public;
            codeMethod.ReturnType = new CodeTypeReference();
            if (reqTypeName != "void")
                codeMethod.Parameters.Add(new CodeParameterDeclarationExpression(reqTypeName, "eventReq"));

            codeMethod.Statements.Add(new CodeSnippetStatement(""));
            codeMethod.Statements.Add(new CodeCommentStatement("Create temp event source object, set the event action and create the event header"));
            // Generated Code: DpwsWseEventSource eventSource;
            codeMethod.Statements.Add(
                new CodeVariableDeclarationStatement(
                    "DpwsWseEventSource",
                    "eventSource"
                )
            );
            // Generated Code: eventSource = EventSources["reqTypeName"];
            codeMethod.Statements.Add(
                new CodeAssignStatement(
                    new CodeVariableReferenceExpression("eventSource"),
                    new CodeArrayIndexerExpression(
                        new CodeVariableReferenceExpression("EventSources"),
                        new CodePrimitiveExpression(serviceOp.Operation.Name)
                    )
                )
            );
            // Generated Code: string action;
            codeMethod.Statements.Add(
                new CodeVariableDeclarationStatement(
                    "String",
                    "action"
                )
            );
            // Generated Code: action = "http://www.nrf-arts.org/IXRetail/namespace/PosPrinter/EventName";
            codeMethod.Statements.Add(
                new CodeAssignStatement(
                    new CodeVariableReferenceExpression("action"),
                    new CodePrimitiveExpression(serviceOp.OutAction)
                )
            );
            // Generated Code: WsWsaHeader header;
            codeMethod.Statements.Add(
                new CodeVariableDeclarationStatement(
                    "WsWsaHeader",
                    "header"
                )
            );
            // Generated Code: header = new WsWsaHeader(action, null, null, null, null, null);
            codeMethod.Statements.Add(
                new CodeAssignStatement(
                    new CodeVariableReferenceExpression("header"),
                    new CodeObjectCreateExpression(
                        "WsWsaHeader",
                        new CodeExpression[] {
                            new CodeVariableReferenceExpression("action"),
                            new CodePrimitiveExpression(null),
                            new CodePrimitiveExpression(null),
                            new CodePrimitiveExpression(null),
                            new CodePrimitiveExpression(null),
                            new CodePrimitiveExpression(null),
                        }
                    )
                )
            );

            // Generated Code: WsMessage msg = new WsMessage(header, eventReq, WsPrefix.Wse);
            codeMethod.Statements.Add(
                new CodeVariableDeclarationStatement("WsMessage", "msg",
                    new CodeObjectCreateExpression(
                        "WsMessage",
                        new CodeExpression[] {
                            new CodeVariableReferenceExpression("header"),
                            new CodeVariableReferenceExpression("eventReq"),
                            new CodeFieldReferenceExpression( 
                                new CodeVariableReferenceExpression("WsPrefix"), "Wse" 
                            )
                        }
                    )
                )
            );

            // If the event message body is empty skip serialization code
            if (reqTypeName != "void")
            {
                codeMethod.Statements.Add(new CodeSnippetStatement(""));
                codeMethod.Statements.Add(new CodeCommentStatement("Create event serializer and write the event object"));
                // eventTypeNameDataContractSerializer eventDcs;
                codeMethod.Statements.Add(
                    new CodeVariableDeclarationStatement(
                        CodeGenUtils.GenerateDcsName(reqTypeName),
                        "eventDcs"
                    )
                );

                // Generated Code: eventDcs = new eventTypeNameDataContractSerializer("elementTypeName", ServiceNamespace.Namespace);
                codeMethod.Statements.Add(
                    new CodeAssignStatement(
                        new CodeVariableReferenceExpression("eventDcs"),
                        new CodeObjectCreateExpression(
                            CodeGenUtils.GenerateDcsName(reqTypeName),
                            new CodeExpression[] {
                            new CodePrimitiveExpression(reqElementName),
                            new CodePrimitiveExpression(reqElementNamespace)
                        }
                        )
                    )
                );

                // Generated Code: msg.Serializer = eventDcs;
                codeMethod.Statements.Add(
                    new CodeAssignStatement(
                        new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("msg"), "Serializer"),
                        new CodeVariableReferenceExpression("eventDcs")
                    )
                );

                /*
                codeMethod.Statements.Add(new CodeSnippetStatement(""));
                codeMethod.Statements.Add(new CodeCommentStatement("Build event body message"));

                //Generated Code: byte[] soapBuffer = SoapMessageBuilder.BuildEventBody(header, eventDcs, eventReq);
                codeMethod.Statements.Add(
                    new CodeVariableDeclarationStatement(
                        typeof(byte[]),
                        "soapBuffer",
                        new CodeMethodInvokeExpression(
                            new CodeVariableReferenceExpression("SoapMessageBuilder"),
                            "BuildEventBody",
                            new CodeExpression[] {
                            new CodeVariableReferenceExpression("header"),
                            new CodeVariableReferenceExpression("eventDcs"),
                            new CodeVariableReferenceExpression("eventReq")
                        }
                        )
                    )
                );
                */

                codeMethod.Statements.Add(new CodeSnippetStatement(""));
                /*
                // string soapMessage = new string(Encoding.UTF8.GetChars(soapBuffer));
                codeMethod.Statements.Add(
                    new CodeVariableDeclarationStatement(
                        typeof(string),
                        "soapMessage",
                        new CodeObjectCreateExpression(
                            typeof(string),
                            new CodeExpression[] {
                                new CodeMethodInvokeExpression(
                                    new CodeFieldReferenceExpression(
                                        new CodeVariableReferenceExpression("Encoding"),
                                        "UTF8"
                                    ),
                                    "GetChars",
                                    new CodeExpression[] {
                                        new CodeArgumentReferenceExpression("soapBuffer")
                                    }
                                )
                            }
                        )
                    )
                );
                */
            }

            codeMethod.Statements.Add(new CodeCommentStatement("Fire event"));

            /*
            // If message body is empty pass a null soapMessage to the FireEvent method
            CodeExpression soapMessageExpression;
            if (reqTypeName == "void")
                soapMessageExpression = new CodePrimitiveExpression(null);
            else
                soapMessageExpression = new CodeTypeReferenceExpression("soapMessage");
            */

            //Device.SubscriptionManager.FireEvent(this, eventSource, header, soapMessage);
            codeMethod.Statements.Add(
                new CodeMethodInvokeExpression(
                    new CodeFieldReferenceExpression(
                        new CodeVariableReferenceExpression("Dpws.Device.Device"),
                        "SubscriptionManager"
                    ),
                    "FireEvent",
                    new CodeExpression[] {
                        new CodeThisReferenceExpression(),
                        new CodeTypeReferenceExpression("eventSource"),
                        new CodeTypeReferenceExpression("msg") //"header"),
                        //soapMessageExpression
                    }
                )
            );

            return codeMethod;
        }

        internal void GenerateCode(string filename, HostedServices services)
        {

            // Create Hosted Service file stream
            FileStream hsStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);
            StreamWriter hsStreamWriter = new StreamWriter(hsStream);

            // Write the auto generated header
            hsStreamWriter.Write(AutoGenTextHeader.Message);
            
            // Set up data contract code generator
            CSharpCodeProvider cSharpCP = new CSharpCodeProvider();
            ICodeGenerator hsCodeGen = cSharpCP.CreateGenerator(hsStreamWriter);
            CodeGeneratorOptions codeGenOptions = new CodeGeneratorOptions();
            codeGenOptions.BracingStyle = "C";

            CodeNamespace hsCodeNamespace = new CodeNamespace(services.Namespace);

            // Add hosted service using directives
            CodeSnippetCompileUnit compileUnit = new CodeSnippetCompileUnit("using System;");
            hsCodeGen.GenerateCodeFromCompileUnit(compileUnit, hsStreamWriter, codeGenOptions);
            compileUnit.Value = "using System.Text;";
            hsCodeGen.GenerateCodeFromCompileUnit(compileUnit, hsStreamWriter, codeGenOptions);
            compileUnit.Value = "using System.Xml;";
            hsCodeGen.GenerateCodeFromCompileUnit(compileUnit, hsStreamWriter, codeGenOptions);
            compileUnit.Value = "using Dpws.Device;";
            hsCodeGen.GenerateCodeFromCompileUnit(compileUnit, hsStreamWriter, codeGenOptions);
            compileUnit.Value = "using Dpws.Device.Services;";
            hsCodeGen.GenerateCodeFromCompileUnit(compileUnit, hsStreamWriter, codeGenOptions);
            compileUnit.Value = "using Ws.Services;";
            hsCodeGen.GenerateCodeFromCompileUnit(compileUnit, hsStreamWriter, codeGenOptions);
            foreach (HostedService hostedService in services)
            {
                if (hostedService.EncodingType == MessageEncodingType.Mtom)
                {
                    compileUnit.Value = "using Ws.Services.Mtom;";
                    hsCodeGen.GenerateCodeFromCompileUnit(compileUnit, hsStreamWriter, codeGenOptions);
                    break;
                }
            }
            compileUnit.Value = "using Ws.Services.WsaAddressing;";
            hsCodeGen.GenerateCodeFromCompileUnit(compileUnit, hsStreamWriter, codeGenOptions);
            compileUnit.Value = "using Ws.Services.Xml;";
            hsCodeGen.GenerateCodeFromCompileUnit(compileUnit, hsStreamWriter, codeGenOptions);
            compileUnit.Value = "using Ws.Services.Binding;";
            hsCodeGen.GenerateCodeFromCompileUnit(compileUnit, hsStreamWriter, codeGenOptions);
            compileUnit.Value = "using Ws.Services.Soap;";
            hsCodeGen.GenerateCodeFromCompileUnit(compileUnit, hsStreamWriter, codeGenOptions);
            compileUnit.Value = "";
            hsCodeGen.GenerateCodeFromCompileUnit(compileUnit, hsStreamWriter, codeGenOptions);
            compileUnit.Namespaces.Add(hsCodeNamespace);

            // For each service generate a code type declaration and generate code
            foreach (HostedService service in services)
            {
                // Class Declaration
                // Generated Code: public class ServiceName : DpwsHostedService
                CodeTypeDeclaration codeType = new CodeTypeDeclaration();
                codeType.Name = service.Name;
                codeType.TypeAttributes = TypeAttributes.Public;
                codeType.IsClass = true;
                CodeTypeReference codeBaseRef = new CodeTypeReference("DpwsHostedService");
                codeType.BaseTypes.Add(codeBaseRef);

                // Check operations for any request flow pattern. If pattern is found for any operation
                // create the global interface instance.
                bool hasRequest = false;
                foreach (ServiceOp operation in service.ServiceOperations)
                {
                    if (operation.Operation.Messages.Flow != OperationFlow.Notification && operation.Operation.Messages.Flow != OperationFlow.None)
                    {
                        hasRequest = true;
                        break;
                    }
                }

                if (hasRequest)
                {
                    // Add service implementation field
                    // Generated Code: private IServiceName m_service;
                    CodeMemberField codeMember = new CodeMemberField("I" + service.Name, "m_service");
                    //CodeExpression initExpression = new CodePrimitiveExpression(null);
                    //codeMember.InitExpression = initExpression;
                    codeMember.Attributes = MemberAttributes.Private;
                    codeType.Members.Add(codeMember);
                }

                // Build constructor
                codeType.Members.Add(BuildConstructor(hasRequest, service));
                codeType.Members.Add(BuildConstructor2(hasRequest, service));

                // Build operations
                foreach (ServiceOp serviceOp in service.ServiceOperations)
                {
                    if (serviceOp.Operation.Messages.Flow == OperationFlow.OneWay || serviceOp.Operation.Messages.Flow == OperationFlow.RequestResponse)
                    {
                        // Display operation info
                        Logger.WriteLine("Creating hosted service operation:", LogLevel.Verbose);
                        Logger.WriteLine("\tOperation Name: " + serviceOp.Operation.Name, LogLevel.Verbose);
                        codeType.Members.Add(BuildOperation(serviceOp, service.Messages, service.EncodingType));
                    }
                    else if (serviceOp.Operation.Messages.Flow == OperationFlow.Notification)
                    {
                        // Display operation info
                        Logger.WriteLine("Creating hosted service event handler:", LogLevel.Verbose);
                        Logger.WriteLine("\tEvent handler name: " + serviceOp.Operation.Name, LogLevel.Verbose);
                        codeType.Members.Add(BuildEventSource(serviceOp, service.Messages));
                    }
                    else
                        throw new ArgumentException("Unsupported operation type detected. " + serviceOp.Operation.Messages.Flow.ToString());
                }

                hsCodeNamespace.Types.Add(codeType);
            }

            // Generate hosted service source file
            hsCodeGen.GenerateCodeFromNamespace(hsCodeNamespace, hsStreamWriter, codeGenOptions);
            hsStreamWriter.Flush();
            hsStreamWriter.Close();
        }
    }
}
