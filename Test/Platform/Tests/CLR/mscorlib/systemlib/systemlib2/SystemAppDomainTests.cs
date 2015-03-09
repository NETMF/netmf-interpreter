////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.SPOT.Platform.Test;
using System.Threading;


namespace Microsoft.SPOT.Platform.Tests
{
    public class SystemAppDomainTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests");
            string szAssm = typeof(SystemAppDomainTests).Assembly.FullName;
            m_appDomain = AppDomain.CreateDomain(this.GetType().FullName);
            m_appDomain.Load(szAssm);
            m_mbroProxy = (MyMarshalByRefObject)m_appDomain.CreateInstanceAndUnwrap(szAssm, typeof(MyMarshalByRefObject).FullName);
            m_mbro = new MyMarshalByRefObject();
            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests");
            if (m_appDomain != null)
            {
                AppDomain.Unload(m_appDomain);

                m_appDomain = null;
                m_mbro = null;
                m_mbroProxy = null;
                Debug.GC(true);
            }
        }

        [TestMethod]
        public MFTestResults AppDomain_Test1()
        {
            /// <summary>
            ///  1. Creates an AppDomain
            ///  2. Calls SomeMethod in it
            ///  3. Unloads that domain
            ///  4. Calls SomeMethod again
            ///  21291	Calling a method in an unloaded app domain should throw an exception.
            /// </summary>

            MFTestResults testResult = MFTestResults.Pass;

            Log.Comment("Get and display the friendly name of the default AppDomain.");
            string callingDomainName = Thread.GetDomain().FriendlyName;
            if (callingDomainName != AppDomain.CurrentDomain.FriendlyName)
            {
                Log.Comment("Failure : Expected '" + AppDomain.CurrentDomain.FriendlyName +
                    "' but got '" + callingDomainName + "'");
                testResult = MFTestResults.Fail;
            }
            Log.Comment(callingDomainName);

            Type Type1 = this.GetType();
            string exeAssembly = Assembly.GetAssembly(Type1).FullName;

            Log.Comment("1. Creates an AppDomain");
            AppDomain ad2 = AppDomain.CreateDomain("AppDomain #2");
            TestMarshalByRefType mbrt = (TestMarshalByRefType)ad2.CreateInstanceAndUnwrap(
                exeAssembly,
                typeof(TestMarshalByRefType).FullName);
            Assembly asm4 = ad2.Load(ad2.GetAssemblies()[0].FullName);

            if (asm4.FullName != ad2.GetAssemblies()[0].FullName)
            {
                testResult = MFTestResults.Fail;
            }

            Log.Comment("2. Calls SomeMethod in it");
            mbrt.SomeMethod(callingDomainName);

            Log.Comment("3. Unloads that domain:");
            AppDomain.Unload(ad2);
            try
            {
                Log.Comment("4. calls SomeMethod again");
                mbrt.SomeMethod(callingDomainName);
                Log.Comment("Sucessful call.");
                testResult = MFTestResults.Fail;
            }
            catch (AppDomainUnloadedException e)
            {
                Log.Comment("Specific Catch; this is expected. " + e.Message);
            }
            catch (Exception e)
            {
                Log.Comment("Generic Catch; this is bad. " + e.Message);
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults AppDomain_Test2()
        {
            MFTestResults testResult = MFTestResults.Pass;

            try
            {
                Log.Comment("Get and display the friendly name of the default AppDomain.");
                string callingDomainName = Thread.GetDomain().FriendlyName;

                Type Type1 = this.GetType();
                string exeAssembly = Assembly.GetAssembly(Type1).FullName;
                Log.Comment("1. Creates an AppDomain");
                AppDomain ad2 = AppDomain.CreateDomain("AppDomain #3");
                TestMarshalByRefType mbrt = (TestMarshalByRefType)ad2.CreateInstanceAndUnwrap(
                    exeAssembly,
                    typeof(TestMarshalByRefType).FullName);
                ad2.Load(ad2.GetAssemblies()[0].FullName);

                Log.Comment("2. Calls SomeMethod in it");
                mbrt.SomeMethod(callingDomainName);

                Log.Comment("3. Unloads that domain:");
                AppDomain.Unload(ad2);

                Log.Comment("4. Reload AppDomain with same friendly name");
                ad2 = AppDomain.CreateDomain("AppDomain #3");
                mbrt = (TestMarshalByRefType)ad2.CreateInstanceAndUnwrap(
                    exeAssembly,
                    typeof(TestMarshalByRefType).FullName);
                ad2.Load(ad2.GetAssemblies()[0].FullName);

                Log.Comment("5. Calls SomeMethod in it");
                mbrt.SomeMethod(callingDomainName);

                Log.Comment("6. Unloads that domain:");
                AppDomain.Unload(ad2);
            }
            catch (Exception e)
            {
                Log.Comment("Generic Catch; this is bad. " + e.Message);
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }



        [TestMethod]
        public MFTestResults MarshalInt_Test2()
        {
            MFTestResults testResult = MFTestResults.Pass;
            int i = 123;
            int result = m_mbroProxy.MarhsalInt(i);
            if (result != i)
            {
                Log.Comment("Failure : Expected '" + i + "' but got '" + result + "'");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults MarshalIntByRef_Test3()
        {
            MFTestResults testResult = MFTestResults.Pass;
            int i = 123;
            int i2 = 456;
            m_mbroProxy.MarshalIntByRef(ref i, i2);
            if (i != i2)
            {
                Log.Comment("Failure : Expected '" + i2 + "' but got '" + i + "'");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults MarshalIntArrayByRef_Test4()
        {
            MFTestResults testResult = MFTestResults.Pass;
            int[] i = new int[] { 123 };
            int i2 = 456;
            m_mbroProxy.MarshalIntByRef(ref i[0], i2);
            if (i[0] != i2)
            {
                Log.Comment("Failure : Expected '" + i2 + "' but got '" + i[0] + "'");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults MarshalDouble_Test5()
        {
            MFTestResults testResult = MFTestResults.Pass;
            double d = 123.456;
            double d2 = m_mbroProxy.MarhsalDouble(d);
            if (d2 != d)
            {
                Log.Comment("Failure : Expected '" + d + "' but got '" + d2 + "'");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults MarshalDoubleByRef_Test6()
        {
            MFTestResults testResult = MFTestResults.Pass;
            double d = 123.0;
            double d2 = 456.0;
            m_mbroProxy.MarshalDoubleByRef(ref d, d2);
            if (d != d2)
            {
                Log.Comment("Failure : Expected '" + d2 + "' but got '" + d + "'");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults MarshalDateTime_Test7()
        {
            MFTestResults testResult = MFTestResults.Pass;
            DateTime dt = new DateTime(1976, 3, 4);
            DateTime dt2 = m_mbroProxy.MarshalDateTime(dt);
            if (dt != dt2)
            {
                Log.Comment("Failure : Expected '" + dt2 + "' but got '" + dt + "'");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults MarshalDateTimeByRef_Test8()
        {
            MFTestResults testResult = MFTestResults.Pass;
            DateTime dt = new DateTime(1976, 3, 4);
            DateTime dt2 = new DateTime(1976, 3, 10);
            m_mbroProxy.MarshalDateTimeByRef(ref dt, dt2);
            if (dt != dt2)
            {
                Log.Comment("Failure : Expected '" + dt2 + "' but got '" + dt + "'");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults MarshalString_Test9()
        {
            MFTestResults testResult = MFTestResults.Pass;
            string s = "hello";
            string s2 = m_mbroProxy.MarshalString(s);
            if (s != s2)
            {
                Log.Comment("Failure : Expected '" + s + "' but got '" + s2 + "'");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults MarshalStringByRef_Test10()
        {
            MFTestResults testResult = MFTestResults.Pass;
            string s = "hello";
            string s2 = "goodbye";
            m_mbroProxy.MarshalStringByRef(ref s, s2);
            if (s != s2)
            {
                Log.Comment("Failure : Expected '" + s2 + "' but got '" + s + "'");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults TestNull_Test11()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (!m_mbroProxy.MarshalNull(null))
            {
                Log.Comment("Failure : Marshalling a null obj. is not equal to null");
                testResult = MFTestResults.Fail;
            }
            return testResult;
        }

        [TestMethod]
        public MFTestResults TestThrow_Test12()
        {
            string message = "message";
            MFTestResults testResult = MFTestResults.Fail;

            try
            {
                m_mbroProxy.Throw(message);
                Log.Comment("Failure to throw Exception");
            }
            catch (Exception e)
            {
                if (e.Message == message)
                {
                    testResult = MFTestResults.Pass;
                }
                else
                {
                    Log.Comment("Failure : Expected message '" + message + "' but got '" + e.Message + "'");
                }
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults TestNonSerializableClass_Test13()
        {
            MFTestResults testResult = MFTestResults.Fail;

            try
            {
                m_mbroProxy.TestNonSerializableClass(new NonSerializableClass());
                Log.Comment("Failure to throw Exception");
            }
            catch (Exception)
            {
                testResult = MFTestResults.Pass;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults TestProxyEquality_Test14()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (!m_mbroProxy.ProxyEquality(m_mbro, m_mbro))
            {
                Log.Comment("Failure : Marshalling the same obj. and comparing failed");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults TestProxyDelegate_Test15()
        {
            MyMarshalByRefObject.PrintDelegate dlg = new MyMarshalByRefObject.PrintDelegate(m_mbroProxy.Print);
            dlg("Hello world");

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults TestProxyMulticastDelegate_Test16()
        {
            MyMarshalByRefObject.PrintDelegate dlg = null;

            dlg = (MyMarshalByRefObject.PrintDelegate)Microsoft.SPOT.WeakDelegate.Combine(dlg, new MyMarshalByRefObject.PrintDelegate(m_mbroProxy.Print));
            dlg = (MyMarshalByRefObject.PrintDelegate)Microsoft.SPOT.WeakDelegate.Combine(dlg, new MyMarshalByRefObject.PrintDelegate(m_mbroProxy.Print));

            dlg("Goodnight moon");

            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults MarshalMBRO_Test17()
        {
            MFTestResults testResult = MFTestResults.Pass;
            MyMarshalByRefObject mbro = m_mbroProxy.MarshalMBRO(m_mbro);
            if (mbro != m_mbro)
            {
                Log.Comment("Failure : Expected '" + m_mbro + "' but got '" + mbro + "'");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults MarshalMBROByRef_Test18()
        {
            MFTestResults testResult = MFTestResults.Pass;
            MyMarshalByRefObject mbro = new MyMarshalByRefObject();
            MyMarshalByRefObject mbro2 = new MyMarshalByRefObject();

            m_mbroProxy.MarshalMBROByRef(ref mbro, mbro2);
            if(!Object.ReferenceEquals(mbro, mbro2))
            {
            Log.Comment("Failure : Marshalling Obj. by Reference and comparing Object.ReferenceEquals(mbro, mbro2) failed");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults MarshalClass_Test19()
        {          
            ClassToMarshal c = new ClassToMarshal(123, "hello");          

            return EqualsButNotSameInstance(c, m_mbroProxy.MarshalClass(c)) ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults MarshalClassByRef_Test20()
        {          
            ClassToMarshal c = new ClassToMarshal(123, "hello");
            ClassToMarshal c2 = new ClassToMarshal(456, "goodbye");
            m_mbroProxy.MarshalClassByRef(ref c, c2);           

            return this.EqualsButNotSameInstance(c, c2) ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults MarshalClassArrayByRef_Test21()
        {
            ClassToMarshal[] c = new ClassToMarshal[] { new ClassToMarshal(123, "hello") };
            ClassToMarshal c2 = new ClassToMarshal(456, "goodbye");

            m_mbroProxy.MarshalClassByRef(ref c[0], c2);

            return this.EqualsButNotSameInstance(c[0], c2) ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults FieldAccess_Test22()
        {
            MFTestResults testResult = MFTestResults.Pass;
            int i = 123;
            string s = "hello";

            m_mbroProxy.m_int = i;
            m_mbroProxy.m_string = s;

            if (m_mbroProxy.m_int != i) 
            {
                Log.Comment("Failure : int acces, expected '" + i + "' but got '" + m_mbroProxy.m_int + "'");
                testResult = MFTestResults.Fail;
            }
            if (m_mbroProxy.m_string != s)
            {
                Log.Comment("Failure : string acces, expected '" + s + "' but got '" + m_mbroProxy.m_string + "'");
                testResult = MFTestResults.Fail;
            }

            i = 456;
            s = "goodbye";

            FieldInfo fi = m_mbroProxy.GetType().GetField("m_int", BindingFlags.Instance | BindingFlags.Public);
            fi.SetValue(m_mbroProxy, i);

            if ((int)fi.GetValue(m_mbroProxy) != i)
            {
                Log.Comment("Failure : Reflection int , expected '" + i + "' but got '" + (int)fi.GetValue(m_mbroProxy) + "'");
                testResult = MFTestResults.Fail;
            }

            fi = m_mbroProxy.GetType().GetField("m_string", BindingFlags.Instance | BindingFlags.Public);

            fi.SetValue(m_mbroProxy, s);

            if ((string)fi.GetValue(m_mbroProxy) != s)
            {
                Log.Comment("Failure : Reflection string , expected '" + s + "' but got '" + (string)fi.GetValue(m_mbroProxy) + "'");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults StartThread_Test23()
        {
            m_mbroProxy.StartThread();
           
            return MFTestResults.Pass;
        }

        [TestMethod]
        public MFTestResults MarshalType_Test24()
        {
            MFTestResults testResult = MFTestResults.Pass;
            Type s = typeof(int);
            Type s2 = typeof(bool);
            s2 = m_mbroProxy.MarshalType(s);
            if (s != s2)
            {
                Log.Comment("Failure : Expected '" + s2 + "' but got '" + s + "'");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults MarshalTypeByRef_Test25()
        {
            MFTestResults testResult = MFTestResults.Pass;
            Type s = typeof(int);
            Type s2 = typeof(bool);
            m_mbroProxy.MarshalTypeByRef(ref s, s2);
            if (s != s2)
            {
                Log.Comment("Failure : Expected '" + s2 + "' but got '" + s + "'");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults MarshalTypeArray_Test26()
        {
            MFTestResults testResult = MFTestResults.Pass;
            Type[] s = new Type[] { typeof(int), typeof(bool) };
            Type[] s2 = null;
            s2 = m_mbroProxy.MarshalTypeArray(s);
            if (s2 == null || s2[0] != s[0] || s2[1] != s[1])
            {
                Log.Comment("Failure : Expected '" + s2 + "' but got '" + s + "'");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults MarshalAssembly_Test27()
        {
            MFTestResults testResult = MFTestResults.Pass;
            Assembly s = typeof(int).Assembly;
            Assembly s2 = typeof(MFTestResults).Assembly;
            s2 = m_mbroProxy.MarshalAssembly(s);
            if (s.FullName != s2.FullName)
            {
                Log.Comment("Failure : Expected '" + s2.FullName + "' but got '" + s.FullName + "'");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults MarshalAssemblyByRef_Test28()
        {
            MFTestResults testResult = MFTestResults.Pass;
            Assembly s = typeof(int).Assembly;
            Assembly s2 = typeof(MFTestResults).Assembly;
            m_mbroProxy.MarshalAssemblyByRef(ref s, s2);
            if (s.FullName != s2.FullName)
            {
                Log.Comment("Failure : Expected '" + s2.FullName + "' but got '" + s.FullName + "'");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults MarshalFieldInfo_Test29()
        {
            MFTestResults testResult = MFTestResults.Pass;
            FieldInfo s = typeof(MyMarshalByRefObject).GetField("m_int");
            FieldInfo s2 = typeof(MyMarshalByRefObject).GetFields()[1];
            s2 = m_mbroProxy.MarshalFieldInfo(s);
            if (s.Name != s2.Name)
            {
                Log.Comment("Failure : Expected '" + s2.Name + "' but got '" + s.Name + "'");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults MarshalFieldInfoByRef_Test30()
        {
            MFTestResults testResult = MFTestResults.Pass;
            FieldInfo s = typeof(MyMarshalByRefObject).GetField("m_int");
            FieldInfo s2 = typeof(MyMarshalByRefObject).GetFields()[1];
            m_mbroProxy.MarshalFieldInfoByRef(ref s, s2);
            if (s.Name != s2.Name)
            {
                Log.Comment("Failure : Expected '" + s2.Name + "' but got '" + s.Name + "'");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults MarshalMethodInfo_Test31()
        {
            MFTestResults testResult = MFTestResults.Pass;
            MethodInfo s = typeof(MyMarshalByRefObject).GetMethod("Print");
            MethodInfo s2 = typeof(Array).GetMethods()[0];
            s2 = m_mbroProxy.MarshalMethodInfo(s);
            if (s.Name != s2.Name)
            {
                Log.Comment("Failure : Expected '" + s2.Name + "' but got '" + s.Name + "'");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults MarshalMethodInfoByRef_Test32()
        {
            MFTestResults testResult = MFTestResults.Pass;
            MethodInfo s = typeof(MyMarshalByRefObject).GetMethod("Print");
            MethodInfo s2 = typeof(Array).GetMethods()[0];
            m_mbroProxy.MarshalMethodInfoByRef(ref s, s2);
            if (s.Name != s2.Name)
            {
                Log.Comment("Failure : Expected '" + s2.Name + "' but got '" + s.Name + "'");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults MarshalConstructorInfo_Test33()
        {
            MFTestResults testResult = MFTestResults.Pass;
            ConstructorInfo s = typeof(MyMarshalByRefObject).GetConstructor(new Type[] {});
            ConstructorInfo s2 = typeof(Array).GetConstructor(new Type[] { });
            s2 = m_mbroProxy.MarshalConstructorInfo(s);
            if (s.Name != s2.Name)
            {
                Log.Comment("Failure : Expected '" + s2.Name + "' but got '" + s.Name + "'");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults MarshalMethodInfoByRef_Test34()
        {
            MFTestResults testResult = MFTestResults.Pass;
            ConstructorInfo s = typeof(MyMarshalByRefObject).GetConstructor(new Type[] { });
            ConstructorInfo s2 = typeof(Array).GetConstructor(new Type[] { });
            m_mbroProxy.MarshalConstructorInfoByRef(ref s, s2);
            if (s.Name != s2.Name)
            {
                Log.Comment("Failure : Expected '" + s2.Name + "' but got '" + s.Name + "'");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }



        #region Variables

        protected AppDomain m_appDomain;
        protected MyMarshalByRefObject m_mbroProxy;
        protected MyMarshalByRefObject m_mbro;

        #endregion Variables

        #region HelperMethods

        private bool EqualsButNotSameInstance(object obj1, object obj2)
        {
            bool bResult = true;
            if (!Object.Equals(obj1, obj2))
            {
                Log.Comment("Failure : Object.Equals(obj1, obj2) returned false");
                bResult = false;
            }
            else if (Object.ReferenceEquals(obj1, obj2))
            {
                Log.Comment("Failure : Object.ReferenceEquals(obj1, obj2) returned true");
                bResult = false;
            }

            return bResult;
        }

        #endregion HelperMethods

        #region HelperClasses

        public class TestMarshalByRefType : MarshalByRefObject
        {
            /// <summary>
            ///  1. This method is called via a proxy.
            ///  2. Display the name of the calling AppDomain and the name of the second domain
            /// </summary>
            public void SomeMethod(string callingDomainName)
            {
                Log.Comment("Calling from  " + callingDomainName + " to "
                    + Thread.GetDomain().FriendlyName);
            }
        }

        public class MyMarshalByRefObject : MarshalByRefObject
        {
            public int m_int;
            public string m_string;

            public delegate void PrintDelegate(string text);

            public void Print(string text)
            {
                Debug.Print(text);

                if (System.Runtime.Remoting.RemotingServices.IsTransparentProxy(this))
                {
                    Debug.Print("FAILURE: METHOD CALL ON PROXY OBJECT ");
                }
            }

            public int MarhsalInt(int i)
            {
                return i;
            }

            public void MarshalIntByRef(ref int i, int i2)
            {
                i = i2;
            }

            public double MarhsalDouble(double d)
            {
                return d;
            }

            public void MarshalDoubleByRef(ref double d, double d2)
            {
                d = d2;
            }

            public string MarshalString(string s)
            {
                return s;
            }

            public void MarshalStringByRef(ref string s, string s2)
            {
                s = s2;
            }

            public DateTime MarshalDateTime(DateTime dt)
            {
                return dt;
            }

            public void MarshalDateTimeByRef(ref DateTime dt, DateTime dt2)
            {
                dt = dt2;
            }

            public void Throw(string message)
            {
                throw new Exception(message);
            }

            public bool MarshalNull(object o)
            {
                return o == null;
            }

            public ClassToMarshal MarshalClass(ClassToMarshal c)
            {
                return c;
            }

            public void MarshalClassByRef(ref ClassToMarshal c, ClassToMarshal c2)
            {
                c = c2;
            }

            public void TestNonSerializableClass(NonSerializableClass c)
            {
            }

            public bool ProxyEquality(MyMarshalByRefObject mbro1, MyMarshalByRefObject mbro2)
            {
                return mbro1 == mbro2;
            }

            public void MarshalDyingProxy(MyMarshalByRefObject mbro)
            {
            }

            public void MarshalDeadProxy(MyMarshalByRefObject mbro)
            {
            }

            public MyMarshalByRefObject MarshalMBRO(MyMarshalByRefObject mbro)
            {
                return mbro;
            }

            public void MarshalMBROByRef(ref MyMarshalByRefObject mbro, MyMarshalByRefObject mbro2)
            {
                mbro = mbro2;
            }

            public Type MarshalType(Type t1)
            {
                return t1;
            }

            public void MarshalTypeByRef(ref Type t1, Type t2)
            {
                t1 = t2;
            }
            public Type[] MarshalTypeArray(Type[] t1)
            {
                return t1;
            }
            public void MarshalTypeArrayByRef(ref Type[] t1, Type[] t2)
            {
                Array.Copy(t1, t2, t1.Length);
            }

            public Assembly MarshalAssembly(Assembly a1)
            {
                return a1;
            }

            public void MarshalAssemblyByRef(ref Assembly a1, Assembly a2)
            {
                a1 = a2;
            }

            public FieldInfo MarshalFieldInfo(FieldInfo a1)
            {
                return a1;
            }

            public void MarshalFieldInfoByRef(ref FieldInfo a1, FieldInfo a2)
            {
                a1 = a2;
            }

            public MethodInfo MarshalMethodInfo(MethodInfo a1)
            {
                return a1;
            }

            public void MarshalMethodInfoByRef(ref MethodInfo a1, MethodInfo a2)
            {
                a1 = a2;
            }

            public ConstructorInfo MarshalConstructorInfo(ConstructorInfo a1)
            {
                return a1;
            }

            public void MarshalConstructorInfoByRef(ref ConstructorInfo a1, ConstructorInfo a2)
            {
                a1 = a2;
            }


            public MyMarshalByRefObject CreateMBRO()
            {
                return new MyMarshalByRefObject();
            }


            public void StartThread()
            {
                Thread th = new Thread(new ThreadStart(ThreadWorker));
                th.Start();
            }

            private void ThreadWorker()
            {
                try
                {
                    while (true) ;
                }
                catch (Exception)
                {
                    Debug.Print("ThreadWorker being aborted..");
                }
            }
        }

        [Serializable]
        public class ClassToMarshal
        {
            public int m_int;
            public string m_string;

            public ClassToMarshal(int i, string s)
            {
                m_int = i;
                m_string = s;
            }

            public override bool Equals(object obj)
            {
                ClassToMarshal cls = obj as ClassToMarshal;

                if (cls == null) return false;
                if (cls.m_int != m_int) return false;
                if (cls.m_string != m_string) return false;

                return true;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        public class NonSerializableClass
        {
        }

        #endregion HelperClasses
    }
}
