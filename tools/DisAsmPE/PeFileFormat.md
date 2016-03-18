### .NET Micro Framework PE file Format
The .NET Micro Framework PE data format is based on the ECMA-335 specification. Specifically sections II.22 - II.24.
Due to the constraints of the systems NETMF targets the PE file format is not an exact match/implementation of the
ECMA-335 specification. NETMF PE file format is essentially an extended subset of the format defined in ECMA-335.

#### Major differences from ECMA-335

- The number and size of the metadata tables is limited in NETMF to keep the overall memory footprint as low as possible.
- Since NETMF is designed to operate without an OS the Windows PE32/COFF header, tables and information is stripped out
- Table indexes are limited to 12 bits
  - This also means that the metadata tokens are 16 bits and not 32 so the actual IL instruction stream is different for NETMF
- Resources are handled in a very different manner with their own special table in the assembly header

#### Assembly Header
The Assembly header is the top level structure of every NETMF PE file. On disk the AssemblyHeader structure is at offset 0 of
the .PE file. On the device the AssemblyHeader is aligned at a 32 bit boundary within a well known FLASH region ( the DAT region
or the Deployment region) with the first assembly at offset 0 of the region. 

##### AssemblyHeader strucutre
The AssemblyHeader structure contains a number of verification markers and CRCs to validate the legitimacy of the assembly at
runtime. Additionally, the Assembly header contains the location information for the MetadataTables and BLOB storage areas. 

The Structure of the AssemblyHeader is as follows:

| Name                                            | Type                  | Description  
|-------------------------------------------------|-----------------------|------------  
| [Marker](#marker)                               | `uint8_t[8]`          | Id marker for an assembly ('MSSpot1')  
| [HeaderCRC](#headercrc)                         | `uint32_t`            | CRC32 of the AssemblyHeader structure itself
| [AssemblyCRC](#assemblycrc)                     | `uint32_t`            | CRC32 of the complete assembly
| [Flags](#flags)                                 | `AssemblyHeaderFlags` | Flags for the assembly
| [NativeMethodsChecksum](#nativemethodschecksum) | `uint32_t`            | Native Method Checksum
| [PatchEntryOffset](#patchentryoffset)           | `uint32_t`            | Offset into ResourceData table of patch native method
| [Version](#version)                             | `VersionInfo`         | Version information data structure for this assembly
| [AssemblyName](#assemblyname)                   | `uint16_t`            | String table index for the Assembly's name
| [StartOfTables](#startoftables)                 | `uint32_t[16]`        | Array of offsets into the PE file for the metadata tables
| NumPatchedMethods                               | `uint32_t`            | (Deprecated) No longer used
| [PaddingOfTables](#paddingoftables)             | `uint8_t[16]`         | amount of alignment padding for each metadata table

##### Field Details
The following sections describe the individual fields of the AssemblyHeader structure.

###### Marker
The assembly marker is an eight character marker consisting of the  non zero terminated string 'MSSpot1' in ASCII encoded characters.
This is used to clearly identify a .NET Micro Framework PE file on disk and in memory at runtime. It also indicates the version of
this data structure, thus any modifications to this structure in future releases **MUST** use a new marker string.

###### HeaderCRC
ANSI X3.66 32 bit CRC for the AssemblyHeader. This is computed assuming the HeaderCRC and AssemblyCRC fields are 0.

###### AssemblyCRC
ANSI X3.66 32 bit CRC for the entire contents of the Assembly PE data. This is computed assuming the HeaderCRC and AssemblyCRC fields
are 0.

###### Flags
The flags property contains a bit flags value indicating core information about the assembly the possible flags are:  

| Name       | value      | Description
|------------|:----------:|------------
| None       | 0x00000000 | No special flags
| NeedReboot | 0x00000001 | Indicates the CLR must be rebooted before this assembly can be loaded
| Patch      | 0x00000002 | (Deprecated) Indicates the assembly is a patch assembly
| BigEndian  | 0x80000080 | Flag to indicate the assembly is in big-endian format

***NeedReboot*** has dubious value and should be considered deprecated. It indicates to the CLR that the system must be rebooted
before the assembly can be loaded. Presumably the reboot process would either clear the flag or replace the assembly as it would
otherwise not be able to load the assembly after the reboot.

The ***Patch*** flag is not currently used (Technically it is used but the remaining code that does use it is effectively a NOP)
and should be considered deprecated. 

The ***BigEndian*** flag indicates if the assembly is stored in big-endian mode. This value was deliberately chosen such that it reads
the same from both little-endian and big-endian readers. Thus any reader reading the assembly can easily determine the endianess of
the assembly data. This should also apply to all of the data stored in the Resources for the assembly. NETMF tools explicitly define
and use two types of binary data structures (Bitmaps and fonts) so the tools automatically handle converting the binary data of those
structures. Any custom binary resources must account for the endianess conversions either at build time or at runtime in the code that
uses the resources.

###### NativeMethodsChecksum
The ***NativeMethodsChecksum*** is a unique value that is matched against the native methods table stored in the CLR firmware to
ensure the methods match. The actual algorithm used for computing this checksum are documented in the [NativeMethodsChecksum Algorithm]
document. Though, it worth noting that the actual algorithm doesn't matter. Nothing in the runtime will compute this value. The
runtime only compares the assembly's value with the one for the native code registered for a given assembly to ensure they match.
As long as the tool generating the assembly and the native method stubs header and code files use the same value then the actual
algorithm is mostly irrelevant. The most important aspect of the algorithm chosen is that any change to any type or method signature
of any type with native methods **MUST** generate a distinct checksum value. The current MetadataProcessor algorithm constructs a
mangled string name for the native methods (used to generate the stubs), sorts them all and runs a CRC32 across them to get a distinct
value. Since the CRC is based on the fully qualified method name and the types of all parameters any change of the signatures will
generate a new value - denoting a mismatch.

###### PatchEntryOffset
The ***PatchEntryOffset*** field is an offset into the assemblies ResourceData blob table where native "patch" code exists. At runtime
if this is not 0xFFFFFFFF then the CLR will compute a physical address of the start of the patch code and call the function located
there. The function must be position independent and must have the following signature `void PatchEntry()` This is a very limited
mechanism at present and ultimately requires deep knowledge of the underlying platform HAL/PAL etc... to be of any real use. Of
special importance is the location of the assembly in physical memory as many micro controllers limit the memory addresses where
executable code can reside. (i.e. internal flash or RAM only ) thus, this is not a generalized extensibility/dynamically loaded
native code mechanism.

###### Version
The ***Version*** field holds the assembly's version number. (as opposed to the version of the AssemblyHeaderStructure itself). This
is used by the debugger for version checks at deployment time. The runtime itself doesn't use versions to resolve references, as only
one version of an assembly can be loaded at a time. Thus assembly references in the PE format don't include a version.

###### AssemblyName
[String Table](StringTable.md) index for the name of the assembly

###### StartOfTables
Fixed array of offsets to the table data for each of the 16 different tables. The entries in this array are offsets from the start
of the assembly header itself (e.g. the file seek offset if the PE image is from a file)

###### PaddingOfTables
todo: blah blah