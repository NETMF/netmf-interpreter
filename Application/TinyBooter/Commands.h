////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _COMMANDS_H_
#define _COMMANDS_H_

////////////////////////////////////////////////////////////////////////////////////////////////////

#include <TinyCLR_Types.h>

#include <WireProtocol.h>

#define LOADER_ENGINE_ISFLAGSET(x, y)  (y == ((x)->m_flags & y))
#define LOADER_ENGINE_SETFLAG(x, y)    (x)->m_flags |=  y
#define LOADER_ENGINE_CLEARFLAG(x, y)  (x)->m_flags &= ~y

////////////////////////////////////////////////////////////////////////////////////////////////////

struct Buffer
{
    BYTE* m_array;
    int   m_offset;
    int   m_count;
};

//--//

struct MemoryRange
{
    UINT8* m_location;
    UINT32 m_size;

    MemoryRange( UINT8* location, UINT32 size )
    {
        m_location = location;
        m_size     = size;
    }

    bool LimitToRange( MemoryRange& filtered, UINT8* address, UINT32 length ) const
    {
        UINT8* addressEnd  = address    + length;
        UINT8* locationEnd = m_location + m_size;
    
        if(addressEnd  <= m_location) return false;
        if(locationEnd <= address   ) return false;

        if(address    < m_location ) address    = m_location;
        if(addressEnd > locationEnd) addressEnd = locationEnd;
    
    
        filtered.m_location =          address;
        filtered.m_size     = (UINT32)(addressEnd - address);
    
        return true;
    }
};

//--//

typedef void (*ApplicationStartAddress)();

struct Loader_Engine
{
    typedef bool (Loader_Engine::*CommandHandler)( WP_Message* msg );

    struct CommandHandlerLookup
    {
        UINT32         cmd;
        CommandHandler hnd;
    };

    //--//

    class SignedDataState
    {
    private:
        UINT32 m_dataAddress;
        UINT32 m_dataLength;
        UINT32 m_sectorType;
        BlockStorageDevice *m_pDevice ;

    public:
        bool CheckDirty          (                                 );
        void Reset               (                                 );
        void EraseMemoryAndReset (                                 );
        bool VerifyContiguousData( UINT32 address  , UINT32 length );
        bool VerifySignature     ( UINT8* signature, UINT32 length, UINT32 keyIndex );
    };

    //--//

    COM_HANDLE    m_port;
    UINT8         m_receptionBuffer[ 2048 ];
    UINT32        m_flags;
    UINT32        m_lastPacketSequence;
    WP_Controller m_controller;

    TINYBOOTER_KEY_CONFIG m_deployKeyConfig;

    static const UINT32  c_LoaderEngineFlag_ReceptionBufferInUse = 0x00000001;
    static const UINT32  c_LoaderEngineFlag_ValidConnection      = 0x00000002;

    //--//

    SignedDataState m_signedDataState;

    //--//

    HRESULT Initialize( COM_HANDLE port );

    void ProcessCommands(                          );
    void SendTextMessage( char* buffer, int length );

    void ReplyToCommand( WP_Message* msg, bool fSuccess, bool fCritical, void* ptr, int size );
    void ReplyToCommand( WP_Message* msg, bool fSuccess, bool fCritical                      );
                                                              
    static bool Phy_ReceiveBytes   ( void* state, UINT8* & ptr, UINT32 & size );
    static bool Phy_TransmitMessage( void* state, const WP_Message* msg     );

    static bool App_ProcessHeader ( void* state, WP_Message* msg );
    static bool App_ProcessPayload( void* state, WP_Message* msg );
    static bool App_Release       ( void* state, WP_Message* msg );

private:

    bool TransmitMessage( const WP_Message* msg, bool fQueue );

    bool ProcessHeader             ( WP_Message* msg );
    bool ProcessPayload            ( WP_Message* msg );
    
    void Launch                    ( ApplicationStartAddress startAddress );

public:
    bool Monitor_Ping              ( WP_Message* msg );
    bool Monitor_OemInfo           ( WP_Message* msg );
    bool Monitor_Reboot            ( WP_Message* msg );
    
    bool Monitor_ReadMemory        ( WP_Message* msg );
    bool Monitor_WriteMemory       ( WP_Message* msg );
    bool Monitor_CheckMemory       ( WP_Message* msg );
    bool Monitor_EraseMemory       ( WP_Message* msg );
    bool Monitor_Execute           ( WP_Message* msg );
    bool Monitor_MemoryMap         ( WP_Message* msg );
    bool Monitor_CheckSignature    ( WP_Message* msg );
    bool Monitor_FlashSectorMap    ( WP_Message* msg );
    bool Monitor_SignatureKeyUpdate( WP_Message* msg );
    bool EnumerateAndLaunch        (                 );

#if defined(BIG_ENDIAN)
public:     
    void    SwapDebuggingValue ( UINT8* &msg, UINT32 size                          );
    void    SwapEndian         ( WP_Message* msg, void* ptr, int size, bool fReply );
    UINT32  SwapEndianPattern  ( UINT8* &buffer, UINT32 size, UINT32 count=1       );
#endif

};
    
#endif // _COMMANDS_H_
