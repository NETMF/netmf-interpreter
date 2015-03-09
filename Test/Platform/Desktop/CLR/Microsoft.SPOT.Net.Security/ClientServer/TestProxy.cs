using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.SPOT.Platform.Test
{
    public class TestProxy
    {
        //Test to run can be a string
        public string testToRun = "";

        //Any valid Integer Value
        public int serverTimeoutSeconds = 120;
        
        //The test result of the test being executed on the server
        public MFTestResults serverTestResult = MFTestResults.Fail;

        //Under each test case there may be a specific scenario to run. which can be specified here.
        public int scenarioToRun = 0;

        public TestProxy() { }

        public TestProxy(byte[] command)
        {
            Deserialize(command);
        }

        public void Deserialize(byte[] command)
        {
            if (command == null || command.Length < 4)
                throw new Exception("There is no data to deserialize.");

            char[] commandChars = new char[command.Length];

            String[] commands = new string[4];
            int startIndex = 0;
            int commandNumber = 0;
            for (int i = 0; i < command.Length; ++i)
            {
                if (command[i] == ' ')
                {
                    //Put the data into string format
                    commands[commandNumber] = new String(commandChars, startIndex, i - startIndex);
                    
                    commandNumber++;
                    startIndex = i + 1;
                }
                else
                    commandChars[i] = (char)command[i];
            }

            //Deserialize the data to what it really means.
            testToRun = commands[0];
            serverTimeoutSeconds = int.Parse(commands[1]);
            serverTestResult = (MFTestResults)(int.Parse(commands[2]));
            scenarioToRun = int.Parse(commands[3]);

        }

        public byte [] Serialize()
        {
            //Concatenate them together delimited by a whitespace
            string serializedData = testToRun + " " + serverTimeoutSeconds.ToString() + " " + ((int)serverTestResult).ToString() + " " + scenarioToRun.ToString() + " ";

            //String can be no longer than 1000 characters total.
            if( serializedData.Length >= 1000 )
                throw new Exception("The length of the data that is being serialized is greater than 1000 characters.  Reduce the length of the data being serialized.");

            byte [] serializedBytes = new byte [serializedData.Length];

            //serialize objects into a byte array
            for (int i = 0; i < serializedData.Length; ++i)
            {
                char c = serializedData[i];
                serializedBytes[i] = (byte)c;
            }

            return serializedBytes;
        }

        public static void ValidateTestProxy()
        {
            TestProxy tp = new TestProxy();
            tp.serverTestResult = MFTestResults.Pass;
            tp.serverTimeoutSeconds = int.MinValue;
            tp.testToRun = "T";

            TestProxy tpCopy = tp;

            byte [] serializedData = tp.Serialize();
            tp.Deserialize(serializedData);

            if ((tpCopy.serverTestResult == tp.serverTestResult)&&(tpCopy.serverTimeoutSeconds == tp.serverTimeoutSeconds)&&(tpCopy.testToRun == tp.testToRun))
                Console.WriteLine("Pass");
            else
                Console.WriteLine("Fail");

        }
    }
}
