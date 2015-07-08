using System;
using Microsoft.NETMF.Networking;
using Microsoft.SPOT;
using Microsoft.SPOT.AllJoyn;
using Microsoft.SPOT.AllJoyn.Services;

namespace AlljoynSecurityService
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

        const string DaemonServiceName = null;
        const int CONNECT_TIMEOUT = (2000);
        const uint UNMARSHAL_TIMEOUT = 5000;

        // The key exchange is in the 16 MSB
        const int AUTH_KEYX_ECDHE =       0x00400000;

        // The key authentication suite is in the 16 LSB
        const int AUTH_SUITE_ECDHE_NULL  = (AUTH_KEYX_ECDHE | 0x0001);
        const int AUTH_SUITE_ECDHE_PSK   = (AUTH_KEYX_ECDHE | 0x0002);
        const int AUTH_SUITE_ECDHE_ECDSA = (AUTH_KEYX_ECDHE | 0x0004);
        
        // List of allowed security mechanisms. Edit to restrict

        static int [] SecuritySuites = { AUTH_SUITE_ECDHE_ECDSA, AUTH_SUITE_ECDHE_PSK, AUTH_SUITE_ECDHE_NULL };

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

        // Handle ping message from client

        static AJ_Status AppHandlePing(AJ_Message msg, AJ myAlljoyn)
        {
            AJ_Status status;
            AJ_Message reply = new AJ_Message();
            UInt32 argPtr = myAlljoyn.GetArgPtr(3);

            status = myAlljoyn.UnmarshalArg(msg, argPtr);

            if (AJ_Status.AJ_OK == status)
            {
                string msgArg = myAlljoyn.GetArgString(3);                
                Debug.Print("Received ping request " + msgArg);
                
                status = myAlljoyn.MarshalReplyMsg(msg, reply);
                if (AJ_Status.AJ_OK == status)
                {                    
                     // Just return the arg we received
                    status = myAlljoyn.MarshalArg(reply, "s", msgArg);
                    if (AJ_Status.AJ_OK == status)
                    {
                        status = myAlljoyn.DeliverMsg(reply);
                    }
                }
            }

            return status;
        }

        public static void Main()
        {
            #region Initialize AlljoynSecurityService
            
            // Define device interface. Interfaces beginning with
            // dollar sign ('$') are deemed secure
            string[] deviceInterface = {
                "$org.alljoyn.bus.samples.secure.SecureInterface",
                "?Ping inStr<s outStr>s",
                " "
            };

            UInt32 AJ_METHOD_ACCEPT_SESSION = AJ.BusMessageId(2, 0, 0);
            UInt32 AJ_SIGNAL_SESSION_JOINED = AJ.BusMessageId(2, 0, 1);
            UInt32 BASIC_SERVICE_PING = AJ.AppMessageId(0, 0, 0);
            
            bool            connected = false;
            const ushort    port = 0;
            UInt32          bus = 0;
            var             msg = new AJ_Message();
            var             myAlljoyn = new AJ();
            
            // Initialize Alljoyn object
            myAlljoyn.CreateBus(ref bus);
            myAlljoyn.Initialize();
            myAlljoyn.RegisterObjectsInterface(new AJ_Object() { path = ServicePath, interfaces = deviceInterface }, false, true);
            
            #endregion

            // Attach to Netowrk Link state change notifications
            var netmon = new NetworkStateMonitor();

            // Start the message processing loop
            while (true)
            {
                netmon.WaitForIpAddress();
                AJ_Status status;
                if (!connected)
                {
                    status = myAlljoyn.StartService(bus, DaemonServiceName, CONNECT_TIMEOUT, AJ.AJ_FALSE, ServicePort, ServiceName, AJ.AJ_NAME_REQ_DO_NOT_QUEUE);
                    if (status != AJ_Status.AJ_OK)
                    {
                        Debug.Print(status.ToString());
                        continue;
                    }

                    Debug.Print("StartService returned AJ_OK; running \n");
                    connected = true;

                    // Provide credentials for all security
                    // suites by setting write-only properties

                    myAlljoyn.KeyExpiration = KeyExpiration;
                    myAlljoyn.PskHint = PskHint;
                    myAlljoyn.PskString = PskChar;
                    myAlljoyn.PemPriv = PemPrv;
                    myAlljoyn.PemX509 = PemX509;

                    // Enable security suites
                    myAlljoyn.EnableSecurity(bus, SecuritySuites);

                    // once connected to the Alljoyn router do the "About" annoucement
                    if (status == AJ_Status.AJ_OK)
                    {
                        myAlljoyn.doAnnounce = true;
                        myAlljoyn.AboutAnnounce(bus, ServicePort);
                    }
                }

                // wait for a message from the router
                status = myAlljoyn.UnmarshalMsg(bus, msg, UNMARSHAL_TIMEOUT);
                if (status == AJ_Status.AJ_ERR_TIMEOUT)
                {
                    Debug.Print("do work\n");
                    continue;
                }

                // if the message is ok, check for message types this app wants to handle 
                if (status == AJ_Status.AJ_OK)
                {
                    string str = "Received message + msgId=" + msg.msgId.ToString("X") + "\n";
                    Debug.Print(str);

                    // Session connection from a client?
                    if (msg.msgId == AJ_METHOD_ACCEPT_SESSION)
                    {
                        string joiner = "";
                        UInt32 sessionId = 0;

                        myAlljoyn.UnmarshalArgs(msg, "qus", port, sessionId, joiner);
                        status = myAlljoyn.BusReplyAcceptSession(msg, 1);
                        if (status == AJ_Status.AJ_OK)
                        {
                            Debug.Print("Accepted session session_id \n");
                        }
                        else
                        {
                            Debug.Print("AJ_BusReplyAcceptSession: error \n");
                        }
                    }  // Session connected
                    else if (msg.msgId == BASIC_SERVICE_PING)
                    {
                        status = AppHandlePing(msg, myAlljoyn);
                    }
                    else if (msg.msgId == AJ_SIGNAL_SESSION_JOINED)
                    {
                        // do nothing...
                    } // Request to read an interface property                    
                    else // default handling (pass it on to the bus)
                    {
                        myAlljoyn.BusHandleBusMessage(msg, bus, ServicePort);
                    }
                }
                myAlljoyn.CloseMsg(msg);

                if (status == AJ_Status.AJ_ERR_READ)
                {
                    Debug.Print("AllJoyn disconnect\n");
                    myAlljoyn.Disconnect(bus);
                    connected = false;

                    // sleep a little while before trying to connect again
                    myAlljoyn.Sleep(10 * 1000);
                }
            }
        }
    }
}
