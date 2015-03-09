using System;
using Microsoft.SPOT;

namespace Microsoft.SPOT.Platform.Test
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class SetUp : Attribute
    {
        public SetUp()
            : base()
        {
            // Only one SetUp attribute should be allowed per test class.
            // Any method attributed with [SetUp] should always be executed first.
            Debug.Print("<?xml version=\"1.0\" encoding=\"utf-8\" ?> ");
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