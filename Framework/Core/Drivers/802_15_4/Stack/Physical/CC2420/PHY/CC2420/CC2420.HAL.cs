////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT.Hardware;

namespace Microsoft.SPOT.Wireless.IEEE_802_15_4.Phy
{
    /// <summary>
    /// Hardware abstraction layer for ChipCon CC2420.
    /// The CC2420 implements the 802.15.4 PHY layer (as well as some MAC support) and
    /// is connected via SPI bus.
    /// </summary>
    internal class HALCC2420 : IDisposable
    {
        #region Field definitions
        /// <summary>
        /// Enumerates the command strobes of the Chipcon CC2420 RF Transceiver.
        /// </summary>
        internal enum Cmd : byte
        {
            SNOP = 0x00,    // No-op (no effect other than reading out status bits)
            SXOscOn = 0x01,    // Turn on the crystal oscillator (set XOSC16M_PD = 0 and BIAS_PD = 0)
            STXCal = 0x02,    // Enable and calibrate frequency synthesizer for TX
            SRXOn = 0x03,    // Enable RX
            STXOn = 0x04,    // Enable TX after calibration
            STXOnCCA = 0x05,    // If CCA indicates clear channel then enable calibration then TX, start encryption, o/w do nothing
            SRFOff = 0x06,    // Disable RX/TX and frequency synthesizer
            SXOscOff = 0x07,    // Turn off the crystal oscillator and RF
            SFlushRX = 0x08,    // Flush the RX FIFO buffer and reset the demodulator
            SFlushTX = 0x09,    // Flush the TX FIFO buffer
            SACK = 0x0A,    // Send ACK frame with pending field cleared
            SACKPend = 0x0B,    // Send ACK frame with pending field set
            SRXDec = 0x0C,    // Start RX-FIFO with in-line decryption / authentication
            STXEnc = 0x0D,    // Start TX-FIFO with in-line encryption / authentication
            SAES = 0x0E,    // AES Stand alone encryption strobe
        }

        /// <summary>
        /// Enumerates the hardware registers of the Chipcon CC2420 RF Transceiver.
        /// </summary>
        internal enum Reg : byte
        {
            ////////////////////////////////////////////////////////////////////////////
            /// Command Strobe Registers for the CC2420
            SNOP = 0x00,    // No-op (no effect other than reading out status bits)
            SXOscOn = 0x01,    // Turn on the crystal oscillator (set XOSC16M_PD = 0 and BIAS_PD = 0)
            STXCal = 0x02,    // Enable and calibrate frequency synthesizer for TX
            SRXOn = 0x03,    // Enable RX
            STXOn = 0x04,    // Enable TX after calibration
            STXOnCCA = 0x05,    // If CCA indicates clear channel then enable calibration then TX, start encryption, o/w do nothing
            SRFOff = 0x06,    // Disable RX/TX and frequency synthesizer
            SXOscOff = 0x07,    // Turn off the crystal oscillator and RF
            SFlushRX = 0x08,    // Flush the RX FIFO buffer and reset the demodulator
            SFlushTX = 0x09,    // Flush the TX FIFO buffer
            SACK = 0x0A,    // Send ACK frame with pending field cleared
            SACKPend = 0x0B,    // Send ACK frame with pending field set
            SRXDec = 0x0C,    // Start RX-FIFO with in-line decryption / authentication
            STXEnc = 0x0D,    // Start TX-FIFO with in-line encryption / authentication
            SAES = 0x0E,    // AES Stand alone encryption strobe
            ////////////////////////////////////////////////////////////////////////////
            /// Configuration Registers for the CC2420
            Main = 0x10,    // Main Control Register
            MDMCtrl0 = 0x11,    // Modem Control Register 0
            MDMCtrl1 = 0x12,    // Modem Control Register 1
            RSSI = 0x13,    // RSSI and CCA Status and Control register
            SyncWord = 0x14,    // Synchronisation word control register
            TXCtrl = 0x15,    // Transmit Control Register
            RXCtrl0 = 0x16,    // Receive Control Register 0
            RXCtrl1 = 0x17,    // Receive Control Register 1
            FSCtrl = 0x18,    // Frequency Synthesizer Control and Status Register
            SecCtrl0 = 0x19,    // Security Control Register 0
            SecCtrl1 = 0x1A,    // Security Control Register 1
            BattMon = 0x1B,    // Battery Monitor Control and Status Register
            IOCFG0 = 0x1C,    // Input / Output Control Register 0
            IOCFG1 = 0x1D,    // Input / Output Control Register 1
            ManfIDL = 0x1E,    // Manufacturer ID, Low 16 bits
            ManfIDH = 0x1F,    // Manufacturer ID, High 16 bits
            FSMTC = 0x20,    // Finite State Machine Time Constants
            ManAND = 0x21,    // Manual signal AND override register
            ManOR = 0x22,    // Manual signal OR override register
            AGCCtrl = 0x23,    // AGC Control Register
            AGCTst0 = 0x24,    // AGC Test Register 0
            AGCTst1 = 0x25,    // AGC Test Register 1
            AGCTst2 = 0x26,    // AGC Test Register 2
            FSTst0 = 0x27,    // Frequency Synthesizer Test Register 0
            FSTst1 = 0x28,    // Frequency Synthesizer Test Register 1
            FSTst2 = 0x29,    // Frequency Synthesizer Test Register 2
            FSTst3 = 0x2A,    // Frequency Synthesizer Test Register 3
            RXBPFTst = 0x2B,    // Receiver Bandpass Filter Test Register
            FSMState = 0x2C,    // Finite State Machine State Status Register
            ADCTst = 0x2D,    // ADC Test Register
            DACTst = 0x2E,    // DAC Test Register
            TOPTst = 0x2F,    // Top Level Test Register
            Reserved = 0x30,    // Reserved for future use control / status register
            TXFIFO = 0x3E,    // Transmit FIFO Byte Register
            RXFIFO = 0x3F,    // Receiver FIFO Byte Register
        }

        /// <summary>
        /// Enumerates the hardware register fields of the Chipcon CC2420 RF
        /// Transceiver.  Ref: CC2420Const.h by J. Polastre.
        /// </summary>
        internal enum Field : byte
        {
            ////////////////////////////////////////////////////////////////////////
            /// Main register bit positions
            MainResetN = 15,  // Active low reset of entire circuit, should be applied
            // before doing anything else (same as RESETn reset pin).

            EncResetN = 14,  // Active low reset of the encryption module. (Test only)
            DemodResetN = 13,  // Active low reset of the demodulator module. (Test only)
            ModResetN = 12,  // Active low reset of the modulator module. (Test only)
            FSResetN = 11,  // Active low reset of the frequency synthesizer module. (Test only)
            // 1:10 = Reserved, write as 0

            MainXOsc16MBypass = 0,  // Bypasses the crystal oscillator and uses a buffered version
            // of the signal on Q1 directly. This can be used to apply an
            // external rail-rail clock signal to the Q1 pin.
            ////////////////////////////////////////////////////////////////////////
            // MDMCtrl0 Register Bit Positions
            MDMCtrl0Frame = 13,  // 0 : reject reserved frame types, 1 = accept
            MDMCtrl0PANCrd = 12,  // 0 : not a PAN coordinator
            MDMCtrl0AddrDecode = 11,  // 1 : enable address decode
            MDMCtrl0CCAHyst = 8,   // 3 bits (8,9,10) : CCA hysteris in db
            MDMCtrl0CCAMode = 6,   // 2 bits (6,7)    : CCA trigger modes
            MDMCtrl0AutoCRC = 5,   // 1 : generate/chk CRC
            MDMCtrl0AutoACK = 4,   // 1 : Ack valid packets
            MDMCtrl0Preabml = 0,   // 4 bits (0..3): Preamble length
            ////////////////////////////////////////////////////////////////////////
            // MDMCtrl1 Register Bit Positions
            MDMCtrl1CorrThresh = 6,   // 5 bits (6..10) : correlator threshold
            MDMCtrl1DemodMode = 5,   // 0: lock freq after preamble match, 1: continous udpate
            MDMCtrl1ModuMode = 4,   // 0: IEEE 802.15.4
            MDMCtrl1TXMode = 2,   // 2 bits (2,3) : 0: use buffered TXFIFO
            MDMCtrl1RXMode = 0,   // 2 bits (0,1) : 0: use buffered RXFIFO
            ////////////////////////////////////////////////////////////////////////
            // RSSI Register Bit Positions
            RSSICCAThresh = 8,   // 8 bits (8..15) : 2's compl CCA threshold
            ////////////////////////////////////////////////////////////////////////
            // TXCtrl Register Bit Positions
            TXCtrlBufCur = 14,  // 2 bits (14,15) : Tx mixer buffer bias current
            TXCtrlTurnArnd = 13,  // wait time after STXOn before xmit
            TXCtrlVar = 11,  // 2 bits (11,12) : Varactor array settings
            TXCtrlXmitCur = 9,   // 2 bits (9,10)  : Xmit mixer currents
            TXCtrlPACur = 6,   // 3 bits (6..8)  : PA current
            TXCtrlPADiff = 5,   // 1: Diff PA, 0: Single ended PA
            TXCtrlPAPwr = 0,   // 5 bits (0..4): Output PA level
            ////////////////////////////////////////////////////////////////////////
            // RXCtrl0 Register Bit Positions
            RXCtrl0MixBufCur = 12,  // 2 bits (12,13) : Rx mixer buffer bias current
            RXCtrl0HighLNAGain = 10,  // 2 bits (10,11) : High gain, LNA current
            RXCtrl0MedLNAGain = 8,   // 2 bits (8,9)   : Med gain, LNA current
            RXCtrl0LowLNAGain = 6,   // 2 bits (6,7)   : Lo gain, LNA current
            RXCtrl0HighLNACur = 4,   // 2 bits (4,5)   : Main high LNA current
            RXCtrl0MedLNACur = 2,   // 2 bits (2,3)   : Main med  LNA current
            RXCtrl0LowLNACur = 0,   // 2 bits (0,1)   : Main low LNA current
            ////////////////////////////////////////////////////////////////////////
            // RXCtrl1 Register Bit Positions
            RXCtrl1BPFLOCur = 13,  // Ref bias current to Rx bandpass filter
            RXCtrl1BPFMIDCur = 12,  // Ref bias current to Rx bandpass filter
            RXCtrl1LowLowGain = 11,  // LAN low gain mode
            RXCtrl1MedLowGain = 10,  // LAN low gain mode
            RXCtrl1HighHGM = 9,   // Rx mixers, hi gain mode
            RXCtrl1MedHGM = 8,   // Rx mixers, hi gain mode
            RXCtrl1LNACAP = 6,   // 2 bits (6,7) Selects LAN varactor array setting
            RXCtrl1RMixTail = 4,   // 2 bits (4,5) Receiver mixer output current
            RXCtrl1RMixVCM = 2,   // 2 bits (2,3) VCM level, mixer feedback
            RXCtrl1RMixCur = 0,   // 2 bits (0,1) Receiver mixer current
            ////////////////////////////////////////////////////////////////////////
            // FSCtrl Register Bit Positions
            FSCtrlLock = 14,  // 2 bits (14,15) # of clocks for synch
            FSCtrlCALDone = 13,  // Read only, =1 if cal done since freq synth turned on
            FSCtrlCALRunning = 12,  // Read only, =1 if cal in progress
            FSCtrlLockLen = 11,  // Synch window pulse width
            FSCtrlLockStat = 10,  // Read only, = 1 if freq synthesizer is loced
            FSCtrlFreq = 0,   // 10 bits, set operating frequency
            ////////////////////////////////////////////////////////////////////////
            // SecCtrl0 Register Bit Positions
            SecCtrl0Protect = 9,  // Protect enable Rx fifo
            SecCtrl0CBCHead = 8,  // Define 1st byte of CBC-MAC
            SecCtrl0SAKeySel = 7,  // Stand alone key select
            SecCtrl0TXKeySel = 6,  // Tx key select
            SecCtrl0RXKeySel = 5,  // Rx key select
            SecCtrl0SecM = 2,  // 2 bits (2..4) # of bytes in CBC-MAX auth field
            SecCtrl0SecMode = 0,  // Security mode
            ////////////////////////////////////////////////////////////////////////
            // SecCtrl1 Register Bit Positions
            SecCtrl1TXL = 8, // 7 bits (8..14) Tx in-line security
            SecCtrl1_RXL = 0, // 7 bits (0..7)  Rx in-line security
            ////////////////////////////////////////////////////////////////////////
            // BattMon  Register Bit Positions
            BattMonOK = 6, // Read only, batter voltage OK
            BattMonEn = 5, // Enable battery monitor
            BattMonVolt = 0, // 5 bits (0..4) Battery toggle voltage
            ////////////////////////////////////////////////////////////////////////
            // IOCFG0 Register Bit Positions
            IOCfgBCN_ACCEPT = 11, // Accept all beacon frames when address recognition is enabled.
            // This bit should be set when the PAN identifier prorammed into CC2420 RAM is equal to 0xFFFF
            // and cleared otherwise. This bit is don't care when MDMCTRL0.ADR_DECODE = 0.
            // 0: Only accept beacons with source PAN identifier which matches the PAN identifier
            // programmed into the CC2420 RAM.
            // 1: Accept all beacons regardless of the PAN identifier.
            IOCfg0FIFOPol = 10, // Fifo signal polarity
            IOCfg0FIFOPPol = 9,  // FifoP signal polarity
            IOCfg0SFD = 8,  // SFD signal polarity
            IOCfg0CCAPol = 7,  // CCA signal polarity
            IOCfg0FIFOTHR = 0,  // 7 bits, (0..6) # of Rx bytes in fifo to trg fifop
            ////////////////////////////////////////////////////////////////////////
            // IOCFG1 Register Bit Positions
            IOCfg1HSSD = 10, // 2 bits (10,11) HSSD module config
            IOCfg1SFDMux = 5,  // 5 bits (5..9)  SFD multiplexer pin settings
            IOCfg1CCAMux = 0,  // 5 bits (0..4)  CCA multiplexe pin settings
            ////////////////////////////////////////////////////////////////////////
            // STATUS Bit Positions
            StatusXOsc16MStable = 6,
            StatusTXUnderflow = 5,
            StatusEncBusy = 4,
            StatusTXActive = 3,
            StatusLock = 2,
            StatusRSSIValid = 1,
        }

        #endregion

        #region Constants
        ///////////////////////////////////////////////////////////////////
        /// Internal CC2420-specific constants
        ///
        internal const byte MaxChannel = (byte)26;    // Maximum value for the radio channel
        internal const byte MinChannel = (byte)11;     // Minimum value for the radio channel.
        internal const ushort RAM_PanAddress = (ushort)0x168; // Address of the 16-bit PAN address in RAM
        internal const ushort RAM_ExtendedAddress = (ushort)0x160; // Address of the 64-bit IEEE address in RAM
        internal const ushort RAM_ShortAddress = (ushort)0x16A; // Address of the 16-bit device address in RAM
        internal const int FIFOSize = 128;                         // The size of the RX and TX FIFOs in bytes
        ///////////////////////////////////////////////////////////////////
        /// Private CC2420-specific constants
        ///
        private const byte c_RAM_RW = (byte)0x00;    // Byte select for R+W from+to RAM
        private const byte c_RAM_R = (byte)0x20;    // Byte select for R from RAM
        private const byte c_RAM_Reg_bit = (byte)0x80;    // Byte to turn on RAM/Register access via SPI bus
        private const byte c_TXFIFO_Bank = (byte)0x00;    // Byte select for the TXFIFO RAM bank
        private const byte c_RXFIFO_Bank = (byte)0x40;    // Byte select for the RXFIFO RAM bank
        private const byte c_Security_Bank = (byte)0x80;    // Byte select for the Security RAM bank

        internal const int c_resetTimeoutMS = 10;   // Time to leave chip off during a reset
        internal const int c_oscillatorTimeoutMS = 2;    // Oscillator startup time (spec says 0.86 ms)
        #endregion

        #region Instance data
        ///////////////////////////////////////////////////////////////////
        /// Private members
        ///
        private SPI _spi;                          // The SPI driver

        internal InputPort _fifoPort;                // The FIFO port
        internal InterruptPort _fifopInterrupt;      // Interrupt port for FIFOP
        internal InterruptPort _sfdInterrupt;        // Interrupt port for SFD
        internal InputPort _ccaPort;                 // The CCA port
        internal OutputPort _resetPort;              // The reset port
        internal OutputPort _powerPort;              // The power port

        private ushort _addressFiltering;          // The value to set the ADDRESS_DECODE bit of MDMCTRL0 to.   [0,1]
        private ushort _autoACK;                   // The value to set the AutoAck bit of MDMCTRL0 to.   [0,1]
        private ushort _panCoord;                  // The value to set the PAN_COORDINATOR of MDMCTRL0 to [0,1]
        private ushort _autoFCS;                   // automatic calculation of FCS
        #endregion

        #region Constructor/Destructor
        /// <summary>
        /// Create a new HALCC2420 object with configuration specified by a given CC2420PinConfig.
        /// </summary>
        /// <param name="pins">          The CC2420PinConfig for configuration     </param>
        internal HALCC2420(CC2420PinConfig pins)
        {
            bool Success = false;

            try
            {
                _spi = new SPI(new SPI.Configuration(pins.CsNPin.PinNumber,      // Chip select port
                                                        false,                        // Chip select active state
                                                        0,                            // Chip select setup time
                                                        0,                            // Chip select hold time
                                                        false,                        // Clock idle state
                                                        true,                         // Clock edge
                                                        8096,                          // Clock rate KHz (986KHz)
                                                        pins.SPI_mod                // SPI port connected to CC2420
                                                     )
                               );

                _fifoPort = new InputPort(pins.FIFOPin.PinNumber, false, pins.FIFOPin.PinMode);
                _fifopInterrupt = new InterruptPort(pins.FIFOPPin.PinNumber,
                                                      false,
                                                      pins.FIFOPPin.PinMode,
                                                      Port.InterruptMode.InterruptEdgeHigh
                                                    );
                _sfdInterrupt = new InterruptPort(pins.SFDPin.PinNumber,
                                                      false,
                                                      pins.SFDPin.PinMode,
                                                      Port.InterruptMode.InterruptEdgeLow
                                                    );

                _ccaPort = new InputPort(pins.CCAPin.PinNumber, false, pins.CCAPin.PinMode);
                _resetPort = new OutputPort(pins.ResetNPin.PinNumber, true);
                _powerPort = new OutputPort(pins.ChipPower.PinNumber, false);

                _addressFiltering = 0; //we start in promiscousmode
                _autoACK = 0;
                _panCoord = 0;
                _autoFCS = 0;

                Success = true;
            }
            finally
            {
                if (!Success) Dispose(true);
            }
        }

        /// <summary>
        /// Disposes of this HALCC2420 object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }    

        public virtual void Close()
        {
            Dispose();
        }

        protected void Dispose(bool disposing)
        {
            if (null != _fifoPort) { _fifoPort.Dispose(); _fifoPort = null; }
            if (null != _fifopInterrupt) { _fifopInterrupt.Dispose(); _fifopInterrupt = null; }
            if (null != _sfdInterrupt) { _sfdInterrupt.Dispose(); _sfdInterrupt = null; }
            if (null != _resetPort) { _resetPort.Dispose(); _resetPort = null; }
            if (null != _ccaPort) { _ccaPort.Dispose(); _ccaPort = null; }
            if (null != _powerPort) { _powerPort.Dispose(); _powerPort = null; }
            if (null != _spi)
            {
                lock (_spi)
                {
                    _spi.Dispose();
                    _spi = null;
                }
            }
        }

        #endregion Constructor/Destructor

        #region HW access
        /// <summary>
        /// Read from the CC2420 RAM.  The 9-bit RAM address consists of two
        /// parts: B1:0 (addr 15:14) and A6:0 (addr6:0) with B=bank and A=address,
        /// the remaining addr bits are masked out.
        /// </summary>
        /// <param name="addr">The address: B1:0 (addr 15:14) and A6:0 (addr6:0)               </param>
        /// <param name="len"> The number of data bytes to read                                </param>
        /// <param name="buf"> Buffer into which read bytes are stored, must have length >= len</param>
        private void ReadRAM(ushort addr, byte len, ref byte[] buf)
        {
            byte[] write = new byte[len + 2];
            int readOffset = 2;

            write[0] = (byte)((addr & 0x7F) | c_RAM_Reg_bit);
            write[1] = (byte)(((addr >> 1) & 0xC0) | c_RAM_R);
            lock (_spi)
            {
                _spi.WriteRead(write, buf, readOffset);
            }
        }

        /// <summary>
        /// Write to the CC2420 RAM.
        /// </summary>
        /// <param name="addr">The address: B1:0 (addr 15:14) and A6:0 (addr 6:0)</param>
        /// <param name="len"> The number of data bytes to write.                </param>
        /// <param name="buf"> Buffer from which to write bytes.                 </param>
        private void WriteRAM(ushort addr, byte len, ref byte[] buf)
        {
            byte[] write = new byte[len + 2];

            write[0] = (byte)((addr & 0x7F) | c_RAM_Reg_bit);
            write[1] = (byte)(((addr >> 1) & 0xC0) | c_RAM_RW);

            System.Array.Copy(buf, 0, write, 2, len);
            lock (_spi)
            {
                _spi.Write(write);
            }
        }

        /// <summary>
        /// Read from a CC2420 register.  The first byte written is the register
        /// address, with the RAM/~REG bit (bit 7) set to 0 and the R/~W bit (bit 6)
        /// set to 1; two don't-care bytes follow.  The first byte read is the status
        /// byte (discarded since readOffset = 1), the next two bytes are the 16-bit
        /// register, read MSB first.
        ///
        /// The SPI transaction looks like (X = Don't care values):
        ///        SI:  0  1 A5 A4 A3 A2 A1 A0   X   X   X   X   X ...  X  X
        ///        SO: S7 S6 S5 S4 S3 S2 S1 S0 D15 D14 D13 D12 D11 ... D1 D0
        ///
        /// </summary>
        /// <param name="reg">The address of the register to read</param>
        /// <returns>         The value read from the register   </returns>
        public ushort ReadReg(Reg reg)
        {
            byte[] read = new byte[2];
            byte[] write = new byte[] { (byte)((int)reg | 0x40), 0, 0 };
            int readOffset = 1;
            lock (_spi)
            {
                _spi.WriteRead(write, read, readOffset);
            }

            return (ushort)((read[0] << 8) + read[1]);
        }

        /// <summary>
        /// Write to a CC2420 register.
        ///
        /// The SPI transaction looks like (X = Don't care values):
        ///     SI:  0  0 A5 A4 A3 A2 A1 A0 D15 D14 D13 D12 D11 ... D1 D0
        ///     SO: S7 S6 S5 S4 S3 S2 S1 S0   X   X   X   X   X ...  X  X
        ///
        /// </summary>
        /// <param name="reg"> The address of the register to write to</param>
        /// <param name="data">The data to be written to the register </param>
        private  byte[] m_writeReg = new byte[3];

        public void WriteReg(Reg reg, ushort data)
        {
            m_writeReg[0] = (byte)reg;
            m_writeReg[1] = (byte)(data >> 8);
            m_writeReg[2] = (byte)(data);

            lock (_spi)
            {
                _spi.Write(m_writeReg);
            }
        }

        /// <summary>
        /// Send the specified command strobe to the chip.
        /// </summary>
        /// <param name="cmd">The command strobe                  </param>
        /// <returns>         A status byte returned from the chip</returns>
        private
        byte[] m_readCmd = new byte[1];
        byte[] m_writeCmd = new byte[1];

        public byte Command(Cmd cmd)
        {
            m_writeCmd[0] = (byte)cmd;
            lock (_spi)
            {
                _spi.WriteRead(m_writeCmd, m_readCmd);
            }

            return m_readCmd[0];
        }

        /// <summary>
        /// SPI WriteRead access
        /// </summary>
        /// <param name="writeBuffer">write buffer</param>
        /// <param name="readBuffer">read buffer</param>
        /// <param name="readOffset">offset into read buffer</param>
        public void WriteRead(byte[] writeBuffer, byte[] readBuffer, int readOffset)
        {
            lock (_spi)
            {
                _spi.WriteRead(writeBuffer, readBuffer, readOffset);
            }
        }

        /// <summary>
        /// SPI Write
        /// </summary>
        /// <param name="frame">content to write</param>
        public void Write(Frame frame)
        {
            lock (_spi)
            {
                _spi.WriteRead(frame.buf, 0, frame.lenData, null, 0, 0, 0);
            }
        }

        #endregion

        #region helper functions
        /// <summary>
        /// Update modem control register. Has to be called after changing any of: AddressFiltering, AutoAck, AutoFCS, PanCoordinator
        /// </summary>
        public void SetModemControl0()
        {
            WriteReg(Reg.MDMCtrl0, (ushort)
                                     ((_panCoord << (byte)Field.MDMCtrl0PANCrd) |
                                     (_addressFiltering << (byte)Field.MDMCtrl0AddrDecode) |
                                     (2 << (byte)Field.MDMCtrl0CCAHyst) |
                                     (3 << (byte)Field.MDMCtrl0CCAMode) |
                                     (_autoFCS << (byte)Field.MDMCtrl0AutoCRC) |
                                     (_autoACK << (byte)Field.MDMCtrl0AutoACK) |
                                     (2 << (byte)Field.MDMCtrl0Preabml)
                                    )
                    );
        }

        #endregion

        #region public properties
        /// <summary>
        /// extended address of device
        /// </summary>
        public UInt64 ExtendedAddress
        {
            get
            {
                byte[] buf = new byte[8];
                ReadRAM(HALCC2420.RAM_ExtendedAddress, 8, ref buf);
                UInt64 res = 0;
                for (int i = 0; i < 8; i++) // canonical byte order
                    res = (res << 8) | buf[7 - i];
                return res;
            }

            set
            {
                byte[] buf = new byte[8];
                UInt64 res = value;
                for (int i = 0; i < 8; i++)
                { // canonical byte order
                    buf[i] = (byte)(res & 0xFF);
                    res = res >> 8;
                }

                WriteRAM(HALCC2420.RAM_ExtendedAddress, 8, ref buf);
            }
        }

        /// <summary>
        /// short address of deivce
        /// </summary>
        public UInt16 ShortAddress
        {
            get
            {
                byte[] buf = new byte[2];
                ReadRAM(HALCC2420.RAM_ShortAddress, 2, ref buf);
                return (UInt16)(buf[0] | (buf[1] << 8));
            }

            set
            {
                byte[] buf = new byte[2];
                buf[0] = (byte)(value & 0xFF);
                buf[1] = (byte)(value >> 8);
                WriteRAM(HALCC2420.RAM_ShortAddress, 2, ref buf);
            }
        }

        /// <summary>
        /// Pan Id of device
        /// </summary>
        public UInt16 PanId
        {
            get
            {
                byte[] buf = new byte[2];
                ReadRAM(HALCC2420.RAM_PanAddress, 2, ref buf);
                return (UInt16)(buf[0] | (buf[1] << 8));
            }

            set
            {
                byte[] buf = new byte[2];
                buf[0] = (byte)(value & 0xFF);
                buf[1] = (byte)(value >> 8);
                WriteRAM(HALCC2420.RAM_PanAddress, 2, ref buf);

                // update BCN_ACCEPT
                ushort reg = ReadReg(HALCC2420.Reg.IOCFG0);
                ushort bit = (1 << (byte)HALCC2420.Field.IOCfgBCN_ACCEPT);
                reg &= (ushort)~(bit);
                if (0xffff == value)
                {
                    reg |= bit;
                }

                WriteReg(HALCC2420.Reg.IOCFG0, reg);
            }
        }

        /// <summary>
        /// discard frames not be received by local device. Call SetModemControl0 after changing value.
        /// </summary>
        public bool AddressFiltering
        {
            get
            {
                return _addressFiltering != 0;
            }

            set
            {
                ushort newFilter = (ushort)(value ? 1 : 0);
                _addressFiltering = newFilter;
            }
        }

        /// <summary>
        /// automatic acknowledgment of received frames. Call SetModemControl0 after changing value.
        /// </summary>
        public bool AutoAck
        {
            get
            {
                return _autoACK != 0;
            }

            set
            {
                ushort newFilter = (ushort)(value ? 1 : 0);
                _autoACK = newFilter;
            }
        }

        /// <summary>
        /// local device is the PAN coordinator. Call SetModemControl0 after changing value.
        /// </summary>
        public bool PanCoordinator
        {
            get
            {
                return _panCoord != 0;
            }

            set
            {
                ushort newFilter = (ushort)(value ? 1 : 0);
                _panCoord = newFilter;
            }
        }

        /// <summary>
        /// automatic frame checksum calculation. Call SetModemControl0 after changing value.
        /// </summary>
        public bool AutoFCS
        {
            get
            {
                return _autoFCS != 0;
            }

            set
            {
                ushort newFilter = (ushort)(value ? 1 : 0);
                _autoFCS = newFilter;
            }
        }

        #endregion public properties
    }
}


