////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for MCBSTM32E board (STM32): Copyright (c) Oberon microsystems, Inc.
//
// Customized for MCBSTM32E board
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include <TinyBooterEntry.h>

#define BUTTON_ENTR     BUTTON_B4
#define BUTTON_UP_IDX   BUTTON_B0_BITIDX
#define BUTTON_DOWN_IDX BUTTON_B1_BITIDX
#define BUTTON_USER_IDX BUTTON_B5_BITIDX

#define LED1 16 + 8  // PB8
#define LED2 16 + 9  // PB9
#define LED3 16 + 10 // PB10
#define LED4 16 + 11 // PB11
#define LED5 16 + 12 // PB12
#define LED6 16 + 13 // PB13

////////////////////////////////////////////////////////////////////////////////
//  Tinybooter_ProgramWordCheck
//
//  Returns the value of the first binary word of an application in FLASH
//  Tinybooter will search for this value at the beginning of each flash sector
//  at boot time and will execute the first instance.
////////////////////////////////////////////////////////////////////////////////
UINT32 Tinybooter_ProgramWordCheck()
{
    return 0x2000E00C;
}


////////////////////////////////////////////////////////////////////////////////
//  Tinybooter_ImageIsCompressed
//
//  Returns true if the image is compressed in flash and needs to be decompressed
//  and run from ram.  The image must be compressed by the BuildHelper tool.
////////////////////////////////////////////////////////////////////////////////
bool Tinybooter_ImageIsCompressed()
{
    return false;
}

////////////////////////////////////////////////////////////////////////////////
//  Tinybooter_CompressedImageStart
//
//  Returns the address of the entry point after the image is decompressed into
//  RAM. 
////////////////////////////////////////////////////////////////////////////////
UINT32 Tinybooter_CompressedImageStart( const CompressedImage_Header& header )
{
    return 0;
}

////////////////////////////////////////////////////////////////////////////////
//  Tinybooter_PrepareForDecompressedLaunch
//
//  Some platforms require some initialzation before they can be executed after
//  decompression.
////////////////////////////////////////////////////////////////////////////////
void Tinybooter_PrepareForDecompressedLaunch()
{
}


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
bool WaitForTinyBooterUpload( INT32 &timeout_ms )
{
    bool enterBooterMode = false;
    GPIO_BUTTON_CONFIG *  ButtonConfig = &g_GPIO_BUTTON_Config;

// wait forever when using RAM build 
#if defined(TARGETLOCATION_RAM)
    enterBooterMode = true;
    timeout_ms = -1;
#endif

    // user override (User button held)
    if (ButtonConfig->Mapping[BUTTON_USER_IDX].m_HW != GPIO_PIN_NONE)
    {
        Events_WaitForEvents(0,100); // wait for buttons to init
        if(!CPU_GPIO_GetPinState( ButtonConfig->Mapping[BUTTON_USER_IDX].m_HW ))
        {
            // user override, so lets stay forever
            timeout_ms = -1;
            enterBooterMode = true;
        }
    }

    return enterBooterMode;
}

////////////////////////////////////////////////////////////////////////////////
// The TinyBooter_OnStateChange method is an event handler for state changes in 
// the TinyBooter.  It is designed to help porting kit users control the tinybooter
// execution and allow them to add diagnostics.
////////////////////////////////////////////////////////////////////////////////
void TinyBooter_OnStateChange( TinyBooterState state, void* data, void ** retData )
{
    switch(state)
    {
        ////////////////////////////////////////////////////////////////////////////////////
        // State_EnterBooterMode - TinyBooter has entered upload mode
        ////////////////////////////////////////////////////////////////////////////////////
        case State_EnterBooterMode:
            CPU_GPIO_EnableOutputPin(LED1, TRUE);
            CPU_GPIO_EnableOutputPin(LED2, FALSE);
            CPU_GPIO_EnableOutputPin(LED3, TRUE);
            CPU_GPIO_EnableOutputPin(LED4, FALSE);
#if defined(TARGETLOCATION_RAM)
            CPU_GPIO_EnableOutputPin(LED5, TRUE);
#else
            CPU_GPIO_EnableOutputPin(LED5, FALSE);
#endif
            CPU_GPIO_EnableOutputPin(LED6, FALSE);
            hal_fprintf( STREAM_LCD, "Waiting\r" );
            break;

        ////////////////////////////////////////////////////////////////////////////////////
        // State_ButtonPress - A button was pressed while Tinybooter 
        // The data parameter is a pointer to the timeout value for the booter mode.
        ////////////////////////////////////////////////////////////////////////////////////
        case State_ButtonPress:
            if(NULL != data)
            {
                UINT32 down, up;
                INT32* timeout_ms = (INT32*)data;
                
                // wait forever if a button was pressed
                *timeout_ms = -1;

                // process buttons
                while(Buttons_GetNextStateChange(down, up))
                {
                    // leave a way to exit boot mode incase it was accidentally entered
                     if(0 != (down & BUTTON_ENTR)) 
                     {
                        // force an enumerate and launch
                        *timeout_ms = 0; 
                     }
                }
            }
            break;

        ////////////////////////////////////////////////////////////////////////////////////
        // State_ValidCommunication - TinyBooter has received valid communication from the host
        // The data parameter is a pointer to the timeout value for the booter mode.
        ////////////////////////////////////////////////////////////////////////////////////
        case State_ValidCommunication:
            if(NULL != data)
            {
                INT32* timeout_ms = (INT32*)data;

                // if we received any com/usb data then let's change the timeout to at least 20 seconds
                if(*timeout_ms != -1 && *timeout_ms < 20000)
                {
                    *timeout_ms = 20000;
                }
            }
            break;

        ////////////////////////////////////////////////////////////////////////////////////
        // State_Timeout - The default timeout for TinyBooter has expired and TinyBooter will 
        // perform an EnumerateAndLaunch
        ////////////////////////////////////////////////////////////////////////////////////
        case State_Timeout:
            break;

        ////////////////////////////////////////////////////////////////////////////////////
        // State_MemoryXXX - Identifies memory accesses.
        ////////////////////////////////////////////////////////////////////////////////////
        case State_MemoryWrite:
            hal_fprintf( STREAM_LCD, "Wr: 0x%08x\r", (UINT32)data );
            break;
        case State_MemoryErase:
            hal_fprintf( STREAM_LCD, "Er: 0x%08x\r", (UINT32)data );
            break;

            
        ////////////////////////////////////////////////////////////////////////////////////
        // State_CryptoXXX - Start and result of Crypto signature check
        ////////////////////////////////////////////////////////////////////////////////////
        case State_CryptoStart:
            hal_fprintf( STREAM_LCD,     "Chk signature \r" );
            hal_printf( "Chk signature \r" );
            break;
        // The data parameter is a boolean that represents signature PASS/FAILURE
        case State_CryptoResult:
            if((bool)data)
            {
                hal_fprintf( STREAM_LCD, "Signature PASS\r\n\r\n" );
                hal_printf( "Signature PASS\r\n\r\n" );
            }
            else
            {
                hal_fprintf( STREAM_LCD, "Signature FAIL\r\n\r\n" );
                hal_printf( "Signature FAIL\r\n\r\n"  );
            }
            DebuggerPort_Flush(HalSystemConfig.DebugTextPort);
            break;

        ////////////////////////////////////////////////////////////////////////////////////
        // State_Launch - The host has requested to launch an application at a given address, 
        //                or a timeout has occured and TinyBooter is about to launch the 
        //                first application it finds in FLASH.
        //
        // The data parameter is a UINT32 value representing the launch address
        ////////////////////////////////////////////////////////////////////////////////////
        case State_Launch:
            if(NULL != data)
            {
                CPU_GPIO_EnableOutputPin(LED1, FALSE);
                hal_fprintf( STREAM_LCD, "Starting application at 0x%08x\r\n", (UINT32)data );
                // copy the native code from the Load area to execute area.
                // set the *retAddres to real execute address after loading the data
                // *retData =  exeAddress 
               *retData =(void*) ((UINT32)data | 1); // set Thumb bit!
            }
            break;
    }
            
}


////////////////////////////////////////////////////////////////////////////////
// The SectorOverlapsBootstrapRegion method enables you to deny access for writing
// certain sectors.  Returning true does not guarrantee that Tinybooter will be 
// able to write to the sector.  It performes other checks (including signature 
// validation) to determine if the write operation is valid.
////////////////////////////////////////////////////////////////////////////////
bool CheckFlashSectorPermission( BlockStorageDevice *pDevice, ByteAddress address )
{
    bool fAllowWrite = false;
    UINT32 BlockType, iRegion, iRange;
    

    if(pDevice->FindRegionFromAddress(address, iRegion, iRange))
    {
        const BlockRange& range = pDevice->GetDeviceInfo()->Regions[iRegion].BlockRanges[iRange];
        
        if (range.IsBootstrap())
        {
#if defined(TARGETLOCATION_RAM) // do not allow overwriting the bootstrap sector unless Tinybooter is RAM build.
            fAllowWrite = true;
#else
            hal_printf( "Trying to write to bootstrap region\r\n" );
            fAllowWrite = false;
#endif
        }
        else
        {
            fAllowWrite = true;
        }
    }
    
    return fAllowWrite;   
}


////////////////////////////////////////////////////////////////////////////////
// TinyBooter_GetOemInfo
//
// Return in version and oeminfostring the compile-time values of some OEM-specific
// data, which you provide in the ReleaseInfo.settings file to apply to all projects
// built in this Porting Kit, or in the settings file specific to this solution.
//
// The properties to set are 
//  MajorVersion, MinorVersion, BuildNumber, RevisionNumber, and OEMSystemInfoString.
// If OEMSystemInfoString is not provided, it will be created by concatenating your
// settings for MFCompanyName and MFCopyright, also defined in ReleaseInfo.settings
// or your solution's own settings file.
//
// It is typically not necessary actually to modify this function. If you do, note
// that the return value indicates to the caller whether the releaseInfo structure
// has been fully initialized and is valid. It is safe to return false if 
// there is no useful build information you wish to report.
////////////////////////////////////////////////////////////////////////////////
BOOL TinyBooter_GetReleaseInfo(MfReleaseInfo& releaseInfo)
{
    MfReleaseInfo::Init(
                    releaseInfo, 
                    VERSION_MAJOR, VERSION_MINOR, VERSION_BUILD, VERSION_REVISION, 
                    OEMSYSTEMINFOSTRING, hal_strlen_s(OEMSYSTEMINFOSTRING)
                    );
    return TRUE;
}

