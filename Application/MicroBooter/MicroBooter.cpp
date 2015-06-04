//-----------------------------------------------------------------------------
//
//  <No description>
//
//  Microsoft dotNetMF Project
//  Copyright ©2004 Microsoft Corporation
//  One Microsoft Way, Redmond, Washington 98052-6399 U.S.A.
//  All rights reserved.
//  MICROSOFT CONFIDENTIAL
//
//-----------------------------------------------------------------------------

#include <MicroBooter_decl.h>
#include <MFUpdate_decl.h>
#include <Drivers\MFUpdate\Storage\BlockStorageUpdate.h>
//#include <Crypto.h>
#include "SrecProcessor.h"

BOOL MemStreamSeekBlockAddress( BlockStorageStream &stream, UINT32 address );

#ifndef MICROBOOTER_NO_SREC_PROCESSING
static SREC_Handler g_SREC;
#endif

HAL_DECLARE_CUSTOM_HEAP( SimpleHeap_Allocate, SimpleHeap_Release, SimpleHeap_ReAllocate );

#pragma arm section zidata = "s_SystemStates"
static INT32 s_SystemStates[SYSTEM_STATE_TOTAL_STATES];
#pragma arm section zidata

BOOL SystemState_Query( SYSTEM_STATE State )
{
    GLOBAL_LOCK(x);
    return s_SystemStates[State];
}
void SystemState_Set  ( SYSTEM_STATE State )
{
    GLOBAL_LOCK(x);
    s_SystemStates[State]++;
}
void SystemState_Clear( SYSTEM_STATE State )
{
    GLOBAL_LOCK(x);
    s_SystemStates[State]--;
}
void SystemState_SetNoLock  ( SYSTEM_STATE State )
{
    s_SystemStates[State]++;
}
void SystemState_ClearNoLock( SYSTEM_STATE State )
{
    s_SystemStates[State]--;
}
BOOL SystemState_QueryNoLock( SYSTEM_STATE State )
{
    return s_SystemStates[State];
}

#if !defined(BUILD_RTM)
void debug_printf( const char* format, ... )
{
}

void lcd_printf( const char* format, ... )
{
}
#endif


//--//
// this is the first C function called after bootstrapping ourselves into ram

// these define the region to zero initialize
extern UINT32 Image$$ER_RAM_RW$$ZI$$Base;
extern UINT32 Image$$ER_RAM_RW$$ZI$$Length;

// here is the execution address/length of code to move from FLASH to RAM
#define IMAGE_RAM_RO_BASE   Image$$ER_RAM_RO$$Base
#define IMAGE_RAM_RO_LENGTH Image$$ER_RAM_RO$$Length

extern UINT32 IMAGE_RAM_RO_BASE;
extern UINT32 IMAGE_RAM_RO_LENGTH;

// here is the execution address/length of data to move from FLASH to RAM
extern UINT32 Image$$ER_RAM_RW$$Base;
extern UINT32 Image$$ER_RAM_RW$$Length;

// here is the load address of the RAM code/data
#define LOAD_RAM_RO_BASE Load$$ER_RAM_RO$$Base

extern UINT32 LOAD_RAM_RO_BASE;
extern UINT32 Load$$ER_RAM_RW$$Base;

//--//

#if defined(TARGETLOCATION_RAM)

extern UINT32 Load$$ER_RAM$$Base;
extern UINT32 Image$$ER_RAM$$Length;

#elif defined(TARGETLOCATION_FLASH)

extern UINT32 Load$$ER_FLASH$$Base;
extern UINT32 Image$$ER_FLASH$$Length;

#else
    !ERROR
#endif

void HAL_AddSoftRebootHandler(ON_SOFT_REBOOT_HANDLER handler)
{
}

#pragma arm section code = "SectionForBootstrapOperations"

static void __section("SectionForBootstrapOperations") Prepare_Copy( volatile UINT32* src, volatile UINT32* dst, UINT32 len )
{
    if(dst != src)
    {
        INT32 extraLen = len & 0x00000003;
        len            = len & 0xFFFFFFFC;
        
        while(len != 0)
        {
            *dst++ = *src++;

            len -= 4;
        }

        // thumb2 code can be multiples of 2...

        UINT8 *dst8 = (UINT8*) dst, *src8 = (UINT8*) src;

        while (extraLen > 0)
        {
            *dst8++ = *src8++;

            extraLen--;
        }
    }
}

static void __section("SectionForBootstrapOperations") Prepare_Zero( volatile UINT32* dst, UINT32 len )
{
    INT32 extraLen = len & 0x00000003;
    len            = len & 0xFFFFFFFC;

    while(len != 0)
    {
        *dst++ = 0;

        len -= 4;
    }

    // thumb2 code can be multiples of 2...

    UINT8 *dst8 = (UINT8*) dst;

    while (extraLen > 0)
    {
        *dst8++ = 0;

        extraLen--;
    }
}

void __section("SectionForBootstrapOperations") PrepareImageRegions()
{
    //
    // Copy RAM RO regions into proper location.
    //
    {
        volatile UINT32* src = (volatile UINT32*)&LOAD_RAM_RO_BASE; 
        volatile UINT32* dst = (volatile UINT32*)&IMAGE_RAM_RO_BASE;
        UINT32           len = (         UINT32 )&IMAGE_RAM_RO_LENGTH; 

        Prepare_Copy( src, dst, len );
    }


    //
    // Copy RAM RW regions into proper location.
    //
    {
        volatile UINT32* src = (volatile UINT32*)&Load$$ER_RAM_RW$$Base; 
        volatile UINT32* dst = (volatile UINT32*)&Image$$ER_RAM_RW$$Base;
        UINT32           len = (         UINT32 )&Image$$ER_RAM_RW$$Length; 

        Prepare_Copy( src, dst, len );
    }



    //
    // Initialize RAM ZI regions.
    //
    {
        volatile UINT32* dst = (volatile UINT32*)&Image$$ER_RAM_RW$$ZI$$Base;
        UINT32  len = (UINT32 )&Image$$ER_RAM_RW$$ZI$$Length;

        Prepare_Zero( dst, len );
    }
}

#pragma arm section code


static UINT8 s_WriteBuffer[512];
static INT32 s_WriteBufferIndex = 0;

static UINT8* s_ReadBuffer = NULL;

static BlockStorageStream            s_MemoryStreamDst;
static const IUpdateStorageProvider* s_MemoryStreamSrc;
static INT32                         s_MemoryStreamSrcOffset = 0;
static INT32                         s_MemoryStreamSrcHandle = -1;
static UINT32                        s_MemoryStreamSrcEraseSize = 0;

// so far we are assuming a one byte read/write in sequential order (as required by compression/decompression)
static BOOL Memory_Write( UINT32 address, UINT32 length, const BYTE* data )
{
    ASSERT(length == 1);

    s_WriteBuffer[s_WriteBufferIndex] = *data;

    s_WriteBufferIndex += length;
    
    if(s_WriteBufferIndex < sizeof(s_WriteBuffer))
    {
        return TRUE;
    }

    s_WriteBufferIndex = 0;

    if(s_MemoryStreamDst.CurrentUsage == BlockUsage::BOOTSTRAP) return FALSE;

    if(0 == (s_MemoryStreamDst.CurrentIndex % s_MemoryStreamDst.BlockLength))
    {
        s_MemoryStreamDst.Erase( s_MemoryStreamDst.BlockLength );
    }

    return s_MemoryStreamDst.Write( (BYTE*)s_WriteBuffer, sizeof(s_WriteBuffer) );
}

static BOOL Memory_Flush()
{
    if(s_WriteBufferIndex > 0)
    {
        BOOL fRet = s_MemoryStreamDst.Write( (BYTE*)s_WriteBuffer, s_WriteBufferIndex );

        s_WriteBufferIndex = 0;

        return fRet;
    }

    return TRUE;
}

static BOOL Memory_Read( UINT32 address, UINT32 length, BYTE* data )
{
    INT32 readBufferSize = 512;
    INT32 index = (address % readBufferSize);

    ASSERT(length == 1);
    
    if(s_ReadBuffer == NULL)
    {
        s_ReadBuffer = (UINT8*)private_malloc(readBufferSize);

        ASSERT(s_ReadBuffer != NULL);
        if(s_ReadBuffer == NULL) return FALSE;

        if(0 != index)
        {
            s_MemoryStreamSrc->Read( s_MemoryStreamSrcHandle, s_MemoryStreamSrcOffset - index, s_ReadBuffer, readBufferSize );

            s_MemoryStreamSrcOffset += readBufferSize - index;
        }
    }

    if(s_ReadBuffer != NULL)
    {
        if(0 == index)
        {
            if(readBufferSize <= ((UINT32)&HeapEnd - (UINT32)&HeapBegin))
            {
                s_MemoryStreamSrc->Read( s_MemoryStreamSrcHandle, s_MemoryStreamSrcOffset, s_ReadBuffer, readBufferSize );

                s_MemoryStreamSrcOffset += readBufferSize;
            }
            else
            {
                return s_MemoryStreamSrc->Read( s_MemoryStreamSrcHandle, s_MemoryStreamSrcOffset++, data, length );
            }
        } 

        *data = s_ReadBuffer[index];

        return TRUE;
    }
    
    return s_MemoryStreamSrc->Read(s_MemoryStreamSrcHandle, s_MemoryStreamSrcOffset++, data, length );
}

BOOL MemStreamSeekBlockAddress( BlockStorageStream &stream, UINT32 address )
{
    if(!stream.Initialize( BlockUsage::ANY )) return FALSE;

    while(!(stream.BaseAddress <= address && address < (stream.BaseAddress + stream.Length)))
    {
        if(!stream.NextStream()) return FALSE;
    }

    while(!(stream.CurrentAddress() <= address && address < (stream.CurrentAddress() + stream.BlockLength)))
    {
        if(!stream.Seek( BlockStorageStream::STREAM_SEEK_NEXT_BLOCK, BlockStorageStream::SeekCurrent ))
        {
            return FALSE;
        }
    }

    return TRUE;
}

extern "C"
{

BOOL MicroBooter_PerformSigCheck(MFUpdateHeader& header, INT32 storageHandle, HAL_UPDATE_CONFIG cfg)
{
    if(cfg.UpdateSignType == HAL_UPDATE_CONFIG_SIGN_TYPE__CRC)
    {
        UINT32 offset = 0;
        UINT8 buf[512];
        INT32 len = sizeof(buf);
        UINT32 crc = 0;
        
        while(offset < header.UpdateSize)
        {
            if((offset + len) > header.UpdateSize)
            {
                len = header.UpdateSize - offset;
            }

            s_MemoryStreamSrc->Read( storageHandle, offset, buf, len );

            crc = SUPPORT_ComputeCRC(buf, len, crc);

            offset += len;
        }

        return crc == *(UINT32*)cfg.UpdateSignature;
    }
    
    return TRUE;
}

BOOL MicroBooter_Install(HAL_UPDATE_CONFIG updateCfg)
{
    MFUpdateHeader header;
    UINT32 src = 0, srcEnd;
    UINT32 dest, destEnd;
    CompressedImage_Header hdr;

    for(INT32 i=0; i<g_MicroBooter_UpdateStorageListCount; i++)
    {
        s_MemoryStreamSrc = g_MicroBooter_UpdateStorageList[i];
    
        s_MemoryStreamSrcHandle = s_MemoryStreamSrc->Open(updateCfg.UpdateID, MFUPDATE_UPDATETYPE_FIRMWARE, MFUPDATE_UPDATESUBTYPE_ANY);

        if(s_MemoryStreamSrcHandle != -1) break;
    }

    if(s_MemoryStreamSrcHandle == -1) return FALSE;

    s_MemoryStreamSrcOffset = sizeof(CompressedImage_Header);

    s_MemoryStreamSrcEraseSize = s_MemoryStreamSrc->GetEraseSize( s_MemoryStreamSrcHandle );
    
    if(!s_MemoryStreamSrc->GetHeader(s_MemoryStreamSrcHandle, &header)) return FALSE;

    if(!MicroBooter_PerformSigCheck(header, s_MemoryStreamSrcHandle, updateCfg)) return FALSE;

    while(src < header.UpdateSize)
    {
        if(!s_MemoryStreamSrc->Read(s_MemoryStreamSrcHandle, src, (UINT8*)&hdr, sizeof(hdr))) return FALSE;

        srcEnd = hdr.Compressed;
        dest = 0; 
        destEnd = hdr.Uncompressed;

        if(!MemStreamSeekBlockAddress( s_MemoryStreamDst, (UINT32)hdr.Destination )) return FALSE;
        
        if( -1 == LZ77_Decompress((UINT8*)src, srcEnd, (UINT8*)dest, destEnd, Memory_Write, Memory_Read)) return FALSE;

        Memory_Flush();

        src += hdr.Compressed + sizeof(hdr);

        if(0 != (src % 4))
        {
            src += (4 - (src % 4));
        }
    }

    return TRUE;
}


typedef void (*FIRMWARE_UPDATE_ENTRYPOINT)(CompressedImage_Header*, UINT32, BOOL);

void BootEntryLoader()
{
    INT32 timeout = 0;
    COM_HANDLE hComm = HalSystemConfig.DebuggerPorts[0];

    BlockStorageStream stream;
    FLASH_WORD ProgramWordCheck;
    const UINT32 c_NoProgramFound = 0xFFFFFFFF;
    UINT32 Program = c_NoProgramFound;

#if defined(COMPILE_THUMB2)
    // Don't initialize floating-point on small builds.
#else
    setlocale( LC_ALL, "" );
#endif

    CPU_Initialize();

    HAL_Time_Initialize();

    HAL_CONTINUATION::InitializeList();
    HAL_COMPLETION  ::InitializeList();

    Events_Initialize();

    UINT8* BaseAddress;
    UINT32 SizeInBytes;

    HeapLocation( BaseAddress, SizeInBytes );

    // Initialize custom heap with heap block returned from CustomHeapLocation
    SimpleHeap_Initialize( BaseAddress, SizeInBytes );

    // this is the place where interrupts are enabled after boot for the first time after boot
    ENABLE_INTERRUPTS();

    BlockStorageList::Initialize();

    BlockStorage_AddDevices();

    BlockStorageList::InitializeDevices();

    if(!EnterMicroBooter(timeout))
    {
        HAL_UPDATE_CONFIG cfg;
    
        timeout = 0;

        if(HAL_CONFIG_BLOCK::ApplyConfig(HAL_UPDATE_CONFIG::GetDriverName(), &cfg, sizeof(cfg)))
        {
            MicroBooter_Install(cfg);

            HAL_CONFIG_BLOCK::InvalidateBlockWithName(HAL_UPDATE_CONFIG::GetDriverName(), FALSE);

            CPU_Reset();
        }
    } 

    if(stream.Initialize(BlockUsage::CODE))
    {
        do
        {
            while(TRUE)
            {
                FLASH_WORD *pWord = &ProgramWordCheck;
                UINT32 Address = stream.CurrentAddress();

                if(!stream.Read((BYTE**)&pWord, sizeof(FLASH_WORD))) break;

                if(*pWord == MicroBooter_ProgramMarker())
                {
                    Program = (UINT32)Address;
                    break;
                }

                if(!stream.Seek(BlockStorageStream::STREAM_SEEK_NEXT_BLOCK))
                {
                    if(!stream.NextStream())
                    {
                        break;
                    }
                }
            }

            if(Program != c_NoProgramFound) break;
        }
        while(stream.NextStream());
    }

    if(Program == c_NoProgramFound)
    {
        timeout = -1;
    }

#ifdef MICROBOOTER_NO_SREC_PROCESSING
    while(true)
    {
        if(Program != c_NoProgramFound && stream.Device != NULL)
        {
            Program = MicroBooter_PrepareForExecution(Program);

            DISABLE_INTERRUPTS();
            ((void (*)())Program)();
        }

        Events_WaitForEvents(0, 1000);
    }
#else
    while(true)
    {
        if(timeout != 0)
        {
            g_SREC.Initialize();
            
            DebuggerPort_Initialize(hComm);

            //--//

            while(true)
            {
                char buf[1024];
                INT32 cnt;

                if(0 == Events_WaitForEvents(ExtractEventFromTransport(hComm) | SYSTEM_EVENT_FLAG_SYSTEM_TIMER, timeout))
                {
                    break;
                }

                // wait for chars to build up
                Events_WaitForEvents(0, 8);
                
                while(0 < (cnt = DebuggerPort_Read(hComm, buf, sizeof(buf))))
                {
                    for(INT32 i=0; i<cnt; i++)
                    {
                        g_SREC.Process(buf[i]);
                    }
                }
                
            }
        }

        if(Program != c_NoProgramFound && stream.Device != NULL)
        {
            Program = MicroBooter_PrepareForExecution(Program);

            DISABLE_INTERRUPTS();
            ((void (*)())Program)();
        }

        timeout = -1;
    }
#endif
}

void FIQ_SubHandler() {}
void UNDEF_SubHandler() {ASSERT(FALSE);}
void ABORTP_SubHandler(){ASSERT(FALSE);}
void ABORTD_SubHandler(){ASSERT(FALSE);}

} // extern "C"

