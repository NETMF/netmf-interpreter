//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Microsoft .NET Micro Framework Porting Kit Code Samples and is unsupported. 
// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use these files except in compliance with the License.
// You may obtain a copy of the License at:
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing
// permissions and limitations under the License.
// 
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using System.Collections;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool
{
    public class UsbXMLConfigSet
    {
        private string FileName;
        private XmlTextReader reader;
        private bool countOnly;
        private byte[] data;
        private int dataIndex;
        ArrayList stringList;
        int autoIndex;


        private const byte USB_DISPLAY_STRING_NUM              = 4;
        private const byte USB_FRIENDLY_STRING_NUM             = 5;

        private const byte USB_END_DESCRIPTOR_MARKER           = 0x00;
        private const byte USB_GENERIC_DESCRIPTOR_MARKER       = 0xFF;

        private const byte USB_DESCRIPTOR_HEADER_LENGTH        = 4;
        private const byte HEADER_MARKER_OFFSET                = 0;
        private const byte HEADER_INDEX_OFFSET                 = 1;
        private const byte HEADER_LENGTH_OFFSET                = 2;

        public UsbXMLConfigSet(string fileName)
        {
            stringList = new ArrayList();
            FileName = fileName;
        }

        public byte[] Read()
        {
            FileStream XmlFile;

            try
            {
                XmlFile = File.OpenRead(FileName);
            }
            catch (Exception e)
            {
                MessageBox.Show("XML file could not be opened due to exception: " + e.Message);
                return null;
            }

            reader = new XmlTextReader(XmlFile);
            stringList.Clear();
            autoIndex = 1;

            // First pass read of XML file - this counts the bytes needed for binary
            // representation of the configuration data in XML file.
            countOnly = true;
            dataIndex = 0;

            try
            {
                if (!ParseXmlFile(reader))
                {
                    reader.Close();     // Closes the file
                    return null;
                }
            }
            catch (Exception e)
            {
                reader.Close();
                MessageBox.Show("Exception encountered while trying to parse XML file: " + e.Message);
                return null;
            }

            DumpStrings();      // Count all bytes associated with auto-generated String descriptors

            // Finished with the first pass, reopen everything for the second pass
            if (dataIndex == 0)      // If no valid data was found in the file
            {
                MessageBox.Show("XML file contained no useful information");
                return null;
            }
            dataIndex += USB_DESCRIPTOR_HEADER_LENGTH;      // There must be a terminating header

            try
            {
                XmlFile.Seek(0, SeekOrigin.Begin);
                reader = new XmlTextReader(XmlFile);
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception trying to Seek to beginning of file: " + e.Message);
                return null;
            }
            countOnly = false;
            stringList.Clear();         // Throw out all auto-generated string information (it will be regenerated)
            autoIndex = 1;              // Reset the auto-generated String descriptor index
            data = new byte[dataIndex]; // Allocate space for entire USB configuration now that we know the size
            dataIndex = 0;

            try
            {
                if (!ParseXmlFile(reader))
                {
                    reader.Close();
                    return null;
                }
            }
            catch (Exception e)
            {
                reader.Close();
                MessageBox.Show("Exception encountered during second pass of XML file: " + e.Message);
                return null;
            }

            DumpStrings();              // Add auto-generated String descriptors to the configuration

            // Add the terminating header
            data[dataIndex + HEADER_MARKER_OFFSET    ] = USB_END_DESCRIPTOR_MARKER;
            data[dataIndex + HEADER_INDEX_OFFSET     ] = 0;
            data[dataIndex + HEADER_LENGTH_OFFSET    ] = 0;
            data[dataIndex + HEADER_LENGTH_OFFSET + 1] = 0;

            // All done
            reader.Close();
            return data;
        }

        private bool ParseXmlFile(XmlTextReader reader)
        {
            // Find the interesting part of the file
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "UsbControllerConfiguration")
                {
                    break;
                }
            }

            // Now parse out the different descriptors
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "DeviceDescriptor")
                    {
                        if (!ParseDevice(reader))
                        {
                            return false;
                        }
                        continue;
                    }
                    if (reader.Name == "ConfigurationDescriptor")
                    {
                        if (!ParseConfiguration(reader))
                        {
                            return false;
                        }
                        continue;
                    }
                    if (reader.Name == "StringDescriptor")
                    {
                        if (!ParseString(reader))
                        {
                            return false;
                        }
                        continue;
                    }
                    if (reader.Name == "GenericDescriptor")
                    {
                        if (!ParseGeneric(reader))
                        {
                            return false;
                        }
                        continue;
                    }
                    MessageBox.Show("Found unknown element (" + reader.Name + ") in UsbControllerConfiguration block of XML file");
                    return false;
                }

                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name != "UsbControllerConfiguration")
                    {
                        MessageBox.Show("Found extraneous end element: " + reader.Name + " in XML file");
                        return false;
                    }
                    return true;
                }
            }
            MessageBox.Show("Unexpected end of file while reading UsbControllerConfiguration block of XML file");
            return false;
        }

        private bool ParseDevice(XmlTextReader reader)
        {
            bool stillGood = true;
            ulong value = 0;

            const byte USB_DEVICE_DESCRIPTOR_MARKER        = 0x01;
            const byte USB_DEVICE_DESCRIPTOR_TYPE          = 1;
            const byte USB_DEVICE_BLENGTH_OFFSET           = 0;
            const byte USB_DEVICE_BDESCR_TYPE_OFFSET       = 1;
            const byte USB_DEVICE_BCDUSB_OFFSET            = 2;
            const byte USB_DEVICE_BCLASS_OFFSET            = 4;
            const byte USB_DEVICE_BSUBCLASS_OFFSET         = 5;
            const byte USB_DEVICE_BPROTOCOL_OFFSET         = 6;
            const byte USB_DEVICE_BSIZE0_OFFSET            = 7;
            const byte USB_DEVICE_IDVENDOR_OFFSET          = 8;
            const byte USB_DEVICE_IDPRODUCT_OFFSET         = 10;
            const byte USB_DEVICE_BCD_DEVICE_OFFSET        = 12;
            const byte USB_DEVICE_IMANUFACT_OFFSET         = 14;
            const byte USB_DEVICE_IPRODUCT_OFFSET          = 15;
            const byte USB_DEVICE_ISN_OFFSET               = 16;
            const byte USB_DEVICE_NCONFIG                  = 17;
            const byte USB_DEVICE_DESCRIPTOR_LENGTH        = 18;

            // These are the fields that must be present
            bool foundIdVendor       = false;
            bool foundIdProduct      = false;
            bool foundBcdDevice      = false;
            bool foundMaxPacketSize0 = false;

            // Fill in the Descriptor header
            if (!countOnly)
            {
                data[dataIndex + HEADER_MARKER_OFFSET    ] = USB_DEVICE_DESCRIPTOR_MARKER;
                data[dataIndex + HEADER_INDEX_OFFSET     ] = 0;
                data[dataIndex + HEADER_LENGTH_OFFSET    ] = USB_DESCRIPTOR_HEADER_LENGTH + USB_DEVICE_DESCRIPTOR_LENGTH;
                data[dataIndex + HEADER_LENGTH_OFFSET + 1] = 0;
            }
            dataIndex += USB_DESCRIPTOR_HEADER_LENGTH;

            // Fill in the default values
            if (!countOnly)
            {
                data[dataIndex + USB_DEVICE_BLENGTH_OFFSET    ] = USB_DEVICE_DESCRIPTOR_LENGTH;
                data[dataIndex + USB_DEVICE_BDESCR_TYPE_OFFSET] = USB_DEVICE_DESCRIPTOR_TYPE;
                data[dataIndex + USB_DEVICE_NCONFIG           ] = 1;        // Only 1 configuration is allowed
                data[dataIndex + USB_DEVICE_IMANUFACT_OFFSET  ] = 0;        // iManufacturer defaults to no string
                data[dataIndex + USB_DEVICE_IPRODUCT_OFFSET   ] = 0;        // iProduct defaults to no string
                data[dataIndex + USB_DEVICE_ISN_OFFSET        ] = 0;        // iSerialNumber defaults to no string
                data[dataIndex + USB_DEVICE_BCDUSB_OFFSET     ] = 0x10;
                data[dataIndex + USB_DEVICE_BCDUSB_OFFSET + 1 ] = 0x01;     // bcdUSB defaults to version 1.10
                data[dataIndex + USB_DEVICE_BCLASS_OFFSET     ] = 0;        // bDeviceClass defaults to 0 (no class)
                data[dataIndex + USB_DEVICE_BSUBCLASS_OFFSET  ] = 0;        // bDeviceSubClass defaults to 0 (no subclass)
                data[dataIndex + USB_DEVICE_BPROTOCOL_OFFSET  ] = 0;        // bDeviceProtocol defaults to 0 (no protocol)
            }

            if (reader.AttributeCount > 0)
            {
                MessageBox.Show("No attributes may be specified in the DeviceDescriptor");
                return false;
            }

            while (reader.Read())
            {
                if (!stillGood)
                {
                    return false;
                }
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "idVendor")
                    {
                        stillGood = ParseInteger(reader, out value, 2);
                        if (stillGood)
                        {
                            stillGood = SaveInteger(reader.Name, value, dataIndex + USB_DEVICE_IDVENDOR_OFFSET, 2);
                            foundIdVendor = true;
                        }
                        continue;
                    }
                    if (reader.Name == "idProduct")
                    {
                        stillGood = ParseInteger(reader, out value, 2);
                        if (stillGood)
                        {
                            stillGood = SaveInteger(reader.Name, value, dataIndex + USB_DEVICE_IDPRODUCT_OFFSET, 2);
                            foundIdProduct = true;
                        }
                        continue;
                    }
                    if (reader.Name == "bcdDevice")
                    {
                        stillGood = ParseBcd(reader, USB_DEVICE_BCD_DEVICE_OFFSET);
                        foundBcdDevice = true;
                        continue;
                    }
                    if (reader.Name == "bMaxPacketSize0")
                    {
                        stillGood = ParseInteger(reader, out value, 1);
                        if (stillGood)
                        {
                            stillGood = SaveInteger(reader.Name, value, dataIndex + USB_DEVICE_BSIZE0_OFFSET, 1);
                            foundMaxPacketSize0 = true;
                        }
                        continue;
                    }
                    if (reader.Name == "iManufacturer")
                    {
                        stillGood = ParseAutoString(reader, USB_DEVICE_IMANUFACT_OFFSET);
                        continue;
                    }
                    if (reader.Name == "iProduct")
                    {
                        stillGood = ParseAutoString(reader, USB_DEVICE_IPRODUCT_OFFSET);
                        continue;
                    }
                    if (reader.Name == "iSerialNumber")
                    {
                        stillGood = ParseAutoString(reader, USB_DEVICE_ISN_OFFSET);
                        continue;
                    }
                    if (reader.Name == "bcdUSB")
                    {
                        stillGood = ParseBcd(reader, USB_DEVICE_BCDUSB_OFFSET);
                        continue;
                    }
                    if (reader.Name == "bDeviceClass")
                    {
                        stillGood = ParseInteger(reader, out value, 1);
                        if (stillGood)
                        {
                            stillGood = SaveInteger(reader.Name, value, dataIndex + USB_DEVICE_BCLASS_OFFSET, 1);
                        }
                        continue;
                    }
                    if (reader.Name == "bDeviceSubClass")
                    {
                        stillGood = ParseInteger(reader, out value, 1);
                        if (stillGood)
                        {
                            stillGood = SaveInteger(reader.Name, value, dataIndex + USB_DEVICE_BSUBCLASS_OFFSET, 1);
                        }
                        continue;
                    }
                    if (reader.Name == "bDeviceProtocol")
                    {
                        stillGood = ParseInteger(reader, out value, 1);
                        if (stillGood)
                        {
                            stillGood = SaveInteger(reader.Name, value, dataIndex + USB_DEVICE_BPROTOCOL_OFFSET, 1);
                        }
                        continue;
                    }
                }
                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name != "DeviceDescriptor")
                    {
                        MessageBox.Show("Found extraneous end element: " + reader.Name + " in DeviceDescriptor of XML file");
                        return false;
                    }
                    if (!foundIdVendor)
                    {
                        MessageBox.Show("No idVendor element found in DeviceDescriptor");
                        return false;
                    }
                    if (!foundIdProduct)
                    {
                        MessageBox.Show("No idProduct element found in DeviceDescriptor");
                        return false;
                    }
                    if (!foundBcdDevice)
                    {
                        MessageBox.Show("No bcdDevice element found in DeviceDescriptor");
                        return false;
                    }
                    if (!foundMaxPacketSize0)
                    {
                        MessageBox.Show("No bMaxPacketSize0 element found in DeviceDescriptor");
                        return false;
                    }
                    dataIndex += USB_DEVICE_DESCRIPTOR_LENGTH;
                    return true;
                }
            }
            MessageBox.Show("Unexpected end of file while reading DeviceDescriptor block of XML file");
            return false;
        }

        private bool ParseConfiguration(XmlTextReader reader)
        {
            const byte USB_ATTRIBUTE_BASE                    = 0x80;
            const byte USB_ATTRIBUTE_SELF_POWER              = 0x40;
            const byte USB_ATTRIBUTE_REMOTE_WAKEUP           = 0x20;

            const byte USB_CONFIGURATION_DESCRIPTOR_MARKER   = 0x02;
            const byte USB_CONFIGURATION_DESCRIPTOR_TYPE     = 2;
            const byte USB_CONFIGURATION_LENGTH_OFFSET       = 0;
            const byte USB_CONFIGURATION_TYPE_OFFSET         = 1;
            const byte USB_CONFIGURATION_TOTAL_LENGTH_OFFSET = 2;
            const byte USB_CONFIGURATION_NUM_ITFC_OFFSET     = 4;
            const byte USB_CONFIGURATION_VALUE_OFFSET        = 5;
            const byte USB_CONFIGURATION_ICONFIG_OFFSET      = 6;
            const byte USB_CONFIGURATION_ATTRIB_OFFSET       = 7;
            const byte USB_CONFIGURATION_POWER_OFFSET        = 8;
            const byte USB_CONFIGURATION_DESCRIPTOR_LENGTH   = 9;

            // The Configuration descriptor header must wait until all information has been gathered before it can be written
            // Therefore, the current index will be saved and the pointer advanced past the headers with nothing
            // written until all information is present
            int headerIndex     = dataIndex;                                // Save Configuration header start location
            dataIndex          += USB_DESCRIPTOR_HEADER_LENGTH + USB_CONFIGURATION_DESCRIPTOR_LENGTH;      // Advance data pointer to after configuration header

            byte bmAttributes   = USB_ATTRIBUTE_BASE;                       // Most significant bit of attributes must always be set
            byte bNumInterfaces = 0;                                        // Start with no interfaces
            byte maxPower       = 0;                                        // Start with no maximum power
            byte iConfig        = 0;                                        // Default to no descriptive string for configuration

            if (reader.AttributeCount > 0)
            {
                reader.MoveToFirstAttribute();
                do
                {
                    if (reader.Name == "self_powered")
                    {
                        bmAttributes |= USB_ATTRIBUTE_SELF_POWER;
                    }
                    else if (reader.Name == "remote_wakeup")
                    {
                        bmAttributes |= USB_ATTRIBUTE_REMOTE_WAKEUP;
                    }
                    else
                    {
                        MessageBox.Show("ConfigurationDescriptor element does not recognize attribute '" + reader.Name + "'.");
                        return false;
                    }
                } while (reader.MoveToNextAttribute());
                reader.MoveToElement();
            }

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "SelfPowered")
                    {
                        bmAttributes |= USB_ATTRIBUTE_SELF_POWER;
                        continue;
                    }
                    if (reader.Name == "RemoteWakeup")
                    {
                        bmAttributes |= USB_ATTRIBUTE_REMOTE_WAKEUP;
                        continue;
                    }
                    if (reader.Name == "InterfaceDescriptor")
                    {
                        if (!ParseInterface(reader))
                        {
                            return false;
                        }
                        bNumInterfaces++;
                        continue;
                    }
                    if(reader.Name == "bMaxPower_mA")
                    {
                        ulong value = 0;

                        if (!ParseInteger(reader, out value, 2))
                        {
                            return false;
                        }
                        if (value < 2 || value > 510)
                        {
                            MessageBox.Show("Value for bMaxPower_ma element was out of range (2-510)");
                        }
                        maxPower = (byte)(value / 2);
                        continue;
                    }
                    if(reader.Name == "iConfiguration")
                    {
                        // This will store the string index at the current data index - where it doesn't belong - but that doesn't
                        // matter, since it can be moved from there to where it does belong.
                        if (!ParseAutoString(reader, 0))
                        {
                            return false;
                        }
                        if (!countOnly)
                        {
                            iConfig = data[dataIndex];
                        }
                        continue;
                    }
                    MessageBox.Show("The element name '" + reader.Name + "' was unexpected in a ConfigurationDescriptor element.");
                    return false;
                }
                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name != "ConfigurationDescriptor")
                    {
                        MessageBox.Show("Found extraneous end element: " + reader.Name + " in ConfigurationDescriptor of XML file");
                        return false;
                    }
                    if (maxPower == 0)
                    {
                        MessageBox.Show("There was no bMaxPower_mA element in the ConfigurationDescriptor element");
                        return false;
                    }
                    if (bNumInterfaces == 0)
                    {
                        MessageBox.Show("No InterfaceDescriptor elements were defined for the ConfigurationDescriptor");
                        return false;
                    }

                    // Configuration header information can be written now that all is known
                    if (!countOnly)
                    {
                        // Fill in the search header information
                        data[headerIndex + HEADER_MARKER_OFFSET    ] = USB_CONFIGURATION_DESCRIPTOR_MARKER;
                        data[headerIndex + HEADER_INDEX_OFFSET     ] = 0;
                        data[headerIndex + HEADER_LENGTH_OFFSET    ] = (byte)(dataIndex - headerIndex);
                        data[headerIndex + HEADER_LENGTH_OFFSET + 1] = (byte)((dataIndex - headerIndex) >> 8);

                        // Fill in the Configuration descriptor "header" information
                        headerIndex     += USB_DESCRIPTOR_HEADER_LENGTH;
                        data[headerIndex + USB_CONFIGURATION_LENGTH_OFFSET          ] = USB_CONFIGURATION_DESCRIPTOR_LENGTH;
                        data[headerIndex + USB_CONFIGURATION_TYPE_OFFSET            ] = USB_CONFIGURATION_DESCRIPTOR_TYPE;
                        data[headerIndex + USB_CONFIGURATION_TOTAL_LENGTH_OFFSET    ] = (byte)(dataIndex - headerIndex);
                        data[headerIndex + USB_CONFIGURATION_TOTAL_LENGTH_OFFSET + 1] = (byte)((dataIndex - headerIndex) >> 8);
                        data[headerIndex + USB_CONFIGURATION_NUM_ITFC_OFFSET        ] = bNumInterfaces;
                        data[headerIndex + USB_CONFIGURATION_VALUE_OFFSET           ] = 1;
                        data[headerIndex + USB_CONFIGURATION_ICONFIG_OFFSET         ] = iConfig;
                        data[headerIndex + USB_CONFIGURATION_ATTRIB_OFFSET          ] = bmAttributes;
                        data[headerIndex + USB_CONFIGURATION_POWER_OFFSET           ] = maxPower;
                    }
                    return true;
                }
            }
            MessageBox.Show("Unexpected end of file while reading ConfigurationDescriptor block of XML file");
            return false;
        }

        private bool ParseInterface(XmlTextReader reader)
        {
            const byte USB_INTERFACE_DESCRIPTOR_TYPE   = 4;
            const byte USB_INTERFACE_LENGTH_OFFSET     = 0;
            const byte USB_INTERFACE_TYPE_OFFSET       = 1;
            const byte USB_INTERFACE_ID_OFFSET         = 2;
            const byte USB_INTERFACE_ALTERNATE_OFFSET  = 3;
            const byte USB_INTERFACE_ENDPOINTS_OFFSET  = 4;
            const byte USB_INTERFACE_CLASS_OFFSET      = 5;
            const byte USB_INTERFACE_SUBCLASS_OFFSET   = 6;
            const byte USB_INTERFACE_PROTOCOL_OFFSET   = 7;
            const byte USB_INTERFACE_IITFC_OFFSET      = 8;
            const byte USB_INTERFACE_DESCRIPTOR_LENGTH = 9;

            // Save start of Interface descriptor so that its information may be updated after
            // the Class and Endpoint descriptors have been written
            int headerIndex = dataIndex;
            ulong value = 0;
            dataIndex += USB_INTERFACE_DESCRIPTOR_LENGTH;

            byte id = 0;                // Interface ID number must be supplied
            byte iInterface = 0;        // Interface descriptor string defaults to none
            byte bClass = 0xFF;        // Interface class defaults to "Vendor" type
            byte bSubClass = 0;         // Interface subclass defaults to "none"
            byte bProtocol = 0;         // Interface protocol defaults to "none"
            byte nEndpoints = 0;        // Start with no endpoints

            bool foundId = false;

            bool stillGood = true;

            if (reader.AttributeCount > 0)
            {
                reader.MoveToFirstAttribute();
                do
                {
                    if (reader.Name != "id")
                    {
                        MessageBox.Show("Unrecognized attribute (" + reader.Name + ") found in InterfaceDescriptor");
                        return false;
                    }
                    foundId = true;
                    if (ConvertInteger(reader.Value, 0, out value) > 0)
                    {
                        id = (byte)value;
                    }
                    else
                    {
                        return false;
                    }
                } while (reader.MoveToNextAttribute());
                reader.MoveToElement();
            }

            while (reader.Read())
            {
                if (!stillGood)
                {
                    return false;
                }
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "iInterface")
                    {
                        // This places the string index at the wrong location, but it's OK because it doesn't
                        // interfere with subsequent data and it can be moved to the right place
                        if (!ParseAutoString(reader, 0))
                        {
                            return false;
                        }
                        if (!countOnly)
                        {
                            iInterface = data[dataIndex];
                        }
                        continue;
                    }
                    if (reader.Name == "bInterfaceClass")
                    {
                        stillGood = ParseInteger(reader, out value, 1);
                        bClass = (byte)value;
                        continue;
                    }
                    if (reader.Name == "bInterfaceSubClass")
                    {
                        stillGood = ParseInteger(reader, out value, 1);
                        bSubClass = (byte)value;
                        continue;
                    }
                    if (reader.Name == "bInterfaceProtocol")
                    {
                        stillGood = ParseInteger(reader, out value, 1);
                        bProtocol = (byte)value;
                        continue;
                    }
                    if (reader.Name == "ClassDescriptor")
                    {
                        if (nEndpoints != 0)
                        {
                            MessageBox.Show("The ClassDescriptor must appear before any EndpointDescriptor in an InterfaceDescriptor");
                            return false;
                        }
                        stillGood = ParseClass(reader);
                        continue;
                    }
                    if (reader.Name == "EndpointDescriptor")
                    {
                        stillGood = ParseEndpoint(reader);
                        nEndpoints++;
                        continue;
                    }
                    MessageBox.Show("Unexpected element (" + reader.Name + ") found in InterfaceDescriptor");
                    return false;
                }
                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name != "InterfaceDescriptor")
                    {
                        MessageBox.Show("Found extraneous end element: " + reader.Name + " in InterfaceDescriptor of XML file");
                        return false;
                    }
                    if (nEndpoints == 0)
                    {
                        MessageBox.Show("InterfaceDescriptor has no EndpointDescriptor");
                        return false;
                    }
                    if (!foundId)
                    {
                        MessageBox.Show("InterfaceDescriptor has no id attribute");
                        return false;
                    }

                    // Build InterfaceDescriptor from gathered information
                    if (!countOnly)
                    {
                        data[headerIndex + USB_INTERFACE_LENGTH_OFFSET   ] = USB_INTERFACE_DESCRIPTOR_LENGTH;
                        data[headerIndex + USB_INTERFACE_TYPE_OFFSET     ] = USB_INTERFACE_DESCRIPTOR_TYPE;
                        data[headerIndex + USB_INTERFACE_ID_OFFSET       ] = id;
                        data[headerIndex + USB_INTERFACE_ALTERNATE_OFFSET] = 0;
                        data[headerIndex + USB_INTERFACE_ENDPOINTS_OFFSET] = nEndpoints;
                        data[headerIndex + USB_INTERFACE_CLASS_OFFSET    ] = bClass;
                        data[headerIndex + USB_INTERFACE_SUBCLASS_OFFSET ] = bSubClass;
                        data[headerIndex + USB_INTERFACE_PROTOCOL_OFFSET ] = bProtocol;
                        data[headerIndex + USB_INTERFACE_IITFC_OFFSET    ] = iInterface;
                    }
                    return true;
                }
            }
            MessageBox.Show("Unexpected end of file while reading InterfaceDescriptor block of XML file");
            return false;
        }

        private bool ParseClass(XmlTextReader reader)
        {
            const byte USB_CLASS_LENGTH_OFFSET            = 0;
            const byte USB_CLASS_TYPE_OFFSET              = 1;
            const byte USB_CLASS_DESCRIPTOR_HEADER_LENGTH = 2;

            int headerIndex = dataIndex;                            // Remember position of class header
            ulong value = 0;
            dataIndex += USB_CLASS_DESCRIPTOR_HEADER_LENGTH;        // Advance data pointer to end of header

            bool foundType = false;

            bool stillGood = true;
            while (reader.Read())
            {
                if (!stillGood)
                {
                    return false;
                }
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "bDescriptorType")
                    {
                        if(!ParseInteger(reader, out value, 1))
                        {
                            return false;
                        }
                        if (!countOnly)
                        {
                            data[headerIndex + USB_CLASS_TYPE_OFFSET] = (byte)value;        // Move Type to class header
                        }
                        foundType = true;
                        continue;
                    }
                    if (reader.Name == "bPadding")
                    {
                        stillGood = ParseInteger(reader, out value, 1);
                        int bPadding = (byte)value;       // Read how much zero padding to add
                        if (!countOnly && stillGood)
                        {
                            for (int i = 0; i < bPadding; i++)
                            {
                                data[dataIndex + i] = 0;
                            }
                        }
                        dataIndex += bPadding;
                        continue;
                    }
                    if (reader.Name == "bData")
                    {
                        stillGood = ParseIntegers(reader, 1);
                        continue;
                    }
                    if (reader.Name == "wData")
                    {
                        stillGood = ParseIntegers(reader, 2);
                        continue;
                    }
                    if (reader.Name == "dwData")
                    {
                        stillGood = ParseIntegers(reader, 4);
                        continue;
                    }
                    if (reader.Name == "iData")
                    {
                        stillGood = ParseAutoString(reader, 0);
                        dataIndex++;
                        continue;
                    }
                    if (reader.Name == "sData")
                    {
                        string text = reader.ReadInnerXml();
                        if (countOnly)
                        {
                            dataIndex += text.Length;
                        }
                        else
                        {
                            for (int i = 0; i < text.Length; i++)
                            {
                                data[dataIndex++] = (byte)text[i];
                            }
                        }
                        continue;
                    }
                    if (reader.Name == "wsData")
                    {
                        string text = reader.ReadInnerXml();
                        if (countOnly)
                        {
                            dataIndex += 2 * text.Length;
                        }
                        else
                        {
                            for (int i = 0; i < text.Length; i++)
                            {
                                data[dataIndex++] = (byte)text[i];
                                data[dataIndex++] = 0;
                            }
                        }
                        continue;
                    }
                    MessageBox.Show("Unexpected element (" + reader.Name + ") found in ClassDescriptor element.");
                    return false;
                }
                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name != "ClassDescriptor")
                    {
                        MessageBox.Show("Found extraneous end element: " + reader.Name + " in ClassDescriptor of XML file");
                        return false;
                    }
                    if (!foundType)
                    {
                        MessageBox.Show("ClassDescriptor element requires a bDescriptorType element");
                        return false;
                    }
                    int length = dataIndex - headerIndex;
                    if (length < 3 || length > byte.MaxValue)
                    {
                        MessageBox.Show("ClassDescriptor payload length is either too long or non-existent");
                        return false;
                    }
                    if (!countOnly)
                    {
                        data[headerIndex + USB_CLASS_LENGTH_OFFSET] = (byte)length;
                    }
                    return true;
                }
            }
            MessageBox.Show("Unexpected end of file while reading ClassDescriptor block of XML file");
            return false;
        }

        private bool ParseEndpoint(XmlTextReader reader)
        {
            const byte USB_ENDPOINT_DESCRIPTOR_TYPE       = 5;
            const byte USB_ENDPOINT_ATTRIBUTE_ISOCHRONOUS = 1;
            const byte USB_ENDPOINT_ATTRIBUTE_BULK        = 2;
            const byte USB_ENDPOINT_ATTRIBUTE_INTERRUPT   = 3;
            const byte USB_ENDPOINT_ATTRIBUTE_ASYNCH      = 0x04;
            const byte USB_ENDPOINT_ATTRIBUTE_ADAPTIVE    = 0x08;
            const byte USB_ENDPOINT_ATTRIBUTE_SYNCH       = 0x0C;
            const byte USB_ENDPOINT_ATTRIBUTE_FEEDBACK    = 0x10;
            const byte USB_ENDPOINT_ATTRIBUTE_IMPLICIT    = 0x20;

            const byte USB_ENDPOINT_LENGTH_OFFSET         = 0;
            const byte USB_ENDPOINT_TYPE_OFFSET           = 1;
            const byte USB_ENDPOINT_ADDRESS_OFFSET        = 2;
            const byte USB_ENDPOINT_ATTRIBUTE_OFFSET      = 3;
            const byte USB_ENDPOINT_SIZE_OFFSET           = 4;
            const byte USB_ENDPOINT_INTERVAL_OFFSET       = 6;
            const byte USB_ENDPOINT_DESCRIPTOR_LENGTH     = 7;

            bool foundDirection = false;
            bool foundMaxSize = false;

            byte address    = 0;        // Clear all address values
            byte attributes = 0;        // Clear all attributes

            // Fill in defaults for EndpointDescriptor
            if(!countOnly)
            {
                data[dataIndex + USB_ENDPOINT_LENGTH_OFFSET  ] = USB_ENDPOINT_DESCRIPTOR_LENGTH;
                data[dataIndex + USB_ENDPOINT_TYPE_OFFSET    ] = USB_ENDPOINT_DESCRIPTOR_TYPE;
                data[dataIndex + USB_ENDPOINT_INTERVAL_OFFSET] = 0;                                 // Interval defaults to zero
            }

            bool stillGood = true;

            // Read in any attributes
            if (reader.AttributeCount > 0)
            {
                reader.MoveToFirstAttribute();
                do
                {
                    ulong value;

                    if (reader.Name == "id")
                    {
                        if((ConvertInteger(reader.Value, 0, out value) <= 0) || value > 0x7F || value == 0)
                        {
                            MessageBox.Show("Bad EndpointDescriptor id attribute value (must be less than 0x7F & non-zero.");
                            return false;
                        }
                        address = (byte)((byte)(address & 0x80) | value);
                        continue;
                    }
                    if (reader.Name == "direction")
                    {
                        if (reader.Value == "in")
                        {
                            address |= 0x80;
                            foundDirection = true;
                            continue;
                        }
                        if (reader.Value == "out")
                        {
                            foundDirection = true;
                            continue;
                        }
                        MessageBox.Show("EndpointDescriptor direction attribute has bad value (" + reader.Value + ").");
                        return false;
                    }
                    if (reader.Name == "transfer")
                    {
                        if (reader.Value == "bulk")
                        {
                            attributes = USB_ENDPOINT_ATTRIBUTE_BULK;
                            continue;
                        }
                        if (reader.Value == "interrupt")
                        {
                            attributes = USB_ENDPOINT_ATTRIBUTE_INTERRUPT;
                            continue;
                        }
                        if (reader.Value == "isochronous")
                        {
                            attributes = USB_ENDPOINT_ATTRIBUTE_ISOCHRONOUS;
                            continue;
                        }
                        MessageBox.Show("EndpointDescriptor transfer attribute has bad value (" + reader.Value + ").");
                        return false;
                    }
                    if (reader.Name == "usage")
                    {
                        if (reader.Value == "data")
                        {
                            continue;       // This is the default case (0)
                        }
                        if (reader.Value == "implicit")
                        {
                            attributes |= USB_ENDPOINT_ATTRIBUTE_IMPLICIT;
                            continue;
                        }
                        if (reader.Value == "feedback")
                        {
                            attributes |= USB_ENDPOINT_ATTRIBUTE_FEEDBACK;
                            continue;
                        }
                        MessageBox.Show("EndpointDescriptor usage attribute has bad value (" + reader.Value + ").");
                        return false;
                    }
                    if (reader.Name == "synchronization")
                    {
                        if (reader.Value == "none")
                        {
                            continue;
                        }
                        if (reader.Value == "asynchronous")
                        {
                            attributes |= USB_ENDPOINT_ATTRIBUTE_ASYNCH;
                            continue;
                        }
                        if (reader.Value == "adaptive")
                        {
                            attributes |= USB_ENDPOINT_ATTRIBUTE_ADAPTIVE;
                            continue;
                        }
                        if (reader.Value == "synchronous")
                        {
                            attributes |= USB_ENDPOINT_ATTRIBUTE_SYNCH;
                            continue;
                        }
                        MessageBox.Show("EndpointDescriptor synchronization attribute has bad value (" + reader.Value + ").");
                        return false;
                    }
                    MessageBox.Show("Unexpected attribute (" + reader.Name + ") found in EndpointDescriptor element.");
                    return false;
                } while (reader.MoveToNextAttribute());
                reader.MoveToElement();
            }

            if ((address & 0x7F) == 0)       // If no address set (Endpoint zero is not legal)
            {
                MessageBox.Show("EndpointDescriptor contains no id or zero id attribute");
                return false;
            }
            if (!foundDirection)
            {
                MessageBox.Show("EndpointDescriptor " + (address & 0x7F).ToString() + " has no direction attribute specified");
                return false;
            }
            if ((attributes & 0x03) == 0)
            {
                MessageBox.Show("EndpointDescriptor " + (address & 0x7F).ToString() + " has no transfer attribute specified");
                return false;
            }

            while (reader.Read())
            {
                ulong value = 0;

                if (!stillGood)
                {
                    return false;
                }
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "wMaxPacketSize")
                    {
                        stillGood = ParseInteger(reader, out value, 2);
                        if (stillGood)
                        {
                            stillGood = SaveInteger(reader.Name, value, dataIndex + USB_ENDPOINT_SIZE_OFFSET, 2);
                            foundMaxSize = true;
                        }
                        continue;
                    }
                    if (reader.Name == "bInterval")
                    {
                        stillGood = ParseInteger(reader, out value, 1);
                        if (stillGood)
                        {
                            stillGood = SaveInteger(reader.Name, value, dataIndex + USB_ENDPOINT_INTERVAL_OFFSET, 1);
                        }
                        continue;
                    }
                    MessageBox.Show("Unexpected element (" + reader.Name + ") found in EndpointDescriptor element.");
                    return false;
                }
                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name != "EndpointDescriptor")
                    {
                        MessageBox.Show("Found extraneous end element: " + reader.Name + " in EndpointDescriptor of XML file");
                        return false;
                    }
                    if(!foundMaxSize)
                    {
                        MessageBox.Show("EndpointDescriptor " + (address & 0x7F).ToString() + " has no maximum size specified");
                        return false;
                    }
                    if(!countOnly)
                    {
                        data[dataIndex + USB_ENDPOINT_ADDRESS_OFFSET  ] = address;
                        data[dataIndex + USB_ENDPOINT_ATTRIBUTE_OFFSET] = attributes;
                    }
                    dataIndex += USB_ENDPOINT_DESCRIPTOR_LENGTH;
                    return true;
                }
            }
            MessageBox.Show("Unexpected end of file while reading EndpointDescriptor block of XML file");
            return false;
        }

        private bool ParseString(XmlTextReader reader)
        {
            if (!reader.MoveToFirstAttribute() || reader.Name != "index")
            {
                MessageBox.Show("All String descriptors must have an index attribute");
                return false;
            }
            string text;             // Read the index value
            ulong value;
            if (ConvertInteger(reader.Value, 0, out value) <= 0 || value > byte.MaxValue || value == 0)
            {
                MessageBox.Show("String index (" + reader.Value + ") is not legal.");
                return false;
            }
            reader.MoveToElement();
            text = reader.ReadInnerXml();           // Read the string information
            if (0 == AddString(text, (int)value))        // Add string to auto string handler
            {
                return false;
            }
            return true;
        }

        private bool ParseGeneric(XmlTextReader reader)
        {
            const byte USB_GENERIC_DESCRIPTOR_MARKER = 0xFF;
            const byte USB_REQUEST_TYPE_IN           = 0x80;
            const byte USB_REQUEST_TYPE_STANDARD     = 0x00;
            const byte USB_REQUEST_TYPE_CLASS        = 0x20;
            const byte USB_REQUEST_TYPE_VENDOR       = 0x40;
            const byte USB_GET_DESCRIPTOR_REQUEST    = 0x06;
            const byte USB_GENERIC_TYPE_OFFSET       = 0;
            const byte USB_GENERIC_REQUEST_OFFSET    = 1;
            const byte USB_GENERIC_VALUE_OFFSET      = 2;
            const byte USB_GENERIC_INDEX_OFFSET      = 4;
            const byte USB_GENERIC_HEADER_LENGTH     = 6;

            // The Generic headers must wait until all information has been gathered before they can be read
            // Therefore, the current index will be saved and the pointer advanced past the headers with nothing
            // written until all information is present
            int headerIndex = dataIndex;                                                // Save Generic header start location
            dataIndex += USB_DESCRIPTOR_HEADER_LENGTH + USB_GENERIC_HEADER_LENGTH;      // Advance data pointer to payload section
            byte type = USB_REQUEST_TYPE_IN;                                            // Out type requests are not supported

            // Keep track of which required fields have been read
            bool foundValue = false;

            if (!reader.MoveToFirstAttribute() || reader.Name != "type")
            {
                MessageBox.Show("The GenericDescriptor element must have a 'type' attribute");
                return false;
            }
            if (reader.Value == "standard")
            {
                type |= USB_REQUEST_TYPE_STANDARD;
            }
            else if (reader.Value == "class")
            {
                type |= USB_REQUEST_TYPE_CLASS;
            }
            else if (reader.Value == "vendor")
            {
                type |= USB_REQUEST_TYPE_VENDOR;
            }
            else
            {
                MessageBox.Show("The GenericDescriptor type attribute had the value '" + reader.Value + "' rather than 'standard', 'class', or 'vendor'");
                return false;
            }
            reader.MoveToElement();

            byte bRequest = USB_GET_DESCRIPTOR_REQUEST;     // Defaults to Get Descriptor
            uint wValue   = 0;
            uint wIndex   = 0;                              // wIndex defaults to 0
            ulong value   = 0;

            bool stillGood = true;
            while (reader.Read())
            {
                if (!stillGood)
                {
                    return false;
                }
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "bRecipient")
                    {
                        string text = reader.ReadInnerXml();
                        if (text == "device")
                        {
                            continue;
                        }
                        if (text == "interface")
                        {
                            type |= 1;
                            continue;
                        }
                        if (text == "endpoint")
                        {
                            type |= 2;
                            continue;
                        }
                        if (text == "other")
                        {
                            type |= 3;
                            continue;
                        }
                        if(ConvertInteger(text, 0, out value) <= 0 || value > 0x1F)
                        {
                            MessageBox.Show("bRecipient value has bad characters or is too high (must be less than 0x20)");
                            return false;
                        }
                        type |= (byte)value;
                        continue;
                    }
                    if (reader.Name == "wValue")
                    {
                        stillGood = ParseInteger(reader, out value, 2);
                        wValue = (uint)value;
                        foundValue = true;
                        continue;
                    }
                    if (reader.Name == "bRequest")
                    {
                        stillGood = ParseInteger(reader, out value, 1);
                        bRequest = (byte)value;
                        continue;
                    }
                    if (reader.Name == "wIndex")
                    {
                        stillGood = ParseInteger(reader, out value, 2);
                        wIndex = (uint)value;
                        continue;
                    }
                    if (reader.Name == "bPadding")
                    {
                        stillGood = ParseInteger(reader, out value, 1);
                        int bPadding = (int)value;
                        if(stillGood && !countOnly)
                        {
                            for (int i = 0; i < bPadding; i++)
                            {
                                data[dataIndex + i] = 0;
                            }
                        }
                        dataIndex += bPadding;
                        continue;
                    }
                    if (reader.Name == "bData")
                    {
                        stillGood = ParseIntegers(reader, 1);
                        continue;
                    }
                    if (reader.Name == "wData")
                    {
                        stillGood = ParseIntegers(reader, 2);
                        continue;
                    }
                    if (reader.Name == "dwData")
                    {
                        stillGood = ParseIntegers(reader, 4);
                        continue;
                    }
                    if (reader.Name == "iData")
                    {
                        stillGood = ParseAutoString(reader, 0);
                        dataIndex++;
                        continue;
                    }
                    if (reader.Name == "sData")
                    {
                        string text = reader.ReadInnerXml();
                        if (countOnly)
                        {
                            dataIndex += text.Length;
                        }
                        else
                        {
                            for (int i = 0; i < text.Length; i++)
                            {
                                data[dataIndex++] = (byte)text[i];
                            }
                        }
                        continue;
                    }
                    if (reader.Name == "wsData")
                    {
                        string text = reader.ReadInnerXml();
                        if (countOnly)
                        {
                            dataIndex += 2 * text.Length;
                        }
                        else
                        {
                            for (int i = 0; i < text.Length; i++)
                            {
                                data[dataIndex++] = (byte)text[i];
                                data[dataIndex++] = 0;
                            }
                        }
                        continue;
                    }
                    MessageBox.Show("Element type '" + reader.Name + "' is not part of a GenericDescriptor element.");
                    return false;
                }
                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name != "GenericDescriptor")
                    {
                        MessageBox.Show("Found extraneous end element: " + reader.Name + " in GenericDescriptor of XML file");
                        return false;
                    }

                    // Time to fill in the headers and check that all information is present
                    if (!foundValue)
                    {
                        MessageBox.Show("GenericDescriptor element requires a wValue element");
                        return false;
                    }
                    if (!countOnly)
                    {
                        // Write the descriptor header
                        data[headerIndex + HEADER_MARKER_OFFSET    ] = USB_GENERIC_DESCRIPTOR_MARKER;
                        data[headerIndex + HEADER_INDEX_OFFSET     ] = 0;
                        data[headerIndex + HEADER_LENGTH_OFFSET    ] = (byte)(dataIndex - headerIndex);
                        data[headerIndex + HEADER_LENGTH_OFFSET + 1] = (byte)((dataIndex - headerIndex) >> 8);

                        // Write the Generic descriptor header
                        headerIndex += USB_DESCRIPTOR_HEADER_LENGTH;
                        data[headerIndex + USB_GENERIC_TYPE_OFFSET     ] = type;
                        data[headerIndex + USB_GENERIC_REQUEST_OFFSET  ] = bRequest;
                        data[headerIndex + USB_GENERIC_VALUE_OFFSET    ] = (byte)wValue;
                        data[headerIndex + USB_GENERIC_VALUE_OFFSET + 1] = (byte)(wValue >> 8);
                        data[headerIndex + USB_GENERIC_INDEX_OFFSET    ] = (byte)wIndex;
                        data[headerIndex + USB_GENERIC_INDEX_OFFSET + 1] = (byte)(wIndex >> 8);
                    }
                    return true;
                }
            }
            MessageBox.Show("Unexpected end of file while reading GenericDescriptor block of XML file");
            return false;
        }

        private int ConvertInteger(string text, int offset, out ulong number)
        {
            int index = 0;
            bool hex = false;
            ulong value = 0;
            int digits = 0;

            while ((offset + index) < text.Length)
            {
                char c;

                c = text[offset + index++];
                if (c == ' ' || c == '\t' || c == '\r' || c == '\n')     // If white space
                {
                    if (digits != 0)
                        break;
                    continue;       // Just skip past initial white space
                }
                if (c == '#')
                {
                    if (!hex && (offset + index) < (text.Length - 1) && (text[offset+index] == 'x' || text[offset+index] == 'X'))
                    {
                        index++;        // Skip past 'x'
                        hex = true;
                        continue;
                    }
                }
                if (c >= '0' && c <= '9')
                {
                    if (hex)
                    {
                        value = (value << 4) + (ulong)(c - '0');
                    }
                    else
                    {
                        value = (value * 10) + (ulong)(c - '0');
                    }
                    digits++;
                    continue;
                }
                if (hex && c >= 'a' && c <= 'f')
                {
                    value = (value << 4) + (ulong)(c + 10 - 'a');
                    digits++;
                    continue;
                }
                if (hex && c >= 'A' && c <= 'F')
                {
                    value = (value << 4) + (ulong)(c + 10 - 'A');
                    digits++;
                    continue;
                }
                number = 0;
                return 1 - index;       // Negative return indicates an illegal character at the negative index
            }
            if (digits == 0)
            {
                number = 0;
                return 0;               // Zero return indicates no value was found
            }
            number = value;
            return index;               // Positive return is number of characters read in
        }

        private bool ParseInteger(XmlTextReader reader, out ulong value, int nBytes)
        {
            string name = reader.Name;
            string text = reader.ReadInnerXml();
            return ParseInteger(name, text, out value, nBytes);
        }

        private bool ParseInteger(string name, string text, out ulong value, int nBytes)
        {
            ulong number;
            int nRead = ConvertInteger(text, 0, out number);

            if (nRead < 0)
            {
                MessageBox.Show("Illegal character (" + text[0 - nRead] + ") found in " + name + " element.");
                value = 0;
                return false;
            }
            if (nRead == 0)
            {
                MessageBox.Show("No value was found in " + name + " element.");
                value = 0;
                return false;
            }
            value = number;
            if (ConvertInteger(text, nRead, out number) != 0)
            {
                MessageBox.Show("Garbage discovered past end of number in " + name + " element.");
                return false;
            }
            if ((nBytes == 1 && value > byte.MaxValue) || (nBytes == 2 && value > UInt16.MaxValue) || (nBytes == 4 && value > UInt32.MaxValue))
            {
                MessageBox.Show("Number value was too large in " + name + " element.");
                return false;
            }
            return true;
        }

        private bool ParseIntegers(XmlTextReader reader, int nBytes)
        {
            string name = reader.Name;
            string text = reader.ReadInnerXml();
            ulong value = 0;
            int index   = 0;
            int nRead   = 0;
            int numbers = 0;

            do
            {
                nRead = ConvertInteger(text, index, out value);
                if (nRead > 0)
                {
                    if(!SaveInteger(name, value, dataIndex, nBytes))
                    {
                        return false;
                    }
                    numbers++;
                    index += nRead;
                    dataIndex += nBytes;
                }
            } while (nRead > 0);
            if (nRead == 0 && numbers == 0)
            {
                MessageBox.Show("No values were found in " + name + " element.");
                return false;
            }
            if (nRead < 0)
            {
                MessageBox.Show("Illegal character (" + text[index - nRead] + ") found in " + name + " element.");
                return false;
            }
            return true;
        }

        bool ParseBcd(XmlTextReader reader, byte offset)
        {
            string name = reader.Name;
            string text = reader.ReadInnerXml();
            int index   = 0;
            int numbers = 0;
            int digits  = 0;
            ulong value = 0;
            bool dPoint = false;

            while (index < text.Length)
            {
                char c;

                c = text[index++];
                if (c == ' ' || c == '\t' || c == '\r' || c == '\n')
                {
                    if (digits != 0 || dPoint)
                    {
                        if (numbers != 0)
                        {
                            break;      // Only one number allowed in a BCD field
                        }

                        // Normalize value so decimal point lies between bytes
                        if (dPoint)
                        {
                            value <<= (4 * (2 - digits));
                        }
                        else
                        {
                            value <<= 8;
                        }

                        // BCD values are always two bytes and always part of a well-defined structure
                        if (!SaveInteger(name, value, dataIndex + offset, 2))
                        {
                            return false;
                        }
                        dPoint = false;
                        digits = 0;
                        numbers++;
                    }
                    continue;
                }
                if (c == '.' && !dPoint)
                {
                    dPoint = true;
                    digits = 0;
                    continue;
                }
                if (c >= '0' && c <= '9')
                {
                    if (digits > 1)
                    {
                        MessageBox.Show("Too many digits for BCD value in " + name + " element.");
                        return false;
                    }
                    value = (value << 4) + (ulong)(c - '0');
                    digits++;
                    continue;
                }
                MessageBox.Show("Non-numeric character (" + c + ") found in " + name + " BCD element.");
                return false;
            }
            if (digits != 0 || dPoint)
            {
                if (numbers != 0)
                {
                    MessageBox.Show("More than one numeric value found in " + name + " element");
                    return false;
                }

                // Normalize value so decimal point lies between bytes
                if (dPoint)
                {
                    value <<= (4 * (2 - digits));
                }
                else
                {
                    value <<= 8;
                }

                // BCD values are always two bytes and always part of a well-defined structure
                if (!SaveInteger(name, value, dataIndex + offset, 2))
                {
                    return false;
                }
                numbers++;
            }
            if (numbers == 0)       // If no numbers were found in the element
            {
                MessageBox.Show("Empty " + name + " element. Expected a BCD value.");
                return false;
            }
            return true;
        }

        bool SaveInteger(string name, ulong value, int index, int nBytes)
        {
            switch(nBytes)
            {
                case 1:
                    if (value > byte.MaxValue)
                    {
                        break;
                    }
                    if (!countOnly)
                    {
                        data[index] = (byte)value;
                    }
                    return true;
                case 2:
                    if (value > UInt16.MaxValue)
                    {
                        break;
                    }
                    if (!countOnly)
                    {
                        data[index] = (byte)value;
                        data[index + 1] = (byte)(value >> 8);
                    }
                    return true;
                case 4:
                    if (value > UInt32.MaxValue)
                    {
                        break;
                    }
                    if (!countOnly)
                    {
                        data[index    ] = (byte)(value      );
                        data[index + 1] = (byte)(value >> 8 );
                        data[index + 2] = (byte)(value >> 16);
                        data[index + 3] = (byte)(value >> 24);
                    }
                    return true;
                default:
                    MessageBox.Show("Programming error: nBytes has value of " + nBytes.ToString() + " in " + name + " element.");
                    return false;
            }
            MessageBox.Show("Integer value too large in " + name + " element.");
            return false;
        }

        private class StringDescriptor
        {
            public int Index;
            public string Text;

            public StringDescriptor(int index, string text)
            {
                Index = index;
                Text = text;
            }
        }

        private bool ParseAutoString(XmlTextReader reader, byte offset)
        {
            string name = reader.Name;
            string text = reader.ReadInnerXml();
            int index   = AddString(text, 0);

            if (index == 0)         // If there was an error adding the string
            {
                return false;
            }
            if (!countOnly)
            {
                data[dataIndex + (int)offset] = (byte)index;
            }
            return true;
        }

        private int AddString(string text, int index)
        {
            if (index == 0)      // If this is an auto string (no index supplied)
            {
                index = autoIndex;
                autoIndex++;
                if (autoIndex == USB_DISPLAY_STRING_NUM || autoIndex == USB_FRIENDLY_STRING_NUM)     // Off-limits string indexes
                {
                    autoIndex = 6;
                }
            }
            if (index < 1 || index > byte.MaxValue)
            {
                MessageBox.Show("Specified string descriptor index (" + index.ToString() + ") is too large.");
                return 0;
            }
            if (text.Length >= 126)
            {
                MessageBox.Show("Specified string (" + text + ") is too long.  It must have fewer than 126 characters.");
                return 0;
            }
            stringList.Add(new StringDescriptor(index, text));
            return index;
        }

        private void DumpStrings()
        {
            const byte USB_STRING_DESCRIPTOR_MARKER        = 0x03;
            const byte USB_STRING_DESCRIPTOR_TYPE          = 3;
            const byte USB_STRING_BLENGTH_OFFSET           = 0;
            const byte USB_STRING_DESCRIPTOR_TYPE_OFFSET   = 1;
            const byte USB_STRING_DESCRIPTOR_HEADER_LENGTH = 2;

            for (int i = 0; i < stringList.Count; i++)
            {
                StringDescriptor stringDesc;

                stringDesc = (StringDescriptor)stringList[i];

                // Write out the descriptor header information
                if (!countOnly)
                {
                    int length = (stringDesc.Text.Length * 2) + USB_STRING_DESCRIPTOR_HEADER_LENGTH + USB_DESCRIPTOR_HEADER_LENGTH;
                    data[dataIndex + HEADER_MARKER_OFFSET    ] = USB_STRING_DESCRIPTOR_MARKER;
                    data[dataIndex + HEADER_INDEX_OFFSET     ] = (byte)stringDesc.Index;
                    data[dataIndex + HEADER_LENGTH_OFFSET    ] = (byte)length;
                    data[dataIndex + HEADER_LENGTH_OFFSET + 1] = (byte)(length >> 8);
                }
                dataIndex += USB_DESCRIPTOR_HEADER_LENGTH;

                // Write out the string descriptor header
                if (!countOnly)
                {
                    byte length = (byte)((stringDesc.Text.Length * 2) + USB_STRING_DESCRIPTOR_HEADER_LENGTH);
                    data[dataIndex + USB_STRING_BLENGTH_OFFSET        ] = length;
                    data[dataIndex + USB_STRING_DESCRIPTOR_TYPE_OFFSET] = USB_STRING_DESCRIPTOR_TYPE;
                }
                dataIndex += USB_STRING_DESCRIPTOR_HEADER_LENGTH;

                // Write out string text information
                for (int index = 0; index < stringDesc.Text.Length; index++)
                {
                    char c;

                    if (!countOnly)
                    {
                        c = stringDesc.Text[index];
                        data[dataIndex] = (byte)c;
                        data[dataIndex + 1] = 0;
                    }
                    dataIndex += 2;
                }
            }
        }
    }

    public class UsbXMLConfigGet
    {
    }

}