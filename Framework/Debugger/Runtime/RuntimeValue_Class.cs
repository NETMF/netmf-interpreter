namespace Microsoft.SPOT.Debugger
{
    public class RuntimeValue_Class : RuntimeValue
    {
        protected internal RuntimeValue_Class( Engine eng, WireProtocol.Commands.Debugging_Value handle ) : base( eng, handle )
        {
        }

        public override bool IsReference { get { return false; } }
        public override bool IsNull      { get { return false; } }
        public override bool IsPrimitive { get { return false; } }
        public override bool IsValueType { get { return false; } }
        public override bool IsArray     { get { return false; } }
        public override bool IsReflection{ get { return false; } }

        public override uint NumOfFields
        {
            get
            {
                return m_handle.m_size - 1;
            }
        }

        public override RuntimeValue GetField( uint offset, uint fd )
        {
            return m_eng.GetFieldValue( this, offset, fd );            
        }
    }
}