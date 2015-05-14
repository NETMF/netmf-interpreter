////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _TINYCLR_INTEROP_H_
#define _TINYCLR_INTEROP_H_

#include <tinyhal.h>
#include <TinyClr_PlatformDef.h>

#include <TinyCLR_ErrorCodes.h>


///////////////////////////////////////////////////////////////////////////////////////////////////

extern HRESULT TINYCLR_DEBUG_PROCESS_EXCEPTION( HRESULT hr, LPCSTR szFunc, LPCSTR szFile, int line );

#if defined(PLATFORM_WINDOWS) || defined(PLATFORM_WINCE)

#define TINYCLR_LEAVE()  { if(FAILED(hr)) TINYCLR_DEBUG_PROCESS_EXCEPTION( hr, NULL, NULL, 0 ); goto TinyCLR_Cleanup; }
#define TINYCLR_RETURN() return hr

#else

#if defined(TINYCLR_TRACE_HRESULT)

#define TINYCLR_LEAVE()  { if(FAILED(hr)) TINYCLR_DEBUG_PROCESS_EXCEPTION( hr, __func__, __FILE__, __LINE__ ); goto TinyCLR_Cleanup; }
#define TINYCLR_RETURN() return hr

#else

#define TINYCLR_LEAVE()  goto TinyCLR_Cleanup
#define TINYCLR_RETURN() return hr

#endif

#endif


#define TINYCLR_HEADER()               HRESULT hr
#define TINYCLR_CHECK_HRESULT(expr)    { if(FAILED(hr = (expr))) TINYCLR_LEAVE(); }
#define TINYCLR_EXIT_ON_SUCCESS(expr)  { if(SUCCEEDED(hr = (expr))) TINYCLR_LEAVE(); }
#define TINYCLR_SET_AND_LEAVE(expr)    { hr = (expr); TINYCLR_LEAVE(); }
#define TINYCLR_CLEANUP()              hr = S_OK; TinyCLR_Cleanup:
#define TINYCLR_CLEANUP_END()          TINYCLR_RETURN()
#define TINYCLR_NOCLEANUP()            TINYCLR_CLEANUP(); TINYCLR_CLEANUP_END()
#define TINYCLR_NOCLEANUP_NOLABEL()    hr = S_OK; TINYCLR_RETURN()
#define FAULT_ON_NULL(ptr)             if(!(ptr)) TINYCLR_SET_AND_LEAVE(CLR_E_NULL_REFERENCE)
#define FAULT_ON_NULL_ARG(ptr)         if(!(ptr)) TINYCLR_SET_AND_LEAVE(CLR_E_ARGUMENT_NULL)


//    Correspondence between CLR C# and C++ native types:
//    CLR Type          C/C++ type	    C/C++ Ref Type

//    System.Byte       UINT8           UINT8&
//    System.UInt16     UINT16          UINT16&
//    System.UInt32     UINT32          UINT32&
//    System.UInt64     UINT64          UINT64&
//    System.Char       CHAR            CHAR &
//    System.SByte      INT8            INT8 &
//    System.Int16      INT16           INT16&
//    System.Int32      INT32           INT32&
//    System.Int64      INT64           INT64&
//    System.Single     float           float&
//    System.Double     double          double&
//    System.String     const CHAR *    CHAR *
//    System.Byte[]     UINT8 *         Same as C/C++ type

// Forward declaration for managed stack frame.
// This is internal CLR type, reference to managed stack frame is exposed to marshaling code.
struct CLR_RT_StackFrame;
/**********************************************************************
**
** Forward structure declarations .
** CLR_RT_HeapBlock                        - generic structure that keeps C# basic types
** CLR_RT_HeapBlock_NativeEventDispatcher  - Object that supports managed callback initiated by Interrupts
**********************************************************************/
struct CLR_RT_HeapBlock;
struct CLR_RT_HeapBlock_NativeEventDispatcher;

// Do not support marshaling of struct and class. For these types we create params of CLR_RT_HeapBlock* 
// and then set it to zero.
typedef CLR_RT_HeapBlock* UNSUPPORTED_TYPE;

#define CLR_RT_HEAP_BLOCK_SIZE 12

#define TINYCLR_NATIVE_DECLARE(mtd) static HRESULT mtd( CLR_RT_StackFrame& stack )

typedef HRESULT (*CLR_RT_MethodHandler)( CLR_RT_StackFrame& stack );

struct CLR_RT_NativeAssemblyData

{   // Assembly name or static name provided by driver developer to enable interrups.
    const char                 *m_szAssemblyName;

    // Check sum for the assembly.
    UINT32                      m_checkSum;

    // Pointer to array of functions that implement native methods.
    // It could be different type of the function depending if it is assembly Interop Method
    // or function enabling Interrupts by user driver.
    const void                 *m_pNativeMethods;
};

// Looks up in interop assemblies table for assembly with specified name.
const CLR_RT_NativeAssemblyData *GetAssemblyNativeData( const char *lpszAssemblyName );

// C++ by value parameters ( not reference types ).
/**********************************************************************
**
** Functions: Interop_Marshal_*
**
** Synopsis: This group of functions retrienves by value parameters ( not reference types )
**           Each function gets reference to managed stack frame in index of parameter to extract.
**           The C++ value is returned in "param".
**
** Arguments: [stackFrame]           - Reference to the managed stack frame.
**            [paramIndex]           - Index of parameter passed from managed code. This parameter will be updated now.
**            [param]                - Reference to native C++ variable. Filled by the function.
**                                     This variable is filled with value passed from managed code.
**
** Returns:   S_OK on success or error in case of paramter mismatch or invalid data passed from managed code.
**********************************************************************/

HRESULT Interop_Marshal_bool  ( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, bool   &param );
HRESULT Interop_Marshal_UINT8 ( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, UINT8  &param );
HRESULT Interop_Marshal_UINT16( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, UINT16 &param );
HRESULT Interop_Marshal_UINT32( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, UINT32 &param );
HRESULT Interop_Marshal_UINT64( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, UINT64 &param );
HRESULT Interop_Marshal_CHAR  ( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CHAR   &param );
HRESULT Interop_Marshal_INT8  ( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, INT8   &param );
HRESULT Interop_Marshal_INT16 ( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, INT16  &param );
HRESULT Interop_Marshal_INT32 ( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, INT32  &param );
HRESULT Interop_Marshal_INT64 ( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, INT64  &param );
#if !defined(TINYCLR_EMULATED_FLOATINGPOINT)
HRESULT Interop_Marshal_float ( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, float  &param );
HRESULT Interop_Marshal_double( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, double &param );

#else
HRESULT Interop_Marshal_float ( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, INT32  &param );
HRESULT Interop_Marshal_double( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, INT64  &param );
#endif

HRESULT Interop_Marshal_LPCSTR( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, LPCSTR &param );
// For unsupported types we set param to NULL. 
HRESULT Interop_Marshal_UNSUPPORTED_TYPE( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, UNSUPPORTED_TYPE &param );
/**********************************************************************
**
** Functions: Interop_Marshal_*_ARRAY
**
** Synopsis: This group of functions retrienves C++ by value parameters ( not reference types )
**           Each function gets reference to managed stack frame in index of parameter to extract.
**           The C++ value is returned in "param".
**
** Arguments: [stackFrame]    - Reference to the managed stack frame.
**            [paramIndex]    - Index of parameter passed from managed code. This parameter will be updated now.
**            [pByteParam]    - Reference to pointer filled by the function.
**                              On return pParam points to native C++ array with values passed from managed code.
**                              This variable is filled with value passed from managed code.
**            [arraySize]     - Count of elements in array. Filled by the function.
**
** Returns:   S_OK on success or error in case of paramter mismatch or invalid data passed from managed code.
**********************************************************************/


template <class T> class CLR_RT_TypedArray

{ 
    T      *m_pData;
    UINT32  m_ElemCount;

friend HRESULT Interop_Marshal_bool_ARRAY  ( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray<UINT8>  &typedArray );
friend HRESULT Interop_Marshal_UINT8_ARRAY ( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray<UINT8>  &typedArray );
friend HRESULT Interop_Marshal_UINT16_ARRAY( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray<UINT16> &typedArray );
friend HRESULT Interop_Marshal_UINT32_ARRAY( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray<UINT32> &typedArray );
friend HRESULT Interop_Marshal_UINT64_ARRAY( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray<UINT64> &typedArray );
friend HRESULT Interop_Marshal_CHAR_ARRAY  ( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray<CHAR>   &typedArray );
friend HRESULT Interop_Marshal_INT8_ARRAY  ( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray<INT8>   &typedArray );
friend HRESULT Interop_Marshal_INT16_ARRAY ( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray<INT16>  &typedArray );
friend HRESULT Interop_Marshal_INT32_ARRAY ( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray<INT32>  &typedArray );
friend HRESULT Interop_Marshal_INT64_ARRAY ( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray<INT64>  &typedArray );

#if !defined(TINYCLR_EMULATED_FLOATINGPOINT)
friend HRESULT Interop_Marshal_float_ARRAY ( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray<float>  &typedArray );
friend HRESULT Interop_Marshal_double_ARRAY( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray<double> &typedArray );
#else
friend HRESULT Interop_Marshal_float_ARRAY ( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray<INT32>  &typedArray );
friend HRESULT Interop_Marshal_double_ARRAY( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray<INT64>  &typedArray );

#endif


public :
    CLR_RT_TypedArray() { m_pData = NULL; m_ElemCount = 0; }

    T GetValue( UINT32 i )             { return m_pData[ i ];}
    void SetValue( UINT32 i, T &val )  { m_pData[ i ] = val; }
    T& operator[]( UINT32 i )          { return m_pData[ i ];}

    UINT32 GetSize() { return m_ElemCount; }

    // Direct access to buffer is provided for cases when other
    // native functions require access to buffer and cannot use CLR_RT_TypedArray object
    T *GetBuffer()   { return m_pData; }

 };

typedef CLR_RT_TypedArray<CHAR>   CLR_RT_TypedArray_CHAR;
typedef CLR_RT_TypedArray<UINT8>  CLR_RT_TypedArray_UINT8;
typedef CLR_RT_TypedArray<UINT16> CLR_RT_TypedArray_UINT16;
typedef CLR_RT_TypedArray<UINT32> CLR_RT_TypedArray_UINT32;
typedef CLR_RT_TypedArray<UINT64> CLR_RT_TypedArray_UINT64;
typedef CLR_RT_TypedArray<INT8>   CLR_RT_TypedArray_INT8;
typedef CLR_RT_TypedArray<INT16>  CLR_RT_TypedArray_INT16;
typedef CLR_RT_TypedArray<INT32>  CLR_RT_TypedArray_INT32;
typedef CLR_RT_TypedArray<INT64>  CLR_RT_TypedArray_INT64;
#if !defined(TINYCLR_EMULATED_FLOATINGPOINT)
typedef CLR_RT_TypedArray<float>  CLR_RT_TypedArray_float;
typedef CLR_RT_TypedArray<double> CLR_RT_TypedArray_double;
#else
typedef CLR_RT_TypedArray<INT32>  CLR_RT_TypedArray_float;
typedef CLR_RT_TypedArray<INT64>  CLR_RT_TypedArray_double;


#endif

HRESULT Interop_Marshal_bool_ARRAY  ( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray_UINT8  &typedArray );
HRESULT Interop_Marshal_UINT8_ARRAY ( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray_UINT8  &typedArray );
HRESULT Interop_Marshal_UINT16_ARRAY( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray_UINT16 &typedArray );
HRESULT Interop_Marshal_UINT32_ARRAY( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray_UINT32 &typedArray );
HRESULT Interop_Marshal_UINT64_ARRAY( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray_UINT64 &typedArray );
HRESULT Interop_Marshal_CHAR_ARRAY  ( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray_CHAR   &typedArray );
HRESULT Interop_Marshal_INT8_ARRAY  ( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray_INT8   &typedArray );
HRESULT Interop_Marshal_INT16_ARRAY ( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray_INT16  &typedArray );
HRESULT Interop_Marshal_INT32_ARRAY ( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray_INT32  &typedArray );
HRESULT Interop_Marshal_INT64_ARRAY ( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray_INT64  &typedArray );
HRESULT Interop_Marshal_float_ARRAY ( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray_float  &typedArray );
HRESULT Interop_Marshal_double_ARRAY( const CLR_RT_StackFrame &stackFrame, UINT32 paramIndex, CLR_RT_TypedArray_double &typedArray );

/**********************************************************************
**
** Functions: SetResult_*
**
** Synopsis: This group of functions set value returned by managed method call.
**           Value passed to this function will be the return value of the managed function.
**
** Arguments: [stackFrame]    - Reference to the managed stack frame.
**            [value]         - Return value. This value is stored on managed stack frame and passed to managed code.
**********************************************************************/

void SetResult_bool  ( CLR_RT_StackFrame &stackFrame, bool   value );
void SetResult_CHAR  ( CLR_RT_StackFrame &stackFrame, CHAR   value );
void SetResult_INT8  ( CLR_RT_StackFrame &stackFrame, INT8   value );
void SetResult_INT16 ( CLR_RT_StackFrame &stackFrame, INT16  value );
void SetResult_INT32 ( CLR_RT_StackFrame &stackFrame, INT32  value );
void SetResult_INT64 ( CLR_RT_StackFrame &stackFrame, INT64  value );
void SetResult_UINT8 ( CLR_RT_StackFrame &stackFrame, UINT8  value );
void SetResult_UINT16( CLR_RT_StackFrame &stackFrame, UINT16 value );
void SetResult_UINT32( CLR_RT_StackFrame &stackFrame, UINT32 value );
void SetResult_UINT64( CLR_RT_StackFrame &stackFrame, UINT64 value );
#if !defined(TINYCLR_EMULATED_FLOATINGPOINT)
void SetResult_float ( CLR_RT_StackFrame &stackFrame, float  value );
void SetResult_double( CLR_RT_StackFrame &stackFrame, double value );
#else
void SetResult_float ( CLR_RT_StackFrame &stackFrame, INT32 value );
void SetResult_double( CLR_RT_StackFrame &stackFrame, INT64 value );

#endif

void SetResult_LPCSTR( CLR_RT_StackFrame &stackFrame, LPCSTR value );

/**********************************************************************
**
** Functions: Interop_Marshal_LoadRef_*
**
** Synopsis: This group of functions pointer to reference parameter passed from managed code.
**           On return of this function pParam points to reference parameter of managed code.
**           Native code can retrieve and modidy the reference parameter using pParam
**
** Arguments: [stackFrame]    - Reference to the managed stack frame.
**            [paramIndex]    - Index of parameter passed from managed code. This parameter will be updated now.
**            [pByteParam]    - Reference to pointer filled by the function.
**                              On return pParam points to native C++ array with values passed from managed code.
**                              This variable is filled with value passed from managed code.
**            [pParam]        - Address of reference parameter passed from managed code. Filled by the function.
**
** Returns:   S_OK on success or error in case of paramter mismatch or invalid data passed from managed code.
**********************************************************************/
HRESULT Interop_Marshal_bool_ByRef  ( const CLR_RT_StackFrame &stackFrame, void *pHeapBlock, UINT32 paramIndex, UINT8  *&pParam );
HRESULT Interop_Marshal_UINT8_ByRef ( const CLR_RT_StackFrame &stackFrame, void *pHeapBlock, UINT32 paramIndex, UINT8  *&pParam );
HRESULT Interop_Marshal_UINT16_ByRef( const CLR_RT_StackFrame &stackFrame, void *pHeapBlock, UINT32 paramIndex, UINT16 *&pParam );
HRESULT Interop_Marshal_UINT32_ByRef( const CLR_RT_StackFrame &stackFrame, void *pHeapBlock, UINT32 paramIndex, UINT32 *&pParam );
HRESULT Interop_Marshal_UINT64_ByRef( const CLR_RT_StackFrame &stackFrame, void *pHeapBlock, UINT32 paramIndex, UINT64 *&pParam );
HRESULT Interop_Marshal_CHAR_ByRef  ( const CLR_RT_StackFrame &stackFrame, void *pHeapBlock, UINT32 paramIndex, CHAR   *&pParam );
HRESULT Interop_Marshal_INT8_ByRef  ( const CLR_RT_StackFrame &stackFrame, void *pHeapBlock, UINT32 paramIndex, INT8   *&pParam );
HRESULT Interop_Marshal_INT16_ByRef ( const CLR_RT_StackFrame &stackFrame, void *pHeapBlock, UINT32 paramIndex, INT16  *&pParam );
HRESULT Interop_Marshal_INT32_ByRef ( const CLR_RT_StackFrame &stackFrame, void *pHeapBlock, UINT32 paramIndex, INT32  *&pParam );
HRESULT Interop_Marshal_INT64_ByRef ( const CLR_RT_StackFrame &stackFrame, void *pHeapBlock, UINT32 paramIndex, INT64  *&pParam );
#if !defined(TINYCLR_EMULATED_FLOATINGPOINT)
HRESULT Interop_Marshal_float_ByRef ( const CLR_RT_StackFrame &stackFrame, void *pHeapBlock, UINT32 paramIndex, float  *&pParam );
HRESULT Interop_Marshal_double_ByRef( const CLR_RT_StackFrame &stackFrame, void *pHeapBlock, UINT32 paramIndex, double *&pParam );
#else
HRESULT Interop_Marshal_float_ByRef ( const CLR_RT_StackFrame &stackFrame, void *pHeapBlock, UINT32 paramIndex, INT32  *&pParam );
HRESULT Interop_Marshal_double_ByRef( const CLR_RT_StackFrame &stackFrame, void *pHeapBlock, UINT32 paramIndex, INT64  *&pParam );

#endif


// For unsuppoted types return NULL reference

HRESULT Interop_Marshal_UNSUPPORTED_TYPE_ByRef( const CLR_RT_StackFrame &stackFrame, void *pHeapBlock, UINT32 paramIndex, UNSUPPORTED_TYPE *&pParam );
/**********************************************************************
**
** Function: Interop_Marshal_StoreRef
**
** Synopsis: This function is used to store updated parameter or basic type passed by reference from managed code.
**           It stores data from heap block pointed by pVoidHeapBlock back to the managed stack frame.
**
** Arguments: [stackFrame]           - Reference to the managed stack frame.
**            [pVoidHeapBlock]       - Pointer to heap block that keeps updated basic type value
**            [paramIndex]           - Index of parameter passed from managed code. This parameter will be updated now.
**
** Returns:   S_OK on success or error from StoreToReference. Error return would cause exception thrown in managed code.
**********************************************************************/
HRESULT Interop_Marshal_StoreRef( CLR_RT_StackFrame &stackFrame, void *pVoidHeapBlock, UINT32 paramIndex );

/**********************************************************************
**
** Function: Interop_Marshal_RetrieveManagedObject
**
** Synopsis: Retrieves pointer to managed object from managed stack frame.
**
** Arguments: [stackFrame]           - Reference to the managed stack frame.
**
** Returns:   Pointer to managed object or NULL in case of error.
**********************************************************************/
CLR_RT_HeapBlock* Interop_Marshal_RetrieveManagedObject( CLR_RT_StackFrame &stackFrame );

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
UINT8  &Interop_Marshal_GetField_bool  ( CLR_RT_HeapBlock *pThis, UINT32 fieldIndex );
UINT8  &Interop_Marshal_GetField_UINT8 ( CLR_RT_HeapBlock *pThis, UINT32 fieldIndex );
UINT16 &Interop_Marshal_GetField_UINT16( CLR_RT_HeapBlock *pThis, UINT32 fieldIndex );
UINT32 &Interop_Marshal_GetField_UINT32( CLR_RT_HeapBlock *pThis, UINT32 fieldIndex );
UINT64 &Interop_Marshal_GetField_UINT64( CLR_RT_HeapBlock *pThis, UINT32 fieldIndex );
CHAR   &Interop_Marshal_GetField_CHAR  ( CLR_RT_HeapBlock *pThis, UINT32 fieldIndex );
INT8   &Interop_Marshal_GetField_INT8  ( CLR_RT_HeapBlock *pThis, UINT32 fieldIndex );
INT16  &Interop_Marshal_GetField_INT16 ( CLR_RT_HeapBlock *pThis, UINT32 fieldIndex );
INT32  &Interop_Marshal_GetField_INT32 ( CLR_RT_HeapBlock *pThis, UINT32 fieldIndex );
INT64  &Interop_Marshal_GetField_INT64 ( CLR_RT_HeapBlock *pThis, UINT32 fieldIndex );
//----------------- Float point types - float and double
#if !defined(TINYCLR_EMULATED_FLOATINGPOINT)
float  &Interop_Marshal_GetField_float ( CLR_RT_HeapBlock *pThis, UINT32 fieldIndex );
double &Interop_Marshal_GetField_double( CLR_RT_HeapBlock *pThis, UINT32 fieldIndex );
#else
INT32 &Interop_Marshal_GetField_float ( CLR_RT_HeapBlock *pThis, UINT32 fieldIndex );
INT64 &Interop_Marshal_GetField_double( CLR_RT_HeapBlock *pThis, UINT32 fieldIndex );
#endif

// For all other unsupported types we always return NULL reference.
UNSUPPORTED_TYPE &Interop_Marshal_GetField_UNSUPPORTED_TYPE( CLR_RT_HeapBlock *pThis, UINT32 fieldIndex );

/**********************************************************************
**
** Function Signatures that should be supported by drivers to enable managed callbacks.
**
** InitializeInterruptsProc
** This driver function is called in native code by constructor of managed NativeEventDispatcher object.
** Parameters:
**              [pContext] - Pointer to the context that should be passed to the ISR ( INterrupt Service Routine ).
**              [userData] - User data passed to constructor of NativeEventDispatcher.
**********************************************************************/
typedef HRESULT (*InitializeInterruptsProc     )( CLR_RT_HeapBlock_NativeEventDispatcher *pContext, UINT64 userData );

/**********************************************************************
** EnableorDisableInterruptsProc
**  This function is called by EnableInterrupt/DisableInterrupt functions of managed NativeEventDispatcher object.
**  It should enable or disable HW interrupts.
**              [pContext] - Pointer to the context, same as passed to InitializeInterruptsProc
**              [fEnable]  - If "true" - enable interrupts
**                           If "false" - disable interrupts
**********************************************************************/
typedef HRESULT (*EnableorDisableInterruptsProc)( CLR_RT_HeapBlock_NativeEventDispatcher *pContext, bool fEnable );

/**********************************************************************
**
** CleanupInterruptsProc
**    This function is called to release hardware that generates interrups and release associated resources
**              [pContext] - Pointer to the context, same as passed to InitializeInterruptsProc
**
**********************************************************************/
typedef HRESULT (*CleanupInterruptsProc)( CLR_RT_HeapBlock_NativeEventDispatcher *pContext );

/**********************************************************************
**
** InterruptServiceProc
**    This is the prototype for the interrupt service routie.
**              [pContext] - Pointer to the context, same as passed to InitializeInterruptsProc
**
**    This function should call SaveNativeEventToHALQueue in order to create managed callback.
**    The minimal implementation for this function would be:
**  static void ISR_TestProc( CLR_RT_HeapBlock_NativeEventDispatcher *pContext )
**
**  {
**      GLOBAL_LOCK(irq);
**      // To do - Initialize userData1 and userData2 to userData.
**      UINT32 userData1;
**      UINT32 userData2;
**      // Saves data to queue. Each call to SaveNativeEventToHALQueue initiates managed callback.
**      SaveNativeEventToHALQueue( pContext, userData1, userData2 );
**  }
**********************************************************************/
typedef HRESULT (*InterruptServiceProc)( CLR_RT_HeapBlock_NativeEventDispatcher *pContext );



/**********************************************************************
**
** Structure that keeps pointers to driver functions.
**    The driver functions are accessed by CLR through global instance of this structure.
**    See Spot_InteropSample_Native_Microsoft_SPOT_Interop_TestCallback.cpp file for use of this structure.
**
**********************************************************************/
struct CLR_RT_DriverInterruptMethods

{
    InitializeInterruptsProc      m_InitProc;
    EnableorDisableInterruptsProc m_EnableProc;
    CleanupInterruptsProc         m_CleanupProc;
};

// Randomly generated 32 bit number.
// This check sum validates that m_pNativeMethods points to CLR_RT_DriverInterruptMethods structure
#define DRIVER_INTERRUPT_METHODS_CHECKSUM   0xf32f4c3e

// Creates instance of CLR_RT_HeapBlock_NativeEventDispatcher.
CLR_RT_HeapBlock_NativeEventDispatcher *CreateNativeEventInstance( CLR_RT_StackFrame& stack );

// Saves data from ISR. The data from this queue is used to call managed callbacks.
// Should be called from ISR.
void SaveNativeEventToHALQueue( CLR_RT_HeapBlock_NativeEventDispatcher *pContext, UINT32 data1, UINT32 data2 );

// Cleans up the data in the queue after interrupts were closed and no managed callbacks are expected.
void CleanupNativeEventsFromHALQueue( CLR_RT_HeapBlock_NativeEventDispatcher *pContext );

void CLR_RetrieveCurrentMethod( UINT32& assmIdx, UINT32& methodIdx );

void CLR_SoftReboot();

void CLR_DebuggerBreak();

#endif // _TINYCLR_INTEROP_H_


