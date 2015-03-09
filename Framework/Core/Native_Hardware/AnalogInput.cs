using System;
using System.Runtime.CompilerServices;

namespace Microsoft.SPOT.Hardware
{
    public class AnalogInput : IDisposable
    {
        static object s_syncRoot = new Object();

        //--//

        private readonly Cpu.Pin m_pin;
        private readonly Cpu.AnalogChannel m_channel;
        private double m_scale;
        private double m_offset;
        private readonly int m_precision;
        private bool m_disposed;

        //--//

        /// <summary>
        /// Builds an instance of AnalogInput type for the specified channel
        /// </summary>
        /// <param name="channel">The channel for the AnalogInput</param>
        /// <param name="scale">A multiplicative factor to apply to the raw reading from the sensor</param>
        /// <param name="offset">A constant factor to add to the raw reading from the sensor</param>
        /// <param name="precisionInBits">The desired bit precision for the A/D conversion. A value of -1 indicates default precision.</param>
        public AnalogInput(Cpu.AnalogChannel channel, double scale, double offset, int precisionInBits) 
        {
            m_channel = channel;
            
            HardwareProvider hwProvider = HardwareProvider.HwProvider;

            if(hwProvider == null) throw new InvalidOperationException();
            
            m_pin = hwProvider.GetAnalogPinForChannel(channel);
            m_scale = scale;
            m_offset = offset;

            int[] availablePrecisions = hwProvider.GetAvailablePrecisionInBitsForChannel(channel);
            if(precisionInBits == -1)
            {
                if(availablePrecisions.Length == 0) throw new InvalidOperationException();
                
                m_precision = availablePrecisions[0];
            }
            else
            {
                bool found = false;
                foreach(int precision in availablePrecisions)
                {
                    if(precisionInBits == precision)
                    {
                        m_precision = precision;
                        found = true;
                        break;
                    }
                }

                if(!found)
                {
                    throw new ArgumentException();
                }
            }

            bool fReserved = false;
            try
            {
                lock(s_syncRoot)
                {
                    fReserved = Port.ReservePin(m_pin, true);
                    Initialize(channel, m_precision);
                }
            }
            catch
            {
                if (fReserved)
                {
                    Port.ReservePin(m_pin, false);
                }
                throw;
            }
        }

        /// <summary>
        /// Builds an instance of AnalogInput type for the specified channel
        /// </summary>
        /// <param name="channel">The channel for the AnalogInput</param>
        /// <param name="precisionInBits">The desired bit precision for the A/D conversion.</param>
        public AnalogInput(Cpu.AnalogChannel channel, int precisionInBits) : this(channel, 1.0, 0.0, precisionInBits)
        {
        }


        /// <summary>
        /// Builds an instance of AnalogInput type for the specified channel.
        /// </summary>
        /// <param name="channel">The channel for the AnalogInput</param>
        public AnalogInput(Cpu.AnalogChannel channel) : this(channel, 1.0, 0.0, -1)
        {
        }
        
        /// <summary>
        /// Destructs the instance of the AnalogInput
        /// </summary>
        ~AnalogInput()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes the instance of the AnalogInput
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        private void Dispose(bool fDisposing)
        {
            Port.ReservePin(m_pin, false);

            Uninitialize(m_channel);
        }

        /// <summary>
        /// Gets and sets a multiplicative factor that will be applied to the raw sensor reading before thw value is returned
        /// </summary>
        public double Scale
        {
            get
            {
                return m_scale;
            }
            set
            {
                m_scale = value;
            }
        }

        /// <summary>
        /// Gets and sets a constant factor that will be applied to the raw sensor reading before thw value is returned
        /// </summary>
        public double Offset
        {
            get
            {
                return m_offset;
            }
            set
            {
                m_offset = value;
            }
        }

        /// <summary>
        /// Gets the precision in bits for this channel
        /// </summary>
        public int Precision 
        {
            get
            {
                return m_precision;
            }
        }

        /// <summary>
        /// The GPIO pin chosen for the selected channel
        /// </summary>
        public Cpu.Pin Pin
        {
            get
            {
                return m_pin;
            }
        }

        /// <summary>
        /// Reads from the AnalogInput and returns the reading, eventually modified by the multiplicative and constant factor following the formula result = raw * scale + K 
        /// </summary>
        /// <returns></returns>
        public double Read()
        {
            double raw = ReadRaw() / (double)((1 << m_precision)-1);
                 
            if(m_scale != 1.0)
            {
                raw *= m_scale;
            }
            if(m_offset != 0.0)
            {
                raw += m_offset;
            }
      
            return raw;
        }

        /// <summary>
        /// Reads from the AnalogInput and returns the raw reading.
        /// </summary>
        /// <returns>The raw AD value</returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern int ReadRaw();

        //--//

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        protected static extern void Initialize(Cpu.AnalogChannel channel, int precisionInBits);     
        
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        protected static extern void Uninitialize(Cpu.AnalogChannel channel);        
    }
}


