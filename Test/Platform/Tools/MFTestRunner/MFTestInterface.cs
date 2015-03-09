using System;
using Microsoft.SPOT;

namespace Microsoft.SPOT.Platform.Test
{
    public enum InitializeResult
    {
        ReadyToGo,
        Skip,
    }

    public interface IMFTestInterface
    {
        [SetUp]
        InitializeResult Initialize();

        [TearDown]
        void CleanUp();
    }
}
