//-----------------------------------------------------------------------------
//
//                   ** WARNING! ** 
//    This file was generated automatically by a tool.
//    Re-running the tool will overwrite this file.
//    You should copy this file to a custom location
//    before adding any customization in the copy to
//    prevent loss of your changes when the tool is
//    re-run.
//
//-----------------------------------------------------------------------------



#include "security_pkcs11.h"



HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiRNG::GenerateRandom___VOID__SZARRAY_U1__I4__I4__BOOLEAN( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock*       pThis    = stack.This();
    CLR_RT_HeapBlock*       pSession;
    CLR_RT_HeapBlock_Array* pData    = stack.Arg1().DereferenceArray();
    CLR_INT32               offset   = stack.Arg2().NumericByRef().s4;
    CLR_INT32               len      = stack.Arg3().NumericByRef().s4;
    bool                    fNonZero = stack.Arg4().NumericByRef().s4 == 1;
    CK_SESSION_HANDLE       hSession;
    CLR_UINT8*              pDataElem;

    FAULT_ON_NULL_ARG(pData);

    if(len+offset > (CLR_INT32)pData->m_numOfElements) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);

    pSession = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_SessionContainer::FIELD__m_session].Dereference(); FAULT_ON_NULL(pSession);
    hSession = pSession[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_handle].NumericByRef().s4;

    pDataElem = pData->GetElement(offset);
    
    CRYPTOKI_CHECK_RESULT(stack, C_GenerateRandom(hSession, pDataElem, len));

    if(fNonZero)
    {
        int i,idx = -1;
        CLR_UINT8 replacements[20];

        for(i=0; i<len; i++)
        {
            if(*pDataElem == 0)
            {
                if(idx == -1 || idx >= ARRAYSIZE(replacements))
                {
                    CRYPTOKI_CHECK_RESULT(stack, C_GenerateRandom(hSession, replacements, ARRAYSIZE(replacements)));
                    idx = 0;
                }

                *pDataElem = replacements[idx++];
            }
        }
    }

    TINYCLR_NOCLEANUP();
}

HRESULT Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_CryptokiRNG::SeedRandom___VOID__SZARRAY_U1( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock*       pThis    = stack.This();
    CLR_RT_HeapBlock_Array* pData    = stack.Arg1().DereferenceArray();
    CLR_RT_HeapBlock*       pSession;
    CK_SESSION_HANDLE       hSession;

    FAULT_ON_NULL_ARG(pData);

    pSession = pThis[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_SessionContainer::FIELD__m_session].Dereference(); FAULT_ON_NULL(pSession);
    hSession = pSession[Library_security_pkcs11_native_Microsoft_SPOT_Cryptoki_Session::FIELD__m_handle].NumericByRef().s4;

    CRYPTOKI_CHECK_RESULT(stack, C_SeedRandom(hSession, pData->GetFirstElement(), pData->m_numOfElements));

    TINYCLR_NOCLEANUP();
}
