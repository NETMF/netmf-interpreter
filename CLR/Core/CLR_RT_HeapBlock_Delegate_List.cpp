////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Core.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT CLR_RT_HeapBlock_Delegate_List::CreateInstance( CLR_RT_HeapBlock_Delegate_List*& list, CLR_UINT32 length )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_UINT32 totLength = (CLR_UINT32)(sizeof(CLR_RT_HeapBlock_Delegate_List) + length * sizeof(CLR_RT_HeapBlock));

    list = (CLR_RT_HeapBlock_Delegate_List*)g_CLR_RT_ExecutionEngine.ExtractHeapBytesForObjects( DATATYPE_DELEGATELIST_HEAD, 0, totLength ); CHECK_ALLOCATION(list);

    list->ClearData();
    list->m_cls.Clear();
    list->m_length = length;
    list->m_flags  = 0;

    TINYCLR_NOCLEANUP();
}

CLR_RT_HeapBlock* CLR_RT_HeapBlock_Delegate_List::CopyAndCompress( CLR_RT_HeapBlock* src, CLR_RT_HeapBlock* dst, CLR_UINT32 num )
{
    NATIVE_PROFILE_CLR_CORE();
    while(num--)
    {
        CLR_RT_HeapBlock_Delegate* dlg = src->DereferenceDelegate();
        if(dlg)
        {
            dst->SetObjectReference( dlg );

            dst++;
        }
        else
        {
            m_length--;
        }

        src++;
    }

    return dst;
}

HRESULT CLR_RT_HeapBlock_Delegate_List::Change( CLR_RT_HeapBlock& reference, CLR_RT_HeapBlock& delegateSrc, CLR_RT_HeapBlock& delegateTarget, bool fCombine, bool fWeak )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_Delegate_List* dlgListSrc;
    CLR_RT_HeapBlock_Delegate_List* dlgListDst;
    CLR_RT_HeapBlock_Delegate*      dlg;
    CLR_RT_HeapBlock*               newDlgs;
    CLR_RT_HeapBlock*               oldDlgs;
    CLR_UINT32                      oldNum;
    CLR_UINT32                      newNum;

    CLR_UINT32 num = 0;

    reference.SetObjectReference( NULL );

    if(delegateSrc   .DataType() != DATATYPE_OBJECT ||
       delegateTarget.DataType() != DATATYPE_OBJECT  )
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
    }

    dlg = delegateTarget.DereferenceDelegate();

    if(dlg == NULL)
    {
        reference.SetObjectReference( delegateSrc.DereferenceDelegate() );
        TINYCLR_SET_AND_LEAVE(S_OK);
    }

    if(dlg->DataType() == DATATYPE_DELEGATELIST_HEAD)
    {
        CLR_RT_HeapBlock     intermediate; intermediate.Assign( delegateSrc );
        CLR_RT_ProtectFromGC gc( intermediate );

        dlgListDst = (CLR_RT_HeapBlock_Delegate_List*)dlg;
        newDlgs    = dlgListDst->GetDelegates();

        for(num=0; num<dlgListDst->m_length; num++, newDlgs++)
        {
            if(newDlgs->DataType() == DATATYPE_OBJECT && newDlgs->DereferenceDelegate() != NULL) // The delegate could have been GC'ed.
            {
                TINYCLR_CHECK_HRESULT(Change( reference, intermediate, *newDlgs, fCombine, fWeak ));

                intermediate.Assign( reference );
            }
        }
    }
    else
    {
        dlgListSrc = delegateSrc.DereferenceDelegateList();
        if(dlgListSrc == NULL)
        {
            oldDlgs = NULL;
            oldNum  = 0;
        }
        else
        {
            switch(dlgListSrc->DataType())
            {
            case DATATYPE_DELEGATE_HEAD:
                oldDlgs = &delegateSrc;
                oldNum  = 1;
                break;

            case DATATYPE_DELEGATELIST_HEAD:
                oldDlgs = dlgListSrc->GetDelegates();
                oldNum  = dlgListSrc->m_length;
                break;

            default:
                TINYCLR_SET_AND_LEAVE(CLR_E_WRONG_TYPE);
            }
        }

        if(fCombine)
        {
            if(oldNum == 0 && fWeak == false)
            {
                //
                // Empty input list, copy the delegate.
                //
                reference.Assign( delegateTarget );
                TINYCLR_SET_AND_LEAVE(S_OK);
            }

            //--//

            newNum = oldNum + 1;
        }
        else
        {
            for(num=0, newDlgs=oldDlgs; num<oldNum; num++, newDlgs++)
            {
                CLR_RT_HeapBlock_Delegate* ptr = newDlgs->DereferenceDelegate();
                if(ptr)
                {
                    if( ptr->DelegateFtn().m_data   == dlg->DelegateFtn().m_data   &&
                        ptr->m_object.Dereference() == dlg->m_object.Dereference()  )
                    {
                        break;
                    }
                }
            }

            if(num == oldNum)
            {
                reference.Assign( delegateSrc ); // Nothing to remove.
                TINYCLR_SET_AND_LEAVE(S_OK);
            }

            if(oldNum == 2 && (dlgListSrc->m_flags & CLR_RT_HeapBlock_Delegate_List::c_Weak) == 0)
            {
                reference.Assign( oldDlgs[ 1-num ] ); // Convert from a list to delegate.
                TINYCLR_SET_AND_LEAVE(S_OK);
            }

            if(oldNum == 1)
            {
                reference.SetObjectReference( NULL ); // Oops, empty delegate...
                TINYCLR_SET_AND_LEAVE(S_OK);
            }

            //--//

            newNum = oldNum - 1;
        }

        TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Delegate_List::CreateInstance( dlgListDst, newNum ));

        dlgListDst->m_cls = dlg->m_cls;

        newDlgs = dlgListDst->GetDelegates();

        if(fCombine)
        {
            newDlgs = dlgListDst->CopyAndCompress( oldDlgs, newDlgs, oldNum );

            newDlgs->Assign( delegateTarget );
        }
        else
        {
            newDlgs = dlgListDst->CopyAndCompress( oldDlgs      , newDlgs,          num++ );
            newDlgs = dlgListDst->CopyAndCompress( oldDlgs + num, newDlgs, oldNum - num   );
        }

        dlgListDst->m_flags = (dlgListSrc && oldNum > 1) ? dlgListSrc->m_flags : 0;

        if(fWeak) dlgListDst->m_flags |= CLR_RT_HeapBlock_Delegate_List::c_Weak;

        reference.SetObjectReference( dlgListDst );
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock_Delegate_List::Combine( CLR_RT_HeapBlock& reference, CLR_RT_HeapBlock& delegateSrc, CLR_RT_HeapBlock& delegateNew, bool fWeak )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(Change( reference, delegateSrc, delegateNew, true, fWeak ));

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock_Delegate_List::Remove( CLR_RT_HeapBlock& reference, CLR_RT_HeapBlock& delegateSrc, CLR_RT_HeapBlock& delegateOld )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(Change( reference, delegateSrc, delegateOld, false, false ));

    TINYCLR_NOCLEANUP();
}

void CLR_RT_HeapBlock_Delegate_List::Relocate()
{
    NATIVE_PROFILE_CLR_CORE();
    CLR_RT_GarbageCollector::Heap_Relocate( GetDelegates(), m_length );
}
