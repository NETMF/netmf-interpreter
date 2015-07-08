/*
 * @file
 */

/******************************************************************************
 * Copyright AllSeen Alliance. All rights reserved.
 *
 *    Permission to use, copy, modify, and/or distribute this software for any
 *    purpose with or without fee is hereby granted, provided that the above
 *    copyright notice and this permission notice appear in all copies.
 *
 *    THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
 *    WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
 *    MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
 *    ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
 *    WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
 *    ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
 *    OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 ******************************************************************************/

#define AJ_MODULE NETBUSSAMPLE

#include <stdio.h>
#include <stdlib.h>
#include <assert.h>

#include "alljoyn.h"
#include "aj_debug.h"
#include "aj_helper.h"
#include "aj_debug.h"
#include "aj_config.h"
#include "aj_net.h"
#include "aj_bus.h"
#include "aj_disco.h"
#include "aj_wifi_ctrl.h"
#include "aj_guid.h"

#ifndef NDEBUG
AJ_EXPORT uint8_t dbgNETBUSSAMPLE = 0;
#endif

static const char* const testInterface[] = {
    "org.alljoyn.alljoyn_test",
    "?my_ping inStr<s outStr>s",
    NULL
};



static const AJ_InterfaceDescription testInterfaces[] = {
    testInterface,
    NULL
};

static const AJ_Object AppObjects[] = {
    { "/org/alljoyn/alljoyn_test", testInterfaces },
    { NULL }
};


static uint32_t MyBusAuthPwdCB(uint8_t* buf, uint32_t bufLen)
{
    const char* myPwd = "1234";
    strncpy((char*)buf, myPwd, bufLen);
    return (uint32_t)strlen(myPwd);
}

static const char serviceName[] = "org.alljoyn.BusNode";

static AJ_Status ConnectToBus(AJ_BusAttachment* bus)
{
    AJ_Status status;
    AJ_Service service;
    uint32_t timeout = 5000;

#ifdef AJ_SERIAL_CONNECTION
    AJ_Time start, now;
    AJ_InitTimer(&start);
#endif

    AJ_InfoPrintf(("AJ_Connect(bus=0x%p, serviceName=\"%s\", timeout=%d.)\n", bus, serviceName, timeout));

    /*
     * Clear the bus struct
     */
    memset(bus, 0, sizeof(AJ_BusAttachment));
    /*
     * Clear stale name->GUID mappings
     */
    AJ_GUID_ClearNameMap();

    /*
     * First we need to discover a routing node.  This is done with the function AJ_Discover.
     * It will store the connection information in the AJ_Service struct.
     */

#if AJ_CONNECT_LOCALHOST
    service.ipv4port = 9955;
#if HOST_IS_LITTLE_ENDIAN
    service.ipv4 = 0x0100007F; // 127.0.0.1
#endif
#if HOST_IS_BIG_ENDIAN
    service.ipv4 = 0x7f000001; // 127.0.0.1
#endif
    service.addrTypes = AJ_ADDR_TCP4;
#elif defined ARDUINO
    service.ipv4port = 9955;
    service.ipv4 = 0x6501A8C0; // 192.168.1.101
    service.addrTypes = AJ_ADDR_TCP4;
    status = AJ_Discover(serviceName, &service, timeout, AJ_SELECTION_TIMEOUT);
    if (status != AJ_OK) {
        AJ_InfoPrintf(("AJ_Connect(): AJ_Discover status=%s\n", AJ_StatusText(status)));
        goto ExitConnect;
    }
#elif defined AJ_SERIAL_CONNECTION
    // don't bother with discovery, we are connected to a daemon.
    // however, take this opportunity to bring up the serial connection
    status = AJ_Serial_Up();
    if (status != AJ_OK) {
        AJ_InfoPrintf(("AJ_Connect(): AJ_Serial_Up status=%s\n", AJ_StatusText(status)));
    }
#else
    status = AJ_Discover(serviceName, &service, timeout, AJ_SELECTION_TIMEOUT);
    if (status != AJ_OK) {
        AJ_InfoPrintf(("AJ_Connect(): AJ_Discover status=%s\n", AJ_StatusText(status)));
        goto ExitConnect;
    }
#endif

    /*
     * Now that we have discovered a routing node, we can connect to it.  This is done with AJ_Net_Connect.
     */

    status = AJ_Net_Connect(bus, &service);
    if (status != AJ_OK) {
        // or retry discovery to find another node that will accept our connection
        AJ_InfoPrintf(("AJ_Connect(): AJ_Net_Connect status=%s\n", AJ_StatusText(status)));
        goto ExitConnect;
    }

#ifdef AJ_SERIAL_CONNECTION
    // run the state machine for long enough to (hopefully) do the SLAP handshake
    do {
        AJ_StateMachine();
        AJ_InitTimer(&now);
    } while (AJ_SerialLinkParams.linkState != AJ_LINK_ACTIVE && AJ_GetTimeDifference(&now, &start) < timeout);

    if (AJ_SerialLinkParams.linkState != AJ_LINK_ACTIVE) {
        AJ_InfoPrintf(("Failed to establish active SLAP connection in %u msec\n", timeout));
        AJ_SerialShutdown();
        return AJ_ERR_TIMEOUT;
    }
#endif

    /*
     * We are connected to a routing node!  We still need to authenticate with it
     * before it will route our messages.
     */
    status = AJ_Authenticate(bus);

ExitConnect:

    if (status != AJ_OK) {
        AJ_InfoPrintf(("AJ_Connect(): status=%s\n", AJ_StatusText(status)));
        AJ_Disconnect(bus);
    }
    return status;
}


static const char ssid[] = "AllJoyn";
static const char passphrase[] = "ajajajaj";
static const AJ_WiFiSecurityType secType = AJ_WIFI_SECURITY_WPA2;


int AJ_Main(void)
{
#ifdef AJ_CONFIGURE_WIFI_UPON_START
# error This sample cannot be built with AJ_CONFIGURE_WIFI_UPON_START defined
#endif
// dhcp_attempts applies to systems in need of network connections
#if !(defined(ARDUINO) || defined(__linux) || defined(_WIN32) || defined(__MACH__))
    int32_t dhcp_attempts = 5;
#endif

    AJ_Status status = AJ_OK;
    AJ_BusAttachment bus;

    /*
     * One time initialization before calling any other AllJoyn APIs
     */
    AJ_Initialize();

    AJ_PrintXML(AppObjects);
    AJ_RegisterObjects(AppObjects, NULL);

    SetBusAuthPwdCallback(MyBusAuthPwdCB);

// Windows, Linux and Arduino are already connected to the network when we get to this point.
#if !(defined(ARDUINO) || defined(__linux) || defined(_WIN32) || defined(__MACH__))
#define AJ_DHCP_TIMEOUT 10000

    // Step 1: connect to a WIFI network.
    // This will also attempt to acquire an IP from DHCP
    status = AJ_ConnectWiFi(ssid, secType, AJ_WIFI_CIPHER_CCMP, passphrase);

    // if DHCP timed out, try it again for up to five attempts
    while (status == AJ_ERR_TIMEOUT && --dhcp_attempts) {
        uint32_t ip, mask, gw;
        status = AJ_AcquireIPAddress(&ip, &mask, &gw, AJ_DHCP_TIMEOUT);

        if (status != AJ_OK) {
            AJ_ErrPrintf(("AJ_Net_Up(): AJ_AcquireIPAddress Failed\n"));
        }
    }


    if (status != AJ_OK) {
        printf("Unable to connect to wifi and acquire IP\n");
        return status;
    }

#endif

    //Now we need to find a daemon, connect to it and authenticate
    // see the ConnectToBus for an explanation.
    status = ConnectToBus(&bus);

    AJ_Sleep(10000);

    AJ_Disconnect(&bus);

    return status;
}

#ifdef AJ_MAIN
int main()
{
    return AJ_Main();
}
#endif
