# About this branch
This branch is a new clean branch for the development work on the next major release of the 
.NET Micro Framework. The next version of NETMF is starting with a clean slate and building
up from there with a completely new build infrastructure.

**Note:  
This does not mean that all of the code is being re-written from scratch. However, the build infrastructure,
folder layout, and even code formatting/style and many other aspects will change. Thus, this is starting
as a clean branch. Work will progress in a logical fashion, bringing code in from the current v4.4 branch
one piece at a time, starting with the boot loader and working up from there to a running NETMF**

## Major goals for this release
* Easier to get up and running on new hardware by leveraging existing native HAL and platform code bases
* Easier to build using improved build support with full integration into Visual Studio
* Easier to manage components used to create a working NETMF runtime
* Greater portability of apps from NETMF to UWP
  * This includes runtime support such as Generics and Base class library support.
    * It would be ideal if NETMF was either fully [.NET Standard library](https://docs.microsoft.com/en-us/dotnet/articles/standard/library) compliant or at least compliant
      with a strict subset of the Standard library.
* Everything is documented directly in the repository.
  * While SDK API documentation will still be generated from code and conceptual topic content the documentation
    for the design of the framework and tooling will start here with this topic file and grow as the system is
    brought up one piece at a time. This will help ensure that more contributors can participate in the maintenance
    and enhancement of the core framework as well as help users better understand the trade-offs that were made and
    why they were made.

See the [Docs](Docs/Index.md) for more details and to monitor progress of formalizing the plans for the next
generation of the .NET Micro Framework.

## License
The .NET Micro Framework is licensed under the Apache 2.0 license.

## .NET Foundation
The .NET Micro Framework is a [.NET Foundation](http://www.dotnetfoundation.org/projects) project.

This project has adopted the code of conduct defined by the [Contributor Covenant](http://contributor-covenant.org/)
to clarify expected behavior in our community. For more information, see the
[.NET Foundation Code of Conduct.](http://www.dotnetfoundation.org/code-of-conduct)

## Related Projects
* [Llilum](http://github.com/netmf/Llilum) an Ahead of Time compilation tool-set for .NET in micro controllers
* [CMSIS.Pack](https://github.com/NETMF/CMSIS.Pack) a .NET library for working with CMSIS-Pack packages and repositories
* [Common Project System](https://github.com/Microsoft/VSProjectSystem) Extensible project system for Visual Studio
* [Roslyn project System](https://github.com/dotnet/roslyn-project-system) Extensible project system for managed code projects
