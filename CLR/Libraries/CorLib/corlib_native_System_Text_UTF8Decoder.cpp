////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "CorLib.h"


HRESULT Library_corlib_native_System_Text_UTF8Decoder::Convert___VOID__SZARRAY_U1__I4__I4__SZARRAY_CHAR__I4__I4__BOOLEAN__BYREF_I4__BYREF_I4__BYREF_BOOLEAN( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_UTF8_DECODER();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Array* pArrayBytes;
    CLR_INT32               byteIndex;
    CLR_INT32               byteCount;
    CLR_RT_HeapBlock_Array* pArrayChars;
    CLR_INT32               charIndex;
    CLR_INT32               charCount;
    CLR_INT32               byteUsed;
    CLR_INT32               charUsed;
    bool                    completed;

    CLR_RT_UnicodeHelper uh;
    CLR_UINT8* byteStart;

    // Get all the parameters
    pArrayBytes = stack.Arg1().DereferenceArray(); FAULT_ON_NULL(pArrayBytes);
    byteIndex   = stack.Arg2().NumericByRef().s4;
    byteCount   = stack.Arg3().NumericByRef().s4;

    pArrayChars = stack.Arg4(  ).DereferenceArray(); FAULT_ON_NULL(pArrayChars);
    charIndex   = stack.ArgN( 5 ).NumericByRef().s4;
    charCount   = stack.ArgN( 6 ).NumericByRef().s4;

    // Parameters error checking
    if (byteIndex < 0 || 
        byteCount < 0 || 
        charIndex < 0 || 
        charCount < 0 ||
        (CLR_INT32)pArrayBytes->m_numOfElements - byteIndex < byteCount ||
        (CLR_INT32)pArrayChars->m_numOfElements - charIndex < charCount )
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }

    _ASSERTE(pArrayBytes->m_typeOfElement == DATATYPE_U1);
    _ASSERTE(pArrayChars->m_typeOfElement == DATATYPE_CHAR);

    _ASSERTE(stack.ArgN( 8 ).DataType()  == DATATYPE_BYREF);
    _ASSERTE(stack.ArgN( 9 ).DataType()  == DATATYPE_BYREF);
    _ASSERTE(stack.ArgN( 10 ).DataType() == DATATYPE_BYREF);


    // Setup the UnicodeHelper
    byteStart = pArrayBytes->GetElement( byteIndex );
    uh.SetInputUTF8( (LPSTR)byteStart );

    uh.m_outputUTF16      = (CLR_UINT16*)pArrayChars->GetElement( charIndex );
    uh.m_outputUTF16_size = charCount;

    uh.ConvertFromUTF8( charCount, false, byteCount );

    // Calculate return values
    byteUsed  = (CLR_INT32)(uh.m_inputUTF8 - byteStart);
    charUsed  = charCount - uh.m_outputUTF16_size;
    completed = (byteUsed == byteCount);

    stack.ArgN(8).Dereference()->SetInteger( byteUsed );
    stack.ArgN(9).Dereference()->SetInteger( charUsed );
    stack.ArgN(10).Dereference()->SetBoolean( completed );

    TINYCLR_NOCLEANUP();
}
