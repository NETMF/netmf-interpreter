////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SPOT_Hardware_Usb.h"

static HRESULT UnmarshalConfigDescriptor( CLR_RT_HeapBlock* descriptorReference, const USB_CONFIGURATION_DESCRIPTOR* nativeConfiguration );
static HRESULT AllocateConfigurationBytes( CLR_RT_HeapBlock_Array* descriptors, CLR_RT_HeapBlock* configurationByteArray );
static HRESULT MarhalConfigDescriptor( CLR_RT_HeapBlock* configurationDescriptor, CLR_UINT8* &nativePtr );

HRESULT Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_UsbController::get_Status___MicrosoftSPOTHardwareUsbClientUsbControllerPortState( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_INT32         status = USB_DEVICE_STATE_UNINITIALIZED;
    CLR_INT32         controllerIndex;
    CLR_RT_HeapBlock* pThis;

    pThis = stack.This(); FAULT_ON_NULL(pThis);

    controllerIndex = pThis[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_UsbController::FIELD__m_controllerIndex ].NumericByRef().s4;

    if( controllerIndex >= TOTAL_USB_CONTROLLER )
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }
    else
    {
        status = USB_GetStatus( controllerIndex );
    }

    stack.SetResult_I4( status );
        
    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_UsbController::nativeStart___BOOLEAN( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_INT8          retVal = FALSE;
    CLR_INT32         controllerIndex;
    CLR_RT_HeapBlock* pThis;

    pThis = stack.This(); FAULT_ON_NULL(pThis);

    controllerIndex = pThis[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_UsbController::FIELD__m_controllerIndex ].NumericByRef().s4;

    if( controllerIndex >= TOTAL_USB_CONTROLLER )
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }
    else
    {
        // this call is used to load the configuration from the config sector (if it exists)
        USB_GetConfiguration( controllerIndex );
        
        retVal = USB_Initialize( controllerIndex );
    }

    stack.SetResult_I4( retVal );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_UsbController::nativeStop___BOOLEAN( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_INT8          retVal = FALSE;
    CLR_INT32         controllerIndex;
    CLR_RT_HeapBlock* pThis;

    pThis = stack.This(); FAULT_ON_NULL(pThis);

    controllerIndex = pThis[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_UsbController::FIELD__m_controllerIndex ].NumericByRef().s4;

    if( controllerIndex >= TOTAL_USB_CONTROLLER )
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }
    else
    {
        retVal = USB_Uninitialize( controllerIndex );
    }
    stack.SetResult_I4( retVal );

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_UsbController::get_Count___STATIC__I4( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();
    
    CLR_INT32 retVal = USB_GetControllerCount();
    stack.SetResult_I4( retVal );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_UsbController::get_Configuration___MicrosoftSPOTHardwareUsbClientConfiguration( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_INT32         controllerIndex, nDescriptors, descriptorIndex;

    CLR_RT_HeapBlock* pThis;                // Points to the UsbController object (this)
    CLR_RT_HeapBlock* configuration;        // Pointer to configuration structure
    CLR_RT_HeapBlock* descriptorArrayRef;   // Pointer to reference to managed array of descriptors
    CLR_RT_HeapBlock_Array* descriptors;    // This will point to the array of descriptors
    const USB_DYNAMIC_CONFIGURATION* nativeConfig;
    const USB_DESCRIPTOR_HEADER*     nativeDescriptor;

    CLR_RT_HeapBlock& configurationRef = stack.PushValueAndClear(); 

    pThis = stack.This(); FAULT_ON_NULL(pThis);

    controllerIndex = pThis[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_UsbController::FIELD__m_controllerIndex ].NumericByRef().s4;
    if( controllerIndex >= TOTAL_USB_CONTROLLER )
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
    }

    TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex( configurationRef, g_CLR_RT_WellKnownTypes.m_UsbClientConfiguration ));

    // Obtain a pointer to the null Descriptor array reference in the Configuration
    configuration = configurationRef.Dereference(); FAULT_ON_NULL(configuration);
    descriptorArrayRef = &(configuration[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration::FIELD__descriptors ]);

    // Discover how many descriptors there are in the native configuration
    nativeConfig = USB_GetConfiguration( controllerIndex );
    FAULT_ON_NULL(nativeConfig);      // If no configuration has been set
    nativeDescriptor = (const USB_DESCRIPTOR_HEADER*)nativeConfig;
    for( nDescriptors = 0; nativeDescriptor->marker != USB_END_DESCRIPTOR_MARKER; nDescriptors++ )
    {
        nativeDescriptor = nativeDescriptor->next(nativeDescriptor);
    }

    // Allocate an array of Descriptors for the returned Configuration
    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance( *descriptorArrayRef, nDescriptors, g_CLR_RT_WellKnownTypes.m_UsbClientConfiguration__Descriptor ));
    descriptors = descriptorArrayRef->DereferenceArray(); FAULT_ON_NULL(descriptors);

    // Fill the array with information from the native Configuration
    nativeDescriptor = (const USB_DESCRIPTOR_HEADER*)nativeConfig;     // Return to the start of the native Configuration
    descriptorIndex = 0;
    while( nativeDescriptor->marker != USB_END_DESCRIPTOR_MARKER )
    {
        CLR_UINT32        size;
        CLR_RT_HeapBlock* descriptorRef;       // Pointer to descriptor class reference
        CLR_RT_HeapBlock* descriptor;          // Pointer to descriptor class

        // Get a pointer to the Descriptor class to fill in
        descriptorRef = (CLR_RT_HeapBlock*)descriptors->GetElement( descriptorIndex ); FAULT_ON_NULL(descriptorRef);

        // for each descriptorREf, we need to protect it from GC while we go through the loop

        switch( nativeDescriptor->marker )
        {
        case USB_DEVICE_DESCRIPTOR_MARKER:
            {
                const USB_DEVICE_DESCRIPTOR* nativeDevice;

                nativeDevice = (const USB_DEVICE_DESCRIPTOR*)nativeDescriptor;
                // Create a Configuration.DeviceDescriptor in the array
                TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex( *descriptorRef, g_CLR_RT_WellKnownTypes.m_UsbClientConfiguration__DeviceDescriptor ));
                descriptor   = descriptorRef->Dereference(); FAULT_ON_NULL(descriptor);
                descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__Descriptor      ::FIELD__index           ].SetInteger( (CLR_INT32)0                             );
                descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__DeviceDescriptor::FIELD__bcdDevice       ].SetInteger( (CLR_INT16)nativeDevice->bcdDevice       );
                descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__DeviceDescriptor::FIELD__idVendor        ].SetInteger( (CLR_INT16)nativeDevice->idVendor        );
                descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__DeviceDescriptor::FIELD__idProduct       ].SetInteger( (CLR_INT16)nativeDevice->idProduct       );
                descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__DeviceDescriptor::FIELD__iManufacturer   ].SetInteger( (CLR_INT8 )nativeDevice->iManufacturer   );
                descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__DeviceDescriptor::FIELD__iProduct        ].SetInteger( (CLR_INT8 )nativeDevice->iProduct        );
                descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__DeviceDescriptor::FIELD__iSerialNumber   ].SetInteger( (CLR_INT8 )nativeDevice->iSerialNumber   );
                descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__DeviceDescriptor::FIELD__bDeviceClass    ].SetInteger( (CLR_INT8 )nativeDevice->bDeviceClass    );
                descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__DeviceDescriptor::FIELD__bDeviceSubClass ].SetInteger( (CLR_INT8 )nativeDevice->bDeviceSubClass );
                descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__DeviceDescriptor::FIELD__bDeviceProtocol ].SetInteger( (CLR_INT8 )nativeDevice->bDeviceProtocol );
                descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__DeviceDescriptor::FIELD__bMaxPacketSize0 ].SetInteger( (CLR_INT8 )nativeDevice->bMaxPacketSize0 );
                descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__DeviceDescriptor::FIELD__bcdUSB          ].SetInteger( (CLR_INT16)nativeDevice->bcdUSB          );
            }
            break;

        case USB_CONFIGURATION_DESCRIPTOR_MARKER:
            {
                const USB_CONFIGURATION_DESCRIPTOR* nativeConfiguration;

                nativeConfiguration = (const USB_CONFIGURATION_DESCRIPTOR*)nativeDescriptor;
                // Create a Configuration.ConfigurationDescriptor in the array
                TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex( *descriptorRef, g_CLR_RT_WellKnownTypes.m_UsbClientConfiguration__ConfigurationDescriptor));
                descriptor = descriptorRef->Dereference(); FAULT_ON_NULL(descriptor);
                descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__Descriptor             ::FIELD__index          ].SetInteger( (CLR_INT32)0                        );
                descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__ConfigurationDescriptor::FIELD__iConfiguration ].SetInteger( nativeConfiguration->iConfiguration );
                descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__ConfigurationDescriptor::FIELD__bmAttributes   ].SetInteger( nativeConfiguration->bmAttributes   );
                descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__ConfigurationDescriptor::FIELD__bMaxPower      ].SetInteger( nativeConfiguration->bMaxPower      );
                TINYCLR_CHECK_HRESULT(UnmarshalConfigDescriptor( descriptorRef, nativeConfiguration ));
            }
            break;

        case USB_STRING_DESCRIPTOR_MARKER:
            {
                const USB_STRING_DESCRIPTOR_HEADER* nativeString;

                nativeString = (const USB_STRING_DESCRIPTOR_HEADER*)nativeDescriptor;
                size = (nativeString->header.size - sizeof(USB_STRING_DESCRIPTOR_HEADER)) / sizeof(USB_STRING_CHAR);        // Size of string in characters
                // Create a Configuration.StringDescriptor in the array
                TINYCLR_CHECK_HRESULT( g_CLR_RT_ExecutionEngine.NewObjectFromIndex( *descriptorRef, g_CLR_RT_WellKnownTypes.m_UsbClientConfiguration__StringDescriptor) );
                descriptor = descriptorRef->Dereference(); FAULT_ON_NULL(descriptor);
                descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__Descriptor::FIELD__index ].SetInteger( (CLR_INT32)nativeString->header.iValue );
                {
                    CLR_UINT16 tempString[ USB_STRING_DESCRIPTOR_MAX_LENGTH ];              // Word aligned storage for uh
                    USB_STRING_CHAR* nativeText = (USB_STRING_CHAR*)&nativeString[ 1 ];     // Native text begins right after header

                    // Copy byte aligned USB character storage to word aligned UTF16 temporary storage for uh
                    for(CLR_UINT32 i = 0; i < size; i++)
                    {
                        tempString[ i ] = (CLR_UINT16)nativeText[ i ];
                    }

                    // Create the UTF8 based string object using the supplied UTF16 string
                    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_String::CreateInstance( descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__StringDescriptor::FIELD__sString ], tempString, size ));
                }
            }
            break;

        case USB_GENERIC_DESCRIPTOR_MARKER:
            {
                CLR_UINT8*        payloadData;
                CLR_UINT8*        nativeData;
                CLR_RT_HeapBlock* payloadArrayRef;
                CLR_RT_HeapBlock_Array* payload;
                const USB_GENERIC_DESCRIPTOR_HEADER* nativeGeneric;

                nativeGeneric = (const USB_GENERIC_DESCRIPTOR_HEADER*)nativeDescriptor;
                size = nativeGeneric->header.size - sizeof(USB_GENERIC_DESCRIPTOR_HEADER);      // Size of payload in bytes

                // Create a Configuration.GenericDescriptor in the array
                TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex( *descriptorRef, g_CLR_RT_WellKnownTypes.m_UsbClientConfiguration__GenericDescriptor ));
                descriptor = descriptorRef->Dereference(); FAULT_ON_NULL(descriptor);
                descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__Descriptor::FIELD__index                ].SetInteger( (CLR_INT32)0 );
                descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__GenericDescriptor::FIELD__bmRequestType ].SetInteger( nativeGeneric->bmRequestType );
                descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__GenericDescriptor::FIELD__bRequest      ].SetInteger( nativeGeneric->bRequest );
                descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__GenericDescriptor::FIELD__wValue        ].SetInteger( nativeGeneric->wValue );
                descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__GenericDescriptor::FIELD__wIndex        ].SetInteger( nativeGeneric->wIndex );

                // Copy the payload
                payloadArrayRef = &(descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__GenericDescriptor::FIELD__payload ]);
                TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance( *payloadArrayRef, size, g_CLR_RT_WellKnownTypes.m_UInt8 ));
                payload = payloadArrayRef->DereferenceArray(); FAULT_ON_NULL(payload);
                payloadData = payload->GetFirstElement(); FAULT_ON_NULL(payloadData);
                nativeData  = (CLR_UINT8*)(&nativeGeneric[ 1 ]);
                memcpy( payloadData, nativeData, size );
            }
            break;

        default:
            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
            break;
        }

        descriptorIndex++;                                              // Move to next managed array element
        nativeDescriptor = nativeDescriptor->next(nativeDescriptor);    // Move to next native Descriptor
    }

    TINYCLR_NOCLEANUP();
}

HRESULT Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_UsbController::set_Configuration___VOID__MicrosoftSPOTHardwareUsbClientConfiguration( CLR_RT_StackFrame& stack )
{
    TINYCLR_HEADER();

    CLR_INT32  retValue, controllerIndex;
    CLR_UINT32 configSize;                     // Size of array allocated for native configuration
    CLR_UINT32 nDescriptors, descriptorIndex;  // Number of descriptors to marshal
    CLR_UINT8* nativeConfiguration;            // Pointer to start of converted native configuration
    CLR_UINT8* nativePtr;                      // Pointer to next byte of native configuration to write
    CLR_RT_HeapBlock* pThis;                    // Pointer to the UsbController object (this)
    CLR_RT_HeapBlock* configuration;           // Pointer to Configuration class
    CLR_RT_HeapBlock  configurationByteArrayRef; // Storage for native configuration (array reference)
    CLR_RT_HeapBlock_Array* configurationByteArray;    // Pointer to managed array of bytes for native configuration
    CLR_RT_HeapBlock_Array* descriptors;       // Pointer to array of descriptors

    pThis = stack.This(); FAULT_ON_NULL(pThis);

    controllerIndex = pThis[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_UsbController::FIELD__m_controllerIndex ].NumericByRef().s4;

    // Get and check the reference to the configuration class
    configuration = stack.Arg1().Dereference();
    if( configuration == NULL )       // If no configuration passed (null reference), use default
    {
        // Use the default configuration (from the Flash configuration sector)
        retValue = USB_Configure( controllerIndex, NULL );
        stack.SetResult_I4( retValue );
        TINYCLR_SET_AND_LEAVE(S_OK);
    }

    // Allocate space for the native Configuration
    descriptors = configuration[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration::FIELD__descriptors ].DereferenceArray(); FAULT_ON_NULL(descriptors);
    TINYCLR_CHECK_HRESULT(AllocateConfigurationBytes( descriptors, &configurationByteArrayRef ));
    configurationByteArray = configurationByteArrayRef.DereferenceArray(); FAULT_ON_NULL(configurationByteArray);
    nativeConfiguration    = configurationByteArray->GetFirstElement(); FAULT_ON_NULL(nativeConfiguration);
    configSize             = configurationByteArray->m_numOfElements;

    // Fill in the native configuration from the descriptor array
    nDescriptors = descriptors->m_numOfElements;
    nativePtr    = nativeConfiguration;
    for( descriptorIndex = 0; descriptorIndex < nDescriptors; descriptorIndex++ )
    {
        CLR_RT_HeapBlock* descriptorRef;
        CLR_RT_HeapBlock* descriptor;

        descriptorRef = (CLR_RT_HeapBlock*)descriptors->GetElement(descriptorIndex); FAULT_ON_NULL( descriptorRef );
        descriptor    = descriptorRef->Dereference(); FAULT_ON_NULL(descriptor);
        
        // If this is a DeviceDescriptor
        if( descriptor->DataType() == DATATYPE_CLASS && descriptor->ObjectCls().m_data == g_CLR_RT_WellKnownTypes.m_UsbClientConfiguration__DeviceDescriptor.m_data)
        {
            USB_DEVICE_DESCRIPTOR* nativeDevice = (USB_DEVICE_DESCRIPTOR*)nativePtr;

            nativeDevice->header.marker   = USB_DEVICE_DESCRIPTOR_MARKER;
            nativeDevice->header.iValue   = 0;
            nativeDevice->header.size     = sizeof(USB_DEVICE_DESCRIPTOR);
            nativeDevice->bLength         = sizeof(USB_DEVICE_DESCRIPTOR) - sizeof(USB_DESCRIPTOR_HEADER);
            nativeDevice->bDescriptorType = USB_DEVICE_DESCRIPTOR_TYPE;
            nativeDevice->bcdUSB          = descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__DeviceDescriptor::FIELD__bcdUSB ]         .NumericByRef().u2;
            nativeDevice->bDeviceClass    = descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__DeviceDescriptor::FIELD__bDeviceClass ]   .NumericByRef().u1;
            nativeDevice->bDeviceSubClass = descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__DeviceDescriptor::FIELD__bDeviceSubClass ].NumericByRef().u1;
            nativeDevice->bDeviceProtocol = descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__DeviceDescriptor::FIELD__bDeviceProtocol ].NumericByRef().u1;
            nativeDevice->bMaxPacketSize0 = descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__DeviceDescriptor::FIELD__bMaxPacketSize0 ].NumericByRef().u1;
            nativeDevice->idVendor        = descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__DeviceDescriptor::FIELD__idVendor ]       .NumericByRef().u2;
            nativeDevice->idProduct       = descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__DeviceDescriptor::FIELD__idProduct ]      .NumericByRef().u2;
            nativeDevice->bcdDevice       = descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__DeviceDescriptor::FIELD__bcdDevice ]      .NumericByRef().u2;
            nativeDevice->iManufacturer   = descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__DeviceDescriptor::FIELD__iManufacturer ]  .NumericByRef().u1;
            nativeDevice->iProduct        = descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__DeviceDescriptor::FIELD__iProduct ]       .NumericByRef().u1;
            nativeDevice->iSerialNumber   = descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__DeviceDescriptor::FIELD__iSerialNumber ]  .NumericByRef().u1;
            nativeDevice->bNumConfigurations = 1;

            // Advance native pointer to next descriptor
            nativePtr += sizeof(USB_DEVICE_DESCRIPTOR);
        }
        // If this is a ConfigurationDescriptor
        else if( descriptor->DataType() == DATATYPE_CLASS && descriptor->ObjectCls().m_data == g_CLR_RT_WellKnownTypes.m_UsbClientConfiguration__ConfigurationDescriptor.m_data)
        {
            // Marshalling the configuration descriptor is a bit messy to do in line
            TINYCLR_CHECK_HRESULT(MarhalConfigDescriptor( descriptor, nativePtr ));
        }
        // If this is a StringDescriptor
        else if( descriptor->DataType() == DATATYPE_CLASS && descriptor->ObjectCls().m_data == g_CLR_RT_WellKnownTypes.m_UsbClientConfiguration__StringDescriptor.m_data)
        {
            CLR_UINT32                    stringLength;     // Number of characters in string
            USB_STRING_CHAR*              usbString;        // Pointer to native string destination
            LPCSTR                        string;           // Pointer to managed string storage
            USB_STRING_DESCRIPTOR_HEADER* nativeString;     // Pointer to where native string descriptor will go
            CLR_RT_HeapBlock_String*      sString;          // Pointer to managed code string
            CLR_UINT16                    tempString[ USB_STRING_DESCRIPTOR_MAX_LENGTH ];     // Word aligned storage for uh
            CLR_RT_UnicodeHelper          uh;               // Helper to convert UTF8 to UTF16

            nativeString = (USB_STRING_DESCRIPTOR_HEADER*)nativePtr;
            sString = descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__StringDescriptor::FIELD__sString ].DereferenceString(); FAULT_ON_NULL(sString);
            string = sString->StringText(); FAULT_ON_NULL(string);        // Get pointer to actual zero-terminated string
            usbString = (USB_STRING_CHAR*)&nativeString[ 1 ];

            // Fill in the native String descriptor data
            nativeString->header.marker   = USB_STRING_DESCRIPTOR_MARKER;
            nativeString->header.iValue   = descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__Descriptor::FIELD__index ].NumericByRef().u1;
            nativeString->bDescriptorType = USB_STRING_DESCRIPTOR_TYPE;

            uh.SetInputUTF8(string);
            stringLength = uh.CountNumberOfCharacters(USB_STRING_DESCRIPTOR_MAX_LENGTH);
            uh.m_outputUTF16 = tempString;
            uh.m_outputUTF16_size = stringLength;
            uh.ConvertFromUTF8( stringLength, false, -1 );

            // Fill in size parameters
            nativeString->header.size = sizeof(USB_STRING_DESCRIPTOR_HEADER) + (stringLength * sizeof(USB_STRING_CHAR));
            nativeString->bLength     = (sizeof(USB_STRING_DESCRIPTOR_HEADER) - sizeof(USB_DESCRIPTOR_HEADER)) + (stringLength * sizeof(USB_STRING_CHAR));
            nativePtr += nativeString->header.size;

            // Copy string from temporary storage to native string descriptor
            for(CLR_UINT32 i = 0; i < stringLength; i++)
            {
                usbString[ i ] = (USB_STRING_CHAR)tempString[ i ];      // This will convert word aligned to byte aligned storage
            }
        }
        // If this is a GenericDescriptor
        else if( descriptor->DataType() == DATATYPE_CLASS && descriptor->ObjectCls().m_data == g_CLR_RT_WellKnownTypes.m_UsbClientConfiguration__GenericDescriptor.m_data)
        {
            CLR_UINT32 index;                                  // General loop index
            CLR_UINT8* nativePayload;                          // Pointer to where native payload will be
            CLR_UINT8* payloadData;                            // Pointer to managed payload actual data
            CLR_RT_HeapBlock_Array* payload;                   // Pointer to managed payload array
            USB_GENERIC_DESCRIPTOR_HEADER* nativeGeneric;      // Pointer to native Generic Descriptor

            nativeGeneric = (USB_GENERIC_DESCRIPTOR_HEADER*)nativePtr;
            payload       = descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__GenericDescriptor::FIELD__payload ].DereferenceArray(); FAULT_ON_NULL(payload);

            // Fill in the fields
            nativeGeneric->header.marker = USB_GENERIC_DESCRIPTOR_MARKER;
            nativeGeneric->header.iValue = 0;
            nativeGeneric->header.size   = sizeof(USB_GENERIC_DESCRIPTOR_HEADER) + payload->m_numOfElements;
            nativeGeneric->bmRequestType = descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__GenericDescriptor::FIELD__bmRequestType ].NumericByRef().u1;
            nativeGeneric->bRequest      = descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__GenericDescriptor::FIELD__bRequest ]     .NumericByRef().u1;
            nativeGeneric->wValue        = descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__GenericDescriptor::FIELD__wValue ]       .NumericByRef().u2;
            nativeGeneric->wIndex        = descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__GenericDescriptor::FIELD__wIndex ]       .NumericByRef().u2;

            // Copy managed payload to native descriptor
            nativePayload = (CLR_UINT8*)&nativeGeneric[ 1 ];
            payloadData   = payload->GetFirstElement();
            for( index = 0; index < payload->m_numOfElements; index++ )
                *nativePayload++ = *payloadData++;
            
            nativePtr += nativeGeneric->header.size;
        }
        // If the object is of an unknown type
        else
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER)
        }
    }

    {
        USB_DESCRIPTOR_HEADER* last;       // Pointer to where native descriptor list terminator will go
        
        // Terminate the list of descriptors
        last         = (USB_DESCRIPTOR_HEADER*)nativePtr;
        last->marker = USB_END_DESCRIPTOR_MARKER;
        last->iValue = 0;
        last->size   = 0;
    }

    // Actually set the native configuration for the requested USB controller
    retValue = USB_Configure( controllerIndex, (const USB_DYNAMIC_CONFIGURATION*)nativeConfiguration );

    // Always put the returned error number in UsbController.m_configError
    pThis[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_UsbController::FIELD__m_configError ].SetInteger(retValue);
    if( retValue != USB_CONFIG_ERR_OK )
    {
        // If there was a problem setting the configuration, pull an exception
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER)
    }

    TINYCLR_NOCLEANUP();
}


// Parsing out the configuration descriptor is somewhat messy, so it is put in a separate routine
// to make things appear prettier than they really are.
// descriptorReference is a poitner to a reference to the ConfigurationDescriptor object
HRESULT UnmarshalConfigDescriptor( CLR_RT_HeapBlock* descriptorReference, const USB_CONFIGURATION_DESCRIPTOR* nativeConfiguration )
{
    TINYCLR_HEADER();

    CLR_UINT32 nInterfaces;                            // Holds the number of interfaces
    CLR_UINT32 interfaceIndex;                         // Index into managed array of interfaces
    CLR_INT32  endpointIndex = 0;                      // Index into managed array of endpoints
    CLR_INT32  classIndex = 0;                         // Index into managed array of class descriptors
    CLR_UINT16 totalLength;                            // Holds the total length of the native configuration descriptor
    CLR_UINT16 length;                                 // Running length as compared to totalLength
    USB_CLASS_DESCRIPTOR_HEADER* nativeDescriptor;     // Points to the current native descriptor
    CLR_RT_HeapBlock* descriptor;                      // Pointer to managed descriptor
    CLR_RT_HeapBlock* interfaceArrayRef;               // Pointer to reference to array of managed descriptors
    CLR_RT_HeapBlock* usbInterface = NULL;             // Pointer to current managed interface descriptor
    CLR_RT_HeapBlock_Array* interfaces;                // Pointer to managaed array of interfaces
    CLR_RT_HeapBlock_Array* endpoints = NULL;          // Pointer to reference to array of endpoints
    CLR_RT_HeapBlock_Array* classes   = NULL;          // Pointer to reference to array of class descriptors

    // In order to search through the native config descriptor, its length must be known
    totalLength = nativeConfiguration->wTotalLength - nativeConfiguration->bLength;
    
    // First, count the number of interfaces
    length           = 0;
    nInterfaces      = 0;
    nativeDescriptor = (USB_CLASS_DESCRIPTOR_HEADER*)&nativeConfiguration[ 1 ];
    while( length < totalLength )
    {
        if( nativeDescriptor->bDescriptorType == USB_INTERFACE_DESCRIPTOR_TYPE )
        {
            nInterfaces++;
        }

        if( nativeDescriptor->bLength == 0 ) TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);

        length += nativeDescriptor->bLength;

        nativeDescriptor  = (USB_CLASS_DESCRIPTOR_HEADER*)&(((CLR_UINT8*)nativeDescriptor)[ nativeDescriptor->bLength ]);
    }

    FAULT_ON_NULL( descriptorReference );
    descriptor = descriptorReference->Dereference(); FAULT_ON_NULL(descriptor);

    // Create the array of UsbInterfaces
    interfaceArrayRef = &(descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__ConfigurationDescriptor::FIELD__interfaces ]);
    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance( *interfaceArrayRef, nInterfaces, g_CLR_RT_WellKnownTypes.m_UsbClientConfiguration__UsbInterface ));
    interfaces = interfaceArrayRef->DereferenceArray(); FAULT_ON_NULL(interfaces);

    // Fill in the information for all UsbInterfaces
    nativeDescriptor = (USB_CLASS_DESCRIPTOR_HEADER*)&nativeConfiguration[ 1 ];        // Back to top of Configuration list
    if( nativeDescriptor->bDescriptorType != USB_INTERFACE_DESCRIPTOR_TYPE )
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL)       // The Configuration has been checked, so this should never happen
    length         = 0;
    interfaceIndex = 0;
    while( length < totalLength )
    {
        CLR_RT_HeapBlock* usbInterfaceRef;      // Pointer to reference to managed interface descriptor
        CLR_RT_HeapBlock* endpointArrayRef;     // Pointer to reference to managed array of endpoints
        CLR_RT_HeapBlock* classArrayRef = NULL; // Pointer to reference to managed array of class descriptors

        if( nativeDescriptor->bDescriptorType == USB_INTERFACE_DESCRIPTOR_TYPE )
        {
            CLR_INT32  nEndpoints;                             // Number of endpoints for the current interface descriptor
            CLR_INT32  nClasses;                               // Number of class descriptors for the current interface descriptor
            CLR_UINT16 subLength;                              // Number of bytes from end if interface descriptor to end of entire configuration descriptor
            USB_INTERFACE_DESCRIPTOR*    nativeInterface;      // Pointer to the current native interface descriptor
            USB_CLASS_DESCRIPTOR_HEADER* subDescriptor;        // Pointer to native descriptor associated with a single interface descriptor

            nativeInterface = (USB_INTERFACE_DESCRIPTOR*)nativeDescriptor;

            // usbInterface is a pointer to an interface reference
            usbInterfaceRef = (CLR_RT_HeapBlock*)interfaces->GetElement( interfaceIndex++ );
            TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex( *usbInterfaceRef, g_CLR_RT_WellKnownTypes.m_UsbClientConfiguration__UsbInterface ));
            usbInterface = usbInterfaceRef->Dereference(); FAULT_ON_NULL(usbInterface);

            usbInterface[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__UsbInterface::FIELD__bInterfaceNumber   ].SetInteger(nativeInterface->bInterfaceNumber);
            usbInterface[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__UsbInterface::FIELD__bInterfaceClass    ].SetInteger(nativeInterface->bInterfaceClass);
            usbInterface[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__UsbInterface::FIELD__bInterfaceSubClass ].SetInteger(nativeInterface->bInterfaceSubClass);
            usbInterface[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__UsbInterface::FIELD__bInterfaceProtocol ].SetInteger(nativeInterface->bInterfaceProtocol);
            usbInterface[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__UsbInterface::FIELD__iInterface         ].SetInteger(nativeInterface->iInterface);

            // The number of endpoints for the interface must be counted
            nEndpoints    = 0;
            nClasses      = 0;
            subDescriptor = (USB_CLASS_DESCRIPTOR_HEADER*)&(((CLR_UINT8*)nativeDescriptor)[ nativeDescriptor->bLength ]);     // Start with descriptor after interface descriptor
            subLength     = length + nativeDescriptor->bLength;
            while( subLength < totalLength )
            {
                if( subDescriptor->bDescriptorType == USB_ENDPOINT_DESCRIPTOR_TYPE )
                    nEndpoints++;
                else if( subDescriptor->bDescriptorType == USB_INTERFACE_DESCRIPTOR_TYPE )
                    break;
                else
                    nClasses++;
                
                subLength     += subDescriptor->bLength;
                subDescriptor  = (USB_CLASS_DESCRIPTOR_HEADER*)&(((CLR_UINT8*)subDescriptor)[ subDescriptor->bLength ]);
            }

            // Allocate array for the endpoints - endpoints is a pointer to a reference to an array
            endpointArrayRef = &(usbInterface[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__UsbInterface::FIELD__endpoints ]);
            TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance( *endpointArrayRef, nEndpoints, g_CLR_RT_WellKnownTypes.m_UsbClientConfiguration__Endpoint ));
            endpoints = endpointArrayRef->DereferenceArray();
            endpointIndex = 0;

            if(nClasses > 0)
            {
                classArrayRef = &(usbInterface[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__UsbInterface::FIELD__classDescriptors ]);
                TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance( *classArrayRef, nClasses, g_CLR_RT_WellKnownTypes.m_UsbClientConfiguration__ClassDescriptor ));
                classes = classArrayRef->DereferenceArray();
            }
            else
            {
                classes = NULL;
            }
            classIndex = 0;
        }
        else if (nativeDescriptor->bDescriptorType == USB_ENDPOINT_DESCRIPTOR_TYPE )
        {
            USB_ENDPOINT_DESCRIPTOR* nativeEndpoint;       // Pointer to native endpoint
            CLR_RT_HeapBlock*        endpointRef;          // Pointer to reference to endpoint
            CLR_RT_HeapBlock*        endpoint;             // Pointer to endpoint reference

            nativeEndpoint = (USB_ENDPOINT_DESCRIPTOR*)nativeDescriptor;
            FAULT_ON_NULL(endpoints);
            endpointRef = (CLR_RT_HeapBlock*)endpoints->GetElement( endpointIndex++ ); FAULT_ON_NULL(endpointRef);
            TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex( *endpointRef, g_CLR_RT_WellKnownTypes.m_UsbClientConfiguration__Endpoint ));
            endpoint = endpointRef->Dereference(); FAULT_ON_NULL(endpoint);
            endpoint[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__Endpoint::FIELD__bEndpointAddress ].SetInteger(nativeEndpoint->bEndpointAddress & 0x7F);
            endpoint[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__Endpoint::FIELD__bmAttributes     ].SetInteger(nativeEndpoint->bmAttributes | (nativeEndpoint->bEndpointAddress & 0x80));
            endpoint[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__Endpoint::FIELD__wMaxPacketSize   ].SetInteger(nativeEndpoint->wMaxPacketSize);
            endpoint[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__Endpoint::FIELD__bInterval        ].SetInteger(nativeEndpoint->bInterval);
        }
        else        // Assume it to be some kind of interface class descriptor
        {
            // Calculate the payload size
            CLR_UINT32        size;                  // Number of bytes in payload (native or managed)
            CLR_RT_HeapBlock* classReference;        // Pointer to Interface class reference
            CLR_RT_HeapBlock* interfaceClass;        // Pointer to Interface class managed object
            CLR_RT_HeapBlock* payloadReference;      // Pointer to payload array reference
            CLR_UINT8*        payloadData;           // Pointer to managed array data
            CLR_UINT8*        nativePayload;         // Pointer to native payload
            CLR_RT_HeapBlock_Array* payload;         // Pointer to managed array of payload bytes

            // Get size of native payload
            size = nativeDescriptor->bLength - sizeof(USB_CLASS_DESCRIPTOR_HEADER);
            
            // Create the new class object
            FAULT_ON_NULL(classes);
            classReference   = (CLR_RT_HeapBlock*)classes->GetElement( classIndex++ ); FAULT_ON_NULL(classReference);
            TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex( *classReference, g_CLR_RT_WellKnownTypes.m_UsbClientConfiguration__ClassDescriptor ));
            interfaceClass   = classReference->Dereference(); FAULT_ON_NULL(interfaceClass);
            payloadReference = &(interfaceClass[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__ClassDescriptor::FIELD__payload ]);
            TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance( *payloadReference, size, g_CLR_RT_WellKnownTypes.m_UInt8 ));
            payload          = payloadReference->DereferenceArray(); FAULT_ON_NULL(payload);

            // Copy the native interface class descriptor values to the managed class
            interfaceClass[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__ClassDescriptor::FIELD__bDescriptorType ].SetInteger(nativeDescriptor->bDescriptorType);
            payloadData   = payload->GetFirstElement(); FAULT_ON_NULL(payloadData);
            nativePayload = (CLR_UINT8*)&nativeDescriptor[ 1 ];
            memcpy( payloadData, nativePayload, size );
        }

        // Get next descriptor
        length += nativeDescriptor->bLength;
        nativeDescriptor = (USB_CLASS_DESCRIPTOR_HEADER*)&(((CLR_UINT8*)nativeDescriptor)[ nativeDescriptor->bLength ]);
    }

    TINYCLR_NOCLEANUP();
}

HRESULT AllocateConfigurationBytes( CLR_RT_HeapBlock_Array* descriptors, CLR_RT_HeapBlock* configurationByteArrayRef )
{
    TINYCLR_HEADER();

    CLR_UINT32 nBytes;                          // Total number of bytes required by the native configuration
    CLR_INT32  nDescriptors;                    // Number of descriptors in configuration descriptor array
    CLR_INT32  descriptorIndex;                 // Index into managed array of descriptors

    FAULT_ON_NULL(descriptors);                 // Descriptor array must not be empty
    FAULT_ON_NULL(configurationByteArrayRef);   // Pointer to array reference must not be bad

    nBytes = 0;
    nDescriptors = descriptors->m_numOfElements;
    for( descriptorIndex = 0; descriptorIndex < nDescriptors; descriptorIndex++ )
    {
        CLR_RT_HeapBlock_Array* payload;       // Pointer to managed array of bytes that make up interface class payload or generic descriptor payload
        CLR_RT_HeapBlock* descriptorRef;       // Pointer to reference to current managed descriptor
        CLR_RT_HeapBlock* descriptor;          // Pointer to current managed descriptor

        descriptorRef = (CLR_RT_HeapBlock*)descriptors->GetElement( descriptorIndex ); FAULT_ON_NULL(descriptorRef);
        descriptor = descriptorRef->Dereference(); FAULT_ON_NULL(descriptor);

        // If this is a DeviceDescriptor
        if( descriptor->DataType() == DATATYPE_CLASS && descriptor->ObjectCls().m_data == g_CLR_RT_WellKnownTypes.m_UsbClientConfiguration__DeviceDescriptor.m_data)
        {
            nBytes += sizeof(USB_DEVICE_DESCRIPTOR);
        }
        // If this is a ConfigurationDescriptor
        else if( descriptor->DataType() == DATATYPE_CLASS && descriptor->ObjectCls().m_data == g_CLR_RT_WellKnownTypes.m_UsbClientConfiguration__ConfigurationDescriptor.m_data)
        {
            CLR_INT32  nInterfaces;                 // Number of interfaces in the configuration descriptor
            CLR_INT32  interfaceIndex;              // Index into managed array of interfaces
            CLR_RT_HeapBlock_Array* interfaces;     // Pointer to managed array of interfaces for a configuration descriptor

            nBytes += sizeof(USB_CONFIGURATION_DESCRIPTOR);
            interfaces = descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__ConfigurationDescriptor::FIELD__interfaces ].DereferenceArray(); FAULT_ON_NULL(interfaces);        // Interface array must not be empty
            nInterfaces = interfaces->m_numOfElements;
            for( interfaceIndex = 0; interfaceIndex < nInterfaces; interfaceIndex++ )
            {
                CLR_RT_HeapBlock* usbInterfaceRef;     // Pointer to reference to current managed interface
                CLR_RT_HeapBlock* usbInterface;        // Pointer to current managed interface
                CLR_RT_HeapBlock_Array* classes;       // Pointer to current interface class descriptors
                CLR_RT_HeapBlock_Array* endpoints;     // Pointer to managed array of endpoints for the current interface
                CLR_UINT32 nClasses;

                usbInterfaceRef = (CLR_RT_HeapBlock*)interfaces->GetElement(interfaceIndex); FAULT_ON_NULL(usbInterfaceRef);
                usbInterface    = usbInterfaceRef->Dereference(); FAULT_ON_NULL(usbInterface);

                nBytes += sizeof(USB_INTERFACE_DESCRIPTOR);
                
                classes = usbInterface[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__UsbInterface::FIELD__classDescriptors ].DereferenceArray(); 

                if(classes != NULL)
                {
                    for(nClasses = 0; nClasses < classes->m_numOfElements; nClasses++)
                    {
                        CLR_RT_HeapBlock* classDescriptor = (CLR_RT_HeapBlock*)classes->GetElement(nClasses); FAULT_ON_NULL(classDescriptor);
                        classDescriptor                   = classDescriptor->Dereference();                   FAULT_ON_NULL(classDescriptor);
                        
                        payload = classDescriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__ClassDescriptor::FIELD__payload ].DereferenceArray(); FAULT_ON_NULL(payload);      // Array must exist - even if it is empty
                        nBytes += sizeof(USB_CLASS_DESCRIPTOR_HEADER) + payload->m_numOfElements;
                    }
                }
                endpoints = usbInterface[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__UsbInterface::FIELD__endpoints ].DereferenceArray(); FAULT_ON_NULL(endpoints);          // There must be endpoints allocated to every interface
                nBytes   += (sizeof(USB_ENDPOINT_DESCRIPTOR) * endpoints->m_numOfElements);
            }
        }
        // If this is a StringDescriptor
        else if(descriptor->DataType() == DATATYPE_CLASS && descriptor->ObjectCls().m_data == g_CLR_RT_WellKnownTypes.m_UsbClientConfiguration__StringDescriptor.m_data)
        {
            int                      stringLength;  // Actual length of string in characters
            CLR_RT_HeapBlock_String* sString;       // Pointer to managed string object in string descriptor
            LPCSTR                   string;        // Pointer to managed string storage
            CLR_RT_UnicodeHelper     uh;

            sString = descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__StringDescriptor::FIELD__sString ].DereferenceString(); FAULT_ON_NULL(sString);                 // A string must be allocated for the string descriptor
            string = sString->StringText(); FAULT_ON_NULL(string);
            uh.SetInputUTF8( string );
            stringLength = uh.CountNumberOfCharacters(USB_STRING_DESCRIPTOR_MAX_LENGTH);
            if( stringLength < 0 )
            {
                TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER)
            }
            nBytes += sizeof(USB_STRING_DESCRIPTOR_HEADER);
            nBytes += sizeof(USB_STRING_CHAR) * stringLength;
        }
        // If this is a GenericDescriptor
        else if( descriptor->DataType() == DATATYPE_CLASS && descriptor->ObjectCls().m_data == g_CLR_RT_WellKnownTypes.m_UsbClientConfiguration__GenericDescriptor.m_data)
        {
            payload = descriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__GenericDescriptor::FIELD__payload ].DereferenceArray(); FAULT_ON_NULL(payload);       // The Generic Descriptor must have a payload - even if it contains zero bytes
            nBytes += sizeof(USB_GENERIC_DESCRIPTOR_HEADER) + payload->m_numOfElements;
        }
        // If the object is of an unknown type
        else
        {
            // Descriptor type is illegal
            TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER)
        }
    }

    if( nBytes == 0 )       // If configuration was completely empty
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER)
    }
    nBytes += sizeof(USB_DESCRIPTOR_HEADER);        // Add room for the list terminator

    // Create a byte array of the appropriate size
    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance( *configurationByteArrayRef, nBytes, g_CLR_RT_WellKnownTypes.m_UInt8 ))

    TINYCLR_NOCLEANUP();
}

HRESULT MarhalConfigDescriptor( CLR_RT_HeapBlock* configurationDescriptor, CLR_UINT8* &nativePtr )
{
    TINYCLR_HEADER();

    CLR_INT32   nInterfaces;                           // Number of interfaces in configuration descriptor
    CLR_INT32   interfaceIndex;                        // Index into managed array of interfaces
    USB_CONFIGURATION_DESCRIPTOR* nativeConfig;        // Pointer to where native configuration will be written
    CLR_RT_HeapBlock_Array* interfaces;                // Pointer to managed array of interfaces in configuration descriptor
    
    nativeConfig = (USB_CONFIGURATION_DESCRIPTOR*)nativePtr;
    FAULT_ON_NULL(configurationDescriptor);
    
    //Fill in all configuration descriptor members that we can (before knowing how many interfaces, etc)
    nativeConfig->header.marker   = USB_CONFIGURATION_DESCRIPTOR_MARKER;
    nativeConfig->header.iValue   = 0;
    nativeConfig->bLength         = sizeof(USB_CONFIGURATION_DESCRIPTOR) - sizeof(USB_DESCRIPTOR_HEADER);
    nativeConfig->bDescriptorType = USB_CONFIGURATION_DESCRIPTOR_TYPE;
    nativeConfig->bConfigurationValue = 1;
    nativeConfig->iConfiguration  = configurationDescriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__ConfigurationDescriptor::FIELD__iConfiguration ].NumericByRef().u1;
    nativeConfig->bmAttributes    = configurationDescriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__ConfigurationDescriptor::FIELD__bmAttributes   ].NumericByRef().u1;
    nativeConfig->bMaxPower       = configurationDescriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__ConfigurationDescriptor::FIELD__bMaxPower      ].NumericByRef().u1;
    
    interfaces = configurationDescriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__ConfigurationDescriptor::FIELD__interfaces ].DereferenceArray(); FAULT_ON_NULL(interfaces);
    nInterfaces = interfaces->m_numOfElements;
    nativeConfig->bNumInterfaces  = nInterfaces;
    nativePtr = (CLR_UINT8*)&nativeConfig[ 1 ];
    for( interfaceIndex = 0; interfaceIndex < nInterfaces; interfaceIndex++ )
    {
        CLR_UINT32 endpointIndex = 0;                  // Index into managed array of endpoints
        CLR_UINT32 classIndex = 0;                     // Index into managed array of class descriptors
        USB_INTERFACE_DESCRIPTOR* nativeInterface;     // Pointer to where native interface will be written
        CLR_RT_HeapBlock*         usbInterface;        // Pointer to current managed interface
        CLR_RT_HeapBlock*         usbInterfaceRef;     // Pointer to reference to managed interface
        CLR_RT_HeapBlock_Array*   classes;             // Pointer to current managed interface class descriptor array
        CLR_RT_HeapBlock_Array*   endpoints;           // Pointer to managed array of endpoints in the current interface

        usbInterfaceRef = (CLR_RT_HeapBlock*)interfaces->GetElement(interfaceIndex); FAULT_ON_NULL(usbInterfaceRef);
        usbInterface    = usbInterfaceRef->Dereference(); FAULT_ON_NULL(usbInterface);
        nativeInterface = (USB_INTERFACE_DESCRIPTOR*)nativePtr;

        // Fill in all fields of interface descriptor possible at this time
        nativeInterface->bLength            = sizeof(USB_INTERFACE_DESCRIPTOR);
        nativeInterface->bDescriptorType    = USB_INTERFACE_DESCRIPTOR_TYPE;
        nativeInterface->bInterfaceNumber   = usbInterface[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__UsbInterface::FIELD__bInterfaceNumber ]  .NumericByRef().u1;
        nativeInterface->bAlternateSetting  = 0;
        nativeInterface->bInterfaceClass    = usbInterface[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__UsbInterface::FIELD__bInterfaceClass ]   .NumericByRef().u1;
        nativeInterface->bInterfaceSubClass = usbInterface[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__UsbInterface::FIELD__bInterfaceSubClass ].NumericByRef().u1;
        nativeInterface->bInterfaceProtocol = usbInterface[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__UsbInterface::FIELD__bInterfaceProtocol ].NumericByRef().u1;
        nativeInterface->iInterface         = usbInterface[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__UsbInterface::FIELD__iInterface ]        .NumericByRef().u1;

        // Check for and handle the interface class descriptor if it exists
        nativePtr       = (CLR_UINT8*)&nativeInterface[ 1 ];
        classes = usbInterface[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__UsbInterface::FIELD__classDescriptors ].DereferenceArray();
        if(classes != NULL)
        {
            for( classIndex = 0; classIndex < classes->m_numOfElements; classIndex++ )
            {
                CLR_RT_HeapBlock*            classDescriptor;      // Pointer to current managed interface class descriptor (if it exists)
                CLR_UINT32                   index;                // General-purpose index
                CLR_UINT8*                   nativePayload;        // Pointer to where payload for native interface class will be written
                CLR_UINT8*                   payloadData;          // Pointer to data in managed code payload array for interface class descriptor
                USB_CLASS_DESCRIPTOR_HEADER* nativeClass;          // Pointer to where native interface class will be written
                CLR_RT_HeapBlock_Array*      payload;              // Pointer to array of bytes that make up (possible) interface class descriptor payload

                classDescriptor = (CLR_RT_HeapBlock*)classes->GetElement(classIndex); FAULT_ON_NULL(classDescriptor);
                classDescriptor = classDescriptor->Dereference(); FAULT_ON_NULL(classDescriptor);

                payload       = classDescriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__ClassDescriptor::FIELD__payload ].DereferenceArray(); FAULT_ON_NULL(payload);       // If interface class descriptor exists, so must its payload
                nativeClass   = (USB_CLASS_DESCRIPTOR_HEADER*)nativePtr;
                nativePayload = (CLR_UINT8*)&nativeClass[ 1 ];
                payloadData   = payload->GetFirstElement(); FAULT_ON_NULL(payloadData);

                // Fill in interface class members
                nativeClass->bLength         = sizeof(USB_CLASS_DESCRIPTOR_HEADER) + payload->m_numOfElements;
                nativeClass->bDescriptorType = classDescriptor[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__ClassDescriptor::FIELD__bDescriptorType ].NumericByRef().u1;
                for( index = 0; index < payload->m_numOfElements; index++ )
                    *nativePayload++ = *payloadData++;
                nativePtr = nativePayload;      // Move pointer to end of payload
            }
        }

        // Now handle the endpoints
        endpoints = usbInterface[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__UsbInterface::FIELD__endpoints ].DereferenceArray(); FAULT_ON_NULL(endpoints);
        nativeInterface->bNumEndpoints = endpoints->m_numOfElements;
        for( endpointIndex = 0; endpointIndex < endpoints->m_numOfElements; endpointIndex++ )
        {
            USB_ENDPOINT_DESCRIPTOR* nativeEndpoint;      // Pointer to where native endpoint will be written
            CLR_RT_HeapBlock*        endpointRef;         // Pointer to reference to managed endpoint
            CLR_RT_HeapBlock*        endpoint;            // Pointer to current managed endpoint
    
            endpointRef    = (CLR_RT_HeapBlock*)endpoints->GetElement(endpointIndex); FAULT_ON_NULL( endpointRef );
            endpoint       = endpointRef->Dereference(); FAULT_ON_NULL(endpoint);
            nativeEndpoint = (USB_ENDPOINT_DESCRIPTOR*)nativePtr;

            // Fill in the endpoint members
            nativeEndpoint->bLength          = sizeof(USB_ENDPOINT_DESCRIPTOR);
            nativeEndpoint->bDescriptorType  = USB_ENDPOINT_DESCRIPTOR_TYPE;
            nativeEndpoint->bEndpointAddress = endpoint[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__Endpoint::FIELD__bEndpointAddress ].NumericByRef().u1;
            nativeEndpoint->bmAttributes     = endpoint[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__Endpoint::FIELD__bmAttributes     ].NumericByRef().u1;
            nativeEndpoint->wMaxPacketSize   = endpoint[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__Endpoint::FIELD__wMaxPacketSize   ].NumericByRef().u2;
            nativeEndpoint->bInterval        = endpoint[ Library_spot_hardware_usb_native_Microsoft_SPOT_Hardware_UsbClient_Configuration__Endpoint::FIELD__bInterval        ].NumericByRef().u1;

            // Move IN endpoint attribute - this looks goofy, but it keeps the attributes together in managed code land
            nativeEndpoint->bEndpointAddress |= nativeEndpoint->bmAttributes & USB_ENDPOINT_DIRECTION_IN;
            nativeEndpoint->bmAttributes     &= ~USB_ENDPOINT_DIRECTION_IN;
            nativePtr                         = (CLR_UINT8*)&nativeEndpoint[ 1 ];        // Move pointer to end of endpoint
        }
    }
    // Now we know the size of the complete configuration descriptor
    nativeConfig->header.size  = nativePtr - (CLR_UINT8*)nativeConfig;
    nativeConfig->bLength      = sizeof(USB_CONFIGURATION_DESCRIPTOR) - sizeof(USB_DESCRIPTOR_HEADER);
    nativeConfig->wTotalLength = nativeConfig->header.size            - sizeof(USB_DESCRIPTOR_HEADER);

    TINYCLR_NOCLEANUP();
}


