# .NET Micro Framework Interpreter  
Welcome to the .NET Micro Framework interpreter GitHub repository. 

## Getting Started
1. Clone or fork the repository from GitHub  
It is recommended to use at least one level of directory hierarchy from the root to accommodate the binaries from step #3
2. Download the [binary tools](http://netmf.github.io/downloads/build-tools.zip) zip file  
The zip contains binaries necessary to build the SDK and support files needed to build custom device images. Our longer term
goal is to remove the need for these binaries so they are a separate download at present.
3. Unzip the contents of the tools zip to the parent folder of the repository, that is the tools and bin folders in the zip
should become siblings of the folder containing the repository.

## Building the code

### Build Requirements:
1. To build the SDK you must have Visual Studio 2013 Community, Pro, or Ultimate editions and the Visual Studio 2013 SDK
If you want to generate a VSIX package for Visual Studio 2015 Preview, you must also have the Visual Studio 2015 SDK installed.

### Building the SDK
1. Open a command prompt and navigate to the root of the repository
2. Run the build_sdk.cmd command script to generate a new SDK MSI and VSIX packages.  
This will generate the SDK installer MSI and the VSIX packages in the BuildOutput folder

### Building a firmware solution
1. Open a command prompt and navigate to the root of the repository
2. run setenv_<toolset> <args> to set up the build environment for the tool-set you have.  
Example: `C:\GitHub\Netmf\netmf-interpreter>setenv_mdk 5.05`
3. navigate to the Solutions\<platformname> folder for the platform you wish to build
4. Select the build flavor of the tools  
`set FLAVOR_WIN=`(Release | Debug)
This step is new for anyone coming from previous versions. This is a temporary step as part of transitioning the build.
If you forget this you may get build errors about missing binaries. We intend to resolve this with further improvements
to the build.
5. Run msbuild to build the firmware  
While a simple `msbuild` with no arguments is enough it tends to make for a noisy display. An easier to follow and faster build
is achieved by providing the options for generating a detailed log file but use only minimal logging to the console window as
follows:  
`msbuild /flp:verbosity=detailed /clp:verbosity=minimal`

More info on building the framework and internal development guides will appear on the wiki. If you have content that
is relevant to the NETMF development community that you would like to contribute feel free to join in and participate in
the future of the .NET Micro Framework. 
