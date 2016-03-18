#pragma once
////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use these files
// except in compliance with the License.
// You may obtain a copy of the License at:
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software distributed under the
// License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
// either express or implied. See the License for the specific language governing permissions
// and limitations under the License.
// 
////////////////////////////////////////////////////////////////////////////////////////////////
#include <windows.h>

namespace Microsoft
{
    namespace Win32
    {
        // template class to manage Win32 handles in RAII pattern implementations
        // Type Params:
        //        Handle_t - actual type for the HANDLE
        //    HandleTraits - traits template containing methods for
        //                   manipulating handles of type Handle_t
        //
        template<typename Handle_t, typename HandleTraits>
        class Win32Handle
        {
        public:

            Win32Handle( Handle_t handle = HandleTraits::DefaultInvalidValue( ) )
                : Handle( handle )
            { }

            ~Win32Handle( )
            {
                Close( );
            }

            // copy, assign and move not supported
            // perhaps they can use DuplicateHandle
            // but that's not supported by all handle
            // types so would need support from the
            // traits
            Win32Handle( const Win32Handle& ) = delete;
            Win32Handle& operator=( const Win32Handle& ) = delete;

            Win32Handle& operator=( HANDLE other )
            {
                InternalAssign( other );
                return *this;
            }

            bool IsValid( )
            {
                return HandleTraits::ValidateHandle( Handle );
            }

            bool Close( )
            {
                return InternalAssign( HandleTraits::DefaultInvalidValue( ) );
            }

            explicit operator Handle_t( )
            {
                return Handle;
            }

        protected:
            bool InternalAssign( Handle_t handle )
            {
                auto oldHandle = Handle.exchange( handle );
                if( !HandleTraits::ValidateHandle( oldHandle ) )
                    return true;

                return HandleTraits::ReleaseHandle( oldHandle );
            }

        private:
            std::atomic<Handle_t> Handle;
        };

        template<typename Handle_t>
        inline bool ValidateHandle( Handle_t h )
        {
            return h != ( Handle_t )INVALID_HANDLE_VALUE;
        }

        template<typename Handle_t>
        inline bool ValidateHandleNotNull( Handle_t h )
        {
            return h != nullptr;
        }

        template<typename Handle_t>
        inline bool ValidateHandleNotNullOrInvalid( Handle_t h )
        {
            return ValidateHandleNotNull( h ) && ValidateHandle( h );
        }

        struct StdNullInvalidHandleTraits
        {
            static bool ReleaseHandle( HANDLE h )
            {
                return !!CloseHandle( h );
            }

            static bool ValidateHandle( HANDLE h )
            {
                return ValidateHandleNotNullOrInvalid( h );
            }

            static HANDLE DefaultInvalidValue( )
            {
                return INVALID_HANDLE_VALUE;
            }
        };
    }
}