using System;
using System.Runtime.InteropServices;
using CorDebugInterop;

namespace Microsoft.SPOT.Debugger
{
    public class CorDebugCode : ICorDebugCode
    {
        CorDebugFunction m_function;

        public CorDebugCode(CorDebugFunction function)
        {
            m_function = function;
        }


        #region ICorDebugCode Members

        int ICorDebugCode.GetAddress (out ulong pStart)
        {
            // CorDebugCode.GetAddress is not implemented
            pStart = 0;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugCode.GetEnCRemapSequencePoints (uint cMap, out uint pcMap, uint[] offsets)
        {
            // CorDebugCode.GetEnCRemapSequencePoints is not implemented
            pcMap = 0;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugCode.CreateBreakpoint (uint offset, out ICorDebugFunctionBreakpoint ppBreakpoint)
        {
            ppBreakpoint = new CorDebugFunctionBreakpoint(m_function, offset);

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugCode.GetSize (out uint pcBytes)
        {
            // CorDebugCode.GetSize is not implemented
            pcBytes = 0;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugCode.GetVersionNumber (out uint nVersion)
        {
            // CorDebugCode.GetVersionNumber is not implemented
            nVersion = 1;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugCode.GetILToNativeMapping (uint cMap, out uint pcMap, COR_DEBUG_IL_TO_NATIVE_MAP[] map)
        {
            int pbIsJustMyCode;
            pcMap = 0;

            this.m_function.ICorDebugFunction2.GetJMCStatus(out pbIsJustMyCode);

            //
            // REQUIRED FOR 'JUST MY CODE'
            // The behavior we attain by this code allows our framework libraries to be considered 
            // non-user code.  This allows exceptions caused by the user application but handled by 
            // the framework to be shown as unhandled in the debugger (unless JMC is turned off).  
            // The VS debugger considers anything with symbols and an IL mapping to be user code by
            // default.  By throwing an exception (which translates to a E_xx COM return value) we 
            // are telling the system that this is not user code.
            //
            if (!Utility.Boolean.IntToBool(pbIsJustMyCode) && cMap == 0)
            {
                return Utility.COM_HResults.E_NOTIMPL;
            }

            pcMap = 1;

            //We don't have a native mapping.  This is just a workaround to get cpde to treat this as use code,
            //to allow set next statement, and whatever else cpde relies on.
            if (map != null)
            {
                map[0].ilOffset          = (uint)CorDebugIlToNativeMappingTypes.NO_MAPPING;
                map[0].nativeStartOffset = 0xFFFFFFFF;
                map[0].nativeEndOffset   = 0xFFFFFFFF;
            }

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugCode.GetCode (uint startOffset, uint endOffset, uint cBufferAlloc, byte[] buffer, out uint pcBufferSize)
        {
            // CorDebugCode.GetCode is not implemented
            pcBufferSize = 0;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugCode.IsIL (out int pbIL)
        {
            pbIL = Utility.Boolean.TRUE;

            return Utility.COM_HResults.S_OK;
        }

        int ICorDebugCode.GetFunction (out ICorDebugFunction ppFunction)
        {
            ppFunction = m_function;

            return Utility.COM_HResults.S_OK;
        }

#endregion
    }
}
