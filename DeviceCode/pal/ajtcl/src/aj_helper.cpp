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

/**
 * Per-module definition of the current module for debug logging.  Must be defined
 * prior to first inclusion of aj_debug.h
 */
#define AJ_MODULE HELPER

#include "alljoyn.h"
#include "aj_helper.h"
#include "aj_link_timeout.h"
#include "aj_debug.h"
#include "aj_config.h"

/**
 * Turn on per-module debug printing by setting this variable to non-zero value
 * (usually in debugger).
 */
#ifndef NDEBUG
uint8_t dbgHELPER = 0;
#endif

/**
 *  Type to describe pending timers
 */
typedef struct {
    TimeoutHandler handler; /**< The callback handler */
    void* context;          /**< A context pointer passed in by the user */
    uint32_t abs_time;      /**< The absolute time when this timer will fire */
    uint32_t repeat;        /**< The amount of time between timer events */
} Timer;

static Timer Timers[AJ_MAX_TIMERS] = {
    { NULL }
};

static uint32_t RunExpiredTimers(uint32_t now)
{
    uint32_t i = 0;
    uint32_t next = (uint32_t) -1;

    for (; i < AJ_MAX_TIMERS; ++i) {
        Timer* timer = Timers + i;
        if (timer->handler != NULL && timer->abs_time <= now) {
            (timer->handler)(timer->context);

            if (timer->repeat) {
                timer->abs_time += timer->repeat;
            } else {
                memset(timer, 0, sizeof(Timer));
            }
        }

        // track the next time we need to run
        if (timer->handler != NULL && next > timer->abs_time) {
            next = timer->abs_time;
        }
    }

    // return the next timeout that will run
    return next;
}

uint32_t AJ_SetTimer(uint32_t relative_time, TimeoutHandler handler, void* context, uint32_t repeat)
{
    uint32_t i;
    for (i = 0; i < AJ_MAX_TIMERS; ++i) {
        Timer* timer = Timers + i;
        // need to find an available timer slot
        if (timer->handler == NULL) {
            AJ_Time start = { 0, 0 };
            uint32_t now = AJ_GetElapsedTime(&start, FALSE);
            timer->handler = handler;
            timer->context = context;
            timer->repeat = repeat;
            timer->abs_time = now + relative_time;
            return i + 1;
        }
    }

    // available slot not found!
    AJ_ErrPrintf(("AJ_SetTimer(): Slot not found\n"));
    return 0;
}

void AJ_CancelTimer(uint32_t id)
{
    Timer* timer = Timers + (id - 1);
    AJ_ASSERT(id > 0 && id <= AJ_MAX_TIMERS);
    memset(timer, 0, sizeof(Timer));
}


AJ_Status AJ_RunAllJoynService(AJ_BusAttachment* bus, AllJoynConfiguration* config)
{
    uint8_t connected = FALSE;
    AJ_Status status = AJ_OK;
    AJ_InfoPrintf(("AJ_RunAllJoynService(bus=0x%p, config=0x%p)\n", bus, config));

    while (TRUE) {
        AJ_Time start = { 0, 0 };
        AJ_Message msg;
        uint32_t now;
        // get the next timeout
        uint32_t next;
        // wait forever
        uint32_t timeout = (uint32_t) -1;

        if (!connected) {
            status = AJ_StartService(
                bus,
                config->daemonName, config->connect_timeout, config->connected,
                config->session_port, config->service_name, config->flags, config->opts);
            if (status != AJ_OK) {
                AJ_ErrPrintf(("AJ_RunAllJoynService(): status=%s.\n", AJ_StatusText(status)));
                continue;
            }

            AJ_InfoPrintf(("AJ_RunAllJoynService(): connected to daemon: \"%s\"\n", AJ_GetUniqueName(bus)));

            connected = TRUE;

            /* Register a callback for providing bus authentication password */
            AJ_BusSetPasswordCallback(bus, config->password_callback);

            /* Configure timeout for the link to the daemon bus */
            AJ_SetBusLinkTimeout(bus, config->link_timeout);

            if (config->connection_handler != NULL) {
                (config->connection_handler)(connected);
            }
        }

        // absolute time in milliseconds
        now = AJ_GetElapsedTime(&start, FALSE);
        next = RunExpiredTimers(now);

        if (next != (uint32_t) -1) {
            // if no timers running, wait forever
            timeout = next;
        }

        status = AJ_UnmarshalMsg(bus, &msg, min(500, timeout));
        if (AJ_ERR_TIMEOUT == status && AJ_ERR_LINK_TIMEOUT == AJ_BusLinkStateProc(bus)) {
            AJ_ErrPrintf(("AJ_RunAllJoynService(): AJ_ERR_READ\n"));
            status = AJ_ERR_READ;
        }

        if (status == AJ_ERR_TIMEOUT) {
            // go back around and handle the expired timers
            continue;
        }

        if (status == AJ_OK) {
            uint8_t handled = FALSE;
            const MessageHandlerEntry* message_entry = config->message_handlers;
            const PropHandlerEntry* prop_entry = config->prop_handlers;

            // check the user's handlers first.  ANY message that AllJoyn can handle is override-able.
            while (handled != TRUE && message_entry->msgid != 0) {
                if (message_entry->msgid == msg.msgId) {
                    if (msg.hdr->msgType == AJ_MSG_METHOD_CALL) {
                        // build a method reply
                        AJ_Message reply;
                        status = AJ_MarshalReplyMsg(&msg, &reply);

                        if (status == AJ_OK) {
                            status = (message_entry->handler)(&msg, &reply);
                        }

                        if (status == AJ_OK) {
                            status = AJ_DeliverMsg(&reply);
                        }
                    } else {
                        // call the handler!
                        status = (message_entry->handler)(&msg, NULL);
                    }

                    handled = TRUE;
                }

                ++message_entry;
            }

            // we need to check whether this is a property getter or setter.
            // these are stored in an array because multiple getters and setters can exist if running more than one bus object
            while (handled != TRUE && prop_entry->msgid != 0) {
                if (prop_entry->msgid == msg.msgId) {
                    // extract the method from the ID; GetProperty or SetProperty
                    uint32_t method = prop_entry->msgid & 0x000000FF;
                    if (method == AJ_PROP_GET) {
                        status = AJ_BusPropGet(&msg, prop_entry->callback, prop_entry->context);
                    } else if (method == AJ_PROP_SET) {
                        status = AJ_BusPropSet(&msg, prop_entry->callback, prop_entry->context);
                    } else {
                        // this should never happen!!!
                        AJ_ASSERT(!"Invalid property method");
                    }

                    handled = TRUE;
                }

                ++prop_entry;
            }

            // handler not found!
            if (handled == FALSE) {
                if (msg.msgId == AJ_METHOD_ACCEPT_SESSION) {
                    uint8_t accepted = (config->acceptor)(&msg);
                    status = AJ_BusReplyAcceptSession(&msg, accepted);
                } else {
                    AJ_InfoPrintf(("AJ_RunAllJoynService(): AJ_BusHandleBusMessage()\n"));
                    status = AJ_BusHandleBusMessage(&msg);
                }
            }

            // Any received packets indicates the link is active, so call to reinforce the bus link state
            AJ_NotifyLinkActive();
        }
        /*
         * Unarshaled messages must be closed to free resources
         */
        AJ_CloseMsg(&msg);

        if ((status == AJ_ERR_READ) || (status == AJ_ERR_LINK_DEAD)) {
            AJ_InfoPrintf(("AJ_RunAllJoynService(): AJ_Disconnect(): daemon \"%s\"\n", AJ_GetUniqueName(bus)));
            AJ_Disconnect(bus);
            connected = FALSE;
            if (config->connection_handler != NULL) {
                (config->connection_handler)(connected);
            }
            /*
             * Sleep a little while before trying to reconnect
             */
            AJ_InfoPrintf(("AJ_RunAllJoynService(): AJ_Sleep()\n"));
            AJ_Sleep(10 * 1000);
        }
    }

    // this will never actually return!
    return AJ_OK;
}

AJ_Status AJ_StartService(AJ_BusAttachment* bus,
                          const char* daemonName,
                          uint32_t timeout,
                          uint8_t connected,
                          uint16_t port,
                          const char* name,
                          uint32_t flags,
                          const AJ_SessionOpts* opts
                          )
{
    AJ_Status status;
    AJ_Time timer;
    uint8_t serviceStarted = FALSE;

    AJ_InfoPrintf(("AJ_StartService(bus=0x%p, daemonName=\"%s\", timeout=%d., connected=%d., port=%d., name=\"%s\", flags=0x%x, opts=0x%p)\n",
                   bus, daemonName, timeout, connected, port, name, flags, opts));

    AJ_InitTimer(&timer);

    while (TRUE) {
        if (AJ_GetElapsedTime(&timer, TRUE) > timeout) {
            return AJ_ERR_TIMEOUT;
        }
        if (!connected) {
            AJ_InfoPrintf(("AJ_StartService(): AJ_FindBusAndConnect()\n"));
            status = AJ_FindBusAndConnect(bus, daemonName, AJ_CONNECT_TIMEOUT);
            if (status != AJ_OK) {
                AJ_WarnPrintf(("AJ_StartService(): connect failed: sleeping for %d seconds\n", AJ_CONNECT_PAUSE / 1000));
                AJ_Sleep(AJ_CONNECT_PAUSE);
                continue;
            }
            AJ_InfoPrintf(("AJ_StartService(): connected to bus\n"));
        }
        /*
         * Kick things off by binding a session port
         */
        AJ_InfoPrintf(("AJ_StartService(): AJ_BindSessionPort()\n"));
        status = AJ_BusBindSessionPort(bus, port, opts, 0);
        if (status == AJ_OK) {
            break;
        }
        AJ_ErrPrintf(("AJ_StartService(): AJ_Disconnect(): status=%s.\n", AJ_StatusText(status)));
        AJ_Disconnect(bus);
    }

    while (!serviceStarted && (status == AJ_OK)) {
        AJ_Message msg;

        status = AJ_UnmarshalMsg(bus, &msg, AJ_UNMARSHAL_TIMEOUT);
        if (status == AJ_ERR_NO_MATCH) {
            // Ignore unknown messages
            status = AJ_OK;
            continue;
        }
        if (status != AJ_OK) {
            AJ_ErrPrintf(("AJ_StartService(): status=%s.\n", AJ_StatusText(status)));
            break;
        }

        switch (msg.msgId) {
        case AJ_REPLY_ID(AJ_METHOD_BIND_SESSION_PORT):
            if (msg.hdr->msgType == AJ_MSG_ERROR) {
                AJ_ErrPrintf(("AJ_StartService(): AJ_METHOD_BIND_SESSION_PORT: %s\n", msg.error));
                status = AJ_ERR_FAILURE;
            } else {
                AJ_InfoPrintf(("AJ_StartService(): AJ_BusRequestName()\n"));
                status = AJ_BusRequestName(bus, name, flags);
            }
            break;

        case AJ_REPLY_ID(AJ_METHOD_REQUEST_NAME):
            if (msg.hdr->msgType == AJ_MSG_ERROR) {
                AJ_ErrPrintf(("AJ_StartService(): AJ_METHOD_REQUEST_NAME: %s\n", msg.error));
                status = AJ_ERR_FAILURE;
            } else {
                AJ_InfoPrintf(("AJ_StartService(): AJ_BusAdvertiseName()\n"));
                status = AJ_BusAdvertiseName(bus, name, (opts != NULL) ? opts->transports : AJ_TRANSPORT_ANY, AJ_BUS_START_ADVERTISING, 0);
            }
            break;

        case AJ_REPLY_ID(AJ_METHOD_ADVERTISE_NAME):
            if (msg.hdr->msgType == AJ_MSG_ERROR) {
                AJ_ErrPrintf(("AJ_StartService(): AJ_METHOD_ADVERTISE_NAME: %s\n", msg.error));
                status = AJ_ERR_FAILURE;
            } else {
                serviceStarted = TRUE;
            }
            break;

        default:
            /*
             * Pass to the built-in bus message handlers
             */
            AJ_InfoPrintf(("AJ_StartService(): AJ_BusHandleBusMessage()\n"));
            status = AJ_BusHandleBusMessage(&msg);
            break;
        }
        AJ_CloseMsg(&msg);
    }

    if (status == AJ_OK) {
        status = AJ_AboutInit(bus, port);
    } else {
        AJ_WarnPrintf(("AJ_StartService(): AJ_Disconnect(): status=%s\n", AJ_StatusText(status)));
        AJ_Disconnect(bus);
    }
    return status;
}

static
AJ_Status StartClient(AJ_BusAttachment* bus,
                      const char* daemonName,
                      uint32_t timeout,
                      uint8_t connected,
                      const char* name,
                      uint16_t port,
                      const char** interfaces,
                      uint32_t* sessionId,
                      char* serviceName,
                      const AJ_SessionOpts* opts,
                      char* fullName)
{
    AJ_Status status = AJ_OK;
    AJ_Time timer;
    uint8_t found = FALSE;
    uint8_t clientStarted = FALSE;
    uint32_t elapsed = 0;
    char* rule;
    size_t ruleLen;
    const char* base = "interface='org.alljoyn.About',sessionless='t'";
    const char* impl = ",implements='";
    const char** ifaces;

    AJ_InfoPrintf(("AJ_StartClient(bus=0x%p, daemonName=\"%s\", timeout=%d., connected=%d., interface=\"%p\", sessionId=0x%p, serviceName=0x%p, opts=0x%p)\n",
                   bus, daemonName, timeout, connected, interfaces, sessionId, serviceName, opts));

    AJ_InitTimer(&timer);

    if ((name != NULL) && (interfaces != NULL)) {
        return AJ_ERR_INVALID;
    }

    while (elapsed < timeout) {
        if (!connected) {
            status = AJ_FindBusAndConnect(bus, daemonName, AJ_CONNECT_TIMEOUT);
            elapsed = AJ_GetElapsedTime(&timer, TRUE);
            if (status != AJ_OK) {
                elapsed += AJ_CONNECT_PAUSE;
                if (elapsed > timeout) {
                    break;
                }
                AJ_WarnPrintf(("AJ_StartClient(): Failed to connect to bus, sleeping for %d seconds\n", AJ_CONNECT_PAUSE / 1000));
                AJ_Sleep(AJ_CONNECT_PAUSE);
                continue;
            }
            AJ_InfoPrintf(("AJ_StartClient(): AllJoyn client connected to bus\n"));
        }
        if (name != NULL) {
            /*
             * Kick things off by finding the service names
             */
            status = AJ_BusFindAdvertisedName(bus, name, AJ_BUS_START_FINDING);
            AJ_InfoPrintf(("AJ_StartClient(): AJ_BusFindAdvertisedName()\n"));
        } else {
            /*
             * Kick things off by registering for the Announce signal.
             * Optionally add the implements clause per given interface
             */
            ruleLen = strlen(base) + 1;
            if (interfaces != NULL) {
                ifaces = interfaces;
                while (*ifaces != NULL) {
                    ruleLen += strlen(impl) + strlen(*ifaces) + 1;
                    ifaces++;
                }
            }
            rule = (char*) AJ_Malloc(ruleLen);
            if (rule == NULL) {
                status = AJ_ERR_RESOURCES;
                break;
            }
            strcpy(rule, base);
            if (interfaces != NULL) {
                ifaces = interfaces;
                while (*ifaces != NULL) {
                    strcat(rule, impl);
                    if ((*ifaces)[0] == '$') {
                        strcat(rule, &(*ifaces)[1]);
                    } else {
                        strcat(rule, *ifaces);
                    }
                    strcat(rule, "'");
                    ifaces++;
                }
            }
            status = AJ_BusSetSignalRule(bus, rule, AJ_BUS_SIGNAL_ALLOW);
            AJ_InfoPrintf(("AJ_StartClient(): Client SetSignalRule: %s\n", rule));
            AJ_Free(rule);
        }
        if (status == AJ_OK) {
            break;
        }
        if (!connected) {
            AJ_WarnPrintf(("AJ_StartClient(): Client disconnecting from bus: status=%s.\n", AJ_StatusText(status)));
            AJ_Disconnect(bus);
        }
    }
    if (elapsed > timeout) {
        AJ_WarnPrintf(("AJ_StartClient(): Client timed-out trying to connect to bus: status=%s.\n", AJ_StatusText(status)));
        return AJ_ERR_TIMEOUT;
    }
    timeout -= elapsed;

    if (status != AJ_OK) {
        return status;
    }

    *sessionId = 0;
    if (serviceName != NULL) {
        *serviceName = '\0';
    }

    while (!clientStarted && (status == AJ_OK)) {
        AJ_Message msg;
        status = AJ_UnmarshalMsg(bus, &msg, AJ_UNMARSHAL_TIMEOUT);
        if ((status == AJ_ERR_TIMEOUT) && !found) {
            /*
             * Timeouts are expected until we find a name or service
             */
            if (timeout < AJ_UNMARSHAL_TIMEOUT) {
                return status;
            }
            timeout -= AJ_UNMARSHAL_TIMEOUT;
            status = AJ_OK;
            continue;
        }
        if (status == AJ_ERR_NO_MATCH) {
            // Ignore unknown messages
            status = AJ_OK;
            continue;
        }
        if (status != AJ_OK) {
            AJ_ErrPrintf(("AJ_StartClient(): status=%s\n", AJ_StatusText(status)));
            break;
        }
        switch (msg.msgId) {

        case AJ_REPLY_ID(AJ_METHOD_FIND_NAME):
        case AJ_REPLY_ID(AJ_METHOD_FIND_NAME_BY_TRANSPORT):
            if (msg.hdr->msgType == AJ_MSG_ERROR) {
                AJ_ErrPrintf(("AJ_StartClient(): AJ_METHOD_FIND_NAME: %s\n", msg.error));
                status = AJ_ERR_FAILURE;
            } else {
                uint32_t disposition;
                AJ_UnmarshalArgs(&msg, "u", &disposition);
                if ((disposition != AJ_FIND_NAME_STARTED) && (disposition != AJ_FIND_NAME_ALREADY)) {
                    AJ_ErrPrintf(("AJ_StartClient(): AJ_ERR_FAILURE\n"));
                    status = AJ_ERR_FAILURE;
                }
            }
            break;

        case AJ_SIGNAL_FOUND_ADV_NAME:
            {
                AJ_Arg arg;
                AJ_UnmarshalArg(&msg, &arg);
                AJ_InfoPrintf(("FoundAdvertisedName(%s)\n", arg.val.v_string));
                if (!found) {
                    if (fullName) {
                        strncpy(fullName, arg.val.v_string, arg.len);
                        fullName[arg.len] = '\0';
                    }
                    found = TRUE;
                    status = AJ_BusJoinSession(bus, arg.val.v_string, port, opts);
                }
            }
            break;

        case AJ_SIGNAL_ABOUT_ANNOUNCE:
            {
                uint16_t aboutVersion, aboutPort;
#ifdef ANNOUNCE_BASED_DISCOVERY
                status = AJ_AboutHandleAnnounce(&msg, &aboutVersion, &aboutPort, serviceName, &found);
                if (interfaces != NULL) {
                    found = TRUE;
                }
                if ((status == AJ_OK) && (found == TRUE)) {
                    AJ_InfoPrintf(("AJ_StartClient(): AboutAnnounce from (%s) About Version: %d Port: %d\n", msg.sender, aboutVersion, aboutPort));
#else
                AJ_InfoPrintf(("AJ_StartClient(): AboutAnnounce from (%s)\n", msg.sender));
                if (!found) {
                    found = TRUE;
                    AJ_UnmarshalArgs(&msg, "qq", &aboutVersion, &aboutPort);
                    if (serviceName != NULL) {
                        strncpy(serviceName, msg.sender, AJ_MAX_NAME_SIZE);
                        serviceName[AJ_MAX_NAME_SIZE] = '\0';
                    }
#endif
                    /*
                     * Establish a session with the provided port.
                     * If port value is 0 use the About port unmarshalled from the Announcement instead.
                     */
                    if (port == 0) {
                        status = AJ_BusJoinSession(bus, msg.sender, aboutPort, opts);
                    } else {
                        status = AJ_BusJoinSession(bus, msg.sender, port, opts);
                    }
                    if (status != AJ_OK) {
                        AJ_ErrPrintf(("AJ_StartClient(): BusJoinSession failed (%s)\n", AJ_StatusText(status)));
                    }
                }
            }
            break;

        case AJ_REPLY_ID(AJ_METHOD_JOIN_SESSION):
            {
                uint32_t replyCode;

                if (msg.hdr->msgType == AJ_MSG_ERROR) {
                    AJ_ErrPrintf(("AJ_StartClient(): AJ_METHOD_JOIN_SESSION: %s\n", msg.error));
                    status = AJ_ERR_FAILURE;
                } else {
                    status = AJ_UnmarshalArgs(&msg, "uu", &replyCode, sessionId);
                    if (replyCode == AJ_JOINSESSION_REPLY_SUCCESS) {
                        clientStarted = TRUE;
                    } else {
                        AJ_ErrPrintf(("AJ_StartClient(): AJ_METHOD_JOIN_SESSION reply (%d)\n", replyCode));
                        status = AJ_ERR_FAILURE;
                    }
                }
            }
            break;

        case AJ_SIGNAL_SESSION_LOST_WITH_REASON:
            /*
             * Force a disconnect
             */
            {
                uint32_t id, reason;
                AJ_UnmarshalArgs(&msg, "uu", &id, &reason);
                AJ_InfoPrintf(("Session lost. ID = %u, reason = %u", id, reason));
            }
            AJ_ErrPrintf(("AJ_StartClient(): AJ_SIGNAL_SESSION_LOST_WITH_REASON: AJ_ERR_READ\n"));
            status = AJ_ERR_READ;
            break;

        default:
            /*
             * Pass to the built-in bus message handlers
             */
            AJ_InfoPrintf(("AJ_StartClient(): AJ_BusHandleBusMessage()\n"));
            status = AJ_BusHandleBusMessage(&msg);
            break;
        }
        AJ_CloseMsg(&msg);
    }
    if (status != AJ_OK && !connected) {
        AJ_WarnPrintf(("AJ_StartClient(): Client disconnecting from bus: status=%s\n", AJ_StatusText(status)));
        AJ_Disconnect(bus);
    }
    return status;
}

AJ_Status AJ_StartClientByName(AJ_BusAttachment* bus,
                               const char* daemonName,
                               uint32_t timeout,
                               uint8_t connected,
                               const char* name,
                               uint16_t port,
                               uint32_t* sessionId,
                               const AJ_SessionOpts* opts,
                               char* fullName)
{
    return StartClient(bus, daemonName, timeout, connected, name, port, NULL, sessionId, NULL, opts, fullName);
}

AJ_Status AJ_StartClient(AJ_BusAttachment* bus,
                         const char* daemonName,
                         uint32_t timeout,
                         uint8_t connected,
                         const char* name,
                         uint16_t port,
                         uint32_t* sessionId,
                         const AJ_SessionOpts* opts)
{
    AJ_WarnPrintf(("AJ_StartClient(): This function is deprecated. Please use AJ_StartClientByName() instead\n"));
    return StartClient(bus, daemonName, timeout, connected, name, port, NULL, sessionId, NULL, opts, NULL);
}

AJ_Status AJ_StartClientByInterface(AJ_BusAttachment* bus,
                                    const char* daemonName,
                                    uint32_t timeout,
                                    uint8_t connected,
                                    const char** interfaces,
                                    uint32_t* sessionId,
                                    char* uniqueName,
                                    const AJ_SessionOpts* opts)
{
    return StartClient(bus, daemonName, timeout, connected, NULL, 0, interfaces, sessionId, uniqueName, opts, NULL);
}

#ifdef ANNOUNCE_BASED_DISCOVERY
AJ_Status AJ_StartClientByPeerDescription(AJ_BusAttachment* bus,
                                          const char* daemonName,
                                          uint32_t timeout,
                                          uint8_t connected,
                                          const AJ_AboutPeerDescription* peerDesc,
                                          uint16_t port,
                                          uint32_t* sessionId,
                                          char* uniqueName,
                                          const AJ_SessionOpts* opts)
{
    if (peerDesc != NULL) {
        AJ_AboutRegisterAnnounceHandlers(peerDesc, 1);
    }
    return StartClient(bus, daemonName, timeout, connected, NULL, port, NULL, sessionId, uniqueName, opts, NULL);
}
#endif
