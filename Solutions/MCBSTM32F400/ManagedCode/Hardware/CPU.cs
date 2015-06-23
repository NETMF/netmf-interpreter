using System;
using System.Collections;
using System.Threading;
using System.Runtime.CompilerServices;
using Microsoft.SPOT.Hardware;
using System.IO.Ports;

namespace Microsoft.SPOT.Hardware.MCBSTM32F400
{

    /* Specifies valid hardware I/O pins. */
    public static class Pins
    {
        public const Cpu.Pin GPIO_A8      = (Cpu.Pin) 8;   // GPIO Port A Pin 8
        public const Cpu.Pin GPIO_B14     = (Cpu.Pin) 30;  // GPIO Port B Pin 14
        public const Cpu.Pin GPIO_B15     = (Cpu.Pin) 31;  // GPIO Port B Pin 15
        public const Cpu.Pin GPIO_D3      = (Cpu.Pin) 51;  // GPIO Port D Pin 3
        public const Cpu.Pin GPIO_F6      = (Cpu.Pin) 86;  // GPIO Port F Pin 6
        public const Cpu.Pin GPIO_F7      = (Cpu.Pin) 87;  // GPIO Port F Pin 7
        public const Cpu.Pin GPIO_F8      = (Cpu.Pin) 88;  // GPIO Port F Pin 8
        public const Cpu.Pin GPIO_F10     = (Cpu.Pin) 90;  // GPIO Port F Pin 10
        public const Cpu.Pin GPIO_G6_LED  = (Cpu.Pin) 102; // GPIO Port G Pin 6
        public const Cpu.Pin GPIO_G7_LED  = (Cpu.Pin) 103; // GPIO Port G Pin 7
        public const Cpu.Pin GPIO_G8_LED  = (Cpu.Pin) 104; // GPIO Port G Pin 8
        public const Cpu.Pin GPIO_H2_LED  = (Cpu.Pin) 114; // GPIO Port H Pin 2
        public const Cpu.Pin GPIO_H3_LED  = (Cpu.Pin) 115; // GPIO Port H Pin 3
        public const Cpu.Pin GPIO_H6_LED  = (Cpu.Pin) 118; // GPIO Port H Pin 6
        public const Cpu.Pin GPIO_H7_LED  = (Cpu.Pin) 119; // GPIO Port H Pin 7
        public const Cpu.Pin GPIO_H15     = (Cpu.Pin) 127; // GPIO Port H Pin 15
        public const Cpu.Pin GPIO_I1      = (Cpu.Pin) 129; // GPIO Port I Pin 1
        public const Cpu.Pin GPIO_I8      = (Cpu.Pin) 136; // GPIO Port I Pin 8
        public const Cpu.Pin GPIO_I10_LED = (Cpu.Pin) 138; // GPIO Port I Pin 10
        
        public const Cpu.Pin GPIO_NONE = Cpu.Pin.GPIO_NONE;
    }

    /* Specifies valid hardware serial ports */
    public static class SerialPorts
    {
        public const string COM1 = "COM1";
    }
    
    /* Specifies valid baud rates for hardware serial ports */
    public static class BaudRates
    {
        public const BaudRate Baud9600   = BaudRate.Baudrate9600;   // Baudrate 9600
        public const BaudRate Baud19200  = BaudRate.Baudrate19200;  // Baudrate 19200
        public const BaudRate Baud38400  = BaudRate.Baudrate38400;  // Baudrate 38400
        public const BaudRate Baud57600  = BaudRate.Baudrate57600;  // Baudrate 57600
        public const BaudRate Baud115200 = BaudRate.Baudrate115200; // Baudrate 115200
        public const BaudRate Baud230400 = BaudRate.Baudrate230400; // Baudrate 230400
    }

    /* Specifies resistor modes for GPIO ports */
    public static class ResistorModes
    {
        public const Port.ResistorMode PullUp   = Port.ResistorMode.PullUp;   // Internal Resistor Pull-Up
        public const Port.ResistorMode PullDown = Port.ResistorMode.PullDown; // Internal Resistor Pull-Down
        public const Port.ResistorMode Disabled = Port.ResistorMode.Disabled; // Internal Resistor Disabled
        
    }
     /* Specifies interrupt modes for GPIO ports */
    public static class InterruptModes
    {
        public const Port.InterruptMode InterruptEdgeLow       = Port.InterruptMode.InterruptEdgeLow ;
        public const Port.InterruptMode InterruptEdgeHigh      = Port.InterruptMode.InterruptEdgeHigh;
        public const Port.InterruptMode InterruptEdgeBoth      = Port.InterruptMode.InterruptEdgeBoth;
        public const Port.InterruptMode InterruptEdgeLevelHigh = Port.InterruptMode.InterruptEdgeLevelHigh;
        public const Port.InterruptMode InterruptEdgeLevelLow  = Port.InterruptMode.InterruptEdgeLevelLow;
        public const Port.InterruptMode InterruptNone          = Port.InterruptMode.InterruptNone;
    }
    
    /* Specified valid SPI ports */
    public static class SPI_Devices
    {
        public const Microsoft.SPOT.Hardware.SPI.SPI_module SPI3 = Microsoft.SPOT.Hardware.SPI.SPI_module.SPI3; // SPI Module SPI3
    }
    
}
