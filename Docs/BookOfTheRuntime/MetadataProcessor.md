#MetadataProcessor
The MetadataProcessor has multiple roles:

1. Convert the ECMA specified CIL metadata format into a form that is more compact and suitable for direct execution
2. Generating interop code template for native calls
3. Packing together multiple assemblies
4. Computing a delta between assemblies
5. Handling of localization for resources
6. Singing and key generation operations
7. Other miscellaneous tasks...

The size of that list means MetadataProcessor id a central element of the build process for .NET Micro Framework making it a bit of a [Swiss Army Knife](https://en.wikipedia.org/wiki/Swiss_Army_knife) of a tool. (Future work may consider splitting this out to multiple individual tools for easier maintenance and flexibility)

## Command Line Options
#### -loadHints **\<assembly> \<file>**
Loads a specific file as a dependency  
**\<assembly\>** = Name of the assembly to process  
**\<file>** = File path for the assembly

#### -ignoreAssembly **\<assembly>**
Do not include an assembly in the dependencies  
**\<assembly>** = Assembly to ignore

#### -parse **\<file>**
Parse and analyzes a .NET assembly stored as metadata in a Windows PE/COFF binary

#### -cfg **\<file>**
Loads command line options from a file (when command line lengths are limited or easier to read logging is desired)

#### -noBitmapCompression
Turns off compression of bitmaps

#### -verboseMinimize
Turns on verbose level for the minimization phase

#### -noByteCode
Skips any ByteCode present in the assembly

#### -noAttributes
Skips any attribute in the assembly

#### -excludeClassByName **\<class>**
Removes the class from an assembly (e.g. filters a specific class out of the generated NETMF PE file)

#### -minimize
Minimizes the assembly removing unwanted elements

#### -compile **\<file>**
Compiles a .NET Assembly file into the NETMF PE format

#### -load **\<file>**
Loads a NETMF PE format assembly

#### -loadDatabase **\<file>**
Loads a set of assemblies packed into a single image file (created with the [-createDatabase](#-create_database-config-file) option)

#### -dump_all **\<file>**
Generate a report of an assembly's metadata

#### -dump_exports **\<file>**
Generates a report of an assembly's metadata in a more readable format

#### -refresh_assembly **\<name>** **\<output>**
Recomputes the CRCs for an assembly

#### -resolve
Tries to resolve cross-assembly references

#### -generate_dependency **\<file>**
Generates an XML file with the relationship between assemblies

#### -create_database **\<config>** **\<file>**
Creates a bundle of NETMF formatted PE assemblies suitable for placement in ROM/FLASH (a.k.a. a DAT file)  
**\<config>** = file containing list of assemblies to include in the bundle  
**\<file>** = Output file for the database  

## Interop Skeleton Code Generation
#### -generate_skeleton **\<file>** **\<name>** **\<project>**
Generates a skeleton for the methods implemented in native code  
**\<file>** = Prefix name for the files  
**\<name>** = Name of the assembly  
**\<project>** = Identifier for the library  

## String Table Options
#### -saveStrings **\<file>**
Saves strings table to a file

#### -loadStrings **\<file>**
Loads strings table from a file

#### -generateStringsTable **\<file>**
Writes the collected database of strings to a file

## Localization Options
#### -exportLoc **\<file>**
#### -importLoc **\<file>**
#### -importLanguages **\<langs>**

## Obsoleted Commands **(Do not use)**
The following command line options to MetadataProcessor are considered obsolete and
should not be used as they will likely be removed when MetadataProcessor is split to
multiple tools.
#### -patchReboot
#### -patchNative **\<file>**
#### -diff **\<oldAssembly> \<newAssembly> \<patch>**
#### -diffHex **\<oldAssembly> \<newAssembly> \<patch>**

# Parsing
The parsing process is composed by a set of operations responsible for converting an
assembly from the normal CLR format to the TinyCLR one. Most of the work is done by
the class `MetaData::Parser`, with the help of the `IMetaData*` COM objects present
in the CLR runtime. `MetaData::Parser::Analyze` is the main import method. It loads
an assembly, enumerates all its tables (AssemblyRef, ModuleRef, TypeRef, MemberRef,
TypeDef, TypeField, TypeMethod, TypeInterface, TypeSpecs, UserStrings, Resources,
CustomAttributes), and creates an in-memory representation of the metadata.
`MetaData::Parser::RemoveUnused` and `MetaData::Parser::VerifyConsistency` are used
to filter out all the unnecessary data and ensure the set of features required by the
assembly are actually supported by TinyCLR. Once the metadata has been parsed and
validated, the system uses another class, `WatchAssemblyBuilder::Linker`, to generate 
the metadata in the TinyCLR format. The job of the Linker class is to encode the
metadata in the right format, validating that the various fields are not exceeding
the allowed ranges. The method `WatchAssemblyBuilder::Linker::Process` is used to
import the metadata from the Parser object into a Linker object. Then the method
`WatchAssemblyBuilder::Linker::Generate` deals with the actual encoding.
