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

    internal class ClientProxy
    {
        private string m_className;
        private string m_classCallbackInterfaceName;
        private Guid m_callbackID = Guid.NewGuid();
        private List<ServiceOp> m_operations = new List<ServiceOp>();
        private List<Message> m_messages = new List<Message>();
        private MessageEncodingType m_encodingType = MessageEncodingType.Soap;

        public ClientProxy(string className, TargetPlatform platform)
        {
            m_className = className;
            m_classCallbackInterfaceName = "I" + m_className + "Callback";
        }

        public void AddOperation(Operation operation, string inAction, string outAction)
        {
            ServiceOp serviceOp = new ServiceOp(operation, inAction, outAction);
            m_operations.Add(serviceOp);
        }

        public Guid CallbackID { get { return m_callbackID; } }

        public MessageEncodingType EncodingType { get { return m_encodingType; } set { m_encodingType = value; } }

        public string CallbackInterfaceName { get { return m_classCallbackInterfaceName; } set { m_classCallbackInterfaceName = value; } }
        
        public List<Message> Messages { get { return m_messages; } }

        public string Name { get { return m_className; } }

        public List<ServiceOp> ServiceOperations { get { return m_operations; } }
    }

    internal class ClientProxies : List<ClientProxy>
    {
        private string m_namespace;

        public ClientProxies(string classNamespace)
        {
            if (classNamespace == null)
                throw new ArgumentNullException("targetNamesapce", "Must not be null.");
            m_namespace = classNamespace;
        }

        public string Namespace { get { return m_namespace; } }

    }

    internal class ClientProxyGenerator
    {
        private CodeConstructor BuildFirstConstructor(bool hasEvents, ClientProxy clientProxy)
        {
            CodeConstructor constructor = new CodeConstructor();
            constructor.Name = clientProxy.Name + "ClientProxy";
            constructor.Parameters.Add(new CodeParameterDeclarationExpression("Binding", "binding"));
            constructor.Parameters.Add(new CodeParameterDeclarationExpression("ProtocolVersion", "version"));
            constructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("binding"));
            constructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("version"));
            constructor.Attributes = MemberAttributes.Public;

            if (hasEvents)
            {
                constructor.Parameters.Add(new CodeParameterDeclarationExpression(clientProxy.CallbackInterfaceName, "callbackHandler"));
                constructor.Statements.Add(new CodeCommentStatement("Set the client callback implementation property"));
                // Assign service parameter to local variable
                // Generated Code: m_eventHandler = callbackHandler;
                constructor.Statements.Add(
                    new CodeAssignStatement(
                        new CodeVariableReferenceExpression("m_eventHandler"),
                        new CodeVariableReferenceExpression("callbackHandler")
                    )
                );
            }

            constructor.Statements.Add(new CodeSnippetStatement(""));
            constructor.Statements.Add(new CodeCommentStatement("Set client endpoint address"));

            // Create RequestChannel 
            // Generated Code:  m_requestChannel = m_localBinding.CreateClientChannel(new ClientBindingContext());
            constructor.Statements.Add(
                new CodeAssignStatement(
                    new CodeVariableReferenceExpression("m_requestChannel"),
                    new CodeMethodInvokeExpression( 
                        new CodeVariableReferenceExpression("m_localBinding"),
                        "CreateClientChannel",
                        new CodeObjectCreateExpression("ClientBindingContext", 
                            new CodeVariableReferenceExpression("m_version")
                        )
                    )
                )
            );

            //if (clientProxy.EncodingType == MessageEncodingType.Mtom)
            //{
            //    constructor.Statements.Add(new CodeSnippetStatement(""));
            //    constructor.Statements.Add(new CodeCommentStatement("Create the BodyParts instance"));

            //    // Generated Code: BodyParts = new WsMtomBodyParts();
            //    constructor.Statements.Add(
            //        new CodeAssignStatement(
            //            new CodeVariableReferenceExpression("BodyParts"),
            //            new CodeObjectCreateExpression(
            //                "WsMtomBodyParts",
            //                new CodeExpression[] {}
            //            )
            //        )
            //    );
            //}

            // Add event service callbacks
            bool firstPass = true;
            foreach (ServiceOp serviceOp in clientProxy.ServiceOperations)
            {
                if (serviceOp.Operation.Messages.Flow == OperationFlow.Notification)
                {
                    if (firstPass)
                    {
                        constructor.Statements.Add(new CodeSnippetStatement(""));
                        constructor.Statements.Add(new CodeCommentStatement("Add client callback operations and event source types"));
                        firstPass = false;
                    }
                    
                    // Get action property or build default
                    string action = serviceOp.OutAction;
                    int actionIndex = action.LastIndexOf(':');
                    actionIndex = actionIndex == -1 || actionIndex < action.LastIndexOf('/') ? action.LastIndexOf('/') : actionIndex;
                    string actionNamespace = actionIndex == -1 ? action : action.Substring(0, actionIndex);
                    string actionName = actionIndex == -1 ? action : action.Substring(actionIndex + 1, action.Length - (actionIndex + 1));

                    // Generated Code: ServiceOperations.Add(new WsServiceOperation("callback namespace", "calback method name"));
                    constructor.Statements.Add(
                        new CodeMethodInvokeExpression(
                            new CodeVariableReferenceExpression("ServiceOperations"),
                            "Add",
                            new CodeObjectCreateExpression(
                                "WsServiceOperation",
                                new CodeExpression[] {
                                    new CodePrimitiveExpression(actionNamespace),
                                    new CodePrimitiveExpression(actionName)
                                }
                            )
                        )
                    );

                    // Generated Code: EventSources.Add(new DpwsServiceType("MethodName", "Method Namespace"));
                    constructor.Statements.Add(
                        new CodeMethodInvokeExpression(
                            new CodeVariableReferenceExpression("EventSources"),
                            "Add",
                            new CodeObjectCreateExpression(
                                "DpwsServiceType",
                                new CodeExpression[] {
                                    new CodePrimitiveExpression(actionName),
                                    new CodePrimitiveExpression(actionNamespace)
                                }
                            )
                        )
                    );
                }
            }

            if(hasEvents)
            {
                constructor.Statements.Add(new CodeSnippetStatement(""));
                constructor.Statements.Add(new CodeCommentStatement("Add eventing SubscriptionEnd ServiceOperations. By default Subscription End call back to this client"));

                // Generated Code: ServiceOperations.Add(new WsServiceOperation("WsWellKnownUri.WseNamespaceUri", "SubscriptionEnd"));
                constructor.Statements.Add(
                    new CodeMethodInvokeExpression(
                        new CodeVariableReferenceExpression("ServiceOperations"),
                        "Add",
                        new CodeObjectCreateExpression(
                            "WsServiceOperation",
                            new CodeExpression[] {
                                new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("WsWellKnownUri"), "WseNamespaceUri"),
                                new CodePrimitiveExpression("SubscriptionEnd")
                            }
                        )
                    )
                );

                constructor.Statements.Add(new CodeSnippetStatement(""));

                // Generated Code: this.StartEventHandlers();
                constructor.Statements.Add(
                    new CodeMethodInvokeExpression(
                        new CodeThisReferenceExpression(),
                        "StartEventListeners"
                    )
                );            
            }

            return constructor;
        }


        private CodeMemberMethod BuildProxyCallbackMethod(ServiceOp serviceOp, ClientProxy clientProxy)
        {
            string reqElementName = null;
            string reqTypeName = null;
            string reqElementNamespace = null;
            foreach (Message message in clientProxy.Messages)
            {
                if (serviceOp.Operation.Messages.Output != null && serviceOp.Operation.Messages.Output.Message.Name == message.Name)
                {
                    reqElementName = CodeGenUtils.GetMessageElementName(serviceOp.Operation.PortType.ServiceDescription, message);
                    reqTypeName = CodeGenUtils.GetMessageTypeName(serviceOp.Operation.PortType.ServiceDescription, message);
                    reqElementNamespace = message.Parts[0].Element == null ? message.Parts[0].Type.Namespace : message.Parts[0].Element.Namespace;
                    break;
                }
            }

            // If request element name is null set to default for serializer
            reqElementName = reqElementName == null ? serviceOp.Operation.Name + "Request" : reqElementName;

            // If request type null set to default "void"
            reqTypeName = reqTypeName == null ? "void" : reqTypeName;

            // Set the callback method name from Action.
            string methodName = serviceOp.OutAction;
            if (methodName == null)
                methodName = serviceOp.Operation.Name;
            else
            {
                int index = methodName.LastIndexOfAny(new char[] { ':', '/', '\\' });
                methodName = (index == -1 || index == (methodName.Length - 1)) ? methodName : methodName.Substring(index + 1);
            }

            // Create the callback handler method
            CodeMemberMethod codeMethod = new CodeMemberMethod();
            codeMethod.Name = methodName;
            codeMethod.Attributes = MemberAttributes.Public;
            codeMethod.ReturnType = new CodeTypeReference("WsMessage"); //"Byte", 1);
            codeMethod.Parameters.Add(new CodeParameterDeclarationExpression("WsMessage", "request"));
            //codeMethod.Parameters.Add(new CodeParameterDeclarationExpression("WsWsaHeader", "header"));
            //codeMethod.Parameters.Add(new CodeParameterDeclarationExpression("XmlReader", "reader"));

            // If message body is empty skip read serializer
            if (reqTypeName != "void")
            {
                codeMethod.Statements.Add(new CodeCommentStatement("Build request object"));
                // Generated Code: OperationNameDataContractSerializer reqDcs;
                codeMethod.Statements.Add(
                    new CodeVariableDeclarationStatement(
                        CodeGenUtils.GenerateDcsName(reqTypeName),
                        "reqDcs"
                    )
                );
                // Generated Code: reqDcs = new OperationNameDataContractSerializer("InMessagePartName", InputMessageTypeNamespace);
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
            if (clientProxy.EncodingType == MessageEncodingType.Mtom)
                codeMethod.Statements.Add(
                    new CodeAssignStatement(
                        new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("reqDcs"), "BodyParts"),
                        new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "BodyParts")
                    )
                );
            
            if (reqTypeName != "void")
            {
                // Generated Code: InMessagePartName req;
                codeMethod.Statements.Add(
                    new CodeVariableDeclarationStatement(
                        reqTypeName,
                        "req"
                    )
                );
                // req = (typeName)reqDcs.ReadObject(reader);
                codeMethod.Statements.Add(
                    new CodeAssignStatement(
                        new CodeVariableReferenceExpression("req"),
                        new CodeCastExpression(
                            reqTypeName,
                            new CodeMethodInvokeExpression(
                                new CodeVariableReferenceExpression("reqDcs"),
                                "ReadObject",
                                new CodeFieldReferenceExpression( 
                                    new CodeVariableReferenceExpression("request"), 
                                    "Reader"
                                )
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

            codeMethod.Statements.Add(new CodeSnippetStatement(""));
            codeMethod.Statements.Add(new CodeCommentStatement("Call service operation to process request."));

            // Add OneWay call to device developer supplied service method implementation
            if (reqTypeName != "void")
            {
                // Generated Code: m_class.MethodName(req);
                codeMethod.Statements.Add(
                    new CodeMethodInvokeExpression(
                        new CodeVariableReferenceExpression("m_eventHandler"),
                        serviceOp.Operation.Name,
                        new CodeExpression[] {
                            new CodeVariableReferenceExpression("req")
                        }
                    )
                );
            }
            else
            {
                // Generated Code: m_class.MethodName();
                codeMethod.Statements.Add(
                    new CodeMethodInvokeExpression(
                        new CodeVariableReferenceExpression("m_eventHandler"),
                        serviceOp.Operation.Name,
                        new CodeExpression[] { }
                    )
                );
            }

            codeMethod.Statements.Add(new CodeSnippetStatement(""));
            codeMethod.Statements.Add(new CodeCommentStatement("Return OneWayResponse message for event callback messages"));
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

        private CodeMemberMethod BuildClientProxyMethod(ServiceOp serviceOp, IList<Message> messages, MessageEncodingType encodingType)
        {
            string reqElementName = null;
            string reqTypeName = null;
            string reqElementNamespace = null;
            string respElementName = null;
            string respTypeName = null;
            string respElementNamespace = null;
            foreach (Message message in messages)
            {
                if (serviceOp.Operation.Messages.Input != null && serviceOp.Operation.Messages.Input.Message.Name == message.Name)
                {
                    if (message.Parts.Count != 0)
                    {
                        reqElementName = CodeGenUtils.GetMessageElementName(serviceOp.Operation.PortType.ServiceDescription, message);
                        reqTypeName = CodeGenUtils.GetMessageTypeName(serviceOp.Operation.PortType.ServiceDescription, message);
                        reqElementNamespace = message.Parts[0].Element == null ? message.Parts[0].Type.Namespace : message.Parts[0].Element.Namespace;
                    }
                }
                else if (serviceOp.Operation.Messages.Output != null && serviceOp.Operation.Messages.Output.Message.Name == message.Name)
                {
                    if (message.Parts.Count != 0)
                    {
                        respElementName = CodeGenUtils.GetMessageElementName(serviceOp.Operation.PortType.ServiceDescription, message);
                        respTypeName = CodeGenUtils.GetMessageTypeName(serviceOp.Operation.PortType.ServiceDescription, message);
                        respElementNamespace = message.Parts[0].Element == null ? message.Parts[0].Type.Namespace : message.Parts[0].Element.Namespace;
                    }
                }
            }

            // If either element name is null set to default for serializer naming
            reqElementName = reqElementName == null ? serviceOp.Operation.Name + "Request" : reqElementName;
            respElementName = respElementName == null ? serviceOp.Operation.Name + "Response" : respElementName;

            // If either type name is null set to default "void"
            reqTypeName = reqTypeName == null ? "void" : reqTypeName;
            respTypeName = respTypeName == null ? "void" : respTypeName;

            // Set the method name from Action.
            string methodName = serviceOp.InAction;
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
            if (respTypeName != "void")
                codeMethod.ReturnType = new CodeTypeReference(respTypeName);
            else
                codeMethod.ReturnType = new CodeTypeReference(typeof(void));

            if (reqTypeName != "void")
                codeMethod.Parameters.Add(new CodeParameterDeclarationExpression(reqTypeName, "req"));

            codeMethod.Statements.Add(new CodeSnippetStatement(""));
            codeMethod.Statements.Add(new CodeCommentStatement("Create request header"));
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
                    new CodePrimitiveExpression(serviceOp.InAction)
                )
            );
            // Generated Code: WsWsaHeader header;
            codeMethod.Statements.Add(
                new CodeVariableDeclarationStatement(
                    "WsWsaHeader",
                    "header"
                )
            );
            // Generated Code: header = new WsWsaHeader(action, null, EndpointAddress, m_version.AnonymousUri, null, null);
            codeMethod.Statements.Add(
                new CodeAssignStatement(
                    new CodeVariableReferenceExpression("header"),
                    new CodeObjectCreateExpression(
                        "WsWsaHeader",
                        new CodeExpression[] {
                            new CodeVariableReferenceExpression("action"),
                            new CodePrimitiveExpression(null),
                            new CodeVariableReferenceExpression("EndpointAddress"),
                                new CodeFieldReferenceExpression(
                                    new CodeVariableReferenceExpression("m_version"), "AnonymousUri"),
                            new CodePrimitiveExpression(null),
                            new CodePrimitiveExpression(null),
                        }
                    )
                )
            );

            CodeExpression obj;

            if (reqTypeName != "void")
            {
                obj = new CodeVariableReferenceExpression("req");
            }
            else
            {
                obj = new CodePrimitiveExpression(null);
            }

            // Generated Code:  WsMessage request = new WsMessage(header, req, WsPrefix.None);
            codeMethod.Statements.Add(
                new CodeVariableDeclarationStatement("WsMessage", "request",
                    new CodeObjectCreateExpression(
                        "WsMessage",
                        new CodeVariableReferenceExpression("header"),
                        obj,
                        new CodeFieldReferenceExpression( 
                            new CodeVariableReferenceExpression("WsPrefix"), 
                            "None" 
                        )
                    )
                )
            );

            // If message body is empty skip request seralizer
            if (reqTypeName != "void")
            {
                codeMethod.Statements.Add(new CodeSnippetStatement(""));
                codeMethod.Statements.Add(new CodeCommentStatement("Create request serializer"));
                // requestTypeNameDataContractSerializer eventDcs;
                codeMethod.Statements.Add(
                    new CodeVariableDeclarationStatement(
                        CodeGenUtils.GenerateDcsName(reqTypeName),
                        "reqDcs"
                    )
                );
                // reqDcs = new requestTypeNameDataContractSerializer("reqTypeName", ServiceNamespace.Namespace);
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

                // Generated Code: request.Serializer = reqDcs;
                codeMethod.Statements.Add(
                    new CodeAssignStatement(
                        new CodeFieldReferenceExpression( new CodeVariableReferenceExpression("request"), "Serializer" ),
                        new CodeVariableReferenceExpression( "reqDcs" )
                    )
                );

            }

            codeMethod.Statements.Add(
                new CodeAssignStatement(
                    new CodeFieldReferenceExpression( new CodeVariableReferenceExpression("request"), "Method" ),
                    new CodePrimitiveExpression( methodName )
                )
            );


            

            codeMethod.Statements.Add(new CodeSnippetStatement(""));
            //codeMethod.Statements.Add(new CodeCommentStatement("Build soap request message"));
            //// Generated Code: byte[] soapBuffer = SoapMessageBuilder.BuildSoapMessage(header, reqDcs, req);
            //CodeExpression reqDcsParam;
            //CodeExpression reqParam;
            //if (reqTypeName == "void")
            //{
            //    reqDcsParam = new CodePrimitiveExpression(null);
            //    reqParam = new CodePrimitiveExpression(null);
            //}
            //else
            //{
            //    reqDcsParam = new CodeVariableReferenceExpression("reqDcs");
            //    reqParam = new CodeVariableReferenceExpression("req");
            //}

            //codeMethod.Statements.Add(
            //    new CodeVariableDeclarationStatement(
            //        typeof(byte[]),
            //        "soapBuffer",
            //        new CodeMethodInvokeExpression(
            //            new CodeVariableReferenceExpression("SoapMessageBuilder"),
            //            "BuildSoapMessage",
            //            new CodeExpression[] {
            //                new CodeVariableReferenceExpression("header"),
            //                reqDcsParam,
            //                reqParam
            //            }
            //        )
            //    )
            //);

            // If this is an mtom message create body parts collection
            // If the message encoding is Mtom set the mtom encoded flag
            if (encodingType == MessageEncodingType.Mtom)
            {
                codeMethod.Statements.Add(new CodeSnippetStatement(""));
                codeMethod.Statements.Add(new CodeCommentStatement("Indicate that this message will use Mtom encoding"));
                // Generated Code: request.BodyParts = new WsMtomBodyParts();
                codeMethod.Statements.Add(
                    new CodeAssignStatement(
                        new CodeFieldReferenceExpression( new CodeVariableReferenceExpression("request"), "BodyParts" ),
                        new CodeObjectCreateExpression("WsMtomBodyParts")
                    )
                );


                //codeMethod.Statements.Add(new CodeCommentStatement("Build mtom request message and store as first Mtom Bodypart"));
                //// Generated Code: MessageType = WsMessageType.Mtom;
                //codeMethod.Statements.Add(
                //    new CodeAssignStatement(
                //        new CodeVariableReferenceExpression("MessageType"),
                //        new CodeFieldReferenceExpression(
                //            new CodeVariableReferenceExpression("WsMessageType"),
                //            "Mtom"
                //        )
                //    )
                //);
                // WsMtomBodyParts bodyParts = new WsMtomBodyParts();
                //codeMethod.Statements.Add(
                //    new CodeVariableDeclarationStatement("WsMtomBodyParts",
                //        "bodyParts",
                //        new CodeObjectCreateExpression("WsMtomBodyParts", new CodeExpression[] {})  
                //    )
                //);
                //// Generated Code: bodyParts.Start = "<soap@soap>";
                //codeMethod.Statements.Add(
                //    new CodeAssignStatement(
                //        new CodeFieldReferenceExpression(
                //            new CodeVariableReferenceExpression("bodyParts"),
                //            "Start"
                //        ),
                //        new CodePrimitiveExpression("<soap@soap>")
                //    )
                //);
                //// bodyParts.Add(respDcs.CreateNewBodyPart(soapBuffer, "soap@soap"));
                //codeMethod.Statements.Add(
                //    new CodeMethodInvokeExpression(
                //        new CodeVariableReferenceExpression("bodyParts"),
                //        "Add",
                //        new CodeExpression[] {
                //            new CodeMethodInvokeExpression(
                //                new CodeVariableReferenceExpression("reqDcs"),
                //                "CreateNewBodyPart",
                //                new CodeExpression[] {
                //                    new CodeVariableReferenceExpression("soapBuffer"),
                //                    new CodePrimitiveExpression("<soap@soap>")
                //                }
                //            )
                //        }
                //    )
                //);
                //// bodyParts.Add(reqDcs.BodyParts[0]);
                //codeMethod.Statements.Add(
                //    new CodeMethodInvokeExpression(
                //        new CodeVariableReferenceExpression("bodyParts"),
                //        "Add",
                //        new CodeExpression[] {
                //            new CodeIndexerExpression(
                //                new CodeFieldReferenceExpression(
                //                    new CodeVariableReferenceExpression("reqDcs"),
                //                    "BodyParts"
                //                ),
                //                new CodeExpression[] {
                //                    new CodePrimitiveExpression(0)
                //                }
                //            )
                //        }
                //    )
                //);
            }
            codeMethod.Statements.Add(new CodeSnippetStatement(""));
            codeMethod.Statements.Add(new CodeCommentStatement("Send service request"));
            
            // Generated Code: DpwsSoapResponse response;
            //codeMethod.Statements.Add(
            //    new CodeVariableDeclarationStatement("DpwsSoapResponse", "response")
            //);
            //// Depending on whether this is an mtom request or not
            //// Generated Code: response = m_httpClient.SendRequest(BodyParts, SimpleService, true, false);
            //// or
            //// Generated Code: response = m_httpClient.SendRequest(soapBuffer, SimpleService, true, false);
            //codeMethod.Statements.Add(
            //    new CodeAssignStatement(
            //        new CodeVariableReferenceExpression("response"),
            //        new CodeMethodInvokeExpression(
            //            new CodeVariableReferenceExpression("m_httpClient"),
            //            "SendRequest",
            //            new CodeExpression[] {
            //                new CodeVariableReferenceExpression(encodingType == MessageEncodingType.Mtom ? "ref bodyParts" : "soapBuffer"),
            //                new CodeVariableReferenceExpression("EndpointAddress"),
            //                new CodePrimitiveExpression(serviceOp.Operation.Messages.Flow == OperationFlow.OneWay || respTypeName == "void" ? true : false),
            //                new CodePrimitiveExpression(false)
            //            }
            //        )
            //    )
            //);

            // Generated Code: m_requestChannel.Open();
            codeMethod.Statements.Add(
                new CodeMethodInvokeExpression(
                    new CodeVariableReferenceExpression("m_requestChannel"),
                    "Open"
                )
            );

            if (serviceOp.Operation.Messages.Flow == OperationFlow.OneWay || (respTypeName == "void"))
            {
                // Generated Code: m_requestChannel.RequestOneWay(request);
                codeMethod.Statements.Add(
                    new CodeMethodInvokeExpression(
                        new CodeVariableReferenceExpression("m_requestChannel"),
                        "RequestOneWay",
                        new CodeVariableReferenceExpression("request")
                    )
                );
            }
            else
            {
                // Generated Code: WsMessage response = m_requestChannel.Request(request);

                codeMethod.Statements.Add(
                    new CodeVariableDeclarationStatement("WsMessage", "response",
                        new CodeMethodInvokeExpression(
                            new CodeVariableReferenceExpression("m_requestChannel"),
                            "Request",
                            new CodeVariableReferenceExpression("request")
                        )
                    )
                );
            }

            // Generated Code: m_requestChannel.Open();
            codeMethod.Statements.Add(
                new CodeMethodInvokeExpression(
                    new CodeVariableReferenceExpression("m_requestChannel"),
                    "Close"
                )
            );
            

            if (serviceOp.Operation.Messages.Flow == OperationFlow.RequestResponse || serviceOp.Operation.Messages.Flow == OperationFlow.RequestResponse)
            {

                if (respTypeName != "void")
                {
                    codeMethod.Statements.Add(new CodeSnippetStatement(""));
                    codeMethod.Statements.Add(new CodeCommentStatement("Process response"));
                    // Generated Code: TwoWayResponseDataContractSerializer respDcs;
                    codeMethod.Statements.Add(
                        new CodeVariableDeclarationStatement(
                            CodeGenUtils.GenerateDcsName(respTypeName),
                            "respDcs"
                        )
                    );
                    // Generated Code: respDcs = new ResponseDataTypeNameDataContractSerializer("ResponseDataTypeName", "http://schemas.example.org/SimpleService");
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

                    // If the encoding type is mtom copy the body parts content to the response object
                    if (encodingType == MessageEncodingType.Mtom)
                    {
                        // respDcs.BodyParts = bodyParts;
                        codeMethod.Statements.Add(
                            new CodeAssignStatement(
                                new CodeFieldReferenceExpression(
                                    new CodeVariableReferenceExpression("respDcs"),
                                    "BodyParts"
                                ),
                                new CodeFieldReferenceExpression( 
                                    new CodeVariableReferenceExpression("response"),
                                    "BodyParts" 
                                )
                                //new CodeVariableReferenceExpression("bodyParts")
                            )
                        );
                    }
                    
                    // Generated Code: TwoWayResponse resp;
                    codeMethod.Statements.Add(
                        new CodeVariableDeclarationStatement(respTypeName, "resp")
                    );
                    // Generated Code: resp = (TwoWayResponse)respDcs.ReadObject(response.Reader);
                    codeMethod.Statements.Add(
                        new CodeAssignStatement(
                            new CodeVariableReferenceExpression("resp"),
                            new CodeCastExpression(
                                respTypeName,
                                new CodeMethodInvokeExpression(
                                    new CodeVariableReferenceExpression("respDcs"),
                                    "ReadObject",
                                    new CodeExpression[] {
                                        new CodeFieldReferenceExpression(
                                            new CodeVariableReferenceExpression("response"),
                                            "Reader"
                                        )
                                    }
                                )
                            )
                        )
                    );

                    // Add response.Reader.Dispose() method call
                    codeMethod.Statements.Add( 
                        new CodeMethodInvokeExpression(
                            new CodeFieldReferenceExpression( 
                                new CodeVariableReferenceExpression("response"), 
                                "Reader"
                            ),
                            "Dispose", new CodeExpression[] { }
                        )
                    );

                    // reqest.Reader = null;
                    codeMethod.Statements.Add(
                        new CodeAssignStatement(
                            new CodeFieldReferenceExpression( 
                                new CodeVariableReferenceExpression("response"), 
                                "Reader"
                            ),
                            new CodePrimitiveExpression(null)
                        )
                    );
                                
                    // return resp;
                    codeMethod.Statements.Add(
                        new CodeMethodReturnStatement(
                            new CodeVariableReferenceExpression("resp")
                        )
                    );
                }
                else
                {
                    // return null;
                    codeMethod.Statements.Add(
                        new CodeMethodReturnStatement()
                    );
                }
            }

            return codeMethod;
        }

        internal void GenerateCode(string filename, ClientProxies clientProxies)
        {
            // Create Hosted Service file stream
            FileStream cpStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);
            StreamWriter cpStreamWriter = new StreamWriter(cpStream);

            // Write the auto generated header
            cpStreamWriter.Write(AutoGenTextHeader.Message);

            // Set up data contract code generator
            CSharpCodeProvider cSharpCP = new CSharpCodeProvider();
            ICodeGenerator hsCodeGen = cSharpCP.CreateGenerator(cpStreamWriter);
            CodeGeneratorOptions codeGenOptions = new CodeGeneratorOptions();
            codeGenOptions.BracingStyle = "C";

            CodeNamespace cpCodeNamespace = new CodeNamespace(clientProxies.Namespace);

            // Add hosted service using directives
            CodeSnippetCompileUnit compileUnit = new CodeSnippetCompileUnit("using System;");
            hsCodeGen.GenerateCodeFromCompileUnit(compileUnit, cpStreamWriter, codeGenOptions);
            compileUnit.Value = "using System.Xml;";
            hsCodeGen.GenerateCodeFromCompileUnit(compileUnit, cpStreamWriter, codeGenOptions);
            compileUnit.Value = "using Dpws.Client;";
            hsCodeGen.GenerateCodeFromCompileUnit(compileUnit, cpStreamWriter, codeGenOptions);
            compileUnit.Value = "using Dpws.Client.Discovery;";
            hsCodeGen.GenerateCodeFromCompileUnit(compileUnit, cpStreamWriter, codeGenOptions);
            compileUnit.Value = "using Dpws.Client.Eventing;";
            hsCodeGen.GenerateCodeFromCompileUnit(compileUnit, cpStreamWriter, codeGenOptions);
            //compileUnit.Value = "using Dpws.Client.Transport;";
            //hsCodeGen.GenerateCodeFromCompileUnit(compileUnit, cpStreamWriter, codeGenOptions);
            compileUnit.Value = "using Ws.Services;";
            hsCodeGen.GenerateCodeFromCompileUnit(compileUnit, cpStreamWriter, codeGenOptions);
            compileUnit.Value = "using Ws.Services.Utilities;";
            hsCodeGen.GenerateCodeFromCompileUnit(compileUnit, cpStreamWriter, codeGenOptions);
            compileUnit.Value = "using Ws.Services.Binding;";
            hsCodeGen.GenerateCodeFromCompileUnit(compileUnit, cpStreamWriter, codeGenOptions);
            compileUnit.Value = "using Ws.Services.Soap;";
            hsCodeGen.GenerateCodeFromCompileUnit(compileUnit, cpStreamWriter, codeGenOptions);

            foreach (ClientProxy clientProxy in clientProxies)
            {
                if (clientProxy.EncodingType == MessageEncodingType.Mtom)
                {
                    compileUnit.Value = "using Ws.Services.Mtom;";
                    hsCodeGen.GenerateCodeFromCompileUnit(compileUnit, cpStreamWriter, codeGenOptions);
                    break;
                }
            }
            compileUnit.Value = "using Ws.Services.WsaAddressing;";
            hsCodeGen.GenerateCodeFromCompileUnit(compileUnit, cpStreamWriter, codeGenOptions);
            compileUnit.Value = "using Ws.Services.Xml;";
            hsCodeGen.GenerateCodeFromCompileUnit(compileUnit, cpStreamWriter, codeGenOptions);
            compileUnit.Value = "";
            hsCodeGen.GenerateCodeFromCompileUnit(compileUnit, cpStreamWriter, codeGenOptions);
            compileUnit.Namespaces.Add(cpCodeNamespace);

            foreach (ClientProxy clientProxy in clientProxies)
            {
                // Class Declaration
                // Generated Code: public class ServiceName : DpwsClient
                CodeTypeDeclaration codeType = new CodeTypeDeclaration();
                codeType.Name = clientProxy.Name + "ClientProxy";
                codeType.TypeAttributes = TypeAttributes.Public;
                codeType.IsClass = true;
                CodeTypeReference codeBaseRef = new CodeTypeReference("DpwsClient");
                codeType.BaseTypes.Add(codeBaseRef);

                // Check operations for any notify flow pattern. If pattern is found add event callback processing
                bool hasEvents = false;
                foreach (ServiceOp operation in clientProxy.ServiceOperations)
                {
                    if (operation.Operation.Messages.Flow == OperationFlow.Notification && operation.Operation.Messages.Flow != OperationFlow.None)
                    {
                        hasEvents = true;
                        break;
                    }
                }

                CodeMemberField codeMember = null;
                if (hasEvents)
                {
                    // Add callback implementation field
                    // Generated Code: private IEventingServiceCallback m_eventHandler = null;
                    codeMember = new CodeMemberField(clientProxy.CallbackInterfaceName, "m_eventHandler");
                    codeMember.InitExpression = new CodePrimitiveExpression(null);
                    codeMember.Attributes = MemberAttributes.Private;
                    codeType.Members.Add(codeMember);
                    // Add event source service types collection
                    // Generated Code: public DpwsServiceTypes EventSources = new DpwsServiceTypes();
                    codeMember = new CodeMemberField("DpwsServiceTypes", "EventSources");
                    codeMember.InitExpression = new CodeObjectCreateExpression("DpwsServiceTypes", new CodeExpression[] { });
                    codeMember.Attributes = MemberAttributes.Public;
                    codeType.Members.Add(codeMember);
                }

                // Add client endpoint field
                // Generated Code: public string ServiceEndpoint = null;
                //codeMember = new CodeMemberField(typeof(string), "ServiceEndpoint");
                //codeMember.InitExpression = new CodePrimitiveExpression(null);
                //codeMember.Attributes = MemberAttributes.Public;
                //codeType.Members.Add(codeMember);

                // Add IRequestChannel field
                // Generated Code: private IRequestChannel m_requestChannel;
                codeMember = new CodeMemberField("IRequestChannel", "m_requestChannel");
                codeMember.InitExpression = new CodePrimitiveExpression(null);
                codeMember.Attributes = MemberAttributes.Private;
                codeType.Members.Add(codeMember);

                // Add an HttpClient
                // Generated Code: DpwsHttpClient httpClient = new DpwsHttpClient();
                //codeMember = new CodeMemberField("DpwsHttpClient", "m_httpClient");
                //codeMember.InitExpression = new CodeObjectCreateExpression("DpwsHttpClient", new CodeExpression[] { });
                //codeMember.Attributes = MemberAttributes.Private;
                //codeType.Members.Add(codeMember);
                
                // Build constructors
                codeType.Members.Add(BuildFirstConstructor(hasEvents, clientProxy));

                // Build proxy methods and callback handlers
                foreach (ServiceOp serviceOp in clientProxy.ServiceOperations)
                {
                    if (serviceOp.Operation.Messages.Flow == OperationFlow.OneWay || serviceOp.Operation.Messages.Flow == OperationFlow.RequestResponse)
                    {
                        // Display operation info
                        Logger.WriteLine("Creating client proxy method:", LogLevel.Verbose);
                        Logger.WriteLine("\tOperation Name: " + serviceOp.Operation.Name, LogLevel.Verbose);
                        codeType.Members.Add(BuildClientProxyMethod(serviceOp, clientProxy.Messages, clientProxy.EncodingType));
                    }
                    else if (serviceOp.Operation.Messages.Flow == OperationFlow.Notification)
                    {
                        // Display operation info
                        Logger.WriteLine("Creating event callback handler:", LogLevel.Verbose);
                        Logger.WriteLine("\tEvent handler name: " + serviceOp.Operation.Name, LogLevel.Verbose);
                        codeType.Members.Add(BuildProxyCallbackMethod(serviceOp, clientProxy));
                    }
                    else
                        throw new ArgumentException("Unsupported operation type detected. " + serviceOp.Operation.Messages.Flow.ToString());
                }

                cpCodeNamespace.Types.Add(codeType);
            }
            
            // Generate hosted service source file
            hsCodeGen.GenerateCodeFromNamespace(cpCodeNamespace, cpStreamWriter, codeGenOptions);
            cpStreamWriter.Flush();
            cpStreamWriter.Close();
        }
    }
}
