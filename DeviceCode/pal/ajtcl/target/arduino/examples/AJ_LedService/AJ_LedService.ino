/**
 * @file
 */
/******************************************************************************
 * Copyright (c) 2013-2014, AllSeen Alliance. All rights reserved.
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

#undef WIFI_UDP_WORKING

#include <SPI.h>
#ifdef WIFI_UDP_WORKING
#include <WiFi.h>
#else
#include <Ethernet.h>
#endif

#include <alljoyn.h>

#include "due_led.h"
#include <stdint.h>

int led = 13;

#ifdef WIFI_UDP_WORKING
char ssid[] = "yourNetwork";     // the name of your network
int status = WL_IDLE_STATUS;     // the Wifi radio's status
#endif


void DUE_led_timed(uint32_t msec)
{
    AJ_AlwaysPrintf(("DUE_led_timed\n"));
    digitalWrite(led, HIGH); // turn the LED on (HIGH is the voltage level)
    delay(msec);             // wait for a second
    digitalWrite(led, LOW);  // turn the LED off by making the voltage LOW
    delay(msec);             // wait for a second
}

void DUE_led(uint8_t on)
{
    AJ_AlwaysPrintf(("DUE_led(%u)\n", on));
    digitalWrite(led, on ? HIGH : LOW); // turn the LED on (HIGH is the voltage level)
}


// the setup routine runs once when you press reset:
void setup() {
    // initialize the digital pin as an output.
    pinMode(led, OUTPUT);

    Serial.begin(115200);
    while (!Serial) ;

    digitalWrite(led, LOW);


#ifdef WIFI_UDP_WORKING
    // check for the presence of the shield:
    if (WiFi.status() == WL_NO_SHIELD) {
        AJ_AlwaysPrintf(("WiFi shield not present\n"));
        // don't continue:
        while (true) ;
    }

    // attempt to connect to Wifi network:
    while (wifiStatus != WL_CONNECTED) {
        Serial.print("Attempting to connect to open SSID: ");
        Serial.println(ssid);
        status = WiFi.begin(ssid);

        // wait 10 seconds for connection:
        delay(10000);

        IPAddress ip = WiFi.localIP();
        Serial.print("Connected: ");
        Serial.println(ip);
    }
#else
    byte mac[] = { 0x00, 0xAA, 0xBB, 0xCC, 0xDE, 0x02 };
    // start the Ethernet connection:
    if (Ethernet.begin(mac) == 0) {
        AJ_AlwaysPrintf(("Failed to configure Ethernet using DHCP\n"));
        // no point in carrying on, so do nothing forevermore:
        for (;;)
            ;
    }
#endif
}

// the loop routine runs over and over again forever:
void loop() {
    AJ_AlwaysPrintf(("Hello\n"));
    AJ_Main();
}


