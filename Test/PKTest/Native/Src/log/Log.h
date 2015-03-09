////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include <BlockStorage_decl.h>

#ifndef _NMT_H_
#define _NMT_H_ 1

//enum BOOL {FAIL=0, PASS=1, SKIP=0x10, UNKNOWN=0x20, KNOWN_FAILURE=0x30};
enum NMT_WAIT {WAIT_BUTTON=0, WAIT_SEC=0x1, WAIT_FOREVER=0xFF};
enum NMT_STREAM   {NULLSTREAM=0x0, LCDSTREAM=1, FLASHSTREAM=2, 
	           USBSTREAM=3, SERIALSTREAM=4, MEMORYSTREAM=5, HALSTREAM=6};

#define LOG_BUFFER_SIZE 512

typedef
struct _Log
{
    char         LogBuffer[LOG_BUFFER_SIZE];
    BOOL         Init;
    BOOL         Result;
    UINT32       State;                      // test defined state variable
    NMT_STREAM   StreamType;

    BOOL Initialize(NMT_STREAM); 
    BOOL Uninitialize();

    void BeginTest(char *);
    void EndTest(BOOL, char *);
    void EndTest(BOOL);

    void NextTest(NMT_WAIT);

    void Comment(char * );

    char * ConvertStatus(BOOL);

    //
    // Begin Preliminary
    //
    void    StoreState( UINT32 );
    UINT32  LastState();
    void    IncrementState();
    void    LogState( char *);
    //
    // End Preliminary
    //
    

    void (* _NMT_Output_Stream)(char * message);

    //
    // Block Storage Log 
    //
    BOOL InitBlockStorage();

    BlockStorageDevice * pBlockDevice;
    SectorAddress        CurrentLogSector;
    SectorAddress        BeginLogSector;
    SectorAddress        EndLogSector; 
    UINT32               NumSectors;
    UINT32               DataBytesPerSector;
    void  WriteFLASHStream(char * message);

    void SetBlockParams(BlockStorageDevice * pB, 
                        UINT32               NumSectors,
                        SectorAddress        BeginLog,
                        SectorAddress        EndLog)

    {
	    pBlockDevice          = pB;
	    NumSectors            = NumSectors;
	    BeginLogSector        = BeginLog;
            CurrentLogSector      = BeginLog;
            EndLogSector          = EndLog;
    }
    
} Log;





#endif  //_NMT_H_     
