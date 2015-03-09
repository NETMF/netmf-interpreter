////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Hardware.UsbClient;

// This test should work whether the USB controller is already running (the debugger) or not.
// If it is running something besides a TinyBooter compatible USB configuration, or
// if the default configuration (as set up in usb_config.cpp) is not TinyBooter compatible
// or if there is a USB configuration in the Flash configuration sector and it is not a
// TinyBooter compatible configuration, then this test will not work.
// Tests ONLY the first controller - if it exists.

namespace Microsoft.SPOT.Platform.Tests
{

    public class UsbDefaultConfigTest : IMFTestInterface
    {
        DefaultUSB defaultConfig = null;
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("UsbDefaultConfig test Initialize");
            try
            {
                defaultConfig = new DefaultUSB();
            }

            catch (System.NotSupportedException)
            {
                if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)  //The Emulator on Windows Host
                {
                    Log.Comment("UsbClient not supported UnSupported Exception expected");
                    return InitializeResult.Skip;
                }
            }
            catch (Exception E)
            {
                Log.Exception("UsbClient Unexpected usbDevices enumeration failed" + E.Message);
                return InitializeResult.Skip;
            }
            //
            // usbDevices enumerated with something, now check known issue
            //
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
            {
                Log.Comment("UsbClient does not return expected NotSupprotedException on Windows - bug 20936\n");
                return InitializeResult.Skip;
            }

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("UsbDefaultConfig test CleanUp");
            // Not sure what needs to be here...
        }

        [TestMethod]
        public MFTestResults TestDefaultConfiguration()
        {
            if (defaultConfig.controllers.Length == 0)
            {
                //
                // Note: For platforms without USB controllers
                //
                Log.Comment("Device Platform has 0 USB Controllers");
                return MFTestResults.Skip;
            }

            if (defaultConfig.controllers.Length > 1)
            {
                Log.Comment("The target actually has " + defaultConfig.controllers.Length.ToString() + " controllers. This test only applies to controller 0");
            }
            if (defaultConfig.controllers[0].Status == UsbController.PortState.Stopped)        // If configuration may not already be set
            {
                try
                {
                    defaultConfig.controllers[0].Configuration = null;        // Set USB configuration to default
                }
                catch (Exception e)
                {
                    Log.Exception("Setting the default configuration caused an exception (" + e.ToString() + "). The configuration failed with error " + defaultConfig.controllers[0].ConfigurationError.ToString());
                    return MFTestResults.Fail;
                }
            }

            DefaultUSB.config = defaultConfig.controllers[0].Configuration;
            MFTestResults result;
            DefaultUSB defaultTests = new DefaultUSB();

            result = defaultTests.CheckDescriptors();
            if (result == MFTestResults.Pass)     // If all tests pass
            {
                Log.Comment("USB Default Configuration passed all tests");
            }
            return result;
        }
    }

    public class DefaultUSB
    {
        int manufacturerIndex;
        int productIndex;
        bool descriptorFound;
        bool displayNameFound;
        bool friendlyNameFound;
        bool manufacturerNameFound;
        bool productNameFound;
        bool sideshowOsStringFound;
        bool sideshowXCompatIdFound;
        public UsbController[] controllers;
        public static Configuration config = null;

        public DefaultUSB()
        {
            controllers = UsbController.GetControllers();
        }

        public MFTestResults CheckDescriptors()
        {
            MFTestResults result = MFTestResults.Pass;

            manufacturerIndex = 0;
            productIndex = 0;
            descriptorFound = false;
            displayNameFound = false;
            friendlyNameFound = false;
            manufacturerNameFound = false;
            productNameFound = false;
            sideshowOsStringFound = false;
            sideshowXCompatIdFound = false;

            if (config == null)      // If the default configuration was totally bogus
            {
                if (UsbController.GetControllers().Length == 0)        // If there were no controllers
                {
                    return MFTestResults.Skip;                          // The test should not be run
                }
                return MFTestResults.Fail;
            }

            // These tests must be done in this order to work properly
            if (!DeviceDescriptor())
            {
                result = MFTestResults.Fail;
            }
            if (!ConfigurationDescriptor())
            {
                result = MFTestResults.Fail;
            }
            if (!StringDescriptors())
            {
                result = MFTestResults.Fail;
            }
            if (!GenericDescriptors())
            {
                result = MFTestResults.Fail;
            }

            return result;
        }

        private bool DeviceDescriptor()
        {
            bool result = true;

            // Find and check the Device Descriptor
            descriptorFound = false;
            foreach (Configuration.Descriptor descriptor in config.descriptors)
            {
                if (descriptor.GetType() == typeof(Configuration.DeviceDescriptor))
                {
                    Configuration.DeviceDescriptor device = (Configuration.DeviceDescriptor)descriptor;

                    if (descriptorFound)
                    {
                        Log.Exception("More than one Device descriptor found in default configuration");
                        return false;
                    }
                    if (device.bDeviceClass != 0 || device.bDeviceSubClass != 0 || device.bDeviceProtocol != 0)
                    {
                        Log.Exception("Device class of default configuration is not set to 'no class' (0)");
                        result = false;
                    }
                    if (device.bcdUSB != 0x0200)
                    {
                        Log.Exception("USB version of default class is " + Microsoft.SPOT.Platform.Test.MFUtilities.UintToHex(device.bcdUSB) + " instead of 0x0200");
                        result = false;
                    }
                    Log.Comment("Default configuration has Vid = " + Microsoft.SPOT.Platform.Test.MFUtilities.UintToHex(device.idVendor) + " and Pid of " + Microsoft.SPOT.Platform.Test.MFUtilities.UintToHex(device.idProduct));

                    manufacturerIndex = device.iManufacturer;
                    productIndex = device.iProduct;
                    if (device.iManufacturer != 1 || device.iProduct != 2)
                    {
                        if (device.iManufacturer == 0)
                        {
                            Log.Comment("Note that the default configuration has no manufacturer name");
                        }
                        else if (device.iManufacturer == 4 || device.iManufacturer == 5)
                        {
                            Log.Exception("Default configuration has illegal string number (4 or 5) for Device Manufacturer name");
                            result = false;
                        }
                        if (device.iProduct == 0)
                        {
                            Log.Comment("Note that the default configuration has no product name");
                        }
                        else if (device.iProduct == 4 || device.iProduct == 5)
                        {
                            Log.Exception("Default configuration has illegal string number (4 or 5) for Device Product name");
                            result = false;
                        }
                    }
                    if (device.iSerialNumber == 4 || device.iSerialNumber == 5)
                    {
                        Log.Exception("Default configuration has illegal string number (4 or 5) for its serial number");
                        result = false;
                    }
                    descriptorFound = true;
                }
            }
            if (!descriptorFound)
            {
                Log.Exception("No Device descriptor found in default configuration");
                result = false;
            }

            return result;
        }

        private bool ConfigurationDescriptor()
        {
            bool result = true;

            // Locate and check the Configuration descriptor
            descriptorFound = false;
            foreach (Configuration.Descriptor descriptor in config.descriptors)
            {
                if (descriptor.GetType() == typeof(Configuration.ConfigurationDescriptor))
                {
                    Configuration.ConfigurationDescriptor configuration = (Configuration.ConfigurationDescriptor)descriptor;

                    if (descriptorFound)
                    {
                        Log.Exception("More than one Configuration descriptor found in default configuration");
                        return false;
                    }
                    if (configuration.iConfiguration == 4 || configuration.iConfiguration == 5)
                    {
                        Log.Exception("Configuration descriptor string number for default configuration is illegal (4 or 5)");
                        result = false;
                    }
                    if ((configuration.bmAttributes & Configuration.ConfigurationDescriptor.ATTRIB_Base) == 0)
                    {
                        Log.Exception("Configuration descriptor for default configuration does not have the base attribute bit set");
                        result = false;
                    }
                    if (configuration.bMaxPower == 0)
                    {
                        Log.Exception("Configuration descriptor for default configuration has no power requirement specified");
                        result = false;
                    }
                    if (configuration.interfaces == null || configuration.interfaces.Length == 0)
                    {
                        Log.Exception("No Interfaces found for default configuration");
                        return false;
                    }
                    if (configuration.interfaces.Length != 1)
                    {
                        Log.Comment("Default configuration has more than 1 (there are " + configuration.interfaces.Length.ToString() + ") interface...");
                    }
                    bool interfaceFound = false;
                    foreach (Configuration.UsbInterface usbInterface in configuration.interfaces)
                    {
                        // The debugger uses interface #0 only
                        if (usbInterface.bInterfaceNumber == 0)
                        {
                            if (interfaceFound)
                            {
                                Log.Exception("Default configuration contained more than one Interface #0");
                                return false;
                            }
                            if (usbInterface.bInterfaceClass != 0xFF || usbInterface.bInterfaceSubClass != 1 || usbInterface.bInterfaceProtocol != 1)
                            {
                                Log.Exception("Interface Class, subclass or protocol incorrect for Interface 0 of default configuration");
                                result = false;
                            }
                            if (usbInterface.classDescriptors != null)
                            {
                                Log.Exception("Interface 0 for the default configuration should not have a class descriptor");
                                result = false;
                            }
                            if (usbInterface.endpoints == null || usbInterface.endpoints.Length != 2)
                            {
                                Log.Exception("Interface 0 for default configuration must have exactly two endpoints (it has " + usbInterface.endpoints.Length.ToString() + ")");
                                return false;
                            }
                            if (usbInterface.iInterface == 4 || usbInterface.iInterface == 5)
                            {
                                Log.Exception("Interface 0 has illegal string number (4 or 5) for default configuration");
                                result = false;
                            }

                            // Make sure the endpoints are OK
                            bool readEndpointFound = false;
                            bool writeEndpointFound = false;
                            foreach (Configuration.Endpoint endpoint in usbInterface.endpoints)
                            {
                                if ((endpoint.bEndpointAddress & 0x7F) == 1)
                                {
                                    if ((endpoint.bmAttributes & Configuration.Endpoint.ATTRIB_Write) != Configuration.Endpoint.ATTRIB_Write)
                                    {
                                        Log.Exception("Endpoint #1 of Interface 0 in default configuration must be a Write endpoint");
                                        result = false;
                                    }
                                    if ((endpoint.bmAttributes & 0x7F) != Configuration.Endpoint.ATTRIB_Bulk)
                                    {
                                        Log.Exception("Endpoint #1 of Interface 0 in default configuration must be a Bulk endpoint");
                                        result = false;
                                    }
                                    writeEndpointFound = true;
                                }
                                else if ((endpoint.bEndpointAddress & 0x7F) == 2)
                                {
                                    if ((endpoint.bmAttributes & Configuration.Endpoint.ATTRIB_Write) == Configuration.Endpoint.ATTRIB_Write)
                                    {
                                        Log.Exception("Endpoint #2 of Interface 0 in default configuration must be a Read endpoint");
                                        result = false;
                                    }
                                    if ((endpoint.bmAttributes & 0x7F) != Configuration.Endpoint.ATTRIB_Bulk)
                                    {
                                        Log.Exception("Endpoint #2 of Interface 0 in default configuration must be a Bulk endpoint");
                                        result = false;
                                    }
                                    readEndpointFound = true;
                                }
                                else
                                {
                                    Log.Exception("There should be no Endpoint #" + Microsoft.SPOT.Platform.Test.MFUtilities.ByteToHex((byte)(endpoint.bEndpointAddress & 0x7F)) + " in Interface 0 of the default configuration");
                                    result = false;
                                }
                            }
                            if (!readEndpointFound || !writeEndpointFound)
                            {
                                Log.Exception("Read or Write endpoint for Interface 0 of default configuration is missing");
                                result = false;
                            }
                            interfaceFound = true;
                        }
                        else
                        {
                            Log.Comment("Extra interface #" + usbInterface.bInterfaceNumber.ToString() + " found in default configuration - is this supposed to be here?");
                            if (usbInterface.iInterface == 4 || usbInterface.iInterface == 5)
                            {
                                Log.Exception("Interface #" + usbInterface.bInterfaceNumber.ToString() + "of default configuration has illegal string number (4 or 5)");
                                result = false;
                            }
                        }
                    }
                    if (!interfaceFound)
                    {
                        Log.Exception("Default configuration contained no interface #0 - this must be present");
                        result = false;
                    }

                    descriptorFound = true;
                }
            }
            if (!descriptorFound)
            {
                Log.Exception("No Configuration descriptor found in default configuration");
                result = false;
            }

            return result;
        }

        private bool StringDescriptors()
        {
            bool result = true;

            // Check the String descriptors
            foreach (Configuration.Descriptor descriptor in config.descriptors)
            {
                if (descriptor.GetType() == typeof(Configuration.StringDescriptor))
                {
                    Configuration.StringDescriptor stringDescriptor = (Configuration.StringDescriptor)descriptor;
                    if (stringDescriptor.bIndex == 4)
                    {
                        if (displayNameFound)
                        {
                            Log.Exception("There was more than one display name string (4) defined for the default configuration");
                            result = false;
                        }
                        Log.Comment("Display name  = " + stringDescriptor.sString);
                        displayNameFound = true;
                    }
                    if (stringDescriptor.bIndex == 5)
                    {
                        if (friendlyNameFound)
                        {
                            Log.Exception("There was more than one friendly name string (5) defined for the default configuration");
                            result = false;
                        }
                        Log.Comment("Friendly name = " + stringDescriptor.sString);
                        friendlyNameFound = true;
                    }
                    if (stringDescriptor.bIndex == 0xEE)        // If OS String descriptor found
                    {
                        if (stringDescriptor.sString != "MSFT100\xA5")
                        {
                            Log.Exception("Default configuration OS String descriptor is '" + stringDescriptor.sString + "' rather than 'MSFT100\xA5'");
                            result = false;
                        }
                        Log.Comment("Sideshow OS String descriptor for default configuration found and implemented using a standard String descriptor");
                        sideshowOsStringFound = true;
                    }
                    if (stringDescriptor.sString.Length > 30)
                    {
                        Log.Exception("Default configuration string #" + Microsoft.SPOT.Platform.Test.MFUtilities.ByteToHex(stringDescriptor.bIndex) + " is longer than 30 characters");
                        result = false;
                    }
                    if (stringDescriptor.bIndex == manufacturerIndex)
                    {
                        manufacturerNameFound = true;
                        Log.Comment("Manufacturer  = " + stringDescriptor.sString);
                    }
                    if (stringDescriptor.bIndex == productIndex)
                    {
                        productNameFound = true;
                        Log.Comment("Product       = " + stringDescriptor.sString);
                    }
                }
            }
            if (manufacturerIndex != 0 && !manufacturerNameFound)
            {
                Log.Exception("The manufacturer name string is missing");
                result = false;
            }
            if (productIndex != 0 && !productNameFound)
            {
                Log.Exception("The product name string is missing");
                result = false;
            }
            if (!displayNameFound || !friendlyNameFound)
            {
                Log.Exception("The default configuration must contain both a string 4 and string 5 descriptor");
                result = false;
            }

            return result;
        }

        private bool GenericDescriptors()
        {
            bool result = true;

            foreach (Configuration.Descriptor descriptor in config.descriptors)
            {
                if (descriptor.GetType() == typeof(Configuration.GenericDescriptor))
                {
                    Configuration.GenericDescriptor generic = (Configuration.GenericDescriptor)descriptor;

                    // If Sideshow OS String descriptor
                    if (generic.bmRequestType == Configuration.GenericDescriptor.REQUEST_IN && generic.bRequest == 6 && generic.wValue == 0x03EE)
                    {
                        byte[] payload = new byte[] { 0x12, 0x03, 0x4D, 0x00, 0x53, 0x00, 0x46, 0x00, 0x54, 0x00, 0x31, 0x00, 0x30, 0x00, 0x30, 0x00, 0xA5, 0x00 };

                        if (sideshowOsStringFound)     // If already implemented as a string
                        {
                            Log.Exception("Sideshow OS String descriptor apparently implemented as both a string and a generic in default configuration");
                            result = false;
                        }
                        if (generic.wIndex != 0)
                        {
                            Log.Exception("Sideshow OS String descriptor wIndex value in default configuration is not zero");
                            result = false;
                        }
                        if (payload.Length != generic.payload.Length)
                        {
                            Log.Exception("Sideshow OS String descriptor payload length in default configuration is " + generic.payload.Length.ToString() + "rather than " + payload.Length.ToString());
                            result = false;
                        }
                        for (int i = 0; i < payload.Length; i++)
                        {
                            if (payload[i] != generic.payload[i])
                            {
                                Log.Exception("Byte " + i.ToString() + " of Sideshow OS String descriptor in default configuration is " + Microsoft.SPOT.Platform.Test.MFUtilities.ByteToHex(generic.payload[i]) + " rather than " + Microsoft.SPOT.Platform.Test.MFUtilities.ByteToHex(payload[i]));
                                result = false;
                                break;
                            }
                        }
                        Log.Comment("Sideshow OS String descriptor for default configuration found and implemented using a Generic descriptor");
                        sideshowOsStringFound = true;
                    }
                    // If Sideshow Extended Compatible OS ID descriptor
                    else if (generic.bRequest == 0xA5)
                    {
                        byte[] payload = new byte[]
                        {
                            0x28, 0x00, 0x00, 0x00,                             // Length = 40
                            0x00, 0x01,                                         // Version = 1.00
                            0x04, 0x00,                                         // wIndex = 4
                            0x01,                                               // bCount = 1 property section
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,           // padding
                            0x00,                                               // Interface #0
                            0x01,                                               // Reserved (must be set to 1)
                            0x53, 0x49, 0x44, 0x45, 0x53, 0x48, 0x57, 0x00,     // SIDESHW
                            0x45, 0x4E, 0x48, 0x56, 0x31, 0x00, 0x00, 0x00,     // ENHV1
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00                  // Padding
                        };
                        if (generic.bmRequestType != (Configuration.GenericDescriptor.REQUEST_IN | Configuration.GenericDescriptor.REQUEST_Vendor))
                        {
                            Log.Exception("bmRequest field of Extended Compatible OS ID descriptor is " + Microsoft.SPOT.Platform.Test.MFUtilities.ByteToHex(generic.bmRequestType) +
                                " rather than " + Microsoft.SPOT.Platform.Test.MFUtilities.ByteToHex(Configuration.GenericDescriptor.REQUEST_IN | Configuration.GenericDescriptor.REQUEST_Vendor));
                            result = false;
                        }
                        if (generic.wValue != 0)
                        {
                            Log.Exception("wValue field of Extended Compatible OS ID descriptor is " + Microsoft.SPOT.Platform.Test.MFUtilities.UintToHex(generic.wValue) + " rather than 0");
                            result = false;
                        }
                        if (generic.wIndex != 4)
                        {
                            Log.Exception("wIndex field of Extended Compatible OS ID descriptor is " + Microsoft.SPOT.Platform.Test.MFUtilities.UintToHex(generic.wIndex) + " rather than 0x0004");
                            result = false;
                        }
                        if (payload.Length != generic.payload.Length)
                        {
                            Log.Exception("Sideshow Extended Compatible OS ID descriptor payload in default configuration is " + generic.payload.Length.ToString() +
                                " rather than " + payload.Length.ToString());
                            result = false;
                        }
                        for (int i = 0; i < payload.Length; i++)
                        {
                            if (payload[i] != generic.payload[i])
                            {
                                Log.Exception("Byte " + i.ToString() + " of Sideshow Extended Compatible OS ID payload in default configuration had " +
                                     Microsoft.SPOT.Platform.Test.MFUtilities.ByteToHex(generic.payload[i]) + " rather than " + Microsoft.SPOT.Platform.Test.MFUtilities.ByteToHex(payload[i]));
                                result = false;
                                break;
                            }
                        }
                        sideshowXCompatIdFound = true;
                    }
                    else
                    {
                        Log.Exception("Unknown generic descriptor found in default configuration with bRequest = " + generic.bRequest.ToString() +
                            " and wValue = " + generic.wValue.ToString() + " and bmRequestType = " + generic.bmRequestType.ToString());
                        result = false;
                    }
                }
            }
            if (!sideshowOsStringFound && sideshowXCompatIdFound)
            {
                Log.Exception("Sideshow Extended Compatible ID descriptor found in default configuration, but not its corresponding OS String descriptor");
                result = false;
            }
            if (sideshowOsStringFound && !sideshowXCompatIdFound)
            {
                Log.Exception("Sideshow OS String descriptor found in default configuration, but not its corresponding Extended Compatible ID descriptor");
                result = false;
            }
            if (!sideshowXCompatIdFound)
            {
                Log.Comment("Sideshow descriptors were not found in the default configuration");
            }
            else
            {
                Log.Comment("Sideshow descriptors were correctly specified in the default configuration");
            }

            return result;
        }
    }
}
