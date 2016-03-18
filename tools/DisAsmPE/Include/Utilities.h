#ifndef _UTILITIES_H_
#define _UTILITIES_H_
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

#include <cstddef>
#include <cstdint>

namespace NETMF
{
    template< std::size_t n >
    constexpr std::size_t max_strlen( char[ n ] ) { return n; }

    //template<typename AlignT, typename T>
    //constexpr T* AlignToSizeOf( T* pData )
    //{
    //    typedef typename std::remove_const<T>::T baseT;
    //    auto p = reinterpret_cast< uint8_t* >( const_cast< baseT* >( pData ) );
    //    return reinterpret_cast<T*>( ( p + sizeof( T ) - 1 ) & ~( sizeof( T ) - 1 ) );
    //}


    //////////////////////////////////////////////////////////////////
    // Description:
    //    Computes the CRC32 value using the ZModem, IEEE 802 protocol
    //
    // Input:
    //            pBlock - The memory block to compute a checksum for
    //            length - The length of the block
    //               crc - [default: 0] Initial seed CRC value
    //       embeddedCRC - [default: 0] Offset of embeddedCRC to be treated as 0 value
    //    embeddedCRCLen - [default: 0] Length of embeddedCRC to be treated as 0 value
    // 
    // Remarks:
    //    This method computes a 32 bit CRC value for a block of memory. 
    //    In cases where this is checking a block of Read-Only memory
    //    that contains the claimed CRC but the CRC must be computed 
    //    assuming that field is 0 the embeddedCRC and embeddedCRCLen
    //    are used to indicate what byte range to treat as 0.
    extern std::uint32_t ComputeCRC( const void* pBlock
                                   , std::size_t length
                                   , std::size_t crc = 0
                                   , size_t embeddedCRC = 0
                                   , size_t embeddedCrcLen = 0
                                   );
}
#endif

