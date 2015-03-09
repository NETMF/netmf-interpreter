////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef __TINYBOOTERENTRY_H__
#define __TINYBOOTERENTRY_H__

/////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////////
// State_EnterBooterMode    - TinyBooter has entered upload mode
// State_ButtonPress        - A button was pressed while Tinybooter 
// State_ValidCommunication - TinyBooter has received valid communication from the host
// State_Timeout            - The default timeout for TinyBooter has expired and 
//                            TinyBooter will perform an EnumerateAndLaunch
// State_MemoryXXX          - Identifies memory accesses
// State_CryptoXXX          - Start and result of Crypto signature check
// State_Launch             - The host has requested to launch an application at a 
//                            given address, or the timeout occured and the TinyBooter
//                            is about to launch the fist application in FLASH
////////////////////////////////////////////////////////////////////////////////////
typedef enum _TinyBooterState
{
   State_EnterBooterMode,
   State_ButtonPress,
   State_ValidCommunication,
   State_MemoryWrite,
   State_MemoryErase,
   State_CryptoStart,
   State_CryptoResult,
   State_Timeout,
   State_Launch,
} TinyBooterState; 


////////////////////////////////////////////////////////////////////////////////
// Destination  - the address the header should be decompressed to 
// Compressed   - the size of the compressed executable 
// Uncompressed - the size of the uncompressed executable 
////////////////////////////////////////////////////////////////////////////////
struct CompressedImage_Header
{
    char* Destination;
    int   Compressed;
    int   Uncompressed;
};

////////////////////////////////////////////////////////////////////////////////
// The WaitForTinyBooterUpload method was designed to allow porting kit partners
// to define how/when tinybooter mode is entered as well as configure default
// timeout values.  
//
// timeout_ms   - this parameter determines the time in milliseconds TinyBooter is
//                supposed to wait for commands from the host.  A -1 value will 
//                indicate to wait forever.
// return value - the boolean return value indicates whether TinyBooter should enter
//                upload mode.  If false is returned the booter will attempt to
//                launch the first application in FLASH that it finds.  If the return
//                value is true, TinyBooter will wait for the given timeout value
//                (parameter timeout_ms) for valid commands before launching the first
//                application
////////////////////////////////////////////////////////////////////////////////
extern bool WaitForTinyBooterUpload( INT32 &timeout_ms );

////////////////////////////////////////////////////////////////////////////////
// The TinyBooter_OnStateChange method is an event handler for state changes in 
// the TinyBooter.  It is designed to help porting kit users control the tinybooter
// execution and allow them to add diagnostics.
////////////////////////////////////////////////////////////////////////////////
void TinyBooter_OnStateChange( TinyBooterState state, void* data, void ** retData = (void**)NULL );


////////////////////////////////////////////////////////////////////////////////
//  Tinybooter_CompressedImageStart
//
//  Returns the address of the entry point after the image is decompressed into
//  RAM. 
////////////////////////////////////////////////////////////////////////////////
extern UINT32 Tinybooter_ProgramWordCheck();


////////////////////////////////////////////////////////////////////////////////
//  Tinybooter_ImageIsCompressed
//
//  Returns true if the image is compressed in flash and needs to be decompressed
//  and run from ram.  The image must be compressed by the BuildHelper tool.
////////////////////////////////////////////////////////////////////////////////
bool Tinybooter_ImageIsCompressed();

////////////////////////////////////////////////////////////////////////////////
//  Tinybooter_CompressedImageStart
//
//  Returns the address of the entry point after the image is decompressed into
//  RAM. 
////////////////////////////////////////////////////////////////////////////////
UINT32 Tinybooter_CompressedImageStart( const CompressedImage_Header& header );

////////////////////////////////////////////////////////////////////////////////
//  Tinybooter_PrepareForDecompressedLaunch
//
//  Some platforms require some initialzation before they can be executed after
//  decompression.
////////////////////////////////////////////////////////////////////////////////
void Tinybooter_PrepareForDecompressedLaunch();

////////////////////////////////////////////////////////////////////////////////
// The SectorOverlapsBootstrapRegion method enables you to deny access for writing
// certain sectors.  Returning true does not guarrantee that Tinybooter will be 
// able to write to the sector.  It performes other checks (including signature 
// validation) to determine if the write operation is valid.
////////////////////////////////////////////////////////////////////////////////
bool CheckFlashSectorPermission( BlockStorageDevice * pDevice, ByteAddress Sector );
    
/////////////////////////////////////////////////////////////////////////////

#endif /* __TINYBOOTERENTRY_H__ */
