# TypeSpecTableEntry (CLR_RECORD_TYPESPEC)
The TypeSpec table is used to describe composite types, like an array.

| Name          | Type                 | Description  
|---------------|----------------------|------------  
| Sig           | SigTableIndex        | Index into the signature table for the TypeSpec
| pad           | uint16_t             | Padding for alignment

## Signtature Table Usage
(TODO: Define valid signature table sequences for a TypeSpec)