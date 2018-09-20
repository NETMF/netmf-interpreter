////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _DRIVERS_USB_DECL_H_
#define _DRIVERS_USB_DECL_H_ 1

#if defined(_MSC_VER)
#pragma pack(push, USB_DECL_H_, 1)
#endif

//--//

// enable USB remote wakeup, the USB configuration descriptor should change also
// the USB_ATTRIBUTES should set the REMOTE_WAKE_UP bit (0x20) to "1"
//#define USB_REMOTE_WAKEUP   1
#undef  USB_REMOTE_WAKEUP

//--//

//#define DEBUG_USB 1
#undef  DEBUG_USB

//--//

//#define DEBUG_USB 1
#undef  DEBUG_USB

//--//
//--//
//--//

// please define your own endpoints for debugging in your own platform header file 
// if you wish to override the defaults
#ifndef USB_DEBUG_EP_WRITE
#define USB_DEBUG_EP_WRITE  1
#endif
#ifndef USB_DEBUG_EP_READ
#define USB_DEBUG_EP_READ   2
#endif

#define USB_DEBUG_EVENT_IN  (1 << USB_DEBUG_EP_READ)
#define USB_EVENT_ALL       0xFFFFFFFF

//--//
//--//
//--//

// USB 2.0 host requests
#define USB_GET_STATUS           0
#define USB_CLEAR_FEATURE        1
#define USB_SET_FEATURE          3
#define USB_SET_ADDRESS          5
#define USB_GET_DESCRIPTOR       6
#define USB_SET_DESCRIPTOR       7
#define USB_GET_CONFIGURATION    8
#define USB_SET_CONFIGURATION    9
#define USB_GET_INTERFACE       10
#define USB_SET_INTERFACE       11
#define USB_SYNCH_FRAME         12

// USB 2.0 defined descriptor types
#define USB_DEVICE_DESCRIPTOR_TYPE        1
#define USB_CONFIGURATION_DESCRIPTOR_TYPE 2
#define USB_STRING_DESCRIPTOR_TYPE        3
#define USB_INTERFACE_DESCRIPTOR_TYPE     4
#define USB_ENDPOINT_DESCRIPTOR_TYPE      5

// USB 2.0 host request type defines
#define USB_SETUP_DIRECTION(n)          ((n) & 0x80)
#define USB_SETUP_DIRECTION_DEVICE      0x00
#define USB_SETUP_DIRECTION_HOST        0x80

#define USB_SETUP_TYPE(n)        ((n) & 0x70)
#define USB_SETUP_TYPE_STANDARD         0x00
#define USB_SETUP_TYPE_CLASS            0x10
#define USB_SETUP_TYPE_VENDOR           0x20
#define USB_SETUP_TYPE_RESERVED         0x30

#define USB_SETUP_RECIPIENT(n)          ((n) & 0x0F)
#define USB_SETUP_RECIPIENT_DEVICE             0x00
#define USB_SETUP_RECIPIENT_INTERFACE          0x01
#define USB_SETUP_RECIPIENT_ENDPOINT           0x02
#define USB_SETUP_RECIPIENT_OTHER              0x03

// Local device status defines 
#define USB_STATUS_DEVICE_NONE           0x0000
#define USB_STATUS_DEVICE_SELF_POWERED   0x0001
#define USB_STATUS_DEVICE_REMOTE_WAKEUP  0x0002

#define USB_STATUS_INTERFACE_NONE        0x0000

#define USB_STATUS_ENDPOINT_NONE         0x0000
#define USB_STATUS_ENDPOINT_HALT         0x0001

#define USB_FEATURE_DEVICE_REMOTE_WAKEUP 0x0001
#define USB_FEATURE_ENDPOINT_HALT        0x0000

// Local device possible states
#define USB_DEVICE_STATE_DETACHED       0
#define USB_DEVICE_STATE_ATTACHED       1
#define USB_DEVICE_STATE_POWERED        2
#define USB_DEVICE_STATE_DEFAULT        3
#define USB_DEVICE_STATE_ADDRESS        4
#define USB_DEVICE_STATE_CONFIGURED     5
#define USB_DEVICE_STATE_SUSPENDED      6
#define USB_DEVICE_STATE_NO_CONTROLLER  0xFE
#define USB_DEVICE_STATE_UNINITIALIZED  0xFF

// Possible responses to host requests
#define USB_STATE_DATA                  0
#define USB_STATE_STALL                 1
#define USB_STATE_DONE                  2
#define USB_STATE_ADDRESS               3
#define USB_STATE_STATUS                4
#define USB_STATE_CONFIGURATION         5
#define USB_STATE_REMOTE_WAKEUP         6

#define USB_CURRENT_UNIT        2

/////////////////////////////////////////////////////////////////////////
// ATTENTION:
// 2.0 is the lowest version that works with WinUSB on Windows 8!!! 
// use older values below if you do not care about that
//
#define USB_VERSION             0x0200 
#define DEVICE_RELEASE_VERSION  0x0200
// #define USB_VERSION             0x0110
// #define DEVICE_RELEASE_VERSION  0x0110
// 
/////////////////////////////////////////////////////////////////////////

#define USB_SETUP_COMPLETE      0
#define USB_SETUP_ERROR         1

#define USB_TX_RETRY_TIMEOUT_USEC  100000
#define USB_TX_RETRY_MAX           3

#define USB_DISPLAY_STRING_NUM     4
#define USB_FRIENDLY_STRING_NUM    5

#define OS_DESCRIPTOR_STRING_INDEX        0xEE
#define OS_DESCRIPTOR_STRING_VENDOR_CODE  0xA5

//--//

#ifndef USB_MAX_QUEUES
#define USB_MAX_QUEUES         16
#endif
#ifdef PLATFORM_DEPENDENT_USB_QUEUE_PACKET_COUNT
#define USB_QUEUE_PACKET_COUNT  PLATFORM_DEPENDENT_USB_QUEUE_PACKET_COUNT
#else
#define USB_QUEUE_PACKET_COUNT  8
#endif

//--//

// USB 2.0 request packet from host
ADS_PACKED struct GNU_PACKED USB_SETUP_PACKET
{
    UINT8 bmRequestType;
    UINT8 bRequest;
    UINT16 wValue;
    UINT16 wIndex;
    UINT16 wLength;
};

// USB 2.0 response structure lengths
#define USB_STRING_DESCRIPTOR_MAX_LENGTH        126  // Maximum number of characters allowed in USB string descriptor 
#define USB_FRIENDLY_NAME_LENGTH                 32
#define USB_DEVICE_DESCRIPTOR_LENGTH             18
#define USB_CONFIGURATION_DESCRIPTOR_LENGTH       9
#define USB_STRING_DESCRIPTOR_HEADER_LENGTH       2
// Sideshow descriptor lengths
#define OS_DESCRIPTOR_STRING_SIZE                18
#define OS_DESCRIPTOR_STRING_LENGTH               7
#define USB_XCOMPATIBLE_OS_SIZE                  40
#define USB_XPROPERTY_OS_SIZE_WINUSB     0x0000008E  // Size of this descriptor (78 bytes for guid + 40 bytes for the property name + 24 bytes for other fields = 142 bytes)
#define USB_XCOMPATIBLE_OS_REQUEST                4
#define USB_XPROPERTY_OS_REQUEST                  5



//--//

/////////////////////////////////////////////////////////////////////////////////////
// USB Configuration list structures
// Dynamic USB Controller configuration is implemented as a packed list of structures.
// Each structure contains two parts: a header that describes the host request
// the structure satisfies, and the exact byte by byte structure that satisfies
// the host request.  The header also contains the size of the structure so that
// the next structure in the list can be quickly located.

// Marker values for the header portion of the USB configuration list structures.
// These specify the type of host request that the structure satisfies
#define USB_END_DESCRIPTOR_MARKER           0x00
#define USB_DEVICE_DESCRIPTOR_MARKER        0x01
#define USB_CONFIGURATION_DESCRIPTOR_MARKER 0x02
#define USB_STRING_DESCRIPTOR_MARKER        0x03
#define USB_GENERIC_DESCRIPTOR_MARKER       0xFF


// The header portion of the USB configuration list structure.
ADS_PACKED struct GNU_PACKED USB_DESCRIPTOR_HEADER
{
    UINT8  marker;
    UINT8  iValue;
    UINT16 size;
    static USB_DESCRIPTOR_HEADER *next(const USB_DESCRIPTOR_HEADER *This)
    {
        UINT8 *pHeader = (UINT8 *)This;
        pHeader += This->size;
        return( (USB_DESCRIPTOR_HEADER *)pHeader );
    }
};

// USB configuration list structure that responds to device descriptor requests
ADS_PACKED struct GNU_PACKED USB_DEVICE_DESCRIPTOR
{
    USB_DESCRIPTOR_HEADER header;

    UINT8  bLength;
    UINT8  bDescriptorType;
    UINT16 bcdUSB;
    UINT8  bDeviceClass;
    UINT8  bDeviceSubClass;
    UINT8  bDeviceProtocol;
    UINT8  bMaxPacketSize0;
    UINT16 idVendor;
    UINT16 idProduct;
    UINT16 bcdDevice;
    UINT8  iManufacturer;
    UINT8  iProduct;
    UINT8  iSerialNumber;
    UINT8  bNumConfigurations;

};

// USB configuration list structure that responds to configuration descriptor requests
ADS_PACKED struct GNU_PACKED USB_CONFIGURATION_DESCRIPTOR
{
    USB_DESCRIPTOR_HEADER header;

    UINT8  bLength;
    UINT8  bDescriptorType;
    UINT16 wTotalLength;
    UINT8  bNumInterfaces;
    UINT8  bConfigurationValue;
    UINT8  iConfiguration;
    UINT8  bmAttributes;
#define USB_ATTRIBUTE_REMOTE_WAKEUP    0x20
#define USB_ATTRIBUTE_SELF_POWER       0x40
#define USB_ATTRIBUTE_BASE             0x80
    UINT8  bMaxPower;

};

// At least one of these structures must follow the configuration descriptor structure
// described above
ADS_PACKED struct GNU_PACKED USB_INTERFACE_DESCRIPTOR
{
    UINT8 bLength;
    UINT8 bDescriptorType;
    UINT8 bInterfaceNumber;
    UINT8 bAlternateSetting;
    UINT8 bNumEndpoints;
    UINT8 bInterfaceClass;
    UINT8 bInterfaceSubClass;
    UINT8 bInterfaceProtocol;
    UINT8 iInterface;
};

// May optionally follow the interface descriptor described above.
// Note that this structure is incomplete; it must be followed by the actual
// data that describes the class of the interface it is associated with.
ADS_PACKED struct GNU_PACKED USB_CLASS_DESCRIPTOR_HEADER
{
    UINT8 bLength;
    UINT8 bDescriptorType;
};

// At least one of these structures must follow the class descriptor or interface
// descriptor described above.
ADS_PACKED struct GNU_PACKED USB_ENDPOINT_DESCRIPTOR
{
    UINT8  bLength;
    UINT8  bDescriptorType;
#define USB_ENDPOINT_DIRECTION_IN 0x80
#define USB_ENDPOINT_DIRECTION_OUT 0x00
    UINT8  bEndpointAddress;
#define USB_ENDPOINT_ATTRIBUTE_ISOCHRONOUS 1
#define USB_ENDPOINT_ATTRIBUTE_BULK 2
#define USB_ENDPOINT_ATTRIBUTE_INTERRUPT 3
    UINT8  bmAttributes;
    UINT16 wMaxPacketSize;
    UINT8  bInterval;
};

// USB configuration list structure that responds to string descriptor requests.
// Note that this only describes the first part of the string descriptor; it must
// be followed by the actual Unicode string.
ADS_PACKED struct GNU_PACKED USB_STRING_DESCRIPTOR_HEADER
{
    USB_DESCRIPTOR_HEADER header;

    UINT8 bLength;
    UINT8 bDescriptorType;
};

typedef ADS_PACKED wchar_t USB_STRING_CHAR;

// USB configuration list structure that responds to any host request for constant
// data over the Control endpoint (endpoint 0).
// This structure is simply a larger header that specifies the entire host request
// packet.  If the host request packet matches the data in this header, the data
// following this header will be returned, byte for byte, to the host.
// Note, therefore, that this header must be directly followed by the actual data
// to be returned.
ADS_PACKED struct GNU_PACKED USB_GENERIC_DESCRIPTOR_HEADER
{
    USB_DESCRIPTOR_HEADER header;

    UINT8  bmRequestType;
#define USB_REQUEST_TYPE_OUT       0x00
#define USB_REQUEST_TYPE_IN        0x80
#define USB_REQUEST_TYPE_STANDARD  0x00
#define USB_REQUEST_TYPE_CLASS     0x20
#define USB_REQUEST_TYPE_VENDOR    0x40
#define USB_REQUEST_TYPE_DEVICE    0x00
#define USB_REQUEST_TYPE_INTERFACE 0x01
#define USB_REQUEST_TYPE_ENDPOINT  0x02
    UINT8  bRequest;
    UINT16 wValue;
    UINT16 wIndex;
};

// USB configuration list structure that responds to requests for the OS string descriptor
// This is generally only used by WinUSB
ADS_PACKED struct GNU_PACKED USB_OS_STRING_DESCRIPTOR
{
    USB_DESCRIPTOR_HEADER header;
    
    UINT8   bLength;
    UINT8   bDescriptorType;
    USB_STRING_CHAR signature[OS_DESCRIPTOR_STRING_LENGTH];
    UINT8   bMS_VendorCode;
    UINT8   padding;
};

// USB configuration list structure that responds to requests for the OS extended compatible ID
// Note that this is created as a Generic descriptor as described above.
// This is generally only used by WinUSB
ADS_PACKED struct GNU_PACKED USB_XCOMPATIBLE_OS_ID
{
    USB_GENERIC_DESCRIPTOR_HEADER header;
    
    UINT32 dwLength;
    UINT16 bcdVersion;
    UINT16 wIndex;
    UINT8  bCount;
    UINT8  padding1[7];
    // One Extended Compatible OS ID function record
    UINT8  bFirstInterfaceNumber;
    UINT8  reserved;
    UINT8  compatibleID[8];
    UINT8  subCompatibleID[8];
    UINT8  padding2[6];
};

// USB configuration list structure that responds to requests for the OS extended properties feature descriptor 
// This is generally only used by WinUSB
ADS_PACKED struct GNU_PACKED USB_XPROPERTIES_OS_WINUSB
{
    USB_GENERIC_DESCRIPTOR_HEADER header;
    
    UINT32 dwLength;
    UINT16 bcdVersion;
    UINT16 wIndex;
    UINT16  bCount;
    // One Extended Property OS record
    UINT32 dwSize;
#define EX_PROPERTY_DATA_TYPE__RESERVED                 0
#define EX_PROPERTY_DATA_TYPE__REG_SZ                   1
#define EX_PROPERTY_DATA_TYPE__REG_SZ_ENV               2
#define EX_PROPERTY_DATA_TYPE__REG_BINARY               3
#define EX_PROPERTY_DATA_TYPE__REG_DWORD_LITTLE_ENDIAN  4
#define EX_PROPERTY_DATA_TYPE__REG_DWORD_BIG_ENDIAN     5
#define EX_PROPERTY_DATA_TYPE__REG_LINK                 6
#define EX_PROPERTY_DATA_TYPE__REG_MULTI_SZ             7
    UINT32 dwPropertyDataType;
    UINT16 wPropertyNameLengh;
    UINT8  bPropertyName[40]; // NULL -terminated UNICODE string
    UINT32 dwPropertyDataLengh;
    UINT8  bPropertyData[78]; // NULL -terminated UNICODE string
};


ADS_PACKED struct GNU_PACKED USB_DYNAMIC_CONFIGURATION;

// These are the errors returned by UsbConfigurationCheck() that checks a USB configuration list
// for simple errors.
enum USB_CONFIGURATION_ERRORS
{
    USB_CONFIG_ERR_OK             =  0,     // All OK
    USB_CONFIG_ERR_MISSING_RECORD =  1,     // Missing Device or Configuration Descriptor record
    USB_CONFIG_ERR_DUP_DEVICE     =  2,     // More than one Device descriptor record found
    USB_CONFIG_ERR_DEVICE_SIZE    =  3,     // Device descriptor length or header size incorrect
    USB_CONFIG_ERR_DEVICE_TYPE    =  4,     // Device descriptor type mismatch
    USB_CONFIG_ERR_EP0_SIZE       =  5,     // Device descriptor has wrong max size for endpoint 0
    USB_CONFIG_ERR_NCONFIGS       =  6,     // More than one configuration is defined
    USB_CONFIG_ERR_DUP_CONFIG     =  7,     // More than one configuration descriptor header found
    USB_CONFIG_ERR_CONFIG_SIZE    =  8,     // Record size does not match full configuration descriptor size or config descriptor size is wrong
    USB_CONFIG_ERR_CONFIG_TYPE    =  9,     // Configuration descriptor type mismatch
    USB_CONFIG_ERR_NO_INTERFACE   = 10,     // A configuration was defined with no interfaces
    USB_CONFIG_ERR_CONFIG_NUM     = 11,     // Configuration number was larger than 1
    USB_CONFIG_ERR_CONFIG_ATTR    = 12,     // Configuration attributes violate USB specification
    USB_CONFIG_ERR_INTERFACE_LEN  = 13,     // Length of an interface descriptor is wrong
    USB_CONFIG_ERR_INTERFACE_TYPE = 14,     // Expected an interface descriptor, but found something else
    USB_CONFIG_ERR_INTERFACE_ALT  = 15,     // Alternate interfaces are not allowed
    USB_CONFIG_ERR_NO_ENDPOINT    = 16,     // An interface was defined without endpoints
    USB_CONFIG_ERR_ENDPOINT_LEN   = 17,     // Length of an endpoint descriptor is wrong
    USB_CONFIG_ERR_ENDPOINT_TYPE  = 18,     // Expected an endpoint descriptor, but found something else
    USB_CONFIG_ERR_ENDPOINT_ATTR  = 19,     // Endpoint attributes voilate USB specification
    USB_CONFIG_ERR_STRING_SIZE    = 20,     // String descriptor length and header size does not match
    USB_CONFIG_ERR_STRING_TYPE    = 21,     // String descriptor type mismatch
    USB_CONFIG_ERR_GENERIC_DIR    = 22,     // Generic descriptor data direction (bmRequestType) can only be IN
    USB_CONFIG_ERR_UNKNOWN_RECORD = 23,     // Unknown record type found in configuration descriptor list
    USB_CONFIG_ERR_ENDPOINT_RANGE = 24,     // Endpoint number either zero or too large
    USB_CONFIG_ERR_DUP_ENDPOINT   = 25,     // Same endpoint number is used twice in the configuration list
    USB_CONFIG_ERR_DUP_INTERFACE  = 26,     // Same interface number is used twice in the configuration list
    USB_CONFIG_ERR_TOO_MANY_ITFC  = 27,     // More than 10 interfaces were defined in the configuration list (isn't that a little excessive?)
    USB_CONFIG_ERR_NO_CONTROLLER  = 28,     // No such controller in this device
    USB_CONFIG_ERR_STARTED        = 29      // An attempt was made to change the configuration of an active Controller
};

//--//

// TODO:  This structure is no longer in use
struct USB_NAME_CONFIG
{
    char FriendlyName[USB_FRIENDLY_NAME_LENGTH+1];
    UINT8 buffer; // make 4-byte aligned

    static LPCSTR GetDriverName() { return "USB_NAME_CONFIG"; }
};

// TODO:  This structure is no longer in use - it cannot be made to work with the new dynamic configuration
struct USB_CONFIG
{
    HAL_DRIVER_CONFIG_HEADER Header;

    //--//

    char FriendlyName[USB_FRIENDLY_NAME_LENGTH+1];

    USB_DYNAMIC_CONFIGURATION *configuration;

    //--//

    static LPCSTR GetDriverName() { return "USB"; }
};


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

//--//

#define USB_MAX_DATA_PACKET_SIZE 64

struct USB_PACKET64
{
    UINT32 Size;
    UINT8  Buffer[USB_MAX_DATA_PACKET_SIZE];
};

#define USB_NULL_ENDPOINT 0xFF

struct USB_STREAM_MAP
{
    UINT8 RxEP;
    UINT8 TxEP;
};

struct USB_CONTROLLER_STATE;

typedef void (*USB_NEXT_CALLBACK)( USB_CONTROLLER_STATE* );

struct USB_CONTROLLER_STATE
{
    BOOL                                             Initialized;
    UINT8                                            CurrentState;
    UINT8                                            ControllerNum;
    UINT32                                           Event;

    const USB_DYNAMIC_CONFIGURATION*                 Configuration;

    /* Queues & MaxPacketSize must be initialized by the HAL */
    Hal_Queue_KnownSize<USB_PACKET64,USB_QUEUE_PACKET_COUNT> *Queues[USB_MAX_QUEUES];
    UINT8                                                    CurrentPacketOffset[USB_MAX_QUEUES];
    UINT8                                                    MaxPacketSize[USB_MAX_QUEUES];
    BOOL                                                     IsTxQueue[USB_MAX_QUEUES];

    /* Arbitrarily as many streams as endpoints since that is the maximum number of streams
       necessary to represent the maximum number of endpoints */
    USB_STREAM_MAP                                   streams[USB_MAX_QUEUES];

    //--//

    /* used for transferring packets between upper & lower */
    UINT8*                                           Data;
    UINT8                                            DataSize;

    /* USB hardware information */
    UINT8                                            Address;
    UINT8                                            DeviceState;
    UINT8                                            PacketSize;
    UINT8                                            ConfigurationNum;
    UINT32                                           FirstGetDescriptor;

    /* USB status information, used in
       GET_STATUS, SET_FEATURE, CLEAR_FEATURE */
    UINT16                                           DeviceStatus;
    UINT16*                                          EndpointStatus;
    UINT8                                            EndpointCount;
    UINT8                                            EndpointStatusChange;

    /* callback function for getting next packet */
    USB_NEXT_CALLBACK                                DataCallback;

    /* for helping out upper layer during callbacks */
    UINT8*                                           ResidualData;
    UINT16                                           ResidualCount;
    UINT16                                           Expected;

#if defined( USB_REMOTE_WAKEUP)
    BOOL                                             RemoteWkUpRequest;
    USB_REMOTEWKUP_STATE                             RemoteWkUpState;
#endif    
};

//--//

int    USB_GetControllerCount();
BOOL   USB_Initialize  ( int Controller                                );
int    USB_Configure   ( int Controller, const USB_DYNAMIC_CONFIGURATION* Config );
const USB_DYNAMIC_CONFIGURATION * USB_GetConfiguration( int Controller );
BOOL   USB_Uninitialize( int Controller                                );
BOOL   USB_OpenStream  ( int UsbStream,        int writeEP, int readEP );
BOOL   USB_CloseStream ( int UsbStream                                 );
int    USB_Write       ( int UsbStream,  const char* Data, size_t size );
int    USB_Read        ( int UsbStream,        char* Data, size_t size );
BOOL   USB_Flush       ( int UsbStream                                 );
UINT32 USB_GetEvent    ( int Controller,      UINT32 Mask              );
UINT32 USB_SetEvent    ( int Controller,      UINT32 Event             );
UINT32 USB_ClearEvent  ( int Controller,      UINT32 Event             );
UINT8  USB_GetStatus   ( int Controller                                );
void   USB_DiscardData ( int UsbStream ,      BOOL fTx                 );

//--//

struct USB_CONTROLLER_STATE;

USB_CONTROLLER_STATE *CPU_USB_GetState  ( int Controller          );
HRESULT        CPU_USB_Initialize       ( int Controller          );
HRESULT        CPU_USB_Uninitialize     ( int Controller          );
BOOL           CPU_USB_StartOutput      ( USB_CONTROLLER_STATE* State, int endpoint );
BOOL           CPU_USB_RxEnable         ( USB_CONTROLLER_STATE* State, int endpoint );
BOOL           CPU_USB_GetInterruptState(                         );
BOOL           CPU_USB_ProtectPins      ( int Controller, BOOL On );

//--//

#if defined(_MSC_VER)
#pragma pack(pop, USB_DECL_H_)
#endif

#endif // _DRIVERS_USB_DECL_H_

