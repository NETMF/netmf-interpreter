namespace Microsoft.SPOT.Debugger
{
    public class RuntimeValue_ValueType : RuntimeValue_Class
    {
        protected internal RuntimeValue_ValueType( Engine eng, WireProtocol.Commands.Debugging_Value handle ) : base( eng, handle )
        {
        }

        public override bool IsReference { get { return false; } }
        public override bool IsNull      { get { return false; } }
        public override bool IsPrimitive { get { return false; } }
        public override bool IsValueType { get { return true ; } }
        public override bool IsArray     { get { return false; } }
        public override bool IsReflection{ get { return false; } }
    }
}