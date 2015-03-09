using System;
using System.Collections;
using System.Threading;
using System.IO;
using System.Runtime.CompilerServices;

namespace Microsoft.SPOT.Hardware.UsbClient
{
    public sealed class UsbStream : System.IO.Stream
    {
        public const int NullEndpoint = 0xFF;

        //--//

        private bool m_disposed;
        private readonly int m_streamIndex;
        private readonly int m_controllerIndex;

        //--//

        public readonly int WriteEndpoint;
        public readonly int ReadEndpoint;

        //--//

        internal UsbStream(int controllerIndex, int writeEndpoint, int readEndpoint)
        {
            m_controllerIndex = controllerIndex;
            WriteEndpoint = writeEndpoint;
            ReadEndpoint = readEndpoint;

            m_streamIndex = nativeOpen(writeEndpoint, readEndpoint);

            m_disposed = false;
        }

        //--//

        ~UsbStream()
        {
            Dispose(false);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (m_disposed) throw new ObjectDisposedException();

            if (!CanRead) throw new InvalidOperationException();

            return nativeRead(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (m_disposed) throw new ObjectDisposedException();

            if (!CanWrite) throw new InvalidOperationException();

            nativeWrite(buffer, offset, count);
        }

        public override void Flush()
        {
            if (m_disposed) throw new ObjectDisposedException();

            nativeFlush();
        }

        public override bool CanRead
        {
            get
            {
                return ReadEndpoint != NullEndpoint;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return WriteEndpoint != NullEndpoint;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        //--//

        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    nativeClose();
                }
                finally
                {
                    ArrayList streams = UsbController.OpenStreams(m_controllerIndex);

                    streams.Remove(this);

                    // if this controller has no open streams, then stop it
                    if (streams.Count == 0)
                    {
                        UsbController.GetController(m_controllerIndex).Stop();
                    }

                    m_disposed = true;
                }
            }
        }

        //--//

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern int nativeOpen(int writeEndpoint, int readEndpoint);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern void nativeClose();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern int nativeRead(byte[] buffer, int offset, int count);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern int nativeWrite(byte[] buffer, int offset, int count);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern void nativeFlush();
    }

    public class Configuration
    {
        public abstract class Descriptor
        {
            protected Descriptor(byte Index)
            {
                index = Index;
            }

            protected byte index;
        }

        public class DeviceDescriptor : Descriptor
        {
            public DeviceDescriptor(ushort Vendor, ushort Product, ushort DeviceVersion)
                : base(0)
            {
                idVendor        = Vendor;
                idProduct       = Product;
                bcdDevice       = DeviceVersion;
                iManufacturer   = 0;        // Default to no Manufacturer string
                iProduct        = 0;        // Default to no Product string
                iSerialNumber   = 0;        // Default to no Serial Number string
                bDeviceClass    = 0;        // Default to no Device Class
                bDeviceSubClass = 0;        // Default to no Device Sub Class
                bDeviceProtocol = 0;        // Default to no Device Protocol
                bMaxPacketSize0 = 8;        // Default to maximum control packet size of 8
                bcdUSB          = 0x0200;   // Default to USB version 2.00
            }

            public ushort idVendor;
            public ushort idProduct;
            public ushort bcdDevice;
            public byte iManufacturer;
            public byte iProduct;
            public byte iSerialNumber;
            public byte bDeviceClass;
            public byte bDeviceSubClass;
            public byte bDeviceProtocol;
            public byte bMaxPacketSize0;
            public ushort bcdUSB;
        }

        public class ClassDescriptor
        {
            public ClassDescriptor(byte DescriptorType, byte[] Payload)
            {
                bDescriptorType = DescriptorType;
                payload = Payload;
            }

            public byte bDescriptorType;
            private byte[] payload;
        }

        public class Endpoint
        {
            public const byte ATTRIB_Read = 0;
            public const byte ATTRIB_Write = 0x80;
            public const byte ATTRIB_Isochronous = 0x01;
            public const byte ATTRIB_Bulk = 0x02;
            public const byte ATTRIB_Interrupt = 0x03;
            public const byte ATTRIB_NoSynch = 0;
            public const byte ATTRIB_Asynch = 0x04;
            public const byte ATTRIB_Adaptive = 0x08;
            public const byte ATTRIB_Synchronous = 0x0C;
            public const byte ATTRIB_Data = 0;
            public const byte ATTRIB_Feedback = 0x10;
            public const byte ATTRIB_Implicit = 0x20;

            public Endpoint(byte EndpointAddress, byte Attributes)
            {
                bEndpointAddress = EndpointAddress;
                bmAttributes = Attributes;
                wMaxPacketSize = 64;                  // Default to 64 byte packet size
                bInterval = 0;                   // Default to no interval
            }

            public byte bEndpointAddress;
            public byte bmAttributes;
            public ushort wMaxPacketSize;
            public byte bInterval;
        }

        public class UsbInterface
        {
            public UsbInterface(byte InterfaceNumber, Endpoint[] Endpoints)
            {
                bInterfaceNumber = InterfaceNumber;
                endpoints = Endpoints;
                bInterfaceClass = 0xFF;      // Defaults to Vendor class
                bInterfaceSubClass = 1;         // Defaults to Sub Class #1
                bInterfaceProtocol = 1;         // Defaults to Protocol #1
                iInterface = 0;         // Defaults to no Interface string
            }

            public byte bInterfaceNumber;
            public Endpoint[] endpoints;
            public ClassDescriptor[] classDescriptors;
            public byte bInterfaceClass;
            public byte bInterfaceSubClass;
            public byte bInterfaceProtocol;
            public byte iInterface;
        }

        public class ConfigurationDescriptor : Descriptor
        {
            public const byte ATTRIB_Base = 0x80;
            public const byte ATTRIB_SelfPowered = 0x40;
            public const byte ATTRIB_RemoteWakeup = 0x20;

            private const ushort PowerFactor = 2;

            public ConfigurationDescriptor(ushort MaxPower_mA, UsbInterface[] Interfaces)
                : base(0)
            {
                bMaxPower = (byte)(MaxPower_mA / PowerFactor);
                interfaces = Interfaces;
                iConfiguration = 0;             // Default to no Configuration string
                bmAttributes = ATTRIB_Base;   // Default to no attributes
            }

            public UsbInterface[] interfaces;
            public byte iConfiguration;
            public byte bmAttributes;
            public byte bMaxPower;
        }  // End of ConfigurationDescriptor class

        public class StringDescriptor : Descriptor
        {
            public StringDescriptor(byte index, string theString)
                : base(index)
            {
                sString = theString;
            }

            public byte bIndex
            {
                get
                {
                    return index;
                }
            }

            public string sString;
        }

        public class GenericDescriptor : Descriptor
        {
            public const byte REQUEST_OUT = 0;
            public const byte REQUEST_IN = 0x80;
            public const byte REQUEST_Standard = 0;
            public const byte REQUEST_Class = 0x20;
            public const byte REQUEST_Vendor = 0x40;
            public const byte REQUEST_Device = 0;
            public const byte REQUEST_Interface = 0x01;
            public const byte REQUEST_Endpoint = 0x02;
            public const byte REQUEST_Other = 0x03;

            private const byte REQUEST_GET_DESCRIPTOR = 0x06;

            public GenericDescriptor(byte RequestType, ushort Value, byte[] Payload)
                : base(0)
            {
                bmRequestType = (byte)(RequestType | REQUEST_IN);       // The Generic Descriptor only supports "Get" type requests by default
                bRequest = REQUEST_GET_DESCRIPTOR;                 // Default to request for descriptor
                wValue = Value;
                wIndex = 0;                                      // Default to a zero index
                payload = Payload;
            }

            public byte bmRequestType;
            public byte bRequest;
            public ushort wValue;
            public ushort wIndex;
            public byte[] payload;
        }

        public Descriptor[] descriptors;
    }

    public sealed class UsbController
    {
        private readonly int m_controllerIndex;
        private ConfigError m_configError;

        //--//

        private static UsbController[] s_controllers;
        private static ArrayList[] s_openStreams;

        //--//

        public enum PortState
        {
            Detached,
            Attached,
            Powered,
            Default,
            Address,
            Running,
            Suspended,
            Stopped = 0xFF
        }

        public enum ConfigError
        {
            ConfigOK,               // Configuration is OK
            MissingRecord,          // Missing Device or Configuration Descriptor record
            ExtraDevice,            // More than one Device descriptor record found
            DeviceSize,             // Device descriptor length or header size incorrect
            DeviceType,             // Device descriptor type mismatch
            Ep0Size,                // Device descriptor has wrong max size for endpoint 0
            NumConfigs,             // More than one configuration is defined
            ExtraConfig,            // More than one configuration descriptor header found
            ConfigSize,             // Record size does not match full configuration descriptor size or config descriptor size is wrong
            ConfigType,             // Configuration descriptor type mismatch
            NoInterface,            // A configuration was defined with no interfaces
            ConfigNum,              // Configuration number was larger than 1
            ConfigAttr,             // Configuration attributes violate USB specification
            InterfaceLen,           // Length of an interface descriptor is wrong
            InterfaceType,          // Expected an interface descriptor, but found something else
            InterfaceAlt,           // Alternate interfaces are not allowed
            NoEndpoint,             // An interface was defined without endpoints
            EndpointLength,         // Length of an endpoint descriptor is wrong
            EndpointType,           // Expected an endpoint descriptor, but found something else
            EndpointAttr,           // Endpoint attributes voilate USB specification
            StringSize,             // String descriptor length and header size does not match
            StringType,             // String descriptor type mismatch
            GenericDir,             // Generic descriptor data direction (bmRequestType) can only be IN
            UnknownRecord,          // Unknown record type found in configuration descriptor list
            EndpointRange,          // Endpoint number either zero or too large
            DuplicateEndpoint,      // Same endpoint number is used twice in the configuration list
            DuplicateInterface,     // Same interface number is used twice in the configuration list
            TooManyInterfaces,      // More than 10 interfaces were defined in the configuration list (isn't that a little excessive?)
            NoSuchController,       // No such controller in this device
            ControllerStarted       // An attempt was made to change the configuration of an active Controller
        }

        static UsbController()
        {
            s_openStreams = new ArrayList[Count];
        }

        internal static ArrayList OpenStreams(int i)
        {
            return s_openStreams[i];
        }

        public ConfigError ConfigurationError
        {
            get
            {
                return m_configError;
            }
        }

        public bool Start()
        {
            return nativeStart();
        }

        public bool Stop()
        {
            bool ret = false;

            // Dispose all open streams and clear the list associated with this controller

            try
            {
                ArrayList streams = s_openStreams[m_controllerIndex];

                if (streams != null)
                {
                    while (streams.Count != 0)
                    {
                        UsbStream stream = (UsbStream)streams[0];

                        try
                        {
                            stream.Close();
                        }
                        catch // swallow any exception
                        {
                        }
                        finally
                        {
                            // the stream should already be gone from the array, but just in case...
                            streams.Remove(stream);
                        }
                    }
                }
            }
            finally
            {
                ret = nativeStop();
            }

            return ret;
        }

        public UsbStream CreateUsbStream(int writeEndpoint, int readEndpoint)
        {
            UsbStream s = null;

            lock (s_openStreams)
            {
                if (s_openStreams[m_controllerIndex] == null)
                {
                    s_openStreams[m_controllerIndex] = new ArrayList();
                }

                ArrayList streams = (ArrayList)s_openStreams[m_controllerIndex];

                // if this is the first UsbStream associated with this controller, then start the controller
                if (streams.Count == 0)
                {
                    // UsbController.GetController(m_controllerIndex).Start();
                    Start();
                }

                try
                {
                    s = new UsbStream(m_controllerIndex, writeEndpoint, readEndpoint);
                }
                catch (Exception e)
                {
                    Stop();
                    throw e;
                }

                streams.Add(s);
            }

            return s;
        }

        private UsbController(int controller)
        {
            m_controllerIndex = controller;
            m_configError = ConfigError.ConfigOK;
        }

        public static UsbController[] GetControllers()
        {
            if (s_controllers == null)
            {
                int numControllers = Count;

                UsbController[] controllers = new UsbController[numControllers];

                for (int i = 0; i < numControllers; i++)
                {
                    controllers[i] = new UsbController(i);
                }

                s_controllers = controllers;
            }

            return s_controllers;
        }

        public static UsbController GetController(int i)
        {
            UsbController[] controllers = GetControllers();

            return controllers[i];
        }

        //--//

        private static extern int Count
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        public extern PortState Status
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        public extern Configuration Configuration
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            set;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern bool nativeStart();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern bool nativeStop();
    }
}


