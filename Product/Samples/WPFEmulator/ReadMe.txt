Emulator Sample
===============
This sample demonstrates the following:
- Building and installing a custom emulator.
- Using the emulator classes provided with the .NET Micro Framework.
- Using a Windows Form to create a user interface to simulate a product.
- Getting button input.
- Displaying output on an LCD display.


To build and run this sample:
1. Open SampleEmulator.sln in Visual Studio 2008.

2. In the Build menu, select Rebuild Solution.

3. Close SampleEmulator.sln. 

4. Open a regular sample, such as Puzzle.sln.

5. In Solution Explorer, right-click the project, select Properties, and then 
   select the .NET Micro Framework tab.  Click the Device drop-down list and 
   then select "Sample Emulator", which now appears in the list.

6. Open and run any sample, such as Puzzle.sln.  To do this, click the Debug 
   menu, and then click Start Without Debugging.  The sample is now running in 
   the sample emulator.

7. Attach the Visual Studio debugger to the running instance of the sample 
   emulator.  To do this, click the Debug menu, and then click Attach to 
   Process.  In the Available Processes list, select 
   Microsoft.SPOT.Emulator.Sample.SampleEmulator.exe.  Then click the Attach 
   button.

8. Set breakpoints on any extensibility interface implementation, such as 
   Microsoft.SPOT.Emulator.Time.TimeDriver.CurrentTime.  Source code is 
   available through the Micro Framework Porting Kit. 
