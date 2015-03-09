using System;

namespace Microsoft.SPOT.Debugger
{
    public class RuntimeValue_ByRef : RuntimeValue_Indirect
    {
        protected internal RuntimeValue_ByRef( Engine eng, WireProtocol.Commands.Debugging_Value[] array, int pos ) : base( eng, array, pos )
        {
            if(m_value == null && m_handle.m_arrayref_referenceID != 0)
            {
                m_value = m_eng.GetArrayElement( m_handle.m_arrayref_referenceID, m_handle.m_arrayref_index );
            }
         
            if(m_value == null)
            {
                throw new ArgumentException();
            }            
        }

        public override bool IsReference { get { return true          ; } }
        public override bool IsNull      { get { return m_value.IsNull; } }
    }
}