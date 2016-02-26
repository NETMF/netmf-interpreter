////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Microsoft .NET Micro Framework Porting Kit Code Samples and is unsupported. 
// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use these files except in compliance with the License.
// You may obtain a copy of the License at:
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing
// permissions and limitations under the License.
// 
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#include <pcap.h>

namespace WinPCAP
{
    // template class for a forward only iterator using 
    // singly linked list nodes. Type 'T' must have a
    // field named 'next' that is a pointer to the next
    // node in the list.
    template<typename T>
    class SListForwardIterator
    {
    public:
        typedef SListForwardIterator<T> _My_t;
        SListForwardIterator( T* firstNode )
            :p( firstNode )
        {
        }
        
        SListForwardIterator( const _My_t& other ) = delete;
        _My_t& operator=( const _My_t& other ) = delete;

        SListForwardIterator( const _My_t&& other )
        {
            swap( p, other)
        }

        T* operator*( ) const
        {
            return p;
        }

        _My_t& operator++( )
        {
            p = p->next;
            return *this;
        }

        _My_t& operator++( int )
        {
            auto old = _My_t( p );
            operator++();
            return old;
        }

        bool operator==( const _My_t& other ) const
        {
            return p == other.p;
        }

        bool operator!=( const _My_t& other ) const
        {
            return p != other.p;
        }

    private:
        T* p;
    };

    // class to wrap the WinPCAP APIs for enumerating
    // the available adapter interface devices in a
    // manner generally consistent with patterns used
    // in the C++ Standard Library
    class DeviceList
    {
    public:
        typedef SListForwardIterator<pcap_if_t> forward_iterator;
        DeviceList( )
        {
            ScanForDevices( );
        }

        ~DeviceList( )
        {
            FreeList( );
        }

        // Start a scan for devices
        // return:
        //   Winpcap error code
        int ScanForDevices( )
        {
            FreeList( );
            LastError = pcap_findalldevs( &pAllDevices, ErrBuf );
            return LastError;
        }

        forward_iterator begin( )
        {
            return forward_iterator( pAllDevices );
        }

        forward_iterator end( )
        {
            return forward_iterator( nullptr );
        }
        
        // release all resources held by the list
        void FreeList( )
        {
            if( pAllDevices != nullptr )
            {
                pcap_freealldevs( pAllDevices );
                pAllDevices = nullptr;
            }
        }

        bool empty()
        {
            return pAllDevices == nullptr;
        }

    private:
        int LastError;
        char ErrBuf[ PCAP_ERRBUF_SIZE ];
        pcap_if_t* pAllDevices = nullptr;
    };
}
