////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _TINYCLR_DIAGNOSTICS_H_
#define _TINYCLR_DIAGNOSTICS_H_

////////////////////////////////////////////////////////////////////////////////////////////////////

#include <TinyCLR_Runtime.h>
#include <TinyCLR_Graphics.h>

//--//

#define MARKER_LOG_V1 "MSlogV2" // Used to identify the start of a log in flash.

struct CLR_DIAG_LogHandler
{
    struct LogHeader
    {
        char m_marker[ 8 ];
        int  m_buildCRC;
        int  m_sequenceNumber;
        int  m_nextSector;
        int  m_numberOfRecords;

        int  m_CRC;
        int  m_totalLength;
    };

    struct RecordHeader
    {
        int m_CRC;
        int m_length;
    };

    struct RecordType
    {
        static const char c_SpecialObjectMarker = 0;
        static const char c_Type_Object         = 0;
        static const char c_Type_String         = 1;
        static const char c_Type_Exception      = 2;

        char m_id[ sizeof(UINT32) ];
    };

    struct RecordException
    {
        CLR_RT_TypeDef_Index m_td;
        CLR_UINT32           m_hr;
        CLR_UINT32           m_depth;
    };

    //--//

    struct PendingRecord : CLR_RT_HeapBlock_Node
    {
        RecordType m_type;
        size_t     m_length;
        CLR_UINT8  m_data[ 1 ];
    };

    //--//

    static const size_t c_SizeOfLogHeader    = CONVERTFROMSIZETOELEMENTS(sizeof(LogHeader   ),FLASH_WORD);
    static const size_t c_SizeOfRecordHeader = CONVERTFROMSIZETOELEMENTS(sizeof(RecordHeader),FLASH_WORD);

    static const size_t c_MaxRecordLength = 10*1024;
    static const size_t c_MaxBytesInUse   = 20*1024;

    HAL_COMPLETION         m_completion;
    CLR_RT_DblLinkedList   m_records;
    CLR_UINT32             m_bytesInUse;

    LogHeader              m_template;

    int                    m_FLASH_firstSector;
    int                    m_FLASH_lastSector;
    FLASH_WORD*            m_FLASH_ptr;
    FLASH_WORD*            m_FLASH_end;

    int                    m_recnum_Committed;
    int                    m_recnum_Pending;
    int                    m_sequenceNumber;

    //--//

    HRESULT Initialize           (           const char marker[ 8 ]                    );
    void    Uninitialize         (                                                     );
    HRESULT Erase                (           bool fIgnoreMarker                        );
    void    EraseCorruptedSectors(                                                     );

    HRESULT AppendString         (           LPCSTR            szText                  );
    HRESULT AppendException      (           CLR_RT_HeapBlock& object                  );
    HRESULT AppendRecord         (           CLR_RT_HeapBlock& object                  );
    HRESULT AppendCpuAttribution ( const CLR_RT_MethodDef_Index* owner                 );
    HRESULT GetNumberOfRecords   ( int& num                                            );
    HRESULT GetRecord            ( int  num, CLR_RT_HeapBlock& object                  );
    bool    GetRecord            ( int  num, CLR_UINT8*& dataPtr, CLR_UINT32& dataSize );

private:
    static const CLR_UINT32 c_SearchSlot       = 0x00000001;
    static const CLR_UINT32 c_InitializeSector = 0x00000002;
    static const CLR_UINT32 c_EraseSector      = 0x00000004;

    void    RevertSlotAllocation(                   CLR_UINT32 length                                                                      );
    HRESULT FindEmptySlot       ( CLR_UINT32 steps, CLR_UINT32 length, const ByteAddress*& sect, FLASH_WORD*& header, FLASH_WORD*& payload );

    HRESULT InitializeSector( const ByteAddress* sect, int sequenceNumber );

    FLASH_WORD* AppendDataToRecord( RecordHeader& rec, FLASH_WORD* payload, CLR_UINT8* data, CLR_UINT32 length );
    FLASH_WORD* CloseRecord       ( RecordHeader& rec, FLASH_WORD* header                                      );

    FLASH_WORD* WriteData   ( FLASH_WORD* dst, CLR_UINT8* src, CLR_UINT32 length );
    FLASH_WORD* RepairSector( FLASH_WORD* start, FLASH_WORD* end );

    //--//

    static void Callback( void* arg );

    void EnqueueNextCallback();

    HRESULT AdvanceState();

    CLR_UINT32 ComputeWriteTime( CLR_UINT32 len );
    CLR_UINT32 ComputeEraseTime(                );

    void QueueRecord( int type, size_t len, const void* data, PendingRecord** res = NULL );

    PendingRecord* AllocateMemoryForRecord( size_t         length );
    void           ReleaseMemoryForRecord ( PendingRecord* ptr    );
};

//--//

#endif // _TINYCLR_DIAGNOSTICS_H_
