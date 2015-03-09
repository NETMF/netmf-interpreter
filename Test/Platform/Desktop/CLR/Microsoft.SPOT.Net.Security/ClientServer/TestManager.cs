using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;


namespace Microsoft.SPOT.Platform.Test
{
    class TestManager
    {
        Socket        socket            = null;
        IPAddress     ipAddress         = null;
        int           port              = 0;
        object        classOfTestsToRun = null;
        string        testToRun         = null;
        int           scenarioToRun     = 0;
        MFTestResults testResult        = MFTestResults.Fail;

        public void Initialize(IPAddress ipAddress, int port)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            socket.Blocking = true;
            socket.ReceiveTimeout = 30000;
            socket.SendTimeout = 30000;

            this.ipAddress = ipAddress;
            this.port = port;
            Console.WriteLine("Connect to IPAddress: " + this.ipAddress + " Port Number: " + port);
        }

        public void Start(object classOfTests)
        {

            //Continually run and try to make a connection to the device.
            while (true)
            {
                try
                {
                    //If we can't connect then an exception will be thrown.  Thus retrying to connect.
                    socket.Connect(ipAddress, port);
                    Console.WriteLine("Connected.");

                    int quitCounter = 0;
                    //Now we have a connection so continually receive a test to run, execute it, then report the result back.
                    while (true)
                    {
                        byte[] receiveBuffer = new byte[1000];
                        int dataAvailable = 0;

                        //Waiting for device to request a test to execute.
                        //If loose connection OR no more tests to execute this will throw an exception on Read Timeout.
                        //Thus putting us back into the Connect loop.
                        dataAvailable = socket.Receive(receiveBuffer, SocketFlags.None);

                        //Continue if we've read something.
                        if (dataAvailable > 0)
                        {
                            quitCounter = 0;
                            TestProxy tp = new TestProxy(receiveBuffer);
                            classOfTestsToRun = classOfTests;
                            testToRun = tp.testToRun;
                            scenarioToRun = tp.scenarioToRun;

                            //do this in process 
                            RunTest();

                            tp.serverTestResult = testResult;
                            socket.Send(tp.Serialize(), SocketFlags.None);
                        }
                        else
                        {
                            quitCounter++;
                            //only try to recieve 20 times then go back to try and connect.
                            if (quitCounter > 20)
                                break;
                        }
                    }
                }
                catch (SocketException e)
                {
                    //Error code 10061 is connection refused because server is not running yet.
                    //ignore this error code.
                    if (e.ErrorCode != 10061)
                    {
                        Console.WriteLine("SocketException: " + e);
                        Console.WriteLine("Error Code: " + e.ErrorCode);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e.ToString());
                }
                finally
                {
                    if (socket != null)
                    {
                        socket.Close();
                        socket = null;
                    }
                    //reinitilize and start the loop over again to reconnect to the device.
                    Initialize(ipAddress, port);
                }

                Thread.Sleep(2000);
            }
        }


        /// <summary>
        /// Locates the test to run in the classOfTestsToRun and executes that test.  Uses reflection to locate the test.
        /// </summary>
        /// <param name="classOfTestsToRun">A class that contains tests to be executed.</param>
        /// <param name="testToRun">The string name of the test to run in the classOfTestsToRun object.</param>
        /// <returns></returns>
        private void RunTest()
        {
            try
            {
                // Get all the types from the class.
                Type t = classOfTestsToRun.GetType();// System.Reflection.MethodBase.GetCurrentMethod();// FilterTypeNameIgnoreCase.getGetTypesImplementingInterface(typeof(IMFTestInterface));

                // Get all the methods and call the one that matches the return type for test methods.
                MethodInfo[] methods = t.GetMethods();
                foreach (MethodInfo method in methods)
                {
                    //Only get the methods with return type of MFtestResults and that match the name of the function we
                    //are trying to execute.
                    if ((method.ReturnType == typeof(MFTestResults)) && ((method.Name.Trim().ToLower() == testToRun.Trim().ToLower())))
                    {
                        Console.WriteLine("TEST: " + method.Name + " Starting");

                        try
                        {
                            object[] param = new object [1];
                            param[0] = (object)scenarioToRun;
                            object result = method.Invoke(classOfTestsToRun, param);
                            testResult = (MFTestResults)result;

                            if (MFTestResults.Fail == testResult)
                            {
                                Console.WriteLine("TEST: " + method.Name + " " + MFTestResults.Fail.ToString());
                            }
                            else if (MFTestResults.Pass == testResult)
                            {
                                Console.WriteLine("TEST: " + method.Name + " " + MFTestResults.Pass.ToString());
                            }
                            else if (MFTestResults.Skip == testResult)
                            {
                                Console.WriteLine("TEST: " + method.Name + " " + MFTestResults.Skip.ToString());
                            }
                            else if (MFTestResults.KnownFailure == testResult)
                            {
                                Console.WriteLine("TEST: " + method.Name + " " + MFTestResults.KnownFailure.ToString());
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Exception trying to run test:" + ex.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception trying to run test:" + ex.ToString());
            }
            finally
            {
                GC.WaitForPendingFinalizers();
            }
        }

        ~TestManager()
        {
            if (socket != null)
                socket.Close();
        }

    }
}
