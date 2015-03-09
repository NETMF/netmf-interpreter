using System;
using System.Collections.Specialized;
using System.IO;

namespace Microsoft.SPOT.Debugger
{
    [Serializable]
    public class PortDefinition_Usb : PortDefinition
    {
        public PortDefinition_Usb(string displayName, string port, ListDictionary properties)
            : base(displayName, port)
        {
            m_properties = properties;
        }

        public override object UniqueId
        {
            get
            {
                return m_properties[UsbStream.DeviceHash];
            }
        }

        public override Stream CreateStream()
        {
            try
            {
                return new AsyncUsbStream(m_port);
            }
            catch
            {
                object uniqueId = UniqueId;

                foreach (PortDefinition pd in AsyncUsbStream.EnumeratePorts())
                {
                    if (Object.Equals(pd.UniqueId, uniqueId))
                    {
                        m_properties = pd.Properties;
                        m_port = pd.Port;

                        return new AsyncUsbStream(m_port);
                    }
                }

                throw;
            }
        }
    }
}