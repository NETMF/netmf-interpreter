using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using TestFW;
using TestFW.Logging;
using TestFW.Utility;

/// <summary>
///   This is a generic hashing test.  It takes a hash test spec -- an XML file that defines
///   a set of hash methods, as well as a set of inputs and expected outputs.
/// </summary>
public class HashDriver
{
    public const int PASS_CODE = 100;
    public const int FAIL_CODE = 101;

    private static void Usage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("HashDriver testfile.xml");
        return;
    }
 
    public static int Main(string[] args)
    {
        // parse the command line
        CommandLineArguments cmdLine = CommandLineParser.Parse(new string[] {}, args);
        if(cmdLine.GetArguments().Length != 1)
        {
            Usage();
            return FAIL_CODE;
        }

        string testFile = cmdLine.GetArguments()[0];

        // parse the test file
        TestGroup[] groups = null;
        try
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(testFile);

            // Load up the algorithm tables
            Hashtable algorithms = ParseAlgorithms((XmlElement)doc.GetElementsByTagName("HashFunctions")[0]);

            // Load the test cases
            groups = ParseTestCases((XmlElement)doc.GetElementsByTagName("Tests")[0], algorithms);
        }
        catch(Exception e)
        {
            Console.Error.WriteLine("Error parsing input file : " + e.Message);
            return FAIL_CODE;
        }

        // setup the tests
        TestRunner runner = new TestRunner(groups, new ILogger[] { new ConsoleLogger(), new XmlLogger()});
        return runner.Run("HashDriver") ? PASS_CODE : FAIL_CODE;
    }

    /// <summary>
    ///   Parse the algorithm specifications from the <HashFunctions> element in the
    ///   test spec file.
    /// </summary>
    /// <remarks>
    ///   Throws an InvalidOperationException if an invalid XML node is presented
    /// </remarks>
    /// <param name="specRoot">HashFunctions element in the test spec file</param>
    /// <returns>Hashtable mapping algorithm names to their implementations</returns>
    private static Hashtable ParseAlgorithms(XmlElement specRoot)
    {
        Hashtable results = new Hashtable();

        foreach(XmlElement hashFunction in specRoot.ChildNodes)
        {
            string name = hashFunction.GetAttribute("name");
            string impl = hashFunction.GetAttribute("class");

            if(name == "" || impl == "")
                throw new InvalidOperationException("Missing 'name' or 'class' attribute on HashFunction");

            results.Add(name, impl);
        }

        return results;
    }
    
    /// <summary>
    ///   Parse the test case specifications from the <Tests> element in the
    ///   test spec file
    /// </summary>
    /// <remarks>
    ///   Throws an InvalidOperationException if an invalid XML node is presented
    /// </remarks>
    /// <param name="specRoot">Tests element in the test spec file</param>
    /// <param name="algorithms">mapping of algorithm names to implementations</returns>
    private static TestGroup[] ParseTestCases(XmlElement specRoot, Hashtable algorithms)
    {
        ArrayList groups = new ArrayList();

        // read all the test groups
        foreach(XmlElement test in specRoot.GetElementsByTagName("Test"))
        {
            string groupName = test.GetAttribute("name");

            if(groupName == "")
                throw new InvalidOperationException("Missing 'name' attribute on Test");

            ArrayList testCases = new ArrayList();

            // read all the tests in the group
            foreach(XmlElement testCase in test.GetElementsByTagName("TestItem"))
            {
                string dataType = testCase.GetAttribute("dataType").ToLower();
                string data = testCase.GetAttribute("data");

                // validate
                if(dataType == "")
                    throw new InvalidOperationException("Missing 'dataType' attribute on TestItem");
                if(!(dataType == "string" || dataType == "base64" || dataType == "hex"))
                    throw new InvalidOperationException("Invalid data type: " + dataType);

                // parse the data
                byte[] dataBytes = null;
                switch(dataType)
                {
                  case "string":
                    dataBytes = new System.Text.UTF8Encoding().GetBytes(data);
                    break;
                   
                  case "base64":
                    dataBytes = Convert.FromBase64String(data);
                    break;

                  case "hex":
                    dataBytes = Util.ParseHexBytes(data);
                    break;
                }

                // grab all the results for this
                foreach(XmlElement testResult in testCase.GetElementsByTagName("HashValue"))
                {
                    string algName = testResult.GetAttribute("algorithm");
                    string result = testResult.GetAttribute("value");
                    string key = testResult.GetAttribute("key");
					string truncation = testResult.GetAttribute("truncation");

                    if(algName == "" || result == "")
                        throw new InvalidOperationException("Missing 'algorithm' or 'value' attribute on HashValue");
                    
                    // create the hashing function
                    string algClass = (string)algorithms[algName];
                    HashAlgorithm hasher = null;
                    if(algClass == null)
                    {
                        hasher = (HashAlgorithm)CryptoConfig.CreateFromName(algName);
                    }
                    else
                    {
                        // create the hash algorithm using reflection
                        Type algType = Type.GetType(algClass);
                        if(algType == null)
                            throw new InvalidOperationException("Invalid hashing algorithm: " + algClass);
                        hasher = (HashAlgorithm)Activator.CreateInstance(algType);
                    }

                    if(hasher == null)
                        throw new InvalidOperationException("Could not create hash algorithm: " + algName);

                    // parse the key
                    byte[] keyBytes = Util.ParseHexBytes(key);

                    // see if the hashing algorithm needs a key
                    if(hasher is KeyedHashAlgorithm)
                        ((KeyedHashAlgorithm)hasher).Key = keyBytes;

					// convert the value to bytes
                    byte[] val = Util.ParseHexBytes(result);

                    // create a test case
                    string caseName = algName + " - " + data;
                    caseName = caseName.Length > 50 ? caseName.Substring(0, 50) : caseName;
                    
					if (truncation == "")
						testCases.Add(new HashTestCase(caseName, hasher, dataBytes, val));
					else
						testCases.Add(new HashTestCaseTruncate(caseName + "-" + truncation, hasher, dataBytes, val, Int32.Parse(truncation)));
                }
            }

            groups.Add(new TestGroup(groupName, (TestCase[])testCases.ToArray(new TestCase("", null).GetType())));
        }

        return (TestGroup[])groups.ToArray(new TestGroup("", new TestCase[] { }).GetType());
    }
}
