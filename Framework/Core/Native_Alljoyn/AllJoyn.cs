using System;
using System.Text;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;          // for Cpu.Pin type
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;


namespace Microsoft.SPOT.AllJoyn
{
    public enum AJ_Status
    {
        AJ_OK = 0,                          // Success status
        AJ_ERR_NULL = 1,                    // Unexpected NULL pointer
        AJ_ERR_UNEXPECTED = 2,              // An operation was unexpected at this time
        AJ_ERR_INVALID = 3,                 // A value was invalid
        AJ_ERR_IO_BUFFER = 4,               // An I/O buffer was invalid or in the wrong state
        AJ_ERR_READ = 5,                    // An error while reading data from the network
        AJ_ERR_WRITE = 6,                   // An error while writing data to the network
        AJ_ERR_TIMEOUT = 7,                 // A timeout occurred
        AJ_ERR_MARSHAL = 8,                 // Marshaling failed due to badly constructed message argument
        AJ_ERR_UNMARSHAL = 9,               // Unmarshaling failed due to a corrupt or invalid message
        AJ_ERR_END_OF_DATA = 10,            // Not enough data
        AJ_ERR_RESOURCES = 11,              // Insufficient memory to perform the operation
        AJ_ERR_NO_MORE = 12,                // Attempt to unmarshal off the end of an array
        AJ_ERR_SECURITY = 13,               // Authentication or decryption failed
        AJ_ERR_CONNECT = 14,                // Network connect failed
        AJ_ERR_UNKNOWN = 15,                // A unknown value
        AJ_ERR_NO_MATCH = 16,               // Something didn't match
        AJ_ERR_SIGNATURE = 17,              // Signature is not what was expected
        AJ_ERR_DISALLOWED = 18,             // An operation was not allowed
        AJ_ERR_FAILURE = 19,                // A failure has occurred
        AJ_ERR_RESTART = 20,                // The OEM event loop must restart
        AJ_ERR_LINK_TIMEOUT = 21,           // The bus link is inactive too long
        AJ_ERR_DRIVER = 22,                 // An error communicating with a lower-layer driver
        AJ_ERR_OBJECT_PATH = 23,            // Object path was not specified
        AJ_ERR_BUSY = 24,                   // An operation failed and should be retried later
        AJ_ERR_DHCP = 25,                   // A DHCP operation has failed
        AJ_ERR_ACCESS = 26,                 // The operation specified is not allowed
        AJ_ERR_SESSION_LOST = 27,           // The session was lost
        AJ_ERR_LINK_DEAD = 28,              // The network link is now dead
        AJ_ERR_HDR_CORRUPT = 29,            // The message header was corrupt
        AJ_ERR_RESTART_APP = 30,            // The application must cleanup and restart
        AJ_ERR_INTERRUPTED = 31,            // An I/O operation (READ) was interrupted
        AJ_ERR_REJECTED = 32,               // The connection was rejected
        AJ_ERR_RANGE = 33,                  // Value provided was out of range
        AJ_ERR_ACCESS_ROUTING_NODE = 34,    // Access defined by routing node
        AJ_ERR_KEY_EXPIRED = 35,            // The key has expired
        AJ_ERR_SPI_NO_SPACE = 36,           // Out of space error
        AJ_ERR_SPI_READ = 37,               // Read error
        AJ_ERR_SPI_WRITE = 38,              // Write error
        AJ_ERR_OLD_VERSION = 39,            // Router you connected to is old and unsupported
        AJ_ERR_NVRAM_READ = 40,             // Error while reading from NVRAM
        AJ_ERR_NVRAM_WRITE = 41,            // Error while writing to NVRAM
        AJ_STATUS_LAST = 41                 // The last error status code
    }

    public enum AJ_Args
    {
        AJ_ARG_INVALID   =        '\0',   // AllJoyn invalid type 
        AJ_ARG_ARRAY     =        'a',    // AllJoyn array container type 
        AJ_ARG_BOOLEAN   =        'b',    // AllJoyn boolean basic type 
        AJ_ARG_DOUBLE    =        'd',    // AllJoyn IEEE 754 double basic type 
        AJ_ARG_SIGNATURE =        'g',    // AllJoyn signature basic type 
        AJ_ARG_HANDLE    =        'h',    // AllJoyn socket handle basic type 
        AJ_ARG_INT32     =        'i',    // AllJoyn 32-bit signed integer basic type 
        AJ_ARG_INT16     =        'n',    // AllJoyn 16-bit signed integer basic type 
        AJ_ARG_OBJ_PATH  =        'o',    // AllJoyn Name of an AllJoyn object instance basic type 
        AJ_ARG_UINT16    =        'q',    // AllJoyn 16-bit unsigned integer basic type 
        AJ_ARG_STRING    =        's',    // AllJoyn UTF-8 NULL terminated string basic type 
        AJ_ARG_UINT64    =        't',    // AllJoyn 64-bit unsigned integer basic type 
        AJ_ARG_UINT32    =        'u',    // AllJoyn 32-bit unsigned integer basic type 
        AJ_ARG_VARIANT   =        'v',    // AllJoyn variant container type 
        AJ_ARG_INT64     =        'x',    // AllJoyn 64-bit signed integer basic type 
        AJ_ARG_BYTE      =        'y',    // AllJoyn 8-bit unsigned integer basic type
        AJ_ARG_STRUCT    =        '(',    // AllJoyn struct container type
        AJ_ARG_DICT_ENTRY=        '{'     // AllJoyn dictionary or map container type - an array of key-value pairs
    }
    
    public class AJ_IOBuffer {
        byte direction;     // I/O buffer is either a Tx buffer or an Rx buffer
        byte flags;         // ports to send to or receive on
        UInt16 bufSize;     // Size of the data buffer
        IntPtr bufStart;    // Start for the data buffer
        IntPtr readPtr;     // Current position in buf for reading data
        IntPtr writePtr;    // Current position in buf for writing data
        IntPtr TxRxFunc;    // function pointer union, either AJ_TxFunc or AJ_RxFunc    
        IntPtr context;     // Abstracted context for managing I/O
    };
    
    public class AJ_NetSocket {
        AJ_IOBuffer tx;             // transmit network socket
        AJ_IOBuffer rx;             // receive network socket
    };
    
    public class AJ_BusAttachment {
        public UInt16 aboutPort;                // The port to use in announcements
        byte [] uniqueName = new byte[16];      // The unique name returned by the hello message
        AJ_NetSocket sock;                      // Abstracts a network socket
        UInt32 serial;                          // Next outgoing message serial number
        IntPtr pwdCallback;                     // Callback for obtaining passwords
        IntPtr authListenerCallback;            // Callback for obtaining passwords
        IntPtr suites;                          // Supported cipher suites
        UInt32 numsuites;                       // Number of supported cipher suites
    };
       
    public class AJ_Message {
        public UInt32 msgId;            // Identifies the message to the application
        IntPtr hdr;                     // The message header
        UInt32 union0;                  // either null terminated object path or NULL -or- reply serial number
        IntPtr union1;                  // either null terminated member name string or NULL -or-
                                        // null terminated error name string or NULL        
        IntPtr iface;                   // The nul terminated interface string or NULL
        IntPtr sender;                  // The nul terminated sender string or NULL
        IntPtr destination;             // The nul terminated destination string or NULL
        IntPtr signature;               // The nul terminated signature string or NULL 
        UInt32 sessionId;               // Session id
        UInt32 timestamp;               // Timestamp
        UInt32 ttl;                     // Time to live

        // Private message state - the application should not touch this data

        byte sigOffset;                 // Offset to current position in the signature
        byte varOffset;                 // For variant marshalling/unmarshalling - Offset to start of variant signature
        UInt16 bodyBytes;               // Running count of the number body bytes written
        IntPtr bus;                     // Bus attachment for this message
        IntPtr outer;                   // Container arg current being marshaled
    };

    public class AJ_MsgHeader {
        sbyte endianess;        // The endianness of this message
        byte msgType;           // Indicates if the message is method call, signal, etc.
        byte flags;             // Flag bits
        byte majorVersion;      // Major version of this message
        UInt32 bodyLen;         // Length of the body data
        UInt32 serialNum;       // serial of this message
        UInt32 headerLen;       // Length of the header data
    };
    
    public struct AJ_Arg {

        byte typeId;       // the argument type
        byte flags;        // non-zero if the value is a variant - values > 1 indicate variant-of-variant etc.
        ushort len;        // length of a string or array in bytes
        IntPtr val;        // pointer to data val union
        IntPtr sigPtr;     // pointer to the signature
        IntPtr container;  // pointer to container
    };

    public class AJ_SessionOpts
    {
        public byte traffic;                // traffic type
        public byte proximity;              // proximity
        public UInt16 transports;           // allowed transports
        public UInt32 isMultipoint;         // multi-point session capable
    };
    
    [StructLayout(LayoutKind.Sequential)]
    public struct AJ_Object
    {
        public string path;                                    // object path
        public string[] interfaces;                            // interface descriptor
        public byte flags;                                     // flags for the object
        public IntPtr context;                                 // an application provided context pointer for this object
    }        
    
    // callback supplied by user to process get/set property requests    
    public delegate AJ_Status PropertyCB(AJ_Message reply, AJ_Message msg, uint propId, AJ aj);
    
    public partial class AJ
    {        
        public const sbyte AJ_FALSE = 0;
        public const sbyte AJ_TRUE = 1;

        public const uint AJ_PROP_GET = 0;
        public const uint AJ_PROP_SET = 1;

        public const uint AJ_NAME_REQ_DO_NOT_QUEUE  = 4;
        
        public const uint AJ_BUS_ID_FLAG = 0;                 

        public const uint ALLJOYN_FLAG_SESSIONLESS = 0x10;
        
        public const uint AJ_SIGNAL_ABOUT_ANNOUNCE                  = (((uint)(AJ_BUS_ID_FLAG) << 24) | (((uint)(5)) << 16) | (((uint)(1)) << 8) | (3));
        
        public const uint AJ_METHOD_ABOUT_GET_PROP                  = (((uint)(AJ_BUS_ID_FLAG) << 24) | (((uint)(5)) << 16) | (((uint)(0)) << 8) | (AJ_PROP_GET));
        public const uint AJ_METHOD_ABOUT_GET_ABOUT_DATA            = (((uint)(AJ_BUS_ID_FLAG) << 24) | (((uint)(5)) << 16) | (((uint)(1)) << 8) | (1));
        public const uint AJ_METHOD_ABOUT_ICON_GET_PROP             = (((uint)(AJ_BUS_ID_FLAG) << 24) | (((uint)(6)) << 16) | (((uint)(0)) << 8) | (1));        
        public const uint AJ_METHOD_ABOUT_GET_OBJECT_DESCRIPTION    = (((uint)(AJ_BUS_ID_FLAG) << 24) | (((uint)(5)) << 16) | (((uint)(1)) << 8) | (2));
        public const uint AJ_METHOD_ABOUT_ICON_GET_URL              = (((uint)(AJ_BUS_ID_FLAG) << 24) | (((uint)(6)) << 16) | (((uint)(1)) << 8) | (3));
        public const uint AJ_METHOD_ABOUT_ICON_GET_CONTENT          = (((uint)(AJ_BUS_ID_FLAG) << 24) | (((uint)(6)) << 16) | (((uint)(1)) << 8) | (4));
        
        public const uint AJ_PROPERTY_ABOUT_VERSION             = (((uint)(AJ_BUS_ID_FLAG) << 24) | (((uint)(5)) << 16) | (((uint)(1)) << 8) | (0));
        public const uint AJ_PROPERTY_ABOUT_ICON_VERSION_PROP   = (((uint)(AJ_BUS_ID_FLAG) << 24) | (((uint)(6)) << 16) | (((uint)(1)) << 8) | (0));
        public const uint AJ_PROPERTY_ABOUT_ICON_MIMETYPE_PROP  = (((uint)(AJ_BUS_ID_FLAG) << 24) | (((uint)(6)) << 16) | (((uint)(1)) << 8) | (1));
        public const uint AJ_PROPERTY_ABOUT_ICON_SIZE_PROP      = (((uint)(AJ_BUS_ID_FLAG) << 24) | (((uint)(6)) << 16) | (((uint)(1)) << 8) | (2));

        public const uint ABOUT_VERSION = 1;
        public const uint ABOUT_ICON_VERSION = 1;
                
        public string       AboutIconMime = "image/png";
        public uint         AboutIconSize = 0;
        public byte []      AboutIconContent = new byte[0];
        public string       AboutIconURL = String.Empty;                                
                
        public const uint AJ_REP_ID_FLAG = 0x80;  /**< Indicates a message is a reply message */                          
                
        public const string AJ_RELEASE_YEAR = "2015";
        public const string AJ_RELEASE_MONTH = "4";
        public const string AJ_FEATURE_VERSION = "1";
        public const string AJ_BUGFIX_VERSION = "0";
        public const string AJ_RELEASE_TAG = "rtm";              
        public const string APP_ID_SIGNATURE = "ay";
                
        
        
        public static string GetGUID()
        {
            string guid = String.Empty;
            
            byte [] ba = new byte[16];                                    
            AJ_Status status = GetLocalGUID(ba);
            if (status == AJ_Status.AJ_OK)
            {
                guid = BitConverter.ToString(ba);
                
                StringBuilder aStr = new StringBuilder(guid);
                for (int i = 0; i < aStr.Length; i++)
                {
                    if (aStr[i] == '-')
                    {
                        aStr.Remove(i, 1);
                    }
                }
                
                guid = aStr.ToString();
            }
            
            return guid;
        }
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern AJ_Status GetLocalGUID(byte [] b);                       
                
        public void SetAboutIconContent(byte [] b)
        {
            AboutIconContent = b;
        }
        
        public void SetAboutIconURL(string URL)
        {
            AboutIconURL = URL;
        }                
                
        public string GetVersion()
        {
            string version = AJ_RELEASE_YEAR + "." + AJ_RELEASE_MONTH + "." + AJ_FEATURE_VERSION + AJ_BUGFIX_VERSION + " Tag " + AJ_RELEASE_TAG;
            return version;
        }        
                
        private static string ConvertStringArrayToString(string[] array)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string value in array)
            {
                sb.Append(value);
                sb.Append(',');
            }
            return sb.ToString();
        }
        
        public void RegisterObjectsInterface(AJ_Object ajObj, bool useProperties, bool local)
        {
            string path = ajObj.path;
            byte flags = ajObj.flags;
            string interfaces = ConvertStringArrayToString(ajObj.interfaces);
            IntPtr context = ajObj.context;

            AJ.RegisterObjects(path, interfaces, flags, context, useProperties, local);
        }
        
        private AJ_Status ProcessProperty(AJ_Message msg, PropertyCB propCB, uint propType)
        {
            AJ_Status status = AJ_Status.AJ_OK;
            UInt32 propId = 0;                

            string sig = UnmarshalPropertyArgs(msg, ref propId);

            AJ_Message reply = new AJ_Message();
            MarshalReplyMsg(msg, reply);                
                            
            if (propType == AJ.AJ_PROP_GET)
            {
                MarshalVariant(reply, sig);    
                if (propCB != null)
                {
                    status = propCB(reply, msg, propId, this);
                }
            }
            else
            {
                string variant = UnmarshalVariant(msg);
                if (0 == String.Compare(variant, sig))
                {
                    if (propCB != null)
                    {
                        status = propCB(reply, msg, propId, this);
                    }
                }
                else
                {
                    status = AJ_Status.AJ_ERR_SIGNATURE;
                }
            }

            if (status != AJ_Status.AJ_OK)
            {
                //AJ_MarshalStatusMsg(msg, &reply, status);
            }

            DeliverMsg(reply);
            
            return status;
        }
                
        public AJ_Status BusHandleBusMessage(AJ_Message msg, UInt32 bus, UInt32 busAboutPort)
        {
            AJ_Status status = AJ_Status.AJ_OK;
            AJ_Message reply = new AJ_Message();
        
            switch(msg.msgId)
            {
                case AJ_METHOD_ABOUT_GET_PROP:
                    return AboutHandleGetProp(msg);

                case AJ_METHOD_ABOUT_GET_ABOUT_DATA:
                    status = AboutHandleGetAboutData(msg, reply);
                    break;
                    
                case AJ_METHOD_ABOUT_ICON_GET_PROP:
                    return AboutIconHandleGetProp(msg);
                    
                case AJ_METHOD_ABOUT_ICON_GET_CONTENT:
                    status = AboutIconHandleGetContent(msg, reply, AboutIconContent);
                    break;    
                    
                case AJ_METHOD_ABOUT_ICON_GET_URL:
                    status = AboutIconHandleGetURL(msg, reply);
                    break;
                    
                default:
                    return BusHandleBusMessageInner(msg);
            }
            
            if ((status == AJ_Status.AJ_OK) /*&& (msg->hdr->msgType == AJ_MSG_METHOD_CALL)*/) {
                status = DeliverMsg(reply);
            }
            if (status == AJ_Status.AJ_OK) {
                AboutAnnounce(bus, busAboutPort);
            }
            return status;
        }        
        
        public AJ_Status BusGetProp(AJ_Message msg, PropertyCB propCB)
        {
            return ProcessProperty(msg, propCB, AJ.AJ_PROP_GET);
        }
        
        public AJ_Status BusSetProp(AJ_Message msg, PropertyCB propCB)
        {
            return ProcessProperty(msg, propCB, AJ.AJ_PROP_SET);
        }
                        
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void RegisterObjects(string path, string interfaceDescription, byte flags, IntPtr context, bool useProperties, bool local);
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern string GetUniqueName(UInt32 bus);                
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void Initialize();                
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void PrintXML(string localPath, string localInterfaceDescription, byte localFlags, IntPtr localContext);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern AJ_Status SetBusLinkTimeout(UInt32 bus, UInt32 timeout);
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void NotifyLinkActive();

        [MethodImpl(MethodImplOptions.InternalCall)]        
        public extern AJ_Status BusLinkStateProc(UInt32 bus);
        
        [MethodImpl(MethodImplOptions.InternalCall)]        
        public extern AJ_Status SetIdleTimeouts(UInt32 bus, UInt32 idleTo, UInt32 probeTo);
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern AJ_Status StartService(UInt32 bus,
                                            string daemonName,
                                            UInt32 timeout,
                                            sbyte connected,
                                            UInt16 port,
                                            string name,
                                            UInt32 flags);
                                                      
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern AJ_Status StartClientByName(UInt32 bus,
                                                  string daemonName,
                                                  UInt32 timeout,
                                                  byte connected,
                                                  string name,
                                                  UInt16 port,
                                                  ref UInt32 sessionId,
                                                  AJ_SessionOpts opts,
                                                  ref string fullName);
                                                      
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void AlwaysPrintf(string msg);                
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern AJ_Status AboutIconHandleGetContent(AJ_Message msg, AJ_Message reply, byte [] aboutIconContent);
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern AJ_Status BusHandleBusMessageInner(AJ_Message msg);                
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern AJ_Status BusCancelSessionless(UInt32 bus, UInt32 serialNum);
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern AJ_Status MarshalReplyMsg(AJ_Message msg, AJ_Message replyMsg);
        
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern AJ_Status MarshalSignal(  UInt32 bus,
                                                AJ_Message msg,
                                                UInt32 msgId,
                                                UInt32 destination,
                                                UInt32 sessionId,
                                                byte flags,
                                                UInt32 ttl)	;
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern AJ_Status MarshalMethodCall( UInt32 bus,
                                                      AJ_Message msg,
                                                      UInt32 msgId,
                                                      string destination,
                                                      UInt32 sessionId,
                                                      byte flags,
                                                      UInt32 timeout);
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern AJ_Status UnmarshalArg(AJ_Message msg, UInt32 argPtr);
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern AJ_Status DeliverMsg(AJ_Message msg);
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void BusSetPasswordCallback();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern AJ_Status UnmarshalMsg(UInt32 bus, AJ_Message msg, UInt32 timeout);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern string UnmarshalVariant(AJ_Message msg);
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern AJ_Status MarshalVariant(AJ_Message msg, string sig);
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern AJ_Status UnmarshalArgs(AJ_Message msg, string sig, UInt16 port, UInt32 sessionId, string joiner);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern AJ_Status UnmarshalArgs(AJ_Message msg, string sig, ref uint arg);
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern string UnmarshalArgs(AJ_Message msg, string sig);
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern AJ_Status UnmarshalArgs(AJ_Message msg, string sig, ref uint arg1, ref uint arg2);
        
        [MethodImpl(MethodImplOptions.InternalCall)]        
        public extern string UnmarshalPropertyArgs(AJ_Message msg, ref UInt32 propId);
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern AJ_Status MarshalArg(AJ_Message msg, string sig, UInt32 val);
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern AJ_Status MarshalArg(AJ_Message msg, string sig, string val);
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern AJ_Status MarshalArgs(AJ_Message msg, string sig, string val1, string val2, string val3);
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern AJ_Status MarshalArgs(AJ_Message msg, string sig, string val1, string val2);
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern AJ_Status MarshalArgs(AJ_Message msg, string sig, string val1, byte[] val2);
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern AJ_Status MarshalContainer(AJ_Message msg, UInt32 argPtr, byte typeId);
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern AJ_Status MarshalCloseContainer(AJ_Message msg, UInt32 argPtr);
                
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern AJ_Status BusReplyAcceptSession(AJ_Message msg, UInt32 accept);        

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void CloseMsg(AJ_Message msg);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void Disconnect(UInt32 bus);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void Sleep(UInt32 time);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern UInt32 AppMessageId(UInt32 p, UInt32 i, UInt32 m);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern UInt32 BusMessageId(UInt32 p, UInt32 i, UInt32 m);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern UInt32 PrxMessageId(UInt32 p, UInt32 i, UInt32 m);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern UInt32 AppPropertyId(UInt32 p, UInt32 i, UInt32 m);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern UInt32 BusPropertyId(UInt32 p, UInt32 i, UInt32 m);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern UInt32 PrxPropertyId(UInt32 p, UInt32 i, UInt32 m);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void CreateBus(ref UInt32 bus);     
    }
}
