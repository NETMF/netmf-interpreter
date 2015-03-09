////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "ntp.h"

//--//

INT32 HAL_TIMESERVICE_GetTimeFromSNTPServer(UINT8* serverIP, SYSTEMTIME* systemTime)
{
    UINT32 ip = 0;
    INT32 i = 0;
    for (i = 0; i < 4; i++)
    {
        ip <<= 8;
        ip |= serverIP[i];
    }

    SNTPClient client(ip);
    HRESULT res = client.Connect();
    if(res == HAL_TIMESERVICE_SUCCESS) {
        //TIME localClockOffset =  client.getLocalClockOffset();
        //TIME t = client.getTransmitTimestamp() +  localClockOffset;
        // TODO: keep track of the transmit / receive delay with local clock offset
        TIME t = client.getTransmitTimestamp() - TIME_ZONE_OFFSET;
        Time_ToSystemTime(t, systemTime);
    }
    
    return res;
}

INT32 HAL_TIMESERVICE_GetTimeFromSNTPServerList(UINT8* serverIP, INT32 serverNum, SYSTEMTIME* systemTime)
{   
    // only two servers supported
    serverNum = __min(2, serverNum);
    UINT32 ip[2]; ip[0] = 0; ip[1] = 0;

    ip[0] |= serverIP[0]; ip[0] <<= 8;
    ip[0] |= serverIP[1]; ip[0] <<= 8;
    ip[0] |= serverIP[2]; ip[0] <<= 8;
    ip[0] |= serverIP[3];

    if(serverNum > 1) {
        ip[1] |= serverIP[4]; ip[1] <<= 8;
        ip[1] |= serverIP[5]; ip[1] <<= 8;
        ip[1] |= serverIP[6]; ip[1] <<= 8;
        ip[1] |= serverIP[7];
    }

    SNTPClient client(ip, serverNum);
    HRESULT res = client.Connect();
    if(res == HAL_TIMESERVICE_SUCCESS) {
        //TIME localClockOffset =  client.getLocalClockOffset();
        //TIME t = client.getTransmitTimestamp() +  localClockOffset;
        // TODO: keep track of the transmit / receive delay with local clock offset
        TIME t = client.getTransmitTimestamp() - TIME_ZONE_OFFSET;
        Time_ToSystemTime(t, systemTime);
    }
    
    return res;
}

