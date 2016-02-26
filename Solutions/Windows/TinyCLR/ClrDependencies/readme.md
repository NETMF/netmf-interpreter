### ClrDependencies Project Overview
#### MAKEFILE PROJECT

This project does not contain any files, so there are none displayed in Solution Explorer.

##### Summary:  
This is a temporary answer to the larger problem of converting the project system to
full standard VS project and solution file with VCXPROJ files for native code.

>**NOTE:**  
>While this effort is helping to understand the requirements of shifting the build system the
>primary purpose of this Win32 Native project is on building and testing the OS level porting
>process for NETMF and not on converting the entire build. A future effort will take what
>is learend here and apply that to a full conversion of the tree.

##### Remarks:
The main executable is built from VS via TinyCLR.vcxproj. However the dependent libraries
are not currently built from VCXPROJ projects so something is needed to create the libraries
the app will depend on. That is where this project comes in. It wraps building a legacy NETMF
project in a standard VS "MAKEFILE" project so that the libraries can be generated from within
VS instead of having to constantly shift to a command line tool for building.
