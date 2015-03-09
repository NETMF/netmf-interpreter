Copyright (c) 2009 Microsoft Corporation. All rights reserved.



The Test\JTAG folder contains projects supporting testing scenarios.

The projects found under Apps are managed code unit test with project
files supporting compilation of ALL of the components for running
UNIT managed code test, TINYCLR, and drivers that are intended to be
deployed under JTAG.

Once the test application is deployed via JTAG, you'll be able to
debug your TINYCLR port while the managed code unit test drives
native-level functionality. You will not have visibility into the 
managed code level.

Managed Code Test

Use one of the provided managed code test. Alternatively, you may
create your own. However, you must design your managed code so that
it continuously runs as the provided applications are designed.

Use a small sleep in the initial step. 


How-to-Get-Ready

The requirement to run managed code began not-so-long ago, but certainly
you must be able to  build the TinyClr without error. The CLR may or
may not fully support execution, but it must be buildable without error. 




How-To-build


Build the TinyCLR without error. Now you are ready to build in an
application. 

   1. CD Solutions\<MYPORT>

   2. MSBUILD TINYCLR.PROJ /P:FLAVOR=DEBUG;MEMORY=RAM



Then, set the environment variable FORCEDAT so that it 
matches a project under Test\JTAG\Apps. 

For example: 

   use HelloWorld to send a debug print statement...  (start here)

   4. SET FORCEDAT=HellowWorld

   5. CD %SPOCLIENT%\Test\JTAG\Apps\HelloWorld

   6. MSBUILD HelloWorld.PROJ

   7. MSBUILD HelloWorld.DATPROJ

   

   4. MSBUILD TINYCLR.PROJ /P:FLAVOR=DEBUG;MEMORY=RAM

The image under BUILDOUTPUT\ARM\RVDS3.1\RAM\Debug\

TINYCLR.HEX or TINYCLR.AXF are ready to be run. 


  - Use TINYCLR.HEX with MFDEPLOY
  - Use TINYCLR.AXF with JTAG


Other Applications to Test

The JTAG application suite are coded and ready to use with project files. The
suite is ever growing including:

   Network  to run a simple socket test...
   LCD   to run a simple presentation drawing test...


Remember to switch applications:

    A. Set FORCEDAT=<name>
    B. MSBUILD <name>.proj
    C. MSBUILD <name>.datproj
    D. MSBUILD TINYCLR.PROJ /P:flavor=debug;memory=ram



Trouble-shooting 

1. My application doesn't appear in the TinycLR... 

   Building RAM-Debug images with applications is a useful tool, and there can be complications
   if the tree is partially populated with libraries. If your output is not appearing, 
   try cleaning the buildoutput tree. For example, if you are
   using RVDS compiler and IMXS solution...

   rd /s /q %SPOCLIENT%\Buildoutput\ARM\RVDS3.1\RAM\Debug\iMXS



2. How do I know if I built the assemblies correctly?

   The system assemblies are compiled into a DAT file under the Buildoutput\public folder. To
   confirm you have built them sucessfully, 

   Buildoutput\public>dir /s *.dat

   You may see:   ...\BuildOutput\public\release\client\dat\Tinyclr_helloworld.dat

   Secondly, you can also search for assemblies. If you shuffle the configuration files in a significant
   manner, the output can change. After building helloworld, you can search for the PE and EXE files.

   buildoutput>  Dir /s *HelloWorld*

   You will see:    Microsoft.SPOT.Test.HelloWorld.pe and many other files with diffierent suffixes.




