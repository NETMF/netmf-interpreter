using System;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    class Master
    {
        static void Main()
        {
            string[] args = { "FeatureTests" };
            MFDesktopTestRunner runner = new MFDesktopTestRunner(args);
        }
    }
}
