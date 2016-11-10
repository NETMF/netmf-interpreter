/**
  ******************************************************************************
  * @file    usbd_WINUSB_core.h
  * @author  MCD Application Team
  * @version V2.4.2
  * @date    11-December-2015
  * @brief   Header file for the usbd_WINUSB_core.c file.
  ******************************************************************************
  * @attention
  *
  * <h2><center>&copy; COPYRIGHT 2015 STMicroelectronics</center></h2>
  *
  * Licensed under MCD-ST Liberty SW License Agreement V2, (the "License");
  * You may not use this file except in compliance with the License.
  * You may obtain a copy of the License at:
  *
  *        http://www.st.com/software_license_agreement_liberty_v2
  *
  * Unless required by applicable law or agreed to in writing, software 
  * distributed under the License is distributed on an "AS IS" BASIS, 
  * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  * See the License for the specific language governing permissions and
  * limitations under the License.
  *
  ******************************************************************************
  */

/* Define to prevent recursive inclusion -------------------------------------*/ 
#ifndef __USB_WINUSB_CORE_H
#define __USB_WINUSB_CORE_H

#ifdef __cplusplus
 extern "C" {
#endif

/* Includes ------------------------------------------------------------------*/
#include  "usbd_ioreq.h"

/** @addtogroup STM32_USB_DEVICE_LIBRARY
  * @{
  */
  
/** @defgroup USBD_WINUSB
  * @brief This file is the header file for usbd_WINUSB_core.c
  * @{
  */ 


/** @defgroup USBD_WINUSB_Exported_Defines
  * @{
  */ 
#define WINUSB_EPIN_ADDR                0x81
//#define WINUSB_EPIN_SIZE                0x10

#define WINUSB_EPOUT_ADDR               0x02
//#define WINUSB_EPOUT_SIZE               0x10

#define USB_WINUSB_CONFIG_DESC_SIZ      (9 + 9 + 7 + 7)

#define WINUSB_MAX_FS_PACKET            0x40
#define WINUSB_MAX_HS_PACKET            0x40

// interface GUID supported by this device 
#define WINUSB_DEVICE_INTERFACE_GUID    L"{D32D1D64-963D-463E-874A-8EC8C8082CBF}"

// dwPropertyDataType
#define EX_PROPERTY_DATA_TYPE__RESERVED                 0 
#define EX_PROPERTY_DATA_TYPE__REG_SZ                   1 
#define EX_PROPERTY_DATA_TYPE__REG_SZ_ENV               2 
#define EX_PROPERTY_DATA_TYPE__REG_BINARY               3 
#define EX_PROPERTY_DATA_TYPE__REG_DWORD_LITTLE_ENDIAN  4 
#define EX_PROPERTY_DATA_TYPE__REG_DWORD_BIG_ENDIAN     5 
#define EX_PROPERTY_DATA_TYPE__REG_LINK                 6 
#define EX_PROPERTY_DATA_TYPE__REG_MULTI_SZ             7 


#define OS_DESCRIPTOR_STRING_VENDOR_CODE    0xA5
#define OS_DESCRIPTOR_EX_VERSION            0x0100
#define USB_XPROPERTY_OS_REQUEST            0x05
#define USB_XCOMPATIBLE_OS_REQUEST          0x04

// these defines should be in usbd_def.h
//#define	 USB_REQ_GET_OS_FEATURE_DESCRIPTOR				0x20


/**
  * @}
  */ 


/** @defgroup USBD_CORE_Exported_TypesDefinitions
  * @{
  */

////////////////////////////////////////////////////////////////////////////////////////////
/* need to add pack pragma to the following structs with 1 byte width to force the compiler 
to line up data without padding otherwise the USB descriptors won't be properly formed    */  
////////////////////////////////////////////////////////////////////////////////////////////

// OS String Descriptor structure
// see https://msdn.microsoft.com/en-us/windows/hardware/gg463179.aspx
#pragma pack(push, 1)
typedef struct
{
    uint8_t  bLength;
    uint8_t  bDescriptorType;
    uint8_t  qwSignature[2*7];
    uint8_t  bMS_VendorCode;
    const uint8_t  bPad;
} WINUSB_OSStringDescStruct;
#pragma pack (pop)

// Extended Properties OS Feature Descriptor
// see https://msdn.microsoft.com/en-us/windows/hardware/gg463179.aspx
// see https://msdn.microsoft.com/en-us/library/windows/hardware/hh450799(v=vs.85).aspx
#pragma pack(push, 1)
typedef struct
{ 
    uint32_t  dwSize;
    uint32_t  dwPropertyDataType;
    uint16_t  wPropertyNameLength;
    // despite the spec calls for a variable lenght we can have a fixed size here
    // because we need to set this to 'DeviceInterfaceGUID'
    wchar_t  bPropertyName[sizeof("DeviceInterfaceGUID")];
    uint32_t  dwPropertyDataLength;
    // despite the spec calls for a variable lenght we can have a fixed size here
    // because we need to set this to a GUID such as '{8FE6D4D7-49DD-41E7-9486-49AFC6BFE475}'    
    wchar_t  bPropertyData[sizeof("{8FE6D4D7-49DD-41E7-9486-49AFC6BFE475}")];
    
} WINUSB_CustomPropertySectionStruct;
#pragma pack (pop)

#pragma pack(push, 1)
typedef struct
{
    // Header
    uint32_t  dwLength;
    uint16_t  bcdVersion;
    uint16_t  wIndex;
    uint16_t  wCount;
    
    // Custom Property Sections
    // Section 1
    WINUSB_CustomPropertySectionStruct propertySection1;
    
} WINUSB_ExtendedPropertiesDescStruct;
#pragma pack (pop)

#pragma pack(push, 1)
typedef struct
{
    uint8_t  bFirstInterfaceNumber;
    uint8_t  reserved1;
    uint8_t  compatibleID[8];
    uint8_t  subCompatibleID[8];
    uint8_t  reserved2[6];

} WINUSB_CompatIDFunctionStruct;

#pragma pack(push, 1)
typedef struct
{
    // Header
    uint32_t dwLength;
    uint16_t bcdVersion;
    uint16_t wIndex;
    uint8_t  bCount;
    uint8_t  bReserved[7];
    
    // Funcion sections
    // Section 1
    WINUSB_CompatIDFunctionStruct section1;
    
} WINUSB_ExtendedCompatIDOSDescStruct;
#pragma pack (pop)

/**
  * @}
  */ 



/** @defgroup USBD_CORE_Exported_Macros
  * @{
  */ 

/**
  * @}
  */ 

/** @defgroup USBD_CORE_Exported_Variables
  * @{
  */ 

extern USBD_ClassTypeDef  USBD_WINUSB;
#define USBD_WINUSB_CLASS    &USBD_WINUSB
/**
  * @}
  */ 

/** @defgroup USB_CORE_Exported_Functions
  * @{
  */ 
/**
  * @}
  */ 
  

#ifdef __cplusplus
}
#endif

#endif  /* __USB_WINUSB_CORE_H */
/**
  * @}
  */ 

/**
  * @}
  */ 
  
/************************ (C) COPYRIGHT STMicroelectronics *****END OF FILE****/
