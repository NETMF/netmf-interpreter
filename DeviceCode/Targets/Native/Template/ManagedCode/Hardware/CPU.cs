using System;
using System.Collections;
using System.Threading;
using System.Runtime.CompilerServices;
using Microsoft.SPOT.Hardware;
using System.IO.Ports;

namespace Microsoft.SPOT.Hardware.Template
{

    /// <summary>
    /// Specifies identifiers for hardware I/O pins.
    /// </summary>
    public static class Pins
    {
        /// <summary>
        /// GPIO pin 0
        /// </summary>
        public const Cpu.Pin GPIO_PIN_0  = (Cpu.Pin)0;
        /// <summary>
        /// GPIO pin 1
        /// </summary>
        public const Cpu.Pin GPIO_PIN_1  = (Cpu.Pin)1;
        /// <summary>
        /// GPIO pin 2
        /// </summary>
        public const Cpu.Pin GPIO_PIN_2  = (Cpu.Pin)2;
        /// <summary>
        /// GPIO pin 3
        /// </summary>
        public const Cpu.Pin GPIO_PIN_3  = (Cpu.Pin)3;
        /// <summary>
        /// GPIO pin 4
        /// </summary>
        public const Cpu.Pin GPIO_PIN_4  = (Cpu.Pin)4;
        /// <summary>
        /// GPIO pin 5
        /// </summary>
        public const Cpu.Pin GPIO_PIN_5  = (Cpu.Pin)5;
        /// <summary>
        /// GPIO pin 6
        /// </summary>
        public const Cpu.Pin GPIO_PIN_6  = (Cpu.Pin)6;
        /// <summary>
        /// GPIO pin 7
        /// </summary>
        public const Cpu.Pin GPIO_PIN_7  = (Cpu.Pin)7;
        /// <summary>
        /// GPIO pin 8
        /// </summary>
        public const Cpu.Pin GPIO_PIN_8  = (Cpu.Pin)8;
        /// <summary>
        /// GPIO pin 9
        /// </summary>
        public const Cpu.Pin GPIO_PIN_9  = (Cpu.Pin)9;
        
        public const Cpu.Pin GPIO_NONE = Cpu.Pin.GPIO_NONE;
    }

    public static class SerialPorts
    {
        public const string COM1 = "COM1";
        public const string COM2 = "COM2";
    }
    
    public static class BaudRates
    {
        public const BaudRate Baud19200  = BaudRate.Baudrate19200;
        public const BaudRate Baud38400  = BaudRate.Baudrate38400;
        public const BaudRate Baud57600  = BaudRate.Baudrate57600;
        public const BaudRate Baud115200 = BaudRate.Baudrate115200;
        public const BaudRate Baud230400 = BaudRate.Baudrate230400;
    }

    public static class ResistorModes
    {
        public const Port.ResistorMode PullUp   = Port.ResistorMode.PullUp;
        public const Port.ResistorMode Disabled = Port.ResistorMode.Disabled;
    }

    public static class InterruptModes
    {
        public const Port.InterruptMode InterruptEdgeLow       = Port.InterruptMode.InterruptEdgeLow ;
        public const Port.InterruptMode InterruptEdgeHigh      = Port.InterruptMode.InterruptEdgeHigh;
        public const Port.InterruptMode InterruptEdgeBoth      = Port.InterruptMode.InterruptEdgeBoth;
        public const Port.InterruptMode InterruptEdgeLevelHigh = Port.InterruptMode.InterruptEdgeLevelHigh;
        public const Port.InterruptMode InterruptEdgeLevelLow  = Port.InterruptMode.InterruptEdgeLevelLow;
        public const Port.InterruptMode InterruptNone          = Port.InterruptMode.InterruptNone;
    }

    public static class SPI_Devices
    {
        public const Microsoft.SPOT.Hardware.SPI.SPI_module SPI1 = Microsoft.SPOT.Hardware.SPI.SPI_module.SPI1;
    }
    
}
