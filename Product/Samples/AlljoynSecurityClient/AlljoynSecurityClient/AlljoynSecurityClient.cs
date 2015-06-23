using System;
using Microsoft.NETMF.Networking;
using Microsoft.SPOT;
using Microsoft.SPOT.AllJoyn;


namespace AlljoynSecurityClient
{
    public class Program
    {
        #region Constant Data

        const UInt32 KeyExpiration = 0xFFFFFFFF;
        const UInt32 CONNECT_ATTEMPTS = 10;

        const string ServiceName = "org.alljoyn.bus.samples.secure";
        const string InterfaceName = "org.alljoyn.bus.samples.secure.SecureInterface";
        const string ServicePath = "/SecureService";
        const UInt16 ServicePort = 42;

        static string FullServiceName = String.Empty;

        const string DaemonServiceName = null;
        const int CONNECT_TIMEOUT = (30000);
        const uint UNMARSHAL_TIMEOUT = 5000;
        static uint METHOD_TIMEOUT    = (100 * 10);

        // The key exchange is in the 16 MSB
        const int AUTH_KEYX_ECDHE =       0x00400000;

        // The key authentication suite is in the 16 LSB 
        const int AUTH_SUITE_ECDHE_NULL  = (AUTH_KEYX_ECDHE | 0x0001);
        const int AUTH_SUITE_ECDHE_PSK   = (AUTH_KEYX_ECDHE | 0x0002);
        const int AUTH_SUITE_ECDHE_ECDSA = (AUTH_KEYX_ECDHE | 0x0004);

        // List of allowed security mechanisms. Edit to change. Allowed mechanisms are:
        // AUTH_SUITE_ECDHE_ECDSA
        // AUTH_SUITE_ECDHE_PSK
        // AUTH_SUITE_ECDHE_NULL
        static int[] SecuritySuites = { AUTH_SUITE_ECDHE_NULL };

        const byte AJ_FLAG_ENCRYPTED = 0x80;

        static readonly UInt32 PING_METHOD = AJ.PrxMessageId(0, 0, 0);
        static readonly UInt32 AJ_SIGNAL_SESSION_LOST_WITH_REASON = AJ.BusMessageId(1, 0, 17);

        static string pingString = "Client AllJoyn Lite says Hello AllJoyn!";

        #endregion

        #region Security Suites

        // Private Key
        const string PemPrv = 
            "-----BEGIN EC PRIVATE KEY-----" +
            "MDECAQEEIICSqj3zTadctmGnwyC/SXLioO39pB1MlCbNEX04hjeioAoGCCqGSM49" +
            "AwEH" +
            "-----END EC PRIVATE KEY-----";

        // X509 Cert
        const string PemX509 =
            "-----BEGIN CERTIFICATE-----" +
            "MIIBWjCCAQGgAwIBAgIHMTAxMDEwMTAKBggqhkjOPQQDAjArMSkwJwYDVQQDDCAw" +
            "ZTE5YWZhNzlhMjliMjMwNDcyMGJkNGY2ZDVlMWIxOTAeFw0xNTAyMjYyMTU1MjVa" +
            "Fw0xNjAyMjYyMTU1MjVaMCsxKTAnBgNVBAMMIDZhYWM5MjQwNDNjYjc5NmQ2ZGIy" +
            "NmRlYmRkMGM5OWJkMFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEP/HbYga30Afm" +
            "0fB6g7KaB5Vr5CDyEkgmlif/PTsgwM2KKCMiAfcfto0+L1N0kvyAUgff6sLtTHU3" +
            "IdHzyBmKP6MQMA4wDAYDVR0TBAUwAwEB/zAKBggqhkjOPQQDAgNHADBEAiAZmNVA" +
            "m/H5EtJl/O9x0P4zt/UdrqiPg+gA+wm0yRY6KgIgetWANAE2otcrsj3ARZTY/aTI" +
            "0GOQizWlQm8mpKaQ3uE=" +
            "-----END CERTIFICATE-----";

        // Pre-shared Key
        const string PskHint = "<anonymous>";
        const string PskChar = "faaa0af3dd3f1e0379da046a3ab6ca44";

        #endregion

               

        // Helper function to formulate the reply id of
        // a proxy message
        static UInt32 AJ_REPLY_ID(UInt32 id)
        {
            UInt32 rep = (id | ((UInt32)0x80 << 24));
            return rep;
        }
        
        // Make ping method call to service
        static public AJ_Status SendPing(UInt32 bus, UInt32 sessionId, AJ myAlljoyn)
        {
            AJ_Status status;
            AJ_Message msg = new AJ_Message();

            status = myAlljoyn.MarshalMethodCall(bus, msg, PING_METHOD, FullServiceName, sessionId, AJ_FLAG_ENCRYPTED, METHOD_TIMEOUT);

            if (AJ_Status.AJ_OK == status) {
                status = myAlljoyn.MarshalArg(msg, "s", pingString);
            } else {
            }

            if (AJ_Status.AJ_OK == status) {
                status = myAlljoyn.DeliverMsg(msg);
            } else {
            }

            if (AJ_Status.AJ_OK != status) {
            }

            return status;
        }        

        public static void Main()
        {
            // Define device interface. Interfaces beginning with
            // dollar sign ('$') are deemed secure
            string[] deviceInterface = {
                "$org.alljoyn.bus.samples.secure.SecureInterface",
                "?Ping inStr<s outStr>s",
                " "
            };

            AJ_Status   status = AJ_Status.AJ_OK;
            AJ_Status   authStatus = AJ_Status.AJ_ERR_NULL;
            UInt32      bus = 0;
            bool        connected = false;
            bool        done = false;
            UInt32      sessionId = 0;
            AJ          myAlljoyn = new AJ();

            // Initialize Alljoyn object
            myAlljoyn.CreateBus( ref bus );
            myAlljoyn.Initialize();
            myAlljoyn.RegisterObjectsInterface(new AJ_Object() { path = ServicePath, interfaces = deviceInterface }, false, false);

            // Attach to Netowrk Link state change notifications
            var netmon = new NetworkStateMonitor();

            while (!done) {

                netmon.WaitForIpAddress();

                AJ_Message msg = new AJ_Message();

                if (!connected) {
                    status = myAlljoyn.StartClientByName(  bus,
                                                            null,
                                                            CONNECT_TIMEOUT,
                                                            0,
                                                            ServiceName,
                                                            ServicePort,
                                                            ref sessionId,
                                                            null,
                                                            ref FullServiceName);
                    
                    
                    if (status == AJ_Status.AJ_OK) {
                        connected = true;
                        authStatus = AJ_Status.AJ_ERR_NULL;

                        // Provide credentials for all security
                        // suites by setting write-only properties.
                        // Only the security suites specified
                        // in SecuritySuites will be used
                        myAlljoyn.KeyExpiration = KeyExpiration;
                        myAlljoyn.PskHint = PskHint;
                        myAlljoyn.PskString = PskChar;
                        myAlljoyn.PemPriv = PemPrv;
                        myAlljoyn.PemX509 = PemX509;

                        // Enable security suites
                        myAlljoyn.EnableSecurity(bus, SecuritySuites);

                        // Clear any stored credentials
                        myAlljoyn.ClearCredentials();

                        // Begin authentication process with the service. This is
                        // an asynchronous process so we will need to poll the 
                        // authStatus value before the main message processing loop
                        // can begin
                        status = myAlljoyn.AuthenticatePeer(bus, FullServiceName);
                        
                        if (status != AJ_Status.AJ_OK)
                        {
                            Debug.Print("AJ_BusAuthenticatePeer returned " + status.ToString());
                            break;
                        }

                    } else {
                        
                        continue;
                    }
                }

                // Poll for authStatus to determine when
                // authentication handshaking is complete.               

                authStatus = myAlljoyn.GetAuthStatus();
                if (authStatus != AJ_Status.AJ_ERR_NULL)
                {
                    if (authStatus != AJ_Status.AJ_OK)
                    {
                        myAlljoyn.Disconnect(bus);
                        break;
                    }

                    // We set the authStatus to NULL when the
                    // handshaking is finished to indicate that
                    // we should proceed with normal alljoyn
                    // message processing
                    myAlljoyn.SetAuthStatus(AJ_Status.AJ_ERR_NULL);           
         
                    // Make ping method call to service
                    status = SendPing(bus, sessionId, myAlljoyn);
                    if (status != AJ_Status.AJ_OK)
                    {
                        Debug.Print("SendPing returned " + status.ToString());
                        continue;
                    }
                }

                // process messages from the router
                status = myAlljoyn.UnmarshalMsg(bus, msg, UNMARSHAL_TIMEOUT);
                if (AJ_Status.AJ_ERR_TIMEOUT == status) {
                    continue;
                }

                if (AJ_Status.AJ_OK == status) {
                    
                    // Handle Ping response
                    if(msg.msgId == AJ_REPLY_ID(PING_METHOD))
                    {
                        UInt32 argPtr = myAlljoyn.GetArgPtr(4);

                        if (AJ_Status.AJ_OK == myAlljoyn.UnmarshalArg(msg, argPtr))
                        {
                            string value = myAlljoyn.GetArgString(4);
                            if (value == pingString)
                            {
                                Debug.Print("Ping was successful");
                            }
                            else
                            {
                                Debug.Print("Ping returned different string");
                            }
                        }
                        else
                        {
                        }

                        done = true;
                    }
                    else if (msg.msgId == AJ_SIGNAL_SESSION_LOST_WITH_REASON)
                    {
                        // Just eating the reason for this demo, in production
                        // reason should be inspected and acted upon
                        {
                            UInt32 id=0, reason=0;
                            myAlljoyn.UnmarshalArgs(msg, "uu", ref id, ref reason);
                        }
                        status = AJ_Status.AJ_ERR_SESSION_LOST;
                    }
                    else
                    {
                        // Pass to the built-in handlers
                        status = myAlljoyn.BusHandleBusMessage(msg, bus, ServicePort);
                    }
                }

                // Clean up
                myAlljoyn.CloseMsg(msg);

                if (status == AJ_Status.AJ_ERR_SESSION_LOST) {
                    myAlljoyn.Disconnect(bus);
                }
            }
        }
    }
}
