CustomUI Sample
============
This sample demonstrates the following:
-  Using the Touch Panel API.
-  Creating custom UIElement controls.
-  Registering for touch events.
-  Using the File System features of the Micro Framework.

This sample runs on the following devices:
-  The .NET Micro Framework SDK Emulator.
-  Any physical device that implements the Block Storage API and provides a 
   valid configuration for the block storage device.
-  Any emulator that implements the Microsoft.SPOT.BlockStorage interface.

To build and run this sample:
1. Open CustomUI.sln in Visual Studio.

2. To run this sample in the Emulator, open the project Properties page, click
   the .NET Micro Framework tab, set the Transport property to Emulator, and
   then select the emulator you want to use.

   To run this sample on a device, open the project Properties page, click the
   .NET Micro Framework tab, set the Transport property to the transport that
   your device supports, and then select the device you want to target.

3. In the Debug menu, select Start Debugging (or press F5). A representation of
   the file system appears on screen.

4. Tap any of the following elements on the display:
      New File: Creates a new file in the current directory with the name
                File_##.txt
      New Dir:  Creates a new directory in the current directory with the
                name Directory_##
      Delete:   Deletes the currently selected file or directory.
      Format:   Formats the selected volume (top-level directories only).
      Tapping on a file or directory selects that item.
      Tapping on a selected directory makes that the current directory.


For best results, run the sample on a device.  Any actual application that uses
this code will require modifications of this code.  This solution provides
sample code, rather than ship-ready code, and is provided for instructional
purposes only.
