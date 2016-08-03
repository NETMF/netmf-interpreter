////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _TINYCLR_RUNTIME__HEAPBLOCK_H_
#define _TINYCLR_RUNTIME__HEAPBLOCK_H_

#ifdef __arm__
// ARM compiler does not allow anonymous structs by default
#pragma anon_unions
#endif

////////////////////////////////////////////////////////////////////////////////////////////////////
#if !defined(NETMF_TARGET_BIG_ENDIAN)

#define CLR_RT_HEAPBLOCK_RAW_ID( dataType, flags, size ) ( (dataType & 0x000000FF) | ((flags & 0x000000FF) << 8) | ((size & 0x0000FFFF) << 16))
#define CLR_RT_HEAPBLOCK_ASSIGN_INTEGER32_SIGNED( dataType, num ) { m_id.raw = CLR_RT_HEAPBLOCK_RAW_ID( dataType, 0, 1 ); m_data.numeric.s4 = (CLR_INT32)num; }
#define CLR_RT_HEAPBLOCK_ASSIGN_INTEGER64_SIGNED( dataType, num ) { m_id.raw = CLR_RT_HEAPBLOCK_RAW_ID( dataType, 0, 1 ); m_data.numeric.s8 =            num; }

#define CLR_RT_HEAPBLOCK_ASSIGN_INTEGER32_UNSIGNED(dataType,num) { m_id.raw = CLR_RT_HEAPBLOCK_RAW_ID( dataType, 0, 1 ); m_data.numeric.u4 = (CLR_UINT32)num; }
#define CLR_RT_HEAPBLOCK_ASSIGN_INTEGER64_UNSIGNED(dataType,num) { m_id.raw = CLR_RT_HEAPBLOCK_RAW_ID( dataType, 0, 1 ); m_data.numeric.u8 =             num; }

#define CLR_RT_HEAPBLOCK_ASSIGN_FLOAT32(dataType,num) { m_id.raw = CLR_RT_HEAPBLOCK_RAW_ID( dataType, 0, 1 ); m_data.numeric.r4 = num; }
#define CLR_RT_HEAPBLOCK_ASSIGN_FLOAT64(dataType,num) { m_id.raw = CLR_RT_HEAPBLOCK_RAW_ID( dataType, 0, 1 ); m_data.numeric.r8 = num; }

#else // NETMF_TARGET_BIG_ENDIAN

#define CLR_RT_HEAPBLOCK_RAW_ID( dataType, flags, size ) ( (size & 0x0000FFFF) | ((flags & 0x000000FF) << 16) | ((dataType & 0x000000FF) << 24))
#define CLR_RT_HEAPBLOCK_ASSIGN_INTEGER32_SIGNED( dataType, num ) { m_id.raw = CLR_RT_HEAPBLOCK_RAW_ID( dataType, 0, 1 );   m_data.numeric.padS4 = 0;  m_data.numeric.s4 = (CLR_INT32)num; }
#define CLR_RT_HEAPBLOCK_ASSIGN_INTEGER64_SIGNED( dataType, num ) { m_id.raw = CLR_RT_HEAPBLOCK_RAW_ID( dataType, 0, 1 );                              m_data.numeric.s8 =            num; }

#define CLR_RT_HEAPBLOCK_ASSIGN_INTEGER32_UNSIGNED( dataType, num ) { m_id.raw = CLR_RT_HEAPBLOCK_RAW_ID( dataType, 0, 1 ); m_data.numeric.padU4 = 0; m_data.numeric.u4 = (CLR_UINT32)num; }
#define CLR_RT_HEAPBLOCK_ASSIGN_INTEGER64_UNSIGNED( dataType, num ) { m_id.raw = CLR_RT_HEAPBLOCK_RAW_ID( dataType, 0, 1 );                           m_data.numeric.u8 =             num; }

#define CLR_RT_HEAPBLOCK_ASSIGN_FLOAT32( dataType, num ) { m_id.raw = CLR_RT_HEAPBLOCK_RAW_ID( dataType, 0, 1 );            m_data.numeric.padR4 = 0; m_data.numeric.r4 =             num; }
#define CLR_RT_HEAPBLOCK_ASSIGN_FLOAT64( dataType, num ) { m_id.raw = CLR_RT_HEAPBLOCK_RAW_ID( dataType, 0, 1 );                                      m_data.numeric.r8 =             num; }

#endif // NETMF_TARGET_BIG_ENDIAN

////////////////////////////////////////////////////////////

#define CLR_RT_HEAPBLOCK_RELOCATE(ptr)                                                  \
{                                                                                       \
    CLR_DataType dt = ptr->DataType();                                                  \
                                                                                        \
    if(dt > DATATYPE_LAST_NONPOINTER && dt < DATATYPE_FIRST_INVALID)                    \
    {                                                                                   \
        CLR_RT_HeapBlockRelocate rel = c_CLR_RT_DataTypeLookup[ dt ].m_relocate;        \
        if(rel)                                                                         \
        {                                                                               \
            (ptr->*rel)();                                                              \
        }                                                                               \
    }                                                                                   \
}


////////////////////////////////////////////////////////////

//
// This is used in memory move operations.
//
struct CLR_RT_HeapBlock_Raw
{
    CLR_UINT32 data[ 3 ];
};

struct CLR_RT_HeapBlock
{
    friend struct CLR_RT_HeapBlock_Node;
    friend struct CLR_RT_DblLinkedList;

    friend struct MethodCompiler;
    //--//

#if defined(TINYCLR_EMULATED_FLOATINGPOINT)
/*********************************************************/
// Keep  in Sync with tinyhal.h 
//#define HAL_FLOAT_SHIFT          10
//#define HAL_FLOAT_PRECISION      1000
//#define HAL_DOUBLE_SHIFT          16
//#define HAL_DOUBLE_PRECISION    10000
/*********************************************************/

    static const int        HB_FloatShift  = 10;
    static const int        HB_DoubleShift = 16;

    static const CLR_INT32  HB_FloatUnit   = (1 << HB_FloatShift );
    static const CLR_INT64  HB_DoubleUnit  = (ULONGLONGCONSTANT(1) << HB_DoubleShift);

    static const CLR_INT32  HB_FloatMask   = ((1 << HB_FloatShift) - 1);
    static const CLR_INT64  HB_DoubleMask  = ((ULONGLONGCONSTANT(1) << HB_DoubleShift) - 1);

#endif

    

    static const CLR_UINT32 HB_Alive            = 0x01;
    static const CLR_UINT32 HB_KeepAlive        = 0x02;
    static const CLR_UINT32 HB_Event            = 0x04;
    static const CLR_UINT32 HB_Pinned           = 0x08;
    static const CLR_UINT32 HB_Boxed            = 0x10;
    static const CLR_UINT32 HB_Unused20         = 0x20;
    //If more bits are needed, HB_Signaled and HB_SignalAutoReset can be freed for use with a little work.
    //It is not necessary that any heapblock can be waited upon.  Currently, only Threads (Thread.Join), 
    //ManualResetEvent, and AutoResetEvent are waitable objects.
    static const CLR_UINT32 HB_Signaled         = 0x40;
    static const CLR_UINT32 HB_SignalAutoReset  = 0x80;

    static const CLR_UINT32 HB_Unmovable        = HB_Pinned | HB_Event;

    //
    // These are special flags used to control allocation.
    //
    static const CLR_UINT32 HB_InitializeToZero       = 0x00100;
    static const CLR_UINT32 HB_NoGcOnFailure          = 0x00200;
    static const CLR_UINT32 HB_SpecialGCAllocation    = 0x00400;
    static const CLR_UINT32 HB_CompactOnFailure       = 0x00800;

    static const CLR_UINT32 HB_NoGcOnFailedAllocation = HB_SpecialGCAllocation | HB_NoGcOnFailure;
    static const CLR_UINT32 HB_MaxSize                = 0x0000FFFF;

    static const CLR_UINT32 HB_Object_Fields_Offset = 1;

    //--//

private:

    union CLR_RT_HeapBlock_Id
    {
        struct Type
        {
            CLR_UINT8  dataType;    // CLR_DataType
            CLR_UINT8  flags;       // HB_*
            CLR_UINT16 size;
        } type;

        CLR_UINT32 raw;
    } m_id;

    union CLR_RT_HeapBlock_AtomicData
    {
        struct NodeLink
        {
            CLR_RT_HeapBlock_Node* nextBlock;
            CLR_RT_HeapBlock_Node* prevBlock;
        } nodeLink;
        
        union Numeric
        {
#if !defined(NETMF_TARGET_BIG_ENDIAN)
            CLR_UINT8  u1;
            CLR_UINT16 u2;
            CLR_UINT32 u4;            
#else
            struct { char padU1[7]; CLR_UINT8  u1;      };
            struct { char padU2[6]; CLR_UINT16 u2;      };
            struct { CLR_UINT32 padU4; CLR_UINT32 u4;   };
#endif
            struct U8
            {
#if !defined(NETMF_TARGET_BIG_ENDIAN)
                CLR_UINT32 _L;
                CLR_UINT32 _H;
                //--//                
#else
                CLR_UINT32 _H;
                CLR_UINT32 _L;
#endif
                operator CLR_UINT64() const
                {
                    return ((CLR_UINT64)_H << 32 | (CLR_UINT64)_L );
                }

                U8& operator=( const CLR_UINT64 num )
                {
                    _L = (CLR_UINT32)((ULONGLONGCONSTANT(0x00000000FFFFFFFF) & num)      );
                    _H = (CLR_UINT32)((ULONGLONGCONSTANT(0xFFFFFFFF00000000) & num) >> 32);
                    return *this;
                }
                U8& operator+=( const U8& num )
                {
                    CLR_UINT64 value = (CLR_UINT64)*this; // uses conversion
                    value += (CLR_UINT64)num;             // uses conversion and then built-in type __int64
                    *this = value;                        // uses assignment operator (operator=)
                    return *this;
                }

                U8& operator-=( const U8& num )
                {
                    CLR_UINT64 value = (CLR_UINT64)*this; // uses conversion
                    value -= (CLR_UINT64)num;             // uses conversion and then built-in type __int64
                    *this = value;                        // uses assignment operator (operator=)
                    return *this;
                }


                U8 operator*( const U8& num)
                {
                    CLR_UINT64 value = (CLR_UINT64)*this;
                    U8 ret_value;
                    value *= (CLR_UINT64)num;           // uses conversion and then built-in type __int64
                    ret_value = value;
                    return ret_value;
                }

                U8 operator/( const U8& num )
                {
                    CLR_UINT64 value = (CLR_UINT64)*this;
                    U8 ret_value;
                    value /= (CLR_UINT64)num;           // uses conversion and then built-in type __int64
                    ret_value = value;
                    return ret_value;
                }

                U8 operator~( )
                {
                    U8 ret_value;
                    ret_value._L = ~_L;                        
                    ret_value._H = ~_H;
                    return ret_value;
                }


                U8& operator%=( const U8& num )
                {
                    CLR_UINT64 value = (CLR_UINT64)*this; // uses conversion
                    value %= (CLR_UINT64)num;             // uses conversion and then built-in type __int64
                    *this = value;                        // uses assignment operator (operator=)
                    return *this;
                }

                U8& operator&=( const U8& num )
                {
                    _L &= num._L;
                    _H &= num._H;
                    return *this;
                }

                U8& operator|=( const U8& num )
                {
                    _L |= num._L;
                    _H |= num._H;
                    return *this;
                }

                U8& operator^=( const U8& num )
                {
                    _L ^= num._L;
                    _H ^= num._H;
                    return *this;
                }

                U8& operator>>=( const CLR_UINT32 num )
                {

                    CLR_UINT64 value = (CLR_UINT64)*this; // uses conversion
                    value >>= num;                        // uses conversion and then built-in type __int64
                    *this = value;                        // uses assignment operator (operator=)
                    return *this;
                }

                U8& operator<<=( const CLR_UINT32 num )
                {
                    CLR_UINT64 value = (CLR_UINT64)*this; // uses conversion
                    value <<= num;                        // uses conversion and then built-in type __int64
                    *this = value;                        // uses assignment operator (operator=)
                    return *this;
                }

                bool operator<( const U8& num )
                {
                    CLR_UINT64 value = (CLR_UINT64)*this; // uses conversion
                    return ( value < (CLR_UINT64)num );
                }

                bool operator>( const U8& num )
                {
                    CLR_UINT64 value = (CLR_UINT64)*this; // uses conversion
                    return ( value > (CLR_UINT64)num );
                }

                bool operator==( const U8& num )
                {
                    CLR_UINT64 value = (CLR_UINT64)*this; // uses conversion
                    return ( value ==(CLR_UINT64)num );
                }

                bool operator==( const CLR_UINT64 num )
                {
                    CLR_UINT64 value = (CLR_UINT64)*this; // uses conversion
                    return ( value == num );
                }
                
            } u8;
            //
#if !defined(NETMF_TARGET_BIG_ENDIAN)
            CLR_INT8  s1;
            CLR_INT16 s2;
            CLR_INT32 s4;
#else
            struct { char padS1[7]; CLR_INT8  s1;       };
            struct { char padS2[6]; CLR_INT16 s2;       };
            struct { CLR_UINT32 padS4; CLR_INT32 s4;    };
#endif
            struct S8
            {
#if !defined(NETMF_TARGET_BIG_ENDIAN)
                CLR_UINT32 _L;
                CLR_UINT32 _H;
                //--//                
#else
                CLR_UINT32 _H;
                CLR_UINT32 _L;
#endif
                //--//                

                operator CLR_INT64() const
                {
                     return (((CLR_UINT64)_H) << 32 | (CLR_UINT64)_L);
                }

                S8& operator=( const CLR_INT64 num )
                {
                    _L = (CLR_UINT32) (( ULONGLONGCONSTANT(0x00000000FFFFFFFF) & num)       );
                    _H = (CLR_UINT32) (( ULONGLONGCONSTANT(0xFFFFFFFF00000000) & num) >> 32 );
                    return *this;
                }

                S8& operator+=( const S8& num )
                {
                    CLR_INT64 value = (CLR_INT64)*this; // uses conversion
                    value += (CLR_INT64)num;            // uses conversion and then built-in type __int64
                    *this = value;                      // uses assignment operator (operator=)
                    return *this;
                }

                S8& operator-=( const S8& num )
                {
                    CLR_INT64 value = (CLR_INT64)*this; // uses conversion
                    value -= (CLR_INT64)num;             // uses conversion and then built-in type __int64
                    *this = value;                       // uses assignment operator (operator=)
                    return *this;
                }

                S8 operator*( const S8& num)
                {
                    CLR_INT64 value = (CLR_INT64)*this;
                    S8 ret_value;
                    value *= (CLR_INT64)num;           // uses conversion and then built-in type __int64
                    ret_value = value;
                    return ret_value;
                }

                S8 operator/( const S8& num )
                {
                    CLR_INT64 value = (CLR_INT64)*this;
                    S8 ret_value;
                    value /= (CLR_INT64)num;           // uses conversion and then built-in type __int64
                    ret_value = value;
                    return ret_value;
                }

                S8 operator~()
                {
                    S8 ret_value;
                    ret_value._L = ~_L;
                    ret_value._H = ~_H;    
                    return ret_value;
                }



                S8& operator%=( const S8& num )
                {
                    CLR_INT64 value = (CLR_INT64)*this; // uses conversion
                    value %= (CLR_INT64)num;            // uses conversion and then built-in type __int64
                    *this = value;                      // uses assignment operator (operator=)
                    return *this;
                }

                S8& operator&=( const S8& num )
                {
                    _L &= num._L;
                    _H &= num._H;
                    return *this;
                }

                S8& operator|=( const S8& num )
                {
                    _L |= num._L;
                    _H |= num._H;
                    return *this;
                }

                S8& operator^=( const S8& num )
                {
                    _L ^= num._L;
                    _H ^= num._H;
                    return *this;
                }

                S8& operator>>=( const CLR_UINT32 num )
                {
                    CLR_INT64 value = (CLR_INT64)*this; // uses conversion
                    value >>= num;                      // uses conversion and then built-in type __int64
                    *this = value;                      // uses assignment operator (operator=)
                    return *this;
                }

                S8& operator<<=( const CLR_UINT32 num )
                {
                    CLR_INT64 value = (CLR_INT64)*this; // uses conversion
                    value <<= num;                      // uses conversion and then built-in type __int64
                    *this = value;                      // uses assignment operator (operator=)
                    return *this;
                }

                bool operator<( const S8& num )
                {
                    CLR_INT64 value = (CLR_INT64)*this; // uses conversion
                    return ( value < (CLR_INT64)num );
                }

                bool operator>( const S8& num )
                {
                    CLR_INT64 value = (CLR_INT64)*this; // uses conversion
                    return ( value > (CLR_INT64)num );
                }

                bool operator==( const S8& num )
                {
                    CLR_INT64 value = (CLR_INT64)*this; // uses conversion
                    return ( value == (CLR_INT64)num );
                }


                bool operator==( const CLR_INT64 num )
                {
                    CLR_INT64 value = (CLR_INT64)*this; // uses conversion
                    return ( value == num );
                }
                
            } s8;
            //

#if !defined(TINYCLR_EMULATED_FLOATINGPOINT)
            
#if !defined(NETMF_TARGET_BIG_ENDIAN)
            float      r4;
#else
            struct { CLR_UINT32 padR4; float      r4; };
#endif
            struct R8
            {
#if !defined(NETMF_TARGET_BIG_ENDIAN)
                CLR_UINT32 _L;
                CLR_UINT32 _H;
#else
                CLR_UINT32 _H;
                CLR_UINT32 _L;
#endif
                //--//                
                operator double() const
                {
                    double ret_val;

#if !defined(NETMF_TARGET_BIG_ENDIAN)
#if defined(__GNUC__)
///
/// This code fixes an optimization problem with the gcc compiler.
/// When the optimization level is greater than zero, the gcc compiler
/// code will not work with the UINT32* conversion, it requires you 
/// to copy byte by byte.
///
                    CLR_UINT8* tmp = (CLR_UINT8*)&ret_val;
                    CLR_UINT8* src = (CLR_UINT8*)&_L;
                    int i;
                    
                    for(i=0; i<sizeof(CLR_UINT32); i++)
                    {
                        *tmp++ = *src++;
                    }

                    src = (CLR_UINT8*)&_H;
                    for(i=0; i<sizeof(CLR_UINT32); i++)
                    {
                        *tmp++ = *src++;
                    }
#else
                    CLR_UINT32 *tmp = (CLR_UINT32*)&ret_val;
                    tmp[0]=_L;
                    tmp[1]=_H;
#endif // defined(__GNUC__)
#else
#if defined(__GNUC__)
///
/// This code fixes an optimization problem with the gcc compiler.
/// When the optimization level is greater than zero, the gcc compiler
/// code will not work with the UINT32* conversion, it requires you 
/// to copy byte by byte.
///
                    CLR_UINT8* tmp = (CLR_UINT8*)&ret_val;
                    CLR_UINT8* src = (CLR_UINT8*)&_H;
                    int i;
                    
                    for(i=0; i<sizeof(UINT32); i++)
                    {
                        *tmp++ = *src++;
                    }

                    src = (UINT8*)&_L;
                    for(i=0; i<sizeof(UINT32); i++)
                    {
                        *tmp++ = *src++;
                    }
#else
                    CLR_UINT32 *tmp = (CLR_UINT32*)&ret_val;
                    tmp[0]=_H;
                    tmp[1]=_L;
#endif // defined(__GNUC__)
#endif                    

                    return ret_val; 
                }

                R8& operator=( const double num )
                {
#if !defined(NETMF_TARGET_BIG_ENDIAN)
#if defined(__GNUC__)
///
/// This code fixes an optimization problem with the gcc compiler.
/// When the optimization level is greater than zero, the gcc compiler
/// code will not work with the UINT32* conversion, it requires you 
/// to copy byte by byte.
///
                    CLR_UINT8* src = (CLR_UINT8*)&num;
                    CLR_UINT8* dst = (CLR_UINT8*)&_L;
                    int i;
                    
                    for(i=0; i<sizeof(CLR_UINT32); i++)
                    {
                        *dst++ = *src++;
                    }

                    dst = (CLR_UINT8*)&_H;
                    for(i=0; i<sizeof(CLR_UINT32); i++)
                    {
                        *dst++ = *src++;
                    }
#else
                    CLR_UINT32* tmp= (CLR_UINT32 *) &num;
                    _L = (CLR_UINT32)tmp[0];
                    _H = (CLR_UINT32)tmp[1];
#endif                    
#else
#if defined(__GNUC__)
///
/// This code fixes an optimization problem with the gcc compiler.
/// When the optimization level is greater than zero, the gcc compiler
/// code will not work with the UINT32* conversion, it requires you 
/// to copy byte by byte.
///
                    CLR_UINT8* src = (CLR_UINT8*)&num;
                    CLR_UINT8* dst = (CLR_UINT8*)&_H;
                    int i;
                    
                    for(i=0; i<sizeof(CLR_UINT32); i++)
                    {
                        *dst++ = *src++;
                    }

                    dst = (CLR_UINT8*)&_L;
                    for(i=0; i<sizeof(CLR_UINT32); i++)
                    {
                        *dst++ = *src++;
                    }
#else
                    CLR_UINT32* tmp= (CLR_UINT32 *) &num;
                    _H = (CLR_UINT32)tmp[0];
                    _L = (CLR_UINT32)tmp[1];
#endif
#endif

                    return *this;
                }

                R8& operator+=( const R8& num )
                {
                    double value = (double)*this;   // uses conversion
                    value += (double)num;           // uses conversion and then built-in type double
                    *this = value;                  // uses assignment operator (operator=)
                    return *this;
                }



                R8& operator-=( const R8& num )
                {
                    double value = (double)*this;   // uses conversion
                    value -= (double)num;           // uses conversion and then built-in type double
                    *this = value;                  // uses assignment operator (operator=)
                    return *this;
                }

                R8 operator*( const R8& num )
                {
                    double value = (double)*this;    // uses conversion
                    R8 ret_value;
                    value *= (double)num;            // uses conversion and then built-in type __int64
                    ret_value = value;               // uses assignment operator (operator=)
                    return ret_value;
                }

                R8 operator/( const R8& num )
                {
                    double value = (double)*this;    // uses conversion
                    R8 ret_value;
                    value /= (double)num;            // uses conversion and then built-in type __int64
                    ret_value = value;               // uses assignment operator (operator=)
                    return ret_value;
                }

                bool operator<( const R8& num )
                {
                    double value = (double)*this;    // uses conversion
                    return ( value < (double)num );
                }

                bool operator>( const R8& num )
                {
                    double value = (double)*this;    // uses conversion
                    return ( value > (double)num );
                }

                bool operator==( const R8& num )
                {
                    double value = (double)*this;    // uses conversion
                    return ( value == (double)num );
                }

                bool operator==( const double num )
                {
                    double value = (double)*this;    // uses conversion                   
                    return ( value == num );
                }

            } r8;

#else  
 /// not using floating point lib, emulated one

#if defined(NETMF_TARGET_BIG_ENDIAN)
            CLR_UINT32 padR4; 
#endif
 
            struct R4 {
               
                CLR_INT32  _L;

                operator CLR_INT32 () const
                {
                    return _L;
                }

                R4& operator=( const CLR_INT32 num )
                {
                    _L = num;
                    return *this;
                }
                R4& operator+=( const R4& num )
                {
                    _L += num._L;
                    return *this;
                }

                R4& operator-=( const R4& num )
                {
                    _L -= num._L;
                   return *this;
                }


                R4& operator%=( const R4& num )
                {
                    _L %= num._L;
                    return *this;
                }


                R4 operator*( const R4& num )
                {
                    R4  ret_value;
                    ret_value._L = (CLR_INT32)(((CLR_INT64)_L * (CLR_INT64)num._L) >> HB_FloatShift );
                    return ret_value;
                }

                R4 operator/( const R4& num )
                {
                    R4 ret_value;
                    ret_value._L = (CLR_INT32)((((CLR_INT64)_L) << HB_FloatShift)  / (CLR_INT64)num._L); 
                    return ret_value;
                }

            } r4;

            struct R8
            {
#if !defined(NETMF_TARGET_BIG_ENDIAN)
                CLR_UINT32 _L;
                CLR_UINT32 _H;
                //--//                
#else
                CLR_UINT32 _H;
                CLR_UINT32 _L;
#endif
                //--//                

                operator CLR_INT64() const
                {
                    return ((CLR_INT64)_H << 32 | (CLR_INT64)_L );
                }

                R8& operator=( const CLR_INT64 num )
                {
                    _L = (CLR_UINT32) (( ULONGLONGCONSTANT(0x00000000FFFFFFFF) & num)       );
                    _H = (CLR_UINT32) (( ULONGLONGCONSTANT(0xFFFFFFFF00000000) & num) >> 32 );
                    return *this;
                }

                R8& operator+=( const R8& num )
                {
                    CLR_INT64 value = (CLR_INT64)*this; // uses conversion
                    value += (CLR_INT64)num;            
                    *this = value;
                    return *this;
                }

                R8& operator-=( const R8& num )
                {
                    CLR_INT64 value = (CLR_INT64)*this; // uses conversion
                    value -= (CLR_INT64)num;
                    *this = value;
                    return *this;
                }

                R8 operator*( const R8& num)
                {
                    CLR_UINT64 op1     = (CLR_INT64) *this;
                    CLR_UINT64 op2     = (CLR_INT64) num;
                    CLR_UINT64 res     = 0;
                    R8 ret_value;

                    bool       fNegate = false;

                    if((CLR_INT64)op1 < (CLR_INT64)res)
                    {
                        op1     = -(CLR_INT64)op1;
                        fNegate = !fNegate;
                    }

                    if((CLR_INT64)op2 < (CLR_INT64)res)
                    {
                        op2     = -(CLR_INT64)op2;
                        fNegate = !fNegate;
                    }

                    CLR_UINT64 v;

#define ACCUMULATE(res,op,part) v = op1 * (CLR_UINT16)(op2 >> (16 * part)); res += (16 * part - HB_DoubleShift >= 0) ? (v << (16 * part - HB_DoubleShift)) : (v >> (HB_DoubleShift - 16 * part))

                    ACCUMULATE(res,+=,0);
                    ACCUMULATE(res,+=,1);
                    ACCUMULATE(res,+=,2);
                    ACCUMULATE(res,+=,3);

#undef ACCUMULATE

                    ret_value = (CLR_INT64)res;

                    if(fNegate && (CLR_INT64)ret_value != 0)
                    {
                        ret_value = -(CLR_INT64)ret_value;
                    }
                    
                    return ret_value;
                }

                R8 operator/( const R8& num )
                {
                    CLR_INT64 value = (CLR_INT64)*this;
                    R8 ret_value;

                    value = (value / num) << HB_DoubleShift; 

                    ret_value = value;                    

                    return ret_value;
                }

                bool operator==( const R8& num )
                {
                    CLR_INT64 value = (CLR_INT64)*this;             // uses conversion
                    return ( value == (CLR_INT64)num );
                }


                bool operator==( const CLR_INT64 num )
                {
                    CLR_INT64 value = (CLR_INT64)*this;             // uses conversion
                    return ( value == num );
                }
                R8& operator%=( const R8& num )
                {
                    CLR_INT64 value = (CLR_INT64)*this;             // uses conversion
                    
                    value = (CLR_INT64)((value % (CLR_INT64)num)) ; // uses conversion and then built-in type double
                    *this = value;                                  // uses assignment operator (operator=)

                    return *this;
                }

            } r8;

#endif
        } numeric;

        // The macro CT_ASSERT is used to validate that members of Numeric union start at zero offset in union.
        // This presumption is used in CRL_RT_Interop code.
        // This macro does not add any code or data member, it is pure compiler time validation.
        // This is not a valid check for Big Endian. 
        // Addr:  0011223344556677  Number: 0xAABBCCDD
        // MemBE: AABBCCDD
        // MemLE: DDCCBBAA
        // So for LE DD,Addr 00 is the low byte. 
        // For BE it is certainly not true, hence this check will not work. Will Interop? FIXME GJS - verify interop
#if !defined(NETMF_TARGET_BIG_ENDIAN)
        CT_ASSERT_UNIQUE_NAME( offsetof( Numeric, s1 ) == 0, s1 )
        CT_ASSERT_UNIQUE_NAME( offsetof( Numeric, s2 ) == 0, s2 )
        CT_ASSERT_UNIQUE_NAME( offsetof( Numeric, s4 ) == 0, s4 )
        CT_ASSERT_UNIQUE_NAME( offsetof( Numeric, u1 ) == 0, u1 )
        CT_ASSERT_UNIQUE_NAME( offsetof( Numeric, u2 ) == 0, u2 )
        CT_ASSERT_UNIQUE_NAME( offsetof( Numeric, u4 ) == 0, u4 )
        CT_ASSERT_UNIQUE_NAME( offsetof( Numeric, r4 ) == 0, r4 )
#endif
        CT_ASSERT_UNIQUE_NAME( offsetof( Numeric, s8 ) == 0, s8 )
        CT_ASSERT_UNIQUE_NAME( offsetof( Numeric, u8 ) == 0, u8 )
        CT_ASSERT_UNIQUE_NAME( offsetof( Numeric, r8 ) == 0, r8 )

        struct String
        {
            LPCSTR           m_text;
#if !defined(TINYCLR_NO_ASSEMBLY_STRINGS)
            CLR_RT_Assembly* m_assm;
#endif
        } string;

        //--//

        struct ObjectReference
        {
            CLR_RT_HeapBlock* ptr;
        } objectReference;

        struct ArrayReference
        {
            CLR_RT_HeapBlock_Array* array;
            CLR_UINT32              index;
        } arrayReference;

        struct ObjectHeader
        {
            CLR_RT_TypeDef_Index   cls;
            CLR_RT_HeapBlock_Lock* lock;
        } objectHeader;

#if defined(TINYCLR_APPDOMAINS)
        struct TransparentProxy
        {
            CLR_RT_HeapBlock*      ptr;                //points to a DATATYPE_CLASS derived from MBRO
            CLR_RT_AppDomain*      appDomain;          //EVENT HEAP -- NO RELOCATION            
        } transparentProxy;
#endif

        //--//

        CLR_RT_ReflectionDef_Index reflection;

        //--//

        struct BinaryBlob
        {
            CLR_RT_MarkingHandler    m_mark;
            CLR_RT_RelocationHandler m_relocate;
        } binaryBlob;

    } m_data;

public:

    //--//

    void InitializeToZero();

    //--//

    CLR_DataType DataType () const { return (CLR_DataType)m_id.type.dataType; }
    CLR_UINT8    DataFlags() const { return               m_id.type.flags   ; }
    CLR_UINT16   DataSize () const { return               m_id.type.size    ; }
    
    // Returns number of bytes actually used in CLR_RT_HeapBlock_AtomicData 
    CLR_UINT32 GetAtomicDataUsedBytes() const;

    void SetDataId      ( CLR_UINT32 id       ) { m_id.raw           = id;       }
    void ChangeDataType ( CLR_UINT32 dataType ) { m_id.type.dataType = dataType; }
    void ChangeDataFlags( CLR_UINT32 flags    ) { m_id.type.flags    = flags;    }

    void ClearData()
    {
        m_data.nodeLink.nextBlock = NULL;
        m_data.nodeLink.prevBlock = NULL;
    }

    void SetFlags  ( CLR_UINT8 flags )       {         m_id.type.flags |=  flags      ; }
    void ResetFlags( CLR_UINT8 flags )       {         m_id.type.flags &= ~flags      ; }
    bool IsFlagSet ( CLR_UINT8 flags ) const { return (m_id.type.flags &   flags) != 0; }

    //--//

#if defined(TINYCLR_FILL_MEMORY_WITH_DIRTY_PATTERN)

    void        Debug_ClearBlock  ( int   data );
    void        Debug_CheckPointer(            ) const;
    static void Debug_CheckPointer( void* ptr  );

#else

    void        Debug_ClearBlock  ( int   data )       {}
    void        Debug_CheckPointer(            ) const {}
    static void Debug_CheckPointer( void* ptr  )       {}

#endif

    //--//

    bool IsAlive          () const { return IsFlagSet ( HB_Alive     ); }
    bool IsEvent          () const { return IsFlagSet ( HB_Event     ); }
    bool IsForcedAlive    () const { return IsFlagSet ( HB_KeepAlive ); }

    bool IsPinned         () const { return IsFlagSet ( HB_Pinned    ); }
    void Pin              ()       {        SetFlags  ( HB_Pinned    ); }
    void Unpin            ()       {        ResetFlags( HB_Pinned    ); }

    void MarkDead         ()       {        ResetFlags( HB_Alive     ); }
    void MarkAlive        ()       {        SetFlags  ( HB_Alive     ); }

    void MarkForcedAlive  ()       {        SetFlags  ( HB_KeepAlive ); }
    void UnmarkForcedAlive()       {        ResetFlags( HB_KeepAlive ); }

    bool IsBoxed          () const { return IsFlagSet ( HB_Boxed     ); }
    void Box              ()       {        SetFlags  ( HB_Boxed     ); }
    void Unbox            ()       {        ResetFlags( HB_Boxed     ); }

    //--//

    void SetBoolean( bool              val                     ) { CLR_RT_HEAPBLOCK_ASSIGN_INTEGER32_SIGNED( DATATYPE_BOOLEAN, val ? 1 : 0 ); }
    void SetInteger( const CLR_INT32   num, CLR_UINT8 dataType ) { CLR_RT_HEAPBLOCK_ASSIGN_INTEGER32_SIGNED( dataType        , num         ); }

    void SetInteger( const CLR_INT8    num ) { CLR_RT_HEAPBLOCK_ASSIGN_INTEGER32_SIGNED  (DATATYPE_I1, num); }
    void SetInteger( const CLR_INT16   num ) { CLR_RT_HEAPBLOCK_ASSIGN_INTEGER32_SIGNED  (DATATYPE_I2, num); }
    void SetInteger( const CLR_INT32   num ) { CLR_RT_HEAPBLOCK_ASSIGN_INTEGER32_SIGNED  (DATATYPE_I4, num); }
    void SetInteger( const CLR_INT64&  num ) { CLR_RT_HEAPBLOCK_ASSIGN_INTEGER64_SIGNED  (DATATYPE_I8, num); }
    void SetInteger( const CLR_UINT8   num ) { CLR_RT_HEAPBLOCK_ASSIGN_INTEGER32_UNSIGNED(DATATYPE_U1, num); }
    void SetInteger( const CLR_UINT16  num ) { CLR_RT_HEAPBLOCK_ASSIGN_INTEGER32_UNSIGNED(DATATYPE_U2, num); }
    void SetInteger( const CLR_UINT32  num ) { CLR_RT_HEAPBLOCK_ASSIGN_INTEGER32_UNSIGNED(DATATYPE_U4, num); }
    void SetInteger( const CLR_UINT64& num ) { CLR_RT_HEAPBLOCK_ASSIGN_INTEGER64_UNSIGNED(DATATYPE_U8, num); }

#if !defined(TINYCLR_EMULATED_FLOATINGPOINT)
    void SetFloat  ( const float       num ) { CLR_RT_HEAPBLOCK_ASSIGN_FLOAT32           (DATATYPE_R4, num); }
    void SetDouble ( const double      num ) { CLR_RT_HEAPBLOCK_ASSIGN_FLOAT64           (DATATYPE_R8, num); }
    void SetFloatFromBits ( const CLR_UINT32  num ) { SetFloat ( *(const float *)&num ); }
    void SetDoubleFromBits( const CLR_UINT64& num ) { SetDouble( *(const double*)&num ); }
#else
    void SetFloat  ( const CLR_INT32   num ) { CLR_RT_HEAPBLOCK_ASSIGN_FLOAT32           (DATATYPE_R4, num); }
    void SetDouble ( const CLR_INT64&  num ) { CLR_RT_HEAPBLOCK_ASSIGN_FLOAT64           (DATATYPE_R8, num); }
    HRESULT SetFloatIEEE754 ( const CLR_UINT32  num );
    HRESULT SetDoubleIEEE754( const CLR_UINT64& num );
#endif


    void SetObjectReference( const CLR_RT_HeapBlock* ptr )
    {
        m_id.raw                   = CLR_RT_HEAPBLOCK_RAW_ID(DATATYPE_OBJECT, 0, 1);
        m_data.objectReference.ptr = (CLR_RT_HeapBlock*)ptr;
    }

#if defined(TINYCLR_APPDOMAINS)    
    CLR_RT_AppDomain* TransparentProxyAppDomain  () const { return m_data.transparentProxy.appDomain; }
    CLR_RT_HeapBlock* TransparentProxyDereference() const { return Dereference()                    ; }

    void SetTransparentProxyReference( CLR_RT_AppDomain* appDomain, CLR_RT_HeapBlock* ptr );
    HRESULT TransparentProxyValidate() const;

    bool IsTransparentProxy() const { return IsAReferenceOfThisType( DATATYPE_TRANSPARENT_PROXY ); }
#endif

    void SetReference( CLR_RT_HeapBlock& dst )
    {
        CLR_RT_HeapBlock* obj;

        //
        // ValueTypes are implemented as pointers to objects,
        // so getting a reference to a ValueType has to be treated like getting a reference to object, not to its holder!
        //
        if(dst.IsAValueType())
        {
            obj = dst.Dereference();
        }
        else
        {
            obj = &dst;
        }

        m_id.raw                   = CLR_RT_HEAPBLOCK_RAW_ID(DATATYPE_BYREF, 0, 1);
        m_data.objectReference.ptr = obj;
    }

    bool IsAReferenceOfThisType( CLR_DataType dataType ) const
    {
        if(DataType() == DATATYPE_OBJECT)
        {
            CLR_RT_HeapBlock* obj = Dereference();

            if(obj && obj->DataType() == dataType) return true;
        }

        return false;
    }

    bool IsAValueType() const
    {
        if(DataType() == DATATYPE_OBJECT)
        {
            CLR_RT_HeapBlock* obj = Dereference();

            if(obj && obj->DataType() == DATATYPE_VALUETYPE && obj->IsBoxed() == false) return true;
        }

        return false;
    }

    bool SameHeader( const CLR_RT_HeapBlock& right ) const
    {
        return this->m_data.numeric.u8 == right.m_data.numeric.u8;
    }

    //--//

    CLR_RT_HeapBlock_Array* RecoverArrayHeader() const
    {
        return (DataType() == DATATYPE_ARRAY_BYREF) ? m_data.arrayReference.array : NULL;
    }

    //--//

    LPCSTR RecoverString() const
    {
        if(DataType() == DATATYPE_OBJECT)
        {
            const CLR_RT_HeapBlock* ptr = m_data.objectReference.ptr;

            if(ptr)
            {
                return ptr->StringText();
            }
        }

        return NULL;
    }

    //--//
#if !defined(NETMF_TARGET_BIG_ENDIAN)
    const CLR_RT_HeapBlock_AtomicData& DataByRefConst() const { return m_data; }
#else
    const CLR_RT_HeapBlock_AtomicData& DataByRefConst() const 
    { 
        switch ( m_id.type.dataType )
        {
            case DATATYPE_BOOLEAN    :
            case DATATYPE_I1         :
            case DATATYPE_U1         :
            case DATATYPE_CHAR       :
                return (CLR_RT_HeapBlock_AtomicData&)m_data.numeric.u1;
            break;

            case DATATYPE_I2         :
            case DATATYPE_U2         :
                return (CLR_RT_HeapBlock_AtomicData&)m_data.numeric.u2;
            break;

            case DATATYPE_I4         :
            case DATATYPE_U4         :
            case DATATYPE_R4         :
                return (CLR_RT_HeapBlock_AtomicData&)m_data.numeric.u4;
            break;
        }
        return m_data; 
    }
#endif

    //--//

          CLR_RT_HeapBlock_AtomicData::Numeric& NumericByRef     ()       { return m_data.numeric; }
    const CLR_RT_HeapBlock_AtomicData::Numeric& NumericByRefConst() const { return m_data.numeric; }

    //--//

    LPCSTR StringText() const { return m_data.string.m_text; }

#if defined(TINYCLR_NO_ASSEMBLY_STRINGS)
    void SetStringText( LPCSTR szText )
    {
        m_data.string.m_text = szText;
    }
#else
    void SetStringText( LPCSTR szText, CLR_RT_Assembly* assm )
    {
        m_data.string.m_text = szText;
        m_data.string.m_assm = assm;
    }

    CLR_RT_Assembly* StringAssembly() const { return m_data.string.m_assm; }
#endif

    //--//

    const CLR_RT_TypeDef_Index& ObjectCls    (                             ) const { return m_data.objectHeader.cls        ; }
    CLR_RT_HeapBlock_Lock*      ObjectLock   (                             ) const { return m_data.objectHeader.lock       ; }
    void                        SetObjectLock( CLR_RT_HeapBlock_Lock* lock )       {        m_data.objectHeader.lock = lock; }

    HRESULT SetObjectCls( const CLR_RT_TypeDef_Index& cls );

    //--//

    const CLR_RT_ReflectionDef_Index& ReflectionDataConst() const { return m_data.reflection; }
    CLR_RT_ReflectionDef_Index&       ReflectionData     ()       { return m_data.reflection; }

    //--//

    CLR_RT_HeapBlock_Array* Array              () const { return m_data.arrayReference.array  ; }
    CLR_UINT32              ArrayIndex         () const { return m_data.arrayReference.index  ; }
    void                    ArrayIndexIncrement()       {        m_data.arrayReference.index++; }

    //--//

    CLR_RT_MarkingHandler    BinaryBlobMarkingHandler   () const { return m_data.binaryBlob.m_mark    ; }
    CLR_RT_RelocationHandler BinaryBlobRelocationHandler() const { return m_data.binaryBlob.m_relocate; }

    void SetBinaryBlobHandlers( CLR_RT_MarkingHandler mark, CLR_RT_RelocationHandler relocate )
    {
        m_data.binaryBlob.m_mark     = mark    ;
        m_data.binaryBlob.m_relocate = relocate;
    }

    //--//
    //--//

    CLR_RT_HeapBlock* Dereference() const { return m_data.objectReference.ptr; }

    CLR_RT_HeapBlock_WeakReference* DereferenceWeakReference() const { return (CLR_RT_HeapBlock_WeakReference*)Dereference(); }
    CLR_RT_HeapBlock_String       * DereferenceString       () const { return (CLR_RT_HeapBlock_String       *)Dereference(); }
    CLR_RT_HeapBlock_Array        * DereferenceArray        () const { return (CLR_RT_HeapBlock_Array        *)Dereference(); }
    CLR_RT_HeapBlock_Delegate     * DereferenceDelegate     () const { return (CLR_RT_HeapBlock_Delegate     *)Dereference(); }
    CLR_RT_HeapBlock_Delegate_List* DereferenceDelegateList () const { return (CLR_RT_HeapBlock_Delegate_List*)Dereference(); }
    CLR_RT_HeapBlock_BinaryBlob   * DereferenceBinaryBlob   () const { return (CLR_RT_HeapBlock_BinaryBlob   *)Dereference(); }

    //--//

    void AssignId( const CLR_RT_HeapBlock& value )
    {
        _ASSERTE(value.DataSize() == 1);

        m_id = value.m_id;
    }

    void AssignData( const CLR_RT_HeapBlock& value )
    {
        _ASSERTE(value.DataSize() == 1);

        m_data = value.m_data;
    }

    void Assign( const CLR_RT_HeapBlock& value )
    {
        _ASSERTE(value.DataSize() == 1);

        value.Debug_CheckPointer();        

        CLR_RT_HeapBlock_Raw* src = (CLR_RT_HeapBlock_Raw*) this;
        CLR_RT_HeapBlock_Raw* dst = (CLR_RT_HeapBlock_Raw*)&value;

        *src = *dst;
    }

    void AssignAndPreserveType( const CLR_RT_HeapBlock& value )
    {
        _ASSERTE(value.DataSize() == 1);

        this->m_data = value.m_data;

        if(this->DataType() > DATATYPE_LAST_PRIMITIVE_TO_PRESERVE) this->m_id = value.m_id;
    }

    void AssignPreserveTypeCheckPinned( const CLR_RT_HeapBlock& value )
    {
        _ASSERTE(value.DataSize() == 1);


        {   // If local variable does not have pinned type - move source data into it
            if (!IsPinned())
            {
                AssignAndPreserveType( value );
            }
            else // IsPinned() is true;
            {  AssignAndPinReferencedObject( value );          
            }
        }
    }

    // This function is called for assigning to "pinned" reference variables. 
    // Since it is rare case, the code is not inlined to save code size.
    void AssignAndPinReferencedObject( const CLR_RT_HeapBlock& value );

    HRESULT Convert( CLR_DataType et, bool fOverflow, bool fUnsigned )
    {
        //
        // For V1, we don't throw on overflow.
        //
        return Convert_Internal( et );
    }

    bool InitObject();

    //--//

    static CLR_INT32 Compare_Unsigned_Values( const CLR_RT_HeapBlock& left, const CLR_RT_HeapBlock& right ) { return Compare_Values( left, right, false ); }
    static CLR_INT32 Compare_Signed_Values  ( const CLR_RT_HeapBlock& left, const CLR_RT_HeapBlock& right ) { return Compare_Values( left, right, true  ); }

    //--//


    HRESULT SetReflection( const CLR_RT_ReflectionDef_Index& reflex );
    HRESULT SetReflection( const CLR_RT_Assembly_Index&      assm   );
    HRESULT SetReflection( const CLR_RT_TypeSpec_Index&      sig    );
    HRESULT SetReflection( const CLR_RT_TypeDef_Index&       cls    );
    HRESULT SetReflection( const CLR_RT_FieldDef_Index&      fd     );
    HRESULT SetReflection( const CLR_RT_MethodDef_Index&     md     );


    HRESULT InitializeArrayReference      ( CLR_RT_HeapBlock&       ref  , int index );
    void    InitializeArrayReferenceDirect( CLR_RT_HeapBlock_Array& array, int index );
    void    FixArrayReferenceForValueTypes(                                          );

    HRESULT           LoadFromReference    ( CLR_RT_HeapBlock&              ref           );
    HRESULT           StoreToReference     ( CLR_RT_HeapBlock&              ref, int size );
    HRESULT           Reassign             ( const CLR_RT_HeapBlock&        value         );
    HRESULT           PerformBoxingIfNeeded(                                              );
    HRESULT           PerformBoxing        ( const CLR_RT_TypeDef_Instance& cls           );
    HRESULT           PerformUnboxing      ( const CLR_RT_TypeDef_Instance& cls           );
    CLR_RT_HeapBlock* FixBoxingReference   (                                              );
    HRESULT           EnsureObjectReference( CLR_RT_HeapBlock*&             obj           );

    //--//

    bool IsZero () const;
    void Promote();

    static CLR_UINT32 GetHashCode   ( CLR_RT_HeapBlock* ptr, bool fRecurse, CLR_UINT32 crc                             );
    static bool       ObjectsEqual  ( const CLR_RT_HeapBlock& left, const CLR_RT_HeapBlock& right, bool fSameReference );

    static CLR_INT32  Compare_Values( const CLR_RT_HeapBlock& left, const CLR_RT_HeapBlock& right, bool fSigned        );

    HRESULT Convert_Internal( CLR_DataType et               );
    HRESULT NumericAdd      ( const CLR_RT_HeapBlock& right );
    HRESULT NumericSub      ( const CLR_RT_HeapBlock& right );
    HRESULT NumericMul      ( const CLR_RT_HeapBlock& right );
    HRESULT NumericDiv      ( const CLR_RT_HeapBlock& right );
    HRESULT NumericDivUn    ( const CLR_RT_HeapBlock& right );
    HRESULT NumericRem      ( const CLR_RT_HeapBlock& right );
    HRESULT NumericRemUn    ( const CLR_RT_HeapBlock& right );
    HRESULT NumericShl      ( const CLR_RT_HeapBlock& right );
    HRESULT NumericShr      ( const CLR_RT_HeapBlock& right );
    HRESULT NumericShrUn    ( const CLR_RT_HeapBlock& right );
    HRESULT NumericNeg      (                               );

    CLR_RT_HeapBlock* ExtractValueBlock(                       int offset );
    void              ReadValue        (       CLR_INT64& val, int offset );
    void              WriteValue       ( const CLR_INT64& val, int offset );

    void Relocate__HeapBlock();
    void Relocate_String    ();
    void Relocate_Obj       ();
    void Relocate_Cls       ();
    void Relocate_Ref       ();
    void Relocate_ArrayRef  ();

#if defined(TINYCLR_APPDOMAINS)
    void Relocate_TransparentProxy();
#endif

private:
    CLR_RT_HeapBlock& operator=( const CLR_RT_HeapBlock& );


};

//--//

#define TINYCLR_FOREACH_NODE(cls,ptr,lst)                                                            \
    {                                                                                                \
        cls* ptr;                                                                                    \
        cls* ptr##Next;                                                                              \
                                                                                                     \
        for(ptr = (cls*)(lst).FirstNode(); (ptr##Next = (cls*)ptr->Next()) != NULL; ptr = ptr##Next) \
        { TINYCLR_FAULT_ON_EARLY_COLLECTION( ptr##Next );

#define TINYCLR_FOREACH_NODE__NODECL(cls,ptr,lst)                                                    \
    {                                                                                                \
        cls* ptr##Next;                                                                              \
                                                                                                     \
        for(ptr = (cls*)(lst).FirstNode(); (ptr##Next = (cls*)ptr->Next()) != NULL; ptr = ptr##Next) \
        { TINYCLR_FAULT_ON_EARLY_COLLECTION( ptr##Next );

#define TINYCLR_FOREACH_NODE__DIRECT(cls,ptr,startNode)                                              \
    {                                                                                                \
        cls* ptr;                                                                                    \
        cls* ptr##Next;                                                                              \
                                                                                                     \
        for(ptr = (cls*)(startNode); (ptr##Next = (cls*)ptr->Next()) != NULL; ptr = ptr##Next)       \
        { TINYCLR_FAULT_ON_EARLY_COLLECTION( ptr##Next );

#define TINYCLR_FOREACH_NODE_PREPARE_RELOAD(ptr)                                                     \
            TINYCLR_CANCEL_EARLY_COLLECTION(ptr##Next)

#define TINYCLR_FOREACH_NODE_RELOAD(cls,ptr)                                                         \
            ptr##Next = (cls*)ptr->Next()

#define TINYCLR_FOREACH_NODE_RESTART(cls,ptr,lst)                                                    \
            ptr##Next = (cls*)(lst).FirstNode(); continue

#define TINYCLR_FOREACH_NODE_END()                                                                   \
        }                                                                                            \
    }


#define TINYCLR_FOREACH_NODE_BACKWARD(cls,ptr,lst)                                                   \
    {                                                                                                \
        cls* ptr;                                                                                    \
        cls* ptr##Prev;                                                                              \
                                                                                                     \
        for(ptr = (cls*)(lst).LastNode(); (ptr##Prev = (cls*)ptr->Prev()) != NULL; ptr = ptr##Prev)  \
        { TINYCLR_FAULT_ON_EARLY_COLLECTION( ptr##Prev );

#define TINYCLR_FOREACH_NODE_BACKWARD__NODECL(cls,ptr,lst)                                           \
    {                                                                                                \
        cls* ptr##Prev;                                                                              \
                                                                                                     \
        for(ptr = (cls*)(lst).LastNode(); (ptr##Prev = (cls*)ptr->Prev()) != NULL; ptr = ptr##Prev)  \
        { TINYCLR_FAULT_ON_EARLY_COLLECTION( ptr##Prev );

#define TINYCLR_FOREACH_NODE_BACKWARD__DIRECT(cls,ptr,startNode)                                     \
    {                                                                                                \
        cls* ptr;                                                                                    \
        cls* ptr##Prev;                                                                              \
                                                                                                     \
        for(ptr = (cls*)(startNode); (ptr##Prev = (cls*)ptr->Prev()) != NULL; ptr = ptr##Prev)       \
        { TINYCLR_FAULT_ON_EARLY_COLLECTION( ptr##Prev );

#define TINYCLR_FOREACH_NODE_BACKWARD_RESTART(cls,ptr,lst)                                           \
            ptr##Prev = (cls*)(lst).LastNode(); continue

#define TINYCLR_FOREACH_NODE_BACKWARD_PREPARE_RELOAD(ptr)                                            \
            TINYCLR_CANCEL_EARLY_COLLECTION(ptr##Prev)

#define TINYCLR_FOREACH_NODE_BACKWARD_RELOAD(cls,ptr)                                                \
            ptr##Prev = (cls*)ptr->Prev()

#define TINYCLR_FOREACH_NODE_BACKWARD_END()                                                          \
        }                                                                                            \
    }



struct CLR_RT_HeapBlock_Node : public CLR_RT_HeapBlock
{
    friend struct CLR_RT_DblLinkedList;

    void GenericNode_Initialize()
    {
        ClearData();
    }

    CLR_RT_HeapBlock_Node*  Next   () const { return m_data.nodeLink.nextBlock; }
    CLR_RT_HeapBlock_Node*  Prev   () const { return m_data.nodeLink.prevBlock; }

    void SetNext( CLR_RT_HeapBlock_Node* next ) { m_data.nodeLink.nextBlock = next; }
    void SetPrev( CLR_RT_HeapBlock_Node* prev ) { m_data.nodeLink.prevBlock = prev; }

    //
    // The ARM compiler cannot inline these methods,
    // it keeps injecting a call to a 4 instruction-long function (arg!!).
    //
    // So we'll have to use the explicit check...
    //
    //bool IsValidForward () const { return Next() != NULL; }
    //bool IsValidBackward() const { return Prev() != NULL; }

    //--//

#if TINYCLR_VALIDATE_HEAP >= TINYCLR_VALIDATE_HEAP_1_HeapBlocksAndUnlink
    void ConsistencyCheck_Nodes( CLR_RT_HeapBlock_Node* prev, CLR_RT_HeapBlock_Node* next )
    {
        if( prev &&  next) return;
        if(!prev && !next) return;

        CLR_Debug::Printf( "Bad node!!\r\n" );

        TINYCLR_DEBUG_STOP();
    }

#else

    void ConsistencyCheck_Nodes( CLR_RT_HeapBlock_Node* prev, CLR_RT_HeapBlock_Node* next ) {}

#endif

    void RemoveFromList()
    {
        CLR_RT_HeapBlock_Node* prev = m_data.nodeLink.prevBlock;
        CLR_RT_HeapBlock_Node* next = m_data.nodeLink.nextBlock;

        ConsistencyCheck_Nodes( prev, next );

        if(prev) prev->m_data.nodeLink.nextBlock = next;
        if(next) next->m_data.nodeLink.prevBlock = prev;
    }

    void Unlink()
    {
        CLR_RT_HeapBlock_Node* prev = m_data.nodeLink.prevBlock;
        CLR_RT_HeapBlock_Node* next = m_data.nodeLink.nextBlock;

        ConsistencyCheck_Nodes( prev, next );

        if(prev) prev->m_data.nodeLink.nextBlock = next;
        if(next) next->m_data.nodeLink.prevBlock = prev; 
        
        m_data.nodeLink.prevBlock = NULL;
        m_data.nodeLink.nextBlock = NULL;
    }

    //--//

    void Relocate();
};

struct CLR_RT_DblLinkedList
{
private:
    //
    // Logically, a list starts with a CLR_RT_HeapBlock_Node with only the Next() set and ends with a node with only Prev() set.
    // This can be collapsed to have the two nodes overlap.
    //
    CLR_RT_HeapBlock_Node* m_first; // ANY HEAP - DO RELOCATION -
    CLR_RT_HeapBlock_Node* m_null;
    CLR_RT_HeapBlock_Node* m_last;  // ANY HEAP - DO RELOCATION -

    //--//

public:
    void DblLinkedList_Initialize ();
    void DblLinkedList_PushToCache();
    void DblLinkedList_Release    ();

    int  NumOfNodes               ();

    void Relocate                 ();

    //--//

    CLR_RT_HeapBlock_Node* FirstNode     () const { return m_first          ; }
    CLR_RT_HeapBlock_Node* LastNode      () const { return m_last           ; }
    bool                   IsEmpty       () const { return m_first == Tail(); }

    CLR_RT_HeapBlock_Node* FirstValidNode() const { CLR_RT_HeapBlock_Node* res = m_first; return res->Next() ? res : NULL; }
    CLR_RT_HeapBlock_Node* LastValidNode () const { CLR_RT_HeapBlock_Node* res = m_last ; return res->Prev() ? res : NULL; }
    
    // Check that node pNode is not "dummy" tail or head node. 
    static bool IsValidListNode( CLR_RT_HeapBlock_Node* pNode ) 
    { 
        return pNode->m_data.nodeLink.nextBlock != NULL  &&  pNode->m_data.nodeLink.prevBlock != NULL; 
    }

    CLR_RT_HeapBlock_Node* Head() const { return (CLR_RT_HeapBlock_Node*)((size_t)&m_first - offsetof(CLR_RT_HeapBlock, m_data.nodeLink.nextBlock)); }
    CLR_RT_HeapBlock_Node* Tail() const { return (CLR_RT_HeapBlock_Node*)((size_t)&m_last  - offsetof(CLR_RT_HeapBlock, m_data.nodeLink.prevBlock)); }

#if TINYCLR_VALIDATE_HEAP >= TINYCLR_VALIDATE_HEAP_2_DblLinkedList
    void ValidateList();
#else
    void ValidateList() {};
#endif

    //--//

private:
    void Insert( CLR_RT_HeapBlock_Node* prev, CLR_RT_HeapBlock_Node* next, CLR_RT_HeapBlock_Node* node )
    {
        node->m_data.nodeLink.nextBlock = next;
        node->m_data.nodeLink.prevBlock = prev;

        next->m_data.nodeLink.prevBlock = node;
        prev->m_data.nodeLink.nextBlock = node;
    }

public:
    void InsertBeforeNode( CLR_RT_HeapBlock_Node* node, CLR_RT_HeapBlock_Node* nodeNew )
    {
        ValidateList();

        if(node && nodeNew && node != nodeNew)
        {
            nodeNew->RemoveFromList();

            Insert( node->Prev(), node, nodeNew );
        }
    }

    void InsertAfterNode( CLR_RT_HeapBlock_Node* node, CLR_RT_HeapBlock_Node* nodeNew )
    {
        ValidateList();

        if(node && nodeNew && node != nodeNew)
        {
            nodeNew->RemoveFromList();

            Insert( node, node->Next(), nodeNew );
        }
    }

    void LinkAtFront( CLR_RT_HeapBlock_Node* node )
    {
        InsertAfterNode( Head(), node );
    }

    void LinkAtBack( CLR_RT_HeapBlock_Node* node )
    {
        InsertBeforeNode( Tail(), node );
    }

    CLR_RT_HeapBlock_Node* ExtractFirstNode()
    {
        ValidateList();

        CLR_RT_HeapBlock_Node* node = FirstValidNode();

        if(node) node->Unlink();

        return node;
    }

    CLR_RT_HeapBlock_Node* ExtractLastNode()
    {
        ValidateList();

        CLR_RT_HeapBlock_Node* node = LastValidNode();

        if(node) node->Unlink();

        return node;
    }

    //--//

    PROHIBIT_COPY_CONSTRUCTORS(CLR_RT_DblLinkedList);
};


struct CLR_RT_AVLTree
{
    struct Owner;

    enum Skew
    {
        SKEW_LEFT  = -1,
        SKEW_NONE  =  0,
        SKEW_RIGHT =  1,
    };

    enum Result
    {
        RES_OK        = 0,
        RES_BALANCE   = 1,
        RES_NOTFOUND  = 2,
        RES_DUPLICATE = 3,
        RES_ERROR     = 4,
    };

    struct Entry : public CLR_RT_HeapBlock_Node
    {
        Entry* m_left;
        Entry* m_right;
        Skew   m_skew;
    };

    typedef int    (*ComparerFtn)( void* state, Entry* left, Entry* right );
    typedef Entry* (*NewNodeFtn )( void* state, Entry* payload            );
    typedef void   (*FreeNodeFtn)( void* state, Entry* node               );
    typedef void   (*ReassignFtn)( void* state, Entry* from, Entry* to    );

    struct OwnerInfo
    {
        ComparerFtn m_ftn_compare;
        NewNodeFtn  m_ftn_newNode;
        FreeNodeFtn m_ftn_freeNode;
        ReassignFtn m_ftn_reassignNode;
        void*       m_state;
    };

    //--//

    Entry*    m_root;
    OwnerInfo m_owner;

    //--//

    void Initialize();

    Result Insert( Entry* newDatum );
    Result Remove( Entry* oldDatum );
    Entry* Find  ( Entry* srcDatum );

private:
    static void RotateLeft    ( Entry*& n );
    static void RotateRight   ( Entry*& n );

    static Result LeftGrown   ( Entry*& n );
    static Result RightGrown  ( Entry*& n );

    static Result LeftShrunk  ( Entry*& n );
    static Result RightShrunk ( Entry*& n );

    bool FindHighest          ( Entry*& n, Entry* target, Result& res );
    bool FindLowest           ( Entry*& n, Entry* target, Result& res );

    Result Insert( Entry*& n, Entry* newDatum );
    Result Remove( Entry*& n, Entry* oldDatum );
};


////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////

struct CLR_RT_HeapBlock_String : public CLR_RT_HeapBlock
{
    static CLR_RT_HeapBlock_String* CreateInstance( CLR_RT_HeapBlock& reference, CLR_UINT32  length                        );
    static HRESULT                  CreateInstance( CLR_RT_HeapBlock& reference, LPCSTR      szText                        );
    static HRESULT                  CreateInstance( CLR_RT_HeapBlock& reference, LPCSTR      szText, CLR_UINT32 length     );
    static HRESULT                  CreateInstance( CLR_RT_HeapBlock& reference, CLR_UINT32  token , CLR_RT_Assembly* assm );
    static HRESULT                  CreateInstance( CLR_RT_HeapBlock& reference, LPCSTR      szText, CLR_RT_Assembly* assm );
    static HRESULT                  CreateInstance( CLR_RT_HeapBlock& reference, CLR_UINT16* szText, CLR_UINT32 length     );

    static CLR_RT_HeapBlock_String* GetStringEmpty();
};

struct CLR_RT_HeapBlock_Array : public CLR_RT_HeapBlock
{
    CLR_UINT32 m_numOfElements;
    CLR_UINT8  m_typeOfElement;
    CLR_UINT8  m_sizeOfElement;
    CLR_UINT8  m_fReference;
    CLR_UINT8  m_pad;

    //--//

    static HRESULT CreateInstance( CLR_RT_HeapBlock& reference, CLR_UINT32 length, const CLR_RT_ReflectionDef_Index& reflex                );
    static HRESULT CreateInstance( CLR_RT_HeapBlock& reference, CLR_UINT32 length, const CLR_RT_TypeDef_Index&       cls                   );
    static HRESULT CreateInstance( CLR_RT_HeapBlock& reference, CLR_UINT32 length,       CLR_RT_Assembly*            assm  , CLR_UINT32 tk );

    CLR_UINT8* GetFirstElement() { return ((CLR_UINT8*)&this[ 1 ]); }

    CLR_UINT8* GetElement( CLR_UINT32 index ) { return GetFirstElement() + m_sizeOfElement * index; }

    HRESULT ClearElements( int index, int length );

    //--//

    void Relocate();

    //--//

    static bool CheckRange( int index, int length, int numOfElements );

    static HRESULT IndexOf( CLR_RT_HeapBlock_Array* array, CLR_RT_HeapBlock& match, int start, int stop, bool fForward, int& index );
    static HRESULT Copy( CLR_RT_HeapBlock_Array* arraySrc, int indexSrc, CLR_RT_HeapBlock_Array* arrayDst, int indexDst, int length );
};

struct CLR_RT_HeapBlock_Delegate : public CLR_RT_HeapBlock_Node // OBJECT HEAP - DO RELOCATION -
{
    CLR_RT_TypeDef_Index   m_cls;
    CLR_RT_MethodDef_Index m_ftn;
#if defined(TINYCLR_DELEGATE_PRESERVE_STACK)
    CLR_UINT32             m_numOfStackFrames;
#endif
    CLR_RT_HeapBlock       m_object;  // ANY HEAP - DO RELOCATION -

#if defined(TINYCLR_APPDOMAINS)
    CLR_RT_AppDomain*     m_appDomain;
#endif

    //--//

    const CLR_RT_MethodDef_Index& DelegateFtn             () const { return m_ftn                              ; }
#if defined(TINYCLR_DELEGATE_PRESERVE_STACK)
    CLR_UINT32                    DelegateNumOfStackFrames() const { return m_numOfStackFrames                 ; }
    CLR_RT_MethodDef_Index*       GetStackFrames()                 { return (CLR_RT_MethodDef_Index*)&this[ 1 ]; }
#endif
    //--//

    static HRESULT CreateInstance( CLR_RT_HeapBlock& reference, const CLR_RT_MethodDef_Index& ftn, CLR_RT_StackFrame* call );

    void Relocate();
};

struct CLR_RT_HeapBlock_Delegate_List : public CLR_RT_HeapBlock_Node // OBJECT HEAP - DO RELOCATION -
{
    static const CLR_UINT32 c_Weak = 0x00000001;

    CLR_RT_TypeDef_Index m_cls;
    CLR_UINT32           m_length;
    CLR_UINT32           m_flags;

    CLR_RT_HeapBlock* GetDelegates() { return &this[ 1 ]; } // ANY HEAP - DO RELOCATION -

    //--//

    static HRESULT CreateInstance( CLR_RT_HeapBlock_Delegate_List*& list, CLR_UINT32 length );

    static HRESULT Combine( CLR_RT_HeapBlock& reference, CLR_RT_HeapBlock& delegateSrc, CLR_RT_HeapBlock& delegateNew, bool fWeak );
    static HRESULT Remove ( CLR_RT_HeapBlock& reference, CLR_RT_HeapBlock& delegateSrc, CLR_RT_HeapBlock& delegateOld             );

    void Relocate();

private:

    static HRESULT Change( CLR_RT_HeapBlock& reference, CLR_RT_HeapBlock& delegateSrc, CLR_RT_HeapBlock& delegateTarget, bool fCombine, bool fWeak );

    CLR_RT_HeapBlock* CopyAndCompress( CLR_RT_HeapBlock* src, CLR_RT_HeapBlock* dst, CLR_UINT32 num );
};

//--//

struct CLR_RT_HeapBlock_BinaryBlob : public CLR_RT_HeapBlock
{
    friend struct CLR_RT_Memory;

    //--//

    CLR_RT_Assembly* m_assembly;

    void*                               GetData(            ) { return (void*)&this[ 1 ]                     ; }
    static CLR_RT_HeapBlock_BinaryBlob* GetBlob( void* data ) { return (CLR_RT_HeapBlock_BinaryBlob*)data - 1; }

    //--//

    static HRESULT                      CreateInstance( CLR_RT_HeapBlock& reference, CLR_UINT32 length, CLR_RT_MarkingHandler mark, CLR_RT_RelocationHandler relocate, CLR_UINT32 flags );

    void Release( bool fEvent );

    void Relocate();

private:
    static CLR_RT_HeapBlock_BinaryBlob* Allocate( CLR_UINT32 length, CLR_UINT32 flags );
};

////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////

struct CLR_RT_ObjectToEvent_Destination : public CLR_RT_HeapBlock_Node // EVENT HEAP - NO RELOCATION -
{
    CLR_RT_DblLinkedList m_references;   // EVENT HEAP - NO RELOCATION - list of CLR_RT_ObjectToEvent_Source

    //--//

    void Initialize       ();
    void CheckAll         ();
    void SignalAll        ();
    void DetachAll        ();
    bool IsReadyForRelease();
    bool ReleaseWhenDead  ();
};

struct CLR_RT_ObjectToEvent_Source : public CLR_RT_HeapBlock_Node // EVENT HEAP - NO RELOCATION -
{
    CLR_RT_ObjectToEvent_Destination* m_eventPtr;     // EVENT HEAP  - NO RELOCATION - Pointer to the event referenced.

    CLR_RT_HeapBlock*                 m_objectPtr;    // OBJECT HEAP - DO RELOCATION - Pointer to the object to keep alive.
    CLR_RT_HeapBlock*                 m_referencePtr; // OBJECT HEAP - DO RELOCATION - Pointer to the HeapBlock referencing this structure.

    //--//

    static HRESULT                      CreateInstance ( CLR_RT_ObjectToEvent_Destination* event, CLR_RT_HeapBlock& object, CLR_RT_HeapBlock& reference );
    static CLR_RT_ObjectToEvent_Source* ExtractInstance(                                                                    CLR_RT_HeapBlock& reference );

    void EnsureObjectIsAlive();
    void Detach             ();

    void Relocate           ();
};

////////////////////////////////////////

struct CLR_RT_HeapBlock_Button : public CLR_RT_HeapBlock_Node // EVENT HEAP - NO RELOCATION -
{
    int  m_key;
    bool m_fPressed;
};

//--//

struct CLR_RT_HeapBlock_Lock : public CLR_RT_HeapBlock_Node // EVENT HEAP - NO RELOCATION -
{
    struct Owner : public CLR_RT_HeapBlock_Node // EVENT HEAP - NO RELOCATION -
    {
        CLR_RT_SubThread* m_owningSubThread;    // EVENT HEAP - NO RELOCATION -
        CLR_UINT32        m_recursion;
    };

    CLR_RT_Thread*       m_owningThread; // EVENT HEAP - NO RELOCATION -

    CLR_RT_HeapBlock     m_resource;     // OBJECT HEAP - DO RELOCATION -

#if defined(TINYCLR_APPDOMAINS)
    CLR_RT_AppDomain*    m_appDomain;   // EVENT HEAP - NO RELOCATION -
#endif

    CLR_RT_DblLinkedList m_owners;       // EVENT HEAP - NO RELOCATION - list of CLR_RT_HeapBlock_Lock::Owner
    CLR_RT_DblLinkedList m_requests;     // EVENT HEAP - NO RELOCATION - list of CLR_RT_HeapBlock_LockRequest

    //--//

    static HRESULT CreateInstance    ( CLR_RT_HeapBlock_Lock*& lock, CLR_RT_Thread*     th, CLR_RT_HeapBlock& resource               );
    static HRESULT IncrementOwnership( CLR_RT_HeapBlock_Lock*  lock, CLR_RT_SubThread* sth, const CLR_INT64& timeExpire, bool fForce );
    static HRESULT DecrementOwnership( CLR_RT_HeapBlock_Lock*  lock, CLR_RT_SubThread* sth                                           );

    void DestroyOwner( CLR_RT_SubThread* sth );
    void ChangeOwner (                       );

    void Relocate      ();
    void Relocate_Owner();
};

struct CLR_RT_HeapBlock_LockRequest : public CLR_RT_HeapBlock_Node // EVENT HEAP - NO RELOCATION -
{
    CLR_RT_SubThread* m_subthreadWaiting;  // EVENT HEAP - NO RELOCATION -
    CLR_INT64         m_timeExpire;
    bool              m_fForce;

    //--//

    static HRESULT CreateInstance( CLR_RT_HeapBlock_Lock* lock, CLR_RT_SubThread* sth, const CLR_INT64& timeExpire, bool fForce );
};

struct CLR_RT_HeapBlock_Timer : public CLR_RT_ObjectToEvent_Destination // EVENT HEAP - NO RELOCATION -
{
    static const CLR_UINT32 c_NoFixedChange    = 0x00000000;
    static const CLR_UINT32 c_SecondChange     = 0x00000001;
    static const CLR_UINT32 c_MinuteChange     = 0x00000002;
    static const CLR_UINT32 c_HourChange       = 0x00000003;
    static const CLR_UINT32 c_DayChange        = 0x00000004;
    static const CLR_UINT32 c_TimeZoneChange   = 0x00000005;
    static const CLR_UINT32 c_SetTimeChange    = 0x00000006;
    static const CLR_UINT32 c_AnyChange        = 0x0000000F;

    static const CLR_UINT32 c_Triggered        = 0x00000010;
    static const CLR_UINT32 c_Executing        = 0x00000020;

    static const CLR_UINT32 c_ACTION_Create    = 0x00010000;
    static const CLR_UINT32 c_ACTION_Change    = 0x00020000;
    static const CLR_UINT32 c_ACTION_Destroy   = 0x00040000;
    static const CLR_UINT32 c_UNUSED_00080000  = 0x00080000;

    static const CLR_UINT32 c_INPUT_Int32      = 0x00100000;
    static const CLR_UINT32 c_INPUT_TimeSpan   = 0x00200000;
    static const CLR_UINT32 c_INPUT_Absolute   = 0x00400000;

    static const CLR_UINT32 c_UNUSED_10000000  = 0x10000000;
    static const CLR_UINT32 c_AbsoluteTimer    = 0x20000000;
    static const CLR_UINT32 c_Recurring        = 0x40000000;
    static const CLR_UINT32 c_EnabledTimer     = 0x80000000;

    //--//

    static const CLR_INT32 c_TickPerMillisecond = 10000;
    //--//

    CLR_UINT32                 m_flags;
    CLR_INT64                  m_timeExpire;
    CLR_INT64                  m_timeFrequency;
    CLR_INT64                  m_timeLastExpiration;
    CLR_INT64                  m_ticksLastExpiration;

    //--//

    static HRESULT CreateInstance ( CLR_UINT32 flags, CLR_RT_HeapBlock& owner, CLR_RT_HeapBlock&        tmRef );
    static HRESULT ExtractInstance(                   CLR_RT_HeapBlock& ref  , CLR_RT_HeapBlock_Timer*& timer );
    static HRESULT ConfigureObject( CLR_RT_StackFrame& stack, CLR_UINT32 flags                                );
    static bool    CheckDisposed  ( CLR_RT_StackFrame& stack                                                  );

    void AdjustNextFixedExpire( const SYSTEMTIME& systemTime, bool fNext );

    void Trigger      ();
    void Reschedule   ();
    void RecoverFromGC();
    void SpawnTimer   ( CLR_RT_Thread* thread );

    static void ThreadTerminationCallback( void* param );
};

struct CLR_RT_HeapBlock_EndPoint : public CLR_RT_ObjectToEvent_Destination // EVENT HEAP - NO RELOCATION -
{
    struct Port
    {
        CLR_UINT32 m_type;
        CLR_UINT32 m_id;

        bool Compare( const Port& port );
    };

    struct Address
    {
        CLR_UINT32 m_seq;
        Port       m_from;
        Port       m_to;
    };


    struct Message : public CLR_RT_HeapBlock_Node
    {
        CLR_UINT32 m_cmd;
        Address    m_addr;

        CLR_UINT32 m_found;
        CLR_UINT32 m_length;

        CLR_UINT8  m_data[ 1 ];
    };

    Port                 m_addr;
    CLR_RT_DblLinkedList m_messages;
    CLR_UINT32           m_seq;

    //--//

    static CLR_RT_DblLinkedList m_endPoints;

    static void HandlerMethod_Initialize   ();
    static void HandlerMethod_RecoverFromGC();
    static void HandlerMethod_CleanUp      ();

    static CLR_RT_HeapBlock_EndPoint* FindEndPoint( const CLR_RT_HeapBlock_EndPoint::Port& port );

    static HRESULT CreateInstance ( const CLR_RT_HeapBlock_EndPoint::Port& port, CLR_RT_HeapBlock& owner, CLR_RT_HeapBlock&           epRef    );
    static HRESULT ExtractInstance(                                              CLR_RT_HeapBlock& ref  , CLR_RT_HeapBlock_EndPoint*& endPoint );

    bool ReleaseWhenDeadEx();
    void RecoverFromGC    ();

    Message* FindMessage( CLR_UINT32 cmd, const CLR_UINT32* seq );
};

//--//

struct CLR_RT_HeapBlock_WaitForObject : public CLR_RT_HeapBlock_Node // EVENT HEAP - NO RELOCATION -
{
    CLR_INT64         m_timeExpire;
    CLR_UINT32        m_cObjects;
    bool              m_fWaitAll;

    CLR_RT_HeapBlock* GetWaitForObjects() { return &this[ 1 ]; } // EVENT HEAP - DO RELOCATION -

    //--//

    static HRESULT WaitForSignal ( CLR_RT_StackFrame& stack, const CLR_INT64& timeExpire, CLR_RT_HeapBlock& object );    
    static HRESULT WaitForSignal ( CLR_RT_StackFrame& stack, const CLR_INT64& timeExpire, CLR_RT_HeapBlock* objects, CLR_UINT32 cObjects, bool fWaitAll );    
    static void SignalObject     ( CLR_RT_HeapBlock& object );

    void Relocate();

private:
    static bool    TryWaitForSignal ( CLR_RT_Thread* caller,                              CLR_RT_HeapBlock* objects, CLR_UINT32 cObjects, bool fWaitAll );
    static void    TryWaitForSignal ( CLR_RT_Thread* caller );
    static HRESULT CreateInstance   ( CLR_RT_Thread* caller, const CLR_INT64& timeExpire, CLR_RT_HeapBlock* objects, CLR_UINT32 cObjects, bool fWaitAll );    
};

//--//

struct CLR_RT_HeapBlock_Finalizer : public CLR_RT_HeapBlock_Node // EVENT HEAP - NO RELOCATION -
{
    CLR_RT_HeapBlock*      m_object;   // OBJECT HEAP - DO RELOCATION -
    CLR_RT_MethodDef_Index m_md;

#if defined(TINYCLR_APPDOMAINS)
    CLR_RT_AppDomain*      m_appDomain;
#endif

    //--//

    static HRESULT CreateInstance( CLR_RT_HeapBlock* object, const CLR_RT_TypeDef_Instance& inst );

    void Relocate();

    static void SuppressFinalize( CLR_RT_HeapBlock* object );

private:
    static void RemoveInstance( CLR_RT_HeapBlock* object, CLR_RT_DblLinkedList& lst );
};

//--//

struct CLR_RT_HeapBlock_MemoryStream : public CLR_RT_HeapBlock_Node // EVENT HEAP - NO RELOCATION -
{
    struct Buffer : public CLR_RT_HeapBlock_Node // EVENT HEAP - NO RELOCATION -
    {
        static const int c_NumOfBlocks = 32;
        static const int c_PayloadSize = c_NumOfBlocks * sizeof(CLR_RT_HeapBlock) - sizeof(CLR_UINT8*) - sizeof(int);

        CLR_UINT8* m_data;
        int        m_length;
        CLR_UINT8  m_payload[ c_PayloadSize ];
    };

    //--//

    CLR_RT_DblLinkedList m_buffers;           // EVENT HEAP - NO RELOCATION - list of CLR_RT_HeapBlock_MemoryStream::Buffer

    Buffer*              m_current;
    int                  m_pos;
    CLR_UINT32           m_avail;

    //--//

    static HRESULT CreateInstance( CLR_RT_HeapBlock_MemoryStream*& stream, CLR_UINT8* buf, int len );
    static void    DeleteInstance( CLR_RT_HeapBlock_MemoryStream*& stream                          );

    void Release();

    void Rewind();
    void Reset();

    HRESULT ToArray  ( CLR_RT_HeapBlock& ref );

    int BitsAvailable();
    int BitsWritten  ();

    HRESULT ReadBits  (       CLR_UINT32& val, CLR_UINT32 bits  );
    HRESULT WriteBits (       CLR_UINT32  val, CLR_UINT32 bits  );

    HRESULT ReadBits  (       CLR_UINT64& val, CLR_UINT32 bits  );
    HRESULT WriteBits (       CLR_UINT64  val, CLR_UINT32 bits  );

    HRESULT ReadArray (       CLR_UINT8*  buf, CLR_UINT32 bytes );
    HRESULT WriteArray( const CLR_UINT8*  buf, CLR_UINT32 bytes );

private:

    HRESULT Initialize( CLR_UINT8* buf, CLR_UINT32 len );

    Buffer* NewBuffer();
};

////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////

struct CLR_RT_HeapBlock_WeakReference_Identity
{
    CLR_UINT32 m_crc;
    CLR_UINT32 m_flags;
    CLR_UINT32 m_length;

    CLR_UINT32 m_selectorHash;
    CLR_UINT32 m_id;
    CLR_INT32  m_priority;

    CLR_UINT32 ComputeCRC( const CLR_UINT8* ptr, CLR_UINT32 len ) const;
};

struct CLR_RT_HeapBlock_WeakReference : public CLR_RT_HeapBlock_Node // OBJECT HEAP - DO RELOCATION -
{
    static const CLR_UINT32 WR_SurviveBoot        = 0x00000001;
    static const CLR_UINT32 WR_SurvivePowerdown   = 0x00000002;
    static const CLR_UINT32 WR_ArrayOfBytes       = 0x00000004;
    static const CLR_UINT32 WR_Unused_00000008    = 0x00000008;
    static const CLR_UINT32 WR_Unused_00000010    = 0x00000010;
    static const CLR_UINT32 WR_Unused_00000020    = 0x00000020;
    static const CLR_UINT32 WR_Unused_00000040    = 0x00000040;
    static const CLR_UINT32 WR_Unused_00000080    = 0x00000080;
    static const CLR_UINT32 WR_Unused_00000100    = 0x00000100;
    static const CLR_UINT32 WR_Unused_00000200    = 0x00000200;
    static const CLR_UINT32 WR_Unused_00000400    = 0x00000400;
    static const CLR_UINT32 WR_Unused_00000800    = 0x00000800;
    static const CLR_UINT32 WR_Unused_00001000    = 0x00001000;
    static const CLR_UINT32 WR_Unused_00002000    = 0x00002000;
    static const CLR_UINT32 WR_Unused_00004000    = 0x00004000;
    static const CLR_UINT32 WR_Unused_00008000    = 0x00008000;
    static const CLR_UINT32 WR_Unused_00010000    = 0x00010000;
    static const CLR_UINT32 WR_Unused_00020000    = 0x00020000;
    static const CLR_UINT32 WR_Unused_00040000    = 0x00040000;
    static const CLR_UINT32 WR_Unused_00080000    = 0x00080000;
    static const CLR_UINT32 WR_Unused_00100000    = 0x00100000;
    static const CLR_UINT32 WR_Unused_00200000    = 0x00200000;
    static const CLR_UINT32 WR_Unused_00400000    = 0x00400000;
    static const CLR_UINT32 WR_Unused_00800000    = 0x00800000;
    static const CLR_UINT32 WR_Unused_01000000    = 0x01000000;
    static const CLR_UINT32 WR_Unused_02000000    = 0x02000000;
    static const CLR_UINT32 WR_Unused_04000000    = 0x04000000;
    static const CLR_UINT32 WR_Unused_08000000    = 0x08000000;
    static const CLR_UINT32 WR_Unused_10000000    = 0x10000000;
    static const CLR_UINT32 WR_Persisted          = 0x20000000;
    static const CLR_UINT32 WR_Restored           = 0x40000000;
    static const CLR_UINT32 WR_ExtendedType       = 0x80000000;

    static const CLR_UINT32 WR_MaskForStorage = WR_SurviveBoot | WR_SurvivePowerdown | WR_ArrayOfBytes | WR_ExtendedType;

    //--//

    CLR_RT_HeapBlock_WeakReference_Identity m_identity;

    CLR_RT_HeapBlock*                       m_targetDirect;     // OBJECT HEAP - DO RELOCATION -
    CLR_RT_HeapBlock_Array*                 m_targetSerialized; // OBJECT HEAP - DO RELOCATION -
    CLR_RT_HeapBlock_Array*                 m_targetCopied;     // OBJECT HEAP - NO RELOCATION -

    //--//

    static HRESULT CreateInstance( CLR_RT_HeapBlock_WeakReference*& weakref );

    static void RecoverObjects    ( CLR_RT_DblLinkedList& lstHeap                          );
    static bool PrepareForRecovery( CLR_RT_HeapBlock_Node* ptr, CLR_RT_HeapBlock_Node* end, CLR_UINT32 blockSize );

    HRESULT SetTarget( CLR_RT_HeapBlock& targetReference );
    HRESULT GetTarget( CLR_RT_HeapBlock& targetReference );

    void InsertInPriorityOrder();

    void Relocate();
};

//--//

struct CLR_RT_Persistence_Manager
{
    static const CLR_UINT32 c_Erased = 0xFFFFFFFF;

    struct ObjectHeader
    {
        static const CLR_UINT32 c_Version      = 0x504F5631; // POV1 = Persisted Object V1

        static const CLR_UINT32 c_InUseBlock   = 0xFFFFFFFF;
        static const CLR_UINT32 c_DeletedBlock = 0x00000000;

        CLR_UINT32                              m_signature;
        CLR_UINT32                              m_status;

        CLR_UINT32                              m_crcIdentity;
        CLR_RT_HeapBlock_WeakReference_Identity m_identity;

        CLR_RT_HeapBlock_Array                  m_object;

        //--//

        static ObjectHeader* Find( FLASH_WORD* start, FLASH_WORD* end );

        bool Initialize( CLR_RT_HeapBlock_WeakReference* ref );

        bool HasGoodSignature() const { return m_signature == c_Version   ; }
        bool IsInUse         () const { return m_status    == c_InUseBlock; }

        bool IsGood( bool fIncludeData ) const;

        void Delete();

               CLR_UINT32 Length(                                           ) const;
        static CLR_UINT32 Length( const CLR_RT_HeapBlock_WeakReference* ref );
        static CLR_UINT32 Length( CLR_UINT32 data                           );

        ObjectHeader* Next() const;

    private:
        CLR_UINT32 ComputeCRC() const;

        //--//
    };

    struct BankHeader
    {
        static const CLR_UINT32 c_Version     = 0x50535631; // PSV1 = Persisted Storage V1

        static const CLR_UINT32 c_InUseBank   = 0xFFFFFFFF;
        static const CLR_UINT32 c_DeletedBank = 0x00000000;

        CLR_UINT32 m_signature;
        CLR_UINT32 m_status;

        CLR_UINT32 m_sequenceNumber;

        //--//

        static BankHeader* Find( FLASH_WORD* start, FLASH_WORD* end );

        void Initialize();

        bool HasGoodSignature() const { return m_signature == c_Version;   } 
        bool IsInUse         () const { return m_status    == c_InUseBank; } 

        bool IsGood() const;

        void Delete();

        ObjectHeader* FirstObjectHeader() const { return (ObjectHeader*)&this[ 1 ]; }
    };

    struct Bank
    {
        static const CLR_UINT32 c_SafetyMargin = 10;

        static const FLASH_WORD c_Erased       = 0xFFFFFFFF;
        static const FLASH_WORD c_Invalidated  = 0x00000000;

        //--//

        CLR_UINT32          m_totalBytes;
        CLR_UINT32          m_totalSafeBytes;

        BlockStorageStream  m_stream;

        BankHeader*         m_bankHeader;

        FLASH_WORD*         m_start;
        FLASH_WORD*         m_end;

        FLASH_WORD*         m_current;

        //--//

        bool IsGood() const;

        bool Initialize( UINT32 kind );

        bool Erase   ( int& sectorIndex );
        void EraseAll(                  );

        bool Format     (                           );
        bool SetSequence( CLR_UINT32 sequenceNumber );

        void Switch( Bank& other );

        CLR_RT_Persistence_Manager::ObjectHeader* RecoverHeader( CLR_RT_HeapBlock_WeakReference* ref                                         );
        bool                                      WriteHeader  ( CLR_RT_HeapBlock_WeakReference* ref, ObjectHeader*& pOH, FLASH_WORD*& pData );

        //--//
        
        BankHeader* GetBankHeader() const { return m_bankHeader; }

        //--//
        
        void ReloadNonXIPBufferData();

        bool WriteNonXIPData(FLASH_WORD* dst, CLR_UINT32 length);

        static bool FindBankWriteNonXIPData(FLASH_WORD* dst, CLR_UINT32 length);
        //--//

        static bool CanWrite  ( FLASH_WORD* dst,                          CLR_UINT32 length );
        static bool Write     ( FLASH_WORD* dst, const FLASH_WORD* src  , CLR_UINT32 length );
        static void Invalidate( FLASH_WORD* dst,       FLASH_WORD  match, CLR_UINT32 length );

        static FLASH_WORD* IncrementPointer( FLASH_WORD* ptr, CLR_UINT32 length ) 
        {             
            return (FLASH_WORD*)((CLR_UINT8*)ptr + length);
        }
        static FLASH_WORD* DecrementPointer( FLASH_WORD* ptr, CLR_UINT32 length ) { return (FLASH_WORD*)((CLR_UINT8*)ptr - length); }
    };

    //--//

    static const CLR_UINT32 STATE_FlushNextObject   = 0;
    static const CLR_UINT32 STATE_Idle              = 1;
    static const CLR_UINT32 STATE_Erase             = 2;
    static const CLR_UINT32 STATE_EraseSector       = 3;
    static const CLR_UINT32 STATE_CopyToOtherBank   = 4;
    static const CLR_UINT32 STATE_CopyBackToRam     = 5;
    static const CLR_UINT32 STATE_SwitchBank        = 6;

    static const CLR_UINT32 c_MaxWriteBurst = 128;

    //--//

    HAL_COMPLETION                  m_completion;

    UINT32                          m_margin_BurstWrite;
    UINT32                          m_margin_BlockErase;

    Bank                            m_bankA;
    Bank                            m_bankB;

    CLR_UINT32                      m_state;

    CLR_RT_HeapBlock_WeakReference* m_pending_object;
    ObjectHeader*                   m_pending_header;
    CLR_UINT32                      m_pending_size;
    FLASH_WORD*                     m_pending_src;
    FLASH_WORD*                     m_pending_dst;

    int                             m_eraseIndex;

    //--//

    void Initialize();
    void Uninitialize();
    void EraseAll  ();

    void InvalidateEntry( CLR_RT_HeapBlock_WeakReference* weak );

    void Relocate();

    ObjectHeader* RecoverHeader( CLR_RT_HeapBlock_WeakReference* ref );

    static void Callback( void* arg );

#if !defined(BUILD_RTM)
    void GenerateStatistics( CLR_UINT32& totalSize, CLR_UINT32& inUse );
#endif

    void Flush();

    //--//

#undef DECL_POSTFIX
#if defined(TINYCLR_TRACE_PERSISTENCE)
#define DECL_POSTFIX
#else
#define DECL_POSTFIX {}
#endif

    static void Trace_Emit( LPSTR szText ) DECL_POSTFIX;

    static void Trace_Printf( LPCSTR format, ... ) DECL_POSTFIX;

    static void Trace_DumpIdentity( LPSTR& szBuffer, size_t& iBuffer, CLR_RT_HeapBlock_WeakReference_Identity* identity ) DECL_POSTFIX;

    static void Trace_DumpState( LPCSTR szText, FLASH_WORD* dst, ObjectHeader* oh, CLR_RT_HeapBlock_WeakReference* wr ) DECL_POSTFIX;

    //--//

private:

    bool AdvanceState( bool force );

    void EnqueueNextCallback();
};

extern CLR_RT_Persistence_Manager g_CLR_RT_Persistence_Manager;

//--//


struct CLR_RT_ApplicationInterrupt;

//--//

struct GPIO_PortParams
{
    GPIO_INT_EDGE m_interruptMode;
    GPIO_RESISTOR m_resistorMode;
    bool          m_glitchFilterEnable;
    bool          m_initialState;
    bool          m_initialDirectionOutput;
    
    //--//

    static const CLR_UINT8 c_Input             = 0x01; // GPIO_ATTRIBUTE_INPUT
    static const CLR_UINT8 c_Output            = 0x02; // GPIO_ATTRIBUTE_OUTPUT
    static const CLR_UINT8 c_AlternateA        = 0x04; // GPIO_ATTRIBUTE_ALTERNATE_A
    static const CLR_UINT8 c_AlternateB        = 0x08; // GPIO_ATTRIBUTE_ALTERNATE_B

    static const CLR_UINT8 c_InterruptDisabled = 0x10;
    static const CLR_UINT8 c_Disposed          = 0x20;
};


struct CLR_RT_HeapBlock_NativeEventDispatcher : public CLR_RT_ObjectToEvent_Destination // EVENT HEAP - NO RELOCATION -
{
    static void HandlerMethod_Initialize ();
    static void HandlerMethod_RecoverFromGC ();
    static void HandlerMethod_CleanUp ();

    static CLR_RT_DblLinkedList m_ioPorts; 


    struct InterruptPortInterrupt
    {
        CLR_INT64                 m_time;
        CLR_RT_HeapBlock_NativeEventDispatcher*  m_context;
        CLR_UINT32                m_data1;
        CLR_UINT32                m_data2;
        CLR_UINT32                m_data3;
    };

    // Pointer to Hardware driver methods
    CLR_RT_DriverInterruptMethods  *m_DriverMethods;
    //--//
    // Poiner to custom data used by device drivers.
    void  *m_pDrvCustomData;



    //--//

    static HRESULT CreateInstance ( CLR_RT_HeapBlock& owner, CLR_RT_HeapBlock& portRef );
    static HRESULT ExtractInstance( CLR_RT_HeapBlock&        ref, CLR_RT_HeapBlock_NativeEventDispatcher*& port                                                          );

    HRESULT StartDispatch       ( CLR_RT_ApplicationInterrupt* interrupt, CLR_RT_Thread* th );
    HRESULT RecoverManagedObject( CLR_RT_HeapBlock*& port                                   );

    static void ThreadTerminationCallback( void* arg                                  );
    void SaveToHALQueue( UINT32 data1, UINT32 data2 );
    void RemoveFromHALQueue();

    void RecoverFromGC    ();
    bool ReleaseWhenDeadEx();
};

//--//

struct CLR_RT_ApplicationInterrupt : public CLR_RT_HeapBlock_Node
{        
    CLR_RT_HeapBlock_NativeEventDispatcher ::InterruptPortInterrupt m_interruptPortInterrupt;
};

//--//

struct CLR_RT_HeapBlock_I2CXAction : public CLR_RT_ObjectToEvent_Destination // EVENT HEAP - NO RELOCATION -
{
    I2C_HAL_XACTION*       m_HalXAction;
    I2C_HAL_XACTION_UNIT** m_HalXActionUnits;
    I2C_WORD**             m_dataBuffers;
    size_t                 m_xActionUnits;

    //--//

    static CLR_RT_DblLinkedList m_i2cPorts;

    static void HandlerMethod_Initialize ();
    static void HandlerMethod_RecoverFromGC ();
    static void HandlerMethod_CleanUp ();

    static HRESULT CreateInstance ( CLR_RT_HeapBlock& owner, CLR_RT_HeapBlock& xActionRef        );
    static HRESULT ExtractInstance( CLR_RT_HeapBlock& ref, CLR_RT_HeapBlock_I2CXAction*& xAction );

    HRESULT AllocateXAction   ( CLR_UINT32 numXActionsUnits                               );
    HRESULT PrepareXAction    ( I2C_USER_CONFIGURATION& config, size_t numXActionUnits    );
    HRESULT PrepareXActionUnit( CLR_UINT8* buffer, size_t length, size_t unit, bool fRead );
    void    CopyBuffer        ( CLR_UINT8* dst   , size_t length, size_t unit             );
    void    ReleaseBuffers    (                                                           );
    
    HRESULT Enqueue(             );
    void    Cancel ( bool signal );
    
    size_t    TransactedBytes  ();
    bool      IsPending        ();
    bool      IsTerminated     ();
    bool      IsCompleted      ();
    bool      IsReadXActionUnit( size_t unit );
    CLR_UINT8 GetStatus        ();

    void    RecoverFromGC    ();
    bool    ReleaseWhenDeadEx();       
};

//--//

#define Library_corlib_native_System_Collections_ArrayList__FIELD___items 1
#define Library_corlib_native_System_Collections_ArrayList__FIELD___size  2

struct CLR_RT_HeapBlock_ArrayList : public CLR_RT_HeapBlock
{
public:
    HRESULT GetItem( CLR_INT32 index, CLR_RT_HeapBlock*& value );
    HRESULT SetItem( CLR_INT32 index, CLR_RT_HeapBlock* value );

    HRESULT Add( CLR_RT_HeapBlock* value, CLR_INT32& index );
    HRESULT Clear();
    HRESULT Insert( CLR_INT32 index, CLR_RT_HeapBlock* value );
    HRESULT RemoveAt( CLR_INT32 index );
    HRESULT SetCapacity( CLR_INT32 newCapacity );

    //--//
    
    __inline CLR_INT32 GetSize() { return ((CLR_RT_HeapBlock*)this)[ Library_corlib_native_System_Collections_ArrayList__FIELD___size ].NumericByRef().s4; }

private:

    // Keep in-sync with _defaultCapacity in System.Collections.ArrayList class in ArrayList.cs
    static const CLR_INT32 c_DefaultCapacity = 4;

    HRESULT EnsureCapacity( CLR_INT32 min, CLR_INT32 currentCapacity );

    __inline CLR_RT_HeapBlock_Array* GetItems() { return ((CLR_RT_HeapBlock*)this)[ Library_corlib_native_System_Collections_ArrayList__FIELD___items ].DereferenceArray(); }
    
    __inline void SetItems( CLR_RT_HeapBlock_Array* items ) { ((CLR_RT_HeapBlock*)this)[ Library_corlib_native_System_Collections_ArrayList__FIELD___items ].SetObjectReference( items ); }
    __inline void SetSize ( CLR_INT32 size                ) { ((CLR_RT_HeapBlock*)this)[ Library_corlib_native_System_Collections_ArrayList__FIELD___size  ].SetInteger        ( size  ); }
};


#define Library_corlib_native_System_Collections_Queue__FIELD___array 1
#define Library_corlib_native_System_Collections_Queue__FIELD___head  2
#define Library_corlib_native_System_Collections_Queue__FIELD___tail  3
#define Library_corlib_native_System_Collections_Queue__FIELD___size  4

struct CLR_RT_HeapBlock_Queue : public CLR_RT_HeapBlock
{
public:
    HRESULT Dequeue( CLR_RT_HeapBlock*& value );
    HRESULT Enqueue( CLR_RT_HeapBlock* value );

    HRESULT Peek( CLR_RT_HeapBlock*& value );
    HRESULT Clear();

    HRESULT CopyTo( CLR_RT_HeapBlock_Array* toArray, CLR_INT32 index );

    //--//

    __inline CLR_INT32 GetSize() { return ((CLR_RT_HeapBlock*)this)[ Library_corlib_native_System_Collections_Queue__FIELD___size ].NumericByRef().s4; }

private:

    // Keep in-sync with _defaultCapacity in System.Collections.Queue class in Queue.cs
    static const CLR_INT32 c_DefaultCapacity = 4;

    static HRESULT ObjArrayMemcpy( CLR_RT_HeapBlock_Array* arraySrc, int indexSrc, CLR_RT_HeapBlock_Array* arrayDst, int indexDst, int length );

    __inline CLR_RT_HeapBlock_Array* GetArray() { return ((CLR_RT_HeapBlock*)this)[ Library_corlib_native_System_Collections_Queue__FIELD___array ].DereferenceArray(); }
    __inline CLR_INT32                  Head () { return ((CLR_RT_HeapBlock*)this)[ Library_corlib_native_System_Collections_Queue__FIELD___head  ].NumericByRef().s4;  }
    __inline CLR_INT32               GetTail () { return ((CLR_RT_HeapBlock*)this)[ Library_corlib_native_System_Collections_Queue__FIELD___tail  ].NumericByRef().s4;  }
    
    __inline void SetArray( CLR_RT_HeapBlock_Array* array ) { ((CLR_RT_HeapBlock*)this)[ Library_corlib_native_System_Collections_Queue__FIELD___array ].SetObjectReference( array ); }
    __inline void SetHead ( CLR_INT32 head                ) { ((CLR_RT_HeapBlock*)this)[ Library_corlib_native_System_Collections_Queue__FIELD___head  ].SetInteger        ( head  ); }
    __inline void SetTail ( CLR_INT32 tail                ) { ((CLR_RT_HeapBlock*)this)[ Library_corlib_native_System_Collections_Queue__FIELD___tail  ].SetInteger        ( tail  ); }
    __inline void SetSize ( CLR_INT32 size                ) { ((CLR_RT_HeapBlock*)this)[ Library_corlib_native_System_Collections_Queue__FIELD___size  ].SetInteger        ( size  ); }
};


#define Library_corlib_native_System_Collections_Stack__FIELD___array 1
#define Library_corlib_native_System_Collections_Stack__FIELD___size  2

struct CLR_RT_HeapBlock_Stack : public CLR_RT_HeapBlock
{
public:
    HRESULT Pop( CLR_RT_HeapBlock*& value );
    HRESULT Push( CLR_RT_HeapBlock* value );

    HRESULT Peek( CLR_RT_HeapBlock*& value );
    HRESULT Clear();

    //--//

    __inline CLR_INT32 GetSize() { return ((CLR_RT_HeapBlock*)this)[ Library_corlib_native_System_Collections_Stack__FIELD___size ].NumericByRef().s4; }

protected:

    // Keep in-sync with _defaultCapacity in System.Collections.Stack class in Stack.cs
    static const CLR_INT32 c_DefaultCapacity = 4;

    __inline CLR_RT_HeapBlock_Array* GetArray() { return ((CLR_RT_HeapBlock*)this)[ Library_corlib_native_System_Collections_Stack__FIELD___array ].DereferenceArray(); }
    
    __inline void SetArray( CLR_RT_HeapBlock_Array* array ) { ((CLR_RT_HeapBlock*)this)[ Library_corlib_native_System_Collections_Stack__FIELD___array ].SetObjectReference( array ); }
    __inline void SetSize ( CLR_INT32 size                ) { ((CLR_RT_HeapBlock*)this)[ Library_corlib_native_System_Collections_Stack__FIELD___size  ].SetInteger        ( size  ); }
};

////////////////////////////////////////////////////////////////////////////////////////////////////

#endif // _TINYCLR_RUNTIME__HEAPBLOCK_H_
