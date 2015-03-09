using System;

namespace Microsoft.SPOT.Platform.Test
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class SetUp : Attribute
    {
        public SetUp()
            : base()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class TestMethod : Attribute
    {
        public TestMethod()
            : base()
        {
            // Allow for multiple methods with [TestMethod] attribute.            
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class TearDown : Attribute
    {
        public TearDown()
            : base()
        {
            // Only one TearDown attribute should be allowed per test class.
            // Any method attributed with [TearDown] should always be executed at the end.
            // This method needs to get executed irrespective of what the test results are.
        }
    }
}