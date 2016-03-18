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
// Description:
//    Provides generalized C++ output stream for ::OutputDebugString()
//    that will conditionally compile to a NOP in release builds

#include <sstream>
#include <winbase.h>

namespace Microsoft
{
    namespace Win32
    {
#ifdef DEBUG
        ////////////////////////////////////////////////////
        // Description:
        //    C++ stream buffer for Win32 OutputDebugString()
        //
        class OutputDebugStreamBuffer
            : public std::wstringbuf
        {
        public:
            OutputDebugStreamBuffer( )
                : std::wstringbuf( std::ios_base::out )
            { }

        protected:
            virtual int sync( ) override
            {
                ::OutputDebugString( std::wstringbuf::str( ).c_str( ) );
                str( L"" );
                return 0;
            }
        };

        ///////////////////////////////////////////////////////////////
        // Description:
        //    C++ stream class for Win32 OutputDebugString()
        //
        class OutputDebugStream : public std::wostream
        {
            OutputDebugStreamBuffer Buf;

        public:
            OutputDebugStream( )
                : std::wostream( &Buf, false )
            {
                clear( );
            }
        };
#else
        //////////////////////////////////////////////////////////
        // Description:
        //   NOP stream for debug information in a release build
        //
        // Remarks:
        //   The compiler is smart enough to completely eliminate 
        //   any use of "instances" of this stream so it is safe
        //   to use in retail builds
        class OutputDebugStream
        {
        public:
            // eat simple manipulators, beats std:operator<< in lookups to resolve to a NOP
            OutputDebugStream& operator<<( std::wostream& ( __cdecl *_Pfn )( std::wostream& ) )
            {
                return *this;
            }

            // ignore all data types
            template<typename T>
            OutputDebugStream& operator<<( T val )
            {
                return *this; /* NOP */
            }
        };

#endif
        extern OutputDebugStream cdbg;
    }
}

