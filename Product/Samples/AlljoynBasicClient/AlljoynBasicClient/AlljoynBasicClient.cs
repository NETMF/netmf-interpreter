using System;
using Microsoft.NETMF.Networking;
using Microsoft.SPOT;
using Microsoft.SPOT.AllJoyn;

namespace sample_client
{
    public class AlljoynBasicClient
    {
        static uint CONNECT_TIMEOUT   = (1000 * 7);
        static uint UNMARSHAL_TIMEOUT = (1000 * 5);
        static uint METHOD_TIMEOUT    = (100 * 10);

        static string ServiceName = "org.alljoyn.Bus.sample";
        static string ServicePath = "/sample";
        static UInt16 ServicePort = 25;
        static string fullServiceName = "test";

        static string[] SampleInterface = {
            "org.alljoyn.Bus.sample",           // The first entry is the interface name
            "?Dummy foo<i",                     // This is just a dummy entry at index 0 for illustration purposes
            "?Dummy2 fee<i",                    // This is just a dummy entry at index 1 for illustration purposes
            "?cat inStr1<s inStr2<s outStr>s",  // Method at index 2
            ""
        };

        static readonly UInt32 BASIC_CLIENT_CAT = AJ.PrxMessageId(0,0,2);
        static readonly UInt32 AJ_SIGNAL_SESSION_LOST_WITH_REASON = AJ.BusMessageId(1, 0, 17);

        static UInt32 AJ_REPLY_ID(UInt32 id)
        {
            UInt32 rep = (id | ((UInt32)0x80 << 24));
            return rep;
        }

        static public void MakeMethodCall(UInt32 bus, UInt32 sessionId, AJ myAlljoyn)
        {
            AJ_Status status = AJ_Status.AJ_OK;
            AJ_Message msg = new AJ_Message();

            status = myAlljoyn.MarshalMethodCall(bus, msg, BASIC_CLIENT_CAT, fullServiceName, sessionId, 0, METHOD_TIMEOUT);

            if (status == AJ_Status.AJ_OK) {
                status = myAlljoyn.MarshalArgs(msg, "ss", "Hello ", "World!");
            }

            if (status == AJ_Status.AJ_OK) {
                status = myAlljoyn.DeliverMsg(msg);
            }
        }

        public static void Main()
        {
            AJ_Status status = AJ_Status.AJ_OK;
            UInt32 bus = 0;
            bool connected = false;
            bool done = false;
            UInt32 sessionId = 0;

            AJ myAlljoyn = new AJ();
            myAlljoyn.CreateBus( ref bus );
            myAlljoyn.Initialize();
            myAlljoyn.RegisterObjectsInterface(new AJ_Object( ) { path = ServicePath, interfaces = SampleInterface }, false, false);

            // Attach to Netowrk Link state change notifications
            var netmon = new NetworkStateMonitor();
            int retry = 0;

            while (!done) {

                netmon.WaitForIpAddress();

                AJ_Message msg = new AJ_Message();

                if (!connected) {

                    do
                    {
                      status = myAlljoyn.ClientConnectBus(bus, null, CONNECT_TIMEOUT);
                    } while (status != AJ_Status.AJ_OK);

                    do
                    {
                      status = myAlljoyn.ClientFindService(bus, ServiceName, null, CONNECT_TIMEOUT);
                    } while (status != AJ_Status.AJ_OK);
                    retry = 0;
                    do
                    {
                        status = myAlljoyn.ClientConnectService(bus, CONNECT_TIMEOUT, ServiceName, ServicePort, ref sessionId, null, ref fullServiceName);
                        if (retry++ > 10) break;
                    } while (status != AJ_Status.AJ_OK);
                    
                    if (status == AJ_Status.AJ_OK) {
                        connected = true;
                        MakeMethodCall(bus, sessionId, myAlljoyn);

                    } else {
                        Debug.Print("Timed out");
                        myAlljoyn.Disconnect(bus);
                        continue;
                    }
                }

                status = myAlljoyn.UnmarshalMsg(bus, msg, UNMARSHAL_TIMEOUT);

                if (AJ_Status.AJ_ERR_TIMEOUT == status) {
                    continue;
                }

                if (AJ_Status.AJ_OK == status) {
                    
                    if(msg.msgId == AJ_REPLY_ID(BASIC_CLIENT_CAT))
                    {
                        UInt32 argPtr = myAlljoyn.GetArgPtr(4);
                        
                        status = myAlljoyn.UnmarshalArg(msg, argPtr);


                        string value = myAlljoyn.GetArgString(4);

                        if (AJ_Status.AJ_OK == status) {

                            Debug.Print(ServiceName + "cat returned " + value);

                            done = true;
                        } else {
                            MakeMethodCall(bus, sessionId, myAlljoyn);
                        }
                    }
                    else if (msg.msgId == AJ_SIGNAL_SESSION_LOST_WITH_REASON)
                    {
                        {
                            UInt32 id=0, reason=0;
                            myAlljoyn.UnmarshalArgs(msg, "uu", ref id, ref reason);
                        }
                        status = AJ_Status.AJ_ERR_SESSION_LOST;
                    }
                    else
                    {
                        /* Pass to the built-in handlers. */
                        status = myAlljoyn.BusHandleBusMessage(msg, bus, ServicePort);
                    }
                }

                /* Messages MUST be discarded to free resources. */
                myAlljoyn.CloseMsg(msg);

                if (status == AJ_Status.AJ_ERR_SESSION_LOST) {
                    myAlljoyn.Disconnect(bus);
                    //exit(0);
                }
            }
        }
    }
}
