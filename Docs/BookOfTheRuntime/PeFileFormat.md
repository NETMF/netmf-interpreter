# .NET Micro Framework PE file Format
The .NET Micro Framework PE data format is based on the ECMA-335 specification. Specifically sections II.22 - II.24.
Due to the constraints of the systems NETMF targets the PE file format is not an exact match/implementation of the
ECMA-335 specification. NETMF PE file format is essentially an extended subset of the format defined in ECMA-335.

## Major differences from ECMA-335

- The number and size of the metadata tables is limited in NETMF to keep the overall memory footprint as low as possible.
- Since NETMF is designed to operate without an OS the Windows PE32/COFF header, tables and information is stripped out
- Switch instruction branch table index is limited to 8 bits
- Table indexes are limited to 12 bits
  - This also means that the metadata tokens are 16 bits and not 32 so the actual IL instruction stream is different for NETMF
- Resources are handled in a very different manner with their own special table in the assembly header

## File Data Structure
The PE file starts with an [Assembly header](AssemblyHeader.md) which is the top level structure of every NETMF PE file. On disk the AssemblyHeader structure is at offset 0 of the .PE file. On the device the AssemblyHeader is aligned at a 32 bit boundary within a well known ROM/FLASH region ( the DAT region or the Deployment region) with the first assembly at offset 0 of the region. Immediately following the assembly header is the metadata table data. Since there are no fixed requirements that an assembly requires all possible tables or what the number of entries in each table will be, the exact size and location of the start of each table's data is entirely described within the header including the end of the assembly, which is used to compute the start location of any subsequent assemblies in memory or in a PE database (a.k.a. DAT region file).

## Database Files (DAT)
A Database file/region is just a concatenation of PE files where each PE file has a starting offset from the beginning that is aligned to a 32 bit boundary. When loaded into memory for the run-time the DAT file/region itself must be aligned on a 32 bit boundary. This alignment ensures that almost all of the data in the assembly is readable without resorting to special handling for unaligned data. (The exception is multi-byte data in the IL byte code stream, which may not end up aligned)

```
+-----------------+ <--- Aligned to 32 bit boundary in memory
| AssemblyHeader  |   
+-----------------+
| Metadata        |
+-----------------+
| { padding }     |
+-----------------+ <--- Aligned to 32 bit boundary in memory
| AssemblyHeader  |   
+-----------------+
| Metadata        |
+-----------------+ 
| { padding }     |
+-----------------+ <--- Aligned to 32 bit boundary in memory
|  ...            |
```
