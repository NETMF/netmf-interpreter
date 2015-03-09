////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;
using System.Collections;


namespace Microsoft.SPOT.Platform.Tests
{
    public class SystemReflectionMemberTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");
            // Add your functionality here.   

            Log.Comment("Testing Reflection types MethodInfo, ConstructorInfo, and FieldInfo");

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests");
        }

        [TestMethod]
        public MFTestResults SystemReflectionMemberTests_Properties_Test0()
        {
            bool fRes = true;

            TestClass tst = new TestClass();

            ///
            /// Test the PropertyInfo class members
            /// 
            MethodInfo mi = typeof(TestClass).GetMethod("BaseInternalProtectedMethod", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            fRes &= !mi.IsAbstract;
            fRes &= !mi.IsFinal;
            fRes &= !mi.IsPublic;
            fRes &= !mi.IsStatic;
            fRes &= !mi.IsVirtual;
            fRes &= mi.ReturnType == typeof(object);
            fRes &= mi.Invoke(tst, new object[] { 3 }) == null;
            fRes &= mi.DeclaringType == typeof(AbsTestClass);

            mi = typeof(AbsTestClass).GetMethod("AbstractPublicMethod", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            fRes &= mi.IsAbstract;
            fRes &= !mi.IsFinal;
            fRes &= mi.IsPublic;
            fRes &= !mi.IsStatic;
            fRes &= mi.IsVirtual;
            fRes &= mi.ReturnType == typeof(float);
            fRes &= (float)mi.Invoke(tst, new object[] { 3 }) == 38.4f;
            fRes &= mi.DeclaringType == typeof(AbsTestClass);

            mi = typeof(TestClass).GetMethod("VirtualInternalMethod", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            fRes &= !mi.IsAbstract;
            fRes &= !mi.IsFinal;
            fRes &= !mi.IsPublic;
            fRes &= !mi.IsStatic;
            fRes &= mi.IsVirtual;
            fRes &= mi.ReturnType == typeof(int);
            fRes &= (int)mi.Invoke(tst, new object[] { true }) == 34;
            fRes &= mi.DeclaringType == typeof(TestClass);

            mi = typeof(TestClass).GetMethod("SealedPublicMethod", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            fRes &= !mi.IsAbstract;
            fRes &= mi.IsFinal;
            fRes &= mi.IsPublic;
            fRes &= !mi.IsStatic;
            fRes &= mi.IsVirtual;
            fRes &= mi.ReturnType == typeof(bool);
            fRes &= (bool)mi.Invoke(tst, new object[] {});
            fRes &= mi.DeclaringType == typeof(TestClass);

            mi = typeof(TestClass).GetMethod("StaticPrivateAbsMethod", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            fRes &= !mi.IsAbstract;
            fRes &= !mi.IsFinal;
            fRes &= !mi.IsPublic;
            fRes &= mi.IsStatic;
            fRes &= !mi.IsVirtual;
            fRes &= mi.ReturnType == typeof(void);
            TestClass.s_WasStaticMethodCalled = false;
            mi.Invoke(tst, new object[] {});
            fRes &= TestClass.s_WasStaticMethodCalled;
            fRes &= mi.DeclaringType == typeof(AbsTestClass);

            mi = typeof(TestClass).GetMethod("PublicMethod", BindingFlags.Instance | BindingFlags.Public);
            fRes &= !mi.IsAbstract;
            fRes &= mi.IsFinal;
            fRes &= mi.IsPublic;
            fRes &= !mi.IsStatic;
            fRes &= mi.IsVirtual;
            fRes &= mi.ReturnType == typeof(void);
            mi.Invoke(tst, new object[] { });
            fRes &= mi.DeclaringType == typeof(TestClass);

            mi = typeof(TestClass).GetMethod("InternalMethod", BindingFlags.Instance | BindingFlags.NonPublic);
            fRes &= !mi.IsAbstract;
            fRes &= !mi.IsFinal;
            fRes &= !mi.IsPublic;
            fRes &= !mi.IsStatic;
            fRes &= !mi.IsVirtual;
            fRes &= mi.ReturnType == typeof(int);
            fRes &= 1 == (int)mi.Invoke(tst, new object[] { 90.3f });
            fRes &= mi.DeclaringType == typeof(TestClass);

            mi = typeof(TestClass).GetMethod("PrivateMethod", BindingFlags.Instance | BindingFlags.NonPublic);
            fRes &= !mi.IsAbstract;
            fRes &= !mi.IsFinal;
            fRes &= !mi.IsPublic;
            fRes &= !mi.IsStatic;
            fRes &= !mi.IsVirtual;
            fRes &= mi.ReturnType == typeof(float);
            fRes &= 3.3f == (float)mi.Invoke(tst, new object[] { 92 });
            fRes &= mi.DeclaringType == typeof(TestClass);

            return fRes ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults SystemReflectionMemberTests_DelegateMethod_Test1()
        {
            bool fRes = true;

            ///
            /// Test the MethodInfo returned from the Delegate.Method property
            /// 

            Delegate del = new MyDelegate(MyDelegateImpl);
            MethodInfo mi = del.Method;

            fRes &= !mi.IsPublic;
            fRes &= mi.IsStatic;
            fRes &= !mi.IsVirtual;
            fRes &= !mi.IsAbstract;
            fRes &= !mi.IsFinal;
            fRes &= mi.Name == "MyDelegateImpl";
            fRes &= mi.ReturnType == typeof(bool);
            fRes &= (bool)mi.Invoke(null, new object[] { 1, 3.3f });
            fRes &= mi.DeclaringType == typeof(SystemReflectionMemberTests);

            return fRes ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults SystemReflectionMemberTests_ConstructorInfo_Test2()
        {
            bool fRes = true;

            ///
            /// Test the ConstructorInfo class members
            /// 
            Type t = typeof(TestClass);

            ConstructorInfo ci = t.GetConstructor(new Type[] { });
            fRes &= ci.IsPublic;
            fRes &= !ci.IsStatic;
            fRes &= ci.Invoke(new object[] { }) is TestClass;

            ci = typeof(AbsTestClass).GetConstructor(new Type[] { typeof(float) });
            fRes &= !ci.IsPublic;
            fRes &= !ci.IsStatic;
            fRes &= ci.DeclaringType == typeof(AbsTestClass);
            AbsTestClass tst = ci.Invoke(new object[] { 1.2f }) as AbsTestClass;
            fRes &= tst != null;

            ci = t.GetConstructor(new Type[] { typeof(int) });
            fRes &= !ci.IsStatic;
            fRes &= !ci.IsPublic;
            fRes &= ci.Invoke(new object[] { 3 }) is TestClass;

            ci = t.GetConstructor(new Type[] { typeof(bool) });
            fRes &= !ci.IsStatic;
            fRes &= !ci.IsPublic;
            fRes &= ci.Invoke(new object[] { true }) is TestClass;

            return fRes ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults SystemReflectionMemberTests_FieldInfo_Test3()
        {
            bool fRes = true;

            ///
            /// Test the FieldInfo class members
            /// 

            Type t = typeof(TestClass);
            FieldInfo fi = t.GetField("AbsPrivateField", BindingFlags.Instance | BindingFlags.NonPublic);
            fRes &= fi.FieldType == typeof(int);
            fRes &= fi.DeclaringType == typeof(AbsTestClass);

            fi = t.GetField("PublicField");
            fRes &= fi.FieldType == typeof(int);
            fRes &= fi.DeclaringType == typeof(TestClass);

            fi = t.GetField("IntProtectedField", BindingFlags.Instance | BindingFlags.NonPublic);
            fRes &= fi.FieldType == typeof(float);
            fRes &= fi.DeclaringType == t;

            fi = t.GetField("BoolPrivateField", BindingFlags.Static | BindingFlags.NonPublic);
            fRes &= fi.FieldType == typeof(bool);
            fRes &= fi.DeclaringType == t;


            return fRes ? MFTestResults.Pass : MFTestResults.Fail;
        }

        //--------------------Classes/Interfaces/Delegates used by this test class----------------------//

        #region INTERNAL_TEST_CLASSES
        private static bool MyDelegateImpl(int i, float f)
        {
            return true;
        }

        public delegate bool MyDelegate(int i, float f);

        abstract class AbsTestClass
        {
            int AbsPrivateField;

            public static bool s_WasStaticMethodCalled;

            static AbsTestClass()
            {
                s_WasStaticMethodCalled = false;
            }
            protected AbsTestClass(float f)
            {
                AbsPrivateField = (int)f;
            }

            internal protected object BaseInternalProtectedMethod(int i)
            {
                return null;
            }

            public abstract float AbstractPublicMethod(int i);

            internal virtual int VirtualInternalMethod(bool b)
            {
                return 0;
            }

            public virtual bool SealedPublicMethod()
            {
                return false;
            }

            private static void StaticPrivateAbsMethod()
            {
                s_WasStaticMethodCalled = true;
            }
        }

        interface IBaseInterface
        {
            void PublicMethod();
        }

        private class TestClass : AbsTestClass, IBaseInterface
        {
            public int PublicField;
            internal protected float IntProtectedField;
            private static bool BoolPrivateField;

            public TestClass() : base(3.4f)
            {
                PublicField = 0;
            }

            internal TestClass(int i) : base(45.6f)
            {
                IntProtectedField = i;
            }

            private TestClass(bool b) : base(2.2f)
            {
                BoolPrivateField = b;
            }

            public void PublicMethod()
            {
            }
            public sealed override bool SealedPublicMethod()
            {
                return true;
            }

            internal int InternalMethod(float f)
            {
                return 1;
            }
            protected bool ProtectedMethod(bool b)
            {
                return true;
            }
            private float PrivateMethod(int i)
            {
                return 3.3f;
            }
            public override float AbstractPublicMethod(int i)
            {
                return 38.4f;
            }

            internal override int VirtualInternalMethod(bool b)
            {
                return 34;
            }
        }
        #endregion
    }
}