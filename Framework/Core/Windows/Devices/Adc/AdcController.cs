using System;
using Windows.Devices.Adc.Provider;

namespace Windows.Devices.Adc
{
    public sealed class AdcController
    {
        private IAdcControllerProvider m_provider;

        internal AdcController(IAdcControllerProvider provider)
        {
            m_provider = provider;
        }

        public int ChannelCount
        {
            get
            {
                return m_provider.ChannelCount;
            }
        }

        public int ResolutionInBits
        {
            get
            {
                return m_provider.ResolutionInBits;
            }
        }

        public int MinValue
        {
            get
            {
                return m_provider.MinValue;
            }
        }

        public int MaxValue
        {
            get
            {
                return m_provider.MaxValue;
            }
        }

        public AdcChannelMode ChannelMode
        {
            get
            {
                return (AdcChannelMode)m_provider.ChannelMode;
            }

            set
            {
                switch (value)
                {
                case AdcChannelMode.Differential:
                case AdcChannelMode.SingleEnded:
                    break;

                default:
                    throw new ArgumentException();
                }

                m_provider.ChannelMode = (ProviderAdcChannelMode)value;
            }
        }

        public static AdcController[] GetControllers(IAdcProvider provider)
        {
            // FUTURE: This should return "Task<IVectorView<AdcController>>"

            var providers = provider.GetControllers();
            var controllers = new AdcController[providers.Length];

            for (int i = 0; i < providers.Length; ++i)
            {
                controllers[i] = new AdcController(providers[i]);
            }

            return controllers;
        }

        public bool IsChannelModeSupported(AdcChannelMode channelMode)
        {
            switch (channelMode)
            {
            case AdcChannelMode.Differential:
            case AdcChannelMode.SingleEnded:
                break;

            default:
                throw new ArgumentException();
            }

            return m_provider.IsChannelModeSupported((ProviderAdcChannelMode)channelMode);
        }

        public AdcChannel OpenChannel(int channelNumber)
        {
            if ((channelNumber < 0) || (channelNumber >= m_provider.ChannelCount))
            {
                throw new ArgumentOutOfRangeException();
            }

            return new AdcChannel(this, m_provider, channelNumber);
        }
    }
}
