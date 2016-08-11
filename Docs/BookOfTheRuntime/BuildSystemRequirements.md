# Build System Requirements for the .NET Micro Framework
In order to fully understand the various aspects of the build and the new proposal it is important to understand what the system
actually does, what problems it solves without considering how it solves them (e.g. a functional spec for the system). The following
section serves as a high level guide to understanding the needs of the build infrastructure for the .NET Micro Framework. Much of the
needs are fairly common and straight forward requirements of any software build system. Some aspects are unique to the needs of embedded
micro controllers, and a few are specific to NETMF itself.

## A brief history of the system (as of v4.4)
The current system began as a purely MAKEFILE based build, since MSBuild didn't exist at the time. When the Micro Framework
was moving out of an internal product to a general purpose platform for use by third parties there was strong demand to move
to leverage MSBuild for the build system. Unfortunately, at that time, the Visual C++ project system in Visual Studio did not
use MSBuild and the NETMF interpreter and HAL are written in C and C++. Thus, for native code, a custom build was required.
Given the amount of work involved in creating a completely custom system the original approach taken was to essentially,
transliterate the MAKEFILES into MSBuild form to get a new system up and going quickly, which could then be updated/modified
as needed. While this has continued to evolve over time and served the NETMF for many years it isn't without issues. The rest
of this topic discusses the issues and maps out a plan for a new build infrastructure for the future of NETMF.

## What does the build system do?
There are several different top level scenarios for the build system:

1. Building the build infrastructure components needed to build the rest of the system
  - This includes building any extensions of the build infrastructure that is used but
     isn't built into the build tool used.
1. Building the core tools used to build NETMF code
  - While there are several such tools in NETMF the one most users will be familiar with is MetadataProcessor
1. Building the SDK used by developers to build applications deployed to NETMF
1. Building the Firmware images for a .NET Micro Framework device
1. Building applications written by third parties to deploy to a NETMF enabled device

## Target Architecture Neutrality
It is important to note that the .NET Micro framework is a target architecture neutral platform. That is, NETMF is designed and intended to
run on any 32 bit target architecture including both little-endian and big-endian systems. Furthermore, since NETMF is an Open Source project
it needs to support the use of Open Source compilers and tools. Thus, the NETMF build system for C, C++ and assembly code must be extensible
to include new tool chains for new architectures as well as multiple OSS and commercial compilers for each architecture. This doesn't mean
that the build has to implement all such support, just that it must allow for someone to add it without breaking the existing systems. That
is the build must allow for extensibility of the compilation tool chain used and must allow the user to select which one to use if more than
one is available for a given target. Ideally, the system could detect which tool sets are installed and automatically select the tool set if
only one is available for a given target architecture (The current NETMF firmware build does not have such an optimization).

## Building infrastructure components
This stage of the build is intended to resolve the ["Chicken vs. Egg" problem](https://en.wikipedia.org/wiki/Chicken_or_the_egg#Chicken-and-egg_problem).
This stage is run solely to build components of the build infrastructure that are needed to build the rest of the system. Due to the fact that they are
needed within the build itself the build of these components cannot be expressed as simple dependencies. A reasonable design goal for any build system is
to keep the number of such components as small as possible. Ideally there are none. For build systems like MSBuild there is a catch with this stage. Since,
the build tasks are loaded into MSBuild itself as managed assemblies they cannot be used by the MSBuild instance that is building them. Furthermore, since
MSBuild has an optimization where it keeps a copy of itself running for a period of time after a build completes (to help optimize load times when building
multiple projects) any previous instances of MSBuild must be shutdown before a build of the tasks can complete, since the DLLs being built may still be
loaded into MSBuild. 

## Building Core Tools
There are a number of tools used throughout the NETMF build process, the most extensively used is MetadataProcessor.exe, which is a bit of a "Jack of all trades"
tool for NETMF. These tools must be built before the other parts of the build can complete. These are generally desktop console based applications for performing
various tasks during the build and are independent of the build system itself. (e.g. they are standalone executables that can be run from the command line rather
than MSBuild specific tasks etc...) In the current system the custom MSbuild tasks for NETMF are generally MSBuild Task wrappers around the command line tools.
Ideally, since these tools are standard desktop applications they can, and should just build using the standard Visual Studio project files. 

## Building the SDK
There are a number of stages to building the .NET Micro Framework SDK

1. Build the Prerequisites
  - This includes steps 1 & 2 from the top level of the build (building infrastructure components and the core tools)
1. Compile framework assemblies into DLLs
1. Convert framework assembly DLLs into PE files with debug symbols info for big-endian and little-endian systems
1. Building the VS integration components for supported versions of Visual Studio
1. Signing all the desktop tools and components before packaging 
1. Building VSIX packages for supported Visual Studio versions
1. Signing the VSIX packages
1. Building the SDK MSI package
1. Signing the SDK MSI package

NOTE: The signing process must be done in multiple phases, the first phase signs the contents of the packages (MSI, VSIX, NUGET, ...)
second phase bundles the signed contents into the package and then signs the final package if the container supports signing (i.e. MSI and
VSIX support signing the package, NUGET does not)

NOTE: There is another Chicken and Egg problem in the SDK. Building the NETMF assemblies can't use normal SDK project files since it is building
the SDK itself. Thus either the way the SDK works would need to change or the Assemblies included in the SDK need to have special project files
in order to resolve references to the other SDK assemblies (especially mscorlib) [The build system future thinking topic covers some ideas on how to resolve this]

## OEM Builds for device firmware
The build infrastructure for NETMF has many functions, some more obvious than others.

1. Compile native source code into obj files
1. Compile native source code into .lib files
1. Link Native OBJ and libs into a loadable binary
1. Perform Link/Locate on loadable binary to get absolute XIP flash image
1. Sign Binary images for secured boot loader support
1. Produce MFUpdate compressed signed bundles for use with MFUpdate
1. Generate Configuration flash section image for the device's configuration
1. Compile managed code into managed assemblies
1. Generate native code skeletons from managed assemblies for Interop extensibility points
1. Generate NETMF Executable PE files, and associated debugging support files from managed
 assemblies for both big endian and little endian systems
1. Generate DAT file for built-in assemblies selected by the user to include in device flash

## Building applications for deployment to a NETMF enabled device
Application developers targeting the .NET Micro framework expect to build, deploy and debug applications pretty much exactly as they would
any other application with Visual Studio. Thus the build support for users of the "SDK" must be MSBuild based csproj or vbproj file based. 

## But wait, there's more!
There's another aspect of the system that, while technically speaking is orthogonal to the build system itself, has a significant impact on it.
That is dependency resolution. In particular for NETMF, and many types of software systems really, there are two major kinds of dependencies to resolve:

1. Hard dependencies  
These are the standard normal dependencies that build systems have handled since the very first one was built. For example, Application A
requires library xyz to link so the build must build library xyz before it can link application A. Some build systems will not even
start building A until xyz is finished. More modern systems take advantage of multi-core systems and allow building the object files for A
while building XYZ and only block the link stage of A for xyz to complete.
2. Soft, API or interface based dependencies  
These are dependencies where a given component has a dependency on a specific API or interface where there could be more than one implementation
but doesn't have any requirements on which particular implementation is used. These types of dependencies are impossible to fully resolve automatically
in all cases. In many cases, it can be determined that there is only one implementation available automatically, e.g. only one possible implementation
available for the target architecture the application is targeting, therefore the system can infer that the only one available should be the one to use.
However, if another option was added at a later time, then the system cannot know which to use. 

The problem of soft dependencies is the most difficult to solve, the current solution to this, in NETMF firmware builds is quite complex and difficult
to manage and maintain. Thus the effort to re-think the build based on everything we've learned from the existing system and that of other systems used in
other contexts. This includes re-evaluating the presumption that the build system is ultimately responsible for #2. Since soft dependencies are, in general,
impossible to resolve automatically trying to use the build system to solve this problem may be a case of putting a square peg in a round hole. Certainly, with a
big enough hammer you can, in fact get the square peg into the round hole, but it won't be very pretty! At a minimum the build must be able to indicate an 
error when it doesn't have enough information to automatically resolve the soft dependency and leave the resolution in such cases to some other means (manually
editing some build input files, using a UI tool to resolve the dependencies, etc...)

Thus, while technically separate from the build, if the build has support for automating the resolution when possible, while providing meaningful errors to the
user when it can't the build process is simpler for the user.





