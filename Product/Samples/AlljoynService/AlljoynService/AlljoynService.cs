//
// AlljoynService.cs
//
// Sample alljoyn service demonstrating Methods, Properties, Notifications, and About Service
//
//

using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.AllJoyn;
using Microsoft.SPOT.AllJoyn.Services;

namespace TestAlljoyn
{
    public class Program
    {
        // Define some attributes for use with the Alljoyn About Service

        static string   deviceManufactureName   = "Microsoft";
        static string   deviceProductName       = ".NET Micro Framework Toaster Client";
        static string   defaultLanguage         = "en";
        static string[] supportedLanguages      = { defaultLanguage };
        static string[] defaultDeviceNames      = { "NETMF Toaster" };
        static string[] defaultSupportUrls      = { "http://microsoft.com" };
        static string[] defaultDescriptions     = { "The best smart toaster"};
        static string[] defaultManufacturers    = { "Microsoft" };
        static string   defaultModelNumber      = "1.2.3.4";
        static string   defaultSoftwareVersion    = "Beta release";

        // Define the icon to be displayed in About messages

        string aboutIconMimetype = "image/png";
        static byte [] aboutIconContent = new byte[]
            {
            0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a, 0x00, 0x00, 0x00, 0x0d, 0x49, 0x48, 0x44, 0x52 
            , 0x00, 0x00, 0x00, 0x32, 0x00, 0x00, 0x00, 0x32, 0x02, 0x03, 0x00, 0x00, 0x00, 0x63, 0x51, 0x60 
            , 0x22, 0x00, 0x00, 0x00, 0x0c, 0x50, 0x4c, 0x54, 0x45, 0x65, 0x2d, 0x67, 0xeb, 0x00, 0x88, 0xec 
            , 0x06, 0x8d, 0xf1, 0x44, 0xaa, 0x1f, 0x54, 0xd3, 0x5b, 0x00, 0x00, 0x00, 0x01, 0x74, 0x52, 0x4e 
            , 0x53, 0x00, 0x40, 0xe6, 0xd8, 0x66, 0x00, 0x00, 0x00, 0xe8, 0x49, 0x44, 0x41, 0x54, 0x28, 0xcf 
            , 0x95, 0x92, 0x51, 0x0a, 0xc3, 0x20, 0x0c, 0x86, 0xd3, 0xc0, 0x40, 0x84, 0xdd, 0x61, 0xec, 0x49 
            , 0x72, 0x9f, 0xde, 0x47, 0xfa, 0x34, 0x3c, 0x45, 0x18, 0x85, 0x49, 0x4e, 0xb9, 0x18, 0xab, 0xd6 
            , 0xb1, 0x97, 0x06, 0xb4, 0x7e, 0xa6, 0x26, 0xbf, 0x89, 0x00, 0x97, 0xec, 0xb6, 0x9e, 0xc9, 0x8b 
            , 0x0e, 0xee, 0x04, 0x40, 0x92, 0x1b, 0x49, 0x04, 0x7a, 0xcb, 0x01, 0x28, 0x20, 0xc4, 0xd4, 0x7c 
            , 0x0f, 0x90, 0x11, 0x04, 0x39, 0xd0, 0x29, 0x24, 0xd3, 0x39, 0x41, 0x0c, 0x53, 0x3e, 0x4c, 0x1b 
            , 0x4b, 0x4f, 0x87, 0x29, 0x65, 0x49, 0x7b, 0x89, 0x01, 0x64, 0x91, 0x44, 0xf6, 0x2a, 0xc4, 0x26 
            , 0xf1, 0x1f, 0x5d, 0x10, 0xbb, 0xba, 0xe5, 0x77, 0x93, 0x15, 0x4c, 0x40, 0xb5, 0x64, 0xc1, 0x9a 
            , 0x66, 0x37, 0x91, 0x2d, 0x10, 0xda, 0xf5, 0x9e, 0xba, 0xc0, 0xad, 0x39, 0x31, 0xea, 0xc0, 0xfe 
            , 0xab, 0x2b, 0x5b, 0x9d, 0x42, 0x11, 0x3e, 0xd0, 0x68, 0x5c, 0x18, 0x13, 0x74, 0xf2, 0x01, 0x4b 
            , 0x71, 0xea, 0x95, 0x3d, 0x05, 0x56, 0xcc, 0x5a, 0xb9, 0xb2, 0x19, 0x20, 0xfb, 0xa8, 0x5f, 0x3e 
            , 0x0a, 0xcd, 0xc4, 0x07, 0x89, 0xd3, 0x84, 0xcd, 0xb7, 0xa8, 0x8b, 0x4c, 0x4f, 0x39, 0xb7, 0x68 
            , 0xd6, 0x1a, 0xbc, 0xcc, 0xf7, 0x58, 0x7c, 0xad, 0x43, 0x77, 0x8d, 0xf3, 0xd2, 0x72, 0x0c, 0xd2 
            , 0x16, 0x0d, 0x95, 0x34, 0x91, 0xfa, 0x46, 0x67, 0x21, 0x45, 0xcb, 0xd0, 0x1a, 0x56, 0xc7, 0x41 
            , 0x7a, 0xc6, 0xe7, 0x89, 0xe4, 0x3f, 0x81, 0x51, 0xfc, 0x79, 0x3f, 0xc3, 0x96, 0xf5, 0xda, 0x5b 
            , 0x84, 0x2f, 0x85, 0x3b, 0x47, 0x0d, 0xe8, 0x0d, 0xca, 0xd3, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45 
            , 0x4e, 0x44, 0xae, 0x42, 0x60, 0x82, 0x82 };

        int aboutIconContentSize = 0;        
        static string aboutIconUrl = "https://www.alljoyn.org/sites/all/themes/at_alljoyn/images/img-alljoyn-logo.png";

        // Define messages used in the property handlers
        
        static UInt32 GET_DARKNESS = AJ.AppMessageId(0, 0, AJ.AJ_PROP_GET);
        static UInt32 SET_DARKNESS = AJ.AppMessageId(0, 0, AJ.AJ_PROP_SET);
        static UInt32 DARKNESS_PROPERTY_ID = AJ.AppPropertyId(0, 1, 3);

        // Darkness property state

        static uint Darkness = 0;

        // GetPropertyHandler:
        //
        // Callback function used to process get property requests from clients
        //

        public static AJ_Status GetPropertyHandler(AJ_Message reply, AJ_Message msg, uint propId, AJ aj)
        {
            if (propId == DARKNESS_PROPERTY_ID)
            {
                aj.MarshalArg(reply, "u", Darkness);
            }
            
            return AJ_Status.AJ_OK;
        }

        // SetPropertyHandler:
        //
        // Callback function used to process set property requests from clients
        //

        public static AJ_Status SetPropertyHandler(AJ_Message reply, AJ_Message msg, uint propId, AJ aj)
        {
            if (propId == DARKNESS_PROPERTY_ID)
            {
                aj.UnmarshalArgs(msg, "u", ref Darkness);
            }

            return AJ_Status.AJ_OK;
        }

        // program entry point

        public static void Main()
        {
            // Initialize Alljoyn class instance

            AJ myAlljoyn = new AJ();
            var ajServices = new AJ_Services(myAlljoyn);

            // some state variables
            
            bool connected = false;
            AJ_Status status = AJ_Status.AJ_OK;
            
            // constants used in the message processing loop

            UInt32 CONNECT_TIMEOUT = (1000 * 1000);
            UInt32 UNMARSHAL_TIMEOUT = 5000;            

            UInt32 AJ_METHOD_ACCEPT_SESSION = AJ.BusMessageId(2, 0, 0);
            UInt32 AJ_SIGNAL_SESSION_JOINED = AJ.BusMessageId(2, 0, 1);

            UInt32 START_TOASTING = AJ.AppMessageId(0, 1, 0);
            UInt32 STOP_TOASTING = AJ.AppMessageId(0, 1, 1);
            UInt32 TOAST_DONE_SIGNAL = AJ.AppMessageId(0, 1, 2);

            UInt32 DARKNESS = AJ.AppPropertyId(0, 1, 3);

            byte AJ_FLAG_GLOBAL_BROADCAST = 0x20;

            UInt16 port = 0;            
            UInt32 bus = 0;
            AJ_Message msg = new AJ_Message();

            // Define the application that the outside world
            // will see

            string ServiceName = "com.microsoft.sample.toaster";
            string ServicePath = "/toaster";
            UInt16 ServicePort = 42;
            string DaemonServiceName = "org.alljoyn.BusNode.Led";


            // Define the Alljoyn interface that will be
            // published to the outside world

            string[] testInterface = {
                "com.microsoft.sample.toaster",
                "?startToasting",
                "?stopToasting",
                "!toastDone status>i",
                "@Darkness=u",
                " "
            };

            // Create and set the property store

            PropertyStore ps = new PropertyStore();
            ps.SetDefaultDeviceNames(defaultDeviceNames);
            ps.SetSupportedLanguages(supportedLanguages);
            ps.SetDefaultLanguage(defaultLanguage);
            ps.SetDefaultSupportUrls(defaultSupportUrls);
            ps.SetDefaultDeviceNames(defaultDeviceNames);
            ps.SetDefaultDescriptions(defaultDescriptions);
            ps.SetDefaultManufacturers(defaultManufacturers);
            ps.SetDefaultModelNumber(defaultModelNumber);
            ps.SetDefaultSoftwareVersion(defaultSoftwareVersion);
            ps.InitMandatoryFields();
            ajServices.SetPropertyStore(ps);

            // Set About attributes

            myAlljoyn.SetAboutIconContent(aboutIconContent);
            myAlljoyn.SetAboutIconURL(aboutIconUrl); 

            // Create the Alljoyn bus instance

            myAlljoyn.CreateBus(ref bus);

            // Initialize Alljoyn instance

            myAlljoyn.Initialize();

            // Register alljoyn interface. We set the 2nd parameter to true to indicate that
            // properties are going to be used

            myAlljoyn.RegisterObjectsInterface(new AJ_Object() { path = ServicePath, interfaces = testInterface }, true, true);
            
            // Start the message processing loop
            
            while (true)
            {
                if (!connected)
                {
                    IntPtr arg = new IntPtr();
                    status = myAlljoyn.StartService(bus, DaemonServiceName, CONNECT_TIMEOUT, AJ.AJ_FALSE, ServicePort, ServiceName, AJ.AJ_NAME_REQ_DO_NOT_QUEUE);
                    if (status != AJ_Status.AJ_OK)
                    {
                        goto exit;
                    }
                    myAlljoyn.AlwaysPrintf(("StartService returned AJ_OK; running \n"));
                    connected = true;

                    if (status == AJ_Status.AJ_OK)
                    {
                        myAlljoyn.doAnnounce = true;
                        myAlljoyn.AboutAnnounce(bus, ServicePort);
                    }
                }

                status = myAlljoyn.UnmarshalMsg(bus, msg, UNMARSHAL_TIMEOUT);
                if (status != AJ_Status.AJ_OK)
                {
                    if (status == AJ_Status.AJ_ERR_TIMEOUT)
                    {
                        myAlljoyn.AlwaysPrintf(("do work\n"));
                        goto exit;
                    }
                }

                if (status == AJ_Status.AJ_OK)
                {
                    string str = "Received message + msgId=" + msg.msgId.ToString("X") + "\n";
                    myAlljoyn.AlwaysPrintf((str));

                    if (msg.msgId == AJ_METHOD_ACCEPT_SESSION)
                    {
                        string joiner = "";
                        UInt32 sessionId = 0;

                        myAlljoyn.UnmarshalArgs(msg, "qus", port, sessionId, joiner);
                        status = myAlljoyn.BusReplyAcceptSession(msg, 1);

                        if (status == AJ_Status.AJ_OK)
                        {
                            myAlljoyn.AlwaysPrintf(("Accepted session session_id \n"));
                        }
                        else
                        {
                            myAlljoyn.AlwaysPrintf(("AJ_BusReplyAcceptSession: error \n"));
                        }

                    }
                    else if (msg.msgId == AJ_SIGNAL_SESSION_JOINED)
                    {
                        // do nothing...

                    }
                    else if (msg.msgId == START_TOASTING)
                    {
                        AJ_Message reply = new AJ_Message();
                        status = myAlljoyn.MarshalReplyMsg(msg, reply);

                        if (status == AJ_Status.AJ_OK)
                        {
                            status = myAlljoyn.DeliverMsg(reply);
                        }
                    }
                    else if (msg.msgId == STOP_TOASTING)
                    {
                        AJ_Status s = 0;
                        AJ_Message reply = new AJ_Message();
                        s = myAlljoyn.MarshalReplyMsg(msg, reply);

                        if (s == AJ_Status.AJ_OK)
                        {
                            s = myAlljoyn.DeliverMsg(reply);
                        }

                        // send a signal here

                        AJ_Message sig = new AJ_Message();
                        s = myAlljoyn.MarshalSignal(bus, sig, TOAST_DONE_SIGNAL, 0, 0, AJ_FLAG_GLOBAL_BROADCAST, 0);
                        s = myAlljoyn.MarshalArg(sig, "i", (uint)s);
                        myAlljoyn.DeliverMsg(sig);

                    }
                    else if (msg.msgId == GET_DARKNESS)
                    {
                        myAlljoyn.BusGetProp(msg, GetPropertyHandler);
                    }
                    else if (msg.msgId == SET_DARKNESS)
                    {
                        myAlljoyn.BusSetProp(msg, SetPropertyHandler);
                    }
                    else
                    {
                        myAlljoyn.BusHandleBusMessage(msg, bus, ServicePort);
                    }
                }
                myAlljoyn.CloseMsg(msg);

                if (status == AJ_Status.AJ_ERR_READ)
                {
                    myAlljoyn.AlwaysPrintf(("AllJoyn disconnect\n"));
                    myAlljoyn.Disconnect(bus);
                    connected = false;
                    
                    // sleep a little while before trying to connect

                    myAlljoyn.Sleep(10 * 1000);
                }

            exit:
                myAlljoyn.AlwaysPrintf((" Exit  \n"));
            }
        }
    }
}

