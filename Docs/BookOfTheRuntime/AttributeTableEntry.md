# AttributeTableEntry (CLR_RECORD_ATTRIBUTE)
The Attribute table contains entries with the following structure

| Name          | Type                 | Description  
|---------------|----------------------|------------  
| OwnerType     | TableKind            | One of `TableKind::TypeDef`, `TableKind::MethodDef`, or `TableKind::FieldDef`
| OwnerIndex    | uint16_t             | Index into the table specified by OwnerType
| Constructor   | MethodRefOrMethodDef | Binary token for a MethodRef or MethodDef that represents the constructor of the Attribute
| Data          | SigTableIndex        | Index into the signature table that defines the parts of the attribute

## Signature Table Usage
(TODO: Define valid signature table sequences for an attribute)