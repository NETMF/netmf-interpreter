namespace Windows.Devices.Gpio
{
    public sealed class GpioPinValueChangedEventArgs
    {
        // Construction and destruction

        internal GpioPinValueChangedEventArgs(GpioPinEdge edge)
        {
            m_edge = edge;
        }

        // Public properties

        public GpioPinEdge Edge
        {
            get
            {
                return m_edge;
            }
        }

        // Private fields

        private GpioPinEdge m_edge;
    }
}
