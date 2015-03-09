////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "CorLib.h"

struct ExceptionLookup
{
    HRESULT hr;
    CLR_RT_TypeDef_Index* ptr;
};

static const ExceptionLookup c_ExceptionLookup[] =
{
#define EL(hr,fld) { hr, &g_CLR_RT_WellKnownTypes.fld }
    EL(CLR_E_APPDOMAIN_EXITED         , m_AppDomainUnloadedException ),
    EL(CLR_E_INVALID_PARAMETER        , m_ArgumentException          ),
    EL(CLR_E_ARGUMENT_NULL            , m_ArgumentNullException      ),
    EL(CLR_E_OUT_OF_RANGE             , m_ArgumentOutOfRangeException),
    EL(CLR_E_INDEX_OUT_OF_RANGE       , m_IndexOutOfRangeException   ),
    EL(CLR_E_INVALID_CAST             , m_InvalidCastException       ),
    EL(CLR_E_INVALID_OPERATION        , m_InvalidOperationException  ),        
    EL(CLR_E_NOT_SUPPORTED            , m_NotSupportedException      ),
    EL(CLR_E_NOTIMPL                  , m_NotImplementedException    ),    
    EL(CLR_E_NULL_REFERENCE           , m_NullReferenceException     ),
    EL(CLR_E_OUT_OF_MEMORY            , m_OutOfMemoryException       ),
    EL(CLR_E_OBJECT_DISPOSED          , m_ObjectDisposedException    ),
    EL(CLR_E_FILE_IO                  , m_IOException                ),
    EL(CLR_E_INVALID_DRIVER           , m_IOException                ),
    EL(CLR_E_FILE_NOT_FOUND           , m_IOException                ),
    EL(CLR_E_DIRECTORY_NOT_FOUND      , m_IOException                ),
    EL(CLR_E_VOLUME_NOT_FOUND         , m_IOException                ),
    EL(CLR_E_PATH_TOO_LONG            , m_IOException                ),
    EL(CLR_E_DIRECTORY_NOT_EMPTY      , m_IOException                ),
    EL(CLR_E_UNAUTHORIZED_ACCESS      , m_IOException                ),
    EL(CLR_E_PATH_ALREADY_EXISTS      , m_IOException                ),
    EL(CLR_E_TOO_MANY_OPEN_HANDLES    , m_IOException                ),
    EL(CLR_E_WATCHDOG_TIMEOUT         , m_WatchdogException          ),

#undef EL
};

//--//

HRESULT Library_corlib_native_System_Exception::get_StackTrace___STRING( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Array*   pArray;
    StackTrace*               pStackTrace;
    CLR_RT_HeapBlock*         pBlkString;
    char                      buf[ 512 ];
    LPSTR                     strName;
    size_t                    iName;
    CLR_RT_HeapBlock          tmpArray; tmpArray.SetObjectReference( NULL );
    CLR_RT_ProtectFromGC      gc(tmpArray);
    int                       depth        = 0;
    CLR_RT_HeapBlock*         pThis        = stack.This();         FAULT_ON_NULL(pThis);

    pArray = pThis[ FIELD__m_stackTrace ].DereferenceArray();

    if(pArray)
    {
        _ASSERTE(pArray->m_typeOfElement == DATATYPE_U1);

        depth = pArray->m_numOfElements / sizeof(StackTrace);        
    }

    if(depth == 0) TINYCLR_SET_AND_LEAVE(stack.SetResult_String(NULL));
            
    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance( tmpArray, depth, g_CLR_RT_WellKnownTypes.m_String ));

    pStackTrace = (StackTrace*)pArray->GetFirstElement();
    pBlkString  = (CLR_RT_HeapBlock*)tmpArray.DereferenceArray()->GetFirstElement();

    for(int i = 0; i < depth; i++)
    {
        strName = &buf[ 0 ];
        iName   = MAXSTRLEN(buf) - 2 ;

        TINYCLR_CHECK_HRESULT(g_CLR_RT_TypeSystem.BuildMethodName( pStackTrace->m_md, strName, iName ));
        
        memcpy(strName, "\r\n\0", 3);

        TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_String::CreateInstance( *pBlkString, buf ));

        pStackTrace++;
        pBlkString++;
    }            

    TINYCLR_SET_AND_LEAVE(Library_corlib_native_System_String::Concat( stack, (CLR_RT_HeapBlock*)tmpArray.DereferenceArray()->GetFirstElement(), depth ));

    TINYCLR_NOCLEANUP();
}

//--//

/*
    This function will always succeed.
    Return values 
        - S_OK.  
        - S_FALSE.  ref points to the pre-allocated OutOfMemory exception
*/
HRESULT Library_corlib_native_System_Exception::CreateInstance( CLR_RT_HeapBlock& ref, const CLR_RT_TypeDef_Index& cls, HRESULT hrIn, CLR_RT_StackFrame* stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

#if defined(TINYCLR_APPDOMAINS)
    CLR_RT_AppDomain* appDomainSav = g_CLR_RT_ExecutionEngine.SetCurrentAppDomain( stack->m_appDomain );
#endif
    
    CLR_RT_HeapBlock* obj;

    _ASSERTE(CLR_RT_ExecutionEngine::IsInstanceOf( cls, g_CLR_RT_WellKnownTypes.m_Exception ));

    if(FAILED(hr = g_CLR_RT_ExecutionEngine.NewObjectFromIndex( ref, cls )))
    {
#if defined(TINYCLR_APPDOMAINS)
        ref.SetObjectReference( g_CLR_RT_ExecutionEngine.GetCurrentAppDomain()->m_outOfMemoryException );
#else
        ref.SetObjectReference( g_CLR_RT_ExecutionEngine.m_outOfMemoryException );
#endif

        hrIn         = CLR_E_OUT_OF_MEMORY;
        hr           = S_FALSE;        
    }

    obj = ref.Dereference();

    obj[ FIELD__m_HResult ].SetInteger( (CLR_UINT32)hrIn );

    if(hr == S_OK)
    {
        (void)SetStackTrace( ref, stack );
    }
        
#if defined(TINYCLR_APPDOMAINS)
    (void)g_CLR_RT_ExecutionEngine.SetCurrentAppDomain( appDomainSav );
#endif

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_Exception::CreateInstance( CLR_RT_HeapBlock& ref, HRESULT hrIn, CLR_RT_StackFrame* stack )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_TypeDef_Index* cls = &g_CLR_RT_WellKnownTypes.m_Exception;

    _ASSERTE(FAILED(hrIn));

    for(int i = 0; i < ARRAYSIZE(c_ExceptionLookup); i++)
    {
        if(c_ExceptionLookup[ i ].hr == hrIn)
        {
            cls = c_ExceptionLookup[ i ].ptr;
            break;
        }
    }
    
    return CreateInstance( ref, *cls, hrIn, stack );
}

HRESULT Library_corlib_native_System_Exception::SetStackTrace( CLR_RT_HeapBlock& ref, CLR_RT_StackFrame* stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    if(stack)
    {
        CLR_RT_HeapBlock*       obj;
        CLR_RT_HeapBlock_Array* array;
        StackTrace*             dst;
        CLR_UINT32              depth;

        if(CLR_RT_ExecutionEngine::IsInstanceOf( ref, g_CLR_RT_WellKnownTypes.m_Exception ) == false) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);

        //--//

        obj   = ref.Dereference();
        depth = 0;

        TINYCLR_FOREACH_NODE_BACKWARD__DIRECT(CLR_RT_StackFrame,stackSub,stack)
        {
            depth++;
        }
        TINYCLR_FOREACH_NODE_BACKWARD_END();

        //--//

        TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance( obj[ FIELD__m_stackTrace ], depth * sizeof(StackTrace), g_CLR_RT_WellKnownTypes.m_UInt8 ));

        //--//

        array = obj[ FIELD__m_stackTrace ].DereferenceArray();
        dst   = (StackTrace*)array->GetFirstElement();

        TINYCLR_FOREACH_NODE_BACKWARD__DIRECT(CLR_RT_StackFrame,stackSub,stack)
        {
            dst->m_md =              stackSub->m_call;
            dst->m_IP = (CLR_UINT32)(stackSub->m_IP - stackSub->m_IPstart);

            dst++;
        }
        TINYCLR_FOREACH_NODE_BACKWARD_END();

#if !defined(BUILD_RTM)
        //shutting down the EE happens by Thread->Abort.  These exceptions are by design, and 
        //don't need to be logged, or written to the console....
        if(!g_CLR_RT_ExecutionEngine.m_fShuttingDown)    
#endif
        {
            CLR_RT_DUMP::EXCEPTION( *stack, ref );
        }
    }

    TINYCLR_NOCLEANUP();
}

CLR_RT_HeapBlock* Library_corlib_native_System_Exception::GetTarget( CLR_RT_HeapBlock& ref )
{
    NATIVE_PROFILE_CLR_CORE();
    return CLR_RT_ExecutionEngine::IsInstanceOf( ref, g_CLR_RT_WellKnownTypes.m_Exception ) ? ref.Dereference() : NULL;
}

Library_corlib_native_System_Exception::StackTrace* Library_corlib_native_System_Exception::GetStackTrace( CLR_RT_HeapBlock* obj, CLR_UINT32& depth )
{
    NATIVE_PROFILE_CLR_CORE();
    if(obj)
    {
        CLR_RT_HeapBlock_Array* array = obj[ FIELD__m_stackTrace ].DereferenceArray();

        if(array)
        {
            depth = array->m_numOfElements / sizeof(StackTrace);

            return (StackTrace*)array->GetFirstElement();
        }
    }

    depth = 0;

    return NULL;
}
