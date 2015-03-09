////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for STM32: Copyright (c) Oberon microsystems, Inc.
//
//  class Microsoft.SPOT.Hardware.STM32.STM32HardwareProvider
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using System.Runtime.CompilerServices;
using Microsoft.SPOT.Hardware;

namespace Microsoft.SPOT.Hardware.STM32
{
    internal class STM32HardwareProvider : HardwareProvider
    {
        static STM32HardwareProvider()
        {
            Microsoft.SPOT.Hardware.HardwareProvider.Register(new STM32HardwareProvider());
        }


        override public void GetSerialPins(string comPort, out Cpu.Pin rxPin, out Cpu.Pin txPin, out Cpu.Pin ctsPin, out Cpu.Pin rtsPin)
        {
            switch (comPort)
            {
                case "COM1":
                    rxPin  = Pins.GPIO_PIN_A_10;
                    txPin  = Pins.GPIO_PIN_A_9;
                    ctsPin = Pins.GPIO_PIN_A_11;
                    rtsPin = Pins.GPIO_PIN_A_12;
                    break;
                case "COM2":
                    rxPin  = Pins.GPIO_PIN_A_3;
                    txPin  = Pins.GPIO_PIN_A_2;
                    ctsPin = Pins.GPIO_PIN_A_0;
                    rtsPin = Pins.GPIO_PIN_A_1;
                    break;
                case "COM3":
                    rxPin  = Pins.GPIO_PIN_B_11;
                    txPin  = Pins.GPIO_PIN_B_10;
                    ctsPin = Pins.GPIO_PIN_B_13;
                    rtsPin = Pins.GPIO_PIN_B_14;
                    break;
                default:
                    throw new NotSupportedException();                    
            }
        }

        override public void GetI2CPins( out Cpu.Pin scl, out Cpu.Pin sda )
        {
            scl = Pins.GPIO_PIN_B_6;
            sda = Pins.GPIO_PIN_B_7;
        }

        override public void GetSpiPins( SPI.SPI_module spi_mod, out Cpu.Pin msk, out Cpu.Pin miso, out Cpu.Pin mosi )
        {
            switch (spi_mod)
            {
                case SPI.SPI_module.SPI1:
                    msk  = Pins.GPIO_PIN_A_5;
                    miso = Pins.GPIO_PIN_A_6;
                    mosi = Pins.GPIO_PIN_A_7;
                    break;
                case SPI.SPI_module.SPI2:
                    msk  = Pins.GPIO_PIN_B_13;
                    miso = Pins.GPIO_PIN_B_14;
                    mosi = Pins.GPIO_PIN_B_15;
                    break;
                case SPI.SPI_module.SPI3:
                    msk  = Pins.GPIO_PIN_B_3;
                    miso = Pins.GPIO_PIN_B_4;
                    mosi = Pins.GPIO_PIN_B_5;
                    break;
                default:
                    throw new NotSupportedException();                    
            }
        }        
    }
}
