using System;
using System.Runtime.CompilerServices;

namespace Microsoft.SPOT.Hardware
{
    public sealed class SPI : IDisposable
    {
        public enum SPI_module : int
        {
            SPI1 = 0,
            SPI2 = 1,
            SPI3 = 2,
            SPI4 = 3,
        }

        public class Configuration
        {
            public readonly Cpu.Pin ChipSelect_Port;
            public readonly bool ChipSelect_ActiveState;
            public readonly uint ChipSelect_SetupTime;
            public readonly uint ChipSelect_HoldTime;
            public readonly bool Clock_IdleState;
            public readonly bool Clock_Edge;
            public readonly uint Clock_RateKHz;
            public readonly SPI_module SPI_mod;
            public readonly Cpu.Pin BusyPin;
            public readonly bool    BusyPin_ActiveState;

            public Configuration(
                                  Cpu.Pin ChipSelect_Port,
                                  bool ChipSelect_ActiveState,
                                  uint ChipSelect_SetupTime,
                                  uint ChipSelect_HoldTime,
                                  bool Clock_IdleState,
                                  bool Clock_Edge,
                                  uint Clock_RateKHz,
                                  SPI_module SPI_mod
                                ) : this( ChipSelect_Port,
                                          ChipSelect_ActiveState,
                                          ChipSelect_SetupTime,
                                          ChipSelect_HoldTime,
                                          Clock_IdleState,
                                          Clock_Edge,
                                          Clock_RateKHz,
                                          SPI_mod,
                                          Cpu.Pin.GPIO_NONE,
                                          false)
                                          
                                
            {
            }
            
            public Configuration(
                                  Cpu.Pin ChipSelect_Port,
                                  bool ChipSelect_ActiveState,
                                  uint ChipSelect_SetupTime,
                                  uint ChipSelect_HoldTime,
                                  bool Clock_IdleState,
                                  bool Clock_Edge,
                                  uint Clock_RateKHz,
                                  SPI_module SPI_mod,
                                  Cpu.Pin BusyPin,
                                  bool BusyPin_ActiveState
                                  
                                )
            {
                this.ChipSelect_Port = ChipSelect_Port;
                this.ChipSelect_ActiveState = ChipSelect_ActiveState;
                this.ChipSelect_SetupTime = ChipSelect_SetupTime;
                this.ChipSelect_HoldTime = ChipSelect_HoldTime;
                this.Clock_IdleState = Clock_IdleState;
                this.Clock_Edge = Clock_Edge;
                this.Clock_RateKHz = Clock_RateKHz;
                this.SPI_mod = SPI_mod;
                this.BusyPin = BusyPin;
                this.BusyPin_ActiveState = BusyPin_ActiveState;
            }
        }

        //--//

        private Configuration m_config;
        private OutputPort m_cs;
        private bool m_disposed;

        //--//

        public SPI(Configuration config)
        {
            HardwareProvider hwProvider = HardwareProvider.HwProvider;

            if (hwProvider != null)
            {
                Cpu.Pin msk;
                Cpu.Pin miso;
                Cpu.Pin mosi;

                hwProvider.GetSpiPins(config.SPI_mod, out msk, out miso, out mosi);

                if (msk != Cpu.Pin.GPIO_NONE)
                {
                    Port.ReservePin(msk, true);
                }

                if (miso != Cpu.Pin.GPIO_NONE)
                {
                    Port.ReservePin(miso, true);
                }

                if (mosi != Cpu.Pin.GPIO_NONE)
                {
                    Port.ReservePin(mosi, true);
                }
            }

            if (config.ChipSelect_Port != Cpu.Pin.GPIO_NONE)
            {
                m_cs = new OutputPort(config.ChipSelect_Port, !config.ChipSelect_ActiveState);
            }

            m_config = config;
            m_disposed = false;
        }

        ~SPI()
        {
            Dispose(false);
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        private void Dispose(bool fDisposing)
        {
            if (!m_disposed)
            {
                try
                {
                    HardwareProvider hwProvider = HardwareProvider.HwProvider;

                    if (hwProvider != null)
                    {
                        Cpu.Pin msk;
                        Cpu.Pin miso;
                        Cpu.Pin mosi;

                        hwProvider.GetSpiPins(m_config.SPI_mod, out msk, out miso, out mosi);

                        if (msk != Cpu.Pin.GPIO_NONE)
                        {
                            Port.ReservePin(msk, false);
                        }

                        if (miso != Cpu.Pin.GPIO_NONE)
                        {
                            Port.ReservePin(miso, false);
                        }

                        if (mosi != Cpu.Pin.GPIO_NONE)
                        {
                            Port.ReservePin(mosi, false);
                        }
                    }

                    if (m_config.ChipSelect_Port != Cpu.Pin.GPIO_NONE)
                    {
                        m_cs.Dispose();
                    }
                }
                finally
                {
                    m_disposed = true;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        public void WriteRead(ushort[] writeBuffer, int writeOffset, int writeCount, ushort[] readBuffer, int readOffset, int readCount, int startReadOffset)
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException();
            }

            if (
                // write buffer can never be null
                (writeBuffer == null) ||
                // write buffer must be larger than the sum of the offset and the count for writing from it
                (writeOffset + writeCount > writeBuffer.Length) ||
                // read buffer must be larger than the offset and the count for writing from it
                ((readBuffer != null) && (readOffset + readCount > readBuffer.Length)) 
               ) 
            {
                throw new ArgumentException();
            }

            InternalWriteRead(writeBuffer, writeOffset, writeCount, readBuffer, readOffset, readCount, startReadOffset);
        }

        public void WriteRead(ushort[] writeBuffer, ushort[] readBuffer, int startReadOffset)
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException();
            }

            if (writeBuffer == null) 
            {
                throw new ArgumentException();
            }

            int readBufLen = 0;
            
            if (readBuffer != null)
            {
                readBufLen = readBuffer.Length;
            }

            InternalWriteRead(writeBuffer, 0, writeBuffer.Length, readBuffer, 0, readBufLen, startReadOffset);
        }

        public void WriteRead(ushort[] writeBuffer, ushort[] readBuffer)
        {
            if (readBuffer == null)
            {
                throw new ArgumentException();
            }

            WriteRead(writeBuffer, readBuffer, 0);
        }

        public void Write(ushort[] writeBuffer)
        {
            WriteRead(writeBuffer, null, 0 );
        }

        public void WriteRead(byte[] writeBuffer, int writeOffset, int writeCount, byte[] readBuffer, int readOffset, int readCount, int startReadOffset)
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException();
            }

            if (
                // write buffer can never be null
                (writeBuffer == null) ||
                // write buffer must be larger than the sum of the offset and the count for writing from it
                (writeOffset + writeCount > writeBuffer.Length) ||
                // read buffer must be larger than the offset and the count for writing from it
                ((readBuffer != null) && (readOffset + readCount > readBuffer.Length)) 
               )
            {
                throw new ArgumentException();
            }

            InternalWriteRead(writeBuffer, writeOffset, writeCount, readBuffer, readOffset, readCount, startReadOffset);
        }

        public void WriteRead(byte[] writeBuffer, byte[] readBuffer, int startReadOffset)
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException();
            }

            if (writeBuffer == null) 
            {
                throw new ArgumentException();
            }
            int readBufLen = 0;
            
            if (readBuffer != null)
            {
                readBufLen = readBuffer.Length;
            }

            InternalWriteRead(writeBuffer, 0, writeBuffer.Length, readBuffer, 0, readBufLen, startReadOffset);
        }

        public void WriteRead(byte[] writeBuffer, byte[] readBuffer)
        {
            if (readBuffer == null)
            {
                throw new ArgumentException();
            }

            WriteRead(writeBuffer, readBuffer, 0);
        }

        public void Write(byte[] writeBuffer)
        {
            WriteRead(writeBuffer, null, 0);
        }

        public Configuration Config
        {
            get
            {
                return m_config;
            }

            set
            {
                m_config = value;
            }
        }

        //--//
        /// <summary>
        ///  Writes specified number of bytes from writeBuffer to SPI bus. Reads data from SPI bus and places into readBuffer.
        ///  writeBuffer     - array with data to be written to SPI bus.
        ///  writeElemCount  - number of elements to write to SPI bus. If writeElemCount is -1, then all data is array is written to bus.
        ///  readBuffer      - buffer to place data read from SPI bus
        ///  readOffset      - Number of elements to skip before starting to read.
        /// </summary>
        /// <param name="writeBuffer"></param>
        /// <param name="writeElemCount"></param>
        /// <param name="readBuffer"></param>
        /// <param name="readOffset"></param>

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private void InternalWriteRead(ushort[] writeBuffer, int writeOffset, int writeCount, ushort[] readBuffer, int readOffset, int readCount, int startReadOffset);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private void InternalWriteRead(byte[] writeBuffer  , int writeOffset, int writeCount, byte[] readBuffer  , int readOffset, int readCount, int startReadOffset);
    }
}


