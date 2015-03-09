using System;
using System.Text;

namespace Microsoft.SPOT.Debugger
{
    public class RuntimeValue_String : RuntimeValue
    {
        internal string m_value;

        protected internal RuntimeValue_String( Engine eng, WireProtocol.Commands.Debugging_Value handle ) : base( eng, handle )
        {
            byte[] buf = handle.m_builtinValue;

            if(handle.m_bytesInString >= buf.Length)
            {
                if(m_eng.ReadMemory( m_handle.m_charsInString, m_handle.m_bytesInString, out buf ) == false)
                {
                    // Revert to the preview on failure
                    buf = handle.m_builtinValue;
                }
            }

            m_value = WireProtocol.Commands.GetZeroTerminatedString( buf, true );
        }

        public override bool IsReference { get { return false; } }
        public override bool IsNull      { get { return false; } }
        public override bool IsPrimitive { get { return false; } }
        public override bool IsValueType { get { return false; } }
        public override bool IsArray     { get { return false; } }
        public override bool IsReflection{ get { return false; } }

        public override object Value
        {
            get
            {
                return m_value;
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        internal override void SetStringValue( string val )
        {
            byte[] buf = Encoding.UTF8.GetBytes( val );

            if(buf.Length != m_handle.m_bytesInString)
            {
                throw new ArgumentException( "String must have same length" );
            }

            if(m_eng.WriteMemory( m_handle.m_charsInString, buf ) == false)
            {
                throw new ArgumentException( "Cannot write string" );
            }

            m_value = val;
        }
    }
}