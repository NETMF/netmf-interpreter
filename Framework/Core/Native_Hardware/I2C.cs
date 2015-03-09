using System;
using System.Runtime.CompilerServices;

namespace Microsoft.SPOT.Hardware
{
    public class I2CDevice : IDisposable
    {
        public class I2CTransaction
        {
            public readonly byte[] Buffer;

            //--//

            protected I2CTransaction(byte[] buffer)
            {
                if (buffer == null)
                {
                    throw new ArgumentNullException();
                }

                this.Buffer = buffer;
            }
        }

        //--//

        sealed public class I2CReadTransaction : I2CTransaction
        {
            internal I2CReadTransaction(byte[] buffer)
                : base(buffer)
            {
            }
        }

        //--//

        sealed public class I2CWriteTransaction : I2CTransaction
        {
            internal I2CWriteTransaction(byte[] buffer)
                : base(buffer)
            {
            }
        }

        //--//

        public class Configuration
        {
            public readonly ushort Address;
            public readonly int ClockRateKhz;

            //--//

            public Configuration(ushort address, int clockRateKhz)
            {
                this.Address = address;
                this.ClockRateKhz = clockRateKhz;
            }
        }

        //--//

        [Microsoft.SPOT.FieldNoReflection]
        private object m_xAction;

        //--//

        public Configuration Config;

        //--//

        protected bool m_disposed;

        //--//

        public I2CDevice(Configuration config)
        {
            this.Config = config;

            HardwareProvider hwProvider = HardwareProvider.HwProvider;

            if (hwProvider != null)
            {
                Cpu.Pin scl;
                Cpu.Pin sda;

                hwProvider.GetI2CPins(out scl, out sda);

                if (scl != Cpu.Pin.GPIO_NONE)
                {
                    Port.ReservePin(scl, true);
                }

                if (sda != Cpu.Pin.GPIO_NONE)
                {
                    Port.ReservePin(sda, true);
                }
            }

            Initialize();

            m_disposed = false;
        }

        ~I2CDevice()
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
                        Cpu.Pin scl;
                        Cpu.Pin sda;

                        hwProvider.GetI2CPins(out scl, out sda);

                        if (scl != Cpu.Pin.GPIO_NONE)
                        {
                            Port.ReservePin(scl, false);
                        }

                        if (sda != Cpu.Pin.GPIO_NONE)
                        {
                            Port.ReservePin(sda, false);
                        }
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

        public static I2CReadTransaction CreateReadTransaction(byte[] buffer)
        {
            return new I2CReadTransaction(buffer);
        }

        public static I2CWriteTransaction CreateWriteTransaction(byte[] buffer)
        {
            return new I2CWriteTransaction(buffer);
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public int Execute(I2CTransaction[] xActions, int timeout);

        //--//

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private void Initialize();
    }
}


