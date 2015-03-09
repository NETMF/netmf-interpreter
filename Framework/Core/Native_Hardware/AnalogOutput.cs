using System;
using System.Runtime.CompilerServices;

namespace Microsoft.SPOT.Hardware {

    public class AnalogOutput : IDisposable
    {
        static object s_syncRoot = new Object();

        //--//

        private readonly Cpu.Pin m_pin;
        private readonly Cpu.AnalogOutputChannel m_channel;
        private double m_scale;
        private double m_offset;
        private readonly int m_precision;

        //--//

        /// <summary>
        /// Builds an instance of AnalogOutput type for the specified channel
        /// </summary>
        /// <param name="channel">The channel for the AnalogOutput</param>
        /// <param name="scale">A multiplicative factor to apply to the value written to the sensor</param>
        /// <param name="offset">A constant factor to add to the value written to the sensor</param>
        /// <param name="precisionInBits">The desired bit precision for the D/A conversion. A value of -1 indicates default precision.</param>
        public AnalogOutput(Cpu.AnalogOutputChannel channel, double scale, double offset, int precisionInBits)
        {
            m_channel = channel;

            HardwareProvider hwProvider = HardwareProvider.HwProvider;

            if (hwProvider == null) throw new InvalidOperationException();
            
            m_pin = hwProvider.GetAnalogOutputPinForChannel(channel);
            m_scale = scale;
            m_offset = offset;

            int[] availablePrecisions = hwProvider.GetAvailableAnalogOutputPrecisionInBitsForChannel(channel);
            if(precisionInBits == -1) {
                if(availablePrecisions.Length == 0) throw new InvalidOperationException();
                m_precision = availablePrecisions[0];
            } else {
                bool found = false;
                foreach(int precision in availablePrecisions) {
                    if(precisionInBits == precision) {
                        m_precision = precision;
                        found = true;
                        break;
                    }
                }
                if(!found) {
                    throw new ArgumentException();
                }
            }
            bool fReserved = false;
            try {
                lock (s_syncRoot) {
                    fReserved = Port.ReservePin(m_pin, true);
                    Initialize(channel, m_precision);
                }
            } catch {
                if (fReserved) {
                    Port.ReservePin(m_pin, false);
                }
                throw;
            }
        }

        /// <summary>
        /// Builds an instance of AnalogOutput type for the specified channel
        /// </summary>
        /// <param name="channel">The channel for the AnalogOutput</param>
        /// <param name="precisionInBits">The desired bit precision for the D/A conversion.</param>
        public AnalogOutput(Cpu.AnalogOutputChannel channel, int precisionInBits) : this(channel, 1.0, 0.0, precisionInBits)
        {
        }


        /// <summary>
        /// Builds an instance of AnalogOutput type for the specified channel.
        /// </summary>
        /// <param name="channel">The channel for the AnalogOutput</param>
        public AnalogOutput(Cpu.AnalogOutputChannel channel) : this(channel, 1.0, 0.0, -1)
        {
        }
        
        /// <summary>
        /// Destructs the instance of the AnalogOutput
        /// </summary>
        ~AnalogOutput()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes the instance of the AnalogOutput
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
        /// Gets and sets a multiplicative factor that will be applied to the value before it is written to the sensor.
        /// </summary>
        public double Scale
        {
            get { return m_scale; }
            set { m_scale = value; }
        }

        /// <summary>
        /// Gets and sets a constant offset that will be applied to the value before it is written to the sensor.
        /// </summary>
        public double Offset
        {
            get { return m_offset; }
            set { m_offset = value; }
        }

        /// <summary>
        /// Gets the precision in bits for this channel
        /// </summary>
        public int Precision
        {
            get { return m_precision; }
        }

        /// <summary>
        /// The GPIO pin chosen for the selected channel
        /// </summary>
        public Cpu.Pin Pin
        {
            get { return m_pin; }
        }

        /// <summary>
        /// Writes a level to the AnalogOutput, eventually modified by the scale and offset following the formula raw = value * scale + offset. 
        /// <param name="level">The value to be used.</param>
        /// </summary>
        public void Write(double level)
        {
            if (m_scale != 1.0) level *= m_scale;
            if (m_offset != 0.0)  level += m_offset;
            if (level < 0) level = 0; // avoid overflow
            if (level > 1) level = 1;
            WriteRaw((int)(level * ((1 << m_precision) - 1)));
        }


        /// <summary>
        /// Writes a raw level to the AnalogOutput.
        /// <param name="level">The raw D/A value</param>
        /// </summary>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern void WriteRaw(int level);

        //--//

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        protected static extern void Initialize(Cpu.AnalogOutputChannel channel, int precisionInBits);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        protected static extern void Uninitialize(Cpu.AnalogOutputChannel channel);
    }
}
