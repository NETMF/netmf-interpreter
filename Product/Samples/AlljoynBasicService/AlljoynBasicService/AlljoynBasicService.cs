//
// AlljoynService.cs
//
// Sample alljoyn service demonstrating Methods, Properties, Notifications, and About Service
//
//

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.AllJoyn;

namespace AlljoynBasicService
{
    public class Program
    {
        static string ServiceName = "org.alljoyn.Bus.sample";
        static string ServicePath = "/sample";
        static UInt16 ServicePort = 25;
        static string fullServiceName = "test";

        static readonly UInt32 BASIC_SERVICE_CAT = AJ.AppMessageId(0, 0, 1);

        static AJ_Status AppHandleCat(AJ_Message msg, AJ myAlljoyn)
        {
            AJ_Message reply = new AJ_Message();

            string s1 = myAlljoyn.UnmarshalArgs(msg, "s");
            string s2 = myAlljoyn.UnmarshalArgs(msg, "s");
            
            myAlljoyn.MarshalReplyMsg(msg, reply);

            // We have the arguments. Now do the concatenation.
            string buffer = s1 + s2;
            
            myAlljoyn.MarshalArg(reply, "s", buffer);

            return myAlljoyn.DeliverMsg(reply);
        }

        // program entry point

        public static void Main()
        {
            // Initialize Alljoyn class instance

            AJ myAlljoyn = new AJ();

            // some state variables

            bool connected = false;
            AJ_Status status = AJ_Status.AJ_OK;

            // constants used in the message processing loop

            UInt32 CONNECT_TIMEOUT = (1000 * 1000);
            UInt32 UNMARSHAL_TIMEOUT = 5000;

            UInt32 AJ_METHOD_ACCEPT_SESSION = AJ.BusMessageId(2, 0, 0);
            UInt32 AJ_SIGNAL_SESSION_JOINED = AJ.BusMessageId(2, 0, 1);

            byte AJ_FLAG_GLOBAL_BROADCAST = 0x20;

            UInt16 port = 0;
            UInt32 bus = 0;
            AJ_Message msg = new AJ_Message();            


            // Define the Alljoyn interface that will be
            // published to the outside world

            string[] testInterface = {
                "org.alljoyn.Bus.sample",           // The first entry is the interface name. 
                "?Dummy foo<i",                     // This is just a dummy entry at index 0 for illustration purposes. 
                "?cat inStr1<s inStr2<s outStr>s",  // Method at index 1. 
                " "
            };
           
            // Create the Alljoyn bus instance

            myAlljoyn.CreateBus(ref bus);

            // Initialize Alljoyn instance

            myAlljoyn.Initialize();

            // Register alljoyn interface. We set the 2nd parameter to false to indicate that
            // properties will not be used

            myAlljoyn.RegisterObjectsInterface(new AJ_Object() { path = ServicePath, interfaces = testInterface }, false, true);

            // Start the message processing loop

            while (true)
            {
                if (!connected)
                {
                    IntPtr arg = new IntPtr();
                    status = myAlljoyn.StartService(bus, null, CONNECT_TIMEOUT, AJ.AJ_FALSE, ServicePort, ServiceName, AJ.AJ_NAME_REQ_DO_NOT_QUEUE);
                    if (status != AJ_Status.AJ_OK)
                    {
                        goto exit;
                    }
                    Debug.Print("StartService returned AJ_OK; running \n");
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
                        Debug.Print("do work\n");
                        goto exit;
                    }
                }

                if (status == AJ_Status.AJ_OK)
                {
                    string str = "Received message + msgId=" + msg.msgId.ToString("X") + "\n";
                    Debug.Print(str);

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

                    }
                    else if (msg.msgId == AJ_SIGNAL_SESSION_JOINED)
                    {
                        // do nothing...

                    }
                    else if (msg.msgId == BASIC_SERVICE_CAT)
                    {
                        status = AppHandleCat(msg, myAlljoyn);
                    }
                    else
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

                    // sleep a little while before trying to connect

                    myAlljoyn.Sleep(10 * 1000);
                }

            exit:
                Debug.Print(" Exit  \n");
            }
        }
    }
}

