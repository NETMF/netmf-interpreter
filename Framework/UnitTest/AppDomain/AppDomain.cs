using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.UnitTest;
using System.Reflection;

namespace Microsoft.SPOT.UnitTest.AppDomainTest
{	
    public class AppDomainBvtSuite : TestsSuite
    {
        public AppDomainBvtSuite()
        {            
            AddTest( new AppDomainTest() );
        }
    }
    
    public class TestDriver
    {
        public static void Call2()
        {                        
            System.Diagnostics.Debugger.Break();
        }
        public static void Call1()
        {
            Call2();
            Debug.Print( ".." );
        }

        public static void Main(string[] args)
        {
            Call1();

            TestsSuite suite = new AppDomainBvtSuite();

            Debug.Print( "Debugger? " + System.Diagnostics.Debugger.IsAttached.ToString() );
            System.Diagnostics.Debugger.Break();

            while(true)
            {
                suite.ExecuteAll(); 

                Thread.Sleep( 2000 );
            }
        }
    }

    [Serializable]
    public class ClassToMarshal
    {
        public int m_int;
        public string m_string;

        public ClassToMarshal( int i, string s )
        {
            m_int = i;
            m_string = s;
        }

        public override bool Equals( object obj )
        {
            ClassToMarshal cls = obj as ClassToMarshal;

            if(cls == null) return false;
            if(cls.m_int != m_int) return false;
            if(cls.m_string != m_string) return false;

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    [Serializable]
    public class MyMarshalByRefObjectHolder
    {
        public MyMarshalByRefObject m_mymbro;
        public MyMarshalByRefObject m_proxy;

        public MyMarshalByRefObjectHolder( MyMarshalByRefObject mymbro, MyMarshalByRefObject proxy )
        {
            m_mymbro = mymbro;
            m_proxy  = proxy;
        }
    }

    public class NonSerializableClass
    {
    }

    public class MyMarshalByRefObject : MarshalByRefObject
    {
        public int m_int;
        public string m_string;        

        public delegate void PrintDelegate( string text );

        public void Print( string text )
        {
            Debug.Print( text );

            if(System.Runtime.Remoting.RemotingServices.IsTransparentProxy( this ))
            {
                Debug.Print( "FAILURE: METHOD CALL ON PROXY OBJECT " );
            }
        }

        public int MarhsalInt( int i )
        {
            return i;            
        }

        public void MarshalIntByRef( ref int i, int i2 )
        {
            i = i2;
        }

        public double MarhsalDouble( double d )
        {
            return d;
        }
                
        public void MarshalDoubleByRef( ref double d, double d2 )
        {
            d = d2;
        }

        public string MarshalString( string s )
        {
            return s;
        }
                
        public void MarshalStringByRef( ref string s, string s2 )
        {
            s = s2;
        }
                
        public DateTime MarshalDateTime( DateTime dt )
        {
            return dt;
        }
                
        public void MarshalDateTimeByRef( ref DateTime dt, DateTime dt2 )
        {
            dt = dt2;
        }
                           
        public void Throw(string message)
        {
            throw new Exception( message);
        }

        public bool MarshalNull(object o)
        {
            return o == null;
        }
                
        public ClassToMarshal MarshalClass( ClassToMarshal c )
        {
            return c;
        }

        public void MarshalClassByRef( ref ClassToMarshal c, ClassToMarshal c2 )
        {
            c = c2;
        }
        
        public void TestNonSerializableClass( NonSerializableClass c )
        {
        }

        public bool ProxyEquality( MyMarshalByRefObject mbro1, MyMarshalByRefObject mbro2)
        {
            return mbro1 == mbro2;
        }               

        public void MarshalDyingProxy( MyMarshalByRefObject mbro )
        {
        }

        public void MarshalDeadProxy( MyMarshalByRefObject mbro )
        {
        }

        public MyMarshalByRefObject MarshalMBRO( MyMarshalByRefObject mbro )
        {
            return mbro;
        }        

        public void MarshalMBROByRef( ref MyMarshalByRefObject mbro,  MyMarshalByRefObject mbro2)
        {
            mbro = mbro2;
        }        

        public MyMarshalByRefObject CreateMBRO()
        {
            return new MyMarshalByRefObject();
        }

        public void StartThread()
        {
            Thread th = new Thread( new ThreadStart( ThreadWorker ) );
            th.Start();
        }

        private void ThreadWorker()
        {
            try
            {
                while(true) ;
            }
            catch(Exception)
            {
                Debug.Print( "ThreadWorker being aborted.." );
            }
        }
    }

    public class AppDomainTest : TestBase
    {
        protected AppDomain m_appDomain;
        protected MyMarshalByRefObject m_mbroProxy;
        protected MyMarshalByRefObject m_mbro;

        public delegate bool TestDelegate();
        
        public override void RunCore()
        {
            try
            {
                Initialize();
                RunInner();
            }
            finally
            {
                Uninitialize();
            }
        }

        private void Initialize()
        {
            string szAssm = typeof( AppDomainTest ).Assembly.FullName;
            m_appDomain = AppDomain.CreateDomain( this.GetType().FullName );
            m_appDomain.Load( szAssm );
            m_mbroProxy = (MyMarshalByRefObject)m_appDomain.CreateInstanceAndUnwrap( szAssm, typeof( MyMarshalByRefObject ).FullName );
            m_mbro = new MyMarshalByRefObject();
        }

        private void Uninitialize()
        {                        
            if(m_appDomain != null)
            {
                AppDomain.Unload( m_appDomain );

                m_appDomain = null;
                m_mbro      = null;
                m_mbroProxy = null;
                Debug.GC( true );
            }
        }

        protected virtual void RunInner()
        {
            TestDelegate[] tests = GetTests();
    
            bool fSuccessAll = true;

            if(tests != null)
            {
                for (int i = 0; i < tests.Length; i++)
                {
                    bool fSuccess = tests[i]();

                    Debug.Print("Test: " + tests[i].Method.Name + " " + (fSuccess ? "succeeded" : "FAILED"));

                    if (!fSuccess)
                    {
                        fSuccessAll = false;
                    }
                }

                if(!fSuccessAll)
                {
                    throw new Exception("Failed");
                }
            }
        }

        private TestDelegate[] GetTests()
        {                        
            return new TestDelegate[] {
                new TestDelegate(MarshalInt),
                new TestDelegate(MarshalIntByRef),
                new TestDelegate(MarshalIntArrayByRef),
                new TestDelegate(MarshalDouble),
                new TestDelegate(MarshalDoubleByRef),
                new TestDelegate(MarshalString),
                new TestDelegate(MarshalDateTime),                          
                new TestDelegate(MarshalDateTimeByRef),
                new TestDelegate(MarshalMBRO),
                new TestDelegate(MarshalMBROByRef),
                new TestDelegate(MarshalClass),
                new TestDelegate(MarshalClassByRef),
                new TestDelegate(MarshalClassArrayByRef),
                new TestDelegate(FieldAccess),
                new TestDelegate(TestNull),
                new TestDelegate(TestThrow),   
                new TestDelegate(TestNonSerializableClass),   
                new TestDelegate(TestProxyEquality),
                new TestDelegate(TestProxyDelegate),
                new TestDelegate(TestProxyMulticastDelegate),
                new TestDelegate(StartThread),
            };
        }

        private bool EqualsButNotSameInstance( object obj1, object obj2 )
        {
            return Object.Equals( obj1, obj2 ) && !Object.ReferenceEquals( obj1, obj2 );
        }

        private bool MarshalInt()
        {
            int i = 123;

            return i == m_mbroProxy.MarhsalInt( i );
        }

        private bool MarshalIntByRef()
        {
            int i = 123;
            int i2 = 456;

            m_mbroProxy.MarshalIntByRef( ref i, i2 );

            return i == i2;
        }
                
        private bool MarshalIntArrayByRef()
        {
            int[] i = new int[] { 123 };            
            int i2 = 456;

            m_mbroProxy.MarshalIntByRef( ref i[0], i2 );

            return i[0] == i2;
        }

        private bool MarshalDouble()
        {
            double d = 123.456;

            return d == m_mbroProxy.MarhsalDouble( d );
        }

        private bool MarshalDoubleByRef()
        {
            double d = 123.0;
            double d2 = 456.0;

            m_mbroProxy.MarshalDoubleByRef( ref d, d2 );

            return d == d2;
        }
                
        private bool MarshalDateTime()
        {
            DateTime dt = new DateTime( 1976, 3, 4 );

            return dt == m_mbroProxy.MarshalDateTime( dt );
        }
                
        private bool MarshalDateTimeByRef()
        {
            DateTime dt = new DateTime( 1976, 3, 4 );
            DateTime dt2 = new DateTime( 1976, 3, 10 );

            m_mbroProxy.MarshalDateTimeByRef( ref dt, dt2 );

            return dt == dt2;
        }

        private bool MarshalString()
        {
            string s = "hello";

            return s == m_mbroProxy.MarshalString( s );
        }

        private bool MarshalStringByRef()
        {
            string s = "hello";
            string s2 = "goodbye";

            m_mbroProxy.MarshalStringByRef(ref s, s2);

            return s == s2;
        }

        private bool TestNull()
        {
            return m_mbroProxy.MarshalNull( null );
        }

        private bool TestThrow()
        {
            string message = "message";
            bool fSuccess = false;

            try
            {
                m_mbroProxy.Throw( message );
            }
            catch(Exception e)
            {
                fSuccess = e.Message == message;
            }

            return fSuccess;
        }

        private bool TestNonSerializableClass()
        {
            bool fSuccess = false;

            try
            {
                m_mbroProxy.TestNonSerializableClass( new NonSerializableClass() );
            }
            catch(Exception)
            {
                fSuccess = true;
            }

            return fSuccess;
        }

        private bool TestProxyEquality()
        {
            return m_mbroProxy.ProxyEquality( m_mbro, m_mbro );
        }

        private bool TestProxyDelegate()
        {
            MyMarshalByRefObject.PrintDelegate dlg = new MyMarshalByRefObject.PrintDelegate( m_mbroProxy.Print );
            dlg( "Hello world" );

            return true;
        }

        private bool TestProxyMulticastDelegate()
        {
            MyMarshalByRefObject.PrintDelegate dlg = null;

            dlg = (MyMarshalByRefObject.PrintDelegate)Microsoft.SPOT.WeakDelegate.Combine( dlg, new MyMarshalByRefObject.PrintDelegate( m_mbroProxy.Print ) );
            dlg = (MyMarshalByRefObject.PrintDelegate)Microsoft.SPOT.WeakDelegate.Combine( dlg, new MyMarshalByRefObject.PrintDelegate( m_mbroProxy.Print ) );
            
            dlg( "Goodnight moon" );

            return true;
        }

        private bool MarshalMBRO()
        {
            MyMarshalByRefObject mbro = m_mbroProxy.MarshalMBRO( m_mbro );

            return mbro == m_mbro;
        }

        private bool MarshalMBROByRef()
        {
            MyMarshalByRefObject mbro = new MyMarshalByRefObject();
            MyMarshalByRefObject mbro2 = new MyMarshalByRefObject();

            m_mbroProxy.MarshalMBROByRef( ref mbro, mbro2 );

            return Object.ReferenceEquals( mbro, mbro2 );            
        }

        private bool MarshalClass()
        {
            ClassToMarshal c = new ClassToMarshal( 123, "hello" );

            return EqualsButNotSameInstance( c, m_mbroProxy.MarshalClass( c ) );
        }

        private bool MarshalClassByRef()
        {
            ClassToMarshal c = new ClassToMarshal( 123, "hello" );
            ClassToMarshal c2 = new ClassToMarshal( 456, "goodbye" );

            m_mbroProxy.MarshalClassByRef( ref c, c2 );

            return this.EqualsButNotSameInstance( c, c2 );
        }

        private bool MarshalClassArrayByRef()
        {                        
            ClassToMarshal[] c = new ClassToMarshal[] {new ClassToMarshal( 123, "hello" )};
            ClassToMarshal c2 = new ClassToMarshal( 456, "goodbye" );
            
            m_mbroProxy.MarshalClassByRef( ref c[0], c2 );

            return this.EqualsButNotSameInstance( c[0], c2 );
        }

        private bool FieldAccess()
        {
            int i = 123;
            string s = "hello";

            m_mbroProxy.m_int = i;            
            m_mbroProxy.m_string = s;
                        
            if(m_mbroProxy.m_int != i) return false;
            if(m_mbroProxy.m_string != s) return false;

            i = 456;
            s = "goodbye";
                        
            FieldInfo fi = m_mbroProxy.GetType().GetField( "m_int", BindingFlags.Instance | BindingFlags.Public );
            fi.SetValue( m_mbroProxy, i );

            if((int)fi.GetValue( m_mbroProxy ) != i) return false;

            fi = m_mbroProxy.GetType().GetField( "m_string", BindingFlags.Instance | BindingFlags.Public );

            fi.SetValue( m_mbroProxy, s );

            if((string)fi.GetValue( m_mbroProxy ) != s) return false;

            return true;
        }

        private bool StartThread()
        {
            m_mbroProxy.StartThread();

            return true;
        }
    }

    public enum BitmapResources : int
    {
        None = 0
    }

    public enum FontResources : int
    {   
        None = 0
    }
}
