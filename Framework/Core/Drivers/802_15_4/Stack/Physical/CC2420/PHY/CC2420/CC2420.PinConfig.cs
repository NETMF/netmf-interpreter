////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using Microsoft.SPOT.Hardware;

namespace Microsoft.SPOT.Wireless.IEEE_802_15_4.Phy
{

    /// <summary>
    /// Specifies identifiers for hardware I/O pins.
    /// </summary>
    public static class CC2420Pins
    {
        /// <summary>
        /// GPIO port pin 0.
        /// </summary>
        public const Cpu.Pin GPIO_PORT_CC2420_FIFOP = (Cpu.Pin)0;
        /// <summary>
        /// GPIO port pin 11.
        /// </summary>
        public const Cpu.Pin GPIO_PORT_PIN_11 = (Cpu.Pin)11;
        /// <summary>
        /// GPIO port pin 16.
        /// </summary>
        public const Cpu.Pin GPIO_PORT_CC2420_SFD = (Cpu.Pin)16;
        /// <summary>
        /// GPIO port pin 22.
        /// </summary>
        public const Cpu.Pin GPIO_PORT_CC2420_RESETN = (Cpu.Pin)22;
        /// <summary>
        /// GPIO port pin 23.
        /// </summary>
        public const Cpu.Pin GPIO_PORT_PIN_23 = (Cpu.Pin)23;
        /// <summary>
        /// GPIO port pin 25.
        /// </summary>
        public const Cpu.Pin GPIO_PORT_PIN_25 = (Cpu.Pin)25;
        /// <summary>
        /// GPIO port pin 26.
        /// </summary>
        public const Cpu.Pin GPIO_PORT_PIN_26 = (Cpu.Pin)26;
        /// <summary>
        /// GPIO port pin 34.
        /// </summary>
        public const Cpu.Pin GPIO_PORT_PIN_34 = (Cpu.Pin)34;
        /// <summary>
        /// GPIO port pin 35.
        /// </summary>
        public const Cpu.Pin GPIO_PORT_PIN_35 = (Cpu.Pin)35;
        /// <summary>
        /// GPIO port pin 36.
        /// </summary>
        public const Cpu.Pin GPIO_PORT_PIN_36 = (Cpu.Pin)36;
        /// <summary>
        /// GPIO port pin 38.
        /// </summary>
        public const Cpu.Pin GPIO_PORT_PIN_38 = (Cpu.Pin)38;
        /// <summary>
        /// GPIO port pin 39.
        /// </summary>
        public const Cpu.Pin GPIO_PORT_PIN_39 = (Cpu.Pin)39;
        /// <summary>
        /// GPIO port pin 41.
        /// </summary>
        public const Cpu.Pin GPIO_PORT_PIN_41 = (Cpu.Pin)41;
        /// <summary>
        /// GPIO port pin 42.
        /// </summary>
        public const Cpu.Pin GPIO_PORT_PIN_42 = (Cpu.Pin)42;
        /// <summary>
        /// GPIO port pin 43.
        /// </summary>
        public const Cpu.Pin GPIO_PORT_PIN_43 = (Cpu.Pin)43;
        /// <summary>
        /// GPIO port pin 46.
        /// </summary>
        public const Cpu.Pin GPIO_PORT_PIN_46 = (Cpu.Pin)46;
        /// <summary>
        /// GPIO port pin 47.
        /// </summary>
        public const Cpu.Pin GPIO_PORT_PIN_47 = (Cpu.Pin)47;
        /// <summary>
        /// GPIO port pin 96.
        /// </summary>
        public const Cpu.Pin GPIO_PORT_PIN_96 = (Cpu.Pin)96;
        /// <summary>
        /// GPIO port pin 99.
        /// </summary>
        public const Cpu.Pin GPIO_PORT_PIN_99 = (Cpu.Pin)99;
        /// <summary>
        /// GPIO port pin 103.
        /// </summary>
        public const Cpu.Pin GPIO_PORT_LED_RED = (Cpu.Pin)47;
        /// <summary>
        /// GPIO port pin 104.
        /// </summary>
        public const Cpu.Pin GPIO_PORT_LED_GREEN = (Cpu.Pin)49;
        /// <summary>
        /// GPIO port pin 105.
        /// </summary>
        public const Cpu.Pin GPIO_PORT_LED_BLUE = (Cpu.Pin)41;
        /// <summary>
        /// GPIO port pin 114.
        /// </summary>
        public const Cpu.Pin GPIO_PORT_CC2420_FIFO = (Cpu.Pin)114;
        /// <summary>
        /// GPIO port pin 115.
        /// </summary>
        public const Cpu.Pin GPIO_PORT_CC2420_VREG_EN = (Cpu.Pin)115;
        /// <summary>
        /// GPIO port pin 116.
        /// </summary>
        public const Cpu.Pin GPIO_PORT_CC2420_CCA = (Cpu.Pin)116;
        /// <summary>
        /// GPIO port pin 117.
        /// </summary>
        public const Cpu.Pin GPIO_PORT_PIN_117 = (Cpu.Pin)117;
        /// <summary>
        /// GPIO port pin 118.
        /// </summary>
        public const Cpu.Pin GPIO_PORT_PIN_118 = (Cpu.Pin)118;
        public const Cpu.Pin GPIO_NONE = Cpu.Pin.GPIO_NONE;
    }

    /// <summary>
    /// Configuration pins for the Chipcon CC2420 IEEE 802.15.4 chip.
    /// </summary>
    public struct CC2420PinConfig
    {
        /// <summary>
        /// Pin information for CC2420 pins
        /// </summary>
        public struct PinInfo
        {
            /// <summary>
            /// The pin number
            /// </summary>
            public Cpu.Pin PinNumber;
            /// <summary>
            /// The pin resistor mode
            /// </summary>
            public Port.ResistorMode PinMode;
        }

        /// <summary>
        /// switch on/off radio chipset.
        /// </summary>
        public PinInfo ChipPower;

        /// <summary>
        /// The fifo pin
        /// </summary>
        public PinInfo FIFOPin;
        /// <summary>
        /// The fifop pin
        /// </summary>
        public PinInfo FIFOPPin;
        /// <summary>
        /// The CCA pin (currently not used)
        /// </summary>
        public PinInfo CCAPin;              // Not currently used
        /// <summary>
        /// The start-of-frame-detected pin
        /// </summary>
        public PinInfo SFDPin;
        /// <summary>
        /// Resets the chip
        /// </summary>
        public PinInfo ResetNPin;
        /// <summary>
        /// Chip select for the chip.
        /// </summary>
        public PinInfo CsNPin;
        /// <summary>
        /// PIN of RX activity Led.
        /// </summary>
        public PinInfo LedRxPin;
        /// <summary>
        /// PIN of TX activity Led.
        /// </summary>
        public PinInfo LedTxPin;
        /// <summary>
        /// The SPI module to which the chip is connected
        /// </summary>
        public SPI.SPI_module SPI_mod;

        /// <summary>
        /// Get default configuration
        /// </summary>
        /// <returns>default configuration</returns>
        public static CC2420PinConfig Default()
        {
            CC2420PinConfig res = new CC2420PinConfig();
            res.ChipPower.PinNumber = CC2420Pins.GPIO_PORT_CC2420_VREG_EN;
            res.ChipPower.PinMode = Port.ResistorMode.Disabled;
            res.FIFOPin.PinNumber = CC2420Pins.GPIO_PORT_CC2420_FIFO;
            res.FIFOPin.PinMode = Port.ResistorMode.Disabled;
            res.FIFOPPin.PinNumber = CC2420Pins.GPIO_PORT_CC2420_FIFOP;
            res.FIFOPPin.PinMode = Port.ResistorMode.Disabled;
            res.CCAPin.PinNumber = CC2420Pins.GPIO_PORT_CC2420_CCA;
            res.CCAPin.PinMode = Port.ResistorMode.Disabled;
            res.SFDPin.PinNumber = CC2420Pins.GPIO_PORT_CC2420_SFD;
            res.SFDPin.PinMode = Port.ResistorMode.Disabled;
            res.CsNPin.PinNumber = CC2420Pins.GPIO_PORT_PIN_39;
            res.CsNPin.PinMode = Port.ResistorMode.Disabled;
            res.ResetNPin.PinNumber = CC2420Pins.GPIO_PORT_CC2420_RESETN;
            res.ResetNPin.PinMode = Port.ResistorMode.Disabled;
            res.LedRxPin.PinNumber = CC2420Pins.GPIO_PORT_LED_RED;
            res.LedRxPin.PinMode = Port.ResistorMode.Disabled;
            res.LedTxPin.PinNumber = CC2420Pins.GPIO_PORT_LED_GREEN;
            res.LedTxPin.PinMode = Port.ResistorMode.Disabled;
            res.SPI_mod = SPI.SPI_module.SPI3;
            return res;
        }

        public static CC2420PinConfig DefaultiMXS()
        {
            CC2420PinConfig res = new CC2420PinConfig();
            res.ChipPower.PinNumber = (Cpu.Pin)48;
            res.ChipPower.PinMode = Port.ResistorMode.Disabled;
            res.FIFOPin.PinNumber = (Cpu.Pin)40;
            res.FIFOPin.PinMode = Port.ResistorMode.Disabled;
            res.FIFOPPin.PinNumber = (Cpu.Pin)63;
            res.FIFOPPin.PinMode = Port.ResistorMode.Disabled;
            res.CCAPin.PinNumber = (Cpu.Pin)62;
            res.CCAPin.PinMode = Port.ResistorMode.Disabled;
            res.SFDPin.PinNumber = (Cpu.Pin)60;
            res.SFDPin.PinMode = Port.ResistorMode.Disabled;
            res.CsNPin.PinNumber = (Cpu.Pin)61;
            res.CsNPin.PinMode = Port.ResistorMode.Disabled;
            res.ResetNPin.PinNumber = (Cpu.Pin)44;
            res.ResetNPin.PinMode = Port.ResistorMode.Disabled;

            res.LedRxPin.PinNumber = (Cpu.Pin)41; /// VK_MENU: MC9328MXL_GPIO::c_Port_B_15
            res.LedRxPin.PinMode = Port.ResistorMode.Disabled;
            res.LedTxPin.PinNumber = (Cpu.Pin)49; /// VK_BACK: pin = MC9328MXL_GPIO::c_Port_B_17;
            res.LedTxPin.PinMode = Port.ResistorMode.Disabled;

            res.SPI_mod = SPI.SPI_module.SPI1;
            return res;
        }

        public static CC2420PinConfig DefaultSAM9261()
        {
            CC2420PinConfig res = new CC2420PinConfig();
            res.ChipPower.PinNumber = (Cpu.Pin)22;
            res.ChipPower.PinMode = Port.ResistorMode.Disabled;
            res.ResetNPin.PinNumber = (Cpu.Pin)21;
            res.ResetNPin.PinMode = Port.ResistorMode.Disabled;
            res.FIFOPin.PinNumber = (Cpu.Pin)16;
            res.FIFOPin.PinMode = Port.ResistorMode.Disabled;
            res.FIFOPPin.PinNumber = (Cpu.Pin)15;
            res.FIFOPPin.PinMode = Port.ResistorMode.Disabled;
            res.CCAPin.PinNumber = (Cpu.Pin)8;
            res.CCAPin.PinMode = Port.ResistorMode.Disabled;
            res.SFDPin.PinNumber = (Cpu.Pin)7;
            res.SFDPin.PinMode = Port.ResistorMode.Disabled;
            res.CsNPin.PinNumber = (Cpu.Pin)4;
            res.CsNPin.PinMode = Port.ResistorMode.Disabled;

            res.LedRxPin.PinNumber = (Cpu.Pin)13;
            res.LedRxPin.PinMode = Port.ResistorMode.Disabled;
            res.LedTxPin.PinNumber = (Cpu.Pin)14;
            res.LedTxPin.PinMode = Port.ResistorMode.Disabled;

            res.SPI_mod = SPI.SPI_module.SPI1;
            return res;
        }
    }
}


