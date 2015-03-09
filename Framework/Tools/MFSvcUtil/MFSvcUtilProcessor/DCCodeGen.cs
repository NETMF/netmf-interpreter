using System;
using System.Collections;
using System.Text;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Web.Services.Description;
using System.Xml.Schema;
using System.Xml;

namespace Ws.SvcUtilCodeGen
{
    /// <summary>
    /// Class generates data contract source files.
    /// </summary>
    class DCCodeGen
    {
        private enum BindingStyle
        {
            Rpc = 0,
            Document = 1,
        }
        
        DCSCodeGen m_dcsCodeGen = new DCSCodeGen();
        private MessageEncodingType m_encodingType = MessageEncodingType.Soap;
        private bool m_wsiCompliant = true;
        private bool m_rpcStyleSupport = false;
        private CodeNamespaceCollection m_codeNamespaces = null;

        public DCCodeGen()
        {
        }

        // Collection used to store the compile unit namespaces generated while parsing schemas.
        internal CodeNamespaceCollection CodeNamespaces
        {
            set
            {
                m_codeNamespaces = value;
                m_dcsCodeGen.CodeNamespaces = m_codeNamespaces;
            }
        }

        // Flag used to tell the method generators if the messages are mtom encoded
        internal MessageEncodingType EncodingType { get { return m_encodingType; } set { m_encodingType = value; } }
        
        // Flag used to enforce ws-i message binding sytle conformance
        internal bool WsiCompliant { set { m_wsiCompliant = value; } }

        // Flag used to enable or disable parsing of Rpc sytle bindings
        internal bool RpcBindingStyleSupport { set { m_rpcStyleSupport = value; } }

        /// <summary>
        /// For a given wsdl service operation, generate a data contract and data contract serializer for
        /// each element used by the operation.
        /// </summary>
        /// <param name="operationMessage">A OperationMessage type containing an OperationInput or OperationOutput type.</param>
        /// <param name="messageParts">A collection of message parts used by the operation.</param>
        /// <param name="messageTypes">An arraylist of message elements and or types used by the operation.</param>
        public void GenerateContracts(OperationMessage operationMessage, MessagePartCollection messageParts, ArrayList messageTypes)
        {
            // If style = document and use = literal
            // Doc/Lit must have no parts or 1 to n elements
            if (CodeGenUtils.IsDocumentLiteral(operationMessage))
            {
                // Document literal parts can only contain element references. Do a quick check.
                for (int i = 0; i < messageTypes.Count; ++i)
                    if (!(messageTypes[0] is XmlSchemaElement))
                        throw new XmlException("Invalid wsdl:message element: " +
                            ((XmlSchemaType)messageTypes[0]).Name +
                            ". Document/Literal message parts must contain element attributes or no attributes.");
                
                CodeTypeDeclaration codeType = new CodeTypeDeclaration();

                // Check for wrapped style
                bool isWrapped = false;
                if (messageParts != null && messageParts.Count > 0 && CodeGenUtils.IsWrapped(messageParts[0]))
                    isWrapped = true;

                // If this is wrapped and has more than one message part throw
                if (isWrapped && messageParts.Count > 1)
                    throw new XmlException("Invalid number of message parts for document/literal/wrapped message \"" + operationMessage.Message.Name + "\".");

                // If wrapped process single element. For now assume as long as there is one part this is wrapped.
                if (isWrapped || messageParts.Count == 1)
                {
                    XmlSchemaElement element = (XmlSchemaElement)messageTypes[0];
                    CodeNamespace codeNs = CodeGenUtils.GetDotNetNamespace(m_codeNamespaces, element.QualifiedName.Namespace);
                    BuildElementType(element, codeType);
                }
                else
                {
                    // If the WS-I compliance flag is set document/literal can have at most one
                    // message part so throw if we get here
                    if (m_wsiCompliant)
                        throw new XmlException("Invalid number of message parts for WS-I compliant document/literal message \"" + operationMessage.Message.Name + "\".");

                    // To simplify parameter passing to and from serailizers, like Rpc/Literal, seperate doc/lit
                    // element parts (parameters) are wrapped in an object. This will not affect the format of the
                    // actual XML messages, it will only affect parameters and return types exchnged by the
                    // serializers and implementation contracts. 

                    // Generate Document style contracts
                    GenerateWrappedContracts(BindingStyle.Document, operationMessage, messageParts, messageTypes);
                }
            }
            // Else this is an rpc/literal message
            else
            {
                if (!m_rpcStyleSupport)
                    throw new XmlException("Rpc binding style is not supported. Operation = \"" + operationMessage.Operation.Name + "\".");
                
                // For rpc literal parts can only contain type references. Do a quick check.
                for (int i = 0; i < messageTypes.Count; ++i)
                    if (messageTypes[0] is XmlSchemaElement)
                        throw new XmlException("Invalid wsdl:message type: " +
                            ((XmlSchemaElement)messageTypes[0]).Name +
                            ". Rpc/Literal message parts must contain type attributes or no attributes.");

                // Generate Rpc style contracts
                GenerateWrappedContracts(BindingStyle.Rpc, operationMessage, messageParts, messageTypes);
            }
        }

        /// <summary>
        /// For a given wsdl service operation, generate an Rpc or document style wrapped data contract
        /// and data contract serializer.
        /// </summary>
        /// <param name="BindingStyle">A flag indicating the binding style.</param>
        /// <param name="operationMessage">A OperationMessage type containing an OperationInput or OperationOutput type.</param>
        /// <param name="messageParts">A collection of message parts used by the operation.</param>
        /// <param name="messageTypes">An arraylist of message elements and or types used by the operation.</param>
        private void GenerateWrappedContracts(BindingStyle bindingStyle, OperationMessage operationMessage, MessagePartCollection messageParts, ArrayList messageTypes)
        {
            if (messageTypes.Count == 0)
                return;
            
            // Rules: Rpc/Literal support only. WS-I basic profile Rpc/Literal rules apply.
            
            // Create a temporary wrapper element type that will be used to wrap the message parts.
            // As per rpc/literal spec the name of the type is the name of the operation
            // If the message is a response the name = the operation name + "Response".
            // The temporary wrapper element is stored in a new schema that will be added to the existing
            // ServiceDescription schema set and recompiled to resolve all type references.
            XmlSchema schema = operationMessage.Operation.PortType.ServiceDescription.Types.Schemas[0];
            XmlSchemaSet schemaSet = new XmlSchemaSet();
            XmlSchemaElement wrapperElement = new XmlSchemaElement();
            schema.Items.Add(wrapperElement);
            string elementName = operationMessage.Operation.Name;
            elementName = operationMessage is OperationOutput ? elementName + "Response" : elementName + "Request";
            wrapperElement.Name = elementName;

            // The element namespace is the target namespace by default else it is 
            // defined by the binding/operation/soap12Body/namespace attribute
            schema.TargetNamespace = CodeGenUtils.GetOperationNamespace(operationMessage);

            // Add a new complex type to the element
            XmlSchemaComplexType complexType = new XmlSchemaComplexType();
            wrapperElement.SchemaType = complexType;

            // Add a sequence particles to the complex type
            XmlSchemaSequence sequence = new XmlSchemaSequence();
            complexType.Particle = sequence;

            // Loop through the list of parts and add thier definitions to the sequence
            for (int i = 0; i < messageTypes.Count; ++i)
            {
                if (bindingStyle == BindingStyle.Rpc)
                {
                    XmlSchemaElement typeElement = new XmlSchemaElement();
                    typeElement.Name = messageParts[i].Name;
                    typeElement.SchemaTypeName = new XmlQualifiedName(messageParts[i].Type.Name, messageParts[i].Type.Namespace);
                    sequence.Items.Add(typeElement);
                    schema.Items.Add((XmlSchemaType)messageTypes[i]);
                }
                else
                    sequence.Items.Add((XmlSchemaElement)messageTypes[i]);
            }

            // Add the new schema containing the new element to a set and compile
            schemaSet.Add(schema);
            schemaSet.Compile();

            // Build the new element and serializer
            CodeTypeDeclaration codeType = new CodeTypeDeclaration();
            CodeNamespace codeNs = CodeGenUtils.GetDotNetNamespace(m_codeNamespaces, wrapperElement.QualifiedName.Namespace);
            BuildElementType(wrapperElement, codeType);
            if (!CodeGenUtils.TypeExists(codeNs, codeType.Name))
            {
                codeNs.Types.Add(codeType);
                m_dcsCodeGen.BuildDataContractSerializer(m_encodingType, codeType, codeNs);
            }

            // Swap the existing message parts with a new replacement part. All subsequent processing
            // will use the new single message part
            foreach (Message message in operationMessage.Operation.PortType.ServiceDescription.Messages)
            {
                if (message.Name == operationMessage.Message.Name)
                {
                    MessagePart replacementPart = new MessagePart();
                    replacementPart.Element = new XmlQualifiedName(elementName);
                    replacementPart.Type = wrapperElement.QualifiedName;
                    replacementPart.Name = operationMessage.Name;
                    replacementPart.Namespaces = operationMessage.Namespaces;
                    message.Parts.Clear();
                    message.Parts.Add(replacementPart);
                    break;
                }
            }
        }
         
        private CodeExpression GetInitExpression(string defaultValue, string typeName)
        {
            CodeExpression initExpression = null;
            if (defaultValue != null && CodeGenUtils.GetClrType(typeName) != null)
                if (typeName == "System.String")
                    initExpression = new CodePrimitiveExpression(typeName);
                else
                    initExpression = new CodeMethodInvokeExpression(
                        new CodeTypeReferenceExpression("XmlConvert"),
                        CodeGenUtils.GetConvertMethod(typeName),
                        new CodeExpression[] { new CodePrimitiveExpression(defaultValue) });
            return initExpression;
        }

        private void WriteClassDeclaration(string typeName, string typeNamespace, CodeTypeDeclaration codeType)
        {
            // Display class info
            Logger.WriteLine("Creating DataContract:", LogLevel.Verbose);
            Logger.WriteLine("\tClass Name: " + typeName, LogLevel.Verbose);
            Logger.WriteLine("\tClass Namespace: " + typeNamespace, LogLevel.Verbose);

            CodeAttributeArgument codeAttr = new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(typeNamespace));
            CodeAttributeDeclaration codeAttrDecl = new CodeAttributeDeclaration("DataContract", codeAttr);
            codeType.CustomAttributes.Add(codeAttrDecl);

            // Add type declaration
            codeType.Name = typeName;
            codeType.TypeAttributes = TypeAttributes.Public;
            codeType.IsClass = true;
        }

        private void WriteAttributeField(XmlSchemaAttribute attribute, CodeTypeDeclaration codeType, string defaultValue)
        {
            // Convert Xml Schema Type to ClrType if this is a native type
            CodeTypeDeclaration attribCodeType = new CodeTypeDeclaration();
            string typeName = null;
            if (attribute.AttributeSchemaType.Content != null)
                typeName = BuildSimpleType(attribute.AttributeSchemaType, attribCodeType);
            typeName = typeName == null ? "System.String" : typeName;

            // Write the attribute field
            Logger.WriteLine("Adding Attribute:", LogLevel.Verbose);
            Logger.WriteLine("\tMember Name: " + attribute.QualifiedName.Name, LogLevel.Verbose);
            Logger.WriteLine("\tMember Namespace: " + attribute.QualifiedName.Namespace, LogLevel.Verbose);
            Logger.WriteLine("\tMember Type: " + typeName, LogLevel.Verbose);

            // Create DataMember attribute
            CodeAttributeArgument codeAttr = new CodeAttributeArgument("IsAttribute", new CodePrimitiveExpression(true));
            CodeAttributeDeclaration codeAttrDecl = new CodeAttributeDeclaration("DataMember", codeAttr);

            // Add field
            CodeMemberField codeMember = new CodeMemberField(typeName, attribute.QualifiedName.Name);
            CodeExpression initExpression = GetInitExpression(defaultValue, typeName);
            if (initExpression != null)
                codeMember.InitExpression = initExpression;
            codeMember.Attributes = MemberAttributes.Public;
            codeMember.CustomAttributes.Add(codeAttrDecl);
            codeType.Members.Add(codeMember);
        }

        private void WriteAnyAttribute(XmlSchemaAnyAttribute anyAttribute, CodeTypeDeclaration codeType)
        {
            Logger.WriteLine("Adding DataMember:", LogLevel.Verbose);
            Logger.WriteLine("\tMember Name: " + "AnyAttr", LogLevel.Verbose);
            Logger.WriteLine("\tMember Namespace: " + anyAttribute.Namespace, LogLevel.Verbose);
            Logger.WriteLine("\tMember Type: " + "XmlAttribute[]", LogLevel.Verbose);

            // Create DataMember attribute
            CodeAttributeDeclaration codeAttrDecl = new CodeAttributeDeclaration("DataMember");
            codeAttrDecl.Arguments.Add(new CodeAttributeArgument("IsNillable", new CodePrimitiveExpression(true)));
            codeAttrDecl.Arguments.Add(new CodeAttributeArgument("IsRequired", new CodePrimitiveExpression(false)));

            // Add field
            CodeMemberField codeMember = new CodeMemberField("XmlAttribute", "AnyAttr");
            codeMember.Type.ArrayRank = 1;
            codeMember.Attributes = MemberAttributes.Public;
            codeMember.CustomAttributes.Add(codeAttrDecl);
            codeType.Members.Add(codeMember);
        }

        private void WriteAnyElement(decimal minOccurs, decimal maxOccurs, bool isNillable, XmlSchemaAny anyElement, CodeTypeDeclaration codeType)
        {
            Logger.WriteLine("Adding DataMember:", LogLevel.Verbose);
            Logger.WriteLine("\tMember Name: " + "Any", LogLevel.Verbose);
            Logger.WriteLine("\tMember Namespace: " + anyElement.Namespace, LogLevel.Verbose);
            Logger.WriteLine("\tMember Type: " + "XmlElement[]", LogLevel.Verbose);

            // Create DataMember custom attribute
            CodeAttributeDeclaration codeAttrDecl = new CodeAttributeDeclaration("DataMember");
            if (isNillable == true)
                codeAttrDecl.Arguments.Add(new CodeAttributeArgument("IsNillable", new CodePrimitiveExpression(true)));
            if (minOccurs == 0)
                codeAttrDecl.Arguments.Add(new CodeAttributeArgument("IsRequired", new CodePrimitiveExpression(false)));
            // need to add Min and Max occurs actual value to enable collectionprocessing in DataContractSerializers

            // Add field
            CodeMemberField codeMember = new CodeMemberField("XmlElement", "Any");
            codeMember.Type.ArrayRank = 1;
            codeMember.Attributes = MemberAttributes.Public;
            codeMember.CustomAttributes.Add(codeAttrDecl);
            codeType.Members.Add(codeMember);
        }

        private void WriteDataMember(int orderNumber, decimal minOccurs, decimal maxOccurs, bool isNillable, string memberName, string memberNamespace, string SchemaTypeName, string defaultValue, CodeTypeDeclaration codeType)
        {
            Logger.WriteLine("Adding DataMember:", LogLevel.Verbose);
            Logger.WriteLine("\tMember Name: " + memberName, LogLevel.Verbose);
            Logger.WriteLine("\tMember Namespace: " + memberNamespace, LogLevel.Verbose);
            Logger.WriteLine("\tMember Type: " + SchemaTypeName, LogLevel.Verbose);

            // Create DataMember custom attribute
            CodeAttributeDeclaration codeAttrDecl = new CodeAttributeDeclaration("DataMember");
            if (orderNumber >= 0)
                codeAttrDecl.Arguments.Add(new CodeAttributeArgument("Order", new CodePrimitiveExpression(orderNumber)));
            if (isNillable == true)
                codeAttrDecl.Arguments.Add(new CodeAttributeArgument("IsNillable", new CodePrimitiveExpression(true)));
            if (minOccurs == 0)
                codeAttrDecl.Arguments.Add(new CodeAttributeArgument("IsRequired", new CodePrimitiveExpression(false)));
            // need to add Min and Max occurs actual value to enable collectionprocessing in DataContractSerializers

            // Is this is an array set array flag and strip []
            bool isArray = SchemaTypeName.IndexOf("[]") == SchemaTypeName.Length - 2 ? true : false;
            SchemaTypeName = isArray ? SchemaTypeName.Substring(0, SchemaTypeName.Length - 2) : SchemaTypeName;
            
            // If this is a native type convert Xml Schema Type to ClrType
            string typeName = CodeGenUtils.GetClrType(SchemaTypeName);
            typeName = typeName == null ? SchemaTypeName : typeName;

            // Add field
            CodeMemberField codeMember = new CodeMemberField(typeName, memberName);
            if (isArray)
            {
                codeMember.Type.ArrayElementType = new CodeTypeReference(typeName);
                codeMember.Type.ArrayRank = 1;
            }
            CodeExpression initExpression = GetInitExpression(defaultValue, typeName);
            if (initExpression != null)
                codeMember.InitExpression = initExpression;
            codeMember.Attributes = MemberAttributes.Public;
            codeMember.CustomAttributes.Add(codeAttrDecl);
            codeType.Members.Add(codeMember);
        }

        private void AddBaseType(XmlSchemaType baseType)
        {
            // If the base type is not a native type create base type contract
            if (CodeGenUtils.GetClrType(baseType.QualifiedName.Name) == null)
            {
                CodeNamespace codeNs = CodeGenUtils.GetDotNetNamespace(m_codeNamespaces, baseType.QualifiedName.Namespace);
                CodeTypeDeclaration baseCodeType = new CodeTypeDeclaration();
                WriteClassDeclaration(baseType.QualifiedName.Name, baseType.QualifiedName.Namespace, baseCodeType);
                BuildComplexType((XmlSchemaComplexType)baseType, baseCodeType);

                // If this is a new type add it to the namespace
                if (!CodeGenUtils.TypeExists(codeNs, baseCodeType.Name))
                {
                    codeNs.Types.Add(baseCodeType);
                    m_dcsCodeGen.BuildDataContractSerializer(m_encodingType, baseCodeType, codeNs);
                }
            }
        }

        private void CreateEnumeration(XmlSchemaSimpleTypeRestriction restriction, CodeTypeDeclaration codeType)
        {
            Logger.WriteLine("\tRestriction base type: " + restriction.BaseTypeName, LogLevel.Verbose);

            // Process facets
            foreach (XmlSchemaFacet facet in restriction.Facets)
            {
                int enumValue = -1;
                bool IsEnum = false;

                Logger.WriteLine("\t\tFacet Type: " + facet.GetType().ToString(), LogLevel.Verbose);
                Logger.WriteLine("\t\tFacet Value: " + facet.Value, LogLevel.Verbose);

                // If this is an Enum facet
                if (facet is XmlSchemaEnumerationFacet)
                {
                    IsEnum = true;
                    if (facet.Annotation != null)
                    {
                        foreach (XmlSchemaObject item in facet.Annotation.Items)
                        {
                            if (item is XmlSchemaAppInfo)
                            {
                                enumValue = Convert.ToInt32(((XmlSchemaAppInfo)item).Markup[0].InnerText);
                                break;
                            }
                            enumValue = -1;
                        }
                    }

                    // Validate the enum string. If the first char is numeric or any character is not Alpha Numeric throw
                    string enumTestValue = facet.Value.ToLower();
                    string enumFacet = "";
                    if (enumTestValue.Length < 64 && (enumTestValue[0] >= 'a' && facet.Value[0] <= 'z' || enumTestValue[0] == '_'))
                    {
                        for (int i = 0; i < enumTestValue.Length; ++i)
                        {
                            if ((enumTestValue[i] < '0' || (enumTestValue[i] > '9' && enumTestValue[i] < 'a') || enumTestValue[i] > 'z') && enumTestValue[i] != '_')
                                continue;
                            enumFacet += facet.Value[i];
                        }
                    }
                    else
                        throw new XmlException("Invalid enumeration value. " + facet.Value);
                    
                    // Add enum member to data contract
                    CodeMemberField codeMember = new CodeMemberField(enumFacet, enumFacet);

                    // Since valid XMLSchema enum values may contain values that cannot be used as real C# enum values a mechanism
                    // is required to map XmlSchema value names to real enum value names. If an XmlSchema enum value name cannot be
                    // used for a real enum value, a UserData dictionary entry is created containing the real facet value
                    // (the XmlSchema value name) that is used by serializers to map between the real enum and the XmlSchema enum.
                    if (enumFacet != facet.Value)
                    {
                        codeMember.UserData.Add(typeof(XmlSchemaEnumerationFacet), facet);
                    }
                    if (Convert.ToInt32(enumValue) >= 0)
                        codeMember.InitExpression = new CodePrimitiveExpression(enumValue);
                    CodeAttributeDeclaration fieldAttrDecl = new CodeAttributeDeclaration("EnumMember");
                    codeMember.CustomAttributes.Add(fieldAttrDecl);
                    codeMember.Attributes = MemberAttributes.Public;
                    codeType.Members.Add(codeMember);
                }
                else
                {
                    // Per WCF DataContract processing rules, length, minlength, maxlength,
                    // whitespace and pattern are are forbidden elements in a simple type enumeration
                    // If this is not an enumeration only restriction and simple type are supported
                    // all others are ignored.
                    Type facetType = facet.GetType();
                    if (IsEnum && (facetType == typeof(XmlSchemaLengthFacet) ||
                        facetType == typeof(XmlSchemaMinLengthFacet) ||
                        facetType == typeof(XmlSchemaMaxLengthFacet) ||
                        facetType == typeof(XmlSchemaWhiteSpaceFacet) ||
                        facetType == typeof(XmlSchemaPatternFacet)))
                    {
                        Logger.WriteLine("SimpleType facet " + facetType.ToString() +
                            " was ingnored in the enumeration.", LogLevel.Normal);
                        continue;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            return;
        }

        private void BuildSimpleContent(XmlSchemaSimpleContent simpleContent, CodeTypeDeclaration codeType)
        {
            // SimpleContent can contain Extension, Restriction or Annotation(ignored) elements
            if (simpleContent.Content is XmlSchemaSimpleContentExtension)
            {
                // Extension has a base type and can contain attribute, anyAttribute,
                // attributeGroup(not supported) or annotation(ignored) elements

                // Get the extension type
                XmlSchemaSimpleContentExtension extension = (XmlSchemaSimpleContentExtension)simpleContent.Content;

                // Add Attributes
                foreach (XmlSchemaAttribute attribute in extension.Attributes)
                {
                    WriteAttributeField(attribute, codeType, attribute.DefaultValue);
                }

                // Add anyAttribute if not null
                if (extension.AnyAttribute != null)
                    WriteAnyAttribute(extension.AnyAttribute, codeType);
            }
            else if (simpleContent.Content.GetType() == typeof(XmlSchemaSimpleContentRestriction))
            {
                // Restriction has a base attribute and can contain attribute, anyAttribute, simpleType, attributeGroup(not supported) or annotation(ignored) elements
                // and facets
                XmlSchemaSimpleContentRestriction restriction = (XmlSchemaSimpleContentRestriction)simpleContent.Content;

                // Add Attributes
                foreach (XmlSchemaAttribute attribute in restriction.Attributes)
                {
                    WriteAttributeField(attribute, codeType, attribute.DefaultValue);
                }

                // need to add restriction facet processing
                foreach (XmlSchemaFacet facet in restriction.Facets)
                {
                }

                // Add anyAttribute if not null
                if (restriction.AnyAttribute != null)
                    WriteAnyAttribute(restriction.AnyAttribute, codeType);
            }
        }

        private bool HasEnumFacets(XmlSchemaSimpleTypeRestriction restriction)
        {
            foreach (XmlSchemaFacet facet in restriction.Facets)
            {
                if (facet is XmlSchemaEnumerationFacet)
                    return true;
            }
            return false;
        }

        private string BuildSimpleType(XmlSchemaSimpleType simpleType, CodeTypeDeclaration codeType)
        {

            // SimpleType could have annotation, restriction|list|union content.
            // Currently unions are represented using strings, Only enumeration restrictions are supported,
            // annotation is ignored unless it is used to set an enum value
            // and list are represented as an array of the base type.

            string typeName = CodeGenUtils.GetClrType(simpleType.TypeCode);
            typeName = typeName == "anySimpleType" ? CodeGenUtils.GetClrType(simpleType.TypeCode) : typeName;
            typeName = typeName == null ? "System.String" : typeName;

            XmlSchemaSimpleTypeContent content = simpleType.Content;

            // If the simple type is a union return string type
            if (content is XmlSchemaSimpleTypeUnion)
            {
                Logger.WriteLine("SimpleType Union restriction processing is currently not supported." +
                    " Code for type: " + simpleType.QualifiedName.Namespace + " will be converted to string type." +
                    " Line Number = " + simpleType.Content.LineNumber +
                    " Line Position: " + simpleType.Content.LinePosition, LogLevel.Normal);

                return "System.String";
            }

            // If the simple type has restrictions - if an emueration restriction is found build enum
            // DataContract and return enum type name. This version only supports enumeration restriction facets
            else if (content is XmlSchemaSimpleTypeRestriction)
            {
                XmlSchemaSimpleTypeRestriction restriction = (XmlSchemaSimpleTypeRestriction)content;

                // If the restriction facet is a simple type build the base type
                if (restriction.BaseType != null && restriction.BaseType is XmlSchemaSimpleType)
                {
                    CodeTypeDeclaration simpleCodeType = new CodeTypeDeclaration();
                    typeName = BuildSimpleType(((XmlSchemaSimpleType)restriction.BaseType), simpleCodeType);
                }
                // Process facets - Enumeration facets build a DataContract
                // All other facets are ignored and return the restriction base type name.
                // In future versions, need to add RestrictionMembers attribute and create a collection
                // of restriction facets used by a DataContractSerializer to validate types by restriction.
                else if (HasEnumFacets(restriction))
                {
                    // Create new type declaration
                    CodeTypeDeclaration enumCodeType = new CodeTypeDeclaration();
                    enumCodeType.IsEnum = true;
                    CreateEnumeration(restriction, enumCodeType);
                    enumCodeType.Name = simpleType.Name;
                    enumCodeType.TypeAttributes = TypeAttributes.Public;
                    CodeAttributeArgument codeAttr = new CodeAttributeArgument("Namespace", new CodePrimitiveExpression(simpleType.QualifiedName.Namespace));
                    CodeAttributeDeclaration codeAttrDecl = new CodeAttributeDeclaration("DataContract", codeAttr);
                    enumCodeType.CustomAttributes.Add(codeAttrDecl);

                    // Get a code namespace
                    CodeNamespace codeNs = CodeGenUtils.GetDotNetNamespace(m_codeNamespaces, simpleType.QualifiedName.Namespace);

                    // Add new enum contract to namespace
                    if (!CodeGenUtils.TypeExists(codeNs, enumCodeType.Name))
                    {
                        codeNs.Types.Add(enumCodeType);
                        m_dcsCodeGen.BuildDataContractSerializer(m_encodingType, enumCodeType, codeNs);
                    }
                    typeName = enumCodeType.Name;
                }
                else
                {
                    if (typeName != null && CodeGenUtils.IsNativeClrType(typeName))
                        codeType.BaseTypes.Add(new CodeTypeReference(typeName));
                }

                // Ignore all other restriction facets for now

            }
            // If this is a simple type list return base type array type name or if the list contains
            // Enumeration restriction facets, create DataContract and return DataContract array type name.
            // This version only supports enumeration restriction facets
            else if (content is XmlSchemaSimpleTypeList)
            {
                XmlSchemaSimpleTypeList simpleTypeList = (XmlSchemaSimpleTypeList)simpleType.Content;
                XmlSchemaSimpleType baseSimpleType = (XmlSchemaSimpleType)simpleTypeList.BaseItemType;
               
                // If the list contains a native type create and array
                if (simpleTypeList.ItemTypeName != null && CodeGenUtils.GetClrType(simpleTypeList.ItemTypeName.Name) != null)
                {
                    typeName = CodeGenUtils.GetClrType(simpleTypeList.ItemTypeName.Name) + "[]";
                }
                // If item type is null try to build base simple type
                else if (baseSimpleType.Content is XmlSchemaSimpleTypeRestriction)
                {
                    // In this version, if the base type is a string recurse in case the content is an enum,
                    // if the base type is something else build an array of the base type.
                    // In future versions, need to add RestrictionMembers attribute and create a collection
                    // of restriction facets used by a DataContractSerializer to validate types by restriction.
                    baseSimpleType.Name = simpleType.Name;
                    if (((XmlSchemaSimpleTypeRestriction)baseSimpleType.Content).BaseTypeName.Name == "string")
                    {
                        CodeTypeDeclaration simpleCodeType = new CodeTypeDeclaration();
                        typeName = BuildSimpleType(baseSimpleType, simpleCodeType);
                    }
                    else
                        typeName = CodeGenUtils.GetClrType(baseSimpleType.TypeCode) + "[]";
                }
                else
                {
                    Logger.WriteLine("SimpleType list has unsupported child. " +
                        " Code for type: " + simpleType.QualifiedName.Namespace + " will be generated as a string." +
                        " Line Number = " + simpleType.Content.LineNumber +
                        " Line Position: " + simpleType.Content.LinePosition, LogLevel.Normal);
                    return null;
                }
            }

            // Return string containing xml schema type name
            return typeName;
        }

        private void BuildComplexContent(XmlSchemaComplexContent complexContent, CodeTypeDeclaration codeType)
        {
            // If this is an extension type
            if (complexContent.Content.GetType() == typeof(XmlSchemaComplexContentExtension))
            {
                XmlSchemaComplexContentExtension extension = (XmlSchemaComplexContentExtension)complexContent.Content;

                // Add Attributes
                foreach (XmlSchemaAttribute attribute in extension.Attributes)
                {
                    WriteAttributeField(attribute, codeType, attribute.DefaultValue);
                }

                // (Mod:11-26-08) If the particle is null return after adding attributes. The derived type is does not change the base type.
                if (extension.Particle == null)
                    return;

                // If extension has sequence particles
                if (extension.Particle.GetType() == typeof(XmlSchemaSequence))
                {
                    ProcessSequence((XmlSchemaSequence)extension.Particle, codeType);
                }
                else if (extension.Particle.GetType() == typeof(XmlSchemaChoice))
                {
                    ProcessChoice((XmlSchemaChoice)extension.Particle, codeType);
                }
                else if (extension.Particle.GetType() == typeof(XmlSchemaAll))
                {
                    ProcessAll((XmlSchemaAll)extension.Particle, codeType);
                }

                // Add anyAttribute if not null
                if (extension.AnyAttribute != null)
                    WriteAnyAttribute(extension.AnyAttribute, codeType);
            }
            // Else if this is a type restriction
            else if (complexContent.Content.GetType() == typeof(XmlSchemaComplexContentRestriction))
            {
                // need to add code to handle complex type restrictions here
                XmlSchemaComplexContentRestriction restriction = (XmlSchemaComplexContentRestriction)complexContent.Content;

                // (Mod:11-26-08) If the particle is null return after adding attributes. The derived type is does not change the base type.
                if (restriction.Particle == null)
                    return;
            }
        }

        public void BuildComplexType(XmlSchemaComplexType complexType, CodeTypeDeclaration codeType)
        {
            XmlSchemaParticle particle = complexType.Particle;
            XmlSchemaContentModel contentModel = complexType.ContentModel;

            // Add defined attributes
            foreach (XmlSchemaAttribute attribute in complexType.Attributes)
            {
                WriteAttributeField(attribute, codeType, attribute.DefaultValue);
            }

            // If the complex type contains sequence particles it is a complex type that restricts
            // anytype so parse here and add data members
            if (particle != null)
            {
                if (particle.GetType() == typeof(XmlSchemaSequence))
                    ProcessSequence((XmlSchemaSequence)particle, codeType);
                else if (particle.GetType() == typeof(XmlSchemaChoice))
                    ProcessChoice((XmlSchemaChoice)particle, codeType);
                else if (particle.GetType() == typeof(XmlSchemaAll))
                    ProcessAll((XmlSchemaAll)particle, codeType);
                else
                    throw new XmlSchemaException("Invalid particle type: " + particle.ToString());
            }
            // If complex type contains content element process content
            else if (contentModel != null)
            {
                // If this is not a native schema type add the type to the base type list
                if (CodeGenUtils.GetClrType(complexType.BaseXmlSchemaType.TypeCode) == null)
                {
                    // Get the namespace
                    CodeNamespace codeNs = CodeGenUtils.GetDotNetNamespace(m_codeNamespaces, complexType.QualifiedName.Namespace);
                    string baseTypeNs = CodeGenUtils.GenerateDotNetNamespace(complexType.BaseXmlSchemaType.QualifiedName.Namespace);
                    CodeTypeReference codeBaseRef = null;
                    if (baseTypeNs == codeNs.Name)
                        codeBaseRef = new CodeTypeReference(complexType.BaseXmlSchemaType.QualifiedName.Name);
                    else
                        codeBaseRef = new CodeTypeReference(baseTypeNs + "." + complexType.BaseXmlSchemaType.QualifiedName.Name);

                    codeType.BaseTypes.Add(codeBaseRef);
                }
                // Add value member
                else
                {
                    string baseTypeName = complexType.BaseXmlSchemaType.QualifiedName.Name;
                    
                    // If the base type is a simple type list fix up the type name
                    if (complexType.BaseXmlSchemaType.DerivedBy == XmlSchemaDerivationMethod.List)
                        baseTypeName = CodeGenUtils.GetClrType(complexType.BaseXmlSchemaType.TypeCode) + "[]";
                    WriteDataMember(-1, 1, 1, false, "Value", null, baseTypeName, null, codeType);
                }

                // Process simple content type
                if (contentModel.GetType() == typeof(XmlSchemaSimpleContent))
                {
                    // Process content
                    BuildSimpleContent((XmlSchemaSimpleContent)contentModel, codeType);

                }
                // Process complex content type
                else if (contentModel.GetType() == typeof(XmlSchemaComplexContent))
                {
                    // Process content
                    BuildComplexContent((XmlSchemaComplexContent)contentModel, codeType);
                }

                // If the base type is a complex type create base type else build a simple type
                if (complexType.BaseXmlSchemaType is XmlSchemaComplexType)
                    AddBaseType(complexType.BaseXmlSchemaType);
            }

            // If anyAttribute exist write it last 
            if (complexType.AnyAttribute != null)
            {
                WriteAnyAttribute((XmlSchemaAnyAttribute)complexType.AnyAttribute, codeType);
            }
        }

        private void ProcessSequence(XmlSchemaSequence sequence, CodeTypeDeclaration codeType)
        {
            int seqIndex = 0;

            foreach (object item in sequence.Items)
            {
                if (item.GetType() == typeof(XmlSchemaElement))
                {
                    ProcessElement((XmlSchemaElement)item, ref seqIndex, codeType);
                }
                else if (item.GetType() == typeof(XmlSchemaAny))
                {
                    WriteAnyElement(((XmlSchemaAny)item).MinOccurs, ((XmlSchemaAny)item).MaxOccurs, true, (XmlSchemaAny)item, codeType);
                }
                else if (item.GetType() == typeof(XmlSchemaChoice))
                {
                    ProcessChoice((XmlSchemaChoice)item, codeType);
                }
                else if (item.GetType() == typeof(XmlSchemaAll))
                {
                    ProcessAll((XmlSchemaAll)item, codeType);
                }
                else if (item.GetType() == typeof(XmlSchemaGroup))
                {
                    ProcessGroup((XmlSchemaGroup)item, codeType);
                }
                else if (item.GetType() == typeof(XmlSchemaAnnotation))
                {
                    // Ignore for now
                    return;
                }
                else
                    throw new XmlSchemaException("Invalid particle type: " + item.ToString());
            }
            return;
        }

        private void ProcessElement(XmlSchemaElement element, ref int seqIndex, CodeTypeDeclaration codeType)
        {
            string elementName = element.QualifiedName.Name;
            string elementTypeName;
            if (element.RefName.IsEmpty)
                elementTypeName = element.SchemaTypeName.Name;
            else
                elementTypeName = element.RefName.Name;
            elementTypeName = elementTypeName == "" ? "String" : elementTypeName;

            // if the WSDL identifies a element name that is an internal C# type then we must change it.
            if (CodeGenUtils.IsIntrinsicName(elementName))
            {
                elementName = elementName.ToUpper();
            }

            // Create DataMember attribute
            CodeAttributeDeclaration codeAttrDecl = new CodeAttributeDeclaration("DataMember");
            codeAttrDecl.Arguments.Add(new CodeAttributeArgument("Order", new CodePrimitiveExpression(seqIndex)));
            ++seqIndex;
            if (element.IsNillable)
                codeAttrDecl.Arguments.Add(new CodeAttributeArgument("IsNillable", new CodePrimitiveExpression(true)));
            if (element.MinOccurs == 0)
                codeAttrDecl.Arguments.Add(new CodeAttributeArgument("IsRequired", new CodePrimitiveExpression(false)));

            // Get Field type
            string typeName = CodeGenUtils.GetClrType(elementTypeName);
            bool isNativeType = typeName == null ? false : true;
            typeName = typeName == null ? elementTypeName : typeName;

            // If this is not a native XSD type build the new type.
            // Create a new code declaration and add it after the element is built
            if (!isNativeType)
            {
                XmlSchemaType type = element.ElementSchemaType;
                if (type.GetType() == typeof(XmlSchemaComplexType))
                {
                    // Add type declaration
                    CodeTypeDeclaration complexCodeType = new CodeTypeDeclaration();
                    WriteClassDeclaration(element.ElementSchemaType.QualifiedName.Name, element.ElementSchemaType.QualifiedName.Namespace, complexCodeType);
                    BuildComplexType((XmlSchemaComplexType)type, complexCodeType);

                    // If a typeName for this element is always the name of the last recursive type created
                    typeName = complexCodeType.Name;
                    typeName = element.MaxOccurs > 1 ? typeName + "[]" : typeName;

                    // Get a namespace
                    CodeNamespace codeNs = CodeGenUtils.GetDotNetNamespace(m_codeNamespaces, element.ElementSchemaType.QualifiedName.Namespace);

                    if (!CodeGenUtils.TypeExists(codeNs, complexCodeType.Name))
                    {
                        codeNs.Types.Add(complexCodeType);
                        m_dcsCodeGen.BuildDataContractSerializer(m_encodingType, complexCodeType, codeNs);
                    }
                }
                else if (type.GetType() == typeof(XmlSchemaSimpleType))
                {

                    // Create new CodeTypeDeclaration and call this in case we have to build an enum
                    CodeTypeDeclaration simpleCodeType = new CodeTypeDeclaration();
                    typeName = BuildSimpleType((XmlSchemaSimpleType)type, simpleCodeType);

                    isNativeType = null != CodeGenUtils.GetClrType(typeName.TrimEnd(new char[]{'[',']'}));
                }
            }

            // Add field member
            CodeMemberField codeMember = new CodeMemberField();
            codeMember.Name = elementName;
            if (typeName.IndexOf("[]") == typeName.Length - 2)
                codeMember.Type.ArrayElementType = new CodeTypeReference(typeName.Substring(0, typeName.Length - 2), 1);
            string typeNamespace = "";
            if (!isNativeType && element.QualifiedName.Namespace != element.ElementSchemaType.QualifiedName.Namespace)
            {
                typeNamespace = CodeGenUtils.GenerateDotNetNamespace(element.ElementSchemaType.QualifiedName.Namespace);
                typeNamespace = typeNamespace == null ? "" : typeNamespace + ".";
            }
            codeMember.Type.BaseType = typeNamespace + typeName;
            CodeExpression initExpression = GetInitExpression(element.DefaultValue, typeName);
            if (initExpression != null)
                codeMember.InitExpression = initExpression;
            codeMember.Attributes = MemberAttributes.Public;
            codeMember.CustomAttributes.Add(codeAttrDecl);
            codeType.Members.Add(codeMember);
            return;
        }
        
        private void ProcessChoice(XmlSchemaChoice choice, CodeTypeDeclaration codeType)
        {
            foreach (object item in choice.Items)
            {
                if (item.GetType() == typeof(XmlSchemaElement))
                {
                    int seqNo = -1;
                    ProcessElement((XmlSchemaElement)item, ref seqNo, codeType);
                }
                else if (item.GetType() == typeof(XmlSchemaAny))
                {
                    WriteAnyElement(((XmlSchemaAny)item).MinOccurs, ((XmlSchemaAny)item).MaxOccurs, true, (XmlSchemaAny)item, codeType);
                }
                else if (item.GetType() == typeof(XmlSchemaSequence))
                {
                    ProcessAll((XmlSchemaAll)item, codeType);
                }
                else if (item.GetType() == typeof(XmlSchemaGroup))
                {
                    ProcessGroup((XmlSchemaGroup)item, codeType);
                }
                else if (item.GetType() == typeof(XmlSchemaChoice))
                {
                    ProcessChoice((XmlSchemaChoice)item, codeType);
                }
                else if (item.GetType() == typeof(XmlSchemaAnnotation))
                {
                    // Ignore for now
                    return;
                }
                else
                    throw new XmlSchemaException("Invalid particle type: " + item.ToString());
            }
        }

        private void ProcessAll(XmlSchemaAll all, CodeTypeDeclaration codeType)
        {
            foreach (object item in all.Items)
            {
                if (item.GetType() == typeof(XmlSchemaElement))
                {
                    int seqNo = -1;
                    ProcessElement((XmlSchemaElement)item, ref seqNo, codeType);
                }
                else if (item.GetType() == typeof(XmlSchemaAnnotation))
                {
                    // Ignore for now
                    return;
                }
                else
                    throw new XmlSchemaException("Invalid particle type: " + item.ToString());
            }
        }

        private void ProcessGroup(XmlSchemaGroup group, CodeTypeDeclaration codeType)
        {
                if (group.Particle.GetType() == typeof(XmlSchemaSequence))
                {
                    ProcessSequence((XmlSchemaSequence)group.Particle, codeType);
                }
                else if (group.Particle.GetType() == typeof(XmlSchemaChoice))
                {
                    ProcessChoice((XmlSchemaChoice)group.Particle, codeType);
                }
                else if (group.Particle.GetType() == typeof(XmlSchemaAll))
                {
                    ProcessAll((XmlSchemaAll)group.Particle, codeType);
                }
                else
                    throw new XmlSchemaException("Invalid particle type: " + group.Particle.ToString());
        }

        public void BuildElementType(XmlSchemaElement element, CodeTypeDeclaration codeType)
        {
            CodeNamespace codeNs = CodeGenUtils.GetDotNetNamespace(m_codeNamespaces, element.QualifiedName.Namespace);

            // If this is a complex type build data contracts and add code ttype to code namespace
            if (element.ElementSchemaType.GetType() == typeof(XmlSchemaComplexType))
            {
                // Create a codeDom type declaration
                WriteClassDeclaration(element.QualifiedName.Name, element.QualifiedName.Namespace, codeType);

                // If this type already exists return
                if (CodeGenUtils.TypeExists(codeNs, element.QualifiedName.Name))
                    return;

                BuildComplexType((XmlSchemaComplexType)element.ElementSchemaType, codeType);
                codeNs.Types.Add(codeType);
                m_dcsCodeGen.BuildDataContractSerializer(m_encodingType, codeType, codeNs);
            }
            // If this simple type defines an enum an enum data contract will be created and added to the
            // code namespace otherwise theres no need to add a contract because the message contract refenences
            // a type instead of an element.
            else if (element.ElementSchemaType.GetType() == typeof(XmlSchemaSimpleType))
            {
                // Create a codeDom type declaration
                WriteClassDeclaration(element.ElementSchemaType.QualifiedName.Name, element.ElementSchemaType.QualifiedName.Namespace, codeType);

                // If this type already exists return
                if (CodeGenUtils.TypeExists(codeNs, element.ElementSchemaType.QualifiedName.Name))
                    return;

                string typeName = BuildSimpleType((XmlSchemaSimpleType)element.ElementSchemaType, codeType);

                // If the simpleType is an enum a Serializer has already been created.
                if (codeType.IsEnum)
                    return;

                // If this is a native xml schema type a DataContract is not created instead a
                // native type DataContractSerializer
                if (typeName != null && typeName.Length > 7 && typeName.Substring(0, 7) == "System.")
                {
                    string nativeTypeName = typeName.Substring(7);
                    
                    // If the type is an array make a special name for the DataContractSerializer
                    if (typeName.Length > 2 && typeName.Substring(typeName.Length - 2) == "[]")
                        nativeTypeName = nativeTypeName.TrimEnd(new char[] { '[', ']' }) + "Array";
                    m_dcsCodeGen.BuildNativeDataContractSerializer(typeName, nativeTypeName, codeNs);
                }
                else
                    m_dcsCodeGen.BuildDataContractSerializer(m_encodingType, codeType, codeNs);
            }
            return;
        }

    }
}
