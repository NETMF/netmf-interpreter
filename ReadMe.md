# About this branch
This branch is new clean branch for the development work on the next major release of the 
.NET Micro Framework. The next version of NETMF is starting with a clean slate and building
up from there with a completely new build infrastrucutre.

**Note:  
This does not mean that all of the code is being re-written from scratch. However, the build infrastrure,
folder layout, and even code formatting/style and many other aspects will change. Thus, this is starting
as a clean branch. Work will progress in a logical fashion, bringing code in from the current v4.4 branch
one piece at a time, starting with the boot loader and working up from there to a running NETMF**

## Major goals for this release
* Easier to get up and running on new hardware by leveraging existing native HAL and platform code bases
* Easier to build using improved build support with full integration into Visual Studio
* Easier to manage components used to create a working NETMF runtime
* Greater portability of apps from NETMF to UWP
  * This includes runtime support such as Generics and Base class library support.
    * It would be ideal if NETMF was either fully .NET Stnadard library compliant or at least compliant
      with a strict subset of the Standard library.
* Everything is documented directly in the repository.
  * While SDK API documentation will still be generated from code and conceptual topic content the documentation
    for the design of the framework and tooling will start here with this topic file and grow as the system is
    brought up one piece at a time. This will help ensure that more contributors can participate in the mainenance
    and enhancement of the core framework as well as help users better understand the tradeoffs that were made and
    why they were made.

## The Build System
The V4.x and earlier NETMF build system was based on a custom MSBuild project system that was,
in many ways transliterated from an earlier makefile system. WHile it has undergone several
refactorings and improvements since the original transliteration it is still difficult to
maintain and support as it is totally custom to NETMF and isn't supported by Visual Studio. 

The build infrastructure for the next generation of NETMF will build off of the 
[Common Project System](https://github.com/Microsoft/VSProjectSystem) (CPS) for both C++ and
.NET code bases. With the upcoming Visual Studio "15" the
[Roslyn project System](https://github.com/dotnet/roslyn-project-system) is scheduled to
replace the previous project system. The Roslyn system is based on CPS so both managed and
native code are built from a common infrastructure and extensibility model. This allows the
NETMF build support to eliminate a large part of it's onw proprietary build system, including
the replacement of "dotnetmf.proj" files with a sensibly named project that can actually load
into Visual Studio.

## The New PAL
The .NETMF HAL and PAL model was based on a world where there was little or no commonality of 
low level device code across silicon vendors. Thus the NETMF had to be in the business of defining
a HAL and PAL layer to enable running on a wide variety of hardware. The world of Micro controllers
has changed dramatically since then and it's time the Micro Framework officially acknowledged that
with a new model. The new world has a lot of common support for low level device code and even embedded
RTOS offerings that are small enough to run Micro Framework on top of. Thus the NETMF doesn't really
need to be in the business of defining the HAL layer code APIs and interfaces anymore. We still need
a level of abstraction from the hardware and thus we'll still have a PAL but it will be designed such
that it is a very thin adapter layer on top of the common code. The commonly available code could be
CMSIS, mBed OS, CMSIS++, Zephyr, silicon vendor specific libraries, or any oher commonly available
micro controller based runtime. The PAL design is a component based modular one where you will only
need to include what you need and can leave out the rest. ("Look ma, no stubs!" 8^) ). The
[New PAL](NewPal.md) topic provides more details on the design.

## License
The .NET Micro Framework is licensed under the Apache 2.0 license.

## .NET Foundation
The .NET Micro Framework is a [.NET Foundation](http://www.dotnetfoundation.org/projects) project.

This project has adopted the code of conduct defined by the [Contributor Covenant](http://contributor-covenant.org/)
to clarify expected behavior in our community. For more information, see the
[.NET Foundation Code of Conduct.](http://www.dotnetfoundation.org/code-of-conduct)

## Related Projects
* [Llilum](http://github.com/netmf/Llilum) an Ahead of Time compilation toolset for .NET in micro controllers
* [CMSIS.Pack](https://github.com/NETMF/CMSIS.Pack) a .NET library for working with CMSIS-Pack packages and repositories
* [Common Project System](https://github.com/Microsoft/VSProjectSystem) Extensible project system for Visual Studio
* [Roslyn project System](https://github.com/dotnet/roslyn-project-system) Extensible project system for managed code projects
