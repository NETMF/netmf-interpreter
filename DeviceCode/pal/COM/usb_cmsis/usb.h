////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _PAL_USB_H_
#define _PAL_USB_H_ 1

//--//

#include "Tinyhal.h"

//--//

void USB_debug_printf( const char*format, ... );

//--//

#if defined(BUILD_RTM)
    #undef USB_METRIC_COUNTING
    #undef USB_METRIC_NAK_COUNTING

#else
    #define USB_METRIC_COUNTING     1
//    #undef USB_METRIC_COUNTING
//   turn off NAK, as NAK-in will trigger all the time
//    #define USB_METRIC_NAK_COUNTING 1
    #undef USB_METRIC_NAK_COUNTING
#endif

//--//

#define PORT_TX_TO_ENDPOINT(P)  (((P) << 1) | 1)
#define PORT_RX_TO_ENDPOINT(P)  (((P) << 1) + 2)

//--//

extern USB_SETUP_PACKET RequestPacket;

//--//

extern UINT8 USB_HandleSetConfiguration( USB_CONTROLLER_STATE* State, USB_SETUP_PACKET* Setup, BOOL DataPhase);

extern USB_PACKET64* USB_RxEnqueue( USB_CONTROLLER_STATE* State, int queue, BOOL& DisableRx );
extern USB_PACKET64* USB_TxDequeue( USB_CONTROLLER_STATE* State, int queue, BOOL  Done      );

extern UINT8 USB_ControlCallback  ( USB_CONTROLLER_STATE* State );
extern void  USB_StateCallback    ( USB_CONTROLLER_STATE* State );

extern int  UsbConfigurationCheck ( const USB_DYNAMIC_CONFIGURATION* firstRecord );
extern BOOL USB_NextEndpoint      ( USB_CONTROLLER_STATE* State, const USB_ENDPOINT_DESCRIPTOR* &ep, const USB_INTERFACE_DESCRIPTOR* &itfc );

//--//

#if defined( USB_REMOTE_WAKEUP)
enum USB_REMOTEWKUP_STATE
{
    USB_REMOTEWKUP_NOT_READY   = 0, // not allowed any remote wake up
    USB_REMOTEWKUP_WAIT_SD5    = 1, // wait for 5ms idle for allow remote wk up
    USB_REMOTEWKUP_SD5_READY   = 2, // SD5 is fulfilled
    USB_REMOTEWKUP_WAIT_10MS   = 3, // hold remote wk up signal for 10ms when Remotewk up is implememnted
    USB_REMOTEWKUP_10MS_READY  = 4, // complete 10 ms RESUME
    USB_REMOTEWKUP_WAIT_EOP    = 5, // wait for the EOP
    USB_REMOTEWKUP_EOP_READY   = 6, // Receive EOP isr
    USB_REMOTEWKUP_100MS_EXPIRE =0xBA    //error of not found EOP
};
#endif

#if defined(USB_METRIC_COUNTING)
struct USB_PERFORMANCE_METRICS
{
    UINT32 RxErrCnt; // sum of any Rx err
    UINT32 TxErrCnt; // sum of any Tx Err
    UINT32 ULDCnt;
    UINT32 InNAKCnt[3];   // NAK cnt for each endpoint, except ep0
    UINT32 OutNAKCnt[3];
    UINT32 FrameCnt;
    UINT32 OverunCnt[3]; //For ep2, ep4, ep6 (rx ep) (ep0 no err)
    UINT32 UnderunCnt[3]; // for ep1, ep3, ep5, (tx ep)
    UINT32 SD5Cnt;
    UINT32 SD3Cnt;
    UINT32 EOPCnt;
    UINT32 DMACnt;
    UINT32 ResumeCnt;
};
#endif 

//--//

extern const char* UsbStrings[];

extern 
#if !defined(_ARC) // the ARC compiler does not like const in ths situation
const 
#else
#endif
ADS_PACKED struct USB_DYNAMIC_CONFIGURATION UsbDefaultConfiguration;
 
//--//

void USB_ClearQueues( USB_CONTROLLER_STATE *State, BOOL ClrRxQueue, BOOL ClrTxQueue );

USB_PACKET64* USB_RxEnqueue( int queue, BOOL& DisableRx );
USB_PACKET64* USB_TxDequeue( int queue, BOOL  Done      );

UINT8 USB_VendorControl( USB_CONTROLLER_STATE* State, USB_SETUP_PACKET* Setup);

UINT8 USB_ControlCallback( USB_CONTROLLER_STATE* State );

void USB_StateCallback(USB_CONTROLLER_STATE *State);

void USB_DataCallback(USB_CONTROLLER_STATE *State);

const USB_DESCRIPTOR_HEADER * USB_FindRecord(USB_CONTROLLER_STATE* State, UINT8 marker, USB_SETUP_PACKET * iValue);

//--//

struct USB_Driver
{
    static int  GetControllerCount();
    static BOOL Initialize  ( int Controller );
    static BOOL Uninitialize( int Controller );
    static int  Configure   ( int Controller, const USB_DYNAMIC_CONFIGURATION* Config );
    static const USB_DYNAMIC_CONFIGURATION * GetConfiguration( int Controller );

    static BOOL OpenStream  ( int stream, int writeEP, int readEP );
    static BOOL CloseStream ( int stream );

    static int  Write( int UsbStream, const char* Data, size_t size );
    static int  Read ( int UsbStream, char*       Data, size_t size );
    static BOOL Flush( int UsbStream                                );

    static UINT32 GetEvent  ( int Controller, UINT32 Mask  );
    static UINT32 SetEvent  ( int Controller, UINT32 Event );
    static UINT32 ClearEvent( int Controller, UINT32 Event );
    static UINT8  GetStatus ( int Controller               );

    static void DiscardData( int UsbStream, BOOL fTx );
};

//--//

#endif /* _PAL_USB_H_ */
