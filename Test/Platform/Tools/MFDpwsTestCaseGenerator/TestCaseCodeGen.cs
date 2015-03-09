using System;
using System.Collections.Generic;
using System.Reflection;
using System.CodeDom;

namespace MFDpwsTestCaseGenerator
{
    public class FixtureNamespace
    {
        #region FixtureNamespace Members


        CodeNamespace m_Namespace;
        Dictionary<string, InterfaceImplementation> m_Interfaces = new Dictionary<string, InterfaceImplementation>();
        TypeFactoryCollection m_Collection = new TypeFactoryCollection();
        Assembly m_Assembly;
        Assembly[] m_Referenced;

        public FixtureNamespace(Assembly assembly, Assembly[] referencedAssemblies)
        {
            m_Assembly = assembly;
            m_Referenced = referencedAssemblies;

            foreach (Type type in GetInterfaces(assembly))
            {
                if (m_Namespace == null && IsServiceInterfaceType(type))
                {
                    m_Namespace = new CodeNamespace(
                        type.Namespace);
                    m_Namespace.Imports.Add(new CodeNamespaceImport("System"));
                    m_Namespace.Imports.Add(new CodeNamespaceImport("Dpws.Device"));
                    m_Namespace.Imports.Add(new CodeNamespaceImport("MFDpwsTestFixtureUtilities"));
                    m_Namespace.Imports.Add(new CodeNamespaceImport("Microsoft.SPOT.Platform.Test"));
                    m_Namespace.Imports.Add(new CodeNamespaceImport("Ws.Services.Binding"));
                    m_Namespace.Imports.Add(new CodeNamespaceImport("Ws.Services"));
                    m_Interfaces.Add(type.FullName, new ServiceImplementation(type, this));
                }
                else
                {
                    m_Interfaces.Add(type.FullName, new InterfaceImplementation(type, this));
                }
            }
        }

        private static IEnumerable<Type> GetInterfaces(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsInterface)
                {
                    yield return type;
                }
            }
        }

        private bool IsServiceInterfaceType(Type type)
        {
            foreach (CustomAttributeData attrData in CustomAttributeData.GetCustomAttributes(type))
            {
                if (attrData.ToString().Contains("Ws.ServiceModel.ServiceContractAttribute"))
                {
                    return true;
                }
            }

            return false;
        }

        public CodeNamespace Namespace
        {
            get
            {
                foreach (KeyValuePair<string, InterfaceImplementation> iface in m_Interfaces)
                {
                    m_Namespace.Types.Add(iface.Value.CodeTypeDeclaration);
                }

                m_Collection.AddToNamespace(m_Namespace);

                return m_Namespace;
            }
        }

        public Type GetType(string typeName)
        {
            if (!typeName.StartsWith(m_Namespace.Name))
            {
                typeName = m_Namespace.Name + "." + typeName;
            }
            return m_Assembly.GetType(typeName);
        }

        public Type GetReferencedType(string typeName)
        {
            foreach (Assembly asm in m_Referenced)
            {
                Type t = asm.GetType(typeName);

                if (t != null) return t;
            }

            return null;
        }

        public TypeFactoryCollection TypeFactoryCollection
        {
            get { return m_Collection; }
        }

        private InterfaceImplementation this[Type t]
        {
            get
            {
                return m_Interfaces[t.FullName];
            }
        }

        private InterfaceImplementation this[string s]
        {
            get
            {
                return m_Interfaces[s];
            }
        }

        public int TestIterations { get; set; }

        #endregion

        #region Nested Classes

        class InterfaceImplementation
        {
            protected List<InterfaceMethodImplementation> m_Methods = new List<InterfaceMethodImplementation>();
            protected CodeTypeDeclaration m_TypeDecl = null;
            protected string m_TypeName;
            protected string m_TypePrefix;
            protected Type m_InterfaceType;
            protected FixtureNamespace m_FixtureNamespace;
            private bool m_MembersAdded = false;

            internal InterfaceImplementation(Type type, FixtureNamespace ns)
            {
                m_InterfaceType = type;
                m_FixtureNamespace = ns;
                m_TypePrefix = type.Name;

                if (m_TypePrefix.StartsWith("I"))
                {
                    m_TypePrefix = m_TypePrefix.Remove(0, 1);
                }

                m_TypeName = m_TypePrefix + "Implementation";

                m_TypeDecl = new CodeTypeDeclaration(m_TypeName);
                m_TypeDecl.Attributes = MemberAttributes.Public;
                m_TypeDecl.IsPartial = true;
                m_TypeDecl.BaseTypes.Add(new CodeTypeReference(m_InterfaceType));

                foreach (MethodInfo mi in m_InterfaceType.GetMethods())
                {
                    InterfaceMethodImplementation mim = new InterfaceMethodImplementation(mi, this, m_FixtureNamespace);
                    m_Methods.Add(mim);
                }
            }

            public string TypeName
            {
                get
                {
                    return m_TypeName;
                }
            }

            public virtual CodeTypeDeclaration CodeTypeDeclaration
            {
                get
                {
                    if (!m_MembersAdded)
                    {
                        foreach (InterfaceMethodImplementation mi in m_Methods)
                        {
                            m_TypeDecl.Members.Add(mi.CodeMemberMethod);
                        }

                        m_MembersAdded = true;
                    }
                    return m_TypeDecl;
                }
            }

            public virtual CodeObjectCreateExpression CreateExpression
            {
                get
                {
                    return new CodeObjectCreateExpression(
                        m_TypeName, new CodeExpression[] { });
                }
            }
        }

        class InterfaceMethodImplementation
        {
            private FixtureNamespace m_Namespace;
            private MethodInfo m_MethodInfo;
            private InterfaceImplementation m_TypeImpl;
            private CodeMemberMethod m_cmMethod;
            private CodeMemberMethod m_cmExecute;
            private string m_Action;
            private bool m_MethodComplete = false;

            private List<CodeExpression> m_Parameters = new List<CodeExpression>();

            internal InterfaceMethodImplementation(MethodInfo mi, InterfaceImplementation typeImpl, FixtureNamespace ns)
            {
                m_Namespace = ns;

                m_MethodInfo = mi;

                m_TypeImpl = typeImpl;

                m_cmMethod = new CodeMemberMethod();

                m_cmMethod.Name = mi.Name;

                m_cmMethod.Attributes = MemberAttributes.Public;

                m_cmMethod.ReturnType = new CodeTypeReference(mi.ReturnType);

                foreach (ParameterInfo pi in mi.GetParameters())
                {
                    #region Parameter Creation Code
                    CodeMethodInvokeExpression invoke;
                    TypeFactoryGenerator gen = ns.TypeFactoryCollection[pi.ParameterType];

                    if (pi.ParameterType.IsArray)
                    {
                        invoke = gen.ArrayInvocation;
                    }
                    else
                    {
                        invoke = gen.Invocation;
                    }

                    m_Parameters.Add(invoke);
                    #endregion
                }

                foreach (CustomAttributeData data in CustomAttributeData.GetCustomAttributes(mi))
                {
                    foreach (CustomAttributeNamedArgument namedArg in data.NamedArguments)
                    {
                        if (namedArg.MemberInfo.Name == "Action")
                        {
                            Uri uri = new Uri(namedArg.TypedValue.Value.ToString());

                            string[] pathparts = uri.LocalPath.Split(new char[] { '/' });

                            m_Action = pathparts[pathparts.Length - 1];
                        }
                    }
                }
            }

            public CodeMemberMethod CodeMemberMethod
            {
                get
                {
                    if (m_MethodComplete) return m_cmMethod;

                    m_cmMethod.Statements.Add(TestLog.LogComment(m_cmMethod.Name));

                    var excepHandler = new CodeTryCatchFinallyStatement();
                    m_cmMethod.Statements.Add(excepHandler);

                    var serviceImpl = m_TypeImpl as ServiceImplementation;

                    int parameterIndex = 0;

                    foreach (ParameterInfo pi in m_MethodInfo.GetParameters())
                    {
                        // Add parameter to method signature
                        m_cmMethod.Parameters.Add(
                            new CodeParameterDeclarationExpression(pi.ParameterType, pi.Name));

                        if (serviceImpl != null)
                        {
                            // Pop the parameter off of the stack
                            var pop = new CodeMethodInvokeExpression(
                                serviceImpl.Stack.FieldReferenceExpression, "Pop", new CodeExpression[] { });

                            var cast = new CodeCastExpression(pi.ParameterType, pop);

                            string varName = "param" + parameterIndex++;
                            excepHandler.TryStatements.Add(
                                new CodeVariableDeclarationStatement(
                                    pi.ParameterType, varName, cast));

                            excepHandler.TryStatements.Add(
                                TestLog.LogComment("Testing Parameter " + varName));

                            var testCall = new CodeMethodInvokeExpression(
                                new CodeTypeReferenceExpression("ObjectTest"),
                                "TestEqualEx",
                                new CodeExpression[] { new CodeVariableReferenceExpression(varName), new CodeVariableReferenceExpression(pi.Name) });

                            excepHandler.TryStatements.Add(testCall);
                        }
                    }

                    // Need to push a result onto
                    // the stack.
                    if (serviceImpl != null)
                    {
                        excepHandler.TryStatements.Add(TestLog.LogComment("Server side test passed"));
                        excepHandler.TryStatements.Add(new CodeMethodInvokeExpression(
                                serviceImpl.Stack.FieldReferenceExpression,
                                "Push",
                                new CodeExpression[] 
                                { 
                                    new CodeFieldReferenceExpression(
                                        new CodeTypeReferenceExpression("MFTestResults"),
                                        "Pass") 
                                }));
                    }

                    // Catch block
                    // Catch all Exceptions and print out a log.
                    var catchClause = new CodeCatchClause("e", new CodeTypeReference(typeof(Exception)));
                    catchClause.Statements.Add(TestLog.LogException(new CodeVariableReferenceExpression("e")));

                    if (serviceImpl != null)
                    {
                        catchClause.Statements.Add(
                            TestLog.LogComment("Server side test failed"));
                        catchClause.Statements.Add(new CodeMethodInvokeExpression(
                                    serviceImpl.Stack.FieldReferenceExpression,
                                    "Push",
                                    new CodeExpression[] 
                                { 
                                    new CodeFieldReferenceExpression(
                                        new CodeTypeReferenceExpression("MFTestResults"),
                                        "Fail") 
                                }));
                    }
                    excepHandler.CatchClauses.Add(
                        catchClause);


                    if (m_MethodInfo.ReturnType != typeof(void))
                    {
                        string varName = "retval";
                        TypeFactoryGenerator gen = m_Namespace.TypeFactoryCollection[m_MethodInfo.ReturnType];

                        m_cmMethod.Statements.Add(
                            new CodeVariableDeclarationStatement(
                                m_MethodInfo.ReturnType, varName,
                                m_MethodInfo.ReturnType.IsArray ? gen.ArrayInvocation : gen.Invocation));

                        var varRef = new CodeVariableReferenceExpression(
                            varName);

                        if (serviceImpl != null)
                        {
                            // Push the parameter onto the stack
                            var push = new CodeMethodInvokeExpression(
                                serviceImpl.Stack.FieldReferenceExpression, "Push", new CodeExpression[] { varRef });

                            m_cmMethod.Statements.Add(push);
                        }

                        CodeMethodReturnStatement rs = new CodeMethodReturnStatement(
                            varRef);

                        m_cmMethod.Statements.Add(rs);
                    }
                    else if (serviceImpl != null)
                    {
                        // Since the method does not have a return value
                        // Set the Manual Reset event to signal the client 
                        // that the test is done.
                        excepHandler.FinallyStatements.Add(new CodeMethodInvokeExpression(
                                serviceImpl.Stack.FieldReferenceExpression,
                                "Set",
                                new CodeExpression[] { }));
                    }

                    m_MethodComplete = true;

                    return m_cmMethod;
                }
            }

            public CodeExpression[] Parameters
            {
                get
                {
                    return m_Parameters.ToArray();
                }
            }

            public string Action
            {
                get { return m_Action; }
            }

            public CodeMemberMethod Execute(CodeExpression targetObject)
            {

                if (m_cmExecute != null) return m_cmExecute;

                m_cmExecute = new CodeMemberMethod();

                // Method Signature
                m_cmExecute.Name = "Execute_" + m_cmMethod.Name;
                m_cmExecute.Attributes = MemberAttributes.Public;
                m_cmExecute.CustomAttributes.Add(new CodeAttributeDeclaration("TestMethod"));
                m_cmExecute.ReturnType = new CodeTypeReference("MFTestResults");

                // Method Body

                m_cmExecute.Statements.Add(
                    TestLog.LogComment(m_cmExecute.Name));

                bool iterationTest = (m_Namespace.TestIterations > 1);

                // Create the return variable and set its value
                // to MFTestResults.Pass.  Only a Fail will 
                // change this value.
                m_cmExecute.Statements.Add(
                    new CodeVariableDeclarationStatement(
                        "MFTestResults",
                        "testResult",
                        new CodeFieldReferenceExpression(
                            new CodeTypeReferenceExpression("MFTestResults"),
                            "Pass")));

                var excepHandler = new CodeTryCatchFinallyStatement();

                // Figure out if this method is for a ServiceImplementation
                // if so then there is a stack to push and pop variables 
                // for testing.
                var serviceImpl = m_TypeImpl as ServiceImplementation;

                // Clear the internal message passing stack
                // This prevents failures from previous tests
                // from hacking this test.
                if (serviceImpl != null)
                {
                    var push = new CodeMethodInvokeExpression(
                        serviceImpl.Stack.FieldReferenceExpression, "Clear", new CodeExpression[] { });
                    excepHandler.TryStatements.Add(push);
                }

                List<CodeVariableReferenceExpression> parameters = new List<CodeVariableReferenceExpression>();

                // Parameter handling
                int parameterIndex = 0;
                foreach (ParameterInfo pi in m_MethodInfo.GetParameters())
                {
                    // Declare and create an instance of a parameter
                    string varName = "param" + parameterIndex++;

                    // This will call the appropriate type factory
                    // based on the type of the parameter.  If the parameter
                    // type is an array then an array will be created.
                    excepHandler.TryStatements.Add(
                        new CodeVariableDeclarationStatement(
                            pi.ParameterType, varName,
                            pi.ParameterType.IsArray ? 
                                m_Namespace.TypeFactoryCollection[pi.ParameterType].ArrayInvocation : 
                                m_Namespace.TypeFactoryCollection[pi.ParameterType].Invocation));

                    var varRef = new CodeVariableReferenceExpression(
                        varName);

                    // Push the parameter onto the stack
                    if (serviceImpl != null)
                    {
                        var push = new CodeMethodInvokeExpression(
                            serviceImpl.Stack.FieldReferenceExpression, "Push", new CodeExpression[] { varRef });
                        excepHandler.TryStatements.Add(push);
                    }
                    // Put the parameter in the list for calling the method later on.
                    parameters.Add(varRef);
                }

                excepHandler.TryStatements.Add(
                    TestLog.LogComment(
                        string.Format("Calling service method {0} with {1} parameters for test", Action, parameterIndex)));

                // This is the invocation of the service method.
                var invoke = new CodeMethodInvokeExpression(
                    targetObject,
                    Action,
                    parameters.ToArray());


                if (m_MethodInfo.ReturnType.FullName != "System.Void")
                {
                    // Call the service method and capture the return value
                    // into a variable called retval.
                    var vardec = new CodeVariableDeclarationStatement(
                        m_MethodInfo.ReturnType,
                        "retval",
                        invoke);

                    // Add method invocation with return value
                    // capture.
                    excepHandler.TryStatements.Add(vardec);
                }
                else
                {
                    // Since there is no return value
                    // this test will use a manual reset event
                    // to synchronize with the service.

                    // Reset the manual reset event.
                    excepHandler.TryStatements.Add(
                        new CodeMethodInvokeExpression(
                        serviceImpl.Stack.FieldReferenceExpression, "Reset", new CodeExpression[] { }));

                    // Invoke the service method
                    excepHandler.TryStatements.Add(invoke);

                    // Wait for the service method to complete.
                    excepHandler.TryStatements.Add(
                        new CodeMethodInvokeExpression(
                        serviceImpl.Stack.FieldReferenceExpression, "Wait", new CodeExpression[] { }));
                }

                if (serviceImpl != null)
                {
                    // Pop the service side test result off the stack
                    // and store in a variable call serviceResult
                    var pop = new CodeMethodInvokeExpression(
                        serviceImpl.Stack.FieldReferenceExpression, "Pop", new CodeExpression[] { });

                    var cast = new CodeCastExpression("MFTestResults", pop);

                    string varName = "serviceResult";
                    excepHandler.TryStatements.Add(
                        new CodeVariableDeclarationStatement(
                            "MFTestResults", varName, cast));

                    var serviceResult = new CodeVariableReferenceExpression("serviceResult");

                    // Test the service side result for Fail
                    var test = new CodeConditionStatement();
                    test.Condition = new CodeBinaryOperatorExpression(
                        serviceResult,
                        CodeBinaryOperatorType.ValueEquality,
                        new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("MFTestResults"), "Fail"));
                    test.TrueStatements.Add(TestLog.LogComment("Server side test failed"));

                    // If the test failed then set the method return variable
                    // to Fail.
                    test.TrueStatements.Add(
                        new CodeAssignStatement()
                        {
                            Left = new CodeVariableReferenceExpression() { VariableName = "testResult" },
                            Right = serviceResult
                        });

                    // If the test passed just log a little message
                    test.FalseStatements.Add(
                        TestLog.LogComment("Server side test passed. Continuing..."));

                    excepHandler.TryStatements.Add(test);
                }

                // If the service method has a return value
                // then it will be tested here.
                if (m_MethodInfo.ReturnType.FullName != "System.Void" && serviceImpl != null)
                {
                    // Pop the return value off of the stack
                    // and store in a variable named stackval.
                    var pop = new CodeMethodInvokeExpression(
                        serviceImpl.Stack.FieldReferenceExpression, "Pop", new CodeExpression[] { });

                    var cast = new CodeCastExpression(m_MethodInfo.ReturnType, pop);

                    string varName = "stackval";
                    excepHandler.TryStatements.Add(
                        new CodeVariableDeclarationStatement(
                            m_MethodInfo.ReturnType, varName, cast));

                    excepHandler.TryStatements.Add(
                        TestLog.LogComment("Testing return value from server"));

                    // Test the return val against the popped stack val
                    var testCall = new CodeMethodInvokeExpression(
                        new CodeTypeReferenceExpression("ObjectTest"),
                        "TestEqualEx",
                        new CodeExpression[] { new CodeVariableReferenceExpression(varName), new CodeVariableReferenceExpression("retval") });

                    excepHandler.TryStatements.Add(testCall);
                }

                // Catch block
                // Catch all Exceptions and print out a log.
                var catchClause = new CodeCatchClause("e", new CodeTypeReference(typeof(Exception)));
                catchClause.Statements.Add(TestLog.LogException(new CodeVariableReferenceExpression("e")));

                // Set the method return variable to Fail
                catchClause.Statements.Add(
                    new CodeAssignStatement()
                    {
                        Left = new CodeVariableReferenceExpression() { VariableName = "testResult" },
                        Right = new CodeFieldReferenceExpression()
                        {
                            TargetObject = new CodeTypeReferenceExpression("MFTestResults"),
                            FieldName = "Fail"
                        }
                    });

                excepHandler.CatchClauses.Add(
                    catchClause);

                // If this test isn't run iteratively then add the 
                // try/catch block to the method's statemens
                // Otherwise, put the try/catch block into
                // a for loop to interate the requested number 
                // of times.
                if (!iterationTest)
                {
                    m_cmExecute.Statements.Add(excepHandler);
                }
                else
                {
                    var i = new CodeVariableReferenceExpression() { VariableName = "i" };
                    var one = new CodePrimitiveExpression(1);

                    // Iterate the requested number of times.
                    // m_Namespace.TestIterations contains value
                    // for the total loop count.
                    var forloop = new CodeIterationStatement()
                    {
                        InitStatement = new CodeVariableDeclarationStatement()
                        {
                            Type = new CodeTypeReference(typeof(int)),
                            Name = i.VariableName,
                            InitExpression = one
                        },
                        TestExpression = new CodeBinaryOperatorExpression()
                        {
                            Left = i,
                            Operator = CodeBinaryOperatorType.LessThanOrEqual,
                            Right = new CodePrimitiveExpression(m_Namespace.TestIterations)
                        },
                        IncrementStatement = new CodeAssignStatement()
                        {
                            Left = i,
                            Right = new CodeBinaryOperatorExpression()
                            {
                                Left = i,
                                Operator = CodeBinaryOperatorType.Add,
                                Right = one
                            }
                        }
                    };

                    forloop.Statements.Add(TestLog.LogComment(
                        new CodeBinaryOperatorExpression(
                            new CodePrimitiveExpression("Running Test Case Number"),
                            CodeBinaryOperatorType.Add,
                            i)));

                    forloop.Statements.Add(excepHandler);

                    m_cmExecute.Statements.Add(forloop);
                }

                // Return the final test result
                m_cmExecute.Statements.Add(
                    new CodeMethodReturnStatement(
                        new CodeVariableReferenceExpression() { VariableName = "testResult" }));

                return m_cmExecute;
            }
        }
        
        class ServiceImplementation : InterfaceImplementation
        {
            private ServiceMember m_ServiceMember;
            private ConstructorMember m_Constructor;
            private ClientProxyMember m_Proxy;
            private FieldMember m_Stack;

            internal ServiceImplementation(Type type, FixtureNamespace ns)
                : base(type, ns)
            {
                this.m_TypeDecl.BaseTypes.Add(
                    new CodeTypeReference("Microsoft.SPOT.Platform.Test.IMFTestInterface"));

                m_ServiceMember = new ServiceMember(ns.GetType(m_TypePrefix));
                m_TypeDecl.Members.Add(m_ServiceMember.MemberField);

                m_Proxy = new ClientProxyMember(
                    ns.GetType(m_TypePrefix + "ClientProxy"), ns);
                m_TypeDecl.Members.Add(m_Proxy.MemberField);

                m_Stack = new FieldMember(
                    ns.GetReferencedType("MFDpwsTestFixtureUtilities.Stack"));
                m_TypeDecl.Members.Add(m_Stack.MemberFieldAndInit);                
                m_Constructor = new ConstructorMember(
                    this,
                    ns);                
            }

            public override CodeTypeDeclaration CodeTypeDeclaration
            {
                get
                {
                    CodeTypeDeclaration typeDecl = base.CodeTypeDeclaration;

                    typeDecl.Members.Add(m_Constructor.Constructor);

                    typeDecl.Members.Add(Main);

                    typeDecl.Members.Add(ServiceMethod("Initialize", true));

                    typeDecl.Members.Add(ServiceMethod("CleanUp", false));

                    foreach (InterfaceMethodImplementation imi in this.m_Methods)
                    {
                        CodeMemberMethod execute =
                            imi.Execute(m_Proxy.FieldReferenceExpression);

                        typeDecl.Members.Add(
                            execute);
                    }

                    return typeDecl;
                }
            }

            private CodeEntryPointMethod Main
            {
                get
                {
                    var main = new CodeEntryPointMethod();

                    var args = new CodeVariableDeclarationStatement(
                        typeof(object[]),
                        "args",
                        new CodeArrayCreateExpression(
                            typeof(object),
                            new CodeExpression[] { new CodePrimitiveExpression(m_TypeName) }));

                    main.Statements.Add(args);

                    var create = new CodeObjectCreateExpression(
                        m_FixtureNamespace.GetReferencedType("Microsoft.SPOT.Platform.Test.MFTestRunner"),
                        new CodeExpression[] { new CodeVariableReferenceExpression("args") });

                    main.Statements.Add(create);

                    return main;
                }
            }

            private CodeMemberMethod ServiceMethod(string name, bool start)
            {
                CodeMemberMethod method = new CodeMemberMethod();
                method.Name = name;
                method.Attributes = MemberAttributes.Public;
                if (string.Equals(name, "Initialize"))
                {
                    method.ReturnType = new CodeTypeReference("InitializeResult"); 
                }


                if (start)
                {
                    // Add the setup attribute.
                    method.CustomAttributes.Add(new CodeAttributeDeclaration("SetUp"));

                    // Start a try block and check if networking support is available by attempting to get host entry from a
                    // ITG server "itgproxy.dns.microsoft.com". 
                    // ??? What do we do for customers - for them this server is probably not accessible.
                    CodeTryCatchFinallyStatement tryStatement = new CodeTryCatchFinallyStatement();
                    CodeComment comment = new CodeComment("Check networking - we need to make sure we can reach our proxy server.");
                    CodeCommentStatement commentStatement = new CodeCommentStatement(comment);
                    tryStatement.TryStatements.Add(commentStatement);
                    CodeExpression invokeExpression = new CodeMethodInvokeExpression(
                        new CodeTypeReferenceExpression("System.Net.Dns"),
                        "GetHostEntry", new CodePrimitiveExpression("itgproxy.dns.microsoft.com"));
                    tryStatement.TryStatements.Add(invokeExpression);                                                          
                    
                    // Add the catch clause and return a skip since networking support is not available.
                    var catchClause = new CodeCatchClause("ex", new CodeTypeReference(typeof(Exception)));                    
                    comment = new CodeComment("Unable to get address for itgproxy.dns.microsoft.com");
                    commentStatement = new CodeCommentStatement(comment);
                    catchClause.Statements.Add(commentStatement);
                    catchClause.Statements.Add(TestLog.LogException(new CodeVariableReferenceExpression("ex")));
                    var skipVal = new CodeVariableReferenceExpression("InitializeResult.Skip");
                    catchClause.Statements.Add(new CodeMethodReturnStatement(skipVal));
                    tryStatement.CatchClauses.Add(catchClause);
                    method.Statements.Add(tryStatement);

                    // Start the device utility and return readytogo status to the test runner.
                    method.Statements.Add(DeviceUtility.Start);
                    var returnVal = new CodeVariableReferenceExpression("InitializeResult.ReadyToGo");
                    method.Statements.Add(new CodeMethodReturnStatement(returnVal));
                }
                else
                {
                    method.CustomAttributes.Add(new CodeAttributeDeclaration("TearDown"));
                    method.Statements.Add(
                        new CodeMethodInvokeExpression(
                            m_Proxy.FieldReferenceExpression,
                            "Dispose",
                            new CodeExpression[] { }));
                    method.Statements.Add(DeviceUtility.Stop);
                }

                return method;
            }

            public FieldMember Stack
            {
                get
                {
                    return m_Stack;
                }
            }

            #region Nested Classes

            class ServiceMember: FieldMember
            {
                internal ServiceMember(Type type)
                    : base(type)
                {}

                public CodePropertyReferenceExpression ServiceID
                {
                    get
                    {
                        return new CodePropertyReferenceExpression(
                            FieldReferenceExpression,
                            "ServiceID");
                    }
                }

                public override CodeObjectCreateExpression FieldCreateExpression
                {
                    get
                    {
                        return new CodeObjectCreateExpression(m_TypeName, new CodeExpression[] { new CodeThisReferenceExpression() });
                    }
                }
            }

            class ClientProxyMember : FieldMember
            {
                FixtureNamespace m_Namespace;
                Type m_ConstructorArgType;

                internal ClientProxyMember(Type type, FixtureNamespace ns)
                    : base(type)
                {
                    m_Namespace = ns;
                    m_ConstructorArgType = null;

                    foreach (ConstructorInfo ci in type.GetConstructors())
                    {
                        ParameterInfo[] pi = ci.GetParameters();

                        if (pi.Length == 0)
                        {
                            continue;
                        }
                        if (pi.Length == 3)
                        {
                            if (pi[2].ParameterType.IsInterface)
                            {
                                m_ConstructorArgType = pi[2].ParameterType;
                                break;
                            }
                        }
                    }
                }

                public override CodeObjectCreateExpression FieldCreateExpression
                {
                    get
                    {                        
                        if (m_ConstructorArgType == null)
                        {
                            return new CodeObjectCreateExpression(m_TypeName, 
                                new CodeObjectCreateExpression("WS2007HttpBinding"),
                                new CodeObjectCreateExpression("ProtocolVersion10")
                                );
                        }
                        else
                        {
                            return new CodeObjectCreateExpression(
                                m_TypeName,
                                new CodeObjectCreateExpression("WS2007HttpBinding"),
                                new CodeObjectCreateExpression("ProtocolVersion10"),
                                m_Namespace[m_ConstructorArgType].CreateExpression);
                        }
                    }
                }

                public CodePropertyReferenceExpression ServiceEndpoint
                {
                    get
                    {
                        return new CodePropertyReferenceExpression(
                            this.FieldReferenceExpression, 
                            "EndpointAddress");                            
                    }
                }

                public CodeAssignStatement SetLocalEndpoint(CodeExpression serviceId)
                {
                    return SetEndpoint(DeviceUtility.ToString(DeviceUtility.IPV4Address), serviceId);
                }

                public CodeAssignStatement SetEndpoint(CodeExpression ipAddress, CodeExpression serviceId)
                {
                    var binop = new CodeBinaryOperatorExpression(
                        new CodePrimitiveExpression("http://"),
                        CodeBinaryOperatorType.Add,
                        new CodeBinaryOperatorExpression(
                            ipAddress,
                            CodeBinaryOperatorType.Add,
                            new CodeBinaryOperatorExpression(
                                new CodePrimitiveExpression(":8084/"),
                                CodeBinaryOperatorType.Add,
                                new CodeMethodInvokeExpression( serviceId,
                                "Substring", 
                                new CodePrimitiveExpression(9)
                                ))));

                    return new CodeAssignStatement(
                        ServiceEndpoint,
                        binop);
                }
            }

            class ConstructorMember
            {
                FixtureNamespace m_Namespace;
                ServiceImplementation m_Type;
                internal ConstructorMember(ServiceImplementation impl, FixtureNamespace ns)
                {
                    m_Type = impl;
                    m_Namespace = ns;
                }

                public CodeConstructor Constructor
                {
                    get
                    {
                        CodeConstructor con = new CodeConstructor();
                        
                        CodeTryCatchFinallyStatement tryStatement = new CodeTryCatchFinallyStatement();
                        CodeComment comment = new CodeComment("Check networking - we need to make sure we can reach our proxy server.");
                        CodeCommentStatement commentStatement = new CodeCommentStatement(comment);
                        tryStatement.TryStatements.Add(commentStatement);
                        CodeExpression invokeExpression = new CodeMethodInvokeExpression(
                            new CodeTypeReferenceExpression("System.Net.Dns"),
                            "GetHostEntry", new CodePrimitiveExpression("itgproxy.dns.microsoft.com"));
                        tryStatement.TryStatements.Add(invokeExpression);

                        // Add the catch clause and return a skip since networking support is not available.
                        var catchClause = new CodeCatchClause("ex", new CodeTypeReference(typeof(Exception)));
                        comment = new CodeComment("Unable to get address for itgproxy.dns.microsoft.com");
                        commentStatement = new CodeCommentStatement(comment);
                        catchClause.Statements.Add(commentStatement);
                        catchClause.Statements.Add(TestLog.LogException(new CodeVariableReferenceExpression("ex")));
                        CodeThrowExceptionStatement throwException = new CodeThrowExceptionStatement(
                            new CodeObjectCreateExpression(
                                new CodeTypeReference(typeof(System.NotSupportedException)),
                                new CodeExpression[] { }));
                        catchClause.Statements.Add(throwException);
                        tryStatement.CatchClauses.Add(catchClause);
                        con.Statements.Add(tryStatement);

                        con.Statements.Add(
                            new CodeMethodInvokeExpression(
                                new CodeVariableReferenceExpression("Device"),
                                "Initialize",
                                new CodeObjectCreateExpression("WS2007HttpBinding", 
                                    new CodeObjectCreateExpression("HttpTransportBindingConfig", 
                                        new CodeBinaryOperatorExpression(
                                            new CodePrimitiveExpression("urn:uuid:"),
                                            CodeBinaryOperatorType.Add,
                                            new CodeMethodInvokeExpression(
                                                new CodeMethodInvokeExpression(
                                                    new CodeVariableReferenceExpression("Guid"),
                                                    "NewGuid"
                                                ),
                                                "ToString"
                                            )
                                        ),
                                        new CodePrimitiveExpression(8084)
                                    )
                                ),
                                new CodeObjectCreateExpression("ProtocolVersion10")

                            )
                        );

                        // Create the Service Member
                        con.Statements.Add(m_Type.m_ServiceMember.FieldAssignStatement);

                        // Add this service to the Device.HostedServices property
                        con.Statements.Add(
                            DeviceUtility.AddHostedService(m_Type.m_ServiceMember.FieldReferenceExpression));

                        // Create the Client Proxy member
                        con.Statements.Add(
                            m_Type.m_Proxy.FieldAssignStatement);

                        // Set the Client Proxy ServiceEndpoint
                        con.Statements.Add(
                            m_Type.m_Proxy.SetLocalEndpoint(m_Type.m_ServiceMember.ServiceID));

                        return con;
                    }
                }
            }

            #endregion
        }

        class FieldMember
        {
            protected Type m_Type;
            protected string m_TypeName;
            protected string m_InstanceName;

            internal FieldMember(Type type)
            {
                m_Type = type;

                m_TypeName = type.Name;

                m_InstanceName = "m_" + m_TypeName;
            }

            public virtual CodeMemberField MemberField
            {
                get { return new CodeMemberField(m_TypeName, m_InstanceName); }
            }

            public virtual CodeFieldReferenceExpression FieldReferenceExpression
            {
                get
                {
                    return new CodeFieldReferenceExpression(
                        new CodeThisReferenceExpression(),
                        m_InstanceName);
                }
            }

            public virtual CodeObjectCreateExpression FieldCreateExpression
            {
                get
                {
                    return new CodeObjectCreateExpression(m_TypeName);
                }
            }

            public virtual CodeMemberField MemberFieldAndInit
            {
                get
                {
                    var mf = MemberField;                    
                    mf.InitExpression = FieldCreateExpression;
                    return mf;
                }
            }

            public virtual CodeMemberField ArrayMemberField
            {
                get{ return new CodeMemberField(m_Type + "[]", m_InstanceName); }
            }

            public virtual CodeArrayCreateExpression GetArrayCreateExpression(int size)
            {
                return GetArrayCreateExpression(new CodePrimitiveExpression(size));
            }

            public virtual CodeArrayCreateExpression GetArrayCreateExpression(CodeExpression size)
            {
                return new CodeArrayCreateExpression(m_Type, size);
            }

            public virtual CodeMemberField GetArrayMemberFieldAndInit(int size)
            {
                return GetArrayMemberFieldAndInit(new CodePrimitiveExpression(size));
            }

            public virtual CodeMemberField GetArrayMemberFieldAndInit(CodeExpression size)
            {
                var mf = ArrayMemberField;
                mf.InitExpression = GetArrayCreateExpression(size);
                return mf;
            }

            public virtual CodeAssignStatement FieldAssignStatement
            {
                get
                {
                    return new CodeAssignStatement(
                        FieldReferenceExpression,
                        FieldCreateExpression);
                }
            }

            public virtual CodeArrayIndexerExpression GetArrayValue(int index)
            {
                return GetArrayValue(new CodePrimitiveExpression(index));
            }

            public virtual CodeArrayIndexerExpression GetArrayValue(CodeExpression index)
            {
                return new CodeArrayIndexerExpression(
                    FieldReferenceExpression, index);
            }
        }

        public class DeviceUtility
        {
            public static CodeTypeReferenceExpression Reference
            {
                get
                {
                    return new CodeTypeReferenceExpression("Device");
                }
            }

            public static CodePropertyReferenceExpression HostedServices
            {
                get
                {
                    return new CodePropertyReferenceExpression(
                        Reference, "HostedServices");
                }
            }

            public static CodeMethodInvokeExpression AddHostedService(CodeExpression hostedService)
            {
                return new CodeMethodInvokeExpression(
                    HostedServices,
                    "Add",
                    new CodeExpression[] { hostedService });
            }

            public static CodePropertyReferenceExpression IPV4Address
            {
                get
                {
                    return new CodePropertyReferenceExpression(
                        Reference, "IPV4Address");
                }
            }

            public static CodeMethodInvokeExpression ToString(CodeExpression targetObject)
            {
                return new CodeMethodInvokeExpression(
                    targetObject,
                    "ToString",
                    new CodeExpression[] { });
            }

            public static CodeMethodInvokeExpression Start
            {
                get
                {
                    return InvokeMethod("Start", new CodeObjectCreateExpression("ServerBindingContext", new CodeObjectCreateExpression("ProtocolVersion10")));
                }
            }

            public static CodeMethodInvokeExpression Stop
            {
                get
                {
                    return InvokeMethod("Stop");
                }
            }

            private static CodeMethodInvokeExpression InvokeMethod(string methodName, params CodeExpression[] parms)
            {
                return new CodeMethodInvokeExpression(
                    Reference,
                    methodName,
                    parms);
            }
        }

        #endregion

    }

    public class TestLog
    {
        public static CodeMethodInvokeExpression LogComment(string message)
        {
            return LogComment(new CodePrimitiveExpression(message));
        }

        public static CodeMethodInvokeExpression LogComment(CodeExpression ce)
        {
            return GenLog("Comment", ce);
        }

        public static CodeMethodInvokeExpression LogException(CodeVariableReferenceExpression e)
        {
            var message = new CodeBinaryOperatorExpression(
                new CodePropertyReferenceExpression(e, "Message"),
                CodeBinaryOperatorType.Add,
                new CodeBinaryOperatorExpression(
                    new CodePrimitiveExpression("\r\n\r\n"),
                    CodeBinaryOperatorType.Add,
                    new CodePropertyReferenceExpression(e, "StackTrace")));

            return LogException(message);
        }

        public static CodeMethodInvokeExpression LogException(string message)
        {
            return LogException(new CodePrimitiveExpression(message));
        }

        public static CodeMethodInvokeExpression LogException(CodeExpression ce)
        {
            return GenLog("Exception", ce);
        }

        private static CodeMethodInvokeExpression GenLog(string method, CodeExpression expr)
        {
            return new CodeMethodInvokeExpression(
                new CodeTypeReferenceExpression("Log"),
                method,
                new CodeExpression[] { expr });
        }
    }

    public class TypeFactoryCollection
    {
        Dictionary<Type, TypeFactoryGenerator> m_typeDictionary = new Dictionary<Type, TypeFactoryGenerator>();
        CodeTypeDeclarationCollection m_decCollection = new CodeTypeDeclarationCollection();

        CodeRegionDirective region = new CodeRegionDirective(
            CodeRegionMode.Start, "Type Factories");

        public TypeFactoryGenerator this[Type t]
        {
            get
            {
                if (t.IsArray)
                {
                    t = t.GetElementType();
                }

                if (m_typeDictionary.ContainsKey(t))
                {
                    return m_typeDictionary[t];
                }

                TypeFactoryGenerator g = TypeFactoryGenerator.GetFactory(t, this);

                m_typeDictionary.Add(t, g);

                var declaration = g.Declaration;
                if (declaration != null)
                {
                    m_decCollection.Add(g.Declaration);
                }

                return g;
            }
        }

        public TypeFactoryGenerator this[string t]
        {
            get
            {
                return this[Type.GetType(t)];
            }
        }

        public void AddToNamespace(CodeNamespace ns)
        {

            ns.Types.AddRange(m_decCollection);
        }

        public string InterfaceToImplementation(string interfaceName)
        {
            if (interfaceName.StartsWith("I"))
            {
                interfaceName = interfaceName.Substring(1);
            }

            return interfaceName + "Implementation";
        }
    }


    public class TypeFactoryGenerator
    {
        private Type m_type;
        private string m_FactoryName;
        private TypeFactoryCollection m_Collection;
        private CodeMemberMethod m_Complete;

        public static TypeFactoryGenerator GetFactory(Type t, TypeFactoryCollection c)
        {
            return new TypeFactoryGenerator(t, c);
        }

        protected TypeFactoryGenerator(Type t, TypeFactoryCollection c)
        {
            m_type = t;
            m_FactoryName = m_type.Name + "Factory";
            m_Collection = c;
        }

        public CodeTypeDeclaration Declaration
        {
            get
            {
                switch (m_type.FullName)
                {
                    case "System.String":
                    case "System.Int16":
                    case "System.UInt16":
                    case "System.Int32":
                    case "System.UInt32":
                    case "System.Int64":
                    case "System.UInt64":
                    case "System.Double":
                    case "System.Single":
                    case "System.Boolean":
                    case "System.Byte":
                    case "System.SByte":
                    case "System.TimeSpan":
                    case "System.DateTime":
                    case "Ws.Services.Xml.WsXmlNode":
                    case "Ws.Services.Xml.WsXmlAttribute":
                        return null;
                    case "System.Uri":                    
                    default:
                        CodeTypeDeclaration dec = new CodeTypeDeclaration(m_FactoryName);

                        dec.Attributes = MemberAttributes.Public;

                        dec.Members.Add(GetObjectComplete());

                        dec.Members.Add(GetArrayComplete());

                        return dec;
                }
            }
        }

        public CodeMethodInvokeExpression Invocation
        {
            get
            {
                return FactoryInvoke("GetObject");
            }
        }

        public CodeMethodInvokeExpression ArrayInvocation
        {
            get
            {
                return FactoryInvoke("GetArray");
            }
        }

        protected CodeMethodInvokeExpression FactoryInvoke(string methodName)
        {
            CodeMethodInvokeExpression invoke = new CodeMethodInvokeExpression(
                    new CodeTypeReferenceExpression(m_FactoryName),
                    methodName,
                    new CodeExpression[] { });

            return invoke;
        }

        protected CodeMemberMethod GetObject()
        {
            CodeMemberMethod method = new CodeMemberMethod();
            method.Name = "GetObject";
            method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            method.ReturnType = new CodeTypeReference(m_type);

            return method;
        }

        /*
         
        Supported Types from MFSvcUtil spec
        
        System.String
        Int16 
        UInt16
        Int32
        UInt32
        Int64
        UInt64
        Double
        Single
        Boolean
        Byte
        Byte[]
        SByte
        System.TimeSpan
        System.DateTime
        System.Uri

        */

        protected CodeMemberMethod GetObjectComplete()
        {
            if (m_Complete != null) return m_Complete;

            CodeMemberMethod method = GetObject();
            switch (m_type.FullName)
            {
                case "System.String":
                    method.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression("Foo")));
                    break;
                case "System.Int16":
                    method.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(Int16.MinValue)));
                    break;
                case "System.UInt16":
                    method.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(UInt16.MaxValue)));
                    break;
                case "System.Int32":
                    method.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(Int32.MinValue)));
                    break;
                case "System.UInt32":
                    method.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(UInt32.MaxValue)));
                    break;
                case "System.Int64":
                    method.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(Int64.MinValue)));
                    break;
                case "System.UInt64":
                    method.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(UInt64.MaxValue)));
                    break;
                case "System.Double":
                    method.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(Double.MaxValue)));
                    break;
                case "System.Single":
                    method.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(Single.MaxValue)));
                    break;
                case "System.Boolean":
                    method.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(true)));
                    break;
                case "System.Byte":
                    method.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(Byte.MinValue)));
                    break;
                case "System.SByte":
                    method.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(SByte.MaxValue)));
                    break;
                case "System.TimeSpan":
                    // return TimeSpan.MaxValue
                    method.Statements.Add(new CodeMethodReturnStatement(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(typeof(System.TimeSpan)), "MaxValue")));
                    break;
                case "System.DateTime":
                    method.Statements.Add(new CodeMethodReturnStatement(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(typeof(System.DateTime)), "MaxValue")));
                    break;
                case "System.Uri":
                    method.Statements.Add(new CodeMethodReturnStatement(
                        new CodeObjectCreateExpression(typeof(System.Uri), new CodeExpression[] { new CodePrimitiveExpression("http://foobar.com") })));
                    break;
                case "Ws.Services.Xml.WsXmlNode":
                    method.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));
                    break;
                case "Ws.Services.Xml.WsXmlAttribute":
                    method.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));
                    break;
                default:
                    string varName = "retvar";

                    method.Statements.Add(
                        new CodeVariableDeclarationStatement(m_type, varName,
                            new CodeObjectCreateExpression(m_type, new CodeExpression[] { })));

                    CodeVariableReferenceExpression retvar = new CodeVariableReferenceExpression(varName);

                    // Enums are complicated.
                    // The result will simply be a new enum set to the default value.
                    if (!m_type.IsEnum)
                    {
                        foreach (FieldInfo finfo in m_type.GetFields(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Static))
                        {
                            // If the field is read only then ignore it. (For now).
                            if ((finfo.Attributes & FieldAttributes.InitOnly) == FieldAttributes.InitOnly)
                            {
                                continue;
                            }

                            CodeExpression referenceExpression;

                            if (!finfo.IsStatic)
                            {
                                referenceExpression = retvar;
                            }
                            else
                            {
                                referenceExpression = new CodeTypeReferenceExpression(m_type);
                            }

                            CodeFieldReferenceExpression fre = new CodeFieldReferenceExpression(
                                    referenceExpression,
                                    finfo.Name);

                            CodeMethodInvokeExpression invoke = null;

                            if (!finfo.FieldType.IsArray)
                            {
                                invoke = m_Collection[finfo.FieldType].Invocation;
                            }
                            else
                            {
                                invoke = m_Collection[finfo.FieldType.GetElementType()].ArrayInvocation;
                            }

                            CodeAssignStatement assign = new CodeAssignStatement(
                                fre,
                                invoke
                                );

                            method.Statements.Add(assign);
                        }
                    }
                    method.Statements.Add(new CodeMethodReturnStatement(retvar));

                    break;
            }

            m_Complete = method;
            return method;
        }

        protected CodeMemberMethod GetArray()
        {
            CodeMemberMethod method = new CodeMemberMethod();
            method.Name = "GetArray";
            method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            method.ReturnType = new CodeTypeReference(
                new CodeTypeReference(m_type),
                1);

            return method;
        }

        protected CodeMemberMethod GetArrayComplete()
        {
            CodeMemberMethod method = GetArray();

            switch (m_type.FullName)
            {
                case "Ws.Services.Xml.WsXmlNode":
                    method.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));
                    break;
                case "Ws.Services.Xml.WsXmlAttribute":
                    method.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));
                    break;
                default:
                    CodeArrayCreateExpression array = new CodeArrayCreateExpression(
                        m_type,
                        new CodeExpression[] 
                        { 
                            new CodeMethodInvokeExpression(
                                new CodeTypeReferenceExpression(m_FactoryName),
                                "GetObject",
                                new CodeExpression[] {})
                        });


                    CodeMethodReturnStatement ret = new CodeMethodReturnStatement(array);
                    method.Statements.Add(ret);
                    break;
            }
            return method;
        }

    }
}
