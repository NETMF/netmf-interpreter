#ifndef _UNALIGNED_ACCESS_H_
#define _UNALIGNED_ACCESS_H_
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

#include <cstdint>

namespace Microsoft
{
    namespace Utilities
    {
        inline uint16_t ReadUnalignedUInt16( uint8_t const*& ip) 
        {
            uint16_t retVal;
        #if !defined(NETMF_TARGET_BIG_ENDIAN)
            retVal  = (uint16_t)(*(const uint8_t *)ip);
            ip += sizeof(uint8_t);
            retVal |= (uint16_t)(*(const uint8_t *)ip) << 8;
            ip += sizeof(uint8_t);
        #else
            retVal  = (uint16_t)(*(const uint8_t *)ip) << 8;
            ip += sizeof(uint8_t);
            retVal |= (uint16_t)(*(const uint8_t *)ip);
            ip += sizeof(uint8_t);
        #endif
            return retVal;
        }

        inline uint32_t ReadUnalignedUInt32( uint8_t const*& ip) 
        {
            uint32_t retVal;
        #if !defined(NETMF_TARGET_BIG_ENDIAN)
            retVal  = (uint32_t)(*(const uint8_t *)ip);
            ip += sizeof(uint8_t);
            retVal |= (uint32_t)(*(const uint8_t *)ip) << 8;
            ip += sizeof(uint8_t);
            retVal |= (uint32_t)(*(const uint8_t *)ip) << 16;
            ip += sizeof(uint8_t);
            retVal |= (uint32_t)(*(const uint8_t *)ip) << 24;
            ip += sizeof(uint8_t);
        #else
            retVal = (uint32_t)(*(const uint8_t *)ip) << 24;
            ip += sizeof(uint8_t);
            retVal |= (uint32_t)(*(const uint8_t *)ip) << 16;
            ip += sizeof(uint8_t);
            retVal |= (uint32_t)(*(const uint8_t *)ip) << 8;
            ip += sizeof(uint8_t);
            retVal |= (uint32_t)(*(const uint8_t *)ip);
            ip += sizeof(uint8_t);
        #endif //NETMF_TARGET_BIG_ENDIAN
            return retVal;
        }

        inline uint64_t ReadUnalignedUInt64( uint8_t const*& ip) 
        {
            uint64_t retVal;
        #if !defined(NETMF_TARGET_BIG_ENDIAN)
            retVal  = (uint64_t)(*(const uint8_t *)ip);
            ip += sizeof(uint8_t);
            retVal |= (uint64_t)(*(const uint8_t *)ip) << 8;
            ip += sizeof(uint8_t);
            retVal |= (uint64_t)(*(const uint8_t *)ip) << 16;
            ip += sizeof(uint8_t);
            retVal |= (uint64_t)(*(const uint8_t *)ip) << 24;
            ip += sizeof(uint8_t);
            retVal |= (uint64_t)(*(const uint8_t *)ip) << 32;
            ip += sizeof(uint8_t);
            retVal |= (uint64_t)(*(const uint8_t *)ip) << 40;
            ip += sizeof(uint8_t);
            retVal |= (uint64_t)(*(const uint8_t *)ip) << 48;
            ip += sizeof(uint8_t);
            retVal |= (uint64_t)(*(const uint8_t *)ip) << 56;
            ip += sizeof(uint8_t);
        #else
            retVal = (uint64_t)(*(const uint8_t *)ip) << 56;
            ip += sizeof(uint8_t);
            retVal |= (uint64_t)(*(const uint8_t *)ip) << 48;
            ip += sizeof(uint8_t);
            retVal |= (uint64_t)(*(const uint8_t *)ip) << 40;
            ip += sizeof(uint8_t);
            retVal |= (uint64_t)(*(const uint8_t *)ip) << 32;
            ip += sizeof(uint8_t);
            retVal |= (uint64_t)(*(const uint8_t *)ip) << 24;
            ip += sizeof(uint8_t);
            retVal |= (uint64_t)(*(const uint8_t *)ip) << 16;
            ip += sizeof(uint8_t);
            retVal |= (uint64_t)(*(const uint8_t *)ip) << 8;
            ip += sizeof(uint8_t);
            retVal |= (uint64_t)(*(const uint8_t *)ip);
            ip += sizeof(uint8_t);
        #endif //NETMF_TARGET_BIG_ENDIAN
            return retVal;
        }

        inline int16_t ReadUnalignedInt16( uint8_t const*& ip) 
        {
            return static_cast< int16_t >( ReadUnalignedUInt16( ip ) );
        }

        inline int32_t ReadUnalignedInt32( uint8_t const*& ip) 
        {
            return static_cast< int32_t >( ReadUnalignedUInt32( ip ) );
        }

        inline int64_t ReadUnalignedInt64( uint8_t const*& ip) 
        {
            return static_cast< int64_t >( ReadUnalignedUInt64( ip ) );
        }
    }
}
#endif