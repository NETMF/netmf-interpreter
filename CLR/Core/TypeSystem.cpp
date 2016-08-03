////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Core.h"

#include "corhdr_private.h"
#include <TinyCLR_Types.h> 
#include <TinyCLR_Endian.h>


////////////////////////////////////////////////////////////////////////////////////////////////////

#define ITERATE_THROUGH_RECORDS(assm,i,tblName,tblNameUC) \
    const CLR_RECORD_##tblNameUC*      src = (const CLR_RECORD_##tblNameUC*)assm->GetTable( TBL_##tblName );\
    CLR_RT_##tblName##_CrossReference* dst = assm->m_pCrossReference_##tblName;\
    for(i=0; i<assm->m_pTablesSize[TBL_##tblName]; i++, src++, dst++)

////////////////////////////////////////////////////////////////////////////////////////////////////

#if defined(_WIN32)
#define TINYCLR_TRACE_DEFAULT(win,arm) (win)
#else
#define TINYCLR_TRACE_DEFAULT(win,arm) (arm)
#endif

#if defined(TINYCLR_TRACE_ERRORS)
int s_CLR_RT_fTrace_Errors                     = TINYCLR_TRACE_DEFAULT(c_CLR_RT_Trace_Info,c_CLR_RT_Trace_Info);
#endif

#if defined(TINYCLR_TRACE_EXCEPTIONS)
int s_CLR_RT_fTrace_Exceptions                 = TINYCLR_TRACE_DEFAULT(c_CLR_RT_Trace_Info,c_CLR_RT_Trace_Info);
#endif

#if defined(TINYCLR_TRACE_INSTRUCTIONS)
int s_CLR_RT_fTrace_Instructions               = TINYCLR_TRACE_DEFAULT(c_CLR_RT_Trace_None,c_CLR_RT_Trace_None);
#endif

#if defined(TINYCLR_GC_VERBOSE)
int s_CLR_RT_fTrace_Memory                     = TINYCLR_TRACE_DEFAULT(c_CLR_RT_Trace_None,c_CLR_RT_Trace_None);
#endif

#if defined(TINYCLR_TRACE_MEMORY_STATS)
int s_CLR_RT_fTrace_MemoryStats                = TINYCLR_TRACE_DEFAULT(c_CLR_RT_Trace_Info,c_CLR_RT_Trace_Info);
#endif

#if defined(TINYCLR_GC_VERBOSE)
int s_CLR_RT_fTrace_GC                         = TINYCLR_TRACE_DEFAULT(c_CLR_RT_Trace_None,c_CLR_RT_Trace_None);
#endif

#if defined(WIN32)
int s_CLR_RT_fTrace_SimulateSpeed              = TINYCLR_TRACE_DEFAULT(c_CLR_RT_Trace_Info,c_CLR_RT_Trace_None);
#endif

#if !defined(BUILD_RTM)
int s_CLR_RT_fTrace_AssemblyOverhead           = TINYCLR_TRACE_DEFAULT(c_CLR_RT_Trace_Info,c_CLR_RT_Trace_None);
#endif

#if defined(TINYCLR_JITTER)
bool s_CLR_RT_fJitter_Enabled                  = TINYCLR_TRACE_DEFAULT(false,true);
int  s_CLR_RT_fJitter_Trace_Statistics         = TINYCLR_TRACE_DEFAULT(c_CLR_RT_Trace_None,c_CLR_RT_Trace_None);
int  s_CLR_RT_fJitter_Trace_Compile            = TINYCLR_TRACE_DEFAULT(c_CLR_RT_Trace_None,c_CLR_RT_Trace_None);
int  s_CLR_RT_fJitter_Trace_Invoke             = TINYCLR_TRACE_DEFAULT(c_CLR_RT_Trace_None,c_CLR_RT_Trace_None);
int  s_CLR_RT_fJitter_Trace_Execution          = TINYCLR_TRACE_DEFAULT(c_CLR_RT_Trace_None,c_CLR_RT_Trace_None);
#endif

#if defined(WIN32)
int s_CLR_RT_fTrace_StopOnFAILED               = TINYCLR_TRACE_DEFAULT(c_CLR_RT_Trace_None,c_CLR_RT_Trace_None);
#endif

#if defined(WIN32)
int s_CLR_RT_fTrace_ARM_Execution              = 0;

int          s_CLR_RT_fTrace_RedirectLinesPerFile;
std::wstring s_CLR_RT_fTrace_RedirectOutput;
std::wstring s_CLR_RT_fTrace_RedirectCallChain;

std::wstring s_CLR_RT_fTrace_HeapDump_FilePrefix;
bool         s_CLR_RT_fTrace_HeapDump_IncludeCreators  = false;

bool         s_CLR_RT_fTimeWarp = false;
#endif

////////////////////////////////////////////////////////////////////////////////////////////////////

void CLR_RT_ReflectionDef_Index::Clear()
{
    NATIVE_PROFILE_CLR_CORE();
    m_kind       = REFLECTION_INVALID;
    m_levels     = 0;
    m_data.m_raw = 0;
}

CLR_UINT32 CLR_RT_ReflectionDef_Index::GetTypeHash() const
{
    NATIVE_PROFILE_CLR_CORE();
    switch(m_kind)
    {
    case REFLECTION_TYPE:
        {
            CLR_RT_TypeDef_Instance inst;

            if(m_levels != 0) return 0;

            if(!inst.InitializeFromIndex( m_data.m_type )) return 0;

            return inst.CrossReference().m_hash;
        }

    case REFLECTION_TYPE_DELAYED:
        return m_data.m_raw;
    }

    return 0;
}

void CLR_RT_ReflectionDef_Index::InitializeFromHash( CLR_UINT32 hash )
{
    NATIVE_PROFILE_CLR_CORE();
    m_kind       = REFLECTION_TYPE_DELAYED;
    m_levels     = 0;
    m_data.m_raw = hash;
}

CLR_UINT64 CLR_RT_ReflectionDef_Index::GetRawData() const
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_UINT64 data;
    _ASSERTE(sizeof(data) == sizeof(*this));

    memcpy( &data, this, sizeof(data) );

    return data;
}

void CLR_RT_ReflectionDef_Index::SetRawData( CLR_UINT64 data )
{
    NATIVE_PROFILE_CLR_CORE();
    _ASSERTE(sizeof(data) == sizeof(*this));

    memcpy( this, &data, sizeof(data) );
}

bool CLR_RT_ReflectionDef_Index::Convert( CLR_RT_HeapBlock& ref, CLR_RT_Assembly_Instance& inst )
{
    NATIVE_PROFILE_CLR_CORE();
    if(ref.DataType() == DATATYPE_REFLECTION)
    {
        return inst.InitializeFromIndex( ref.ReflectionDataConst().m_data.m_assm );
    }

    return false;
}

bool CLR_RT_ReflectionDef_Index::Convert( CLR_RT_HeapBlock& ref, CLR_RT_TypeDef_Instance& inst, CLR_UINT32* levels )
{
    NATIVE_PROFILE_CLR_CORE();
    if(ref.DataType() == DATATYPE_REFLECTION)
    {
        return inst.InitializeFromReflection( ref.ReflectionDataConst(), levels );
    }

    return false;
}

bool CLR_RT_ReflectionDef_Index::Convert( CLR_RT_HeapBlock& ref, CLR_RT_MethodDef_Instance& inst )
{
    NATIVE_PROFILE_CLR_CORE();
    if(ref.DataType() == DATATYPE_REFLECTION)
    {
        switch(ref.ReflectionData().m_kind)
        {
        case REFLECTION_CONSTRUCTOR:
        case REFLECTION_METHOD:
            return inst.InitializeFromIndex( ref.ReflectionDataConst().m_data.m_method );
        }
    }

    return false;
}

bool CLR_RT_ReflectionDef_Index::Convert( CLR_RT_HeapBlock& ref, CLR_RT_FieldDef_Instance& inst )
{
    NATIVE_PROFILE_CLR_CORE();
    if(ref.DataType() == DATATYPE_REFLECTION && ref.ReflectionData().m_kind == REFLECTION_FIELD)
    {
        return inst.InitializeFromIndex( ref.ReflectionDataConst().m_data.m_field );
    }

    return false;
}

bool CLR_RT_ReflectionDef_Index::Convert( CLR_RT_HeapBlock& ref, CLR_UINT32& hash )
{
    NATIVE_PROFILE_CLR_CORE();
    if(ref.DataType() != DATATYPE_REFLECTION) return false;

    hash = ref.ReflectionData().GetTypeHash();

    return hash != 0;
}

////////////////////////////////////////////////////////////////////////////////////////////////////

void CLR_RT_SignatureParser::Initialize_TypeSpec( CLR_RT_Assembly* assm, const CLR_RECORD_TYPESPEC* ts )
{
    NATIVE_PROFILE_CLR_CORE();
    Initialize_TypeSpec( assm, assm->GetSignature( ts->sig ) );
}

void CLR_RT_SignatureParser::Initialize_TypeSpec( CLR_RT_Assembly* assm, CLR_PMETADATA ts )
{
    NATIVE_PROFILE_CLR_CORE();
    m_assm  = assm;
    m_sig   = ts;

    m_type  = CLR_RT_SignatureParser::c_TypeSpec;
    m_flags = 0;
    m_count = 1;
}

//--//

void CLR_RT_SignatureParser::Initialize_Interfaces( CLR_RT_Assembly* assm, const CLR_RECORD_TYPEDEF* td )
{
    NATIVE_PROFILE_CLR_CORE();
    if(td->interfaces != CLR_EmptyIndex)
    {
        CLR_PMETADATA sig = assm->GetSignature( td->interfaces );

        m_count = (*sig++);
        m_sig   = sig;
    }
    else
    {
        m_count = 0;
        m_sig   = NULL;
    }

    m_type  = CLR_RT_SignatureParser::c_Interfaces;
    m_flags = 0;

    m_assm  = assm;
}

//--//

void CLR_RT_SignatureParser::Initialize_FieldDef( CLR_RT_Assembly* assm, const CLR_RECORD_FIELDDEF* fd )
{
    NATIVE_PROFILE_CLR_CORE();
    Initialize_FieldDef( assm, assm->GetSignature( fd->sig ) );
}

void CLR_RT_SignatureParser::Initialize_FieldDef( CLR_RT_Assembly* assm, CLR_PMETADATA fd )
{
    NATIVE_PROFILE_CLR_CORE();
    m_type  = CLR_RT_SignatureParser::c_Field;
    m_flags = (*fd++);
    m_count = 1;

    m_assm  = assm;
    m_sig   = fd;
}

//--//

void CLR_RT_SignatureParser::Initialize_MethodSignature( CLR_RT_Assembly* assm, const CLR_RECORD_METHODDEF* md )
{
    NATIVE_PROFILE_CLR_CORE();
    Initialize_MethodSignature( assm, assm->GetSignature( md->sig ) );
}

void CLR_RT_SignatureParser::Initialize_MethodSignature( CLR_RT_Assembly* assm, CLR_PMETADATA md )
{
    NATIVE_PROFILE_CLR_CORE();
    m_type  = CLR_RT_SignatureParser::c_Method;
    m_flags = (*md++);
    m_count = (*md++) + 1;

    m_assm  = assm;
    m_sig   = md;
}

//--//

void CLR_RT_SignatureParser::Initialize_MethodLocals( CLR_RT_Assembly* assm, const CLR_RECORD_METHODDEF* md )
{
    NATIVE_PROFILE_CLR_CORE();
    //
    // WARNING!!!
    //
    // If you change this method, change "CLR_RT_ExecutionEngine::InitializeLocals" too.
    //

    m_assm  = assm;
    m_sig   = assm->GetSignature( md->locals );

    m_type  = CLR_RT_SignatureParser::c_Locals;
    m_flags = 0;
    m_count = md->numLocals;
}

//--//

void CLR_RT_SignatureParser::Initialize_Objects( CLR_RT_HeapBlock* lst, int count, bool fTypes )
{
    NATIVE_PROFILE_CLR_CORE();
    m_lst = lst;

    m_type  = CLR_RT_SignatureParser::c_Object;
    m_flags = fTypes ? 1 : 0;
    m_count = count;
}

//--//

HRESULT CLR_RT_SignatureParser::Advance( Element& res )
{
    NATIVE_PROFILE_CLR_CORE();
    //
    // WARNING!!!
    //
    // If you change this method, change "CLR_RT_ExecutionEngine::InitializeLocals" too.
    //

    TINYCLR_HEADER();

    _ASSERTE(m_count > 0);

    m_count--;

    res.m_fByRef = false;
    res.m_levels = 0;

    switch(m_type)
    {
    case c_Interfaces:
        {
            CLR_RT_TypeDef_Instance cls;

            res.m_dt = DATATYPE_CLASS;

            if(cls.ResolveToken( CLR_TkFromStream( m_sig ), m_assm ) == false)
            {
                TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
            }

            res.m_cls = cls;
        }
        break;

    case c_Object:
        {
            CLR_RT_TypeDescriptor desc;
            CLR_RT_HeapBlock*     ptr = m_lst++;

            if(m_flags)
            {
                // Reflection types are now boxed, so unbox first
                if(ptr->DataType() == DATATYPE_OBJECT)
                {
                    ptr = ptr->Dereference();
                }
                if(ptr->DataType() != DATATYPE_REFLECTION)
                {
                    TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
                }

                TINYCLR_CHECK_HRESULT(desc.InitializeFromReflection( ptr->ReflectionDataConst() ));
            }
            else
            {
                switch(ptr->DataType())
                {
                case DATATYPE_BYREF:
                case DATATYPE_ARRAY_BYREF:
                    res.m_fByRef = true;
                    break;
                }

                TINYCLR_CHECK_HRESULT(desc.InitializeFromObject( *ptr ));
            }

            desc.m_handlerCls.InitializeFromIndex( desc.m_reflex.m_data.m_type );

            res.m_levels =               desc.m_reflex.m_levels;
            res.m_dt     = (CLR_DataType)desc.m_handlerCls.m_target->dataType;
            res.m_cls    =               desc.m_reflex.m_data.m_type;

            //
            // Special case for Object types.
            //
            if(res.m_cls.m_data == g_CLR_RT_WellKnownTypes.m_Object.m_data)
            {
                res.m_dt = DATATYPE_OBJECT;
            }
        }
        break;

    default:
        while(true)
        {
            res.m_dt = CLR_UncompressElementType( m_sig );

            switch(res.m_dt)
            {
            case DATATYPE_BYREF:
                if(res.m_fByRef)
                {
                    TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
                }

                res.m_fByRef = true;
                break;

            case DATATYPE_SZARRAY:
                res.m_levels++;
                break;

            case DATATYPE_CLASS    :
            case DATATYPE_VALUETYPE:
                {
                    CLR_UINT32 tk = CLR_TkFromStream( m_sig );

                    if(CLR_TypeFromTk( tk ) == TBL_TypeSpec)
                    {
                        CLR_RT_SignatureParser sub; sub.Initialize_TypeSpec( m_assm, m_assm->GetTypeSpec( CLR_DataFromTk( tk ) ) );
                        int                    extraLevels = res.m_levels;

                        TINYCLR_CHECK_HRESULT(sub.Advance( res ));

                        res.m_levels += extraLevels;
                    }
                    else
                    {
                        CLR_RT_TypeDef_Instance cls;

                        if(cls.ResolveToken( tk, m_assm ) == false)
                        {
                            TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
                        }

                        res.m_cls = cls;
                    }

                    TINYCLR_SET_AND_LEAVE(S_OK);
                }

            case DATATYPE_OBJECT:
                res.m_cls = g_CLR_RT_WellKnownTypes.m_Object;

                TINYCLR_SET_AND_LEAVE(S_OK);

            case DATATYPE_VOID:
                res.m_cls = g_CLR_RT_WellKnownTypes.m_Void;

                TINYCLR_SET_AND_LEAVE(S_OK);

            default:
                {
                    const CLR_RT_TypeDef_Index* cls = c_CLR_RT_DataTypeLookup[ res.m_dt ].m_cls;

                    if(cls)
                    {
                        res.m_cls = *cls;
                        TINYCLR_SET_AND_LEAVE(S_OK);
                    }
                    else
                    {
                        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
                    }
                }
            }
        }
        break;
    }

    TINYCLR_NOCLEANUP();
}

////////////////////////////////////////////////////////////////////////////////////////////////////

bool CLR_RT_Assembly_Instance::InitializeFromIndex( const CLR_RT_Assembly_Index& idx )
{
    NATIVE_PROFILE_CLR_CORE();
    if(TINYCLR_INDEX_IS_VALID(idx))
    {
        m_data = idx.m_data;
        m_assm = g_CLR_RT_TypeSystem.m_assemblies[ Assembly()-1 ];

        return true;
    }

    m_data = 0;
    m_assm = NULL;

    return false;
}

void CLR_RT_Assembly_Instance::Clear()
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_Assembly_Index::Clear();

    m_assm = NULL;
}

//////////////////////////////

bool CLR_RT_TypeSpec_Instance::InitializeFromIndex( const CLR_RT_TypeSpec_Index& idx )
{
    NATIVE_PROFILE_CLR_CORE();
    if(TINYCLR_INDEX_IS_VALID(idx))
    {
        m_data   =                       idx.m_data;
        m_assm   =                       g_CLR_RT_TypeSystem.m_assemblies[ Assembly()-1 ]       ;
        m_target = m_assm->GetSignature( m_assm->GetTypeSpec             ( TypeSpec()   )->sig );

        return true;
    }

    m_data   = 0;
    m_assm   = NULL;
    m_target = NULL;

    return false;
}

void CLR_RT_TypeSpec_Instance::Clear()
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_TypeSpec_Index::Clear();

    m_assm   = NULL;
    m_target = NULL;
}

bool CLR_RT_TypeSpec_Instance::ResolveToken( CLR_UINT32 tk, CLR_RT_Assembly* assm )
{
    NATIVE_PROFILE_CLR_CORE();
    if(assm && CLR_TypeFromTk( tk ) == TBL_TypeSpec)
    {
        CLR_UINT32 idx = CLR_DataFromTk( tk );

        Set( assm->m_idx, idx );

        m_assm   = assm;
        m_target = assm->GetSignature( assm->GetTypeSpec( idx )->sig );

        return true;
    }

    Clear();

    return false;
}

//////////////////////////////

bool CLR_RT_TypeDef_Instance::InitializeFromReflection( const CLR_RT_ReflectionDef_Index& reflex, CLR_UINT32* levels )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_TypeDef_Index        cls;
    const CLR_RT_TypeDef_Index* ptr = NULL;

    if(levels) *levels = reflex.m_levels;

    switch(reflex.m_kind)
    {
    case REFLECTION_TYPE:
        if(reflex.m_levels > 0 && levels == NULL)
        {
            ptr = &g_CLR_RT_WellKnownTypes.m_Array;
        }
        else
        {
            ptr = &reflex.m_data.m_type;
        }
        break;

    case REFLECTION_TYPE_DELAYED:
        if(g_CLR_RT_TypeSystem.FindTypeDef( reflex.m_data.m_raw, cls ))
        {
            ptr = &cls;
        }
        break;
    }

    return ptr ? InitializeFromIndex( *ptr ) : false;
}

bool CLR_RT_TypeDef_Instance::InitializeFromIndex( const CLR_RT_TypeDef_Index& idx )
{
    NATIVE_PROFILE_CLR_CORE();
    if(TINYCLR_INDEX_IS_VALID(idx))
    {
        m_data   = idx.m_data;
        m_assm   = g_CLR_RT_TypeSystem.m_assemblies[ Assembly()-1 ];
        m_target = m_assm->GetTypeDef              ( Type    ()   );

        return true;
    }

    m_data   = 0;
    m_assm   = NULL;
    m_target = NULL;

    return false;
}

bool CLR_RT_TypeDef_Instance::InitializeFromMethod( const CLR_RT_MethodDef_Instance& md  )
{
    NATIVE_PROFILE_CLR_CORE();
    if(TINYCLR_INDEX_IS_VALID(md))
    {
        CLR_IDX idxAssm = md.Assembly();
        CLR_IDX idxType = md.CrossReference().GetOwner();

        Set( idxAssm, idxType );

        m_assm    = g_CLR_RT_TypeSystem.m_assemblies[ idxAssm-1 ];
        m_target  = m_assm->GetTypeDef              ( idxType   );

        return true;
    }

    Clear();

    return false;
}

bool CLR_RT_TypeDef_Instance::InitializeFromField( const CLR_RT_FieldDef_Instance& fd )
{
    NATIVE_PROFILE_CLR_CORE();
    if(TINYCLR_INDEX_IS_VALID(fd))
    {
        CLR_RT_Assembly*          assm     = fd.m_assm;
        const CLR_RECORD_TYPEDEF* td       = (const CLR_RECORD_TYPEDEF*)assm->GetTable( TBL_TypeDef );
        CLR_IDX                   idxField = fd.Field();
        int                       i        = assm->m_pTablesSize[ TBL_TypeDef ];

        if(fd.m_target->flags & CLR_RECORD_FIELDDEF::FD_Static)
        {
            for(;i; i--, td++)
            {
                if(td->sFields_First <= idxField && idxField < td->sFields_First + td->sFields_Num)
                {
                    break;
                }
            }
        }
        else
        {
            for(;i; i--, td++)
            {
                if(td->iFields_First <= idxField && idxField < td->iFields_First + td->iFields_Num)
                {
                    break;
                }
            }
        }

        if(i)
        {
            CLR_IDX idxAssm = fd.Assembly();
            CLR_IDX idxType = assm->m_pTablesSize[ TBL_TypeDef ] - i;

            Set( idxAssm, idxType );

            m_assm    = g_CLR_RT_TypeSystem.m_assemblies[ idxAssm-1 ];
            m_target  = m_assm->GetTypeDef              ( idxType   );

            return true;
        }
    }

    Clear();

    return false;
}

bool CLR_RT_TypeDef_Instance::IsATypeHandler()
{
    NATIVE_PROFILE_CLR_CORE();
    return (m_data == g_CLR_RT_WellKnownTypes.m_Type.m_data || m_data == g_CLR_RT_WellKnownTypes.m_TypeStatic.m_data);
}

void CLR_RT_TypeDef_Instance::Clear()
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_TypeDef_Index::Clear();

    m_assm   = NULL;
    m_target = NULL;
}

bool CLR_RT_TypeDef_Instance::ResolveToken( CLR_UINT32 tk, CLR_RT_Assembly* assm )
{
    NATIVE_PROFILE_CLR_CORE();
    if(assm)
    {
        CLR_UINT32 idx = CLR_DataFromTk( tk );

        switch( CLR_TypeFromTk( tk ) )
        {
        case TBL_TypeRef:
            m_data   = assm->m_pCrossReference_TypeRef[ idx ].m_target.m_data;
            m_assm   = g_CLR_RT_TypeSystem.m_assemblies[ Assembly()-1 ];
            m_target = m_assm->GetTypeDef              ( Type    ()   );
            return true;

        case TBL_TypeDef:
            Set( assm->m_idx, idx );

            m_assm   = assm;
            m_target = assm->GetTypeDef( idx );
            return true;
        }
    }

    Clear();

    return false;
}

//--//

bool CLR_RT_TypeDef_Instance::SwitchToParent()
{
    NATIVE_PROFILE_CLR_CORE();
    if(TINYCLR_INDEX_IS_VALID(*this))
    {
        CLR_IDX extends = m_target->extends;

        if(extends != CLR_EmptyIndex)
        {
            CLR_RT_TypeDef_Index        tmp;
            const CLR_RT_TypeDef_Index* cls;

            if(extends & 0x8000) // TypeRef
            {
                cls = &m_assm->m_pCrossReference_TypeRef[ extends & 0x7FFF ].m_target;
            }
            else
            {
                tmp.Set( Assembly(), extends );

                cls = &tmp;
            }

            return InitializeFromIndex( *cls );
        }
    }

    Clear();

    return false;
}

bool CLR_RT_TypeDef_Instance::HasFinalizer() const
{
    NATIVE_PROFILE_CLR_CORE();
    return TINYCLR_INDEX_IS_VALID(*this) && (CrossReference().m_flags & CLR_RT_TypeDef_CrossReference::TD_CR_HasFinalizer);
}

//////////////////////////////

bool CLR_RT_FieldDef_Instance::InitializeFromIndex( const CLR_RT_FieldDef_Index& idx )
{
    NATIVE_PROFILE_CLR_CORE();
    if(TINYCLR_INDEX_IS_VALID(idx))
    {
        m_data   = idx.m_data;
        m_assm   = g_CLR_RT_TypeSystem.m_assemblies[ Assembly()-1 ];
        m_target = m_assm->GetFieldDef             ( Field   ()   );

        return true;
    }

    m_data   = 0;
    m_assm   = NULL;
    m_target = NULL;

    return false;
}

void CLR_RT_FieldDef_Instance::Clear()
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_FieldDef_Index::Clear();

    m_assm   = NULL;
    m_target = NULL;
}

bool CLR_RT_FieldDef_Instance::ResolveToken( CLR_UINT32 tk, CLR_RT_Assembly* assm )
{
    NATIVE_PROFILE_CLR_CORE();
    if(assm)
    {
        CLR_UINT32 idx = CLR_DataFromTk( tk );

        switch(CLR_TypeFromTk( tk ))
        {
        case TBL_FieldRef:
            m_data   = assm->m_pCrossReference_FieldRef[ idx ].m_target.m_data;
            m_assm   = g_CLR_RT_TypeSystem.m_assemblies[ Assembly()-1 ];
            m_target = m_assm->GetFieldDef             ( Field   ()   );
            return true;

        case TBL_FieldDef:
            Set( assm->m_idx, idx );

            m_assm   = assm;
            m_target = m_assm->GetFieldDef( idx );
            return true;
        }
    }

    Clear();

    return false;
}

//////////////////////////////

bool CLR_RT_MethodDef_Instance::InitializeFromIndex( const CLR_RT_MethodDef_Index& idx )
{
    NATIVE_PROFILE_CLR_CORE();
    if(TINYCLR_INDEX_IS_VALID(idx))
    {
        m_data   = idx.m_data;
        m_assm   = g_CLR_RT_TypeSystem.m_assemblies[ Assembly()-1 ];
        m_target = m_assm->GetMethodDef            ( Method  ()   );

        return true;
    }

    m_data   = 0;
    m_assm   = NULL;
    m_target = NULL;

    return false;
}

void CLR_RT_MethodDef_Instance::Clear()
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_MethodDef_Index::Clear();

    m_assm   = NULL;
    m_target = NULL;
}

bool CLR_RT_MethodDef_Instance::ResolveToken( CLR_UINT32 tk, CLR_RT_Assembly* assm )
{
    NATIVE_PROFILE_CLR_CORE();
    if(assm)
    {
        CLR_UINT32 idx = CLR_DataFromTk( tk );

        switch(CLR_TypeFromTk( tk ))
        {
        case TBL_MethodRef:
            m_data   = assm->m_pCrossReference_MethodRef[ idx ].m_target.m_data;
            m_assm   = g_CLR_RT_TypeSystem.m_assemblies[ Assembly()-1 ];
            m_target = m_assm->GetMethodDef            ( Method  ()   );
            return true;

        case TBL_MethodDef:
            Set( assm->m_idx, idx );

            m_assm   = assm;
            m_target = m_assm->GetMethodDef( idx );
            return true;
        }
    }

    Clear();

    return false;
}

////////////////////////////////////////////////////////////////////////////////////////////////////

void CLR_RT_TypeDescriptor::TypeDescriptor_Initialize()
{
    NATIVE_PROFILE_CLR_CORE();
    m_flags = 0;          // CLR_UINT32                 m_flags;
    m_handlerCls.Clear(); // CLR_RT_TypeDef_Instance    m_handlerCls;
                          //
    m_reflex    .Clear(); // CLR_RT_ReflectionDef_Index m_reflex;
}

HRESULT CLR_RT_TypeDescriptor::InitializeFromDataType( CLR_DataType dt )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    if(dt >= DATATYPE_FIRST_INVALID)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }
    else
    {
        const CLR_RT_DataTypeLookup& dtl = c_CLR_RT_DataTypeLookup[ dt ];

        m_flags = dtl.m_flags & CLR_RT_DataTypeLookup::c_SemanticMask2;

        if(dtl.m_cls)
        {
            if(m_handlerCls.InitializeFromIndex( *dtl.m_cls ) == false)
            {
                TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
            }

            m_reflex.m_kind        = REFLECTION_TYPE;
            m_reflex.m_levels      = 0;
            m_reflex.m_data.m_type = *dtl.m_cls;
        }
        else
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
        }
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_TypeDescriptor::InitializeFromReflection( const CLR_RT_ReflectionDef_Index& reflex )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_TypeDef_Instance inst;
    CLR_UINT32              levels;

    if(inst.InitializeFromReflection( reflex, &levels ) == false)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    TINYCLR_CHECK_HRESULT(InitializeFromType( inst ));

    if(levels)
    {
        m_reflex.m_levels = levels;

        ConvertToArray();
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_TypeDescriptor::InitializeFromTypeSpec( const CLR_RT_TypeSpec_Index& sig )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_TypeSpec_Instance inst;
    CLR_RT_SignatureParser   parser;

    if(inst.InitializeFromIndex( sig ) == false)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    parser.Initialize_TypeSpec( inst.m_assm, inst.m_target );

    TINYCLR_SET_AND_LEAVE(InitializeFromSignatureParser( parser ));

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_TypeDescriptor::InitializeFromType( const CLR_RT_TypeDef_Index& cls )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    if(m_handlerCls.InitializeFromIndex( cls ) == false)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }
    else
    {
        const CLR_RT_DataTypeLookup& dtl = c_CLR_RT_DataTypeLookup[ m_handlerCls.m_target->dataType ];

        m_flags                = dtl.m_flags & CLR_RT_DataTypeLookup::c_SemanticMask;

        m_reflex.m_kind        = REFLECTION_TYPE;
        m_reflex.m_levels      = 0;
        m_reflex.m_data.m_type = m_handlerCls;

        if(m_flags == CLR_RT_DataTypeLookup::c_Primitive)
        {
            if((m_handlerCls.m_target->flags & CLR_RECORD_TYPEDEF::TD_Semantics_Mask) == CLR_RECORD_TYPEDEF::TD_Semantics_Enum)
            {
                m_flags = CLR_RT_DataTypeLookup::c_Enum;
            }
        }
        else
        {
            switch(m_handlerCls.m_target->flags & CLR_RECORD_TYPEDEF::TD_Semantics_Mask)
            {
            case CLR_RECORD_TYPEDEF::TD_Semantics_ValueType: m_flags = CLR_RT_DataTypeLookup::c_ValueType; break;
            case CLR_RECORD_TYPEDEF::TD_Semantics_Class    : m_flags = CLR_RT_DataTypeLookup::c_Class    ; break;
            case CLR_RECORD_TYPEDEF::TD_Semantics_Interface: m_flags = CLR_RT_DataTypeLookup::c_Interface; break;
            case CLR_RECORD_TYPEDEF::TD_Semantics_Enum     : m_flags = CLR_RT_DataTypeLookup::c_Enum     ; break;
            }
        }

        if(m_handlerCls.m_data == g_CLR_RT_WellKnownTypes.m_Array.m_data)
        {
            m_flags |= CLR_RT_DataTypeLookup::c_Array;
        }

        if(m_handlerCls.m_data == g_CLR_RT_WellKnownTypes.m_ArrayList.m_data)
        {
            m_flags |= CLR_RT_DataTypeLookup::c_ArrayList;
        }
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_TypeDescriptor::InitializeFromFieldDefinition( const CLR_RT_FieldDef_Instance& fd )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_SignatureParser parser; parser.Initialize_FieldDef( fd.m_assm, fd.m_target );

    TINYCLR_SET_AND_LEAVE(InitializeFromSignatureParser( parser ));

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_TypeDescriptor::InitializeFromSignatureParser( CLR_RT_SignatureParser& parser )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_SignatureParser::Element res;

    if(parser.Available() <= 0)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);
    }

    TINYCLR_CHECK_HRESULT(parser.Advance( res ));

    TINYCLR_CHECK_HRESULT(InitializeFromType( res.m_cls ));

    if(res.m_levels)
    {
        m_reflex.m_levels = res.m_levels;

        ConvertToArray();
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_TypeDescriptor::InitializeFromObject( const CLR_RT_HeapBlock& ref )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    const CLR_RT_HeapBlock* obj = &ref;
    CLR_DataType            dt;


    while(true)
    {
        dt = (CLR_DataType)obj->DataType();

        if(dt == DATATYPE_BYREF ||
           dt == DATATYPE_OBJECT )
        {
            obj = obj->Dereference(); FAULT_ON_NULL(obj);
        }
#if defined(TINYCLR_APPDOMAINS)
        else if(dt == DATATYPE_TRANSPARENT_PROXY)
        {
            TINYCLR_CHECK_HRESULT(obj->TransparentProxyValidate());
            obj = obj->TransparentProxyDereference();
        }
#endif
        else
        {
            break;
        }
    }

    {
        const CLR_RT_TypeDef_Index*       cls    = NULL;
        const CLR_RT_ReflectionDef_Index* reflex = NULL;

        switch(dt)
        {
        case DATATYPE_SZARRAY:
            reflex = &obj->ReflectionDataConst();
            cls    = &reflex->m_data.m_type;
            break;

        case DATATYPE_VALUETYPE:
        case DATATYPE_CLASS:
            cls = &obj->ObjectCls();
            break;

        case DATATYPE_DELEGATE_HEAD:
            {
                CLR_RT_HeapBlock_Delegate* dlg = (CLR_RT_HeapBlock_Delegate*)obj;

                cls = TINYCLR_INDEX_IS_VALID(dlg->m_cls) ? &dlg->m_cls : &g_CLR_RT_WellKnownTypes.m_Delegate;
            }
            break;

        case DATATYPE_DELEGATELIST_HEAD:
            {
                CLR_RT_HeapBlock_Delegate_List* dlgLst = (CLR_RT_HeapBlock_Delegate_List*)obj;

                cls = TINYCLR_INDEX_IS_VALID(dlgLst->m_cls) ? &dlgLst->m_cls : &g_CLR_RT_WellKnownTypes.m_MulticastDelegate;
            }
            break;

        //--//

        case DATATYPE_WEAKCLASS:
            {
                CLR_RT_HeapBlock_WeakReference* weak = (CLR_RT_HeapBlock_WeakReference*)obj;

                if(weak->m_identity.m_flags & CLR_RT_HeapBlock_WeakReference::WR_ExtendedType)
                {
                    cls = &g_CLR_RT_WellKnownTypes.m_ExtendedWeakReference;
                }
                else
                {
                    cls = &g_CLR_RT_WellKnownTypes.m_WeakReference;
                }
            }
            break;

        //--//

        case DATATYPE_REFLECTION:
            reflex = &(obj->ReflectionDataConst());

            switch(reflex->m_kind)
            {
            case REFLECTION_ASSEMBLY    : cls = &g_CLR_RT_WellKnownTypes.m_Assembly       ; break;
            case REFLECTION_TYPE        : cls = &g_CLR_RT_WellKnownTypes.m_Type           ; break;
            case REFLECTION_TYPE_DELAYED: cls = &g_CLR_RT_WellKnownTypes.m_Type           ; break;
            case REFLECTION_CONSTRUCTOR : cls = &g_CLR_RT_WellKnownTypes.m_ConstructorInfo; break;
            case REFLECTION_METHOD      : cls = &g_CLR_RT_WellKnownTypes.m_MethodInfo     ; break;
            case REFLECTION_FIELD       : cls = &g_CLR_RT_WellKnownTypes.m_FieldInfo      ; break;
            }

            break;

        //--//

        case DATATYPE_ARRAY_BYREF:
            {
                CLR_RT_HeapBlock_Array* array = obj->Array(); FAULT_ON_NULL(array);

                if(array->m_fReference)
                {
                    obj = (CLR_RT_HeapBlock*)array->GetElement( obj->ArrayIndex() );

                    TINYCLR_SET_AND_LEAVE(InitializeFromObject( *obj ));
                }

                reflex = &array->ReflectionDataConst();
                cls    = &reflex->m_data.m_type;
            }
            break;

        //--//

        default:
            TINYCLR_SET_AND_LEAVE(InitializeFromDataType( dt ));
        }

        if(cls)
        {
            TINYCLR_CHECK_HRESULT(InitializeFromType( *cls ));
        }

        if(reflex)
        {
            m_reflex = *reflex;
        }

        if(dt == DATATYPE_SZARRAY)
        {
            ConvertToArray();
        }
    }

    TINYCLR_NOCLEANUP();
}

////////////////////////////////////////

void CLR_RT_TypeDescriptor::ConvertToArray()
{
    NATIVE_PROFILE_CLR_CORE();
    m_flags &= CLR_RT_DataTypeLookup::c_SemanticMask;
    m_flags |= CLR_RT_DataTypeLookup::c_Array;

    m_handlerCls.InitializeFromIndex( g_CLR_RT_WellKnownTypes.m_Array );
}

bool CLR_RT_TypeDescriptor::ShouldEmitHash()
{
    NATIVE_PROFILE_CLR_CORE();
    if(m_flags & (CLR_RT_DataTypeLookup::c_Array | CLR_RT_DataTypeLookup::c_ArrayList))
    {
        return true;
    }

    if(m_flags & (CLR_RT_DataTypeLookup::c_Primitive | CLR_RT_DataTypeLookup::c_ValueType | CLR_RT_DataTypeLookup::c_Enum))
    {
        return false;
    }

    if(m_handlerCls.CrossReference().m_hash != 0)
    {
        return true;
    }

    return false;
}

bool CLR_RT_TypeDescriptor::GetElementType( CLR_RT_TypeDescriptor& sub )
{
    NATIVE_PROFILE_CLR_CORE();
    switch(m_reflex.m_levels)
    {
    case 0:
        return false;

    case 1:
        sub.InitializeFromType( m_reflex.m_data.m_type );
        break;

    default:
        sub = *this;
        sub.m_reflex.m_levels--;
        break;
    }

    return true;
}

////////////////////////////////////////

HRESULT CLR_RT_TypeDescriptor::ExtractObjectAndDataType( CLR_RT_HeapBlock*& ref, CLR_DataType& dt )
{
    NATIVE_PROFILE_CLR_CORE();

    TINYCLR_HEADER();

    while(true)
    {
        dt = (CLR_DataType)ref->DataType();

        if(dt == DATATYPE_BYREF ||
           dt == DATATYPE_OBJECT )
        {
            ref = ref->Dereference(); FAULT_ON_NULL(ref);
        }
        else
        {
            break;
        }
    }

    TINYCLR_NOCLEANUP();
}


HRESULT CLR_RT_TypeDescriptor::ExtractTypeIndexFromObject( const CLR_RT_HeapBlock& ref, CLR_RT_TypeDef_Index& res )
{
    NATIVE_PROFILE_CLR_CORE();

    TINYCLR_HEADER();

    CLR_RT_HeapBlock* obj = (CLR_RT_HeapBlock*)&ref;
    CLR_DataType            dt;

    TINYCLR_CHECK_HRESULT(CLR_RT_TypeDescriptor::ExtractObjectAndDataType(obj, dt));

    if(dt == DATATYPE_VALUETYPE || dt == DATATYPE_CLASS)
    {
        res = obj->ObjectCls();
    }
    else
    {
        const CLR_RT_DataTypeLookup& dtl = c_CLR_RT_DataTypeLookup[ dt ];

        if(dtl.m_cls)
        {
            res = *dtl.m_cls;
        }
        else
        {
            res.Clear();
        }
    }

    if(TINYCLR_INDEX_IS_INVALID(res))
    {
        CLR_RT_TypeDescriptor desc;

        TINYCLR_CHECK_HRESULT(desc.InitializeFromObject( ref ))
        
        // If desc.InitializeFromObject( ref ) call succeded, then we use m_handlerCls for res
        res = desc.m_handlerCls;

        if(TINYCLR_INDEX_IS_INVALID(res))
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
        }
    }

    TINYCLR_NOCLEANUP();
}

////////////////////////////////////////////////////////////////////////////////////////////////////

///////////////////////////////////////////////////////////////////////////////////////////////////

//
// Keep these strings less than 8-character long!! They are stuffed into an 8-byte structure.
//
static const char c_MARKER_ASSEMBLY_V1[] = "MSSpot1";

bool CLR_RECORD_ASSEMBLY::GoodHeader() const
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RECORD_ASSEMBLY header = *this; header.headerCRC = 0;

#if !defined(NETMF_TARGET_BIG_ENDIAN)
    if ( (header.flags & CLR_RECORD_ASSEMBLY::c_Flags_BigEndian) == CLR_RECORD_ASSEMBLY::c_Flags_BigEndian)
#else
    if ( (header.flags & CLR_RECORD_ASSEMBLY::c_Flags_BigEndian) != CLR_RECORD_ASSEMBLY::c_Flags_BigEndian)
#endif
    {
        // Incorrect endianness
        return false;
    }

    if(SUPPORT_ComputeCRC( &header, sizeof(header), 0 ) != this->headerCRC) return false;

    if(this->stringTableVersion != c_CLR_StringTable_Version) return false;

    return memcmp( marker, c_MARKER_ASSEMBLY_V1, sizeof(marker) ) == 0;
}

bool CLR_RECORD_ASSEMBLY::GoodAssembly() const
{
    NATIVE_PROFILE_CLR_CORE();
    if(!GoodHeader()) return false;
    return SUPPORT_ComputeCRC( &this[ 1 ], this->TotalSize() - sizeof(*this), 0 ) == this->assemblyCRC;
}

#if defined(WIN32)

void CLR_RECORD_ASSEMBLY::ComputeCRC()
{
    NATIVE_PROFILE_CLR_CORE();
    memcpy( marker, c_MARKER_ASSEMBLY_V1, sizeof(marker) );

    headerCRC   = 0;
    assemblyCRC = SUPPORT_ComputeCRC( &this[ 1 ], this->TotalSize() - sizeof(*this), 0 );
    headerCRC   = SUPPORT_ComputeCRC(  this     ,                     sizeof(*this), 0 );
}

#endif

CLR_UINT32 CLR_RECORD_ASSEMBLY::ComputeAssemblyHash( LPCSTR name, const CLR_RECORD_VERSION& ver )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_UINT32 assemblyHASH;

    assemblyHASH = SUPPORT_ComputeCRC( name, (int)hal_strlen_s( name ), 0            );
    assemblyHASH = SUPPORT_ComputeCRC( &ver,            sizeof( ver  ), assemblyHASH );

    return assemblyHASH;
}

//--//

CLR_PMETADATA CLR_RECORD_EH::ExtractEhFromByteCode( CLR_PMETADATA ipEnd, const CLR_RECORD_EH*& ptrEh, CLR_UINT32& numEh )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_UINT32 num = *(--ipEnd); ipEnd -= sizeof(CLR_RECORD_EH) * num;

    numEh = num;
    ptrEh = (const CLR_RECORD_EH*)ipEnd;

    return ipEnd;
}

CLR_UINT32 CLR_RECORD_EH::GetToken() const
{
    NATIVE_PROFILE_CLR_CORE();
    if(classToken & 0x8000)
    {
        return CLR_TkFromType( TBL_TypeRef, classToken & 0x7FFF );
    }
    else
    {
        return CLR_TkFromType( TBL_TypeDef, classToken );
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////////

bool CLR_RT_ExceptionHandler::ConvertFromEH( const CLR_RT_MethodDef_Instance& owner, CLR_PMETADATA ipStart, const CLR_RECORD_EH* ehPtr )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RECORD_EH eh; memcpy( &eh, ehPtr, sizeof(eh) );

    switch(eh.mode)
    {
    case CLR_RECORD_EH::EH_Finally:
        m_typeFilter.Clear();
        break;

    case CLR_RECORD_EH::EH_Filter:
        m_userFilterStart = ipStart + eh.filterStart;
        break;

    case CLR_RECORD_EH::EH_CatchAll:
        m_typeFilter = g_CLR_RT_WellKnownTypes.m_Object;
        break;

    case CLR_RECORD_EH::EH_Catch:
        {
            CLR_RT_TypeDef_Instance cls;
            if(cls.ResolveToken( eh.GetToken(), owner.m_assm ) == false) return false;
            m_typeFilter = cls;
        }
        break;

    default:
        return false;
    }

    if(owner.m_target->RVA == CLR_EmptyIndex) return false;

    m_ehType       = eh.mode;
    m_tryStart     = ipStart + eh.tryStart;
    m_tryEnd       = ipStart + eh.tryEnd;
    m_handlerStart = ipStart + eh.handlerStart;
    m_handlerEnd   = ipStart + eh.handlerEnd;

    return true;
}

////////////////////////////////////////////////////////////////////////////////////////////////////


bool CLR_RT_Assembly::IsSameAssembly( const CLR_RT_Assembly& assm ) const
{
    if( 
        m_header->headerCRC   == assm.m_header->headerCRC   && 
        m_header->assemblyCRC == assm.m_header->assemblyCRC
      ) 
    {
        return true;
    }

    return false;
}

void CLR_RT_Assembly::Assembly_Initialize( CLR_RT_Assembly::Offsets& offsets )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_UINT8* buffer = (CLR_UINT8*)this;
    int        i;

    m_szName = GetString( m_header->assemblyName );

    //--//
                                                                                buffer += offsets.iBase          ;
    m_pCrossReference_AssemblyRef = (CLR_RT_AssemblyRef_CrossReference*)buffer; buffer += offsets.iAssemblyRef   ;
    m_pCrossReference_TypeRef     = (CLR_RT_TypeRef_CrossReference    *)buffer; buffer += offsets.iTypeRef       ;
    m_pCrossReference_FieldRef    = (CLR_RT_FieldRef_CrossReference   *)buffer; buffer += offsets.iFieldRef      ;
    m_pCrossReference_MethodRef   = (CLR_RT_MethodRef_CrossReference  *)buffer; buffer += offsets.iMethodRef     ;
    m_pCrossReference_TypeDef     = (CLR_RT_TypeDef_CrossReference    *)buffer; buffer += offsets.iTypeDef       ;
    m_pCrossReference_FieldDef    = (CLR_RT_FieldDef_CrossReference   *)buffer; buffer += offsets.iFieldDef      ;
    m_pCrossReference_MethodDef   = (CLR_RT_MethodDef_CrossReference  *)buffer; buffer += offsets.iMethodDef     ;

#if !defined(TINYCLR_APPDOMAINS)
    m_pStaticFields               = (CLR_RT_HeapBlock                 *)buffer; buffer += offsets.iStaticFields  ;

    memset( m_pStaticFields, 0, offsets.iStaticFields );
#endif

    //--//

    {
        ITERATE_THROUGH_RECORDS(this,i,TypeDef,TYPEDEF)
        {
            dst->m_flags       = 0;
            dst->m_totalFields = 0;
            dst->m_hash        = 0;
        }
    }

    {
        ITERATE_THROUGH_RECORDS(this,i,FieldDef,FIELDDEF)
        {
            dst->m_offset = CLR_EmptyIndex;
        }
    }

    {
        ITERATE_THROUGH_RECORDS(this,i,MethodDef,METHODDEF)
        {
            dst->m_data = CLR_EmptyIndex;
        }
    }

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
    {
        m_pDebuggingInfo_MethodDef = (CLR_RT_MethodDef_DebuggingInfo*)buffer; buffer += offsets.iDebuggingInfoMethods;

        memset( m_pDebuggingInfo_MethodDef, 0, offsets.iDebuggingInfoMethods );
    }
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
}

HRESULT CLR_RT_Assembly::CreateInstance( const CLR_RECORD_ASSEMBLY* header, CLR_RT_Assembly*& assm )
{
    NATIVE_PROFILE_CLR_CORE();
    //
    // We have to use this trick, otherwise the C++ compiler will try to all the constructor for Assembly.
    //
    TINYCLR_HEADER();

    CLR_UINT8        buf[ sizeof(CLR_RT_Assembly) ];
    CLR_RT_Assembly* skeleton = (CLR_RT_Assembly*)buf;

    TINYCLR_CLEAR(*skeleton);

    if(header->GoodAssembly() == false) TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);

    skeleton->m_header = header;


    //
    // Compute overall size for assembly data structure.
    //
    {
        for(int i=0; i<ARRAYSIZE(skeleton->m_pTablesSize)-1; i++)
        {
            skeleton->m_pTablesSize[ i ]  = header->SizeOfTable    ( (CLR_TABLESENUM)i );
        }

        skeleton->m_pTablesSize[ TBL_AssemblyRef    ] /= sizeof(CLR_RECORD_ASSEMBLYREF  );
        skeleton->m_pTablesSize[ TBL_TypeRef        ] /= sizeof(CLR_RECORD_TYPEREF      );
        skeleton->m_pTablesSize[ TBL_FieldRef       ] /= sizeof(CLR_RECORD_FIELDREF     );
        skeleton->m_pTablesSize[ TBL_MethodRef      ] /= sizeof(CLR_RECORD_METHODREF    );
        skeleton->m_pTablesSize[ TBL_TypeDef        ] /= sizeof(CLR_RECORD_TYPEDEF      );
        skeleton->m_pTablesSize[ TBL_FieldDef       ] /= sizeof(CLR_RECORD_FIELDDEF     );
        skeleton->m_pTablesSize[ TBL_MethodDef      ] /= sizeof(CLR_RECORD_METHODDEF    );
        skeleton->m_pTablesSize[ TBL_Attributes     ] /= sizeof(CLR_RECORD_ATTRIBUTE    );
        skeleton->m_pTablesSize[ TBL_TypeSpec       ] /= sizeof(CLR_RECORD_TYPESPEC     );
        skeleton->m_pTablesSize[ TBL_Resources      ] /= sizeof(CLR_RECORD_RESOURCE     );
        skeleton->m_pTablesSize[ TBL_ResourcesFiles ] /= sizeof(CLR_RECORD_RESOURCE_FILE);
    }

    //--//

    //
    // Count static fields.
    //
    {
        const CLR_RECORD_TYPEDEF* src = (const CLR_RECORD_TYPEDEF*)skeleton->GetTable( TBL_TypeDef );

        for(int i=0; i<skeleton->m_pTablesSize[ TBL_TypeDef ]; i++, src++)
        {
            skeleton->m_iStaticFields += src->sFields_Num;
        }
    }

    //--//

    {
        CLR_RT_Assembly::Offsets offsets;

        offsets.iBase                 = ROUNDTOMULTIPLE(sizeof(CLR_RT_Assembly)                                                               , CLR_UINT32);
        offsets.iAssemblyRef          = ROUNDTOMULTIPLE(skeleton->m_pTablesSize[ TBL_AssemblyRef ] * sizeof(CLR_RT_AssemblyRef_CrossReference), CLR_UINT32);
        offsets.iTypeRef              = ROUNDTOMULTIPLE(skeleton->m_pTablesSize[ TBL_TypeRef     ] * sizeof(CLR_RT_TypeRef_CrossReference    ), CLR_UINT32);
        offsets.iFieldRef             = ROUNDTOMULTIPLE(skeleton->m_pTablesSize[ TBL_FieldRef    ] * sizeof(CLR_RT_FieldRef_CrossReference   ), CLR_UINT32);
        offsets.iMethodRef            = ROUNDTOMULTIPLE(skeleton->m_pTablesSize[ TBL_MethodRef   ] * sizeof(CLR_RT_MethodRef_CrossReference  ), CLR_UINT32);
        offsets.iTypeDef              = ROUNDTOMULTIPLE(skeleton->m_pTablesSize[ TBL_TypeDef     ] * sizeof(CLR_RT_TypeDef_CrossReference    ), CLR_UINT32);
        offsets.iFieldDef             = ROUNDTOMULTIPLE(skeleton->m_pTablesSize[ TBL_FieldDef    ] * sizeof(CLR_RT_FieldDef_CrossReference   ), CLR_UINT32);
        offsets.iMethodDef            = ROUNDTOMULTIPLE(skeleton->m_pTablesSize[ TBL_MethodDef   ] * sizeof(CLR_RT_MethodDef_CrossReference  ), CLR_UINT32);

        if(skeleton->m_header->numOfPatchedMethods > 0)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_ASSM_PATCHING_NOT_SUPPORTED);
        }

#if !defined(TINYCLR_APPDOMAINS)
        offsets.iStaticFields         = ROUNDTOMULTIPLE(skeleton->m_iStaticFields                * sizeof(CLR_RT_HeapBlock                 ), CLR_UINT32);
#endif

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
        offsets.iDebuggingInfoMethods = ROUNDTOMULTIPLE(skeleton->m_pTablesSize[ TBL_MethodDef ] * sizeof(CLR_RT_MethodDef_DebuggingInfo   ), CLR_UINT32);
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

        size_t iTotalRamSize = offsets.iBase           +
                               offsets.iAssemblyRef    +
                               offsets.iTypeRef        +
                               offsets.iFieldRef       +
                               offsets.iMethodRef      +
                               offsets.iTypeDef        +
                               offsets.iFieldDef       +
                               offsets.iMethodDef;

#if !defined(TINYCLR_APPDOMAINS)
        iTotalRamSize += offsets.iStaticFields;
#endif

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
        iTotalRamSize += offsets.iDebuggingInfoMethods;
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

        //--//

        assm = EVENTCACHE_EXTRACT_NODE_AS_BYTES(g_CLR_RT_EventCache,CLR_RT_Assembly,DATATYPE_ASSEMBLY,0,(CLR_UINT32)iTotalRamSize); CHECK_ALLOCATION(assm);

        {
            //
            // We don't want to blow away the block header...
            //
            CLR_RT_HeapBlock* src = skeleton;
            CLR_RT_HeapBlock* dst = assm;

            memset( &dst[ 1 ],         0, iTotalRamSize-sizeof(CLR_RT_HeapBlock) );
            memcpy( &dst[ 1 ], &src[ 1 ], sizeof(*assm)-sizeof(CLR_RT_HeapBlock) );
        }

        assm->Assembly_Initialize( offsets );
        
#if !defined(BUILD_RTM)
        CLR_Debug::Printf( "   Assembly: %s (%d.%d.%d.%d)  ", assm->m_szName, header->version.iMajorVersion, header->version.iMinorVersion, header->version.iBuildNumber, header->version.iRevisionNumber );

        if(s_CLR_RT_fTrace_AssemblyOverhead >= c_CLR_RT_Trace_Info)
        {
            size_t iMetaData = header->SizeOfTable( TBL_AssemblyRef ) +
                               header->SizeOfTable( TBL_TypeRef     ) +
                               header->SizeOfTable( TBL_FieldRef    ) +
                               header->SizeOfTable( TBL_MethodRef   ) +
                               header->SizeOfTable( TBL_TypeDef     ) +
                               header->SizeOfTable( TBL_FieldDef    ) +
                               header->SizeOfTable( TBL_MethodDef   ) +
                               header->SizeOfTable( TBL_Attributes  ) +
                               header->SizeOfTable( TBL_TypeSpec    ) +
                               header->SizeOfTable( TBL_Signatures  );

            CLR_Debug::Printf( " (%d RAM - %d ROM - %d METADATA)\r\n\r\n", iTotalRamSize, header->TotalSize(), iMetaData );

            CLR_Debug::Printf( "   AssemblyRef    = %8d bytes (%8d elements)\r\n", offsets.iAssemblyRef   , skeleton->m_pTablesSize[ TBL_AssemblyRef ] );
            CLR_Debug::Printf( "   TypeRef        = %8d bytes (%8d elements)\r\n", offsets.iTypeRef       , skeleton->m_pTablesSize[ TBL_TypeRef     ] );
            CLR_Debug::Printf( "   FieldRef       = %8d bytes (%8d elements)\r\n", offsets.iFieldRef      , skeleton->m_pTablesSize[ TBL_FieldRef    ] );
            CLR_Debug::Printf( "   MethodRef      = %8d bytes (%8d elements)\r\n", offsets.iMethodRef     , skeleton->m_pTablesSize[ TBL_MethodRef   ] );
            CLR_Debug::Printf( "   TypeDef        = %8d bytes (%8d elements)\r\n", offsets.iTypeDef       , skeleton->m_pTablesSize[ TBL_TypeDef     ] );
            CLR_Debug::Printf( "   FieldDef       = %8d bytes (%8d elements)\r\n", offsets.iFieldDef      , skeleton->m_pTablesSize[ TBL_FieldDef    ] );
            CLR_Debug::Printf( "   MethodDef      = %8d bytes (%8d elements)\r\n", offsets.iMethodDef     , skeleton->m_pTablesSize[ TBL_MethodDef   ] );
#if !defined(TINYCLR_APPDOMAINS) 
            CLR_Debug::Printf( "   StaticFields   = %8d bytes (%8d elements)\r\n", offsets.iStaticFields  , skeleton->m_iStaticFields                );
#endif
            CLR_Debug::Printf( "\r\n" );

            CLR_Debug::Printf( "   Attributes      = %8d bytes (%8d elements)\r\n", skeleton->m_pTablesSize[ TBL_Attributes     ] * sizeof(CLR_RECORD_ATTRIBUTE), skeleton->m_pTablesSize[ TBL_Attributes     ] );
            CLR_Debug::Printf( "   TypeSpec        = %8d bytes (%8d elements)\r\n", skeleton->m_pTablesSize[ TBL_TypeSpec       ] * sizeof(CLR_RECORD_TYPESPEC ), skeleton->m_pTablesSize[ TBL_TypeSpec       ] );
            CLR_Debug::Printf( "   Resources       = %8d bytes (%8d elements)\r\n", skeleton->m_pTablesSize[ TBL_Resources      ] * sizeof(CLR_RECORD_RESOURCE ), skeleton->m_pTablesSize[ TBL_Resources      ] );
            CLR_Debug::Printf( "   Resources Files = %8d bytes (%8d elements)\r\n", skeleton->m_pTablesSize[ TBL_ResourcesFiles ] * sizeof(CLR_RECORD_RESOURCE ), skeleton->m_pTablesSize[ TBL_ResourcesFiles ] );
            CLR_Debug::Printf( "   Resources Data  = %8d bytes\r\n"                                                                                             , skeleton->m_pTablesSize[ TBL_ResourcesData  ] );
            CLR_Debug::Printf( "   Strings         = %8d bytes\r\n"                                                                                             , skeleton->m_pTablesSize[ TBL_Strings        ] );
            CLR_Debug::Printf( "   Signatures      = %8d bytes\r\n"                                                                                             , skeleton->m_pTablesSize[ TBL_Signatures     ] );
            CLR_Debug::Printf( "   ByteCode        = %8d bytes\r\n"                                                                                             , skeleton->m_pTablesSize[ TBL_ByteCode       ] );
            CLR_Debug::Printf( "\r\n\r\n" );
        }
#endif
    }

    TINYCLR_NOCLEANUP();
}


#if defined(WIN32)
HRESULT CLR_RT_Assembly::CreateInstance( const CLR_RECORD_ASSEMBLY* header, CLR_RT_Assembly*& assm, LPCWSTR szName )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    std::string strPath;

    TINYCLR_CHECK_HRESULT(CLR_RT_Assembly::CreateInstance( header, assm ));

    if(szName != NULL)
    {
        CLR_RT_UnicodeHelper::ConvertToUTF8( szName, strPath );

        assm->m_strPath = new std::string( strPath );
    }

    TINYCLR_NOCLEANUP();
}
#endif

bool CLR_RT_Assembly::Resolve_AssemblyRef( bool fOutput )
{
    NATIVE_PROFILE_CLR_CORE();
    bool fGot = true;
    int  i;

    ITERATE_THROUGH_RECORDS(this,i,AssemblyRef,ASSEMBLYREF)
    {
        LPCSTR szName = GetString( src->name );

        if(dst->m_target == NULL)
        {
            bool fExact = true;

            //
            // Exact matching if this is a patch and the reference is toward the patched assembly.
            //
            if(m_header->flags & CLR_RECORD_ASSEMBLY::c_Flags_Patch)
            {
                if(!strcmp( szName, m_szName ))
                {
                    fExact = true;
                }
            }

            CLR_RT_Assembly* target = g_CLR_RT_TypeSystem.FindAssembly( szName, &src->version, fExact );

            if(target == NULL || (target->m_flags & CLR_RT_Assembly::c_Resolved) == 0)
            {
#if !defined(BUILD_RTM)
                if(fOutput)
                {
                    CLR_Debug::Printf( "Assembly: %s (%d.%d.%d.%d)", m_szName, m_header->version.iMajorVersion, m_header->version.iMinorVersion, m_header->version.iBuildNumber, m_header->version.iRevisionNumber );

                    CLR_Debug::Printf( " needs assembly '%s' (%d.%d.%d.%d)\r\n", szName, src->version.iMajorVersion, src->version.iMinorVersion, src->version.iBuildNumber, src->version.iRevisionNumber );
                }
#endif

                fGot = false;
            }
            else
            {
                dst->m_target = target;
            }
        }
    }

    return fGot;
}

void CLR_RT_Assembly::DestroyInstance()
{
    NATIVE_PROFILE_CLR_CORE();
    if(m_idx)
    {
        g_CLR_RT_TypeSystem.m_assemblies[ m_idx-1 ] = NULL;
    }

#if defined(WIN32)
    if(this->m_strPath != NULL)
    {
        delete this->m_strPath;
        this->m_strPath = NULL;
    }
#endif

    //--//

    g_CLR_RT_EventCache.Append_Node( this );
}

//--//
HRESULT CLR_RT_Assembly::Resolve_TypeRef()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    int i;

    ITERATE_THROUGH_RECORDS(this,i,TypeRef,TYPEREF)
    {
        if(src->scope & 0x8000) // Flag for TypeRef
        {
            CLR_RT_TypeDef_Instance inst;

            if(inst.InitializeFromIndex( m_pCrossReference_TypeRef[ src->scope & 0x7FFF ].m_target ) == false)
            {
#if !defined(BUILD_RTM)
                CLR_Debug::Printf( "Resolve: unknown scope: %08x\r\n", src->scope );
#endif
                TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
            }

            LPCSTR szName = GetString( src->name );
            if(inst.m_assm->FindTypeDef( szName, inst.Type(), dst->m_target ) == false)
            {
#if !defined(BUILD_RTM)
                CLR_Debug::Printf( "Resolve: unknown type: %s\r\n", szName );
#endif

                TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
            }
        }
        else
        {
            CLR_RT_Assembly* assm = m_pCrossReference_AssemblyRef[ src->scope ].m_target;
            if(assm == NULL)
            {
                TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
            }

            LPCSTR szNameSpace = GetString( src->nameSpace );
            LPCSTR szName      = GetString( src->name      );
            if(assm->FindTypeDef( szName, szNameSpace, dst->m_target ) == false)
            {
#if !defined(BUILD_RTM)
                CLR_Debug::Printf( "Resolve: unknown type: %s.%s\r\n", szNameSpace, szName );
#endif

                TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
            }
        }
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_Assembly::Resolve_FieldRef()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    int i;

    ITERATE_THROUGH_RECORDS(this,i,FieldRef,FIELDREF)
    {
        CLR_RT_TypeDef_Instance inst;

        if(inst.InitializeFromIndex( m_pCrossReference_TypeRef[ src->container ].m_target ) == false)
        {
#if !defined(BUILD_RTM)
            CLR_Debug::Printf( "Resolve Field: unknown scope: %08x\r\n", src->container );
#endif

            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }

        LPCSTR szName = GetString( src->name );

        if(inst.m_assm->FindFieldDef( inst.m_target, szName, this, src->sig, dst->m_target ) == false)
        {
#if !defined(BUILD_RTM)
            CLR_Debug::Printf( "Resolve: unknown field: %s\r\n", szName );
#endif

            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_Assembly::Resolve_MethodRef()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    int i;

    ITERATE_THROUGH_RECORDS(this,i,MethodRef,METHODREF)
    {
        CLR_RT_TypeDef_Instance inst;

        if(inst.InitializeFromIndex( m_pCrossReference_TypeRef[ src->container ].m_target ) == false)
        {
#if !defined(BUILD_RTM)
            CLR_Debug::Printf( "Resolve Field: unknown scope: %08x\r\n", src->container );
#endif

            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }

        LPCSTR name = GetString( src->name );
        bool   fGot = false;

        while(TINYCLR_INDEX_IS_VALID(inst))
        {
            if(inst.m_assm->FindMethodDef( inst.m_target, name, this, src->sig, dst->m_target ))
            {
                fGot = true;
                break;
            }

            inst.SwitchToParent();
        }

        if(fGot == false)
        {
            inst.InitializeFromIndex( m_pCrossReference_TypeRef[ src->container ].m_target );

            const CLR_RECORD_TYPEDEF* qTD   = inst.m_target;
            CLR_RT_Assembly*          qASSM = inst.m_assm;

#if !defined(BUILD_RTM)
            CLR_Debug::Printf( "Resolve: unknown method: %s.%s.%s\r\n", qASSM->GetString( qTD->nameSpace ), qASSM->GetString( qTD->name ), name );
#endif

            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }
    }

    TINYCLR_NOCLEANUP();
}

void CLR_RT_Assembly::Resolve_Link()
{
    NATIVE_PROFILE_CLR_CORE();
    int iStaticFields = 0;
    int idxType;

    ITERATE_THROUGH_RECORDS(this,idxType,TypeDef,TYPEDEF)
    {
        int num;
        int i;

        //
        // Link static fields.
        //
        {
            CLR_RT_FieldDef_CrossReference* fd = &m_pCrossReference_FieldDef[ src->sFields_First ];

            num = src->sFields_Num;

            for(; num; num--, fd++)
            {
                fd->m_offset = iStaticFields++;
            }
        }

        //
        // Link instance fields.
        //
        {
            CLR_RT_TypeDef_Index    idx ; idx.Set( m_idx, idxType );
            CLR_RT_TypeDef_Instance inst; inst.InitializeFromIndex( idx );
            CLR_IDX                 tot = 0;

            do
            {
                if(inst.m_target->flags & CLR_RECORD_TYPEDEF::TD_HasFinalizer)
                {
                    dst->m_flags |= CLR_RT_TypeDef_CrossReference::TD_CR_HasFinalizer;
                }

#if defined(TINYCLR_APPDOMAINS)
                if(inst.m_data == g_CLR_RT_WellKnownTypes.m_MarshalByRefObject.m_data)
                {
                    dst->m_flags |= CLR_RT_TypeDef_CrossReference::TD_CR_IsMarshalByRefObject;
                }
#endif

                tot += inst.m_target->iFields_Num;
            }
            while(inst.SwitchToParent());

            dst->m_totalFields = tot;

            //--//

            CLR_RT_FieldDef_CrossReference* fd = &m_pCrossReference_FieldDef[ src->iFields_First ];

            num = src->iFields_Num;
            i   = tot - num + CLR_RT_HeapBlock::HB_Object_Fields_Offset; // Take into account the offset from the beginning of the object.

            for(; num; num--, i++, fd++)
            {
                fd->m_offset = i;
            }
        }

        //
        // Link methods.
        //
        {
            CLR_RT_MethodDef_CrossReference* md = &m_pCrossReference_MethodDef[ src->methods_First ];

            int num = src->vMethods_Num + src->iMethods_Num + src->sMethods_Num;

            for(; num; num--, md++)
            {
                md->m_data = idxType;
            }
        }
    }
}

//--//

#if defined(TINYCLR_APPDOMAINS)

HRESULT CLR_RT_AppDomain::CreateInstance( LPCSTR szName, CLR_RT_AppDomain*& appDomain )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();
    CLR_RT_HeapBlock name; name.SetObjectReference( NULL );
    CLR_RT_ProtectFromGC gc( name );

    if(!szName || szName[ 0 ] == '\0') TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);

    //Check to see that the name does not already exist.
    TINYCLR_FOREACH_NODE(CLR_RT_AppDomain,appDomain,g_CLR_RT_ExecutionEngine.m_appDomains)
    {
        if(!strcmp( appDomain->m_strName->StringText(), szName )) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }
    TINYCLR_FOREACH_NODE_END();

    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_String::CreateInstance( name, szName ));

    appDomain = EVENTCACHE_EXTRACT_NODE(g_CLR_RT_EventCache,CLR_RT_AppDomain,DATATYPE_APPDOMAIN_HEAD); CHECK_ALLOCATION(appDomain);

    appDomain->AppDomain_Initialize();

    appDomain->m_strName = name.DereferenceString();

    g_CLR_RT_ExecutionEngine.m_appDomains.LinkAtBack( appDomain );

    TINYCLR_NOCLEANUP();
}

void CLR_RT_AppDomain::RecoverFromGC()
{
    NATIVE_PROFILE_CLR_CORE();
    CheckAll();

    /*
        AppDomains can be zombied and stay around forever.  It is worth looking into cleaning up dead AppDomains, but this
        needs to be done with great care.  First, enumerations of AppDomains must not be allowed.  Second, the AppDomain must really be dead.
        This is much harder to ensure.  Everything that is needed to be checked for explicit AD unloading (stack frames, finalizable objects)
        must be checked, but also other things that can cause the AppDomain to be alive, like timers, manaaged drivers, transparent proxies, etc..
    */

    if(m_state == CLR_RT_AppDomain::AppDomainState_Unloaded)
    {
        //We could actually clean up here.  Since the AppDomain is now officially loaded,
        //we can remove all object_to_event_dst references, and null out the
        //pointers in the managed AppDomain class, provided that calling any method on the
        //AppDomain will result in a AppDomainDisposed exception.  However, it's probably
        //not worth the effort, the majority of the resources have been already cleaned
        //up, from AppDomain_Uninitialize

        if(IsReadyForRelease())
        {
            DestroyInstance();
        }
    }
}

void CLR_RT_AppDomain::AppDomain_Initialize()
{
    NATIVE_PROFILE_CLR_CORE();
    Initialize();

    m_appDomainAssemblies.DblLinkedList_Initialize();

    m_state                       = CLR_RT_AppDomain::AppDomainState_Loaded;
    m_id                          = g_CLR_RT_ExecutionEngine.m_appDomainIdNext++;
    m_globalLock                  = NULL;
    m_strName                     = NULL;
    m_outOfMemoryException        = NULL;
    m_appDomainAssemblyLastAccess = NULL;
}

void CLR_RT_AppDomain::AppDomain_Uninitialize()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_FOREACH_NODE(CLR_RT_AppDomainAssembly,appDomainAssembly,m_appDomainAssemblies)
    {
        appDomainAssembly->DestroyInstance();
    }
    TINYCLR_FOREACH_NODE_END();
}

void CLR_RT_AppDomain::DestroyInstance()
{
    NATIVE_PROFILE_CLR_CORE();
    AppDomain_Uninitialize();

    Unlink();

    g_CLR_RT_EventCache.Append_Node( this );
}

HRESULT CLR_RT_AppDomain::LoadAssembly( CLR_RT_Assembly* assm )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_AppDomain*         appDomainSav      = g_CLR_RT_ExecutionEngine.SetCurrentAppDomain( this );
    CLR_RT_AppDomainAssembly* appDomainAssembly = NULL;
    int i;

    FAULT_ON_NULL( assm );

    //check to make sure the assembly is not already loaded
    if(FindAppDomainAssembly( assm ) != NULL) TINYCLR_SET_AND_LEAVE(S_OK);

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
    g_CLR_RT_ExecutionEngine.Breakpoint_Assemblies_Loaded();
#endif

    //Next, make sure that all dependent assemblies are loaded.
    {
        ITERATE_THROUGH_RECORDS(assm,i,AssemblyRef,ASSEMBLYREF)
        {
            TINYCLR_CHECK_HRESULT(LoadAssembly( dst->m_target ));
        }
    }

    TINYCLR_CHECK_HRESULT(CLR_RT_AppDomainAssembly::CreateInstance( this, assm, appDomainAssembly ));

    if(m_outOfMemoryException == NULL)
    {
        //Allocate an out of memory exception.  We should never get into a case where an out of memory exception
        //cannot be thrown.
        CLR_RT_HeapBlock exception;

        _ASSERTE(!strcmp( assm->m_szName, "mscorlib" )); //always the first assembly to be loaded

        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex( exception, g_CLR_RT_WellKnownTypes.m_OutOfMemoryException ));

        m_outOfMemoryException = exception.Dereference();
    }

    TINYCLR_CLEANUP();

    if(FAILED(hr))
    {
        if(appDomainAssembly)
        {
            appDomainAssembly->DestroyInstance();
        }
    }

    (void)g_CLR_RT_ExecutionEngine.SetCurrentAppDomain( appDomainSav );

    TINYCLR_CLEANUP_END();
}


HRESULT CLR_RT_AppDomain::GetManagedObject( CLR_RT_HeapBlock& res )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_AppDomain* appDomainSav = g_CLR_RT_ExecutionEngine.SetCurrentAppDomain( this );

    res.SetObjectReference( NULL );

    //Check if a managed object is already present, and use it
    TINYCLR_FOREACH_NODE(CLR_RT_ObjectToEvent_Source,ref,m_references)
    {
        CLR_RT_HeapBlock* obj = ref->m_objectPtr;

        _ASSERTE(FIMPLIES(obj, obj->DataType() == DATATYPE_CLASS || obj->DataType() == DATATYPE_VALUETYPE));

        if(obj && obj->ObjectCls().m_data == g_CLR_RT_WellKnownTypes.m_AppDomain.m_data)
        {
            //managed appDomain is found.  Use it.
            res.SetObjectReference( ref->m_objectPtr );

            TINYCLR_SET_AND_LEAVE(S_OK);
        }
    }
    TINYCLR_FOREACH_NODE_END();

    {
        //Create the managed AppDomain in the destination AppDomain
        CLR_RT_HeapBlock*    pRes;
        CLR_RT_ProtectFromGC gc( res );

        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex( res, g_CLR_RT_WellKnownTypes.m_AppDomain ));

        pRes = res.Dereference();

        TINYCLR_CHECK_HRESULT(CLR_RT_ObjectToEvent_Source::CreateInstance( this, *pRes, pRes[ Library_corlib_native_System_AppDomain::FIELD__m_appDomain ] ));

        pRes[ Library_corlib_native_System_AppDomain::FIELD__m_friendlyName ].SetObjectReference( m_strName );
    }

    TINYCLR_CLEANUP();

    g_CLR_RT_ExecutionEngine.SetCurrentAppDomain( appDomainSav );

    TINYCLR_CLEANUP_END();
}

CLR_RT_AppDomainAssembly* CLR_RT_AppDomain::FindAppDomainAssembly( CLR_RT_Assembly* assm )
{
    NATIVE_PROFILE_CLR_CORE();
    if(m_appDomainAssemblyLastAccess != NULL && m_appDomainAssemblyLastAccess->m_assembly == assm)
    {
        return m_appDomainAssemblyLastAccess;
    }

    TINYCLR_FOREACH_NODE(CLR_RT_AppDomainAssembly,appDomainAssembly,m_appDomainAssemblies)
    {
        if(appDomainAssembly->m_assembly == assm)
        {
            m_appDomainAssemblyLastAccess = appDomainAssembly;

            return appDomainAssembly;
        }
    }
    TINYCLR_FOREACH_NODE_END();

    return NULL;
}

void CLR_RT_AppDomain::Relocate()
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_globalLock           );
    CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_strName              );
    CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_outOfMemoryException );
}

HRESULT CLR_RT_AppDomain::VerifyTypeIsLoaded( const CLR_RT_TypeDef_Index& idx )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_TypeDef_Instance inst;

    if(!inst.InitializeFromIndex( idx         )) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER           );
    if(!FindAppDomainAssembly   ( inst.m_assm )) TINYCLR_SET_AND_LEAVE(CLR_E_APPDOMAIN_MARSHAL_EXCEPTION);

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_AppDomain::MarshalObject( CLR_RT_HeapBlock& src, CLR_RT_HeapBlock& dst, CLR_RT_AppDomain* appDomainSrc )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    //This function marshals an object from appDomainSrc to 'this' AppDomain
    //If appDomainSrc == NULL, this uses the current AppDomain

    CLR_RT_AppDomain* appDomainDst = this;
    CLR_RT_HeapBlock* proxySrc     = NULL;
    CLR_RT_HeapBlock* mbroSrc      = NULL;
    bool fSimpleAssign             = false;
    CLR_RT_TypeDef_Index idxVerify = g_CLR_RT_WellKnownTypes.m_Object;
    CLR_DataType dtSrc             = src.DataType();
    CLR_RT_AppDomain* appDomainSav = g_CLR_RT_ExecutionEngine.GetCurrentAppDomain();

    if(!appDomainSrc)
    {
        appDomainSrc = appDomainSav;
    }

    //
    //DATATYPE_LAST_PRIMITIVE_TO_MARSHAL note
    //We should think about allowing STRINGS to be shared across AD boundaries
    //Strings are read-only and it is safe to do this with some small restrictions
    //First, as with the Assembly unloading, some strings can actually point within an
    //assembly.  If we allow assemblies to be unloaded (and why shouldn't we, if they are
    //not in any AppDomain, than we need to deal with this case).  If we just
    //get copy the string on constructor, then we do not need to marshal strings
    //across AD boundaries.
    //
    fSimpleAssign = (appDomainSrc == appDomainDst);
    fSimpleAssign = fSimpleAssign || (dtSrc <= DATATYPE_LAST_PRIMITIVE_TO_MARSHAL);
    fSimpleAssign = fSimpleAssign || (dtSrc == DATATYPE_OBJECT && src.Dereference() == NULL);

#if !defined(TINYCLR_NO_ASSEMBLY_STRINGS)
    fSimpleAssign = fSimpleAssign || (dtSrc == DATATYPE_STRING && !src.StringAssembly());
#endif

    if(!fSimpleAssign)
    {
        if(dtSrc == DATATYPE_OBJECT)
        {
            CLR_RT_HeapBlock* ptr = src.Dereference();

            switch(ptr->DataType())
            {
                case DATATYPE_TRANSPARENT_PROXY:
                    {
                        proxySrc = ptr;

                        TINYCLR_CHECK_HRESULT(proxySrc->TransparentProxyValidate());

                        idxVerify = proxySrc->TransparentProxyDereference()->ObjectCls();

                        if(proxySrc->TransparentProxyAppDomain() != appDomainDst)
                        {
                            //marshalling a transparent proxy object to a third AppDomain
                            //This makes the marshaling a simple assign of the DATATYPE_TRANSPARENT_PROXY heapblock
                            fSimpleAssign = true;
                        }
                    }
                    break;
                case DATATYPE_CLASS:
                    {
                        CLR_RT_TypeDef_Instance inst;

                        if(inst.InitializeFromIndex( ptr->ObjectCls() ))
                        {
                            if((inst.CrossReference().m_flags & CLR_RT_TypeDef_CrossReference::TD_CR_IsMarshalByRefObject) != 0)
                            {
                                idxVerify = inst;

                                mbroSrc = ptr;
                            }
                        }
                    }
                    break;
            }
        }
    }

    TINYCLR_CHECK_HRESULT(appDomainDst->VerifyTypeIsLoaded( idxVerify ));

    if(fSimpleAssign)
    {
        dst.Assign( src );
    }
    else if(proxySrc != NULL)
    {
        //src is OBJECT->TRANSPARENT_PROXY->CLASS, and we are marshalling into 'this' appDomain
        //dst is OBJECT->CLASS
        _ASSERTE(proxySrc->TransparentProxyAppDomain() == appDomainDst);

        dst.SetObjectReference( proxySrc->TransparentProxyDereference() );
    }
    else if(mbroSrc != NULL)
    {
        //src is a MarshalByRefObject that we are marshalling outside of its AppDomain
        //src is OBJECT->CLASS
        //dst is OBJECT->TRANSPARENT_PROXY->CLASS

        (void)g_CLR_RT_ExecutionEngine.SetCurrentAppDomain( appDomainDst );
        CLR_RT_HeapBlock* proxyDst = g_CLR_RT_ExecutionEngine.ExtractHeapBlocksForObjects( DATATYPE_TRANSPARENT_PROXY, 0, 1 ); CHECK_ALLOCATION(proxyDst);

        proxyDst->SetTransparentProxyReference( appDomainSrc, mbroSrc );
        dst.SetObjectReference( proxyDst );
    }
    else
    {
        CLR_RT_HeapBlock blk; blk.SetObjectReference( NULL );
        CLR_RT_ProtectFromGC  gc( blk );
        bool fNoCompaction = CLR_EE_DBG_IS(NoCompaction);

        //Need to prevent compaction between serialization/deserialization.
        //Doesn't seem that currently compaction can actually occur during this time,
        //but just to be safe, we should prevent it.

        CLR_EE_DBG_SET(NoCompaction);
        TINYCLR_CHECK_HRESULT(CLR_RT_BinaryFormatter::Serialize( blk, src, NULL, CLR_RT_BinaryFormatter::c_Flags_Marshal ));

        (void)g_CLR_RT_ExecutionEngine.SetCurrentAppDomain( appDomainDst );
        hr = CLR_RT_BinaryFormatter::Deserialize( dst, blk, NULL, NULL, CLR_RT_BinaryFormatter::c_Flags_Marshal );

        CLR_EE_DBG_RESTORE(NoCompaction,fNoCompaction);
    }

    TINYCLR_CLEANUP();

    (void)g_CLR_RT_ExecutionEngine.SetCurrentAppDomain( appDomainSav );

    TINYCLR_CLEANUP_END();
}

HRESULT CLR_RT_AppDomain::MarshalParameters( CLR_RT_HeapBlock* callerArgs, CLR_RT_HeapBlock* calleeArgs, int count, bool fOnReturn, CLR_RT_AppDomain* appDomainSrc )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock* src = fOnReturn ? calleeArgs : callerArgs;
    CLR_RT_HeapBlock* dst = fOnReturn ? callerArgs : calleeArgs;

    while(count-- > 0)
    {
        CLR_DataType dtSrc = src->DataType();
        CLR_DataType dtDst = dst->DataType();

        if(dtSrc == DATATYPE_BYREF || dtSrc == DATATYPE_ARRAY_BYREF)
        {
            CLR_RT_HeapBlock  srcObj;
            CLR_RT_HeapBlock  dstObj;

            if(fOnReturn)
            {
                srcObj.Assign( *src->Dereference() );

                TINYCLR_CHECK_HRESULT(MarshalObject( srcObj, dstObj, appDomainSrc ));

                //Move the marshaled object back into dst heapblock
                if(dtDst == DATATYPE_BYREF)
                {
                    dst->Dereference()->Assign( dstObj );
                }
                else
                {
                    dstObj.StoreToReference( *dst, 0 );
                }
            }
            else //!fOnReturn
            {
                CLR_RT_HeapBlock* dstPtr = NULL;

                if(dtSrc == DATATYPE_BYREF)
                {
                    srcObj.Assign( *src->Dereference() );
                }
                else
                {
                    srcObj.LoadFromReference( *src );
                }

                TINYCLR_CHECK_HRESULT(MarshalObject( srcObj, dstObj, appDomainSrc ));

                //Need to copy DstObj onto the heap.
                dstPtr = g_CLR_RT_ExecutionEngine.ExtractHeapBlocksForObjects( DATATYPE_OBJECT, 0, 1 ); FAULT_ON_NULL(dstPtr);

                _ASSERTE(c_CLR_RT_DataTypeLookup[ dstObj.DataType() ].m_sizeInBytes != CLR_RT_DataTypeLookup::c_NA);

                dstPtr->Assign( dstObj );

                //Turn the OBJECT back into a BYREF
                dst->SetReference( *dstPtr );
            }
        }
        else //Not BYREF
        {
            if(!fOnReturn)
            {
                TINYCLR_CHECK_HRESULT(MarshalObject( *src, *dst, appDomainSrc ));
            }
        }

        src++;
        dst++;
    }

    TINYCLR_NOCLEANUP();
}


HRESULT CLR_RT_AppDomain::GetAssemblies( CLR_RT_HeapBlock& ref )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    int               count  = 0;
    CLR_RT_HeapBlock* pArray = NULL;
    
    for(int pass=0; pass<2; pass++)
    {
        TINYCLR_FOREACH_ASSEMBLY_IN_APPDOMAIN( this )
        {
            if(pASSM->m_header->flags & CLR_RECORD_ASSEMBLY::c_Flags_Patch) continue;

            if(pass == 0)
            {
                count++;
            }
            else
            {
                CLR_RT_HeapBlock* hbObj;
                CLR_RT_Assembly_Index idx; idx.Set( pASSM->m_idx );

                TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex(*pArray, g_CLR_RT_WellKnownTypes.m_Assembly));
                hbObj = pArray->Dereference();
                
                hbObj->SetReflection( idx ); 

                pArray++;
            }
        }
        TINYCLR_FOREACH_ASSEMBLY_IN_APPDOMAIN_END();

        if(pass == 0)
        {
            TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance( ref, count, g_CLR_RT_WellKnownTypes.m_Assembly ));

            pArray = (CLR_RT_HeapBlock*)ref.DereferenceArray()->GetFirstElement();
        }
    }

    TINYCLR_NOCLEANUP();
}

bool CLR_RT_AppDomain::IsLoaded()
{
    NATIVE_PROFILE_CLR_CORE();
    return m_state == CLR_RT_AppDomain::AppDomainState_Loaded;
}

//--//

HRESULT CLR_RT_AppDomainAssembly::CreateInstance( CLR_RT_AppDomain* appDomain, CLR_RT_Assembly* assm, CLR_RT_AppDomainAssembly*& appDomainAssembly )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    int size = CONVERTFROMSIZETOHEAPBLOCKS(sizeof(CLR_RT_AppDomainAssembly)) + assm->m_iStaticFields;

    appDomainAssembly = EVENTCACHE_EXTRACT_NODE_AS_BLOCKS(g_CLR_RT_EventCache,CLR_RT_AppDomainAssembly,DATATYPE_APPDOMAIN_ASSEMBLY,CLR_RT_HeapBlock::HB_InitializeToZero,size); CHECK_ALLOCATION(appDomainAssembly);

    TINYCLR_CHECK_HRESULT(appDomainAssembly->AppDomainAssembly_Initialize( appDomain, assm ));

    TINYCLR_CLEANUP();

    if(FAILED(hr))
    {
        if(appDomainAssembly)
        {
            appDomainAssembly->DestroyInstance();
        }
    }

    TINYCLR_CLEANUP_END();
}

HRESULT CLR_RT_AppDomainAssembly::AppDomainAssembly_Initialize( CLR_RT_AppDomain* appDomain, CLR_RT_Assembly* assm )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_AppDomain* appDomainSav = g_CLR_RT_ExecutionEngine.SetCurrentAppDomain( appDomain );

    m_appDomain     = appDomain;
    m_assembly      = assm;
    m_flags         = 0;
    m_pStaticFields = (CLR_RT_HeapBlock*)&this[ 1 ];

    /*
        The AppDomainAssembly gets linked before it is actually initialized, for two reasons.  First, it needs to be
        attached to the AppDomain in case a GC runs during the allocation of fields.  Second, this assembly needs to be
        loaded in the AppDomain when the static constructors are run.
    */

    appDomain->m_appDomainAssemblies.LinkAtBack( this );

    TINYCLR_CHECK_HRESULT(assm->Resolve_AllocateStaticFields( m_pStaticFields ));

    if(!CLR_EE_DBG_IS_MASK(State_Initialize,State_Mask))
    {
        //Only in the non-boot case should we do this. Otherwise, debug events can occur out of order (thread creation of the
        //static constructor before thread creation of the main thread.
        g_CLR_RT_ExecutionEngine.SpawnStaticConstructor( g_CLR_RT_ExecutionEngine.m_cctorThread );
    }

    TINYCLR_CLEANUP();

    if(FAILED(hr))
    {
        Unlink();
    }

    g_CLR_RT_ExecutionEngine.SetCurrentAppDomain( appDomainSav );

    TINYCLR_CLEANUP_END();
}

void CLR_RT_AppDomainAssembly::DestroyInstance()
{
    NATIVE_PROFILE_CLR_CORE();
    Unlink();

    g_CLR_RT_EventCache.Append_Node( this );
}

void CLR_RT_AppDomainAssembly::Relocate()
{
    NATIVE_PROFILE_CLR_CORE();
     CLR_RT_GarbageCollector::Heap_Relocate( m_pStaticFields, m_assembly->m_iStaticFields );
}

#endif //TINYCLR_APPDOMAINS

//--//


struct TypeIndexLookup
{
    LPCSTR                nameSpace;
    LPCSTR                name;
    CLR_RT_TypeDef_Index* ptr;
};

static const TypeIndexLookup c_TypeIndexLookup[] =
{
#define TIL(ns,nm,fld) { ns, nm, &g_CLR_RT_WellKnownTypes.fld }
    TIL( "System"                  , "Boolean"                       , m_Boolean                                            ),
    TIL( "System"                  , "Char"                          , m_Char                                               ),
    TIL( "System"                  , "SByte"                         , m_Int8                                               ),
    TIL( "System"                  , "Byte"                          , m_UInt8                                              ),
    TIL( "System"                  , "Int16"                         , m_Int16                                              ),
    TIL( "System"                  , "UInt16"                        , m_UInt16                                             ),
    TIL( "System"                  , "Int32"                         , m_Int32                                              ),
    TIL( "System"                  , "UInt32"                        , m_UInt32                                             ),
    TIL( "System"                  , "Int64"                         , m_Int64                                              ),
    TIL( "System"                  , "UInt64"                        , m_UInt64                                             ),
    TIL( "System"                  , "Single"                        , m_Single                                             ),
    TIL( "System"                  , "Double"                        , m_Double                                             ),
    TIL( "System"                  , "DateTime"                      , m_DateTime                                           ),
    TIL( "System"                  , "TimeSpan"                      , m_TimeSpan                                           ),
    TIL( "System"                  , "String"                        , m_String                                             ),

    TIL( "System"                  , "Void"                          , m_Void                                               ),
    TIL( "System"                  , "Object"                        , m_Object                                             ),
    TIL( "System"                  , "ValueType"                     , m_ValueType                                          ),
    TIL( "System"                  , "Enum"                          , m_Enum                                               ),

    TIL( "System"                  , "AppDomainUnloadedException"    , m_AppDomainUnloadedException                         ),
    TIL( "System"                  , "ArgumentNullException"         , m_ArgumentNullException                              ),
    TIL( "System"                  , "ArgumentException"             , m_ArgumentException                                  ),
    TIL( "System"                  , "ArgumentOutOfRangeException"   , m_ArgumentOutOfRangeException                        ),
    TIL( "System"                  , "Exception"                     , m_Exception                                          ),
    TIL( "System"                  , "IndexOutOfRangeException"      , m_IndexOutOfRangeException                           ),
    TIL( "System"                  , "InvalidCastException"          , m_InvalidCastException                               ),
    TIL( "System"                  , "InvalidOperationException"     , m_InvalidOperationException                          ),
    TIL( "System"                  , "NotSupportedException"         , m_NotSupportedException                              ),
    TIL( "System"                  , "NotImplementedException"       , m_NotImplementedException                            ),
    TIL( "System"                  , "NullReferenceException"        , m_NullReferenceException                             ),
    TIL( "System"                  , "OutOfMemoryException"          , m_OutOfMemoryException                               ),
    TIL( "System"                  , "ObjectDisposedException"       , m_ObjectDisposedException                            ),
    TIL( "System.IO"               , "IOException"                   , m_IOException                                        ),
    TIL( "System.Threading"        , "ThreadAbortException"          , m_ThreadAbortException                               ),
    TIL( "Microsoft.SPOT"          , "ConstraintException"           , m_ConstraintException                                ),
    TIL( "Microsoft.SPOT"          , "UnknownTypeException"          , m_UnknownTypeException                               ),

    TIL( "System"                  , "Delegate"                      , m_Delegate                                           ),
    TIL( "System"                  , "MulticastDelegate"             , m_MulticastDelegate                                  ),

    TIL( "System"                  , "Array"                         , m_Array                                              ),
    TIL( "System.Collections"      , "ArrayList"                     , m_ArrayList                                          ),
    TIL( "System"                  , "ICloneable"                    , m_ICloneable                                         ),
    TIL( "System.Collections"      , "IList"                         , m_IList                                              ),

    TIL( "System.Reflection"       , "Assembly"                      , m_Assembly                                           ),
    TIL( "System"                  , "Type"                          , m_TypeStatic                                         ),
    TIL( "System"                  , "RuntimeType"                   , m_Type                                               ),
    TIL( "System.Reflection"       , "RuntimeConstructorInfo"        , m_ConstructorInfo                                    ),
    TIL( "System.Reflection"       , "RuntimeMethodInfo"             , m_MethodInfo                                         ),
    TIL( "System.Reflection"       , "RuntimeFieldInfo"              , m_FieldInfo                                          ),

    TIL( "System"                  , "WeakReference"                 , m_WeakReference                                      ),
    TIL( "Microsoft.SPOT"          , "ExtendedWeakReference"         , m_ExtendedWeakReference                              ),

    TIL( "Microsoft.SPOT"          , "SerializationHintsAttribute"   , m_SerializationHintsAttribute                        ),

    TIL( "Microsoft.SPOT"          , "ExtendedTimeZone"              , m_ExtendedTimeZone                                   ),

    TIL( "Microsoft.SPOT"          , "Bitmap"                        , m_Bitmap                                             ),
    TIL( "Microsoft.SPOT"          , "Font"                          , m_Font                                               ),

    TIL( "Microsoft.SPOT.Touch"    , "TouchEvent"                    , m_TouchEvent                                         ),
    TIL( "Microsoft.SPOT.Touch"    , "TouchInput"                    , m_TouchInput                                         ),

    TIL( "Microsoft.SPOT.Messaging", "Message"                       , m_Message                                            ),

    TIL( "Microsoft.SPOT.Hardware" , "ScreenMetrics"                 , m_ScreenMetrics                                      ),

    TIL( "Microsoft.SPOT.Hardware" , "WatchdogException"             , m_WatchdogException                                  ),

    TIL( "Microsoft.SPOT.Hardware" , "I2CDevice"                     , m_I2CDevice                                          ),
    TIL( NULL                      , "I2CReadTransaction"            , m_I2CDevice__I2CReadTransaction                      ),
    TIL( NULL                      , "I2CWriteTransaction"           , m_I2CDevice__I2CWriteTransaction                     ),
    
    TIL( "Microsoft.SPOT.Hardware.UsbClient", "Configuration"        , m_UsbClientConfiguration                             ),
    TIL( NULL                      , "Descriptor"                    , m_UsbClientConfiguration__Descriptor                 ),
    TIL( NULL                      , "DeviceDescriptor"              , m_UsbClientConfiguration__DeviceDescriptor           ),
    TIL( NULL                      , "ClassDescriptor"               , m_UsbClientConfiguration__ClassDescriptor            ),
    TIL( NULL                      , "Endpoint"                      , m_UsbClientConfiguration__Endpoint                   ),
    TIL( NULL                      , "UsbInterface"                  , m_UsbClientConfiguration__UsbInterface               ),
    TIL( NULL                      , "ConfigurationDescriptor"       , m_UsbClientConfiguration__ConfigurationDescriptor    ),
    TIL( NULL                      , "StringDescriptor"              , m_UsbClientConfiguration__StringDescriptor           ),
    TIL( NULL                      , "GenericDescriptor"             , m_UsbClientConfiguration__GenericDescriptor          ),

    TIL( "Microsoft.SPOT.Net.NetworkInformation", "NetworkInterface" , m_NetworkInterface                                   ),
    TIL( "Microsoft.SPOT.Net.NetworkInformation", "Wireless80211"    , m_Wireless80211                                      ),

    TIL( "Microsoft.SPOT.Time"     , "TimeServiceSettings"           , m_TimeServiceSettings                                ),
    TIL( "Microsoft.SPOT.Time"     , "TimeServiceStatus"             , m_TimeServiceStatus                                  ),

#if defined(TINYCLR_APPDOMAINS)
    TIL( "System"                  , "AppDomain"                     , m_AppDomain                                          ),
    TIL( "System"                  , "MarshalByRefObject"            , m_MarshalByRefObject                                 ),
#endif

    TIL( "System.Threading"        , "Thread"                        , m_Thread                                             ),
    TIL( "System.Resources"        , "ResourceManager"               , m_ResourceManager                                    ),

    TIL( "System.Net.Sockets"      , "SocketException"               , m_SocketException                                    ),

    TIL( "Microsoft.SPOT.IO"       , "NativeFileInfo"                , m_NativeFileInfo                                     ),
    TIL( "Microsoft.SPOT.IO"       , "VolumeInfo"                    , m_VolumeInfo                                         ),

    TIL( "System.Xml"              , "XmlNameTable_Entry"            , m_XmlNameTable_Entry                                 ),
    TIL( "System.Xml"              , "XmlReader_XmlNode"             , m_XmlReader_XmlNode                                  ),
    TIL( "System.Xml"              , "XmlReader_XmlAttribute"        , m_XmlReader_XmlAttribute                             ),
    TIL( "System.Xml"              , "XmlReader_NamespaceEntry"      , m_XmlReader_NamespaceEntry                           ),

    TIL( "System.Security.Cryptography", "CryptoKey"                 , m_CryptoKey                                          ),
    TIL( "Microsoft.SPOT.Cryptoki"     , "CryptokiObject"            , m_CryptokiObject                                     ),
    TIL( "Microsoft.SPOT.Cryptoki"     , "Session"                   , m_CryptokiSession                                    ),
    TIL( "Microsoft.SPOT.Cryptoki"     , "Slot"                      , m_CryptokiSlot                                       ),
    TIL( "Microsoft.SPOT.Cryptoki"     , "MechanismType"             , m_CryptokiMechanismType                              ),
    TIL( "System.Security.Cryptography", "CryptographicException"    , m_CryptoException                                    ),
    TIL( "Microsoft.SPOT.Cryptoki"     , "CryptokiCertificate"       , m_CryptokiCertificate                                ),

#undef TIL
};


//--//


struct MethodIndexLookup
{
    LPCSTR                  name;
    CLR_RT_TypeDef_Index*   type;
    CLR_RT_MethodDef_Index* method;
};

static const MethodIndexLookup c_MethodIndexLookup[] =
{
    #define MIL(nm,type,method) { nm, &g_CLR_RT_WellKnownTypes.type, &g_CLR_RT_WellKnownMethods.method }

    MIL( "GetObjectFromId"     , m_ResourceManager, m_ResourceManager_GetObjectFromId      ),
    MIL( "GetObjectChunkFromId", m_ResourceManager, m_ResourceManager_GetObjectChunkFromId ),

    #undef MIL
};

void CLR_RT_Assembly::Resolve_TypeDef()
{
    NATIVE_PROFILE_CLR_CORE();
    const TypeIndexLookup* tilOuterClass = NULL;
    const TypeIndexLookup* til           = c_TypeIndexLookup;

    for(size_t i=0; i<ARRAYSIZE(c_TypeIndexLookup); i++, til++)
    {
        CLR_RT_TypeDef_Index& dst = *til->ptr;

        if(TINYCLR_INDEX_IS_INVALID(dst))
        {
            if(til->nameSpace == NULL)
            {
                if(tilOuterClass)
                {
                    FindTypeDef( til->name, tilOuterClass->ptr->Type(), dst );
                }
            }
            else
            {
                FindTypeDef( til->name, til->nameSpace, dst );
            }
        }

        if(til->nameSpace != NULL)
        {
            tilOuterClass = til;
        }
    }
}

void CLR_RT_Assembly::Resolve_MethodDef()
{
    NATIVE_PROFILE_CLR_CORE();
    const CLR_RECORD_METHODDEF* md = GetMethodDef( 0 );

    for(int i=0; i<m_pTablesSize[ TBL_MethodDef ]; i++, md++)
    {
        const MethodIndexLookup* mil = c_MethodIndexLookup;

        CLR_RT_MethodDef_Index idx; idx.Set( m_idx, i );

        //Check for wellKnownMethods
        for(size_t i=0; i<ARRAYSIZE(c_MethodIndexLookup); i++, mil++)
        {
            CLR_RT_TypeDef_Index&   idxType   = *mil->type;
            CLR_RT_MethodDef_Index& idxMethod = *mil->method;

            if(TINYCLR_INDEX_IS_VALID(idxType) && TINYCLR_INDEX_IS_INVALID(idxMethod))
            {
                CLR_RT_TypeDef_Instance instType;

                _SIDE_ASSERTE(instType.InitializeFromIndex( idxType ));

                if(instType.m_assm == this)
                {
                    if(!strcmp( GetString(md->name), mil->name ))
                    {
                        idxMethod.m_data = idx.m_data;
                    }
                }
            }
        }

        if(md->flags & CLR_RECORD_METHODDEF::MD_EntryPoint)
        {
            g_CLR_RT_TypeSystem.m_entryPoint = idx;
        }
    }
}

HRESULT CLR_RT_Assembly::Resolve_AllocateStaticFields( CLR_RT_HeapBlock* pStaticFields )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    const CLR_RECORD_FIELDDEF* fd = GetFieldDef( 0 );

    for(int i=0; i<m_pTablesSize[ TBL_FieldDef ]; i++, fd++)
    {
        if(fd->flags & CLR_RECORD_FIELDDEF::FD_Static)
        {
            CLR_RT_FieldDef_CrossReference& res = m_pCrossReference_FieldDef[ i ];

            TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.InitializeReference( pStaticFields[ res.m_offset ], fd, this ));
        }
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_Assembly::PrepareForExecution()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    if((m_flags & CLR_RT_Assembly::c_PreparingForExecution) != 0)
    {
        //Circular dependency
        _ASSERTE(FALSE);

        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    if((m_flags & CLR_RT_Assembly::c_PreparedForExecution) == 0)
    {
        int i;

        m_flags |= CLR_RT_Assembly::c_PreparingForExecution;

        ITERATE_THROUGH_RECORDS(this,i,AssemblyRef,ASSEMBLYREF)
        {
            _ASSERTE(dst->m_target != NULL);

            if(dst->m_target != NULL)
            {
                TINYCLR_CHECK_HRESULT(dst->m_target->PrepareForExecution());
            }
        }

        if(m_header->patchEntryOffset != 0xFFFFFFFF)
        {
            CLR_PMETADATA ptr = GetResourceData( m_header->patchEntryOffset );

#if defined(WIN32)
            CLR_Debug::Printf( "Simulating jump into patch code...\r\n" );
#else
            ((void (*)())ptr)();
#endif
        }

#if defined(TINYCLR_APPDOMAINS)
        //Temporary solution.  All Assemblies get added to the current AppDomain
        //Which assemblies get loaded at boot, and when assemblies get added to AppDomain at runtime is
        //not yet determined/implemented

        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.GetCurrentAppDomain()->LoadAssembly( this ));
#endif
    }

    TINYCLR_CLEANUP();

    //Only try once.  If this fails, then what?
    m_flags |=  CLR_RT_Assembly::c_PreparedForExecution;
    m_flags &= ~CLR_RT_Assembly::c_PreparingForExecution;

    TINYCLR_CLEANUP_END();
}

//--//

CLR_UINT32 CLR_RT_Assembly::ComputeAssemblyHash()
{
    NATIVE_PROFILE_CLR_CORE();
    return m_header->ComputeAssemblyHash( m_szName, m_header->version );
}

CLR_UINT32 CLR_RT_Assembly::ComputeAssemblyHash( const CLR_RECORD_ASSEMBLYREF* ar )
{
    NATIVE_PROFILE_CLR_CORE();
    return m_header->ComputeAssemblyHash( GetString( ar->name ), ar->version );
}

//--//

bool CLR_RT_Assembly::FindTypeDef( LPCSTR name, LPCSTR nameSpace, CLR_RT_TypeDef_Index& idx )
{
    NATIVE_PROFILE_CLR_CORE();
    const CLR_RECORD_TYPEDEF* target  = GetTypeDef( 0 );
    int                       tblSize = m_pTablesSize[ TBL_TypeDef ];

    for(int i=0; i<tblSize; i++, target++)
    {
        if(target->enclosingType == CLR_EmptyIndex)
        {
            LPCSTR szNameSpace = GetString( target->nameSpace );
            LPCSTR szName      = GetString( target->name      );

            if(!strcmp( szName, name ) && !strcmp( szNameSpace, nameSpace ))
            {
                idx.Set( m_idx, i );

                return true;
            }
        }
    }

    idx.Clear();

    return false;
}

bool CLR_RT_Assembly::FindTypeDef( LPCSTR name, CLR_IDX scope, CLR_RT_TypeDef_Index& idx )
{
    NATIVE_PROFILE_CLR_CORE();
    const CLR_RECORD_TYPEDEF* target  = GetTypeDef( 0 );
    int                       tblSize = m_pTablesSize[ TBL_TypeDef ];

    for(int i=0; i<tblSize; i++, target++)
    {
        if(target->enclosingType == scope)
        {
            LPCSTR szName = GetString( target->name );

            if(!strcmp( szName, name ))
            {
                idx.Set( m_idx, i );

                return true;
            }
        }
    }

    idx.Clear();

    return false;
}

bool CLR_RT_Assembly::FindTypeDef( CLR_UINT32 hash, CLR_RT_TypeDef_Index& idx )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_TypeDef_CrossReference* p       = m_pCrossReference_TypeDef;
    CLR_UINT32                     tblSize = m_pTablesSize[ TBL_TypeDef ];
    CLR_UINT32                     i;

    for(i=0; i<tblSize; i++, p++)
    {
        if(p->m_hash == hash) break;
    }

    if(i != tblSize)
    {
        idx.Set( m_idx, i );

        return true;
    }
    else
    {
        idx.Clear();

        return false;
    }
}

//--//

static bool local_FindFieldDef( CLR_RT_Assembly* assm, CLR_UINT32 first, CLR_UINT32 num, LPCSTR szText, CLR_RT_Assembly* base, CLR_IDX sig, CLR_RT_FieldDef_Index& res )
{
    NATIVE_PROFILE_CLR_CORE();
    const CLR_RECORD_FIELDDEF* fd = assm->GetFieldDef( first );

    for(CLR_UINT32 i=0; i<num; i++, fd++)
    {
        LPCSTR fieldName = assm->GetString( fd->name );

        if(!strcmp( fieldName, szText ))
        {
            if(base)
            {
                CLR_RT_SignatureParser parserLeft ; parserLeft .Initialize_FieldDef( assm, fd                        );
                CLR_RT_SignatureParser parserRight; parserRight.Initialize_FieldDef( base, base->GetSignature( sig ) );

                if(CLR_RT_TypeSystem::MatchSignature( parserLeft, parserRight ) == false) continue;
            }

            res.Set( assm->m_idx, first + i );

            return true;
        }
    }

    res.Clear();

    return false;
}

bool CLR_RT_Assembly::FindFieldDef( const CLR_RECORD_TYPEDEF* td, LPCSTR name, CLR_RT_Assembly* base, CLR_IDX sig, CLR_RT_FieldDef_Index& idx )
{
    NATIVE_PROFILE_CLR_CORE();
    if(local_FindFieldDef( this, td->iFields_First, td->iFields_Num, name, base, sig, idx )) return true;
    if(local_FindFieldDef( this, td->sFields_First, td->sFields_Num, name, base, sig, idx )) return true;

    idx.Clear();

    return false;
}

bool CLR_RT_Assembly::FindMethodDef( const CLR_RECORD_TYPEDEF* td, LPCSTR name, CLR_RT_Assembly* base, CLR_SIG sig, CLR_RT_MethodDef_Index& idx )
{
    NATIVE_PROFILE_CLR_CORE();
    int                         i;
    int                         num = td->vMethods_Num + td->iMethods_Num + td->sMethods_Num;
    const CLR_RECORD_METHODDEF* md  = GetMethodDef( td->methods_First );

    for(i=0; i<num; i++, md++)
    {
        LPCSTR methodName = GetString( md->name );

        if(!strcmp( methodName, name ))
        {
            bool fMatch = true;

            if(CLR_SIG_INVALID != sig)
            {
                CLR_RT_SignatureParser parserLeft ; parserLeft .Initialize_MethodSignature( this, md                        );
                CLR_RT_SignatureParser parserRight; parserRight.Initialize_MethodSignature( base, base->GetSignature( sig ) );

                fMatch = CLR_RT_TypeSystem::MatchSignature( parserLeft, parserRight );
            }

            if(fMatch)
            {
                idx.Set( m_idx, i + td->methods_First );

                return true;
            }
        }
    }

    idx.Clear();

    return false;
}

////////////////////////////////////////////////////////////////////////////////////////////////////

bool CLR_RT_Assembly::FindMethodBoundaries( CLR_IDX i, CLR_OFFSET& start, CLR_OFFSET& end )
{
    NATIVE_PROFILE_CLR_CORE();
    const CLR_RECORD_METHODDEF* p = GetMethodDef( i );

    if(p->RVA == CLR_EmptyIndex) return false;

    start = p->RVA;

    while(true)
    {
        p++;
        i++;

        if(i == m_pTablesSize[ TBL_MethodDef ])
        {
            end = m_pTablesSize[ TBL_ByteCode ];
            break;
        }

        if(p->RVA != CLR_EmptyIndex)
        {
            end = p->RVA;
            break;
        }
    }

    return true;
}

bool CLR_RT_Assembly::FindNextStaticConstructor( CLR_RT_MethodDef_Index& idx )
{
    NATIVE_PROFILE_CLR_CORE();
    _ASSERTE(m_idx == idx.Assembly());

    for(int i=idx.Method(); i<m_pTablesSize[ TBL_MethodDef ]; i++)
    {
        const CLR_RECORD_METHODDEF* md = GetMethodDef( i );

        idx.Set( m_idx, i );

        if(md->flags & CLR_RECORD_METHODDEF::MD_StaticConstructor)
        {
            return true;
        }
    }

    idx.Clear();
    return false;
}

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT CLR_RT_Assembly::Resolve_ComputeHashes()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    const CLR_RECORD_TYPEDEF*      src = GetTypeDef( 0 );
    CLR_RT_TypeDef_CrossReference* dst = m_pCrossReference_TypeDef;

    for(int i=0; i<m_pTablesSize[TBL_TypeDef]; i++, src++, dst++)
    {
        CLR_RT_TypeDef_Index    idx ; idx.Set( m_idx, i );
        CLR_RT_TypeDef_Instance inst; inst.InitializeFromIndex( idx );
        CLR_UINT32              hash = ComputeHashForName( idx, 0 );


        while(TINYCLR_INDEX_IS_VALID(inst))
        {
            const CLR_RECORD_TYPEDEF*  target = inst.m_target;
            const CLR_RECORD_FIELDDEF* fd     = inst.m_assm->GetFieldDef( target->iFields_First );

            for(int j=0; j<target->iFields_Num; j++, fd++)
            {
                if((fd->flags & CLR_RECORD_FIELDDEF::FD_NotSerialized) == 0)
                {
                    CLR_RT_SignatureParser          parser; parser.Initialize_FieldDef( inst.m_assm, fd );
                    CLR_RT_SignatureParser::Element res;

                    TINYCLR_CHECK_HRESULT(parser.Advance( res ));

                    while(res.m_levels-- > 0)
                    {
                        hash = ComputeHashForType( DATATYPE_SZARRAY, hash );
                    }

                    hash = ComputeHashForType(  res.m_dt, hash );

                    switch(res.m_dt)
                    {
                    case DATATYPE_VALUETYPE:
                    case DATATYPE_CLASS    :
                        hash = ComputeHashForName( res.m_cls, hash );
                        break;
                    }

                    LPCSTR fieldName = inst.m_assm->GetString( fd->name );

                    hash = SUPPORT_ComputeCRC( fieldName, (CLR_UINT32)hal_strlen_s(fieldName), hash );
                }
            }

            inst.SwitchToParent();
        }

        dst->m_hash = hash ? hash : 0xFFFFFFFF; // Don't allow zero as an hash value!!
    }

    TINYCLR_NOCLEANUP();
}

CLR_UINT32 CLR_RT_Assembly::ComputeHashForName( const CLR_RT_TypeDef_Index& td, CLR_UINT32 hash )
{
    NATIVE_PROFILE_CLR_CORE();
    char   rgBuffer[ 512 ];
    LPSTR  szBuffer = rgBuffer;
    size_t iBuffer  = MAXSTRLEN(rgBuffer);

    g_CLR_RT_TypeSystem.BuildTypeName( td, szBuffer, iBuffer );

    CLR_UINT32 hashPost = SUPPORT_ComputeCRC( rgBuffer, (int)(MAXSTRLEN(rgBuffer) - iBuffer), hash );

    return hashPost;
}

CLR_UINT32 CLR_RT_Assembly::ComputeHashForType( CLR_DataType et, CLR_UINT32 hash )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_UINT8 val = (CLR_UINT8)CLR_RT_TypeSystem::MapDataTypeToElementType( et );

    CLR_UINT32 hashPost = SUPPORT_ComputeCRC( &val, sizeof(val), hash );

    return hashPost;
}

//--//

CLR_RT_HeapBlock* CLR_RT_Assembly::GetStaticField( const int index )
{
    NATIVE_PROFILE_CLR_CORE();

#if defined(TINYCLR_APPDOMAINS)

    CLR_RT_AppDomainAssembly* adAssm = g_CLR_RT_ExecutionEngine.GetCurrentAppDomain()->FindAppDomainAssembly( this );

    _ASSERTE(adAssm);

    return &adAssm->m_pStaticFields[ index ];
    
#else

    return &m_pStaticFields[ index ];

#endif
}

//--//

void CLR_RT_Assembly::Relocate()
{
    NATIVE_PROFILE_CLR_CORE();

#if !defined(TINYCLR_APPDOMAINS)
    CLR_RT_GarbageCollector::Heap_Relocate( m_pStaticFields, m_iStaticFields );
#endif

    CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_header     );
    CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_szName     );
    CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_pFile      );
    CLR_RT_GarbageCollector::Heap_Relocate( (void**)&m_nativeCode );
}

HRESULT CLR_RT_Assembly::VerifyEndian(CLR_RECORD_ASSEMBLY* header)
{
    UINT32 u = 0x1234567;
    UINT8 *t = (UINT8*)&u;
    BOOL  localIsBE=false;
    BOOL  assyIsBE=false;

    TINYCLR_HEADER();
    
    // Is this a Big Endian system?
    if ( 0x12!=*t)
    {
        localIsBE=true;
    }

    if( (header->flags & CLR_RECORD_ASSEMBLY::c_Flags_BigEndian) == CLR_RECORD_ASSEMBLY::c_Flags_BigEndian )
    {
        assyIsBE=true;
    }
    
    if (assyIsBE==localIsBE)
    {
        TINYCLR_SET_AND_LEAVE(S_OK);
    }
    else
    {
        TINYCLR_SET_AND_LEAVE(S_FALSE);
    }

    TINYCLR_NOCLEANUP();
}

////////////////////////////////////////////////////////////////////////////////////////////////////

void CLR_RT_TypeSystem::TypeSystem_Initialize()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_CLEAR(g_CLR_RT_TypeSystem);
    TINYCLR_CLEAR(g_CLR_RT_WellKnownTypes);
}

void CLR_RT_TypeSystem::TypeSystem_Cleanup()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_FOREACH_ASSEMBLY(*this)
    {
        pASSM->DestroyInstance();

        *ppASSM = NULL;
    }
    TINYCLR_FOREACH_ASSEMBLY_END();

    m_assembliesMax = 0;
}

//--//

void CLR_RT_TypeSystem::Link( CLR_RT_Assembly* assm )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_FOREACH_ASSEMBLY_NULL(*this)
    {
        *ppASSM = assm;

        assm->m_idx = idx;

        PostLinkageProcessing( assm );

        if(m_assembliesMax < idx) m_assembliesMax = idx;

        return;
    }
    TINYCLR_FOREACH_ASSEMBLY_NULL_END();
}

void CLR_RT_TypeSystem::PostLinkageProcessing( CLR_RT_Assembly* assm )
{
    NATIVE_PROFILE_CLR_CORE();
    if(!strcmp( assm->m_szName, "mscorlib" ))
    {
        m_assemblyMscorlib = assm;
    }
    if(!strcmp( assm->m_szName, "Microsoft.SPOT.Native" ))
    {
        m_assemblyNative = assm;
    }
}

CLR_RT_Assembly* CLR_RT_TypeSystem::FindAssembly( LPCSTR szName, const CLR_RECORD_VERSION* ver, bool fExact )
{
    NATIVE_PROFILE_CLR_CORE();

    TINYCLR_FOREACH_ASSEMBLY(*this)
    {
        if(!strcmp(pASSM->m_szName, szName))
        {
            // if there is no version information, anything goes
            if(NULL == ver)
            {
                return pASSM;
            }
            // exact match must take into accoutn all numbers
            else if(fExact)
            {
                if(0 == memcmp( &pASSM->m_header->version, ver, sizeof(*ver) ))
                {
                    return pASSM;
                }
            }
            // if excet match is not required but still we have version we will enforce only the first two number because by convention
            // we increse the minor numbers when native assemblies change CRC
            else if(
                     ver->iMajorVersion == pASSM->m_header->version.iMajorVersion &&
                     ver->iMinorVersion == pASSM->m_header->version.iMinorVersion
                   )
            {
                return pASSM;
            }
        }
    }
    TINYCLR_FOREACH_ASSEMBLY_END();

    return NULL;
}

bool CLR_RT_TypeSystem::FindTypeDef( LPCSTR name, LPCSTR nameSpace, CLR_RT_Assembly* assm, CLR_RT_TypeDef_Index& res )
{
    NATIVE_PROFILE_CLR_CORE();

    if(assm)
    {
        TINYCLR_FOREACH_ASSEMBLY_IN_CURRENT_APPDOMAIN(*this)
        {
            if(pASSM->IsSameAssembly( *assm ) && pASSM->FindTypeDef( name, nameSpace, res )) return true;
        }
        TINYCLR_FOREACH_ASSEMBLY_IN_CURRENT_APPDOMAIN_END();

        res.Clear();
        
        return false;
    }

    return FindTypeDef( name, nameSpace, res);
}

bool CLR_RT_TypeSystem::FindTypeDef( LPCSTR name, LPCSTR nameSpace, CLR_RT_TypeDef_Index& res )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_FOREACH_ASSEMBLY_IN_CURRENT_APPDOMAIN(*this)
    {
        if(pASSM->FindTypeDef( name, nameSpace, res )) return true;
    }
    TINYCLR_FOREACH_ASSEMBLY_IN_CURRENT_APPDOMAIN_END();

    res.Clear();
    return false;
}

bool CLR_RT_TypeSystem::FindTypeDef( CLR_UINT32 hash, CLR_RT_TypeDef_Index& res )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_FOREACH_ASSEMBLY_IN_CURRENT_APPDOMAIN(*this)
    {
        if(pASSM->FindTypeDef( hash, res )) return true;
    }
    TINYCLR_FOREACH_ASSEMBLY_IN_CURRENT_APPDOMAIN_END();

    res.Clear();
    return false;
}

bool CLR_RT_TypeSystem::FindTypeDef( LPCSTR szClass, CLR_RT_Assembly* assm, CLR_RT_TypeDef_Index& res )
{
    NATIVE_PROFILE_CLR_CORE();
    char rgName     [ MAXTYPENAMELEN ];
    char rgNamespace[ MAXTYPENAMELEN ];

    if(hal_strlen_s(szClass) < ARRAYSIZE(rgNamespace))
    {
        LPCSTR szPtr              = szClass;
        LPCSTR szPtr_LastDot      = NULL;
        LPCSTR szPtr_FirstSubType = NULL;
        char   c;
        size_t len;

        while(true)
        {
            c = szPtr[ 0 ]; if(!c) break;

            if(c == '.')
            {
                szPtr_LastDot = szPtr;
            }
            else if(c == '+')
            {
                szPtr_FirstSubType = szPtr;
                break;
            }

            szPtr++;
        }

        if(szPtr_LastDot)
        {
            len = szPtr_LastDot++ - szClass      ; hal_strncpy_s( rgNamespace, ARRAYSIZE(rgNamespace), szClass      , len );
            len = szPtr           - szPtr_LastDot; hal_strncpy_s( rgName     , ARRAYSIZE(rgName     ), szPtr_LastDot, len );
        }
        else
        {
            rgNamespace[ 0 ] = 0;
            hal_strcpy_s( rgName, ARRAYSIZE(rgName), szClass );
        }

        
        if(FindTypeDef( rgName, rgNamespace, assm, res ))
        {
            //
            // Found the containing type, let's look for the nested type.
            //
            if(szPtr_FirstSubType)
            {
                CLR_RT_TypeDef_Instance inst;

                do
                {
                    szPtr = ++szPtr_FirstSubType;

                    while(true)
                    {
                        c = szPtr_FirstSubType[ 0 ]; if(!c) break;

                        if(c == '+') break;

                        szPtr_FirstSubType++;
                    }

                    len = szPtr_FirstSubType - szPtr; hal_strncpy_s( rgName, ARRAYSIZE(rgName), szPtr, len );

                    inst.InitializeFromIndex( res );

                    if(inst.m_assm->FindTypeDef( rgName, res.Type(), res ) == false)
                    {
                        return false;
                    }

                } while(c == '+');
            }

            return true;
        }
    }

    res.Clear();

    return false;
}

bool CLR_RT_TypeSystem::FindTypeDef( LPCSTR szClass, CLR_RT_Assembly* assm, CLR_RT_ReflectionDef_Index& reflex )
{
    NATIVE_PROFILE_CLR_CORE();
    char rgName     [ MAXTYPENAMELEN ];
    char rgNamespace[ MAXTYPENAMELEN ];
    CLR_RT_TypeDef_Index res;

    if(hal_strlen_s(szClass) < ARRAYSIZE(rgNamespace))
    {
        LPCSTR szPtr              = szClass;
        LPCSTR szPtr_LastDot      = NULL;
        LPCSTR szPtr_FirstSubType = NULL;
        char   c;
        size_t len;
        bool arrayType = false;

        while(true)
        {
            c = szPtr[ 0 ]; if(!c) break;

            if(c == '.')
            {
                szPtr_LastDot = szPtr;
            }
            else if(c == '+')
            {
                szPtr_FirstSubType = szPtr;
                break;
            }
            else if(c == '[')
            {
                char ch = szPtr[ 1 ];
                if (ch == ']')
                {
                    arrayType = true;
                    break;
                }
            }

            szPtr++;
        }

        if(szPtr_LastDot)
        {
            len = szPtr_LastDot++ - szClass      ; hal_strncpy_s( rgNamespace, ARRAYSIZE(rgNamespace), szClass      , len );
            len = szPtr           - szPtr_LastDot; hal_strncpy_s( rgName     , ARRAYSIZE(rgName     ), szPtr_LastDot, len );
        }
        else
        {
            rgNamespace[ 0 ] = 0;
            hal_strcpy_s( rgName, ARRAYSIZE(rgName), szClass );
        }

        if(FindTypeDef( rgName, rgNamespace, assm, res ))
        {
            //
            // Found the containing type, let's look for the nested type.
            //
            if(szPtr_FirstSubType)
            {
                CLR_RT_TypeDef_Instance inst;

                do
                {
                    szPtr = ++szPtr_FirstSubType;

                    while(true)
                    {
                        c = szPtr_FirstSubType[ 0 ]; if(!c) break;

                        if(c == '+') break;

                        szPtr_FirstSubType++;
                    }

                    len = szPtr_FirstSubType - szPtr; hal_strncpy_s( rgName, ARRAYSIZE(rgName), szPtr, len );

                    inst.InitializeFromIndex( res );

                    if(inst.m_assm->FindTypeDef( rgName, res.Type(), res ) == false)
                    {
                        return false;
                    }

                } while(c == '+');
            }

            reflex.m_kind        = REFLECTION_TYPE;
            // make sure this works for multidimensional arrays.
            reflex.m_levels      = arrayType ? 1 : 0;
            reflex.m_data.m_type = res;
            return true;
        }
    }

    res.Clear();

    return false;
}

//--//

int
#if defined(_MSC_VER)
__cdecl
#endif
CompareResource( const void* p1, const void* p2 )
{
    NATIVE_PROFILE_CLR_CORE();
    const CLR_RECORD_RESOURCE* resource1 = (const CLR_RECORD_RESOURCE*)p1;
    const CLR_RECORD_RESOURCE* resource2 = (const CLR_RECORD_RESOURCE*)p2;

    return (int)resource1->id - (int)resource2->id;
}

HRESULT CLR_RT_TypeSystem::LocateResourceFile( CLR_RT_Assembly_Instance assm, LPCSTR name, CLR_INT32& idxResourceFile )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_Assembly* pAssm = assm.m_assm;

    for(idxResourceFile = 0; idxResourceFile < pAssm->m_pTablesSize[ TBL_ResourcesFiles ]; idxResourceFile++)
    {
        const CLR_RECORD_RESOURCE_FILE* resourceFile = pAssm->GetResourceFile( idxResourceFile );

        if(!strcmp( pAssm->GetString( resourceFile->name ), name ))
        {
            TINYCLR_SET_AND_LEAVE(S_OK);
        }
    }

    idxResourceFile = -1;

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_TypeSystem::LocateResource( CLR_RT_Assembly_Instance assm, CLR_INT32 idxResourceFile, CLR_INT16 id, const CLR_RECORD_RESOURCE*& res, CLR_UINT32& size )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_Assembly* pAssm = assm.m_assm;
    const CLR_RECORD_RESOURCE_FILE* resourceFile;
    CLR_RECORD_RESOURCE resourceT;
    const CLR_RECORD_RESOURCE* resNext;
    const CLR_RECORD_RESOURCE* resZero;

    res  = NULL;
    size = 0;

    if(idxResourceFile < 0 || idxResourceFile >= pAssm->m_pTablesSize[ TBL_ResourcesFiles ]) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);

    resourceFile = pAssm->GetResourceFile( idxResourceFile );

    _ASSERTE(resourceFile->numberOfResources > 0);

    resZero = pAssm->GetResource( resourceFile->offset );

    resourceT.id = id;

    res = (const CLR_RECORD_RESOURCE*)bsearch( &resourceT, resZero, resourceFile->numberOfResources, sizeof(CLR_RECORD_RESOURCE), CompareResource );

    if(res != NULL)
    {
        //compute size here...
        //assert not the last resource
        _ASSERTE(res + 1 <= pAssm->GetResource( pAssm->m_pTablesSize[ TBL_Resources ] - 1));
        resNext = res+1;

        size = resNext->offset - res->offset;

        //deal with alignment.
        size -= (resNext->flags & CLR_RECORD_RESOURCE::FLAGS_PaddingMask);
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_TypeSystem::ResolveAll()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    bool fOutput = false;

    while(true)
    {
        bool fGot            = false;
        bool fNeedResolution = false;

        TINYCLR_FOREACH_ASSEMBLY(*this)
        {
            if((pASSM->m_flags & CLR_RT_Assembly::c_Resolved) == 0)
            {
                fNeedResolution = true;

                if(pASSM->Resolve_AssemblyRef( fOutput ))
                {
                    fGot = true;

                    pASSM->m_flags |= CLR_RT_Assembly::c_Resolved;

                    TINYCLR_CHECK_HRESULT(pASSM->Resolve_TypeRef       ());
                    TINYCLR_CHECK_HRESULT(pASSM->Resolve_FieldRef      ());
                    TINYCLR_CHECK_HRESULT(pASSM->Resolve_MethodRef     ());
                    /********************/pASSM->Resolve_TypeDef       () ;
                    /********************/pASSM->Resolve_MethodDef     () ;
                    /********************/pASSM->Resolve_Link          () ;
                    TINYCLR_CHECK_HRESULT(pASSM->Resolve_ComputeHashes ());

#if !defined(TINYCLR_APPDOMAINS)
                    TINYCLR_CHECK_HRESULT(pASSM->Resolve_AllocateStaticFields( pASSM->m_pStaticFields ));
#endif

                    pASSM->m_flags |= CLR_RT_Assembly::c_ResolutionCompleted;
                }
            }
        }
        TINYCLR_FOREACH_ASSEMBLY_END();

        if(fOutput == true)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_TYPE_UNAVAILABLE);
        }

        if(fGot == false)
        {
            if(fNeedResolution)
            {
#if !defined(BUILD_RTM)
                CLR_Debug::Printf( "Link failure: some assembly references cannot be resolved!!\r\n\r\n" );
#endif

                fOutput = true;
            }
            else
            {
                break;
            }
        }
    }

#if !defined(BUILD_RTM)

    if(s_CLR_RT_fTrace_AssemblyOverhead >= c_CLR_RT_Trace_Info)
    {
        {
            int                      pTablesSize[ TBL_Max ]; memset(  pTablesSize, 0, sizeof(pTablesSize) );
            CLR_RT_Assembly::Offsets offsets               ; memset( &offsets    , 0, sizeof(offsets    ) );

            size_t                   iStaticFields   = 0 ;
            size_t                   iTotalRamSize   = 0 ;
            size_t                   iTotalRomSize   = 0 ;
            size_t                   iMetaData       = 0 ;

            TINYCLR_FOREACH_ASSEMBLY(*this)
            {
                offsets.iBase                 += ROUNDTOMULTIPLE(sizeof(CLR_RT_Assembly)                                                            , CLR_UINT32);
                offsets.iAssemblyRef          += ROUNDTOMULTIPLE(pASSM->m_pTablesSize[ TBL_AssemblyRef ] * sizeof(CLR_RT_AssemblyRef_CrossReference), CLR_UINT32);
                offsets.iTypeRef              += ROUNDTOMULTIPLE(pASSM->m_pTablesSize[ TBL_TypeRef     ] * sizeof(CLR_RT_TypeRef_CrossReference    ), CLR_UINT32);
                offsets.iFieldRef             += ROUNDTOMULTIPLE(pASSM->m_pTablesSize[ TBL_FieldRef    ] * sizeof(CLR_RT_FieldRef_CrossReference   ), CLR_UINT32);
                offsets.iMethodRef            += ROUNDTOMULTIPLE(pASSM->m_pTablesSize[ TBL_MethodRef   ] * sizeof(CLR_RT_MethodRef_CrossReference  ), CLR_UINT32);
                offsets.iTypeDef              += ROUNDTOMULTIPLE(pASSM->m_pTablesSize[ TBL_TypeDef     ] * sizeof(CLR_RT_TypeDef_CrossReference    ), CLR_UINT32);
                offsets.iFieldDef             += ROUNDTOMULTIPLE(pASSM->m_pTablesSize[ TBL_FieldDef    ] * sizeof(CLR_RT_FieldDef_CrossReference   ), CLR_UINT32);
                offsets.iMethodDef            += ROUNDTOMULTIPLE(pASSM->m_pTablesSize[ TBL_MethodDef   ] * sizeof(CLR_RT_MethodDef_CrossReference  ), CLR_UINT32);

#if !defined(TINYCLR_APPDOMAINS)
                offsets.iStaticFields         += ROUNDTOMULTIPLE(pASSM->m_iStaticFields                * sizeof(CLR_RT_HeapBlock                 ), CLR_UINT32);
#endif

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
                offsets.iDebuggingInfoMethods += ROUNDTOMULTIPLE(pASSM->m_pTablesSize[ TBL_MethodDef ] * sizeof(CLR_RT_MethodDef_DebuggingInfo   ), CLR_UINT32);
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

                iMetaData += pASSM->m_header->SizeOfTable( TBL_AssemblyRef ) +
                             pASSM->m_header->SizeOfTable( TBL_TypeRef     ) +
                             pASSM->m_header->SizeOfTable( TBL_FieldRef    ) +
                             pASSM->m_header->SizeOfTable( TBL_MethodRef   ) +
                             pASSM->m_header->SizeOfTable( TBL_TypeDef     ) +
                             pASSM->m_header->SizeOfTable( TBL_FieldDef    ) +
                             pASSM->m_header->SizeOfTable( TBL_MethodDef   ) +
                             pASSM->m_header->SizeOfTable( TBL_Attributes  ) +
                             pASSM->m_header->SizeOfTable( TBL_TypeSpec    ) +
                             pASSM->m_header->SizeOfTable( TBL_Signatures  );

                for(int tbl=0; tbl<TBL_Max; tbl++)
                {
                    pTablesSize[ tbl ] += pASSM->m_pTablesSize[ tbl ];
                }

                iTotalRomSize += pASSM->m_header->TotalSize();

                iStaticFields += pASSM->m_iStaticFields;
            }
            TINYCLR_FOREACH_ASSEMBLY_END();

            iTotalRamSize = offsets.iBase           +
                            offsets.iAssemblyRef    +
                            offsets.iTypeRef        +
                            offsets.iFieldRef       +
                            offsets.iMethodRef      +
                            offsets.iTypeDef        +
                            offsets.iFieldDef       +
                            offsets.iMethodDef;

#if !defined(TINYCLR_APPDOMAINS)
            iTotalRamSize += offsets.iStaticFields;
#endif

            CLR_Debug::Printf( "\r\nTotal: (%d RAM - %d ROM - %d METADATA)\r\n\r\n", iTotalRamSize, iTotalRomSize, iMetaData );

            CLR_Debug::Printf( "   AssemblyRef    = %8d bytes (%8d elements)\r\n", offsets.iAssemblyRef   , pTablesSize[TBL_AssemblyRef] );
            CLR_Debug::Printf( "   TypeRef        = %8d bytes (%8d elements)\r\n", offsets.iTypeRef       , pTablesSize[TBL_TypeRef    ] );
            CLR_Debug::Printf( "   FieldRef       = %8d bytes (%8d elements)\r\n", offsets.iFieldRef      , pTablesSize[TBL_FieldRef   ] );
            CLR_Debug::Printf( "   MethodRef      = %8d bytes (%8d elements)\r\n", offsets.iMethodRef     , pTablesSize[TBL_MethodRef  ] );
            CLR_Debug::Printf( "   TypeDef        = %8d bytes (%8d elements)\r\n", offsets.iTypeDef       , pTablesSize[TBL_TypeDef    ] );
            CLR_Debug::Printf( "   FieldDef       = %8d bytes (%8d elements)\r\n", offsets.iFieldDef      , pTablesSize[TBL_FieldDef   ] );
            CLR_Debug::Printf( "   MethodDef      = %8d bytes (%8d elements)\r\n", offsets.iMethodDef     , pTablesSize[TBL_MethodDef  ] );

#if !defined(TINYCLR_APPDOMAINS)
            CLR_Debug::Printf( "   StaticFields   = %8d bytes (%8d elements)\r\n", offsets.iStaticFields  , iStaticFields                );
#endif

            CLR_Debug::Printf( "\r\n" );

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
            CLR_Debug::Printf( "   DebuggingInfo  = %8d bytes\r\n", offsets.iDebuggingInfoMethods );
            CLR_Debug::Printf( "\r\n" );
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

            CLR_Debug::Printf( "   Attributes      = %8d bytes (%8d elements)\r\n", pTablesSize[ TBL_Attributes     ] * sizeof(CLR_RECORD_ATTRIBUTE     ), pTablesSize[ TBL_Attributes     ] );
            CLR_Debug::Printf( "   TypeSpec        = %8d bytes (%8d elements)\r\n", pTablesSize[ TBL_TypeSpec       ] * sizeof(CLR_RECORD_TYPESPEC      ), pTablesSize[ TBL_TypeSpec       ] );
            CLR_Debug::Printf( "   Resources Files = %8d bytes (%8d elements)\r\n", pTablesSize[ TBL_ResourcesFiles ] * sizeof(CLR_RECORD_RESOURCE_FILE ), pTablesSize[ TBL_ResourcesFiles ] );
            CLR_Debug::Printf( "   Resources       = %8d bytes (%8d elements)\r\n", pTablesSize[ TBL_Resources      ] * sizeof(CLR_RECORD_RESOURCE      ), pTablesSize[ TBL_Resources      ] );
            CLR_Debug::Printf( "   Resources Data  = %8d bytes\r\n"                                                                                      , pTablesSize[ TBL_ResourcesData  ] );
            CLR_Debug::Printf( "   Strings         = %8d bytes\r\n"                                                                                      , pTablesSize[ TBL_Strings        ] );
            CLR_Debug::Printf( "   Signatures      = %8d bytes\r\n"                                                                                      , pTablesSize[ TBL_Signatures     ] );
            CLR_Debug::Printf( "   ByteCode        = %8d bytes\r\n"                                                                                      , pTablesSize[ TBL_ByteCode       ] );
            CLR_Debug::Printf( "\r\n\r\n" );
        }
    }

#endif

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_TypeSystem::PrepareForExecutionHelper( LPCSTR szAssembly )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_FOREACH_ASSEMBLY(*this)
    {
        if(!strcmp( szAssembly, pASSM->m_szName ))
        {
            TINYCLR_CHECK_HRESULT(pASSM->PrepareForExecution());
        }
    }

    TINYCLR_FOREACH_ASSEMBLY_END();

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_TypeSystem::PrepareForExecution()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
    CLR_EE_DBG_SET(BreakpointsDisabled);
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

#if !defined(TINYCLR_APPDOMAINS)
    if(g_CLR_RT_ExecutionEngine.m_outOfMemoryException == NULL)
    {
        CLR_RT_HeapBlock exception;

        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex( exception, g_CLR_RT_WellKnownTypes.m_OutOfMemoryException ));

        g_CLR_RT_ExecutionEngine.m_outOfMemoryException = exception.Dereference();
    }
#endif

    //Load Native to ensure that CultureInfo gets properly initialized
    TINYCLR_CHECK_HRESULT(PrepareForExecutionHelper( "Microsoft.SPOT.Native" ));

    TINYCLR_FOREACH_ASSEMBLY(*this)
    {
        TINYCLR_CHECK_HRESULT(pASSM->PrepareForExecution());
    }
    TINYCLR_FOREACH_ASSEMBLY_END();

    TINYCLR_CLEANUP();

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
    CLR_EE_DBG_CLR(BreakpointsDisabled);

    g_CLR_RT_ExecutionEngine.Breakpoint_Assemblies_Loaded();
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

    TINYCLR_CLEANUP_END();
}

//--//

bool CLR_RT_TypeSystem::MatchSignature( CLR_RT_SignatureParser& parserLeft, CLR_RT_SignatureParser& parserRight )
{
    NATIVE_PROFILE_CLR_CORE();
    if(parserLeft.m_type  != parserRight.m_type ) return false;
    if(parserLeft.m_flags != parserRight.m_flags) return false;

    return MatchSignatureDirect( parserLeft, parserRight, false );
}

bool CLR_RT_TypeSystem::MatchSignatureDirect( CLR_RT_SignatureParser& parserLeft, CLR_RT_SignatureParser& parserRight, bool fIsInstanceOfOK )
{
    NATIVE_PROFILE_CLR_CORE();
    while(true)
    {
        int iAvailLeft  = parserLeft .Available();
        int iAvailRight = parserRight.Available();

        if(iAvailLeft != iAvailRight) return false;

        if(!iAvailLeft) return true;

        CLR_RT_SignatureParser::Element resLeft;  if(FAILED(parserLeft .Advance( resLeft  ))) return false;
        CLR_RT_SignatureParser::Element resRight; if(FAILED(parserRight.Advance( resRight ))) return false;

        if(!MatchSignatureElement( resLeft, resRight, fIsInstanceOfOK )) return false;
    }

    return true;
}

bool CLR_RT_TypeSystem::MatchSignatureElement( CLR_RT_SignatureParser::Element& resLeft, CLR_RT_SignatureParser::Element& resRight, bool fIsInstanceOfOK )
{
    NATIVE_PROFILE_CLR_CORE();
    if(fIsInstanceOfOK)
    {
        CLR_RT_ReflectionDef_Index idxLeft ; CLR_RT_TypeDescriptor descLeft;
        CLR_RT_ReflectionDef_Index idxRight; CLR_RT_TypeDescriptor descRight;

        idxLeft.m_kind          = REFLECTION_TYPE;
        idxLeft.m_levels        = resLeft.m_levels;
        idxLeft.m_data.m_type   = resLeft.m_cls;

        idxRight.m_kind         = REFLECTION_TYPE;
        idxRight.m_levels       = resRight.m_levels;
        idxRight.m_data.m_type  = resRight.m_cls;

        if(FAILED(descLeft .InitializeFromReflection( idxLeft  ))) return false;
        if(FAILED(descRight.InitializeFromReflection( idxRight ))) return false;

        if(!CLR_RT_ExecutionEngine::IsInstanceOf( descRight, descLeft )) return false;
    }
    else
    {
        if(resLeft.m_fByRef     != resRight.m_fByRef    ) return false;
        if(resLeft.m_levels     != resRight.m_levels    ) return false;
        if(resLeft.m_dt         != resRight.m_dt        ) return false;
        if(resLeft.m_cls.m_data != resRight.m_cls.m_data) return false;
    }

    return true;
}

//--//

HRESULT CLR_RT_TypeSystem::QueueStringToBuffer( LPSTR& szBuffer, size_t& iBuffer, LPCSTR szText )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    if(szText)
    {
        if(CLR_SafeSprintf( szBuffer, iBuffer, "%s", szText ) == false)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_MEMORY);
        }
    }
    else
    {
        szBuffer[ 0 ] = 0;
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_TypeSystem::BuildTypeName( const CLR_RT_TypeDef_Index& cls, LPSTR& szBuffer, size_t& iBuffer )
{
    NATIVE_PROFILE_CLR_CORE();
    return BuildTypeName( cls, szBuffer, iBuffer, CLR_RT_TypeSystem::TYPENAME_FLAGS_FULL, 0 );
}

HRESULT CLR_RT_TypeSystem::BuildTypeName ( const CLR_RT_TypeDef_Index& cls, LPSTR& szBuffer, size_t& iBuffer, CLR_UINT32 flags, CLR_UINT32 levels )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_TypeDef_Instance   inst;
    CLR_RT_Assembly*          assm;
    const CLR_RECORD_TYPEDEF* td;
    bool fFullName;

    if(inst.InitializeFromIndex( cls ) == false) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);

    assm      = inst.m_assm;
    td        = inst.m_target;
    fFullName = flags & CLR_RT_TypeSystem::TYPENAME_FLAGS_FULL;

    if(fFullName && td->enclosingType != CLR_EmptyIndex)
    {
        CLR_RT_TypeDef_Index clsSub; clsSub.Set( inst.Assembly(), td->enclosingType );

        TINYCLR_CHECK_HRESULT(BuildTypeName( clsSub, szBuffer, iBuffer, flags, 0 ));

        TINYCLR_CHECK_HRESULT(QueueStringToBuffer( szBuffer, iBuffer, (flags & CLR_RT_TypeSystem::TYPENAME_NESTED_SEPARATOR_DOT) ? "." : "+" ));
    }

    if(fFullName && td->nameSpace != CLR_EmptyIndex)
    {
        LPCSTR szNameSpace = assm->GetString( td->nameSpace );

        if(szNameSpace[ 0 ])
        {
            TINYCLR_CHECK_HRESULT(QueueStringToBuffer( szBuffer, iBuffer, szNameSpace ));
            TINYCLR_CHECK_HRESULT(QueueStringToBuffer( szBuffer, iBuffer, "."         ));
        }
    }

    TINYCLR_CHECK_HRESULT(QueueStringToBuffer( szBuffer, iBuffer, assm->GetString( td->name ) ));

    while(levels-- > 0)
    {
        TINYCLR_CHECK_HRESULT(QueueStringToBuffer( szBuffer, iBuffer, "[]" ));
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_TypeSystem::BuildMethodName( const CLR_RT_MethodDef_Index& md, LPSTR& szBuffer, size_t& iBuffer )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_MethodDef_Instance inst;
    CLR_RT_TypeDef_Instance   instOwner;

    if(inst     .InitializeFromIndex ( md   ) == false) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    if(instOwner.InitializeFromMethod( inst ) == false) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);

    TINYCLR_CHECK_HRESULT(BuildTypeName( instOwner, szBuffer, iBuffer ));

    CLR_SafeSprintf( szBuffer, iBuffer, "::%s", inst.m_assm->GetString( inst.m_target->name ) );

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_TypeSystem::BuildFieldName( const CLR_RT_FieldDef_Index& fd, LPSTR& szBuffer, size_t& iBuffer )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_FieldDef_Instance  inst;
    CLR_RT_TypeDef_Instance   instOwner;

    if(inst     .InitializeFromIndex( fd   ) == false) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    if(instOwner.InitializeFromField( inst ) == false) TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);

    TINYCLR_CHECK_HRESULT(BuildTypeName( instOwner, szBuffer, iBuffer ));

    CLR_SafeSprintf( szBuffer, iBuffer, "::%s", inst.m_assm->GetString( inst.m_target->name ) );

    TINYCLR_NOCLEANUP();
}

//--//

bool CLR_RT_TypeSystem::FindVirtualMethodDef( const CLR_RT_TypeDef_Index& cls, const CLR_RT_MethodDef_Index& calleeMD, CLR_RT_MethodDef_Index& idx )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_MethodDef_Instance calleeInst;

    if(calleeInst.InitializeFromIndex( calleeMD ))
    {
        LPCSTR calleeName = calleeInst.m_assm->GetString( calleeInst.m_target->name );

        CLR_RT_TypeDef_Instance inst; inst.InitializeFromMethod( calleeInst );

        if((inst.m_target->flags & CLR_RECORD_TYPEDEF::TD_Semantics_Mask) == CLR_RECORD_TYPEDEF::TD_Semantics_Interface)
        {
            //
            // It's an interface method, it could be that the class is implementing explicitly the method.
            // Prepend the Interface name to the method name and try again.
            //
            char   rgBuffer[ 512 ];
            LPSTR  szBuffer = rgBuffer;
            size_t iBuffer  = MAXSTRLEN(rgBuffer);

            BuildTypeName      ( inst, szBuffer, iBuffer, CLR_RT_TypeSystem::TYPENAME_FLAGS_FULL | CLR_RT_TypeSystem::TYPENAME_NESTED_SEPARATOR_DOT, 0 );
            QueueStringToBuffer(       szBuffer, iBuffer, "."        );
            QueueStringToBuffer(       szBuffer, iBuffer, calleeName );

            if(FindVirtualMethodDef( cls, calleeMD, rgBuffer, idx )) return true;
        }

        if(FindVirtualMethodDef( cls, calleeMD, calleeName, idx )) return true;
    }

    idx.Clear();

    return false;
}

bool CLR_RT_TypeSystem::FindVirtualMethodDef( const CLR_RT_TypeDef_Index& cls, const CLR_RT_MethodDef_Index& calleeMD, LPCSTR calleeName, CLR_RT_MethodDef_Index& idx )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_TypeDef_Instance   clsInst   ; clsInst   .InitializeFromIndex( cls      );
    CLR_RT_MethodDef_Instance calleeInst; calleeInst.InitializeFromIndex( calleeMD );

    CLR_RT_Assembly*            calleeAssm    = calleeInst.m_assm;
    const CLR_RECORD_METHODDEF* calleeMDR     = calleeInst.m_target;
    CLR_UINT8                   calleeNumArgs = calleeMDR->numArgs;

    while(TINYCLR_INDEX_IS_VALID(clsInst))
    {
        CLR_RT_Assembly*            targetAssm = clsInst.m_assm;
        const CLR_RECORD_TYPEDEF*   targetTDR  = clsInst.m_target;
        const CLR_RECORD_METHODDEF* targetMDR  = targetAssm->GetMethodDef( targetTDR->methods_First );
        int                         num        =                           targetTDR->vMethods_Num + targetTDR->iMethods_Num;

        for(int i=0; i<num; i++, targetMDR++)
        {
            if(targetMDR == calleeMDR)
            {
                // Shortcut for identity.
                idx = calleeInst;
                return true;
            }

            if(targetMDR->numArgs == calleeNumArgs && (targetMDR->flags & CLR_RECORD_METHODDEF::MD_Abstract) == 0)
            {
                LPCSTR targetName = targetAssm->GetString( targetMDR->name );

                if(!strcmp( targetName, calleeName ))
                {
                    CLR_RT_SignatureParser parserLeft ; parserLeft .Initialize_MethodSignature( calleeAssm, calleeMDR );
                    CLR_RT_SignatureParser parserRight; parserRight.Initialize_MethodSignature( targetAssm, targetMDR );

                    if(CLR_RT_TypeSystem::MatchSignature( parserLeft, parserRight ))
                    {
                        idx.Set( targetAssm->m_idx, i + targetTDR->methods_First );

                        return true;
                    }
                }
            }
        }

        clsInst.SwitchToParent();
    }

    idx.Clear();

    return false;
}

CLR_DataType CLR_RT_TypeSystem::MapElementTypeToDataType( CLR_UINT32 et )
{
    NATIVE_PROFILE_CLR_CORE();
    const CLR_RT_DataTypeLookup* ptr = c_CLR_RT_DataTypeLookup;

    for(CLR_UINT32 num = 0; num<DATATYPE_FIRST_INVALID; num++, ptr++)
    {
        if(ptr->m_convertToElementType == et) return (CLR_DataType)num;
    }

    if(et == ELEMENT_TYPE_I) return DATATYPE_I4;
    if(et == ELEMENT_TYPE_U) return DATATYPE_U4;

    return DATATYPE_FIRST_INVALID;
}

CLR_UINT32 CLR_RT_TypeSystem::MapDataTypeToElementType( CLR_DataType dt )
{
    NATIVE_PROFILE_CLR_CORE();
    return c_CLR_RT_DataTypeLookup[ dt ].m_convertToElementType;
}

//--//

void CLR_RT_AttributeEnumerator::Initialize( CLR_RT_Assembly* assm )
{
    NATIVE_PROFILE_CLR_CORE();
    m_assm = assm;   // CLR_RT_Assembly*            m_assm;
    m_ptr  = NULL;   // const CLR_RECORD_ATTRIBUTE* m_ptr;
    m_num  = 0;      // int                         m_num;
                     // CLR_RECORD_ATTRIBUTE        m_data;
    m_match.Clear(); // CLR_RT_MethodDef_Index      m_match;
}

void CLR_RT_AttributeEnumerator::Initialize( const CLR_RT_TypeDef_Instance& inst )
{
    NATIVE_PROFILE_CLR_CORE();
    m_data.ownerType = TBL_TypeDef;
    m_data.ownerIdx  = inst.Type();

    Initialize( inst.m_assm );
}

void CLR_RT_AttributeEnumerator::Initialize( const CLR_RT_FieldDef_Instance& inst )
{
    NATIVE_PROFILE_CLR_CORE();
    m_data.ownerType = TBL_FieldDef;
    m_data.ownerIdx  = inst.Field();

    Initialize( inst.m_assm );
}

void CLR_RT_AttributeEnumerator::Initialize( const CLR_RT_MethodDef_Instance& inst )
{
    NATIVE_PROFILE_CLR_CORE();
    m_data.ownerType = TBL_FieldDef;
    m_data.ownerIdx  = inst.Method();

    Initialize( inst.m_assm );
}

bool CLR_RT_AttributeEnumerator::Advance()
{
    NATIVE_PROFILE_CLR_CORE();
    const CLR_RECORD_ATTRIBUTE* ptr  = m_ptr;
    int                         num  = m_num;
    CLR_UINT32                  key  = m_data.Key();
    bool                        fRes = false;

    if(ptr == NULL)
    {
        ptr = m_assm->GetAttribute( 0 )           - 1;
        num = m_assm->m_pTablesSize[ TBL_Attributes ];
    }

    while(num)
    {
        ptr++;
        num--;

        if(ptr->Key() == key)
        {
            CLR_IDX tk = ptr->constructor;
            if(tk & 0x8000)
            {
                m_match = m_assm->m_pCrossReference_MethodRef[ tk & 0x7FFF ].m_target;
            }
            else
            {
                m_match.Set( m_assm->m_idx, tk );
            }

            m_blob = m_assm->GetSignature( ptr->data );

            fRes = true;
            break;
        }
    }

    m_ptr = ptr;
    m_num = num;

    return fRes;
}

bool CLR_RT_AttributeEnumerator::MatchNext( const CLR_RT_TypeDef_Instance* instTD, const CLR_RT_MethodDef_Instance* instMD )
{
    NATIVE_PROFILE_CLR_CORE();
    while(Advance())
    {
        if(instMD)
        {
            if(m_match.m_data != instMD->m_data) continue;
        }

        if(instTD)
        {
            CLR_RT_MethodDef_Instance md;
            CLR_RT_TypeDef_Instance   td;

            md.InitializeFromIndex ( m_match );
            td.InitializeFromMethod( md      );

            if(td.m_data != instTD->m_data) continue;
        }

        return true;
    }

    return false;
}

//--//

HRESULT CLR_RT_AttributeParser::Initialize( const CLR_RT_AttributeEnumerator& en )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    if(m_md.InitializeFromIndex ( en.m_match ) == false ||
       m_td.InitializeFromMethod( m_md       ) == false  )
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    m_parser.Initialize_MethodSignature( m_md.m_assm, m_md.m_target );
    m_parser.Advance( m_res ); // Skip return value.

    m_assm = en.m_assm;
    m_blob = en.m_blob;

    m_currentPos  = 0;
    m_fixed_Count = m_md.m_target->numArgs - 1;
    m_named_Count = -1;

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_AttributeParser::Next( Value*& res )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    if(m_currentPos == m_fixed_Count)
    {
        TINYCLR_READ_UNALIGNED_UINT16(m_named_Count,m_blob);
    }

    if(m_currentPos < m_fixed_Count)
    {
        m_lastValue.m_mode = Value::c_ConstructorArgument;
        m_lastValue.m_name = NULL;

        //
        // attribute contructor support is currently not implemented
        //
        TINYCLR_SET_AND_LEAVE(CLR_E_NOT_SUPPORTED);
    }
    else if(m_currentPos < m_fixed_Count + m_named_Count)
    {
        CLR_UINT32 kind; TINYCLR_READ_UNALIGNED_UINT8(kind,m_blob);

        m_lastValue.m_name = GetString();

        if(kind == SERIALIZATION_TYPE_FIELD)
        {
            CLR_RT_FieldDef_Index    fd;
            CLR_RT_FieldDef_Instance inst;

            m_lastValue.m_mode = Value::c_NamedField;

            TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.FindFieldDef( m_td, m_lastValue.m_name, fd ));

            inst.InitializeFromIndex( fd );

            m_parser.Initialize_FieldDef( inst.m_assm, inst.m_target );
        }
        else
        {
            m_lastValue.m_mode = Value::c_NamedProperty;

            //
            //attribute contructor support is currently not implemented
            //
            TINYCLR_SET_AND_LEAVE(CLR_E_NOT_SUPPORTED);
        }
    }
    else
    {
        res = NULL;
        TINYCLR_SET_AND_LEAVE(S_OK);
    }


    TINYCLR_CHECK_HRESULT(m_parser.Advance( m_res ));

    res = &m_lastValue;

    //
    // Check for Enums.
    //
    if(m_res.m_dt == DATATYPE_VALUETYPE)
    {
        CLR_RT_TypeDef_Instance td; td.InitializeFromIndex( m_res.m_cls );

        if((td.m_target->flags & CLR_RECORD_TYPEDEF::TD_Semantics_Mask) == CLR_RECORD_TYPEDEF::TD_Semantics_Enum)
        {
            m_res.m_dt = (CLR_DataType)td.m_target->dataType;
        }
    }

    //
    // Skip value info.
    //
    m_blob += sizeof(CLR_UINT8);

    {
        const CLR_RT_DataTypeLookup& dtl = c_CLR_RT_DataTypeLookup[ m_res.m_dt ];

        if(dtl.m_flags & CLR_RT_DataTypeLookup::c_Numeric)
        {
            m_lastValue.m_value.SetDataId( CLR_RT_HEAPBLOCK_RAW_ID(m_res.m_dt, 0, 1) );

            CLR_UINT32 size = (dtl.m_sizeInBits + 7) / 8;

// FIXME GJS - the numeric values, what is their endiannes??? In the MSTV code there is a BIG endian fix but it looks like it will not work, so was it ever used?
#if !defined(NETMF_TARGET_BIG_ENDIAN)
            memcpy( &m_lastValue.m_value.NumericByRef(), m_blob, size ); m_blob += size;
#else
            switch(size)
            {
                // [1-4] copy to the High address since the low DWORD is in the high address
            case 1:
            case 2:
            case 3:
            case 4:
                memcpy( &m_lastValue.m_value.NumericByRef().u4, m_blob, size );
                m_lastValue.m_value.NumericByRef().u4=SwapEndian(m_lastValue.m_value.NumericByRef().u4);
                m_blob += size;

                break;
            case 5:
            case 6:
            case 7:
            case 8:
                memcpy( &m_lastValue.m_value.NumericByRef().u8, m_blob, size );

                m_lastValue.m_value.NumericByRef().u8._L=SwapEndian(m_lastValue.m_value.NumericByRef().u8._L);
                m_lastValue.m_value.NumericByRef().u8._H=SwapEndian(m_lastValue.m_value.NumericByRef().u8._H);
                m_blob += size;
                break;
            }
#endif
        }
        else if(m_res.m_dt == DATATYPE_STRING)
        {
            CLR_UINT32 tk; TINYCLR_READ_UNALIGNED_UINT16(tk,m_blob);

            CLR_RT_HeapBlock_String::CreateInstance( m_lastValue.m_value, CLR_TkFromType( TBL_Strings, tk ), m_assm );
        }
        else
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
        }
    }

    m_lastValue.m_pos = m_currentPos++;

    TINYCLR_NOCLEANUP();
}

LPCSTR CLR_RT_AttributeParser::GetString()
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_UINT32 tk; TINYCLR_READ_UNALIGNED_UINT16(tk,m_blob);

    return m_assm->GetString( tk );
}
