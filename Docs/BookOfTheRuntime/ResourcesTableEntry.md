# ResourcesTableEntry (CLR_RECORD_RESOURCE)
The Resources table is used to describe the resources bound to an assembly

| Name          | Type                 | Description  
|---------------|----------------------|------------  
| Id            | uint16_t             | Id of the resource
| Kind          | ResourceKind         | Kind of the resource
| Flags         | uint8_t              | Flags for the resource
| Offset        | ResourcesDataTableIndex | Index into the ResourcesData blob table

## ResourceKind
| Name    | Value | Description
|---------|------|-----
| Invalid | 0x00 | Invalid entry
| Bitmap  | 0x01 | The resource is a Bitmap
| Font    | 0x02 | The resource is a Font in the TinyCLR Font format
| String  | 0x03 | The resource is a String
| Binary  | 0x04 | The resource is a binary blob

## Manifest Constants
| Name       | Value  | Description
|------------|--------|-----
| SentinelId | 0x7FFF | Sentinel marker (See [Notes](#Notes) for details)
| FlagsPaddingMask | 0x03 | Mask for lower two bits of flags to retrieve padding (See [Notes](#Notes) for details)

## Notes
The last entry in the Resources table will have:

| Field  | Value
|--------|-------
| Id     | SentinelId
| Kind   | Invalid
| Offset | Size of the ResourceData table
| Flags  | 0

This is used to ensure that the preceding entry can use the offset of the last entry
to compute the size of its data in the ResourceData blob table.

The lower two bits of the flags is the padding applied to align this entries data in
the ResourcesData blob table. That is, the size of the previous entries data is the
offset of this entry minus the offset of the previous entry minus the padding for this
entry. Or to put that another way, to compute the size of a resource requires a reference
to the next entry in the table. With the next entry the size is computable using the
following formula:

`sizeOfResource = next.Offset - Offset + ( next.Flags & FlagsPaddingMask )`



 

