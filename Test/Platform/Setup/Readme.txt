
==============================================================================
                              Release Notes for the 
        Microsoft .NET Micro Framework Platform Test Automation Suite
                   (C) Copyright 2008 Microsoft Corporation
------------------------------------------------------------------------------

Thank you for installing and using the .NET Micro Framework Platform Test 
Automation Suite. This document describes how to get started, where you can go 
for more information, system requirements, and a list of known issues at 
release time.


==============================================================================
Installation Instructions
------------------------------------------------------------------------------
1. Install the .NET Micro Framework SDK, if it is not installed already, from
http://msdn.microsoft.com/embedded/netmf.
2. Use Windows Explorer to navigate to 
<%spoclient%>\BuildOutput\public\Debug\Test\Server\msm.
3. Double-click the file MicroFrameworkTestKit.msi to install the Test Automation 
Suite.


==============================================================================
Getting Started
------------------------------------------------------------------------------

Test Suite Location
------------------------------------------------------------------------------
To access the Platform Test Automation Suite, select the Start menu and then 
choose All Programs. Next, click Microsoft .NET Micro Framework followed by 
Tests. The Tests folder contains three items:

1.  A cmd window link named .NET Micro Framework Test SDK Command Prompt
2.  Release notes.
3.  A Test Cases folder that has the following subfolders:
    -Assemblies - Contains test assemblies that aid in the execution of the 
                  Test Automation.
    -Results - Holds the test results for each test run.
    -TestCases - Contains all of the test cases for the Test Automation Suite.
    -Tools - This is a directory of desktop tools that automate the execution 
             of tests using Visual Studio.

Running All of the Tests
------------------------------------------------------------------------------
To run the tests, open the .NET Micro Framework Test SDK Command Prompt by 
navigating to the Start menu and selecting All Programs then Microsoft .NET Micro
Framework followed by Tests then .NET Micro Framework Test SDK Command Prompt.

From the command line, run the program RunTests.exe. All of the tests will 
execute and a summary will be provided at the end with the test results.

When running tests on a device, there are four transports to target the tests 
towards: Emulator, Serial, TCPIP, or USB. Here are the commands to run the 
tests on each of these transports:

Emulator:  RunTests.exe -transport emulator -device Microsoft
Serial:      RunTests.exe -transport serial -device COM1 | COM2
TCPIP:    RunTests.exe -transport TCPIP -device 157.56.167.28 (Your IP 
                address will be different from the example listed here).
USB:       RunTests.exe -transport USB - device a7e70ea2  (Your -device ID 
                will be different from the example listed here).

Running a specific test
------------------------------------------------------------------------------
Once all of the tests are executed if any issues arise from executing the 
tests, it is possible to execute a single set of tests. To execute a single 
set of tests run:
RunTests.exe –test basic\basic.sln -transport tcpip -device 157.56.167.28 -showvswindow

Reading the Summary 
------------------------------------------------------------------------------
The results summary is color coded to display pass (green) and fail (red) test 
results.  The fail results can be clicked from the summary page, which will 
then bring up the log for the tests that failed.


==============================================================================
System Requirements 
------------------------------------------------------------------------------
Operating Systems:
Microsoft Windows XP or Microsoft Windows Server 2003 or Windows Vista  

Required Applications:
Microsoft Visual Studio 2008, Standard Edition or greater (C# must be installed) 
.NET Micro Framework SDK 4.0

==============================================================================
Known Issues at Release 
------------------------------------------------------------------------------
The result logs may contain extra text when a first chance exception is thrown or garbage collection occurs.  This extra text is intended to be informational only.

==============================================================================
Third-Party Technologies
------------------------------------------------------------------------------
Portions of this software are based in part on the work of the Independent JPEG Group. 
