using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

namespace Microsoft.SPOT.Debugger
{
    [Serializable]

    public class PortDefinition_WinUsb : PortDefinition
    {
        public PortDefinition_WinUsb(string displayName, string port, ListDictionary properties)
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
                return new WinUsb_AsyncUsbStream(m_port);
            }
            catch( IOException )
            {
                var uniqueId = UniqueId;

                foreach (var pd in WinUsb_AsyncUsbStream.EnumeratePorts())
                {
                    if( !Object.Equals( pd.UniqueId, uniqueId ) )
                        continue;

                    m_properties = pd.Properties;
                    m_port = pd.Port;

                    return new WinUsb_AsyncUsbStream(m_port);
                }
                throw new FileNotFoundException( "Device not found", UniqueId.ToString( ) );
            }
        }
    }
}