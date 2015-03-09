/*---------------------------------------------------------------------
* VolumeLabelTests.cs - file description
* Version: 1.0
* Author: 
* Created: 
* 
* Tests the basic functionality of file system volumes
* ---------------------------------------------------------------------*/

using System;
using System.Reflection;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.IO;
using Microsoft.SPOT.IO;

namespace Microsoft.SPOT.Platform.Tests
{
    public class VolumeLabelTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests");

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
        }

        [TestMethod]
        public MFTestResults VolumeLabelTest_SetLabel()
        {
            bool testResult = true;

            VolumeInfo[] volumes = VolumeInfo.GetVolumes();

            for (int i = 0; i < volumes.Length; i++)
            {
                volumes[i].Format(volumes[i].FileSystem, 0, "Label1", true);

                testResult &= volumes[i].VolumeLabel == "Label1";

                volumes[i].Format(volumes[i].FileSystem, 0, "Label2", true);

                testResult &= volumes[i].VolumeLabel == "Label2";
            }

            return testResult ? MFTestResults.Pass : MFTestResults.Fail;
        }
    }
}
