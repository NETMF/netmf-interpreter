using System;
using System.Collections;
using System.Threading;

namespace Microsoft.SPOT.UnitTest
{
    public interface IRunnableTest
    {
        void Declare();
        void Run    ();
    }
    
    public abstract class Test 
    {
        public string Name;
        public string Comment; 
        
        protected Test( string comment )
        {
            Name    = this.ToString(); 
            Comment = comment; 
        }
        
        protected Test()
        {
            Name    = this.ToString(); 
            Comment = "N/A"; 
        }

        protected internal void UnexpectedException( Exception e )
        {
            Debug.Print( "Test        : " + Name   + "caused an unexpected exception" );
            Debug.Print( "Test Comment: " + Comment                                   );
            Debug.Print( "Exception Message: "                                        );                                          
            Debug.Print( e.Message                                                    );
            Debug.Print( "Exception Stack Trace: "                                    );
            Debug.Print( e.StackTrace                                                 );   
        }

        protected internal void UnexpectedBehavior()
        {
            Debug.Print( "Test        : " + Name   + "was not supposed to execute this code" );
            Debug.Print( "Test Comment: " + Comment                                          );
        }
    }

    public abstract class TestBase : Test, IRunnableTest
    {
        public virtual void Declare()
        {
            Debug.Print( "Test         : " + Name );
        }

        public virtual void Run()
        {
            try
            {
                RunCore();
            }
            catch(Exception e)
            {                
                UnexpectedException( e );
            }
        }

        public virtual void RunCore()
        {
        }

    }

    public class TestsSuite
    {
        protected ArrayList m_tests; 
        
        protected TestsSuite()
        {
            m_tests = new ArrayList();
        }

        protected void AddTest(IRunnableTest test)
        {
            m_tests.Add( test );
        }

        public void ExecuteAll()
        {
            for (int i = 0; i < m_tests.Count; i++)
            {
                try
                {
                    Debug.Print("Executing test: " + ((Test)m_tests[i]).Name);

                    ((IRunnableTest)m_tests[i]).Declare();

                    ((IRunnableTest)m_tests[i]).Run();
                }
                catch (Exception e)
                {
                    Debug.Print("Unexpected Exception" + e.StackTrace);
                }
            }
        }

        public void Execute(int testIndex)
        {
            Debug.Print( "Executing test: " + ((Test)m_tests[ testIndex ]).Name );
                    
            ((IRunnableTest)m_tests[ testIndex ]).Declare(); 
                        
            ((IRunnableTest)m_tests[ testIndex ]).Run(); 
        }
    }
}
