using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;

namespace Ws.SvcUtilCodeGen
{
    class DCSCodeGen
    {
        private MessageEncodingType m_encodingType = MessageEncodingType.Soap;
        private static int m_cid = 0;
        private CodeNamespaceCollection m_codeNamespaces = null;

        public DCSCodeGen()
        {
        }

        // Collection used to store the compile unit namespaces generated while parsing schemas.
        public CodeNamespaceCollection CodeNamespaces { set { m_codeNamespaces = value; } }

        public void BuildDataContractSerializer(MessageEncodingType encodingType, CodeTypeDeclaration codeType, CodeNamespace codeNs)
        {
            string className = codeType.Name + "DataContractSerializer";
            string defaultNamespace = "";

            // If this serializer has already been created return
            if (CodeGenUtils.TypeExists(codeNs, className))
                return;

            // Set encoding type
            m_encodingType = encodingType;

            // Find namespace attribute value
            foreach (CodeAttributeDeclaration attribute in codeType.CustomAttributes)
            {
                if (attribute.Name == "DataContract")
                    foreach (CodeAttributeArgument argument in attribute.Arguments)
                        if (argument.Name == "Namespace")
                            defaultNamespace = ((CodePrimitiveExpression)argument.Value).Value.ToString();
            }

            // Display class info
            Logger.WriteLine("Creating DataContractSerializer:", LogLevel.Verbose);
            Logger.WriteLine("\tClass Name: " + className, LogLevel.Verbose);
            Logger.WriteLine("", LogLevel.Verbose);

            // Class Declaration
            CodeTypeDeclaration DCSCodeType = new CodeTypeDeclaration();
            DCSCodeType.Name = className;
            DCSCodeType.TypeAttributes = TypeAttributes.Public;
            DCSCodeType.IsClass = true;
            CodeTypeReference codeBaseRef = new CodeTypeReference("DataContractSerializer");
            DCSCodeType.BaseTypes.Add(codeBaseRef);

            // private WsMtomBodyParts m_bodyParts;
            //if (m_encodingType == MessageEncodingType.Mtom)
            //{
            //    CodeMemberField codeMember = new CodeMemberField("WsMtomBodyParts", "BodyParts");
            //    codeMember.InitExpression = new CodeObjectCreateExpression("WsMtomBodyParts");
            //    codeMember.Attributes = MemberAttributes.Public;
            //    DCSCodeType.Members.Add(codeMember);
            //}

            // Add Constructors
            DCSCodeType.Members.Add(BuildConstructor(className));
            DCSCodeType.Members.Add(BuildComplexConstructor(className));

            // If the type is derived expand it to include base type properties
            CodeTypeDeclaration newCodeType = new CodeTypeDeclaration(codeType.Name);
            if (codeType.IsClass == true)
                newCodeType.IsClass = true;
            else if (codeType.IsEnum == true)
                newCodeType.IsEnum = true;
            else if (codeType.IsInterface == true)
                newCodeType.IsInterface = true;
            else if (codeType.IsStruct == true)
                newCodeType.IsStruct = true;
            newCodeType.Attributes = codeType.Attributes;
            foreach (CodeTypeReference codeRef in codeType.BaseTypes)
                newCodeType.BaseTypes.Add(codeRef);
            foreach (CodeAttributeDeclaration codeAttr in codeType.CustomAttributes)
                newCodeType.CustomAttributes.Add(codeAttr);
            ExpandCodeType(ref newCodeType, codeType);
            foreach (CodeTypeMember codeMember in codeType.Members)
                newCodeType.Members.Add(codeMember);            

            // Add ReadObject method
            DCSCodeType.Members.Add(BuildReadObjectMethod(defaultNamespace, newCodeType, codeNs));

            // Add WriteObject method
            DCSCodeType.Members.Add(BuildWriteObjectMethod(defaultNamespace, newCodeType, codeNs));

            if (!CodeGenUtils.TypeExists(codeNs, DCSCodeType.Name))
                codeNs.Types.Add(DCSCodeType);
        }

        public void BuildNativeDataContractSerializer(string type, string typeName, CodeNamespace codeNs)
        {
            string className = typeName + "DataContractSerializer";

            // If this serializer has already been created return
            if (CodeGenUtils.TypeExists(codeNs, className))
                return;

            // Display class info
            Logger.WriteLine("Creating native type DataContractSerializer:", LogLevel.Verbose);
            Logger.WriteLine("\tField Name: " + className, LogLevel.Verbose);

            // Class Declaration
            CodeTypeDeclaration DCSCodeType = new CodeTypeDeclaration();
            DCSCodeType.Name = className;
            DCSCodeType.TypeAttributes = TypeAttributes.Public;
            DCSCodeType.IsClass = true;
            CodeTypeReference codeBaseRef = new CodeTypeReference("DataContractSerializer");
            DCSCodeType.BaseTypes.Add(codeBaseRef);

            // Add Constructor
            DCSCodeType.Members.Add(BuildConstructor(className));

            // Fix up the local field name for native types
            string localFieldName = "temp" + typeName;

            // Add ReadObject method
            DCSCodeType.Members.Add(BuildNativeReadObjectMethod(type, localFieldName));

            // Add WriteObject method
            DCSCodeType.Members.Add(BuildNativeWriteObjectMethod(type, localFieldName, codeNs));

            if (!CodeGenUtils.TypeExists(codeNs, DCSCodeType.Name))
                codeNs.Types.Add(DCSCodeType);

        }

        private enum MemberType
        {
            Field,
            Attribute,
        }

        private CodeConstructor BuildComplexConstructor(string className)
        {
            CodeConstructor constructor = new CodeConstructor();
            constructor.Name = className;
            constructor.Attributes = MemberAttributes.Public;
            constructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "rootName"));
            constructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "rootNameSpace"));
            constructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "localNameSpace"));
            constructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("rootName"));
            constructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("rootNameSpace"));
            constructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("localNameSpace"));
            return constructor;
        }

        private CodeConstructor BuildConstructor(string className)
        {
            CodeConstructor constructor = new CodeConstructor();
            constructor.Name = className;
            constructor.Attributes = MemberAttributes.Public;
            constructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "rootName"));
            constructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "rootNameSpace"));
            constructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("rootName"));
            constructor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("rootNameSpace"));
            return constructor;
        }

        private CodeMemberMethod BuildReadObjectMethod(string defaultNamespace, CodeTypeDeclaration codeType, CodeNamespace codeNs)
        {
            CodeMemberMethod codeMemberMethod = new CodeMemberMethod();
            codeMemberMethod.Attributes = MemberAttributes.Public | MemberAttributes.Override;
            codeMemberMethod.ReturnType = new CodeTypeReference(typeof(object));
            codeMemberMethod.Name = "ReadObject";
            codeMemberMethod.Parameters.Add(new CodeParameterDeclarationExpression("XmlReader", "reader"));

            // Add type declaration
            CodeVariableDeclarationStatement declaration = new CodeVariableDeclarationStatement(codeType.Name, codeType.Name + "Field");
            if (codeType.IsEnum)
                declaration.InitExpression = new CodePrimitiveExpression(0);
            else
                declaration.InitExpression = new CodePrimitiveExpression(null);
            codeMemberMethod.Statements.Add(declaration);

            // Add IsParentStartElement condition
            codeMemberMethod.Statements.Add(BuildIsParentStartElementCondition(defaultNamespace, codeType, codeNs));

            // Add return statement
            codeMemberMethod.Statements.Add(new CodeMethodReturnStatement(new CodeArgumentReferenceExpression(codeType.Name + "Field")));

            return codeMemberMethod;
        }

        private CodeMemberMethod BuildNativeReadObjectMethod(string type, string typeName)
        {
            // Create ReadObject method
            CodeMemberMethod codeMemberMethod = new CodeMemberMethod();
            codeMemberMethod.Attributes = MemberAttributes.Public | MemberAttributes.Override;
            codeMemberMethod.ReturnType = new CodeTypeReference(typeof(object));
            codeMemberMethod.Name = "ReadObject";
            codeMemberMethod.Parameters.Add(new CodeParameterDeclarationExpression("XmlReader", "reader"));

            // Create temporary variable
            CodeVariableDeclarationStatement varDecl = new CodeVariableDeclarationStatement(type, typeName);
            if (varDecl.Type.ArrayElementType == null)
            {
                if (type == "System.String")
                    varDecl.InitExpression = new CodePrimitiveExpression(null);
            }
            else
                varDecl.InitExpression = new CodeArrayCreateExpression(type, new CodeExpression[] { });
            codeMemberMethod.Statements.Add(varDecl);

            // Create temporary member field used to generate read string code
            CodeMemberField memberField = new CodeMemberField(type, typeName);

            // Create conditional read statement
            CodeStatementCollection readStatements = new CodeStatementCollection();
            if (memberField.Type.ArrayElementType != null && CodeGenUtils.IsNativeClrType(memberField.Type.ArrayElementType.BaseType))
            {
                readStatements.AddRange(BuildReadStringArrayStatements(MemberType.Field, null, memberField));
            }
            // Else if this is a native type
            else if (Type.GetType(memberField.Type.BaseType) != null)
            {
                readStatements.Add(BuildReadStringStatement(MemberType.Field, null, memberField));
            }
            else
                throw new ArgumentException("Invalid native field type for schema item: " + memberField.Name);

            // Add ReadEndElement to condition statements
            readStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("reader"),
                "ReadEndElement", new CodeExpression[] { }));

            // Add native type return statement
            readStatements.Add(new CodeMethodReturnStatement(new CodeArgumentReferenceExpression(typeName)));

            // Convert statement collection to array
            CodeStatement[] readStatementArray = new CodeStatement[readStatements.Count];
            readStatements.CopyTo(readStatementArray, 0);

            // Build IsParentStartElement if statement
            CodeVariableReferenceExpression readerParam = new CodeVariableReferenceExpression("reader");
            CodePrimitiveExpression nillableParam = new CodePrimitiveExpression(false);
            CodePrimitiveExpression requiredParam = new CodePrimitiveExpression(true);
            CodeConditionStatement condition = new CodeConditionStatement(new CodeMethodInvokeExpression(null,
                "IsParentStartElement",
                new CodeExpression[] { readerParam, nillableParam, requiredParam }),
                readStatementArray);

            // Add condition
            codeMemberMethod.Statements.Add(condition);

            // Add null return statement
            codeMemberMethod.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));

            return codeMemberMethod;
        }

        private CodeConditionStatement BuildIsParentStartElementCondition(string defaultNamespace, CodeTypeDeclaration codeType, CodeNamespace codeNs)
        {
            // Check to see if the element is nillable
            string nillable = CodeGenUtils.GetCustomAttributeArgumentValue("IsNillable", codeType.CustomAttributes);
            bool isNillable = nillable == null || nillable == "false" ? false : true;

            // Check to see if the element is required (i.e. minOccurs > 0)
            string required = CodeGenUtils.GetCustomAttributeArgumentValue("IsRequired", codeType.CustomAttributes);
            bool isRequired = required == null || required == "true" ? true : false;

            // Build ReadStartObject if statement
            CodeVariableReferenceExpression readerParam = new CodeVariableReferenceExpression("reader");
            CodePrimitiveExpression nillableParam = new CodePrimitiveExpression(isNillable);
            CodePrimitiveExpression requiredParam = new CodePrimitiveExpression(isRequired);
            CodeConditionStatement condition = new CodeConditionStatement(new CodeMethodInvokeExpression(null,
                "IsParentStartElement",
                new CodeExpression[] { readerParam, nillableParam, requiredParam }),
                // True condition code. Build read statements based on contract member type
                BuildIsParentStartElementStatements(defaultNamespace, null, codeNs, codeType)
            );
            return condition;
        }

        private CodeStatement[] BuildIsParentStartElementStatements(string defaultNamespace, string fieldName, CodeNamespace codeNs, CodeTypeDeclaration codeType)
        {

            CodeStatementCollection statements = new CodeStatementCollection();
            bool addReadEndElement = fieldName == null ? true : false;
            bool addRead = fieldName == null ? true : false;

            // New up the local DataContract object
            if (fieldName == null)
            {
                fieldName = codeType.Name + "Field";
                statements.Add(CodeAssignNewObjectToVariableStatement(fieldName, codeType.Name));
            }

            // If the type is an enum build switch statement
            if (codeType.IsEnum)
            {
                statements.AddRange(BuildReaderEnumSwitchStatement(codeType.Name + "Field", codeType, "reader.ReadString()"));
            }
            else
            {
                // Build sort order list of attribute member indexes
                int[] sortOrder = CodeGenUtils.GetListOfAttributes(codeType.Members);

                // For attributes build parsing code
                // Attributes must be parsed before Reading elements (they are parsed first)
                foreach (int index in sortOrder)
                {
                    CodeMemberField memberField = (CodeMemberField)codeType.Members[index];

                    // Create IsAttribute parameters
                    CodeVariableReferenceExpression readerParam = new CodeVariableReferenceExpression("reader");
                    CodeVariableReferenceExpression nameParam = new CodeVariableReferenceExpression("\"" + memberField.Name + "\"");

                    if (memberField.Name == "AnyAttr")
                        statements.Add(BuildReadAnyAttributeStatement(fieldName, memberField));
                    else
                    {
                        statements.Add(
                            new CodeConditionStatement(
                                new CodeMethodInvokeExpression(
                                    null,
                                    "IsAttribute",
                                    new CodeExpression[] { readerParam, nameParam }
                                ),
                                memberField.Type.ArrayElementType == null ? BuildIsAttributeStatements(MemberType.Attribute, fieldName, memberField, codeNs) : BuildIsAttributeArrayStatements(MemberType.Attribute, fieldName, memberField, codeNs)
                            )
                        );
                    }
                }

                // Add Read to the IsParentStartElement conditional statements
                // reader.Read([FieldName]);
                // Since this method is recursive provisions restrict it to the first time through only.
                if (addRead)
                {
                    statements.Add(
                        new CodeMethodInvokeExpression(
                            new CodeVariableReferenceExpression("reader"),
                            "Read",
                            new CodeExpression[] { }
                        )
                    );
                }

                // Build sort order list of field member indexes
                sortOrder = CodeGenUtils.GetListOfMembers(codeType.Members);

                // For each field member build processing code
                foreach (int index in sortOrder)
                {
                    CodeMemberField memberField = (CodeMemberField)codeType.Members[index];

                    // If this a an Any element write the parsing calls here and forgo normal child processing
                    if (memberField.Type.BaseType == "XmlElement" && memberField.Name == "Any")
                    {
                        statements.Add(BuildReadAnyElementStatement(fieldName, memberField));
                    }
                    // Else if this is an Any element write the parsing calls here to forgo normal child
                    // processing. Create special reader
                    else if (!CodeGenUtils.IsNativeClrType(memberField.Type.BaseType))
                    {
                        statements.AddRange(BuildIsChildStartElementStatements(MemberType.Attribute, defaultNamespace, fieldName, memberField, codeNs));
                    }
                    else
                    {
                        

                        string nillable = CodeGenUtils.GetCustomAttributeArgumentValue("IsNillable", memberField.CustomAttributes);
                        bool isNillable = nillable == null || nillable == "false" ? false : true;

                        // Check to see if the element is required (i.e. minOccurs > 0)
                        string required = CodeGenUtils.GetCustomAttributeArgumentValue("IsRequired", memberField.CustomAttributes);
                        bool isRequired = required == null || required == "true" ? true : false;

                        // Build ReadStartObject if statement
                        CodeVariableReferenceExpression readerParam = new CodeVariableReferenceExpression("reader");
                        CodeVariableReferenceExpression nameParam = new CodeVariableReferenceExpression("\"" + memberField.Name + "\"");
                        CodePrimitiveExpression nillableParam = new CodePrimitiveExpression(isNillable);
                        CodePrimitiveExpression requiredParam = new CodePrimitiveExpression(isRequired);
                        CodeConditionStatement condition = new CodeConditionStatement(new CodeMethodInvokeExpression(null,
                            "IsChildStartElement",
                            new CodeExpression[] { readerParam, nameParam, nillableParam, requiredParam }),
                            // True condition code. Build read statements based on contract member type
                            BuildIsChildStartElementStatements(MemberType.Attribute, defaultNamespace, fieldName, memberField, codeNs)
                        );
                        statements.Add(condition);
                    }
                }
            }

            if (addReadEndElement)
            {
                // Build ReadEndElement statement
                statements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("reader"),
                    "ReadEndElement", new CodeExpression[] { }));
            }

            // Build a CodeStatement array object to return
            CodeStatement[] codeStatements = new CodeStatement[statements.Count];
            for (int i = 0; i < statements.Count; ++i)
                codeStatements[i] = statements[i];
            return codeStatements;
        }

        private CodeStatementCollection BuildReaderEnumSwitchStatement(string fieldName, CodeTypeDeclaration codeType, string expression)
        {
            // Since CodeDom doesn't support switch this code will only work when
            // generating C#. This blows the reason for using CodeDom right now. Hopefully this will be addressed
            // in future versions of CodeDom.
            CodeStatementCollection statements = new CodeStatementCollection();
            statements.Add(new CodeSnippetStatement("\t\t\t\tswitch(" + expression + ")"));
            statements.Add(new CodeSnippetStatement("\t\t\t\t{"));
            foreach (CodeMemberField codeField in codeType.Members)
            {
                if (codeField.UserData.Contains(typeof(XmlSchemaEnumerationFacet)))
                    statements.Add(new CodeSnippetStatement("\t\t\t\t\tcase \"" + ((XmlSchemaEnumerationFacet)codeField.UserData[typeof(XmlSchemaEnumerationFacet)]).Value + "\":"));
                else
                    statements.Add(new CodeSnippetStatement("\t\t\t\t\tcase \"" + codeField.Name + "\":"));
                statements.Add(new CodeSnippetExpression("\t\t" + fieldName + " = " + codeType.Name + "." + codeField.Name));
                statements.Add(new CodeSnippetExpression("\t\tbreak"));
            }
            statements.Add(new CodeSnippetStatement("\t\t\t\t\tdefault:"));
            statements.Add(new CodeSnippetExpression("\t\tthrow new XmlException()"));
            statements.Add(new CodeSnippetStatement("\t\t\t\t}"));

            return statements;
        }

        private CodeStatement BuildReadAnyElementStatement(string classRefName, CodeMemberField memberField)
        {
            // Get IsRequired value
            bool isRequired = false;
            string isRequiredValue;
            if ((isRequiredValue = CodeGenUtils.GetCustomAttributeArgumentValue("IsRequired", memberField.CustomAttributes)) != null)
            {
                isRequired = isRequiredValue.ToLower() == "false" ? false : true;
            }

            // Build ReadString expressions
            CodeFieldReferenceExpression fieldRef = new CodeFieldReferenceExpression(
                new CodeTypeReferenceExpression(classRefName),
                "Any");

            // Build [ClassName].[FieldName] = ReadAnyElement(reader, false);
            CodeAssignStatement readStatement = new CodeAssignStatement(
                fieldRef,
                new CodeMethodInvokeExpression(
                    null,
                    "ReadAnyElement",
                    new CodeExpression[] {
                        new CodeTypeReferenceExpression("reader"),
                        new CodePrimitiveExpression(isRequired),
                    }
                )
            );

            return readStatement;
        }

        private CodeStatement BuildReadAnyAttributeStatement(string classRefName, CodeMemberField memberField)
        {
            // Build WriteString expressions
            // [ClassName].[FieldName]
            CodeFieldReferenceExpression fieldRef = new CodeFieldReferenceExpression(
                new CodeTypeReferenceExpression(classRefName),
                "AnyAttr");

            // Build [ClassName].[FieldName] = ReadAnyAttribute(reader);
            CodeAssignStatement readStatement = new CodeAssignStatement(
                fieldRef,
                new CodeMethodInvokeExpression(
                    null,
                    "ReadAnyAttribute",
                    new CodeExpression[] {
                        new CodeTypeReferenceExpression("reader"),
                    }
                )
            );

            return readStatement;
        }

        private CodeStatement[] BuildIsAttributeStatements(MemberType memberType, string classRefName, CodeMemberField memberField, CodeNamespace codeNs)
        {
            CodeStatementCollection statements = new CodeStatementCollection();

            // If this is a native schema type create IsAttribute conditional statement
            if ((memberField.Type.ArrayElementType != null && Type.GetType(memberField.Type.ArrayElementType.BaseType) != null) || Type.GetType(memberField.Type.BaseType) != null)
            {
                statements.Add(BuildReadStringStatement(MemberType.Attribute, classRefName, memberField));
            }
            // Else if this is an enum type build switch
            else
            {
                // Attempt to get the enum type
                // Note: 2-16-09 - Changed code to get memberField.Type.BaseType instead of memberField.Name - Should
                // be going after the enum type not the complex field
                CodeTypeDeclaration enumType = CodeGenUtils.GetCodeType(memberField.Type.BaseType, codeNs);
                if (enumType == null || enumType.IsEnum == false)
                    throw new XmlException();
                statements.AddRange(BuildReaderEnumSwitchStatement(classRefName + "." + memberField.Name, enumType, "reader.Value"));
            }

            // Build a CodeStatement array object to return
            CodeStatement[] codeStatements = new CodeStatement[statements.Count];
            for (int i = 0; i < statements.Count; ++i)
                codeStatements[i] = statements[i];
            return codeStatements;
        }

        private CodeStatement[] BuildIsAttributeArrayStatements(MemberType memberType, string classRefName, CodeMemberField memberField, CodeNamespace codeNs)
        {
            CodeStatementCollection statements = new CodeStatementCollection();
            string tempListName = memberField.Name + "_List";

            // Build: string[] tempArray = reader.ReadString().Split();
            CodeVariableDeclarationStatement declaration = new CodeVariableDeclarationStatement(typeof(string[]), tempListName);
            CodeExpression expression = new CodeFieldReferenceExpression(
                new CodeTypeReferenceExpression("reader"),
                "Value");
            expression = new CodeMethodInvokeExpression(
                expression,
                "Split");
            declaration.InitExpression = expression;
            statements.Add(declaration);

            // Build: ClassRefName.FieldName = new FieldType[tempListName.Length];
            // Initializes the attribute field array
            statements.Add(
                new CodeAssignStatement(
                    new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(classRefName), memberField.Name),
                        new CodeArrayCreateExpression(
                            memberField.Type.BaseType,
                            new CodeFieldReferenceExpression(
                                new CodeVariableReferenceExpression(tempListName), "Length"))));

            // Build: for (int i = 0; i < tempListName.Length; ++i)
            // Build For iterator
            CodeIterationStatement forLoop = new CodeIterationStatement(
                // initStatement parameter for pre-loop initialization.
                new CodeVariableDeclarationStatement(typeof(int), "i", new CodePrimitiveExpression(0)),
                // testExpression parameter to test for continuation condition.
                new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("i"),
                    CodeBinaryOperatorType.LessThan,
                    new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(tempListName), "Length")),
                // incrementStatement parameter indicates statement to execute after each iteration.
                new CodeAssignStatement(new CodeVariableReferenceExpression("i"),
                    new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("i"),
                        CodeBinaryOperatorType.Add, new CodePrimitiveExpression(1))),
                // statements parameter contains the statements to execute during each interation of the loop.
                new CodeStatement[] { BuildReadStringStatement(MemberType.Field, classRefName, memberField) }
            );

            // Add processing expression
            statements.Add(forLoop);
            // Build a CodeStatement array object to return
            CodeStatement[] codeStatements = new CodeStatement[statements.Count];
            for (int i = 0; i < statements.Count; ++i)
                codeStatements[i] = statements[i];
            return codeStatements;
        }

        private CodeStatement[] BuildIsChildStartElementStatements(MemberType memberType, string defaultNamespace, string classRefName, CodeMemberField memberField, CodeNamespace codeNs)
        {
            CodeStatementCollection statements = new CodeStatementCollection();
            bool addReadEndElement = true;

            // Add Read to the IsChildStartElement conditional statements
            // reader.Read();
            CodeMethodInvokeExpression ReadExpression = new CodeMethodInvokeExpression(
                new CodeVariableReferenceExpression("reader"),
                "Read",
                new CodeExpression[] { }
            );
            statements.Add(ReadExpression);

            // If the messages uses Mtom encoding and this is a byte array special mtom processing applies
            if (m_encodingType == MessageEncodingType.Mtom && memberField.Type.ArrayElementType != null && memberField.Type.ArrayElementType.BaseType == "System.Byte")
            {
                // Build statements that retreive the Mtom message info
                statements.AddRange(BuildMtomReadStringStatements(memberType, classRefName, memberField));

                addReadEndElement = false;
            }
            else
            {

                // Else if this is a native schema array type create string array reader
                if (memberField.Type.ArrayElementType != null && CodeGenUtils.IsNativeClrType(memberField.Type.ArrayElementType.BaseType))
                {
                    statements.AddRange(BuildReadStringArrayStatements(MemberType.Field, classRefName, memberField));
                }
                // Else if this is a native type
                else if (Type.GetType(memberField.Type.BaseType) != null)
                {
                    statements.Add(BuildReadStringStatement(MemberType.Field, classRefName, memberField));
                }
                // Else if this is an enum type build switch
                else
                {
                    // If this is an enum  or nested object don't write ReadEndElement
                    addReadEndElement = false;

                    // Attempt to get the enum type
                    CodeTypeDeclaration enumType = CodeGenUtils.GetCodeType(memberField.Name, codeNs);
                    if (enumType != null && enumType.IsEnum == true)
                    {
                        statements.AddRange(BuildReaderEnumSwitchStatement(enumType.Name + "Field", enumType, "reader.Value"));
                    }
                    else
                    {
                        // Clear the ReadExpression from the statements collection if we get here
                        statements.Clear();

                        // New up a DataContractSerialiser for this type
                        CodeVariableDeclarationStatement classDeclaration = new CodeVariableDeclarationStatement(memberField.Type.BaseType + "DataContractSerializer", memberField.Name + "DCS");

                        // Find the namespace of the member type which is required if there are nested namespaces in the data contract
                        string ns = CodeGenUtils.GetNamespaceFromType(m_codeNamespaces, memberField.Type.BaseType);

                        // Use the default namespace if the member type's could not be found
                        if(string.IsNullOrEmpty(ns))
                        {
                           ns = defaultNamespace;
                        }

                        classDeclaration.InitExpression = new CodeObjectCreateExpression(
                            memberField.Type.BaseType + "DataContractSerializer",
                            new CodeExpression[] { new CodePrimitiveExpression(memberField.Name), new CodePrimitiveExpression(defaultNamespace), new CodePrimitiveExpression(ns) });

                        statements.Add(classDeclaration);

                        // Add data contract serializer used to process this type
                        if (memberField.Type.ArrayElementType == null)
                        {
                            // CODEGEN: [codeField]DCS.BodyParts = this.BodyParts;
                            CodeAssignStatement bodyParts = new CodeAssignStatement(
                                new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(memberField.Name + "DCS"), "BodyParts"),
                                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "BodyParts")
                                );

                            statements.Add(bodyParts);

                            // Build [ClassName].[FieldName] = codeField[DCS].ReadObject(reader); expression
                            CodeFieldReferenceExpression fieldRef = new CodeFieldReferenceExpression(
                                new CodeTypeReferenceExpression(classRefName),
                                memberField.Name);
                            CodeCastExpression readExpression = new CodeCastExpression(memberField.Type.BaseType,
                                new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(memberField.Name + "DCS"),
                                    "ReadObject",
                                    new CodeExpression[] { new CodeTypeReferenceExpression("reader") }));

                            CodeAssignStatement convertStatement = new CodeAssignStatement(fieldRef, readExpression);

                            // Add processing expression
                            statements.Add(convertStatement);
                        }
                        //Else if the base type is an array build array processor
                        else
                        {
                            statements.AddRange(BuildElementArrayReadStatements(classRefName, memberField, defaultNamespace, codeNs));
                        }
                    }
                }
            }

            // Build ReadEndElement statement
            if (addReadEndElement)
            {
                statements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("reader"),
                    "ReadEndElement", new CodeExpression[] { }));
            }

            // Build a CodeStatement array object to return
            CodeStatement[] codeStatements = new CodeStatement[statements.Count];
            for (int i = 0; i < statements.Count; ++i)
                codeStatements[i] = statements[i];
            return codeStatements;
        }

        private CodeStatementCollection BuildElementArrayReadStatements(string fieldName, CodeMemberField memberField, string defaultNamespace, CodeNamespace codeNs)
        {
            CodeStatementCollection statements = new CodeStatementCollection();
            string tempListName = memberField.Name + "_List";
            
            CodeVariableReferenceExpression readerParam = new CodeVariableReferenceExpression("reader");
            CodeVariableReferenceExpression nameParam = new CodeVariableReferenceExpression("\"" + memberField.Name + "\"");
            CodePrimitiveExpression nillableParam = new CodePrimitiveExpression(false);
            CodePrimitiveExpression requiredParam = new CodePrimitiveExpression(false);

            // Generated Code: if (IsChildStartElement(reader, "typeName", false, false))
            CodeConditionStatement isChildCondition = new CodeConditionStatement(
                new CodeMethodInvokeExpression(
                    null,
                    "!IsChildStartElement",
                    new CodeExpression[] { readerParam, nameParam, nillableParam, requiredParam }
                ),
                new CodeStatement[] {
                    // Generated Code: FieldName.MemberName = new MemberName[tempListName.Count];
                    new CodeAssignStatement(
                        new CodeFieldReferenceExpression(
                            new CodeVariableReferenceExpression(fieldName),
                            memberField.Name
                        ),
                        new CodeArrayCreateExpression(
                            memberField.Type.BaseType,
                            new CodeFieldReferenceExpression(
                                new CodeVariableReferenceExpression(tempListName),
                                "Count"
                            )
                        )
                    ),
                    // Generated Code: tempListName.CopyTo(PartitionsField.Partition);
                    new CodeExpressionStatement(
                        new CodeMethodInvokeExpression(
                            new CodeVariableReferenceExpression(tempListName),
                            "CopyTo",
                            new CodeExpression[] {
                                new CodeFieldReferenceExpression(
                                    new CodeVariableReferenceExpression(fieldName),
                                    memberField.Name
                                )
                            }
                        )
                    ),
                    // Generated Code: break;
                    new CodeSnippetStatement("\t\t\t\t\t\tbreak;")
                }
            );

            // tempListName.Add((type)(typeDCS.ReadObject(reader)));
            CodeMethodInvokeExpression arrayAssignStatement = new CodeMethodInvokeExpression(
                new CodeVariableReferenceExpression(tempListName),
                "Add",
                new CodeCastExpression(
                    new CodeTypeReference(memberField.Type.BaseType),
                    new CodeMethodInvokeExpression(
                        new CodeVariableReferenceExpression(memberField.Name + "DCS"),
                        "ReadObject",
                        new CodeExpression[] { new CodeVariableReferenceExpression("reader") }
                    )
                )
            );

            // Generated Code: ArrayList tempListName = new ArrayList();
            statements.Add(
                new CodeVariableDeclarationStatement(
                    "System.Collections.ArrayList",
                    tempListName,
                    new CodeObjectCreateExpression(
                        "System.Collections.ArrayList",
                        new CodeExpression[] { }
                    )
                )
            );

            // Generated Code: for (i = 0; i == i; ++i)
            CodeIterationStatement forLoop = new CodeIterationStatement(
                // initStatement parameter for pre-loop initialization.
                new CodeVariableDeclarationStatement(typeof(int), "i", new CodePrimitiveExpression(0)),
                // testExpression parameter to test for continuation condition.
                new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("i"),
                    CodeBinaryOperatorType.GreaterThan,
                    new CodeVariableReferenceExpression("-1")),
                // incrementStatement parameter indicates statement to execute after each iteration.
                new CodeAssignStatement(new CodeVariableReferenceExpression("i"),
                    new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("i"),
                        CodeBinaryOperatorType.Add, new CodePrimitiveExpression(1))),
                // statements parameter contains the statements to execute during each interation of the loop.
                    new CodeStatement[] {
                        isChildCondition,
                        new CodeExpressionStatement(arrayAssignStatement)
                    }
            );
            statements.Add(forLoop);

            return statements;
        }

        private CodeStatementCollection BuildMtomReadStringStatements(MemberType memberType, string classRefName, CodeMemberField memberField)
        {
            CodeStatementCollection statements = new CodeStatementCollection();
            CodeStatementCollection trueStatements = new CodeStatementCollection();
            CodeStatementCollection falseStatements = new CodeStatementCollection();

            // string contentID
            trueStatements.Add(
                new CodeVariableDeclarationStatement(
                    typeof(string),
                    "contentID"
                )
            );
            // contentID = reader.Value;
            trueStatements.Add(
                new CodeAssignStatement(
                    new CodeVariableReferenceExpression("contentID"),
                    new CodeFieldReferenceExpression(
                        new CodeVariableReferenceExpression("reader"),
                        "Value"
                    )
                )
            );
            // reader.MoveToElement();
            trueStatements.Add(
                new CodeMethodInvokeExpression(
                    new CodeVariableReferenceExpression("reader"),
                    "MoveToElement"
                )
            );
            // reader.ReadStartElement("Include", "http://www.w3.org/2004/08/xop/include");
            trueStatements.Add(
                new CodeMethodInvokeExpression(
                    new CodeVariableReferenceExpression("reader"),
                    "ReadStartElement",
                    new CodeExpression[] { 
                            new CodePrimitiveExpression("Include"),
                            new CodePrimitiveExpression("http://www.w3.org/2004/08/xop/include")
                        }
                )
            );
            // reader.ReadEndElement();
            trueStatements.Add(
                new CodeMethodInvokeExpression(
                    new CodeVariableReferenceExpression("reader"),
                    "ReadEndElement"
                )
            );
            // ObjectName.FieldName = GetBodyPartContent(contentID)
            trueStatements.Add(
                new CodeAssignStatement(
                    new CodeFieldReferenceExpression(
                        new CodeVariableReferenceExpression(classRefName),
                        memberField.Name
                    ),
                    new CodeMethodInvokeExpression(
                        null,
                        "GetBodyPartContent",
                        new CodeExpression[] {
                            new CodeVariableReferenceExpression("contentID"),
                            new CodeVariableReferenceExpression("BodyParts")
                        }
                    )
                )
            );

            falseStatements.Add(
                new CodeAssignStatement(
                    new CodeFieldReferenceExpression(
                        new CodeVariableReferenceExpression(classRefName),
                        memberField.Name
                    ),
                    new CodeMethodInvokeExpression(
                        new CodeTypeReferenceExpression("Convert"),
                        "FromBase64String",
                        new CodeExpression[] {
                            new CodeMethodInvokeExpression( 
                                new CodeVariableReferenceExpression("reader"), 
                                "ReadString")
                        }
                    )
                )
            );

            falseStatements.Add(
                new CodeMethodInvokeExpression(
                    new CodeVariableReferenceExpression("reader"),
                    "ReadEndElement"
                )
            );

            CodeStatement[] trues = new CodeStatement[trueStatements.Count];
            CodeStatement[] falses = new CodeStatement[falseStatements.Count];

            trueStatements.CopyTo(trues, 0);
            falseStatements.CopyTo(falses, 0);

            // if( IsAttribute(reader, "href") )
            statements.Add(
                new CodeConditionStatement( 
                    new CodeMethodInvokeExpression(
                        null,
                        "IsAttribute",
                        new CodeExpression[] {
                            new CodeVariableReferenceExpression("reader"),
                            new CodePrimitiveExpression("href")
                        }
                    ),
                    trues,
                    falses
                )
            );
            return statements;
        }

        private CodeStatement[] BuildReadStringArrayStatements(MemberType memberType, string classRefName, CodeMemberField memberField)
        {
            CodeStatementCollection statements = new CodeStatementCollection();
            string tempListName = memberField.Name + "_List";

            // Build: string[] tempArray = reader.ReadString().Split();
            CodeVariableDeclarationStatement declaration = new CodeVariableDeclarationStatement(typeof(string[]), tempListName);
            CodeExpression expression = new CodeMethodInvokeExpression(
                new CodeTypeReferenceExpression("reader"),
                "ReadString");
            expression = new CodeMethodInvokeExpression(
                expression,
                "Split");
            declaration.InitExpression = expression;
            statements.Add(declaration);

            if (memberField.Type.ArrayElementType.BaseType == "System.Byte")
            {
                statements.Add(
                    new CodeConditionStatement(
                        new CodeBinaryOperatorExpression(
                            new CodeFieldReferenceExpression(
                                new CodeThisReferenceExpression(),
                                "_CompressByteArrays"
                            ),

                            CodeBinaryOperatorType.BooleanOr,

                            new CodeBinaryOperatorExpression(
                                new CodeBinaryOperatorExpression(
                                    new CodeFieldReferenceExpression(
                                        new CodeVariableReferenceExpression(tempListName),
                                        "Length"),
                                    CodeBinaryOperatorType.ValueEquality,
                                    new CodePrimitiveExpression(1)
                                ),
                                CodeBinaryOperatorType.BooleanAnd,
                                new CodeBinaryOperatorExpression(
                                    new CodeFieldReferenceExpression(
                                        new CodeArrayIndexerExpression(
                                            new CodeVariableReferenceExpression(tempListName),
                                            new CodePrimitiveExpression(0)
                                        ),
                                        "Length"
                                    ),
                                    CodeBinaryOperatorType.GreaterThan,
                                    new CodePrimitiveExpression(2)
                                )
                            )
                        ),
                        BuildBase64ArrayStatement(tempListName, classRefName, memberField),
                        BuildReadStringArrayStatementsNormal(tempListName, classRefName, memberField)
                    )
                );

            }
            else
            {
                statements.AddRange(BuildReadStringArrayStatementsNormal(tempListName, classRefName, memberField));
            }

            // Build a CodeStatement array object to return
            CodeStatement[] codeStatements = new CodeStatement[statements.Count];
            for (int i = 0; i < statements.Count; ++i)
                codeStatements[i] = statements[i];
            return codeStatements;
        }

        private CodeStatement[] BuildBase64ArrayStatement(string tempListName, string classRefName, CodeMemberField memberField)
        {
            CodeStatementCollection statements = new CodeStatementCollection();

            // If called by the Native contract serializer method classRefName
            // will be null indicating this is a local field type
            CodeTypeReferenceExpression classRefType = null;
            if (classRefName != null || classRefName == "")
                classRefType = new CodeTypeReferenceExpression(classRefName);

            // Build left code assignment value
            CodeFieldReferenceExpression leftStatement = new CodeFieldReferenceExpression(
                classRefType,
                memberField.Name);

            statements.Add(
                new CodeAssignStatement(
                    leftStatement,
                    new CodeMethodInvokeExpression(
                        new CodeMethodReferenceExpression(
                            new CodeTypeReferenceExpression("Convert"),
                            "FromBase64String"
                            ),
                        new CodeArrayIndexerExpression(
                            new CodeVariableReferenceExpression(tempListName),
                            new CodePrimitiveExpression(0)
                        )
                    )
                )
            );


            
            // Build a CodeStatement array object to return
            CodeStatement[] codeStatements = new CodeStatement[statements.Count];
            for (int i = 0; i < statements.Count; ++i)
                codeStatements[i] = statements[i];
            return codeStatements;
        }


        private CodeStatement[] BuildReadStringArrayStatementsNormal(string tempListName, string classRefName, CodeMemberField memberField)
        {
            CodeStatementCollection statements = new CodeStatementCollection();

            // If classRefName is null initialize a local variable otherwise initialize the class member;
            if (classRefName == null)
            {
                // LocalVarName = new FieldType[tempListName.Length];
                // Note: The memberField.Name is simply used for clarity
                statements.Add(
                    new CodeAssignStatement(
                        new CodeVariableReferenceExpression(memberField.Name),
                        new CodeArrayCreateExpression(
                            memberField.Type.BaseType,
                            new CodeFieldReferenceExpression(
                                new CodeVariableReferenceExpression(tempListName), "Length"
                            )
                        )
                    )
                );
            }
            else
            {
                // Build: ClassRefName.FieldName = new FieldType[tempListName.Length];
                // Initializes the attribute field array
                statements.Add(
                    new CodeAssignStatement(
                        new CodeFieldReferenceExpression(
                            new CodeVariableReferenceExpression(classRefName),
                            memberField.Name
                        ),
                        new CodeArrayCreateExpression(
                            memberField.Type.BaseType,
                            new CodeFieldReferenceExpression(
                                new CodeVariableReferenceExpression(tempListName), "Length"
                            )
                        )
                    )
                );
            }

            // If called by the Native contract serializer method classRefName
            // will be null indicating this is a local field type
            CodeTypeReferenceExpression classRefType = null;
            if (classRefName != null || classRefName == "")
                classRefType = new CodeTypeReferenceExpression(classRefName);

            // Build For iterator: for (int i = 0; i < tempArray.Length; ++i)
            CodeIterationStatement forLoop = new CodeIterationStatement(
                // initStatement parameter for pre-loop initialization.
                new CodeVariableDeclarationStatement(typeof(int), "i", new CodePrimitiveExpression(0)),
                // testExpression parameter to test for continuation condition.
                new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("i"),
                    CodeBinaryOperatorType.LessThan,
                    new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(tempListName), "Length")),
                // incrementStatement parameter indicates statement to execute after each iteration.
                new CodeAssignStatement(new CodeVariableReferenceExpression("i"),
                    new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("i"),
                        CodeBinaryOperatorType.Add, new CodePrimitiveExpression(1))),
                // statements parameter contains the statements to execute during each interation of the loop.
                    new CodeStatement[] { BuildReadStringStatement(MemberType.Field, classRefName, memberField) }
            );

            // Add processing expression
            statements.Add(forLoop);

            // Build a CodeStatement array object to return
            CodeStatement[] codeStatements = new CodeStatement[statements.Count];
            for (int i = 0; i < statements.Count; ++i)
                codeStatements[i] = statements[i];
            return codeStatements;
        }

        private CodeStatement BuildReadStringStatement(MemberType memberType, string classRefName, CodeMemberField memberField)
        {
            string tempListName = memberField.Name + "_List";

            // If called by the Native contract serializer method classRefName
            // will be null indicating this is a local field type
            CodeTypeReferenceExpression classRefType = null;
            if (classRefName != null || classRefName == "")
                classRefType = new CodeTypeReferenceExpression(classRefName);

            // Build left code assignment value
            CodeFieldReferenceExpression leftStatement = new CodeFieldReferenceExpression(
                classRefType,
                memberField.Name + (memberField.Type.ArrayElementType == null ? "" : "[i]"));

            // Build right code assignment value
            CodeExpression readExpression;
            if (memberField.Type.ArrayElementType != null)
            {
                readExpression = new CodeArrayIndexerExpression(
                    new CodeVariableReferenceExpression(tempListName),
                    new CodeVariableReferenceExpression("i"));
            }
            else if (memberType == MemberType.Field)
                readExpression = new CodeMethodInvokeExpression(
                    new CodeTypeReferenceExpression("reader"),
                    "ReadString");
            else
                readExpression = new CodeFieldReferenceExpression(
                    new CodeTypeReferenceExpression("reader"),
                    "Value");

            CodeStatement assignStatement;

            // if the field is a string type just read it
            if (memberField.Type.BaseType == "System.String")
            {
                assignStatement = new CodeAssignStatement(leftStatement, readExpression);
            }
            // Else build convert statement
            else
            {
                CodeExpression convertExpression = new CodeMethodInvokeExpression(
                    new CodeTypeReferenceExpression("XmlConvert"),
                    CodeGenUtils.GetConvertMethod(memberField.Type.BaseType),
                    new CodeExpression[] { readExpression });

                assignStatement = new CodeAssignStatement(leftStatement, convertExpression);
            }
            return assignStatement;
        }

        private CodeMemberMethod BuildNativeWriteObjectMethod(string type, string typeName, CodeNamespace codeNs)
        {
            // Create WriteObject method
            CodeMemberMethod codeMemberMethod = new CodeMemberMethod();
            codeMemberMethod.Attributes = MemberAttributes.Public | MemberAttributes.Override;
            codeMemberMethod.ReturnType = new CodeTypeReference(typeof(void));
            codeMemberMethod.Name = "WriteObject";
            codeMemberMethod.Parameters.Add(new CodeParameterDeclarationExpression("XmlWriter", "writer"));
            codeMemberMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "graph"));

            // Add type declaration
            CodeVariableDeclarationStatement declaration = new CodeVariableDeclarationStatement(type, typeName);
            declaration.InitExpression = new CodeCastExpression(type, new CodeVariableReferenceExpression("graph"));
            codeMemberMethod.Statements.Add(declaration);

            // Create temporary member field used to generate write string code
            CodeMemberField memberField = new CodeMemberField(type, typeName);

            // Create conditional write statement
            CodeStatementCollection writeStatements = new CodeStatementCollection();
            if (memberField.Type.ArrayElementType != null && CodeGenUtils.IsNativeClrType(memberField.Type.ArrayElementType.BaseType))
            {
                writeStatements.AddRange(BuildWriteArrayStatements(null, memberField));
            }
            // Else if this is a native type
            else if (Type.GetType(memberField.Type.BaseType) != null)
            {
                writeStatements.Add(BuildWriteStringStatement(null, memberField));
            }
            else
                throw new ArgumentException("Invalid native field type for schema item: " + memberField.Name);

            // Add WriteEndElement statement
            writeStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("writer"),
                "WriteEndElement", new CodeExpression[] { }));

            // Convert statement collection to array
            CodeStatement[] writeStatementArray = new CodeStatement[writeStatements.Count];
            writeStatements.CopyTo(writeStatementArray, 0);

            // Build WriteParentElement condition paramters
            CodeVariableReferenceExpression writerParam = new CodeVariableReferenceExpression("writer");
            CodePrimitiveExpression nillableParam = new CodePrimitiveExpression(false);
            CodePrimitiveExpression requiredParam = new CodePrimitiveExpression(true);
            CodeVariableReferenceExpression objectParam = new CodeVariableReferenceExpression("graph");

            // Build WriteParentElement condition statement
            CodeConditionStatement condition = new CodeConditionStatement(new CodeMethodInvokeExpression(null,
                "WriteParentElement",
                new CodeExpression[] { writerParam, nillableParam, requiredParam, objectParam }),
                // True condition code. Build read statements based on contract member type
                writeStatementArray
            );

            // Add condition
            codeMemberMethod.Statements.Add(condition);

            return codeMemberMethod;
        }

        private CodeMemberMethod BuildWriteObjectMethod(string defaultNamespace, CodeTypeDeclaration codeType, CodeNamespace codeNs)
        {
            // Create method declaration
            CodeMemberMethod codeMemberMethod = new CodeMemberMethod();
            codeMemberMethod.Attributes = MemberAttributes.Public | MemberAttributes.Override;
            codeMemberMethod.ReturnType = new CodeTypeReference(typeof(void));
            codeMemberMethod.Name = "WriteObject";
            codeMemberMethod.Parameters.Add(new CodeParameterDeclarationExpression("XmlWriter", "writer"));
            codeMemberMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "graph"));

            // Add type declaration
            CodeVariableDeclarationStatement declaration = new CodeVariableDeclarationStatement(codeType.Name, codeType.Name + "Field");
            declaration.InitExpression = new CodeCastExpression(codeType.Name, new CodeVariableReferenceExpression("graph"));
            codeMemberMethod.Statements.Add(declaration);

            // Add WriteParentElementElement condition
            codeMemberMethod.Statements.Add(BuildWriteParentElementCondition(defaultNamespace, codeType, codeNs));

            // Add return statement
            codeMemberMethod.Statements.Add(new CodeMethodReturnStatement());

            return codeMemberMethod;
        }

        private CodeConditionStatement BuildWriteParentElementCondition(string defaultNamespace, CodeTypeDeclaration codeType, CodeNamespace codeNs)
        {
            // Check to see if the element is nillable
            string nillable = CodeGenUtils.GetCustomAttributeArgumentValue("IsNillable", codeType.CustomAttributes);
            nillable = nillable == null || nillable == "false" ? "false" : "true";

            // Check to see if the element is required (i.e. minOccurs > 0)
            string required = CodeGenUtils.GetCustomAttributeArgumentValue("IsRequired", codeType.CustomAttributes);
            bool isRequired = required == null || required == "true" ? true : false;

            // Build WriteStartObject if statement
            CodeVariableReferenceExpression writerParam = new CodeVariableReferenceExpression("writer");
            CodePrimitiveExpression nillableParam = new CodePrimitiveExpression(true);
            CodePrimitiveExpression requiredParam = new CodePrimitiveExpression(isRequired);
            CodeVariableReferenceExpression objectParam = new CodeVariableReferenceExpression("graph");

            CodeConditionStatement condition = new CodeConditionStatement(new CodeMethodInvokeExpression(null,
                "WriteParentElement",
                new CodeExpression[] { writerParam, nillableParam, requiredParam, objectParam }),
                // True condition code. Build read statements based on contract member type
                BuildWriteParentElementStatements(defaultNamespace, null, codeNs, codeType)
            );

            return condition;
        }

        private CodeStatementCollection BuildWriterEnumSwitchStatement(CodeTypeDeclaration codeType)
        {
            // Since CodeDom doesn't support switch this code will only work when
            // generating C#. This blows the reason for using CodeDom right now. Hopefully this will be addressed
            // in future versions of CodeDom.
            CodeStatementCollection statements = new CodeStatementCollection();
            statements.Add(new CodeSnippetStatement("\t\t\t\tswitch(" + codeType.Name + "Field)"));
            statements.Add(new CodeSnippetStatement("\t\t\t\t{"));
            foreach (CodeMemberField codeField in codeType.Members)
            {
                statements.Add(new CodeSnippetStatement("\t\t\t\t\tcase " + codeType.Name + "." + codeField.Name + ":"));
                if (codeField.UserData.Contains(typeof(XmlSchemaEnumerationFacet)))
                    statements.Add(new CodeSnippetExpression("\t\twriter.WriteString(\"" + ((XmlSchemaEnumerationFacet)codeField.UserData[typeof(XmlSchemaEnumerationFacet)]).Value + "\")"));
                else
                    statements.Add(new CodeSnippetExpression("\t\twriter.WriteString(\"" + codeField.Name + "\")"));
                statements.Add(new CodeSnippetExpression("\t\tbreak"));
            }
            statements.Add(new CodeSnippetStatement("\t\t\t\t\tdefault:"));
            statements.Add(new CodeSnippetExpression("\t\tthrow new XmlException()"));
            statements.Add(new CodeSnippetStatement("\t\t\t\t}"));

            return statements;
        }

        private CodeMethodInvokeExpression BuildWriteAttributeExpression(string defaultNamespace, string localTypeName, CodeMemberField codeField)
        {
            // Check to see if the element is required (i.e. minOccurs > 0)
            string required = CodeGenUtils.GetCustomAttributeArgumentValue("IsRequired", codeField.CustomAttributes);
            bool isRequired = required == null || required == "true" ? true : false;

            // Create WriteStartObject parameters
            CodeVariableReferenceExpression readerParam = new CodeVariableReferenceExpression("writer");
            CodeVariableReferenceExpression nameParam = new CodeVariableReferenceExpression("\"" + codeField.Name + "\"");
            CodePrimitiveExpression requiredParam = new CodePrimitiveExpression(isRequired);
            CodeSnippetExpression fieldParam = new CodeSnippetExpression(localTypeName + "." + codeField.Name);

            // Create WriteChildElement condition
            return new CodeMethodInvokeExpression(
                    null,
                    "WriteAttribute",
                    new CodeExpression[] { readerParam, nameParam, requiredParam, fieldParam });
        }

        private CodeConditionStatement BuildWriteChildElementCondition(string defaultNamespace, string localTypeName, CodeMemberField codeField)
        {
            // Check to see if the element is nillable
            string nillable = CodeGenUtils.GetCustomAttributeArgumentValue("IsNillable", codeField.CustomAttributes);
            bool isNillable = nillable == null || nillable == "false" ? false : true;

            // Check to see if the element is required (i.e. minOccurs > 0)
            string required = CodeGenUtils.GetCustomAttributeArgumentValue("IsRequired", codeField.CustomAttributes);
            bool isRequired = required == null || required == "true" ? true : false;

            // Create WriteStartObject parameters
            CodeVariableReferenceExpression readerParam = new CodeVariableReferenceExpression("writer");
            CodeVariableReferenceExpression nameParam = new CodeVariableReferenceExpression("\"" + codeField.Name + "\"");
            CodePrimitiveExpression nillableParam = new CodePrimitiveExpression(isNillable);
            CodePrimitiveExpression requiredParam = new CodePrimitiveExpression(isRequired);
            CodeSnippetExpression fieldTestParam = new CodeSnippetExpression(localTypeName + "." + codeField.Name);

            // Create WriteChildElement condition
            CodeConditionStatement condition = new CodeConditionStatement(
                new CodeMethodInvokeExpression(null,
                    "WriteChildElement",
                    new CodeExpression[] { readerParam, nameParam, nillableParam, requiredParam, fieldTestParam }),
                    BuildWriteChildElementStatements(localTypeName, codeField)
            );
            return condition;
        }

        private CodeStatementCollection BuildWriteParentElementStatement(string defaultNamespace, string fieldName, CodeNamespace codeNs, CodeTypeDeclaration codeType)
        {
            CodeStatementCollection statements = new CodeStatementCollection();

            // If the code type is an enum build reader.
            if (codeType.IsEnum)
            {
                statements.AddRange(BuildWriterEnumSwitchStatement(codeType));
            }
            else
            {
                // Sort member list.
                int[] sortOrder = CodeGenUtils.SortMemberTypes(codeType.Members);
                CodeMemberField codeField;
                
                // Build write statement for each member of the contract
                foreach (int index in sortOrder)
                {
                    codeField = (CodeMemberField)codeType.Members[index];
                 
                    // If this is an attribute build WriteAttribute element
                    if (CodeGenUtils.GetCustomAttributeArgumentValue("IsAttribute", codeField.CustomAttributes) == "True")
                    {
                        statements.Add(BuildWriteAttributeExpression(defaultNamespace, fieldName, codeField));
                    }
                    // Else if this is an anyAttibute element create special reader
                    else if (codeField.Type.BaseType != null && codeField.Name == "AnyAttr")
                    {
                        statements.Add(BuildWriteAnyAttributeStatement(fieldName, codeField));
                    }
                    // If this is a native schema type create WriteChildElement conditional statement
                    else if ((codeField.Type.ArrayElementType != null && CodeGenUtils.IsNativeClrType(codeField.Type.ArrayElementType.BaseType)) || Type.GetType(codeField.Type.BaseType) != null)
                    {
                        statements.Add(BuildWriteChildElementCondition(defaultNamespace, fieldName, codeField));
                    }
                    // If this is an any element create special reader
                    else if (codeField.Type.BaseType != null && codeField.Name == "Any")
                    {
                        statements.Add(BuildWriteAnyElementStatement(fieldName, codeField));
                    }
                    // Else Create new serializer and call it
                    else
                    {
                        // New up a DataContractSerialiser for this type
                        CodeVariableDeclarationStatement classDeclaration = new CodeVariableDeclarationStatement(codeField.Type.BaseType + "DataContractSerializer", codeField.Name + "DCS");

                        // Find the namespace of the member type which is required if there are nested namespaces in the data contract
                        string ns = CodeGenUtils.GetNamespaceFromType(m_codeNamespaces, codeField.Type.BaseType);

                        // use the default namespace if the field type was not found
                        if(string.IsNullOrEmpty(ns))
                        {
                            ns = defaultNamespace;
                        }

                        classDeclaration.InitExpression = new CodeObjectCreateExpression(
                            codeField.Type.BaseType + "DataContractSerializer",
                            new CodeExpression[] { new CodePrimitiveExpression(codeField.Name), new CodePrimitiveExpression(defaultNamespace), new CodePrimitiveExpression(ns) });
                        statements.Add(classDeclaration);

                        // If this is not an array process the element
                        if (codeField.Type.ArrayElementType == null)
                        {
                            CodeAssignStatement bodyParts = new CodeAssignStatement(
                                new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(codeField.Name + "DCS"), "BodyParts"),
                                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "BodyParts")
                                );

                            statements.Add(bodyParts);
                            
                            // Build codeField[DCS].WriteObject(reader, [ClassName.FieldName); expression
                            CodeFieldReferenceExpression fieldRef = new CodeFieldReferenceExpression(
                                new CodeTypeReferenceExpression(fieldName),
                                codeField.Name);

                            CodeExpression writeExpression = new CodeMethodInvokeExpression(
                                new CodeTypeReferenceExpression(codeField.Name + "DCS"),
                                "WriteObject",
                                new CodeExpression[] { new CodeTypeReferenceExpression("writer"), fieldRef });

                            // Add processing expression
                            statements.Add(writeExpression);
                        }
                        // Else process an array of elements
                        else
                        {
                            statements.AddRange(BuildElementArrayWriteStatements(fieldName, codeField, defaultNamespace, codeNs));
                        }
                    }
                }
            }
            return statements;
        }

        private CodeStatementCollection BuildElementArrayWriteStatements(string fieldName, CodeMemberField codeField, string defaultNamespace, CodeNamespace codeNs)
        {
            CodeStatementCollection statements = new CodeStatementCollection();

            // Generated Code: typeDCS.WriteObject(writer, localField.Partition[i]);
            CodeMethodInvokeExpression writeExpression = new CodeMethodInvokeExpression(
                new CodeVariableReferenceExpression(codeField.Name + "DCS"),
                "WriteObject",
                new CodeExpression[] {
                    new CodeVariableReferenceExpression("writer"),
                    new CodeFieldReferenceExpression(
                        new CodeVariableReferenceExpression(fieldName),
                        codeField.Name + "[i]"
                    )
                }
            );

            // Generated Code: for (i = 0; i < localField.Length(); ++i)
            CodeIterationStatement forLoop = new CodeIterationStatement(
                // initStatement parameter for pre-loop initialization.
                new CodeVariableDeclarationStatement(typeof(int), "i", new CodePrimitiveExpression(0)),
                // testExpression parameter to test for continuation condition.
                new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("i"),
                    CodeBinaryOperatorType.LessThan,
                    new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(fieldName), codeField.Name + ".Length")),
                // incrementStatement parameter indicates statement to execute after each iteration.
                new CodeAssignStatement(new CodeVariableReferenceExpression("i"),
                    new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("i"),
                        CodeBinaryOperatorType.Add, new CodePrimitiveExpression(1))),
                // statements parameter contains the statements to execute during each interation of the loop.
                    new CodeStatement[] { new CodeExpressionStatement(writeExpression) }
            );
            statements.Add(forLoop);

            return statements;
        }

        private CodeStatement[] BuildWriteParentElementStatements(string defaultNamespace, string fieldName, CodeNamespace codeNs, CodeTypeDeclaration codeType)
        {
            CodeStatementCollection statements = new CodeStatementCollection();
            bool writeWriteEnd = fieldName == null ? true : false;

            // New up the local DataContract object
            fieldName = (fieldName == null) ? codeType.Name + "Field" : fieldName;

            // Build member write statements based on member type
            statements.AddRange(BuildWriteParentElementStatement(defaultNamespace, fieldName, codeNs, codeType));

            // If this is not a recursive call
            if (writeWriteEnd)
            {
                // Build WriteEndElement statement
                statements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("writer"),
                    "WriteEndElement", new CodeExpression[] { }));
            }

            // Build a CodeStatement array object to return
            CodeStatement[] codeStatements = new CodeStatement[statements.Count];
            for (int i = 0; i < statements.Count; ++i)
                codeStatements[i] = statements[i];
            return codeStatements;

        }

        private CodeStatement[] BuildWriteChildElementStatements(string classRefName, CodeMemberField memberField)
        {
            CodeTypeReferenceExpression readerRef = new CodeTypeReferenceExpression("writer");
            CodeStatementCollection statements = new CodeStatementCollection();

            // If the messages uses Mtom encoding and this is a byte array special mtom processing applies
            if (m_encodingType == MessageEncodingType.Mtom && memberField.Type.ArrayElementType != null && memberField.Type.ArrayElementType.BaseType == "System.Byte")
            {
                // Increment the cid identifier
                ++m_cid;

                // BodyParts.Add(CreateNewBodyPart(ClassName.FieldName, "<cid@body>"));
                statements.Add(
                    new CodeMethodInvokeExpression(
                        new CodeVariableReferenceExpression("BodyParts"),
                        "Add",
                        new CodeExpression[] {
                            new CodeMethodInvokeExpression(
                                null,
                                "CreateNewBodyPart",
                                new CodeExpression[] {
                                    new CodeFieldReferenceExpression(
                                        new CodeVariableReferenceExpression(classRefName),
                                        memberField.Name
                                    ),
                                    new CodePrimitiveExpression("<" + m_cid.ToString() + "@body>")
                                }
                            )
                        }
                    )
                );
                // writer.WriteStartElement("xop", "Include", "http://www.w3.org/2004/08/xop/include");
                statements.Add(
                    new CodeMethodInvokeExpression(
                        new CodeVariableReferenceExpression("writer"),
                        "WriteStartElement",
                        new CodeExpression[] {
                            new CodePrimitiveExpression("xop"),
                            new CodePrimitiveExpression("Include"),
                            new CodePrimitiveExpression("http://www.w3.org/2004/08/xop/include")
                        }
                    )
                );
                // writer.WriteAttributeString(null, "href", null, "0@body");
                statements.Add(
                    new CodeMethodInvokeExpression(
                        new CodeVariableReferenceExpression("writer"),
                        "WriteAttributeString",
                        new CodeExpression[] {
                            new CodePrimitiveExpression(null),
                            new CodePrimitiveExpression("href"),
                            new CodePrimitiveExpression(null),
                            new CodePrimitiveExpression("cid:" + m_cid.ToString() + "@body")
                        }
                    )
                );
                // writer.WriteEndElement();
                statements.Add(
                    new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("writer"), "WriteEndElement")
                );
            }
            // If this is an any element create special reader
            else if (memberField.Type.BaseType == "XmlElement" && memberField.Name == "Any")
            {
                statements.Add(BuildWriteAnyElementStatement(classRefName, memberField));
            }
            // Else if this is a native array
            else if (memberField.Type.ArrayElementType != null && CodeGenUtils.IsNativeClrType(memberField.Type.ArrayElementType.BaseType))
            {
                statements.AddRange(BuildWriteArrayStatements(classRefName, memberField));
            }
            else
                statements.Add(BuildWriteStringStatement(classRefName, memberField));
            
            // If this is not an attribute add WriteEndElement statement
            if (CodeGenUtils.GetCustomAttributeArgumentValue("IsAttribute", memberField.CustomAttributes) == null)
            {
                CodeMethodInvokeExpression writeEndElementExp = new CodeMethodInvokeExpression(
                    readerRef,
                    "WriteEndElement",
                    new CodeExpression[] { });
                statements.Add(writeEndElementExp);
            }

            // Build a CodeStatement array object to return
            CodeStatement[] codeStatements = new CodeStatement[statements.Count];
            for (int i = 0; i < statements.Count; ++i)
                codeStatements[i] = statements[i];
            return codeStatements;
        }

        private CodeStatement BuildWriteStringStatement(string classRefName, CodeMemberField memberField)
        {
            // Build writer.WriteString([ClassName].[FieldName]);
            CodeExpression writeExpression;

            // Special processing for any and anyAttr tags
            if (memberField.Name == "Any")
            {
                writeExpression = BuildWriteAnyElementStatement(classRefName, memberField);
            }
            else if (memberField.Name == "AnyAttr")
            {
                writeExpression = BuildWriteAnyAttributeStatement(classRefName, memberField);
            }
            else
            {
                // If called by the Native contract serializer method classRefName
                // will be null indicating this is a local field type
                CodeTypeReferenceExpression classRefType = null;
                if (classRefName != null || classRefName == "")
                    classRefType = new CodeTypeReferenceExpression(classRefName);

                // Build WriteString expressions
                CodeFieldReferenceExpression fieldRef = new CodeFieldReferenceExpression(
                    classRefType,
                    memberField.Name);

                // if this is a string write the field otherwise convert it
                CodeExpression valueExpression;
                if (memberField.Type.BaseType == "System.String" && memberField.Type.ArrayElementType == null)
                {
                    valueExpression = fieldRef;
                }
                // Else Build Convert statement: XmlConvert.ToString([classRefName].[memberField.Name][i])
                else
                {
                    valueExpression = new CodeMethodInvokeExpression(
                        new CodeTypeReferenceExpression("XmlConvert"),
                        "ToString",
                        new CodeExpression[] { fieldRef });
                }

                // Get local namespace if it exists
                string nameSpace = CodeGenUtils.GetCustomAttributeArgumentValue("Namespace", memberField.CustomAttributes);

                // If this is an attribute field set method call to WriteAttributeString else WriteString
                if (CodeGenUtils.GetCustomAttributeArgumentValue("IsAttribute", memberField.CustomAttributes) == "True")
                {
                    CodeExpression nameSpaceExp;
                    if (nameSpace == null)
                        nameSpaceExp = new CodePrimitiveExpression(null);
                    else
                        nameSpaceExp = new CodeVariableReferenceExpression(nameSpace);

                    // Add writer.WriteAttributeString(prefix, localName, namespace, tempList);
                    writeExpression = (new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("writer"),
                        "WriteAttributeString",
                        new CodeExpression[] {
                            new CodePrimitiveExpression(null),
                            new CodeVariableReferenceExpression("\"" + memberField.Name + "\""),
                            nameSpaceExp,
                            valueExpression }));
                }
                else
                {
                    // Add writer.WriteString(tempList);
                    writeExpression = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("writer"),
                        "WriteString",
                        new CodeExpression[] { valueExpression });
                }
            }
            return new CodeExpressionStatement(writeExpression);
        }

        private CodeStatement[] BuildWriteArrayStatements(string classRefName, CodeMemberField memberField)
        {
            CodeStatementCollection statements = new CodeStatementCollection();
            string tempListName = memberField.Name + "_List";

            // Add create tempList statement: string tempListName;
            CodeVariableDeclarationStatement declaration = new CodeVariableDeclarationStatement(typeof(string), tempListName);
            declaration.InitExpression = new CodePrimitiveExpression("");
            statements.Add(declaration);

            if (memberField.Type.ArrayElementType.BaseType == "System.Byte")
            {
                statements.Add(
                    new CodeConditionStatement(
                        new CodeFieldReferenceExpression(
                            new CodeThisReferenceExpression(),
                            "_CompressByteArrays"
                        ),
                        BuildWriteArrayStatementsBase64(tempListName, classRefName, memberField),
                        BuildWriteArrayStatementsNormal(tempListName, classRefName, memberField)
                    )
                );
            }
            else
            {
                statements.AddRange(BuildWriteArrayStatementsNormal(tempListName, classRefName, memberField));
            }

            // Build a CodeStatement array object to return
            CodeStatement[] codeStatements = new CodeStatement[statements.Count];
            for (int i = 0; i < statements.Count; ++i)
                codeStatements[i] = statements[i];
            return codeStatements;
        }

        private CodeStatement[] BuildWriteArrayStatementsBase64(string tempListName, string classRefName, CodeMemberField memberField)
        {
            CodeStatementCollection statements = new CodeStatementCollection();

            // If called by the Native contract serializer method classRefName
            // will be null indicating this is a local field type
            CodeTypeReferenceExpression classRefType = null;
            if (classRefName != null || classRefName == "")
                classRefType = new CodeTypeReferenceExpression(classRefName);

            statements.Add(
                new CodeConditionStatement(
                    new CodeBinaryOperatorExpression(
                        new CodeFieldReferenceExpression(classRefType, memberField.Name),
                        CodeBinaryOperatorType.IdentityInequality,
                        new CodePrimitiveExpression(null)
                    ),
                    new CodeAssignStatement(
                        new CodeVariableReferenceExpression(tempListName),
                        new CodeMethodInvokeExpression(
                            new CodeTypeReferenceExpression("Convert"),
                            "ToBase64String",
                            new CodeFieldReferenceExpression(classRefType, memberField.Name)
                        )
                    )
                )
            );

            // Add writer.WriteString(tempListName);
            statements.Add(
                new CodeMethodInvokeExpression(
                    new CodeTypeReferenceExpression("writer"),
                    "WriteString",
                    new CodeVariableReferenceExpression(tempListName) 
                )
            );


            // Build a CodeStatement array object to return
            CodeStatement[] codeStatements = new CodeStatement[statements.Count];
            for (int i = 0; i < statements.Count; ++i)
                codeStatements[i] = statements[i];
            return codeStatements;
        }

        private CodeStatement[] BuildWriteArrayStatementsNormal(string tempListName, string classRefName, CodeMemberField memberField)
        {
            CodeStatementCollection statements = new CodeStatementCollection();

            // If called by the Native contract serializer method classRefName
            // will be null indicating this is a local field type
            CodeTypeReferenceExpression classRefType = null;
            if (classRefName != null || classRefName == "")
                classRefType = new CodeTypeReferenceExpression(classRefName);

            // Build field assignment variable: [classRefName].[memberField.Name][i];
            CodeFieldReferenceExpression fieldRef = new CodeFieldReferenceExpression(
                classRefType,
                memberField.Name + "[i]");

            // If the base type is not a string convert it
            CodeExpression convertExpression;
            if (memberField.Type.BaseType == "System.String")
            {
                convertExpression = fieldRef;
            }
            // Else Build Convert statement: XmlConvert.ToString([classRefName].[memberField.Name][i])
            else
            {
                convertExpression = new CodeMethodInvokeExpression(
                    new CodeTypeReferenceExpression("XmlConvert"),
                    "ToString",
                    new CodeExpression[] { fieldRef });
            }

            // Build tempList assignment statement:
            // tempList = tempList + Convert.ToString([classRefName].[memberField.Name][i]);
            CodeAssignStatement assignStatement = new CodeAssignStatement(
                new CodeVariableReferenceExpression(tempListName),
                new CodeBinaryOperatorExpression(
                    new CodeVariableReferenceExpression(tempListName),
                    CodeBinaryOperatorType.Add,
                    convertExpression
                )
            );

            // Build spacing statement
            // if (i < ClassName.FieldName.Length - 1) tempListName = tempListName + " ";
            CodeVariableReferenceExpression indexVar = new CodeVariableReferenceExpression("i");
            CodeVariableReferenceExpression tempListVar = new CodeVariableReferenceExpression(tempListName);
            CodeBinaryOperatorExpression testValue = new CodeBinaryOperatorExpression(tempListVar, CodeBinaryOperatorType.Subtract, indexVar);
            CodeConditionStatement spacingStatement = new CodeConditionStatement(
                new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("i"),
                    CodeBinaryOperatorType.LessThan,
                    new CodeBinaryOperatorExpression(
                        new CodeFieldReferenceExpression(    
                            new CodeFieldReferenceExpression(classRefType, memberField.Name),
                            "Length"
                        ),
                        CodeBinaryOperatorType.Subtract,
                        new CodePrimitiveExpression(1)
                    )
                ),
                new CodeStatement[] { 
                    new CodeAssignStatement(
                        tempListVar,
                        new CodeBinaryOperatorExpression(
                            tempListVar,
                            CodeBinaryOperatorType.Add,
                            new CodePrimitiveExpression(" ")
                        )
                    )
                }
             );

            // Build For iterator: for (int i = 0; i < tempArray.Length; ++i)
            CodeIterationStatement forLoop = new CodeIterationStatement(
                // initStatement parameter for pre-loop initialization.
                new CodeVariableDeclarationStatement(typeof(int), "i", new CodePrimitiveExpression(0)),
                // testExpression parameter to test for continuation condition.
                new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("i"),
                    CodeBinaryOperatorType.LessThan,
                    new CodeFieldReferenceExpression(new CodeFieldReferenceExpression(classRefType, memberField.Name),
                        "Length")),
                // incrementStatement parameter indicates statement to execute after each iteration.
                new CodeAssignStatement(new CodeVariableReferenceExpression("i"),
                    new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("i"),
                        CodeBinaryOperatorType.Add, new CodePrimitiveExpression(1))),
                    new CodeStatement[] { assignStatement, spacingStatement }
            );

            // Add processing expression
            statements.Add(forLoop);

            // Get local namespace if it exists
            string nameSpace = CodeGenUtils.GetCustomAttributeArgumentValue("Namespace", memberField.CustomAttributes);

            // If this is an attribute field set method call to WriteAttributeString else WriteString
            if (CodeGenUtils.GetCustomAttributeArgumentValue("IsAttribute", memberField.CustomAttributes) == "True")
            {
                CodeExpression nameSpaceExp;
                if (nameSpace == null)
                    nameSpaceExp = new CodePrimitiveExpression(null);
                else
                    nameSpaceExp = new CodeVariableReferenceExpression(nameSpace);

                // Add writer.WriteAttributeString(prefix, localName, namespace, tempList);
                statements.Add(new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("writer"),
                    "WriteAttributeString",
                    new CodeExpression[] {
                        new CodePrimitiveExpression(null),
                        new CodeVariableReferenceExpression("\"" + memberField.Name + "\""),
                        nameSpaceExp,
                        new CodeVariableReferenceExpression(tempListName) }));
            }
            else
            {
                // Add writer.WriteString(tempListName);
                statements.Add(new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("writer"),
                    "WriteString",
                    new CodeExpression[] { new CodeVariableReferenceExpression(tempListName) }));
            }

            // Build a CodeStatement array object to return
            CodeStatement[] codeStatements = new CodeStatement[statements.Count];
            for (int i = 0; i < statements.Count; ++i)
                codeStatements[i] = statements[i];
            return codeStatements;
        }

        private CodeExpression BuildWriteAnyElementStatement(string classRefName, CodeMemberField memberField)
        {
            // Get IsRequired value
            bool isRequired = false;
            string isRequiredValue;
            if ((isRequiredValue = CodeGenUtils.GetCustomAttributeArgumentValue("IsRequired", memberField.CustomAttributes)) != null)
            {
                isRequired = isRequiredValue.ToLower() == "false" ? false : true; 
            }
            
            // Build WriteString expressions
            CodeFieldReferenceExpression fieldRef = new CodeFieldReferenceExpression(
                new CodeTypeReferenceExpression(classRefName),
                "Any");

            // Build WriteAnyElement(writer, [ClassName].[FieldName], IsRequired);
            CodeExpression writeExpression = new CodeMethodInvokeExpression(
                null,
                "WriteAnyElement",
                new CodeExpression[] {
                    new CodeTypeReferenceExpression("writer"),
                    fieldRef,
                    new CodePrimitiveExpression(isRequired)
                }
            );

            return writeExpression;
        }

        private CodeExpression BuildWriteAnyAttributeStatement(string classRefName, CodeMemberField memberField)
        {
            // Build WriteString expressions
            CodeFieldReferenceExpression fieldRef = new CodeFieldReferenceExpression(
                new CodeTypeReferenceExpression(classRefName),
                "AnyAttr");

            // Build WriteAnyAttribute(writer, [ClassName].[FieldName]);
            CodeExpression writeExpression = new CodeMethodInvokeExpression(null,
                "WriteAnyAttribute",
                new CodeExpression[] { new CodeTypeReferenceExpression("writer"), fieldRef });

            return writeExpression;
        }

        private CodeAssignStatement CodeAssignNewObjectToVariableStatement(string varName, string typeName)
        {
            CodeObjectCreateExpression newStatement = new CodeObjectCreateExpression(typeName);
            return new CodeAssignStatement(new CodeTypeReferenceExpression(varName), newStatement);
        }

        private void ExpandCodeType(ref CodeTypeDeclaration codeTypeRef, CodeTypeDeclaration codeType)
        {
            // If there's nothing to expand or the base type is a primitive type return original code type.
            if (codeType.BaseTypes == null)
                return;

            // Create a new expanded code type. For each base type add attributes and properties from the base type
            foreach (CodeTypeReference baseTypeCodeRef in codeType.BaseTypes)
            {
                // If the base type is a primitive type skip it
                if (CodeGenUtils.IsNativeClrType(baseTypeCodeRef.BaseType))
                    continue;

                // Determine the baseType name and namespace
                int typeNameIndex = baseTypeCodeRef.BaseType.LastIndexOf('.');
                typeNameIndex = typeNameIndex == -1 ? 0 : typeNameIndex;
                string typeNamespace = baseTypeCodeRef.BaseType.Substring(0, typeNameIndex++);
                string typeName = baseTypeCodeRef.BaseType.Substring(typeNameIndex, baseTypeCodeRef.BaseType.Length - typeNameIndex);

                // Get the base types namespace
                CodeNamespace baseCodeNs = CodeGenUtils.GetDotNetNamespace(m_codeNamespaces, typeNamespace);

                // Add a property to the new type for each property in the base type
                foreach (CodeTypeDeclaration baseCodeType in baseCodeNs.Types)
                {
                    if (baseCodeType.Name == typeName)
                    {
                        // If the base is derived recurse to process it first
                        ExpandCodeType(ref codeTypeRef, baseCodeType);

                        // Add base type members to the new type
                        foreach (CodeTypeMember baseTypeMember in baseCodeType.Members)
                            codeTypeRef.Members.Add(baseTypeMember);
                        break;
                    }
                }
            }
            return;
        }
    }
}
