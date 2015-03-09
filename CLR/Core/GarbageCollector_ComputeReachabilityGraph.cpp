////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Core.h"

//--//

void CLR_RT_GarbageCollector::MarkStack::Initialize( MarkStackElement* ptr, size_t num )
{
    NATIVE_PROFILE_CLR_CORE();
    GenericNode_Initialize();

    m_last      = &ptr[ num-1 ];
    m_top       =  ptr;

    //
    // Empty element to act a sentinel.
    //
    ptr->ptr = NULL;
    ptr->num = 0;
#if defined(TINYCLR_VALIDATE_APPDOMAIN_ISOLATION)
    ptr->appDomain = NULL;
#endif
}

//--//

bool CLR_RT_GarbageCollector::ComputeReachabilityGraphForSingleBlock( CLR_RT_HeapBlock** ptr )
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_HeapBlock* obj = *ptr; if(obj == NULL || obj->IsAlive()) return true;

    return ComputeReachabilityGraphForMultipleBlocks( obj, 1 );
}


bool CLR_RT_GarbageCollector::ComputeReachabilityGraphForMultipleBlocks( CLR_RT_HeapBlock* lst, CLR_UINT32 num )
{
    NATIVE_PROFILE_CLR_CORE();

    MarkStack       * stackList;
    MarkStackElement* stack;
    MarkStackElement* stackLast;

#define COMPUTEREACHABILITY_LOADSTATE() stackLast = g_CLR_RT_GarbageCollector.m_markStack->m_last; stack = g_CLR_RT_GarbageCollector.m_markStack->m_top; stackList = g_CLR_RT_GarbageCollector.m_markStack;
#define COMPUTEREACHABILITY_SAVESTATE() g_CLR_RT_GarbageCollector.m_markStack->m_last = stackLast; g_CLR_RT_GarbageCollector.m_markStack->m_top = stack; g_CLR_RT_GarbageCollector.m_markStack = stackList;

    COMPUTEREACHABILITY_LOADSTATE();

    {
        CLR_RT_HeapBlock* sub = NULL;

        while(true)
        {
            CLR_RT_HeapBlock* ptr = lst;

            if(num == 0)
            {
                if(stack->num == 0)
                {
                    MarkStack* stackNext = (MarkStack*)stackList->Prev();

                    //finished with this MarkStack
                    if(stackNext->Prev() == NULL)
                    {
                        //finished marking
                        break;
                    }
                    else
                    {
                        COMPUTEREACHABILITY_SAVESTATE();
                        g_CLR_RT_GarbageCollector.m_markStack = stackNext;                        
                        COMPUTEREACHABILITY_LOADSTATE();
                    }
                }

                ptr = stack->ptr;

                stack->ptr++;
                stack->num--;

                if(stack->num == 0)
                {
                    stack--;

#if defined(TINYCLR_VALIDATE_APPDOMAIN_ISOLATION)                                 
                    (void)g_CLR_RT_ExecutionEngine.SetCurrentAppDomain( stack->appDomain );
#endif
                }
            }
            else if(num > 1)
            {
                if(stack == stackLast)
                {
                    MarkStack* stackNext = (MarkStack*)stackList->Next();

                    if(stackNext->Next() != NULL)
                    {                        
                        COMPUTEREACHABILITY_SAVESTATE();
                        stackList = stackNext;
                        COMPUTEREACHABILITY_LOADSTATE();
                    }
                    else
                    {                                                           
                        //try to allocate another stack list...
                        stackNext = NULL;

                        //If there was no space for GC last time, don't bother trying to allocate again
                        if(!g_CLR_RT_GarbageCollector.m_fOutOfStackSpaceForGC)
                        {
                            for(int cElement = g_CLR_RT_GarbageCollector.c_minimumSpaceForGC; cElement >= 1; cElement /= 2)
                            {
                                CLR_UINT32 size = sizeof(MarkStack) + sizeof(MarkStackElement) * cElement;

                                stackNext = (MarkStack*)CLR_RT_Memory::Allocate( size, CLR_RT_HeapBlock::HB_SpecialGCAllocation );

                                if(stackNext)
                                {
                                    COMPUTEREACHABILITY_SAVESTATE();

                                    stackNext->Initialize( (MarkStackElement*)(&stackNext[ 1 ]), (size_t)cElement );                            

                                    g_CLR_RT_GarbageCollector.m_markStackList->LinkAtBack( stackNext );
                                    
                                    g_CLR_RT_GarbageCollector.m_markStack = stackNext;
                                    
                                    COMPUTEREACHABILITY_LOADSTATE();

                                    break;
                                }
                            }
                        }

                        if(stackNext == NULL)
                        {
                            //Out of stack support space
                            //Set the failure flag and continue, ignoring lst, num
                            //The mark will complete later via MarkSlow

                            g_CLR_RT_GarbageCollector.m_fOutOfStackSpaceForGC = true;
                                                
                            lst = NULL;
                            num = 0;
                            continue;
                        }
                    }
                }

                stack++;

                stack->ptr = lst+1;
                stack->num = num-1;

#if defined(TINYCLR_VALIDATE_APPDOMAIN_ISOLATION) 
                stack->appDomain = g_CLR_RT_ExecutionEngine.GetCurrentAppDomain();
#endif                                        
            }


            {

                lst = NULL;
                num = 0;
                                        
                CLR_RT_HeapBlock::Debug_CheckPointer( ptr );

                ptr->MarkAlive();

                switch(ptr->DataType())
                {
                case DATATYPE_OBJECT:
                case DATATYPE_BYREF:
                    sub = ptr->Dereference();
                    break;

                //--//

#if defined(TINYCLR_APPDOMAINS)
                case DATATYPE_TRANSPARENT_PROXY:
                    {
                        CLR_RT_AppDomain* appDomain = ptr->TransparentProxyAppDomain();

                        if(appDomain)
                        {
                            if(!appDomain->IsLoaded())
                            {
                                //If the AppDomain is unloading, we no longer need to keep the reference around
                                ptr->SetTransparentProxyReference( NULL, NULL );                                
                            }
                            else
                            {
#if defined(TINYCLR_VALIDATE_APPDOMAIN_ISOLATION) 
                                (void)g_CLR_RT_ExecutionEngine.SetCurrentAppDomain( ptr->TransparentProxyAppDomain() );
#endif
                                sub = ptr->TransparentProxyDereference();
                            }
                        }             
                    }
                    break;
#endif

    //--//

                case DATATYPE_ARRAY_BYREF:
                    sub = (CLR_RT_HeapBlock*)ptr->Array();
                    break;

                //--//

                case DATATYPE_CLASS:
                case DATATYPE_VALUETYPE:
                    //
                    // This is the real object, mark all its fields.
                    //
                    lst = ptr             + 1;
                    num = ptr->DataSize() - 1;
                    break;

                case DATATYPE_SZARRAY:
                    //
                    // If the array is full of reference types, mark each of them.
                    //
                    {
                        CLR_RT_HeapBlock_Array* array = (CLR_RT_HeapBlock_Array*)ptr;

                        if(array->m_fReference)
                        {
                            lst = (CLR_RT_HeapBlock*)array->GetFirstElement();
                            num =                    array->m_numOfElements;
                        }
                    }
                    break;

                case DATATYPE_REFLECTION:
                    break;

                case DATATYPE_DELEGATE_HEAD:
                    {
                        CLR_RT_HeapBlock_Delegate* dlg = (CLR_RT_HeapBlock_Delegate*)ptr;

                        lst = &dlg->m_object;
                        num = 1;
                    }
                    break;

                case DATATYPE_BINARY_BLOB_HEAD:
                    {
                        CLR_RT_HeapBlock_BinaryBlob* blob = (CLR_RT_HeapBlock_BinaryBlob*)ptr;

                        _ASSERTE(blob->BinaryBlobMarkingHandler() == NULL);
                    }
                    break;

                case DATATYPE_DELEGATELIST_HEAD:
                    {
                        CLR_RT_HeapBlock_Delegate_List* dlgList = (CLR_RT_HeapBlock_Delegate_List*)ptr;

                        if(dlgList->m_flags & CLR_RT_HeapBlock_Delegate_List::c_Weak)
                        {
                            dlgList->ClearData();

                            g_CLR_RT_GarbageCollector.m_weakDelegates_Reachable.LinkAtBack( dlgList );
                        }
                        else
                        {
                            lst = dlgList->GetDelegates();
                            num = dlgList->m_length;
                        }
                    }
                    break;
                }
                      
                if(sub)
                {
                    if(sub->IsAlive() == false)
                    {
                        lst = sub;
                        num = 1;
                    }

                    sub = NULL;
                }

            }
        }
    }

    COMPUTEREACHABILITY_SAVESTATE();

    return true;
}

//--//

