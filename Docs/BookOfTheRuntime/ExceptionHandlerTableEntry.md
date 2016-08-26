# ExceptionHandlerTableEntry (CLR_RECORD_EH)
The Exception Handler table contains entries for the exception handling blocks
within a method. If a method has the `MethodDefFlags::HasExceptionHandlers` flag
set then the last byte of the ByteCode table for the method is the number of
Exception handlers for the method. The exception handlers precede the count in
the byte code stream (e.g. with a negative offset from the size). 

**NOTE:**  
This means that instances of this structure stored in a PE
image may be stored at an address that is **NOT** correctly
aligned for the structure. Thus consumers should always copy
the data into a properly aligned buffer.

**_REVIEW:_**
In a future revision of the PE binary format this should be
managed by inserting padding into the ByteCode stream so that
copying and dealing with unaligned data isn't needed.

| Name          | Type                 | Description  
|---------------|----------------------|------------  
| Mode          | ExceptionHandlerMode | Mode for the exception handler
| ClassToken([1](#Notes)) | TypeRefOrTypeDef | Class token for the handler
| FilterStart   | MetadataOffset       | Offset into the IL ByteCode stream for the filter code
| TryStart      | MetadataOffset       | Offset into the IL ByteCode stream for the starting range this handler covers
| TryEnd        | MetadataOffset       | Offset into the IL ByteCode stream for the end of the range this handler covers
| HandlerStart  | MetadataOffset       | Offset into the IL ByteCode stream for the start of the exception handler
| HandlerEnd    | MetadataOffset       | Offset into the IL ByteCode stream for the end of the exception handler


###### Notes

1. The Mode, ClassToken and FilterStart form a discriminated union with the mode as the discriminator
and the ClassToken and FilterStart sharing the same memory location. That is the ClassToken and 
FilterStart are representable as a C/C++ anonymous union.
2. Start and End offsets are inclusive

## ExceptionHandlerMode
The exception handler mode determines the specific mode for the exception handler,
the base type for the enumeration is a uint16_t

| Name     | Value  | Description
|----------|--------|------------
| Catch    | 0x0000 | Exception handler is a catch handler for a specific type
| CatchAll | 0x0001 | Exception handler catches all exceptions, regardless of type
| Finally  | 0x0002 | Exception handler is a Finally block
| Filter   | 0x0003 | Exception handler is a filter block