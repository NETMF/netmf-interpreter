# MethodRefTableEntry (CLR_RECORD_METHODREF)
The MethodRef table contains entries with the following structure

| Name      | Type                 | Description  
|-----------|----------------------|------------  
| name      | StringTableIndex     | Index into [string table](StringTable.md) for the name of the method |
| container | TypeRefTableIndex    | Index into [TypeRef table](TypeRefTableEntry.md) for the type containing the method |
| sig       | SignatureTableIndex  | Index into [signature table](SignatureTable.md) for signature of the method |
| pad       | uint16_t             | padding for alignment requirements |