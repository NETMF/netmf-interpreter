/**
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

#include <aj_util.h>
#include <aj_wifi_ctrl.h>
#include <aj_crypto.h>
#include <alljoyn.h>

/* Forward Declaration */
static void wifiScanResultCallback(void* context, const char* ssid, const uint8_t mac[6], uint8_t rssi, AJ_WiFiSecurityType secType, AJ_WiFiCipherType cipherType);

static void PrintStats(void);

static char* TestAddrStr(uint32_t addr);

/* Configurable test knobs */
static const uint8_t RUN_NEGATIVE_TESTS = TRUE;
static const uint16_t DELAY_BETWEEN_SCANS = 5000; /* milliseconds */
static const uint16_t DELAY_BETWEEN_ASSOCIATIONS = 15000; /* milliseconds */
static const uint16_t DHCP_TIMEOUT = 1000 * 10; /* in milliseconds */
static const uint16_t DISCOVER_TIMEOUT = 1000 * 3; /* in milliseconds */

/* static globals */
#define AJ_TEST_NUM_MAX_OCTETS_IN_SSID 32

static const char* const secStr[] = { "OPEN", "WEP", "WPA", "WPA2" };
static const char* const ciphStr[] = { "OPEN", "TKIP", "CCMP", "WEP" };

typedef struct {
    char ssid[AJ_TEST_NUM_MAX_OCTETS_IN_SSID + 1];
    uint8_t bssid[6];
    uint8_t rssi;
    AJ_WiFiSecurityType secType;
    AJ_WiFiCipherType cipherType;
} AJ_WiFiScanInfoEntry;

static AJ_WiFiScanInfoEntry wifiScanInfo[255]; /* 255 == UINT8_MAX */

static uint8_t numValidScanEntries = 0;

/* The intention is to explcitly avoid connecting to any routing node */
static const char routingNodePrefix[] = "test.randhex";

/* counters to keep track of the progress */
typedef struct {
    uint16_t numSuccessful;
    uint16_t numTimedout;
    uint16_t numFailed;
} testStats_t;

static testStats_t scanStats;
static testStats_t associationStats;
static testStats_t dhcpStats;
static testStats_t discoverStats;
static testStats_t disassociationStats;

static const uint8_t* sentinelForWifiScanContext = NULL;
static uint8_t currentContext = 0; /* an offset value from the above pointer */

void AJ_Main(void)
{
    uint8_t i = 0;
    AJ_Status status = AJ_ERR_FAILURE; /* the glass is half-empty */
    AJ_BusAttachment bus;

    /* these variables are used when kicking off DHCP */
    uint32_t current_ip_address = 0;
    uint32_t current_subnet_mask = 0;
    uint32_t current_default_gateway = 0;

    AJ_Initialize();

    AJ_Printf("\nAllJoyn Release: %s\n\n", AJ_GetVersion());

    /* reset the wifi to start with a clean slate */
    status = AJ_ResetWiFi();
    if (AJ_OK != status) {
        AJ_Printf("WARN: AJ_ResetWiFi returned %s (code: %u).\n", AJ_StatusText(status), status);
    }

    /*
     * Repeatedly do the following:
     * a. scan for access points in the vicinity
     * b. For each open access point in the returned results:
     *      i.   associate using AJ_ConnectWiFi
     *      ii.  acquire an ip address using AJ_AcquireIPAddress
     *      iii. perform a short-lived discovery on some random node prefix
     *           using AJ_FindBusAndConnect
     *      iv.  disassociate using AJ_DisconnectWiFi
     * c. For each secured access point in the returned results:
     *      i. Connect to it with intentionally incorrect parameters
     *         (Negative test)
     */
    while (TRUE) {
        AJ_RandBytes(&currentContext, sizeof(currentContext));

        uint8_t currentMaxAps = 0;
        AJ_RandBytes(&currentMaxAps, sizeof(currentMaxAps));

        /* Reset numValidScanEntries, before every scan attempt. */
        numValidScanEntries = 0;

        status = AJ_WiFiScan((void*) (sentinelForWifiScanContext + currentContext), wifiScanResultCallback, currentMaxAps);

        if (AJ_OK != status) {
            if (AJ_ERR_FAILURE == status && 0 != currentMaxAps) {
                scanStats.numFailed++;
            } else if (AJ_ERR_RESOURCES == status) {
                scanStats.numFailed++;
            }

            AJ_Printf("Failed to scan: %s (code: %u)\n", AJ_StatusText(status), status);

            /* No point in attempting to do wifi operations, when scan failed */
            continue;
        } else {
            /*
             * A success was returned from AJ_WiFiScan. Were any results
             * returned after all??
             */
            if (0 < numValidScanEntries) {
                scanStats.numSuccessful++;
            } else {
                AJ_Printf("WARN: AJ_WiFiScan returned %s (code: %u), but returned ZERO scan results...\n", AJ_StatusText(status), status);
                scanStats.numFailed++;

                /* When num of scan results is zero, there is nothing to do */
                continue;
            }
        }

        /* numValidScanEntries is an index into the array. Hence +1. */
        if (currentMaxAps < numValidScanEntries + 1) {
            AJ_Printf("WARN: Scan returned more results (%u) than requested (%u).\n", numValidScanEntries + 1, currentMaxAps);
        } else {
            AJ_Printf("Wifi scan successful (got %u results).\n", numValidScanEntries);
        }

        for (i = 0; i < numValidScanEntries; i++) {
            if (AJ_WIFI_SECURITY_NONE != wifiScanInfo[i].secType) {
                /* On some targets, it is not possible to check for 802.11
                 * authentication failure when security type is WEP.
                 * Hence, run negative tests only for WPA/WPA2-based APs.
                 */
                if (RUN_NEGATIVE_TESTS && AJ_WIFI_SECURITY_WEP != wifiScanInfo[i].secType) {
                    /* Run a negative test for wifi association */
                    AJ_Printf("RUN (negative test): Attempting to associate with %s using a randomly generated passphrase...", wifiScanInfo[i].ssid);
                    char random_passphrase[128 + 1];
                    AJ_RandHex(random_passphrase, sizeof(random_passphrase), (sizeof(random_passphrase) - 1) / 2);

                    /* Set a random passphrase length based on security type */
                    uint8_t randomPassphraseLen = 0;
                    /* WPA / WPA2 - assuming min len is 8 and max is 64 */
                    AJ_RandBytes(&randomPassphraseLen, sizeof(randomPassphraseLen));
                    randomPassphraseLen = 8 + randomPassphraseLen % (64 - 8 + 1);
                    random_passphrase[randomPassphraseLen] = '\0';
                    status = AJ_ConnectWiFi(wifiScanInfo[i].ssid, wifiScanInfo[i].secType, wifiScanInfo[i].cipherType, random_passphrase);
                    if (AJ_OK == status) {
                        /* negative test failed */
                        AJ_Printf("FAIL (negative test): Associated with SSID: %s BSSID: %x:%x:%x:%x:%x:%x Security: %s(%s) Passphrase: %s RSSI: %u ...\n", wifiScanInfo[i].ssid, wifiScanInfo[i].bssid[0], wifiScanInfo[i].bssid[1], wifiScanInfo[i].bssid[2], wifiScanInfo[i].bssid[3], wifiScanInfo[i].bssid[4], wifiScanInfo[i].bssid[5], secStr[wifiScanInfo[i].secType], ciphStr[wifiScanInfo[i].cipherType], random_passphrase, wifiScanInfo[i].rssi);

                        /* negative test failed - don't go any further */
                        AJ_ASSERT(0);
                    } else {
                        AJ_Printf("Done (negative test).\n");
                    }
                    status = AJ_DisconnectWiFi();
                    AJ_Sleep(DELAY_BETWEEN_ASSOCIATIONS);
                }

                continue;
            } else {
                AJ_Printf("Attempting to associate with SSID: %s BSSID: %x:%x:%x:%x:%x:%x Security: %s(%s) RSSI: %u ...\n",
                          wifiScanInfo[i].ssid,
                          wifiScanInfo[i].bssid[0], wifiScanInfo[i].bssid[1], wifiScanInfo[i].bssid[2], wifiScanInfo[i].bssid[3], wifiScanInfo[i].bssid[4], wifiScanInfo[i].bssid[5],
                          secStr[wifiScanInfo[i].secType], ciphStr[wifiScanInfo[i].cipherType],
                          wifiScanInfo[i].rssi);
                status = AJ_ConnectWiFi(wifiScanInfo[i].ssid, wifiScanInfo[i].secType, wifiScanInfo[i].cipherType, "");
                if (AJ_OK != status) {
                    associationStats.numFailed++;
                    AJ_Printf("Failed to associate : %s (code: %u)\n", AJ_StatusText(status), status);
                    /*
                     * No point in proceeding any further when WiFi association
                     * has failed
                     */
                    continue;
                } else {
                    associationStats.numSuccessful++;
                }

                AJ_Printf("Successfully associated. Attempting to get IP Address via DHCP...\n");

                status = AJ_AcquireIPAddress(&current_ip_address, &current_subnet_mask, &current_default_gateway, DHCP_TIMEOUT);
                if (AJ_OK != status) {
                    if (AJ_ERR_TIMEOUT == status) {
                        dhcpStats.numTimedout++;
                        AJ_Printf("Timedout (%u ms) while trying to get IP Address via DHCP\n", DHCP_TIMEOUT);
                    } else {
                        dhcpStats.numFailed++;
                        AJ_Printf("Failed to get IP Address via DHCP : %s (code: %u)\n", AJ_StatusText(status), status);
                    }
                    /*
                     * No point in proceeding any further when IP address was
                     * not acquired
                     */
                    continue;
                } else {
                    dhcpStats.numSuccessful++;
                    AJ_Printf("Successfully obtained\n");
                    AJ_Printf("\tIP Addresss    : %s\n", TestAddrStr(current_ip_address));
                    AJ_Printf("\tSubnet Mask    : %s\n", TestAddrStr(current_subnet_mask));
                    AJ_Printf("\tDefault Gateway: %s\n", TestAddrStr(current_default_gateway));
                }

                /* Generate a random name using routingNodePrefix */
                char currentRoutingNodeName[32 + 1];
                strncpy(currentRoutingNodeName, routingNodePrefix, sizeof(routingNodePrefix));
                AJ_RandHex(currentRoutingNodeName + strlen(routingNodePrefix), sizeof(currentRoutingNodeName) - sizeof(routingNodePrefix), (sizeof(currentRoutingNodeName) - sizeof(routingNodePrefix) - 1) / 2);
                currentRoutingNodeName[32] = '\0'; /* just to be safe */

                AJ_Printf("Attempting to discover routing node: %s...", currentRoutingNodeName);

                status = AJ_FindBusAndConnect(&bus, currentRoutingNodeName, DISCOVER_TIMEOUT);

                if (AJ_ERR_TIMEOUT == status) {
                    /* this is the expected result */
                    discoverStats.numTimedout++;
                    AJ_Printf("Done (discovery of routing node).\n");
                } else if (AJ_OK != status) {
                    discoverStats.numFailed++;
                    AJ_Printf("Failed to connect to routing node: %s (code: %u)\n", AJ_StatusText(status), status);
                } else if (AJ_OK == status) {
                    /*
                     * the test attempted to discovery a randomly generated
                     * routing node prefix and it worked - highly unlikely event
                     */
                    AJ_Printf("FATAL: Was able to discover and connect to routing node with prefix %s. Got unique address %s.", currentRoutingNodeName, AJ_GetUniqueName(&bus));
                    AJ_ASSERT(0);
                }

                status = AJ_DisconnectWiFi();

                if (AJ_OK != status) {
                    disassociationStats.numFailed++;
                    AJ_Printf("Failed to disassociate: %s (code: %u)\n", AJ_StatusText(status), status);
                } else {
                    disassociationStats.numSuccessful++;
                    AJ_Printf("Disassociated from access point. ");
                }
                AJ_Sleep(DELAY_BETWEEN_ASSOCIATIONS);
            }
        }

        PrintStats();

        AJ_Sleep(DELAY_BETWEEN_SCANS);
    }
}

static void wifiScanResultCallback(void* context, const char* ssid, const uint8_t mac[6], uint8_t rssi, AJ_WiFiSecurityType secType, AJ_WiFiCipherType cipherType)
{
    uint8_t callbackContext = (((uint8_t*) context) - sentinelForWifiScanContext);
    if (currentContext != callbackContext) {
        AJ_Printf("WARN: Got wifi callback with context: %u different from current context: %u. Ignoring...\n", callbackContext, currentContext);
        return;
    }

    size_t lengthOfSsid = strlen(ssid);
    if (AJ_TEST_NUM_MAX_OCTETS_IN_SSID < lengthOfSsid) {
        AJ_Printf("WARN: Got wifi callback with an ssid %s of length %u (greater than %u). Ignoring...\n", ssid, lengthOfSsid, AJ_TEST_NUM_MAX_OCTETS_IN_SSID);
        return;
    }

    if (0 == lengthOfSsid) {
        AJ_Printf("WARN: Got wifi callback with an ssid of length ZERO. Ignoring...\n");
        return;
    }

    if ((AJ_WIFI_SECURITY_NONE == secType && AJ_WIFI_CIPHER_NONE != cipherType) ||
        (AJ_WIFI_SECURITY_WEP == secType && AJ_WIFI_CIPHER_WEP != cipherType) ||
        ((AJ_WIFI_SECURITY_WPA == secType || AJ_WIFI_SECURITY_WPA2 == secType) && (AJ_WIFI_CIPHER_CCMP != cipherType && AJ_WIFI_CIPHER_TKIP != cipherType))) {
        AJ_Printf("WARN: Got wifi callback with security type: %s and ciphertype: %s. Ignoring...\n", secStr[secType], ciphStr[cipherType]);
        return;
    }

    /* input validation done - store the result */

    strncpy(wifiScanInfo[numValidScanEntries].ssid, ssid, lengthOfSsid + 1);
    wifiScanInfo[numValidScanEntries].ssid[lengthOfSsid + 1] = '\0'; /* Force nul termination */

    memcpy(wifiScanInfo[numValidScanEntries].bssid, mac, sizeof(wifiScanInfo[numValidScanEntries].bssid));

    wifiScanInfo[numValidScanEntries].rssi = rssi;

    wifiScanInfo[numValidScanEntries].secType = secType;

    wifiScanInfo[numValidScanEntries].cipherType = cipherType;

    /*
     * All pieces have been copied to this entry, point to the next
     * the entry so that the next callback copies into that
     */
    numValidScanEntries++;

    return;
}

static void PrintStats(void)
{
    AJ_Printf("\n\t--Scan Stats--\n"
              "\t--------------\n"
              "\tSuccessful = %u Failed = %u\n",
              scanStats.numSuccessful, scanStats.numFailed);
    AJ_Printf("\n\t--Wifi Connect Stats--\n"
              "\t----------------------\n"
              "\tSuccessful = %u Failed = %u\n",
              associationStats.numSuccessful, associationStats.numFailed);
    AJ_Printf("\n\t--Dhcp Stats--\n"
              "\t--------------\n"
              "\tSuccessful = %u Timedout = %u Failed = %u\n",
              dhcpStats.numSuccessful, dhcpStats.numTimedout, dhcpStats.numFailed);
    AJ_Printf("\n\t--Discovery Stats--\n"
              "\t-------------------\n"
              "\tTimedout = %u Failed = %u\n",
              discoverStats.numTimedout, discoverStats.numFailed);
    AJ_Printf("\n\t--Wifi Disconnect Stats--\n"
              "\t-------------------------\n"
              "\tSuccessful = %u Failed = %u\n\n",
              disassociationStats.numSuccessful, disassociationStats.numFailed);
}

static char bufferToHoldCharsOfIpAddress[17];
static char* TestAddrStr(uint32_t addr)
{
    sprintf((char*)&bufferToHoldCharsOfIpAddress, "%3u.%3u.%3u.%3u",
            (addr & 0xFF000000) >> 24,
            (addr & 0x00FF0000) >> 16,
            (addr & 0x0000FF00) >>  8,
            (addr & 0x000000FF)
            );
    bufferToHoldCharsOfIpAddress[16] = '\0'; /* force null termination */

    return bufferToHoldCharsOfIpAddress;
}
