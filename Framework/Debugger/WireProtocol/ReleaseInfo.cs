using System;
using System.Text;

namespace Microsoft.SPOT.Debugger.WireProtocol
{
    public class ReleaseInfo : IConverter
    {
        public VersionStruct m_version;
        public byte[] m_info;

        public ReleaseInfo()
        {
            m_info = new byte[64 - 8];
        }

        public void PrepareForDeserialize(int size, byte[] data, Converter converter)
        {
            m_info = new byte[64 - 8];
        }

        public Version Version
        {
            get { return m_version.Version; }
        }

        public string Info
        {
            get
            {
                return Encoding.UTF8.GetString(m_info);
            }
        }
    }
}