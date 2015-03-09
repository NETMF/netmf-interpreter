using System;
using System.IO;

namespace Microsoft.SPOT.Debugger
{
    [Serializable]
    public class PortDefinition_Serial : PortDefinition
    {
        uint m_baudRate;

        public PortDefinition_Serial( string displayName, string port, uint baudRate ) : base(displayName, port)
        {
            m_baudRate = baudRate;
        }

        public uint BaudRate
        {
            get
            {
                return m_baudRate;
            }

            set
            {
                m_baudRate = value;
            }
        }

        public override Stream CreateStream()
        {
            return new AsyncSerialStream( m_port, m_baudRate );
        }

        public override string PersistName
        {
            get { return m_displayName; }
        }
    }
}