using System;
using Windows.Devices.Pwm.Provider;

namespace Windows.Devices.Pwm
{
    public sealed class PwmController
    {
        private IPwmControllerProvider m_provider;

        internal PwmController(IPwmControllerProvider provider)
        {
            m_provider = provider;
        }

        public int PinCount
        {
            get
            {
                return m_provider.PinCount;
            }
        }

        public double MinFrequency
        {
            get
            {
                return m_provider.MinFrequency;
            }
        }

        public double MaxFrequency
        {
            get
            {
                return m_provider.MaxFrequency;
            }
        }

        public double ActualFrequency
        {
            get
            {
                return m_provider.ActualFrequency;
            }
        }

        public static PwmController[] GetControllers(IPwmProvider provider)
        {
            // FUTURE: This should return "Task<IReadOnlyList<PwmController>>"

            var providers = provider.GetControllers();
            var controllers = new PwmController[providers.Length];

            for (int i = 0; i < providers.Length; ++i)
            {
                controllers[i] = new PwmController(providers[i]);
            }

            return controllers;
        }

        public double SetDesiredFrequency(double desiredFrequency)
        {
            if ((desiredFrequency < m_provider.MinFrequency) || (desiredFrequency > m_provider.MaxFrequency))
            {
                throw new ArgumentOutOfRangeException();
            }

            return m_provider.SetDesiredFrequency(desiredFrequency);
        }

        public PwmPin OpenPin(int pinNumber)
        {
            if ((pinNumber < 0) || (pinNumber >= m_provider.PinCount))
            {
                throw new ArgumentOutOfRangeException();
            }

            return new PwmPin(this, m_provider, pinNumber);
        }
    }
}
