/******************************************************************************
 *
 *
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

#include <gtest/gtest.h>

extern "C" {
#include "aj_debug.h"
#include "alljoyn.h"
}

static const char* serviceName = "org.alljoyn.thinclient.test.theoffice";
static const uint16_t servicePort = 1984;

// The expectation is that there is a daemon running on the same machine.
// Hence, we should be able to discover and connect to it within 1.5 seconds.
static const uint16_t connectTimeout = 1500;

// We expect to receive replies very quickly from the local daemon.
static const uint16_t unmarshalTimeout = 250;

static AJ_BusAttachment testBus;

class BusAttachmentTest : public testing::Test {
  public:

    virtual void SetUp() {
        AJ_Status status = AJ_ERR_FAILURE;

        AJ_Initialize();

        // Connect to the bus
        status = AJ_Connect(&testBus, NULL, connectTimeout);
        // No point in continuing with any further tests, if we
        // cannot connect to the daemon. So ASSERT.
        ASSERT_EQ(AJ_OK, status) << "Unable to connect to the daemon. "
                                 << "The status returned is "
                                 << AJ_StatusText(status);

        if (AJ_OK == status) {
            AJ_AlwaysPrintf(("Connected to the bus. The unique name is %s\n", AJ_GetUniqueName(&testBus)));
        }
    }
    virtual void TearDown() {
        // Disconnect from the bus
        AJ_Disconnect(&testBus);
    }
};

// Helper function to test: RequestName and ReleaseName apis.
// It returns the 'disposition' in the reply to the method call.
uint32_t RequestReleaseNameTestHelper(const uint32_t method, const char* name, const uint32_t flags)
{
    // Initialized to an invalid value
    // (valid values are 1, 2, 3, 4 - See DBus spec)
    uint32_t returnValue = 0;

    AJ_Status status = AJ_ERR_FAILURE;

    switch (method) {
    case AJ_METHOD_REQUEST_NAME:
        status = AJ_BusRequestName(&testBus, name, flags);

        EXPECT_EQ(AJ_OK, status) << "Call to AJ_BusRequestName was not successful "
                                 << "when invoked with name: " << name
                                 << " and flags 0x" << std::hex
                                 << flags << ". Got status " << AJ_StatusText(status);

        break;

    case AJ_METHOD_RELEASE_NAME:
        status = AJ_BusReleaseName(&testBus, name);

        EXPECT_EQ(AJ_OK, status) << "Call to AJ_BusReleaseName was not successful "
                                 << "when invoked with name: " << name
                                 << ". Got status " << AJ_StatusText(status);

        break;
    }

    // Given the message loop nature of the code, we need to wait
    // until we get a reply for the call that we made and then check
    // that result.  We could get other notifications from
    // the bus during this wait.
    // However, we don't want to wait indefinitely and hence we
    // need some kind of a time out.
    // Also once we get what we are waiting for, there is no point
    // in waiting any further.
    bool yetToReceiveReplyInterestedIn = true;
    AJ_Time timer;
    AJ_InitTimer(&timer);
    const uint16_t loopTimeoutValue = 500; // five hundred milliseconds

    while (yetToReceiveReplyInterestedIn && AJ_GetElapsedTime(&timer, TRUE) < loopTimeoutValue) {
        AJ_Message msg;

        status = AJ_UnmarshalMsg(&testBus, &msg, unmarshalTimeout);
        EXPECT_EQ(AJ_OK, status) << "Unable to unmarshal a message with in "
                                 << unmarshalTimeout << "ms. Got status "
                                 << AJ_StatusText(status);

        if (AJ_OK != status) {
            // No point continuing the test if we are unable to unmarshal.
            break;
        }

        uint32_t disposition = 0;
        switch (msg.msgId) {
        case AJ_REPLY_ID(AJ_METHOD_REQUEST_NAME):
            EXPECT_EQ(AJ_MSG_METHOD_RET, msg.hdr->msgType) << "The response to RequestName method "
                                                           << "call was not a method return.";
            status = AJ_UnmarshalArgs(&msg, "u", &disposition);
            EXPECT_EQ(AJ_OK, status) << "Unable to unmarshal args from "
                                     << "RequestName reply msg.";

            returnValue = disposition;

            yetToReceiveReplyInterestedIn = false;
            break;

        case AJ_REPLY_ID(AJ_METHOD_RELEASE_NAME):
            EXPECT_EQ(AJ_MSG_METHOD_RET, msg.hdr->msgType) << "The response to ReleaseName method "
                                                           << "call was not a method return.";
            status = AJ_UnmarshalArgs(&msg, "u", &disposition);
            EXPECT_EQ(AJ_OK, status) << "Unable to unmarshal args from "
                                     << "ReleaseName reply msg.";

            returnValue = disposition;

            yetToReceiveReplyInterestedIn = false;
            break;

        default:
            // Any other message sent from the bus should be handled
            // as a bus message.
            status = AJ_BusHandleBusMessage(&msg);
            EXPECT_EQ(AJ_OK, status) << "The bus message was not handled "
                                     << "correctly. Got status "
                                     << AJ_StatusText(status);
            break;
        }
        status = AJ_CloseMsg(&msg);
        EXPECT_EQ(AJ_OK, status) << "Unable to close message. Got status "
                                 << AJ_StatusText(status);
    }

    return returnValue;
}

static const uint32_t DBUS_REQUEST_NAME_REPLY_PRIMARY_OWNER = 1;
static const uint32_t DBUS_REQUEST_NAME_REPLY_IN_QUEUE = 2;
static const uint32_t DBUS_REQUEST_NAME_REPLY_EXISTS = 3;
static const uint32_t DBUS_REQUEST_NAME_REPLY_ALREADY_OWNER = 4;

TEST_F(BusAttachmentTest, RequestName)
{
    // Request names that are already taken
    EXPECT_EQ(DBUS_REQUEST_NAME_REPLY_IN_QUEUE, RequestReleaseNameTestHelper(AJ_METHOD_REQUEST_NAME, AJ_DBusDestination, AJ_NAME_REQ_REPLACE_EXISTING))
        << "Did not get 'in queue' while requesting an already existing name: "
        << AJ_DBusDestination;

    EXPECT_EQ(DBUS_REQUEST_NAME_REPLY_EXISTS, RequestReleaseNameTestHelper(AJ_METHOD_REQUEST_NAME, AJ_BusDestination, AJ_NAME_REQ_ALLOW_REPLACEMENT | AJ_NAME_REQ_DO_NOT_QUEUE))
        << "Did not get 'name already exists' while requesting an already existing name: "
        << AJ_BusDestination;

    // Request a new name that is not taken
    uint32_t disposition = RequestReleaseNameTestHelper(AJ_METHOD_REQUEST_NAME, serviceName, 0x0);
    EXPECT_EQ(DBUS_REQUEST_NAME_REPLY_PRIMARY_OWNER, disposition)
        << "Unable to request a well-known name: " << serviceName;

    if (DBUS_REQUEST_NAME_REPLY_PRIMARY_OWNER == disposition) {
        // Request the same name again
        EXPECT_EQ(DBUS_REQUEST_NAME_REPLY_ALREADY_OWNER, RequestReleaseNameTestHelper(AJ_METHOD_REQUEST_NAME, serviceName, 0x0))
            << "Did not get 'name already owner' while requesting an already requested name:"
            << serviceName;
    }

}

static const uint32_t DBUS_RELEASE_NAME_REPLY_RELEASED = 1;
static const uint32_t DBUS_RELEASE_NAME_REPLY_NON_EXISTENT = 2;
static const uint32_t DBUS_RELEASE_NAME_REPLY_NOT_OWNER = 3;

TEST_F(BusAttachmentTest, ReleaseName)
{
    // Release a non-existent name
    const char* nonExistentName = "org.freeedesktop.DBus";
    EXPECT_EQ(DBUS_RELEASE_NAME_REPLY_NON_EXISTENT, RequestReleaseNameTestHelper(AJ_METHOD_RELEASE_NAME, nonExistentName, 0x0))
        << "Did not get 'name non existent' when tried to release the name: "
        << nonExistentName;

    // Release a name for which we are not an owner
    EXPECT_EQ(DBUS_RELEASE_NAME_REPLY_NOT_OWNER, RequestReleaseNameTestHelper(AJ_METHOD_RELEASE_NAME, AJ_BusDestination, 0x0))
        << "Did not get 'name not owner' when tried to release the name: "
        << AJ_BusDestination;

    // Request and Release a unique name
    uint32_t disposition = RequestReleaseNameTestHelper(AJ_METHOD_REQUEST_NAME, serviceName, 0x0);
    EXPECT_EQ(DBUS_REQUEST_NAME_REPLY_PRIMARY_OWNER, disposition)
        << "Unable to request a well-known name: " << serviceName;

    if (DBUS_REQUEST_NAME_REPLY_PRIMARY_OWNER == disposition) {
        // release the name
        EXPECT_EQ(DBUS_RELEASE_NAME_REPLY_RELEASED, RequestReleaseNameTestHelper(AJ_METHOD_RELEASE_NAME, serviceName, 0x0))
            << "Unable to release the well-known name: " << serviceName;
    }
}
