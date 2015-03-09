namespace Microsoft.SPOT.Debugger
{
    public class RuntimeValue_Internal : RuntimeValue
    {
        protected internal RuntimeValue_Internal( Engine eng, WireProtocol.Commands.Debugging_Value handle ) : base( eng, handle )
        {
        }

        public override bool IsReference { get { return false; } }
        public override bool IsNull      { get { return false; } }
        public override bool IsPrimitive { get { return false; } }
        public override bool IsValueType { get { return false; } }
        public override bool IsArray     { get { return false; } }
        public override bool IsReflection{ get { return false; } }
    }
}