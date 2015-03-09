using System;
using System.Threading;
using System.Runtime.CompilerServices;
using Microsoft.SPOT.Hardware;

namespace Microsoft.SPOT.Hardware.STM32F4
{
    internal class STM32F4HardwareProvider : HardwareProvider
    {
        static STM32F4HardwareProvider()
        {
            Microsoft.SPOT.Hardware.HardwareProvider.Register(new STM32F4HardwareProvider());
        }
    }
}
