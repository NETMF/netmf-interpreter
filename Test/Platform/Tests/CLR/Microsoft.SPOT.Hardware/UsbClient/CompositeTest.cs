using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Hardware.UsbClient;
using Microsoft.SPOT.Platform.Test;

namespace iMXS_Compound
{
    public class CompositeTest : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("CompositTest needs a serial debugging as the usbcontroller port status has to be Stopped");
            return InitializeResult.Skip;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("UsbClient CleanUp");
        }

        public Cpu.Pin p = Cpu.Pin.GPIO_NONE;
        [TestMethod]
        public MFTestResults CompositeDeviceTest()
        {
            MFTestResults testResult = MFTestResults.Pass;

            // Gain access to all USB controllers
            UsbController[] controllers = UsbController.GetControllers();

            // Set up all buttons to be monitored
            buttons.up = new InputPort((Cpu.Pin)43, true, Port.ResistorMode.Disabled);
            buttons.down = new InputPort((Cpu.Pin)42, true, Port.ResistorMode.Disabled);
            buttons.left = new InputPort((Cpu.Pin)48, true, Port.ResistorMode.Disabled);
            buttons.right = new InputPort((Cpu.Pin)44, true, Port.ResistorMode.Disabled);
            buttons.b1 = new InputPort((Cpu.Pin)40, true, Port.ResistorMode.Disabled);
            buttons.b2 = new InputPort((Cpu.Pin)45, true, Port.ResistorMode.Disabled);
            buttons.b3 = new InputPort((Cpu.Pin)46, true, Port.ResistorMode.Disabled);
            buttons.done = new InputPort((Cpu.Pin)47, true, Port.ResistorMode.Disabled);


            // Use the first available USB controller if it exists
            if (controllers.Length < 1)
            {
                Debug.Print("No USB controllers exist for this device - we're done!");
                return MFTestResults.Skip;
            }
            UsbController UsbPort = controllers[0];


            // Configure the USB port and open streams to both interfaces
            if (!ConfigureMouseAndPinger(UsbPort))      // If USB configure fails, we're done
            {
                testResult = MFTestResults.Fail;
            }
            UsbStream pinger = UsbPort.CreateUsbStream(1, 2);                           // Pinger writes to endpoint 1 and reads from endpoint 2
            UsbStream mouse = UsbPort.CreateUsbStream(3, UsbStream.NullEndpoint);       // Mouse is write only to endpoint 3
            MouseAndPingerLoop(mouse, pinger);      // Behave like both a mouse and "TinyBooter

            // Done being a mouse and a pinger.  Start being just a pinger
            pinger.Close();
            mouse.Close();
            UsbPort.Stop();
            Thread.Sleep(500);          // Keep USB port disconnected for half a second to be sure host has seen
            if (!ConfigurePinger(UsbPort))
            {
                testResult = MFTestResults.Fail;
            }
            pinger = UsbPort.CreateUsbStream(1, 2);       // Pinger writes to endpoint 1 and reads from endpoint 2
            PingerLoop(pinger);

            // Done being just a pinger.  Start being a mouse and pinger again
            pinger.Close();
            UsbPort.Stop();
            Thread.Sleep(500);          // Keep USB port disconnected for half a second to be sure host has seen

            return testResult;
        }
        public struct ButtonList
        {
            public InputPort up;
            public InputPort down;
            public InputPort left;
            public InputPort right;
            public InputPort b1;
            public InputPort b2;
            public InputPort b3;
            public InputPort done;
        };

        public static ButtonList buttons;

        static void MouseAndPingerLoop(UsbStream mouse, UsbStream pinger)
        {
            // Allocate in and out packets for "TinyBooter" simulation
            WireProtocol.WP_Packet inPacket;                                        // Allocate packet for Pinger input
            UInt16 seq = 0;                                                         // Initialize Pinger packet sequence number

            // While done button is not pressed
            while (buttons.done.Read())
            {
                // Perform these operations once every 10 milliseconds (actually a bit more than 10 milliseconds)
                // We've asked the host to query for mouse info at least every 10 milliseconds, but it actually
                // queries every 8 milliseconds - it's OK not to respond to every query.
                Thread.Sleep(10);

                byte xChange = 0;
                byte yChange = 0;

                // Make the mouse move by 3 steps each sample time if any movement button is pressed
                if (!buttons.up.Read())
                    yChange = 0xFD;
                if (!buttons.down.Read())
                    yChange = 3;
                if (!buttons.left.Read())
                    xChange = 0xFD;
                if (!buttons.right.Read())
                    xChange = 3;

                // Report to host the condition of "movement" or of the buttons
                SendMouseReport(mouse, !buttons.b1.Read(), !buttons.b2.Read(), !buttons.b3.Read(), xChange, yChange);

                // If a good WireProtocol packet was received from the host
                if (WireProtocol.ReadPacket(pinger, out inPacket))
                {
                    RespondToPacket(pinger, inPacket, seq);
                }
            }

            // Wait for the done button to be released
            while (!buttons.done.Read()) ;
        }

        static void PingerLoop(UsbStream pinger)
        {
            // Allocate in and out packets for "TinyBooter" simulation
            WireProtocol.WP_Packet inPacket;                                        // Allocate packet for Pinger input
            UInt16 seq = 0;                                                         // Initialize Pinger packet sequence number

            // While the done button is not pressed
            while (buttons.done.Read())
            {
                Thread.Sleep(10);       // No need to respond in a hurry

                if (WireProtocol.ReadPacket(pinger, out inPacket))
                {
                    RespondToPacket(pinger, inPacket, seq);
                }
            }

            // Wait for the done button to be released
            while (!buttons.done.Read()) ;
        }

        // Accepts only a good packet.  If the packet is a Ping, the Ping response (ACK) is sent.
        // Otherwise, the command is NAK'ed.
        static bool RespondToPacket(UsbStream pinger, WireProtocol.WP_Packet inPacket, UInt16 seq)
        {
            WireProtocol.WP_Packet outPacket = new WireProtocol.WP_Packet();

            // Allocate space for any data following the packet
            int size = (int)(inPacket.m_size & 0xFFFF);
            byte[] buffer = new byte[size];

            // Read in the data that came with the packet
            if (inPacket.m_size != 0)
                pinger.Read(buffer, 0, size);

            // Fill in the blanks of the response packet
            outPacket.m_signature = "MSdbgV1";                      // Standard target signature
            outPacket.m_cmd = inPacket.m_cmd;                 // Show which command this is a response to
            outPacket.m_seq = seq++;                          // Keep track of the target message sequence number
            outPacket.m_seqReply = inPacket.m_seq;                 // Show which host packet sequence number this is a response to

            // If the host packet was a Ping
            if (inPacket.m_cmd == WireProtocol.Commands.Ping)
            {
                byte[] data = new byte[8];                          // The Ping has an 8 byte data response
                outPacket.m_flags = 0x8003;                         // Return a low-priority ACK
                for (int i = 0; i < 8; i++)
                    data[i] = 0;                                    // Initialize all bytes of the data response to zero
                data[0] = 1;                                        // This tells the host that we are TinyBooter
                data[4] = 2;                                        // This is an innoccuous flag value
                WireProtocol.SendPacket(pinger, outPacket, data);   // Send response to the Ping
            }
            else
            {
                outPacket.m_flags = 0x4003;                                     // Return a low-priority NACK
                WireProtocol.SendPacket(pinger, outPacket, new byte[0]);        // Signal to the host that we don't know what to do with the command
            }
            return true;
        }

        static bool SendMouseReport(UsbStream stream, bool button1, bool button2, bool button3, byte Xmovement, byte Ymovement)
        {
            byte[] report = new byte[3];

            report[0] = (byte)((button1 ? 1 : 0) | (button2 ? 2 : 0) | (button3 ? 4 : 0));
            report[1] = Xmovement;
            report[2] = Ymovement;

            stream.Write(report, 0, 3);
            return true;
        }

        static bool ConfigureMouseAndPinger(UsbController port)
        {
            UsbController.PortState portState = port.Status;

            if (portState != UsbController.PortState.Stopped)
            {
                Debug.Print("The controller has already been initialized and is in state " + portState.ToString() + ".");
                // return false;
            }

            // Create the device descriptor
            Configuration.DeviceDescriptor device = new Configuration.DeviceDescriptor(0x15A2, 0x0026, 0x0100);
            device.iManufacturer = 1;       // String 1 is the manufacturer name
            device.iProduct = 2;       // String 2 is the product name
            device.bcdUSB = 0x200;  // USB version 2.00

            // Create the compound configuration descriptor
            Configuration.Endpoint endpoint1 = new Configuration.Endpoint(1, Configuration.Endpoint.ATTRIB_Bulk | Configuration.Endpoint.ATTRIB_Write);
            Configuration.Endpoint endpoint2 = new Configuration.Endpoint(2, Configuration.Endpoint.ATTRIB_Bulk | Configuration.Endpoint.ATTRIB_Read);
            Configuration.Endpoint endpoint3 = new Configuration.Endpoint(3, Configuration.Endpoint.ATTRIB_Interrupt | Configuration.Endpoint.ATTRIB_Write);
            endpoint1.wMaxPacketSize = 64;
            endpoint2.wMaxPacketSize = 64;      // Pinger endpoints use the maximum allowable buffer
            endpoint3.wMaxPacketSize = 8;       // Mouse data requires only eight bytes
            endpoint3.bInterval = 10;      // Host should request data from mouse every 10 mS
            Configuration.Endpoint[] pingerEndpoints = new Configuration.Endpoint[2] { endpoint1, endpoint2 };
            Configuration.Endpoint[] mouseEndpoints = new Configuration.Endpoint[1] { endpoint3 };
            // Set up the Pinger interface
            Configuration.UsbInterface UsbInterface0 = new Configuration.UsbInterface(0, pingerEndpoints);
            UsbInterface0.bInterfaceClass = 0xFF;    // Vendor class
            UsbInterface0.bInterfaceSubClass = 1;
            UsbInterface0.bInterfaceProtocol = 1;
            // Set up the Mouse interface
            Configuration.UsbInterface UsbInterface1 = new Configuration.UsbInterface(1, mouseEndpoints);
            UsbInterface1.bInterfaceClass = 3;    // HID interface
            UsbInterface1.bInterfaceSubClass = 1;    // Boot device
            UsbInterface1.bInterfaceProtocol = 2;    // Standard mouse protocol
            // Assemble the HID class descriptor
            byte[] HidPayload = new byte[]
            {
                0x01, 0x01,     // bcdHID = HID version 1.01
                0x00,           // bCountryCode (unimportant)
                0x01,           // bNumDescriptors = number of descriptors available for this device
                0x22,           // bDescriptorType = Report descriptor
                0x32, 0x00      // Total size of Report descriptor (50)
            };
            UsbInterface1.classDescriptors = new Configuration.ClassDescriptor[1];
            UsbInterface1.classDescriptors[0] = new Configuration.ClassDescriptor(0x21, HidPayload);
            Configuration.UsbInterface[] UsbInterfaces = new Configuration.UsbInterface[2] { UsbInterface0, UsbInterface1 };
            Configuration.ConfigurationDescriptor config = new Configuration.ConfigurationDescriptor(280, UsbInterfaces);

            // Create the report descriptor as a Generic
            // This data was created using 
            byte[] BootMouseReportPayload = new byte[]
            {
                0x05, 0x01,                    // USAGE_PAGE (Generic Desktop)
                0x09, 0x02,                    // USAGE (Mouse)
                0xa1, 0x01,                    // COLLECTION (Application)
                0x09, 0x01,                    //   USAGE (Pointer)
                0xa1, 0x00,                    //   COLLECTION (Physical)
                0x05, 0x09,                    //     USAGE_PAGE (Button)
                0x19, 0x01,                    //     USAGE_MINIMUM (Button 1)
                0x29, 0x03,                    //     USAGE_MAXIMUM (Button 3)
                0x15, 0x00,                    //     LOGICAL_MINIMUM (0)
                0x25, 0x01,                    //     LOGICAL_MAXIMUM (1)
                0x95, 0x03,                    //     REPORT_COUNT (3)
                0x75, 0x01,                    //     REPORT_SIZE (1)
                0x81, 0x02,                    //     INPUT (Data,Var,Abs)
                0x95, 0x01,                    //     REPORT_COUNT (1)
                0x75, 0x05,                    //     REPORT_SIZE (5)
                0x81, 0x01,                    //     INPUT (Cnst,Ary,Abs)
                0x05, 0x01,                    //     USAGE_PAGE (Generic Desktop)
                0x09, 0x30,                    //     USAGE (X)
                0x09, 0x31,                    //     USAGE (Y)
                0x15, 0x81,                    //     LOGICAL_MINIMUM (-127)
                0x25, 0x7f,                    //     LOGICAL_MAXIMUM (127)
                0x75, 0x08,                    //     REPORT_SIZE (8)
                0x95, 0x02,                    //     REPORT_COUNT (2)
                0x81, 0x06,                    //     INPUT (Data,Var,Rel)
                0xc0,                          //     END_COLLECTION
                0xc0                           // END_COLLECTION
            };
            const byte DescriptorRequest = 0x81;
            const byte ReportDescriptor = 0x22;
            const byte GetDescriptor = 0x06;
            const ushort Report_wValue = (ushort)ReportDescriptor << 8;
            Configuration.GenericDescriptor mouseReportDescriptor = new Configuration.GenericDescriptor(DescriptorRequest, Report_wValue, BootMouseReportPayload);
            mouseReportDescriptor.bRequest = GetDescriptor;
            mouseReportDescriptor.wIndex = 1;        // The interface number

            // Create the standard strings needed for the iMXS board Pinger
            Configuration.StringDescriptor manufacturerName = new Configuration.StringDescriptor(1, "Freescale");
            Configuration.StringDescriptor productName = new Configuration.StringDescriptor(2, "Micro Framework MXS Reference ");
            Configuration.StringDescriptor displayName = new Configuration.StringDescriptor(4, "iMXS");
            Configuration.StringDescriptor friendlyName = new Configuration.StringDescriptor(5, "a7e70ea2");

            // Create the Sideshow OS String descriptor as if it were a standard string descriptor
            Configuration.StringDescriptor OS_StringDescriptor = new Configuration.StringDescriptor(0xEE, "MSFT100\xA5");

            // Create the Sideshow OS Extended Compatible ID Descriptor as a Generic
            byte[] compatibleIdPayload = new byte[]
            {
                40, 0, 0, 0,    // Size of the payload
                00, 1,          // Version 1.00
                04, 0,          // OS Compatible ID request
                1,              // Interface count
                0,0,0,0,0,0,0,  // padding (7 zeros)

                0,              // Interface number
                1,              // reserved
                (byte)'S', (byte)'I', (byte)'D', (byte)'E', (byte)'S', (byte)'H', (byte)'W', 0,     // Compatible ID
                (byte)'E', (byte)'N', (byte)'H', (byte)'V', (byte)'1',         0,         0, 0,     // Sub-compatible ID
                0,0,0,0,0,0     // padding (6 zeros)
            };
            // Create the report descriptor as a Generic
            const byte RequestType = Configuration.GenericDescriptor.REQUEST_IN | Configuration.GenericDescriptor.REQUEST_Vendor;
            const byte RequestCommand = 0xA5;
            const byte XCompatibleOsRequest = 4;
            Configuration.GenericDescriptor XCompatibleOsDescriptor = new Configuration.GenericDescriptor(RequestType, 0, compatibleIdPayload);
            XCompatibleOsDescriptor.bRequest = RequestCommand;
            XCompatibleOsDescriptor.wIndex = XCompatibleOsRequest;

            // Now assemble all of these descriptors into the final configuration
            Configuration compoundConfig = new Configuration();
            compoundConfig.descriptors = new Configuration.Descriptor[]
            {
                device,
                config,
                manufacturerName,
                productName,
                displayName,
                friendlyName,
                OS_StringDescriptor,
                XCompatibleOsDescriptor,
                mouseReportDescriptor
            };

            try
            {
                // Set the configuration to the USB Controller
                port.Configuration = compoundConfig;

                if (port.ConfigurationError != UsbController.ConfigError.ConfigOK)
                {
                    Debug.Print("Compound configuration reported an error " + port.ConfigurationError.ToString());
                    return false;
                }

                // Kick the USB Controller into action
                if (!port.Start())
                {
                    Debug.Print("Compound USB could not be started.");
                    return false;
                }
            }
            catch (ArgumentException)
            {
                Debug.Print("Couldn't configure Compound USB due to error " + port.ConfigurationError.ToString());
                return false;
            }

            const int MaxTenths = 20;       // Wait up to two seconds for controller to finish handshake
            int tenths;
            for (tenths = 0; tenths < MaxTenths; tenths++)
            {
                if (port.Status == UsbController.PortState.Running)     // If host has finished with handshake
                    break;
                System.Threading.Thread.Sleep(100);
            }
            if (tenths < MaxTenths)
                return true;

            Debug.Print("After " + MaxTenths.ToString() + " tenths of a second, Compound USB still had status of " + port.Status.ToString());
            return false;
        }

        static bool ConfigurePinger(UsbController port)
        {
            UsbController.PortState portState = port.Status;

            if (portState != UsbController.PortState.Stopped)
            {
                Debug.Print("The controller has already been initialized and is in state " + portState.ToString() + ".");
                // return false;
            }

            // Create the device descriptor
            Configuration.DeviceDescriptor device = new Configuration.DeviceDescriptor(0x15A2, 0x0026, 0x0100);
            device.iManufacturer = 1;       // String 1 is the manufacturer name
            device.iProduct = 2;       // String 2 is the product name
            device.bcdUSB = 0x0200;  // USB version 2.00

            // Create the simple configuration descriptor
            Configuration.Endpoint endpoint1 = new Configuration.Endpoint(1, Configuration.Endpoint.ATTRIB_Bulk | Configuration.Endpoint.ATTRIB_Write);
            Configuration.Endpoint endpoint2 = new Configuration.Endpoint(2, Configuration.Endpoint.ATTRIB_Bulk | Configuration.Endpoint.ATTRIB_Read);
            endpoint1.wMaxPacketSize = 64;
            endpoint2.wMaxPacketSize = 64;      // Pinger endpoints use the maximum allowable buffer
            Configuration.Endpoint[] endpoints = new Configuration.Endpoint[2] { endpoint1, endpoint2 };
            // Assign the endpoints to the interface and set up the interface fields
            Configuration.UsbInterface UsbInterface0 = new Configuration.UsbInterface(0, endpoints);
            UsbInterface0.bInterfaceClass = 0xFF;    // Vendor class
            UsbInterface0.bInterfaceSubClass = 1;
            UsbInterface0.bInterfaceProtocol = 1;
            // Finish by assigning the interfaces to the configuration descriptor
            Configuration.UsbInterface[] UsbInterfaces = new Configuration.UsbInterface[1] { UsbInterface0 };
            Configuration.ConfigurationDescriptor config = new Configuration.ConfigurationDescriptor(280, UsbInterfaces);

            // Create the standard strings needed for the iMXS board Pinger
            Configuration.StringDescriptor manufacturerName = new Configuration.StringDescriptor(1, "Freescale");
            Configuration.StringDescriptor productName = new Configuration.StringDescriptor(2, "Micro Framework MXS Reference ");
            Configuration.StringDescriptor displayName = new Configuration.StringDescriptor(4, "iMXS");
            Configuration.StringDescriptor friendlyName = new Configuration.StringDescriptor(5, "a7e70ea2");

            // Create the Sideshow OS String descriptor as if it were a standard string descriptor
            Configuration.StringDescriptor OS_StringDescriptor = new Configuration.StringDescriptor(0xEE, "MSFT100\xA5");

            // Create the Sideshow OS Extended Compatible ID Descriptor as a Generic
            byte[] compatibleIdPayload = new byte[]
            {
                40, 0, 0, 0,    // Size of the payload
                00, 1,          // Version 1.00
                04, 0,          // OS Compatible ID request
                1,              // Interface count
                0,0,0,0,0,0,0,  // padding (7 zeros)

                0,              // Interface number
                1,              // reserved
                (byte)'S', (byte)'I', (byte)'D', (byte)'E', (byte)'S', (byte)'H', (byte)'W', 0,     // Compatible ID
                (byte)'E', (byte)'N', (byte)'H', (byte)'V', (byte)'1',         0,         0, 0,     // Sub-compatible ID
                0,0,0,0,0,0     // padding (6 zeros)
            };
            // Assign the payload to the generic and fill in the generic descriptor fields
            const byte RequestType = Configuration.GenericDescriptor.REQUEST_IN | Configuration.GenericDescriptor.REQUEST_Vendor;
            const byte RequestCommand = 0xA5;
            const byte XCompatibleOsRequest = 4;
            Configuration.GenericDescriptor XCompatibleOsDescriptor = new Configuration.GenericDescriptor(RequestType, 0, compatibleIdPayload);
            XCompatibleOsDescriptor.bRequest = RequestCommand;
            XCompatibleOsDescriptor.wIndex = XCompatibleOsRequest;

            // Now assemble all of these descriptors into the final configuration
            Configuration simpleConfig = new Configuration();
            simpleConfig.descriptors = new Configuration.Descriptor[]
            {
                device,
                config,
                manufacturerName,
                productName,
                displayName,
                friendlyName,
                OS_StringDescriptor,
                XCompatibleOsDescriptor,
            };

            try
            {
                // Set the configuration to the USB Controller
                port.Configuration = simpleConfig;

                if (port.ConfigurationError != UsbController.ConfigError.ConfigOK)
                {
                    Debug.Print("Simple configuration reported an error " + port.ConfigurationError.ToString());
                    return false;
                }

                // Kick the USB Controller into action
                if (!port.Start())
                {
                    Debug.Print("Simple USB could not be started.");
                    return false;
                }
            }
            catch (ArgumentException)
            {
                Debug.Print("Couldn't configure Simple USB due to error " + port.ConfigurationError.ToString());
                return false;
            }

            const int MaxTenths = 20;       // Wait up to two seconds for controller to finish handshake
            int tenths;
            for (tenths = 0; tenths < MaxTenths; tenths++)
            {
                if (port.Status == UsbController.PortState.Running)     // If host has finished with handshake
                    break;
                System.Threading.Thread.Sleep(100);
            }
            if (tenths < MaxTenths)
                return true;

            Debug.Print("After " + MaxTenths.ToString() + " tenths of a second, Simple USB still had status of " + port.Status.ToString());
            return false;
        }

        class WireProtocol
        {
            public enum Commands
            {
                Ping = 0,
                Message = 1
            };

            public struct WP_Packet
            {
                public const int m_signature_offset = 0;
                public const int m_crcHeader_offset = 8;
                public const int m_crcData_offset = 12;
                public const int m_cmd_offset = 16;
                public const int m_seq_offset = 20;
                public const int m_seqReply_offset = 22;
                public const int m_flags_offset = 24;
                public const int m_size_offset = 28;
                public const int packetSize = 32;

                public string m_signature;      // Offset 0 - eight bytes, zero terminated
                public UInt32 m_crcHeader;      // Offset 8
                public UInt32 m_crcData;        // Offset 12

                public Commands m_cmd;            // Offset 16
                public UInt16 m_seq;            // Offset 20
                public UInt16 m_seqReply;       // Offset 22
                public UInt32 m_flags;          // Offset 24
                public UInt32 m_size;           // Offset 28
            };

            //
            // CRC 32 table for use under ZModem protocol, IEEE 802
            // G(x) = x^32+x^26+x^23+x^22+x^16+x^12+x^11+x^10+x^8+x^7+x^5+x^4+x^2+x+1
            //
            static UInt32[] c_CRCTable =
            {
                0x00000000, 0x04C11DB7, 0x09823B6E, 0x0D4326D9, 0x130476DC, 0x17C56B6B, 0x1A864DB2, 0x1E475005,
                0x2608EDB8, 0x22C9F00F, 0x2F8AD6D6, 0x2B4BCB61, 0x350C9B64, 0x31CD86D3, 0x3C8EA00A, 0x384FBDBD,
                0x4C11DB70, 0x48D0C6C7, 0x4593E01E, 0x4152FDA9, 0x5F15ADAC, 0x5BD4B01B, 0x569796C2, 0x52568B75,
                0x6A1936C8, 0x6ED82B7F, 0x639B0DA6, 0x675A1011, 0x791D4014, 0x7DDC5DA3, 0x709F7B7A, 0x745E66CD,
                0x9823B6E0, 0x9CE2AB57, 0x91A18D8E, 0x95609039, 0x8B27C03C, 0x8FE6DD8B, 0x82A5FB52, 0x8664E6E5,
                0xBE2B5B58, 0xBAEA46EF, 0xB7A96036, 0xB3687D81, 0xAD2F2D84, 0xA9EE3033, 0xA4AD16EA, 0xA06C0B5D,
                0xD4326D90, 0xD0F37027, 0xDDB056FE, 0xD9714B49, 0xC7361B4C, 0xC3F706FB, 0xCEB42022, 0xCA753D95,
                0xF23A8028, 0xF6FB9D9F, 0xFBB8BB46, 0xFF79A6F1, 0xE13EF6F4, 0xE5FFEB43, 0xE8BCCD9A, 0xEC7DD02D,
                0x34867077, 0x30476DC0, 0x3D044B19, 0x39C556AE, 0x278206AB, 0x23431B1C, 0x2E003DC5, 0x2AC12072,
                0x128E9DCF, 0x164F8078, 0x1B0CA6A1, 0x1FCDBB16, 0x018AEB13, 0x054BF6A4, 0x0808D07D, 0x0CC9CDCA,
                0x7897AB07, 0x7C56B6B0, 0x71159069, 0x75D48DDE, 0x6B93DDDB, 0x6F52C06C, 0x6211E6B5, 0x66D0FB02,
                0x5E9F46BF, 0x5A5E5B08, 0x571D7DD1, 0x53DC6066, 0x4D9B3063, 0x495A2DD4, 0x44190B0D, 0x40D816BA,
                0xACA5C697, 0xA864DB20, 0xA527FDF9, 0xA1E6E04E, 0xBFA1B04B, 0xBB60ADFC, 0xB6238B25, 0xB2E29692,
                0x8AAD2B2F, 0x8E6C3698, 0x832F1041, 0x87EE0DF6, 0x99A95DF3, 0x9D684044, 0x902B669D, 0x94EA7B2A,
                0xE0B41DE7, 0xE4750050, 0xE9362689, 0xEDF73B3E, 0xF3B06B3B, 0xF771768C, 0xFA325055, 0xFEF34DE2,
                0xC6BCF05F, 0xC27DEDE8, 0xCF3ECB31, 0xCBFFD686, 0xD5B88683, 0xD1799B34, 0xDC3ABDED, 0xD8FBA05A,
                0x690CE0EE, 0x6DCDFD59, 0x608EDB80, 0x644FC637, 0x7A089632, 0x7EC98B85, 0x738AAD5C, 0x774BB0EB,
                0x4F040D56, 0x4BC510E1, 0x46863638, 0x42472B8F, 0x5C007B8A, 0x58C1663D, 0x558240E4, 0x51435D53,
                0x251D3B9E, 0x21DC2629, 0x2C9F00F0, 0x285E1D47, 0x36194D42, 0x32D850F5, 0x3F9B762C, 0x3B5A6B9B,
                0x0315D626, 0x07D4CB91, 0x0A97ED48, 0x0E56F0FF, 0x1011A0FA, 0x14D0BD4D, 0x19939B94, 0x1D528623,
                0xF12F560E, 0xF5EE4BB9, 0xF8AD6D60, 0xFC6C70D7, 0xE22B20D2, 0xE6EA3D65, 0xEBA91BBC, 0xEF68060B,
                0xD727BBB6, 0xD3E6A601, 0xDEA580D8, 0xDA649D6F, 0xC423CD6A, 0xC0E2D0DD, 0xCDA1F604, 0xC960EBB3,
                0xBD3E8D7E, 0xB9FF90C9, 0xB4BCB610, 0xB07DABA7, 0xAE3AFBA2, 0xAAFBE615, 0xA7B8C0CC, 0xA379DD7B,
                0x9B3660C6, 0x9FF77D71, 0x92B45BA8, 0x9675461F, 0x8832161A, 0x8CF30BAD, 0x81B02D74, 0x857130C3,
                0x5D8A9099, 0x594B8D2E, 0x5408ABF7, 0x50C9B640, 0x4E8EE645, 0x4A4FFBF2, 0x470CDD2B, 0x43CDC09C,
                0x7B827D21, 0x7F436096, 0x7200464F, 0x76C15BF8, 0x68860BFD, 0x6C47164A, 0x61043093, 0x65C52D24,
                0x119B4BE9, 0x155A565E, 0x18197087, 0x1CD86D30, 0x029F3D35, 0x065E2082, 0x0B1D065B, 0x0FDC1BEC,
                0x3793A651, 0x3352BBE6, 0x3E119D3F, 0x3AD08088, 0x2497D08D, 0x2056CD3A, 0x2D15EBE3, 0x29D4F654,
                0xC5A92679, 0xC1683BCE, 0xCC2B1D17, 0xC8EA00A0, 0xD6AD50A5, 0xD26C4D12, 0xDF2F6BCB, 0xDBEE767C,
                0xE3A1CBC1, 0xE760D676, 0xEA23F0AF, 0xEEE2ED18, 0xF0A5BD1D, 0xF464A0AA, 0xF9278673, 0xFDE69BC4,
                0x89B8FD09, 0x8D79E0BE, 0x803AC667, 0x84FBDBD0, 0x9ABC8BD5, 0x9E7D9662, 0x933EB0BB, 0x97FFAD0C,
                0xAFB010B1, 0xAB710D06, 0xA6322BDF, 0xA2F33668, 0xBCB4666D, 0xB8757BDA, 0xB5365D03, 0xB1F740B4
            };

            // Return the CRC of a block of nLength bytes in rgBlock starting with the byte at the given offset
            // and the starting crc.
            static UInt32 SUPPORT_ComputeCRC(byte[] rgBlock, int offset, int nLength, UInt32 crc)
            {
                int index = 0;

                for (index = 0; index < nLength; index++)
                {
                    crc = c_CRCTable[((crc >> 24) ^ rgBlock[index + offset]) & 0xFF] ^ (crc << 8);
                }
                return crc;
            }

            // Read a little-endian UInt32 from the byte data at the given offset
            static UInt32 ReadUINT32(byte[] data, int offset)
            {
                UInt32 dWord;

                dWord = (UInt32)data[offset] | ((UInt32)data[offset + 1] << 8)
                    | ((UInt32)data[offset + 2] << 16) | ((UInt32)data[offset + 3] << 24);
                return dWord;
            }

            // Read a little endian UInt16 from the byte data at the given offset
            static UInt16 ReadUINT16(byte[] data, int offset)
            {
                UInt16 word;

                word = (UInt16)((UInt16)data[offset] | ((UInt16)data[offset + 1] << 8));
                return word;
            }

            // Write the given UInt32 value into the data byte array at the given offset
            // in little-endian format
            static void WriteUINT32(byte[] data, int offset, UInt32 value)
            {
                data[offset] = (byte)value;
                data[offset + 1] = (byte)(value >> 8);
                data[offset + 2] = (byte)(value >> 16);
                data[offset + 3] = (byte)(value >> 24);
            }

            // Write the given UInt16 value into the data byte array at the given offset
            // in little-endian format
            static void WriteUINT16(byte[] data, int offset, UInt16 value)
            {
                data[offset] = (byte)value;
                data[offset + 1] = (byte)(value >> 8);
            }

            // Check the given UsbStream for a valid WireProtocol packet (header).  Returns true
            // along with the WP_Packet (header) if available.  Returns false if nothing valid received as yet.
            public static bool ReadPacket(UsbStream stream, out WP_Packet packet)
            {
                packet = new WP_Packet();
                byte[] buffer = new byte[WP_Packet.packetSize];       // Size of marshalled WP_Packet
                int nRead;

                // Read in enough data for a complete WP_Packet (header)
                nRead = stream.Read(buffer, 0, WP_Packet.packetSize);

                // If there were not enough bytes to fill the complete WP_Packet, it's no good
                if (nRead != WP_Packet.packetSize)
                {
                    return false;
                }

                // Unmarshal the signature string from the buffer to the WP_Packet
                packet.m_signature = "";
                for (int i = 0; (i < WP_Packet.m_crcHeader_offset) && (buffer[i] != 0); i++)
                {
                    packet.m_signature += (char)buffer[i];
                }

                // Unmarshal the rest of the buffer into the WP_Packet structure.
                packet.m_crcHeader = ReadUINT32(buffer, WP_Packet.m_crcHeader_offset);
                packet.m_crcData = ReadUINT32(buffer, WP_Packet.m_crcData_offset);
                packet.m_seq = ReadUINT16(buffer, WP_Packet.m_seq_offset);
                packet.m_seqReply = ReadUINT16(buffer, WP_Packet.m_seqReply_offset);
                packet.m_flags = ReadUINT32(buffer, WP_Packet.m_flags_offset);
                packet.m_size = ReadUINT32(buffer, WP_Packet.m_size_offset);
                packet.m_cmd = (Commands)ReadUINT32(buffer, WP_Packet.m_cmd_offset);

                // The header CRC must first be zeroed to calculate the correct header CRC
                WriteUINT32(buffer, WP_Packet.m_crcHeader_offset, 0);

                // If the calculated CRC does not match that of the packet, it is no good
                if (packet.m_crcHeader != SUPPORT_ComputeCRC(buffer, 0, WP_Packet.packetSize, 0))
                {
                    return false;
                }

                return true;
            }

            // Sends the given WP_Packet (header) and 
            public static void SendPacket(UsbStream stream, WP_Packet packet, byte[] data)
            {
                int index;
                // Create a buffer whose size is large enough to contain both the WP_Packet (header)
                // and data so that they may be sent as a single packet.
                byte[] buffer = new byte[WP_Packet.packetSize + data.Length];

                // Marshal the signature string into the buffer
                for (index = 0; index < packet.m_signature.Length && index < WP_Packet.m_crcHeader_offset; index++)
                {
                    buffer[index] = (byte)packet.m_signature[index];
                }
                while (index++ < WP_Packet.m_crcHeader_offset)
                    buffer[index] = 0;      // Fill to end with zeros

                // Marshal the WP_Packet (header) into the buffer.  Note that the CRC values start as zero
                // so that the CRC of the WP_Packet (header) may be computed the same for host and target.
                WriteUINT32(buffer, WP_Packet.m_crcHeader_offset, 0);
                WriteUINT32(buffer, WP_Packet.m_crcData_offset, 0);
                WriteUINT32(buffer, WP_Packet.m_cmd_offset, (UInt32)packet.m_cmd);
                WriteUINT16(buffer, WP_Packet.m_seq_offset, packet.m_seq);
                WriteUINT16(buffer, WP_Packet.m_seqReply_offset, packet.m_seqReply);
                WriteUINT32(buffer, WP_Packet.m_flags_offset, packet.m_flags);
                WriteUINT32(buffer, WP_Packet.m_size_offset, (UInt32)data.Length);

                // Copy the data to the buffer
                for (int i = 0; i < data.Length; i++)
                    buffer[WP_Packet.packetSize + i] = data[i];

                // Calculate the CRC of the data
                if (data.Length != 0)
                {
                    packet.m_crcData = SUPPORT_ComputeCRC(data, 0, data.Length, 0);
                    WriteUINT32(buffer, WP_Packet.m_crcData_offset, packet.m_crcData);
                }

                // Calculate the CRC of the packet header
                packet.m_crcHeader = SUPPORT_ComputeCRC(buffer, 0, WP_Packet.packetSize, 0);
                WriteUINT32(buffer, WP_Packet.m_crcHeader_offset, packet.m_crcHeader);

                // Write out the complete packet to the USB stream
                stream.Write(buffer, 0, buffer.Length);
            }
        }
    }
}
