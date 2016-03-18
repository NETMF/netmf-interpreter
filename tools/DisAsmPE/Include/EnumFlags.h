#ifndef _ENUM_FLAGS_H_
#define _ENUM_FLAGS_H_
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

#include <type_traits>

/////////////////////////////////////////////////////////////////////////////////////////////
// Description:
//    Creates operators for bitwise operations on an enum intended to be
//    used as a set of flags.
//
// Input:
//    T - Enumeration type to create the operators for
//
// Remarks:
//    This uses "old school template" preprocessor macros as doing this in real templates
//    is just a major PITA or unreadable gunk. ( The challenge with templates, in this case,
//    is specializing a "traits" template to indicate the enum is intended as a set of flags.
//    Doing so across namespaces is very problematic or just plain ugly. )
//
#define ENUM_FLAGS( T )\
        static_assert( std::is_enum<T>::value, "ENUM_FLAGS Requires an enumeration type!" );\
        constexpr T operator~( T r )\
        {\
            typedef std::underlying_type<T>::type enum_t;\
            return static_cast< T >( ~static_cast< enum_t >( r ) );\
        }\
        inline T& operator|=( T& a, T b )\
        {\
            a = static_cast< T >( static_cast< std::underlying_type< T >::type>( a ) | static_cast< std::underlying_type< T >::type>( b ) );\
            return a;\
        }\
        constexpr T operator&( T l, T r )\
        {\
            typedef std::underlying_type<T>::type enum_t;\
            return static_cast< T >( static_cast< enum_t >( l ) & static_cast< enum_t >( r ) );\
        }\
        inline T& operator&=( T& a, T b )\
        {\
            a = static_cast< T >( static_cast< std::underlying_type< T >::type>( a ) & static_cast< std::underlying_type< T >::type>( b ) );\
            return a;\
        }\
        constexpr T operator|( T l, T r )\
        {\
            typedef std::underlying_type<T>::type enum_t;\
            return static_cast< T >( static_cast< enum_t >( l ) | static_cast< enum_t >( r ) );\
        }\

namespace Microsoft
{
    namespace Utilities
    {
        struct EnumFlags
        {
            // Tests a given enumeration instance for all flags provided.
            // Returns true if all flags are set in value
            template<typename T>
            static typename std::enable_if<std::is_enum<T>::value, bool>::type
            HasFlags( T value, T flag )
            {
                return flag == ( value & flag );
            }
        };
    }
}
#endif
