////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//--//

#ifndef _TINYHAL_H_
#define _TINYHAL_H_ 1

#include <stdio.h>
#include <string.h>
#include <stdarg.h>

#if !defined(PLATFORM_EMULATED_FLOATINGPOINT)
#include <math.h>
#if !defined(__RENESAS__)
#include <locale.h>
#endif

#else 

/***************************************************/
// Keep in sync with the tinycr_runtime__heapblock.h


#define HAL_FLOAT_SHIFT          10
#define HAL_FLOAT_PRECISION      1000

#define HAL_DOUBLE_SHIFT          16
#define HAL_DOUBLE_PRECISION    10000

/************************************************/

#endif

#include <tinyhal_types.h>
#include <tinyhal_releaseinfo.h>

#if !defined(_WIN32) && !defined(FIQ_SAMPLING_PROFILER) && !defined(HAL_REDUCESIZE) && defined(PROFILE_BUILD)
#define ENABLE_NATIVE_PROFILER
#endif

#include "..\pal\Diagnostics\Native_Profiler.h"

#if (defined(_WIN32) || defined(WIN32)) && !defined(_WIN32_WCE)
    #include <crtdbg.h>
#endif

#include <TinySupport.h>
#include <Heap_Decl.h>

//--//

#if defined(__arm) && !defined(PLATFORM_ARM_OS_PORT)

// we include this to error at link time if we use any of the C semihosted stuff
#pragma import(__use_no_semihosting_swi)

#endif

#if defined(_WIN32_WCE)

#define PLATFORM_WINCE
#define ADS_PACKED
#define GNU_PACKED
#define __section(x)
#define __irq

#define FORCEINLINE __forceinline


#elif defined(_WIN32) || defined(WIN32)

#define PLATFORM_WINDOWS
#define ADS_PACKED
#define GNU_PACKED
#define __section(x)
#define __irq

#define FORCEINLINE __forceinline


#elif defined(__GNUC__)

#define PLATFORM_ARM
#define ADS_PACKED
#define GNU_PACKED  __attribute__((packed))
#ifndef __section
#define __section(x) __attribute__((section(x)))
#endif
#define __irq __attribute__((interrupt))
#define __forceinline __attribute__((always_inline))
#define ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED // Include so that zi variables are correctly sectioned

#define FORCEINLINE __forceinline


#elif defined(arm) || defined(__arm)

#define PLATFORM_ARM
#define ADS_PACKED __packed
#define GNU_PACKED
#define ADS_LINKER_BUG__NOT_ALL_UNUSED_VARIABLES_ARE_REMOVED // Some global variables are not removed if left in the default section.
#define __section(x)

#if defined(ARM_V1_2)

#define __forceinline
#define FORCEINLINE __inline      // ads1.2 does not support forceinline

#elif defined(ARM_V3_1)

#define FORCEINLINE __forceinline

#else

#define FORCEINLINE __forceinline


#endif

#elif defined(__ADSPBLACKFIN__)

#define PLATFORM_BLACKFIN
#define ADS_PACKED
#define GNU_PACKED
#define __forceinline __inline
#define __irq

#define FORCEINLINE __inline

#elif defined(__RENESAS__)

#define PLATFORM_SH
#define ADS_PACKED
#define GNU_PACKED
#define __forceinline inline
#define __inline inline
#define __irq
#define __section(x)

#define FORCEINLINE inline

#else
!ERROR
#endif

//--//

enum SYSTEM_STATE
{
    SYSTEM_STATE_CONTINUOUS_CPU,
    SYSTEM_STATE_ISR,
    SYSTEM_STATE_NO_CONTINUATIONS,
    SYSTEM_STATE_NO_ARM_CORE_CLOCK_STOPPAGE,
    SYSTEM_STATE_FLASH_OPERATION_ACTIVE,
    SYSTEM_STATE_TOTAL_STATES
};

//--//

#define ONE_MHZ  1000000
#define TEN_MHZ 10000000

//--//

typedef UINT32 GPIO_PIN;

//--//

struct GPIO_FLAG
{
    GPIO_PIN  Pin;
    BOOL      ActiveState;
};

//--//

#define INVALID_MEMORY_ADDRESS 0xBAADF00D

// Port stuff

#define STANDARD_USART_MIN_BAUDRATE  75
#define STANDARD_USART_MAX_BAUDRATE  2304000

// COM_HANDLE Defines a type representing both a port type or "transport" and a port number
// The COM_HANDLE is a multi bit field value with the following bit fields usage
//    |--------+--------+--------+--------|
//    |33222222|22221111|111111  |        |
//    |10987654|32109876|54321098|76543210| bit position
//    |--------+--------+--------+--------|
//    |00000000|00000000|TTTTTTTT|pppppppp| ( transport != USB_TRANSPORT )
//    |--------+--------+--------+--------|
//    |00000000|00000000|TTTTTTTT|cccppppp| ( transport == USB_TRANSPORT )
//    |--------+--------+--------+--------|
// 
// where:
//    T => Transport type
//              USART_TRANSPORT => 1
//                USB_TRANSPORT => 2
//             SOCKET_TRANSPORT => 3
//              DEBUG_TRANSPORT => 4
//                LCD_TRANSPORT => 5
//        FLASH_WRITE_TRANSPORT => 6
//          MESSAGING_TRANSPORT => 7
//            GENERIC_TRANSPORT => 8
//    p => port instance number 
//        Port instances in the handle are 1 based. (e.g. p == 0 is invalid except when T == 0 )
//    c -> Controller instance number ( USB_TRANSPORT only )
//
//    NULL_PORT => T==0 && p == 0
//
// GENERIC_TRANSPORT is any custom port that isn't one of the above, they 
// are implemneted for the DebugPort_xxxx apis and the port number is 
// and index into a const global table of port interfaces (structure of
// function pointers) These allow custom extensions to the normal transports
// without needing to continue defining additional transport types and modifiying
// switch on transport code. To keep compatibility high and code churn low, the
// previous legacy transports remain. 
typedef INT32 COM_HANDLE;

#define TRANSPORT_SHIFT             8
#define TRANSPORT_MASK              (0xFF << TRANSPORT_SHIFT)
#define PORT_NUMBER_MASK            0x00FF

// Macro to extract the transport type from a COM_HANDLE
#define ExtractTransport(x)         ((UINT32)(x) & TRANSPORT_MASK)

// Macro to extract wellknown system event flag ids from a COM_HANDLE
#define ExtractEventFromTransport(x) (ExtractTransport(x) == USART_TRANSPORT     ? SYSTEM_EVENT_FLAG_COM_IN: \
                                      ExtractTransport(x) == USB_TRANSPORT       ? SYSTEM_EVENT_FLAG_USB_IN: \
                                      ExtractTransport(x) == SOCKET_TRANSPORT    ? SYSTEM_EVENT_FLAG_SOCKET: \
                                      ExtractTransport(x) == GENERIC_TRANSPORT   ? SYSTEM_EVENT_FLAG_GENERIC_PORT: \
                                      ExtractTransport(x) == DEBUG_TRANSPORT     ? SYSTEM_EVENT_FLAG_DEBUGGER_ACTIVITY: \
                                      ExtractTransport(x) == MESSAGING_TRANSPORT ? SYSTEM_EVENT_FLAG_MESSAGING_ACTIVITY: \
                                      0) \

#define USART_TRANSPORT             (1 << TRANSPORT_SHIFT)
#define COM_NULL                    ((COM_HANDLE)(USART_TRANSPORT))

#define USB_TRANSPORT               (2 << TRANSPORT_SHIFT)
#define USB_CONTROLLER_SHIFT        5
#define USB_CONTROLLER_MASK         0xE0
#define USB_STREAM_MASK             0x00FF
#define USB_STREAM_INDEX_MASK       0x001F

#define SOCKET_TRANSPORT            (3 << TRANSPORT_SHIFT)
#define COM_SOCKET_DBG              ((COM_HANDLE)(SOCKET_TRANSPORT + 1))

#define DEBUG_TRANSPORT             (4 << TRANSPORT_SHIFT)

#define LCD_TRANSPORT               (5 << TRANSPORT_SHIFT)
#define STREAM_LCD                  ((COM_HANDLE)(LCD_TRANSPORT + 1))

#define FLASH_WRITE_TRANSPORT       (6 << TRANSPORT_SHIFT)
#define STREAM_FLASH_LOGGING        ((COM_HANDLE)(FLASH_WRITE_TRANSPORT + 1))

#define MESSAGING_TRANSPORT         (7 << TRANSPORT_SHIFT)

#define GENERIC_TRANSPORT           (8 << TRANSPORT_SHIFT)

#define COM_IsSerial(x)             (((x) & TRANSPORT_MASK) == USART_TRANSPORT)
#define COM_IsUsb(x)                (((x) & TRANSPORT_MASK) == USB_TRANSPORT)
#define COM_IsSock(x)               (((x) & TRANSPORT_MASK) == SOCKET_TRANSPORT)
#define COM_IsDebug(x)              (((x) & TRANSPORT_MASK) == DEBUG_TRANSPORT)
#define COM_IsMessaging(x)          (((x) & TRANSPORT_MASK) == MESSAGING_TRANSPORT)
#define COM_IsGeneric(x)            (((x) & TRANSPORT_MASK) == GENERIC_TRANSPORT)

// Extracts a USART port number from a USART COM_HANDLE
#define ConvertCOM_ComPort(x)       (((x) & PORT_NUMBER_MASK) - 1)

// Extracts a USB stream id from a USB COM_HANDLE
#define ConvertCOM_UsbStream(x)     (((x) & USB_STREAM_MASK ) - 1)

// Extracts a USB Controller id from a USB COM_HANDLE
#define ConvertCOM_UsbController(x) (((x) & USB_CONTROLLER_MASK) >> USB_CONTROLLER_SHIFT )

// Extracts a USB stream index from a USB COM_HANDLE
#define ConvertCOM_UsbStreamIndex(x) ((x) & USB_STREAM_INDEX_MASK)      /* NOTE: This only works with Streams - NOT Handles */

// Extracts a Socket transport port id from a SOCKET_TRASNPORT COM_HANDLE
#define ConvertCOM_SockPort(x)      (((x) & PORT_NUMBER_MASK) - 1)

// Extracts a Debug transport port id from a DEBUG_TRANSPORT COM_HANDLE
#define ConvertCOM_DebugPort(x)     (((x) & PORT_NUMBER_MASK) - 1)

// Extracts a messaging transport port id from a MESSAGING_TRANSPORT COM_HANDLE
#define ConvertCOM_MessagingPort(x) (((x) & PORT_NUMBER_MASK) - 1)

// Extracts a generic transport port id from a GENERIC_TRANSPORT COM_HANDLE
#define ConvertCOM_GenericPort(x) (((x) & PORT_NUMBER_MASK) - 1)

// Creates a COM_HANDLE value for a platform specific USART port number
#define ConvertCOM_ComHandle(x)      ((COM_HANDLE)((x) + USART_TRANSPORT     + 1))

// Creates a COM_HANDLE value for a platform specific USB port number
#define ConvertCOM_UsbHandle(x)      ((COM_HANDLE)((x) + USB_TRANSPORT       + 1))

// Creates a COM_HANDLE value for a platform specific socket port number
// NOTE: The meaning of port # here is platform specific transport layer number
//       and not a TCP/IP stack port number. (i.e. it can be an index to a HAL table of open sockets)
#define ConvertCOM_SockHandle(x)     ((COM_HANDLE)((x) + SOCKET_TRANSPORT    + 1))

// Creates a COM_HANDLE value for a platform specific port number
#define ConvertCOM_DebugHandle(x)    ((COM_HANDLE)((x) + DEBUG_TRANSPORT     + 1))

// Creates a COM_HANDLE value for a platform specific MESSAGING port number
#define ConvertCOM_MessagingHandle(x)((COM_HANDLE)((x) + MESSAGING_TRANSPORT + 1))

// Creates a COM_HANDLE value for a platform specific GENERIC port number
#define ConvertCOM_GenericHandle(x)((COM_HANDLE)((x) + GENERIC_TRANSPORT + 1))

typedef UINT32 FLASH_WORD;

struct BlockStorageDevice;
struct BlockRegionInfo;

struct HAL_CONFIG_BLOCK_STORAGE_DATA
{
    UINT32 ConfigAddress;
    UINT32 BlockLength;
    BOOL   isXIP;

    BlockStorageDevice *Device;
};

struct ConfigurationSector;

struct HAL_CONFIG_BLOCK
{
    static const UINT32 c_Version_V2 = 0x324C4148; // HAL2
    static const UINT32 c_Seed       = 1; // HAL_STRUCT_VERSION

    //--//

    UINT32 Signature;
    UINT32 HeaderCRC;
    UINT32 DataCRC;
    UINT32 Size;
    char   DriverName[64];

private:
    BOOL IsGoodBlock() const;
    BOOL IsGoodData () const;
    BOOL IsGood     () const;

    const HAL_CONFIG_BLOCK* Next() const;
    const void*             Data() const;

    BOOL Prepare( const char* Name, void* Data, UINT32 Size );

    const HAL_CONFIG_BLOCK* Find  ( const char* Name, BOOL fSkipCurrent, BOOL fAppend = false ) const;

    static BOOL UpdateBlock (const HAL_CONFIG_BLOCK_STORAGE_DATA &blData, const void* Address, const HAL_CONFIG_BLOCK* Header, void* Data, size_t Length, const void* LastConfigAddress, BOOL isChipRO );

    static BOOL GetConfigSectorAddress(HAL_CONFIG_BLOCK_STORAGE_DATA &blockStorageData);

    static BOOL CompactBlock(HAL_CONFIG_BLOCK_STORAGE_DATA& blData, const ConfigurationSector* cfgStatic, const HAL_CONFIG_BLOCK* cfgEnd);

    //--//

    static size_t RoundLength( size_t Size )
    {
        return (Size + sizeof(FLASH_WORD) - 1) & ~(sizeof(FLASH_WORD)-1);
    }

public:
    static BOOL InvalidateBlockWithName( const char* Name, BOOL isChipRO );
    
    static BOOL UpdateBlockWithName( const char*  Name, void* Data, size_t Length, BOOL isChipRO );

    static BOOL ApplyConfig( const char* Name, void* Address, size_t Length );

    static BOOL ApplyConfig( const char* Name, void* Address, size_t Length, void** newAlloc );


};

struct HAL_DRIVER_CONFIG_HEADER
{
    UINT32 Enable;
};

struct HAL_SYSTEM_MEMORY_CONFIG
{
    UINT32 Base;
    UINT32 Size;
};

struct HAL_SYSTEM_CONFIG
{
    static const UINT32 c_MaxDebuggers = 1;
    static const UINT32 c_MaxMessaging = 1;

    HAL_DRIVER_CONFIG_HEADER Header;

    //--//

    COM_HANDLE               DebuggerPorts[c_MaxDebuggers];
    COM_HANDLE               MessagingPorts[c_MaxMessaging];
    // communication channel for debug messages in the debugger
    // which may be VS, MFDEPLOY, etc... Accessed via debug_printf
    // in the HAL/PAL and System.Diagnostics.Debug.Print() in managed
    // applications
    COM_HANDLE               DebugTextPort;

    UINT32                   USART_DefaultBaudRate;
    // internal HAL/PAL debug/tracing channel, this is seperate
    // to allow tracing messages in the driver that implements
    // the transport for the Debugger and DebugTextPort. This
    // channel is accessed via hal_printf() in the HAL/PAL
    COM_HANDLE               stdio;

    HAL_SYSTEM_MEMORY_CONFIG RAM1;
    HAL_SYSTEM_MEMORY_CONFIG FLASH;

    //--//

    static LPCSTR GetDriverName() { return "HAL_SYSTEM"; }
};

extern const char         HalName[];
extern HAL_SYSTEM_CONFIG  HalSystemConfig;

//--//

#ifndef __max
#define __max(a,b)  (((a) > (b)) ? (a) : (b))
#endif

#ifndef __min
#define __min(a,b)  (((a) < (b)) ? (a) : (b))
#endif

//--//

//
// The ADS compiler normally generates good code, except for some simple 64bit comparisons, like X <op> 0...
//
__inline bool Uint64IsZero   ( UINT64 num ) { return ((UINT32)(num >> 32) | (UINT32)(num)) == 0; }
__inline bool Uint64IsNotZero( UINT64 num ) { return ((UINT32)(num >> 32) | (UINT32)(num)) != 0; }
__inline bool Int64IsZero    (  INT64 num ) { return (( INT32)(num >> 32) | ( INT32)(num)) == 0; }
__inline bool Int64IsNotZero (  INT64 num ) { return (( INT32)(num >> 32) | ( INT32)(num)) != 0; }
__inline bool Int64IsNegative(  INT64 num ) { return (( INT32)(num >> 32)                ) <  0; }

__inline bool AbsoluteInt64IsLessThan( INT64 num, UINT64 range )
{
    return (num > -(INT64)range && num < (INT64)range);
}

__inline bool AbsoluteInt64IsGreaterThan( INT64 num, UINT64 range )
{
    return (num < -(INT64)range || num > (INT64)range);
}

__inline UINT32 iabs32( INT32 s ) { if(         s         < 0) s = 0 - s; return s; }
__inline UINT64 iabs64( INT64 s ) { if(((INT32)(s >> 32)) < 0) s = 0 - s; return s; }

//--//


__inline UINT64 square_sum64( INT32 x, INT32 y )
{
    return (UINT64)(((INT64)x * x) + ((INT64)y * y));
}


UINT32 ilog64     ( UINT64 a );
UINT32 ilog64_x16 ( UINT64 a );
UINT32 ilog64_x256( UINT64 a );

UINT32 isqrt64( UINT64 x );
UINT32 isqrt32( UINT32 x );

//--//

extern const UINT8 c_BitSet_Lookup[];

__inline UINT32 BitSetCount8( UINT8 Bits )
{
    return c_BitSet_Lookup[Bits];
}

__inline UINT32 BitSetCount32( UINT32 Bits )
{
    UINT32 SetCount = 0;

    while(Bits)
    {
        SetCount += BitSetCount8( (UINT8)Bits );

        Bits >>= 8;
    }

    return SetCount;
}

//--//

__inline INT32 SineCosine_Rotate( INT16 SinCos, INT32 Magnitude )
{
    Magnitude *= (INT32)SinCos;

    if(Magnitude >= 0) Magnitude += (1 << 14);
    else               Magnitude -= (1 << 14);

    return Magnitude >> 15;
}

void SineCosine_Fast   ( INT16 Angle, INT16& Sine, INT16& Cosine );
void SineCosine_Precise( INT16 Angle, INT16& Sine, INT16& Cosine );

//--//

#if !defined(BUILD_RTM)

extern "C"
{
void HARD_Breakpoint();
}

#define HARD_BREAKPOINT()     HARD_Breakpoint()

#if defined(_DEBUG)
#define DEBUG_HARD_BREAKPOINT()     HARD_Breakpoint()
#else
#define DEBUG_HARD_BREAKPOINT()
#endif

#else

#define HARD_BREAKPOINT()
#define DEBUG_HARD_BREAKPOINT()

#endif  // !defined(BUILD_RTM)

struct HAL_USART_STATE_CONFIG
{
    UINT8* TxBuffer;
    size_t TxBufferSize;

    UINT8* RxBuffer;
    size_t RxBufferSize;
};

extern HAL_USART_STATE_CONFIG Hal_Usart_State_Config[];

//--//

#if !defined(BUILD_RTM)
typedef void (*LOGGING_CALLBACK)(LPCSTR text);

void hal_fprintf_SetLoggingCallback( LOGGING_CALLBACK fpn );

extern "C"

{
void lcd_printf( const char* format, ... );
}

#else

extern "C"

{
__inline void lcd_printf( const char* format, ... ) {}
}

#endif  // !defined(BUILD_RTM)

//--//
// this locates the tinyclr.dat regardless of the build
// also see client\clr\exe\tinyclr_dat_start.c
#if defined(__RENESAS__)
extern char * TinyClr_Dat_Start;
extern char * TinyClr_Dat_End;

#else
extern char TinyClr_Dat_Start[];
extern char TinyClr_Dat_End[];
#endif
//--//
// Function macros

void HAL_Assert  ( LPCSTR Func, int Line, LPCSTR File );
// HAL_AssertEx is defined in the processor or platform selector files.
extern void HAL_AssertEx();

#if defined(PLATFORM_ARM)
    #if !defined(BUILD_RTM)
        #define       ASSERT(i)  { if(!(i)) HAL_AssertEx(); }
        #define _SIDE_ASSERTE(i) { if(!(i)) HAL_AssertEx(); }
    #endif
#else
    #if defined(_DEBUG) && !defined(_WIN32_WCE) && !defined(__RENESAS__)
#if !defined _ASSERTE
#error
#endif
        #define       ASSERT(i)  _ASSERTE(i)
        #define _SIDE_ASSERTE(i) _ASSERTE(i)
    #endif
#endif

#ifndef ASSERT
#define ASSERT(i)
#endif

#ifndef _ASSERTE
#define _ASSERTE(expr) ASSERT(expr)
#endif

#ifndef _SIDE_ASSERTE
#define _SIDE_ASSERTE(expr) (expr)
#endif

#if STATIC_ASSERT_SUPPORTED
#define CT_ASSERT_STRING( x ) #x
#define CT_ASSERT_UNIQUE_NAME(e,name)static_assert( (e), CT_ASSERT_STRING( name ) "@" __FILE__ CT_ASSERT_STRING(__LINE__) ); 
#define CT_ASSERT(e) static_assert( (e), __FILE__ CT_ASSERT_STRING(__LINE__) );
#else
// CT_ASSERT (compile-time assert) macro is used to test condition at compiler time and generate
// compiler error if condition is FALSE.
// Example: CT_ASSERT( sizeof( UINT32 ) == 2 ) would cause compilation error.
//          CT_ASSERT( sizeof( UINT32 ) == 4 ) compiles without error.
// Since this declaration is just typedef - it does not create any CPU code.
//
// Reason for CT_ASSERT_UNIQUE_NAME
// The possible problem with the macro - it creates multiple identical typedefs.
// It is not a problem in global scope, but if macro is used inside of struct - it generates warnings.
// CT_ASSERT_UNIQUE_NAME is the same in essence, but it provides a way to customize the name of the type.
#define CT_ASSERT_UNIQUE_NAME(e,name) typedef char __CT_ASSERT__##name[(e)?1:-1];
#define CT_ASSERT(e)                  CT_ASSERT_UNIQUE_NAME(e,tinyclr)
#endif
/****************************************************************************/

extern "C"
{
#if !defined(BUILD_RTM)

void debug_printf( const char *format, ... );

#else

__inline void debug_printf( const char *format, ... ) {}

#endif  // !defined(BUILD_RTM)
}
//--//

void ApplicationEntryPoint();

//--//

//These events match emulator events in Framework\Tools\Emulator\Events.cs

#define SYSTEM_EVENT_FLAG_COM_IN                    0x00000001
#define SYSTEM_EVENT_FLAG_COM_OUT                   0x00000002
#define SYSTEM_EVENT_FLAG_USB_IN                    0x00000004
#define SYSTEM_EVENT_FLAG_USB_OUT                   0x00000008
#define SYSTEM_EVENT_FLAG_SYSTEM_TIMER              0x00000010
#define SYSTEM_EVENT_FLAG_TIMER1                    0x00000020
#define SYSTEM_EVENT_FLAG_TIMER2                    0x00000040
#define SYSTEM_EVENT_FLAG_BUTTON                    0x00000080
#define SYSTEM_EVENT_FLAG_GENERIC_PORT              0x00000100
#define SYSTEM_EVENT_FLAG_UNUSED_0x00000200         0x00000200
#define SYSTEM_EVENT_FLAG_UNUSED_0x00000400         0x00000400
#define SYSTEM_EVENT_FLAG_NETWORK                   0x00000800
#define SYSTEM_EVENT_FLAG_TONE_COMPLETE             0x00001000
#define SYSTEM_EVENT_FLAG_TONE_BUFFER_EMPTY         0x00002000
#define SYSTEM_EVENT_FLAG_SOCKET                    0x00004000
#define SYSTEM_EVENT_FLAG_SPI                       0x00008000
#define SYSTEM_EVENT_FLAG_CHARGER_CHANGE            0x00010000
#define SYSTEM_EVENT_FLAG_OEM_RESERVED_1            0x00020000
#define SYSTEM_EVENT_FLAG_OEM_RESERVED_2            0x00040000
#define SYSTEM_EVENT_FLAG_IO                        0x00080000
#define SYSTEM_EVENT_FLAG_UNUSED_0x00100000         0x00100000


#if defined(PLATFORM_SH7619_NATIVE) || defined(PLATFORM_SH7619_EVB)
#define SYSTEM_EVENT_FLAG_ETHER                     0x00100000
#endif


#define SYSTEM_EVENT_FLAG_UNUSED_0x00200000         0x00200000
#define SYSTEM_EVENT_FLAG_UNUSED_0x00400000         0x00400000
#define SYSTEM_EVENT_FLAG_UNUSED_0x00800000         0x00800000
#define SYSTEM_EVENT_FLAG_UNUSED_0x01000000         0x01000000
#define SYSTEM_EVENT_FLAG_UNUSED_0x02000000         0x02000000
#define SYSTEM_EVENT_FLAG_UNUSED_0x04000000         0x04000000
#define SYSTEM_EVENT_HW_INTERRUPT                   0x08000000
#define SYSTEM_EVENT_I2C_XACTION                    0x10000000
#define SYSTEM_EVENT_FLAG_DEBUGGER_ACTIVITY         0x20000000
#define SYSTEM_EVENT_FLAG_MESSAGING_ACTIVITY        0x40000000
#define SYSTEM_EVENT_FLAG_UNUSED_0x80000000         0x80000000
#define SYSTEM_EVENT_FLAG_ALL                       0xFFFFFFFF



//--//

void SystemState_Set  ( SYSTEM_STATE NewState );
void SystemState_Clear( SYSTEM_STATE State    );
BOOL SystemState_Query( SYSTEM_STATE State    );

//--//


extern "C"
{
void SystemState_SetNoLock  ( SYSTEM_STATE State );
void SystemState_ClearNoLock( SYSTEM_STATE State );
BOOL SystemState_QueryNoLock( SYSTEM_STATE State );
}
//--//

extern "C"
{
void IDelayLoop( int iterations );
void IDelayLoop2( int iterations );
}

// this costs a minimun of 12 cycles when called from 0 wait-state RAM, 34 cycles when
// called from 2 wait-state FLASH
#define CYCLE_DELAY_LOOP(d) IDelayLoop(d)

// This routine was designed for the XScale processor with cache and branch prediction
// enabled.  It costs about 10 extra cycles to call this routine.
#define CYCLE_DELAY_LOOP2(d) IDelayLoop2(d)

//--//

#define NOP() {__asm{ NOP }}

#include "smartptr_irq.h"

//--//

#if !defined(PLATFORM_WINDOWS)
extern "C" INT32 InterlockedIncrement( volatile INT32* lpAddend );
extern "C" INT32 InterlockedDecrement( volatile INT32* lpAddend );
extern "C" INT32 InterlockedExchange( volatile INT32* Target, INT32 Value );
extern "C" INT32 InterlockedCompareExchange( INT32* Destination, INT32 Exchange, INT32 Comperand );
extern "C" INT32 InterlockedExchangeAdd( volatile INT32* Addend, INT32 Value );
extern "C" INT32 InterlockedOr( volatile INT32* Destination, INT32 Flag );
extern "C" INT32 InterlockedAnd( volatile INT32* Destination, INT32 Flag );
#endif // PLATFORM_WINDOWS

struct OpaqueQueueNode
{
    void* payload;
};

struct OpaqueListNode
{
    OpaqueListNode* next;
    void* payload;
};

template <class T> class HAL_DblLinkedList;

template <class T> class HAL_DblLinkedNode
{
    T* m_nextNode;
    T* m_prevNode;

    friend class HAL_DblLinkedList<T>;

public:
    void Initialize()
    {
        m_nextNode = NULL;
        m_prevNode = NULL;
    }

    T* Next() const { return m_nextNode; }
    T* Prev() const { return m_prevNode; }

    void SetNext( T* next ) { m_nextNode = next; }
    void SetPrev( T* prev ) { m_prevNode = prev; }

    bool IsLinked() const { return m_nextNode != NULL; }

    //--//

    void RemoveFromList()
    {
        T* next = m_nextNode;
        T* prev = m_prevNode;

        if(prev) prev->m_nextNode = next;
        if(next) next->m_prevNode = prev;
    }

    void Unlink()
    {
        T* next = m_nextNode;
        T* prev = m_prevNode;

        if(prev) prev->m_nextNode = next;
        if(next) next->m_prevNode = prev;

        m_nextNode = NULL;
        m_prevNode = NULL;
    }
};

//--//

template <class T> class HAL_DblLinkedList
{
    //
    // Logically, a list starts with a HAL_DblLinkedNode with only the Next() set and ends with a node with only Prev() set.
    // This can be collapsed to have the two nodes overlap.
    //
    T* m_first;
    T* m_null;
    T* m_last;

    //--//

public:
    void Initialize()
    {
        m_first = Tail();
        m_null  = NULL;
        m_last  = Head();
    }

    int NumOfNodes()
    {
        T*  ptr;
        T*  ptrNext;
        int num = 0;

        for(ptr = FirstNode(); (ptrNext = ptr->Next()) != NULL; ptr = ptrNext)
        {
            num++;
        }

        return num;
    }

    //--//

    T* FirstNode() const { return m_first          ; }
    T* LastNode () const { return m_last           ; }
    bool           IsEmpty  () const { return m_first == Tail(); }

    T* FirstValidNode() const { T* res = m_first; return res->Next() ? res : NULL; }
    T* LastValidNode () const { T* res = m_last ; return res->Prev() ? res : NULL; }

    T* Head() const { return (T*)((size_t)&m_first - offsetof(T, m_nextNode)); }
    T* Tail() const { return (T*)((size_t)&m_last  - offsetof(T, m_prevNode)); }

    //--//

private:

    void Insert( T* prev, T* next, T* node )
    {
        node->m_nextNode = next;
        node->m_prevNode = prev;

        next->m_prevNode = node;
        prev->m_nextNode = node;
    }

public:
#if defined(_DEBUG)
    BOOL Exists( T* searchNode )
    {
        T* node = FirstValidNode();
        while( node != NULL && node != searchNode )
        {
            if (node == node->Next())
            {
                ASSERT(FALSE);
            }
            node = node->Next();
        }
        return (node == NULL? FALSE: TRUE);
    }
#endif

    void InsertBeforeNode( T* node, T* nodeNew )
    {
        if(node && nodeNew && node != nodeNew)
        {
            nodeNew->RemoveFromList();

            Insert( node->Prev(), node, nodeNew );
        }
    }

    void InsertAfterNode( T* node, T* nodeNew )
    {
        if(node && nodeNew && node != nodeNew)
        {
            nodeNew->RemoveFromList();

            Insert( node, node->Next(), nodeNew );
        }
    }

    void LinkAtFront( T* node )
    {
        InsertAfterNode( Head(), node );
    }

    void LinkAtBack( T* node )
    {
        InsertBeforeNode( Tail(), node );
    }

    T* ExtractFirstNode()
    {
        T* node = FirstValidNode();

        if(node) node->Unlink();

        return node;
    }

    T* ExtractLastNode()
    {
        T* node = LastValidNode();

        if(node) node->Unlink();

        return node;
    }
};

//--//

template <typename T, size_t size> class Hal_Queue_KnownSize
{
    size_t m_writer;
    size_t m_reader;
    BOOL   m_full;
    T      m_data[size];

public:
    void Initialize()
    {
        m_writer = 0;
        m_reader = 0;
        m_full   = FALSE;
    }

    size_t NumberOfElements()
    {
        if(m_writer < m_reader) return size + m_writer - m_reader;
        else if(m_full)         return size;
        else                    return        m_writer - m_reader;
    }

    BOOL IsEmpty()
    {
        return (m_writer == m_reader && !m_full);
    }

    BOOL IsFull()
    {
        return m_full;
    }

    T* operator[](int index)
    {
        if(index < 0 || index >= NumberOfElements()) return NULL;

        return &m_data[(m_reader + index) % ARRAYSIZE(m_data)];
    }


    T* Push()
    {
        size_t oldWriter  = m_writer;
        size_t nextWriter = oldWriter + 1; if(nextWriter == size) nextWriter = 0;

        if(m_full) return NULL;

        if(nextWriter == m_reader)
        {
            m_full = TRUE;
        }

        m_writer = nextWriter;

        return &m_data[oldWriter];
    }

    T* Peek()
    {
        if(m_writer == m_reader && !m_full) return NULL;

        return &m_data[m_reader];
    }

    T* Pop()
    {
        size_t oldReader = m_reader;

        if(oldReader == m_writer && !m_full) return NULL;

        size_t nextReader = oldReader + 1; if(nextReader == size) nextReader = 0;

        m_reader = nextReader;

        m_full = FALSE;

        return &m_data[oldReader];
    }
};

template <typename T> class Hal_Queue_UnknownSize
{
    size_t m_writer;
    size_t m_reader;
    size_t m_size;
    BOOL   m_full;
    T*     m_data;

public:
    void Initialize( T* data, size_t size )
    {
        m_writer = 0;
        m_reader = 0;
        m_size   = size;
        m_data   = data;
        m_full   = FALSE;
    }

    size_t NumberOfElements()
    {
        if(m_writer < m_reader) return m_size + m_writer - m_reader;
        else if(m_full)         return m_size;
        else                    return m_writer - m_reader;
    }

    BOOL IsEmpty()
    {
        return (m_writer == m_reader && !m_full);
    }

    BOOL IsFull()
    {
        return m_full;
    }

    T* operator[](int index)
    {
        if(index < 0 || index >= NumberOfElements()) return NULL;

        return &m_data[(m_reader + index) % m_size];
    }

    T* Push()
    {
        size_t oldWriter  = m_writer;

        if(m_full) return NULL;

        m_writer++;  if(m_writer == m_size) m_writer = 0;

        if(m_writer == m_reader) m_full = TRUE;

        return &m_data[oldWriter];
    }

    T* Peek()
    {
        if(m_writer == m_reader && !m_full) return NULL;

        return &m_data[m_reader];
    }

    T* Pop()
    {
        size_t oldReader = m_reader;

        if(m_reader == m_writer && !m_full) return (T*)NULL;

        m_reader++;  if(m_reader == m_size) m_reader = 0;

        m_full = FALSE;

        return &m_data[oldReader];
    }

    T* Push( size_t &nElements )
    {
        size_t oldWriter  = m_writer;
        size_t max = 0;

        if(m_full || (nElements == 0))
        {
            nElements = 0;
            return NULL;
        }

        if(m_writer < m_reader) max = m_reader - m_writer;
        else                    max = m_size   - m_writer;

        nElements = (max < nElements? max: nElements);

        m_writer += nElements; if(m_writer == m_size) m_writer = 0;

        if(m_writer == m_reader) m_full = TRUE;

        return &m_data[oldWriter];
    }

    T* Pop( size_t &nElements )
    {
        size_t oldReader = m_reader;
        size_t max = 0;

        if(nElements == 0) return NULL;

        if((m_reader == m_writer) && !m_full)
        {
            nElements = 0;
            // reset the reader/writer to maximize push potential
            m_reader  = 0;
            m_writer  = 0;
            return NULL;
        }

        if(m_writer <= m_reader) max = m_size   - m_reader;
        else                     max = m_writer - m_reader;

        nElements = (max < nElements? max: nElements);

        m_reader += nElements; if(m_reader == m_size) m_reader = 0;

        m_full = FALSE;

        return &m_data[oldReader];
    }

    T* Storage() { return m_data; }
};

/***************************************************************************/

//--//

#if defined(_WIN32) || defined(WIN32)

#define HAL_TIMEWARP

#endif

#if defined(HAL_TIMEWARP)

#define TIMEWARP_DISABLE ((INT64)1 << 62)

extern int   s_timewarp_armingState;
extern INT64 s_timewarp_lastButton;
extern INT64 s_timewarp_compensate;

#endif

//--//

#if defined(PLATFORM_ARM) || defined(PLATFORM_SH)

extern int HeapBegin;
extern int HeapEnd;
extern int CustomHeapBegin;
extern int CustomHeapEnd;
extern int StackBottom;
extern int StackTop;

#if !defined(BUILD_RTM)

// Registers[n] == Rn in ARM terms, R13=sp, R14=lr, R15=pc
typedef void (*AbortHandlerFunc)(UINT32 cpsr, UINT32 Registers[16]);

extern "C"
{
void StackOverflow( UINT32 sp );

void NULL_Pointer_Write();
}

UINT32 Stack_MaxUsed();

#endif

#endif  // defined(PLATFORM_ARM)

//--//

// Simple Heap is for use by Porting Kit users who need private memory allocation.
/*************************************************************************************
**
** Function: SimpleHeap_Allocate
**
** Synopsis: Initializes simple heap from supplied buffer.
** Pointer to buffer is saved in global variable.
** Later is used for allocation of blocks by SimpleHeap_Allocate
**
** Arguments: [pHeapBuffer] - Pointer to heap buffer. This pointer is saved in global variable,
**                            later used by SimpleHeap_* function.
**            [pHeapBuffer] - Size of memory block pointed by pHeapBuffer
**
**************************************************************************************/
void SimpleHeap_Initialize( void* pHeapBuffer, UINT32 heapBuufferSize );

/**********************************************************************
**
** Function: SimpleHeap_Allocate
**
** Synopsis: Allocates block of memory from heap buffer initialized by SimpleHeap_Initialize
**
**
** Arguments: [len]                  - Size of block to allocate.
**
** Returns:   Pointer to newly allocated memory
              or NULL if there is no free memory to accomodate block of size len
**********************************************************************/
void* SimpleHeap_Allocate   ( size_t len );

/**********************************************************************
**
** Function: SimpleHeap_Release
**
** Synopsis: Releases memory block allocated by SimpleHeap_Allocate
**
**
** Arguments: [pHeapBlock] - Memory block to release.
**
**********************************************************************/
void  SimpleHeap_Release    ( void*  pHeapBlock );


/**********************************************************************
**
** Function: SimpleHeap_ReAllocate
**
** Synopsis: Reallocates memory on an existing pointer and copies bck the
** data
**
** Arguments: [pHeapBlock] - Memory block to reallocate.
** Arguments: [len]        - Size of block to allocate.
**
**********************************************************************/
void* SimpleHeap_ReAllocate( void*  pHeapBlock, size_t len );

/**********************************************************************
**
** Function: SimpleHeap_IsAllocated
**
** Synopsis: Checks if pHeapBlock points to memory block allocated by SimpleHeap_Allocate
**
** Arguments: [pHeapBlock] - Memory block to release.
**
** Returns:   TRUE if pHeapBlock points to memory allocated, FALSE otherwise.
**********************************************************************/
BOOL  SimpleHeap_IsAllocated( void*  pHeapBlock );

/**********************************************************************
**
** Function: HAL_Init_Custom_Heap
**
** Synopsis: Initializes simple heap with memory buffer provided by CustomHeapLocation function.
**
**********************************************************************/
inline void HAL_Init_Custom_Heap()
{
    UINT8* BaseAddress = 0;
    UINT32 SizeInBytes = 0;

    // Retrieve location for Custom Heap. The location is defined in scatter file.
    CustomHeapLocation( BaseAddress, SizeInBytes );

    // Initialize custom heap with heap block returned from CustomHeapLocation
    SimpleHeap_Initialize( BaseAddress, SizeInBytes );
}


//--//

// hal cleanup for CLR reboot

void HAL_Initialize();
void HAL_Uninitialize();

void HAL_EnterBooterMode();

typedef void (*ON_SOFT_REBOOT_HANDLER)(void);

void HAL_AddSoftRebootHandler(ON_SOFT_REBOOT_HANDLER handler);

//--//


//
// This has to be extern "C" because the Crypto library has C-linkage.
//
extern "C" {

void* private_malloc ( size_t len             );
void  private_free   ( void*  ptr             );
void* private_realloc( void*  ptr, size_t len );

}

template <typename T> __inline void private_release( T& ref )
{
    T ptr = ref;

    if(ptr)
    {
        ref = NULL;

        private_free( ptr );
    }
}

//--//

__inline void* ReAllocate_NotImplemented( void * ptr, size_t len ) { ASSERT(FALSE); return NULL; }

//--//

#define HAL_DECLARE_CUSTOM_HEAP(allocFtn,freeFtn,reallocFtn)                           \
    extern "C" {                                                                       \
    void* private_malloc ( size_t len             ) { return allocFtn  ( len      ); } \
    void  private_free   ( void*  ptr             ) {        freeFtn   ( ptr      ); } \
    void* private_realloc( void*  ptr, size_t len ) { return reallocFtn( ptr, len ); } \
    }

#define HAL_DECLARE_NULL_HEAP()                                      \
    extern "C" {                                                     \
    void* private_malloc ( size_t len             ) { return NULL; } \
    void  private_free   ( void*  ptr             ) {              } \
    void* private_realloc( void * ptr, size_t len ) { return NULL; } \
    }


//--//

extern UINT32 LOAD_IMAGE_Start;
extern UINT32 LOAD_IMAGE_Length;
extern UINT32 LOAD_IMAGE_CRC;
extern UINT32 LOAD_IMAGE_CalcCRC;


#if !defined(BUILD_RTM)

UINT32 Checksum_RAMConstants();

// prototype this as a continuation capable function

void Verify_RAMConstants( void* arg );

#endif  // !defined(BUILD_RTM)

//--//

struct SECTOR_BIT_FIELD
{
    static const UINT32 c_MaxSectorCount = 287; // pxa271 has 259 sectors, 287 == 9 * sizeof(UINT32) - 1, which is the next biggest whole
    static const UINT32 c_MaxFieldUnits  = (c_MaxSectorCount + 1) / (8 * sizeof(UINT32)); // bits

    volatile     UINT32 BitField[c_MaxFieldUnits];
};

struct TINYBOOTER_KEY_CONFIG
{
    UINT8  SectorKey[260]; //RSAKey 4 bytes (exponent) + 128 bytes (module) + 128 bytes (exponent)
};

struct CONFIG_SECTOR_VERSION
{
    UINT8 Major;
    UINT8 Minor;
    UINT8 TinyBooter;
    UINT8 Extra;
};

struct SECTOR_BIT_FIELD_TB
{
    static const UINT32 c_MaxBitCount    = 8640;
    static const UINT32 c_MaxFieldUnits  = (c_MaxBitCount / (8 * sizeof(UINT32))); // bits

    volatile     UINT32 BitField[c_MaxFieldUnits];
};

struct ConfigurationSector
{
    static const UINT32 c_BackwardsCompatibilityBufferSize = 88;
    static const UINT32 c_MaxBootEntryFlags   = 50;
    static const UINT32 c_MaxSignatureCount   = 8;
    static const UINT32 c_BootEntryKey        = ('B' << 24 | 'T' << 16 | 'L' << 8 | 'D');
    static const UINT32 c_DeployKeyCount      = 2;
    static const  INT32 c_DeployKeyFirmware   = 0;
    static const  INT32 c_DeployKeyDeployment = 1;

    static const  UINT8 c_CurrentVersionMajor      = 3;
    static const  UINT8 c_CurrentVersionMinor      = 0;
    static const  UINT8 c_CurrentVersionTinyBooter = 4;

    UINT32                ConfigurationLength;

    CONFIG_SECTOR_VERSION Version;

    UINT8                 Buffer[c_BackwardsCompatibilityBufferSize]; // for backwards compatibility - keep booterflagarray and signature check at same index

    UINT32                BooterFlagArray[c_MaxBootEntryFlags];

    SECTOR_BIT_FIELD      SignatureCheck[c_MaxSignatureCount]; // 8 changes before erase

    TINYBOOTER_KEY_CONFIG DeploymentKeys[c_DeployKeyCount];

    OEM_MODEL_SKU         OEM_Model_SKU;

    OEM_SERIAL_NUMBERS    OemSerialNumbers;

    SECTOR_BIT_FIELD_TB   CLR_ConfigData;

    HAL_CONFIG_BLOCK      FirstConfigBlock;
};

//
// This structure is never used, its purpose is to generate a compiler error in case the size of any structure changes.
//
struct ConfigurationSector_CompileCheck
{
    char buf2[offsetof(ConfigurationSector,OEM_Model_SKU   ) % sizeof(FLASH_WORD) == 0 ? 1 : -1];
    char buf3[offsetof(ConfigurationSector,OemSerialNumbers) % sizeof(FLASH_WORD) == 0 ? 1 : -1];
    char buf5[offsetof(ConfigurationSector,FirstConfigBlock) % sizeof(FLASH_WORD) == 0 ? 1 : -1];
    char buf6[offsetof(ConfigurationSector,BooterFlagArray ) ==  96 ? 1 : -1];  // The offset must be preserved for backwards compatibility with Tinybooter
    char buf7[offsetof(ConfigurationSector,SignatureCheck  ) == 296 ? 1 : -1];  // The offset must be preserved for backwards compatibility with Tinybooter
    char buf8[offsetof(ConfigurationSector,DeploymentKeys  ) == 584 ? 1 : -1];  // The offset must be preserved for backwards compatibility with Tinybooter
};

extern OEM_MODEL_SKU             OEM_Model_SKU;


#if defined(PLATFORM_SH)
// Renesas needs this to know the g_ConfigurationSector goes to, otherwise it will not able to put it 
// at the  corresponding section even this is the "extern" declaration.
#pragma section SectionForConfig
#endif
extern const ConfigurationSector g_ConfigurationSector;
#if defined(PLATFORM_SH)
#pragma section
#endif 
//--//

#if defined(PLATFORM_ARM) || defined(PLATFORM_BLACKFIN) || defined(PLATFORM_WINDOWS) ||defined(PLATFORM_SH)

#if !defined(BUILD_RTM)

#define DEBUG_TRACE0(t, s)                              if(((t) & DEBUG_TRACE) != 0) hal_printf( (s)                                               )
#define DEBUG_TRACE1(t, s, p1)                          if(((t) & DEBUG_TRACE) != 0) hal_printf( (s), (p1)                                         )
#define DEBUG_TRACE2(t, s, p1,p2)                       if(((t) & DEBUG_TRACE) != 0) hal_printf( (s), (p1),(p2)                                    )
#define DEBUG_TRACE3(t, s, p1,p2,p3)                    if(((t) & DEBUG_TRACE) != 0) hal_printf( (s), (p1),(p2),(p3)                               )
#define DEBUG_TRACE4(t, s, p1,p2,p3,p4)                 if(((t) & DEBUG_TRACE) != 0) hal_printf( (s), (p1),(p2),(p3),(p4)                          )
#define DEBUG_TRACE5(t, s, p1,p2,p3,p4,p5)              if(((t) & DEBUG_TRACE) != 0) hal_printf( (s), (p1),(p2),(p3),(p4),(p5)                     )
#define DEBUG_TRACE6(t, s, p1,p2,p3,p4,p5,p6)           if(((t) & DEBUG_TRACE) != 0) hal_printf( (s), (p1),(p2),(p3),(p4),(p5),(p6)                )
#define DEBUG_TRACE7(t, s, p1,p2,p3,p4,p5,p6,p7)        if(((t) & DEBUG_TRACE) != 0) hal_printf( (s), (p1),(p2),(p3),(p4),(p5),(p6),(p7)           )
#define DEBUG_TRACE8(t, s, p1,p2,p3,p4,p5,p6,p7,p8)     if(((t) & DEBUG_TRACE) != 0) hal_printf( (s), (p1),(p2),(p3),(p4),(p5),(p6),(p7),(p8)      )
#define DEBUG_TRACE9(t, s, p1,p2,p3,p4,p5,p6,p7,p8,p9)  if(((t) & DEBUG_TRACE) != 0) hal_printf( (s), (p1),(p2),(p3),(p4),(p5),(p6),(p7),(p8),(p9) )

#else

#define DEBUG_TRACE0(t, s)
#define DEBUG_TRACE1(t, s, p1)
#define DEBUG_TRACE2(t, s, p1,p2)
#define DEBUG_TRACE3(t, s, p1,p2,p3)
#define DEBUG_TRACE4(t, s, p1,p2,p3,p4)
#define DEBUG_TRACE5(t, s, p1,p2,p3,p4,p5)
#define DEBUG_TRACE6(t, s, p1,p2,p3,p4,p5,p6)
#define DEBUG_TRACE7(t, s, p1,p2,p3,p4,p5,p6,p7)
#define DEBUG_TRACE8(t, s, p1,p2,p3,p4,p5,p6,p7,p8)
#define DEBUG_TRACE9(t, s, p1,p2,p3,p4,p5,p6,p7,p8,p9)

#endif  // defined(_DEBUG)

#endif

//--//

struct CPU_UTILIZATION_TIME
{
    INT64   Boot_RTC_Ticks;
    INT64   ISR_RTC_Ticks;
    INT64   Sleep_RTC_Ticks;
    INT64   Spinning_RTC_Ticks;
    INT64   Callback_RTC_Ticks;
    INT64   Cont_RTC_Ticks;
    INT64   GFX_CLR_Ticks;
    INT64   GFX_HAL_Ticks;
    INT64   MAC_ARCTAN_RTC_Ticks;
    INT64   VITERBI_RTC_Ticks;
    INT32   MAC_ARCTAN_IRQs;
    INT32   VITERBI_IRQs;
};

//--//

#include <..\Initialization\MasterConfig.h>

#ifdef PLATFORM_DEPENDENT__UPDATE_SIGNATURE_SIZE
#define HAL_UPDATE_SIGNATURE_SIZE PLATFORM_DEPENDENT__UPDATE_SIGNATURE_SIZE
#else
#define HAL_UPDATE_SIGNATURE_SIZE 4
#endif

#define HAL_UPDATE_CONFIG_SIGN_TYPE__SIGNATURE 0x0000
#define HAL_UPDATE_CONFIG_SIGN_TYPE__CRC       0x0001
#define HAL_UPDATE_CONFIG_SIGN_TYPE__USER_DEF  0x8000

struct HAL_UPDATE_CONFIG
{
    HAL_DRIVER_CONFIG_HEADER Header;

    UINT32 UpdateID;
    UINT32 UpdateSignType;
    UINT32 UpdateSignature[(HAL_UPDATE_SIGNATURE_SIZE+sizeof(UINT32)-1)/sizeof(UINT32)];

    static LPCSTR GetDriverName() { return "BTLD"; }
};

//--//

extern bool g_fDoNotUninitializeDebuggerPort;

//--//

#include <network_decl.h>
#include <tinypal.h>
#include <drivers.h>
#include <tinybooterentry.h>

//--//

#endif  // _TINYHAL_H_
