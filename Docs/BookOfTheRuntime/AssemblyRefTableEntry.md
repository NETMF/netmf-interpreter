# AssembkyRefTableEntry (CLR_RECORD_ASSEMBLYREF)
The assembly Reference table contains references to other assemblies. The runtime will lookup the assembly by name and version
when resolving the reference to an assembly header.

The format of the AssemblyRefTableEntry is as follows:

| Name      | Type                  | Description  
|-----------|-----------------------|------------  
| Name      | StringTableIndex      | index into the string table blob for the name of the referenced assembly
| {padding} | uint16_t              | Unused padding (Must be 0)
| Version   | VersionInfo           | VersionInfo structure for the version of the assembly (Checked at runtime for an EXACT match)
