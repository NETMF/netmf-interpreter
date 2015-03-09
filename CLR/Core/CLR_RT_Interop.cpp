////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Core.h"


/**********************************************************************
** Macro: TINYCLR_INTEROP_CHECK_ARG_TYPE 
**  This macro is specific for Interop_Marshal_ functions. 
**  It checks that data type corresponds to the data type stored in heapblock 
**  in parameter paramIndex.
**  In release mode macro does nothing.
**********************************************************************/


// For the final product we disable checking of the argument types
// TINYCLR_INTEROP_CHECK_ARG_TYPE is reduced to nothing.
#define TINYCLR_INTEROP_CHECK_ARG_TYPE( arg_type ) 
#define TINYCLR_INTEROP_NOCLEANUP()                return S_OK;
 

HRESULT Interop_Marshal_bool( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, bool &param )
{   
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_INTEROP_CHECK_ARG_TYPE(DATATYPE_I4);
// The comparison with 0 converts numeric type "u1" to boolean type.
    param = stackFrame.ArgN( paramIndex ).NumericByRef().u1 != 0;
    TINYCLR_INTEROP_NOCLEANUP();
}

HRESULT Interop_Marshal_UINT8( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, UINT8 &param )         
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_INTEROP_CHECK_ARG_TYPE(DATATYPE_I4);
    param = stackFrame.ArgN( paramIndex ).NumericByRef().u1;
    TINYCLR_INTEROP_NOCLEANUP();
}

HRESULT Interop_Marshal_UINT16( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, UINT16 &param )         
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_INTEROP_CHECK_ARG_TYPE(DATATYPE_I4)
    param = stackFrame.ArgN( paramIndex ).NumericByRef().u2;
    TINYCLR_INTEROP_NOCLEANUP();
}

HRESULT Interop_Marshal_UINT32( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, UINT32 &param )         
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_INTEROP_CHECK_ARG_TYPE(DATATYPE_I4)
    param = stackFrame.ArgN( paramIndex ).NumericByRef().u4;
    TINYCLR_INTEROP_NOCLEANUP();
}

HRESULT Interop_Marshal_UINT64( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, UINT64 &param )         
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_INTEROP_CHECK_ARG_TYPE(DATATYPE_I8);
    param = stackFrame.ArgN( paramIndex ).NumericByRef().u8;
    TINYCLR_INTEROP_NOCLEANUP();
}

HRESULT Interop_Marshal_CHAR( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CHAR &param )         
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_INTEROP_CHECK_ARG_TYPE(DATATYPE_I4)
    param = stackFrame.ArgN( paramIndex ).NumericByRef().s1;
    TINYCLR_INTEROP_NOCLEANUP();
}

HRESULT Interop_Marshal_INT8( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, INT8 &param )         
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_INTEROP_CHECK_ARG_TYPE(DATATYPE_I4)
    param = stackFrame.ArgN( paramIndex ).NumericByRef().s1;
    TINYCLR_INTEROP_NOCLEANUP();
}

HRESULT Interop_Marshal_INT16( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, INT16 &param )         
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_INTEROP_CHECK_ARG_TYPE(DATATYPE_I4);
    param = stackFrame.ArgN( paramIndex ).NumericByRef().s2;
    TINYCLR_INTEROP_NOCLEANUP();
}

HRESULT Interop_Marshal_INT32( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, INT32 &param )         
{
    NATIVE_PROFILE_CLR_CORE();
   TINYCLR_INTEROP_CHECK_ARG_TYPE(DATATYPE_I4);
    param = stackFrame.ArgN( paramIndex ).NumericByRef().s4;
    TINYCLR_INTEROP_NOCLEANUP();
}

HRESULT Interop_Marshal_INT64( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, INT64 &param )         
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_INTEROP_CHECK_ARG_TYPE(DATATYPE_I8);
    param = stackFrame.ArgN( paramIndex ).NumericByRef().s8;
    TINYCLR_INTEROP_NOCLEANUP();
}

#if !defined(TINYCLR_EMULATED_FLOATINGPOINT)

HRESULT Interop_Marshal_float( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, float &param )         
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_INTEROP_CHECK_ARG_TYPE(DATATYPE_R4);
    param = stackFrame.ArgN( paramIndex ).NumericByRef().r4;
    TINYCLR_INTEROP_NOCLEANUP();
}

HRESULT Interop_Marshal_double( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, double &param )         
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_INTEROP_CHECK_ARG_TYPE(DATATYPE_R8);
    param = stackFrame.ArgN( paramIndex ).NumericByRef().r8;
    TINYCLR_INTEROP_NOCLEANUP();
}
#else
HRESULT Interop_Marshal_float( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, INT32 &param )         
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_INTEROP_CHECK_ARG_TYPE(DATATYPE_R4);
    param = stackFrame.ArgN( paramIndex ).NumericByRef().r4;
    TINYCLR_INTEROP_NOCLEANUP();
}

HRESULT Interop_Marshal_double( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, INT64 &param )         
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_INTEROP_CHECK_ARG_TYPE(DATATYPE_R8);
    param = stackFrame.ArgN( paramIndex ).NumericByRef().r8;
    TINYCLR_INTEROP_NOCLEANUP();
}

#endif

HRESULT Interop_Marshal_LPCSTR( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, LPCSTR &param )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
    // Check the type of passed varialble - should be object for strings.
    if ( stackFrame.ArgN( paramIndex ).DataType() != DATATYPE_OBJECT ) 
    {   TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);                      
    } 

    // Checks that the string was actually present in managed code, not a NULL reference to sting.
    param = stackFrame.ArgN( paramIndex ).RecoverString();  FAULT_ON_NULL(param);

    TINYCLR_NOCLEANUP();
}

// For unsupported types we set param to zero and always return S_OK.
HRESULT Interop_Marshal_UNSUPPORTED_TYPE( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, UNSUPPORTED_TYPE &param )
{
    NATIVE_PROFILE_CLR_CORE();
    param = NULL;
    return S_OK;
}

#undef TINYCLR_INTEROP_CHECK_ARG_TYPE
/**********************************************************************
**
** Function: Interop_Marshal_NUMERIC_ARRAY   
**
** Synopsis: This function is used marshal arrays of basic types created by managed code.
**           The native code can access and update array data. 
**           Thus the data can be exchanged between managed and native code in both directions.
**           The native code cannot change size of array.
**
** Arguments: [stackFrame]           - Reference to the managed stack frame. 
**            [paramIndex]           - Index of parameter passed from managed code. This parameter will be updated now.
**            [pByteParam]           - Reference of pointer to buffer with array data. Filled by the function
**            [arraySize]            - Count of elements in array. Filled by the function
**            [elementSize]          - Size of array element in bytes.
**
** Returns:   S_OK on success or error from StoreToReference. Error return would cause exception thrown in managed code.
**********************************************************************/
template <class T>    
static HRESULT Interop_Marshal_NUMERIC_ARRAY  
( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, T *&pByteParam, UINT32 &arraySize, UINT32 elementSize )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Array *pHeapBlockArray = NULL;
    
    if ( stackFrame.ArgN( paramIndex ).DataType() != DATATYPE_OBJECT )
    {   
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE); 
    }
    
    pHeapBlockArray = stackFrame.ArgN( paramIndex ).DereferenceArray();   FAULT_ON_NULL(pHeapBlockArray);
  
    if ( pHeapBlockArray->m_sizeOfElement != elementSize )
    {   
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }
    
    arraySize  = pHeapBlockArray->m_numOfElements;
    pByteParam = (T *)pHeapBlockArray->GetFirstElement();

    TINYCLR_NOCLEANUP();
}

/**********************************************************************
** Group of function that retrive array of values from managed stack frame.  
** All these functions are wrappers around Interop_Marshal_NUMERIC_ARRAY,
** they create type safe interface for users of Interop library.
**********************************************************************/
HRESULT Interop_Marshal_bool_ARRAY( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray_UINT8 &typedArray )
{
    NATIVE_PROFILE_CLR_CORE();
    return Interop_Marshal_NUMERIC_ARRAY<UINT8>( stackFrame, paramIndex, typedArray.m_pData, typedArray.m_ElemCount, sizeof( UINT8 ) );
}

HRESULT Interop_Marshal_UINT8_ARRAY( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray_UINT8 &typedArray )
{
    NATIVE_PROFILE_CLR_CORE();
    return Interop_Marshal_NUMERIC_ARRAY<UINT8>( stackFrame, paramIndex, typedArray.m_pData, typedArray.m_ElemCount, sizeof( UINT8 ) );
}

HRESULT Interop_Marshal_UINT16_ARRAY( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray_UINT16 &typedArray )
{
    NATIVE_PROFILE_CLR_CORE();
    return Interop_Marshal_NUMERIC_ARRAY<UINT16>( stackFrame, paramIndex, typedArray.m_pData, typedArray.m_ElemCount, sizeof( UINT16 ) );
}

HRESULT Interop_Marshal_UINT32_ARRAY( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray_UINT32 &typedArray )
{
    NATIVE_PROFILE_CLR_CORE();
    return Interop_Marshal_NUMERIC_ARRAY<UINT32>( stackFrame, paramIndex, typedArray.m_pData, typedArray.m_ElemCount, sizeof( UINT32 ) );
}

HRESULT Interop_Marshal_UINT64_ARRAY( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray_UINT64 &typedArray )
{
    NATIVE_PROFILE_CLR_CORE();
    return Interop_Marshal_NUMERIC_ARRAY<UINT64>( stackFrame, paramIndex, typedArray.m_pData, typedArray.m_ElemCount, sizeof( UINT64 ) );
}

HRESULT Interop_Marshal_CHAR_ARRAY( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray_CHAR &typedArray )
{
    NATIVE_PROFILE_CLR_CORE();
    return Interop_Marshal_NUMERIC_ARRAY<CHAR>( stackFrame, paramIndex, typedArray.m_pData, typedArray.m_ElemCount, sizeof( CHAR ) );
}

HRESULT Interop_Marshal_INT8_ARRAY( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray_INT8 &typedArray )
{
    NATIVE_PROFILE_CLR_CORE();
    return Interop_Marshal_NUMERIC_ARRAY<INT8>( stackFrame, paramIndex, typedArray.m_pData, typedArray.m_ElemCount, sizeof( INT8 ) );
}

HRESULT Interop_Marshal_INT16_ARRAY( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray_INT16 &typedArray )
{
    NATIVE_PROFILE_CLR_CORE();
   return Interop_Marshal_NUMERIC_ARRAY<INT16>( stackFrame, paramIndex, typedArray.m_pData, typedArray.m_ElemCount, sizeof( INT16 ) );
}

HRESULT Interop_Marshal_INT32_ARRAY( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray_INT32 &typedArray )
{
    NATIVE_PROFILE_CLR_CORE();
    return Interop_Marshal_NUMERIC_ARRAY<INT32>( stackFrame, paramIndex, typedArray.m_pData, typedArray.m_ElemCount, sizeof( INT32 ) );
}

HRESULT Interop_Marshal_INT64_ARRAY( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray_INT64 &typedArray )
{
    NATIVE_PROFILE_CLR_CORE();
    return Interop_Marshal_NUMERIC_ARRAY<INT64>( stackFrame, paramIndex, typedArray.m_pData, typedArray.m_ElemCount, sizeof( INT64 ) );
}

#if !defined(TINYCLR_EMULATED_FLOATINGPOINT)
HRESULT Interop_Marshal_float_ARRAY( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray_float &typedArray )
{
    NATIVE_PROFILE_CLR_CORE();
    return Interop_Marshal_NUMERIC_ARRAY<float>( stackFrame, paramIndex, typedArray.m_pData, typedArray.m_ElemCount, sizeof( float ) );
}

HRESULT Interop_Marshal_double_ARRAY( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray_double &typedArray )
{
    NATIVE_PROFILE_CLR_CORE();
    return Interop_Marshal_NUMERIC_ARRAY<double>( stackFrame, paramIndex, typedArray.m_pData, typedArray.m_ElemCount, sizeof( double ) );
}
#else

HRESULT Interop_Marshal_float_ARRAY( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray_float &typedArray )
{
    NATIVE_PROFILE_CLR_CORE();
    return Interop_Marshal_NUMERIC_ARRAY<INT32>( stackFrame, paramIndex, typedArray.m_pData, typedArray.m_ElemCount, sizeof( INT32 ) );
}

HRESULT Interop_Marshal_double_ARRAY( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray_double &typedArray )
{
    NATIVE_PROFILE_CLR_CORE();
    return Interop_Marshal_NUMERIC_ARRAY<INT64>( stackFrame, paramIndex, typedArray.m_pData, typedArray.m_ElemCount, sizeof( INT64 ) );
}

#endif


/**********************************************************************
**
** Functions: SetResult_*   
**
** Synopsis: This group of functions set result returned by native function to managed stack frame. 
**           It stores data from heap block pointed by pVoidHeapBlock back to the managed stack frame.
**
** Arguments: [stackFrame]           - Reference to the managed stack frame. 
**            [pVoidHeapBlock]       - Pointer to heap block that keeps updated basic type value
**            [paramIndex]           - Index of parameter passed from managed code. This parameter will be updated now.
**
** Returns:   S_OK on success or error from StoreToReference. Error return would cause exception thrown in managed code.
**********************************************************************/

void SetResult_bool( CLR_RT_StackFrame &stackFrame, bool value )
{
    NATIVE_PROFILE_CLR_CORE();
    stackFrame.SetResult_Boolean( value );
}

void SetResult_CHAR( CLR_RT_StackFrame &stackFrame, CHAR value )
{
    NATIVE_PROFILE_CLR_CORE();
    stackFrame.SetResult( value, DATATYPE_I1 ); 
}

void SetResult_INT8( CLR_RT_StackFrame &stackFrame, INT8 value )
{
    NATIVE_PROFILE_CLR_CORE();
    stackFrame.SetResult( value, DATATYPE_I1 ); 
}

void SetResult_INT16( CLR_RT_StackFrame &stackFrame, INT16 value )
{
    NATIVE_PROFILE_CLR_CORE();
    stackFrame.SetResult( value, DATATYPE_I2 );
}

void SetResult_INT32( CLR_RT_StackFrame &stackFrame, INT32 value )
{
    NATIVE_PROFILE_CLR_CORE();
    stackFrame.SetResult( value, DATATYPE_I4 );
}

void SetResult_INT64( CLR_RT_StackFrame &stackFrame, INT64 value )
{
    NATIVE_PROFILE_CLR_CORE();
    stackFrame.SetResult_I8( value );
}

void SetResult_UINT8( CLR_RT_StackFrame &stackFrame, UINT8 value )
{
    NATIVE_PROFILE_CLR_CORE();
    stackFrame.SetResult( value, DATATYPE_U1 );
}

void SetResult_UINT16( CLR_RT_StackFrame &stackFrame, UINT16 value )
{
    NATIVE_PROFILE_CLR_CORE();
    stackFrame.SetResult( value, DATATYPE_U2 );
}

void SetResult_UINT32( CLR_RT_StackFrame &stackFrame, UINT32 value )
{
    NATIVE_PROFILE_CLR_CORE();
    stackFrame.SetResult( value, DATATYPE_U4 );
}

void SetResult_UINT64( CLR_RT_StackFrame &stackFrame, UINT64 value )
{
    NATIVE_PROFILE_CLR_CORE();
    stackFrame.SetResult_U8( value );
}
#if !defined(TINYCLR_EMULATED_FLOATINGPOINT)
void SetResult_float( CLR_RT_StackFrame &stackFrame, float value )
{
    NATIVE_PROFILE_CLR_CORE();
    stackFrame.SetResult_R4( value );
}

void SetResult_double( CLR_RT_StackFrame &stackFrame, double value )
{
    NATIVE_PROFILE_CLR_CORE();
    stackFrame.SetResult_R8( value );
}
#else
void SetResult_float( CLR_RT_StackFrame &stackFrame, CLR_INT32 value )
{
    NATIVE_PROFILE_CLR_CORE();
    stackFrame.SetResult_R4( value );
}

void SetResult_double( CLR_RT_StackFrame &stackFrame, CLR_INT64& value )
{
    NATIVE_PROFILE_CLR_CORE();
    stackFrame.SetResult_R8( value );
}
#endif

void SetResult_LPCSTR( CLR_RT_StackFrame &stackFrame, LPCSTR lpszString )
{
    NATIVE_PROFILE_CLR_CORE();
    stackFrame.SetResult_String( lpszString );
}

/**********************************************************************
**
** Group of function that retrive reference to particular type from managed stack frame.  
** This function hides  CLR_RT_HeapBlock structure.
** 
** Functions: Interop_Marshal_*_ByRef   
**
** Synopsis: For paramenters passed by reference gets the 
**           reference to basic type parameter.
**           After the call the pParam points to variable passed by reference from managed code.
**
** Arguments: [stackFrame]           - Reference to the managed stack frame. 
**            [pHeapBlock]           - Pointer to heap block that keeps reference type extracted from managed stack frame
**            [paramIndex]           - Index of parameter passed from managed code.
**            [pParam]               - Pointer to variable passed by reference from managed code. Filled by function.
**
** Returns:   S_OK on success or error from LoadFromReference of CLR_E_WRONG_TYPE if type invalid. Error return would cause exception thrown in managed code.
**********************************************************************/

/**********************************************************************
** Macro: TINYCLR_INTEROP_CHECK_REF_TYPE 
**  This macro is specific for Interop_Marshal_*_ByRef functions. 
**  It checks that retrived type corresponds to the data type stored in heapblock 
**  in parameter paramIndex.
**  In release mode macro does nothing.
**********************************************************************/

#if defined(_DEBUG)

// In debug mode check reference type. 
#define TINYCLR_INTEROP_CHECK_REF_TYPE( ref_type ) \
if ( ((CLR_RT_HeapBlock *)pHeapBlock)->DataType() != ref_type ) \
{                                                                  \
    TINYCLR_SET_AND_LEAVE( CLR_E_WRONG_TYPE );                     \
}

#else // Not defined _DEBUG, means we are in release mode - do nothing.

#define TINYCLR_INTEROP_CHECK_REF_TYPE( ref_type )
 
#endif
                                                                      
//------------------ Unsigned Integral types ----------------------------------------------------------------------------
HRESULT Interop_Marshal_bool_ByRef( const CLR_RT_StackFrame &stackFrame, void *pHeapBlock, UINT32 paramIndex, UINT8 *&pParam )
{
    NATIVE_PROFILE_CLR_CORE();
    // Declare hRes ( #define TINYCLR_HEADER() HRESULT hr )
    TINYCLR_HEADER();
    
    // Loads heapblock data from heapblock in managed stack frame 
    TINYCLR_CHECK_HRESULT(((CLR_RT_HeapBlock *)pHeapBlock)->LoadFromReference( stackFrame.ArgN(paramIndex) )); 
    
    // Validates that data in heapblock correspond to requested parameter.
    TINYCLR_INTEROP_CHECK_REF_TYPE(DATATYPE_U1)
    
    // Now we have initialized pHeapBlock with reference paramenter. 
    // Need to cast the pointer because &s1 is "CLR_INT8 *", while we need "UINT8 *"
    pParam = &((CLR_RT_HeapBlock *)pHeapBlock)->NumericByRef().u1;
    
    // Return S_OK or error if there is "go to" from TINYCLR_CHECK_HRESULT or TINYCLR_SET_AND_LEAVE
    TINYCLR_NOCLEANUP();
}

HRESULT Interop_Marshal_UINT8_ByRef( const CLR_RT_StackFrame &stackFrame, void *pHeapBlock, UINT32 paramIndex, UINT8 *&pParam )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
    TINYCLR_CHECK_HRESULT(((CLR_RT_HeapBlock *)pHeapBlock)->LoadFromReference( stackFrame.ArgN(paramIndex) )); 
    TINYCLR_INTEROP_CHECK_REF_TYPE(DATATYPE_U1)
    pParam = &((CLR_RT_HeapBlock *)pHeapBlock)->NumericByRef().u1;
    TINYCLR_NOCLEANUP();
}

HRESULT Interop_Marshal_UINT16_ByRef( const CLR_RT_StackFrame &stackFrame, void *pHeapBlock, UINT32 paramIndex, UINT16 *&pParam )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
    TINYCLR_CHECK_HRESULT(((CLR_RT_HeapBlock *)pHeapBlock)->LoadFromReference( stackFrame.ArgN(paramIndex) )); 
    TINYCLR_INTEROP_CHECK_REF_TYPE(DATATYPE_U2)
    pParam = &((CLR_RT_HeapBlock *)pHeapBlock)->NumericByRef().u2;
    TINYCLR_NOCLEANUP();
}

HRESULT Interop_Marshal_UINT32_ByRef( const CLR_RT_StackFrame &stackFrame, void *pHeapBlock, UINT32 paramIndex, UINT32 *&pParam )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
    TINYCLR_CHECK_HRESULT(((CLR_RT_HeapBlock *)pHeapBlock)->LoadFromReference( stackFrame.ArgN(paramIndex) )); 
    TINYCLR_INTEROP_CHECK_REF_TYPE(DATATYPE_U4)
    pParam = &((CLR_RT_HeapBlock *)pHeapBlock)->NumericByRef().u4;
    TINYCLR_NOCLEANUP();
}

HRESULT Interop_Marshal_UINT64_ByRef( const CLR_RT_StackFrame &stackFrame, void *pHeapBlock, UINT32 paramIndex, UINT64 *&pParam )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
    TINYCLR_CHECK_HRESULT(((CLR_RT_HeapBlock *)pHeapBlock)->LoadFromReference( stackFrame.ArgN(paramIndex) )); 
    TINYCLR_INTEROP_CHECK_REF_TYPE(DATATYPE_U8)
    pParam = (UINT64 *)&((CLR_RT_HeapBlock *)pHeapBlock)->NumericByRef().u8;
    TINYCLR_NOCLEANUP();
}

//------------------ Signed Integral types ----------------------------------------------------------------------------
HRESULT Interop_Marshal_CHAR_ByRef( const CLR_RT_StackFrame &stackFrame, void *pHeapBlock, UINT32 paramIndex, CHAR *&pParam )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
    TINYCLR_CHECK_HRESULT(((CLR_RT_HeapBlock *)pHeapBlock)->LoadFromReference( stackFrame.ArgN(paramIndex) )); 
    TINYCLR_INTEROP_CHECK_REF_TYPE(DATATYPE_CHAR)
    pParam = (CHAR *)&((CLR_RT_HeapBlock *)pHeapBlock)->NumericByRef().s1;
    TINYCLR_NOCLEANUP();
}

HRESULT Interop_Marshal_INT8_ByRef( const CLR_RT_StackFrame &stackFrame, void *pHeapBlock, UINT32 paramIndex, INT8 *&pParam )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
    TINYCLR_CHECK_HRESULT(((CLR_RT_HeapBlock *)pHeapBlock)->LoadFromReference( stackFrame.ArgN(paramIndex) )); 
    // INT8 servers for both boolean and sbyte types. So we check for either of them.
#if defined(_DEBUG)
    if ( !( ((CLR_RT_HeapBlock *)pHeapBlock)->DataType() == DATATYPE_I1 ||
            ((CLR_RT_HeapBlock *)pHeapBlock)->DataType() == DATATYPE_BOOLEAN ) 
       )
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }
#endif 
    pParam = &((CLR_RT_HeapBlock *)pHeapBlock)->NumericByRef().s1;
    TINYCLR_NOCLEANUP();
}

HRESULT Interop_Marshal_INT16_ByRef( const CLR_RT_StackFrame &stackFrame, void *pHeapBlock, UINT32 paramIndex, INT16 *&pParam )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
    TINYCLR_CHECK_HRESULT(((CLR_RT_HeapBlock *)pHeapBlock)->LoadFromReference( stackFrame.ArgN(paramIndex) )); 
    TINYCLR_INTEROP_CHECK_REF_TYPE(DATATYPE_I2)
    pParam = &((CLR_RT_HeapBlock *)pHeapBlock)->NumericByRef().s2;
    TINYCLR_NOCLEANUP();
}

HRESULT Interop_Marshal_INT32_ByRef( const CLR_RT_StackFrame &stackFrame, void *pHeapBlock, UINT32 paramIndex, INT32 *&pParam )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
    TINYCLR_CHECK_HRESULT(((CLR_RT_HeapBlock *)pHeapBlock)->LoadFromReference( stackFrame.ArgN(paramIndex) )); 
    TINYCLR_INTEROP_CHECK_REF_TYPE(DATATYPE_I4)
    pParam = &((CLR_RT_HeapBlock *)pHeapBlock)->NumericByRef().s4;
    TINYCLR_NOCLEANUP();
}

HRESULT Interop_Marshal_INT64_ByRef( const CLR_RT_StackFrame &stackFrame, void *pHeapBlock, UINT32 paramIndex, INT64 *&pParam )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
    TINYCLR_CHECK_HRESULT(((CLR_RT_HeapBlock *)pHeapBlock)->LoadFromReference( stackFrame.ArgN(paramIndex) )); 
    TINYCLR_INTEROP_CHECK_REF_TYPE(DATATYPE_I8)
    pParam = (INT64 *)&((CLR_RT_HeapBlock *)pHeapBlock)->NumericByRef().s8;
    TINYCLR_NOCLEANUP();
}

//----------------- Float point types - float and double
#if !defined(TINYCLR_EMULATED_FLOATINGPOINT)

HRESULT Interop_Marshal_float_ByRef( const CLR_RT_StackFrame &stackFrame, void *pHeapBlock, UINT32 paramIndex, float *&pParam )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
    TINYCLR_CHECK_HRESULT(((CLR_RT_HeapBlock *)pHeapBlock)->LoadFromReference( stackFrame.ArgN(paramIndex) )); 
    TINYCLR_INTEROP_CHECK_REF_TYPE(DATATYPE_R4)
    pParam = &((CLR_RT_HeapBlock *)pHeapBlock)->NumericByRef().r4;
    TINYCLR_NOCLEANUP();
}

HRESULT Interop_Marshal_double_ByRef( const CLR_RT_StackFrame &stackFrame, void *pHeapBlock, UINT32 paramIndex, double *&pParam )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
    TINYCLR_CHECK_HRESULT(((CLR_RT_HeapBlock *)pHeapBlock)->LoadFromReference( stackFrame.ArgN(paramIndex) )); 
    TINYCLR_INTEROP_CHECK_REF_TYPE(DATATYPE_R8)
    pParam = (double *)&((CLR_RT_HeapBlock *)pHeapBlock)->NumericByRef().r8;
    TINYCLR_NOCLEANUP();
}


//----------------- Non Float point types - float and double
#else
HRESULT Interop_Marshal_float_ByRef( const CLR_RT_StackFrame &stackFrame, void *pHeapBlock, UINT32 paramIndex, INT32 *&pParam )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
    TINYCLR_CHECK_HRESULT(((CLR_RT_HeapBlock *)pHeapBlock)->LoadFromReference( stackFrame.ArgN(paramIndex) )); 
    TINYCLR_INTEROP_CHECK_REF_TYPE(DATATYPE_R4)
    pParam = (INT32 *)&((CLR_RT_HeapBlock *)pHeapBlock)->NumericByRef().r4;
    TINYCLR_NOCLEANUP();
}

HRESULT Interop_Marshal_double_ByRef( const CLR_RT_StackFrame &stackFrame, void *pHeapBlock, UINT32 paramIndex, INT64* &pParam )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
    TINYCLR_CHECK_HRESULT(((CLR_RT_HeapBlock *)pHeapBlock)->LoadFromReference( stackFrame.ArgN(paramIndex) )); 
    TINYCLR_INTEROP_CHECK_REF_TYPE(DATATYPE_R8)
    pParam = (INT64 *)&((CLR_RT_HeapBlock *)pHeapBlock)->NumericByRef().r8;
    TINYCLR_NOCLEANUP();
}

#endif
HRESULT Interop_Marshal_UNSUPPORTED_TYPE_ByRef( const CLR_RT_StackFrame &stackFrame, void *pHeapBlock, UINT32 paramIndex, UNSUPPORTED_TYPE *&pParam )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
    TINYCLR_CHECK_HRESULT(((CLR_RT_HeapBlock *)pHeapBlock)->LoadFromReference( stackFrame.ArgN(paramIndex) )); 
    pParam = NULL;
    TINYCLR_NOCLEANUP();
}


#undef TINYCLR_INTEROP_CHECK_REF_TYPE


/**********************************************************************
**
** Function: Interop_Marshal_StoreRef   
**
** Synopsis: Stores data from the heap block passed in pVoidHeapBlock back to managed stack frame.
**           Thus the managed stack frame and application may receive modified value of variable passed by reference..
**
** Arguments: [stackFrame]           - Reference to the managed stack frame. 
**            [pVoidHeapBlock]       - Pointer to heap block that keeps updated value of variable passed by reference.
**            [paramIndex]           - Index of parameter passed from managed code.
**
** Returns:   S_OK on success or error from StoreToReference. Error return would cause exception thrown in managed code.
**********************************************************************/

HRESULT Interop_Marshal_StoreRef( CLR_RT_StackFrame &stackFrame, void *pVoidHeapBlock, UINT32 paramIndex ) 
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
    CLR_RT_HeapBlock *pHeapBlock = (CLR_RT_HeapBlock *)pVoidHeapBlock;
    TINYCLR_CHECK_HRESULT(pHeapBlock->StoreToReference( stackFrame.ArgN(paramIndex), 0 ));
    TINYCLR_NOCLEANUP();
}

CLR_RT_HeapBlock* Interop_Marshal_RetrieveManagedObject( CLR_RT_StackFrame &stackFrame )

{
    return stackFrame.This();
}

/**********************************************************************
**
** Functions: Interop_Marshal_GetField_   
**
** Synopsis: Retrieves C++ reference ( pointer ) to the field in managed object.
**
** Arguments: [pThis]           - Pointer to the managed object retrieved by Interop_Marshal_RetrieveManagedObject. 
**            [fieldIndex]      - Field index.
**
** Returns:   Reference to the field.
**********************************************************************/
UINT8 &Interop_Marshal_GetField_bool(   CLR_RT_HeapBlock *pThis, UINT32 fieldIndex )
{
    return pThis[ fieldIndex ].NumericByRef().u1;
}

UINT8 &Interop_Marshal_GetField_UINT8(  CLR_RT_HeapBlock *pThis, UINT32 fieldIndex )         
{
    return pThis[ fieldIndex ].NumericByRef().u1;
}

UINT16 &Interop_Marshal_GetField_UINT16( CLR_RT_HeapBlock *pThis, UINT32 fieldIndex )
{
    return pThis[ fieldIndex ].NumericByRef().u2;
}

UINT32 &Interop_Marshal_GetField_UINT32( CLR_RT_HeapBlock *pThis, UINT32 fieldIndex )
{
    return pThis[ fieldIndex ].NumericByRef().u4;
}

UINT64 &Interop_Marshal_GetField_UINT64( CLR_RT_HeapBlock *pThis, UINT32 fieldIndex )
{
    return (UINT64 &)pThis[ fieldIndex ].NumericByRef().u8;
}

CHAR &Interop_Marshal_GetField_CHAR(   CLR_RT_HeapBlock *pThis, UINT32 fieldIndex )
{
    return (CHAR &)pThis[ fieldIndex ].NumericByRef().s1;
}

INT8 &Interop_Marshal_GetField_INT8(   CLR_RT_HeapBlock *pThis, UINT32 fieldIndex )
{
    return pThis[ fieldIndex ].NumericByRef().s1;
}

INT16 &Interop_Marshal_GetField_INT16(  CLR_RT_HeapBlock *pThis, UINT32 fieldIndex )
{
    return pThis[ fieldIndex ].NumericByRef().s2;
}

INT32 &Interop_Marshal_GetField_INT32(  CLR_RT_HeapBlock *pThis, UINT32 fieldIndex )
{
    return pThis[ fieldIndex ].NumericByRef().s4;
}

INT64 &Interop_Marshal_GetField_INT64(  CLR_RT_HeapBlock *pThis, UINT32 fieldIndex )
{
    return (INT64 &)pThis[ fieldIndex ].NumericByRef().s8;
}

//----------------- Float point types - float and double
#if !defined(TINYCLR_EMULATED_FLOATINGPOINT)

float &Interop_Marshal_GetField_float(  CLR_RT_HeapBlock *pThis, UINT32 fieldIndex )
{
    return pThis[ fieldIndex ].NumericByRef().r4;
}

double &Interop_Marshal_GetField_double( CLR_RT_HeapBlock *pThis, UINT32 fieldIndex )
{
    return (double &)pThis[ fieldIndex ].NumericByRef().r8;
}
#else

INT32 &Interop_Marshal_GetField_float(  CLR_RT_HeapBlock *pThis, UINT32 fieldIndex )
{
    return (INT32 &) pThis[ fieldIndex ].NumericByRef().r4;
}

INT64 &Interop_Marshal_GetField_double( CLR_RT_HeapBlock *pThis, UINT32 fieldIndex )
{
    return (INT64 &)pThis[ fieldIndex ].NumericByRef().r8;
}


#endif

#if defined(__arm)
#pragma push
#pragma diag_suppress 284
#endif
UNSUPPORTED_TYPE &Interop_Marshal_GetField_UNSUPPORTED_TYPE( CLR_RT_HeapBlock *pThis, UINT32 fieldIndex )
{
    return (UNSUPPORTED_TYPE &)(*((UNSUPPORTED_TYPE *)NULL));
}
#if defined(__arm)
#pragma pop
#endif

