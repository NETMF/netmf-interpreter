using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using System.Text.RegularExpressions;

namespace TextTests
{
    class CacheTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
        }

        [TestMethod]
        public MFTestResults RegExpTest_7_Cache_Test_0()
        {
            //This should be in cache already from a previous Test
            bool result = false;
            foreach (RegexProgram rp in Regex.Cache.ToArray())
                if (rp.Pattern.Equals(@"((\w+)[\s.])+")) result = true;
            return result ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults RegExpTest_7_Cache_Test_1()
        {
            //This should be in cache already from a previous Test
            bool result = false;
            foreach(RegexProgram rp in Regex.Cache.ToArray())
                if(rp.Pattern.Equals("^a.*b$")) result = true;
            return result ? MFTestResults.Pass : MFTestResults.Fail;
        }

        [TestMethod]
        public MFTestResults RegExpTest_7_Cache_Test_2()
        {
            //Compile a pattern and check the cache
            string pattern = "w+\b";
            new Regex(pattern);
            bool result = false;
            foreach (RegexProgram rp in Regex.Cache.ToArray())
                if (rp.Pattern.Equals(pattern)) result = true;
            return result ? MFTestResults.Pass : MFTestResults.Fail;
        }

    }
}
