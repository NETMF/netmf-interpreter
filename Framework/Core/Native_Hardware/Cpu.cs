using System;
using System.Collections;
using System.Threading;
using System.Runtime.CompilerServices;

namespace Microsoft.SPOT.Hardware
{
    public static class Cpu
    {
        [Flags]
        public enum PinUsage : byte
        {
            NONE = 0,
            INPUT = 1,
            OUTPUT = 2,
            ALTERNATE_A = 4,
            ALTERNATE_B = 8,
        };

        [Flags]
        public enum PinValidResistorMode : byte
        {
            NONE = 0,
            Disabled = 1 << Microsoft.SPOT.Hardware.Port.ResistorMode.Disabled,
            PullUp = 1 << Microsoft.SPOT.Hardware.Port.ResistorMode.PullDown,
            PullDown = 1 << Microsoft.SPOT.Hardware.Port.ResistorMode.PullUp,
        };

        [Flags]
        public enum PinValidInterruptMode : byte
        {
            NONE = 0,
            InterruptEdgeLow = 1 << Microsoft.SPOT.Hardware.Port.InterruptMode.InterruptEdgeLow,
            InterruptEdgeHigh = 1 << Microsoft.SPOT.Hardware.Port.InterruptMode.InterruptEdgeHigh,
            InterruptEdgeBoth = 1 << Microsoft.SPOT.Hardware.Port.InterruptMode.InterruptEdgeBoth,
            InterruptEdgeLevelHigh = 1 << Microsoft.SPOT.Hardware.Port.InterruptMode.InterruptEdgeLevelHigh,
            InterruptEdgeLevelLow = 1 << Microsoft.SPOT.Hardware.Port.InterruptMode.InterruptEdgeLevelLow,
        };

        public enum Pin : int
        {
            GPIO_NONE = -1,
            GPIO_Pin0 = 0,
            GPIO_Pin1 = 1,
            GPIO_Pin2 = 2,
            GPIO_Pin3 = 3,
            GPIO_Pin4 = 4,
            GPIO_Pin5 = 5,
            GPIO_Pin6 = 6,
            GPIO_Pin7 = 7,
            GPIO_Pin8 = 8,
            GPIO_Pin9 = 9,
            GPIO_Pin10 = 10,
            GPIO_Pin11 = 11,
            GPIO_Pin12 = 12,
            GPIO_Pin13 = 13,
            GPIO_Pin14 = 14,
            GPIO_Pin15 = 15,
        }

        public enum PWMChannel : int
        {
            PWM_NONE = -1,
            PWM_0    =  0,
            PWM_1    =  1,
            PWM_2    =  2,
            PWM_3    =  3,
            PWM_4    =  4,
            PWM_5    =  5,
            PWM_6    =  6,
            PWM_7    =  7,
            PWM_8    =  8,
            PWM_9    =  9,
            PWM_10   = 10,
            PWM_11   = 11,
            PWM_12   = 12,
            PWM_13   = 13,
            PWM_14   = 14,
            PWM_15   = 15,
        }

        public enum AnalogChannel : int
        {
            ANALOG_NONE = -1,
            ANALOG_0 = 0,
            ANALOG_1 = 1,
            ANALOG_2 = 2,
            ANALOG_3 = 3,
            ANALOG_4 = 4,
            ANALOG_5 = 5,
            ANALOG_6 = 6,
            ANALOG_7 = 7,
        }

        public enum AnalogOutputChannel : int
        {
            ANALOG_OUTPUT_NONE = -1,
            ANALOG_OUTPUT_0 = 0,
            ANALOG_OUTPUT_1 = 1,
            ANALOG_OUTPUT_2 = 2,
            ANALOG_OUTPUT_3 = 3,
            ANALOG_OUTPUT_4 = 4,
            ANALOG_OUTPUT_5 = 5,
            ANALOG_OUTPUT_6 = 6,
            ANALOG_OUTPUT_7 = 7,
        }

        //--//

        extern public static uint SystemClock
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        extern public static uint SlowClock
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        extern public static TimeSpan GlitchFilterTime
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;

            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            set;
        }
    }
}


