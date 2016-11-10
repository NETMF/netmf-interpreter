/**
  ******************************************************************************
  * @file    USB_Device/HID_Standalone/Inc/usbd_desc.h
  * @author  MCD Application Team
  * @version V1.4.3
  * @date    29-January-2016
  * @brief   Header for usbd_desc.c module
  ******************************************************************************
  * @attention
  *
  * <h2><center>&copy; COPYRIGHT(c) 2016 STMicroelectronics</center></h2>
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
#ifndef __USBD_DESC_H
#define __USBD_DESC_H

/* Includes ------------------------------------------------------------------*/
#include "usbd_def.h"
#include "usbd_winusb.h"

// definition @ 'solution'.settings
//#define USBD_VID                        0x0483
// definition @ 'solution'.settings
//#define USBD_PID                        0xA08F
// definition @ 'solution'.settings
//#define USBD_MANUFACTURER_STRING        "Microsoft OpenTech"
// definition @ 'solution'.settings
//#define USBD_PRODUCT_HS_STRING          "STM32F4DISCOVERY"
// definition @ 'solution'.settings
//#define USBD_PRODUCT_FS_STRING          "STM32F4DISCOVERY"

#define USBD_LANGID_STRING              0x409

// not used
#define USBD_CONFIGURATION_HS_STRING    "STM32F4DISCOVERY"
#define USBD_INTERFACE_HS_STRING        "a7e70ea2"

// not used
#define USBD_CONFIGURATION_FS_STRING    "STM32F4DISCOVERY"
#define USBD_INTERFACE_FS_STRING        "a7e70ea2"

/* Exported types ------------------------------------------------------------*/
/* Exported constants --------------------------------------------------------*/
#define         DEVICE_ID1          (0x1FFF7A10)
#define         DEVICE_ID2          (0x1FFF7A14)
#define         DEVICE_ID3          (0x1FFF7A18)

#define  USB_SIZ_STRING_SERIAL       0x1A
#define  USB_DESCRIPTOR_MAX_PACKET_SIZE
/* Exported macro ------------------------------------------------------------*/
/* Exported functions ------------------------------------------------------- */
extern USBD_DescriptorsTypeDef WINUSB_Desc;

uint8_t *USBD_WINUSB_SerialStrDescriptor(USBD_SpeedTypeDef speed, uint16_t *length);

#endif /* __USBD_DESC_H */


 
/************************ (C) COPYRIGHT STMicroelectronics *****END OF FILE****/
