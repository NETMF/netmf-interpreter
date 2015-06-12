using System;
using Windows.Devices.Adc.Provider;

namespace Windows.Devices.Adc
{
    public sealed class AdcChannel : IDisposable
    {
        private readonly int m_channelNumber;
        private AdcController m_controller;
        private IAdcControllerProvider m_provider;
        private bool m_disposed = false;

        internal AdcChannel(AdcController controller, IAdcControllerProvider provider, int channelNumber)
        {
            m_controller = controller;
            m_provider = provider;
            m_channelNumber = channelNumber;

            m_provider.AcquireChannel(channelNumber);
        }

        ~AdcChannel()
        {
            Dispose(false);
        }

        public AdcController Controller
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

        public int ReadValue()
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException();
            }

            return m_provider.ReadValue(m_channelNumber);
        }

        public double ReadRatio()
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException();
            }

            return ((double)m_provider.ReadValue(m_channelNumber)) / m_provider.MaxValue;
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
                m_provider.ReleaseChannel(m_channelNumber);
                m_controller = null;
                m_provider = null;
            }
        }
    }
}
