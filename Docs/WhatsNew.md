## Major Features
The following high level features are the primary targets for the next generation of the .NET Micro Framework

### The Build System
The V4.x and earlier NETMF build system was based on a custom MSBuild project system that was,
in many ways transliterated from an earlier makefile system. WHile it has undergone several
re-factorings and improvements since the original transliteration it is still difficult to
maintain and support as it is totally custom to NETMF and isn't supported by Visual Studio. 

The build infrastructure for the next generation of NETMF will build off of the 
[Common Project System](https://github.com/Microsoft/VSProjectSystem) (CPS) for both C++ and
.NET code bases. With the upcoming Visual Studio "15" the
[Roslyn project System](https://github.com/dotnet/roslyn-project-system) is scheduled to
replace the previous managed code project system. The Roslyn system is based on CPS so both managed and
native code are built from a common infrastructure and extensibility model. This allows the
NETMF build support to eliminate a large part of it's own proprietary build system, including
the replacement of "dotnetmf.proj" files with a sensibly named project that can actually load
into Visual Studio.

### The New PAL
The .NETMF HAL and PAL model was based on a world where there was little or no commonality of 
low level device code across silicon vendors. Thus, NETMF had to be in the business of defining
a HAL and PAL layer to enable running on a wide variety of hardware. The world of micro-controllers
has changed dramatically since then and it's time the Micro Framework officially acknowledged that
with a new model. The new world has a lot of common support for low level device code and even embedded
RTOS offerings that are small enough to run Micro Framework on top of. Thus, the NETMF doesn't really
need to be in the business of defining the HAL layer code APIs and interfaces anymore.

We still need a level of abstraction from the hardware and thus we'll still have a PAL but it will be designed such
that it is a very thin adapter layer on top of the common code. The commonly available code could be
CMSIS, mBed OS, CMSIS++, Zephyr, silicon vendor specific libraries, or any other commonly available
micro controller based runtime. The PAL design is a component based modular one where you will only
need to include what you need and can leave out the rest. ("Look ma, no stubs!" 8^) ). The
[New PAL](NewPal.md) topic provides more details on the design.
