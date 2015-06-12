////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Implementation for the MCBSTM32F400 board (STM32F4): Copyright (c) Oberon microsystems, Inc.
//
//  *** STM32F4DISCOVERY USB Configuration ***
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>

//--//

//string descriptor
#define     MANUFACTURER_NAME_SIZE 19   // "Microsoft OpenTech "
// NOTE: Having more than (probably) 32 characters causes the MFUSB KERNEL driver
// to *CRASH* which, of course, causes Windows to crash
#define     PRODUCT_NAME_SIZE      16   // "STM32F4DISCOVERY"
// NOTE: If these two strings are not present, the MFUSB KERNEL driver will *CRASH*
// which, of course, causes Windows to crash
#define     DISPLAY_NAME_SIZE      16   // "STM32F4DISCOVERY"
#define     FRIENDLY_NAME_SIZE      8   // "a7e70ea2"
// index for the strings
#define     MANUFACTURER_NAME_INDEX 1
#define     PRODUCT_NAME_INDEX      2
#define     SERIAL_NUMBER_INDEX     0
// device descriptor
#define     VENDOR_ID          0x0483  // STM32F4 Test VID
#define     PRODUCT_ID         0xA08F  // STM32F4 Test PID
#define     MAX_EP0_SIZE            8
//configuration descriptor
#define     USB_MAX_CURRENT     (100/USB_CURRENT_UNIT)

#define     USB_ATTRIBUTES      (USB_ATTRIBUTE_BASE | USB_ATTRIBUTE_SELF_POWER)

// Configuration for extended descriptor
#define OS_DESCRIPTOR_EX_VERSION            0x0100

/////////////////////////////////////////////////////////////
// The following structure defines the USB descriptor
// for a basic device with a USB debug interface via the
// WinUSB extended Compat ID.
//
// This USB configuration is always used to define the USB
// configuration for TinyBooter.  It is also the default for
// the runtime if there is no USB configuration in the Flash
// configuration sector.

ADS_PACKED struct GNU_PACKED USB_DYNAMIC_CONFIGURATION
{
    USB_DEVICE_DESCRIPTOR        device;
    USB_CONFIGURATION_DESCRIPTOR config;
      USB_INTERFACE_DESCRIPTOR   itfc0;
        USB_ENDPOINT_DESCRIPTOR  ep1;
        USB_ENDPOINT_DESCRIPTOR  ep2;
    USB_STRING_DESCRIPTOR_HEADER manHeader;
      USB_STRING_CHAR            manString[MANUFACTURER_NAME_SIZE];
    USB_STRING_DESCRIPTOR_HEADER prodHeader;
      USB_STRING_CHAR            prodString[PRODUCT_NAME_SIZE];
    USB_STRING_DESCRIPTOR_HEADER string4;
      USB_STRING_CHAR            displayString[DISPLAY_NAME_SIZE];
    USB_STRING_DESCRIPTOR_HEADER string5;
      USB_STRING_CHAR            friendlyString[FRIENDLY_NAME_SIZE];
    USB_OS_STRING_DESCRIPTOR     OS_String;
    USB_XCOMPATIBLE_OS_ID        OS_XCompatible_ID;
    USB_XPROPERTIES_OS_WINUSB    OS_XProperty;
    USB_DESCRIPTOR_HEADER        endList;
};

extern const ADS_PACKED struct GNU_PACKED USB_DYNAMIC_CONFIGURATION UsbDefaultConfiguration;

const struct USB_DYNAMIC_CONFIGURATION UsbDefaultConfiguration =
{
    // Device descriptor
    {
        {
            USB_DEVICE_DESCRIPTOR_MARKER,
            0,
            sizeof(USB_DEVICE_DESCRIPTOR)
        },
        USB_DEVICE_DESCRIPTOR_LENGTH,       // Length of device descriptor
        USB_DEVICE_DESCRIPTOR_TYPE,         // USB device descriptor type
        0x0200,                             // USB Version 2.00 (BCD) (2.0 required for Extended ID recognition)
        0,                                  // Device class (none)
        0,                                  // Device subclass (none)
        0,                                  // Device protocol (none)
        MAX_EP0_SIZE,                       // Endpoint 0 size
        VENDOR_ID,                          // Vendor ID
        PRODUCT_ID,                         // Product ID
        DEVICE_RELEASE_VERSION,             // Product version 1.00 (BCD)
        MANUFACTURER_NAME_INDEX,            // Manufacturer name string index
        PRODUCT_NAME_INDEX,                 // Product name string index
        0,                                  // Serial number string index (none)
        1                                   // Number of configurations
    },

    // Configuration descriptor
    {
        {
            USB_CONFIGURATION_DESCRIPTOR_MARKER,
            0,
            sizeof(USB_CONFIGURATION_DESCRIPTOR)
                + sizeof(USB_INTERFACE_DESCRIPTOR)
                + sizeof(USB_ENDPOINT_DESCRIPTOR)
                + sizeof(USB_ENDPOINT_DESCRIPTOR)
        },
        USB_CONFIGURATION_DESCRIPTOR_LENGTH,
        USB_CONFIGURATION_DESCRIPTOR_TYPE,
        USB_CONFIGURATION_DESCRIPTOR_LENGTH
            + sizeof(USB_INTERFACE_DESCRIPTOR)
            + sizeof(USB_ENDPOINT_DESCRIPTOR)
            + sizeof(USB_ENDPOINT_DESCRIPTOR),
        1,                                          // Number of interfaces
        1,                                          // Number of this configuration
        0,                                          // Config descriptor string index (none)
        USB_ATTRIBUTES,                             // Config attributes
        USB_MAX_CURRENT                             // Device max current draw
    },

    // Interface 0 descriptor
    {
        sizeof(USB_INTERFACE_DESCRIPTOR),
        USB_INTERFACE_DESCRIPTOR_TYPE,
        0,                                          // Interface number
        0,                                          // Alternate number (main)
        2,                                          // Number of endpoints
        0xFF,                                       // Interface class (vendor)
        1,                                          // Interface subclass
        1,                                          // Interface protocol
        0                                           // Interface descriptor string index (none)
    },

    // Endpoint 1 descriptor
    {
        sizeof(USB_ENDPOINT_DESCRIPTOR),
        USB_ENDPOINT_DESCRIPTOR_TYPE,
        USB_ENDPOINT_DIRECTION_IN + 1,
        USB_ENDPOINT_ATTRIBUTE_BULK,
        64,                                         // Endpoint 1 packet size
        0                                           // Endpoint 1 interval
    },

    // Endpoint 2 descriptor
    {
        sizeof(USB_ENDPOINT_DESCRIPTOR),
        USB_ENDPOINT_DESCRIPTOR_TYPE,
        USB_ENDPOINT_DIRECTION_OUT + 2,
        USB_ENDPOINT_ATTRIBUTE_BULK,
        64,                                         // Endpoint 2 packet size
        0                                           // Endpoint 2 interval
    },

    // Manufacturer name string descriptor
    {
        {
            USB_STRING_DESCRIPTOR_MARKER,
            MANUFACTURER_NAME_INDEX,
            sizeof(USB_STRING_DESCRIPTOR_HEADER) + (sizeof(USB_STRING_CHAR) * MANUFACTURER_NAME_SIZE)
        },
        USB_STRING_DESCRIPTOR_HEADER_LENGTH + (sizeof(USB_STRING_CHAR) * MANUFACTURER_NAME_SIZE),
        USB_STRING_DESCRIPTOR_TYPE
    },
    { 'M', 'i', 'c', 'r', 'o', 's', 'o', 'f', 't', ' ', 'O', 'p', 'e', 'n', 'T', 'e', 'c', 'h', ' ' },

    // Product name string descriptor
    {
        {
            USB_STRING_DESCRIPTOR_MARKER,
            PRODUCT_NAME_INDEX,
            sizeof(USB_STRING_DESCRIPTOR_HEADER) + (sizeof(USB_STRING_CHAR) * PRODUCT_NAME_SIZE)
        },
        USB_STRING_DESCRIPTOR_HEADER_LENGTH + (sizeof(USB_STRING_CHAR) * PRODUCT_NAME_SIZE),
        USB_STRING_DESCRIPTOR_TYPE
    },
    { 'S', 'T', 'M', '3', '2', 'F', '4', 'D', 'I', 'S', 'C', 'O', 'V', 'E', 'R', 'Y' },

    // String 4 descriptor (display name)
    {
        {
            USB_STRING_DESCRIPTOR_MARKER,
            USB_DISPLAY_STRING_NUM,
            sizeof(USB_STRING_DESCRIPTOR_HEADER) + (sizeof(USB_STRING_CHAR) * DISPLAY_NAME_SIZE)
        },
        USB_STRING_DESCRIPTOR_HEADER_LENGTH + (sizeof(USB_STRING_CHAR) * DISPLAY_NAME_SIZE),
        USB_STRING_DESCRIPTOR_TYPE
    },
    { 'S', 'T', 'M', '3', '2', 'F', '4', 'D', 'I', 'S', 'C', 'O', 'V', 'E', 'R', 'Y' },

    // String 5 descriptor (friendly name)
    {
        {
            USB_STRING_DESCRIPTOR_MARKER,
            USB_FRIENDLY_STRING_NUM,
            sizeof(USB_STRING_DESCRIPTOR_HEADER) + (sizeof(USB_STRING_CHAR) * FRIENDLY_NAME_SIZE)
        },
        USB_STRING_DESCRIPTOR_HEADER_LENGTH + (sizeof(USB_STRING_CHAR) * FRIENDLY_NAME_SIZE),
        USB_STRING_DESCRIPTOR_TYPE
    },
    { 'a', '7', 'e', '7', '0', 'e', 'a', '2' },

    // OS Descriptor string for Extended OS Compat ID
    {
        {
            USB_STRING_DESCRIPTOR_MARKER,
            OS_DESCRIPTOR_STRING_INDEX,
            sizeof(USB_DESCRIPTOR_HEADER) + OS_DESCRIPTOR_STRING_SIZE
        },
        OS_DESCRIPTOR_STRING_SIZE,
        USB_STRING_DESCRIPTOR_TYPE,
        { 'M', 'S', 'F', 'T', '1', '0', '0' },
        OS_DESCRIPTOR_STRING_VENDOR_CODE,
        0x00
    },

    // OS Extended Compatible ID for WinUSB
    {
        // Generic Descriptor header
        {
            {
                USB_GENERIC_DESCRIPTOR_MARKER,
                0,
                sizeof(USB_GENERIC_DESCRIPTOR_HEADER) + USB_XCOMPATIBLE_OS_SIZE
            },
            USB_REQUEST_TYPE_IN | USB_REQUEST_TYPE_VENDOR,
            OS_DESCRIPTOR_STRING_VENDOR_CODE,
            0,                                              // Intfc # << 8 + Page #
            USB_XCOMPATIBLE_OS_REQUEST                      // Extended Compatible OS ID request
        },
        USB_XCOMPATIBLE_OS_SIZE,                            // Size of this descriptor
        OS_DESCRIPTOR_EX_VERSION,                           // Version 1.00 (BCD)
        USB_XCOMPATIBLE_OS_REQUEST,                         // Extended Compatible OS ID response
        1,                                                  // Only 1 function record
        { 0, 0, 0, 0, 0, 0, 0 },                            // (padding)
        // Extended Compatible OS ID function record
        0,                                                  // Interface 0
        1,                                                  // (reserved)
        { 'W', 'I', 'N', 'U', 'S', 'B',  0,  0 },           // Compatible ID
        {  0,   0,   0,   0,   0,   0,   0,  0 },           // Sub-compatible ID
        { 0, 0, 0, 0, 0, 0 }                                // Padding
    },

    // OS Extended Property 
    {
        // Generic Descriptor header
        {
            {
                USB_GENERIC_DESCRIPTOR_MARKER,
                0,
                sizeof(USB_GENERIC_DESCRIPTOR_HEADER) + USB_XPROPERTY_OS_SIZE_WINUSB
            },
            USB_REQUEST_TYPE_IN | USB_REQUEST_TYPE_VENDOR | USB_REQUEST_TYPE_INTERFACE,
            OS_DESCRIPTOR_STRING_VENDOR_CODE,
            0,                                              // Intfc # << 8 + Page #
            USB_XPROPERTY_OS_REQUEST                        // Extended Property OS ID request
        },
        USB_XPROPERTY_OS_SIZE_WINUSB,                       // Size of this descriptor (78 bytes for guid + 40 bytes for the property name + 24 bytes for other fields = 142 bytes)
        OS_DESCRIPTOR_EX_VERSION,                           // Version 1.00 (BCD)
        USB_XPROPERTY_OS_REQUEST,                           // Extended Compatible OS ID response
        1,                                                  // Only 1 ex property record
        // Extended Property OS ID function record
        0x00000084,                                         // size in bytes 
        EX_PROPERTY_DATA_TYPE__REG_SZ,                      // data type (unicode string)
        40,                                                 // name length
        // property name (null -terminated unicode string: 'DeviceInterfaceGuid\0')
        { 'D','\0', 'e','\0', 'v','\0', 'i','\0', 'c','\0', 'e','\0', 'I','\0', 'n','\0', 't','\0', 'e','\0', 'r','\0', 'f','\0', 'a','\0', 'c','\0', 'e','\0', 'G','\0', 'u','\0', 'i','\0', 'd','\0', '\0','\0' },
        78,                                                 // data length
        // data ({D32D1D64-963D-463E-874A-8EC8C8082CBF})
        { '{','\0', 'D','\0', '3','\0', '2','\0', 'D','\0', '1','\0', 'D','\0', '6','\0', '4','\0', '-','\0', '9','\0', '6','\0', '3','\0', 'D','\0', '-','\0', '4','\0', '6','\0', '3','\0', 'E','\0', '-','\0', '8','\0', '7','\0', '4','\0', 'A','\0', '-','\0', '8','\0', 'E','\0', 'C','\0', '8','\0', 'C','\0', '8','\0', '0','\0', '8','\0', '2','\0', 'C','\0', 'B','\0', 'F','\0', '}','\0', '\0','\0' }
    },

    // End of configuration marker
    {
        USB_END_DESCRIPTOR_MARKER,
        0,
        0
    },
};
