////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "..\Core\Core.h"

#include <TinyCLR_Jitter.h>

////////////////////////////////////////////////////////////////////////////////////////////////////

#if defined(TINYCLR_JITTER)

////////////////////////////////////////////////////////////////////////////////////////////////////

void TypedArrayGeneric::Initialize()
{
    m_ptr = NULL;
    m_num = 0;
}

void TypedArrayGeneric::Release()
{
    if(m_ptr)
    {
        CLR_RT_Memory::Release( m_ptr );

        m_ptr = NULL;
        m_num = 0;
    }
}

HRESULT TypedArrayGeneric::Allocate( size_t num, size_t len )
{
    TINYCLR_HEADER();

    if(m_ptr) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);

    if(num)
    {
        m_ptr = CLR_RT_Memory::Allocate_And_Erase( num * len ); CHECK_ALLOCATION(m_ptr);
        m_num = num;
    }

    TINYCLR_NOCLEANUP();
}

//////////////////////////////////////////////////

void TypedQueueGeneric::Initialize( size_t size )
{
    m_ptr  = NULL;
    m_size = size;
    m_num  = 0;
    m_pos  = 0;
}

void TypedQueueGeneric::Release()
{
    if(m_ptr)
    {
        CLR_RT_Memory::Release( m_ptr );

        m_ptr = NULL;
        m_num = 0;
        m_pos = 0;
    }
}

void* TypedQueueGeneric::Reserve( size_t reserve )
{
    if(reserve > m_num)
    {
        size_t newNum = m_num ? m_num * 3 / 2 : c_default; if(newNum < reserve) newNum = reserve;
        void*  ptr    = CLR_RT_Memory::Allocate( newNum * m_size ); if(!ptr) return NULL;

        if(m_ptr)
        {
            memcpy( ptr, m_ptr, m_num * m_size );

            CLR_RT_Memory::Release( m_ptr );
        }

        m_ptr = ptr;
        m_num = newNum;
    }

    return m_ptr;
}

void TypedQueueGeneric::Clear()
{
    m_pos = 0;
}

void* TypedQueueGeneric::Insert( size_t pos )
{
    if(pos <= m_pos)
    {
        if(Reserve( m_pos + 1 ))
        {
            void* ptrRes = GetElement( pos );

            if(pos < m_pos)
            {
                memmove( GetElement( pos+1 ), ptrRes, m_size * (m_pos - pos) );
            }

            m_pos++;

            return ptrRes;
        }
    }

    return NULL;
}

bool TypedQueueGeneric::Remove( size_t pos, void* ptr )
{
    if(pos >= m_pos) return false;

    m_pos--;

    if(ptr)
    {
        memcpy( ptr, GetElement( pos ), m_size );
    }

    if(pos < m_pos)
    {
        memmove( GetElement( pos ), GetElement( pos+1 ), m_size * (m_pos - pos) );
    }

    return true;
}

void* TypedQueueGeneric::Top()
{
    return m_pos ? GetElement( m_pos - 1) : NULL;
}

void* TypedQueueGeneric::GetElement( size_t pos ) const
{
    return m_ptr ? (CLR_UINT8*)m_ptr + m_size * pos : NULL;
}

////////////////////////////////////////////////////////////////////////////////////////////////////

#endif
