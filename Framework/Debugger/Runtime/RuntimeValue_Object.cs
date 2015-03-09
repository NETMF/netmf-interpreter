namespace Microsoft.SPOT.Debugger
{
    public class RuntimeValue_Object : RuntimeValue_Indirect
    {
        protected internal RuntimeValue_Object( Engine eng, WireProtocol.Commands.Debugging_Value[] array, int pos ) : base( eng, array, pos )
        {
        }
    }
}