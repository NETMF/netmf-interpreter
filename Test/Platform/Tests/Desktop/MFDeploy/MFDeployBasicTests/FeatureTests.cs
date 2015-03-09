using System;
using Microsoft.SPOT.Platform.Test;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;

namespace Microsoft.SPOT.Platform.Tests
{
    public class FeatureTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("NOTE: This is just a placeholder currently. More tests will be added later");
            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");
        }

        [TestMethod]
        public MFTestResults MFPortDefinitionEnumerationTest()
        {
            MFDeploy dep = new MFDeploy();
            try
            {
                int count = dep.DeviceList.Count;
                Log.Comment("Found " + count + " PortDefinitions.");
                int i = 1;
                foreach (MFPortDefinition pd in dep.DeviceList)
                {
                    Log.Comment("====================================");
                    Log.Comment("Port Definition " + i++);
                    Log.Comment("====================================");
                    Log.Comment("Name = " + pd.Name);
                    Log.Comment("Port = " + pd.Port);
                    Log.Comment("Transport = " + pd.Transport);
                }
                Log.Comment("====================================");
                Log.Comment("Successfully iterated through all the enumerations");
                Log.Comment("====================================");
            }
            catch (Exception ex)
            {
                Log.Exception(ex.Message);
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        }

    }
}
