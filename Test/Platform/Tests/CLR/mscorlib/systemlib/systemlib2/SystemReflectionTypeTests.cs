////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;
using System.Collections;


namespace Microsoft.SPOT.Platform.Tests
{
    delegate bool MyDelegate(int i);

    public class SystemReflectionTypeTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");
            // Add your functionality here.   

            Log.Comment("Tests RuntimeType and System.Type reflection");

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests");
        }

        [TestMethod]
        public MFTestResults SystemReflectionType_ObjectGetType_Test0()
        {
            bool fRes = true;

            ///
            /// Test object.GetType method for various types (including
            /// reflection types and arrays of reflection types).
            /// 
            object o = (object)1;
            fRes &= (o.GetType() == typeof(int));

            o = (object)typeof(Type);
            fRes &= o.GetType() == typeof(Type).GetType();

            o = AppDomain.CurrentDomain.GetAssemblies();
            fRes &= o.GetType() == typeof(Assembly[]);

            o = new TestClass();
            fRes &= o.GetType() == typeof(TestClass);

            o = new TestStruct();
            fRes &= o.GetType() == typeof(TestStruct);

            o = new MyDelegate(MyDelegateImpl);
            fRes &= o.GetType() == typeof(MyDelegate);

            o = (new MyDelegate(MyDelegateImpl)).Method;
            Debug.Print("object (MethodInfo) GetType: " + o.GetType().ToString());

            MethodInfo mi = typeof(SystemReflectionTypeTests).GetMethod("MyDelegateImpl", BindingFlags.Static | BindingFlags.NonPublic);
            fRes &= o.GetType() == mi.GetType();

            return fRes ? MFTestResults.Pass : MFTestResults.Fail;
        }


        [TestMethod]
        public MFTestResults SystemReflectionType_RuntimeType_Test1()
        {
            bool fRes = true;
            ArrayList list = new ArrayList();
            int i = 0;

            ///
            /// Test the RuntimeType class members
            /// 
            TestClass cls = new TestClass();

            // Test Type members for a class type
            Type t = cls.GetType();
            Assembly asm = t.Assembly;
            list.Add(asm);
            fRes &= ((Assembly)list[i]).GetName().Name == "Microsoft.SPOT.Platform.Tests.Systemlib2";
            fRes &= asm.GetName().Name == "Microsoft.SPOT.Platform.Tests.Systemlib2";
            fRes &= t.Name == "TestClass";
            fRes &= t.FullName == "Microsoft.SPOT.Platform.Tests.SystemReflectionTypeTests+TestClass";
            fRes &= t.BaseType == typeof(object);
            fRes &= t.GetElementType() == null;

            MethodInfo []mis = t.GetMethods();
            fRes &= mis[0].Name == "Method1";
            mis = t.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
            fRes &= mis[0].Name == "Method2";
            fRes &= t.GetMethod("Method1") != null;
            fRes &= t.GetMethod("Method2", BindingFlags.Instance | BindingFlags.NonPublic) != null;
            
            FieldInfo[] fis = t.GetFields();
            fRes &= fis[0].Name == "m_Field1";
            fis = t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            fRes &= fis[0].Name == "m_Field2";
            fRes &= t.GetField("m_Field1") != null;
            fRes &= t.GetField("m_Field2", BindingFlags.NonPublic | BindingFlags.Instance) != null;
            fRes &= t.GetConstructor(new Type[] { }) != null;
            
            Type[] ifaces = t.GetInterfaces();
            fRes &= ifaces.Length == 2;
            fRes &= ifaces[0].Name == "IInterface1";
            fRes &= ifaces[1].Name == "IInterface2";
            fRes &= t.IsSubclassOf(typeof(object));
            i++;

            // test Type members for a struct valuetype
            TestStruct str = new TestStruct();
            t = str.GetType();
            asm = t.Assembly;
            list.Add(asm);
            fRes &= ((Assembly)list[i]).GetName().Name == "Microsoft.SPOT.Platform.Tests.Systemlib2";
            fRes &= asm.GetName().Name == "Microsoft.SPOT.Platform.Tests.Systemlib2";
            fRes &= t.Name == "TestStruct";
            fRes &= t.FullName == "Microsoft.SPOT.Platform.Tests.SystemReflectionTypeTests+TestStruct";
            fRes &= t.BaseType == typeof(System.ValueType);
            fRes &= t.GetInterfaces().Length == 0;
            fRes &= t.GetElementType() == null;
            i++;

            // test Type members for an Assembly reflection type
            Assembly asmObj = typeof(TestClass).Assembly;
            t = asmObj.GetType();
            asm = t.Assembly;
            list.Add(asm);
            fRes &= ((Assembly)list[i]).GetName().Name == "mscorlib";
            fRes &= asm.GetName().Name == "mscorlib";
            fRes &= t.Name == "Assembly";
            fRes &= t.FullName == "System.Reflection.Assembly";
            fRes &= t.BaseType == typeof(Object);
            fRes &= t.GetInterfaces().Length == 0;
            fRes &= t.GetElementType() == null;

            mis = typeof(TestClass).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            t = mis.GetType();
            fRes &= t.Name == "RuntimeMethodInfo[]";
            fRes &= t.FullName == "System.Reflection.RuntimeMethodInfo[]";
            fRes &= t.BaseType == typeof(Array);
            fRes &= t.GetInterfaces().Length > 0;
            fRes &= t.GetElementType().Name == "RuntimeMethodInfo";

            // test Type members for a delegate
            Delegate del = new MyDelegate(MyDelegateImpl);
            t = del.GetType();
            fRes &= t.DeclaringType == null;
            fRes &= t.Name == "MyDelegate";
            fRes &= t.BaseType == typeof(MulticastDelegate);

            // test Type members for an enum
            TestEnum en = TestEnum.Item1;
            t = en.GetType();
            fRes &= t.DeclaringType == typeof(SystemReflectionTypeTests);
            fRes &= t.IsEnum;
            fRes &= !t.IsAbstract;
            fRes &= !t.IsClass;
            fRes &= !t.IsPublic;
            fRes &= t.IsValueType;

            return fRes ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults SystemReflectionType_SystemType_Test2()
        {
            bool fRes = true;

            /// 
            /// Test System.Type members that use or relate to reflection
            /// 

            int[] blah = new int[3];

            fRes &= typeof(Array).IsInstanceOfType(blah);
            fRes &= typeof(TestStruct[]).IsArray;
            fRes &= !typeof(Array).IsValueType;
            fRes &= typeof(TestStruct).IsValueType;
            fRes &= typeof(Type).IsSubclassOf(typeof(MemberInfo));
            fRes &= typeof(Type).GetInterfaces()[0].Name == "IReflect";
            fRes &= typeof(MyDelegate).IsInstanceOfType(new MyDelegate(MyDelegateImpl));

            // Get known type from assembly qualified type name Culture and PublicKeyToken are used by debugger to identify types
            // so we must be able to parse them (even if we through out the culture/key).
            Type t = Type.GetType("System.Int32, mscorlib, version=4.1.0.0, CultureInfo=enu, PublicKeyToken=null");

            fRes &= t != null;
            
            return fRes ? MFTestResults.Pass : MFTestResults.Fail;
        }

        //------------------- Classes/interfaces/structs used by this test class ---------------------------//

        #region INTERNAL_TEST_CLASSES

        enum TestEnum
        {
            Item1,
            Item2,
            Item3
        };

        private static bool MyDelegateImpl(int i)
        {
            return true;
        }

        public struct TestStruct
        {
            public int Field1;
            public bool Field2;
        }

        interface IInterface1
        {
        }
        interface IInterface2
        {
        }

        class TestClass : IInterface1, IInterface2
        {
            public int m_Field1;
            private float m_Field2;

            public int Method1(bool b)
            {
                m_Field1 = 3;
                return m_Field1;
            }

            private bool Method2(int i)
            {
                m_Field2 = (float)i;
                return true;
            }
        }
        #endregion
    }
}