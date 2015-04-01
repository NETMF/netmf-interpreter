Clock Sample
============
This sample demonstrates the following:
-  Using the basic features of time.
-  Synchronizing device time to a public time server.

This sample runs on the following devices:
-  The .NET Micro Framework SDK Emulator.
-  Any physical device that implements the Time, Ethernet and Sockets API and
   provides a valid configuration for the Ethernet controller.
-  Any emulator that implements the Microsoft.SPOT.Emulator.Time and
   Microsoft.SPOT.Emulator.Sockets extensibility interface.

In our sample we will use an arbitrary time server with address time-nw.nist.gov,
one of the public time servers listed here: http://tf.nist.gov/tf-cgi/servers.cgi
You might need to change your proxy settings, to access the public time server.


To build and run this sample:
1. Open Clock.sln in Visual Studio.

2. To run this sample in the Emulator, open the project Properties page, click
   the .NET Micro Framework tab, set the Transport property to Emulator, and
   then select the emulator you want to use.

   To run this sample on a device, open the project Properties page, click the
   .NET Micro Framework tab, set the Transport property to the transport that
   your device supports, and then select the device you want to target.

3. In the Debug menu, select Start Debugging (or press F5). A digital clock
   appears on the device screen.

4. Click the Up, Down, or Select (middle) buttons on the device.
      Up:     Resets the clock to an arbitrary local time.
      Select: Synchronizes the clock to the server time.
      Down:   Sets the clock to sync periodically with the server time.


For best results, run the sample on a device.  Any actual application that uses
this code will require modifications of this code.  This solution provides
sample code, rather than ship-ready code, and is provided for instructional
purposes only.
