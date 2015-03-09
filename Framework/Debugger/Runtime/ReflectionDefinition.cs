namespace Microsoft.SPOT.Debugger
{
    public class ReflectionDefinition
    {
        public enum Kind : ushort
        {
            REFLECTION_INVALID      = 0x00,
            REFLECTION_ASSEMBLY     = 0x01,
            REFLECTION_TYPE         = 0x02,
            REFLECTION_TYPE_DELAYED = 0x03,
            REFLECTION_CONSTRUCTOR  = 0x04,
            REFLECTION_METHOD       = 0x05,
            REFLECTION_FIELD        = 0x06,
        };

        public ushort m_kind;
        public ushort m_levels;
    
        public uint   m_raw;
    }
}