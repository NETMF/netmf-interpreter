////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using System.Collections;
using System.Runtime.CompilerServices;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Wireless.IEEE_802_15_4.Phy;
using System.Text;

namespace Microsoft.SPOT.Wireless.IEEE_802_15_4.Phy
{
    /// <summary>
    /// Driver for the ChipCon CC2420 IEEE 802.15.4 chip.
    /// This driver implements the 802.15.4 PHY interface and some extensions to support the software MAC.
    /// </summary>
    public sealed class CC2420 : IPhy, IDisposable
    {
        ///////////////////////////////////////////////////////////////////
        // Private members
        //
        private HALCC2420 _hal;            // The hardware abstraction layer

        private bool _enabled;             // hardware is powered on

        private byte[] _rxFrame;           // Buffer to read received frame length
        private byte[] _rxReadCmd;         // The command to read from the RX FIFO

        private AutoResetEvent _rxEvent;
        private Thread _rxThread;
        private bool _rxThreadStop;

        private Frame _txFrame;            // frame currently loaded into TX buffer
        private byte[] _txWriteCmd;

        private byte _channel;             // current channel
        private int _rfPower;              // current tx power in dBm
        private bool _rxEnabled;           // rx is enabled

        /// <summary>
        /// Create and initialize a new driver instance with the specified configuration.
        /// </summary>
        /// <param name="pins">The configuration specified as chip pin settings</param>
        public CC2420(CC2420PinConfig pins)
        {
            bool Success = false;
            try
            {
                _hal = new HALCC2420(pins);
         
                _enabled = false;

                _rxFrame = new byte[1];
                _rxReadCmd = new byte[2];
                _rxReadCmd[0] = ((int)HALCC2420.Reg.RXFIFO) | 0x40;

                _rxEvent = new AutoResetEvent(false);
                _rxThreadStop = false;
                _rxThread = new Thread(ReceiveThread);
#if RTEXTENSIONS
                _rxThread.Priority = ThreadPriority.Highest; // MAC is 150
#else
                _rxThread.Priority = (ThreadPriority)151000; // fake RT priority
#endif
                _rxThread.Start();

                _txFrame = null;
                _txWriteCmd = new byte[2];
                _txWriteCmd[0] = (byte)HALCC2420.Reg.TXFIFO;

                _channel = HALCC2420.MinChannel;
                _rfPower = -1; // dBm
                _rxEnabled = false;

                Success = true;
            }
            finally
            {
                if (!Success) Dispose(true);
            }
        }

        public void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {   
            SetPower(false);

            if (_rxThread != null)
            {
                _rxThreadStop = true;
                _rxEvent.Set();
                _rxThread.Join();
            }

            if (_hal != null)
            {
                _hal.Dispose();
                _hal = null;
            }
        }

        #region helper functions
        /// <summary>
        /// Does a soft reset of the CC2420 by writing the RESETn bit of the
        /// Main register to low and then high.
        /// </summary>
        private void SWReset()
        {
            _hal.WriteReg(HALCC2420.Reg.Main, 0x0000);
            System.Threading.Thread.Sleep(HALCC2420.c_resetTimeoutMS);
            _hal.WriteReg(HALCC2420.Reg.Main, 0xF800);
        }

        /// <summary>
        /// Do a hard reset of the CC2420 by switching the power to the chip
        /// off and then on again.
        /// </summary>
        private void HWReset()
        {
            _hal._resetPort.Write(false);
            System.Threading.Thread.Sleep(HALCC2420.c_resetTimeoutMS);
            _hal._resetPort.Write(true);
        }

        /// <summary>
        /// Starts the crystal oscillator and waits for it to stabilize.
        /// </summary>
        private void StartOsc()
        {
            _hal.Command(HALCC2420.Cmd.SXOscOn);

            bool stabilized = false;

            DateTime start = DateTime.Now;
            TimeSpan to = new TimeSpan(0, 0, 0, 0, HALCC2420.c_oscillatorTimeoutMS);

            do
            {
                Thread.Sleep(HALCC2420.c_oscillatorTimeoutMS);

                byte status = _hal.Command(HALCC2420.Cmd.SNOP);

                if (((status >> (byte)HALCC2420.Field.StatusXOsc16MStable) & 0x01) != 0)
                {
                    stabilized = true;
                    break;
                }

            } while ((DateTime.Now - start) < to);

            if (!stabilized)
            {
                throw new SystemException("Unable to stabilize crystal oscillator");
            }

            // Flush buffers
            _hal.Command(HALCC2420.Cmd.SFlushTX);
            _hal.Command(HALCC2420.Cmd.SFlushRX);
            _hal.Command(HALCC2420.Cmd.SFlushRX);
        }

        /// <summary>
        /// Initialize chip configuration registers.
        /// </summary>
        public void InitRegisters()
        {
            // Modem control register 0
            // Reset all fields except for MDMCtrl0AddrDecode which is disabled.
            _hal.SetModemControl0();

            // Modem control register 1
            // Demodulator correlator threshold value should always be set to 20.
            // Reset all other fields.
            _hal.WriteReg(HALCC2420.Reg.MDMCtrl1, 20 << (byte)HALCC2420.Field.MDMCtrl1CorrThresh);

            // RSSI and CCA status register
            // Reset all fields.
            _hal.WriteReg(HALCC2420.Reg.RSSI, 0xE080);

            // Synchronization control word register
            // Reset field.
            _hal.WriteReg(HALCC2420.Reg.SyncWord, 0xA70F);

            // Transmit control register
            // Reset all fields.
            _hal.WriteReg(HALCC2420.Reg.TXCtrl,
                     ((2 << (byte)HALCC2420.Field.TXCtrlBufCur) |       // 2 is nominal - 1 is the watch value
                      (1 << (byte)HALCC2420.Field.TXCtrlTurnArnd) |
                      (3 << (byte)HALCC2420.Field.TXCtrlPACur) |
                      (1 << (byte)HALCC2420.Field.TXCtrlPADiff) |
                      (0x001F << (byte)HALCC2420.Field.TXCtrlPAPwr)
                      )
                     );

            // Receive control register 0
            // Reset all fields.
            _hal.WriteReg(HALCC2420.Reg.RXCtrl0,
                     ((1 << (byte)HALCC2420.Field.RXCtrl0MixBufCur) |
                      (2 << (byte)HALCC2420.Field.RXCtrl0HighLNAGain) |    // 2 is nominal - zero is the reset value
                      (2 << (byte)HALCC2420.Field.RXCtrl0MedLNAGain) |
                      (3 << (byte)HALCC2420.Field.RXCtrl0LowLNAGain) |
                      (2 << (byte)HALCC2420.Field.RXCtrl0HighLNACur) |
                      (1 << (byte)HALCC2420.Field.RXCtrl0MedLNACur) |
                      (1 << (byte)HALCC2420.Field.RXCtrl0LowLNACur)
                     )
                    );

            // Receive control register 1
            // Reset all fields.
            _hal.WriteReg(HALCC2420.Reg.RXCtrl1,
                     ((1 << (byte)HALCC2420.Field.RXCtrl1BPFLOCur) |    // 1 is nominal - zero is the reset value & not recommended
                      (1 << (byte)HALCC2420.Field.RXCtrl1LowLowGain) |
                      (1 << (byte)HALCC2420.Field.RXCtrl1HighHGM) |
                      (1 << (byte)HALCC2420.Field.RXCtrl1LNACAP) |
                      (1 << (byte)HALCC2420.Field.RXCtrl1RMixTail) |
                      (1 << (byte)HALCC2420.Field.RXCtrl1RMixVCM) |
                      (2 << (byte)HALCC2420.Field.RXCtrl1RMixCur)
                     )
                    );

            // Frequency synthesis control and status register
            // Reset all fields.
            _hal.WriteReg(HALCC2420.Reg.FSCtrl,
                     ((1 << (byte)HALCC2420.Field.FSCtrlLock) |
                     ((357 + 5 * (11 - HALCC2420.MinChannel)) << (byte)HALCC2420.Field.FSCtrlFreq) // Formula for RF frequency of a channel
                     )
                    );

            // Security control register 0
            // Reset all fields except SecCtrl0Protect
            _hal.WriteReg(HALCC2420.Reg.SecCtrl0,
                     ((1 << (byte)HALCC2420.Field.SecCtrl0CBCHead) |
                      (1 << (byte)HALCC2420.Field.SecCtrl0SAKeySel) |
                      (1 << (byte)HALCC2420.Field.SecCtrl0TXKeySel) |
                      (1 << (byte)HALCC2420.Field.SecCtrl0SecM)
                     )
                    );

            // Security control register 1
            // Reset all fields.
            _hal.WriteReg(HALCC2420.Reg.SecCtrl1, 0x0000);

            // Set I/O control register 0
            // Set threshold in number of bytes in RXFIFO for FIFOP to go high to max = 127.
            // Reset all other fields.
            _hal.WriteReg(HALCC2420.Reg.IOCFG0,
                (((127) << (byte)HALCC2420.Field.IOCfg0FIFOTHR) |
                (0 << (byte)HALCC2420.Field.IOCfg0CCAPol) |
                (0 << (byte)HALCC2420.Field.IOCfg0FIFOPPol) |
                (0 << (byte)HALCC2420.Field.IOCfg0FIFOPol) |
                (0 << (byte)HALCC2420.Field.IOCfg0SFD) |
                (0 << (byte)HALCC2420.Field.IOCfgBCN_ACCEPT)
                ));

            // Set I/O control register 1
            // Reset all fields.
            _hal.WriteReg(HALCC2420.Reg.IOCFG1, 0x0000);

            // Debug trace
            UInt16 u = _hal.ReadReg(HALCC2420.Reg.ManfIDH);
            Trace.Print("CC2420 version " + (u >> 12));
        }

        private void EnableInterrupts()
        {
            _hal._fifopInterrupt.OnInterrupt += new NativeEventHandler(FifopHandler);
            _hal._sfdInterrupt.OnInterrupt += new NativeEventHandler(SfdHandler);
        }

        private void DisableInterrupts()
        {
            _hal._fifopInterrupt.OnInterrupt -= new NativeEventHandler(FifopHandler);
            _hal._sfdInterrupt.OnInterrupt -= new NativeEventHandler(SfdHandler);
        }

        [Serializable]
        private class CC2420Address
        {
            public UInt64 extAddress;
        }

        private void On()
        {
            lock (_hal)
            {
                _hal._powerPort.Write(true);
                Thread.Sleep(200);

                SWReset();
                InitRegisters();
                StartOsc();
                EnableInterrupts();
                Channel = _channel;
                Power = _rfPower;
                _rxEnabled = false;

                // CC2420 ext address is a random value. Recover/generate ext address from flash if needed
                {
                    CC2420Address addr = null;
                    // try to recover address
                    ExtendedWeakReference ewr = ExtendedWeakReference.Recover(typeof(CC2420Address), 0);
                    if (addr != null)
                    {
                        // address recovered from flash
                        _hal.ExtendedAddress = addr.extAddress;
                    }
                    else
                    {
                        // save current address in flash
                        ewr = ExtendedWeakReference.RecoverOrCreate(typeof(CC2420Address), 0, ExtendedWeakReference.c_SurvivePowerdown);
                        if (ewr != null)
                        {
                            addr = new CC2420Address();
                            addr.extAddress = _hal.ExtendedAddress;
                            ewr.Priority = (int)ExtendedWeakReference.PriorityLevel.System;
                            ewr.Target = addr;
                            ewr.PushBackIntoRecoverList();
                        }
                    }
                }
            }
        }

        private void Off()
        {
            lock (_hal)
            {
                DisableInterrupts();
                _hal.Command(HALCC2420.Cmd.SXOscOff);    // Stop the oscillator
                _hal._powerPort.Write(false);
            }
        }

        private byte Channel
        {
            get { return _channel; }
            set
            {
                if (value < HALCC2420.MinChannel || value > HALCC2420.MaxChannel)
                    throw new Exception("Bad channel value");

                lock (_hal)
                {
                    _channel = value;
                    _hal.WriteReg(HALCC2420.Reg.FSCtrl,
                        (ushort)((1 << (ushort)HALCC2420.Field.FSCtrlLock) |
                        ((357 + 5 * (_channel - 11)) << (ushort)HALCC2420.Field.FSCtrlFreq))
                        );
                    if (_rxEnabled)
                    {
                        _hal.Command(HALCC2420.Cmd.SFlushRX);
                        _hal.Command(HALCC2420.Cmd.SFlushRX);
                        _hal.Command(HALCC2420.Cmd.SRXOn);
                    }
                }
            }
        }

        // mapping from PA_LEVEL to output power in dBm for PA_LEVEL 3..31
        // interpolated from CC spec, table 9
        // PaLevel / OutputPower
        //  0  -34.9609   (-32 is minimum according to PHY spec)
        //  1  -31.5625
        //  2  -28.2161
        //  3  -25.0000
        //  4  -21.9922
        //  5  -19.2708
        //  6  -16.9141
        //  7  -15.0000
        //  8  -13.4570
        //  9  -12.1354
        // 10  -10.9961
        // 11  -10.0000
        // 12   -9.1164
        // 13   -8.3313
        // 14   -7.6305
        // 15   -7.0000
        // 16   -6.4437
        // 17   -5.9500
        // 18   -5.4813
        // 19   -5.0000
        // 20   -4.5000
        // 21   -4.0000
        // 22   -3.5000
        // 23   -3.0000
        // 24   -2.4688
        // 25   -1.9167
        // 26   -1.4063
        // 27   -1.0000
        // 28   -0.6797
        // 29   -0.3958
        // 30   -0.1641
        // 31    0
        private static int[] paLevels = {
            -32, -31, -28, -25,
            -22, -19, -17, -15,
            -13, -12, -11, -10,
            -9, -8, -8, -7,
            -6, -6, -5, -5,
            -5, -4, -4, -3,
            -2, -2, -1, -1,
            -1, 0, 0, 0 };

        /// <summary>
        /// Set the output power of the radio. Range is -32..31 dBm. CC supports -25..0 dBm
        /// </summary>
        private int Power
        {
            get { return _rfPower; }
            set
            {
                lock (_hal)
                {
                    int level = 0;
                    while (level < paLevels.Length && paLevels[level] < value)
                        level++;
                    if (level >= paLevels.Length)
                        level = paLevels.Length - 1;
                    _rfPower = paLevels[level];
                    // 0..31

                    ushort txctrl = _hal.ReadReg(HALCC2420.Reg.TXCtrl);

                    // Zero-out the PAPwr field
                    ushort msk = (ushort)(0x1F << (ushort)HALCC2420.Field.TXCtrlPAPwr);
                    msk = (ushort)~msk;
                    txctrl &= msk;

                    _hal.WriteReg(HALCC2420.Reg.TXCtrl,
                                    (ushort)(txctrl | (level << (ushort)HALCC2420.Field.TXCtrlPAPwr)));
                }
            }
        }

        private bool RXEnabled
        {
            get { return _rxEnabled; }
            set
            {
                if (_rxEnabled != value)
                {
                    _rxEnabled = value;
                    lock (_hal)
                    {
                        if (_rxEnabled)
                        {
                            _hal.Command(HALCC2420.Cmd.SFlushRX);
                            _hal.Command(HALCC2420.Cmd.SFlushRX);      // Flush any junk left in the RX FIFO
                            _hal.Command(HALCC2420.Cmd.SRXOn);         // Turn the receiver on
                        }
                        else
                        {
                            _hal.Command(HALCC2420.Cmd.SRFOff);         // Turn the receiver on
                        }
                    }
                }
            }
        }

        private byte GetNormalizedRSSI()
        {
            ushort uval = _hal.ReadReg(HALCC2420.Reg.RSSI);
            SByte sval = (SByte)uval; // bits 7:0, signed
            int i = sval;
            // expect -60..40
            if (i < -60)
                i = -60;
            if (i > 40)
                i = 40;
            // shift to 0..100
            i += 60;
            return (byte)((i * 255) / 100);
        }

        #endregion helper functions

        #region IPHY_EXT_SAP
        /// <summary>
        /// Get required header space for data frames to be sent
        /// </summary>
        /// <param name="head">required space for header</param>
        /// <param name="tail">required space for tail</param>
        public void GetMtuSize(out int mtu, out int head, out int tail)
        {
            mtu = 127;
            head = 2;
            tail = 0;
        }

        /// <summary>
        /// Get extended device address
        /// </summary>
        public void GetDeviceAddress(out UInt64 address)
        {
            lock (_hal)
            {
                address = _hal.ExtendedAddress;
            }
        }

        /// <summary>
        /// Test if radio supports given capability
        /// </summary>
        /// <param name="cap">the requested capability</param>
        /// <param name="supported">true if capability is supported</param>
        public void IsCapabilitySupported(Capabilities cap, out bool supported)
        {
            switch (cap)
            {
                case Capabilities.PowerOff:
                case Capabilities.AutoFcs:
                case Capabilities.AddressFilter:
                case Capabilities.AutoAck:
                case Capabilities.TxCache:
                    supported = true;
                    break;
                default:
                    supported = false;
                    break;
            }
        }

        /// <summary>
        /// radio can be powered off
        /// </summary>
        /// <param name="enable"></param>
        public void SetPower(bool enable)
        {
            if (enable == _enabled)
                return;

            if (enable)
            {
                On();
            }
            else
            {
                Off();
            }

            _enabled = enable;
        }

        /// <summary>
        /// radio can calculate/check MAC FCS. MAC layer to send/receive dummy FCS values.
        /// </summary>
        /// <param name="enable"></param>
        public void SetAutoFCS(bool enable)
        {
            lock (_hal)
            {
                _hal.AutoFCS = enable;
                _hal.SetModemControl0();
            }
        }

        /// <summary>
        /// radio can filter for MAC addresses
        /// </summary>
        /// <param name="enable"></param>
        /// <param name="shortAddr"></param>
        /// <param name="panId"></param>
        /// <param name="panCoordinator"></param>
        public void SetAddressFilter(
            bool enable,
            UInt16 shortAddr,
            UInt16 panId,
            bool panCoordinator)
        {
            lock (_hal)
            {
                if (enable)
                {
                    _hal.ShortAddress = shortAddr;
                    _hal.PanId = panId;
                    _hal.PanCoordinator = panCoordinator;
                    _hal.AddressFiltering = true;
                }
                else
                {
                    _hal.AddressFiltering = false;
                }

                _hal.SetModemControl0();
            }
        }

        /// <summary>
        /// radio can ack frames, required address filter to be set
        /// </summary>
        /// <param name="enable"></param>
        public void SetAutoAck(bool enable)
        {
            _hal.AutoAck = enable;
            _hal.SetModemControl0();
        }

        /// <summary>
        /// If the radio has an internal TX buffer, the previously sent frame is cached in the buffer.
        /// This enables efficient retransmissions. However, when the frame is returned to the frame pool and used to sent another frame,
        /// the PHY layer might be unable to detect that the frame content has changed. Use this function to mark the TX buffer as invalid.
        /// </summary>
        public void ClearTxBuffer()
        {
            _txFrame = null; // tx buffer is "empty"
        }

        #endregion IPHY_EXT_SAP

        #region IPHY_DATA_SAP
        #region receive
        /// <summary>
        /// Not needed. If disabled, FifoP interrupt is not served!
        /// </summary>
        /// <param name="port"></param>
        /// <param name="state"></param>
        /// <param name="time"></param>
        private void SfdHandler(uint a, uint b, DateTime time)
        {
        }

        ///////////////////////////////////////////////////////////////////
        // receive
        //
        /// <summary>
        /// Event handler for FIFOP interrupt events.
        /// Reads out the next frame in the RXFIFO, puts it in the <code>NewFrames</code>
        /// queue and signals that a new frame has been added.  If there was an RXFIFO
        /// overflow then the RXFIFO is flushed.
        /// </summary>
        /// <param name="port"> The port generating the interrupt     </param>
        /// <param name="state">True if pin is high, false if it's low</param>
        /// <param name="time"> Time passed since system boot         </param>
        private void FifopHandler(uint a, uint b, DateTime time)
        {
            _rxEvent.Set();

        }

        public void DataPoll()
        {
            _rxEvent.Set();
        }

        private string GetHex(byte[] data, int maxCount)
        {
            char[] c = new char[data.Length * 3];
            int cntToPrint = data.Length > maxCount ? maxCount : data.Length;
            for (int i = 0; i < cntToPrint; i++)
            {
                c[i * 3 + 1] = (char)(((data[i] & 0xF) > 9) ? ((data[i] & 0xF) + 'a' - 10) : ((data[i] & 0xF) + '0'));
                c[i * 3] = (char)(((data[i] >> 4) > 9) ? ((data[i] >> 4) + 'a' - 10) : ((data[i] >> 4) + '0'));
                c[i * 3 + 2] = ' ';
            }

            return new string(c);
        }

        private void ReceiveThread()
        {
            while (!_rxThreadStop)
            {
                _rxEvent.WaitOne();

                lock (_hal)
                {
                    while (_hal._fifopInterrupt.Read())
                    {
                        if (_hal._fifoPort.Read())
                        {
                            // get frame length, rxFrame is single byte
                            _hal.WriteRead(_rxReadCmd, _rxFrame, 1);
                            byte len = _rxFrame[0];
                            if (len > 0 && len < HALCC2420.FIFOSize)
                            {
                                // get frame
                                byte[] buf = new byte[len];
                                _hal.WriteRead(_rxReadCmd, buf, 1);

                                Frame frame = Frame.GetFrame(2, len + 1); // len + LQI byte. Reserve 2 bytes for header in case this exact frame is resent later on.
                                frame.WriteToBack(buf);
                                // buf = null; // dispose

                                bool ok = true;
                                int lqi = 0;

                                if (_hal.AutoFCS)
                                {
                                    // since we are using AutoCRC, MAC FCS is replaced with:
                                    // - RSSI (first byte)
                                    // - LQI (7 bits, lsb)
                                    // - CRC ok (1 bit, msb)
                                    // LQI needs to be scaled from 50..110 to 0..255 [CC.24]
                                    int i = frame.ReadByte(len - 1);
                                    if ((i & 0x80) == 0 || len < 3)
                                    { // CRC error
                                        ok = false;
                                    }
                                    else
                                    { // calculate LQI
                                        i &= 0x7f; // should be 50..110
                                        if (i < 50)
                                            i = 50;
                                        if (i > 110)
                                            i = 110;
                                        // is 50..110
                                        i -= 50;
                                        // 0..60
                                        lqi = (i * 255) / 60;
                                        // 0..255 (well, within that range at least)

                                        frame.DeleteFromBack(2); // remove FCS
                                    }
                                }
                                else
                                {
                                    lqi = GetNormalizedRSSI();
                                }

                                if (ok)
                                {
                                    DataIndicationHandler ind = _DataIndication;
                                    if (ind == null || frame.LengthDataUsed == 0)
                                    {
                                        Frame.Release(ref frame);
                                    }
                                    else
                                    {
                                        try
                                        {
                                            ind.Invoke(this, frame, (byte)lqi);

#if DEBUG
                                            {
                                                DateTime curTime = DateTime.Now;
                                                Debug.Print("Get Frm " + buf[2] + " Len " + len.ToString() + " Data " + ":" + GetHex(buf, 16) + " at " + curTime.Second + ":" + curTime.Millisecond);
                                            }
#endif
                                        }
                                        catch (Exception) { }
                                    }
                                }
                                else
                                {
                                    Frame.Release(ref frame);
                                }
                            }
                            else
                            {
                                _hal.Command(HALCC2420.Cmd.SFlushRX);
                                _hal.Command(HALCC2420.Cmd.SFlushRX);
                            }
                        }
                        else // !m_hal.m_fifoPort.Read()
                        {
                            _hal.Command(HALCC2420.Cmd.SFlushRX);
                            _hal.Command(HALCC2420.Cmd.SFlushRX);
                        }
                    }
                }
            }
        }

        private DataIndicationHandler _DataIndication;
        public DataIndicationHandler DataIndication
        {
            get { return _DataIndication; }
            set { _DataIndication = value; }
        }

        #endregion receive

        #region send
        /// <summary>
        /// (6.2.1.1-2) The PD-DATA.request primitive requests the transfer of an MPDU (i.e., PSDU)
        /// from the MAC sublayer to the local PHY entity.
        /// </summary>
        /// <param name="frame">the PSDU to be transmitted by the PHY entity. Frame is not released by PHY layer!</param>
        /// <param name="Status">the results of the operation</param>
        public void DataRequest(
            Frame frame,
            out Status status)
        {
            Status result = Status.Success;
            lock (_hal)
            {
                // radio is Busy, potentially ongoing tx
                if (_hal._sfdInterrupt.Read())
                {
                    result = Status.Busy;
                }
                else
                {
                    WriteTXFIFO(frame);
                    _hal.Command(HALCC2420.Cmd.STXOnCCA);
                }
            }

            status = result;
        }

        /// <summary>
        /// Write to the TXFIFO.
        /// </summary>
        /// <param name="len">The number of bytes to write</param>
        /// <param name="buf">The buffer to write from    </param>
        private void WriteTXFIFO(Frame frame)
        {
            if (frame.LengthHeaderAvail < 2)
                throw new ArgumentException();

            lock (_hal)
            {
                int len = frame.LengthDataUsed; // including fcs when not using autoFcs
                if (_hal.AutoFCS)
                    len += 2;
                _txWriteCmd[1] = (byte)len;
                frame.AppendToFront(_txWriteCmd, 0, 2);
                _hal.Write(frame);
                frame.DeleteFromFront(2);
                _txFrame = frame;
            }
        }

        #endregion send
        #endregion IPHY_DATA_SAP

        #region IPHY_MGMT

        /// <summary>
        /// (6.2.2.1-2) The PLME-CCA.request primitive requests that the PLME perform a CCA as defined in 6.9.9.
        /// </summary>
        /// <param name="Status"></param>
        public void CCARequest(
            out Status status)
        {
            lock (_hal)
            {
                if (!_rxEnabled)
                {
                    status = Status.TRxOff;
                }
                else
                {
                    if (!_hal._ccaPort.Read()) // inverted
                        status = Status.Busy;
                    else
                        status = Status.Idle;
                }
            }
        }

        /// <summary>
        /// (6.2.2.3-4) The PLME-ED.request primitive requests that the PLME perform an ED measurement (see 6.9.7).
        /// </summary>
        /// <param name="status">The result of the request to perform an ED measurement.</param>
        /// <param name="energyLevel">ED level for the current channel. If status is set to Success, this is
        /// the ED level for the current channel. Otherwise, the value of this parameter will be ignored.</param>
        public void EDRequest(
            out Status status,
            out Byte energyLevel)
        {
            lock (_hal)
            {
                if (!_rxEnabled)
                {
                    status = Status.TRxOff;
                    energyLevel = 0;
                }
                else
                {
                    byte st = _hal.Command(HALCC2420.Cmd.SNOP);
                    byte StatusRSSIValid = (byte)(1 << (int)HALCC2420.Field.StatusRSSIValid);
                    if ((st & StatusRSSIValid) == 0)
                    {
                        status = Status.Busy;
                        energyLevel = 0;
                    }
                    else
                    {
                        status = Status.Success;
                        energyLevel = GetNormalizedRSSI();
                    }
                }
            }
        }

        /// <summary>
        /// (6.2.2.5-6) The PLME-GET.request primitive requests information about a given PHY PIB attribute.
        /// </summary>
        /// <param name="attribute">The identifier of the PHY PIB attribute to get.</param>
        /// <param name="status">The result of the request for PHY PIB attribute information.</param>
        /// <param name="attributeValue">The value of the indicated PHY PIB attribute that was requested. This
        /// parameter has zero length when the status parameter is set to UNSUPPORTED_ATTRIBUTE.</param>
        public void GetRequest(
            PibAttribute attribute,
            out Status status,
            out PibValue attributeValue)
        {
            attributeValue = new PibValue();
            switch (attribute)
            {
                case PibAttribute.phyCurrentChannel:
                    status = Status.Success;
                    attributeValue.Int = Channel;
                    break;
                case PibAttribute.phyChannelsSupported:
                    {
                        int res = 0;
                        for (int i = HALCC2420.MinChannel; i <= HALCC2420.MaxChannel; i++)
                            res |= (1 << i);
                        // CC2420 only supports channel page 0
                        int[] channels = new int[1];
                        channels[0] = res;
                        status = Status.Success;
                        attributeValue.IntArray = channels;
                        break;
                    }
                case PibAttribute.phyTransmitPower:
                    status = Status.Success;
                    attributeValue.Int = Power;
                    break;
                case PibAttribute.phyCCAMode:
                    status = Status.Success;
                    attributeValue.Int = 3; // hard-coded in hal, could be changed
                    break;
                case PibAttribute.phyCurrentPage:
                    status = Status.Success;
                    attributeValue.Int = 0;
                    break;
                case PibAttribute.phyMaxFrameDuration:
                    // see 6.4.2 and below
                    status = Status.Success;
                    attributeValue.Int = 1064;
                    break;
                case PibAttribute.phySHRDuration:
                    // SHR is 4+1 byte, BPSK (5*8)
                    status = Status.Success;
                    attributeValue.Int = 40;
                    break;
                case PibAttribute.phySymbolsPerOctet:
                    status = Status.Success;
                    attributeValue.Float = 8; // BPSK only
                    break;
                default:
                    status = Status.UnsupportedAttr;
                    break;
            }
        }

        /// <summary>
        /// (6.2.2.9-10) The PLME-SET.request primitive attempts to set the indicated PHY PIB attribute to the given value.
        /// </summary>
        /// <param name="attribute">The identifier of the PIB attribute to set.</param>
        /// <param name="attributeValue">The value of the indicated PIB attribute to set.</param>
        /// <param name="status">The status of the attempt to set the requested PIB attribute.</param>
        public void SetRequest(
            PibAttribute attribute,
            PibValue attributeValue,
            out Status status)
        {
            switch (attribute)
            {
                case PibAttribute.phyCurrentChannel:
                    {
                        if (attributeValue.Type != PibValue.ValueType.Int)
                        {
                            status = Status.InvalidParam;
                        }
                        else
                        {
                            int value = attributeValue.Int;
                            if (value < HALCC2420.MinChannel || value > HALCC2420.MaxChannel)
                            {
                                status = Status.InvalidParam;
                            }
                            else
                            {
                                Channel = (byte)value;
                                status = Status.Success;
                            }
                        }
                        break;
                    }
                case PibAttribute.phyTransmitPower:
                    {
                        if (attributeValue.Type != PibValue.ValueType.Int)
                        {
                            status = Status.InvalidParam;
                        }
                        else
                        {
                            int value = attributeValue.Int;
                            if (value < -32 || value > 31) // dBm
                            {
                                status = Status.InvalidParam;
                            }
                            else
                            {
                                Power = value;
                                status = Status.Success;
                            }
                        }
                        break;
                    }
                case PibAttribute.phyCurrentPage:
                    {
                        if (attributeValue.Type != PibValue.ValueType.Int)
                        {
                            status = Status.InvalidParam;
                        }
                        else
                        {
                            int value = attributeValue.Int;
                            if (value == 0)
                                status = Status.Success;
                            else
                                status = Status.InvalidParam;
                        }
                        break;
                    }
                case PibAttribute.phyChannelsSupported:
                case PibAttribute.phyCCAMode:
                case PibAttribute.phyMaxFrameDuration:
                case PibAttribute.phySHRDuration:
                case PibAttribute.phySymbolsPerOctet:
                    status = Status.ReadOnly;
                    break;
                default:
                    status = Status.InvalidParam;
                    break;
            }
        }

        /// <summary>
        /// (6.2.2.7-8) The PLME-SET-TRX-STATE.request primitive requests that the PHY entity change the internal
        /// operating state of the transceiver.
        /// </summary>
        /// <param name="state">The new state in which to configure the transceiver.</param>
        /// <param name="status">The result of the request to change the state of the transceiver.</param>
        public void SetTrxStateRequest(
            State state,
            out Status status)
        {
            switch (state)
            {
                // this receiver can enable RX while TX is active
                case State.RxOn:
                    RXEnabled = true;
                    status = Status.RxOn;
                    break;
                case State.TxOn:
                    // nothing to do
                    status = Status.TxOn;
                    break;
                case State.TRxOff:
                case State.ForceTRxOff:
                    RXEnabled = false;
                    status = Status.TRxOff;
                    break;
                default:
                    status = Status.InvalidParam;
                    break;
            }
        }

        #endregion
    }
}


