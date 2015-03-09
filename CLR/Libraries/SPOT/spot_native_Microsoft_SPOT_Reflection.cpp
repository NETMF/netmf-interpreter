////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPOT.h"


HRESULT Library_spot_native_Microsoft_SPOT_Reflection::GetTypesImplementingInterface___STATIC__SZARRAY_mscorlibSystemType__mscorlibSystemType( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock&       top    = stack.PushValueAndClear();
    int                     tot    = 0;
    CLR_RT_HeapBlock*       pArray = NULL;
    CLR_RT_TypeDef_Instance tdMatch;
    CLR_RT_HeapBlock*       hbType = stack.Arg0().Dereference();

    TINYCLR_CHECK_HRESULT(Library_corlib_native_System_RuntimeType::GetTypeDescriptor( *hbType, tdMatch ));

    if((tdMatch.m_target->flags & CLR_RECORD_TYPEDEF::TD_Semantics_Mask) != CLR_RECORD_TYPEDEF::TD_Semantics_Interface)
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }

    for(int pass=0; pass<2; pass++)
    {
        int              count = 0;

        TINYCLR_FOREACH_ASSEMBLY_IN_CURRENT_APPDOMAIN(g_CLR_RT_TypeSystem)
        {
            const CLR_RECORD_TYPEDEF* td      = pASSM->GetTypeDef( 0 );
            int                       tblSize = pASSM->m_pTablesSize[ TBL_TypeDef ];

            for(int i=0; i<tblSize; i++, td++)
            {
                if(td->flags & CLR_RECORD_TYPEDEF::TD_Abstract) continue;

                CLR_RT_TypeDef_Index idx; idx.Set( pASSM->m_idx, i );

                if(CLR_RT_ExecutionEngine::IsInstanceOf( idx, tdMatch ))
                {
                    if(pass == 0)
                    {
                        tot++;
                    }
                    else
                    {
                        CLR_RT_HeapBlock* hbObj;
                        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex(pArray[count], g_CLR_RT_WellKnownTypes.m_TypeStatic));
                        hbObj = pArray[count].Dereference();
                        hbObj->SetReflection( idx );
                    }

                    count++;
                }
            }
        }
        TINYCLR_FOREACH_ASSEMBLY_IN_CURRENT_APPDOMAIN_END();

        if(pass == 0)
        {
            if(tot == 0) break;

            TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance( top, tot, g_CLR_RT_WellKnownTypes.m_TypeStatic ));

            CLR_RT_HeapBlock_Array* array = top.DereferenceArray();

            pArray = (CLR_RT_HeapBlock*)array->GetFirstElement();
        }
    }


    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_native_Microsoft_SPOT_Reflection::IsTypeLoaded___STATIC__BOOLEAN__mscorlibSystemType( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_TypeDef_Instance inst;
    CLR_RT_HeapBlock*       hbType = stack.Arg0().Dereference();

    stack.SetResult_Boolean( CLR_RT_ReflectionDef_Index::Convert( *hbType, inst, NULL ) );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_native_Microsoft_SPOT_Reflection::GetTypeHash___STATIC__U4__mscorlibSystemType( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_UINT32        hash;
    CLR_RT_HeapBlock* hbType = stack.Arg0().Dereference();

    if(CLR_RT_ReflectionDef_Index::Convert( *hbType, hash ) == false)
    {
        hash = 0;
    }

    stack.SetResult( hash, DATATYPE_U4 );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_native_Microsoft_SPOT_Reflection::GetAssemblyHash___STATIC__U4__mscorlibSystemReflectionAssembly( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_Assembly_Instance inst;
    CLR_UINT32               hash;
    CLR_RT_HeapBlock*        hbAsm = stack.Arg0().Dereference();

    if(CLR_RT_ReflectionDef_Index::Convert( *hbAsm, inst ))
    {
        hash = inst.m_assm->ComputeAssemblyHash();
    }
    else
    {
        hash = 0;
    }

    stack.SetResult( hash, DATATYPE_U4 );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_native_Microsoft_SPOT_Reflection::GetAssemblies___STATIC__SZARRAY_mscorlibSystemReflectionAssembly( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock& top    = stack.PushValueAndClear();

#if defined(TINYCLR_APPDOMAINS)
    TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.GetCurrentAppDomain()->GetAssemblies( top ));
#else
    int               count  = 0;
    CLR_RT_HeapBlock* pArray = NULL;

    for(int pass=0; pass<2; pass++)
    {
        TINYCLR_FOREACH_ASSEMBLY(g_CLR_RT_TypeSystem)
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
        TINYCLR_FOREACH_ASSEMBLY_END();

        if(pass == 0)
        {
            TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance( top, count, g_CLR_RT_WellKnownTypes.m_Assembly ));

            pArray = (CLR_RT_HeapBlock*)top.DereferenceArray()->GetFirstElement();
        }
    }
#endif //TINYCLR_APPDOMAINS

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_native_Microsoft_SPOT_Reflection::GetAssemblyInfo___STATIC__BOOLEAN__SZARRAY_U1__MicrosoftSPOTReflectionAssemblyInfo( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Array* array = NULL;
    CLR_RT_Assembly*        assm  = NULL;
    CLR_RECORD_ASSEMBLY*    header;

    array = stack.Arg0().DereferenceArray(); FAULT_ON_NULL(array);

    header = (CLR_RECORD_ASSEMBLY*)array->GetFirstElement();

    if(header->GoodAssembly())
    {
        CLR_RT_HeapBlock* dst = stack.Arg1().Dereference(); FAULT_ON_NULL(dst);

        TINYCLR_CHECK_HRESULT(CLR_RT_Assembly::CreateInstance( header, assm ));

        TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_String::CreateInstance( dst[ Library_spot_native_Microsoft_SPOT_Reflection__AssemblyInfo::FIELD__m_name ], assm->m_szName ));

        {
            CLR_RT_HeapBlock& refs   = dst[ Library_spot_native_Microsoft_SPOT_Reflection__AssemblyInfo::FIELD__m_refs ];
            CLR_UINT32        numRef = assm->m_pTablesSize[ TBL_AssemblyRef ];


            TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance( refs, numRef, g_CLR_RT_WellKnownTypes.m_UInt32 ));

            {
                const CLR_RECORD_ASSEMBLYREF* ar  =              assm->GetAssemblyRef( 0 );
                CLR_UINT32*                   dst = (CLR_UINT32*)refs.DereferenceArray()->GetFirstElement();

                while(numRef--)
                {
                    *dst++ = assm->ComputeAssemblyHash( ar++ );
                }
            }
        }

        dst[ Library_spot_native_Microsoft_SPOT_Reflection__AssemblyInfo::FIELD__m_flags ].SetInteger(                 assm->m_header->flags                  , DATATYPE_U4 );
        dst[ Library_spot_native_Microsoft_SPOT_Reflection__AssemblyInfo::FIELD__m_size  ].SetInteger( ROUNDTOMULTIPLE(assm->m_header->TotalSize(), CLR_INT32), DATATYPE_I4 );
        dst[ Library_spot_native_Microsoft_SPOT_Reflection__AssemblyInfo::FIELD__m_hash  ].SetInteger(                 assm->ComputeAssemblyHash()            , DATATYPE_U4 );

        stack.SetResult_Boolean( true );
    }
    else
    {
        stack.SetResult_Boolean( false );
    }

    TINYCLR_CLEANUP();

    if(assm)
    {
        assm->DestroyInstance();
    }

    TINYCLR_CLEANUP_END();
}

HRESULT Library_spot_native_Microsoft_SPOT_Reflection::GetAssemblyMemoryInfo___STATIC__BOOLEAN__mscorlibSystemReflectionAssembly__MicrosoftSPOTReflectionAssemblyMemoryInfo( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_Assembly_Instance   assm;
    const CLR_RECORD_ASSEMBLY* header = NULL;
    
    TINYCLR_CHECK_HRESULT(Library_corlib_native_System_Reflection_Assembly::GetTypeDescriptor( *stack.Arg0().Dereference(), assm ));

    header = assm.m_assm->m_header;
        
    if(header->GoodAssembly())
    {
        CLR_RT_HeapBlock* dst = stack.Arg1().Dereference(); FAULT_ON_NULL(dst);

        CLR_RT_Assembly::Offsets offsets;
        
        offsets.iBase                 = ROUNDTOMULTIPLE(sizeof(CLR_RT_Assembly)                                                           , CLR_UINT32);
        offsets.iAssemblyRef          = ROUNDTOMULTIPLE(assm.m_assm->m_pTablesSize[ TBL_AssemblyRef ] * sizeof(CLR_RT_AssemblyRef_CrossReference), CLR_UINT32);
        offsets.iTypeRef              = ROUNDTOMULTIPLE(assm.m_assm->m_pTablesSize[ TBL_TypeRef     ] * sizeof(CLR_RT_TypeRef_CrossReference    ), CLR_UINT32);
        offsets.iFieldRef             = ROUNDTOMULTIPLE(assm.m_assm->m_pTablesSize[ TBL_FieldRef    ] * sizeof(CLR_RT_FieldRef_CrossReference   ), CLR_UINT32);
        offsets.iMethodRef            = ROUNDTOMULTIPLE(assm.m_assm->m_pTablesSize[ TBL_MethodRef   ] * sizeof(CLR_RT_MethodRef_CrossReference  ), CLR_UINT32);
        offsets.iTypeDef              = ROUNDTOMULTIPLE(assm.m_assm->m_pTablesSize[ TBL_TypeDef     ] * sizeof(CLR_RT_TypeDef_CrossReference    ), CLR_UINT32);
        offsets.iFieldDef             = ROUNDTOMULTIPLE(assm.m_assm->m_pTablesSize[ TBL_FieldDef    ] * sizeof(CLR_RT_FieldDef_CrossReference   ), CLR_UINT32);
        offsets.iMethodDef            = ROUNDTOMULTIPLE(assm.m_assm->m_pTablesSize[ TBL_MethodDef   ] * sizeof(CLR_RT_MethodDef_CrossReference  ), CLR_UINT32);
#if !defined(TINYCLR_APPDOMAINS)
        offsets.iStaticFields         = ROUNDTOMULTIPLE(assm.m_assm->m_iStaticFields                  * sizeof(CLR_RT_HeapBlock                 ), CLR_UINT32);
#endif
#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
        offsets.iDebuggingInfoMethods = ROUNDTOMULTIPLE(assm.m_assm->m_pTablesSize[ TBL_MethodDef ]    * sizeof(CLR_RT_MethodDef_DebuggingInfo   ), CLR_UINT32);
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
    
        dst[ Library_spot_native_Microsoft_SPOT_Reflection__AssemblyMemoryInfo::FIELD__RamSize                  ].SetInteger(                 iTotalRamSize                                                             , DATATYPE_U4 );
        dst[ Library_spot_native_Microsoft_SPOT_Reflection__AssemblyMemoryInfo::FIELD__RomSize                  ].SetInteger( ROUNDTOMULTIPLE(assm.m_assm->m_header->TotalSize(), CLR_INT32)                            , DATATYPE_I4 );
        dst[ Library_spot_native_Microsoft_SPOT_Reflection__AssemblyMemoryInfo::FIELD__MetadataSize             ].SetInteger(                 iMetaData                                                                 , DATATYPE_U4 );
        dst[ Library_spot_native_Microsoft_SPOT_Reflection__AssemblyMemoryInfo::FIELD__AssemblyRef              ].SetInteger(                 offsets.iAssemblyRef                                                      , DATATYPE_U4 );
        dst[ Library_spot_native_Microsoft_SPOT_Reflection__AssemblyMemoryInfo::FIELD__AssemblyRefElements      ].SetInteger(                 assm.m_assm->m_pTablesSize[ TBL_AssemblyRef    ]                          , DATATYPE_U4 );
        dst[ Library_spot_native_Microsoft_SPOT_Reflection__AssemblyMemoryInfo::FIELD__TypeRef                  ].SetInteger(                 offsets.iTypeRef                                                          , DATATYPE_U4 );
        dst[ Library_spot_native_Microsoft_SPOT_Reflection__AssemblyMemoryInfo::FIELD__TypeRefElements          ].SetInteger(                 assm.m_assm->m_pTablesSize[ TBL_TypeRef        ]                          , DATATYPE_U4 );
        dst[ Library_spot_native_Microsoft_SPOT_Reflection__AssemblyMemoryInfo::FIELD__FieldRef                 ].SetInteger(                 offsets.iFieldRef                                                         , DATATYPE_U4 );
        dst[ Library_spot_native_Microsoft_SPOT_Reflection__AssemblyMemoryInfo::FIELD__FieldRefElements         ].SetInteger(                 assm.m_assm->m_pTablesSize[ TBL_FieldRef       ]                          , DATATYPE_U4 );
        dst[ Library_spot_native_Microsoft_SPOT_Reflection__AssemblyMemoryInfo::FIELD__MethodRef                ].SetInteger(                 offsets.iMethodRef                                                        , DATATYPE_U4 );
        dst[ Library_spot_native_Microsoft_SPOT_Reflection__AssemblyMemoryInfo::FIELD__MethodRefElements        ].SetInteger(                 assm.m_assm->m_pTablesSize[ TBL_MethodRef      ]                          , DATATYPE_U4 );
        dst[ Library_spot_native_Microsoft_SPOT_Reflection__AssemblyMemoryInfo::FIELD__TypeDef                  ].SetInteger(                 offsets.iTypeDef                                                          , DATATYPE_U4 );
        dst[ Library_spot_native_Microsoft_SPOT_Reflection__AssemblyMemoryInfo::FIELD__TypeDefElements          ].SetInteger(                 assm.m_assm->m_pTablesSize[ TBL_TypeDef        ]                          , DATATYPE_U4 );
        dst[ Library_spot_native_Microsoft_SPOT_Reflection__AssemblyMemoryInfo::FIELD__FieldDef                 ].SetInteger(                 offsets.iFieldDef                                                         , DATATYPE_U4 );
        dst[ Library_spot_native_Microsoft_SPOT_Reflection__AssemblyMemoryInfo::FIELD__FieldDefElements         ].SetInteger(                 assm.m_assm->m_pTablesSize[ TBL_FieldDef       ]                          , DATATYPE_U4 );
        dst[ Library_spot_native_Microsoft_SPOT_Reflection__AssemblyMemoryInfo::FIELD__MethodDef                ].SetInteger(                 offsets.iMethodDef                                                        , DATATYPE_U4 );
        dst[ Library_spot_native_Microsoft_SPOT_Reflection__AssemblyMemoryInfo::FIELD__MethodDefElements        ].SetInteger(                 assm.m_assm->m_pTablesSize[ TBL_MethodDef      ]                          , DATATYPE_U4 );
        dst[ Library_spot_native_Microsoft_SPOT_Reflection__AssemblyMemoryInfo::FIELD__StaticFields             ].SetInteger(                 assm.m_assm->m_iStaticFields                                              , DATATYPE_U4 );
        dst[ Library_spot_native_Microsoft_SPOT_Reflection__AssemblyMemoryInfo::FIELD__Attributes               ].SetInteger(                 assm.m_assm->m_pTablesSize[ TBL_Attributes     ] * sizeof(CLR_RECORD_ATTRIBUTE), DATATYPE_U4 );
        dst[ Library_spot_native_Microsoft_SPOT_Reflection__AssemblyMemoryInfo::FIELD__AttributesElements       ].SetInteger(                 assm.m_assm->m_pTablesSize[ TBL_Attributes     ]                               , DATATYPE_U4 );
        dst[ Library_spot_native_Microsoft_SPOT_Reflection__AssemblyMemoryInfo::FIELD__TypeSpec                 ].SetInteger(                 assm.m_assm->m_pTablesSize[ TBL_TypeSpec       ] * sizeof(CLR_RECORD_TYPESPEC ), DATATYPE_U4 );
        dst[ Library_spot_native_Microsoft_SPOT_Reflection__AssemblyMemoryInfo::FIELD__TypeSpecElements         ].SetInteger(                 assm.m_assm->m_pTablesSize[ TBL_TypeSpec       ]                               , DATATYPE_U4 );
        dst[ Library_spot_native_Microsoft_SPOT_Reflection__AssemblyMemoryInfo::FIELD__Resources                ].SetInteger(                 assm.m_assm->m_pTablesSize[ TBL_Resources      ] * sizeof(CLR_RECORD_RESOURCE ), DATATYPE_U4 );
        dst[ Library_spot_native_Microsoft_SPOT_Reflection__AssemblyMemoryInfo::FIELD__ResourcesElements        ].SetInteger(                 assm.m_assm->m_pTablesSize[ TBL_Resources      ]                               , DATATYPE_U4 );
        dst[ Library_spot_native_Microsoft_SPOT_Reflection__AssemblyMemoryInfo::FIELD__ResourcesFiles           ].SetInteger(                 assm.m_assm->m_pTablesSize[ TBL_ResourcesFiles ] * sizeof(CLR_RECORD_RESOURCE ), DATATYPE_U4 );
        dst[ Library_spot_native_Microsoft_SPOT_Reflection__AssemblyMemoryInfo::FIELD__ResourcesFilesElements   ].SetInteger(                 assm.m_assm->m_pTablesSize[ TBL_ResourcesFiles ]                               , DATATYPE_U4 );
        dst[ Library_spot_native_Microsoft_SPOT_Reflection__AssemblyMemoryInfo::FIELD__ResourcesData            ].SetInteger(                 assm.m_assm->m_pTablesSize[ TBL_ResourcesData  ]                               , DATATYPE_U4 );
        dst[ Library_spot_native_Microsoft_SPOT_Reflection__AssemblyMemoryInfo::FIELD__Strings                  ].SetInteger(                 assm.m_assm->m_pTablesSize[ TBL_Strings        ]                               , DATATYPE_U4 );
        dst[ Library_spot_native_Microsoft_SPOT_Reflection__AssemblyMemoryInfo::FIELD__Signatures               ].SetInteger(                 assm.m_assm->m_pTablesSize[ TBL_Signatures     ]                               , DATATYPE_U4 );
        dst[ Library_spot_native_Microsoft_SPOT_Reflection__AssemblyMemoryInfo::FIELD__ByteCode                 ].SetInteger(                 assm.m_assm->m_pTablesSize[ TBL_ByteCode       ]                               , DATATYPE_U4 );
        
        stack.SetResult_Boolean( true );
    }
    else 
    {
        stack.SetResult_Boolean( false );
    }
        
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_native_Microsoft_SPOT_Reflection::GetTypeFromHash___STATIC__mscorlibSystemType__U4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock&    top = stack.PushValueAndClear();
    CLR_RT_TypeDef_Index res;

    if(g_CLR_RT_TypeSystem.FindTypeDef( stack.Arg0().NumericByRefConst().u4, res ))
    {
        CLR_RT_HeapBlock* hbObj;

        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex(top, g_CLR_RT_WellKnownTypes.m_TypeStatic));
        hbObj = top.Dereference();
        
        hbObj->SetReflection( res ); 
    }

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_native_Microsoft_SPOT_Reflection::GetAssemblyFromHash___STATIC__mscorlibSystemReflectionAssembly__U4( CLR_RT_StackFrame& stack )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock& top  = stack.PushValueAndClear();
    CLR_UINT32        hash = stack.Arg0().NumericByRefConst().u4;

    TINYCLR_FOREACH_ASSEMBLY_IN_CURRENT_APPDOMAIN(g_CLR_RT_TypeSystem)
    {
        if(pASSM->ComputeAssemblyHash() == hash)
        {
            CLR_RT_HeapBlock* hbObj;
            CLR_RT_Assembly_Index idx; idx.Set( pASSM->m_idx );
            
            TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex(top, g_CLR_RT_WellKnownTypes.m_Assembly));
            hbObj = top.Dereference();
            
            hbObj->SetReflection( idx ); 

            TINYCLR_SET_AND_LEAVE(S_OK);
        }
    }
    TINYCLR_FOREACH_ASSEMBLY_IN_CURRENT_APPDOMAIN_END();

    TINYCLR_NOCLEANUP();
}
