    /**
  ******************************************************************************
  * @file    usbd_winusb.c
  * @author  MCD Application Team
  * @version V2.4.2
  * @date    11-December-2015
  * @brief   This file provides the HID core functions.
  *
  * @verbatim
  *      
  *          ===================================================================      
  *                                WINUSB Class  Description
  *          ===================================================================
  *          
  *
  *
  *
  *           
  *      
  * @note     In HS mode and when the DMA is used, all variables and data structures
  *           dealing with the DMA during the transaction process should be 32-bit aligned.
  *           
  *      
  *  @endverbatim
  *
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
  // https://msdn.microsoft.com/en-us/library/windows/hardware/hh450799(v=vs.85).aspx
  

/* Includes ------------------------------------------------------------------*/
#include <tinyhal.h>
#include <pal\com\usb\USB.h>

#include "usbd_winusb.h"
#include "usbd_desc.h"
#include "usbd_ctlreq.h"

uint8_t rxBuffer[80];
uint8_t txBuffer[80];


static uint8_t  USBD_WINUSB_Init (USBD_HandleTypeDef *pdev, 
                               uint8_t cfgidx);

static uint8_t  USBD_WINUSB_DeInit (USBD_HandleTypeDef *pdev, 
                                 uint8_t cfgidx);

static uint8_t  USBD_WINUSB_Setup (USBD_HandleTypeDef *pdev, 
                                USBD_SetupReqTypedef *req);

static uint8_t  *USBD_WINUSB_GetCfgDesc (uint16_t *length);

static uint8_t  *USBD_WINUSB_GetDeviceQualifierDesc (uint16_t *length);

static uint8_t  USBD_WINUSB_DataIn (USBD_HandleTypeDef *pdev, uint8_t epnum);

static uint8_t  USBD_WINUSB_DataOut (USBD_HandleTypeDef *pdev, uint8_t epnum);

static uint8_t  USBD_WINUSB_EP0_RxReady (USBD_HandleTypeDef *pdev);

static uint8_t  USBD_WINUSB_EP0_TxReady (USBD_HandleTypeDef *pdev);

static uint8_t  USBD_WINUSB_SOF (USBD_HandleTypeDef *pdev);

static uint8_t  USBD_WINUSB_IsoINIncomplete (USBD_HandleTypeDef *pdev, uint8_t epnum);

static uint8_t  USBD_WINUSB_IsoOutIncomplete (USBD_HandleTypeDef *pdev, uint8_t epnum);

static uint8_t  *USBD_WINUSB_GetUsrStrDescriptor(struct _USBD_HandleTypeDef *pdev, uint8_t index,  uint16_t *length);

static uint8_t *USBD_WINUSB_OSStringDescriptor(USBD_SpeedTypeDef speed , uint16_t *length);
static uint8_t *USBD_WINUSB_ExtendedPropertiesOSFeatureDescriptor(USBD_SpeedTypeDef speed , uint16_t *length);
static uint8_t *USBD_WINUSB_ExtendedCompatIDOSDescriptor(USBD_SpeedTypeDef speed , uint16_t *length);
static uint8_t  USBD_WINUSB_StandardRequest(USBD_HandleTypeDef *pdev, USBD_SetupReqTypedef *req);
static uint8_t  USBD_WINUSB_VendorRequest(USBD_HandleTypeDef *pdev, USBD_SetupReqTypedef *req);
static uint8_t  USBD_WINUSB_GetMSExtendedCompatIDOSDescriptor (USBD_HandleTypeDef *pdev, USBD_SetupReqTypedef *req);
static uint8_t  USBD_WINUSB_GetMSExtendedPropertiesOSDescriptor (USBD_HandleTypeDef *pdev, USBD_SetupReqTypedef *req);
static uint8_t  USBD_WINUSB_VendorRequestDevice(USBD_HandleTypeDef *pdev, USBD_SetupReqTypedef *req);
static uint8_t  USBD_WINUSB_VendorRequestInterface(USBD_HandleTypeDef *pdev, USBD_SetupReqTypedef *req);

USBD_ClassTypeDef  USBD_WINUSB = 
{
  USBD_WINUSB_Init,
  USBD_WINUSB_DeInit,
  USBD_WINUSB_Setup,
  USBD_WINUSB_EP0_TxReady,  
  USBD_WINUSB_EP0_RxReady,
  USBD_WINUSB_DataIn,
  USBD_WINUSB_DataOut,
  NULL, // was USBD_WINUSB_SOF,
  NULL, // was USBD_WINUSB_IsoINIncomplete,
  NULL, // was USBD_WINUSB_IsoOutIncomplete,      
  USBD_WINUSB_GetCfgDesc,
  USBD_WINUSB_GetCfgDesc, 
  USBD_WINUSB_GetCfgDesc,
  USBD_WINUSB_GetDeviceQualifierDesc,
  USBD_WINUSB_GetUsrStrDescriptor,
};

/* USB WINUSB device Configuration Descriptor */
const uint8_t USBD_WINUSB_CfgDesc[USB_WINUSB_CONFIG_DESC_SIZ]=
{
  0x09, /* bLength: Configuation Descriptor size */
  USB_DESC_TYPE_CONFIGURATION, /* bDescriptorType: Configuration */
  USB_WINUSB_CONFIG_DESC_SIZ,  /* wTotalLength: Bytes returned */
  0x00,
  0x01,         /*bNumInterfaces: 1 interface*/
  0x01,         /*bConfigurationValue: Configuration value*/
  0x00,         /*iConfiguration: Index of string descriptor describing the configuration*/
  0xE0,         /*bmAttributes: bus powered and Supports Remote Wakeup */
  0x32,         /*MaxPower 100 mA: this current is used for detecting Vbus*/
  
  // Interface 0 descriptor
  0x09,   /* bLength: Interface Descriptor size */
  0x04,   /* bDescriptorType: */
  0x00,   /* bInterfaceNumber: Number of Interface */
  0x00,   /* bAlternateSetting: Alternate setting */
  0x02,   /* bNumEndpoints*/
  0xFF,   /* bInterfaceClass: Vendor  Class */
  0x01,   /* bInterfaceSubClass : none*/
  0x01,   /* nInterfaceProtocol */
  0x00,   /* iInterface: */
  
  // Endpoint 1 descriptor  
  0x07,   /*Endpoint descriptor length = 7*/
  0x05,   /*Endpoint descriptor type */
  WINUSB_EPIN_ADDR,   /*Endpoint address (IN, address 1) */
  0x02,   /*Bulk endpoint type */
  LOBYTE(WINUSB_MAX_FS_PACKET),
  HIBYTE(WINUSB_MAX_FS_PACKET),
  0x00,   /*Polling interval in milliseconds */
  
  // Endpoint 2 descriptor
  0x07,   /*Endpoint descriptor length = 7*/
  0x05,   /*Endpoint descriptor type */
  WINUSB_EPOUT_ADDR,   /*Endpoint address (OUT, address 1) */
  0x02,   /*Bulk endpoint type */
  LOBYTE(WINUSB_MAX_FS_PACKET),
  HIBYTE(WINUSB_MAX_FS_PACKET),
  0x00,   /*Polling interval in milliseconds */
};
  
/* USB Standard Device Descriptor */
const uint8_t USBD_WINUSB_DeviceQualifierDesc[USB_LEN_DEV_QUALIFIER_DESC]=
{
  USB_LEN_DEV_QUALIFIER_DESC,
  USB_DESC_TYPE_DEVICE_QUALIFIER,
  0x00,
  0x02,
  0x00,
  0x00,
  0x00,
  0x40,
  0x01,
  0x00,
};

// Microsoft OS descriptor
const WINUSB_OSStringDescStruct USBD_WINUSB_OS_String_Descriptor= 
{
    sizeof(USBD_WINUSB_OS_String_Descriptor),
    USB_DESC_TYPE_STRING,
    // this is the Unicode representation of "MSFT100" which can't be used here as a string because it would include the terminator thus overflowing the expected 14 bytes lenght
    {0x4D, 0x00, 0x53, 0x00, 0x46, 0x00, 0x54, 0x00, 0x31, 0x00, 0x30, 0x00, 0x30, 0x00}, 
    OS_DESCRIPTOR_STRING_VENDOR_CODE,
    0
};

const WINUSB_ExtendedCompatIDOSDescStruct USBD_WINUSB_Extended_Compat_ID_OS_Descriptor= 
{
    // Header
    sizeof(USBD_WINUSB_Extended_Compat_ID_OS_Descriptor),
    OS_DESCRIPTOR_EX_VERSION,
    USB_XCOMPATIBLE_OS_REQUEST,
    0x01,
    {0},
    
    // Section 1
    {
        0,
        0,
        "WINUSB",
        {0},
        {0},  
    },
};

// Extended properties descriptior
const WINUSB_ExtendedPropertiesDescStruct USBD_WINUSB_Extended_Properties_OS_Feature_Descriptor =
{
    // Header
    sizeof(USBD_WINUSB_Extended_Properties_OS_Feature_Descriptor),
    OS_DESCRIPTOR_EX_VERSION,
    USB_XPROPERTY_OS_REQUEST,
    0x1,

    // Section 1
    {
        sizeof(USBD_WINUSB_Extended_Properties_OS_Feature_Descriptor.propertySection1), //0x84,
        EX_PROPERTY_DATA_TYPE__REG_SZ,
        sizeof(USBD_WINUSB_Extended_Properties_OS_Feature_Descriptor.propertySection1.bPropertyName), //0x28,
        L"DeviceInterfaceGuid",
        sizeof(USBD_WINUSB_Extended_Properties_OS_Feature_Descriptor.propertySection1.bPropertyData), //0x4E,
        WINUSB_DEVICE_INTERFACE_GUID,       
    },
};

/**
  * @brief  USBD_WINUSB_Init
  *         Initialize the WINUSB interface
  * @param  pdev: device instance
  * @param  cfgidx: Configuration index
  * @retval status
  */
static uint8_t  USBD_WINUSB_Init (USBD_HandleTypeDef *pdev, 
                               uint8_t cfgidx)
{
  uint8_t ret = 0;

    USBD_LL_FlushEP(pdev, WINUSB_EPOUT_ADDR);
    USBD_LL_FlushEP(pdev, WINUSB_EPIN_ADDR);

    /* Open EP OUT */
    USBD_LL_OpenEP(pdev,
                WINUSB_EPOUT_ADDR,
                USBD_EP_TYPE_BULK,
                WINUSB_MAX_FS_PACKET);

    /* Open EP IN */
    USBD_LL_OpenEP(pdev,
                WINUSB_EPIN_ADDR,
                USBD_EP_TYPE_BULK,
                WINUSB_MAX_FS_PACKET);

    /* Prepare Out endpoint to receive next packet */
    USBD_LL_PrepareReceive(pdev,
                        WINUSB_EPOUT_ADDR,
                        &rxBuffer[0],
                        WINUSB_MAX_FS_PACKET);    

    return ret;
}

/**
  * @brief  USBD_WINUSB_Init
  *         DeInitialize the WINUSB layer
  * @param  pdev: device instance
  * @param  cfgidx: Configuration index
  * @retval status
  */
static uint8_t  USBD_WINUSB_DeInit (USBD_HandleTypeDef *pdev, 
                                 uint8_t cfgidx)
{
  // close end points/
  USBD_LL_CloseEP(pdev, WINUSB_EPOUT_ADDR);
  USBD_LL_CloseEP(pdev, WINUSB_EPIN_ADDR);

  // free class
  pdev->pClassData  = NULL;

  return USBD_OK;
}

/**
  * @brief  USBD_WINUSB_Setup
  *         Handle the WINUSB specific requests
  * @param  pdev: instance
  * @param  req: usb requests
  * @retval status
  */
static uint8_t  USBD_WINUSB_Setup (USBD_HandleTypeDef *pdev, USBD_SetupReqTypedef *req)
{
  switch (req->bmRequest & USB_REQ_TYPE_MASK)
  {
    case USB_REQ_TYPE_CLASS :  
        break;
        
    case USB_REQ_TYPE_STANDARD:
        return USBD_WINUSB_StandardRequest(pdev, req);

    case USB_REQ_TYPE_VENDOR:
        return USBD_WINUSB_VendorRequest(pdev, req);
                        
    default:
      USBD_CtlError (pdev, req);
      return USBD_FAIL;                 
  }
  
  return USBD_OK;
}


/**
  * @brief  USBD_WINUSB_GetCfgDesc 
  *         return configuration descriptor
  * @param  length : pointer data length
  * @retval pointer to descriptor buffer
  */
static uint8_t  *USBD_WINUSB_GetCfgDesc (uint16_t *length)
{
  *length = sizeof (USBD_WINUSB_CfgDesc);
  return (uint8_t*)USBD_WINUSB_CfgDesc;
}

/**
* @brief  DeviceQualifierDescriptor 
*         return Device Qualifier descriptor
* @param  length : pointer data length
* @retval pointer to descriptor buffer
*/
uint8_t  *USBD_WINUSB_DeviceQualifierDescriptor (uint16_t *length)
{
  *length = sizeof (USBD_WINUSB_DeviceQualifierDesc);
  return (uint8_t*)USBD_WINUSB_DeviceQualifierDesc;
}


/**
  * @brief  USBD_WINUSB_DataIn
  *         handle data IN Stage
  * @param  pdev: device instance
  * @param  epnum: endpoint index
  * @retval status
  */
static uint8_t  USBD_WINUSB_DataIn (USBD_HandleTypeDef *pdev, uint8_t epnum)
{
    USB_CONTROLLER_STATE* state = (USB_CONTROLLER_STATE*)pdev->pUserData;
    
    if(epnum == 1)
    {
        // Tx data endpoint
        USB_PACKET64* usbPacket = USB_TxDequeue(state, epnum, TRUE);
        
        if(usbPacket)
        {  
            // data to send
            // Transmit next packet
                            
            // copy data to be transmitted from USB packet struct to tx buffer  
            memcpy(&txBuffer[0], usbPacket->Buffer, usbPacket->Size);

            USBD_LL_Transmit(pdev, WINUSB_EPIN_ADDR, &txBuffer[0], usbPacket->Size);
            // USB_Debug( 's' );
        }

    }

    return USBD_OK;
}

/**
  * @brief  USBD_WINUSB_EP0_RxReady
  *         handle EP0 Rx Ready event
  * @param  pdev: device instance
  * @retval status
  */
static uint8_t  USBD_WINUSB_EP0_RxReady (USBD_HandleTypeDef *pdev)
{
  return USBD_OK;
}

/**
  * @brief  USBD_WINUSB_EP0_TxReady
  *         handle EP0 TRx Ready event
  * @param  pdev: device instance
  * @retval status
  */
static uint8_t  USBD_WINUSB_EP0_TxReady(USBD_HandleTypeDef *pdev)
{
  return USBD_OK;
}
/**
  * @brief  USBD_WINUSB_SOF
  *         handle SOF event
  * @param  pdev: device instance
  * @retval status
  */
static uint8_t  USBD_WINUSB_SOF (USBD_HandleTypeDef *pdev)
{
    
  return USBD_OK;
}
/**
  * @brief  USBD_WINUSB_IsoINIncomplete
  *         handle data ISO IN Incomplete event
  * @param  pdev: device instance
  * @param  epnum: endpoint index
  * @retval status
  */
static uint8_t  USBD_WINUSB_IsoINIncomplete (USBD_HandleTypeDef *pdev, uint8_t epnum)
{

  return USBD_OK;
}
/**
  * @brief  USBD_WINUSB_IsoOutIncomplete
  *         handle data ISO OUT Incomplete event
  * @param  pdev: device instance
  * @param  epnum: endpoint index
  * @retval status
  */
static uint8_t  USBD_WINUSB_IsoOutIncomplete (USBD_HandleTypeDef *pdev, uint8_t epnum)
{

  return USBD_OK;
}
/**
  * @brief  USBD_WINUSB_DataOut
  *         handle data OUT Stage
  * @param  pdev: device instance
  * @param  epnum: endpoint index
  * @retval status
  */
static uint8_t  USBD_WINUSB_DataOut (USBD_HandleTypeDef *pdev, uint8_t epnum)
{
    USB_PACKET64* usbPacket;
    USB_CONTROLLER_STATE* state;
    state = (USB_CONTROLLER_STATE*)pdev->pUserData;

    if(epnum == 2)
    {
        // EP2
        
        BOOL bufferIsFull;
        usbPacket = USB_RxEnqueue(state, epnum, bufferIsFull);

        if(usbPacket == NULL)
        {  
            // should not happen
            //USB_Debug( '?' );
            //_ASSERT( 0 );
        }

        // get how many bytes are available in the buffer...
        // ... and set USB packet size
        usbPacket->Size = USBD_LL_GetRxDataSize (pdev , epnum);

        // copy received data to USB packet struct  
        memcpy(usbPacket->Buffer, &rxBuffer[0], usbPacket->Size);

        // prepare WINUSB_EPOUT_ADDR endpoint to receive next packet
        USBD_LL_PrepareReceive(pdev, epnum, &rxBuffer[0], WINUSB_MAX_FS_PACKET);
    }  

    return USBD_OK;
}

/**
* @brief  DeviceQualifierDescriptor 
*         return Device Qualifier descriptor
* @param  length : pointer data length
* @retval pointer to descriptor buffer
*/
static uint8_t  *USBD_WINUSB_GetDeviceQualifierDesc (uint16_t *length)
{
  *length = sizeof (USBD_WINUSB_DeviceQualifierDesc);
  return (uint8_t*)USBD_WINUSB_DeviceQualifierDesc;
}

/**
* @brief  GetUsrStrDescriptor
*         return non standard string descriptor 
* @param  pdev: device instance
* @param  index : descriptor index
* @param  length : pointer data length
* @retval pointer to descriptor buffer
*/
static uint8_t * USBD_WINUSB_GetUsrStrDescriptor(struct _USBD_HandleTypeDef *pdev, uint8_t index,  uint16_t *length)
{
  *length = 0;
  
  // MS OS String Descriptor is in index 0xEE
  if (index ==  0xEE)
  {
    *length =  sizeof(USBD_WINUSB_OS_String_Descriptor);
    return (uint8_t*)&USBD_WINUSB_OS_String_Descriptor;  
  }
  return NULL;
}


static uint8_t *USBD_WINUSB_ExtendedPropertiesOSFeatureDescriptor(USBD_SpeedTypeDef speed , uint16_t *length)
{
   *length =  sizeof(USBD_WINUSB_Extended_Properties_OS_Feature_Descriptor);
   return (uint8_t*)&USBD_WINUSB_Extended_Properties_OS_Feature_Descriptor;  
}

static uint8_t *USBD_WINUSB_ExtendedCompatIDOSDescriptor(USBD_SpeedTypeDef speed , uint16_t *length)
{
  *length =  sizeof(USBD_WINUSB_Extended_Compat_ID_OS_Descriptor);
  return (uint8_t*)&USBD_WINUSB_Extended_Compat_ID_OS_Descriptor;  
}

static uint8_t  USBD_WINUSB_StandardRequest(USBD_HandleTypeDef *pdev, USBD_SetupReqTypedef *req)
{
  switch (req->bRequest)
  {
  case USB_REQ_GET_INTERFACE :
    break;

  case USB_REQ_SET_INTERFACE :
    break;

  case USB_REQ_CLEAR_FEATURE:

    /* Flush the FIFO and Clear the stall status */
    USBD_LL_FlushEP(pdev, (uint8_t)req->wIndex);

    /* Re-activate the EP */
    USBD_LL_CloseEP (pdev , (uint8_t)req->wIndex);
    if((((uint8_t)req->wIndex) & 0x80) == 0x80)
    {
      /* Open EP IN */
      USBD_LL_OpenEP(pdev, WINUSB_EPIN_ADDR, USBD_EP_TYPE_BULK, WINUSB_MAX_FS_PACKET);
    }
    else
    {
      /* Open EP OUT */
      USBD_LL_OpenEP(pdev, WINUSB_EPOUT_ADDR, USBD_EP_TYPE_BULK, WINUSB_MAX_FS_PACKET);
    }
    break;
    
  default:
    USBD_CtlError(pdev , req);
    return USBD_FAIL;
  }
  
  return USBD_OK;
}

static uint8_t USBD_WINUSB_VendorRequest(USBD_HandleTypeDef *pdev, USBD_SetupReqTypedef *req)
{
    uint16_t len;
    uint8_t *pbuf;
  
    switch ( req->bmRequest & USB_REQ_RECIPIENT_MASK )
    {
        case USB_REQ_RECIPIENT_DEVICE:
            if(req->bRequest == OS_DESCRIPTOR_STRING_VENDOR_CODE)
            {
                return USBD_WINUSB_GetMSExtendedCompatIDOSDescriptor(pdev, req);
            }
            else
            {
                return USBD_WINUSB_VendorRequestDevice(pdev, req);
            }
            break;
        
        case USB_REQ_RECIPIENT_INTERFACE:
            if(req->bRequest ==  OS_DESCRIPTOR_STRING_VENDOR_CODE)
            {
                return USBD_WINUSB_GetMSExtendedPropertiesOSDescriptor(pdev, req);
            }
            else if(req->bRequest ==  USB_REQ_GET_DESCRIPTOR)
            {
                switch ((uint8_t)(req->wValue))
                {
                    case USBD_IDX_CONFIG_STR:
                        // NETMF specific
                        // sending the product string descriptor here
                        pbuf = pdev->pDesc->GetProductStrDescriptor(pdev->dev_speed, &len);
                        break;
                    
                    case USBD_IDX_INTERFACE_STR:
                        // NETMF specific
                        // sending the device serial number to have an unique ID for each connected device in MFDeploy and VStudio 
                        pbuf = pdev->pDesc->GetSerialStrDescriptor(pdev->dev_speed, &len);
                        break;    
                }
                
                if((len != 0)&& (req->wLength != 0))
                {
                    
                    len = MIN(len , req->wLength);
                    
                    USBD_CtlSendData (pdev, 
                                    pbuf,
                                    len);
                }
                                
                return USBD_OK;
            }
            else
            {
                return USBD_WINUSB_VendorRequestInterface(pdev, req);
            }
        
        case USB_REQ_RECIPIENT_ENDPOINT:
        default:
            break;
    }
    
    USBD_CtlError(pdev , req);
    return USBD_FAIL;
}

static uint8_t  USBD_WINUSB_VendorRequestDevice(USBD_HandleTypeDef *pdev, USBD_SetupReqTypedef *req)
{
  USBD_CtlError(pdev , req);
  return USBD_FAIL;
}

static uint8_t  USBD_WINUSB_VendorRequestInterface(USBD_HandleTypeDef *pdev, USBD_SetupReqTypedef *req)
{
  return USBD_OK;
}

static uint8_t USBD_WINUSB_GetMSExtendedCompatIDOSDescriptor(USBD_HandleTypeDef *pdev, USBD_SetupReqTypedef *req)
{
    switch (req->wIndex)
    {
        case 0x04:
            USBD_CtlSendData (pdev, (uint8_t*)&USBD_WINUSB_Extended_Compat_ID_OS_Descriptor, req->wLength);
            break;
        default:
            USBD_CtlError(pdev , req);
            return USBD_FAIL;
    }
    return USBD_OK;
}

static uint8_t USBD_WINUSB_GetMSExtendedPropertiesOSDescriptor(USBD_HandleTypeDef *pdev, USBD_SetupReqTypedef *req)
{
    uint8_t byInterfaceIndex = (uint8_t)req->wValue;
    uint16_t len;
    uint8_t *pbuf;
    
    if ( req->wIndex != 0x05 )
    {
        USBD_CtlError(pdev , req);
        return USBD_FAIL;
    }
    
    switch ( byInterfaceIndex )
    {
        case 0:
        case 1:   
            USBD_CtlSendData (pdev, (uint8_t*)&USBD_WINUSB_Extended_Properties_OS_Feature_Descriptor, req->wLength);
            break;

        default:
            USBD_CtlError(pdev , req);
            return USBD_FAIL;
    }
    
    return USBD_OK;
}
