////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include <tinyhal.h>
#include <PAL\COM\USB\Usb.h>

//--//

const char* UsbStrings[] = { NULL };

#define     MANUFACTURER_NAME_SIZE 1
#define     PRODUCT_NAME_SIZE      1
#define     DISPLAY_NAME_SIZE      1
#define     FRIENDLY_NAME_SIZE     1


ADS_PACKED struct GNU_PACKED USB_DYNAMIC_CONFIGURATION
{
    USB_DEVICE_DESCRIPTOR           device;
    USB_CONFIGURATION_DESCRIPTOR    config;
    USB_INTERFACE_DESCRIPTOR        itfc0;
    USB_ENDPOINT_DESCRIPTOR         ep1;
    USB_ENDPOINT_DESCRIPTOR         ep2;
    USB_STRING_DESCRIPTOR_HEADER    manHeader;
    USB_STRING_CHAR                 manString[MANUFACTURER_NAME_SIZE];
    USB_STRING_DESCRIPTOR_HEADER    prodHeader;
    USB_STRING_CHAR                 prodString[PRODUCT_NAME_SIZE];
    USB_STRING_DESCRIPTOR_HEADER    string4;
    USB_STRING_CHAR                 displayString[DISPLAY_NAME_SIZE];
    USB_STRING_DESCRIPTOR_HEADER    string5;
    USB_STRING_CHAR                 friendlyString[FRIENDLY_NAME_SIZE];
    USB_OS_STRING_DESCRIPTOR        OS_String;
    USB_XCOMPATIBLE_OS_ID           OS_XCompatible_ID;
    USB_XPROPERTIES_OS_WINUSB       OS_XProperty;
    USB_DESCRIPTOR_HEADER           endList;
};

extern const ADS_PACKED struct GNU_PACKED USB_DYNAMIC_CONFIGURATION UsbDefaultConfiguration;

const ADS_PACKED struct GNU_PACKED USB_DYNAMIC_CONFIGURATION UsbDefaultConfiguration=
{
    {
        {
            0,
            0,
            0,
        },
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
    },
};
