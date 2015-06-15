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

#include <aj_crypto.h>
#include <alljoyn.h>

/* Forward Declaration */
static void Print_Connection_Summary(void);

static const char* routingNodeName = "org.alljoyn.BusNode";

/* All time values are in milliseconds */

static const uint16_t CONNECT_TIMEOUT   = 1000 * 5;

/* PAUSE is the delay between connection attempts */
static const uint16_t PAUSE_MAX   = 1000 * 4; /* Four seconds */
static const uint16_t PAUSE_MIN   = 500  * 1; /* Half-a-second */
/*
 * If RANDOM_PAUSE is TRUE, then a random value between MIN and MAX is generated
 * If RANDOM_PAUSE is FALSE, then the average of MIN & MAX is used
 */
static const uint8_t RANDOM_PAUSE = TRUE;

/*
 * Counters
 * The total number of connection attempts is the sum of
 * various pieces, viz. timed out, network error, unexpected error and success.
 * i.   timed out attempts - AJ_ERR_TIMEOUT timeout is returned
 * ii.  network error attempts - AJ_ERR_READ or AJ_ERR_WRITE or
 *                               AJ_ERR_CONNECT or AJ_ERR_LINK_DEAD is returned
 * iii. unexpected error attempts - Any other AJ_ERR_* returned
 * iv.  success attempts - AJ_OK is returned
 */
static uint32_t num_total_attempts            = 0;
static uint16_t num_network_error_attempts    = 0;
static uint16_t num_unexpected_error_attempts = 0;
static uint16_t num_timedout_attempts         = 0;
static uint16_t num_successful_attempts       = 0;

void AJ_Main(void)
{
    AJ_Status status = AJ_OK;
    AJ_BusAttachment bus;

    uint16_t timeout = 0;

    AJ_Initialize();

    AJ_Printf("\nAllJoyn Release: %s\n\n", AJ_GetVersion());

    /*
     * Set the minimum protocol version of 'acceptable' router node to 14.02
     * to exercise LegacyNS code as well.
     * The protocol version for 14.02 is 9, as specified at:
     * https://git.allseenalliance.org/cgit/core/alljoyn.git/tree/alljoyn_core/inc/alljoyn/AllJoynStd.h?id=v14.02#n33
     */

    AJ_SetMinProtoVersion(9);

    /*
     * Set the selection timeout to a different value from the default.
     */
    AJ_SetSelectionTimeout(2000);

    /* Connect and disconnect forever */
    while (TRUE) {
        AJ_Printf("Attempting to connect to a routing node with prefix: %s ...\n", routingNodeName);

        status = AJ_FindBusAndConnect(&bus, routingNodeName, CONNECT_TIMEOUT);
        num_total_attempts++;

        if (AJ_ERR_READ == status || AJ_ERR_WRITE == status ||
            AJ_ERR_CONNECT == status || AJ_ERR_LINK_DEAD == status) {
            num_network_error_attempts++;
            AJ_Printf("Network failure while connecting to routing node: %s (code: %u)\n", AJ_StatusText(status), status);
        } else if (AJ_ERR_TIMEOUT == status) {
            num_timedout_attempts++;
            AJ_Printf("Timedout while connecting to routing node\n");
        } else if (AJ_OK == status) {
            num_successful_attempts++;
            AJ_Printf("Connected to routing node (protocol version = %u). Got unique name - %s\n", AJ_GetRoutingProtoVersion(), AJ_GetUniqueName(&bus));
        } else {
            /* Unexpected failures */
            num_unexpected_error_attempts++;
            AJ_Printf("!!!Unexpected!!! failure when connecting to routing node: %s (code: %u)\n", AJ_StatusText(status), status);
        }

        Print_Connection_Summary();

        if (RANDOM_PAUSE) {
            /* Generate random timeout, between PAUSE_MIN and PAUSE_MAX */
            AJ_RandBytes((uint8_t*)(&timeout), sizeof(timeout));
            timeout = PAUSE_MIN + timeout % (PAUSE_MAX - PAUSE_MIN);
        } else {
            /* fixed timeout is the average of PAUSE_MIN and PAUSE_MAX */
            timeout = (PAUSE_MIN + PAUSE_MAX) / 2;
        }
        AJ_Sleep(timeout);

        if (AJ_OK == status) {
            AJ_Disconnect(&bus);
            AJ_Printf("Disconnected from the routing node. ");
        }
    }
}

static void Print_Connection_Summary(void) {
    AJ_Printf("\n\t--Connection counters--\n"
              "\tNumber of successful attempts        = %u\n"
              "\tNumber of timedout attempts          = %u\n"
              "\tNumber of network error attempts     = %u\n"
              "\tNumber of unexpected error attempts  = %u\n"
              "\tTotal Number of attempts             = %u\n"
              "\t-----------------------\n\n",
              num_successful_attempts, num_timedout_attempts,
              num_network_error_attempts, num_unexpected_error_attempts,
              num_total_attempts);
}

#ifdef AJ_MAIN
int main(void)
{
    /* AJ_Main is not expected to return */
    AJ_Main();

    return 0;
}
#endif
