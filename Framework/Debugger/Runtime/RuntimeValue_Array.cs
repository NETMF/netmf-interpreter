namespace Microsoft.SPOT.Debugger
{
    public class RuntimeValue_Array : RuntimeValue
    {
        protected internal RuntimeValue_Array( Engine eng, WireProtocol.Commands.Debugging_Value handle ) : base( eng, handle )
        {
        }

        public override bool IsReference { get { return false; } }
        public override bool IsNull      { get { return false; } }
        public override bool IsPrimitive { get { return false; } }
        public override bool IsValueType { get { return false; } }
        public override bool IsArray     { get { return true ; } }
        public override bool IsReflection{ get { return false; } }

        public override RuntimeValue GetElement( uint index )
        {
            return m_eng.GetArrayElement( m_handle.m_referenceID, index );            
        }

        public override uint Length { get { return m_handle.m_array_numOfElements; } }
        public override uint Depth  { get { return m_handle.m_array_depth        ; } }
        public override uint Type   { get { return m_handle.m_array_typeIndex    ; } }

    }
}