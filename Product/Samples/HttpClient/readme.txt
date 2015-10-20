The MIT License (MIT)

Copyright (c) 2015 Microsoft Corporation

Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 THE SOFTWARE.

HttpClient Sample
=================
This sample demonstrates the following:
-  Using the Micro Framework’s Http and Https classes to create an Http or Https 
   client.
-  Using an Http or Https client to retrieve pages from several different URLs 
   using Http and Https.

This sample runs on the following devices:
-  The .NET Micro Framework SDK 4.0 Emulator.
-  Any physical device that implements the Sockets API and the Ethernet 
   controller API and provides a valid configuration for the Ethernet 
   controller.
-  Any emulator that implements the Microsoft.SPOT.Emulator.Sockets 
   extensibility interface.
- SSL support for testing the secured sockets capabilities

To explore this sample:
1. Open HttpClient.sln in Visual Studio.

2. To run this sample in the Emulator, open the project Properties page, click 
   the .NET Micro Framework tab, set the Transport property to Emulator, and 
   then select the emulator you want to use.

   To run this sample on a device, open the project Properties page, click the 
   .NET Micro Framework tab, set the Transport property to the transport that 
   your device supports, and then select the device you want to target. 

3. In the Build menu, select Start Debugging (or press F5).

4. In Visual Studio, show the Output window for Debug.  The content of some Web 
   pages is printed in the Output window.

5. Close the application.

6. In file HttpClient.cs, set a breakpoint on the PrintHttpData method.

7. Press F5 to start debugging the HTTP requests and responses. 

For best results, run the sample on a device.  Any actual application that uses 
this code will require modifications of this code.  This solution provides 
sample code, rather than ship-ready code, and is provided for instructional 
purposes only.
