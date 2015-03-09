////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Log.h"
#include <blockstorage_decl.h>

//
void _NMT_Output_LCDStream(char *);
void _NMT_Output_HALStream(char *);
void _NMT_Output_FLASHStream(char * );


//- FUNCTIONS -//


BOOL Log::Initialize(NMT_STREAM StreamPath)
{
    Log::Result = false;
    Log::State  = 0;
    Log::LogBuffer[0] = (char) '\0';
    switch (StreamPath)
    {
        case LCDSTREAM:
            {
		Log::StreamType=LCDSTREAM;
                Log::Init=true; 
                _NMT_Output_Stream = _NMT_Output_LCDStream;
                return true;
            }
        case HALSTREAM:
            {
		Log::StreamType=HALSTREAM;
                Log::Init=true; 
                _NMT_Output_Stream = _NMT_Output_HALStream;
                return true;
            }
        case FLASHSTREAM:
            {
		Log::StreamType=FLASHSTREAM;
                //Log::Init=true; 
                //  _NMT_Output_Stream = _NMT_Output_HALStream;
                //return true;
            }
        case USBSTREAM:
        case SERIALSTREAM:
        case MEMORYSTREAM:
        default:
            {
                return false;
            }
    }
    return true;
}


BOOL Log::Uninitialize( )
{
    return true;
}

char * Log::ConvertStatus(BOOL Status)
{
    switch(Status)
    {
        case true:
		return "Pass";
        case false:
	default:
		return "Fail <";
    }
}

void Log::BeginTest(char * message )
{
    if (!Init) return;
    Log::Result = false;
    Log::State  = 0;
    _NMT_Output_Stream(message) ;
}

void Log::EndTest(BOOL status)
{
    Log::EndTest(status, "");
}

void Log::EndTest(BOOL status, char * message )
{
    char StringBuffer[128];
    char * status_string = ConvertStatus(status);
    char * fmt = Log::StreamType == LCDSTREAM ? "\t%s\t%s\r\n" : "\t%s\t%s\r";


    if (!Init) return;

    if (hal_strlen_s(message) != 0)
    {
        hal_snprintf(StringBuffer,
                     sizeof(StringBuffer), 
                     fmt,
                     status_string,
                     message);
    }
    else
    {
	    /*
        hal_snprintf(StringBuffer,
                     sizeof(StringBuffer), 
                     fmt,
                     status_string,
                     Log::LogBuffer);
	*/
        hal_snprintf(StringBuffer,
                     sizeof(StringBuffer), 
                     fmt,
                     status_string,
                     message);
    }

     _NMT_Output_Stream(StringBuffer);
}

void Log::Comment(char * message)
{
    if (!Init) return;
    switch (StreamType)
    {
        case LCDSTREAM:
        case HALSTREAM:
            {
		hal_strcpy_s(Log::LogBuffer,LOG_BUFFER_SIZE, message);
                return ;
            }
        case FLASHSTREAM:
	default:
            {
                return;
            }
    }
}

void Log::NextTest(NMT_WAIT wait)
{
   if (!Init) return;
   switch(wait)
   {
       case WAIT_BUTTON:
           {
               // ToBe
               //
               //wait for button press
               break;

           }
       case WAIT_SEC:
           {
               
               Events_WaitForEvents(0, 1000);
               break;
           }
       case WAIT_FOREVER:
           {
               while (1)
               {
                   Events_WaitForEvents(0, 2000);
               }
           }
       default:
           {
               break;
           }
   } 
}

void Log::StoreState(UINT32 value)
{
    State=value;
}
UINT32 Log::LastState()
{
    return State;
}
void Log::IncrementState()
{
   Log::State += 1;
}
void Log::LogState(char * message)
{
    char StringBuffer[128];

    if (!Init) return;
    hal_snprintf(StringBuffer,
		  sizeof(StringBuffer), 
                  "LogState:%0x:%s",
		  Log::State,
		  message);

    _NMT_Output_Stream(StringBuffer);
}

///////////////////////////////////////////////////////////////////////////////
//
// LCDSTREAM LOG support
//
void _NMT_Output_LCDStream(char * message)
{
   lcd_printf("%s", message);
}

///////////////////////////////////////////////////////////////////////////////
//
// HALSTREAM LOG support
//
void _NMT_Output_HALStream(char * message)
{
   hal_printf("%s\r\n", message);
}

///////////////////////////////////////////////////////////////////////////////
//
// FLASHSTREAM LOG support
//
// First implementation using blockstorage logging
// 1. Identify deployment region (size and starting sectoraddress)
// 2. write log once in sector units, when full, stop.
//
// Improve behavior with requirements from users
//
//
void WriteFLASHStream(char * message)
{
}
/*
    if (hal_strlen_s(message) == 0)
        return;

    char * pSectorBuffer = Log::LogBuffer;

    UINT32 messageSize      =   hal_strlen_s(message);

    UINT32 numSectorsToWrite =  (messageSize + (Log::DataBytesPerSector-1)) / 
                                Log::DataBytesPerSector;

    // Avoid overflow, revise for circular buffer management
    if ((numSectorsToWrite + 
       (Log::EndLog - Log::BeginLog)) > Log::NumSectors)
       {
           return;
       }

    // leave room for end-of-record '/r/n'
    if (messageSize > LOG_BUFFER_SIZE)
    {
        messageSize = LOG_BUFFER_SIZE-2;
        hal_strncpy_s(pSectorBuffer, LOG_BUFFER_SIZE, message, LOG_BUFFER_SIZE-2 );
        //pSectorBuffer[LOG_BUFFER_SIZE-2] = (char )'/r';
        //pSectorBuffer[LOG_BUFFER_SIZE-1] = (char )'/n';
    }
    else
    {
        hal_strncpy_s(pSectorBuffer, LOG_BUFFER_SIZE, message, messageSize );
    }

    Log::pBlockDevice->WriteSector(Log::BeginLog,
                                               numSectorsToWrite,
                                               (BYTE *)pSectorBuffer,
                                               NULL);
    // update LogBufferState
    //
    Log::EndLog += numSectorsToWrite; 
}

BOOL Log::InitBlockStorage()
{
    int count = BlockStorageList::GetNumDevices();
    if (!count)
    {
        return false;
    }

    BlockStorageDevice * pBlockDevice = BlockStorageList::GetFirstDevice();
    const BlockDeviceInfo * deviceInfo; 

    deviceInfo = pBlockDevice->GetDeviceInfo();

    //int numRegions = deviceInfo.NumRegions;
    //int write_Secs = deviceInfo.MaxSectorWrite_uSec;
    //int erase_Secs = deviceInfo.MaxBlockErase_uSec;
    //
    const BlockRegionInfo * pRegion = deviceInfo->pRegions;

    // Use first region only!!
    // Find deploy regions to use for block store; 
    // otherwise - ask users what behavior is appropriate 
    //

    const BlockRegionInfo  * region = &pRegion[0]; 
    SectorAddress blockAddressStart = region->Start;
    //int BytesPerSector    = region->DataBytesPerSector;
    int NumBlocks         = region->NumBlocks;


    BOOL   foundDeployBlock    = false;
    SectorAddress DeploymentSectorAddress    = 0xFFFFFFFF;
    UINT32 DeploymentBlockIndex = 0xFFFFFFFF; 
    UINT32 numDeploySector     = 0;
    SectorAddress location     = 0;

    for (int blockindex=0; blockindex<NumBlocks; blockindex++)
    {
        BlockStatus status;
	location = blockAddressStart + region->SectorsPerBlock * blockindex;
	if (!(pBlockDevice->GetBlockStatus(location,status)))
	{
		break;
	}
        if (status.IsDeployment())
        {
            if (!foundDeployBlock)
            {
                // record first deployment block
                //
                DeploymentSectorAddress = location;
                DeploymentBlockIndex = blockindex;
            }
            foundDeployBlock = true;
            numDeploySector += region->SectorsPerBlock;
        }
        else
            if (foundDeployBlock)
            {
                // stop search if non-contiguous blocks found
                break;
            }
    } //for
    if (foundDeployBlock)
    {
        //
        // Store 
        //

	SetBlockParams(pBlockDevice,
                       numDeploySector,
                       (DeploymentSectorAddress + numDeploySector), 
                       DeploymentSectorAddress,	
                       region->DataBytesPerSector);

        
        return true;
    }
    else
    {
        return false;
    }
} // INitBlockStorage

*/
