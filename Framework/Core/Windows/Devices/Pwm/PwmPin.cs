using System;
using Windows.Devices.Pwm.Provider;

namespace Windows.Devices.Pwm
{
    public sealed class PwmPin : IDisposable
    {
        private readonly int m_pinNumber;
        private PwmController m_controller;
        private IPwmControllerProvider m_provider;
        private bool m_started = false;
        private bool m_disposed = false;
        private double m_dutyCycle = 0;
        private PwmPulsePolarity m_polarity = PwmPulsePolarity.ActiveHigh;

        internal PwmPin(PwmController controller, IPwmControllerProvider provider, int pinNumber)
        {
            m_controller = controller;
            m_provider = provider;
            m_pinNumber = pinNumber;

            m_provider.AcquirePin(pinNumber);
        }

        ~PwmPin()
        {
            Dispose(false);
        }

        public PwmController Controller
        {
            get
            {
                if (m_disposed)
                {
                    throw new ObjectDisposedException();
                }

                return m_controller;
            }
        }

        public PwmPulsePolarity Polarity
        {
            get
            {
                if (m_disposed)
                {
                    throw new ObjectDisposedException();
                }

                return m_polarity;
            }

            set
            {
                if (m_disposed)
                {
                    throw new ObjectDisposedException();
                }

                switch (value)
                {
                case PwmPulsePolarity.ActiveHigh:
                case PwmPulsePolarity.ActiveLow:
                    break;

                default:
                    throw new ArgumentException();
                }

                if (m_started)
                {
                    m_provider.SetPulseParameters(m_pinNumber, m_dutyCycle, value == PwmPulsePolarity.ActiveLow);
                    m_polarity = value;
                }
            }
        }

        public bool IsStarted
        {
            get
            {
                if (m_disposed)
                {
                    throw new ObjectDisposedException();
                }

                return m_started;
            }
        }

        public void Start()
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException();
            }

            if (!m_started)
            {
                m_provider.EnablePin(m_pinNumber);
                m_provider.SetPulseParameters(m_pinNumber, m_dutyCycle, m_polarity == PwmPulsePolarity.ActiveLow);
                m_started = true;
            }
        }

        public void Stop()
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException();
            }

            if (m_started)
            {
                m_provider.DisablePin(m_pinNumber);
                m_started = false;
            }
        }

        public double GetActiveDutyCyclePercentage()
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException();
            }

            return m_dutyCycle;
        }

        public void SetActiveDutyCyclePercentage(double dutyCyclePercentage)
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException();
            }

            if ((dutyCyclePercentage < 0) || (dutyCyclePercentage > 1))
            {
                throw new ArgumentOutOfRangeException();
            }

            if (m_started)
            {
                m_provider.SetPulseParameters(m_pinNumber, dutyCyclePercentage, m_polarity == PwmPulsePolarity.ActiveLow);
                m_dutyCycle = dutyCyclePercentage;
            }
        }

        public void Dispose()
        {
            if (!m_disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
                m_disposed = true;
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_provider.ReleasePin(m_pinNumber);
                m_controller = null;
                m_provider = null;
            }
        }
    }
}
