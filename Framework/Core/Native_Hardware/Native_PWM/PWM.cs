using System;
using Microsoft.SPOT.Hardware;
using System.Runtime.CompilerServices;

namespace Microsoft.SPOT.Hardware
{
    /// <summary>
    /// type for PWM port
    /// </summary>
    public class PWM : IDisposable
    {
        public enum ScaleFactor : uint
        {
            Milliseconds = 1000,
            Microseconds = 1000000,
            Nanoseconds  = 1000000000,
        }

        //--//
        
        /// <summary>
        /// The pin used for this PWM port, can be set only when the port is constructed
        /// </summary>
        private readonly Cpu.Pin m_pin;
        /// <summary>
        /// The channel used for this PWM port, can be set only when the port is constructed
        /// </summary>
        private readonly Cpu.PWMChannel m_channel;
        /// <summary>
        /// The period of the PWM in microseconds
        /// </summary>
        private uint m_period;
        /// <summary>
        /// The Duty Cycle of the PWM wave in microseconds
        /// </summary>
        private uint m_duration;
        /// <summary>
        /// Polarity of the wave, it determines the idle state of the port
        /// </summary>
        private bool m_invert;
        /// <summary>
        /// Scale of the period/duration (mS, uS, nS)
        /// </summary>
        private ScaleFactor m_scale;
        /// <summary>
        /// Whether this object has been disposed.
        /// </summary>
        private bool m_disposed = false;

        //--//

        /// <summary>
        /// Build an instance of the PWM type
        /// </summary>
        /// <param name="channel">The channel to use</param>
        /// <param name="frequency_Hz">The frequency of the pulse in Hz</param>
        /// <param name="dutyCycle">The duty cycle of the pulse as a fraction of unity.  Value should be between 0.0 and 1.0</param>
        /// <param name="invert">Whether the output should be inverted or not</param>
        public PWM(Cpu.PWMChannel channel, double frequency_Hz, double dutyCycle, bool invert)
        {
            HardwareProvider hwProvider = HardwareProvider.HwProvider;

            if(hwProvider == null) throw new InvalidOperationException();

            m_pin = hwProvider.GetPwmPinForChannel(channel);
            m_channel = channel;
            //--//
            m_period = PeriodFromFrequency(frequency_Hz, out m_scale);
            m_duration = DurationFromDutyCycleAndPeriod(dutyCycle, m_period);
            m_invert = invert;
            //--//
            try
            {
                Init();
                
                Commit();
                
                Port.ReservePin(m_pin, true);
            }
            catch
            {
                Dispose(false);
            }
        }

        /// <summary>
        /// Build an instance of the PWM type
        /// </summary>
        /// <param name="channel">The channel</param>
        /// <param name="period">The period of the pulse</param>
        /// <param name="duration">The duration of the pulse.  The value should be a fraction of the period</param>
        /// <param name="scale">The scale factor for the period/duration (nS, uS, mS)</param>
        /// <param name="invert">Whether the output should be inverted or not</param>
        public PWM(Cpu.PWMChannel channel, uint period, uint duration, ScaleFactor scale, bool invert)
        {
            HardwareProvider hwProvider = HardwareProvider.HwProvider;

            if (hwProvider == null) throw new InvalidOperationException();

            m_pin      = hwProvider.GetPwmPinForChannel(channel);
            m_channel  = channel;
            //--//
            m_period   = period;
            m_duration = duration;
            m_scale    = scale;
            m_invert   = invert;
            //--//
            try
            {
                Init();
                
                Commit();
                
                Port.ReservePin(m_pin, true);
            }
            catch
            {
                Dispose(false);
            }
        }

        /// <summary>
        /// Finalizer for the PWM type, will stop the port if still running and un-reserve the underlying pin
        /// </summary>
        ~PWM()
        {
            Dispose(false);
        }

        /// <summary>
        /// Diposes the PWM type, will stop the port if still running and un-reserve the PIN
        /// </summary>
        public void Dispose()
        {
            if (!m_disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
                m_disposed = true;
            }
        }

        /// <summary>
        /// Starts the PWM port for an indefinite amount of time
        /// </summary>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public void Start();

        /// <summary>
        /// Stop the PWM port
        /// </summary>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public void Stop();

        //--//

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
        /// Gets and sets the frequency (in Hz) of the pulse
        /// </summary>
        public double Frequency
        {
            get
            {
                return FrequencyFromPeriod(m_period, m_scale);
            }
            set
            {
                // save the duty cycle so that we can preserve it after changing the period
                double dutyCycle = DutyCycle;
                
                m_period = PeriodFromFrequency(value, out m_scale);

                // restore the proper duty cycle
                m_duration = DurationFromDutyCycleAndPeriod(dutyCycle, m_period);
                
                Commit();
                //--//
            }
        }


        /// <summary>
        /// Gets and sets the duty cycle of the pulse as a fraction of unity. Value should be included between 0.0 and 1.0
        /// </summary>
        public double DutyCycle
        {
            get
            {
                return DutyCycleFromDurationAndPeriod(m_period, m_duration);
            }
            set
            {
                m_duration = DurationFromDutyCycleAndPeriod(value, m_period);
                Commit();
                //--//
            }
        }


        /// <summary>
        /// Gets and sets the period of the pulse
        /// </summary>
        public uint Period
        {
            get
            {
                return m_period;
            }
            set
            {
                m_period = value;
                Commit();
                //--//
            }
        }


        /// <summary>
        /// Gets and sets the duration of the pulse.  The Value should be a fraction of the period.
        /// </summary>
        public uint Duration
        {
            get
            {
                return m_duration;
            }
            set
            {
                m_duration = value;
                Commit();
                //--//
            }
        }

        /// <summary>
        /// Gets or sets the scale factor for the Duration and Period.  Setting the Scale does not cause 
        /// an immediate update to the PWM.   The update occurs when Duration or Period are set.
        /// </summary>
        public ScaleFactor Scale
        {
            get
            {
                return m_scale;
            }
            set
            {
                m_scale = value;
                Commit();
                //--//
            }
        }
        

        //--//

        /// <summary>
        /// Starts a number of PWM ports at the same time
        /// </summary>
        /// <param name="ports"></param>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public static void Start(PWM[] ports);

        /// <summary>
        /// Stops a number of PWM ports at the same time
        /// </summary>
        /// <param name="ports"></param>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public static void Stop(PWM[] ports);

        //--//

        protected void Dispose(bool disposing)
        {
            try
            {
                Stop();
            }
            catch
            {
                // hide all exceptions...
            }
            finally
            {
                Uninit();

                Port.ReservePin(m_pin, false);
            }
        }

        /// <summary>
        /// Moves values to the HAL
        /// </summary>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern protected void Commit();

        /// <summary>
        /// Initializes the controller
        /// </summary>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern protected void Init();

        /// <summary>
        /// Uninitializes the controller
        /// </summary>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern protected void Uninit();

        //--//

        private static uint PeriodFromFrequency(double f, out ScaleFactor scale)
        {
            if(f >= 1000.0)
            {
                scale = ScaleFactor.Nanoseconds;
                return (uint)(((uint)ScaleFactor.Nanoseconds / f) + 0.5);
            }
            else if(f >= 1.0)
            {
                scale = ScaleFactor.Microseconds;
                return (uint)(((uint)ScaleFactor.Microseconds / f) + 0.5);
            }
            else
            {
                scale = ScaleFactor.Milliseconds;
                return (uint)(((uint)ScaleFactor.Milliseconds / f) + 0.5);
            }
        }

        private static uint DurationFromDutyCycleAndPeriod(double dutyCycle, double period)
        {
            if (period <= 0)
                throw new ArgumentException();

            if (dutyCycle < 0)
                return 0;

            if (dutyCycle > 1)
                return 1;

            return (uint)(dutyCycle * period);            
        }

        private static double FrequencyFromPeriod(double period, ScaleFactor scale)
        {
            return ((uint)scale / period);
        }

        private static double DutyCycleFromDurationAndPeriod(double period, double duration)
        {
            if (period <= 0)
                throw new ArgumentException();

            if (duration < 0)
                return 0;

            if (duration > period)
                return 1;

            return (duration / period);
        }
    }
}

