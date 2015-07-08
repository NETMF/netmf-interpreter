using System;
using Microsoft.NETMF.Networking;
using Microsoft.SPOT;
using Microsoft.SPOT.AllJoyn;
using Microsoft.SPOT.AllJoyn.Services;

namespace weather
{
    public class AlljoynWeatherStationService
    {
        #region Constant Data
        private const string defaultLanguage = "en";
        static readonly string[ ] supportedLanguages = { defaultLanguage };
        static readonly string[ ] defaultDeviceNames = { "NETMF Weather Station" };
        static readonly string[ ] defaultSupportUrls = { "http://microsoft.com" };
        static readonly string[ ] defaultDescriptions = { "A weather station" };
        static readonly string[ ] defaultManufacturers = { "Microsoft" };
        private const string defaultModelNumber = "1.2.3.4";
        private const string defaultSoftwareVersion = "Beta release";

        const string ServiceName = "com.microsoft.NetMicroFrameWork.WeatherStation";
        const string ServicePath = "/WeatherStation";
        const ushort ServicePort = 42;
        const string DaemonServiceName = null;
        const int CONNECT_TIMEOUT = ( 2000 );
        const uint UNMARSHAL_TIMEOUT = 5000;

        // Define the icon to be displayed in About messages

        static readonly byte[ ] aboutIconContent = new byte[ ]
            { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a, 0x00, 0x00, 0x00, 0x0d, 0x49, 0x48, 0x44, 0x52 
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
            , 0x4e, 0x44, 0xae, 0x42, 0x60, 0x82, 0x82
            };

        private const string aboutIconUrl = "https://www.alljoyn.org/sites/all/themes/at_alljoyn/images/img-alljoyn-logo.png";
        #endregion

        #region WeatherStation Alljoyn Device properties
        static readonly UInt32 GET_PROPERTY = AJ.AppMessageId( 0, 0, AJ.AJ_PROP_GET );
        static readonly UInt32 SET_PROPERTY = AJ.AppMessageId( 0, 0, AJ.AJ_PROP_SET );
        static readonly UInt32 PROPERTY1_ID = AJ.AppPropertyId( 0, 1, 1 );
        static readonly UInt32 PROPERTY2_ID = AJ.AppPropertyId( 0, 1, 2 );
        static readonly UInt32 PROPERTY3_ID = AJ.AppPropertyId( 0, 1, 3 );

        // These are the properties that will be read and written to by the client
        // application.  Set them to whatever you like.

        static string Property1 = "70"; // Current temp for the Weatherstation
        static string Property2 = String.Empty;
        static string Property3 = String.Empty;
        #endregion

        // Callback to retrieve WeatherStation properties
        public static AJ_Status GetPropertyHandler( AJ_Message reply, AJ_Message msg, uint propId, AJ aj )
        {
            if( propId == PROPERTY1_ID )
            {
                aj.MarshalArg( reply, "s", Property1 );
            }
            else if( propId == PROPERTY2_ID )
            {
                aj.MarshalArg( reply, "s", Property2 );
            }
            else if( propId == PROPERTY3_ID )
            {
                aj.MarshalArg( reply, "s", Property3 );
            }

            return AJ_Status.AJ_OK;
        }

        public static AJ_Status SetPropertyHandler( AJ_Message reply, AJ_Message msg, uint propId, AJ aj )
        {
            if( propId == PROPERTY1_ID )
            {
                Property1 = aj.UnmarshalArgs( msg, "s" );
            }
            else if( propId == PROPERTY2_ID )
            {
                Property2 = aj.UnmarshalArgs( msg, "s" );
            }
            else if( propId == PROPERTY3_ID )
            {
                Property3 = aj.UnmarshalArgs( msg, "s" );
            }

            return AJ_Status.AJ_OK;
        }

        public static void Main( )
        {
            #region Initialize ThinClient Service
            string[ ] weatherInterface = {
                "com.microsoft.NetMicroFrameWork.WeatherStation",
                "!WeatherEvent Arg1>s",
                "@String1=s",
                "@String2=s",
                "@String3=s",
                " "
            };


            UInt32 AJ_METHOD_ACCEPT_SESSION = AJ.BusMessageId( 2, 0, 0 );
            UInt32 AJ_SIGNAL_SESSION_JOINED = AJ.BusMessageId( 2, 0, 1 );
            UInt32 WEATHER_EVENT_SIGNAL = AJ.AppMessageId( 0, 1, 0 );

            const byte AJ_FLAG_GLOBAL_BROADCAST = 0x20;
            bool connected = false;
            const ushort port = 0;
            UInt32 bus = 0;
            var msg = new AJ_Message( );

            // Initialize Alljoyn object
            var myAlljoyn = new AJ( );
            var ajServices = new AJ_Services(myAlljoyn);

            // Create and set the property store
            var ps = new PropertyStore( );
            ps.SetDefaultDeviceNames( defaultDeviceNames );
            ps.SetSupportedLanguages( supportedLanguages );
            ps.SetDefaultLanguage( defaultLanguage );
            ps.SetDefaultSupportUrls( defaultSupportUrls );
            ps.SetDefaultDeviceNames( defaultDeviceNames );
            ps.SetDefaultDescriptions( defaultDescriptions );
            ps.SetDefaultManufacturers( defaultManufacturers );
            ps.SetDefaultModelNumber( defaultModelNumber );
            ps.SetDefaultSoftwareVersion( defaultSoftwareVersion );
            ps.InitMandatoryFields( );
            ajServices.SetPropertyStore( ps );

            // Set About attributes
            myAlljoyn.SetAboutIconContent( aboutIconContent );
            myAlljoyn.SetAboutIconURL( aboutIconUrl );

            myAlljoyn.CreateBus( ref bus );
            myAlljoyn.Initialize( );
            myAlljoyn.RegisterObjectsInterface( new AJ_Object( ) { path = ServicePath, interfaces = weatherInterface }, true, true );
            #endregion
            
            ajServices.Initialize_NotificationService("DEFAULT");

            // Attach to Netowrk Link state change notifications
            var netmon = new NetworkStateMonitor( );

            // Start the message processing loop
            while( true )
            {
                netmon.WaitForIpAddress( );
                AJ_Status status;
                if( !connected )
                {
                    status = myAlljoyn.StartService( bus, DaemonServiceName, CONNECT_TIMEOUT, AJ.AJ_FALSE, ServicePort, ServiceName, AJ.AJ_NAME_REQ_DO_NOT_QUEUE);
                    if( status != AJ_Status.AJ_OK )
                    {
                        continue;
                    }
                    Debug.Print( "StartService returned AJ_OK; running \n" );
                    connected = true;                    

                    // once connected to the Alljoyn router do the "About" annoucement
                    if( status == AJ_Status.AJ_OK )
                    {
                        myAlljoyn.doAnnounce = true;
                        myAlljoyn.AboutAnnounce( bus, ServicePort );
                    }
                }

                // wait for a message from the router
                status = myAlljoyn.UnmarshalMsg( bus, msg, UNMARSHAL_TIMEOUT );
                if( status == AJ_Status.AJ_ERR_TIMEOUT )
                {
                    Debug.Print( "do work\n" );
                    continue;
                }

                // if the message is ok, check for message types this app wants to handle 
                if( status == AJ_Status.AJ_OK )
                {
                    string str = "Received message + msgId=" + msg.msgId.ToString( "X" ) + "\n";
                    Debug.Print( str );

                    // Session connection from a client?
                    if( msg.msgId == AJ_METHOD_ACCEPT_SESSION )
                    {
                        string joiner = "";
                        UInt32 sessionId = 0;

                        myAlljoyn.UnmarshalArgs( msg, "qus", port, sessionId, joiner );
                        status = myAlljoyn.BusReplyAcceptSession( msg, 1 );
                        if( status == AJ_Status.AJ_OK )
                        {
                            Debug.Print( "Accepted session session_id \n" );
                        }
                        else
                        {
                            Debug.Print( "AJ_BusReplyAcceptSession: error \n" );
                        }
                    }  // Session connected
                    else if( msg.msgId == AJ_SIGNAL_SESSION_JOINED )
                    {
                        // do nothing...
                    } // Request to read an interface property
                    else if( msg.msgId == GET_PROPERTY )
                    {
                        myAlljoyn.BusGetProp( msg, GetPropertyHandler );
                    } // Set a property on the advertised interface
                    else if( msg.msgId == SET_PROPERTY )
                    {
                        myAlljoyn.BusSetProp( msg, SetPropertyHandler );

                        // This is how to send a signal. You can put this code in
                        // the handler for a weather event, etc.

                        uint sn = 4;
                        ajServices.SendNotification(bus, "Sample notify", 2, 500, ref sn);

                        var sig = new AJ_Message();
                        AJ_Status s = myAlljoyn.MarshalSignal(bus, sig, WEATHER_EVENT_SIGNAL, 0, 0, AJ_FLAG_GLOBAL_BROADCAST, 0);
                        s = myAlljoyn.MarshalArg(sig, "s", "22222 This is a bogus weather event");
                        myAlljoyn.DeliverMsg(sig);

                    }
                    else // default handling (pass it on to the bus)
                    {
                        myAlljoyn.BusHandleBusMessage( msg, bus, ServicePort );
                    }
                }
                myAlljoyn.CloseMsg( msg );

                if( status == AJ_Status.AJ_ERR_READ )
                {
                    Debug.Print( "AllJoyn disconnect\n" );
                    myAlljoyn.Disconnect( bus );
                    connected = false;

                    // sleep a little while before trying to connect again
                    myAlljoyn.Sleep( 10 * 1000 );
                }
            }
        }
    }
}
